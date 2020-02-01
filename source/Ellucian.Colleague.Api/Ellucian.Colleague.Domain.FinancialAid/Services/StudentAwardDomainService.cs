/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Exceptions;

namespace Ellucian.Colleague.Domain.FinancialAid.Services
{
    /// <summary>
    /// This class provides the business logic to update StudentAwards
    /// </summary>
    public static class StudentAwardDomainService
    {
        /// <summary>
        /// Verify the updates on studentAward objects for a year
        /// </summary>
        /// <param name="newStudentAwards">StudentAward objects containing the data with which to verify</param>
        /// <returns>The list of studentAward objects containing verified data</returns>
        public static IEnumerable<StudentAward> VerifyUpdatedStudentAwards(StudentAwardYear studentAwardYear, 
            IEnumerable<StudentAward> newStudentAwards, IEnumerable<StudentAward> currentStudentAwards, 
            IEnumerable<StudentLoanLimitation> studentLoanLimitations, bool suppressMaximumLoanLimits)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }
            if (newStudentAwards == null)
            {
                throw new ArgumentNullException("newStudentAward");
            }
            if (currentStudentAwards == null)
            {
                throw new ArgumentNullException("currentStudentAwards");
            }
            if (studentLoanLimitations == null)
            {
                throw new ArgumentNullException("studentLoanLimitations");
            }

            var newStudentAwardsForYear = newStudentAwards.Where(sa => sa.StudentAwardYear.Equals(studentAwardYear));
            if (newStudentAwardsForYear.Count() == 0)
            {
                throw new ApplicationException(string.Format("No new student awards for year {0} and studentId {1}", studentAwardYear.Code, studentAwardYear.StudentId));
            }


            var currentAwardsForYear = currentStudentAwards.Where(sa => sa.StudentAwardYear.Equals(studentAwardYear));
            if (currentAwardsForYear.Count() == 0)
            {
                throw new ApplicationException(string.Format("No current student awards for year {0} and studentId {1}", studentAwardYear.Code, studentAwardYear.StudentId));
            }

            var loanLimitationForYear = studentLoanLimitations.FirstOrDefault(l => l.AwardYear == studentAwardYear.Code && l.StudentId == studentAwardYear.StudentId);
            if (loanLimitationForYear == null)
            {
                throw new ApplicationException(string.Format("No Loan Limitation object exists for student {0} awardYear {1}", studentAwardYear.StudentId, studentAwardYear.Code));
            }
            

