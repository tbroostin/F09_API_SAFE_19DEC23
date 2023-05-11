// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Section = Ellucian.Colleague.Dtos.Student.Section;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SectionsControllerTests
    {
        [TestClass]
        public class SectionsController_GetSectionRosterAsync_Tests
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            private List<RosterStudent> rosterStudents;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                rosterStudents = new List<RosterStudent>()
                {
                    new RosterStudent()
                    {
                        Id = "0001234",
                        FirstName = "John",
                        LastName = "Smith"
                    },
                    new RosterStudent()
                    {
                        Id = "0005678",
                        FirstName = "Jane",
                        LastName = "Doe"
                    }
                };

                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionRosterAsync_ArgumentNullException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionRosterAsync(It.IsAny<string>()))
                    .Throws(new ArgumentNullException());
                await sectionsController.GetSectionRosterAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionRosterAsync_ApplicationException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionRosterAsync(It.IsAny<string>()))
                    .Throws(new ApplicationException());
                await sectionsController.GetSectionRosterAsync("100");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionRosterAsync_Exception()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionRosterAsync(It.IsAny<string>()))
                    .Throws(new Exception());
                await sectionsController.GetSectionRosterAsync("100");
            }

            [TestMethod]
            public async Task SectionsController_GetSectionRosterAsync_Valid()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionRosterAsync(It.IsAny<string>()))
                    .ReturnsAsync(rosterStudents);
                var response = await sectionsController.GetSectionRosterAsync("100");
                Assert.IsNotNull(response);
                Assert.AreEqual(rosterStudents.Count, response.Count());
            }
        }

        [TestClass]
        public class SectionsController_GetSectionRoster2Async_Tests
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            private SectionRoster sectionRoster;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                sectionRoster = new SectionRoster()
                {
                    SectionId = "100",
                    StudentIds = new List<string>() { "0001234", "0005678" }
                };

                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionRoster2Async_ArgumentNullException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionRoster2Async(It.IsAny<string>()))
                    .Throws(new ArgumentNullException());
                await sectionsController.GetSectionRoster2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionRoster2Async_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionRoster2Async(It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                await sectionsController.GetSectionRoster2Async("100");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionRoster2Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionRoster2Async(It.IsAny<string>()))
                    .Throws(new Exception());
                await sectionsController.GetSectionRoster2Async("100");
            }

            [TestMethod]
            public async Task SectionsController_GetSectionRoster2Async_Valid()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionRoster2Async(It.IsAny<string>()))
                    .ReturnsAsync(sectionRoster);
                var response = await sectionsController.GetSectionRoster2Async("100");
                Assert.IsNotNull(response);
                Assert.AreEqual("100", response.SectionId);
                Assert.AreEqual(sectionRoster.StudentIds.Count(), response.StudentIds.Count());
                for (int i = 0; i < sectionRoster.StudentIds.Count(); i++)
                {
                    Assert.AreEqual(sectionRoster.StudentIds.ElementAt(i), response.StudentIds.ElementAt(i));
                }
            }
        }

        [TestClass]
        public class SectionsController_GetSectionWaitlistAsync_Test
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            private SectionWaitlist sectionWaitlist;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                sectionWaitlist = new SectionWaitlist()
                {
                    SectionId = "100",
                    StudentIds = new List<string>() { "0001234", "0005678" }
                };

                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlistAsync_ArgumentNullException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistAsync(It.IsAny<string>()))
                    .Throws(new ArgumentNullException());
                await sectionsController.GetSectionWaitlistAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlistAsync_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistAsync(It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                await sectionsController.GetSectionWaitlistAsync("100");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlistAsync_Exception()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistAsync(It.IsAny<string>()))
                    .Throws(new Exception());
                await sectionsController.GetSectionWaitlistAsync("100");
            }

            [TestMethod]
            public async Task SectionsController_GetSectionWaitlistAsync_Valid()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistAsync(It.IsAny<string>()))
                    .ReturnsAsync(sectionWaitlist);
                var response = await sectionsController.GetSectionWaitlistAsync("100");
                Assert.IsNotNull(response);
                Assert.AreEqual("100", response.SectionId);
                Assert.AreEqual(sectionWaitlist.StudentIds.Count(), response.StudentIds.Count());
                for (int i = 0; i < sectionWaitlist.StudentIds.Count(); i++)
                {
                    Assert.AreEqual(sectionWaitlist.StudentIds.ElementAt(i), response.StudentIds.ElementAt(i));
                }
            }
        }
        [TestClass]
        public class SectionsController_GetSectionWaitlist2Async_Test
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            private List<SectionWaitlistStudent> sectionWaitlists;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;
                sectionWaitlists = new List<SectionWaitlistStudent>();
                sectionWaitlists.Add(new SectionWaitlistStudent()
                {
                    SectionId = "100",
                    StudentId = "0001234",
                    Rating = 2,
                    StatusDate = DateTime.Today,
                    WaitListAddDate=DateTime.Today,
                    WaitTime = DateTime.Now
                });

                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlist2Async_ArgumentNullException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistAsync(It.IsAny<string>()))
                    .Throws(new ArgumentNullException());
                await sectionsController.GetSectionWaitlistAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlist2Async_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistAsync(It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                await sectionsController.GetSectionWaitlistAsync("100");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlist2Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistAsync(It.IsAny<string>()))
                    .Throws(new Exception());
                await sectionsController.GetSectionWaitlistAsync("100");
            }

            [TestMethod]
            public async Task SectionsController_GetSectionWaitlist2Async_Valid()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlist2Async(It.IsAny<string>()))
                    .ReturnsAsync(sectionWaitlists);
                var response = await sectionsController.GetSectionWaitlist2Async("100");
                Assert.IsNotNull(response);
                Assert.AreEqual("100", response.ElementAt(0).SectionId);
                Assert.AreEqual(sectionWaitlists.Count(), response.Count());
                for (int i = 0; i < sectionWaitlists.Count(); i++)
                {
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).StudentId, response.ElementAt(i).StudentId);
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).Rating, response.ElementAt(i).Rating);
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).StatusDate, response.ElementAt(i).StatusDate);
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).WaitListAddDate, response.ElementAt(i).WaitListAddDate);
                }
            }
        }

        [TestClass]
        public class SectionsController_QuerySectionWaitlistAsync_Test
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            private List<SectionWaitlistStudent> sectionWaitlists;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;
                sectionWaitlists = new List<SectionWaitlistStudent>();
                sectionWaitlists.Add(new SectionWaitlistStudent()
                {
                    SectionId = "100",
                    StudentId = "0001234",
                    Rating = 2,
                    StatusDate = DateTime.Today,
                    Rank = 2,
                    WaitListAddDate = DateTime.Today,
                    WaitTime = DateTime.Now
                });
                sectionWaitlists.Add(new SectionWaitlistStudent()
                {
                    SectionId = "101",
                    StudentId = "0001235",
                    Rating = 3,
                    StatusDate = DateTime.Today,
                    Rank = 1,
                    WaitListAddDate = DateTime.Today,
                    WaitTime = DateTime.Now
                });
                sectionWaitlists.Add(new SectionWaitlistStudent()
                {
                    SectionId = "102",
                    StudentId = "0001236",
                    Rating = 1,
                    StatusDate = DateTime.Today,
                    Rank= 2,
                    WaitListAddDate = DateTime.Today,
                    WaitTime = DateTime.Now
                });

                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_QuerySectionWaitlistAsync_ArgumentNullException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlist3Async(It.IsAny<SectionWaitlistQueryCriteria>()))
                    .Throws(new ArgumentNullException());
                await sectionsController.QuerySectionWaitlistAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_QuerySectionWaitlistAsync_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlist3Async(It.IsAny<SectionWaitlistQueryCriteria>()))
                    .Throws(new KeyNotFoundException());
                SectionWaitlistQueryCriteria criteria = new SectionWaitlistQueryCriteria()
                {
                    SectionIds = new List<string>() { "100" },
                    IncludeCrossListedSections = true
                };
                await sectionsController.QuerySectionWaitlistAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_QuerySectionWaitlistAsync_Exception()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlist3Async(It.IsAny<SectionWaitlistQueryCriteria>()))
                    .Throws(new Exception());
                SectionWaitlistQueryCriteria criteria = new SectionWaitlistQueryCriteria()
                {
                    SectionIds = new List<string>() { "100" },
                    IncludeCrossListedSections = true
                };
                await sectionsController.QuerySectionWaitlistAsync(criteria);
            }

            [TestMethod]
            public async Task SectionsController_QuerySectionWaitlistAsync_Valid()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlist3Async(It.IsAny<SectionWaitlistQueryCriteria>()))
                    .ReturnsAsync(sectionWaitlists);
                SectionWaitlistQueryCriteria criteria = new SectionWaitlistQueryCriteria()
                {
                    SectionIds = new List<string>() { "100" },
                    IncludeCrossListedSections = true
                };
                var response = await sectionsController.QuerySectionWaitlistAsync(criteria);
                Assert.IsNotNull(response);
                Assert.AreEqual("100", response.ElementAt(0).SectionId);
                Assert.AreEqual("101", response.ElementAt(1).SectionId);
                Assert.AreEqual("102", response.ElementAt(2).SectionId);
                Assert.AreEqual(sectionWaitlists.Count(), response.Count());
                for (int i = 0; i < sectionWaitlists.Count(); i++)
                {
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).StudentId, response.ElementAt(i).StudentId);
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).Rating, response.ElementAt(i).Rating);
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).StatusDate, response.ElementAt(i).StatusDate);
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).Rank, response.ElementAt(i).Rank);
                    Assert.AreEqual(sectionWaitlists.ElementAt(i).WaitListAddDate, response.ElementAt(i).WaitListAddDate);
                    
                }
            }
        }

        [TestClass]
        public class SectionsController_GetSectionWaitlistSettingAsync_Test
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            private SectionWaitlistConfig sectionWaitlistSetting;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;
                sectionWaitlistSetting = new SectionWaitlistConfig();
                sectionWaitlistSetting.ShowRank = true;
                sectionWaitlistSetting.ShowRating = true;
                sectionWaitlistSetting.SortOrderByRank = true;
                sectionWaitlistSetting.WaitlistSortByDate = true;
                sectionWaitlistSetting.InlcudeCrossListedSections = true;
                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlistSettingAsync_ArgumentNullException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistConfigAsync(It.IsAny<string>()))
                    .Throws(new ArgumentNullException());
                await sectionsController.GetSectionWaitlistConfigAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlistSettingAsync_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistConfigAsync(It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                await sectionsController.GetSectionWaitlistConfigAsync("100");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlistSettingAsync_Exception()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistConfigAsync(It.IsAny<string>()))
                    .Throws(new Exception());
                await sectionsController.GetSectionWaitlistConfigAsync("100");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetSectionWaitlistSettingAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistConfigAsync(It.IsAny<string>()))
                        .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    await sectionsController.GetSectionWaitlistConfigAsync("100");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public async Task SectionsController_GetSectionWaitlistSettingAsync_Valid()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistConfigAsync(It.IsAny<string>()))
                    .ReturnsAsync(sectionWaitlistSetting);
                var response = await sectionsController.GetSectionWaitlistConfigAsync("100");
                Assert.IsNotNull(response);
                Assert.AreEqual(true, response.ShowRank);
                Assert.AreEqual(true, response.ShowRating);
                Assert.AreEqual(true, response.SortOrderByRank);
                Assert.AreEqual(true, response.WaitlistSortByDate);
                Assert.AreEqual(true, response.InlcudeCrossListedSections);
            }
        }

        [TestClass]
        public class SectionsController_GetStudentSectionWaitlistsByStudentAndSectionId_Test
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            private StudentSectionWaitlistInfo studentSectionwaitlistInfo;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;
                studentSectionwaitlistInfo = new StudentSectionWaitlistInfo();
                studentSectionwaitlistInfo.SectionWaitlistConfig = new SectionWaitlistConfig();
                studentSectionwaitlistInfo.SectionWaitlistStudent = new SectionWaitlistStudent();
                studentSectionwaitlistInfo.SectionWaitlistStudent.SectionId = "100";
                studentSectionwaitlistInfo.SectionWaitlistStudent.StudentId = "0001234";
                studentSectionwaitlistInfo.SectionWaitlistStudent.Rank = 3;
                studentSectionwaitlistInfo.SectionWaitlistStudent.Rating = 4;
                studentSectionwaitlistInfo.SectionWaitlistConfig.ShowRank = true;
                studentSectionwaitlistInfo.SectionWaitlistConfig.ShowRating = true;

                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetStudentSectionWaitlistsByStudentAndSectionId_ArgumentNullException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetStudentSectionWaitlistsByStudentAndSectionIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentNullException());
                await sectionsController.GetStudentSectionWaitlistsByStudentAndSectionIdAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetStudentSectionWaitlistsByStudentAndSectionId_KeyNotFoundException()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetStudentSectionWaitlistsByStudentAndSectionIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                await sectionsController.GetStudentSectionWaitlistsByStudentAndSectionIdAsync("100", "0001234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetStudentSectionWaitlistsByStudentAndSectionId_Exception()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetSectionWaitlistAsync(It.IsAny<string>()))
                    .Throws(new Exception());
                await sectionsController.GetSectionWaitlistAsync("100");
            }

            [TestMethod]
            public async Task SectionsController_GetStudentSectionWaitlistsByStudentAndSectionId_Valid()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetStudentSectionWaitlistsByStudentAndSectionIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(studentSectionwaitlistInfo);
                var response = await sectionsController.GetStudentSectionWaitlistsByStudentAndSectionIdAsync("100", "0001234");
                Assert.IsNotNull(response);
                Assert.AreEqual("100", response.SectionWaitlistStudent.SectionId);
                Assert.AreEqual(studentSectionwaitlistInfo.SectionWaitlistStudent.StudentId, response.SectionWaitlistStudent.StudentId);
                Assert.AreEqual(studentSectionwaitlistInfo.SectionWaitlistStudent.Rating, response.SectionWaitlistStudent.Rating);
                Assert.AreEqual(studentSectionwaitlistInfo.SectionWaitlistStudent.Rank, response.SectionWaitlistStudent.Rank);
                Assert.AreEqual(studentSectionwaitlistInfo.SectionWaitlistConfig.ShowRank, response.SectionWaitlistConfig.ShowRank);
                Assert.AreEqual(studentSectionwaitlistInfo.SectionWaitlistConfig.ShowRating, response.SectionWaitlistConfig.ShowRating);
            }
        }

        [TestClass]
        public class SectionsController_GetStudentWaitlistStatusesAsync_Test
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            private IEnumerable<StudentWaitlistStatus> studentWaitlistStatus;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;
                studentWaitlistStatus = BuildStatuses();


                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            private List<StudentWaitlistStatus> BuildStatuses()
            {
                List<StudentWaitlistStatus> statuses = new List<StudentWaitlistStatus>()
            {
                new StudentWaitlistStatus()
                {
                    StatusCode = "1",
                    Status = "A",
                    StatusDescription = "Active"
                },
                new StudentWaitlistStatus()
                {
                    StatusCode = "2",
                    Status = "E",
                    StatusDescription = "Enrolled"
                },
                new StudentWaitlistStatus()
                {
                    StatusCode = "3",
                    Status = "C",
                    StatusDescription = "Dropped"
                },
                new StudentWaitlistStatus()
                {
                    StatusCode = "4",
                    Status = "P",
                    StatusDescription = "Permission to Register"
                },
                new StudentWaitlistStatus()
                {
                    StatusCode = "5",
                    Status = "X",
                    StatusDescription = "Expired"
                },
            };

                return statuses;
            }

            [TestMethod]
            public async Task SectionsController_GetStudentWaitlistStatusesAsync_Valid()
            {
                sectionCoordinationServiceMock.Setup(s => s.GetStudentWaitlistStatusesAsync())
                    .ReturnsAsync(studentWaitlistStatus);
                var response = await sectionsController.GetStudentWaitlistStatusesAsync();
                Assert.IsNotNull(response);
                Assert.IsTrue(response.Count() > 0);
                Assert.AreEqual(studentWaitlistStatus.Count(), 5);
                Assert.AreEqual(studentWaitlistStatus.Where(x => x.StatusCode == "1").FirstOrDefault(), response.Where(x => x.StatusCode == "1").FirstOrDefault());
                Assert.AreEqual(studentWaitlistStatus.Where(x => x.StatusCode == "2").FirstOrDefault(), response.Where(x => x.StatusCode == "2").FirstOrDefault());
                Assert.AreEqual(studentWaitlistStatus.Where(x => x.StatusCode == "3").FirstOrDefault(), response.Where(x => x.StatusCode == "3").FirstOrDefault());
                Assert.AreEqual(response.Where(x => x.StatusCode == "1").FirstOrDefault().StatusDescription, "Active");
                Assert.AreEqual(response.Where(x => x.StatusCode == "4").FirstOrDefault().StatusDescription, "Permission to Register");

            }
        }

        [TestClass]
        public class SectionsControllerTestsPutGradesExceptions
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                // controller that will be tested using mock objects
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_UpdateGradesPermissionException()
            {
                string sectionId = "100";
                sectionCoordinationServiceMock.Setup(s => s.ImportGradesAsync(It.Is<Ellucian.Colleague.Dtos.Student.SectionGrades>(x => x.SectionId == sectionId)))
                    .Throws(new Ellucian.Web.Security.PermissionsException());
                await sectionsController.PutCollectionOfStudentGradesAsync(sectionId, new SectionGrades() { SectionId = sectionId, StudentGrades = GetStudentGrade() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_CoordinationServiceThrowsException()
            {
                string sectionId = "101";
                sectionCoordinationServiceMock.Setup(s => s.ImportGradesAsync(It.Is<Ellucian.Colleague.Dtos.Student.SectionGrades>(x => x.SectionId == sectionId)))
                    .Throws(new Exception());
                await sectionsController.PutCollectionOfStudentGradesAsync(sectionId, new SectionGrades() { SectionId = sectionId, StudentGrades = GetStudentGrade() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_MismatchedSectionIds()
            {
                await sectionsController.PutCollectionOfStudentGradesAsync("1", new SectionGrades() { SectionId = "2", StudentGrades = GetStudentGrade() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_MissingSectionId()
            {
                await sectionsController.PutCollectionOfStudentGradesAsync("1", new SectionGrades() { SectionId = null, StudentGrades = GetStudentGrade() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_MissingGrades()
            {
                await sectionsController.PutCollectionOfStudentGradesAsync("1", new SectionGrades() { SectionId = "1" });
            }

            private List<Dtos.Student.StudentGrade> GetStudentGrade()
            {
                List<Dtos.Student.StudentGrade> listOfOneGrade = new List<StudentGrade>();
                listOfOneGrade.Add(new Dtos.Student.StudentGrade() { StudentId = "1" });
                return listOfOneGrade;
            }

        }

        [TestClass]
        public class SectionsControllerTestsPutGrades
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                sectionCoordinationServiceMock.Setup(s => s.ImportGradesAsync(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades>()))
                   .Returns(Task.FromResult<IEnumerable<SectionGradeResponse>>(GetSectionGradeResponse()));

                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            public async Task SectionsController_PutCollectionOfStudentGrades()
            {
                string sectionId = "111";
                var response = await sectionsController.PutCollectionOfStudentGradesAsync(sectionId, new SectionGrades() { SectionId = sectionId, StudentGrades = GetStudentGrade() });
                var expected = GetSectionGradeResponse();

                Assert.AreEqual(expected[0].StudentId, response.First().StudentId);
                Assert.AreEqual(expected[0].Status, response.First().Status);
                Assert.AreEqual(expected[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(expected[0].Errors[0].Message, response.First().Errors[0].Message);
                Assert.AreEqual(expected[0].Errors[0].Property, response.First().Errors[0].Property);
            }

            private List<Dtos.Student.SectionGradeResponse> GetSectionGradeResponse()
            {
                List<SectionGradeResponse> returnList = new List<SectionGradeResponse>();

                var response = new Dtos.Student.SectionGradeResponse();
                response.StudentId = "101";
                response.Status = "status";

                var error = new Dtos.Student.SectionGradeResponseError();
                error.Message = "message";
                error.Property = "property";
                response.Errors.Add(error);

                returnList.Add(response);
                return returnList;
            }

            private List<Dtos.Student.StudentGrade> GetStudentGrade()
            {
                List<Dtos.Student.StudentGrade> listOfOneGrade = new List<StudentGrade>();
                listOfOneGrade.Add(new Dtos.Student.StudentGrade() { StudentId = "1" });
                return listOfOneGrade;
            }
        }

        [TestClass]
        public class SectionsControllerTestsPutGrades2
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

            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                sectionCoordinationServiceMock.Setup(s => s.ImportGrades2Async(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades2>()))
                   .Returns(Task.FromResult<IEnumerable<SectionGradeResponse>>(GetSectionGradeResponse()));

                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades2_EmptySectionGrades()
            {
                var response = await sectionsController.PutCollectionOfStudentGrades2Async("123", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades2_ModelState()
            {
                sectionsController.ModelState.AddModelError("key", "error message");
                var response = await sectionsController.PutCollectionOfStudentGrades2Async("123", null);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades2_EmptySectionID()
            {
                var response = await sectionsController.PutCollectionOfStudentGrades2Async("", new SectionGrades2() { SectionId = "", StudentGrades = GetStudentGrade2() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades2_NonmatchingSectionID()
            {
                var response = await sectionsController.PutCollectionOfStudentGrades2Async("123", new SectionGrades2() { SectionId = "321", StudentGrades = GetStudentGrade2() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades2_Exception()
            {

                sectionCoordinationServiceMock.Setup(s => s.ImportGrades2Async(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades2>())).Throws<Exception>();

                var response = await sectionsController.PutCollectionOfStudentGrades2Async("123", new SectionGrades2() { SectionId = "321", StudentGrades = GetStudentGrade2() });
            }

            [TestMethod]
            public async Task SectionsController_PutCollectionOfStudentGrades2()
            {
                string sectionId = "111";
                var response = await sectionsController.PutCollectionOfStudentGrades2Async(sectionId, new SectionGrades2() { SectionId = sectionId, StudentGrades = GetStudentGrade2() });
                var expected = GetSectionGradeResponse();

                Assert.AreEqual(expected[0].StudentId, response.First().StudentId);
                Assert.AreEqual(expected[0].Status, response.First().Status);
                Assert.AreEqual(expected[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(expected[0].Errors[0].Message, response.First().Errors[0].Message);
                Assert.AreEqual(expected[0].Errors[0].Property, response.First().Errors[0].Property);
            }

            private List<Dtos.Student.SectionGradeResponse> GetSectionGradeResponse()
            {
                List<SectionGradeResponse> returnList = new List<SectionGradeResponse>();

                var response = new Dtos.Student.SectionGradeResponse();
                response.StudentId = "101";
                response.Status = "status";

                var error = new Dtos.Student.SectionGradeResponseError();
                error.Message = "message";
                error.Property = "property";
                response.Errors.Add(error);

                returnList.Add(response);
                return returnList;
            }

            private List<Dtos.Student.StudentGrade2> GetStudentGrade2()
            {
                List<Dtos.Student.StudentGrade2> listOfOneGrade = new List<StudentGrade2>();
                listOfOneGrade.Add(new Dtos.Student.StudentGrade2() { StudentId = "1", EffectiveStartDate = DateTime.Now, EffectiveEndDate = DateTime.Now.AddDays(30) });
                return listOfOneGrade;
            }
        }

        [TestClass]
        public class SectionsControllerTestsPutGrades3
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
            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                sectionCoordinationServiceMock.Setup(s => s.ImportGrades3Async(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades3>()))
                    .Returns(Task.FromResult<IEnumerable<SectionGradeResponse>>(GetSectionGradeResponse()));

                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades3_EmptySectionGrades()
            {
                try
                {
                    var response = await sectionsController.PutCollectionOfStudentGrades3Async("123", null);
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades3_ModelState()
            {
                try
                {
                    sectionsController.ModelState.AddModelError("key", "error message");
                    var response = await sectionsController.PutCollectionOfStudentGrades3Async("123", null);
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades3_EmptySectionID()
            {
                try
                {
                    var response = await sectionsController.PutCollectionOfStudentGrades3Async("", new SectionGrades3() { SectionId = "", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades3_NonmatchingSectionID()
            {
                try
                {
                    var response = await sectionsController.PutCollectionOfStudentGrades3Async("123", new SectionGrades3() { SectionId = "321", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades3_Exception()
            {

                try
                {
                    sectionCoordinationServiceMock.Setup(s => s.ImportGrades3Async(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades3>())).Throws<Exception>();
                    var response = await sectionsController.PutCollectionOfStudentGrades3Async("123", new SectionGrades3() { SectionId = "321", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades3_PermissionsException()
            {
                string sectionId = "100";
                sectionCoordinationServiceMock.Setup(s => s.ImportGrades3Async(It.Is<Ellucian.Colleague.Dtos.Student.SectionGrades3>(x => x.SectionId == sectionId)))
                    .Throws(new Ellucian.Web.Security.PermissionsException());

                try
                {
                    var response = await sectionsController.PutCollectionOfStudentGrades3Async(sectionId, new SectionGrades3() { SectionId = sectionId, StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.Forbidden);
                    throw;
                }
            }

            [TestMethod]
            public async Task SectionsController_PutCollectionOfStudentGrades3()
            {
                string sectionId = "111";
                var response = await sectionsController.PutCollectionOfStudentGrades3Async(sectionId, new SectionGrades3() { SectionId = sectionId, StudentGrades = GetStudentGrade2() });
                var expected = GetSectionGradeResponse();

                Assert.AreEqual(expected[0].StudentId, response.First().StudentId);
                Assert.AreEqual(expected[0].Status, response.First().Status);
                Assert.AreEqual(expected[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(expected[0].Errors[0].Message, response.First().Errors[0].Message);
                Assert.AreEqual(expected[0].Errors[0].Property, response.First().Errors[0].Property);
            }


            private List<Dtos.Student.SectionGradeResponse> GetSectionGradeResponse()
            {
                List<SectionGradeResponse> returnList = new List<SectionGradeResponse>();

                var response = new Dtos.Student.SectionGradeResponse();
                response.StudentId = "101";
                response.Status = "status";

                var error = new Dtos.Student.SectionGradeResponseError();
                error.Message = "message";
                error.Property = "property";
                response.Errors.Add(error);

                returnList.Add(response);
                return returnList;
            }

            private List<Dtos.Student.StudentGrade2> GetStudentGrade2()
            {
                List<Dtos.Student.StudentGrade2> listOfOneGrade = new List<StudentGrade2>();
                listOfOneGrade.Add(new Dtos.Student.StudentGrade2() { StudentId = "1", EffectiveStartDate = DateTime.Now, EffectiveEndDate = DateTime.Now.AddDays(30) });
                return listOfOneGrade;
            }
        }

        [TestClass]
        public class SectionsControllerTestsPutGrades4
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
            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                sectionCoordinationServiceMock.Setup(s => s.ImportGrades4Async(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades3>()))
                    .Returns(Task.FromResult<IEnumerable<SectionGradeResponse>>(GetSectionGradeResponse()));

                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades4_EmptySectionGrades()
            {
                try
                {
                    var response = await sectionsController.PutCollectionOfStudentGrades4Async("123", null);
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades4_ModelState()
            {
                try
                {
                    sectionsController.ModelState.AddModelError("key", "error message");
                    var response = await sectionsController.PutCollectionOfStudentGrades4Async("123", null);
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades4_EmptySectionID()
            {
                try
                {
                    var response = await sectionsController.PutCollectionOfStudentGrades4Async("", new SectionGrades3() { SectionId = "", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades4_NonmatchingSectionID()
            {
                try
                {
                    var response = await sectionsController.PutCollectionOfStudentGrades4Async("123", new SectionGrades3() { SectionId = "321", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades4_Exception()
            {

                try
                {
                    sectionCoordinationServiceMock.Setup(s => s.ImportGrades4Async(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades3>())).Throws<Exception>();
                    var response = await sectionsController.PutCollectionOfStudentGrades4Async("123", new SectionGrades3() { SectionId = "321", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutCollectionOfStudentGrades4_PermissionsException()
            {
                string sectionId = "100";
                sectionCoordinationServiceMock.Setup(s => s.ImportGrades4Async(It.Is<Ellucian.Colleague.Dtos.Student.SectionGrades3>(x => x.SectionId == sectionId)))
                    .Throws(new Ellucian.Web.Security.PermissionsException());

                try
                {
                    var response = await sectionsController.PutCollectionOfStudentGrades4Async(sectionId, new SectionGrades3() { SectionId = sectionId, StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.Forbidden);
                    throw;
                }
            }

            [TestMethod]
            public async Task SectionsController_PutCollectionOfStudentGrades4()
            {
                string sectionId = "111";
                var response = await sectionsController.PutCollectionOfStudentGrades4Async(sectionId, new SectionGrades3() { SectionId = sectionId, StudentGrades = GetStudentGrade2() });
                var expected = GetSectionGradeResponse();

                Assert.AreEqual(expected[0].StudentId, response.First().StudentId);
                Assert.AreEqual(expected[0].Status, response.First().Status);
                Assert.AreEqual(expected[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(expected[0].Errors[0].Message, response.First().Errors[0].Message);
                Assert.AreEqual(expected[0].Errors[0].Property, response.First().Errors[0].Property);
            }


            private List<Dtos.Student.SectionGradeResponse> GetSectionGradeResponse()
            {
                List<SectionGradeResponse> returnList = new List<SectionGradeResponse>();

                var response = new Dtos.Student.SectionGradeResponse();
                response.StudentId = "101";
                response.Status = "status";

                var error = new Dtos.Student.SectionGradeResponseError();
                error.Message = "message";
                error.Property = "property";
                response.Errors.Add(error);

                returnList.Add(response);
                return returnList;
            }

            private List<Dtos.Student.StudentGrade2> GetStudentGrade2()
            {
                List<Dtos.Student.StudentGrade2> listOfOneGrade = new List<StudentGrade2>();
                listOfOneGrade.Add(new Dtos.Student.StudentGrade2() { StudentId = "1", EffectiveStartDate = DateTime.Now, EffectiveEndDate = DateTime.Now.AddDays(30) });
                return listOfOneGrade;
            }
        }

        [TestClass]
        public class SectionsControllerTestsPutIlpGrades1
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
            private SectionsController sectionsController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                sectionCoordinationServiceMock.Setup(s => s.ImportIlpGrades1Async(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades3>()))
                    .Returns(Task.FromResult<IEnumerable<SectionGradeResponse>>(GetSectionGradeResponse()));

                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutIlpCollectionOfStudentGrades1_EmptySectionGrades()
            {
                try
                {
                    var response = await sectionsController.PutIlpCollectionOfStudentGrades1Async("123", null);
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutIlpCollectionOfStudentGrades1_ModelState()
            {
                try
                {
                    sectionsController.ModelState.AddModelError("key", "error message");
                    var response = await sectionsController.PutIlpCollectionOfStudentGrades1Async("123", null);
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutIlpCollectionOfStudentGrades1_EmptySectionID()
            {
                try
                {
                    var response = await sectionsController.PutIlpCollectionOfStudentGrades1Async("", new SectionGrades3() { SectionId = "", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutIlpCollectionOfStudentGrades1_NonmatchingSectionID()
            {
                try
                {
                    var response = await sectionsController.PutIlpCollectionOfStudentGrades1Async("123", new SectionGrades3() { SectionId = "321", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutIlpCollectionOfStudentGrades1_Exception()
            {

                try
                {
                    sectionCoordinationServiceMock.Setup(s => s.ImportIlpGrades1Async(It.IsAny<Ellucian.Colleague.Dtos.Student.SectionGrades3>())).Throws<Exception>();
                    var response = await sectionsController.PutIlpCollectionOfStudentGrades1Async("123", new SectionGrades3() { SectionId = "321", StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.BadRequest);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutIlpCollectionOfStudentGrades1_PermissionsException()
            {
                string sectionId = "100";
                sectionCoordinationServiceMock.Setup(s => s.ImportIlpGrades1Async(It.Is<Ellucian.Colleague.Dtos.Student.SectionGrades3>(x => x.SectionId == sectionId)))
                    .Throws(new Ellucian.Web.Security.PermissionsException());

                try
                {
                    var response = await sectionsController.PutIlpCollectionOfStudentGrades1Async(sectionId, new SectionGrades3() { SectionId = sectionId, StudentGrades = GetStudentGrade2() });
                }
                catch (HttpResponseException hte)
                {
                    Assert.AreEqual(hte.Response.StatusCode, HttpStatusCode.Forbidden);
                    throw;
                }
            }

            [TestMethod]
            public async Task SectionsController_PutIlpCollectionOfStudentGrades1()
            {
                string sectionId = "111";
                var response = await sectionsController.PutIlpCollectionOfStudentGrades1Async(sectionId, new SectionGrades3() { SectionId = sectionId, StudentGrades = GetStudentGrade2() });
                var expected = GetSectionGradeResponse();

                Assert.AreEqual(expected[0].StudentId, response.First().StudentId);
                Assert.AreEqual(expected[0].Status, response.First().Status);
                Assert.AreEqual(expected[0].Errors.Count(), response.First().Errors.Count());
                Assert.AreEqual(expected[0].Errors[0].Message, response.First().Errors[0].Message);
                Assert.AreEqual(expected[0].Errors[0].Property, response.First().Errors[0].Property);
            }


            private List<Dtos.Student.SectionGradeResponse> GetSectionGradeResponse()
            {
                List<SectionGradeResponse> returnList = new List<SectionGradeResponse>();

                var response = new Dtos.Student.SectionGradeResponse();
                response.StudentId = "101";
                response.Status = "status";

                var error = new Dtos.Student.SectionGradeResponseError();
                error.Message = "message";
                error.Property = "property";
                response.Errors.Add(error);

                returnList.Add(response);
                return returnList;
            }

            private List<Dtos.Student.StudentGrade2> GetStudentGrade2()
            {
                List<Dtos.Student.StudentGrade2> listOfOneGrade = new List<StudentGrade2>();
                listOfOneGrade.Add(new Dtos.Student.StudentGrade2() { StudentId = "1", EffectiveStartDate = DateTime.Now, EffectiveEndDate = DateTime.Now.AddDays(30) });
                return listOfOneGrade;
            }
        }


        [TestClass]
        public class SectionsControllerTestsQuerySectionRegistrationDates
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

            private SectionsController sectionsController;
            private Mock<IRegistrationGroupService> registrationGroupServiceMock;
            private IRegistrationGroupService registrationGroupService;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                registrationGroupServiceMock = new Mock<IRegistrationGroupService>();
                registrationGroupService = registrationGroupServiceMock.Object;
                logger = new Mock<ILogger>().Object;
                sectionsController = new SectionsController(null, null, registrationGroupService, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                registrationGroupService = null;
            }

            [TestMethod]
            public async Task SectionsController_SuccessfulQuery()
            {
                registrationGroupServiceMock.Setup(r => r.GetSectionRegistrationDatesAsync(It.IsAny<List<string>>(), true)).Returns(Task.FromResult<IEnumerable<SectionRegistrationDate>>(GetSectionRegistrationDatesResponse()));
                var criteria = new Dtos.Student.SectionDateQueryCriteria();
                criteria.SectionIds = new List<string>() { "Section1", "Section2" };
                criteria.ConsiderUsersGroup = true;
                var response = await sectionsController.QuerySectionRegistrationDatesAsync(criteria);
                var expected = GetSectionRegistrationDatesResponse();
                Assert.AreEqual(expected.Count(), response.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    registrationGroupServiceMock.Setup(r => r.GetSectionRegistrationDatesAsync(It.IsAny<List<string>>(), true)).Throws(new Exception());
                    var criteria = new Dtos.Student.SectionDateQueryCriteria();
                    criteria.SectionIds = new List<string>() { "Section1", "Section2" };
                    criteria.ConsiderUsersGroup = true;
                    var response = await sectionsController.QuerySectionRegistrationDatesAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_NoSectionIdsProvided_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    registrationGroupServiceMock.Setup(r => r.GetSectionRegistrationDatesAsync(It.IsAny<List<string>>(), true)).Returns(Task.FromResult<IEnumerable<SectionRegistrationDate>>(GetSectionRegistrationDatesResponse()));
                    var criteria = new Dtos.Student.SectionDateQueryCriteria();
                    var response = await sectionsController.QuerySectionRegistrationDatesAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            private List<Dtos.Student.SectionRegistrationDate> GetSectionRegistrationDatesResponse()
            {
                List<Dtos.Student.SectionRegistrationDate> returnList = new List<Dtos.Student.SectionRegistrationDate>();
                var item1 = new Dtos.Student.SectionRegistrationDate() { SectionId = "section1", RegistrationStartDate = DateTime.Today, RegistrationEndDate = DateTime.Today.AddDays(10) };
                returnList.Add(item1);
                var item2 = new Dtos.Student.SectionRegistrationDate() { SectionId = "section2", RegistrationStartDate = DateTime.Today, RegistrationEndDate = DateTime.Today.AddDays(10) };
                returnList.Add(item2);
                return returnList;
            }

            [TestMethod]
            public async Task SectionsController_SuccessfulQuery1()
            {
                registrationGroupServiceMock.Setup(r => r.GetSectionRegistrationDatesAsync(It.IsAny<List<string>>(), false)).Returns(Task.FromResult<IEnumerable<SectionRegistrationDate>>(GetSectionRegistrationDatesResponse()));
                var criteria = new Dtos.Student.SectionDateQueryCriteria();
                criteria.SectionIds = new List<string>() { "Section1", "Section2" };
                criteria.ConsiderUsersGroup = false;
                var response = await sectionsController.QuerySectionRegistrationDatesAsync(criteria);
                var expected = GetSectionRegistrationDatesResponse();
                Assert.AreEqual(expected.Count(), response.Count());
            }


        }

        //V6 Data Model tests

        [TestClass]
        public class SectionsControllerTests_PostHedmSectionAsync_V6
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

            private IAdapterRegistry adapterRegistry;
            private SectionsController sectionsController;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepository;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;
            private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>();

            private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

            private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
            private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
            private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";


            private List<Dtos.Section3> allSectionDtos;
            private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
            private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
            private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;

                studentRepoMock = new Mock<IStudentRepository>();
                studentRepository = studentRepoMock.Object;

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
                allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
                if (allCourseLevels != null)
                    levels.Add(allCourseLevels[0].Code);

                adapterRegistry = adapterRegistryMock.Object;

                List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

                firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                firstSection.TermId = "2012/FA";
                firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
                firstSection.Guid = firstSectionGuid;
                sectionEntityList.Add(firstSection);

                secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
                secondSection.Guid = secondSectionGuid;
                sectionEntityList.Add(secondSection);

                thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
                thirdSection.EndDate = new DateTime(2011, 12, 21);
                thirdSection.Guid = thirdSectionGuid;
                sectionEntityList.Add(thirdSection);

                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
                IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());

                // Mock section repo GetCachedSections
                bool bestFit = false;
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // Mock section repo GetNonCachedSections
                sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // mock DTO adapters
                var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

                academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
                allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

                allSectionDtos = new List<Dtos.Section3>();
                foreach (var sectionEntity in sectionEntityList)
                {
                    //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                    var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section3>(adapterRegistry, logger);
                    Ellucian.Colleague.Dtos.Section3 target = sectionDtoAdapter.MapToType(sectionEntity);
                    target.Id = sectionEntity.Guid;
                    Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                    target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                    Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                    target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                    var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                    if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                    if (sectionEntity.CourseLevelCodes != null)
                    {
                        var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                        target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                    }
                    allSectionDtos.Add(target);
                }

                // mock controller
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                sectionsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionRepository = null;
                studentRepository = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PostHedmSectionAsync_Exception()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                    .Setup(svc => svc.PostSection3Async(It.IsAny<Dtos.Section3>()))
                    .Throws<Exception>();
                await sectionsController.PostHedmSection2Async(It.IsAny<Dtos.Section3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PostHedmSectionAsync_ConfigurationException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PostSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<ConfigurationException>();
                await sectionsController.PostHedmSection2Async(It.IsAny<Dtos.Section3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PostHedmSectionAsync_PermissionException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PostSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<PermissionsException>();
                await sectionsController.PostHedmSection2Async(It.IsAny<Dtos.Section3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PostHedmSectionAsync_ArgNullException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                  .Setup(svc => svc.PostSection3Async(null))
                  .Throws<ArgumentNullException>();
                await sectionsController.PostHedmSection2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PostHedmSectionAsync_RepoException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PostSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<RepositoryException>();
                await sectionsController.PostHedmSection2Async(It.IsAny<Dtos.Section3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PostHedmSectionAsync_IntegrationApiException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PostSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<IntegrationApiException>();
                await sectionsController.PostHedmSection2Async(It.IsAny<Dtos.Section3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PostHedmSectionAsync_ID_Null()
            {
                sectionCoordinationServiceMock
                   .Setup(svc => svc.PostSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<IntegrationApiException>();
                await sectionsController.PostHedmSection2Async(new Dtos.Section3() { Id = string.Empty });
            }

            [TestMethod]
            public async Task SectionsController_PostHedmSectionAsync()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock.Setup(svc => svc.PostSection3Async(section)).ReturnsAsync(section);
                var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

                var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
                Ellucian.Colleague.Domain.Student.Entities.Course course = null;
                if (section.Course != null)
                    course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);
                section.Id = Guid.Empty.ToString();
                var sectionByGuid = await sectionsController.PostHedmSection2Async(section);
                Assert.AreEqual(section.Id, sectionByGuid.Id);
                Assert.AreEqual(section.Title, sectionByGuid.Title);
                Assert.AreEqual(section.Description, sectionByGuid.Description);
                Assert.AreEqual(academicLevel.Guid, sectionByGuid.AcademicLevels.FirstOrDefault().Id);
                Assert.AreEqual(gradeScheme.Guid, sectionByGuid.GradeSchemes.FirstOrDefault().Id);
                Assert.AreEqual(section.StartOn, sectionByGuid.StartOn);
                Assert.AreEqual(section.EndOn, sectionByGuid.EndOn);
                if (course != null && sectionByGuid.Course != null)
                    Assert.AreEqual(course.Guid, sectionByGuid.Course.Id);
                Assert.AreEqual(section.Number, sectionByGuid.Number);
                Assert.AreEqual(section.MaximumEnrollment, sectionByGuid.MaximumEnrollment);

                var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
                if (courseLevel != null)
                    Assert.AreEqual(courseLevel.Guid, sectionByGuid.CourseLevels[0].Id);

            }

            //POST V6
            //Successful
            //PostHedmSection2Async

            [TestMethod]
            public async Task SectionsController_PostHedmSection2Async_Permissions()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
                section.Id = Guid.Empty.ToString();
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Sections" },
                        { "action", "PostHedmSection2Async" }
                    };
                HttpRoute route = new HttpRoute("Sections", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionsController.Request.SetRouteData(data);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection);

                var controllerContext = sectionsController.ControllerContext;
                var actionDescriptor = sectionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                sectionCoordinationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionCoordinationServiceMock.Setup(svc => svc.PostSection3Async(section)).ReturnsAsync(section);
                var sectionByGuid = await sectionsController.PostHedmSection2Async(section);

                Object filterObject;
                sectionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateSection));

            }

            //POST V6
            //Exception
            //PostHedmSection2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task sectionsController_PostHedmSection2Async_Invalid_Permissions()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Sections" },
                        { "action", "PostHedmSection2Async" }
                    };
                HttpRoute route = new HttpRoute("sections", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionsController.ControllerContext;
                var actionDescriptor = sectionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionCoordinationServiceMock.Setup(svc => svc.PostSection3Async(It.IsAny<Dtos.Section3>())).Throws<PermissionsException>();
                    sectionCoordinationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create sections."));
                    await sectionsController.PostHedmSection2Async(It.IsAny<Dtos.Section3>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

        }

        [TestClass]
        public class SectionsControllerTests_PutHedmSectionAsync_V6
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

            private IAdapterRegistry adapterRegistry;
            private SectionsController sectionsController;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepository;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;
            private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>();

            private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

            private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
            private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
            private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

            private List<Dtos.Section3> allSectionDtos;
            private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
            private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
            private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;

                studentRepoMock = new Mock<IStudentRepository>();
                studentRepository = studentRepoMock.Object;

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
                allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
                if (allCourseLevels != null)
                    levels.Add(allCourseLevels[0].Code);

                adapterRegistry = adapterRegistryMock.Object;

                List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

                firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                firstSection.TermId = "2012/FA";
                firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
                firstSection.Guid = firstSectionGuid;
                sectionEntityList.Add(firstSection);

                secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
                secondSection.Guid = secondSectionGuid;
                sectionEntityList.Add(secondSection);

                thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
                thirdSection.EndDate = new DateTime(2011, 12, 21);
                thirdSection.Guid = thirdSectionGuid;
                sectionEntityList.Add(thirdSection);

                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
                IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());

                // Mock section repo GetCachedSections
                bool bestFit = false;
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // Mock section repo GetNonCachedSections
                sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // mock DTO adapters
                var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

                academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
                allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

                allSectionDtos = new List<Dtos.Section3>();
                foreach (var sectionEntity in sectionEntityList)
                {
                    //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                    var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section3>(adapterRegistry, logger);
                    Ellucian.Colleague.Dtos.Section3 target = sectionDtoAdapter.MapToType(sectionEntity);
                    target.Id = sectionEntity.Guid;
                    Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                    target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                    Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                    target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                    var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                    if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                    if (sectionEntity.CourseLevelCodes != null)
                    {
                        var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                        target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                    }
                    allSectionDtos.Add(target);
                }

                // mock controller
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                sectionsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionRepository = null;
                studentRepository = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_Exception()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                    .Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>()))
                    .Throws<Exception>();
                await sectionsController.PutHedmSection2Async(section.Id, section);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_ConfigurationException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<ConfigurationException>();
                await sectionsController.PutHedmSection2Async(section.Id, section);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_PermissionException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<PermissionsException>();
                await sectionsController.PutHedmSection2Async(section.Id, section);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_RepoException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<RepositoryException>();
                await sectionsController.PutHedmSection2Async(section.Id, section);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_ArgException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<ArgumentException>();
                await sectionsController.PutHedmSection2Async(section.Id, section);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_NilGUID_ArgException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<ArgumentException>();
                await sectionsController.PutHedmSection2Async(Guid.Empty.ToString(), section);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_SectionIdNull_ArgException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<ArgumentException>();
                await sectionsController.PutHedmSection2Async("1234", new Dtos.Section3() { Id = "" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_IntegrationApiException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                   .Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>()))
                   .Throws<IntegrationApiException>();
                await sectionsController.PutHedmSection2Async(section.Id, section);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_NullIDException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                await sectionsController.PutHedmSection2Async(null, section);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_NullSectionException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                await sectionsController.PutHedmSection2Async(firstSectionGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_PutHedmSectionAsync_InvalidException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                await sectionsController.PutHedmSection2Async(secondSectionGuid, section);
            }

            [TestMethod]
            public async Task SectionsController_PutHedmSectionAsync()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock.Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>())).ReturnsAsync(section);
                sectionCoordinationServiceMock.Setup(svc => svc.GetSection3ByGuidAsync(section.Id)).ReturnsAsync(section);

                var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

                var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
                Ellucian.Colleague.Domain.Student.Entities.Course course = null;
                if (section.Course != null)
                    course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

                var sectionByGuid = await sectionsController.PutHedmSection2Async(firstSectionGuid, section);
                Assert.AreEqual(section.Id, sectionByGuid.Id);
                Assert.AreEqual(section.Title, sectionByGuid.Title);
                Assert.AreEqual(section.Description, sectionByGuid.Description);
                Assert.AreEqual(academicLevel.Guid, sectionByGuid.AcademicLevels.FirstOrDefault().Id);
                Assert.AreEqual(gradeScheme.Guid, sectionByGuid.GradeSchemes.FirstOrDefault().Id);
                Assert.AreEqual(section.StartOn, sectionByGuid.StartOn);
                Assert.AreEqual(section.EndOn, sectionByGuid.EndOn);
                if (course != null && sectionByGuid.Course != null)
                    Assert.AreEqual(course.Guid, sectionByGuid.Course.Id);
                Assert.AreEqual(section.Number, sectionByGuid.Number);
                Assert.AreEqual(section.MaximumEnrollment, sectionByGuid.MaximumEnrollment);

                var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
                if (courseLevel != null)
                    Assert.AreEqual(courseLevel.Guid, sectionByGuid.CourseLevels[0].Id);
            }

            //PUT V6
            //Successful
            //PutHedmSection2Async

            [TestMethod]
            public async Task SectionsController_PutHedmSection2Async_Permissions()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Sections" },
                        { "action", "PutHedmSection2Async" }
                    };
                HttpRoute route = new HttpRoute("sections", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionsController.Request.SetRouteData(data);
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection);

                var controllerContext = sectionsController.ControllerContext;
                var actionDescriptor = sectionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                sectionCoordinationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionCoordinationServiceMock.Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>())).ReturnsAsync(section);
                var sectionByGuid = await sectionsController.PutHedmSection2Async(firstSectionGuid, section);

                Object filterObject;
                sectionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateSection));

            }

            //Put V6
            //Exception
            //PutHedmSection2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task sectionsController_PutHedmSection2Async_Invalid_Permissions()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Sections" },
                        { "action", "PutHedmSection2Async" }
                    };
                HttpRoute route = new HttpRoute("sections", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionsController.ControllerContext;
                var actionDescriptor = sectionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionCoordinationServiceMock.Setup(svc => svc.PutSection3Async(It.IsAny<Dtos.Section3>())).Throws<PermissionsException>();
                    sectionCoordinationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update sections."));
                    await sectionsController.PutHedmSection2Async(section.Id, section);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

        }

        [TestClass]
        public class SectionsControllerTestsGetHedmSectionById_V6
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

            private IAdapterRegistry adapterRegistry;
            private SectionsController sectionsController;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepository;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;
            private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>();

            private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

            private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
            private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
            private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

            private Ellucian.Colleague.Domain.Student.Entities.Student firstStudent;
            private Ellucian.Colleague.Domain.Student.Entities.Student secondStudent;
            private Ellucian.Colleague.Domain.Student.Entities.Student thirdStudent;

            private List<Dtos.Section3> allSectionDtos;
            private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
            private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
            private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;

                studentRepoMock = new Mock<IStudentRepository>();
                studentRepository = studentRepoMock.Object;

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
                allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
                if (allCourseLevels != null)
                    levels.Add(allCourseLevels[0].Code);

                firstStudent = new Domain.Student.Entities.Student("STU1", "Gamgee", null, new List<string>(), new List<string>());
                firstStudent.FirstName = "Samwise";
                secondStudent = new Domain.Student.Entities.Student("STU2", "Took", null, new List<string>(), new List<string>());
                secondStudent.FirstName = "Peregrin";
                thirdStudent = new Domain.Student.Entities.Student("STU1", "Baggins", null, new List<string>(), new List<string>());
                thirdStudent.FirstName = "Frodo"; thirdStudent.MiddleName = "Ring-bearer";

                adapterRegistry = adapterRegistryMock.Object;

                List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

                firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                firstSection.TermId = "2012/FA";
                firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
                firstSection.Guid = firstSectionGuid;
                sectionEntityList.Add(firstSection);

                secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
                secondSection.Guid = secondSectionGuid;
                sectionEntityList.Add(secondSection);

                thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
                thirdSection.EndDate = new DateTime(2011, 12, 21);
                thirdSection.Guid = thirdSectionGuid;
                sectionEntityList.Add(thirdSection);

                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
                IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());


                // Mock section repo GetCachedSections
                bool bestFit = false;
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // Mock section repo GetNonCachedSections
                sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // mock DTO adapters
                var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

                academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
                allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

                allSectionDtos = new List<Dtos.Section3>();
                foreach (var sectionEntity in sectionEntityList)
                {
                    //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                    var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section3>(adapterRegistry, logger);
                    Ellucian.Colleague.Dtos.Section3 target = sectionDtoAdapter.MapToType(sectionEntity);
                    target.Id = sectionEntity.Guid;
                    Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                    target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                    Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                    target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                    var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                    if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                    if (sectionEntity.CourseLevelCodes != null)
                    {
                        var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                        target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                    }
                    allSectionDtos.Add(target);
                }

                sectionCoordinationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                // mock controller
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionRepository = null;
                studentRepository = null;
                sectionCoordinationService = null;
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSectionByGuid_Exception()
            {
                sectionCoordinationServiceMock.Setup(svc => svc.GetSection3ByGuidAsync(It.IsAny<string>())).Throws<Exception>();
                await sectionsController.GetHedmSectionByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSectionByGuid_ConfigurationException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionsController.GetHedmSectionByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSectionByGuid_PermissionException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionsController.GetHedmSectionByGuid2Async(It.IsAny<string>());
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSectionByGuid_ArgNullException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionsController.GetHedmSectionByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSectionByGuid_RepoException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new RepositoryException());
                await sectionsController.GetHedmSectionByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSectionByGuid_IntegrationApiException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionsController.GetHedmSectionByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSectionByGuid_ConfigurationExceptionException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionsController.GetHedmSectionByGuid2Async("");
            }

            [TestMethod]
            public async Task SectionsController_GetHedmSectionByGuid()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock.Setup(svc => svc.GetSection3ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
                var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

                var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
                Ellucian.Colleague.Domain.Student.Entities.Course course = null;
                if (section.Course != null)
                    course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

                var sectionByGuid = await sectionsController.GetHedmSectionByGuid2Async(firstSectionGuid);
                Assert.AreEqual(section.Id, sectionByGuid.Id);
                Assert.AreEqual(section.Title, sectionByGuid.Title);
                Assert.AreEqual(section.Description, sectionByGuid.Description);
                Assert.AreEqual(academicLevel.Guid, sectionByGuid.AcademicLevels.FirstOrDefault().Id);
                Assert.AreEqual(gradeScheme.Guid, sectionByGuid.GradeSchemes.FirstOrDefault().Id);
                Assert.AreEqual(section.StartOn, sectionByGuid.StartOn);
                Assert.AreEqual(section.EndOn, sectionByGuid.EndOn);
                if (course != null && sectionByGuid.Course != null)
                    Assert.AreEqual(course.Guid, sectionByGuid.Course.Id);
                Assert.AreEqual(section.Number, sectionByGuid.Number);
                Assert.AreEqual(section.MaximumEnrollment, sectionByGuid.MaximumEnrollment);

                var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
                if (courseLevel != null)
                    Assert.AreEqual(courseLevel.Guid, sectionByGuid.CourseLevels[0].Id);

            }

        }

        [TestClass]
        public class SectionsControllerTestsDeleteHedmSectionById_V6
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

            private IAdapterRegistry adapterRegistry;
            private SectionsController sectionsController;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepository;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;
            private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>();

            private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

            private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
            private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
            private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

            private Ellucian.Colleague.Domain.Student.Entities.Student firstStudent;
            private Ellucian.Colleague.Domain.Student.Entities.Student secondStudent;
            private Ellucian.Colleague.Domain.Student.Entities.Student thirdStudent;

            private List<Dtos.Section3> allSectionDtos;
            private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
            private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
            private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;

                studentRepoMock = new Mock<IStudentRepository>();
                studentRepository = studentRepoMock.Object;

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
                allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
                if (allCourseLevels != null)
                    levels.Add(allCourseLevels[0].Code);

                firstStudent = new Domain.Student.Entities.Student("STU1", "Gamgee", null, new List<string>(), new List<string>());
                firstStudent.FirstName = "Samwise";
                secondStudent = new Domain.Student.Entities.Student("STU2", "Took", null, new List<string>(), new List<string>());
                secondStudent.FirstName = "Peregrin";
                thirdStudent = new Domain.Student.Entities.Student("STU1", "Baggins", null, new List<string>(), new List<string>());
                thirdStudent.FirstName = "Frodo"; thirdStudent.MiddleName = "Ring-bearer";

                adapterRegistry = adapterRegistryMock.Object;

                List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

                firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                firstSection.TermId = "2012/FA";
                firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
                firstSection.Guid = firstSectionGuid;
                sectionEntityList.Add(firstSection);

                secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
                secondSection.Guid = secondSectionGuid;
                sectionEntityList.Add(secondSection);

                thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
                thirdSection.EndDate = new DateTime(2011, 12, 21);
                thirdSection.Guid = thirdSectionGuid;
                sectionEntityList.Add(thirdSection);

                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
                IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());


                // Mock section repo GetCachedSections
                bool bestFit = false;
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // Mock section repo GetNonCachedSections
                sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // mock DTO adapters
                var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

                academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
                allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

                allSectionDtos = new List<Dtos.Section3>();
                foreach (var sectionEntity in sectionEntityList)
                {
                    //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                    var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section3>(adapterRegistry, logger);
                    Ellucian.Colleague.Dtos.Section3 target = sectionDtoAdapter.MapToType(sectionEntity);
                    target.Id = sectionEntity.Guid;
                    Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                    target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                    Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                    target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                    var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                    if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                    if (sectionEntity.CourseLevelCodes != null)
                    {
                        var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                        target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                    }
                    allSectionDtos.Add(target);
                }


                // mock controller
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionRepository = null;
                studentRepository = null;
                sectionCoordinationService = null;
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_Exception()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock.Setup(svc => svc.GetSection3ByGuidAsync(It.IsAny<string>())).Throws<Exception>();
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_ConfigurationException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_PermissionException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_ArgNullException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionsController.DeleteHedmSectionByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_ArgException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentException());
                await sectionsController.DeleteHedmSectionByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_RepoException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new RepositoryException());
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_IntegrationApiException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_ConfigurationExceptionException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.GetSection3ByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_GuidException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
                section.Id = secondSectionGuid;
                sectionCoordinationServiceMock.Setup(svc => svc.GetSection3ByGuidAsync(firstSectionGuid)).ReturnsAsync(section);
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid_NullException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock.Setup(svc => svc.GetSection3ByGuidAsync(firstSectionGuid)).ReturnsAsync(() => null);
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_DeleteHedmSectionByGuid()
            {
                await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
            }

        }

        [TestClass]
        public class SectionsControllerTestsGetHedmSection_V6
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

            private IAdapterRegistry adapterRegistry;
            private SectionsController sectionsController;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepository;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;
            private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
            private List<string> levels = new List<string>();

            private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
            private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

            private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
            private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
            private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

            private Ellucian.Colleague.Domain.Student.Entities.Student firstStudent;
            private Ellucian.Colleague.Domain.Student.Entities.Student secondStudent;
            private Ellucian.Colleague.Domain.Student.Entities.Student thirdStudent;

            private List<Dtos.Section3> allSectionDtos;
            private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
            private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
            private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;
            private Ellucian.Web.Http.Models.Paging paging = new Web.Http.Models.Paging(3, 0);

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;

                studentRepoMock = new Mock<IStudentRepository>();
                studentRepository = studentRepoMock.Object;

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
                allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
                if (allCourseLevels != null)
                    levels.Add(allCourseLevels[0].Code);

                firstStudent = new Domain.Student.Entities.Student("STU1", "Gamgee", null, new List<string>(), new List<string>());
                firstStudent.FirstName = "Samwise";
                secondStudent = new Domain.Student.Entities.Student("STU2", "Took", null, new List<string>(), new List<string>());
                secondStudent.FirstName = "Peregrin";
                thirdStudent = new Domain.Student.Entities.Student("STU1", "Baggins", null, new List<string>(), new List<string>());
                thirdStudent.FirstName = "Frodo"; thirdStudent.MiddleName = "Ring-bearer";

                adapterRegistry = adapterRegistryMock.Object;

                List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

                firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                firstSection.TermId = "2012/FA";
                firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
                firstSection.Guid = firstSectionGuid;
                sectionEntityList.Add(firstSection);

                secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
                secondSection.Guid = secondSectionGuid;
                sectionEntityList.Add(secondSection);

                thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
                thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
                thirdSection.EndDate = new DateTime(2011, 12, 21);
                thirdSection.Guid = thirdSectionGuid;
                sectionEntityList.Add(thirdSection);

                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
                IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
                sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());


                // Mock section repo GetCachedSections
                bool bestFit = false;
                sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // Mock section repo GetNonCachedSections
                sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
                // mock DTO adapters
                var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

                academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
                allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

                allSectionDtos = new List<Dtos.Section3>();
                foreach (var sectionEntity in sectionEntityList)
                {
                    //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                    var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section3>(adapterRegistry, logger);
                    Ellucian.Colleague.Dtos.Section3 target = sectionDtoAdapter.MapToType(sectionEntity);
                    target.Id = sectionEntity.Guid;
                    Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                    target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                    Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                    target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                    var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                    if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                    if (sectionEntity.CourseLevelCodes != null)
                    {
                        var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                        target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                    }
                    allSectionDtos.Add(target);
                }

                sectionCoordinationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                // mock controller
                sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionsController = null;
                sectionRepository = null;
                studentRepository = null;
                sectionCoordinationService = null;
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSections_Exception()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                    .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                    .Throws<Exception>();
                await sectionsController.GetHedmSections2Async(paging, section.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSections_ConfigurationException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                     .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new ConfigurationException());
                await sectionsController.GetHedmSections2Async(paging, section.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSections_PermissionException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                     .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new PermissionsException());
                await sectionsController.GetHedmSections2Async(paging, section.Title);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSections_ArgNullException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                     .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionsController.GetHedmSections2Async(paging, section.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSections_RepoException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                     .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new RepositoryException());
                await sectionsController.GetHedmSections2Async(paging, section.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSections_IntegrationApiException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                     .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionsController.GetHedmSections2Async(paging, section.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_GetHedmSections_ConfigurationExceptionException()
            {
                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

                sectionCoordinationServiceMock
                     .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new ConfigurationException());
                await sectionsController.GetHedmSections2Async(paging, section.Title);
            }

            [TestMethod]
            public async Task SectionsController_GetHedmSectionByTitle()
            {
                sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
                var tuple = new Tuple<IEnumerable<Dtos.Section3>, int>(new List<Dtos.Section3>() { section }, 5);

                sectionCoordinationServiceMock.Setup(svc => svc.GetSection3ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
                sectionCoordinationServiceMock.Setup(svc => svc.GetSections3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                    .ReturnsAsync(tuple);
                var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

                var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
                Ellucian.Colleague.Domain.Student.Entities.Course course = null;
                if (section.Course != null)
                    course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

                var sectionByTitle = await sectionsController.GetHedmSections2Async(paging, title: section.Title);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionByTitle.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Section3> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section3>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(sectionByTitle is IHttpActionResult);

                Assert.AreEqual(section.Id, result.Id);
                Assert.AreEqual(section.Title, result.Title);
                Assert.AreEqual(section.Description, result.Description);
                Assert.AreEqual(academicLevel.Guid, result.AcademicLevels.FirstOrDefault().Id);
                Assert.AreEqual(gradeScheme.Guid, result.GradeSchemes.FirstOrDefault().Id);
                Assert.AreEqual(section.StartOn, result.StartOn);
                Assert.AreEqual(section.EndOn, result.EndOn);
                if (course != null && result.Course != null)
                    Assert.AreEqual(course.Guid, result.Course.Id);
                Assert.AreEqual(section.Number, result.Number);
                Assert.AreEqual(section.MaximumEnrollment, result.MaximumEnrollment);

                var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
                if (courseLevel != null)
                    Assert.AreEqual(courseLevel.Guid, result.CourseLevels[0].Id);

            }
        }
    }

    [TestClass]
    public class SectionsControllerTests_PostHedmSectionAsync_V8
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

        private IAdapterRegistry adapterRegistry;
        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
        private List<string> levels = new List<string>();

        private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

        private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
        private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
        private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";


        private List<Dtos.Section4> allSectionDtos;
        private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
        private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;

            allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
            allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
            if (allCourseLevels != null)
                levels.Add(allCourseLevels[0].Code);

            adapterRegistry = adapterRegistryMock.Object;

            List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

            firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            firstSection.TermId = "2012/FA";
            firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
            firstSection.Guid = firstSectionGuid;
            sectionEntityList.Add(firstSection);

            secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
            secondSection.Guid = secondSectionGuid;
            sectionEntityList.Add(secondSection);

            thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
            thirdSection.EndDate = new DateTime(2011, 12, 21);
            thirdSection.Guid = thirdSectionGuid;
            sectionEntityList.Add(thirdSection);

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
            IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());

            // Mock section repo GetCachedSections
            bool bestFit = false;
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // Mock section repo GetNonCachedSections
            sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // mock DTO adapters
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

            academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

            allSectionDtos = new List<Dtos.Section4>();
            foreach (var sectionEntity in sectionEntityList)
            {
                //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section4>(adapterRegistry, logger);
                Ellucian.Colleague.Dtos.Section4 target = sectionDtoAdapter.MapToType(sectionEntity);
                target.Id = sectionEntity.Guid;
                Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                if (sectionEntity.CourseLevelCodes != null)
                {
                    var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                    target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                }
                allSectionDtos.Add(target);
            }

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            sectionsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid)));
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSectionAsync_Exception()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<Exception>();
            await sectionsController.PostHedmSection4Async(It.IsAny<Dtos.Section4>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSectionAsync_ConfigurationException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<ConfigurationException>();
            await sectionsController.PostHedmSection4Async(It.IsAny<Dtos.Section4>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSectionAsync_PermissionException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<PermissionsException>();
            await sectionsController.PostHedmSection4Async(It.IsAny<Dtos.Section4>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSectionAsync_ArgNullException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection4Async(null))
                .Throws<ArgumentNullException>();
            await sectionsController.PostHedmSection2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSectionAsync_RepoException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<RepositoryException>();
            await sectionsController.PostHedmSection4Async(It.IsAny<Dtos.Section4>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSectionAsync_IntegrationApiException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<IntegrationApiException>();
            await sectionsController.PostHedmSection4Async(It.IsAny<Dtos.Section4>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSectionAsync_ID_Null()
        {
            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<IntegrationApiException>();
            await sectionsController.PostHedmSection4Async(new Dtos.Section4() { Id = string.Empty });
        }

        [TestMethod]
        public async Task SectionsController_PostHedmSectionAsync()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock.Setup(svc => svc.PostSection4Async(section)).ReturnsAsync(section);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);
            section.Id = Guid.Empty.ToString();
            var sectionByGuid = await sectionsController.PostHedmSection4Async(section);
            Assert.AreEqual(section.Id, sectionByGuid.Id);
            Assert.AreEqual(section.Title, sectionByGuid.Title);
            Assert.AreEqual(section.Description, sectionByGuid.Description);
            Assert.AreEqual(academicLevel.Guid, sectionByGuid.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, sectionByGuid.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, sectionByGuid.StartOn);
            Assert.AreEqual(section.EndOn, sectionByGuid.EndOn);
            if (course != null && sectionByGuid.Course != null)
                Assert.AreEqual(course.Guid, sectionByGuid.Course.Id);
            Assert.AreEqual(section.Number, sectionByGuid.Number);
            Assert.AreEqual(section.MaximumEnrollment, sectionByGuid.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, sectionByGuid.CourseLevels[0].Id);

        }

        //POST V8
        //Successful
        //PostHedmSection4Async

        [TestMethod]
        public async Task SectionsController_PostHedmSection4Async_Permissions()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            section.Id = Guid.Empty.ToString();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Sections" },
                        { "action", "PostHedmSection4Async" }
                    };
            HttpRoute route = new HttpRoute("Sections", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            sectionsController.Request.SetRouteData(data);
            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection);

            var controllerContext = sectionsController.ControllerContext;
            var actionDescriptor = sectionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            sectionCoordinationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            sectionCoordinationServiceMock.Setup(svc => svc.PostSection4Async(section)).ReturnsAsync(section);
            var sectionByGuid = await sectionsController.PostHedmSection4Async(section);

            Object filterObject;
            sectionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateSection));

        }

        //POST V8
        //Exception
        //PostHedmSection4Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task sectionsController_PostHedmSection4Async_Invalid_Permissions()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Sections" },
                        { "action", "PostHedmSection4Async" }
                    };
            HttpRoute route = new HttpRoute("sections", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            sectionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = sectionsController.ControllerContext;
            var actionDescriptor = sectionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                sectionCoordinationServiceMock.Setup(svc => svc.PostSection4Async(It.IsAny<Dtos.Section4>())).Throws<PermissionsException>();
                sectionCoordinationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create sections."));
                await sectionsController.PostHedmSection4Async(It.IsAny<Dtos.Section4>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

    }

    [TestClass]
    public class SectionsControllerTests_PutHedmSectionAsync_V8
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

        private IAdapterRegistry adapterRegistry;
        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
        private List<string> levels = new List<string>();

        private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

        private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
        private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
        private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

        private List<Dtos.Section4> allSectionDtos;
        private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
        private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;

            allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
            allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
            if (allCourseLevels != null)
                levels.Add(allCourseLevels[0].Code);

            adapterRegistry = adapterRegistryMock.Object;

            List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

            firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            firstSection.TermId = "2012/FA";
            firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
            firstSection.Guid = firstSectionGuid;
            sectionEntityList.Add(firstSection);

            secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
            secondSection.Guid = secondSectionGuid;
            sectionEntityList.Add(secondSection);

            thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
            thirdSection.EndDate = new DateTime(2011, 12, 21);
            thirdSection.Guid = thirdSectionGuid;
            sectionEntityList.Add(thirdSection);

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
            IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());

            // Mock section repo GetCachedSections
            bool bestFit = false;
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // Mock section repo GetNonCachedSections
            sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // mock DTO adapters
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

            academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

            allSectionDtos = new List<Dtos.Section4>();
            foreach (var sectionEntity in sectionEntityList)
            {
                //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section4>(adapterRegistry, logger);
                Ellucian.Colleague.Dtos.Section4 target = sectionDtoAdapter.MapToType(sectionEntity);
                target.Id = sectionEntity.Guid;
                Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                if (sectionEntity.CourseLevelCodes != null)
                {
                    var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                    target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                }
                allSectionDtos.Add(target);
            }

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            sectionsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid)));
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_Exception()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<Exception>();
            await sectionsController.PutHedmSection4Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_ConfigurationException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<ConfigurationException>();
            await sectionsController.PutHedmSection4Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_PermissionException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<PermissionsException>();
            await sectionsController.PutHedmSection4Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_RepoException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<RepositoryException>();
            await sectionsController.PutHedmSection4Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_ArgException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<ArgumentException>();
            await sectionsController.PutHedmSection4Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_NilGUID_ArgException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<ArgumentException>();
            await sectionsController.PutHedmSection4Async(Guid.Empty.ToString(), section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_SectionIdNull_ArgException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<ArgumentException>();
            await sectionsController.PutHedmSection4Async("1234", new Dtos.Section4() { Id = "" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_IntegrationApiException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>()))
                .Throws<IntegrationApiException>();
            await sectionsController.PutHedmSection4Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_NullIDException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            await sectionsController.PutHedmSection4Async(null, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_NullSectionException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            await sectionsController.PutHedmSection4Async(firstSectionGuid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSectionAsync_InvalidException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            await sectionsController.PutHedmSection4Async(secondSectionGuid, section);
        }

        [TestMethod]
        public async Task SectionsController_PutHedmSectionAsync()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(section.Id)).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>())).ReturnsAsync(section);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sectionByGuid = await sectionsController.PutHedmSection4Async(firstSectionGuid, section);
            Assert.AreEqual(section.Id, sectionByGuid.Id);
            Assert.AreEqual(section.Title, sectionByGuid.Title);
            Assert.AreEqual(section.Description, sectionByGuid.Description);
            Assert.AreEqual(academicLevel.Guid, sectionByGuid.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, sectionByGuid.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, sectionByGuid.StartOn);
            Assert.AreEqual(section.EndOn, sectionByGuid.EndOn);
            if (course != null && sectionByGuid.Course != null)
                Assert.AreEqual(course.Guid, sectionByGuid.Course.Id);
            Assert.AreEqual(section.Number, sectionByGuid.Number);
            Assert.AreEqual(section.MaximumEnrollment, sectionByGuid.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, sectionByGuid.CourseLevels[0].Id);
        }

        //PUT V8
        //Successful
        //PutHedmSection4Async

        [TestMethod]
        public async Task SectionsController_PutHedmSection4Async_Permissions()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Sections" },
                        { "action", "PutHedmSection4Async" }
                    };
            HttpRoute route = new HttpRoute("sections", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            sectionsController.Request.SetRouteData(data);
            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateAndUpdateSection);

            var controllerContext = sectionsController.ControllerContext;
            var actionDescriptor = sectionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            sectionCoordinationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            sectionCoordinationServiceMock.Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>())).ReturnsAsync(section);
            var sectionByGuid = await sectionsController.PutHedmSection4Async(firstSectionGuid, section);

            Object filterObject;
            sectionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateAndUpdateSection));

        }

        //Put V8
        //Exception
        //PutHedmSection4Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task sectionsController_PutHedmSection4Async_Invalid_Permissions()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                    {
                        { "controller", "Sections" },
                        { "action", "PutHedmSection4Async" }
                    };
            HttpRoute route = new HttpRoute("sections", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            sectionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = sectionsController.ControllerContext;
            var actionDescriptor = sectionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                sectionCoordinationServiceMock.Setup(svc => svc.PutSection4Async(It.IsAny<Dtos.Section4>())).Throws<PermissionsException>();
                sectionCoordinationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update sections."));
                await sectionsController.PutHedmSection4Async(section.Id, section);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


    }

    /*
    [TestClass]
    public class SectionsControllerTests_PostHedmSectionAsync_V13
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

        private IAdapterRegistry adapterRegistry;
        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
        private List<string> levels = new List<string>();

        private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

        private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
        private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
        private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";


        private List<Dtos.Section6> allSectionDtos;
        private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
        private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;

            allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
            allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
            if (allCourseLevels != null)
                levels.Add(allCourseLevels[0].Code);

            adapterRegistry = adapterRegistryMock.Object;

            List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

            firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            firstSection.TermId = "2012/FA";
            firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
            firstSection.Guid = firstSectionGuid;
            sectionEntityList.Add(firstSection);

            secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
            secondSection.Guid = secondSectionGuid;
            sectionEntityList.Add(secondSection);

            thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
            thirdSection.EndDate = new DateTime(2011, 12, 21);
            thirdSection.Guid = thirdSectionGuid;
            sectionEntityList.Add(thirdSection);

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
            IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());

            // Mock section repo GetCachedSections
            bool bestFit = false;
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // Mock section repo GetNonCachedSections
            sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // mock DTO adapters
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

            academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

            allSectionDtos = new List<Dtos.Section6>();
            foreach (var sectionEntity in sectionEntityList)
            {
                //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section6>(adapterRegistry, logger);
                Ellucian.Colleague.Dtos.Section6 target = sectionDtoAdapter.MapToType(sectionEntity);
                target.Id = sectionEntity.Guid;
                Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                if (sectionEntity.CourseLevelCodes != null)
                {
                    var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                    target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                }
                allSectionDtos.Add(target);
            }

            // mock controller
            sectionsController = new SectionsController(adapterRegistry, sectionRepository, sectionCoordinationService, null, null, null,logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSection6Async_Exception()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<Exception>();
            await sectionsController.PostHedmSection6Async(It.IsAny<Dtos.Section6>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSection6Async_ConfigurationException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<ConfigurationException>();
            await sectionsController.PostHedmSection6Async(It.IsAny<Dtos.Section6>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSection6Async_PermissionException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<PermissionsException>();
            await sectionsController.PostHedmSection6Async(It.IsAny<Dtos.Section6>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSection6Async_ArgNullException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection6Async(null))
                .Throws<ArgumentNullException>();
            await sectionsController.PostHedmSection6Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSection6Async_RepoException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<RepositoryException>();
            await sectionsController.PostHedmSection6Async(It.IsAny<Dtos.Section6>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSection6Async_IntegrationApiException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<IntegrationApiException>();
            await sectionsController.PostHedmSection6Async(It.IsAny<Dtos.Section6>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PostHedmSection6Async_ID_Null()
        {
            sectionCoordinationServiceMock
                .Setup(svc => svc.PostSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<IntegrationApiException>();
            await sectionsController.PostHedmSection6Async(new Dtos.Section6() { Id = string.Empty });
        }

        [TestMethod]
        public async Task SectionsController_PostHedmSection6Async()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            section.Id = Guid.Empty.ToString();
            sectionCoordinationServiceMock.Setup(svc => svc.PostSection6Async(section)).ReturnsAsync(section);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sectionByGuid = await sectionsController.PostHedmSection6Async(section);
            Assert.AreEqual(section.Id, sectionByGuid.Id);
            Assert.AreEqual(section.Title, sectionByGuid.Title);
            Assert.AreEqual(section.Description, sectionByGuid.Description);
            Assert.AreEqual(academicLevel.Guid, sectionByGuid.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, sectionByGuid.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, sectionByGuid.StartOn);
            Assert.AreEqual(section.EndOn, sectionByGuid.EndOn);
            if (course != null && sectionByGuid.Course != null)
                Assert.AreEqual(course.Guid, sectionByGuid.Course.Id);
            Assert.AreEqual(section.Number, sectionByGuid.Number);
            Assert.AreEqual(section.MaximumEnrollment, sectionByGuid.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, sectionByGuid.CourseLevels[0].Id);

        }
    }
    */

    /*
    [TestClass]
    public class SectionsControllerTests_PutHedmSectionAsync_V13
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

        private IAdapterRegistry adapterRegistry;
        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
        private List<string> levels = new List<string>();

        private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

        private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
        private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
        private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

        private List<Dtos.Section6> allSectionDtos;
        private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
        private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;

            allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
            allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
            if (allCourseLevels != null)
                levels.Add(allCourseLevels[0].Code);

            adapterRegistry = adapterRegistryMock.Object;

            List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

            firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            firstSection.TermId = "2012/FA";
            firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
            firstSection.Guid = firstSectionGuid;
            sectionEntityList.Add(firstSection);

            secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
            secondSection.Guid = secondSectionGuid;
            sectionEntityList.Add(secondSection);

            thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
            thirdSection.EndDate = new DateTime(2011, 12, 21);
            thirdSection.Guid = thirdSectionGuid;
            sectionEntityList.Add(thirdSection);

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
            IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());

            // Mock section repo GetCachedSections
            bool bestFit = false;
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // Mock section repo GetNonCachedSections
            sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // mock DTO adapters
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

            academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

            allSectionDtos = new List<Dtos.Section6>();
            foreach (var sectionEntity in sectionEntityList)
            {
                //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section6>(adapterRegistry, logger);
                Ellucian.Colleague.Dtos.Section6 target = sectionDtoAdapter.MapToType(sectionEntity);
                target.Id = sectionEntity.Guid;
                Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                if (sectionEntity.CourseLevelCodes != null)
                {
                    var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                    target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                }
                allSectionDtos.Add(target);
            }

            // mock controller
            sectionsController = new SectionsController(adapterRegistry, sectionRepository, sectionCoordinationService, null, null, null,logger)
            {
                Request = new HttpRequestMessage()
            };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            sectionsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid)));
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_Exception()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<Exception>();
            await sectionsController.PutHedmSection6Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_ConfigurationException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<ConfigurationException>();
            await sectionsController.PutHedmSection6Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_PermissionException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<PermissionsException>();
            await sectionsController.PutHedmSection6Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_RepoException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<RepositoryException>();
            await sectionsController.PutHedmSection6Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_ArgException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<ArgumentException>();
            await sectionsController.PutHedmSection6Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_NilGUID_ArgException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<ArgumentException>();
            await sectionsController.PutHedmSection6Async(Guid.Empty.ToString(), section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_SectionIdNull_ArgException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<ArgumentException>();
            await sectionsController.PutHedmSection6Async("1234", new Dtos.Section6() { Id = "" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_IntegrationApiException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.PutSection6Async(It.IsAny<Dtos.Section6>()))
                .Throws<IntegrationApiException>();
            await sectionsController.PutHedmSection6Async(section.Id, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_NullIDException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            await sectionsController.PutHedmSection6Async(null, section);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_NullSectionException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            await sectionsController.PutHedmSection6Async(firstSectionGuid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_PutHedmSection6Async_InvalidException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            await sectionsController.PutHedmSection6Async(secondSectionGuid, section);
        }

        [TestMethod]
        public async Task SectionsController_PutHedmSection6Async()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection6ByGuidAsync(section.Id, It.IsAny<bool>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.PutSection6Async(It.IsAny<Section6>())).ReturnsAsync(section);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sectionByGuid = await sectionsController.PutHedmSection6Async(firstSectionGuid, section);
            Assert.AreEqual(section.Id, sectionByGuid.Id);
            Assert.AreEqual(section.Title, sectionByGuid.Title);
            Assert.AreEqual(section.Description, sectionByGuid.Description);
            Assert.AreEqual(academicLevel.Guid, sectionByGuid.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, sectionByGuid.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, sectionByGuid.StartOn);
            Assert.AreEqual(section.EndOn, sectionByGuid.EndOn);
            if (course != null && sectionByGuid.Course != null)
                Assert.AreEqual(course.Guid, sectionByGuid.Course.Id);
            Assert.AreEqual(section.Number, sectionByGuid.Number);
            Assert.AreEqual(section.MaximumEnrollment, sectionByGuid.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, sectionByGuid.CourseLevels[0].Id);
        }
    }
    */

    [TestClass]
    public class SectionsControllerTestsGetHedmSectionById_V6
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

        private IAdapterRegistry adapterRegistry;
        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
        private List<string> levels = new List<string>();

        private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

        private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
        private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
        private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

        private Ellucian.Colleague.Domain.Student.Entities.Student firstStudent;
        private Ellucian.Colleague.Domain.Student.Entities.Student secondStudent;
        private Ellucian.Colleague.Domain.Student.Entities.Student thirdStudent;

        private List<Dtos.Section4> allSectionDtos;
        private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
        private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;

            allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
            allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
            if (allCourseLevels != null)
                levels.Add(allCourseLevels[0].Code);

            firstStudent = new Domain.Student.Entities.Student("STU1", "Gamgee", null, new List<string>(), new List<string>());
            firstStudent.FirstName = "Samwise";
            secondStudent = new Domain.Student.Entities.Student("STU2", "Took", null, new List<string>(), new List<string>());
            secondStudent.FirstName = "Peregrin";
            thirdStudent = new Domain.Student.Entities.Student("STU1", "Baggins", null, new List<string>(), new List<string>());
            thirdStudent.FirstName = "Frodo"; thirdStudent.MiddleName = "Ring-bearer";

            adapterRegistry = adapterRegistryMock.Object;

            List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

            firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            firstSection.TermId = "2012/FA";
            firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
            firstSection.Guid = firstSectionGuid;
            sectionEntityList.Add(firstSection);

            secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
            secondSection.Guid = secondSectionGuid;
            sectionEntityList.Add(secondSection);

            thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
            thirdSection.EndDate = new DateTime(2011, 12, 21);
            thirdSection.Guid = thirdSectionGuid;
            sectionEntityList.Add(thirdSection);

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
            IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());


            // Mock section repo GetCachedSections
            bool bestFit = false;
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // Mock section repo GetNonCachedSections
            sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // mock DTO adapters
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

            academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

            allSectionDtos = new List<Dtos.Section4>();
            foreach (var sectionEntity in sectionEntityList)
            {
                //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section4>(adapterRegistry, logger);
                Ellucian.Colleague.Dtos.Section4 target = sectionDtoAdapter.MapToType(sectionEntity);
                target.Id = sectionEntity.Guid;
                Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                if (sectionEntity.CourseLevelCodes != null)
                {
                    var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                    target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                }
                allSectionDtos.Add(target);
            }


            sectionCoordinationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid_Exception()
        {
            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await sectionsController.GetHedmSectionByGuid2Async(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid_ConfigurationException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new ConfigurationException());
            await sectionsController.GetHedmSectionByGuid2Async(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid_PermissionException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new PermissionsException());
            await sectionsController.GetHedmSectionByGuid2Async(It.IsAny<string>());
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid_ArgNullException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentNullException());
            await sectionsController.GetHedmSectionByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid_RepoException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new RepositoryException());
            await sectionsController.GetHedmSectionByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid_IntegrationApiException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new IntegrationApiException());
            await sectionsController.GetHedmSectionByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid_ConfigurationExceptionException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new ConfigurationException());
            await sectionsController.GetHedmSectionByGuid2Async("");
        }

        [TestMethod]
        public async Task SectionsController_GetHedmSectionByGuid()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sectionByGuid = await sectionsController.GetHedmSectionByGuid3Async(firstSectionGuid);
            Assert.AreEqual(section.Id, sectionByGuid.Id);
            Assert.AreEqual(section.Title, sectionByGuid.Title);
            Assert.AreEqual(section.Description, sectionByGuid.Description);
            Assert.AreEqual(academicLevel.Guid, sectionByGuid.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, sectionByGuid.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, sectionByGuid.StartOn);
            Assert.AreEqual(section.EndOn, sectionByGuid.EndOn);
            if (course != null && sectionByGuid.Course != null)
                Assert.AreEqual(course.Guid, sectionByGuid.Course.Id);
            Assert.AreEqual(section.Number, sectionByGuid.Number);
            Assert.AreEqual(section.MaximumEnrollment, sectionByGuid.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, sectionByGuid.CourseLevels[0].Id);

        }

    }

    [TestClass]
    public class SectionsControllerTestsDeleteHedmSectionById_V6
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

        private IAdapterRegistry adapterRegistry;
        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
        private List<string> levels = new List<string>();

        private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

        private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
        private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
        private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

        private Ellucian.Colleague.Domain.Student.Entities.Student firstStudent;
        private Ellucian.Colleague.Domain.Student.Entities.Student secondStudent;
        private Ellucian.Colleague.Domain.Student.Entities.Student thirdStudent;

        private List<Dtos.Section4> allSectionDtos;
        private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
        private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;

            allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
            allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
            if (allCourseLevels != null)
                levels.Add(allCourseLevels[0].Code);

            firstStudent = new Domain.Student.Entities.Student("STU1", "Gamgee", null, new List<string>(), new List<string>());
            firstStudent.FirstName = "Samwise";
            secondStudent = new Domain.Student.Entities.Student("STU2", "Took", null, new List<string>(), new List<string>());
            secondStudent.FirstName = "Peregrin";
            thirdStudent = new Domain.Student.Entities.Student("STU1", "Baggins", null, new List<string>(), new List<string>());
            thirdStudent.FirstName = "Frodo"; thirdStudent.MiddleName = "Ring-bearer";

            adapterRegistry = adapterRegistryMock.Object;

            List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

            firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            firstSection.TermId = "2012/FA";
            firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
            firstSection.Guid = firstSectionGuid;
            sectionEntityList.Add(firstSection);

            secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
            secondSection.Guid = secondSectionGuid;
            sectionEntityList.Add(secondSection);

            thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
            thirdSection.EndDate = new DateTime(2011, 12, 21);
            thirdSection.Guid = thirdSectionGuid;
            sectionEntityList.Add(thirdSection);

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
            IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());


            // Mock section repo GetCachedSections
            bool bestFit = false;
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // Mock section repo GetNonCachedSections
            sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // mock DTO adapters
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

            academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

            allSectionDtos = new List<Dtos.Section4>();
            foreach (var sectionEntity in sectionEntityList)
            {
                //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section4>(adapterRegistry, logger);
                Ellucian.Colleague.Dtos.Section4 target = sectionDtoAdapter.MapToType(sectionEntity);
                target.Id = sectionEntity.Guid;
                Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                if (sectionEntity.CourseLevelCodes != null)
                {
                    var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                    target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                }
                allSectionDtos.Add(target);
            }


            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_Exception()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_ConfigurationException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new ConfigurationException());
            await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_PermissionException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new PermissionsException());
            await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_ArgNullException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentNullException());
            await sectionsController.DeleteHedmSectionByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_ArgException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException());
            await sectionsController.DeleteHedmSectionByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_RepoException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new RepositoryException());
            await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_IntegrationApiException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new IntegrationApiException());
            await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_ConfigurationExceptionException()
        {
            sectionCoordinationServiceMock
                .Setup(c => c.GetSection4ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new ConfigurationException());
            await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_GuidException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            section.Id = secondSectionGuid;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(firstSectionGuid)).ReturnsAsync(section);
            await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_DeleteHedmSectionByGuid_NullException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(firstSectionGuid)).ReturnsAsync(() => null);
            await sectionsController.DeleteHedmSectionByGuid2Async(firstSectionGuid);
        }


    }

    [TestClass]
    public class SectionsControllerTestsGetHedmSection_V6
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

        private IAdapterRegistry adapterRegistry;
        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
        private List<string> levels = new List<string>();

        private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

        private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
        private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
        private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

        private Ellucian.Colleague.Domain.Student.Entities.Student firstStudent;
        private Ellucian.Colleague.Domain.Student.Entities.Student secondStudent;
        private Ellucian.Colleague.Domain.Student.Entities.Student thirdStudent;

        private List<Dtos.Section4> allSectionDtos;
        private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
        private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;
        private Ellucian.Web.Http.Models.Paging paging = new Web.Http.Models.Paging(3, 0);

        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        private Ellucian.Web.Http.Models.QueryStringFilter searchableFilter = new Web.Http.Models.QueryStringFilter("searchable", "");
        private Ellucian.Web.Http.Models.QueryStringFilter keywordSearchFilter = new Web.Http.Models.QueryStringFilter("keywordSearch", "");
        private Ellucian.Web.Http.Models.QueryStringFilter subjectFilter = new Web.Http.Models.QueryStringFilter("subject", "");
        private Ellucian.Web.Http.Models.QueryStringFilter instructorFilter = new Web.Http.Models.QueryStringFilter("instructor", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));


            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;

            allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
            allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
            if (allCourseLevels != null)
                levels.Add(allCourseLevels[0].Code);

            firstStudent = new Domain.Student.Entities.Student("STU1", "Gamgee", null, new List<string>(), new List<string>());
            firstStudent.FirstName = "Samwise";
            secondStudent = new Domain.Student.Entities.Student("STU2", "Took", null, new List<string>(), new List<string>());
            secondStudent.FirstName = "Peregrin";
            thirdStudent = new Domain.Student.Entities.Student("STU1", "Baggins", null, new List<string>(), new List<string>());
            thirdStudent.FirstName = "Frodo"; thirdStudent.MiddleName = "Ring-bearer";

            adapterRegistry = adapterRegistryMock.Object;

            List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

            firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            firstSection.TermId = "2012/FA";
            firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
            firstSection.Guid = firstSectionGuid;
            sectionEntityList.Add(firstSection);

            secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
            secondSection.Guid = secondSectionGuid;
            sectionEntityList.Add(secondSection);

            thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
            thirdSection.EndDate = new DateTime(2011, 12, 21);
            thirdSection.Guid = thirdSectionGuid;
            sectionEntityList.Add(thirdSection);

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
            IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());


            // Mock section repo GetCachedSections
            bool bestFit = false;
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // Mock section repo GetNonCachedSections
            sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // mock DTO adapters
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

            academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

            allSectionDtos = new List<Dtos.Section4>();
            foreach (var sectionEntity in sectionEntityList)
            {
                //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section4>(adapterRegistry, logger);
                Ellucian.Colleague.Dtos.Section4 target = sectionDtoAdapter.MapToType(sectionEntity);
                target.Id = sectionEntity.Guid;
                Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                if (sectionEntity.CourseLevelCodes != null)
                {
                    var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                    target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                }
                allSectionDtos.Add(target);
            }

            sectionCoordinationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_Exception()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                .Throws<Exception>();
            await sectionsController.GetHedmSections2Async(paging, section.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_ConfigurationException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                    .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                .ThrowsAsync(new ConfigurationException());
            await sectionsController.GetHedmSections2Async(paging, section.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_PermissionException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                    .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                .ThrowsAsync(new PermissionsException());
            await sectionsController.GetHedmSections2Async(paging, section.Title);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_ArgNullException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                    .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                .ThrowsAsync(new ArgumentNullException());
            await sectionsController.GetHedmSections2Async(paging, section.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_RepoException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                    .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                .ThrowsAsync(new RepositoryException());
            await sectionsController.GetHedmSections2Async(paging, section.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_IntegrationApiException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                    .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                .ThrowsAsync(new IntegrationApiException());
            await sectionsController.GetHedmSections2Async(paging, section.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_ConfigurationExceptionException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionCoordinationServiceMock
                    .Setup(svc => svc.GetSections2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", "", "", "", "", ""))
                .ThrowsAsync(new ConfigurationException());
            await sectionsController.GetHedmSections2Async(paging, section.Title);
        }

        [TestMethod]
        public async Task SectionsController_GetHedmSection4ByTitle_withBinder()
        {
            var filterGroupName = "criteria";

            var cancelToken = new System.Threading.CancellationToken(false);
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            Dictionary<String, String> criterion = new Dictionary<string, string>();
            criterion.Add("title", section.Title);
            var criteriaJson = Newtonsoft.Json.JsonConvert.SerializeObject(criterion);

            var binder = new QueryStringFilter(filterGroupName, criteriaJson);

            var mockactioncontext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage()
                },
                ActionArguments = { { filterGroupName, binder } }
            };
            mockactioncontext.ControllerContext.Configuration = new HttpConfiguration();
            mockactioncontext.ControllerContext.Configuration.Formatters.Add(new JsonMediaTypeFormatter());

            var filter = new QueryStringFilterFilter(filterGroupName, typeof(Dtos.Filters.SectionFilter));
            await filter.OnActionExecutingAsync(mockactioncontext, cancelToken);

            sectionsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };
            sectionsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.Filters.SectionFilter() { Title = section.Title });

            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", It.IsAny<List<string>>(), "", "", "", It.IsAny<List<string>>(), "", It.IsAny<string>(), SectionsSearchable.NotSet, It.IsAny<string>()))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            criteriaFilter.JsonQuery = criteriaJson;
            var sectionByTitle = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionByTitle.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(sectionByTitle is IHttpActionResult);

            Assert.AreEqual(section.Id, result.Id);
            Assert.AreEqual(section.Title, result.Title);
            Assert.AreEqual(section.Description, result.Description);
            Assert.AreEqual(academicLevel.Guid, result.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, result.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, result.StartOn);
            Assert.AreEqual(section.EndOn, result.EndOn);
            if (course != null && result.Course != null)
                Assert.AreEqual(course.Guid, result.Course.Id);
            Assert.AreEqual(section.Number, result.Number);
            Assert.AreEqual(section.MaximumEnrollment, result.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, result.CourseLevels[0].Id);

        }

        [TestMethod]
        public async Task SectionsController_GetHedmSection4ByTitle()
        {
            var filterGroupName = "criteria";

            var cancelToken = new System.Threading.CancellationToken(false);
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };
            sectionsController.Request.Properties.Add(
                string.Format("FilterObject{0}", filterGroupName),
                new Dtos.Filters.SectionFilter() { Title = section.Title });

            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), section.Title, "", "", "", "", "", "", It.IsAny<List<string>>(), "", "", "", It.IsAny<List<string>>(), "", It.IsAny<string>(), SectionsSearchable.NotSet, It.IsAny<string>()))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sectionByTitle = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionByTitle.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(sectionByTitle is IHttpActionResult);

            Assert.AreEqual(section.Id, result.Id);
            Assert.AreEqual(section.Title, result.Title);
            Assert.AreEqual(section.Description, result.Description);
            Assert.AreEqual(academicLevel.Guid, result.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, result.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, result.StartOn);
            Assert.AreEqual(section.EndOn, result.EndOn);
            if (course != null && result.Course != null)
                Assert.AreEqual(course.Guid, result.Course.Id);
            Assert.AreEqual(section.Number, result.Number);
            Assert.AreEqual(section.MaximumEnrollment, result.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, result.CourseLevels[0].Id);

        }


        [TestMethod]
        public async Task SectionsController_GetHedmSection4ByStatus()
        {
            var filterGroupName = "criteria";

            var cancelToken = new System.Threading.CancellationToken(false);
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };
            sectionsController.Request.Properties.Add(
                string.Format("FilterObject{0}", filterGroupName),
                new Dtos.Filters.SectionFilter() { Status = new Dtos.DtoProperties.SectionStatusDtoProperty() { Category = SectionStatus2.Open } });

            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc =>
                svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc =>
                svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), SectionStatus2.Open.ToString(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), SectionsSearchable.NotSet, It.IsAny<string>()))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sections = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sections.ExecuteAsync(cancelToken);

            var results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(sections is IHttpActionResult);
            Assert.AreEqual(section.Id, result.Id);
        }

        [TestMethod]
        public async Task SectionsController_GetHedmSectionBy4_StartOnFilter()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            var filterGroupName = "criteria";
            section.StartOn = new DateTimeOffset(DateTime.Now);
            sectionsController.Request.Properties.Add(
             string.Format("FilterObject{0}", filterGroupName),
             new Dtos.Filters.SectionFilter() { StartOn = section.StartOn }
             );

            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>(), It.IsAny<string>(), SectionsSearchable.NotSet, It.IsAny<string>()))
                .ReturnsAsync(tuple);

            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sections = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sections.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;
            var result = results.FirstOrDefault();

            Assert.IsTrue(sections is IHttpActionResult);
            Assert.AreEqual(section.Id, result.Id);
        }

        [TestMethod]
        public async Task SectionsController_GetHedmSectionBy4_EndOnFilter()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);

            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            var filterGroupName = "criteria";
            section.EndOn = new DateTimeOffset(DateTime.Now);
            sectionsController.Request.Properties.Add(
             string.Format("FilterObject{0}", filterGroupName),
             new Dtos.Filters.SectionFilter() { EndOn = section.EndOn }
             );

            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>(), It.IsAny<string>(), SectionsSearchable.NotSet, It.IsAny<string>()))
                .ReturnsAsync(tuple);

            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);
            var sections = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sections.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;
            var result = results.FirstOrDefault();

            Assert.IsTrue(sections is IHttpActionResult);
            Assert.AreEqual(section.Id, result.Id);
        }

        [TestMethod]
        public async Task SectionsController_GetHedmSectionBy4_CourseFilter()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            var filterGroupName = "criteria";
            sectionsController.Request.Properties.Add(
             string.Format("FilterObject{0}", filterGroupName),
             new Dtos.Filters.SectionFilter() { Course = new GuidObject2(section.Course.Id) });

            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>(), It.IsAny<string>(), SectionsSearchable.NotSet, It.IsAny<string>()))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

            var sections = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sections.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;
            var result = results.FirstOrDefault();

            Assert.IsTrue(sections is IHttpActionResult);
            Assert.AreEqual(section.Id, result.Id);
        }

        [TestMethod]
        public async Task SectionsController_GetHedmSections4_Searchable_No()
        {
            var filterGroupName = "searchable";
            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            sectionsController.Request.Properties.Add(
              string.Format("FilterObject{0}", filterGroupName),
              new Dtos.Filters.SearchableFilter() { Search = SectionsSearchable.No });

            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", It.IsAny<List<string>>(), "", "", "", It.IsAny<List<string>>(), "", It.IsAny<string>(), SectionsSearchable.No, It.IsAny<string>()))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sectionByTitle = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionByTitle.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(sectionByTitle is IHttpActionResult);

            Assert.AreEqual(section.Id, result.Id);
            Assert.AreEqual(section.Title, result.Title);
            Assert.AreEqual(section.Description, result.Description);
            Assert.AreEqual(academicLevel.Guid, result.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, result.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, result.StartOn);
            Assert.AreEqual(section.EndOn, result.EndOn);
            if (course != null && result.Course != null)
                Assert.AreEqual(course.Guid, result.Course.Id);
            Assert.AreEqual(section.Number, result.Number);
            Assert.AreEqual(section.MaximumEnrollment, result.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, result.CourseLevels[0].Id);

        }

        [TestMethod]
        public async Task SectionsController_GetHedmSections4_Searchable_Yes()
        {
            var filterGroupName = "searchable";
            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            sectionsController.Request.Properties.Add(
             string.Format("FilterObject{0}", filterGroupName),
             new Dtos.Filters.SearchableFilter() { Search = SectionsSearchable.Yes });


            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "", "", "", It.IsAny<List<string>>(), "", "", "", It.IsAny<List<string>>(), "", It.IsAny<string>(), SectionsSearchable.Yes, It.IsAny<string>()))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);
            var sectionByTitle = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionByTitle.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(sectionByTitle is IHttpActionResult);

            Assert.AreEqual(section.Id, result.Id);
            Assert.AreEqual(section.Title, result.Title);
            Assert.AreEqual(section.Description, result.Description);
            Assert.AreEqual(academicLevel.Guid, result.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, result.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, result.StartOn);
            Assert.AreEqual(section.EndOn, result.EndOn);
            if (course != null && result.Course != null)
                Assert.AreEqual(course.Guid, result.Course.Id);
            Assert.AreEqual(section.Number, result.Number);
            Assert.AreEqual(section.MaximumEnrollment, result.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, result.CourseLevels[0].Id);

        }

        [TestMethod]
        public async Task SectionsController_GetHedmSections4_Searchable_Hidden()
        {
            var filterGroupName = "searchable";

            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            sectionsController.Request.Properties.Add(
             string.Format("FilterObject{0}", filterGroupName),
             new Dtos.Filters.SearchableFilter() { Search = SectionsSearchable.Hidden });

            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", It.IsAny<List<string>>(), "", "", "", It.IsAny<List<string>>(), "", It.IsAny<string>(), SectionsSearchable.Hidden, It.IsAny<string>()))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            var sectionByTitle = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionByTitle.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(sectionByTitle is IHttpActionResult);

            Assert.AreEqual(section.Id, result.Id);
            Assert.AreEqual(section.Title, result.Title);
            Assert.AreEqual(section.Description, result.Description);
            Assert.AreEqual(academicLevel.Guid, result.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, result.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, result.StartOn);
            Assert.AreEqual(section.EndOn, result.EndOn);
            if (course != null && result.Course != null)
                Assert.AreEqual(course.Guid, result.Course.Id);
            Assert.AreEqual(section.Number, result.Number);
            Assert.AreEqual(section.MaximumEnrollment, result.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, result.CourseLevels[0].Id);

        }

        [TestMethod]
        public async Task SectionsController_GetHedmSections4_Keyword()
        {

            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var filterGroupName = "keywordSearch";
            sectionsController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               new Dtos.Filters.KeywordSearchFilter() { Search = "Math" });

            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var tuple = new Tuple<IEnumerable<Dtos.Section4>, int>(new List<Dtos.Section4>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection4ByGuidAsync(It.IsAny<string>())).ReturnsAsync(section);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(It.IsAny<int>(), It.IsAny<int>(), "", "", "", "", "", "", "", It.IsAny<List<string>>(), "", "", "", It.IsAny<List<string>>(), "", It.IsAny<string>(), SectionsSearchable.NotSet, "Math"))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);


            var sectionByTitle = await sectionsController.GetHedmSections4Async(paging, criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionByTitle.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section4>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(sectionByTitle is IHttpActionResult);

            Assert.AreEqual(section.Id, result.Id);
            Assert.AreEqual(section.Title, result.Title);
            Assert.AreEqual(section.Description, result.Description);
            Assert.AreEqual(academicLevel.Guid, result.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, result.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, result.StartOn);
            Assert.AreEqual(section.EndOn, result.EndOn);
            if (course != null && result.Course != null)
                Assert.AreEqual(course.Guid, result.Course.Id);
            Assert.AreEqual(section.Number, result.Number);
            Assert.AreEqual(section.MaximumEnrollment, result.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, result.CourseLevels[0].Id);

        }
    }

    [TestClass]
    public class SectionsControllerTestsGetHedmSection_V16
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

        private IAdapterRegistry adapterRegistry;
        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private List<Domain.Student.Entities.OfferingDepartment> dpts = new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ART", 100m) };
        private List<string> levels = new List<string>();

        private Ellucian.Colleague.Domain.Student.Entities.Section firstSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section secondSection;
        private Ellucian.Colleague.Domain.Student.Entities.Section thirdSection;

        private string firstSectionGuid = "eab43b50-ce12-4e7f-8947-2d63661ab7ac";
        private string secondSectionGuid = "51adca90-4839-442b-b22a-12c8009b1186";
        private string thirdSectionGuid = "cd74617e-d304-4c6d-8598-6622ed15192a";

        private Ellucian.Colleague.Domain.Student.Entities.Student firstStudent;
        private Ellucian.Colleague.Domain.Student.Entities.Student secondStudent;
        private Ellucian.Colleague.Domain.Student.Entities.Student thirdStudent;

        private List<Dtos.Section6> allSectionDtos;
        private List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> academicLevels;
        private List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeSchemes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;
        private Ellucian.Web.Http.Models.Paging paging = new Web.Http.Models.Paging(3, 0);

        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        private Ellucian.Web.Http.Models.QueryStringFilter searchableFilter = new Web.Http.Models.QueryStringFilter("searchable", "");
        private Ellucian.Web.Http.Models.QueryStringFilter keywordSearchFilter = new Web.Http.Models.QueryStringFilter("keywordSearch", "");
        private Ellucian.Web.Http.Models.QueryStringFilter subjectFilter = new Web.Http.Models.QueryStringFilter("subject", "");
        private Ellucian.Web.Http.Models.QueryStringFilter instructorFilter = new Web.Http.Models.QueryStringFilter("instructor", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));


            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;

            allCourses = new TestCourseRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.Course>;
            allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
            if (allCourseLevels != null)
                levels.Add(allCourseLevels[0].Code);

            firstStudent = new Domain.Student.Entities.Student("STU1", "Gamgee", null, new List<string>(), new List<string>());
            firstStudent.FirstName = "Samwise";
            secondStudent = new Domain.Student.Entities.Student("STU2", "Took", null, new List<string>(), new List<string>());
            secondStudent.FirstName = "Peregrin";
            thirdStudent = new Domain.Student.Entities.Student("STU1", "Baggins", null, new List<string>(), new List<string>());
            thirdStudent.FirstName = "Frodo"; thirdStudent.MiddleName = "Ring-bearer";

            adapterRegistry = adapterRegistryMock.Object;

            List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionEntityList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-6)) };

            firstSection = new Ellucian.Colleague.Domain.Student.Entities.Section("1", allCourses[0].Name, "01", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            firstSection.TermId = "2012/FA";
            firstSection.EndDate = new DateTime(2012, 12, 21); firstSection.AddActiveStudent("STU1");
            firstSection.Guid = firstSectionGuid;
            sectionEntityList.Add(firstSection);

            secondSection = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "1119", "02", new DateTime(2012, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            secondSection.TermId = "2012/FA"; secondSection.AddActiveStudent("STU1"); secondSection.AddActiveStudent("STU2");
            secondSection.Guid = secondSectionGuid;
            sectionEntityList.Add(secondSection);

            thirdSection = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "1119", "03", new DateTime(2011, 09, 01), 3.0m, 0, "Introduction to Art", "IN", dpts, levels, "UG", statuses);
            thirdSection.TermId = "2011/FA"; thirdSection.AddActiveStudent("STU1"); thirdSection.AddActiveStudent("STU2"); thirdSection.AddActiveStudent("STU3");
            thirdSection.EndDate = new DateTime(2011, 12, 21);
            thirdSection.Guid = thirdSectionGuid;
            sectionEntityList.Add(thirdSection);

            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = sectionEntityList;
            IEnumerable<string> listOfIds = new List<string>() { "1", "2", "3" };

            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("99")).Throws(new ArgumentException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("98")).Throws(new PermissionsException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionRosterAsync("")).Throws(new ArgumentNullException());


            // Mock section repo GetCachedSections
            bool bestFit = false;
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // Mock section repo GetNonCachedSections
            sectionRepoMock.Setup(repo => repo.GetNonCachedSectionsAsync(listOfIds, bestFit)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sections));
            // mock DTO adapters
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionAdapter);

            academicLevels = new TestAcademicLevelRepository().GetAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel>;
            allGradeSchemes = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result as List<Ellucian.Colleague.Domain.Student.Entities.GradeScheme>;

            allSectionDtos = new List<Dtos.Section6>();
            foreach (var sectionEntity in sectionEntityList)
            {
                //Ellucian.Colleague.Dtos.Section target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section>(entity);
                var sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Section6>(adapterRegistry, logger);
                Ellucian.Colleague.Dtos.Section6 target = sectionDtoAdapter.MapToType(sectionEntity);
                target.Id = sectionEntity.Guid;
                Ellucian.Colleague.Domain.Student.Entities.AcademicLevel academicLevel = academicLevels.FirstOrDefault(x => x.Code == sectionEntity.AcademicLevelCode);
                target.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = academicLevel.Guid } };

                Ellucian.Colleague.Domain.Student.Entities.GradeScheme gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);

                target.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = gradeScheme.Guid } };

                var course = allCourses.FirstOrDefault(x => x.Name == sectionEntity.CourseId);
                if (course != null) target.Course = new Dtos.GuidObject2 { Id = course.Guid };

                if (sectionEntity.CourseLevelCodes != null)
                {
                    var courseLevel = allCourseLevels.FirstOrDefault(x => x.Code == sectionEntity.CourseLevelCodes[0]);
                    target.CourseLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = courseLevel.Guid } };
                }
                //V16
                target.Titles = new List<Dtos.DtoProperties.CoursesTitlesDtoProperty>()
                {
                    new Dtos.DtoProperties.CoursesTitlesDtoProperty()
                    {
                        Type = new GuidObject2("dab43b50-ce12-4e7f-8947-2d63661ab7ab"),
                        Value = "Value 1"
                    }
                };
                target.OwningInstitutionUnits = new List<OwningInstitutionUnit>()
                {
                    new OwningInstitutionUnit()
                    {
                         InstitutionUnit = new GuidObject2("dab43b50-ce12-4e7f-8947-2d63661ab7ab")
                    }
                };
                allSectionDtos.Add(target);
            }

            sectionCoordinationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_JsonReaderException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            List<string> list = new List<string>();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(0, 100, "", "", "", "", "", "", "", "", list, "", "", "", list, "", "",
                SectionsSearchable.NotSet, null, It.IsAny<bool>()))
               .ThrowsAsync(new JsonReaderException());
            await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_JsonSerializationException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            List<string> list = new List<string>();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(0, 100, "", "", "", "", "", "", "", "", list, "", "", "", list, "", "",
                SectionsSearchable.NotSet, null, It.IsAny<bool>()))
               .ThrowsAsync(new JsonSerializationException());
            await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_KeyNotFoundException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            List<string> list = new List<string>();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(0, 100, "", "", "", "", "", "", "", "", list, "", "", "", list, "", "",
                SectionsSearchable.NotSet, null, It.IsAny<bool>()))
               .ThrowsAsync(new KeyNotFoundException());
            await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_PermissionsException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            List<string> list = new List<string>();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(0, 100, "", "", "", "", "", "", "", "", list, "", "", "", list, "", "",
                SectionsSearchable.NotSet, null, It.IsAny<bool>()))
               .ThrowsAsync(new PermissionsException());
            await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_ArgumentException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            List<string> list = new List<string>();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(0, 100, "", "", "", "", "", "", "", "", list, "", "", "", list, "", "",
                SectionsSearchable.NotSet, null, It.IsAny<bool>()))
               .ThrowsAsync(new ArgumentException());
            await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_RepositoryException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            List<string> list = new List<string>();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(0, 100, "", "", "", "", "", "", "", "", list, "", "", "", list, "", "",
                SectionsSearchable.NotSet, null, It.IsAny<bool>()))
               .ThrowsAsync(new RepositoryException());
            await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_IntegrationApiException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            List<string> list = new List<string>();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(0, 100, "", "", "", "", "", "", "", "", list, "", "", "", list, "", "",
                SectionsSearchable.NotSet, null, It.IsAny<bool>()))
               .ThrowsAsync(new IntegrationApiException());
            await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSections_Exception()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            List<string> list = new List<string>();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(0, 100, "", "", "", "", "", "", "", "", list, "", "", "", list, "", "",
                SectionsSearchable.NotSet, null, It.IsAny<bool>()))
               .ThrowsAsync(new Exception());
            await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
        }

        [TestMethod]
        public async Task SectionsController_GetHedmSection6()
        {
            var filterGroupName = "criteria";
            sectionsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            sectionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
            var cancelToken = new System.Threading.CancellationToken(false);

            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            sectionsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName), section);
            var tuple = new Tuple<IEnumerable<Dtos.Section6>, int>(new List<Dtos.Section6>() { section }, 5);

            sectionCoordinationServiceMock.Setup(svc => svc.GetSections6Async(It.IsAny<int>(), It.IsAny<int>(), section.Titles[0].Value, "", "", "", section.Number, "", "", "",
                new List<string>() { section.AcademicLevels[0].Id }, section.Course.Id, "", "", new List<string>() { section.OwningInstitutionUnits[0].InstitutionUnit.Id }, "", It.IsAny<string>(),
                SectionsSearchable.NotSet, It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);
            var academicLevel = academicLevels.FirstOrDefault(x => x.Guid == section.AcademicLevels.FirstOrDefault().Id);

            var gradeScheme = allGradeSchemes.FirstOrDefault(x => x.Code == academicLevel.GradeScheme);
            Ellucian.Colleague.Domain.Student.Entities.Course course = null;
            if (section.Course != null)
                course = allCourses.FirstOrDefault(x => x.Guid == section.Course.Id);

            //criteriaFilter.JsonQuery = criteriaJson;
            var sectionByTitle = await sectionsController.GetHedmSections6Async(It.IsAny<Paging>(), criteriaFilter, searchableFilter, keywordSearchFilter, subjectFilter, instructorFilter);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionByTitle.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Section6> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Section6>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Section6>;

            var result = results.FirstOrDefault();

            Assert.IsTrue(sectionByTitle is IHttpActionResult);

            Assert.AreEqual(section.Id, result.Id);
            Assert.AreEqual(academicLevel.Guid, result.AcademicLevels.FirstOrDefault().Id);
            Assert.AreEqual(gradeScheme.Guid, result.GradeSchemes.FirstOrDefault().Id);
            Assert.AreEqual(section.StartOn, result.StartOn);
            Assert.AreEqual(section.EndOn, result.EndOn);
            if (course != null && result.Course != null)
                Assert.AreEqual(course.Guid, result.Course.Id);
            Assert.AreEqual(section.Number, result.Number);
            Assert.AreEqual(section.MaximumEnrollment, result.MaximumEnrollment);

            var courseLevel = allCourseLevels.FirstOrDefault(x => x.Guid == section.CourseLevels[0].Id);
            if (courseLevel != null)
                Assert.AreEqual(courseLevel.Guid, result.CourseLevels[0].Id);

        }

        [TestMethod]
        public async Task SectionsController_GetHedmSectionByGuid6Async()
        {
            sectionsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            sectionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var id = section.Id;

            sectionCoordinationServiceMock.Setup(svc => svc.GetSection6ByGuidAsync(id, It.IsAny<bool>()))
                .ReturnsAsync(section);

            var result = await sectionsController.GetHedmSectionByGuid6Async(id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid6Async_IdNull()
        {
            var result = await sectionsController.GetHedmSectionByGuid6Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid6Async_KeyNotFoundException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var id = section.Id;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSection6ByGuidAsync(id, It.IsAny<bool>()))
            .ThrowsAsync(new KeyNotFoundException());

            var result = await sectionsController.GetHedmSectionByGuid6Async(id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid6Async_PermissionsException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var id = section.Id;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSection6ByGuidAsync(id, It.IsAny<bool>()))
            .ThrowsAsync(new PermissionsException());

            var result = await sectionsController.GetHedmSectionByGuid6Async(id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid6Async_ArgumentException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var id = section.Id;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSection6ByGuidAsync(id, It.IsAny<bool>()))
            .ThrowsAsync(new ArgumentException());

            var result = await sectionsController.GetHedmSectionByGuid6Async(id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid6Async_RepositoryException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var id = section.Id;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSection6ByGuidAsync(id, It.IsAny<bool>()))
            .ThrowsAsync(new RepositoryException());

            var result = await sectionsController.GetHedmSectionByGuid6Async(id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid6Async_IntegrationApiException()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var id = section.Id;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSection6ByGuidAsync(id, It.IsAny<bool>()))
            .ThrowsAsync(new IntegrationApiException());

            var result = await sectionsController.GetHedmSectionByGuid6Async(id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetHedmSectionByGuid6Async_Exception()
        {
            var section = allSectionDtos.FirstOrDefault(x => x.Id == firstSectionGuid);
            var id = section.Id;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSection6ByGuidAsync(id, It.IsAny<bool>()))
            .ThrowsAsync(new Exception());

            var result = await sectionsController.GetHedmSectionByGuid6Async(id);
        }
    }

    [TestClass]
    public class SectionsControllerTests_GetSectionMeetingInstances
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

        private SectionsController sectionsController;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepository;
        private Mock<IStudentRepository> studentRepoMock;
        private IStudentRepository studentRepository;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private IEnumerable<SectionMeetingInstance> allMeetingDtos;
        private ILogger logger;


        [TestInitialize]
        public async void Initialize()
        {
            var testSectionRepo = new TestSectionRepository();
            var sectionMeetingInstanceEntities = await testSectionRepo.GetSectionMeetingInstancesAsync("1111111");
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>().Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepository = sectionRepoMock.Object;

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepository = studentRepoMock.Object;

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;


            allMeetingDtos = BuildSectionMeetingInstanceDtos(sectionMeetingInstanceEntities);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionMeetingInstancesAsync("1111111")).Returns(Task.FromResult<IEnumerable<SectionMeetingInstance>>(allMeetingDtos));
            var sectionMeetingInstanceEmpyResponse = new List<SectionMeetingInstance>().AsEnumerable();
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionMeetingInstancesAsync("2222222")).ThrowsAsync(new KeyNotFoundException());

            // mock DTO adapters
            var meetingAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionMeetingInstance, SectionMeetingInstance>(null, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionMeetingInstance, SectionMeetingInstance>()).Returns(meetingAdapter);

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionRepository = null;
            studentRepository = null;
            sectionCoordinationService = null;
        }

        [TestMethod]
        public async Task SectionsController_GetSectionMeetingInstancesAsync_ReturnResults()
        {

            var meetingResults = await sectionsController.GetSectionMeetingInstancesAsync("1111111");
            Assert.AreEqual(allMeetingDtos.Count(), meetingResults.Count());
            foreach (var meetingDto in meetingResults)
            {
                var expectedMeetingDto = allMeetingDtos.Where(m => m.SectionId == meetingDto.SectionId && m.InstructionalMethod == meetingDto.InstructionalMethod && m.MeetingDate == meetingDto.MeetingDate).FirstOrDefault();
                Assert.IsNotNull(expectedMeetingDto);
                Assert.AreEqual(expectedMeetingDto.StartTime, meetingDto.StartTime);
                Assert.AreEqual(expectedMeetingDto.EndTime, meetingDto.EndTime);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionsController_GetSectionMeetingInstancesAsync_ReturnError()
        {
            var meetingResults = await sectionsController.GetSectionMeetingInstancesAsync("2222222");
        }

        private IEnumerable<SectionMeetingInstance> BuildSectionMeetingInstanceDtos(IEnumerable<Domain.Student.Entities.SectionMeetingInstance> sectionMeetingInstanceEntities)
        {
            List<SectionMeetingInstance> result = new List<SectionMeetingInstance>();
            foreach (var meetingEntity in sectionMeetingInstanceEntities)
            {
                var meetingDto = new SectionMeetingInstance();
                meetingDto.SectionId = meetingEntity.SectionId;
                meetingDto.InstructionalMethod = meetingEntity.InstructionalMethod;
                meetingDto.MeetingDate = meetingEntity.MeetingDate;
                meetingDto.StartTime = meetingEntity.StartTime;
                meetingDto.EndTime = meetingEntity.EndTime;
                result.Add(meetingDto);
            }
            return result;
        }
    }

    [TestClass]
    public class SectionsControllerTests_QuerySectionsByPost3Async
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

        private SectionsController sectionsController;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private HttpResponse response;
        private Dtos.Student.Section3 section1;
        private Dtos.Student.Section3 section2;
        PrivacyWrapper<List<Dtos.Student.Section3>> privacyWrapper;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            logger = new Mock<ILogger>().Object;
            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections3Async(null, It.IsAny<Boolean>(), It.IsAny<Boolean>())).ThrowsAsync(new ArgumentNullException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections3Async(new List<string>(), It.IsAny<Boolean>(), It.IsAny<Boolean>())).ThrowsAsync(new ArgumentNullException());


            // section have faculty same as logged in user
            section1 = new Ellucian.Colleague.Dtos.Student.Section3();
            section1.Id = "SEC1";
            section1.TermId = "2012/FA";
            section1.EndDate = new DateTime(2012, 12, 21);
            section1.LearningProvider = "Ellucian";
            section1.ActiveStudentIds = new List<string>() { "student-1", "student-2" };
            section1.FacultyIds = new List<string>() { "0000678" };

            //section with no faculty assigned
            section2 = new Ellucian.Colleague.Dtos.Student.Section3();
            section2.Id = "SEC2";
            section2.TermId = "2012/FA";
            section2.EndDate = new DateTime(2012, 12, 21);
            section2.LearningProvider = "Ellucian";
            section1.ActiveStudentIds = new List<string>() { "student-1", "student-2" };

            privacyWrapper = new PrivacyWrapper<List<Dtos.Student.Section3>>();
            privacyWrapper.Dto = new List<Dtos.Student.Section3>();
            privacyWrapper.Dto.Add(section1);
            privacyWrapper.Dto.Add(section2);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections3Async(new List<string>() { "SEC1", "SEC2" }, It.IsAny<Boolean>(), It.IsAny<Boolean>())).Returns(Task.FromResult<PrivacyWrapper<List<Dtos.Student.Section3>>>(privacyWrapper));

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };

            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionCoordinationService = null;
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionsByPostAsync_Criteria_Is_Null()
        {
            await sectionsController.QuerySectionsByPost3Async(null);
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionsByPostAsync_Criteria_SectionId_Is_Null()

        {
            SectionsQueryCriteria criteria = new SectionsQueryCriteria();
            criteria.SectionIds = null;
            await sectionsController.QuerySectionsByPost3Async(criteria);
        }

        [TestMethod]
        public async Task QuerySectionsByPostAsync_Criteria_Have_SectionId_with_PrivacyRestrictions()

        {
            privacyWrapper.HasPrivacyRestrictions = true;
            SectionsQueryCriteria criteria = new SectionsQueryCriteria();
            criteria.SectionIds = new List<string>() { "SEC1", "SEC2" };

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionsByPost3Async(criteria);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(section1, result.ToList()[0]);
            Assert.AreEqual(section2, result.ToList()[1]);
            //Assert.IsTrue(System.Web.HttpContext.Current.Response.Headers.AllKeys.Contains("X-Content-Restricted"));
        }

        [TestMethod]
        public async Task QuerySectionsByPostAsync_Criteria_Have_SectionId_with_No_PrivacyRestrictions()

        {
            privacyWrapper.HasPrivacyRestrictions = false;
            SectionsQueryCriteria criteria = new SectionsQueryCriteria();
            criteria.SectionIds = new List<string>() { "SEC1", "SEC2" };

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionsByPost3Async(criteria);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(section1, result.ToList()[0]);
            Assert.AreEqual(section2, result.ToList()[1]);
        }
    }

    [TestClass]
    public class SectionsControllerTests_QuerySectionsByPost4Async
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

        private SectionsController sectionsController;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private HttpResponse response;
        private Dtos.Student.Section4 section1;
        private Dtos.Student.Section4 section2;
        PrivacyWrapper<List<Dtos.Student.Section4>> privacyWrapper;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            logger = new Mock<ILogger>().Object;
            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(null, It.IsAny<Boolean>(), It.IsAny<Boolean>())).ThrowsAsync(new ArgumentNullException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(new List<string>(), It.IsAny<Boolean>(), It.IsAny<Boolean>())).ThrowsAsync(new ArgumentNullException());

            // section have faculty same as logged in user
            section1 = new Ellucian.Colleague.Dtos.Student.Section4();
            section1.Id = "SEC1";
            section1.TermId = "2012/FA";
            section1.EndDate = new DateTime(2012, 12, 21);
            section1.LearningProvider = "Ellucian";
            section1.ActiveStudentIds = new List<string>() { "student-1", "student-2" };
            section1.FacultyIds = new List<string>() { "0000678" };

            //section with no faculty assigned
            section2 = new Ellucian.Colleague.Dtos.Student.Section4();
            section2.Id = "SEC2";
            section2.TermId = "2012/FA";
            section2.EndDate = new DateTime(2012, 12, 21);
            section2.LearningProvider = "Ellucian";
            section1.ActiveStudentIds = new List<string>() { "student-1", "student-2" };

            privacyWrapper = new PrivacyWrapper<List<Dtos.Student.Section4>>();
            privacyWrapper.Dto = new List<Dtos.Student.Section4>();
            privacyWrapper.Dto.Add(section1);
            privacyWrapper.Dto.Add(section2);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections4Async(new List<string>() { "SEC1", "SEC2" }, It.IsAny<Boolean>(), It.IsAny<Boolean>())).Returns(Task.FromResult<PrivacyWrapper<List<Dtos.Student.Section4>>>(privacyWrapper));

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };

            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionCoordinationService = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionsByPostAsync_Criteria_Is_Null()
        {
            await sectionsController.QuerySectionsByPost4Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionsByPostAsync_Criteria_SectionId_Is_Null()

        {
            SectionsQueryCriteria criteria = new SectionsQueryCriteria();
            criteria.SectionIds = null;
            await sectionsController.QuerySectionsByPost4Async(criteria);
        }

        [TestMethod]
        public async Task QuerySectionsByPostAsync_Criteria_Have_SectionId_with_PrivacyRestrictions()

        {
            privacyWrapper.HasPrivacyRestrictions = true;
            SectionsQueryCriteria criteria = new SectionsQueryCriteria();
            criteria.SectionIds = new List<string>() { "SEC1", "SEC2" };

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionsByPost4Async(criteria);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(section1, result.ToList()[0]);
            Assert.AreEqual(section2, result.ToList()[1]);
            //Assert.IsTrue(System.Web.HttpContext.Current.Response.Headers.AllKeys.Contains("X-Content-Restricted"));
        }

        [TestMethod]
        public async Task QuerySectionsByPostAsync_Criteria_Have_SectionId_with_No_PrivacyRestrictions()

        {
            privacyWrapper.HasPrivacyRestrictions = false;
            SectionsQueryCriteria criteria = new SectionsQueryCriteria();
            criteria.SectionIds = new List<string>() { "SEC1", "SEC2" };

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionsByPost4Async(criteria);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(section1, result.ToList()[0]);
            Assert.AreEqual(section2, result.ToList()[1]);
        }
    }

    [TestClass]
    public class SectionsControllerTests_QuerySectionsByPost2Async
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

        private SectionsController sectionsController;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private HttpResponse response;
        private Dtos.Student.Section3 section1;
        private Dtos.Student.Section3 section2;
        PrivacyWrapper<List<Dtos.Student.Section3>> privacyWrapper;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            logger = new Mock<ILogger>().Object;
            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections3Async(null, It.IsAny<Boolean>(), It.IsAny<Boolean>())).ThrowsAsync(new ArgumentNullException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections3Async(new List<string>(), It.IsAny<Boolean>(), It.IsAny<Boolean>())).ThrowsAsync(new ArgumentNullException());


            // section have faculty same as logged in user
            section1 = new Ellucian.Colleague.Dtos.Student.Section3();
            section1.Id = "SEC1";
            section1.TermId = "2012/FA";
            section1.EndDate = new DateTime(2012, 12, 21);
            section1.LearningProvider = "Ellucian";
            section1.ActiveStudentIds = new List<string>() { "student-1", "student-2" };
            section1.FacultyIds = new List<string>() { "0000678" };

            //section with no faculty assigned
            section2 = new Ellucian.Colleague.Dtos.Student.Section3();
            section2.Id = "SEC2";
            section2.TermId = "2012/FA";
            section2.EndDate = new DateTime(2012, 12, 21);
            section2.LearningProvider = "Ellucian";
            section1.ActiveStudentIds = new List<string>() { "student-1", "student-2" };

            privacyWrapper = new PrivacyWrapper<List<Dtos.Student.Section3>>();
            privacyWrapper.Dto = new List<Dtos.Student.Section3>();
            privacyWrapper.Dto.Add(section1);
            privacyWrapper.Dto.Add(section2);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections3Async(new List<string>() { "SEC1", "SEC2" }, It.IsAny<Boolean>(), It.IsAny<Boolean>())).Returns(Task.FromResult<PrivacyWrapper<List<Dtos.Student.Section3>>>(privacyWrapper));

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };

            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionCoordinationService = null;
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionsByPostAsync_SectionId_Is_Null()
        {
            await sectionsController.QuerySectionsByPost2Async(null);
        }

        [TestMethod]
        public async Task QuerySectionsByPostAsync_Criteria_Have_SectionId_with_PrivacyRestrictions()

        {
            privacyWrapper.HasPrivacyRestrictions = true;
            IEnumerable<string> sectionIds = new List<string>() { "SEC1", "SEC2" };

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionsByPost2Async(sectionIds);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(section1, result.ToList()[0]);
            Assert.AreEqual(section2, result.ToList()[1]);
        }

        [TestMethod]
        public async Task QuerySectionsByPostAsync_Criteria_Have_SectionId_with_No_PrivacyRestrictions()

        {
            privacyWrapper.HasPrivacyRestrictions = false;
            IEnumerable<string> sectionIds = new List<string>() { "SEC1", "SEC2" };

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionsByPost2Async(sectionIds);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(section1, result.ToList()[0]);
            Assert.AreEqual(section2, result.ToList()[1]);
        }
    }

    [TestClass]
    public class SectionsControllerTests_QuerySectionsByPostAsync
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

        private SectionsController sectionsController;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private HttpResponse response;
        private Dtos.Student.Section2 section1;
        private Dtos.Student.Section2 section2;
        PrivacyWrapper<List<Dtos.Student.Section2>> privacyWrapper;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            logger = new Mock<ILogger>().Object;
            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections2Async(null, It.IsAny<Boolean>())).ThrowsAsync(new ArgumentNullException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections2Async(new List<string>(), It.IsAny<Boolean>())).ThrowsAsync(new ArgumentNullException());


            // section have faculty same as logged in user
            section1 = new Ellucian.Colleague.Dtos.Student.Section2();
            section1.Id = "SEC1";
            section1.TermId = "2012/FA";
            section1.EndDate = new DateTime(2012, 12, 21);
            section1.LearningProvider = "Ellucian";
            section1.ActiveStudentIds = new List<string>() { "student-1", "student-2" };
            section1.FacultyIds = new List<string>() { "0000678" };

            //section with no faculty assigned
            section2 = new Ellucian.Colleague.Dtos.Student.Section2();
            section2.Id = "SEC2";
            section2.TermId = "2012/FA";
            section2.EndDate = new DateTime(2012, 12, 21);
            section2.LearningProvider = "Ellucian";
            section1.ActiveStudentIds = new List<string>() { "student-1", "student-2" };

            privacyWrapper = new PrivacyWrapper<List<Dtos.Student.Section2>>();
            privacyWrapper.Dto = new List<Dtos.Student.Section2>();
            privacyWrapper.Dto.Add(section1);
            privacyWrapper.Dto.Add(section2);
            sectionCoordinationServiceMock.Setup(svc => svc.GetSections2Async(new List<string>() { "SEC1", "SEC2" }, It.IsAny<Boolean>())).Returns(Task.FromResult<PrivacyWrapper<List<Dtos.Student.Section2>>>(privacyWrapper));

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };

            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionCoordinationService = null;
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionsByPostAsync_SectionId_Is_Null()
        {
            await sectionsController.QuerySectionsByPostAsync(null);
        }

        [TestMethod]
        public async Task QuerySectionsByPostAsync_Criteria_Have_SectionId_with_PrivacyRestrictions()

        {
            privacyWrapper.HasPrivacyRestrictions = true;
            IEnumerable<string> sectionIds = new List<string>() { "SEC1", "SEC2" };

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionsByPostAsync(sectionIds);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(section1, result.ToList()[0]);
            Assert.AreEqual(section2, result.ToList()[1]);
        }

        [TestMethod]
        public async Task QuerySectionsByPostAsync_Criteria_Have_SectionId_with_No_PrivacyRestrictions()

        {
            privacyWrapper.HasPrivacyRestrictions = false;
            IEnumerable<string> sectionIds = new List<string>() { "SEC1", "SEC2" };

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionsByPostAsync(sectionIds);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(section1, result.ToList()[0]);
            Assert.AreEqual(section2, result.ToList()[1]);
        }
    }

    [TestClass]
    public class SectionsControllerTests_QuerySectionEventsICalAsync
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

        private SectionsController sectionsController;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;
        private HttpResponse response;
        Ellucian.Colleague.Dtos.Base.EventsICal iCalResult = new Dtos.Base.EventsICal("raw iCal");


        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            logger = new Mock<ILogger>().Object;
            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionEventsICalAsync(null, It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ThrowsAsync(new ArgumentNullException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionEventsICalAsync(new List<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ThrowsAsync(new ArgumentNullException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionEventsICalAsync(new List<string>() { "SEC1", "SEC2" }, null, null)).ReturnsAsync(iCalResult);

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };

            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionCoordinationService = null;
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionEventsICalAsync_Criteria_Is_Null()
        {
            await sectionsController.QuerySectionEventsICalAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionEventsICalAsync_Criteria_Does_Not_have_SectionIds()

        {
            SectionEventsICalQueryCriteria criteria = new SectionEventsICalQueryCriteria();
            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionEventsICalAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionEventsICalAsync_Criteria_SectionIds_AreEmpty()

        {
            SectionEventsICalQueryCriteria criteria = new SectionEventsICalQueryCriteria();
            criteria.SectionIds = new List<string>();
            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionEventsICalAsync(criteria);
        }


        [TestMethod]
        public async Task QuerySectionEventsICalAsync_Criteria_Have_SectionId()

        {

            SectionEventsICalQueryCriteria criteria = new SectionEventsICalQueryCriteria();
            criteria.SectionIds = new List<string>() { "SEC1", "SEC2" }; ;

            sectionsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            // Set up an Http Context
            response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

            var result = await sectionsController.QuerySectionEventsICalAsync(criteria);
            Assert.IsNotNull(result);
            Assert.AreEqual(iCalResult, result);
            Assert.AreEqual("raw iCal", result.iCal);
        }
    }

    [TestClass]
    public class SectionsController_GetSectionMidtermGradeComplete_Tests
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

        private SectionsController sectionsController;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            logger = new Mock<ILogger>().Object;

            // controller that will be tested using mock objects
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }


        [TestMethod]
        public async Task SectionsController_GetSectionMidtermGradingCompleteAsync_PermissionException()
        {
            // Test that a permission exception coming out of the coordination service produces an HTTP forbidden status
            string sectionId = "Permission";
            sectionCoordinationServiceMock.Setup(s => s.GetSectionMidtermGradingCompleteAsync(sectionId)).Throws(new PermissionsException());

            bool HitException = false;
            try
            {
                await sectionsController.GetSectionMidtermGradingCompleteAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                HitException = true;
            }
            catch (Exception ex)
            {
            }
            Assert.AreEqual(HitException, true);
        }

        [TestMethod]
        public async Task SectionsController_GetSectionMidtermGradingCompleteAsync_ArgumentNullException()
        {
            // Test that an argument null exception coming out of the coordination service produces an HTTP bad request status
            string sectionId = "ArgNull";
            sectionCoordinationServiceMock.Setup(s => s.GetSectionMidtermGradingCompleteAsync(sectionId)).Throws(new ArgumentNullException());

            bool HitException = false;
            try
            {
                await sectionsController.GetSectionMidtermGradingCompleteAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                HitException = true;
            }
            catch (Exception ex)
            {
            }
            Assert.AreEqual(HitException, true);
        }

        [TestMethod]
        public async Task SectionsController_GetSectionMidtermGradingCompleteAsync_GenericException()
        {
            // Test that a generic exception coming out of the coordination service produces an HTTP bad request status
            string sectionId = "Except";
            sectionCoordinationServiceMock.Setup(s => s.GetSectionMidtermGradingCompleteAsync(sectionId)).Throws(new Exception());

            bool HitException = false;
            try
            {
                await sectionsController.GetSectionMidtermGradingCompleteAsync(sectionId);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                HitException = true;
            }
            catch (Exception ex)
            {
            }
            Assert.AreEqual(HitException, true);
        }

        [TestMethod]
        public async Task SectionsController_GetSectionMidtermGradingCompleteAsync_DtoPassThrough()
        {
            // Test that the dto returned by the coordination service is passed back through the controller call
            string sectionId = "Good";
            SectionMidtermGradingComplete dto = new SectionMidtermGradingComplete();
            dto.SectionId = sectionId;
            List<GradingCompleteIndication> midtermGradingComplete1 = new List<GradingCompleteIndication>();
            midtermGradingComplete1.Add(new GradingCompleteIndication { CompleteOperator = "Oper1", DateAndTime = new DateTimeOffset(2010, 1, 1, 1, 1, 1, new TimeSpan(-3, 0, 0)) });
            midtermGradingComplete1.Add(new GradingCompleteIndication { CompleteOperator = "Oper2", DateAndTime = new DateTimeOffset(2010, 1, 2, 1, 1, 2, new TimeSpan(-3, 0, 0)) });
            dto.MidtermGrading1Complete = midtermGradingComplete1;

            List<GradingCompleteIndication> midtermGradingComplete2 = new List<GradingCompleteIndication>();
            List<GradingCompleteIndication> midtermGradingComplete3 = new List<GradingCompleteIndication>();
            List<GradingCompleteIndication> midtermGradingComplete4 = new List<GradingCompleteIndication>();
            List<GradingCompleteIndication> midtermGradingComplete5 = new List<GradingCompleteIndication>();
            List<GradingCompleteIndication> midtermGradingComplete6 = new List<GradingCompleteIndication>();
            dto.MidtermGrading2Complete = midtermGradingComplete2;
            dto.MidtermGrading3Complete = midtermGradingComplete3;
            dto.MidtermGrading4Complete = midtermGradingComplete4;
            dto.MidtermGrading5Complete = midtermGradingComplete5;
            dto.MidtermGrading6Complete = midtermGradingComplete6;


            sectionCoordinationServiceMock.Setup(s => s.GetSectionMidtermGradingCompleteAsync(sectionId)).Returns(Task.FromResult(dto));

            var returnDto = await sectionsController.GetSectionMidtermGradingCompleteAsync(sectionId);
            Assert.AreEqual(dto.SectionId, returnDto.SectionId);
            Assert.AreEqual(dto.MidtermGrading1Complete.Count(), returnDto.MidtermGrading1Complete.Count());
            Assert.AreEqual(dto.MidtermGrading2Complete.Count(), returnDto.MidtermGrading2Complete.Count());
            Assert.AreEqual(dto.MidtermGrading3Complete.Count(), returnDto.MidtermGrading3Complete.Count());
            Assert.AreEqual(dto.MidtermGrading4Complete.Count(), returnDto.MidtermGrading4Complete.Count());
            Assert.AreEqual(dto.MidtermGrading5Complete.Count(), returnDto.MidtermGrading5Complete.Count());
            Assert.AreEqual(dto.MidtermGrading6Complete.Count(), returnDto.MidtermGrading6Complete.Count());
            Assert.AreEqual(dto.MidtermGrading1Complete.ElementAt(0).CompleteOperator, returnDto.MidtermGrading1Complete.ElementAt(0).CompleteOperator);
            Assert.AreEqual(dto.MidtermGrading1Complete.ElementAt(0).DateAndTime, returnDto.MidtermGrading1Complete.ElementAt(0).DateAndTime);
            Assert.AreEqual(dto.MidtermGrading1Complete.ElementAt(1).CompleteOperator, returnDto.MidtermGrading1Complete.ElementAt(1).CompleteOperator);
            Assert.AreEqual(dto.MidtermGrading1Complete.ElementAt(1).DateAndTime, returnDto.MidtermGrading1Complete.ElementAt(1).DateAndTime);

        }
    }

    [TestClass]
    public class SectionsController_PostSectionMidtermGradingCompleteAsync_Tests
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

        private SectionsController sectionsController;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ISectionCoordinationService sectionCoordinationService;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            logger = new Mock<ILogger>().Object;

            // controller that will be tested using mock objects
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestMethod]
        public async Task SectionsController_PostSectionMidtermGradingCompleteAsync_PermissionException()
        {
            // Test that a permission exception coming out of the coordination service produces an HTTP forbidden status
            sectionCoordinationServiceMock.Setup(s => s.PostSectionMidtermGradingCompleteAsync(It.IsAny<string>(), It.IsAny<SectionMidtermGradingCompleteForPost>())).Throws(new PermissionsException());

            bool HitException = false;
            try
            {
                await sectionsController.PostSectionMidtermGradingCompleteAsync("12345", new SectionMidtermGradingCompleteForPost());
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, ex.Response.StatusCode);
                HitException = true;
            }
            catch (Exception ex)
            {
            }
            Assert.AreEqual(HitException, true);
        }

        [TestMethod]
        public async Task SectionsController_PostSectionMidtermGradingCompleteAsync_ArgumentNullException()
        {
            // Test that an argument null exception coming out of the coordination service produces an HTTP bad request status
            sectionCoordinationServiceMock.Setup(s => s.PostSectionMidtermGradingCompleteAsync(null, null)).Throws(new ArgumentNullException());

            bool HitException = false;
            try
            {
                await sectionsController.PostSectionMidtermGradingCompleteAsync(null, null);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                HitException = true;
            }
            catch (Exception ex)
            {
            }
            Assert.AreEqual(HitException, true);
        }

        [TestMethod]
        public async Task SectionsController_PostSectionMidtermGradingCompleteAsync_ArgumentException()
        {
            // Test that a generic exception coming out of the coordination service produces an HTTP bad request status
            sectionCoordinationServiceMock.Setup(s => s.PostSectionMidtermGradingCompleteAsync(It.IsAny<string>(), It.IsAny<SectionMidtermGradingCompleteForPost>())).Throws(new ArgumentException());

            bool HitException = false;
            try
            {
                await sectionsController.PostSectionMidtermGradingCompleteAsync("12345", new SectionMidtermGradingCompleteForPost());
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                HitException = true;
            }
            catch (Exception ex)
            {
            }
            Assert.AreEqual(HitException, true);
        }

        [TestMethod]
        public async Task SectionsController_PostSectionMidtermGradingCompleteAsync_GenericException()
        {
            // Test that a generic exception coming out of the coordination service produces an HTTP bad request status
            sectionCoordinationServiceMock.Setup(s => s.PostSectionMidtermGradingCompleteAsync(It.IsAny<string>(), It.IsAny<SectionMidtermGradingCompleteForPost>())).Throws(new Exception());

            bool HitException = false;
            try
            {
                await sectionsController.PostSectionMidtermGradingCompleteAsync("12345", new SectionMidtermGradingCompleteForPost());
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                HitException = true;
            }
            catch (Exception ex)
            {
            }
            Assert.AreEqual(HitException, true);
        }


        [TestMethod]
        public async Task SectionsController_PostSectionMidtermGradingCompleteAsync_DtoPassThrough()
        {
            // Test that the dto returned by the coordination service is passed back through the controller call
            string sectionId = "Good";
            SectionMidtermGradingCompleteForPost dto = new SectionMidtermGradingCompleteForPost()
            {
                CompleteOperator = "0012345",
                DateAndTime = DateTimeOffset.Now,
                MidtermGradeNumber = 1
            };
            SectionMidtermGradingComplete success = new SectionMidtermGradingComplete()
            {
                MidtermGrading1Complete = new List<GradingCompleteIndication>()
                {
                    new GradingCompleteIndication()
                    {
                        CompleteOperator = dto.CompleteOperator,
                        DateAndTime = dto.DateAndTime.Value
                    }
                },
                SectionId = sectionId
            };

            sectionCoordinationServiceMock.Setup(s => s.PostSectionMidtermGradingCompleteAsync(sectionId, dto)).ReturnsAsync(success);

            var returnDto = await sectionsController.PostSectionMidtermGradingCompleteAsync(sectionId, dto);
            Assert.AreEqual(success.SectionId, returnDto.SectionId);
            Assert.AreEqual(1, returnDto.MidtermGrading1Complete.Count());
            Assert.AreEqual(success.MidtermGrading1Complete.ElementAt(0).CompleteOperator, returnDto.MidtermGrading1Complete.ElementAt(0).CompleteOperator);
            Assert.AreEqual(success.MidtermGrading1Complete.ElementAt(0).DateAndTime, returnDto.MidtermGrading1Complete.ElementAt(0).DateAndTime);
        }
    }

    [TestClass]
    public class SectionsController_PostSectionSearchAsync
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

        private SectionsController sectionsController;

        private Mock<ICourseService> courseServiceMock;
        private ICourseService courseService;
        //private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Courses> allCourses;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            courseServiceMock = new Mock<ICourseService>();
            courseService = courseServiceMock.Object;
            //allCourses = await new TestCourseRepository().GetAsync();
            var coursesList = new List<Course>();

            sectionsController = new SectionsController(null, null, null, courseService, loggerMock.Object);

            var sectionPage = new SectionPage();
            courseServiceMock.Setup(svc => svc.SectionSearchAsync(It.IsAny<SectionSearchCriteria>(), 10, 1)).Returns(Task.FromResult(sectionPage));
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            courseService = null;
        }

        [TestMethod]
        public async Task KeywordSearch()
        {
            var criteria = new SectionSearchCriteria();
            criteria.Keyword = "HIST";
            var response = await sectionsController.PostSectionSearchAsync(criteria, 10, 1);
            Assert.IsTrue(response is SectionPage);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RethrowsExceptionAsHttpResponseException()
        {
            courseServiceMock.Setup(x => x.SectionSearchAsync(It.IsAny<SectionSearchCriteria>(), 10, 1)).Throws(new Exception());
            var criteria = new SectionSearchCriteria();
            criteria.Keyword = "HIST";
            var response = await sectionsController.PostSectionSearchAsync(criteria, 10, 1);
        }
    }

    [TestClass]
    public class SectionsController_PostSectionCensusCertificationAsync_Tests
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

        private SectionsController sectionsController;
        private ISectionCoordinationService sectionCoordinationService;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private ILogger logger;

        private SectionCensusToCertify sectionCensusToCertify = null;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            logger = new Mock<ILogger>().Object;

            // controller that will be tested using mock objects
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger);
            sectionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            sectionCensusToCertify = new SectionCensusToCertify()
            {
                CensusCertificationDate = new DateTime(2021, 01, 01),
                CensusCertificationLabel = string.Empty,
                CensusCertificationPosition = "1",
                CensusCertificationRecordedDate = new DateTime(2021, 01, 01),
                CensusCertificationRecordedTime = new DateTime(2021, 01, 01, 13, 12, 0)
            };
        }

        [TestMethod]
        public async Task SectionsController_PostSectionCensusCertificationAsync_sectionId_null()
        {
            bool hitException = false;
            try
            {
                await sectionsController.PostSectionCensusCertificationAsync(null, sectionCensusToCertify);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                hitException = true;
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(hitException, true);
        }

        [TestMethod]
        public async Task SectionsController_PostSectionCensusCertificationAsync_sectionCensusToCertify_null()
        {
            bool hitException = false;
            try
            {
                await sectionsController.PostSectionCensusCertificationAsync("123", null);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                hitException = true;
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(hitException, true);
        }

        [TestMethod]
        public async Task SectionsController_PostSectionCensusCertificationAsync_CensusCertificationDate_null()
        {
            sectionCensusToCertify.CensusCertificationDate = null;
            bool hitException = false;
            try
            {
                await sectionsController.PostSectionCensusCertificationAsync("123", sectionCensusToCertify);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                hitException = true;
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(hitException, true);
        }

        [TestMethod]
        public async Task SectionsController_PostSectionCensusCertificationAsync_CensusCertificationRecordedDate_null()
        {
            sectionCensusToCertify.CensusCertificationRecordedDate = null;
            bool hitException = false;
            try
            {
                await sectionsController.PostSectionCensusCertificationAsync("123", sectionCensusToCertify);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                hitException = true;
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(hitException, true);
        }

        [TestMethod]
        public async Task SectionsController_PostSectionCensusCertificationAsync_CensusCertificationRecordedTime_null()
        {
            sectionCensusToCertify.CensusCertificationRecordedTime = null;
            bool hitException = false;
            try
            {
                await sectionsController.PostSectionCensusCertificationAsync("123", sectionCensusToCertify);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                hitException = true;
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(hitException, true);
        }

        [TestMethod]
        public async Task SectionsController_PostSectionCensusCertificationAsync_CensusCertificationPosition_null()
        {
            bool hitException = false;
            sectionCensusToCertify.CensusCertificationPosition = null;
            try
            {
                await sectionsController.PostSectionCensusCertificationAsync("123", sectionCensusToCertify);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                hitException = true;
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(hitException, true);
        }


        [TestMethod]
        public async Task SectionsController_PostSectionCensusCertificationAsync_CensusCertificationPosition_emtpy()
        {
            bool hitException = false;
            sectionCensusToCertify.CensusCertificationPosition = string.Empty;
            try
            {
                await sectionsController.PostSectionCensusCertificationAsync("123", sectionCensusToCertify);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                hitException = true;
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(hitException, true);
        }


        [TestMethod]
        public async Task SectionsController_PostSectionCensusCertificationAsync_CensusCertificationPosition_whitespace()
        {
            bool hitException = false;
            sectionCensusToCertify.CensusCertificationPosition = "   ";
            try
            {
                await sectionsController.PostSectionCensusCertificationAsync("123", sectionCensusToCertify);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                hitException = true;
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(hitException, true);
        }
    }

    [TestClass]
    public class SectionsControllerTests_QuerySectionsSeatsAsync
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

        private ILogger logger;
        private SectionsController sectionsController;
        private ISectionCoordinationService sectionCoordinationService;
        private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
        private SectionSeats sectionSeatsMathOpen;
        private SectionSeats sectionSeatsMathSxrf;
        private SectionSeats sectionSeatsCompSxrf;
        private SectionSeats sectionSeatsMathFull;
        private SectionSeats sectionSeatsMathWait;
        private IEnumerable<SectionSeats> singleMathSection;
        private IEnumerable<SectionSeats> crosslistedMathSections;
        private IEnumerable<SectionSeats> allMathSections;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            logger = new Mock<ILogger>().Object;
            sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
            sectionCoordinationService = sectionCoordinationServiceMock.Object;
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionsSeatsAsync(null)).ThrowsAsync(new ArgumentNullException());
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionsSeatsAsync(new List<string>())).ThrowsAsync(new ArgumentNullException());

            sectionSeatsMathOpen = new SectionSeats()
            {
                Id = "25463",
                Guid = "98b3b1b2-f4ee-42e6-8311-66ba4325a445",
                NumberOnWaitlist = 0,
                PermittedToRegisterOnWaitlist = 0,
                ReservedSeats = null,
                SectionCapacity = null,
                GlobalCapacity = null,
                CombineCrosslistWaitlists = false,
                Capacity = null,
                Available = null,
                Waitlisted = 0
            };

            sectionSeatsMathSxrf = new SectionSeats()
            {
                Id = "26242",
                Guid = "d8d61062-2436-46cc-ab06-d9ebde993292",
                NumberOnWaitlist = 0,
                PermittedToRegisterOnWaitlist = 0,
                ReservedSeats = 0,
                SectionCapacity = 30,
                GlobalCapacity = null,
                CombineCrosslistWaitlists = false,
                Capacity = 30,
                Available = 26,
                Waitlisted = 0
            };

            sectionSeatsCompSxrf = new SectionSeats()
            {
                Id = "26607",
                Guid = "34067fc2-17e0-41a6-80e0-c2a66977c102",
                NumberOnWaitlist = 0,
                PermittedToRegisterOnWaitlist = 0,
                ReservedSeats = 0,
                SectionCapacity = null,
                GlobalCapacity = null,
                CombineCrosslistWaitlists = false,
                Capacity = null,
                Available = null,
                Waitlisted = 0
            };

            sectionSeatsMathFull = new SectionSeats()
            {
                Id = "26346",
                Guid = "340b8051-1824-4833-87fe-b8526378a906",
                NumberOnWaitlist = 0,
                PermittedToRegisterOnWaitlist = 0,
                ReservedSeats = 0,
                SectionCapacity = 1,
                GlobalCapacity = null,
                CombineCrosslistWaitlists = false,
                Capacity = 1,
                Available = 0,
                Waitlisted = 0
            };

            sectionSeatsMathWait = new SectionSeats()
            {
                Id = "25426",
                Guid = "5c0de765-08f3-450f-ba5e-d36dd7c48670",
                NumberOnWaitlist = 2,
                PermittedToRegisterOnWaitlist = 0,
                ReservedSeats = 0,
                SectionCapacity = 1,
                GlobalCapacity = null,
                CombineCrosslistWaitlists = false,
                Capacity = 1,
                Available = 0,
                Waitlisted = 2
            };

            singleMathSection = new List<SectionSeats>() { sectionSeatsMathOpen };
            crosslistedMathSections = new List<SectionSeats>() { sectionSeatsCompSxrf, sectionSeatsMathFull };
            allMathSections = new List<SectionSeats>() { sectionSeatsMathOpen, sectionSeatsMathSxrf, sectionSeatsMathFull, sectionSeatsMathWait, sectionSeatsCompSxrf };

            //returns only one section
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionsSeatsAsync(new List<string>() { "25463" })).Returns(Task.FromResult<IEnumerable<SectionSeats>>(singleMathSection));

            //returns section and cross-listed section
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionsSeatsAsync(new List<string>() { "26242" })).Returns(Task.FromResult<IEnumerable<SectionSeats>>(crosslistedMathSections));

            //returns all sections and cross-listed section
            sectionCoordinationServiceMock.Setup(svc => svc.GetSectionsSeatsAsync(new List<string>() { "25463", "26242", "26346", "25426"})).Returns(Task.FromResult<IEnumerable<SectionSeats>>(allMathSections));

            // mock controller
            sectionsController = new SectionsController(sectionCoordinationService, null, null, null, logger)
            {
                Request = new HttpRequestMessage()
            };

            sectionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionsController = null;
            sectionCoordinationService = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionsSeatsAsync_SectionIds_Null()
        {
            await sectionsController.QuerySectionsSeatsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QuerySectionsSeatsAsync_SectionIds_Empty()
        {
            await sectionsController.QuerySectionsSeatsAsync(new List<string>());
        }

        [TestMethod]
        public async Task QuerySectionsSeatsAsync_SingleSectionIdWithoutCrosslistedSections_ReturnsOneSection()
        {
            var result = await sectionsController.QuerySectionsSeatsAsync(new List<string>() { "25463" });
            var sectionSeats = result.FirstOrDefault();

            Assert.AreEqual(singleMathSection.Count(), result.Count());
            Assert.AreEqual(sectionSeatsMathOpen.Id, sectionSeats.Id);
            Assert.AreEqual(sectionSeatsMathOpen.Guid, sectionSeats.Guid);
            Assert.AreEqual(sectionSeatsMathOpen.NumberOnWaitlist, sectionSeats.NumberOnWaitlist);
            Assert.AreEqual(sectionSeatsMathOpen.PermittedToRegisterOnWaitlist, sectionSeats.PermittedToRegisterOnWaitlist);
            Assert.AreEqual(sectionSeatsMathOpen.ReservedSeats, sectionSeats.ReservedSeats);
            Assert.AreEqual(sectionSeatsMathOpen.SectionCapacity, sectionSeats.SectionCapacity);
            Assert.AreEqual(sectionSeatsMathOpen.GlobalCapacity, sectionSeats.GlobalCapacity);
            Assert.AreEqual(sectionSeatsMathOpen.CombineCrosslistWaitlists, sectionSeats.CombineCrosslistWaitlists);
            Assert.AreEqual(sectionSeatsMathOpen.Capacity, sectionSeats.Capacity);
            Assert.AreEqual(sectionSeatsMathOpen.Available, sectionSeats.Available);
            Assert.AreEqual(sectionSeatsMathOpen.Waitlisted, sectionSeats.Waitlisted);
        }

        [TestMethod]
        public async Task QuerySectionsSeatsAsync_SingleSectionIdWithCrosslistedSections_ReturnsMultipleSections()
        {
            var result = (await sectionsController.QuerySectionsSeatsAsync(new List<string>() { "26242" })).ToList();

            //resulting count will include the cross-listed section
            Assert.AreEqual(crosslistedMathSections.Count(), result.Count());

            int i = 0;
            foreach(SectionSeats sectionSeats in crosslistedMathSections)
            {
                Assert.AreEqual(sectionSeats.Id, result[i].Id);
                Assert.AreEqual(sectionSeats.Guid, result[i].Guid);
                Assert.AreEqual(sectionSeats.NumberOnWaitlist, result[i].NumberOnWaitlist);
                Assert.AreEqual(sectionSeats.PermittedToRegisterOnWaitlist, result[i].PermittedToRegisterOnWaitlist);
                Assert.AreEqual(sectionSeats.ReservedSeats, result[i].ReservedSeats);
                Assert.AreEqual(sectionSeats.SectionCapacity, result[i].SectionCapacity);
                Assert.AreEqual(sectionSeats.GlobalCapacity, result[i].GlobalCapacity);
                Assert.AreEqual(sectionSeats.CombineCrosslistWaitlists, result[i].CombineCrosslistWaitlists);
                Assert.AreEqual(sectionSeats.Capacity, result[i].Capacity);
                Assert.AreEqual(sectionSeats.Available, result[i].Available);
                Assert.AreEqual(sectionSeats.Waitlisted, result[i].Waitlisted);
                i++;
            }
        }

        [TestMethod]
        public async Task QuerySectionsSeatsAsync_SectionIds_ReturnsAllMultipleSections()
        {
            var result = (await sectionsController.QuerySectionsSeatsAsync(new List<string>() { "25463", "26242", "26346", "25426" })).ToList();

            //resulting count will include the cross-listed section
            Assert.AreEqual(allMathSections.Count(), result.Count());

            int i = 0;
            foreach (SectionSeats sectionSeats in allMathSections)
            {
                Assert.AreEqual(sectionSeats.Id, result[i].Id);
                Assert.AreEqual(sectionSeats.Guid, result[i].Guid);
                Assert.AreEqual(sectionSeats.NumberOnWaitlist, result[i].NumberOnWaitlist);
                Assert.AreEqual(sectionSeats.PermittedToRegisterOnWaitlist, result[i].PermittedToRegisterOnWaitlist);
                Assert.AreEqual(sectionSeats.ReservedSeats, result[i].ReservedSeats);
                Assert.AreEqual(sectionSeats.SectionCapacity, result[i].SectionCapacity);
                Assert.AreEqual(sectionSeats.GlobalCapacity, result[i].GlobalCapacity);
                Assert.AreEqual(sectionSeats.CombineCrosslistWaitlists, result[i].CombineCrosslistWaitlists);
                Assert.AreEqual(sectionSeats.Capacity, result[i].Capacity);
                Assert.AreEqual(sectionSeats.Available, result[i].Available);
                Assert.AreEqual(sectionSeats.Waitlisted, result[i].Waitlisted);
                i++;
            }
        }
    }
}

