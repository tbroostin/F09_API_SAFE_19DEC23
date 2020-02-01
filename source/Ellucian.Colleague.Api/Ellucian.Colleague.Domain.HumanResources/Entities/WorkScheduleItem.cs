/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class WorkScheduleItem
    {
        /// <summary>
        /// Id of work schedule used to create this item
        /// </summary>
        public string ScheduleId { get; set; }

        /// <summary>
        /// Specific day of week these units apply to
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Units (in hours) for the scheduled time
        /// </summary>
        public decimal WorkUnits { get; set; }

        /// <summary>
        /// Project (if any) thse work units apply to
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Creates a WorkScheduleItem object containing the day of the week, units (hours), and project for a work schedule.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="day"></param>
        /// <param name="workUnits"></param>
        /// <param name="projectId"></param>
        public WorkScheduleItem(string id, DayOfWeek day, decimal workUnits, string projectId = null)
        {
            ScheduleId = id;
            DayOfWeek = day;
            WorkUnits = workUnits;
            ProjectId = projectId;
        }
    }
}
