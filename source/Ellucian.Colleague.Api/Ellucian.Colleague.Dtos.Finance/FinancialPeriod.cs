// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Dtos.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A financial period
    /// </summary>
    public class FinancialPeriod
    {
        /// <summary>
        /// Period type - Past, Current, or Future
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PeriodType Type { get; set; }

        /// <summary>
        /// Optional start date of the financial period
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        /// Optional end date of the financial period
        /// </summary>
        public DateTime? End { get; set; }
    }
}
