using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about an application influence (campus visit, brochure, etc)
    /// </summary>
    public class ApplicationInfluence
    {
        /// <summary>
        /// Unique code for this application influence
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this application influence
        /// </summary>
        public string Description { get; set; }
    }
}