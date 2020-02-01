// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Base GL component query filter criteria.
    /// </summary>
    public class BaseGlComponentQueryCriteria
    {
        /// <summary>
        /// Fiscal year.
        /// </summary>
        public string FiscalYear { get; set; }

        /// <summary>
        /// A list of general ledger component filter criteria.
        /// </summary>
        public List<CostCenterComponentQueryCriteria> ComponentCriteria { get; set; }

        /// <summary>
        /// Boolean flag to control what type of accounts are returned.
        /// </summary>
        public bool IncludeActiveAccountsWithNoActivity { get; set; }        

    }
}
