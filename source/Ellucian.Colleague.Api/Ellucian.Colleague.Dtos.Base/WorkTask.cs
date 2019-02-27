// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// An assigned work task.
    /// </summary>
    public class WorkTask
    {
        /// <summary>
        /// Unique ID of this task
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Category for this task, user-defined, used for grouping like tasks
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Detailed task descriptions
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The related process associated with this task, if available
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public WorkTaskProcess TaskProcess { get; set; }

        /// <summary>
        /// Exec State
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ExecutionState? ExecState { get; set; }

        /// <summary>
        /// Start date and time
        /// </summary>
        public DateTimeOffset? StartDate { get; set; }
    }
}
