/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class AwardPackageChangeRequestRepositoryTests : BaseRepositorySetup
    {
        public string studentId;

        public TestAwardPackageChangeRequestRepository expectedRepository;
        public List<AwardPackageChangeRequest> expectedChangeRequests
        {
            get { return expectedRepository.GetAwardPackageChangeRequestsAsync(studentId).Result.ToList(); }
        }

        public AwardPackageChangeRequestRepository actualRepository;

        public IEnumerable<AwardPackageChangeRequest> actualChangeRequests;
        
        public CreateAmountChangeRequestRequest actualCreateAmountChangeRequestTransaction;
        public CreateStatusChangeRequestRequest actualCreateStatusChangeRequestTransaction;

        public void AwardPackageChangeRequestRepositoryTestsInitialize()
        {
            MockInitialize();
            studentId = "0003914";
            expectedRepository = new TestAwardPackageChangeRequestRepository();
            BuildAwardPackageChangeRequestRepository();
        }

        private void BuildAwardPackageChangeRequestRepository()
        {
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<LoanChgRequest>(It.IsAny<string>(), true))
                .Returns<string, bool>((c, b) => Task.FromResult(new Collection<LoanChgRequest>(
                    expectedRepository.LoanAmountChangeRequestData.Where(d => d.studentId == studentId).Select(record =>
                        new LoanChgRequest()
                        {
                            Recordkey = record.id,
                            LncrStudentId = record.studentId,
                            LncrAwardYear = record.awardYear,
                            LncrAward = record.awardId,
                            LncrCurrentStatus = record.statusCode,
                            LoanChgRequestAdddate = record.AddDate,
                            LoanChgRequestAddtime = record.AddTime,
                            LncrAssignedToId = record.counselorId,
                            LncrChangedInfoEntityAssociation = record.periodChangeRequests.Select(period =>
                                new LoanChgRequestLncrChangedInfo()
                                {
                                    LncrAwardPeriodsAssocMember = period.awardPeriodId,
                                    LncrNewLoanActionsAssocMember = period.newStatusId,
                                    LncrNewLoanAmountsAssocMember = period.newAmount
                                }).ToList()
                        }).ToList())));

            dataReaderMock.Setup(r => r.BulkReadRecordAsync<DeclAwdRequest>(It.IsAny<string>(), true))
                .Returns<string, bool>((c, b) => Task.FromResult(new Collection<DeclAwdRequest>(
                    expectedRepository.DeclinedStatusChangeRequestData.Where(d => d.studentId == studentId).Select(record =>
                        new DeclAwdRequest()
                        {
                            Recordkey = record.id,
                            DawrStudentId = record.studentId,
                            DawrAwardYear = record.awardYear,
                            DawrAward = record.awardId,
                            DawrAssignedToId = record.counselorId,
                            DeclAwdRequestAdddate = record.AddDate,
                            DeclAwdRequestAddtime = record.AddTime,
                            DawrDeclinedActionStatus = record.declinedStatusCode,
                            DawrCurrentStatus = record.statusCode,
                            DawrDeclinedInfoEntityAssociation = record.periodChangeRequests.Select(period =>
                                new DeclAwdRequestDawrDeclinedInfo()
                                {
                                    DawrAwardPeriodsAssocMember = period.awardPeriodId
                                }).ToList()
                        }).ToList())));

            dataReaderMock.Setup(r => r.ReadRecordAsync<FinAid>(It.IsAny<string>(), true))
                .Returns<string, bool>((c, b) => 
                {
                    FinAid finAidRecord = new FinAid();
                    finAidRecord.FaCounselorsEntityAssociation = new List<FinAidFaCounselors>();
                    foreach (var counselorEntity in expectedRepository.counselorEntities)
                    {
                        finAidRecord.FaCounselorsEntityAssociation.Add(new FinAidFaCounselors()
                        {
                            FaCounselorAssocMember = counselorEntity.FinancialAidCounselorId,
                            FaCounselorEndDateAssocMember = counselorEntity.FaCounselorEndDate,
                            FaCounselorStartDateAssocMember = counselorEntity.FaCounselorStartDate
                        });
                    }
                    return Task.FromResult(finAidRecord);
                });

            transManagerMock.Setup(t => t.ExecuteAsync<CreateAmountChangeRequestRequest, CreateAmountChangeRequestResponse>(It.IsAny<CreateAmountChangeRequestRequest>()))
                .Callback<CreateAmountChangeRequestRequest>(req => actualCreateAmountChangeRequestTransaction = req)
                .Returns<CreateAmountChangeRequestRequest>(req =>
                    Task.FromResult(new CreateAmountChangeRequestResponse()
                    {
                        AwardPeriodChange = req.AwardPeriodChange,
                        ChangeRequestId = new AwardPackageChangeRequest("", req.StudentId, req.AwardYear, req.AwardId).GetHashCode().ToString()
                    }));

            transManagerMock.Setup(t => t.ExecuteAsync<CreateStatusChangeRequestRequest, CreateStatusChangeRequestResponse>(It.IsAny<CreateStatusChangeRequestRequest>()))
                .Callback<CreateStatusChangeRequestRequest>(req => actualCreateStatusChangeRequestTransaction = req)
                .Returns<CreateStatusChangeRequestRequest>(req =>
                    Task.FromResult(new CreateStatusChangeRequestResponse()
                    {
                        AwardPeriodData = req.AwardPeriodData,
                        ChangeRequestId = new AwardPackageChangeRequest("", req.StudentId, req.AwardYear, req.AwardId).GetHashCode().ToString()
                    }));

            actualRepository = new AwardPackageChangeRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestClass]
        public class GetAwardPackageChangeRequestsTests : AwardPackageChangeRequestRepositoryTests
        {
            [TestInitialize]
            public async void Initialize()
            {
                AwardPackageChangeRequestRepositoryTestsInitialize();
                actualChangeRequests = await actualRepository.GetAwardPackageChangeRequestsAsync(studentId);
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedChangeRequests, actualChangeRequests.ToList());
            }

            [TestMethod]
            public void AssignedToCounselorIdTest()
            {
                foreach (var actual in actualChangeRequests)
                {
                    var expected = expectedChangeRequests.First(cr => cr.Equals(actual));
                    Assert.AreEqual(expected.AssignedToCounselorId, actual.AssignedToCounselorId);
                }
            }

            [TestMethod]
            public void CreateDateTimeTest()
            {
                foreach (var actual in actualChangeRequests)
                {
                    var expected = expectedChangeRequests.First(cr => cr.Equals(actual));
                    Assert.AreEqual(expected.CreateDateTime, actual.CreateDateTime);
                }
            }

            [TestMethod]
            public void StatusTest()
            {
                foreach (var actual in actualChangeRequests)
                {
                    var expected = expectedChangeRequests.First(cr => cr.Equals(actual));

                    foreach (var actualPeriod in actual.AwardPeriodChangeRequests)
                    {
                        var expectedPeriod = expected.AwardPeriodChangeRequests.FirstOrDefault(cr => cr.AwardPeriodId == actualPeriod.AwardPeriodId);
                        Assert.AreEqual(expectedPeriod.Status, actualPeriod.Status);
                    }
                }
            }

            [TestMethod]
            public void NewAmountTest()
            {
                foreach (var actual in actualChangeRequests)
                {
                    var expected = expectedChangeRequests.First(cr => cr.Equals(actual));

                    foreach (var actualPeriod in actual.AwardPeriodChangeRequests)
                    {
                        var expectedPeriod = expected.AwardPeriodChangeRequests.FirstOrDefault(cr => cr.AwardPeriodId == actualPeriod.AwardPeriodId);
                        Assert.AreEqual(expectedPeriod.NewAmount, actualPeriod.NewAmount);
                    }
                }
            }

            [TestMethod]
            public void NewStatusIdTest()
            {
                foreach (var actual in actualChangeRequests)
                {
                    var expected = expectedChangeRequests.First(cr => cr.Equals(actual));

                    foreach (var actualPeriod in actual.AwardPeriodChangeRequests)
                    {
                        var expectedPeriod = expected.AwardPeriodChangeRequests.FirstOrDefault(cr => cr.AwardPeriodId == actualPeriod.AwardPeriodId);
                        Assert.AreEqual(expectedPeriod.NewAwardStatusId, actualPeriod.NewAwardStatusId);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = "";
                await actualRepository.GetAwardPackageChangeRequestsAsync(studentId);
            }

            [TestMethod]
            public async Task NoRecordsForStudentIdTest()
            {
                studentId = "foobar";
                actualChangeRequests = await actualRepository.GetAwardPackageChangeRequestsAsync(studentId);
                Assert.AreEqual(0, actualChangeRequests.Count());
            }

            [TestMethod]
            public async Task CatchErrorsCreatingObjectsTest()
            {
                expectedRepository.LoanAmountChangeRequestData.ForEach(r => r.awardYear = "");
                expectedRepository.DeclinedStatusChangeRequestData.ForEach(r => r.awardId = "");
                actualChangeRequests = await actualRepository.GetAwardPackageChangeRequestsAsync(studentId);

                Assert.AreEqual(0, actualChangeRequests.Count());
            }
        }

        [TestClass]
        public class GetAwardPackageChangeRequestsForSpecificAwardTests : AwardPackageChangeRequestRepositoryTests
        {
            private List<AwardPackageChangeRequest> expectedRequests;
            private List<AwardPackageChangeRequest> actualRequests;
            private StudentAward studentAward;
            private StudentAwardYear studentAwardYear;
            private Award award, loan;

            [TestInitialize]
            public async void Initialize()
            {
                AwardPackageChangeRequestRepositoryTestsInitialize();

                studentAwardYear = new StudentAwardYear(studentId, "2016");
                award = new Award("AMERC", "AMERCORP", new AwardCategory("Grant", "Grant award", AwardCategoryType.Grant));
                loan = new Award("AMERC", "AMERCORP", new AwardCategory("Loan", "Loan award", AwardCategoryType.Loan));
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);

                expectedRequests = expectedRepository.GetAwardPackageChangeRequestsAsync(studentId, studentAward).Result.ToList();
                actualRequests = (await actualRepository.GetAwardPackageChangeRequestsAsync(studentId, studentAward)).ToList();
            }

            [TestMethod]
            public void StudentAwardIsNotLoan_ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedRequests, actualRequests);
            }

            [TestMethod]
            public async Task StudentAwardIsLoan_ExpectedEqualsActualTest()
            {
                studentAward = new StudentAward(studentAwardYear, studentId, loan, true);
                expectedRequests = expectedRepository.GetAwardPackageChangeRequestsAsync(studentId, studentAward).Result.ToList();
                actualRequests = (await actualRepository.GetAwardPackageChangeRequestsAsync(studentId, studentAward)).ToList();
                CollectionAssert.AreEqual(expectedRequests, actualRequests);
            }

            [TestMethod]
            public void AssignedToCounselorIdTest()
            {
                foreach (var actual in actualRequests)
                {
                    var expected = expectedRequests.First(cr => cr.Equals(actual));
                    Assert.AreEqual(expected.AssignedToCounselorId, actual.AssignedToCounselorId);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await actualRepository.GetAwardPackageChangeRequestsAsync(null, studentAward);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardIsRequiredTest()
            {
                await actualRepository.GetAwardPackageChangeRequestsAsync(studentId, null);
            }

            [TestMethod]
            public async Task StudentAwardIsNotLoan_ExpectedNumberOfChangeRequestsIsReturnedTest()
            {
                int expected = expectedRepository.DeclinedStatusChangeRequestData.Count;
                Assert.AreEqual(expected, (await actualRepository.GetAwardPackageChangeRequestsAsync(studentId, studentAward)).Count());
            }

            [TestMethod]
            public async Task StudentAwardIsLoan_ExpectedNumberOfChangeRequestsIsReturnedTest()
            {
                studentAward = new StudentAward(studentAwardYear, studentId, loan, true);
                int expected = expectedRepository.DeclinedStatusChangeRequestData.Count + expectedRepository.LoanAmountChangeRequestData.Count;
                Assert.AreEqual(expected, (await actualRepository.GetAwardPackageChangeRequestsAsync(studentId, studentAward)).Count());
            }
        }

        [TestClass]
        public class CreateAwardPackageChangeRequestAsyncTests : AwardPackageChangeRequestRepositoryTests
        {
            private StudentAward originalStudentAward;
            private AwardPackageChangeRequest awardPackageChangeRequest;

            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestRepositoryTestsInitialize();
                originalStudentAward = new StudentAward(
                    new StudentAwardYear("0003914", "2015"), "0003914", new Award("GRUMPY", "Grumpy Award", new AwardCategory("Grant", "Grant", AwardCategoryType.Grant)), true);
                originalStudentAward.StudentAwardPeriods = new List<StudentAwardPeriod>()
                {
                    new StudentAwardPeriod(originalStudentAward, "14/FA", new AwardStatus("A", "Accepted", AwardStatusCategory.Accepted), false, false)
                };
                awardPackageChangeRequest = new AwardPackageChangeRequest("", studentId, "2015", originalStudentAward.Award.Code);
                awardPackageChangeRequest.AwardPeriodChangeRequests = new List<AwardPeriodChangeRequest>()
                {
                    new AwardPeriodChangeRequest("14/FA"){
                        NewAwardStatusId = "D",
                        Status = AwardPackageChangeRequestStatus.Pending
                    }
                };
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardPackageChangeRequestIsRequiredTest()
            {
                await actualRepository.CreateAwardPackageChangeRequestAsync(null, originalStudentAward);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task OriginalStudentAwardIsRequiredTest()
            {
                await actualRepository.CreateAwardPackageChangeRequestAsync(awardPackageChangeRequest, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullFinAidRecordThrowsExceptionTest()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<FinAid>(It.IsAny<string>(), true))
                    .ReturnsAsync(null);                
                try
                {
                    await actualRepository.CreateAwardPackageChangeRequestAsync(awardPackageChangeRequest, originalStudentAward);
                }
                catch
                {
                    loggerMock.Verify(l => l.Error(string.Format("No FIN.AID record found for {0}", awardPackageChangeRequest.StudentId)));
                    throw;
                }
            } 

            [TestMethod]
            public async Task AssignedToCounselorId_equalsExpectedTest()
            {
                var expectedId = expectedRepository.counselorEntities.First(c => ((!c.FaCounselorStartDate.HasValue || c.FaCounselorStartDate.Value <= DateTime.Today)
                    && (!c.FaCounselorEndDate.HasValue || c.FaCounselorEndDate.Value >= DateTime.Today))).FinancialAidCounselorId;
                Assert.AreEqual(expectedId, (await actualRepository.CreateAwardPackageChangeRequestAsync(awardPackageChangeRequest, originalStudentAward)).AssignedToCounselorId);
                            
            }
        }
    }
}
