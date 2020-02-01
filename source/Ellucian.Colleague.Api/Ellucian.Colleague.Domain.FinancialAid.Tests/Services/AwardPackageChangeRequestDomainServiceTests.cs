/*Copyright 2015-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Services
{
    [TestClass]
    public class AwardPackageChangeRequestDomainServiceTests
    {
        public TestFinancialAidOfficeRepository financialAidOfficeRepository;
        public TestFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestStudentAwardRepository studentAwardRepository;
        public TestStudentLoanLimitationRepository studentLoanLimitationRepository;
        public TestAwardPackageChangeRequestRepository awardPackageChangeRequestRepository;

        public string studentId;
        public string counselorId;

        public void StudentAwardDomainServiceTestsInitialize()
        {
            financialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            financialAidReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            studentAwardRepository = new TestStudentAwardRepository();
            studentLoanLimitationRepository = new TestStudentLoanLimitationRepository();
            awardPackageChangeRequestRepository = new TestAwardPackageChangeRequestRepository();

            studentId = "0003914";
            counselorId = "0010749";
        }

        [TestClass]
        public class VerifyAwardPackageChangeRequestTests : AwardPackageChangeRequestDomainServiceTests
        {
            public AwardPackageChangeRequest inputAmountChangeRequest;
            public AwardPackageChangeRequest inputStatusChangeRequest;
            public StudentAward inputStudentAward;
            public Student.Entities.Student inputStudent;
            public Student.Entities.Applicant inputApplicant;
            public IEnumerable<AwardStatus> inputAwardStatuses;

            public AwardPackageChangeRequest outputChangeRequest;


            [TestInitialize]
            public void Initialize()
            {
                StudentAwardDomainServiceTestsInitialize();

                inputStudentAward = studentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())),
                    financialAidReferenceDataRepository.Awards,
                    financialAidReferenceDataRepository.AwardStatuses).Result.First(sa => sa.Award.LoanType.HasValue);

                inputStudentAward.StudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired = true;
                inputStudentAward.StudentAwardYear.CurrentConfiguration.IsLoanAmountChangeRequestRequired = true;

                inputStudent = new Student.Entities.Student(studentId, "foo", null, new List<string>(), new List<string>()) { FinancialAidCounselorId = counselorId };
                inputApplicant = new Student.Entities.Applicant(studentId, "foo") { FinancialAidCounselorId = counselorId };
                inputAwardStatuses = financialAidReferenceDataRepository.AwardStatuses;

                inputAmountChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code)
                    {
                        AwardPeriodChangeRequests = inputStudentAward.StudentAwardPeriods.Select(period =>
                            new AwardPeriodChangeRequest(period.AwardPeriodId)
                            {
                                NewAmount = period.AwardAmount + 10,
                                NewAwardStatusId = inputAwardStatuses.First(s => s.Category == AwardStatusCategory.Accepted).Code,

                            }).ToList()
                    };

                inputStatusChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code)
                {
                    AwardPeriodChangeRequests = inputStudentAward.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest(period.AwardPeriodId)
                        {
                            NewAwardStatusId = inputAwardStatuses.First(s => s.Category == AwardStatusCategory.Denied).Code,
                        }).ToList()
                };
            }

            public void VerifyChangeRequest(AwardPackageChangeRequest changeReqest, IEnumerable<StudentAward> allAwardsForYear = null)
            {
                outputChangeRequest = AwardPackageChangeRequestDomainService.VerifyAwardPackageChangeRequest(changeReqest, inputStudentAward, inputAwardStatuses, allAwardsForYear);
            }

            [TestMethod]
            public void VerifiedAmountChangeRequest()
            {
                VerifyChangeRequest(inputAmountChangeRequest);
                Assert.AreEqual(inputAmountChangeRequest, outputChangeRequest);
                Assert.IsTrue(outputChangeRequest.AwardPeriodChangeRequests.All(cr => cr.Status == AwardPackageChangeRequestStatus.Pending));
            }

            [TestMethod]
            public void VerifiedStatusChangeRequest()
            {
                VerifyChangeRequest(inputStatusChangeRequest);
                Assert.AreEqual(inputStatusChangeRequest, outputChangeRequest);
                Assert.IsTrue(outputChangeRequest.AwardPeriodChangeRequests.All(cr => cr.Status == AwardPackageChangeRequestStatus.Pending));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullInputStudentAwardTest()
            {
                inputStudentAward = null;
                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullInputChangeRequestTest()
            {
                VerifyChangeRequest(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullInputAwardStatusesTest()
            {
                inputAwardStatuses = null;
                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void MismatchedStudentIdTest()
            {
                inputAmountChangeRequest = new AwardPackageChangeRequest("", "foobar", inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code);
                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void MismatchedAwardYearIdTest()
            {
                inputAmountChangeRequest = new AwardPackageChangeRequest("", studentId, "foobar", inputStudentAward.Award.Code);
                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void MismatchedAwardIdTest()
            {
                inputAmountChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, "foobar");
                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NoAwardPeriodChangeRequestsTest()
            {
                inputAmountChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code);
                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NoChangesInAwardPeriodChangeRequestsTest()
            {
                inputAmountChangeRequest.AwardPeriodChangeRequests.ForEach(p => { p.NewAmount = null; p.NewAwardStatusId = string.Empty; });
                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public void PendingChangeRequestAlreadyExistsForStudentAwardTest()
            {
                var id = "5";
                inputStudentAward.PendingChangeRequestId = id;
                try
                {
                    VerifyChangeRequest(inputAmountChangeRequest);
                }
                catch (ExistingResourceException ere)
                {
                    Assert.AreEqual(ere.ExistingResourceId, id);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void AmountChangeRequestForNonLoanTypeAwardTest()
            {
                inputStudentAward = studentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())),
                    financialAidReferenceDataRepository.Awards,
                    financialAidReferenceDataRepository.AwardStatuses).Result.First(sa => !sa.Award.LoanType.HasValue);
                inputAmountChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code)
                {
                    AwardPeriodChangeRequests = inputStudentAward.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest(period.AwardPeriodId)
                        {
                            NewAmount = period.AwardAmount + 10,
                            NewAwardStatusId = inputAwardStatuses.First(s => s.Category == AwardStatusCategory.Accepted).Code,

                        }).ToList()
                };

                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void AmountChangeRequestsDoNotRequireReviewTest()
            {
                inputStudentAward.StudentAwardYear.CurrentConfiguration.IsLoanAmountChangeRequestRequired = false;
                VerifyChangeRequest(inputAmountChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void StatusChangeRequestsDoNotRequireReviewTest()
            {
                inputStudentAward.StudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired = false;
                VerifyChangeRequest(inputStatusChangeRequest);
            }

            [TestMethod]
            public void BadAwardPeriodIdRejectedTest()
            {
                var badId = "foobar";
                inputAmountChangeRequest.AwardPeriodChangeRequests.Add(new AwardPeriodChangeRequest(badId));
                VerifyChangeRequest(inputAmountChangeRequest);

                var rejectedPeriod = outputChangeRequest.AwardPeriodChangeRequests.First(cr => cr.AwardPeriodId == badId);
                Assert.AreEqual(AwardPackageChangeRequestStatus.RejectedBySystem, rejectedPeriod.Status);
                Assert.AreEqual("Cannot process request for award period because no matching StudentAwardPeriod exists", rejectedPeriod.StatusReason);
            }

            [TestMethod]
            public void NoAmountChangeForPeriodRequestedTest()
            {
                inputAmountChangeRequest.AwardPeriodChangeRequests.First().NewAmount = null;
                VerifyChangeRequest(inputAmountChangeRequest);
                var noChangePeriod = inputAmountChangeRequest.AwardPeriodChangeRequests.First();
                Assert.AreEqual(AwardPackageChangeRequestStatus.New, noChangePeriod.Status);
            }

            [TestMethod]
            public void PeriodRequestForAmountLessThanZeroTest()
            {
                inputAmountChangeRequest.AwardPeriodChangeRequests.First().NewAmount = -1;
                VerifyChangeRequest(inputAmountChangeRequest);
                var rejectedPeriod = inputAmountChangeRequest.AwardPeriodChangeRequests.First();
                Assert.AreEqual(AwardPackageChangeRequestStatus.RejectedBySystem, rejectedPeriod.Status);
                Assert.AreEqual("Cannot change amount to a value less than zero", rejectedPeriod.StatusReason);
            }

            [TestMethod]
            public void NoStatusChangeForPeriodRequestedTest()
            {
                inputStatusChangeRequest.AwardPeriodChangeRequests.First().NewAwardStatusId = string.Empty;
                VerifyChangeRequest(inputStatusChangeRequest);
                var noChangePeriod = inputStatusChangeRequest.AwardPeriodChangeRequests.First();
                Assert.AreEqual(AwardPackageChangeRequestStatus.New, noChangePeriod.Status);
            }

            [TestMethod]
            public void StatusChangeForNonExistantStatusTest()
            {
                inputStatusChangeRequest.AwardPeriodChangeRequests.First().NewAwardStatusId = "foobar";
                VerifyChangeRequest(inputStatusChangeRequest);
                var rejectedPeriod = inputStatusChangeRequest.AwardPeriodChangeRequests.First();
                Assert.AreEqual(AwardPackageChangeRequestStatus.RejectedBySystem, rejectedPeriod.Status);
                Assert.AreEqual("Cannot process status change request for award period because no matching AwardStatus exists", rejectedPeriod.StatusReason);
            }

            [TestMethod]
            public void StatusChangeForNonDeclinedStatusTest()
            {
                inputStatusChangeRequest.AwardPeriodChangeRequests.First().NewAwardStatusId = inputAwardStatuses.First(s => s.Category == AwardStatusCategory.Pending).Code;
                VerifyChangeRequest(inputStatusChangeRequest);
                var rejectedPeriod = inputStatusChangeRequest.AwardPeriodChangeRequests.First();
                Assert.AreEqual(AwardPackageChangeRequestStatus.RejectedBySystem, rejectedPeriod.Status);
                Assert.AreEqual("Change Requests can only be submitted for Rejected or Denied AwardStatusCategories at this time", rejectedPeriod.StatusReason);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void AllChangesAreRejectedTest()
            {
                inputStatusChangeRequest.AwardPeriodChangeRequests.ForEach(cr => cr.NewAwardStatusId = "foobar");
                VerifyChangeRequest(inputStatusChangeRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void DeclineUnsubWhenPendingSubExists_ThrowsInvalidOperationExceptionTest()
            {
                var allAwardsForYear = studentAwardRepository.GetStudentAwardsForYearAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices()))
                    .First(ay => ay.Code == "2017"),
                    financialAidReferenceDataRepository.Awards,
                    financialAidReferenceDataRepository.AwardStatuses);

                inputStudentAward = allAwardsForYear.Result.First(sa => sa.Award.LoanType.HasValue && sa.Award.LoanType.Value == LoanType.UnsubsidizedLoan);

                inputStatusChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code)
                {
                    AwardPeriodChangeRequests = inputStudentAward.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest(period.AwardPeriodId)
                        {
                            NewAwardStatusId = inputAwardStatuses.First(s => s.Category == AwardStatusCategory.Denied).Code,
                        }).ToList()
                };
                
                VerifyChangeRequest(inputStatusChangeRequest, allAwardsForYear.Result);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ChangeAmountUnsubWhenPendingSubExists_ThrowsInvalidOperationExceptionTest()
            {
                var allAwardsForYear = studentAwardRepository.GetStudentAwardsForYearAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices()))
                    .First(ay => ay.Code == "2017"),
                    financialAidReferenceDataRepository.Awards,
                    financialAidReferenceDataRepository.AwardStatuses);

                inputStudentAward = allAwardsForYear.Result.First(sa => sa.Award.LoanType.HasValue && sa.Award.LoanType.Value == LoanType.UnsubsidizedLoan);

                inputStatusChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code)
                {
                    AwardPeriodChangeRequests = inputStudentAward.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest(period.AwardPeriodId)
                        {
                            NewAmount = period.AwardAmount + 10,
                            NewAwardStatusId = inputAwardStatuses.First(s => s.Category == AwardStatusCategory.Accepted).Code,
                        }).ToList()
                };

                VerifyChangeRequest(inputStatusChangeRequest, allAwardsForYear.Result);
            }

            [TestMethod]
            public void DeclineSub_DoesntThrowInvalidOperationExceptionTest()
            {
                bool exceptionThrown = false;
                var allAwardsForYear = studentAwardRepository.GetStudentAwardsForYearAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices()))
                    .First(ay => ay.Code == "2017"),
                    financialAidReferenceDataRepository.Awards,
                    financialAidReferenceDataRepository.AwardStatuses);

                inputStudentAward = allAwardsForYear.Result.First(sa => sa.Award.LoanType.HasValue && sa.Award.LoanType.Value == LoanType.SubsidizedLoan);
                inputStudentAward.StudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired = true;

                inputStatusChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code)
                {
                    AwardPeriodChangeRequests = inputStudentAward.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest(period.AwardPeriodId)
                        {
                            NewAwardStatusId = inputAwardStatuses.First(s => s.Category == AwardStatusCategory.Denied).Code,
                        }).ToList()
                };
                try
                {
                    VerifyChangeRequest(inputStatusChangeRequest, allAwardsForYear.Result);
                }
                catch (InvalidOperationException ioe)
                {
                    exceptionThrown = true;
                }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public void DeclineUnsub_NoPendingSub_DoesntThrowInvalidOperationExceptionTest()
            {
                bool exceptionThrown = false;
                studentAwardRepository.awardPeriodData.Where(ap => ap.year == "2017" && ap.award == "SUBDL").ToList().ForEach(ap => ap.awardStatus = "A");
                var year = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices()))
                    .First(ay => ay.Code == "2017");
                year.CurrentConfiguration.AllowDeclineZeroOfAcceptedLoans = false;
                var allAwardsForYear = studentAwardRepository.GetStudentAwardsForYearAsync(
                    studentId,
                    year,
                    financialAidReferenceDataRepository.Awards,
                    financialAidReferenceDataRepository.AwardStatuses);

                inputStudentAward = allAwardsForYear.Result.First(sa => sa.Award.LoanType.HasValue && sa.Award.LoanType.Value == LoanType.UnsubsidizedLoan);
                inputStudentAward.StudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired = true;

                inputStatusChangeRequest = new AwardPackageChangeRequest("", studentId, inputStudentAward.StudentAwardYear.Code, inputStudentAward.Award.Code)
                {
                    AwardPeriodChangeRequests = inputStudentAward.StudentAwardPeriods.Select(period =>
                        new AwardPeriodChangeRequest(period.AwardPeriodId)
                        {
                            NewAwardStatusId = inputAwardStatuses.First(s => s.Category == AwardStatusCategory.Denied).Code,
                        }).ToList()
                };
                try
                {
                    VerifyChangeRequest(inputStatusChangeRequest, allAwardsForYear.Result);
                }
                catch (InvalidOperationException ioe)
                {
                    exceptionThrown = true;
                }
                Assert.IsFalse(exceptionThrown);
            }
        }

        [TestClass]
        public class GetCommunicationsTests : AwardPackageChangeRequestDomainServiceTests
        {
            private AwardPackageChangeRequest changeRequest;
            private StudentAward studentAward;
            private List<AwardStatus> awardStatuses;

            private List<Communication> actualCommunications;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardDomainServiceTestsInitialize();
                awardStatuses = financialAidReferenceDataRepository.AwardStatuses.ToList();
                studentAward = studentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())),
                    financialAidReferenceDataRepository.Awards,
                    awardStatuses).Result.First(sa => sa.Award.Code == "UNSUB1");
                changeRequest = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result
                    .FirstOrDefault(r => r.AwardId == studentAward.Award.Code);                
            }

            [TestMethod]
            public void GetCommunications_DeclineLoan_ReturnsExpectedResultTest()
            {
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsNotNull(actualCommunications);
                Assert.IsTrue(actualCommunications.Count == 1);
                Assert.AreEqual("WEBREJECT", actualCommunications.First().Code);
            }
            
            [TestMethod]
            public void GetCommunications_NoRejectCode_NoCommunicationReturnedTest()
            {
                financialAidOfficeRepository.officeParameterRecordData.ForEach(pr => pr.RejectedAwardCommunicationCode = null);
                studentAward = studentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())),
                    financialAidReferenceDataRepository.Awards,
                    awardStatuses).Result.First(sa => sa.Award.Code == "UNSUB1");
                changeRequest = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result
                    .FirstOrDefault(r => r.AwardId == studentAward.Award.Code);
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsFalse(actualCommunications.Any());
            }

            [TestMethod]
            public void GetCommunications_NoStatusChanges_NoCommunicationReturnedTest()
            {
                changeRequest = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result
                    .FirstOrDefault(r => r.AwardId == studentAward.Award.Code);
                changeRequest.AwardPeriodChangeRequests.ForEach(pcr => pcr.NewAwardStatusId = string.Empty);
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsFalse(actualCommunications.Any());
            }

            [TestMethod]
            public void GetCommunications_RequestNonPendingStatus_NoCommunicationReturnedTest()
            {
                changeRequest = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result
                    .FirstOrDefault(r => r.AwardId == studentAward.Award.Code);
                changeRequest.AwardPeriodChangeRequests.ForEach(pcr => pcr.Status = AwardPackageChangeRequestStatus.RejectedBySystem);
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsFalse(actualCommunications.Any());
            }

            [TestMethod]
            public void GetCommunications_NonDeclineNewStatus_NoCommunicationReturnedTest()
            {
                changeRequest = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result
                    .FirstOrDefault(r => r.AwardId == studentAward.Award.Code);
                changeRequest.AwardPeriodChangeRequests.ForEach(pcr => pcr.NewAwardStatusId = "A");
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsFalse(actualCommunications.Any());
            }

            [TestMethod]
            public void GetCommunications_ChangeLoanAmount_ReturnsExpectedResultTest()
            {
                GetLoanChangeRequest();
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsNotNull(actualCommunications);
                Assert.IsTrue(actualCommunications.Count == 1);
                Assert.AreEqual("WEBLOANCHANGE", actualCommunications.First().Code);
            }

            [TestMethod]
            public void GetCommunications_NoLoanChangeCode_NoCommunicationReturnedTest()
            {
                financialAidOfficeRepository.officeParameterRecordData.ForEach(pr => pr.LoanChangeCommunicationCode = null);
                GetLoanChangeRequest();
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsFalse(actualCommunications.Any());
            }

            [TestMethod]
            public void GetCommunications_NoAmountChanges_NoCommunicationReturnedTest()
            {
                GetLoanChangeRequest();
                changeRequest.AwardPeriodChangeRequests.ForEach(pcr => pcr.NewAmount = null);
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsFalse(actualCommunications.Any());
            }

            [TestMethod]
            public void GetCommunications_AmountChangeRequestNonPendingStatus_NoCommunicationReturnedTest()
            {
                GetLoanChangeRequest();
                changeRequest.AwardPeriodChangeRequests.ForEach(pcr => pcr.Status = AwardPackageChangeRequestStatus.RejectedBySystem);
                actualCommunications = AwardPackageChangeRequestDomainService.GetCommunications(changeRequest, studentAward, awardStatuses).ToList();
                Assert.IsFalse(actualCommunications.Any());
            }

            private void GetLoanChangeRequest()
            {
                studentAward = studentAwardRepository.GetAllStudentAwardsAsync(
                                    studentId,
                                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())),
                                    financialAidReferenceDataRepository.Awards,
                                    awardStatuses).Result.First(sa => sa.Award.Code == "SNEEZY");
                changeRequest = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result
                    .FirstOrDefault(r => r.AwardId == studentAward.Award.Code);
            }
        }
    }
}
