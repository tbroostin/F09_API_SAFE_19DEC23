//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for LeaveTypes services
    /// </summary>
    public interface ILeaveTypesService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.LeaveTypes>> GetLeaveTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.LeaveTypes> GetLeaveTypesByGuidAsync(string id);
    }
}
