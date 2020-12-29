// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class GradeSchemesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IGradeSchemeService> gradeSchemeServiceMock;
        private Mock<ILogger> loggerMock;
        private GradeSchemesController gradeSchemesController;      
        private IEnumerable<GradeScheme> allGradeScheme;
        private List<Dtos.GradeScheme2> gradeSchemeCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            gradeSchemeServiceMock = new Mock<IGradeSchemeService>();
            loggerMock = new Mock<ILogger>();
            gradeSchemeCollection = new List<Dtos.GradeScheme2>();

            allGradeScheme = (await new TestStudentReferenceDataRepository().GetGradeSchemesAsync()).ToList();
            
            foreach (var source in allGradeScheme)
            {
                var gradeScheme = new Ellucian.Colleague.Dtos.GradeScheme2
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                gradeSchemeCollection.Add(gradeScheme);
            }

            gradeSchemesController = new GradeSchemesController(gradeSchemeServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            gradeSchemesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            gradeSchemesController = null;
            allGradeScheme = null;
            gradeSchemeCollection = null;
            loggerMock = null;
            gradeSchemeServiceMock = null;
        }

        [TestMethod]
        public async Task GradeSchemeController_GetGradeScheme_ValidateFields_Nocache()
        {
            gradeSchemesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            gradeSchemeServiceMock.Setup(x => x.GetGradeSchemes2Async(false)).ReturnsAsync(gradeSchemeCollection);
       
            var sourceContexts = (await gradeSchemesController.GetGradeSchemes2Async()).ToList();
            Assert.AreEqual(gradeSchemeCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = gradeSchemeCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task GradeSchemeController_GetGradeScheme_ValidateFields_Cache()
        {
            gradeSchemesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            gradeSchemeServiceMock.Setup(x => x.GetGradeSchemes2Async(true)).ReturnsAsync(gradeSchemeCollection);

            var sourceContexts = (await gradeSchemesController.GetGradeSchemes2Async()).ToList();
            Assert.AreEqual(gradeSchemeCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = gradeSchemeCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task GradeSchemeController_GetGradeSchemesByIdAsync_ValidateFields()
        {
            var expected = gradeSchemeCollection.FirstOrDefault();
            gradeSchemeServiceMock.Setup(x => x.GetGradeSchemeByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await gradeSchemesController.GetGradeSchemeByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSchemeController_GetGradeScheme_Exception()
        {
            gradeSchemeServiceMock.Setup(x => x.GetGradeSchemes2Async(false)).Throws<Exception>();
            await gradeSchemesController.GetGradeSchemes2Async();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSchemeController_GetGradeSchemesByIdAsync_Exception()
        {
            gradeSchemeServiceMock.Setup(x => x.GetGradeSchemeByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await gradeSchemesController.GetGradeSchemeByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSchemeController_PostGradeSchemesAsync_Exception()
        {
            await gradeSchemesController.PostGradeSchemeAsync(gradeSchemeCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSchemeController_PutGradeSchemesAsync_Exception()
        {
            var sourceContext = gradeSchemeCollection.FirstOrDefault();
            await gradeSchemesController.PutGradeSchemeAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSchemeController_DeleteGradeSchemesAsync_Exception()
        {
            await gradeSchemesController.DeleteGradeSchemeAsync(gradeSchemeCollection.FirstOrDefault().Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSchemesController_GetNonEthosGradeSchemeByIdAsync_KeyNotFoundException()
        {
            gradeSchemeServiceMock.Setup(x => x.GetNonEthosGradeSchemeByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await gradeSchemesController.GetNonEthosGradeSchemeByIdAsync("UG");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSchemesController_GetNonEthosGradeSchemeByIdAsync_GenericException()
        {
            gradeSchemeServiceMock.Setup(x => x.GetNonEthosGradeSchemeByIdAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
            await gradeSchemesController.GetNonEthosGradeSchemeByIdAsync("UG");
        }

        [TestMethod]
        public async Task GradeSchemesController_GetNonEthosGradeSchemeByIdAsync_Valid()
        {
            gradeSchemeServiceMock.Setup(x => x.GetNonEthosGradeSchemeByIdAsync("UG")).ReturnsAsync(new Dtos.Student.GradeScheme() { Code = "UG", Description = "Undergraduate", GradeCodes = new List<string>() { "A", "B" } });
            var scheme = await gradeSchemesController.GetNonEthosGradeSchemeByIdAsync("UG");
            Assert.AreEqual("UG", scheme.Code);
        }
    }
}