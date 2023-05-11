// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
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
using System.Web.Http.Hosting;
using Ellucian.Web.Security;
using Ellucian.Colleague.Api.Controllers;


namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentRecordsReleaseControllerTests_GetStudentReleaseAccessCodesAsync
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
        private IStudentRecordsReleaseService studentRecordsReleaseService;
        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private StudentRecordsReleaseController studentRecordsReleaseController;
        private IEnumerable<Domain.Student.Entities.StudentReleaseAccess> studentReleaseAccess;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentReleaseAccess, StudentReleaseAccess>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentReleaseAccess, StudentReleaseAccess>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            studentReleaseAccess = BuildStudentReleaseAccess();
            studentRecordsReleaseController = new StudentRecordsReleaseController(adapterRegistry, referenceDataRepository, studentConfigurationRepository, logger, studentRecordsReleaseService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRecordsReleaseController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task StudentRecordsReleaseController_ReturnsStudentReleaseAccessDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetStudentReleaseAccessCodesAsync()).Returns(Task.FromResult(studentReleaseAccess));
            var studentReleaseAccessDtos = await studentRecordsReleaseController.GetStudentReleaseAccessCodesAsync();
            Assert.IsTrue(studentReleaseAccessDtos is IEnumerable<Dtos.Student.StudentReleaseAccess>);
            Assert.AreEqual(2, studentReleaseAccessDtos.Count());
        }

        [TestMethod]
        public async Task StudentRecordsReleaseController_NullRepositoryResponse_ReturnsStudentReleaseAccessDtos()
        {
            IEnumerable<Domain.Student.Entities.StudentReleaseAccess> nullStudentReleaseAccessEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetStudentReleaseAccessCodesAsync()).Returns(Task.FromResult(nullStudentReleaseAccessEntities));
            var studentReleaseAccessDtos = await studentRecordsReleaseController.GetStudentReleaseAccessCodesAsync();
            Assert.IsTrue(studentReleaseAccessDtos is IEnumerable<Dtos.Student.StudentReleaseAccess>);
            Assert.AreEqual(0, studentReleaseAccessDtos.Count());
        }

        [TestMethod]
        public async Task StudentRecordsReleaseController_EmptyRepositoryResponse_ReturnsEmptyStudentReleaseAccessDtos()
        {
            IEnumerable<Domain.Student.Entities.StudentReleaseAccess> emptyStudentReleaseAccessEntities = new List<Domain.Student.Entities.StudentReleaseAccess>();
            referenceDataRepositoryMock.Setup(x => x.GetStudentReleaseAccessCodesAsync()).Returns(Task.FromResult(emptyStudentReleaseAccessEntities));
            var studentReleaseAccessDtos = await studentRecordsReleaseController.GetStudentReleaseAccessCodesAsync();
            Assert.IsTrue(studentReleaseAccessDtos is IEnumerable<Dtos.Student.StudentReleaseAccess>);
            Assert.AreEqual(0, studentReleaseAccessDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRecordsReleaseController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetStudentReleaseAccessCodesAsync()).Throws(new ApplicationException());
                var StudentReleaseAccessCodes = await studentRecordsReleaseController.GetStudentReleaseAccessCodesAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private IEnumerable<Domain.Student.Entities.StudentReleaseAccess> BuildStudentReleaseAccess()
        {
            var studentReleaseAccessCodes = new List<Domain.Student.Entities.StudentReleaseAccess>()
                {
                    new Domain.Student.Entities.StudentReleaseAccess("GRADE","Grade Details", "Graduation grade details"),
                    new Domain.Student.Entities.StudentReleaseAccess("ADR","Address","Present and Permanent Address details")
                };

            return studentReleaseAccessCodes;
        }
    }

    [TestClass]
    public class StudentConfigurationControllerTests_GetStudentRecordsReleaseConfigAsync
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

        private IStudentRecordsReleaseService studentRecordsReleaseService;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        private IStudentReferenceDataRepository referenceDataRepository;
        private StudentRecordsReleaseController studentRecordsReleaseController;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            studentConfigurationRepository = studentConfigurationRepositoryMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var configAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentRecordsReleaseConfig, StudentRecordsReleaseConfig>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.StudentRecordsReleaseConfig, StudentRecordsReleaseConfig>()).Returns(configAdapter);

            logger = new Mock<ILogger>().Object;

            studentRecordsReleaseController = new StudentRecordsReleaseController(adapterRegistry, referenceDataRepository, studentConfigurationRepository, logger, studentRecordsReleaseService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRecordsReleaseController = null;
            studentConfigurationRepository = null;
        }

        [TestMethod]
        public async Task GetStudentRecordsReleaseConfigAsync_ReturnStudentRecordsReleaseConfigtDto()
        {
            var configurationEntity = new Domain.Student.Entities.StudentRecordsReleaseConfig()
            {
                Text = new List<string>() { "Information text related to PIN & FERPA Authorization" },
                IsPinRequired = true
            };

            // Mock the respository get
            studentConfigurationRepositoryMock.Setup(src => src.GetStudentRecordsReleaseConfigAsync()).ReturnsAsync(configurationEntity);

            // Take Action
            var configuration = await studentRecordsReleaseController.GetStudentRecordsReleaseConfigAsync();

            // Test Result
            Assert.IsTrue(configuration is Dtos.Student.StudentRecordsReleaseConfig);
            CollectionAssert.AreEqual(configurationEntity.Text, configuration.Text);
            Assert.IsTrue(configurationEntity.IsPinRequired);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetStudentRecordsReleaseConfigAsync_AnyException_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                studentConfigurationRepositoryMock.Setup(src => src.GetStudentRecordsReleaseConfigAsync()).Throws(new Exception());
                var configuration = await studentRecordsReleaseController.GetStudentRecordsReleaseConfigAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetStudentRecordsReleaseConfigAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                studentConfigurationRepositoryMock.Setup(src => src.GetStudentRecordsReleaseConfigAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                var configuration = await studentRecordsReleaseController.GetStudentRecordsReleaseConfigAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw ex;
            }
        }
    }

    [TestClass]
    public class StudentRecordsReleaseControllerTests_GetStudentRecordsReleaseInformationAsync
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

        private IStudentRecordsReleaseService studentRecordsReleaseService;
        private Mock<IStudentRecordsReleaseService> studentRecordsReleaseServiceMock;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private IStudentReferenceDataRepository referenceDataRepository;
        private StudentRecordsReleaseController studentRecordsReleaseController;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private List<StudentRecordsReleaseInfo> studentRecordsReleaseInfoDtos;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            studentRecordsReleaseServiceMock = new Mock<IStudentRecordsReleaseService>();
            studentRecordsReleaseService = studentRecordsReleaseServiceMock.Object;
            adapterRegistry = new Mock<IAdapterRegistry>().Object;
            logger = new Mock<ILogger>().Object;
            studentRecordsReleaseInfoDtos = BuildstudentRecordsReleaseInfoDtos();
            studentRecordsReleaseController = new StudentRecordsReleaseController(adapterRegistry, referenceDataRepository, studentConfigurationRepository, logger, studentRecordsReleaseService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRecordsReleaseController = null;
            studentRecordsReleaseService = null;
        }

        [TestMethod]
        public async Task GetStudentRecordsReleaseInformationAsync_ReturnsStudentRecordsReleaseInfoDtos()
        {
            studentRecordsReleaseServiceMock.Setup(x => x.GetStudentRecordsReleaseInformationAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<StudentRecordsReleaseInfo>>(studentRecordsReleaseInfoDtos));
            var studentRecordsReleaseInfo = await studentRecordsReleaseController.GetStudentRecordsReleaseInformationAsync("0000015");
            Assert.IsTrue(studentRecordsReleaseInfo is IEnumerable<Dtos.Student.StudentRecordsReleaseInfo>);
            Assert.AreEqual(2, studentRecordsReleaseInfo.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetStudentRecordsReleaseInformationAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
        {
            try
            {
                studentRecordsReleaseServiceMock.Setup(x => x.GetStudentRecordsReleaseInformationAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var studentRecordsReleaseInfo = await studentRecordsReleaseController.GetStudentRecordsReleaseInformationAsync("0000015");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetStudentRecordsReleaseInformationAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                studentRecordsReleaseServiceMock.Setup(x => x.GetStudentRecordsReleaseInformationAsync(It.IsAny<string>())).Throws(new ApplicationException());
                var studentRecordsReleaseInfo = await studentRecordsReleaseController.GetStudentRecordsReleaseInformationAsync("0000015");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        private List<Dtos.Student.StudentRecordsReleaseInfo> BuildstudentRecordsReleaseInfoDtos()
        {
            List<StudentRecordsReleaseInfo> studentsRecordsReleaseInfo = new List<StudentRecordsReleaseInfo>();

            var studentRecordsReleaseInfo1 = new StudentRecordsReleaseInfo()
            {
                Id = "1",
                StudentId = "0000015",
                FirstName = "John",
                LastName = "Peter",
                PIN = "9999",
                RelationType = "Father",
                AccessAreas = new List<string>() { "GRD", "PHONE" },
                StartDate = new DateTime(2022, 05, 10),
                EndDate = new DateTime(2022, 05, 20),
            };
            studentsRecordsReleaseInfo.Add(studentRecordsReleaseInfo1);

            var studentRecordsReleaseInfo2 = new StudentRecordsReleaseInfo()
            {
                Id = "2",
                StudentId = "0000015",
                FirstName = "John",
                LastName = "Peter",
                PIN = "9888",
                RelationType = "Father",
                AccessAreas = new List<string>() { "ADR", "PHONE" },
                StartDate = new DateTime(2022, 05, 20),
                EndDate = new DateTime(2022, 05, 25),
            };
            studentsRecordsReleaseInfo.Add(studentRecordsReleaseInfo2);

            return studentsRecordsReleaseInfo;
        }
    }

    [TestClass]
    public class StudentRecordsReleaseController_PostStudentRecordsReleaseAsync_Tests
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

        private StudentRecordsReleaseController studentRecordsReleaseController;
        private Mock<IStudentRecordsReleaseService> studentRecordsReleaseServiceMock;
        private IStudentRecordsReleaseService studentRecordsReleaseService;
        private ILogger logger;
        public AddStudentReleaseRecordResponse createtranResponse;
        private StudentRecordsReleaseInfo addStudentRecordsReleaseInfo;
        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;



        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            studentRecordsReleaseServiceMock = new Mock<IStudentRecordsReleaseService>();
            studentRecordsReleaseService = studentRecordsReleaseServiceMock.Object;

            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, StudentRecordsReleaseInfo>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, StudentRecordsReleaseInfo>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            // controller that will be tested using mock objects
            studentRecordsReleaseController = new StudentRecordsReleaseController(adapterRegistry, referenceDataRepository, studentConfigurationRepository, logger, studentRecordsReleaseService);
            studentRecordsReleaseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentRecordsReleaseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRecordsReleaseController = null;
            studentRecordsReleaseService = null;
        }

        [TestMethod]
        public async Task AddStudentRecordsReleaseInfo_Success()
        {
            addStudentRecordsReleaseInfo = new StudentRecordsReleaseInfo()
            {
                StudentId = "0000011",
                FirstName = "First",
                LastName = "Last",
                PIN = "1111",
                RelationType = "Mother",
                AccessAreas = new List<string>() { "GRADE", "PHONE" }
            };

            createtranResponse = new AddStudentReleaseRecordResponse()
            {
                OutStudentRecordsReleaseId = "31",
                OutError = "0",
                OutErrorMessages = new List<string>()
            };

            Dtos.Student.StudentRecordsReleaseInfo dtoStudentRecordsReleaseInfo = new StudentRecordsReleaseInfo()
            {
                Id = "31",
                StudentId = "0000011",
                FirstName = "First",
                LastName = "Last",
                PIN = "1111",
                RelationType = "Mother",
                AccessAreas = new List<string>() { "GRADE", "PHONE" }
            };

            studentRecordsReleaseServiceMock.Setup(s => s.AddStudentRecordsReleaseInfoAsync(It.IsAny<StudentRecordsReleaseInfo>())).ReturnsAsync(dtoStudentRecordsReleaseInfo);
            var serviceResponse = await studentRecordsReleaseService.AddStudentRecordsReleaseInfoAsync(addStudentRecordsReleaseInfo);
            Assert.IsNotNull(serviceResponse);
            Assert.AreEqual("31", serviceResponse.Id);
            Assert.AreEqual("0000011", serviceResponse.StudentId);
            Assert.AreEqual("First", serviceResponse.FirstName);
            Assert.AreEqual("Last", serviceResponse.LastName);
            Assert.AreEqual("1111", serviceResponse.PIN);
            Assert.AreEqual("Mother", serviceResponse.RelationType);
            Assert.AreEqual(2, serviceResponse.AccessAreas.Count());
        }
    }

    [TestClass]
    public class StudentRecordsReleaseController_PutStudentRecordsReleaseAsync_Tests
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

        private StudentRecordsReleaseController studentRecordsReleaseController;
        private Mock<IStudentRecordsReleaseService> studentRecordsReleaseServiceMock;
        private IStudentRecordsReleaseService studentRecordsReleaseService;
        private ILogger logger;
        public UpdateStudentReleaseRecordsResponse createtranResponse;
        private StudentRecordsReleaseInfo updateStudentRecordsReleaseInfo;
        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;



        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            studentRecordsReleaseServiceMock = new Mock<IStudentRecordsReleaseService>();
            studentRecordsReleaseService = studentRecordsReleaseServiceMock.Object;

            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, StudentRecordsReleaseInfo>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, StudentRecordsReleaseInfo>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            // controller that will be tested using mock objects
            studentRecordsReleaseController = new StudentRecordsReleaseController(adapterRegistry, referenceDataRepository, studentConfigurationRepository, logger, studentRecordsReleaseService);
            studentRecordsReleaseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentRecordsReleaseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRecordsReleaseController = null;
            studentRecordsReleaseService = null;
        }

        [TestMethod]
        public async Task UpdateStudentRecordsReleaseInfo_Success()
        {
            updateStudentRecordsReleaseInfo = new StudentRecordsReleaseInfo()
            {
                Id = "300",
                StudentId = "0000011",
                FirstName = "First",
                LastName = "Last",
                PIN = "1111",
                RelationType = "Mother",
                AccessAreas = new List<string>() { "GRADE", "PHONE" },
                EndDate = DateTime.Today
            };

            createtranResponse = new UpdateStudentReleaseRecordsResponse()
            {
                OutError = "0",
                OutErrorMessages = new List<string>()
            };

            studentRecordsReleaseServiceMock.Setup(s => s.UpdateStudentRecordsReleaseInfoAsync(It.IsAny<StudentRecordsReleaseInfo>())).ReturnsAsync(updateStudentRecordsReleaseInfo);
            var serviceResponse = await studentRecordsReleaseService.UpdateStudentRecordsReleaseInfoAsync(updateStudentRecordsReleaseInfo);
            Assert.IsNotNull(serviceResponse);
            Assert.AreEqual("300", serviceResponse.Id);
            Assert.AreEqual("0000011", serviceResponse.StudentId);
            Assert.AreEqual("First", serviceResponse.FirstName);
            Assert.AreEqual("Last", serviceResponse.LastName);
            Assert.AreEqual("1111", serviceResponse.PIN);
            Assert.AreEqual("Mother", serviceResponse.RelationType);
            Assert.AreEqual(2, serviceResponse.AccessAreas.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateStudentRecordsReleaseInfo_Null()
        {
            studentRecordsReleaseServiceMock.Setup(s => s.UpdateStudentRecordsReleaseInfoAsync(null)).Throws(new ArgumentNullException());
            var serviceResponse = await studentRecordsReleaseService.UpdateStudentRecordsReleaseInfoAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateStudentRecordsReleaseInfo_FailureNoId()
        {
            updateStudentRecordsReleaseInfo = new StudentRecordsReleaseInfo()
            {
                Id = "",
                StudentId = "0000011",
                FirstName = "First",
                LastName = "Last",
                PIN = "1111",
                RelationType = "Mother",
                AccessAreas = new List<string>() { "GRADE", "PHONE" },
                EndDate = DateTime.Today
            };

            createtranResponse = new UpdateStudentReleaseRecordsResponse()
            {
                OutError = "0",
                OutErrorMessages = new List<string>() { "Id is required" }
            };

            studentRecordsReleaseServiceMock.Setup(s => s.UpdateStudentRecordsReleaseInfoAsync(updateStudentRecordsReleaseInfo)).Throws(new ArgumentNullException());
            var serviceResponse = await studentRecordsReleaseService.UpdateStudentRecordsReleaseInfoAsync(updateStudentRecordsReleaseInfo);


        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateStudentRecordsReleaseInfo_FailureNoAccessAreas()
        {
            updateStudentRecordsReleaseInfo = new StudentRecordsReleaseInfo()
            {
                Id = "300",
                StudentId = "0000011",
                FirstName = "First",
                LastName = "Last",
                PIN = "1111",
                RelationType = "Mother",
                AccessAreas = new List<string>() { },
                EndDate = DateTime.Today
            };

            createtranResponse = new UpdateStudentReleaseRecordsResponse()
            {
                OutError = "0",
                OutErrorMessages = new List<string>() { "Id is required" }
            };

            studentRecordsReleaseServiceMock.Setup(s => s.UpdateStudentRecordsReleaseInfoAsync(updateStudentRecordsReleaseInfo)).Throws(new ArgumentNullException());
            var serviceResponse = await studentRecordsReleaseService.UpdateStudentRecordsReleaseInfoAsync(updateStudentRecordsReleaseInfo);


        }
    }

    [TestClass]
    public class StudentRecordsReleaseControllerTests_GetStudentRecordsReleaseDenyAccessAsync
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

        private IStudentRecordsReleaseService studentRecordsReleaseService;
        private Mock<IStudentRecordsReleaseService> studentRecordsReleaseServiceMock;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private IStudentReferenceDataRepository referenceDataRepository;
        private StudentRecordsReleaseController studentRecordsReleaseController;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private StudentRecordsReleaseDenyAccess studentRecordsReleaseDenyAccessDto;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            studentRecordsReleaseServiceMock = new Mock<IStudentRecordsReleaseService>();
            studentRecordsReleaseService = studentRecordsReleaseServiceMock.Object;
            adapterRegistry = new Mock<IAdapterRegistry>().Object;
            logger = new Mock<ILogger>().Object;
            studentRecordsReleaseDenyAccessDto = new StudentRecordsReleaseDenyAccess()
            {
                DenyAccessToAll = true,
            };
            studentRecordsReleaseController = new StudentRecordsReleaseController(adapterRegistry, referenceDataRepository, studentConfigurationRepository, logger, studentRecordsReleaseService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRecordsReleaseController = null;
            studentRecordsReleaseService = null;
        }

        [TestMethod]
        public async Task GetStudentRecordsReleaseDenyAccessAsync_ReturnsStudentRecordsReleaseDenyAccessDtos()
        {
            studentRecordsReleaseServiceMock.Setup(x => x.GetStudentRecordsReleaseDenyAccessAsync(It.IsAny<string>())).Returns(Task.FromResult<StudentRecordsReleaseDenyAccess>(studentRecordsReleaseDenyAccessDto));
            var studentRecordsReleaseDenyAccess = await studentRecordsReleaseController.GetStudentRecordsReleaseDenyAccessAsync("0000015");
            Assert.IsTrue(studentRecordsReleaseDenyAccess is Dtos.Student.StudentRecordsReleaseDenyAccess);
            Assert.IsTrue(studentRecordsReleaseDenyAccessDto.DenyAccessToAll);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetStudentRecordsReleaseDenyAccessAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
        {
            try
            {
                studentRecordsReleaseServiceMock.Setup(x => x.GetStudentRecordsReleaseDenyAccessAsync(It.IsAny<string>())).Throws(new PermissionsException());
                var studentRecordsReleaseInfo = await studentRecordsReleaseController.GetStudentRecordsReleaseDenyAccessAsync("0000015");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetStudentRecordsReleaseDenyAccessAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                studentRecordsReleaseServiceMock.Setup(x => x.GetStudentRecordsReleaseDenyAccessAsync(It.IsAny<string>())).Throws(new ApplicationException());
                var studentRecordsReleaseInfo = await studentRecordsReleaseController.GetStudentRecordsReleaseDenyAccessAsync("0000015");
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

    }


    [TestClass]
    public class StudentRecordsReleaseController_EndStudentRecordsReleaseAsync_Tests
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

        private StudentRecordsReleaseController studentRecordsReleaseController;
        private Mock<IStudentRecordsReleaseService> studentRecordsReleaseServiceMock;
        private IStudentRecordsReleaseService studentRecordsReleaseService;
        private ILogger logger;
        public DeleteStudentReleaseRecordResponse deleteResponse;
        private StudentRecordsReleaseInfo studentRecordsReleaseInfo;
        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;



        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            studentRecordsReleaseServiceMock = new Mock<IStudentRecordsReleaseService>();
            studentRecordsReleaseService = studentRecordsReleaseServiceMock.Object;

            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, StudentRecordsReleaseInfo>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, StudentRecordsReleaseInfo>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            // controller that will be tested using mock objects
            studentRecordsReleaseController = new StudentRecordsReleaseController(adapterRegistry, referenceDataRepository, studentConfigurationRepository, logger, studentRecordsReleaseService);
            studentRecordsReleaseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentRecordsReleaseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRecordsReleaseController = null;
            studentRecordsReleaseService = null;
        }

        [TestMethod]
        public async Task DeleteStudentRecordsReleaseInfo_Success()
        {
            studentRecordsReleaseInfo = new StudentRecordsReleaseInfo()
            {
                Id = "300",
                StudentId = "0000011",
                FirstName = "First",
                LastName = "Last",
                PIN = "1111",
                RelationType = "Mother",
                AccessAreas = new List<string>() { "GRADE", "PHONE" },
                EndDate = DateTime.Today
            };

            deleteResponse = new DeleteStudentReleaseRecordResponse()
            {
                OutError = "0",
                OutErrorMessages = new List<string>()
            };

            studentRecordsReleaseServiceMock.Setup(s => s.DeleteStudentRecordsReleaseInfoAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentRecordsReleaseInfo);
            var serviceResponse = await studentRecordsReleaseService.DeleteStudentRecordsReleaseInfoAsync(studentRecordsReleaseInfo.StudentId, studentRecordsReleaseInfo.Id);
            Assert.IsNotNull(serviceResponse);
            Assert.AreEqual("300", serviceResponse.Id);
            Assert.AreEqual("0000011", serviceResponse.StudentId);
            Assert.AreEqual("First", serviceResponse.FirstName);
            Assert.AreEqual("Last", serviceResponse.LastName);
            Assert.AreEqual("1111", serviceResponse.PIN);
            Assert.AreEqual("Mother", serviceResponse.RelationType);
            Assert.AreEqual(2, serviceResponse.AccessAreas.Count());
            Assert.AreEqual(DateTime.Today, serviceResponse.EndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteStudentRecordsReleaseInfo_Null()
        {
            studentRecordsReleaseServiceMock.Setup(s => s.DeleteStudentRecordsReleaseInfoAsync(null, null)).Throws(new ArgumentNullException());
            var serviceResponse = await studentRecordsReleaseService.DeleteStudentRecordsReleaseInfoAsync(null,null);
        }
    }


    [TestClass]
    public class StudentRecordsReleaseController_DenyStudentRecordsReleaseAccessAsync_Tests
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

        private StudentRecordsReleaseController studentRecordsReleaseController;
        private Mock<IStudentRecordsReleaseService> studentRecordsReleaseServiceMock;
        private IStudentRecordsReleaseService studentRecordsReleaseService;
        private ILogger logger;
        public StudentRecordsReleaseDenyAccessAllResponse denyAccessAllTranResponse;
        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private DenyStudentRecordsReleaseAccessInformation denyStudentRecordsReleaseAccessInformation;



        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            studentRecordsReleaseServiceMock = new Mock<IStudentRecordsReleaseService>();
            studentRecordsReleaseService = studentRecordsReleaseServiceMock.Object;

            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, StudentRecordsReleaseInfo>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo, StudentRecordsReleaseInfo>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            // controller that will be tested using mock objects
            studentRecordsReleaseController = new StudentRecordsReleaseController(adapterRegistry, referenceDataRepository, studentConfigurationRepository, logger, studentRecordsReleaseService);
            studentRecordsReleaseController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentRecordsReleaseController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRecordsReleaseController = null;
            studentRecordsReleaseService = null;
        }

        [TestMethod]
        public async Task DenyStudentRecordsReleaseAccessAsync_Success()
        {
            List<StudentRecordsReleaseInfo> updatedStudentRecordsReleaseInfo = new List<StudentRecordsReleaseInfo>();
            var updatedStudentRecordsReleaseInfo1 = new StudentRecordsReleaseInfo()
            {
                Id = "300",
                StudentId = "0000011",
                FirstName = "First",
                LastName = "Last",
                PIN = "1111",
                RelationType = "Mother",
                AccessAreas = new List<string>() { "GRADE", "PHONE" },
                StartDate = new DateTime(2022, 05, 10),
                EndDate = DateTime.Today
            };
            updatedStudentRecordsReleaseInfo.Add(updatedStudentRecordsReleaseInfo1);

            denyStudentRecordsReleaseAccessInformation = new DenyStudentRecordsReleaseAccessInformation()
            {
                DenyAccessToAll = true,
                StudentId = "0000011"

            };

            denyAccessAllTranResponse = new StudentRecordsReleaseDenyAccessAllResponse()
            {
                OutError = "0",
                OutErrorMessages = new List<string>()
            };

            studentRecordsReleaseServiceMock.Setup(s => s.DenyStudentRecordsReleaseAccessAsync(It.IsAny<DenyStudentRecordsReleaseAccessInformation>())).ReturnsAsync(updatedStudentRecordsReleaseInfo);
            var serviceResponse = await studentRecordsReleaseService.DenyStudentRecordsReleaseAccessAsync(denyStudentRecordsReleaseAccessInformation);
            Assert.IsNotNull(serviceResponse);
            Assert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).Id, serviceResponse.ElementAt(0).Id);
            Assert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).StudentId, serviceResponse.ElementAt(0).StudentId);
            Assert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).FirstName, serviceResponse.ElementAt(0).FirstName);
            Assert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).LastName, serviceResponse.ElementAt(0).LastName);
            Assert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).PIN, serviceResponse.ElementAt(0).PIN);
            Assert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).RelationType, serviceResponse.ElementAt(0).RelationType);
            CollectionAssert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).AccessAreas, serviceResponse.ElementAt(0).AccessAreas);
            Assert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).StartDate, serviceResponse.ElementAt(0).StartDate);
            Assert.AreEqual(updatedStudentRecordsReleaseInfo.ElementAt(0).EndDate, serviceResponse.ElementAt(0).EndDate);


        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DenyStudentRecordsReleaseAccessAsync_Null()
        {
            studentRecordsReleaseServiceMock.Setup(s => s.DenyStudentRecordsReleaseAccessAsync(null)).Throws(new ArgumentNullException());
            var serviceResponse = await studentRecordsReleaseService.DenyStudentRecordsReleaseAccessAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DenyStudentRecordsReleaseAccessAsync_FailureNoStudentId()
        {
            List<StudentRecordsReleaseInfo> updatedStudentRecordsReleaseInfo = new List<StudentRecordsReleaseInfo>();
            var updatedStudentRecordsReleaseInfo2 = new StudentRecordsReleaseInfo()
            {
                Id = "300",
                StudentId = "0000011",
                FirstName = "First",
                LastName = "Last",
                PIN = "1111",
                RelationType = "Mother",
                AccessAreas = new List<string>() { "GRADE", "PHONE" },
                StartDate = new DateTime(2022, 05, 10),
                EndDate = DateTime.Today
            };
            updatedStudentRecordsReleaseInfo.Add(updatedStudentRecordsReleaseInfo2);

            denyStudentRecordsReleaseAccessInformation = new DenyStudentRecordsReleaseAccessInformation()
            {
                DenyAccessToAll = true,
                StudentId = null

            };
            denyAccessAllTranResponse = new StudentRecordsReleaseDenyAccessAllResponse()
            {
                OutError = "0",
                OutErrorMessages = new List<string>()
            };


            studentRecordsReleaseServiceMock.Setup(s => s.DenyStudentRecordsReleaseAccessAsync(denyStudentRecordsReleaseAccessInformation)).Throws(new ArgumentNullException());
            var serviceResponse = await studentRecordsReleaseService.DenyStudentRecordsReleaseAccessAsync(denyStudentRecordsReleaseAccessInformation);


        }

    }
}
