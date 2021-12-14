// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.AnonymousGrading
{
    /// <summary>
    /// Container for preliminary anonymous grading information for a course section
    /// </summary>
    public class SectionPreliminaryAnonymousGrading
    {
        /// <summary>
        /// Course section ID
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Collection of preliminary anonymous grades for the course section
        /// </summary>
        public IEnumerable<PreliminaryAnonymousGrade> AnonymousGradesForSection { get; set; }

        /// <summary>
        /// Collection of preliminary anonymous grades for any crosslisted course sections
        /// </summary>
        public IEnumerable<PreliminaryAnonymousGrade> AnonymousGradesForCrosslistedSections { get; set; }

       /// <summary>
        /// Collection of preliminary anonymous grades for any crosslisted course sections
        /// </summary>
        public IEnumerable<AnonymousGradeError> Errors { get; set; }
    }
}
