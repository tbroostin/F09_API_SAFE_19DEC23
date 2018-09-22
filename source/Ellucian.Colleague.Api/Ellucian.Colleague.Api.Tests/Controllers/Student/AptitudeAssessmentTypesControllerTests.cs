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
    public class AptitudeAssessmentTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAptitudeAssessmentTypesService> aptitudeAssessmentTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private AptitudeAssessmentTypesController aptitudeAssessmentTypesController;
        private IEnumerable<NonCourseCategories> allNonCourseCategories;
        private List<Dtos.AptitudeAssessmentTypes> aptitudeAssessmentTypesCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            aptitudeAssessmentTypesServiceMock = new Mock<IAptitudeAssessmentTypesService>();
            loggerMock = new Mock<ILogger>();
            aptitudeAssessmentTypesCollection = new List<Dtos.AptitudeAssessmentTypes>();

            allNonCourseCategories = new List<NonCourseCategories>()
            {
                new NonCourseCategories("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                new NonCourseCategories("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                new NonCourseCategories("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
            };

            foreach (var source in allNonCourseCategories)
            {
                var aptitudeAssessmentTypes = new Ellucian.Colleague.Dtos.AptitudeAssessmentTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                aptitudeAssessmentTypesCollection.Add(aptitudeAssessmentTypes);
            }

            aptitudeAssessmentTypesController = new AptitudeAssessmentTypesController(aptitudeAssessmentTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            aptitudeAssessmentTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            aptitudeAssessmentTypesController = null;
            allNonCourseCategories = null;
            aptitudeAssessmentTypesCollection = null;
            loggerMock = null;
            aptitudeAssessmentTypesServiceMock = null;
        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesController_GetAptitudeAssessmentTypes_ValidateFields_Nocache()
        {
            aptitudeAssessmentTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = false};

            aptitudeAssessmentTypesServiceMock.Setup(x => x.GetAptitudeAssessmentTypesAsync(false)).ReturnsAsync(aptitudeAssessmentTypesCollection);

            var sourceContexts = (await aptitudeAssessmentTypesController.GetAptitudeAssessmentTypesAsync()).ToList();
            Assert.AreEqual(aptitudeAssessmentTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = aptitudeAssessmentTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesController_GetAptitudeAssessmentTypes_ValidateFields_Cache()
        {
            aptitudeAssessmentTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            aptitudeAssessmentTypesServiceMock.Setup(x => x.GetAptitudeAssessmentTypesAsync(true)).ReturnsAsync(aptitudeAssessmentTypesCollection);

            var sourceContexts = (await aptitudeAssessmentTypesController.GetAptitudeAssessmentTypesAsync()).ToList();
            Assert.AreEqual(aptitudeAssessmentTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = aptitudeAssessmentTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesController_GetAptitudeAssessmentTypesByGuidAsync_ValidateFields()
        {
            var expected = aptitudeAssessmentTypesCollection.FirstOrDefault();
            aptitudeAssessmentTypesServiceMock.Setup(x => x.GetAptitudeAssessmentTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var response = await aptitudeAssessmentTypesController.GetAptitudeAssessmentTypesByGuidAsync(expected.Id);

            var actual = response.Content.ReadAsAsync<Dtos.AptitudeAssessmentTypes>().Result;

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AptitudeAssessmentTypesController_GetAptitudeAssessmentTypes_Exception()
        {
            aptitudeAssessmentTypesServiceMock.Setup(x => x.GetAptitudeAssessmentTypesAsync(false)).Throws<Exception>();
            await aptitudeAssessmentTypesController.GetAptitudeAssessmentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AptitudeAssessmentTypesController_GetAptitudeAssessmentTypesByGuidAsync_Exception()
        {
            aptitudeAssessmentTypesServiceMock.Setup(x => x.GetAptitudeAssessmentTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await aptitudeAssessmentTypesController.GetAptitudeAssessmentTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AptitudeAssessmentTypesController_PostAptitudeAssessmentTypesAsync_Exception()
        {
            await aptitudeAssessmentTypesController.PostAptitudeAssessmentTypesAsync(aptitudeAssessmentTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AptitudeAssessmentTypesController_PutAptitudeAssessmentTypesAsync_Exception()
        {
            var sourceContext = aptitudeAssessmentTypesCollection.FirstOrDefault();
            await aptitudeAssessmentTypesController.PutAptitudeAssessmentTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AptitudeAssessmentTypesController_DeleteAptitudeAssessmentTypesAsync_Exception()
        {
            await aptitudeAssessmentTypesController.DeleteAptitudeAssessmentTypesAsync(aptitudeAssessmentTypesCollection.FirstOrDefault().Id);
        }
    }
}