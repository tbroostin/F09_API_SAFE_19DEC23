// Copyright 2021 Ellucian Company L.P. and its affiliates..

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class RegionIsoCodesService : BaseCoordinationService, IRegionIsoCodesService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;

        public RegionIsoCodesService(IReferenceDataRepository referenceDataRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Gets all region ISO codes
        /// </summary>
        /// <param name="criteriaFilter">criteria filter</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of Region ISO Codes<see cref="RegionIsoCodes">regionIsoCodes</see> objects</returns>
        public async Task<IEnumerable<RegionIsoCodes>> GetRegionIsoCodesAsync(Dtos.RegionIsoCodes criteriaFilter, bool bypassCache = false)
        {
            string countryGuid = string.Empty;
            if (criteriaFilter != null)
            {
                countryGuid = (criteriaFilter.Country != null && !string.IsNullOrEmpty(criteriaFilter.Country.Id)) ? criteriaFilter.Country.Id : string.Empty;
            }

            var regionIsoCodesCollection = new List<Ellucian.Colleague.Dtos.RegionIsoCodes>();

            var placesEntities = await _referenceDataRepository.GetPlacesAsync(bypassCache);

            if (placesEntities != null && placesEntities.Any())
            {
                // Get Places with a region and no subregion.
                var subList = placesEntities.Where(x => !(string.IsNullOrEmpty(x.PlacesRegion)) &&  (string.IsNullOrEmpty(x.PlacesSubRegion)));

                if (subList != null && subList.Any())
                {
                    foreach (var regionIsoCode in subList)
                    {
                        try
                        {
                            // validate against country filter if needed
                            bool validCountry = true;
                            if (!string.IsNullOrEmpty(countryGuid))
                            {
                                validCountry = false;
                                if (!string.IsNullOrEmpty(regionIsoCode.PlacesCountry))
                                {
                                    // Get the matching Place record for the country of the region.  This country Place record will have no region or sub-region.
                                    var countries = placesEntities.Where(x => x.PlacesCountry == regionIsoCode.PlacesCountry && (string.IsNullOrEmpty(x.PlacesRegion)) && (string.IsNullOrEmpty(x.PlacesSubRegion)));
                                    if (countries != null && countries.Any())
                                    {
                                        foreach (var country in countries)
                                        {
                                            // check if the places record for this country has a GUID matching incoming country GUID filter value
                                            if (country.Guid == countryGuid)
                                            {
                                                validCountry = true;
                                            }                                            
                                        }
                                    }     
                                }
                            }
                            if (validCountry == true)
                            {
                                regionIsoCodesCollection.Add(ConvertPlaceEntityToRegionIsoCodesDto(regionIsoCode, placesEntities));
                            }
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError("Error occurred extracting region-iso-code: " + ex.Message, guid: regionIsoCode.Guid);
                        }
                    }
                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }
                }
            }
            return regionIsoCodesCollection;
        }
        /// <summary>
        /// Get a region ISO code by guid.
        /// </summary>
        /// <param name="guid">Guid of the region ISO code in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="RegionIsoCode">regionIsoCode</see></returns>
        public async Task<RegionIsoCodes> GetRegionIsoCodesByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                var regionIsoCode = (await _referenceDataRepository.GetRegionPlaceByGuidAsync(guid));

                if (regionIsoCode == null)
                {
                    throw new KeyNotFoundException();
                }
                IEnumerable<Place> placesEntities = new List<Place>();
                // If the Place has a country, get all the PLACES.  We'll need it in the Convert method to find
                // the Place for that country.
                if (!(string.IsNullOrEmpty(regionIsoCode.PlacesCountry)))
                {
                    placesEntities = await _referenceDataRepository.GetPlacesAsync(bypassCache);                   
                }
                return ConvertPlaceEntityToRegionIsoCodesDto(regionIsoCode, placesEntities);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No region-iso-codes was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No region-iso-codes was found for guid '{0}'", guid), ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Places domain entity to its corresponding RegionIsoCodes DTO
        /// </summary>
        /// <param name="source">single Places domain entity</param>
        /// <param name="places">all Places domain entity</param>
        /// <returns>RegionIsoCodes DTO</returns>
        private RegionIsoCodes ConvertPlaceEntityToRegionIsoCodesDto(Place source, IEnumerable<Place> places)
        {
            if (source == null)
                return null;

            var regionIsoCodes = new Ellucian.Colleague.Dtos.RegionIsoCodes();

            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError("Missing guid for region-iso-code " + (!string.IsNullOrEmpty(source.PlacesRegion) ? source.PlacesRegion : ""));
            }
            regionIsoCodes.Id = source.Guid;
            regionIsoCodes.Title = source.PlacesDesc;
            regionIsoCodes.ISOCode = source.PlacesRegion;
            regionIsoCodes.Status = (!string.IsNullOrEmpty(source.PlacesInactive) && source.PlacesInactive.Equals("Y", StringComparison.OrdinalIgnoreCase))
                ? Status.Inactive : Status.Active;
            if (!string.IsNullOrEmpty(source.PlacesCountry))
            {
                // Get the matching Place for the country of the region.  This country Place will have no region or sub-region.
                var subList = places.Where(x => x.PlacesCountry == source.PlacesCountry && (string.IsNullOrEmpty(x.PlacesRegion)) && (string.IsNullOrEmpty(x.PlacesSubRegion)));
                if (subList != null && subList.Any())
                {
                    foreach (var countryPlace in subList)
                    {
                        regionIsoCodes.Country = new GuidObject2(countryPlace.Guid);

                    }
                }
            }
            return regionIsoCodes;
        }

    }
}