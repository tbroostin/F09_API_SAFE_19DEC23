// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Definition of the methods that must be implemented for a requisition service
    /// </summary>
    public interface IRequisitionService : IBaseService
    {
        /// <summary>
        /// Gets all requisitions
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Requisitions">requisitions</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Requisitions>, int>> GetRequisitionsAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a requisition by guid.
        /// </summary>
        /// <param name="guid">Guid of the requisition in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="Requisitions">requisition</see></returns>
        Task<Ellucian.Colleague.Dtos.Requisitions> GetRequisitionsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Update a requisitions.
        /// </summary>
        /// <param name="requisitions">The <see cref="Requisitions">requisitions</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Requisitions">requisitions</see></returns>
        Task<Ellucian.Colleague.Dtos.Requisitions> UpdateRequisitionsAsync(Ellucian.Colleague.Dtos.Requisitions requisitions);

        /// <summary>
        /// Create a requisitions.
        /// </summary>
        /// <param name="requisitions">The <see cref="Requisitions">requisitions</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Requisitions">requisitions</see></returns>
        Task<Ellucian.Colleague.Dtos.Requisitions> CreateRequisitionsAsync(Ellucian.Colleague.Dtos.Requisitions requisitions);

        /// <summary>
        /// Delete a requisitions by guid.
        /// </summary>
        /// <param name="guid">Guid of the requisitions in Colleague.</param>
        Task DeleteRequisitionAsync(string guid);
        
        /// <summary>
        /// Returns a specified requisition from an id
        /// </summary>
        /// <param name="requisitionId">The requested requisition ID</param>
        /// <returns>Requisition DTO</returns>
        Task<Requisition> GetRequisitionAsync(string requisitionId);

       
    }
}
