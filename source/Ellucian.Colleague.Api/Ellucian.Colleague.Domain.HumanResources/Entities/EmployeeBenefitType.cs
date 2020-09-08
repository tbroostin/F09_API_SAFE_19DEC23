/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Employee Benefit
    /// </summary>
    [Serializable]
    public class EmployeeBenefitType
    {
        /// <summary>
        /// Benefit type
        /// </summary>
        public string BenefitType { get { return benefitType; } }
        private readonly string benefitType;
        /// <summary>
        /// Benefit Description
        /// </summary>
        public string BenefitTypeDescription { get { return benefitTypeDescription; } }
        private readonly string benefitTypeDescription;

        /// <summary>
        /// The minimum number of benefits required for enrollment
        /// </summary>
        public int? MinBenefits { get; set; }

        /// <summary>
        /// The maximum number of benefits required for enrollment
        /// </summary>
        public int? MaxBenefits { get; set; }

        /// <summary>
        /// Allow opt out flag
        /// </summary>
        public bool AllowOptOut { get; set; }

        /// <summary>
        /// Benefit opt out text
        /// </summary>
        public string BenefitOptOutText { get; set; }

        /// <summary>
        /// Special processing code3 for benefit type icon
        /// </summary>
        public string BenefitTypeSpecialProcessingCode { get; set; }

        /// <summary>
        /// List of enrollment period benefit ids under this benefit type
        /// </summary>
        public List<EnrollmentPeriodBenefit> EnrollmentPeriodBenefitIds { get; set; }

        /// <summary>
        /// SS Benefits Selection Page Text from BETD
        /// </summary>
        public string BenefitsSelectionPageCustomText { get; set; }

        /// <summary>
        /// EmployeeBenefitType constructor
        /// </summary>
        /// <param name="benefitType"></param>
        /// <param name="benefitTypeDescription"></param>
        public EmployeeBenefitType(string benefitType, string benefitTypeDescription)
        {
            if (string.IsNullOrEmpty(benefitType))
            {
                throw new ArgumentNullException("benefitType");
            }
            if (string.IsNullOrEmpty(benefitTypeDescription))
            {
                throw new ArgumentNullException("benefitDescription");
            }
            this.benefitType = benefitType;
            this.benefitTypeDescription = benefitTypeDescription;
            EnrollmentPeriodBenefitIds = new List<EnrollmentPeriodBenefit>();
        }

    }
}
