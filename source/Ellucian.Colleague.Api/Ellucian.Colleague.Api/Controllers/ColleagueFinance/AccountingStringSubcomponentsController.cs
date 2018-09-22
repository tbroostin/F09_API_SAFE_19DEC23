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
    /// Provides access to AccountingStringSubcomponents
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class AccountingStringSubcomponentsController : BaseCompressedApiController
    {
        private readonly IAccountingStringSubcomponentsService _accountingStringSubcomponentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountingStringSubcomponentsController class.
        /// </summary>
        /// <param name="accountingStringSubcomponentsService">Service of type <see cref="IAccountingStringSubcomponentsService">IAccountingStringSubcomponentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AccountingStringSubcomponentsController(IAccountingStringSubcomponentsService accountingStringSubcomponentsService, ILogger logger)
        {
            _accountingStringSubcomponentsService = accountingStringSubcomponentsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all accountingStringSubcomponents
        /// </summary>
        /// <returns>List of AccountingStringSubcomponents <see cref="Dtos.AccountingStringSubcomponents"/> objects representing matching accountingStringSubcomponents</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringSubcomponents>> GetAccountingStringSubcomponentsAsync()
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
                var accountingStringSubcomponents = await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsAsync(bypassCache);

                if (accountingStringSubcomponents != null && accountingStringSubcomponents.Any())
                {
                    AddEthosContextProperties(await _accountingStringSubcomponentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _accountingStringSubcomponentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              accountingStringSubcomponents.Select(a => a.Id).ToList()));
                }
                return accountingStringSubcomponents;
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
        /// Read (GET) a accountingStringSubcomponents using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringSubcomponents</param>
        /// <returns>A accountingStringSubcomponents object <see cref="Dtos.AccountingStringSubcomponents"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AccountingStringSubcomponents> GetAccountingStringSubcomponentsByGuidAsync(string guid)
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
                //AddDataPrivacyContextProperty((await _accountingStringSubcomponentsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                   await _accountingStringSubcomponentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _accountingStringSubcomponentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsByGuidAsync(guid);
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
        /// Create (POST) a new accountingStringSubcomponents
        /// </summary>
        /// <param name="accountingStringSubcomponents">DTO of the new accountingStringSubcomponents</param>
        /// <returns>A accountingStringSubcomponents object <see cref="Dtos.AccountingStringSubcomponents"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AccountingStringSubcomponents> PostAccountingStringSubcomponentsAsync([FromBody] Dtos.AccountingStringSubcomponents accountingStringSubcomponents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringSubcomponents
        /// </summary>
        /// <param name="guid">GUID of the accountingStringSubcomponents to update</param>
        /// <param name="accountingStringSubcomponents">DTO of the updated accountingStringSubcomponents</param>
        /// <returns>A accountingStringSubcomponents object <see cref="Dtos.AccountingStringSubcomponents"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AccountingStringSubcomponents> PutAccountingStringSubcomponentsAsync([FromUri] string guid, [FromBody] Dtos.AccountingStringSubcomponents accountingStringSubcomponents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingStringSubcomponents
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringSubcomponents</param>
        [HttpDelete]
        public async Task DeleteAccountingStringSubcomponentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}