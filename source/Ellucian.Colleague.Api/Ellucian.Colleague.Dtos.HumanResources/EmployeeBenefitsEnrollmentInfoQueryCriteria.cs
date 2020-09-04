/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Query criteria class for querying benefit enrollment information
    /// </summary>
    public class EmployeeBenefitsEnrollmentInfoQueryCriteria
    {
        /// <summary>
        /// Employee Id for benefits 
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// Enrollment period id
        /// </summary>
        public string EnrollmentPeriodId { get; set; }
        /// <summary>
        /// Benefit Type id
        /// </summary>
        public string BenefitTypeId { get; set; }
    }
}
