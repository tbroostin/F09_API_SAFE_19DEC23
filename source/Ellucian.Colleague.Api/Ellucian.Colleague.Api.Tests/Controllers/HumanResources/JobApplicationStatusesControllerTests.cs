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
    public class JobApplicationStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private JobApplicationStatusesController jobApplicationStatusesController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            jobApplicationStatusesController = new JobApplicationStatusesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            jobApplicationStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            jobApplicationStatusesController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task JobApplicationStatusesController_GetAll()
        {
            var jobApplicationStatuses = await jobApplicationStatusesController.GetJobApplicationStatusesAsync();

            Assert.IsTrue(jobApplicationStatuses.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationStatusesController_GetJobApplicationStatusesByGuid_Exception()
        {
            await jobApplicationStatusesController.GetJobApplicationStatusesByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationStatusesController_PostJobApplicationStatusesAsync_Exception()
        {
            await jobApplicationStatusesController.PostJobApplicationStatusesAsync(It.IsAny<Dtos.JobApplicationStatuses>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationStatusesController_PutJobApplicationStatusesAsync_Exception()
        {

            await jobApplicationStatusesController.PutJobApplicationStatusesAsync(It.IsAny<string>(), It.IsAny<Dtos.JobApplicationStatuses>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task JobApplicationStatusesController_DeleteJobApplicationStatusesAsync_Exception()
        {
            await jobApplicationStatusesController.DeleteJobApplicationStatusesAsync(It.IsAny<string>());
        }
    }
}