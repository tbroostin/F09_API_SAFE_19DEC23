// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Property to contain the financial aid fund financials year id, office id, and budgeted amount
    /// </summary>
    [Serializable]
    public class FinancialAidFundsFinancialProperty
    {
        /// <summary>
        /// Id of the financial aid fund year
        /// </summary>
        public string AidYear { get; private set; }

        /// <summary>
        /// Id of the financial aid fund office
        /// </summary>
        public string Office { get; private set; }

        /// <summary>
        /// The budgeted amount of financial aid funds
        /// </summary>
        public decimal BudgetedAmount { get; private set; }

        /// <summary>
        /// The maximum offered budget amount of financial aid funds
        /// </summary>
        public decimal? MaximumOfferedBudgetAmount { get; set; }

        /// <summary>
        /// The award fund code
        /// </summary>
        public string AwardCode { get; set; }

        /// <summary>
        /// constructor to initialize properties
        /// </summary>
        public FinancialAidFundsFinancialProperty(string aidYear, string office, decimal budget, string awardId, decimal? maxBudget = null)
        {
            if (string.IsNullOrEmpty(aidYear))
            {
                throw new ArgumentNullException("Aid year is null for financial aid fund financial property.");
            }
            if (string.IsNullOrEmpty(office))
            {
                throw new ArgumentNullException("Office is null for financial aid fund financial property.");
            }
            if (budget == null)
            {
                throw new ArgumentNullException("Budget is null for financial aid fund financial property.");
            }
            AidYear = aidYear;
            Office = office;
            BudgetedAmount = budget;
            AwardCode = awardId;
            MaximumOfferedBudgetAmount = maxBudget;
        }
    }
}
