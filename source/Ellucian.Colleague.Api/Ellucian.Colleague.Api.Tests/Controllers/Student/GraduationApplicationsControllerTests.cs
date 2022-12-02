// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class GraduationApplicationsControllerTests
    {
        [TestClass]
        public class GraduationApplicationsControllerTests_Get
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
            private IGraduationApplicationService graduationApplicationService;
            private Mock<IGraduationApplicationService> graduationApplicationServiceMock;
            private GraduationApplicationsController graduationApplicationsController;
            private Ellucian.Colleague.Dtos.Student.GraduationApplication graduationApplicationDto;
            List<Dtos.Student.GraduationApplication> graduationApplicationsList = new List<Dtos.Student.GraduationApplication>();
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                graduationApplicationServiceMock = new Mock<IGraduationApplicationService>();
                graduationApplicationService = graduationApplicationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                graduationApplicationDto = new Ellucian.Colleague.Dtos.Student.GraduationApplication();
                graduationApplicationDto.StudentId = "0004032";
                graduationApplicationDto.ProgramCode = "MATH.BA";
                graduationApplicationDto.Id = "0004032*MATH.BA";
                graduationApplicationsList.Add(graduationApplicationDto);
                graduationApplicationsController = new GraduationApplicationsController(graduationApplicationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                graduationApplicationsController = null;
                graduationApplicationService = null;
            }

            [TestMethod]
            public async Task GetStudentGraduationApplication_ForGivenStudentId_ReturnGraduationApplicationDto()
            {
                graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(graduationApplicationDto));
                var graduationApplication = await graduationApplicationsController.GetGraduationApplicationAsync("0004032", "MATH.BA");
                Assert.IsTrue(graduationApplication is Dtos.Student.GraduationApplication);
                Assert.AreEqual("0004032*MATH.BA", graduationApplicationDto.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplication_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new ColleagueSessionExpiredException("session expired"));
                    var graduationApplication = await graduationApplicationsController.GetGraduationApplicationAsync("0004032", "MATH.BA");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplication_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new PermissionsException());
                    var graduationApplication = await graduationApplicationsController.GetGraduationApplicationAsync("0004032", "MATH.BA");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplication_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new KeyNotFoundException());
                    var graduationApplication = await graduationApplicationsController.GetGraduationApplicationAsync("0004032", "MATH.BA");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplication_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new ApplicationException());
                    var graduationApplication = await graduationApplicationsController.GetGraduationApplicationAsync("0004032", "MATH.BA");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

        }
        [TestClass]
        public class GraduationApplicationsControllerTests_GetMultiple
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
            private IGraduationApplicationService graduationApplicationService;
            private Mock<IGraduationApplicationService> graduationApplicationServiceMock;
            private GraduationApplicationsController graduationApplicationsController;
            private Ellucian.Colleague.Dtos.Student.GraduationApplication graduationApplicationDto_1;
            private Ellucian.Colleague.Dtos.Student.GraduationApplication graduationApplicationDto_2;
            List<Dtos.Student.GraduationApplication> graduationApplicationsList = new List<Dtos.Student.GraduationApplication>();
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                graduationApplicationServiceMock = new Mock<IGraduationApplicationService>();
                graduationApplicationService = graduationApplicationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                graduationApplicationDto_1 = new Ellucian.Colleague.Dtos.Student.GraduationApplication();
                graduationApplicationDto_1.StudentId = "0004032";
                graduationApplicationDto_1.ProgramCode = "MATH.BA";
                graduationApplicationDto_1.Id = "0004032*MATH.BA";
                graduationApplicationsList.Add(graduationApplicationDto_1);
                graduationApplicationsController = new GraduationApplicationsController(graduationApplicationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                graduationApplicationsController = null;
                graduationApplicationService = null;
            }

            [TestMethod]
            public async Task GetStudentGraduationApplications_ForGivenStudentId_ReturnGraduationApplicationList()
            {
                graduationApplicationServiceMock.Setup<Task<PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(new PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>(graduationApplicationsList, false));
                var graduationApplications = (await graduationApplicationsController.GetGraduationApplicationsAsync("0004032")).ToList();
                Assert.IsTrue(graduationApplications is List<Dtos.Student.GraduationApplication>);
                Assert.IsNotNull(graduationApplications);

            }

            [TestMethod]
            public async Task GetStudentGraduationApplications_ForGivenStudentId_ReturnGraduationApplicationListOfSize_1()
            {
                graduationApplicationServiceMock.Setup<Task<PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(new PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>(graduationApplicationsList, false));
                var graduationApplications = (await graduationApplicationsController.GetGraduationApplicationsAsync("0004032")).ToList();
                Assert.IsTrue(graduationApplications is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(graduationApplications.Count, 1);
            }

            [TestMethod]
            public async Task GetStudentGraduationApplications_ForGivenStudentId_ReturnGraduationApplicationListOfSize_2()
            {
                graduationApplicationDto_2 = new Ellucian.Colleague.Dtos.Student.GraduationApplication();
                graduationApplicationDto_2.StudentId = "0004032";
                graduationApplicationDto_2.ProgramCode = "CS.BA";
                graduationApplicationDto_2.Id = "0004032*CS.BA";
                graduationApplicationsList.Add(graduationApplicationDto_2);
                graduationApplicationServiceMock.Setup<Task<PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(new PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>(graduationApplicationsList, false));
                var graduationApplications = (await graduationApplicationsController.GetGraduationApplicationsAsync("0004032")).ToList();
                Assert.IsTrue(graduationApplications is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(graduationApplications.Count, 2);
            }

            [TestMethod]
            public async Task GetStudentGraduationApplications_ForGivenStudentId_ReturnCorrectGraduationApplicationListDtos()
            {
                graduationApplicationDto_2 = new Ellucian.Colleague.Dtos.Student.GraduationApplication();
                graduationApplicationDto_2.StudentId = "0004032";
                graduationApplicationDto_2.ProgramCode = "CS.BA";
                graduationApplicationDto_2.Id = "0004032*CS.BA";
                graduationApplicationsList.Add(graduationApplicationDto_2);
                graduationApplicationServiceMock.Setup<Task<PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>>>(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).ReturnsAsync(new PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>(graduationApplicationsList, false));
                var graduationApplications = (await graduationApplicationsController.GetGraduationApplicationsAsync("0004032")).ToList();
                Assert.IsTrue(graduationApplications is List<Dtos.Student.GraduationApplication>);
                Assert.AreEqual(graduationApplications[0].Id, graduationApplicationDto_1.Id);
                Assert.AreEqual(graduationApplications[1].Id, graduationApplicationDto_2.Id);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplications_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).Throws(new ColleagueSessionExpiredException("session expired"));
                    var graduationApplications = await graduationApplicationsController.GetGraduationApplicationsAsync("0004032");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplications_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).Throws(new PermissionsException());
                    var graduationApplications = await graduationApplicationsController.GetGraduationApplicationsAsync("0004032");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplications_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                    var graduationApplications = await graduationApplicationsController.GetGraduationApplicationsAsync("0004032");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplications_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationsAsync(It.IsAny<string>())).Throws(new ApplicationException());
                    var graduationApplications = await graduationApplicationsController.GetGraduationApplicationsAsync("0004032");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentGraduationApplications_ForNullInput_ReturnsNullException()
            {
                var graduationApplications = await graduationApplicationsController.GetGraduationApplicationsAsync(null);
            }
        }

        public class GraduationApplicationsControllerTests_Post
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

            private IGraduationApplicationService graduationApplicationService;
            private Mock<IGraduationApplicationService> graduationApplicationServiceMock;
            private GraduationApplicationsController graduationApplicationsController;
            private Ellucian.Colleague.Dtos.Student.GraduationApplication graduationApplicationDto;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                graduationApplicationServiceMock = new Mock<IGraduationApplicationService>();
                graduationApplicationService = graduationApplicationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                graduationApplicationDto = new Ellucian.Colleague.Dtos.Student.GraduationApplication();
                graduationApplicationDto.StudentId = "0004032";
                graduationApplicationDto.ProgramCode = "MATH.BA";
                graduationApplicationDto.Id = "0004032*MATH.BA";
                graduationApplicationDto.GraduationTerm = "2015/FA";
                graduationApplicationDto.DiplomaName = "Diploma Name";
                graduationApplicationDto.WillPickupDiploma = true;
                graduationApplicationsController = new GraduationApplicationsController(graduationApplicationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                graduationApplicationsController = null;
                graduationApplicationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostGraduationApplicationAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.CreateGraduationApplicationAsync(graduationApplicationDto)).Throws(new PermissionsException());
                    var graduationApplication = await graduationApplicationsController.PostGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostGraduationApplicationAsync_KeyNotFoundException_ReturnsHttpResponseException_Conflict()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.CreateGraduationApplicationAsync(graduationApplicationDto)).Throws(new ExistingResourceException());
                    var graduationApplication = await graduationApplicationsController.PutGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Conflict, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostGraduationApplicationAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.CreateGraduationApplicationAsync(graduationApplicationDto)).Throws(new ApplicationException());
                    var graduationApplication = await graduationApplicationsController.PutGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }
        }

        [TestClass]
        public class GraduationApplicationsControllerTests_Put
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

            private IGraduationApplicationService graduationApplicationService;
            private Mock<IGraduationApplicationService> graduationApplicationServiceMock;
            private GraduationApplicationsController graduationApplicationsController;
            private Ellucian.Colleague.Dtos.Student.GraduationApplication graduationApplicationDto;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                graduationApplicationServiceMock = new Mock<IGraduationApplicationService>();
                graduationApplicationService = graduationApplicationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                graduationApplicationDto = new Ellucian.Colleague.Dtos.Student.GraduationApplication();
                graduationApplicationDto.StudentId = "0004032";
                graduationApplicationDto.ProgramCode = "MATH.BA";
                graduationApplicationDto.Id = "0004032*MATH.BA";
                graduationApplicationDto.GraduationTerm = "2015/FA";
                graduationApplicationDto.DiplomaName = "Diploma Name";
                graduationApplicationDto.WillPickupDiploma = true;
                graduationApplicationsController = new GraduationApplicationsController(graduationApplicationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                graduationApplicationsController = null;
                graduationApplicationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentGraduationApplication_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.UpdateGraduationApplicationAsync(graduationApplicationDto)).Throws(new ColleagueSessionExpiredException("session expired"));
                    await graduationApplicationsController.PutGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentGraduationApplication_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.UpdateGraduationApplicationAsync(graduationApplicationDto)).Throws(new PermissionsException());
                    await graduationApplicationsController.PutGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentGraduationApplication_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.UpdateGraduationApplicationAsync(graduationApplicationDto)).Throws(new KeyNotFoundException());
                    await graduationApplicationsController.PutGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentGraduationApplication_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.UpdateGraduationApplicationAsync(graduationApplicationDto)).Throws(new ApplicationException());
                    await graduationApplicationsController.PutGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }
        }

        [TestClass]
        public class GraduationApplicationsControllerTests_GetGraduationApplicationFeeAsync
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
            private IGraduationApplicationService graduationApplicationService;
            private Mock<IGraduationApplicationService> graduationApplicationServiceMock;
            private GraduationApplicationsController graduationApplicationsController;
            private Ellucian.Colleague.Dtos.Student.GraduationApplicationFee graduationApplicationFeeDto;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                graduationApplicationServiceMock = new Mock<IGraduationApplicationService>();
                graduationApplicationService = graduationApplicationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                graduationApplicationFeeDto = new Ellucian.Colleague.Dtos.Student.GraduationApplicationFee();
                graduationApplicationFeeDto.StudentId = "0004032";
                graduationApplicationFeeDto.ProgramCode = "MATH.BA";
                graduationApplicationFeeDto.Amount = 30m;
                graduationApplicationFeeDto.PaymentDistributionCode = "BANK";
                graduationApplicationsController = new GraduationApplicationsController(graduationApplicationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                graduationApplicationsController = null;
                graduationApplicationService = null;
            }

            [TestMethod]
            public async Task GetStudentGraduationApplicationFeeAsync_ForGivenStudentId_ReturnsGraduationApplicationFeeDto()
            {
                graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(graduationApplicationFeeDto));
                var graduationApplicationFee = await graduationApplicationsController.GetGraduationApplicationFeeAsync("0004032", "MATH.BA");
                Assert.IsTrue(graduationApplicationFee is Dtos.Student.GraduationApplicationFee);
                Assert.AreEqual("0004032", graduationApplicationFeeDto.StudentId);
                Assert.AreEqual("MATH.BA", graduationApplicationFeeDto.ProgramCode);
                Assert.AreEqual("BANK", graduationApplicationFeeDto.PaymentDistributionCode);
                Assert.AreEqual(30m, graduationApplicationFeeDto.Amount);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplicationFeeAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new ColleagueSessionExpiredException("session expired"));
                    var graduationApplication = await graduationApplicationsController.GetGraduationApplicationAsync("0004032", "MATH.BA");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentGraduationApplicationFeeAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new ApplicationException());
                    var graduationApplication = await graduationApplicationsController.GetGraduationApplicationAsync("0004032", "MATH.BA");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }
        }

        [TestClass]
        public class GraduationApplicationsControllerTests_QueryGraduationApplicationEligibilityAsync
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
            private IGraduationApplicationService graduationApplicationService;
            private Mock<IGraduationApplicationService> graduationApplicationServiceMock;
            private GraduationApplicationsController graduationApplicationsController;
            private IEnumerable<Dtos.Student.GraduationApplicationProgramEligibility> graduationApplicationEligDtos;
            private Dtos.Student.GraduationApplicationEligibilityCriteria criteria;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                graduationApplicationServiceMock = new Mock<IGraduationApplicationService>();
                graduationApplicationService = graduationApplicationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                criteria = new Dtos.Student.GraduationApplicationEligibilityCriteria() { StudentId = "StudentId", ProgramCodes = new List<string>() { "Program1", "Program2" } };

                // Mock up the result from the service
                graduationApplicationEligDtos = new List<Dtos.Student.GraduationApplicationProgramEligibility>()
                {
                       new Dtos.Student.GraduationApplicationProgramEligibility() { ProgramCode = "Program1", IsEligible = true },
                       new Dtos.Student.GraduationApplicationProgramEligibility() { ProgramCode = "Program2", IsEligible = false, IneligibleMessages = new List<string>() {"string1", "string2" } }
                };

                graduationApplicationsController = new GraduationApplicationsController(graduationApplicationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                graduationApplicationsController = null;
                graduationApplicationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryGraduationApplicationFEligibilityAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationEligibilityAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Throws(new PermissionsException());
                    await graduationApplicationsController.QueryGraduationApplicationEligibilityAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryGraduationApplicationFEligibilityAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationEligibilityAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Throws(new ColleagueSessionExpiredException("session expired"));
                    await graduationApplicationsController.QueryGraduationApplicationEligibilityAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryGraduationApplicationFEligibilityAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationEligibilityAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Throws(new ArgumentException());
                    await graduationApplicationsController.QueryGraduationApplicationEligibilityAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task QueryGraduationApplicationFEligibilityAsync_ReturnsGraduationApplicationProgramEligibilityDtos()
            {
                graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationEligibilityAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(graduationApplicationEligDtos));
                var results = await graduationApplicationsController.QueryGraduationApplicationEligibilityAsync(criteria);
                Assert.AreEqual(2, results.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryGraduationApplicationFEligibilityAsync_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationEligibilityAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Throws(new Exception());
                    var results = await graduationApplicationsController.QueryGraduationApplicationEligibilityAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryGraduationApplicationFEligibilityAsync_AnyOtherException_ReturnsHttpResponseException_PermissionsException()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.GetGraduationApplicationEligibilityAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Throws(new PermissionsException());
                    var results = await graduationApplicationsController.QueryGraduationApplicationEligibilityAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }
        }
    }
}