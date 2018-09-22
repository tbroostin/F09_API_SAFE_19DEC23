// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Relationship data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RelationshipsController : BaseCompressedApiController
    {
        private IRelationshipService _relationshipService;
        private ILogger _logger;

        /// <summary>
        /// Instantiates a RelationshipController
        /// </summary>
        /// <param name="relationshipService">a relationship service of type <see cref="IRelationshipService"/></param>
        /// <param name="logger">a logging service of type <see cref="ILogger"/></param>
        public RelationshipsController(IRelationshipService relationshipService, ILogger logger)
        {
            _relationshipService = relationshipService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the primary relationships for a person or organization
        /// </summary>
        /// <param name="personId">The identifier of the person of interest</param>
        /// <returns>An enumeration of the person's primary relationship with other persons or organizations.</returns>
        /// <accessComments>
        /// Only the current user can get their own primary relationships.
        /// </accessComments>
        public async Task<IEnumerable<Relationship>> GetPersonPrimaryRelationshipsAsync(string personId)
        {
            try
            {
                return await _relationshipService.GetPersonPrimaryRelationshipsAsync(personId);
            }
            catch (ArgumentNullException anex)
            {
                _logger.Error(anex.ToString());
                throw CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates the given relationship type between the two given entities
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship"/> to be created</param>
        /// <returns>The created <see cref="Relationship"/></returns>
        /// <accessComments>
        /// Only the current user can create their own relationship.
        /// </accessComments>
        public async Task<Relationship> PostRelationshipAsync([FromBody] Relationship relationship)
        {
            try 
            {
                return await _relationshipService.PostRelationshipAsync(relationship.OtherEntity, relationship.RelationshipType, relationship.PrimaryEntity);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}