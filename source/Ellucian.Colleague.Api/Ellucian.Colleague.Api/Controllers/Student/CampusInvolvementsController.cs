// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to CampusInvolvement data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.CampusOrgs)]
    public class CampusInvolvementsController : BaseCompressedApiController
    {
        private readonly ICampusOrganizationService _campusOrganizationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CampusInvolvementController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="campusOrganizationService">Service of type <see cref="ICampusOrganizationService">ICampusOrganizationService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CampusInvolvementsController(IAdapterRegistry adapterRegistry, ICampusOrganizationService campusOrganizationService, ILogger logger)
        {
            _campusOrganizationService = campusOrganizationService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Retrieves all campus involvement.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, 
        /// otherwise cached data is returned.
        /// </summary>
        /// <returns>All campus involvement  objects.</returns>
        [PagingFilter( IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetCampusInvolvementsAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _campusOrganizationService.GetCampusInvolvementsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _campusOrganizationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _campusOrganizationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.CampusInvolvement>>(pageOfItems.Item1, page, pageOfItems.Item2, Request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Retrieves a campus involvement by ID.
        /// </summary>
        /// <param name="id">Id of campus involvement  to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CampusInvolvement">campus involvement.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CampusInvolvement> GetCampusInvolvementByIdAsync(string id)
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
                AddEthosContextProperties(
                    await _campusOrganizationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _campusOrganizationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return await _campusOrganizationService.GetCampusInvolvementByGuidAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Creates a CampusInvolvement.
        /// </summary>
        /// <param name="campusInvolvement"><see cref="Dtos.CampusInvolvement">CampusInvolvement</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CampusInvolvement">CampusInvolvement</see></returns>
        [HttpPost]
        public async Task<Dtos.CampusInvolvement> PostCampusInvolvementAsync([FromBody] Dtos.CampusInvolvement campusInvolvement)
        {
            //Create is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Updates a accounting code.
        /// </summary>
        /// <param name="id">Id of the CampusInvolvement to update</param>
        /// <param name="campusInvolvement"><see cref="Dtos.CampusInvolvement">CampusInvolvement</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CampusInvolvement">CampusInvolvement</see></returns>
        [HttpPut]
        public async Task<Dtos.CampusInvolvement> PutCampusInvolvementAsync([FromUri] string id, [FromBody] Dtos.CampusInvolvement campusInvolvement)
        {
            //Update is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Delete (DELETE) an existing campusInvolvement
        /// </summary>
        /// <param name="id">Id of the campusInvolvement to delete</param>
        [HttpDelete]
        public async Task DeleteCampusInvolvementAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}