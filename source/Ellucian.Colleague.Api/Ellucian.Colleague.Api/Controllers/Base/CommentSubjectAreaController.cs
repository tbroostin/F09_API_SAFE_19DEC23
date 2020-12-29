// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using System.Net;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Comment Subject Area data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CommentSubjectAreaController : BaseCompressedApiController
    {
        private readonly ICommentSubjectAreaService _commentSubjectAreaService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CommentSubjectAreaController class.
        /// </summary>
        /// <param name="commentSubjectAreaService">Service of type <see cref="ICommentSubjectAreaService">ICommentSubjectAreaService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CommentSubjectAreaController(ICommentSubjectAreaService commentSubjectAreaService, ILogger logger)
        {
            _commentSubjectAreaService = commentSubjectAreaService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all comment subject areas.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All CommentSubjectArea objects.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommentSubjectArea>> GetCommentSubjectAreaAsync()
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
                var commentSubjectArea = await _commentSubjectAreaService.GetCommentSubjectAreaAsync(bypassCache);

                if (commentSubjectArea != null && commentSubjectArea.Any())
                {
                    AddEthosContextProperties(await _commentSubjectAreaService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _commentSubjectAreaService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              commentSubjectArea.Select(a => a.Id).ToList()));
                }
                return commentSubjectArea;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a comment subject area by ID.
        /// </summary>
        /// <param name="id">Id of Comment Subject Area to retrieve</param>
        /// <returns>A CommentSubjectArea.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CommentSubjectArea> GetCommentSubjectAreaByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _commentSubjectAreaService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _commentSubjectAreaService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _commentSubjectAreaService.GetCommentSubjectAreaByIdAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Creates a CommentSubjectArea.
        /// </summary>
        /// <param name="commentSubjectArea"><see cref="Dtos.CommentSubjectArea2">CommentSubjectArea</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CommentSubjectArea2">CommentSubjectArea</see></returns>
        [HttpPost]
        public  async Task<Dtos.CommentSubjectArea> PostCommentSubjectAreaAsync([FromBody] Dtos.CommentSubjectArea commentSubjectArea)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Updates a Comment Subject Area.
        /// </summary>
        /// <param name="id">Id of the Comment Subject Area to update</param>
        /// <param name="commentSubjectArea"><see cref="Dtos.CommentSubjectArea2">CommentSubjectArea</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CommentSubjectArea2">CommentSubjectArea</see></returns>
        [HttpPut]
        public async Task<Dtos.CommentSubjectArea> PutCommentSubjectAreaAsync([FromUri] string id, [FromBody] Dtos.CommentSubjectArea commentSubjectArea)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Comment Subject Area
        /// </summary>
        /// <param name="id">Id of the Comment Subject Area to delete</param>
        [HttpDelete]
        public async Task DeleteCommentSubjectAreaAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}