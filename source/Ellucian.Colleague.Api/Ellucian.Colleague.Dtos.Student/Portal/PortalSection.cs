// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Contains a course section that is applicable for update from portal.
    /// </summary>
    public class PortalSection
    {
        /// <summary>
        /// Course section identifier
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Course section title
        /// </summary>
        public string ShortTitle { get; set; }

        /// <summary>
        /// Course description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Course section location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Course section term
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Course section start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Course section end date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Course section meeting information
        /// </summary>
        public List<PortalSectionMeeting> MeetingInformation { get; set; }

        /// <summary>
        /// Course section capacity
        /// </summary>
        public Nullable<int> Capacity { get; set; }

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
        /// Course section number
        /// </summary>
        public string SectionNumber { get; set; }

        /// <summary>
        /// Course section acad level
        /// </summary>
        public string AcademicLevel { get; set; }

        /// <summary>
        /// Course section synonym
        /// </summary>
        public string Synonym { get; set; }

        /// <summary>
        /// Course section faculty
        /// </summary>
        public List<string> Faculty { get; set; }

        /// <summary>
        /// Course section minimum
        /// </summary>
        public decimal? MinimumCredits { get; set; }

        /// <summary>
        /// Course section maximum
        /// </summary>
        public decimal? MaximumCredits { get; set; }

        /// <summary>
        /// Course section name
        /// </summary>
        public string SectionName { get; set; }

        /// <summary>
        /// Course identifier
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Course prerequisite text
        /// </summary>
        public string PrerequisiteText { get; set; }

        /// <summary>
        /// List of course types
        /// </summary>
        public List<string> CourseTypes { get; set; }

        /// <summary>
        /// Course section continuing education units
        /// </summary>
        public decimal? ContinuingEducationUnits { get; set; }

        /// <summary>
        /// Course section printed comments
        /// </summary>
        public string PrintedComments { get; set; }

        /// <summary>
        /// List of course section book information
        /// </summary>
        public List<PortalBookInformation> BookInformation { get; set; }

        /// <summary>
        /// Course section total cost of books
        /// </summary>
        public decimal TotalBookCost { get; set; }

        /// <summary>
        /// Portal Section constructor
        /// </summary>
        public PortalSection()
        {
            MeetingInformation = new List<PortalSectionMeeting>();
            Departments = new List<string>();
            Faculty = new List<string>();
            CourseTypes = new List<string>();
            BookInformation = new List<PortalBookInformation>();
        }

    }
}
