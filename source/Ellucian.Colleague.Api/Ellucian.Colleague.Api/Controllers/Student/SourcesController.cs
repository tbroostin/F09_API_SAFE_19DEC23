// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Source data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SourcesController : BaseCompressedApiController
    {
        private readonly ISourceService _sourceService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SourcesController class.
        /// </summary>
        /// <param name="sourceService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to logger</param>
        public SourcesController(ISourceService sourceService, ILogger logger)
        {
            _sourceService = sourceService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves all sources.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All Source objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Source>> GetSourcesAsync()
        {
            bool bypassCache = false; 
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                return await _sourceService.GetSourcesAsync(bypassCache);
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
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves a source by ID.
        /// </summary>
        /// <param name="id">Id of Source to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.Source">Source.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.Source> GetSourceByIdAsync(string id)
        {
            try
            {
                return await _sourceService.GetSourceByIdAsync(id);
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
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Creates a Source.
        /// </summary>
        /// <param name="source"><see cref="Dtos.Source">Source</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.Source">Source</see></returns>
        [HttpPost]
        public async Task<Dtos.Source> PostSourcesAsync([FromBody] Dtos.Source source)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Updates a Source.
        /// </summary>
        /// <param name="id">Id of the Source to update</param>
        /// <param name="source"><see cref="Dtos.Source">Source</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Source">Source</see></returns>
        [HttpPut]
        public async Task<Dtos.Source> PutSourcesAsync([FromUri] string id, [FromBody] Dtos.Source source)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Source
        /// </summary>
        /// <param name="id">Id of the Source to delete</param>
        [HttpDelete]
        public async Task DeleteSourcesAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}