// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using System.Net;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Instructional Platform data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class InstructionalPlatformsController : BaseCompressedApiController
    {
        private readonly IInstructionalPlatformService _instructionalPlatformService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructionalPlatformsController class.
        /// </summary>
        /// <param name="instructionalPlatformService">Service of type <see cref="IInstructionalPlatformService">IInstructionalPlatformService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public InstructionalPlatformsController(IInstructionalPlatformService instructionalPlatformService, ILogger logger)
        {
            _instructionalPlatformService = instructionalPlatformService;
            _logger = logger;
        }

        #region Get Methods

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Instructional Platforms.
        /// </summary>
        /// <returns>All <see cref="InstructionalPlatform"> </see>InstructionalPlatform.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<InstructionalPlatform>> GetInstructionalPlatformsAsync()
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
                return await _instructionalPlatformService.GetInstructionalPlatformsAsync(bypassCache);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, "Permissions exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, "Argument exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, "Integration API exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception occurred");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Instructional Platform by GUID.
        /// </summary>
        /// <param name="id">Id of the Instructional Platform to retrieve</param>
        /// <returns>An <see cref="InstructionalPlatform">InstructionalPlatform </see>object.</returns>
        public async Task<InstructionalPlatform> GetInstructionalPlatformsByIdAsync(string id)
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
                return await _instructionalPlatformService.GetInstructionalPlatformByGuidAsync(id, bypassCache);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, "Permissions exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, "Argument exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, "Integration API exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception occurred");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        #endregion

        #region Post Methods

        /// <summary>
        /// Creates a instructionalPlatform.
        /// </summary>
        /// <param name="instructionalPlatform"><see cref="InstructionalPlatform">InstructionalPlatform</see> to create</param>
        /// <returns>Newly created <see cref="InstructionalPlatform">InstructionalPlatform</see></returns>
        [HttpPost]
        public async Task<InstructionalPlatform> PostInstructionalPlatformsAsync([FromBody] InstructionalPlatform instructionalPlatform)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods

        /// <summary>
        /// Creates a instructionalPlatform.
        /// </summary>
        /// <param name="id">Id of InstructionalPlatform to create</param>
        /// <param name="instructionalPlatform"><see cref="InstructionalPlatform">InstructionalPlatform</see> to create</param>
        /// <returns>Newly created <see cref="InstructionalPlatform">InstructionalPlatform</see></returns>
        [HttpPut]
        public async Task<InstructionalPlatform> PutInstructionalPlatformsAsync([FromUri] string id, [FromBody] InstructionalPlatform instructionalPlatform)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Delete (DELETE) an existing Intructional Platform
        /// </summary>
        /// <param name="id">Id of the Instructional Platform to delete</param>
        [HttpDelete]
        public async Task DeleteInstructionalPlatformsAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

    }
}
