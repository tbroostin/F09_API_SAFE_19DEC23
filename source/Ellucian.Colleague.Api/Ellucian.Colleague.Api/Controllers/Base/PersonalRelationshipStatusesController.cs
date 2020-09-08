// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Personal Relationship Statuses data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonalRelationshipStatusesController : BaseCompressedApiController
    {
        private readonly IPersonalRelationshipTypeService _personalRelationshipService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonalRelationshipStatusesController class.
        /// </summary>
        /// <param name="personalRelationshipService">Service of type <see cref="IPersonalRelationshipTypeService">IPersonalRelationshipService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public PersonalRelationshipStatusesController(IPersonalRelationshipTypeService personalRelationshipService, ILogger logger)
        {
            _personalRelationshipService = personalRelationshipService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves all personal relationship statuses.
        /// </summary>
        /// <returns>All PersonalRelationshipStatuses objects.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationshipStatus>> GetPersonalRelationshipStatusesAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var personalRelationshipStatuses = await _personalRelationshipService.GetPersonalRelationshipStatusesAsync(bypassCache);

                if (personalRelationshipStatuses != null && personalRelationshipStatuses.Any())
                {
                    AddEthosContextProperties(await _personalRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personalRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personalRelationshipStatuses.Select(a => a.Id).ToList()));
                }

                return personalRelationshipStatuses;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves a personal relationship status by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.PersonalRelationshipStatus">PersonalRelationshipStatus.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.PersonalRelationshipStatus> GetPersonalRelationshipStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                  await _personalRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                  await _personalRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));
                return await _personalRelationshipService.GetPersonalRelationshipStatusByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a PersonalRelationshipStatus.
        /// </summary>
        /// <param name="personalRelationshipStatus"><see cref="PersonalRelationshipStatus">PersonalRelationshipStatus</see> to update</param>
        /// <returns>Newly updated PersonalRelationshipStatus</returns>
        [HttpPut]
        public async Task<Dtos.PersonalRelationshipStatus> PutPersonalRelationshipStatusAsync([FromBody] Dtos.PersonalRelationshipStatus personalRelationshipStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a PersonalRelationshipStatus.
        /// </summary>
        /// <param name="personalRelationshipStatus"><see cref="PersonalRelationshipStatus">PersonalRelationshipStatus</see> to create</param>
        /// <returns>Newly created <see cref="PersonalRelationshipStatus">PersonalRelationshipStatus</see></returns>
        [HttpPost]
        public async Task<Dtos.PersonalRelationshipStatus> PostPersonalRelationshipStatusAsync([FromBody] Dtos.PersonalRelationshipStatus personalRelationshipStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonalRelationshipStatus
        /// </summary>
        /// <param name="id">Id of the PersonalRelationshipStatus to delete</param>
        [HttpDelete]
        public async Task DeletePersonalRelationshipStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
