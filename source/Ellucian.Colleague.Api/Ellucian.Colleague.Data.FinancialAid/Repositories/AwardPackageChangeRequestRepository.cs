/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AwardPackageChangeRequestRepository : BaseColleagueRepository, IAwardPackageChangeRequestRepository
    {
        private readonly string colleagueTimeZone;
                
        public AwardPackageChangeRequestRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        public async Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            return (await BuildStudentLoanAmountChangeRequestsAsync(studentId)).Concat(
                (await BuildStudentDeclinedStatusChangeRequestsAsync(studentId)));
        }

        /// <summary>
        /// Gets existing pending award package change requests. Use for getting change requests for a single award.
        /// If award is not a loan - no need to retrieve loan change requests
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAward">student award to assign the request to</param>
        /// <returns>List of award package change requests</returns>
        public async Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId, StudentAward studentAward)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAward == null)
            {
                throw new ArgumentNullException("studentAward");
            }
            IEnumerable<AwardPackageChangeRequest> loanAmountChangeRequests = new List<AwardPackageChangeRequest>();
            
            //Get StudentLoanAmountChangeRequests only if award category type is unknown or of type "Loan"
            if (!studentAward.Award.AwardCategoryType.HasValue || studentAward.Award.AwardCategoryType.Value == AwardCategoryType.Loan)
            {
                loanAmountChangeRequests = await BuildStudentLoanAmountChangeRequestsAsync(studentId);
            }

            return loanAmountChangeRequests.Concat(
                (await BuildStudentDeclinedStatusChangeRequestsAsync(studentId)));
        }

        /// <summary>
        /// Gets AwardPackageChangeRequests for loan amount changes
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>List of AwardPackageChangeRequest entities</returns>
        private async Task<IEnumerable<AwardPackageChangeRequest>> BuildStudentLoanAmountChangeRequestsAsync(string studentId)
        {
            var awardPackageChangeRequests = new List<AwardPackageChangeRequest>();
            
            var crCriteria = string.Format("WITH LNCR.STUDENT.ID EQ '" + studentId + "'");
            
            var loanChangeRequestData = await DataReader.BulkReadRecordAsync<LoanChgRequest>(crCriteria);
            
            if (loanChangeRequestData != null && loanChangeRequestData.Count() > 0)
            {
                foreach (var loanChangeRequestRecord in loanChangeRequestData)
                {
                    try
                    {
                        awardPackageChangeRequests.Add(
                            new AwardPackageChangeRequest(loanChangeRequestRecord.Recordkey, studentId, loanChangeRequestRecord.LncrAwardYear, loanChangeRequestRecord.LncrAward)
                            {
                                AssignedToCounselorId = loanChangeRequestRecord.LncrAssignedToId,
                                CreateDateTime = loanChangeRequestRecord.LoanChgRequestAddtime.ToPointInTimeDateTimeOffset(loanChangeRequestRecord.LoanChgRequestAdddate, colleagueTimeZone),
                                AwardPeriodChangeRequests = loanChangeRequestRecord.LncrChangedInfoEntityAssociation.Select(entity =>
                                    new AwardPeriodChangeRequest(entity.LncrAwardPeriodsAssocMember)
                                    {
                                        Status = TranslateRequestStatusCode(loanChangeRequestRecord.LncrCurrentStatus),
                                        NewAmount = entity.LncrNewLoanAmountsAssocMember,
                                        NewAwardStatusId = entity.LncrNewLoanActionsAssocMember,
                                    }).ToList()
                            });
                    }
                    catch (Exception e)
                    {
                        LogDataError("LOAN_CHG_REQUEST", loanChangeRequestRecord.Recordkey, loanChangeRequestRecord, e);
                    }
                }                
            }
            return awardPackageChangeRequests;
        }

        /// <summary>
        /// Gets AwardPackageChangeRequests for declined awards
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>List of AwardPackageChangeRequest entities</returns>
        private async Task<IEnumerable<AwardPackageChangeRequest>> BuildStudentDeclinedStatusChangeRequestsAsync(string studentId)
        {
            var awardPackageChangeRequests = new List<AwardPackageChangeRequest>();
            
            var crCriteria = string.Format("WITH DAWR.STUDENT.ID EQ '" + studentId + "'");
            
            var declinedStatusRequestData = await DataReader.BulkReadRecordAsync<DeclAwdRequest>(crCriteria);
            
            if (declinedStatusRequestData != null && declinedStatusRequestData.Count() > 0)
            {
                foreach (var declinedStatusRecord in declinedStatusRequestData)
                {
                    try
                    {
                        awardPackageChangeRequests.Add(
                            new AwardPackageChangeRequest(declinedStatusRecord.Recordkey, studentId, declinedStatusRecord.DawrAwardYear, declinedStatusRecord.DawrAward)
                            {
                                AssignedToCounselorId = declinedStatusRecord.DawrAssignedToId,
                                CreateDateTime = declinedStatusRecord.DeclAwdRequestAddtime.ToPointInTimeDateTimeOffset(declinedStatusRecord.DeclAwdRequestAdddate, colleagueTimeZone),
                                AwardPeriodChangeRequests = declinedStatusRecord.DawrDeclinedInfoEntityAssociation.Select(entity =>
                                    new AwardPeriodChangeRequest(entity.DawrAwardPeriodsAssocMember)
                                    {
                                        Status = TranslateRequestStatusCode(declinedStatusRecord.DawrCurrentStatus),
                                        NewAwardStatusId = declinedStatusRecord.DawrDeclinedActionStatus
                                    }).ToList()
                            });
                    }
                    catch (Exception e)
                    {
                        LogDataError("DECL_AWD_REQUEST", declinedStatusRecord.Recordkey, declinedStatusRecord, e);
                    }
                }
            }

            return awardPackageChangeRequests;
        }

        private AwardPackageChangeRequestStatus TranslateRequestStatusCode(string requestStatus)
        {
            if (string.IsNullOrEmpty(requestStatus))
            {
                throw new ArgumentNullException("requestStatus");
            }
            switch (requestStatus.ToUpper())
            {
                case "A":
                    return AwardPackageChangeRequestStatus.Accepted;
                case "P":
                    return AwardPackageChangeRequestStatus.Pending;
                case "R":
                    return AwardPackageChangeRequestStatus.RejectedByCounselor;
                default:
                    var message = string.Format("Unable to translate request status {0}", requestStatus);
                    logger.Error(message);
                    throw new ApplicationException(message);
            }

        }

        /// <summary>
        /// Creates an awaward package change request
        /// </summary>
        /// <param name="awardPackageChangeRequest">passed in award package change request data</param>
        /// <param name="originalStudentAward">original student award the request is for</param>
        /// <returns>AwardPackageChangeRequest entity</returns>
        public async Task<AwardPackageChangeRequest> CreateAwardPackageChangeRequestAsync(AwardPackageChangeRequest awardPackageChangeRequest, StudentAward originalStudentAward)
        {
            if (awardPackageChangeRequest == null)
            {
                throw new ArgumentNullException("awardPackageChangeRequest");
            }
            if (originalStudentAward == null)
            {
                throw new ArgumentNullException("originalStudentAward");
            }

            var finAidRecord = await DataReader.ReadRecordAsync<FinAid>(awardPackageChangeRequest.StudentId);
            if (finAidRecord == null)
            {
                string message = string.Format("No FIN.AID record found for {0}", awardPackageChangeRequest.StudentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }
            
            //Get fin aid counselor id
            string financialAidCounselorId = string.Empty;
            if (finAidRecord != null &&
                                    finAidRecord.FaCounselorsEntityAssociation != null &&
                                    finAidRecord.FaCounselorsEntityAssociation.Any())
            {
                foreach (var faCounselorEntity in finAidRecord.FaCounselorsEntityAssociation)
                {
                    if (
                        (!faCounselorEntity.FaCounselorEndDateAssocMember.HasValue ||
                        DateTime.Today <= faCounselorEntity.FaCounselorEndDateAssocMember.Value) &&
                        (!faCounselorEntity.FaCounselorStartDateAssocMember.HasValue ||
                        DateTime.Today >= faCounselorEntity.FaCounselorStartDateAssocMember.Value)
                       )
                    {
                        financialAidCounselorId = faCounselorEntity.FaCounselorAssocMember;
                        break;
                    }
                }
            }

            awardPackageChangeRequest.AssignedToCounselorId = financialAidCounselorId;
            var pendingPeriodAmountChangeRequests = awardPackageChangeRequest.AwardPeriodChangeRequests.Where(pcr => pcr.NewAmount.HasValue && pcr.Status == AwardPackageChangeRequestStatus.Pending).ToList();
            if (pendingPeriodAmountChangeRequests.Count() > 0)
            {
                var request = new CreateAmountChangeRequestRequest()
                {
                    StudentId = awardPackageChangeRequest.StudentId,
                    AwardYear = awardPackageChangeRequest.AwardYearId,
                    AwardId = awardPackageChangeRequest.AwardId,
                    AssignedToId = awardPackageChangeRequest.AssignedToCounselorId,
                    AwardPeriodChange = new List<AwardPeriodChange>(),
                };
                foreach (var pendingPeriodChangeRequest in pendingPeriodAmountChangeRequests)
                {
                    var originalStudentAwardPeriod = originalStudentAward.StudentAwardPeriods.FirstOrDefault(p => p.AwardPeriodId == pendingPeriodChangeRequest.AwardPeriodId);
                    if (originalStudentAwardPeriod != null)
                    {
                        request.AwardPeriodChange.Add(
                            new AwardPeriodChange()
                            {
                                AwardPeriod = pendingPeriodChangeRequest.AwardPeriodId,
                                NewLoanAmount = pendingPeriodChangeRequest.NewAmount,
                                OriginalLoanAmount = originalStudentAwardPeriod.AwardAmount,
                                NewStatus = pendingPeriodChangeRequest.NewAwardStatusId,
                                OriginalStatus = (originalStudentAwardPeriod.AwardStatus != null) ? originalStudentAwardPeriod.AwardStatus.Code : string.Empty
                            });
                    }
                    else
                    {
                        logger.Warn(string.Format("StudentAwardPeriod {0} does not exist", pendingPeriodChangeRequest.AwardPeriodId));
                    }
                }
                
                var response = await transactionInvoker.ExecuteAsync<CreateAmountChangeRequestRequest, CreateAmountChangeRequestResponse>(request);
                
                awardPackageChangeRequest.Id = response.ChangeRequestId;
                awardPackageChangeRequest.CreateDateTime = DateTimeOffset.Now;

                return awardPackageChangeRequest;
            }

            var pendingPeriodStatusChangeRequests = awardPackageChangeRequest.AwardPeriodChangeRequests.Where(pcr => !string.IsNullOrEmpty(pcr.NewAwardStatusId) && pcr.Status == AwardPackageChangeRequestStatus.Pending).ToList();
            if (pendingPeriodStatusChangeRequests.Count() > 0)
            {
                var request = new CreateStatusChangeRequestRequest()
                {
                    StudentId = awardPackageChangeRequest.StudentId,
                    AwardYear = awardPackageChangeRequest.AwardYearId,
                    AwardId = awardPackageChangeRequest.AwardId,
                    AssignedToId = awardPackageChangeRequest.AssignedToCounselorId,
                    AwardPeriodData = new List<AwardPeriodData>(),
                    NewStatusId = pendingPeriodStatusChangeRequests.First().NewAwardStatusId
                };
                foreach (var pendingPeriodChangeRequest in pendingPeriodStatusChangeRequests)
                {
                    var originalStudentAwardPeriod = originalStudentAward.StudentAwardPeriods.FirstOrDefault(p => p.AwardPeriodId == pendingPeriodChangeRequest.AwardPeriodId);
                    if (originalStudentAwardPeriod != null)
                    {
                        request.AwardPeriodData.Add(
                            new AwardPeriodData()
                            {
                                AwardPeriodId = pendingPeriodChangeRequest.AwardPeriodId,
                                AwardPeriodAmount = originalStudentAwardPeriod.AwardAmount,
                                AwardPeriodOrigStatusId = (originalStudentAwardPeriod.AwardStatus != null) ? originalStudentAwardPeriod.AwardStatus.Code : string.Empty
                            });
                    }
                    else
                    {
                        logger.Warn(string.Format("StudentAwardPeriod {0} does not exist", pendingPeriodChangeRequest.AwardPeriodId));
                    }
                }
                
                var response = await transactionInvoker.ExecuteAsync<CreateStatusChangeRequestRequest, CreateStatusChangeRequestResponse>(request);
                
                awardPackageChangeRequest.Id = response.ChangeRequestId;
                awardPackageChangeRequest.CreateDateTime = DateTimeOffset.Now;

                return awardPackageChangeRequest;
            }

            return awardPackageChangeRequest;
        }
    }
}
