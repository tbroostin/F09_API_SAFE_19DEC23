// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
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

        public RetentionAlertWorkCaseSendMail(string updatedBy, string mailSubject, string mailBody, List<string> mailNames, List<string> mailAddresses, List<string> mailTypes)
        {
            if (mailNames == null)
            {
                MailNames = new List<string>();
            }
            if (mailAddresses == null)
            {
                MailAddresses = new List<string>();
            }
            if (mailTypes == null)
            {
                MailTypes = new List<string>();
            }
            if (!string.IsNullOrEmpty(updatedBy))
            {
                UpdatedBy = updatedBy;
            }

            MailSubject = mailSubject;
            MailBody = mailBody;
        }
    }
}
