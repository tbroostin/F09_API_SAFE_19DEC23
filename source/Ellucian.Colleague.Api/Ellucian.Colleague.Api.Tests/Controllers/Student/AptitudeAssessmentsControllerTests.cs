//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AptitudeAssessmentsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAptitudeAssessmentsService> aptitudeAssessmentsServiceMock;
        private Mock<ILogger> loggerMock;
        private AptitudeAssessmentsController aptitudeAssessmentsController;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.NonCourse> allNonCourses;
        private List<Dtos.AptitudeAssessment> aptitudeAssessmentsCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            aptitudeAssessmentsServiceMock = new Mock<IAptitudeAssessmentsService>();
            loggerMock = new Mock<ILogger>();
            aptitudeAssessmentsCollection = new List<Dtos.AptitudeAssessment>();

            allNonCourses = new List<Ellucian.Colleague.Domain.Student.Entities.NonCourse>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.NonCourse("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT"),
                    new Ellucian.Colleague.Domain.Student.Entities.NonCourse("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC"),
                    new Ellucian.Colleague.Domain.Student.Entities.NonCourse("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU")
                };

            foreach (var source in allNonCourses)
            {
                var aptitudeAssessments = new Ellucian.Colleague.Dtos.AptitudeAssessment
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                aptitudeAssessmentsCollection.Add(aptitudeAssessments);
            }

            aptitudeAssessmentsController = new AptitudeAssessmentsController(aptitudeAssessmentsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            aptitudeAssessmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            aptitudeAssessmentsController = null;
            allNonCourses = null;
            aptitudeAssessmentsCollection = null;
            loggerMock = null;
            aptitudeAssessmentsServiceMock = null;
        }

        [TestMethod]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessments_ValidateFields_Nocache()
        {
            aptitudeAssessmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsAsync(false)).ReturnsAsync(aptitudeAssessmentsCollection);

            var sourceContexts = (await aptitudeAssessmentsController.GetAptitudeAssessmentsAsync()).ToList();
            Assert.AreEqual(aptitudeAssessmentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = aptitudeAssessmentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessments_ValidateFields_Cache()
        {
            aptitudeAssessmentsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsAsync(true)).ReturnsAsync(aptitudeAssessmentsCollection);

            var sourceContexts = (await aptitudeAssessmentsController.GetAptitudeAssessmentsAsync()).ToList();
            Assert.AreEqual(aptitudeAssessmentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = aptitudeAssessmentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessmentsByGuidAsync_ValidateFields()
        {
            var expected = aptitudeAssessmentsCollection.FirstOrDefault();
            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await aptitudeAssessmentsController.GetAptitudeAssessmentsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessments_Exception()
        {
            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsAsync(false)).Throws<Exception>();
            await aptitudeAssessmentsController.GetAptitudeAssessmentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessments_KeyNotFoundException()
        {
            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsAsync(false)).Throws<KeyNotFoundException>();
            await aptitudeAssessmentsController.GetAptitudeAssessmentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessments_ArgumentNullException()
        {
            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsAsync(false)).Throws<ArgumentNullException>();
            await aptitudeAssessmentsController.GetAptitudeAssessmentsAsync();
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessmentsByGuidAsync_Null_Id_Exception()
        {
            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await aptitudeAssessmentsController.GetAptitudeAssessmentsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessmentsByGuidAsync_Exception()
        {
            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await aptitudeAssessmentsController.GetAptitudeAssessmentsByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessmentsByGuidAsync_KeyNotFoundException()
        {
            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await aptitudeAssessmentsController.GetAptitudeAssessmentsByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_GetAptitudeAssessmentsByGuidAsync_ArgumentNullException()
        {
            aptitudeAssessmentsServiceMock.Setup(x => x.GetAptitudeAssessmentsByGuidAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
            await aptitudeAssessmentsController.GetAptitudeAssessmentsByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_PostAptitudeAssessmentsAsync_Exception()
        {
            await aptitudeAssessmentsController.PostAptitudeAssessmentsAsync(aptitudeAssessmentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_PutAptitudeAssessmentsAsync_Exception()
        {
            var sourceContext = aptitudeAssessmentsCollection.FirstOrDefault();
            await aptitudeAssessmentsController.PutAptitudeAssessmentsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentsController_DeleteAptitudeAssessmentsAsync_Exception()
        {
            await aptitudeAssessmentsController.DeleteAptitudeAssessmentsAsync(aptitudeAssessmentsCollection.FirstOrDefault().Id);
        }
    }
}