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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to RoommateCharacteristics
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class RoommateCharacteristicsController : BaseCompressedApiController
    {
        private readonly IRoommateCharacteristicsService _roommateCharacteristicsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RoommateCharacteristicsController class.
        /// </summary>
        /// <param name="roommateCharacteristicsService">Service of type <see cref="IRoommateCharacteristicsService">IRoommateCharacteristicsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public RoommateCharacteristicsController(IRoommateCharacteristicsService roommateCharacteristicsService, ILogger logger)
        {
            _roommateCharacteristicsService = roommateCharacteristicsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all roommateCharacteristics
        /// </summary>
        /// <returns>List of RoommateCharacteristics <see cref="Dtos.RoommateCharacteristics"/> objects representing matching roommateCharacteristics</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RoommateCharacteristics>> GetRoommateCharacteristicsAsync()
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
                return await _roommateCharacteristicsService.GetRoommateCharacteristicsAsync(bypassCache);
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
        /// Read (GET) a roommateCharacteristics using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired roommateCharacteristics</param>
        /// <returns>A roommateCharacteristics object <see cref="Dtos.RoommateCharacteristics"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.RoommateCharacteristics> GetRoommateCharacteristicsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _roommateCharacteristicsService.GetRoommateCharacteristicsByGuidAsync(guid);
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
        /// Create (POST) a new roommateCharacteristics
        /// </summary>
        /// <param name="roommateCharacteristics">DTO of the new roommateCharacteristics</param>
        /// <returns>A roommateCharacteristics object <see cref="Dtos.RoommateCharacteristics"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.RoommateCharacteristics> PostRoommateCharacteristicsAsync([FromBody] Dtos.RoommateCharacteristics roommateCharacteristics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing roommateCharacteristics
        /// </summary>
        /// <param name="guid">GUID of the roommateCharacteristics to update</param>
        /// <param name="roommateCharacteristics">DTO of the updated roommateCharacteristics</param>
        /// <returns>A roommateCharacteristics object <see cref="Dtos.RoommateCharacteristics"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.RoommateCharacteristics> PutRoommateCharacteristicsAsync([FromUri] string guid, [FromBody] Dtos.RoommateCharacteristics roommateCharacteristics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a roommateCharacteristics
        /// </summary>
        /// <param name="guid">GUID to desired roommateCharacteristics</param>
        [HttpDelete]
        public async Task DeleteRoommateCharacteristicsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}