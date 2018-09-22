// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Entities for instructional event approvals
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstructionalEventApprovalEntity
    {
        /// <summary>
        /// User
        /// </summary>
        User,

        /// <summary>
        /// System
        /// </summary>
        System
    }
}
