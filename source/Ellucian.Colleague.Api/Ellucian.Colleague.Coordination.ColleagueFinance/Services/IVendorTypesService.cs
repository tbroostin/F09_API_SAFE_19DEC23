// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using System;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface to VendorTypes service
    /// </summary>
    public interface IVendorTypesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.VendorType>> GetVendorTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.VendorType> GetVendorTypeByIdAsync(string id);
    }
}
