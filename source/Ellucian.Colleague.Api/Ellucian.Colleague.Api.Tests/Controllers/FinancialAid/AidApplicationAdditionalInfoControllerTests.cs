// Copyright 2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{

    [TestClass]
    public class AidApplicationAdditionalInfoControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IAidApplicationAdditionalInfoService> _aidApplicationAdditionalInfoServiceMock;
        private Mock<ILogger> _loggerMock;

        private AidApplicationAdditionalInfoController _aidApplicationAdditionalInfoController;

        private List<AidApplicationAdditionalInfo> _aidApplicationAdditionalInfoCollection;
        private AidApplicationAdditionalInfo _aidApplicationAdditionalInfoDto;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string AidApplicationAdditionalInfoId = "1";
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _aidApplicationAdditionalInfoServiceMock = new Mock<IAidApplicationAdditionalInfoService>();
            _loggerMock = new Mock<ILogger>();

            _aidApplicationAdditionalInfoCollection = new List<AidApplicationAdditionalInfo>();

            var additionalInfo1 = new AidApplicationAdditionalInfo
            {
                Id = AidApplicationAdditionalInfoId,
                AppDemoId = AidApplicationAdditionalInfoId,
                PersonId = "0002990",
                AidYear = "2021",
                ApplicationType = "CALISIR",
                ApplicantAssignedId = "987654321",
                StudentStateId = "20192837",
                FosterCare = true,
                ApplicationCounty = "Orange",
                WardshipState = "US",
                ChafeeConsideration = true,
                CreateCcpgRecord = true,
                User1 = "Test1",
                User2 = "Test2",
                User3 = "Test3",
                User4 = "Test4",
                User5 = "Test5",
                User6 = "Test6",
                User7 = "Test7",
                User8 = "Test8",
                User9 = "Test9",
                User10 = "Test10",
                User11 = "Test11",
                User12 = "Test12",
                User13 = "Test13",
                User14 = "Test14",
                User15 = new DateTime(2001, 1, 20),
                User16 = new DateTime(2002, 10, 21),
                User17 = new DateTime(2003, 11, 22),
                User18 = new DateTime(2004, 12, 23),
                User19 = new DateTime(2005, 5, 24),
                User21 = new DateTime(2006, 6, 25)
            };

            _aidApplicationAdditionalInfoCollection.Add(additionalInfo1);

            BuildData();

            _aidApplicationAdditionalInfoController = new AidApplicationAdditionalInfoController(_aidApplicationAdditionalInfoServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _aidApplicationAdditionalInfoController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _aidApplicationAdditionalInfoController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_aidApplicationAdditionalInfoDto));
        }

        private void BuildData()
        {
            _aidApplicationAdditionalInfoDto = new AidApplicationAdditionalInfo
            {
                Id = AidApplicationAdditionalInfoId,
                AppDemoId = AidApplicationAdditionalInfoId,
                PersonId = "0002990",
                AidYear = "2021",
                ApplicationType = "CALISIR",
                ApplicantAssignedId = "987654321",
                StudentStateId = "20192837",
                FosterCare = true,
                ApplicationCounty = "Orange",
                WardshipState = "US",
                ChafeeConsideration = true,
                CreateCcpgRecord = true,
                User1 = "Test1",
                User2 = "Test2",
                User3 = "Test3",
                User4 = "Test4",
                User5 = "Test5",
                User6 = "Test6",
                User7 = "Test7",
                User8 = "Test8",
                User9 = "Test9",
                User10 = "Test10",
                User11 = "Test11",
                User12 = "Test12",
                User13 = "Test13",
                User14 = "Test14",
                User15 = new DateTime(2001, 1, 20),
                User16 = new DateTime(2002, 10, 21),
                User17 = new DateTime(2003, 11, 22),
                User18 = new DateTime(2004, 12, 23),
                User19 = new DateTime(2005, 5, 24),
                User21 = new DateTime(2006, 6, 25)
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _aidApplicationAdditionalInfoController = null;
            _aidApplicationAdditionalInfoCollection = null;
            _loggerMock = null;
            _aidApplicationAdditionalInfoServiceMock = null;
        }

        #region GetAidApplicationAdditionalInfo

        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfo()
        {
            _aidApplicationAdditionalInfoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationAdditionalInfoController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>(_aidApplicationAdditionalInfoCollection, 1);

            _aidApplicationAdditionalInfoServiceMock
                .Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationAdditionalInfo>()))
                .ReturnsAsync(tuple);

            var aidApplicationInfoRecord = await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationInfoRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplicationAdditionalInfo>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplicationAdditionalInfo>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);

            }
        }

        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfo_AppDemoId()
        {
            _aidApplicationAdditionalInfoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _aidApplicationAdditionalInfoController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>(_aidApplicationAdditionalInfoCollection, 1);

            var filterGroupName = "criteria";
            _aidApplicationAdditionalInfoController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new AidApplicationAdditionalInfo() { AppDemoId = AidApplicationAdditionalInfoId });

            _aidApplicationAdditionalInfoServiceMock
                .Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationAdditionalInfo>()))
                .ReturnsAsync(tuple);

            var aidApplicationInfoRecord = await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await aidApplicationInfoRecord.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<AidApplicationAdditionalInfo>>)httpResponseMessage.Content)
                .Value as IEnumerable<AidApplicationAdditionalInfo>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfo_KeyNotFoundException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<KeyNotFoundException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(null, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfo_PermissionsException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<PermissionsException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfo_ArgumentException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<ArgumentException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfo_RepositoryException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<RepositoryException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfo_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<IntegrationApiException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(paging, criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfo_Exception()
        {
            var paging = new Paging(100, 0);
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<Exception>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(paging, criteriaFilter);
        }

        //Successful
        //GetAidApplicationAdditionalInfoAsync
        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoAsync_Permissions()
        {
            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationAdditionalInfoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "GetAidApplicationAdditionalInfoAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);
            _aidApplicationAdditionalInfoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewAidApplicationAdditionalInfo);

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>(_aidApplicationAdditionalInfoCollection, 1);

            _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationAdditionalInfoServiceMock
                .Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationAdditionalInfo>()))
                .ReturnsAsync(tuple);
            var aidApplicationInfoRecord = await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _aidApplicationAdditionalInfoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAidApplicationAdditionalInfo));


        }

        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoAsync_UpdatePermissions()
        {
            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationAdditionalInfoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "GetAidApplicationAdditionalInfoAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);
            _aidApplicationAdditionalInfoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo);

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>(_aidApplicationAdditionalInfoCollection, 1);

            _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationAdditionalInfoServiceMock
                .Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AidApplicationAdditionalInfo>()))
                .ReturnsAsync(tuple);
            var aidApplicationInfoRecord = await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(new Paging(10, 0), criteriaFilter);

            Object filterObject;
            _aidApplicationAdditionalInfoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo));


        }

        //Exception
        //GetAidApplicationAdditionalInfoAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "GetAidApplicationAdditionalInfoAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .Throws<PermissionsException>();
                _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view aid-application-additional-info."));
                await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoAsync(paging, criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetAidApplicationAdditionalInfo

        #region GetAidApplicationAdditionalInfoById

        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoById()
        {
            _aidApplicationAdditionalInfoController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationAdditionalInfoId, StringComparison.OrdinalIgnoreCase));

            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

            var actual = await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoById_NullException()
        {
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoById_KeyNotFoundException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoById_PermissionsException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoById_ArgumentException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoById_RepositoryException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoById_IntegrationApiException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoById_Exception()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);
        }

        //Successful
        //GetAidApplicationAdditionalInfoByIdAsync
        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoByIdAsync_Permissions()
        {
            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationAdditionalInfoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "GetAidApplicationAdditionalInfoByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);
            _aidApplicationAdditionalInfoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewAidApplicationAdditionalInfo);

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);

            Object filterObject;
            _aidApplicationAdditionalInfoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAidApplicationAdditionalInfo));


        }

        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoByIdAsync_UpdatePermissions()
        {
            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationAdditionalInfoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "GetAidApplicationAdditionalInfoByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);
            _aidApplicationAdditionalInfoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo);

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);

            Object filterObject;
            _aidApplicationAdditionalInfoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo));


        }

        //Exception
        //GetAidApplicationAdditionalInfoByIdAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_GetAidApplicationAdditionalInfoByIdAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "GetAidApplicationAdditionalInfoByIdAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>())).Throws<PermissionsException>();
                _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view aid-application-additional-info."));
                await _aidApplicationAdditionalInfoController.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion GetAidApplicationAdditionalInfoById

        #region Put

        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_PUT()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, It.IsAny<AidApplicationAdditionalInfo>())).ReturnsAsync(_aidApplicationAdditionalInfoDto);
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId)).ReturnsAsync(_aidApplicationAdditionalInfoDto);
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
            Assert.IsNotNull(result);

            Assert.AreEqual(_aidApplicationAdditionalInfoDto.Id, result.Id);
        }

        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_ValidateUpdateRequest_RequestId_Null()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(AidApplicationAdditionalInfoId)).ReturnsAsync(_aidApplicationAdditionalInfoDto);
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, It.IsAny<AidApplicationAdditionalInfo>())).ReturnsAsync(_aidApplicationAdditionalInfoDto);
            _aidApplicationAdditionalInfoDto.Id = string.Empty;
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_POST()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ReturnsAsync(_aidApplicationAdditionalInfoDto);
            _aidApplicationAdditionalInfoDto.Id = string.Empty;
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_POST_Null_Dto()
        {
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_POST_KeyNotFoundException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ThrowsAsync(new KeyNotFoundException());
            _aidApplicationAdditionalInfoDto.Id = string.Empty;
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_POST_PermissionsException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ThrowsAsync(new PermissionsException());
            _aidApplicationAdditionalInfoDto.Id = string.Empty;
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_POST_ArgumentException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ThrowsAsync(new ArgumentException());
            _aidApplicationAdditionalInfoDto.Id = string.Empty;
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_POST_RepositoryException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ThrowsAsync(new RepositoryException());
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_POST_IntegrationApiException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_POST_Exception()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ThrowsAsync(new Exception());
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_PermissionsException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto)).ThrowsAsync(new PermissionsException());
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_RepositoryException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto)).ThrowsAsync(new RepositoryException());
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_IntegrationApiException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto)).ThrowsAsync(new IntegrationApiException());
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_Exception1()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto)).ThrowsAsync(new Exception());
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_KeyNotFoundException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto)).ThrowsAsync(new KeyNotFoundException());
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]

        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_ArgumentException()
        {
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto)).ThrowsAsync(new ArgumentException());
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_ValidateUpdateRequest_Id_Null_Exception()
        {
            await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(string.Empty, _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_ValidateUpdateRequest_Request_Null_Exception()
        {
            await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_ValidateUpdateRequest_EmptyGuid_Null_Exception()
        {
            await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync("1", _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_ValidateUpdateRequest_RequestIdIsEmptyGuid_Null_Exception()
        {
            _aidApplicationAdditionalInfoDto.Id = "";
            await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_ValidateUpdateRequest_GuidsNotMatching_Exception()
        {
            _aidApplicationAdditionalInfoDto.Id = "2";
            await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfo_Exception()
        {
            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault();
            await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(expected.Id, expected);
        }

        //Successful
        //PutAidApplicationAdditionalInfoAsync
        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfoAsync_Permissions()
        {
            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault(x => x.Id.Equals(AidApplicationAdditionalInfoId, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "PutAidApplicationAdditionalInfoAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);
            _aidApplicationAdditionalInfoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo);

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationAdditionalInfoServiceMock.Setup(x => x.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>())).ReturnsAsync(_aidApplicationAdditionalInfoDto);
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, It.IsAny<AidApplicationAdditionalInfo>())).ReturnsAsync(_aidApplicationAdditionalInfoDto);
            var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);

            Object filterObject;
            _aidApplicationAdditionalInfoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo));


        }

        //Exception
        //PutAidApplicationAdditionalInfoAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PutAidApplicationAdditionalInfoAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "PutAidApplicationAdditionalInfoAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto)).ThrowsAsync(new PermissionsException());
                _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update aid-application-additional-info."));
                var result = await _aidApplicationAdditionalInfoController.PutAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfoId, _aidApplicationAdditionalInfoDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Post

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PostAidApplicationAdditionalInfo_Exception()
        {
            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault();
            await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(expected);
        }

        //Successful
        //PostAidApplicationAdditionalInfoAsync
        [TestMethod]
        public async Task AidApplicationAdditionalInfoController_PostAidApplicationAdditionalInfoAsync_Permissions()
        {
            _aidApplicationAdditionalInfoDto.Id = string.Empty;
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "PostAidApplicationAdditionalInfoAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);
            _aidApplicationAdditionalInfoController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo);

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ReturnsAsync(_aidApplicationAdditionalInfoDto);
            var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);

            Object filterObject;
            _aidApplicationAdditionalInfoController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAidApplicationAdditionalInfo));


        }

        //Exception
        //PostAidApplicationAdditionalInfoAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_PostAidApplicationAdditionalInfoAsync_Invalid_Permissions()
        {
            _aidApplicationAdditionalInfoDto.Id = string.Empty;
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AidApplicationAdditionalInfo" },
                    { "action", "PostAidApplicationAdditionalInfoAsync" }
                };
            HttpRoute route = new HttpRoute("AidApplicationAdditionalInfo", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _aidApplicationAdditionalInfoController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _aidApplicationAdditionalInfoController.ControllerContext;
            var actionDescriptor = _aidApplicationAdditionalInfoController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _aidApplicationAdditionalInfoServiceMock.Setup(i => i.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto)).ThrowsAsync(new PermissionsException());
                _aidApplicationAdditionalInfoServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create aid-application-additional-info."));
                var result = await _aidApplicationAdditionalInfoController.PostAidApplicationAdditionalInfoAsync(_aidApplicationAdditionalInfoDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AidApplicationAdditionalInfoController_DeleteAidApplicationAdditionalInfoAsync_Exception()
        {
            var expected = _aidApplicationAdditionalInfoCollection.FirstOrDefault();
            await _aidApplicationAdditionalInfoController.DeleteAidApplicationAdditionalInfoAsync(expected.Id);
        }

        #endregion
    }
}
