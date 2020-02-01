// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to GenderIdentityType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class GenderIdentityTypesController : BaseCompressedApiController
    {
        private readonly IGenderIdentityTypeService _genderIdentityTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GenderIdentityTypesController class.
        /// </summary>
        /// <param name="genderIdentityTypeService">Service of type <see cref="IGenderIdentityTypeService">IGenderIdentityTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public GenderIdentityTypesController(IGenderIdentityTypeService genderIdentityTypeService, ILogger logger)
        {
            _genderIdentityTypeService = genderIdentityTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves gender identity types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.GenderIdentityType">GenderIdentityType</see> objects></returns>
        [HttpGet]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.GenderIdentityType>> GetAsync()
        {
            try
            {
                bool ignoreCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        ignoreCache = true;
                    }
                }
                return await _genderIdentityTypeService.GetBaseGenderIdentityTypesAsync(ignoreCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Return all genderIdentities
        /// </summary>
        /// <returns>List of GenderIdentities <see cref="Dtos.GenderIdentities"/> objects representing matching genderIdentities</returns>         
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.GenderIdentities>> GetGenderIdentitiesAsync()
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
                var genderIdentities = await _genderIdentityTypeService.GetGenderIdentitiesAsync(bypassCache);

                if (genderIdentities != null && genderIdentities.Any())
                {
                    AddEthosContextProperties(await _genderIdentityTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _genderIdentityTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              genderIdentities.Select(a => a.Id).ToList()));
                }
                return genderIdentities;
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
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Read (GET) a genderIdentities using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired genderIdentities</param>
        /// <returns>A genderIdentities object <see cref="Dtos.GenderIdentities"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.GenderIdentities> GetGenderIdentitiesByGuidAsync(string guid)
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
                   await _genderIdentityTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _genderIdentityTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _genderIdentityTypeService.GetGenderIdentitiesByGuidAsync(guid);
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
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Create (POST) a new genderIdentities
        /// </summary>
        /// <param name="genderIdentities">DTO of the new genderIdentities</param>
        /// <returns>A genderIdentities object <see cref="Dtos.GenderIdentities"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.GenderIdentities> PostGenderIdentitiesAsync([FromBody] Dtos.GenderIdentities genderIdentities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing genderIdentities
        /// </summary>
        /// <param name="guid">GUID of the genderIdentities to update</param>
        /// <param name="genderIdentities">DTO of the updated genderIdentities</param>
        /// <returns>A genderIdentities object <see cref="Dtos.GenderIdentities"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.GenderIdentities> PutGenderIdentitiesAsync([FromUri] string guid, [FromBody] Dtos.GenderIdentities genderIdentities)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a genderIdentities
        /// </summary>
        /// <param name="guid">GUID to desired genderIdentities</param>
        [HttpDelete]
        public async Task DeleteGenderIdentitiesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
