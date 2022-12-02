// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SectionPermissionsControllerTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private ISectionPermissionService sectionPermissionService;
        private Mock<ISectionPermissionService> sectionPermissionServiceMock;
        private SectionPermissionsController sectionPermissionsController;
        private SectionPermission sectionPermissionDto;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            sectionPermissionServiceMock = new Mock<ISectionPermissionService>();
            sectionPermissionService = sectionPermissionServiceMock.Object;
            adapterRegistry = new Mock<IAdapterRegistry>().Object;
            logger = new Mock<ILogger>().Object;

            sectionPermissionDto = BuildSectionPermissionDto();

            sectionPermissionsController = new SectionPermissionsController(sectionPermissionService, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionPermissionsController = null;
            sectionPermissionService = null;
        }

        [TestMethod]
        public async Task GetSectionPermission_ShouldReturnSectionPermissionDto()
        {
            sectionPermissionServiceMock.Setup(service => service.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(sectionPermissionDto));
            var sectionPermission = await sectionPermissionsController.GetSectionPermissionAsync("SEC1");

            Assert.IsTrue(sectionPermission is Dtos.Student.SectionPermission);
            Assert.AreEqual(sectionPermissionDto.StudentPetitions.Count(), sectionPermission.StudentPetitions.Count());
            Assert.AreEqual(sectionPermissionDto.FacultyConsents.Count(), sectionPermission.FacultyConsents.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetSectionPermission_PermissionsException_ReturnsHttpResponseException_Forbidden()
        {
            try
            {
                sectionPermissionServiceMock.Setup(service => service.GetAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var sectionPermission = await sectionPermissionsController.GetSectionPermissionAsync("SEC1");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetSectionPermission_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
        {
            try
            {
                sectionPermissionServiceMock.Setup(service => service.GetAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                var sectionPermission = await sectionPermissionsController.GetSectionPermissionAsync("SEC1");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetSectionPermission_AnyOtherException_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                sectionPermissionServiceMock.Setup(service => service.GetAsync(It.IsAny<string>())).Throws(new ApplicationException());
                var sectionPermission = await sectionPermissionsController.GetSectionPermissionAsync("SEC1");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetSectionPermission_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {

            try
            {
                sectionPermissionServiceMock.Setup(service => service.GetAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                await sectionPermissionsController.GetSectionPermissionAsync("SEC1");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw;
            }
        }

        private SectionPermission BuildSectionPermissionDto()
        {
            var sectionPermission = new SectionPermission();

            var studentPetition1 = new StudentPetition {
                Id = "1",
                StudentId = "0000123",
                SectionId = "SEC1",
                StatusCode = "A",
                ReasonCode = "OVHM",
                Type = StudentPetitionType.StudentPetition,
                Comment = null
            };
            sectionPermission.StudentPetitions.Add(studentPetition1);

            var studentPetition2 = new StudentPetition
            {
                Id = "2",
                StudentId = "0000456",
                SectionId = "SEC1",
                StatusCode = "A",
                ReasonCode = null,
                Type = StudentPetitionType.StudentPetition,
                Comment = "Student 456 ART-101 Petition comment."
            };
            sectionPermission.StudentPetitions.Add(studentPetition2);

            var studentPetition3 = new StudentPetition
            {
                Id = "3",
                StudentId = "0000111",
                SectionId = "SEC1",
                StatusCode = "D",
                ReasonCode = null,
                Type = StudentPetitionType.StudentPetition,
                Comment = "Student 111 ART-101 Petition comment."
            };
            sectionPermission.StudentPetitions.Add(studentPetition3);

            var studentPetition4 = new StudentPetition
            {
                Id = "4",
                StudentId = "0000789",
                SectionId = "SEC1",
                StatusCode = "D",
                ReasonCode = "ICHI",
                Type = StudentPetitionType.StudentPetition,
                Comment = "Student 789 ART-101 Petition comment. Line1 \ncomment line2\ncomment line3 the end."
            };
            sectionPermission.StudentPetitions.Add(studentPetition4);

            var facultyConsent1 = new StudentPetition
            {
                Id = "1",
                StudentId = "0000123",
                SectionId = "SEC1",
                StatusCode = "D",
                ReasonCode = "ICHI",
                Type = StudentPetitionType.FacultyConsent,
                Comment = null
            };
            sectionPermission.FacultyConsents.Add(facultyConsent1);

            var facultyConsent2 = new StudentPetition
            {
                Id = "2",
                StudentId = "0000456",
                SectionId = "SEC1",
                StatusCode = "A",
                ReasonCode = null,
                Type = StudentPetitionType.FacultyConsent,
                Comment = "Student 456 ART-101 Consent comment."
            };
            sectionPermission.FacultyConsents.Add(facultyConsent2);

            var facultyConsent3 = new StudentPetition
            {
                Id = "4",
                StudentId = "0000789",
                SectionId = "SEC1",
                StatusCode = "A",
                ReasonCode = "OVHM",
                Type = StudentPetitionType.FacultyConsent,
                Comment = "Student 789 ART-101 Consent comment. Line1 \ncomment line2\ncomment line3 the end."
            };
            sectionPermission.FacultyConsents.Add(facultyConsent3);

            return sectionPermission;
        }

    }
}
