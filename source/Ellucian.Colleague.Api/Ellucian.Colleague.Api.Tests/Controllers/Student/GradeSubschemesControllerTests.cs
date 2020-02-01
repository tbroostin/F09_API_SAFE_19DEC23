// Copyright 2019 Ellucian Company L.P. and its affiliates.
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
    public class GradeSubschemesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IGradeSchemeService> gradeSchemeServiceMock;
        private Mock<ILogger> loggerMock;
        private GradeSubschemesController GradeSubschemesController;
        private IEnumerable<GradeSubscheme> allGradeSubscheme;
        private List<Dtos.Student.GradeSubscheme> gradeSchemeCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            gradeSchemeServiceMock = new Mock<IGradeSchemeService>();
            loggerMock = new Mock<ILogger>();
            gradeSchemeCollection = new List<Dtos.Student.GradeSubscheme>();

            allGradeSubscheme = (await new TestStudentReferenceDataRepository().GetGradeSubschemesAsync()).ToList();

            foreach (var source in allGradeSubscheme)
            {
                var gradeScheme = new Ellucian.Colleague.Dtos.Student.GradeSubscheme
                {
                    Code = source.Code,
                    Description = source.Description,
                    GradeCodes = source.GradeCodes
                };
                gradeSchemeCollection.Add(gradeScheme);
            }

            GradeSubschemesController = new GradeSubschemesController(gradeSchemeServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            GradeSubschemesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            GradeSubschemesController = null;
            allGradeSubscheme = null;
            gradeSchemeCollection = null;
            loggerMock = null;
            gradeSchemeServiceMock = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSubschemesController_GetGradeSubschemeByIdAsync_KeyNotFoundException()
        {
            gradeSchemeServiceMock.Setup(x => x.GetGradeSubschemeByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await GradeSubschemesController.GetGradeSubschemeByIdAsync("UG");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeSubschemesController_GetGradeSubschemeByIdAsync_GenericException()
        {
            gradeSchemeServiceMock.Setup(x => x.GetGradeSubschemeByIdAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
            await GradeSubschemesController.GetGradeSubschemeByIdAsync("UG");
        }

        [TestMethod]
        public async Task GradeSubschemesController_GetGradeSubschemeByIdAsync_Valid()
        {
            gradeSchemeServiceMock.Setup(x => x.GetGradeSubschemeByIdAsync("UG")).ReturnsAsync(new Dtos.Student.GradeSubscheme() { Code = "UG", Description = "Undergraduate", GradeCodes = new List<string>() { "A", "B" } });
            var scheme = await GradeSubschemesController.GetGradeSubschemeByIdAsync("UG");
            Assert.AreEqual("UG", scheme.Code);
        }
    }
}