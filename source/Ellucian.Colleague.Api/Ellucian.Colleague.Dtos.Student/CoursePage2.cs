// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A page of courses displayed in the course search results
    /// <see cref="CourseSearch2"/>
    /// <see cref="DataPage{T}"/>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
    public class CoursePage2 : DataPage<CourseSearch2>
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public CoursePage2()
            : base()
        {

        }

        /// <summary>
        /// CoursePage constructor initializes the basic page data
        /// </summary>
        /// <param name="allItems">List of <see cref="CourseSearch">CourseSearch</see> items</param>
        /// <param name="pageSize">Size of page to construct</param>
        /// <param name="pageIndex">Page number</param>
        public CoursePage2(IEnumerable<CourseSearch2> allItems, int pageSize, int pageIndex)
            : base(allItems, pageSize, pageIndex)
        {

        }
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Subjects
        /// </summary>
        public IEnumerable<Filter> Subjects { get { return subjects; } set { subjects = value; } }
        private IEnumerable<Filter> subjects = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for AcademicLevel
        /// </summary>
        public IEnumerable<Filter> AcademicLevels { get { return academicLevels; } set { academicLevels = value; } }
        private IEnumerable<Filter> academicLevels = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for CourseLevel
        /// </summary>
        public IEnumerable<Filter> CourseLevels { get { return courseLevels; } set { courseLevels = value; } }
        private IEnumerable<Filter> courseLevels = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for DaysOfWeek
        /// </summary>
        public IEnumerable<Filter> DaysOfWeek { get { return daysOfWeek; } set { daysOfWeek = value; } }
        private IEnumerable<Filter> daysOfWeek = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Location
        /// </summary>
        public IEnumerable<Filter> Locations { get { return locations; } set { locations = value; } }
        private IEnumerable<Filter> locations = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Faculty
        /// </summary>
        public IEnumerable<Filter> Faculty { get { return faculty; } set { faculty = value; } }
        private IEnumerable<Filter> faculty = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for CourseType
        /// </summary>
        public IEnumerable<Filter> CourseTypes { get { return courseTypes; } set { courseTypes = value; } }
        private IEnumerable<Filter> courseTypes = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for TopicCodes
        /// </summary>
        public IEnumerable<Filter> TopicCodes { get { return topicCodes; } set { topicCodes = value; } }
        private IEnumerable<Filter> topicCodes = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Terms
        /// </summary>
        public IEnumerable<Filter> Terms { get { return terms; } set { terms = value; } }
        private IEnumerable<Filter> terms = new List<Filter>();

        /// <summary>
        /// Start time from midnight (in minutes) of earliest course/section in result set.  9:45am == 585.
        /// </summary>
        public int EarliestTime { get; set; }

        /// <summary>
        /// End time from midnight (in minutes) of latest course/section in results set. 9:45am == 585.
        /// </summary>
        public int LatestTime { get; set; }

        /// <summary>
        /// Section <see cref="Filter">filter</see> result for OnlineCategory filtering. Possible filter values:
        ///    Online: Sections that have only online instruction
        ///    NotOnline: Sections that have no online instruction
        ///    Hybrid: Sections that have some instruction that is online and some instruction that is not online
        /// </summary>
        public IEnumerable<Filter> OnlineCategories { get { return onlineCategories; } set { onlineCategories = value; } }
        private IEnumerable<Filter> onlineCategories = new List<Filter>();
        /// <summary>
        /// this will control to filter only sections that are open.
        /// open secctions have seats available.
        /// </summary>

        public Filter OpenSections { get { return openSections; } set { openSections = value; } }
        private Filter openSections = new Filter();

        /// <summary>
        /// this will control to filter sections that are open and waitlisted.
        /// open secctions have seats available.
        /// </summary>
        public Filter OpenAndWaitlistSections { get; set; }
    }
}
