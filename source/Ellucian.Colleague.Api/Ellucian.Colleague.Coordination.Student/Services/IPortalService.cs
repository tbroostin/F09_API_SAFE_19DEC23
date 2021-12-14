// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Student.Portal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IPortalService : IBaseService
    {
        /// <summary>
        /// This syncs deleted courses and returns them for Portal.
        /// </summary>
        Task<PortalDeletedCoursesResult> GetCoursesForDeletionAsync();

        /// <summary>
        /// This syncs updated sections and returns them for Portal.
        /// </summary>
        Task<PortalUpdatedSectionsResult> GetSectionsForUpdateAsync();

        /// <summary>
        /// This syncs deleted course sections and returns them for Portal.
        /// </summary>
        Task<PortalDeletedSectionsResult> GetSectionsForDeletionAsync();

        /// <summary>
        /// This syncs updated courses and returns them for Portal.
        /// </summary>
        Task<PortalUpdatedCoursesResult> GetCoursesForUpdateAsync();

        /// <summary>
        /// Returns event and reminders to be displayed in the Portal for the authenticated user.
        /// </summary>
        /// <param name="criteria">Event and reminder selection criteria</param>
        /// <returns>A <see cref="PortalEventsAndReminders"/> object</returns>
        Task<PortalEventsAndReminders> GetEventsAndRemindersAsync(PortalEventsAndRemindersQueryCriteria criteria);

        /// <summary>
        /// Updates a student's list of preferred course sections
        /// </summary>
        /// <param name="studentId">ID of the student whose list of preferred course sections is being updated</param>
        /// <param name="courseSectionIds">IDs of the course sections to be added to the student's list of preferred course sections</param>
        /// <returns>Collection of <see cref="PortalStudentPreferredCourseSectionUpdateResult"/></returns>
        Task<IEnumerable<PortalStudentPreferredCourseSectionUpdateResult>> UpdateStudentPreferredCourseSectionsAsync(string studentId, IEnumerable<string> courseSectionIds);
    }
}
