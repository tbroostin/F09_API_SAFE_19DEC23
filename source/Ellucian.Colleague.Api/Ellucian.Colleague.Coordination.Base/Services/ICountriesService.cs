//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Countries services
    /// </summary>
    public interface ICountriesService : IBaseService
    {
        /// <summary>
        /// Gets all countries
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Countries">countries</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Countries>> GetCountriesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a countries by guid.
        /// </summary>
        /// <param name="guid">Guid of the countries in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="Countries">countries</see></returns>
        Task<Ellucian.Colleague.Dtos.Countries> GetCountriesByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Get a countries by guid.
        /// </summary>
        /// <param name="guid">Guid of the countries in Colleague.</param>
        /// <param name="countries">countries dto to update</param>
        /// <returns>The <see cref="Countries">updated countries</see></returns>
        Task<Ellucian.Colleague.Dtos.Countries> PutCountriesAsync(string guid, Dtos.Countries countries);

        /// <summary>
        /// Gets all country-iso-codes
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CountryIsoCodes">countryIsoCodes</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CountryIsoCodes>> GetCountryIsoCodesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a countryIsoCodes by guid.
        /// </summary>
        /// <param name="guid">Guid of the countryIsoCodes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CountryIsoCodes">countryIsoCodes</see></returns>
        Task<Ellucian.Colleague.Dtos.CountryIsoCodes> GetCountryIsoCodesByGuidAsync(string guid, bool bypassCache = true);


    }
}