// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert work case send mail
    /// </summary>
    public class RetentionAlertWorkCaseSendMail
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Subject of the mail
        /// </summary>
        public string MailSubject { get; set; }

        /// <summary>
        /// Body of the mail
        /// </summary>
        public string MailBody { get; set; }

        /// <summary>
        /// The mail name for the persons
        /// </summary>
        public IEnumerable<string> MailNames { get; set; }

        /// <summary>
        /// The mail address of the person 
        /// </summary>
        public IEnumerable<string> MailAddresses { get; set; }

        /// <summary>
        /// The type for the mail address
        /// </summary>
        public IEnumerable<string> MailTypes { get; set; }
    }
}
