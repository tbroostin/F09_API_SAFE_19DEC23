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
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AssessmentCalculationMethodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAssessmentCalculationMethodsService> assessmentCalculationMethodsServiceMock;
        private Mock<ILogger> loggerMock;
        private AssessmentCalculationMethodsController assessmentCalculationMethodsController;
        private IEnumerable<NonCourseGradeUses> allNonCourseGradeUses;
        private List<Dtos.AssessmentCalculationMethods> assessmentCalculationMethodsCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            assessmentCalculationMethodsServiceMock = new Mock<IAssessmentCalculationMethodsService>();
            loggerMock = new Mock<ILogger>();
            assessmentCalculationMethodsCollection = new List<Dtos.AssessmentCalculationMethods>();

            allNonCourseGradeUses = new List<NonCourseGradeUses>()
                {
                    new NonCourseGradeUses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new NonCourseGradeUses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new NonCourseGradeUses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allNonCourseGradeUses)
            {
                var assessmentCalculationMethods = new Ellucian.Colleague.Dtos.AssessmentCalculationMethods
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                assessmentCalculationMethodsCollection.Add(assessmentCalculationMethods);
            }

            assessmentCalculationMethodsController = new AssessmentCalculationMethodsController(assessmentCalculationMethodsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            assessmentCalculationMethodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            assessmentCalculationMethodsController = null;
            allNonCourseGradeUses = null;
            assessmentCalculationMethodsCollection = null;
            loggerMock = null;
            assessmentCalculationMethodsServiceMock = null;
        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsController_GetAssessmentCalculationMethods_ValidateFields_Nocache()
        {
            assessmentCalculationMethodsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            assessmentCalculationMethodsServiceMock.Setup(x => x.GetAssessmentCalculationMethodsAsync(false)).ReturnsAsync(assessmentCalculationMethodsCollection);

            var sourceContexts = (await assessmentCalculationMethodsController.GetAssessmentCalculationMethodsAsync()).ToList();
            Assert.AreEqual(assessmentCalculationMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = assessmentCalculationMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsController_GetAssessmentCalculationMethods_ValidateFields_Cache()
        {
            assessmentCalculationMethodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            assessmentCalculationMethodsServiceMock.Setup(x => x.GetAssessmentCalculationMethodsAsync(true)).ReturnsAsync(assessmentCalculationMethodsCollection);

            var sourceContexts = (await assessmentCalculationMethodsController.GetAssessmentCalculationMethodsAsync()).ToList();
            Assert.AreEqual(assessmentCalculationMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = assessmentCalculationMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AssessmentCalculationMethodsController_GetAssessmentCalculationMethodsByGuidAsync_ValidateFields()
        {
            var expected = assessmentCalculationMethodsCollection.FirstOrDefault();
            assessmentCalculationMethodsServiceMock.Setup(x => x.GetAssessmentCalculationMethodsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await assessmentCalculationMethodsController.GetAssessmentCalculationMethodsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentCalculationMethodsController_GetAssessmentCalculationMethods_Exception()
        {
            assessmentCalculationMethodsServiceMock.Setup(x => x.GetAssessmentCalculationMethodsAsync(false)).Throws<Exception>();
            await assessmentCalculationMethodsController.GetAssessmentCalculationMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentCalculationMethodsController_GetAssessmentCalculationMethodsByGuidAsync_Exception()
        {
            assessmentCalculationMethodsServiceMock.Setup(x => x.GetAssessmentCalculationMethodsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await assessmentCalculationMethodsController.GetAssessmentCalculationMethodsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentCalculationMethodsController_PostAssessmentCalculationMethodsAsync_Exception()
        {
            await assessmentCalculationMethodsController.PostAssessmentCalculationMethodsAsync(assessmentCalculationMethodsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentCalculationMethodsController_PutAssessmentCalculationMethodsAsync_Exception()
        {
            var sourceContext = assessmentCalculationMethodsCollection.FirstOrDefault();
            await assessmentCalculationMethodsController.PutAssessmentCalculationMethodsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentCalculationMethodsController_DeleteAssessmentCalculationMethodsAsync_Exception()
        {
            await assessmentCalculationMethodsController.DeleteAssessmentCalculationMethodsAsync(assessmentCalculationMethodsCollection.FirstOrDefault().Id);
        }
    }
}