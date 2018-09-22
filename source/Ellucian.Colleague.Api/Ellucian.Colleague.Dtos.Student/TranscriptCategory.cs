using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a transcript category (major course, college prep, etc)
    /// </summary>
    public class TranscriptCategory
    {
        /// <summary>
        /// Unique code for this transcript category
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this transcript category
        /// </summary>
        public string Description { get; set; }
    }
}