// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentsControllerTests
    {
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
                studentsDto.Tags = new List<GuidObject2>() {new GuidObject2("1a507924-f207-460a-8c1d-1854ebe80562")};
                studentsDto.Type = new GuidObject2(typeFilter);
                studentsDto.Cohorts = new List<GuidObject2>() {new GuidObject2(cohortsFilter)};
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
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
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
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
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
            private string[] programIds = new string[] {"BA.MATH", "BA.ENGL"};
            private string[] programCodes = new string[] {"PROG1", "PROG2"};
            private string[] catalogCodes = new string[] {"2011", "2012"};
            private string[] academicCreditIds = new string[] {"19000", "38001", "39"};
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


                testStudentRepo = new TestStudentRepository();

                // mock student repo call
                var student = new Domain.Student.Entities.Student(studentId, lastName, 2,
                    new List<string> {programIds[0], programIds[1]},
                    new List<string> {academicCreditIds[0], academicCreditIds[1]});
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
                var studentDto1 = new Dtos.Student.Student() {Id = "00000001", LastName = "Dog", FirstName = "Able"};
                var studentDto2 = new Dtos.Student.Student() {Id = "00000002", LastName = "Dog", FirstName = "Baker"};
                var studentDto3 = new Dtos.Student.Student() {Id = "00000003", LastName = "Dog", FirstName = "Charlie"};

                var justOne = new List<Dtos.Student.Student>() {studentDto1};
                var justTwo = new List<Dtos.Student.Student>() {studentDto2};
                var allThree = new List<Dtos.Student.Student>() {studentDto1, studentDto2, studentDto3};


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
                    new List<string> {programIds[0], programIds[1]},
                    new List<string> {academicCreditIds[0], academicCreditIds[1]});
                var studentDto = stuAdapter.MapToType(student1);
                studentServiceMock.Setup(svc => svc.GetAsync(studentId)).Returns(Task.FromResult(new PrivacyWrapper<Dtos.Student.Student>(studentDto,false)));

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
            public async Task Get_StudentPrograms_All()
            {
                List<StudentProgram> studentProgramDTO =
                    (List<StudentProgram>) (await studentsController.GetStudentProgramsAsync(studentId));
                Assert.AreEqual(2, studentProgramDTO.Count);
                Assert.AreEqual(studentId, studentProgramDTO[0].StudentId);
                Assert.AreEqual(studentId, studentProgramDTO[1].StudentId);
                Assert.AreEqual(programCodes[0], studentProgramDTO[0].ProgramCode);
                Assert.AreEqual(programCodes[1], studentProgramDTO[1].ProgramCode);
                Assert.AreEqual(catalogCodes[0], studentProgramDTO[0].CatalogCode);
                Assert.AreEqual(catalogCodes[1], studentProgramDTO[1].CatalogCode);
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
            [ExpectedException(typeof (HttpResponseException))]
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
            [ExpectedException(typeof (HttpResponseException))]
            public async Task GetTranscriptRestrictions2_MissingStudent()
            {
                var transcriptAccess = await studentsController.GetTranscriptRestrictions2Async("00000005");
            }

            [TestMethod]
            [ExpectedException(typeof (HttpResponseException))]
            public async Task GetStudentRestrictions_NullStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof (HttpResponseException))]
            public async Task GetStudentRestrictions_EmptyStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof (HttpResponseException))]
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
            [ExpectedException(typeof (HttpResponseException))]
            public async Task GetStudentRestrictions2_NullStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync2(null);
            }

            [TestMethod]
            [ExpectedException(typeof (HttpResponseException))]
            public async Task GetStudentRestrictions2_EmptyStudentId()
            {
                var rests = await studentsController.GetStudentRestrictionsAsync2(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof (HttpResponseException))]
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

            [TestMethod]
            [ExpectedException(typeof (HttpResponseException))]
            public async Task Register_NullStudentId()
            {
                var messages = await studentsController.RegisterAsync(null, sectionRegistrations);
            }

            [TestMethod]
            [ExpectedException(typeof (HttpResponseException))]
            public async Task Register_NullSectionRegistrations()
            {
                var messages = await studentsController.RegisterAsync("1111", null);
            }

            [TestMethod]
            [ExpectedException(typeof (HttpResponseException))]
            public async Task Register_ZeroSectionRegistrations()
            {
                var messages =
                    await studentsController.RegisterAsync("1111", new List<Dtos.Student.SectionRegistration>());
            }

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
    }
}
