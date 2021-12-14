// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Contains a course section that is applicable for update from portal.
    /// </summary>
    [Serializable]
    public class PortalSection
    {
        /// <summary>
        /// Course section identifier
        /// </summary>
        public string SectionId { get; private set; }

        /// <summary>
        /// Course section title
        /// </summary>
        public string ShortTitle { get; private set; }

        /// <summary>
        /// Course description
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Course section location
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Course section term
        /// </summary>
        public string Term { get; private set; }

        /// <summary>
        /// Course section start date
        /// </summary>
        public DateTime? StartDate { get; private set; }

        /// <summary>
        /// Course section end date
        /// </summary>
        public DateTime? EndDate { get; private set; }

        /// <summary>
        /// Course section meeting information
        /// </summary>
        public List<PortalSectionMeeting> MeetingInformation { get; private set; }

        /// <summary>
        /// Course section capacity
        /// </summary>
        public Nullable<int> Capacity { get; private set; }

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
        /// Course section number
        /// </summary>
        public string SectionNumber { get; private set; }

        /// <summary>
        /// Course section acad level
        /// </summary>
        public string AcademicLevel { get; private set; }

        /// <summary>
        /// Course section synonym
        /// </summary>
        public string Synonym { get; private set; }

        /// <summary>
        /// Course section faculty
        /// </summary>
        public List<string> Faculty { get; private set; }

        /// <summary>
        /// Course section minimum
        /// </summary>
        public decimal? MinimumCredits { get; private set; }

        /// <summary>
        /// Course section maximum
        /// </summary>
        public decimal? MaximumCredits { get; private set; }

        /// <summary>
        /// Course section name
        /// </summary>
        public string SectionName { get; private set; }

        /// <summary>
        /// Course identifier
        /// </summary>
        public string CourseId { get; private set; }

        /// <summary>
        /// Course prerequisite text
        /// </summary>
        public string PrerequisiteText { get; private set; }

        /// <summary>
        /// List of course types
        /// </summary>
        public List<string> CourseTypes { get; private set; }

        /// <summary>
        /// Course section continuing education units
        /// </summary>
        public decimal? ContinuingEducationUnits { get; private set; }

        /// <summary>
        /// Course section printed comments
        /// </summary>
        public string PrintedComments { get; private set; }

        /// <summary>
        /// List of course section book information
        /// </summary>
        public List<PortalBookInformation> BookInformation { get; private set; }

        /// <summary>
        /// Course section total cost of books
        /// </summary>
        public decimal TotalBookCost { get; private set; }

        public PortalSection(string sectionId, string shortTitle, string description, string location, string term,
            DateTime? startDate, DateTime? endDate, List<PortalSectionMeeting> meetingInformation, int? capacity, string subject,
            List<string> departments, string courseNumber, string sectionNumber, string academicLevel, string synonym,
            List<string> faculty, decimal? minimumCredits, decimal? maximumCredits, string sectionName, string courseId,
            string prerequisiteText, List<string> courseTypes, decimal? continuingEducationUnits, string printedComments,
            List<PortalBookInformation> bookInformation, decimal totalBookCost)
        {
            SectionId = sectionId;
            ShortTitle = shortTitle;
            Description = description;
            Location = location;
            Term = term;
            StartDate = startDate;
            EndDate = endDate;
            MeetingInformation = meetingInformation;
            Capacity = capacity;
            Subject = subject;
            Departments = departments;
            CourseNumber = courseNumber;
            SectionNumber = sectionNumber;
            AcademicLevel = academicLevel;
            Synonym = synonym;
            Faculty = faculty;
            MinimumCredits = minimumCredits;
            MaximumCredits = maximumCredits;
            SectionName = sectionName;
            CourseId = courseId;
            PrerequisiteText = prerequisiteText;
            CourseTypes = courseTypes;
            ContinuingEducationUnits = continuingEducationUnits;
            PrintedComments = printedComments;
            BookInformation = bookInformation;
            TotalBookCost = totalBookCost;
        }
    }
}
