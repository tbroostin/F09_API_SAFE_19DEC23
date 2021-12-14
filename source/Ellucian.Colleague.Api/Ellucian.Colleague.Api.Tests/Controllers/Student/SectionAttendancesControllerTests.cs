// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
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
    public class SectionAttendancesControllerTests
    {
        [TestClass]
        public class SectionAttendancesControllerTests_PutSectionAttendancesAsync
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
            private SectionAttendancesController sectionAttendancesController;
            private SectionAttendance sectionAttendance;
            private SectionAttendanceResponse sectionAttendanceResponse;
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
                sectionAttendance = new SectionAttendance()
                {
                    SectionId = "SectionId",
                    MeetingInstance = new SectionMeetingInstance()
                    {
                        Id = "meetingInstanceId",
                        SectionId = "sectionId",
                        MeetingDate = DateTime.Today,
                        StartTime = startTime,
                        EndTime = endTime,
                        InstructionalMethod = "LEC"
                    },
                    StudentAttendances = new List<StudentSectionAttendance>()
                    {
                        new StudentSectionAttendance()
                        {
                            AttendanceCategoryCode = "P",
                            StudentCourseSectionId = "SCS1",
                            Comment = "Comment1"
                        },
                                                new StudentSectionAttendance()
                        {
                            AttendanceCategoryCode = "P",
                            StudentCourseSectionId = "SCS2",
                        }
                    }

                };
                sectionAttendanceResponse = new SectionAttendanceResponse()
                {
                    SectionId = "SectionId",
                    MeetingInstance = new SectionMeetingInstance()
                    {
                        Id = "meetingInstanceId",
                        SectionId = "sectionId",
                        MeetingDate = DateTime.Today,
                        StartTime = startTime,
                        EndTime = endTime,
                        InstructionalMethod = "LEC"
                    },
                    UpdatedStudentAttendances = new List<StudentAttendance>(),
                    StudentAttendanceErrors = new List<StudentSectionAttendanceError>() { new StudentSectionAttendanceError() {  StudentCourseSectionId = "SCS2", ErrorMessage = "Locked"} },
                    StudentCourseSectionsWithDeletedAttendances = new List<string>() { "12345" }
                };

                sectionAttendancesController = new SectionAttendancesController(studentAttendanceService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionAttendancesController = null;
                studentAttendanceService = null;
            }

            [TestMethod]
            public async Task PutSectiontAttendancesAsync_ReturnsSectionAttendanceResponseDto()
            {
                studentAttendanceServiceMock.Setup(x => x.UpdateSectionAttendanceAsync(sectionAttendance)).Returns(Task.FromResult(sectionAttendanceResponse));
                var result = await sectionAttendancesController.PutSectionAttendancesAsync(sectionAttendance);
                Assert.IsInstanceOfType(result, typeof(SectionAttendanceResponse));
                Assert.AreEqual(sectionAttendance.SectionId, result.SectionId);
                Assert.AreEqual(sectionAttendance.MeetingInstance.Id, result.MeetingInstance.Id);
                Assert.AreEqual(0, result.UpdatedStudentAttendances.Count());
                Assert.AreEqual(1, result.StudentAttendanceErrors.Count());
                Assert.AreEqual(1, result.StudentCourseSectionsWithDeletedAttendances.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentAttendancesAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    studentAttendanceServiceMock.Setup(x => x.UpdateSectionAttendanceAsync(sectionAttendance)).Throws(new PermissionsException());
                    var result = await sectionAttendancesController.PutSectionAttendancesAsync(sectionAttendance);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentAttendancesAsync_OtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentAttendanceServiceMock.Setup(x => x.UpdateSectionAttendanceAsync(sectionAttendance)).Throws(new ApplicationException());
                    var result = await sectionAttendancesController.PutSectionAttendancesAsync(sectionAttendance);

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
                    var result = await sectionAttendancesController.PutSectionAttendancesAsync(null);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }


        }

        [TestClass]
        public class SectionAttendancesControllerTests_PutSectionAttendances2Async
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
            private SectionAttendancesController sectionAttendancesController;
            private SectionAttendance sectionAttendance;
            private SectionAttendanceResponse sectionAttendanceResponse;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                var timespan = new TimeSpan();
                var startTime = new DateTimeOffset(2017, 12, 1, 9, 15, 00, timespan);
                var endTime = new DateTimeOffset(2017, 12, 10, 10, 15, 0, timespan);
                var meetingTime = new DateTime(2017, 12, 5);

                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                studentAttendanceServiceMock = new Mock<IStudentAttendanceService>();
                studentAttendanceService = studentAttendanceServiceMock.Object;
                sectionAttendancesController = new SectionAttendancesController(studentAttendanceService, logger);

                sectionAttendance = new SectionAttendance()
                {
                    SectionId = "SectionId",
                    MeetingInstance = new SectionMeetingInstance()
                    {
                        Id = "",
                        SectionId = "sectionId",
                        MeetingDate = meetingTime,
                        StartTime = startTime,
                        EndTime = endTime,
                        InstructionalMethod = "LEC"
                    },
                    StudentAttendances = new List<StudentSectionAttendance>()
                    {
                        new StudentSectionAttendance()
                        {
                            AttendanceCategoryCode = "P",
                            StudentCourseSectionId = "SCS1",
                            Comment = "Comment1"
                        },
                        new StudentSectionAttendance()
                        {
                            AttendanceCategoryCode = "P",
                            StudentCourseSectionId = "SCS2",
                        }
                    }
                };

                sectionAttendanceResponse = new SectionAttendanceResponse()
                {
                    SectionId = "SectionId",
                    MeetingInstance = new SectionMeetingInstance()
                    {
                        Id = "",
                        SectionId = "sectionId",
                        MeetingDate = meetingTime,
                        StartTime = startTime,
                        EndTime = endTime,
                        InstructionalMethod = "LEC"
                    },
                    UpdatedStudentAttendances = new List<StudentAttendance>(),
                    StudentAttendanceErrors = new List<StudentSectionAttendanceError>() { new StudentSectionAttendanceError() { StudentCourseSectionId = "SCS2", ErrorMessage = "Locked" } },
                    StudentCourseSectionsWithDeletedAttendances = new List<string>() { "12345" }
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionAttendancesController = null;
                studentAttendanceService = null;
            }

            [TestMethod]
            public async Task PutSectionAttendances2Async_ReturnsSectionAttendanceResponseDto()
            {
                studentAttendanceServiceMock.Setup(x => x.UpdateSectionAttendance2Async(sectionAttendance)).Returns(Task.FromResult(sectionAttendanceResponse));
                var result = await sectionAttendancesController.PutSectionAttendances2Async(sectionAttendance);
                Assert.IsInstanceOfType(result, typeof(SectionAttendanceResponse));
                Assert.AreEqual(sectionAttendance.SectionId, result.SectionId);
                Assert.AreEqual(sectionAttendance.MeetingInstance.Id, result.MeetingInstance.Id);
                Assert.AreEqual(sectionAttendance.MeetingInstance.MeetingDate, result.MeetingInstance.MeetingDate);
                Assert.AreEqual(sectionAttendance.MeetingInstance.StartTime, result.MeetingInstance.StartTime);
                Assert.AreEqual(sectionAttendance.MeetingInstance.EndTime, result.MeetingInstance.EndTime);
                Assert.AreEqual(0, result.UpdatedStudentAttendances.Count());
                Assert.AreEqual(1, result.StudentAttendanceErrors.Count());
                Assert.AreEqual(1, result.StudentCourseSectionsWithDeletedAttendances.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentAttendances2Async_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    studentAttendanceServiceMock.Setup(x => x.UpdateSectionAttendance2Async(sectionAttendance)).Throws(new PermissionsException());
                    var result = await sectionAttendancesController.PutSectionAttendances2Async(sectionAttendance);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionAttendances2Async_OtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentAttendanceServiceMock.Setup(x => x.UpdateSectionAttendance2Async(sectionAttendance)).Throws(new ApplicationException());
                    var result = await sectionAttendancesController.PutSectionAttendances2Async(sectionAttendance);

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutSectionAttendances2Async_MissingStudentAttendance_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    var result = await sectionAttendancesController.PutSectionAttendances2Async(null);

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
