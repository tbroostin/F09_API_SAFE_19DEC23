// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Enumeration of possible  user profile property permissions
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecutionState
    {
        /// <summary>
        /// Workflow or worklist has been opened but not started
        /// </summary>
        OpenNotStarted,

        /// <summary>
        /// Workflow or worklist has been opened but is not active
        /// </summary>
        OpenNotActive,

        /// <summary>
        /// Workflow or worklist has been completed and closed
        /// </summary>
        ClosedCompleted
    }
}
