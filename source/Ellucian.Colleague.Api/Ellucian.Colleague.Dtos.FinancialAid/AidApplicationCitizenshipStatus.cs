/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of an Citizenship Status for CALISIR
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationCitizenshipStatus
    {
        /// <summary>
        /// Citizen
        /// </summary>
        [EnumMember(Value = "citizen")]        
        Citizen = 1,

        /// <summary>
        /// Non-citizen
        /// </summary>
        [EnumMember(Value = "nonCitizen")]        
        NonCitizen = 2,

        /// <summary>
        /// Non-citizen
        /// </summary>
        [EnumMember(Value = "notEligible")]        
        NotEligible = 3,

    }
}