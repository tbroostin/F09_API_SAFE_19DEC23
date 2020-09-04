/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    /// <summary>
    /// Interface for BenefitsEnrollment repository methods
    /// </summary>
    public interface IBenefitsEnrollmentRepository
    {
        /// <summary>
        /// Returns EmployeeBenefitsEnrollmentEligibility object
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment eligibility</param>
        /// <returns>EmployeeBenefitsEnrollmentEligibility Domain Entity containing benefits enrollment eligibility information</returns>
        Task<EmployeeBenefitsEnrollmentEligibility> GetEmployeeBenefitsEnrollmentEligibilityAsync(string employeeId);

        /// <summary>
        /// ReturnsEmployeeBenefitsEnrollmentPool object
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment information</param>
        /// <returns>EmployeeBenefitsEnrollmentPool Domain Entity containing benefits enrollment dependent and beneficiary pool information</returns>
        Task<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>> GetEmployeeBenefitsEnrollmentPoolAsync(string employeeId);

        /// <summary>
        /// Gets EmployeeBenefitsEnrollmentPackage for specified employee id
        /// </summary>
        /// <param name="employeeId">employee id for whom to get the enrollment package</param>
        /// <param name="enrollmentPeriodId">optional</param>
        /// <returns></returns>
        Task<EmployeeBenefitsEnrollmentPackage> GetEmployeeBenefitsEnrollmentPackageAsync(string employeeId, string enrollmentPeriodId = null);

        /// <summary>
        /// Adds new benefits enrollment pool information to an employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"></param>
        /// <returns>Newly added EmployeeBenefitsEnrollmentPoolItem Entity</returns>
        Task<EmployeeBenefitsEnrollmentPoolItem> AddEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem);

        /// <summary>
        /// Updates benefits enrollment pool information of an employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"></param>
        /// <returns>Updated EmployeeBenefitsEnrollmentPoolItem Entity</returns>
        Task<EmployeeBenefitsEnrollmentPoolItem> UpdateEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem);

        /// <summary>
        /// This checks whether the dependent with id exists or not
        /// </summary>
        /// <param name="benefitsEnrollmentPoolId">BenefitsEnrollmentPoolId</param>
        /// <returns>true if dependent exists or else false</returns>
        Task<bool> CheckDependentExistsAsync(string benefitsEnrollmentPoolId);

        /// <summary>
        /// Queries enrollment period benefits based on specified criteria
        /// </summary>
        /// <param name="benefitTypeId"></param>
        /// <param name="enrollmentPeriodId"></param>
        /// <param name="packageId"></param>
        /// <param name="enrollmentPeriodBenefitIds"></param>
        /// <returns></returns>
        Task<IEnumerable<EnrollmentPeriodBenefit>> QueryEnrollmentPeriodBenefitsAsync(string benefitTypeId,
            string enrollmentPeriodId = null,
            string packageId = null,
            List<string> enrollmentPeriodBenefitIds = null);

        /// <summary>
        /// Queries employee benefits enrollment information based on specified criteria
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="enrollmentPeriodId"></param>
        /// <param name="benefitTypeId"></param>
        /// <param name="includeCancelActions"></param>
        /// <param name="includeOptOutActions"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object</returns>
        Task<EmployeeBenefitsEnrollmentInfo> QueryEmployeeBenefitsEnrollmentInfoAsync(string employeeId, string enrollmentPeriodId, string benefitTypeId);

        /// <summary>
        /// Updates the given benefits enrollment information
        /// </summary>
        /// <param name="employeeBenefitEnrollmentInfo"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object</returns>
        Task<EmployeeBenefitsEnrollmentInfo> UpdateEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfo employeeBenefitEnrollmentInfo);

        /// <summary>
        /// Submits the benefits elected by an employee, for enrollment completion process
        /// </summary>
        /// <param name="employeeId">Employee Id of the user</param>
        /// <param name="enrollmentPeriodId">Id of the current enrollment period</param>
        /// <param name="benefitPackageId">Id of the benefits package associated with the enrollment period</param>
        /// <returns>BenefitEnrollmentCompletionInfo Entity</returns>
        Task<BenefitEnrollmentCompletionInfo> SubmitBenefitElectionAsync(string employeeId, string enrollmentPeriodId, string benefitPackageId);

        /// <summary>
        /// Re-opens the benefits elected by an employee.
        /// </summary>
        /// <param name="employeeId">Employee Id of the user</param>
        /// <param name="enrollmentPeriodId">Id of the current enrollment period</param>    
        /// <returns>BenefitEnrollmentCompletionInfo Entity</returns>
        Task<BenefitEnrollmentCompletionInfo> ReOpenBenefitElectionsAsync(string employeeId, string enrollmentPeriodId);
    }
}
