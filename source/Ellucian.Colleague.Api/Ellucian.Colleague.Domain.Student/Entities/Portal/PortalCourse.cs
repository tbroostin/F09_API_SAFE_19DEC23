// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Contains a course that is applicable for update from portal.
    /// </summary>
    [Serializable]
    public class PortalCourse
    {
        /// <summary>
        /// Course identifier
        /// </summary>
        public string CourseId { get; private set; }

        /// <summary>
        /// Course Short title
        /// </summary>
        public string ShortTitle { get; private set; }

        /// <summary>
        /// Course title
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Course description
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Course subject
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// Course section departments
        /// </summary>
        public List<string> Departments { get; private set; }

        /// <summary>
        /// Course number of the section
        /// </summary>
        public string CourseNumber { get; private set; }

        /// <summary>
        /// Course section acad level
        /// </summary>
        public string AcademicLevel { get; private set; }

        /// <summary>
        /// Course name
        /// </summary>
        public string CourseName { get; private set; }

        /// <summary>
        /// List of course types
        /// </summary>
        public List<string> CourseTypes { get; private set; }

        /// <summary>
        /// Course prerequisite text
        /// </summary>
        public string PrerequisiteText { get; private set; }

        /// <summary>
        /// Course locations
        /// </summary>
        public List<string> Locations { get; private set; }

        public PortalCourse(string courseId, string shortTitle, string title, string description, 
            string subject, List<string> departments, string courseNumber, string academicLevel, 
            string courseName, List<string> courseTypes, string prerequisiteText, List<string> locations)
        {
            CourseId = courseId;
            ShortTitle = shortTitle;
            Title = title;
            Description = description;
            Subject = subject;
            Departments = departments;
            CourseNumber = courseNumber;
            AcademicLevel = academicLevel;
            CourseName = courseName;
            CourseTypes = courseTypes;
            PrerequisiteText = prerequisiteText;
            Locations = locations;
        }

    }
}
