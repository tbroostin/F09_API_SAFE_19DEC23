/* Copyright 2017-2021 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// A deduction item for a tax, benefit, or other deduction
    /// </summary>
    [Serializable]
    public class PayStatementDeduction
    {
        /// <summary>
        /// The Deduction code.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// The description of the deduction
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The type of deduction
        /// </summary>
        public PayStatementDeductionType Type { get; private set; }

        /// <summary>
        /// The amount deducted from the employee's earnings for this deduction line item for the period
        /// </summary>
        public decimal? EmployeePeriodAmount { get; private set; }

        /// <summary>
        /// The amount the employer paid for this deduction line item for the period.
        /// </summary>
        public decimal? EmployerPeriodAmount { get; private set; }

        /// <summary>
        /// The basis amount on which the deduction amount was calculated.
        /// </summary>
        public decimal? ApplicableGrossPeriodAmount { get; private set; }

        /// <summary>
        /// The amount deducted from the employee's earnings for this deduction line item, year to date.
        /// </summary>
        public decimal? EmployeeYearToDateAmount
        {
            get
            {
                if (Type == PayStatementDeductionType.Tax)
                {
                    return (yearToDateTaxEntries.Sum(t => t.EmployeeTaxAmount)
                        + yearToDateTaxEntries.Sum(t => t.EmployeeAdjustmentAmount));
                }
                else
                {
                    return (yearToDateBenefitDeductionEntries.Sum(t => t.EmployeeAmount)
                        + yearToDateBenefitDeductionEntries.Sum(t => t.EmployeeAdjustmentAmount));
                }
            }
        }
        /// <summary>
        /// The amount the employer paid for this deduction line item, year to date
        /// </summary>
        public decimal? EmployerYearToDateAmount
        {
            get
            {
                if (Type == PayStatementDeductionType.Tax)
                {
                    return (yearToDateTaxEntries.Sum(t => t.EmployerTaxAmount)
                        + yearToDateTaxEntries.Sum(t => t.EmployerAdjustmentAmount)); 
                }
                else
                {
                    return (yearToDateBenefitDeductionEntries.Sum(t => t.EmployerAmount)
                        + yearToDateBenefitDeductionEntries.Sum(t => t.EmployerAdjustmentAmount)); 
                }
            }
        }

        /// <summary>
        /// The total basis amount on which the deduction amounts are calculated plus/minus any adjustments
        /// </summary>
        public decimal? ApplicableGrossYearToDateAmount
        {
            get
            {
                if (Type == PayStatementDeductionType.Tax)
                {
                    return (yearToDateTaxEntries.Sum(t => t.EmployeeTaxableAmount)
                        + yearToDateTaxEntries.Sum(t => t.EmployeeTaxableAdjustmentAmount));
                }
                else
                {
                    return (yearToDateBenefitDeductionEntries.Sum(t => t.EmployeeBasisAmount)
                        + yearToDateBenefitDeductionEntries.Sum(t => t.EmployeeBasisAdjustmentAmount));
                }
            }
        }

        public bool IsZeroYearToDateAmount
        {
            get
            {
                return !(EmployeeYearToDateAmount.HasValue || EmployerYearToDateAmount.HasValue);
            }
        }


        private IEnumerable<PayrollRegisterTaxEntry> yearToDateTaxEntries;
        private IEnumerable<PayrollRegisterBenefitDeductionEntry> yearToDateBenefitDeductionEntries;

        /// <summary>
        /// Create a Tax Type PayStatementDeduction for taxes paid during this period.
        /// </summary>
        /// <param name="taxCode"></param>
        /// <param name="periodTaxEntry"></param>
        /// <param name="yearToDateTaxEntries"></param>
        public PayStatementDeduction(TaxCode taxCode,
            PayrollRegisterTaxEntry periodTaxEntry, 
            IEnumerable<PayrollRegisterTaxEntry> yearToDateTaxEntries)
        {
            if (taxCode == null)
            {
                throw new ArgumentNullException("taxCode");
            }
            if (periodTaxEntry == null)
            {
                throw new ArgumentNullException("periodTaxEntry");
            }
            if (yearToDateTaxEntries == null)
            {
                throw new ArgumentNullException("yearToDateTaxEntries");
            }

            Code = taxCode.Code;
            Description = taxCode.Description;
            Type = PayStatementDeductionType.Tax;
            EmployeePeriodAmount = periodTaxEntry.EmployeeTaxAmount;
            EmployerPeriodAmount = periodTaxEntry.EmployerTaxAmount;
            ApplicableGrossPeriodAmount = periodTaxEntry.EmployeeTaxableAmount;

            this.yearToDateTaxEntries = yearToDateTaxEntries.Where(t => t.TaxCode == Code).ToList();


        }

        /// <summary>
        /// Create a Tax Type PayStatementDeduction for taxes not paid during this period, but paid at some point in the year to date
        /// </summary>
        /// <param name="taxCode"></param>
        /// <param name="yearToDateTaxEntries"></param>
        public PayStatementDeduction(TaxCode taxCode,
            IEnumerable<PayrollRegisterTaxEntry> yearToDateTaxEntries)
        {
            if (taxCode == null)
            {
                throw new ArgumentNullException("taxCode");
            }
            if (yearToDateTaxEntries == null)
            {
                throw new ArgumentNullException("yearToDateTaxEntries");
            }

            Code = taxCode.Code;
            Description = taxCode.Description;
            Type = PayStatementDeductionType.Tax;

            this.yearToDateTaxEntries = yearToDateTaxEntries.Where(t => t.TaxCode == Code).ToList();                       
        }

        /// <summary>
        /// Create a Benefit or Other Deduction type PayStatementDeduction for benefits or deductions paid in the period
        /// </summary>
        /// <param name="benefitDeductionType"></param>
        /// <param name="periodBenefitDeductionEntry"></param>
        /// <param name="yearToDateBenefitDeductionEntries"></param>
        public PayStatementDeduction(BenefitDeductionType benefitDeductionType, 
            PayrollRegisterBenefitDeductionEntry periodBenefitDeductionEntry, 
            IEnumerable<PayrollRegisterBenefitDeductionEntry> yearToDateBenefitDeductionEntries)
        {       
            if (benefitDeductionType == null)
            {
                throw new ArgumentNullException("benefitDeductionType");
            }
            if (periodBenefitDeductionEntry == null)
            {
                throw new ArgumentNullException("periodBenefitDeductionEntry");
            }
            if (yearToDateBenefitDeductionEntries == null)
            {
                throw new ArgumentNullException("yearToDateBenefitDeductionEntries");
            }
            

            Code = benefitDeductionType.Id;
            Description = string.IsNullOrWhiteSpace(benefitDeductionType.SelfServiceDescription) ? benefitDeductionType.Description : benefitDeductionType.SelfServiceDescription;
            Type = benefitDeductionType.Category == BenefitDeductionTypeCategory.Benefit ? PayStatementDeductionType.Benefit : PayStatementDeductionType.Deduction;
            EmployeePeriodAmount = periodBenefitDeductionEntry.EmployeeAmount;
            EmployerPeriodAmount = periodBenefitDeductionEntry.EmployerAmount;
            ApplicableGrossPeriodAmount = periodBenefitDeductionEntry.EmployeeBasisAmount;

            this.yearToDateBenefitDeductionEntries = yearToDateBenefitDeductionEntries.Where(bd => bd.BenefitDeductionId == Code).ToList();        
        }

        /// <summary>
        /// Create a Benefit or Other Deduction type PayStatementDeduction for benefits or deductions not paid in the period,
        /// but paid at some point in the year to date.
        /// </summary>
        /// <param name="benefitDeductionType"></param>
        /// <param name="yearToDateBenefitDeductionEntries"></param>
        public PayStatementDeduction(BenefitDeductionType benefitDeductionType,
            IEnumerable<PayrollRegisterBenefitDeductionEntry> yearToDateBenefitDeductionEntries)
        {
            if (benefitDeductionType == null)
            {
                throw new ArgumentNullException("benefitDeductionType");
            }
            if (yearToDateBenefitDeductionEntries == null)
            {
                throw new ArgumentNullException("yearToDateBenefitDeductionEntries");
            }


            Code = benefitDeductionType.Id;
            Description = string.IsNullOrWhiteSpace(benefitDeductionType.SelfServiceDescription) ? benefitDeductionType.Description : benefitDeductionType.SelfServiceDescription;
            Type = benefitDeductionType.Category == BenefitDeductionTypeCategory.Benefit ? PayStatementDeductionType.Benefit : PayStatementDeductionType.Deduction;

            this.yearToDateBenefitDeductionEntries = yearToDateBenefitDeductionEntries.Where(bd => bd.BenefitDeductionId == Code).ToList();
        }        
    }
}
