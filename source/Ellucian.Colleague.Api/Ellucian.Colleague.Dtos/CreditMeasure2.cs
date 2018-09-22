// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Unit of Measure
    /// </summary>
    [JsonConverter(typeof(CreditMeasure2Converter))]
    public enum CreditMeasure2
    {
        /// <summary>
        /// Credits
        /// </summary>
        [EnumMember(Value = "credit")]
        Credit,

        /// <summary>
        /// Continuing Education Units
        /// </summary>
        [EnumMember(Value = "ceu")]
        CEU,

        /// <summary>
        /// Hours
        /// </summary>
        [EnumMember(Value = "hours")]
        Hours
    }
}
