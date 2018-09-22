using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a Course Level (such as 100-level)
    /// </summary>
    public class CourseLevel
    {
        /// <summary>
        /// Unique system Id for this course level
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this course level
        /// </summary>
        public string Description { get; set; }
    }
}