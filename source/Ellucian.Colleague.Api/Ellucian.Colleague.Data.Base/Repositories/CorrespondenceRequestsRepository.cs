// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository class gets CorrespondenceRequests from the colleague database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CorrespondenceRequestsRepository : BaseColleagueRepository, ICorrespondenceRequestsRepository
    {
        /// <summary>
        /// Dependency Injection constructor for the CorrespondenceRequestsRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public CorrespondenceRequestsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get all of a person's correspondence requests
        /// </summary>
        /// <param name="personId">The Id of the person for whom to get correspondence requests</param>
        /// <returns>A list of CorrespondenceRequest objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if the personId argument is null or empty</exception>
        public async Task<IEnumerable<CorrespondenceRequest>> GetCorrespondenceRequestsAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            var correspondenceRequestList = new List<CorrespondenceRequest>();

            var mailingData = await DataReader.ReadRecordAsync<Mailing>(personId);
            if (mailingData == null)
            {
                logger.Debug(string.Format("Person with ID {0} has no MAILING record. Unable to retrieve correspondence requests.", personId));
                return correspondenceRequestList;
            }

            if (mailingData.ChCorrEntityAssociation.Count() == 0 && mailingData.MailingCurrentCrcCode == null)
            {
                logger.Debug(string.Format("No correspondence requests found on MAILING record for Person with ID {0}", personId));
                return correspondenceRequestList;
            }

            if (mailingData.ChCorrEntityAssociation.Count() > 0)
            {
                foreach (var chCorr in mailingData.ChCorrEntityAssociation)
                {
                    try
                    {
                        var correspondenceRequest = CreateCorrespondenceRequest(
                                         personId,
                                         chCorr.MailingCorrReceivedAssocMember,
                                         chCorr.MailingCorrRecvdActDtAssocMember,
                                         chCorr.MailingCorrReceivedDateAssocMember,
                                         chCorr.MailingCorrRecvdInstanceAssocMember,
                                         chCorr.MailingCorrRecvdStatusAssocMember);

                        correspondenceRequestList.Add(correspondenceRequest);

                    }
                    catch (Exception e)
                    {
                        logger.Info(e, e.Message);
                    }
                }
            }

            if (mailingData.MailingCurrentCrcCode != null && mailingData.MailingCurrentCrcCode.Count() > 0)
            {
                var coreqIds = mailingData.MailingCurrentCrcCode.Where(code => !string.IsNullOrEmpty(code)).Select(code => string.Format("{0}*{1}", personId, code));
                var coreqRecords = await DataReader.BulkReadRecordAsync<Coreq>(coreqIds.ToArray());

                foreach (var coreq in coreqRecords)
                {
                    foreach (var request in coreq.CoreqRequestsEntityAssociation)
                    {
                        //see if we have a correspondence request with same code, status date, and instance in correspondenceRequestList
                        var matchingCorrespondenceRequest = correspondenceRequestList
                            .FirstOrDefault(sd => sd.Code == request.CoreqCcCodeAssocMember &&
                                                  sd.StatusDate == request.CoreqCcDateAssocMember &&
                                                  sd.Instance == request.CoreqCcInstanceAssocMember);

                        // If we found a duplicate, don't add it to the list again
                        if (matchingCorrespondenceRequest == null)
                        {
                            try
                            {
                                var correspondenceRequest = CreateCorrespondenceRequest(
                                                    personId,
                                                    request.CoreqCcCodeAssocMember,
                                                    request.CoreqCcExpActDtAssocMember,
                                                    request.CoreqCcDateAssocMember,
                                                    request.CoreqCcInstanceAssocMember,
                                                    request.CoreqCcStatusAssocMember);
                                correspondenceRequestList.Add(correspondenceRequest);
                            }
                            catch (Exception e)
                            {
                                logger.Info(e, e.Message);
                            }
                        }
                    }
                }

            }

            return correspondenceRequestList;
        }

        /// <summary>
        /// Creates a correspondence request entity and assigns its properties
        /// </summary>
        /// <param name="personId">person id</param>
        /// <param name="code">correspondence request code</param>
        /// <param name="dueDate">correspondence request due date</param>
        /// <param name="statusDate">correspondence request status date</param>
        /// <param name="instanceDescription">correspondence request instance description</param>
        /// <param name="statusCode">correspondence request status code</param>        
        /// <returns>CorrespondenceRequest entity</returns>
        private CorrespondenceRequest CreateCorrespondenceRequest(string personId, string code, DateTime? dueDate, DateTime? statusDate, string instanceDescription, string statusCode)
        {
            var correspondenceRequest = new CorrespondenceRequest(personId, code);
            correspondenceRequest.DueDate = dueDate;
            correspondenceRequest.StatusDate = statusDate;
            correspondenceRequest.Instance = instanceDescription;

            var correspondenceRequestStatuses = GetCorrespondenceRequestStatuses();
            ApplValcodesVals statusCodeObject = null;
            if (!string.IsNullOrEmpty(statusCode) && correspondenceRequestStatuses != null && correspondenceRequestStatuses.ValsEntityAssociation != null)
            {
                statusCodeObject = correspondenceRequestStatuses.ValsEntityAssociation.FirstOrDefault(
                        v => v.ValInternalCodeAssocMember != null && v.ValInternalCodeAssocMember.ToUpper() == statusCode.ToUpper());
                if (statusCodeObject == null)
                {
                    logger.Error(string.Format("Correspondence request has status {0} not present in CORR.STATUSES. Using default status values.", statusCode));
                }
            }

            if (statusCodeObject == null)
            {
                correspondenceRequest.Status = CorrespondenceRequestStatus.Incomplete;
                correspondenceRequest.StatusDescription = "";
            }
            else
            {
                switch (statusCodeObject.ValActionCode1AssocMember)
                {
                    case "0":
                        correspondenceRequest.Status = CorrespondenceRequestStatus.Waived;
                        break;
                    case "1":
                        correspondenceRequest.Status = CorrespondenceRequestStatus.Received;
                        break;
                    default:
                        correspondenceRequest.Status = CorrespondenceRequestStatus.Incomplete;
                        break;
                }

                correspondenceRequest.StatusDescription = statusCodeObject.ValExternalRepresentationAssocMember;
            }

            return correspondenceRequest;
        }

        /// <summary>
        /// Helper method to get correspondence request statuses
        /// </summary>
        /// <returns>valcode values for correspondence request statuses</returns>
        private ApplValcodes GetCorrespondenceRequestStatuses()
        {
            return GetOrAddToCache<ApplValcodes>("DocumentStatuses",
                () =>
                {
                    var statusTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "CORR.STATUSES");
                    if (statusTable == null)
                    {
                        var message = "Unable to get CORE->CORR.STATUSES valcode table";
                        logger.Error(message);
                        throw new Exception(message);
                    }
                    return statusTable;
                }, Level1CacheTimeoutValue);
        }
    }
}
