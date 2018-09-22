// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Net.Http.Headers;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos;
using System;
using System.Net;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Newtonsoft.Json.Linq;
using CredentialType = Ellucian.Colleague.Dtos.EnumProperties.CredentialType;
using SocialMediaTypeCategory = Ellucian.Colleague.Dtos.SocialMediaTypeCategory;
using Ellucian.Web.Http.Models;
using System.Data;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{

    #region Organizations Get V6 and Delete V6 Controller Tests
    [TestClass]
    public class OrganizationsGetV6ControllerTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IFacilitiesService> _facilitiesServiceMock;
        private IFacilitiesService _facilitiesService;
        private Mock<IPersonService> _personServiceMock;
        private IPersonService _personService;
        private ILogger _logger;
        private IEducationalInstitutionsService _educationalInstitutionsService;
        private Mock<IEducationalInstitutionsService> _educationalInstitutionsServiceMock;

        private OrganizationsController _organizationsController;
        private IEnumerable<Department> _allDepartmentEntities;
        private List<Dtos.Organization2> _allOrganization2Dtos;
        private Tuple<IEnumerable<Dtos.Organization2>, int> _allOrganization2DtosTuple;
        List<PersonAddressDtoProperty> _addressesCollection;

        private const string PersonGuid = "1a507924-f207-460a-8c1d-1854ebe80566";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _facilitiesServiceMock = new Mock<IFacilitiesService>();
            _facilitiesService = _facilitiesServiceMock.Object;
            
            _personServiceMock = new Mock<IPersonService>();
            _personService = _personServiceMock.Object;

            _educationalInstitutionsServiceMock = new Mock<IEducationalInstitutionsService>();
            _educationalInstitutionsService = _educationalInstitutionsServiceMock.Object;

            _logger = new Mock<ILogger>().Object;
            _allDepartmentEntities = new TestDepartmentRepository().Get();
            _allOrganization2Dtos = new List<Dtos.Organization2>();
            _addressesCollection = new List<PersonAddressDtoProperty>();
          

            // Mock the reference repository for county
            var counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };

            // Mock the reference repository for states
            var states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };

              // Mock the reference repository for country
            var countries = new List<Domain.Base.Entities.Country>()
                 {
                    new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };

            var allAddresses = new TestAddressRepository().GetAddressData().ToList();
            var source = allAddresses[0];
           
                var address = new PersonAddressDtoProperty
                {
                   address = new PersonAddress(){
                        Id = source.Guid,
                        AddressLines = source.AddressLines,
                        Latitude = source.Latitude,
                        Longitude = source.Longitude
                    }
                    
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

                address.address.Place = new Dtos.AddressPlace() { Country = countryPlace };
                _addressesCollection.Add(address);
            

            var organization1 = new Dtos.Organization2
            {
                Id = PersonGuid,
                Title = "Acme Corporation",
                Addresses = new List<PersonAddressDtoProperty>() {_addressesCollection[0]},
                Credentials = new List<CredentialDtoProperty>()
                {
                    new CredentialDtoProperty() {Value = "000425", Type = CredentialType.ColleaguePersonId}
                },
                EmailAddresses = new List<PersonEmailDtoProperty>()
                {
                    new PersonEmailDtoProperty() { Address = "admissions@AcmeUniversity.com"}
                },
                Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty(){ Number = "999-999-9999"}
                },
                Roles = new List<OrganizationRoleDtoProperty>()
                {
                    new OrganizationRoleDtoProperty() { Type = OrganizationRoleType.Vendor }
                },
                SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "", 
                        Type = new PersonSocialMediaType() { Category = SocialMediaTypeCategory.facebook},
                        Preference = PersonPreference.Primary
                    },
                }
            };

            _allOrganization2Dtos.Add(organization1);
            _allOrganization2DtosTuple = new Tuple<IEnumerable<Organization2>, int>(_allOrganization2Dtos, _allOrganization2Dtos.Count);
            _organizationsController = new OrganizationsController(_adapterRegistry,
                _facilitiesService, _educationalInstitutionsService, _personService,
                _logger) { Request = new HttpRequestMessage() };
            _organizationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            _organizationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _organizationsController = null;
            _allOrganization2Dtos = null;
            _allDepartmentEntities = null;
            _facilitiesService = null;
            _personService = null;
            _adapterRegistry = null;
            _facilitiesServiceMock = null;
            _adapterRegistryMock = null;
            _logger = null;
        }

        [TestMethod]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_VendorRole()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>()))
                .ReturnsAsync(_allOrganization2DtosTuple);

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "vendor", "colleaguePersonId", "1");

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Organization2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Organization2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Organization2>;
            var count = results.Count();
            Assert.AreEqual(_allOrganization2Dtos.Count, count);            
        }

        [TestMethod]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_PartnerRole()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_allOrganization2DtosTuple);

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "partner", "colleaguePersonId", "1");

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Organization2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Organization2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Organization2>;
            var count = results.Count();
            Assert.AreEqual(_allOrganization2Dtos.Count, count);
        }

        [TestMethod]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_AffiliateRole()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_allOrganization2DtosTuple);

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "affiliate", "colleaguePersonId", "1");

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Organization2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Organization2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Organization2>;
            var count = results.Count();
            Assert.AreEqual(_allOrganization2Dtos.Count, count);
        }

        [TestMethod]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_ConstituentRole()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_allOrganization2DtosTuple);

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "constituent", "colleaguePersonId", "1");

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Organization2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Organization2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.Organization2>;
            var count = results.Count();
            Assert.AreEqual(_allOrganization2Dtos.Count, count);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_SSN()
        {           
            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "vendor", "SSN", "1");            
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_CredTypeValueNull()
        {
            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "vendor", "colleaguePersonId", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_CredTypeNull()
        {
            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "vendor", "", "123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_InvalidRole()
        {
            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "invalid", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_PermissionsException()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new PermissionsException());

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_ArgumentException()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException());

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_RepositoryException()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new RepositoryException());

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_IntegrationApiException()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new IntegrationApiException());

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "", "", "");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizations2Async_ValidateFields_Exception()
        {
            _personServiceMock.Setup(x => x.GetOrganizations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var actuals = await _organizationsController.GetOrganizations2Async(It.IsAny<Paging>(), "", "", "");
        }

        [TestMethod]
        public async Task OrganizationsController_GetOrganizationByGuid2Async_ValidateFields()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
            _personServiceMock.Setup(x => x.GetOrganization2Async(expected.Id)).ReturnsAsync(expected);

            Debug.Assert(expected != null, "expected != null");
            var actual = (await _organizationsController.GetOrganizationByGuid2Async(expected.Id));

            Assert.AreEqual(expected.Id, actual.Id, "Guid");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            var expectedAddress = expected.Addresses.ToList()[0];
            var actualAddress = actual.Addresses.ToList()[0];
            Assert.AreEqual(expectedAddress.address.Id, actualAddress.address.Id);
            Assert.AreEqual(expectedAddress.address.AddressLines[0], actualAddress.address.AddressLines[0]);
            var expectedPlace = expectedAddress.address.Place;
            var actualPlace = actualAddress.address.Place;
            Assert.AreEqual(expectedPlace.Country.Code, actualPlace.Country.Code);
            Assert.AreEqual(expectedPlace.Country.CarrierRoute, actualPlace.Country.CarrierRoute);
            Assert.AreEqual(expectedPlace.Country.CorrectionDigit, actualPlace.Country.CorrectionDigit);
            Assert.AreEqual(expectedPlace.Country.DeliveryPoint, actualPlace.Country.DeliveryPoint);
            Assert.AreEqual(expectedPlace.Country.Locality, actualPlace.Country.Locality);
            Assert.AreEqual(expectedPlace.Country.PostalCode, actualPlace.Country.PostalCode);
            Assert.AreEqual(expectedPlace.Country.PostalTitle, actualPlace.Country.PostalTitle);
            Assert.AreEqual(expectedPlace.Country.Region.Code, actualPlace.Country.Region.Code);
            Assert.AreEqual(expectedPlace.Country.Region.Title, actualPlace.Country.Region.Title);
            Assert.AreEqual(expectedPlace.Country.SubRegion.Code, actualPlace.Country.SubRegion.Code);
            Assert.AreEqual(expectedPlace.Country.SubRegion.Title, actualPlace.Country.SubRegion.Title);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_GetOrganizationByGuid2Async_Exception()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
            _personServiceMock.Setup(x => x.GetOrganization2Async(expected.Id)).Throws<Exception>();

            Debug.Assert(expected != null, "expected != null");
            await _organizationsController.GetOrganizationByGuid2Async(expected.Id);
        }

      

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationsController_DeleteThrowsIntAppiExc()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
            await _organizationsController.DeleteOrganizationByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_GetOrganizationByGuid2_ArgumentException()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
            _personServiceMock.Setup(x => x.GetOrganization2Async(expected.Id)).Throws<ArgumentException>();
            await _organizationsController.GetOrganizationByGuid2Async(PersonGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_GetOrganizationByGuid2_RepositoryException()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
            _personServiceMock.Setup(x => x.GetOrganization2Async(expected.Id)).Throws<RepositoryException>();
            await _organizationsController.GetOrganizationByGuid2Async(PersonGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_GetOrganizationByGuid2_IntegrationApiException()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
           _personServiceMock.Setup(x => x.GetOrganization2Async(expected.Id)).Throws<IntegrationApiException>();
            await _organizationsController.GetOrganizationByGuid2Async(PersonGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_GetOrganizationByGuid2_ConfigurationException()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
           _personServiceMock.Setup(x => x.GetOrganization2Async(expected.Id)).Throws<ConfigurationException>();
            await _organizationsController.GetOrganizationByGuid2Async(PersonGuid);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_GetOrganizationByGuid2_PermissionsException()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
            _personServiceMock.Setup(x => x.GetOrganization2Async(expected.Id)).Throws<PermissionsException>();
            await _organizationsController.GetOrganizationByGuid2Async(PersonGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_GetOrganizationByGuid2_KeyNotFoundException()
        {
            var expected = _allOrganization2Dtos.FirstOrDefault();
            _personServiceMock.Setup(x => x.GetOrganization2Async(expected.Id)).Throws<KeyNotFoundException>();
            await _organizationsController.GetOrganizationByGuid2Async(PersonGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_GetOrganizationByGuid2_EmptyId()
        {
            await _organizationsController.GetOrganizationByGuid2Async("");
        }
    }
    #endregion

    #region Organizations Put V6 Controller Test

    [TestClass]
    public class OrganizationsPutV6ControllerTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IFacilitiesService> _facilitiesServiceMock;
        private IFacilitiesService _facilitiesService;
        private Mock<IPersonService> _personServiceMock;
        private IPersonService _personService;
        private ILogger _logger;
        private IEducationalInstitutionsService _educationalInstitutionsService;
        private Mock<IEducationalInstitutionsService> _educationalInstitutionsServiceMock;

        private OrganizationsController _organizationsController;
        private IEnumerable<Department> _allDepartmentEntities;
        private List<Dtos.Organization2> _allOrganization2Dtos;
        List<PersonAddressDtoProperty> _addressesCollection;

        private const string PersonGuid = "1a507924-f207-460a-8c1d-1854ebe80566";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _facilitiesServiceMock = new Mock<IFacilitiesService>();
            _facilitiesService = _facilitiesServiceMock.Object;

            _personServiceMock = new Mock<IPersonService>();
            _personService = _personServiceMock.Object;

            _educationalInstitutionsServiceMock = new Mock<IEducationalInstitutionsService>();
            _educationalInstitutionsService = _educationalInstitutionsServiceMock.Object;

            _logger = new Mock<ILogger>().Object;
            _allDepartmentEntities = new TestDepartmentRepository().Get();
            _allOrganization2Dtos = new List<Dtos.Organization2>();
            _addressesCollection = new List<PersonAddressDtoProperty>();


            // Mock the reference repository for county
            var counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };

            // Mock the reference repository for states
            var states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };

            // Mock the reference repository for country
            var countries = new List<Domain.Base.Entities.Country>()
                 {
                    new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };

            var allAddresses = new TestAddressRepository().GetAddressData().ToList();
            var source = allAddresses[0];

            var address = new PersonAddressDtoProperty
            {
                address = new PersonAddress()
                {
                    Id = source.Guid,
                    AddressLines = source.AddressLines,
                    Latitude = source.Latitude,
                    Longitude = source.Longitude
                }

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

            address.address.Place = new Dtos.AddressPlace() { Country = countryPlace };
            _addressesCollection.Add(address);


            var organization1 = new Dtos.Organization2
            {
                Id = PersonGuid,
                Title = "Acme Corporation",
                Addresses = new List<PersonAddressDtoProperty>() { _addressesCollection[0] },
                Credentials = new List<CredentialDtoProperty>()
                {
                    new CredentialDtoProperty() {Value = "000425", Type = CredentialType.ColleaguePersonId}
                },
                EmailAddresses = new List<PersonEmailDtoProperty>()
                {
                    new PersonEmailDtoProperty() { Address = "admissions@AcmeUniversity.com", Type = new PersonEmailTypeDtoProperty(){EmailType = EmailTypeList.Business}}
                },
                Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty(){ Number = "999-999-9999"}
                },
                Roles = new List<OrganizationRoleDtoProperty>()
                {
                    new OrganizationRoleDtoProperty() { Type = OrganizationRoleType.Vendor }
                },
                SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "", 
                        Type = new PersonSocialMediaType() { Category = SocialMediaTypeCategory.facebook},
                        Preference = PersonPreference.Primary
                    },
                }
            };

            _allOrganization2Dtos.Add(organization1);
            _organizationsController = new OrganizationsController(_adapterRegistry,
                _facilitiesService, _educationalInstitutionsService, _personService,
                _logger) { Request = new HttpRequestMessage() };
            _organizationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            _organizationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _organizationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_allOrganization2Dtos[0]));

            _personServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _organizationsController = null;
            _allOrganization2Dtos = null;
            _allDepartmentEntities = null;
            _facilitiesService = null;
            _personService = null;
            _adapterRegistry = null;
            _facilitiesServiceMock = null;
            _adapterRegistryMock = null;
            _logger = null;
        }

        [TestMethod]
        public async Task PutOrganization()
        {
            var org = _allOrganization2Dtos[0];
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync(It.IsAny<Organization2>())).ReturnsAsync(org);

            var organization = await  _organizationsController.PutOrganizationAsync(PersonGuid, org);
            Assert.IsTrue(organization != null);
        }

        [TestMethod]
        public async Task PutOrganizationPermissionsException()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            var org = _allOrganization2Dtos[0];
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync(It.IsAny<Organization2>())).Throws(new PermissionsException());
            try
            {
                await  _organizationsController.PutOrganizationAsync(PersonGuid, org);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
        }

        [TestMethod]
        public async Task PutOrganizationException()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync( _allOrganization2Dtos[0])).Throws(new Exception());
            try
            {
                await  _organizationsController.PutOrganizationAsync(PersonGuid,  _allOrganization2Dtos[0]);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PutOrganizationNullGuidException()
        {
            await  _organizationsController.PutOrganizationAsync(null,  _allOrganization2Dtos[0]);
        }



        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PutOrganizationNullPersonGuidException()
        {
            var org = _allOrganization2Dtos[0];
            org.Id = null;
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);

            await  _organizationsController.PutOrganizationAsync(null, org);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PutOrganizationGuidMismatchException()
        {
            var org = _allOrganization2Dtos[0];
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);

            await  _organizationsController.PutOrganizationAsync("123", org);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_ArgumentException()
        {
            var org = _allOrganization2Dtos[0];
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync(It.IsAny<Organization2>())).Throws<ArgumentException>();
            await  _organizationsController.PutOrganizationAsync(PersonGuid,  _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_RepositoryException()
        {
            var org = _allOrganization2Dtos[0];
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync(It.IsAny<Organization2>())).Throws<RepositoryException>();
            await  _organizationsController.PutOrganizationAsync(PersonGuid, org);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_IntegrationApiException()
        {
            var org = _allOrganization2Dtos[0];
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync(It.IsAny<Organization2>())).Throws<IntegrationApiException>();
            await  _organizationsController.PutOrganizationAsync(PersonGuid,  _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_ConfigurationException()
        {
            var org = _allOrganization2Dtos[0];
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync(It.IsAny<Organization2>())).Throws<ConfigurationException>();
            await  _organizationsController.PutOrganizationAsync(PersonGuid,  _allOrganization2Dtos[0]);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_NilGuid()
        {
            await  _organizationsController.PutOrganizationAsync(Guid.Empty.ToString(),  _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_NilGuidInBody()
        {
            _allOrganization2Dtos[0].Id = Guid.Empty.ToString();
            await _organizationsController.PutOrganizationAsync(new Guid().ToString(), _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_NilPersonGuid()
        {
             _allOrganization2Dtos[0].Id = string.Empty;
            await  _organizationsController.PutOrganizationAsync(new Guid().ToString(),  _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_KeyNotFoundException()
        {
            var org = _allOrganization2Dtos[0];
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync(It.IsAny<Organization2>())).Throws<KeyNotFoundException>();
            await _organizationsController.PutOrganizationAsync(PersonGuid, _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_EmptyTitle()
        {
            var org = _allOrganization2Dtos[0];
            org.Title = string.Empty;
            _organizationsController.Request.Properties.Remove("PartialInputJsonObject");
            _organizationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(org));
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            _personServiceMock.Setup(s => s.UpdateOrganizationAsync(It.IsAny<Organization2>())).ReturnsAsync(org);
            await _organizationsController.PutOrganizationAsync(PersonGuid, org);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_Empty_RolesArray()
        {
            var org = _allOrganization2Dtos[0];
            org.Roles = new List<OrganizationRoleDtoProperty>();
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            await _organizationsController.PutOrganizationAsync(PersonGuid, _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_Empty_CredentialsArray()
        {
            var org = _allOrganization2Dtos[0];
            org.Credentials = new List<CredentialDtoProperty>();
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            await _organizationsController.PutOrganizationAsync(PersonGuid, _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task OrganizationController_PutOrganization_Empty_Roles_NotSet()
        {
            var org = _allOrganization2Dtos[0];
            org.Roles = new List<OrganizationRoleDtoProperty>() { new OrganizationRoleDtoProperty() { Type = OrganizationRoleType.NotSet } };
            _personServiceMock.Setup(s => s.GetOrganization2Async(org.Id)).ReturnsAsync(org);
            await _organizationsController.PutOrganizationAsync(PersonGuid, _allOrganization2Dtos[0]);
        }
    }

    #endregion 

    #region Organizations Post V6 Controller Test

    [TestClass]
    public class OrganizationsPostV6ControllerTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IFacilitiesService> _facilitiesServiceMock;
        private IFacilitiesService _facilitiesService;
        private Mock<IPersonService> _personServiceMock;
        private IPersonService _personService;
        private ILogger _logger;
        private IEducationalInstitutionsService _educationalInstitutionsService;
        private Mock<IEducationalInstitutionsService> _educationalInstitutionsServiceMock;

        private OrganizationsController _organizationsController;
       
        private List<Dtos.Organization2> _allOrganization2Dtos;
        List<PersonAddressDtoProperty> _addressesCollection;

        private const string PersonGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
        private const string NullGuid = "00000000-0000-0000-0000-000000000000";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _facilitiesServiceMock = new Mock<IFacilitiesService>();
            _facilitiesService = _facilitiesServiceMock.Object;

            _personServiceMock = new Mock<IPersonService>();
            _personService = _personServiceMock.Object;

            _educationalInstitutionsServiceMock = new Mock<IEducationalInstitutionsService>();
            _educationalInstitutionsService = _educationalInstitutionsServiceMock.Object;

            _logger = new Mock<ILogger>().Object;
            
            _allOrganization2Dtos = new List<Dtos.Organization2>();
            _addressesCollection = new List<PersonAddressDtoProperty>();


            // Mock the reference repository for county
            var counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };

            // Mock the reference repository for states
            var states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };

            // Mock the reference repository for country
            var countries = new List<Domain.Base.Entities.Country>()
                 {
                    new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };

            var allAddresses = new TestAddressRepository().GetAddressData().ToList();
            var source = allAddresses[0];

            var address = new PersonAddressDtoProperty
            {
                address = new PersonAddress()
                {
                    Id = source.Guid,
                    AddressLines = source.AddressLines,
                    Latitude = source.Latitude,
                    Longitude = source.Longitude
                }

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

            address.address.Place = new Dtos.AddressPlace() { Country = countryPlace };
            _addressesCollection.Add(address);


            var organization1 = new Dtos.Organization2
            {
                Id = NullGuid,
                Title = "Acme Corporation",
                Addresses = new List<PersonAddressDtoProperty>() { _addressesCollection[0] },
                Credentials = new List<CredentialDtoProperty>()
                {
                    new CredentialDtoProperty() {Value = "9827398127", Type = CredentialType.ElevateID}
                },
                EmailAddresses = new List<PersonEmailDtoProperty>()
                {
                    new PersonEmailDtoProperty() { Address = "admissions@AcmeUniversity.com", Type = new PersonEmailTypeDtoProperty(){EmailType = EmailTypeList.Business}}
                },
                Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty(){ Number = "999-999-9999"}
                },
                Roles = new List<OrganizationRoleDtoProperty>()
                {
                    new OrganizationRoleDtoProperty() { Type = OrganizationRoleType.Vendor }
                },
                SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "", 
                        Type = new PersonSocialMediaType() { Category = SocialMediaTypeCategory.facebook},
                        Preference = PersonPreference.Primary
                    },
                }
            };

            _allOrganization2Dtos.Add(organization1);


            var organization2 = new Dtos.Organization2
            {
                Id = PersonGuid,
                Title = "Acme Corporation",
                Addresses = new List<PersonAddressDtoProperty>() { _addressesCollection[0] },
                Credentials = new List<CredentialDtoProperty>()
                {
                    new CredentialDtoProperty() {Value = "000425", Type = CredentialType.ColleaguePersonId}
                },
                EmailAddresses = new List<PersonEmailDtoProperty>()
                {
                    new PersonEmailDtoProperty() { Address = "admissions@AcmeUniversity.com", Type = new PersonEmailTypeDtoProperty(){EmailType = EmailTypeList.Business}}
                },
                Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty(){ Number = "999-999-9999"}
                },
                Roles = new List<OrganizationRoleDtoProperty>()
                {
                    new OrganizationRoleDtoProperty() { Type = OrganizationRoleType.Vendor }
                },
                SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "",
                        Type = new PersonSocialMediaType() { Category = SocialMediaTypeCategory.facebook},
                        Preference = PersonPreference.Primary
                    },
                }
            };

            _allOrganization2Dtos.Add(organization2);


            var organization3 = new Dtos.Organization2
            {
                Id = NullGuid,
                Title = "Acme Corporation",
                Addresses = new List<PersonAddressDtoProperty>() { _addressesCollection[0] },
                Credentials = new List<CredentialDtoProperty>()
                {
                    new CredentialDtoProperty() {Value = "000425", Type = CredentialType.ColleaguePersonId}
                },
                EmailAddresses = new List<PersonEmailDtoProperty>()
                {
                    new PersonEmailDtoProperty() { Address = "admissions@AcmeUniversity.com", Type = new PersonEmailTypeDtoProperty(){EmailType = EmailTypeList.Business}}
                },
                Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty(){ Number = "999-999-9999"}
                },
                Roles = new List<OrganizationRoleDtoProperty>()
                {
                    new OrganizationRoleDtoProperty() { Type = OrganizationRoleType.Vendor }
                },
                SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "",
                        Type = new PersonSocialMediaType() { Category = SocialMediaTypeCategory.facebook},
                        Preference = PersonPreference.Primary
                    },
                }
            };

            _allOrganization2Dtos.Add(organization3);

            _organizationsController = new OrganizationsController(_adapterRegistry,
                _facilitiesService, _educationalInstitutionsService, _personService,
                _logger) { Request = new HttpRequestMessage() };
            _organizationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            _organizationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _organizationsController = null;
            _allOrganization2Dtos = null;
            _facilitiesService = null;
            _personService = null;
            _adapterRegistry = null;
            _facilitiesServiceMock = null;
            _adapterRegistryMock = null;
            _logger = null;
        }

        [TestMethod]
        public async Task PostOrganization()
        {
            _personServiceMock.Setup(s => s.CreateOrganizationAsync(_allOrganization2Dtos[0])).ReturnsAsync(_allOrganization2Dtos[0]);
           
            var organization = await  _organizationsController.PostOrganizationAsync( _allOrganization2Dtos[0]);
            Assert.IsTrue(organization != null);
        }

        [TestMethod]
        public async Task PostOrganizationPermissionsException()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            _personServiceMock.Setup(s => s.CreateOrganizationAsync(_allOrganization2Dtos[0])).Throws(new PermissionsException());
            try
            {
                await  _organizationsController.PostOrganizationAsync( _allOrganization2Dtos[0]);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationArgumentException()
        {
            _personServiceMock.Setup(s => s.CreateOrganizationAsync( _allOrganization2Dtos[0])).Throws(new ArgumentException());
            await  _organizationsController.PostOrganizationAsync( _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationRepositoryException()
        {
            _personServiceMock.Setup(s => s.CreateOrganizationAsync( _allOrganization2Dtos[0])).Throws(new RepositoryException());
            await  _organizationsController.PostOrganizationAsync( _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationIntegrationApiException()
        {
            _personServiceMock.Setup(s => s.CreateOrganizationAsync( _allOrganization2Dtos[0])).Throws(new IntegrationApiException());
            await  _organizationsController.PostOrganizationAsync( _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationConfigurationException()
        {
            _personServiceMock.Setup(s => s.CreateOrganizationAsync( _allOrganization2Dtos[0])).Throws(new ConfigurationException());
            await  _organizationsController.PostOrganizationAsync( _allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationNullException()
        {

            _personServiceMock.Setup(s => s.CreateOrganizationAsync( _allOrganization2Dtos[0])).Throws(new ConfigurationException());
            await  _organizationsController.PostOrganizationAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationNullIdException()
        {
             _allOrganization2Dtos[0].Id = null;
            _personServiceMock.Setup(s => s.CreateOrganizationAsync( _allOrganization2Dtos[0])).Throws(new ConfigurationException());

            await  _organizationsController.PostOrganizationAsync( _allOrganization2Dtos[0]);
        }
        [TestMethod]
        public async Task PostOrganizationException()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            _personServiceMock.Setup(s => s.CreateOrganizationAsync( _allOrganization2Dtos[0])).Throws(new Exception());
            try
            {
                await  _organizationsController.PostOrganizationAsync( _allOrganization2Dtos[0]);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task Organization_Null_Institution()
        {
            _educationalInstitutionsServiceMock.Setup(s => s.GetEducationalInstitutionByGuidAsync("1234", It.IsAny<bool>()))
                .ReturnsAsync(new EducationalInstitution());
            await _organizationsController.GetOrganizationByGuid2Async("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task Organization_ArgumentException()
        {
            _educationalInstitutionsServiceMock.Setup(s => s.GetEducationalInstitutionByGuidAsync("1234", It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            await _organizationsController.GetOrganizationByGuid2Async("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationKeyNotFoundException()
        {
            _personServiceMock.Setup(s => s.CreateOrganizationAsync(_allOrganization2Dtos[0])).Throws(new KeyNotFoundException());
            await _organizationsController.PostOrganizationAsync(_allOrganization2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationEmptyTitle()
        {
            _allOrganization2Dtos[0].Title = string.Empty;
            _personServiceMock.Setup(s => s.CreateOrganizationAsync(_allOrganization2Dtos[0])).ReturnsAsync(_allOrganization2Dtos[0]);

            var organization = await _organizationsController.PostOrganizationAsync(_allOrganization2Dtos[0]);
            
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationV6NonNullGuid()
        {
            var org = _allOrganization2Dtos[1];
            Assert.AreNotEqual(Guid.Empty.ToString(), org.Id);
            await _organizationsController.PostOrganizationAsync(org);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PostOrganizationV6WithColleaguePersonId()
        {
            var org = _allOrganization2Dtos[2];
            await _organizationsController.PostOrganizationAsync(org);
        }

    }

    #endregion

}