/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO class for EmployeeBenefitsEnrollmentEligibility
    /// </summary>
    public class EmployeeBenefitsEnrollmentEligibility
    {
        /// <summary>
        /// Employee id of user
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Enrollment period the employee is eligible for
        /// </summary>
        public string EligibilityPeriod { get; set; }

        /// <summary>
        /// Start date of benefits enrollment period
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date of benefits enrollment period
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Description of enrollment period
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Reason employee is not eligible for benefits enrollment
        /// </summary>
        public string IneligibleReason { get; set; }

        /// <summary>
        /// Package the employee is eligible for
        /// </summary>
        public string EligibilityPackage { get; set; }

        /// <summary>
        /// Enrollment Confirmation Text from BENP
        /// </summary>
        public List<string> EnrollmentConfirmationText { get; set; }

        /// <summary>
        /// Confirmation Complete Text from BENP
        /// </summary>
        public List<string> ConfirmationCompleteText { get; set; }

        /// <summary>
        /// SS Benefits Page Text from BENP
        /// </summary>
        public List<string> BenefitsPageCustomText { get; set; }

        /// <summary>
        /// SS Benefits Enrollment Page Text from BENP
        /// </summary>
        public List<string> BenefitsEnrollmentPageCustomText { get; set; }

        /// <summary> 
        /// SS Manage Dep/Ben Page Text from BENP
        /// </summary>
        public List<string> ManageDepBenPageCustomText { get; set; }

        /// <summary>
        /// Flag indicating whether the employee has started their 
        /// enrollment in the current period
        /// </summary>
        public bool IsEnrollmentInitiated { get; set; }
        /// <summary>
        /// Flag indicating whether the enrollment package selections
        /// have been submitted
        /// </summary>
        public bool IsPackageSubmitted { get; set; }
    }
}