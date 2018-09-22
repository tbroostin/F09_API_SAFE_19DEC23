//Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPersonBeneficiariesRepository
    {


        /// <summary>
        /// Get a collection of PersonBeneficiary
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of PersonBeneficiary</returns>
        Task<Tuple<IEnumerable<PersonBeneficiary>, int>> GetPersonBeneficiariesAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Returns a review for a specified Employment Performance Reviews key.
        /// </summary>
        /// <param name="ids">Key to Employment Performance Reviews to be returned</param>
        /// <returns>PersonBeneficiary Objects</returns>
        Task<PersonBeneficiary> GetPersonBeneficiaryByIdAsync(string id);

        ///// <summary>
        ///// Get a collection of PersonBeneficiary
        ///// </summary>
        ///// <param name="ignoreCache">Bypass cache flag</param>
        ///// <returns>Collection of PersonBeneficiary</returns>
        //Task<IEnumerable<PersonBeneficiary>> GetPersonBeneficiariesAsync(bool bypassCache = false);
        
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
