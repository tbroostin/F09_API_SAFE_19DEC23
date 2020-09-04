// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Base.Services;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Marital Status data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class MaritalStatusesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MaritalStatusesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public MaritalStatusesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IDemographicService demographicService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _demographicService = demographicService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Marital Statuses.
        /// </summary>
        /// <returns>All <see cref="MaritalStatus">Marital Status codes and descriptions.</see></returns>
        public async Task<IEnumerable<MaritalStatus>> GetAsync()
        {
            var maritalStatusCollection = await _referenceDataRepository.MaritalStatusesAsync();

            // Get the right adapter for the type mapping
            var maritalStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.MaritalStatus, MaritalStatus>();

            // Map the maritalStatus entity to the program DTO
            var maritalStatusDtoCollection = new List<MaritalStatus>();
            foreach (var maritalStatus in maritalStatusCollection)
            {
                maritalStatusDtoCollection.Add(maritalStatusDtoAdapter.MapToType(maritalStatus));
            }

            return maritalStatusDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all marital statuses.
        /// </summary>
        /// <returns>All <see cref="MaritalStatus">MaritalStatuses.</see></returns>
        [Obsolete("Obsolete as of HeDM Version 4, use Accept Header Version 4 instead.")]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus>> GetMaritalStatusesAsync()
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
                return await _demographicService.GetMaritalStatusesAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a marital status by GUID.
        /// </summary>
        /// <returns>A <see cref="MaritalStatus">MaritalStatus.</see></returns>
        [Obsolete("Obsolete as of HeDM Version 4, use Accept Header Version 4 instead.")]
        public async Task<Ellucian.Colleague.Dtos.MaritalStatus> GetMaritalStatusByGuidAsync(string guid)
        {
            try
            {
                return await _demographicService.GetMaritalStatusByGuidAsync(guid);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEEDM Version 4</remarks>
        /// <summary>
        /// Retrieves all marital statuses. If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All MaritalStatuses.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus2>> GetMaritalStatuses2Async()
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
                var maritalStatuses = await _demographicService.GetMaritalStatuses2Async(bypassCache);

                if (maritalStatuses != null && maritalStatuses.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              maritalStatuses.Select(a => a.Id).ToList()));
                }
                return maritalStatuses;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 4</remarks>
        /// <summary>
        /// Retrieves a marital status by ID.
        /// </summary>
        /// <returns>A MaritalStatus.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.MaritalStatus2> GetMaritalStatusById2Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _demographicService.GetMaritalStatusById2Async(id);
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
        /// Creates a Marital Status.
        /// </summary>
        /// <param name="maritalStatus"><see cref="MaritalStatus2">MaritalStatus</see> to create</param>
        /// <returns>Newly created <see cref="MaritalStatus2">MaritalStatus</see></returns>
        [HttpPost]
        public async Task<Dtos.MaritalStatus2> PostMaritalStatusesAsync([FromBody] Dtos.MaritalStatus2 maritalStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Marital Status.
        /// </summary>
        /// <param name="id">Id of the Marital Status to update</param>
        /// <param name="maritalStatus"><see cref="MaritalStatus2">MaritalStatus</see> to create</param>
        /// <returns>Updated <see cref="MaritalStatus2">MaritalStatus</see></returns>
        [HttpPut]
        public async Task<Dtos.MaritalStatus2> PutMaritalStatusesAsync([FromUri] string id, [FromBody] Dtos.MaritalStatus2 maritalStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Marital Status
        /// </summary>
        /// <param name="id">Id of the Marital Status to delete</param>
        [HttpDelete]
        public async Task<Dtos.MaritalStatus2> DeleteMaritalStatusesAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
