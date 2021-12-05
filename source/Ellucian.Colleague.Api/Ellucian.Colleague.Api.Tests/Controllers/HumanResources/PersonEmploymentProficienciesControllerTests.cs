// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PersonEmploymentProficienciesControllerTests_V10
    {
        [TestClass]
        public class PersonEmploymentProficienciesController_GET_GET_BY_ID
        {
            private TestContext testContextInstance;

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

            #region DECLARATIONS

            private Mock<IPersonEmploymentProficienciesService> serviceMock;
            private Mock<ILogger> loggerMock;

            private PersonEmploymentProficienciesController controller;
            private int offset = 0, limit = 10;

            private Tuple<IEnumerable<Dtos.PersonEmploymentProficiencies>, int> tupleResult;
            private List<Dtos.PersonEmploymentProficiencies> personEmploymentProficiencies;

            private string guid = "5a1a02c4-21da-4cbb-98f1-bfd47cba87cd";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                serviceMock = new Mock<IPersonEmploymentProficienciesService>();
                loggerMock = new Mock<ILogger>();

                InitializeTestData();

                controller = new PersonEmploymentProficienciesController(loggerMock.Object, serviceMock.Object) { Request = new HttpRequestMessage() };

                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                controller.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                serviceMock = null;
                loggerMock = null;
                controller = null;
            }

            private void InitializeTestData()
            {
                personEmploymentProficiencies = new List<Dtos.PersonEmploymentProficiencies>()
                {
                    new Dtos.PersonEmploymentProficiencies()
                    {
                        Id = "1"
                    }
                };
                tupleResult = new Tuple<IEnumerable<Dtos.PersonEmploymentProficiencies>, int>(personEmploymentProficiencies, 1);
            }

            #endregion

            [TestMethod]
            public async Task GetPersonEmploymentProficienciesAsync_ValidateFields_Nocache()
            {
                controller.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false, Public = true };

                serviceMock.Setup(x => x.GetPersonEmploymentProficienciesAsync(offset, limit, false)).ReturnsAsync(tupleResult);

                var results = await controller.GetPersonEmploymentProficienciesAsync(new Paging(limit, offset));

                Assert.IsNotNull(results);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonEmploymentProficiencies>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonEmploymentProficiencies>;

                Assert.AreEqual(personEmploymentProficiencies.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = personEmploymentProficiencies.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                }
            }

            [TestMethod]
            public async Task GetPersonEmploymentProficienciesAsync_ValidateFields_Cache()
            {
                controller.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, Public = true };

                serviceMock.Setup(x => x.GetPersonEmploymentProficienciesAsync(offset, limit, true)).ReturnsAsync(tupleResult);

                var results = await controller.GetPersonEmploymentProficienciesAsync(new Paging(limit, offset));

                Assert.IsNotNull(results);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonEmploymentProficiencies>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonEmploymentProficiencies>;

                Assert.AreEqual(personEmploymentProficiencies.Count(), actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = personEmploymentProficiencies.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesAsync_KeyNotFoundException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new KeyNotFoundException());

                await controller.GetPersonEmploymentProficienciesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesAsync_PermissionsException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new PermissionsException());

                await controller.GetPersonEmploymentProficienciesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesAsync_ArgumentException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new ArgumentException());

                await controller.GetPersonEmploymentProficienciesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesAsync_RepositoryException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new RepositoryException());

                await controller.GetPersonEmploymentProficienciesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesAsync_IntegrationApiException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new IntegrationApiException());

                await controller.GetPersonEmploymentProficienciesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesAsync_Exception()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new Exception());

                await controller.GetPersonEmploymentProficienciesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_IntegrationApiException_Guid_Null()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

                await controller.GetPersonEmploymentProficienciesByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_KeyNotFoundException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_PermissionsException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

                await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_ArgumentException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_RepositoryException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_IntegrationApiException()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

                await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_Exception()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            public async Task GetPersonEmploymentProficienciesByGuidAsync()
            {
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ReturnsAsync(personEmploymentProficiencies.FirstOrDefault());

                var result = await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostPersonEmploymentProficienciesAsync_IntegrationApiException_NotSupported()
            {
                await controller.PostPersonEmploymentProficienciesAsync(new Dtos.PersonEmploymentProficiencies() { });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutPersonEmploymentProficienciesAsync_IntegrationApiException_NotSupported()
            {
                await controller.PutPersonEmploymentProficienciesAsync(guid, new Dtos.PersonEmploymentProficiencies() { });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DeletePersonEmploymentProficienciesAsync_IntegrationApiException_NotSupported()
            {
                await controller.DeletePersonEmploymentProficienciesAsync(guid);
            }


            //Permissions

            // Success
            // Get 10
            //GetPersonEmploymentProficienciesAsync
            [TestMethod]
            public async Task PersonEmploymentProficienciesController_GetPersonEmploymentProficienciesAsync_Permissions()
            {
                int offset = 0;
                int limit = 10;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmploymentProficiencies" },
                    { "action", "GetPersonEmploymentProficienciesAsync" }
                };
                HttpRoute route = new HttpRoute("person-employment-proficiencies", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                controller.Request.SetRouteData(data);
                controller.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewPersonEmpProficiencies);

                var controllerContext = controller.ControllerContext;
                var actionDescriptor = controller.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var cancelToken = new System.Threading.CancellationToken(false);

                var tupleResult = new Tuple<IEnumerable<Dtos.PersonEmploymentProficiencies>, int>(personEmploymentProficiencies, 1);

                serviceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                serviceMock.Setup(x => x.GetPersonEmploymentProficienciesAsync(offset, limit, false)).ReturnsAsync(tupleResult);
                var res = await controller.GetPersonEmploymentProficienciesAsync(new Paging(10, 0));

                Object filterObject;
                controller.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewPersonEmpProficiencies));

            }


            // Exception
            // Get 10
            //GetPayrollDeductionArrangementsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonEmploymentProficienciesController_GetPersonEmploymentProficienciesAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmploymentProficiencies" },
                    { "action", "GetPersonEmploymentProficienciesAsync" }
                };
                HttpRoute route = new HttpRoute("person-employment-proficiencies", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                controller.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = controller.ControllerContext;
                var actionDescriptor = controller.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.PersonEmploymentProficiencies>, int>(personEmploymentProficiencies, 5);

                    serviceMock.Setup(s => s.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new PermissionsException());
                    serviceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view person-employment-proficiencies."));
                    var resp = await controller.GetPersonEmploymentProficienciesAsync(null);

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Permissions

            // Success
            // Get By Id 10
            //GetPersonEmploymentProficienciesByGuidAsync
            [TestMethod]
            public async Task PersonEmploymentProficienciesController_GetPersonEmploymentProficienciesByGuidAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmploymentProficiencies" },
                    { "action", "GetPersonEmploymentProficienciesByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("person-employment-proficiencies", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                controller.Request.SetRouteData(data);
                controller.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewPersonEmpProficiencies);

                var controllerContext = controller.ControllerContext;
                var actionDescriptor = controller.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.PersonEmploymentProficiencies>, int>(personEmploymentProficiencies, 1);
                serviceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ReturnsAsync(personEmploymentProficiencies.FirstOrDefault());
                var result = await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);

                Object filterObject;
                controller.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewPersonEmpProficiencies));

            }


            // Exception
            // Get By Id 10
            //GetPersonEmploymentProficienciesByGuidAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonEmploymentProficienciesController_GetPersonEmploymentProficienciesByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonEmploymentProficiencies" },
                    { "action", "GetPersonEmploymentProficienciesByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("person-employment-proficiencies", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                controller.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = controller.ControllerContext;
                var actionDescriptor = controller.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.PersonEmploymentProficiencies>, int>(personEmploymentProficiencies, 5);

                    serviceMock.Setup(s => s.GetPersonEmploymentProficienciesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                    serviceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view person-employment-proficiencies."));
                    var resp = await controller.GetPersonEmploymentProficienciesByGuidAsync(guid);

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

        }
    }
}
