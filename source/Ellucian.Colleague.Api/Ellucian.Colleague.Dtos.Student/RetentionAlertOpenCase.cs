// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention Alert open case details for reporting
    /// </summary>
    public class RetentionAlertOpenCase
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Case Category Id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the cases which are thirty days old.
        /// </summary>
        public IEnumerable<string> ThirtyDaysOld { get; set; }

        /// <summary>
        /// Gets or sets the cases which are sixty days old.
        /// </summary>
        public IEnumerable<string> SixtyDaysOld { get; set; }

        /// <summary>
        /// Gets or sets the cases which are ninety days old.
        /// </summary>
        public IEnumerable<string> NinetyDaysOld { get; set; }

        /// <summary>
        /// Gets or sets the cases over ninety days old.
        /// </summary>
        public IEnumerable<string> OverNinetyDaysOld { get; set; }

        /// <summary>
        /// Gets or sets the total open cases.
        /// </summary>
        public IEnumerable<string> TotalOpenCases { get; set; }
    }
}
