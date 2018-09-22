using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information for a minor, or area of study (e.g., Math, Art History)
    /// </summary>
    public class Minor
    {
        /// <summary>
        /// Unique code for this minor
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this minor
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Federal Course Classification assigned to this Minor Code
        /// </summary>
        public string FederalCourseClassification { get; set; }
    }
}
