using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Application Status (Accepted, Early Accept, Provisional Accept, Applied, Deferred, etc)
    /// </summary>>
    public class ApplicationStatus
    {
        /// <summary>
        /// Unique code for this application status
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Application status description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Application status special processing code
        /// </summary>
        public string SpecialProcessingCode { get; set; }
    }
}