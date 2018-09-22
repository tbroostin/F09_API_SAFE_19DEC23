/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class ProfileApplicationsControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IProfileApplicationService> profileApplicationServiceMock;

        public ProfileApplicationsController profileApplicationsController;

        public FunctionEqualityComparer<ProfileApplication> profileApplicationDtoComparer;

        public void ProfileApplicationControllerTestsInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            profileApplicationServiceMock = new Mock<IProfileApplicationService>();

            profileApplicationDtoComparer = new FunctionEqualityComparer<ProfileApplication>(
                (p1, p2) => (p1.Id == p2.Id),
                (p) => p.Id.GetHashCode());
        }

        [TestClass]
        public class GetProfileApplicationsTests : ProfileApplicationsControllerTests
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

            public List<ProfileApplication> expectedDtos;

            public IEnumerable<ProfileApplication> actualDtos;
            
            public string studentId;

            [TestInitialize]
            public void Initialize()
            {
                ProfileApplicationControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0003914";

                expectedDtos = new List<ProfileApplication>()
                {
                    new ProfileApplication()
                    {
                        Id = "51234",
                        StudentId = studentId,
                        AwardYear = "2014",
                        IsFederallyFlagged = false,
                        IsInstitutionallyFlagged = true
                    },
                    new ProfileApplication()
                    {
                        Id = "12345",
                        StudentId = studentId,
                        AwardYear = "2014",
                        IsFederallyFlagged = false,
                        IsInstitutionallyFlagged = false
                    }
                };

                profileApplicationServiceMock.Setup(s => s.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, b) => Task.FromResult(expectedDtos.Where(p => p.StudentId == id)));

                profileApplicationsController = new ProfileApplicationsController(adapterRegistryMock.Object, profileApplicationServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualDtos = await profileApplicationsController.GetProfileApplicationsAsync(studentId);
                CollectionAssert.AreEqual(expectedDtos, actualDtos.ToList(), profileApplicationDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdArgumentIsRequiredTest()
            {
                try
                {
                    await profileApplicationsController.GetProfileApplicationsAsync(null);
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
                profileApplicationServiceMock.Setup(s => s.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await profileApplicationsController.GetProfileApplicationsAsync(studentId);
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
            public async Task GenericExceptionTest()
            {
                profileApplicationServiceMock.Setup(s => s.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await profileApplicationsController.GetProfileApplicationsAsync(studentId);
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
