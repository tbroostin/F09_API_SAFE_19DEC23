// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for General Ledger Transaction
    /// </summary>
    public interface IGeneralLedgerTransactionService: IBaseService
    {
        /// <summary>
        /// Returns a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>GeneralLedgerTransaction DTO</returns>
        Task<Dtos.GeneralLedgerTransaction> GetByIdAsync(string id);

        /// <summary>
        /// Returns a single general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>GeneralLedgerTransaction DTO</returns>
        Task<Dtos.GeneralLedgerTransaction2> GetById2Async(string id);

        /// <summary>
        /// Returns a single general ledger transaction for the data model version 12
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>GeneralLedgerTransaction DTO</returns>
        Task<Ellucian.Colleague.Dtos.GeneralLedgerTransaction3> GetById3Async(string id);

        /// <summary>
        /// Returns all general ledger transactions for the data model version 6
        /// </summary>
        /// <returns>Collection of GeneralLedgerTransactions</returns>
        Task<IEnumerable<Dtos.GeneralLedgerTransaction>> GetAsync();

        /// <summary>
        /// Returns all general ledger transactions for the data model version 8
        /// </summary>
        /// <returns>Collection of GeneralLedgerTransactions</returns>
        Task<IEnumerable<Dtos.GeneralLedgerTransaction2>> Get2Async();

        /// <summary>
        /// Returns all general ledger transactions for the data model version 12
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<GeneralLedgerTransaction3>> Get3Async(bool bypassCache);

        /// <summary>
        /// Update a single general ledger transaction for the data model version 6
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        Task<Dtos.GeneralLedgerTransaction> UpdateAsync(string id, Dtos.GeneralLedgerTransaction generalLedgerDto);

        /// <summary>
        /// Update a single general ledger transaction for the data model version 8
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        Task<Dtos.GeneralLedgerTransaction2> Update2Async(string id, Dtos.GeneralLedgerTransaction2 generalLedgerDto);

        /// <summary>
        /// Update a single general ledger transaction for the data model version 12
        /// </summary>
        /// <param name="id"></param>
        /// <param name="generalLedgerDto"></param>
        /// <returns></returns>
        Task<Dtos.GeneralLedgerTransaction3> Update3Async(string id, Dtos.GeneralLedgerTransaction3 generalLedgerDto);

        /// <summary>
        /// Create a single general ledger transaction for the data model version 6
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        Task<Dtos.GeneralLedgerTransaction> CreateAsync(Dtos.GeneralLedgerTransaction generalLedgerDto);

        /// <summary>
        /// Create a single general ledger transaction for the data model version 8
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        Task<Dtos.GeneralLedgerTransaction2> Create2Async(Dtos.GeneralLedgerTransaction2 generalLedgerDto);

        /// <summary>
        /// Create a single general ledger transaction for the data model version 12
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        Task<Dtos.GeneralLedgerTransaction3> Create3Async(Dtos.GeneralLedgerTransaction3 generalLedgerDto);
    }
}
