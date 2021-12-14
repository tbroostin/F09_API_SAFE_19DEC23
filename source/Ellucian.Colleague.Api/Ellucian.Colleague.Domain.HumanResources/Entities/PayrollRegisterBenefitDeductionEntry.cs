/*Copyright 2017-2021 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Describes the specific benefits a person paid (taken from their earnings) and the deductions taken from a person's earnings
    /// during a pay period
    /// </summary>
    [Serializable]
    public class PayrollRegisterBenefitDeductionEntry
    {
        /// <summary>
        /// The id of the BenefitDeduction object
        /// </summary>
        public string BenefitDeductionId { get; private set; }

        /// <summary>
        /// The amount the employee paid to this Benefit/Deduction
        /// </summary>
        public decimal? EmployeeAmount { get; set; }

        /// <summary>
        /// The basis amount on which the employee amount was calculated
        /// </summary>
        public decimal? EmployeeBasisAmount { get; set; }

        /// <summary>
        /// The amount the employer paid to this Benefit/Deduction
        /// </summary>
        public decimal? EmployerAmount { get; set; }


        /// <summary>
        /// The basis amount on which the employer amount was calculated
        /// </summary>
        public decimal? EmployerBasisAmount { get; set; }
        /// <summary>
        /// Employee Adjustment Amount if this is an adjustment
        /// </summary>
        public decimal? EmployeeAdjustmentAmount { get; set; }

        /// <summary>
        /// Employer Adjustment Amount if this is an adjustment
        /// </summary>
        public decimal? EmployerAdjustmentAmount { get; set; }

        /// <summary>
        /// Employee Basis Adjustment Amount if this is an adjustment
        /// </summary>
        public decimal? EmployeeBasisAdjustmentAmount { get; set; }

        /// <summary>
        /// Employer Basis Adjustment Amount if this is an adjustment
        /// </summary>
        public decimal? EmployerBasisAdjustmentAmount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="benefitDeductionId"></param>
        public PayrollRegisterBenefitDeductionEntry(string benefitDeductionId)
        {
            if (string.IsNullOrEmpty(benefitDeductionId))
            {
                throw new ArgumentNullException("benefitDeductionId");
            }

            BenefitDeductionId = benefitDeductionId;
        }
    }
}
