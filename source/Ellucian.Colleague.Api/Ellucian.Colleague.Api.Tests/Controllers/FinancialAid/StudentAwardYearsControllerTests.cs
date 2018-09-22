//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
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
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class StudentAwardYearsControllerTests
    {
        [TestClass]
        public class GetStudentAwardYearsAsyncTests
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
            private Mock<IStudentAwardYearService> studentAwardYearServiceMock;

            private string studentId;

            private IEnumerable<StudentAwardYear2> expectedStudentAwardYears;
            private List<StudentAwardYear2> testStudentAwardYears;
            private IEnumerable<StudentAwardYear2> actualStudentAwardYears;

            private StudentAwardYearsController StudentAwardYearsController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                studentAwardYearServiceMock = new Mock<IStudentAwardYearService>();

                studentId = "0003914";

                expectedStudentAwardYears = new List<StudentAwardYear2>()
                {
                    new StudentAwardYear2()
                    {
                        Code = "2013"
                    },
                    new StudentAwardYear2()
                    {
                        Code = "2014"
                    }
                };
                testStudentAwardYears = new List<StudentAwardYear2>();
                foreach (var awardYear in expectedStudentAwardYears)
                {
                    var testAwardYear = new StudentAwardYear2();
                    foreach (var property in typeof(StudentAwardYear2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        property.SetValue(testAwardYear, property.GetValue(awardYear, null), null);
                    }
                    testStudentAwardYears.Add(testAwardYear);
                }

                studentAwardYearServiceMock.Setup<Task<IEnumerable<StudentAwardYear2>>>(y => y.GetStudentAwardYears2Async(studentId, It.IsAny<bool>())).ReturnsAsync(testStudentAwardYears);
                StudentAwardYearsController = new StudentAwardYearsController(adapterRegistryMock.Object, studentAwardYearServiceMock.Object, loggerMock.Object);
                actualStudentAwardYears = await StudentAwardYearsController.GetStudentAwardYears2Async(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                studentAwardYearServiceMock = null;
                studentId = null;
                expectedStudentAwardYears = null;
                testStudentAwardYears = null;
                actualStudentAwardYears = null;
                StudentAwardYearsController = null;
            }

            [TestMethod]
            public void StudentAwardYearTypeTest()
            {
                Assert.AreEqual(expectedStudentAwardYears.GetType(), actualStudentAwardYears.GetType());
                foreach (var actualYear in actualStudentAwardYears)
                {
                    Assert.AreEqual(typeof(StudentAwardYear2), actualYear.GetType());
                }
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                var studentAwardYearProperties = typeof(StudentAwardYear2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var expectedYear in expectedStudentAwardYears)
                {
                    var actualYear = actualStudentAwardYears.First(y => y.Code == expectedYear.Code);
                    foreach (var property in studentAwardYearProperties)
                    {
                        var expectedPropertyValue = property.GetValue(expectedYear, null);
                        var actualPropertyValue = property.GetValue(actualYear, null);
                        Assert.AreEqual(expectedPropertyValue, actualPropertyValue);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                await StudentAwardYearsController.GetStudentAwardYears2Async(null);
            }

            [TestMethod]
            public async Task StudentIdRequired_BadRequestResponseTest()
            {
                bool exceptionCaught = false;
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYears2Async(string.Empty);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task CatchPermissionsExceptionAndLogErrorMessageTest()
            {
                studentAwardYearServiceMock.Setup(s => s.GetStudentAwardYears2Async(studentId, It.IsAny<bool>())).Throws(new PermissionsException("Permissions Exception"));

                var exceptionCaught = false;
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYears2Async(studentId);
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
            public async Task CatchNotFoundExceptionAndLogErrorMessageTest()
            {
                studentAwardYearServiceMock.Setup(s => s.GetStudentAwardYears2Async(studentId, It.IsAny<bool>())).Throws(new KeyNotFoundException("Not Found Exception"));

                var exceptionCaught = false;
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYears2Async(studentId);
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
            public async Task CatchUnknownExceptionAndLogErrorMessageTest()
            {
                studentAwardYearServiceMock.Setup(s => s.GetStudentAwardYears2Async(studentId, It.IsAny<bool>())).Throws(new Exception("Unknown Exception"));

                var exceptionCaught = false;
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYears2Async(studentId);
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

        [TestClass]
        public class GetSingleStudentAwardYearAsyncTests
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
            private Mock<IStudentAwardYearService> studentAwardYearServiceMock;

            private string studentId;
            private string awardYear;

            private StudentAwardYear2 expectedStudentAwardYear;
            private StudentAwardYear2 actualStudentAwardYear;

            private StudentAwardYearsController StudentAwardYearsController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                studentAwardYearServiceMock = new Mock<IStudentAwardYearService>();

                studentId = "0003914";
                awardYear = "2014";

                expectedStudentAwardYear = new StudentAwardYear2() { Code = "2014" };

                studentAwardYearServiceMock.Setup<Task<StudentAwardYear2>>(y => y.GetStudentAwardYear2Async(studentId, awardYear, It.IsAny<bool>())).ReturnsAsync(expectedStudentAwardYear);
                StudentAwardYearsController = new StudentAwardYearsController(adapterRegistryMock.Object, studentAwardYearServiceMock.Object, loggerMock.Object);
                actualStudentAwardYear = await StudentAwardYearsController.GetStudentAwardYear2Async(studentId, awardYear);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                studentAwardYearServiceMock = null;
                studentId = null;
                expectedStudentAwardYear = null;
                actualStudentAwardYear = null;
                StudentAwardYearsController = null;
            }

            [TestMethod]
            public void StudentAwardYearDtoNotNullTest()
            {
                Assert.IsNotNull(actualStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequired_BadRequestResponseTest()
            {
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYear2Async(null, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AwardYearRequired_BadRequestResponseTest()
            {
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYear2Async(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionThrowsForbiddenResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.GetStudentAwardYear2Async(studentId, awardYear, It.IsAny<bool>())).Throws(new PermissionsException("PermissionsException Message"));
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYear2Async(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionThrowsNotFoundResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.GetStudentAwardYear2Async(studentId, awardYear, It.IsAny<bool>())).Throws(new KeyNotFoundException("KeyNotFoundException Message"));
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYear2Async(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InvalidOperationExceptionThrowsBadRequestResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.GetStudentAwardYear2Async(studentId, awardYear, It.IsAny<bool>())).Throws(new InvalidOperationException("InvalidOperationException Message"));
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYear2Async(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UnknownExceptionThrowsBadRequestResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.GetStudentAwardYear2Async(studentId, awardYear, It.IsAny<bool>())).Throws(new Exception("GenericException Message"));
                try
                {
                    await StudentAwardYearsController.GetStudentAwardYear2Async(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class UpdatePaperCopyOptionFlagAsyncTests
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
            private Mock<IStudentAwardYearService> studentAwardYearServiceMock;

            private string studentId;
            private string awardYear;

            private StudentAwardYear2 expectedStudentAwardYear;
            private StudentAwardYear2 actualStudentAwardYear;
            private StudentAwardYear2 inputStudentAwardYear;

            private StudentAwardYearsController StudentAwardYearsController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                studentAwardYearServiceMock = new Mock<IStudentAwardYearService>();

                studentId = "0003914";
                awardYear = "2014";

                expectedStudentAwardYear = new StudentAwardYear2() { StudentId = studentId, Code = awardYear, IsPaperCopyOptionSelected = true };
                inputStudentAwardYear = new StudentAwardYear2() { StudentId = studentId, Code = awardYear, IsPaperCopyOptionSelected = true };

                studentAwardYearServiceMock.Setup<Task<StudentAwardYear2>>(y => y.UpdateStudentAwardYear2Async(inputStudentAwardYear)).ReturnsAsync(inputStudentAwardYear);
                StudentAwardYearsController = new StudentAwardYearsController(adapterRegistryMock.Object, studentAwardYearServiceMock.Object, loggerMock.Object);
                actualStudentAwardYear = await StudentAwardYearsController.UpdateStudentAwardYear2Async(studentId, inputStudentAwardYear);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                studentAwardYearServiceMock = null;
                studentId = null;
                expectedStudentAwardYear = null;
                actualStudentAwardYear = null;
                inputStudentAwardYear = null;
                StudentAwardYearsController = null;
            }

            /// <summary>
            /// Tests if the types of the expected and actual student award year
            /// objects match
            /// </summary>
            [TestMethod]
            public void StudentAwardYearTypeTest()
            {
                Assert.AreEqual(expectedStudentAwardYear.GetType(), actualStudentAwardYear.GetType());
            }

            /// <summary>
            /// Tests if the attributes of the expected and actual student award year objects match
            /// </summary>
            [TestMethod]
            public void EqualAttributesTest()
            {
                Assert.AreEqual(expectedStudentAwardYear.Code, actualStudentAwardYear.Code);
                Assert.AreEqual(expectedStudentAwardYear.StudentId, actualStudentAwardYear.StudentId);
                Assert.AreEqual(expectedStudentAwardYear.IsPaperCopyOptionSelected, actualStudentAwardYear.IsPaperCopyOptionSelected);
            }

            /// <summary>
            /// Tests if exception is thrown when studentId argument is null 
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequired_ExceptionThrownTest()
            {
                await StudentAwardYearsController.UpdateStudentAwardYear2Async(null, inputStudentAwardYear);
            }

            /// <summary>
            /// Tests if exception is thrown if studentAwardYear argument is null
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAwardYearRequired_ExceptionThrownTest()
            {
                await StudentAwardYearsController.UpdateStudentAwardYear2Async(studentId, null);
            }

            /// <summary>
            /// Tests if ArgumentNullException is caught and another is thrown
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentNullExceptionThrowsBadRequestResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.UpdateStudentAwardYear2Async(inputStudentAwardYear)).Throws(new ArgumentNullException("ArgumentNullException Message"));
                try
                {
                    await StudentAwardYearsController.UpdateStudentAwardYear2Async(studentId, inputStudentAwardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), It.IsAny<string>()));
                    throw;
                }
            }

            /// <summary>
            /// Tests if ArgumentException is caught and another is thrown
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentExceptionThrowsBadRequestResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.UpdateStudentAwardYear2Async(inputStudentAwardYear)).Throws(new ArgumentException("ArgumentException Message"));
                try
                {
                    await StudentAwardYearsController.UpdateStudentAwardYear2Async(studentId, inputStudentAwardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
                    throw;
                }
            }

            /// <summary>
            /// Tests if PermissionsException is caught and another is thrown
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionThrowsBadRequestResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.UpdateStudentAwardYear2Async(inputStudentAwardYear)).Throws(new PermissionsException("PermissionsException Message"));
                try
                {
                    await StudentAwardYearsController.UpdateStudentAwardYear2Async(studentId, inputStudentAwardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    throw;
                }
            }

            /// <summary>
            /// Tests if OperationCancelledException is caught and another is thrown
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OperationCancelledExceptionThrowsBadRequestResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.UpdateStudentAwardYear2Async(inputStudentAwardYear)).Throws(new OperationCanceledException("OperationCanceledException Message"));
                try
                {
                    await StudentAwardYearsController.UpdateStudentAwardYear2Async(studentId, inputStudentAwardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Conflict, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<OperationCanceledException>(), It.IsAny<string>()));
                    throw;
                }
            }

            /// <summary>
            /// Tests if exception is caught and another is thrown on a generic exception
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UnknownExceptionThrowsBadRequestResponseTest()
            {
                studentAwardYearServiceMock.Setup(s => s.UpdateStudentAwardYear2Async(inputStudentAwardYear)).Throws(new Exception("GenericException Message"));
                try
                {
                    await StudentAwardYearsController.UpdateStudentAwardYear2Async(studentId, inputStudentAwardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }

        }
    }
}
