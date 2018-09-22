/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Object for PayStatementTaxableBenefit. A taxable benefit is a benefit which has a calculated taxable amount and that
    /// amount is included in an employee's total taxable gross amount.
    /// </summary>
    public class PayStatementTaxableBenefit
    {
        /// <summary>
        /// The id of the BenefitDeduction object
        /// </summary>
        public string TaxableBenefitId { get; set; }

        /// <summary>
        /// The description of the BenefitDeduction object
        /// </summary>
        public string TaxableBenefitDescription { get; set; }

        /// <summary>
        /// The amount for this benefit that is included in the taxable gross
        /// </summary>
        public decimal? TaxableBenefitAmt { get; set; }

        /// <summary>
        /// The year to date amount for this taxable benefit
        /// </summary>
        public decimal? TaxableBenefitYtdAmt { get; set; }

    }
}
