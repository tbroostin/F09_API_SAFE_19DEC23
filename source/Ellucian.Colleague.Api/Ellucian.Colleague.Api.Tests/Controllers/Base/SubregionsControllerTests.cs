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
    public class SubregionsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private SubregionsController subregionsController;      
        

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
         
            subregionsController = new SubregionsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            subregionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            subregionsController = null;           
            loggerMock = null;
        
        }

       
        
        [TestMethod]
        public async Task SubregionsController_GetAll()
        {
            var subregions = await subregionsController.GetSubregionsAsync();

            Assert.IsTrue(subregions.Count().Equals(0));        
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SubregionsController_GetSubregionsByGuid_Exception()
        {
            await subregionsController.GetSubregionsByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SubregionsController_PostSubregionsAsync_Exception()
        {
            await subregionsController.PostSubregionsAsync(It.IsAny<Dtos.Subregions>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SubregionsController_PutSubregionsAsync_Exception()
        {
 
            await subregionsController.PutSubregionsAsync(It.IsAny<string>(), It.IsAny<Dtos.Subregions>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SubregionsController_DeleteSubregionsAsync_Exception()
        {
            await subregionsController.DeleteSubregionsAsync(It.IsAny<string>());
        }
    }
}