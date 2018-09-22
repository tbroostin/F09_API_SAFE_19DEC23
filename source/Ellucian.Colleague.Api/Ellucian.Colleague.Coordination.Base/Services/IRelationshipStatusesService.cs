//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for RelationshipStatuses services
    /// </summary>
    public interface IRelationshipStatusesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.RelationshipStatuses>> GetRelationshipStatusesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.RelationshipStatuses> GetRelationshipStatusesByGuidAsync(string id);
    }
}
