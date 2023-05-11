/* Copyright 2019-2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Domain.Base.Services;
using System.Text;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Repository for the employee leave request
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EmployeeLeaveRequestRepository : PersonBaseRepository, IEmployeeLeaveRequestRepository
    {
        private readonly int bulkReadSize;
        private readonly string colleagueTimeZone;
        private readonly string separator = ",";
        private readonly string space = " ";
        private Dictionary<string, PersonHierarchyName> personNameHierarchyDict;
        private IHumanResourcesReferenceDataRepository humanResourcesReferenceDataRepository;
        public EmployeeLeaveRequestRepository(IHumanResourcesReferenceDataRepository humanResourcesReferenceDataRepository, ICacheProvider cacheProvider,
            IColleagueTransactionFactory transactionFactory,
            ILogger logger,
            ApiSettings settings) : base(cacheProvider, transactionFactory, logger, settings)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
            colleagueTimeZone = settings.ColleagueTimeZone;
            this.humanResourcesReferenceDataRepository = humanResourcesReferenceDataRepository;

            if (personNameHierarchyDict == null)
                personNameHierarchyDict = new Dictionary<string, PersonHierarchyName>();

        }

        /// <summary>
        /// Gets all leave requests for the input employee Ids
        /// </summary>
        /// <param name="effectivePersonIds">List of employee Ids</param>
        /// <returns>List of LeaveRequest entities</returns>
        public async Task<IEnumerable<Domain.HumanResources.Entities.LeaveRequest>> GetLeaveRequestsAsync(IEnumerable<string> effectivePersonIds)
        {
            if (effectivePersonIds == null || !effectivePersonIds.Any())
            {
                var message = "Employee Ids are required to get leave requests";
                logger.Error(message);
                throw new ArgumentException(message);
            }

            var employeeIds = effectivePersonIds.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct();

            if (!employeeIds.Any())
            {
                var message = "No employee ids found in input list";
                logger.Error(message);
                throw new ArgumentException(message);
            }

            #region DATA ACCESS
            logger.Debug(string.Format("Leave Requests will be retrieved for the employees = ", string.Join(",", employeeIds.ToArray())));
            var leaveRequestCriteria = "WITH LR.EMPLOYEE.ID EQ ?";
            var leaveRequestKeys = await DataReader.SelectAsync("LEAVE.REQUEST", leaveRequestCriteria, employeeIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
            if (leaveRequestKeys == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST SelectAsync";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestKeys.Any())
            {
                logger.Error("No LEAVE.REQUEST keys exist for the given employee Ids: " + string.Join(",", employeeIds));
            }

            var leaveRequestDetailCriteria = "WITH LRD.LEAVE.REQUEST.ID EQ ?";
            var leaveRequestDetailKeys = await DataReader.SelectAsync("LEAVE.REQUEST.DETAIL", leaveRequestDetailCriteria, leaveRequestKeys.Select(key => string.Format("\"{0}\"", key)).ToArray());
            if (leaveRequestDetailKeys == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.DETAIL SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestDetailKeys.Any())
            {
                logger.Debug("No LEAVE.REQUEST.DETAIL keys exist for the given Leave Request Ids: " + string.Join(",", leaveRequestKeys));
            }

            var leaveRequestStatusCriteria = "WITH LRS.LEAVE.REQUEST.ID EQ ?";
            var leaveRequestStatusKeys = await DataReader.SelectAsync("LEAVE.REQUEST.STATUS", leaveRequestStatusCriteria, leaveRequestKeys.Select(key => string.Format("\"{0}\"", key)).ToArray());
            if (leaveRequestStatusKeys == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.STATUS SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestStatusKeys.Any())
            {
                logger.Debug("No LEAVE.REQUEST.STATUS keys exist for the given Leave Request Ids: " + string.Join(",", leaveRequestKeys));
            }


            // Select all LEAVE.REQUEST.COMMENTS records matching the input leaveRequestId
            string leaveRequestCommentsCriteria = "WITH LRC.LEAVE.REQUEST.ID EQ ?";
            var leaveRequestCommentsKeys = await DataReader.SelectAsync("LEAVE.REQ.COMMENTS", leaveRequestCommentsCriteria, leaveRequestKeys.Select(key => string.Format("\"{0}\"", key)).ToArray());

            var dbLeaveRequests = new List<DataContracts.LeaveRequest>();
            var dbLeaveRequestDetails = new List<DataContracts.LeaveRequestDetail>();
            var dbleaveRequestStatuses = new List<DataContracts.LeaveRequestStatus>();
            var dbleaveRequestComments = new List<DataContracts.LeaveReqComments>();

            for (int i = 0; i < leaveRequestKeys.Count(); i += bulkReadSize)
            {
                var subList = leaveRequestKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequest>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Leave Request records");
                }
                else
                {
                    dbLeaveRequests.AddRange(records);
                }
            }
            logger.Debug("Leave Requests Records fetched from the database");

            for (int i = 0; i < leaveRequestDetailKeys.Count(); i += bulkReadSize)
            {
                var subList = leaveRequestDetailKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Leave Request Detail records");
                }
                else
                {
                    dbLeaveRequestDetails.AddRange(records);
                }
            }
            logger.Debug("Leave Requests Detail Records fetched from the database");

            for (int i = 0; i < leaveRequestStatusKeys.Count(); i += bulkReadSize)
            {
                var subList = leaveRequestStatusKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequestStatus>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Leave Request Status records");
                }
                else
                {
                    dbleaveRequestStatuses.AddRange(records);
                }
            }
            logger.Debug("Leave Requests Status Records fetched from the database");

            //Comments are optional for a leave request.
            if (leaveRequestCommentsKeys != null && leaveRequestCommentsKeys.Any())
            {
                // Bulkread the No LEAVE.REQUEST.COMMENTS records          
                for (int i = 0; i < leaveRequestCommentsKeys.Count(); i += bulkReadSize)
                {
                    var subList = leaveRequestCommentsKeys.Skip(i).Take(bulkReadSize);
                    var selectedRecords = await DataReader.BulkReadRecordAsync<DataContracts.LeaveReqComments>(subList.ToArray());
                    if (selectedRecords == null)
                    {
                        logger.Error("Unexpected null from bulk read of LEAVE.REQUEST.COMMENTS records");
                    }
                    else
                    {
                        dbleaveRequestComments.AddRange(selectedRecords);
                    }
                }
                logger.Debug("Leave Requests Comments Records fetched from the database");
            }
            #endregion

            #region DATA MAPPING
            var LeaveRequestEntities = new List<Domain.HumanResources.Entities.LeaveRequest>();
            var LeaveRequestDetailEntities = new List<Domain.HumanResources.Entities.LeaveRequestDetail>();
            var LeaveRequestStatusEntities = new List<Domain.HumanResources.Entities.LeaveRequestStatus>();
            var LeaveRequestCommentsEntities = new List<Domain.HumanResources.Entities.LeaveRequestComment>();

            var leaveRequestDetailsRecordDict = dbLeaveRequestDetails.ToLookup(lrd => lrd.LrdLeaveRequestId);
            var leaveRequestStatusRecordDict = dbleaveRequestStatuses.ToLookup(lrs => lrs.LrsLeaveRequestId);
            var leaveRequestCommentsRecordDict = dbleaveRequestComments.ToLookup(lrc => lrc.LrcLeaveRequestId);


            //Loop through each Leave Request Record
            List<Task<Domain.HumanResources.Entities.LeaveRequest>> BuildLeaveRequestEntitiesTasks = new List<Task<Domain.HumanResources.Entities.LeaveRequest>>();

            logger.Debug(string.Format("Total Leave Requests fetched = {0}", dbLeaveRequests.Count()));
            try
            {
                foreach (var leaveRequestRecord in dbLeaveRequests)
                {
                    //logger.Debug("Leave Request Id + ", leaveRequestRecord.Recordkey);
                    //Get all leave request details from dict and build entity
                    if (leaveRequestDetailsRecordDict.Contains(leaveRequestRecord.Recordkey))
                    {
                        LeaveRequestDetailEntities = BuildLeaveRequestDetailEntities(leaveRequestDetailsRecordDict[leaveRequestRecord.Recordkey].ToList());
                        //logger.Debug(string.Format("Leave Request Details Ids are {0}", string.Join(",", LeaveRequestDetailEntities.Select(lrd => lrd.Id).ToArray())));
                    }

                    //Get all leave request statuses from dict and build entity
                    if (leaveRequestStatusRecordDict.Contains(leaveRequestRecord.Recordkey))
                    {
                        LeaveRequestStatusEntities = BuildLeaveRequestStatusEntities(leaveRequestStatusRecordDict[leaveRequestRecord.Recordkey].ToList());
                        //logger.Debug(string.Format("Leave Request Status Ids are {0}", string.Join(",", LeaveRequestStatusEntities.Select(lrd => lrd.Id).ToArray())));
                    }

                    //Get all leave request comments from dict and build entity
                    if (leaveRequestCommentsRecordDict.Contains(leaveRequestRecord.Recordkey))
                    {
                        LeaveRequestCommentsEntities = await BuildLeaveRequestCommentEntities(leaveRequestCommentsRecordDict[leaveRequestRecord.Recordkey].ToList());
                        //logger.Debug(string.Format("Leave Request Comment Ids are {0}", string.Join(",", LeaveRequestCommentsEntities.Select(lrd => lrd.Id).ToArray())));

                    }
                    else
                    {
                        LeaveRequestCommentsEntities = null;
                        logger.Debug("There are no comments yet for this leave request");
                    }
                    //Create a task for each BuildLeaveRequestEntity and add to the list
                    BuildLeaveRequestEntitiesTasks.Add(BuildLeaveRequestEntity(leaveRequestRecord, LeaveRequestDetailEntities, LeaveRequestStatusEntities, LeaveRequestCommentsEntities));
                }
                //Invoke tasks parallely and collate the results
                LeaveRequestEntities.AddRange(await Task.WhenAll(BuildLeaveRequestEntitiesTasks));

                #endregion

                return LeaveRequestEntities;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Unable to build LeaveRequest Entities. Exception Message = {0}", e.Message));
                throw;
            }
        }


        /// <summary>
        /// Gets the Approved Leave Requests for a timecard week based on the date range. 
        /// </summary>
        /// <param name="startDate">Start date of timecard week </param>
        /// <param name="endDate">End date of timecard week</param>
        /// <param name="employeeIds">List of person Ids</param>
        /// <returns>List of Leave Request Domain Entities</returns>
        public async Task<IEnumerable<Domain.HumanResources.Entities.LeaveRequest>> GetLeaveRequestsForTimeEntryAsync(DateTime startDate, DateTime endDate, IEnumerable<string> employeeIds)
        {

            if (employeeIds == null || !employeeIds.Any())
            {
                var message = "Employee Ids are required to get approved leave requests for the specified date range";
                logger.Error(message);
                throw new ArgumentNullException("employeeIds", message);
            }


            //Get leave requests of loggedin user and their supervisees.
            var leaveRequestCriteria = "WITH LR.EMPLOYEE.ID EQ ?";
            var leaveRequestKeys = await DataReader.SelectAsync("LEAVE.REQUEST", leaveRequestCriteria, employeeIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
            if (leaveRequestKeys == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST SelectAsync";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestKeys.Any())
            {
                logger.Error("No LEAVE.REQUEST keys exist for the given employee Ids: " + string.Join(",", employeeIds));
            }

            //Filter leave requests that are in approved status
            string leaveRequestStatusCriteria = "WITH LRS.LEAVE.REQUEST.ID EQ ?";
            var leaveRequestStatusKeys = await DataReader.SelectAsync("LEAVE.REQUEST.STATUS", leaveRequestStatusCriteria, leaveRequestKeys.Select(key => string.Format("\"{0}\"", key)).ToArray());
            IEnumerable<string> approvedLeaveRequestIds = new List<string>();
            if (leaveRequestStatusKeys == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.STATUS SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestStatusKeys.Any())
            {
                logger.Info("No LEAVE.REQUEST.STATUS keys exist for the given Leave Request Ids: " + string.Join(",", leaveRequestStatusKeys));
            }

            //get leave detail data contract records 
            for (int i = 0; i < leaveRequestStatusKeys.Count(); i += bulkReadSize)
            {
                var subList = leaveRequestStatusKeys.Skip(i).Take(bulkReadSize);
                var leaveRequestStatusRecords = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequestStatus>(subList.ToArray());
                if (leaveRequestStatusRecords == null)
                {
                    logger.Error(string.Format("No leave status data available for requested date range {0} - {1}", startDate, endDate));
                }
                else
                {
                    //Identify Latest status, filter "Approved" status records and extract leave request Ids
                    var latestStatusRecordKeys = leaveRequestStatusRecords.Where(lrs => (leaveRequestStatusRecords.GroupBy(g => g.LrsLeaveRequestId)
                                                    .Select(x => x.Max(o => int.Parse(o.Recordkey)).ToString()).Contains(lrs.Recordkey)
                                                      && lrs.LrsActionType.Equals(LeaveStatusAction.Approved.ToString()))).ToList();


                    if (latestStatusRecordKeys != null && latestStatusRecordKeys.Any())
                    {
                        approvedLeaveRequestIds = latestStatusRecordKeys.Select(x => x.LrsLeaveRequestId);
                    }

                }
            }
            //Holds the final list of leave request to be returned
            var dbLeaveRequests = new List<DataContracts.LeaveRequest>();

            for (int i = 0; i < approvedLeaveRequestIds.Count(); i += bulkReadSize)
            {
                var subList = approvedLeaveRequestIds.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequest>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Leave Request records");
                }
                else
                {
                    dbLeaveRequests.AddRange(records);
                }
            }

            //Filter leave requests that falls in the range of time week start and end dates.
            //Either Leave Start Date (or) Leave End Date must be within time week range. 
            if (dbLeaveRequests != null && dbLeaveRequests.Any())
            {

                dbLeaveRequests = dbLeaveRequests.Where(lr => ((lr.LrStartDate >= startDate && lr.LrStartDate <= endDate) ||
                                            (lr.LrEndDate >= startDate && lr.LrEndDate <= endDate) || (lr.LrStartDate <= startDate && lr.LrEndDate >= endDate))
                                            ).ToList();
            }

            #region Final Leave Requests

            var dbLeaveRequestDetails = new List<DataContracts.LeaveRequestDetail>();
            var dbleaveRequestStatuses = new List<DataContracts.LeaveRequestStatus>();

            var finalLeaveRequestKeys = dbLeaveRequests.Select(lr => lr.Recordkey);

            /*FINAL LEAVE REQUEST DETAIL RECORDS FETCH*/
            var finalLeaveRequestDetailCriteria = "WITH LRD.LEAVE.REQUEST.ID EQ ?";
            var finalLeaveRequestDetailKeys = await DataReader.SelectAsync("LEAVE.REQUEST.DETAIL", finalLeaveRequestDetailCriteria, finalLeaveRequestKeys.Select(key => string.Format("\"{0}\"", key)).ToArray());
            if (finalLeaveRequestDetailKeys == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.DETAIL SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!finalLeaveRequestDetailKeys.Any())
            {
                logger.Info("No LEAVE.REQUEST.DETAIL keys exist for the given Leave Request Ids: " + string.Join(",", finalLeaveRequestKeys));
            }

            for (int i = 0; i < finalLeaveRequestDetailKeys.Count(); i += bulkReadSize)
            {
                var subList = finalLeaveRequestDetailKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Leave Request Detail records");
                }
                else
                {
                    dbLeaveRequestDetails.AddRange(records);
                }
            }

            /*FINAL LEAVE REQUEST STATUS RECORDS FETCH*/
            var finalLeaveRequestStatusCriteria = "WITH LRS.LEAVE.REQUEST.ID EQ ?";
            var finalLeaveRequestStatusKeys = await DataReader.SelectAsync("LEAVE.REQUEST.STATUS", finalLeaveRequestStatusCriteria, finalLeaveRequestKeys.Select(key => string.Format("\"{0}\"", key)).ToArray());
            if (finalLeaveRequestStatusKeys == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.STATUS SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!finalLeaveRequestStatusKeys.Any())
            {
                logger.Info("No LEAVE.REQUEST.STATUS keys exist for the given Leave Request Ids: " + string.Join(",", finalLeaveRequestStatusKeys));
            }


            for (int i = 0; i < finalLeaveRequestStatusKeys.Count(); i += bulkReadSize)
            {
                var subList = finalLeaveRequestStatusKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequestStatus>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Leave Request Status records");
                }
                else
                {
                    dbleaveRequestStatuses.AddRange(records);
                }
            }

            #region DATA MAPPING
            var LeaveRequestEntities = new List<Domain.HumanResources.Entities.LeaveRequest>();
            var LeaveRequestDetailEntities = new List<Domain.HumanResources.Entities.LeaveRequestDetail>();
            var LeaveRequestStatusEntities = new List<Domain.HumanResources.Entities.LeaveRequestStatus>();


            var leaveRequestDetailsRecordDict = dbLeaveRequestDetails.ToLookup(lrd => lrd.LrdLeaveRequestId);
            var leaveRequestStatusRecordDict = dbleaveRequestStatuses.ToLookup(lrs => lrs.LrsLeaveRequestId);

            try
            {
                //Loop through each Leave Request Record
                List<Task<Domain.HumanResources.Entities.LeaveRequest>> BuildLeaveRequestEntitiesTasks = new List<Task<Domain.HumanResources.Entities.LeaveRequest>>();
                foreach (var leaveRequestRecord in dbLeaveRequests)
                {
                    //Get all leave request details from dict and build entity
                    if (leaveRequestDetailsRecordDict.Contains(leaveRequestRecord.Recordkey))
                    {
                        LeaveRequestDetailEntities = BuildLeaveRequestDetailEntities(leaveRequestDetailsRecordDict[leaveRequestRecord.Recordkey].ToList());
                    }

                    // Get all leave request statuses from dict and build entity
                    if (leaveRequestStatusRecordDict.Contains(leaveRequestRecord.Recordkey))
                    {
                        LeaveRequestStatusEntities = BuildLeaveRequestStatusEntities(leaveRequestStatusRecordDict[leaveRequestRecord.Recordkey].ToList());
                    }


                    //Create a task for each BuildLeaveRequestEntity and add to the list
                    BuildLeaveRequestEntitiesTasks.Add(BuildLeaveRequestEntity(leaveRequestRecord, LeaveRequestDetailEntities, LeaveRequestStatusEntities, null));
                }
                //Invoke tasks parallely and collate the results
                LeaveRequestEntities.AddRange(await Task.WhenAll(BuildLeaveRequestEntitiesTasks));

                #endregion

                #endregion

                return LeaveRequestEntities;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Unable to build LeaveRequest Entities. Exception Message = {0}", e.Message));
                throw;
            }
        }
        /// <summary>
        /// Gets a single LeaveRequest object matching the given id. 
        /// </summary>
        /// <param name="id">Leave Request Id</param>
        /// <param name="currentUserId">Current User Id(optional)</param>         
        /// <returns>LeaveRequest Entity</returns>
        public async Task<Domain.HumanResources.Entities.LeaveRequest> GetLeaveRequestInfoByLeaveRequestIdAsync(string id, string currentUserId = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("leaveRequestId");
            }

            #region LEAVE.REQUEST
            // Select a LEAVE.REQUEST record matching the input leaveRequestId
            var leaveRequestRecord = await DataReader.ReadRecordAsync<DataContracts.LeaveRequest>(id);
            if (leaveRequestRecord == null)
            {
                var message = "LEAVE.REQUEST record with" + id + "doesn't exist";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }
            #endregion

            #region LEAVE.REQUEST.DETAIL
            // Select all LEAVE.REQUEST.DETAIL records matching the input leaveRequestId
            string lrdCriteria = "WITH LRD.LEAVE.REQUEST.ID EQ '{0}'";
            string[] leaveRequestDetailIds = await DataReader.SelectAsync("LEAVE.REQUEST.DETAIL", string.Format(lrdCriteria, id));

            if (leaveRequestDetailIds == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.DETAIL SelectAsync";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestDetailIds.Any())
            {
                var message = "No LEAVE.REQUEST.DETAIL keys exist for the given leaveRequestId: " + id;
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            // Bulkread the No LEAVE.REQUEST.DETAIL records
            var leaveRequestDetailRecords = new List<DataContracts.LeaveRequestDetail>();
            for (int i = 0; i < leaveRequestDetailIds.Count(); i += bulkReadSize)
            {
                var subList = leaveRequestDetailIds.Skip(i).Take(bulkReadSize);
                var selectedRecords = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequestDetail>(subList.ToArray());
                if (selectedRecords == null)
                {
                    logger.Error("Unexpected null from bulk read of LEAVE.REQUEST.DETAIL records");
                }
                else
                {
                    leaveRequestDetailRecords.AddRange(selectedRecords);
                }
            }

            #endregion

            #region LEAVE.REQUEST.STATUS
            // Select all LEAVE.REQUEST.STATUS records matching the input leaveRequestId
            string lrsCriteria = "WITH LRS.LEAVE.REQUEST.ID EQ '{0}'";
            string[] leaveRequestStatusIds = await DataReader.SelectAsync("LEAVE.REQUEST.STATUS", string.Format(lrsCriteria, id));

            if (leaveRequestStatusIds == null)
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.STATUS SelectAsync";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestStatusIds.Any())
            {
                var message = "No LEAVE.REQUEST.STATUS keys exist for the given person leaveRequestId: " + id;
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            // Bulkread the No LEAVE.REQUEST.STATUS records
            var leaveRequestStatusRecords = new List<DataContracts.LeaveRequestStatus>();
            for (int i = 0; i < leaveRequestStatusIds.Count(); i += bulkReadSize)
            {
                var subList = leaveRequestStatusIds.Skip(i).Take(bulkReadSize);
                var selectedRecords = await DataReader.BulkReadRecordAsync<DataContracts.LeaveRequestStatus>(subList.ToArray());
                if (selectedRecords == null)
                {
                    logger.Error("Unexpected null from bulk read of LEAVE.REQUEST.STATUS records");
                }
                else
                {
                    leaveRequestStatusRecords.AddRange(selectedRecords);
                }
            }

            #endregion

            #region LEAVE.REQUEST.COMMENTS
            // Select all LEAVE.REQUEST.COMMENTS records matching the input leaveRequestId
            // This will be mapped to the domain entity once the requirements are clear
            string lrcCriteria = "WITH LRC.LEAVE.REQUEST.ID EQ '{0}'";
            string[] leaveRequestCommentsIds = await DataReader.SelectAsync("LEAVE.REQ.COMMENTS", string.Format(lrcCriteria, id));
            var leaveRequestCommentsRecords = new List<LeaveReqComments>();

            //Comments are optional for a leave request
            if (leaveRequestCommentsIds != null && leaveRequestCommentsIds.Any())
            {
                // Bulkread the No LEAVE.REQUEST.COMMENTS records          
                for (int i = 0; i < leaveRequestCommentsIds.Count(); i += bulkReadSize)
                {
                    var subList = leaveRequestCommentsIds.Skip(i).Take(bulkReadSize);
                    var selectedRecords = await DataReader.BulkReadRecordAsync<DataContracts.LeaveReqComments>(subList.ToArray());
                    if (selectedRecords == null)
                    {
                        logger.Error("Unexpected null from bulk read of LEAVE.REQUEST.COMMENTS records");
                    }
                    else
                    {
                        leaveRequestCommentsRecords.AddRange(selectedRecords);
                    }
                }
            }
            // *** Build LEAVE.REQUEST.COMMENT Entites and pass them on to the BuildLeaveRequestEntity() once the requirements are clear ***
            #endregion

            // Build the LEAVE.REQUEST.DETAIL Entities
            var leaveRequestDetailEntities = BuildLeaveRequestDetailEntities(leaveRequestDetailRecords);

            // Build the LEAVE.REQUEST.STATUS Entities
            var leaveRequestStatusEntities = BuildLeaveRequestStatusEntities(leaveRequestStatusRecords);

            //Build the LEAVE.REQUEST.COMMENT Entities
            var leaveRequestCommentEntities = await BuildLeaveRequestCommentEntities(leaveRequestCommentsRecords);

            // Build the LEAVE.REQUEST Entity
            //Added new param currentUserId to compute the flag to show/hide Delete button for Supervisor user
            var leaveRequestEntity = await BuildLeaveRequestEntity(leaveRequestRecord, leaveRequestDetailEntities, leaveRequestStatusEntities, leaveRequestCommentEntities, currentUserId);

            return leaveRequestEntity;
        }

        /// <summary>
        /// Creates a new leave request record
        /// </summary>
        /// <param name="leaveRequest">Leave Request Entity</param>
        /// <returns>Newly created leave request</returns>
        public async Task<Domain.HumanResources.Entities.LeaveRequest> CreateLeaveRequestAsync(Domain.HumanResources.Entities.LeaveRequest leaveRequest)
        {
            if (leaveRequest == null)
            {
                var message = "Leave Request object is required to add a Leave Request";
                logger.Error(message);
                throw new ArgumentNullException("leaveRequest", message);
            }

            var request = new Ellucian.Colleague.Data.HumanResources.Transactions.CreateLeaveRequestRequest()
            {
                LrEmployeeId = leaveRequest.EmployeeId,
                LrPerleaveId = leaveRequest.PerLeaveId,
                LrApproverId = leaveRequest.ApproverId,
                LrApproverName = leaveRequest.ApproverName,
                LrStartDate = DateTime.SpecifyKind(DateTime.Parse(leaveRequest.StartDate.ToString()), DateTimeKind.Unspecified),
                LrEndDate = DateTime.SpecifyKind(DateTime.Parse(leaveRequest.EndDate.ToString()), DateTimeKind.Unspecified),

                CreateLeaveRequestDetails = leaveRequest.LeaveRequestDetails.Select(lrd => new Ellucian.Colleague.Data.HumanResources.Transactions.CreateLeaveRequestDetails()
                {
                    LrdLeaveDate = DateTime.SpecifyKind(lrd.LeaveDate, DateTimeKind.Unspecified),
                    LrdHours = lrd.LeaveHours
                }
               ).ToList()
            };

            var response = await transactionInvoker.ExecuteAsync<Ellucian.Colleague.Data.HumanResources.Transactions.CreateLeaveRequestRequest,
                Ellucian.Colleague.Data.HumanResources.Transactions.CreateLeaveRequestResponse>(request);

            if (response == null)
            {
                var message = "Could not create leave request. Unexpected null response from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error(response.ErrorMessage);
                throw new ApplicationException(response.ErrorMessage);
            }

            return await GetLeaveRequestInfoByLeaveRequestIdAsync(response.OutLeaveRequestId);
        }

        /// <summary>
        /// Creates a new leave request status record
        /// </summary>
        /// <param name="status">Leave Request Status Entity</param>        
        /// <returns>Newly created leave request status</returns>
        public async Task<Domain.HumanResources.Entities.LeaveRequestStatus> CreateLeaveRequestStatusAsync(Domain.HumanResources.Entities.LeaveRequestStatus status)
        {

            if (status == null)
            {
                var message = "Leave Request Status object is required.";
                logger.Error(message);
                throw new ArgumentNullException("leaveRequestStatus", message);
            }

            if (string.IsNullOrEmpty(status.LeaveRequestId))
            {
                throw new ArgumentException("leave request Id attribute is required in status");
            }

            var request = new Ellucian.Colleague.Data.HumanResources.Transactions.CreateLeaveRequestStatusRequest()
            {
                LrLeaveRequestId = status.LeaveRequestId,
                LrActionType = status.ActionType.ToString(),
                LrActionerId = status.ActionerId,
                LrWithdrawOption = status.WithdrawOption
            };
            CreateLeaveRequestStatusResponse response = null;
            try
            {
                // execute the request
                response = await transactionInvoker.ExecuteAsync<CreateLeaveRequestStatusRequest, CreateLeaveRequestStatusResponse>(request);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new ApplicationException("Error occurred in Create Leave Request Status transaction. See API log for details.");
            }

            #region Post CTX Execution Check
            // check the response for errors and throw an exception for any irregularities
            if (response == null)
            {
                var message = "Could not create leaverequest status. Unexpected null response from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                // check the response for errors and throw an exception for any irregularities
                var message = response.ErrorMessage;
                logger.Error(message);
                throw new ApplicationException(message);
            }

            //Check for ID of newly created leave request status record
            if (string.IsNullOrEmpty(response.NewLeaveStatusKey))
            {
                var message = "Could not create leaverequest status record. NewLeaveStatusKey is not available";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            #endregion

            //Get the newly created Leave Request Status
            string id = response.NewLeaveStatusKey;

            var leaveRequestStatusRecord = await DataReader.ReadRecordAsync<DataContracts.LeaveRequestStatus>(id);
            if (leaveRequestStatusRecord == null)
            {
                var message = string.Format("LEAVE.REQUEST.STATUS record with {0} doesn't exist", id);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }
            var leaveRequestStatusEntity = BuildLeaveRequestStatusEntity(leaveRequestStatusRecord);
            //Extract Actioner Name
            if (leaveRequestStatusEntity != null && leaveRequestStatusEntity.ActionType != LeaveStatusAction.Deleted)
            {
                leaveRequestStatusEntity.ActionerName = await ExtractActionerNameFromPersonInfo(leaveRequestStatusEntity.ActionerId);

            }
            return leaveRequestStatusEntity;
        }

        /// <summary>
        /// Updates an existing leave request record
        /// </summary>
        /// <param name="leaveRequest">Leave Request Entity</param>
        /// <returns>Newly updated leave request</returns>
        public async Task<Domain.HumanResources.Entities.LeaveRequest> UpdateLeaveRequestAsync(LeaveRequestHelper leaveRequestHelper)
        {
            if (leaveRequestHelper == null)
            {
                var message = "leaveRequestHelper is null";
                logger.Error(message);
                throw new ArgumentNullException("leaveRequestHelper", message);
            }

            #region Build_CTX_Request
            var allLeaveRequestDetails = new List<Ellucian.Colleague.Data.HumanResources.Transactions.LeaveRequestDetails>();

            // Leave request detail records to be created
            if (leaveRequestHelper.LeaveRequestDetailsToCreate != null && leaveRequestHelper.LeaveRequestDetailsToCreate.Any())
            {
                var leaveRequestDetailsToCreate = leaveRequestHelper.LeaveRequestDetailsToCreate.Select(lrd => new Ellucian.Colleague.Data.HumanResources.Transactions.LeaveRequestDetails()
                {
                    InLrType = "ADD",
                    LrdLeaveRequestId = lrd.LeaveRequestId,
                    LeaveRequestDetailId = lrd.Id,
                    LrdLeaveDate = DateTime.SpecifyKind(DateTime.Parse(lrd.LeaveDate.ToString()), DateTimeKind.Unspecified),
                    LrdLeaveHours = lrd.LeaveHours
                }).ToList();
                allLeaveRequestDetails.AddRange(leaveRequestDetailsToCreate);
            }

            // Leave request detail records to be updated
            if (leaveRequestHelper.LeaveRequestDetailsToUpdate != null && leaveRequestHelper.LeaveRequestDetailsToUpdate.Any())
            {
                var leaveRequestDetailsToUpdate = leaveRequestHelper.LeaveRequestDetailsToUpdate.Select(lrd => new Ellucian.Colleague.Data.HumanResources.Transactions.LeaveRequestDetails()
                {
                    InLrType = "UPDATE",
                    LrdLeaveRequestId = lrd.LeaveRequestId,
                    LeaveRequestDetailId = lrd.Id,
                    LrdLeaveDate = DateTime.SpecifyKind(DateTime.Parse(lrd.LeaveDate.ToString()), DateTimeKind.Unspecified),
                    LrdLeaveHours = lrd.LeaveHours
                }).ToList();
                allLeaveRequestDetails.AddRange(leaveRequestDetailsToUpdate);
            }

            if (leaveRequestHelper.LeaveRequestDetailsToDelete != null && leaveRequestHelper.LeaveRequestDetailsToDelete.Any())
            {
                // Leave request detail records to be deleted
                var leaveRequestDetailsToDelete = leaveRequestHelper.LeaveRequestDetailsToDelete.Select(lrd => new Ellucian.Colleague.Data.HumanResources.Transactions.LeaveRequestDetails()
                {
                    InLrType = "DELETE",
                    LrdLeaveRequestId = lrd.LeaveRequestId,
                    LeaveRequestDetailId = lrd.Id,
                    LrdLeaveDate = DateTime.SpecifyKind(DateTime.Parse(lrd.LeaveDate.ToString()), DateTimeKind.Unspecified),
                    LrdLeaveHours = lrd.LeaveHours
                }).ToList();
                allLeaveRequestDetails.AddRange(leaveRequestDetailsToDelete);
            }

            // Create the request object for UPDATE.LEAVE.REQUEST CTX
            UpdateLeaveRequestRequest request;
            if (leaveRequestHelper.LeaveRequest != null)
            {
                request = new Ellucian.Colleague.Data.HumanResources.Transactions.UpdateLeaveRequestRequest()
                {
                    LeaveRequestId = leaveRequestHelper.LeaveRequest.Id,
                    LrEmployeeId = leaveRequestHelper.LeaveRequest.EmployeeId,
                    LrPerleaveId = leaveRequestHelper.LeaveRequest.PerLeaveId,
                    LrStartDate = leaveRequestHelper.LeaveRequest.StartDate.HasValue ? DateTime.SpecifyKind(DateTime.Parse(leaveRequestHelper.LeaveRequest.StartDate.ToString()), DateTimeKind.Unspecified) as DateTime? : null,
                    LrEndDate = leaveRequestHelper.LeaveRequest.EndDate.HasValue ? DateTime.SpecifyKind(DateTime.Parse(leaveRequestHelper.LeaveRequest.EndDate.ToString()), DateTimeKind.Unspecified) as DateTime? : null,
                    LrApproverId = leaveRequestHelper.LeaveRequest.ApproverId,
                    LrApproverName = leaveRequestHelper.LeaveRequest.ApproverName,
                    LeaveRequestDetails = allLeaveRequestDetails,
                    IsLeaveRequestFromSupervisorOrProxy = leaveRequestHelper.IsLeaveRequestFromSupervisorOrProxy
                };
            }
            else
            {
                throw new ArgumentException("LeaveRequest property of leaveRequestHelper is null", "leaveRequestHelper");
            }
            #endregion

            // Invoke the UPDATE.LEAVE.REQUEST CTX
            var response = await transactionInvoker.ExecuteAsync<Ellucian.Colleague.Data.HumanResources.Transactions.UpdateLeaveRequestRequest,
                Ellucian.Colleague.Data.HumanResources.Transactions.UpdateLeaveRequestResponse>(request);

            if (response == null)
            {
                var message = "Could not update the leave request. Unexpected null returned from the CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error(response.ErrorMessage);
                if (response.ErrorMessage.StartsWith("CONFLICT", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new RecordLockException(response.ErrorMessage, "LEAVE.REQUEST", leaveRequestHelper.LeaveRequest.Id);
                }
                else
                {
                    throw new ApplicationException(response.ErrorMessage);
                }
            }

            // Return the newly updated leave request.
            return await GetLeaveRequestInfoByLeaveRequestIdAsync(leaveRequestHelper.LeaveRequest.Id);
        }

        /// <summary>
        /// Creates a new leave request comment associated with a leave request.
        /// </summary>
        /// <param name="leaveRequestComment">Leave Request Comment Entity</param>
        /// <returns>Leave Request Comment Domain Entity that contains newly created comment's information </returns>
        public async Task<Domain.HumanResources.Entities.LeaveRequestComment> CreateLeaveRequestCommentsAsync(LeaveRequestComment leaveRequestComment)
        {
            if (leaveRequestComment == null)
            {
                var message = "Leave Request Comment object is required to create a Leave Request Comment";
                logger.Error(message);
                throw new ArgumentNullException("leaveRequestComment", message);
            }

            //Create the request object
            var request = new Ellucian.Colleague.Data.HumanResources.Transactions.CreateLeaveRequestCommentsRequest()
            {
                LeaveRequestId = leaveRequestComment.LeaveRequestId,
                EmployeeId = leaveRequestComment.EmployeeId,
                Comments = leaveRequestComment.Comments,
                CommentsAuthorName = leaveRequestComment.CommentAuthorName
            };

            //Invoke the CTX
            var response = await transactionInvoker.ExecuteAsync<Ellucian.Colleague.Data.HumanResources.Transactions.CreateLeaveRequestCommentsRequest,
               Ellucian.Colleague.Data.HumanResources.Transactions.CreateLeaveRequestCommentsResponse>(request);

            //check if response is null
            if (response == null)
            {
                var message = "Could not create leave request comment. Unexpected null response from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            //Check for ID of newly created leave request comment record
            if (string.IsNullOrEmpty(response.NewKey))
            {
                var message = "Could not create leaveRequestComment record. NewKey is not available";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            //get the newly created comment record
            var newComment = await DataReader.ReadRecordAsync<DataContracts.LeaveReqComments>(response.NewKey);

            if (newComment == null)
            {
                var message = "Newly created comment not found";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            return await BuildCommentsEntity(newComment);
        }


        #region Helper Methods
        /// <summary>
        /// Helper method that builds a list of LeaveRequestStatus entities associated with a particular LeaveRequest record.
        /// </summary>
        /// <param name="leaveRequestStatusRecords"></param>
        /// <returns>List of LeaveRequestStatus entities</returns>
        private List<Domain.HumanResources.Entities.LeaveRequestStatus> BuildLeaveRequestStatusEntities(List<DataContracts.LeaveRequestStatus> leaveRequestStatusRecords)
        {
            var leaveRequestStatusEntities = new List<Domain.HumanResources.Entities.LeaveRequestStatus>();
            if (leaveRequestStatusRecords != null && leaveRequestStatusRecords.Any())
            {
                foreach (var leaveRequestStatus in leaveRequestStatusRecords)
                {
                    leaveRequestStatusEntities.Add(BuildLeaveRequestStatusEntity(leaveRequestStatus));
                }
            }

            return leaveRequestStatusEntities;
        }

        /// <summary>
        ///  Helper method that builds a LeaveRequestComment entity associated with a particular LeaveRequest record.
        /// </summary>
        /// <param name="leaveRequestCommentRecords"></param>
        /// <returns>Leave Request Comment Entity</returns>
        private async Task<List<Domain.HumanResources.Entities.LeaveRequestComment>> BuildLeaveRequestCommentEntities(List<DataContracts.LeaveReqComments> leaveRequestCommentRecords)
        {
            var leaveRequestCommentEntities = new List<Domain.HumanResources.Entities.LeaveRequestComment>();
            if (leaveRequestCommentRecords != null && leaveRequestCommentRecords.Any())
            {
                StringBuilder commentsAuthorName = new StringBuilder();
                logger.Debug("Leave Request Comments are =");
                foreach (var leaveRequestComment in leaveRequestCommentRecords)
                {
                    commentsAuthorName.Append(string.IsNullOrWhiteSpace(leaveRequestComment.LrcEmployeeId) ? leaveRequestComment.LrcAddOpername : await ExtractCommentAuthorNameFromPersonInfo(leaveRequestComment.LeaveReqCommentsAddopr));

                    leaveRequestCommentEntities.Add(new LeaveRequestComment(leaveRequestComment.Recordkey,
                                                                            leaveRequestComment.LrcLeaveRequestId,
                                                                            leaveRequestComment.LrcEmployeeId,
                                                                            leaveRequestComment.LrcComments,
                                                                           commentsAuthorName.ToString())

                    {
                        Timestamp = new Timestamp(leaveRequestComment.LeaveReqCommentsAddopr,
                    leaveRequestComment.LeaveReqCommentsAddtime.ToPointInTimeDateTimeOffset(leaveRequestComment.LeaveReqCommentsAdddate, colleagueTimeZone).Value,
                     leaveRequestComment.LeaveReqCommentsChgopr,
                     leaveRequestComment.LeaveReqCommentsChgtime.ToPointInTimeDateTimeOffset(leaveRequestComment.LeaveReqCommentsChgdate, colleagueTimeZone).Value)
                    });

                    commentsAuthorName.Clear();

                    logger.Debug(string.Format("LeaveCommentId :{0}, LeaveRequestId :{1},EmployeeId :{2}, Comment Text :{3}.",
                        leaveRequestComment.Recordkey, leaveRequestComment.LrcLeaveRequestId, leaveRequestComment.LrcEmployeeId, leaveRequestComment.LrcComments));
                }
            }
            // Sort the leave request comments based on the Timestamp
            leaveRequestCommentEntities = leaveRequestCommentEntities.OrderBy(lrc => lrc.Timestamp.AddDateTime).ToList();

            return leaveRequestCommentEntities;
        }

        /// <summary>
        ///  Helper method that builds a LeaveRequestStatus entity associated with a particular LeaveRequest record.
        /// </summary>
        /// <param name="leaveRequestStatusRecord"></param>
        /// <returns>LeaveRequestStatus Entity</returns>
        private Domain.HumanResources.Entities.LeaveRequestStatus BuildLeaveRequestStatusEntity(DataContracts.LeaveRequestStatus leaveRequestStatusRecord)
        {
            LeaveStatusAction actionType;
            if (!Enum.TryParse(leaveRequestStatusRecord.LrsActionType, true, out actionType))
            {
                var message = string.Format("Unable to resolve action type for leaveRequestStatus {0}", leaveRequestStatusRecord.Recordkey);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            logger.Debug(string.Format("LeaveRequestStatusId ={0}, LeaveRequestId ={1} ",leaveRequestStatusRecord.Recordkey,
                leaveRequestStatusRecord.LrsLeaveRequestId));

            return (new Domain.HumanResources.Entities.LeaveRequestStatus(
              leaveRequestStatusRecord.Recordkey,
              leaveRequestStatusRecord.LrsLeaveRequestId,
              actionType,
              leaveRequestStatusRecord.LrsActionerId)
            {

                Timestamp = new Timestamp(leaveRequestStatusRecord.LeaveRequestStatusAddopr,
            leaveRequestStatusRecord.LeaveRequestStatusAddtime.ToPointInTimeDateTimeOffset(leaveRequestStatusRecord.LeaveRequestStatusAdddate, colleagueTimeZone).Value,
            leaveRequestStatusRecord.LeaveRequestStatusChgopr,
            leaveRequestStatusRecord.LeaveRequestStatusChgtime.ToPointInTimeDateTimeOffset(leaveRequestStatusRecord.LeaveRequestStatusChgdate, colleagueTimeZone).Value)
            });
        }

        /// <summary>
        /// Helper method that builds a list of LeaveRequestDetail entities associated with a particular LeaveRequest record.
        /// </summary>
        /// <param name="leaveRequestDetailRecords"></param>
        /// <returns>List of LeaveRequestDetail entities</returns>                                     
        private List<Domain.HumanResources.Entities.LeaveRequestDetail> BuildLeaveRequestDetailEntities(List<DataContracts.LeaveRequestDetail> leaveRequestDetailRecords)
        {
            var leaveRequestDetailEntities = new List<Domain.HumanResources.Entities.LeaveRequestDetail>();
            if (leaveRequestDetailRecords != null && leaveRequestDetailRecords.Any())
            {
                logger.Debug("****Leave Request Details*****");
                foreach (var leaveRequestDetail in leaveRequestDetailRecords)
                {
                    leaveRequestDetailEntities.Add(new Domain.HumanResources.Entities.LeaveRequestDetail(leaveRequestDetail.Recordkey, leaveRequestDetail.LrdLeaveRequestId,
                    leaveRequestDetail.LrdLeaveDate.Value, leaveRequestDetail.LrdLeaveHours, !string.IsNullOrWhiteSpace(leaveRequestDetail.LrdPayPeriodProcessed) && leaveRequestDetail.LrdPayPeriodProcessed.Equals("Y", StringComparison.OrdinalIgnoreCase),
                    leaveRequestDetail.LeaveRequestDetailChgopr));

                    logger.Debug(string.Format("leaveRequestDetail :{0}, LeaveRequestId :{1},LeaveDetailDate :{2}, Hours :{3}.",
                        leaveRequestDetail.Recordkey, leaveRequestDetail.LrdLeaveRequestId, leaveRequestDetail.LrdLeaveDate.Value, leaveRequestDetail.LrdLeaveHours));
                }
            }
            logger.Debug("******Total Leave Requests Detail Records are = " + leaveRequestDetailEntities.Count() + "*****");
            return leaveRequestDetailEntities;
        }

        /// <summary>
        /// Helper method that builds a LeaveRequest domain entity
        /// </summary>
        /// <param name="leaveRequestRecord"></param>
        /// <param name="leaveRequestDetailRecords"></param>
        /// <param name="leaveRequestStatusRecords"></param>
        /// <param name="leaveRequestCommentsRecords"></param>
        /// <param name="currentUserId">?Current user id (optional)</param>
        /// <returns>A LeaveRequest entity</returns>
        private async Task<Domain.HumanResources.Entities.LeaveRequest> BuildLeaveRequestEntity(DataContracts.LeaveRequest leaveRequestRecord,
            List<Domain.HumanResources.Entities.LeaveRequestDetail> leaveRequestDetailEntities,
            List<Domain.HumanResources.Entities.LeaveRequestStatus> leaveRequestStatusEntities,
            List<Domain.HumanResources.Entities.LeaveRequestComment> leaveRequestCommentEntities,
            string currentUserId = null)
        {
            logger.Debug(" Process to build leave request entity started");
            if (!leaveRequestRecord.LrStartDate.HasValue)
            {
                logger.Debug("Leave Request Record must have a Start Date");
                throw new ArgumentException("LrStartDate must have a value", "leaveRequestRecord.LrStartDate");
            }
            if (!leaveRequestRecord.LrEndDate.HasValue)
            {
                logger.Debug("Leave Request Record must have a End Date");
                throw new ArgumentException("LrEndDate must have a value", "leaveRequestRecord.LrEndDate");
            }

            try
            {
                //Fetch latest leave request status entity
                var latestLeaveRequestStatusEntity = GetLatestLeaveRequestStatusEntity(leaveRequestStatusEntities);
                logger.Debug("Latest Leave Request Status Extracted");

                //Check if Leave Request Status is Approved or Rejected
                var latestStatusAction = latestLeaveRequestStatusEntity.ActionType;

                //Default Actioner Details from Leave Request Record 
                string approverId = latestLeaveRequestStatusEntity.ActionerId;
                string approverName = await ExtractActionerNameFromPersonInfo(latestLeaveRequestStatusEntity.ActionerId);
                string employeeName = await ExtractActionerNameFromPersonInfo(leaveRequestRecord.LrEmployeeId);
                logger.Debug("Approver and Employee Names computed as per name hierarchy");

                //enableDeleteForSupervisor - True when Detail and Status changes are done by same Supervisor user 
                bool enableDeleteForSupervisor = false;
                if (currentUserId != null)
                {
                    bool detailChangesBySameSupervisor = !leaveRequestDetailEntities.Where(lrd => lrd.LeaveRequestDetailChgopr != currentUserId).Any();
                    bool statusChangesBySameSupervisor = !leaveRequestStatusEntities.Where(lrs => lrs.Timestamp.ChangeOperator != currentUserId).Any();
                    enableDeleteForSupervisor = detailChangesBySameSupervisor && statusChangesBySameSupervisor;
                }

                logger.Debug(string.Format("LeaveRequestId :{0}, PerLeaveId :{1},EmployeeId :{2}, LeaveStartDate :{3}, LeaveEndDate :{4}, ApproverId :{5}",
                           leaveRequestRecord.Recordkey, leaveRequestRecord.LrPerleaveId,
                           leaveRequestRecord.LrEmployeeId, leaveRequestRecord.LrStartDate.Value,
                           leaveRequestRecord.LrEndDate.Value, approverId));

                var leaveRequestEntity = new Domain.HumanResources.Entities.LeaveRequest(leaveRequestRecord.Recordkey,
                    leaveRequestRecord.LrPerleaveId,
                    leaveRequestRecord.LrEmployeeId,
                    leaveRequestRecord.LrStartDate.Value,
                    leaveRequestRecord.LrEndDate.Value,
                    approverId,
                    approverName,
                    employeeName,
                    latestStatusAction,
                    leaveRequestDetailEntities, leaveRequestCommentEntities,
                    leaveRequestRecord.LrIsWdrwPendingApproval == "Y" ? true : false,
                    leaveRequestRecord.LrIsWithdrawn == "Y" ? true : false,
                    leaveRequestRecord.LrWithdrawOption,
                    enableDeleteForSupervisor);

                return leaveRequestEntity;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Unable to build LeaveRequest Entity for record = {0}. Exception Message = {1}", leaveRequestRecord.Recordkey, e.Message));
                throw;
            }
        }


        /// <summary>
        /// Helper method to retrieve the latest status entity of a leave request
        /// </summary>
        /// <param name="leaveRequestStatusEntities">List of Status Entities associated with a leave request</param>
        /// <returns>LeaveStatusAction Entity</returns>
        private Domain.HumanResources.Entities.LeaveRequestStatus GetLatestLeaveRequestStatusEntity(List<Domain.HumanResources.Entities.LeaveRequestStatus> leaveRequestStatusEntities)
        {
            if (leaveRequestStatusEntities == null || !leaveRequestStatusEntities.Any())
            {
                throw new ArgumentException("A Leave.Request record must have at least one associated Leave.Request.Status record", "leaveRequestStatusEntities");
            }

            return leaveRequestStatusEntities.Where(lrs => lrs.Id != null).OrderByDescending(lrs => int.Parse(lrs.Id)).FirstOrDefault();
        }


        /// <summary>
        /// Builds a leave request comments entity from its data contract
        /// </summary>
        /// <param name="dbCommment">leave request comment datacontract</param>
        /// <returns>leave request comment entity</returns>
        private async Task<LeaveRequestComment> BuildCommentsEntity(DataContracts.LeaveReqComments dbCommment)
        {
            if (dbCommment == null)
            {
                throw new ArgumentNullException("dbCommment");
            }
            if (string.IsNullOrEmpty(dbCommment.LrcLeaveRequestId))
            {
                throw new ArgumentException("leave request id must have a value");
            }
            //fetch AuthorName from Name Hierarchy.If no name found from hierarachy, return the operator name
            string commentsAuthorName = string.IsNullOrWhiteSpace(dbCommment.LeaveReqCommentsAddopr) ? dbCommment.LrcAddOpername : await ExtractCommentAuthorNameFromPersonInfo(dbCommment.LeaveReqCommentsAddopr);

            logger.Debug(string.Format("LeaveCommentId :{0}, LeaveRequestId :{1},EmployeeId :{2}, Comment Text :{3}.",
                        dbCommment.Recordkey, dbCommment.LrcLeaveRequestId, dbCommment.LrcEmployeeId, dbCommment.LrcComments));

            return (new LeaveRequestComment(
                dbCommment.Recordkey,
                dbCommment.LrcLeaveRequestId,
                dbCommment.LrcEmployeeId,
                dbCommment.LrcComments,
                commentsAuthorName)
            {
                Timestamp = new Timestamp(dbCommment.LeaveReqCommentsAddopr,
                    dbCommment.LeaveReqCommentsAddtime.ToPointInTimeDateTimeOffset(dbCommment.LeaveReqCommentsAdddate, colleagueTimeZone).Value,
                     dbCommment.LeaveReqCommentsChgopr,
                     dbCommment.LeaveReqCommentsChgtime.ToPointInTimeDateTimeOffset(dbCommment.LeaveReqCommentsChgdate, colleagueTimeZone).Value)

            });
        }



        /// Returns Actioner Name extracted from actioner Id either using
        /// Name Hierarchy or person base entity
        /// </summary>
        /// <param name="actionerId">person id of the actioner</param>
        /// <returns>Actioner Name in Last,First,Middle Format</returns>
        private async Task<string> ExtractActionerNameFromPersonInfo(string actionerId)
        {
            string actionerName = string.Empty;
            try
            {
                PersonBase personBase = null;
                var PersonBaseEntities = await GetPersonsBaseAsync(new List<string>() { actionerId });
                if (PersonBaseEntities != null && PersonBaseEntities.Any())
                {
                    personBase = PersonBaseEntities.First();
                    var personHierarchyName = await GetPersonNameFromNameHierarchy(personBase);

                    if (personHierarchyName != null)
                        actionerName = personHierarchyName.FullName;
                    else
                        actionerName = GetFormattedActionerName(personBase.LastName, personBase.FirstName, personBase.MiddleName);
                }
            }
            catch (Exception e)
            {
                var message = string.Format("Unable to determine actionerName for user {0} {1}", actionerId, e.Message);
                logger.Error(message);
            }

            return actionerName;
        }


        /// Returns Comments Author Name extracted from actioner Id either using
        /// Name Hierarchy or person base entity
        /// </summary>
        /// <param name="actionerId">person id of the actioner</param>
        /// <returns>Actioner Name in Last,First,Middle Format</returns>
        private async Task<string> ExtractCommentAuthorNameFromPersonInfo(string commentAuthorId)
        {

            string commentAuthorName = string.Empty;
            try
            {
                PersonBase personBase = null;
                var PersonBaseEntities = await GetPersonsBaseAsync(new List<string>() { commentAuthorId });
                if (PersonBaseEntities != null && PersonBaseEntities.Any())
                {
                    personBase = PersonBaseEntities.First();
                    var personHierarchyName = await GetPersonNameFromNameHierarchy(personBase);

                    if (personHierarchyName != null)
                        commentAuthorName = personHierarchyName.FullName;
                    else
                        commentAuthorName = GetFormattedCommentAuthorName(personBase.LastName, personBase.FirstName);
                }
            } catch (Exception e)
            {
                var message = string.Format("Unable to determine commentAuthorName for user {0} {1}", commentAuthorId, e.Message);
                logger.Error(message);
            }
            return commentAuthorName;
        }


        /// <summary>
        /// Formats name for an actioner based on their first last and middle name
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <returns>Formatted Name</returns>
        private string GetFormattedActionerName(string lastName, string firstName, string middleName)
        {
            string name = string.Empty;
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(middleName))
            {
                name = lastName + separator + space + firstName + space + middleName.Substring(0, 1) + ".";
            }
            else if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                name = lastName + separator + space + firstName;
            }
            else if (!string.IsNullOrEmpty(lastName))
            {
                name = lastName;
            }

            return name;

        }

        /// <summary>
        /// Formats name for an comment author based on their first and last name
        /// </summary>
        /// <param name="lastname">last name of the author</param>
        /// <param name="firstName">first name of the author</param>
        /// <returns>Name in (First Name ,Last Name) format</returns>
        private string GetFormattedCommentAuthorName(string lastname, string firstName)
        {
            // if the entity has a FirstName, use FirstName and LastName, else just return the LastName
            return !string.IsNullOrWhiteSpace(firstName) ? string.Format("{0}" + " " + "{1}", firstName, lastname) : lastname;

        }


        /// <summary>
        /// Retrieves Name from the Person Name Hierarchy Service.
        /// Builds a PersonHeirarchyName lookup dictionary.
        /// </summary>
        /// <param name="personBase">Person Base Object of the person whose name needs to be extracted</param>
        /// <returns>Person Hierarchy Name Object</returns>
        public async Task<PersonHierarchyName> GetPersonNameFromNameHierarchy(PersonBase personBase)
        {
            PersonHierarchyName personhierarchyname = null;

            //Check if the person name as per hierarchy exists in the dictionary.
            if (personNameHierarchyDict != null && personNameHierarchyDict.Any())
            {
                try
                {
                    var personnamesfromdict = personNameHierarchyDict.Where(psn => psn.Key == personBase.Id);
                    if (personnamesfromdict != null && personnamesfromdict.Any())
                    {
                        personhierarchyname = personnamesfromdict.First().Value;
                        return personhierarchyname;

                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Unable to fetch person name hierarchy from the dictionary for employee = {0}. Exception Message = {1}", personBase.Id, e.Message));
                }
            }

            //Fetch Person Name as per NAHM only when the name is not found in the dictionary.
            HRSSConfiguration hrssConfiguration = await humanResourcesReferenceDataRepository.GetHrssConfigurationAsync();
            if (hrssConfiguration != null && !string.IsNullOrEmpty(hrssConfiguration.HrssDisplayNameHierarchy))
            {
                // Calculate the person display name
                NameAddressHierarchy hierarchy = null;
                try
                {
                    hierarchy = await GetCachedNameAddressHierarchyAsync(hrssConfiguration.HrssDisplayNameHierarchy);
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, string.Format("Unable to find name address hierarchy with ID {0}. Not calculating hierarchy name.", hrssConfiguration.HrssDisplayNameHierarchy));

                }
                if (hierarchy != null)
                {
                    personhierarchyname = PersonNameService.GetHierarchyName(personBase, hierarchy);
                }

            }
            if (personhierarchyname != null)
            {
                try
                {
                    //Add this to Person Name Hierarchy Dictionary
                    if (!personNameHierarchyDict.ContainsKey(personBase.Id))
                        personNameHierarchyDict.Add(personBase.Id, personhierarchyname);
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Unable to add person name hierarchy to the dictionary for employee = {0}. Exception Message = {1}", personBase.Id, e.Message));
                }
            }

            return personhierarchyname;
        }
        #endregion
    }


}