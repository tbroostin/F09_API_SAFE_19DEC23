// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for Currency Repository
    /// </summary>
    public interface ICurrencyRepository : IEthosExtended
    {

        /// <summary>
        /// CurrencyConv with guid.
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        Task<IEnumerable<CurrencyConv>> GetCurrencyConversionAsync(bool ignoreCache);

        /// <summary>
        /// UpdateCurrencyAsync
        /// </summary>
        /// <param name="currency">CurrencyConv domain entity to be updated</param>
        /// <returns>updated CurrencyConv domain entity</returns>
        Task<CurrencyConv> UpdateCurrencyConversionAsync(CurrencyConv currency);


        /// <summary>
        /// Get Currency by guid with enhanced error collections
        /// </summary>
        /// <param name="guid">CurrencyConv id</param>
        /// <returns>CurrencyConv Entity</returns>
        Task<CurrencyConv> GetCurrencyConversionByGuidAsync(string guid);

        /// <summary>
        /// Get a collection of IntgIsoCurrencyCodes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgIsoCurrencyCodes</returns>
        Task<IEnumerable<IntgIsoCurrencyCodes>> GetIntgIsoCurrencyCodesAsync(bool ignoreCache);


    }
}