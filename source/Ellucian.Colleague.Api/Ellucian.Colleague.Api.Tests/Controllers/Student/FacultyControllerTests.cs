// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class FacultyControllerTests
    {
        /// <summary>
        /// Set up class to use for each faculty controller test class
        /// </summary>
        public abstract class FacultyControllerTestSetup
        {
            #region Test Context

            protected TestContext testContextInstance;

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

            public FacultyController facultyController;
            public Mock<IFacultyService> facultyServiceMock;
            public IFacultyService facultyService;
            public Mock<IFacultyRestrictionService> facultyRestrictionServiceMock;
            public IFacultyRestrictionService facultyRestrictionService;
            public Mock<ILogger> loggerMock;
            public IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Faculty> allFaculty;

            public async Task InitializeFacultyController()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                facultyServiceMock = new Mock<IFacultyService>();
                facultyService = facultyServiceMock.Object;
                facultyRestrictionServiceMock = new Mock<IFacultyRestrictionService>();
                facultyRestrictionService = facultyRestrictionServiceMock.Object;
                allFaculty = await new TestFacultyRepository().GetAsync();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object) { Request = new HttpRequestMessage() };
                facultyController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                facultyController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();

            }

        }

        [TestClass]
        public class FacultyController_PostFacultyAsync : FacultyControllerTestSetup
        {
            string facultyId;
            private List<Domain.Student.Entities.Faculty> entities;
            private List<Faculty> dtos;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                facultyId = "0000036";
                entities = allFaculty.ToList();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                dtos = new List<Faculty>();
                foreach (var entity in entities)
                {
                    dtos.Add(Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>(entity));
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task FacultyController_PostFacultyAsync_Valid_Request()
            {
                // arrange
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(dtos.FirstOrDefault());

                // act
                var response = await facultyController.PostFacultyAsync(facultyId);

                // assert
                Assert.IsNotNull(response);
                Assert.IsTrue(response is IEnumerable<Faculty>);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_PostFacultyAsync_Bad_Request()
            {
                // arrange
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());

                // act
                var response = await facultyController.PostFacultyAsync(facultyId);
            }
        }

        [TestClass]
        public class FacultyController_GetFacultyAsync: FacultyControllerTestSetup
        {
            string facultyId;
            private Faculty facultyDto;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                facultyId = "0000036";
                var facultyEntity = allFaculty.Where(f => f.Id == facultyId ).First();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                facultyDto = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>(facultyEntity);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task FacultyController_GetFacultyAsync_Valid_Request()
            {
                // arrange
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(facultyDto));

                // act
                var response = await facultyController.GetFacultyAsync(facultyId);

                // assert
                Assert.IsTrue(response is Faculty);
                Assert.AreEqual(facultyId, response.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_GetFacultyAsync_Bad_Request()
            {
                // arrange
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());

                // act
                var response = await facultyController.GetFacultyAsync(facultyId);
            }
        }

        [TestClass]
        public class FacultyController_GetFacultyRestrictions : FacultyControllerTestSetup
        {
            string facultyId;
            private Faculty facultyDto;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                facultyId = "0000036";
                var facultyEntity = allFaculty.Where(f => f.Id == facultyId).First();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                facultyDto = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>(facultyEntity);
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(facultyDto));
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task ReturnsFacultyRestrictions()
            {
                // arrange
                facultyRestrictionServiceMock.Setup(svc => svc.GetFacultyRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Dtos.Base.PersonRestriction>>(new List<PersonRestriction>()
                    {
                        new PersonRestriction() {Id = "1", Title = "Restriction1"},
                        new PersonRestriction() {Id = "2", Title = "Restriction2"}
                    }));
                // act
                var restrictions = await facultyController.GetFacultyRestrictionsAsync(facultyId);
                // assert
                Assert.IsTrue(restrictions is IEnumerable<PersonRestriction>);
            }

        }

        [TestClass]
        public class FacultyController_GetFacultySectionsAsync : FacultyControllerTestSetup
        {
            string facultyId;
            private Faculty facultyDto;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                facultyId = "0000036";
                var facultyEntity = allFaculty.Where(f => f.Id == facultyId).First();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                facultyDto = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>(facultyEntity);
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(facultyDto));
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySectionsAsync_null_ID_returns_empty_list()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section>>(new List<Section>()
                    {
                        new Section() {Id = "1", CourseId = "12" },
                        new Section() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySectionsAsync(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySectionsAsync(null, getDate, getDate, false);
                // Assert
                CollectionAssert.AreEqual(new List<Section>(), sections.ToList());
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySectionsAsync_Valid_Privacy_Restricted()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section>>(new List<Section>()
                    {
                        new Section() {Id = "1", CourseId = "12" },
                        new Section() {Id = "2", CourseId = "22" }
                    }, true);
                facultyServiceMock.Setup(svc => svc.GetFacultySectionsAsync(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySectionsAsync(facultyId, getDate, getDate, false);
                // Assert
                Assert.IsTrue(sections is IEnumerable<Section>);
                Assert.AreEqual(2, sections.Count());
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySectionsAsync_Valid_not_Privacy_Restricted()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section>>(new List<Section>()
                    {
                        new Section() {Id = "1", CourseId = "12" },
                        new Section() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySectionsAsync(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySectionsAsync(facultyId, getDate, getDate, false);
                // Assert
                Assert.IsTrue(sections is IEnumerable<Section>);
                Assert.AreEqual(2, sections.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_GetFacultySectionsAsync_handles_exceptions()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section>>(new List<Section>()
                    {
                        new Section() {Id = "1", CourseId = "12" },
                        new Section() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySectionsAsync(facultyId, getDate, getDate, false)).ThrowsAsync(new ApplicationException("An error occurred."));
                // Act
                var sections = await facultyController.GetFacultySectionsAsync(facultyId, getDate, getDate, false);
            }
        }

        [TestClass]
        public class FacultyController_GetFacultySections2Async : FacultyControllerTestSetup
        {
            string facultyId;
            private Faculty facultyDto;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                facultyId = "0000036";
                var facultyEntity = allFaculty.Where(f => f.Id == facultyId).First();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                facultyDto = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>(facultyEntity);
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(facultyDto));
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections2Async_null_ID_returns_empty_list()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section2>>(new List<Section2>()
                    {
                        new Section2() {Id = "1", CourseId = "12" },
                        new Section2() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections2Async(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySections2Async(null, getDate, getDate, false);
                // Assert
                CollectionAssert.AreEqual(new List<Section>(), sections.ToList());
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections2Async_Valid_Privacy_Restricted()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section2>>(new List<Section2>()
                    {
                        new Section2() {Id = "1", CourseId = "12" },
                        new Section2() {Id = "2", CourseId = "22" }
                    }, true);
                facultyServiceMock.Setup(svc => svc.GetFacultySections2Async(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySections2Async(facultyId, getDate, getDate, false);
                // Assert
                Assert.IsTrue(sections is IEnumerable<Section2>);
                Assert.AreEqual(2, sections.Count());
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections2Async_Valid_not_Privacy_Restricted()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section2>>(new List<Section2>()
                    {
                        new Section2() {Id = "1", CourseId = "12" },
                        new Section2() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections2Async(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySections2Async(facultyId, getDate, getDate, false);
                // Assert
                Assert.IsTrue(sections is IEnumerable<Section2>);
                Assert.AreEqual(2, sections.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_GetFacultySections2Async_handles_exceptions()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section2>>(new List<Section2>()
                    {
                        new Section2() {Id = "1", CourseId = "12" },
                        new Section2() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections2Async(facultyId, getDate, getDate, false)).ThrowsAsync(new ApplicationException("An error occurred."));
                // Act
                var sections = await facultyController.GetFacultySections2Async(facultyId, getDate, getDate, false);
            }
        }

        [TestClass]
        public class FacultyController_GetFacultySections3Async : FacultyControllerTestSetup
        {
            string facultyId;
            private Faculty facultyDto;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                facultyId = "0000036";
                var facultyEntity = allFaculty.Where(f => f.Id == facultyId).First();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                facultyDto = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>(facultyEntity);
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(facultyDto));
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections3Async_null_ID_returns_empty_list()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section3>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>()
                    {
                        new Section3() {Id = "1", CourseId = "12" },
                        new Section3() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections3Async(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySections3Async(null, getDate, getDate, false);
                // Assert
                CollectionAssert.AreEqual(new List<Section3>(), sections.ToList());
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections3Async_Valid_Privacy_Restricted()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section3>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>()
                    {
                        new Section3() {Id = "1", CourseId = "12" },
                        new Section3() {Id = "2", CourseId = "22" }
                    }, true);
                facultyServiceMock.Setup(svc => svc.GetFacultySections3Async(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySections3Async(facultyId, getDate, getDate, false);
                // Assert
                Assert.IsTrue(sections is IEnumerable<Section3>);
                Assert.AreEqual(2, sections.Count());
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections3Async_Valid_not_Privacy_Restricted()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section3>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>()
                    {
                        new Section3() {Id = "1", CourseId = "12" },
                        new Section3() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections3Async(facultyId, getDate, getDate, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySections3Async(facultyId, getDate, getDate, false);
                // Assert
                Assert.IsTrue(sections is IEnumerable<Section3>);
                Assert.AreEqual(2, sections.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_GetFacultySections3Async_handles_exceptions()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section3>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>()
                    {
                        new Section3() {Id = "1", CourseId = "12" },
                        new Section3() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections3Async(facultyId, getDate, getDate, false)).ThrowsAsync(new ApplicationException("An error occurred."));
                // Act
                var sections = await facultyController.GetFacultySections3Async(facultyId, getDate, getDate, false);
            }
        }

        [TestClass]
        public class FacultyController_GetFacultySections4Async : FacultyControllerTestSetup
        {
            string facultyId;
            private Faculty facultyDto;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                facultyId = "0000036";
                var facultyEntity = allFaculty.Where(f => f.Id == facultyId).First();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object) { Request = new HttpRequestMessage() };
                facultyController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                facultyController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                facultyDto = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>(facultyEntity);
                facultyServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(facultyDto));
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections4Async_null_ID_returns_empty_list()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section3>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>()
                    {
                        new Section3() {Id = "1", CourseId = "12" },
                        new Section3() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections4Async(facultyId, getDate, getDate, false, It.IsAny<bool>())).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                // Act
                var sections = await facultyController.GetFacultySections4Async(null, getDate, getDate, false);
                // Assert
                CollectionAssert.AreEqual(new List<Section3>(), sections.ToList());
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections4Async_Valid_Privacy_Restricted_Cache()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section3>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>()
                    {
                        new Section3() {Id = "1", CourseId = "12" },
                        new Section3() {Id = "2", CourseId = "22" }
                    }, true);
                facultyServiceMock.Setup(svc => svc.GetFacultySections4Async(facultyId, getDate, getDate, false, true)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
                facultyController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                // Act
                var sections = await facultyController.GetFacultySections4Async(facultyId, getDate, getDate, false);
                // Assert
                Assert.IsTrue(sections is IEnumerable<Section3>);
                Assert.AreEqual(2, sections.Count());
            }

            [TestMethod]
            public async Task FacultyController_GetFacultySections4Async_Valid_not_Privacy_Restricted_no_Cache()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section3>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>()
                    {
                        new Section3() {Id = "1", CourseId = "12" },
                        new Section3() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections4Async(facultyId, getDate, getDate, false, false)).ReturnsAsync(privacyWrapper);

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
                facultyController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                // Act
                var sections = await facultyController.GetFacultySections4Async(facultyId, getDate, getDate, false);
                // Assert
                Assert.IsTrue(sections is IEnumerable<Section3>);
                Assert.AreEqual(2, sections.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_GetFacultySections4Async_handles_exceptions()
            {
                // Arrange
                DateTime? getDate = DateTime.Now;
                PrivacyWrapper<IEnumerable<Section3>> privacyWrapper = new Coordination.Base.PrivacyWrapper<IEnumerable<Section3>>(new List<Section3>()
                    {
                        new Section3() {Id = "1", CourseId = "12" },
                        new Section3() {Id = "2", CourseId = "22" }
                    }, false);
                facultyServiceMock.Setup(svc => svc.GetFacultySections4Async(facultyId, getDate, getDate, false, It.IsAny<bool>())).ThrowsAsync(new ApplicationException("An error occurred."));

                // Set up an Http Context
                HttpResponse response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
                facultyController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };


                // Act
                var sections = await facultyController.GetFacultySections4Async(facultyId, getDate, getDate, false);
            }
        }

        [TestClass]
        public class FacultyController_QueryFacultyByPost: FacultyControllerTestSetup
        {
            private List<string> facultyIds;
            private List<Faculty> facultyList;
            FacultyQueryCriteria criteria;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                facultyIds = new List<string>() { "0000036", "0000045", "0000046", "0000048", "9999999" };
                var facultyEntityList = allFaculty.Where(f => facultyIds.Contains(f.Id)).ToList();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                facultyList = new List<Faculty>();
                foreach (var faculty in facultyEntityList)
                {
                    Faculty target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>(faculty);
                    facultyList.Add(target);
                }
                facultyRestrictionServiceMock.Setup(svc => svc.GetFacultyRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Dtos.Base.PersonRestriction>>(new List<PersonRestriction>()));

                criteria = new FacultyQueryCriteria() { FacultyIds = facultyIds };
                facultyServiceMock.Setup(x => x.QueryFacultyAsync(criteria)).Returns(Task.FromResult<IEnumerable<Faculty>>(facultyList));
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task ReturnsFacultyDtos()
            {
                // act
                var response = await facultyController.QueryFacultyByPostAsync(criteria);

                // assert
                Assert.IsTrue(response is IEnumerable<Faculty>);
                Assert.AreEqual(4, response.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RethrowsNullArgumentException()
            {
                // arrange
                criteria.FacultyIds = null;
                facultyServiceMock.Setup(x => x.QueryFacultyAsync(criteria)).Throws(new ArgumentNullException());

                // act
                var response =await facultyController.QueryFacultyByPostAsync(criteria);
            }
        }

        [TestClass]
        public class FacultyController_GetFacultyRestrictionsAsync : FacultyControllerTestSetup
        {
            private List<Domain.Base.Entities.PersonRestriction> entities;
            private List<PersonRestriction> dtos;
            private string userId;
            private string otherUserId;
            private string appExId;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                userId = "0001234";
                otherUserId = "0001235";
                appExId = "0001236";
                dtos = new List<PersonRestriction>()
                {
                    new PersonRestriction()
                    {
                        Id = "1",
                        Details = "Details",
                        EndDate = DateTime.Today.AddDays(7),
                        Hyperlink = "http://www.ellucian.com",
                        HyperlinkText = "Go to this website",
                        OfficeUseOnly = false,
                        RestrictionId = "12",
                        Severity = 3,
                        StartDate = DateTime.Today.AddDays(-7),
                        StudentId = userId,
                        Title = "Title"
                    }
                };

                facultyRestrictionServiceMock.Setup(fs => fs.GetFacultyRestrictionsAsync(userId)).ReturnsAsync(dtos);
                facultyRestrictionServiceMock.Setup(fs => fs.GetFacultyRestrictionsAsync(otherUserId)).ThrowsAsync(new PermissionsException());
                facultyRestrictionServiceMock.Setup(fs => fs.GetFacultyRestrictionsAsync(appExId)).ThrowsAsync(new ApplicationException());

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task FacultyController_GetFacultyRestrictionsAsync_Valid_Request()
            {
                var restrictions = await facultyController.GetFacultyRestrictionsAsync(userId);
                Assert.IsNotNull(restrictions);
                Assert.AreEqual(dtos.Count, restrictions.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_GetFacultyRestrictionsAsync_Forbidden()
            {
                var restrictions = await facultyController.GetFacultyRestrictionsAsync(otherUserId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_GetFacultyRestrictionsAsync_BadRequest()
            {
                var restrictions = await facultyController.GetFacultyRestrictionsAsync(appExId);
            }
        }

        [TestClass]
        public class FacultyController_PostFacultyIdsAsync : FacultyControllerTestSetup
        {
            FacultyQueryCriteria criteria;
            private List<string> ids;

            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                criteria = new FacultyQueryCriteria();
                ids = allFaculty.Where(f => f != null).Select(f => f.Id).ToList();

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task FacultyController_PostFacultyIdsAsync_Valid_Request_with_Criteria()
            {
                // arrange
                facultyServiceMock.Setup(x => x.SearchFacultyIdsAsync(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(ids);

                // act
                var response = await facultyController.PostFacultyIdsAsync(criteria);

                // assert
                Assert.IsNotNull(response);
                Assert.IsTrue(response is IEnumerable<string>);
            }

            [TestMethod]
            public async Task FacultyController_PostFacultyIdsAsync_Valid_Request_null_Criteria()
            {
                // arrange - when criteria is null, false/true are passed to the coordination service
                // any other combination will throw an exception, so this test will verify that passing null criteria 
                // falls back to the defaults
                facultyServiceMock.Setup(x => x.SearchFacultyIdsAsync(true, false)).ThrowsAsync(new ApplicationException());
                facultyServiceMock.Setup(x => x.SearchFacultyIdsAsync(true, true)).ThrowsAsync(new ApplicationException());
                facultyServiceMock.Setup(x => x.SearchFacultyIdsAsync(false, false)).ThrowsAsync(new ApplicationException());
                facultyServiceMock.Setup(x => x.SearchFacultyIdsAsync(false, true)).ReturnsAsync(ids);

                // act
                var response = await facultyController.PostFacultyIdsAsync(null);

                // assert
                Assert.IsNotNull(response);
                Assert.IsTrue(response is IEnumerable<string>);
                CollectionAssert.AreEqual(ids, response.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task FacultyController_PostFacultyIdsAsync_Bad_Request()
            {
                // arrange
                facultyServiceMock.Setup(x => x.SearchFacultyIdsAsync(It.IsAny<bool>(), It.IsAny<bool>())).ThrowsAsync(new ApplicationException());

                // act
                var response = await facultyController.PostFacultyIdsAsync(criteria);
            }
        }



        [TestClass]
        public class FacultyController_GetFacultyPermissions2Async_Tests : FacultyControllerTestSetup
        {
            

            private FacultyPermissions dto;


            [TestInitialize]
            public async void Initialize()
            {
                await InitializeFacultyController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                dto = new FacultyPermissions()
                {
                    CanGrantFacultyConsent = true,
                    CanGrantStudentPetition = true,
                    CanUpdateGrades = true,
                    CanWaivePrerequisiteRequirement = true,

                };

                facultyController = new FacultyController(facultyService, facultyRestrictionService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();

                facultyRestrictionServiceMock.Setup(svc => svc.GetFacultyRestrictionsAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Dtos.Base.PersonRestriction>>(new List<PersonRestriction>()));

                // mock an FacultyPermissions DTO to return
                facultyServiceMock.Setup(x => x.GetFacultyPermissions2Async()).ReturnsAsync(dto);
            }

            [TestCleanup]
            public void Cleanup()
            {
                facultyController = null;
                facultyService = null;
            }

            [TestMethod]
            public async Task GetFacultyPermissions2Async_Success()
            {
                var response = await facultyController.GetFacultyPermissions2Async();
                Assert.IsNotNull(response);
                Assert.IsInstanceOfType(response, typeof(FacultyPermissions));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetFacultyPermissions2Async_Exception()
            {
                ApplicationException thrownEx = new ApplicationException("Error lower in call stack.");
                facultyServiceMock.Setup(x => x.GetFacultyPermissions2Async()).ThrowsAsync(thrownEx);
                var response = await facultyController.GetFacultyPermissions2Async();
                loggerMock.Verify(l => l.Error(thrownEx.ToString()));

            }

        }
    }
}