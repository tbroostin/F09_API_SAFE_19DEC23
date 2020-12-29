/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmployeeCompensationControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IEmployeeCompensationService> employeeCompensationServiceMock;

        public EmployeeCompensationController controllerUnderTest;

        public void InitializeEmployeeCompensationControllerTests()
        {
            loggerMock = new Mock<ILogger>();
            employeeCompensationServiceMock = new Mock<IEmployeeCompensationService>();
            controllerUnderTest = new EmployeeCompensationController(employeeCompensationServiceMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetEmployeeCompensationTests : EmployeeCompensationControllerTests
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

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                InitializeEmployeeCompensationControllerTests();
                employeeCompensationServiceMock.Setup(s => s.GetEmployeeCompensationAsync(It.IsAny<string>(), It.IsAny<decimal?>())).ReturnsAsync(new Dtos.HumanResources.EmployeeCompensation());
                employeeCompensationServiceMock.Setup(s => s.GetEmployeeCompensationAsync(null, null)).ReturnsAsync(new Dtos.HumanResources.EmployeeCompensation());

            }

            [TestMethod]
            public async Task GetEmployeeCompensationAsync_MethodExecutesNoErrors()
            {
                var result = await controllerUnderTest.GetEmployeeCompensationAsync();
                Assert.IsInstanceOfType(result, typeof(Dtos.HumanResources.EmployeeCompensation));
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeCompensationAsync_CatchPermissionsExceptionTest()
            {
                employeeCompensationServiceMock.Setup(s => s.GetEmployeeCompensationAsync(null, null)).ThrowsAsync(new PermissionsException());
                var result = await controllerUnderTest.GetEmployeeCompensationAsync();
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeeCompensationAsync_CatchGenericExceptionTest()
            {
                employeeCompensationServiceMock.Setup(s => s.GetEmployeeCompensationAsync(null, null)).ThrowsAsync(new Exception());
                var result = await controllerUnderTest.GetEmployeeCompensationAsync();
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                employeeCompensationServiceMock = null;
            }
        }
    }
}
