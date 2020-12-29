// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention Alert Case
    /// </summary>
    public class RetentionAlertCase
    {
        /// <summary>
        /// Id number of the student
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Case type under the category
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// Summary of the case item
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Detailed notes of the case
        /// </summary>
        public IEnumerable<string> Notes { get; set; }

        /// <summary>
        /// Method of contact for the case
        /// </summary>
        public IEnumerable<string> MethodOfContact { get; set; }
    }
}
