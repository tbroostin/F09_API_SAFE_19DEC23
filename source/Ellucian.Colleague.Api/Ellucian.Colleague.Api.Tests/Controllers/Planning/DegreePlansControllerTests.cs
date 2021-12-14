// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Newtonsoft.Json;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Planning.Tests;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Api.Controllers.Student;

namespace Ellucian.Colleague.Api.Tests.Controllers.Planning
{
    [TestClass]
    public class DegreePlansControllerTests
    {
        [TestClass]
        public class DegreePlanPreview_Get
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

            private Mock<IDegreePlanService> degreePlanServiceMock;
            private IDegreePlanService degreePlanService;
            private DegreePlansController degreePlanController;
            private ILogger logger;
            private ApiSettings apiSettings;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                logger = new Mock<ILogger>().Object;
                degreePlanServiceMock = new Mock<IDegreePlanService>();
                degreePlanService = degreePlanServiceMock.Object;
                degreePlanController = new DegreePlansController(degreePlanService, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanController = null;
                degreePlanService = null;
            }

            [TestMethod]
            public async Task DegreePlanController_GetSamplePlanPreview5Async()
            {
                // Arrange
                int id = 12345;
                string programCode = "ENGL+BA";
                string term = "2015FA";
                DegreePlanPreview5 planPreview = new DegreePlanPreview5()
                {
                    Preview = new DegreePlan4(),
                    MergedDegreePlan = new DegreePlan4(),
                    AcademicHistory = new Dtos.Student.AcademicHistory3()
                };

                // Mock the degree plan service that updates a plan.
                degreePlanServiceMock.Setup(svc => svc.PreviewSampleDegreePlan5Async(id, programCode, term)).Returns(Task.FromResult(planPreview));

                // Act
                var response = await degreePlanController.GetSamplePlanPreview5Async(id, programCode, term);

                // Assert
                Assert.IsInstanceOfType(response, typeof(DegreePlanPreview5));
            }

            [TestMethod]
            public async Task DegreePlanController_GetSamplePlanPreview6Async()
            {
                // Arrange
                int id = 12345;
                string programCode = "ENGL+BA";
                string term = "2015FA";
                DegreePlanPreview6 planPreview = new DegreePlanPreview6()
                {
                    Preview = new DegreePlan4(),
                    MergedDegreePlan = new DegreePlan4(),
                    AcademicHistory = new Dtos.Student.AcademicHistory4()
                };

                // Mock the degree plan service that updates a plan.
                degreePlanServiceMock.Setup(svc => svc.PreviewSampleDegreePlan6Async(id, programCode, term)).Returns(Task.FromResult(planPreview));

                // Act
                var response = await degreePlanController.GetSamplePlanPreview6Async(id, programCode, term);

                // Assert
                Assert.IsInstanceOfType(response, typeof(DegreePlanPreview6));
            }

            [TestMethod]
            public async Task DegreePlanController_GetSamplePlanPreview7Async()
            {
                // Arrange
                int id = 12345;
                string programCode = "ENGL+BA";
                string term = "2015FA";
                DegreePlanPreview6 planPreview = new DegreePlanPreview6()
                {
                    Preview = new DegreePlan4(),
                    MergedDegreePlan = new DegreePlan4(),
                    AcademicHistory = new Dtos.Student.AcademicHistory4()
                };

                // Mock the degree plan service that updates a plan.
                degreePlanServiceMock.Setup(svc => svc.PreviewSampleDegreePlan7Async(id, programCode, term)).Returns(Task.FromResult(planPreview));

                // Act
                var response = await degreePlanController.GetSamplePlanPreview7Async(id, programCode, term);

                // Assert
                Assert.IsInstanceOfType(response, typeof(DegreePlanPreview6));
            }
        }

        [TestClass]
        public class DegreePlanController_PostArchive
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

            private Mock<IDegreePlanService> degreePlanServiceMock;
            private IDegreePlanService degreePlanService;
            private DegreePlansController degreePlanController;

            private ILogger logger;
            private ApiSettings apiSettings;

            TestStudentDegreePlanRepository testStudentDegreePlanRepo = new TestStudentDegreePlanRepository();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                degreePlanServiceMock = new Mock<IDegreePlanService>();
                degreePlanService = degreePlanServiceMock.Object;

                var testTermRepo = new TestTermRepository();
                var testStudentProgramRepo = new TestStudentProgramRepository();

                degreePlanController = new DegreePlansController(degreePlanService, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanController = null;
                degreePlanService = null;
            }

