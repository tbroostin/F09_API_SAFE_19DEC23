using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about an external transcript status (repeated, withdrawn, etc)
    /// </summary>
    public class ExternalTranscriptStatus
    {
        /// <summary>
        /// Unique code for this external transcript status
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this external transcript status
        /// </summary>
        public string Description { get; set; }
    }
}