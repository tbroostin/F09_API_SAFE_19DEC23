/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Award Categories are used to group together Awards with similar attributes
    /// </summary>
    [Serializable]
    public class AwardCategory : CodeItem
    {
        /// <summary>
        /// The type indicates the very top level kind of awards this award category describes. An award category's type
        /// is recommended, but not required.
        /// This is used by both Financial Aid and Student Success CRM (aka Pilot)
        /// </summary>
        public AwardCategoryType? AwardCategoryType { get { return awardCategoryType; } }
        private readonly AwardCategoryType? awardCategoryType;


        public LoanType? CategoryLoanType
        {
            get
            {
                if (AwardCategoryType.HasValue && AwardCategoryType.Value == Domain.FinancialAid.Entities.AwardCategoryType.Loan)
                {
                    switch (this.Code.ToUpper())
                    {
                        case "GSL":
                            return LoanType.SubsidizedLoan;
                        case "USTF":
                            return LoanType.UnsubsidizedLoan;
                        case "GPLUS":
                            return LoanType.GraduatePlusLoan;
                        case "PLUS":
                            return LoanType.PlusLoan;
                        default:
                            return LoanType.OtherLoan;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Create an AwardCategory object
        /// </summary>
        /// <param name="code">The unique code id of the award category</param>
        /// <param name="description">A short description of the award category</param>
        public AwardCategory(string code, string description, AwardCategoryType? awardCategoryType)
            : base(code, description)
        {
            this.awardCategoryType = awardCategoryType;
        }

        /// <summary>
        /// Two AwardCategory objects are equal when their codes are equal
        /// </summary>
        /// <param name="obj">AwardCategory object to compare to this AwardCategory object</param>
        /// <returns>True of the two AwardCategory object's codes are equal. False otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var category = obj as AwardCategory;

            if (category.Code.ToUpper() == this.Code.ToUpper())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the HashCode for this AwardCategory object
        /// </summary>
        /// <returns>A HashCode based on this AwardCategory's Code</returns>
        public override int GetHashCode()
        {
            return this.Code.GetHashCode();
        }

        /// <summary>
        /// Get the string representation of this AwardCategory object
        /// </summary>
        /// <returns>A string representation of this AwardCategory's Code</returns>
        public override string ToString()
        {
            return this.Code.ToUpper();
        }
    }
}
