// Copyright 2017 Ellucian Company L.P. and its affiliates.using System;

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Response from repository related to validating a GL account.
    /// If there is a fiscal year, it is also validated it for that year.
    /// </summary>
    [Serializable]
    public class GlAccountValidationResponse
    {
        /// <summary>
        /// Status of validating the GL account.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// GL account ID.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Remaining Balance for the GL account for a fiscal year.
        /// </summary>
        public decimal RemainingBalance { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes the General ledger account validation response object.
        /// </summary>
        public GlAccountValidationResponse(string id)
        {
            this.id = id;
        }
    }
}