// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface ILanguageRepository : IEthosExtended
    {
        /// <summary>
        /// Get a collection of Languages
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Languages</returns>
        Task<IEnumerable<Language2>> GetLanguagesAsync(bool ignoreCache);

        /// <summary>
        /// Get a Language by GUID
        /// </summary>
        /// <param name="id">Language GUID</param>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Language/returns>
        Task<Language2> GetLanguageByGuidAsync(string id, bool ignoreCache);

        /// <summary>
        /// Update a language ISO code by GUID
        /// </summary>
        /// <param name="language">Language object</param>
        /// <returns>Language/returns>
        Task<Language2> UpdateLanguageAsync(Language2 language);
    }
}
