// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Unit of Measure
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CreditMeasure
    {
        /// <summary>
        /// Credits
        /// </summary>
        Credit,

        /// <summary>
        /// Continuing Education Units
        /// </summary>
        CEU,

        /// <summary>
        /// Hours
        /// </summary>
        Hours
    }
}
