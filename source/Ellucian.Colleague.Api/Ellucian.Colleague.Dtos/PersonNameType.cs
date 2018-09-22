// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Enumeration of possible person name types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PersonNameType
    {
        /// <summary>
        /// Primary
        /// </summary>
        Primary,
        /// <summary>
        /// Birth
        /// </summary>
        Birth
    }
}