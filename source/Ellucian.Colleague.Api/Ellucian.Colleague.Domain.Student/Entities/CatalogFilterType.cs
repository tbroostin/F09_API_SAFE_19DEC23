// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Defines the various types of catalog filters
    /// </summary>
    [Serializable]
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
        Availability,
        /// <summary>
        /// Section Synonyms
        /// </summary>
        Synonyms


    }
}