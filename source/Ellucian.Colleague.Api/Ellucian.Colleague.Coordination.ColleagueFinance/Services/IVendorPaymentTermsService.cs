//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for VendorPaymentTerms services
    /// </summary>
    public interface IVendorPaymentTermsService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.VendorPaymentTerms>> GetVendorPaymentTermsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.VendorPaymentTerms> GetVendorPaymentTermsByGuidAsync(string id);
    }
}
