/*Copyright 2018-2023 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// The leave balance configuration contains the settings necessary to determine whether a leave plan's details
    /// should be displayed in SS leave balance page.
    /// </summary>
    [DataContract]
    public class LeaveBalanceConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LeaveBalanceConfiguration()
        {
            ExcludedLeavePlanIds = new List<string>();
        }

        /// <summary>
        /// List of all excluded leave plans.
        /// </summary>
        [JsonProperty("excludeLeavePlanIds")]
        [Metadata("HRSS.EXCLUDE.LEAVE.PLAN.IDS", DataDescription = "The IDs of the leave plans excluded to be displayed in Self-Service Application Users")]
        public List<string> ExcludedLeavePlanIds { get; set; }
        /// <summary>
        /// Number of days to lookback for leave
        /// </summary>
        [JsonProperty("leaveRequestLookbackDays")]
        [Metadata("HRSS.LEAVE.LKBK", DataDescription = "The look back days count for which a leave request should be displayed in Self service")]
        public int? LeaveRequestLookbackDays { get; set; }

        /// <summary>
        /// Unsubmit/Withdraw type for Leave Request
        /// </summary>
        [JsonProperty("leaveRequestActionType")]
        [Metadata("HRSS.LR.UNSUBMIT.WDRW", DataDescription = "Leave Request action type is set to R (Reject) if not set or  U (UnSubmit before Approval) or W (Withdraw After Approval not required approval) or A (Withdraw After Approval required approval).")]
        public LeaveRequestActionType LeaveRequestActionType { get; set; }

        /// <summary>
        /// Allow the supervisor to create/edit leave requests
        /// </summary>
        [JsonProperty("allowSupervisorToEditLeaveRequests")]
        [Metadata("HRSS.LR.ALLOW.SUPRVSR.EDIT", DataDescription = "If Supervisor allowed to edit the leave request is set to true, else false.")]
        public bool AllowSupervisorToEditLeaveRequests { get; set; }
    }
}
