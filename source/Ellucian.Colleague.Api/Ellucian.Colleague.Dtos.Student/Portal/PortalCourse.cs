// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Contains a course that is applicable for update from portal.
    /// </summary>
    public class PortalCourse
    {
        /// <summary>
        /// Course identifier
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Course Short title
        /// </summary>
        public string ShortTitle { get; set; }

        /// <summary>
        /// Course title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Course description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Course subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Course section departments
        /// </summary>
        public List<string> Departments { get; set; }

        /// <summary>
        /// Course number of the section
        /// </summary>
        public string CourseNumber { get; set; }

        /// <summary>
        /// Course section acad level
        /// </summary>
        public string AcademicLevel { get; set; }

        /// <summary>
        /// Course name
        /// </summary>
        public string CourseName { get; set; }

        /// <summary>
        /// List of course types
        /// </summary>
        public List<string> CourseTypes { get; set; }

        /// <summary>
        /// Course prerequisite text
        /// </summary>
        public string PrerequisiteText { get; set; }

        /// <summary>
        /// Course locations
        /// </summary>
        public List<string> Locations { get; set; }

        /// <summary>
        /// Portal Course constructor
        /// </summary>
        public PortalCourse()
        {
            Departments = new List<string>();
            CourseTypes = new List<string>();
            Locations = new List<string>();
        }
    }
}
