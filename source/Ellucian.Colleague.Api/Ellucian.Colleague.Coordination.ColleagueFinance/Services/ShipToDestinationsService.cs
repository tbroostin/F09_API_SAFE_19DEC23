//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class ShipToDestinationsService : BaseCoordinationService, IShipToDestinationsService
    {

        private readonly IColleagueFinanceReferenceDataRepository _cfReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public ShipToDestinationsService(

            IColleagueFinanceReferenceDataRepository cfReferenceDataRepository, 
            IReferenceDataRepository referenceDataRepository,
			IAddressRepository addressRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {

            _cfReferenceDataRepository = cfReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _addressRepository = addressRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all ship-to-destinations
        /// </summary>
        /// <returns>Collection of ShipToDestinations DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ShipToDestinations>> GetShipToDestinationsAsync(bool bypassCache = false)
        {
            var shipToDestinationsCollection = new List<Ellucian.Colleague.Dtos.ShipToDestinations>();

            var shipToDestinationsEntities = await _cfReferenceDataRepository.GetShipToDestinationsAsync(bypassCache);
            if (shipToDestinationsEntities != null && shipToDestinationsEntities.Any())
            {
                foreach (var shipToDestinations in shipToDestinationsEntities)
                {
                    shipToDestinationsCollection.Add(await ConvertShipToDestinationsEntityToDto(shipToDestinations, bypassCache));
                }
            }
            return shipToDestinationsCollection;
        }

        /// <summary>
        /// Gets all Ship to Codes with descriptions
        /// </summary>
        /// <returns>Collection of ShipToCode</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ShipToCode>> GetShipToCodesAsync()
        {
            var shipToCodeCollection = new List<Ellucian.Colleague.Dtos.ColleagueFinance.ShipToCode>();

            var shipToCodesEntities = await _cfReferenceDataRepository.GetShipToCodesAsync();
            if (shipToCodesEntities != null && shipToCodesEntities.Any())
            {
                //sort the entities on code, then by description
                shipToCodesEntities = shipToCodesEntities.OrderBy(x => x.Code);
                if (shipToCodesEntities != null && shipToCodesEntities.Any())
                {
                    foreach (var shipToCodeEntity in shipToCodesEntities)
                    {
                        //convert shipToCode entity to dto
                        shipToCodeCollection.Add(await ConvertShipToCodeEntityToDto(shipToCodeEntity));
                    }
                }
            }
            return shipToCodeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ShipToDestinations from its GUID
        /// </summary>
        /// <returns>ShipToDestinations DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ShipToDestinations> GetShipToDestinationsByGuidAsync(string guid)
        {
            try
            {
                return await ConvertShipToDestinationsEntityToDto((await _cfReferenceDataRepository.GetShipToDestinationsAsync(true)).Where(r => r.Guid == guid).First(), true);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("ship-to-destinations not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("ship-to-destinations not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ShipToDestinations domain entity to its corresponding ShipToDestinations DTO
        /// </summary>
        /// <param name="source">ShipToDestinations domain entity</param>
        /// <returns>ShipToDestinations DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.ShipToDestinations> ConvertShipToDestinationsEntityToDto(ShipToDestination source, bool bypassCache)
        {
            var shipToDestinations = new Ellucian.Colleague.Dtos.ShipToDestinations();

            shipToDestinations.Id = source.Guid;
            shipToDestinations.Code = source.Code;
            shipToDestinations.Title = source.Description;
            shipToDestinations.Description = null;
            shipToDestinations.AddressLines = source.addressLines;
            shipToDestinations.Contact = new ShipToDestinationsContact() { Name = source.contactName, 
                Phone = new PhoneDtoProperty() { Number = !string.IsNullOrEmpty(source.phoneNumber) ? source.phoneNumber : null, Extension = !string.IsNullOrEmpty(source.phoneExtension) ? source.phoneExtension : null } };
            shipToDestinations.Place = await GetAddressCountry(source.placeCountryCode, source.placeCountryLocality, source.placeCountryRegionCode, source.placeCountryPostalCode, await GetHostCountry(), bypassCache);
                                                                                                                                                                        
            return shipToDestinations;
        }

        /// <summary>
        /// Converts a ShipToCodes domain entity to its corresponding ShipToCodes DTO
        /// </summary>
        /// <param name="source">ShipToCodes domain entity</param>
        /// <returns>ShipToCodes DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.ColleagueFinance.ShipToCode> ConvertShipToCodeEntityToDto(Ellucian.Colleague.Domain.ColleagueFinance.Entities.ShipToCode source)
        {
            var shipToCode = new Ellucian.Colleague.Dtos.ColleagueFinance.ShipToCode();
            shipToCode.Code = source.Code;
            shipToCode.Description = source.Description;
            return shipToCode;
        }

        private IEnumerable<Domain.Base.Entities.Country> _countries = null;
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetAllCountriesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await _referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        private IEnumerable<ZipcodeXlat> _zipCodes = null;
        private async Task<IEnumerable<ZipcodeXlat>> GetAllZipCodesAsync(bool bypassCache)
        {
            if (_zipCodes == null)
            {
                _zipCodes = await _referenceDataRepository.GetZipCodeXlatAsync(bypassCache);
            }
            return _zipCodes;
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

        private string _hostCountry = null;
        private async Task<string> GetHostCountry()
        {
			if (_hostCountry == null)
            {
                _hostCountry = await _addressRepository.GetHostCountryAsync();
            }
            return _hostCountry;
        }

        private async Task<AddressPlace> GetAddressCountry(string addressCountry, string addressCity,
            string addressState, string addressZip, string hostCountry, bool bypassCache = false)
        {
            var addressCountryDto = new Dtos.AddressCountry();
            Dtos.AddressRegion region = null;
            Domain.Base.Entities.Country country = null;
            if (!string.IsNullOrEmpty(addressCountry))
                country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressCountry);
            else
            {
                if (!string.IsNullOrEmpty(addressState))
                {
                    var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressState);
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
                    if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "USA");
                    else
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "CAN");
                }
            }
            if (country == null)
            {
                if (!string.IsNullOrEmpty(addressCountry))
                {
                    throw new KeyNotFoundException("Unable to locate ISO country code for " + addressCountry);
                }
                throw new KeyNotFoundException("Unable to locate ISO country code for " + addressCountry);
            }

            //need to check to make sure ISO code is there.
            if (country != null && string.IsNullOrEmpty(country.IsoAlpha3Code))
                throw new ArgumentException("Unable to locate ISO country code for " + country.Code);


            switch (country.IsoAlpha3Code)
            {
                case "USA":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.USA;
                    addressCountryDto.PostalTitle = "UNITED STATES OF AMERICA";
                    break;
                case "CAN":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.CAN;
                    addressCountryDto.PostalTitle = "CANADA";
                    break;
                case "AUS":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.AUS;
                    addressCountryDto.PostalTitle = "AUSTRALIA";
                    break;
                case "BRA":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.BRA;
                    addressCountryDto.PostalTitle = "BRAZIL";
                    break;
                case "MEX":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.MEX;
                    addressCountryDto.PostalTitle = "MEXICO";
                    break;
                case "NLD":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.NLD;
                    addressCountryDto.PostalTitle = "NETHERLANDS";
                    break;
                case "GBR":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.GBR;
                    addressCountryDto.PostalTitle = "UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND";
                    break;
                default:
                    try
                    {
                        addressCountryDto.Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), country.IsoAlpha3Code);
                    }
                    catch (Exception ex)
                    {
                        throw new ColleagueWebApiException(string.Concat(ex.Message, "For the Country: '", addressCountry, "' .ISOCode Not found: ", country.IsoAlpha3Code));
                    }

                    addressCountryDto.PostalTitle = country.Description.ToUpper();
                    break;
            }

            if (!string.IsNullOrEmpty(addressState))
            {
                var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressState);
                if (states != null)
                {
                    region = new Dtos.AddressRegion();
                    region.Code = string.Concat(country.IsoCode, "-", states.Code);
                    region.Title = states.Description;
                }
            }
            if (region != null)
            {
                addressCountryDto.Region = region;
            }

            if (!string.IsNullOrEmpty(addressCity))
            {
                addressCountryDto.Locality = addressCity;
            }
            addressCountryDto.PostalCode = addressZip;

            if (country != null)
                addressCountryDto.Title = country.Description;

            if (addressCountryDto != null
                && (!string.IsNullOrEmpty(addressCountryDto.Locality)
                || !string.IsNullOrEmpty(addressCountryDto.PostalCode)
                || addressCountryDto.Region != null
                ))
            {
                return new AddressPlace()
                {
                    Country = addressCountryDto
                };
            }

            return null;
        }
        
    }
 
}