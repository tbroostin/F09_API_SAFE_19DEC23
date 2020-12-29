// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Contains information for a budget officer.
    /// </summary>
    [Serializable]
    public class BudgetOfficer
    {
        /// <summary>
        /// The budget officer ID.
        /// </summary>
        public string BudgetOfficerId { get { return budgetOfficerId; } }
        private string budgetOfficerId {get; set;}

        /// <summary>
        /// The budget officer ID.
        /// </summary>
        public string BudgetOfficerLogin { get; set; }

        /// <summary>
        /// The budget officer name.
        /// </summary>
        public string BudgetOfficerName { get; set; }

        /// <summary>
        /// Constructor that initializes a budget officer entity.
        /// </summary>
        public BudgetOfficer(string budgetOfficerId)
        {
            this.budgetOfficerId = budgetOfficerId;
        }
    }
}
