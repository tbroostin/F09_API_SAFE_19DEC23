// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Citizenship Statuses data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CitizenshipStatusesController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RacesController class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public CitizenshipStatusesController(IDemographicService demographicService, ILogger logger)
        {
            _demographicService = demographicService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves all citizenship statuses.
        /// </summary>
        /// <returns>All CitizenshipStatuses objects.</returns>
        /// 
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CitizenshipStatus>> GetCitizenshipStatusesAsync()
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

                var citizenshipStatuses = await _demographicService.GetCitizenshipStatusesAsync(bypassCache);

                if (citizenshipStatuses != null && citizenshipStatuses.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              citizenshipStatuses.Select(a => a.Id).ToList()));
                }
                return citizenshipStatuses;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves a citizenship status by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CitizenshipStatus">CitizenshipStatus.</see></returns>
        /// 
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CitizenshipStatus> GetCitizenshipStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _demographicService.GetCitizenshipStatusByGuidAsync(id);
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

        /// <summary>
        /// Updates a CitizenshipStatus.
        /// </summary>
        /// <param name="citizenshipStatus"><see cref="CitizenshipStatus">CitizenshipStatus</see> to update</param>
        /// <returns>Newly updated <see cref="CitizenshipStatus">CitizenshipStatus</see></returns>
        [HttpPut]
        public async Task<Dtos.CitizenshipStatus> PutCitizenshipStatusAsync([FromBody] Dtos.CitizenshipStatus citizenshipStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a CitizenshipStatus.
        /// </summary>
        /// <param name="citizenshipStatus"><see cref="CitizenshipStatus">CitizenshipStatus</see> to create</param>
        /// <returns>Newly created <see cref="CitizenshipStatus">CitizenshipStatus</see></returns>
        [HttpPost]
        public async Task<Dtos.CitizenshipStatus> PostCitizenshipStatusAsync([FromBody] Dtos.CitizenshipStatus citizenshipStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing CitizenshipStatus
        /// </summary>
        /// <param name="id">Id of the CitizenshipStatus to delete</param>
        [HttpDelete]
        public async Task DeleteCitizenshipStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
