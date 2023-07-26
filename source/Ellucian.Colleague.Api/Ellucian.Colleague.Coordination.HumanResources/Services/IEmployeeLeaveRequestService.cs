//Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.HumanResources;

/// <summary>
/// Interface for the employee leave request coordination service. 
/// </summary>
namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IEmployeeLeaveRequestService
    {
        /// <summary>
        /// Gets all leave requests for current user
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of LeaveRequest DTOs</returns>
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsAsync(string effectivePersonId = null);

        /// <summary>
        /// Service method that gets a single LeaveRequest object matching the given id based on the user's permissions. 
        /// </summary>
        /// <param name="id">Leave Request Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>LeaveRequest DTO</returns>
        Task<LeaveRequest> GetLeaveRequestInfoByLeaveRequestIdAsync(string id, string effectivePersonId = null);

        /// <summary>
        /// Creates a single Leave Request along with its details.
        /// </summary>
        /// <param name="leaveRequest">Leave Request DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly created Leave Request Object</returns>
        Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest, string effectivePersonId = null);

        /// <summary>
        /// Gets the HumanResourceDemographics information of supervisors for the given position of a supervisee.</summary>
        /// <param name="positionId">Position Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id.</param>
        /// <returns>List of HumanResourceDemographics DTOs</returns>
        Task<IEnumerable<HumanResourceDemographics>> GetSupervisorsByPositionIdAsync(string positionId, string effectivePersonId = null);

        /// <summary>
        /// Creates a single Leave LeaveRequestStatus.
        /// </summary>
        /// <param name="status">Leave Request Status DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly created Leave Request Status Object</returns>
        Task<LeaveRequestStatus> CreateLeaveRequestStatusAsync(LeaveRequestStatus status, string effectivePersonId = null);

        /// <summary>
        /// Updates an existing Leave Request record.
        /// </summary>
        /// <param name="leaveRequest">Leave Request to be updated</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly updated LeaveRequest DTO</returns>
        Task<LeaveRequest> UpdateLeaveRequestAsync(LeaveRequest leaveRequest, string effectivePersonId = null);

        /// <summary>
        /// Creates a new leave request comment associated with a leave request.
        /// </summary>
        /// <param name="leaveRequestComment">Leave Request Comment DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Leave Request Comment DTO</returns
        Task<Dtos.HumanResources.LeaveRequestComment> CreateLeaveRequestCommentsAsync(Dtos.HumanResources.LeaveRequestComment leaveRequestComment, string effectivePersonId = null);

        /// <summary>
        /// Retreives list of Supervisees for a Leave Approver/Supervisor
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of HumanDemographics DTOs containing supervisee information</returns>
        Task<IEnumerable<Dtos.HumanResources.HumanResourceDemographics>> GetSuperviseesByPrimaryPositionForSupervisorAsync(string effectivePersonId = null);

        /// <summary>
        /// Gets the Approved Leave Requests for a timecard week based on the date range. 
        /// </summary>
        /// <param name="startDate">Start date of timecard week </param>
        /// <param name="endDate">End date of timecard week</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of Leave Request DTO</returns>
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsForTimeEntryAsync(DateTime startDate, DateTime endDate, string effectivePersonId = null);

        /// <summary>
        /// Gets the Leavetypes for Leave request Information
        /// </summary>
        /// <param name="effectivePersonId"></param>
        /// <returns></returns>
        Task<IEnumerable<LeaveRequestLeaveTypes>> GetLeaveTypesForLeaveRequestAsync(string effectivePersonId = null);

    }
}
