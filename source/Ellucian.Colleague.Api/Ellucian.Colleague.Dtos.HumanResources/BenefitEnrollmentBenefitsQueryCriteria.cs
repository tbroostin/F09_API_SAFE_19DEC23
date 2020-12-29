/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Query criteria class for querying benefit enrollment benefits
    /// </summary>
    public class BenefitEnrollmentBenefitsQueryCriteria
    {
        /// <summary>
        /// Enrollment period id
        /// </summary>
        public string EnrollmentPeriodId { get; set; }
        /// <summary>
        /// Enrollment Period Benefit Package Id
        /// </summary>
        public string PackageId { get; set; }
        /// <summary>
        /// Benefit Type id
        /// </summary>
        public string BenefitTypeId { get; set; }
        /// <summary>
        /// Collection of benefit ids
        /// </summary>
        public List<string> EnrollmentPeriodBenefitIds { get; set; }
    }
}
