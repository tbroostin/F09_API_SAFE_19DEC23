// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Web.Http;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using System.Net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Building data.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BuildingsController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IFacilitiesService _institutionService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// BuildingsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Reference data repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="institutionService">Service of type <see cref="IFacilitiesService">IInstitutionService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public BuildingsController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IFacilitiesService institutionService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _institutionService = institutionService;
            _logger = logger;
        }

        //[CacheControlFilter(MaxAgeHours = 1, Public = true, Revalidate = true)]
        /// <summary>
        /// Retrieves all Buildings.
        /// </summary>
        /// <returns>All <see cref="Building">Building codes and descriptions.</see></returns>
        public async Task<IEnumerable<Building>> GetBuildingsAsync()
        {
            try
            {
                var buildingCollection = await _referenceDataRepository.BuildingsAsync();

                // Get the right adapter for the type mapping
                var buildingDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Building, Building>();

                // Map the building entity to the building DTO
                var buildingDtoCollection = new List<Building>();
                foreach (var bldg in buildingCollection)
                {
                    buildingDtoCollection.Add(buildingDtoAdapter.MapToType(bldg));
                }

                return buildingDtoCollection.OrderBy(s => s.Description);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Your previous session has expired and is no longer valid.";
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString() + ex.StackTrace);
                throw;
            }
        }

        //[CacheControlFilter(MaxAgeHours = 1, Pubilc = true, Revalidate = true)]
        /// <summary>
        /// Retrieves all Building Types.
        /// </summary>
        /// <returns>All <see cref="BuildingType">Building Type codes and descriptions.</see></returns>
        public IEnumerable<BuildingType> GetBuildingTypes()
        {
            var buildingTypes = _referenceDataRepository.BuildingTypes;
            var buildingTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.BuildingType, BuildingType>();
            var buildingTypeDtoCollection = new List<BuildingType>();
            foreach (var bldgType in buildingTypes)
            {
                buildingTypeDtoCollection.Add(buildingTypeDtoAdapter.MapToType(bldgType));
            }
            return buildingTypeDtoCollection.OrderBy(s => s.Code);
        }

        ///// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        ///// <summary>
        ///// Retrieves all buildings.
        ///// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        ///// </summary>
        ///// <returns>All <see cref="Building">Buildings.</see></returns>
        //[HttpGet, EedmResponseFilter]
        //[ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        //[Authorize]
        //public async Task<IEnumerable<Ellucian.Colleague.Dtos.Building3>> GetHedmBuildings3Async([FromUri] string mapVisibility = "")
        //{
        //    bool bypassCache = false;
        //    if (Request.Headers.CacheControl != null)
        //    {
        //        if (Request.Headers.CacheControl.NoCache)
        //        {
        //            bypassCache = true;
        //        }
        //    }
        //    string mapVisibilityFilter = string.Empty;
        //    if (!string.IsNullOrEmpty(mapVisibility))
        //    {
        //        var criteriaValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(mapVisibility);

        //        foreach (var value in criteriaValues)
        //        {
        //            switch (value.Key.ToLower())
        //            {
        //                case "mapvisibility":
        //                    mapVisibilityFilter = string.IsNullOrWhiteSpace(value.Value) ? string.Empty : value.Value;
        //                    break;
        //                default:
        //                    throw new ArgumentException(string.Concat("Invalid filter value received: ", value.Key));
        //            }
        //        }
        //    }
        //    try
        //    {
        //        var items = await _institutionService.GetBuildings3Async(bypassCache, mapVisibilityFilter);

        //        AddEthosContextProperties(
        //            await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
        //            await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
        //                items.Select(i => i.Id).ToList()));

        //        return items;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex.ToString());
        //        throw CreateHttpResponseException(ex.Message);
        //    }
        //}

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all buildings.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Building">Buildings.</see></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("mapVisibility", typeof(Dtos.Filters.MapVisibilityFilter))]
        [Authorize]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Building3>> GetHedmBuildings3Async(QueryStringFilter mapVisibility)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            string mapVisibilityFilter = string.Empty;
            BuildingMapVisibility visibility = BuildingMapVisibility.NotSet;

            try
            {
                var mapVisibilityObj = GetFilterObject<Dtos.Filters.MapVisibilityFilter>(_logger, "mapVisibility");
                if (mapVisibilityObj != null)
                {
                    visibility = mapVisibilityObj.Visibility;
                    mapVisibilityFilter = !visibility.Equals(BuildingMapVisibility.NotSet) ? visibility.ToString() : null;
                }

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.Building3>();

                var items = await _institutionService.GetBuildings3Async(bypassCache, mapVisibilityFilter);

                AddEthosContextProperties(
                    await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }

        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all buildings.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Building">Buildings.</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        [Authorize]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Building2>> GetHedmBuildings2Async()
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
                var items = await _institutionService.GetBuildings2Async(bypassCache);

                AddEthosContextProperties(
                    await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a building by ID.
        /// </summary>
        /// <returns>A <see cref="Dto.Building2">Building.</see></returns>
        [Authorize]
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Building2> GetHedmBuildingByIdAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var building = await _institutionService.GetBuilding2Async(id);

                if (building != null)
                {

                    AddEthosContextProperties(await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { building.Id }));
                }

                return building;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a building by ID.
        /// </summary>
        /// <returns>A <see cref="Dto.Building3">Building.</see></returns>
        [HttpGet, EedmResponseFilter]
        [Authorize]
        public async Task<Ellucian.Colleague.Dtos.Building3> GetHedmBuildingById2Async(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var building = await _institutionService.GetBuilding3Async(id);

                if (building != null)
                {

                    AddEthosContextProperties(await _institutionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _institutionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { building.Id }));
                }

                return building;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>        
        /// Creates a Building.
        /// </summary>
        /// <param name="building"><see cref="Dtos.Building2">Building</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.Building2">Building</see></returns>
        [HttpPost]
        public async Task<Dtos.Building2> PostBuildingAsync([FromBody] Dtos.Building2 building)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a Building.
        /// </summary>
        /// <param name="id">Id of the Building to update</param>
        /// <param name="building"><see cref="Dtos.Building2">Building</see> to create</param>
        /// <returns>Updated <see cref="Dtos.Building2">Building</see></returns>
        [HttpPut]
        public Task<Dtos.Building2> PutBuildingAsync([FromUri] string id, [FromBody] Dtos.Building2 building)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Building
        /// </summary>
        /// <param name="id">Id of the Building to delete</param>
        [HttpDelete]
        public Task<Dtos.Building2> DeleteBuildingAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}