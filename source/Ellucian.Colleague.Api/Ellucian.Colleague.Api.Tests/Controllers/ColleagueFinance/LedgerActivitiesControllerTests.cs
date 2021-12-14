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
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class LedgerActivitiesControllerTests_V11
    {
        [TestClass]
        public class LedgerActivitiesControllerTests_GET
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }

            private Mock<ILedgerActivityService> ledgerActivityServiceMock;
            private Mock<ILogger> loggerMock;
            private LedgerActivitiesController ledgerActivitiesController;

            private Paging page;

            private Tuple<IEnumerable<Dtos.LedgerActivity>, int> tupleResult;

            private Dtos.LedgerActivity ledgerActivity;
            private List<Dtos.LedgerActivity> ledgerActivities;

            private string guid = "83f78f38-cb00-403b-a107-557dabf0f451";
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            private string filterGroupName = "criteria";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                ledgerActivityServiceMock = new Mock<ILedgerActivityService>();

                InitializeTestData();

                ledgerActivityServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(tupleResult);

                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(ledgerActivities.FirstOrDefault());

                ledgerActivitiesController = new LedgerActivitiesController(ledgerActivityServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };

                ledgerActivitiesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                ledgerActivitiesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                ledgerActivitiesController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(ledgerActivity));

            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                ledgerActivityServiceMock = null;
                ledgerActivitiesController = null;
            }

            private void InitializeTestData()
            {
                page = new Paging(0, 100);

                ledgerActivity = new Dtos.LedgerActivity() { Id = guid };

                ledgerActivities = new List<Dtos.LedgerActivity>()
                {
                    ledgerActivity
                };

                tupleResult = new Tuple<IEnumerable<Dtos.LedgerActivity>, int>(ledgerActivities, 1);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_WithOut_Filters()
            {
                ledgerActivitiesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                await ledgerActivitiesController.GetLedgerActivitiesAsync(null, null);
            }

            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_FiscalYear_Filter()
            {
                //string criteria = @"{'fiscalYear':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}}";
                ledgerActivitiesController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.LedgerActivityFilter() { FiscalYear = new Dtos.GuidObject2("70479f3b-bb79-4c0b-a0db-c240cd51e300") });

                var result = await ledgerActivitiesController.GetLedgerActivitiesAsync(null, criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.LedgerActivity>>)httpResponseMessage.Content)
                    .Value as IEnumerable<Dtos.LedgerActivity>;

                Assert.IsNotNull(result);
                Assert.AreEqual(actuals.FirstOrDefault().Id, guid);
            }

            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_FiscalYear_NamedQuery_Filter()
            {
                //string fiscalYear = @"{'fiscalYear':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}}";
                ledgerActivitiesController.Request.Properties.Add(
                      string.Format("FilterObject{0}", "fiscalYear"),
                      new Dtos.Filters.FiscalYearFilter() { FiscalYear = new Dtos.GuidObject2("70479f3b-bb79-4c0b-a0db-c240cd51e300") });

                var result = await ledgerActivitiesController.GetLedgerActivitiesAsync(null, criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.LedgerActivity>>)httpResponseMessage.Content)
                    .Value as IEnumerable<Dtos.LedgerActivity>;

                Assert.IsNotNull(result);
                Assert.AreEqual(actuals.FirstOrDefault().Id, guid);
            }


            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_FiscalPeriod_Filter()
            {
                //string criteria = @"{'fiscalPeriod':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}}";
                ledgerActivitiesController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.LedgerActivityFilter() { FiscalPeriod = new Dtos.GuidObject2("70479f3b-bb79-4c0b-a0db-c240cd51e300") });

                var result = await ledgerActivitiesController.GetLedgerActivitiesAsync(null, criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.LedgerActivity>>)httpResponseMessage.Content)
                    .Value as IEnumerable<Dtos.LedgerActivity>;

                Assert.IsNotNull(result);
                Assert.AreEqual(actuals.FirstOrDefault().Id, guid);
            }

            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_Period_Filter()
            {
                //string criteria = @"{'period':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}}";
                ledgerActivitiesController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.LedgerActivityFilter() { Period = new Dtos.GuidObject2("70479f3b-bb79-4c0b-a0db-c240cd51e300") });

                var result = await ledgerActivitiesController.GetLedgerActivitiesAsync(null, criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.LedgerActivity>>)httpResponseMessage.Content)
                    .Value as IEnumerable<Dtos.LedgerActivity>;

                Assert.IsNotNull(result);
                Assert.AreEqual(actuals.FirstOrDefault().Id, guid);
            }

            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_Period_and_FiscalPeriod_Filter_Same()
            {
                //string criteria = @"{'fiscalPeriod':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}, 'period':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}}";
                ledgerActivitiesController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.LedgerActivityFilter()
                      {
                          Period = new Dtos.GuidObject2("70479f3b-bb79-4c0b-a0db-c240cd51e300"),
                          FiscalPeriod = new Dtos.GuidObject2("70479f3b-bb79-4c0b-a0db-c240cd51e300")
                      }); ;

                var result = await ledgerActivitiesController.GetLedgerActivitiesAsync(null, criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.LedgerActivity>>)httpResponseMessage.Content)
                    .Value as IEnumerable<Dtos.LedgerActivity>;

                Assert.IsNotNull(result);
                Assert.AreEqual(actuals.FirstOrDefault().Id, guid);
            }

            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_Period_and_FiscalPeriod_Filter_Different()
            {
                //string criteria = @"{'fiscalPeriod':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}, 'period':{'id':'80479f3b-bb79-4c0b-a0db-c240cd51e301'}}";
                ledgerActivitiesController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.LedgerActivityFilter()
                      {
                          Period = new Dtos.GuidObject2("70479f3b-bb79-4c0b-a0db-c240cd51e300"),
                          FiscalPeriod = new Dtos.GuidObject2("80479f3b-bb79-4c0b-a0db-c240cd51e301")
                      }); ;

                var result = await ledgerActivitiesController.GetLedgerActivitiesAsync(null, criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                HttpResponseMessage httpResponseMessage = await result.ExecuteAsync(cancelToken);

                var actuals = ((ObjectContent<IEnumerable<Dtos.LedgerActivity>>)httpResponseMessage.Content)
                    .Value as IEnumerable<Dtos.LedgerActivity>;

                Assert.IsNotNull(result);
                Assert.AreEqual(actuals.Count(), 0);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_ArgumentNullException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new ArgumentNullException());

                //string criteria = @"{'fiscalyear' :'','fiscalperiod':'2010', 'reportingsegment':'1', 'transactiondate':'2017-08-27'}";
                ledgerActivitiesController.Request.Properties.Add(
                     string.Format("FilterObject{0}", filterGroupName),
                     new Dtos.Filters.LedgerActivityFilter() { FiscalYear = null });

                await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_KeyNotFoundException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new KeyNotFoundException());

                //string criteria = @"{'fiscalyear' :'2010','fiscalperiod':'', 'reportingsegment':'1', 'transactiondate':'2017-08-27'}";
                ledgerActivitiesController.Request.Properties.Add(
                     string.Format("FilterObject{0}", filterGroupName),
                     new Dtos.Filters.LedgerActivityFilter() { FiscalYear = new Dtos.GuidObject2("invalid") });


                await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_PermissionsException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new PermissionsException());

                //string criteria = @"{'fiscalyear' :'2010','fiscalperiod':'2010', 'reportingsegment':'', 'transactiondate':'2017-08-27'}";
                ledgerActivitiesController.Request.Properties.Add(
                     string.Format("FilterObject{0}", filterGroupName),
                     new Dtos.Filters.LedgerActivityFilter() { ReportingSegment = "" });

                await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_RepositoryException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new RepositoryException());

                await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_IntegrationApiException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new IntegrationApiException());

                await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_InvalidOperationException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new InvalidOperationException());

                await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_Exception()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new Exception());

                await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_Guid_Null()
            {
                ledgerActivitiesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_KeyNotFoundException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new KeyNotFoundException());
                await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_PermissionsException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new PermissionsException());
                await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_ArgumentException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new ArgumentException());
                await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_RepositoryException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new RepositoryException());
                await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_IntegrationApiException()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new IntegrationApiException());
                await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_Exception()
            {
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new Exception());
                await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);
            }

            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync()
            {
                var result = await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, guid);
            }

            //GET by id v11.1.0 v11
            //Successful
            //GetLedgerActivitiesByGuidAsync
            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "LedgerActivities" },
                    { "action", "GetLedgerActivitiesByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("ledger-activities", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                ledgerActivitiesController.Request.SetRouteData(data);
                ledgerActivitiesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(ColleagueFinancePermissionCodes.ViewLedgerActivities);

                var controllerContext = ledgerActivitiesController.ControllerContext;
                var actionDescriptor = ledgerActivitiesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                ledgerActivityServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ReturnsAsync(ledgerActivities.FirstOrDefault());
                var result = await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);

                Object filterObject;
                ledgerActivitiesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewLedgerActivities));


            }

            //GET by id v11.1.0 v11
            //Exception
            //GetLedgerActivitiesByGuidAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "LedgerActivities" },
                    { "action", "GetLedgerActivitiesByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("ledger-activities", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                ledgerActivitiesController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = ledgerActivitiesController.ControllerContext;
                var actionDescriptor = ledgerActivitiesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    ledgerActivityServiceMock.Setup(s => s.GetLedgerActivityByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new PermissionsException());
                    ledgerActivityServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view ledger-activities."));
                    await ledgerActivitiesController.GetLedgerActivitiesByGuidAsync(guid);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET v11.1.0 v11
            //Successful
            //GetLedgerActivitiesAsync
            [TestMethod]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "LedgerActivities" },
                    { "action", "GetLedgerActivitiesAsync" }
                };
                HttpRoute route = new HttpRoute("ledger-activities", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                ledgerActivitiesController.Request.SetRouteData(data);
                ledgerActivitiesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(ColleagueFinancePermissionCodes.ViewLedgerActivities);

                var controllerContext = ledgerActivitiesController.ControllerContext;
                var actionDescriptor = ledgerActivitiesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                ledgerActivityServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(tupleResult);
                await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);

                Object filterObject;
                ledgerActivitiesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(ColleagueFinancePermissionCodes.ViewLedgerActivities));


            }

            //GET v11.1.0 v11
            //Exception
            //GetLedgerActivitiesAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_GetLedgerActivitiesAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "LedgerActivities" },
                    { "action", "GetLedgerActivitiesAsync" }
                };
                HttpRoute route = new HttpRoute("ledger-activities", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                ledgerActivitiesController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = ledgerActivitiesController.ControllerContext;
                var actionDescriptor = ledgerActivitiesController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    ledgerActivityServiceMock.Setup(s => s.GetLedgerActivitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false)).ThrowsAsync(new PermissionsException());
                    ledgerActivityServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view ledger-activities."));
                    await ledgerActivitiesController.GetLedgerActivitiesAsync(page, criteriaFilter);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            #region NOT SUPPORTED

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_PostLedgerActivitiesAsync_NotSupported()
            {
                await ledgerActivitiesController.PostLedgerActivitiesAsync(new Dtos.LedgerActivity() { });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_PutLedgerActivitiesAsync_NotSupported()
            {
                await ledgerActivitiesController.PutLedgerActivitiesAsync(guid, new Dtos.LedgerActivity() { });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LedgerActivitiesController_DeleteLedgerActivitiesAsync_NotSupported()
            {
                await ledgerActivitiesController.DeleteLedgerActivitiesAsync(guid);
            }

            #endregion
        }
    }
}
