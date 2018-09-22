// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The available categories for Financial Health Indicators.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FinancialThreshold
    {
        /// <summary>
        /// Financial amounts are under the specified threshold.
        /// </summary>
        UnderThreshold,

        /// <summary>
        /// Financial amounts are near the specified threshold.
        /// </summary>
        NearThreshold,

        /// <summary>
        /// Financial amounts are over the specified threshold.
        /// </summary>
        OverThreshold
    }
}
