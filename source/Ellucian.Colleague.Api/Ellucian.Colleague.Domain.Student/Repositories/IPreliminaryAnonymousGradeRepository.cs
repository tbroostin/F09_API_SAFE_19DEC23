// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for preliminary anonymous grade repository
    /// </summary>
    public interface IPreliminaryAnonymousGradeRepository
    {
        /// <summary>
        /// Get preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="primarySectionId">ID of the course section for which to retrieve preliminary anonymous grade information</param>
        /// <param name="crossListedSectionIds">IDs of any crosslisted course sections for which to retrieve preliminary anonymous grade information</param>
        /// <returns>Preliminary anonymous grade information for the specified course section</returns>
        /// <exception cref="ArgumentNullException">A course section ID is required when retrieving preliminary anonymous grade information.</exception>
        /// <exception cref="ConfigurationException">Academic record configuration from AC.DEFAULTS is null.</exception>
        /// <exception cref="ConfigurationException">Generate Random IDs field from ACPR is not set. In order to retrieve preliminary anonymous grade information, ACPR > Generate Random IDs must be set to either (S)ection or (T)erm.</exception>
        /// <exception cref="ColleagueException">An error occurred while building preliminary anonymous grades for course section: ACPR > Generate Random IDs is set to (T)erm but no STUDENT.TERMS records were found for section's associated STUDENT.COURSE.SEC / STUDENT.ACAD.CRED records.</exception>
        Task<SectionPreliminaryAnonymousGrading> GetPreliminaryAnonymousGradesBySectionIdAsync(string primarySectionId, IEnumerable<string> crossListedSectionIds);

        /// <summary>
        /// Update preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="sectionId">ID of the course section for which to process preliminary anonymous grade updates</param>
        /// <param name="preliminaryAnonymousGrades">Preliminary anonymous grade updates to process</param>
        /// <returns>Preliminary anonymous grade update results</returns>
        /// <exception cref="ArgumentNullException">At least one preliminary anonymous grade is required when updating preliminary anonymous grade information.</exception>
        Task<IEnumerable<PreliminaryAnonymousGradeUpdateResult>> UpdatePreliminaryAnonymousGradesBySectionIdAsync(string sectionId, IEnumerable<PreliminaryAnonymousGrade> preliminaryAnonymousGrades);
    }
}
