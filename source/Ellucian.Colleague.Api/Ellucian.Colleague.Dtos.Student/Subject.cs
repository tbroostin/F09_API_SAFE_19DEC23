using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// An institutionally defined subject used in the course name (e.g., ENGL)
    /// </summary>
    public class Subject
    {
        /// <summary>
        /// Unique code of this subject
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this subject
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Indicates whether to show this subject, or courses and sections related to this subject in the course catalog search
        /// </summary>
        public bool ShowInCourseSearch { get; set; }
    }
}
