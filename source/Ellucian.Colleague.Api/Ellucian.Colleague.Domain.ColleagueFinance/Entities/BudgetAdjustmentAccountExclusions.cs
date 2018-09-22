// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.using System;

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes which GL accounts to prohibit from being used in budget adjustments based on GL component values.
    /// </summary>
    [Serializable]
    public class BudgetAdjustmentAccountExclusions
    {
        /// <summary>
        /// List of GL account components with a From and a To values that are excluded from being used in a budget adjustment.
        /// </summary>
        public List<BudgetAdjustmentExcludedElement> ExcludedElements { get; set; }

        /// <summary>
        /// Initialize the exclusions object.
        /// </summary>
        public BudgetAdjustmentAccountExclusions()
        {
            ExcludedElements = new List<BudgetAdjustmentExcludedElement>();
        }
    }
}