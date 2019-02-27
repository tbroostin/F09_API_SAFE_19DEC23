//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
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

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to AlternativeCredentialTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AlternativeCredentialTypesController : BaseCompressedApiController
    {
        private readonly IAlternativeCredentialTypesService _alternativeCredentialTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AlternativeCredentialTypesController class.
        /// </summary>
        /// <param name="alternativeCredentialTypesService">Service of type <see cref="IAlternativeCredentialTypesService">IAlternativeCredentialTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AlternativeCredentialTypesController(IAlternativeCredentialTypesService alternativeCredentialTypesService, ILogger logger)
        {
            _alternativeCredentialTypesService = alternativeCredentialTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all alternativeCredentialTypes
        /// </summary>
        /// <returns>List of AlternativeCredentialTypes <see cref="Dtos.AlternativeCredentialTypes"/> objects representing matching alternativeCredentialTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.AlternativeCredentialTypes>> GetAlternativeCredentialTypesAsync()
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
                var alternativeCredentialTypes = await _alternativeCredentialTypesService.GetAlternativeCredentialTypesAsync(bypassCache);

                if (alternativeCredentialTypes != null && alternativeCredentialTypes.Any())
                {
                    AddEthosContextProperties(await _alternativeCredentialTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _alternativeCredentialTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              alternativeCredentialTypes.Select(a => a.Id).ToList()));
                }
                return alternativeCredentialTypes;
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
        /// Read (GET) a alternativeCredentialTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired alternativeCredentialTypes</param>
        /// <returns>A alternativeCredentialTypes object <see cref="Dtos.AlternativeCredentialTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AlternativeCredentialTypes> GetAlternativeCredentialTypesByGuidAsync(string guid)
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
                   await _alternativeCredentialTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _alternativeCredentialTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _alternativeCredentialTypesService.GetAlternativeCredentialTypesByGuidAsync(guid);
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
        /// Create (POST) a new alternativeCredentialTypes
        /// </summary>
        /// <param name="alternativeCredentialTypes">DTO of the new alternativeCredentialTypes</param>
        /// <returns>A alternativeCredentialTypes object <see cref="Dtos.AlternativeCredentialTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AlternativeCredentialTypes> PostAlternativeCredentialTypesAsync([FromBody] Dtos.AlternativeCredentialTypes alternativeCredentialTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing alternativeCredentialTypes
        /// </summary>
        /// <param name="guid">GUID of the alternativeCredentialTypes to update</param>
        /// <param name="alternativeCredentialTypes">DTO of the updated alternativeCredentialTypes</param>
        /// <returns>A alternativeCredentialTypes object <see cref="Dtos.AlternativeCredentialTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AlternativeCredentialTypes> PutAlternativeCredentialTypesAsync([FromUri] string guid, [FromBody] Dtos.AlternativeCredentialTypes alternativeCredentialTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a alternativeCredentialTypes
        /// </summary>
        /// <param name="guid">GUID to desired alternativeCredentialTypes</param>
        [HttpDelete]
        public async Task DeleteAlternativeCredentialTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}