using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information used to determine if a student should be prevented from seeing or requesting their transcript.
    /// </summary>
    public class TranscriptAccess
    {
        /// <summary>
        /// A flag to used to determine if transcript restrictions should prevent a student from seeing or requesting their transcript.
        /// </summary>
        public bool EnforceTranscriptRestriction { get; set; }

        /// <summary>
        /// A list of restrictions that may prevent the student from seeing or requesting their transcript.
        /// </summary>
        public List<TranscriptRestriction> TranscriptRestrictions { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TranscriptAccess()
        {
            TranscriptRestrictions = new List<TranscriptRestriction>();
        }

    }
}
