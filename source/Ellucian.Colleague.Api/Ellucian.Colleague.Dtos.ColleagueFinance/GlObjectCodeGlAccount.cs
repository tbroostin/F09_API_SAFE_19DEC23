// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A General Ledger Account DTO.
    /// </summary>
    public class GlObjectCodeGlAccount
    {
        /// <summary>
        /// This is a GL account in a GL object code.
        /// </summary>
        public string GlAccountNumber { get; set; }

        /// <summary>
        /// This is the GL account formatted for display.
        /// </summary>
        public string FormattedGlAccount { get; set; }

        /// <summary>
        /// The GL account description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The total budget amount for this GL account
        /// </summary>
        public decimal Budget { get; set; }

        /// <summary>
        /// The total encumbrances amount for this GL account
        /// </summary>
        public decimal Encumbrances { get; set; }

        /// <summary>
        /// The total actuals amount for this GL account
        /// </summary>
        public decimal Actuals { get; set; }

        /// <summary>
        /// Is the GL account part of a GL Budget pool and in what capacity.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GlBudgetPoolType PoolType { get; set; }
    }
}
