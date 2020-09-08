// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Retention Alert open case details for reporting
    /// </summary>
    [Serializable]
    public class RetentionAlertOpenCase
    {
        /// <summary>
        /// Case Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Case Category Id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// Cases which are thirty days old
        /// </summary>
        public IEnumerable<string> ThirtyDaysOld { get; set; }

        /// <summary>
        /// Cases which are sixty days old
        /// </summary>
        public IEnumerable<string> SixtyDaysOld { get; set; }

        /// <summary>
        /// Cases which are ninety days old
        /// </summary>
        public IEnumerable<string> NinetyDaysOld { get; set; }

        /// <summary>
        /// Cases which are over ninety days old
        /// </summary>
        public IEnumerable<string> OverNinetyDaysOld { get; set; }

        /// <summary>
        /// Total open cases
        /// </summary>
        public IEnumerable<string> TotalOpenCases { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionAlertOpenCase"/> class.
        /// </summary>
        public RetentionAlertOpenCase()
        {
            ThirtyDaysOld = new List<string>();
            SixtyDaysOld = new List<string>();
            NinetyDaysOld = new List<string>();
            OverNinetyDaysOld = new List<string>();
            TotalOpenCases = new List<string>();
        }
    }
}
