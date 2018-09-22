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
    public class StudentTagsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ILogger> loggerMock;
        private StudentTagsController StudentTagsController;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            loggerMock = new Mock<ILogger>();

            StudentTagsController = new StudentTagsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            StudentTagsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            StudentTagsController = null;
            loggerMock = null;

        }



        [TestMethod]
        public async Task StudentTagsController_GetAll()
        {
            var StudentTags = await StudentTagsController.GetStudentTagsAsync();

            Assert.IsTrue(StudentTags.Count().Equals(0));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTagsController_GetStudentTagsByGuid_Exception()
        {
            await StudentTagsController.GetStudentTagByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTagsController_PostStudentTagsAsync_Exception()
        {
            await StudentTagsController.PostStudentTagAsync(It.IsAny<StudentTag>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTagsController_PutStudentTagsAsync_Exception()
        {

            await StudentTagsController.PutStudentTagAsync(It.IsAny<string>(), It.IsAny<StudentTag>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTagsController_DeleteStudentTagsAsync_Exception()
        {
            await StudentTagsController.DeleteStudentTagAsync(It.IsAny<string>());
        }
    }
}