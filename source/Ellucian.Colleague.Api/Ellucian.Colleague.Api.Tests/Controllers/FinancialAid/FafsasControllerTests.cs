/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
    public class FafsasControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IFafsaService> fafsaServiceMock;

        public FafsaController fafsaController;

        public FunctionEqualityComparer<Fafsa> fafsaDtoComparer;

        public void FafsasControllerTestsInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            fafsaServiceMock = new Mock<IFafsaService>();

            fafsaDtoComparer = new FunctionEqualityComparer<Fafsa>(
                (f1, f2) => (f1.Id == f2.Id && f1.StudentId == f2.StudentId && f1.AwardYear == f2.AwardYear),
                (f) => (f.Id.GetHashCode()));
        }

        [TestClass]
        public class GetStudentFafsasTests : FafsasControllerTests
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

            public List<Fafsa> fafsaDtos;

            public string studentId;

            [TestInitialize]
            public void Initialize()
            {
                FafsasControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0003914";

                fafsaDtos = new List<Fafsa>()
                {
                    new Fafsa()
                    {
                        Id = "54321",
                        StudentId = studentId,
                        AwardYear = "2014",
                        IsFederallyFlagged = true
                    },
                    new Fafsa()
                    {
                        Id = "54322",
                        StudentId = studentId,
                        AwardYear = "2014",
                        IsFederallyFlagged = false
                    }
                };

                fafsaServiceMock.Setup(s => s.GetStudentFafsasAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(fafsaDtos);

                fafsaController = new FafsaController(adapterRegistryMock.Object, fafsaServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actualFafsas = await fafsaController.GetStudentFafsasAsync(studentId);
                CollectionAssert.AreEqual(fafsaDtos, actualFafsas.ToList(), fafsaDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdArgumentIsRequiredTest()
            {
                try
                {
                    await fafsaController.GetStudentFafsasAsync(null);
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
                fafsaServiceMock.Setup(s => s.GetStudentFafsasAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await fafsaController.GetStudentFafsasAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>(), It.IsAny<object[]>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionTest()
            {
                fafsaServiceMock.Setup(s => s.GetStudentFafsasAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await fafsaController.GetStudentFafsasAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
                    throw;
                }
            }
        }
    }
}
