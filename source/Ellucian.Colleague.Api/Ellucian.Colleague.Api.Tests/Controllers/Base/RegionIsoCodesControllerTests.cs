//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RegionIsoCodesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private RegionIsoCodesController regionIsoCodesController;      
        

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
         
            regionIsoCodesController = new RegionIsoCodesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            regionIsoCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            regionIsoCodesController = null;           
            loggerMock = null;
        
        }

       
        
        [TestMethod]
        public async Task RegionIsoCodesController_GetAll()
        {
            var regionIsoCodes = await regionIsoCodesController.GetRegionIsoCodesAsync();

            Assert.IsTrue(regionIsoCodes.Count().Equals(0));        
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuid_Exception()
        {
            await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_PostRegionIsoCodesAsync_Exception()
        {
            await regionIsoCodesController.PostRegionIsoCodesAsync(It.IsAny<Dtos.RegionIsoCodes>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_PutRegionIsoCodesAsync_Exception()
        {
 
            await regionIsoCodesController.PutRegionIsoCodesAsync(It.IsAny<string>(), It.IsAny<Dtos.RegionIsoCodes>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_DeleteRegionIsoCodesAsync_Exception()
        {
            await regionIsoCodesController.DeleteRegionIsoCodesAsync(It.IsAny<string>());
        }
    }
}