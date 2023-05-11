﻿/*Copyright 2019-2022 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using System.Collections;
using Ellucian.Colleague.Dtos.HumanResources;
using System.Net.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// API end-points related to employee leave rquest.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeLeaveRequestController : BaseCompressedApiController
    {
        private readonly IEmployeeLeaveRequestService _employeeLeaveRequestService;
        private readonly ILogger _logger;

        private const string existingResourceErrorMessage = "Cannot create resource that already exists.";
        private const string recordLockErrorMessage = "The record you tried to access was locked.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private static readonly string noLeaveRequestIdErrorMessage = "leaveRequestId is required in the request";
        private static readonly string noPositionIdErrorMessage = "positionId is required in the request";
        private static readonly string unexpectedErrorMessage = "Unexpected error occurred while getting leave request details";
        private static readonly string positionSupervisorsUnexpectedErrorMessage = "Unexpected error occurred while getting the position supervisors information";
        private static readonly string noLeaveRequestObjectErrorMessage = "Leave Request DTO is required in body of request";
        private static readonly string noLeaveRequestCommentObjectErrorMessage = "Leave Request Comment DTO is required in body of the request";
        private const string getLeaveRequestRouteId = "GetLeaveRequestInfoByLeaveRequestId";
        private static readonly string supervisorsUnexpectedErrorMessage = "Unexpected error occurred while getting the supervisee information by their primary position";
        private static readonly string unexpectedErrorMessageLeaveRequestsForTimeEntry = "Unexpected error occurred while getting leave request details for the specified date range";
        /// <summary>
        /// Initializes a new instance of the EmployeeLeaveRequestController class.
        /// </summary>
        /// <param name="employeeLeaveRequestService"></param>
        /// <param name="logger"></param>
        public EmployeeLeaveRequestController(IEmployeeLeaveRequestService employeeLeaveRequestService, ILogger logger)
        {
            _employeeLeaveRequestService = employeeLeaveRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all leave requests for the currently authenticated API user .
        /// All leave requests will be returned regardless of status.
        /// The endpoint will not return the leave requests if:
        ///     1.  403 - User does not have permission to get requested leave request
        ///</summary>
        /// <accessComments>
        /// If the current user is an employee, all of the employee's leave requests will be returned.
        /// If the current user is a leave approver or a proxy of the leave approver, leave requests of all the supervisees will be returned. 
        /// </accessComments>
        /// <param name="effectivePersonId">
        ///  Optional parameter for passing effective person Id
        /// </param>
        /// <returns>A list of Leave Requests</returns>
        [HttpGet]
        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsAsync([FromUri] string effectivePersonId = null)
        {
            try
            {
                _logger.Debug("************Start- Process to get Leave Request for a speciifc/logged in user - "+ effectivePersonId + " - Start************");
                var leaveRequests = await _employeeLeaveRequestService.GetLeaveRequestsAsync(effectivePersonId);
                _logger.Debug("************End- Process to get Leave Requests for a speciifc/logged in user- " + effectivePersonId + " End************");

                return leaveRequests;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Gets the LeaveRequest information corresponding to the input id.
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can view their own leave request information.
        /// 2) Leave approvers(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxies can view the leave request information of their supervisees. 
        /// </accessComments>
        /// <param name="id">Leave Request Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>LeaveRequest DTO</returns>
        [HttpGet]
        public async Task<LeaveRequest> GetLeaveRequestInfoByLeaveRequestIdAsync([FromUri] string id, [FromUri] string effectivePersonId = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.Error(noLeaveRequestIdErrorMessage);
                throw CreateHttpResponseException(noLeaveRequestIdErrorMessage, HttpStatusCode.BadRequest);
            }
            try
            {
                _logger.Debug("************Start- Process to get Leave Requests for a specific leave request Id - Start************");
                var employeeleaverequests = await _employeeLeaveRequestService.GetLeaveRequestInfoByLeaveRequestIdAsync(id, effectivePersonId);
                _logger.Debug("************End- Process to get Leave Requests for a specific leave request Id - End************");

                return employeeleaverequests;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format(pe.Message);
                _logger.Error(pe, message);
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(unexpectedErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Approved Leave Requests for a timecard week based on the date range.
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can view their own leave request information within the date range.
        /// 2) Timecard approvers(users with the permission APPROVE.REJECT.TIME.ENTRY) or their proxies can view the leave request information of their supervisees, within the date range. 
        /// </accessComments>
        /// <param name="startDate">Start date of timecard week</param>
        /// <param name="endDate">End date of timecard week</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of LeaveRequest DTO</returns>
        [HttpGet]
        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsForTimeEntryAsync(DateTime startDate, DateTime endDate, string effectivePersonId = null)
        {
            try
            {
                _logger.Debug("************Start- Process to get Leave Requests for time entry - Start************");
                var leaverequests = await _employeeLeaveRequestService.GetLeaveRequestsForTimeEntryAsync(startDate, endDate, effectivePersonId);
                _logger.Debug("************End- Process to get Leave Requests for time entry - End************");

                return leaverequests;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ane)
            {
                var message = "Unexpected null value found in argument(s)";
                _logger.Error(ane, ane.Message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                var message = "User doesn't have permissions to view approved leave requests for the specified date range ";
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(unexpectedErrorMessageLeaveRequestsForTimeEntry, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Creates a single Leave Request. This POST endpoint will create a Leave Request along with its associated leave request details 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can create their own leave request     
        /// Supervisor can create leave requests on behalf of their supervisees
        /// The endpoint will reject the creation of a Leave Request if Employee does not have the correct permission.
        /// </accessComments>
        /// <param name="leaveRequest">Leave Request DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly created Leave Request Object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> CreateLeaveRequestAsync([FromBody] LeaveRequest leaveRequest, [FromUri] string effectivePersonId = null)
        {
            if (leaveRequest == null)
            {
                throw CreateHttpResponseException("Leave Request DTO is required in body of request");
            }
            try
            {
                _logger.Debug("************Start - Process to create Leave Request -- Start ************");
                var newLeaveRequest = await _employeeLeaveRequestService.CreateLeaveRequestAsync(leaveRequest, effectivePersonId);
                var response = Request.CreateResponse<LeaveRequest>(HttpStatusCode.Created, newLeaveRequest);
                SetResourceLocationHeader(getLeaveRequestRouteId, new { id = newLeaveRequest.Id });
                _logger.Debug("************End - Process to create Leave Request -- End ************");
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format(pe.Message);
                _logger.Error(pe, message);
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (ExistingResourceException ere)
            {
                _logger.Error(ere, ere.Message);
                SetResourceLocationHeader(getLeaveRequestRouteId, new { id = ere.ExistingResourceId });
                var exception = new WebApiException();
                exception.Message = ere.Message;
                exception.AddConflict(ere.ExistingResourceId);
                throw CreateHttpResponseException(exception, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create a Leave Request Status record.
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can create a leave request status record for their own leave request.
        /// 2) Leave approvers(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxies can create a leave request status record for the leave requests of their supervisees. 
        /// </accessComments>
        /// <param name="status">Leave Request Status DTO</param>
        /// <param name="effectivePersonId">Optional parameter - Current user or proxy user person id.</param>
        /// <returns>Newly created Leave Request Status</returns>
        [HttpPost]
        public async Task<LeaveRequestStatus> CreateLeaveRequestStatusAsync([FromBody] LeaveRequestStatus status, [FromUri] string effectivePersonId = null)
        {
            if (status == null)
            {
                throw CreateHttpResponseException(noLeaveRequestObjectErrorMessage, HttpStatusCode.BadRequest);
            }

            try
            {
                _logger.Debug("************Start - Process to create Leave Request Status -- Start ************");
                var response = await _employeeLeaveRequestService.CreateLeaveRequestStatusAsync(status, effectivePersonId);
                _logger.Debug("************End - Process to create Leave Request Status -- End ************");
                return response;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Gets the HumanResourceDemographics information of supervisors for the given position of a supervisee.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can access the HumanResourceDemographics information of their own supervisors.
        /// </accessComments>
        /// <param name="id">Position Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id.</param>
        /// <returns>List of HumanResourceDemographics DTOs</returns>
        [HttpPost]
        public async Task<IEnumerable<HumanResourceDemographics>> GetSupervisorsByPositionIdAsync([FromBody] string id, [FromUri] string effectivePersonId = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.Error(noPositionIdErrorMessage);
                throw CreateHttpResponseException(noPositionIdErrorMessage, HttpStatusCode.BadRequest);
            }
            try
            {
                _logger.Debug("************Start - Process to fetch Supervisors by position Id-- Start ************");
                var supervisors = await _employeeLeaveRequestService.GetSupervisorsByPositionIdAsync(id, effectivePersonId);
                _logger.Debug("************End - Process to fetch Supervisors by position Id-- End ************");
                return supervisors;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(positionSupervisorsUnexpectedErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This end point returns all the supervisees for the currently authenticated leave approver.       
        /// The endpoint will not return the supervisees if:
        ///     1.  403 - User does not have permission to get supervisee information
        /// </summary>
        /// <accessComments>
        ///  Current user must be Leave Approver(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxy to fetch all of their supervisees
        /// </accessComments>
        /// <param name="effectivePersonId">
        ///  Optional parameter for passing effective person Id
        /// </param>
        /// <returns><see cref="HumanResourceDemographics">List of HumanResourceDemographics DTOs</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to fetch supervisees.</exception>
        [HttpGet]
        public async Task<IEnumerable<HumanResourceDemographics>> GetSuperviseesByPrimaryPositionForSupervisorAsync([FromUri] string effectivePersonId = null)
        {
            try
            {
                _logger.Debug("************Start - Process to fetch supervisee primary position -- Start ************");
                var supervisees = await _employeeLeaveRequestService.GetSuperviseesByPrimaryPositionForSupervisorAsync(effectivePersonId);
                _logger.Debug("************End - Process to fetch supervisee primary position -- End ************");
                return supervisees;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(supervisorsUnexpectedErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This endpoint will update an existing Leave Request along with its Leave Request Details. 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update their own leave request.    
        /// Supervisor can update leave requests created by their supervisees 
        /// The endpoint will reject the update of a Leave Request if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="leaveRequest"><see cref="LeaveRequest">Leave Request DTO</see></param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns><see cref="LeaveRequest">Newly updated Leave Request object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the leaveRequest DTO is not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to update the leave request.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if the leave request record to be edited doesn't exist in the DB.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if the leave request record to be edited is locked or if a duplicate leave request record already exists in the DB.</exception>
        [HttpPut]
        public async Task<LeaveRequest> UpdateLeaveRequestAsync([FromBody] LeaveRequest leaveRequest, [FromUri] string effectivePersonId = null)
        {
            try
            {
                if (leaveRequest == null)
                {
                    throw CreateHttpResponseException(noLeaveRequestObjectErrorMessage);
                }
                _logger.Debug("************Start - Process to update leave request -- Start ************");
                var updatedLeaveRequest = await _employeeLeaveRequestService.UpdateLeaveRequestAsync(leaveRequest, effectivePersonId);
                _logger.Debug("************End - Process to update leave request -- End ************");
                return updatedLeaveRequest;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = string.Format(pe.Message);
                _logger.Error(pe, message);
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException cnfe)
            {
                _logger.Error(cnfe, cnfe.Message);
                throw CreateNotFoundException("LeaveRequest", leaveRequest.Id);
            }
            catch (RecordLockException rle)
            {
                _logger.Error(rle, rle.Message);
                throw CreateHttpResponseException(recordLockErrorMessage, HttpStatusCode.Conflict);
            }
            catch (ExistingResourceException ere)
            {
                _logger.Error(ere, ere.Message);
                SetResourceLocationHeader(getLeaveRequestRouteId, new { id = ere.ExistingResourceId });
                var exception = new WebApiException();
                exception.Message = existingResourceErrorMessage;
                exception.AddConflict("ExistingResource");
                throw CreateHttpResponseException(exception, HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This endpoint will create a new leave request comment associated with a leave request. 
        /// </summary>
        /// <accessComments>
        /// 1) Any authenticated user can create a comment associated with their own leave request.     
        /// 2) Leave approvers(users with the permission APPROVE.REJECT.LEAVE.REQUEST) or their proxies can create a comment for the leave requests of their supervisees.  
        /// </accessComments>     
        /// <param name="leaveRequestComment">Leave Request Comment DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns><see cref="LeaveRequestComment">Leave Request Comment DTO</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to create the leave request comment.</exception>
        [HttpPost]
        public async Task<LeaveRequestComment> CreateLeaveRequestCommentsAsync([FromBody] LeaveRequestComment leaveRequestComment, [FromUri] string effectivePersonId = null)
        {
            if (leaveRequestComment == null)
            {
                throw CreateHttpResponseException(noLeaveRequestCommentObjectErrorMessage);
            }
            try
            {
                _logger.Debug("************Start - Process to create leave request comment -- Start ************");
                var leaverequestcomment = await _employeeLeaveRequestService.CreateLeaveRequestCommentsAsync(leaveRequestComment, effectivePersonId);
                _logger.Debug("************End - Process to create leave request comment -- End ************");
                return leaverequestcomment;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (PermissionsException pe)
            {
                var message = pe.Message;
                _logger.Error(pe, message);
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }


        }

    }
}