// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Filter for account-funds-available and accountSpecifications named query
    /// </summary>
    public class AccountingStringsFilter
    {
        /// <summary>
        /// Filter to return all records effective on a given date.
        /// </summary>
        [JsonProperty("effectiveOn")]
        [FilterProperty("effectiveOn")]
        public DateTime? EffectiveOn { get; set; }
    }
}
