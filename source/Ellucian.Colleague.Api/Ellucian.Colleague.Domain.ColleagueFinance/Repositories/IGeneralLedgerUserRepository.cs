// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// This is the definition of the methods that must be implemented
    /// for a general ledger user repository.
    /// </summary>
    public interface IGeneralLedgerUserRepository
    {
        /// <summary>
        /// This is the method definition for getting a general ledger user object.
        /// </summary>
        /// <param name="id">This is the user id.</param>
        /// <param name="fullAccessRole">Role that defines full GL account access.</param>
        /// <param name="classificationName">Name of the GL class component.</param>
        /// <param name="expenseClassValues">Set of GL class component expense account values.</param>
        /// <returns>Returns a general ledger user domain entity.</returns>
        Task<GeneralLedgerUser> GetGeneralLedgerUserAsync(string id, string fullAccessRole, string classificationName, IEnumerable<string> expenseClassValues);

        /// <summary>
        /// Returns a general ledger user object.
        /// </summary>
        /// <param name="id">This is the user id.</param>
        /// <param name="fullAccessRole">Role that defines full GL account access.</param>
        /// <param name="glClassConfiguration">GL Class Configuration</param>
        /// <returns>Returns a general ledger user domain entity.</returns>
        Task<GeneralLedgerUser> GetGeneralLedgerUserAsync2(string id, string fullAccessRole, GeneralLedgerClassConfiguration glClassConfiguration);

        /// <summary>
        /// See if the sod record exist and if it has OVERRIDE
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> CheckOverride(string id);
    }
}