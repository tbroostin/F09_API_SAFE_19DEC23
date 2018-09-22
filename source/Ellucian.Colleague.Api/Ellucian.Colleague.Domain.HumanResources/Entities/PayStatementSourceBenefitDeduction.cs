/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

//TODO mcd: Class no longer used. Can be deleted.
namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Pay Statement Source Benefit/Deduction
    /// </summary>
    [Serializable]
    public class PayStatementSourceBenefitDeduction
    {
        public string BenefitDeductionCode { get; private set; }
        public string BenefitDeductionDescription { get; private set; }
        public decimal? EmployeePeriodDeductionAmount { get; private set; } // calc
        public decimal? EmployerPeriodDeductionAmount { get; private set; }
        public decimal? EmployeePeriodDeductionBaseAmount { get; private set; }  //base
        public decimal? EmployerPeriodDeductionBaseAmount { get; private set; }
        public decimal? PeriodApplicableBenefitDeductionGross { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="benefitDeductionCode"></param>
        /// <param name="benefitDeductionDescription"></param>
        /// <param name="employeePeriodDeductionAmount"></param>
        /// <param name="employerPeriodDeductionAmount"></param>
        /// <param name="employeePeriodDeductionBaseAmount"></param>
        /// <param name="employerPeriodDeductionBaseAmount"></param>
        /// <param name="periodApplicableBenefitDeductionGross"></param>
        public PayStatementSourceBenefitDeduction(
            string benefitDeductionCode, string benefitDeductionDescription, 
            decimal? employeePeriodDeductionAmount, decimal? employerPeriodDeductionAmount, 
            decimal? employeePeriodDeductionBaseAmount, decimal? employerPeriodDeductionBaseAmount
        )
        {
            if (string.IsNullOrWhiteSpace(benefitDeductionCode))
            {
                throw new ArgumentNullException("benefitDeductionCode");
            }

            if (string.IsNullOrWhiteSpace(benefitDeductionDescription))
            {
                throw new ArgumentNullException("benefitDeductionDescription");
            }

            BenefitDeductionCode = benefitDeductionCode;
            BenefitDeductionDescription = benefitDeductionDescription;
            EmployeePeriodDeductionAmount = employeePeriodDeductionAmount;
            EmployerPeriodDeductionAmount = employerPeriodDeductionAmount;
            EmployeePeriodDeductionBaseAmount = employeePeriodDeductionBaseAmount;
            EmployerPeriodDeductionBaseAmount = employerPeriodDeductionBaseAmount;
            PeriodApplicableBenefitDeductionGross = employeePeriodDeductionBaseAmount ?? employerPeriodDeductionBaseAmount;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            return BenefitDeductionCode == ((PayStatementSourceBenefitDeduction)obj).BenefitDeductionCode;
        }

        public override int GetHashCode()
        {
            return BenefitDeductionCode.GetHashCode();
        }
    }
}
