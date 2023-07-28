/* Copyright 2021-2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO for leave request detail.
    /// Each leave request record may contain many leave request detail records.
    /// A leave request detail record contains the leave related details for one day.
    /// </summary>
    
    [DataContract]
    public class LeaveRequestDetail
    {
        /// <summary>
        /// DB id of this leave request detail object
        /// </summary>
        
        [JsonProperty("leaveRequestDetailId")]
        [Metadata("LEAVE.REQUEST.DETAIL.ID", DataDescription = "Id of this leave request detail object.")]
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the leave request object
        /// </summary>
        [JsonProperty("leaveRequestId")]
        [Metadata("LRD.LEAVE.REQUEST.ID", DataDescription = "Leave request Id.")]
        public string LeaveRequestId { get; set; }

        /// <summary>
        /// Date for which leave is requested
        /// </summary>
        [JsonProperty("leaveDate")]
        [Metadata("LRD.LEAVE.DATE", DataDescription = "Leave requested date.")]
        public DateTime LeaveDate { get; set; }

        /// <summary>
        /// Hours of leave requested
        /// </summary>
        [JsonProperty("leaveHours")]
        [Metadata("LRD.LEAVE.HOURS", DataDescription = "Total number of leave requested hours.")]
        public decimal? LeaveHours { get; set; }

        /// <summary>
        /// Indicates if this detail record has been processed in a pay period by payroll
        /// </summary>
        /// 
        [JsonProperty("processedInPayPeriod")]
        [Metadata("LRD.PAY.PERIOD.PROCESSED", DataDescription = "Indicates if this detail record has been processed in a pay period by payroll.")]
        public bool ProcessedInPayPeriod { get; set; }

        /// <summary>
        /// Leave request detail change operator
        /// </summary>
        [JsonProperty("leaveRequestDetailChgopr")]
        [Metadata("LEAVE.REQUEST.DEATIL.CHOOPR", DataDescription = "Operator who changed the leave request details.")]
        public string LeaveRequestDetailChgopr { get; set; }
    }
}
