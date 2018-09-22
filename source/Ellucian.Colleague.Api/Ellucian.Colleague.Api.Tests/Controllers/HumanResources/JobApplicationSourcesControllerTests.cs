//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.HumanResources;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class JobApplicationSourcesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private JobApplicationSourcesController jobApplicationSourcesController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            jobApplicationSourcesController = new JobApplicationSourcesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            jobApplicationSourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            jobApplicationSourcesController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task JobApplicationSourcesController_GetAll()
        {
            var jobApplicationSources = await jobApplicationSourcesController.GetJobApplicationSourcesAsync();

            Assert.IsTrue(jobApplicationSources.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationSourcesController_GetJobApplicationSourcesByGuid_Exception()
        {
            await jobApplicationSourcesController.GetJobApplicationSourcesByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationSourcesController_PostJobApplicationSourcesAsync_Exception()
        {
            await jobApplicationSourcesController.PostJobApplicationSourcesAsync(It.IsAny<Dtos.JobApplicationSources>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationSourcesController_PutJobApplicationSourcesAsync_Exception()
        {

            await jobApplicationSourcesController.PutJobApplicationSourcesAsync(It.IsAny<string>(), It.IsAny<Dtos.JobApplicationSources>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationSourcesController_DeleteJobApplicationSourcesAsync_Exception()
        {
            await jobApplicationSourcesController.DeleteJobApplicationSourcesAsync(It.IsAny<string>());
        }
    }
}