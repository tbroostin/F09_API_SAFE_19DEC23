// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Web.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Newtonsoft.Json;
using Ellucian.Web.Security;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Api.Controllers.Student;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentDegreePlansControllerTests
    {
        [TestClass]
        public class StudentDegreePlanController_Get
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

            #endregion

            private IStudentDegreePlanService studentDegreePlanService;
            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            DegreePlan degreePlan2;
            DegreePlan degreePlan4;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;

                var testStudentDegreePlanRepo = new TestStudentDegreePlanRepository();
                var testTermRepo = new TestTermRepository();
                var testStudentProgramRepo = new TestStudentProgramRepository();

                degreePlan2 = BuildDegreePlanDto(await testStudentDegreePlanRepo.GetAsync(2));
                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlanAsync(2)).Returns(Task.FromResult(degreePlan2));

                degreePlan4 = BuildDegreePlanDto(await testStudentDegreePlanRepo.GetAsync(4));
                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlanAsync(4)).Returns(Task.FromResult(degreePlan4));

                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlanAsync(99)).Throws(new ArgumentException());

                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_GetAsync_Success()
            {
                var degreePlan = await studentDegreePlanController.GetAsync(2);
                Assert.AreEqual(degreePlan2, degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_GetAsync_PlanDoesNotExist()
            {
                var degreePlan = await studentDegreePlanController.GetAsync(99);
            }

            private DegreePlan BuildDegreePlanDto(Domain.Student.Entities.DegreePlans.DegreePlan degreePlan)
            {
                var degreePlanDto = new DegreePlan();
                degreePlanDto.Id = degreePlan.Id;
                degreePlanDto.PersonId = degreePlan.PersonId;
                return degreePlanDto;
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_InvalidDegreePlanId()
            {
                var messages = await studentDegreePlanController.PutRegistrationAsync(0, "test");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_EmptyTermId()
            {
                var messages = await studentDegreePlanController.PutRegistrationAsync(1, "");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_NullTermId()
            {
                var messages = await studentDegreePlanController.PutRegistrationAsync(1, null);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Get2
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;

            private ILogger logger;

            DegreePlan2 degreePlan2;
            DegreePlan2 degreePlan4;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;

                var testStudentDegreePlanRepo = new TestStudentDegreePlanRepository();
                var testTermRepo = new TestTermRepository();
                var testStudentProgramRepo = new TestStudentProgramRepository();

                degreePlan2 = BuildDegreePlanDto(await testStudentDegreePlanRepo.GetAsync(2));
                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlan2Async(2)).Returns(Task.FromResult(degreePlan2));

                degreePlan4 = BuildDegreePlanDto(await testStudentDegreePlanRepo.GetAsync(4));
                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlan2Async(4)).Returns(Task.FromResult(degreePlan4));

                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlan2Async(99)).Throws(new ArgumentException());

                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Get2Async_Success()
            {
                var degreePlan = await studentDegreePlanController.Get2Async(2);
                Assert.AreEqual(degreePlan2, degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Get2Async_PlanDoesNotExist()
            {
                var degreePlan = await studentDegreePlanController.Get2Async(99);
            }

            private DegreePlan2 BuildDegreePlanDto(Domain.Student.Entities.DegreePlans.DegreePlan degreePlan)
            {
                var degreePlanDto = new DegreePlan2();
                degreePlanDto.Id = degreePlan.Id;
                degreePlanDto.PersonId = degreePlan.PersonId;
                return degreePlanDto;
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Get3
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            DegreePlan3 degreePlan2;
            DegreePlan3 degreePlan4;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                var testDegreePlanRepo = new TestStudentDegreePlanRepository();
                var testTermRepo = new TestTermRepository();
                var testStudentProgramRepo = new TestStudentProgramRepository();

                degreePlan2 = BuildDegreePlanDto(await testDegreePlanRepo.GetAsync(2));
                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlan3Async(2)).Returns(Task.FromResult(degreePlan2));

                degreePlan4 = BuildDegreePlanDto(await testDegreePlanRepo.GetAsync(4));
                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlan3Async(4)).Returns(Task.FromResult(degreePlan4));

                studentDegreePlanServiceMock.Setup(repo => repo.GetDegreePlan3Async(99)).Throws(new ArgumentException());

                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Get3Async_Success()
            {
                var degreePlan = await studentDegreePlanController.Get3Async(2);
                Assert.AreEqual(degreePlan2, degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Get3Async_PlanDoesNotExist()
            {
                var degreePlan = await studentDegreePlanController.Get3Async(99);
            }

            private DegreePlan3 BuildDegreePlanDto(Domain.Student.Entities.DegreePlans.DegreePlan degreePlan)
            {
                var degreePlanDto = new DegreePlan3();
                degreePlanDto.Id = degreePlan.Id;
                degreePlanDto.PersonId = degreePlan.PersonId;
                return degreePlanDto;
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Get4
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;

            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Get4Async_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory2() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm2>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(svc => svc.GetDegreePlan4Async(degreePlan.Id, true)).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Get4Async(degreePlan.Id);
                Assert.IsTrue(result is DegreePlanAcademicHistory);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }

            [TestMethod]
            public async Task StudentDegreePlanController_GetUnvalidated_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory2() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm2>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(svc => svc.GetDegreePlan4Async(degreePlan.Id, false)).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Get4Async(degreePlan.Id, false);
                Assert.IsTrue(result is DegreePlanAcademicHistory);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Get5Async
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;

            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Get5Async_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory2()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory3() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm3>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(svc => svc.GetDegreePlan5Async(degreePlan.Id, true)).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Get5Async(degreePlan.Id);
                Assert.IsTrue(result is DegreePlanAcademicHistory2);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }

            [TestMethod]
            public async Task StudentDegreePlanController_GetUnvalidated_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory2()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory3() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm3>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(svc => svc.GetDegreePlan5Async(degreePlan.Id, false)).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Get5Async(degreePlan.Id, false);
                Assert.IsTrue(result is DegreePlanAcademicHistory2);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Get6Async
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;

            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Get6Async_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory3()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory4() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm4>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(svc => svc.GetDegreePlan6Async(degreePlan.Id, true, It.IsAny<bool>())).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Get6Async(degreePlan.Id);
                Assert.IsTrue(result is DegreePlanAcademicHistory3);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Get6Async_Unvalidated_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory3()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory4() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm4>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(svc => svc.GetDegreePlan6Async(degreePlan.Id, false, It.IsAny<bool>())).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Get6Async(degreePlan.Id, false);
                Assert.IsTrue(result is DegreePlanAcademicHistory3);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Get6Async_Permissions_Exception()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory3()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory4() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm4>() }
                };

                // mock needed ctor items
                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(svc => svc.GetDegreePlan6Async(degreePlan.Id, false, It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                studentDegreePlanService = studentDegreePlanServiceMock.Object;

                // mock controller
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);

                var result = await studentDegreePlanController.Get6Async(degreePlan.Id, false);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Get6Async_Generic_Exception()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory3()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory4() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm4>() }
                };

                // mock needed ctor items
                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(svc => svc.GetDegreePlan6Async(degreePlan.Id, false, It.IsAny<bool>())).ThrowsAsync(new ApplicationException());
                studentDegreePlanService = studentDegreePlanServiceMock.Object;

                // mock controller
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);

                var result = await studentDegreePlanController.Get6Async(degreePlan.Id, false);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Put
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;

            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_PutAsync_Success()
            {
                var degreePlan = new DegreePlan();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm>();
                var degreePlanTerm = new DegreePlanTerm();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse>() {
                    new PlannedCourse() { CourseId = "5" },
                    new PlannedCourse() { CourseId = "10" },
                    new PlannedCourse() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlanAsync(It.Is<DegreePlan>(r => r.Id == 5))).Returns(Task.FromResult(degreePlan));

                var updatedDegreePlan = await studentDegreePlanController.PutAsync(degreePlan);
                Assert.AreEqual(updatedDegreePlan.Id, degreePlan.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_PutAsync_VersionMismatch()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlanAsync(It.Is<DegreePlan>(r => r.Id == 10))).Throws(new InvalidOperationException());

                DegreePlan degreePlan = new DegreePlan();
                degreePlan.Id = 10;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm>();

                var updatedDegreePlan = await studentDegreePlanController.PutAsync(degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_PutAsync_Failure()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlanAsync(It.Is<DegreePlan>(r => r.Id == 99))).Throws(new ArgumentException());

                DegreePlan degreePlan = new DegreePlan();
                degreePlan.Id = 99;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm>();

                var updatedDegreePlan = await studentDegreePlanController.PutAsync(degreePlan);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Put2
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Put2Async_Success()
            {
                var degreePlan = new DegreePlan2();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm2>();
                var degreePlanTerm = new DegreePlanTerm2();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse2>() {
                    new PlannedCourse2() { CourseId = "5" },
                    new PlannedCourse2() { CourseId = "10" },
                    new PlannedCourse2() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan2Async(It.Is<DegreePlan2>(r => r.Id == 5))).Returns(Task.FromResult(degreePlan));

                var updatedDegreePlan = await studentDegreePlanController.Put2Async(degreePlan);
                Assert.AreEqual(updatedDegreePlan.Id, degreePlan.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put2Async_VersionMismatch()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan2Async(It.Is<DegreePlan2>(r => r.Id == 10))).Throws(new InvalidOperationException());

                var degreePlan = new DegreePlan2();
                degreePlan.Id = 10;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm2>();

                var updatedDegreePlan = await studentDegreePlanController.Put2Async(degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put2Async_Failure()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan2Async(It.Is<DegreePlan2>(r => r.Id == 99))).Throws(new ArgumentException());

                var degreePlan = new DegreePlan2();
                degreePlan.Id = 99;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm2>();

                var updatedDegreePlan = await studentDegreePlanController.Put2Async(degreePlan);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Put3
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Put3Async_Success()
            {
                var degreePlan = new DegreePlan3();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm3>();
                var degreePlanTerm = new DegreePlanTerm3();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse3>() {
                    new PlannedCourse3() { CourseId = "5" },
                    new PlannedCourse3() { CourseId = "10" },
                    new PlannedCourse3() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan3Async(It.Is<DegreePlan3>(r => r.Id == 5))).Returns(Task.FromResult(degreePlan));

                var updatedDegreePlan = await studentDegreePlanController.Put3Async(degreePlan);
                Assert.AreEqual(updatedDegreePlan.Id, degreePlan.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put3Async_VersionMismatch()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan3Async(It.Is<DegreePlan3>(r => r.Id == 10))).Throws(new InvalidOperationException());

                var degreePlan = new DegreePlan3();
                degreePlan.Id = 10;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm3>();

                var updatedDegreePlan = await studentDegreePlanController.Put3Async(degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put3Async_Failure()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan3Async(It.Is<DegreePlan3>(r => r.Id == 99))).Throws(new ArgumentException());

                var degreePlan = new DegreePlan3();
                degreePlan.Id = 99;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm3>();

                var updatedDegreePlan = await studentDegreePlanController.Put3Async(degreePlan);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Put4
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Put4Async_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory2() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm2>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan4Async(It.Is<DegreePlan4>(r => r.Id == 5))).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Put4Async(degreePlan);
                Assert.IsTrue(result is DegreePlanAcademicHistory);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put4Async_VersionMismatch()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan4Async(It.Is<DegreePlan4>(r => r.Id == 10))).Throws(new InvalidOperationException());

                var degreePlan = new DegreePlan4();
                degreePlan.Id = 10;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();

                var updatedDegreePlan = await studentDegreePlanController.Put4Async(degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put4Async_Failure()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan4Async(It.Is<DegreePlan4>(r => r.Id == 99))).Throws(new ArgumentException());

                var degreePlan = new DegreePlan4();
                degreePlan.Id = 99;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();

                var updatedDegreePlan = await studentDegreePlanController.Put4Async(degreePlan);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Put5Async
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

            #endregion

            //private Mock<IDegreePlanService> degreePlanServiceMock;
            //private IDegreePlanService degreePlanService;
            //private DegreePlansController degreePlanController;
            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Put5Async_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory2()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory3() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm3>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan5Async(It.Is<DegreePlan4>(r => r.Id == 5))).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Put5Async(degreePlan);
                Assert.IsTrue(result is DegreePlanAcademicHistory2);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put5Async_VersionMismatch()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan5Async(It.Is<DegreePlan4>(r => r.Id == 10))).Throws(new InvalidOperationException());

                var degreePlan = new DegreePlan4();
                degreePlan.Id = 10;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();

                var updatedDegreePlan = await studentDegreePlanController.Put5Async(degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put5Async_Failure()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan5Async(It.Is<DegreePlan4>(r => r.Id == 99))).Throws(new ArgumentException());

                var degreePlan = new DegreePlan4();
                degreePlan.Id = 99;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();

                var updatedDegreePlan = await studentDegreePlanController.Put5Async(degreePlan);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_Put6Async
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            public async Task StudentDegreePlanController_Put6Async_Success()
            {
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory3()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory4() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm4>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan6Async(It.Is<DegreePlan4>(r => r.Id == 5))).Returns(Task.FromResult(returnDto));

                var result = await studentDegreePlanController.Put6Async(degreePlan);
                Assert.IsTrue(result is DegreePlanAcademicHistory3);
                Assert.AreEqual(degreePlan.Id, result.DegreePlan.Id);
                Assert.AreEqual(degreePlan.PersonId, result.AcademicHistory.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put6Async_VersionMismatch()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan6Async(It.Is<DegreePlan4>(r => r.Id == 10))).Throws(new InvalidOperationException());

                var degreePlan = new DegreePlan4();
                degreePlan.Id = 10;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();

                var updatedDegreePlan = await studentDegreePlanController.Put6Async(degreePlan);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentDegreePlanController_Put6Async_Failure()
            {
                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlan6Async(It.Is<DegreePlan4>(r => r.Id == 99))).Throws(new ArgumentException());

                var degreePlan = new DegreePlan4();
                degreePlan.Id = 99;
                degreePlan.PersonId = "0004444";
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();

                var updatedDegreePlan = await studentDegreePlanController.Put6Async(degreePlan);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_CreateDegreePlan5Async
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [Ignore] // Ignore this test because we currently don't know how to mock up the response inside of the controller
            [TestMethod]
            public async Task StudentDegreePlan_CreateDegreePlan5Async()
            {
                var degreePlan = new DegreePlanAcademicHistory2();
                degreePlan.AcademicHistory = new Dtos.Student.AcademicHistory3();
                degreePlan.AcademicHistory.OverallGradePointAverage = 3.5m;
                var id = "0000123";


                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(repo => repo.CreateDegreePlan5Async(id)).Returns(Task.FromResult(degreePlan));

                var newDegreePlanResponse = await studentDegreePlanController.Post5Async(id);
                var newDegreePlan = JsonConvert.DeserializeObject<DegreePlanAcademicHistory2>(newDegreePlanResponse.Content.ReadAsStringAsync().Result);
                Assert.IsInstanceOfType(newDegreePlan, typeof(DegreePlanAcademicHistory2));
                Assert.AreEqual(degreePlan.AcademicHistory.OverallGradePointAverage, newDegreePlan.AcademicHistory.OverallGradePointAverage);
            }
        }

        [TestClass]
        public class StudentDegreePlanController_CreateDegreePlan6Async
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

            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;
                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [Ignore] // Ignore this test because we currently don't know how to mock up the response inside of the controller
            [TestMethod]
            public async Task StudentDegreePlan_CreateDegreePlan6Async()
            {
                var id = "0000123";
                var degreePlan = new DegreePlan4();
                degreePlan.Id = 5;
                degreePlan.PersonId = id;
                degreePlan.Version = 14;
                degreePlan.Terms = new List<DegreePlanTerm4>();
                var degreePlanTerm = new DegreePlanTerm4();
                degreePlanTerm.TermId = "2012/FA";
                degreePlanTerm.PlannedCourses = new List<PlannedCourse4>() {
                    new PlannedCourse4() { CourseId = "5" },
                    new PlannedCourse4() { CourseId = "10" },
                    new PlannedCourse4() { CourseId = "15" }
                };
                degreePlan.Terms.Add(degreePlanTerm);
                var returnDto = new DegreePlanAcademicHistory3()
                {
                    DegreePlan = degreePlan,
                    AcademicHistory = new Dtos.Student.AcademicHistory4() { StudentId = degreePlan.PersonId, AcademicTerms = new List<Dtos.Student.AcademicTerm4>() }
                };

                // Mock the degree plan service that updates a plan.
                studentDegreePlanServiceMock.Setup(repo => repo.CreateDegreePlan6Async(id)).Returns(Task.FromResult(returnDto));

                var newDegreePlanResponse = await studentDegreePlanController.Post6Async(id);
                var newDegreePlan = JsonConvert.DeserializeObject<DegreePlanAcademicHistory3>(newDegreePlanResponse.Content.ReadAsStringAsync().Result);
                Assert.IsInstanceOfType(newDegreePlan, typeof(DegreePlanAcademicHistory3));
                Assert.AreEqual(returnDto.AcademicHistory.OverallGradePointAverage, newDegreePlan.AcademicHistory.OverallGradePointAverage);
            }
        }




        [TestClass]
        public class StudentDegreePlanController_PutRegistration
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
            #endregion

            private Mock<IStudentDegreePlanService> studentDegreePlanServiceMock;
            private IStudentDegreePlanService studentDegreePlanService;
            private StudentDegreePlansController studentDegreePlanController;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                studentDegreePlanServiceMock = new Mock<IStudentDegreePlanService>();
                studentDegreePlanService = studentDegreePlanServiceMock.Object;

                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlanAsync(It.Is<DegreePlan>(r => r.Id == 99))).Throws(new ArgumentException());

                // Mock the UpdatreDegreePlan when the service fails to return a valid degree plan.
                studentDegreePlanServiceMock.Setup(repo => repo.UpdateDegreePlanAsync(It.Is<DegreePlan>(r => r.Id == 10))).Throws(new InvalidOperationException());

                studentDegreePlanController = new StudentDegreePlansController(studentDegreePlanService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentDegreePlanController = null;
                studentDegreePlanService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_InvalidDegreePlanId()
            {
                var messages = await studentDegreePlanController.PutRegistrationAsync(0, "test");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_EmptyTermId()
            {
                var messages = await studentDegreePlanController.PutRegistrationAsync(1, "");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_NullTermId()
            {
                var messages = await studentDegreePlanController.PutRegistrationAsync(1, null);
            }
        }


    }
}
