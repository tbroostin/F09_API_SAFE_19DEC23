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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to VeteranStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class VeteranStatusesController : BaseCompressedApiController
    {
        private readonly IVeteranStatusesService _veteranStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VeteranStatusesController class.
        /// </summary>
        /// <param name="veteranStatusesService">Service of type <see cref="IVeteranStatusesService">IVeteranStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public VeteranStatusesController(IVeteranStatusesService veteranStatusesService, ILogger logger)
        {
            _veteranStatusesService = veteranStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all veteranStatuses
        /// </summary>
        /// <returns>List of VeteranStatuses <see cref="Dtos.VeteranStatuses"/> objects representing matching veteranStatuses</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.VeteranStatuses>> GetVeteranStatusesAsync()
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
                return await _veteranStatusesService.GetVeteranStatusesAsync(bypassCache);
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
        /// Read (GET) a veteranStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired veteranStatuses</param>
        /// <returns>A veteranStatuses object <see cref="Dtos.VeteranStatuses"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.VeteranStatuses> GetVeteranStatusesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _veteranStatusesService.GetVeteranStatusesByGuidAsync(guid);
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
        /// Create (POST) a new veteranStatuses
        /// </summary>
        /// <param name="veteranStatuses">DTO of the new veteranStatuses</param>
        /// <returns>A veteranStatuses object <see cref="Dtos.VeteranStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.VeteranStatuses> PostVeteranStatusesAsync([FromBody] Dtos.VeteranStatuses veteranStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing veteranStatuses
        /// </summary>
        /// <param name="guid">GUID of the veteranStatuses to update</param>
        /// <param name="veteranStatuses">DTO of the updated veteranStatuses</param>
        /// <returns>A veteranStatuses object <see cref="Dtos.VeteranStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.VeteranStatuses> PutVeteranStatusesAsync([FromUri] string guid, [FromBody] Dtos.VeteranStatuses veteranStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a veteranStatuses
        /// </summary>
        /// <param name="guid">GUID to desired veteranStatuses</param>
        [HttpDelete]
        public async Task DeleteVeteranStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}