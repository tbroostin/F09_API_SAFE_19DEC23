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

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class PersonPublicationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        private Mock<ILogger> loggerMock;
        private PersonPublicationsController personPublicationsController;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            loggerMock = new Mock<ILogger>();
            personPublicationsController = new PersonPublicationsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            personPublicationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            personPublicationsController = null;
            loggerMock = null;
        }

        [TestMethod]
        public async Task PersonPublicationsController_GetPersonPublications()
        {
            var actuals = (await personPublicationsController.GetPersonPublicationsAsync()).ToList();
            Assert.IsNotNull(actuals);
            Assert.AreEqual(0, actuals.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonPublicationsController_GetPersonPublicationsByGuidAsync_ValidateFields()
        {
            var actual = await personPublicationsController.GetPersonPublicationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonPublicationsController_PostPersonPublicationsAsync_Exception()
        {
            await personPublicationsController.PostPersonPublicationsAsync(new Dtos.PersonPublications());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonPublicationsController_PutPersonPublicationsAsync_Exception()
        {
            var sourceContext = new Dtos.PersonPublications();
            await personPublicationsController.PutPersonPublicationsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonPublicationsController_DeletePersonPublicationsAsync_Exception()
        {
            await personPublicationsController.DeletePersonPublicationsAsync(expectedGuid);
        }
    }
}