// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for Country Repository
    /// </summary>
    public interface ICountryRepository : IEthosExtended
    {

        /// <summary>
        /// Countries with guid.
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        Task<IEnumerable<Country>> GetCountryCodesAsync(bool ignoreCache);

        /// <summary>
        /// UpdateCountryAsync
        /// </summary>
        /// <param name="country">country domain entity to be updated</param>
        /// <returns>updated country domain entity</returns>
        Task<Country> UpdateCountryAsync(Country country);


        /// <summary>
        /// Get Country by guid with enhanced error collections
        /// </summary>
        /// <param name="guid">Address id</param>
        /// <returns>Address Entity</returns>
        Task<Country> GetCountryByGuidAsync(string guid);

    }
}