/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// EmployeeBenefitsEnrollmentPackage holds benefits details 
    /// available to the employee
    /// </summary>
    public class EmployeeBenefitsEnrollmentPackage
    {
        /// <summary>
        /// Employee id
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// Benefit Enrollment package id
        /// </summary>
        public string PackageId { get; set; }
        /// <summary>
        /// Package Description
        /// </summary>
        public string PackageDescription { get; set; }
        /// <summary>
        /// Benefits enrollment period id
        /// </summary>
        public string BenefitsEnrollmentPeriodId { get; set; }

        /// <summary>
        /// List of benefit types employee is eligible to enroll in
        /// </summary>
        public IEnumerable<EmployeeBenefitType> EmployeeEligibleBenefitTypes { get; set; }
    }
}
