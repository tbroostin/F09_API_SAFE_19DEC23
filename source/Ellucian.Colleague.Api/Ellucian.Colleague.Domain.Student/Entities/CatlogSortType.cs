// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Defines the types of course catalog for which sorting can occur on sections.
    /// </summary>
    [Serializable]

    public enum CatalogSortType
    {

        /// <summary>
        /// No need to sort
        /// </summary>
        None,
        /// <summary>
        /// Status has three values- Waitlisted , Open, Closed
        /// </summary>
        Status,
        /// <summary>
        /// This is combination of CourseName + Section number
        /// </summary>
        SectionName,
        /// <summary>
        /// Number of seats available, which is Available property in Section, If there is no capacity, Available property is null
        /// </summary>
        SeatsAvailable,
        /// <summary>
        /// Location of the section
        /// </summary>
        Location,
        /// <summary>
        /// Term
        /// </summary>
        Term,
        /// <summary>
        /// Section Title
        /// </summary>
        Title,
        /// <summary>
        /// Section Start Dates
        /// </summary>
        Dates,
        /// <summary>
        /// Instructional Methods
        /// </summary>
        InstructionalMethod,
        /// <summary>
        /// Section Meeting
        /// </summary>
        MeetingInformation,
        /// <summary>
        /// Faculty Name
        /// </summary>
        FacultyName,
        /// <summary>
        /// Section Credits/CEUs
        /// </summary>
        Credits,
        /// <summary>
        /// Course Types
        /// </summary>
        CourseType,
        /// <summary>
        /// Academic Level
        /// </summary>
        AcademicLevel

    }
}
