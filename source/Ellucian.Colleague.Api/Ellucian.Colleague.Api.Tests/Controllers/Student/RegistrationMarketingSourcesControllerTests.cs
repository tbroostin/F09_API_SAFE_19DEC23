// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class RegistrationMarketingSourcesControllerTests
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
        private RegistrationMarketingSourcesController RegistrationMarketingSourcesController;
        private IEnumerable<Domain.Student.Entities.RegistrationMarketingSource> RegistrationMarketingSources;
        private IEnumerable<Domain.Student.Entities.RegistrationMarketingSource> RegistrationMarketingSources2;
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
            var adapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationMarketingSource, Dtos.Student.RegistrationMarketingSource>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.RegistrationMarketingSource, Dtos.Student.RegistrationMarketingSource>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            RegistrationMarketingSources = BuildRegistrationMarketingSources();
            RegistrationMarketingSources2 = BuildNonCachedRegistrationMarketingSources();
            referenceDataRepositoryMock.Setup(repo => repo.GetRegistrationMarketingSourcesAsync(false)).ReturnsAsync(RegistrationMarketingSources);
            referenceDataRepositoryMock.Setup(repo => repo.GetRegistrationMarketingSourcesAsync(true)).ReturnsAsync(RegistrationMarketingSources2);
            RegistrationMarketingSourcesController = new RegistrationMarketingSourcesController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestMethod]
        public async Task RegistrationMarketingSourcesController_GetAsync_returns_RegistrationMarketingSource_DTOs()
        {
            var registrationMarketingSourceDtos = await RegistrationMarketingSourcesController.GetRegistrationMarketingSourcesAsync();
            Assert.AreEqual(registrationMarketingSourceDtos.Count(), RegistrationMarketingSources.Count());
            for (int i = 0; i < RegistrationMarketingSources.Count(); i++)
            {
                Assert.AreEqual(RegistrationMarketingSources.ElementAt(i).Code, registrationMarketingSourceDtos.ElementAt(i).Code);
                Assert.AreEqual(RegistrationMarketingSources.ElementAt(i).Description, registrationMarketingSourceDtos.ElementAt(i).Description);
            }
        }

        [TestMethod]
        public async Task RegistrationMarketingSourcesController_GetAsync_Bypass_Cache_returns_noncached_RegistrationMarketingSource_DTOs()
        {
            RegistrationMarketingSourcesController = new RegistrationMarketingSourcesController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
            RegistrationMarketingSourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            RegistrationMarketingSourcesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            var registrationMarketingSourceDtos = await RegistrationMarketingSourcesController.GetRegistrationMarketingSourcesAsync();
            Assert.AreEqual(registrationMarketingSourceDtos.Count(), RegistrationMarketingSources2.Count());
            for (int i = 0; i < RegistrationMarketingSources2.Count(); i++)
            {
                Assert.AreEqual(RegistrationMarketingSources2.ElementAt(i).Code, registrationMarketingSourceDtos.ElementAt(i).Code);
                Assert.AreEqual(RegistrationMarketingSources2.ElementAt(i).Description, registrationMarketingSourceDtos.ElementAt(i).Description);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegistrationMarketingSourcesController_GetAsync_throws_exception_when_exception_caught()
        {
            referenceDataRepositoryMock.Setup(repo => repo.GetRegistrationMarketingSourcesAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            var RegistrationMarketingSources = await RegistrationMarketingSourcesController.GetRegistrationMarketingSourcesAsync();
        }


        private IEnumerable<Domain.Student.Entities.RegistrationMarketingSource> BuildRegistrationMarketingSources()
        {
            var RegistrationMarketingSources = new List<Domain.Student.Entities.RegistrationMarketingSource>()
            {
                new Domain.Student.Entities.RegistrationMarketingSource("NEWSAD", "I saw it in a newspaper ad"),
                new Domain.Student.Entities.RegistrationMarketingSource("ONLINE", "I found it online")
            };

            return RegistrationMarketingSources;
        }
        private IEnumerable<Domain.Student.Entities.RegistrationMarketingSource> BuildNonCachedRegistrationMarketingSources()
        {
            var RegistrationMarketingSources = new List<Domain.Student.Entities.RegistrationMarketingSource>()
            {
                new Domain.Student.Entities.RegistrationMarketingSource("NEWSAD", "I saw it in a newspaper ad"),
                new Domain.Student.Entities.RegistrationMarketingSource("ONLINE", "I found it online"),
                new Domain.Student.Entities.RegistrationMarketingSource("OTHER", "Other"),
            };

            return RegistrationMarketingSources;
        }

    }
}
