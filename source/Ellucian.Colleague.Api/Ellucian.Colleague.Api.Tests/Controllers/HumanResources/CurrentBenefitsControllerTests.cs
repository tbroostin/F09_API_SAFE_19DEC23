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
    public class CurrentBenefitsControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<ICurrentBenefitsService> currentBenefitsServiceMock;

        public CurrentBenefitsController controllerUnderTest;

        public void InitializeCurrentBenefitsControllerTests()
        {
            loggerMock = new Mock<ILogger>();
            currentBenefitsServiceMock = new Mock<ICurrentBenefitsService>();
            controllerUnderTest = new CurrentBenefitsController(currentBenefitsServiceMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetEmployeeCurrentBenefitsTests : CurrentBenefitsControllerTests
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
                InitializeCurrentBenefitsControllerTests();
                currentBenefitsServiceMock.Setup(s => s.GetEmployeesCurrentBenefitsAsync(It.IsAny<string>())).ReturnsAsync(new Dtos.HumanResources.EmployeeBenefits());
                currentBenefitsServiceMock.Setup(s => s.GetEmployeesCurrentBenefitsAsync(null)).ReturnsAsync(new Dtos.HumanResources.EmployeeBenefits());
            }

            [TestMethod]
            public async Task GetEmployeesCurrentBenefitsAsync_MethodExecutesNoErrors()
            {
                var result = await controllerUnderTest.GetEmployeeCurrentBenefitsAsync();
                Assert.IsInstanceOfType(result, typeof(Dtos.HumanResources.EmployeeBenefits));
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeesCurrentBenefitsAsync_CatchPermissionsExceptionTest()
            {
                currentBenefitsServiceMock.Setup(s => s.GetEmployeesCurrentBenefitsAsync(null)).ThrowsAsync(new PermissionsException());
                var result = await controllerUnderTest.GetEmployeeCurrentBenefitsAsync();
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task GetEmployeesCurrentBenefitsAsync_CatchGenericExceptionTest()
            {
                currentBenefitsServiceMock.Setup(s => s.GetEmployeesCurrentBenefitsAsync(null)).ThrowsAsync(new Exception());
                var result = await controllerUnderTest.GetEmployeeCurrentBenefitsAsync();
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                currentBenefitsServiceMock = null;
            }
        }
    }
}
