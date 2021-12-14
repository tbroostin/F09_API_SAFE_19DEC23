// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of the methods that must be implemented for a GL cost center repository.
    /// </summary>
    public interface ICostCenterRepository
    {
        /// <summary>
        /// Get a list of GL cost center assigned to the user logged in.
        /// </summary>
        /// <param name="generalLedgerUser">General Ledger User domain entity.</param>
        /// <param name="costCenterStructure">Cost Center structure domain entity.</param>
        /// <param name="glClassConfiguration">GL class configuration structure domain entity.</param>
        /// <param name="costCenterId">A cost center id for the department view, or nothing for the cost centers view.</param>
        /// <param name="fiscalYear">The fiscal year requested.</param>
        /// <param name="criteria">Cost center filter criteria.</param>
        /// <param name="personId">ID of the user.</param>
        /// <param name="includeJustificationNotes">Show Justification Notes flag.</param>
        /// <returns>List of GL cost center domain entities.</returns>
        Task<IEnumerable<CostCenter>> GetCostCentersAsync(GeneralLedgerUser generalLedgerUser, CostCenterStructure costCenterStructure, GeneralLedgerClassConfiguration glClassConfiguration, string costCenterId, string fiscalYear, CostCenterQueryCriteria criteria, string personId, bool includeJustificationNotes = false);
    }
}