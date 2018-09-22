// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The GL account DTO.
    /// </summary>
    public class CostCenterGlAccount
    {
        /// <summary>
        /// This is the GL account for the line item GL distribution.
        /// </summary>
        public string GlAccountNumber { get; set; }

        /// <summary>
        /// This is the GL account for the line item GL distribution formatted for display.
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
