// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IRegistrationGroupService
    {
        /// <summary>
        /// Get the section registration date overrides for the specific sections related to the requestor's registration group id.
        /// </summary>
        /// <param name="sectionIds">List of Section IDs for which override dates have been requested.</param>
        /// <param name="considerUsersGroup">This is set to true if the registration group should be considered for dates calculation, otherwise set to false</param>
        /// <returns>Collection of SectionRegistrationDate DTOs</returns>
        Task<IEnumerable<Dtos.Student.SectionRegistrationDate>> GetSectionRegistrationDatesAsync(IEnumerable<string> sectionIds, bool considerUsersGroup = true);
    }
}
