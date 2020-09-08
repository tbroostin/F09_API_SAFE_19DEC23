// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention Alet Closed Cases grouped by Closure Reason.
    /// </summary>
    public class RetentionAlertClosedCasesByReason
    {
        /// <summary>
        /// Retention Alert Case Closure Reason
        /// </summary>
        public string ClosureReasonId { get; set; }

        /// <summary>
        /// Retention Alert Case Ids
        /// </summary>
        public IEnumerable<RetentionAlertClosedCase> Cases { get; set; }
    }
}
