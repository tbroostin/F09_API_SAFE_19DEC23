// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
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
    public class NonAcademicAttendancesControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<ILogger> _loggerMock;
        private Mock<INonAcademicAttendanceService> _nonAcademicAttendanceServiceMock;

        private string personId;
        private List<NonAcademicAttendance> dtos;

        private NonAcademicAttendancesController _nonAcademicAttendancesController;


        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _loggerMock = new Mock<ILogger>();
            _nonAcademicAttendanceServiceMock = new Mock<INonAcademicAttendanceService>();

            personId = "0001234";
            dtos = new List<NonAcademicAttendance>()
            {
                new NonAcademicAttendance()
                {
                    Id = "1",
                    PersonId = personId,
                    UnitsEarned = 24m,
                    EventId = "11"
                },
                new NonAcademicAttendance()
                {
                    Id = "2",
                    PersonId = personId,
                    UnitsEarned = 30m,
                    EventId = "12"
                }
            };

            _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendancesAsync(personId)).ReturnsAsync(dtos);

            _nonAcademicAttendancesController = new NonAcademicAttendancesController(_nonAcademicAttendanceServiceMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _nonAcademicAttendanceServiceMock = null;
            personId = null;
            dtos = null;
            _nonAcademicAttendancesController = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonAcademicAttendancesController_GetNonAcademicAttendancesAsync_Permissions_Exception()
        {
            _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendancesAsync(personId)).ThrowsAsync(new PermissionsException());
            _nonAcademicAttendancesController = new NonAcademicAttendancesController(_nonAcademicAttendanceServiceMock.Object,
                _loggerMock.Object);
            var expectedDtos = await _nonAcademicAttendancesController.GetNonAcademicAttendancesAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonAcademicAttendancesController_GetNonAcademicAttendancesAsync_ColleagueDataReader_Exception()
        {
            _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendancesAsync(personId)).ThrowsAsync(new ColleagueDataReaderException("Could not read file."));
            _nonAcademicAttendancesController = new NonAcademicAttendancesController(_nonAcademicAttendanceServiceMock.Object,
                _loggerMock.Object);
            var expectedDtos = await _nonAcademicAttendancesController.GetNonAcademicAttendancesAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonAcademicAttendancesController_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendancesAsync(personId))
                    .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                _nonAcademicAttendancesController = new NonAcademicAttendancesController(_nonAcademicAttendanceServiceMock.Object,
                    _loggerMock.Object);
                await _nonAcademicAttendancesController.GetNonAcademicAttendancesAsync(personId);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NonAcademicAttendancesController_GetNonAcademicAttendancesAsync_Generic_Exception()
        {
            _nonAcademicAttendanceServiceMock.Setup(svc => svc.GetNonAcademicAttendancesAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            _nonAcademicAttendancesController = new NonAcademicAttendancesController(_nonAcademicAttendanceServiceMock.Object,
                _loggerMock.Object);
            var expectedDtos = await _nonAcademicAttendancesController.GetNonAcademicAttendancesAsync(null);
        }

        [TestMethod]
        public async Task NonAcademicAttendancesController_GetNonAcademicAttendancesAsync_returns_DTOs()
        {
            var expectedDtos = await _nonAcademicAttendancesController.GetNonAcademicAttendancesAsync(personId);
            CollectionAssert.AreEqual(dtos, expectedDtos.ToList());
        }
    }
}
