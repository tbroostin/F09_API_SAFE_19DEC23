/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.FinancialAid.Transactions;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class StudentBudgetComponentRepositoryTests : BaseRepositorySetup
    {
        public string studentId;

        public TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;
        public TestStudentAwardYearRepository testStudentAwardYearRepository;
        public List<StudentAwardYear> inputStudentAwardYears
        {
            get
            {
                return testStudentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(testFinancialAidOfficeRepository.GetFinancialAidOffices())).ToList();
            }
        }

        public TestStudentBudgetComponentRepository expectedRepository;
        public IEnumerable<StudentBudgetComponent> expectedStudentBudgetComponents
        {
            get
            {
                return expectedRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears).Result;
            }
        }

        public StudentBudgetComponentRepository actualRepository;
        public IEnumerable<StudentBudgetComponent> actualStudentBudgetComponents;
        
        //create base class initializer here if needed

        //helper to build repository
        private StudentBudgetComponentRepository BuildStudentBudgetComponentRepository()
        {
            transManagerMock.Setup(tm => tm.ExecuteAsync<GetStuBudgetComponentsRequest, GetStuBudgetComponentsResponse>(It.IsAny<GetStuBudgetComponentsRequest>()))
                .Returns<GetStuBudgetComponentsRequest>((req) =>
                {
                    var resp = expectedRepository.responses.FirstOrDefault(r => r.studentId == req.StudentId && r.year == req.Year);
                    if (resp != null)
                    {
                        return Task.FromResult(new GetStuBudgetComponentsResponse()
                        {
                            StudentBudgetComponents = resp.StudentBudgetComponents,
                            StuBgtComponentOrigAmts = resp.StuBgtComponentOrigAmts,
                            StuBgtComponentOvrAmts = resp.StuBgtComponentOvrAmts
                        });
                    }
                    return null;
                });

            return new StudentBudgetComponentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetStudentBudgetComponentsAsync : StudentBudgetComponentRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();
                expectedRepository = new TestStudentBudgetComponentRepository();

                studentId = "0003914";

                actualRepository = BuildStudentBudgetComponentRepository();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);
                CollectionAssert.AreEqual(expectedStudentBudgetComponents.ToList(), actualStudentBudgetComponents.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = null;
                await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);
            }

            [TestMethod]
            public async Task GetStuBudgetComponentsRequestTransactionFails_LogsMessageTest()
            {
                transManagerMock.Setup(tm => tm.ExecuteAsync<GetStuBudgetComponentsRequest, GetStuBudgetComponentsResponse>(It.IsAny<GetStuBudgetComponentsRequest>())).Throws(new Exception());
                actualRepository = new StudentBudgetComponentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), string.Format("Could not retrieve budget components for student {0}, award year {1}", studentId, inputStudentAwardYears.First().Code)));
            }

            [TestMethod]
            public async Task NullStudentAwardYearsLogsMessageReturnsEmptyListTest()
            {
                var budgets = await actualRepository.GetStudentBudgetComponentsAsync(studentId, null);
                Assert.AreEqual(0, budgets.Count());

                loggerMock.Verify(l => l.Info(string.Format("Cannot get budget components for student {0} with no studentAwardYears", studentId)));
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearsLogsMessageReturnsEmptyListTest()
            {
                testStudentAwardYearRepository.FaStudentData.FaCsYears = new List<string>();
                testStudentAwardYearRepository.FaStudentData.FaSaYears = new List<string>();
                testStudentAwardYearRepository.FaStudentData.FaYsYears = new List<string>();
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                Assert.AreEqual(0, actualStudentBudgetComponents.Count());

                loggerMock.Verify(l => l.Info(string.Format("Cannot get budget components for student {0} with no studentAwardYears", studentId)));
            }

            [TestMethod]
            public async Task NoStudentBudgetsForYearWithNoBudgetComponentsYearTest()
            {
                var bogusYear = "foobar";
                testStudentAwardYearRepository.FaStudentData.FaCsYears.Add(bogusYear);
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                Assert.IsNull(actualStudentBudgetComponents.FirstOrDefault(c => c.AwardYear == bogusYear));
            }

            [TestMethod]
            public async Task NoStudentBudgetsForYearWithNullBudgetsTest()
            {
                var testRecord = expectedRepository.responses.First();
                testRecord.StudentBudgetComponents = null;
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                Assert.IsNull(actualStudentBudgetComponents.FirstOrDefault(c => c.AwardYear == testRecord.year));
            }

            [TestMethod]
            public async Task NullOriginalAmountTranslatedToZeroTest()
            {
                expectedRepository.responses.ForEach(resp => resp.StuBgtComponentOrigAmts = new List<string>() { });
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                Assert.IsTrue(actualStudentBudgetComponents.All(sbc => sbc.CampusBasedOriginalAmount == 0));
            }

            [TestMethod]
            public async Task CorruptRecord_CatchExceptionLogErrorTest()
            {
                var testRecord = expectedRepository.responses.First(r => (inputStudentAwardYears.Select(y => y.Code)).Contains(r.year));
                testRecord.StudentBudgetComponents[0] = string.Empty;

                var budgets = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                var message = string.Format("Unable to create budget component code {0} for student {1}, award year {2}", testRecord.StudentBudgetComponents[0], studentId, testRecord.year);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), message));
            }

            [TestMethod]
            public async Task NullOverwriteAmounts_BudgetComponentsReturnedTest()
            {
                expectedRepository.responses.ForEach(resp => resp.StuBgtComponentOvrAmts = new List<string>() { });
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);
                Assert.IsTrue(actualStudentBudgetComponents.Any());
            }
            
        }

    }
}
