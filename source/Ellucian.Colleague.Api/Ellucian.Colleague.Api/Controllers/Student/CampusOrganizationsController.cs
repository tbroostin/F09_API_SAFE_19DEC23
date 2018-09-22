// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to CampusOrganization data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.CampusOrgs)]
    public class CampusOrganizationsController : BaseCompressedApiController
    {
        private readonly ICampusOrganizationService _campusOrganizationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CampusOrganizationController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="campusOrganizationService">Service of type <see cref="ICampusOrganizationService">ICampusOrganizationService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CampusOrganizationsController(IAdapterRegistry adapterRegistry, ICampusOrganizationService campusOrganizationService, ILogger logger)
        {
            _campusOrganizationService = campusOrganizationService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves all campus organizations.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All campus organizations objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CampusOrganization>> GetCampusOrganizationsAsync()
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
                var items = await _campusOrganizationService.GetCampusOrganizationsAsync(bypassCache);

                AddEthosContextProperties(
                    await _campusOrganizationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _campusOrganizationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Retrieves a campus organization by ID.
        /// </summary>
        /// <param name="id">Id of campus organization to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CampusOrganization">campus organization.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CampusOrganization> GetCampusOrganizationByIdAsync(string id)
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
                return await _campusOrganizationService.GetCampusOrganizationByGuidAsync(id);
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

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Creates a CampusOrganization.
        /// </summary>
        /// <param name="campusOrganization"><see cref="Dtos.CampusOrganization">CampusOrganization</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CampusOrganizationType">CampusOrganization</see></returns>
        [HttpPost]
        public async Task<Dtos.CampusOrganization> PostCampusOrganizationAsync([FromBody] Dtos.CampusOrganization campusOrganization)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Updates a campus organization.
        /// </summary>
        /// <param name="id">Id of the CampusOrganization to update</param>
        /// <param name="campusOrganization"><see cref="Dtos.CampusOrganization">CampusOrganization</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CampusOrganization">CampusOrganization</see></returns>
        [HttpPut]
        public async Task<Dtos.CampusOrganization> PutCampusOrganizationAsync([FromUri] string id, [FromBody] Dtos.CampusOrganization campusOrganization)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Delete (DELETE) an existing campusOrganization
        /// </summary>
        /// <param name="id">Id of the campusOrganization to delete</param>
        [HttpDelete]
        public async Task DeleteCampusOrganizationAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}