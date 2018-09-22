/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.IO;
using System.Web.Http.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class FinancialAidApplicationsControllerTests
    {
        [TestClass]
        public class GetFinancialAidApplicationsControllerTests
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
            private Mock<IFinancialAidApplicationService> financialAidApplicationServiceMock;

            private string studentId;

            private IEnumerable<FinancialAidApplication> expectedFinancialAidApplications;
            private List<FinancialAidApplication> testFinancialAidApplications;
            private IEnumerable<FinancialAidApplication> actualFinancialAidApplications;

            private FinancialAidApplicationsController FinancialAidApplicationsController;

            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                financialAidApplicationServiceMock = new Mock<IFinancialAidApplicationService>();

                studentId = "0003915";
                expectedFinancialAidApplications = new List<FinancialAidApplication>()
                {
                    new FinancialAidApplication()
                    {
                        StudentId = "0003915",
                        AwardYear = "2014",
                        IsFafsaComplete = true,
                        IsProfileComplete = true,
                    },
                    new FinancialAidApplication()
                    {
                        StudentId = "0003915",
                        AwardYear = "2015",
                        IsFafsaComplete = true,
                        IsProfileComplete = false,
                    }
                };

                testFinancialAidApplications = new List<FinancialAidApplication>();
                foreach (var application in expectedFinancialAidApplications)
                {
                    var testApplication = new FinancialAidApplication();
                    foreach (var property in typeof(FinancialAidApplication).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        property.SetValue(testApplication, property.GetValue(application, null), null);
                    }
                    testFinancialAidApplications.Add(testApplication);
                }

                financialAidApplicationServiceMock.Setup(a => a.GetFinancialAidApplications(studentId)).Returns(testFinancialAidApplications);
                FinancialAidApplicationsController = new FinancialAidApplicationsController(adapterRegistryMock.Object, financialAidApplicationServiceMock.Object, loggerMock.Object);
                actualFinancialAidApplications = FinancialAidApplicationsController.GetFinancialAidApplications(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                financialAidApplicationServiceMock = null;
                studentId = null;
                expectedFinancialAidApplications = null;
                testFinancialAidApplications = null;
                actualFinancialAidApplications = null;
                FinancialAidApplicationsController = null;
            }

            [TestMethod]
            public void FinancialAidApplicationTypeTest()
            {
                Assert.AreEqual(expectedFinancialAidApplications.GetType(), actualFinancialAidApplications.GetType());

                foreach (var singleApplication in actualFinancialAidApplications)
                {
                    Assert.AreEqual(typeof(FinancialAidApplication), singleApplication.GetType());
                }
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                var financialAidApplicationsProperties = typeof(FinancialAidApplication).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var singleApplication in expectedFinancialAidApplications)
                {
                    var actualFinancialAidApplication = expectedFinancialAidApplications.First(d => d.AwardYear == singleApplication.AwardYear);
                    foreach (var property in financialAidApplicationsProperties)
                    {
                        var expectedPropertyValue = property.GetValue(singleApplication, null);
                        var actualPropertyValue = property.GetValue(actualFinancialAidApplication, null);
                        Assert.AreEqual(expectedPropertyValue, actualPropertyValue);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void StudentIdRequiredTest()
            {
                FinancialAidApplicationsController.GetFinancialAidApplications(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void CatchPermissionsExceptionTest()
            {
                financialAidApplicationServiceMock.Setup(a => a.GetFinancialAidApplications(studentId)).Throws(new PermissionsException("pex"));
                try
                {
                    FinancialAidApplicationsController.GetFinancialAidApplications(studentId);
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
            public void CatchKeyNotFoundExceptionTest()
            {
                financialAidApplicationServiceMock.Setup(a => a.GetFinancialAidApplications(studentId)).Throws(new KeyNotFoundException("knfe"));
                try
                {
                    FinancialAidApplicationsController.GetFinancialAidApplications(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void CatchGenericExceptionTest()
            {
                financialAidApplicationServiceMock.Setup(a => a.GetFinancialAidApplications(studentId)).Throws(new Exception("knfe"));
                try
                {
                    FinancialAidApplicationsController.GetFinancialAidApplications(studentId);
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
