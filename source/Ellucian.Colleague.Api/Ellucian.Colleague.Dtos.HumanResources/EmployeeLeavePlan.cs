/*Copyright 2017-2021 Ellucian Company L.P. and its affiliates.*/
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
    /// The EmployeeLeavePlan is a leave plan assigned to a specific employee that's active for the date range.
    /// </summary>
    public class EmployeeLeavePlan
    {
        /// <summary>
        /// The database Id of the Employee Leave Plan
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Colleague PERSON id of the Employee
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// The start date of this employee leave plan
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of this employee leave plan. If null, leave plan has no end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The database id of the LeavePlan definition
        /// </summary>
        public string LeavePlanId { get; set; }

        /// <summary>
        /// The description of the LeavePlan Definition
        /// </summary>
        public string LeavePlanDescription { get; set; }

        /// <summary>
        /// The start date of the LeavePlan definition
        /// </summary>
        public DateTime LeavePlanStartDate { get; set; }

        /// <summary>
        /// The end date of the LeavePlan definition. If null, leave plan definition has no end date
        /// </summary>
        public DateTime? LeavePlanEndDate { get; set; }

        /// <summary>
        /// The start date the employee may begin entering leave for this plan
        /// </summary>
        public DateTime LeaveAllowedDate { get; set; }

        /// <summary>
        /// Indicates if this leave plan allows a negative balance
        /// </summary>
        public bool AllowNegativeBalance { get; set; }

        /// <summary>
        /// Indicates if this is a leave reporting plan
        /// </summary>
        public bool IsLeaveReportingPlan { get; set; }

        /// <summary>
        /// The category of the leave plan based on the leave plan definition's leave type. 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LeaveTypeCategory LeavePlanTypeCategory { get; set; }

        /// <summary>
        /// The database Id of the EarningsType associated to this Leave Plan. When the employee takes this type of leave,
        /// they track their leave hours to this earnings type
        /// </summary>
        public string EarningsTypeId { get; set; }

        /// <summary>
        /// The description of the EarningsType identified by the EarningsTypeId. Also see the /earnings-types endpoint for more
        /// details about Earnings Types.
        /// </summary>
        public string EarningsTypeDescription { get; set; }

        /// <summary>
        /// The leave balance as of the last pay period, represented as hours
        /// </summary>
        public decimal PriorPeriodLeaveBalance { get; set; }

        /// <summary>
        /// The starting balance for this leave plan of the current plan year (as of today).
        /// </summary>
        public decimal CurrentPlanYearStartingBalance { get; set; }

        /// <summary>
        /// The number of earned hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public decimal CurrentPlanYearEarnedHours { get; set; }

        /// <summary>
        /// The number of used hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public decimal CurrentPlanYearUsedHours { get; set; }

        /// <summary>
        /// The number of adjusted hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public decimal CurrentPlanYearAdjustedHours { get; set; }

        /// <summary>
        /// The balance (Earned - Used + Adjusted) of this leave plan in the current plan year (as of today)
        /// </summary>
        public decimal CurrentPlanYearBalance { get; set; }

        /// <summary>
        /// The Leave Plans start date in the current year. Only the Month and Day should be used from this date.
        /// </summary>
        public DateTime CurrentPlanYearStartDate { get; set; }
        
        /// <summary>
        /// The Leave Plans end date.
        /// </summary>
        public DateTime CurrentPlanYearEndDate { get; set; }

        /// <summary>
        /// Accrual Rate
        /// </summary>
        public Decimal? AccrualRate { get; set; }

        /// <summary>
        /// Accrual Limit
        /// </summary>
        public Decimal? AccrualLimit { get; set; }

        /// <summary>
        /// Accrual Maximum Carry Over
        /// </summary>
        public Decimal? AccrualMaxCarryOver { get; set; }

        /// <summary>
        ///  Accrual Method
        /// </summary>
        public string AccrualMethod { get; set; }

        /// <summary>
        /// List of all Earning Type IDs associated with a Employee Leave Plan
        /// </summary>
        public IEnumerable<string> EarningTypeIDList { get; set; }

        /// <summary>
        /// Indicates whether or not this leave plan has a plan year start date defined in LEAD form. 
        /// </summary>
        public bool IsPlanYearStartDateDefined { get; set; }
    }
}
