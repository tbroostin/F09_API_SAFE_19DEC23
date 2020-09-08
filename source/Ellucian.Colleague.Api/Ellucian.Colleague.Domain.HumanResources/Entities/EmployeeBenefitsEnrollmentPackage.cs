/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// EmployeeBenefitsEnrollmentPackage holds benefits details 
    /// available to the employee
    /// </summary>
    [Serializable]
    public class EmployeeBenefitsEnrollmentPackage
    {
        /// <summary>
        /// Employee id
        /// </summary>
        public string EmployeeId { get { return employeeId; } }
        private readonly string employeeId;
        /// <summary>
        /// Benefit Enrollment package id
        /// </summary>
        public string PackageId { get { return packageId; } }
        private readonly string packageId;
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

        /// <summary>
        /// EmployeeBenefitsEnrollmentPackage constructor
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="packageId"></param>
        public EmployeeBenefitsEnrollmentPackage(string employeeId, string packageId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            if (string.IsNullOrEmpty(packageId))
            {
                throw new ArgumentNullException("packageId");
            }
            this.employeeId = employeeId;
            this.packageId = packageId;
            EmployeeEligibleBenefitTypes = new List<EmployeeBenefitType>();
        }
    }
}
