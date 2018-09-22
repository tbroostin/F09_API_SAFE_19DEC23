using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A page of courses displayed in the course search results
    /// <see cref="CourseSearch"/>
    /// <see cref="DataPage{T}"/>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
    public class CoursePage : DataPage<CourseSearch>
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public CoursePage()
            : base()
        {

        }

        /// <summary>
        /// CoursePage constructor initializes the basic page data
        /// </summary>
        /// <param name="allItems">List of <see cref="CourseSearch">CourseSearch</see> items</param>
        /// <param name="pageSize">Size of page to construct</param>
        /// <param name="pageIndex">Page number</param>
        public CoursePage(IEnumerable<CourseSearch> allItems, int pageSize, int pageIndex)
            : base(allItems, pageSize, pageIndex)
        {

        }
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Subjects
        /// </summary>
        public IEnumerable<Filter> Subjects = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for AcademicLevel
        /// </summary>
        public IEnumerable<Filter> AcademicLevels = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for CourseLevel
        /// </summary>
        public IEnumerable<Filter> CourseLevels = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for DaysOfWeek
        /// </summary>
        public IEnumerable<Filter> DaysOfWeek = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Location
        /// </summary>
        public IEnumerable<Filter> Locations = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Faculty
        /// </summary>
        public IEnumerable<Filter> Faculty = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for CourseType
        /// </summary>
        public IEnumerable<Filter> CourseTypes = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for TopicCodes
        /// </summary>
        public IEnumerable<Filter> TopicCodes = new List<Filter>();
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Terms
        /// </summary>
        public IEnumerable<Filter> Terms = new List<Filter>();

        /// <summary>
        /// Start time from midnight (in minutes) of earliest course/section in result set.  9:45am == 585.
        /// </summary>
        public int EarliestTime;

        /// <summary>
        /// End time from midnight (in minutes) of latest course/section in results set. 9:45am == 585.
        /// </summary>
        public int LatestTime;

    }
}
