// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of the methods that must be implemented for a GL object code repository.
    /// </summary>
    public interface IGlObjectCodeRepository
    {
        /// <summary>
        /// Get a list of GL object codes assigned to the user logged in.
        /// They are used in the object view which you can only access from the cost center view,
        /// so the cost center filter criteria is used to filter the GL object codes.
        /// </summary>
        /// <param name="generalLedgerUser">General Ledger User domain entity.</param>
        /// <param name="glAccountStructure">GL Account structure domain entity.</param>
        /// <param name="glClassConfiguration">GL class configuration structure domain entity.</param>
        /// <param name="criteria">Cost center filter criteria.</param>
        /// <param name="personId">ID of the user.</param>
        /// <returns>List of GL object code domain entities.</returns>
        Task<IEnumerable<GlObjectCode>> GetGlObjectCodesAsync(GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration, CostCenterQueryCriteria criteria, string personId);
    }
}