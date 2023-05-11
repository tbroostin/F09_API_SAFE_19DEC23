// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class ApplicantsControllerTests
    {
        [TestClass]
        public class GetApplicantTests
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
            private Mock<IApplicantService> applicantServiceMock;

            private string applicantId;

            private Applicant expectedApplicant;
            private Applicant testApplicant;
            private Applicant actualApplicant;

            private ApplicantsController applicantsController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                applicantServiceMock = new Mock<IApplicantService>();

                applicantId = "0003914";

                expectedApplicant = new Applicant()
                {
                    Id = "0003914",
                    FirstName = "Matt",
                    MiddleName = "C",
                    LastName = "DeDiana",
                    PreferredName = "Matt",
                    PreferredAddress = new List<string>() { "1111 One Street", "Washington DC, 22222"}
                };

                testApplicant = new Applicant();
                foreach (var property in typeof(Applicant).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    property.SetValue(testApplicant, property.GetValue(expectedApplicant, null), null);
                }
                applicantServiceMock.Setup<Task<Applicant>>(s => s.GetApplicantAsync(applicantId)).Returns(Task.FromResult(testApplicant));

                applicantsController = new ApplicantsController(adapterRegistryMock.Object, applicantServiceMock.Object, loggerMock.Object);
                actualApplicant = await applicantsController.GetApplicantAsync(applicantId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                applicantServiceMock = null;
                applicantId = null;
                expectedApplicant = null;
                testApplicant = null;
                actualApplicant = null;
                applicantsController = null;
            }

            [TestMethod]
            public void ApplicantsTypeTest()
            {
                Assert.AreEqual(typeof(Applicant), actualApplicant.GetType());
                Assert.AreEqual(expectedApplicant.GetType(), actualApplicant.GetType());
            }

            [TestMethod]
            public void NumberOfKnownPropertiesTest()
            {
                var applicantProperties = typeof(Applicant).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(23, applicantProperties.Length);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                await applicantsController.GetApplicantAsync(null);
            }

            [TestMethod]
            public async Task StudentIdRequired_BadRequestResponseTest()
            {
                bool exceptionCaught = false;
                try
                {
                   await applicantsController.GetApplicantAsync(null);
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
                applicantServiceMock.Setup(s => s.GetApplicantAsync(applicantId)).Throws(new PermissionsException("Permissions Exception"));

                bool exceptionCaught = false;
                try
                {
                    await applicantsController.GetApplicantAsync(applicantId);
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
                applicantServiceMock.Setup(s => s.GetApplicantAsync(applicantId)).Throws(new KeyNotFoundException("Not Found Exception"));

                bool exceptionCaught = false;
                try
                {
                    await applicantsController.GetApplicantAsync(applicantId);
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
                applicantServiceMock.Setup(s => s.GetApplicantAsync(applicantId)).Throws(new Exception("Unkown Exception"));

                bool exceptionCaught = false;
                try
                {
                    await applicantsController.GetApplicantAsync(applicantId);
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
