// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Student.AnonymousGrading;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class PreliminaryAnonymousGradesControllerTests
    {
        public TestContext TestContext { get; set; }

        public PreliminaryAnonymousGradesController controller;

        public Mock<IPreliminaryAnonymousGradeService> preliminaryAnonymousGradeServiceMock;
        public Mock<ILogger> loggerMock;

        public IPreliminaryAnonymousGradeService preliminaryAnonymousGradeService;
        public ILogger logger;

        [TestInitialize]
        public void PreliminaryAnonymousGradesControllerTests_Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            preliminaryAnonymousGradeServiceMock = new Mock<IPreliminaryAnonymousGradeService>();
            preliminaryAnonymousGradeService = preliminaryAnonymousGradeServiceMock.Object;

            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            controller = new PreliminaryAnonymousGradesController(preliminaryAnonymousGradeService, logger);
        }
    }

    [TestClass]
    public class GetPreliminaryAnonymousGradesBySectionIdAsyncTests : PreliminaryAnonymousGradesControllerTests
    {
        string sectionId;

        [TestInitialize]
        public void GetPreliminaryAnonymousGradesBySectionIdAsyncTests_Initialize()
        {
            base.PreliminaryAnonymousGradesControllerTests_Initialize();
            sectionId = "12345";
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_null_sectionId_BadRequest()
        {
            bool correctException = false;
            try
            {
                var response = await controller.GetPreliminaryAnonymousGradesBySectionIdAsync(null);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_PermissionsException_Forbidden()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId)).ThrowsAsync(new PermissionsException());

            bool correctException = false;
            try
            {
                var response = await controller.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.Forbidden;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_KeyNotFoundException_NotFound()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId)).ThrowsAsync(new KeyNotFoundException());

            bool correctException = false;
            try
            {
                var response = await controller.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_ConfigurationException_BadRequest()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId)).ThrowsAsync(new ConfigurationException());

            bool correctException = false;
            try
            {
                var response = await controller.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_ColleagueException_BadRequest()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId)).ThrowsAsync(new ColleagueException());

            bool correctException = false;
            try
            {
                var response = await controller.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_Exception_BadRequest()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId)).ThrowsAsync(new ArgumentException());

            bool correctException = false;
            try
            {
                var response = await controller.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                preliminaryAnonymousGradeServiceMock.Setup(svc => svc.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId))
                    .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                await controller.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        public async Task GetPreliminaryAnonymousGradesBySectionIdAsync_Valid()
        {
            // Set up coordination service to throw a permissions exception
            var dto = new SectionPreliminaryAnonymousGrading()
            {
                SectionId = sectionId,
                AnonymousGradesForSection = new List<PreliminaryAnonymousGrade>()
                {
                    new PreliminaryAnonymousGrade()
                },
                AnonymousGradesForCrosslistedSections = new List<PreliminaryAnonymousGrade>()
                {
                    new PreliminaryAnonymousGrade()
                },
                Errors = new List<AnonymousGradeError>()
                {
                    new AnonymousGradeError()
                }
            };
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId)).ReturnsAsync(dto);

            var response = await controller.GetPreliminaryAnonymousGradesBySectionIdAsync(sectionId);
            Assert.AreEqual(dto.SectionId, response.SectionId);
            CollectionAssert.AreEqual(dto.AnonymousGradesForSection.ToList(), response.AnonymousGradesForSection.ToList());
            CollectionAssert.AreEqual(dto.AnonymousGradesForCrosslistedSections.ToList(), response.AnonymousGradesForCrosslistedSections.ToList());
            CollectionAssert.AreEqual(dto.Errors.ToList(), response.Errors.ToList());
        }
    }

    [TestClass]
    public class UpdatePreliminaryAnonymousGradesBySectionIdAsyncTests : PreliminaryAnonymousGradesControllerTests
    {
        string sectionId;
        IEnumerable<PreliminaryAnonymousGrade> preliminaryAnonymousGrades;

        [TestInitialize]
        public void UpdatePreliminaryAnonymousGradesBySectionIdAsyncTests_Initialize()
        {
            base.PreliminaryAnonymousGradesControllerTests_Initialize();
            sectionId = "12345";
            preliminaryAnonymousGrades = new List<PreliminaryAnonymousGrade>()
            {
                new PreliminaryAnonymousGrade()
                {
                    AnonymousGradingId = "12345",
                    CourseSectionId = sectionId,
                    FinalGradeExpirationDate = null,
                    FinalGradeId = "1",
                    StudentCourseSectionId = "23456"
                },
                new PreliminaryAnonymousGrade()
                {
                    AnonymousGradingId = "12346",
                    CourseSectionId = sectionId,
                    FinalGradeExpirationDate = null,
                    FinalGradeId = "2",
                    StudentCourseSectionId = "23457"
                }
            };
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_null_sectionId_BadRequest()
        {
            bool correctException = false;
            try
            {
                var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(null, preliminaryAnonymousGrades);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_null_preliminaryAnonymousGrades_BadRequest()
        {
            bool correctException = false;
            try
            {
                var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, null);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_empty_preliminaryAnonymousGrades_BadRequest()
        {
            bool correctException = false;
            try
            {
                var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, new List<PreliminaryAnonymousGrade>());
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_PermissionsException_Forbidden()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades)).ThrowsAsync(new PermissionsException());

            bool correctException = false;
            try
            {
                var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.Forbidden;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_KeyNotFoundException_NotFound()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades)).ThrowsAsync(new KeyNotFoundException());

            bool correctException = false;
            try
            {
                var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_ConfigurationException_BadRequest()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades)).ThrowsAsync(new ConfigurationException());

            bool correctException = false;
            try
            {
                var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_ColleagueException_BadRequest()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades)).ThrowsAsync(new ColleagueException());

            bool correctException = false;
            try
            {
                var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Exception_BadRequest()
        {
            // Set up coordination service to throw a permissions exception
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades)).ThrowsAsync(new ArgumentException());

            bool correctException = false;
            try
            {
                var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            }
            catch (HttpResponseException ex)
            {
                correctException = ex.Response.StatusCode == System.Net.HttpStatusCode.BadRequest;
            }
            Assert.IsTrue(correctException);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                preliminaryAnonymousGradeServiceMock.Setup(svc => svc.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades))
                    .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        public async Task UpdatePreliminaryAnonymousGradesBySectionIdAsync_Valid()
        {
            // Set up coordination service to throw a permissions exception
            var dto = new List<PreliminaryAnonymousGradeUpdateResult>()
            {
                new PreliminaryAnonymousGradeUpdateResult()
                {
                    StudentCourseSectionId = "23456",
                    Message = "",
                    Status = PreliminaryAnonymousGradeUpdateStatus.Success
                },
                new PreliminaryAnonymousGradeUpdateResult()
                {
                    StudentCourseSectionId = "23457",
                    Message = "Invalid grade ID '2'",
                    Status = PreliminaryAnonymousGradeUpdateStatus.Failure
                }
            };
            preliminaryAnonymousGradeServiceMock.Setup(svc => svc.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, It.IsAny<IEnumerable<PreliminaryAnonymousGrade>>())).ReturnsAsync(dto);
            var response = await controller.UpdatePreliminaryAnonymousGradesBySectionIdAsync(sectionId, preliminaryAnonymousGrades);

            CollectionAssert.AreEqual(dto, response.ToList());
        }
    }
}
