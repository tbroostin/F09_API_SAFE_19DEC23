//Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class BudgetAdjustmentsControllerTests
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IBudgetAdjustmentService> _budgetAdjustmentServiceMock;

        private HttpResponse _response;

        private BudgetAdjustmentsController _budgetAdjustmentsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _budgetAdjustmentServiceMock = new Mock<IBudgetAdjustmentService>();


            _response = new HttpResponse(new StringWriter());

            _budgetAdjustmentsController = new BudgetAdjustmentsController(_budgetAdjustmentServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            _budgetAdjustmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _budgetAdjustmentServiceMock = null;
            _budgetAdjustmentsController = null;
        }

        [TestMethod]
        public async Task BudgetAdjustmentsController_PutAsync_Valid()
        {
            _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ReturnsAsync(new Dtos.ColleagueFinance.BudgetAdjustment());
            var budgetAdjustmentDto = await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
            Assert.IsNotNull(budgetAdjustmentDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsController_PutAsync_PermissionsException()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ThrowsAsync(new PermissionsException());
                await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to update the budget adjustment.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsController_PutAsync_ConfigurationException()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ThrowsAsync(new ConfigurationException());
                await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get budget adjustment configuration.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsController_PutAsync_Exception()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ThrowsAsync(new Exception());
                await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to update the budget adjustment.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsControllerr_PutAsync_ExpiredSessionException()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("timeout", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsController_PostAsync_Create_ExpiredSessionException()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.CreateBudgetAdjustmentAsync(It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _budgetAdjustmentsController.PostAsync(new Dtos.ColleagueFinance.BudgetAdjustment());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("timeout", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsControllerr_GetBudgetAdjustmentAsync_ExpiredSessionException()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.GetBudgetAdjustmentAsync(It.IsAny<string>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _budgetAdjustmentsController.GetBudgetAdjustmentAsync("1");
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("timeout", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsControllerr_GetBudgetAdjustmentPendingApprovalDetailAsync_ExpiredSessionException()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.GetBudgetAdjustmentPendingApprovalDetailAsync(It.IsAny<string>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _budgetAdjustmentsController.GetBudgetAdjustmentPendingApprovalDetailAsync("1");
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("timeout", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsControllerr_GetBudgetAdjustmentsPendingApprovalSummaryAsync_ExpiredSessionException()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.GetBudgetAdjustmentsPendingApprovalSummaryAsync()).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _budgetAdjustmentsController.GetBudgetAdjustmentsPendingApprovalSummaryAsync();
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("timeout", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsControllerr_PostBudgetAdjustmentApprovalAsync_ExpiredSessionException()
        {
            try
            {
                _budgetAdjustmentServiceMock.Setup(x => x.PostBudgetAdjustmentApprovalAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustmentApproval>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _budgetAdjustmentsController.PostBudgetAdjustmentApprovalAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustmentApproval>());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("timeout", responseJson.Message);
                throw;
            }
        }
    }
}
