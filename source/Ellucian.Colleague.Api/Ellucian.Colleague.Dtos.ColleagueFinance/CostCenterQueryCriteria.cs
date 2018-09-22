// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Cost Center query filter criteria.
    /// </summary>
    public class CostCenterQueryCriteria
    {
        /// <summary>
        /// List of cost center IDs. Only 0 or 1 IDs will be accepted.
        /// </summary>
        public List<string> Ids { get; set; }

        /// <summary>
        /// Fiscal year from which to get cost center information.
        /// </summary>
        public string FiscalYear { get; set;}

        /// <summary>
        /// Boolean flag to control what type of accounts are returned.
        /// </summary>
        public bool IncludeActiveAccountsWithNoActivity { get; set; }

        /// <summary>
        /// Financial threshold filter criteria.
        /// </summary>
        public List<FinancialThreshold> FinancialThresholds { get; set; }

        /// <summary>
        /// A list of cost center component filter criteria.
        /// </summary>
        public List<CostCenterComponentQueryCriteria> ComponentCriteria { get; set; }
    }

}
