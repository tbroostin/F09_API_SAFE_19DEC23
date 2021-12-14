/*Copyright 2018-2021 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// The leave balance configuration contains the settings necessary to determine whether a leave plan's details
    /// should be displayed in SS leave balance page.
    /// </summary>
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
        public List<string> ExcludedLeavePlanIds { get; set; }
        /// <summary>
        /// Number of days to lookback for leave
        /// </summary>
        public int? LeaveRequestLookbackDays { get; set; }
    }
}
