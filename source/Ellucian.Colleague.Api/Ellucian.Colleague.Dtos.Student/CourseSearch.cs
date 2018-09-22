using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A course and sections found as a result of search and filter. <see cref="Course"/>
    /// </summary>
    public class CourseSearch : Course
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public CourseSearch()
            : base()
        {

        }
        /// <summary>
        /// List of Ids of the sections that were found by the search
        /// </summary>
        public List<string> MatchingSectionIds { get; set; }
    }
}
