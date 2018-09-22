///Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
    public class PersonAchievementsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private PersonAchievementsController personAchievementsController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            personAchievementsController = new PersonAchievementsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            personAchievementsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            personAchievementsController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task PersonAchievementsController_GetAll()
        {
            var personAchievements = await personAchievementsController.GetPersonAchievementsAsync();

            Assert.IsTrue(personAchievements.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonAchievementsController_GetPersonAchievementsByGuid_Exception()
        {
            await personAchievementsController.GetPersonAchievementByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonAchievementsController_PostPersonAchievementsAsync_Exception()
        {
            await personAchievementsController.PostPersonAchievementAsync(It.IsAny<PersonAchievement>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonAchievementsController_PutPersonAchievementsAsync_Exception()
        {

            await personAchievementsController.PutPersonAchievementAsync(It.IsAny<string>(), It.IsAny<PersonAchievement>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonAchievementsController_DeletePersonAchievementAsync_Exception()
        {
            await personAchievementsController.DeletePersonAchievementAsync(It.IsAny<string>());
        }
    }
}