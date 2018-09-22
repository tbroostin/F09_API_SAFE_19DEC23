//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
    public class PurchaseClassificationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private PurchaseClassificationsController purchaseClassificationsController;      
        

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();
         
            purchaseClassificationsController = new PurchaseClassificationsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            purchaseClassificationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            purchaseClassificationsController = null;           
            loggerMock = null;
        
        }

       
        
        [TestMethod]
        public async Task PurchaseClassificationsController_GetAll()
        {
            var purchaseClassifications = await purchaseClassificationsController.GetPurchaseClassificationsAsync();

            Assert.IsTrue(purchaseClassifications.Count().Equals(0));        
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseClassificationsController_GetPurchaseClassificationsByGuid_Exception()
        {
            await purchaseClassificationsController.GetPurchaseClassificationsByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseClassificationsController_PostPurchaseClassificationsAsync_Exception()
        {
            await purchaseClassificationsController.PostPurchaseClassificationsAsync(It.IsAny<Dtos.PurchaseClassifications>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseClassificationsController_PutPurchaseClassificationsAsync_Exception()
        {
 
            await purchaseClassificationsController.PutPurchaseClassificationsAsync(It.IsAny<string>(), It.IsAny<Dtos.PurchaseClassifications>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PurchaseClassificationsController_DeletePurchaseClassificationsAsync_Exception()
        {
            await purchaseClassificationsController.DeletePurchaseClassificationsAsync(It.IsAny<string>());
        }
    }
}