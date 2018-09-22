// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface ICommerceTaxCodeService: IBaseService
    {
        Task<IEnumerable<Dtos.CommerceTaxCode>> GetCommerceTaxCodesAsync(bool bypassCache = false);
        Task<Dtos.CommerceTaxCode> GetCommerceTaxCodeByGuidAsync(string guid);
    }
}
