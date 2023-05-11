// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class RegistrationReasonsControllerTests
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

        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private RegistrationReasonsController RegistrationReasonsController;
        private IEnumerable<Domain.Student.Entities.RegistrationReason> RegistrationReasons;
        private IEnumerable<Domain.Student.Entities.RegistrationReason> RegistrationReasons2;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationReason, Dtos.Student.RegistrationReason>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.RegistrationReason, Dtos.Student.RegistrationReason>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            RegistrationReasons = BuildRegistrationReasons();
            RegistrationReasons2 = BuildNonCachedRegistrationReasons();
            referenceDataRepositoryMock.Setup(repo => repo.GetRegistrationReasonsAsync(false)).ReturnsAsync(RegistrationReasons);
            referenceDataRepositoryMock.Setup(repo => repo.GetRegistrationReasonsAsync(true)).ReturnsAsync(RegistrationReasons2);
            RegistrationReasonsController = new RegistrationReasonsController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestMethod]
        public async Task RegistrationReasonsController_GetAsync_returns_RegistrationReason_DTOs()
        {
            var registrationReasonDtos = await RegistrationReasonsController.GetRegistrationReasonsAsync();
            Assert.AreEqual(registrationReasonDtos.Count(), RegistrationReasons.Count());
            for (int i = 0; i < RegistrationReasons.Count(); i++)
            {
                Assert.AreEqual(RegistrationReasons.ElementAt(i).Code, registrationReasonDtos.ElementAt(i).Code);
                Assert.AreEqual(RegistrationReasons.ElementAt(i).Description, registrationReasonDtos.ElementAt(i).Description);
            }
        }

        [TestMethod]
        public async Task RegistrationReasonsController_GetAsync_Bypass_Cache_returns_noncached_RegistrationReason_DTOs()
        {
            RegistrationReasonsController = new RegistrationReasonsController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
            RegistrationReasonsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            RegistrationReasonsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            var registrationReasonDtos = await RegistrationReasonsController.GetRegistrationReasonsAsync();
            Assert.AreEqual(registrationReasonDtos.Count(), RegistrationReasons2.Count());
            for (int i = 0; i < RegistrationReasons2.Count(); i++)
            {
                Assert.AreEqual(RegistrationReasons2.ElementAt(i).Code, registrationReasonDtos.ElementAt(i).Code);
                Assert.AreEqual(RegistrationReasons2.ElementAt(i).Description, registrationReasonDtos.ElementAt(i).Description);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegistrationReasonsController_GetAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetRegistrationReasonsAsync(It.IsAny<bool>())).Throws(new ColleagueSessionExpiredException("session expired"));
                await RegistrationReasonsController.GetRegistrationReasonsAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegistrationReasonsController_GetAsync_throws_exception_when_exception_caught()
        {
            referenceDataRepositoryMock.Setup(repo => repo.GetRegistrationReasonsAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            var RegistrationReasons = await RegistrationReasonsController.GetRegistrationReasonsAsync();
        }


        private IEnumerable<Domain.Student.Entities.RegistrationReason> BuildRegistrationReasons()
        {
            var RegistrationReasons = new List<Domain.Student.Entities.RegistrationReason>()
            {
                new Domain.Student.Entities.RegistrationReason("FUN", "Just for fun"),
                new Domain.Student.Entities.RegistrationReason("JOB", "Needed for my job")
            };

            return RegistrationReasons;
        }
        private IEnumerable<Domain.Student.Entities.RegistrationReason> BuildNonCachedRegistrationReasons()
        {
            var RegistrationReasons = new List<Domain.Student.Entities.RegistrationReason>()
            {
                new Domain.Student.Entities.RegistrationReason("FUN", "Just for fun"),
                new Domain.Student.Entities.RegistrationReason("JOB", "Needed for my job"),
                new Domain.Student.Entities.RegistrationReason("OTHER", "Other"),
            };

            return RegistrationReasons;
        }

    }
}
