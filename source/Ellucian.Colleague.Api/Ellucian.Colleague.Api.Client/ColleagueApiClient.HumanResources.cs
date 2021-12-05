/*Copyright 2015-2021 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Utility;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Client.Exceptions;
using System.Net.Http;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Colleague.Api.Client.Core;

namespace Ellucian.Colleague.Api.Client
{
    public partial class ColleagueApiClient
    {

        /// <summary>
        /// Obsolete as of API 1.16 for security reasons. Use GetPayrollDepositDirectives instead
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16.")]
        public Task<DirectDeposits> GetDirectDepositsAsync(string employeeId)
        {
            throw new InvalidOperationException("GetDirectDepositsAsync method is obsolete as of API 1.16. Use GetPayrollDepositDirectives instead.");
        }

        /// <summary>
        /// Obsolete as of API 1.16 for security reasons. Use UpdatePayrollDepositDirectives instead
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.16.")]
        public Task<DirectDeposits> UpdateDirectDepositsAsync(string employeeId, DirectDeposits directDeposits)
        {
            throw new InvalidOperationException("UpdateDirectDepositsAsync method is obsolete as of API 1.16. Use UpdatePayrollDepositDirectives instead.");
        }

        /// <summary>
        /// Returns benefits enrollment eligibility for an employee
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment eligibility</param>
        /// <returns>EmployeeBenefitsEnrollmentEligibility dto containing the enrollment period if eligible or a reson for ineligibility <see cref="Dtos.HumanResources.EmployeeBenefitsEnrollmentEligibility"></see> </returns>
        public async Task<EmployeeBenefitsEnrollmentEligibility> GetEmployeeBenefitsEnrollmentEligibilityAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeesPath, employeeId, _employeeBenefitsEnrollmentEligibilityPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<EmployeeBenefitsEnrollmentEligibility>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get benefits enrollment eligibility");
                throw;
            }
        }

        /// <summary>
        /// Returns benefits enrollment pool items (dependent and beneficiary information) for an employee
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment pool items</param>
        /// <returns>List of EmployeeBenefitsEnrollmentPoolItem dtos containing the dependent and beneficiary information for employee <see cref="Dtos.HumanResources.EmployeeBenefitsEnrollmentPoolItem"></see></returns>
        public async Task<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>> GetEmployeeBenefitsEnrollmentPoolAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeesPath, employeeId, _employeeBenefitsEnrollmentPoolPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get benefits enrollment pool items");
                throw;
            }
        }

        /// <summary>
        /// Returns benefits enrollment configuration
        /// </summary>
        /// <returns>BenefitsEnrollmentConfiguration dto containing configuration information <see cref="Dtos.HumanResources.BenefitsEnrollmentConfiguration"></see> </returns>
        public async Task<BenefitsEnrollmentConfiguration> GetBenefitsEnrollmentConfigurationAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_benefitsEnrollmentConfigurationPath);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<BenefitsEnrollmentConfiguration>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException ex)
            {
                logger.Error(ex, "Unable to get the benefits enrollment configuration.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the benefits enrollment  configuration.");
                throw;
            }
        }
        /// <summary>
        /// Get Employee current benefits data based on the permissions of the current user.
        /// </summary>
        /// <returns><returns>EmployeeBenefits DTO containing list of employee's current benefits.<see cref="Dtos.HumanResources.EmployeeBenefits"></see></returns></returns>
        public async Task<EmployeeBenefits> GetEmployeeCurrentBenefitsAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeeCurrentBenefitsPath);
                string query;
                if (effectivePersonId != null)
                {
                    query = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                    urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<EmployeeBenefits>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Employee current benefits data");
                throw;
            }
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
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeesPath, employeeId, _employeeBenefitsEnrollmentPackagePath);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<EmployeeBenefitsEnrollmentPackage>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get employee benefits package");
                throw;
            }
        }

        /// <summary>
        /// This endpoint will add benefits enrollment pool information to an employee. 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can add benefits enrollment pool information.     
        /// The endpoint will reject add benefits enrollment pool if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="employeeId">Required parameter to add benefits enrollment pool information to an employee</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"><see cref="EmployeeBenefitsEnrollmentPoolItem">EmployeeBenefitsEnrollmentPoolItem DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentPoolItem">Newly added EmployeeBenefitsEnrollmentPoolItem DTO object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeId or employeeBenefitsEnrollmentPoolItem are not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeBenefitsEnrollmentPoolItem is not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to add benefits enrollment pool information.</exception>
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

            if (string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.OrganizationName) && string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.LastName))
            {
                throw new ArgumentNullException("OrganizationName or LastName is required.");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeesPath, employeeId, _employeeBenefitsEnrollmentPoolPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePostRequestWithResponseAsync(employeeBenefitsEnrollmentPoolItem, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmployeeBenefitsEnrollmentPoolItem>(await response.Content.ReadAsStringAsync());
            }
            // If the HTTP request fails, the benefits enrollment pool information was probably not added to an employee
            catch (HttpRequestFailedException hre)
            {
                string message = "Adding benefits enrollment pool information to an employee is failed.";
                logger.Error(hre, message);
                throw new InvalidOperationException(message, hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                string message = "Unable to add benefits enrollment pool to an employee.";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// This endpoint will update benefits enrollment pool information to an employee. 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update benefits enrollment pool information.     
        /// The endpoint will reject updated benefits enrollment pool if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="employeeId">Required parameter to update benefits enrollment pool information of an employee</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"><see cref="EmployeeBenefitsEnrollmentPoolItem">EmployeeBenefitsEnrollmentPoolItem DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentPoolItem">Updated EmployeeBenefitsEnrollmentPoolItem DTO object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeId or employeeBenefitsEnrollmentPoolItem are not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeBenefitsEnrollmentPoolItem is not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to add benefits enrollment pool information.</exception>
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

            if (string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.OrganizationName) && string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.LastName))
            {
                throw new ArgumentNullException("OrganizationName or LastName is required.");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeesPath, employeeId, _employeeBenefitsEnrollmentPoolPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePutRequestWithResponseAsync(employeeBenefitsEnrollmentPoolItem, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmployeeBenefitsEnrollmentPoolItem>(await response.Content.ReadAsStringAsync());
            }
            // If the HTTP request fails, the benefits enrollment pool information was probably not updated to an employee
            catch (HttpRequestFailedException hre)
            {
                string message = "Updating benefits enrollment pool information to an employee is failed.";
                logger.Error(hre, message);
                throw new InvalidOperationException(message, hre);
            }
            // HTTP request successful, but some other problem encountered...
            catch (Exception ex)
            {
                string message = "Unable to update benefits enrollment pool to an employee.";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// This endpoint will update benefits enrollment information of an employee for the given benefit types specified
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update their own benefits enrollment information
        /// The endpoint will reject the updated benefits enrollment information if the employee does not have a valid permission
        /// </accessComments>    
        /// <param name="employeeId">Required parameter to update benefits enrollment information</param>
        /// <param name="employeeBenefitsEnrollmentInfo"><see cref="EmployeeBenefitsEnrollmentInfo">EmployeeBenefitsEnrollmentInfo DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentInfo">Updated EmployeeBenefitsEnrollmentInfo DTO object</see></returns>
        public async Task<EmployeeBenefitsEnrollmentInfo> UpdateEmployeeBenefitsEnrollmentInfoAsync(string employeeId, EmployeeBenefitsEnrollmentInfo employeeBenefitsEnrollmentInfo)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            if (employeeBenefitsEnrollmentInfo == null)
            {
                throw new ArgumentNullException("employeeBenefitsEnrollmentInfo");
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentInfo.EnrollmentPeriodId))
            {
                throw new ArgumentNullException("employeeBenefitsEnrollmentInfo.EnrollmentPeriodId");
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentInfo.BenefitPackageId))
            {
                throw new ArgumentNullException("employeeBenefitsEnrollmentInfo.BenefitPackageId");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeesPath, employeeId, _employeeBenefitsEnrollmentInfoPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePutRequestWithResponseAsync(employeeBenefitsEnrollmentInfo, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<EmployeeBenefitsEnrollmentInfo>(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestFailedException hre)
            {
                string message = "Updating benefits enrollment information for employee failed.";
                logger.Error(hre, message);
                throw new InvalidOperationException(message, hre);
            }
            catch (Exception ex)
            {
                string message = "Unable to update benefits enrollment information for employee .";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Queries benefits enrollment information based on specified criteria; if no benefit type is provided, all of the employee's elected benefit information is returned
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Set of available enrollment period benefits by benefit type</returns>
        public async Task<IEnumerable<EnrollmentPeriodBenefit>> QueryEnrollmentPeriodBenefitsAsync(BenefitEnrollmentBenefitsQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            if (string.IsNullOrEmpty(criteria.BenefitTypeId))
            {
                throw new ArgumentException("benefitTypeId cannot be empty", "benefitTypeId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _benefitsEnrollmentBenefitsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<EnrollmentPeriodBenefit>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get benefits enrollment benefits");
                throw;
            }
        }

        /// <summary>
        /// Queries an employee's benefit enrollment information for the given criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Employee benefits enrollment information object</returns>
        public async Task<EmployeeBenefitsEnrollmentInfo> QueryEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfoQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            if (string.IsNullOrEmpty(criteria.EmployeeId))
            {
                throw new ArgumentException("EmployeeId cannot be empty", "EmployeeId");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _benefitsEnrollmentInfoPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<EmployeeBenefitsEnrollmentInfo>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get benefits enrollment information");
                throw;
            }
        }

        /// <summary>
        /// This endpoint submits/re-opens the benefits elected by an employee.
        /// A boolean flag present in the input criteria object indicates whether to submit or re-open the benefit elections.
        /// </summary>       
        /// <param name="criteria">BenefitEnrollmentCompletionCriteria object</param>       
        /// <returns><see cref="BenefitEnrollmentCompletionInfo">BenefitEnrollmentCompletionInfo DTO</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the required parameters in the input object have no value (or) in case of any unexpected error while processing the request.</exception>
        public async Task<BenefitEnrollmentCompletionInfo> SubmitOrReOpenBenefitElectionsAsync(BenefitEnrollmentCompletionCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
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
            try
            {
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, _submitOrReOpenBenefitElectionsPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<BenefitEnrollmentCompletionInfo>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to submit/re-open the benefit elections");
                throw;
            }
        }

        /// <summary>
        /// Gets the beneficiary categories
        /// </summary>
        /// <returns>Returns a list of Beneficiary Category DTOs</returns>
        public async Task<IEnumerable<BeneficiaryCategory>> GetBeneficiaryCategoriesAsync()
        {

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeesPath, _beneficiaryCategoriesPath);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<BeneficiaryCategory>>(await response.Content.ReadAsStringAsync());
                return resource;
            }

            catch (Exception e)
            {
                logger.Error(e, "Unable to get beneficiary categories");
                throw;
            }
        }

        /// <summary>
        /// Returns benefits enrollment acknowledgement pdf report
        /// </summary>
        /// <param name="employeeId">Id of employee to get enrolled benefits</param>
        /// <returns>The pdf report of enrolled benefits information</returns>
        public async Task<Tuple<byte[], string>> GetBenefitsEnrollmentAcknowledgementReportAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId", "Employee cannot be null or empty.");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeesPath, employeeId, _employeeBenefitsEnrollmentAcknowledgementPath);

                var headers = new NameValueCollection();

                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPdfVerion1);

                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var fileName = response.Content.Headers.ContentDisposition.FileName;

                var resource = await response.Content.ReadAsByteArrayAsync();

                return new Tuple<byte[], string>(resource, fileName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get benefits enrollment acknowledgement report");
                throw;
            }
        }

        /// <summary>
        /// Get PersonStatus data based on the permissions of the current user.      
        /// </summary>
        /// <example>SelfService getting person-Statuses on behalf of an employee will return that employee's PersonStatuses</example>
        /// <example>SelfService getting person-Statuses on behalf of a supervisor will return that supervisor's PersonStatuses and all the PersonStatuses of the supervisors reports</example>
        /// <param name="effectivePersonId">person id requesting this info</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesAsync(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_personEmploymentStatusesPath);
                string queryString = string.Empty;
                if (!string.IsNullOrEmpty(effectivePersonId))
                {
                    if (lookupStartDate.HasValue)
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId, "lookupStartDate", lookupStartDate.Value.ToShortDateString());
                    }
                    else
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                    }
                }
                else
                {
                    if (lookupStartDate.HasValue)
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("lookupStartDate", lookupStartDate.Value.ToShortDateString());
                    }
                }

                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PersonEmploymentStatus>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PersonEmploymentStatus data");
                throw;
            }
        }

        /// <summary>
        /// Get PersonPosition data based on the permissions of the current user.      
        /// </summary>
        /// <example>SelfService getting person-positions on behalf of an employee will return that employee's PersonPositions</example>
        /// <example>SelfService getting person-positions on behalf of a supervisor will return that supervisor's PersonPositions and all the PersonPositions of the supervisors reports</example>
        /// <param name="effectivePersonId">Optional parameter for effective person Id</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_personPositionsPath);
                string queryString = string.Empty;
                if (!string.IsNullOrEmpty(effectivePersonId))
                {
                    if (lookupStartDate.HasValue)
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId, "lookupStartDate", lookupStartDate.Value.ToShortDateString());
                    }
                    else
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                    }
                }
                else
                {
                    if (lookupStartDate.HasValue)
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("lookupStartDate", lookupStartDate.Value.ToShortDateString());
                    }
                }

                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PersonPosition>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PersonPosition data");
                throw;
            }
        }

        /// <summary>
        /// Get PersonPosition data based on the permissions of the current user.
        /// </summary>
        /// <example>SelfService getting person-position-wages on behalf of an employee will return that employee's PersonPositionWages</example>
        /// <example>SelfService getting person-position-wages on behalf of a supervisor will return that supervisor's PersonPositionWages and all the PersonPositionWages of the supervisors reports</example>    
        /// <param name="effectivePersonId">person id requesting this info</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonPositionWage>> GetPersonPositionWagesAsync(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_personPositionWagesPath);
                string queryString = string.Empty;
                if (!string.IsNullOrEmpty(effectivePersonId))
                {
                    if (lookupStartDate.HasValue)
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId, "lookupStartDate", lookupStartDate.Value.ToShortDateString());
                    }
                    else
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                    }
                }
                else
                {
                    if (lookupStartDate.HasValue)
                    {
                        queryString = UrlUtility.BuildEncodedQueryString("lookupStartDate", lookupStartDate.Value.ToShortDateString());
                    }
                }

                var combinedUrl = UrlUtility.CombineUrlPathAndArguments(urlPath, queryString);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(combinedUrl, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PersonPositionWage>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PersonPositionWage data");
                throw;
            }
        }

        /// <summary>
        /// Get PersonStipend data based on the permissions of the current user.
        /// </summary>
        /// <example>SelfService getting person-stipend on behalf of an employee will return that employee's PersonStipend objects</example>
        /// <example>SelfService getting person-stipend on behalf of a supervisor will return that supervisor's PersonStipend objects and all the PersonStipend objects of the supervisees</example>      
        /// <returns></returns>
        public async Task<IEnumerable<PersonStipend>> GetPersonStipendAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath;
                if (effectivePersonId == null)
                {
                    urlPath = UrlUtility.CombineUrlPath(_personStipendPath);
                }
                else
                {
                    urlPath = _personStipendPath + "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PersonStipend>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PersonStipend data");
                throw;
            }
        }

        /// <summary>
        /// Get all the employment Positions
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Position>> GetPositionsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_positionsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Position>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Position data");
                throw;
            }
        }

        /// <summary>
        /// Get all EarningsType data
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<EarningsType>> GetEarningsTypesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_earningsTypesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<EarningsType>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get EarningsType data");
                throw;
            }
        }

        /// <summary>
        /// Get all EarningsTypeGroups
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<EarningsTypeGroup>> GetEarningsTypeGroupsAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_earningsTypeGroupsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<EarningsTypeGroup>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get EarningsTypeGroup data");
                throw;
            }
        }

        /// <summary>
        /// Get all HumanResourceDemographics data available for the effectivePerson
        /// NOTE: This method is an earlier version and has some performance issues, so it is not recommended
        /// </summary>
        /// <accessComments>
        /// 1. Must have the APPROVE.REJECT.TIME.ENTRY permission to retrieve supervisee HumanResourceDemographics
        /// 2. If the user has the VIEW.ALL.EARNINGS.STATEMENTS permission, it will allow them to retrieve HumanResourceDemographics for all active and inactive employees - 
        /// performance on this is not ideal and therefore version 2 of this method is recommended
        /// </accessComents>
        /// <returns>A collection of HumanResourceDemographics</returns>
        public async Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographicsAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_humanResourceDemographicsPath);
                string query;
                if (effectivePersonId != null)
                {
                    query = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                    urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderHumanResourceDemographics);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<HumanResourceDemographics>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get HumanResourceDemographics data");
                throw;
            }
        }

        /// <summary>
        /// Get all HumanResourceDemographics data available for the effectivePerson
        /// NOTE: This is second version of the GetHumanResourceDemographics method and is recommended for performance reasons
        /// </summary>
        /// <accessComments>
        /// 1. Must have the APPROVE.REJECT.TIME.ENTRY permission to retrieve supervisee HumanResourceDemographics
        /// </accessComents>
        /// <returns>A collection of HumanResourceDemographics</returns>
        public async Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographics2Async(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_humanResourceDemographicsPath);
                Dictionary<string, string> urlArgCollection = new Dictionary<string, string>();
                

                if (effectivePersonId != null)
                {
                    urlArgCollection.Add("effectivePersonId", effectivePersonId);
                }

                if (lookupStartDate.HasValue)
                {
                    urlArgCollection.Add("lookupStartDate", lookupStartDate.Value.ToShortDateString());
                }

                string urlPathWithArguments = UrlUtility.CombineEncodedUrlPathAndArguments(urlPath, urlArgCollection);

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderHumanResourceDemographicsVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPathWithArguments, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<HumanResourceDemographics>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get HumanResourceDemographics data");
                throw;
            }
        }

        /// <summary>
        /// Get specific HumanResourceDemographics data for a given ID
        /// </summary>
        /// <param name="id">The employees's ID for whom demographics are being requested</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <accessComents>If requesting an ID other than the user's own, must have</accessComents>
        /// <returns></returns>
        public async Task<HumanResourceDemographics> GetSpecificHumanResourceDemographicsAsync(string id, string effectivePersonId = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_humanResourceDemographicsPath, id);
                if (!string.IsNullOrWhiteSpace(effectivePersonId))
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderHumanResourceDemographics);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<HumanResourceDemographics>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get HumanResourceDemographics data");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a collection of HumanResourceDemographics from a criteria object and an optional effectivePersonId
        /// </summary>
        /// <param name="criteria">
        /// Query object to retrieve HumanResourceDemographics:
        /// 1. ids: Collection of Ids to retrieve the HumanResourceDemographics of
        /// </param>
        /// <param name="effectivePersonId">(Optional) User the CurrentUser is acting as - if this Id is not the CurrentUser's, the Id must be an active valid ProxySubject for the CurrentUser</param>
        /// <accessComments>
        /// 1. Must have the APPROVE.REJECT.TIME.ENTRY permission in order to access supervisee HumanResourceDemographics
        /// </accessComments>
        /// <returns>A collection of HumanResourceDemographics</returns>
        public async Task<IEnumerable<HumanResourceDemographics>> QueryHumanResourceDemographicsAsync(HumanResourceDemographicsQueryCriteria criteria, string effectivePersonId = null)
        {
            if (criteria == null || !criteria.Ids.Any())
            {
                throw new ArgumentNullException("Neither the criteria nor the criteria IDs may be null");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _humanResourceDemographicsPath);
                string query;

                if (string.IsNullOrEmpty(effectivePersonId))
                {
                    query = UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                    urlPath = UrlUtility.CombineUrlPathAndArguments(urlPath, query);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderHumanResourceDemographics);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<HumanResourceDemographics>>(await response.Content.ReadAsStringAsync());
                return resource;
            }

            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get HumanResourceDemographics data");
                throw;
            }

        }

        /// <summary>
        /// Get PayCycle data
        /// </summary>
        /// <param name="lookbackDate">A optional date which is used to filter previous pay periods with end dates prior to this date.</param>
        /// <returns></returns>
        public async Task<IEnumerable<PayCycle>> GetPayCyclesAsync(DateTime? lookbackDate = null)
        {
            try
            {
                // Build url path and create and execute a request to get all pay cycles
                string urlPath = UrlUtility.CombineUrlPath(_payCyclesPath);
                Dictionary<string, string> urlArgCollection = new Dictionary<string, string>();
                if (lookbackDate.HasValue)
                {
                    urlArgCollection.Add("lookbackDate", lookbackDate.Value.ToShortDateString());
                }

                string urlPathWithArguments = UrlUtility.CombineEncodedUrlPathAndArguments(urlPath, urlArgCollection);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPathWithArguments, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<PayCycle>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get PayCycle data");
                throw;
            }
        }

        /// <summary>
        /// Retrieve a set of statement DTOs from the human resources area.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        [Obsolete("Obsolete as of API version 1.14, use GetTaxFormStatements2 instead")]
        public async Task<IEnumerable<TaxFormStatement>> GetTaxFormStatements(string personId, TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is required.");

            try
            {
                // Create and execute a request to get all projects
                string urlPath = UrlUtility.CombineUrlPath(_taxFormStatementsPath, personId, taxForm.ToString());
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                AddLoggingRestrictions(ref headers, Core.LoggingRestrictions.DoNotLogRequestContent | Core.LoggingRestrictions.DoNotLogResponseContent);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                var resource = JsonConvert.DeserializeObject<List<TaxFormStatement>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get tax form statements.");
                throw;
            }
        }

        /// <summary>
        /// Get a list of summaries of pay statements available to the current user filtered down to any of the given filter criteria.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="hasOnlineConsentFilter"></param>
        /// <param name="payDateFilter"></param>
        /// <param name="payCycleIdFilter"></param>
        /// <param name="startDateFilter"></param>
        /// <param name="endDateFilter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayStatementSummary>> GetPayStatementSummariesAsync(string employeeId = null,
            bool? hasOnlineConsentFilter = null,
            DateTime? payDateFilter = null,
            string payCycleIdFilter = null,
            DateTime? startDateFilter = null,
            DateTime? endDateFilter = null)
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payStatementsPath);
                var queryArray = new List<string>();
                if (!string.IsNullOrWhiteSpace(employeeId))
                {
                    queryArray.Add("employeeId");
                    queryArray.Add(employeeId);
                }
                if (hasOnlineConsentFilter.HasValue)
                {
                    queryArray.Add("hasOnlineConsent");
                    queryArray.Add(hasOnlineConsentFilter.Value.ToString());
                }
                if (payDateFilter.HasValue)
                {
                    queryArray.Add("payDate");
                    queryArray.Add(payDateFilter.Value.ToShortDateString());
                }
                if (!string.IsNullOrWhiteSpace(payCycleIdFilter))
                {
                    queryArray.Add("payCycleId");
                    queryArray.Add(payCycleIdFilter);
                }
                if (startDateFilter.HasValue)
                {
                    queryArray.Add("startDate");
                    queryArray.Add(startDateFilter.Value.ToShortDateString());
                }
                if (endDateFilter.HasValue)
                {
                    queryArray.Add("endDate");
                    queryArray.Add(endDateFilter.Value.ToShortDateString());
                }
                if (queryArray.Any())
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString(queryArray.ToArray());
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                return JsonConvert.DeserializeObject<IEnumerable<PayStatementSummary>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get pay statement summaries");
                throw;
            }
        }

        /// <summary>
        /// Get a PDF Report of a single Pay Statement for an employee
        /// </summary>
        public async Task<Tuple<string, byte[]>> GetPayStatementPdf(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payStatementsPath, id);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPdfVerion1);

                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                IEnumerable<string> disposition;
                response.Content.Headers.TryGetValues("content-disposition", out disposition);
                var fileName = string.Format("ADVICE_{0}.pdf", DateTime.Now.ToString("ddMMMyyyy"));
                if (disposition != null && disposition.Any(d => d.ToLower().Contains(".pdf")))
                {
                    fileName = disposition.First(d => d.ToLower().Contains(".pdf")).Split('=')[1].Replace("-", "");
                }

                var resource = await response.Content.ReadAsByteArrayAsync();

                return new Tuple<string, byte[]>(fileName, resource);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get pay statement pdf");
                throw;
            }
        }

        /// <summary>
        /// Get a PDF containing multiple earnings statements for the given ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<byte[]> GetMultiplePayStatementPdf(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids");
            }

            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payStatementsPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderPdfVerion1);

                var response = await ExecutePostRequestWithResponseAsync(ids, urlPath, headers: headers);
                var resource = await response.Content.ReadAsByteArrayAsync();
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to build mulitple pay statement pdf");
                throw;
            }
        }

        /// <summary>
        /// Get configurations for pay statement
        /// </summary>
        /// <returns></returns>
        public async Task<PayStatementConfiguration> GetPayStatementConfigurationAsync()
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_payStatementConfigurationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                return JsonConvert.DeserializeObject<PayStatementConfiguration>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get pay statement configuration asynchronously");
                throw;
            }
        }

        /// <summary>
        /// Gets leave balance configuration
        /// </summary>
        /// <returns></returns>
        public async Task<LeaveBalanceConfiguration> GetLeaveBalanceConfigurationAsync()
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_leaveConfigurationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                return JsonConvert.DeserializeObject<LeaveBalanceConfiguration>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get leave balance configuration asynchronously");
                throw;
            }
        }

        /// <summary>
        /// Gets Employee Leave Plans
        /// </summary>
        /// <param name="effectivePersonId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EmployeeLeavePlan>> GetEmployeeLeavePlansAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath;
                if (string.IsNullOrWhiteSpace(effectivePersonId))
                {
                    urlPath = UrlUtility.CombineUrlPath(_employeeLeavePlansPath);
                }
                else
                {
                    urlPath = _employeeLeavePlansPath + "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                return JsonConvert.DeserializeObject<IEnumerable<EmployeeLeavePlan>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get employee leave plans");
                throw;
            }
        }

        /// <summary>
        /// Returns Employee Compensation Details 
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user used for retrieving compensation details </param>
        /// <param name="salaryAmount">Estimated Annual Salary amount
        /// If this value is provided,it will be used in computing compensation details in Total Compensation Colleague Transaction.
        /// When not provided, the salary amount will be computed in Total Compensation Colleague Transaction
        /// </param>
        /// <returns>Employee Compensation DTO containing Compensation Details(Benefit-Deductions,Taxes and Stipends).<see cref="Dtos.HumanResources.EmployeeCompensation"></see> </returns>
        /// <accessComments>
        /// Any authenticated user can
        /// 1) view their own compensation information; 
        /// 2) view other employee's compensation information upon having admin access (i.e. VIEW.ALL.TOTAL.COMPENSATION permission)
        /// </accessComments>
        public async Task<EmployeeCompensation> GetEmployeeCompensationAsync(string effectivePersonId = null, decimal? salaryAmount = null)
        {
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_employeeCompensationPath);
                List<string> QueryArray = new List<string>();
                if (!string.IsNullOrEmpty(effectivePersonId))
                {
                    QueryArray.Add("effectivePersonId");
                    QueryArray.Add(effectivePersonId);
                }

                if (salaryAmount.HasValue)
                {
                    QueryArray.Add("salaryAmount");
                    QueryArray.Add(salaryAmount.ToString());
                }

                if (QueryArray.Any())
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString(QueryArray.ToArray());
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);

                return JsonConvert.DeserializeObject<EmployeeCompensation>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get employee compensation details");
                throw;
            }

        }

        /// <summary>
        /// Gets all leave requests for the currently authenticated API user .
        /// All leave requests will be returned regardless of status.
        /// The endpoint will not return the leave requests if:
        ///     1.  403 - User does not have permission to get requested leave request
        ///</summary>
        /// <accessComments>
        /// If the current user is an employee, all of the employee's leave requests will be returned.
        /// </accessComments>
        /// <param name="effectivePersonId">
        ///  Optional parameter for passing effective person Id
        /// </param>
        /// <returns>A list of Leave Requests</returns>
        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath;
                if (effectivePersonId == null)
                {
                    urlPath = UrlUtility.CombineUrlPath(_employeeLeaveRequestPath);
                }
                else
                {
                    urlPath = _employeeLeaveRequestPath + "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<List<LeaveRequest>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                var message = "unable to get leave request";
                logger.Error(e, message);
                throw;
            }
        }

        /// <summary>
        ///  Returns the LeaveRequest information corresponding to the input id.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can
        /// 1) view their own leave request information         
        /// </accessComments>
        /// <param name="id">Leave Request Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>LeaveRequest DTO</returns>
        public async Task<LeaveRequest> GetLeaveRequestInfoByLeaveRequestIdAsync(string id, string effectivePersonId = null)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeeLeaveRequestPath, id);
                if (!string.IsNullOrWhiteSpace(effectivePersonId))
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                return JsonConvert.DeserializeObject<LeaveRequest>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the leave request information");
                throw;
            }
        }

        /// <summary>
        /// Gets the Approved Leave Requests for a timecard week based on the date range.
        /// </summary>
        /// <param name="startDate">Start date of timecard </param>
        /// <param name="endDate">End date of timecard</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>List of LeaveRequest DTO</returns>
        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsForTimeEntryAsync(DateTime startDate, DateTime endDate, string effectivePersonId = null)
        {
            if (startDate == null)
            {
                throw new ArgumentNullException("startDate");
            }

            if (endDate == null)
            {
                throw new ArgumentNullException("endDate");
            }
            try
            {
                var urlPath = UrlUtility.CombineUrlPath(_employeeLeaveRequestsForTimeEntry);

                Dictionary<string, string> urlArgCollection = new Dictionary<string, string>();
                urlArgCollection.Add("startDate", startDate.ToShortDateString());
                urlArgCollection.Add("endDate", endDate.ToShortDateString());
                if (!string.IsNullOrEmpty(effectivePersonId))
                {
                    urlArgCollection.Add("effectivePersonId", effectivePersonId);
                }

                string urlPathWithArguments = UrlUtility.CombineEncodedUrlPathAndArguments(urlPath, urlArgCollection);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPathWithArguments, headers: headers);
                return JsonConvert.DeserializeObject<List<LeaveRequest>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to get the approved leave request information for time entry");
                throw;
            }
        }

        /// <summary>
        /// Creates a single Leave Request. This POST endpoint will create a Leave Request along with its associated leave request details 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can create their own leave request     
        /// The endpoint will reject the creation of a Leave Request if Employee does not have the correct permission.
        /// </accessComments>
        /// <param name="leaveRequest">Leave Request DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns>Newly created Leave Request Object</returns>
        public async Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest, string effectivePersonId = null)
        {
            if (leaveRequest == null)
            {
                throw new ArgumentNullException("leaverequest");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeeLeaveRequestPath);
                if (!string.IsNullOrWhiteSpace(effectivePersonId))
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePostRequestWithResponseAsync(leaveRequest, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<LeaveRequest>(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestFailedException hrfe)
            {
                if (hrfe.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    if (hrfe.Data.Contains("ColleagueApiErrorDetail"))
                    {
                        var errorDetail = hrfe.Data["ColleagueApiErrorDetail"] as ColleagueApiErrorDetail;
                        if (errorDetail.Conflicts.Any())
                        {
                            throw new ExistingResourceException(hrfe.Message, errorDetail.Conflicts.First());
                        }
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                var message = "Unable to create leave request";
                logger.Error(e, message);
                throw;
            }
        }

        /// <summary>
        /// This endpoint will update an existing Leave Request along with its Leave Request Details. 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update their own leave request.     
        /// The endpoint will reject the update of a Leave Request if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="leaveRequest"><see cref="LeaveRequest">Leave Request DTO</see></param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns><see cref="LeaveRequest">Newly updated Leave Request object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the leaveRequest DTO is present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to update the leave request.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if the leave request record to be edited doesn't exist in the DB.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if the leave request record to be edited is locked or if a duplicate leave request record already exists in the DB.</exception>
        public async Task<LeaveRequest> UpdateLeaveRequestAsync(LeaveRequest leaveRequest, string effectivePersonId = null)
        {
            if (leaveRequest == null)
            {
                throw new ArgumentNullException("leaveRequest");
            }
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeeLeaveRequestPath);
                if (!string.IsNullOrWhiteSpace(effectivePersonId))
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

                var response = await ExecutePutRequestWithResponseAsync(leaveRequest, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<LeaveRequest>(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestFailedException hrfe)
            {
                if (hrfe.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    if (hrfe.Data.Contains("ColleagueApiErrorDetail"))
                    {
                        var errorDetail = hrfe.Data["ColleagueApiErrorDetail"] as ColleagueApiErrorDetail;
                        if (errorDetail.Conflicts.Any())
                        {
                            throw new ExistingResourceException(hrfe.Message, errorDetail.Conflicts.First());
                        }
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                var message = "Unable to update the leave request";
                logger.Error(e, message);
                throw;
            }
        }

        /// <summary>
        /// Create a Leave Request Status record.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can create status for their own leave request     
        /// The endpoint will reject the creation of a Leave Request Status if Employee does not have the correct permission.
        /// </accessComments>
        /// <param name="status">Leave Request Status DTO</param>
        /// <param name="effectivePersonId">Optional parameter - Current user or proxy user person id.</param>
        /// <returns>Newly created Leave Request Status</returns>
        public async Task<LeaveRequestStatus> CreateLeaveRequestStatusAsync(LeaveRequestStatus status, string effectivePersonId = null)
        {
            if (status == null)
            {
                throw new ArgumentNullException("status");
            }
            try
            {
                string urlPath = _employeeLeaveRequestStatusesPath;

                if (!string.IsNullOrWhiteSpace(effectivePersonId))
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(status, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<LeaveRequestStatus>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                var message = "Unable to create leave request status";
                logger.Error(e, message);
                throw;
            }
        }

        /// <summary>
        /// This endpoint will create a new leave request comment associated with a leave request. 
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can create a comment associated with their own leave request.     
        /// The endpoint will reject the creation of comment if employee does not have a valid permission.
        /// </accessComments>     
        /// <param name="leaveRequestComment">Leave Request Comment DTO</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id</param>
        /// <returns><see cref="LeaveRequestComment">Leave Request Comment DTO</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to create the leave request comment.</exception
        public async Task<LeaveRequestComment> CreateLeaveRequestCommentsAsync(LeaveRequestComment leaveRequestComment, string effectivePersonId = null)
        {
            if (leaveRequestComment == null)
                throw new ArgumentNullException("leaveRequestComment");


            if (string.IsNullOrWhiteSpace(leaveRequestComment.LeaveRequestId))
                throw new ArgumentException("Comments must be applied to an associable entity");

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_employeeLeaveRequestCommentsPath);

                if (!string.IsNullOrWhiteSpace(effectivePersonId))
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(leaveRequestComment, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<LeaveRequestComment>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                var message = "Unable to create leave request comment";
                logger.Error(e, message);
                throw;
            }
        }

        /// <summary>
        /// Gets the HumanResourceDemographics information of supervisors for the given position of a supervisee.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can access the HumanResourceDemographics information of their own supervisors.
        /// </accessComments>
        /// <param name="id">Position Id</param>
        /// <param name="effectivePersonId">Optional parameter for passing effective person Id.</param>
        /// <returns>List of HumanResourceDemographics DTOs</returns>
        public async Task<IEnumerable<HumanResourceDemographics>> GetSupervisorsByPositionIdAsync(string id, string effectivePersonId = null)
        {
            try
            {
                string urlPath = _positionSupervisorsPath;
                if (!string.IsNullOrWhiteSpace(effectivePersonId))
                {
                    urlPath += "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogResponseContent);
                var response = await ExecutePostRequestWithResponseAsync(id, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<HumanResourceDemographics>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the position supervisors information.");
                throw;
            }
        }

        /// <summary>
        /// This end point returns all the supervisees for the currently authenticated leave approver.       
        /// The endpoint will not return the supervisees if:
        ///     1.  403 - User does not have permission to get supervisee information
        /// </summary>
        /// <accessComments>
        ///  Current user must be Leave Approver/supervisor (users with the permission APPROVE.REJECT.LEAVE.REQUEST) to fetch all of their supervisees
        /// </accessComments>
        /// <param name="effectivePersonId">
        ///  Optional parameter for passing effective person Id
        /// </param>
        /// <returns><see cref="HumanResourceDemographics">List of HumanResourceDemographics DTOs</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to fetch supervisees.</exception>
        public async Task<IEnumerable<HumanResourceDemographics>> GetSuperviseesByPrimaryPositionForSupervisorAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath;
                if (effectivePersonId == null)
                {
                    urlPath = UrlUtility.CombineUrlPath(_leaveApprovalSuperviseesPath);
                }
                else
                {
                    urlPath = _leaveApprovalSuperviseesPath + "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }

                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<HumanResourceDemographics>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the Supervisees for leave approval.");
                throw;
            }
        }
      
        /// <summary>
        /// Queries employee information summary based on the specified criteria.
        /// Either a supervisor id or employee ids must be specified (or both)
        /// </summary>
        /// <param name="criteria">criteria to use for querying</param>
        /// <returns>a list of EmployeeSummary DTOs</returns>
        public async Task<IEnumerable<EmployeeSummary>> QueryEmployeeSummaryAsync(EmployeeSummaryQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _employeeSummaryPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<EmployeeSummary>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve employee summary data for specified criteria");
                throw;
            }
        }

        /// <summary>
        /// Queries employee leave plans based on the specified criteria.
        /// Either a supervisor id or employee ids must be specified (or both)
        /// </summary>
        /// <param name="criteria">criteria to use for querying</param>
        /// <returns>a list of EmployeeLeavePlan DTOs</returns>
        public async Task<IEnumerable<EmployeeLeavePlan>> QueryEmployeeLeavePlanAsync(EmployeeLeavePlanQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_qapiPath, _employeeLeavePlansPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(criteria, urlPath, headers: headers);
                return JsonConvert.DeserializeObject<IEnumerable<EmployeeLeavePlan>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve employee leave plan data for specified criteria");
                throw;
            }
        }
    }
}
