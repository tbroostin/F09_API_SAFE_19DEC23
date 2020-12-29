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
    public class CostCenterQueryCriteria : BaseGlComponentQueryCriteria
    {
        /// <summary>
        /// List of cost center IDs. Only 0 or 1 IDs will be accepted.
        /// </summary>
        public List<string> Ids { get; set; }


        /// <summary>
        /// Financial threshold filter criteria.
        /// </summary>
        public List<FinancialThreshold> FinancialThresholds { get; set; }        

    }

}
