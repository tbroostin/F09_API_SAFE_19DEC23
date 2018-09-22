/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A LoanRequest DTO
    /// </summary>
    public class LoanRequest
    {
        /// <summary>
        /// The unique identifier of this LoanRequest
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Colleague PERSON id of the student who submitted this loanRequest
        /// Required for POST
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The AwardYear for which this LoanRequest applies
        /// Required for POST
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The date the student submitted this LoanRequest
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// The total loan amount the student requested
        /// Required for POST
        /// </summary>
        public int TotalRequestAmount { get; set; }

        /// <summary>
        /// A list of loan request periods
        /// At least one is Required for POST
        /// </summary>
        public List<LoanRequestPeriod> LoanRequestPeriods { get; set; }

        /// <summary>
        /// Any comments the student entered along with the request.
        /// </summary>
        public string StudentComments { get; set; }

        /// <summary>
        /// The current status of the loan request
        /// </summary>       
        public LoanRequestStatus Status { get; set; }

        /// <summary>
        /// The latest date that the current status was updated.
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// The Colleague PERSON Id of the loan counselor this request was assigned to
        /// </summary>
        public string AssignedToId { get; set; }
    }
}
