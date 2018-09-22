// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Services;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student Request.
    /// </summary>
    [Serializable]
    public abstract class StudentRequest
    {
        /// <summary>
        /// resource Id
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }
        private string _id;

        /// <summary>
        /// Student Id
        /// </summary>
        private string _studentId;
        public string StudentId { get { return _studentId; } }
        /// <summary>
        /// Recepient Name
        /// </summary>
        private string _recipientName;
        public string RecipientName { get { return _recipientName; } }
        /// <summary>
        /// Address where the request should be mailed.
        /// </summary>
        public List<string> MailToAddressLines { get; set; }
        /// <summary>
        /// City where request should be mailed.
        /// </summary>
        public string MailToCity { get; set; }
        /// <summary>
        /// State where request should be mailed.
        /// </summary>
        public string MailToState { get; set; }
        /// <summary>
        /// Postal Code where request should be mailed.
        /// </summary>
        public string MailToPostalCode { get; set; }
        /// <summary>
        /// Country where request should be mailed.
        /// </summary>
        public string MailToCountry { get; set; }
        /// <summary>
        /// Hold
        /// </summary>
        public string HoldRequest { get; set; }
        /// <summary>
        /// Number of copies
        /// </summary>
        public int? NumberOfCopies { get; set; }
          
        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// If an invoice was generated as a result of this application this is the invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Date the request was made
        /// </summary>
        public DateTime? RequestDate { get; set; }

        /// <summary>
        /// Date the request was satisfied
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="recipientName"></param>
        public StudentRequest(string studentId, string recipientName)
        {
            this._studentId = studentId;
            this._recipientName = recipientName;
        }

    }
}
