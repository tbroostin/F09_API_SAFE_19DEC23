/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

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
            dataReaderMock.Setup(d => d.ReadRecordAsync<CsAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns<string, string, bool>((acyrFile, studentId, b) =>
                {
                    var csRecord = expectedRepository.csStudentRecords.FirstOrDefault(cs => cs.awardYear == acyrFile.Split('.')[1]);
                    return Task.FromResult((csRecord == null) ? null :
                        new CsAcyr()
                        {
                            Recordkey = studentId,
                            CsCompEntityAssociation = (csRecord.budgetComponents == null) ? null :
                                csRecord.budgetComponents.Select(budgetComponent =>
                                    new CsAcyrCsComp()
                                    {
                                        CsCompIdAssocMember = budgetComponent.budgetComponentCode,
                                        CsCompCbOrigAmtAssocMember = budgetComponent.campusBasedOriginalAmount,
                                        CsCompCbOvrAmtAssocMember = budgetComponent.campusBasedOverrideAmount
                                    }).ToList()
                        });
                }
                );

            return new StudentBudgetComponentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetStudentBudgetComponents : StudentBudgetComponentRepositoryTests
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
            public async Task NoStudentBudgetsForYearWithNullCsRecordTest()
            {
                var bogusYear = "foobar";
                testStudentAwardYearRepository.FaStudentData.FaCsYears.Add(bogusYear);
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                Assert.IsNull(actualStudentBudgetComponents.FirstOrDefault(c => c.AwardYear == bogusYear));
            }

            [TestMethod]
            public async Task NoStudentBudgetsForYearWithNullBudgetsOnCsRecordTest()
            {
                var testRecord = expectedRepository.csStudentRecords.First();
                testRecord.budgetComponents = null;
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                Assert.IsNull(actualStudentBudgetComponents.FirstOrDefault(c => c.AwardYear == testRecord.awardYear));
            }

            [TestMethod]
            public async Task NullOriginalAmountTranslatedToZeroTest()
            {
                expectedRepository.csStudentRecords.ForEach(cs =>
                    cs.budgetComponents.ForEach(b => b.campusBasedOriginalAmount = null));
                actualStudentBudgetComponents = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                Assert.IsTrue(actualStudentBudgetComponents.All(sbc => sbc.CampusBasedOriginalAmount == 0));
            }

            [TestMethod]
            public async Task CorruptRecord_CatchExceptionLogErrorTest()
            {
                var testRecord = expectedRepository.csStudentRecords.First();
                testRecord.budgetComponents.First().budgetComponentCode = string.Empty;

                var budgets = await actualRepository.GetStudentBudgetComponentsAsync(studentId, inputStudentAwardYears);

                var message = string.Format("Unable to create budget component code {0} for student {1}, award year {2}", string.Empty, studentId, testRecord.awardYear);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), message));
            }
        }

    }
}
