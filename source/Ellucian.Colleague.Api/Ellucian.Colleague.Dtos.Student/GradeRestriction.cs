using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Grade restriction for a student
    /// </summary>
    public class GradeRestriction
    {
        /// <summary>
        /// Indicates whether restriction is in place
        /// </summary>
        public bool IsRestricted { get; set; }
        /// <summary>
        /// Text indicating reason for the restriction
        /// </summary>
        public IEnumerable<string> Reasons { get; set; }
    }
}
