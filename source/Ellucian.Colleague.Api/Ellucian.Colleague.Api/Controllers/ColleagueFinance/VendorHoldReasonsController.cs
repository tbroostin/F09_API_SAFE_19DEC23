//Copyright 2016 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to VendorHoldReasons
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorHoldReasonsController : BaseCompressedApiController
    {
        private readonly IVendorHoldReasonsService _vendorHoldReasonsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorHoldReasonsController class.
        /// </summary>
        /// <param name="vendorHoldReasonsService">Service of type <see cref="IVendorHoldReasonsService">IVendorHoldReasonsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public VendorHoldReasonsController(IVendorHoldReasonsService vendorHoldReasonsService, ILogger logger)
        {
            _vendorHoldReasonsService = vendorHoldReasonsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all vendorHoldReasons
        /// </summary>
        /// <returns>List of VendorHoldReasons <see cref="VendorHoldReasons"/> objects representing matching vendorHoldReasons</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<VendorHoldReasons>> GetVendorHoldReasonsAsync()
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
                return await _vendorHoldReasonsService.GetVendorHoldReasonsAsync(bypassCache);
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
        /// Read (GET) a vendorHoldReasons using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendorHoldReasons</param>
        /// <returns>A vendorHoldReasons object <see cref="VendorHoldReasons"/> in EEDM format</returns>
        [HttpGet]
        public async Task<VendorHoldReasons> GetVendorHoldReasonsByIdAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _vendorHoldReasonsService.GetVendorHoldReasonsByGuidAsync(guid);
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
        /// Create (POST) a new vendorHoldReasons
        /// </summary>
        /// <param name="vendorHoldReasons">DTO of the new vendorHoldReasons</param>
        /// <returns>A vendorHoldReasons object <see cref="VendorHoldReasons"/> in EEDM format</returns>
        [HttpPost]
        public async Task<VendorHoldReasons> PostVendorHoldReasonsAsync([FromBody] VendorHoldReasons vendorHoldReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing vendorHoldReasons
        /// </summary>
        /// <param name="guid">GUID of the vendorHoldReasons to update</param>
        /// <param name="vendorHoldReasons">DTO of the updated vendorHoldReasons</param>
        /// <returns>A vendorHoldReasons object <see cref="Dtos.VendorHoldReasons"/> in EEDM format</returns>
        [HttpPut]
        public async Task<VendorHoldReasons> PutVendorHoldReasonsAsync([FromUri] string guid, [FromBody] VendorHoldReasons vendorHoldReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a vendorHoldReasons
        /// </summary>
        /// <param name="guid">GUID to desired vendorHoldReasons</param>
        [HttpDelete]
        public async Task DeleteVendorHoldReasonsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}