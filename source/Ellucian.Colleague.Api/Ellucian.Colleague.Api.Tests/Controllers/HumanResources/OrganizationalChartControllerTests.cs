// Copyright 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Ellucian.Data.Colleague.Exceptions;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class OrganizationalChartControllerTests
    {
        #region Variables_Declaration
        private OrganizationalChartController controllerUnderTest;
        private Mock<ILogger> loggerMock;
        private Mock<IOrganizationalChartService> organizationalChartServiceMock;
        private HttpResponse httpResponse;
        private List<OrgChartEmployee> employees;

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
        #endregion

        [TestClass]
        public class OrganizationalChartTests : OrganizationalChartControllerTests
        {
            private IEnumerable<LeaveRequest> leaveRequests;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                loggerMock = new Mock<ILogger>();
                organizationalChartServiceMock = new Mock<IOrganizationalChartService>();
                controllerUnderTest = new OrganizationalChartController(loggerMock.Object, organizationalChartServiceMock.Object);
                BuildData();
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                leaveRequests = null;
                organizationalChartServiceMock = null;
                controllerUnderTest = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OrganizationalChartController_SessionExpired()
            {
                organizationalChartServiceMock.Setup(s => s.GetOrganizationalChartAsync(It.IsAny<string>())).ThrowsAsync(new ColleagueSessionExpiredException(""));
                var actual = await controllerUnderTest.GetOrganizationalChartAsync("00001");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OrganizationalChartController_PermissionsException()
            {
                organizationalChartServiceMock.Setup(s => s.GetOrganizationalChartAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var actual = await controllerUnderTest.GetOrganizationalChartAsync("00001");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OrganizationalChartController_UnknownException()
            {
                organizationalChartServiceMock.Setup(s => s.GetOrganizationalChartAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await controllerUnderTest.GetOrganizationalChartAsync("00001");
            }

            [TestMethod]
            public async Task OrganizationalChartController_ReturnsData()
            {
                organizationalChartServiceMock.Setup(s => s.GetOrganizationalChartAsync(It.IsAny<string>())).ReturnsAsync(employees);
                var actuals = await controllerUnderTest.GetOrganizationalChartAsync("00001");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(employees, actuals);
            }

            private void BuildData()
            {
                employees = new List<OrgChartEmployee>();
                for (var x = 0; x < 25; x++)
                {
                    var employeeName = new OrgChartEmployeeName()
                    {
                        LastName = "EMPLOYEE_NAME_" + x,
                        FirstName = "EMPLOYEE_NAME_" + x,
                        FullName = "EMPLOYEE_NAME_" + x
                    };
                    employees.Add(new OrgChartEmployee()
                    {
                        PersonPositionId = "TEST_POS",
                        ParentPersonPositionId = "PARENT_POS",
                        PositionCode = "POS_CODE",
                        LocationCode = "LOC",
                        EmployeeName = employeeName
                    });
                }
            }
        }
    }
}