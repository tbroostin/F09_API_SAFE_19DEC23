//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to AccountingStringSubcomponentValues
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class AccountingStringSubcomponentValuesController : BaseCompressedApiController
    {
        private readonly IAccountingStringSubcomponentValuesService _accountingStringSubcomponentValuesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountingStringSubcomponentValuesController class.
        /// </summary>
        /// <param name="accountingStringSubcomponentValuesService">Service of type <see cref="IAccountingStringSubcomponentValuesService">IAccountingStringSubcomponentValuesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AccountingStringSubcomponentValuesController(IAccountingStringSubcomponentValuesService accountingStringSubcomponentValuesService, ILogger logger)
        {
            _accountingStringSubcomponentValuesService = accountingStringSubcomponentValuesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all accountingStringSubcomponentValues
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of AccountingStringSubcomponentValues <see cref="Dtos.AccountingStringSubcomponentValues"/> objects representing matching accountingStringSubcomponentValues</returns>
        [HttpGet, EedmResponseFilter]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAccountingStringSubcomponentValuesAsync(Paging page)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                var pageOfItems = await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                  await _accountingStringSubcomponentValuesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _accountingStringSubcomponentValuesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringSubcomponentValues>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a accountingStringSubcomponentValues using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringSubcomponentValues</param>
        /// <returns>A accountingStringSubcomponentValues object <see cref="Dtos.AccountingStringSubcomponentValues"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AccountingStringSubcomponentValues> GetAccountingStringSubcomponentValuesByGuidAsync(string guid)
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
                //AddDataPrivacyContextProperty((await _accountingStringSubcomponentValuesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                   await _accountingStringSubcomponentValuesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _accountingStringSubcomponentValuesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
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
        /// Create (POST) a new accountingStringSubcomponentValues
        /// </summary>
        /// <param name="accountingStringSubcomponentValues">DTO of the new accountingStringSubcomponentValues</param>
        /// <returns>A accountingStringSubcomponentValues object <see cref="Dtos.AccountingStringSubcomponentValues"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AccountingStringSubcomponentValues> PostAccountingStringSubcomponentValuesAsync([FromBody] Dtos.AccountingStringSubcomponentValues accountingStringSubcomponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringSubcomponentValues
        /// </summary>
        /// <param name="guid">GUID of the accountingStringSubcomponentValues to update</param>
        /// <param name="accountingStringSubcomponentValues">DTO of the updated accountingStringSubcomponentValues</param>
        /// <returns>A accountingStringSubcomponentValues object <see cref="Dtos.AccountingStringSubcomponentValues"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AccountingStringSubcomponentValues> PutAccountingStringSubcomponentValuesAsync([FromUri] string guid, [FromBody] Dtos.AccountingStringSubcomponentValues accountingStringSubcomponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingStringSubcomponentValues
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringSubcomponentValues</param>
        [HttpDelete]
        public async Task DeleteAccountingStringSubcomponentValuesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}