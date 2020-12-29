// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Interface to the General Ledger Activity Detail repository.
    /// </summary>
    public interface IGeneralLedgerActivityDetailRepository
    {
        /// <summary>
        /// Get the GL account activity detail for a fiscal year.
        /// </summary>
        /// <param name="costCenterStructure">Cost Center structure domain entity.</param>
        /// <param name="glAccount">The GL account number.</param>
        /// <param name="fiscalYear">The fiscal year.</param>
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <returns>The activity details for the GL account for the fiscal year.</returns>
        Task<GlAccountActivityDetail> QueryGlActivityDetailAsync(string glAccount, string fiscalYear, CostCenterStructure costCenterStructure, GeneralLedgerClassConfiguration glClassConfiguration, IList<string> majorComponentStartPosition);
    }
}
