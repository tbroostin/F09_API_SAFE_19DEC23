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
    /// Provides access to FloorCharacteristics
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class FloorCharacteristicsController : BaseCompressedApiController
    {
        private readonly IFloorCharacteristicsService _floorCharacteristicsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FloorCharacteristicsController class.
        /// </summary>
        /// <param name="floorCharacteristicsService">Service of type <see cref="IFloorCharacteristicsService">IFloorCharacteristicsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FloorCharacteristicsController(IFloorCharacteristicsService floorCharacteristicsService, ILogger logger)
        {
            _floorCharacteristicsService = floorCharacteristicsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all floorCharacteristics
        /// </summary>
        /// <returns>List of FloorCharacteristics <see cref="Dtos.FloorCharacteristics"/> objects representing matching floorCharacteristics</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FloorCharacteristics>> GetFloorCharacteristicsAsync()
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
                var floorCharacteristics = await _floorCharacteristicsService.GetFloorCharacteristicsAsync(bypassCache);

                if (floorCharacteristics != null && floorCharacteristics.Any())
                {
                    AddEthosContextProperties(await _floorCharacteristicsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _floorCharacteristicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              floorCharacteristics.Select(a => a.Id).ToList()));
                }
                return floorCharacteristics;
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
        /// Read (GET) a floorCharacteristics using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired floorCharacteristics</param>
        /// <returns>A floorCharacteristics object <see cref="Dtos.FloorCharacteristics"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.FloorCharacteristics> GetFloorCharacteristicsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _floorCharacteristicsService.GetFloorCharacteristicsByGuidAsync(guid);
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
        /// Create (POST) a new floorCharacteristics
        /// </summary>
        /// <param name="floorCharacteristics">DTO of the new floorCharacteristics</param>
        /// <returns>A floorCharacteristics object <see cref="Dtos.FloorCharacteristics"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FloorCharacteristics> PostFloorCharacteristicsAsync([FromBody] Dtos.FloorCharacteristics floorCharacteristics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing floorCharacteristics
        /// </summary>
        /// <param name="guid">GUID of the floorCharacteristics to update</param>
        /// <param name="floorCharacteristics">DTO of the updated floorCharacteristics</param>
        /// <returns>A floorCharacteristics object <see cref="Dtos.FloorCharacteristics"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FloorCharacteristics> PutFloorCharacteristicsAsync([FromUri] string guid, [FromBody] Dtos.FloorCharacteristics floorCharacteristics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a floorCharacteristics
        /// </summary>
        /// <param name="guid">GUID to desired floorCharacteristics</param>
        [HttpDelete]
        public async Task DeleteFloorCharacteristicsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}