/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// <summary>
    /// Test Class for the StudentDefaultAwardPeriodController
    /// </summary>
    [TestClass]
    public class StudentDefaultAwardPeriodControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IStudentDefaultAwardPeriodService> studentDefaultAwardPeriodServiceMock;

        public StudentDefaultAwardPeriodController StudentDefaultAwardPeriodController;

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

        private string studentId;

        private List<StudentDefaultAwardPeriod> expectedStudentDefaultAwardPeriod;
        private List<StudentDefaultAwardPeriod> testStudentDefaultAwardPeriod;
        private IEnumerable<StudentDefaultAwardPeriod> actualStudentDefaultAwardPeriod;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            studentDefaultAwardPeriodServiceMock = new Mock<IStudentDefaultAwardPeriodService>();

            studentId = "0003915";

            expectedStudentDefaultAwardPeriod = new List<StudentDefaultAwardPeriod>()
        {
            
            new StudentDefaultAwardPeriod()
            {
                StudentId = "0003915",
                AwardYear = "2015",
                DefaultAwardPeriods = new List<string> {"15/FA","16/WI"}
            },

            new StudentDefaultAwardPeriod()
            {
                StudentId = "0003915",
                AwardYear = "2014",
                DefaultAwardPeriods = new List<string> {"14/FA","15/WI"}
            }
        };

            testStudentDefaultAwardPeriod = new List<StudentDefaultAwardPeriod>();
            foreach (var expectedDefaultAwardPeriod in expectedStudentDefaultAwardPeriod)
            {
                var testAwardPeriod = new StudentDefaultAwardPeriod();
                foreach (var property in typeof(StudentDefaultAwardPeriod).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    property.SetValue(testAwardPeriod, property.GetValue(expectedDefaultAwardPeriod, null), null);
                }
                testStudentDefaultAwardPeriod.Add(testAwardPeriod);
            }
            studentDefaultAwardPeriodServiceMock.Setup(s => s.GetStudentDefaultAwardPeriodsAsync(studentId)).Returns(Task.FromResult(testStudentDefaultAwardPeriod.AsEnumerable()));

            StudentDefaultAwardPeriodController = new StudentDefaultAwardPeriodController(adapterRegistryMock.Object, studentDefaultAwardPeriodServiceMock.Object, loggerMock.Object);
            actualStudentDefaultAwardPeriod = await StudentDefaultAwardPeriodController.GetStudentDefaultAwardPeriodsAsync(studentId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            studentDefaultAwardPeriodServiceMock = null;
            expectedStudentDefaultAwardPeriod = null;
            testStudentDefaultAwardPeriod = null;
            actualStudentDefaultAwardPeriod = null;
            StudentDefaultAwardPeriodController = null;
        }

        [TestMethod]
        public void StudentDefaultAwardPeriodTypeTest()
        {
            Assert.AreEqual(typeof(List<StudentDefaultAwardPeriod>), actualStudentDefaultAwardPeriod.GetType());
            Assert.AreEqual(expectedStudentDefaultAwardPeriod.GetType(), actualStudentDefaultAwardPeriod.GetType());
        }

        [TestMethod]
        public void NumberOfKnownPropertiesTest()
        {
            var studentDefaultAwardPeriodProperties = typeof(StudentDefaultAwardPeriod).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(3, studentDefaultAwardPeriodProperties.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentIdRequiredTest()
        {
            await StudentDefaultAwardPeriodController.GetStudentDefaultAwardPeriodsAsync(null);
        }

        [TestMethod]
        public async Task StudentIdRequired_BadRequestResponseTest()
        {
            bool exceptionCaught = false;
            try
            {
                await StudentDefaultAwardPeriodController.GetStudentDefaultAwardPeriodsAsync(null);
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
            studentDefaultAwardPeriodServiceMock.Setup(s => s.GetStudentDefaultAwardPeriodsAsync(studentId)).Throws(new PermissionsException("Permissions Exception"));

            bool exceptionCaught = false;
            try
            {
                await StudentDefaultAwardPeriodController.GetStudentDefaultAwardPeriodsAsync(studentId);
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
            studentDefaultAwardPeriodServiceMock.Setup(s => s.GetStudentDefaultAwardPeriodsAsync(studentId)).Throws(new KeyNotFoundException("Not Found Exception"));

            bool exceptionCaught = false;
            try
            {
                await StudentDefaultAwardPeriodController.GetStudentDefaultAwardPeriodsAsync(studentId);
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
            studentDefaultAwardPeriodServiceMock.Setup(s => s.GetStudentDefaultAwardPeriodsAsync(studentId)).Throws(new Exception("Unkown Exception"));

            bool exceptionCaught = false;
            try
            {
                await StudentDefaultAwardPeriodController.GetStudentDefaultAwardPeriodsAsync(studentId);
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
