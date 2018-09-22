﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to room characteristic data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RoomCharacteristicsController : BaseCompressedApiController
    {
        private readonly IRoomCharacteristicService _roomCharacteristicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RoomCharacteristicsController class.
        /// </summary>
        /// <param name="roomCharacteristicService">Service of type <see cref="IRoomCharacteristicService">IRoomCharacteristicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public RoomCharacteristicsController(IRoomCharacteristicService roomCharacteristicService, ILogger logger)
        {
            _roomCharacteristicService = roomCharacteristicService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model VERSION 6</remarks>
        /// <summary>
        /// Retrieves all room characteristics.
        /// </summary>
        /// <returns>All RoomCharacteristics objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.RoomCharacteristic>> GetRoomCharacteristicsAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                return await _roomCharacteristicService.GetRoomCharacteristicsAsync(bypassCache);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model VERSION 6</remarks>
        /// <summary>
        /// Retrieves a room characteristic by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.RoomCharacteristic">RoomCharacteristic.</see></returns>
        public async Task<Dtos.RoomCharacteristic> GetRoomCharacteristicByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Must provide a room characteristic id.");
                }
                return await _roomCharacteristicService.GetRoomCharacteristicByGuidAsync(id);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Updates a RoomCharacteristic.
        /// </summary>
        /// <param name="roomCharacteristic"><see cref="RoomCharacteristic">RoomCharacteristic</see> to update</param>
        /// <returns>Newly updated <see cref="RoomCharacteristic">RoomCharacteristic</see></returns>
        [HttpPut]
        public async Task<Dtos.RoomCharacteristic> PutRoomCharacteristicAsync([FromBody] Dtos.RoomCharacteristic roomCharacteristic)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a RoomCharacteristic.
        /// </summary>
        /// <param name="roomCharacteristic"><see cref="RoomCharacteristic">RoomCharacteristic</see> to create</param>
        /// <returns>Newly created <see cref="RoomCharacteristic">RoomCharacteristic</see></returns>
        [HttpPost]
        public async Task<Dtos.RoomCharacteristic> PostRoomCharacteristicAsync([FromBody] Dtos.RoomCharacteristic roomCharacteristic)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing RoomCharacteristic
        /// </summary>
        /// <param name="id">Id of the RoomCharacteristic to delete</param>
        [HttpDelete]
        public async Task DeleteRoomCharacteristicAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
