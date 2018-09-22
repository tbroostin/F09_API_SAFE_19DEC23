/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPayScalesRepository
    {
        /// <summary>
        /// Get PayScales objects for all PayScales bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="bypassCache">Bypass the cache if set to true.</param>
        /// <returns>Tuple of PayScale Entity objects <see cref="PayScale"/> and a count for paging.</returns>
        Task<IEnumerable<PayScale>> GetPayScalesAsync(bool bypassCache = false);

        /// <summary>
        /// Get PayScales objects for a specific Id.
        /// </summary>   
        /// <param name="id">guid of the PayScales record.</param>
        /// <returns>PayScale Entity <see cref="PayScale"./></returns>
        Task<PayScale> GetPayScalesByIdAsync(string id);

        /// <summary>
        /// Get Host Country from international parameters
        /// </summary>
        /// <returns>HOST.COUNTRY</returns>
        Task<string> GetHostCountryAsync();
    }
}
