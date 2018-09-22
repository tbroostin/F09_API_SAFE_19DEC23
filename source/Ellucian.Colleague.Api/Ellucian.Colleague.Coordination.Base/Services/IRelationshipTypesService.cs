//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for RelationshipTypes services
    /// </summary>
    public interface IRelationshipTypesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.RelationshipTypes>> GetRelationshipTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.RelationshipTypes> GetRelationshipTypesByGuidAsync(string id);
    }
}
