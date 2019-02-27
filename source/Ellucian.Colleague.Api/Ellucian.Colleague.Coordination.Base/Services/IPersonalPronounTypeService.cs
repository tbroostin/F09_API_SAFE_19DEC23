// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonalPronounTypeService : IBaseService
    {
        
        /// <summary>
        /// Get personal pronoun types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.PersonalPronounType">PersonalPronounType</see> items consisting of code and description</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonalPronounType>> GetBasePersonalPronounTypesAsync(bool ignoreCache);

        /// <summary>
        /// Gets all personal-pronouns
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonalPronouns">personalPronouns</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.PersonalPronouns>> GetPersonalPronounsAsync(bool bypassCache = false);

        /// <summary>
        /// Get a personalPronouns by guid.
        /// </summary>
        /// <param name="guid">Guid of the personalPronouns in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PersonalPronouns">personalPronouns</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonalPronouns> GetPersonalPronounsByGuidAsync(string guid, bool bypassCache = true);

    }
}
