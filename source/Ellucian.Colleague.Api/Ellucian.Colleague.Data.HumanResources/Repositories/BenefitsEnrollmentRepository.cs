/*Copyright 2019-2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Benefits Enrollment repository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BenefitsEnrollmentRepository : BaseColleagueRepository, IBenefitsEnrollmentRepository
    {
        private readonly ApiSettings apiSettings;

        public BenefitsEnrollmentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings) : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Returns EmployeeBenefitsEnrollmentEligibility object
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment eligibility</param>
        /// <returns>EmployeeBenefitsEnrollmentEligibility domain entity containing benefits enrollment eligibility information</returns>
        public async Task<EmployeeBenefitsEnrollmentEligibility> GetEmployeeBenefitsEnrollmentEligibilityAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            logger.Debug(string.Format("Fetching EmployeeBenefitsEnrollmentEligibility for {0}", employeeId));
            GetBenefitEnrollmentEligibilityRequest request = new GetBenefitEnrollmentEligibilityRequest();
            request.EmployeeId = employeeId;
            GetBenefitEnrollmentEligibilityResponse response = null;

            try
            {
                response = await transactionInvoker.ExecuteAsync<GetBenefitEnrollmentEligibilityRequest, GetBenefitEnrollmentEligibilityResponse>(request);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }

            if (response == null)
            {
                var message = "Could not determine benefits enrollment eligiblity. Unexpected null response from CTX GetBenefitEnrollmentEligibility";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                var message = String.Format("Could not determine benefits enrollment eligiblity. Unexpected error response from CTX GetBenefitEnrollmentEligibility: {0}", response.ErrorMessage);
                logger.Error(message);
                throw new RepositoryException(message);
            }

            var EmployeeBenefitsEnrollmentEligibilityEntity = new EmployeeBenefitsEnrollmentEligibility(employeeId, response.EligiblePeriod, response.IneligibleReason)
            {
                Description = response.EnrollmentPeriodDesc,
                StartDate = response.EnrollmentPeriodStartDate,
                EndDate = response.EnrollmentPeriodEndDate,
                EligibilityPackage = response.EligiblePackage,
                EnrollmentConfirmationText = response.EnrollmentConfirmationText,
                ConfirmationCompleteText = response.EnrollmentConfirmCompleteText,
                IsEnrollmentInitiated = response.IsEnrollmentInitiated,
                IsPackageSubmitted = response.IsPackageSubmitted,
                BenefitsPageCustomText = response.BenefitsText,
                BenefitsEnrollmentPageCustomText = response.BenefitsEnrollmentText,
                ManageDepBenPageCustomText = response.ManageDepBenText
            };

            logger.Debug(string.Format("EmployeeBenefitsEnrollmentEligibility domain entity containing benefits enrollment eligibility information obtained successfully for {0}", employeeId));
            logger.Debug(string.Format("The EnrollmentPeriodStartDate = {0}, EnrollmentPeriodEndDate = {1}", response.EnrollmentPeriodStartDate, response.EnrollmentPeriodEndDate));
            return EmployeeBenefitsEnrollmentEligibilityEntity;
        }

        /// <summary>
        /// EmployeeBenefitsEnrollmentPoolItem object
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment information</param>
        /// <returns>List of EmployeeBenefitsEnrollmentPoolItem domain entities containing benefits enrollment dependent and beneficiary pool information</returns>
        public async Task<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>> GetEmployeeBenefitsEnrollmentPoolAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            logger.Debug(string.Format("Fetching EmployeeBenefitsEnrollmentPoolItem for {0}", employeeId));
            GetBenefitEnrollmentPoolRequest request = new GetBenefitEnrollmentPoolRequest();
            request.EmployeeId = employeeId;
            GetBenefitEnrollmentPoolResponse response = null;

            try
            {
                response = await transactionInvoker.ExecuteAsync<GetBenefitEnrollmentPoolRequest, GetBenefitEnrollmentPoolResponse>(request);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }

            if (response == null)
            {
                var message = "Could not get benefits enrollment pool. Unexpected null response from CTX EmployeeBenefitsEnrollmentPool";
                logger.Error(message);
                throw new RepositoryException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                var message = String.Format("Could not get benefits enrollment pool. Unexpected error response from CTX EmployeeBenefitsEnrollmentPool: {0}", response.ErrorMessage);
                logger.Error(message);
                throw new RepositoryException(message);
            }

            var EmployeeBenefitsEnrollmentPoolItemEntities = new List<EmployeeBenefitsEnrollmentPoolItem>();
            foreach (var pool in response.BenefitEnrollmentPool)
            {
                EmployeeBenefitsEnrollmentPoolItemEntities.Add(CreateEmployeeBenefitsEnrollmentPoolEntity(pool));
            }

            logger.Debug(string.Format("List of EmployeeBenefitsEnrollmentPoolItem domain entities count: {0} containing benefits enrollment dependent and beneficiary pool information obtained successfully for {1}", EmployeeBenefitsEnrollmentPoolItemEntities.Count(), employeeId));
            return EmployeeBenefitsEnrollmentPoolItemEntities;
        }

        /// <summary>
        /// Gets dependent information by id
        /// </summary>
        /// <param name="employeeId">Employee id for which we needs to pull dependents</param>
        /// <param name="benefitsEnrollmentPoolId">Id of dependent to get</param>
        /// <returns>EmployeeBenefitsEnrollmentPoolItem Entity</returns>
        public async Task<EmployeeBenefitsEnrollmentPoolItem> GetEmployeeBenefitsEnrollmentPoolByIdAsync(string employeeId, string benefitsEnrollmentPoolId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            if (string.IsNullOrEmpty(benefitsEnrollmentPoolId))
            {
                throw new ArgumentNullException("benefitsEnrollmentPoolId");
            }
            logger.Debug(string.Format("Fetching dependent information for {0}", employeeId));
            EmployeeBenefitsEnrollmentPoolItem result = null;
            IEnumerable<EmployeeBenefitsEnrollmentPoolItem> dependents = null;

            try
            {
                dependents = await GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to get EmployeeBenefitsEnrollmentPool information by id: {0}", benefitsEnrollmentPoolId);
                logger.Error(ex, message);
                throw;
            }

            if (dependents != null && dependents.Any())
            {
                result = dependents.FirstOrDefault(d => d.Id == benefitsEnrollmentPoolId);
                logger.Debug(string.Format("EmployeeBenefitsEnrollmentPool information by id: {0} obtained successfully", benefitsEnrollmentPoolId));
            }

            return result;
        }

        /// <summary>
        /// Gets EmployeeBenefitsEnrollmentPackage object for the specified employee id
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="enrollmentPeriodId">optional</param>
        /// <returns></returns>
        public async Task<EmployeeBenefitsEnrollmentPackage> GetEmployeeBenefitsEnrollmentPackageAsync(string employeeId, string enrollmentPeriodId = null)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            logger.Debug(string.Format("Fetching EmployeeBenefitsEnrollmentPackage object for {0}", employeeId));
            GetBenefitEnrollmentPackageRequest request = new GetBenefitEnrollmentPackageRequest()
            {
                EmployeeId = employeeId,
                EnrollmentPeriodId = enrollmentPeriodId
            };
            GetBenefitEnrollmentPackageResponse response = null;
            try
            {
                response = await transactionInvoker.ExecuteAsync<GetBenefitEnrollmentPackageRequest, GetBenefitEnrollmentPackageResponse>(request);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
            if (response == null)
            {
                var message = "Could not get employee benefits enrollment package. Unexpected null response from CTX GetBenefitEnrollmentPackage";
                logger.Error(message);
                throw new RepositoryException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                var message = String.Format("Could not get employee benefits enrollment package. Unexpected error response from CTX GetBenefitEnrollmentPackage: {0}", response.ErrorMessage);
                logger.Error(message);
                throw new RepositoryException(message);
            }
            EmployeeBenefitsEnrollmentPackage benefitsPackage = new EmployeeBenefitsEnrollmentPackage(employeeId, response.PkgId)
            {
                BenefitsEnrollmentPeriodId = response.EnrollmentPeriodId,
                PackageDescription = response.PkgDescripion,
                EmployeeEligibleBenefitTypes = BuildEligibleBenefitTypes(response)
            };
            logger.Debug(string.Format("EmployeeBenefitsEnrollmentPackage obtained successfully for {0} with enrollment period id: {1}", employeeId, response.EnrollmentPeriodId));
            return benefitsPackage;
        }

        /// <summary>
        /// Adds new benefits enrollment pool information to an employee.
        /// </summary>
        /// <param name="employeeId">Employee id to which the dependent needs to add</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem">Dependent information to add</param>
        /// <returns>Newly added EmployeeBenefitsEnrollmentPoolItem Entity</returns>
        public async Task<EmployeeBenefitsEnrollmentPoolItem> AddEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                var message = "EmployeeId is required to add benefits enrollment pool.";
                logger.Error(message);
                throw new ArgumentNullException("employeeId", message);
            }

            if (employeeBenefitsEnrollmentPoolItem == null)
            {
                var message = "Benefits enrollment pool information is required to add.";
                logger.Error(message);
                throw new ArgumentNullException("employeeBenefitsEnrollmentPoolItem", message);
            }
            logger.Debug(string.Format("Adding new benefits enrollment pool information to {0}", employeeId));
            DateTime? birthDate = null;
            if (employeeBenefitsEnrollmentPoolItem.BirthDate.HasValue)
            {
                birthDate = DateTime.SpecifyKind(DateTime.Parse(employeeBenefitsEnrollmentPoolItem.BirthDate.Value.ToString()), DateTimeKind.Unspecified);
            }

            var request = new AddBenefitEnrollmentPoolRequest()
            {
                Id = employeeBenefitsEnrollmentPoolItem.Id,
                PersonId = employeeBenefitsEnrollmentPoolItem.PersonId,
                IsTrust = employeeBenefitsEnrollmentPoolItem.IsTrust,
                Prefix = employeeBenefitsEnrollmentPoolItem.Prefix,
                FirstName = employeeBenefitsEnrollmentPoolItem.FirstName,
                MiddleName = employeeBenefitsEnrollmentPoolItem.MiddleName,
                LastName = employeeBenefitsEnrollmentPoolItem.LastName,
                Suffix = employeeBenefitsEnrollmentPoolItem.Suffix,
                AddressLine1 = employeeBenefitsEnrollmentPoolItem.AddressLine1,
                AddressLine2 = employeeBenefitsEnrollmentPoolItem.AddressLine2,
                City = employeeBenefitsEnrollmentPoolItem.City,
                State = employeeBenefitsEnrollmentPoolItem.State,
                PostalCode = employeeBenefitsEnrollmentPoolItem.PostalCode,
                Country = employeeBenefitsEnrollmentPoolItem.Country,
                Relationship = employeeBenefitsEnrollmentPoolItem.Relationship,
                BirthDate = birthDate,
                GovernmentId = employeeBenefitsEnrollmentPoolItem.GovernmentId,
                Gender = employeeBenefitsEnrollmentPoolItem.Gender,
                MaritalStatus = employeeBenefitsEnrollmentPoolItem.MaritalStatus,
                OrganizationName = employeeBenefitsEnrollmentPoolItem.OrganizationName
            };
            logger.Debug(string.Format("Request object for employeeBenefitsEnrollmentPoolItem id: {0} and person id: {1} created successfully", employeeBenefitsEnrollmentPoolItem.Id, employeeBenefitsEnrollmentPoolItem.PersonId));
            var response = await transactionInvoker.ExecuteAsync<AddBenefitEnrollmentPoolRequest, AddBenefitEnrollmentPoolResponse>(request);

            if (response == null)
            {
                string message = "Could not add benefit enrollment pool information. Unexpected null returned from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            // If there is any error message - throw an exception
            if (!string.IsNullOrEmpty(response.Error))
            {
                string message = string.Format("Error(s) occurred while creating benefits enrollment pool for an employee '{0}':", employeeId);
                var exception = new RepositoryException(message);
                response.BenefitEnrollmentPoolErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                logger.Error(message);
                throw exception;
            }

            return await GetEmployeeBenefitsEnrollmentPoolByIdAsync(employeeId, response.Id);
        }


        /// <summary>
        /// Update benefits enrollment pool information of an employee.
        /// </summary>
        /// <param name="employeeId">Employee id to which the dependent needs to be updated</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem">Dependent information to update</param>
        /// <returns>Updated EmployeeBenefitsEnrollmentPoolItem Entity</returns>
        public async Task<EmployeeBenefitsEnrollmentPoolItem> UpdateEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                var message = "EmployeeId is required to update benefits enrollment pool.";
                logger.Error(message);
                throw new ArgumentNullException("employeeId", message);
            }

            if (employeeBenefitsEnrollmentPoolItem == null)
            {
                var message = "Benefits enrollment pool information is required to update.";
                logger.Error(message);
                throw new ArgumentNullException("employeeBenefitsEnrollmentPoolItem", message);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentPoolItem.Id))
            {
                var message = "Employee Benefit Enrollment pool Id is required to update.";
                logger.Error(message);
                throw new ArgumentNullException("employeeBenefitsEnrollmentPoolItem.Id", message);
            }
            logger.Debug(string.Format("Updating benefits enrollment pool information for {0}", employeeId));
            DateTime? birthDate = null;
            if (employeeBenefitsEnrollmentPoolItem.BirthDate.HasValue)
            {
                birthDate = DateTime.SpecifyKind(DateTime.Parse(employeeBenefitsEnrollmentPoolItem.BirthDate.Value.ToString()), DateTimeKind.Unspecified);
            }

            var request = new UpdateBenEnrPoolRequest()
            {
                BenEnrPoolId = employeeBenefitsEnrollmentPoolItem.Id,
                InPersonId = employeeBenefitsEnrollmentPoolItem.PersonId,
                InTrusteeFlag = employeeBenefitsEnrollmentPoolItem.IsTrust,
                InPrefix = employeeBenefitsEnrollmentPoolItem.Prefix,
                InFirstName = employeeBenefitsEnrollmentPoolItem.FirstName,
                InMiddleName = employeeBenefitsEnrollmentPoolItem.MiddleName,
                InLastName = employeeBenefitsEnrollmentPoolItem.LastName,
                InSuffix = employeeBenefitsEnrollmentPoolItem.Suffix,
                InAddrLine1 = employeeBenefitsEnrollmentPoolItem.AddressLine1,
                InAddrLine2 = employeeBenefitsEnrollmentPoolItem.AddressLine2,
                InCity = employeeBenefitsEnrollmentPoolItem.City,
                InState = employeeBenefitsEnrollmentPoolItem.State,
                InZip = employeeBenefitsEnrollmentPoolItem.PostalCode,
                InCountry = employeeBenefitsEnrollmentPoolItem.Country,
                InRelationship = employeeBenefitsEnrollmentPoolItem.Relationship,
                InBirthDate = employeeBenefitsEnrollmentPoolItem.BirthDate,
                InSsn = employeeBenefitsEnrollmentPoolItem.GovernmentId,
                InGender = employeeBenefitsEnrollmentPoolItem.Gender,
                InMaritalStatus = employeeBenefitsEnrollmentPoolItem.MaritalStatus,
                InOrgName = employeeBenefitsEnrollmentPoolItem.OrganizationName
            };
            logger.Debug(string.Format("Request object for employeeBenefitsEnrollmentPoolItem id: {0} and person id: {1} created successfully", employeeBenefitsEnrollmentPoolItem.Id, employeeBenefitsEnrollmentPoolItem.PersonId));

            var response = await transactionInvoker.ExecuteAsync<UpdateBenEnrPoolRequest, UpdateBenEnrPoolResponse>(request);

            if (response == null)
            {
                string message = "Could not update benefit enrollment pool information. Unexpected null returned from CTX.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            // If there is any error message - throw an exception
            if (!string.IsNullOrEmpty(response.Error))
            {
                string message = string.Format("Error(s) occurred while updating benefits enrollment pool for an employee '{0}':", employeeId);
                var exception = new RepositoryException(message);
                response.UpdateBenefitEnrollmentPoolErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                logger.Error(message);
                throw exception;
            }

            return await GetEmployeeBenefitsEnrollmentPoolByIdAsync(employeeId, response.BenEnrPoolId);
        }

        /// <summary>
        /// This checks whether the dependent with id exists or not
        /// </summary>
        /// <param name="benefitsEnrollmentPoolId">BenefitsEnrollmentPoolId</param>
        /// <returns>true if dependent exists or else false</returns>
        public async Task<bool> CheckDependentExistsAsync(string benefitsEnrollmentPoolId)
        {
            if (string.IsNullOrEmpty(benefitsEnrollmentPoolId))
            {
                throw new ArgumentNullException("benefitsEnrollmentPoolId");
            }
            logger.Debug(string.Format("Checking whether the dependent with id: {0} exists or not", benefitsEnrollmentPoolId));
            string criteria = "WITH BEN.ENR.POOL.ID EQ '{0}'";
            string[] benefitsEnrollmentPoolIds = await DataReader.SelectAsync("BEN.ENR.POOL", string.Format(criteria, benefitsEnrollmentPoolId));

            if (benefitsEnrollmentPoolIds == null || !benefitsEnrollmentPoolIds.Any())
            {
                var message = "BEN.ENR.POOL record with" + benefitsEnrollmentPoolId + "doesn't exist";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            return benefitsEnrollmentPoolIds.FirstOrDefault().Equals(benefitsEnrollmentPoolId) ? true : false;
        }

        /// <summary>
        /// Queries enrollment period benefits based on specified criteria
        /// </summary>
        /// <param name="enrollmentPeriodId"></param>
        /// <param name="packageId"></param>
        /// <param name="benefitTypeId"></param>
        /// <param name="enrollmentPeriodBenefitIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EnrollmentPeriodBenefit>> QueryEnrollmentPeriodBenefitsAsync(string benefitTypeId,
            string enrollmentPeriodId = null,
            string packageId = null,
            List<string> enrollmentPeriodBenefitIds = null)
        {
            if (string.IsNullOrEmpty(benefitTypeId))
            {
                throw new ArgumentNullException("benefitTypeId");
            }
            logger.Debug("Querying enrollment period benefits based on specified criteria");
            GetBenefitTypeBenefitsRequest request = new GetBenefitTypeBenefitsRequest()
            {
                EnrPeriodId = enrollmentPeriodId,
                PkgId = packageId,
                EnrPeriodBenTypeId = benefitTypeId,
                EnrPeriodBenIdsIn = enrollmentPeriodBenefitIds
            };
            logger.Debug(string.Format("enrollmentPeriodId: {0}, packageId: {1}, benefitTypeId: {2}", enrollmentPeriodId, packageId, benefitTypeId));
            List<EnrollmentPeriodBenefit> benefits = new List<EnrollmentPeriodBenefit>();
            GetBenefitTypeBenefitsResponse response = null;
            try
            {
                response = await transactionInvoker.ExecuteAsync<GetBenefitTypeBenefitsRequest, GetBenefitTypeBenefitsResponse>(request);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
            if (response == null)
            {
                var message = "Could not get enrollment period benefits. Unexpected null response from CTX GetBenefitTypeBenefits";
                logger.Error(message);
                throw new RepositoryException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                var message = String.Format("Could not get enrollment period benefits. Unexpected error response from CTX GetBenefitTypeBenefits: {0}", response.ErrorMessage);
                logger.Error(message);
                throw new RepositoryException(message);
            }
            benefits = BuildBenefits(response, benefitTypeId);
            logger.Debug("Enrollment period benefits obtained successfully");
            return benefits;
        }

        /// <summary>
        /// Queries employee benefits enrollment info based on specified criteria
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="enrollmentPeriodId"></param>
        /// <param name="benefitTypeId"></param>
        /// <param name="includeCancelActions"></param>
        /// <param name="includeOptOutActions"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object</returns>
        public async Task<EmployeeBenefitsEnrollmentInfo> QueryEmployeeBenefitsEnrollmentInfoAsync(string employeeId, string enrollmentPeriodId, string benefitTypeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            if (string.IsNullOrEmpty(enrollmentPeriodId))
            {
                throw new ArgumentNullException("enrollmentPeriodId");
            }
            logger.Debug("Querying employee benefits enrollment info based on specified criteria");

            GetEmployeeBenefitsEnrollmentInfoRequest request = new GetEmployeeBenefitsEnrollmentInfoRequest()
            {
                EmployeeId = employeeId,
                EnrollmentPeriodId = enrollmentPeriodId,
                BenefitTypeId = benefitTypeId,
            };
            logger.Debug(string.Format("employeeId: {0}, enrollmentPeriodId: {1}, benefitTypeId: {2}", employeeId, enrollmentPeriodId, benefitTypeId));
            GetEmployeeBenefitsEnrollmentInfoResponse response = new GetEmployeeBenefitsEnrollmentInfoResponse();

            try
            {
                response = await transactionInvoker.ExecuteAsync<GetEmployeeBenefitsEnrollmentInfoRequest, GetEmployeeBenefitsEnrollmentInfoResponse>(request);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
            if (response == null)
            {
                var message = "Could not get benefts enrollment info. Unexpected null response from CTX GetEmployeeBenefitsEnrollmentInfo";
                logger.Error(message);
                throw new RepositoryException(message);
            }

            var employeeBenefitsEnrollmentInfoEntity = BuildEmployeeBenefitsEnrollmentInfo(employeeId, enrollmentPeriodId, response);
            logger.Debug("employeeBenefitsEnrollmentInfoEntity obtained. Benefits enrollment info obtained successfully");
            return employeeBenefitsEnrollmentInfoEntity;
        }

        /// <summary>
        /// Updates the given benefits enrollment information
        /// </summary>
        /// <param name="employeeBenefitEnrollmentInfo"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object</returns>
        public async Task<EmployeeBenefitsEnrollmentInfo> UpdateEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfo employeeBenefitEnrollmentInfo)
        {
            if (employeeBenefitEnrollmentInfo == null)
            {
                var message = "Benefits enrollment information record is required";
                logger.Error(message);
                throw new ArgumentNullException("employeeBenefitEnrollmentInfo", message);
            }
            logger.Debug(string.Format("Updating the given benefits enrollment information with id: {0} for employee: {1}", employeeBenefitEnrollmentInfo.Id, employeeBenefitEnrollmentInfo.EmployeeId));
            var enrollPeriodSelections = employeeBenefitEnrollmentInfo.EmployeeBenefitEnrollmentDetails.Select(ebd => new EnrollPeriodSelections()
            {
                EnrollPeriodBenId = ebd.PeriodBenefitId,
                EnrollPeriodAction = ebd.Action,
                EnrollPeriodFlexAmount = ebd.FlexibleBenefitAmount,
                EnrollPeriodBenefitAmount = ebd.Amount,
                EnrollPeriodInsuranceAmount = ebd.InsuranceCoverageAmount != null ? ebd.InsuranceCoverageAmount.Value.ToString() : null,
                EnrollPeriodPercent = ebd.Percent,
                EmployeeProviderId = !string.IsNullOrEmpty(ebd.EmployeeProviderId) ? ebd.EmployeeProviderId.Replace(";", "") : null,
                EmployeeProviderName = !string.IsNullOrEmpty(ebd.EmployeeProviderName) ? ebd.EmployeeProviderName.Replace(";", "") : null,
                DependentPoolIds = string.Join(DmiString.sSM, ebd.DependentPoolIds),
                DependentProviderIds = string.Join(DmiString.sSM, ebd.DependentProviderIds),
                DependentProviderNames = string.Join(DmiString.sSM, ebd.DependentProviderNames),
                BeneficiaryPoolIds = string.Join(DmiString.sSM, ebd.BeneficiaryPoolIds),
                BeneficiaryTypes = string.Join(DmiString.sSM, ebd.BeneficiaryTypes),
                BeneficiaryPercents = string.Join(DmiString.sSM, ebd.BeneficiaryPercent),
            }).ToList();
            logger.Debug("enrollPeriodSelections obtained");
            var request = new UpdateBenefitSelectionRequest()
            {
                EnrollPeriodBenType = employeeBenefitEnrollmentInfo.BenefitType,
                EmployeeId = employeeBenefitEnrollmentInfo.EmployeeId,
                EnrollPeriod = employeeBenefitEnrollmentInfo.EnrollmentPeriodId,
                BenefitPackageId = employeeBenefitEnrollmentInfo.BenefitPackageId,
                EnrollOptout = employeeBenefitEnrollmentInfo.IsWaived,
                EnrollPeriodSelections = enrollPeriodSelections
            };
            logger.Debug(string.Format("Request object for UpdateBenefitSelection employeeBenefitEnrollmentInfo EmployeeId: {0} and EnrollmentPeriodId: {1} created successfully", employeeBenefitEnrollmentInfo.EmployeeId, employeeBenefitEnrollmentInfo.EnrollmentPeriodId));
            UpdateBenefitSelectionResponse response = new UpdateBenefitSelectionResponse();

            try
            {
                response = await transactionInvoker.ExecuteAsync<UpdateBenefitSelectionRequest, UpdateBenefitSelectionResponse>(request);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
            if (response == null)
            {
                var message = "Could update benefts enrollment info. Unexpected null response from CTX UpdateBenefitSelection";
                logger.Error(message);
                throw new RepositoryException(message);
            }
            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                var message = String.Format("Could not update benefts enrollment info. Unexpected error response from CTX UpdateBenefitSelection: {0}", response.ErrorMessage);
                logger.Error(message);
                throw new RepositoryException(message);
            }

            var benefitTypeId = employeeBenefitEnrollmentInfo.EmployeeBenefitEnrollmentDetails.FirstOrDefault().BenefitTypeId;

            var upddatedEntity = await QueryEmployeeBenefitsEnrollmentInfoAsync(employeeBenefitEnrollmentInfo.EmployeeId, employeeBenefitEnrollmentInfo.EnrollmentPeriodId, benefitTypeId);
            logger.Debug(string.Format("The given benefits enrollment information updated successfully for {0}", employeeBenefitEnrollmentInfo.EmployeeId));
            return upddatedEntity;
        }

        /// <summary>
        /// Submits the benefits elected by an employee, for enrollment completion process
        /// </summary>
        /// <param name="employeeId">Employee Id of the user</param>
        /// <param name="enrollmentPeriodId">Id of the current enrollment period</param>
        /// <param name="benefitPackageId">Id of the benefits package associated with the enrollment period</param>
        /// <returns>BenefitEnrollmentCompletionInfo Entity</returns>
        public async Task<BenefitEnrollmentCompletionInfo> SubmitBenefitElectionAsync(string employeeId, string enrollmentPeriodId, string benefitPackageId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            if (string.IsNullOrEmpty(enrollmentPeriodId))
            {
                throw new ArgumentNullException("enrollmentPeriodId");
            }

            if (string.IsNullOrEmpty(benefitPackageId))
            {
                throw new ArgumentNullException("benefitPackageId");
            }
            logger.Debug(string.Format("Submitting the benefits elected by {0}, for enrollment completion process", employeeId));
            TxSubmitBenefitElectionRequest submitBenefitElectionRequest = new TxSubmitBenefitElectionRequest()
            {
                EmployeeId = employeeId,
                EnrollmentPeriodId = enrollmentPeriodId,
                BenefitsPackageId = benefitPackageId
            };
            logger.Debug(string.Format("submitBenefitElectionRequest object created. EmployeeId: {0}, EnrollmentPeriodId: {1}, BenefitsPackageId: {2}", employeeId, enrollmentPeriodId, benefitPackageId));
            TxSubmitBenefitElectionResponse submitBenefitElectionResponse = null;
            try
            {
                submitBenefitElectionResponse = await transactionInvoker.ExecuteAsync<TxSubmitBenefitElectionRequest, TxSubmitBenefitElectionResponse>(submitBenefitElectionRequest);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }

            if (submitBenefitElectionResponse == null)
            {
                var message = "Unable to submit benefit elections. Unexpected null response from CTX TxSubmitBenefitElection";
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }

            var benefitEnrollmentCompletionInfoEntity = BuildBenefitEnrollmentCompletionInfo(employeeId, enrollmentPeriodId, submitBenefitElectionResponse.EnrollmentConfirmationDate, submitBenefitElectionResponse.ErrorMessages);
            logger.Debug(string.Format("benefitEnrollmentCompletionInfoEntity obtained. The benefits elected by {0} submitted successfully", employeeId));
            return benefitEnrollmentCompletionInfoEntity;
        }

        /// <summary>
        /// Re-opens the benefits elected by an employee.
        /// </summary>
        /// <param name="employeeId">Employee Id of the user</param>
        /// <param name="enrollmentPeriodId">Id of the current enrollment period</param>     
        /// <returns>BenefitEnrollmentCompletionInfo Entity</returns>
        public async Task<BenefitEnrollmentCompletionInfo> ReOpenBenefitElectionsAsync(string employeeId, string enrollmentPeriodId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            if (string.IsNullOrEmpty(enrollmentPeriodId))
            {
                throw new ArgumentNullException("enrollmentPeriodId");
            }
            logger.Debug(string.Format("Reopening the benefits elected by {0}", employeeId));

            ReopenBenefitSelectionRequest reOpenBenefitSelectionRequest = new ReopenBenefitSelectionRequest()
            {
                EmployeeId = employeeId,
                EnrollmentPeriodId = enrollmentPeriodId
            };
            logger.Debug(string.Format("reOpenBenefitSelectionRequest object created. EmployeeId: {0}, EnrollmentPeriodId: {1}", employeeId, enrollmentPeriodId));
            ReopenBenefitSelectionResponse reOpenBenefitSelectionResponse = null;
            try
            {
                reOpenBenefitSelectionResponse = await transactionInvoker.ExecuteAsync<ReopenBenefitSelectionRequest, ReopenBenefitSelectionResponse>(reOpenBenefitSelectionRequest);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }

            if (reOpenBenefitSelectionResponse == null)
            {
                var message = "Unable to re-open benefit elections. Unexpected null returned from the CTX REOPEN.BENEFIT.ELECTION";
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }

            var benefitEnrollmentCompletionInfoEntity = BuildBenefitEnrollmentCompletionInfo(employeeId, enrollmentPeriodId, reOpenBenefitSelectionResponse.EnrollmentConfirmationDate, null);
            logger.Debug(string.Format("benefitEnrollmentCompletionInfoEntity obtained. The benefits elected by {0} reopened successfully", employeeId));
            return benefitEnrollmentCompletionInfoEntity;
        }

        #region Helper Methods
        /// <summary>
        /// Builds and returns a set of benefit enrollment period benefits based on specified criteria
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private List<EnrollmentPeriodBenefit> BuildBenefits(GetBenefitTypeBenefitsResponse response, string benefitTypeId)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            logger.Debug("Start - Process to build and return a set of benefit enrollment period benefits based on specified criteria - Start");
            List<EnrollmentPeriodBenefit> benefits = new List<EnrollmentPeriodBenefit>();
            foreach (var benefit in response.Benefits)
            {
                if (benefit != null)
                {
                    benefits.Add(new EnrollmentPeriodBenefit(benefit.BendedIds, benefit.EnrPeriodBenIds, benefitTypeId)
                    {
                        BenefitDescription = !string.IsNullOrEmpty(benefit.BendedSelfSvsDescs) ?
                                benefit.BendedSelfSvsDescs : benefit.BendedDescs,
                        IsDependentRequired = benefit.EpbDependentReqd,
                        IsBeneficiaryRequired = benefit.EpbBeneficiaryReqd,
                        IsHealthCareProviderRequired = benefit.EpbProviderReqd
                    });
                }

            }
            logger.Debug("End - Process to build and return a set of benefit enrollment period benefits based on specified criteria is successful - End");
            return benefits;
        }

        /// <summary>
        /// Builds and returns a list of benefit types objects
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private IEnumerable<EmployeeBenefitType> BuildEligibleBenefitTypes(GetBenefitEnrollmentPackageResponse response)
        {
            logger.Debug("Start - Process to build and return a list of benefit types objects - Start");
            var benefitTypes = new List<EmployeeBenefitType>();
            if (response != null && response.BenefitTypesGroup != null)
            {
                for (var i = 0; i < response.BenefitTypesGroup.Count; i++)
                {
                    var benefitType = response.BenefitTypesGroup[i];
                    if (benefitType != null)
                    {
                        if (!benefitTypes.Any(bt => bt.BenefitType == benefitType.BenefitTypes))
                        {
                            var newBenefitType = new EmployeeBenefitType(benefitType.BenefitTypes, benefitType.BenefitTypesDescriptions)
                            {
                                AllowOptOut = !string.IsNullOrEmpty(benefitType.BenefitTypesOptOutFlags) && benefitType.BenefitTypesOptOutFlags.Equals("Y", StringComparison.InvariantCultureIgnoreCase) ? true : false,
                                BenefitOptOutText = benefitType.BenefitTypesOptOutText,
                                BenefitTypeSpecialProcessingCode = benefitType.BenefitTypesSplProcCode,
                                MinBenefits = benefitType.BenefitTypesMinBenefit,
                                MaxBenefits = benefitType.BenefitTypesMaxBenefit,
                                BenefitsSelectionPageCustomText = benefitType.BenefitSelectionText
                            };
                            var newPeriodBenefit = BuildEnrollmentPeriodBenefit(benefitType);
                            newBenefitType.EnrollmentPeriodBenefitIds.Add(newPeriodBenefit);
                            benefitTypes.Add(newBenefitType);
                        }
                        else
                        {
                            var existingBenefitType = benefitTypes.FirstOrDefault(bt => bt.BenefitType == benefitType.BenefitTypes);
                            if (existingBenefitType != null)
                            {
                                var newPeriodBenefit = BuildEnrollmentPeriodBenefit(benefitType);
                                existingBenefitType.EnrollmentPeriodBenefitIds.Add(newPeriodBenefit);
                            }
                        }

                    }
                }
            }
            logger.Debug("End - Process to build and return a list of benefit types objects is successful - End");
            return benefitTypes;
        }

        /// <summary>
        /// Creates the EmployeeBenefitsEnrollmentInfo entity object
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="enrollmentPeriodId"></param>
        /// <param name="response"></param>
        /// <returns>EmployeeBenefitsEnrollmentInfo object</returns>
        private EmployeeBenefitsEnrollmentInfo BuildEmployeeBenefitsEnrollmentInfo(string employeeId, string enrollmentPeriodId, GetEmployeeBenefitsEnrollmentInfoResponse response)
        {
            logger.Debug("Start - Process to build and return the EmployeeBenefitsEnrollmentInfo entity object - Start");
            var employeeBenefitsEnrollmentDetails = new List<EmployeeBenefitsEnrollmentDetail>();

            foreach (var benefitsEnrollmentInfoDetails in response.BenefitsEnrollmentInfoDetails)
            {
                var employeeBenefitsEnrollmentDetailEntity = new EmployeeBenefitsEnrollmentDetail();
                employeeBenefitsEnrollmentDetailEntity.Id = benefitsEnrollmentInfoDetails.WorkDetailIds;
                employeeBenefitsEnrollmentDetailEntity.BenefitTypeId = benefitsEnrollmentInfoDetails.BenefitTypeIds;
                employeeBenefitsEnrollmentDetailEntity.BenefitTypeEffectiveDate = benefitsEnrollmentInfoDetails.BenefitTypeEffectiveDates;
                employeeBenefitsEnrollmentDetailEntity.PeriodBenefitId = benefitsEnrollmentInfoDetails.EnrollmentPeriodBenefitIds;
                employeeBenefitsEnrollmentDetailEntity.BenefitId = benefitsEnrollmentInfoDetails.BendedIds;
                employeeBenefitsEnrollmentDetailEntity.BenefitDescription = benefitsEnrollmentInfoDetails.BendedDescriptions;
                employeeBenefitsEnrollmentDetailEntity.CoverageLevels = benefitsEnrollmentInfoDetails.CoverageLevels != null ? benefitsEnrollmentInfoDetails.CoverageLevels.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.Action = benefitsEnrollmentInfoDetails.ElectionActions;
                employeeBenefitsEnrollmentDetailEntity.ActionDescription = benefitsEnrollmentInfoDetails.ElectionActionDescs;
                employeeBenefitsEnrollmentDetailEntity.FlexBenefitRequired = benefitsEnrollmentInfoDetails.FlexBenefitIsRequired != null ? benefitsEnrollmentInfoDetails.FlexBenefitIsRequired.Equals("Y", StringComparison.InvariantCultureIgnoreCase) : false;
                employeeBenefitsEnrollmentDetailEntity.DependentNames = benefitsEnrollmentInfoDetails.DependentNames != null ? benefitsEnrollmentInfoDetails.DependentNames.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.DependentIds = benefitsEnrollmentInfoDetails.DependentIds != null ? benefitsEnrollmentInfoDetails.DependentIds.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.DependentPoolIds = benefitsEnrollmentInfoDetails.DependentPoolIds != null ? benefitsEnrollmentInfoDetails.DependentPoolIds.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.DependentProviderIds = benefitsEnrollmentInfoDetails.DependentProviderIds != null ? benefitsEnrollmentInfoDetails.DependentProviderIds.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.DependentProviderNames = benefitsEnrollmentInfoDetails.DependentProviderNames != null ? benefitsEnrollmentInfoDetails.DependentProviderNames.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.BeneficiaryNames = benefitsEnrollmentInfoDetails.BeneficiaryNames != null ? benefitsEnrollmentInfoDetails.BeneficiaryNames.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.BeneficiaryIds = benefitsEnrollmentInfoDetails.BeneficiaryIds != null ? benefitsEnrollmentInfoDetails.BeneficiaryIds.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.BeneficiaryPoolIds = benefitsEnrollmentInfoDetails.BeneficiaryPoolIds != null ? benefitsEnrollmentInfoDetails.BeneficiaryPoolIds.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.BeneficiaryTypes = benefitsEnrollmentInfoDetails.BeneficiaryTypes != null ? benefitsEnrollmentInfoDetails.BeneficiaryTypes.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.BeneficiaryPercent = benefitsEnrollmentInfoDetails.BeneficiaryPercent != null ? benefitsEnrollmentInfoDetails.BeneficiaryPercent.Split(DmiString._SM).Select(p => (string.IsNullOrEmpty(p) ? null : (decimal?)Convert.ToDecimal(p))).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.BeneficiaryDisplayInformation = benefitsEnrollmentInfoDetails.BeneficiaryText != null ? benefitsEnrollmentInfoDetails.BeneficiaryText.Split(DmiString._SM).ToList() : null;
                employeeBenefitsEnrollmentDetailEntity.Amount = benefitsEnrollmentInfoDetails.BenefitAmount;
                employeeBenefitsEnrollmentDetailEntity.Percent = benefitsEnrollmentInfoDetails.BenefitPercent;
                employeeBenefitsEnrollmentDetailEntity.InsuranceCoverageAmount = benefitsEnrollmentInfoDetails.BenefitInsureAmount;
                employeeBenefitsEnrollmentDetailEntity.FlexibleBenefitAmount = benefitsEnrollmentInfoDetails.BenefitFlexAnnualAmt;
                employeeBenefitsEnrollmentDetailEntity.EmployeeProviderId = benefitsEnrollmentInfoDetails.EmployeeProviderId;
                employeeBenefitsEnrollmentDetailEntity.EmployeeProviderName = benefitsEnrollmentInfoDetails.EmployeeProviderName;
                employeeBenefitsEnrollmentDetails.Add(employeeBenefitsEnrollmentDetailEntity);
            }
            logger.Debug("End - Process to build and return the EmployeeBenefitsEnrollmentInfo entity object is successful - End");
            return new EmployeeBenefitsEnrollmentInfo()
            {
                EmployeeId = employeeId,
                BenefitPackageId = response.BenefitsPackageId,
                EnrollmentPeriodId = enrollmentPeriodId,
                ConfirmationDate = response.ConfirmationDate,
                EmployeeBenefitEnrollmentDetails = employeeBenefitsEnrollmentDetails,
                OptOutBenefitTypes = response.OptOutBenefitTypes
            };
        }

        /// <summary>
        /// Builds a EmployeeBenefitsEnrollmentPoolItem
        /// </summary>
        /// <param name="benefitsEnrollmentPoolItem"></param>
        /// <returns></returns>
        private EmployeeBenefitsEnrollmentPoolItem CreateEmployeeBenefitsEnrollmentPoolEntity(BenefitEnrollmentPool benefitsEnrollmentPoolItem)
        {
            var employeeBenefitsEnrollmentPoolItem = new EmployeeBenefitsEnrollmentPoolItem(benefitsEnrollmentPoolItem.BenEnrPoolId, benefitsEnrollmentPoolItem.BeplOrgName, benefitsEnrollmentPoolItem.BeplLastName)
            {
                PersonId = benefitsEnrollmentPoolItem.BeplPersonId,
                IsTrust = !string.IsNullOrEmpty(benefitsEnrollmentPoolItem.BeplTrustFlag) && benefitsEnrollmentPoolItem.BeplTrustFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase) ? true : false,
                Prefix = benefitsEnrollmentPoolItem.BeplPrefixCode,
                PrefixDescription = benefitsEnrollmentPoolItem.BeplPrefixDesc,
                FirstName = benefitsEnrollmentPoolItem.BeplFirstName,
                MiddleName = benefitsEnrollmentPoolItem.BeplMiddleName,
                Suffix = benefitsEnrollmentPoolItem.BeplSuffixCode,
                SuffixDescription = benefitsEnrollmentPoolItem.BeplSuffixDesc,
                AddressLine1 = benefitsEnrollmentPoolItem.BeplAddrLine1,
                AddressLine2 = benefitsEnrollmentPoolItem.BeplAddrLine2,
                City = benefitsEnrollmentPoolItem.BeplAddrCity,
                State = benefitsEnrollmentPoolItem.BeplAddrState,
                PostalCode = benefitsEnrollmentPoolItem.BeplAddrZip,
                Country = benefitsEnrollmentPoolItem.BeplAddrCountry,
                Relationship = benefitsEnrollmentPoolItem.BeplRelationship,
                Gender = benefitsEnrollmentPoolItem.BeplGender,
                MaritalStatus = benefitsEnrollmentPoolItem.BeplMaritalStatus,
                IsFullTimeStudent = string.IsNullOrEmpty(benefitsEnrollmentPoolItem.BeplFullTimeFlag) || benefitsEnrollmentPoolItem.BeplFullTimeFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase),
                OrganizationId = benefitsEnrollmentPoolItem.BeplOrgId,
                IsBirthDateOnFile = benefitsEnrollmentPoolItem.BeplBirthdateOnFileFlag,
                IsGovernmentIdOnFile = benefitsEnrollmentPoolItem.BeplSsnOnFileFlag
            };
            logger.Debug(string.Format("EmployeeBenefitsEnrollmentPoolItem built successfully for benefits enrollment pool id: {0} and personId: {1}", benefitsEnrollmentPoolItem.BenEnrPoolId, benefitsEnrollmentPoolItem.BeplPersonId));
            return employeeBenefitsEnrollmentPoolItem;
        }

        /// <summary>
        /// Creates a new EnrollmentPeriodBenefit record based on given BenefitTypesGroup
        /// </summary>
        /// <param name="benefitType"></param>
        /// <returns></returns>
        private EnrollmentPeriodBenefit BuildEnrollmentPeriodBenefit(BenefitTypesGroup benefitType)
        {
            return new EnrollmentPeriodBenefit(benefitType.BendedIds, benefitType.BenefitTypesBenIds, benefitType.BenefitTypes)
            {
                BenefitDescription = !string.IsNullOrEmpty(benefitType.BendedSelfSvsDescs) ? benefitType.BendedSelfSvsDescs : benefitType.BendedDescs,
                IsDependentRequired = !string.IsNullOrEmpty(benefitType.EpbDependentReqd) && benefitType.EpbDependentReqd.Equals("Y", StringComparison.InvariantCultureIgnoreCase) ? true : false,
                IsBeneficiaryRequired = !string.IsNullOrEmpty(benefitType.EpbBeneficiaryReqd) && benefitType.EpbBeneficiaryReqd.Equals("Y", StringComparison.InvariantCultureIgnoreCase) ? true : false,
                IsHealthCareProviderRequired = !string.IsNullOrEmpty(benefitType.EpbProviderReqd) && benefitType.EpbProviderReqd.Equals("Y", StringComparison.InvariantCultureIgnoreCase) ? true : false,
                CostCalculationRequirement = ConvertToPeriodBenefitCostCalculationRequirement(benefitType.EpbCostCalcRequirement),
                InsuranceMultiplier = benefitType.EpbSalaryMultiplier,
                InsuranceRateDivisor = benefitType.EpbRateDivisor,
                MaximumBenefitAmount = benefitType.EpbMaximumAmount,
                MaximumBenefitPercent = benefitType.EpbMaxPercent,
                MinimumBenefitAmount = benefitType.EpbMinimumAmount,
                MaximumARBenefitAmount = benefitType.EpbMaximumArAmount,
                MaximumPayPeriodAmount = benefitType.EpbMaximumPayPeriodAmount,
                PlanHyperlink = !string.IsNullOrEmpty(benefitType.EpbPlanHyperlink) ? new Uri(benefitType.EpbPlanHyperlink).AbsoluteUri : null,
                RateHyperlink = !string.IsNullOrEmpty(benefitType.EpbRateHyperlink) ? new Uri(benefitType.EpbRateHyperlink).AbsoluteUri : null
            };
        }

        /// <summary>
        /// Converts a period benefit cost calculation requirement to the correct enum type
        /// </summary>
        /// <param name="periodBenefitCostCalculationRequirementCode"></param>
        /// <returns></returns>
        private PeriodBenefitCostCalculationRequirement ConvertToPeriodBenefitCostCalculationRequirement(string periodBenefitCostCalculationRequirementCode)
        {
            if (string.IsNullOrEmpty(periodBenefitCostCalculationRequirementCode))
            {
                return PeriodBenefitCostCalculationRequirement.None;
            }
            switch (periodBenefitCostCalculationRequirementCode.ToUpperInvariant())
            {
                case "A":
                    return PeriodBenefitCostCalculationRequirement.Amount;
                case "P":
                    return PeriodBenefitCostCalculationRequirement.Percentage;
                case "I":
                    return PeriodBenefitCostCalculationRequirement.Insurance;
                case "F":
                    return PeriodBenefitCostCalculationRequirement.Flex;
                default:
                    throw new ApplicationException("Unknown PeriodBenefitCostCalculationRequirementCode " + periodBenefitCostCalculationRequirementCode);
            }
        }

        /// <summary>
        /// Build an BenefitEnrollmentCompletionInfo Entity from CTX response
        /// </summary>
        /// <param name="employeeId">Employee Id of the user</param>
        /// <param name="enrollmentPeriodId">Id of the current enrollment period</param>
        /// <param name="enrollmentConfirmationDate">Date when the user submits the benefits for completion</param>
        /// <param name="errorMessages">List of error message from CTX</param>
        /// <returns>BenefitEnrollmentCompletionInfo Entity</returns>
        private BenefitEnrollmentCompletionInfo BuildBenefitEnrollmentCompletionInfo(string employeeId, string enrollmentPeriodId, DateTime? enrollmentConfirmationDate, List<string> errorMessages)
        {
            return new BenefitEnrollmentCompletionInfo(employeeId, enrollmentPeriodId, enrollmentConfirmationDate, errorMessages);
        }
        #endregion
    }
}