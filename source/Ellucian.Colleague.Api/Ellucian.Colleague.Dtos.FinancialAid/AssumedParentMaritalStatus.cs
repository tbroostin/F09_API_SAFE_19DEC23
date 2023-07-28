/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of Assumed Parent marital status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AssumedParentMaritalStatus
    {
        /// <summary>
        /// MarriedRemarried
        /// </summary>
        [EnumMember(Value = "assumedMarriedRemarried")]
        AssumedMarriedRemarried = 1,

        /// <summary>
        /// Single
        /// </summary>
        [EnumMember(Value = "assumedSingle")]
        AssumedSingle = 2,

    }
}