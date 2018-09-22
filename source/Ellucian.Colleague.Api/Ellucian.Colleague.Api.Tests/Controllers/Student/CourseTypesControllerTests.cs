// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CourseTypesControllerTests
    {
        [TestClass]
        public class CourseTypeControllerGet
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

            private CourseTypesController CourseTypesController;

            private Mock<IStudentReferenceDataRepository> CourseTypeRepositoryMock;
            private IStudentReferenceDataRepository CourseTypeRepository;
            private Mock<ICourseCategoriesService> courseCategoriesServiceMock;
            private Mock<ILogger> loggerMock;
            private IAdapterRegistry AdapterRegistry;

            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CourseType> allCourseTypeDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                CourseTypeRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                CourseTypeRepository = CourseTypeRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseType, CourseType>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                courseCategoriesServiceMock = new Mock<ICourseCategoriesService>();
                loggerMock = new Mock<ILogger>();
                

                allCourseTypeDtos = new TestCourseTypeRepository().Get();
                var CourseTypesList = new List<CourseType>();

                CourseTypesController = new CourseTypesController(AdapterRegistry, CourseTypeRepository, courseCategoriesServiceMock.Object, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.CourseType, CourseType>();
                foreach (var CourseType in allCourseTypeDtos)
                {
                    CourseType target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.CourseType, CourseType>(CourseType);
                    CourseTypesList.Add(target);
                }
                CourseTypeRepositoryMock.Setup(x => x.GetCourseTypesAsync(false)).Returns(Task.FromResult((allCourseTypeDtos)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                CourseTypesController = null;
                CourseTypeRepository = null;
            }


            [TestMethod]
            public async Task ReturnsAllCourseTypes()
            {
                var CourseTypes = await CourseTypesController.GetAsync();
                Assert.AreEqual(CourseTypes.Count(), allCourseTypeDtos.Count());
            }

        }

        [TestClass]
        public class CourseCategoriesControllerTests
        {
            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext { get; set; }

            private IAdapterRegistry AdapterRegistry;
            private Mock<IStudentReferenceDataRepository> courseTypeRepositoryMock;
            private IStudentReferenceDataRepository courseTypeRepository;

            private Mock<ICourseCategoriesService> courseCategoriesServiceMock;
            private Mock<ILogger> loggerMock;
            private CourseTypesController courseCategoriesController;
            private IEnumerable<Domain.Student.Entities.CourseType> allCourseTypes;
            private List<Dtos.CourseCategories> courseCategoriesCollection;
            private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                courseCategoriesServiceMock = new Mock<ICourseCategoriesService>();
                loggerMock = new Mock<ILogger>();
                courseCategoriesCollection = new List<Dtos.CourseCategories>();

                courseTypeRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                courseTypeRepository = courseTypeRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                var logger = loggerMock.Object;
                AdapterRegistry = new AdapterRegistry(adapters, logger);

                allCourseTypes = new List<Domain.Student.Entities.CourseType>()
                {
                    new Domain.Student.Entities.CourseType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.CourseType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.CourseType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

                foreach (var source in allCourseTypes)
                {
                    var courseCategories = new Ellucian.Colleague.Dtos.CourseCategories
                    {
                        Id = source.Guid,
                        Code = source.Code,
                        Title = source.Description,
                        Description = null
                    };
                    courseCategoriesCollection.Add(courseCategories);
                }

                courseCategoriesController = new CourseTypesController(AdapterRegistry, courseTypeRepository,  courseCategoriesServiceMock.Object, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
                courseCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseCategoriesController = null;
                allCourseTypes = null;
                courseCategoriesCollection = null;
                loggerMock = null;
                courseCategoriesServiceMock = null;
            }

            [TestMethod]
            public async Task CourseCategoriesController_GetCourseCategories_ValidateFields_Nocache()
            {
                courseCategoriesController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesAsync(false)).ReturnsAsync(courseCategoriesCollection);

                var sourceContexts = (await courseCategoriesController.GetCourseCategoriesAsync()).ToList();
                Assert.AreEqual(courseCategoriesCollection.Count, sourceContexts.Count);
                for (var i = 0; i < sourceContexts.Count; i++)
                {
                    var expected = courseCategoriesCollection[i];
                    var actual = sourceContexts[i];
                    Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                    Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                    Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                }
            }

            [TestMethod]
            public async Task CourseCategoriesController_GetCourseCategories_ValidateFields_Cache()
            {
                courseCategoriesController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesAsync(true)).ReturnsAsync(courseCategoriesCollection);

                var sourceContexts = (await courseCategoriesController.GetCourseCategoriesAsync()).ToList();
                Assert.AreEqual(courseCategoriesCollection.Count, sourceContexts.Count);
                for (var i = 0; i < sourceContexts.Count; i++)
                {
                    var expected = courseCategoriesCollection[i];
                    var actual = sourceContexts[i];
                    Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                    Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                    Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategories_KeyNotFoundException()
            {
                //
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesAsync(false))
                    .Throws<KeyNotFoundException>();
                await courseCategoriesController.GetCourseCategoriesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategories_PermissionsException()
            {

                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesAsync(false))
                    .Throws<PermissionsException>();
                await courseCategoriesController.GetCourseCategoriesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategories_ArgumentException()
            {

                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesAsync(false))
                    .Throws<ArgumentException>();
                await courseCategoriesController.GetCourseCategoriesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategories_RepositoryException()
            {

                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesAsync(false))
                    .Throws<RepositoryException>();
                await courseCategoriesController.GetCourseCategoriesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategories_IntegrationApiException()
            {

                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesAsync(false))
                    .Throws<IntegrationApiException>();
                await courseCategoriesController.GetCourseCategoriesAsync();
            }

            [TestMethod]
            public async Task CourseCategoriesController_GetCourseCategoriesByGuidAsync_ValidateFields()
            {
                var expected = courseCategoriesCollection.FirstOrDefault();
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesByGuidAsync(expected.Id)).ReturnsAsync(expected);

                var actual = await courseCategoriesController.GetCourseCategoriesByGuidAsync(expected.Id);

                Assert.AreEqual(expected.Id, actual.Id, "Id");
                Assert.AreEqual(expected.Title, actual.Title, "Title");
                Assert.AreEqual(expected.Code, actual.Code, "Code");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategories_Exception()
            {
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesAsync(false)).Throws<Exception>();
                await courseCategoriesController.GetCourseCategoriesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategoriesByGuidAsync_Exception()
            {
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
                await courseCategoriesController.GetCourseCategoriesByGuidAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategoriesByGuid_KeyNotFoundException()
            {
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesByGuidAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();
                await courseCategoriesController.GetCourseCategoriesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategoriesByGuid_PermissionsException()
            {
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesByGuidAsync(It.IsAny<string>()))
                    .Throws<PermissionsException>();
                await courseCategoriesController.GetCourseCategoriesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategoriesByGuid_ArgumentException()
            {
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesByGuidAsync(It.IsAny<string>()))
                    .Throws<ArgumentException>();
                await courseCategoriesController.GetCourseCategoriesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategoriesByGuid_RepositoryException()
            {
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesByGuidAsync(It.IsAny<string>()))
                    .Throws<RepositoryException>();
                await courseCategoriesController.GetCourseCategoriesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategoriesByGuid_IntegrationApiException()
            {
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesByGuidAsync(It.IsAny<string>()))
                    .Throws<IntegrationApiException>();
                await courseCategoriesController.GetCourseCategoriesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_GetCourseCategoriesByGuid_Exception()
            {
                courseCategoriesServiceMock.Setup(x => x.GetCourseCategoriesByGuidAsync(It.IsAny<string>()))
                    .Throws<Exception>();
                await courseCategoriesController.GetCourseCategoriesByGuidAsync(expectedGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_PostCourseCategoriesAsync_Exception()
            {
                await courseCategoriesController.PostCourseCategoriesAsync(courseCategoriesCollection.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_PutCourseCategoriesAsync_Exception()
            {
                var sourceContext = courseCategoriesCollection.FirstOrDefault();
                await courseCategoriesController.PutCourseCategoriesAsync(sourceContext.Id, sourceContext);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CourseCategoriesController_DeleteCourseCategoriesAsync_Exception()
            {
                await courseCategoriesController.DeleteCourseCategoriesAsync(courseCategoriesCollection.FirstOrDefault().Id);
            }
        }
    }
}