            var isDeclineActionTaken = false;
            foreach (var newStudentAward in newStudentAwardsForYear)
            {
                var currentAward = currentAwardsForYear.FirstOrDefault(a => a.Equals(newStudentAward));
                if (currentAward == null)
                {
                    throw new ApplicationException(string.Format("Cannot add new StudentAward {0}", newStudentAward.ToString()));
                }
                if (!string.IsNullOrEmpty(currentAward.PendingChangeRequestId))
                {
                    throw new ApplicationException(string.Format("Pending Change Request already exists for StudentAward {0} and AwardPackageChangeRequest id {1}", currentAward.ToString(), currentAward.PendingChangeRequestId));
                }

                foreach (var newStudentAwardPeriod in newStudentAward.StudentAwardPeriods)
                {
                    var currentStudentAwardPeriod = currentAward.StudentAwardPeriods.FirstOrDefault(p => p.Equals(newStudentAwardPeriod));
                    if (currentStudentAwardPeriod == null)
                    {
                        throw new ApplicationException(string.Format("Cannot add new StudentAwardPeriod {0}", newStudentAwardPeriod.ToString()));
                    }

                    //Check amount changes first. If no amount is specified in the newStudentAwardPeriod, just set it equal to the 
                    //current amount
                    if (!newStudentAwardPeriod.AwardAmount.HasValue)
                    {
                        newStudentAwardPeriod.AwardAmount = currentStudentAwardPeriod.AwardAmount;
                    }
                    else if (newStudentAwardPeriod.AwardAmount != currentStudentAwardPeriod.AwardAmount)
                    {
                        if (currentStudentAwardPeriod.StudentAwardYear.CurrentConfiguration.IsLoanAmountChangeRequestRequired && newStudentAward.Award.LoanType != null)
                        {
                            throw new UpdateRequiresReviewException(string.Format("Financial Aid Counselor review is required for loan amount changes. Create an AwardPackageChangeRequest instead"));
                        }
                    }

                    //Check status change second. If no status is specified in the newStudentAwardPeriod, just set it equal to the
                    //current status
                    if (newStudentAwardPeriod.AwardStatus == null)
                    {
                        newStudentAwardPeriod.AwardStatus = currentStudentAwardPeriod.AwardStatus;
                    }
                    else if (!newStudentAwardPeriod.AwardStatus.Equals(currentStudentAwardPeriod.AwardStatus))
                    {
                        if (newStudentAwardPeriod.AwardStatus.Category == AwardStatusCategory.Denied || newStudentAwardPeriod.AwardStatus.Category == AwardStatusCategory.Rejected)
                        {
                            isDeclineActionTaken = true;
                        }
                        if (!currentStudentAwardPeriod.IsStatusModifiable)
                        {
                            throw new ApplicationException(string.Format("StudentAwardPeriod {0} status cannot be modified", newStudentAwardPeriod.ToString()));
                        }
                        if (currentStudentAwardPeriod.StudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired &&
                            (newStudentAwardPeriod.AwardStatus.Category == AwardStatusCategory.Denied || newStudentAwardPeriod.AwardStatus.Category == AwardStatusCategory.Rejected))
                        {
                            throw new UpdateRequiresReviewException(string.Format("Financial Aid Counselor review is required for declined status changes. Create an AwardPackageChangeRequest instead."));
                        }
                    }
                }
            }

            if (!isDeclineActionTaken && !AreRequestedChangesWithinLoanLimits(newStudentAwardsForYear, currentAwardsForYear, loanLimitationForYear, suppressMaximumLoanLimits))
            {
                throw new ApplicationException(string.Format("Requested amounts cannot exceed loan limitation maximum values"));
            }

