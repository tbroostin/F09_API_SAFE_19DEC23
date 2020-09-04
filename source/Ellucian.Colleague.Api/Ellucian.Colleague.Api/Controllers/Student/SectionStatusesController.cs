//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to SectionStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionStatusesController : BaseCompressedApiController
    {
        private readonly ISectionStatusesService _sectionStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionStatusesController class.
        /// </summary>
        /// <param name="sectionStatusesService">Service of type <see cref="ISectionStatusesService">ISectionStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public SectionStatusesController(ISectionStatusesService sectionStatusesService, ILogger logger)
        {
            _sectionStatusesService = sectionStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all sectionStatuses
        /// </summary>
        /// <returns>List of SectionStatuses <see cref="Dtos.SectionStatuses"/> objects representing matching sectionStatuses</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.SectionStatuses>> GetSectionStatusesAsync()
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
                var sectionStatuses = await _sectionStatusesService.GetSectionStatusesAsync(bypassCache);

                if (sectionStatuses != null && sectionStatuses.Any())
                {
                    AddEthosContextProperties(await _sectionStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              sectionStatuses.Select(a => a.Id).ToList()));
                }
                return sectionStatuses;
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
        /// Read (GET) a sectionStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired sectionStatuses</param>
        /// <returns>A sectionStatuses object <see cref="Dtos.SectionStatuses"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.SectionStatuses> GetSectionStatusesByGuidAsync(string guid)
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
                     await _sectionStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _sectionStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { guid }));
                return await _sectionStatusesService.GetSectionStatusesByGuidAsync(guid);
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
        /// Create (POST) a new sectionStatuses
        /// </summary>
        /// <param name="sectionStatuses">DTO of the new sectionStatuses</param>
        /// <returns>A sectionStatuses object <see cref="Dtos.SectionStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.SectionStatuses> PostSectionStatusesAsync([FromBody] Dtos.SectionStatuses sectionStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing sectionStatuses
        /// </summary>
        /// <param name="guid">GUID of the sectionStatuses to update</param>
        /// <param name="sectionStatuses">DTO of the updated sectionStatuses</param>
        /// <returns>A sectionStatuses object <see cref="Dtos.SectionStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.SectionStatuses> PutSectionStatusesAsync([FromUri] string guid, [FromBody] Dtos.SectionStatuses sectionStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a sectionStatuses
        /// </summary>
        /// <param name="guid">GUID to desired sectionStatuses</param>
        [HttpDelete]
        public async Task DeleteSectionStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}