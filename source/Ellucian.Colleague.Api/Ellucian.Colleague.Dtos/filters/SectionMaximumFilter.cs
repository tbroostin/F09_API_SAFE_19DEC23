// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Filter used for sections-maximum v8.  Supports multiple filters standards.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SectionMaximumFilter
    {
        /// <summary>
        /// Section number
        /// </summary>
        [JsonProperty("number", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public string Number { get; set; }

        /// <summary>
        /// Identifier of a section (Section Name)
        /// </summary>
        [JsonProperty("code", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public string Code { get; set; }

        /// <summary>
        /// Title of section
        /// </summary>
        [JsonProperty("title")]
        [FilterProperty("criteria")]
        public string Title { get; set; }

        /// <summary>
        /// Section start date
        /// </summary>
        [JsonProperty("startOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public DateTimeOffset? StartOn { get; set; }

        /// <summary>
        /// Section end date
        /// </summary>
        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public DateTimeOffset? EndOn { get; set; }

        /// <summary>
        /// Instructional Platform for the section
        /// </summary>
        [JsonProperty("instructionalPlatform", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(CodeTitleDetailFilterConverter<DtoProperties.InstructionalPlatformDtoProperty>))]
        public DtoProperties.InstructionalPlatformDtoProperty InstructionalPlatform { get; set; }

        /// <summary>
        /// Academic Period for the section
        /// </summary>
        [JsonProperty("academicPeriod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(CodeTitleDetailFilterConverter<DtoProperties.AcademicPeriodDtoProperty2>))]  
        public DtoProperties.AcademicPeriodDtoProperty2 AcademicPeriod { get; set; }

        /// <summary>
        /// Site for the section
        /// </summary>
        [JsonProperty("site", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(CodeTitleDetailFilterConverter<DtoProperties.SiteDtoProperty>))] 
        public DtoProperties.SiteDtoProperty Site { get; set; }

        /// <summary>
        /// Academic Levels for the section
        /// </summary>
        [JsonProperty("academicLevels", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(CodeTitleDetailFilterConverter<List<DtoProperties.AcademicLevelDtoProperty>>))]
        public IEnumerable<DtoProperties.AcademicLevelDtoProperty> AcademicLevels { get; set; }

        /// <summary>
        /// The current status of the section
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public SectionStatus2? Status { get; set; }

        /// <summary>
        /// Owning Organizations for the section
        /// </summary>
        [JsonProperty("owningInstitutionUnits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(CodeTitleDetailFilterConverter<List<DtoProperties.OwningOrganizationDtoProperty>>))]
        public IEnumerable<DtoProperties.OwningOrganizationDtoProperty> OwningOrganizations { get; set; }

        /// <summary>
        /// The duration of the section
        /// </summary>
        [JsonProperty("course")]
        [JsonConverter(typeof(CodeTitleDetailFilterConverter<CourseDtoProperty>))]
        public CourseDtoProperty Course { get; set; }

        /// <summary>
        /// InstructionalEvents for the section
        /// </summary>
        [JsonProperty("instructionalEvents", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DtoProperties.InstructionalEventDtoProperty2> InstructionalEvents { get; set; }

        /// <summary>
        /// Contructor
        /// </summary>
        [JsonConstructor]
        public SectionMaximumFilter() : base()
        { }
    }
}
