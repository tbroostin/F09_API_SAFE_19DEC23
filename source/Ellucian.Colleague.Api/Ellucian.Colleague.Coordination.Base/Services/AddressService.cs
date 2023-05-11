// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Coordination service for AddressService
    /// </summary>
    [RegisterType]
    public class AddressService : BaseCoordinationService, IAddressService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";
        private readonly IConfigurationRepository _configurationRepository;

        public AddressService(IAdapterRegistry adapterRegistry, IAddressRepository addressRepository, IConfigurationRepository configurationRepository, IReferenceDataRepository referenceDataRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _addressRepository = addressRepository;
            _configurationRepository = configurationRepository;
            _logger = logger;
        }

        private IEnumerable<Country> _countries = null;
        private async Task<IEnumerable<Country>> GetAllCountriesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await _referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        private IEnumerable<Chapter> _chapters = null;
        private async Task<IEnumerable<Chapter>> GetAllChaptersAsync(bool bypassCache)
        {
            if (_chapters == null)
            {
                _chapters = await _referenceDataRepository.GetChaptersAsync(bypassCache);
            }
            return _chapters;
        }

        private IEnumerable<State> _states = null;
        private async Task<IEnumerable<State>> GetAllStatesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await _referenceDataRepository.GetStateCodesAsync(bypassCache);
            }
            return _states;
        }

        private IEnumerable<County> _counties = null;
        private async Task<IEnumerable<County>> GetAllCountiesAsync(bool bypassCache)
        {
            if (_counties == null)
            {
                _counties = await _referenceDataRepository.GetCountiesAsync(bypassCache);
            }
            return _counties;
        }

        /// <summary>
        /// Get an address by guid
        /// </summary>
        /// <param name="guid">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.Addresses> GetAddressesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "guid is required to get an address.");
            }

            var address = await _addressRepository.GetAddressAsync(guid);

            if (address == null)
            {
                throw new KeyNotFoundException("id not valid.");
            }
            Dtos.Addresses addressDto = null;
            try
            {
                var zipCodeGuidCollection = await _addressRepository.GetZipCodeGuidsCollectionAsync
                 (new List<string>() { address.PostalCode });

                addressDto = await BuildAddressDtoAsync(address, zipCodeGuidCollection, true);
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message);
            }
            return addressDto;
        }

        /// <summary>
        /// Get an address by guid
        /// </summary>
        /// <param name="guid">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.Addresses> GetAddressesByGuid2Async(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("addresses.guid", "Guid is required to get an address.");
            }

            Address address = null;
            try
            {
                address = await _addressRepository.GetAddress2Async(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                IntegrationApiExceptionAddError(string.Format("No addresses was found for guid '{0}'.", guid)
                    , guid: guid, httpStatusCode: System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }

            Dictionary<string, string> zipCodeGuidCollection = null;
            try
            {
                zipCodeGuidCollection = await _addressRepository.GetZipCodeGuidsCollectionAsync
                 (new List<string>() { address.PostalCode });
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            var addressDto = await BuildAddressDto2Async(address, zipCodeGuidCollection, true);
            if (IntegrationApiException != null)
                throw IntegrationApiException;
            return addressDto;
        }

        /// <summary>
        /// Get address with paging if needed.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.Addresses>, int>> GetAddressesAsync(int offset, int limit, bool bypassCache = false)
        {
            var addresses = await _addressRepository.GetAddressesAsync(offset, limit);
            var addressEntities = addresses.Item1;
            var totalRecords = addresses.Item2;

            if (addressEntities == null)
            {
                throw new KeyNotFoundException("id not valid.");
            }
            var ids = addressEntities
                  .Where(x => (!string.IsNullOrEmpty(x.PostalCode)))
                  .Select(x => x.PostalCode).Distinct().ToList();

            var zipCodeGuidCollection = await _addressRepository.GetZipCodeGuidsCollectionAsync(ids);

            var addressesDto = new List<Dtos.Addresses>();
            try
            {
                foreach (var address in addressEntities)
                {
                    var addressDto = await BuildAddressDtoAsync(address, zipCodeGuidCollection, bypassCache);
                    addressesDto.Add(addressDto);
                }

            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message);
            }
            return new Tuple<IEnumerable<Dtos.Addresses>, int>(addressesDto, totalRecords);
        }

        /// <summary>
        /// Get address with paging if needed.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.Addresses>, int>> GetAddresses2Async(int offset, int limit, string personFilter, bool bypassCache = false)
        {           
            string[] filterPersonIds = new List<string>().ToArray();
            if (!string.IsNullOrEmpty(personFilter))
            {
                var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                if (personFilterKeys != null)
                {
                    filterPersonIds = personFilterKeys;
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.Addresses>, int>(new List<Dtos.Addresses>(), 0);
                }
            }

            Tuple<IEnumerable<Address>, int> addresses = null;
            var addressesDto = new List<Dtos.Addresses>();
            try
            {
                addresses = await _addressRepository.GetAddresses2Async(offset, limit, filterPersonIds);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            if (addresses == null || addresses.Item2 == 0)
            {
                return new Tuple<IEnumerable<Dtos.Addresses>, int>(addressesDto, 0);
            }
            var addressEntities = addresses.Item1;
            var totalRecords = addresses.Item2;

            var ids = addressEntities
                  .Where(x => (!string.IsNullOrEmpty(x.PostalCode)))
                  .Select(x => x.PostalCode).Distinct().ToList();

            Dictionary<string, string> zipCodeGuidCollection = null;

            if (ids != null && ids.Any())
            {
                try
                {
                    zipCodeGuidCollection = await _addressRepository.GetZipCodeGuidsCollectionAsync(ids);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
            }

            foreach (var address in addressEntities)
            {
                try
                {
                    var addressDto = await BuildAddressDto2Async(address, zipCodeGuidCollection, bypassCache);
                    addressesDto.Add(addressDto);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, guid: address.Guid, id: address.AddressId);
                }
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return new Tuple<IEnumerable<Dtos.Addresses>, int>(addressesDto, totalRecords);
        }

        /// <summary>
        /// Update an address by guid
        /// </summary>
        /// <param name="id">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.Addresses> PutAddressesAsync(string id, Dtos.Addresses addressDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id is required to update an address.");
            }
            if (addressDto == null)
            {
                throw new ArgumentNullException("address", "Address DTO is required to update an address.");
            }
            var addressEntity = await BuildAddressEntityAsync(addressDto);

            _addressRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            string addressKey = "";
            if (!string.IsNullOrEmpty(addressDto.Id))
                addressKey = await _addressRepository.GetAddressFromGuidAsync(addressDto.Id);

            addressEntity = await _addressRepository.UpdateAsync(addressKey, addressEntity);

            if (addressEntity == null)
            {
                throw new KeyNotFoundException("id not valid.");
            }
            try
            {
                var zipCodeGuidCollection = await _addressRepository.GetZipCodeGuidsCollectionAsync
                 (new List<string>() { addressEntity.PostalCode });

                addressDto = await BuildAddressDtoAsync(addressEntity, zipCodeGuidCollection, true);
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message);
            }
            return addressDto;
        }

        /// <summary>
        /// Update an address by guid
        /// </summary>
        /// <param name="id">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.Addresses> PutAddresses2Async(string id, Dtos.Addresses addressDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id is required to update an address.");
            }
            if (addressDto == null)
            {
                throw new ArgumentNullException("address", "Address DTO is required to update an address.");
            }
            var addressEntity = await BuildAddressEntity2Async(addressDto);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            Address updatedAddressEntity = null;
            var addressKey = string.Empty;
            _addressRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            try
            {

                if (!string.IsNullOrEmpty(addressDto.Id))
                {
                    addressKey = await _addressRepository.GetAddressFromGuidAsync(addressDto.Id);
                }
                updatedAddressEntity = await _addressRepository.Update2Async(addressKey, addressEntity);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            try
            {
                var zipCodeGuidCollection = await _addressRepository.GetZipCodeGuidsCollectionAsync
                 (new List<string>() { updatedAddressEntity.PostalCode });

                var retval = await BuildAddressDto2Async(updatedAddressEntity, zipCodeGuidCollection, true);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return retval;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Update an address by guid
        /// </summary>
        /// <param name="id">guid for the address</param>
        public async Task DeleteAddressesAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Address id is required to delete an address.");
            }

            await _addressRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Update an address by guid
        /// </summary>
        /// <param name="id">guid for the address</param>
        public async Task QueryAddressPermissionAsync(IEnumerable<string> personIds)
        {
            try
            {
                await CheckUserPersonQueryAddresses(personIds);
            }
            catch (PermissionsException pex)
            {
                logger.Error(pex.Message);
                throw new PermissionsException("User is not authorized to view requested address.");
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message);
            }
        }

        /// <summary>
        /// Get all current addresses for a person
        /// </summary>
        /// <param name="personId">Person to get addresses for</param>
        /// <accessComments>
        /// Users can retrieve their own addresses or person's with permission VIEW.PERSON.INFORMATION or EDIT.VENDOR.BANKING.INFORMATION can retrieve addresses for other users.
        /// </accessComments>
        /// <returns>List of Address Objects <see cref="Ellucian.Colleague.Dtos.Base.Address">Address</see></returns>
        public async Task<IEnumerable<Dtos.Base.Address>> GetPersonAddresses2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person Id is required to retrieve the person's addresses.");
            }
            await CheckGetPersonViewPermission( personId);
            var addressDtoCollection = new List<Ellucian.Colleague.Dtos.Base.Address>();
            var addressCollection = await _addressRepository.GetPersonAddressesAsync(personId);
            // Get the right adapter for the type mapping
            var addressDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Address, Ellucian.Colleague.Dtos.Base.Address>();
            // Map the Address entity to the Address DTO
            foreach (var address in addressCollection)
            {
                addressDtoCollection.Add(addressDtoAdapter.MapToType(address));
            }

            return addressDtoCollection;
        }


        /// <summary>
        /// Build a Addresses DTO object from an Address entity
        /// </summary>
        /// <param name="address">Address Entity Object</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        private async Task<Dtos.Addresses> BuildAddressDtoAsync(Address address,
            Dictionary<string, string> zipCodeGuidCollection, bool bypassCache = false)
        {
            var addressDto = new Dtos.Addresses();
            List<Dtos.GuidObject2> geographicAreas = new List<Dtos.GuidObject2>();
            Dtos.AddressCountry addressCountry = new Dtos.AddressCountry();
            Dtos.AddressRegion region = null;
            Dtos.AddressSubRegion subRegion = null;

            addressDto.Id = address.Guid;
            addressDto.AddressLines = address.AddressLines;
            addressDto.Latitude = address.Latitude;
            addressDto.Longitude = address.Longitude;

            if ((!string.IsNullOrEmpty(address.PostalCode)) && (zipCodeGuidCollection != null) && (zipCodeGuidCollection.Any()))
            {
                var zipCodeGuid = string.Empty;
                var postalCode = address.PostalCode.Split('-')[0].ToString().Replace(" ", "");
                zipCodeGuidCollection.TryGetValue(postalCode, out zipCodeGuid);
                if (string.IsNullOrEmpty(zipCodeGuid) && postalCode.Length >= 3)
                {
                    postalCode = postalCode.Substring(0, 3);
                    zipCodeGuidCollection.TryGetValue(postalCode, out zipCodeGuid);
                }
                // Do not throw an error if the zip code guid is not found since all domestic and international postal codes may not be assigned guids
                if (!string.IsNullOrEmpty(zipCodeGuid))
                {
                    geographicAreas.Add(new Dtos.GuidObject2() { Id = zipCodeGuid });
                }
            }


            if ((address.AddressChapter != null) && (address.AddressChapter.Any()))
            {
                //var chapterEntities = await _referenceDataRepository.GetChaptersAsync(false);
                var chapterEntities = await GetAllChaptersAsync(bypassCache);
                foreach (string addressChapter in address.AddressChapter)
                {
                    var chapter = chapterEntities.FirstOrDefault(x => x.Code == addressChapter);
                    if (chapter != null)
                        geographicAreas.Add(new Dtos.GuidObject2() { Id = chapter.Guid });
                }
            }

            Country country = null;
            if (!string.IsNullOrEmpty(address.CountryCode))
                country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == address.CountryCode);
            else
            {
                if (!string.IsNullOrEmpty(address.State))
                {
                    //var states = (await _referenceDataRepository.GetStateCodesAsync()).FirstOrDefault(x => x.Code == address.State);
                    var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == address.State);
                    if (states != null)
                    {
                        if (!string.IsNullOrEmpty(states.CountryCode))
                        {
                            country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == states.CountryCode);
                        }
                    }
                }
                if (country == null)
                {
                    var hostCountry = await _addressRepository.GetHostCountryAsync();
                    if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "USA");
                    else
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "CAN");
                }
            }
            if (country == null)
            {
                if (!string.IsNullOrEmpty(address.CountryCode))
                {
                    throw new KeyNotFoundException("Unable to locate ISO country code for " + address.CountryCode);
                }
                throw new KeyNotFoundException("Unable to locate ISO country code for " + (await _addressRepository.GetHostCountryAsync()));
            }
            //need to check to make sure ISO code is there.
            if (country != null && string.IsNullOrEmpty(country.IsoAlpha3Code))
                throw new KeyNotFoundException("Unable to locate ISO country code for " + country.Code);

            switch (country.IsoAlpha3Code)
            {
                case "USA":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.USA;
                    addressCountry.PostalTitle = "UNITED STATES OF AMERICA";
                    addressCountry.CorrectionDigit = string.IsNullOrEmpty(address.CorrectionDigit) ? null : address.CorrectionDigit;
                    addressCountry.CarrierRoute = string.IsNullOrEmpty(address.CarrierRoute) ? null : address.CarrierRoute;
                    addressCountry.DeliveryPoint = string.IsNullOrEmpty(address.DeliveryPoint) ? null : address.DeliveryPoint;
                    break;
                case "CAN":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.CAN;
                    addressCountry.PostalTitle = "CANADA";
                    break;
                case "AUS":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.AUS;
                    addressCountry.PostalTitle = "AUSTRALIA";
                    break;
                case "BRA":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.BRA;
                    addressCountry.PostalTitle = "BRAZIL";
                    break;
                case "MEX":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.MEX;
                    addressCountry.PostalTitle = "MEXICO";
                    break;
                case "NLD":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.NLD;
                    addressCountry.PostalTitle = "NETHERLANDS";
                    break;
                case "GBR":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.GBR;
                    addressCountry.PostalTitle = "UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND";
                    break;
                default:
                    try
                    {
                        addressCountry.Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), country.IsoAlpha3Code);
                    }
                    catch (Exception ex)
                    {
                        throw new ColleagueWebApiException(string.Concat(ex.Message, "For the Country: '", address.CountryCode, "' .ISOCode Not found: ", country.IsoAlpha3Code));
                    }

                    addressCountry.PostalTitle = country.Description.ToUpper();
                    break;
            }

            if (!string.IsNullOrEmpty(address.State))
            {
                //var states = (await _referenceDataRepository.GetStateCodesAsync()).FirstOrDefault(x => x.Code == address.State);
                var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == address.State);
                if (states != null)
                {
                    region = new Dtos.AddressRegion();
                    region.Code = string.Concat(country.IsoCode, "-", states.Code);
                    region.Title = states.Description;
                }
            }
            else if (!string.IsNullOrEmpty(address.IntlRegion))
            {
                region = new Dtos.AddressRegion();
                region.Code = address.IntlRegion;
                var places = (await _addressRepository.GetPlacesAsync())
                    .FirstOrDefault(x => x.PlacesRegion == address.IntlRegion && x.PlacesCountry == country.IsoAlpha3Code && x.PlacesSubRegion == string.Empty);
                if (places != null)

                    region.Title = places.PlacesDesc;
            }
            if (region != null)
            {
                addressCountry.Region = region;
            }

            if (!string.IsNullOrEmpty(address.County))
            {
                //var county = _referenceDataRepository.Counties.FirstOrDefault(c => c.Code == address.County);
                var county = (await GetAllCountiesAsync(bypassCache)).FirstOrDefault(c => c.Code == address.County);
                if (county != null)
                {
                    subRegion = new Dtos.AddressSubRegion();
                    subRegion.Code = county.Code;
                    subRegion.Title = county.Description;

                    geographicAreas.Add(new Dtos.GuidObject2() { Id = county.Guid });
                }
            }
            else if (!string.IsNullOrEmpty(address.IntlSubRegion))
            {
                subRegion = new Dtos.AddressSubRegion();
                subRegion.Code = address.IntlSubRegion;
                var places = (await _addressRepository.GetPlacesAsync())
                    .FirstOrDefault(x => x.PlacesSubRegion == address.IntlSubRegion && x.PlacesCountry == country.IsoAlpha3Code);
                if (places != null)
                    subRegion.Title = places.PlacesDesc;
            }

            if (subRegion != null)
            {
                addressCountry.SubRegion = subRegion;
            }
            if (!string.IsNullOrEmpty(address.IntlLocality))
            {
                addressCountry.Locality = address.IntlLocality;
            }
            else
            {
                if (!string.IsNullOrEmpty(address.City))
                {
                    addressCountry.Locality = address.City;
                }
            }
            addressCountry.PostalCode = !string.IsNullOrEmpty(address.IntlPostalCode) ? address.IntlPostalCode : address.PostalCode;
            if (addressCountry.PostalCode == string.Empty) addressCountry.PostalCode = null;

            if (country != null)
                addressCountry.Title = country.Description;

            if (addressCountry != null
                && (!string.IsNullOrEmpty(addressCountry.Locality)
                || !string.IsNullOrEmpty(addressCountry.PostalCode)
                || addressCountry.Region != null
                || addressCountry.SubRegion != null
                || !string.IsNullOrEmpty(addressCountry.CorrectionDigit)
                || !string.IsNullOrEmpty(addressCountry.DeliveryPoint)
                || !string.IsNullOrEmpty(addressCountry.CarrierRoute)
                || !string.IsNullOrEmpty(address.CountryCode)))

                addressDto.Place = new Dtos.AddressPlace() { Country = addressCountry };

            if ((geographicAreas != null) && (geographicAreas.Any()))
                addressDto.GeographicAreas = geographicAreas;

            return addressDto;
        }

        /// <summary>
        /// Build a Addresses DTO object from an Address entity
        /// </summary>
        /// <param name="address">Address Entity Object</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        private async Task<Dtos.Addresses> BuildAddressDto2Async(Address address,
            Dictionary<string, string> zipCodeGuidCollection, bool bypassCache = false)
        {
            var geographicAreas = new List<Dtos.GuidObject2>();
            var addressCountry = new Dtos.AddressCountry();

            var addressDto = new Dtos.Addresses()
            {
                Id = address.Guid,
                
                Latitude = address.Latitude,
                Longitude = address.Longitude
            };
            if (address.AddressLines == null || address.AddressLines.Count() == 0)
               addressDto.AddressLines = new List<string>() { " " };
            else
               addressDto.AddressLines = address.AddressLines;

            if ((!string.IsNullOrEmpty(address.PostalCode)) && (zipCodeGuidCollection != null) && (zipCodeGuidCollection.Any()))
            {
                var zipCodeGuid = string.Empty;
                var postalCode = address.PostalCode.Split('-')[0].ToString().Replace(" ", "");
                zipCodeGuidCollection.TryGetValue(postalCode, out zipCodeGuid);
                if (string.IsNullOrEmpty(zipCodeGuid) && postalCode.Length >= 3)
                {
                    postalCode = postalCode.Substring(0, 3);
                    zipCodeGuidCollection.TryGetValue(postalCode, out zipCodeGuid);
                }
                // Do not throw an error if the zip code guid is not found since all domestic and international postal codes may not be assigned guids
                if (!string.IsNullOrEmpty(zipCodeGuid))
                {
                    geographicAreas.Add(new Dtos.GuidObject2() { Id = zipCodeGuid });
                }
            }

            if ((address.AddressChapter != null) && (address.AddressChapter.Any()))
            {
                var chapterEntities = await GetAllChaptersAsync(bypassCache);
                foreach (string addressChapter in address.AddressChapter)
                {
                    var chapter = chapterEntities.FirstOrDefault(x => x.Code == addressChapter);
                    if (chapter != null)
                        geographicAreas.Add(new Dtos.GuidObject2() { Id = chapter.Guid });
                }
            }

            var country = await GetCountryEntityAsync(address, bypassCache);
            if (country == null)
            {
                var errorMessage = string.Format("Unable to locate ISO country code for '{0}'.",
                     string.IsNullOrEmpty(address.CountryCode), await _addressRepository.GetHostCountryAsync(), address.CountryCode);
                IntegrationApiExceptionAddError(errorMessage, code: "Bad.Data", guid: address.Guid, id: address.AddressId);
            }
            //need to check to make sure ISO code is there.
            else if (string.IsNullOrEmpty(country.IsoAlpha3Code))
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate ISO country code for '{0}'.", country.Code),
                    code: "Bad.Data", guid: address.Guid, id: address.AddressId);
            }
            else
            {
                addressCountry.Title = country.Description;

                switch (country.IsoAlpha3Code)
                {
                    case "USA":
                        addressCountry.Code = Dtos.EnumProperties.IsoCode.USA;
                        addressCountry.PostalTitle = GetEnumMemberAttrValue(typeof(Dtos.EnumProperties.PostalTitle), Dtos.EnumProperties.PostalTitle.USA);
                        addressCountry.CorrectionDigit = string.IsNullOrEmpty(address.CorrectionDigit) ? null : address.CorrectionDigit;
                        addressCountry.CarrierRoute = string.IsNullOrEmpty(address.CarrierRoute) ? null : address.CarrierRoute;
                        addressCountry.DeliveryPoint = string.IsNullOrEmpty(address.DeliveryPoint) ? null : address.DeliveryPoint;
                        break;
                    case "CAN":
                        addressCountry.Code = Dtos.EnumProperties.IsoCode.CAN;
                        addressCountry.PostalTitle = GetEnumMemberAttrValue(typeof(Dtos.EnumProperties.PostalTitle), Dtos.EnumProperties.PostalTitle.CAN);
                        break;
                    case "AUS":
                        addressCountry.Code = Dtos.EnumProperties.IsoCode.AUS;
                        addressCountry.PostalTitle = GetEnumMemberAttrValue(typeof(Dtos.EnumProperties.PostalTitle), Dtos.EnumProperties.PostalTitle.AUS);
                        break;
                    case "BRA":
                        addressCountry.Code = Dtos.EnumProperties.IsoCode.BRA;
                        addressCountry.PostalTitle = GetEnumMemberAttrValue(typeof(Dtos.EnumProperties.PostalTitle), Dtos.EnumProperties.PostalTitle.BRA);
                        break;
                    case "MEX":
                        addressCountry.Code = Dtos.EnumProperties.IsoCode.MEX;
                        addressCountry.PostalTitle = GetEnumMemberAttrValue(typeof(Dtos.EnumProperties.PostalTitle), Dtos.EnumProperties.PostalTitle.MEX);
                        break;
                    case "NLD":
                        addressCountry.Code = Dtos.EnumProperties.IsoCode.NLD;
                        addressCountry.PostalTitle = GetEnumMemberAttrValue(typeof(Dtos.EnumProperties.PostalTitle), Dtos.EnumProperties.PostalTitle.NLD);
                        break;
                    case "GBR":
                        addressCountry.Code = Dtos.EnumProperties.IsoCode.GBR;
                        addressCountry.PostalTitle = GetEnumMemberAttrValue(typeof(Dtos.EnumProperties.PostalTitle), Dtos.EnumProperties.PostalTitle.UK);
                        break;
                    default:
                        try
                        {
                            addressCountry.Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), country.IsoAlpha3Code);
                        }
                        catch (Exception)
                        {
                            IntegrationApiExceptionAddError(string.Format("ISOCode '{0}' not found for country '{1}'.", country.IsoAlpha3Code, address.CountryCode),
                                code: "Bad.Data", guid: address.Guid, id: address.AddressId);
                        }
                        addressCountry.PostalTitle = country.Description.ToUpper();
                        break;
                }

                var region = await GetAddressRegionAsync(address, bypassCache, country);
                if (region != null)
                {
                    addressCountry.Region = region;
                }
            }
            var subRegion = await GetAddressSubRegionAsync(address, bypassCache, geographicAreas, country);
            if (subRegion != null)
            {
                addressCountry.SubRegion = subRegion;
            }

            if (!string.IsNullOrEmpty(address.IntlLocality))
            {
                addressCountry.Locality = address.IntlLocality;
            }
            else if (!string.IsNullOrEmpty(address.City))
            {
                addressCountry.Locality = address.City;
            }

            addressCountry.PostalCode = !string.IsNullOrEmpty(address.IntlPostalCode) ? address.IntlPostalCode : address.PostalCode;
            if (string.IsNullOrEmpty(addressCountry.PostalCode))
            {
                addressCountry.PostalCode = null;
            }

            if (IsAddressCountryPopulated(addressCountry, address.CountryCode))
            {
                addressDto.Place = new Dtos.AddressPlace() { Country = addressCountry };
            }

            if ((geographicAreas != null) && (geographicAreas.Any()))
            {
                addressDto.GeographicAreas = geographicAreas;
            }

            return addressDto;
        }

        private async Task<Dtos.AddressSubRegion> GetAddressSubRegionAsync(Address address, bool bypassCache, List<Dtos.GuidObject2> geographicAreas, Country country)
        {
            Dtos.AddressSubRegion subRegion = null;

            if (!string.IsNullOrEmpty(address.County))
            {
                var counties = await GetAllCountiesAsync(bypassCache);
                if (counties != null && counties.Any())
                {
                    var county = counties.FirstOrDefault(c => c.Code == address.County);
                    if (county != null)
                    {
                        subRegion = new Dtos.AddressSubRegion();
                        subRegion.Code = county.Code;
                        subRegion.Title = county.Description;

                        geographicAreas.Add(new Dtos.GuidObject2() { Id = county.Guid });
                    }
                }
            }
            else if (!string.IsNullOrEmpty(address.IntlSubRegion))
            {
                subRegion = new Dtos.AddressSubRegion();
                subRegion.Code = address.IntlSubRegion;
                var places = await _addressRepository.GetPlacesAsync();
                if (places != null && places.Any())
                {
                    var place = places
                        .FirstOrDefault(x => x.PlacesSubRegion == address.IntlSubRegion && x.PlacesCountry == country.IsoAlpha3Code);
                    if (place != null)
                        subRegion.Title = place.PlacesDesc;
                }
            }
            return subRegion;
        }

        private async Task<Dtos.AddressRegion> GetAddressRegionAsync(Address address, bool bypassCache, Country country)
        {
            Dtos.AddressRegion region = null;

            if (!string.IsNullOrEmpty(address.State))
            {
                var states = await GetAllStatesAsync(bypassCache);
                if (states != null && states.Any())
                {
                    var state = states.FirstOrDefault(x => x.Code == address.State);
                    if (state != null)
                    {
                        region = new Dtos.AddressRegion();
                        region.Code = string.Concat(country.IsoCode, "-", state.Code);
                        region.Title = state.Description;
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("The Code '{0}' is missing from the 'STATES' entity.", address.State),
                            code: "Bad.Data", guid: address.Guid, id: address.AddressId);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(address.IntlRegion))
            {
                region = new Dtos.AddressRegion();
                region.Code = address.IntlRegion;
                var places = await _addressRepository.GetPlacesAsync();
                if (places != null && places.Any())
                {
                    var place = places
                    .FirstOrDefault(x => x.PlacesRegion == address.IntlRegion && x.PlacesCountry == country.IsoAlpha3Code && x.PlacesSubRegion == string.Empty);
                    if (place != null)
                        region.Title = place.PlacesDesc;
                }
            }

            return region;
        }

        private async Task<Country> GetCountryEntityAsync(Address address, bool bypassCache)
        {
            Country country = null;
            var countries = await GetAllCountriesAsync(bypassCache);
            if (countries != null && countries.Any())
            {
                if (!string.IsNullOrEmpty(address.CountryCode))
                    country = (countries).FirstOrDefault(x => x.Code == address.CountryCode);
                else
                {
                    if (!string.IsNullOrEmpty(address.State))
                    {
                        var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == address.State);
                        if ((states != null) && (!string.IsNullOrEmpty(states.CountryCode)))
                        {
                            country = (countries).FirstOrDefault(x => x.Code == states.CountryCode);
                        }
                    }
                    if (country == null)
                    {
                        var hostCountry = await _addressRepository.GetHostCountryAsync();
                        if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                            country = (countries).FirstOrDefault(x => x.IsoAlpha3Code == "USA");
                        else
                            country = (countries).FirstOrDefault(x => x.IsoAlpha3Code == "CAN");
                    }
                }
            }
            return country;
        }

        private bool IsAddressCountryPopulated(Dtos.AddressCountry addressCountry, string countryCode)
        {
            return (addressCountry != null
               && (!string.IsNullOrEmpty(addressCountry.Locality)
               || !string.IsNullOrEmpty(addressCountry.PostalCode)
               || addressCountry.Region != null
               || addressCountry.SubRegion != null
               || !string.IsNullOrEmpty(addressCountry.CorrectionDigit)
               || !string.IsNullOrEmpty(addressCountry.DeliveryPoint)
               || !string.IsNullOrEmpty(addressCountry.CarrierRoute)
               || !string.IsNullOrEmpty(countryCode)));
        }

        /// <summary>
        /// Build a Addresses Entity object from an Address DTO
        /// </summary>
        /// <param name="addressDto">Address Entty Object</param>
        /// <returns>An address object <see cref="Address"/></returns>
        private async Task<Address> BuildAddressEntityAsync(Dtos.Addresses addressDto)
        {
            if (addressDto.AddressLines == null)
            {
                throw new ArgumentNullException("addressDto.AddressLines", "Address object is required.");
            }
            var addressEntity = new Address();
            var addressCountry = new Dtos.AddressCountry();

            addressEntity.Guid = addressDto.Id;
            if ((addressDto.AddressLines == null) || (string.IsNullOrWhiteSpace(string.Join("",addressDto.AddressLines))))
            {
                throw new ArgumentNullException("addressDto.AddressLines", "AddressLines is required for an address.");
            }

            addressEntity.AddressLines = addressDto.AddressLines;
            addressEntity.Latitude = addressDto.Latitude;
            addressEntity.Longitude = addressDto.Longitude;
            addressEntity.AddressChapter = new List<string>();

            if ((addressDto.GeographicAreas != null) && (addressDto.GeographicAreas.Any()))
            {
                var chapterEntities = await _referenceDataRepository.GetChaptersAsync(true);

                foreach (var area in addressDto.GeographicAreas)
                {
                    var geographicAreaEntity = await _referenceDataRepository.GetRecordInfoFromGuidGeographicAreaAsync(area.Id);
                    if (geographicAreaEntity == GeographicAreaTypeCategory.Fundraising)
                    {
                        var chapter = chapterEntities.FirstOrDefault(x => x.Guid == area.Id);
                        if (chapter != null)
                            addressEntity.AddressChapter.Add(chapter.Code);
                    }
                }
            }
            if (addressDto.Place != null)
            {
                if (addressDto.Place != null && addressDto.Place.Country != null && !string.IsNullOrEmpty(addressDto.Place.Country.Code.ToString()))
                {
                    addressCountry = addressDto.Place.Country;
                }
                else
                {
                    throw new ArgumentNullException("addressDto.place.country.code", "A country code is required for an address with a place defined.");
                }

                //if there is more than one country that matches the ISO code, we need to pick the country that does not have "Not in use" set to Yes
                Domain.Base.Entities.Country country = null;
                var countryEntities = (await GetAllCountriesAsync(false)).Where(cc => cc.IsoAlpha3Code == addressCountry.Code.ToString());
                if (countryEntities != null && countryEntities.Any())
                {
                    if (countryEntities.Count() > 1)
                        country = countryEntities.FirstOrDefault(con => con.IsNotInUse == false);
                    if (country == null)
                        country = countryEntities.FirstOrDefault();
                }
                //var country = (await GetAllCountriesAsync(true)).FirstOrDefault(x => x.IsoAlpha3Code == addressCountry.Code.ToString());
                if (country == null)
                {
                    throw new KeyNotFoundException(string.Concat("Unable to locate country code '", addressCountry.Code, "'"));
                }

                switch (addressCountry.Code)
                {
                    case Dtos.EnumProperties.IsoCode.USA:
                        addressEntity.Country = country.IsoAlpha3Code;
                        addressEntity.CorrectionDigit = string.IsNullOrEmpty(addressCountry.CorrectionDigit) ? null : addressCountry.CorrectionDigit;
                        addressEntity.CarrierRoute = string.IsNullOrEmpty(addressCountry.CarrierRoute) ? null : addressCountry.CarrierRoute;
                        addressEntity.DeliveryPoint = string.IsNullOrEmpty(addressCountry.DeliveryPoint) ? null : addressCountry.DeliveryPoint;
                        break;
                    default:
                        addressEntity.Country = country.IsoAlpha3Code;
                        if (!string.IsNullOrEmpty(addressCountry.CorrectionDigit) || !string.IsNullOrEmpty(addressCountry.CarrierRoute) || !string.IsNullOrEmpty(addressCountry.DeliveryPoint))
                        {
                            throw new ArgumentOutOfRangeException("addressDto.place.country", "correctionDigit, carrierRoute and deliveryPoint can only be specified when code is 'USA'.");
                        }
                        break;
                }

                if (addressCountry.Region != null && !string.IsNullOrEmpty(addressCountry.Region.Code))
                {
                    string state = "";
                    if (addressCountry.Region.Code.Contains("-"))
                        state = addressCountry.Region.Code.Substring(3);
                    var states = (await _referenceDataRepository.GetStateCodesAsync()).FirstOrDefault(x => x.Code == state);
                    if (states != null)
                        addressEntity.State = states.Code;
                    else
                        addressEntity.IntlRegion = addressCountry.Region == null ? null : addressCountry.Region.Code;
                }

                if (addressCountry.SubRegion != null && !string.IsNullOrEmpty(addressCountry.SubRegion.Code))
                {
                    var county = (await _referenceDataRepository.GetCountiesAsync(false)).FirstOrDefault(c => c.Code == addressCountry.SubRegion.Code);
                    if (county != null)
                        addressEntity.County = county.Code;
                    else
                        addressEntity.IntlSubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
                }

                addressEntity.City = string.IsNullOrEmpty(addressCountry.Locality) ? null : addressCountry.Locality;
                addressEntity.PostalCode = string.IsNullOrEmpty(addressCountry.PostalCode) ? null : addressCountry.PostalCode;
                addressEntity.IntlLocality = addressCountry.Locality;
                addressEntity.IntlPostalCode = addressCountry.PostalCode;
                addressEntity.IntlRegion = addressCountry.Region == null ? null : addressCountry.Region.Code;
                addressEntity.IntlSubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
            }

            return addressEntity;
        }

        /// <summary>
        /// Build a Addresses Entity object from an Address DTO
        /// </summary>
        /// <param name="addressDto">Address Entty Object</param>
        /// <returns>An address object <see cref="Address"/></returns>
        private async Task<Address> BuildAddressEntity2Async(Dtos.Addresses addressDto)
        {
            if (addressDto == null)
            {
                IntegrationApiExceptionAddError("Address body is required for an address.", "address");
            }
            if (string.IsNullOrEmpty(addressDto.Id))
            {
                IntegrationApiExceptionAddError("GUID is required for an address.", "addressDto.ID");
            }
            var addressCountry = new Dtos.AddressCountry();

            var addressEntity = new Address()
            {

                Guid = addressDto.Id,
                AddressLines = addressDto.AddressLines,
                Latitude = addressDto.Latitude,
                Longitude = addressDto.Longitude
            };

            if ((addressDto.GeographicAreas != null) && (addressDto.GeographicAreas.Any()))
            {
                var addressChapter = new List<string>();
                var chapterEntities = await GetAllChaptersAsync(true);

                foreach (var area in addressDto.GeographicAreas)
                {
                    var geographicAreaEntity = new GeographicAreaTypeCategory();
                    try
                    {
                        geographicAreaEntity = await _referenceDataRepository.GetRecordInfoFromGuidGeographicAreaAsync(area.Id);
                        if (geographicAreaEntity == GeographicAreaTypeCategory.Fundraising)
                        {
                            var chapter = chapterEntities.FirstOrDefault(x => x.Guid.Equals(area.Id, StringComparison.OrdinalIgnoreCase));
                            if (chapter != null)
                                addressChapter.Add(chapter.Code);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "addressDto.GeographicAreas", addressDto.Id);
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Format("Geographic Area Id '{0}' not found.", area.Id),
                        "addressDto.GeographicAreas", addressDto.Id);
                    }
                }
                if (addressChapter.Any())
                {
                    addressEntity.AddressChapter = addressChapter;
                }
            }

            if (addressDto.Place == null)
            {
                return addressEntity;
            }

            if ((addressDto.Place.Country == null) || (string.IsNullOrEmpty(addressDto.Place.Country.Code.ToString())))
            {
                IntegrationApiExceptionAddError("A country code is required for an address with a place defined.",
                    "addressDto.place.country.code", addressDto.Id);
            }
            else
            {
                addressCountry = addressDto.Place.Country;
                var countries = await GetAllCountriesAsync(true);
                if (countries == null)
                {
                    IntegrationApiExceptionAddError("Unable to retrieve Countries.", "addressDto.place.country.code",
                        addressDto.Id);
                }

                //if there is more than one country that matches the ISO code, we need to pick the country that does not have "Not in use" set to Yes
                Country country = null;
                var countryEntities = countries.Where(cc => cc.IsoAlpha3Code == addressCountry.Code.ToString());
                if (countryEntities != null && countryEntities.Any())
                {
                    if (countryEntities.Count() > 1)
                        country = countryEntities.FirstOrDefault(con => con.IsNotInUse == false);
                    if (country == null)
                        country = countryEntities.FirstOrDefault();
                }

                if (country == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate country code '{0}'.", addressCountry.Code),
                        "addressDto.place.country.code", addressDto.Id);
                }
                else
                {
                    switch (addressCountry.Code)
                    {
                        case Dtos.EnumProperties.IsoCode.USA:
                            addressEntity.Country = country.IsoAlpha3Code;
                            addressEntity.CorrectionDigit = string.IsNullOrEmpty(addressCountry.CorrectionDigit) ? null : addressCountry.CorrectionDigit;
                            addressEntity.CarrierRoute = string.IsNullOrEmpty(addressCountry.CarrierRoute) ? null : addressCountry.CarrierRoute;
                            addressEntity.DeliveryPoint = string.IsNullOrEmpty(addressCountry.DeliveryPoint) ? null : addressCountry.DeliveryPoint;
                            break;
                        default:
                            addressEntity.Country = country.IsoAlpha3Code;
                            if (!string.IsNullOrEmpty(addressCountry.CorrectionDigit) || !string.IsNullOrEmpty(addressCountry.CarrierRoute) || !string.IsNullOrEmpty(addressCountry.DeliveryPoint))
                            {
                                IntegrationApiExceptionAddError("CorrectionDigit, carrierRoute and deliveryPoint can only be specified when code is 'USA'.",
                                    "addressDto.place.country", addressDto.Id);
                            }
                            break;
                    }

                }
                if (addressCountry.Region != null)
                {
                    if (string.IsNullOrEmpty(addressCountry.Region.Code))
                    {
                        IntegrationApiExceptionAddError("A country code is required for an address with a place defined.",
                            "addressDto.place.country.code", addressDto.Id);
                    }
                    else
                    {
                        var state = string.Empty;
                        if (addressCountry.Region.Code.Contains("-"))
                            state = addressCountry.Region.Code.Substring(3);
                        var states = (await GetAllStatesAsync(false)).FirstOrDefault
                            (x => x.Code.Equals(state, StringComparison.OrdinalIgnoreCase));
                        if (states != null)
                            addressEntity.State = states.Code;
                        else
                            addressEntity.IntlRegion = addressCountry.Region.Code;
                    }
                }

                if (addressCountry.SubRegion != null && !string.IsNullOrEmpty(addressCountry.SubRegion.Code))
                {
                    var county = (await GetAllCountiesAsync(false)).FirstOrDefault
                        (c => c.Code.Equals(addressCountry.SubRegion.Code, StringComparison.OrdinalIgnoreCase));
                    if (county != null)
                        addressEntity.County = county.Code;
                    else
                    {
                        addressEntity.IntlSubRegion = addressCountry.SubRegion.Code;
                    }
                }

                addressEntity.City = string.IsNullOrEmpty(addressCountry.Locality) ? null : addressCountry.Locality;
                addressEntity.PostalCode = string.IsNullOrEmpty(addressCountry.PostalCode) ? null : addressCountry.PostalCode;
                addressEntity.IntlLocality = string.IsNullOrEmpty(addressCountry.Locality) ? null : addressCountry.Locality;
                addressEntity.IntlPostalCode = string.IsNullOrEmpty(addressCountry.PostalCode) ? null : addressCountry.PostalCode;
                addressEntity.IntlRegion = addressCountry.Region == null ? null : addressCountry.Region.Code;
                addressEntity.IntlSubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
            }
            return addressEntity;
        }

        /// <summary>
        /// Provides an integration user permission to view an address using this API.
        /// </summary>
        private async Task CheckUserPersonViewAddressAsync()
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();

            // Access is ok if the current user has the view and update any address.
            if ((!userPermissionList.Contains(BasePermissionCodes.ViewAddress)) && (!userPermissionList.Contains(BasePermissionCodes.UpdateAddress)))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view any address.");
                throw new PermissionsException("User is not authorized to view any address.");
            }
        }

        /// <summary>
        /// Provides an integration user permission to view an address using this API.
        /// </summary>
        private async Task CheckUserPersonViewAddress2Async()
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();

            // Access is ok if the current user has the view or update address permission
            if ((!userPermissionList.Contains(BasePermissionCodes.ViewAddress)) && (!userPermissionList.Contains(BasePermissionCodes.UpdateAddress)))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view addresses.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to view addresses.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Provides an integration user permission to view an address using this API. Used by CRM QueryAdresses
        /// </summary>
        private async Task CheckUserPersonQueryAddresses(IEnumerable<string> personIds)
        {
            IEnumerable<string> userPermissionList = await GetUserPermissionCodesAsync();

            // Access is ok if the current user has view.address
            if ((!userPermissionList.Contains(BasePermissionCodes.ViewAddress)) && !(personIds.Contains(CurrentUser.PersonId) && personIds.Count() == 1))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view any address.");
                throw new PermissionsException("User is not authorized to view any address.");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view person information.
        /// Only a user can retrieve its own address
        /// Or a user with EDIT.VENDOR.BANKING.INFORMATION or with VIEW.PERSON.INFORMATION can retrieve addresses
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private async Task CheckGetPersonViewPermission(string personId)
        {
            IEnumerable<string> userPermissionList = await GetUserPermissionCodesAsync();
            // Access is ok if the current user has view.address
            if (!userPermissionList.Contains(BasePermissionCodes.ViewPersonInformation) && !userPermissionList.Contains(BasePermissionCodes.EditVendorBankingInformation) && CurrentUser.PersonId != personId)
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view person information.");
                throw new PermissionsException("User is not authorized to view person information.");
            }
        }

        /// <summary>
        /// Get an EnumMemberAttribute value
        /// </summary>
        /// <param name="enumType">enumeration Type</param>
        /// <param name="enumVal">enumeration value</param>
        /// <returns>EnumMemberAttribute value</returns>
        private string GetEnumMemberAttrValue(Type enumType, object enumVal)
        {
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo[0].GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Value;
            }

            return null;
        }
    }
}