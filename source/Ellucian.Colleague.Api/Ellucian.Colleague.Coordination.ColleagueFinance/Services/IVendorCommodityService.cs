//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for vendor commodities
    /// </summary>
    public interface IVendorCommodityService : IBaseService
    {
        /// <summary>
        /// Returns a vendor and commodity code association.
        /// </summary>
        /// <param name="vendorId">vendor id.</param>        
        /// <param name="commodityCode">Commodity code.</param>
        /// <returns>VendorCommodities Dto</returns>
        Task<VendorCommodity> GetVendorCommodityAsync(string vendorId, string commodityCode);
    }
}
