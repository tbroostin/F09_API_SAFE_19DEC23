/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Object for PayStatementTaxableBenefit. A taxable benefit is a benefit which has a calculated taxable amount and that
    /// amount is included in an employee's total taxable gross amount.
    /// </summary>
    [Serializable]
    public class PayStatementTaxableBenefit
    {
        /// <summary>
        /// The id of the BenefitDeduction object
        /// </summary>
        public string TaxableBenefitId { get; private set; }

        /// <summary>
        /// The description of the BenefitDeduction object
        /// </summary>
        public string TaxableBenefitDescription { get; private set; }

        /// <summary>
        /// The amount for this benefit that is included in the taxable gross
        /// </summary>
        public decimal? TaxableBenefitAmt { get; private set; }

        /// <summary>
        /// The year-to-date amount for this taxable benefit
        /// </summary>
        public decimal? TaxableBenefitYtdAmt { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxableBenefitId"></param>
        /// <param name="taxableBenefitDesc"></param>
        /// <param name="taxableBenefitAmt"></param>
        public PayStatementTaxableBenefit(string taxableBenefitId, string taxableBenefitDesc, decimal? taxableBenefitAmt, decimal? taxableBenefitYtdAmt)
        {
            if (string.IsNullOrWhiteSpace(taxableBenefitId))
            {
                throw new ArgumentNullException("taxableBenefitId");
            }
            TaxableBenefitId = taxableBenefitId;
            TaxableBenefitDescription = taxableBenefitDesc;
            TaxableBenefitAmt = taxableBenefitAmt;
            TaxableBenefitYtdAmt = taxableBenefitYtdAmt;
        }
    }
}
