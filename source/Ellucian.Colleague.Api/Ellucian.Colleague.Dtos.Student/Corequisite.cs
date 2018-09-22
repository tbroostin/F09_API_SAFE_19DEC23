using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Specified credit that must be obtained at the same time (or prior to) the requesting course or section.
    /// </summary>
    public class Corequisite
    {
        /// <summary>
        /// Unique corequisite Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Indicates whether this corequisite is required. If required, registration may be prevented unless the corequisite is in place.
        /// </summary>
        public bool Required { get; set; }
    }
}
