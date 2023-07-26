/* Copyright 2019-2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO for employee leave request
    /// </summary>
    [DataContract]
    public class LeaveRequest
    {
        /// <summary>
        /// Identifier of this leave request
        /// </summary>
        [JsonProperty("id")]
        [Metadata("LEAVE.REQUEST.ID", DataDescription = "Leave Request Id.")]
        public string Id { get; set; }

        /// <summary>
        /// PerLeaveId of this leave request
        /// </summary>
        [JsonProperty("perLeaveId")]
        [Metadata("LR.PERLEAVE.ID", DataDescription = "Person Leave Id.")]
        public string PerLeaveId { get; set; }

        /// <summary>
        /// Identifier of the employee who raised this leave request
        /// </summary>
        [JsonProperty("employeeId")]
        [Metadata("LR.EMPLOYEE.ID", DataDescription = "Employee Id.")]
        public string EmployeeId { get; set; }

        /// <summary>
        /// Name of the employee who raised this leave request
        /// </summary>
        [JsonProperty("employeeName")]
        [Metadata(DataDescription = "Employee Name.")]
        public string EmployeeName { get; set; }

        /// <summary>
        /// Start date for this leave request
        /// </summary>
        [JsonProperty("startDate")]
        [Metadata("LR.START.DATE", DataDescription = "Leave Start Date.")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for this leave request
        /// </summary>
        [JsonProperty("endDate")]
        [Metadata("LR.END.DATE", DataDescription = "Leave End Date.")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Identifier of the actioner for this leave request
        /// </summary>
        [JsonProperty("approverId")]
        [Metadata("LR.APPROVER.ID", DataDescription = "Leave Approver Id.")]
        public string ApproverId { get; set; }

        /// <summary>
        /// Name of the actioner for this leave request
        /// </summary>
        [JsonProperty("approverName")]
        [Metadata("LR.APPROVER.NAME", DataDescription = "Leave Approver Name.")]
        public string ApproverName { get; set; }

        /// <summary>
        /// Flag which indicates whether this LR is Withdrawn or not.
        /// </summary>
        [JsonProperty("isWithdrawn")]
        [Metadata("LR.IS.WITHDRAWN", DataDescription = "Whether Leave Request withdrawn or not.")]
        public bool IsWithdrawn { get; set; }

        /// <summary>
        /// The value of HRSS.LR.UNSUBMIT.WDRW option in LVSS form when the Leave Request is Withdrawn.
        /// </summary>
        [JsonProperty("withdrawOption")]
        [Metadata("LR.WITHDRAW.OPTION", DataDescription = "The value of HRSS.LR.UNSUBMIT.WDRW option in LVSS form when the Leave Request is Withdrawn.")]
        public string WithdrawOption { get; set; }

        /// <summary>
        /// Current status of this leave request
        /// </summary>
        [JsonProperty("status")]
        [Metadata("LRS.ACTION.TYPE", DataDescription = "Current status of this leave request.")]
        public LeaveStatusAction Status { get; set; }

        /// <summary>
        /// List of all leave request detail object associated wih this leave request
        /// </summary>
        [JsonProperty("leaveRequestDetails")]
        [Metadata(DataDescription = " List of all leave request detail object associated wih this leave request.")]
        public List<LeaveRequestDetail> LeaveRequestDetails { get; set; }

        /// <summary>
        /// List of all comments object associated wih this leave request
        /// </summary>

        [JsonProperty("leaveRequestComments")]
        [Metadata(DataDescription = "List of all comments object associated wih this leave request.")]
        public List<LeaveRequestComment> LeaveRequestComments { get; set; }

        /// <summary>
        /// List of all dates with total leave requested hours for a day exceeding 24 hours 
        /// </summary>
        
        public List<LeaveRequestDetail> LeaveRequestDailyHoursErrorDetails { get; set; }

        /// <summary>
        /// True when leave request is created by same supervisor user with submitted status
        /// </summary>
        
        [JsonProperty("enableDeleteForSupervisor")]
        [Metadata(DataDescription = "True when leave request is created by same supervisor user with submitted status.")]
        public bool EnableDeleteForSupervisor { get; set; }

        /// <summary>
        /// True when a withdraw request has pending supervisor approval
        /// </summary>
        
        [JsonProperty("isWithdrawPendingApproval")]
        [Metadata(DataDescription = "True when a withdraw request has pending supervisor approval.")]
        public bool IsWithdrawPendingApproval { get; set; }

        /// <summary>
        /// The database id of the LeavePlan definition
        /// </summary>
        [JsonProperty("leavePlanId")]
        [Metadata("LEAVPLAN.ID", DataDescription = "The id of the LeavePlan definition.")]
        public string LeavePlanId { get; set; }

        /// <summary>
        /// The description of the LeavePlan Definition
        /// </summary>
        [JsonProperty("leavePlanDescription")]
        [Metadata("LPN.DESC", DataDescription = "The description of the LeavePlan Definition.")]
        public string LeavePlanDescription { get; set; }

        /// <summary>
        /// The database Id of the EarningsType associated to this Leave Plan. When the employee takes this type of leave,
        /// they track their leave hours to this earnings type
        /// </summary>
        [JsonProperty("earningsTypeId")]
        [Metadata("EARNTYPE.ID", DataDescription = "Earning type for employee leave plan.")]
        public string EarningsTypeId { get; set; }

        /// <summary>
        /// The description of the EarningsType identified by the EarningsTypeId. Also see the /earnings-types endpoint for more
        /// details about Earnings Types.
        /// </summary>
        [JsonProperty("earningsTypeDescription")]
        [Metadata("ETP.DESC", DataDescription = "The description of the EarningsType identified by the EarningsTypeId.")]
        public string EarningsTypeDescription { get; set; }
    }
}
