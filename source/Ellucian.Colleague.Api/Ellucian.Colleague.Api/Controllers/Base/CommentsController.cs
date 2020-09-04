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
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Comments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CommentsController : BaseCompressedApiController
    {
        private readonly ICommentsService _commentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CommentsController class.
        /// </summary>
        /// <param name="commentsService">Service of type <see cref="ICommentsService">ICommentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CommentsController(ICommentsService commentsService, ILogger logger)
        {
            _commentsService = commentsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all remarks found in the REMARKS file in Colleague using filter
        /// </summary>
        /// <param name="subjectMatter">find all of the records in REMARKS where the REMARKS.DONOR.ID matches the person or organization ID corresponding to the guid found in subjectMatter.person.id or subjectMatter.organization.id.</param>
        /// <param name="commentSubjectArea">find all of the records in REMARKS where the REMARKS.TYPE matches the code corresponding to the guid in commentSubjectArea.id.</param>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of Comments <see cref="Dtos.Comments"/> objects representing matching comments</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "subjectMatter", "commentSubjectArea" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetCommentsAsync(Paging page, [FromUri] string subjectMatter = "", [FromUri] string commentSubjectArea = "")
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (subjectMatter == null || commentSubjectArea == null)
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Comments>>(new List<Dtos.Comments>(), page, 0, this.Request);
            }

            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _commentsService.GetCommentsAsync(page.Offset, page.Limit, subjectMatter, commentSubjectArea, bypassCache);

                AddEthosContextProperties(await _commentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _commentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Comments>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a comment using a GUID
        /// </summary>
        /// <param name="id">GUID to desired comment</param>
        /// <returns>A comment object <see cref="Dtos.Comments"/> in HEDM format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter]
        public async Task<Dtos.Comments> GetCommentsByGuidAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                var comment = await _commentsService.GetCommentByIdAsync(id);

                if (comment != null)
                {

                    AddEthosContextProperties(await _commentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _commentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { comment.Id }));
                }


                return comment;

            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new comment
        /// </summary>
        /// <param name="comment">DTO of the new comment</param>
        /// <returns>A comment object <see cref="Dtos.Comments"/> in HEDM format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter]
        public async Task<Dtos.Comments> PostCommentsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.Comments comment)
        {
            if (comment == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null comment argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            } 
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _commentsService.ImportExtendedEthosData(await ExtractExtendedData(await _commentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the comment
                var commentCreate = await _commentsService.PostCommentAsync(comment);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _commentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _commentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { commentCreate.Id }));

                return commentCreate;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing comment
        /// </summary>
        /// <param name="id">GUID of the comment to update</param>
        /// <param name="comment">DTO of the updated comment</param>
        /// <returns>A comment object <see cref="Dtos.Comments"/> in HEDM format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2),  EedmResponseFilter]
        public async Task<Dtos.Comments> PutCommentsAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Comments comment)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (comment == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null comment argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(comment.Id))
            {
                comment.Id = id.ToLowerInvariant();
            }
            else if ( (string.Equals(id, Guid.Empty.ToString())) || (string.Equals(comment.Id, Guid.Empty.ToString())) )
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (id.ToLowerInvariant() != comment.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _commentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _commentsService.ImportExtendedEthosData(await ExtractExtendedData(await _commentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var commentReturn = await _commentsService.PutCommentAsync(id,
                    await PerformPartialPayloadMerge(comment, async () => await _commentsService.GetCommentByIdAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _commentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return commentReturn; 
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a comment
        /// </summary>
        /// <param name="id">GUID to desired comment</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task DeleteCommentByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                await _commentsService.DeleteCommentByIdAsync(id);
                // On delete, just return nothing.
               
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
    }
}