            [TestMethod]
            public async Task DegreePlanController_PostArchive_Success()
            {
                var degreePlan2 = BuildDegreePlanDto(await testStudentDegreePlanRepo.GetAsync(2));
                var degreePlanArchive = BuildDegreePlanArchive(degreePlan2);
                degreePlanServiceMock.Setup(service => service.ArchiveDegreePlanAsync(degreePlan2)).Returns(Task.FromResult(degreePlanArchive));

                var newDegreePlanArchive = await degreePlanController.PostArchiveAsync(degreePlan2);
                Assert.AreEqual(degreePlanArchive, newDegreePlanArchive);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_PostArchiveAsync_NullDegreePlan()
            {
                var newDegreePlanArchive = await degreePlanController.PostArchiveAsync(null);
            }

            private DegreePlan2 BuildDegreePlanDto(Domain.Student.Entities.DegreePlans.DegreePlan degreePlan)
            {
                var degreePlanDto = new DegreePlan2();
                degreePlanDto.Id = degreePlan.Id;
                degreePlanDto.PersonId = degreePlan.PersonId;
                return degreePlanDto;
            }

            private DegreePlanArchive BuildDegreePlanArchive(Dtos.Student.DegreePlans.DegreePlan2 degreePlan)
            {
                var degreePlanArchive = new DegreePlanArchive();
                degreePlanArchive.CreatedDate = DateTime.Now;
                return degreePlanArchive;
            }
        }

        [TestClass]
        public class DegreePlanController_PostArchive2
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

            private Mock<IDegreePlanService> degreePlanServiceMock;
            private IDegreePlanService degreePlanService;
            private DegreePlansController degreePlanController;
            private ILogger logger;
            private ApiSettings apiSettings;

            TestStudentDegreePlanRepository testStudentDegreePlanRepo = new TestStudentDegreePlanRepository();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                degreePlanServiceMock = new Mock<IDegreePlanService>();
                degreePlanService = degreePlanServiceMock.Object;

                var testTermRepo = new TestTermRepository();
                var testStudentProgramRepo = new TestStudentProgramRepository();

                degreePlanController = new DegreePlansController(degreePlanService, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanController = null;
                degreePlanService = null;
            }

            [TestMethod]
            public async Task DegreePlanController_PostArchive2Async_Success()
            {
                var degreePlanEntity = await testStudentDegreePlanRepo.GetAsync(2);
                var degreePlan = BuildDegreePlanDto(degreePlanEntity);
                var degreePlanArchive = BuildDegreePlanArchive(degreePlan);
                degreePlanServiceMock.Setup(service => service.ArchiveDegreePlan2Async(degreePlan)).Returns(Task.FromResult(degreePlanArchive));

                var newDegreePlanArchive = await degreePlanController.PostArchive2Async(degreePlan);
                Assert.AreEqual(degreePlanArchive, newDegreePlanArchive);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_PostArchive2Async_NullDegreePlan()
            {
                var newDegreePlanArchive = await degreePlanController.PostArchive2Async(null);
            }

            private DegreePlan3 BuildDegreePlanDto(Domain.Student.Entities.DegreePlans.DegreePlan degreePlan)
            {
                var degreePlanDto = new DegreePlan3();
                degreePlanDto.Id = degreePlan.Id;
                degreePlanDto.PersonId = degreePlan.PersonId;
                return degreePlanDto;
            }

            private DegreePlanArchive2 BuildDegreePlanArchive(Dtos.Student.DegreePlans.DegreePlan3 degreePlan)
            {
                var degreePlanArchive = new DegreePlanArchive2();
                degreePlanArchive.CreatedDate = DateTime.Now;
                return degreePlanArchive;
            }
        }

        [TestClass]
        public class DegreePlanController_GetDegreePlanArchives
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

            private Mock<IDegreePlanService> degreePlanServiceMock;
            private IDegreePlanService degreePlanService;
            private DegreePlansController degreePlanController;
            private ILogger logger;
            private ApiSettings apiSettings;

            TestDegreePlanArchiveRepository testDegreePlanArchiveRepo = new TestDegreePlanArchiveRepository();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                degreePlanServiceMock = new Mock<IDegreePlanService>();
                degreePlanService = degreePlanServiceMock.Object;

                var testTermRepo = new TestTermRepository();
                var testStudentProgramRepo = new TestStudentProgramRepository();

                degreePlanController = new DegreePlansController(degreePlanService, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanController = null;
                degreePlanService = null;
            }

