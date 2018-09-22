// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a blanket purchase order repository
    /// </summary>
    public interface IBlanketPurchaseOrderRepository
    {
        /// <summary>
        /// Get a single blanket purchase order
        /// </summary>
        /// <param name="blanketPurchaseOrderId">The blanket purchase order to retrieve</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>Blanket purchase order domain entity</returns>
        Task<BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string blanketPurchaseOrderId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts);
    }
}
