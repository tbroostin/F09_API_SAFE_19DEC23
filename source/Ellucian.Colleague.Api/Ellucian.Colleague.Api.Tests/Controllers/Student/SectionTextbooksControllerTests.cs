// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SectionTextbooksControllerTests
    {
        [TestClass]
        public class SectionTextbooksControllerTestsAddBookToSection
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

            private SectionTextbooksController sectionTextbooksController;
            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;
            private ILogger logger;
            private SectionTextbook sectionTextbook;
            private SectionTextbook sectionTextbook2;
            private SectionTextbook sectionTextbook3;
            private SectionTextbook sectionTextbook4;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                sectionTextbook = new SectionTextbook()
                {
                    SectionId = "12345",
                    Action = SectionBookAction.Add,
                    RequirementStatusCode = "R",
                    Textbook = new Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };

                sectionTextbook2 = new SectionTextbook()
                {
                    SectionId = "012345",
                    Action = SectionBookAction.Add,
                    RequirementStatusCode = "R",
                    Textbook = new Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };

                sectionTextbook3 = new SectionTextbook()
                {
                    SectionId = "123456",
                    Action = SectionBookAction.Add,
                    RequirementStatusCode = "R",
                    Textbook = new Book()
                    {
                        Id = "123",
                        Isbn = "123456789",
                        Title = "Title",
                        Author = "Author",
                        Publisher = "Publisher",
                        Copyright = "Copyright",
                        Edition = "Edition",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId1",
                        AlternateID2 = "altId2",
                        AlternateID3 = "altId3"
                    }
                };

                sectionTextbook4 = new SectionTextbook()
                {
                    SectionId = "4123456",
                    Action = SectionBookAction.Add,
                    RequirementStatusCode = "R",
                    Textbook = new Book()
                    {
                        Id = "4123",
                        Isbn = "4123456789",
                        Title = "Title 4",
                        Author = "Author 4",
                        Publisher = "Publisher 4",
                        Copyright = "Copyright 4",
                        Edition = "Edition 4",
                        IsActive = true,
                        PriceUsed = 10m,
                        Price = 20m,
                        Comment = "Comment",
                        ExternalComments = "External Comments",
                        AlternateID1 = "altId41",
                        AlternateID2 = "altId42",
                        AlternateID3 = "altId43"
                    }
                };

                var section = new Section3();
                sectionCoordinationServiceMock.Setup(svc => svc.UpdateSectionBookAsync(sectionTextbook)).ThrowsAsync(new ArgumentNullException());
                sectionCoordinationServiceMock.Setup(svc => svc.UpdateSectionBookAsync(sectionTextbook2)).ThrowsAsync(new PermissionsException());
                sectionCoordinationServiceMock.Setup(svc => svc.UpdateSectionBookAsync(sectionTextbook3)).ReturnsAsync(section);
                sectionCoordinationServiceMock.Setup(svc => svc.UpdateSectionBookAsync(sectionTextbook4))
                    .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));

                // mock controller
                sectionTextbooksController = new SectionTextbooksController(sectionCoordinationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionTextbooksController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_AddBookToSection_BadRequest()
            {
                var result = await sectionTextbooksController.UpdateSectionBookAsync(sectionTextbook);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_AddBookToSection_Forbidden()
            {
                var result = await sectionTextbooksController.UpdateSectionBookAsync(sectionTextbook2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionsController_AddBookToSection_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    await sectionTextbooksController.UpdateSectionBookAsync(sectionTextbook4);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public async Task SectionsController_AddBookToSection_AddedBookToSection()
            {
                var result = await sectionTextbooksController.UpdateSectionBookAsync(sectionTextbook3);
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(Section3));
            }
        }
    }
}
