//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
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
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmployeeLeaveTransactionsControllerTests_V11
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IEmployeeLeaveTransactionsService> empLeaveTransServiceMock;
        private Mock<ILogger> loggerMock;
        private EmployeeLeaveTransactionsController empLeaveTransController;

        private EmployeeLeaveTransactions empLeaveTrans;
        private Tuple<IEnumerable<EmployeeLeaveTransactions>, int> tupleResult;

        private Paging paging = new Paging(10, 0);

        private string guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";

        private bool bypassCache = false;

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            empLeaveTransServiceMock = new Mock<IEmployeeLeaveTransactionsService>();

            InitializeTestData();

            empLeaveTransServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), bypassCache)).ReturnsAsync(new List<string>());
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ReturnsAsync(empLeaveTrans);
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ReturnsAsync(tupleResult);

            empLeaveTransController = new EmployeeLeaveTransactionsController(empLeaveTransServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            empLeaveTransController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            empLeaveTransServiceMock = null;
            empLeaveTransController = null;
        }

        private void InitializeTestData()
        {
            empLeaveTrans = new EmployeeLeaveTransactions()
            {
                Id = guid,
            };

            tupleResult = new Tuple<IEnumerable<EmployeeLeaveTransactions>, int>(new List<EmployeeLeaveTransactions>() { empLeaveTrans }, 1);
        }

        #endregion

        #region GET & GET BY ID

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsAsync_KeyNotFoundException()
        {
            bypassCache = true;
            empLeaveTransController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new KeyNotFoundException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsAsync_PermissionsException()
        {
            empLeaveTransController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false };
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new PermissionsException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsAsync_ArgumentException()
        {
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new ArgumentException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsAsync_RepositoryException()
        {
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new RepositoryException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsAsync_IntegrationApiException()
        {
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new IntegrationApiException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsAsync_Exception()
        {
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new Exception());

            await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(paging);
        }

        [TestMethod]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsAsync()
        {
            var result = await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(paging);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsByGuidAsyncc_Guid_As_Null()
        {
            await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsByGuidAsyncc_KeyNotFoundException()
        {
            empLeaveTransController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new KeyNotFoundException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsByGuidAsync_PermissionsException()
        {
            bypassCache = false;
            empLeaveTransController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false };
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new PermissionsException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsByGuidAsyncc_ArgumentException()
        {
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new ArgumentException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsByGuidAsync_RepositoryException()
        {
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new RepositoryException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsByGuidAsync_IntegrationApiException()
        {
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new IntegrationApiException());

            await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsByGuidAsync_Exception()
        {
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new Exception());

            await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);
        }

        [TestMethod]
        public async Task EmployeeLeaveTransactionsController_GetEmployeeLeaveTransactionsByGuidAsync()
        {
            var result = await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);
            Assert.IsNotNull(result);
        }


        //Permissions Tests

        //Success
        //Get 11
        //GetEmployeeLeaveTransactionsAsync
        [TestMethod]
        public async Task empLeaveTransController_GetEmployeeLeaveTransactionsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "EmployeeLeaveTransactions" },
                { "action", "GetEmployeeLeaveTransactionsAsync" }
            };
            HttpRoute route = new HttpRoute("employee-leave-transactions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            empLeaveTransController.Request.SetRouteData(data);
            empLeaveTransController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions);

            var controllerContext = empLeaveTransController.ControllerContext;
            var actionDescriptor = empLeaveTransController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            empLeaveTransServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ReturnsAsync(tupleResult);
            var result = await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(paging);

            Object filterObject;
            empLeaveTransController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions));

        }

        //Exception
        //Get 11
        //GetEmployeeLeaveTransactionsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task empLeaveTransController_GetEmployeeLeaveTransactionsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "EmployeeLeaveTransactions" },
                { "action", "GetEmployeeLeaveTransactionsAsync" }
            };
            HttpRoute route = new HttpRoute("employee-leave-transactions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            empLeaveTransController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = empLeaveTransController.ControllerContext;
            var actionDescriptor = empLeaveTransController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new PermissionsException());
                empLeaveTransServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view employee-leave-transactions."));
                await empLeaveTransController.GetEmployeeLeaveTransactionsAsync(paging);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        //Success
        //Get By Id 11
        //GetEmployeeLeaveTransactionsByGuidAsync
        [TestMethod]
        public async Task empLeaveTransController_GetEmployeeLeaveTransactionsByGuidAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "EmployeeLeaveTransactions" },
                { "action", "GetEmployeeLeaveTransactionsByGuidAsync" }
            };
            HttpRoute route = new HttpRoute("employee-leave-transactions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            empLeaveTransController.Request.SetRouteData(data);
            empLeaveTransController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions);

            var controllerContext = empLeaveTransController.ControllerContext;
            var actionDescriptor = empLeaveTransController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            empLeaveTransServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ReturnsAsync(empLeaveTrans);
            var result = await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);


            Object filterObject;
            empLeaveTransController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions));

        }


        //Exception
        //Get By Id 11
        //GetEmployeeLeaveTransactionsByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task empLeaveTransController_GetEmployeeLeaveTransactionsByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "EmployeeLeaveTransactions" },
                { "action", "GetEmployeeLeaveTransactionsByGuidAsync" }
            };
            HttpRoute route = new HttpRoute("employee-leave-transactions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            empLeaveTransController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = empLeaveTransController.ControllerContext;
            var actionDescriptor = empLeaveTransController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                empLeaveTransServiceMock.Setup(s => s.GetEmployeeLeaveTransactionsByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new PermissionsException());
                empLeaveTransServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view employee-leave-transactions."));
                await empLeaveTransController.GetEmployeeLeaveTransactionsByGuidAsync(guid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion

        #region NOT SUPPORTED

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_PostEmployeeLeaveTransactionsAsync()
        {
            await empLeaveTransController.PostEmployeeLeaveTransactionsAsync(new EmployeeLeaveTransactions() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_PutEmployeeLeaveTransactionsAsync()
        {
            await empLeaveTransController.PutEmployeeLeaveTransactionsAsync(guid, new EmployeeLeaveTransactions() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeaveTransactionsController_DeleteEmployeeLeaveTransactionsAsync()
        {
            await empLeaveTransController.DeleteEmployeeLeaveTransactionsAsync(guid);
        }

        #endregion
    }
}