/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of statuses that can be assigned to a LoanRequest object
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LoanRequestStatus
    {
        /// <summary>
        /// Accepted status indicates that the LoanRequest was accepted by a loan counselor, and a new loan(s) or additional loan money was added
        /// to the student's award records
        /// </summary>
        Accepted,

        /// <summary>
        /// Pending status indicates the LoanRequest was submitted successfully and is waiting to be processed
        /// </summary>
        Pending,

        /// <summary>
        /// Rejected status indicates the LoanRequest was rejected by a loan counselor. No new additional loans or loan money was added to the student's
        /// award records
        /// </summary>
        Rejected
    }
}