            return newStudentAwardsForYear;
        }

        /// <summary>
        /// This helper method determines whether the requested StudentAward changes meet the
        /// student loan limitation criteria.
        /// </summary>
        /// <param name="newStudentAwardsForYear">List of StudentAward objects containing requested changes</param>
        /// <param name="currentStudentAwardsForYear">student awards student has before submitting changes</param>
        /// <param name="studentLoanLimitation">student loan maximums information</param>
        /// <param name="suppressMaximumLoanLimits">flag to indicate whether to ignore loan maximums</param>
        /// <returns>True, if the requested changes meet the limitation criteria. False, otherwise.</returns>        
        private static bool AreRequestedChangesWithinLoanLimits(IEnumerable<StudentAward> newStudentAwardsForYear, 
            IEnumerable<StudentAward> currentStudentAwardsForYear, StudentLoanLimitation studentLoanLimitation, bool suppressMaximumLoanLimits)
        {
            //extract sub awards from currentAwards for year. extract all the award periods with non-null amount. sum the total sub amount
            var currentTotalSubAmount = currentStudentAwardsForYear.Where(a =>
                a.Award.LoanType.HasValue &&
                a.Award.LoanType.Value == LoanType.SubsidizedLoan).SelectMany(a =>
                    a.StudentAwardPeriods).Where(p =>
                        p.AwardStatus.Category != AwardStatusCategory.Denied && p.AwardStatus.Category != AwardStatusCategory.Rejected && p.AwardAmount.HasValue).Sum(p =>
                            p.AwardAmount.Value);

            //extract unsub awards from currentAwards for year. extract all the award periods with non-null amount. sum the total unsub amount
            var currentTotalUnsubAmount = currentStudentAwardsForYear.Where(a =>
                a.Award.LoanType.HasValue &&
                a.Award.LoanType.Value == LoanType.UnsubsidizedLoan).SelectMany(a =>
                    a.StudentAwardPeriods).Where(p =>
                        p.AwardStatus.Category != AwardStatusCategory.Denied && p.AwardStatus.Category != AwardStatusCategory.Rejected && p.AwardAmount.HasValue).Sum(p =>
                            p.AwardAmount.Value);

            //extract grad plus awards from currentAwards for year. extract all the award periods with non-null amounts. sum the total grad plus amount
            var currentTotalGradPlusAmount = currentStudentAwardsForYear.Where(a =>
                a.Award.LoanType.HasValue &&
                a.Award.LoanType.Value == LoanType.GraduatePlusLoan).SelectMany(a =>
                    a.StudentAwardPeriods).Where(p =>
                        p.AwardStatus.Category != AwardStatusCategory.Denied && p.AwardStatus.Category != AwardStatusCategory.Rejected && p.AwardAmount.HasValue).Sum(p =>
                            p.AwardAmount.Value);

            var potentialTotalSubAmount = currentTotalSubAmount;
            var potentialTotalUnsubAmount = currentTotalUnsubAmount;
            var potentialTotalLoanAmount = currentTotalSubAmount + currentTotalUnsubAmount + currentTotalGradPlusAmount;

            //Get the counts for sub and unsub loans
            var allSubsidizedLoansCount = newStudentAwardsForYear.Where(sa => sa.Award.LoanType == LoanType.SubsidizedLoan).Count();
            var allUnsubsidizedLoansCount = newStudentAwardsForYear.Where(sa => sa.Award.LoanType == LoanType.UnsubsidizedLoan).Count();
            
            foreach (var newStudentAward in newStudentAwardsForYear)
            {
                var currentStudentAward = currentStudentAwardsForYear.FirstOrDefault(sa => sa.Award.Equals(newStudentAward.Award));
                if (currentStudentAward == null)
                {
                    throw new KeyNotFoundException(string.Format("No StudentAward exists for year {0}, studentId {1} and awardId {2}", newStudentAward.StudentAwardYear.Code, newStudentAward.StudentId, newStudentAward.Award.Code));
                }

                var currentAmount = currentStudentAward.StudentAwardPeriods.Where(s => s.AwardAmount.HasValue).Sum(s => s.AwardAmount.Value);
                var newAmount = newStudentAward.StudentAwardPeriods.Where(s => s.AwardAmount.HasValue).Sum(s => s.AwardAmount.Value);

                if (currentStudentAward.Award.LoanType.HasValue &&
                    currentStudentAward.Award.LoanType.Value == LoanType.SubsidizedLoan)
                {
                    potentialTotalSubAmount = potentialTotalSubAmount - currentAmount + newAmount;
                    potentialTotalLoanAmount = potentialTotalLoanAmount - currentAmount + newAmount;
                    allSubsidizedLoansCount -= 1;
                }
                else if (currentStudentAward.Award.LoanType.HasValue &&
                    currentStudentAward.Award.LoanType.Value == LoanType.UnsubsidizedLoan)
                {
                    potentialTotalUnsubAmount = potentialTotalUnsubAmount - currentAmount + newAmount;
                    potentialTotalLoanAmount = potentialTotalLoanAmount - currentAmount + newAmount;
                    allUnsubsidizedLoansCount -= 1;
                }
                else if (currentStudentAward.Award.LoanType.HasValue &&
                    currentStudentAward.Award.LoanType.Value == LoanType.GraduatePlusLoan)
                {
                    potentialTotalLoanAmount = potentialTotalLoanAmount - currentAmount + newAmount;
                }

                //If the flag to suppress loan limits is false, and if the student override is set to Yes, check if loans are within their max limits. if the student override is true then no   more checking to be done.
                if (!suppressMaximumLoanLimits &&
                      !studentLoanLimitation.SuppressStudentMaximumAmounts == true)
                {
                    if (newStudentAward.Award.LoanType == LoanType.SubsidizedLoan && allSubsidizedLoansCount == 0)
                    {
                        if (potentialTotalSubAmount > studentLoanLimitation.SubsidizedMaximumAmount)
                        {
                            return false;
                        }
                    }

                    if (newStudentAward.Award.LoanType == LoanType.UnsubsidizedLoan && allUnsubsidizedLoansCount == 0)
                    {
                        if (potentialTotalUnsubAmount > studentLoanLimitation.UnsubsidizedMaximumAmount)
                        {
                            return false;
                        }
                    }
                }
            }
            //In case a negative amount was entered
            if (potentialTotalLoanAmount < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Assigns pending change requests to student awards
        /// </summary>
        /// <param name="studentAwards">list of student awards</param>
        /// <param name="awardPackageChangeRequests">list of award package change requests</param>
        public static void AssignPendingChangeRequests(IEnumerable<StudentAward> studentAwards, IEnumerable<AwardPackageChangeRequest> awardPackageChangeRequests)
        {
            if (studentAwards != null && awardPackageChangeRequests != null)
            {
                foreach (var studentAward in studentAwards)
                {
                    var changeRequest = awardPackageChangeRequests
                        .FirstOrDefault(cr => cr.IsForStudentAward(studentAward) && cr.AwardPeriodChangeRequests.Any(pcr => pcr.Status == AwardPackageChangeRequestStatus.Pending));

                    studentAward.PendingChangeRequestId = (changeRequest != null) ? changeRequest.Id : string.Empty;
                }
            }
        }

        /// <summary>
        /// Assign pending change request to a single student award
        /// </summary>
        /// <param name="studentAward">student award to assign request to</param>
        /// <param name="awardPackageChangeRequests">List of pending change requests</param>
        public static void AssignPendingChangeRequests(StudentAward studentAward, IEnumerable<AwardPackageChangeRequest> awardPackageChangeRequests)
        {
            if (studentAward != null && awardPackageChangeRequests != null)
            {
                var changeRequest = awardPackageChangeRequests
                        .FirstOrDefault(cr => cr.IsForStudentAward(studentAward) && cr.AwardPeriodChangeRequests.Any(pcr => pcr.Status == AwardPackageChangeRequestStatus.Pending));

                studentAward.PendingChangeRequestId = (changeRequest != null) ? changeRequest.Id : string.Empty;                
            }
        }

        /// <summary>
        /// Determines whether there is a change that sets any award periods status to "Accepted"
        /// </summary>
        /// <param name="updatedStudentAwards">updated student awards</param>
        /// <param name="originalStudentAwards">original student awards</param>
        /// <returns></returns>
        public static bool HasAcceptedStatusUpdate(this IEnumerable<StudentAward> updatedStudentAwards, IEnumerable<StudentAward> originalStudentAwards)
        {
            var originalPeriods = originalStudentAwards.SelectMany(orig => orig.StudentAwardPeriods).ToList();
            return updatedStudentAwards.SelectMany(sa => sa.StudentAwardPeriods).Any(updatedPeriod =>
                    {
                        var original = originalPeriods.FirstOrDefault(orig => orig.Equals(updatedPeriod));
                        return (original != null) ?
                            updatedPeriod.AwardStatus != null &&
                            updatedPeriod.AwardStatus.Category == AwardStatusCategory.Accepted &&
                            updatedPeriod.AwardStatus != original.AwardStatus :
                            false;
                    });
        }

        /// <summary>
        /// Determines whether there is a change that sets any award periods status to "Denied" or "Rejected"
        /// </summary>
        /// <param name="updatedStudentAwards">updated student awards</param>
        /// <param name="originalStudentAwards">original student awards</param>
        /// <returns></returns>
        public static bool HasDeclinedStatusUpdate(this IEnumerable<StudentAward> updatedStudentAwards, IEnumerable<StudentAward> originalStudentAwards)
        {
            var originalPeriods = originalStudentAwards.SelectMany(orig => orig.StudentAwardPeriods).ToList();
            return updatedStudentAwards.SelectMany(sa => sa.StudentAwardPeriods).Any(updatedPeriod =>
            {
                var original = originalPeriods.FirstOrDefault(orig => orig.Equals(updatedPeriod));
                return (original != null) ?
                    updatedPeriod.AwardStatus != null &&
                    (updatedPeriod.AwardStatus.Category == AwardStatusCategory.Denied || updatedPeriod.AwardStatus.Category == AwardStatusCategory.Rejected) &&
                    updatedPeriod.AwardStatus != original.AwardStatus :
                    false;
            });
        }

        /// <summary>
        /// Determines if there is an amount change for any of the award periods
        /// </summary>
        /// <param name="updatedStudentAwards">updated student awards</param>
        /// <param name="originalStudentAwards">original student awards</param>
        /// <returns></returns>
        public static bool HasAmountChange(this IEnumerable<StudentAward> updatedStudentAwards, IEnumerable<StudentAward> originalStudentAwards)
        {
            var originalPeriods = originalStudentAwards.SelectMany(orig => orig.StudentAwardPeriods).ToList();
            return updatedStudentAwards.SelectMany(sa => sa.StudentAwardPeriods).Any(updatedPeriod =>
            {
                var original = originalPeriods.FirstOrDefault(orig => orig.Equals(updatedPeriod));
                return (original != null) ?
                    updatedPeriod.AwardAmount.HasValue &&
                    updatedPeriod.AwardAmount != original.AwardAmount :
                    false;
            });
        }

        /// <summary>
        /// Gets Communications for updated awards
        /// </summary>
        /// <param name="studentAwardYear">student award year</param>
        /// <param name="updatedStudentAwards">updated student awards</param>
        /// <param name="originalStudentAwards">original student awards</param>
        /// <returns>List of Communication entities</returns>
        public static IEnumerable<Communication> GetCommunicationsForUpdatedAwards(StudentAwardYear studentAwardYear, IEnumerable<StudentAward> updatedStudentAwards, IEnumerable<StudentAward> originalStudentAwards)
        {
            var communications = new List<Communication>();
            if (!string.IsNullOrEmpty(studentAwardYear.CurrentConfiguration.AcceptedAwardCommunicationCode) && updatedStudentAwards.HasAcceptedStatusUpdate(originalStudentAwards))
            {
                communications.Add(new Communication(studentAwardYear.StudentId, studentAwardYear.CurrentConfiguration.AcceptedAwardCommunicationCode)
                    {
                        StatusCode = studentAwardYear.CurrentConfiguration.AcceptedAwardCommunicationStatus,
                        StatusDate = DateTime.Today
                    });
            }
            if (!string.IsNullOrEmpty(studentAwardYear.CurrentConfiguration.RejectedAwardCommunicationCode) && updatedStudentAwards.HasDeclinedStatusUpdate(originalStudentAwards))
            {
                communications.Add(new Communication(studentAwardYear.StudentId, studentAwardYear.CurrentConfiguration.RejectedAwardCommunicationCode)
                {
                    StatusCode = studentAwardYear.CurrentConfiguration.RejectedAwardCommunicationStatus,
                    StatusDate = DateTime.Today
                });
            }
            if (!string.IsNullOrEmpty(studentAwardYear.CurrentConfiguration.LoanChangeCommunicationCode) && updatedStudentAwards.HasAmountChange(originalStudentAwards))
            {
                communications.Add(new Communication(studentAwardYear.StudentId, studentAwardYear.CurrentConfiguration.LoanChangeCommunicationCode)
                {
                    StatusCode = studentAwardYear.CurrentConfiguration.LoanChangeCommunicationStatus,
                    StatusDate = DateTime.Today
                });
            }

            return communications;
        }
    }
}
