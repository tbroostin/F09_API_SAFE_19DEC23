// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains INCOMING course search/filter request
    /// Search Criteria will only work on sections that are valid for Instant Enrollment. 
    /// If keyword is provided then only keyword will imply to sections for Instant Enrollment and not the whole course catalog.
    /// If SectionIds are provided then only those sections will be filtered which intersect with valid sections for Instant Enrollment.
    /// Once the search is performed the other attributes of this DTO are used to filter the results.
    /// All other attributes may be optionally used to filter the results of the search.
    /// </summary>
    public class InstantEnrollmentCourseSearchCriteria
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public InstantEnrollmentCourseSearchCriteria()
        {
        }

        /// <summary>
        /// Course search first determines if there is a search keyword provided. If there is a keyword provided, the search will
        /// be performed for that keyword.
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// DaysOfWeek Filter: Used to filter returned results to specific days of the week.
        /// </summary>
        public IEnumerable<string> DaysOfWeek { get; set; }

        /// <summary>
        /// Location Filter: Used to filter returned results to specific location codes.
        /// </summary>
        public IEnumerable<string> Locations { get; set; }

        /// <summary>
        ///  Faculty Filter: Used to filter returned results based on specific faculty names.
        /// </summary>
        public IEnumerable<string> Faculty { get; set; }

       
        /// <summary>
        /// Earliest Time Filter: Used to filter returned results based on meeting start times.
        /// </summary>
        public int EarliestTime { get; set; }

        /// <summary>
        /// Latest Time Filter: Used to filter returned results based on meeting end times.
        /// </summary>
        public int LatestTime { get; set; }

        /// <summary>
        /// Course Topic Filter: Used to filter returned results based on course topic codes
        /// </summary>
        public IEnumerable<string> TopicCodes { get; set; }

        /// <summary>
        /// Course Term Filter: Used to filter returned results based on course term codes
        /// </summary>
        public IEnumerable<string> Terms { get; set; }

        /// <summary>
        /// If keyword is not provided then Search will check whether search for Section Ids is requested. 
        /// These sections can only be valid sections for Instant Enrollment.
        /// </summary>
        public IEnumerable<string> SectionIds { get; set; }
        /// <summary>
        /// If keyword is not provided and section ids are not provided then course search will determine if specific course Ids have been provided.
        /// If so it will perform the search based on the course Ids provided.
        /// </summary>
        public IEnumerable<string> CourseIds { get; set; }

        /// <summary>
        /// Online Categories Filter: Indicates whether to filter sections on the basis of online category. The possible values are:
        ///    Online: Selects sections that have only online instruction
        ///    NotOnline: Selects sections that have no online instruction
        ///    Hybrid: Selects sections that have some instruction that is online and some instruction that is not online
        /// </summary>
        public IEnumerable<string> OnlineCategories { get; set; }

        /// <summary>
        /// If provided, it represents the earliest first meeting date for qualifying sections. Any section with a first meeting date earlier than this will not be included.
        /// Note: First meeting date may differ from the section start date.
        /// </summary>
        public DateTime? SectionStartDate { get; set; }

        /// <summary>
        /// If provided, this indicates the latest last meeting date for qualifying sections. Any section with an last meeting date that is greater than or equal to this date will not be included.
        /// </summary>
        public DateTime? SectionEndDate { get; set; }

        /// <summary>
        /// Subject Filter: Used to filter returned results to specific subjects.
        /// </summary>
        public IEnumerable<string> Subjects { get; set; }

        /// <summary>
        /// Synonym Filter: Used to filter returned results to specific Synonyms.
        /// </summary>
        public IEnumerable<string> Synonyms { get; set; }

        /// <summary>
        /// display sections that are open Only - have seats available
        /// </summary>
        public bool OpenSections { get; set; }

        /// <summary>
        /// display sections that are open and waitlisted - have seats available
        /// </summary>
        public bool OpenAndWaitlistSections { get; set; }
    }
}
