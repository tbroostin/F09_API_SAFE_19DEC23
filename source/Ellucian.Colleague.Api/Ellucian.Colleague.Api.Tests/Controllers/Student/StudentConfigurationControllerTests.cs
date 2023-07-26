// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

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
        public class StudentConfigurationControllerTests_GraduationConfiguration2
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
            public async Task GetGraduationConfiguration2Async_ReturnGraduationConfigDto()
            {
                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetGraduationConfigurationAsync()).Returns(Task.FromResult(graduationConfigurationEntity));

                // Mock the adapters
                var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration, Ellucian.Colleague.Dtos.Student.GraduationConfiguration2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration, Ellucian.Colleague.Dtos.Student.GraduationConfiguration2>()).Returns(adapter);
                var gradQuestionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationQuestion, Ellucian.Colleague.Dtos.Student.GraduationQuestion>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationQuestion, Ellucian.Colleague.Dtos.Student.GraduationQuestion>()).Returns(gradQuestionAdapter);

                // Take Action
                var graduationConfiguration2 = await studentConfigurationController.GetGraduationConfiguration2Async();

                // Test Result
                Assert.AreEqual(graduationConfiguration2.ExpandRequirementSetting, Dtos.Student.ExpandRequirementSetting.Expand);
                Assert.IsTrue(graduationConfiguration2 is Dtos.Student.GraduationConfiguration2);
                Assert.AreEqual(graduationConfiguration2.ApplicationQuestions.Count(), graduationConfiguration2.ApplicationQuestions.Count());
                Assert.AreEqual(graduationConfiguration2.GraduationTerms.Count(), graduationConfiguration2.GraduationTerms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetGraduationConfiguration2Async_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetGraduationConfigurationAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                    var graduationConfiguration2 = await studentConfigurationController.GetGraduationConfiguration2Async();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetGraduationConfiguration2Async_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetGraduationConfigurationAsync()).Throws(new KeyNotFoundException());
                    var graduationConfiguration2 = await studentConfigurationController.GetGraduationConfiguration2Async();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetGraduationConfiguration2Async_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetGraduationConfigurationAsync()).Throws(new ArgumentNullException());
                    var graduationConfiguration2 = await studentConfigurationController.GetGraduationConfiguration2Async();
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
                gradConfig.ExpandRequirementSetting = Domain.Student.Entities.ExpandRequirementSetting.Expand;
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
        public class StudentConfigurationControllerTests_CourseCatalogConfiguration3
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
            private Dtos.Student.CourseCatalogConfiguration3 configurationDto;
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
                logger = new Mock<ILogger>().Object;
                startTime = DateTime.Now.AddDays(-100);
                endTime = DateTime.Now.AddDays(-10);
                configurationDto = BuildCourseCatalogConfigurationDto();

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetCourseCatalogConfiguration3Async_ReturnConfigDto()
            {
                // Mock the service get
                configurationServiceMock.Setup(svc => svc.GetCourseCatalogConfiguration3Async()).Returns(Task.FromResult(configurationDto));

                // Take Action
                var configuration = await studentConfigurationController.GetCourseCatalogConfiguration3Async();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.CourseCatalogConfiguration3);
                Assert.AreEqual(1, configuration.CatalogFilterOptions.Count());
                Assert.AreEqual(startTime, configuration.EarliestSearchDate);
                Assert.AreEqual(endTime, configuration.LatestSearchDate);
                Assert.IsTrue(configuration.ShowCourseSectionBookInformation);
                Assert.IsTrue(configuration.ShowCourseSectionFeeInformation);
                Assert.AreEqual(configurationDto.DefaultSelfServiceCourseCatalogSearchView, configuration.DefaultSelfServiceCourseCatalogSearchView);
                Assert.AreEqual(configurationDto.DefaultSelfServiceCourseCatalogSearchResultView, configuration.DefaultSelfServiceCourseCatalogSearchResultView);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetCourseCatalogConfiguration3Async_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                studentConfigurationRepoMock.Setup(repo => repo.GetStudentRequestConfigurationAsync()).Throws(new Exception());
                var configuration = await studentConfigurationController.GetStudentRequestConfigurationAsync();
            }

            private Dtos.Student.CourseCatalogConfiguration3 BuildCourseCatalogConfigurationDto()
            {
                var config = new Dtos.Student.CourseCatalogConfiguration3()
                {
                    CatalogFilterOptions = new List<Dtos.Student.CatalogFilterOption3>()
                    {
                        new Dtos.Student.CatalogFilterOption3()
                        {
                            Type = Dtos.Student.CatalogFilterType3.TimeStartsEnds,
                            IsHidden = false
                        }
                    },
                    EarliestSearchDate = startTime,
                    LatestSearchDate = endTime,
                    ShowCourseSectionBookInformation = true,
                    ShowCourseSectionFeeInformation = true,
                    DefaultSelfServiceCourseCatalogSearchView = Dtos.Student.SelfServiceCourseCatalogSearchView.AdvancedSearch,
                    DefaultSelfServiceCourseCatalogSearchResultView = Dtos.Student.SelfServiceCourseCatalogSearchResultView.SectionListing
                };
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
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(true, false, false, 3);

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
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(true, false, false, 3);

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
                Assert.IsFalse(configuration.SeatServiceIsEnabled);
                Assert.IsFalse(configuration.ExceedAddAuthCapacity);
                Assert.IsFalse(configuration.BypassAddAuthWaitlist);
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_ReturnConfigDto_Other()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(false, false, false, 0, true);
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
                Assert.AreEqual(0, configuration.AddAuthorizationStartOffsetDays);
                Assert.IsTrue(configuration.QuickRegistrationIsEnabled);
                Assert.AreEqual(1, configuration.QuickRegistrationTermCodes.Count());
                Assert.IsFalse(configuration.SeatServiceIsEnabled);
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_ReturnConfigDto_Other2()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(false, true, true, 0, true);

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
                Assert.AreEqual(0, configuration.AddAuthorizationStartOffsetDays);
                Assert.IsTrue(configuration.QuickRegistrationIsEnabled);
                Assert.IsFalse(configuration.QuickRegistrationTermCodes.Any());
                Assert.IsFalse(configuration.SeatServiceIsEnabled);
                Assert.IsTrue(configuration.ExceedAddAuthCapacity);
                Assert.IsTrue(configuration.BypassAddAuthWaitlist);
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
                Assert.AreEqual(configuration.IsDisplayOfficeHours, true);
            }

            [TestMethod]
            public async Task GetStudentProfileConfigurationAsync_DisplayOffHours_False()
            {
                configurationDto.IsDisplayOfficeHours = false;
                // Mock the respository get
                configurationServiceMock.Setup(svc => svc.GetStudentProfileConfigurationAsync()).Returns(Task.FromResult(configurationDto));
                // Take Action
                var configuration = await studentConfigurationController.GetStudentProfileConfigurationAsync();

                // Test Result
                Assert.AreEqual(configuration.IsDisplayOfficeHours, false);
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
                studentConfig.IsDisplayOfficeHours = true;
                return studentConfig;
            }
        }

        [TestClass]
        public class StudentConfigurationControllerTests_GetInstantEnrollmentConfiguration
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
            private Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration configurationEntity;
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

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetInstantEnrollmentConfigurationAsync_ThrowsWhenDemographicFieldsIsNull()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration(
                    Domain.Student.Entities.InstantEnrollment.AddNewStudentProgramBehavior.Any,
                    new List<Domain.Student.Entities.InstantEnrollment.AcademicProgramOption>()
                    {
                        new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.DFLT", "2014X"),
                        new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.SYSTEMASSIGNED", "2016"),
                    }, "BANK", "US", true, "CEUSER", null, false, null, false);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetInstantEnrollmentConfigurationAsync_ThrowsWhenDemographicFieldsIsEmpty()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration(
                    Domain.Student.Entities.InstantEnrollment.AddNewStudentProgramBehavior.Any,
                    new List<Domain.Student.Entities.InstantEnrollment.AcademicProgramOption>()
                    {
                        new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.DFLT", "2014X"),
                        new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.SYSTEMASSIGNED", "2016"),
                    }, "BANK", "US", true, "CEUSER", null, false, new List<DemographicField>(), false);
            }

            [TestMethod]
            public async Task GetInstantEnrollmentConfigurationAsync_ReturnConfigDto()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration(
                    Domain.Student.Entities.InstantEnrollment.AddNewStudentProgramBehavior.Any,
                    new List<Domain.Student.Entities.InstantEnrollment.AcademicProgramOption>()
                    {
                        new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.DFLT", "2014X"),
                        new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.SYSTEMASSIGNED", "2016"),
                    }, "BANK", "US", true, "CEUSER", null, false,
                    new List<DemographicField>()
                    {
                        new DemographicField("FIRST_NAME", "First Name", DemographicFieldRequirement.Required),
                        new DemographicField("MIDDLE_NAME","Middle Name",DemographicFieldRequirement.Optional),
                        new DemographicField("LAST_NAME","Last Name",DemographicFieldRequirement.Required),
                    }, false);

                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetInstantEnrollmentConfigurationAsync()).Returns(Task.FromResult(configurationEntity));

                // Mock the adapters
                var configAdapter = new InstantEnrollmentConfigurationEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration, Ellucian.Colleague.Dtos.Student.InstantEnrollment.InstantEnrollmentConfiguration>()).Returns(configAdapter);

                // Take Action
                var configuration = await studentConfigurationController.GetInstantEnrollmentConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.InstantEnrollment.InstantEnrollmentConfiguration);
                Assert.AreEqual(Dtos.Student.InstantEnrollment.AddNewStudentProgramBehavior.Any, configuration.StudentProgramAssignmentBehavior);
                Assert.AreEqual(2, configuration.AcademicProgramOptions.Count());
                Assert.AreEqual("BANK", configuration.PaymentDistributionCode);
                Assert.AreEqual("US", configuration.CitizenshipHomeCountryCode);
                Assert.AreEqual(true, configuration.WebPaymentsImplemented);
                Assert.AreEqual("CEUSER", configuration.RegistrationUserRole);
                Assert.AreEqual(false, configuration.ShowInstantEnrollmentBookstoreLink);
                Assert.AreEqual(3, configuration.DemographicFields.Count());
                Assert.AreEqual(false, configuration.AllowNonCitizenRegistration);
            }

            [TestMethod]
            public async Task GetInstantEnrollmentConfigurationAsync_ReturnConfigDto_Other()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration(Domain.Student.Entities.InstantEnrollment.AddNewStudentProgramBehavior.New,
                    new List<Domain.Student.Entities.InstantEnrollment.AcademicProgramOption>()
                    {
                        new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.DFLT", "2014X"),
                        new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.SYSTEMASSIGNED", "2016"),
                    }, "BANK", "US", false, "CEUSER", null, true,
                    new List<DemographicField>()
                    {
                        new DemographicField("FIRST_NAME", "First Name", DemographicFieldRequirement.Required),
                        new DemographicField("MIDDLE_NAME","Middle Name",DemographicFieldRequirement.Optional),
                        new DemographicField("LAST_NAME","Last Name",DemographicFieldRequirement.Required),
                    }, true);


                // Mock the respository get
                studentConfigurationRepoMock.Setup(repo => repo.GetInstantEnrollmentConfigurationAsync()).Returns(Task.FromResult(configurationEntity));

                // Mock the adapters
                var configAdapter = new InstantEnrollmentConfigurationEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration, Ellucian.Colleague.Dtos.Student.InstantEnrollment.InstantEnrollmentConfiguration>()).Returns(configAdapter);

                // Take Action
                var configuration = await studentConfigurationController.GetInstantEnrollmentConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.InstantEnrollment.InstantEnrollmentConfiguration);
                Assert.AreEqual(Dtos.Student.InstantEnrollment.AddNewStudentProgramBehavior.New, configuration.StudentProgramAssignmentBehavior);
                Assert.AreEqual(2, configuration.AcademicProgramOptions.Count());
                Assert.AreEqual("BANK", configuration.PaymentDistributionCode);
                Assert.AreEqual("US", configuration.CitizenshipHomeCountryCode);
                Assert.AreEqual(false, configuration.WebPaymentsImplemented);
                Assert.AreEqual("CEUSER", configuration.RegistrationUserRole);
                Assert.AreEqual(true, configuration.ShowInstantEnrollmentBookstoreLink);
                Assert.AreEqual(3, configuration.DemographicFields.Count());
                Assert.AreEqual(true, configuration.AllowNonCitizenRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetGetInstantEnrollmentConfigurationAsyncAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetInstantEnrollmentConfigurationAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                    await studentConfigurationController.GetInstantEnrollmentConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetGetInstantEnrollmentConfigurationAsyncAsync_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(repo => repo.GetInstantEnrollmentConfigurationAsync()).Throws(new Exception());
                    await studentConfigurationController.GetInstantEnrollmentConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }
        }

        [TestClass]
        public class StudentConfigurationControllerTests_GetAcademicRecordConfigurationAsync
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
                logger = new Mock<ILogger>().Object;

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetAcademicRecordConfigurationAsync_ReturnConfigDto()
            {
                var configurationDto = new Dtos.Student.AcademicRecordConfiguration()
                { AnonymousGradingType = Dtos.Student.AnonymousGradingType.None };

                // Mock the respository get
                configurationServiceMock.Setup(svc => svc.GetAcademicRecordConfigurationAsync()).Returns(Task.FromResult(configurationDto));

                // Take Action
                var configuration = await studentConfigurationController.GetAcademicRecordConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.AcademicRecordConfiguration);
                Assert.AreEqual(Dtos.Student.AnonymousGradingType.None, configuration.AnonymousGradingType);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAcademicRecordConfigurationAsync_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    configurationServiceMock.Setup(svc => svc.GetAcademicRecordConfigurationAsync()).Throws(new Exception());
                    var configuration = await studentConfigurationController.GetAcademicRecordConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }
        }

        [TestClass]
        public class StudentConfigurationControllerTests_GetSectionAvailabilityInformationConfigurationAsync
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
                var configAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionAvailabilityInformationConfiguration, Dtos.Student.SectionAvailabilityInformationConfiguration>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionAvailabilityInformationConfiguration, Dtos.Student.SectionAvailabilityInformationConfiguration>()).Returns(configAdapter);

                logger = new Mock<ILogger>().Object;

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetSectionAvailabilityInformationConfigurationAsync_ReturnConfigDto()
            {
                var configurationEntity = new Domain.Student.Entities.SectionAvailabilityInformationConfiguration(true, true);

                // Mock the respository get
                studentConfigurationRepoMock.Setup(svc => svc.GetSectionAvailabilityInformationConfigurationAsync()).ReturnsAsync(configurationEntity);

                // Take Action
                var configuration = await studentConfigurationController.GetSectionAvailabilityInformationConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.SectionAvailabilityInformationConfiguration);
                Assert.AreEqual(configurationEntity.ShowNegativeSeatCounts, configuration.ShowNegativeSeatCounts);
                Assert.AreEqual(configurationEntity.IncludeSeatsTakenInAvailabilityInformation, configuration.IncludeSeatsTakenInAvailabilityInformation);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionAvailabilityInformationConfigurationAsync_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(svc => svc.GetSectionAvailabilityInformationConfigurationAsync()).Throws(new Exception());
                    var configuration = await studentConfigurationController.GetSectionAvailabilityInformationConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

        }

        [TestClass]
        public class StudentConfigurationControllerTests_GetFacultyAttendanceConfigurationAsync
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
                var configAdapter = new AutoMapperAdapter<Domain.Student.Entities.FacultyAttendanceConfiguration, Dtos.Student.FacultyAttendanceConfiguration>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.FacultyAttendanceConfiguration, Dtos.Student.FacultyAttendanceConfiguration>()).Returns(configAdapter);

                logger = new Mock<ILogger>().Object;

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger, configurationService);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentConfigurationController = null;
                studentConfigurationRepo = null;
            }

            [TestMethod]
            public async Task GetFacultyAttendanceConfigurationAsync_ReturnConfigDto()
            {
                var configurationEntity = new Domain.Student.Entities.FacultyAttendanceConfiguration()
                {
                    CloseAttendanceCensusTrackNumber = 1,
                    CloseAttendanceNumberOfDaysPastCensusTrackDate = 23,
                    CloseAttendanceNumberOfDaysPastSectionEndDate = null,
                };

                // Mock the respository get
                studentConfigurationRepoMock.Setup(svc => svc.GetFacultyAttendanceConfigurationAsync()).ReturnsAsync(configurationEntity);

                // Take Action
                var configuration = await studentConfigurationController.GetFacultyAttendanceConfigurationAsync();

                // Test Result
                Assert.IsTrue(configuration is Dtos.Student.FacultyAttendanceConfiguration);
                Assert.AreEqual(configurationEntity.CloseAttendanceCensusTrackNumber, configuration.CloseAttendanceCensusTrackNumber);
                Assert.AreEqual(configurationEntity.CloseAttendanceNumberOfDaysPastCensusTrackDate, configuration.CloseAttendanceNumberOfDaysPastCensusTrackDate);
                Assert.AreEqual(configurationEntity.CloseAttendanceNumberOfDaysPastSectionEndDate, configuration.CloseAttendanceNumberOfDaysPastSectionEndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetFacultyAttendanceConfigurationAsync_AnyException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(svc => svc.GetFacultyAttendanceConfigurationAsync()).Throws(new Exception());
                    var configuration = await studentConfigurationController.GetFacultyAttendanceConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetFacultyAttendanceConfigurationAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    studentConfigurationRepoMock.Setup(svc => svc.GetFacultyAttendanceConfigurationAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                    var configuration = await studentConfigurationController.GetFacultyAttendanceConfigurationAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }
        }
    }
}
