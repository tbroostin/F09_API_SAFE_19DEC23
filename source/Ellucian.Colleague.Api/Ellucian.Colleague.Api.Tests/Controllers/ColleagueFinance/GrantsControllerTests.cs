// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class GrantsControllerTests
    {
        [TestClass]
        public class GrantsControllerTests_V11 {

            [TestClass]
            public class GrantsControllerTests_GET {

                #region DECLARATION

                public TestContext TestContext { get; set; }

                private GrantsController grantsController;
                private Mock<IGrantsService> grantsServiceMock;
                private Mock<ILogger> loggerMock;

                private IEnumerable<Dtos.Grant> grantCollection;
                private Tuple<IEnumerable<Dtos.Grant>, int> grantTuple;

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                    EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                    grantsServiceMock = new Mock<IGrantsService>();
                    loggerMock = new Mock<ILogger>();

                    grantsController = new GrantsController(grantsServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                    grantsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                    grantsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                    InitializeTestData();
                }

                [TestCleanup]
                public void Cleanup()
                {
                    grantsController = null;
                    grantsServiceMock = null;
                    loggerMock = null;
                    TestContext = null;
                }

                private void InitializeTestData()
                {
                    grantCollection = new List<Dtos.Grant>()
                    {
                        new Grant() { Id = "2a082180-b897-46f3-8435-df25caaca921" },
                        new Grant() { Id = "2a082180-b897-46f3-8435-df25caaca922" },
                        new Grant() { Id = "2a082180-b897-46f3-8435-df25caaca923" }
                    };

                    grantTuple = new Tuple<IEnumerable<Grant>, int>(grantCollection, 3);
                }

                #endregion

                #region CACHE-NOCHACHE

                [TestMethod]
                public async Task GrantsController_GetGrantsAsync_Nocache()
                {
                    grantsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };

                    grantsServiceMock.Setup(x => x.GetGrantsAsync(0, 100,It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(grantTuple);

                    var results = await grantsController.GetGrantsAsync(new Paging(100, 0));

                    Assert.IsNotNull(results);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.Grant> actuals =
                        ((ObjectContent<IEnumerable<Dtos.Grant>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Grant>;

                    Assert.AreEqual(grantCollection.Count(), actuals.Count());

                    foreach (var actual in actuals)
                    {
                        var expected = grantCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(expected);
                        Assert.AreEqual(expected.Id, actual.Id);
                    }
                }

                [TestMethod]
                public async Task GrantsController_GetGrantsAsync_Cache()
                {
                    grantsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

                    grantsServiceMock.Setup(x => x.GetGrantsAsync(0, 100,"", "", true)).ReturnsAsync(grantTuple);

                    var results = await grantsController.GetGrantsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

                    Assert.IsNotNull(results);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.Grant> actuals =
                        ((ObjectContent<IEnumerable<Dtos.Grant>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Grant>;

                    Assert.AreEqual(grantCollection.Count(), actuals.Count());

                    foreach (var actual in actuals)
                    {
                        var expected = grantCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(expected);
                        Assert.AreEqual(expected.Id, actual.Id);
                    }
                }

                [TestMethod]
                public async Task GrantsController_GetGrantsAsync_WithFilters()
                {
                    var filterGroupName = "criteria";

                    grantsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

                    grantsController.Request = new System.Net.Http.HttpRequestMessage()
                    {
                        RequestUri = new Uri("http://localhost"),

                    };
                    grantsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                        new Dtos.Grant() { ReportingSegment = "Ellucian University" });

                    grantsController.Request.Properties.Add(string.Format("FilterObject{0}", "fiscalYear"),
                        new Dtos.Filters.FiscalYearFilter() { FiscalYear = new GuidObject2("3a082180-b897-46f3-8435-df25caaca921") });

                    grantsServiceMock.Setup(x => x.GetGrantsAsync(0, 100, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(grantTuple);

                    QueryStringFilter fiscalYear = new QueryStringFilter("fiscalYear", "{'fiscalYear':{'id': '3a082180-b897-46f3-8435-df25caaca921'}}");
                    QueryStringFilter reportingSegment = new QueryStringFilter("reportingSegment", "{'reportingSegment':'Ellucian University'}");

                    var results = await grantsController.GetGrantsAsync(new Paging(100, 0), reportingSegment, fiscalYear);

                    Assert.IsNotNull(results);

                    var cancelToken = new CancellationToken(false);

                    HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

                    IEnumerable<Dtos.Grant> actuals =
                        ((ObjectContent<IEnumerable<Dtos.Grant>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Grant>;

                    Assert.AreEqual(grantCollection.Count(), actuals.Count());

                    foreach (var actual in actuals)
                    {
                        var expected = grantCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(expected);
                        Assert.AreEqual(expected.Id, actual.Id);
                    }
                }


                #endregion

                #region GETALL

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsAsync_KeyNotFoundException()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(),It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                    await grantsController.GetGrantsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsAsync_PermissionException()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync( new PermissionsException());
                    await grantsController.GetGrantsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsAsync_ArgumentException()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                    await grantsController.GetGrantsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsAsync_RepositoryException()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                    await grantsController.GetGrantsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsAsync_IntegrationApiException()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                    await grantsController.GetGrantsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsAsync_Exception()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                    await grantsController.GetGrantsAsync(null);
                }

                #endregion

                #region GETBYID

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsByGuidAsync_KeyNotFoundException()
                {
                    grantsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };
                    grantsServiceMock.Setup(e => e.GetGrantsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                    await grantsController.GetGrantsByGuidAsync("2a082180-b897-46f3-8435-df25caaca921");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsByGuidAsync_PermissionsException()
                {
                    grantsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                    grantsServiceMock.Setup(e => e.GetGrantsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    await grantsController.GetGrantsByGuidAsync("2a082180-b897-46f3-8435-df25caaca921");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsByGuidAsync_ArgumentException()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                    await grantsController.GetGrantsByGuidAsync("2a082180-b897-46f3-8435-df25caaca921");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsByGuidAsync_RepositoryException()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                    await grantsController.GetGrantsByGuidAsync("2a082180-b897-46f3-8435-df25caaca921");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsByGuidAsync_IntegrationApiException()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                    await grantsController.GetGrantsByGuidAsync("2a082180-b897-46f3-8435-df25caaca921");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsByGuidAsync_Exception()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                    await grantsController.GetGrantsByGuidAsync("2a082180-b897-46f3-8435-df25caaca921");
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_GetGrantsByGuidAsync_GuidAsNull()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                    await grantsController.GetGrantsByGuidAsync(null);
                }

                [TestMethod]
                public async Task GrantsController_GetGrantsByGuidAsync()
                {
                    grantsServiceMock.Setup(e => e.GetGrantsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(grantCollection.FirstOrDefault());
                    var result = await grantsController.GetGrantsByGuidAsync("2a082180-b897-46f3-8435-df25caaca921");

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Id, grantCollection.FirstOrDefault().Id);
                }

                [TestMethod]
                public async Task GrantsController_GetPersonsActiveHoldsAsync_Permissions()
                {
                    var contextPropertyName = "PermissionsFilter";

                    HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Grants" },
                        { "action", "GetGrantsAsync" }
                    };
                    HttpRoute route = new HttpRoute("person-holds", routeValueDict);
                    HttpRouteData data = new HttpRouteData(route);
                    grantsController.Request.SetRouteData(data);
                    grantsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                    var permissionsFilter = new PermissionsFilter(new string[] { ColleagueFinancePermissionCodes.ViewGrants });

                    var controllerContext = grantsController.ControllerContext;
                    var actionDescriptor = grantsController.ActionContext.ActionDescriptor
                             ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                    var _context = new HttpActionContext(controllerContext, actionDescriptor);
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    var tuple = new Tuple<IEnumerable<Dtos.Grant>, int>(grantCollection, 5);
                    grantsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                           .Returns(true);
                    grantsServiceMock.Setup(s => s.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                    var resp = await grantsController.GetGrantsAsync(new Paging(10, 0));

                    Object filterObject;
                    grantsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                    var cancelToken = new System.Threading.CancellationToken(false);
                    Assert.IsNotNull(filterObject);

                    var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                         .Select(x => x.ToString())
                                         .ToArray();

                    Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewGrants));

                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task PersonHoldsController_GetPersonsActiveHoldsAsync_Invalid_Permissions()
                {
                    HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Grants" },
                        { "action", "GetGrantsAsync" }
                    };
                    HttpRoute route = new HttpRoute("grants", routeValueDict);
                    HttpRouteData data = new HttpRouteData(route);
                    grantsController.Request.SetRouteData(data);

                    var permissionsFilter = new PermissionsFilter("invalid");

                    var controllerContext = grantsController.ControllerContext;
                    var actionDescriptor = grantsController.ActionContext.ActionDescriptor
                             ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                    var _context = new HttpActionContext(controllerContext, actionDescriptor);
                    try
                    {
                        await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                        var tuple = new Tuple<IEnumerable<Dtos.Grant>, int>(grantCollection, 5);

                        grantsServiceMock.Setup(s => s.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                        grantsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                            .Throws(new PermissionsException("User is not authorized to view person-holds."));
                        var resp = await grantsController.GetGrantsAsync(new Paging(10, 0));
                    }
                    catch (PermissionsException ex)
                    {
                        throw ex;
                    }
                }

                #endregion

                #region POST-PUT-DELETE

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_PostGrantsAsync_Exception()
                {
                    await grantsController.PostGrantsAsync(grantCollection.FirstOrDefault());
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_PutGrantsAsync_Exception()
                {
                    var sourceContext = grantCollection.FirstOrDefault();
                    await grantsController.PutGrantsAsync(sourceContext.Id, sourceContext);
                }

                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task GrantsController_DeleteGrantsAsync_Exception()
                {
                    await grantsController.DeleteGrantsAsync(grantCollection.FirstOrDefault().Id);
                }

                #endregion

            }

        }
    }
}
