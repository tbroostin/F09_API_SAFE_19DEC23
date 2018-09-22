// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CourseLevelsControllerTests
    {
        [TestClass]
        public class CourseLevelControllerGet
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

            private CourseLevelsController CourseLevelController;
            private Mock<ICurriculumService> CurriculumServiceMock;
            private Mock<IStudentReferenceDataRepository> StudentReferenceDataRepositoryMock;
            private ICurriculumService CurriculumService;
            private IAdapterRegistry AdapterRegistry;
            private List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel> allCourseLevels;
            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                CurriculumServiceMock = new Mock<ICurriculumService>();
                CurriculumService = CurriculumServiceMock.Object;

                StudentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseLevel, CourseLevel2>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                allCourseLevels = new TestCourseLevelRepository().Get() as List<Ellucian.Colleague.Domain.Student.Entities.CourseLevel>;
                var CourseLevelsList = new List<CourseLevel2>();

                CourseLevelController = new CourseLevelsController(AdapterRegistry, StudentReferenceDataRepositoryMock.Object, CurriculumService, logger);
                CourseLevelController.Request = new HttpRequestMessage();
                CourseLevelController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var courseLevel in allCourseLevels)
                {
                    CourseLevel2 target = ConvertCourseLevelEntitytoCourseLevelDto(courseLevel);
                    CourseLevelsList.Add(target);
                }

                CurriculumServiceMock.Setup<Task<IEnumerable<CourseLevel2>>>(s => s.GetCourseLevels2Async(It.IsAny<bool>())).ReturnsAsync(CourseLevelsList);
            }

            [TestCleanup]
            public void Cleanup()
            {
                CourseLevelController = null;
                CurriculumService = null;
            }

            [TestMethod]
            public async Task ReturnsAllCourseLevels()
            {
                List<CourseLevel2> CourseLevels = await CourseLevelController.GetCourseLevels2Async() as List<CourseLevel2>;
                Assert.AreEqual(CourseLevels.Count, allCourseLevels.Count);
            }

            [TestMethod]
            public async Task GetCourseLevels_LevelProperties()
            {
                List<CourseLevel2> CourseLevels = await CourseLevelController.GetCourseLevels2Async() as List<CourseLevel2>;
                CourseLevel2 al = CourseLevels.Where(a => a.Code == "100").FirstOrDefault();
                Ellucian.Colleague.Domain.Student.Entities.CourseLevel alt = allCourseLevels.Where(a => a.Code == "100").FirstOrDefault();
                Assert.AreEqual(alt.Code, al.Code);
                Assert.AreEqual(alt.Description, al.Title);
            }

            [TestMethod]
            public async Task CourseLevelsController_GetHedmAsync_CacheControlNotNull()
            {
                CourseLevelController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

                List<CourseLevel2> CourseLevels = await CourseLevelController.GetCourseLevels2Async() as List<CourseLevel2>;
                CourseLevel2 cl = CourseLevels.Where(a => a.Code == "100").FirstOrDefault();
                Ellucian.Colleague.Domain.Student.Entities.CourseLevel clt = allCourseLevels.Where(a => a.Code == "100").FirstOrDefault();
                Assert.AreEqual(clt.Code, cl.Code);
                Assert.AreEqual(clt.Description, cl.Title);
            }

            [TestMethod]
            public async Task CourseLevelsController_GetHedmAsync_NoCache()
            {
                CourseLevelController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                CourseLevelController.Request.Headers.CacheControl.NoCache = true;

                List<CourseLevel2> CourseLevels = await CourseLevelController.GetCourseLevels2Async() as List<CourseLevel2>;
                CourseLevel2 cl = CourseLevels.Where(a => a.Code == "100").FirstOrDefault();
                Ellucian.Colleague.Domain.Student.Entities.CourseLevel clt = allCourseLevels.Where(a => a.Code == "100").FirstOrDefault();
                Assert.AreEqual(clt.Code, cl.Code);
                Assert.AreEqual(clt.Description, cl.Title);
            }

            [TestMethod]
            public async Task CourseLevelsController_GetHedmAsync_Cache()
            {
                CourseLevelController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                CourseLevelController.Request.Headers.CacheControl.NoCache = false;

                List<CourseLevel2> CourseLevels = await CourseLevelController.GetCourseLevels2Async() as List<CourseLevel2>;
                CourseLevel2 cl = CourseLevels.Where(a => a.Code == "100").FirstOrDefault();
                Ellucian.Colleague.Domain.Student.Entities.CourseLevel clt = allCourseLevels.Where(a => a.Code == "100").FirstOrDefault();
                Assert.AreEqual(clt.Code, cl.Code);
                Assert.AreEqual(clt.Description, cl.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseLevelController_GetThrowsIntAppiExc()
            {
                CurriculumServiceMock.Setup(gc => gc.GetCourseLevels2Async(It.IsAny<bool>())).Throws<Exception>();

                await CourseLevelController.GetCourseLevels2Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseLevelController_GetByIdThrowsIntAppiExc()
            {
                CurriculumServiceMock.Setup(gc => gc.GetCourseLevelById2Async(It.IsAny<string>())).Throws<Exception>();

                await CourseLevelController.GetCourseLevelById2Async("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseLevelController_DeleteThrowsIntApiExc()
            {
                await CourseLevelController.DeleteCourseLevelsAsync("100");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseLevelController_PostThrowsIntAppiExc()
            {
                CourseLevel2 clDTO = await CourseLevelController.PostCourseLevelsAsync(new CourseLevel2());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseLevelController_PutThrowsIntAppiExc()
            {
                CourseLevel2 clDTO = await CourseLevelController.PutCourseLevelsAsync("hdsks78hdis", new CourseLevel2());
            }

            /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
            /// <summary>
            /// Converts a CourseLevel domain entity to its corresponding Course Level DTO
            /// </summary>
            /// <param name="source">Course Level domain entity</param>
            /// <returns>CourseLevel2 DTO</returns>
            private Dtos.CourseLevel2 ConvertCourseLevelEntitytoCourseLevelDto(Domain.Student.Entities.CourseLevel source)
            {
                var courseLevel = new Dtos.CourseLevel2();
                courseLevel.Id = source.Guid;
                courseLevel.Code = source.Code;
                courseLevel.Title = source.Description;
                courseLevel.Description = null;
                return courseLevel;
            }
        }

        [TestClass]
        public class CourseLevelsControllerTests_2
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<ICurriculumService> curriculumServiceMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

            private string courseLevelId;

            private CourseLevel2 expectedCourseLevel;
            private CourseLevel2 testCourseLevel;
            private CourseLevel2 actualCourseLevel;

            private CourseLevelsController courseLevelsController;


            public async Task<List<CourseLevel2>> getActualCourseLevels()
            {
                return (await courseLevelsController.GetCourseLevels2Async()).ToList();
            }

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                curriculumServiceMock = new Mock<ICurriculumService>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

                courseLevelId = "idc2935b-29e8-675f-907b-15a34da4f433";

                expectedCourseLevel = new CourseLevel2()
                {
                    Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                    Code = "100",
                    Title = "First Yr",
                    Description = null,
                };

                testCourseLevel = new CourseLevel2();
                foreach (var property in typeof(CourseLevel2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    property.SetValue(testCourseLevel, property.GetValue(expectedCourseLevel, null), null);
                }
                curriculumServiceMock.Setup<Task<CourseLevel2>>(s => s.GetCourseLevelById2Async(courseLevelId)).Returns(Task.FromResult(testCourseLevel));

                courseLevelsController = new CourseLevelsController(adapterRegistryMock.Object, studentReferenceDataRepositoryMock.Object, curriculumServiceMock.Object, loggerMock.Object);
                actualCourseLevel = await courseLevelsController.GetCourseLevelById2Async(courseLevelId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                curriculumServiceMock = null;
                studentReferenceDataRepositoryMock = null;
                courseLevelId = null;
                expectedCourseLevel = null;
                testCourseLevel = null;
                actualCourseLevel = null;
                courseLevelsController = null;
            }

            [TestMethod]
            public void CourseLevelsTypeTest()
            {
                Assert.AreEqual(typeof(CourseLevel2), actualCourseLevel.GetType());
                Assert.AreEqual(expectedCourseLevel.GetType(), actualCourseLevel.GetType());
            }

            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var courseLevelProperties = typeof(CourseLevel2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(5, courseLevelProperties.Length);
            }
        }
    }
}
