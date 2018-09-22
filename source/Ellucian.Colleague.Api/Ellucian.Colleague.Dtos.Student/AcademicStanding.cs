using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about an Academic Standing code
    /// </summary>
    public class AcademicStanding
    {
        /// <summary>
        /// Unique system Id for this advisor type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of the Advisor Type
        /// </summary>
        public string Description { get; set; }
    }
}
