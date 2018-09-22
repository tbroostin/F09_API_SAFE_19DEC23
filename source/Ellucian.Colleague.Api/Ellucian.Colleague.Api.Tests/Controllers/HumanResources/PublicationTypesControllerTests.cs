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

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PublicationTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private PublicationTypesController publicationTypesController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            publicationTypesController = new PublicationTypesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            publicationTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            publicationTypesController = null;
            loggerMock = null;

        }

        [TestMethod]
        public async Task PublicationTypesController_GetAll()
        {
            var publicationTypes = await publicationTypesController.GetPublicationTypesAsync();

            Assert.IsTrue(publicationTypes.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PublicationTypesController_GetPublicationTypesByGuid_Exception()
        {
            await publicationTypesController.GetPublicationTypesByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PublicationTypesController_PostPublicationTypesAsync_Exception()
        {
            await publicationTypesController.PostPublicationTypesAsync(It.IsAny<Dtos.PublicationTypes>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PublicationTypesController_PutPublicationTypesAsync_Exception()
        {

            await publicationTypesController.PutPublicationTypesAsync(It.IsAny<string>(), It.IsAny<Dtos.PublicationTypes>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PublicationTypesController_DeletePublicationTypesAsync_Exception()
        {
            await publicationTypesController.DeletePublicationTypesAsync(It.IsAny<string>());
        }
    }
}