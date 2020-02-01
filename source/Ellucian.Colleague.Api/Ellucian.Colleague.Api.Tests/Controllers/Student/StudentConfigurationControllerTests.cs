// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.Student.Services;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentConfigurationControllerTests
    {
        [TestClass]
        public class StudentConfigurationControllerTests_GraduationConfiguration
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

            private IStudentConfigurationService configurationService;
            private Mock<IStudentConfigurationService> configurationServiceMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private StudentConfigurationController studentConfigurationController;
            private Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration graduationConfigurationEntity;
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                configurationServiceMock = new Mock<IStudentConfigurationService>();
                configurationService = configurationServiceMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                graduationConfigurationEntity = BuildGraduationConfiguration();

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetGraduationConfigurationAsync_ReturnGraduationConfigDto()
            {
                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetGraduationConfigurationAsync()).Returns(Task.FromResult(graduationConfigurationEntity));

                // Mock the adapters
                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration, Ellucian.Colleague.Dtos.Student.GraduationConfiguration>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration, Ellucian.Colleague.Dtos.Student.GraduationConfiguration>()).Returns(adapter);
                var gradQuestionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationQuestion, Ellucian.Colleague.Dtos.Student.GraduationQuestion>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationQuestion, Ellucian.Colleague.Dtos.Student.GraduationQuestion>()).Returns(gradQuestionAdapter);

                // Take Action
                var graduationConfiguration = await studentConfigurationController.GetGraduationConfigurationAsync();

                // Test Result
                Assert.IsTrue(graduationConfiguration is Dtos.Student.GraduationConfiguration);
                Assert.AreEqual(graduationConfiguration.ApplicationQuestions.Count(), graduationConfiguration.ApplicationQuestions.Count());
                Assert.AreEqual(graduationConfiguration.GraduationTerms.Count(), graduationConfiguration.GraduationTerms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetGraduationConfigurationAsync_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetGraduationConfigurationAsync()).Throws(new KeyNotFoundException());
                    var graduationConfiguration = await studentConfigurationController.GetGraduationConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetGraduationConfigurationAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetGraduationConfigurationAsync()).Throws(new ArgumentNullException());
                    var graduationConfiguration = await studentConfigurationController.GetGraduationConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            private Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration BuildGraduationConfiguration()
            {
                var gradConfig = new Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration();
                gradConfig.AddGraduationQuestion(Ellucian.Colleague.Domain.Student.Entities.GraduationQuestionType.AttendCommencement, false);
                gradConfig.AddGraduationQuestion(Ellucian.Colleague.Domain.Student.Entities.GraduationQuestionType.Hometown, true);
                gradConfig.AddGraduationTerm("term1");
                gradConfig.AddGraduationTerm("term2");
                gradConfig.AddGraduationTerm("term3");
                gradConfig.CommencementInformationLink = "commencementURL.com";
                gradConfig.CapAndGownLink = "capandgown.com";
                return gradConfig;
            }
        }

        [TestClass]
        public class StudentConfigurationControllerTests_StudentRequestConfiguration
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

            private IStudentConfigurationService configurationService;
            private Mock<IStudentConfigurationService> configurationServiceMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private StudentConfigurationController studentConfigurationController;
            private Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration configurationEntity;
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                configurationServiceMock = new Mock<IStudentConfigurationService>();
                configurationService = configurationServiceMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                configurationEntity = BuildStudentRequestConfiguration();

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetStuddentRequestConfigurationAsync_ReturnConfigDto()
            {
                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetStudentRequestConfigurationAsync()).Returns(Task.FromResult(configurationEntity));

                // Mock the adapters
                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration, Ellucian.Colleague.Dtos.Student.StudentRequestConfiguration>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration, Ellucian.Colleague.Dtos.Student.StudentRequestConfiguration>()).Returns(adapter);


                // Take Action
                var configuration = await studentConfigurationController.GetStudentRequestConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.StudentRequestConfiguration);
                Assert.IsTrue(configuration.SendTranscriptRequestConfirmation);
                Assert.IsTrue(configuration.SendEnrollmentRequestConfirmation);
                Assert.AreEqual("PRI", configuration.DefaultWebEmailType);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetTranscriptRequestConfigurationAsync_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetStudentRequestConfigurationAsync()).Throws(new Exception());
                    var configuration = await studentConfigurationController.GetStudentRequestConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            private Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration BuildStudentRequestConfiguration()
            {
                var studentConfig = new Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration();
                studentConfig.DefaultWebEmailType = "PRI";
                studentConfig.SendTranscriptRequestConfirmation = true;
                studentConfig.SendEnrollmentRequestConfirmation = true;
                return studentConfig;
            }
        }

        [TestClass]
        public class StudentConfigurationControllerTests_CourseCatalogConfiguration
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

            private IStudentConfigurationService configurationService;
            private Mock<IStudentConfigurationService> configurationServiceMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private StudentConfigurationController studentConfigurationController;
            private Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration configurationEntity;
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ILogger logger;
            DateTime startTime;
            DateTime endTime;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                configurationServiceMock = new Mock<IStudentConfigurationService>();
                configurationService = configurationServiceMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                startTime = DateTime.Now.AddDays(-100);
                endTime = DateTime.Now.AddDays(-10);
                configurationEntity = BuildCourseCatalogConfiguration();

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetCourseCatalogConfigurationAsync_ReturnConfigDto()
            {
                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetCourseCatalogConfigurationAsync()).Returns(Task.FromResult(configurationEntity));

                // Mock the adapters
                var configAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration>()).Returns(configAdapter);
                var optionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CatalogFilterOption, Ellucian.Colleague.Dtos.Student.CatalogFilterOption>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CatalogFilterOption, Ellucian.Colleague.Dtos.Student.CatalogFilterOption>()).Returns(optionAdapter);

                // Take Action
                var configuration = await studentConfigurationController.GetCourseCatalogConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.CourseCatalogConfiguration);
                Assert.AreEqual(2, configuration.CatalogFilterOptions.Count());
                Assert.AreEqual(startTime, configuration.EarliestSearchDate);
                Assert.AreEqual(endTime, configuration.LatestSearchDate);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetTranscriptRequestConfigurationAsync_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetStudentRequestConfigurationAsync()).Throws(new Exception());
                    var configuration = await studentConfigurationController.GetStudentRequestConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            private Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration BuildCourseCatalogConfiguration()
            {
                var config = new Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration(startTime, endTime);
                config.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.Instructors, true);
                config.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.TopicCodes, false);
                return config;
            }
        }

        [TestClass]
        public class StudentConfigurationControllerTests_CourseCatalogConfiguration2
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

            private IStudentConfigurationService configurationService;
            private Mock<IStudentConfigurationService> configurationServiceMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private StudentConfigurationController studentConfigurationController;
            private Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration configurationEntity;
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ILogger logger;
            DateTime startTime;
            DateTime endTime;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                configurationServiceMock = new Mock<IStudentConfigurationService>();
                configurationService = configurationServiceMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                startTime = DateTime.Now.AddDays(-100);
                endTime = DateTime.Now.AddDays(-10);
                configurationEntity = BuildCourseCatalogConfiguration();

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetCourseCatalogConfiguration2Async_ReturnConfigDto()
            {
                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetCourseCatalogConfiguration2Async()).Returns(Task.FromResult(configurationEntity));

                // Mock the adapters
                var configAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration2>()).Returns(configAdapter);
                var optionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CatalogFilterOption, Ellucian.Colleague.Dtos.Student.CatalogFilterOption2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CatalogFilterOption, Ellucian.Colleague.Dtos.Student.CatalogFilterOption2>()).Returns(optionAdapter);

                // Take Action
                var configuration = await studentConfigurationController.GetCourseCatalogConfiguration2Async();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.CourseCatalogConfiguration2);
                Assert.AreEqual(3, configuration.CatalogFilterOptions.Count());
                Assert.AreEqual(startTime, configuration.EarliestSearchDate);
                Assert.AreEqual(endTime, configuration.LatestSearchDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetTranscriptRequestConfiguration2Async_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetStudentRequestConfigurationAsync()).Throws(new Exception());
                    var configuration = await studentConfigurationController.GetStudentRequestConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            private Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration BuildCourseCatalogConfiguration()
            {
                var config = new Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration(startTime, endTime);
                config.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.Instructors, true);
                config.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.TopicCodes, false);
                config.AddCatalogFilterOption(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.Synonyms, false);
                return config;
            }
        }

        [TestClass]
        public class StudentConfigurationControllerTests_GetRegistrationConfiguration
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

            private IStudentConfigurationService configurationService;
            private Mock<IStudentConfigurationService> configurationServiceMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private StudentConfigurationController studentConfigurationController;
            private Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration configurationEntity;
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ILogger logger;
            DateTime startTime;
            DateTime endTime;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                configurationServiceMock = new Mock<IStudentConfigurationService>();
                configurationService = configurationServiceMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                startTime = DateTime.Now.AddDays(-100);
                endTime = DateTime.Now.AddDays(-10);
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(true, 3);

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_ReturnConfigDto()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(true, 3);

                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetRegistrationConfigurationAsync()).Returns(Task.FromResult(configurationEntity));

                // Mock the adapters
                var configAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration, Ellucian.Colleague.Dtos.Student.RegistrationConfiguration>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration, Ellucian.Colleague.Dtos.Student.RegistrationConfiguration>()).Returns(configAdapter);

                // Take Action
                var configuration = await studentConfigurationController.GetRegistrationConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.RegistrationConfiguration);
                Assert.IsTrue(configuration.RequireFacultyAddAuthorization);
                Assert.AreEqual(3, configuration.AddAuthorizationStartOffsetDays);
                Assert.IsFalse(configuration.QuickRegistrationIsEnabled);
                Assert.IsFalse(configuration.QuickRegistrationTermCodes.Any());
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_ReturnConfigDto_Other()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(false, 0, true);
                configurationEntity.AddQuickRegistrationTerm("2019/FA");

                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetRegistrationConfigurationAsync()).Returns(Task.FromResult(configurationEntity));

                // Mock the adapters
                var configAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration, Ellucian.Colleague.Dtos.Student.RegistrationConfiguration>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration, Ellucian.Colleague.Dtos.Student.RegistrationConfiguration>()).Returns(configAdapter);

                // Take Action
                var configuration = await studentConfigurationController.GetRegistrationConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.RegistrationConfiguration);
                Assert.IsFalse(configuration.RequireFacultyAddAuthorization);
                Assert.AreEqual(0,configuration.AddAuthorizationStartOffsetDays);
                Assert.IsTrue(configuration.QuickRegistrationIsEnabled);
                Assert.AreEqual(1, configuration.QuickRegistrationTermCodes.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetTranscriptRequestConfigurationAsync_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetStudentRequestConfigurationAsync()).Throws(new Exception());
                    var configuration = await studentConfigurationController.GetStudentRequestConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

        }

        [TestClass]
        public class StudentConfigurationControllerTests_StudentProfileConfiguration
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

            private IStudentConfigurationService configurationService;
            private Mock<IStudentConfigurationService> configurationServiceMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private StudentConfigurationController studentConfigurationController;
            private Ellucian.Colleague.Domain.Student.Entities.StudentProfileConfiguration configurationEntity;
            private Dtos.Student.StudentProfileConfiguration configurationDto;
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ILogger logger;
            List<string> PhoneTypesHierarchy = new List<string> { "BU", "HO" };
            List<string> EmailTypesHierarchy = new List<string> { "PRI", "SEC" };
            List<string> AddressTypesHierarchy = new List<string> { "PF", "PR" };
            string ProfileFacultyEmailType = "PRI";
            string ProfileFacultyPhoneType = "BU";
            string ProfileAdvsiorType = "GEN";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                configurationServiceMock = new Mock<IStudentConfigurationService>();
                configurationService = configurationServiceMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                configurationEntity = BuildStudentProfileConfiguration();
                configurationDto = BuildStudentProfileConfigurationDto();
                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetStudentProfileConfigurationAsync_ReturnConfigDto()
            {
                // Mock the respository get
                configurationServiceMock.Setup(svc => svc.GetStudentProfileConfigurationAsync()).Returns(Task.FromResult(configurationDto));

                // Take Action
                var configuration = await studentConfigurationController.GetStudentProfileConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.StudentProfileConfiguration);
                Assert.IsTrue(configuration.FacultyPersonConfiguration.ShowAcadamicPrograms);
                Assert.IsTrue(configuration.FacultyPersonConfiguration.ShowPhone);
                Assert.IsTrue(configuration.FacultyPersonConfiguration.ShowAcadLevelStanding);
                Assert.IsTrue(configuration.FacultyPersonConfiguration.ShowAddress);
                Assert.IsTrue(configuration.FacultyPersonConfiguration.ShowAdvisorDetails);
                Assert.IsTrue(configuration.FacultyPersonConfiguration.ShowAdvisorOfficeHours);
                Assert.IsTrue(configuration.FacultyPersonConfiguration.ShowAnticipatedCompletionDate);
                Assert.AreEqual(configuration.PhoneTypesHierarchy.Count, PhoneTypesHierarchy.Count);
                Assert.AreEqual(configuration.EmailTypesHierarchy.Count, EmailTypesHierarchy.Count);
                Assert.AreEqual(configuration.AddressTypesHierarchy.Count, AddressTypesHierarchy.Count);
                Assert.AreEqual(configuration.ProfileFacultyEmailType, ProfileFacultyEmailType);
                Assert.AreEqual(configuration.ProfileFacultyPhoneType, ProfileFacultyPhoneType);
                Assert.AreEqual(configuration.ProfileAdvsiorType, ProfileAdvsiorType);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentProfileConfigurationForFacultyAsync_BadRequest()
            {
                try
                {
                    configurationServiceMock.Setup(svc => svc.GetStudentProfileConfigurationAsync()).Throws(new Exception());
                    var configuration = await studentConfigurationController.GetStudentProfileConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            private Ellucian.Colleague.Domain.Student.Entities.StudentProfileConfiguration BuildStudentProfileConfiguration()
            {
                var studentConfig = new Ellucian.Colleague.Domain.Student.Entities.StudentProfileConfiguration();
                studentConfig.FacultyPersonConfiguration = new Domain.Student.Entities.StudentProfilePersonConfiguration()
                {
                    ShowAcadamicPrograms = true,
                    ShowPhone = true,
                    ShowAcadLevelStanding = true,
                    ShowAddress = true,
                    ShowAdvisorDetails = true,
                    ShowAdvisorOfficeHours = true,
                    ShowAnticipatedCompletionDate = true
                };
                studentConfig.PhoneTypesHierarchy = new List<string> { "BU", "HO" };
                studentConfig.EmailTypesHierarchy = new List<string> { "PRI", "SEC" };
                studentConfig.AddressTypesHierarchy = new List<string> { "PF", "PR" };
                studentConfig.ProfileFacultyEmailType = "PRI";
                studentConfig.ProfileFacultyPhoneType = "BU";
                studentConfig.ProfileAdvsiorType = "GEN";
                return studentConfig;
            }

            private Ellucian.Colleague.Dtos.Student.StudentProfileConfiguration BuildStudentProfileConfigurationDto()
            {
                var studentConfig = new Ellucian.Colleague.Dtos.Student.StudentProfileConfiguration();
                studentConfig.FacultyPersonConfiguration = new Ellucian.Colleague.Dtos.Student.StudentProfilePersonConfiguration()
                {
                    ShowAcadamicPrograms = true,
                    ShowPhone = true,
                    ShowAcadLevelStanding = true,
                    ShowAddress = true,
                    ShowAdvisorDetails = true,
                    ShowAdvisorOfficeHours = true,
                    ShowAnticipatedCompletionDate = true
                };
                studentConfig.PhoneTypesHierarchy = new List<string> { "BU", "HO" };
                studentConfig.EmailTypesHierarchy = new List<string> { "PRI", "SEC" };
                studentConfig.AddressTypesHierarchy = new List<string> { "PF", "PR" };
                studentConfig.ProfileFacultyEmailType = "PRI";
                studentConfig.ProfileFacultyPhoneType = "BU";
                studentConfig.ProfileAdvsiorType = "GEN";
                return studentConfig;
            }
        }
    }
}
