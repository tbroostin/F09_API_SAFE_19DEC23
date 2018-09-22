// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes the members of a GL object code Budget Pool. 
    /// The umbrella GL account and the list of poolee GL accounts.
    /// </summary>
    [Serializable]
    public class GlObjectCodeBudgetPool
    {
        /// <summary>
        /// The umbrella GL account for the budget pool.
        /// </summary>
        public GlObjectCodeGlAccount Umbrella { get; set; }

        /// <summary>
        /// The list of poolee GL accounts for the budget pool that the user
        /// has access to given their GL account security setup.
        /// </summary>
        public List<GlObjectCodeGlAccount> Poolees { get; set; }

        /// <summary>
        /// Whether the umbrella GL account would be visible to the user
        /// given their GL account security setup.
        /// </summary>
        public bool IsUmbrellaVisible { get; set; }

        /// <summary>
        /// Constructor for the GL object code budget pool.
        /// A budget pool must have an umbrella at least.
        /// </summary>
        /// <param name="umbrellaGlAccount">The GL account for the budget pool which contains a reference to the umbrella GL account.</param>
        public GlObjectCodeBudgetPool(GlObjectCodeGlAccount umbrellaGlAccount)
        {
            if (umbrellaGlAccount == null)
            {
                throw new ArgumentNullException("umbrellaGlAccount", "The umbrella GL account is a required field.");
            }

            this.Umbrella = umbrellaGlAccount;
            this.Poolees = new List<GlObjectCodeGlAccount>();
            this.IsUmbrellaVisible = false;
        }

        /// <summary>
        /// Method to add a GL account poolee to a GL object code budget pool.
        /// </summary>
        /// <param name="pooleeGlAccount">A poolee GL account.</param>
        public void AddPoolee(GlObjectCodeGlAccount pooleeGlAccount)
        {
            if (pooleeGlAccount == null)
            {
                throw new ArgumentNullException("pooleeGlAccount", "The poolee GL account is a required field.");
            }

            if (pooleeGlAccount.PoolType != GlBudgetPoolType.Poolee)
            {
                throw new ApplicationException("Only poolee accounts can be added to the poolees list.");
            }

            // Add the poolee to the budget pool if it's not already in the lists
            if (!this.Poolees.Where(x => x.GlAccountNumber == pooleeGlAccount.GlAccountNumber).Any())
            {
                Poolees.Add(pooleeGlAccount);
        }
    }
}
}
