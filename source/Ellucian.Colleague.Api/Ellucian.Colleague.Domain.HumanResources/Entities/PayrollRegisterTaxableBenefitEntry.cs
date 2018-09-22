/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Object for PayrollRegisterTaxableBenefitEntry. A taxable benefit is a benefit which has a calculated taxable amount and that
    /// amount is included in an employee's total taxable gross amount.
    /// </summary>
    [Serializable]
    public class PayrollRegisterTaxableBenefitEntry
    {
        /// <summary>
        /// The id of the BenefitDeduction object
        /// </summary>
        public string TaxableBenefitId { get; private set; }

        /// <summary>
        /// The tax code this taxable benefit applies to
        /// </summary>
        public string TaxableBenefitTaxCode { get; private set; }

        /// <summary>
        /// The amount for this benefit that is included in the taxable gross
        /// </summary>
        public decimal? TaxableBenefitAmt { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxableBenefitId"></param>
        /// <param name="taxableBenefitAmt"></param>
        public PayrollRegisterTaxableBenefitEntry(string taxableBenefitId, string taxableBenefitTaxCode, decimal? taxableBenefitAmt)
        {
            if (string.IsNullOrWhiteSpace(taxableBenefitId))
            {
                throw new ArgumentNullException("taxableBenefitId");
            }

            if (string.IsNullOrWhiteSpace(taxableBenefitTaxCode))
            {
                throw new ArgumentNullException("taxableBenefitTaxCode");
            }

            TaxableBenefitId = taxableBenefitId;
            TaxableBenefitAmt = taxableBenefitAmt;
            TaxableBenefitTaxCode = taxableBenefitTaxCode;
        }
    }
}
