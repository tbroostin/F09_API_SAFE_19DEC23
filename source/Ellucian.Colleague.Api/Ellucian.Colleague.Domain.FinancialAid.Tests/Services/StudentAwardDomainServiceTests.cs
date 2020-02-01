/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentAwardDomainServiceTests
    {
        public TestFinancialAidOfficeRepository financialAidOfficeRepository;
        public TestFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestStudentAwardRepository studentAwardRepository;
        public TestStudentLoanLimitationRepository studentLoanLimitationRepository;
        public TestAwardPackageChangeRequestRepository awardPackageChangeRequestRepository;

        public string studentId;

        public void StudentAwardDomainServiceTestsInitialize()
        {
            financialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            financialAidReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            studentAwardRepository = new TestStudentAwardRepository();
            studentLoanLimitationRepository = new TestStudentLoanLimitationRepository();
            awardPackageChangeRequestRepository = new TestAwardPackageChangeRequestRepository();

            studentId = "0003914";
        }

        [TestClass]
        public class VerifyUpdatedStudentAwardsTests : StudentAwardDomainServiceTests
        {
            public StudentAwardYear inputStudentAwardYear;
            public StudentAwardYear inputStudentAwardYear2;
            public IEnumerable<StudentAward> inputCurrentStudentAwards;
            public IEnumerable<StudentLoanLimitation> inputStudentLoanLimitations;

            public List<StudentAward> inputNewStudentAwards;
            public List<StudentAward> outputNewStudentAwards;

            public StudentAward inputAmountChangeAward;
            public StudentAward inputStatusChangeAward;

            public AwardStatus newAwardStatus;
            private bool suppressMaximumLoanLimits;

            [TestInitialize]
            public async void Initialize()
            {
                StudentAwardDomainServiceTestsInitialize();

                inputStudentAwardYear = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())).First();
                inputStudentAwardYear2 = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())).Last();
                inputStudentAwardYear.CurrentConfiguration.IsLoanAmountChangeRequestRequired = false;
                inputStudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired = false;

                inputCurrentStudentAwards = studentAwardRepository.GetStudentAwardsForYearAsync(studentId, inputStudentAwardYear, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses).Result;
                inputStudentLoanLimitations = studentLoanLimitationRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { inputStudentAwardYear }).Result;

                inputAmountChangeAward = studentAwardRepository.DeepCopy(inputCurrentStudentAwards.First(sa => sa.IsAmountModifiable));
                inputAmountChangeAward.StudentAwardPeriods.Where(p => p.IsAmountModifiable).ToList().ForEach(p => p.AwardAmount += 10);

                newAwardStatus = financialAidReferenceDataRepository.AwardStatuses.First(s => s.Code == "X");
                inputStatusChangeAward = studentAwardRepository.DeepCopy(
                    inputCurrentStudentAwards.First(sa => sa.StudentAwardPeriods.Any(p => p.IsStatusModifiable) && !sa.Equals(inputAmountChangeAward)));
                inputStatusChangeAward.StudentAwardPeriods.Where(p => p.IsStatusModifiable).ToList().ForEach(p => p.AwardStatus = newAwardStatus);

                suppressMaximumLoanLimits = true;

                inputNewStudentAwards = new List<StudentAward>() { inputAmountChangeAward, inputStatusChangeAward };
            }

            //TODO mcd: New exception attribute to verify the exception message?

            [TestMethod]
            public void InputAwardsAreVerified()
            {
                VerifyStudentAwards();
                CollectionAssert.AreEqual(inputNewStudentAwards, outputNewStudentAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAwardYearRequiredTest()
            {
                inputStudentAwardYear = null;
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NewStudentAwardsRequiredTest()
            {
                inputNewStudentAwards = null;
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CurrentStudentAwardsRequiredTest()
            {
                inputCurrentStudentAwards = null;
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentLoanLimitationsRequiredTest()
            {
                inputStudentLoanLimitations = null;
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NoNewStudentAwardsForYearTest()
            {
                var lastStudentAwardYear = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())).Last();
                inputStudentAwardYear = lastStudentAwardYear;
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NoNewStudentAwardsTest()
            {
                inputNewStudentAwards = new List<StudentAward>();
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NoCurrentStudentAwardsForYearTest()
            {
                var lastStudentAwardYear = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())).Last();
                inputCurrentStudentAwards = studentAwardRepository.GetStudentAwardsForYearAsync(studentId, lastStudentAwardYear, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses).Result;
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void NoStudentLoanLimitationsForYearTest()
            {
                inputStudentLoanLimitations = new List<StudentLoanLimitation>();
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void CannotAddStudentAward()
            {
                inputNewStudentAwards.Add(new StudentAward(inputStudentAwardYear, studentId, new Award("foo", "bar", new AwardCategory("A", "A", null)), true));
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void CannotUpdateStudentAwardWithPendingChangeRequestTest()
            {
                inputCurrentStudentAwards.ToList().ForEach(sa => sa.PendingChangeRequestId = "5");
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void CannotAddStudentAwardPeriodTest()
            {
                new StudentAwardPeriod(inputAmountChangeAward, "foobar", newAwardStatus, false, false);
                VerifyStudentAwards();
            }

            [TestMethod]
            public void NullNewAmountDoesNotChangeCurrentAmountToNull()
            {
                inputAmountChangeAward.StudentAwardPeriods.ForEach(p => p.AwardAmount = null);
                VerifyStudentAwards();

                var outputChangeAward = outputNewStudentAwards.First(sa => sa.Equals(inputAmountChangeAward));
                foreach (var outputPeriod in outputChangeAward.StudentAwardPeriods)
                {
                    var equivCurrentPeriod = inputCurrentStudentAwards.SelectMany(sa => sa.StudentAwardPeriods).First(p => p.Equals(outputPeriod));
                    Assert.AreEqual(equivCurrentPeriod.AwardAmount, outputPeriod.AwardAmount);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(UpdateRequiresReviewException))]
            public void LoanTypeAwards_AmountChangeRequiresReviewTest()
            {
                inputStudentAwardYear.CurrentConfiguration.IsLoanAmountChangeRequestRequired = true;
                VerifyStudentAwards();
            }

            [TestMethod]
            public void NonLoanTypeAwards_AmountChangeDoesNotRequireReviewTest()
            {
                var exceptionThrown = false;
                inputStudentAwardYear.CurrentConfiguration.IsLoanAmountChangeRequestRequired = true;

                var nonLoanTypeAward = studentAwardRepository.DeepCopy(inputCurrentStudentAwards.First(sa => sa.Award.LoanType == null));
                nonLoanTypeAward.StudentAwardPeriods.ForEach(ap => ap.AwardAmount = 0);

                inputNewStudentAwards = new List<StudentAward>() { nonLoanTypeAward };
                try
                {
                    VerifyStudentAwards();
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public void NullNewStatusDoesNotChangeCurrentStatusToNullTest()
            {
                inputStatusChangeAward.StudentAwardPeriods.ForEach(p => p.AwardStatus = null);
                VerifyStudentAwards();

                var outputChangeAward = outputNewStudentAwards.First(sa => sa.Equals(inputStatusChangeAward));
                foreach (var outputPeriod in outputChangeAward.StudentAwardPeriods)
                {
                    var equivCurrentPeriod = inputCurrentStudentAwards.SelectMany(sa => sa.StudentAwardPeriods).First(p => p.Equals(outputPeriod));
                    Assert.AreEqual(equivCurrentPeriod.AwardStatus, outputPeriod.AwardStatus);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void StudentAwardPeriodStatusIsNotModifiableTest()
            {
                inputStudentAwardYear.CurrentConfiguration.ExcludeAwardsFromChange.Add(inputStatusChangeAward.Award.Code);
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(UpdateRequiresReviewException))]
            public void DeclinedStatusChangeRequiresReviewTest()
            {
                inputStudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired = true;
                inputStatusChangeAward.StudentAwardPeriods.ForEach(p => p.AwardStatus = financialAidReferenceDataRepository.AwardStatuses.First(s => s.Category == AwardStatusCategory.Denied));
                VerifyStudentAwards();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void AmountChangesAreNotWithinLoanLimitations_NotSuppressLimitsTest()
            {
                suppressMaximumLoanLimits = false;
                inputStudentLoanLimitations.ToList().ForEach(l => { l.GradPlusMaximumAmount = 0; l.SubsidizedMaximumAmount = 0; l.UnsubsidizedMaximumAmount = 0; });
                VerifyStudentAwards2();
            }

            [TestMethod]
            public void AmountChangesAreNotWithinLoanLimitations_SuppressLimitsTest()
            {
                var exceptionThrown = false;
                inputStudentLoanLimitations.ToList().ForEach(l => { l.GradPlusMaximumAmount = 0; l.SubsidizedMaximumAmount = 0; l.UnsubsidizedMaximumAmount = 0; });
                try
                {
                    VerifyStudentAwards();
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void IncomingUnsubCollection_PendingSubAwardPresent_ThrowsInvalidOperationExceptionTest()
            {
                inputStudentAwardYear = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices()))
                    .First(ay => ay.Code == "2017");
                inputCurrentStudentAwards = studentAwardRepository.GetStudentAwardsForYearAsync(studentId, inputStudentAwardYear, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses).Result;
                inputStudentLoanLimitations = studentLoanLimitationRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { inputStudentAwardYear }).Result;
                inputNewStudentAwards = inputCurrentStudentAwards.Where(a => a.Award.LoanType.Value == LoanType.UnsubsidizedLoan).ToList();
                VerifyStudentAwards();
            }


            [TestMethod]
            public void IncomingSingleUnsub_AcceptedSubAwardPresent_VerificationGoesThroughTest()
            {
                bool exceptionThrown = false;
                studentAwardRepository.awardPeriodData.Where(ap => ap.year == "2017").ToList().ForEach(ap => ap.awardStatus = "A");
                inputStudentAwardYear = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices()))
                    .First(ay => ay.Code == "2017");
                inputStudentAwardYear.CurrentConfiguration.AllowDeclineZeroOfAcceptedLoans = false;
                inputCurrentStudentAwards = studentAwardRepository.GetStudentAwardsForYearAsync(studentId, inputStudentAwardYear, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses).Result;
                inputStudentLoanLimitations = studentLoanLimitationRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { inputStudentAwardYear }).Result;
                inputNewStudentAwards = new List<StudentAward>() { inputCurrentStudentAwards.First(a => a.Award.LoanType.Value == LoanType.UnsubsidizedLoan) };

                try
                {
                    VerifyStudentAwards();
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public void AcceptAllAwards_PendingSubAwardPresent_VerificationGoesThroughTest()
            {
                bool exceptionThrown = false;

                inputStudentAwardYear = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices()))
                    .First(ay => ay.Code == "2017");
                inputCurrentStudentAwards = studentAwardRepository.GetStudentAwardsForYearAsync(studentId, inputStudentAwardYear, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses).Result;
                inputStudentLoanLimitations = studentLoanLimitationRepository.GetStudentLoanLimitationsAsync(studentId, new List<StudentAwardYear>() { inputStudentAwardYear }).Result;
                inputNewStudentAwards = inputCurrentStudentAwards.ToList();
                try
                {
                    VerifyStudentAwards();
                }
                catch { exceptionThrown = false; }
                Assert.IsFalse(exceptionThrown);
            }

            private void VerifyStudentAwards()
            {
                outputNewStudentAwards = StudentAwardDomainService.VerifyUpdatedStudentAwards(inputStudentAwardYear, inputNewStudentAwards, inputCurrentStudentAwards, inputStudentLoanLimitations, suppressMaximumLoanLimits).ToList();
            }
            private void VerifyStudentAwards2()
            {
                outputNewStudentAwards = StudentAwardDomainService.VerifyUpdatedStudentAwards(inputStudentAwardYear2, inputNewStudentAwards, inputCurrentStudentAwards, inputStudentLoanLimitations, suppressMaximumLoanLimits).ToList();

            }
        }

        [TestClass]
        public class AssignPendingChangeRequestsTests : StudentAwardDomainServiceTests
        {
            public IEnumerable<StudentAward> inputStudentAwards;
            public IEnumerable<AwardPackageChangeRequest> inputAwardPackageChangeRequests;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardDomainServiceTestsInitialize();

                inputStudentAwards = studentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())),
                    financialAidReferenceDataRepository.Awards,
                    financialAidReferenceDataRepository.AwardStatuses).Result;

                awardPackageChangeRequestRepository.AddPendingChangeRequestRecordsForStudentAwards(inputStudentAwards);
                inputAwardPackageChangeRequests = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result;
            }

            [TestMethod]
            public void StudentAwardWithPendingRequestTest()
            {
                StudentAwardDomainService.AssignPendingChangeRequests(inputStudentAwards, inputAwardPackageChangeRequests);
                foreach (var inputStudentAward in inputStudentAwards.Where(sa => !string.IsNullOrEmpty(sa.PendingChangeRequestId)))
                {
                    var changeRequest = inputAwardPackageChangeRequests.FirstOrDefault(cr => cr.Id == inputStudentAward.PendingChangeRequestId);
                    Assert.IsNotNull(changeRequest);

                    Assert.IsTrue(changeRequest.IsForStudentAward(inputStudentAward));
                }
            }

            [TestMethod]
            public void StudentAwardWithNoPendingRequestTest()
            {
                awardPackageChangeRequestRepository.LoanAmountChangeRequestData = new List<TestAwardPackageChangeRequestRepository.LoanAmountChangeRequestRecord>();
                awardPackageChangeRequestRepository.DeclinedStatusChangeRequestData = new List<TestAwardPackageChangeRequestRepository.DeclinedStatusChangeRequestRecord>();
                inputAwardPackageChangeRequests = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result;

                StudentAwardDomainService.AssignPendingChangeRequests(inputStudentAwards, inputAwardPackageChangeRequests);

                Assert.IsTrue(inputStudentAwards.All(sa => string.IsNullOrEmpty(sa.PendingChangeRequestId)));
            }

            [TestMethod]
            public void NullInputStudentAwardsTest()
            {
                IEnumerable<StudentAward> studentAwards = null;
                StudentAwardDomainService.AssignPendingChangeRequests(studentAwards, inputAwardPackageChangeRequests);
                Assert.IsTrue(inputStudentAwards.All(sa => string.IsNullOrEmpty(sa.PendingChangeRequestId)));
            }

            [TestMethod]
            public void NullInputAwardPackageChangeRequestsTest()
            {
                StudentAwardDomainService.AssignPendingChangeRequests(inputStudentAwards, null);
                Assert.IsTrue(inputStudentAwards.All(sa => string.IsNullOrEmpty(sa.PendingChangeRequestId)));
            }
        }

        [TestClass]
        public class AssignPendingChangeRequestsSingleAwardTeats : StudentAwardDomainServiceTests
        {
            public StudentAward inputStudentAward;
            public IEnumerable<AwardPackageChangeRequest> inputAwardPackageChangeRequests;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardDomainServiceTestsInitialize();

                inputStudentAward = studentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeRepository.GetFinancialAidOffices())),
                    financialAidReferenceDataRepository.Awards,
                    financialAidReferenceDataRepository.AwardStatuses).Result.First();


                awardPackageChangeRequestRepository.AddPendingChangeRequestRecordsForStudentAwards(new List<StudentAward>() { inputStudentAward });
                inputAwardPackageChangeRequests = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result;
            }

            [TestMethod]
            public void PendingLoanRequestIdAssignedTest()
            {
                StudentAwardDomainService.AssignPendingChangeRequests(inputStudentAward, inputAwardPackageChangeRequests);
                var changeRequest = inputAwardPackageChangeRequests.FirstOrDefault(cr => cr.Id == inputStudentAward.PendingChangeRequestId);
                Assert.IsNotNull(changeRequest);

                Assert.IsTrue(changeRequest.IsForStudentAward(inputStudentAward));
            }

            [TestMethod]
            public void StudentAwardIsNull_PendingLoanRequestIdIsNullTest()
            {
                StudentAwardDomainService.AssignPendingChangeRequests((StudentAward)null, inputAwardPackageChangeRequests);
                var changeRequest = inputAwardPackageChangeRequests.FirstOrDefault(cr => cr.Id == inputStudentAward.PendingChangeRequestId);
                Assert.IsNull(changeRequest);
            }

            [TestMethod]
            public void NullInputAwardPackageChangeRequests_NoPendingRequestIdAssignedTest()
            {
                StudentAwardDomainService.AssignPendingChangeRequests(inputStudentAward, null);
                Assert.IsTrue(string.IsNullOrEmpty(inputStudentAward.PendingChangeRequestId));
            }

            [TestMethod]
            public void StudentAwardWithNoPendingRequestTest()
            {
                //reinitialize the repository so that there are no records assigned to the student awards
                awardPackageChangeRequestRepository = new TestAwardPackageChangeRequestRepository();
                inputAwardPackageChangeRequests = awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId).Result;

                StudentAwardDomainService.AssignPendingChangeRequests(inputStudentAward, inputAwardPackageChangeRequests);

                Assert.IsTrue(string.IsNullOrEmpty(inputStudentAward.PendingChangeRequestId));
            }

        }
    }
}
