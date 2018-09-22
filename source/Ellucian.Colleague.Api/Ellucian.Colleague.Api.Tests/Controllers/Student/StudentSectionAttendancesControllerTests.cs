// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentSectionAttendancesControllerTests
    {
      


        [TestClass]
        public class StudentAttendanceControllerTests_QueryStudentSectionAttendancesAsync
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

            private IStudentSectionAttendancesService studentAttendanceService;
            private Mock<IStudentSectionAttendancesService> studentAttendanceServiceMock;
            private StudentSectionAttendancesController studentAttendanceController;
            private StudentSectionsAttendances studentSectionsAttendances;

            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                var timespan = new TimeSpan();
                var startTime = new DateTimeOffset(2017, 12, 1, 9, 15, 00, timespan);
                var endTime = new DateTimeOffset(2017, 12, 1, 10, 15, 0, timespan);
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentAttendanceServiceMock = new Mock<IStudentSectionAttendancesService>();
                studentAttendanceService = studentAttendanceServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                var studentAttendanceDtos = new List<StudentAttendance>()
                {
                    new StudentAttendance() {StudentId = "1111111", SectionId = "SEC1", StudentCourseSectionId = "1234", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "P", Comment = "Comment for 1111111"},
                     new StudentAttendance() {StudentId = "1111111", SectionId = "SEC1", StudentCourseSectionId = "1234", MeetingDate = new DateTime(2018, 1, 6), AttendanceCategoryCode = "A", Comment = "Comment for 1111111"},
                    new StudentAttendance() {StudentId = "1111111", SectionId = "SEC2", StudentCourseSectionId = "1234", MeetingDate = new DateTime(2018, 1, 5), AttendanceCategoryCode = "P", Comment = "Comment for 1111111"},

                };
                studentSectionsAttendances = new StudentSectionsAttendances();
                studentSectionsAttendances.SectionWiseAttendances.Add("SEC1", new List<StudentAttendance>() { studentAttendanceDtos[0], studentAttendanceDtos[1]});
                studentSectionsAttendances.SectionWiseAttendances.Add("SEC2", new List<StudentAttendance>() { studentAttendanceDtos[2] });
                studentAttendanceController = new StudentSectionAttendancesController(studentAttendanceService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAttendanceController = null;
                studentAttendanceService = null;
            }

            [TestMethod]
            public async Task QueryStudentSectionsAttendancesAsync_ReturnsStudentAttendanceDtos()
            {
                var criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = "1111111", SectionIds = new List<string>() { "SEC1" ,"SEC2","SEC3"} };
                studentAttendanceServiceMock.Setup(x => x.QueryStudentSectionAttendancesAsync(criteria)).Returns(Task.FromResult(studentSectionsAttendances));
                var results = await studentAttendanceController.QueryStudentSectionAttendancesAsync(criteria);
                Assert.AreEqual(2, studentSectionsAttendances.SectionWiseAttendances.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendances2Async_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    var criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = "1111112", SectionIds = new List<string>() { "SectionId" } };
                    studentAttendanceServiceMock.Setup(x => x.QueryStudentSectionAttendancesAsync(criteria)).Throws(new PermissionsException());
                    var studentAttendance = await studentAttendanceController.QueryStudentSectionAttendancesAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendances2Async_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = "student1", SectionIds = new List<string>() { "SectionId" } };
                    studentAttendanceServiceMock.Setup(x => x.QueryStudentSectionAttendancesAsync(criteria)).Throws(new Exception());
                    var studentAttendance = await studentAttendanceController.QueryStudentSectionAttendancesAsync(criteria);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendances2Async_NullCriteria_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var studentAttendance = await studentAttendanceController.QueryStudentSectionAttendancesAsync(null);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendances2Async_CriteriaNoStudentId_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = null, SectionIds = new List<string>() {"S1" } };
                    var studentAttendance = await studentAttendanceController.QueryStudentSectionAttendancesAsync(criteria);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task QueryStudentAttendances2Async_CriteriaEmptySectionId_ReturnsOK()
            {
                try
                {
                    var criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = "student1", SectionIds = new List<string>() };
                    studentAttendanceServiceMock.Setup(x => x.QueryStudentSectionAttendancesAsync(criteria)).Returns(Task.FromResult(studentSectionsAttendances));
                    var studentAttendance = await studentAttendanceController.QueryStudentSectionAttendancesAsync(criteria);
                    Assert.IsNotNull(studentAttendance);
                    Assert.AreEqual(2, studentAttendance.SectionWiseAttendances.Count());

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendances2Async_CriteriaEmptyStudentId_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = string.Empty, SectionIds = new List<string>() { "sectionId" } };
                    var studentAttendance = await studentAttendanceController.QueryStudentSectionAttendancesAsync(criteria);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

           
        }
        
    }
}


    

