//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    public interface IVendorContactsRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<OrganizationContact>, int>> GetVendorContactsAsync(int offset, int limit, string vendorId);
        Task<OrganizationContact> GetGetVendorContactsByGuidAsync(string guid);

        Task<string> GetOrganizatonContactIdFromGuidAsync(string guid);
        Task<Tuple<OrganizationContact, string>> CreateVendorContactInitiationProcessAsync(OrganizationContactInitiationProcess entity);

        Task<IEnumerable<OrganizationContact>> GetVendorContactsForVendorsAsync(string[] vendorIds);
    }
}