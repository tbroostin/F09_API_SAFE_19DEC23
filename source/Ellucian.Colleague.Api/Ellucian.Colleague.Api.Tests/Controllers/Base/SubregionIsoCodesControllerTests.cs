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
    public class SubregionIsoCodesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private SubregionIsoCodesController subregionIsoCodesController;      
        

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
         
            subregionIsoCodesController = new SubregionIsoCodesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            subregionIsoCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            subregionIsoCodesController = null;           
            loggerMock = null;
        
        }

       
        
        [TestMethod]
        public async Task SubregionIsoCodesController_GetAll()
        {
            var subregionIsoCodes = await subregionIsoCodesController.GetSubregionIsoCodesAsync();

            Assert.IsTrue(subregionIsoCodes.Count().Equals(0));        
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SubregionIsoCodesController_GetSubregionIsoCodesByGuid_Exception()
        {
            await subregionIsoCodesController.GetSubregionIsoCodesByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SubregionIsoCodesController_PostSubregionIsoCodesAsync_Exception()
        {
            await subregionIsoCodesController.PostSubregionIsoCodesAsync(It.IsAny<Dtos.SubregionIsoCodes>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SubregionIsoCodesController_PutSubregionIsoCodesAsync_Exception()
        {
 
            await subregionIsoCodesController.PutSubregionIsoCodesAsync(It.IsAny<string>(), It.IsAny<Dtos.SubregionIsoCodes>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SubregionIsoCodesController_DeleteSubregionIsoCodesAsync_Exception()
        {
            await subregionIsoCodesController.DeleteSubregionIsoCodesAsync(It.IsAny<string>());
        }
    }
}