// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a vendorcommodities repository
    /// </summary>
    public interface IVendorCommoditiesRepository
    {
        /// <summary>
        /// standard price for given commodity code & vendor
        /// </summary>
        /// <param name="vendorId">vendor id</param>
        /// <param name="commodityCode">commodity code</param>
        /// <returns>standard price</returns>
        Task<decimal?> GetVendorCommodityPriceAsync(string vendorId, string commodityCode);

    }
}

