/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

//TODO mcd: This class is no longer used. It can be deleted.
namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Pay Statement Source Tax.
    /// </summary>
    [Serializable]
    public class PayStatementSourceTax
    {
        public string TaxCode { get; private set; }
        public string TaxDescription { get; private set; }
        public decimal? EmployeePeriodTaxAmount { get; private set; }
        public decimal? EmployerPeriodTaxAmount { get; private set; }
        public decimal? EmployeePeriodTaxableAmount { get; private set; }
        public decimal? EmployerPeriodTaxableAmount { get; private set; }
        public decimal? PeriodApplicableTaxableGross { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxCode"></param>
        /// <param name="taxDescription"></param>
        /// <param name="employeePeriodTaxAmount"></param>
        /// <param name="employerPeriodTaxAmount"></param>
        /// <param name="employeePeriodTaxableAmount"></param>
        /// <param name="employerPeriodTaxableAmount"></param>
        /// <param name="periodApplicableTaxableGross"></param>
        public PayStatementSourceTax(
            string taxCode, string taxDescription, 
            decimal? employeePeriodTaxAmount, decimal? employerPeriodTaxAmount, 
            decimal? employeePeriodTaxableAmount, decimal? employerPeriodTaxableAmount
        )
        {
            if (string.IsNullOrWhiteSpace(taxCode))
            {
                throw new ArgumentNullException("taxCode");
            }
            if (string.IsNullOrWhiteSpace(taxDescription))
            {
                throw new ArgumentNullException("taxDescription");
            }

            TaxCode = taxCode;
            TaxDescription = taxDescription;
            EmployeePeriodTaxAmount = employeePeriodTaxAmount;
            EmployerPeriodTaxAmount = employerPeriodTaxAmount;
            EmployeePeriodTaxableAmount = employeePeriodTaxableAmount;
            EmployerPeriodTaxableAmount = employerPeriodTaxableAmount;
            PeriodApplicableTaxableGross = employeePeriodTaxableAmount ?? employerPeriodTaxableAmount;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            return TaxCode == ((PayStatementSourceTax)obj).TaxCode;
        }

        public override int GetHashCode()
        {
            return TaxCode.GetHashCode();
        }
    }
}