/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of Simplified needs test
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SimplifiedNeedsTest
    {
        /// <summary>
        /// Met
        /// </summary>
        [EnumMember(Value = "sntMet")]
        Y,

        /// <summary>
        /// Not met
        /// </summary>
        [EnumMember(Value = "sntNotMet")]
        N,

    }
}