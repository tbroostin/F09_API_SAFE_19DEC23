﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The approval type for registration
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ApprovalEntity
    {
        /// <summary>
        /// User approval is obtained which may differ from System approval.
        /// Colleague currently makes no distinction.
        /// </summary>
        [EnumMember(Value = "user")]
        User,
        /// <summary>
        /// System Approval is obtained and all updates are initiated by the system.
        /// Colleague currently makes no distinction.
        /// </summary>
        [EnumMember(Value = "system")]
        System
    }
}