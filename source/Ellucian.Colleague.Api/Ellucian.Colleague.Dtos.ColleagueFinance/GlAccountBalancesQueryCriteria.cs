// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// GL account balances query input criteria.
    /// </summary>
    public class GlAccountBalancesQueryCriteria
    {
        /// <summary>
        /// List of GL accounts.
        /// </summary>
        public List<string> GlAccounts { get; set; }

        /// <summary>
        /// The fiscal year for the activity detail. Required
        /// </summary>
        public string FiscalYear { get; set; }
    }
}
