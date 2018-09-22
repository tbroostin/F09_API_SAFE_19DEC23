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
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstructionalDeliveryMethodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private InstructionalDeliveryMethodsController InstructionalDeliveryMethodsController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            InstructionalDeliveryMethodsController = new InstructionalDeliveryMethodsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            InstructionalDeliveryMethodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            InstructionalDeliveryMethodsController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task InstructionalDeliveryMethodsController_GetAll()
        {
            var InstructionalDeliveryMethods = await InstructionalDeliveryMethodsController.GetInstructionalDeliveryMethodsAsync();

            Assert.IsTrue(InstructionalDeliveryMethods.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructionalDeliveryMethodsController_GetInstructionalDeliveryMethodsByGuid_Exception()
        {
            await InstructionalDeliveryMethodsController.GetInstructionalDeliveryMethodByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructionalDeliveryMethodsController_PostInstructionalDeliveryMethodsAsync_Exception()
        {
            await InstructionalDeliveryMethodsController.PostInstructionalDeliveryMethodAsync(It.IsAny<InstructionalDeliveryMethod>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructionalDeliveryMethodsController_PutInstructionalDeliveryMethodsAsync_Exception()
        {

            await InstructionalDeliveryMethodsController.PutInstructionalDeliveryMethodAsync(It.IsAny<string>(), It.IsAny<InstructionalDeliveryMethod>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructionalDeliveryMethodsController_DeleteInstructionalDeliveryMethodsAsync_Exception()
        {
            await InstructionalDeliveryMethodsController.DeleteInstructionalDeliveryMethodAsync(It.IsAny<string>());
        }
    }
}