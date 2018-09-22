// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Contains override information for due dates
    /// </summary>
    [Serializable]
    public class DueDateOverrides
    {
        /// <summary>
        /// Table containing term-based overrides; the key is the term code, and its value is the override date
        /// </summary>
        public Dictionary<string, DateTime> TermOverrides {get; set;}

        /// <summary>
        /// Optional override date for non-term charges
        /// </summary>
        public DateTime? NonTermOverride { get; set; }

        /// <summary>
        /// Override date for the "Past" period (PCF processing)
        /// </summary>
        public DateTime? PastPeriodOverride { get; set; }

        /// <summary>
        /// Override date for the "Current" period (PCF processing)
        /// </summary>
        public DateTime? CurrentPeriodOverride { get; set; }

        /// <summary>
        /// Override date for the "Future" period (PCF processing)
        /// </summary>
        public DateTime? FuturePeriodOverride { get; set; }

        /// <summary>
        /// Constructor for DueDateOverrides - they are optional, so there are no arguments
        /// </summary>
        public DueDateOverrides()
        {
            TermOverrides = new Dictionary<string, DateTime>();
        }
    }
}
