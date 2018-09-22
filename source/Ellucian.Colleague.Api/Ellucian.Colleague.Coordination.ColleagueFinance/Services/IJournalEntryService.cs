// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for Journal Entries
    /// </summary>
    public interface IJournalEntryService
    {
        /// <summary>
        /// Returns the journal entry selected by the user
        /// </summary>
        /// <param name="id">The requested journal entry ID</param>
        /// <returns>Journal Entry DTO</returns>
        Task<JournalEntry> GetJournalEntryAsync(string id);
    }
}
