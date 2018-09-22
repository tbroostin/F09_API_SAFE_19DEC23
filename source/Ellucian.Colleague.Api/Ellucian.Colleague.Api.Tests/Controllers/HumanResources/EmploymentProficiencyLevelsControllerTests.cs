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
using Ellucian.Colleague.Dtos.HumanResources;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmploymentProficiencyLevelsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private EmploymentProficiencyLevelsController employmentProficiencyLevelsController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            employmentProficiencyLevelsController = new EmploymentProficiencyLevelsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            employmentProficiencyLevelsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            employmentProficiencyLevelsController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task EmploymentProficiencyLevelsController_GetAll()
        {
            var employmentProficiencyLevels = await employmentProficiencyLevelsController.GetEmploymentProficiencyLevelsAsync();

            Assert.IsTrue(employmentProficiencyLevels.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentProficiencyLevelsController_GetEmploymentProficiencyLevelsByGuid_Exception()
        {
            await employmentProficiencyLevelsController.GetEmploymentProficiencyLevelByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentProficiencyLevelsController_PostEmploymentProficiencyLevelsAsync_Exception()
        {
            await employmentProficiencyLevelsController.PostEmploymentProficiencyLevelAsync(It.IsAny<EmploymentProficiencyLevel>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentProficiencyLevelsController_PutEmploymentProficiencyLevelsAsync_Exception()
        {

            await employmentProficiencyLevelsController.PutEmploymentProficiencyLevelAsync(It.IsAny<string>(), It.IsAny<EmploymentProficiencyLevel>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentProficiencyLevelsController_DeleteEmploymentProficiencyLevelsAsync_Exception()
        {
            await employmentProficiencyLevelsController.DeleteEmploymentProficiencyLevelAsync(It.IsAny<string>());
        }
    }
}