﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The assignment of a student to institution provided housing. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class HousingAssignment : BaseModel2
    {
        /// <summary>
        /// A reference to the student assigned a room.
        /// </summary>
        [JsonProperty("person")]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// The room assigned to the student.
        /// </summary>
        [JsonProperty("room", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Room { get; set; }

        /// <summary>
        /// The academic period associated with the housing assignment.
        /// </summary>
        [JsonProperty("academicPeriod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 AcademicPeriod { get; set; }

        /// <summary>
        /// The date the housing assignment starts.
        /// </summary>
        [JsonProperty("startOn")]
        public DateTimeOffset? StartOn { get; set; }

        /// <summary>
        /// The date the housing assignment ends
        /// </summary>
        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? EndOn { get; set; }

        /// <summary>
        /// The status of the room assignment.
        /// </summary>
        [JsonProperty("status")]
        public HousingAssignmentsStatus Status { get; set; }

        /// <summary>
        /// The date the housing assignment ends.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("statusDate", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// A reference to the person's request for housing.
        /// </summary>
        [JsonProperty("request", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public GuidObject2 HousingRequest { get; set; }

        /// <summary>
        /// The contract number room assignment.
        /// </summary>
        [JsonProperty("contractNumber", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string ContractNumber { get; set; }

        /// <summary>
        /// The comments for room assignment.
        /// </summary>
        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; set; }

        /// <summary>
        /// A reference to the charge to the student for the housing assignment.
        /// </summary>
        [JsonProperty("roomRate", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public GuidObject2 RoomRate { get; set; }

        /// <summary>
        /// The interval for which rate is defined.
        /// </summary>
        [JsonProperty("ratePeriod", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public EnumProperties.RatePeriod? RatePeriod { get; set; }

        /// <summary>
        /// The override rate for the room assignment.
        /// </summary>
        [JsonProperty("rateOverride", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public HousingAssignmentRateOverrideProperty RateOverride { get; set; }

        /// <summary>
        /// The additional charges applied to the student for the housing assignment.
        /// </summary>
        [JsonProperty("additionalCharges", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<HousingAssignmentAdditionalChargeProperty> AdditionalCharges { get; set; }

        /// <summary>
        /// The resident type of the student for this housing assignment.
        /// </summary>
        [JsonProperty("residentType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public GuidObject2 ResidentType { get; set; }   
    }
}