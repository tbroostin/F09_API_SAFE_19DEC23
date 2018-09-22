/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class StudentFinancialAidBudgetComponentsControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IStudentBudgetComponentService> studentBudgetComponentServiceMock;

        public StudentFinancialAidBudgetComponentsController actualController;

        public FunctionEqualityComparer<StudentBudgetComponent> studentBudgetComponentDtoComparer;

        public void StudentBudgetComponentsControllerTestsInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            studentBudgetComponentServiceMock = new Mock<IStudentBudgetComponentService>();

            studentBudgetComponentDtoComparer = new FunctionEqualityComparer<StudentBudgetComponent>(
                (sbc1, sbc2) => (sbc1.AwardYear == sbc2.AwardYear && sbc1.StudentId == sbc2.StudentId && sbc1.BudgetComponentCode == sbc2.BudgetComponentCode),
                (sbc) => (sbc.AwardYear.GetHashCode() ^ sbc.StudentId.GetHashCode() ^ sbc.BudgetComponentCode.GetHashCode()));
        }

        [TestClass]
        public class GetStudentBudgetComponentsTests : StudentFinancialAidBudgetComponentsControllerTests
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

            public List<StudentBudgetComponent> studentBudgetComponentDtos;

            public string studentId;

            [TestInitialize]
            public void Initialize()
            {
                StudentBudgetComponentsControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0003914";

                studentBudgetComponentDtos = new List<StudentBudgetComponent>()
                {
                    new StudentBudgetComponent()
                    {
                        StudentId = studentId,
                        AwardYear = "2014",
                        BudgetComponentCode = "TUITION",
                        CampusBasedOriginalAmount = 5000,
                        CampusBasedOverrideAmount = 5500
                    },
                    new StudentBudgetComponent()
                    {
                        StudentId = studentId,
                        AwardYear = "2015",
                        BudgetComponentCode = "TUITION",
                        CampusBasedOriginalAmount = 5500,
                        CampusBasedOverrideAmount = null
                    }
                };

                studentBudgetComponentServiceMock.Setup(s => s.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, b) => Task.FromResult(studentBudgetComponentDtos.Where(sbc => sbc.StudentId == id)));

                actualController = new StudentFinancialAidBudgetComponentsController(adapterRegistryMock.Object, studentBudgetComponentServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actualStudentBudgets = await actualController.GetStudentFinancialAidBudgetComponentsAsync(studentId);

                CollectionAssert.AreEqual(studentBudgetComponentDtos, actualStudentBudgets.ToList(), studentBudgetComponentDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdIsRequiredTest()
            {
                try
                {
                    await actualController.GetStudentFinancialAidBudgetComponentsAsync(null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionTest()
            {
                studentBudgetComponentServiceMock.Setup(s => s.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await actualController.GetStudentFinancialAidBudgetComponentsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionTest()
            {
                studentBudgetComponentServiceMock.Setup(s => s.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await actualController.GetStudentFinancialAidBudgetComponentsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }

        }
    }
}
