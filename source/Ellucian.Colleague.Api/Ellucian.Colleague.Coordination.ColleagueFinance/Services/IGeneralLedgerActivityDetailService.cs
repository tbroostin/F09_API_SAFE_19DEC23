// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Definition of methods that must be implemented for a GL activity detail service.
    /// </summary>
    public interface IGeneralLedgerActivityDetailService
    {
        /// <summary>
        /// Get the GL account activity detail for a fiscal year.
        /// </summary>
        /// <param name="glAccount">GL account selected by the user.</param>
        /// <param name="fiscalYear">General Ledger fiscal year.</param>
        /// <returns>GL account DTO with a list of activity detail for the specified fiscal year.</returns>
        Task<GlAccountActivityDetail> QueryGlAccountActivityDetailAsync(string glAccount, string fiscalYear);
    }
}

