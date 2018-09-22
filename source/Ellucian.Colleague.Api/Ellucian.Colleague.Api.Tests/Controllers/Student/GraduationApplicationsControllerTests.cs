// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
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


            private Ellucian.Colleague.Dtos.Student.GraduationApplication getExpectedApplication()
            {
                var graduationApplicationDto = new Ellucian.Colleague.Dtos.Student.GraduationApplication();
                graduationApplicationDto.StudentId = "0004032";
                graduationApplicationDto.ProgramCode = "MATH.BA";
                graduationApplicationDto.Id = "0004032*MATH.BA";
                graduationApplicationDto.GraduationTerm = "2015/FA";
                graduationApplicationDto.DiplomaName = "New Diploma Name";
                graduationApplicationDto.WillPickupDiploma = false;
                return graduationApplicationDto;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutStudentGraduationApplication_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    graduationApplicationServiceMock.Setup(x => x.UpdateGraduationApplicationAsync(graduationApplicationDto)).Throws(new PermissionsException());
                    var graduationApplication = await graduationApplicationsController.PutGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
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
                    var graduationApplication = await graduationApplicationsController.PutGraduationApplicationAsync("0004032", "MATH.BA", graduationApplicationDto);
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

    }
}