/*Copyright 2014-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class StudentLoanLimitationRepositoryTests
    {
        [TestClass]
        public class GetStudentLoanLimitationsTests : BaseRepositorySetup
        {

            public class GetLoanLimitationsResponseTest : GetLoanLimitationsResponse
            {
                public string AwardYear;
            }

            private string studentId;

            private TestStudentAwardYearRepository studentAwardYearRepository;
            private TestFinancialAidOfficeRepository financialAidOfficeRepository;
            private TestStudentAwardRepository studentAwardRepository;
            private CurrentOfficeService currentOfficeService;
            private IEnumerable<StudentAwardYear> inputStudentAwardYears;

            private List<GetLoanLimitationsResponseTest> getLoanLimitationsResponseTransactionTestList;

            private IEnumerable<StudentLoanLimitation> expectedLimitations;
            private IEnumerable<StudentLoanLimitation> actualLimitations;

            private TestStudentLoanLimitationRepository expectedRepository;
            private StudentLoanLimitationRepository actualRepository;
            private List<GetLoanLimitationsRequest> actualGetLoanLimitationsRequestTransactionList;

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                studentId = "0003914";
                studentAwardYearRepository = new TestStudentAwardYearRepository();
                financialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                studentAwardRepository = new TestStudentAwardRepository();
                currentOfficeService = new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices());
                inputStudentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                expectedRepository = new TestStudentLoanLimitationRepository();
                expectedLimitations = expectedRepository.GetStudentLoanLimitationsAsync(studentId, inputStudentAwardYears).Result;

                getLoanLimitationsResponseTransactionTestList = BuildLoanLimitResponseTransactionTestList(expectedRepository.testLoanLimitsData);
                actualGetLoanLimitationsRequestTransactionList = new List<GetLoanLimitationsRequest>();

                actualRepository = BuildRepository();
                actualLimitations = await actualRepository.GetStudentLoanLimitationsAsync(studentId, inputStudentAwardYears);
            }

            private StudentLoanLimitationRepository BuildRepository()
            {
                transManagerMock.Setup(m =>
                    m.ExecuteAsync<GetLoanLimitationsRequest, GetLoanLimitationsResponse>(It.IsAny<GetLoanLimitationsRequest>()))
                    .Returns<GetLoanLimitationsRequest>(
                        req =>
                        {
                            var transactionTest = getLoanLimitationsResponseTransactionTestList.FirstOrDefault(t => t != null && t.AwardYear == req.Year);
                            if (transactionTest == null) return null;
                            return Task.FromResult(new GetLoanLimitationsResponse()
                            {
                                SubEligAmount = transactionTest.SubEligAmount,
                                UnsubEligAmount = transactionTest.UnsubEligAmount,
                                GplusEligAmount = transactionTest.GplusEligAmount
                            });
                        }
                    ).Callback<GetLoanLimitationsRequest>(req => actualGetLoanLimitationsRequestTransactionList.Add(req));

                dataReaderMock
                .Setup<Task<SaAcyr>>(reader => reader.ReadRecordAsync<SaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>(
                    (acyrFileName, key, convertText) =>
                    {
                        return Task.FromResult(studentAwardRepository.awardData
                            .Where(sa =>
                                sa.awardYear == acyrFileName.Split('.')[1] //e.g. SA.2013
                            ).Select(sa =>
                                new SaAcyr()
                                {
                                    Recordkey = studentId,
                                    SaOverLoanMax = sa.supressLoanMax
                                }
                            ).FirstOrDefault());
                    }
                );

                return new StudentLoanLimitationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            private List<GetLoanLimitationsResponseTest> BuildLoanLimitResponseTransactionTestList(List<TestStudentLoanLimitationRepository.TestLoanLimitation> testLoanLimitRecords)
            {
                var responseList = new List<GetLoanLimitationsResponseTest>();
                foreach (var record in testLoanLimitRecords)
                {
                    responseList.Add(new GetLoanLimitationsResponseTest()
                        {
                            AwardYear = record.AwardYear,
                            SubEligAmount = record.SubMaxAmount,
                            UnsubEligAmount = record.UnsubMaxAmount,
                            GplusEligAmount = record.GradPlusMaxAmount
                        });
                }
                return responseList;
            }

            [TestMethod]
            public void LoanLimitationAttributesTest()
            {
                Assert.IsNotNull(expectedLimitations);
                Assert.IsNotNull(actualLimitations);

                Assert.IsTrue(expectedLimitations.Count() > 0);
                Assert.IsTrue(actualLimitations.Count() > 0);

                Assert.AreEqual(expectedLimitations.Count(), actualLimitations.Count());

                foreach (var expectedLimitation in expectedLimitations)
                {
                    var actualLimitation = actualLimitations.First(l => l.AwardYear == expectedLimitation.AwardYear);

                    Assert.AreEqual(expectedLimitation.GradPlusMaximumAmount, actualLimitation.GradPlusMaximumAmount);
                    Assert.AreEqual(expectedLimitation.StudentId, actualLimitation.StudentId);
                    Assert.AreEqual(expectedLimitation.SubsidizedMaximumAmount, actualLimitation.SubsidizedMaximumAmount);
                    Assert.AreEqual(expectedLimitation.UnsubsidizedMaximumAmount, actualLimitation.UnsubsidizedMaximumAmount);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentIdThrowsExceptionTest()
            {
                await actualRepository.GetStudentLoanLimitationsAsync(string.Empty, inputStudentAwardYears);
            }

            [TestMethod]
            public async Task NullInputStudentAwardYears_ReturnsEmptyListTest()
            {
                actualLimitations = await actualRepository.GetStudentLoanLimitationsAsync(studentId, null);
                Assert.AreEqual(0, actualLimitations.Count());
            }

            [TestMethod]
            public async Task EmptyInputStudentAwardYears_ReturnsEmptyListTest()
            {
                actualLimitations = await actualRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>());
                Assert.AreEqual(0, actualLimitations.Count());
            }

            [TestMethod]
            public void NumLimitationObjectsEqualNumStudentAwardYearObjects()
            {
                Assert.AreEqual(inputStudentAwardYears.Count(), actualLimitations.Count());
            }

            [TestMethod]
            public void ActualRequestTransactionsTest()
            {
                Assert.IsTrue(actualGetLoanLimitationsRequestTransactionList.Count() > 0);
                Assert.AreEqual(inputStudentAwardYears.Count(), actualGetLoanLimitationsRequestTransactionList.Count());

                foreach (var studentAwardYear in inputStudentAwardYears)
                {
                    var actualRequestTransaction = actualGetLoanLimitationsRequestTransactionList.First(l => l.Year == studentAwardYear.Code);
                    Assert.AreEqual(studentId, actualRequestTransaction.StudentId);
                    Assert.AreEqual(studentAwardYear.CurrentConfiguration.AllowNegativeUnmetNeedBorrowing, actualRequestTransaction.AllowUnmetNeedBorrowing);
                }
            }

            [TestMethod]
            public async Task ResponseTransaction_SubEligAmountTest()
            {
                int testAmount = 12345;
                var testTransaction = getLoanLimitationsResponseTransactionTestList.First();
                testTransaction.SubEligAmount = testAmount;

                actualRepository = BuildRepository();
                var actualLimitation = (await actualRepository.GetStudentLoanLimitationsAsync(studentId, inputStudentAwardYears)).First(l => l.AwardYear == testTransaction.AwardYear);

                Assert.AreEqual(testAmount, actualLimitation.SubsidizedMaximumAmount);
            }

            [TestMethod]
            public async Task ResponseTransaction_NullSubEligAmountTest()
            {
                var testTransaction = getLoanLimitationsResponseTransactionTestList.First();
                testTransaction.SubEligAmount = null;

                actualRepository = BuildRepository();
                var actualLimitation = (await actualRepository.GetStudentLoanLimitationsAsync(studentId, inputStudentAwardYears)).First(l => l.AwardYear == testTransaction.AwardYear);

                Assert.AreEqual(0, actualLimitation.SubsidizedMaximumAmount);
            }

            //
            [TestMethod]
            public async Task ResponseTransaction_UnsubEligAmountTest()
            {
                int testAmount = 12345;
                var testTransaction = getLoanLimitationsResponseTransactionTestList.First();
                testTransaction.UnsubEligAmount = testAmount;

                actualRepository = BuildRepository();
                var actualLimitation = (await actualRepository.GetStudentLoanLimitationsAsync(studentId, inputStudentAwardYears)).First(l => l.AwardYear == testTransaction.AwardYear);

                Assert.AreEqual(testAmount, actualLimitation.UnsubsidizedMaximumAmount);
            }

            [TestMethod]
            public async Task ResponseTransaction_NullUnsubEligAmountTest()
            {
                var testTransaction = getLoanLimitationsResponseTransactionTestList.First();
                testTransaction.UnsubEligAmount = null;

                actualRepository = BuildRepository();
                var actualLimitation = (await actualRepository.GetStudentLoanLimitationsAsync(studentId, inputStudentAwardYears)).First(l => l.AwardYear == testTransaction.AwardYear);

                Assert.AreEqual(0, actualLimitation.UnsubsidizedMaximumAmount);
            }

            //
            [TestMethod]
            public async Task ResponseTransaction_GradPlusEligAmountTest()
            {
                int testAmount = 12345;
                var testTransaction = getLoanLimitationsResponseTransactionTestList.First();
                testTransaction.GplusEligAmount = testAmount;

                actualRepository = BuildRepository();
                var actualLimitation = (await actualRepository.GetStudentLoanLimitationsAsync(studentId, inputStudentAwardYears)).First(l => l.AwardYear == testTransaction.AwardYear);

                Assert.AreEqual(testAmount, actualLimitation.GradPlusMaximumAmount);
            }

            [TestMethod]
            public async Task ResponseTransaction_NullGradPlusEligAmountTest()
            {
                var testTransaction = getLoanLimitationsResponseTransactionTestList.First();
                testTransaction.GplusEligAmount = null;

                actualRepository = BuildRepository();
                var actualLimitation = (await actualRepository.GetStudentLoanLimitationsAsync(studentId, inputStudentAwardYears)).First(l => l.AwardYear == testTransaction.AwardYear);

                Assert.AreEqual(0, actualLimitation.GradPlusMaximumAmount);
            }

            [TestMethod]
            public async Task SuppressStudentMaximumAmounts_DataContractValueEmpty_ReturnsFalseTest()
            {
                var firstRecord = studentAwardRepository.awardData.First();
                firstRecord.supressLoanMax = string.Empty;

                actualRepository = BuildRepository();
                var actualLimitation = await actualRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { new StudentAwardYear(studentId, firstRecord.awardYear) });

                Assert.IsFalse(actualLimitation.First().SuppressStudentMaximumAmounts);
            }

            [TestMethod]
            public async Task SuppressStudentMaximumAmounts_DataContractValueNull_ReturnsFalseTest()
            {
                var firstRecord = studentAwardRepository.awardData.First();
                firstRecord.supressLoanMax = null;

                actualRepository = BuildRepository();
                var actualLimitation = await actualRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { new StudentAwardYear(studentId, firstRecord.awardYear) });

                Assert.IsFalse(actualLimitation.First().SuppressStudentMaximumAmounts);
            }

            [TestMethod]
            public async Task SuppressStudentMaximumAmounts_DataContractValueUnknownValue_ReturnsFalseTest()
            {
                var firstRecord = studentAwardRepository.awardData.First();
                firstRecord.supressLoanMax = "X";

                actualRepository = BuildRepository();
                var actualLimitation = await actualRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { new StudentAwardYear(studentId, firstRecord.awardYear) });

                Assert.IsFalse(actualLimitation.First().SuppressStudentMaximumAmounts);
            }

            [TestMethod]
            public async Task SuppressStudentMaximumAmounts_DataContractValueN_ReturnsFalseTest()
            {
                var firstRecord = studentAwardRepository.awardData.First();
                firstRecord.supressLoanMax = "N";

                actualRepository = BuildRepository();
                var actualLimitation = await actualRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { new StudentAwardYear(studentId, firstRecord.awardYear) });

                Assert.IsFalse(actualLimitation.First().SuppressStudentMaximumAmounts);
            }

            [TestMethod]
            public async Task SuppressStudentMaximumAmounts_DataContractValueY_ReturnsTrueTest()
            {
                var firstRecord = studentAwardRepository.awardData.First();
                firstRecord.supressLoanMax = "Y";

                actualRepository = BuildRepository();
                var actualLimitation = await actualRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { new StudentAwardYear(studentId, firstRecord.awardYear) });

                Assert.IsTrue(actualLimitation.First().SuppressStudentMaximumAmounts);
            }

        }
    }
}
