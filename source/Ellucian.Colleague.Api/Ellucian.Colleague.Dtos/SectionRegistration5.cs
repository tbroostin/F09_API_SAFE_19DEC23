//Copyright 2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A record of a student's interaction with a specific section such as registration, grades, involvement. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SectionRegistration5 : BaseModel2
    {
        /// <summary>
        /// An instance of a course for which a person is registering.
        /// </summary>
        [JsonProperty("section", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 Section { get; set; }

        /// <summary>
        /// The academic level at which the student is registering for the course (The level specified should match one of the levels allowed for the section).
        /// </summary>
        [JsonProperty("academicLevel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 AcademicLevel { get; set; }

        /// <summary>
        /// The date on which the student originally registered for the section.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("originallyRegisteredOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? OriginallyRegisteredOn { get; set; }

        /// <summary>
        /// The status of this person's registration in the section.
        /// </summary>
        [JsonProperty("status")]
        public SectionRegistrationStatusDtoProperty Status { get; set; }

        /// <summary>
        /// The date on which the status was set.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("statusDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// Unit specification that can be awarded for completing a section.
        /// </summary>
        [JsonProperty("credit", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Credit4DtoProperty Credit { get; set; }

        /// <summary>
        /// The range of dates between which a student was involved with a section.
        /// </summary>
        [JsonProperty("involvement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SectionRegistrationInvolvement Involvement { get; set; }
    }
}