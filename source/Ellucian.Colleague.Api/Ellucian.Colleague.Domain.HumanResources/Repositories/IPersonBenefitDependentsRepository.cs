//Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPersonBenefitDependentsRepository
    {
       

        /// <summary>
        /// Get a collection of PersonBenefitDependent
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of PersonBenefitDependent</returns>
        Task<Tuple<IEnumerable<PersonBenefitDependent>, int>> GetPersonBenefitDependentsAsync(int offset, int limit, bool bypassCache = false);
        
        /// <summary>
        /// Returns a review for a specified Person Benefit Dependents key.
        /// </summary>
        /// <param name="ids">Key to Person Benefit Dependents to be returned</param>
        /// <returns>PersonBenefitDependent Objects</returns>
        Task<PersonBenefitDependent> GetPersonBenefitDependentByIdAsync(string id);

        /// <summary>
        /// Get a specific GUID from a Record Key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<string> GetGuidFromIdAsync(string key, string entity);

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string> GetIdFromGuidAsync(string id);

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<GuidLookupResult> GetInfoFromGuidAsync(string id);

    }
}
