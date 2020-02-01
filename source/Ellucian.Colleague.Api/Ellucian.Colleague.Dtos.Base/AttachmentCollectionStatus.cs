// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// File attachment collection statuses
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttachmentCollectionStatus
    {
        /// <summary>
        /// Active attachment collection
        /// </summary>
        Active,

        /// <summary>
        /// Inactive attachment collection
        /// </summary>
        Inactive
    }
}