// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
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

        public RetentionAlertSendEmailPreference()
        {
            HasSendEmailFlag = false;
        }

        public RetentionAlertSendEmailPreference(bool hasSendEmailFlag, string message)
        {
            HasSendEmailFlag = hasSendEmailFlag;
            Message = message;
        }
    }
}
