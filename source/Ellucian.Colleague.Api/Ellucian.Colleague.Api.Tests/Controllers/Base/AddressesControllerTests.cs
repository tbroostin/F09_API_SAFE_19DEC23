// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Controllers.Base;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Models;
using Newtonsoft.Json.Linq;
using System.Web.Http.Routing;
using System.Collections;
using Ellucian.Web.Http.Filters;
using System.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class AddressesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAddressService> addressesServiceMock;
        private Mock<IAddressRepository> addressesRepositoryMock;
        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> refRepoMock;
        private Dtos.Base.AddressQueryCriteria addressQueryCriteria;
        private IReferenceDataRepository refRepo;

        private AddressesController addressesController;

        private IEnumerable<Domain.Base.Entities.Address> allAddresses;
        private List<Dtos.Addresses> addressesCollection;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Place> place;
        private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;

        private const string usAddressGuid = "d44134f9-0924-45d4-8b91-be9531aa7773";
        private const string foreignAddressGuid = "d44135f9-0924-45d4-8b91-be9531aa7773";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            addressesServiceMock = new Mock<IAddressService>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            addressesRepositoryMock = new Mock<IAddressRepository>();
            refRepoMock = new Mock<IReferenceDataRepository>();
            refRepo = refRepoMock.Object;

            addressQueryCriteria = new Dtos.Base.AddressQueryCriteria()
            {
                PersonIds = new List<string>() { "00000", "00000" },
                AddressIds = new List<string>() { "0012355", "0012356" }
            };

            addressesCollection = new List<Dtos.Addresses>();

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
            refRepoMock.Setup(repo => repo.GetStateCodesAsync()).Returns(Task.FromResult(states));

            // Mock the reference repository for country
            countries = new List<Country>()
                 {
                    new Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };
            refRepoMock.Setup(repo => repo.GetCountryCodesAsync(false)).Returns(Task.FromResult(countries));

            // Mock the reference repository for county
            counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };
            refRepoMock.Setup(repo => repo.Counties).Returns(counties);

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
                    PostalTitle = "US",
                    CarrierRoute = source.CarrierRoute,
                    DeliveryPoint = source.DeliveryPoint,
                    CorrectionDigit = source.CorrectionDigit,
                    Locality = source.City,
                    PostalCode = source.PostalCode,

                };

                var region = new Dtos.AddressRegion() { Code = source.State };
                var title = states.FirstOrDefault(x => x.Code == source.State);
                if (title != null)
                    region.Title = title.Description;

                var subRegion = new Dtos.AddressSubRegion() { Code = "", Title = "" }; ;
                countryPlace.Region = region;
                countryPlace.SubRegion = subRegion;

                address.Place = new Dtos.AddressPlace() { Country = countryPlace };
                addressesCollection.Add(address);
            }

            var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);

            addressesController = new AddressesController(adapterRegistryMock.Object,
                addressesServiceMock.Object, addressesRepositoryMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            addressesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            addressesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            addressesController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(expected));
        }

        [TestCleanup]
        public void Cleanup()
        {
            addressesController = null;
            addressesCollection = null;
            loggerMock = null;
            adapterRegistryMock = null;
            addressesRepositoryMock = null;
            addressesServiceMock = null;
        }

        #region GetAllAddresses

        [TestMethod]
        public async Task AddressesController_GetAddressesAsync()
        {

            addressesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            int Offset = 0;
            int Limit = 4;
            var AddressesTuple =
                new Tuple<IEnumerable<Dtos.Addresses>, int>(addressesCollection.Take(4), addressesCollection.Count());

            addressesServiceMock.Setup(i => i.GetAddressesAsync(Offset, Limit, true)).ReturnsAsync(AddressesTuple);

            Paging paging = new Paging(Limit, Offset);
            var actuals = await addressesController.GetAddressesAsync(paging);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Addresses> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Addresses>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Addresses>;

            Assert.IsNotNull(results);
            Assert.AreEqual(4, results.Count());

            foreach (var actual in results)
            {
                var expected = addressesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
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
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressesAsync_PermissionException()
        {
            addressesServiceMock.Setup(i => i.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            await addressesController.GetAddressesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_QueryAddressesAsync_PermissionException()
        {
            addressesServiceMock.Setup(i => i.QueryAddressPermissionAsync(It.IsAny<IEnumerable<string>>())).Throws(new PermissionsException());
            await addressesController.QueryAddressesAsync(addressQueryCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressesAsync_ArgumentException()
        {
            addressesServiceMock.Setup(i => i.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
            await addressesController.GetAddressesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressesAsync_RepositoryException()
        {
            addressesServiceMock.Setup(i => i.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
            await addressesController.GetAddressesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressesAsync_IntegrationApiException()
        {
            addressesServiceMock.Setup(i => i.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            await addressesController.GetAddressesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressesAsync_Exception()
        {
            addressesServiceMock.Setup(i => i.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            await addressesController.GetAddressesAsync(It.IsAny<Paging>());
        }


        //GET v11.1.0 / v11
        //Successful
        //GetAddresses2Async

        [TestMethod]
        public async Task AddressesController_GetAddresses2Async_Permissions()
        {
            var AddressesTuple = new Tuple<IEnumerable<Dtos.Addresses>, int>(addressesCollection.Take(4), addressesCollection.Count());
            int Offset = 0;
            int Limit = 4;
            Paging paging = new Paging(Limit, Offset);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "GetAddresses2Async" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);
            addressesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress });

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));


            addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            addressesServiceMock.Setup(i => i.GetAddresses2Async(Offset, Limit, It.IsAny<string>(), false)).ReturnsAsync(AddressesTuple);
            var actuals = await addressesController.GetAddresses2Async(paging, It.IsAny<QueryStringFilter>());

            Object filterObject;
            addressesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAddress));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdateAddress));

        }

        //GET v11.1.0 / v11
        //Exception
        //GetAddresses2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddresses2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "GetAddresses2Async" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                addressesServiceMock.Setup(i => i.GetAddresses2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), false)).ThrowsAsync(new PermissionsException());
                addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view Addresses."));
                await addressesController.GetAddresses2Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET v6
        //Successful
        //GetAddressesAsync

        [TestMethod]
        public async Task AddressesController_GetAddressesAsync_Permissions()
        {
            int Offset = 0;
            int Limit = 4;
            var AddressesTuple = new Tuple<IEnumerable<Dtos.Addresses>, int>(addressesCollection.Take(4), addressesCollection.Count());
            Paging paging = new Paging(Limit, Offset);

            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "GetAddressesAsync" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);
            addressesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress });

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            addressesServiceMock.Setup(i => i.GetAddressesAsync(Offset, Limit, false)).ReturnsAsync(AddressesTuple);
            var actuals = await addressesController.GetAddressesAsync(paging);

            Object filterObject;
            addressesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAddress));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdateAddress));

        }

        //GET v6
        //Exception
        //GetAddressesAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressesAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "GetAddressesAsync" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                addressesServiceMock.Setup(i => i.GetAddressesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view Addresses."));
                await addressesController.GetAddressesAsync(It.IsAny<Paging>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }



        #endregion

        #region GetAddressesByGuid

        [TestMethod]
        public async Task AddressesController_GetAddressByGuid_US()
        {
            addressesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(usAddressGuid)).ReturnsAsync(expected);

            var actual = await addressesController.GetAddressByGuidAsync(usAddressGuid);

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
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid_Null_ArgumentNullException()
        {
            await addressesController.GetAddressByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid_String_ArgumentNullException()
        {
            await addressesController.GetAddressByGuidAsync("");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid_PermissionsException()
        {

            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await addressesController.GetAddressByGuidAsync(usAddressGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid_ArgumentException()
        {

            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await addressesController.GetAddressByGuidAsync(usAddressGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid_RepositoryException()
        {

            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await addressesController.GetAddressByGuidAsync(usAddressGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid_KeyNotFoundException()
        {

            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await addressesController.GetAddressByGuidAsync(usAddressGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid_IntegrationApiException()
        {

            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await addressesController.GetAddressByGuidAsync(usAddressGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid_Exception()
        {

            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await addressesController.GetAddressByGuidAsync(usAddressGuid);
        }

        //GET by id v11.1.0 / v11
        //Successful
        //GetAddressByGuid2Async

        [TestMethod]
        public async Task AddressesController_GetAddressByGuid2Async_Permissions()
        {
            var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "GetAddressByGuid2Async" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);
            addressesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress });

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            addressesServiceMock.Setup(x => x.GetAddressesByGuid2Async(usAddressGuid)).ReturnsAsync(expected);
            var actual = await addressesController.GetAddressByGuid2Async(usAddressGuid);

            Object filterObject;
            addressesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAddress));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdateAddress));

        }

        //GET by id v11.1.0 / v11
        //Exception
        //GetAddressByGuid2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuid2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "GetAddressByGuid2Async" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                addressesServiceMock.Setup(x => x.GetAddressesByGuid2Async(It.IsAny<string>())).Throws<PermissionsException>();
                addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view Addresses."));
                await addressesController.GetAddressByGuid2Async(usAddressGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET by id v6
        //Successful
        //GetAddressByGuidAsync

        [TestMethod]
        public async Task AddressesController_GetAddressByGuidAsync_Permissions()
        {
            var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "GetAddressByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);
            addressesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress });

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(usAddressGuid)).ReturnsAsync(expected);
            var actual = await addressesController.GetAddressByGuidAsync(usAddressGuid);

            Object filterObject;
            addressesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAddress));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdateAddress));

        }

        //GET by id v6
        //Exception
        //GetAddressByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_GetAddressByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "GetAddressByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view Addresses."));
                await addressesController.GetAddressByGuidAsync(usAddressGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion  GetAddressesByGuid

        #region PutAddressAsync

        [TestMethod]
        public async Task AddressesController_PutAddressAsync_US()
        {
            var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
            addressesServiceMock.Setup(x => x.PutAddressesAsync(usAddressGuid, It.IsAny<Dtos.Addresses>())).ReturnsAsync(expected);
            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(usAddressGuid)).ReturnsAsync(expected);

            var actual = await addressesController.PutAddressAsync(usAddressGuid, expected);

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
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_Null_ArgumentNullException()
        {
            await addressesController.PutAddressAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_String_ArgumentNullException()
        {
            await addressesController.PutAddressAsync("", new Dtos.Addresses());
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_PermissionsException()
        {

            addressesServiceMock.Setup(x => x.PutAddressesAsync(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<PermissionsException>();
            await addressesController.PutAddressAsync(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_ArgumentException()
        {

            addressesServiceMock.Setup(x => x.PutAddressesAsync(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<ArgumentException>();
            await addressesController.PutAddressAsync(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_RepositoryException()
        {

            addressesServiceMock.Setup(x => x.PutAddressesAsync(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<RepositoryException>();
            await addressesController.PutAddressAsync(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_KeyNotFoundException()
        {

            addressesServiceMock.Setup(x => x.PutAddressesAsync(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<KeyNotFoundException>();
            await addressesController.PutAddressAsync(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_IntegrationApiException()
        {

            addressesServiceMock.Setup(x => x.PutAddressesAsync(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<IntegrationApiException>();
            await addressesController.PutAddressAsync(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_Exception()
        {

            addressesServiceMock.Setup(x => x.PutAddressesAsync(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<Exception>();
            await addressesController.PutAddressAsync(usAddressGuid, new Dtos.Addresses());
        }

        //Put v6
        //Successful
        //PutAddressAsync

        [TestMethod]
        public async Task AddressesController_PutAddressAsync_Permissions()
        {
            var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "PutAddressAsync" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);
            addressesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.UpdateAddress);

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            addressesServiceMock.Setup(x => x.PutAddressesAsync(usAddressGuid, It.IsAny<Dtos.Addresses>())).ReturnsAsync(expected);
            var actual = await addressesController.PutAddressAsync(usAddressGuid, expected);

            Object filterObject;
            addressesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdateAddress));

        }

        //Put v6
        //Exception
        //PutAddressAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddressAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "PutAddressAsync" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                addressesServiceMock.Setup(x => x.PutAddressesAsync(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<PermissionsException>();
                addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update Addresses."));
                await addressesController.PutAddressAsync(usAddressGuid, new Dtos.Addresses());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion

        #region PutAddress2Async

        [TestMethod]
        public async Task AddressesController_PutAddress2Async_US()
        {
            var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
            addressesServiceMock.Setup(x => x.PutAddresses2Async(usAddressGuid, It.IsAny<Dtos.Addresses>())).ReturnsAsync(expected);
            addressesServiceMock.Setup(x => x.GetAddressesByGuidAsync(usAddressGuid)).ReturnsAsync(expected);

            var actual = await addressesController.PutAddress2Async(usAddressGuid, expected);

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
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_Null_ArgumentNullException()
        {
            await addressesController.PutAddress2Async(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_String_ArgumentNullException()
        {
            await addressesController.PutAddress2Async("", new Dtos.Addresses());
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_PermissionsException()
        {

            addressesServiceMock.Setup(x => x.PutAddresses2Async(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<PermissionsException>();
            await addressesController.PutAddress2Async(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_ArgumentException()
        {

            addressesServiceMock.Setup(x => x.PutAddresses2Async(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<ArgumentException>();
            await addressesController.PutAddress2Async(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_RepositoryException()
        {

            addressesServiceMock.Setup(x => x.PutAddresses2Async(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<RepositoryException>();
            await addressesController.PutAddress2Async(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_KeyNotFoundException()
        {

            addressesServiceMock.Setup(x => x.PutAddresses2Async(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<KeyNotFoundException>();
            await addressesController.PutAddress2Async(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_IntegrationApiException()
        {

            addressesServiceMock.Setup(x => x.PutAddresses2Async(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<IntegrationApiException>();
            await addressesController.PutAddress2Async(usAddressGuid, new Dtos.Addresses());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_Exception()
        {

            addressesServiceMock.Setup(x => x.PutAddresses2Async(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<Exception>();
            await addressesController.PutAddress2Async(usAddressGuid, new Dtos.Addresses());
        }

        //Put v11.1.0 / v11
        //Successful
        //PutAddress2Async

        [TestMethod]
        public async Task AddressesController_PutAddress2Async_Permissions()
        {
            var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "PutAddress2Async" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);
            addressesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.UpdateAddress);

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            addressesServiceMock.Setup(x => x.PutAddresses2Async(usAddressGuid, It.IsAny<Dtos.Addresses>())).ReturnsAsync(expected);
            var actual = await addressesController.PutAddress2Async(usAddressGuid, expected);

            Object filterObject;
            addressesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdateAddress));

        }

        //Put v11.1.0 / v11
        //Exception
        //PutAddress2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PutAddress2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Addresses" },
                    { "action", "PutAddress2Async" }
                };
            HttpRoute route = new HttpRoute("addresses", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            addressesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = addressesController.ControllerContext;
            var actionDescriptor = addressesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                addressesServiceMock.Setup(x => x.PutAddresses2Async(It.IsAny<string>(), It.IsAny<Dtos.Addresses>())).Throws<PermissionsException>();
                addressesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update Addresses."));
                await addressesController.PutAddress2Async(usAddressGuid, new Dtos.Addresses());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region PostAddressAsync

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_PostAddressAsync()
        {
            await addressesController.PostAddressAsync(addressesCollection[0]);
        }

        //[TestMethod]
        //public async Task AddressesController_PostAddressAsync_US()
        //{
        //    var expected = addressesCollection.FirstOrDefault(x => x.Id == usAddressGuid);
        //    addressesServiceMock.Setup(x => x.PostAddressesAsync(expected)).ReturnsAsync(expected);

        //    var actual = await addressesController.PostAddressAsync(expected);

        //    Assert.AreEqual(expected.Id, actual.Id);
        //    Assert.AreEqual(expected.AddressLines, actual.AddressLines);
        //    Assert.AreEqual(expected.Place.Country.PostalCode, actual.Place.Country.PostalCode);
        //    Assert.AreEqual(expected.Place.Country.PostalTitle, actual.Place.Country.PostalTitle);
        //    Assert.AreEqual(expected.Place.Country.CarrierRoute, actual.Place.Country.CarrierRoute);
        //    Assert.AreEqual(expected.Place.Country.Code, actual.Place.Country.Code);
        //    Assert.AreEqual(expected.Place.Country.CorrectionDigit, actual.Place.Country.CorrectionDigit);
        //    Assert.AreEqual(expected.Place.Country.DeliveryPoint, actual.Place.Country.DeliveryPoint);
        //    Assert.AreEqual(expected.Place.Country.Locality, actual.Place.Country.Locality);
        //    if (expected.Place.Country.Region != null)
        //    {
        //        Assert.AreEqual(expected.Place.Country.Region.Code, actual.Place.Country.Region.Code);
        //        Assert.AreEqual(expected.Place.Country.Region.Title, actual.Place.Country.Region.Title);
        //    }
        //    if (expected.Place.Country.SubRegion != null)
        //    {
        //        Assert.AreEqual(expected.Place.Country.SubRegion.Code, actual.Place.Country.SubRegion.Code);
        //        Assert.AreEqual(expected.Place.Country.SubRegion.Title, actual.Place.Country.SubRegion.Title);
        //    }
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task AddressesController_PostAddressAsync_Null_ArgumentNullException()
        //{
        //    await addressesController.PostAddressAsync(null);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task AddressesController_PostAddressAsync_PermissionsException()
        //{

        //    addressesServiceMock.Setup(x => x.PostAddressesAsync(It.IsAny<Dtos.Addresses>())).Throws<PermissionsException>();
        //    await addressesController.PostAddressAsync(new Dtos.Addresses());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task AddressesController_PostAddressAsync_ArgumentException()
        //{

        //    addressesServiceMock.Setup(x => x.PostAddressesAsync(It.IsAny<Dtos.Addresses>())).Throws<ArgumentException>();
        //    await addressesController.PostAddressAsync(new Dtos.Addresses());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task AddressesController_PostAddressAsync_RepositoryException()
        //{

        //    addressesServiceMock.Setup(x => x.PostAddressesAsync(It.IsAny<Dtos.Addresses>())).Throws<RepositoryException>();
        //    await addressesController.PostAddressAsync(new Dtos.Addresses());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task AddressesController_PostAddressAsync_KeyNotFoundException()
        //{

        //    addressesServiceMock.Setup(x => x.PostAddressesAsync(It.IsAny<Dtos.Addresses>())).Throws<KeyNotFoundException>();
        //    await addressesController.PostAddressAsync(new Dtos.Addresses());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task AddressesController_PostAddressAsync_IntegrationApiException()
        //{

        //    addressesServiceMock.Setup(x => x.PostAddressesAsync(It.IsAny<Dtos.Addresses>())).Throws<IntegrationApiException>();
        //    await addressesController.PostAddressAsync(new Dtos.Addresses());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task AddressesController_PostAddressAsync_Exception()
        //{
        //    addressesServiceMock.Setup(x => x.PostAddressesAsync(It.IsAny<Dtos.Addresses>())).Throws<Exception>();
        //    await addressesController.PostAddressAsync(new Dtos.Addresses());
        //}

        #endregion

        #region DeleteAddressAsync

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_DeleteAddressAsync_Null_ArgumentNullException()
        {
            addressesServiceMock.Setup(x => x.DeleteAddressesAsync(null)).Throws<ArgumentNullException>();
            await addressesController.DeleteAddressAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_DeleteAddressAsync_String_ArgumentNullException()
        {
            addressesServiceMock.Setup(x => x.DeleteAddressesAsync("")).Throws<ArgumentNullException>();
            await addressesController.DeleteAddressAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressesController_DeleteAddressAsync_KeyNotFoundException()
        {
            addressesServiceMock.Setup(x => x.DeleteAddressesAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await addressesController.DeleteAddressAsync(usAddressGuid);
        }

        #endregion
    }
}