//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for PayClasses services
    /// </summary>
    public interface IPayClassesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.PayClasses>> GetPayClassesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.PayClasses> GetPayClassesByGuidAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.PayClasses2>> GetPayClasses2Async(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.PayClasses2> GetPayClassesByGuid2Async(string id);
    }
}
