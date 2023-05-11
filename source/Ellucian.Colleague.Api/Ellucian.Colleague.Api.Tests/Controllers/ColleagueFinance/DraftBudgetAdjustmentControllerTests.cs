//Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class DraftBudgetAdjustmentControllerTests
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private Mock<IDraftBudgetAdjustmentService> draftBudgetAdjustmentServiceMock;

        private HttpResponse response;

        private DraftBudgetAdjustmentsController draftBudgetAdjustmentsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            draftBudgetAdjustmentServiceMock = new Mock<IDraftBudgetAdjustmentService>();


            response = new HttpResponse(new StringWriter());

            draftBudgetAdjustmentsController = new DraftBudgetAdjustmentsController(draftBudgetAdjustmentServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            draftBudgetAdjustmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            draftBudgetAdjustmentServiceMock = null;
            draftBudgetAdjustmentsController = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DraftBudgetAdjustmentsController_PostAsync_ExpiredSessionException()
        {
            try
            {
                draftBudgetAdjustmentServiceMock.Setup(x => x.SaveDraftBudgetAdjustmentAsync(It.IsAny<Dtos.ColleagueFinance.DraftBudgetAdjustment>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await draftBudgetAdjustmentsController.PostAsync(new Dtos.ColleagueFinance.DraftBudgetAdjustment());
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
        public async Task DraftBudgetAdjustmentsController_UpdateAsync_ExpiredSessionException()
        {
            try
            {
                draftBudgetAdjustmentServiceMock.Setup(x => x.SaveDraftBudgetAdjustmentAsync(It.IsAny<Dtos.ColleagueFinance.DraftBudgetAdjustment>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await draftBudgetAdjustmentsController.UpdateAsync("1", new Dtos.ColleagueFinance.DraftBudgetAdjustment());
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
        public async Task DraftBudgetAdjustmentsController_DeleteAsync_ExpiredSessionException()
        {
            try
            {
                draftBudgetAdjustmentServiceMock.Setup(x => x.DeleteAsync(It.IsAny<string>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await draftBudgetAdjustmentsController.DeleteAsync("1");
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
