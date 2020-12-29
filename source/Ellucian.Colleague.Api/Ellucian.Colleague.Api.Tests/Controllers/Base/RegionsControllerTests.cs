//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RegionsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private RegionsController regionsController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();

            regionsController = new RegionsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            regionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            regionsController = null;
            loggerMock = null;

        }

        [TestMethod]
        public async Task RegionsController_GetAll()
        {
            var regions = await regionsController.GetRegionsAsync();

            Assert.IsTrue(regions.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionsController_GetRegionsByGuid_Exception()
        {
            await regionsController.GetRegionsByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionsController_PostRegionsAsync_Exception()
        {
            await regionsController.PostRegionsAsync(It.IsAny<Dtos.Regions>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionsController_PutRegionsAsync_Exception()
        {

            await regionsController.PutRegionsAsync(It.IsAny<string>(), It.IsAny<Dtos.Regions>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionsController_DeleteRegionsAsync_Exception()
        {
            await regionsController.DeleteRegionsAsync(It.IsAny<string>());
        }
    }
}