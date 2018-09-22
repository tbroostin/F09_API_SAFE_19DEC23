//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to BillingOverrideReasons
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class BillingOverrideReasonsController : BaseCompressedApiController
    {
        private readonly IBillingOverrideReasonsService _billingOverrideReasonsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BillingOverrideReasonsController class.
        /// </summary>
        /// <param name="billingOverrideReasonsService">Service of type <see cref="IBillingOverrideReasonsService">IBillingOverrideReasonsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public BillingOverrideReasonsController(IBillingOverrideReasonsService billingOverrideReasonsService, ILogger logger)
        {
            _billingOverrideReasonsService = billingOverrideReasonsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all billingOverrideReasons
        /// </summary>
        /// <returns>List of BillingOverrideReasons <see cref="Dtos.BillingOverrideReasons"/> objects representing matching billingOverrideReasons</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BillingOverrideReasons>> GetBillingOverrideReasonsAsync()
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
                var billingOverrideReasons =  await _billingOverrideReasonsService.GetBillingOverrideReasonsAsync(bypassCache);

                AddEthosContextProperties(
                    await _billingOverrideReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _billingOverrideReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        billingOverrideReasons.Select(i => i.Id).ToList()));

                return billingOverrideReasons;
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
        /// Read (GET) a billingOverrideReasons using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired billingOverrideReasons</param>
        /// <returns>A billingOverrideReasons object <see cref="Dtos.BillingOverrideReasons"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.BillingOverrideReasons> GetBillingOverrideReasonsByGuidAsync(string guid)
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
                var billingOverRideReason = await _billingOverrideReasonsService.GetBillingOverrideReasonsByGuidAsync(guid);

                AddEthosContextProperties(
                    await _billingOverrideReasonsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _billingOverrideReasonsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return billingOverRideReason;
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
        /// Create (POST) a new billingOverrideReasons
        /// </summary>
        /// <param name="billingOverrideReasons">DTO of the new billingOverrideReasons</param>
        /// <returns>A billingOverrideReasons object <see cref="Dtos.BillingOverrideReasons"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.BillingOverrideReasons> PostBillingOverrideReasonsAsync([FromBody] Dtos.BillingOverrideReasons billingOverrideReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing billingOverrideReasons
        /// </summary>
        /// <param name="guid">GUID of the billingOverrideReasons to update</param>
        /// <param name="billingOverrideReasons">DTO of the updated billingOverrideReasons</param>
        /// <returns>A billingOverrideReasons object <see cref="Dtos.BillingOverrideReasons"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.BillingOverrideReasons> PutBillingOverrideReasonsAsync([FromUri] string guid, [FromBody] Dtos.BillingOverrideReasons billingOverrideReasons)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a billingOverrideReasons
        /// </summary>
        /// <param name="guid">GUID to desired billingOverrideReasons</param>
        [HttpDelete]
        public async Task DeleteBillingOverrideReasonsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}