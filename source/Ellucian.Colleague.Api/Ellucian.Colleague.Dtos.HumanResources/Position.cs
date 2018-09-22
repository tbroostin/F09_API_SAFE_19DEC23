/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Defines an employement position at an institution.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A long form title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A shortened title
        /// </summary>
        public string ShortTitle { get; set; }

        /// <summary>
        /// Whether this is an Exempt position or non-exempt position, meaning the position
        /// is exempt or not exempt from the Fair Labor Standards Act overtime rules. Most
        /// salaried positions are considered exempt. If true, the position is exempt.
        /// </summary>
        public bool IsExempt { get; set; }

        /// <summary>
        /// Wehther this is a salaried or hourly position. If true, the position is salaried
        /// </summary>
        public bool IsSalary { get; set; }

        /// <summary>
        /// The Id of the Position considered the supervising position of this position
        /// </summary>
        public string SupervisorPositionId { get; set; }

        /// <summary>
        /// The Id of the Position considered the alternate supervising position of this position
        /// </summary>
        public string AlternateSupervisorPositionId { get; set; }

        /// <summary>
        /// The type of a timecard associated to this position
        /// See TimecardType attributes for further details
        /// </summary>
        public TimecardType TimecardType { get; set; }

        /// <summary>
        /// The Date this position becomes active
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The Date this position becomes inactive. Can be null
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The Id of the Workweek Schedule. Not sure if needed yet.
        /// </summary>
        //public string WorkWeekScheduleId { get; set; }

        /// <summary>
        /// A list of Ids of the Pay Schedules associated to this position
        /// </summary>
        public List<string> PositionPayScheduleIds { get; set; }

        /// <summary>
        /// The department associated with this position
        /// </summary>
        public string PositionDept { get; set; }

        /// <summary>
        /// The location associated with this position
        /// </summary>
        public string PositionLocation { get; set; }


    }
}
