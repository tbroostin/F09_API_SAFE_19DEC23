// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The approval type for registration
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ApprovalType
    {
        /// <summary>
        /// Approve All.  All verifications have already been evaluated and the registration
        /// is simply updated into Colleague for consistency.  Currently, only All is supported.
        /// </summary>
        All
    }
}