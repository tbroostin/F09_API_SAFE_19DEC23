//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for FreeOnBoardTypes services
    /// </summary>
    public interface IFreeOnBoardTypesService: IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.FreeOnBoardTypes>> GetFreeOnBoardTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.FreeOnBoardTypes> GetFreeOnBoardTypesByGuidAsync(string id);
    }
}
