// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to organizational relationships
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class OrganizationalRelationshipsController : BaseCompressedApiController
    {
        private readonly IOrganizationalPersonPositionService _organizationalPersonPositionService;
        private readonly IOrganizationalRelationshipService _organizationalRelationshipService;
        private readonly ILogger _logger;

        /// <summary>
        /// OrganizationalRelationshipsController constructor
        /// </summary>
        /// <param name="organizationalPersonPositionService">Service of type <see cref="IOrganizationalPersonPositionService">IOrganizationalPersonPositionService</see></param>
        /// <param name="organizationalRelationshipService">Service of type <see cref="IOrganizationalRelationshipService">IOrganizationalRelationshipService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public OrganizationalRelationshipsController(IOrganizationalPersonPositionService organizationalPersonPositionService, IOrganizationalRelationshipService organizationalRelationshipService, ILogger logger)
        {
            _organizationalPersonPositionService = organizationalPersonPositionService;
            _organizationalRelationshipService = organizationalRelationshipService;
            _logger = logger;
        }

        /// <summary>
        /// Create organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">The organizational relationship</param>
        /// <returns>The new organizational relationship</returns>
        /// <accessComments>
        /// Users with the following permission codes can create organizational relationships:
        /// 
        /// UPDATE.ORGANIZATIONAL.RELATIONSHIPS
        /// </accessComments>
        [HttpPost]
        public async Task<Ellucian.Colleague.Dtos.Base.OrganizationalRelationship> CreateOrganizationalRelationshipAsync([FromBody] Ellucian.Colleague.Dtos.Base.OrganizationalRelationship organizationalRelationship)
        {
            try
            {
                return await _organizationalRelationshipService.AddAsync(organizationalRelationship);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Update organizational relationship
        /// </summary>
        /// <param name="organizationalRelationship">The organizational relationship</param>
        /// <returns>The updated organizational relationship</returns>
        /// <accessComments>
        /// Users with the following permission codes can update organizational relationships:
        /// 
        /// UPDATE.ORGANIZATIONAL.RELATIONSHIPS
        /// </accessComments>
        [HttpPost]
        public async Task<Ellucian.Colleague.Dtos.Base.OrganizationalRelationship> UpdateOrganizationalRelationshipAsync([FromBody] Ellucian.Colleague.Dtos.Base.OrganizationalRelationship organizationalRelationship)
        {
            try
            {
                return await _organizationalRelationshipService.UpdateAsync(organizationalRelationship);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Delete an organizational relationship
        /// </summary>
        /// <param name="id">Organizational relationship ID to delete</param>
        /// <accessComments>
        /// Users with the following permission codes can delete organizational relationships:
        /// 
        /// UPDATE.ORGANIZATIONAL.RELATIONSHIPS
        /// </accessComments>
        [HttpDelete]
        public async Task DeleteOrganizationalRelationshipAsync(string id)
        {
            try
            {
                await _organizationalRelationshipService.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }
    }
}
