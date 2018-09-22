//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
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

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to ShippingMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ShippingMethodsController : BaseCompressedApiController
    {
        private readonly IShippingMethodsService _shippingMethodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ShippingMethodsController class.
        /// </summary>
        /// <param name="shippingMethodsService">Service of type <see cref="IShippingMethodsService">IShippingMethodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public ShippingMethodsController(IShippingMethodsService shippingMethodsService, ILogger logger)
        {
            _shippingMethodsService = shippingMethodsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all shippingMethods
        /// </summary>
        /// <returns>List of ShippingMethods <see cref="Dtos.ShippingMethods"/> objects representing matching shippingMethods</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ShippingMethods>> GetShippingMethodsAsync()
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
                var pageOfItems = await _shippingMethodsService.GetShippingMethodsAsync(bypassCache);

                AddEthosContextProperties(
                    await _shippingMethodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _shippingMethodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Select(i => i.Id).Distinct().ToList()));

                return pageOfItems;
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
        /// Read (GET) a ShippingMethods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired ShippingMethods</param>
        /// <returns>A ShippingMethods object <see cref="Dtos.ShippingMethods"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.ShippingMethods> GetShippingMethodsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                AddEthosContextProperties(
                  await _shippingMethodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _shippingMethodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));

                return await _shippingMethodsService.GetShippingMethodsByGuidAsync(guid);
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
        /// Create (POST) a new ShippingMethods
        /// </summary>
        /// <param name="shippingMethods">DTO of the new ShippingMethods</param>
        /// <returns>A ShippingMethods object <see cref="Dtos.ShippingMethods"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.ShippingMethods> PostShippingMethodsAsync([FromBody] Dtos.ShippingMethods shippingMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing ShippingMethods
        /// </summary>
        /// <param name="guid">GUID of the ShippingMethods to update</param>
        /// <param name="shippingMethods">DTO of the updated ShippingMethods</param>
        /// <returns>A ShippingMethods object <see cref="Dtos.ShippingMethods"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.ShippingMethods> PutShippingMethodsAsync([FromUri] string guid, [FromBody] Dtos.ShippingMethods shippingMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a ShippingMethods
        /// </summary>
        /// <param name="guid">GUID to desired ShippingMethods</param>
        [HttpDelete]
        public async Task DeleteShippingMethodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}