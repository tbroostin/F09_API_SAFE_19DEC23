// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a vendorcommodities repository
    /// </summary>
    public interface IVendorCommodityRepository
    {
        /// <summary>
        /// returns a vendor and commodity code association
        /// </summary>
        /// <param name="vendorId">vendor id</param>
        /// <param name="commodityCode">commodity code</param>
        /// <returns>VendorCommodity domain entity</returns>
        Task<Domain.ColleagueFinance.Entities.VendorCommodity> GetVendorCommodityAsync(string vendorId, string commodityCode);

    }
}

