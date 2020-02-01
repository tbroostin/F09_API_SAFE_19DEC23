/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Item containing the day of the week, units (hours), and project for a work schedule. A collection of
    /// WorkScheduleItems creates the work schedule for a given person's position.
    /// </summary>
    public class WorkScheduleItem
    {
        /// <summary>
        /// Id of work schedule used to create this item
        /// </summary>
        public string ScheduleId { get; set; }

        /// <summary>
        /// Specific day of week these units apply to
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Units (in hours) for the scheduled time
        /// </summary>
        public decimal WorkUnits { get; set; }

        /// <summary>
        /// Project (if any) thse work units apply to
        /// </summary>
        public string ProjectId { get; set; }
    }
}
