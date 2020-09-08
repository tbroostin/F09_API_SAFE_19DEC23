/*Copyright 2016-2020 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    public interface IVendorsRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<Vendors>, int>> GetVendorsAsync(int offset, int limit, string vendorDetail = "", List<string> classifications = null,
                            List<string> statuses = null, List<string> relatedReference = null, List<string> type = null);
        Task<Tuple<IEnumerable<Vendors>, int>> GetVendors2Async(int offset, int limit, string vendorDetail = "", List<string> classifications = null,
                            List<string> statuses = null, List<string> relatedReference = null, List<string> type = null, string taxId = null);
        Task<Vendors> GetVendorsByGuidAsync(string guid);

        Task<Vendors> GetVendorsByGuid2Async(string guid);

        Task<Vendors> UpdateVendorsAsync(Vendors vendorsEntity);

        Task<Vendors> UpdateVendors2Async(Vendors vendorsEntity);
        Task<Vendors> CreateVendorsAsync(Vendors vendorsEntity);
        Task<Vendors> CreateVendors2Async(Vendors vendorsEntity);

        Task<string> GetVendorGuidFromIdAsync(string id);
        Task<string> GetVendorIdFromGuidAsync(string id);
        Task<Vendors> GetVendorsAsync(string id);
        Task<IEnumerable<VendorSearchResult>> SearchByKeywordAsync(string searchCriteria, string apType);
        Task<IEnumerable<VendorsVoucherSearchResult>> VendorSearchForVoucherAsync(string searchCriteria);
        Task<VendorDefaultTaxFormInfo> GetVendorDefaultTaxFormInfoAsync(string vendorId, string apType);

        Task<Dictionary<string, string>> GetVendorGuidsCollectionAsync(IEnumerable<string> vendorIds);

    }
}