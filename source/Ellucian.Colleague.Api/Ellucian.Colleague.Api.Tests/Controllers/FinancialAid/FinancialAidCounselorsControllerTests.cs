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

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class FinancialAidCounselorsControllerTests
    {
        [TestClass]
        public class GetSingleCounselorTests
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
            private Mock<IFinancialAidCounselorService> financialAidCounselorServiceMock;

            private string counselorId;

            private FinancialAidCounselor expectedCounselor;
            private FinancialAidCounselor actualCounselor;

            private FinancialAidCounselorsController financialAidCounselorsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                financialAidCounselorServiceMock = new Mock<IFinancialAidCounselorService>();

                counselorId = "0010743";

                expectedCounselor = new FinancialAidCounselor()
                {
                    Id = counselorId,
                    Name = "Matt Counselor",
                    EmailAddress = "email.address@ellucian.edu"
                };

                financialAidCounselorServiceMock.Setup(s => s.GetCounselor(counselorId)).Returns(expectedCounselor);
                financialAidCounselorsController = new FinancialAidCounselorsController(adapterRegistryMock.Object, financialAidCounselorServiceMock.Object, loggerMock.Object);
                actualCounselor = financialAidCounselorsController.GetCounselor(counselorId);
            }

            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var counselorProperties = typeof(FinancialAidCounselor).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(3, counselorProperties.Count());
            }

            [TestMethod]
            public void ObjectNotNullTest()
            {
                Assert.IsNotNull(actualCounselor);
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                Assert.AreEqual(expectedCounselor.Id, actualCounselor.Id);
                Assert.AreEqual(expectedCounselor.Name, actualCounselor.Name);
                Assert.AreEqual(expectedCounselor.EmailAddress, actualCounselor.EmailAddress);
            }

            [TestMethod]
            public void CounselorIdRequired_ThrowsBadRequestException()
            {
                var exceptionCaught = false;
                try
                {
                    financialAidCounselorsController.GetCounselor(string.Empty);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public void CatchPermissionsExceptionAndLogErrorMessageTest()
            {
                financialAidCounselorServiceMock.Setup(s => s.GetCounselor(counselorId)).Throws(new PermissionsException("pex"));

                var exceptionCaught = false;
                try
                {
                    financialAidCounselorsController.GetCounselor(counselorId);
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
            public void CatchKeyNotFoundExceptionAndLogErrorMessageTest()
            {
                financialAidCounselorServiceMock.Setup(s => s.GetCounselor(counselorId)).Throws(new KeyNotFoundException("knfe"));

                var exceptionCaught = false;
                try
                {
                    financialAidCounselorsController.GetCounselor(counselorId);
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
            public void CatchApplicationExceptionAndLogErrorMessageTest()
            {
                financialAidCounselorServiceMock.Setup(s => s.GetCounselor(counselorId)).Throws(new ApplicationException("ae"));

                var exceptionCaught = false;
                try
                {
                    financialAidCounselorsController.GetCounselor(counselorId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<ApplicationException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public void CatchUnkownExceptionAndLogErrorMessageTest()
            {
                financialAidCounselorServiceMock.Setup(s => s.GetCounselor(counselorId)).Throws(new Exception("e"));

                var exceptionCaught = false;
                try
                {
                    financialAidCounselorsController.GetCounselor(counselorId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                financialAidCounselorServiceMock = null;
                counselorId = null;
                expectedCounselor = null;
                actualCounselor = null;
                financialAidCounselorsController = null;
            }
        }

        [TestClass]
        public class GetMultipleCounselorsTests{
            
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
            private Mock<IFinancialAidCounselorService> financialAidCounselorServiceMock;

            private List<string> counselorIds;

            private IEnumerable<FinancialAidCounselor> expectedCounselors;
            private IEnumerable<FinancialAidCounselor> actualCounselors;

            private FinancialAidCounselorsController financialAidCounselorsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                financialAidCounselorServiceMock = new Mock<IFinancialAidCounselorService>();

                counselorIds = new List<string>() { "0005135", "0000862", "0010414", "0000222" };

                expectedCounselors = new List<FinancialAidCounselor>()
                {
                    new FinancialAidCounselor(){
                        Id = "0005135",
                        Name = "John Doe",
                        EmailAddress = "john.doe@ellucian.edu"
                    },
                    new FinancialAidCounselor(){
                        Id = "0000862",
                        Name = "Jane Doe",
                        EmailAddress = "jane.doe@ellucian.edu"
                    },
                    new FinancialAidCounselor(){
                        Id = "0010414",
                        Name = "Emily Thorne",
                        EmailAddress = "emily.thorne@ellucian.edu"
                    },
                    new FinancialAidCounselor(){
                        Id = "0000222",
                        Name = "Daniel Grayson",
                        EmailAddress = "daniel.grayson@ellucian.edu"
                    }
                };

                financialAidCounselorServiceMock.Setup(s => s.GetCounselorsByIdAsync(counselorIds)).Returns(Task.FromResult(expectedCounselors));
                financialAidCounselorsController = new FinancialAidCounselorsController(adapterRegistryMock.Object, financialAidCounselorServiceMock.Object, loggerMock.Object);
                
            }

            [TestCleanup]
            public void Cleanup()
            {
                financialAidCounselorsController = null;
                adapterRegistryMock = null;
                loggerMock = null;
                financialAidCounselorServiceMock = null;
                counselorIds = null;
                expectedCounselors = null;
                actualCounselors = null;

            }

            /// <summary>
            /// Tests if actual counselors received from service are the same as expected
            /// </summary>
            /// <returns>Task</returns>
            [TestMethod]
            public async Task GetFinancialAidCounselorsTest()
            {
                actualCounselors = await financialAidCounselorsController.QueryFinancialAidCounselorsAsync(new FinancialAidCounselorQueryCriteria() { FinancialAidCounselorIds = counselorIds });
                Assert.AreEqual(expectedCounselors.Count(), actualCounselors.Count());
                foreach (var expectedCounselor in expectedCounselors)
                {
                    var actualCounselor = actualCounselors.FirstOrDefault(ac => ac.Id == expectedCounselor.Id);
                    Assert.IsNotNull(actualCounselor);
                    Assert.AreEqual(expectedCounselor.Name, actualCounselor.Name);
                    Assert.AreEqual(expectedCounselor.EmailAddress, actualCounselor.EmailAddress);
                }
            }

            /// <summary>
            /// Checks if Permission exception is caught by the method
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CatchPermissionExceptionTest()
            {
                financialAidCounselorServiceMock.Setup(s => s.GetCounselorsByIdAsync(counselorIds)).Throws(new PermissionsException("pex"));

                var exceptionCaught = false;
                try
                {
                    await financialAidCounselorsController.QueryFinancialAidCounselorsAsync(new FinancialAidCounselorQueryCriteria() { FinancialAidCounselorIds = counselorIds });
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
            }

            /// <summary>
            /// Tests if Unknow exception is caught by the method
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CatchUnknowExceptionTest()
            {
                financialAidCounselorServiceMock.Setup(s => s.GetCounselorsByIdAsync(counselorIds)).Throws(new Exception("e"));

                var exceptionCaught = false;
                try
                {
                    await financialAidCounselorsController.QueryFinancialAidCounselorsAsync(new FinancialAidCounselorQueryCriteria() { FinancialAidCounselorIds = counselorIds });
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

