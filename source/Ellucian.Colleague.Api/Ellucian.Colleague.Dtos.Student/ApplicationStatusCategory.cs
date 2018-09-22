using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Application status categories (Accepted, Applied, Waitlisted, etc.)
    /// </summary>
    public class ApplicationStatusCategory
    {
        /// <summary>
        /// Unique code for this application status category
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this application status category
        /// </summary>
        public string Description { get; set; }
    }
}