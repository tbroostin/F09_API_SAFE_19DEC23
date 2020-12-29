/* Copyright 2016-2019 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{

    /// <summary>
    /// PersonPosition is an object that describes the assignment 
    /// of a position to a person. Employees and non-employees can
    /// hold a position. People can hold more than one position.
    /// </summary>
    public class PersonPosition
    {
        /// <summary>
        /// The database ID of the PersonPosition
        /// The ID will be empty if this entity is a Non-Employee Position as noted by the NonEmployeePosition field
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// The Person Id
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The PositionId. <see cref="Position"/>
        /// </summary>
        public string PositionId { get; set; }

        /// <summary>
        /// The Id of the person's supervisor for this position
        /// </summary>
        public string SupervisorId { get; set; }

        /// <summary>
        /// The Id of the person's alternate supervisor for this position
        /// </summary>
        public string AlternateSupervisorId { get; set; }

        /// <summary>
        /// Contains the list of supervisor Ids (if any) assigned to the supervisory position defined for this position
        /// </summary>
        public List<string> PositionLevelSupervisorIds { get; set; }

        /// <summary>
        /// The date this person begins in this position.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The date this person ends being in this position.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The date this person's position is migrated from WA to SS
        /// </summary>
        public DateTime? MigrationDate { get; set; }

        /// <summary>
        /// The end date of the last PayPeriod the employee entered time for in Web Advisor
        /// </summary>
        public DateTime? LastWebTimeEntryPayPeriodEndDate { get; set; }

        /// <summary>
        /// bool that states whether this PersonPosition is a Non-Employee Position
        /// The Id of this entity will be empty because the Non-Employee Position record comes from HRPER and not PERPOS
        /// </summary>
        public bool NonEmployeePosition { get; set; }

        /// <summary>
        /// Contains the list of work schedule items for this person's position. Each WorkScheduleItem represents a day
        /// of the week with a corresponding unit (in hours) which together form a work schedule for this person's position.
        /// </summary>
        public List<WorkScheduleItem> WorkScheduleItems { get; set; }

        /// <summary>
        /// Decimal field that represents the full-time equivalent value of the employee in the position
        /// </summary>
        public Decimal? FullTimeEquivalent { get; set; }
    }
}
