// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student.AnonymousGrading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IPreliminaryAnonymousGradeService
    {
        /// <summary>
        /// Get preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="sectionId">ID of the course section for which to retrieve preliminary anonymous grade information</param>
        /// <returns>Preliminary anonymous grade information for the specified course section</returns>
        /// <exception cref="ArgumentNullException">A course section ID is required when retrieving preliminary anonymous grade information.</exception>
        Task<SectionPreliminaryAnonymousGrading> GetPreliminaryAnonymousGradesBySectionIdAsync(string sectionId);

        /// <summary>
        /// Update preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="sectionId">ID of the course section for which to process preliminary anonymous grade updates</param>
        /// <param name="preliminaryAnonymousGrades">Preliminary anonymous grade updates to process</param>
        /// <returns>Preliminary anonymous grade update results</returns>
        /// <exception cref="ArgumentNullException">A course section ID is required when updating preliminary anonymous grade information.</exception>
        /// <exception cref="ArgumentNullException">At least one preliminary anonymous grade is required when updating preliminary anonymous grade information.</exception>
        Task<IEnumerable<PreliminaryAnonymousGradeUpdateResult>> UpdatePreliminaryAnonymousGradesBySectionIdAsync(string sectionId,
            IEnumerable<PreliminaryAnonymousGrade> preliminaryAnonymousGrades);
    }
}
