/*Copyright 2015-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Services
{
    public static class AwardPackageChangeRequestDomainService
    {
        public static AwardPackageChangeRequest VerifyAwardPackageChangeRequest(AwardPackageChangeRequest awardPackageChangeRequest, StudentAward studentAward,
            IEnumerable<AwardStatus> awardStatuses, IEnumerable<StudentAward> studentAwardsForYear = null, bool updateAllAwardsFlag = false)
        {
            if (studentAward == null)
            {
                throw new ArgumentNullException("studentAward");
            }
            if (awardPackageChangeRequest == null)
            {
                throw new ArgumentNullException("awardPackageChangeRequest");
            }
            if (awardStatuses == null)
            {
                throw new ArgumentNullException("awardStatuses");
            }
            if (awardPackageChangeRequest.StudentId != studentAward.StudentId)
            {
                throw new ApplicationException("StudentId of AwardPackageChangeRequest must match StudentId of StudentAward");
            }
            if (awardPackageChangeRequest.AwardYearId != studentAward.StudentAwardYear.Code)
            {
                throw new ApplicationException("AwardYearId of AwardPackageChangeRequest must match AwardYear of StudentAward");
            }
            if (awardPackageChangeRequest.AwardId != studentAward.Award.Code)
            {
                throw new ApplicationException("AwardId of AwardPackageChangeRequest must match Award of StudentAward");
            }
            if (awardPackageChangeRequest.AwardPeriodChangeRequests == null || awardPackageChangeRequest.AwardPeriodChangeRequests.Count() == 0)
            {
                throw new ApplicationException("AwardPeriodChangeRequests are required");
            }
            if (awardPackageChangeRequest.AwardPeriodChangeRequests.All(pcr => !pcr.NewAmount.HasValue && pcr.NewAwardStatusId == null))
            {
                throw new ApplicationException("At least one AwardPeriodChangeRequest must specify a new amount or new status");
            }
            if (!string.IsNullOrEmpty(studentAward.PendingChangeRequestId))
            {
                throw new ExistingResourceException(
                    string.Format("StudentAward {0} already has an existing pending change request id {1}", studentAward.ToString(), studentAward.PendingChangeRequestId),
                    studentAward.PendingChangeRequestId);
            }
            //If the incoming award is unsubsidized loan and there is a pending subsidized loan on the record for the year, throw an exception
            //since all subsidized loans must be accepted/rejected before unsubsidized loan can be taken action on
            if (!awardPackageChangeRequest.OverrideUnsubsidizedLoanCheck && studentAward.Award.LoanType.HasValue && studentAward.Award.LoanType.Value == LoanType.UnsubsidizedLoan &&
                studentAwardsForYear != null && studentAwardsForYear.Any(sa =>
                    sa.Award.LoanType.HasValue && sa.Award.LoanType.Value == LoanType.SubsidizedLoan && sa.StudentAwardPeriods.Any(sap => sap.IsStatusModifiable
                                                                                                                && sap.AwardStatus.Category != AwardStatusCategory.Accepted)))
            {
                throw new InvalidOperationException("All subsidized loans must be accepted/rejected before taking action on an unsubsidized loan");
            }


            var anyAmountChangeRequested = awardPackageChangeRequest.AwardPeriodChangeRequests.Join(studentAward.StudentAwardPeriods, cr => cr.AwardPeriodId, p => p.AwardPeriodId,
                (cr, period) =>
                    cr.NewAmount.HasValue && cr.NewAmount != period.AwardAmount).Any(b => b);

            if (anyAmountChangeRequested && !studentAward.Award.LoanType.HasValue)
            {
                throw new ApplicationException("Amount change requests can only be processed for loans at this time");
            }
            if (anyAmountChangeRequested && !studentAward.StudentAwardYear.CurrentConfiguration.IsLoanAmountChangeRequestRequired)
            {
                throw new ApplicationException("Financial Aid Counselors do not need to review loan amount changes for this student. Try updating the StudentAward directly");
            }


            // What we are trying to do with this edit is to weed out an invalid request to create a New.Loan.Request record when it is not needed.
            // If we have a student want to decline an award and Declined Status Changes do not need to be reviewed, then we don't need to create a
            // New.Loan.Request record in Colleague.

            var anyStatusChangeRequested = awardPackageChangeRequest.AwardPeriodChangeRequests.Join(studentAward.StudentAwardPeriods, cr => cr.AwardPeriodId, p => p.AwardPeriodId,
                (cr, period) =>
                    !string.IsNullOrEmpty(cr.NewAwardStatusId) && cr.NewAwardStatusId != period.AwardStatus.Code).Any(b => b);

            var anyDeclinedStatuses = awardPackageChangeRequest.AwardPeriodChangeRequests.Join(studentAward.StudentAwardPeriods, cr => cr.AwardPeriodId, p => p.AwardPeriodId,
                (cr, period) =>
                    !string.IsNullOrEmpty(cr.NewAwardStatusId) && cr.NewAwardStatusId != period.AwardStatus.Code && cr.NewAwardStatusId == "D").Any(b => b);

            // if (anyStatusChangeRequested && !studentAward.StudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired)
            if (anyStatusChangeRequested)
            {
                if (anyDeclinedStatuses == true && !studentAward.StudentAwardYear.CurrentConfiguration.IsDeclinedStatusChangeRequestRequired)
                {
                    throw new ApplicationException("Financial Aid Counselors do not need to review Declined status changes for this student. Try updating the StudentAward directly");
                }
            }

            foreach (var awardPeriodChangeRequest in awardPackageChangeRequest.AwardPeriodChangeRequests)
            {
                if (string.IsNullOrEmpty(awardPeriodChangeRequest.AwardPeriodId))
                {
                    throw new ApplicationException("AwardPeriodId attribute of AwardPeriodChangeRequest is required");
                }

                var studentAwardPeriod = studentAward.StudentAwardPeriods.FirstOrDefault(p => p.AwardPeriodId == awardPeriodChangeRequest.AwardPeriodId);
                if (studentAwardPeriod == null)
                {
                    awardPeriodChangeRequest.Status = AwardPackageChangeRequestStatus.RejectedBySystem;
                    awardPeriodChangeRequest.StatusReason = "Cannot process request for award period because no matching StudentAwardPeriod exists";
                }
                else
                {
                    //This is more the way we need to go, but tests are failing and it's too close to 1.7 release date.
                    ////if amount change, process
                    //if (!string.IsNullOrEmpty(awardPeriodChangeRequest.NewAwardStatusId) && awardPeriodChangeRequest.NewAwardStatusId != studentAwardPeriod.AwardStatus.Code)
                    //{
                    //    ProcessPeriodStatusChangeRequest(awardPeriodChangeRequest, awardStatuses);
                    //}
                    //else if (awardPeriodChangeRequest.NewAmount.HasValue && awardPeriodChangeRequest.NewAmount != studentAwardPeriod.AwardAmount)
                    //{
                    //    ProcessPeriodAmountChangeRequest(awardPeriodChangeRequest);
                    //}

                    //This is probably not the right way to do this, but it's working now based on how SS is sending data to the API.
                    if (anyAmountChangeRequested)
                    {
                        ProcessPeriodAmountChangeRequest(awardPeriodChangeRequest);
                    }
                    //process a status change request 
                    else if (anyStatusChangeRequested)
                    {
                        ProcessPeriodStatusChangeRequest(awardPeriodChangeRequest, awardStatuses);
                    }
                }
            }

            if (!awardPackageChangeRequest.AwardPeriodChangeRequests.Any(cr => cr.Status == AwardPackageChangeRequestStatus.Pending))
            {
                throw new ApplicationException("All the requested changes were rejected");
            }

            return awardPackageChangeRequest;
        }

        private static void ProcessPeriodAmountChangeRequest(AwardPeriodChangeRequest awardPeriodChangeRequest)
        {
            //if an amount change is requested
            if (awardPeriodChangeRequest.NewAmount.HasValue)
            {
                if (awardPeriodChangeRequest.NewAmount.Value < 0)
                {
                    //Reject if the amount is less than zero
                    awardPeriodChangeRequest.Status = AwardPackageChangeRequestStatus.RejectedBySystem;
                    awardPeriodChangeRequest.StatusReason = "Cannot change amount to a value less than zero";
                }
                else
                {
                    //set the status to pending
                    awardPeriodChangeRequest.Status = AwardPackageChangeRequestStatus.Pending;
                }
            }
        }

        private static void ProcessPeriodStatusChangeRequest(AwardPeriodChangeRequest awardPeriodChangeRequest, IEnumerable<AwardStatus> awardStatuses)
        {
            //if a status change is requested
            if (!string.IsNullOrEmpty(awardPeriodChangeRequest.NewAwardStatusId))
            {
                var awardStatusRequested = awardStatuses.FirstOrDefault(s => s.Code == awardPeriodChangeRequest.NewAwardStatusId);
                if (awardStatusRequested == null)
                {
                    //reject if the NewStatusId is not valid
                    awardPeriodChangeRequest.Status = AwardPackageChangeRequestStatus.RejectedBySystem;
                    awardPeriodChangeRequest.StatusReason = "Cannot process status change request for award period because no matching AwardStatus exists";
                }
                else if (awardStatusRequested.Category != AwardStatusCategory.Rejected && awardStatusRequested.Category != AwardStatusCategory.Denied)
                {
                    //reject if the requested status isn't rejected or denied.
                    awardPeriodChangeRequest.Status = AwardPackageChangeRequestStatus.RejectedBySystem;
                    awardPeriodChangeRequest.StatusReason = "Change Requests can only be submitted for Rejected or Denied AwardStatusCategories at this time";
                }
                else
                {
                    //set the status to pending only
                    awardPeriodChangeRequest.Status = AwardPackageChangeRequestStatus.Pending;
                }
            }
        }

        /// <summary>
        /// Gets communication entities associated with award change requests
        /// to be sent to a student 
        /// </summary>
        /// <param name="awardPackageChangeRequest">award package change request in question</param>
        /// <param name="studentAward">student award this request is made for</param>
        /// <param name="awardStatuses">list of all reference award statuses</param>
        /// <returns>List of Communication entities</returns>
        public static IEnumerable<Communication> GetCommunications(AwardPackageChangeRequest awardPackageChangeRequest, StudentAward studentAward, IEnumerable<AwardStatus> awardStatuses)
        {
            var communications = new List<Communication>();
            if (!string.IsNullOrEmpty(studentAward.StudentAwardYear.CurrentConfiguration.RejectedAwardCommunicationCode) &&
                awardPackageChangeRequest.AwardPeriodChangeRequests.Any(p => p != null && !string.IsNullOrEmpty(p.NewAwardStatusId)
                && p.Status == AwardPackageChangeRequestStatus.Pending))
            {
                //Only add a rejected award communication code if the taken action is reject/deny
                var newAwardStatusCategory = DetermineChangeRequestAwardStatusCategory(awardPackageChangeRequest, awardStatuses);
                if (newAwardStatusCategory == AwardStatusCategory.Denied || newAwardStatusCategory == AwardStatusCategory.Rejected)
                {
                    communications.Add(new Communication(studentAward.StudentId, studentAward.StudentAwardYear.CurrentConfiguration.RejectedAwardCommunicationCode)
                    {
                        StatusCode = studentAward.StudentAwardYear.CurrentConfiguration.RejectedAwardCommunicationStatus,
                        StatusDate = DateTime.Today
                    });
                }
            }

            if (!string.IsNullOrEmpty(studentAward.StudentAwardYear.CurrentConfiguration.LoanChangeCommunicationCode) &&
                 awardPackageChangeRequest.AwardPeriodChangeRequests.Any(p => p != null && p.NewAmount.HasValue && p.Status == AwardPackageChangeRequestStatus.Pending))
            {
                communications.Add(new Communication(studentAward.StudentId, studentAward.StudentAwardYear.CurrentConfiguration.LoanChangeCommunicationCode)
                {
                    StatusCode = studentAward.StudentAwardYear.CurrentConfiguration.LoanChangeCommunicationStatus,
                    StatusDate = DateTime.Today
                });
            }

            return communications;
        }

        /// <summary>
        /// Determine the award package change request new status category
        /// </summary>
        /// <param name="awardPackageChangeRequest">award package change request</param>
        /// <param name="awardStatuses">List of reference award statuses</param>
        /// <returns>AwardStatusCategory or null</returns>
        private static AwardStatusCategory? DetermineChangeRequestAwardStatusCategory(AwardPackageChangeRequest awardPackageChangeRequest, IEnumerable<AwardStatus> awardStatuses)
        {
            AwardStatusCategory? statusCategory = null;

            //Get the new status id - all affected periods will have the same status id since we 
            //either accept/reject all marked periods in one transaction
            var awardPeriodWithNewStatus = awardPackageChangeRequest.AwardPeriodChangeRequests.Any() ? awardPackageChangeRequest.AwardPeriodChangeRequests
                .Where(p => p != null && !string.IsNullOrEmpty(p.NewAwardStatusId)).First() : null;
            if (awardPeriodWithNewStatus != null && awardStatuses != null && awardStatuses.Any())
            {
                var matchingAwardStatus = awardStatuses.FirstOrDefault(s => s != null && s.Code == awardPeriodWithNewStatus.NewAwardStatusId);
                statusCategory = matchingAwardStatus != null ? matchingAwardStatus.Category : (AwardStatusCategory?)null;
            }
            return statusCategory;
        }
    }
}
