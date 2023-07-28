/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of Assumed Student marital status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AssumedStudentMaritalStatus
    {
        /// <summary>
        /// Single
        /// </summary>
        [EnumMember(Value = "assumedSingle")]
        AssumedSingle = 1,

        /// <summary>
        /// MarriedRemarried
        /// </summary>
        [EnumMember(Value = "assumedMarriedRemarried")]
        AssumedMarriedRemarried = 2,

    }
}