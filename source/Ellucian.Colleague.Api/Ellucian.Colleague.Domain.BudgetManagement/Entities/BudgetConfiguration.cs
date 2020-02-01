// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Contains configuration information for Budget Development.
    /// </summary>
    [Serializable]
    public class BudgetConfiguration
    {
        /// <summary>
        /// The working budget ID.
        /// </summary>
        public string BudgetId { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The working budget title.
        /// </summary>
        public string BudgetTitle { get; set; }

        /// <summary>
        /// The working budget year.
        /// </summary>
        public string BudgetYear { get; set; }

        /// <summary>
        /// The working budget status.
        /// </summary>
        public BudgetStatus BudgetStatus { get; set; }

        /// <summary>
        /// List of budget comparables used in budget generation.
        /// </summary>
        public ReadOnlyCollection<BudgetConfigurationComparable> BudgetConfigurationComparables { get; private set; }
        private readonly List<BudgetConfigurationComparable> budgetConfigurationComparables = new List<Entities.BudgetConfigurationComparable>();

        /// <summary>
        /// The budget development configuration constractor.
        /// </summary>
        /// <param name="id">The working budget ID.</param>
        public BudgetConfiguration(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id ", "ID is a required field.");

            this.id = id;
            BudgetConfigurationComparables = budgetConfigurationComparables.AsReadOnly();
        }

        /// <summary>
        /// Add a budget configuration comparable to the list in budget configuration.
        /// </summary>
        /// <param name="budgetConfigurationComparable">A budget configuration comparable object.</param>
        public void AddBudgetConfigurationComparable(BudgetConfigurationComparable budgetConfigurationComparable)
        {
            if (budgetConfigurationComparable == null)
            {
                throw new ArgumentNullException("budgetConfigurationComparable", "budgetConfigurationComparable cannot be null");
            }
            budgetConfigurationComparables.Add(budgetConfigurationComparable);
        }

        /// <summary>
        /// Remove all budget configuration comparables from the list in budget configuration.
        /// </summary>
        public void RemoveBudgetConfigurationComparables()
        {
            budgetConfigurationComparables.RemoveAll(x => true);
        }
    }
}

