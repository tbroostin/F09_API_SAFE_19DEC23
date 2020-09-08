/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for BenefitsEnrollment Services methods
    /// </summary>
    public interface IBenefitsEnrollmentService : IBaseService
    {
        /// <summary>
        /// Returns EmployeeBenefitsEnrollmentEligibility object
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment eligibility</param>
        /// <returns>EmployeeBenefitsEnrollmentEligibility DTO containing benefits enrollment eligibility information</returns>
        Task<EmployeeBenefitsEnrollmentEligibility> GetEmployeeBenefitsEnrollmentEligibilityAsync(string employeeId);

        /// <summary>
        /// ReturnsEmployeeBenefitsEnrollmentPool object
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment information</param>
        /// <returns>EmployeeBenefitsEnrollmentPool DTO containing benefits enrollment dependent and beneficiary pool information</returns>
        Task<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>> GetEmployeeBenefitsEnrollmentPoolAsync(string employeeId);

        /// <summary>
        /// Gets EmployeeBenefitsEnrollmentPackage object for the specified employeeId
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="enrollmentPeriodId">optional</param>
        /// <returns></returns>
        Task<EmployeeBenefitsEnrollmentPackage> GetEmployeeBenefitsEnrollmentPackageAsync(string employeeId, string enrollmentPeriodId = null);

        /// <summary>
        /// Adds new benefits enrollment pool information to an employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"></param>
        /// <returns>Newly added EmployeeBenefitsEnrollmentPoolItem DTO</returns>
        Task<EmployeeBenefitsEnrollmentPoolItem> AddEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem);

        /// <summary>
        /// Updates benefits enrollment pool information of an employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"></param>
        /// <returns>Updated EmployeeBenefitsEnrollmentPoolItem DTO</returns>
        Task<EmployeeBenefitsEnrollmentPoolItem> UpdateEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem);

        /// <summary>
        /// Queries enrollment period benefits based on specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        Task<IEnumerable<EnrollmentPeriodBenefit>> QueryEnrollmentPeriodBenefitsAsync(BenefitEnrollmentBenefitsQueryCriteria criteria);

        /// <summary>
        /// Updates the given benefits enrollment information
        /// </summary>
        /// <param name="employeeBenefitEnrollmentInfoDto"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object</returns>
        Task<EmployeeBenefitsEnrollmentInfo> UpdateEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfo employeeBenefitEnrollmentInfoDto);

        /// <summary>
        /// Queries employee benefits enrollment info based on specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object based on critiera</returns>
        Task<EmployeeBenefitsEnrollmentInfo> QueryEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfoQueryCriteria criteria);

        /// <summary>
        /// Submits/Re-opens the benefits elected by an employee.
        /// </summary>
        /// <param name="criteria">BenefitEnrollmentCompletionCriteria object</param>
        /// <returns>BenefitEnrollmentCompletionInfo DTO</returns>
        Task<BenefitEnrollmentCompletionInfo> SubmitOrReOpenBenefitElectionsAsync(BenefitEnrollmentCompletionCriteria criteria);

        /// <summary>
        /// Gets the beneficiary categories
        /// </summary>
        /// <returns>Returns a list of Beneficiary category DTO</returns>
        Task<IEnumerable<BeneficiaryCategory>> GetBeneficiaryCategoriesAsync();

        /// <summary>
        /// Gets benefits information for acknowledgement report
        /// </summary>
        /// <returns></returns>
        Task<Byte[]> GetBenefitsInformationForAcknowledgementReport(string employeeId, string path, string resourceFilePath);
    }
}
