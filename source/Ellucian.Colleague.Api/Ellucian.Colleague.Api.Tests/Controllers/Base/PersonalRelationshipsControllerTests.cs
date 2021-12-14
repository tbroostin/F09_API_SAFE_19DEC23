// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonalRelationshipsControllerTests
    {
        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }

        private PersonalRelationshipsController personalRelationshipsController;
        Mock<ILogger> loggerMock = new Mock<ILogger>();
        IAdapterRegistry AdapterRegistry;
        Mock<IPersonalRelationshipsService> personalRelationshipsServiceeMock = new Mock<IPersonalRelationshipsService>();

        Ellucian.Colleague.Dtos.PersonalRelationship personalRelationship;
        List<Dtos.PersonalRelationship> personalRelationshipsDtos = new List<PersonalRelationship>();
        Tuple<IEnumerable<Dtos.PersonalRelationship>, int> personalRelationshipTuple;
        private Paging page;
        private int limit;
        private int offset;

        string id = "9eeb5365-9478-4b40-8463-1e1d0ecf8956";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);

            personalRelationshipsDtos = new TestPersonalRelationshipsRepository().GetPersonalRelationships().ToList();
            limit = 3;
            offset = 0;
            page = new Paging(limit, offset);

            personalRelationshipTuple = new Tuple<IEnumerable<PersonalRelationship>,int>(personalRelationshipsDtos, 3);

            personalRelationshipsController = new PersonalRelationshipsController(AdapterRegistry, personalRelationshipsServiceeMock.Object, loggerMock.Object);
            personalRelationshipsController.Request = new HttpRequestMessage();
            personalRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

        }

        [TestCleanup]
        public void Cleanup()
        {
            personalRelationshipsController = null;
            personalRelationship = null;
        }       

        #region Exceptions Testing

        #region GET

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_ArgumentNullException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentNullException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_PermissionsException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_RepositoryException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_IntegrationApiException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_KeyNotFoundException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_Exception()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_ArgumentNullException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentNullException());
            var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_InvalidOperationException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException());
            var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_KeyNotFoundException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException());
            var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_Exception()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());
            var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync("1234");
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync()
        {
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personalRelationshipsController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personalRelationshipsServiceeMock.Setup(x => x.GetAllPersonalRelationshipsAsync(page.Offset, page.Limit, It.IsAny<bool>())).ReturnsAsync(personalRelationshipTuple);

            var results = await personalRelationshipsController.GetPersonalRelationshipsAsync(page);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonalRelationship> personalRelationshipResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationship>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonalRelationship>;

            Assert.AreEqual(personalRelationshipsDtos.Count(), 3);
            Assert.AreEqual(personalRelationshipResults.Count(), 3);
            int resultCounts = personalRelationshipResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personalRelationshipsDtos[i];
                var actual = personalRelationshipResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.DirectRelationship.Detail.Id, actual.DirectRelationship.Detail.Id);
                Assert.AreEqual(expected.DirectRelationship.RelationshipType, actual.DirectRelationship.RelationshipType);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.PersonalRelationshipStatus.Id, actual.PersonalRelationshipStatus.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.Detail.Id, actual.ReciprocalRelationship.Detail.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.RelationshipType, actual.ReciprocalRelationship.RelationshipType);
                Assert.AreEqual(expected.RelatedPerson.Id, actual.RelatedPerson.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
            }
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_WithNullPage()
        {
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personalRelationshipsController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personalRelationshipsServiceeMock.Setup(x => x.GetAllPersonalRelationshipsAsync(0, 200, It.IsAny<bool>())).ReturnsAsync(personalRelationshipTuple);

            var results = await personalRelationshipsController.GetPersonalRelationshipsAsync(null);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonalRelationship> personalRelationshipResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationship>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonalRelationship>;

            Assert.AreEqual(personalRelationshipsDtos.Count(), 3);
            Assert.AreEqual(personalRelationshipResults.Count(), 3);
            int resultCounts = personalRelationshipResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personalRelationshipsDtos[i];
                var actual = personalRelationshipResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.DirectRelationship.Detail.Id, actual.DirectRelationship.Detail.Id);
                Assert.AreEqual(expected.DirectRelationship.RelationshipType, actual.DirectRelationship.RelationshipType);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.PersonalRelationshipStatus.Id, actual.PersonalRelationshipStatus.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.Detail.Id, actual.ReciprocalRelationship.Detail.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.RelationshipType, actual.ReciprocalRelationship.RelationshipType);
                Assert.AreEqual(expected.RelatedPerson.Id, actual.RelatedPerson.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
            }
        }

        [TestMethod]
        public async Task PersonVisasController_GetPersonalRelationshipByIdAsync()
        {
            var expected = personalRelationshipsDtos[1];
            personalRelationshipsServiceeMock.Setup(x => x.GetPersonalRelationshipByIdAsync(id)).ReturnsAsync(expected);

            var actual = await personalRelationshipsController.GetPersonalRelationshipByIdAsync(id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Comment, actual.Comment);
            Assert.AreEqual(expected.DirectRelationship.Detail.Id, actual.DirectRelationship.Detail.Id);
            Assert.AreEqual(expected.DirectRelationship.RelationshipType, actual.DirectRelationship.RelationshipType);
            Assert.AreEqual(expected.EndOn, actual.EndOn);
            Assert.AreEqual(expected.PersonalRelationshipStatus.Id, actual.PersonalRelationshipStatus.Id);
            Assert.AreEqual(expected.ReciprocalRelationship.Detail.Id, actual.ReciprocalRelationship.Detail.Id);
            Assert.AreEqual(expected.ReciprocalRelationship.RelationshipType, actual.ReciprocalRelationship.RelationshipType);
            Assert.AreEqual(expected.RelatedPerson.Id, actual.RelatedPerson.Id);
            Assert.AreEqual(expected.StartOn, actual.StartOn);
            Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync()
        {
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            personalRelationshipsServiceeMock.Setup(x => x.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(personalRelationshipTuple);

            var results = await personalRelationshipsController.GetPersonalRelationshipsAsync(null, "test");

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonalRelationship> personalRelationshipResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationship>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonalRelationship>;

            Assert.AreEqual(personalRelationshipsDtos.Count(), 3);
            Assert.AreEqual(personalRelationshipResults.Count(), 3);
            int resultCounts = personalRelationshipResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personalRelationshipsDtos[i];
                var actual = personalRelationshipResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.DirectRelationship.Detail.Id, actual.DirectRelationship.Detail.Id);
                Assert.AreEqual(expected.DirectRelationship.RelationshipType, actual.DirectRelationship.RelationshipType);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.PersonalRelationshipStatus.Id, actual.PersonalRelationshipStatus.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.Detail.Id, actual.ReciprocalRelationship.Detail.Id);
                Assert.AreEqual(expected.ReciprocalRelationship.RelationshipType, actual.ReciprocalRelationship.RelationshipType);
                Assert.AreEqual(expected.RelatedPerson.Id, actual.RelatedPerson.Id);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.SubjectPerson.Id, actual.SubjectPerson.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_PermissionException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new PermissionsException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_ArgumentException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_RepositoryException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new RepositoryException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_IntegrationApiException()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new IntegrationApiException());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetFilterPersonalRelationshipsAsync_Exception()
        {
            personalRelationshipsServiceeMock
                .Setup(i => i.GetPersonalRelationshipsByFilterAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());
            var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()), "test");
        }

        //GET v6
        //Successful
        //GetPersonalRelationshipsAsync
        [TestMethod]
        public async Task StudentMealPlansController_GetPersonalRelationshipsAsync_Permissions()
        {
            
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetPersonalRelationshipsAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyRelationship, BasePermissionCodes.UpdatePersonalRelationship });

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personalRelationshipsServiceeMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personalRelationshipsServiceeMock.Setup(x => x.GetAllPersonalRelationshipsAsync(page.Offset, page.Limit, It.IsAny<bool>())).ReturnsAsync(personalRelationshipTuple);
            var results = await personalRelationshipsController.GetPersonalRelationshipsAsync(page);

            Object filterObject;
            personalRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAnyRelationship));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonalRelationship));


        }

        //GET v6
        //Exception
        //GetPersonalRelationshipsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetPersonalRelationshipsAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personalRelationshipsServiceeMock.Setup(i => i.GetAllPersonalRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                personalRelationshipsServiceeMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view personal-relationships."));
                var result = await personalRelationshipsController.GetPersonalRelationshipsAsync(new Web.Http.Models.Paging(It.IsAny<int>(), It.IsAny<int>()));
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET v6
        //Successful
        //GetPersonalRelationshipByIdAsync
        [TestMethod]
        public async Task StudentMealPlansController_GetPersonalRelationshipByIdAsync_Permissions()
        {
            var expected = personalRelationshipsDtos[1];
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetPersonalRelationshipByIdAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyRelationship, BasePermissionCodes.UpdatePersonalRelationship });

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personalRelationshipsServiceeMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personalRelationshipsServiceeMock.Setup(x => x.GetPersonalRelationshipByIdAsync(id)).ReturnsAsync(expected);
            var actual = await personalRelationshipsController.GetPersonalRelationshipByIdAsync(id);

            Object filterObject;
            personalRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAnyRelationship));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonalRelationship));


        }

        //GET v6
        //Exception
        //GetPersonalRelationshipByIdAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationshipByIdAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetPersonalRelationshipByIdAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personalRelationshipsServiceeMock.Setup(i => i.GetPersonalRelationshipByIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                personalRelationshipsServiceeMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view personal-relationships."));
                var result = await personalRelationshipsController.GetPersonalRelationshipByIdAsync("1234");
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion

        #region POST
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_PostPersonalRelationshipAsync_Exception()
        {
            await personalRelationshipsController.PostPersonalRelationshipAsync(It.IsAny<PersonalRelationship>());
        }
        #endregion

        #region PUT
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_PutPersonalRelationshipAsync_Exception()
        {
            await personalRelationshipsController.PutPersonalRelationshipAsync(It.IsAny<string>(), It.IsAny<PersonalRelationship>());
        }
        #endregion

        

        #endregion
    }

    [TestClass]
    public class PersonalRelationshipsControllerv16Tests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPersonalRelationshipsService> personalRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        IAdapterRegistry AdapterRegistry;
        private PersonalRelationshipsController personalRelationshipsController;
        private IEnumerable<Domain.Base.Entities.Relationship> allRelationship;
        private List<Dtos.PersonalRelationships2> personalRelationshipsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            personalRelationshipsServiceMock = new Mock<IPersonalRelationshipsService>();
            loggerMock = new Mock<ILogger>();
            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);
            personalRelationshipsCollection = new List<Dtos.PersonalRelationships2>();

            allRelationship = new List<Domain.Base.Entities.Relationship>()
                {
                    new Domain.Base.Entities.Relationship("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "d2253ac7-9931-4560-b42f-1fccd43c952e", "Athletic", false, DateTime.Now, DateTime.Now) { Guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", SubjectPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"},
                    new Domain.Base.Entities.Relationship("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "Academic", true, DateTime.Now, DateTime.Now) { Guid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", SubjectPersonGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"},
                    new Domain.Base.Entities.Relationship("d2253ac7-9931-4560-b42f-1fccd43c952e", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "Cultural", true, DateTime.Now, DateTime.Now) { Guid = "d2253ac7-9931-4560-b42f-1fccd43c952e", SubjectPersonGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"}
                };

            foreach (var source in allRelationship)
            {
                var personalRelationships = new Ellucian.Colleague.Dtos.PersonalRelationships2
                {
                    Id = source.Guid,
                    SubjectPerson = new GuidObject2(source.SubjectPersonGuid),
                    Related = new PersonalRelationshipsRelatedPerson() { person = new GuidObject2(source.RelationPersonGuid) },
                    StartOn = DateTime.Now,
                    EndOn = DateTime.Now
                };
                personalRelationshipsCollection.Add(personalRelationships);
            }
            personalRelationshipsController = new PersonalRelationshipsController(AdapterRegistry, personalRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            personalRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            personalRelationshipsController = null;
            allRelationship = null;
            personalRelationshipsCollection = null;
            loggerMock = null;
            personalRelationshipsServiceMock = null;
            AdapterRegistry = null;
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2_ValidateFields_Nocache()
        {
            personalRelationshipsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };

            int Offset = 0;
            int Limit = 4;
            var PersonalRelationshipsTuple =
                new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(personalRelationshipsCollection.Take(4), personalRelationshipsCollection.Count());

            personalRelationshipsServiceMock.Setup(i => i.GetPersonalRelationships2Async(Offset, Limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(PersonalRelationshipsTuple);

            Paging paging = new Paging(Limit, Offset);
            var actuals = await personalRelationshipsController.GetPersonalRelationships2Async(paging);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonalRelationships2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonalRelationships2>;

            Assert.IsNotNull(results);
            Assert.AreEqual(3, results.Count());

            foreach (var actual in results)
            {
                var expected = personalRelationshipsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.SubjectPerson, actual.SubjectPerson);
                Assert.AreEqual(expected.Related.person.Id, actual.Related.person.Id);

                if (expected.StartOn.Value != null)
                {
                    Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
                }
                if (expected.EndOn.Value != null)
                {
                    Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                }
            }
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2_ValidateFields_Cache()
        {
            personalRelationshipsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            int Offset = 0;
            int Limit = 4;
            var PersonalRelationshipsTuple =
                new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(personalRelationshipsCollection.Take(4), personalRelationshipsCollection.Count());

            personalRelationshipsServiceMock.Setup(i => i.GetPersonalRelationships2Async(Offset, Limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(PersonalRelationshipsTuple);

            Paging paging = new Paging(Limit, Offset);
            var actuals = await personalRelationshipsController.GetPersonalRelationships2Async(paging);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonalRelationships2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonalRelationships2>;

            Assert.IsNotNull(results);
            Assert.AreEqual(3, results.Count());

            foreach (var actual in results)
            {
                var expected = personalRelationshipsCollection.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.SubjectPerson, actual.SubjectPerson);
                Assert.AreEqual(expected.Related.person.Id, actual.Related.person.Id);

                if (expected.StartOn.Value != null)
                {
                    Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
                }
                if (expected.EndOn.Value != null)
                {
                    Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2_KeyNotFoundException()
        {
            //
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personalRelationshipsController.GetPersonalRelationships2Async(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2_PermissionsException()
        {

            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personalRelationshipsController.GetPersonalRelationships2Async(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2_ArgumentException()
        {

            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personalRelationshipsController.GetPersonalRelationships2Async(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2_RepositoryException()
        {

            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personalRelationshipsController.GetPersonalRelationships2Async(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2_IntegrationApiException()
        {

            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personalRelationshipsController.GetPersonalRelationships2Async(It.IsAny<Paging>());
        }

        [TestMethod]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuidAsync_ValidateFields()
        {
            var expected = personalRelationshipsCollection.FirstOrDefault();
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.SubjectPerson, actual.SubjectPerson);
            Assert.AreEqual(expected.Related.person.Id, actual.Related.person.Id);

            if (expected.StartOn.Value != null)
            {
                Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
            }
            if (expected.EndOn.Value != null)
            {
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2_Exception()
        {
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personalRelationshipsController.GetPersonalRelationships2Async(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuidAsync_Exception()
        {
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuid_KeyNotFoundException()
        {
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuid_PermissionsException()
        {
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuid_ArgumentException()
        {
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuid_RepositoryException()
        {
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuid_IntegrationApiException()
        {
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuid_Exception()
        {
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expectedGuid);
        }

        //GET v16.0.0
        //Successful
        //GetPersonalRelationships2Async
        [TestMethod]
        public async Task StudentMealPlansController_GetPersonalRelationships2Async_Permissions()
        {
            int Offset = 0;
            int Limit = 4; 
            var PersonalRelationshipsTuple = new Tuple<IEnumerable<Dtos.PersonalRelationships2>, int>(personalRelationshipsCollection.Take(4), personalRelationshipsCollection.Count());
            Paging paging = new Paging(Limit, Offset);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetPersonalRelationships2Async" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyRelationship, BasePermissionCodes.UpdatePersonalRelationship });

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            
            personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personalRelationshipsServiceMock.Setup(i => i.GetPersonalRelationships2Async(Offset, Limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(PersonalRelationshipsTuple);
            var actuals = await personalRelationshipsController.GetPersonalRelationships2Async(paging);

            Object filterObject;
            personalRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAnyRelationship));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonalRelationship));


        }

        //GET v16.0.0
        //Exception
        //GetPersonalRelationships2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetPersonalRelationships2Async" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view personal-relationships."));
                await personalRelationshipsController.GetPersonalRelationships2Async(It.IsAny<Paging>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET by id v16.0.0
        //Successful
        //GetPersonalRelationships2ByGuidAsync
        [TestMethod]
        public async Task StudentMealPlansController_GetPersonalRelationships2ByGuidAsync_Permissions()
        {
            var expected = personalRelationshipsCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetPersonalRelationships2ByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyRelationship, BasePermissionCodes.UpdatePersonalRelationship });

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            var actual = await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expected.Id);

            Object filterObject;
            personalRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAnyRelationship));
            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonalRelationship));


        }

        //GET by id v16.0.0
        //Exception
        //GetPersonalRelationships2ByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetPersonalRelationships2ByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetPersonalRelationships2ByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personalRelationshipsServiceMock.Setup(x => x.GetPersonalRelationships2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view personal-relationships."));
                await personalRelationshipsController.GetPersonalRelationships2ByGuidAsync(expectedGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

    }

    [TestClass]
    public class PersonalRelationshipsControllerTests_POST_V16
    {
        #region DECLARATION

        public TestContext TestContext { get; set; }

        private Mock<IPersonalRelationshipsService> personalRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        IAdapterRegistry AdapterRegistry;
        private PersonalRelationshipsController personalRelationshipsController;

        private PersonalRelationships2 personalRelationships;

        private string guid = "1adc2629-e8a7-410e-b4df-572d02822f8b";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);
            personalRelationshipsServiceMock = new Mock<IPersonalRelationshipsService>();

            InitializeTestData();

            personalRelationshipsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            personalRelationshipsController = new PersonalRelationshipsController(AdapterRegistry, personalRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            personalRelationshipsServiceMock = null;
            personalRelationshipsController = null;
            AdapterRegistry = null;
        }

        private void InitializeTestData()
        {
            personalRelationships = new PersonalRelationships2()
            {
                Id = guid
            };
        }

        #endregion        

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonalRelationships2Async_KeyNotFoundException()
        {
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new KeyNotFoundException());
            await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonalRelationships2Async_PermissionsException()
        {
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new PermissionsException());
            await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonalRelationships2Async_ArgumentException()
        {
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new ArgumentException());
            await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonalRelationships2Async_RepositoryException()
        {
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new RepositoryException());
            await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonalRelationships2Async_IntegrationApiException()
        {
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new IntegrationApiException());
            await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonalRelationships2Async_ConfigurationException()
        {
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new ConfigurationException());
            await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PostPersonalRelationships2Async_Exception()
        {
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new Exception());
            await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        public async Task PersonRelController_PostPersonalRelationships2Async()
        {
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ReturnsAsync(personalRelationships);
            var result = await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });

            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
        }
    }

    [TestClass]
    public class PersonalRelationshipsControllerTests_PUT_V16
    {
        #region DECLARATION

        public TestContext TestContext { get; set; }

        private Mock<IPersonalRelationshipsService> personalRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        IAdapterRegistry AdapterRegistry;
        private PersonalRelationshipsController personalRelationshipsController;

        private PersonalRelationships2 personalRelationships;

        private string guid = "1adc2629-e8a7-410e-b4df-572d02822f8b";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);
            personalRelationshipsServiceMock = new Mock<IPersonalRelationshipsService>();

            InitializeTestData();

            personalRelationshipsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            personalRelationshipsController = new PersonalRelationshipsController(AdapterRegistry, personalRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            personalRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            personalRelationshipsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(personalRelationships));
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            personalRelationshipsServiceMock = null;
            personalRelationshipsController = null;
            AdapterRegistry = null;
        }

        private void InitializeTestData()
        {
            personalRelationships = new PersonalRelationships2()
            {
                Id = guid,
                Comment = "Comment"
            };
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonalRelationships2Async_PermissionsException()
        {
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new PermissionsException());
            await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonalRelationships2Async_ArgumentException()
        {
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new ArgumentException());
            await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonalRelationshipsAsync_RepositoryException()
        {
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new RepositoryException());
            await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonalRelationshipsAsync_IntegrationApiException()
        {
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new IntegrationApiException());
            await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonalRelationshipsAsync_ConfigurationException()
        {
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new ConfigurationException());
            await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonalRelationshipsAsync_KeyNotFoundException()
        {
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new KeyNotFoundException());
            await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_PutPersonalRelationshipsAsync_Exception()
        {
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new Exception());
            await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { Id = guid });
        }

        [TestMethod]
        public async Task PersonRelController_PutPersonalRelationshipsAsync()
        {
            personalRelationships.Comment = "Updated Comment";
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ReturnsAsync(personalRelationships);

            var result = await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { Id = guid });

            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
            Assert.AreEqual("Updated Comment", result.Comment);
        }

        //PUT v16.0.0
        //Successful
        //PutPersonalRelationships2Async
        [TestMethod]
        public async Task StudentMealPlansController_PutPersonalRelationships2Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "PutPersonalRelationships2Async" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter( BasePermissionCodes.UpdatePersonalRelationship );

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ReturnsAsync(personalRelationships);
            var result = await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { Id = guid });

            Object filterObject;
            personalRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonalRelationship));


        }

        //PUT v16.0.0
        //Exception
        //PutPersonalRelationships2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_PutPersonalRelationships2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "PutPersonalRelationships2Async" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personalRelationshipsServiceMock.Setup(s => s.UpdatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new PermissionsException());
                personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update personal-relationships."));
                await personalRelationshipsController.PutPersonalRelationships2Async(guid, new PersonalRelationships2() { });
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //POST v16.0.0
        //Successful
        //PostPersonalRelationships2Async
        [TestMethod]
        public async Task StudentMealPlansController_PostPersonalRelationships2Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "PostPersonalRelationships2Async" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.UpdatePersonalRelationship);

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ReturnsAsync(personalRelationships);
            var result = await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });

            Object filterObject;
            personalRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.UpdatePersonalRelationship));


        }

        //POST v16.0.0
        //Exception
        //PostPersonalRelationships2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_PostPersonalRelationships2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "PostPersonalRelationships2Async" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personalRelationshipsServiceMock.Setup(s => s.CreatePersonalRelationships2Async(It.IsAny<PersonalRelationships2>())).ThrowsAsync(new PermissionsException());
                personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create personal-relationships."));
                await personalRelationshipsController.PostPersonalRelationships2Async(new PersonalRelationships2() { Id = Guid.Empty.ToString() });
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


    }

    [TestClass]
    public class PersonalRelationshipsControllerTests_DELETE_V16
    {
        #region DECLARATION

        public TestContext TestContext { get; set; }

        private Mock<IPersonalRelationshipsService> personalRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        IAdapterRegistry AdapterRegistry;
        private PersonalRelationshipsController personalRelationshipsController;

        private PersonalRelationships2 personalRelationships;

        private string guid = "1adc2629-e8a7-410e-b4df-572d02822f8b";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);
            personalRelationshipsServiceMock = new Mock<IPersonalRelationshipsService>();

            personalRelationshipsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            personalRelationshipsController = new PersonalRelationshipsController(AdapterRegistry, personalRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            personalRelationshipsServiceMock = null;
            personalRelationshipsController = null;
            AdapterRegistry = null;
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonalRelationshipsAsync_Guid_Null()
        {
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonalRelationshipsAsync_PermissionsException()
        {
            personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Throws(new PermissionsException());
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonalRelationshipsAsync_KeyNotFoundException()
        {
            personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Throws(new KeyNotFoundException());
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonalRelationshipsAsync_IntegrationApiException()
        {
            personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Throws(new IntegrationApiException());
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonalRelationshipsAsync_ArgumentException()
        {
            personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Throws(new ArgumentException());
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonalRelationshipsAsync_InvalidOperationException()
        {
            personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Throws(new InvalidOperationException());
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonalRelationshipsAsync_RepositoryException()
        {
            personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Throws(new RepositoryException());
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonRelController_DeletePersonalRelationshipsAsync_Exception()
        {
            personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Throws(new Exception());
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);
        }

        //DELETE
        //Successful
        //DeletePersonalRelationshipsAsync
        [TestMethod]
        public async Task StudentMealPlansController_DeletePersonalRelationshipsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "DeletePersonalRelationshipsAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);
            personalRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(BasePermissionCodes.DeletePersonalRelationship);

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Returns(Task.FromResult(new TaskStatus()));
            await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);

            Object filterObject;
            personalRelationshipsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.DeletePersonalRelationship));


        }

        //DELETE
        //Exception
        //DeletePersonalRelationshipsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_DeletePersonalRelationshipsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "DeletePersonalRelationshipsAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            personalRelationshipsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = personalRelationshipsController.ControllerContext;
            var actionDescriptor = personalRelationshipsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                personalRelationshipsServiceMock.Setup(s => s.DeletePersonalRelationshipsAsync(It.IsAny<String>())).Throws(new PermissionsException());
                personalRelationshipsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to delete personal-relationships."));
                await personalRelationshipsController.DeletePersonalRelationshipsAsync(guid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

    }
}
