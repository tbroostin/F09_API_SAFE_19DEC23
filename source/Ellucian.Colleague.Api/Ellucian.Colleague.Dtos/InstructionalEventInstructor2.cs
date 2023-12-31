﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about the instructor of an instructional event
    /// </summary>
    [JsonObject("instructorRoster")]
    public class InstructionalEventInstructor2
    {
        /// <summary>
        /// The instructor
        /// </summary>
        [JsonProperty("instructor")]
        public GuidObject2 Instructor { get; set; }

        /// <summary>
        /// The percentage of the workload this instructor is responsible for
        /// </summary>
        [JsonProperty("workLoadPercentage")]
        public decimal? WorkLoadPercentage { get; set; }

        /// <summary>
        /// The percentage of the responsibility of this instructor for this event
        /// </summary>
        [JsonProperty("responsibilityPercentage")]
        public decimal? ResponsibilityPercentage { get; set; }

        /// <summary>
        /// The date on which this instructor starts working on this event
        /// </summary>
        [JsonProperty("workStartOn")]
        public DateTimeOffset? WorkStartDate { get; set; }

        /// <summary>
        /// The day on which the instructor stops working on this event
        /// </summary>
        [JsonProperty("workEndOn")]
        public DateTimeOffset? WorkEndDate { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public InstructionalEventInstructor2()
        {
            Instructor = new GuidObject2();
        }
    }
}
