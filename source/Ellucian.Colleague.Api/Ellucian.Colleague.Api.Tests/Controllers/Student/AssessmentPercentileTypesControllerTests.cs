//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AssessmentPercentileTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAssessmentPercentileTypesService> assessmentPercentileTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private AssessmentPercentileTypesController assessmentPercentileTypesController;
        private IEnumerable<IntgTestPercentileType> allIntgTestPercentileTypes;
        private List<Dtos.AssessmentPercentileTypes> assessmentPercentileTypesCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            assessmentPercentileTypesServiceMock = new Mock<IAssessmentPercentileTypesService>();
            loggerMock = new Mock<ILogger>();
            assessmentPercentileTypesCollection = new List<Dtos.AssessmentPercentileTypes>();

            allIntgTestPercentileTypes = new List<IntgTestPercentileType>()
                {
                    new IntgTestPercentileType("792b6834-2f9c-409c-8afa-e0081972adb4", "1", "1st percentile"),
                    new IntgTestPercentileType("ab8395f3-663d-4d09-b3f6-af28668dc362", "2", "2nd percentile")
                };

            foreach (var source in allIntgTestPercentileTypes)
            {
                var assessmentPercentileTypes = new Ellucian.Colleague.Dtos.AssessmentPercentileTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                assessmentPercentileTypesCollection.Add(assessmentPercentileTypes);
            }

            assessmentPercentileTypesController = new AssessmentPercentileTypesController(assessmentPercentileTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            assessmentPercentileTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            assessmentPercentileTypesController = null;
            allIntgTestPercentileTypes = null;
            assessmentPercentileTypesCollection = null;
            loggerMock = null;
            assessmentPercentileTypesServiceMock = null;
        }

        [TestMethod]
        public async Task AssessmentPercentileTypesController_GetAssessmentPercentileTypes_ValidateFields_Nocache()
        {
            assessmentPercentileTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            assessmentPercentileTypesServiceMock.Setup(x => x.GetAssessmentPercentileTypesAsync(false)).ReturnsAsync(assessmentPercentileTypesCollection);

            var sourceContexts = (await assessmentPercentileTypesController.GetAssessmentPercentileTypesAsync()).ToList();
            Assert.AreEqual(assessmentPercentileTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = assessmentPercentileTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AssessmentPercentileTypesController_GetAssessmentPercentileTypes_ValidateFields_Cache()
        {
            assessmentPercentileTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            assessmentPercentileTypesServiceMock.Setup(x => x.GetAssessmentPercentileTypesAsync(true)).ReturnsAsync(assessmentPercentileTypesCollection);

            var sourceContexts = (await assessmentPercentileTypesController.GetAssessmentPercentileTypesAsync()).ToList();
            Assert.AreEqual(assessmentPercentileTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = assessmentPercentileTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AssessmentPercentileTypesController_GetAssessmentPercentileTypesByGuidAsync_ValidateFields()
        {
            var expected = assessmentPercentileTypesCollection.FirstOrDefault();
            assessmentPercentileTypesServiceMock.Setup(x => x.GetAssessmentPercentileTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await assessmentPercentileTypesController.GetAssessmentPercentileTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentPercentileTypesController_GetAssessmentPercentileTypes_Exception()
        {
            assessmentPercentileTypesServiceMock.Setup(x => x.GetAssessmentPercentileTypesAsync(false)).Throws<Exception>();
            await assessmentPercentileTypesController.GetAssessmentPercentileTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentPercentileTypesController_GetAssessmentPercentileTypesByGuidAsync_Exception()
        {
            assessmentPercentileTypesServiceMock.Setup(x => x.GetAssessmentPercentileTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await assessmentPercentileTypesController.GetAssessmentPercentileTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentPercentileTypesController_PostAssessmentPercentileTypesAsync_Exception()
        {
            await assessmentPercentileTypesController.PostAssessmentPercentileTypesAsync(assessmentPercentileTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentPercentileTypesController_PutAssessmentPercentileTypesAsync_Exception()
        {
            var sourceContext = assessmentPercentileTypesCollection.FirstOrDefault();
            await assessmentPercentileTypesController.PutAssessmentPercentileTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentPercentileTypesController_DeleteAssessmentPercentileTypesAsync_Exception()
        {
            await assessmentPercentileTypesController.DeleteAssessmentPercentileTypesAsync(assessmentPercentileTypesCollection.FirstOrDefault().Id);
        }
    }
}