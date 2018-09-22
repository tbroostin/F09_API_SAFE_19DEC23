/*Copyright 2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<string> ExcludedLeavePlanIds;

    }
}
