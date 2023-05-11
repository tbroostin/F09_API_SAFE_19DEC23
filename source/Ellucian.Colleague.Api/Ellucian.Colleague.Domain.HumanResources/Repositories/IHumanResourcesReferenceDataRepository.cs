//Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.HumanResources.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IHumanResourcesReferenceDataRepository
    {
        /// <summary>
        /// Get a collection of bargaining units
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of bargaining units</returns>
        Task<IEnumerable<BargainingUnit>> GetBargainingUnitsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of BeneficiaryTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of BeneficiaryTypes</returns>
        Task<IEnumerable<BeneficiaryTypes>> GetBeneficiaryTypesAsync(bool ignoreCache);

        /// <summary>
        /// Gets a list of all benefits and deductions
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<BenefitDeductionType>> GetBenefitDeductionTypesAsync();

        /// <summary>
        /// Get a collection of CostCalculationMethod
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CostCalculationMethod</returns>
        Task<IEnumerable<CostCalculationMethod>> GetCostCalculationMethodsAsync(bool ignoreCache);

        /// <summary>
        /// Gets a list of tax code / description pairs
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TaxCode>> GetTaxCodesAsync();

        /// <summary>
        /// Get a collection of employee classifications
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of employee classifications</returns>
        Task<IEnumerable<EmploymentClassification>> GetEmploymentClassificationsAsync(bool ignoreCache);

        /// <summary>
        /// Get all employment departments
        /// </summary>
        /// <param name="bypassCache">Boolean to bypass use of cached data and go directly to disk.</param>
        /// <returns>Collection of <see cref="EmploymentDepartment"/></returns>
        Task<IEnumerable<EmploymentDepartment>> GetEmploymentDepartmentsAsync(bool bypassCache);

        /// <summary>
        /// Get guid for EmploymentFrequencies.
        /// </summary>
        /// <param name="code">EmploymentFrequencies code</param>
        /// <returns>Guid</returns>
        Task<string> GetEmploymentFrequenciesGuidAsync(string code);

        /// <summary>
        /// Get a collection of EmploymentFrequency
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of EmploymentFrequency</returns>
        Task<IEnumerable<EmploymentFrequency>> GetEmploymentFrequenciesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of employee proficiencies
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of employee proficiencies</returns>
        Task<IEnumerable<EmploymentProficiency>> GetEmploymentProficienciesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of DeductionCategory
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of DeductionCategory</returns>
        Task<IEnumerable<DeductionCategory>> GetDeductionCategoriesAsync(bool ignoreCache);


        /// <summary>
        /// Get guid for DeductionTypes code
        /// </summary>
        /// <param name="code">DeductionTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetDeductionTypesGuidAsync(string code);

        /// <summary>
        /// Get all deduction types
        /// </summary>
        /// <param name="bypassCache">Boolean to bypass use of cached data and go directly to disk.</param>
        /// <returns>Collection of <see cref="DeductionType"/></returns>
        Task<IEnumerable<DeductionType>> GetDeductionTypesAsync(bool bypassCache);

        /// <summary>
        /// Get all deduction types
        /// </summary>
        /// <param name="bypassCache">Boolean to bypass use of cached data and go directly to disk.</param>
        /// <returns>Collection of <see cref="DeductionType"/></returns>
        Task<IEnumerable<DeductionType>> GetDeductionTypes2Async(bool bypassCache);

        /// <summary>
        /// Get a collection of EarningType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of EarningType</returns>
        Task<IEnumerable<EarningType2>> GetEarningTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of EmploymentPerformanceReviewRating
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of EmploymentPerformanceReviewRating</returns>
        Task<IEnumerable<EmploymentPerformanceReviewRating>> GetEmploymentPerformanceReviewRatingsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of EmploymentPerformanceReviewTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of EmploymentPerformanceReviewTypes</returns>
        Task<IEnumerable<EmploymentPerformanceReviewType>> GetEmploymentPerformanceReviewTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get all employment status ending reasons
        /// </summary>
        /// <param name="bypassCache">Boolean to bypass use of cached data and go directly to disk.</param>
        /// <returns>Collection of <see cref="EmploymentStatusEndingReason"/></returns>
        Task<IEnumerable<EmploymentStatusEndingReason>> GetEmploymentStatusEndingReasonsAsync(bool bypassCache);

        /// <summary>
        /// Get guid for employment status ending reasons code
        /// </summary>
        /// <param name="code">AcadCredentials code</param>
        /// <returns>Guid</returns>
        Task<string> GetEmploymentStatusEndingReasonsGuidAsync(string code);


        /// <summary>
        /// Get guid for leave type.
        /// </summary>
        /// <param name="code">LeaveType code</param>
        /// <returns>Guid</returns>
        Task<string> GetLeaveTypesGuidAsync(string code);

        /// <summary>
        /// Get a collection of LeaveType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of LeaveType</returns>
        Task<IEnumerable<LeaveType>> GetLeaveTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of Payclass
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Payclass</returns>
        Task<IEnumerable<PayClass>> GetPayClassesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for Payclass
        /// </summary>
        /// <param name="code">Payclass code</param>
        /// <returns>Guid</returns>
        Task<string> GetPayClassesGuidAsync(string code);

        /// <summary>
        /// Get a collection of PayCycles
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of PayCycles</returns>
        Task<IEnumerable<PayCycle2>> GetPayCyclesAsync(bool ignoreCache);


        /// <summary>
        /// Get guid for PayrollDeductionArrangementChangeReasons
        /// </summary>
        /// <param name="code">ayrollDeductionArrangementChangeReasons code</param>
        /// <returns>Guid</returns>
        Task<string> GetPayrollDeductionArrangementChangeReasonsGuidAsync(string code);


        /// <summary>
        /// Get all payroll deduction arrangement change reasons
        /// </summary>
        /// <param name="bypassCache">Boolean to bypass use of cached data and go directly to disk.</param>
        /// <returns>Collection of <see cref="PayrollDeductionArrangementChangeReason"/></returns>
        Task<IEnumerable<PayrollDeductionArrangementChangeReason>> GetPayrollDeductionArrangementChangeReasonsAsync(bool bypassCache);
               
        /// <summary>
        /// Get a collection of job change reasons
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of job change reasons</returns>
        Task<IEnumerable<JobChangeReason>> GetJobChangeReasonsAsync(bool ignoreCache);
        
        /// <summary>
        /// Get a collection of rehire types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of rehire types</returns>
        Task<IEnumerable<RehireType>> GetRehireTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get a specific GUID from a Record Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string> GetGuidFromID(string key, string entity);

        /// <summary>
        /// Get a collection of HrStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of HrStatuses</returns>
        Task<IEnumerable<HrStatuses>> GetHrStatusesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of TenureTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of TenureTypes</returns>
        Task<IEnumerable<TenureTypes>> GetTenureTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get the configuration data for use with pay statements
        /// </summary>
        /// <returns>Pay Statement Configuration Object</returns>
        Task<PayStatementConfiguration> GetPayStatementConfigurationAsync();

        /// <summary>
        /// Get all EarningsDifferentials
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<EarningsDifferential>> GetEarningsDifferentialsAsync();

        /// <summary>
        /// Gets the Assignment Contract Types
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Domain.HumanResources.Entities.AsgmtContractTypes>> GetAssignmentContractTypesAsync(bool ignoreCache);

        /// <summary>
        /// Gets all EarningsTypeGroups
        /// </summary>
        /// <returns>Dictionary of EarningsTypeGroups where the key is the EarningsTypeGroup id </returns>
        Task<IDictionary<string, EarningsTypeGroup>> GetEarningsTypesGroupsAsync();

        /// <summary>
        /// Gets all beneficiary categories
        /// </summary>
        /// <returns>List of Beneficiary Category Objects</returns>
        Task<IEnumerable<BeneficiaryCategory>> GetBeneficiaryCategoriesAsync();

        /// <summary>
        /// Get all TimeUnits from HR.VALCODES
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns>Collection of TimeUnits Entities</returns>
        Task<IEnumerable<TimeUnits>> GetTimeUnitsAsync(bool ignoreCache);

        /// <summary>
        /// Gets the Configuration values from HRSS Defaults Entity in the database        
        /// </summary>
        /// <returns>HrssConfiguration Entity</returns>
        Task<HRSSConfiguration> GetHrssConfigurationAsync();
    }
}
