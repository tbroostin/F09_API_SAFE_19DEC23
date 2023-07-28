/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of Assumed Yes/No
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AssumedYesNo
    {
        /// <summary>
        /// Yes
        /// </summary>
        [EnumMember(Value = "assumedYes")]
        AssumedYes = 1,

        /// <summary>
        /// No
        /// </summary>
        [EnumMember(Value = "assumedNo")]
        AssumedNo = 2,

    }
}