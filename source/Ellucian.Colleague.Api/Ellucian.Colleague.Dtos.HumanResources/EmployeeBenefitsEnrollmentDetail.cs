/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Employee Benefit Enrollment Detail DTO
    /// </summary>
    public class EmployeeBenefitsEnrollmentDetail
    {
        /// <summary>
        /// Unique ID of record
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Benefit type id
        /// </summary>
        public string BenefitTypeId { get; set; }

        /// <summary>
        /// Benefit type id
        /// </summary>
        public DateTime? BenefitTypeEffectiveDate { get; set; }

        /// <summary>
        /// Period benefit id
        /// </summary>
        public string PeriodBenefitId { get; set; }

        /// <summary>
        /// Benefit code
        /// </summary>
        public string BenefitId { get; set; }

        /// <summary>
        /// Benefit description
        /// </summary>
        public string BenefitDescription { get; set; }

        /// <summary>
        /// Level of coverage
        /// </summary>
        public List<string> CoverageLevels { get; set; }

        /// <summary>
        /// Election action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Description of election action
        /// </summary>
        public string ActionDescription { get; set; }

        /// <summary>
        /// Flexible benefit required indicator
        /// </summary>
        public bool FlexBenefitRequired { get; set; }

        /// <summary>
        /// Names of dependents
        /// </summary>
        public List<string> DependentNames { get; set; }

        /// <summary>
        /// Work Ids of dependents associated to detail record
        /// </summary>
        public List<string> DependentIds { get; set; }

        /// <summary>
        /// Pool Ids of dependents associated to detail record
        /// </summary>
        public List<string> DependentPoolIds { get; set; }

        /// <summary>
        /// Provider IDs for dependents
        /// </summary>
        public List<string> DependentProviderIds { get; set; }

        /// <summary>
        /// Provider names for dependents
        /// </summary>
        public List<string> DependentProviderNames { get; set; }

        /// <summary>
        /// Names of beneficiaries
        /// </summary>
        public List<string> BeneficiaryNames { get; set; }

        /// <summary>
        /// Work Ids of beneficiaries associated to detail record
        /// </summary>
        public List<string> BeneficiaryIds { get; set; }

        /// <summary>
        /// Pool Ids of beneficiaries associated to detail record
        /// </summary>
        public List<string> BeneficiaryPoolIds { get; set; }

        /// <summary>
        /// Type associated to beneficiary
        /// </summary>
        public List<string> BeneficiaryTypes { get; set; }

        /// <summary>
        /// Percent associated to beneficiary
        /// </summary>
        public List<decimal?> BeneficiaryPercent { get; set; }

        /// <summary>
        /// A list containing all information related to the beneficiaries for a given benefit
        /// </summary>
        public List<string> BeneficiaryDisplayInformation { get; set; }

        /// <summary>
        /// The amount entered for an elected benefit
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// The percent entered for an elected benefit
        /// </summary>
        public decimal? Percent { get; set; }

        /// <summary>
        ///  Insurance Coverage Amount
        /// </summary>
        public decimal? InsuranceCoverageAmount { get; set; }

        /// <summary>
        /// Amount for flex benefit
        /// </summary>
        public decimal? FlexibleBenefitAmount { get; set; }

        /// <summary>
        /// Ids of health care provider for employee
        /// </summary>
        public string EmployeeProviderId { get; set; }

        /// <summary>
        /// Names of health care provider for employee
        /// </summary>
        public string EmployeeProviderName { get; set; }
    }
}
