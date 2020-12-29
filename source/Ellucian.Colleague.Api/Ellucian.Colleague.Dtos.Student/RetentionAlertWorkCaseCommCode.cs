﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert work the case action
    /// </summary>
    public class RetentionAlertWorkCaseCommCode
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Communication Code to add to the Case
        /// </summary>
        public string CommunicationCode { get; set; }

    }
}
