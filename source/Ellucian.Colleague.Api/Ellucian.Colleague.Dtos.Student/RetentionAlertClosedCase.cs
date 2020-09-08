// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention Alet Closed Case and Last Action Date
    /// </summary>
    public class RetentionAlertClosedCase
    {
        /// <summary>
        /// Retention Alert Case Closure Reason
        /// </summary>
        public string CasesId { get; set; }

        /// <summary>
        /// Last Action Date
        /// </summary>
        public DateTime? LastActionDate { get; set; }
    }
}
