// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Retention Alert Closed Cases grouped by Closure Reason
    /// </summary>
    [Serializable]
    public class RetentionAlertClosedCase
    {
        /// <summary>
        /// Retention Alert Case Id
        /// </summary>
        public string CasesId { get; set; }

        /// <summary>
        /// Last Action Date
        /// </summary>
        public DateTime? LastActionDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionAlertClosedCase"/> class.
        /// </summary>
        public RetentionAlertClosedCase()
        {           
        }
    }
}
