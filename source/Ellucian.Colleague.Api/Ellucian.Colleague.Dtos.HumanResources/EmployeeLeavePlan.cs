/*Copyright 2017-2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Attributes;
using System.Collections.ObjectModel;
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
        [JsonProperty("id")]
        [Metadata("PERLEAVE.ID", DataDescription = "he database Id of the Employee Leave Plan.")]
        public string Id { get; set; }

        /// <summary>
        /// The Colleague PERSON id of the Employee
        /// </summary>
        [JsonProperty("employeeId")]
        [Metadata("PERLV.HRP.ID", DataDescription = "The Colleague PERSON id of the Employee.")]
        public string EmployeeId { get; set; }

        /// <summary>
        /// The start date of this employee leave plan
        /// </summary>
        [JsonProperty("startDate")]
        [Metadata("PERLV.START.DATE", DataDescription = "The start date of this employee leave plan.")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of this employee leave plan. If null, leave plan has no end date.
        /// </summary>
        [JsonProperty("endDate")]
        [Metadata("PERLV.END.DATE", DataDescription = "The end date of this employee leave plan.")]
        public DateTime? EndDate { get; set; }

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
        /// The start date of the LeavePlan definition
        /// </summary>
        [JsonProperty("leavPlanStartDate")]
        [Metadata("LPN.START.DATE", DataDescription = "The start date of the LeavePlan definition.")]
        public DateTime LeavePlanStartDate { get; set; }

        /// <summary>
        /// The end date of the LeavePlan definition. If null, leave plan definition has no end date
        /// </summary>
        [JsonProperty("leavePlanEndDate")]
        [Metadata("LPN.END.DATE", DataDescription = "The end date of the LeavePlan definition.")]
        public DateTime? LeavePlanEndDate { get; set; }

        /// <summary>
        /// The leave type associated with the LeavePlan definition
        /// </summary>
        [JsonProperty("leavePlanType")]
        [Metadata("LPN.TYPE", DataDescription = "The leave type associated with the LeavePlan definition.")]
        public string LeavePlanType { get; set; }

        /// <summary>
        /// The start date the employee may begin entering leave for this plan
        /// </summary>
        [JsonProperty("leaveAllowedDate")]
        [Metadata("PERLV.ALLOWED.DATE", DataDescription = "The start date the employee may begin entering leave for this plan.")]
        public DateTime LeaveAllowedDate { get; set; }

        /// <summary>
        /// Indicates if this leave plan allows a negative balance
        /// </summary>
        [JsonProperty("allowNegativeBalance")]
        [Metadata("LPN.ALLOW.NEGATIVE", DataDescription = "Indicates if this leave plan allows a negative balance.")]
        public bool AllowNegativeBalance { get; set; }

        /// <summary>
        /// Indicates if this is a leave reporting plan
        /// </summary>
        [JsonProperty("isLeaveReportingPlan")]
        [Metadata("PERLV.LEAVE.REPORTING", DataDescription = "Indicates if this is a leave reporting plan.")]
        public bool IsLeaveReportingPlan { get; set; }

        /// <summary>
        /// The category of the leave plan based on the leave plan definition's leave type. 
        /// </summary>
        [JsonProperty("leavePlanTypeCategory")]
        [JsonConverter(typeof(StringEnumConverter))]
        [Metadata(DataDescription = "The category of the leave plan based on the leave plan definition's leave type.")]
        public LeaveTypeCategory LeavePlanTypeCategory { get; set; }

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

        /// <summary>
        /// The leave balance as of the last pay period, represented as hours
        /// </summary>
        [JsonProperty("priorPeriodLeaveBalance")]
        [Metadata("PERLV.BALANCE", DataDescription = "The leave balance as of the last pay period, represented as hours.")]
        public decimal PriorPeriodLeaveBalance { get; set; }

        /// <summary>
        /// The starting balance for this leave plan of the current plan year (as of today).
        /// </summary>
        [JsonProperty("currentPlanYearStartingBalance")]
        [Metadata(DataDescription = "The starting balance for this leave plan of the current plan year.")]
        public decimal CurrentPlanYearStartingBalance { get; set; }

        /// <summary>
        /// The number of earned hours for this leave plan in the current plan year (as of today)
        /// </summary>
        [JsonProperty("currentPlanYearEarnedHours")]
        [Metadata(DataDescription = "The number of earned hours for this leave plan in the current plan year.")]
        public decimal CurrentPlanYearEarnedHours { get; set; }

        /// <summary>
        /// The number of used hours for this leave plan in the current plan year (as of today)
        /// </summary>
        [JsonProperty("currentPlanYearUsedHours")]
        [Metadata(DataDescription = "The number of used hours for this leave plan in the current plan year.")]
        public decimal CurrentPlanYearUsedHours { get; set; }

        /// <summary>
        /// The number of adjusted hours for this leave plan in the current plan year (as of today)
        /// </summary>
        [JsonProperty("currentPlanYearAdjustedHours")]
        [Metadata(DataDescription = "The number of adjusted hours for this leave plan in the current plan year.")]
        public decimal CurrentPlanYearAdjustedHours { get; set; }

        /// <summary>
        /// The balance (Earned - Used + Adjusted) of this leave plan in the current plan year (as of today)
        /// </summary>
        [JsonProperty("currentPlanYearBalance")]
        [Metadata(DataDescription = "The balance of leave plan in the current plan year.")]
        public decimal CurrentPlanYearBalance { get; set; }

        /// <summary>
        /// The Leave Plans start date in the current year. Only the Month and Day should be used from this date.
        /// </summary>
        [JsonProperty("currentPlanYearStartDate")]
        [Metadata("PERLV.LATEST.START.BAL.UPDT", DataDescription = "The Leave Plans start date in the current year.")]
        public DateTime CurrentPlanYearStartDate { get; set; }

        /// <summary>
        /// The Leave Plans end date.
        /// </summary>
        [JsonProperty("currentPlanYearEndDate")]
        [Metadata(DataDescription = "The Leave Plans end date in the current year.")]
        public DateTime CurrentPlanYearEndDate { get; set; }

        /// <summary>
        /// Accrual Rate
        /// </summary>
        [JsonProperty("accrualRate")]
        [Metadata("PLA.ACCRUAL.HOURS", DataDescription = "Accural rate for Employee Leave Plan.")]
        public Decimal? AccrualRate { get; set; }

        /// <summary>
        /// Accrual Limit
        /// </summary>
        [JsonProperty("accrualLimit")]
        [Metadata("PLA.ACCRUAL.LIMIT", DataDescription = "Accural limit for Employee Leave Plan.")]
        public Decimal? AccrualLimit { get; set; }

        /// <summary>
        /// Accrual Maximum Carry Over
        /// </summary>
        [JsonProperty("accrualMaxCarryOver")]
        [Metadata("PLA.CARRYOVER.HOURS", DataDescription = "Accural max carryover for Employee Leave Plan.")]
        public Decimal? AccrualMaxCarryOver { get; set; }

        /// <summary>
        /// Accrual Maximum Roll Over
        /// </summary>
        [JsonProperty("accrualMaxRollOver")]
        [Metadata("PLA.ROLLOVER.MAXIMUM", DataDescription = "Accural max rollover for Employee Leave Plan.")]
        public decimal? AccrualMaxRollOver { get; set; }

        /// <summary>
        ///  Accrual Method
        /// </summary>
        [JsonProperty("accrualMethod")]
        [Metadata("LPN.ACCRUAL.METHOD", DataDescription = "Accural method for Employee Leave Plan.")]
        public string AccrualMethod { get; set; }

        /// <summary>
        /// List of all Earning Type IDs associated with a Employee Leave Plan
        /// </summary>
        [JsonProperty("earningTypeIDList")]
        [Metadata(DataDescription = "Earning type Id List for Employee Leave Plan.")]
        public IEnumerable<string> EarningTypeIDList { get; set; }

        /// <summary>
        /// Indicates whether or not this leave plan has a plan year start date defined in LEAD form. 
        /// </summary>
        [JsonProperty("isPlanYearStartDateDefined")]
        [Metadata(DataDescription = "Is plan year start date defined for Employee Leave Plan.")]
        public bool IsPlanYearStartDateDefined { get; set; }

        /// <summary>
        /// List of all Leave Transactions associated with a Employee Leave Plan
        /// </summary>
        [JsonProperty("employeeLeaveTransactions")]
        [Metadata(DataDescription = "Employee leave transaction details list for leave plan.")]
        public IEnumerable<EmployeeLeaveTransaction> EmployeeLeaveTransactions { get; set; }

        /// <summary>
        /// The Current Balance from the B record of the Current Plan Year, if it exists.
        /// </summary>
        [JsonProperty("retroAvailableBalance")]
        [Metadata(DataDescription = "Retro Available balance for Employee Leave Plan.")]
        public decimal RetroAvailableBalance { get; set; }

        /// <summary>
        /// PriorYearEndTrnsaction's forwarding balance hours.
        /// </summary>
        [JsonProperty("priorYearForwardingBalanceHours")]
        [Metadata(DataDescription = "Prior year forwarding balance for Employee Leave Plan.")]
        public decimal? PriorYearForwardingBalanceHours { get; set; }
    }
}
