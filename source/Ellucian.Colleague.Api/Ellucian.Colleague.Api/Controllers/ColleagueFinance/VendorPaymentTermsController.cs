//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to VendorPaymentTerms
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorPaymentTermsController : BaseCompressedApiController
    {
        private readonly IVendorPaymentTermsService _vendorPaymentTermsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorPaymentTermsController class.
        /// </summary>
        /// <param name="vendorPaymentTermsService">Service of type <see cref="IVendorPaymentTermsService">IVendorPaymentTermsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public VendorPaymentTermsController(IVendorPaymentTermsService vendorPaymentTermsService, ILogger logger)
        {
            _vendorPaymentTermsService = vendorPaymentTermsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all vendorPaymentTerms
        /// </summary>
        /// <returns>List of VendorPaymentTerms <see cref="Dtos.VendorPaymentTerms"/> objects representing matching vendorPaymentTerms</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.VendorPaymentTerms>> GetVendorPaymentTermsAsync()
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
                var items = await _vendorPaymentTermsService.GetVendorPaymentTermsAsync(bypassCache);

                AddEthosContextProperties(
                  await _vendorPaymentTermsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _vendorPaymentTermsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
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
        /// Read (GET) a vendorPaymentTerms using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendorPaymentTerms</param>
        /// <returns>A vendorPaymentTerms object <see cref="Dtos.VendorPaymentTerms"/> in EEDM format</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.VendorPaymentTerms> GetVendorPaymentTermsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _vendorPaymentTermsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), true),
                    await _vendorPaymentTermsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _vendorPaymentTermsService.GetVendorPaymentTermsByGuidAsync(guid);
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
        /// Create (POST) a new vendorPaymentTerms
        /// </summary>
        /// <param name="vendorPaymentTerms">DTO of the new vendorPaymentTerms</param>
        /// <returns>A vendorPaymentTerms object <see cref="Dtos.VendorPaymentTerms"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.VendorPaymentTerms> PostVendorPaymentTermsAsync([FromBody] Dtos.VendorPaymentTerms vendorPaymentTerms)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing vendorPaymentTerms
        /// </summary>
        /// <param name="guid">GUID of the vendorPaymentTerms to update</param>
        /// <param name="vendorPaymentTerms">DTO of the updated vendorPaymentTerms</param>
        /// <returns>A vendorPaymentTerms object <see cref="Dtos.VendorPaymentTerms"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.VendorPaymentTerms> PutVendorPaymentTermsAsync([FromUri] string guid, [FromBody] Dtos.VendorPaymentTerms vendorPaymentTerms)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a vendorPaymentTerms
        /// </summary>
        /// <param name="guid">GUID to desired vendorPaymentTerms</param>
        [HttpDelete]
        public async Task DeleteVendorPaymentTermsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}