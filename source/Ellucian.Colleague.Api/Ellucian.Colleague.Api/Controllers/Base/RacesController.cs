// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Race data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RacesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the RacesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public RacesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IDemographicService demographicService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _demographicService = demographicService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all of the races.
        /// </summary>
        /// <returns>All <see cref="Race">Races</see></returns>
        public async Task<IEnumerable<Race>> GetAsync()
        {
            bool bypassCache = false;
            if ((Request != null) && (Request.Headers.CacheControl != null))
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                var raceCollection = await _referenceDataRepository.GetRacesAsync(bypassCache);

                // Get the right adapter for the type mapping
                var raceDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Race, Race>();

                // Map the race entity to the program DTO
                var raceDtoCollection = new List<Race>();
                foreach (var race in raceCollection)
                {
                    raceDtoCollection.Add(raceDtoAdapter.MapToType(race));
                }

                return raceDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Session has expired while retrieving race type data";
                _logger.Error(csse, message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves all races.
        /// </summary>
        /// <returns>All Race objects.</returns>
        /// 
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Race2>> GetRaces2Async()
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

                var races = await _demographicService.GetRaces2Async(bypassCache);

                if (races != null && races.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              races.Select(a => a.Id).ToList()));
                }
                return races;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Retrieves a race by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.Race2">Race.</see></returns>
        /// 
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Race2> GetRaceById2Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _demographicService.GetRaceById2Async(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a Race.
        /// </summary>
        /// <param name="race"><see cref="Race2">Race</see> to update</param>
        /// <returns>Newly updated <see cref="Race2">Race</see></returns>
        [HttpPut]
        public Dtos.Race2 PutRaces([FromBody] Dtos.Race2 race)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a Race.
        /// </summary>
        /// <param name="race"><see cref="Race2">Race</see> to create</param>
        /// <returns>Newly created <see cref="Race2">Race</see></returns>
        [HttpPost]
        public Dtos.Race2 PostRaces([FromBody] Dtos.Race2 race)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Race
        /// </summary>
        /// <param name="id">Id of the Race to delete</param>
        [HttpDelete]
        public void DeleteRaces(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
