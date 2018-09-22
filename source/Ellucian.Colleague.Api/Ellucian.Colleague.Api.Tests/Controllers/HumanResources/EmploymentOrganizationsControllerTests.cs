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
    public class EmploymentOrganizationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private EmploymentOrganizationsController employmentOrganizationsController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            employmentOrganizationsController = new EmploymentOrganizationsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            employmentOrganizationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            employmentOrganizationsController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task EmploymentOrganizationsController_GetAll()
        {
            var employmentOrganizations = await employmentOrganizationsController.GetEmploymentOrganizationsAsync();

            Assert.IsTrue(employmentOrganizations.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentOrganizationsController_GetEmploymentOrganizationsByGuid_Exception()
        {
            await employmentOrganizationsController.GetEmploymentOrganizationsByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentOrganizationsController_PostEmploymentOrganizationsAsync_Exception()
        {
            await employmentOrganizationsController.PostEmploymentOrganizationsAsync(It.IsAny<Dtos.EmploymentOrganizations>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentOrganizationsController_PutEmploymentOrganizationsAsync_Exception()
        {

            await employmentOrganizationsController.PutEmploymentOrganizationsAsync(It.IsAny<string>(), It.IsAny<Dtos.EmploymentOrganizations>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentOrganizationsController_DeleteEmploymentOrganizationsAsync_Exception()
        {
            await employmentOrganizationsController.DeleteEmploymentOrganizationsAsync(It.IsAny<string>());
        }
    }
}