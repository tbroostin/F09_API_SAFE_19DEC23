/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class StudentNsldsInformationControllerTests
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

        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IStudentNsldsInformationService> nsldsInformationServiceMock;

        public StudentNsldsInformationController nsldsInformationController;        

        public StudentNsldsInformation expectedNsldsInformation;
        public StudentNsldsInformation actualNsldsInformation;

        string studentId;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            nsldsInformationServiceMock = new Mock<IStudentNsldsInformationService>();

            studentId = "0004791";
            expectedNsldsInformation = new StudentNsldsInformation()
            {
                StudentId = studentId,
                PellLifetimeEligibilityUsedPercentage = 4567.89m
            };
            
            nsldsInformationServiceMock.Setup(r => r.GetStudentNsldsInformationAsync(It.IsAny<string>())).ReturnsAsync(expectedNsldsInformation);
            
            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.StudentNsldsInformation, StudentNsldsInformation>())
                .Returns(new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentNsldsInformation, StudentNsldsInformation>(adapterRegistryMock.Object, loggerMock.Object));

            nsldsInformationController = new StudentNsldsInformationController(adapterRegistryMock.Object, nsldsInformationServiceMock.Object, loggerMock.Object);

        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            nsldsInformationServiceMock = null;
            nsldsInformationController = null;

            studentId = null;
            expectedNsldsInformation = null;
            actualNsldsInformation = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetNsldsInformation_NullStudentId_ThrowsArgumentNullExceptionTest()
        {
            await nsldsInformationController.GetStudentNsldsInformationAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetNsldsInformation_HandlesKeyNotFoundExceptionTest()
        {
            nsldsInformationServiceMock.Setup(r => r.GetStudentNsldsInformationAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
            try
            {
                await nsldsInformationController.GetStudentNsldsInformationAsync(studentId);
            }
            catch (HttpResponseException hre)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetNsldsInformation_HandlesPermissionsExceptionTest()
        {
            nsldsInformationServiceMock.Setup(r => r.GetStudentNsldsInformationAsync(It.IsAny<string>())).Throws(new PermissionsException());
            try
            {
                await nsldsInformationController.GetStudentNsldsInformationAsync(studentId);
            }
            catch (HttpResponseException hre)
            {
                Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetNsldsInformation_HandlesGenericExceptionTest()
        {
            nsldsInformationServiceMock.Setup(r => r.GetStudentNsldsInformationAsync(It.IsAny<string>())).Throws(new Exception());
            try
            {
                await nsldsInformationController.GetStudentNsldsInformationAsync(studentId);
            }
            catch (HttpResponseException hre)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        public async Task GetNsldsInformation_ActualInformation_EqualsExpectedTest()
        {
            actualNsldsInformation = await nsldsInformationController.GetStudentNsldsInformationAsync(studentId);
            Assert.AreEqual(expectedNsldsInformation.StudentId, actualNsldsInformation.StudentId);
            Assert.AreEqual(expectedNsldsInformation.PellLifetimeEligibilityUsedPercentage, actualNsldsInformation.PellLifetimeEligibilityUsedPercentage);
        }

    }
}
