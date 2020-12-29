// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains configuration information for Budget Development.
    /// </summary>
    public class BudgetConfiguration
    {
        /// <summary>
        /// The working budget ID.
        /// </summary>
        public string BudgetId { get; set; }

        /// <summary>
        /// The working budget title.
        /// </summary>
        public string BudgetTitle { get; set; }

        /// <summary>
        /// The working budget year.
        /// </summary>
        public string BudgetYear { get; set; }

        /// <summary>
        /// The working budget status.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BudgetStatus BudgetStatus { get; set; }

        /// <summary>
        /// List of budget comparables used in budget generation.
        /// </summary>
        public List<BudgetConfigurationComparable> BudgetConfigurationComparables { get; set; }
    }
}

