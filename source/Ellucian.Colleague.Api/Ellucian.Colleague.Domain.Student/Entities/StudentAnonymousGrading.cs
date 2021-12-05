// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student anonymous grading information
    /// </summary>
    [Serializable]
    public class StudentAnonymousGrading
    {
        /// <summary>
        /// Grading Id for student and term or course section.
        /// </summary>
        public string AnonymousGradingId { get; private set; }

        /// <summary>
        /// ID of the academic term to which the anonymous grading ID applies.
        /// </summary>
        public string TermId { get; private set; }

        /// <summary>
        /// ID of the course section to which the anonymous grading ID applies.
        /// </summary>
        public string SectionId { get; private set; }

        /// <summary>
        /// Informational message about the blind grading ID, if necessary.
        /// </summary>
        public string Message { get; private set; }

        public StudentAnonymousGrading(string anonymousGradingId, string termId, string sectionId = null, string message = null)
        {
            if (string.IsNullOrEmpty(anonymousGradingId) && string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("anonymousGradingId", "either anonymousGradingId or a message is required");
            }

            if (string.IsNullOrEmpty(termId) && string.IsNullOrEmpty(sectionId) && string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("termId", "either a termId or sectionId or a message is required");
            }

            AnonymousGradingId = anonymousGradingId;
            TermId = termId;
            SectionId = sectionId;
            Message = message;
        }

    }
}
