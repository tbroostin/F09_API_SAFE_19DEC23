/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Benefit Type benefit class
    /// </summary>
    public class EnrollmentPeriodBenefit : Benefit
    {
        /// <summary>
        /// Enrollment Period benefit id - different from BenefitId(BENDED ID)
        /// </summary>
        public string EnrollmentPeriodBenefitId { get; set; }

        /// <summary>
        /// Benefit Type Id this benefit is associated with
        /// </summary>
        public string BenefitTypeId { get; set; }

        /// <summary>
        /// Flag to check if dependents is required
        /// </summary>
        public bool IsDependentRequired { get; set; }

        /// <summary>
        /// Flag to check if beneficiary is required
        /// </summary>
        public bool IsBeneficiaryRequired { get; set; }

        /// <summary>
        /// Flag to check if health care provider information is required
        /// </summary>
        public bool IsHealthCareProviderRequired { get; set; }

        /// <summary>
        /// The required information for calculating the benefit cost:
        /// Amount - benefit amount is required
        /// Insurance - insurance amount is required
        /// Percentage - percentage for benefit is required
        /// Flex - flex spending amount is required for benefit
        /// None - no cost amount is required for benefit
        /// </summary>
        public PeriodBenefitCostCalculationRequirement CostCalculationRequirement { get; set; }

        /// <summary>
        /// If the insurance coverage is based on a multiple of the insured's salary, then this specifies the multiple for the employee's share
        /// </summary>
        public decimal? InsuranceMultiplier { get; set; }

        /// <summary>
        /// Specifies the units in which to purchase insurance
        /// </summary>
        public decimal? InsuranceRateDivisor { get; set; }

        /// <summary>
        /// Minimum benefit amount to specify
        /// </summary>
        public decimal? MinimumBenefitAmount { get; set; }

        /// <summary>
        /// Maximum benefit amount to specify
        /// </summary>
        public decimal? MaximumBenefitAmount { get; set; }

        /// <summary>
        /// Maximum benefit AR amount to specify
        /// </summary>
        public decimal? MaximumARBenefitAmount { get; set; }

        /// <summary>
        /// Maximum benefit amount for a pay period
        /// </summary>
        public decimal? MaximumPayPeriodAmount { get; set; }

        /// <summary>
        /// Maximum benefit percent
        /// </summary>
        public decimal? MaximumBenefitPercent { get; set; }

        /// <summary>
        /// Plan hyperlink associated to the plan
        /// </summary>
        public string PlanHyperlink { get; set; }

        /// <summary>
        /// Rate hyperlink associated to the plan
        /// </summary>
        public string RateHyperlink { get; set; }

    }
}
