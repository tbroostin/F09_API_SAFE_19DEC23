/*Copyright 2015-2019 Ellucian Company L.P. and its affiliates.*/
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
        /// Get PersonStatus data based on the permissions of the current user.      
        /// </summary>
        /// <example>SelfService getting person-Statuses on behalf of an employee will return that employee's PersonStatuses</example>
        /// <example>SelfService getting person-Statuses on behalf of a supervisor will return that supervisor's PersonStatuses and all the PersonStatuses of the supervisors reports</example>
        /// <returns></returns>
        public async Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath;
                if (effectivePersonId == null)
                {
                    urlPath = UrlUtility.CombineUrlPath(_personEmploymentStatusesPath);
                }
                else
                {
                    urlPath = _personEmploymentStatusesPath + "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
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
        /// <returns></returns>
        public async Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath;
                if (effectivePersonId == null)
                {
                    urlPath = UrlUtility.CombineUrlPath(_personPositionsPath);
                }
                else
                {
                    urlPath = _personPositionsPath + "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
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
        /// <returns></returns>
        public async Task<IEnumerable<PersonPositionWage>> GetPersonPositionWagesAsync(string effectivePersonId = null)
        {
            try
            {
                string urlPath;
                if (effectivePersonId == null)
                {
                    urlPath = UrlUtility.CombineUrlPath(_personPositionWagesPath);
                }
                else
                {
                    urlPath = _personPositionWagesPath + "?" + UrlUtility.BuildEncodedQueryString("effectivePersonId", effectivePersonId);
                }
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
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

        public async Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographics2Async(string effectivePersonId = null)
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
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderHumanResourceDemographicsVersion2);
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
        /// Get specific HumanResourceDemographics data for a given ID
        /// </summary>
        /// <param name="id">The employees's ID for whom demographics are being requested</param>
        /// <accessComents>If requesting an ID other than the user's own, must have</accessComents>
        /// <returns></returns>
        public async Task<HumanResourceDemographics> GetSpecificHumanResourceDemographicsAsync(string id)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_humanResourceDemographicsPath, id);
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
        /// <returns></returns>
        public async Task<IEnumerable<PayCycle>> GetPayCyclesAsync()
        {
            try
            {
                // Build url path and create and execute a request to get all pay cycles
                string urlPath = UrlUtility.CombineUrlPath(_payCyclesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
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
    }
}
