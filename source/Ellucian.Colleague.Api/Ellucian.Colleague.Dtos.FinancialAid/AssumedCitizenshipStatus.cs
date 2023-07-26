/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of Assumed citizenship status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AssumedCitizenshipStatus
    {
        /// <summary>
        /// Citizen
        /// </summary>
        [EnumMember(Value = "assumedCitizen")]
        AssumedCitizen = 1,

        /// <summary>
        /// Non-citizen
        /// </summary>
        [EnumMember(Value = "assumedEligibleNoncitizen")]
        AssumedEligibleNoncitizen = 2,

    }
}