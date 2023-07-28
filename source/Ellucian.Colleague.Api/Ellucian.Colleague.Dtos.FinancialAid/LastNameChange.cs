/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of Last Name Change
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LastNameChange
    {
        /// <summary>
        /// Last Name Change
        /// </summary>
        [EnumMember(Value = "lastNameChange")]
        N

    }
}