﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Privacy Statuses data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PrivacyStatusesController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PrivacyStatusesController class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public PrivacyStatusesController(IDemographicService demographicService, ILogger logger)
        {
            _demographicService = demographicService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves all privacy statuses.
        /// </summary>
        /// <returns>All PrivacyStatus objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PrivacyStatus>> GetPrivacyStatusesAsync()
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
                return await _demographicService.GetPrivacyStatusesAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves a privacy statuses by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.PrivacyStatuses">PrivacyStatus.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.PrivacyStatus> GetPrivacyStatusByIdAsync(string id)
        {
            try
            {
                return await _demographicService.GetPrivacyStatusByGuidAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a PrivacyStatus.
        /// </summary>
        /// <param name="privacyStatus"><see cref="PrivacyStatus">PrivacyStatus</see> to update</param>
        /// <returns>Newly updated <see cref="PrivacyStatus">PrivacyStatus</see></returns>
        [HttpPut]
        public async Task<Dtos.PrivacyStatus> PutPrivacyStatusAsync([FromBody] Dtos.PrivacyStatus privacyStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a PrivacyStatus.
        /// </summary>
        /// <param name="privacyStatus"><see cref="PrivacyStatus">PrivacyStatus</see> to create</param>
        /// <returns>Newly created <see cref="PrivacyStatus">PrivacyStatus</see></returns>
        [HttpPost]
        public async Task<Dtos.PrivacyStatus> PostPrivacyStatusAsync([FromBody] Dtos.PrivacyStatus privacyStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PrivacyStatus
        /// </summary>
        /// <param name="id">Id of the PrivacyStatus to delete</param>
        [HttpDelete]
        public async Task DeletePrivacyStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
