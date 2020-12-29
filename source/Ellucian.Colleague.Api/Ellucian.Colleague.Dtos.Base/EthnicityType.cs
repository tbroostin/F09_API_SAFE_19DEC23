// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// The different types of ethnicities.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]

    public enum EthnicityType
    {
        /// <summary>
        /// Hispanic
        /// </summary>
        Hispanic,
        /// <summary>
        /// Non-hispanic
        /// </summary>
        NonHispanic,
        /// <summary>
        /// Non-resident
        /// </summary>
        NonResident
    }
}
