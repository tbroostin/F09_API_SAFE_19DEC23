// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a general ledger transaction repository
    /// </summary>
    public interface IGeneralLedgerTransactionRepository : IEthosExtended
    {
        /// <summary>
        /// Get a single general ledger transaction
        /// </summary>
        /// <param name="id">The general ledger transaction GUID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A general ledger transaction entity</returns>
        Task<GeneralLedgerTransaction> GetByIdAsync(string id, string personId, GlAccessLevel glAccessLevel);

        Task<GeneralLedgerTransaction> GetById2Async(string id, string personId, GlAccessLevel glAccessLevel);

        /// <summary>
        /// Returns all general ledger transactions for the data model version 6
        /// </summary>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>Collection of GeneralLedgerTransactions</returns>
        Task<IEnumerable<GeneralLedgerTransaction>> GetAsync(string personId, GlAccessLevel glAccessLevel);

        /// <summary>
        /// Returns all general ledger transactions for the data model version 12
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="glAccessLevel"></param>
        /// <returns></returns>
        Task<IEnumerable<GeneralLedgerTransaction>> Get2Async(string personId, GlAccessLevel glAccessLevel);
        /// <summary>
        /// Update a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="id">The general ledger transaction GUID</param>
        /// <param name="generalLedgerTransaction">General Ledger Transaction to update</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        Task<GeneralLedgerTransaction> UpdateAsync(string id, GeneralLedgerTransaction generalLedgerTransaction, string personId, GlAccessLevel glAccessLevel, GeneralLedgerAccountStructure GlConfig);

        /// <summary>
        /// Update a single general ledger transaction for the data model version 12
        /// </summary>
        /// <param name="id"></param>
        /// <param name="generalLedgerTransaction"></param>
        /// <param name="personId"></param>
        /// <param name="glAccessLevel"></param>
        /// <param name="GlConfig"></param>
        /// <returns></returns>
        Task<GeneralLedgerTransaction> Update2Async(string id, GeneralLedgerTransaction generalLedgerTransaction, string personId, GlAccessLevel glAccessLevel, GeneralLedgerAccountStructure GlConfig);

        /// <summary>
        /// Create a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="generalLedgerTransaction">General Ledger Transaction to create</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        Task<GeneralLedgerTransaction> CreateAsync(GeneralLedgerTransaction generalLedgerTransaction, string personId, GlAccessLevel glAccessLevel, GeneralLedgerAccountStructure GlConfig);

        /// <summary>
        /// Create a single general ledger transaction for the data model version 12
        /// </summary>
        /// <param name="generalLedgerTransaction">General Ledger Transaction to create</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        Task<GeneralLedgerTransaction> Create2Async(GeneralLedgerTransaction generalLedgerTransaction, string personId, GlAccessLevel glAccessLevel, GeneralLedgerAccountStructure GlConfig);        
        /// <summary>
        /// Gets project ref no's.
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds);
    }
}