            [TestMethod]
            public async Task DegreePlanController_GetDegreePlanArchivesAsync_Success()
            {
                var degreePlanArchiveEntities = await testDegreePlanArchiveRepo.GetDegreePlanArchivesAsync(2);
                var degreePlanArchiveDtos = BuildDegreePlanArchiveDtos(degreePlanArchiveEntities);
                degreePlanServiceMock.Setup(service => service.GetDegreePlanArchivesAsync(2)).Returns(Task.FromResult(degreePlanArchiveDtos));

                var archives = await degreePlanController.GetDegreePlanArchivesAsync(2);
                Assert.AreEqual(degreePlanArchiveEntities.Count(), archives.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_GetDegreePlanArchivesAsync_ZeroDegreePlanId()
            {
                var degreePlanArchives = await degreePlanController.GetDegreePlanArchivesAsync(0);
            }

            private IEnumerable<DegreePlanArchive> BuildDegreePlanArchiveDtos(IEnumerable<Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive> degreePlanArchiveEntities)
            {
                List<DegreePlanArchive> archives = new List<DegreePlanArchive>();
                foreach (var archive in degreePlanArchiveEntities)
                {
                    var degreePlanArchiveDto = new DegreePlanArchive();
                    degreePlanArchiveDto.Id = archive.Id;
                    degreePlanArchiveDto.CreatedBy = archive.CreatedBy;
                    degreePlanArchiveDto.CreatedDate = (archive.CreatedDate.GetValueOrDefault(DateTime.Now)).DateTime;
                    degreePlanArchiveDto.DegreePlanId = archive.DegreePlanId;
                    degreePlanArchiveDto.StudentId = archive.StudentId;
                    degreePlanArchiveDto.ReviewedBy = archive.ReviewedBy;
                    degreePlanArchiveDto.ReviewedDate = archive.ReviewedDate.HasValue ? archive.ReviewedDate.Value.DateTime : (DateTime?)null;
                    archives.Add(degreePlanArchiveDto);
                }
                return archives;
            }
        }

        [TestClass]
        public class DegreePlanController_GetDegreePlanArchives2
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

            private Mock<IDegreePlanService> degreePlanServiceMock;
            private IDegreePlanService degreePlanService;
            private DegreePlansController degreePlanController;
            private ILogger logger;
            private ApiSettings apiSettings;

            TestDegreePlanArchiveRepository testDegreePlanArchiveRepo = new TestDegreePlanArchiveRepository();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                logger = new Mock<ILogger>().Object;
                degreePlanServiceMock = new Mock<IDegreePlanService>();
                degreePlanService = degreePlanServiceMock.Object;

                var testTermRepo = new TestTermRepository();
                var testStudentProgramRepo = new TestStudentProgramRepository();

                degreePlanController = new DegreePlansController(degreePlanService, logger, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                degreePlanController = null;
                degreePlanService = null;
            }

            [TestMethod]
            public async Task DegreePlanController_GetDegreePlanArchives2Async_Success()
            {
                var degreePlanArchiveEntities = await testDegreePlanArchiveRepo.GetDegreePlanArchivesAsync(2);
                var degreePlanArchiveDtos = BuildDegreePlanArchiveDtos(degreePlanArchiveEntities);
                degreePlanServiceMock.Setup(service => service.GetDegreePlanArchives2Async(2)).Returns(Task.FromResult(degreePlanArchiveDtos));

                var archives = await degreePlanController.GetDegreePlanArchives2Async(2);
                Assert.AreEqual(degreePlanArchiveEntities.Count(), archives.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DegreePlanController_GetDegreePlanArchives2Async_ZeroDegreePlanId()
            {
                var degreePlanArchives = await degreePlanController.GetDegreePlanArchives2Async(0);
            }

            private IEnumerable<DegreePlanArchive2> BuildDegreePlanArchiveDtos(IEnumerable<Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive> degreePlanArchiveEntities)
            {
                var archives = new List<DegreePlanArchive2>();
                foreach (var archive in degreePlanArchiveEntities)
                {
                    var degreePlanArchiveDto = new DegreePlanArchive2();
                    degreePlanArchiveDto.Id = archive.Id;
                    degreePlanArchiveDto.CreatedBy = archive.CreatedBy;
                    degreePlanArchiveDto.CreatedDate = (archive.CreatedDate.GetValueOrDefault(DateTime.Now)).DateTime;
                    degreePlanArchiveDto.DegreePlanId = archive.DegreePlanId;
                    degreePlanArchiveDto.StudentId = archive.StudentId;
                    degreePlanArchiveDto.ReviewedBy = archive.ReviewedBy;
                    degreePlanArchiveDto.ReviewedDate = archive.ReviewedDate.HasValue ? archive.ReviewedDate.Value.DateTime : (DateTime?)null;
                    archives.Add(degreePlanArchiveDto);
                }
                return archives;
            }
        }

    }
}