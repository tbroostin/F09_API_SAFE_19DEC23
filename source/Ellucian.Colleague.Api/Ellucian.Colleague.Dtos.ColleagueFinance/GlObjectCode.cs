// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A General Ledger object code DTO.
    /// </summary>
    public class GlObjectCode
    {
        /// <summary>
        /// The GL object code ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The GL object code name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of GL accounts that are included in this GL object code.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GlClass GlClass { get; set; }

        /// <summary>
        /// The total budget amount for the GL object code.
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// The total actuals amount for the GL object code.
        /// </summary>
        public decimal TotalActuals { get; set; }

        /// <summary>
        /// The total encumbrances amount for the GL object code.
        /// </summary>
        public decimal TotalEncumbrances { get; set; }

        /// <summary>
        /// List of GL account numbers that make up the GL object code.
        /// </summary>
        public List<GlObjectCodeGlAccount> GlAccounts { get; set; }

        /// <summary>
        /// List of GL Budget Pools included in the GL object code.
        /// </summary>
        public List<GlObjectCodeBudgetPool> Pools { get; set; }


    }
}

