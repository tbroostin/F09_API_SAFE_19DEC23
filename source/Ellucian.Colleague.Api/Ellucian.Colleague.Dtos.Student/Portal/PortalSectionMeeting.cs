// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Course section meeting information
    /// </summary>
    public class PortalSectionMeeting
    {
        /// <summary>
        /// Building where this meeting time occurs.
        /// </summary>
        public string Building { get; set; }

        /// <summary>
        /// Room number where this meeting time occurs.
        /// </summary>
        public string Room { get; set; }

        /// <summary>
        /// Meeting time instructional method (such as lecture, lab).
        /// </summary>
        public string InstructionalMethod { get; set; }

        /// <summary>
        /// List of Days of the week (enumerable) when this meeting time occurs.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<DayOfWeek> DaysOfWeek { get; set; }


        /// <summary>
        /// Meeting time start time
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Meeting time end time 
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Portal Section constructor
        /// </summary>
        public PortalSectionMeeting()
        {
            DaysOfWeek = new List<DayOfWeek>();
        }

    }
}
