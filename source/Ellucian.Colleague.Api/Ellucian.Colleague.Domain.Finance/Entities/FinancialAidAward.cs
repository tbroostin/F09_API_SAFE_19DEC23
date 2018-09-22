//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class FinancialAidAward : CodeItem
    {
        /// <summary>
        /// The FinancialAidAwardCategory of this award
        /// </summary>
        public FinancialAidAwardCategory AwardCategory { get { return awardCategory; } }
        private readonly FinancialAidAwardCategory awardCategory;

        private readonly string[] TIVAwardCategories = new string[7] { "GPLUS", "PLUS", "GRTCH", "UGTCH", "GSL", "USTF", "PELL" };
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
        public FinancialAidAward(string code, string desc, FinancialAidAwardCategory awardCategory)
            : base(code, desc)
        {
            this.awardCategory = awardCategory;
        }
    }
}
