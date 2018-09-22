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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to RelationshipStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RelationshipStatusesController : BaseCompressedApiController
    {
        private readonly IRelationshipStatusesService _relationshipStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RelationshipStatusesController class.
        /// </summary>
        /// <param name="relationshipStatusesService">Service of type <see cref="IRelationshipStatusesService">IRelationshipStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public RelationshipStatusesController(IRelationshipStatusesService relationshipStatusesService, ILogger logger)
        {
            _relationshipStatusesService = relationshipStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all relationshipStatuses
        /// </summary>
        /// <returns>List of RelationshipStatuses <see cref="Dtos.RelationshipStatuses"/> objects representing matching relationshipStatuses</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RelationshipStatuses>> GetRelationshipStatusesAsync()
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
                AddDataPrivacyContextProperty((await _relationshipStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _relationshipStatusesService.GetRelationshipStatusesAsync(bypassCache);
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
        /// Read (GET) a relationshipStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired relationshipStatuses</param>
        /// <returns>A relationshipStatuses object <see cref="Dtos.RelationshipStatuses"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.RelationshipStatuses> GetRelationshipStatusesByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddDataPrivacyContextProperty((await _relationshipStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _relationshipStatusesService.GetRelationshipStatusesByGuidAsync(guid);
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
        /// Create (POST) a new relationshipStatuses
        /// </summary>
        /// <param name="relationshipStatuses">DTO of the new relationshipStatuses</param>
        /// <returns>A relationshipStatuses object <see cref="Dtos.RelationshipStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.RelationshipStatuses> PostRelationshipStatusesAsync([FromBody] Dtos.RelationshipStatuses relationshipStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing relationshipStatuses
        /// </summary>
        /// <param name="guid">GUID of the relationshipStatuses to update</param>
        /// <param name="relationshipStatuses">DTO of the updated relationshipStatuses</param>
        /// <returns>A relationshipStatuses object <see cref="Dtos.RelationshipStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.RelationshipStatuses> PutRelationshipStatusesAsync([FromUri] string guid, [FromBody] Dtos.RelationshipStatuses relationshipStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a relationshipStatuses
        /// </summary>
        /// <param name="guid">GUID to desired relationshipStatuses</param>
        [HttpDelete]
        public async Task DeleteRelationshipStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}