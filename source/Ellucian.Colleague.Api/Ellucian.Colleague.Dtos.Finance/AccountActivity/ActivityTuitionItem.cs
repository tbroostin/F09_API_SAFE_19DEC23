// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Schedule information for a tuition charge item
    /// </summary>
    public class ActivityTuitionItem : ActivityTermItem
    {
        /// <summary>
        /// Building and room of class
        /// </summary>
        public string Classroom { get; set; }

        /// <summary>
        /// Academic credits
        /// </summary>
        public decimal? Credits { get; set; }

        /// <summary>
        /// Billing credits
        /// </summary>
        public decimal? BillingCredits { get; set; }

        /// <summary>
        /// Continuing Education Units (CEUs)
        /// </summary>
        public decimal? Ceus { get; set; }

        /// <summary>
        /// List of days the class meets
        /// </summary>
        public List<DayOfWeek> Days { get; set; }

        /// <summary>
        /// Class instructor
        /// </summary>
        public string Instructor { get; set; }

        /// <summary>
        /// Class status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Start time of class (HH:MM format)
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// End time of class (HH:MM format)
        /// </summary>
        public string EndTime { get; set; }
    }
}
