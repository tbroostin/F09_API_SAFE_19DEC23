using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// An institutionally-defined Student Restriction Types
    /// </summary>
    public class RestrictionType
    {
        /// <summary>
        /// Unique ID for this Restriction
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of Restriction
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Severity
        /// </summary>
        public int? Severity { get; set; }
    }
}
