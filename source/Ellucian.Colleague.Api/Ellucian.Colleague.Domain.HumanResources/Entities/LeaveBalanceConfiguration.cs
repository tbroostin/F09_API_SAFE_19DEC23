/*Copyright 2018-2022 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// The leave balance configuration contains the settings necessary to determine whether a leave plan's details
    /// should be displayed in SS leave balance page.
    /// </summary>
    [Serializable]
    public class LeaveBalanceConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LeaveBalanceConfiguration() {
            ExcludedLeavePlanIds = new List<string>();
        }

        /// <summary>
        /// List of all excluded leave plans.
        /// </summary>
        public List<string> ExcludedLeavePlanIds { get; set; }

        /// <summary>
        /// Number of days to lookback for leave
        /// </summary>
        public int? LeaveRequestLookbackDays { get; set; }

        /// <summary>
        /// Unsubmit/Withdraw type for Leave Request
        /// </summary>
        public LeaveRequestActionType LeaveRequestActionType { get; set; }

        /// <summary>
        /// Allow the supervisor to create/edit leave requests
        /// </summary>
        public bool AllowSupervisorToEditLeaveRequests { get; set; }
    }
}
