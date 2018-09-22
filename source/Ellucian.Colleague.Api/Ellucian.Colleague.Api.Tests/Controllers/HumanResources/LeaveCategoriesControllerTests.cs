//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class LeaveCategoriesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private LeaveCategoriesController leaveCategoriesController;      
        

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();
         
            leaveCategoriesController = new LeaveCategoriesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            leaveCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            leaveCategoriesController = null;           
            loggerMock = null;
        
        }

       
        
        [TestMethod]
        public async Task LeaveCategoriesController_GetAll()
        {
            var leaveCategories = await leaveCategoriesController.GetLeaveCategoriesAsync();

            Assert.IsTrue(leaveCategories.Count().Equals(0));        
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveCategoriesController_GetLeaveCategoriesByGuid_Exception()
        {
            await leaveCategoriesController.GetLeaveCategoriesByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveCategoriesController_PostLeaveCategoriesAsync_Exception()
        {
            await leaveCategoriesController.PostLeaveCategoriesAsync(It.IsAny<Dtos.LeaveCategories>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveCategoriesController_PutLeaveCategoriesAsync_Exception()
        {
 
            await leaveCategoriesController.PutLeaveCategoriesAsync(It.IsAny<string>(), It.IsAny<Dtos.LeaveCategories>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeaveCategoriesController_DeleteLeaveCategoriesAsync_Exception()
        {
            await leaveCategoriesController.DeleteLeaveCategoriesAsync(It.IsAny<string>());
        }
    }
}