//Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonMatchingRequestsControllerTests
    {
        public TestContext TestContext { get; set; }
        private Mock<IPersonMatchingRequestsService> personMatchingRequestsServiceMock;
        private Mock<ILogger> loggerMock;
        private PersonMatchingRequestsController personMatchingRequestsController;
        private List<Dtos.PersonMatchingRequests> personMatchingRequestsCollection;
        private List<Dtos.PersonMatchingRequestsInitiationsProspects> personMatchingRequestsInitiationsProspectsCollection;
        int offset = 0;
        int limit = 100;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        private Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
        private PersonMatchingRequests criteria = new PersonMatchingRequests();
        private string person = "";
        private string expectedGuid = Guid.NewGuid().ToString();


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            personMatchingRequestsServiceMock = new Mock<IPersonMatchingRequestsService>();
            loggerMock = new Mock<ILogger>();

            BuildData();

            personMatchingRequestsController = new PersonMatchingRequestsController(personMatchingRequestsServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            personMatchingRequestsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            personMatchingRequestsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
        }

        private void BuildData()
        {
            personMatchingRequestsCollection = new List<Dtos.PersonMatchingRequests>();
            personMatchingRequestsInitiationsProspectsCollection = new List<Dtos.PersonMatchingRequestsInitiationsProspects>();

            var personMatchingRequest1 = new Ellucian.Colleague.Dtos.PersonMatchingRequests
            {
                Id = expectedGuid,
                Outcomes = new List<PersonMatchingRequestsOutcomesDtoProperty>()
                {
                    new PersonMatchingRequestsOutcomesDtoProperty()
                    {
                        Type = Dtos.EnumProperties.PersonMatchingRequestsType.Initial,
                        Status = Dtos.EnumProperties.PersonMatchingRequestsStatus.ReviewRequired,
                        Date = DateTime.Now
                    },
                    new PersonMatchingRequestsOutcomesDtoProperty()
                    {
                        Type = Dtos.EnumProperties.PersonMatchingRequestsType.Final,
                        Status = Dtos.EnumProperties.PersonMatchingRequestsStatus.NewPerson,
                        Date = DateTime.Now
                    }
                },
                Originator = "RECRUIT",
                Person = new GuidObject2(Guid.NewGuid().ToString())
            };

            var personMatchingRequest2 = new Ellucian.Colleague.Dtos.PersonMatchingRequests
            {
                Id = "person-matching-request 2 guid",
                Outcomes = new List<PersonMatchingRequestsOutcomesDtoProperty>()
                {
                    new PersonMatchingRequestsOutcomesDtoProperty()
                    {
                        Type = Dtos.EnumProperties.PersonMatchingRequestsType.Initial,
                        Status = Dtos.EnumProperties.PersonMatchingRequestsStatus.ReviewRequired,
                        Date = DateTime.Now
                    },
                    new PersonMatchingRequestsOutcomesDtoProperty()
                    {
                        Type = Dtos.EnumProperties.PersonMatchingRequestsType.Final,
                        Status = Dtos.EnumProperties.PersonMatchingRequestsStatus.NewPerson,
                        Date = DateTime.Now
                    }
                },
                Originator = "RECRUIT",
                Person = new GuidObject2(Guid.NewGuid().ToString())
            };

            personMatchingRequestsCollection.Add(personMatchingRequest1);
            personMatchingRequestsCollection.Add(personMatchingRequest2);

            var personMatchingRequestInitiationsProspects1 = new Ellucian.Colleague.Dtos.PersonMatchingRequestsInitiationsProspects
            {
                Id = Guid.Empty.ToString(),
                Names = new Dtos.DtoProperties.PersonMatchingRequestNamesDtoProperty()
                {
                    Legal = new Dtos.DtoProperties.PersonMatchingRequestNamesNameDtoProperty()
                    {
                        LastName = "Jones",
                        FirstName = "Bridget"
                    }
                },
                Gender = Dtos.EnumProperties.GenderType2.Female,
                MatchingCriteria = new PersonMatchingRequestsInitiationsMatchingCriteria()
                {
                    DateOfBirth = new DateTime(2004,11,15),
                    Credential = new Dtos.DtoProperties.PersonMatchRequestCredentialDtoProperty()
                    {
                        Type = Dtos.EnumProperties.PersonMatchRequestCredentialType.Ssn,
                        Value = "555-22-9945"
                    }
                }
            };

            var personMatchingRequestInitiationsProspects2 = new Ellucian.Colleague.Dtos.PersonMatchingRequestsInitiationsProspects
            {
                Id = Guid.Empty.ToString(),
                Names = new Dtos.DtoProperties.PersonMatchingRequestNamesDtoProperty()
                {
                    Legal = new Dtos.DtoProperties.PersonMatchingRequestNamesNameDtoProperty()
                    {
                        LastName = "Samuel",
                        FirstName = "Jackson",
                        MiddleName = "Leroy"
                    }
                },
                Gender = Dtos.EnumProperties.GenderType2.Male,
                MatchingCriteria = new PersonMatchingRequestsInitiationsMatchingCriteria()
                {
                    DateOfBirth = new DateTime(1957, 10, 22),
                    Credential = new Dtos.DtoProperties.PersonMatchRequestCredentialDtoProperty()
                    {
                        Type = Dtos.EnumProperties.PersonMatchRequestCredentialType.Ssn,
                        Value = "561-44-9945"
                    }
                }
            };
            personMatchingRequestsInitiationsProspectsCollection.Add(personMatchingRequestInitiationsProspects1);
            personMatchingRequestsInitiationsProspectsCollection.Add(personMatchingRequestInitiationsProspects2);

        }

        [TestCleanup]
        public void Cleanup()
        {
            personMatchingRequestsController = null;
            personMatchingRequestsCollection = null;
            loggerMock = null;
            personMatchingRequestsServiceMock = null;
        }
        [TestMethod]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequests_ValidateFields_Nocache()
        {
            personMatchingRequestsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, Public = true };
            var tuple = new Tuple<IEnumerable<Dtos.PersonMatchingRequests>, int>(personMatchingRequestsCollection, 2);
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>())).ReturnsAsync(tuple);

            var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await personMatchingRequests.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonMatchingRequests> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonMatchingRequests>>)httpResponseMessage.Content)
                                                .Value as IEnumerable<Dtos.PersonMatchingRequests>;

            Assert.IsNotNull(actuals);
            Assert.AreEqual(personMatchingRequestsCollection.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = personMatchingRequestsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Originator, actual.Originator);
                Assert.AreEqual(expected.Outcomes[0].Type, actual.Outcomes[0].Type);
                Assert.AreEqual(expected.Outcomes[1].Type, actual.Outcomes[1].Type);
                Assert.AreEqual(expected.Outcomes[0].Status, actual.Outcomes[0].Status);
                Assert.AreEqual(expected.Outcomes[1].Status, actual.Outcomes[1].Status);
                Assert.AreEqual(expected.Outcomes[0].Date, actual.Outcomes[0].Date);
                Assert.AreEqual(expected.Outcomes[1].Date, actual.Outcomes[1].Date);
            }

        }
        [TestMethod]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequests_ValidateFields_Cache()
        {
            personMatchingRequestsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false, Public = true };
            var tuple = new Tuple<IEnumerable<Dtos.PersonMatchingRequests>, int>(personMatchingRequestsCollection, 2);
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>())).ReturnsAsync(tuple);

            var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await personMatchingRequests.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonMatchingRequests> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonMatchingRequests>>)httpResponseMessage.Content)
                                                .Value as IEnumerable<Dtos.PersonMatchingRequests>;

            Assert.IsNotNull(actuals);
            Assert.AreEqual(personMatchingRequestsCollection.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = personMatchingRequestsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Originator, actual.Originator);
                Assert.AreEqual(expected.Outcomes[0].Type, actual.Outcomes[0].Type);
                Assert.AreEqual(expected.Outcomes[1].Type, actual.Outcomes[1].Type);
                Assert.AreEqual(expected.Outcomes[0].Status, actual.Outcomes[0].Status);
                Assert.AreEqual(expected.Outcomes[1].Status, actual.Outcomes[1].Status);
                Assert.AreEqual(expected.Outcomes[0].Date, actual.Outcomes[0].Date);
                Assert.AreEqual(expected.Outcomes[1].Date, actual.Outcomes[1].Date);
            }

        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequests_KeyNotFoundException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync("BADGUID", It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync("BADGUID");
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequests_PermissionsException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>()))
                 .Throws<PermissionsException>();
            var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequests_ArgumentException()
        {

            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>()))
                .Throws<ArgumentException>();
            var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequests_RepositoryException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>()))
               .Throws<RepositoryException>();
            var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequests_IntegrationApiException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>()))
               .Throws<IntegrationApiException>();
            var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }
        [TestMethod]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuidAsync_ValidateFields()
        {
            var expected = personMatchingRequestsCollection.FirstOrDefault();
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            var actual = await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expected.Id);
            Assert.AreEqual(expected.Id, actual.Id, "Id");
            
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequests_Exception()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>()))
                .Throws<Exception>();
            var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuidAsync_Exception()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(string.Empty);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuid_KeyNotFoundException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expectedGuid);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuid_PermissionsException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expectedGuid);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuid_ArgumentException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expectedGuid);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuid_RepositoryException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expectedGuid);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuid_IntegrationApiException()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expectedGuid);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuid_Exception()
        {
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expectedGuid);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_PostPersonMatchingRequestsAsync_Exception()
        {
            await personMatchingRequestsController.PostPersonMatchingRequestsAsync(personMatchingRequestsCollection.FirstOrDefault());
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_PutPersonMatchingRequestsAsync_Exception()
        {
            var sourceContext = personMatchingRequestsCollection.FirstOrDefault();
            await personMatchingRequestsController.PutPersonMatchingRequestsAsync(sourceContext.Id, sourceContext);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_DeletePersonMatchingRequestsAsync_Exception()
        {
            await personMatchingRequestsController.DeletePersonMatchingRequestsAsync(personMatchingRequestsCollection.FirstOrDefault().Id);
        }

        #region person-matching-requests-initiations-prospects

        [TestMethod]
        public async Task PersonMatchingRequestsController_PostPersonMatchingRequestsInitiationsProspectsAsync_Exception()
        {
            var expected = personMatchingRequestsCollection.FirstOrDefault();
            personMatchingRequestsServiceMock.Setup(x => x.CreatePersonMatchingRequestsInitiationsProspectsAsync(It.IsAny<Dtos.PersonMatchingRequestsInitiationsProspects>())).ReturnsAsync(expected);
            var actual = await personMatchingRequestsController.PostPersonMatchingRequestsInitiationsProspectsAsync(personMatchingRequestsInitiationsProspectsCollection.FirstOrDefault());

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Originator, actual.Originator);
            Assert.AreEqual(expected.Outcomes[0].Type, actual.Outcomes[0].Type);
            Assert.AreEqual(expected.Outcomes[1].Type, actual.Outcomes[1].Type);
            Assert.AreEqual(expected.Outcomes[0].Status, actual.Outcomes[0].Status);
            Assert.AreEqual(expected.Outcomes[1].Status, actual.Outcomes[1].Status);
            Assert.AreEqual(expected.Outcomes[0].Date, actual.Outcomes[0].Date);
            Assert.AreEqual(expected.Outcomes[1].Date, actual.Outcomes[1].Date);
        }

        //Get by id v1.0.0
        //Successful
        //GetPersonMatchingRequestsByGuidAsync
        [TestMethod]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuidAsync_Permissions()
        {
            var expected = personMatchingRequestsCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonMatchingRequests" },
                    { "action", "GetPersonMatchingRequestsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("person-matching-requests", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personMatchingRequestsController.Request.SetRouteData(data);
            personMatchingRequestsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewPersonMatchRequest, BasePermissionCodes.CreatePersonMatchRequestProspects });

            var controllerContext = personMatchingRequestsController.ControllerContext;
            var actionDescriptor = personMatchingRequestsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personMatchingRequestsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            var actual = await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expected.Id);

            Object filterObject;
            personMatchingRequestsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewPersonMatchRequest));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.CreatePersonMatchRequestProspects));


        }

        //Get by id v1.0.0
        //Exception
        //GetPersonMatchingRequestsByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonMatchingRequests" },
                    { "action", "GetPersonMatchingRequestsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("person-matching-requests", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personMatchingRequestsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personMatchingRequestsController.ControllerContext;
            var actionDescriptor = personMatchingRequestsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                personMatchingRequestsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                .Throws(new PermissionsException("User 'npuser' does not have permission to view person-matching-requests."));
                await personMatchingRequestsController.GetPersonMatchingRequestsByGuidAsync(expectedGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Get v1.0.0
        //Successful
        //GetPersonMatchingRequestsAsync
        [TestMethod]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonMatchingRequests" },
                    { "action", "GetPersonMatchingRequestsAsync" }
                };
            HttpRoute route = new HttpRoute("person-matching-requests", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personMatchingRequestsController.Request.SetRouteData(data);
            personMatchingRequestsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewPersonMatchRequest, BasePermissionCodes.CreatePersonMatchRequestProspects });

            var controllerContext = personMatchingRequestsController.ControllerContext;
            var actionDescriptor = personMatchingRequestsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<Dtos.PersonMatchingRequests>, int>(personMatchingRequestsCollection, 2);

            personMatchingRequestsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>())).ReturnsAsync(tuple);
            var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            Object filterObject;
            personMatchingRequestsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewPersonMatchRequest));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.CreatePersonMatchRequestProspects));


        }

        //Get v1.0.0
        //Exception
        //GetPersonMatchingRequestsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_GetPersonMatchingRequestsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonMatchingRequests" },
                    { "action", "GetPersonMatchingRequestsAsync" }
                };
            HttpRoute route = new HttpRoute("person-matching-requests", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personMatchingRequestsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personMatchingRequestsController.ControllerContext;
            var actionDescriptor = personMatchingRequestsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personMatchingRequestsServiceMock.Setup(x => x.GetPersonMatchingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchingRequests>(), person, It.IsAny<bool>())).Throws<PermissionsException>();
                personMatchingRequestsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                .Throws(new PermissionsException("User 'npuser' does not have permission to view person-matching-requests."));
                var personMatchingRequests = await personMatchingRequestsController.GetPersonMatchingRequestsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //POST v1.0.0
        //Successful
        //PostPersonMatchingRequestsInitiationsProspectsAsync
        [TestMethod]
        public async Task PersonMatchingRequestsController_PostPersonMatchingRequestsInitiationsProspectsAsync_Permissions()
        {
            var expected = personMatchingRequestsCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonMatchingRequests" },
                    { "action", "PostPersonMatchingRequestsInitiationsProspectsAsync" }
                };
            HttpRoute route = new HttpRoute("person-matching-requests", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personMatchingRequestsController.Request.SetRouteData(data);
            personMatchingRequestsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.CreatePersonMatchRequestProspects);

            var controllerContext = personMatchingRequestsController.ControllerContext;
            var actionDescriptor = personMatchingRequestsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personMatchingRequestsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personMatchingRequestsServiceMock.Setup(x => x.CreatePersonMatchingRequestsInitiationsProspectsAsync(It.IsAny<Dtos.PersonMatchingRequestsInitiationsProspects>())).ReturnsAsync(expected);
            var actual = await personMatchingRequestsController.PostPersonMatchingRequestsInitiationsProspectsAsync(personMatchingRequestsInitiationsProspectsCollection.FirstOrDefault());

            Object filterObject;
            personMatchingRequestsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.CreatePersonMatchRequestProspects));


        }

        //POST v1.0.0
        //Exception
        //PostPersonMatchingRequestsInitiationsProspectsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonMatchingRequestsController_PostPersonMatchingRequestsInitiationsProspectsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonMatchingRequests" },
                    { "action", "PostPersonMatchingRequestsInitiationsProspectsAsync" }
                };
            HttpRoute route = new HttpRoute("person-matching-requests", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personMatchingRequestsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personMatchingRequestsController.ControllerContext;
            var actionDescriptor = personMatchingRequestsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personMatchingRequestsServiceMock.Setup(x => x.CreatePersonMatchingRequestsInitiationsProspectsAsync(It.IsAny<Dtos.PersonMatchingRequestsInitiationsProspects>())).Throws<PermissionsException>();
                personMatchingRequestsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                .Throws(new PermissionsException("User 'npuser' does not have permission to create person-matching-requests."));
                var actual = await personMatchingRequestsController.PostPersonMatchingRequestsInitiationsProspectsAsync(personMatchingRequestsInitiationsProspectsCollection.FirstOrDefault());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion
    }
}
