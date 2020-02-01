//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentTagAssignmentsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        private Mock<ILogger> loggerMock;
        private StudentTagAssignmentsController studentTagAssignmentsController;      
        
        [TestInitialize]
        public void Initialize() 
        {
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            loggerMock = new Mock<ILogger>();
         
            studentTagAssignmentsController = new StudentTagAssignmentsController(loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentTagAssignmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }
        [TestCleanup]
        public void Cleanup()
        {
            studentTagAssignmentsController = null;           
            loggerMock = null;
        
        }
       
        
        [TestMethod]
        public async Task StudentTagAssignmentsController_GetAll()
        {
            var studentTagAssignments = await studentTagAssignmentsController.GetStudentTagAssignmentsAsync();
            Assert.IsTrue(studentTagAssignments.Count().Equals(0));        
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTagAssignmentsController_GetStudentTagAssignmentsByGuid_Exception()
        {
            await studentTagAssignmentsController.GetStudentTagAssignmentsByGuidAsync(It.IsAny<string>());
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTagAssignmentsController_PostStudentTagAssignmentsAsync_Exception()
        {
            await studentTagAssignmentsController.PostStudentTagAssignmentsAsync(It.IsAny<Dtos.StudentTagAssignments>());
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTagAssignmentsController_PutStudentTagAssignmentsAsync_Exception()
        {
 
            await studentTagAssignmentsController.PutStudentTagAssignmentsAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentTagAssignments>());
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTagAssignmentsController_DeleteStudentTagAssignmentsAsync_Exception()
        {
            await studentTagAssignmentsController.DeleteStudentTagAssignmentsAsync(It.IsAny<string>());
        }
    }
}