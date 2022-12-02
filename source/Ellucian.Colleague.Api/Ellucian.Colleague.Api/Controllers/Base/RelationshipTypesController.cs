//Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to RelationshipTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RelationshipTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IRelationshipTypesService _relationshipTypesService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";
        /// <summary>
        /// Initializes a new instance of the RelationshipTypesController class.
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="relationshipTypesService">Service of type <see cref="IRelationshipTypesService">IRelationshipTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public RelationshipTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IRelationshipTypesService relationshipTypesService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _relationshipTypesService = relationshipTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Relationship Types.
        /// </summary>
        /// <returns>All <see cref="RelationshipType">relationship types.</see></returns>
        public async Task<IEnumerable<RelationshipType>> GetAsync()
        {
            try
            {
                var relationshipTypeCollection = await _referenceDataRepository.GetRelationshipTypesAsync();

                // Get the right adapter for the type mapping
                var relationshipTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RelationshipType, RelationshipType>();

                // Map the RelationshipType entity to the program DTO
                var relationshipTypeDtoCollection = new List<RelationshipType>();
                foreach (var relationshipType in relationshipTypeCollection)
                {
                    relationshipTypeDtoCollection.Add(relationshipTypeDtoAdapter.MapToType(relationshipType));
                }

                return relationshipTypeDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Return all relationshipTypes
        /// </summary>
        /// <returns>List of RelationshipTypes <see cref="Dtos.RelationshipTypes"/> objects representing matching relationshipTypes</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]        
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RelationshipTypes>> GetRelationshipTypesAsync()
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
                var relationshipTypes = await _relationshipTypesService.GetRelationshipTypesAsync(bypassCache);

                if (relationshipTypes != null && relationshipTypes.Any())
                {

                    AddEthosContextProperties(
                      await _relationshipTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _relationshipTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          relationshipTypes.Select(i => i.Id).ToList()));
                }
                return relationshipTypes;

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
        /// Read (GET) a relationshipTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired relationshipTypes</param>
        /// <returns>A relationshipTypes object <see cref="Dtos.RelationshipTypes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.RelationshipTypes> GetRelationshipTypesByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _relationshipTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _relationshipTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _relationshipTypesService.GetRelationshipTypesByGuidAsync(guid);
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
        /// Create (POST) a new relationshipTypes
        /// </summary>
        /// <param name="relationshipTypes">DTO of the new relationshipTypes</param>
        /// <returns>A relationshipTypes object <see cref="Dtos.RelationshipTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.RelationshipTypes> PostRelationshipTypesAsync([FromBody] Dtos.RelationshipTypes relationshipTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing relationshipTypes
        /// </summary>
        /// <param name="guid">GUID of the relationshipTypes to update</param>
        /// <param name="relationshipTypes">DTO of the updated relationshipTypes</param>
        /// <returns>A relationshipTypes object <see cref="Dtos.RelationshipTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.RelationshipTypes> PutRelationshipTypesAsync([FromUri] string guid, [FromBody] Dtos.RelationshipTypes relationshipTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a relationshipTypes
        /// </summary>
        /// <param name="guid">GUID to desired relationshipTypes</param>
        [HttpDelete]
        public async Task DeleteRelationshipTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
