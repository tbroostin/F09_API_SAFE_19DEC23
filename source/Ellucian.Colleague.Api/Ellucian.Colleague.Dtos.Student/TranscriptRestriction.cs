using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A restriction that prevents the student from seeing or requesting their transcript.  Can be related
    /// to a StudentRestriction but is activated by a separate set of rules within Colleague.
    /// </summary>
    public class TranscriptRestriction
    {
        /// <summary>
        /// Restriction Code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Restriction Description
        /// </summary>
        public string Description { get; set; }
    }
}