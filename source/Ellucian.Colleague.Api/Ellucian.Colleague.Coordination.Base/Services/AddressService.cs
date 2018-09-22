// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;

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

        /// <summary>
        /// Get an address by guid
        /// </summary>
        /// <param name="guid">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.Addresses> GetAddressesByGuidAsync(string guid)
        {
            await CheckUserPersonViewAddressAsync();

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
                throw new Exception(ex.Message);
            }
            return addressDto;
        }

        /// <summary>
        /// Get address with paging if needed.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.Addresses>, int>> GetAddressesAsync (int offset, int limit, bool bypassCache = false)
        {
            await CheckUserPersonViewAddressAsync();

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

            var addressesDto = new List<Dtos.Addresses>() ;
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
                throw new Exception(ex.Message);
            }
            return new Tuple<IEnumerable<Dtos.Addresses>, int>( addressesDto, totalRecords);
        }

        /// <summary>
        /// Update an address by guid
        /// </summary>
        /// <param name="id">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.Addresses> PutAddressesAsync(string id, Dtos.Addresses addressDto)
        {
            await CheckUserPersonUpdateAddress();

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
                throw new Exception(ex.Message);
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
            await CheckUserPersonUpdateAddress();

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id is required to update an address.");
            }
            if (addressDto == null)
            {
                throw new ArgumentNullException("address", "Address DTO is required to update an address.");
            }
            var addressEntity = await BuildAddressEntity2Async(addressDto);
   
            _addressRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            string addressKey = "";
            if (!string.IsNullOrEmpty(addressDto.Id))
                addressKey = await _addressRepository.GetAddressFromGuidAsync(addressDto.Id);

            var updatedAddressEntity = await _addressRepository.UpdateAsync(addressKey, addressEntity);

            if (updatedAddressEntity == null)
            {
                throw new Exception("An error occurred updating address.");
            }
            try
            {
                var zipCodeGuidCollection = await _addressRepository.GetZipCodeGuidsCollectionAsync
                 (new List<string>() { updatedAddressEntity.PostalCode });

                return await BuildAddressDtoAsync(updatedAddressEntity,  zipCodeGuidCollection, true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }           
        }

        /// <summary>
        /// Update an address by guid
        /// </summary>
        /// <param name="id">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        public async Task<Dtos.Addresses> PostAddressesAsync(Dtos.Addresses addressDto)
        {
            if (addressDto == null)
            {
                throw new ArgumentNullException("address", "Address DTO is required to update an address.");
            }
            var addressEntity = await BuildAddressEntityAsync(addressDto);

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
                throw new Exception(ex.Message);
            }
            return addressDto;
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
                zipCodeGuidCollection.TryGetValue(address.PostalCode, out zipCodeGuid);
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
                        throw new Exception(string.Concat(ex.Message, "For the Country: '", address.CountryCode,  "' .ISOCode Not found: ", country.IsoAlpha3Code));
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
        /// Build a Addresses Entity object from an Address DTO
        /// </summary>
        /// <param name="addressDto">Address Entty Object</param>
        /// <returns>An address object <see cref="Address"/></returns>
        private async Task<Address> BuildAddressEntityAsync(Dtos.Addresses addressDto)
        {
            if (addressDto.AddressLines == null)
            {
                throw new ArgumentNullException("addressDto.AddressLines", "AddressLines is required for an address.");
            }
            var addressEntity = new Address();
            var addressCountry = new Dtos.AddressCountry();

            addressEntity.Guid = addressDto.Id;
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
                    throw new KeyNotFoundException(string.Concat("Unable to locate country code '", addressCountry.Code , "'"));
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
            if (addressDto.AddressLines == null)
            {
                throw new ArgumentNullException("addressDto.AddressLines", "AddressLines is required for an address.");
            }
            var addressEntity = new Address();
            var addressCountry = new Dtos.AddressCountry();

            addressEntity.Guid = addressDto.Id;
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
                if ((addressDto.Place.Country == null) || (string.IsNullOrEmpty(addressDto.Place.Country.Code.ToString())))
                {               
                    throw new ArgumentNullException("addressDto.place.country.code", "A country code is required for an address with a place defined.");
                }
                addressCountry = addressDto.Place.Country;
                var countries = await GetAllCountriesAsync(true);
                if (countries == null)
                {
                    throw new Exception("Unable to retrieve Countries");
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
                //var country = countries.FirstOrDefault(x => x.IsoAlpha3Code == addressCountry.Code.ToString());
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

                if (addressCountry.Region != null)
                {
                    if (string.IsNullOrEmpty(addressCountry.Region.Code))
                    {
                        throw new ArgumentNullException("addressDto.place.country.code", "A country code is required for an address with a place defined.");
                    }
                    var state = string.Empty;
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
        /// Provides an integration user permission to view an address using this API.
        /// </summary>
        private async Task CheckUserPersonViewAddressAsync()
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();

            // Access is ok if the current user has the update any address.
            if (!userPermissionList.Contains(BasePermissionCodes.ViewAddress))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view any address.");
                throw new PermissionsException("User is not authorized to view any address.");
            }
        }

        /// <summary>
        /// Provides an integration user permission to view an address using this API.
        /// </summary>
        private async Task CheckUserPersonUpdateAddress()
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();

            // access is ok if the current user has the update any address
            if (!userPermissionList.Contains(BasePermissionCodes.UpdateAddress))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update any address.");
                throw new PermissionsException("User is not authorized to update any address.");
            }
        }
    }
}