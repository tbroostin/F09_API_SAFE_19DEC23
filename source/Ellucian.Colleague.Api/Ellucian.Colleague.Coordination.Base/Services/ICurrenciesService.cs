//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Currencies services
    /// </summary>
    public interface ICurrenciesService : IBaseService
    {
        /// <summary>
        /// Gets all currencies
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Currencies">currencies</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Currencies>> GetCurrenciesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a currencies by guid.
        /// </summary>
        /// <param name="guid">Guid of the currencies in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="Currencies">currencies</see></returns>
        Task<Ellucian.Colleague.Dtos.Currencies> GetCurrenciesByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Get a currencies by guid.
        /// </summary>
        /// <param name="guid">Guid of the currencies in Colleague.</param>
        /// <param name="currencies">currencies dto to update</param>
        /// <returns>The <see cref="Currencies">updated currencies</see></returns>
        Task<Ellucian.Colleague.Dtos.Currencies> PutCurrenciesAsync(string guid, Dtos.Currencies currencies);

        /// <summary>
        /// Gets all currency-iso-codes
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CurrencyIsoCodes">currencyIsoCodes</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CurrencyIsoCodes>> GetCurrencyIsoCodesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a currencyIsoCodes by guid.
        /// </summary>
        /// <param name="guid">Guid of the currencyIsoCodes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CurrencyIsoCodes">currencyIsoCodes</see></returns>
        Task<Ellucian.Colleague.Dtos.CurrencyIsoCodes> GetCurrencyIsoCodesByGuidAsync(string guid, bool bypassCache = true);

    }
}