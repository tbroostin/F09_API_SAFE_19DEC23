//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class ProspectOpportunitySourcesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private ProspectOpportunitySourcesController prospectOpportunitySourcesController;      
        

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();
         
            prospectOpportunitySourcesController = new ProspectOpportunitySourcesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            prospectOpportunitySourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            prospectOpportunitySourcesController = null;           
            loggerMock = null;
        
        }

       
        
        [TestMethod]
        public async Task ProspectOpportunitySourcesController_GetAll()
        {
            var prospectOpportunitySources = await prospectOpportunitySourcesController.GetProspectOpportunitySourcesAsync();

            Assert.IsTrue(prospectOpportunitySources.Count().Equals(0));        
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProspectOpportunitySourcesController_GetProspectOpportunitySourcesByGuid_Exception()
        {
            await prospectOpportunitySourcesController.GetProspectOpportunitySourcesByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProspectOpportunitySourcesController_PostProspectOpportunitySourcesAsync_Exception()
        {
            await prospectOpportunitySourcesController.PostProspectOpportunitySourcesAsync(It.IsAny<Dtos.ProspectOpportunitySources>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProspectOpportunitySourcesController_PutProspectOpportunitySourcesAsync_Exception()
        {
 
            await prospectOpportunitySourcesController.PutProspectOpportunitySourcesAsync(It.IsAny<string>(), It.IsAny<Dtos.ProspectOpportunitySources>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProspectOpportunitySourcesController_DeleteProspectOpportunitySourcesAsync_Exception()
        {
            await prospectOpportunitySourcesController.DeleteProspectOpportunitySourcesAsync(It.IsAny<string>());
        }
    }
}