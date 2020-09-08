﻿/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    /// <summary>
    /// Interface for the employee leave request repository.
    /// </summary>
    public interface IEmployeeLeaveRequestRepository
    {
        /// <summary>
        /// Gets all leave requests for the input employee Ids
        /// </summary>
        /// <param name="effectivePersonIds">List of employee Ids</param>
        /// <returns>List of LeaveRequest entities</returns>
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsAsync(IEnumerable<string> effectivePersonIds);

        /// <summary>
        /// Gets a single LeaveRequest object matching the given id. 
        /// </summary>
        /// <param name="id">Leave Request Id</param>     
        /// <returns>LeaveRequest Entity</returns>
        Task<LeaveRequest> GetLeaveRequestInfoByLeaveRequestIdAsync(string leaveRequestId);

        /// <summary>
        /// Creates a new leave request record
        /// </summary>
        /// <param name="leaveRequest">Leave Request Entity</param>
        /// <returns>Newly created leave request</returns>
        Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest);

        /// <summary>
        /// Creates a new leave request status record
        /// </summary>
        /// <param name="status">Leave Request Status Entity</param>
        /// <returns>Newly created leave request status</returns>
        Task<LeaveRequestStatus> CreateLeaveRequestStatusAsync(LeaveRequestStatus status);

        /// <summary>
        /// Updates an existing leave request record
        /// </summary>
        /// <param name="leaveRequest">Leave Request Entity</param>
        /// <returns>Newly updated leave request</returns>
        Task<LeaveRequest> UpdateLeaveRequestAsync(LeaveRequestHelper leaveRequestHelper);

        /// <summary>
        /// Creates a new leave request comment associated with a leave request.
        /// </summary>
        /// <param name="leaveRequestComment">Leave Request Comment Entity</param>
        /// <returns>Leave Request Comment Domain Entity that contains newly created comment's information </returns>
        Task<LeaveRequestComment> CreateLeaveRequestCommentsAsync(LeaveRequestComment leaveRequestComment);
      
    }
}
