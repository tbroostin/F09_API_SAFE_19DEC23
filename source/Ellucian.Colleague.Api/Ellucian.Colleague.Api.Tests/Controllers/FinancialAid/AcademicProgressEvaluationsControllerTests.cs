/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class AcademicProgressEvaluationsControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IAcademicProgressService> academicProgressEvaluationServiceMock;

        public AcademicProgressEvaluationsController actualController;

        public FunctionEqualityComparer<AcademicProgressEvaluation> academicProgressEvaluationDtoComparer;

        public void AcademicProgressEvaluationsControllerTestsInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            academicProgressEvaluationServiceMock = new Mock<IAcademicProgressService>();

            academicProgressEvaluationDtoComparer = new FunctionEqualityComparer<AcademicProgressEvaluation>(
                (sap1, sap2) => sap1.Id == sap2.Id,
                (sap) => sap.Id.GetHashCode());
        }

        [TestClass]
        public class GetStudentAcademicProgressEvaluationsTests : AcademicProgressEvaluationsControllerTests
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

            public List<AcademicProgressEvaluation> academicProgressEvaluationDtos;
            public string studentId;

            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationsControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0003914";

                academicProgressEvaluationDtos = new List<AcademicProgressEvaluation>()
                {
                    new AcademicProgressEvaluation()
                    {
                        Id = "1700"
                    },
                    new AcademicProgressEvaluation()
                    {
                        Id = "12083"
                    }
                };

                academicProgressEvaluationServiceMock.Setup(s => s.GetAcademicProgressEvaluationsAsync(It.IsAny<string>()))
                    .ReturnsAsync(academicProgressEvaluationDtos.AsEnumerable());

                actualController = new AcademicProgressEvaluationsController(academicProgressEvaluationServiceMock.Object, adapterRegistryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actualEvals = await actualController.GetStudentAcademicProgressEvaluationsAsync(studentId);
                CollectionAssert.AreEqual(academicProgressEvaluationDtos, actualEvals.ToList(), academicProgressEvaluationDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                try
                {
                    await actualController.GetStudentAcademicProgressEvaluationsAsync(null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchPermissionsExceptionTest()
            {
                academicProgressEvaluationServiceMock.Setup(s => s.GetAcademicProgressEvaluationsAsync(It.IsAny<string>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await actualController.GetStudentAcademicProgressEvaluationsAsync(studentId);
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
            public async Task CatchGenericExceptionTest()
            {
                academicProgressEvaluationServiceMock.Setup(s => s.GetAcademicProgressEvaluationsAsync(It.IsAny<string>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await actualController.GetStudentAcademicProgressEvaluationsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetStudentAcademicProgressEvaluations2Tests : AcademicProgressEvaluationsControllerTests
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

            public List<AcademicProgressEvaluation2> academicProgressEvaluationDtos;
            public string studentId;

            public FunctionEqualityComparer<AcademicProgressEvaluation2> academicProgressEvaluation2DtoComparer
            {
                get
                {
                    return new FunctionEqualityComparer<AcademicProgressEvaluation2>(
                        (sap1, sap2) => sap1.Id == sap2.Id,
                        (sap) => sap.Id.GetHashCode());
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressEvaluationsControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0003914";

                academicProgressEvaluationDtos = new List<AcademicProgressEvaluation2>()
                {
                    new AcademicProgressEvaluation2()
                    {
                        Id = "1700"
                    },
                    new AcademicProgressEvaluation2()
                    {
                        Id = "12083"
                    }
                };

                academicProgressEvaluationServiceMock.Setup(s => s.GetAcademicProgressEvaluations2Async(It.IsAny<string>()))
                    .ReturnsAsync(academicProgressEvaluationDtos.AsEnumerable());

                actualController = new AcademicProgressEvaluationsController(academicProgressEvaluationServiceMock.Object, adapterRegistryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actualEvals = await actualController.GetStudentAcademicProgressEvaluations2Async(studentId);
                CollectionAssert.AreEqual(academicProgressEvaluationDtos, actualEvals.ToList(), academicProgressEvaluation2DtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                try
                {
                    await actualController.GetStudentAcademicProgressEvaluations2Async(null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchPermissionsExceptionTest()
            {
                academicProgressEvaluationServiceMock.Setup(s => s.GetAcademicProgressEvaluations2Async(It.IsAny<string>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await actualController.GetStudentAcademicProgressEvaluations2Async(studentId);
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
            public async Task CatchGenericExceptionTest()
            {
                academicProgressEvaluationServiceMock.Setup(s => s.GetAcademicProgressEvaluations2Async(It.IsAny<string>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await actualController.GetStudentAcademicProgressEvaluations2Async(studentId);
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
