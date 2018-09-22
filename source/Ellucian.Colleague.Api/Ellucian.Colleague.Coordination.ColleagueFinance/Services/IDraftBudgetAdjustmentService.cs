// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for the Draft budget adjustment service.
    /// </summary>
    public interface IDraftBudgetAdjustmentService
    {
        /// <summary>
        /// Create or update a new draft budget adjustment.
        /// </summary>
        /// <param name="draftBudgetAdjustmentDto">Draft budget adjustment DTO.</param>
        /// <returns>Draft Budget adjustment DTO</returns>
        Task<DraftBudgetAdjustment> SaveDraftBudgetAdjustmentAsync(DraftBudgetAdjustment draftBudgetAdjustmentDto);

        /// <summary>
        /// Get a draft budget adjustment.
        /// </summary>
        /// <param name="id">A draft budget adjustment ID.</param>
        /// <returns>A draft budget adjustment DTO.</returns>
        Task<DraftBudgetAdjustment> GetAsync(string id);

        /// <summary>
        /// Delete a draft budget adjustment.
        /// </summary>
        /// <param name="id">The draft budget adjustment ID to delete.</param>
        /// <returns>Nothing.</returns>
        Task DeleteAsync(string id);
    }
}