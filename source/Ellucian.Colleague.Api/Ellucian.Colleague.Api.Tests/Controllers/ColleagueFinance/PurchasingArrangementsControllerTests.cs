//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class PurchasingArrangementsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private PurchasingArrangementsController purchasingArrangementsController;      
        

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();
         
            purchasingArrangementsController = new PurchasingArrangementsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            purchasingArrangementsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            purchasingArrangementsController = null;           
            loggerMock = null;
        
        }

       
        
        [TestMethod]
        public async Task PurchasingArrangementsController_GetAll()
        {
            var purchasingArrangements = await purchasingArrangementsController.GetPurchasingArrangementsAsync();

            Assert.IsTrue(purchasingArrangements.Count().Equals(0));        
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchasingArrangementsController_GetPurchasingArrangementsByGuid_Exception()
        {
            await purchasingArrangementsController.GetPurchasingArrangementsByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchasingArrangementsController_PostPurchasingArrangementsAsync_Exception()
        {
            await purchasingArrangementsController.PostPurchasingArrangementsAsync(It.IsAny<Dtos.PurchasingArrangement>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchasingArrangementsController_PutPurchasingArrangementsAsync_Exception()
        {
 
            await purchasingArrangementsController.PutPurchasingArrangementsAsync(It.IsAny<string>(), It.IsAny<Dtos.PurchasingArrangement>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchasingArrangementsController_DeletePurchasingArrangementsAsync_Exception()
        {
            await purchasingArrangementsController.DeletePurchasingArrangementAsync(It.IsAny<string>());
        }
    }
}