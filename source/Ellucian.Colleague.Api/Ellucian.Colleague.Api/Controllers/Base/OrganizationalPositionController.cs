// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to organizational positions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]

    public class OrganizationalPositionController : BaseCompressedApiController
    {
        private IOrganizationalPositionService _organizationalPositionService;
        private ILogger _logger;
        /// <summary>
        /// Organizational Position Controller constructor
        /// </summary>
        public OrganizationalPositionController(IOrganizationalPositionService organizationalPositionService, ILogger logger)
        {
            _organizationalPositionService = organizationalPositionService;
            _logger = logger;
        }

        /// <summary>
        /// For the given id, get the Organizational Position
        /// </summary>
        /// <param name="id">Organizational Position id</param>
        /// <returns>Organizational Position DTO</returns>
        [HttpGet]
        public async Task<Dtos.Base.OrganizationalPosition> GetOrganizationalPositionAsync(string id)
        {
            try
            {
                var organizationalPosition = await _organizationalPositionService.GetOrganizationalPositionByIdAsync(id);
                return organizationalPosition;
            }
            catch(Exception e)
            {
                _logger.Error(e, "Unable to get Organizational Position: " + id);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) {
                    Content = new StringContent("Unable to get Organizational Position: " + id)
               });
            }
            
        }
        /// <summary>
        /// For a given list of IDs or search string, returns organizational positions
        /// </summary>
        /// <param name="criteria">Organizational position query criteria</param>
        /// <returns>Matching organizational positions</returns>
        public async Task<IEnumerable<Dtos.Base.OrganizationalPosition>> QueryOrganizationalPositionsAsync(OrganizationalPositionQueryCriteria criteria)
        {
            try
            {
                var organizationalPositions = await _organizationalPositionService.QueryOrganizationalPositionsAsync(criteria);
                return organizationalPositions;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to get Organizational Positions");
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Unable to get Organizational Position")
                });
            }
        }
    }
}
