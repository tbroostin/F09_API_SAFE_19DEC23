/* Copyright 2016-2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.HumanResources
{

    /// <summary>
    /// PersonPosition is an object that describes the assignment 
    /// of a position to a person. Employees and non-employees can
    /// hold a position. People can hold more than one position.
    /// </summary>
    [DataContract]
    public class PersonPosition
    {
        /// <summary>
        /// The database ID of the PersonPosition
        /// The ID will be empty if this entity is a Non-Employee Position as noted by the NonEmployeePosition field
        /// </summary>
        [JsonProperty("id")]
        [Metadata("PERPOS.ID", DataDescription = "Unique identifier of this PersonPosition. This id will be empty if this entity is a Non-Employee Position as noted by the NonEmployeePosition field.")]
        public string Id { get; set; }


        /// <summary>
        /// The Person Id
        /// </summary>
        [JsonProperty("personId")]
        [Metadata("PERPOS.HRP.ID", DataDescription = "Person id (HRPER Id).")]
        public string PersonId { get; set; }

        /// <summary>
        /// The PositionId. <see cref="Position"/>
        /// </summary>
        [JsonProperty("positionId")]
        [Metadata("PERPOS.POSITION.ID", DataDescription = "The PositionId.")]
        public string PositionId { get; set; }

        /// <summary>
        /// The Id of the person's supervisor for this position
        /// </summary>
        [JsonProperty("supervisorId")]
        [Metadata("PERPOS.SUPERVISOR.HRP.ID", DataDescription = "The Id of the person's supervisor for this position.")]
        public string SupervisorId { get; set; }

        /// <summary>
        /// The Id of the person's alternate supervisor for this position
        /// </summary>
        [JsonProperty("alternateSupervisorId")]
        [Metadata("PERPOS.ALT.SUPERVISOR.ID", DataDescription = "The Id of the person's alternate supervisor for this position.")]
        public string AlternateSupervisorId { get; set; }

        /// <summary>
        /// Contains the list of supervisor Ids (if any) assigned to the supervisory position defined for this position
        /// </summary>
        [JsonProperty("positionLevelSupervisorIds")]
        public List<string> PositionLevelSupervisorIds { get; set; }

        /// <summary>
        /// The date this person begins in this position.
        /// </summary>
        [JsonProperty("startDate")]
        [Metadata("PERPOS.START.DATE", DataDescription = "The date this person begins in this position.")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The date this person ends being in this position.
        /// </summary>
        [JsonProperty("endDate")]
        [Metadata("PERPOS.END.DATE", DataDescription = "The date this person ends in this position.")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The date this person's position is migrated from WA to SS
        /// </summary>
        [JsonProperty("migrationDate")]
        [Metadata("EMP.SS.PPD.END.DATES", DataDescription = "The date this person's position is migrated from Web Advisor to Self-Service.")]
        public DateTime? MigrationDate { get; set; }

        /// <summary>
        /// The end date of the last PayPeriod the employee entered time for in Web Advisor
        /// </summary>
        [JsonProperty("lastWebTimeEntryPayPeriodEndDate")]
        [Metadata("EMPTH.PERIOD.END.DATE", DataDescription = "The end date of the last PayPeriod the employee entered time for in Web Advisor.")]
        public DateTime? LastWebTimeEntryPayPeriodEndDate { get; set; }

        /// <summary>
        /// bool that states whether this PersonPosition is a Non-Employee Position
        /// The Id of this entity will be empty because the Non-Employee Position record comes from HRPER and not PERPOS
        /// </summary>
        [JsonProperty("nonEmployeePosition")]
        [Metadata(DataDescription = "Indicates whether this PersonPosition is a Non-Employee Position." +
            "The Id of this entity will be empty because the Non-Employee Position record comes from HRPER and not PERPOS")]
        public bool NonEmployeePosition { get; set; }

        /// <summary>
        /// Contains the list of work schedule items for this person's position. Each WorkScheduleItem represents a day
        /// of the week with a corresponding unit (in hours) which together form a work schedule for this person's position.
        /// </summary>
        [JsonProperty("workScheduleItems")]
        [Metadata(DataDescription = "Contains the list of work schedule items for this person's position." +
            "Each WorkScheduleItem represents a day of the week with a corresponding unit (in hours) which together form a work schedule for this person's position.")]
        public List<WorkScheduleItem> WorkScheduleItems { get; set; }

        /// <summary>
        /// Decimal field that represents the full-time equivalent value of the employee in the position
        /// </summary>
        [JsonProperty("fullTimeEquivalent")]
        [Metadata("PERPOS.FTE", DataDescription = "Full-time equivalent (FTE) value of the employee in the position")]
        public Decimal? FullTimeEquivalent { get; set; }
    }
}
