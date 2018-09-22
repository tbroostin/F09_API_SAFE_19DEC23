//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.HumanResources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class ProficiencyLicensingAuthoritiesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private ProficiencyLicensingAuthoritiesController proficiencyLicensingAuthoritiesController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            proficiencyLicensingAuthoritiesController = new ProficiencyLicensingAuthoritiesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            proficiencyLicensingAuthoritiesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            proficiencyLicensingAuthoritiesController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task ProficiencyLicensingAuthoritiesController_GetAll()
        {
            var proficiencyLicensingAuthorities = await proficiencyLicensingAuthoritiesController.GetProficiencyLicensingAuthoritiesAsync();

            Assert.IsTrue(proficiencyLicensingAuthorities.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProficiencyLicensingAuthoritiesController_GetProficiencyLicensingAuthorityByGuid_Exception()
        {
            await proficiencyLicensingAuthoritiesController.GetProficiencyLicensingAuthorityByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProficiencyLicensingAuthoritiesController_PostProficiencyLicensingAuthorityAsync_Exception()
        {
            await proficiencyLicensingAuthoritiesController.PostProficiencyLicensingAuthorityAsync(It.IsAny<ProficiencyLicensingAuthority>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProficiencyLicensingAuthoritiesController_PutProficiencyLicensingAuthorityAsync_Exception()
        {

            await proficiencyLicensingAuthoritiesController.PutProficiencyLicensingAuthorityAsync(It.IsAny<string>(), It.IsAny<ProficiencyLicensingAuthority>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProficiencyLicensingAuthoritiesController_DeleteProficiencyLicensingAuthorityAsync_Exception()
        {
            await proficiencyLicensingAuthoritiesController.DeleteProficiencyLicensingAuthorityAsync(It.IsAny<string>());
        }
    }
}