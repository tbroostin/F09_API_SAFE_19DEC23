// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Retention Alert Closed Cases grouped by Closure Reason
    /// </summary>
    [Serializable]
    public class RetentionAlertClosedCasesByReason
    {
        /// <summary>
        /// Retention Alert Case Closure Reason Id
        /// </summary>
        public string ClosureReasonId { get; set; }

        /// <summary>
        /// Retention Alert Closed Cases
        /// </summary>
        public IEnumerable<RetentionAlertClosedCase> Cases { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionAlertClosedCasesByReason"/> class.
        /// </summary>
        public RetentionAlertClosedCasesByReason()
        {
            Cases = new List<RetentionAlertClosedCase>();
        }
    }
}
