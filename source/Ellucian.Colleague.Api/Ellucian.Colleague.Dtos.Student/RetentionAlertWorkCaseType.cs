﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert work the case action
    /// </summary>
    public class RetentionAlertWorkCaseType
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Case Type to add to the Case
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// Internal/Detailed Notes to add to the Case Item being recorded (added) to the the case
        /// </summary>
        public IEnumerable<string> Notes { get; set; }

    }
}
