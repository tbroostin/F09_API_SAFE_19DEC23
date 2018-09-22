//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Adapters;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using System.Reflection;
using System.Web.Http;
using Ellucian.Web.Security;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class AverageAwardPackageControllerTests
    {
        [TestClass]
        public class GetAverageAwardPackageTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAverageAwardPackageService> averageAwardPackageServiceMock;

            private string studentId;

            private List<AverageAwardPackage> expectedAverageAwardPackages;
            private List<AverageAwardPackage> testAverageAwardPackages;
            private List<AverageAwardPackage> actualAverageAwardPackages;

            private AverageAwardPackagesController AverageAwardPackageController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                averageAwardPackageServiceMock = new Mock<IAverageAwardPackageService>();

                studentId = "0003915";

                expectedAverageAwardPackages = new List<AverageAwardPackage>()
                {
                    new AverageAwardPackage()
                    {
                        AverageGrantAmount = 7500,
                        AverageLoanAmount = 10000,
                        AverageScholarshipAmount = 2500,
                        AwardYearCode = "2015"
                    },

                    new AverageAwardPackage()
                    {
                        AverageGrantAmount = 5000,
                        AverageLoanAmount = 1000,
                        AverageScholarshipAmount = 3400,
                        AwardYearCode = "2014"
                    }
                };

                testAverageAwardPackages = new List<AverageAwardPackage>();
                foreach (var expectedAverageAwardPackage in expectedAverageAwardPackages)
                {
                    var testAverageAwardPackage = new AverageAwardPackage();
                    foreach (var property in typeof(AverageAwardPackage).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        property.SetValue(testAverageAwardPackage, property.GetValue(expectedAverageAwardPackage, null), null);
                    }
                    testAverageAwardPackages.Add(testAverageAwardPackage);
                }
                averageAwardPackageServiceMock.Setup<Task<IEnumerable<AverageAwardPackage>>>(s => s.GetAverageAwardPackagesAsync(studentId)).ReturnsAsync(testAverageAwardPackages);

                AverageAwardPackageController = new AverageAwardPackagesController(adapterRegistryMock.Object, averageAwardPackageServiceMock.Object, loggerMock.Object);
                actualAverageAwardPackages = (await AverageAwardPackageController.GetAverageAwardPackagesAsync(studentId)).ToList();
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                averageAwardPackageServiceMock = null;
                expectedAverageAwardPackages = null;
                testAverageAwardPackages = null;
                actualAverageAwardPackages = null;
                AverageAwardPackageController = null;
            }

            [TestMethod]
            public void AverageAwardPackageTypeTest()
            {
                Assert.AreEqual(typeof(List<AverageAwardPackage>), actualAverageAwardPackages.GetType());
                Assert.AreEqual(expectedAverageAwardPackages.GetType(), actualAverageAwardPackages.GetType());
            }

            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var averageAwardPackageProperties = typeof(AverageAwardPackage).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(4, averageAwardPackageProperties.Length);
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                var averageAwardPackageProperties = typeof(AverageAwardPackage).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var expectedAverageAwardPackage in expectedAverageAwardPackages)
                {
                    var actualAverageAwardPackage = actualAverageAwardPackages.FirstOrDefault(aap => aap.AwardYearCode == expectedAverageAwardPackage.AwardYearCode);

                    foreach (var property in averageAwardPackageProperties)
                    {
                        var expectedPropertyValue = property.GetValue(expectedAverageAwardPackage, null);
                        var actualPropertyValue = property.GetValue(actualAverageAwardPackage, null);
                        Assert.AreEqual(expectedPropertyValue, actualPropertyValue);
                    }
                }

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                await AverageAwardPackageController.GetAverageAwardPackagesAsync(null);
            }

            [TestMethod]
            public async Task StudentIdRequired_BadRequestResponseTest()
            {
                bool exceptionCaught = false;
                try
                {
                    await AverageAwardPackageController.GetAverageAwardPackagesAsync(null);
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
                averageAwardPackageServiceMock.Setup(s => s.GetAverageAwardPackagesAsync(studentId)).Throws(new PermissionsException("Permissions Exception"));

                bool exceptionCaught = false;
                try
                {
                    await AverageAwardPackageController.GetAverageAwardPackagesAsync(studentId);
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
            public async Task CatchUnknownExceptionTest()
            {
                averageAwardPackageServiceMock.Setup(s => s.GetAverageAwardPackagesAsync(studentId)).Throws(new Exception("Unkown Exception"));

                bool exceptionCaught = false;
                try
                {
                    await AverageAwardPackageController.GetAverageAwardPackagesAsync(studentId);
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
