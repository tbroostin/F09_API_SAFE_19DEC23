/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class StudentLoanLimitationsControllerTests
    {
        [TestClass]
        public class GetStudentLoanLimitationsTests
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
            private Mock<IStudentLoanLimitationService> StudentLoanLimitationServiceMock;

            private string studentId;

            private IEnumerable<StudentLoanLimitation> expectedStudentLoanLimitations;
            private List<StudentLoanLimitation> testStudentLoanLimitations;
            private IEnumerable<StudentLoanLimitation> actualStudentLoanLimitations;

            private StudentLoanLimitationsController StudentLoanLimitationsController;


            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                StudentLoanLimitationServiceMock = new Mock<IStudentLoanLimitationService>();

                studentId = "0003914";
                expectedStudentLoanLimitations = new List<StudentLoanLimitation>()
                {
                    new StudentLoanLimitation()
                    {
                        AwardYear = "2014",
                        StudentId = studentId,
                        SubsidizedMaximumAmount = 10000,
                        UnsubsidizedMaximumAmount = 12000

                    },
                    new StudentLoanLimitation()
                    {
                        AwardYear = "2013",
                        StudentId = studentId,
                        SubsidizedMaximumAmount = 20000,
                        UnsubsidizedMaximumAmount = 15555
                    }
                };

                testStudentLoanLimitations = new List<StudentLoanLimitation>();
                foreach (var letter in expectedStudentLoanLimitations)
                {
                    var testLetter = new StudentLoanLimitation();
                    foreach (var property in typeof(StudentLoanLimitation).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        property.SetValue(testLetter, property.GetValue(letter, null), null);
                    }
                    testStudentLoanLimitations.Add(testLetter);
                }
                StudentLoanLimitationServiceMock.Setup<Task<IEnumerable<StudentLoanLimitation>>>(l => l.GetStudentLoanLimitationsAsync(studentId)).ReturnsAsync(testStudentLoanLimitations);

                StudentLoanLimitationsController = new StudentLoanLimitationsController(adapterRegistryMock.Object, StudentLoanLimitationServiceMock.Object, loggerMock.Object);

                actualStudentLoanLimitations = await StudentLoanLimitationsController.GetStudentLoanLimitationsAsync(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                StudentLoanLimitationServiceMock = null;
                studentId = null;
                expectedStudentLoanLimitations = null;
                testStudentLoanLimitations = null;
                actualStudentLoanLimitations = null;
                StudentLoanLimitationsController = null;
            }

            [TestMethod]
            public void StudentLoanLimitationTypeTest()
            {
                Assert.AreEqual(expectedStudentLoanLimitations.GetType(), actualStudentLoanLimitations.GetType());
                foreach (var actualLetter in actualStudentLoanLimitations)
                {
                    Assert.AreEqual(typeof(StudentLoanLimitation), actualLetter.GetType());
                }
            }

            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var StudentLoanLimitationProperties = typeof(StudentLoanLimitation).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(5, StudentLoanLimitationProperties.Length);
            }

            [TestMethod]
            public void ProperitesAreEqualTest()
            {
                var StudentLoanLimitationProperties = typeof(StudentLoanLimitation).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var expectedLetter in expectedStudentLoanLimitations)
                {
                    var actualLetter = actualStudentLoanLimitations.First(a => a.AwardYear == expectedLetter.AwardYear && a.StudentId == expectedLetter.StudentId);
                    foreach (var property in StudentLoanLimitationProperties)
                    {
                        var expectedPropertyValue = property.GetValue(expectedLetter, null);
                        var actualPropertyValue = property.GetValue(actualLetter, null);
                        Assert.AreEqual(expectedPropertyValue, actualPropertyValue);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                await StudentLoanLimitationsController.GetStudentLoanLimitationsAsync(null);
            }
        }
    }
}
