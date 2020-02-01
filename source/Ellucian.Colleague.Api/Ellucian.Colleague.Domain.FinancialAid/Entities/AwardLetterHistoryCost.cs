//Copyright 2019 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public class AwardLetterHistoryCost
    {
        /// <summary>
        /// The description or name of the Award Letter history cost 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The amount associated with the award letter history cost
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// The Type associated with each cost (direct / indirect)
        /// </summary>
        public string CostType { get; set; }

        public AwardLetterHistoryCost(string description, int amount)
        {
            Description = description;
            Amount = amount;
        }
    }
}
