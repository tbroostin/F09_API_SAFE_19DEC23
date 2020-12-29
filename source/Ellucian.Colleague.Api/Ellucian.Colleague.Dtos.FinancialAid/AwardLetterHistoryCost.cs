//Copyright 2019 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Contains a cost's description and amount, returned from Award Letter History of colleague.
    /// </summary>
    public class AwardLetterHistoryCost
    {
        /// <summary>
        /// Indirect or Direct type
        /// </summary>
        public string CostType { get; set; }
        /// <summary>
        /// The description or name of the Award Letter history cost 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The amount associated with the award letter history cost
        /// </summary>
        public int Amount { get; set; }

    }
}
