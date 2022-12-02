/* Copyright 2019-2022 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Coordination service for the employee leave request repository
    /// </summary>
    [RegisterType]
    public class EmployeeLeaveRequestService : BaseCoordinationService, IEmployeeLeaveRequestService
    {
        private readonly ISupervisorsRepository supervisorsRepository;
        private readonly IPersonBaseRepository personBaseRepository;
        private readonly IEmployeeLeaveRequestRepository employeeLeaveRequestRepository;

        /// <summary>
        /// Parametrized constructor for the EmployeeLeaveRequestService
        /// </summary>
        /// <param name="supervisorsRepository"></param>
        /// <param name="personBaseRepository"></param>
        /// <param name="employeeLeaveRequestRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public EmployeeLeaveRequestService(ISupervisorsRepository supervisorsRepository,
            IPersonBaseRepository personBaseRepository,
            IEmployeeLeaveRequestRepository employeeLeaveRequestRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger) : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.supervisorsRepository = supervisorsRepository;
            this.personBaseRepository = personBaseRepository;
            this.employeeLeaveRequestRepository = employeeLeaveRequestRepository;
        }

        /// <summary>
        /// Gets all leave requests for current user
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of LeaveRequest DTOs</returns>
        public async Task<IEnumerable<Dtos.HumanResources.LeaveRequest>> GetLeaveRequestsAsync(string effectivePersonId = null)
        {
            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }

            bool hasProxyAccess = HasProxyAccessForPerson(effectivePersonId, ProxyWorkflowConstants.TimeManagementLeaveApproval);

            if (!CurrentUser.IsPerson(effectivePersonId) && !(hasProxyAccess || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                throw new PermissionsException(string.Format("CurrentUser {0} is not the supervisor {1} nor has proxy access to supervisor", CurrentUser.PersonId, effectivePersonId));
            }

            var personIds = new List<string>() { effectivePersonId };

            logger.Debug("Leave Requests will be retrieved for the person :- " + effectivePersonId);
            //Check if user has leave approver permission
            if (HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest))
            {
                try
                {
                    logger.Debug(string.Format("{0} has Leave Approver Permission", effectivePersonId));
                    personIds.AddRange(await supervisorsRepository.GetSuperviseesByPrimaryPositionForSupervisorAsync(effectivePersonId));
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    logger.Error("Error getting supervisor employees", e.Message);
                }
            }
            var leaveRequestEntities = await employeeLeaveRequestRepository.GetLeaveRequestsAsync(personIds);

            var leaveRequestEntityToDtoAdapter = _adapterRegistry.GetAdapter<LeaveRequest, Dtos.HumanResources.LeaveRequest>();

            var leaveRequestDTOs = leaveRequestEntities.Select(lre => leaveRequestEntityToDtoAdapter.MapToType(lre)).ToList();

            return leaveRequestDTOs;
        }

        /// <summary>
        /// Gets the Approved Leave Requests for a timecard week based on the date range. 
        /// </summary>
        /// <param name="startDate">Start date of timecard week</param>
        /// <param name="endDate">End date of timecard week</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of Leave Request DTO</returns>
        public async Task<IEnumerable<Dtos.HumanResources.LeaveRequest>> GetLeaveRequestsForTimeEntryAsync(DateTime startDate, DateTime endDate, string effectivePersonId = null)
        {
            //Take LoggedInUser Id when no id is passed
            if (string.IsNullOrWhiteSpace(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }

            if (!CurrentUser.IsPerson(effectivePersonId)
                && !HasPermission(HumanResourcesPermissionCodes.ApproveRejectEmployeeTimecard)
                && !HasProxyAccessForPerson(effectivePersonId, ProxyWorkflowConstants.TimeManagementTimeApproval))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to fetch leave request information of {1}", CurrentUser.PersonId, effectivePersonId));
            }

            var personIds = new List<string>() { effectivePersonId };
            var superviseeIds = await supervisorsRepository.GetSuperviseesBySupervisorAsync(effectivePersonId);
            logger.Debug("Supervisees data retrieved");
            if (superviseeIds == null || !superviseeIds.Any())
            {
                logger.Error(string.Format("Supervisor {0} does not supervise any employees", effectivePersonId));
            }
            else
            {
                personIds.AddRange(superviseeIds);
            }

            //Invoke repository method
            var leaveRequestEntities = await employeeLeaveRequestRepository.GetLeaveRequestsForTimeEntryAsync(startDate, endDate, personIds.Distinct());
            var leaveRequestEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.LeaveRequest, Dtos.HumanResources.LeaveRequest>();
            var leaveRequestDTOs = leaveRequestEntities.Select(lre => leaveRequestEntityToDtoAdapter.MapToType(lre)).ToList();

            return leaveRequestDTOs;
        }

        /// <summary>
        /// Service method that gets a single LeaveRequest object matching the given id based on the user's permissions. 
        /// </summary>
        /// <param name="id">Leave Request Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>LeaveRequest DTO</returns>
        public async Task<Dtos.HumanResources.LeaveRequest> GetLeaveRequestInfoByLeaveRequestIdAsync(string id, string effectivePersonId = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("leaveRequestId");
            }

            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }

            bool hasProxyAccess = HasProxyAccessForPerson(effectivePersonId, ProxyWorkflowConstants.TimeManagementLeaveApproval);

            if (!CurrentUser.IsPerson(effectivePersonId) && !(hasProxyAccess || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                throw new PermissionsException(string.Format("CurrentUser {0} is not the supervisor {1} nor has proxy access to supervisor", CurrentUser.PersonId, effectivePersonId));
            }

            try
            {
                //Added new param currentUserId to compute the flag to show/hide Delete button for Supervisor user
                var leaveRequestEntity = await employeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(id, CurrentUser.PersonId);
                if (leaveRequestEntity == null)
                {
                    var message = "Unexpected null returned from the repository";
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                // If the leaveRequestEntity's employeeId doesn't match the current user's personId, then throw a permission exception.
                // Note: Approver of this leave request is allowed to view this leave request info.
                if (!CurrentUser.IsPerson((leaveRequestEntity.EmployeeId)) && !(await IsAuthorizedLeaveSupervisorAsync(effectivePersonId, leaveRequestEntity.EmployeeId)))
                {
                    throw new PermissionsException(string.Format("User {0} does not have permission to view the leave request info {1}", effectivePersonId, leaveRequestEntity.EmployeeId));
                }

                var leaveRequestEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.LeaveRequest, Dtos.HumanResources.LeaveRequest>();

                return leaveRequestEntityToDtoAdapter.MapToType(leaveRequestEntity);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Creates a single Leave Request along with its details.
        /// </summary>
        /// <param name="leaveRequest">Leave Request DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly created Leave Request Object</returns>
        public async Task<Dtos.HumanResources.LeaveRequest> CreateLeaveRequestAsync(Dtos.HumanResources.LeaveRequest leaveRequest, string effectivePersonId = null)
        {
            if (leaveRequest == null)
            {
                throw new ArgumentNullException("leaveRequest");
            }

            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }

            bool hasProxyAccess = HasProxyAccessForPerson(effectivePersonId, ProxyWorkflowConstants.TimeManagementLeaveApproval);
            if (!CurrentUser.IsPerson(leaveRequest.EmployeeId) && !(hasProxyAccess || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                throw new PermissionsException("User does not have permission to create leave request information");
            }

            // Get adapter to convert the dto to domain entity...
            var leaveRequestDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.HumanResources.LeaveRequest, Domain.HumanResources.Entities.LeaveRequest>();
            var leaveRequestEntity = leaveRequestDtoToEntityAdapter.MapToType(leaveRequest);
            IEnumerable<string> personIds = new List<string>()
            {
                leaveRequestEntity.EmployeeId
            };

            //Check if the employee whose leave request is being created, is actually a valid supervisee of this logged in user(Supervisor)
            if (effectivePersonId != leaveRequestEntity.EmployeeId)
            {
                if (!await IsAuthorizedLeaveSupervisorAsync(effectivePersonId, leaveRequestEntity.EmployeeId))
                {
                    throw new PermissionsException("User does not have permission to create leave request information");
                }
            }

            logger.Debug(string.Format("Permission checks cleared. {0} is allowed to create a new leave request", effectivePersonId));
            // Check if the user already has an existing leave request record same as the input leaveRequest.
            string existingLeaveRequestRecordId;

            // Fetch all the existing leave requests for the effectivePerson
            var existingLeaveRequests = await employeeLeaveRequestRepository.GetLeaveRequestsAsync(personIds);

            bool leaveRequestToBeCreatedExists = CheckForExistingLeaveRequestRecord(existingLeaveRequests, leaveRequestEntity, out existingLeaveRequestRecordId);
            if (leaveRequestToBeCreatedExists)
            {
                var message = string.Format("Current user {0} already has a leave request with Id {1}  for the requested leave type and date range.", CurrentUser.PersonId, existingLeaveRequestRecordId);
                logger.Error(message);
                throw new ExistingResourceException(message, existingLeaveRequestRecordId);
            }

            // Check for 24 hours validation for leave request details.
            var dailyHoursErrorDetails = CheckForDailyHoursLimitPerDay(existingLeaveRequests, leaveRequestEntity);

            if (dailyHoursErrorDetails != null && dailyHoursErrorDetails.Any())
            {
                // Updating the leaveRquest Object with Error details with dates and hours and returning the same object
                leaveRequest.LeaveRequestDailyHoursErrorDetails = dailyHoursErrorDetails;
                return leaveRequest;
            }

            // Create new leave request
            var newLeaveRequest = await employeeLeaveRequestRepository.CreateLeaveRequestAsync(leaveRequestEntity);

            // Convert the domain entity to DTO and return the newly created leave request.
            var leaveRequestEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.LeaveRequest, Dtos.HumanResources.LeaveRequest>(); ;
            return leaveRequestEntityToDtoAdapter.MapToType(newLeaveRequest);

        }

        /// <summary>
        /// Creates a single Leave LeaveRequestStatus.
        /// </summary>
        /// <param name="status">Leave Request Status DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly created Leave Request Status Object</returns>
        public async Task<Dtos.HumanResources.LeaveRequestStatus> CreateLeaveRequestStatusAsync(Dtos.HumanResources.LeaveRequestStatus status, string effectivePersonId = null)
        {

            if (status == null)
            {
                throw new ArgumentNullException("status");
            }

            if (string.IsNullOrEmpty(status.LeaveRequestId))
            {
                throw new ArgumentException("leave request Id attribute is required in status");
            }

            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }

            bool hasProxyAccess = HasProxyAccessForPerson(effectivePersonId, ProxyWorkflowConstants.TimeManagementLeaveApproval);

            if (!CurrentUser.IsPerson(effectivePersonId) && !(hasProxyAccess || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                throw new PermissionsException(string.Format("CurrentUser {0} is not the supervisor {1} nor has proxy access to supervisor", CurrentUser.PersonId, effectivePersonId));
            }

            // If the leaveRequestEntity's employeeId doesn't match the current user's personId, then throw a permission exception.
            // Note: Approver/Proxy Approver of this leave request is allowed to create a leave request status for this leave request.
            var leaveRequestEntity = await employeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(status.LeaveRequestId);
            if (leaveRequestEntity == null)
            {
                var message = "Unexpected null returned from the repository";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!CurrentUser.IsPerson((leaveRequestEntity.EmployeeId)) && !(await IsAuthorizedLeaveSupervisorAsync(effectivePersonId, leaveRequestEntity.EmployeeId)))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to create the leave request status for the leave request of {1}", effectivePersonId, leaveRequestEntity.EmployeeId));
            }
            logger.Debug(string.Format("Permission checks cleared. {0} is allowed to create a new leave request status", effectivePersonId));

            var statusDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.HumanResources.LeaveRequestStatus, Domain.HumanResources.Entities.LeaveRequestStatus>();
            var statusEntity = statusDtoToEntityAdapter.MapToType(status);

            var newLeaveRequestStatusEntity = await employeeLeaveRequestRepository.CreateLeaveRequestStatusAsync(statusEntity);
            var statusEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.LeaveRequestStatus, Dtos.HumanResources.LeaveRequestStatus>();

            var newLeaveRequestStatusDTO = statusEntityToDtoAdapter.MapToType(newLeaveRequestStatusEntity);

            return newLeaveRequestStatusDTO;
        }

        /// <summary>
        /// Gets the HumanResourceDemographics information of supervisors for the given position of a supervisee.</summary>
        /// <param name="positionId">Position Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id.</param>
        /// <returns>List of HumanResourceDemographics DTOs</returns>
        public async Task<IEnumerable<Dtos.HumanResources.HumanResourceDemographics>> GetSupervisorsByPositionIdAsync(string positionId, string effectivePersonId = null)
        {
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId");
            }
            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            else if (!CurrentUser.IsPerson(effectivePersonId))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to access the list of supervisors for the position of the {1}", CurrentUser.PersonId, effectivePersonId));
            }
            // Get all the supervisorIds for the input positionId
            var supervisorIds = await supervisorsRepository.GetSupervisorsByPositionIdAsync(positionId, effectivePersonId);

            logger.Debug(string.Format("Supervisors based on position id {0} fetched", positionId));
            List<PersonBase> personBaseEntities = new List<PersonBase>();
            if (supervisorIds != null && supervisorIds.Any())
            {
                var personEntities = await personBaseRepository.GetPersonsBaseAsync(supervisorIds);
                if (personEntities != null && personEntities.Any())
                    personBaseEntities = personEntities.ToList();
            }

            var personBaseEntityToHumanResourceDemographicsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>();
            return personBaseEntities.Select(pb => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(pb)).ToList();
        }

        /// <summary>
        /// Updates an existing Leave Request record.
        /// </summary>
        /// <param name="leaveRequest">Leave Request to be updated</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly updated LeaveRequest DTO</returns>
        public async Task<Dtos.HumanResources.LeaveRequest> UpdateLeaveRequestAsync(Dtos.HumanResources.LeaveRequest leaveRequest, string effectivePersonId = null)
        {
            if (leaveRequest == null)
            {
                throw new ArgumentNullException("leaveRequest");
            }
            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            else if (!CurrentUser.IsPerson(effectivePersonId) && !HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest))
            {
                throw new PermissionsException("User does not have permission to update the leave request");
            }

            // Convert the input leaveRequest DTO to LeaveRequest Entity
            var leaveRequestDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.HumanResources.LeaveRequest, Domain.HumanResources.Entities.LeaveRequest>();
            var leaveRequestEntity = leaveRequestDtoToEntityAdapter.MapToType(leaveRequest);
            IEnumerable<string> personIds = new List<string>()
            {
                leaveRequestEntity.EmployeeId
            };

            // Check if the employee whose leave request is being created, is actually a valid supervisee of this logged in user(Supervisor)
            if (effectivePersonId != leaveRequestEntity.EmployeeId)
            {
                if (!await IsAuthorizedLeaveSupervisorAsync(effectivePersonId, leaveRequestEntity.EmployeeId))
                {
                    throw new PermissionsException("User does not have permission to update leave request information");
                }
            }

            string existingLeaveRequestRecordId;

            // Fetch all the existing leave requests for the effectivePerson
            var existingLeaveRequests = await employeeLeaveRequestRepository.GetLeaveRequestsAsync(personIds);

            // Check if the user already has an existing leave request record same as the input leaveRequest.
            // If such record is found, do not update the leave request.
            bool leaveRequestToBeUpdatedExists = CheckForExistingLeaveRequestRecord(existingLeaveRequests, leaveRequestEntity, out existingLeaveRequestRecordId);
            if (leaveRequestToBeUpdatedExists)
            {
                var message = string.Format("Current user {0} already has a leave request with Id {1}  for the requested leave type and date range.", CurrentUser.PersonId, existingLeaveRequestRecordId);
                logger.Error(message);
                throw new ExistingResourceException(message, existingLeaveRequestRecordId);
            }

            // Get the corresponding leave request from the DB
            var existingLeaveRequest = await employeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(leaveRequest.Id);
            if (existingLeaveRequest == null)
            {
                var message = string.Format("Failed to update the leave request {0} as no existing leave request was found in the DB", leaveRequest.Id);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            // Compare the input leaveRequest with the existing leave request record.         
            if (existingLeaveRequest.CompareTo(leaveRequestEntity) != 0)
            {
                var message = string.Format("The input leaveRequest {0} has different identifying attributes than the existing leaveRequest {1}", leaveRequestEntity.ToString(), existingLeaveRequest.ToString());
                logger.Error(message);
                throw new ArgumentException(message);
            }

            // Permission checks - User can only update their own leave requests.
            bool hasProxyAccess = HasProxyAccessForPerson(effectivePersonId, ProxyWorkflowConstants.TimeManagementLeaveApproval);

            if (!CurrentUser.IsPerson(existingLeaveRequest.EmployeeId) && !(hasProxyAccess || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                var message = string.Format("Current user doesn't have the permission to update the leave request {0}.", leaveRequest.Id);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            logger.Debug(string.Format("Permission checks cleared. {0} is allowed to Update the leave request", effectivePersonId));
            // Check for 24 hours validation for leave request details.
            var dailyHoursErrorDetails = CheckForDailyHoursLimitPerDay(existingLeaveRequests, leaveRequestEntity);

            if (dailyHoursErrorDetails != null && dailyHoursErrorDetails.Any())
            {
                // Updating the leaveRquest Object with Error details with dates and hours and returning the same object
                leaveRequest.LeaveRequestDailyHoursErrorDetails = dailyHoursErrorDetails;
                return leaveRequest;
            }

            // Create the leave request helper to determine what leave request details to update/delete/create
            var updateLeaveRequestHelper = BuildLeaveRequestHelper(leaveRequestEntity, existingLeaveRequest);

            // Update the leave request
            var updatedLeaveRequest = await employeeLeaveRequestRepository.UpdateLeaveRequestAsync(updateLeaveRequestHelper);

            var leaveRequestEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.LeaveRequest, Dtos.HumanResources.LeaveRequest>();
            return leaveRequestEntityToDtoAdapter.MapToType(updatedLeaveRequest);
        }

        /// <summary>
        /// Creates a new leave request comment associated with a leave request.
        /// </summary>
        /// <param name="leaveRequestComment">Leave Request Comment DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Leave Request Comment DTO</returns>
        public async Task<Dtos.HumanResources.LeaveRequestComment> CreateLeaveRequestCommentsAsync(Dtos.HumanResources.LeaveRequestComment leaveRequestComment, string effectivePersonId = null)
        {
            if (leaveRequestComment == null)
            {
                throw new ArgumentNullException("leaveRequestComment");
            }

            if (string.IsNullOrEmpty(leaveRequestComment.LeaveRequestId))
            {
                throw new ArgumentException("leave request Id attribute is required in leaveRequestComment");
            }

            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }

            bool hasProxyAccess = HasProxyAccessForPerson(effectivePersonId, ProxyWorkflowConstants.TimeManagementLeaveApproval);

            if (!CurrentUser.IsPerson(effectivePersonId) && !(hasProxyAccess || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                throw new PermissionsException(string.Format("CurrentUser {0} is not the supervisor {1} nor has proxy access to supervisor", CurrentUser.PersonId, effectivePersonId));
            }

            // If the leaveRequestEntity's employeeId doesn't match the current user's personId, then throw a permission exception.
            // Note: Approver of this leave request is allowed to create a comment for this leave request.
            var leaveRequestEntity = await employeeLeaveRequestRepository.GetLeaveRequestInfoByLeaveRequestIdAsync(leaveRequestComment.LeaveRequestId);
            if (leaveRequestEntity == null)
            {
                var message = "Unexpected null returned from the repository";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            if (!CurrentUser.IsPerson((leaveRequestEntity.EmployeeId)) && !(await IsAuthorizedLeaveSupervisorAsync(effectivePersonId, leaveRequestEntity.EmployeeId)))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to create a comment for the leave request of {1}", effectivePersonId, leaveRequestEntity.EmployeeId));
            }
            logger.Debug(string.Format("Permission checks cleared. {0} is allowed to create the leave request comment", effectivePersonId));
            //Get the Comment Authour name from person base
            var personBaseEntity = await personBaseRepository.GetPersonBaseAsync(CurrentUser.PersonId);
            string commentAuthorName = CurrentUser.PersonId;
            if (personBaseEntity != null)
            {
                // if the entity has a FirstName, use FirstName and LastName, else just set dto.CommentAuthorName to LastName
                if (!string.IsNullOrWhiteSpace(personBaseEntity.FirstName))
                    commentAuthorName = string.Format("{0}" + " " + "{1}", personBaseEntity.FirstName, personBaseEntity.LastName);

                else
                    commentAuthorName = personBaseEntity.LastName;
            }

            //Update the input DTO
            leaveRequestComment.CommentAuthorName = commentAuthorName;

            // Get adapter to convert the dto to domain entity...
            var leaveRequestCommentDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.HumanResources.LeaveRequestComment, Domain.HumanResources.Entities.LeaveRequestComment>();
            var leaveRequestCommentEntity = leaveRequestCommentDtoToEntityAdapter.MapToType(leaveRequestComment);

            // Create new leave request comment
            var newLeaveRequestCommentEntity = await employeeLeaveRequestRepository.CreateLeaveRequestCommentsAsync(leaveRequestCommentEntity);

            //convert Entity from repository to DTO
            var leaveRequestCommentEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.LeaveRequestComment, Dtos.HumanResources.LeaveRequestComment>();
            var newLeaveRequestCommentDTO = leaveRequestCommentEntityToDtoAdapter.MapToType(newLeaveRequestCommentEntity);

            return newLeaveRequestCommentDTO;
        }

        #region Helper_Methods
        /// <summary>
        /// Helper method that creates a LeaveRequestHelper used for updating an existing leave request record
        /// </summary>
        /// <param name="leaveRequestEntityToUpdate"></param>
        /// <param name="existingLeaveRequest"></param>
        /// <returns>A LeaveRequestHelper object</returns>
        private LeaveRequestHelper BuildLeaveRequestHelper(Domain.HumanResources.Entities.LeaveRequest leaveRequestToUpdate, Domain.HumanResources.Entities.LeaveRequest existingLeaveRequest)
        {
            if (leaveRequestToUpdate == null)
            {
                throw new ArgumentNullException("leaveRequestToUpdate");
            }
            if (existingLeaveRequest == null)
            {
                throw new ArgumentNullException("existingLeaveRequest");
            }

            LeaveRequestHelper leaveRequestHelper = new LeaveRequestHelper(leaveRequestToUpdate);

            foreach (var leaveRequestDetailToUpdate in leaveRequestToUpdate.LeaveRequestDetails)
            {
                // No Id means this is a new leave request detail.
                if (string.IsNullOrEmpty(leaveRequestDetailToUpdate.Id))
                {
                    leaveRequestHelper.LeaveRequestDetailsToCreate.Add(leaveRequestDetailToUpdate);
                }
                else
                {
                    // Find the matching existing leave request detail.
                    var existingLeaveRequestDetail = existingLeaveRequest.LeaveRequestDetails.FirstOrDefault(existing => existing.Equals(leaveRequestDetailToUpdate));
                    if (existingLeaveRequestDetail != null)
                    {
                        // Check for a difference and if found add to the list of leave request details to be updated.
                        if (!leaveRequestDetailToUpdate.CompareLeaveRequestDetail(existingLeaveRequestDetail))
                        {
                            leaveRequestHelper.LeaveRequestDetailsToUpdate.Add(leaveRequestDetailToUpdate);
                        }
                    }
                }
            }

            // Delete those leave request detail records that are in the DB but not in the input LeaveRequest object            
            leaveRequestHelper.LeaveRequestDetailsToDelete = existingLeaveRequest.LeaveRequestDetails.Where(e => !leaveRequestToUpdate.LeaveRequestDetails.Contains(e)).ToList();

            //if the current logged in user is not Employee, then set the IsLeaveRequestFromSupervisorOrProxy to true.
            if (!CurrentUser.IsPerson(leaveRequestToUpdate.EmployeeId))
            {
                leaveRequestHelper.IsLeaveRequestFromSupervisorOrProxy = true;
            }

            return leaveRequestHelper;
        }

        /// <summary>
        /// Checks if the user already has an existing leave request record same as the input leaveRequest.
        /// Existing leave record if exists, would have the same leave type(PerLeaveId) as that of the input leaveRequest and start and end date of the input leaveRequest falls within the range of existing leave record's dates.
        /// </summary>
        /// <param name="effectivePersonId"></param>
        /// <param name="leaveRequest"></param>
        /// <returns>A boolean value</returns>
        private bool CheckForExistingLeaveRequestRecord(IEnumerable<Domain.HumanResources.Entities.LeaveRequest> existingLeaveRequests, Domain.HumanResources.Entities.LeaveRequest leaveRequest, out string existingLeaveRequestRecordId)
        {
            if (existingLeaveRequests == null)
            {
                throw new ArgumentNullException("existingLeaveRequests");
            }
            if (leaveRequest == null)
            {
                throw new ArgumentNullException("leaveRequest");
            }

            bool isDuplicateRecordFound = false;
            existingLeaveRequestRecordId = string.Empty;

            if (existingLeaveRequests.Any())
            {
                // Filter the existing leave requests based on the PerLeaveId and exclude requests in deleted status
                var filteredExistingLeaveRequests = existingLeaveRequests.Where(elr => elr.PerLeaveId == leaveRequest.PerLeaveId && elr.Status != Domain.HumanResources.Entities.LeaveStatusAction.Deleted);

                if (filteredExistingLeaveRequests != null && filteredExistingLeaveRequests.Any())
                {
                    //Exclude existing leave request. Allow the updation of an existing leave request.
                    filteredExistingLeaveRequests = filteredExistingLeaveRequests.Where(x => x.Id != leaveRequest.Id);
                    foreach (var existingLeaveRequest in filteredExistingLeaveRequests)
                    {
                        if (existingLeaveRequest != null)
                        {

                            if (existingLeaveRequest != null)
                            {
                                // Check if the start and end date of the input leaveRequest falls within the range of existingRequest's dates. 
                                isDuplicateRecordFound = ((existingLeaveRequest.StartDate <= leaveRequest.StartDate) && (existingLeaveRequest.EndDate >= leaveRequest.StartDate)) ||
                                                               ((existingLeaveRequest.StartDate <= leaveRequest.EndDate) && (existingLeaveRequest.EndDate >= leaveRequest.EndDate)) ||
                                                               ((existingLeaveRequest.StartDate >= leaveRequest.StartDate) && (existingLeaveRequest.EndDate <= leaveRequest.EndDate));

                                // Avoid further iterations in case a duplicate record is found.
                                if (isDuplicateRecordFound)
                                {
                                    existingLeaveRequestRecordId = existingLeaveRequest.Id;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return isDuplicateRecordFound;
        }

        /// <summary>
        /// Checks hour limit per day for the selected days for all leave type        
        /// </summary>
        /// <param name="existingLeaveRequests">Existing leave request for the effective person id</param>
        /// <param name="leaveRequest">new leave request object to created or updated</param>
        /// <returns>List of LeaveRequestDetail for the date exceeding 24 hours</returns>
        private List<Dtos.HumanResources.LeaveRequestDetail> CheckForDailyHoursLimitPerDay(IEnumerable<Domain.HumanResources.Entities.LeaveRequest> existingLeaveRequests, Domain.HumanResources.Entities.LeaveRequest leaveRequest)
        {
            if (existingLeaveRequests == null)
            {
                throw new ArgumentNullException("existingLeaveRequests");
            }
            if (leaveRequest == null)
            {
                throw new ArgumentNullException("leaveRequest");
            }

            List<Dtos.HumanResources.LeaveRequestDetail> totalHoursList = new List<Dtos.HumanResources.LeaveRequestDetail>();

            if (existingLeaveRequests.Any())
            {
                // Filter the existing leave requests based on the PerLeaveId and exclude requests in deleted and rejected status
                var filteredExistingLeaveRequests = existingLeaveRequests.Where(elr => elr.PerLeaveId != leaveRequest.PerLeaveId && elr.Status != Domain.HumanResources.Entities.LeaveStatusAction.Deleted && elr.Status != Domain.HumanResources.Entities.LeaveStatusAction.Rejected);
                foreach (var item in leaveRequest.LeaveRequestDetails)
                {
                    // Filtering the data based on the selected date           
                    var filterdDateDetails = filteredExistingLeaveRequests.SelectMany(lr => lr.LeaveRequestDetails.Where(lrd => lrd.LeaveDate == item.LeaveDate)).ToList();
                    filterdDateDetails.Add(item);
                    decimal totalHours = (filterdDateDetails.Sum(lr => lr.LeaveHours) ?? 0);
                    if (totalHours > 24)
                    {
                        totalHoursList.Add(new Dtos.HumanResources.LeaveRequestDetail()
                        {
                            LeaveDate = item.LeaveDate,
                            LeaveHours = totalHours
                        });
                    }
                }

            }
            return totalHoursList;
        }

        /// <summary>
        /// Retreives list of Supervisees for a Leave Approver/Supervisor
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of HumanDemographics DTOs containing supervisee information</returns>
        public async Task<IEnumerable<Dtos.HumanResources.HumanResourceDemographics>> GetSuperviseesByPrimaryPositionForSupervisorAsync(string effectivePersonId = null)
        {
            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }

            bool hasProxyAccess = HasProxyAccessForPerson(effectivePersonId, ProxyWorkflowConstants.TimeManagementLeaveApproval);

            if (!CurrentUser.IsPerson(effectivePersonId) && !(hasProxyAccess || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                throw new PermissionsException(string.Format("CurrentUser {0} is not the supervisor {1} nor has proxy access to supervisor", CurrentUser.PersonId, effectivePersonId));
            }
            logger.Debug(string.Format("Permission checks cleared for employee  {0}", effectivePersonId));
            // Get all the supervisorIds for the input positionId
            var supervisorIds = await supervisorsRepository.GetSuperviseesByPrimaryPositionForSupervisorAsync(effectivePersonId);
            logger.Debug("Supervisees by primary position retrieved for supervisor {0}",effectivePersonId);

            List<PersonBase> personBaseEntities = new List<PersonBase>();
            if (supervisorIds != null && supervisorIds.Any())
            {
                var personEntities = await personBaseRepository.GetPersonsBaseAsync(supervisorIds);
                if (personEntities != null && personEntities.Any())
                {
                    personBaseEntities = personEntities.ToList();
                    foreach(var personBaseEntity in personBaseEntities)
                    {
                        var personDisplayName = await employeeLeaveRequestRepository.GetPersonNameFromNameHierarchy(personBaseEntity);
                        personBaseEntity.PersonDisplayName = personDisplayName;
                    }
                }
            }
            var personBaseEntityToHumanResourceDemographicsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>();
            return personBaseEntities.Select(pb => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(pb)).ToList();
        }

        /// <summary>
        /// Checks if the supervisor is authorized to view employee's leave request information
        /// </summary>
        /// <param name="supervisorId">Effective person id of Supervisor</param>
        /// <param name="employeeId">Effective person id of employee </param>
        /// <param name="supervisedEmployeeIds">optional list of supervisees</param>
        /// <returns>True or False</returns>
        private async Task<bool> IsAuthorizedLeaveSupervisorAsync(string supervisorId, string employeeId, IEnumerable<string> supervisedEmployeeIds = null)
        {
            if (HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest))
            {
                supervisedEmployeeIds = supervisedEmployeeIds == null ? await supervisorsRepository.GetSuperviseesByPrimaryPositionForSupervisorAsync(supervisorId) : supervisedEmployeeIds;
                return (supervisedEmployeeIds != null && supervisedEmployeeIds.Contains(employeeId));
            }
            else
            {
                logger.Error(string.Format("Supervisor {0} does not have proper permission code to interact with employee {1} leave request information", supervisorId, employeeId));
            }

            return false;
        }

        #endregion
    }
}

