// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Defines the various types of catalog search result headers
    /// </summary>
    [Serializable]
    public enum CatalogSearchResultHeaderType
    {
        /// <summary>
        /// Academic Level Header
        /// </summary>
        AcademicLevel,
        /// <summary>
        /// Location Header
        /// </summary>
        Location,
        /// <summary>
        /// Planned Status Header (planned, completed, withdrawn etc)
        /// </summary>
        PlannedStatus,
        /// <summary>
        /// Term Header
        /// </summary>
        Term,
        /// <summary>
        /// Section Status Header (open, waitlisted, closed)
        /// </summary>
        SectionStatus,
        /// <summary>
        /// Section Name Header
        /// </summary>
        SectionName,
        /// <summary>
        /// Section Title Header
        /// </summary>
        SectionTitle,
        /// <summary>
        /// Section Dates Header
        /// </summary>
        SectionDates,
        /// <summary>
        /// Meeting Information Header
        /// </summary>
        MeetingInformation,
        /// <summary>
        /// Faculty Header
        /// </summary>
        Faculty,
        /// <summary>
        /// Availability Header
        /// </summary>
        Availability,
        /// <summary>
        /// Credits/Ceus Header
        /// </summary>
        Credits,
        /// <summary>
        /// Course Name Header
        /// </summary>
        CourseName,
        /// <summary>
        /// Course Title Header
        /// </summary>
        CourseTitle,
        /// <summary>
        /// Course Types Header
        /// </summary>
        CourseTypes,
        /// <summary>
        /// Instrictional Methods Header
        /// </summary>
        InstructionalMethods,
        /// <summary>
        /// Printed comments Header
        /// </summary>
        Comments,
        /// <summary>
        /// Book store link Header
        /// </summary>
        BookstoreLink
    }
}