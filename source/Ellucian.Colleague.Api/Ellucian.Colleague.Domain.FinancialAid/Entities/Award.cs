/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Financial Aid Award class. An award is identified by its code.
    /// </summary>
    [Serializable]
    public class Award : CodeItem
    {
        /// <summary>
        /// Text explaining to the student information about this award code.
        /// </summary>
        public string Explanation { get { return _explanation; } }
        private readonly string _explanation;

        /// <summary>
        /// The AwardCategory of this award, used to group Awards with similar attributes
        /// </summary>
        public AwardCategory AwardCategory { get { return awardCategory; } }
        private readonly AwardCategory awardCategory;

        /// <summary>
        /// The LoanType of this award. An award will have a loan type if its a loan.
        /// Otherwise, this attribute will be null.
        /// </summary>
        public LoanType? LoanType
        {
            get
            {
                if (AwardCategory != null)
                {
                    return AwardCategory.CategoryLoanType;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Flag indicates whether or not this award is a type of federal direct loan.
        /// This flag is true for loans that are processed by the federal government.
        /// This flag will be false for awards that are not loans and for loans that
        /// are processed by private lenders.
        /// </summary>
        public bool IsFederalDirectLoan { get; set; }

        /// <summary>
        /// AwardCategoryType defines Loan, Grant, Scholarship or Work and is 
        /// retrieved from the AwardCategory attribute.
        /// Attribute is needed here for Student Success CRM (aka Pilot)
        /// </summary>
        public AwardCategoryType? AwardCategoryType
        {
            get
            {
                if (AwardCategory != null)
                {
                    return AwardCategory.AwardCategoryType;
                }
                else
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// AwardType which defines Funding Source as
        /// Federal
        /// Institutional
        /// State
        /// Other
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Attribute categorizes the award for placement in the Financial Aid Shopping Sheet
        /// </summary>
        public ShoppingSheetAwardGroup? ShoppingSheetGroup { get; set; }

        private readonly string[] TIVAwardCategories = new string[7] { "GPLUS", "PLUS", "GRTCH", "UGTCH", "GSL", "USTF", "PELL"};
        /// <summary>
        /// Flag indicating whether this award is TIV or not
        /// </summary>
        public bool IsTitleIV
        {
            get
            {
                if (AwardCategory == null || string.IsNullOrEmpty(AwardCategory.Code))
                {
                    return false;
                }
                else return TIVAwardCategories.Contains(AwardCategory.Code);
            }
        }

        /// <summary>
        /// Create an Award object 
        /// </summary>
        /// <param name="code">Unique Award Code</param>
        /// <param name="desc">Short description of award</param>
        /// <param name="awardCategory">Category of this Award</param>
        /// <param name="explanation">Explanation of this award for the student</param>
        public Award(string code, string desc, AwardCategory awardCategory, string explanation = "")
            : base(code, desc)
        {
            this.awardCategory = awardCategory;
            _explanation = explanation;
            IsFederalDirectLoan = false;
        }
    }
}
