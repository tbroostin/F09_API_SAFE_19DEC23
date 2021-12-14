// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for Institution Repository
    /// </summary>
    public interface IInstitutionRepository 
    {
        /// <summary>
        /// Get all Institution data
        /// </summary>
        /// <returns>List of Institution Objects</returns>
        IEnumerable<Institution> Get();

        /// <summary>
        /// GetInstitutionAsync
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="instType"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Institution>, int>> GetInstitutionAsync(int offset, int limit, InstType? instType = null, List<Tuple<string, string>> creds = null);

        /// <summary>
        /// GetInstitutionByGuidAsync
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<Institution> GetInstitutionByGuidAsync(string guid);

        /// <summary>
        /// GetInstitutionByIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Institution> GetInstitutionAsync(string id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subList"></param>
        /// <returns></returns>
        Task<IEnumerable<Institution>> GetInstitutionsFromListAsync(string[] subList);

        /// <summary>
        /// Using a list of person ids, determine which ones are associated with an institution
        /// </summary>
        /// <param name="subList">list of person ids</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetInstitutionIdsFromListAsync(string[] subList);

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetInstitutionFromGuidAsync(string guid);

    }
}