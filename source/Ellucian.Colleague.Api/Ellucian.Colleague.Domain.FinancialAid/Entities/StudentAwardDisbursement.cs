// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Details of disbursements by award period.
    /// Used within the Ethos Data Model APIs to represent a Student Financial Aid award.
    /// </summary>
    [Serializable]
    public class StudentAwardDisbursement
    {
        /// <summary>
        /// Term for distribution of funds to student
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// The date the disbursement is scheduled to occur
        /// </summary>
        public DateTime ScheduledOn { get; set; }

        /// <summary>
        /// The date the disbursement occurred
        /// </summary>
        public DateTime? DisbursedOn { get; set; }

        /// <summary>
        /// Planned amount of award
        /// </summary>
        public decimal? PlannedAmount { get; set; }

        /// <summary>
        /// Actual amount of award
        /// </summary>
        public decimal? ActualAmount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StudentAwardDisbursement()
        {
        }
    }
}