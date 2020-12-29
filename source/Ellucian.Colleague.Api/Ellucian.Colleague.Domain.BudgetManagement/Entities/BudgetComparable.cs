// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Contains the comparable information for a budget line item for a working budget.
    /// </summary>
    [Serializable]
    public class BudgetComparable
    {
        /// <summary>
        /// The budget comparable number.
        /// </summary>
        public string ComparableNumber { get { return comparableNumber; } }
        private readonly string comparableNumber;
        /// <summary>
        /// The comparable amount.
        /// </summary>
        public long? ComparableAmount { get; set; }

        /// <summary>
        /// Constructor that initializes a budget comparable entity.
        /// </summary>
        /// <param name="comparableNumber">A comparable number.</param>
        public BudgetComparable(string comparableNumber)
        {
            if (string.IsNullOrEmpty(comparableNumber))
            {
                throw new ArgumentNullException("comparableNumber", "comparableNumber is a required field.");
            }

            this.comparableNumber = comparableNumber;
        }
    }
}
