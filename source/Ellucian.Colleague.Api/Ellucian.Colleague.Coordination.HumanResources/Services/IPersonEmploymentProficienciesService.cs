//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for personEmploymentProficiencies service
    /// </summary>
    public interface IPersonEmploymentProficienciesService: IBaseService
    {
        /// <summary>
        /// Get all personal employment proficiencies records by paging
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<PersonEmploymentProficiencies>, int>> GetPersonEmploymentProficienciesAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get person employment proficiency by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<PersonEmploymentProficiencies> GetPersonEmploymentProficienciesByGuidAsync(string guid);
    }
}
