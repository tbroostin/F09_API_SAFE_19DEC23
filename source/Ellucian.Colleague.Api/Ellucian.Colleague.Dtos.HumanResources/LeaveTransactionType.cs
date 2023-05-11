/*Copyright 2021-2022 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Categories that can be assigned to types of leave. Sick, Hourly and Sick, Salary may be separate leave types, but they
    /// both have the same category Sick
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LeaveTransactionType
    {
        /// <summary>
        /// Earned
        /// </summary>
        Earned,
        /// <summary>
        /// Used
        /// </summary>
        Used,
        /// <summary>
        /// Adjusted
        /// </summary>
        Adjusted,
        /// <summary>
        /// LeaveReporting
        /// </summary>
        LeaveReporting,
        /// <summary>
        /// StartingBalanceAdjustment
        /// </summary>
        StartingBalanceAdjustment,
        /// <summary>
        /// StartingBalance
        /// </summary>
        StartingBalance,
        /// <summary>
        /// MidYearBalanceAdjustment
        /// </summary>
        MidYearBalanceAdjustment,
        /// <summary>
        /// Rollover
        /// </summary>
        Rollover
    }
}
