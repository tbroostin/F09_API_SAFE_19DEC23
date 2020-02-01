// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Actions the identity can take on attachments in the collection
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttachmentAction
    {
        /// <summary>
        /// View
        /// </summary>
        View,

        /// <summary>
        /// Create
        /// </summary>
        Create,

        /// <summary>
        /// Delete
        /// </summary>
        Delete,

        /// <summary>
        /// Update
        /// </summary>
        Update
    }
}