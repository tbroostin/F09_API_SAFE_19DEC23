// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Status of an Organizational Person Position
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrganizationalPersonPositionStatus
    {
        /// <summary>
        /// Organizational Person Position represents a currently active role
        /// </summary>
        Current,

        /// <summary>
        /// Organizational Person Position represents a past role
        /// </summary>
        Past,

        /// <summary>
        /// Organizational Person Position represents a future role
        /// </summary>
        Future,

        /// <summary>
        /// Organizational Person Position has an unknown status
        /// </summary>
        Unknown
    }
}