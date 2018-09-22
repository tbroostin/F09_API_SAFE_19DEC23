// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
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
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                graduationConfigurationEntity = BuildGraduationConfiguration();

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger);
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
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                configurationEntity = BuildStudentRequestConfiguration();

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger);
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
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                startTime = DateTime.Now.AddDays(-100);
                endTime = DateTime.Now.AddDays(-10);
                configurationEntity = BuildCourseCatalogConfiguration();

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger);
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
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                //adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                startTime = DateTime.Now.AddDays(-100);
                endTime = DateTime.Now.AddDays(-10);
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(true, 3); 

                studentConfigurationController = new StudentConfigurationController(studentConfigurationRepo, adapterRegistry, logger);
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
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_ReturnConfigDto_Other()
            {
                configurationEntity = new Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration(false, 0);

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
    }
}
