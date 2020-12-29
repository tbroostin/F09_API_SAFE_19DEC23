// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface ICommerceTaxCodeService: IBaseService
    {
        /// <summary>
        /// Gets all commerce-tax-codes
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CommerceTaxCode">commerceTaxCode</see> objects</returns>
        Task<IEnumerable<Dtos.CommerceTaxCode>> GetCommerceTaxCodesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a commerceTaxCode by guid.
        /// </summary>
        /// <param name="guid">Guid of the commerceTaxCode in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CommerceTaxCode">commerceTaxCode</see></returns>
        Task<Dtos.CommerceTaxCode> GetCommerceTaxCodeByGuidAsync(string guid);


        /// <summary>
        /// Gets all commerce-tax-code-rates
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CommerceTaxCodeRates">commerceTaxCodeRates</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CommerceTaxCodeRates>> GetCommerceTaxCodeRatesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a commerceTaxCodeRates by guid.
        /// </summary>
        /// <param name="guid">Guid of the commerceTaxCodeRates in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CommerceTaxCodeRates">commerceTaxCodeRates</see></returns>
        Task<Ellucian.Colleague.Dtos.CommerceTaxCodeRates> GetCommerceTaxCodeRatesByGuidAsync(string guid, bool bypassCache = true);

    }
}