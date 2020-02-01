// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// File attachment collection identity types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttachmentCollectionIdentityType
    {
        /// <summary>
        /// Role identity
        /// </summary>
        Role,

        /// <summary>
        /// User identity
        /// </summary>
        User,
    }
}