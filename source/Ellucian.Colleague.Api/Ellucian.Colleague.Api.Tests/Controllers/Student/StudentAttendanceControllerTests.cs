// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.
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
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAttendanceControllerTests
    {
        [TestClass]
        public class StudentAttendanceControllerTests_QueryStudentAttendancesAsync
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

            private IStudentAttendanceService studentAttendanceService;
            private Mock<IStudentAttendanceService> studentAttendanceServiceMock;
            private StudentAttendanceController studentAttendanceController;
            private IEnumerable<StudentAttendance> studentAttendances;

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
                studentAttendanceServiceMock = new Mock<IStudentAttendanceService>();
                studentAttendanceService = studentAttendanceServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                studentAttendances = new List<StudentAttendance>()
                {
                    new StudentAttendance() { StudentId = "student1", SectionId = "SectionId", MeetingDate = DateTime.Today, AttendanceCategoryCode = "P", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC", StudentCourseSectionId = "SCS1", Comment = "Comment1"  },
                    new StudentAttendance() { StudentId = "student2", SectionId = "SectionId", MeetingDate = DateTime.Today, AttendanceCategoryCode = "A", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC", StudentCourseSectionId = "SCS2"  },
                    new StudentAttendance() { StudentId = "student3", SectionId = "SectionId", MeetingDate = DateTime.Today, AttendanceCategoryCode = "E", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC", StudentCourseSectionId = "SCS3", Comment = "Comment3"  },
                    new StudentAttendance() { StudentId = "student4", SectionId = "SectionId", MeetingDate = DateTime.Today, AttendanceCategoryCode = "L", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC", StudentCourseSectionId = "SCS4", Comment = "Comment4"  },
                    new StudentAttendance() { StudentId = "student5", SectionId = "SectionId", MeetingDate = DateTime.Today, AttendanceCategoryCode = "P", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC", StudentCourseSectionId = "SCS5", Comment = "Comment5"  },
                    new StudentAttendance() { StudentId = "student6", SectionId = "SectionId", MeetingDate = DateTime.Today, AttendanceCategoryCode = "E" }

                };

                studentAttendanceController = new StudentAttendanceController(studentAttendanceService, logger);
                studentAttendanceController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAttendanceController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentAttendanceController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAttendanceController = null;
                studentAttendanceService = null;
            }

            [TestMethod]
            public async Task QueryStudentAttendancesAsync_ReturnsStudentAttendanceDtos()
            {
                var criteria = new StudentAttendanceQueryCriteria() { SectionId = "SectionId", AttendanceDate = DateTime.Today, IncludeCrossListedAttendances = true };
                studentAttendanceServiceMock.Setup(x => x.QueryStudentAttendancesAsync(criteria, true)).Returns(Task.FromResult(studentAttendances));
                var results = await studentAttendanceController.QueryStudentAttendancesAsync(criteria);
                Assert.AreEqual(studentAttendances.Count(), results.Count() );
            }

            [TestMethod]
            public async Task QueryStudentAttendancesAsync_ReturnsStudentAttendanceDtos_UseCache_false()
            {
                studentAttendanceController = new StudentAttendanceController(studentAttendanceService, logger);
                studentAttendanceController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAttendanceController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentAttendanceController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };

                var criteria = new StudentAttendanceQueryCriteria() { SectionId = "SectionId", AttendanceDate = DateTime.Today, IncludeCrossListedAttendances = true };
                studentAttendanceServiceMock.Setup(x => x.QueryStudentAttendancesAsync(criteria, true)).ThrowsAsync(new Exception("Coordination service called with incorrect useCache param!"));
                studentAttendanceServiceMock.Setup(x => x.QueryStudentAttendancesAsync(criteria, false)).Returns(Task.FromResult(studentAttendances));
                var results = await studentAttendanceController.QueryStudentAttendancesAsync(criteria);
                Assert.AreEqual(studentAttendances.Count(), results.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendancesAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    var criteria = new StudentAttendanceQueryCriteria() { SectionId = "SectionId", AttendanceDate = DateTime.Today, IncludeCrossListedAttendances = true };
                    studentAttendanceServiceMock.Setup(x => x.QueryStudentAttendancesAsync(criteria, true)).Throws(new PermissionsException());
                    var studentAttendance = await studentAttendanceController.QueryStudentAttendancesAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendancesAsync_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var criteria = new StudentAttendanceQueryCriteria() { SectionId = "SectionId", AttendanceDate = DateTime.Today, IncludeCrossListedAttendances = true };
                    studentAttendanceServiceMock.Setup(x => x.QueryStudentAttendancesAsync(criteria, true)).Throws(new Exception());
                    var studentAttendance = await studentAttendanceController.QueryStudentAttendancesAsync(criteria);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendancesAsync_NullCriteria_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var studentAttendance = await studentAttendanceController.QueryStudentAttendancesAsync(null);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendancesAsync_CriteriaNoSection_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var criteria = new StudentAttendanceQueryCriteria() { SectionId = null, AttendanceDate = DateTime.Today, IncludeCrossListedAttendances = true };
                    var studentAttendance = await studentAttendanceController.QueryStudentAttendancesAsync(criteria);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentAttendancesAsync_CriteriaEmptySectionId_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var criteria = new StudentAttendanceQueryCriteria() { SectionId = string.Empty, AttendanceDate = DateTime.Today, IncludeCrossListedAttendances = true };
                    var studentAttendance = await studentAttendanceController.QueryStudentAttendancesAsync(criteria);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

        }


        [TestClass]
        public class StudentAttendanceControllerTests_PutStudentAttendanceAsync
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

            private IStudentAttendanceService studentAttendanceService;
            private Mock<IStudentAttendanceService> studentAttendanceServiceMock;
            private StudentAttendanceController studentAttendanceController;
            private StudentAttendance studentAttendanceToUpdate;

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
                studentAttendanceServiceMock = new Mock<IStudentAttendanceService>();
                studentAttendanceService = studentAttendanceServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                studentAttendanceToUpdate = new StudentAttendance()
                {
                    StudentId = "student1",
                    SectionId = "SectionId",
                    MeetingDate = DateTime.Today,
                    AttendanceCategoryCode = "P",
                    StartTime = startTime,
                    EndTime = endTime,
                    InstructionalMethod = "LEC",
                    StudentCourseSectionId = "SCS1",
                    Comment = "Comment1"
                };


                studentAttendanceController = new StudentAttendanceController(studentAttendanceService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAttendanceController = null;
                studentAttendanceService = null;
            }

            [TestMethod]
            public async Task PutStudentAttendancesAsync_ReturnsStudentAttendanceDto()
            {
                studentAttendanceServiceMock.Setup(x => x.UpdateStudentAttendanceAsync(studentAttendanceToUpdate)).Returns(Task.FromResult(studentAttendanceToUpdate));
                var result = await studentAttendanceController.PutStudentAttendanceAsync(studentAttendanceToUpdate);
                Assert.AreEqual(studentAttendanceToUpdate.SectionId, result.SectionId);
                Assert.AreEqual(studentAttendanceToUpdate.StudentId, result.StudentId);
                Assert.AreEqual(studentAttendanceToUpdate.MeetingDate, result.MeetingDate);
                Assert.AreEqual(studentAttendanceToUpdate.StartTime, result.StartTime);
                Assert.AreEqual(studentAttendanceToUpdate.EndTime, result.EndTime);
                Assert.AreEqual(studentAttendanceToUpdate.InstructionalMethod, result.InstructionalMethod);
                Assert.AreEqual(studentAttendanceToUpdate.StudentCourseSectionId, result.StudentCourseSectionId);
                Assert.AreEqual(studentAttendanceToUpdate.AttendanceCategoryCode, result.AttendanceCategoryCode);
                Assert.AreEqual(studentAttendanceToUpdate.Comment, result.Comment);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentAttendancesAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    studentAttendanceServiceMock.Setup(x => x.UpdateStudentAttendanceAsync(studentAttendanceToUpdate)).Throws(new PermissionsException());
                    var result = await studentAttendanceController.PutStudentAttendanceAsync(studentAttendanceToUpdate);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentAttendancesAsync_PecordLockException_ReturnsHttpResponseException_Conflict()
            {
                try
                {
                    studentAttendanceServiceMock.Setup(x => x.UpdateStudentAttendanceAsync(studentAttendanceToUpdate)).Throws(new RecordLockException());
                    var result = await studentAttendanceController.PutStudentAttendanceAsync(studentAttendanceToUpdate);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Conflict, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentAttendancesAsync_OtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentAttendanceServiceMock.Setup(x => x.UpdateStudentAttendanceAsync(studentAttendanceToUpdate)).Throws(new ApplicationException());
                    var result = await studentAttendanceController.PutStudentAttendanceAsync(studentAttendanceToUpdate);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentAttendancesAsync_MissingStudentAttendance_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var result = await studentAttendanceController.PutStudentAttendanceAsync(null);

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
