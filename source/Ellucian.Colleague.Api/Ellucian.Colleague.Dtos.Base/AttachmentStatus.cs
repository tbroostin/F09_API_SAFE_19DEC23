// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// File attachment statuses
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttachmentStatus
    {
        /// <summary>
        /// Active attachment
        /// </summary>
        Active,

        /// <summary>
        /// Attachment was deleted
        /// </summary>
        Deleted
    }
}