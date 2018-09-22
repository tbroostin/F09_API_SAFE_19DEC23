//Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for VendorHoldReasons services
    /// </summary>
    public interface IVendorHoldReasonsService
    {
        Task<IEnumerable<VendorHoldReasons>> GetVendorHoldReasonsAsync(bool bypassCache = false);
        Task<VendorHoldReasons> GetVendorHoldReasonsByGuidAsync(string id);
    }
}
