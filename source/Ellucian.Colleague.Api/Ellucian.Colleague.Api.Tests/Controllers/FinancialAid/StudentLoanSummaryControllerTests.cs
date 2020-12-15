//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Reflection;
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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class StudentLoanSummaryControllerTests
    {
        [TestClass]
        public class GetStudentLoanSummaryTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentLoanSummaryService> studentLoanSummaryServiceMock;

            private string studentId;

            private StudentLoanSummary expectedStudentLoanSummary;
            private StudentLoanSummary testLoanSummary;
            private StudentLoanSummary actualStudentLoanSummary;

            private StudentLoanSummaryController StudentLoanSummaryController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                studentLoanSummaryServiceMock = new Mock<IStudentLoanSummaryService>();

                studentId = "0003914";

                expectedStudentLoanSummary = new StudentLoanSummary()
                {
                    StudentId = studentId,
                    DirectLoanEntranceInterviewDate = null,
                    DirectLoanMpnExpirationDate = DateTime.Today,
                    GraduatePlusLoanEntranceInterviewDate = new DateTime(2014, 03, 14),
                    PlusLoanMpnExpirationDate = null,
                    StudentLoanHistory = new List<StudentLoanHistory>()
                };

                testLoanSummary = new StudentLoanSummary();
                foreach (var property in typeof(StudentLoanSummary).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    property.SetValue(testLoanSummary, property.GetValue(expectedStudentLoanSummary, null), null);
                }
                studentLoanSummaryServiceMock.Setup<Task<StudentLoanSummary>>(s => s.GetStudentLoanSummaryAsync(studentId)).ReturnsAsync(testLoanSummary);

                StudentLoanSummaryController = new StudentLoanSummaryController(adapterRegistryMock.Object, studentLoanSummaryServiceMock.Object, loggerMock.Object);
                actualStudentLoanSummary = await StudentLoanSummaryController.GetStudentLoanSummaryAsync(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                studentLoanSummaryServiceMock = null;
                expectedStudentLoanSummary = null;
                testLoanSummary = null;
                actualStudentLoanSummary = null;
                StudentLoanSummaryController = null;
            }

            [TestMethod]
            public void StudentLoanSummaryTypeTest()
            {
                Assert.AreEqual(typeof(StudentLoanSummary), actualStudentLoanSummary.GetType());
                Assert.AreEqual(expectedStudentLoanSummary.GetType(), actualStudentLoanSummary.GetType());
            }

            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var studentLoanSummaryProperties = typeof(StudentLoanSummary).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(8, studentLoanSummaryProperties.Length);
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                var studentLoanSummaryProperties = typeof(StudentLoanSummary).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in studentLoanSummaryProperties)
                {
                    var expectedPropertyValue = property.GetValue(expectedStudentLoanSummary, null);
                    var actualPropertyValue = property.GetValue(actualStudentLoanSummary, null);
                    Assert.AreEqual(expectedPropertyValue, actualPropertyValue);
                }

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                await StudentLoanSummaryController.GetStudentLoanSummaryAsync(null);
            }

            [TestMethod]
            public async Task StudentIdRequired_BadRequestResponseTest()
            {
                bool exceptionCaught = false;
                try
                {
                    await StudentLoanSummaryController.GetStudentLoanSummaryAsync(null);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);

                }

                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task CatchPermissionsExceptionTest()
            {
                studentLoanSummaryServiceMock.Setup(s => s.GetStudentLoanSummaryAsync(studentId)).Throws(new PermissionsException("Permissions Exception"));

                bool exceptionCaught = false;
                try
                {
                    await StudentLoanSummaryController.GetStudentLoanSummaryAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchKeyNotFoundExceptionTest()
            {
                studentLoanSummaryServiceMock.Setup(s => s.GetStudentLoanSummaryAsync(studentId)).Throws(new KeyNotFoundException("Not Found Exception"));

                bool exceptionCaught = false;
                try
                {
                    await StudentLoanSummaryController.GetStudentLoanSummaryAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchUnknownExceptionTest()
            {
                studentLoanSummaryServiceMock.Setup(s => s.GetStudentLoanSummaryAsync(studentId)).Throws(new Exception("Unkown Exception"));

                bool exceptionCaught = false;
                try
                {
                    await StudentLoanSummaryController.GetStudentLoanSummaryAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }
    }
}
