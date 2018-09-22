/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    ///  Describes the dates and status for a pay period within a pay cycle. The status is based on the pay control record associated to the pay period.
    /// </summary>
    public class PayPeriod
    {
        /// <summary>
        /// The pay period start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The pay period end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The employee deadline for submission
        /// </summary>
        public DateTimeOffset? EmployeeTimecardCutoffDateTime { get; set; }

        /// <summary>
        /// The supervisor deadline for approval
        /// </summary>
        public DateTimeOffset? SupervisorTimecardCutoffDateTime { get; set; }

        /// <summary>
        /// The status of this pay period
        /// </summary>
        public PayPeriodStatus Status { get; set; }

        /// <summary>
        /// The Id of the PayCycle to which this PayPeriod belongs
        /// </summary>
        public string PayCycle { get; set; }
    }
}
