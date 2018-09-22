// Copyright 2018 Ellucian Company L.P. and its affiliates.using System;

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// A GL account component and its range of values to prohibit from being used in budget adjustments.
    /// </summary>
    [Serializable]
    public class BudgetAdjustmentExcludedElement
    {
        /// <summary>
        /// A GL account component.
        /// </summary>
        public GeneralLedgerComponent ExclusionComponent { get; set; }

        /// <summary>
        /// An object with the component start and end values.
        /// </summary>
        public GeneralLedgerComponentRange ExclusionRange { get; set; }

        /// <summary>
        /// Initialize the exclusions object.
        /// </summary>
        public BudgetAdjustmentExcludedElement()
        {

        }
    }
}