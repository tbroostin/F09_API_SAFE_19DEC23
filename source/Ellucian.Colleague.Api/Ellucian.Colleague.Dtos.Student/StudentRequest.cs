// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Abstarct class to define student request - transcript request or enrollment request.
    /// </summary>
    public abstract class StudentRequest
    {
        /// <summary>
        /// Student Request Id
        /// </summary>
        public string Id { get; set; }
       
        /// <summary>
        /// Student Id
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string StudentId { get ; set;  }
        
        /// <summary>
        /// Recepient Name
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string RecipientName { get; set; }
        /// <summary>
        /// Address where the request should be mailed.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<string> MailToAddressLines { get; set; }
        /// <summary>
        /// City where enrollment verification or transcript should be mailed.
        /// </summary>
        public string MailToCity { get; set; }
        /// <summary>
        /// State where enrollment verification or transcript should be mailed.
        /// </summary>
        public string MailToState { get; set; }
        /// <summary>
        /// Postal Code where enrollment verification or transcript should be mailed.
        /// </summary>
        public string MailToPostalCode { get; set; }
        /// <summary>
        /// Country where enrollment verification or transcript should be mailed.
        /// </summary>
        public string MailToCountry { get; set; }
        /// <summary>
        /// Code which indicates that the request should not be immediately fulfilled
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
       /// <param name="MailToAddressLines"></param>
        public StudentRequest(string studentId, string recipientName,List<string> MailToAddressLines)
        {
            //StudentId = studentId;
            //RecipientName = recipientName;
            this.StudentId = studentId;
            this.RecipientName = recipientName;
            if (MailToAddressLines != null)
            {
                this.MailToAddressLines = new List<string>();
                this.MailToAddressLines = MailToAddressLines;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public StudentRequest()
        { 
             MailToAddressLines=new List<string>();
        }

        
    }
}
