using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a student load (full time, part time, etc)
    /// </summary>
    public class StudentLoad
    {
        /// <summary>
        /// Unique code for this student load
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this student load
        /// </summary>
        public string Description { get; set; }
    }
}