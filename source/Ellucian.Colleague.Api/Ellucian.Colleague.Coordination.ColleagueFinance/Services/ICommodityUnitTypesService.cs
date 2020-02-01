// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for CommodityUnitTypes service
    /// </summary>
    public interface ICommodityUnitTypesService: IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityUnitType>> GetCommodityUnitTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.CommodityUnitType> GetCommodityUnitTypeByIdAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityUnitType>> GetAllCommodityUnitTypesAsync();
    }
}
