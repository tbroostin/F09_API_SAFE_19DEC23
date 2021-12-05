//Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.HumanResources;

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
        [HttpGet, FilteringFilter(IgnoreFiltering = true), CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PermissionsFilter(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements)]
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
                _payrollDeductionArrangementsService.ValidatePermissions(GetPermissionsMetaData());
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
                _logger.Error(e, "Unexpected error getting payroll deduction arrangement.");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns a payroll deduction arrangement.
        /// </summary>
        /// <param name="id">Global Identifier for PayrollDeductionArrangement</param>
        /// <returns>Object of type <see cref="Dtos.PayrollDeductionArrangements"/></returns>
        [HttpGet, EedmResponseFilter, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)] 
        [PermissionsFilter(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements)]

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
                _payrollDeductionArrangementsService.ValidatePermissions(GetPermissionsMetaData());
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, string.Format("No payroll-deduction-arrangements was found for GUID '{0}'.", id));
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
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected error getting payroll deduction arrangement.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PutPayrollDeductionArrangementAsync
        /// </summary>
        /// <param name="id">Id for the PayrollDeduction Arrangement</param>
        /// <param name="payrollDeductionArrangement">The full request to update payroll deduction arrangement</param>
        /// <returns>Object of type <see cref="Dtos.PayrollDeductionArrangements"/></returns>
        [HttpPut, EedmResponseFilter, PermissionsFilter(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements)]
        public async Task<Dtos.PayrollDeductionArrangements> PutPayrollDeductionArrangementAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PayrollDeductionArrangements payrollDeductionArrangement)
        {
            try
            {
                _payrollDeductionArrangementsService.ValidatePermissions(GetPermissionsMetaData());
                //await DoesUpdateViolateDataPrivacySettings(payrollDeductionArrangement, await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetRouteResourceName(), true), _logger);

                //get Data Privacy List
                var dpList = await _payrollDeductionArrangementsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _payrollDeductionArrangementsService.ImportExtendedEthosData(await ExtractExtendedData(await _payrollDeductionArrangementsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                Dtos.PayrollDeductionArrangements originalDto = null, mergedDto = null, payrollDeductionArrangementReturn = null;

                try
                {
                    _payrollDeductionArrangementsService.ValidatePermissions(GetPermissionsMetaData());
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, string.Format("No payroll deduction arrangement was found for guid '{0}'.", id));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
                _logger.Error(e, "Unexpected error getting payroll deduction arrangement.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PostPayrollDeductionArrangementAsync
        /// </summary>
        /// <param name="payrollDeductionArrangement">The full request to create a new payroll deduction arrangement</param>
        /// <returns>Object of type <see cref="Dtos.PayrollDeductionArrangements"/></returns>
        [HttpPost, EedmResponseFilter, PermissionsFilter(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements)]
        public async Task<Dtos.PayrollDeductionArrangements> PostPayrollDeductionArrangementAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PayrollDeductionArrangements payrollDeductionArrangement)
        {
            try
            {
                _payrollDeductionArrangementsService.ValidatePermissions(GetPermissionsMetaData());
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
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
                _logger.Error(e, "Unexpected error getting payroll deduction arrangement.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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