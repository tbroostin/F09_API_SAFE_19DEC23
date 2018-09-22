using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a degree such as Bachelor of Arts
    /// </summary>
    public class Degree
    {
        /// <summary>
        /// Unique code for this degree
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this degree
        /// </summary>
        public string Description { get; set; }
    }
}