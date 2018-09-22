// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
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
    public class NonAcademicAttendanceRequirementsControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<ILogger> _loggerMock;
        private Mock<INonAcademicAttendanceService> _nonAcademicAttendanceServiceMock;

        private string personId;
        private List<NonAcademicAttendanceRequirement> dtos;

        private NonAcademicAttendanceRequirementsController _nonAcademicAttendanceRequirementsController;


        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _loggerMock = new Mock<ILogger>();
            _nonAcademicAttendanceServiceMock = new Mock<INonAcademicAttendanceService>();

            personId = "0001234";
            dtos = new List<NonAcademicAttendanceRequirement>()
            {
                new NonAcademicAttendanceRequirement()
                {
                    DefaultRequiredUnits = 30m,
                    Id = "1",
                    NonAcademicAttendanceIds = new List<string>() { "11", "12" },
                    PersonId = personId,
                    RequiredUnits = 24m,
                    RequiredUnitsOverride = 24m,
                    TermCode = "TERM"
                },
                new NonAcademicAttendanceRequirement()
                {
                    DefaultRequiredUnits = 30m,
                    Id = "2",
                    NonAcademicAttendanceIds = new List<string>() { "13", "14" },
                    PersonId = personId,
                    RequiredUnits = 30m,
                    TermCode = "TERM2"
                }
            };

            _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendanceRequirementsAsync(personId)).ReturnsAsync(dtos);

            _nonAcademicAttendanceRequirementsController = new NonAcademicAttendanceRequirementsController(_nonAcademicAttendanceServiceMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _nonAcademicAttendanceServiceMock = null;
            personId = null;
            dtos = null;
            _nonAcademicAttendanceRequirementsController = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonAcademicAttendanceRequirementsController_GetNonAcademicAttendanceRequirementsAsync_Permissions_Exception()
        {
            _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendanceRequirementsAsync(personId)).ThrowsAsync(new PermissionsException());
            _nonAcademicAttendanceRequirementsController = new NonAcademicAttendanceRequirementsController(_nonAcademicAttendanceServiceMock.Object,
                _loggerMock.Object);
            var expectedDtos = await _nonAcademicAttendanceRequirementsController.GetNonAcademicAttendanceRequirementsAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonAcademicAttendanceRequirementsController_GetNonAcademicAttendanceRequirementsAsync_ColleagueDataReader_Exception()
        {
            _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendanceRequirementsAsync(personId)).ThrowsAsync(new ColleagueDataReaderException("Could not read file."));
            _nonAcademicAttendanceRequirementsController = new NonAcademicAttendanceRequirementsController(_nonAcademicAttendanceServiceMock.Object,
                _loggerMock.Object);
            var expectedDtos = await _nonAcademicAttendanceRequirementsController.GetNonAcademicAttendanceRequirementsAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonAcademicAttendanceRequirementsController_GetNonAcademicAttendanceRequirementsAsync_Generic_Exception()
        {
            _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendanceRequirementsAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            _nonAcademicAttendanceRequirementsController = new NonAcademicAttendanceRequirementsController(_nonAcademicAttendanceServiceMock.Object,
                _loggerMock.Object);
            var expectedDtos = await _nonAcademicAttendanceRequirementsController.GetNonAcademicAttendanceRequirementsAsync(null);
        }

        [TestMethod]
        public async Task NonAcademicAttendanceRequirementsController_GetNonAcademicAttendanceRequirementsAsync_returns_DTOs()
        {
            var expectedDtos = await _nonAcademicAttendanceRequirementsController.GetNonAcademicAttendanceRequirementsAsync(personId);
            CollectionAssert.AreEqual(dtos, expectedDtos.ToList());
        }
    }
}
