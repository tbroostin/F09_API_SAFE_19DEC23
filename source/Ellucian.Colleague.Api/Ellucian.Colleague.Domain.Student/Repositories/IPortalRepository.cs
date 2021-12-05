// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Portal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for a PortalRepository.
    /// </summary>
    public interface IPortalRepository
    {
        Task<PortalDeletedCoursesResult> GetCoursesForDeletionAsync();

        Task<PortalUpdatedSectionsResult> GetSectionsForUpdateAsync();

        Task<PortalDeletedSectionsResult> GetSectionsForDeletionAsync();

        Task<PortalUpdatedCoursesResult> GetCoursesForUpdateAsync();

        /// <summary>
        /// Returns event and reminder information for the user
        /// </summary>
        /// <param name="personId">ID of the person for whom event and reminder information is being retrieved</param>
        /// <param name="criteria">Event and reminder selection criteria</param>
        /// <returns>A <see cref="PortalEventsAndReminders"/> object</returns>
        Task<PortalEventsAndReminders> GetEventsAndRemindersAsync(string personId, PortalEventsAndRemindersQueryCriteria criteria);

        /// <summary>
        /// Updates a student's list of preferred course sections
        /// </summary>
        /// <param name="studentId">ID of the student whose list of preferred course sections is being updated</param>
        /// <param name="courseSectionIds">IDs of the course sections to be added to the student's list of preferred course sections</param>
        /// <returns>Collection of <see cref="PortalStudentPreferredCourseSectionUpdateResult"/></returns>
        Task<IEnumerable<PortalStudentPreferredCourseSectionUpdateResult>> UpdateStudentPreferredCourseSectionsAsync(string studentId, IEnumerable<string> courseSectionIds);
    }
}
