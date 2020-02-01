//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to EmergencyContactPhoneAvailabilities
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EmergencyContactPhoneAvailabilitiesController : BaseCompressedApiController
    {
        private readonly IEmergencyContactPhoneAvailabilitiesService _emergencyContactPhoneAvailabilitiesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmergencyContactPhoneAvailabilitiesController class.
        /// </summary>
        /// <param name="emergencyContactPhoneAvailabilitiesService">Service of type <see cref="IEmergencyContactPhoneAvailabilitiesService">IEmergencyContactPhoneAvailabilitiesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmergencyContactPhoneAvailabilitiesController(IEmergencyContactPhoneAvailabilitiesService emergencyContactPhoneAvailabilitiesService, ILogger logger)
        {
            _emergencyContactPhoneAvailabilitiesService = emergencyContactPhoneAvailabilitiesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all emergencyContactPhoneAvailabilities
        /// </summary>
        /// <returns>List of EmergencyContactPhoneAvailabilities <see cref="Dtos.EmergencyContactPhoneAvailabilities"/> objects representing matching emergencyContactPhoneAvailabilities</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities>> GetEmergencyContactPhoneAvailabilitiesAsync()
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
                var emergencyContactPhoneAvailabilities = await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesAsync(bypassCache);

                if (emergencyContactPhoneAvailabilities != null && emergencyContactPhoneAvailabilities.Any())
                {
                    AddEthosContextProperties(await _emergencyContactPhoneAvailabilitiesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _emergencyContactPhoneAvailabilitiesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              emergencyContactPhoneAvailabilities.Select(a => a.Id).ToList()));
                }
                return emergencyContactPhoneAvailabilities;
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
        /// Read (GET) a emergencyContactPhoneAvailabilities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired emergencyContactPhoneAvailabilities</param>
        /// <returns>A emergencyContactPhoneAvailabilities object <see cref="Dtos.EmergencyContactPhoneAvailabilities"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.EmergencyContactPhoneAvailabilities> GetEmergencyContactPhoneAvailabilitiesByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _emergencyContactPhoneAvailabilitiesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _emergencyContactPhoneAvailabilitiesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(guid);
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
        /// Create (POST) a new emergencyContactPhoneAvailabilities
        /// </summary>
        /// <param name="emergencyContactPhoneAvailabilities">DTO of the new emergencyContactPhoneAvailabilities</param>
        /// <returns>A emergencyContactPhoneAvailabilities object <see cref="Dtos.EmergencyContactPhoneAvailabilities"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.EmergencyContactPhoneAvailabilities> PostEmergencyContactPhoneAvailabilitiesAsync([FromBody] Dtos.EmergencyContactPhoneAvailabilities emergencyContactPhoneAvailabilities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing emergencyContactPhoneAvailabilities
        /// </summary>
        /// <param name="guid">GUID of the emergencyContactPhoneAvailabilities to update</param>
        /// <param name="emergencyContactPhoneAvailabilities">DTO of the updated emergencyContactPhoneAvailabilities</param>
        /// <returns>A emergencyContactPhoneAvailabilities object <see cref="Dtos.EmergencyContactPhoneAvailabilities"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.EmergencyContactPhoneAvailabilities> PutEmergencyContactPhoneAvailabilitiesAsync([FromUri] string guid, [FromBody] Dtos.EmergencyContactPhoneAvailabilities emergencyContactPhoneAvailabilities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a emergencyContactPhoneAvailabilities
        /// </summary>
        /// <param name="guid">GUID to desired emergencyContactPhoneAvailabilities</param>
        [HttpDelete]
        public async Task DeleteEmergencyContactPhoneAvailabilitiesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}