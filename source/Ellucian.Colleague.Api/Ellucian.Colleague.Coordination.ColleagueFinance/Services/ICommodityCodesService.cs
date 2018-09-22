// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for CommodityCodes service
    /// </summary>
    public interface ICommodityCodesService: IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityCode>> GetCommodityCodesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.CommodityCode> GetCommodityCodeByIdAsync(string id);
    }
}
