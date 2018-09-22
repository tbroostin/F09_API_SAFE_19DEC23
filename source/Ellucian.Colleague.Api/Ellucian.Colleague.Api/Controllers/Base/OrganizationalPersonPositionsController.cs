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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to organizational person positions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class OrganizationalPersonPositionsController : BaseCompressedApiController
    {
        private readonly IOrganizationalPersonPositionService _organizationalPersonPositionService;
        private readonly ILogger _logger;

        /// <summary>
        /// OrganizationalPersonPositionsController constructor
        /// </summary>
        /// <param name="organizationalPersonPositionService">Service of type <see cref="IOrganizationalPersonPositionService">IOrganizationalPersonPositionService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public OrganizationalPersonPositionsController(IOrganizationalPersonPositionService organizationalPersonPositionService, ILogger logger)
        {
            _organizationalPersonPositionService = organizationalPersonPositionService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the position for the given ID and the direct relationships to others for the position.
        /// </summary>
        /// <returns>OrganizationalPersonPosition for the given ID</returns>
        [HttpGet]
        public async Task<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition> GetOrganizationalPersonPositionAsync(string id)
        {
            try
            {
                return await _organizationalPersonPositionService.GetOrganizationalPersonPositionByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the positions for the indicated persons and the direct relationships to others for each position.
        /// </summary>
        /// <returns>A list of OrganizationalPersonPosition objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>> QueryOrganizationalPersonPositionAsync(OrganizationalPersonPositionQueryCriteria criteria)
        {
            try
            {
                return await _organizationalPersonPositionService.QueryOrganizationalPersonPositionAsync(criteria);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }
    }
}