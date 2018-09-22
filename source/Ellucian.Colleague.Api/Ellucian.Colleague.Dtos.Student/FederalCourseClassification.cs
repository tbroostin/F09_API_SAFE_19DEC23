using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// An institutionally-defined Federal Course Classification
    /// </summary>
    public class FederalCourseClassification
    {
        /// <summary>
        /// Unique ID for this Course Classification
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of Course Classification
        /// </summary>
        public string Description { get; set; }
    }
}
