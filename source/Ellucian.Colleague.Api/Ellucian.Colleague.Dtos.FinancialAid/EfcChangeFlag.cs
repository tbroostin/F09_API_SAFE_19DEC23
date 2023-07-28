/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of EFC Change flag
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EfcChangeFlag
    {
        /// <summary>
        /// Increase
        /// </summary>
        [EnumMember(Value = "efcIncrease")]
        EfcIncrease = 1,

        /// <summary>
        /// Decrease
        /// </summary>
        [EnumMember(Value = "efcDecrease")]
        EfcDecrease = 2,

    }
}