//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

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