// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Cost Center Subtotal DTO.
    /// </summary>
    public class CostCenterSubtotal
    {
        /// <summary>
        /// The cost center subtotal ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The cost center subtotal name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of GL accounts that are included in the cost center subtotal.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GlClass GlClass { get; set; }

        /// <summary>
        /// True if the cost center subtotal has been defined in Colleague.
        /// False if the cost center subtotal has not been defined.
        /// </summary>
        public bool IsDefined { get; set; }

        /// <summary>
        /// The total budget amount for the cost center subtotal.
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// The total actuals amount for the cost center subtotal.
        /// </summary>
        public decimal TotalActuals { get; set; }

        /// <summary>
        /// The total encumbrances amount for the cost center subtotal.
        /// </summary>
        public decimal TotalEncumbrances { get; set; }

        /// <summary>
        /// List of GL account numbers that make up the cost center subtotal.
        /// </summary>
        public List<CostCenterGlAccount> GlAccounts { get; set; }

        /// <summary>
        /// List of GL Budget Pools included in the cost center subtotal.
        /// </summary>
        public List<GlBudgetPool> Pools { get; set; }
    }
}

