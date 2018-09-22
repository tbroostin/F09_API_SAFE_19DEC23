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
    public class PersonEmploymentReferencesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private PersonEmploymentReferencesController personEmploymentReferencesController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            personEmploymentReferencesController = new PersonEmploymentReferencesController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            personEmploymentReferencesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            personEmploymentReferencesController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task PersonEmploymentReferencesController_GetAll()
        {
            var personEmploymentReferences = await personEmploymentReferencesController.GetPersonEmploymentReferencesAsync();

            Assert.IsTrue(personEmploymentReferences.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonEmploymentReferencesController_GetPersonEmploymentReferencesByGuid_Exception()
        {
            await personEmploymentReferencesController.GetPersonEmploymentReferencesByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonEmploymentReferencesController_PostPersonEmploymentReferencesAsync_Exception()
        {
            await personEmploymentReferencesController.PostPersonEmploymentReferenceAsync(It.IsAny<PersonEmploymentReference>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonEmploymentReferencesController_PutPersonEmploymentReferenceAsync_Exception()
        {

            await personEmploymentReferencesController.PutPersonEmploymentReferenceAsync(It.IsAny<string>(), It.IsAny<PersonEmploymentReference>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonEmploymentReferencesController_DeletePersonEmploymentReferencesAsync_Exception()
        {
            await personEmploymentReferencesController.DeletePersonEmploymentReferenceAsync(It.IsAny<string>());
        }
    }
}