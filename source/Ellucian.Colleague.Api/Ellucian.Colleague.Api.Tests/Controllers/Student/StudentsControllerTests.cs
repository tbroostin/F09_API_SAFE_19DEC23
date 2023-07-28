// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentsControllerTests
    {
        [TestClass]
        public class StudentTests_Get_Eedm_V16
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }

            #endregion

            private IAcademicHistoryService academicHistoryService;
            private IStudentService studentService;
            private IStudentRestrictionService studentRestrictionService;
            private IStudentRepository studentRepo;
            private IStudentProgramRepository studentProgramRepo;
            private IRequirementRepository requirementRepo;
            private IAcademicCreditRepository acadCredRepo;
            private ICourseRepository courseRepo;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;
            private IEmergencyInformationService emergencyInformationService;
            private ILogger logger;
            private ApiSettings apiSettings;
            private string studentId = "0000001";
            private string lastName = "Smith";
            //private string studentProgramId1 = "10";
            private string[] programIds = new string[] { "BA.MATH", "BA.ENGL" };
            private string[] programCodes = new string[] { "PROG1", "PROG2" };
            private string[] catalogCodes = new string[] { "2011", "2012" };
            private string[] academicCreditIds = new string[] { "19000", "38001", "39" };

            //private string personFilter = "1a507924-f207-460a-8c1d-1854ebe80565";
            private string typeFilter = "1a507924-f207-460a-8c1d-1854ebe80561";
            private string cohortsFilter = "1b507924-f207-460a-8c1d-1854ebe80561";
            private string studentGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            private string residencyFilter = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";

            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("person", "");

            private Ellucian.Colleague.Dtos.Students2 studentsDto;

            Mock<IAcademicHistoryService> academicHistoryServiceMock;
            Mock<IStudentService> studentServiceMock;
            Mock<IStudentRestrictionService> studentRestrictionServiceMock;
            Mock<IStudentRepository> studentRepoMock;
            Mock<IStudentProgramRepository> studentProgramRepoMock;
            Mock<IRequirementRepository> requirementRepoMock;
            Mock<IAcademicCreditRepository> acadCredRepoMock;
            Mock<ICourseRepository> courseRepoMock;
            Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;

            private IStudentRepository testStudentRepo;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                // mock needed ctor items
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                studentServiceMock = new Mock<IStudentService>();
                studentRestrictionServiceMock = new Mock<IStudentRestrictionService>();
                studentRepoMock = new Mock<IStudentRepository>();
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                requirementRepoMock = new Mock<IRequirementRepository>();
                acadCredRepoMock = new Mock<IAcademicCreditRepository>();
                courseRepoMock = new Mock<ICourseRepository>();
                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();

                academicHistoryService = academicHistoryServiceMock.Object;
                studentService = studentServiceMock.Object;
                studentRestrictionService = studentRestrictionServiceMock.Object;
                studentRepo = studentRepoMock.Object;
                studentProgramRepo = studentProgramRepoMock.Object;
                requirementRepo = requirementRepoMock.Object;
                acadCredRepo = acadCredRepoMock.Object;
                courseRepo = courseRepoMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                emergencyInformationService = emergencyInformationServiceMock.Object;

                testStudentRepo = new TestStudentRepository();

                // setup students Dto object                
                studentsDto = new Dtos.Students2();
                studentsDto.Id = studentGuid;

                var residency = new StudentResidenciesDtoProperty();
                residency.Residency = new GuidObject2("1a507924-f207-460a-8c1d-1854ebe80567");
                var residencies = new List<StudentResidenciesDtoProperty>() { residency };
                studentsDto.Residencies = residencies;

                var type = new StudentTypesDtoProperty();
                type.Type = new GuidObject2(typeFilter);
                var types = new List<StudentTypesDtoProperty>() { type };
                studentsDto.Types = types;

                var filterResidency = new StudentResidenciesDtoProperty();
                filterResidency.Residency = new GuidObject2(residencyFilter);
                var filterResidencies = new List<StudentResidenciesDtoProperty>() { filterResidency };
                studentsDto.Residencies = residencies;

                var studentDtoList = new List<Students2>() { studentsDto };
                var studentTuple = new Tuple<IEnumerable<Students2>, int>(studentDtoList, 1);

                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Students2>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentTuple);

                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).ReturnsAsync(studentsDto);

                studentServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                // create new students controller
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger,
                    apiSettings)
                {
                    Request = new HttpRequestMessage()
                };
            }


            [TestMethod]
            public async Task StudentsController_GetStudentByGuid2_V16()
            {
                var student = await studentsController.GetStudentsByGuid2Async(studentGuid);
                Assert.AreSame(student.Id, studentGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsByGuid2Async_Exception_V16()
            {
                studentServiceMock.Setup(x => x.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).Throws<Exception>();
                await studentsController.GetStudentsByGuid2Async(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsByGuid2_KeyNotFoundException_V16()
            {
                studentServiceMock.Setup(x => x.GetStudentsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<KeyNotFoundException>();
                await studentsController.GetStudentsByGuid2Async(studentGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentController_GetStudentsByGuid2_PermissionsException_V16()
            {
                studentServiceMock.Setup(x => x.GetStudentsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<PermissionsException>();
                await studentsController.GetStudentsByGuid2Async(studentGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsByGuid2_ArgumentException_V16()
            {
                studentServiceMock.Setup(x => x.GetStudentsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<ArgumentException>();
                await studentsController.GetStudentsByGuid2Async(studentGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsByGuid2_RepositoryException_V16()
            {
                studentServiceMock.Setup(x => x.GetStudentsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<RepositoryException>();
                await studentsController.GetStudentsByGuid2Async(studentGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsByGuid2_IntegrationApiException_V16()
            {
                studentServiceMock.Setup(x => x.GetStudentsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<IntegrationApiException>();
                await studentsController.GetStudentsByGuid2Async(studentGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsByGuid2_Exception_V16()
            {
                studentServiceMock.Setup(x => x.GetStudentsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<Exception>();
                await studentsController.GetStudentsByGuid2Async(studentGuid);
            }

            [TestMethod]
            public async Task StudentsController_GetStudents_NoCache_V16()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var student = await studentsController.GetStudents2Async(new Paging(1, 0), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestCleanup]
            public void Cleanup()
            {
                testStudentRepo = null;
                studentsController = null;
                academicHistoryService = null;
                studentService = null;
                studentRestrictionService = null;
                studentRepo = null;
                studentProgramRepo = null;
                requirementRepo = null;
                acadCredRepo = null;
                courseRepo = null;
                adapterRegistry = null;
                logger = null;
                emergencyInformationService = null;
            }
        }

        [TestClass]
        public class StudentTests_Get_Eedm_V7
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }

            #endregion

            private IAcademicHistoryService academicHistoryService;
            private IStudentService studentService;
            private IStudentRestrictionService studentRestrictionService;
            private IStudentRepository studentRepo;
            private IStudentProgramRepository studentProgramRepo;
            private IRequirementRepository requirementRepo;
            private IAcademicCreditRepository acadCredRepo;
            private ICourseRepository courseRepo;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;
            private IEmergencyInformationService emergencyInformationService;
            private ILogger logger;
            private ApiSettings apiSettings;
            private string studentId = "0000001";
            private string lastName = "Smith";
            //private string studentProgramId1 = "10";
            private string[] programIds = new string[] { "BA.MATH", "BA.ENGL" };
            private string[] programCodes = new string[] { "PROG1", "PROG2" };
            private string[] catalogCodes = new string[] { "2011", "2012" };
            private string[] academicCreditIds = new string[] { "19000", "38001", "39" };

            private string personFilter = "1a507924-f207-460a-8c1d-1854ebe80565";
            private string typeFilter = "1a507924-f207-460a-8c1d-1854ebe80561";
            private string cohortsFilter = "1b507924-f207-460a-8c1d-1854ebe80561";
            private string studentGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            private string residencyFilter = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";

            private Ellucian.Colleague.Dtos.Students studentsDto;

            Mock<IAcademicHistoryService> academicHistoryServiceMock;
            Mock<IStudentService> studentServiceMock;
            Mock<IStudentRestrictionService> studentRestrictionServiceMock;
            Mock<IStudentRepository> studentRepoMock;
            Mock<IStudentProgramRepository> studentProgramRepoMock;
            Mock<IRequirementRepository> requirementRepoMock;
            Mock<IAcademicCreditRepository> acadCredRepoMock;
            Mock<ICourseRepository> courseRepoMock;
            Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;

            private IStudentRepository testStudentRepo;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                // mock needed ctor items
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                studentServiceMock = new Mock<IStudentService>();
                studentRestrictionServiceMock = new Mock<IStudentRestrictionService>();
                studentRepoMock = new Mock<IStudentRepository>();
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                requirementRepoMock = new Mock<IRequirementRepository>();
                acadCredRepoMock = new Mock<IAcademicCreditRepository>();
                courseRepoMock = new Mock<ICourseRepository>();
                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();

                academicHistoryService = academicHistoryServiceMock.Object;
                studentService = studentServiceMock.Object;
                studentRestrictionService = studentRestrictionServiceMock.Object;
                studentRepo = studentRepoMock.Object;
                studentProgramRepo = studentProgramRepoMock.Object;
                requirementRepo = requirementRepoMock.Object;
                acadCredRepo = acadCredRepoMock.Object;
                courseRepo = courseRepoMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                emergencyInformationService = emergencyInformationServiceMock.Object;

                testStudentRepo = new TestStudentRepository();

                // setup students Dto object                
                studentsDto = new Dtos.Students();
                studentsDto.Id = studentGuid;
                studentsDto.Person = new GuidObject2(personFilter);
                studentsDto.Residency = new GuidObject2("1a507924-f207-460a-8c1d-1854ebe80567");
                studentsDto.Tags = new List<GuidObject2>() { new GuidObject2("1a507924-f207-460a-8c1d-1854ebe80562") };
                studentsDto.Type = new GuidObject2(typeFilter);
                studentsDto.Cohorts = new List<GuidObject2>() { new GuidObject2(cohortsFilter) };
                studentsDto.Residency = new GuidObject2(residencyFilter);

                var studentDtoList = new List<Students>() { studentsDto };
                var studentTuple = new Tuple<IEnumerable<Students>, int>(studentDtoList, 1);

                studentServiceMock.Setup(
                    s =>
                        s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personFilter, typeFilter,
                            cohortsFilter, residencyFilter)).ReturnsAsync(studentTuple);

                studentServiceMock.Setup(
                    s =>
                        s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), string.Empty, string.Empty, string.Empty, string.Empty)).ReturnsAsync(studentTuple);

                studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).ReturnsAsync(studentsDto);

                // create new students controller
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger,
                    apiSettings)
                {
                    Request = new HttpRequestMessage()
                };
            }


            [TestMethod]
            public async Task GetStudentByGuid_V7()
            {
                var student = await studentsController.GetStudentsByGuidAsync(studentGuid);
                Assert.AreSame(student.Id, studentGuid);
            }

            [TestMethod]
            public async Task GetStudentsByFilter_NoCache_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var student = await studentsController.GetStudentsAsync(new Paging(1, 0), personFilter, typeFilter, cohortsFilter, residencyFilter);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_NoPaging_NoCache_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var student = await studentsController.GetStudentsAsync(null, personFilter, typeFilter, cohortsFilter, residencyFilter);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudentsByFilter_Cache_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var student = await studentsController.GetStudentsAsync(new Paging(1, 0), personFilter, typeFilter, cohortsFilter, residencyFilter);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_NoPaging_Cache_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var student = await studentsController.GetStudentsAsync(null, personFilter, typeFilter, cohortsFilter, residencyFilter);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudents_NoCache_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var student = await studentsController.GetStudentsAsync(new Paging(1, 0));
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudents_NoPaging_NoCache_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var student = await studentsController.GetStudentsAsync(null);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudents_Cache_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var student = await studentsController.GetStudentsAsync(new Paging(1, 0));
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudents_NoPaging_Cache_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                var student = await studentsController.GetStudentsAsync(null);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_PermissionsException_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
    s =>
        s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personFilter, typeFilter,
            cohortsFilter, residencyFilter)).Throws(new PermissionsException());
                try
                {
                    await studentsController.GetStudentsAsync(null, personFilter, typeFilter, cohortsFilter, residencyFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_ArgumentException_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                    s =>
                        s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personFilter, typeFilter,
                            cohortsFilter, residencyFilter)).Throws(new ArgumentException());
                try
                {
                    await studentsController.GetStudentsAsync(null, personFilter, typeFilter, cohortsFilter, residencyFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_RepositoryException_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                    s =>
                        s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personFilter, typeFilter,
                            cohortsFilter, residencyFilter)).Throws(new RepositoryException());
                try
                {
                    await studentsController.GetStudentsAsync(null, personFilter, typeFilter, cohortsFilter, residencyFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_InegrationApiException_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                    s =>
                        s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personFilter, typeFilter,
                            cohortsFilter, residencyFilter)).Throws(new IntegrationApiException());
                try
                {
                    await studentsController.GetStudentsAsync(null, personFilter, typeFilter, cohortsFilter, residencyFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_Exception_V7()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                    s =>
                        s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personFilter, typeFilter,
                            cohortsFilter, residencyFilter)).Throws(new Exception());
                try
                {
                    await studentsController.GetStudentsAsync(null, personFilter, typeFilter, cohortsFilter, residencyFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentByGuid_NoGuidException_V7()
            {
                var student = await studentsController.GetStudentsByGuidAsync(null);
            }

            [TestMethod]
            public async Task GetStudentByGuid_Exception_V7()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).Throws(new Exception());

                try
                {
                    await studentsController.GetStudentsByGuidAsync(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_PermissionsException_V7()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).Throws(new PermissionsException());

                try
                {
                    await studentsController.GetStudentsByGuidAsync(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_KeyNotFoundException_V7()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).Throws(new KeyNotFoundException());

                try
                {
                    await studentsController.GetStudentsByGuidAsync(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_ArgumentNullException_V7()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).Throws(new ArgumentNullException());

                try
                {
                    await studentsController.GetStudentsByGuidAsync(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_RepositoryException_V7()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).Throws(new RepositoryException());

                try
                {
                    await studentsController.GetStudentsByGuidAsync(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_IntegrationApiException_V7()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).Throws(new IntegrationApiException());

                try
                {
                    await studentsController.GetStudentsByGuidAsync(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            //GET v7
            //Successful
            //GetStudentsAsync
            [TestMethod]
            public async Task StudentsController_GetStudentsAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudentsAsync" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);
                studentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentInformation);

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var studentDtoList = new List<Students>() { studentsDto };
                var studentTuple = new Tuple<IEnumerable<Students>, int>(studentDtoList, 1);

                studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentServiceMock.Setup(s => s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personFilter, typeFilter, cohortsFilter, residencyFilter)).ReturnsAsync(studentTuple);
                var student = await studentsController.GetStudentsAsync(new Paging(1, 0));

                Object filterObject;
                studentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentInformation));


            }


            //GET v7
            //Exception
            //GetStudentsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudentsAsync" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentServiceMock.Setup(s => s.GetStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), personFilter, typeFilter, cohortsFilter, residencyFilter)).Throws(new PermissionsException());
                    studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view students."));
                    await studentsController.GetStudentsAsync(null, personFilter, typeFilter, cohortsFilter, residencyFilter);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET by id v7
            //Successful
            //GetStudentsByGuidAsync
            [TestMethod]
            public async Task StudentsController_GetStudentsByGuidAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudentsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);
                studentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentInformation);

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var studentDtoList = new List<Students>() { studentsDto };
                var studentTuple = new Tuple<IEnumerable<Students>, int>(studentDtoList, 1);

                studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).ReturnsAsync(studentsDto);
                var student = await studentsController.GetStudentsByGuidAsync(studentGuid);

                Object filterObject;
                studentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentInformation));


            }


            //GET by id v7
            //Exception
            //GetStudentsByGuidAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudentsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentServiceMock.Setup(s => s.GetStudentsByGuidAsync(studentGuid, It.IsAny<bool>())).Throws(new PermissionsException());
                    await studentsController.GetStudentsByGuidAsync(studentGuid);
                    studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view students."));
                    await studentsController.GetStudentsByGuidAsync(studentGuid);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }



            [TestCleanup]
            public void Cleanup()
            {
                testStudentRepo = null;
                studentsController = null;
                academicHistoryService = null;
                studentService = null;
                studentRestrictionService = null;
                studentRepo = null;
                studentProgramRepo = null;
                requirementRepo = null;
                acadCredRepo = null;
                courseRepo = null;
                adapterRegistry = null;
                logger = null;
                emergencyInformationService = null;
            }


        }

        [TestClass]
        public class StudentTests_GetStudents2
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }

            #endregion

            private IAcademicHistoryService academicHistoryService;
            private IStudentService studentService;
            private IStudentRestrictionService studentRestrictionService;
            private IStudentRepository studentRepo;
            private IStudentProgramRepository studentProgramRepo;
            private IRequirementRepository requirementRepo;
            private IAcademicCreditRepository acadCredRepo;
            private ICourseRepository courseRepo;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;
            private IEmergencyInformationService emergencyInformationService;
            private ILogger logger;
            private ApiSettings apiSettings;
            private string studentId = "0000001";
            private string lastName = "Smith";
            //private string studentProgramId1 = "10";
            private string[] programIds = new string[] { "BA.MATH", "BA.ENGL" };
            private string[] programCodes = new string[] { "PROG1", "PROG2" };
            private string[] catalogCodes = new string[] { "2011", "2012" };
            private string[] academicCreditIds = new string[] { "19000", "38001", "39" };

            private string personFilter = "1a507924-f207-460a-8c1d-1854ebe80565";
            private string typeFilter = "1a507924-f207-460a-8c1d-1854ebe80561";
            private string cohortsFilter = "1b507924-f207-460a-8c1d-1854ebe80561";
            private string studentGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            private string residencyFilter = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";

            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter
                = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter personCriteriaFilter
                = new Web.Http.Models.QueryStringFilter("personFilter", "");

            private Ellucian.Colleague.Dtos.Students2 studentsDto;

            Mock<IAcademicHistoryService> academicHistoryServiceMock;
            Mock<IStudentService> studentServiceMock;
            Mock<IStudentRestrictionService> studentRestrictionServiceMock;
            Mock<IStudentRepository> studentRepoMock;
            Mock<IStudentProgramRepository> studentProgramRepoMock;
            Mock<IRequirementRepository> requirementRepoMock;
            Mock<IAcademicCreditRepository> acadCredRepoMock;
            Mock<ICourseRepository> courseRepoMock;
            Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;

            private IStudentRepository testStudentRepo;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                // mock needed ctor items
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                studentServiceMock = new Mock<IStudentService>();
                studentRestrictionServiceMock = new Mock<IStudentRestrictionService>();
                studentRepoMock = new Mock<IStudentRepository>();
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                requirementRepoMock = new Mock<IRequirementRepository>();
                acadCredRepoMock = new Mock<IAcademicCreditRepository>();
                courseRepoMock = new Mock<ICourseRepository>();
                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();

                academicHistoryService = academicHistoryServiceMock.Object;
                studentService = studentServiceMock.Object;
                studentRestrictionService = studentRestrictionServiceMock.Object;
                studentRepo = studentRepoMock.Object;
                studentProgramRepo = studentProgramRepoMock.Object;
                requirementRepo = requirementRepoMock.Object;
                acadCredRepo = acadCredRepoMock.Object;
                courseRepo = courseRepoMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                emergencyInformationService = emergencyInformationServiceMock.Object;

                testStudentRepo = new TestStudentRepository();

                // setup students Dto object                
                studentsDto = new Dtos.Students2();
                studentsDto.Id = studentGuid;
                studentsDto.Person = new GuidObject2(personFilter);
                studentsDto.Residencies = new List<StudentResidenciesDtoProperty>()
                {  new StudentResidenciesDtoProperty()
                    { Residency = new GuidObject2("1a507924-f207-460a-8c1d-1854ebe80567") } };
                studentsDto.Types = new List<StudentTypesDtoProperty>()
                {  new StudentTypesDtoProperty() { Type  =  new GuidObject2(typeFilter) } };
                studentsDto.LevelClassifications = new List<StudentLevelClassificationsDtoProperty>()
                {  new StudentLevelClassificationsDtoProperty() { Level = new GuidObject2("2a507924-f207-460a-8c1d-1854ebe80561") }};

                var studentDtoList = new List<Students2>() { studentsDto };
                var studentTuple = new Tuple<IEnumerable<Students2>, int>(studentDtoList, 1);

                // GetStudents2Async(int offset, int limit, Dtos.Students2 criteriaFilter, string personFilter, bool bypassCache = false);
                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Students2>(), personFilter, It.IsAny<bool>()))
                        .ReturnsAsync(studentTuple);

                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), null, string.Empty, It.IsAny<bool>()))
                        .ReturnsAsync(studentTuple);

                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).ReturnsAsync(studentsDto);

                // create new students controller
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger,
                    apiSettings)
                {
                    Request = new HttpRequestMessage()
                };
            }

            [TestMethod]
            public async Task GetStudentByGuid_Students2()
            {
                var student = await studentsController.GetStudentsByGuid2Async(studentGuid);
                Assert.AreSame(student.Id, studentGuid);
            }


            [TestMethod]
            public async Task GetStudents_NoCache_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                studentsController.Request.Properties.Add(string.Format("FilterObject{0}", "criteria"), new Dtos.Students2 { Person = new GuidObject2("guid1") });
                studentsController.Request.Properties.Add(string.Format("FilterObject{0}", "personFilter"), new Dtos.Filters.PersonFilterFilter2 { personFilter = new GuidObject2(personFilter) });

                var student = await studentsController.GetStudents2Async(new Paging(1, 0),
                    criteriaFilter, personCriteriaFilter);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudents_NoPaging_NoCache_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                studentsController.Request.Properties.Add(
                     string.Format("FilterObject{0}", "criteria"),
                     new Dtos.Students2 { Person = new GuidObject2("guid1") });
                studentsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", "personFilter"),
                      new Dtos.Filters.PersonFilterFilter2 { personFilter = new GuidObject2(personFilter) });

                var student = await studentsController.GetStudents2Async(null,
                    criteriaFilter, personCriteriaFilter);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudents_Cache_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                studentsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", "criteria"),
                      new Dtos.Students2 { Person = new GuidObject2("guid1") });
                studentsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", "personFilter"),
                      new Dtos.Filters.PersonFilterFilter2 { personFilter = new GuidObject2(personFilter) });
                var student = await studentsController.GetStudents2Async(new Paging(1, 0),
                    criteriaFilter, personCriteriaFilter);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudents_NoPaging_Cache_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                studentsController.Request.Properties.Add(
                     string.Format("FilterObject{0}", "criteria"),
                     new Dtos.Students2 { Person = new GuidObject2("guid1") });
                studentsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", "personFilter"),
                      new Dtos.Filters.PersonFilterFilter2 { personFilter = new GuidObject2(personFilter) });

                var student = await studentsController.GetStudents2Async(null,
                    criteriaFilter, personCriteriaFilter);
                Assert.IsTrue(student is IHttpActionResult);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_PermissionsException_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                s =>
                 s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Dtos.Students2>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new PermissionsException());
                try
                {
                    await studentsController.GetStudents2Async(null, criteriaFilter, personCriteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_ArgumentException_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Students2>(), It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws(new ArgumentException());
                try
                {
                    await studentsController.GetStudents2Async(null, criteriaFilter, personCriteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_RepositoryException_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Students2>(), It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws(new RepositoryException());
                try
                {
                    await studentsController.GetStudents2Async(null, criteriaFilter, personCriteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_InegrationApiException_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Students2>(), It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws(new IntegrationApiException());
                try
                {
                    await studentsController.GetStudents2Async(null, criteriaFilter, personCriteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentsByFitler_Exception_Students2()
            {
                studentsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Students2>(), It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws(new Exception());
                try
                {
                    await studentsController.GetStudents2Async(null, criteriaFilter, personCriteriaFilter);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentByGuid_NoGuidException_Students2()
            {
                var student = await studentsController.GetStudentsByGuid2Async(null);
            }

            [TestMethod]
            public async Task GetStudentByGuid_Exception_Students2()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).Throws(new Exception());

                try
                {
                    await studentsController.GetStudentsByGuid2Async(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_PermissionsException_Students2()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).Throws(new PermissionsException());

                try
                {
                    await studentsController.GetStudentsByGuid2Async(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_KeyNotFoundException_Students2()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).Throws(new KeyNotFoundException());

                try
                {
                    await studentsController.GetStudentsByGuid2Async(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.NotFound, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_ArgumentNullException_Students2()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).Throws(new ArgumentNullException());

                try
                {
                    await studentsController.GetStudentsByGuid2Async(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_RepositoryException_Students2()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).Throws(new RepositoryException());

                try
                {
                    await studentsController.GetStudentsByGuid2Async(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }

            [TestMethod]
            public async Task GetStudentByGuid_IntegrationApiException_Students2()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).Throws(new IntegrationApiException());

                try
                {
                    await studentsController.GetStudentsByGuid2Async(studentGuid);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
            }



            //GET v16.0.0
            //Successful
            //GetStudents2Async
            [TestMethod]
            public async Task StudentsController_GetStudents2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudents2Async" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);
                studentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentInformation);

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var studentDtoList = new List<Students2>() { studentsDto };
                var studentTuple = new Tuple<IEnumerable<Students2>, int>(studentDtoList, 1);
                studentsController.Request.Properties.Add(string.Format("FilterObject{0}", "criteria"), new Dtos.Students2 { Person = new GuidObject2("guid1") });
                studentsController.Request.Properties.Add(string.Format("FilterObject{0}", "personFilter"), new Dtos.Filters.PersonFilterFilter2 { personFilter = new GuidObject2(personFilter) });

                studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentServiceMock.Setup(s => s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Students2>(), personFilter, It.IsAny<bool>())).ReturnsAsync(studentTuple);
                var student = await studentsController.GetStudents2Async(new Paging(1, 0), criteriaFilter, personCriteriaFilter);

                Object filterObject;
                studentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentInformation));


            }

            //GET v16.0.0
            //Exception
            //GetStudents2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudents2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudents2Async" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentServiceMock.Setup(s => s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Students2>(), It.IsAny<string>(), It.IsAny<bool>())).Throws(new PermissionsException());
                    studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view students."));
                    await studentsController.GetStudents2Async(null, criteriaFilter, personCriteriaFilter);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET by id v16.0.0
            //Successful
            //GetStudentsByGuid2Async
            [TestMethod]
            public async Task StudentsController_GetStudentsByGuid2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudentsByGuid2Async" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);
                studentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentInformation);

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var studentDtoList = new List<Students2>() { studentsDto };
                var studentTuple = new Tuple<IEnumerable<Students2>, int>(studentDtoList, 1);

                studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).ReturnsAsync(studentsDto);
                var student = await studentsController.GetStudentsByGuid2Async(studentGuid);

                Object filterObject;
                studentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentInformation));


            }


            //GET by id v16.0.0
            //Exception
            //GetStudentsByGuid2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetStudentsByGuid2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudentsByGuid2Async" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).Throws(new PermissionsException());
                    studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view students."));
                    await studentsController.GetStudentsByGuid2Async(studentGuid);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            [TestCleanup]
            public void Cleanup()
            {
                testStudentRepo = null;
                studentsController = null;
                academicHistoryService = null;
                studentService = null;
                studentRestrictionService = null;
                studentRepo = null;
                studentProgramRepo = null;
                requirementRepo = null;
                acadCredRepo = null;
                courseRepo = null;
                adapterRegistry = null;
                logger = null;
                emergencyInformationService = null;
            }
        }

        [TestClass]
        public class BaseStudentTests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }

            #endregion

            private Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
            private Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
            private IAcademicHistoryService academicHistoryService;
            private IStudentService studentService;
            private IStudentRestrictionService studentRestrictionService;
            private IStudentRepository studentRepo;
            private IStudentProgramRepository studentProgramRepo;
            private IRequirementRepository requirementRepo;
            private IAcademicCreditRepository acadCredRepo;
            private ICourseRepository courseRepo;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;
            private IEmergencyInformationService emergencyInformationService;
            private ILogger logger;
            private ApiSettings apiSettings;
            private string studentId = "0000001";
            private string lastName = "Smith";
            //private string studentProgramId1 = "10";
            private string[] programIds = new string[] { "BA.MATH", "BA.ENGL" };
            private string[] programCodes = new string[] { "PROG1", "PROG2" };
            private string[] catalogCodes = new string[] { "2011", "2012" };
            private string[] academicCreditIds = new string[] { "19000", "38001", "39" };
            private IEnumerable<Dtos.Student.SectionRegistration> sectionRegistrations;

            private IStudentRepository testStudentRepo;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                // mock needed ctor items
                Mock<IAcademicHistoryService> academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                Mock<IStudentRestrictionService> studentRestrictionServiceMock = new Mock<IStudentRestrictionService>();
                Mock<IStudentRepository> studentRepoMock = new Mock<IStudentRepository>();
                Mock<IRequirementRepository> requirementRepoMock = new Mock<IRequirementRepository>();
                Mock<IAcademicCreditRepository> acadCredRepoMock = new Mock<IAcademicCreditRepository>();
                Mock<ICourseRepository> courseRepoMock = new Mock<ICourseRepository>();
                Mock<IEmergencyInformationService> emergencyInformationServiceMock =
                    new Mock<IEmergencyInformationService>();

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                academicHistoryService = academicHistoryServiceMock.Object;

                studentService = studentServiceMock.Object;
                studentRestrictionService = studentRestrictionServiceMock.Object;
                studentRepo = studentRepoMock.Object;
                studentProgramRepo = studentProgramRepoMock.Object;
                requirementRepo = requirementRepoMock.Object;
                acadCredRepo = acadCredRepoMock.Object;
                courseRepo = courseRepoMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;


                testStudentRepo = new TestStudentRepository();

                // mock student repo call
                var student = new Domain.Student.Entities.Student(studentId, lastName, 2,
                    new List<string> { programIds[0], programIds[1] },
                    new List<string> { academicCreditIds[0], academicCreditIds[1] });
                studentRepoMock.Setup(repo => repo.GetAsync(studentId)).Returns(Task.FromResult(student));

                // mock studentprogram repo call
                List<Ellucian.Colleague.Domain.Student.Entities.StudentProgram> stuPrograms =
                    new List<Ellucian.Colleague.Domain.Student.Entities.StudentProgram>();
                stuPrograms.Add(new Ellucian.Colleague.Domain.Student.Entities.StudentProgram(studentId, programCodes[0],
                    catalogCodes[0]));
                stuPrograms.Add(new Ellucian.Colleague.Domain.Student.Entities.StudentProgram(studentId, programCodes[1],
                    catalogCodes[1]));
                studentProgramRepoMock.Setup(repo => repo.GetAsync(studentId))
                    .ReturnsAsync(stuPrograms.AsEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentProgram>());
                studentProgramRepoMock.Setup(repo => repo.GetAsync(studentId, programIds[0]))
                    .Returns(Task.FromResult(stuPrograms[0]));
                studentProgramRepoMock.Setup(repo => repo.GetAsync(studentId, programIds[1]))
                    .Returns(Task.FromResult(stuPrograms[1]));

                // mock a valid Academic History Service response
                AcademicHistory academicHistory = new AcademicHistory();
                AcademicHistory3 academicHistory3 = new AcademicHistory3();
                academicHistoryServiceMock.Setup(repo => repo.GetAcademicHistoryAsync(studentId, false, true, null))
                    .Returns(Task.FromResult(academicHistory));
                academicHistoryServiceMock.Setup(repo => repo.GetAcademicHistory3Async(studentId, false, true, null))
                    .Returns(Task.FromResult(academicHistory3));

                // mock a valid Student Restriction Service response
                IEnumerable<PersonRestriction> studentRestrictions = new List<PersonRestriction>();
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictionsAsync(studentId, true))
                    .Returns(Task.FromResult(studentRestrictions));
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictionsAsync("0000002", true))
                    .Throws(new PermissionsException());
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictionsAsync("0000002", false))
                    .Throws(new PermissionsException());
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictions2Async(studentId, true))
                    .Returns(Task.FromResult(studentRestrictions));
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictions2Async("0000002", true))
                    .Throws(new PermissionsException());
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictions2Async("0000002", false))
                    .Throws(new PermissionsException());

                IEnumerable<Domain.Student.Entities.TranscriptRestriction> emptyRestrictions =
                    new List<Domain.Student.Entities.TranscriptRestriction>();
                IEnumerable<Domain.Student.Entities.TranscriptRestriction> oneRestriction =
                    new List<Domain.Student.Entities.TranscriptRestriction>()
                    {
                        new Domain.Student.Entities.TranscriptRestriction() {Code = "TEST", Description = "TEST"}
                    };
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictionsAsync(studentId))
                    .Returns(Task.FromResult(emptyRestrictions));
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictionsAsync("00000002"))
                    .Returns(Task.FromResult(oneRestriction));
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictionsAsync("00000003"))
                    .Throws(new HttpResponseException(System.Net.HttpStatusCode.NotFound));

                // mock a valid Student Service Trascript Access responses
                var oneRestrictionDto = new List<TranscriptRestriction>()
                {
                    new TranscriptRestriction() {Code = "TEST", Description = "TEST"}
                };

                var enforcedEmptyTranscriptAccess = new TranscriptAccess();
                enforcedEmptyTranscriptAccess.EnforceTranscriptRestriction = true;
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000001"))
                    .ReturnsAsync(enforcedEmptyTranscriptAccess);

                var unenforcedEmptyTranscriptAccess = new TranscriptAccess();
                unenforcedEmptyTranscriptAccess.EnforceTranscriptRestriction = false;
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000002"))
                    .ReturnsAsync(unenforcedEmptyTranscriptAccess);

                var enforcedOneTranscriptAccess = new TranscriptAccess();
                enforcedOneTranscriptAccess.EnforceTranscriptRestriction = true;
                enforcedOneTranscriptAccess.TranscriptRestrictions = oneRestrictionDto;
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000003"))
                    .ReturnsAsync(enforcedOneTranscriptAccess);

                var unforcedOneTranscriptAccess = new TranscriptAccess();
                unforcedOneTranscriptAccess.EnforceTranscriptRestriction = false;
                unforcedOneTranscriptAccess.TranscriptRestrictions = oneRestrictionDto;
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000004"))
                    .ReturnsAsync(unforcedOneTranscriptAccess);

                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000005"))
                    .Throws(new HttpResponseException(System.Net.HttpStatusCode.NotFound));


                // mock a person emergency information service call
                var personEmergencyInformation = new Ellucian.Colleague.Dtos.Base.EmergencyInformation()
                {
                    PersonId = "1234567",
                    EmergencyContacts = new List<EmergencyContact>()
                };
                emergencyInformationServiceMock.Setup(svc => svc.GetEmergencyInformationAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(personEmergencyInformation));


                // mock a valid student search response
                var studentDto1 = new Dtos.Student.Student() { Id = "00000001", LastName = "Dog", FirstName = "Able" };
                var studentDto2 = new Dtos.Student.Student() { Id = "00000002", LastName = "Dog", FirstName = "Baker" };
                var studentDto3 = new Dtos.Student.Student() { Id = "00000003", LastName = "Dog", FirstName = "Charlie" };

                var justOne = new List<Dtos.Student.Student>() { studentDto1 };
                var justTwo = new List<Dtos.Student.Student>() { studentDto2 };
                var allThree = new List<Dtos.Student.Student>() { studentDto1, studentDto2, studentDto3 };


                studentServiceMock.Setup(svc => svc.SearchAsync("Dog", DateTime.Parse("3/3/33"), null, null, null, null))
                    .Returns(Task.FromResult(allThree.AsEnumerable()));
                studentServiceMock.Setup(
                    svc => svc.SearchAsync("Dog", DateTime.Parse("3/3/33"), "Baker", null, null, null))
                    .Returns(Task.FromResult(justTwo.AsEnumerable()));
                studentServiceMock.Setup(
                    svc => svc.SearchAsync("Dog", DateTime.Parse("3/3/33"), "Able", null, null, null))
                    .Returns(Task.FromResult(justOne.AsEnumerable()));
                studentServiceMock.Setup(
                    svc => svc.SearchAsync("Dog", DateTime.Parse("3/3/33"), "Baker", null, null, null))
                    .Returns(Task.FromResult(justTwo.AsEnumerable()));
                studentServiceMock.Setup(svc => svc.SearchAsync("Smith", null, null, null, null, null))
                    .Throws(new HttpResponseException(System.Net.HttpStatusCode.NotFound));

                // mock valid transcript order response
                string tr = "<xml><something>mock response</something></xml>";
                studentServiceMock.Setup(svc => svc.OrderTranscriptAsync(It.IsAny<TranscriptRequest>()))
                    .Returns(Task.FromResult(tr));

                // invalid transcript order response
                studentServiceMock.Setup(svc => svc.OrderTranscriptAsync(null)).Throws(new Exception());

                // Set up sectionRegistrations
                sectionRegistrations = new List<Dtos.Student.SectionRegistration>()
                {
                    new Dtos.Student.SectionRegistration()
                    {
                        Action = Dtos.Student.RegistrationAction.Add,
                        Credits = null,
                        SectionId = "1111"
                    }
                };

                // mock DTO adapters
                var stuAdapter =
                    new AutoMapperAdapter
                        <Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(
                        adapterRegistry, logger);
                var stuProgAdapter =
                    new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram>(
                        adapterRegistry, logger);

                var stuProg2Adapter =
                    new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram2>(
                        adapterRegistry, logger);

                var AcademicHistoryDto3Adapter =
                    new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, AcademicHistory3>(
                        adapterRegistry, logger);

                adapterRegistryMock.Setup(
                    reg =>
                        reg
                            .GetAdapter
                            <Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student
                                >()).Returns(stuAdapter);

                adapterRegistryMock.Setup(
                    reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram>())
                    .Returns(stuProgAdapter);

                adapterRegistryMock.Setup(
                    reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram2>())
                    .Returns(stuProg2Adapter);

                // mock student service call
                var student1 = new Domain.Student.Entities.Student(studentId, lastName, 2,
                    new List<string> { programIds[0], programIds[1] },
                    new List<string> { academicCreditIds[0], academicCreditIds[1] });
                var studentDto = stuAdapter.MapToType(student1);
                studentServiceMock.Setup(svc => svc.GetAsync(studentId)).Returns(Task.FromResult(new PrivacyWrapper<Dtos.Student.Student>(studentDto, false)));

                // mock controller
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger,
                    apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentsController = null;
                adapterRegistry = null;
                studentRepo = null;
                studentProgramRepo = null;
                acadCredRepo = null;
                courseRepo = null;
            }

            [TestMethod]
            public async Task Get_StudentById()
            {
                Ellucian.Colleague.Dtos.Student.Student studentDTO = await studentsController.GetStudentAsync(studentId);
                Assert.AreEqual(studentId, studentDTO.Id);
                Assert.IsTrue(studentDTO.DegreePlanId.HasValue);
                Assert.AreEqual(2, studentDTO.DegreePlanId);
                // MBS changed to 2 so I could use the test repo - there is no DP1.
                Assert.AreEqual(2, studentDTO.ProgramIds.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentById_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    studentServiceMock.Setup(x => x.GetAsync(studentId)).Throws(new PermissionsException());
                    await studentsController.GetStudentAsync(studentId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentById_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    studentServiceMock.Setup(x => x.GetAsync(studentId)).Throws(new KeyNotFoundException());
                    await studentsController.GetStudentAsync(studentId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentById_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    studentServiceMock.Setup(x => x.GetAsync(studentId)).Throws(new ColleagueSessionExpiredException("session expired"));
                    await studentsController.GetStudentAsync(studentId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentById_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentServiceMock.Setup(x => x.GetAsync(studentId)).Throws(new ArgumentException());
                    await studentsController.GetStudentAsync(studentId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task Get_StudentPrograms_All()
            {
                List<StudentProgram> studentProgramDTO =
                    (List<StudentProgram>)(await studentsController.GetStudentProgramsAsync(studentId));
                Assert.AreEqual(2, studentProgramDTO.Count);
                Assert.AreEqual(studentId, studentProgramDTO[0].StudentId);
                Assert.AreEqual(studentId, studentProgramDTO[1].StudentId);
                Assert.AreEqual(programCodes[0], studentProgramDTO[0].ProgramCode);
                Assert.AreEqual(programCodes[1], studentProgramDTO[1].ProgramCode);
                Assert.AreEqual(catalogCodes[0], studentProgramDTO[0].CatalogCode);
                Assert.AreEqual(catalogCodes[1], studentProgramDTO[1].CatalogCode);
            }

            [TestMethod]
            public async Task GetStudentPrograms2Async_ReturnsValidData()
            {
                List<StudentProgram2> studentProgramDTO = (List<StudentProgram2>)(await studentsController.GetStudentPrograms2Async(studentId));
                Assert.AreEqual(2, studentProgramDTO.Count);
                Assert.AreEqual(studentId, studentProgramDTO[0].StudentId);
                Assert.AreEqual(studentId, studentProgramDTO[1].StudentId);
                Assert.AreEqual(programCodes[0], studentProgramDTO[0].ProgramCode);
                Assert.AreEqual(programCodes[1], studentProgramDTO[1].ProgramCode);
                Assert.AreEqual(catalogCodes[0], studentProgramDTO[0].CatalogCode);
                Assert.AreEqual(catalogCodes[1], studentProgramDTO[1].CatalogCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentPrograms2Async_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    studentProgramRepoMock.Setup(x => x.GetAsync(studentId)).Throws(new PermissionsException());
                    await studentsController.GetStudentPrograms2Async(studentId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentPrograms2Async_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    studentProgramRepoMock.Setup(x => x.GetAsync(studentId)).Throws(new ColleagueSessionExpiredException("session expired"));
                    await studentsController.GetStudentPrograms2Async(studentId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentPrograms2Async_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentProgramRepoMock.Setup(x => x.GetAsync(studentId)).Throws(new ArgumentException());
                    await studentsController.GetStudentPrograms2Async(studentId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }




            [TestMethod]
            public async Task GetTranscriptRestrictions_Empty()
            {
                IEnumerable<Dtos.Student.TranscriptRestriction> restrictions =
                    await studentsController.GetTranscriptRestrictionsAsync(studentId);
                Assert.AreEqual(0, restrictions.Count());
            }

            [TestMethod]
            public async Task GetTranscriptRestrictions_One()
            {
                IEnumerable<Dtos.Student.TranscriptRestriction> restrictions =
                    await studentsController.GetTranscriptRestrictionsAsync("00000002");
                Assert.AreEqual(1, restrictions.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetTranscriptRestrictions_MissingStudent()
            {
                IEnumerable<Dtos.Student.TranscriptRestriction> restrictions =
                    await studentsController.GetTranscriptRestrictionsAsync("00000003");
            }

            [TestMethod]
            public async Task GetTranscriptRestrictions2_EnforceEmpty()
            {
                var transcriptAccess = await studentsController.GetTranscriptRestrictions2Async("00000001");
                Assert.AreEqual(0, transcriptAccess.TranscriptRestrictions.Count());
                Assert.IsTrue(transcriptAccess.EnforceTranscriptRestriction);
            }

            [TestMethod]
            public async Task GetTranscriptRestrictions2_UnenforceEmpty()
            {
                var transcriptAccess = await studentsController.GetTranscriptRestrictions2Async("00000002");
                Assert.AreEqual(0, transcriptAccess.TranscriptRestrictions.Count());
                Assert.IsFalse(transcriptAccess.EnforceTranscriptRestriction);
            }

            [TestMethod]
            public async Task GetTranscriptRestrictions2_EnforceWithRestriction()
            {
                var transcriptAccess = await studentsController.GetTranscriptRestrictions2Async("00000003");
                Assert.AreEqual(1, transcriptAccess.TranscriptRestrictions.Count());
                Assert.IsTrue(transcriptAccess.EnforceTranscriptRestriction);
            }

            [TestMethod]
            public async Task GetTranscriptRestrictions2_UnenforceWithRestriction()
            {
                var transcriptAccess = await studentsController.GetTranscriptRestrictions2Async("00000004");
                Assert.AreEqual(1, transcriptAccess.TranscriptRestrictions.Count());
                Assert.IsFalse(transcriptAccess.EnforceTranscriptRestriction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetTranscriptRestrictions2_MissingStudent()
            {
                var transcriptAccess = await studentsController.GetTranscriptRestrictions2Async("00000005");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions_NullStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions_EmptyStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions_UnauthorizedUser()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync("0000002");
            }

            [TestMethod]
            public async Task GetStudentRestrictions_ValidStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync(studentId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions2_NullStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync2(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions2_EmptyStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync2(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions2_UnauthorizedUser()
            {
                studentsController.Request = new HttpRequestMessage();
                var rests = await studentsController.GetStudentRestrictionsAsync2("0000002");
            }

            [TestMethod]
            public async Task GetStudentRestrictions2_ValidStudentId()
            {
                studentsController.Request = new HttpRequestMessage();
                var rests = await studentsController.GetStudentRestrictionsAsync2(studentId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions3_NullStudentId()
            {
                var rests = await studentsController.GetStudentRestrictions3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions3_EmptyStudentId()
            {
                var rests = await studentsController.GetStudentRestrictions3Async(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRestrictions3_UnauthorizedUser()
            {
                studentsController.Request = new HttpRequestMessage();
                var rests = await studentsController.GetStudentRestrictions3Async("0000002");
            }

            [TestMethod]
            public async Task GetStudentRestrictions3_ValidStudentId()
            {
                studentsController.Request = new HttpRequestMessage();
                var rests = await studentsController.GetStudentRestrictions3Async(studentId);
            }

            // Needs mock permissions now

            //[TestMethod]
            //public void PostSearchStudent_Multi()
            //{
            //    StudentQuery qry = new StudentQuery() { lastName = "Dog", dateOfBirth = DateTime.Parse("3/3/33") };
            //    IEnumerable<Dtos.Student.Student> students = studentsController.PostSearchStudent(qry);
            //    Assert.AreEqual(3, students.Count());
            //}

            //[TestMethod]
            //public void PostSearchStudent_One()
            //{
            //    StudentQuery qry = new StudentQuery() { lastName = "Dog", dateOfBirth = DateTime.Parse("3/3/33"), firstName = "Able" };
            //    IEnumerable<Dtos.Student.Student> students = studentsController.PostSearchStudent(qry);
            //    Assert.AreEqual(1, students.Count());
            //}
            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public void PostSearchStudent_Empty()
            //{
            //    StudentQuery qry = new StudentQuery() { lastName = "Smith", dateOfBirth = DateTime.Parse("3/3/33") };
            //    IEnumerable<Dtos.Student.Student> students = studentsController.PostSearchStudent(qry);
            //}

            #region RegistrationTests

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Register_NullStudentId()
            {
                var messages = await studentsController.RegisterAsync(null, sectionRegistrations);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Register_NullSectionRegistrations()
            {
                var messages = await studentsController.RegisterAsync("1111", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Register_ZeroSectionRegistrations()
            {
                var messages =
                    await studentsController.RegisterAsync("1111", new List<Dtos.Student.SectionRegistration>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterValidationOnly_NullStudentGuid()
            {
                var messages = await studentsController.PostRegisterValidationOnlyAsync(null, new Dtos.Student.StudentRegistrationValidationOnlyRequest());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterValidationOnly_NullRequestDto()
            {
                var messages = await studentsController.PostRegisterValidationOnlyAsync("{aguid}", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterValidationOnly_RequestDtoWithNullActions()
            {
                var messages = await studentsController.PostRegisterValidationOnlyAsync("{aguid}", new Dtos.Student.StudentRegistrationValidationOnlyRequest());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterValidationOnly_RequestDtoWithZeroActions()
            {
                var request = new Dtos.Student.StudentRegistrationValidationOnlyRequest();
                request.SectionActionRequests = new List<SectionRegistrationGuid>();
                var messages = await studentsController.PostRegisterValidationOnlyAsync("{aguid}", request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterValidationOnly_TimeoutException()
            {
                string stuGuid = "{StuGuid1}";
                studentServiceMock.Setup(svc => svc.RegisterValidationOnlyAsync(stuGuid, It.IsAny<Dtos.Student.StudentRegistrationValidationOnlyRequest>()))
                    .Throws(new ColleagueSessionExpiredException("message"));


                var request = new Dtos.Student.StudentRegistrationValidationOnlyRequest();
                request.SectionActionRequests = new List<SectionRegistrationGuid>();
                request.SectionActionRequests.Add(new SectionRegistrationGuid());
                try
                {
                    var messages = await studentsController.PostRegisterValidationOnlyAsync(stuGuid, request);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterValidationOnly_PermissionException()
            {
                string stuGuid = "{StuGuid2}";
                studentServiceMock.Setup(svc => svc.RegisterValidationOnlyAsync(stuGuid, It.IsAny<Dtos.Student.StudentRegistrationValidationOnlyRequest>()))
                    .Throws(new PermissionsException("message"));


                var request = new Dtos.Student.StudentRegistrationValidationOnlyRequest();
                request.SectionActionRequests = new List<SectionRegistrationGuid>();
                request.SectionActionRequests.Add(new SectionRegistrationGuid());
                try
                {
                    var messages = await studentsController.PostRegisterValidationOnlyAsync(stuGuid, request);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterSkipValidations_NullStudentGuid()
            {
                var messages = await studentsController.PostRegisterSkipValidationsAsync(null, new Dtos.Student.StudentRegistrationSkipValidationsRequest());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterSkipValidations_NullRequestDto()
            {
                var messages = await studentsController.PostRegisterSkipValidationsAsync("{aguid}", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterSkipValidations_RequestDtoWithNullActions()
            {
                var messages = await studentsController.PostRegisterSkipValidationsAsync("{aguid}", new Dtos.Student.StudentRegistrationSkipValidationsRequest());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterSkipValidations_RequestDtoWithZeroActions()
            {
                var request = new Dtos.Student.StudentRegistrationSkipValidationsRequest();
                request.SectionActionRequests = new List<SectionRegistrationGuid>();
                var messages = await studentsController.PostRegisterSkipValidationsAsync("{aguid}", request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterSkipValidations_RequestDtoWithBothHomeStudentAndVisitingStudentFlags()
            {
                var request = new Dtos.Student.StudentRegistrationSkipValidationsRequest();
                request.SectionActionRequests = new List<SectionRegistrationGuid>();
                request.SectionActionRequests.Add(new SectionRegistrationGuid());
                request.CrossRegHomeStudent = true;
                request.CrossRegVisitingStudent = true;

                var messages = await studentsController.PostRegisterSkipValidationsAsync("{aguid}", request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterSkipValidations_TimeoutException()
            {
                string stuGuid = "{StuGuid1}";
                studentServiceMock.Setup(svc => svc.RegisterSkipValidationsAsync(stuGuid, It.IsAny<Dtos.Student.StudentRegistrationSkipValidationsRequest>()))
                    .Throws(new ColleagueSessionExpiredException("message"));


                var request = new Dtos.Student.StudentRegistrationSkipValidationsRequest();
                request.SectionActionRequests = new List<SectionRegistrationGuid>();
                request.SectionActionRequests.Add(new SectionRegistrationGuid());
                try
                {
                    var messages = await studentsController.PostRegisterSkipValidationsAsync(stuGuid, request);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostRegisterSkipValidations_PermissionException()
            {
                string stuGuid = "{StuGuid2}";
                studentServiceMock.Setup(svc => svc.RegisterSkipValidationsAsync(stuGuid, It.IsAny<Dtos.Student.StudentRegistrationSkipValidationsRequest>()))
                    .Throws(new PermissionsException("message"));


                var request = new Dtos.Student.StudentRegistrationSkipValidationsRequest();
                request.SectionActionRequests = new List<SectionRegistrationGuid>();
                request.SectionActionRequests.Add(new SectionRegistrationGuid());
                try
                {
                    var messages = await studentsController.PostRegisterSkipValidationsAsync(stuGuid, request);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            #endregion

            [TestMethod]
            public async Task GetAcademicHistory3Async_Test()
            {
                // Already set up other stuff in the initialization
                var result = await studentsController.GetAcademicHistory3Async(studentId, false, true, null);

                // Assert
                Assert.IsTrue(result is AcademicHistory3);
            }
        }

        [TestClass]
        public class GetAcademicHistory4Async_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }

            #endregion

            private IAcademicHistoryService academicHistoryService;
            private IStudentService studentService;
            private IStudentRestrictionService studentRestrictionService;
            private IStudentRepository studentRepo;
            private IStudentProgramRepository studentProgramRepo;
            private IRequirementRepository requirementRepo;
            private IAcademicCreditRepository acadCredRepo;
            private ICourseRepository courseRepo;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;
            private IEmergencyInformationService emergencyInformationService;
            private ILogger logger;
            private ApiSettings apiSettings;
            private string studentId = "0000001";
            private string lastName = "Smith";
            //private string studentProgramId1 = "10";
            private string[] programIds = new string[] { "BA.MATH", "BA.ENGL" };
            private string[] programCodes = new string[] { "PROG1", "PROG2" };
            private string[] catalogCodes = new string[] { "2011", "2012" };
            private string[] academicCreditIds = new string[] { "19000", "38001", "39" };
            private IEnumerable<Dtos.Student.SectionRegistration> sectionRegistrations;

            private IStudentRepository testStudentRepo;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                // mock needed ctor items
                Mock<IAcademicHistoryService> academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                Mock<IStudentRestrictionService> studentRestrictionServiceMock = new Mock<IStudentRestrictionService>();
                Mock<IStudentRepository> studentRepoMock = new Mock<IStudentRepository>();
                Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                Mock<IRequirementRepository> requirementRepoMock = new Mock<IRequirementRepository>();
                Mock<IAcademicCreditRepository> acadCredRepoMock = new Mock<IAcademicCreditRepository>();
                Mock<ICourseRepository> courseRepoMock = new Mock<ICourseRepository>();
                Mock<IEmergencyInformationService> emergencyInformationServiceMock =
                    new Mock<IEmergencyInformationService>();

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                academicHistoryService = academicHistoryServiceMock.Object;

                studentService = studentServiceMock.Object;
                studentRestrictionService = studentRestrictionServiceMock.Object;
                studentRepo = studentRepoMock.Object;
                studentProgramRepo = studentProgramRepoMock.Object;
                requirementRepo = requirementRepoMock.Object;
                acadCredRepo = acadCredRepoMock.Object;
                courseRepo = courseRepoMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                apiSettings = null;

                testStudentRepo = new TestStudentRepository();

                // mock student repo call
                var student = new Domain.Student.Entities.Student(studentId, lastName, 2,
                    new List<string> { programIds[0], programIds[1] },
                    new List<string> { academicCreditIds[0], academicCreditIds[1] });
                studentRepoMock.Setup(repo => repo.GetAsync(studentId)).Returns(Task.FromResult(student));

                // mock studentprogram repo call
                List<Ellucian.Colleague.Domain.Student.Entities.StudentProgram> stuPrograms =
                    new List<Ellucian.Colleague.Domain.Student.Entities.StudentProgram>();
                stuPrograms.Add(new Ellucian.Colleague.Domain.Student.Entities.StudentProgram(studentId, programCodes[0],
                    catalogCodes[0]));
                stuPrograms.Add(new Ellucian.Colleague.Domain.Student.Entities.StudentProgram(studentId, programCodes[1],
                    catalogCodes[1]));
                studentProgramRepoMock.Setup(repo => repo.GetAsync(studentId))
                    .ReturnsAsync(stuPrograms.AsEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentProgram>());
                studentProgramRepoMock.Setup(repo => repo.GetAsync(studentId, programIds[0]))
                    .Returns(Task.FromResult(stuPrograms[0]));
                studentProgramRepoMock.Setup(repo => repo.GetAsync(studentId, programIds[1]))
                    .Returns(Task.FromResult(stuPrograms[1]));

                // mock a valid Academic History Service response
                AcademicHistory academicHistory = new AcademicHistory();
                AcademicHistory3 academicHistory3 = new AcademicHistory3();
                AcademicHistory4 academicHistory4 = new AcademicHistory4();
                academicHistoryServiceMock.Setup(repo => repo.GetAcademicHistoryAsync(studentId, false, true, null))
                    .Returns(Task.FromResult(academicHistory));
                academicHistoryServiceMock.Setup(repo => repo.GetAcademicHistory3Async(studentId, false, true, null))
                    .Returns(Task.FromResult(academicHistory3));
                academicHistoryServiceMock.Setup(repo => repo.GetAcademicHistory4Async(studentId, false, true, null, It.IsAny<bool>()))
                    .Returns(Task.FromResult(academicHistory4));

                // mock a valid Student Restriction Service response
                IEnumerable<PersonRestriction> studentRestrictions = new List<PersonRestriction>();
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictionsAsync(studentId, true))
                    .Returns(Task.FromResult(studentRestrictions));
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictionsAsync("0000002", true))
                    .Throws(new PermissionsException());
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictionsAsync("0000002", false))
                    .Throws(new PermissionsException());
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictions2Async(studentId, true))
                    .Returns(Task.FromResult(studentRestrictions));
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictions2Async("0000002", true))
                    .Throws(new PermissionsException());
                studentRestrictionServiceMock.Setup(svc => svc.GetStudentRestrictions2Async("0000002", false))
                    .Throws(new PermissionsException());

                IEnumerable<Domain.Student.Entities.TranscriptRestriction> emptyRestrictions =
                    new List<Domain.Student.Entities.TranscriptRestriction>();
                IEnumerable<Domain.Student.Entities.TranscriptRestriction> oneRestriction =
                    new List<Domain.Student.Entities.TranscriptRestriction>()
                    {
                        new Domain.Student.Entities.TranscriptRestriction() {Code = "TEST", Description = "TEST"}
                    };
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictionsAsync(studentId))
                    .Returns(Task.FromResult(emptyRestrictions));
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictionsAsync("00000002"))
                    .Returns(Task.FromResult(oneRestriction));
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictionsAsync("00000003"))
                    .Throws(new HttpResponseException(System.Net.HttpStatusCode.NotFound));

                // mock a valid Student Service Trascript Access responses
                var oneRestrictionDto = new List<TranscriptRestriction>()
                {
                    new TranscriptRestriction() {Code = "TEST", Description = "TEST"}
                };

                var enforcedEmptyTranscriptAccess = new TranscriptAccess();
                enforcedEmptyTranscriptAccess.EnforceTranscriptRestriction = true;
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000001"))
                    .ReturnsAsync(enforcedEmptyTranscriptAccess);

                var unenforcedEmptyTranscriptAccess = new TranscriptAccess();
                unenforcedEmptyTranscriptAccess.EnforceTranscriptRestriction = false;
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000002"))
                    .ReturnsAsync(unenforcedEmptyTranscriptAccess);

                var enforcedOneTranscriptAccess = new TranscriptAccess();
                enforcedOneTranscriptAccess.EnforceTranscriptRestriction = true;
                enforcedOneTranscriptAccess.TranscriptRestrictions = oneRestrictionDto;
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000003"))
                    .ReturnsAsync(enforcedOneTranscriptAccess);

                var unforcedOneTranscriptAccess = new TranscriptAccess();
                unforcedOneTranscriptAccess.EnforceTranscriptRestriction = false;
                unforcedOneTranscriptAccess.TranscriptRestrictions = oneRestrictionDto;
                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000004"))
                    .ReturnsAsync(unforcedOneTranscriptAccess);

                studentServiceMock.Setup(svc => svc.GetTranscriptRestrictions2Async("00000005"))
                    .Throws(new HttpResponseException(System.Net.HttpStatusCode.NotFound));


                // mock a person emergency information service call
                var personEmergencyInformation = new Ellucian.Colleague.Dtos.Base.EmergencyInformation()
                {
                    PersonId = "1234567",
                    EmergencyContacts = new List<EmergencyContact>()
                };
                emergencyInformationServiceMock.Setup(svc => svc.GetEmergencyInformationAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(personEmergencyInformation));


                // mock a valid student search response
                var studentDto1 = new Dtos.Student.Student() { Id = "00000001", LastName = "Dog", FirstName = "Able" };
                var studentDto2 = new Dtos.Student.Student() { Id = "00000002", LastName = "Dog", FirstName = "Baker" };
                var studentDto3 = new Dtos.Student.Student() { Id = "00000003", LastName = "Dog", FirstName = "Charlie" };

                var justOne = new List<Dtos.Student.Student>() { studentDto1 };
                var justTwo = new List<Dtos.Student.Student>() { studentDto2 };
                var allThree = new List<Dtos.Student.Student>() { studentDto1, studentDto2, studentDto3 };


                studentServiceMock.Setup(svc => svc.SearchAsync("Dog", DateTime.Parse("3/3/33"), null, null, null, null))
                    .Returns(Task.FromResult(allThree.AsEnumerable()));
                studentServiceMock.Setup(
                    svc => svc.SearchAsync("Dog", DateTime.Parse("3/3/33"), "Baker", null, null, null))
                    .Returns(Task.FromResult(justTwo.AsEnumerable()));
                studentServiceMock.Setup(
                    svc => svc.SearchAsync("Dog", DateTime.Parse("3/3/33"), "Able", null, null, null))
                    .Returns(Task.FromResult(justOne.AsEnumerable()));
                studentServiceMock.Setup(
                    svc => svc.SearchAsync("Dog", DateTime.Parse("3/3/33"), "Baker", null, null, null))
                    .Returns(Task.FromResult(justTwo.AsEnumerable()));
                studentServiceMock.Setup(svc => svc.SearchAsync("Smith", null, null, null, null, null))
                    .Throws(new HttpResponseException(System.Net.HttpStatusCode.NotFound));

                // mock valid transcript order response
                string tr = "<xml><something>mock response</something></xml>";
                studentServiceMock.Setup(svc => svc.OrderTranscriptAsync(It.IsAny<TranscriptRequest>()))
                    .Returns(Task.FromResult(tr));

                // invalid transcript order response
                studentServiceMock.Setup(svc => svc.OrderTranscriptAsync(null)).Throws(new Exception());

                // Set up sectionRegistrations
                sectionRegistrations = new List<Dtos.Student.SectionRegistration>()
                {
                    new Dtos.Student.SectionRegistration()
                    {
                        Action = Dtos.Student.RegistrationAction.Add,
                        Credits = null,
                        SectionId = "1111"
                    }
                };

                // mock DTO adapters
                var stuAdapter =
                    new AutoMapperAdapter
                        <Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(
                        adapterRegistry, logger);
                var stuProgAdapter =
                    new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram>(
                        adapterRegistry, logger);
                var AcademicHistoryDto3Adapter =
                    new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, AcademicHistory3>(
                        adapterRegistry, logger);
                adapterRegistryMock.Setup(
                    reg =>
                        reg
                            .GetAdapter
                            <Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student
                                >()).Returns(stuAdapter);
                adapterRegistryMock.Setup(
                    reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, StudentProgram>())
                    .Returns(stuProgAdapter);

                // mock student service call
                var student1 = new Domain.Student.Entities.Student(studentId, lastName, 2,
                    new List<string> { programIds[0], programIds[1] },
                    new List<string> { academicCreditIds[0], academicCreditIds[1] });
                var studentDto = stuAdapter.MapToType(student1);
                studentServiceMock.Setup(svc => svc.GetAsync(studentId)).Returns(Task.FromResult(new PrivacyWrapper<Dtos.Student.Student>(studentDto, false)));

                // mock controller
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger,
                    apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentsController = null;
                adapterRegistry = null;
                studentRepo = null;
                studentProgramRepo = null;
                acadCredRepo = null;
                courseRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAcademicHistory4Async_Permissions_Exception()
            {
                // mock needed ctor items
                Mock<IAcademicHistoryService> academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryServiceMock.Setup(repo => repo.GetAcademicHistory4Async(studentId, false, true, null, false))
                    .ThrowsAsync(new PermissionsException());
                academicHistoryService = academicHistoryServiceMock.Object;

                // mock controller
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger,
                    apiSettings);

                var history = await studentsController.GetAcademicHistory4Async(studentId);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAcademicHistory4Async_Other_Exception()
            {
                // mock needed ctor items
                Mock<IAcademicHistoryService> academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryServiceMock.Setup(repo => repo.GetAcademicHistory4Async(studentId, false, true, null, false))
                    .ThrowsAsync(new ApplicationException());
                academicHistoryService = academicHistoryServiceMock.Object;

                // mock controller
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger,
                    apiSettings);

                var history = await studentsController.GetAcademicHistory4Async(studentId);
            }

            [TestMethod]
            public async Task GetAcademicHistory4Async_Valid()
            {
                var history = await studentsController.GetAcademicHistory4Async(studentId);
                Assert.IsNotNull(history);
            }
        }

        [TestClass]
        public class QueryStudentByPost2Async_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }

            #endregion

            private IAcademicHistoryService academicHistoryService;
            private IStudentService studentService;
            private IStudentRestrictionService studentRestrictionService;
            private IStudentRepository studentRepo;
            private IStudentProgramRepository studentProgramRepo;
            private IRequirementRepository requirementRepo;
            private IAcademicCreditRepository acadCredRepo;
            private ICourseRepository courseRepo;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;
            private IEmergencyInformationService emergencyInformationService;
            private ILogger logger;
            private ApiSettings apiSettings;
            private IStudentRepository testStudentRepo;

            private PrivacyWrapper<List<Dtos.Student.Student>> studentDtos;

            [TestInitialize]
            public void QueryStudentByPost2Async_Tests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentDtos = new PrivacyWrapper<List<Dtos.Student.Student>>();
                studentDtos.Dto = new List<Dtos.Student.Student>() { new Dtos.Student.Student()
                    {
                        Id = "0001234",
                        LastName = "Smith"
                    } };


                // mock needed ctor items
                Mock<IAcademicHistoryService> academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                studentServiceMock.Setup(svc => svc.Search3Async(It.IsAny<StudentSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(studentDtos);
                Mock<IStudentRestrictionService> studentRestrictionServiceMock = new Mock<IStudentRestrictionService>();
                Mock<IStudentRepository> studentRepoMock = new Mock<IStudentRepository>();
                Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                Mock<IRequirementRepository> requirementRepoMock = new Mock<IRequirementRepository>();
                Mock<IAcademicCreditRepository> acadCredRepoMock = new Mock<IAcademicCreditRepository>();
                Mock<ICourseRepository> courseRepoMock = new Mock<ICourseRepository>();
                Mock<IEmergencyInformationService> emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                academicHistoryService = academicHistoryServiceMock.Object;
                studentService = studentServiceMock.Object;
                studentRestrictionService = studentRestrictionServiceMock.Object;
                studentRepo = studentRepoMock.Object;
                studentProgramRepo = studentProgramRepoMock.Object;
                requirementRepo = requirementRepoMock.Object;
                acadCredRepo = acadCredRepoMock.Object;
                courseRepo = courseRepoMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                apiSettings = null;

                HttpContext.Current = new HttpContext(new HttpRequest("", "http://localhost/api/qapi/students", ""), new HttpResponse(new StringWriter()));

                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger, apiSettings);
            }

            [TestCleanup]
            public void QueryStudentByPost2Async_Tests_Cleanup()
            {
                studentsController = null;
                adapterRegistry = null;
                studentRepo = null;
                studentProgramRepo = null;
                acadCredRepo = null;
                courseRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentByPost2Async_handles_PermissionsException()
            {
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                studentServiceMock.Setup(svc => svc.Search3Async(It.IsAny<StudentSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new PermissionsException());
                studentService = studentServiceMock.Object;
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger, apiSettings);
                var result = await studentsController.QueryStudentByPost2Async(new StudentSearchCriteria() { StudentKeyword = "0001234" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentByPost2Async_handles_generic_Exception()
            {
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                studentServiceMock.Setup(svc => svc.Search3Async(It.IsAny<StudentSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new ApplicationException());
                studentService = studentServiceMock.Object;
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger, apiSettings);
                var result = await studentsController.QueryStudentByPost2Async(new StudentSearchCriteria() { StudentKeyword = "0001234" });
            }

            [TestMethod]
            public async Task QueryStudentByPost2Async_valid_without_privacy_restrictions()
            {
                var result = await studentsController.QueryStudentByPost2Async(new StudentSearchCriteria() { StudentKeyword = "0001234" });
                Assert.AreEqual(studentDtos.Dto.Count, result.Count());
            }

            [TestMethod]
            public async Task QueryStudentByPost2Async_valid_with_privacy_restrictions()
            {
                studentDtos.HasPrivacyRestrictions = true;
                var result = await studentsController.QueryStudentByPost2Async(new StudentSearchCriteria() { StudentKeyword = "0001234" });
                Assert.AreEqual(studentDtos.Dto.Count, result.Count());
            }
        }

        [TestClass]
        public class QueryStudentsById4Async_Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }

            #endregion

            private IAcademicHistoryService academicHistoryService;
            private IStudentService studentService;
            private IStudentRestrictionService studentRestrictionService;
            private IStudentRepository studentRepo;
            private IStudentProgramRepository studentProgramRepo;
            private IRequirementRepository requirementRepo;
            private IAcademicCreditRepository acadCredRepo;
            private ICourseRepository courseRepo;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;
            private IEmergencyInformationService emergencyInformationService;
            private ILogger logger;
            private ApiSettings apiSettings;
            private IStudentRepository testStudentRepo;

            private PrivacyWrapper<IEnumerable<StudentBatch3>> studentBatch3s;

            [TestInitialize]
            public void QueryStudentsById4Async_Tests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentBatch3s = new PrivacyWrapper<IEnumerable<Dtos.Student.StudentBatch3>>();
                studentBatch3s.Dto = new List<Dtos.Student.StudentBatch3>()
                {
                    new StudentBatch3()
                    {
                        Id = "0001234",
                        LastName = "Smith"
                    }
                };

                // mock needed ctor items
                Mock<IAcademicHistoryService> academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                studentServiceMock.Setup(svc => svc.QueryStudentsById4Async(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(studentBatch3s);
                Mock<IStudentRestrictionService> studentRestrictionServiceMock = new Mock<IStudentRestrictionService>();
                Mock<IStudentRepository> studentRepoMock = new Mock<IStudentRepository>();
                Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                Mock<IRequirementRepository> requirementRepoMock = new Mock<IRequirementRepository>();
                Mock<IAcademicCreditRepository> acadCredRepoMock = new Mock<IAcademicCreditRepository>();
                Mock<ICourseRepository> courseRepoMock = new Mock<ICourseRepository>();
                Mock<IEmergencyInformationService> emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                academicHistoryService = academicHistoryServiceMock.Object;
                studentService = studentServiceMock.Object;
                studentRestrictionService = studentRestrictionServiceMock.Object;
                studentRepo = studentRepoMock.Object;
                studentProgramRepo = studentProgramRepoMock.Object;
                requirementRepo = requirementRepoMock.Object;
                acadCredRepo = acadCredRepoMock.Object;
                courseRepo = courseRepoMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                apiSettings = null;

                HttpContext.Current = new HttpContext(new HttpRequest("", "http://localhost/api/qapi/students", ""), new HttpResponse(new StringWriter()));


                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger, apiSettings);
            }

            [TestCleanup]
            public void QueryStudentsById4Async_Tests_Cleanup()
            {
                studentsController = null;
                adapterRegistry = null;
                studentRepo = null;
                studentProgramRepo = null;
                acadCredRepo = null;
                courseRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentsById4Async_handles_PermissionsException()
            {
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                studentServiceMock.Setup(svc => svc.QueryStudentsById4Async(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                studentService = studentServiceMock.Object;
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger, apiSettings);
                var result = await studentsController.QueryStudentsById4Async(new StudentQueryCriteria() { StudentIds = new List<string>() { "0001234" } });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryStudentsById4Async_handles_generic_Exception()
            {
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                studentServiceMock.Setup(svc => svc.QueryStudentsById4Async(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ThrowsAsync(new ApplicationException());
                studentService = studentServiceMock.Object;
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger, apiSettings);
                var result = await studentsController.QueryStudentsById4Async(new StudentQueryCriteria() { StudentIds = new List<string>() { "0001234" } });
            }

            [TestMethod]
            public async Task QueryStudentsById4Async_valid_without_privacy_restrictions()
            {
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                studentServiceMock.Setup(svc => svc.QueryStudentsById4Async(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(studentBatch3s);
                studentService = studentServiceMock.Object;
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger, apiSettings);
                var result = await studentsController.QueryStudentsById4Async(new StudentQueryCriteria() { StudentIds = new List<string>() { "0001234" } });
                Assert.AreEqual(studentBatch3s.Dto.Count(), result.Count());
            }

            [TestMethod]
            public async Task QueryStudentsById4Async_valid_with_privacy_restrictions()
            {
                studentBatch3s.HasPrivacyRestrictions = true;
                Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
                studentServiceMock.Setup(svc => svc.QueryStudentsById4Async(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(studentBatch3s);
                studentService = studentServiceMock.Object;
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger, apiSettings);
                var result = await studentsController.QueryStudentsById4Async(new StudentQueryCriteria() { StudentIds = new List<string>() { "0001234" } });
                Assert.AreEqual(studentBatch3s.Dto.Count(), result.Count());
            }
        }

        [TestClass]
        public class StudentTests_PutStudent2Tests
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }

            #endregion

            private IAcademicHistoryService academicHistoryService;
            private IStudentService studentService;
            private IStudentRestrictionService studentRestrictionService;
            private IStudentRepository studentRepo;
            private IStudentProgramRepository studentProgramRepo;
            private IRequirementRepository requirementRepo;
            private IAcademicCreditRepository acadCredRepo;
            private ICourseRepository courseRepo;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;
            private IEmergencyInformationService emergencyInformationService;
            private ILogger logger;
            private ApiSettings apiSettings;
            private string studentId = "0000001";
            private string lastName = "Smith";
            //private string studentProgramId1 = "10";
            private string[] programIds = new string[] { "BA.MATH", "BA.ENGL" };
            private string[] programCodes = new string[] { "PROG1", "PROG2" };
            private string[] catalogCodes = new string[] { "2011", "2012" };
            private string[] academicCreditIds = new string[] { "19000", "38001", "39" };

            private string personFilter = "1a507924-f207-460a-8c1d-1854ebe80565";
            private string typeFilter = "1a507924-f207-460a-8c1d-1854ebe80561";
            private string cohortsFilter = "1b507924-f207-460a-8c1d-1854ebe80561";
            private string studentGuid = "1a507924-f207-460a-8c1d-1854ebe80566";
            private string residencyFilter = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";

            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter
                = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter personCriteriaFilter
                = new Web.Http.Models.QueryStringFilter("personFilter", "");

            private Ellucian.Colleague.Dtos.Students2 studentsDto;

            Mock<IAcademicHistoryService> academicHistoryServiceMock;
            Mock<IStudentService> studentServiceMock;
            Mock<IStudentRestrictionService> studentRestrictionServiceMock;
            Mock<IStudentRepository> studentRepoMock;
            Mock<IStudentProgramRepository> studentProgramRepoMock;
            Mock<IRequirementRepository> requirementRepoMock;
            Mock<IAcademicCreditRepository> acadCredRepoMock;
            Mock<ICourseRepository> courseRepoMock;
            Mock<IEmergencyInformationService> emergencyInformationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;

            private IStudentRepository testStudentRepo;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                // mock needed ctor items
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                studentServiceMock = new Mock<IStudentService>();
                studentRestrictionServiceMock = new Mock<IStudentRestrictionService>();
                studentRepoMock = new Mock<IStudentRepository>();
                studentProgramRepoMock = new Mock<IStudentProgramRepository>();
                requirementRepoMock = new Mock<IRequirementRepository>();
                acadCredRepoMock = new Mock<IAcademicCreditRepository>();
                courseRepoMock = new Mock<ICourseRepository>();
                emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();

                academicHistoryService = academicHistoryServiceMock.Object;
                studentService = studentServiceMock.Object;
                studentRestrictionService = studentRestrictionServiceMock.Object;
                studentRepo = studentRepoMock.Object;
                studentProgramRepo = studentProgramRepoMock.Object;
                requirementRepo = requirementRepoMock.Object;
                acadCredRepo = acadCredRepoMock.Object;
                courseRepo = courseRepoMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                emergencyInformationService = emergencyInformationServiceMock.Object;

                testStudentRepo = new TestStudentRepository();

                // setup students Dto object                
                studentsDto = new Dtos.Students2();
                studentsDto.Id = studentGuid;
                studentsDto.Person = new GuidObject2(personFilter);
                studentsDto.Residencies = new List<StudentResidenciesDtoProperty>()
                {  new StudentResidenciesDtoProperty()
                    { Residency = new GuidObject2("1a507924-f207-460a-8c1d-1854ebe80567") } };
                studentsDto.Types = new List<StudentTypesDtoProperty>()
                {  new StudentTypesDtoProperty() { Type  =  new GuidObject2(typeFilter) } };
                studentsDto.LevelClassifications = new List<StudentLevelClassificationsDtoProperty>()
                {  new StudentLevelClassificationsDtoProperty() { Level = new GuidObject2("2a507924-f207-460a-8c1d-1854ebe80561") }};

                var studentDtoList = new List<Students2>() { studentsDto };
                var studentTuple = new Tuple<IEnumerable<Students2>, int>(studentDtoList, 1);

                // GetStudents2Async(int offset, int limit, Dtos.Students2 criteriaFilter, string personFilter, bool bypassCache = false);
                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Students2>(), personFilter, It.IsAny<bool>()))
                        .ReturnsAsync(studentTuple);

                studentServiceMock.Setup(
                    s =>
                        s.GetStudents2Async(It.IsAny<int>(), It.IsAny<int>(), null, string.Empty, It.IsAny<bool>()))
                        .ReturnsAsync(studentTuple);

                studentServiceMock.Setup(s => s.GetStudentsByGuid2Async(studentGuid, It.IsAny<bool>())).ReturnsAsync(studentsDto);

                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Students2>(), It.IsAny<bool>())).ReturnsAsync(studentsDto);

                // create new students controller
                studentsController = new StudentsController(adapterRegistry, academicHistoryService, studentService,
                    studentProgramRepo, studentRestrictionService, requirementRepo, emergencyInformationService, logger,
                    apiSettings)
                {
                    Request = new HttpRequestMessage()
                };

                studentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(studentsDto));

            }

            [TestMethod]
            public async Task StudentsController_PutStudent2_V16()
            {
                var student = await studentsController.PutStudent2Async(studentGuid, studentsDto);
                Assert.AreSame(student.Id, studentGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2Async_Null_Guid_Exception_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>())).Throws<Exception>();
                await studentsController.PutStudent2Async(string.Empty, studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2Async_Null_Student_Exception_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>())).Throws<Exception>();
                await studentsController.PutStudent2Async(studentGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2Async_Empty_Guid_Exception_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>())).Throws<Exception>();
                await studentsController.PutStudent2Async(Guid.Empty.ToString(), studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2Async_Guid_Mismatch_Exception_V16()
            {
                studentsDto.Id = Guid.NewGuid().ToString();
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>())).Throws<Exception>();
                await studentsController.PutStudent2Async(studentGuid, studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2_KeyNotFoundException_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>()))
                    .Throws<KeyNotFoundException>();
                await studentsController.PutStudent2Async(studentGuid, studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentController_PutStudent2_PermissionsException_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>()))
                    .Throws<PermissionsException>();
                await studentsController.PutStudent2Async(studentGuid, studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2_ArgumentException_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>()))
                    .Throws<ArgumentException>();
                await studentsController.PutStudent2Async(studentGuid, studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2_RepositoryException_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>()))
                    .Throws<RepositoryException>();
                await studentsController.PutStudent2Async(studentGuid, studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2_IntegrationApiException_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>()))
                    .Throws<IntegrationApiException>();
                await studentsController.PutStudent2Async(studentGuid, studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2_Exception_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>()))
                    .Throws<Exception>();
                await studentsController.PutStudent2Async(studentGuid, studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2_PermissionsException_V16()
            {
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Dtos.Students2>(), It.IsAny<bool>()))
                    .Throws<PermissionsException>();
                await studentsController.PutStudent2Async(studentGuid, studentsDto);
            }

            [TestMethod]
            public async Task StudentsController_PutStudent2_Permissions()
            {   
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudents2Async" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);
                studentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(studentsDto));

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateStudentInformation);

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Students2>(), It.IsAny<bool>())).ReturnsAsync(studentsDto);
                var actual = await studentsController.PutStudent2Async(studentGuid, studentsDto);

                Object filterObject;
                studentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateStudentInformation));

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_PutStudent2_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Students" },
                    { "action", "GetStudents2Async" }
                };
                HttpRoute route = new HttpRoute("students", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentsController.ControllerContext;
                var actionDescriptor = studentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    //studentServiceMock.Setup(x => x.UpdateStudents2Async(It.IsAny<Students2>(), It.IsAny<bool>())).Throws<PermissionsException>();
                    studentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update Student Information."));
                    await studentsController.PutStudent2Async(studentGuid, studentsDto);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            [TestCleanup]
            public void Cleanup()
            {
                testStudentRepo = null;
                studentsController = null;
                academicHistoryService = null;
                studentService = null;
                studentRestrictionService = null;
                studentRepo = null;
                studentProgramRepo = null;
                requirementRepo = null;
                acadCredRepo = null;
                courseRepo = null;
                adapterRegistry = null;
                logger = null;
                emergencyInformationService = null;
            }
        }

        #region Planning Student

        [TestClass]
        public class GetPlanningStudent_Tests
        {
            #region Test Context

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

            #endregion Test Context

            private IStudentProgramRepository studentProgramRepo;
            private IStudentService studentService;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;

            Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
            Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();

            private ApiSettings apiSettings;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentProgramRepo = studentProgramRepoMock.Object;
                studentService = studentServiceMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                // mock students controller
                studentsController = new StudentsController(adapterRegistry, null, studentService, studentProgramRepo,
                    null, null, null, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentsController = null;
                adapterRegistry = null;
                studentProgramRepo = null;
            }

            [TestMethod]
            public async Task GetPlanningStudent()
            {
                // arrange
                var planningStudentDto = new PlanningStudent() { Id = "0000001", DegreePlanId = 12, LastName = "smith" };
                studentServiceMock.Setup(svc => svc.GetPlanningStudentAsync(It.IsAny<string>())).Returns(Task.FromResult(new PrivacyWrapper<Dtos.Student.PlanningStudent>(planningStudentDto, false)));

                // act
                var result = await studentsController.GetPlanningStudentAsync("0000001");

                // assert
                Assert.IsTrue(result is PlanningStudent);
                Assert.AreEqual("0000001", result.Id);
            }
        }

        #endregion Planning Student

        #region DropRegistrationAsync

        [TestClass]
        public class StudentsController_DropRegistrationAsync_Tests
        {
            #region Test Context

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

            #endregion Test Context

            private IStudentProgramRepository studentProgramRepo;
            private IStudentService studentService;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;

            Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
            Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();

            private ApiSettings apiSettings;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentProgramRepo = studentProgramRepoMock.Object;
                studentService = studentServiceMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                // mock students controller
                studentsController = new StudentsController(adapterRegistry, null, studentService, studentProgramRepo,
                    null, null, null, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentsController = null;
                adapterRegistry = null;
                studentProgramRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_DropRegistrationAsync_null_StudentId()
            {
                var dropReg = await studentsController.DropRegistrationAsync(null, new SectionDropRegistration()
                {
                    SectionId = "12345",
                    DropReasonCode = "BECAUSE"
                });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_DropRegistrationAsync_null_SectionDropRegistration()
            {
                var dropReg = await studentsController.DropRegistrationAsync("0001234", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_DropRegistrationAsync_null_SectionDropRegistration_SectionId()
            {
                var dropReg = await studentsController.DropRegistrationAsync("0001234", new SectionDropRegistration());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_DropRegistrationAsync_permissions_exception()
            {
                studentServiceMock.Setup(ss => ss.DropRegistrationAsync(It.IsAny<string>(), It.IsAny<SectionDropRegistration>())).ThrowsAsync(new PermissionsException());
                var dropReg = await studentsController.DropRegistrationAsync("0001234", new SectionDropRegistration()
                {
                    SectionId = "12345",
                    DropReasonCode = "BECAUSE"
                });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_DropRegistrationAsync_general_exception()
            {
                studentServiceMock.Setup(ss => ss.DropRegistrationAsync(It.IsAny<string>(), It.IsAny<SectionDropRegistration>())).ThrowsAsync(new ApplicationException());
                var dropReg = await studentsController.DropRegistrationAsync("0001234", new SectionDropRegistration()
                {
                    SectionId = "12345",
                    DropReasonCode = "BECAUSE"
                });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_DropRegistrationAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {

                try
                {
                    studentServiceMock.Setup(ss => ss.DropRegistrationAsync(It.IsAny<string>(), It.IsAny<SectionDropRegistration>()))
                        .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    await studentsController.DropRegistrationAsync("0001234", new SectionDropRegistration()
                    {
                        SectionId = "12345",
                        DropReasonCode = "BECAUSE"
                    });
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public async Task StudentsController_DropRegistrationAsync_valid()
            {
                studentServiceMock.Setup(ss => ss.DropRegistrationAsync(It.IsAny<string>(), It.IsAny<SectionDropRegistration>())).ReturnsAsync(new RegistrationResponse()
                {
                    RegisteredSectionIds = new List<string>(),
                    Messages = new List<RegistrationMessage>() { new RegistrationMessage() { SectionId = "12345", Message = "Dropped" } }
                });
                var dropReg = await studentsController.DropRegistrationAsync("0001234", new SectionDropRegistration()
                {
                    SectionId = "12345",
                    DropReasonCode = "BECAUSE"
                });
                Assert.AreEqual(1, dropReg.Messages.Count);
            }
        }

        #endregion

        #region GetRegistrationEligibility3Async Tests
        [TestClass]
        public class StudentsController_GetRegistrationEligibility3Async_Tests
        {
            #region Test Context

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

            #endregion Test Context

            private IStudentProgramRepository studentProgramRepo;
            private IStudentService studentService;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;

            Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
            Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();

            private ApiSettings apiSettings;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentProgramRepo = studentProgramRepoMock.Object;
                studentService = studentServiceMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                // mock students controller
                studentsController = new StudentsController(adapterRegistry, null, studentService, studentProgramRepo,
                    null, null, null, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentsController = null;
                adapterRegistry = null;
                studentProgramRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetRegistrationEligibility3Async_null_StudentId()
            {
                await studentsController.GetRegistrationEligibility3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetRegistrationEligibility3Async_permissions_exception()
            {
                studentServiceMock.Setup(ss => ss.CheckRegistrationEligibility3Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await studentsController.GetRegistrationEligibility3Async("0001234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetRegistrationEligibility3Async_general_exception()
            {
                studentServiceMock.Setup(ss => ss.CheckRegistrationEligibility3Async(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
                await studentsController.GetRegistrationEligibility3Async("0001234");
            }

            [TestMethod]
            public async Task StudentsController_GetRegistrationEligibility3Async_valid()
            {
                studentServiceMock.Setup(ss => ss.CheckRegistrationEligibility3Async(It.IsAny<string>())).ReturnsAsync(new Dtos.Student.RegistrationEligibility()
                {
                    Terms = new List<RegistrationEligibilityTerm>(),
                    Messages = new List<RegistrationMessage>() { new RegistrationMessage() { SectionId = "12345", Message = "Registration Eligibility Message" } }
                });
                var registrationEligibility = await studentsController.GetRegistrationEligibility3Async("0001234");
                Assert.AreEqual(1, registrationEligibility.Messages.Count());
            }
        }
        #endregion

        #region GetRegistrationPrioritiesAsync Tests
        [TestClass]
        public class StudentsController_GetRegistrationPrioritiesAsync_Tests
        {
            #region Test Context

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

            #endregion Test Context

            private IStudentProgramRepository studentProgramRepo;
            private IStudentService studentService;
            private IAdapterRegistry adapterRegistry;
            private StudentsController studentsController;

            Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
            Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();
            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();

            private ApiSettings apiSettings;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentProgramRepo = studentProgramRepoMock.Object;
                studentService = studentServiceMock.Object;
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                // mock students controller
                studentsController = new StudentsController(adapterRegistry, null, studentService, studentProgramRepo,
                    null, null, null, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentsController = null;
                adapterRegistry = null;
                studentProgramRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetRegistrationPrioritiesAsync_null_StudentId()
            {
                try
                {
                    await studentsController.GetRegistrationPrioritiesAsync(null);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetRegistrationPrioritiesAsync_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    studentServiceMock.Setup(ss => ss.GetRegistrationPrioritiesAsync(It.IsAny<string>())).ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    await studentsController.GetRegistrationPrioritiesAsync("0001234");

                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetRegistrationPrioritiesAsync_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    studentServiceMock.Setup(ss => ss.GetRegistrationPrioritiesAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                    await studentsController.GetRegistrationPrioritiesAsync("0001234");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentsController_GetRegistrationPrioritiesAsync_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentServiceMock.Setup(ss => ss.GetRegistrationPrioritiesAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
                    await studentsController.GetRegistrationPrioritiesAsync("0001234");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public async Task StudentsController_GetRegistrationPrioritiesAsync_valid()
            {
                var registrationPriority = new RegistrationPriority
                {
                    Id = "1",
                    StudentId = "0001234",
                    TermCode = "2029/FA",
                    Start = DateTimeOffset.Now.AddDays(-10),
                    End = DateTimeOffset.Now.AddDays(10),
                };

                studentServiceMock.Setup(ss => ss.GetRegistrationPrioritiesAsync(It.IsAny<string>())).ReturnsAsync(new List<RegistrationPriority>() { registrationPriority });
                var registrationPriorities = await studentsController.GetRegistrationPrioritiesAsync("0001234");
                var priority = registrationPriorities.FirstOrDefault();

                Assert.IsNotNull(priority);
                Assert.AreEqual(1, registrationPriorities.Count());
                Assert.AreEqual(registrationPriority.Id, priority.Id);
                Assert.AreEqual(registrationPriority.StudentId, priority.StudentId);
                Assert.AreEqual(registrationPriority.TermCode, priority.TermCode);
                Assert.AreEqual(registrationPriority.Start, priority.Start);
                Assert.AreEqual(registrationPriority.End, priority.End);
            }
        }
        #endregion
    }
}
