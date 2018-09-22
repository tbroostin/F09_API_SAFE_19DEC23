// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a requisition repository
    /// </summary>
    public interface ILedgerActivityRepository
    {
        /// <summary>
        /// Returns general ledger activities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="fiscalYear"></param>
        /// <param name="fiscalPeriod"></param>
        /// <param name="reportingSegment"></param>
        /// <param name="transactionDate"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Tuple<IEnumerable<GeneralLedgerActivity>, int></returns>
        Task<Tuple<IEnumerable<GeneralLedgerActivity>, int>> GetGlaFyrAsync(int offset, int limit, string fiscalYear, string fiscalPeriod, string fiscalPeriodYear, string reportingSegment, string transactionDate);

        /// <summary>
        /// Returns a single general ledger activity record.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<GeneralLedgerActivity> GetGlaFyrByIdAsync(string guid);

        /// <summary>
        /// Gets unidata date format.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<string> GetUnidataFormattedDate(string date);
    }
}
