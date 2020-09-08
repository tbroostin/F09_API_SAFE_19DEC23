﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert send email preference
    /// </summary>
    public class RetentionAlertSendEmailPreference
    {
        /// <summary>
        /// Has send email
        /// </summary>
        public bool HasSendEmailFlag { get; set; }

        /// <summary>
        /// Message that carries if the user has set the email flag
        /// </summary>
        public string Message { get; set; }
    }
}
