/* Copyright 2019-20 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO for employee leave request
    /// </summary>
    public class LeaveRequest
    {
        /// <summary>
        /// Identifier of this leave request
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// PerLeaveId of this leave request
        /// </summary>
        public string PerLeaveId { get; set; }

        /// <summary>
        /// Identifier of the employee who raised this leave request
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Name of the employee who raised this leave request
        /// </summary>
        public string EmployeeName { get; set; }

        /// <summary>
        /// Start date for this leave request
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for this leave request
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Identifier of the approver for this leave request
        /// </summary>
        public string ApproverId { get; set; }

        /// <summary>
        /// Name of the approver for this leave request
        /// </summary>
        public string ApproverName { get; set; }

        /// <summary>
        /// Current status of this leave request
        /// </summary>
        public LeaveStatusAction Status { get; set; }

        /// <summary>
        /// List of all leave request detail object associated wih this leave request
        /// </summary>
        public List<LeaveRequestDetail> LeaveRequestDetails { get; set; }

        /// <summary>
        /// List of all comments object associated wih this leave request
        /// </summary>
        public List<LeaveRequestComment> LeaveRequestComments { get; set; }

        /// <summary>
        /// List of all dates with total leave requested hours for a day exceeding 24 hours 
        /// </summary>
        public List<LeaveRequestDetail> LeaveRequestDailyHoursErrorDetails { get; set; }
    }
}
