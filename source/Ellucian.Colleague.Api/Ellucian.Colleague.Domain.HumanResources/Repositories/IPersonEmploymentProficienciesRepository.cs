/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPersonEmploymentProficienciesRepository
    {
        /// <summary>
        /// Get all HR.IND.SKILL records through paging and convert it to a PersonEmploymentProficiency entities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<PersonEmploymentProficiency>, int>> GetPersonEmploymentProficienciesAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a specific HR.IND.SKILL and convert it to a PersonEmploymentProficiency entity
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<PersonEmploymentProficiency> GetPersonEmploymentProficiency(string guid);

        /// <summary>
        /// Get a specific GUID from a Record Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string> GetGuidFromID(string key, string entity);

        /// <summary>
        /// Get GUID information for a single GUID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Guid Lookup Result</returns>
        Task<GuidLookupResult> GetInfoFromGuidAsync(string id);
    }
}
