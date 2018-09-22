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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to SectionTitleTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionTitleTypesController : BaseCompressedApiController
    {
        private readonly ISectionTitleTypesService _sectionTitleTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionTitleTypesController class.
        /// </summary>
        /// <param name="sectionTitleTypesService">Service of type <see cref="ISectionTitleTypesService">ISectionTitleTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public SectionTitleTypesController(ISectionTitleTypesService sectionTitleTypesService, ILogger logger)
        {
            _sectionTitleTypesService = sectionTitleTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all sectionTitleTypes
        /// </summary>
        /// <returns>List of SectionTitleTypes <see cref="Dtos.SectionTitleType"/> objects representing matching sectionTitleTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.SectionTitleType>> GetSectionTitleTypesAsync()
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
                var sectionTitleTypes = await _sectionTitleTypesService.GetSectionTitleTypesAsync(bypassCache);

                if (sectionTitleTypes != null && sectionTitleTypes.Any())
                {
                    AddEthosContextProperties(await _sectionTitleTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _sectionTitleTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              sectionTitleTypes.Select(a => a.Id).ToList()));
                }
                return sectionTitleTypes;
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
        /// Read (GET) a sectionTitleTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired sectionTitleTypes</param>
        /// <returns>A sectionTitleTypes object <see cref="Dtos.SectionTitleType"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.SectionTitleType> GetSectionTitleTypeByGuidAsync(string guid)
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
                   await _sectionTitleTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionTitleTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _sectionTitleTypesService.GetSectionTitleTypeByGuidAsync(guid);
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
        /// Create (POST) a new sectionTitleTypes
        /// </summary>
        /// <param name="sectionTitleTypes">DTO of the new sectionTitleTypes</param>
        /// <returns>A sectionTitleTypes object <see cref="Dtos.SectionTitleType"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.SectionTitleType> PostSectionTitleTypeAsync([FromBody] Dtos.SectionTitleType sectionTitleTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing sectionTitleTypes
        /// </summary>
        /// <param name="guid">GUID of the sectionTitleTypes to update</param>
        /// <param name="sectionTitleTypes">DTO of the updated sectionTitleTypes</param>
        /// <returns>A sectionTitleTypes object <see cref="Dtos.SectionTitleType"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.SectionTitleType> PutSectionTitleTypeAsync([FromUri] string guid, [FromBody] Dtos.SectionTitleType sectionTitleTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a sectionTitleTypes
        /// </summary>
        /// <param name="guid">GUID to desired sectionTitleTypes</param>
        [HttpDelete]
        public async Task DeleteSectionTitleTypeAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}