/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
