// Copyright 2015 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonalRelationshipType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonalRelationshipTypesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonalRelationshipTypeService _personalRelationshipService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonalRelationshipController class.
        /// </summary>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="personalRelationshipService">Service of type <see cref="IPersonalRelationshipTypeService">IPersonalRelationshipService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public PersonalRelationshipTypesController(IReferenceDataRepository referenceDataRepository, IPersonalRelationshipTypeService personalRelationshipService, ILogger logger)
        {
            
            _referenceDataRepository = referenceDataRepository;
            _personalRelationshipService = personalRelationshipService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all relation types.
        /// </summary>
        /// <returns>All <see cref="Dtos.RelationType">RelationType</see>objects.</returns>
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async  Task<IEnumerable<Ellucian.Colleague.Dtos.RelationType>> GetPersonalRelationTypesAsync()
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
                var relationshipTypes = await _personalRelationshipService.GetPersonalRelationTypesAsync(bypassCache);

                if (relationshipTypes != null && relationshipTypes.Any())
                {

                    AddEthosContextProperties(await _personalRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _personalRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              relationshipTypes.Select(a => a.Id).ToList()));
                }
                return relationshipTypes;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an relation type by GUID.
        /// </summary>
        /// <returns>An <see cref="Dtos.RelationType">RelationType </see>object.</returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.RelationType> GetPersonalRelationTypeByGuidAsync(string guid)
        {
            try
            {
                var relationshipType = await _personalRelationshipService.GetPersonalRelationTypeByGuidAsync(guid);
                if (relationshipType != null)
                {

                    AddEthosContextProperties(await _personalRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _personalRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { relationshipType.Id }));
                }
                return relationshipType;
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
        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing Relation type in Colleague (Not Supported)
        /// </summary>
        /// <param name="guid">Unique ID representing the Personal Relation Type to delete</param>
        [HttpDelete]
        public void DeletePersonalRelationType([FromUri] string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update a Relation Type Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="guid">Unique ID representing the Personal Relation Type to update</param>
        /// <param name="relationType"><see cref="Dtos.RelationType">RelationType</see> to update</param>
        [HttpPut]
        public Ellucian.Colleague.Dtos.RelationType PutPersonalRelationType([FromUri] string guid, [FromBody] Ellucian.Colleague.Dtos.EmailType relationType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create a Relation Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="relationType"><see cref="Dtos.RelationType">RelationType</see> to create</param>
        [HttpPost]
        public Ellucian.Colleague.Dtos.RelationType PostPersonalRelationType([FromBody] Ellucian.Colleague.Dtos.RelationType relationType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
