/*Copyright 2019-2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Dependency;
using DomainEntities = Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Ellucian.Colleague.Coordination.Base.Reports;
using Ellucian.Colleague.Coordination.Base.Utility;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Service class for Benefits Enrollment
    /// </summary>
    [RegisterType]
    public class BenefitsEnrollmentService : BaseCoordinationService, IBenefitsEnrollmentService
    {
        private readonly IBenefitsEnrollmentRepository benefitsEnrollmentRepository;
        private readonly IHumanResourcesReferenceDataRepository humanResourceRepository;

        private const string waivedPlanDescription = "Waived";

        public BenefitsEnrollmentService(IBenefitsEnrollmentRepository benefitsEnrollmentRepository, IHumanResourcesReferenceDataRepository humanResourceRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository = null, IConfigurationRepository configurationRepository = null) : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            this.benefitsEnrollmentRepository = benefitsEnrollmentRepository;
            this.humanResourceRepository = humanResourceRepository;
        }

        /// <summary>
        /// Returns EmployeeBenefitsEnrollmentEligibility object
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment eligibility</param>
        /// <returns>EmployeeBenefitsEnrollmentEligibility DTO containing benefits enrollment eligibility information</returns>
        public async Task<EmployeeBenefitsEnrollmentEligibility> GetEmployeeBenefitsEnrollmentEligibilityAsync(string employeeId)
        {
            if (!CurrentUser.IsPerson(employeeId))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view EmployeeBenefitsEnrollmentEligibility for person {1}", CurrentUser.PersonId, employeeId));
            }

            var employeeBenefitsEnrollmentEligibilityEntity = await benefitsEnrollmentRepository.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
            logger.Debug(string.Format("******employeeBenefitsEnrollmentEligibilityEntity obtained for {0}********", employeeId));
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EmployeeBenefitsEnrollmentEligibility, Dtos.HumanResources.EmployeeBenefitsEnrollmentEligibility>();
            var employeeBenefitsEnrollmentEligibilitydto = entityToDtoAdapter.MapToType(employeeBenefitsEnrollmentEligibilityEntity);
            logger.Debug("********employeeBenefitsEnrollmentEligibilitydto obtained successfully********");
            return employeeBenefitsEnrollmentEligibilitydto;
        }

        /// <summary>
        /// ReturnsEmployeeBenefitsEnrollmentPool object
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment information</param>
        /// <returns>EmployeeBenefitsEnrollmentPool DTO containing benefits enrollment dependent and beneficiary pool information</returns>
        public async Task<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>> GetEmployeeBenefitsEnrollmentPoolAsync(string employeeId)
        {
            if (!CurrentUser.IsPerson(employeeId))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view EmployeeBenefitsEnrollmentPool for person {1}", CurrentUser.PersonId, employeeId));
            }

            var employeeBenefitsEnrollmentPooldtos = new List<EmployeeBenefitsEnrollmentPoolItem>();
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EmployeeBenefitsEnrollmentPoolItem, Dtos.HumanResources.EmployeeBenefitsEnrollmentPoolItem>();
            var employeeBenefitsEnrollmentPoolEntities = await benefitsEnrollmentRepository.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
            logger.Debug(string.Format("*******employeeBenefitsEnrollmentPoolEntities obtained for {0}******", employeeId));
            if (employeeBenefitsEnrollmentPoolEntities!= null && employeeBenefitsEnrollmentPoolEntities.Any())
            {
                var sortedEmployeeBenefitsEnrollmentPoolEntities = SortEnrollmentPoolEntities(employeeBenefitsEnrollmentPoolEntities);
                foreach (var employeeBenefitsEnrollmentPoolEntity in sortedEmployeeBenefitsEnrollmentPoolEntities)
                {
                    employeeBenefitsEnrollmentPooldtos.Add(entityToDtoAdapter.MapToType(employeeBenefitsEnrollmentPoolEntity));
                }
            }
            logger.Debug("employeeBenefitsEnrollmentPooldtos obtained successfully");
            return employeeBenefitsEnrollmentPooldtos;
        }

        /// <summary>
        /// Puts all organizations after all non-organizations
        /// </summary>
        /// <param name="employeeBenefitsEnrollmentPoolEntities"></param>
        /// <returns></returns>
        private List<DomainEntities.EmployeeBenefitsEnrollmentPoolItem> SortEnrollmentPoolEntities(IEnumerable<DomainEntities.EmployeeBenefitsEnrollmentPoolItem> employeeBenefitsEnrollmentPoolEntities)
        {
            var sortedEntities = new List<DomainEntities.EmployeeBenefitsEnrollmentPoolItem>();
            if (employeeBenefitsEnrollmentPoolEntities != null && employeeBenefitsEnrollmentPoolEntities.Any())
            {
                var organizations = employeeBenefitsEnrollmentPoolEntities.Where(e => !string.IsNullOrEmpty(e.OrganizationId) || !string.IsNullOrEmpty(e.OrganizationName));
                logger.Debug("******Organizations obtained********");
                var nonOrganizations = employeeBenefitsEnrollmentPoolEntities.Except(organizations);
                logger.Debug("******Non Organizations obtained******");
                logger.Debug("*******Putting all organizations after all non-organizations*********");
                sortedEntities.AddRange(nonOrganizations.ToList());
                sortedEntities.AddRange(organizations.ToList());
            }

            return sortedEntities;
        }

        /// <summary>
        /// Gets EmployeeBenefitsEnrollmentPackage object for the specified employee id
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="enrollmentPeriodId">optional</param>
        /// <returns>EmployeeBenefitsEnrollmentPackage DTO</returns>
        public async Task<EmployeeBenefitsEnrollmentPackage> GetEmployeeBenefitsEnrollmentPackageAsync(string employeeId, string enrollmentPeriodId = null)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            if (!CurrentUser.IsPerson(employeeId))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view EmployeeBenefitsEnrollmentPackage for person {1}", CurrentUser.PersonId, employeeId));
            }
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EmployeeBenefitsEnrollmentPackage, Dtos.HumanResources.EmployeeBenefitsEnrollmentPackage>();
            var enrollmentPackageEntity = await benefitsEnrollmentRepository.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
            logger.Debug(string.Format("*******enrollmentPackageEntity obtained for {0}********", employeeId));
            var enrollmentPackageDto = entityToDtoAdapter.MapToType(enrollmentPackageEntity);
            logger.Debug("enrollmentPackageDto obtained successfully");
            return enrollmentPackageDto;
        }

        /// <summary>
        /// Adds new benefits enrollment pool information to an employee.
        /// </summary>
        /// <param name="employeeId">Employee id to which the dependent needs to add</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem">Dependent information to add</param>
        /// <returns>Newly added EmployeeBenefitsEnrollmentPoolItem DTO</returns>
        public async Task<EmployeeBenefitsEnrollmentPoolItem> AddEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            if (employeeBenefitsEnrollmentPoolItem == null)
            {
                throw new ArgumentNullException("employeeBenefitsEnrollmentPoolItem");
            }

            if (!CurrentUser.IsPerson(employeeId))
            {
                throw new PermissionsException("User does not have permission to add benefits enrollment pool information");
            }

            // Get adapter to convert the dto to domain entity...
            var dtoToEntityAdapter = _adapterRegistry.GetAdapter<EmployeeBenefitsEnrollmentPoolItem, DomainEntities.EmployeeBenefitsEnrollmentPoolItem>();
            var entityBenefitsEnrollmentPool = dtoToEntityAdapter.MapToType(employeeBenefitsEnrollmentPoolItem);
            logger.Debug("benefitsEnrollmentPoolEntity obtained");
            var addedEmployeeBenefitsEnrollmentPoolItem = await benefitsEnrollmentRepository.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, entityBenefitsEnrollmentPool);

            if (addedEmployeeBenefitsEnrollmentPoolItem == null)
            {
                string message = "Couldn't add benefits enrollment pool information to an employee.";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            logger.Debug(string.Format("Added benefits enrollment pool information to {0}", employeeId));
            // Convert the domain entity to DTO and return the newly created newBenefitsPool.
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<DomainEntities.EmployeeBenefitsEnrollmentPoolItem, EmployeeBenefitsEnrollmentPoolItem>();

            return entityToDtoAdapter.MapToType(addedEmployeeBenefitsEnrollmentPoolItem);
        }

        /// <summary>
        /// Updated benefits enrollment pool information of an employee.
        /// </summary>
        /// <param name="employeeId">Employee id to which the dependent needs to be updated</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem">Dependent information to be updated</param>
        /// <returns>Updated EmployeeBenefitsEnrollmentPoolItem DTO</returns>
        public async Task<EmployeeBenefitsEnrollmentPoolItem> UpdateEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            if (employeeBenefitsEnrollmentPoolItem == null)
            {
                throw new ArgumentNullException("employeeBenefitsEnrollmentPoolItem");
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentPoolItem.Id))
            {
                throw new ArgumentNullException("employeeBenefitsEnrollmentPoolItem.Id");
            }

            if (!CurrentUser.IsPerson(employeeId))
            {
                throw new PermissionsException("User does not have permission to add benefits enrollment pool information");
            }

            //Check whether the dependent exists or not before update
            var isDependentExists = await benefitsEnrollmentRepository.CheckDependentExistsAsync(employeeBenefitsEnrollmentPoolItem.Id);
            if (!isDependentExists)
            {
                var message = string.Format("Failed to update the benefits enrollment pool {0} as no existing dependent was found in the DB", employeeBenefitsEnrollmentPoolItem.Id);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            logger.Debug(string.Format("Existing dependent found in the DB. Updating the benefits enrollment pool {0}", employeeBenefitsEnrollmentPoolItem.Id));
            // Get adapter to convert the dto to domain entity...
            var dtoToEntityAdapter = _adapterRegistry.GetAdapter<EmployeeBenefitsEnrollmentPoolItem, DomainEntities.EmployeeBenefitsEnrollmentPoolItem>();
            var entityBenefitsEnrollmentPool = dtoToEntityAdapter.MapToType(employeeBenefitsEnrollmentPoolItem);
            logger.Debug("benefitsEnrollmentPoolEntity obtained");
            var updatedEmployeeBenefitsEnrollmentPoolItem = await benefitsEnrollmentRepository.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, entityBenefitsEnrollmentPool);

            if (updatedEmployeeBenefitsEnrollmentPoolItem == null)
            {
                string message = "Couldn't update benefits enrollment pool information of an employee.";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            logger.Debug(string.Format("Updated the benefits enrollment pool information for {0}", employeeId));
            // Convert the domain entity to DTO and return the newly created newBenefitsPool.
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<DomainEntities.EmployeeBenefitsEnrollmentPoolItem, EmployeeBenefitsEnrollmentPoolItem>();

            return entityToDtoAdapter.MapToType(updatedEmployeeBenefitsEnrollmentPoolItem);
        }

        /// <summary>
        /// Queries enrollment period benefits based on specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EnrollmentPeriodBenefit>> QueryEnrollmentPeriodBenefitsAsync(BenefitEnrollmentBenefitsQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            if (string.IsNullOrEmpty(criteria.BenefitTypeId))
            {
                throw new ArgumentException("criteria", "BenefitTypeId must be present");
            }
            List<EnrollmentPeriodBenefit> benefitDtos = new List<EnrollmentPeriodBenefit>();

            var benefitEntities = await benefitsEnrollmentRepository.QueryEnrollmentPeriodBenefitsAsync(criteria.BenefitTypeId,
                criteria.EnrollmentPeriodId, criteria.PackageId, criteria.EnrollmentPeriodBenefitIds);
            logger.Debug("benefitEntities obtained");
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<DomainEntities.EnrollmentPeriodBenefit, EnrollmentPeriodBenefit>();
            foreach (var entity in benefitEntities)
            {
                benefitDtos.Add(entityToDtoAdapter.MapToType(entity));
            }
            logger.Debug("benefitDtos obtained successfully");
            return benefitDtos;
        }

        /// <summary>
        /// Queries employee benefits enrollment info based on specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object based on criteria</returns>
        public async Task<EmployeeBenefitsEnrollmentInfo> QueryEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfoQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if (string.IsNullOrEmpty(criteria.EmployeeId))
            {
                throw new ArgumentException("criteria", "EmployeeId must be present");
            }

            if (!CurrentUser.IsPerson(criteria.EmployeeId))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view EmployeeBenefitsEnrollmentInfo for person {1}", CurrentUser.PersonId, criteria.EmployeeId));
            }

            EmployeeBenefitsEnrollmentInfo employeeBenefitsEnrollmentInfoDto = new EmployeeBenefitsEnrollmentInfo();
            var employeeBenefitsEnrollmentInfoEntity = await benefitsEnrollmentRepository.QueryEmployeeBenefitsEnrollmentInfoAsync(criteria.EmployeeId, criteria.EnrollmentPeriodId, criteria.BenefitTypeId);
            logger.Debug("employeeBenefitsEnrollmentInfoEntity obtained");
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<DomainEntities.EmployeeBenefitsEnrollmentInfo, EmployeeBenefitsEnrollmentInfo>();
            return entityToDtoAdapter.MapToType(employeeBenefitsEnrollmentInfoEntity);
        }

        /// <summary>
        /// Updates the given benefits enrollment information
        /// </summary>
        /// <param name="employeeBenefitEnrollmentInfoDto"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object</returns>
        public async Task<EmployeeBenefitsEnrollmentInfo> UpdateEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfo employeeBenefitEnrollmentInfoDto)
        {
            if (employeeBenefitEnrollmentInfoDto == null)
            {
                throw new ArgumentNullException("employeeBenefitEnrollmentInfoDto");
            }

            var employeeId = employeeBenefitEnrollmentInfoDto.EmployeeId;

            if (!CurrentUser.IsPerson(employeeId))
            {
                throw new PermissionsException("User does not have permission to update employee benefits enrollment information");
            }

            var dtoToEntityAdapter = _adapterRegistry.GetAdapter<EmployeeBenefitsEnrollmentInfo, DomainEntities.EmployeeBenefitsEnrollmentInfo>();
            var employeeBenefitEnrollmentInfoEntity = dtoToEntityAdapter.MapToType(employeeBenefitEnrollmentInfoDto);
            logger.Debug("employeeBenefitEnrollmentInfoEntity obtained");
            var updatedEmployeeBenefitEnrollmentInfoEntity = await benefitsEnrollmentRepository.UpdateEmployeeBenefitsEnrollmentInfoAsync(employeeBenefitEnrollmentInfoEntity);

            if (updatedEmployeeBenefitEnrollmentInfoEntity == null)
            {
                string message = "Couldn't update employee benefits enrollment information";
                logger.Error(message);
                throw new ApplicationException(message);
            }
            logger.Debug("Employee benefits enrollment information updated successfully. updatedEmployeeBenefitEnrollmentInfoEntity obtained");
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<DomainEntities.EmployeeBenefitsEnrollmentInfo, EmployeeBenefitsEnrollmentInfo>();

            return entityToDtoAdapter.MapToType(updatedEmployeeBenefitEnrollmentInfoEntity);
        }

        /// <summary>
        /// Submits/Re-opens the benefits elected by an employee.
        /// </summary>
        /// <param name="criteria">BenefitEnrollmentCompletionCriteria object</param>
        /// <returns>BenefitEnrollmentCompletionInfo DTO</returns>
        public async Task<BenefitEnrollmentCompletionInfo> SubmitOrReOpenBenefitElectionsAsync(BenefitEnrollmentCompletionCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "BenefitEnrollmentCompletionCriteria is required");
            }

            if (string.IsNullOrWhiteSpace(criteria.EmployeeId))
            {
                throw new ArgumentException("Employee Id is required");
            }
            if (string.IsNullOrWhiteSpace(criteria.EnrollmentPeriodId))
            {
                throw new ArgumentException("Enrollment Period Id is required");
            }
            if (criteria.SubmitBenefitElections && string.IsNullOrWhiteSpace(criteria.BenefitsPackageId))
            {
                throw new ArgumentException("BenefitsPackageId is required");
            }

            if (!CurrentUser.IsPerson(criteria.EmployeeId))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to submit/re-open the elected benefits for person {1}", CurrentUser.PersonId, criteria.EmployeeId));
            }

            DomainEntities.BenefitEnrollmentCompletionInfo benefitEnrollmentCompletionInfoEntity;
            if (criteria.SubmitBenefitElections)
            {
                benefitEnrollmentCompletionInfoEntity = await benefitsEnrollmentRepository.SubmitBenefitElectionAsync(criteria.EmployeeId, criteria.EnrollmentPeriodId, criteria.BenefitsPackageId);
                logger.Debug("Benefits elected submitted successfully. benefitEnrollmentCompletionInfoEntity obtained");
            }
            else
            {
                benefitEnrollmentCompletionInfoEntity = await benefitsEnrollmentRepository.ReOpenBenefitElectionsAsync(criteria.EmployeeId, criteria.EnrollmentPeriodId);
                logger.Debug("Benefits elected reopened successfully. benefitEnrollmentCompletionInfoEntity obtained");
            }

            var benefitEnrollmentCompletionInfoEntityToAdapter = _adapterRegistry.GetAdapter<DomainEntities.BenefitEnrollmentCompletionInfo, BenefitEnrollmentCompletionInfo>();
            return benefitEnrollmentCompletionInfoEntityToAdapter.MapToType(benefitEnrollmentCompletionInfoEntity);
        }

        /// <summary>
        /// Get the beneficiary categories
        /// </summary>
        /// <returns>a list of beneficiaryCategoryDtos DTOs</returns>
        public async Task<IEnumerable<BeneficiaryCategory>> GetBeneficiaryCategoriesAsync()
        {
            List<BeneficiaryCategory> beneficiaryCategoryDtos = new List<BeneficiaryCategory>();
            var beneficiaryCategoryEntities = await humanResourceRepository.GetBeneficiaryCategoriesAsync();
            logger.Debug("beneficiaryCategoryEntities obtained");
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.BeneficiaryCategory, BeneficiaryCategory>();
            foreach (var entity in beneficiaryCategoryEntities)
            {
                beneficiaryCategoryDtos.Add(entityToDtoAdapter.MapToType(entity));
            }
            logger.Debug("beneficiaryCategoryDtos obtained successfully");
            return beneficiaryCategoryDtos;
        }

        /// <summary>
        /// Get benefits enrollment acknowledgement as a byte array representation of a PDF file.  
        /// </summary>
        /// <param name="employeeId">Employee id</param>
        /// <param name="reportPath">The path on the server to the report template</param>
        /// <param name="resourceFilePath">The path on the server to the resource file</param>
        /// <returns>Bytes array to render pdf report for benefits enrollment</returns>
        public async Task<byte[]> GetBenefitsInformationForAcknowledgementReport(string employeeId, string reportPath, string resourceFilePath)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            if (string.IsNullOrEmpty(reportPath))
            {
                throw new ArgumentNullException("reportPath");
            }

            if (string.IsNullOrEmpty(resourceFilePath))
            {
                throw new ArgumentNullException("resourceFilePath");
            }

            if (!File.Exists(resourceFilePath))
            {
                throw new FileNotFoundException("The benefits enrollment resource file could not be found.", resourceFilePath);
            }

            if (!CurrentUser.IsPerson(employeeId))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to get the benefits acknowledgement report for person {1}", CurrentUser.PersonId, employeeId));
            }

            // Get employee benefits enrollment eligibility for an end date and for confirmation text.
            var eligibilityInfo = await GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);

            if (eligibilityInfo == null)
            {
                string errorMessage = string.Format("Employee Benefits Enrollment Eligibility data is null for employee {0}.", employeeId);
                logger.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            logger.Debug(string.Format("Employee Benefits Enrollment Eligibility data obtained for employee {0}.", employeeId));
            var benefits = await GetBenefitsAcknowledgementInformationAsync(employeeId);
            logger.Debug(string.Format("Benefits enrollment acknowledgement information obtained successfully for {0}", employeeId));
            LocalReport report = new LocalReport();

            try
            {
                report.ReportPath = reportPath;
                report.SetBasePermissionsForSandboxAppDomain(new PermissionSet(PermissionState.Unrestricted));

                var utility = new ReportUtility();

                var parameters = utility.BuildReportParametersFromResourceFiles(new List<string>() { resourceFilePath });
                logger.Debug("Collection of report parameter objects built successfully from a collection of resource file paths");
                var benefitsConfirmationText = string.Empty;

                //Replacing the parameter with formated resource string by adding end date
                if (parameters.Any())
                {
                    var parameter = parameters.FirstOrDefault(p => p.Name.Equals("Report_Header_TitleDescription"));

                    if (parameter != null && eligibilityInfo.EndDate.HasValue)
                    {
                        parameter.Values[0] = string.Concat(parameter.Values[0], " <b>", eligibilityInfo.EndDate.Value.ToShortDateString(), "</br>");

                        parameters[parameters.FindIndex(p => p.Name.Equals("Report_Header_TitleDescription"))] = parameter;
                    }
                    logger.Debug("Parameter replaced with the formatted resource string by adding end date");
                }

                if (eligibilityInfo.ConfirmationCompleteText.Any())
                {
                    var benefitsCompleteText = string.Join(" ", eligibilityInfo.ConfirmationCompleteText.ToArray());
                    parameters.Add(new ReportParameter("Benefits_CompleteText", benefitsCompleteText));
                    logger.Debug("ConfirmationCompleteText added to the parameter");
                }

                report.SetParameters(parameters);
                logger.Debug("Report parameter properties successfully set for the local report");
                var benefitsDataSet = utility.ConvertToDataSet(benefits.ToArray());

                report.DataSources.Add(new ReportDataSource("Benefits", benefitsDataSet.Tables[0]));

                // Set up some options for the report
                string mimeType = string.Empty;
                string encoding;
                string fileNameExtension;
                Warning[] warnings;
                string[] streams;
                logger.Debug("Rendering the report as a byte array in the following format:");
                logger.Debug("ReportType, DeviceInfo, mimeType, encoding, fileNameExtension, streams and warnings");
                // Render the report as a byte array
                return report.Render(PdfReportConstants.ReportType, PdfReportConstants.DeviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
            }
            catch (ApplicationException e)
            {
                logger.Error(e, "Unable to get benefits enrollment information.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to generate benefits enrollment acknowledgement report.");
                throw;
            }
            finally
            {
                report.DataSources.Clear();
                report.ReleaseSandboxAppDomain();
                report.Dispose();
            }
        }

        /// <summary>
        /// Get benefits enrollment acknowledgement information
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>List of BenefitsEnrollmentAcknowledgement DTO's</returns>
        private async Task<IEnumerable<BenefitsEnrollmentAknowledgement>> GetBenefitsAcknowledgementInformationAsync(string employeeId)
        {
            var packageInfo = await GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);

            if (packageInfo == null)
            {
                string errorMessage = string.Format("Employee Benefits Enrollment Package data is null for employee {0}.", employeeId);
                logger.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            logger.Debug(string.Format("Employee Benefits Enrollment Package data obtained for employee {0}.", employeeId));
            var criteria = new EmployeeBenefitsEnrollmentInfoQueryCriteria()
            {
                EmployeeId = employeeId,
                EnrollmentPeriodId = packageInfo.BenefitsEnrollmentPeriodId
            };

            var enrollmentInfo = await QueryEmployeeBenefitsEnrollmentInfoAsync(criteria);

            if (enrollmentInfo == null)
            {
                string errorMessage = string.Format("Employee Benefits Enrollment Information is null for employee {0} and enrollment period {1}.", employeeId, packageInfo.BenefitsEnrollmentPeriodId);
                logger.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            logger.Debug(string.Format("Employee Benefits Enrollment Information obtained for employee {0} and enrollment period {1}.", employeeId, packageInfo.BenefitsEnrollmentPeriodId));
            var acknowledgementInfo = new List<BenefitsEnrollmentAknowledgement>();

            //Separate opted out benefit types
            if (enrollmentInfo.OptOutBenefitTypes.Any())
            {
                acknowledgementInfo.AddRange(packageInfo.EmployeeEligibleBenefitTypes
                                                        .Where(b => enrollmentInfo.OptOutBenefitTypes.Contains(b.BenefitType))
                                                        .Select(b => new BenefitsEnrollmentAknowledgement(b) { BenefitPlanDescription = waivedPlanDescription }));
            }
            logger.Debug("Opted out benefit types separated successfully");
            foreach (var type in packageInfo.EmployeeEligibleBenefitTypes.Where(b => !enrollmentInfo.OptOutBenefitTypes.Contains(b.BenefitType)))
            {
                var details = enrollmentInfo.EmployeeBenefitEnrollmentDetails.Where(d => d.BenefitTypeId.Equals(type.BenefitType) && (!d.Action.Equals(BenefitTypeAction.Cancel.ToString(), StringComparison.OrdinalIgnoreCase)));

                if (details.Any())
                {
                    details.ToList().ForEach(detail => acknowledgementInfo.Add(new BenefitsEnrollmentAknowledgement(type, detail)));
                }
                else
                {
                    acknowledgementInfo.Add(new BenefitsEnrollmentAknowledgement(type));
                }
            }

            //Update index parameter of each benefit types which
            //would be used to sort on the report
            if (acknowledgementInfo.Any())
            {
                int index = 0;
                acknowledgementInfo.ForEach(a => a.Index = index++);
                logger.Debug("Index parameter of each benefit types updated successfully");
            }

            return acknowledgementInfo;
        }
    }
}
