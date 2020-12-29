// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Status of a person agreement
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PersonAgreementStatus
    {
        /// <summary>
        /// Indicates acceptance of the person agreement
        /// </summary>
        Accepted,
        /// <summary>
        /// Indicates declination of the person agreement
        /// </summary>
        Declined
    }
}
