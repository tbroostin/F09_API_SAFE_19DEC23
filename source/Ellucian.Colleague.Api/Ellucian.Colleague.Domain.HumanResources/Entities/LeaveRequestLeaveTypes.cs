/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain entity for employee leave request leave types
    /// </summary>
    [Serializable]
    public class LeaveRequestLeaveTypes
    {
        /// <summary>
        ///  Leave Plan Id
        /// </summary>
        public string PerLeaveId { get; set; }

        /// <summary>
        ///  Leave Type Description
        /// </summary>
        public string LeaveDescription { get; set; }

        /// <summary>
        /// Leave Balance for the current Year
        /// </summary>
        public decimal BalanceLeaveHours { get; set; }

        public decimal LeaveTypeUsedHours { get; set; }

        /// <summary>
        /// Leave Allowed date for the Employee
        /// </summary>
        public DateTime LeaveAllowedDate { get; set; }

        /// <summary>
        /// Per Leave Start date of the leave type
        /// </summary>
        public DateTime PerLeaveStartDate { get; set; }

        /// <summary>
        /// Per Leave End date of the leave type
        /// </summary>
        public DateTime? PerLeaveEndDate { get; set; }

        /// <summary>
        /// Leave Accrual Method
        /// </summary>
        public string LeaveAccrualMethod { get; set; }

        /// <summary>
        /// Current Plan Year Start Date
        /// </summary>
        public DateTime CurrentPlanYearStartDate { get; set; }

        /// <summary>
        /// Current Plan Year End Date
        /// </summary>
        public DateTime CurrentPlanYearEndDate { get; set; }


        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="perLeaveId"></param>
        /// <param name="leaveDescription"></param>
        /// <param name="balanceLeaveHours"></param>
        /// <param name="leaveAllowedDate"></param>
        /// <param name="perLeaveStartDate"></param>
        /// <param name="perLeaveEndDate"></param>
        public LeaveRequestLeaveTypes(string perLeaveId,
            string leaveDescription,
            decimal balanceLeaveHours,
            DateTime leaveAllowedDate,
            DateTime perLeaveStartDate,
            DateTime? perLeaveEndDate,
            string leaveAccrualMethod,
            DateTime currentPlanYearStartDate,
            DateTime currentPlanYearEndDate)
        {
            PerLeaveId = perLeaveId;
            LeaveDescription = leaveDescription;
            BalanceLeaveHours = balanceLeaveHours;
            LeaveAllowedDate = leaveAllowedDate;
            PerLeaveStartDate = perLeaveStartDate;
            PerLeaveEndDate = perLeaveEndDate;
            LeaveAccrualMethod = leaveAccrualMethod;
            CurrentPlanYearStartDate = currentPlanYearStartDate;
            CurrentPlanYearEndDate = currentPlanYearEndDate;
        }
    }
}
