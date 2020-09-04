// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains INCOMING section search/filter request
    /// Search criteria can be 1 of the following: a Keyword or Course Ids or section Ids.
    /// Once the search is performed the other attributes of this DTO are used to filter the results.
    /// All other attributes may be optionally used to filter the results of the search.
    /// </summary>
    public class SectionSearchCriteria
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public SectionSearchCriteria()
        {
        }

        /// <summary>
        /// Section search first determines if there is a search keyword provided. If there is a keyword provided, the search will
        /// be performed for that keyword.
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// If keyword is not provided, search will determine if specific course Ids or section Ids have been provided.
        /// </summary>
        public IEnumerable<string> CourseIds { get; set; }


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
        /// Course Type Filter: Used to filter returned results based on specific course types.
        /// </summary>
        public IEnumerable<string> CourseTypes { get; set; }

        /// <summary>
        /// Course Level Filter: Used to filter returned results based on specific course levels.
        /// </summary>
        public IEnumerable<string> CourseLevels { get; set; }

        /// <summary>
        /// Academic Level Filter: Used to filter returned results based on specific academic levels.
        /// </summary>
        public IEnumerable<string> AcademicLevels { get; set; }

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
        /// Term Filter: Used to filter returned results based on term codes
        /// </summary>
        public IEnumerable<string> Terms { get; set; }

        /// <summary>
        /// If keyword is not provided and course Ids are not provided, search will determine if specific section Ids have been provided.
        /// If keyword is null or empty and there are no course Ids or section Ids then no sections will be returned. 
        /// </summary>
        public IEnumerable<string> SectionIds { get; set; }

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
        /// display sections that are open Only - have seats available
        /// </summary>
        public bool OpenSections { get; set; }

        /// <summary>
        /// display sections that are open and waitlisted - have seats available
        /// </summary>
        public bool OpenAndWaitlistSections { get; set; }
    }
}
