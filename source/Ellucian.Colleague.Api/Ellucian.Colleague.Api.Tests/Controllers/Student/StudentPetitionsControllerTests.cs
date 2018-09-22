// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
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
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentPetitionsControllerTests
    {
        [TestClass]
        public class StudentPetitionsControllerTests_Get
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
            private StudentPetitionsController studentPetitionsController;
            private StudentPetition studentPetitionDto;
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

                studentPetitionDto = new StudentPetition();
                studentPetitionDto.Id = "111";
                studentPetitionDto.SectionId = "Section1";
                studentPetitionDto.StudentId = "StudentId";
                studentPetitionDto.StatusCode = "A";
                studentPetitionDto.ReasonCode = "ABC";

                studentPetitionsController = new StudentPetitionsController(sectionPermissionService, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentPetitionsController = null;
                sectionPermissionService = null;
            }

            [TestMethod]
            public async Task GetSectionstudentPetitions_ReturnsStudentPetitionDto()
            {
                sectionPermissionServiceMock.Setup(x => x.GetStudentPetitionAsync(It.IsAny<string>(), It.IsAny<string>(), StudentPetitionType.FacultyConsent)).Returns(Task.FromResult(studentPetitionDto));
                var studentPetitions = await studentPetitionsController.GetAsync("111", "Section1", StudentPetitionType.FacultyConsent);
                Assert.IsTrue(studentPetitions is Dtos.Student.StudentPetition);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionstudentPetitions_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    sectionPermissionServiceMock.Setup(x => x.GetStudentPetitionAsync(It.IsAny<string>(), It.IsAny<string>(), StudentPetitionType.FacultyConsent)).Throws(new PermissionsException());
                    var studentPetitions = await studentPetitionsController.GetAsync("111", "Section1", StudentPetitionType.FacultyConsent);
                    Assert.IsTrue(studentPetitions is IEnumerable<Dtos.Student.StudentPetition>);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionstudentPetitions_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    sectionPermissionServiceMock.Setup(x => x.GetStudentPetitionAsync(It.IsAny<string>(), It.IsAny<string>(), StudentPetitionType.FacultyConsent)).Throws(new KeyNotFoundException());
                    var studentPetitions = await studentPetitionsController.GetAsync("111", "Section1", StudentPetitionType.FacultyConsent);
                    Assert.IsTrue(studentPetitions is IEnumerable<Dtos.Student.StudentPetition>);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionstudentPetitions_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    sectionPermissionServiceMock.Setup(x => x.GetStudentPetitionAsync(It.IsAny<string>(), It.IsAny<string>(), StudentPetitionType.FacultyConsent)).Throws(new ApplicationException());
                    var studentPetitions = await studentPetitionsController.GetAsync("111", "Section1", StudentPetitionType.FacultyConsent);
                    Assert.IsTrue(studentPetitions is IEnumerable<Dtos.Student.StudentPetition>);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }


        }

        [TestClass]
        public class StudentPetitionsControllerTests_Get_StudentPetitions
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

            private IStudentPetitionService studentPermissionService;
            private Mock<IStudentPetitionService> studentPermissionServiceMock;
            private StudentPetitionsController studentPetitionsController;
            private List<StudentPetition> studentPetitionsDto=new List<StudentPetition>();
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentPermissionServiceMock = new Mock<IStudentPetitionService>();
                studentPermissionService = studentPermissionServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                studentPetitionsDto.Add
                    (
                 new StudentPetition()
                 {
                     Id = "111",
                     SectionId = "Section1",
                     StudentId = "StudentId",
                     StatusCode = "A",
                     ReasonCode = "ABC"
                 });

                studentPetitionsDto.Add
                      (
                   new StudentPetition()
                   {
                       Id = "111",
                       SectionId = "Section1",
                       StudentId = "StudentId",
                       StatusCode = "A",
                       ReasonCode = "ABC"
                   });

                studentPetitionsController = new StudentPetitionsController(null, studentPermissionService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentPetitionsController = null;
                studentPermissionService = null;
            }

            [TestMethod]
            public async Task GetSectionstudentPetitions_ReturnsStudentPetitionsDto()
            {
                studentPermissionServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<StudentPetition>>(studentPetitionsDto));
                var studentPetitions = await studentPetitionsController.GetAsync("111");
                Assert.IsTrue(studentPetitions is IEnumerable<Dtos.Student.StudentPetition>);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentPetitions_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    studentPermissionServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Throws(new PermissionsException());
                    var studentPetitions = await studentPetitionsController.GetAsync("111");
                    Assert.IsTrue(studentPetitions is IEnumerable<Dtos.Student.StudentPetition>);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionstudentPetitions_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentPermissionServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Throws(new ApplicationException());
                    var studentPetitions = await studentPetitionsController.GetAsync("111");
                    Assert.IsTrue(studentPetitions is IEnumerable<Dtos.Student.StudentPetition>);
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
