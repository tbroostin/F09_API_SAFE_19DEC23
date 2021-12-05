// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class GradesControllerTest
    {
        [TestClass]
        public class GradesControllerGet
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

            private GradesController gradeController;

            private Mock<IGradeService> gradeServiceMock;
            private Mock<IGradeRepository> gradeRepoMock;
            private List<Dtos.Grade> allGradesDtos;
            private IGradeService gradeService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Grade> allGrades;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();


                loggerMock = new Mock<ILogger>();
                gradeServiceMock = new Mock<IGradeService>();
                gradeRepoMock = new Mock<IGradeRepository>();
                gradeService = gradeServiceMock.Object;
                allGrades = await new TestGradeRepository().GetHedmAsync(false);
                allGradesDtos = new List<Dtos.Grade>();

                gradeController = new GradesController(adapterRegistryMock.Object, gradeRepoMock.Object, gradeServiceMock.Object, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Grade, Dtos.Grade>();

                gradeController.Request = new HttpRequestMessage();

                gradeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach(var grade in allGrades)
                {
                    Ellucian.Colleague.Dtos.Grade target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.Grade>(grade);                    
                    target.GradeItem = new Dtos.GradeItem() { GradeValue = grade.LetterGrade };
                    
                    allGradesDtos.Add(target);
                }
            }

            [TestMethod]
            public async Task GradesController_GetHedmAsync()
            {
                gradeServiceMock.Setup(gc => gc.GetAsync(It.IsAny<bool>())).ReturnsAsync(allGradesDtos);

                var result = await gradeController.GetHedmAsync();
                Assert.AreEqual(result.Count(), allGrades.Count());
                
                int count = allGrades.Count();
                for(int i = 0; i < count; i++)
                {
                    var expected = allGradesDtos[i];
                    var actual = allGrades.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.GradeItem.GradeValue, actual.LetterGrade);
                }
            }

            [TestMethod]
            public async Task GradesController_GetHedmAsync_CacheControlNotNull()
            {
                gradeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                gradeServiceMock.Setup(gc => gc.GetAsync(It.IsAny<bool>())).ReturnsAsync(allGradesDtos);

                var result = await gradeController.GetHedmAsync();
                Assert.AreEqual(result.Count(), allGrades.Count());

                int count = allGrades.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allGradesDtos[i];
                    var actual = allGrades.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.GradeItem.GradeValue, actual.LetterGrade);
                }
            }

            [TestMethod]
            public async Task GradesController_GetHedmAsync_NoCache()
            {
                gradeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                gradeController.Request.Headers.CacheControl.NoCache = true;

                gradeServiceMock.Setup(gc => gc.GetAsync(It.IsAny<bool>())).ReturnsAsync(allGradesDtos);

                var result = await gradeController.GetHedmAsync();
                Assert.AreEqual(result.Count(), allGrades.Count());

                int count = allGrades.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allGradesDtos[i];
                    var actual = allGrades.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.GradeItem.GradeValue, actual.LetterGrade);
                }
            }

            [TestMethod]
            public async Task GradesController_GetHedmAsync_Cache()
            {
                gradeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                gradeController.Request.Headers.CacheControl.NoCache = false;

                gradeServiceMock.Setup(gc => gc.GetAsync(It.IsAny<bool>())).ReturnsAsync(allGradesDtos);

                var result = await gradeController.GetHedmAsync();
                Assert.AreEqual(result.Count(), allGrades.Count());

                int count = allGrades.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allGradesDtos[i];
                    var actual = allGrades.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.GradeItem.GradeValue, actual.LetterGrade);
                }
            }

            [TestMethod]
            public async Task GradesController_GetByIdHedmAsync()
            {
                var thisGradeType = allGradesDtos.Where(m => m.Id == "A").FirstOrDefault();

                gradeServiceMock.Setup(x => x.GetGradeByIdAsync(It.IsAny<string>())).ReturnsAsync(thisGradeType);

                var gradeType = await gradeController.GetByIdHedmAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0");
                Assert.AreEqual(thisGradeType.Id, gradeType.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GradeController_GetThrowsIntAppiExc()
            {
                gradeServiceMock.Setup(gc => gc.GetAsync(It.IsAny<bool>())).Throws<Exception>();

                await gradeController.GetHedmAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GradeController_PostThrowsIntAppiExc()
            {
                await gradeController.PostGradeAsync(allGradesDtos[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GradeController_PutThrowsIntAppiExc()
            {
                var result = await gradeController.PutGradeAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", allGradesDtos[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GradeController_DeleteThrowsIntAppiExc()
            {
                var result = await gradeController.DeleteGradeByIdAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0");
            }
        }

        [TestClass]
        public class GradesControllerGradesDefinitionsMaximum
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

            private GradesController gradeController;

            private Mock<IGradeService> gradeServiceMock;
            private Mock<IGradeRepository> gradeRepoMock;
            private List<Dtos.GradeDefinitionsMaximum> allGradeDefinitionsMaximumDtos;
            private IGradeService gradeService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Grade> allGrades;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();


                loggerMock = new Mock<ILogger>();
                gradeServiceMock = new Mock<IGradeService>();
                gradeRepoMock = new Mock<IGradeRepository>();
                gradeService = gradeServiceMock.Object;
                allGrades = await new TestGradeRepository().GetHedmAsync(false);
                allGradeDefinitionsMaximumDtos = new List<Dtos.GradeDefinitionsMaximum>();

                gradeController = new GradesController(adapterRegistryMock.Object, gradeRepoMock.Object, gradeServiceMock.Object, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Grade, Dtos.Grade>();

                gradeController.Request = new HttpRequestMessage();

                gradeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var grade in allGrades)
                {
                    Ellucian.Colleague.Dtos.GradeDefinitionsMaximum target = new Dtos.GradeDefinitionsMaximum(); //Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Grade, Ellucian.Colleague.Dtos.GradeDefinitionsMaximum>(grade);
                    target.GradeItem = new Dtos.GradeItem() { GradeValue = grade.LetterGrade };
                    target.Id = grade.Guid;
                    target.GradeScheme = new Dtos.GradeSchemeProperty()
                    {
                        Detail = new Dtos.GuidObject2 { Id = new Guid().ToString() }

                    };
                    allGradeDefinitionsMaximumDtos.Add(target);
                }
            }

            [TestMethod]
            public async Task GradesController_GetGradesDefinitionsMaximum()
            {
                gradeServiceMock.Setup(gc => gc.GetGradesDefinitionsMaximumAsync(It.IsAny<bool>())).ReturnsAsync(allGradeDefinitionsMaximumDtos);

                var result = await gradeController.GetGradeDefinitionsMaximumAsync();
                Assert.AreEqual(result.Count(), allGrades.Count());

                int count = allGrades.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allGradeDefinitionsMaximumDtos[i];
                    var actual = allGrades.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.GradeItem.GradeValue, actual.LetterGrade);
                }
            }

            [TestMethod]
            public async Task GradesController_GetGradesDefinitionsMaximum_CacheControlNotNull()
            {
                gradeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                gradeServiceMock.Setup(gc => gc.GetGradesDefinitionsMaximumAsync(It.IsAny<bool>())).ReturnsAsync(allGradeDefinitionsMaximumDtos);

                var result = await gradeController.GetGradeDefinitionsMaximumAsync();
                Assert.AreEqual(result.Count(), allGrades.Count());

                int count = allGrades.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allGradeDefinitionsMaximumDtos[i];
                    var actual = allGrades.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.GradeItem.GradeValue, actual.LetterGrade);
                }
            }

            [TestMethod]
            public async Task GradesController_GetGradesDefinitionsMaximum_NoCache()
            {
                gradeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                gradeController.Request.Headers.CacheControl.NoCache = true;

                gradeServiceMock.Setup(gc => gc.GetGradesDefinitionsMaximumAsync(It.IsAny<bool>())).ReturnsAsync(allGradeDefinitionsMaximumDtos);

                var result = await gradeController.GetGradeDefinitionsMaximumAsync();
                Assert.AreEqual(result.Count(), allGrades.Count());

                int count = allGrades.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allGradeDefinitionsMaximumDtos[i];
                    var actual = allGrades.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.GradeItem.GradeValue, actual.LetterGrade);
                }
            }

            [TestMethod]
            public async Task GradesController_GetGradesDefinitionsMaximum_Cache()
            {
                gradeController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                gradeController.Request.Headers.CacheControl.NoCache = false;

                gradeServiceMock.Setup(gc => gc.GetGradesDefinitionsMaximumAsync(It.IsAny<bool>())).ReturnsAsync(allGradeDefinitionsMaximumDtos);

                var result = await gradeController.GetGradeDefinitionsMaximumAsync();
                Assert.AreEqual(result.Count(), allGrades.Count());

                int count = allGrades.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allGradeDefinitionsMaximumDtos[i];
                    var actual = allGrades.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid); 
                    Assert.AreEqual(expected.GradeItem.GradeValue, actual.LetterGrade);
                }
            }

            [TestMethod]
            public async Task GradesController_GetGradesDefinitionsMaximumId()
            {
                 var thisGradeType = allGradeDefinitionsMaximumDtos.Where(m => m.Id == "d874e05d-9d97-4fa3-8862-5044ef2384d0").FirstOrDefault();

                gradeServiceMock.Setup(x => x.GetGradesDefinitionsMaximumIdAsync(It.IsAny<string>())).ReturnsAsync(thisGradeType);

                var gradeType = await gradeController.GetGradeDefinitionsMaximumByIdAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0");
                
                Assert.AreEqual(thisGradeType.Id, gradeType.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GradeController_GetGradesDefinitionsMaximumThrowsIntAppiExc()
            {
                gradeServiceMock.Setup(gc => gc.GetGradesDefinitionsMaximumAsync(It.IsAny<bool>())).Throws<Exception>();

                await gradeController.GetGradeDefinitionsMaximumAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GradeController_PostGradesDefinitionsMaximumThrowsIntAppiExc()
            {
                await gradeController.PostGradeDefinitionsMaximumAsync(allGradeDefinitionsMaximumDtos[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GradeController_PutGradesDefinitionsMaximumThrowsIntAppiExc()
            {
                var result = await gradeController.PutGradeDefinitionsMaximumAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0", allGradeDefinitionsMaximumDtos[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GradeController_DeleteGradesDefinitionsMaximumByIdThrowsIntAppiExc()
            {
                var result = await gradeController.DeleteGradeDefinitionsMaximumByIdAsync("d874e05d-9d97-4fa3-8862-5044ef2384d0");
            }
        }

        [TestClass]
        public class GradeControllerTests_QueryAnonymousGradingIdsAsync
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

            private ILogger logger;
            private IGradeService gradeService;
            private Mock<IGradeService> gradeServiceMock;
            private IGradeRepository gradeRepo;
            private Mock<IGradeRepository> gradeRepoMock;
            private GradesController gradeController;
            private List<Dtos.Student.StudentAnonymousGrading> studentAnonymousGradingIds;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();

                logger = new Mock<ILogger>().Object;
                gradeServiceMock = new Mock<IGradeService>();
                gradeService = gradeServiceMock.Object;
                gradeRepoMock = new Mock<IGradeRepository>();
                gradeRepo = gradeRepoMock.Object;

                gradeController = new GradesController(adapterRegistryMock.Object, gradeRepo, gradeService, logger);
                gradeController.Request = new HttpRequestMessage();
                gradeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                studentAnonymousGradingIds = new List<Dtos.Student.StudentAnonymousGrading>()
                {
                    new Dtos.Student.StudentAnonymousGrading()
                    { AnonymousGradingId = "GradeId1", SectionId = "SectionIdA", TermId = "TermId1", Message = "Message for GradeId1" },
                    new Dtos.Student.StudentAnonymousGrading()
                    { AnonymousGradingId = "GradeId2", SectionId = "SectionIdB", TermId = "TermId2", Message = "Message for GradeId2" },

                };
        }

        [TestCleanup]
            public void Cleanup()
            {
                gradeRepo = null;
                gradeService = null;
                gradeController = null;
            }

            [TestMethod]
            public async Task QueryAnonymousGradingIdsAsync_ReturnStudentAnonymousGradingDtos()
            {
                var criteria = new Dtos.Student.AnonymousGradingQueryCriteria() { StudentId = "1" };

                // Mock the respository get
                gradeServiceMock.Setup(svc => svc.QueryAnonymousGradingIdsAsync(criteria)).ReturnsAsync(studentAnonymousGradingIds);

                // Take Action
                var studentGradingIds = (await gradeController.QueryAnonymousGradingIdsAsync(criteria)).ToList();

                // Test Result
                Assert.AreEqual(studentGradingIds.Count, studentAnonymousGradingIds.Count);
                for (int i = 0; i < studentGradingIds.Count(); i++)
                {
                    Assert.IsTrue(studentGradingIds[i] is Dtos.Student.StudentAnonymousGrading);
                    Assert.AreEqual(studentGradingIds[i].AnonymousGradingId, studentAnonymousGradingIds[i].AnonymousGradingId);
                    Assert.AreEqual(studentGradingIds[i].SectionId, studentAnonymousGradingIds[i].SectionId);
                    Assert.AreEqual(studentGradingIds[i].TermId, studentAnonymousGradingIds[i].TermId);
                    Assert.AreEqual(studentGradingIds[i].Message, studentAnonymousGradingIds[i].Message);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryAnonymousGradingIdsAsync_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    gradeServiceMock.Setup(svc => svc.QueryAnonymousGradingIdsAsync(null)).Throws(new Exception());
                    var studentGradingIds = await gradeController.QueryAnonymousGradingIdsAsync(null);
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