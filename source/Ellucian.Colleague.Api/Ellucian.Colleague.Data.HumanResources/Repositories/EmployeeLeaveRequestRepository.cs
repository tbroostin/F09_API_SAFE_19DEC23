/* Copyright 2019-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
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
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Data.Base.Repositories;

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
        public EmployeeLeaveRequestRepository(ICacheProvider cacheProvider,
            IColleagueTransactionFactory transactionFactory,
            ILogger logger,
            ApiSettings settings) : base(cacheProvider, transactionFactory, logger, settings)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
            colleagueTimeZone = settings.ColleagueTimeZone;

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

            var leaveRequestCriteria = "WITH LR.EMPLOYEE.ID EQ ?";
            var leaveRequestKeys = await DataReader.SelectAsync("LEAVE.REQUEST", leaveRequestCriteria, employeeIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
            if (leaveRequestKeys == null) // || !leaveRequestKeys.Any() ??
            {
                var message = "Unexpected null returned from LEAVE.REQUEST SelectAsync";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestKeys.Any())
            {
                logger.Info("No LEAVE.REQUEST keys exist for the given employee Ids: " + string.Join(",", employeeIds));
            }

            var leaveRequestDetailCriteria = "WITH LRD.LEAVE.REQUEST.ID EQ ?";
            var leaveRequestDetailKeys = await DataReader.SelectAsync("LEAVE.REQUEST.DETAIL", leaveRequestDetailCriteria, leaveRequestKeys.Select(key => string.Format("\"{0}\"", key)).ToArray());
            if (leaveRequestDetailKeys == null) // || !leaveRequestDetailKeys.Any() ??
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.DETAIL SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestDetailKeys.Any())
            {
                logger.Info("No LEAVE.REQUEST.DETAIL keys exist for the given Leave Request Ids: " + string.Join(",", leaveRequestKeys));
            }

            var leaveRequestStatusCriteria = "WITH LRS.LEAVE.REQUEST.ID EQ ?";
            var leaveRequestStatusKeys = await DataReader.SelectAsync("LEAVE.REQUEST.STATUS", leaveRequestStatusCriteria, leaveRequestKeys.Select(key => string.Format("\"{0}\"", key)).ToArray());
            if (leaveRequestStatusKeys == null) // || !leaveRequestStatusKeys.Any() ??
            {
                var message = "Unexpected null returned from LEAVE.REQUEST.STATUS SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!leaveRequestStatusKeys.Any())
            {
                logger.Info("No LEAVE.REQUEST.STATUS keys exist for the given Leave Request Ids: " + string.Join(",", leaveRequestKeys));
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
            foreach (var leaveRequestRecord in dbLeaveRequests)
            {
                //Get all leave request details from dict and build entity
                if (leaveRequestDetailsRecordDict.Contains(leaveRequestRecord.Recordkey))
                {
                    LeaveRequestDetailEntities = BuildLeaveRequestDetailEntities(leaveRequestDetailsRecordDict[leaveRequestRecord.Recordkey].ToList());
                }

                //Get all leave request statuses from dict and build entity
                if (leaveRequestStatusRecordDict.Contains(leaveRequestRecord.Recordkey))
                {
                    LeaveRequestStatusEntities = BuildLeaveRequestStatusEntities(leaveRequestStatusRecordDict[leaveRequestRecord.Recordkey].ToList());
                }

                //Get all leave request comments from dict and build entity
                if (leaveRequestCommentsRecordDict.Contains(leaveRequestRecord.Recordkey))
                {
                    LeaveRequestCommentsEntities = BuildLeaveRequestCommentEntities(leaveRequestCommentsRecordDict[leaveRequestRecord.Recordkey].ToList());
                }
                else
                {
                    LeaveRequestCommentsEntities = null;
                }
                //Create a task for each BuildLeaveRequestEntity and add to the list
                BuildLeaveRequestEntitiesTasks.Add(BuildLeaveRequestEntity(leaveRequestRecord, LeaveRequestDetailEntities, LeaveRequestStatusEntities, LeaveRequestCommentsEntities));
            }
            //Invoke tasks parallely and collate the results
            LeaveRequestEntities.AddRange(await Task.WhenAll(BuildLeaveRequestEntitiesTasks));

            #endregion

            return LeaveRequestEntities;
        }

        /// <summary>
        /// Gets a single LeaveRequest object matching the given id. 
        /// </summary>
        /// <param name="id">Leave Request Id</param>     
        /// <returns>LeaveRequest Entity</returns>
        public async Task<Domain.HumanResources.Entities.LeaveRequest> GetLeaveRequestInfoByLeaveRequestIdAsync(string id)
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
            var leaveRequestCommentEntities = BuildLeaveRequestCommentEntities(leaveRequestCommentsRecords);

            // Build the LEAVE.REQUEST Entity
            var leaveRequestEntity = await BuildLeaveRequestEntity(leaveRequestRecord, leaveRequestDetailEntities, leaveRequestStatusEntities, leaveRequestCommentEntities);

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
                LrActionerId = status.ActionerId
            };
            CreateLeaveRequestStatusResponse response = null;
            try
            {
                // execute the request
                response = await transactionInvoker.ExecuteAsync<CreateLeaveRequestStatusRequest, CreateLeaveRequestStatusResponse>(request);
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
            if (leaveRequestStatusEntity != null && (leaveRequestStatusEntity.ActionType == LeaveStatusAction.Approved || leaveRequestStatusEntity.ActionType == LeaveStatusAction.Rejected))
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
                    LeaveRequestDetails = allLeaveRequestDetails
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

            return BuildCommentsEntity(newComment);
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
        private List<Domain.HumanResources.Entities.LeaveRequestComment> BuildLeaveRequestCommentEntities(List<DataContracts.LeaveReqComments> leaveRequestCommentRecords)
        {
            var leaveRequestCommentEntities = new List<Domain.HumanResources.Entities.LeaveRequestComment>();
            if (leaveRequestCommentRecords != null && leaveRequestCommentRecords.Any())
            {
                foreach (var leaveRequestComment in leaveRequestCommentRecords)
                {
                    leaveRequestCommentEntities.Add(new LeaveRequestComment(leaveRequestComment.Recordkey,
                                                                            leaveRequestComment.LrcLeaveRequestId,
                                                                            leaveRequestComment.LrcEmployeeId,
                                                                            leaveRequestComment.LrcComments,
                                                                            leaveRequestComment.LrcAddOpername)
                    {
                        Timestamp = new Timestamp(leaveRequestComment.LeaveReqCommentsAddopr,
                    leaveRequestComment.LeaveReqCommentsAddtime.ToPointInTimeDateTimeOffset(leaveRequestComment.LeaveReqCommentsAdddate, colleagueTimeZone).Value,
                     leaveRequestComment.LeaveReqCommentsChgopr,
                     leaveRequestComment.LeaveReqCommentsChgtime.ToPointInTimeDateTimeOffset(leaveRequestComment.LeaveReqCommentsChgdate, colleagueTimeZone).Value)
                    });
                    ;
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
                throw new ApplicationException(message);
            }
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
                foreach (var leaveRequestDetail in leaveRequestDetailRecords)
                {
                    leaveRequestDetailEntities.Add(new Domain.HumanResources.Entities.LeaveRequestDetail(
                      leaveRequestDetail.Recordkey,
                      leaveRequestDetail.LrdLeaveRequestId,
                      leaveRequestDetail.LrdLeaveDate.Value,
                      leaveRequestDetail.LrdLeaveHours));
                }
            }

            return leaveRequestDetailEntities;
        }

        /// <summary>
        /// Helper method that builds a LeaveRequest domain entity
        /// </summary>
        /// <param name="leaveRequestRecord"></param>
        /// <param name="leaveRequestDetailRecords"></param>
        /// <param name="leaveRequestStatusRecords"></param>
        /// <param name="leaveRequestCommentsRecords"></param>
        /// <returns>A LeaveRequest entity</returns>
        private async Task<Domain.HumanResources.Entities.LeaveRequest> BuildLeaveRequestEntity(DataContracts.LeaveRequest leaveRequestRecord,
            List<Domain.HumanResources.Entities.LeaveRequestDetail> leaveRequestDetailEntities,
            List<Domain.HumanResources.Entities.LeaveRequestStatus> leaveRequestStatusEntities,
            List<Domain.HumanResources.Entities.LeaveRequestComment> leaveRequestCommentEntities)
        {
            if (!leaveRequestRecord.LrStartDate.HasValue)
            {
                throw new ArgumentException("LrStartDate must have a value", "leaveRequestRecord.LrStartDate");
            }
            if (!leaveRequestRecord.LrEndDate.HasValue)
            {
                throw new ArgumentException("LrEndDate must have a value", "leaveRequestRecord.LrEndDate");
            }
            //Fetch latest leave request status entity
            var latestLeaveRequestStatusEntity = GetLatestLeaveRequestStatusEntity(leaveRequestStatusEntities);

            //Check if Leave Request Status is Approved or Rejected
            var latestStatusAction = latestLeaveRequestStatusEntity.ActionType;
            bool isStatusApprovedOrRejected = (latestStatusAction == LeaveStatusAction.Approved || latestStatusAction == LeaveStatusAction.Rejected);

            //Default Approver Details from Leave Request Record .Based on leave request status update it accordingly.
            string approverId = isStatusApprovedOrRejected ? latestLeaveRequestStatusEntity.ActionerId : leaveRequestRecord.LrApproverId;
            string approverName = isStatusApprovedOrRejected ? await ExtractActionerNameFromPersonInfo(latestLeaveRequestStatusEntity.ActionerId) : string.Empty;

            return new Domain.HumanResources.Entities.LeaveRequest(leaveRequestRecord.Recordkey,
                leaveRequestRecord.LrPerleaveId,
                leaveRequestRecord.LrEmployeeId,
                leaveRequestRecord.LrStartDate.Value,
                leaveRequestRecord.LrEndDate.Value,
                approverId,
                approverName,
                latestStatusAction,
                leaveRequestDetailEntities, leaveRequestCommentEntities);
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
        private LeaveRequestComment BuildCommentsEntity(DataContracts.LeaveReqComments dbCommment)
        {
            if (dbCommment == null)
            {
                throw new ArgumentNullException("dbCommment");
            }
            if (string.IsNullOrEmpty(dbCommment.LrcLeaveRequestId))
            {
                throw new ArgumentException("leave request id must have a value");
            }

            return (new LeaveRequestComment(
                dbCommment.Recordkey,
                dbCommment.LrcLeaveRequestId,
                dbCommment.LrcEmployeeId,
                dbCommment.LrcComments,
                dbCommment.LrcAddOpername)
            {
                Timestamp = new Timestamp(dbCommment.LeaveReqCommentsAddopr,
                    dbCommment.LeaveReqCommentsAddtime.ToPointInTimeDateTimeOffset(dbCommment.LeaveReqCommentsAdddate, colleagueTimeZone).Value,
                     dbCommment.LeaveReqCommentsChgopr,
                     dbCommment.LeaveReqCommentsChgtime.ToPointInTimeDateTimeOffset(dbCommment.LeaveReqCommentsChgdate, colleagueTimeZone).Value)

            });
        }

        /// <summary>
        /// Retrieves Person records based on list of employee IDs
        /// </summary>
        /// <param name="employeeIds">List of employee IDs whose person records are required.</param>
        /// <returns>List of Person Base Entities</returns>
        private async Task<IEnumerable<PersonBase>> GetPersonBaseEntities(IEnumerable<string> employeeIds)
        {
            IEnumerable<PersonBase> personBaseEntities = new List<PersonBase>();
            if (employeeIds != null && employeeIds.Any())
            {
                personBaseEntities = await GetPersonsBaseAsync(employeeIds);
            }

            return personBaseEntities;
        }

        /// <summary>
        /// Derives employee name from their first/last names,as per their person record
        /// </summary>
        /// <param name="firstName">first name of the employee </param>
        /// <param name="lastName">last name of the employee</param>
        /// <returns></returns>
        private string GetFormattedEmployeeName(string firstName, string lastName)
        {
            return (!string.IsNullOrWhiteSpace(firstName)) ? string.Format("{0}" + " " + "{1}", firstName, lastName) : lastName;
        }

        /// <summary>
        /// Returns Actioner Name extracted from actioner Id
        /// </summary>
        /// <param name="actionerId">person id of the actioner</param>
        /// <returns></returns>
        private async Task<string> ExtractActionerNameFromPersonInfo(string actionerId)
        {
            string actionerName = string.Empty;
            var PersonBaseEntities = await GetPersonBaseEntities(new List<string>() { actionerId });
            if (PersonBaseEntities != null && PersonBaseEntities.Any())
            {
                var person = PersonBaseEntities.First();
                actionerName = GetFormattedActionerName(person.LastName, person.FirstName, person.MiddleName);
            }
            return actionerName;
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
        #endregion
    }
}
