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
    /// Provides access to RoomRates
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class RoomRatesController : BaseCompressedApiController
    {
        private readonly IRoomRatesService _roomRatesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RoomRatesController class.
        /// </summary>
        /// <param name="roomRatesService">Service of type <see cref="IRoomRatesService">IRoomRatesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public RoomRatesController(IRoomRatesService roomRatesService, ILogger logger)
        {
            _roomRatesService = roomRatesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all roomRates
        /// </summary>
        /// <returns>List of RoomRates <see cref="Dtos.RoomRates"/> objects representing matching roomRates</returns>
        [HttpGet]       
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RoomRates>> GetRoomRatesAsync()
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
                var roomRates = await _roomRatesService.GetRoomRatesAsync(bypassCache);

                if (roomRates != null && roomRates.Any())
                {

                    AddEthosContextProperties(await _roomRatesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _roomRatesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              roomRates.Select(a => a.Id).ToList()));
                }
                return roomRates;
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
        /// Read (GET) a roomRates using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired roomRates</param>
        /// <returns>A roomRates object <see cref="Dtos.RoomRates"/> in EEDM format</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.RoomRates> GetRoomRatesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                var rate = await _roomRatesService.GetRoomRatesByGuidAsync(guid);

                if (rate != null)
                {

                    AddEthosContextProperties(await _roomRatesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _roomRatesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { rate.Id }));
                }


                return rate;
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
        /// Create (POST) a new roomRates
        /// </summary>
        /// <param name="roomRates">DTO of the new roomRates</param>
        /// <returns>A roomRates object <see cref="Dtos.RoomRates"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.RoomRates> PostRoomRatesAsync([FromBody] Dtos.RoomRates roomRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing roomRates
        /// </summary>
        /// <param name="guid">GUID of the roomRates to update</param>
        /// <param name="roomRates">DTO of the updated roomRates</param>
        /// <returns>A roomRates object <see cref="Dtos.RoomRates"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.RoomRates> PutRoomRatesAsync([FromUri] string guid, [FromBody] Dtos.RoomRates roomRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a roomRates
        /// </summary>
        /// <param name="guid">GUID to desired roomRates</param>
        [HttpDelete]
        public async Task DeleteRoomRatesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}