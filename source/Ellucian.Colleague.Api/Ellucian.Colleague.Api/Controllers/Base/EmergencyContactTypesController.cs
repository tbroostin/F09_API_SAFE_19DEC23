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
    /// Provides access to EmergencyContactTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EmergencyContactTypesController : BaseCompressedApiController
    {
        private readonly IEmergencyContactTypesService _emergencyContactTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EmergencyContactTypesController class.
        /// </summary>
        /// <param name="emergencyContactTypesService">Service of type <see cref="IEmergencyContactTypesService">IEmergencyContactTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EmergencyContactTypesController(IEmergencyContactTypesService emergencyContactTypesService, ILogger logger)
        {
            _emergencyContactTypesService = emergencyContactTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all emergencyContactTypes
        /// </summary>
        /// <returns>List of EmergencyContactTypes <see cref="Dtos.EmergencyContactTypes"/> objects representing matching emergencyContactTypes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmergencyContactTypes>> GetEmergencyContactTypesAsync()
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
                var emergencyContactTypes = await _emergencyContactTypesService.GetEmergencyContactTypesAsync(bypassCache);

                if (emergencyContactTypes != null && emergencyContactTypes.Any())
                {
                    AddEthosContextProperties(await _emergencyContactTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _emergencyContactTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              emergencyContactTypes.Select(a => a.Id).ToList()));
                }
                return emergencyContactTypes;
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
        /// Read (GET) a emergencyContactTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired emergencyContactTypes</param>
        /// <returns>A emergencyContactTypes object <see cref="Dtos.EmergencyContactTypes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.EmergencyContactTypes> GetEmergencyContactTypesByGuidAsync(string guid)
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
                   await _emergencyContactTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _emergencyContactTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _emergencyContactTypesService.GetEmergencyContactTypesByGuidAsync(guid);
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
        /// Create (POST) a new emergencyContactTypes
        /// </summary>
        /// <param name="emergencyContactTypes">DTO of the new emergencyContactTypes</param>
        /// <returns>A emergencyContactTypes object <see cref="Dtos.EmergencyContactTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.EmergencyContactTypes> PostEmergencyContactTypesAsync([FromBody] Dtos.EmergencyContactTypes emergencyContactTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing emergencyContactTypes
        /// </summary>
        /// <param name="guid">GUID of the emergencyContactTypes to update</param>
        /// <param name="emergencyContactTypes">DTO of the updated emergencyContactTypes</param>
        /// <returns>A emergencyContactTypes object <see cref="Dtos.EmergencyContactTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.EmergencyContactTypes> PutEmergencyContactTypesAsync([FromUri] string guid, [FromBody] Dtos.EmergencyContactTypes emergencyContactTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a emergencyContactTypes
        /// </summary>
        /// <param name="guid">GUID to desired emergencyContactTypes</param>
        [HttpDelete]
        public async Task DeleteEmergencyContactTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}