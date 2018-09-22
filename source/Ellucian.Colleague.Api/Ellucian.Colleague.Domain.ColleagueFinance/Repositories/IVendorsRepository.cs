/*Copyright 2016-2017 Ellucian Company L.P. and its affiliates.*/

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
        Task<Vendors> GetVendorsByGuidAsync(string guid);

        Task<Vendors> UpdateVendorsAsync(Vendors vendorsEntity);
        Task<Vendors> CreateVendorsAsync(Vendors vendorsEntity);
       
        Task<string> GetVendorGuidFromIdAsync(string id);
        Task<string> GetVendorIdFromGuidAsync(string id);
        Task<Vendors> GetVendorsAsync(string id);

    }
}