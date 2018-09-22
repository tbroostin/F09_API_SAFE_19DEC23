// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Describes the members of a General Ledger Budget Pool. 
    /// The umbrella GL account and the list of poolee GL accounts.
    /// </summary>
    public class GlBudgetPool
    {
        /// <summary>
        /// The umbrella GL account for the budget pool.
        /// </summary>
        public CostCenterGlAccount Umbrella { get; set; }

        /// <summary>
        /// The list of poolee GL accounts for the budget pool that the user
        /// has access to given their GL account security setup.
        /// </summary>
        public List<CostCenterGlAccount> Poolees { get; set; }

        /// <summary>
        /// Whether the umbrella GL account would be visible to the user
        /// given their GL account security setup.
        /// </summary>
        public bool IsUmbrellaVisible { get; set; }

    }
}
