//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes payroll deduction arrangement change data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayrollDeductionArrangementsController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IPayrollDeductionArrangementService _payrollDeductionArrangementsService;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="payrollDeductionArrangementsService"></param>
        public PayrollDeductionArrangementsController(ILogger logger, IPayrollDeductionArrangementService payrollDeductionArrangementsService)
        {
            this._logger = logger;
            this._payrollDeductionArrangementsService = payrollDeductionArrangementsService;
        }

        #region Version 7
        /// <summary>
        /// Accept requests from external systems for new employee deductions in the authoritative HR system.
        /// </summary>
        /// <param name="page">Page of items for Paging</param>
        /// <param name="person">Person GUID filter</param>
        /// <param name="contribution">Contribution ID filter</param>
        /// <param name="deductionType">Deposit Type filter</param>
        /// <param name="status">Status Type filter</param>
        /// <returns>HTTP action results object containing <see cref="Dtos.PayrollDeductionArrangements"/></returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "person", "contribution", "deductionType", "status" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetPayrollDeductionArrangementsAsync(Paging page,
            [FromUri] string person = "", [FromUri] string contribution = "", [FromUri] string deductionType = "",
            [FromUri] string status = "")
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }

            if (person == null || contribution == null || deductionType == null || status == null)
                // null vs. empty string means they entered a filter with no criteria and we should return an empty set.
                return new PagedHttpActionResult<IEnumerable<Dtos.PayrollDeductionArrangements>>(new List<Dtos.PayrollDeductionArrangements>(), page, 0, this.Request);

            try
            {
                var pageOfItems = await _payrollDeductionArrangementsService.GetPayrollDeductionArrangementsAsync(page.Offset, page.Limit, bypassCache,
                    person, contribution, deductionType, status);

                AddEthosContextProperties(await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _payrollDeductionArrangementsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PayrollDeductionArrangements>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting payroll deduction arrangement.");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns a payroll deduction arrangement.
        /// </summary>
        /// <param name="id">Global Identifier for PayrollDeductionArrangement</param>
        /// <returns>Object of type <see cref="Dtos.PayrollDeductionArrangements"/></returns>
        [EedmResponseFilter]
        public async Task<Dtos.PayrollDeductionArrangements> GetPayrollDeductionArrangementByIdAsync([FromUri] string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                var payrollDeductionArrangement = await _payrollDeductionArrangementsService.GetPayrollDeductionArrangementsByGuidAsync(id);

                if (payrollDeductionArrangement != null)
                {

                    AddEthosContextProperties(await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _payrollDeductionArrangementsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { payrollDeductionArrangement.Id }));
                }

                return payrollDeductionArrangement;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, string.Format("No payroll deduction arrangement was found for guid '{0}'.", id));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting payroll deduction arrangement.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PutPayrollDeductionArrangementAsync
        /// </summary>
        /// <param name="id">Id for the PayrollDeduction Arrangement</param>
        /// <param name="payrollDeductionArrangement">The full request to update payroll deduction arrangement</param>
        /// <returns>Object of type <see cref="Dtos.PayrollDeductionArrangements"/></returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.PayrollDeductionArrangements> PutPayrollDeductionArrangementAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PayrollDeductionArrangements payrollDeductionArrangement)
        {
            try
            {
                //await DoesUpdateViolateDataPrivacySettings(payrollDeductionArrangement, await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetRouteResourceName(), true), _logger);

                //get Data Privacy List
                var dpList = await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _payrollDeductionArrangementsService.ImportExtendedEthosData(await ExtractExtendedData(await _payrollDeductionArrangementsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                Dtos.PayrollDeductionArrangements originalDto = null, mergedDto = null, payrollDeductionArrangementReturn = null;

                try
                {
                     originalDto = await _payrollDeductionArrangementsService.GetPayrollDeductionArrangementsByGuidAsync(id);
                     mergedDto = await PerformPartialPayloadMerge(payrollDeductionArrangement, originalDto, dpList, _logger);

                    if (originalDto.Person != null && mergedDto.Person != null && originalDto.Person.Id != mergedDto.Person.Id)
                    {
                        throw new ArgumentNullException("person.id", "The person id cannot be changed on an update request. ");
                    }
                    
                }
                catch (RepositoryException)
                {
                    // No existing deduction, perform a create instead.
                }

                if (originalDto != null)
                {
                    //do update with partial logic
                    payrollDeductionArrangementReturn = await _payrollDeductionArrangementsService.UpdatePayrollDeductionArrangementsAsync(id, mergedDto);
                }
                else
                {
                    // No existing deduction, perform a create instead.
                    payrollDeductionArrangementReturn = await _payrollDeductionArrangementsService.UpdatePayrollDeductionArrangementsAsync(id, payrollDeductionArrangement);
                }

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _payrollDeductionArrangementsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return payrollDeductionArrangementReturn; 
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, string.Format("No payroll deduction arrangement was found for guid '{0}'.", id));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting payroll deduction arrangement.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PostPayrollDeductionArrangementAsync
        /// </summary>
        /// <param name="payrollDeductionArrangement">The full request to create a new payroll deduction arrangement</param>
        /// <returns>Object of type <see cref="Dtos.PayrollDeductionArrangements"/></returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.PayrollDeductionArrangements> PostPayrollDeductionArrangementAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PayrollDeductionArrangements payrollDeductionArrangement)
        {
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _payrollDeductionArrangementsService.ImportExtendedEthosData(await ExtractExtendedData(await _payrollDeductionArrangementsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the payroll deduction
                var payrollDeduction = await _payrollDeductionArrangementsService.CreatePayrollDeductionArrangementsAsync(payrollDeductionArrangement);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _payrollDeductionArrangementsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { payrollDeduction.Id }));

                return payrollDeduction;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting payroll deduction arrangement.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region Version 11

        /// <summary>
        /// Return all payrollDeductionArrangements
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Filter Criteria for person, contribution, deductionType and status</param>
        /// <returns>List of PayrollDeductionArrangements <see cref="Dtos.PayrollDeductionArrangements"/> objects representing matching payrollDeductionArrangements</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PayrollDeductionArrangements))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetPayrollDeductionArrangements2Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }

            string person = string.Empty, contribution = string.Empty, deductionType = string.Empty, status = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.PayrollDeductionArrangements>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.PayrollDeductionArrangements>>(new List<Dtos.PayrollDeductionArrangements>(), page, this.Request);

            if (criteriaValues != null)
            {
                if (criteriaValues.Person != null && !string.IsNullOrEmpty(criteriaValues.Person.Id))
                    person = criteriaValues.Person.Id;
                if (criteriaValues.PaymentTarget != null && criteriaValues.PaymentTarget.Commitment != null && !string.IsNullOrEmpty(criteriaValues.PaymentTarget.Commitment.Contribution))
                    contribution = criteriaValues.PaymentTarget.Commitment.Contribution;
                if (criteriaValues.PaymentTarget != null && criteriaValues.PaymentTarget.Deduction != null && criteriaValues.PaymentTarget.Deduction.DeductionType != null && !string.IsNullOrEmpty(criteriaValues.PaymentTarget.Deduction.DeductionType.Id))
                    deductionType = criteriaValues.PaymentTarget.Deduction.DeductionType.Id;
                if (criteriaValues.Status != null && criteriaValues.Status != Dtos.EnumProperties.PayrollDeductionArrangementStatuses.NotSet)
                    status = criteriaValues.Status.ToString();
            }

            try
            {
                var pageOfItems = await _payrollDeductionArrangementsService.GetPayrollDeductionArrangementsAsync(page.Offset, page.Limit, bypassCache, person, contribution, deductionType, status);

                AddEthosContextProperties(await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _payrollDeductionArrangementsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PayrollDeductionArrangements>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a payrollDeductionArrangements using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired payrollDeductionArrangements</param>
        /// <returns>A payrollDeductionArrangements object <see cref="Dtos.PayrollDeductionArrangements"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PayrollDeductionArrangements> GetPayrollDeductionArrangements2ByIdAsync(string guid)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                var payrollDeductionArrangement = await _payrollDeductionArrangementsService.GetPayrollDeductionArrangementsByGuidAsync(guid);

                if (payrollDeductionArrangement != null)
                {

                    AddEthosContextProperties(await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _payrollDeductionArrangementsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { payrollDeductionArrangement.Id }));
                }

                return payrollDeductionArrangement;

            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing PayrollDeductionArrangements
        /// </summary>
        /// <param name="guid">GUID of the payrollDeductionArrangements to update</param>
        /// <param name="payrollDeductionArrangements">DTO of the updated payrollDeductionArrangements</param>
        /// <returns>A PayrollDeductionArrangements object <see cref="Dtos.PayrollDeductionArrangements"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.PayrollDeductionArrangements> PutPayrollDeductionArrangement2Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PayrollDeductionArrangements payrollDeductionArrangements)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (payrollDeductionArrangements == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null payrollDeductionArrangements argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(payrollDeductionArrangements.Id))
            {
                payrollDeductionArrangements.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, payrollDeductionArrangements.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);
                //call import extend method that needs the extracted extension dataa and the config
                await _payrollDeductionArrangementsService.ImportExtendedEthosData(await ExtractExtendedData(await _payrollDeductionArrangementsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

            Dtos.PayrollDeductionArrangements originalDto = null, mergedDto = null, payrollDeductionArrangementReturn = null;

                try
                {
                    originalDto = await _payrollDeductionArrangementsService.GetPayrollDeductionArrangementsByGuidAsync(guid);
                    mergedDto = await PerformPartialPayloadMerge(payrollDeductionArrangements, originalDto, dpList, _logger);

                    if (originalDto.Person != null && mergedDto.Person != null && originalDto.Person.Id != mergedDto.Person.Id)
                    {
                        throw new ArgumentNullException("person.id", "The person id cannot be changed on an update request. ");
                    }

                }
                catch (RepositoryException)
                {
                    // No existing deduction, perform a create instead.
                }

                if (originalDto != null)
                {
                    //do update with partial logic
                    payrollDeductionArrangementReturn = await _payrollDeductionArrangementsService.UpdatePayrollDeductionArrangementsAsync(guid, mergedDto);
                }
                else
                {
                    // No existing deduction, perform a create instead.
                    payrollDeductionArrangementReturn = await _payrollDeductionArrangementsService.UpdatePayrollDeductionArrangementsAsync(guid, payrollDeductionArrangements);
                }


                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                await _payrollDeductionArrangementsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

            return payrollDeductionArrangementReturn;
            }
            
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new payrollDeductionArrangements
        /// </summary>
        /// <param name="payrollDeductionArrangements">DTO of the new payrollDeductionArrangements</param>
        /// <returns>A payrollDeductionArrangements object <see cref="Dtos.PayrollDeductionArrangements"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.PayrollDeductionArrangements> PostPayrollDeductionArrangementsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PayrollDeductionArrangements payrollDeductionArrangements)
        {
            if (payrollDeductionArrangements == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid payrollDeductionArrangements.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(payrollDeductionArrangements.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null payrollDeductionArrangements id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _payrollDeductionArrangementsService.ImportExtendedEthosData(await ExtractExtendedData(await _payrollDeductionArrangementsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the payroll deduction
                var payrollDeduction = await _payrollDeductionArrangementsService.CreatePayrollDeductionArrangementsAsync(payrollDeductionArrangements);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _payrollDeductionArrangementsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { payrollDeduction.Id }));

                return payrollDeduction;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        #endregion

        /// <summary>
        /// DeletePayrollDeductionArrangementAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Unsupported Default message of type <see cref="IntegrationApiUtility.DefaultNotSupportedApiErrorMessage"/></returns>
        [HttpDelete]
        public async Task DeletePayrollDeductionArrangementAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}