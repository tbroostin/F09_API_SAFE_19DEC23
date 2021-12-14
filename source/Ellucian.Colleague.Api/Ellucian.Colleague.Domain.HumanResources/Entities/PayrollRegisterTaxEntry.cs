/*Copyright 2017-2021 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Describes the tax amounts that were taken out of a person's paycheck. 
    /// A TaxEntry must be assigned to a PayrollRegisterEntry.
    /// </summary>
    [Serializable]
    public class PayrollRegisterTaxEntry
    {
        /// <summary>
        /// The code of the TaxCode that defines this Tax.
        /// </summary>
        public string TaxCode { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public PayrollTaxProcessingCode ProcessingCode { get; private set; }

        /// <summary>
        /// Any amount that's associated with a Processing Code of FixedAmount, AdditionalTaxAmount or AdditionalTaxableAmount ie.
        /// ProcessingCode = AdditionalTaxAmount, and SpecialProcessingAmount = 100, means that
        /// $100 is added to the total tax paid.
        /// </summary>
        public decimal? SpecialProcessingAmount { get; set; }

        /// <summary>
        /// The number of exemptions taken for this tax entry
        /// </summary>
        public int Exemptions { get; set; }

        /// <summary>
        /// The amount of tax paid by the employee
        /// </summary>
        public decimal? EmployeeTaxAmount { get; set; }

        /// <summary>
        /// The amount of tax paid by the employer
        /// </summary>
        public decimal? EmployerTaxAmount { get; set; }

        /// <summary>
        /// The basis amount used to calculate the employee's portion of the tax amount, e.g. $600 is the basis amount in the following formula
        /// $600 Basis * %5 Tax = $30 tax amount
        /// </summary>
        public decimal? EmployeeTaxableAmount { get; set; }

        /// <summary>
        /// The basis amount used to calculate the employer's portion of the tax amount.
        /// </summary>
        public decimal? EmployerTaxableAmount { get; set; }

        /// <summary>
        /// Employee Adjustment amount if this is an adjustment
        /// </summary>
        public decimal? EmployeeAdjustmentAmount { get; set; }

        /// <summary>
        /// Employer Adjustment amount if this is an adjustment
        /// </summary>
        public decimal? EmployerAdjustmentAmount { get; set; }

        /// <summary>
        /// Employee Taxable Adjustment amount if this is an adjustment
        /// </summary>
        public decimal? EmployeeTaxableAdjustmentAmount { get; set; }

        /// <summary>
        /// Employer Taxable Adjustment amount if this is an adjustment
        /// </summary>
        public decimal? EmployerTaxableAdjustmentAmount { get; set; }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxCode"></param>
        public PayrollRegisterTaxEntry(string taxCode, PayrollTaxProcessingCode processingCode)
        {
            if (string.IsNullOrEmpty(taxCode))
            {
                throw new ArgumentNullException("taxCode");
            }

            TaxCode = taxCode;
            ProcessingCode = processingCode;
        }
    }
}
