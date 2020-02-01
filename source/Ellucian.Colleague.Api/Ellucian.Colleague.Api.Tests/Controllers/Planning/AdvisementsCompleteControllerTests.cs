// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Planning;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Planning
{
    [TestClass]
    public class AdvisementsCompleteControllerTests
    {
        [TestClass]
        public class PostCompletedAdvisementAsync : AdvisementsCompleteControllerTests
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

            private AdvisementsCompleteController AdvisementsCompleteController;
            private Mock<IAdvisorService> advisorServiceMock;
            private IAdvisorService advisorService;
            ILogger logger = new Mock<ILogger>().Object;

            private Coordination.Base.PrivacyWrapper<Advisee> wrapper;
            private Advisee advisee;
            private CompletedAdvisement advisementComplete;

            private HttpResponse response;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                advisorServiceMock = new Mock<IAdvisorService>();
                advisorService = advisorServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                advisee = new Advisee()
                {
                    Id = "0001234",
                    PrivacyStatusCode = null                    
                };
                advisementComplete = new CompletedAdvisement()
                {
                    CompletionDate = DateTime.Today,
                    CompletionTime = DateTime.Now,
                    AdvisorId = "0001235"
                };
                wrapper = new Coordination.Base.PrivacyWrapper<Advisee>(advisee, false);
                advisorServiceMock.Setup(x => x.PostCompletedAdvisementAsync(It.IsAny<string>(), It.IsAny<CompletedAdvisement>())).ReturnsAsync(wrapper);
                AdvisementsCompleteController = new AdvisementsCompleteController(advisorService, logger);

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
            }

            [TestCleanup]
            public void Cleanup()
            {
                AdvisementsCompleteController = null;
                advisorService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostCompletedAdvisementAsync_PermissionsException()
            {
                advisorServiceMock.Setup(x => x.PostCompletedAdvisementAsync(It.IsAny<string>(), It.IsAny<CompletedAdvisement>())).ThrowsAsync(new PermissionsException());
                AdvisementsCompleteController = new AdvisementsCompleteController(advisorService, logger);
                var updatedAdvisee = await AdvisementsCompleteController.PostCompletedAdvisementAsync(advisee.Id, advisementComplete);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostCompletedAdvisementAsync_Exception()
            {
                advisorServiceMock.Setup(x => x.PostCompletedAdvisementAsync(It.IsAny<string>(), It.IsAny<CompletedAdvisement>())).ThrowsAsync(new Exception());
                AdvisementsCompleteController = new AdvisementsCompleteController(advisorService, logger);
                var updatedAdvisee = await AdvisementsCompleteController.PostCompletedAdvisementAsync(advisee.Id, advisementComplete);
            }

            [TestMethod]
            public async Task PostCompletedAdvisementAsync_Advisee_Has_Privacy_Restriction()
            {
                wrapper.HasPrivacyRestrictions = true;
                advisorServiceMock.Setup(x => x.PostCompletedAdvisementAsync(It.IsAny<string>(), It.IsAny<CompletedAdvisement>())).ReturnsAsync(wrapper);
                AdvisementsCompleteController = new AdvisementsCompleteController(advisorService, logger);
                var updatedAdvisee = await AdvisementsCompleteController.PostCompletedAdvisementAsync(advisee.Id, advisementComplete);
                Assert.AreEqual(typeof(Advisee), updatedAdvisee.GetType());
            }

            [TestMethod]
            public async Task PostCompletedAdvisementAsync_Advisee_Has_No_Privacy_Restriction()
            {
                var updatedAdvisee = await AdvisementsCompleteController.PostCompletedAdvisementAsync(advisee.Id, advisementComplete);
                Assert.AreEqual(typeof(Advisee), updatedAdvisee.GetType());
            }
        }
    }
}
