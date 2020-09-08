// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Provides access to Ethnicity data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EthnicitiesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EthnicitiesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public EthnicitiesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IDemographicService demographicService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _demographicService = demographicService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all of the ethnicities.
        /// </summary>
        /// <returns>All <see cref="Ethnicity">Ethnicities</see></returns>
        public async Task<IEnumerable<Ethnicity>> GetAsync()
        {
            var ethnicityCollection = await _referenceDataRepository.EthnicitiesAsync();

            // Get the right adapter for the type mapping
            var ethnicityDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Ethnicity, Ethnicity>();

            // Map the ethnicity entity to the program DTO
            var ethnicityDtoCollection = new List<Ethnicity>();
            foreach (var ethnicity in ethnicityCollection)
            {
                ethnicityDtoCollection.Add(ethnicityDtoAdapter.MapToType(ethnicity));
            }

            return ethnicityDtoCollection;
        }

        /// <remarks>For use with Ellucian EEDM Version 4</remarks>
        /// <summary>
        /// Retrieves all ethnicities. If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All Ethnicity objects.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Ethnicity2>> GetEthnicities2Async()
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
                var ethnicities = await _demographicService.GetEthnicities2Async(bypassCache);

                if (ethnicities != null && ethnicities.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              ethnicities.Select(a => a.Id).ToList()));
                }
                return ethnicities;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>For use with Ellucian EEDM Version 4</remarks>
        /// <summary>
        /// Retrieves an ethnicity by ID.
        /// </summary>
        /// <returns>An Ethnicity</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Ethnicity2> GetEthnicityById2Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _demographicService.GetEthnicityById2Async(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a Ethnicity.
        /// </summary>
        /// <param name="ethnicity"><see cref="Ethnicity2">Ethnicity</see> to update</param>
        /// <returns>Newly updated <see cref="Ethnicity2">Ethnicity</see></returns>
        [HttpPut]
        public async Task<Dtos.Ethnicity2> PutEthnicitiesAsync([FromBody] Dtos.Ethnicity2 ethnicity)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a Ethnicity.
        /// </summary>
        /// <param name="ethnicity">Ethnicity to create</param>
        /// <returns>Newly created Ethnicity</returns>
        [HttpPost]
        public async Task<Dtos.Ethnicity2> PostEthnicitiesAsync([FromBody] Dtos.Ethnicity2 ethnicity)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Ethnicity
        /// </summary>
        /// <param name="id">Id of the Ethnicity to delete</param>
        [HttpDelete]
        public async Task DeleteEthnicitiesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
