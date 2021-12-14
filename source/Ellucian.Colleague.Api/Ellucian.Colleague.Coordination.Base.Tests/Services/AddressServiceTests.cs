// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class AddressServiceTests
    {
        [TestClass]
        public class GetAddress : GenericUserFactory
        {
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;
            private IAddressRepository _addressRepository;
            private Mock<IAddressRepository> _addressRepositoryMock;
            private Mock<IAddressService> _addressServiceMock;
            private ILogger _logger;
            private AddressService _addressesService;


            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;



            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IEnumerable<Domain.Base.Entities.Address> allAddresses;
            private List<Dtos.Addresses> addressesCollection;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Place> place;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private IEnumerable<Domain.Base.Entities.Chapter> allChapters;
            private IEnumerable<Domain.Base.Entities.County> allCounties;
            private IEnumerable<Domain.Base.Entities.ZipcodeXlat> allZipCodeXlats;
            private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;

            private const string usAddressGuid = "d44134f9-0924-45d4-8b91-be9531aa7773";
            private const string foreignAddressGuid = "d44135f9-0924-45d4-8b91-be9531aa7773";

            private string[,] CleanAddressData = {
                                       {"0011304", "052 ", "PO Box 14428", "PO Box 14428", null, null, "8001", "AU", "Australia", null, "d44135f9-0924-45d4-8b91-be9531aa7773", null, "Home", "H"},
                                       {"0000304", "101 ", "65498 Ft. Belvoir Hwy;Mount Vernon;Alexandria, VA 21348", "65498 Ft. Belvoir Hwy;Mount Vernon", "Alexandria", "VA", "21348", "US", "United States of America", "Father of our Country", "d44134f9-0924-45d4-8b91-be9531aa7773", "FFX", "Home", "H"},
                                       {"0000304", "102 ", null, "235 Beacon Hill Dr.", "Boston", "MA", "03549", "US", null, null, "081ae7a2-f7b3-45f4-808b-a35f50c5c418", null, "Home", "H"},
                                       {"0000304", "103 ", "1 Champs d'Elyssie;U.S. Embassy;Paris;FRANCE", "1 Champs d'Elyssie", "Paris", null, null, "FR", "France", "Ambassador to France", "da5905c9-d607-4788-996a-f0f2567b0bd4", null, "Mailing", "MA"},
                                       {"0000404", "104", null, "1812 Dolly Madison Dr.", "Arlington", "VA", "22146", "US", null, null, "ec9da88c-b14a-4a8e-a9d0-4760b31816aa", null, "Home", "H"},
                                       {"0000404", "105", null, "1787 Constitution Ave.", "Franklin", "VA", "34567", "US", null, null, "9482c660-cbe1-4c4a-9ee1-10818a7c7f27", null, "Home", "H"},
                                       {"0000504", "106", null, "1600 Pennsylvania Ave.;The White House", "Washington", "DC", "12345", "US", null, "POTUS", "ebdd0871-54aa-4237-8a66-ddb7cbb15753", null, "Home", "H"},
                                       {"0000504", "107", null, "7413 Clifton Quarry Dr.", "Clifton", "VA", "20121", "US", null, null, "2ec57bef-8a13-4a6a-8a79-ea99d062fd27", null, "Mailing", "MA"},
                                       {"9999999", "108 ", null, null, null, null, null, null, null, null, "d43bbf09-bbdc-4b17-86cc-4a183b1ec6d6", null, "Home", "H"},
                                       {"9999998", "109 ", null, null, null, null, null, null, null, null, "3ba6b4ba-8668-42e0-a4aa-319410aff7cb", null, "Home", "H"}
                                   };

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _logger = new Mock<ILogger>().Object;
                _addressRepositoryMock = new Mock<IAddressRepository>();
                _addressRepository = _addressRepositoryMock.Object;
                _addressServiceMock = new Mock<IAddressService>();

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                currentUserFactory = new AddressUser();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                var viewAddressRole = new Domain.Entities.Role(1, "VIEW.ADDRESS");
                viewAddressRole.AddPermission(new Domain.Entities.Permission("VIEW.ADDRESS"));

                var updateAddressRole = new Domain.Entities.Role(2, "UPDATE.ADDRESS");
                updateAddressRole.AddPermission(new Domain.Entities.Permission("UPDATE.ADDRESS"));

                roleRepoMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    viewAddressRole,
                    updateAddressRole
                });

                addressesCollection = new List<Dtos.Addresses>();

                allChapters = new TestGeographicAreaRepository().GetChapters();
                allCounties = new TestGeographicAreaRepository().GetCounties();
                allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();
                allGeographicAreaTypes = new TestGeographicAreaRepository().Get();

                place = new List<Place>()
                {
                    new Place(){PlacesCountry = "FRA", PlacesDesc= "France", PlacesRegion="Normandy", PlacesSubRegion="Calvados"},
                    new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Barwon South West"},
                    new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Gippsland"},
                    new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Greater Melbourne"},
                    new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Hume"},
                    new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Loddon Mallee"}
                };
                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts"),
                    new State("DC","District of Columbia"),
                    new State("TN","tennessee")
                };
                _referenceDataRepositoryMock.Setup(repo => repo.GetStateCodesAsync()).Returns(Task.FromResult(states));
                _referenceDataRepositoryMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));


                // Mock the reference repository for country
                countries = new List<Domain.Base.Entities.Country>()
                 {
                    new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };
                _referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };
                _referenceDataRepositoryMock.Setup(repo => repo.Counties).Returns(counties);
                _referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(counties);

                allAddresses = new TestAddressRepository().GetAddressData().ToList();

                var CleanAllAddresses = CleanAddresses();

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.Addresses
                    {
                        Id = source.Guid,
                        AddressLines = source.AddressLines,
                        Latitude = source.Latitude,
                        Longitude = source.Longitude,

                    };

                    // The data setup from Alladdresses are a bit off for testing purposes.
                    // This second set gets a proper clean set.
                    var ThisCleanAddress = CleanAllAddresses.FirstOrDefault(x => x.Guid == source.Guid);
                    var CleanCountry = ThisCleanAddress.Country;

                    string City = (source.City == "") ? null : source.City;

                    var countryPlace = new Dtos.AddressCountry()
                    {
                        Code = Dtos.EnumProperties.IsoCode.USA,
                        Title = source.Country,
                        PostalTitle = "UNITED STATES OF AMERICA",
                        CarrierRoute = source.CarrierRoute,
                        DeliveryPoint = source.DeliveryPoint,
                        CorrectionDigit = source.CorrectionDigit,
                        Locality = (source.City == "") ? null : source.City,
                        PostalCode = (source.PostalCode == "") ? null : source.PostalCode,

                    };
                    var region = new Dtos.AddressRegion();
                    var countyDesc = counties.FirstOrDefault(c => c.Code == source.County);
                    var subRegion = new Dtos.AddressSubRegion();

                    if (!string.IsNullOrEmpty(source.State))
                    {
                        region.Code = CleanCountry + "-" + source.State;

                        var title = states.FirstOrDefault(x => x.Code == source.State);
                        if (title != null)
                            region.Title = title.Description;
                    }
                    else
                    {
                        region = null;

                    }
                    if (!string.IsNullOrEmpty(source.County))
                    {
                        subRegion.Code = source.County;
                        if (countyDesc != null)
                            subRegion.Title = countyDesc.Description;
                    }
                    else
                    {
                        subRegion = null;
                    }

                    countryPlace.Region = region;
                    countryPlace.SubRegion = subRegion;

                    address.Place = new Dtos.AddressPlace() { Country = countryPlace };
                    addressesCollection.Add(address);
                }

                _referenceDataRepositoryMock.Setup(repo => repo.GetChaptersAsync(It.IsAny<bool>())).ReturnsAsync(allChapters);
                //_referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(allCounties);
                _referenceDataRepositoryMock.Setup(repo => repo.GetZipCodeXlatAsync(It.IsAny<bool>())).ReturnsAsync(allZipCodeXlats);

                _addressesService = new AddressService(adapterRegistry, _addressRepository, baseConfigurationRepository, _referenceDataRepository, currentUserFactory, roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _referenceDataRepository = null;
                _addressesService = null;
                _logger = null;
                _referenceDataRepository = null;
                _referenceDataRepositoryMock = null;
                _addressRepository = null;
                _addressRepositoryMock = null;
            }

            [TestMethod]
            public async Task AddressService_GetAddressesAsync()
            {

                Tuple<IEnumerable<Domain.Base.Entities.Address>, int> _AddressesTuple =
                    new Tuple<IEnumerable<Domain.Base.Entities.Address>, int>(allAddresses, allAddresses.Count());

                _addressRepositoryMock.Setup(i =>
                    i.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(_AddressesTuple);

                var actuals = await _addressesService.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = addressesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AddressLines, actual.AddressLines);
                    //These two ID's are meant to fail so will ignore these two ID's data.
                    if (expected.Id != "d43bbf09-bbdc-4b17-86cc-4a183b1ec6d6" &&
                        expected.Id != "3ba6b4ba-8668-42e0-a4aa-319410aff7cb")
                    {
                        Assert.AreEqual(expected.Place.Country.PostalCode, actual.Place.Country.PostalCode);
                        Assert.AreEqual(expected.Place.Country.PostalTitle, actual.Place.Country.PostalTitle);
                        Assert.AreEqual(expected.Place.Country.CarrierRoute, actual.Place.Country.CarrierRoute);
                        Assert.AreEqual(expected.Place.Country.Code, actual.Place.Country.Code);
                        Assert.AreEqual(expected.Place.Country.CorrectionDigit, actual.Place.Country.CorrectionDigit);
                        Assert.AreEqual(expected.Place.Country.DeliveryPoint, actual.Place.Country.DeliveryPoint);
                        Assert.AreEqual(expected.Place.Country.Locality, actual.Place.Country.Locality);

                        if (expected.Place.Country.Region != null)
                        {
                            Assert.AreEqual(expected.Place.Country.Region.Code, actual.Place.Country.Region.Code);
                            Assert.AreEqual(expected.Place.Country.Region.Title, actual.Place.Country.Region.Title);
                        }
                        if (expected.Place.Country.SubRegion != null)
                        {
                            Assert.AreEqual(expected.Place.Country.SubRegion.Code, actual.Place.Country.SubRegion.Code);
                            Assert.AreEqual(expected.Place.Country.SubRegion.Title, actual.Place.Country.SubRegion.Title);
                        }
                    }


                }
            }

            [TestMethod]
            public async Task AddressService_GetAddressesAsync_PostalCode()
            {

                var _addressesTuple =
                    new Tuple<IEnumerable<Domain.Base.Entities.Address>, int>(allAddresses, allAddresses.Count());

                var dict = new Dictionary<string, string>();
                dict.Add("12345", "ed809943-eb26-42d0-9a95-d8db912a581f");
                dict.Add("22345", "6f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("34567", "1f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("8001", "2f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                _addressRepositoryMock.Setup(i => i.GetZipCodeGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                foreach (var address in _addressesTuple.Item1)
                {
                    address.PostalCode = "12345";
                }

                _addressRepositoryMock.Setup(i =>
                    i.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(_addressesTuple);

                var actuals = await _addressesService.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = addressesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AddressLines, actual.AddressLines);
                    //These two ID's are meant to fail so will ignore these two ID's data.
                    if (expected.Id != "d43bbf09-bbdc-4b17-86cc-4a183b1ec6d6" &&
                        expected.Id != "3ba6b4ba-8668-42e0-a4aa-319410aff7cb")
                    {
                        Assert.AreEqual("12345", actual.Place.Country.PostalCode);
                        Assert.AreEqual(expected.Place.Country.PostalTitle, actual.Place.Country.PostalTitle);
                        Assert.AreEqual(expected.Place.Country.CarrierRoute, actual.Place.Country.CarrierRoute);
                        Assert.AreEqual(expected.Place.Country.Code, actual.Place.Country.Code);
                        Assert.AreEqual(expected.Place.Country.CorrectionDigit, actual.Place.Country.CorrectionDigit);
                        Assert.AreEqual(expected.Place.Country.DeliveryPoint, actual.Place.Country.DeliveryPoint);
                        Assert.AreEqual(expected.Place.Country.Locality, actual.Place.Country.Locality);

                        if (expected.Place.Country.Region != null)
                        {
                            Assert.AreEqual(expected.Place.Country.Region.Code, actual.Place.Country.Region.Code);
                            Assert.AreEqual(expected.Place.Country.Region.Title, actual.Place.Country.Region.Title);
                        }
                        if (expected.Place.Country.SubRegion != null)
                        {
                            Assert.AreEqual(expected.Place.Country.SubRegion.Code, actual.Place.Country.SubRegion.Code);
                            Assert.AreEqual(expected.Place.Country.SubRegion.Title, actual.Place.Country.SubRegion.Title);
                        }

                        var zipCodeGuid = string.Empty;
                        dict.TryGetValue(actual.Place.Country.PostalCode, out zipCodeGuid);

                        Assert.AreEqual(zipCodeGuid, actual.GeographicAreas[0].Id);
                    }
                }
            }

            #region GetAddressById

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddressService_GetAddressByGuid_ArgumentNullException()
            {
                await _addressesService.GetAddressesByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AddressService_GetAddressById_InvalidID()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.GetAddressAsync(usAddressGuid)).ReturnsAsync(address);

                await _addressesService.GetAddressesByGuidAsync("invalid");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddressService_GetAddressById_InvalidOperationException()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.GetAddressAsync(usAddressGuid)).Throws<ArgumentNullException>();
                await _addressesService.GetAddressesByGuidAsync(usAddressGuid);
            }

            [TestMethod]
            public async Task AddressService_GetAddressById()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.GetAddressAsync(usAddressGuid)).ReturnsAsync(address);


                var actual = await _addressesService.GetAddressesByGuidAsync(usAddressGuid);
                var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AddressLines, actual.AddressLines);
                Assert.AreEqual(expected.Place.Country.PostalCode, actual.Place.Country.PostalCode);
                Assert.AreEqual(expected.Place.Country.PostalTitle, actual.Place.Country.PostalTitle);
                Assert.AreEqual(expected.Place.Country.CarrierRoute, actual.Place.Country.CarrierRoute);
                Assert.AreEqual(expected.Place.Country.Code, actual.Place.Country.Code);
                Assert.AreEqual(expected.Place.Country.CorrectionDigit, actual.Place.Country.CorrectionDigit);
                Assert.AreEqual(expected.Place.Country.DeliveryPoint, actual.Place.Country.DeliveryPoint);
                Assert.AreEqual(expected.Place.Country.Locality, actual.Place.Country.Locality);
                if (expected.Place.Country.Region != null)
                {
                    Assert.AreEqual(expected.Place.Country.Region.Code, actual.Place.Country.Region.Code);
                    Assert.AreEqual(expected.Place.Country.Region.Title, actual.Place.Country.Region.Title);
                }
                if (expected.Place.Country.SubRegion != null)
                {
                    Assert.AreEqual(expected.Place.Country.SubRegion.Code, actual.Place.Country.SubRegion.Code);
                    Assert.AreEqual(expected.Place.Country.SubRegion.Title, actual.Place.Country.SubRegion.Title);
                }
            }

            [TestMethod]
            public async Task AddressService_GetAddressById_PostalCode()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.GetAddressAsync(usAddressGuid)).ReturnsAsync(address);

                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("12345", "ed809943-eb26-42d0-9a95-d8db912a581f");
                dict.Add("22345", "6f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("34567", "1f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                dict.Add("21348", "2f11fcd7-40bf-4c24-8e97-602c363eb8cf");
                _addressRepositoryMock.Setup(i => i.GetZipCodeGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

                var actual = await _addressesService.GetAddressesByGuidAsync(usAddressGuid);
                var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AddressLines, actual.AddressLines);
                Assert.AreEqual(expected.Place.Country.PostalCode, actual.Place.Country.PostalCode);
                Assert.AreEqual(expected.Place.Country.PostalTitle, actual.Place.Country.PostalTitle);
                Assert.AreEqual(expected.Place.Country.CarrierRoute, actual.Place.Country.CarrierRoute);
                Assert.AreEqual(expected.Place.Country.Code, actual.Place.Country.Code);
                Assert.AreEqual(expected.Place.Country.CorrectionDigit, actual.Place.Country.CorrectionDigit);
                Assert.AreEqual(expected.Place.Country.DeliveryPoint, actual.Place.Country.DeliveryPoint);
                Assert.AreEqual(expected.Place.Country.Locality, actual.Place.Country.Locality);
                if (expected.Place.Country.Region != null)
                {
                    Assert.AreEqual(expected.Place.Country.Region.Code, actual.Place.Country.Region.Code);
                    Assert.AreEqual(expected.Place.Country.Region.Title, actual.Place.Country.Region.Title);
                }
                if (expected.Place.Country.SubRegion != null)
                {
                    Assert.AreEqual(expected.Place.Country.SubRegion.Code, actual.Place.Country.SubRegion.Code);
                    Assert.AreEqual(expected.Place.Country.SubRegion.Title, actual.Place.Country.SubRegion.Title);
                }
                var zipCodeGuid = string.Empty;
                dict.TryGetValue(actual.Place.Country.PostalCode, out zipCodeGuid);

                Assert.AreEqual(zipCodeGuid, actual.GeographicAreas[0].Id);
            }
            #endregion GetAddressById

            private IEnumerable<Domain.Base.Entities.Address> CleanAddresses()
            {
                string[,] recordData = CleanAddressData;

                int recordCount = recordData.Length / 13;
                var results = new List<Domain.Base.Entities.Address>();
                for (int i = 0; i < recordCount; i++)
                {
                    var response = new Domain.Base.Entities.Address();
                    string key = recordData[i, 0].TrimEnd();
                    string addressId = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                    List<string> label = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd().Split(';').ToList<string>();
                    List<string> lines = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd().Split(';').ToList<string>();
                    string city = (recordData[i, 4] == null) ? null : recordData[i, 4].TrimEnd();
                    string state = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                    string zip = (recordData[i, 6] == null) ? null : recordData[i, 6].TrimEnd();
                    string country = (recordData[i, 7] == null) ? null : recordData[i, 7].TrimEnd();
                    string countryDesc = (recordData[i, 8] == null) ? null : recordData[i, 8].TrimEnd();
                    string modifier = (recordData[i, 9] == null) ? null : recordData[i, 9].TrimEnd();
                    string guid = (recordData[i, 10] == null) ? new Guid().ToString() : recordData[i, 10].TrimEnd();
                    string county = (recordData[i, 11] == null) ? null : recordData[i, 11].TrimEnd();
                    string type = recordData[i, 12];
                    string typeCode = recordData[i, 13];

                    response.Guid = guid;
                    response.AddressLines = lines;
                    response.City = city;
                    response.State = state;
                    response.PostalCode = zip;
                    response.Country = country;
                    response.County = county;
                    response.Type = type;
                    response.TypeCode = typeCode;
                    results.Add(response);
                }

                return results;
            }

        }

        [TestClass]
        public class PutAddress : GenericUserFactory
        {
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;
            private IAddressRepository _addressRepository;
            private Mock<IAddressRepository> _addressRepositoryMock;
            private ILogger _logger;
            private AddressService _addressesService;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;



            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IEnumerable<Domain.Base.Entities.Address> allAddresses;
            private List<Dtos.Addresses> addressesCollection;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Place> place;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private IEnumerable<Domain.Base.Entities.Chapter> allChapters;
            private IEnumerable<Domain.Base.Entities.County> allCounties;
            private IEnumerable<Domain.Base.Entities.ZipcodeXlat> allZipCodeXlats;
            private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;

            private const string usAddressGuid = "d44134f9-0924-45d4-8b91-be9531aa7773";
            private const string foreignAddressGuid = "d44135f9-0924-45d4-8b91-be9531aa7773";

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _logger = new Mock<ILogger>().Object;
                _addressRepositoryMock = new Mock<IAddressRepository>();
                _addressRepository = _addressRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                addressesCollection = new List<Dtos.Addresses>();

                allChapters = new TestGeographicAreaRepository().GetChapters();
                allCounties = new TestGeographicAreaRepository().GetCounties();
                allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();
                allGeographicAreaTypes = new TestGeographicAreaRepository().Get();

                place = new List<Place>()
            {
                new Place(){PlacesCountry = "FRA", PlacesDesc= "France", PlacesRegion="Normandy", PlacesSubRegion="Calvados"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Barwon South West"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Gippsland"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Greater Melbourne"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Hume"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Loddon Mallee"}
            };
                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };
                _referenceDataRepositoryMock.Setup(repo => repo.GetStateCodesAsync()).Returns(Task.FromResult(states));
                _referenceDataRepositoryMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

                // Mock the reference repository for country
                countries = new List<Domain.Base.Entities.Country>()
                 {
                    new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };
                _referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };
                _referenceDataRepositoryMock.Setup(repo => repo.Counties).Returns(counties);
                _referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(counties);

                allAddresses = new TestAddressRepository().GetAddressData().ToList();

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.Addresses
                    {
                        Id = source.Guid,
                        AddressLines = source.AddressLines,
                        Latitude = source.Latitude,
                        Longitude = source.Longitude,

                    };
                    var countryPlace = new Dtos.AddressCountry()
                    {
                        Code = Dtos.EnumProperties.IsoCode.USA,
                        Title = source.Country,
                        PostalTitle = "UNITED STATES OF AMERICA",
                        CarrierRoute = source.CarrierRoute,
                        DeliveryPoint = source.DeliveryPoint,
                        CorrectionDigit = source.CorrectionDigit,
                        Locality = source.City,
                        PostalCode = source.PostalCode,

                    };

                    var region = new Dtos.AddressRegion() { Code = source.Country + "-" + source.State };
                    var title = states.FirstOrDefault(x => x.Code == source.State);
                    if (title != null)
                        region.Title = title.Description;

                    var countyDesc = counties.FirstOrDefault(c => c.Code == source.County);
                    var subRegion = new Dtos.AddressSubRegion() { Code = source.County }; ;
                    if (countyDesc != null)
                        subRegion.Title = countyDesc.Description;

                    countryPlace.Region = region;
                    countryPlace.SubRegion = subRegion;

                    address.Place = new Dtos.AddressPlace() { Country = countryPlace };
                    addressesCollection.Add(address);
                }

                _referenceDataRepositoryMock.Setup(repo => repo.GetChaptersAsync(It.IsAny<bool>())).ReturnsAsync(allChapters);
                //_referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(allCounties);
                _referenceDataRepositoryMock.Setup(repo => repo.GetZipCodeXlatAsync(It.IsAny<bool>())).ReturnsAsync(allZipCodeXlats);

                currentUserFactory = new AddressUser();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                var viewAddressRole = new Domain.Entities.Role(1, "VIEW.ADDRESS");
                viewAddressRole.AddPermission(new Domain.Entities.Permission("VIEW.ADDRESS"));

                var updateAddressRole = new Domain.Entities.Role(2, "UPDATE.ADDRESS");
                updateAddressRole.AddPermission(new Domain.Entities.Permission("UPDATE.ADDRESS"));

                roleRepoMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    viewAddressRole,
                    updateAddressRole
                });

                _addressesService = new AddressService(adapterRegistry, _addressRepository, baseConfigurationRepository, _referenceDataRepository, currentUserFactory, roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _referenceDataRepository = null;
                _addressesService = null;
                _logger = null;
                _referenceDataRepository = null;
                _referenceDataRepositoryMock = null;
                _addressRepository = null;
                _addressRepositoryMock = null;
            }

            #region PutAddressById

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddressService_PutAddressByGuid_ArgumentNullException()
            {
                await _addressesService.PutAddressesAsync("", new Dtos.Addresses());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AddressService_PutAddressById_InvalidID()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.UpdateAsync(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).Throws<KeyNotFoundException>();

                var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
                await _addressesService.PutAddressesAsync("invalid", expected);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddressService_PutAddressById_InvalidOperationException()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.UpdateAsync(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).Throws<ArgumentNullException>();
                await _addressesService.PutAddressesAsync(usAddressGuid, new Dtos.Addresses());
            }

            [TestMethod]
            public async Task AddressService_PutAddressById()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.UpdateAsync(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).ReturnsAsync(address);

                var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
                var actual = await _addressesService.PutAddressesAsync(usAddressGuid, expected);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AddressLines, actual.AddressLines);
                Assert.AreEqual(expected.Place.Country.PostalCode, actual.Place.Country.PostalCode);
                Assert.AreEqual(expected.Place.Country.PostalTitle, actual.Place.Country.PostalTitle);
                Assert.AreEqual(expected.Place.Country.CarrierRoute, actual.Place.Country.CarrierRoute);
                Assert.AreEqual(expected.Place.Country.Code, actual.Place.Country.Code);
                Assert.AreEqual(expected.Place.Country.CorrectionDigit, actual.Place.Country.CorrectionDigit);
                Assert.AreEqual(expected.Place.Country.DeliveryPoint, actual.Place.Country.DeliveryPoint);
                Assert.AreEqual(expected.Place.Country.Locality, actual.Place.Country.Locality);
                if (expected.Place.Country.Region != null)
                {
                    Assert.AreEqual(expected.Place.Country.Region.Code, actual.Place.Country.Region.Code);
                    Assert.AreEqual(expected.Place.Country.Region.Title, actual.Place.Country.Region.Title);
                }
                if (expected.Place.Country.SubRegion != null)
                {
                    Assert.AreEqual(expected.Place.Country.SubRegion.Code, actual.Place.Country.SubRegion.Code);
                    Assert.AreEqual(expected.Place.Country.SubRegion.Title, actual.Place.Country.SubRegion.Title);
                }
            }
            #endregion PutAddressById
        }

        [TestClass]
        public class PutAddress2 : GenericUserFactory
        {
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;
            private IAddressRepository _addressRepository;
            private Mock<IAddressRepository> _addressRepositoryMock;
            private ILogger _logger;
            private AddressService _addressesService;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;



            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IEnumerable<Domain.Base.Entities.Address> allAddresses;
            private List<Dtos.Addresses> addressesCollection;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Place> place;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private IEnumerable<Domain.Base.Entities.Chapter> allChapters;
            private IEnumerable<Domain.Base.Entities.County> allCounties;
            private IEnumerable<Domain.Base.Entities.ZipcodeXlat> allZipCodeXlats;
            private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;

            private const string usAddressGuid = "d44134f9-0924-45d4-8b91-be9531aa7773";
            private const string foreignAddressGuid = "d44135f9-0924-45d4-8b91-be9531aa7773";

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _logger = new Mock<ILogger>().Object;
                _addressRepositoryMock = new Mock<IAddressRepository>();
                _addressRepository = _addressRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                addressesCollection = new List<Dtos.Addresses>();

                allChapters = new TestGeographicAreaRepository().GetChapters();
                allCounties = new TestGeographicAreaRepository().GetCounties();
                allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();
                allGeographicAreaTypes = new TestGeographicAreaRepository().Get();

                place = new List<Place>()
            {
                new Place(){PlacesCountry = "FRA", PlacesDesc= "France", PlacesRegion="Normandy", PlacesSubRegion="Calvados"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Barwon South West"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Gippsland"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Greater Melbourne"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Hume"},
                new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Loddon Mallee"}
            };
                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };
                _referenceDataRepositoryMock.Setup(repo => repo.GetStateCodesAsync()).Returns(Task.FromResult(states));
                _referenceDataRepositoryMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

                // Mock the reference repository for country
                countries = new List<Domain.Base.Entities.Country>()
                 {
                    new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };
                _referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };
                _referenceDataRepositoryMock.Setup(repo => repo.Counties).Returns(counties);
                _referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(counties);

                allAddresses = new TestAddressRepository().GetAddressData().ToList();

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.Addresses
                    {
                        Id = source.Guid,
                        AddressLines = source.AddressLines,
                        Latitude = source.Latitude,
                        Longitude = source.Longitude,

                    };
                    var countryPlace = new Dtos.AddressCountry()
                    {
                        Code = Dtos.EnumProperties.IsoCode.USA,
                        Title = source.Country,
                        PostalTitle = "UNITED STATES OF AMERICA",
                        CarrierRoute = source.CarrierRoute,
                        DeliveryPoint = source.DeliveryPoint,
                        CorrectionDigit = source.CorrectionDigit,
                        Locality = source.City,
                        PostalCode = source.PostalCode,

                    };

                    var region = new Dtos.AddressRegion() { Code = source.Country + "-" + source.State };
                    var title = states.FirstOrDefault(x => x.Code == source.State);
                    if (title != null)
                        region.Title = title.Description;

                    var countyDesc = counties.FirstOrDefault(c => c.Code == source.County);
                    var subRegion = new Dtos.AddressSubRegion() { Code = source.County }; ;
                    if (countyDesc != null)
                        subRegion.Title = countyDesc.Description;

                    countryPlace.Region = region;
                    countryPlace.SubRegion = subRegion;

                    address.Place = new Dtos.AddressPlace() { Country = countryPlace };
                    addressesCollection.Add(address);
                }

                _referenceDataRepositoryMock.Setup(repo => repo.GetChaptersAsync(It.IsAny<bool>())).ReturnsAsync(allChapters);
                //_referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(allCounties);
                _referenceDataRepositoryMock.Setup(repo => repo.GetZipCodeXlatAsync(It.IsAny<bool>())).ReturnsAsync(allZipCodeXlats);

                currentUserFactory = new AddressUser();

                var viewAddressRole = new Domain.Entities.Role(1, "VIEW.ADDRESS");
                viewAddressRole.AddPermission(new Domain.Entities.Permission("VIEW.ADDRESS"));

                var updateAddressRole = new Domain.Entities.Role(2, "UPDATE.ADDRESS");
                updateAddressRole.AddPermission(new Domain.Entities.Permission("UPDATE.ADDRESS"));

                roleRepoMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    viewAddressRole,
                    updateAddressRole
                });
                _addressesService = new AddressService(adapterRegistry, _addressRepository, baseConfigurationRepository, _referenceDataRepository, currentUserFactory, roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _referenceDataRepository = null;
                _addressesService = null;
                _logger = null;
                _referenceDataRepository = null;
                _referenceDataRepositoryMock = null;
                _addressRepository = null;
                _addressRepositoryMock = null;
            }

            #region PutAddressById

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddressService_PutAddress2ByGuid_ArgumentNullException()
            {
                await _addressesService.PutAddresses2Async("", new Dtos.Addresses());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AddressService_PutAddress2ById_InvalidID()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.Update2Async(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).Throws<KeyNotFoundException>();

                var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
                await _addressesService.PutAddresses2Async("invalid", expected);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AddressService_PutAddress2ById_IntegrationApiException()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.Update2Async(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).Throws<ArgumentNullException>();
                await _addressesService.PutAddresses2Async(usAddressGuid, new Dtos.Addresses());
            }

            [TestMethod]
            public async Task AddressService_PutAddress2ById()
            {
                var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
                _addressRepositoryMock.Setup(x => x.Update2Async(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).ReturnsAsync(address);

                var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
                var actual = await _addressesService.PutAddresses2Async(usAddressGuid, expected);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AddressLines, actual.AddressLines);
                Assert.AreEqual(expected.Place.Country.PostalCode, actual.Place.Country.PostalCode);
                Assert.AreEqual(expected.Place.Country.PostalTitle, actual.Place.Country.PostalTitle);
                Assert.AreEqual(expected.Place.Country.CarrierRoute, actual.Place.Country.CarrierRoute);
                Assert.AreEqual(expected.Place.Country.Code, actual.Place.Country.Code);
                Assert.AreEqual(expected.Place.Country.CorrectionDigit, actual.Place.Country.CorrectionDigit);
                Assert.AreEqual(expected.Place.Country.DeliveryPoint, actual.Place.Country.DeliveryPoint);
                Assert.AreEqual(expected.Place.Country.Locality, actual.Place.Country.Locality);
                if (expected.Place.Country.Region != null)
                {
                    Assert.AreEqual(expected.Place.Country.Region.Code, actual.Place.Country.Region.Code);
                    Assert.AreEqual(expected.Place.Country.Region.Title, actual.Place.Country.Region.Title);
                }
                if (expected.Place.Country.SubRegion != null)
                {
                    Assert.AreEqual(expected.Place.Country.SubRegion.Code, actual.Place.Country.SubRegion.Code);
                    Assert.AreEqual(expected.Place.Country.SubRegion.Title, actual.Place.Country.SubRegion.Title);
                }
            }
            #endregion PutAddressById
        }

        //[TestClass]
        //public class PostAddress
        //{
        //    private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
        //    private IReferenceDataRepository _referenceDataRepository;
        //    private IAddressRepository _addressRepository;
        //    private Mock<IAddressRepository> _addressRepositoryMock;
        //    private ILogger _logger;
        //    private AddressService _addressesService;

        //    private Mock<IAdapterRegistry> adapterRegistryMock;
        //    private IAdapterRegistry adapterRegistry;


        //    private Mock<IRoleRepository> roleRepoMock;
        //    private IRoleRepository roleRepo;
        //    private ICurrentUserFactory currentUserFactory;


        //    private IConfigurationRepository baseConfigurationRepository;
        //    private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        //    private IEnumerable<Domain.Base.Entities.Address> allAddresses;
        //    private List<Dtos.Addresses> addressesCollection;
        //    private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
        //    private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
        //    private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Place> place;
        //    private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
        //    private IEnumerable<Domain.Base.Entities.Chapter> allChapters;
        //    private IEnumerable<Domain.Base.Entities.County> allCounties;
        //    private IEnumerable<Domain.Base.Entities.ZipcodeXlat> allZipCodeXlats;
        //    private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;

        //    private const string usAddressGuid = "d44134f9-0924-45d4-8b91-be9531aa7773";
        //    private const string foreignAddressGuid = "d44135f9-0924-45d4-8b91-be9531aa7773";

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
        //        _referenceDataRepository = _referenceDataRepositoryMock.Object;
        //        _logger = new Mock<ILogger>().Object;
        //        _addressRepositoryMock = new Mock<IAddressRepository>();
        //        _addressRepository = _addressRepositoryMock.Object;

        //        adapterRegistryMock = new Mock<IAdapterRegistry>();
        //        adapterRegistry = adapterRegistryMock.Object;
        //        roleRepoMock = new Mock<IRoleRepository>();
        //        roleRepo = roleRepoMock.Object;

        //        baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
        //        baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

        //        addressesCollection = new List<Dtos.Addresses>();

        //        allChapters = new TestGeographicAreaRepository().GetChapters();
        //        allCounties = new TestGeographicAreaRepository().GetCounties();
        //        allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();
        //        allGeographicAreaTypes = new TestGeographicAreaRepository().Get();

        //        place = new List<Place>()
        //    {
        //        new Place(){PlacesCountry = "FRA", PlacesDesc= "France", PlacesRegion="Normandy", PlacesSubRegion="Calvados"},
        //        new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Barwon South West"},
        //        new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Gippsland"},
        //        new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Greater Melbourne"},
        //        new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Hume"},
        //        new Place(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Loddon Mallee"}
        //    };
        //        // Mock the reference repository for states
        //        states = new List<State>()
        //        {
        //            new State("VA","Virginia"),
        //            new State("MD","Maryland"),
        //            new State("NY","New York"),
        //            new State("MA","Massachusetts")
        //        };
        //        _referenceDataRepositoryMock.Setup(repo => repo.GetStateCodesAsync()).Returns(Task.FromResult(states));
        //        _referenceDataRepositoryMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

        //        // Mock the reference repository for country
        //        countries = new List<Domain.Base.Entities.Country>()
        //         {
        //            new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
        //            new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
        //            new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
        //            new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
        //            new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
        //            new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
        //        };
        //        _referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

        //        // Mock the reference repository for county
        //        counties = new List<County>()
        //        {
        //            new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
        //            new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
        //            new County(Guid.NewGuid().ToString(), "NY","New York County"),
        //            new County(Guid.NewGuid().ToString(), "BOS","Boston County")
        //        };
        //        _referenceDataRepositoryMock.Setup(repo => repo.Counties).Returns(counties);
        //        _referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(counties);

        //        allAddresses = new TestAddressRepository().GetAddressData().ToList();

        //        foreach (var source in allAddresses)
        //        {
        //            var address = new Ellucian.Colleague.Dtos.Addresses
        //            {
        //                Id = source.Guid,
        //                AddressLines = source.AddressLines,
        //                Latitude = source.Latitude,
        //                Longitude = source.Longitude,

        //            };
        //            var countryPlace = new Dtos.AddressCountry()
        //            {
        //                Code = Dtos.EnumProperties.IsoCode.USA,
        //                Title = source.Country,
        //                PostalTitle = "UNITED STATES OF AMERICA",
        //                CarrierRoute = source.CarrierRoute,
        //                DeliveryPoint = source.DeliveryPoint,
        //                CorrectionDigit = source.CorrectionDigit,
        //                Locality = source.City,
        //                PostalCode = source.PostalCode,

        //            };

        //            var region = new Dtos.AddressRegion() { Code = source.Country + "-" + source.State };
        //            var title = states.FirstOrDefault(x => x.Code == source.State);
        //            if (title != null)
        //                region.Title = title.Description;

        //            var countyDesc = counties.FirstOrDefault(c => c.Code == source.County);
        //            var subRegion = new Dtos.AddressSubRegion() { Code = source.County }; ;
        //            if (countyDesc != null)
        //                subRegion.Title = countyDesc.Description;

        //            countryPlace.Region = region;
        //            countryPlace.SubRegion = subRegion;

        //            address.Place = new Dtos.AddressPlace() { Country = countryPlace };
        //            addressesCollection.Add(address);
        //        }

        //        _referenceDataRepositoryMock.Setup(repo => repo.GetChaptersAsync(It.IsAny<bool>())).ReturnsAsync(allChapters);
        //        //_referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(allCounties);
        //        _referenceDataRepositoryMock.Setup(repo => repo.GetZipCodeXlatAsync(It.IsAny<bool>())).ReturnsAsync(allZipCodeXlats);

        //        // Set up current user
        //        currentUserFactory = new CurrentUserSetup.PersonUserFactory();

        //        _addressesService = new AddressService(adapterRegistry, _addressRepository, baseConfigurationRepository, _referenceDataRepository, currentUserFactory, roleRepo, _logger);
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        _referenceDataRepository = null;
        //        _addressesService = null;
        //        _logger = null;
        //        _referenceDataRepository = null;
        //        _referenceDataRepositoryMock = null;
        //        _addressRepository = null;
        //        _addressRepositoryMock = null;
        //    }

        //    #region PostAddress

        //    [TestMethod]
        //    [ExpectedException(typeof(ArgumentNullException))]
        //    public async Task AddressService_PostAddress_ArgumentNullException()
        //    {
        //        await _addressesService.PostAddressesAsync(new Dtos.Addresses());
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task AddressService_PostAddress_InvalidID()
        //    {
        //        var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
        //        _addressRepositoryMock.Setup(x => x.UpdateAsync(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).Throws<KeyNotFoundException>();

        //        var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
        //        await _addressesService.PostAddressesAsync(expected);
        //    }


        //    [TestMethod]
        //    [ExpectedException(typeof(ArgumentNullException))]
        //    public async Task AddressService_PostAddress_InvalidOperationException()
        //    {
        //        var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
        //        _addressRepositoryMock.Setup(x => x.UpdateAsync(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).Throws<ArgumentNullException>();
        //        await _addressesService.PostAddressesAsync(new Dtos.Addresses());
        //    }

        //    [TestMethod]
        //    public async Task AddressService_PostAddress()
        //    {
        //        var address = allAddresses.FirstOrDefault(ac => ac.Guid == usAddressGuid);
        //        _addressRepositoryMock.Setup(x => x.UpdateAsync(address.AddressId, It.IsAny<Domain.Base.Entities.Address>())).ReturnsAsync(address);

        //        var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
        //        var actual = await _addressesService.PostAddressesAsync(expected);

        //        Assert.AreEqual(expected.Id, actual.Id);
        //        Assert.AreEqual(expected.AddressLines, actual.AddressLines);
        //        Assert.AreEqual(expected.Place.Country.PostalCode, actual.Place.Country.PostalCode);
        //        Assert.AreEqual(expected.Place.Country.PostalTitle, actual.Place.Country.PostalTitle);
        //        Assert.AreEqual(expected.Place.Country.CarrierRoute, actual.Place.Country.CarrierRoute);
        //        Assert.AreEqual(expected.Place.Country.Code, actual.Place.Country.Code);
        //        Assert.AreEqual(expected.Place.Country.CorrectionDigit, actual.Place.Country.CorrectionDigit);
        //        Assert.AreEqual(expected.Place.Country.DeliveryPoint, actual.Place.Country.DeliveryPoint);
        //        Assert.AreEqual(expected.Place.Country.Locality, actual.Place.Country.Locality);
        //        if (expected.Place.Country.Region != null)
        //        {
        //            Assert.AreEqual(expected.Place.Country.Region.Code, actual.Place.Country.Region.Code);
        //            Assert.AreEqual(expected.Place.Country.Region.Title, actual.Place.Country.Region.Title);
        //        }
        //        if (expected.Place.Country.SubRegion != null)
        //        {
        //            Assert.AreEqual(expected.Place.Country.SubRegion.Code, actual.Place.Country.SubRegion.Code);
        //            Assert.AreEqual(expected.Place.Country.SubRegion.Title, actual.Place.Country.SubRegion.Title);
        //        }
        //    }
        //    #endregion PostAddress
        //}

        [TestClass]
        public class DeleteAddress
        {
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;
            private IAddressRepository _addressRepository;
            private Mock<IAddressRepository> _addressRepositoryMock;
            private ILogger _logger;
            private AddressService _addressesService;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;


            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private const string usAddressGuid = "d44134f9-0924-45d4-8b91-be9531aa7773";
            private const string foreignAddressGuid = "d44135f9-0924-45d4-8b91-be9531aa7773";

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _logger = new Mock<ILogger>().Object;
                _addressRepositoryMock = new Mock<IAddressRepository>();
                _addressRepository = _addressRepositoryMock.Object;


                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                _addressesService = new AddressService(adapterRegistry, _addressRepository, baseConfigurationRepository, _referenceDataRepository, currentUserFactory, roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _referenceDataRepository = null;
                _addressesService = null;
                _logger = null;
                _referenceDataRepository = null;
                _referenceDataRepositoryMock = null;
                _addressRepository = null;
                _addressRepositoryMock = null;
            }

            #region DeleteAddress

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddressService_DeleteAddress_ArgumentNullException()
            {
                await _addressesService.DeleteAddressesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AddressService_DeleteAddress_InvalidID()
            {
                _addressRepositoryMock.Setup(x => x.DeleteAsync("invalidId")).Throws<KeyNotFoundException>();
                await _addressesService.DeleteAddressesAsync("invalidId");
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddressService_DeleteAddress_InvalidOperationException()
            {
                _addressRepositoryMock.Setup(x => x.DeleteAsync(null)).Throws<ArgumentNullException>();
                await _addressesService.DeleteAddressesAsync(null);
            }

            [TestMethod]
            public async Task AddressService_DeleteAddress()
            {
                _addressRepositoryMock.Setup(x => x.DeleteAsync(usAddressGuid)).Returns(Task.FromResult(new TaskStatus()));
                await _addressesService.DeleteAddressesAsync(usAddressGuid);
            }
            #endregion DeleteAddress
        }

        [TestClass]
        public class QueryAddress
        {
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;
            private IAddressRepository _addressRepository;
            private Mock<IAddressRepository> _addressRepositoryMock;
            private ILogger _logger;
            private AddressService _addressesService;

            private Dtos.Base.AddressQueryCriteria addressQueryCriteria;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;


            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;


            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private const string usAddressGuid = "d44134f9-0924-45d4-8b91-be9531aa7773";
            private const string foreignAddressGuid = "d44135f9-0924-45d4-8b91-be9531aa7773";

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _logger = new Mock<ILogger>().Object;
                _addressRepositoryMock = new Mock<IAddressRepository>();
                _addressRepository = _addressRepositoryMock.Object;


                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                roleRepoMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {

                });

                addressQueryCriteria = new Dtos.Base.AddressQueryCriteria()
                {
                    PersonIds = new List<string>() { "00000", "00000" },
                    AddressIds = new List<string>() { "0012355", "0012356" }
                };

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                _addressesService = new AddressService(adapterRegistry, _addressRepository, baseConfigurationRepository, _referenceDataRepository, currentUserFactory, roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _referenceDataRepository = null;
                _addressesService = null;
                _logger = null;
                _referenceDataRepository = null;
                _referenceDataRepositoryMock = null;
                _addressRepository = null;
                _addressRepositoryMock = null;
                addressQueryCriteria = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AddressService_QueryAddressPermission_PermissionException()
            {
                await _addressesService.QueryAddressPermissionAsync(addressQueryCriteria.PersonIds);
            }
        }
    }
}