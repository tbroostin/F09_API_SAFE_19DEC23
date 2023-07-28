/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
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
    public class LeaveRequestLeaveTypes
    {
        /// <summary>
        ///  Leave Plan Id
        /// </summary>

        [JsonProperty("perLeaveId")]
        [Metadata("LR.PERLEAVE.ID", DataDescription = "Person Leave Id.")]
        public string PerLeaveId { get; set; }

        /// <summary>
        ///  Leave Type Description
        /// </summary>
        [JsonProperty("leaveDescription")]
        [Metadata(DataDescription = "Leave Type Description")]
        public string LeaveDescription { get; set; }

        /// <summary>
        /// Leave Balance for the current Year
        /// </summary>
        public decimal BalanceLeaveHours { get; set; }

        /// <summary>
        /// LeaveTypeUsedHours
        /// </summary>
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

    }
}
