// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A Project Line item DTO 
    /// (this class is moved to Colleague Finance module from Projects Accounting module)
    /// </summary>
    public class ProjectLineItem
    {
        /// <summary>
        /// This is the line item code
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// The type of GL accounts that are included in the cost center subtotal.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GlClass GlClass { get; set; }

        /// <summary>
        /// This is the line item total budget
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// This is the line item total actuals
        /// </summary>
        public decimal TotalActuals { get; set; }

        /// <summary>
        /// This is the line item total memo actuals
        /// </summary>
        public decimal TotalMemoActuals { get; set; }

        /// <summary>
        /// This is the line item total encumbrances
        /// </summary>
        public decimal TotalEncumbrances { get; set; }

        /// <summary>
        /// This is the line item total memo encumbrances
        /// </summary>
        public decimal TotalMemoEncumbrances { get; set; }

        /// <summary>
        /// List of GL accounts for the line item
        /// </summary>
        public List<ProjectLineItemGlAccount> GlAccounts { get; set; }
    }
}
