// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to CampusOrganizationTypes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.CampusOrgs)]
    public class CampusOrganizationTypesController : BaseCompressedApiController
    {
        private readonly ICampusOrganizationService _campusOrganizationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CampusInvolvementRolesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="campusOrganizationService">Service of type <see cref="ICampusOrganizationService">ICampusOrganizationService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CampusOrganizationTypesController(IAdapterRegistry adapterRegistry, ICampusOrganizationService campusOrganizationService, ILogger logger)
        {
            _campusOrganizationService = campusOrganizationService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all campus organization types.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All campus organization types objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CampusOrganizationType>> GetCampusOrganizationTypesAsync()
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

                var items = await _campusOrganizationService.GetCampusOrganizationTypesAsync(bypassCache);

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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a campus organization type by ID.
        /// </summary>
        /// <param name="id">Id of campus organization type to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CampusOrganizationType">campus organization type.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CampusOrganizationType> GetCampusOrganizationTypeByIdAsync(string id)
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
                return await _campusOrganizationService.GetCampusOrganizationTypeByGuidAsync(id);
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a CampusOrganizationType.
        /// </summary>
        /// <param name="campusOrganizationType"><see cref="Dtos.CampusOrganizationType">CampusOrganizationType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CampusOrganizationType">CampusOrganizationType</see></returns>
        [HttpPost]
        public async Task<Dtos.CampusOrganizationType> PostCampusOrganizationTypeAsync([FromBody] Dtos.CampusOrganizationType campusOrganizationType)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a campus organization type.
        /// </summary>
        /// <param name="id">Id of the CampusOrganizationType to update</param>
        /// <param name="campusOrganizationType"><see cref="Dtos.CampusOrganizationType">CampusOrganizationType</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CampusOrganizationType">CampusOrganizationType</see></returns>
        [HttpPut]
        public async Task<Dtos.CampusOrganizationType> PutCampusOrganizationTypeAsync([FromUri] string id, [FromBody] Dtos.CampusOrganizationType campusOrganizationType)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing campusOrganizationType
        /// </summary>
        /// <param name="id">Id of the campusOrganizationType to delete</param>
        [HttpDelete]
        public async Task DeleteCampusOrganizationTypeAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}