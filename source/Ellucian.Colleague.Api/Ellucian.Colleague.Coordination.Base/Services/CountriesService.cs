// Copyright 2019 Ellucian Company L.P. and its affiliates..

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
    public class CountriesService : BaseCoordinationService, ICountriesService
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;

        public CountriesService(IReferenceDataRepository referenceDataRepository, IAdapterRegistry adapterRegistry, 
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ICountryRepository countryRepository,
            IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _countryRepository = countryRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Gets all countries
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Countries">countries</see> objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Countries>> GetCountriesAsync(bool bypassCache = false)
        {
            var countriesCollection = new List<Ellucian.Colleague.Dtos.Countries>();

            List<Domain.Base.Entities.Country> countryEntities = null;

            try
            {
                countryEntities = (await _countryRepository.GetCountryCodesAsync(bypassCache)).ToList();
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            foreach (var countryEntity in countryEntities)
            {
                countriesCollection.Add(ConvertCountryEntityToCountriesDto(countryEntity));
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return countriesCollection;
        }

        /// <summary>
        /// Get a countries by guid.
        /// </summary>
        /// <param name="guid">Guid of the countries in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="Countries">countries</see></returns>
        public async Task<Dtos.Countries> GetCountriesByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a country.");
            }

            Domain.Base.Entities.Country countryEntity = null;
            try
            {
                countryEntity = await _countryRepository.GetCountryByGuidAsync(guid);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            if (countryEntity == null)
            {
                throw new KeyNotFoundException("No country was found for guid " + guid);
            }

            var retval = ConvertCountryEntityToCountriesDto(countryEntity);

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return retval;
        }

        /// <summary>
        /// Update a countries by guid.
        /// </summary>
        /// <param name="guid">Guid of the countries in Colleague.</param>
        /// <param name="countries">countries dto to update</param>
        /// <returns>The <see cref="Countries">updated countries</see></returns>
        public async Task<Dtos.Countries> PutCountriesAsync(string guid, Dtos.Countries countries)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to update a Country.");
            }

            if (countries == null)
            {
                throw new ArgumentNullException("countries", "Message body required to update a Country");
            }

            CheckCountryCreateUpdatePermissions();

            Countries existingCountryDto = null;

            //Only allow updates on existing records.
            try
            {
                existingCountryDto = await this.GetCountriesByGuidAsync(guid);
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("No country was found for guid " + guid);
            }
            
            Validate(guid, countries, existingCountryDto);

            _countryRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            var entity = this.ConvertCountriesDtoToCountryEntity(countries);
            if (IntegrationApiException != null)
                throw IntegrationApiException;

            Domain.Base.Entities.Country newEntity = null;
            try
            {
                newEntity = await _countryRepository.UpdateCountryAsync(entity);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            var newDto = ConvertCountryEntityToCountriesDto(newEntity);

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return newDto;
        }

        /// <summary>
        /// Validate countries dto for update
        /// </summary>
        /// <param name="guid">countries guid</param>
        /// <param name="countries">countries dto</param>
        /// <param name="existingCountryDto">existing countries record</param>
        private void Validate(string guid, Countries countries, Countries existingCountryDto)
        {
            if (existingCountryDto == null)
            {
                throw new KeyNotFoundException("No country was found for guid " + guid);
            }

            //If the code is included in the request as an empty string, then issue an error 
            if (string.IsNullOrEmpty(countries.Code))
            {
                IntegrationApiExceptionAddError("The code cannot be removed for an existing country.");
            }
            if (string.IsNullOrEmpty(countries.Title))
            {
                IntegrationApiExceptionAddError("The title is required.");
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            //The code cannot be changed to a different code than the original. 
            if (!existingCountryDto.Code.Equals(countries.Code))
            {
                IntegrationApiExceptionAddError("The code cannot be changed for an existing country.", guid: guid);
            }
            
            // Colleague allows for an empty CTRY.DESC, but the field is required. 
            // On GET, If no description is found in CTRY.DESC, then we will publish the COUNTRIES.ID as the title, therefore,
            // on UPDATE, allow an incoming title (CTRY.DESC) that matches the existing records code (COUNTRIES.ID).
            if (string.IsNullOrEmpty(existingCountryDto.Title) && !existingCountryDto.Code.Equals(countries.Title))
            {
                IntegrationApiExceptionAddError("The title cannot be changed for an existing country.", guid: guid);
            }
            //Otherwise, if the existing record has a CTRY.DESC, then it must match the value from the payload.
            else if (!string.IsNullOrEmpty(existingCountryDto.Title) && !existingCountryDto.Title.Equals(countries.Title))
            {
                IntegrationApiExceptionAddError("The title cannot be changed for an existing country.", guid: guid);
            }

            if ((!string.IsNullOrEmpty(countries.ISOCode)) && (countries.ISOCode.Length > 3))
            {
                IntegrationApiExceptionAddError("The ISO Code can not be greater than 3 characters.", guid: guid);
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Convert country domain entity to a countries DTO
        /// </summary>
        /// <param name="source">country domain entity</param>
        /// <returns>countries DTO</returns>
        private Dtos.Countries ConvertCountryEntityToCountriesDto(Domain.Base.Entities.Country source)
        {
            var country = new Ellucian.Colleague.Dtos.Countries();

            if (source == null)
            {
                IntegrationApiExceptionAddError("Country body is required.");
                return null;
            }

            if (source.Guid == null)
            {
                IntegrationApiExceptionAddError("Guid is missing on existing country.", id: source.Code);
                return null;
            }
            country.Id = source.Guid;
            country.Code = source.Code;
            country.Title = source.Description;
            country.ISOCode = string.IsNullOrEmpty(source.IsoAlpha3Code) ? null : source.IsoAlpha3Code;

            return country;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert a countries dto to a country entity
        /// </summary>
        /// <param name="source">Countries DTO</param>
        /// <returns>Country domain entity</returns>
        private Domain.Base.Entities.Country ConvertCountriesDtoToCountryEntity(Dtos.Countries source)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("Country body is required.");
                throw IntegrationApiException;
            }
            if (source.Id == null)
            {
                IntegrationApiExceptionAddError("Country.Id is required.");
                throw IntegrationApiException;
            }
            
            Domain.Base.Entities.Country country = null;

            try
            {             
                country = new Domain.Base.Entities.Country(source.Id, source.Code, source.Title, "",
                    (!string.IsNullOrEmpty(source.ISOCode)) ? source.ISOCode.ToUpper() : string.Empty);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An error occurred building the country: " + ex.Message,  id:source.Id);
            }
            return country;
        }

        /// <summary>
        /// Provides an integration user permission to update Countries
        /// </summary>
        private void CheckCountryCreateUpdatePermissions()
        {
            // access is ok if the current user has the create/update country permission
            if (!HasPermission(BasePermissionCodes.UpdateCountry))
            {
                logger.Error("User '" + CurrentUser.UserId + "' does not have permission to update countries.");
                throw new PermissionsException("User '" + CurrentUser.UserId + "' does not have permission to update countries.");
            }
        }

        public async Task<IEnumerable<CountryIsoCodes>> GetCountryIsoCodesAsync(bool bypassCache = false)
        {
            var countryIsoCodesCollection = new List<Ellucian.Colleague.Dtos.CountryIsoCodes>();

            var countryIsoCodesEntities = await _referenceDataRepository.GetPlacesAsync(bypassCache);

            if (countryIsoCodesEntities != null && countryIsoCodesEntities.Any())
            {
                var subList = countryIsoCodesEntities.Where(x => !(string.IsNullOrEmpty(x.PlacesCountry))
                && (string.IsNullOrEmpty(x.PlacesRegion)) && (string.IsNullOrEmpty(x.PlacesSubRegion)));

                if (subList != null && subList.Any())
                {
                    foreach (var countryIsoCode in subList)
                    {
                        try
                        {
                            countryIsoCodesCollection.Add(ConvertPlaceEntityToCountryIsoCodesDto(countryIsoCode));
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError("Error occurred extracting country-iso-code: " + ex.Message, guid: countryIsoCode.Guid);
                        }
                    }
                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }
                }
            }
            return countryIsoCodesCollection;
        }

        public async Task<CountryIsoCodes> GetCountryIsoCodesByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                var countryIsoCode = (await _referenceDataRepository.GetPlaceByGuidAsync(guid));

                if (countryIsoCode == null)
                {
                    throw new KeyNotFoundException();
                }
                return ConvertPlaceEntityToCountryIsoCodesDto(countryIsoCode);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No country-iso-codes was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No country-iso-codes was found for guid '{0}'", guid), ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Places domain entity to its corresponding CountryIsoCodes DTO
        /// </summary>
        /// <param name="source">Places domain entity</param>
        /// <returns>CountryIsoCodes DTO</returns>
        private Ellucian.Colleague.Dtos.CountryIsoCodes ConvertPlaceEntityToCountryIsoCodesDto(Place source)
        {
            if (source == null)
                return null;

            var countryIsoCodes = new Ellucian.Colleague.Dtos.CountryIsoCodes();

            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError("Missing guid for country-iso-code " + (!string.IsNullOrEmpty(source.PlacesCountry) ? source.PlacesCountry : ""));
            }
            countryIsoCodes.Id = source.Guid;
            countryIsoCodes.Title = source.PlacesDesc;
            countryIsoCodes.ISOCode = source.PlacesCountry;
            countryIsoCodes.Status = (!string.IsNullOrEmpty(source.PlacesInactive) && source.PlacesInactive.Equals("Y", StringComparison.OrdinalIgnoreCase))
                ? Status.Inactive : Status.Active;
           
            return countryIsoCodes;
        }

       
    }
}