/* Copyright 2016-2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Defines an employement position at an institution.
    /// </summary>
    [DataContract]
    public class Position
    {
        /// <summary>
        /// The database Id
        /// </summary>
        [JsonProperty("id")]
        [Metadata("POSITION.ID", DataDescription = "Unique identifier of this Position.")]
        public string Id { get; set; }

        /// <summary>
        /// A long form title
        /// </summary>
        [JsonProperty("title")]
        [Metadata("POS.TITLE", DataDescription = "A long form title.")]
        public string Title { get; set; }

        /// <summary>
        /// A shortened title
        /// </summary>
        [JsonProperty("shortTitle")]
        [Metadata("POS.SHORT.TITLE", DataDescription = "A shortened/abbreviated title.")]
        public string ShortTitle { get; set; }

        /// <summary>
        /// Whether this is an Exempt position or non-exempt position, meaning the position
        /// is exempt or not exempt from the Fair Labor Standards Act overtime rules. Most
        /// salaried positions are considered exempt. If true, the position is exempt.
        /// </summary>
        [JsonProperty("isExempt")]
        [Metadata("POS.EXEMPT.OR.NOT", DataDescription = "Whether this is an Exempt position or non-exempt position, meaning the position" +
                    "is exempt or not exempt from the Fair Labor Standards Act overtime rules." +
                    "Most salaried positions are considered exempt. If true, the position is exempt.")]
        public bool IsExempt { get; set; }

        /// <summary>
        /// Whether this is a salaried or hourly position. If true, the position is salaried
        /// </summary>
        [JsonProperty("isSalary")]
        [Metadata("POS.HRLY.OR.SLRY", DataDescription = "Whether this is a salaried or hourly position. If true, the position is salaried.")]
        public bool IsSalary { get; set; }

        /// <summary>
        /// The Id of the Position considered the supervising position of this position
        /// </summary>
        [JsonProperty("supervisorPositionId")]
        [Metadata("POS.SUPERVISOR.POS.ID", DataDescription = "The Id of the Position considered the supervising position of this position." +
                    "Used to identify the supervising position.")]
        public string SupervisorPositionId { get; set; }

        /// <summary>
        /// The Id of the Position considered the alternate supervising position of this position
        /// </summary>
        [JsonProperty("alternateSupervisorPositionId")]
        [Metadata("POS.ALT.SUPER.POS.ID", DataDescription = "The Id of the Position considered the alternate supervising position of this position." +
                    "Used to identify the alternate supervisor position.")]
        public string AlternateSupervisorPositionId { get; set; }

        /// <summary>
        /// The type of a timecard associated to this position
        /// See TimecardType attributes for further details
        /// </summary>
        [JsonProperty("timecardType")]
        [Metadata("POS.TIME.ENTRY.FORM", DataDescription = "The type of a timecard associated to this position." +
                    "Timecard type value could be one of these :- Summary, Detailed, Clock, None")]
        public TimecardType TimecardType { get; set; }

        /// <summary>
        /// The Date this position becomes active
        /// </summary>
        [JsonProperty("startDate")]
        [Metadata("POS.START.DATE", DataDescription = "The Date this position becomes active.")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The Date this position becomes inactive. Can be null
        /// </summary>
        [JsonProperty("endDate")]
        [Metadata("POS.END.DATE", DataDescription = "The Date this position becomes inactive. Can be null.")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// A list of Ids of the Pay Schedules associated to this position
        /// </summary>
        [JsonProperty("positionPayScheduleIds")]
        [Metadata("ALL.POSPAY", DataDescription = "A list of Ids of the Pay Schedules(Position Pay Information) associated to this position.")]
        public List<string> PositionPayScheduleIds { get; set; }

        /// <summary>
        /// The department associated with this position
        /// </summary>
        [JsonProperty("positionDept")]
        [Metadata("POS.DEPT", DataDescription = "The department associated with this position")]
        public string PositionDept { get; set; }

        /// <summary>
        /// The location associated with this position
        /// </summary>
        [JsonProperty("positionLocation")]
        [Metadata("POS.LOCATION", DataDescription = "The location associated with this position")]
        public string PositionLocation { get; set; }


    }
}
