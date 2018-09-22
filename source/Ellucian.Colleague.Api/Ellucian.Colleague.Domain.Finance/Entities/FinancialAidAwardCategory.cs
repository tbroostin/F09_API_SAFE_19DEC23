/*Copyright 2018 Ellucian Company L.P. and its affiliates.*/
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Financial Aid Award Category class
    /// </summary>
    [Serializable]
    public class FinancialAidAwardCategory : CodeItem
    {
        
        /// <summary>
        /// Create an FinancialAidAwardCategory object
        /// </summary>
        /// <param name="code">The unique code id of the award category</param>
        /// <param name="description">A short description of the award category</param>
        public FinancialAidAwardCategory(string code, string description)
            : base(code, description)
        {}

        /// <summary>
        /// Two FinancialAidAwardCategory objects are equal when their codes are equal
        /// </summary>
        /// <param name="obj">FinancialAidAwardCategory object to compare to this FinancialAidAwardCategory object</param>
        /// <returns>True of the two FinancialAidAwardCategory object's codes are equal. False otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var category = obj as FinancialAidAwardCategory;

            if (category.Code.ToUpper() == this.Code.ToUpper())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the HashCode for this FinancialAidAwardCategory object
        /// </summary>
        /// <returns>A HashCode based on this FinancialAidAwardCategory's Code</returns>
        public override int GetHashCode()
        {
            return this.Code.GetHashCode();
        }

        /// <summary>
        /// Get the string representation of this FinancialAidAwardCategory object
        /// </summary>
        /// <returns>A string representation of this FinancialAidAwardCategory's Code</returns>
        public override string ToString()
        {
            return this.Code.ToUpper();
        }
    }
}
