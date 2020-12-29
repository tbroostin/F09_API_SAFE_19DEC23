//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to FinancialAidFunds
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidFundsController : BaseCompressedApiController
    {
        private readonly IFinancialAidFundsService _financialAidFundsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidFundsController class.
        /// </summary>
        /// <param name="financialAidFundsService">Service of type <see cref="IFinancialAidFundsService">IFinancialAidFundsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FinancialAidFundsController(IFinancialAidFundsService financialAidFundsService, ILogger logger)
        {
            _financialAidFundsService = financialAidFundsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all financialAidFunds
        /// </summary>
        /// <returns>List of FinancialAidFunds <see cref="Dtos.FinancialAidFunds"/> objects representing matching financialAidFunds</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.FinancialAidFundsFilter))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetFinancialAidFundsAsync(Paging page, QueryStringFilter criteria)
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

            var criteriaFilter = GetFilterObject<Dtos.Filters.FinancialAidFundsFilter>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.FinancialAidFunds>>(new List<Dtos.FinancialAidFunds>(), page, 0, this.Request);

            try
            {
                var pageOfItems = await _financialAidFundsService.GetFinancialAidFundsAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _financialAidFundsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _financialAidFundsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          pageOfItems.Item1.Select(dd => dd.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.FinancialAidFunds>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Read (GET) a financialAidFunds using a GUID
        /// </summary>
        /// <param name="id">GUID to desired financialAidFunds</param>
        /// <returns>A financialAidFunds object <see cref="Dtos.FinancialAidFunds"/> in EEDM format</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter]
        public async Task<Dtos.FinancialAidFunds> GetFinancialAidFundsByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument", IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {

                var fund = await _financialAidFundsService.GetFinancialAidFundsByGuidAsync(id);
                if (fund == null)
                {
                    throw CreateHttpResponseException(new IntegrationApiException("GUID.Not.Found", 
                        IntegrationApiUtility.GetDefaultApiError(string.Format("financial-aid-funds not found for GUID '{0}'", id))), HttpStatusCode.NotFound);
                }

                AddEthosContextProperties(await _financialAidFundsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                                          await _financialAidFundsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          new List<string>() { fund.Id }));
                return fund;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Create (POST) a new financialAidFunds
        /// </summary>
        /// <param name="financialAidFunds">DTO of the new financialAidFunds</param>
        /// <returns>A financialAidFunds object <see cref="Dtos.FinancialAidFunds"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidFunds> PostFinancialAidFundsAsync([FromBody] Dtos.FinancialAidFunds financialAidFunds)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing financialAidFunds
        /// </summary>
        /// <param name="id">GUID of the financialAidFunds to update</param>
        /// <param name="financialAidFunds">DTO of the updated financialAidFunds</param>
        /// <returns>A financialAidFunds object <see cref="Dtos.FinancialAidFunds"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidFunds> PutFinancialAidFundsAsync([FromUri] string id, [FromBody] Dtos.FinancialAidFunds financialAidFunds)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a financialAidFunds
        /// </summary>
        /// <param name="id">GUID to desired financialAidFunds</param>
        [HttpDelete]
        public async Task DeleteFinancialAidFundsAsync(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}