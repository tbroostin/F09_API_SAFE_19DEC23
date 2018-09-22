// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AssessmentSpecialCircumstancesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICurriculumService> AssessmentSpecialCircumstanceServiceMock;
        private Mock<ILogger> loggerMock;
        private AssessmentSpecialCircumstancesController AssessmentSpecialCircumstancesController;      
        private IEnumerable<AssessmentSpecialCircumstance> allAssessmentSpecialCircumstance;
        private List<Dtos.AssessmentSpecialCircumstance> assessmentSpecialCircumstanceCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            AssessmentSpecialCircumstanceServiceMock = new Mock<ICurriculumService>();
            loggerMock = new Mock<ILogger>();
            assessmentSpecialCircumstanceCollection = new List<Dtos.AssessmentSpecialCircumstance>();

            allAssessmentSpecialCircumstance = (await new TestStudentReferenceDataRepository().GetAssessmentSpecialCircumstancesAsync(true)).ToList();
            
            foreach (var source in allAssessmentSpecialCircumstance)
            {
                var assessmentSpecialCircumstance = new Ellucian.Colleague.Dtos.AssessmentSpecialCircumstance
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                };
                assessmentSpecialCircumstanceCollection.Add(assessmentSpecialCircumstance);
            }

            AssessmentSpecialCircumstancesController = new AssessmentSpecialCircumstancesController(AssessmentSpecialCircumstanceServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            AssessmentSpecialCircumstancesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            AssessmentSpecialCircumstancesController = null;
            allAssessmentSpecialCircumstance = null;
            assessmentSpecialCircumstanceCollection = null;
            loggerMock = null;
            AssessmentSpecialCircumstanceServiceMock = null;
        }

        [TestMethod]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstance_ValidateFields_Nocache()
        {
            AssessmentSpecialCircumstancesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstancesAsync(false)).ReturnsAsync(assessmentSpecialCircumstanceCollection);
       
            var sourceContexts = (await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstancesAsync()).ToList();
            Assert.AreEqual(assessmentSpecialCircumstanceCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = assessmentSpecialCircumstanceCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstance_ValidateFields_Cache()
        {
            AssessmentSpecialCircumstancesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstancesAsync(true)).ReturnsAsync(assessmentSpecialCircumstanceCollection);

            var sourceContexts = (await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstancesAsync()).ToList();
            Assert.AreEqual(assessmentSpecialCircumstanceCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = assessmentSpecialCircumstanceCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstancesByIdAsync_ValidateFields()
        {
            var expected = assessmentSpecialCircumstanceCollection.FirstOrDefault();
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstanceByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstanceByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstance_PermissionsException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstancesAsync(false)).Throws<PermissionsException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstancesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstance_KeyNotFoundException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstancesAsync(false)).Throws<KeyNotFoundException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstancesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstance_ArgumentNullException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstancesAsync(false)).Throws<ArgumentNullException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstancesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstance_RepositoryException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstancesAsync(false)).Throws<RepositoryException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstancesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstance_IntgApiException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstancesAsync(false)).Throws<IntegrationApiException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstancesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstance_Exception()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstancesAsync(false)).Throws<Exception>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstancesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstancesByIdAsync_PermissionsException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstanceByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstanceByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstancesByIdAsync_KeyNotFoundException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstanceByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstanceByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstancesByIdAsync_ArgumentNullException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstanceByGuidAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstanceByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstancesByIdAsync_RepositoryException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstanceByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstanceByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstancesByIdAsync_IntgApiException()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstanceByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstanceByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_GetAssessmentSpecialCircumstancesByIdAsync_Exception()
        {
            AssessmentSpecialCircumstanceServiceMock.Setup(x => x.GetAssessmentSpecialCircumstanceByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await AssessmentSpecialCircumstancesController.GetAssessmentSpecialCircumstanceByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_PostAssessmentSpecialCircumstancesAsync_Exception()
        {
            await AssessmentSpecialCircumstancesController.PostAssessmentSpecialCircumstanceAsync(assessmentSpecialCircumstanceCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_PutAssessmentSpecialCircumstancesAsync_Exception()
        {
            var sourceContext = assessmentSpecialCircumstanceCollection.FirstOrDefault();
            await AssessmentSpecialCircumstancesController.PutAssessmentSpecialCircumstanceAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AssessmentSpecialCircumstanceController_DeleteAssessmentSpecialCircumstancesAsync_Exception()
        {
            await AssessmentSpecialCircumstancesController.DeleteAssessmentSpecialCircumstanceAsync(assessmentSpecialCircumstanceCollection.FirstOrDefault().Id);
        }
    }
}