/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public class BudgetComponent
    {
        /// <summary>
        /// The first part of the unique identifier of this BudgetComponent object
        /// </summary>
        public string Code { get { return _Code; } }
        private readonly string _Code;

        /// <summary>
        /// The second part of the unique identifier of this BudgetComponent object
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }
        private readonly string _AwardYear;

        /// <summary>
        /// A short description of what this BudgetComponent defines
        /// </summary>
        public string Description { get { return _Description; } }
        private readonly string _Description;

        /// <summary>
        /// The group assigned to this BudgetComponent to categorize students' costs on the Financial Aid shopping sheet
        /// </summary>
        public ShoppingSheetBudgetGroup? ShoppingSheetGroup { get; set; }

        /// <summary>
        /// Create a BudgetComponent object
        /// </summary>
        /// <param name="awardYear">The AwardYear this component is assigned to</param>
        /// <param name="code">The code that identifies the BudgetComponent</param>
        /// <param name="description">A short description of this budget component</param>
        public BudgetComponent(string awardYear, string code, string description)
        {
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }

            _Code = code;
            _AwardYear = awardYear;
            _Description = description;
        }

        /// <summary>
        /// Two BudgetComponents are equal if their codes and award years are equal.
        /// </summary>
        /// <param name="obj">The BudgetComponent object to compare to this</param>
        /// <returns>True or False</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var budgetComponenet = obj as BudgetComponent;

            if (budgetComponenet.AwardYear == this.AwardYear &&
                budgetComponenet.Code == this.Code)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the HashCode of this BudgetComponent
        /// </summary>
        /// <returns>The HashCode of this object</returns>
        public override int GetHashCode()
        {
            return AwardYear.GetHashCode() ^ Code.GetHashCode();
        }

        /// <summary>
        /// Get a string representation of this BudgetComponent [AwardYear]*[Code]
        /// </summary>
        /// <returns>A string representing this BudgetComponent</returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}", AwardYear, Code);
        }
    }
}
