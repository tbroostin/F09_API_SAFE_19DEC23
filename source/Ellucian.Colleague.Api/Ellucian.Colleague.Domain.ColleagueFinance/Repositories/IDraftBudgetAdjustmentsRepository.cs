// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Defines methods necessary to read/write draft budget adjustments.
    /// </summary>
    public interface IDraftBudgetAdjustmentsRepository
    {
        /// <summary>
        /// Create or update a draft budget adjustment.
        /// </summary>
        /// <param name="draftBudgetAdjustment">Draft budget adjustment.</param>
        /// <returns></returns>
        Task<DraftBudgetAdjustment> SaveAsync(DraftBudgetAdjustment draftBudgetAdjustment);

        /// <summary>
        /// Get the draft budget adjustment for the specified ID.
        /// </summary>
        /// <param name="id">A draft budget adjustment ID.</param>
        /// <returns>A draft budget adjustment domain entity.</returns>
        Task<DraftBudgetAdjustment> GetAsync(string id);

        /// <summary>
        /// Deletes the draft budget adjustment with the specified id.
        /// </summary>
        /// <param name="id">The draft budget adjustment ID to delete.</param>
        /// <returns>Nothing</returns>
        Task DeleteAsync(string id);
    }
}