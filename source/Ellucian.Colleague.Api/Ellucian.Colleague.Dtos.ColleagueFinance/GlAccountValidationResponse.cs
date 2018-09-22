// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Contains the result of validating a GL account. 
    /// It returns the Id, a status condition, and a set of properties depending on the status.
    /// If the status is set to failure, it returns the Error message that indicates how the validation failed.
    /// If the status is set to success, it returns the FormattedId, Description and the remaining balance that will be used for funds availability.
    /// </summary>
    public class GlAccountValidationResponse
    {
        /// <summary>
        /// Status: success or failure
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// GL account (internal format).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Formatted GL account.
        /// </summary>
        public string FormattedId { get; set; }

        /// <summary>
        /// Description of the GL account.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Remaining Balance for the GL account for a fiscal year.
        /// </summary>
        public decimal? RemainingBalance { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}