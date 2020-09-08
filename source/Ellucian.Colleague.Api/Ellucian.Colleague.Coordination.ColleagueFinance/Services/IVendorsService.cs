// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for Vendors services
    /// </summary>
    public interface IVendorsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Vendors>, int>> GetVendorsAsync(int offset, int limit,
            VendorFilter criteriaValues, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.Vendors> GetVendorsByGuidAsync(string id);

        Task<Ellucian.Colleague.Dtos.Vendors> PutVendorAsync(string guid, Ellucian.Colleague.Dtos.Vendors vendor);
        Task<Ellucian.Colleague.Dtos.Vendors> PostVendorAsync(Ellucian.Colleague.Dtos.Vendors vendor);

        /// <summary>
        /// vendors GET all v11
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="vendorDetails"></param>
        /// <param name="classifications"></param>
        /// <param name="status"></param>
        /// <param name="relatedReference"></param>
        /// <param name="vendorType"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Vendors2>, int>> GetVendorsAsync2(int offset, int limit, string vendorDetails, List<string> classifications, 
            List<string> status, List<string> relatedReference, List<string> types = null, string taxId = null, bool bypassCache = false);

        /// <summary>
        /// vendors Get by ID v11
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.Vendors2> GetVendorsByGuidAsync2(string id);

        /// <summary>
        /// vendors PUT v11
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.Vendors2> PutVendorAsync2(string guid, Ellucian.Colleague.Dtos.Vendors2 vendor);

        /// <summary>
        /// vendors POST v11
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.Vendors2> PostVendorAsync2(Ellucian.Colleague.Dtos.Vendors2 vendor);

        /// <summary>
        /// Get the list of vendors based on keyword search.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for vendor search.</param>
        /// <returns> The vendor search results</returns> 
        Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.VendorSearchResult>> QueryVendorsByPostAsync(VendorSearchCriteria searchCriteria);

        /// <summary>
        /// Get the list of vendors for vouchers based on keyword search.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for vendor search.</param>
        /// <returns> The vendor search results for vouchers</returns> 
        Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult>> QueryVendorForVoucherAsync(VendorSearchCriteria searchCriteria);

        /// <summary>
        /// gets vendor default tax for info
        /// </summary>
        /// <param name="vendorId">vendor id</param>
        /// <param name="apType">ap type</param>
        /// <returns>Vendor default tax form info</returns>
        Task<Ellucian.Colleague.Dtos.ColleagueFinance.VendorDefaultTaxFormInfo> GetVendorDefaultTaxFormInfoAsync(string vendorId, string apType);

    }
}
