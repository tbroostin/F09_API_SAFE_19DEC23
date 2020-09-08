// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The response received from a Retention Alert Work Case Action 
    /// </summary>
    public class RetentionAlertWorkCaseActionResponse
    {
        /// <summary>
        ///  Key of the case record
        /// </summary>
        public string CaseId { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error messages
        /// </summary>
        public IEnumerable<string> ErrorMessages { get; set; }
    }
}
