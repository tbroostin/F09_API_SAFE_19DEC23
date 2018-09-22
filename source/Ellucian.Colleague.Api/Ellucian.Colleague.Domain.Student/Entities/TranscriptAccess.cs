using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class TranscriptAccess
    {
        /// <summary>
        /// A flag to used to determine if transcript restrictions should prevent a student from seeing or requesting their transcript.
        /// </summary>
        public bool EnforceTranscriptRestriction { get; private set; }

        /// <summary>
        /// The restrictions that may prevent the student from seeing or requesting their transcript.
        /// </summary>
        private readonly List<TranscriptRestriction> _transcriptRestrictions = new List<TranscriptRestriction>();
        public ReadOnlyCollection<TranscriptRestriction> TranscriptRestrictions { get; private set; }

        /// <summary>
        /// Constructor for the TranscriptAccess
        /// </summary>
        /// <param name="enforceTranscriptRestriction"> A flag to used to determine if transcript restrictions should be enforced.</param>
        public TranscriptAccess(bool enforceTranscriptRestriction)
        {
            EnforceTranscriptRestriction = enforceTranscriptRestriction;
            TranscriptRestrictions = _transcriptRestrictions.AsReadOnly();
        }

        /// <summary>
        /// Method to add a transcript restriction to the transcript restrictions collection
        /// </summary>
        /// <param name="studentPetition">The student petition to add.</param>
        public void AddTranscriptRestriction(TranscriptRestriction transcriptRestriction)
        {
            if (transcriptRestriction != null)
            {
                _transcriptRestrictions.Add(transcriptRestriction);
            }
        }
    }
}
