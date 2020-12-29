/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Employee Benefit
    /// </summary>
    public class EmployeeBenefitType
    {
        /// <summary>
        /// Benefit type
        /// </summary>
        public string BenefitType { get; set; }
        /// <summary>
        /// Benefit Description
        /// </summary>
        public string BenefitTypeDescription { get; set; }
        /// <summary>
        /// The minimum number of benefits required for enrollment
        /// </summary>
        public int? MinBenefits { get; set; }
        /// <summary>
        /// The maximum number of benefits required for enrollment
        /// </summary>
        public int? MaxBenefits { get; set; }
        /// <summary>
        /// Allow Opt out
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
        /// List of enrollment period benefit ids for the type
        /// </summary>
        public IEnumerable<EnrollmentPeriodBenefit> EnrollmentPeriodBenefitIds { get; set; }

        /// <summary>
        /// SS Benefits Selection Page Text from BETD
        /// </summary>
        public string BenefitsSelectionPageCustomText { get; set; }

    }
}
