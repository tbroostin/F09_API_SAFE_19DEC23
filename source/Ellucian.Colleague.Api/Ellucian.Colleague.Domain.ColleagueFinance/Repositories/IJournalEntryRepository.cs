// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a journal entry repository
    /// </summary>
    public interface IJournalEntryRepository
    {
        /// <summary>
        /// Get a single journal entry
        /// </summary>
        /// <param name="journalEntryId">The journal entry to retrieve</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>A journal entry</returns>
        Task<JournalEntry> GetJournalEntryAsync(string journalEntryId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts);
    }
}
