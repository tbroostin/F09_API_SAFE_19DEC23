using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information for instructional methods (e.g., Lecture, Lab)
    /// </summary>
    public class InstructionalMethod
    {
        /// <summary>
        /// Unique code for this instructional method
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this instructional method
        /// </summary>
        public string Description { get; set; }
    }
}
