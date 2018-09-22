// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// This is an instance of a course full representation, offered once
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SectionMaximum3
    {
        /// <summary>
        /// A Globally Unique ID (GUID)
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Section number
        /// </summary>
        [JsonProperty("number", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Number { get; set; }

        /// <summary>
        /// Identifier of a section (Section Name)
        /// </summary>
        [JsonProperty("code", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Code { get; set; }

        /// <summary>
        /// Title of section
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Long description
        /// </summary>
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Section start date
        /// </summary>
        [JsonProperty("startOn")]
        public DateTimeOffset? StartOn { get; set; }

        /// <summary>
        /// Section end date
        /// </summary>
        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset? EndOn { get; set; }

        /// <summary>
        /// Instructional Platform for the section
        /// </summary>
        [JsonProperty("instructionalPlatform", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DtoProperties.InstructionalPlatformDtoProperty InstructionalPlatform { get; set; }

        /// <summary>
        /// Academic Period for the section
        /// </summary>
        [JsonProperty("academicPeriod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DtoProperties.AcademicPeriodDtoProperty2 AcademicPeriod { get; set; }

        /// <summary>
        /// Administrative Period for the section
        /// </summary>
        [JsonProperty("administrativePeriod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DtoProperties.AdministrativePeriodDtoProperty AdministrativePeriod { get; set; }

        /// <summary>
        /// Site for the section
        /// </summary>
        [JsonProperty("site", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DtoProperties.SiteDtoProperty Site { get; set; }

        /// <summary>
        /// Academic Levels for the section
        /// </summary>
        [JsonProperty("academicLevels", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DtoProperties.AcademicLevelDtoProperty> AcademicLevels { get; set; }

        /// <summary>
        /// Course Levels for the section
        /// </summary>
        [JsonProperty("courseLevels", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DtoProperties.CourseLevelDtoProperty> CourseLevels { get; set; }

        /// <summary>
        /// The current status of the section
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SectionStatus2? Status { get; set; }

        /// <summary>
        /// The duration of the section
        /// </summary>
        [JsonProperty("duration", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SectionDuration2 Duration { get; set; }

        /// <summary>
        /// The maximum enrollment for the section
        /// </summary>
        [JsonProperty("maxEnrollment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? MaximumEnrollment { get; set; }

        /// <summary>
        /// Owning Organizations for the section
        /// </summary>
        [JsonProperty("owningInstitutionUnits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DtoProperties.OwningOrganizationDtoProperty> OwningOrganizations { get; set; }

        /// <summary>
        /// The duration of the section
        /// </summary>
        [JsonProperty("course")]
        public CourseDtoProperty Course { get; set; }

        /// <summary>
        /// Owning Organizations for the section
        /// </summary>
        [JsonProperty("credits", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DtoProperties.Credit2DtoProperty> Credits { get; set; }

       /// <summary>
        /// Owning Organizations for the section
        /// </summary>
        [JsonProperty("gradeSchemes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DtoProperties.GradeSchemeDtoProperty2> GradeSchemes { get; set; }

        /// <summary>
        /// InstructionalEvents for the section
        /// </summary>
        [JsonProperty("instructionalEvents", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DtoProperties.InstructionalEventDtoProperty2> InstructionalEvents { get; set; }

        /// <summary>
        /// Contructor
        /// </summary>
        [JsonConstructor]
        public SectionMaximum3(): base()
        {
            
        }

    }
}
