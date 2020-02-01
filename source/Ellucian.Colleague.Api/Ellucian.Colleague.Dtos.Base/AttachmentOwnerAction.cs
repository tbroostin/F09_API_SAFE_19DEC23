// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Actions the owner can take on attachments they own in the collection
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttachmentOwnerAction
    {
        /// <summary>
        /// Update
        /// </summary>
        Update,

        /// <summary>
        /// Delete
        /// </summary>
        Delete
    }
}