// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Student;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Course = Ellucian.Colleague.Dtos.Student.Course;
using Course2 = Ellucian.Colleague.Dtos.Student.Course2;
using Section = Ellucian.Colleague.Dtos.Student.Section;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CoursesControllerTests
    {
        [TestClass]
        public class CourseControllerGet
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

            private CoursesController courseController;

            private Mock<ICourseService> courseServiceMock;
            private ICourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();
                courseService = courseServiceMock.Object;
                allCourses = await new TestCourseRepository().GetAsync();
                var coursesList = new List<Course>();

                courseController = new CoursesController(courseService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Course>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                foreach (var course in allCourses)
                {
                    Course target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Course, Course>(course);
                    coursesList.Add(target);
                }
                courseServiceMock.Setup(x => x.GetCourseByIdAsync(It.IsAny<string>())).Returns((string x) => Task.FromResult(coursesList.Where(c => x == c.Id).First()));
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                courseService = null;
            }

            // RCFIX
            //[TestMethod]
            //public void GetSingleId()
            //{
            //    foreach (var course in allCourses)
            //    {
            //        var response = courseController.Get(course.Id);
            //        var responseContent = response.Content.ReadAsStringAsync().Result;
            //        Assert.IsTrue(response is System.Net.Http.HttpResponseMessage);
            //        Assert.AreEqual(course.Id, JsonConvert.DeserializeObject<List<Resources.Course>>(responseContent)[0].Id);
            //    }
            //}


            // RCFIX
            //[TestMethod]
            //public void CourseNotFound()
            //{
            //    var response = courseController.Get("NoCourseWithThisID");
            //    var responseContent = response.Content.ReadAsStringAsync().Result;
            //    Assert.IsTrue(response is System.Net.Http.HttpResponseMessage);
            //    Assert.AreEqual(0, JsonConvert.DeserializeObject<List<Resources.Course>>(responseContent).Count);
            //}

            // RCFIX
            //[TestMethod]
            //public void GetManyIds()
            //{
            //    var idString = "";
            //    for (int i = 0; i < allCourses.Count(); i++)
            //    {
            //        idString += allCourses.ElementAt(i).Id;
            //        if (i + 1 < allCourses.Count())
            //        {
            //            idString += ",";
            //        }
            //    }

            //    var response = courseController.Get(idString);
            //    var responseContent = response.Content.ReadAsStringAsync().Result;
            //    Assert.IsTrue(response is System.Net.Http.HttpResponseMessage);
            //    Assert.AreEqual(allCourses.Count(), JsonConvert.DeserializeObject<List<Resources.Course>>(responseContent).Count);
            //}
        }

        [TestClass]
        public class CourseControllerPostSubject
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

            private CoursesController courseController;

            private Mock<ICourseService> courseServiceMock;
            private ICourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();
                courseService = courseServiceMock.Object;
                allCourses =await new TestCourseRepository().GetAsync();
                var coursesList = new List<Course>();

                courseController = new CoursesController(courseService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Course>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                foreach (var course in allCourses)
                {
                    Course target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Course, Course>(course);
                    coursesList.Add(target);
                }
                var coursePage = new CoursePage();
                courseServiceMock.Setup(svc => svc.SearchAsync(It.IsAny<CourseSearchCriteria>(), 10, 1)).Returns(Task.FromResult(coursePage));
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                courseService = null;
            }

            // RCFIX
            //[TestMethod]
            //public void SubjectSearch()
            //{
            //    var criteria = new CourseSearchCriteria();
            //    criteria.Subjects = new List<string>() { "HIST" };
            //    var response = courseController.PostSearch(criteria, 10, 1);
            //    Assert.IsTrue(response is System.Net.Http.HttpResponseMessage);
            //}
        }

        [TestClass]
        public class CoursesController_GetCourseSectionsAsync
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

            private CoursesController courseController;
            private Mock<ICourseService> courseServiceMock;
            private ICourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private List<Section> sectionDtos;
            private Coordination.Base.PrivacyWrapper<IEnumerable<Section>> privacyWrapper;
            private Mock<ILogger> loggerMock;
            ILogger logger;
            private HttpResponse response;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;

                courseServiceMock = new Mock<ICourseService>();
                courseService = courseServiceMock.Object;
                allSections = await new TestSectionRepository().GetAsync();
                sectionDtos = new List<Section>();

                courseController = new CoursesController(courseService, logger) { Request = new HttpRequestMessage() };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Section, Section>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();

                foreach (var section in allSections)
                {
                    Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(section);
                    sectionDtos.Add(target);
                }

                privacyWrapper = new PrivacyWrapper<IEnumerable<Section>>(sectionDtos, true);
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                courseService = null;
            }

            [TestMethod]
            public async Task GetCourseSectionsAsync_cached_has_privacy_restrictions()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = true;

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                courseServiceMock.Setup(serv => serv.GetSectionsAsync(courseList, true)).ReturnsAsync(privacyWrapper);
                var sectionResults = await courseController.GetCourseSectionsAsync(courseIds);
                Assert.IsNotNull(sectionResults);
                Assert.AreEqual(privacyWrapper.Dto.Count(), sectionResults.Count());
            }

            [TestMethod]
            public async Task GetCourseSectionsAsync_non_cached_no_privacy_restrictions()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = false;

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                courseServiceMock.Setup(serv => serv.GetSectionsAsync(courseList, false)).ReturnsAsync(privacyWrapper);
                var sectionResults = await courseController.GetCourseSectionsAsync(courseIds);
                Assert.IsNotNull(sectionResults);
                Assert.AreEqual(privacyWrapper.Dto.Count(), sectionResults.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetCourseSectionsAsync_generic_exception()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = false;

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                ApplicationException appEx = new ApplicationException("An error occurred.");
                courseServiceMock.Setup(serv => serv.GetSectionsAsync(courseList, false)).ThrowsAsync(appEx);
                var sectionResults = await courseController.GetCourseSectionsAsync(courseIds);
                loggerMock.Verify(l => l.Error(appEx.ToString()));
            }
        }

        [TestClass]
        public class CoursesController_GetCourseSections2Async
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

            private CoursesController courseController;
            private Mock<ICourseService> courseServiceMock;
            private ICourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private List<Dtos.Student.Section2> sectionDtos;
            private Coordination.Base.PrivacyWrapper<IEnumerable<Dtos.Student.Section2>> privacyWrapper;
            private Mock<ILogger> loggerMock;
            ILogger logger;
            private HttpResponse response;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;

                courseServiceMock = new Mock<ICourseService>();
                courseService = courseServiceMock.Object;
                allSections = await new TestSectionRepository().GetAsync();
                sectionDtos = new List<Dtos.Student.Section2>();

                courseController = new CoursesController(courseService, logger) { Request = new HttpRequestMessage() };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section2>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Requisite, Dtos.Student.Requisite>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>();
                foreach (var section in allSections)
                {
                    Dtos.Student.Section2 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section2>(section);
                    sectionDtos.Add(target);
                }

                privacyWrapper = new PrivacyWrapper<IEnumerable<Dtos.Student.Section2>>(sectionDtos, true);
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                courseService = null;
            }

            [TestMethod]
            public async Task GetCourseSections2Async_cached_has_privacy_restrictions()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = true;

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                courseServiceMock.Setup(serv => serv.GetSections2Async(courseList, true)).ReturnsAsync(privacyWrapper);
                var sectionResults = await courseController.GetCourseSections2Async(courseIds);
                Assert.IsNotNull(sectionResults);
                Assert.AreEqual(privacyWrapper.Dto.Count(), sectionResults.Count());
            }

            [TestMethod]
            public async Task GetCourseSections2Async_non_cached_no_privacy_restrictions()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = false;

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                courseServiceMock.Setup(serv => serv.GetSections2Async(courseList, false)).ReturnsAsync(privacyWrapper);
                var sectionResults = await courseController.GetCourseSections2Async(courseIds);
                Assert.IsNotNull(sectionResults);
                Assert.AreEqual(privacyWrapper.Dto.Count(), sectionResults.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetCourseSections2Async_generic_exception()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = false;

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                ApplicationException appEx = new ApplicationException("An error occurred.");
                courseServiceMock.Setup(serv => serv.GetSections2Async(courseList, false)).ThrowsAsync(appEx);
                var sectionResults = await courseController.GetCourseSections2Async(courseIds);
                loggerMock.Verify(l => l.Error(appEx.ToString()));
            }
        }

        [TestClass]
        public class CoursesController_GetCourseSections3Async
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

            private CoursesController courseController;
            private Mock<ICourseService> courseServiceMock;
            private ICourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private List<Dtos.Student.Section3> sectionDtos;
            private Coordination.Base.PrivacyWrapper<IEnumerable<Dtos.Student.Section3>> privacyWrapper;
            private Mock<ILogger> loggerMock;
            ILogger logger;
            private HttpResponse response;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;

                courseServiceMock = new Mock<ICourseService>();
                courseService = courseServiceMock.Object;
                allSections = await new TestSectionRepository().GetAsync();
                sectionDtos = new List<Dtos.Student.Section3>();

                courseController = new CoursesController(courseService, logger) { Request = new HttpRequestMessage() };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting2>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Requisite, Dtos.Student.Requisite>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>();
                foreach (var section in allSections)
                {
                    Dtos.Student.Section3 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>(section);
                    sectionDtos.Add(target);
                }

                privacyWrapper = new PrivacyWrapper<IEnumerable<Dtos.Student.Section3>>(sectionDtos, true);
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                courseService = null;
            }

            [TestMethod]
            public async Task GetCourseSections3Async_cached_has_privacy_restrictions()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = true;

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                courseServiceMock.Setup(serv => serv.GetSections3Async(courseList, true)).ReturnsAsync(privacyWrapper);
                var sectionResults = await courseController.GetCourseSections3Async(courseIds);
                Assert.IsNotNull(sectionResults);
                Assert.AreEqual(privacyWrapper.Dto.Count(), sectionResults.Count());
            }

            [TestMethod]
            public async Task GetCourseSections3Async_non_cached_no_privacy_restrictions()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = false;

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                courseServiceMock.Setup(serv => serv.GetSections3Async(courseList, false)).ReturnsAsync(privacyWrapper);
                var sectionResults = await courseController.GetCourseSections3Async(courseIds);
                Assert.IsNotNull(sectionResults);
                Assert.AreEqual(privacyWrapper.Dto.Count(), sectionResults.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetCourseSections3Async_generic_exception()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                privacyWrapper.HasPrivacyRestrictions = false;

                string courseIds = "100";
                List<string> courseList = new List<string>() { "100" };
                ApplicationException appEx = new ApplicationException("An error occurred.");
                courseServiceMock.Setup(serv => serv.GetSections3Async(courseList, false)).ThrowsAsync(appEx);
                var sectionResults = await courseController.GetCourseSections3Async(courseIds);
                loggerMock.Verify(l => l.Error(appEx.ToString()));
            }
        }

        [TestClass]
        public class CoursesController_GetCourse2
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

            private CoursesController courseController;

            private Mock<ICourseService> courseServiceMock;
            private ICourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();
                courseService = courseServiceMock.Object;
                allCourses = await new TestCourseRepository().GetAsync();
                var coursesList = new List<Course2>();

                courseController = new CoursesController(courseService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.LocationCycleRestriction, LocationCycleRestriction>();
                foreach (var course in allCourses)
                {
                    Course2 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>(course);
                    coursesList.Add(target);
                }
                courseServiceMock.Setup(x => x.GetCourseById2Async(It.IsAny<string>())).Returns((string x) => Task.FromResult(coursesList.Where(c => x == c.Id).First()));
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                courseService = null;
            }

            [TestMethod]
            public async Task GetSingleId()
            {
                foreach (var course in allCourses)
                {
                    var response = await courseController.GetCourse2Async(course.Id);
                    Assert.IsTrue(response is Course2);
                    Assert.AreEqual(course.Id, response.Id);
                }
            }
        }

        [TestClass]
        public class CourseController_PostSearch2
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

            private CoursesController courseController;

            private Mock<ICourseService> courseServiceMock;
            private ICourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();
                courseService = courseServiceMock.Object;
                allCourses = await new TestCourseRepository().GetAsync();
                var coursesList = new List<Course>();

                courseController = new CoursesController(courseService, loggerMock.Object);

                var coursePage = new CoursePage2();
                courseServiceMock.Setup(svc => svc.Search2Async(It.IsAny<CourseSearchCriteria>(), 10, 1)).Returns(Task.FromResult(coursePage));
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                courseService = null;
            }

            [TestMethod]
            public async Task SubjectSearch()
            {
                var criteria = new CourseSearchCriteria();
                criteria.Subjects = new List<string>() { "HIST" };
                var response = await courseController.PostSearch2Async(criteria, 10, 1);
                Assert.IsTrue(response is CoursePage2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RethrowsExceptionAsHttpResponseException()
            {
                courseServiceMock.Setup(x => x.Search2Async(It.IsAny<CourseSearchCriteria>(), 10, 1)).Throws(new Exception());
                var criteria = new CourseSearchCriteria();
                criteria.Subjects = new List<string>() { "HIST" };
                var response = await courseController.PostSearch2Async(criteria, 10, 1);
            }
        }

        [TestClass]
        public class CoursesController_QueryCoursesByPost2
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

            private CoursesController courseController;

            private Mock<ICourseService> courseServiceMock;
            private ICourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private Mock<ILogger> loggerMock;
            List<Course2> coursesList;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();
                courseService = courseServiceMock.Object;
                allCourses = await new TestCourseRepository().GetAsync();
                coursesList = new List<Course2>();

                courseController = new CoursesController(courseService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>();
                foreach (var course in allCourses)
                {
                    Course2 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>(course);
                    coursesList.Add(target);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                courseService = null;
            }

            [TestMethod]
            public async Task ReturnsCourseDtos()
            {
                // arrange
                var badCourseIdList = new List<string>() { "2222222" };
                var goodCourseIdList = new List<string>() { "186", "42", "139" };
                var courseIdList = badCourseIdList;
                courseIdList.AddRange(goodCourseIdList);
                var criteria = new CourseQueryCriteria() { CourseIds = courseIdList };
                courseServiceMock.Setup(x => x.GetCourses2Async(criteria)).Returns(Task.FromResult(coursesList.Where(c => courseIdList.Contains(c.Id))));

                // act
                var response = await courseController.QueryCoursesByPost2Async(criteria);
                
                // assert
                Assert.IsTrue(response is IEnumerable<Course2>);
                Assert.AreEqual(3, response.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RethrowsNullArgumentException()
            {
                // arrange
                var criteria = new CourseQueryCriteria() { CourseIds = new List<string>() };
                courseServiceMock.Setup(x => x.GetCourses2Async(criteria)).Throws(new ArgumentNullException());

                // act
                var response =await courseController.QueryCoursesByPost2Async(criteria);
            }
        }

        [TestClass]
        public class CourseController_GETHedmCoursesAsync
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

            private Mock<ICourseService> courseServiceMock;
            //ICourseService courseService;
            private Mock<ILogger> loggerMock;
            

            private CoursesController courseController;
            private IEnumerable<Domain.Student.Entities.Course> allEntityCourses;
            private List<Ellucian.Colleague.Dtos.Course2> allDtoCourses = new List<Ellucian.Colleague.Dtos.Course2>();

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();

                allEntityCourses = await new TestCourseRepository().GetAsync();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());               

                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course2>();

                foreach(var course in allEntityCourses)
                {
                    Ellucian.Colleague.Dtos.Course2 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course2>(course);
                    allDtoCourses.Add(target);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                allEntityCourses = null;
                allDtoCourses = null;
            }

          
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseController_DeleteHedmCourseByIdAsync()
            {
                await courseController.DeleteHedmCourseByIdAsync("646546");
            }

        }

        [TestClass]
        public class CourseController_GETHedmCoursesV6Async
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

            private Mock<ICourseService> courseServiceMock;
            //ICourseService courseService;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            private IEnumerable<Domain.Student.Entities.Course> allEntityCourses;
            private List<Ellucian.Colleague.Dtos.Course3> allDtoCourses = new List<Ellucian.Colleague.Dtos.Course3>();

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();

                allEntityCourses = await new TestCourseRepository().GetAsync();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course3>();

                foreach (var course in allEntityCourses)
                {
                    Ellucian.Colleague.Dtos.Course3 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course3>(course);
                    allDtoCourses.Add(target);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                allEntityCourses = null;
                allDtoCourses = null;
            }

            [TestMethod]
            public async Task CoursesController_GetHedmCoursesAsync()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                string empty = string.Empty;
                //Arrange
                var courseTuple = new Tuple<IEnumerable<Course3>, int>(allDtoCourses, allDtoCourses.Count());
                courseServiceMock.Setup(c => c.GetCourses3Async(0, 200, It.IsAny<bool>(), empty, empty, empty, empty, empty, empty, empty, empty)).ReturnsAsync(courseTuple);

                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses3Async(new Paging(200, 0), empty, empty, empty, empty, empty, empty, empty, empty);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await courseDtos.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Course3> result = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Course3>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.Course3>;
                //Assert
                Assert.AreEqual(result.Count(), allDtoCourses.Count());

                foreach (var expected in allDtoCourses)
                {
                    var actual = result.FirstOrDefault(i => i.Id.Equals(expected.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expected.AcademicLevels.Count(), actual.AcademicLevels.Count());
                    Assert.AreEqual(expected.CourseLevels.Count(), actual.CourseLevels.Count());
                    Assert.AreEqual(expected.Credits.Count(), actual.Credits.Count());
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.EffectiveEndDate, actual.EffectiveEndDate);
                    Assert.AreEqual(expected.EffectiveStartDate, actual.EffectiveStartDate);
                    Assert.AreEqual(expected.GradeSchemes.Count(), actual.GradeSchemes.Count());
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.InstructionMethods.Count(), actual.InstructionMethods.Count());
                    Assert.AreEqual(expected.Number, actual.Number);
                    Assert.AreEqual(expected.OwningInstitutionUnits.Count(), actual.OwningInstitutionUnits.Count());
                    Assert.AreEqual(expected.Subject.Id, actual.Subject.Id);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseController_GetHedmCourses3Async_Exception()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                string empty = string.Empty;

                //Arrange
                courseServiceMock.Setup(c => c.GetCourses3Async(0, 200, true, empty, empty, empty, empty, empty, empty, empty, empty)).ThrowsAsync(new Exception());
                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses3Async(new Paging(200, 0), empty, empty, empty, empty, empty, empty, empty, empty);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await courseDtos.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Course3> result = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Course3>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.Course3>;
                //Assert
            }

            [TestMethod]
            public async Task CoursesController_GetHedmCourses3ByIdAsync()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                //Arrange
                Dtos.Course3 course = allDtoCourses.FirstOrDefault(i => i.Id == "110");
                courseServiceMock.Setup(c => c.GetCourseByGuid3Async("110")).ReturnsAsync(course);
                //Act
                var result = await courseController.GetHedmCourse3ByIdAsync("110");
                //Assert
                Assert.AreEqual(result.Id, course.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CoursesController_GetHedmCourses3ByIdAsync_Exception()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid3Async(""))
                    .ThrowsAsync(new ArgumentNullException());
                await courseController.GetHedmCourse3ByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses3ByIdAsync_HttpResponseException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid3Async("sdsad"))
                    .ThrowsAsync(new HttpResponseException(System.Net.HttpStatusCode.BadRequest));
                await courseController.GetHedmCourse3ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses3ByIdAsync_PermissionException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid3Async("sdsad"))
                    .ThrowsAsync(new PermissionsException());
                await courseController.GetHedmCourse3ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses3ByIdAsync_KeyNotFoundException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid3Async("sdsad"))
                    .ThrowsAsync(new KeyNotFoundException());
                await courseController.GetHedmCourse3ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses3ByIdAsync_ArgumentException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid3Async("sdsad"))
                    .ThrowsAsync(new ArgumentException());
                await courseController.GetHedmCourse3ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses3ByIdAsync_RepositoryException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid3Async("sdsad"))
                    .ThrowsAsync(new RepositoryException());
                await courseController.GetHedmCourse3ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses3ByIdAsync_IntegrationApiException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid3Async("sdsad"))
                    .ThrowsAsync(new IntegrationApiException());
                await courseController.GetHedmCourse3ByIdAsync("sdsad");
            }
        }

        [TestClass]
        public class CourseController_POSTHedmCoursesAsync
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

            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            private Dtos.Course2 courseDto;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                courseServiceMock = new Mock<ICourseService>();
                loggerMock = new Mock<ILogger>();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object);

                BuildCourse2();
            }

            private void BuildCourse2()
            {
                courseDto = new Dtos.Course2();
                courseDto.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.GradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.CourseLevels = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };

                var creditCategoryOne = new Dtos.CreditIdAndTypeProperty();
                creditCategoryOne.Detail = new GuidObject2() { Id = "1" };

                var creditCategoryTwo = new Dtos.CreditIdAndTypeProperty();
                creditCategoryTwo.Detail = new GuidObject2() { Id = "2" };

                courseDto.Credits = new List<Dtos.Credit2>() 
                                        { new Dtos.Credit2() { CreditCategory = creditCategoryOne }, 
                                          new Dtos.Credit2() { CreditCategory = creditCategoryTwo } 
                                        };
                courseDto.Description = "Description";
                courseDto.EffectiveEndDate = DateTime.Now.AddDays(1);
                courseDto.EffectiveStartDate = DateTime.Now;
                courseDto.InstructionMethods = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.Number = "101";
                courseDto.OwningOrganizations = new List<Dtos.OfferingOrganization2>() 
                                                    { 
                                                        new Dtos.OfferingOrganization2() { Organization = new GuidObject2("1"), Share = 1 },
                                                        new Dtos.OfferingOrganization2() { Organization = new GuidObject2("2"), Share = 1 } 
                                                    };
                courseDto.Subject = new Dtos.GuidObject2("1");
                courseDto.Title = "MATH-101";
                //courseDto.TranscriptGradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseServiceMock = null;
                loggerMock = null; 
                courseController = null;
                //entityCourse = null;
            }

        }

        [TestClass]
        public class CourseController_POSTHedmCourses3Async
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

            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            private Dtos.Course3 courseDto;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                courseServiceMock = new Mock<ICourseService>();
                loggerMock = new Mock<ILogger>();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object);

                BuildCourse3();
            }

            private void BuildCourse3()
            {
                courseDto = new Dtos.Course3();
                courseDto.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.GradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.CourseLevels = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };

                var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
                creditCategoryOne.Detail = new GuidObject2("1");

                var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
                creditCategoryTwo.Detail = new GuidObject2("2");

                courseDto.Credits = new List<Dtos.Credit3>() 
                                        { new Dtos.Credit3() { CreditCategory = creditCategoryOne }, 
                                          new Dtos.Credit3() { CreditCategory = creditCategoryTwo } 
                                        };
                courseDto.Description = "Description";
                courseDto.EffectiveEndDate = DateTime.Now.AddDays(1);
                courseDto.EffectiveStartDate = DateTime.Now;
                courseDto.InstructionMethods = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.Number = "101";
                courseDto.OwningInstitutionUnits = new List<Dtos.OwningInstitutionUnit>() 
                                                    { 
                                                        new Dtos.OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1"), OwnershipPercentage = 1 },
                                                        new Dtos.OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("2"), OwnershipPercentage = 1 } 
                                                    };
                courseDto.Subject = new Dtos.GuidObject2("1");
                courseDto.Title = "MATH-101";
                //courseDto.TranscriptGradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseServiceMock = null;
                loggerMock = null;
                courseController = null;
                //entityCourse = null;
            }

            [TestMethod]
            public async Task CourseController_Post3Async()
            {
                //Arrange
                courseServiceMock.Setup(c => c.CreateCourse3Async(courseDto)).ReturnsAsync(courseDto);
                //Act
                courseDto.Id = Guid.Empty.ToString();
                var result = await courseController.PostCourse3Async(courseDto);
                //Assert
                Assert.AreEqual(courseDto.AcademicLevels, result.AcademicLevels);
                Assert.AreEqual(courseDto.GradeSchemes, result.GradeSchemes);
                Assert.AreEqual(courseDto.CourseLevels.Count, result.CourseLevels.Count);
                Assert.AreEqual(courseDto.Credits.Count, result.Credits.Count);
                Assert.AreEqual(courseDto.Description, result.Description);
                Assert.AreEqual(courseDto.EffectiveEndDate, result.EffectiveEndDate);
                Assert.AreEqual(courseDto.EffectiveStartDate, result.EffectiveStartDate);
                Assert.AreEqual(courseDto.InstructionMethods.Count, result.InstructionMethods.Count);
                Assert.AreEqual(courseDto.Number, result.Number);
                Assert.AreEqual(courseDto.OwningInstitutionUnits.Count, result.OwningInstitutionUnits.Count);
                Assert.AreEqual(courseDto.Subject, result.Subject);
                Assert.AreEqual(courseDto.Title, result.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post3Async_BadRequest()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse3Async(courseDto))
                    .ThrowsAsync(new InvalidOperationException());
                await courseController.PostCourse3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post3Async_ID_Null()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse3Async(courseDto))
                    .ThrowsAsync(new InvalidOperationException());
                await courseController.PostCourse3Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post3Async_PermissionsException()
            {
                courseDto.Id = "1";
                courseServiceMock
                    .Setup(c => c.CreateCourse3Async(courseDto))
                    .ThrowsAsync(new PermissionsException());
                await courseController.PostCourse3Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post3Async_ConfigurationException()
            {
                courseDto.Id = "1";
                courseServiceMock
                    .Setup(c => c.CreateCourse3Async(courseDto))
                    .ThrowsAsync(new ConfigurationException());
                await courseController.PostCourse3Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post3Async_InvalidOperationException()
            {
                courseDto.Id = "1";
                courseServiceMock
                    .Setup(c => c.CreateCourse3Async(courseDto))
                    .ThrowsAsync(new InvalidOperationException());
                await courseController.PostCourse3Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post2Async_ArgumentNullException()
            {
                courseDto.Id = "1";
                courseServiceMock
                    .Setup(c => c.CreateCourse3Async(courseDto))
                    .ThrowsAsync(new Exception());
                await courseController.PostCourse3Async(courseDto);
            }
        }

        [TestClass]
        public class CourseController_PUTHedmCoursesAsync
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

            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            //private Domain.Student.Entities.Course entityCourse;
            private Dtos.Course2 courseDto;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                courseServiceMock = new Mock<ICourseService>();
                loggerMock = new Mock<ILogger>();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object);

                BuildCourse2();
            }

            private void BuildCourse2()
            {
                courseDto = new Dtos.Course2();
                courseDto.Id = "1";
                courseDto.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.GradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.CourseLevels = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };

                var creditCategoryOne = new Dtos.CreditIdAndTypeProperty();
                creditCategoryOne.Detail = new GuidObject2() { Id = "1" };

                var creditCategoryTwo = new Dtos.CreditIdAndTypeProperty();
                creditCategoryTwo.Detail = new GuidObject2() { Id = "2" };

                courseDto.Credits = new List<Dtos.Credit2>() 
                                        { new Dtos.Credit2() { CreditCategory = creditCategoryOne }, 
                                          new Dtos.Credit2() { CreditCategory = creditCategoryTwo } 
                                        };
                courseDto.Description = "Description";
                courseDto.EffectiveEndDate = DateTime.Now.AddDays(1);
                courseDto.EffectiveStartDate = DateTime.Now;
                courseDto.InstructionMethods = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.Number = "101";
                courseDto.OwningOrganizations = new List<Dtos.OfferingOrganization2>() 
                                                    { 
                                                        new Dtos.OfferingOrganization2() { Organization = new GuidObject2("1"), Share = 1 },
                                                        new Dtos.OfferingOrganization2() { Organization = new GuidObject2("2"), Share = 1 } 
                                                    };
                courseDto.Subject = new Dtos.GuidObject2("1");
                courseDto.Title = "MATH-101";
                //courseDto.TranscriptGradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseServiceMock = null;
                loggerMock = null;
                courseController = null;
                //entityCourse = null;
            }
                       
        }

        [TestClass]
        public class CourseController_PUTHedmCourses3Async
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

            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            //private Domain.Student.Entities.Course entityCourse;
            private Dtos.Course3 courseDto;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                courseServiceMock = new Mock<ICourseService>();
                loggerMock = new Mock<ILogger>();

                BuildCourse2();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                courseController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(courseDto));

            }

            private void BuildCourse2()
            {
                courseDto = new Dtos.Course3();
                courseDto.Id = "1";
                courseDto.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.GradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.CourseLevels = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };

                var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { Detail = new GuidObject2("1"), CreditType = Ellucian.Colleague.Dtos.EnumProperties.CreditCategoryType3.ContinuingEducation };

                var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { Detail = new GuidObject2("2"), CreditType = Ellucian.Colleague.Dtos.EnumProperties.CreditCategoryType3.Exchange };

                courseDto.Credits = new List<Dtos.Credit3>() 
                                        { new Dtos.Credit3() { CreditCategory = creditCategoryOne }, 
                                          new Dtos.Credit3() { CreditCategory = creditCategoryTwo } 
                                        };
                courseDto.Description = "Description";
                courseDto.EffectiveEndDate = DateTime.Now.AddDays(1);
                courseDto.EffectiveStartDate = DateTime.Now;
                courseDto.InstructionMethods = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.Number = "101";
                courseDto.OwningInstitutionUnits = new List<Dtos.OwningInstitutionUnit>() 
                                                    { 
                                                        new Dtos.OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1"), OwnershipPercentage = 1 },
                                                        new Dtos.OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("2"), OwnershipPercentage = 1 } 
                                                    };
                courseDto.Subject = new Dtos.GuidObject2("1");
                courseDto.Title = "MATH-101";
                //courseDto.TranscriptGradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseServiceMock = null;
                loggerMock = null;
                courseController = null;
                //entityCourse = null;
            }

            [TestMethod]
            public async Task CourseController_PUT2Async()
            {
                //Arrange
                courseServiceMock.Setup(c => c.UpdateCourse3Async(It.IsAny<Course3>())).ReturnsAsync(courseDto);
                courseServiceMock.Setup(c => c.GetCourseByGuid3Async(courseDto.Id)).ReturnsAsync(courseDto);
                //Act
                var result = await courseController.PutCourse3Async("1", courseDto);
                //Assert
                Assert.AreEqual(courseDto.AcademicLevels, result.AcademicLevels);
                Assert.AreEqual(courseDto.GradeSchemes, result.GradeSchemes);
                Assert.AreEqual(courseDto.CourseLevels.Count, result.CourseLevels.Count);
                Assert.AreEqual(courseDto.Credits.Count, result.Credits.Count);
                Assert.AreEqual(courseDto.Description, result.Description);
                Assert.AreEqual(courseDto.EffectiveEndDate, result.EffectiveEndDate);
                Assert.AreEqual(courseDto.EffectiveStartDate, result.EffectiveStartDate);
                Assert.AreEqual(courseDto.InstructionMethods.Count, result.InstructionMethods.Count);
                Assert.AreEqual(courseDto.Number, result.Number);
                Assert.AreEqual(courseDto.OwningInstitutionUnits.Count, result.OwningInstitutionUnits.Count);
                Assert.AreEqual(courseDto.Subject, result.Subject);
                Assert.AreEqual(courseDto.Title, result.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_PermissionsException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new PermissionsException());
                await courseController.PutCourse3Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_ArgumentNullException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new ArgumentNullException());
                await courseController.PutCourse3Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_ArgumentException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new ArgumentException());
                await courseController.PutCourse3Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_ApplicationException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new ApplicationException());
                await courseController.PutCourse3Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_ConfigurationException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new ConfigurationException());
                await courseController.PutCourse3Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_Exception()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse3Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_IdNull()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse3Async("", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_CourseNull()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse3Async("1", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_IdEqualsEmptyGuid()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse3Async(Guid.Empty.ToString(), courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_CourseObjectIdNotEqualsToId()
            {
                courseDto.Id = "2";
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse3Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT3Async_CourseObjectIdNull()
            {
                courseDto.Id = string.Empty;
                courseServiceMock
                    .Setup(c => c.UpdateCourse3Async(courseDto))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse3Async("1", courseDto);
            }


            //PUT V6
            //Successful
            //PutCourse3Async

            [TestMethod]
            public async Task CoursesController_PutCourse3Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Courses" },
                    { "action", "PutCourse3Async" }
                };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateCourse);

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                courseServiceMock.Setup(c => c.UpdateCourse3Async(It.IsAny<Course3>())).ReturnsAsync(courseDto);
                var result = await courseController.PutCourse3Async("1", courseDto);

                Object filterObject;
                courseController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateCourse));

            }

            //Put V6
            //Exception
            //PutCourse3Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse3Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Courses" },
                    { "action", "PutCourse3Async" }
                };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    courseServiceMock.Setup(c => c.UpdateCourse3Async(courseDto)).ThrowsAsync(new PermissionsException()); 
                    courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update courses."));
                    await courseController.PutCourse3Async("1", courseDto);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            //POST V6
            //Successful
            //PostCourse3Async

            [TestMethod]
            public async Task CoursesController_PostCourse3Async_Permissions()
            {
                courseDto.Id = Guid.Empty.ToString();
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Courses" },
                        { "action", "PostCourse3Async" }
                    };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateCourse);

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                courseServiceMock.Setup(c => c.CreateCourse3Async(courseDto)).ReturnsAsync(courseDto);
                var result = await courseController.PostCourse3Async(courseDto);

                Object filterObject;
                courseController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateCourse));

            }

            //POST V6
            //Exception
            //PostCourse3Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse3Async_Invalid_Permissions()
            {
                courseDto.Id = "1";
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Courses" },
                        { "action", "PostCourse3Async" }
                    };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    courseServiceMock.Setup(c => c.CreateCourse3Async(courseDto)).ThrowsAsync(new PermissionsException());
                    courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create courses."));
                    await courseController.PostCourse3Async(courseDto);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }


        #region Courses V8 methods
        [TestClass]
        public class CourseController_GETHedmCoursesV8Async
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

            private Mock<ICourseService> courseServiceMock;
            //ICourseService courseService;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            private IEnumerable<Domain.Student.Entities.Course> allEntityCourses;
            private List<Ellucian.Colleague.Dtos.Course4> allDtoCourses = new List<Ellucian.Colleague.Dtos.Course4>();

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();

                allEntityCourses = await new TestCourseRepository().GetAsync();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course4>();

                foreach (var course in allEntityCourses)
                {
                    Ellucian.Colleague.Dtos.Course4 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course4>(course);
                    allDtoCourses.Add(target);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                allEntityCourses = null;
                allDtoCourses = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseController_GetHedmCourses4Async_Exception()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                string empty = string.Empty;

                //Arrange
                courseServiceMock.Setup(c => c.GetCourses4Async(0, 200, true, empty, empty, null, null, empty, null, empty, empty)).ThrowsAsync(new Exception());
                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses4Async(new Paging(200, 0), null);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await courseDtos.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Course4> result = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Course4>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.Course4>;
                //Assert
            }

            [TestMethod]
            public async Task CoursesController_GetHedmCourses4ByIdAsync()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                //Arrange
                Dtos.Course4 course = allDtoCourses.FirstOrDefault(i => i.Id == "110");
                courseServiceMock.Setup(c => c.GetCourseByGuid4Async("110")).ReturnsAsync(course);
                //Act
                var result = await courseController.GetHedmCourse4ByIdAsync("110");
                //Assert
                Assert.AreEqual(result.Id, course.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CoursesController_GetHedmCourses4ByIdAsync_Exception()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid4Async(""))
                    .ThrowsAsync(new ArgumentNullException());
                await courseController.GetHedmCourse4ByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses4ByIdAsync_HttpResponseException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid4Async("sdsad"))
                    .ThrowsAsync(new HttpResponseException(System.Net.HttpStatusCode.BadRequest));
                await courseController.GetHedmCourse4ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses4ByIdAsync_PermissionsException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid4Async("sdsad"))
                    .ThrowsAsync(new PermissionsException());
                await courseController.GetHedmCourse4ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses4ByIdAsync_KeyNotFoundException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid4Async("sdsad"))
                    .ThrowsAsync(new KeyNotFoundException());
                await courseController.GetHedmCourse4ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses4ByIdAsync_ArgumentException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid4Async("sdsad"))
                    .ThrowsAsync(new ArgumentException());
                await courseController.GetHedmCourse4ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses4ByIdAsync_RepositoryException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid4Async("sdsad"))
                    .ThrowsAsync(new RepositoryException());
                await courseController.GetHedmCourse4ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses4ByIdAsync_IntegrationApiException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid4Async("sdsad"))
                    .ThrowsAsync(new IntegrationApiException());
                await courseController.GetHedmCourse4ByIdAsync("sdsad");
            }
        }

        [TestClass]
        public class CourseController_POSTHedmCoursesAsync_V8
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

            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            private Dtos.Course4 courseDto;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                courseServiceMock = new Mock<ICourseService>();
                loggerMock = new Mock<ILogger>();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


                BuildCourse4();
            }

            private void BuildCourse4()
            {
                courseDto = new Dtos.Course4();
                courseDto.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.GradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.CourseLevels = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };

                var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
                creditCategoryOne.Detail = new GuidObject2("1");

                var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
                creditCategoryTwo.Detail = new GuidObject2("2");

                courseDto.Credits = new List<Dtos.Credit3>() 
                                        { new Dtos.Credit3() { CreditCategory = creditCategoryOne }, 
                                          new Dtos.Credit3() { CreditCategory = creditCategoryTwo } 
                                        };

                courseDto.Description = "Description";
                courseDto.EffectiveEndDate = DateTime.Now.AddDays(1);
                courseDto.EffectiveStartDate = DateTime.Now;
                courseDto.InstructionMethods = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.Number = "101";
                courseDto.OwningInstitutionUnits = new List<Dtos.OwningInstitutionUnit>() 
                                                    { 
                                                        new Dtos.OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1"), OwnershipPercentage = 1 },
                                                        new Dtos.OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("2"), OwnershipPercentage = 1 } 
                                                    };
                courseDto.Subject = new Dtos.GuidObject2("1");
                courseDto.Title = "MATH-101";
                courseDto.Billing = new Dtos.DtoProperties.BillingCreditDtoProperty() {Minimum = 2};
                

                //courseDto.TranscriptGradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseServiceMock = null;
                loggerMock = null;
                courseController = null;
                //entityCourse = null;
            }

            [TestMethod]
            public async Task CourseController_Post4Async()
            {
                //Arrange
                courseServiceMock.Setup(c => c.CreateCourse4Async(courseDto, false)).ReturnsAsync(courseDto);
                //Act
                courseDto.Id = Guid.Empty.ToString();
                var result = await courseController.PostCourse4Async(courseDto);
                //Assert
                Assert.AreEqual(courseDto.AcademicLevels, result.AcademicLevels);
                Assert.AreEqual(courseDto.GradeSchemes, result.GradeSchemes);
                Assert.AreEqual(courseDto.CourseLevels.Count, result.CourseLevels.Count);
                Assert.AreEqual(courseDto.Credits.Count, result.Credits.Count);
                Assert.AreEqual(courseDto.Description, result.Description);
                Assert.AreEqual(courseDto.EffectiveEndDate, result.EffectiveEndDate);
                Assert.AreEqual(courseDto.EffectiveStartDate, result.EffectiveStartDate);
                Assert.AreEqual(courseDto.InstructionMethods.Count, result.InstructionMethods.Count);
                Assert.AreEqual(courseDto.Number, result.Number);
                Assert.AreEqual(courseDto.OwningInstitutionUnits.Count, result.OwningInstitutionUnits.Count);
                Assert.AreEqual(courseDto.Subject, result.Subject);
                Assert.AreEqual(courseDto.Title, result.Title);
                Assert.AreEqual(courseDto.Billing.Minimum, result.Billing.Minimum);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post4Async_InvalidOperationException()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse4Async(courseDto, false))
                    .ThrowsAsync(new InvalidOperationException());
                await courseController.PostCourse4Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post4Async_ID_Null()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse4Async(courseDto, false))
                    .ThrowsAsync(new InvalidOperationException());
                await courseController.PostCourse4Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post4Async_PermissionsException()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse4Async(courseDto, false))
                    .ThrowsAsync(new PermissionsException());
                await courseController.PostCourse4Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post4Async_ConfigurationException()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse4Async(courseDto, false))
                    .ThrowsAsync(new ConfigurationException());
                await courseController.PostCourse4Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post4Async_HttpResponseException()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse4Async(courseDto, false))
                    .ThrowsAsync(new HttpResponseException(System.Net.HttpStatusCode.BadRequest));
                loggerMock.Setup(l => l.Info(string.Empty));
                loggerMock.Setup(l => l.Error(string.Empty));
                await courseController.PostCourse4Async(courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_Post4Async_ArgumentNullException()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse4Async(null, false))
                    .ThrowsAsync(new ArgumentNullException());
                await courseController.PostCourse4Async(null);
            }

            //POST V8
            //Successful
            //PostCourse4Async

            [TestMethod]
            public async Task CoursesController_PostCourse4Async_Permissions()
            {
                courseDto.Id = Guid.Empty.ToString();
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Courses" },
                        { "action", "PostCourse4Async" }
                    };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateCourse);

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                courseServiceMock.Setup(c => c.CreateCourse4Async(courseDto, false)).ReturnsAsync(courseDto);
                var result = await courseController.PostCourse4Async(courseDto);

                Object filterObject;
                courseController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateCourse));

            }

            //POST V8
            //Exception
            //PostCourse4Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse4Async_Invalid_Permissions()
            {
                courseDto.Id = "1";
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Courses" },
                        { "action", "PostCourse4Async" }
                    };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    courseServiceMock.Setup(c => c.CreateCourse4Async(courseDto, It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create courses."));
                    await courseController.PostCourse4Async(courseDto);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }


        [TestClass]
        public class CourseController_PUTHedmCourses4Async
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

            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            //private Domain.Student.Entities.Course entityCourse;
            private Dtos.Course4 courseDto;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                courseServiceMock = new Mock<ICourseService>();
                loggerMock = new Mock<ILogger>();

                BuildCourse4();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                courseController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(courseDto));
            }

            private void BuildCourse4()
            {
                courseDto = new Dtos.Course4();
                courseDto.Id = "1";
                courseDto.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.GradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.CourseLevels = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };

                var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { Detail = new GuidObject2("1"), CreditType = Ellucian.Colleague.Dtos.EnumProperties.CreditCategoryType3.ContinuingEducation };

                var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2() { Detail = new GuidObject2("2"), CreditType = Ellucian.Colleague.Dtos.EnumProperties.CreditCategoryType3.Exchange };

                courseDto.Credits = new List<Dtos.Credit3>() 
                                        { new Dtos.Credit3() { CreditCategory = creditCategoryOne }, 
                                          new Dtos.Credit3() { CreditCategory = creditCategoryTwo } 
                                        };
                courseDto.Description = "Description";
                courseDto.EffectiveEndDate = DateTime.Now.AddDays(1);
                courseDto.EffectiveStartDate = DateTime.Now;
                courseDto.InstructionMethods = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
                courseDto.Number = "101";
                courseDto.OwningInstitutionUnits = new List<Dtos.OwningInstitutionUnit>() 
                                                    { 
                                                        new Dtos.OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1"), OwnershipPercentage = 1 },
                                                        new Dtos.OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("2"), OwnershipPercentage = 1 } 
                                                    };
                courseDto.Subject = new Dtos.GuidObject2("1");
                courseDto.Title = "MATH-101";
                courseDto.Billing = new Dtos.DtoProperties.BillingCreditDtoProperty() { Minimum = 2 };

                //courseDto.TranscriptGradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2("1"), new Dtos.GuidObject2("2") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseServiceMock = null;
                loggerMock = null;
                courseController = null;
                //entityCourse = null;
            }

            [TestMethod]
            public async Task CourseController_PUT4Async()
            {
                //Arrange
                courseServiceMock.Setup(c => c.UpdateCourse4Async(It.IsAny<Course4>(), It.IsAny<bool>())).ReturnsAsync(courseDto);
                courseServiceMock.Setup(c => c.GetCourseByGuid4Async(courseDto.Id)).ReturnsAsync(courseDto);
                //Act
                var result = await courseController.PutCourse4Async("1", courseDto);
                //Assert
                Assert.AreEqual(courseDto.AcademicLevels, result.AcademicLevels);
                Assert.AreEqual(courseDto.GradeSchemes, result.GradeSchemes);
                Assert.AreEqual(courseDto.CourseLevels.Count, result.CourseLevels.Count);
                Assert.AreEqual(courseDto.Credits.Count, result.Credits.Count);
                Assert.AreEqual(courseDto.Description, result.Description);
                Assert.AreEqual(courseDto.EffectiveEndDate, result.EffectiveEndDate);
                Assert.AreEqual(courseDto.EffectiveStartDate, result.EffectiveStartDate);
                Assert.AreEqual(courseDto.InstructionMethods.Count, result.InstructionMethods.Count);
                Assert.AreEqual(courseDto.Number, result.Number);
                Assert.AreEqual(courseDto.OwningInstitutionUnits.Count, result.OwningInstitutionUnits.Count);
                Assert.AreEqual(courseDto.Subject, result.Subject);
                Assert.AreEqual(courseDto.Title, result.Title);
                Assert.AreEqual(courseDto.Billing.Minimum, result.Billing.Minimum);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_PermissionsException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new PermissionsException());
                await courseController.PutCourse4Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_ArgumentNullException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new ArgumentNullException());
                await courseController.PutCourse4Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_ArgumentException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new ArgumentException());
                await courseController.PutCourse4Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_ApplicationException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new ApplicationException());
                await courseController.PutCourse4Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_ConfigurationException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new ConfigurationException());
                await courseController.PutCourse4Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_Exception()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse4Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_IdNull()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse4Async("", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_CourseNull()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse4Async("1", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_IdEqualsEmptyGuid()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse4Async(Guid.Empty.ToString(), courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_CourseObjectIdNotEqualsToId()
            {
                courseDto.Id = "2";
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse4Async("1", courseDto);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PUT4Async_CourseObjectIdNull()
            {
                courseDto.Id = string.Empty;
                courseServiceMock
                    .Setup(c => c.UpdateCourse4Async(courseDto, false))
                    .ThrowsAsync(new Exception());
                await courseController.PutCourse4Async("1", courseDto);
            }

            //PUT V8
            //Successful
            //PutCourse4Async

            [TestMethod]
            public async Task CoursesController_PutCourse4Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Courses" },
                    { "action", "PutCourse4Async" }
                };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateCourse);

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                courseServiceMock.Setup(c => c.UpdateCourse4Async(It.IsAny<Course4>(), It.IsAny<bool>())).ReturnsAsync(courseDto);
                var result = await courseController.PutCourse4Async("1", courseDto);

                Object filterObject;
                courseController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateCourse));

            }

            //Put V8
            //Exception
            //PutCourse4Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse4Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Courses" },
                    { "action", "PutCourse4Async" }
                };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    courseServiceMock.Setup(c => c.UpdateCourse4Async(courseDto, false)).ThrowsAsync(new PermissionsException()); 
                    courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update courses."));
                    await courseController.PutCourse4Async("1", courseDto);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }
        #endregion


        #region Courses V20

        [TestClass]
        public class CourseController_GETHedmCoursesV20Async
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

            private Mock<ICourseService> courseServiceMock;
            //ICourseService courseService;
            private Mock<ILogger> loggerMock;


            private CoursesController courseController;
            private IEnumerable<Domain.Student.Entities.Course> allEntityCourses;
            private List<Ellucian.Colleague.Dtos.Course5> allDtoCourses = new List<Ellucian.Colleague.Dtos.Course5>();
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            private Ellucian.Web.Http.Models.QueryStringFilter activeOnFilter = new Web.Http.Models.QueryStringFilter("activeOn", "");

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();

                allEntityCourses = await new TestCourseRepository().GetAsync();

                courseController = new CoursesController(courseServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                courseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                //Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course5>();

                foreach (var course in allEntityCourses)
                {
                    Ellucian.Colleague.Dtos.Course5 target = new Course5()
                    {
                        Id = course.Id,
                        AcademicLevels = new List<GuidObject2>() { new GuidObject2("1") },
                        Categories = new List<GuidObject2>() { new GuidObject2("1") },
                        EffectiveEndDate = course.EndDate,
                        EffectiveStartDate = course.StartDate.HasValue ? course.StartDate.Value : default(DateTime),
                        InstructionalMethodDetails = new List<InstructionalMethodDetail>() { new InstructionalMethodDetail() { InstructionalMethod = new GuidObject2("1") } },
                        OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1") } },
                        Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>() { new Dtos.DtoProperties.CoursesTitlesDtoProperty() { Value = "1" } }
                    };
                    allDtoCourses.Add(target);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseController = null;
                allEntityCourses = null;
                allDtoCourses = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseController_GetHedmCourses5Async_Exception()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                string empty = string.Empty;

                //Arrange
                courseServiceMock.Setup(c => c.GetCourses5Async(0, 200, true, empty, empty, null, null, null, null, empty, empty, empty, null, null)).ThrowsAsync(new Exception());
                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses5Async(new Paging(200, 0), null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseController_GetHedmCourses5Async_KeyNotFoundException()
            {
                string empty = string.Empty;
                List<string> emptyList = new List<string>();

                //Arrange
                courseServiceMock.Setup(c => c.GetCourses5Async(0, 200, false, empty, empty, emptyList, emptyList, emptyList, emptyList, empty, empty, empty, emptyList, empty)).ThrowsAsync(new KeyNotFoundException());
                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses5Async(new Paging(200, 0), null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseController_GetHedmCourses5Async_PermissionsException()
            {
                string empty = string.Empty;
                List<string> emptyList = new List<string>();

                //Arrange
                courseServiceMock.Setup(c => c.GetCourses5Async(0, 200, false, empty, empty, emptyList, emptyList, emptyList, emptyList, empty, empty, empty, emptyList, empty)).ThrowsAsync(new PermissionsException());
                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses5Async(It.IsAny<Paging>(), criteriaFilter, activeOnFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseController_GetHedmCourses5Async_ArgumentNullException()
            {
                string empty = string.Empty;
                List<string> emptyList = new List<string>();

                //Arrange
                courseServiceMock.Setup(c => c.GetCourses5Async(0, 200, false, empty, empty, emptyList, emptyList, emptyList, emptyList, empty, empty, empty, emptyList, empty)).ThrowsAsync(new ArgumentNullException());
                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses5Async(new Paging(200, 0), null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseController_GetHedmCourses5Async_RepositoryException()
            {
                string empty = string.Empty;
                List<string> emptyList = new List<string>();

                //Arrange
                courseServiceMock.Setup(c => c.GetCourses5Async(0, 200, false, empty, empty, emptyList, emptyList, emptyList, emptyList, empty, empty, empty, emptyList, empty)).ThrowsAsync(new RepositoryException());
                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses5Async(new Paging(200, 0), null, null);
            }

            [TestMethod]
            public async Task CourseController_GetHedmCourses5Async()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                string empty = string.Empty;
                var listCriteria = new List<string>() { "1" };
                var filterGroupName = "criteria";
                var filterRecord = allDtoCourses.FirstOrDefault(i => i.Id.Equals("110"));

                courseController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName), filterRecord);

                Tuple<IEnumerable<Dtos.Course5>, int> tuple = new Tuple<IEnumerable<Course5>, int>(allDtoCourses, allDtoCourses.Count());
                //Arrange
                courseServiceMock.Setup(c => c.GetCourses5Async(It.IsAny<int>(), It.IsAny<int>(), true, empty, empty, listCriteria, listCriteria, listCriteria, listCriteria, "1/1/2001", empty, empty, listCriteria, empty))
                    .ReturnsAsync(tuple);
                //Act
                var courseDtos = await courseController.GetAllAndFilteredCourses5Async(It.IsAny<Paging>(), criteriaFilter, activeOnFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await courseDtos.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Course5> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Course5>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.Course5>;
                //Assert
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task CoursesController_GetHedmCourses5ByIdAsync()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                //Arrange
                Dtos.Course5 course = allDtoCourses.FirstOrDefault(i => i.Id == "110");
                courseServiceMock.Setup(c => c.GetCourseByGuid5Async(It.IsAny<string>())).ReturnsAsync(course);
                //Act
                var result = await courseController.GetHedmCourse5ByIdAsync("110");
                //Assert
                Assert.AreEqual(result.Id, course.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CoursesController_GetHedmCourses5ByIdAsync_Exception()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid5Async(""))
                    .ThrowsAsync(new ArgumentNullException());
                await courseController.GetHedmCourse5ByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses5ByIdAsync_HttpResponseException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid5Async("sdsad"))
                    .ThrowsAsync(new HttpResponseException(System.Net.HttpStatusCode.BadRequest));
                await courseController.GetHedmCourse5ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses4ByIdAsync_PermissionsException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid5Async("sdsad"))
                    .ThrowsAsync(new PermissionsException());
                await courseController.GetHedmCourse5ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses5ByIdAsync_KeyNotFoundException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid5Async("sdsad"))
                    .ThrowsAsync(new KeyNotFoundException());
                await courseController.GetHedmCourse5ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses5ByIdAsync_ArgumentException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid5Async("sdsad"))
                    .ThrowsAsync(new ArgumentException());
                await courseController.GetHedmCourse5ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses5ByIdAsync_RepositoryException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid5Async("sdsad"))
                    .ThrowsAsync(new RepositoryException());
                await courseController.GetHedmCourse5ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_GetHedmCourses5ByIdAsync_IntegrationApiException()
            {
                courseServiceMock
                    .Setup(c => c.GetCourseByGuid5Async("sdsad"))
                    .ThrowsAsync(new IntegrationApiException());
                await courseController.GetHedmCourse5ByIdAsync("sdsad");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse5Async_InvalidOperationException()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new InvalidOperationException());
                await courseController.PostCourse5Async(new Course5() { Id = Guid.Empty.ToString() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse5Async_PermissionsException()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await courseController.PostCourse5Async(new Course5() { Id = Guid.Empty.ToString() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse5Async_ConfigurationException()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ConfigurationException());
                await courseController.PostCourse5Async(new Course5() { Id = Guid.Empty.ToString() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse5Async_Exception()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await courseController.PostCourse5Async(new Course5() { Id = Guid.Empty.ToString() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse5Async_CourseNull()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                await courseController.PostCourse5Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse5Async_IdNull()
            {
                await courseController.PostCourse5Async(new Course5() { Id = string.Empty });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse5Async_Id_Not_EmptyGuid()
            {
                await courseController.PostCourse5Async(new Course5() { Id = Guid.NewGuid().ToString() });
            }

            [TestMethod]
            public async Task CoursesController_PostCourse5Async()
            {
                courseServiceMock
                    .Setup(c => c.CreateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>())).ReturnsAsync(allDtoCourses.First());
                var result = await courseController.PostCourse5Async(new Course5() { Id = Guid.Empty.ToString() });
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_InvalidOperationException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new InvalidOperationException());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_PermissionsException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_ArgumentNullException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_ArgumentException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentException());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_ApplicationException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ApplicationException());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_ConfigurationException()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ConfigurationException());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_Exception()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_CourseNull()
            {
                courseController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                await courseController.PutCourse5Async("1", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_IdNull()
            {
                await courseController.PutCourse5Async("", new Course5() { Id = Guid.NewGuid().ToString() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_Id_Not_Guid()
            {
                await courseController.PutCourse5Async("1", new Course5() { Id = Guid.NewGuid().ToString() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_Id_IsEmpty_Guid()
            {
                await courseController.PutCourse5Async(Guid.Empty.ToString(), new Course5() { Id = Guid.Empty.ToString() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_Ids_NotSame()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>())).ReturnsAsync(allDtoCourses.First());
                var result = await courseController.PutCourse5Async("1", new Course5() { Id = Guid.NewGuid().ToString() });
            }

            [TestMethod]            
            public async Task CoursesController_PutCourse5Async_Ids()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>())).ReturnsAsync(allDtoCourses.First());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task CoursesController_PutCourse5Async_Course_NullId()
            {
                courseServiceMock
                    .Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>())).ReturnsAsync(allDtoCourses.First());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "" });
                Assert.IsNotNull(result);
            }

            //PUT V16.1.0
            //Successful
            //PutCourse5Async

            [TestMethod]
            public async Task CoursesController_PutCourse5Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Courses" },
                    { "action", "PutCourse5Async" }
                };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateCourse);

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                courseServiceMock.Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>())).ReturnsAsync(allDtoCourses.First());
                var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });

                Object filterObject;
                courseController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateCourse));

            }

            //Put V16.1.0
            //Exception
            //PutCourse5Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PutCourse5Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "Courses" },
                    { "action", "PutCourse5Async" }
                };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    courseServiceMock.Setup(c => c.UpdateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update courses."));
                    var result = await courseController.PutCourse5Async("139", new Course5() { Id = "139" });
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            //POST V16.1.0
            //Successful
            //PostCourse5Async

            [TestMethod]
            public async Task CoursesController_PostCourse5Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Courses" },
                        { "action", "PostCourse5Async" }
                    };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);
                courseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateCourse);

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                courseServiceMock.Setup(c => c.CreateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>())).ReturnsAsync(allDtoCourses.First());
                var result = await courseController.PostCourse5Async(new Course5() { Id = Guid.Empty.ToString() });

                Object filterObject;
                courseController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateCourse));

            }

            //POST V16.1.0
            //Exception
            //PostCourse5Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CoursesController_PostCourse5Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Courses" },
                        { "action", "PostCourse5Async" }
                    };
                HttpRoute route = new HttpRoute("courses", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                courseController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = courseController.ControllerContext;
                var actionDescriptor = courseController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    courseServiceMock.Setup(c => c.CreateCourse5Async(It.IsAny<Course5>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    courseServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create courses."));
                    await courseController.PostCourse5Async(new Course5() { Id = Guid.Empty.ToString() });
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }


        #endregion
    }
}