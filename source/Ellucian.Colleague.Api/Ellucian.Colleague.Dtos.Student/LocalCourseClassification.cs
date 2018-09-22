using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Course Classification for Federal Government ID
    /// </summary>
    public class LocalCourseClassification
    {
        /// <summary>
        /// Local Government Course Classification Key
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of the Classification Code
        /// </summary>
        public string Description { get; set; }
    }
}
