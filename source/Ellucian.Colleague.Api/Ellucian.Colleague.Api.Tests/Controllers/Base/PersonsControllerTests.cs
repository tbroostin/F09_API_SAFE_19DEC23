// Copyright 2014-2018 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using System.Net.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Base;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonsControllerTests
    {
        #region Get Tests HEDM v6

        [TestClass]
        public class PersonsHedmGet_6
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Ellucian.Colleague.Dtos.Person2 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person2();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.GetPerson2ByGuidNonCachedAsync(personGuid)).ReturnsAsync(personDto);
                personServiceMock.Setup(s => s.GetPerson2ByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personDto);
                var personDtoList = new List<Dtos.Person2>() { personDto };
                var personTuple = new Tuple<IEnumerable<Dtos.Person2>, int>(personDtoList, 1);
                personServiceMock.Setup(s => s.GetPerson2NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                    null, "Ricky", null, null, "Brown", null,
                    null, null, null, null, null)).ReturnsAsync(personTuple);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPerson2_Cache()
            {
                personsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var person = await personsController.GetPerson2ByGuidAsync(personGuid);
                Assert.IsTrue(person is Person2);
            }

            [TestMethod]
            public async Task GetPerson2_NoCache()
            {
                personsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var person = await personsController.GetPerson2ByGuidAsync(personGuid);
                Assert.IsTrue(person is Person2);
            }

            [TestMethod]
            public async Task GetPerson2PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson2ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPerson2ByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            public async Task GetPersonMartialStatusIsNull()
            {
                //insure that the martail status always comes null if service call returns it as empty.
                var person = await personsController.GetPerson2ByGuidAsync(personGuid);
                Assert.AreEqual(person.MaritalStatus, null);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson2ArgumentException()
            {
                personServiceMock.Setup(s => s.GetPerson2ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new ArgumentException());
                await personsController.GetPerson2ByGuidAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson2RepositoryException()
            {
                personServiceMock.Setup(s => s.GetPerson2ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new RepositoryException());
                await personsController.GetPerson2ByGuidAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson2IntegrationApiException()
            {
                personServiceMock.Setup(s => s.GetPerson2ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new IntegrationApiException());
                await personsController.GetPerson2ByGuidAsync(personGuid);
            }

            [TestMethod]
            public async Task GetPerson2NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson2ByGuidNonCachedAsync(personGuid)).Throws(new Exception());
                personServiceMock.Setup(s => s.GetPerson2ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new Exception());
                try
                {
                    await personsController.GetPerson2ByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetPerson2Filtered()
            {
                var personList = await personsController.GetPerson2Async(new Paging(1, 0), null, "Ricky", null, null,
                    "Brown", null, null, null, null, null, null);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson2Filtered_NoPaging()
            {
                var personList = await personsController.GetPerson2Async(null, null, "Ricky", null, null,
                    "Brown", null, null, null, null, null, null);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetPerson2Filtered_MissingValueArgumentException()
            {
                var personList = await personsController.GetPerson2Async(null, string.Empty, string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, string.Empty, "ssn", string.Empty, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetPerson2Filtered_MissingTypeArgumentException()
            {
                var personList = await personsController.GetPerson2Async(null, string.Empty, string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "894-99-3728", string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetPerson2Filtered_ArgumentException()
            {
                personServiceMock.Setup(s => s.GetPerson2NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                    null, null, null, null, null, null,
                    null, null, "ssn", null, null)).Throws(new ArgumentException());

                var personList = await personsController.GetPerson2Async(null, string.Empty, string.Empty, string.Empty, string.Empty,
                   string.Empty, string.Empty, string.Empty, string.Empty, "ssn", string.Empty, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetPerson2Filtered_CredentialInvalidEnumException()
            {
                var personList = await personsController.GetPerson2Async(null, string.Empty, string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, string.Empty, "tax", string.Empty, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetPerson2Filtered_CredentialNotSupportedException()
            {
                var personList = await personsController.GetPerson2Async(null, string.Empty, string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, string.Empty, "tax", string.Empty, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetPerson2Filtered_RoleInvalidEnumException()
            {
                var personList = await personsController.GetPerson2Async(null, string.Empty, string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, "stu", string.Empty, string.Empty, string.Empty);
            }

            [TestMethod]
            public async Task GetGetPersons2Filtered_PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson2NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPerson2Async(new Paging(1, 0), string.Empty, "Ricky", string.Empty, string.Empty,
                        "Brown", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            public async Task GetGetPersons2Filtered_NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson2NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new IntegrationApiException());
                try
                {
                    await personsController.GetPerson2Async(new Paging(00, 0), string.Empty, "Ricky", string.Empty, string.Empty, "Brown",
                        string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region Get Tests HEDM v8

        [TestClass]
        public class PersonsHedmGet_v8
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Ellucian.Colleague.Dtos.Person3 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;
            private Ellucian.Web.Http.Models.QueryStringFilter criteria = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter personFilterFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
            private Ellucian.Web.Http.Models.QueryStringFilter preferredNameFilter = new Web.Http.Models.QueryStringFilter("preferredName", "");

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person3();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.GetPerson3ByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personDto);
                var personDtoList = new List<Dtos.Person3>() { personDto };
                var personTuple = new Tuple<IEnumerable<Dtos.Person3>, int>(personDtoList, 1);
                personServiceMock.Setup(s => s.GetPerson3NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Dtos.Filters.PersonFilter>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(personTuple);


                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }


            //*****************************
            // New V8 tests

            [TestMethod]
            public async Task GetPerson3_title()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Title = "Mr." });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3_firstname()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { FirstName = "Ricky" });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3_middlename()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { MiddleName = "Bobby" });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }



            [TestMethod]
            public async Task GetPerson3_lastnameprefix()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { LastNamePrefix = "Van" });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3_lastname()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { LastName = "Brown" });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3_pedigree()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Pedigree = "pedigreeguid" });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3_preferredname()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { PreferredName = "Ricky Bobby Brown" });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3_role()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Role = Dtos.EnumProperties.PersonRoleType.Student });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson3_credentials_Invalid()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = Dtos.EnumProperties.CredentialType2.NotSet, Value = "00009999" } } });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson3_credentials_NoValueInvalid()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = Dtos.EnumProperties.CredentialType2.NotSet, Value = "" } } });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson3_credentials_NoTypeInvalid()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = null, Value = "00009999" } } });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
            }

            [TestMethod]
            public async Task GetPerson3_credentials()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = Dtos.EnumProperties.CredentialType2.ColleaguePersonId, Value = "00009999" } } });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3_prefabricatedfilter()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { PersonFilterFilter = "anyfilter" });
                var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3_credentials_no_sin_filter()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = Dtos.EnumProperties.CredentialType2.Sin, Value = "00009999" } } });
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                try
                {
                    var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }


            [TestMethod]
            public async Task GetPerson3_credentials_no_ssn_filter()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = Dtos.EnumProperties.CredentialType2.Ssn, Value = "00009999" } } });
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                try
                {
                    var personList = await personsController.GetPerson3Async(page, criteria, personFilterFilter, preferredNameFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);

            }

            //**************************
            [TestMethod]
            public async Task GetPerson3_Cache()
            {
                personsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var person = await personsController.GetPersonByGuid3Async(personGuid);
                Assert.IsTrue(person is Person3);
            }

            [TestMethod]
            public async Task GetPerson3_NoCache()
            {
                personsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var person = await personsController.GetPersonByGuid3Async(personGuid);
                Assert.IsTrue(person is Person3);
            }

            [TestMethod]
            public async Task GetPerson3PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson3ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPersonByGuid3Async(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            public async Task GetPersonMartialStatusIsNull()
            {
                //insure that the martail status always comes null if service call returns it as empty.
                var person = await personsController.GetPersonByGuid3Async(personGuid);
                Assert.AreEqual(person.MaritalStatus, null);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson3ArgumentException()
            {
                personServiceMock.Setup(s => s.GetPerson3ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new ArgumentException());
                await personsController.GetPersonByGuid3Async(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson3RepositoryException()
            {
                personServiceMock.Setup(s => s.GetPerson3ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new RepositoryException());
                await personsController.GetPersonByGuid3Async(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson3IntegrationApiException()
            {
                personServiceMock.Setup(s => s.GetPerson3ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new IntegrationApiException());
                await personsController.GetPersonByGuid3Async(personGuid);
            }

            [TestMethod]
            public async Task GetPerson3NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson3ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new Exception());
                try
                {
                    await personsController.GetPersonByGuid3Async(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetPerson3Filtered()
            {
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { FirstName = "Ricky", LastName = "Brown" });
                var personList = await personsController.GetPerson3Async(new Paging(1, 0), criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson3Filtered_NoPaging()
            {
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { FirstName = "Ricky", LastName = "Brown" });
                var personList = await personsController.GetPerson3Async(null, criteria, personFilterFilter, preferredNameFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task GetPerson3Filtered_MissingValueArgumentException()
            //{
            //    var filterGroupName = "criteria";
            //    personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
            //          new Dtos.Filters.PersonFilter() { FirstName = "" } );
            //    var personList = await personsController.GetPerson3Async(null, criteria, personFilterFilter, preferredNameFilter);
            //}

            [TestMethod]
            public async Task GetGetPersons3Filtered_PermissionsException()
            {

                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { FirstName = "Ricky" });
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson3NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Dtos.Filters.PersonFilter>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPerson3Async(new Paging(1, 0), criteria, personFilterFilter, preferredNameFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            public async Task GetGetPersons3Filtered_NotFoundException()
            {
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilter() { FirstName = "Ricky" });
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson3NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Dtos.Filters.PersonFilter>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new IntegrationApiException());
                try
                {
                    await personsController.GetPerson3Async(new Paging(00, 0), criteria, personFilterFilter, preferredNameFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region Get Tests HEDM v12

        [TestClass]
        public class PersonsHedmGet_v12
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Ellucian.Colleague.Dtos.Person4 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter personFilterFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person4();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.GetPerson4ByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personDto);
                var personDtoList = new List<Dtos.Person4>() { personDto };
                var personTuple = new Tuple<IEnumerable<Dtos.Person4>, int>(personDtoList, 1);
                personServiceMock.Setup(s => s.GetPerson4NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person4>(), It.IsAny<string>())).ReturnsAsync(personTuple);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPerson4ById_Cache()
            {
                personsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var person = await personsController.GetPerson4ByIdAsync(personGuid);
                Assert.IsTrue(person is Person4);
            }

            [TestMethod]
            public async Task GetPerson4ById_NoCache()
            {
                personsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var person = await personsController.GetPerson4ByIdAsync(personGuid);
                Assert.IsTrue(person is Person4);
            }

            [TestMethod]
            public async Task GetPerson4ByIdPermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson4ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPerson4ByIdAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            public async Task GetPerson4ByIdKeyNotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson4ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new KeyNotFoundException());
                try
                {
                    await personsController.GetPerson4ByIdAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4ByIdArgumentException()
            {
                personServiceMock.Setup(s => s.GetPerson4ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new ArgumentException());
                await personsController.GetPerson4ByIdAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4ByIdRepositoryException()
            {
                personServiceMock.Setup(s => s.GetPerson4ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new RepositoryException());
                await personsController.GetPerson4ByIdAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4ByIdIntegrationApiException()
            {
                personServiceMock.Setup(s => s.GetPerson4ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new IntegrationApiException());
                await personsController.GetPerson4ByIdAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4ByIdException()
            {
                personServiceMock.Setup(s => s.GetPerson4ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new Exception());
                await personsController.GetPerson4ByIdAsync(personGuid);
            }

            [TestMethod]
            public async Task GetPerson4_Cache()
            {
                personsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var personList = await personsController.GetPerson4Async(null, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_NoCache()
            {
                personsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var personList = await personsController.GetPerson4Async(null, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_PersonFilter()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "personFilter";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilterFilter() { personFilterId = personGuid });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);

            }

            [TestMethod]
            public async Task GetPerson4_title()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { Title = "Mr." } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_firstname()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { FirstName = "Ricky" } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_middlename()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { MiddleName = "Bobby" } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_lastnameprefix()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { LastNamePrefix = "Van" } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_lastname()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { LastName = "Brown" } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_role_Invalid()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { Roles = new List<PersonRoleDtoProperty> { new PersonRoleDtoProperty { RoleType = null } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_role()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { Roles = new List<PersonRoleDtoProperty> { new PersonRoleDtoProperty { RoleType = Dtos.EnumProperties.PersonRoleType.Student } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_Credentials()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId, Value = "00009999" } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4_Credentials_NotSupportedType()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.BannerUserName, Value = "00009999" } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
            }

            [TestMethod]
            public async Task GetPerson4_credentials_Invalid()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId, Value = "0000009999" } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);

            }

            [TestMethod]
            public async Task GetPerson4Filtered_NoPaging()
            {
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { FirstName = "Ricky" } } });
                var personList = await personsController.GetPerson4Async(null, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson4_email()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { EmailAddresses = new List<PersonEmailDtoProperty> { new PersonEmailDtoProperty { Address = "emailaddress" } } });
                var personList = await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);

            }


            [TestMethod]
            public async Task GetGetPersons4KeyNotFoundException()
            {
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { FirstName = "Ricky" } } });

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson4NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person4>(), It.IsAny<string>())).
                    ThrowsAsync(new KeyNotFoundException());
                try
                {
                    await personsController.GetPerson4Async(new Paging(10, 0), personFilterFilter, criteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            public async Task GetGetPersons4PermissionsException()
            {

                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { FirstName = "Ricky" } } });
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson4NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person4>(), It.IsAny<string>())).
                    ThrowsAsync(new PermissionsException());
                try
                {
                    await personsController.GetPerson4Async(new Paging(10, 0), personFilterFilter, criteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4ArgumentException()
            {
                var page = new Paging(10, 0);
                personServiceMock.Setup(s => s.GetPerson4NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person4>(), It.IsAny<string>())).
                    ThrowsAsync(new ArgumentException());
                await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4RepositoryException()
            {
                var page = new Paging(10, 0);
                personServiceMock.Setup(s => s.GetPerson4NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person4>(), It.IsAny<string>())).
                    ThrowsAsync(new RepositoryException());
                await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4IntegrationApiException()
            {
                var page = new Paging(10, 0);
                personServiceMock.Setup(s => s.GetPerson4NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person4>(), It.IsAny<string>())).
                    ThrowsAsync(new IntegrationApiException());
                await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4Exception()
            {
                var page = new Paging(10, 0);
                personServiceMock.Setup(s => s.GetPerson4NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person4>(), It.IsAny<string>())).
                    ThrowsAsync(new Exception());
                await personsController.GetPerson4Async(page, personFilterFilter, criteriaFilter);
            }
        }

        #endregion

        #region Get Tests HEDM v12_1_0

        [TestClass]
        public class PersonsHedmGet_v12_1_0
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Ellucian.Colleague.Dtos.Person5 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter personFilterFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person5();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.GetPerson5ByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personDto);
                var personDtoList = new List<Dtos.Person5>() { personDto };
                var personTuple = new Tuple<IEnumerable<Dtos.Person5>, int>(personDtoList, 1);
                personServiceMock.Setup(s => s.GetPerson5NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person5>(), It.IsAny<string>())).ReturnsAsync(personTuple);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPerson5ById_Cache()
            {
                personsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var person = await personsController.GetPerson5ByIdAsync(personGuid);
                Assert.IsTrue(person is Person5);
            }

            [TestMethod]
            public async Task GetPerson5ById_NoCache()
            {
                personsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var person = await personsController.GetPerson5ByIdAsync(personGuid);
                Assert.IsTrue(person is Person5);
            }

            [TestMethod]
            public async Task GetPerson5ByIdPermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson5ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPerson5ByIdAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetPerson5ByIdKeyNotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson5ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new KeyNotFoundException());
                try
                {
                    await personsController.GetPerson5ByIdAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson5ByIdArgumentException()
            {
                personServiceMock.Setup(s => s.GetPerson5ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new ArgumentException());
                await personsController.GetPerson5ByIdAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson5ByIdRepositoryException()
            {
                personServiceMock.Setup(s => s.GetPerson5ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new RepositoryException());
                await personsController.GetPerson5ByIdAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson5ByIdIntegrationApiException()
            {
                personServiceMock.Setup(s => s.GetPerson5ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new IntegrationApiException());
                await personsController.GetPerson5ByIdAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson5ByIdException()
            {
                personServiceMock.Setup(s => s.GetPerson5ByGuidAsync(personGuid, It.IsAny<bool>())).Throws(new Exception());
                await personsController.GetPerson5ByIdAsync(personGuid);
            }

            [TestMethod]
            public async Task GetPerson5_Cache()
            {
                personsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var personList = await personsController.GetPerson5Async(null, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_NoCache()
            {
                personsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var personList = await personsController.GetPerson5Async(null, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_PersonFilter()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "personFilter";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.PersonFilterFilter() { personFilterId = personGuid });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);

            }

            [TestMethod]
            public async Task GetPerson5_title()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { Title = "Mr." } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_firstname()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { FirstName = "Ricky" } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_middlename()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { MiddleName = "Bobby" } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_lastnameprefix()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { LastNamePrefix = "Van" } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_lastname()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { LastName = "Brown" } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_role_Invalid()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { Roles = new List<PersonRoleDtoProperty> { new PersonRoleDtoProperty { RoleType = null } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_role()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { Roles = new List<PersonRoleDtoProperty> { new PersonRoleDtoProperty { RoleType = Dtos.EnumProperties.PersonRoleType.Student } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_Credentials()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId, Value = "00009999" } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson5_Credentials_NotSupportedType()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.BannerUserName, Value = "00009999" } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
            }

            [TestMethod]
            public async Task GetPerson5_credentials_Invalid()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId, Value = "0000009999" } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);

            }

            [TestMethod]
            public async Task GetPerson5Filtered_NoPaging()
            {
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { FirstName = "Ricky" } } });
                var personList = await personsController.GetPerson5Async(null, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetPerson5_email()
            {
                var page = new Paging(10, 0);
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { EmailAddresses = new List<PersonEmailDtoProperty> { new PersonEmailDtoProperty { Address = "emailaddress" } } });
                var personList = await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
                Assert.IsTrue(personList is IHttpActionResult);

            }


            [TestMethod]
            public async Task GetGetPersons5KeyNotFoundException()
            {
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person4() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { FirstName = "Ricky" } } });

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson5NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person5>(), It.IsAny<string>())).
                    ThrowsAsync(new KeyNotFoundException());
                try
                {
                    await personsController.GetPerson5Async(new Paging(10, 0), personFilterFilter, criteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            public async Task GetGetPersons5PermissionsException()
            {

                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Person5() { PersonNames = new List<PersonName2DtoProperty> { new PersonName2DtoProperty { FirstName = "Ricky" } } });
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPerson5NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person5>(), It.IsAny<string>())).
                    ThrowsAsync(new PermissionsException());
                try
                {
                    await personsController.GetPerson5Async(new Paging(10, 0), personFilterFilter, criteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson5ArgumentException()
            {
                var page = new Paging(10, 0);
                personServiceMock.Setup(s => s.GetPerson5NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person5>(), It.IsAny<string>())).
                    ThrowsAsync(new ArgumentException());
                await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson5RepositoryException()
            {
                var page = new Paging(10, 0);
                personServiceMock.Setup(s => s.GetPerson5NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person5>(), It.IsAny<string>())).
                    ThrowsAsync(new RepositoryException());
                await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson5IntegrationApiException()
            {
                var page = new Paging(10, 0);
                personServiceMock.Setup(s => s.GetPerson5NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person5>(), It.IsAny<string>())).
                    ThrowsAsync(new IntegrationApiException());
                await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPerson4Exception()
            {
                var page = new Paging(10, 0);
                personServiceMock.Setup(s => s.GetPerson5NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Person5>(), It.IsAny<string>())).
                    ThrowsAsync(new Exception());
                await personsController.GetPerson5Async(page, personFilterFilter, criteriaFilter);
            }
        }

        #endregion

        #region Get PersonCredentials Tests

        [TestClass]
        public class PersonsCredentialsGet
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Ellucian.Colleague.Dtos.PersonCredential personCredential;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personCredential Dto object                

                personCredential = new Dtos.PersonCredential()
                {
                    Id = personGuid,
                    Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                };
                var credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>();
                credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty()
                {
                    Type = Dtos.EnumProperties.CredentialType.Ssn,
                    Value = "444-33-2222"
                });
                personCredential.Credentials = credentials;

                personServiceMock.Setup(s => s.GetPersonCredentialByGuidAsync(personGuid)).ReturnsAsync(personCredential);

                personServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonCredential()
            {
                var personCredential = await personsController.GetPersonCredentialByGuidAsync(personGuid);
                Assert.IsTrue(personCredential is PersonCredential);
            }

            [TestMethod]
            public async Task GetPersonCredentialPermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonCredentialByGuidAsync(personGuid)).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPersonCredentialByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetPersonCredentialNotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonCredentialByGuidAsync(personGuid)).Throws(new Exception());
                try
                {
                    await personsController.GetPersonCredentialByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }
        }

        #endregion

        #region GetPersonCredential2ByGuid Tests

        [TestClass]
        public class PersonsController_GetPersonCredential2ByGuid
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Ellucian.Colleague.Dtos.PersonCredential2 personCredential;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personCredential Dto object                

                personCredential = new Dtos.PersonCredential2()
                {
                    Id = personGuid,
                    Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                };
                var credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>();
                credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty2()
                {
                    Type = Dtos.EnumProperties.CredentialType2.Ssn,
                    Value = "444-33-2222"
                });
                personCredential.Credentials = credentials;

                personServiceMock.Setup(s => s.GetPersonCredential2ByGuidAsync(personGuid)).ReturnsAsync(personCredential);

                personServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonCredential2()
            {
                var personCredential = await personsController.GetPersonCredential2ByGuidAsync(personGuid);
                Assert.IsTrue(personCredential is PersonCredential2);
            }

            [TestMethod]
            public async Task GetPersonCredential2PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonCredential2ByGuidAsync(personGuid)).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPersonCredential2ByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetPersonCredential2NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonCredential2ByGuidAsync(personGuid)).Throws(new Exception());
                try
                {
                    await personsController.GetPersonCredential2ByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonCredential2NotFoundException_KeyNotFoundException()
            {
                personServiceMock.Setup(i => i.GetPersonCredential2ByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await personsController.GetPersonCredential2ByGuidAsync("1234");
            }
        }

        #endregion

        #region GetPersonCredential3ByGuid Tests

        [TestClass]
        public class PersonsController_GetPersonCredential3ByGuid
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Ellucian.Colleague.Dtos.PersonCredential2 personCredential;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personCredential Dto object                

                personCredential = new Dtos.PersonCredential2()
                {
                    Id = personGuid,
                    Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                };
                var credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>();
                credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty2()
                {
                    Type = Dtos.EnumProperties.CredentialType2.Ssn,
                    Value = "444-33-2222"
                });
                personCredential.Credentials = credentials;

                personServiceMock.Setup(s => s.GetPersonCredential3ByGuidAsync(personGuid)).ReturnsAsync(personCredential);

                personServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonCredential3ByGuid()
            {
                var personCredential = await personsController.GetPersonCredential3ByGuidAsync(personGuid);
                Assert.IsTrue(personCredential is PersonCredential2);
            }

            [TestMethod]
            public async Task GetPersonCredential3ByGuid_PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonCredential3ByGuidAsync(personGuid)).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPersonCredential3ByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetPersonCredential3ByGuid_NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonCredential3ByGuidAsync(personGuid)).Throws(new Exception());
                try
                {
                    await personsController.GetPersonCredential3ByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonCredential3ByGuid_KeyNotFoundException()
            {
                personServiceMock.Setup(i => i.GetPersonCredential3ByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await personsController.GetPersonCredential3ByGuidAsync("invalid");
            }
        }

        #endregion

        #region GetPersonCredential4ByGuid Tests

        [TestClass]
        public class PersonsController_GetPersonCredential4ByGuid
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Ellucian.Colleague.Dtos.PersonCredential3 personCredential;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personCredential Dto object                

                personCredential = new Dtos.PersonCredential3()
                {
                    Id = personGuid,
                    Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>(),
                    AlternativeCredentials = new List<AlternativeCredentials>()

                };
                var credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>();
                credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                {
                    Type = Dtos.EnumProperties.Credential3Type.Ssn,
                    Value = "444-33-2222"
                });
                personCredential.Credentials = credentials;

                var altCredentials = new List<Dtos.DtoProperties.AlternativeCredentials>();
                altCredentials.Add(new Dtos.DtoProperties.AlternativeCredentials()
                {
                    Type = new GuidObject2("123"),
                    Value = "444-33-2222"
                });
                personCredential.AlternativeCredentials = altCredentials;

                personServiceMock.Setup(s => s.GetPersonCredential4ByGuidAsync(personGuid)).ReturnsAsync(personCredential);

                personServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonCredential4ByGuid()
            {
                var personCredential = await personsController.GetPersonCredential4ByGuidAsync(personGuid);
                Assert.IsTrue(personCredential is PersonCredential3);
            }

            [TestMethod]
            public async Task GetPersonCredential4ByGuid_PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonCredential4ByGuidAsync(personGuid)).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPersonCredential4ByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetPersonCredential4ByGuid_NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonCredential4ByGuidAsync(personGuid)).Throws(new Exception());
                try
                {
                    await personsController.GetPersonCredential4ByGuidAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonCredential4ByGuid_KeyNotFoundException()
            {
                personServiceMock.Setup(i => i.GetPersonCredential4ByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await personsController.GetPersonCredential4ByGuidAsync("invalid");
            }
        }

        #endregion



        #region Get PersonCredentials Tests

        [TestClass]
        public class PersonsCredentialsGet_ALL
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private IEnumerable<Dtos.PersonCredential> personCredentialDtos;
            private Tuple<IEnumerable<Dtos.PersonCredential>, int> personCredentialDtosTuple;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personCredential Dto object                

                personCredentialDtos = new List<Dtos.PersonCredential>()
                {
                    new Dtos.PersonCredential()
                    {
                        Id = personGuid,
                        Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
                        {
                            new Dtos.DtoProperties.CredentialDtoProperty()
                            {
                                Type = Dtos.EnumProperties.CredentialType.Ssn,
                                Value = "444-33-2222"
                            },
                            new Dtos.DtoProperties.CredentialDtoProperty()
                            {
                                Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                                Value = "PID123"
                            }
                        }
                    }
                };
                personCredentialDtosTuple = new Tuple<IEnumerable<PersonCredential>, int>(personCredentialDtos, personCredentialDtos.Count());
                personServiceMock.Setup(s => s.GetAllPersonCredentialsAsync(0, 2, It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
                personsController.Request = new HttpRequestMessage();
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonAllCredential()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personServiceMock.Setup(s => s.GetAllPersonCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                var personCredential = await personsController.GetPersonCredentialsAsync(It.IsAny<Paging>());
                Assert.IsNotNull(personCredential);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential_ArgumentNullException()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var personCredential = await personsController.GetPersonCredentialsAsync(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential_KeyNotFoundException()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var personCredential = await personsController.GetPersonCredentialsAsync(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential_Exception()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentialsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var personCredential = await personsController.GetPersonCredentialsAsync(It.IsAny<Paging>());
            }
        }

        #endregion

        #region Get PersonCredentials2 Tests

        [TestClass]
        public class PersonsCredentialsGet2_ALL
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private IEnumerable<Dtos.PersonCredential2> personCredentialDtos;
            private Tuple<IEnumerable<Dtos.PersonCredential2>, int> personCredentialDtosTuple;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personCredential Dto object                

                personCredentialDtos = new List<Dtos.PersonCredential2>()
                {
                    new Dtos.PersonCredential2()
                    {
                        Id = personGuid,
                        Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                        {
                            new Dtos.DtoProperties.CredentialDtoProperty2()
                            {
                                Type = Dtos.EnumProperties.CredentialType2.Ssn,
                                Value = "444-33-2222"
                            },
                            new Dtos.DtoProperties.CredentialDtoProperty2()
                            {
                                Type = Dtos.EnumProperties.CredentialType2.ColleaguePersonId,
                                Value = "PID123"
                            }
                        }
                    }
                };
                personCredentialDtosTuple = new Tuple<IEnumerable<PersonCredential2>, int>(personCredentialDtos, personCredentialDtos.Count());
                personServiceMock.Setup(s => s.GetAllPersonCredentials2Async(0, 2, It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
                personsController.Request = new HttpRequestMessage();
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonAllCredential2()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personServiceMock.Setup(s => s.GetAllPersonCredentials2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                var personCredential = await personsController.GetPersonCredentials2Async(It.IsAny<Paging>());
                Assert.IsNotNull(personCredential);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential2_ArgumentNullException()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var personCredential = await personsController.GetPersonCredentials2Async(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential2_KeyNotFoundException()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var personCredential = await personsController.GetPersonCredentials2Async(It.IsAny<Paging>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential2_Exception()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var personCredential = await personsController.GetPersonCredentials2Async(It.IsAny<Paging>());
            }
        }

        #endregion

        #region Get PersonCredentials3 Tests

        [TestClass]
        public class PersonsCredentialsGet3_ALL
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private IEnumerable<Dtos.PersonCredential2> personCredentialDtos;
            private Tuple<IEnumerable<Dtos.PersonCredential2>, int> personCredentialDtosTuple;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personCredential Dto object                

                personCredentialDtos = new List<Dtos.PersonCredential2>()
                {
                    new Dtos.PersonCredential2()
                    {
                        Id = personGuid,
                        Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                        {
                            new Dtos.DtoProperties.CredentialDtoProperty2()
                            {
                                Type = Dtos.EnumProperties.CredentialType2.Ssn,
                                Value = "444-33-2222"
                            },
                            new Dtos.DtoProperties.CredentialDtoProperty2()
                            {
                                Type = Dtos.EnumProperties.CredentialType2.ColleaguePersonId,
                                Value = "PID123"
                            }
                        }
                    }
                };
                personCredentialDtosTuple = new Tuple<IEnumerable<PersonCredential2>, int>(personCredentialDtos, personCredentialDtos.Count());
                personServiceMock.Setup(s => s.GetAllPersonCredentials2Async(0, 2, It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
                personsController.Request = new HttpRequestMessage();
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonAllCredential3()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);
                Assert.IsNotNull(personCredential);
            }

            [TestMethod]
            public async Task GetPersonAllCredential3_Filter_colleaguePersonId()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);
                var expected = personCredentialDtosTuple.Item1.FirstOrDefault(x => x.Id == personGuid);

                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential2>;
                var actual = results.FirstOrDefault(x => x.Id == personGuid);

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            public async Task GetPersonAllCredential3_Filter_ssn()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);
                var expected = personCredentialDtosTuple.Item1.FirstOrDefault(x => x.Id == personGuid);

                //string criteria = @"{'credentials':[{'type':'ssn','value':'444-33-2222'}]}";
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential2>;
                var actual = results.FirstOrDefault(x => x.Id == personGuid);

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            public async Task GetPersonAllCredential3_Filter_InvalidValue()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(new List<PersonCredential2>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'type':'colleaguePersonId','value':'INVALID'}]}";
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential2>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetPersonAllCredential3_Filter_InvalidType()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(new List<PersonCredential2>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'type':'invalid','value':'INVALID'}]}";
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential2>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetPersonAllCredential3_Filter_DuplicateType()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(new List<PersonCredential2>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'type':'colleaguePersonId','value':'ABC1234'}, {'type':'colleaguePersonId','value':'4321CBA'}]}";
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.PersonCredential2() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = Dtos.EnumProperties.CredentialType2.ColleaguePersonId, Value = "4321CBA" } } });
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential2>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetPersonAllCredential3_Filter_MissingType()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(new List<PersonCredential2>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'value':'INVALID'}]}";                
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.PersonCredential2() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = Dtos.EnumProperties.CredentialType2.NotSet, Value = "0000009999" } } });
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential2>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetPersonAllCredential3_Filter_MissingValue()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(new List<PersonCredential2>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'type':'colleaguePersonId'}]}";
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.PersonCredential2() { Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = Dtos.EnumProperties.CredentialType2.ColleaguePersonId, Value = "" } } });
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential2>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential3_ArgumentNullException()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                   It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential3_KeyNotFoundException()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential3_Exception()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential2>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var personCredential = await personsController.GetPersonCredentials3Async(It.IsAny<Paging>(), criteriaFilter);
            }

        }

        #endregion

        #region Get PersonCredentials4 Tests

        [TestClass]
        public class PersonsCredentialsGet4_ALL
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private IEnumerable<Dtos.PersonCredential3> personCredentialDtos;
            private Tuple<IEnumerable<Dtos.PersonCredential3>, int> personCredentialDtosTuple;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personCredential Dto object                

                personCredentialDtos = new List<Dtos.PersonCredential3>()
                {
                    new Dtos.PersonCredential3()
                    {
                        Id = personGuid,
                        Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                        {
                            new Dtos.DtoProperties.Credential3DtoProperty()
                            {
                                Type = Dtos.EnumProperties.Credential3Type.Ssn,
                                Value = "444-33-2222"
                            },
                            new Dtos.DtoProperties.Credential3DtoProperty()
                            {
                                Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId,
                                Value = "PID123"
                            }
                        }
                    }
                };
                personCredentialDtosTuple = new Tuple<IEnumerable<PersonCredential3>, int>(personCredentialDtos, personCredentialDtos.Count());
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(0, 2, It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
                personsController.Request = new HttpRequestMessage();
                personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonAllCredential4()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);
                Assert.IsNotNull(personCredential);
            }

            [TestMethod]
            public async Task GetPersonAllCredential4_Filter_colleaguePersonId()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);
                var expected = personCredentialDtosTuple.Item1.FirstOrDefault(x => x.Id == personGuid);

                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential3>;
                var actual = results.FirstOrDefault(x => x.Id == personGuid);

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            public async Task GetPersonAllCredential4_Filter_ssn()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);
                var expected = personCredentialDtosTuple.Item1.FirstOrDefault(x => x.Id == personGuid);

                //string criteria = @"{'credentials':[{'type':'ssn','value':'444-33-2222'}]}";
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential3>;
                var actual = results.FirstOrDefault(x => x.Id == personGuid);

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            public async Task GetPersonAllCredential4_Filter_InvalidValue()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential3>, int>(new List<PersonCredential3>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'type':'colleaguePersonId','value':'INVALID'}]}";
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential3>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetPersonAllCredential4_Filter_InvalidType()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential3>, int>(new List<PersonCredential3>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'type':'invalid','value':'INVALID'}]}";
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential3>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetPersonAllCredential4_Filter_DuplicateType()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential3>, int>(new List<PersonCredential3>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'type':'colleaguePersonId','value':'ABC1234'}, {'type':'colleaguePersonId','value':'4321CBA'}]}";
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.PersonCredential3() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId, Value = "4321CBA" } } });
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential3>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetPersonAllCredential4_Filter_MissingType()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential3>, int>(new List<PersonCredential3>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'value':'INVALID'}]}";                
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.PersonCredential3() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.NotSet, Value = "0000009999" } } });
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential3>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetPersonAllCredential4_Filter_MissingValue()
            {
                personsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                personsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                personCredentialDtosTuple = new Tuple<IEnumerable<Dtos.PersonCredential3>, int>(new List<PersonCredential3>(), 0);
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ReturnsAsync(personCredentialDtosTuple);

                //string criteria = @"{'credentials':[{'type':'colleaguePersonId'}]}";
                var filterGroupName = "criteria";
                personsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.PersonCredential3() { Credentials = new List<Credential3DtoProperty> { new Credential3DtoProperty { Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId, Value = "" } } });
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);
                var httpResponseMessage = await personCredential.ExecuteAsync(cancelToken);
                var results = ((ObjectContent<IEnumerable<Dtos.PersonCredential3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonCredential3>;

                Assert.IsTrue(personCredential is IHttpActionResult);

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential4_ArgumentNullException()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                   It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential4_KeyNotFoundException()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonAllCredential4_Exception()
            {
                personServiceMock.Setup(s => s.GetAllPersonCredentials4Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<PersonCredential3>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var personCredential = await personsController.GetPersonCredentials4Async(It.IsAny<Paging>(), criteriaFilter);
            }

        }

        #endregion

        #region GetPersonsActiveRestrictionTypes Tests

        [TestClass]
        public class GetPersonsActiveRestrictionType
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private List<Dtos.GuidObject> personRestrictionTypeDto;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;

            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            string personRestriction1 = "b55e8f9c-6f37-457f-afe9-494a49136c6d";
            string personRestriction2 = "e73f59ac-6c0f-4382-943f-a265edd79c84";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup person restriction type object                
                personRestrictionTypeDto = new List<Dtos.GuidObject>()
                {
                    new GuidObject(){ Guid = personRestriction1 },
                    new GuidObject(){ Guid = personRestriction2 }
                };
                personRestrictionTypeServiceMock.Setup(s => s.GetActivePersonRestrictionTypesAsync(personGuid)).ReturnsAsync(personRestrictionTypeDto);
                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task GetPersonRestrictionTypes()
            {
                var personRestrictionType = await personsController.GetActivePersonRestrictionTypesAsync(personGuid);
                Assert.IsTrue(personRestrictionType is List<Dtos.GuidObject>);
            }

            [TestMethod]
            public async Task GetPersonRestictionTypesException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personRestrictionTypeServiceMock.Setup(s => s.GetActivePersonRestrictionTypesAsync(personGuid)).Throws(new Exception());
                try
                {
                    await personsController.GetActivePersonRestrictionTypesAsync(personGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region GetProfile Tests

        [TestClass]
        public class GetProfileTests
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Dtos.Base.Profile profileDto;
            private string personId;

            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object    
                personId = "0000010";
                profileDto = new Dtos.Base.Profile()
                {
                    Id = personId,
                    LastName = "Brown",
                    FirstName = "Ricky",
                    PreferredEmailAddress = "rickybrown@ellucian.com"
                };
                personServiceMock.Setup(s => s.GetProfileAsync(personId, It.IsAny<bool>())).ReturnsAsync(profileDto);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task GetProfile_Success()
            {
                var profile = await personsController.GetProfileAsync(personId);
                Assert.IsTrue(profile is Dtos.Base.Profile);
            }

            [TestMethod]
            public async Task GetProfile_PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetProfileAsync(personId, It.IsAny<bool>())).Throws(new PermissionsException());
                try
                {
                    await personsController.GetProfileAsync(personId);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetProfile_NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetProfileAsync(personId, It.IsAny<bool>())).Throws(new Exception());
                try
                {
                    await personsController.GetProfileAsync(personId);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            public async Task GetProfile_ArgumentNullException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetProfileAsync(null, It.IsAny<bool>())).Throws(new ArgumentNullException());

                try
                {
                    await personsController.GetProfileAsync(null);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region GetPersonProxyDetailsAsync Test
        [TestClass]

        public class GetPersonProxyDetailsAsyncTest
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Dtos.Base.PersonProxyDetails personProxyDetailsDto;
            private string personId;

            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            /// Gets or sets the test context which provides
            /// information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;


                // Set up the Dto Object
                personId = "0000010";
                personProxyDetailsDto = new Dtos.Base.PersonProxyDetails()
                {
                    PersonId = personId,
                    ProxyEmailAddress = "hierarchyJulie@test.com",
                    PreferredName = "Julie Adams"
                };

                personServiceMock.Setup(s => s.GetPersonProxyDetailsAsync(personId)).ReturnsAsync(personProxyDetailsDto);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task GetPersonProxyDetailsAsync_Success()
            {
                var personProxyDetails = await personsController.GetPersonProxyDetailsAsync(personId);
                Assert.IsTrue(personProxyDetails is Dtos.Base.PersonProxyDetails);
            }
            
            [TestMethod]
            public async Task GetPersonProxyDetailsAsync_PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonProxyDetailsAsync(personId)).Throws(new PermissionsException());
                try
                {
                    await personsController.GetPersonProxyDetailsAsync(personId);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetPersonProxyDetailsAsync_NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonProxyDetailsAsync(personId)).Throws(new Exception());
                try
                {
                    await personsController.GetPersonProxyDetailsAsync(personId);
                }
                catch(HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetPersonProxyDetailsAsync_ArgumentNullException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.GetPersonProxyDetailsAsync(null)).Throws(new ArgumentNullException());
                try
                {
                    await personsController.GetPersonProxyDetailsAsync(null);
                }
                catch(HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }
        #endregion

        #region GetEmergencyInformationAsync Tests

        [TestClass]
        public class GetEmergencyInformationAsyncTests
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Dtos.Base.EmergencyInformation emergencyInformationDto;
            private string personId;

            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object    
                personId = "0000010";
                emergencyInformationDto = new Dtos.Base.EmergencyInformation()
                {
                    PersonId = personId,
                    InsuranceInformation = "BCBS",
                    HospitalPreference = "Memorial Hospital"
                };
                emergencyInformationServiceMock.Setup(s => s.GetEmergencyInformationAsync(personId)).Returns(Task.FromResult(emergencyInformationDto));

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task GetEmergencyInformationAsync_Success()
            {
                var emergencyInfo = await personsController.GetEmergencyInformationAsync(personId);
                Assert.IsTrue(emergencyInfo is Dtos.Base.EmergencyInformation);
            }

            [TestMethod]
            public async Task GetEmergencyInformationAsync_PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.GetEmergencyInformationAsync(personId)).Throws(new PermissionsException());
                try
                {
                    await personsController.GetEmergencyInformationAsync(personId);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetEmergencyInformationAsync_NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.GetEmergencyInformationAsync(personId)).Throws(new Exception());
                try
                {
                    await personsController.GetEmergencyInformationAsync(personId);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            public async Task GetEmergencyInformationAsync_ArgumentNullException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.GetEmergencyInformationAsync(null)).Throws(new ArgumentNullException());

                try
                {
                    await personsController.GetEmergencyInformationAsync(null);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region GetEmergencyInformation2Async Tests

        [TestClass]
        public class GetEmergencyInformation2AsyncTests
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Dtos.Base.EmergencyInformation emergencyInformationDto;
            private PrivacyWrapper<Dtos.Base.EmergencyInformation> privacyWrappedEmergencyInformationDto;
            private string personId;

            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object    
                personId = "0000010";
                emergencyInformationDto = new Dtos.Base.EmergencyInformation()
                {
                    PersonId = personId,
                    InsuranceInformation = "BCBS",
                    HospitalPreference = "Memorial Hospital"
                };
                privacyWrappedEmergencyInformationDto = new PrivacyWrapper<EmergencyInformation>(emergencyInformationDto, false);
                emergencyInformationServiceMock.Setup(s => s.GetEmergencyInformation2Async(personId)).Returns(Task.FromResult(privacyWrappedEmergencyInformationDto));

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task GetEmergencyInformation2Async_Success()
            {
                var emergencyInfo = await personsController.GetEmergencyInformation2Async(personId);
                Assert.IsTrue(emergencyInfo is Dtos.Base.EmergencyInformation);
            }

            [TestMethod]
            public async Task GetEmergencyInformation2Async_PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.GetEmergencyInformation2Async(personId)).Throws(new PermissionsException());
                try
                {
                    await personsController.GetEmergencyInformation2Async(personId);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetEmergencyInformation2Async_NotFoundException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.GetEmergencyInformation2Async(personId)).Throws(new Exception());
                try
                {
                    await personsController.GetEmergencyInformation2Async(personId);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            public async Task GetEmergencyInformation2Async_ArgumentNullException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.GetEmergencyInformation2Async(null)).Throws(new ArgumentNullException());

                try
                {
                    await personsController.GetEmergencyInformation2Async(null);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region Post2 Tests

        [TestClass]
        public class PersonsPost2
        {
            private PersonsController _personsController;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IPersonService> _personServiceMock;
            private IPersonService _personService;
            private Mock<IPersonRestrictionTypeService> _personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService _personRestrictionTypeService;
            private Mock<IEmergencyInformationService> _emergencyInformationServiceMock;
            private IEmergencyInformationService _emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person2 _personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                _adapterRegistry = new AdapterRegistry(adapters, logger);

                _personServiceMock = new Mock<IPersonService>();
                _personService = _personServiceMock.Object;

                _personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                _personRestrictionTypeService = _personRestrictionTypeServiceMock.Object;

                _emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                _emergencyInformationService = _emergencyInformationServiceMock.Object;

                // setup personDto object                
                _personDto = new Dtos.Person2 { Id = personGuid };
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                _personServiceMock.Setup(s => s.GetPerson2ByGuidNonCachedAsync(personGuid)).ReturnsAsync(_personDto);

                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).ReturnsAsync(_personDto);

                _personsController = new PersonsController(_adapterRegistry, _personService, _personRestrictionTypeService, _emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personsController = null;
                _personService = null;
                _personRestrictionTypeService = null;
                _emergencyInformationService = null;
            }

            [TestMethod]
            public async Task PostPerson2()
            {
                var person = await _personsController.PostPerson2Async(_personDto);
                Assert.IsTrue(person is Person2);
            }

            [TestMethod]
            public async Task PostPerson2PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).Throws(new PermissionsException());
                try
                {
                    await _personsController.PostPerson2Async(_personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson2ArgumentException()
            {
                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).Throws(new ArgumentException());
                await _personsController.PostPerson2Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson2RepositoryException()
            {
                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).Throws(new RepositoryException());
                await _personsController.PostPerson2Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson2IntegrationApiException()
            {
                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).Throws(new IntegrationApiException());
                await _personsController.PostPerson2Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson2ConfigurationException()
            {
                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).Throws(new ConfigurationException());
                await _personsController.PostPerson2Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson2NullException()
            {

                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).Throws(new ConfigurationException());
                await _personsController.PostPerson2Async(null);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson2NullIdException()
            {
                _personDto.Id = null;
                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).Throws(new ConfigurationException());

                await _personsController.PostPerson2Async(_personDto);
            }
            [TestMethod]
            public async Task PostPerson2Exception()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.CreatePerson2Async(_personDto)).Throws(new Exception());
                try
                {
                    await _personsController.PostPerson2Async(_personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region Post3 Tests v8

        [TestClass]
        public class PersonsPost3_v8
        {
            private PersonsController _personsController;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IPersonService> _personServiceMock;
            private IPersonService _personService;
            private Mock<IPersonRestrictionTypeService> _personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService _personRestrictionTypeService;
            private Mock<IEmergencyInformationService> _emergencyInformationServiceMock;
            private IEmergencyInformationService _emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person3 _personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                _adapterRegistry = new AdapterRegistry(adapters, logger);

                _personServiceMock = new Mock<IPersonService>();
                _personService = _personServiceMock.Object;

                _personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                _personRestrictionTypeService = _personRestrictionTypeServiceMock.Object;

                _emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                _emergencyInformationService = _emergencyInformationServiceMock.Object;

                // setup personDto object                
                _personDto = new Dtos.Person3 { Id = personGuid };
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                List<string> personGuidList = new List<string>() { personGuid };
                List<Dtos.Person3> _personDtoList = new List<Dtos.Person3>() { _personDto };
                _personServiceMock.Setup(s => s.GetPerson3ByGuidNonCachedAsync(personGuidList)).ReturnsAsync(_personDtoList);

                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).ReturnsAsync(_personDto);

                _personsController = new PersonsController(_adapterRegistry, _personService, _personRestrictionTypeService, _emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personsController = null;
                _personService = null;
                _personRestrictionTypeService = null;
                _emergencyInformationService = null;
            }

            [TestMethod]
            public async Task PostPerson3()
            {
                var person = await _personsController.PostPerson3Async(_personDto);
                Assert.IsTrue(person is Person3);
            }

            [TestMethod]
            public async Task PostPerson3PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).Throws(new PermissionsException());
                try
                {
                    await _personsController.PostPerson3Async(_personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson3ArgumentException()
            {
                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).Throws(new ArgumentException());
                await _personsController.PostPerson3Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson3RepositoryException()
            {
                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).Throws(new RepositoryException());
                await _personsController.PostPerson3Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson3IntegrationApiException()
            {
                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).Throws(new IntegrationApiException());
                await _personsController.PostPerson3Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson3ConfigurationException()
            {
                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).Throws(new ConfigurationException());
                await _personsController.PostPerson3Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson3NullException()
            {

                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).Throws(new ConfigurationException());
                await _personsController.PostPerson3Async(null);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson3NullIdException()
            {
                _personDto.Id = null;
                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).Throws(new ConfigurationException());

                await _personsController.PostPerson3Async(_personDto);
            }
            [TestMethod]
            public async Task PostPerson3Exception()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.CreatePerson3Async(_personDto)).Throws(new Exception());
                try
                {
                    await _personsController.PostPerson3Async(_personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region Post4 Tests v12

        [TestClass]
        public class PersonsPost4_v12
        {
            private PersonsController _personsController;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IPersonService> _personServiceMock;
            private IPersonService _personService;
            private Mock<IPersonRestrictionTypeService> _personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService _personRestrictionTypeService;
            private Mock<IEmergencyInformationService> _emergencyInformationServiceMock;
            private IEmergencyInformationService _emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person4 _personDto;
            string emptyPersonGuid = "00000000-0000-0000-0000-000000000000";
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                _adapterRegistry = new AdapterRegistry(adapters, logger);

                _personServiceMock = new Mock<IPersonService>();
                _personService = _personServiceMock.Object;

                _personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                _personRestrictionTypeService = _personRestrictionTypeServiceMock.Object;

                _emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                _emergencyInformationService = _emergencyInformationServiceMock.Object;

                // setup personDto object                
                _personDto = new Dtos.Person4 { Id = emptyPersonGuid };
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                List<string> personGuidList = new List<string>() { emptyPersonGuid };
                List<Dtos.Person4> _personDtoList = new List<Dtos.Person4>() { _personDto };
                //_personServiceMock.Setup(s => s.GetPerson3ByGuidNonCachedAsync(personGuidList)).ReturnsAsync(_personDtoList);

                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).ReturnsAsync(_personDto);

                _personsController = new PersonsController(_adapterRegistry, _personService, _personRestrictionTypeService, _emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personsController = null;
                _personService = null;
                _personRestrictionTypeService = null;
                _emergencyInformationService = null;
            }

            [TestMethod]
            public async Task PostPerson4()
            {
                var person = await _personsController.PostPerson4Async(_personDto);
                Assert.IsTrue(person is Person4);
            }

            [TestMethod]
            public async Task PostPerson4PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new PermissionsException());
                try
                {
                    await _personsController.PostPerson4Async(_personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson4ArgumentException()
            {
                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new ArgumentException());
                await _personsController.PostPerson4Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson4RepositoryException()
            {
                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new RepositoryException());
                await _personsController.PostPerson4Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson4IntegrationApiException()
            {
                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new IntegrationApiException());
                await _personsController.PostPerson4Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson4ConfigurationException()
            {
                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new ConfigurationException());
                await _personsController.PostPerson4Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson4NullException()
            {

                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new ConfigurationException());
                await _personsController.PostPerson4Async(null);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPerson4NullIdException()
            {
                _personDto.Id = null;
                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new ConfigurationException());

                await _personsController.PostPerson4Async(_personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PostPerson4NotEmptyIdException()
            {
                _personDto.Id = personGuid;
                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new ConfigurationException());

                await _personsController.PostPerson4Async(_personDto);
            }

            [TestMethod]
            public async Task PostPerson4Exception()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.CreatePerson4Async(_personDto)).Throws(new Exception());
                try
                {
                    await _personsController.PostPerson4Async(_personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region Put2 Tests

        [TestClass]
        public class Persons2Put
        {
            private PersonsController _personsController;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IPersonService> _personServiceMock;
            private IPersonService _personService;
            private Mock<IPersonRestrictionTypeService> _personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService _personRestrictionTypeService;
            private Mock<IEmergencyInformationService> _emergencyInformationServiceMock;
            private IEmergencyInformationService _emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person2 _personDto;
            private Ellucian.Colleague.Dtos.Base.Profile _profileDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                var adapters = new HashSet<ITypeAdapter>();
                _adapterRegistry = new AdapterRegistry(adapters, logger);

                _personServiceMock = new Mock<IPersonService>();
                _personService = _personServiceMock.Object;

                _personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                _personRestrictionTypeService = _personRestrictionTypeServiceMock.Object;

                _emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                _emergencyInformationService = _emergencyInformationServiceMock.Object;

                // setup personDto object                
                _personDto = new Dtos.Person2 { Id = personGuid };
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                _personServiceMock.Setup(s => s.GetPerson2ByGuidNonCachedAsync(personGuid)).ReturnsAsync(_personDto);
                _personServiceMock.Setup(s => s.GetPerson2ByGuidAsync(personGuid, true)).ReturnsAsync(_personDto);
                _personServiceMock.Setup(s => s.UpdatePerson2Async(It.IsAny<Person2>())).ReturnsAsync(_personDto);

                _personsController = new PersonsController(_adapterRegistry, _personService, _personRestrictionTypeService, _emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                _personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                _personsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_personDto));
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personsController = null;
                _personService = null;
                _personRestrictionTypeService = null;
                _emergencyInformationService = null;
            }

            [TestMethod]
            public async Task PutPerson2()
            {
                var person = await _personsController.PutPerson2Async(personGuid, _personDto);
                Assert.IsTrue(person is Person2);
            }

            [TestMethod]
            public async Task PutPerson2PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.UpdatePerson2Async(It.IsAny<Person2>())).Throws(new PermissionsException());
                try
                {
                    await _personsController.PutPerson2Async(personGuid, _personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            public async Task PutPerson2Exception()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.UpdatePerson2Async(It.IsAny<Person2>())).Throws(new Exception());
                try
                {
                    await _personsController.PutPerson2Async(personGuid, _personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson2NullGuidException()
            {
                await _personsController.PutPerson2Async(null, _personDto);
            }



            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson2NullPersonGuidException()
            {
                _personDto.Id = null;
                await _personsController.PutPerson2Async(personGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson2GuidMismatchException()
            {
                await _personsController.PutPerson2Async("123", _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson2_ArgumentException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson2Async(It.IsAny<Person2>())).Throws<ArgumentException>();
                await _personsController.PutPerson2Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson2_RepositoryException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson2Async(It.IsAny<Person2>())).Throws<RepositoryException>();
                await _personsController.PutPerson2Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson2_IntegrationApiException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson2Async(It.IsAny<Person2>())).Throws<IntegrationApiException>();
                await _personsController.PutPerson2Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson2_ConfigurationException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson2Async(It.IsAny<Person2>())).Throws<ConfigurationException>();
                await _personsController.PutPerson2Async(personGuid, _personDto);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson2_NilGuid()
            {
                await _personsController.PutPerson2Async(Guid.Empty.ToString(), _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson2_NilPersonGuid()
            {
                _personDto.Id = string.Empty;
                await _personsController.PutPerson2Async(new Guid().ToString(), _personDto);
            }
        }

        #endregion

        #region Put3 Tests v8

        [TestClass]
        public class Persons3Put_v8
        {
            private PersonsController _personsController;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IPersonService> _personServiceMock;
            private IPersonService _personService;
            private Mock<IPersonRestrictionTypeService> _personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService _personRestrictionTypeService;
            private Mock<IEmergencyInformationService> _emergencyInformationServiceMock;
            private IEmergencyInformationService _emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person3 _personDto;
            private Ellucian.Colleague.Dtos.Base.Profile _profileDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                var adapters = new HashSet<ITypeAdapter>();
                _adapterRegistry = new AdapterRegistry(adapters, logger);

                _personServiceMock = new Mock<IPersonService>();
                _personService = _personServiceMock.Object;

                _personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                _personRestrictionTypeService = _personRestrictionTypeServiceMock.Object;

                _emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                _emergencyInformationService = _emergencyInformationServiceMock.Object;

                // setup personDto object                
                _personDto = new Dtos.Person3 { Id = personGuid };
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                List<string> personGuidList = new List<string>() { personGuid };
                List<Dtos.Person3> _personDtoList = new List<Dtos.Person3>() { _personDto };
                _personServiceMock.Setup(s => s.GetPerson3ByGuidNonCachedAsync(personGuidList)).ReturnsAsync(_personDtoList);
                _personServiceMock.Setup(s => s.GetPerson3ByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(_personDto);

                _personServiceMock.Setup(s => s.UpdatePerson3Async(It.IsAny<Person3>())).ReturnsAsync(_personDto);

                _personsController = new PersonsController(_adapterRegistry, _personService, _personRestrictionTypeService, _emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                _personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                _personsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_personDto));
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personsController = null;
                _personService = null;
                _personRestrictionTypeService = null;
                _emergencyInformationService = null;
            }

            [TestMethod]
            public async Task PutPerson3()
            {
                var person = await _personsController.PutPerson3Async(personGuid, _personDto);
                Assert.IsTrue(person is Person3);
            }

            [TestMethod]
            public async Task PutPerson3PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.UpdatePerson3Async(It.IsAny<Person3>())).Throws(new PermissionsException());
                try
                {
                    await _personsController.PutPerson3Async(personGuid, _personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            public async Task PutPerson3Exception()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.UpdatePerson3Async(It.IsAny<Person3>())).Throws(new Exception());
                try
                {
                    await _personsController.PutPerson3Async(personGuid, _personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson3NullGuidException()
            {
                await _personsController.PutPerson3Async(null, _personDto);
            }



            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson3NullPersonGuidException()
            {
                _personDto.Id = null;
                await _personsController.PutPerson3Async(personGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson3GuidMismatchException()
            {
                await _personsController.PutPerson3Async("123", _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson3_ArgumentException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson3Async(It.IsAny<Person3>())).Throws<ArgumentException>();
                await _personsController.PutPerson3Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson3_RepositoryException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson3Async(It.IsAny<Person3>())).Throws<RepositoryException>();
                await _personsController.PutPerson3Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson2_IntegrationApiException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson3Async(It.IsAny<Person3>())).Throws<IntegrationApiException>();
                await _personsController.PutPerson3Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson3_ConfigurationException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson3Async(It.IsAny<Person3>())).Throws<ConfigurationException>();
                await _personsController.PutPerson3Async(personGuid, _personDto);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson3_NilGuid()
            {
                await _personsController.PutPerson3Async(Guid.Empty.ToString(), _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson3_NilPersonGuid()
            {
                _personDto.Id = string.Empty;
                await _personsController.PutPerson3Async(new Guid().ToString(), _personDto);
            }
        }

        #endregion

        #region Put4 Tests v12

        [TestClass]
        public class Persons4Put_v12
        {
            private PersonsController _personsController;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IPersonService> _personServiceMock;
            private IPersonService _personService;
            private Mock<IPersonRestrictionTypeService> _personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService _personRestrictionTypeService;
            private Mock<IEmergencyInformationService> _emergencyInformationServiceMock;
            private IEmergencyInformationService _emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person4 _personDto;
            private Ellucian.Colleague.Dtos.Base.Profile _profileDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                var adapters = new HashSet<ITypeAdapter>();
                _adapterRegistry = new AdapterRegistry(adapters, logger);

                _personServiceMock = new Mock<IPersonService>();
                _personService = _personServiceMock.Object;

                _personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                _personRestrictionTypeService = _personRestrictionTypeServiceMock.Object;

                _emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                _emergencyInformationService = _emergencyInformationServiceMock.Object;

                // setup personDto object                
                _personDto = new Dtos.Person4 { Id = personGuid };
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                List<string> personGuidList = new List<string>() { personGuid };
                List<Dtos.Person4> _personDtoList = new List<Dtos.Person4>() { _personDto };
                //_personServiceMock.Setup(s => s.GetPerson3ByGuidNonCachedAsync(personGuidList)).ReturnsAsync(_personDtoList);
                _personServiceMock.Setup(s => s.GetPerson4ByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(_personDto);

                _personServiceMock.Setup(s => s.UpdatePerson4Async(It.IsAny<Person4>())).ReturnsAsync(_personDto);

                _personsController = new PersonsController(_adapterRegistry, _personService, _personRestrictionTypeService, _emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                _personsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                _personsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_personDto));
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personsController = null;
                _personService = null;
                _personRestrictionTypeService = null;
                _emergencyInformationService = null;
            }

            [TestMethod]
            public async Task PutPerson4()
            {
                var person = await _personsController.PutPerson4Async(personGuid, _personDto);
                Assert.IsTrue(person is Person4);
            }

            [TestMethod]
            public async Task PutPerson4PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.UpdatePerson4Async(It.IsAny<Person4>())).Throws(new PermissionsException());
                try
                {
                    await _personsController.PutPerson4Async(personGuid, _personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            public async Task PutPerson4Exception()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                _personServiceMock.Setup(s => s.UpdatePerson4Async(It.IsAny<Person4>())).Throws(new Exception());
                try
                {
                    await _personsController.PutPerson4Async(personGuid, _personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson4NullGuidException()
            {
                await _personsController.PutPerson4Async(null, _personDto);
            }



            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson4NullPersonGuidException()
            {
                _personDto.Id = null;
                await _personsController.PutPerson4Async(personGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPerson4GuidMismatchException()
            {
                await _personsController.PutPerson4Async("123", _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson4_ArgumentException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson4Async(It.IsAny<Person4>())).Throws<ArgumentException>();
                await _personsController.PutPerson4Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson4_RepositoryException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson4Async(It.IsAny<Person4>())).Throws<RepositoryException>();
                await _personsController.PutPerson4Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson4_IntegrationApiException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson4Async(It.IsAny<Person4>())).Throws<IntegrationApiException>();
                await _personsController.PutPerson4Async(personGuid, _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson4_ConfigurationException()
            {
                _personServiceMock.Setup(s => s.UpdatePerson4Async(It.IsAny<Person4>())).Throws<ConfigurationException>();
                await _personsController.PutPerson4Async(personGuid, _personDto);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson4_NilGuid()
            {
                await _personsController.PutPerson4Async(Guid.Empty.ToString(), _personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonController_PutPerson4_NilPersonGuid()
            {
                _personDto.Id = string.Empty;
                await _personsController.PutPerson4Async(new Guid().ToString(), _personDto);
            }
        }

        #endregion

        #region Delete Tests

        [TestClass]
        public class PersonsDelete
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person3 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person3();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.UpdatePerson3Async(personDto)).ReturnsAsync(personDto);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonsController_DeleteThrowsIntAppiExc()
            {
                await personsController.DeletePersonAsync(personGuid);
            }
        }

        #endregion

        #region PutEmergencyInformation Tests

        [TestClass]
        public class PutEmergencyInformationTests
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Dtos.Base.EmergencyInformation emergencyInformationDto;
            private string personId;

            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object    
                personId = "0000010";
                emergencyInformationDto = new Dtos.Base.EmergencyInformation()
                {
                    PersonId = personId,
                    InsuranceInformation = "BCBS",
                    HospitalPreference = "Memorial Hospital"
                };
                emergencyInformationServiceMock.Setup(s => s.UpdateEmergencyInformation(emergencyInformationDto)).Returns(emergencyInformationDto);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public void PutEmergencyInformation_Success()
            {
                var emergencyInfo = personsController.PutEmergencyInformation(emergencyInformationDto);
                Assert.IsTrue(emergencyInfo is Dtos.Base.EmergencyInformation);
            }

            [TestMethod]
            public void PutEmergencyInformation_PermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.UpdateEmergencyInformation(emergencyInformationDto)).Throws(new PermissionsException());
                try
                {
                    personsController.PutEmergencyInformation(emergencyInformationDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public void PutEmergencyInformation_Exception()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.UpdateEmergencyInformation(emergencyInformationDto)).Throws(new Exception());
                try
                {
                    personsController.PutEmergencyInformation(emergencyInformationDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public void PutEmergencyInformation_NullArgumentException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                emergencyInformationServiceMock.Setup(s => s.UpdateEmergencyInformation(emergencyInformationDto)).Throws(new ArgumentNullException());
                try
                {
                    personsController.PutEmergencyInformation(null);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region QueryPersonByPost Tests

        [TestClass]
        public class PersonQuery2ByPost
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person2 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person2();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.QueryPerson2ByPostAsync(personDto)).ReturnsAsync(new List<Person2>() { personDto }.AsEnumerable());

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task QueryPersonByPost()
            {
                var person = await personsController.QueryPerson2ByPostAsync(personDto);
                Assert.IsTrue(person is IEnumerable<Person2>);
            }

            [TestMethod]
            public async Task QueryPersonByPostPermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.QueryPerson2ByPostAsync(personDto)).Throws(new PermissionsException());
                try
                {
                    await personsController.QueryPerson2ByPostAsync(personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task QueryPersonByPostException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.QueryPerson2ByPostAsync(personDto)).Throws(new Exception());
                try
                {
                    await personsController.QueryPerson2ByPostAsync(personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        [TestClass]
        public class PersonQuery3ByPost
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person3 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person3();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.QueryPerson3ByPostAsync(personDto)).ReturnsAsync(new List<Person3>() { personDto }.AsEnumerable());

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task QueryPersonByPost()
            {
                var person = await personsController.QueryPerson3ByPostAsync(personDto);
                Assert.IsTrue(person is IEnumerable<Person3>);
            }

            [TestMethod]
            public async Task QueryPersonByPostPermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.QueryPerson3ByPostAsync(personDto)).Throws(new PermissionsException());
                try
                {
                    await personsController.QueryPerson3ByPostAsync(personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task QueryPersonByPostException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.QueryPerson3ByPostAsync(personDto)).Throws(new Exception());
                try
                {
                    await personsController.QueryPerson3ByPostAsync(personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        [TestClass]
        public class PersonQuery4ByPost
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person4 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person4();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.QueryPerson4ByPostAsync(personDto, false)).ReturnsAsync(new List<Person4>() { personDto }.AsEnumerable());

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task QueryPersonByPost()
            {
                var person = await personsController.QueryPerson4ByPostAsync(personDto);
                Assert.IsTrue(person is IEnumerable<Person4>);
            }

            [TestMethod]
            public async Task QueryPersonByPostPermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.QueryPerson4ByPostAsync(personDto, false)).Throws(new PermissionsException());
                try
                {
                    await personsController.QueryPerson4ByPostAsync(personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task QueryPersonByPostException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.QueryPerson4ByPostAsync(personDto, false)).Throws(new Exception());
                try
                {
                    await personsController.QueryPerson4ByPostAsync(personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        [TestClass]
        public class PersonQuery5ByPost
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;

            private Ellucian.Colleague.Dtos.Person5 personDto;
            string personGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object                
                personDto = new Dtos.Person5();
                personDto.Id = personGuid;
                var personNames = new List<Dtos.PersonName>();
                var personPrimaryName = new Dtos.PersonName()
                {
                    NameType = Dtos.PersonNameType.Primary,
                    FirstName = "Ricky",
                    LastName = "Brown"
                };
                personServiceMock.Setup(s => s.QueryPerson5ByPostAsync(personDto, false)).ReturnsAsync(new List<Person5>() { personDto }.AsEnumerable());

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task QueryPersonByPost()
            {
                var person = await personsController.QueryPerson5ByPostAsync(personDto);
                Assert.IsTrue(person is IEnumerable<Person5>);
            }

            [TestMethod]
            public async Task QueryPersonByPostPermissionsException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.QueryPerson5ByPostAsync(personDto, false)).Throws(new PermissionsException());
                try
                {
                    await personsController.QueryPerson5ByPostAsync(personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task QueryPersonByPostException()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.QueryPerson5ByPostAsync(personDto, false)).Throws(new Exception());
                try
                {
                    await personsController.QueryPerson5ByPostAsync(personDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }
        #endregion

        #region PutProfileAsync Tests

        [TestClass]
        public class PutProfileAsyncTests
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Dtos.Base.Profile profileDto;
            private string personId;

            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                // setup personDto object    
                personId = "0000010";
                profileDto = new Dtos.Base.Profile()
                {
                    Id = personId,
                    LastName = "Brown",
                    FirstName = "Ricky",
                    PreferredEmailAddress = "rickybrown@ellucian.com"
                };
                personServiceMock.Setup(s => s.UpdateProfileAsync(profileDto)).ReturnsAsync(profileDto);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task PutProfileAsync_Success()
            {
                var profile = await personsController.PutProfileAsync(personId, profileDto);
                Assert.IsTrue(profile is Dtos.Base.Profile);
            }

            [TestMethod]
            public async Task PutProfileAsync_RequestMissingUriPersonId_ReturnsBadRequestStatus()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                try
                {
                    var profile = await personsController.PutProfileAsync(string.Empty, profileDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);

            }

            [TestMethod]
            public async Task PutProfileAsync_RequestMissingRequestBody_ReturnsBadRequestStatus()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                try
                {
                    var profile = await personsController.PutProfileAsync(personId, null);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);

            }

            [TestMethod]
            public async Task PutProfileAsync_RequestBodyMissingPersonId_ReturnsBadRequestStatus()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                try
                {
                    profileDto.Id = string.Empty;
                    var profile = await personsController.PutProfileAsync(personId, profileDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);

            }

            [TestMethod]
            public async Task PutProfileAsync_PersonIdDoesNotMatchIdInRequestBody_ReturnsBadRequestStatus()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                try
                {
                    personId = "123";
                    var profile = await personsController.PutProfileAsync(personId, profileDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);

            }

            [TestMethod]
            public async Task UpdateProfileAsync_ReturnsPermissionsException_ReturnsForbiddenStatus()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.UpdateProfileAsync(profileDto)).Throws(new PermissionsException());
                try
                {
                    await personsController.PutProfileAsync(personId, profileDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task UpdateProfileAsync_ReturnsException_ReturnsBadRequestStatus()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                personServiceMock.Setup(s => s.UpdateProfileAsync(profileDto)).Throws(new ArgumentNullException());

                try
                {
                    await personsController.PutProfileAsync(personId, profileDto);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }
        }

        #endregion

        #region QueryPersonMatchResultsByPostAsync Tests

        [TestClass]
        public class QueryPersonMatchResultsByPostAsyncTests
        {
            private PersonsController personsController;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonService> personServiceMock;
            private IPersonService personService;
            private Mock<IPersonRestrictionTypeService> personRestrictionTypeServiceMock;
            private IPersonRestrictionTypeService personRestrictionTypeService;
            private Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            private IEmergencyInformationService emergencyInformationService;
            private Dtos.Base.PersonMatchCriteria criteriaDto;
            private List<Dtos.Base.PersonMatchResult> resultsDto;

            ILogger logger = new Mock<ILogger>().Object;

            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);

                personServiceMock = new Mock<IPersonService>();
                personService = personServiceMock.Object;

                personRestrictionTypeServiceMock = new Mock<IPersonRestrictionTypeService>();
                personRestrictionTypeService = personRestrictionTypeServiceMock.Object;

                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                emergencyInformationService = emergencyInformationServiceMock.Object;

                criteriaDto = new Dtos.Base.PersonMatchCriteria()
                {
                    MatchCriteriaIdentifier = "PROXY.PERSON",
                    MatchNames = new List<Dtos.Base.PersonName>()
                    {
                        new Dtos.Base.PersonName() { GivenName = "given", FamilyName = "family" }
                    }
                };

                resultsDto = new List<Dtos.Base.PersonMatchResult>()
                {
                    new Dtos.Base.PersonMatchResult() { MatchCategory = Dtos.Base.PersonMatchCategoryType.Potential, MatchScore = 60, PersonId = "0003315" },
                    new Dtos.Base.PersonMatchResult() { MatchCategory = Dtos.Base.PersonMatchCategoryType.Potential, MatchScore = 50, PersonId = "0003315" },
                };

                personServiceMock.Setup(s => s.QueryPersonMatchResultsByPostAsync(criteriaDto)).ReturnsAsync(resultsDto);

                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personsController = null;
                personService = null;
                personRestrictionTypeService = null;
                emergencyInformationService = null;
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsByPostAsync_Success()
            {
                var results = await personsController.QueryPersonMatchResultsByPostAsync(criteriaDto);
                Assert.AreEqual(resultsDto.Count, results.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryPersonMatchResultsByPostAsync_Exception()
            {
                personServiceMock.Setup(s => s.QueryPersonMatchResultsByPostAsync(criteriaDto)).Throws(new Exception("An error occurred"));
                personsController = new PersonsController(adapterRegistry, personService, personRestrictionTypeService, emergencyInformationService, logger);

                var results = await personsController.QueryPersonMatchResultsByPostAsync(criteriaDto);
            }
        }

        #endregion
    }
}