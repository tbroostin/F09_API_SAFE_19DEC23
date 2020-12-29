// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Defines the types of course catalog filters available for customization in the Course Catalog 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CatalogFilterType
    {
        /// <summary>
        /// Course types filter
        /// </summary>
        CourseTypes,
        /// <summary>
        /// Topic code filter
        /// </summary>
        TopicCodes,
        /// <summary>
        /// Instructional Type filter
        /// </summary>
        InstructionTypes,
        /// <summary>
        /// Course Levels filter
        /// </summary>
        CourseLevels,
        /// <summary>
        /// Academic level filter
        /// </summary>
        AcademicLevels,
        /// <summary>
        /// Day of Week Filter
        /// </summary>
        DaysOfWeek,
        /// <summary>
        /// Time of Day Filter
        /// </summary>
        TimesOfDay,
        /// <summary>
        /// Location filter
        /// </summary>
        Locations,
        /// <summary>
        /// Instructor filter
        /// </summary>
        Instructors,
        /// <summary>
        /// Term filter
        /// </summary>
        Terms,
        /// <summary>
        /// Availability Filter
        /// </summary>
        Availability
    }
}
