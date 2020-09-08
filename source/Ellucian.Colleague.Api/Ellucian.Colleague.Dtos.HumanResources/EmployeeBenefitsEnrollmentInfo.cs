/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Employee Benefit Enrollment Information DTO
    /// </summary>
    public class EmployeeBenefitsEnrollmentInfo
    {
        /// <summary>
        /// Unique ID of record
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Employee's Id
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Benefit enrollment Period
        /// </summary>
        public string EnrollmentPeriodId { get; set; }

        /// <summary>
        /// Benefit enrollment package
        /// </summary>
        public string BenefitPackageId { get; set; }

        /// <summary>
        /// Benefit enrollment confirmation date
        /// </summary>
        public DateTime? ConfirmationDate { get; set; }

        /// <summary>
        /// Benefit types opted out
        /// </summary>
        public IEnumerable<string> OptOutBenefitTypes { get; set; }

        /// <summary>
        /// The benefit type of the benefit
        /// </summary>
        public string BenefitType { get; set; }

        /// <summary>
        /// Indicates if the benefit type is waived or not
        /// </summary>
        public bool IsWaived { get; set; }

        /// <summary>
        /// List of benefit enrollment detail record associated to this enrollment
        /// </summary>
        public IEnumerable<EmployeeBenefitsEnrollmentDetail> EmployeeBenefitEnrollmentDetails { get; set; }

    }
}
