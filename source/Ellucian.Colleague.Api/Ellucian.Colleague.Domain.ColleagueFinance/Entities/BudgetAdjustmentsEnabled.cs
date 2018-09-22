// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Are budget adjustments turned on or off?
    /// </summary>
    [Serializable]
    public class BudgetAdjustmentsEnabled
    {
        /// <summary>
        /// Are budget adjustments turned on or off?
        /// </summary>
        public bool Enabled { get { return enabled; } }
        private readonly bool enabled;

        /// <summary>
        /// Initialize the entity.
        /// </summary>
        public BudgetAdjustmentsEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
    }
}