// Copyright 2021 Ellucian Company L.P. and its affiliates.
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
    public class IntentToWithdrawCodesControllerTests
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
        private IntentToWithdrawCodesController IntentToWithdrawCodesController;
        private IEnumerable<Domain.Student.Entities.IntentToWithdrawCode> IntentToWithdrawCodes;
        private IEnumerable<Domain.Student.Entities.IntentToWithdrawCode> IntentToWithdrawCodes2;
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
            var adapter = new AutoMapperAdapter<Domain.Student.Entities.IntentToWithdrawCode, Dtos.Student.IntentToWithdrawCode>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.IntentToWithdrawCode, Dtos.Student.IntentToWithdrawCode>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            IntentToWithdrawCodes = BuildIntentToWithdrawCodes();
            IntentToWithdrawCodes2 = BuildNonCachedIntentToWithdrawCodes();
            referenceDataRepositoryMock.Setup(repo => repo.GetIntentToWithdrawCodesAsync(false)).ReturnsAsync(IntentToWithdrawCodes);
            referenceDataRepositoryMock.Setup(repo => repo.GetIntentToWithdrawCodesAsync(true)).ReturnsAsync(IntentToWithdrawCodes2);
            IntentToWithdrawCodesController = new IntentToWithdrawCodesController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
        }

        [TestMethod]
        public async Task IntentToWithdrawCodesController_GetAsync_returns_IntentToWithdrawCode_DTOs()
        {
            var IntentToWithdrawCodeDtos = await IntentToWithdrawCodesController.GetIntentToWithdrawCodesAsync();
            Assert.AreEqual(IntentToWithdrawCodeDtos.Count(), IntentToWithdrawCodes.Count());
            for (int i = 0; i < IntentToWithdrawCodes.Count(); i++)
            {
                Assert.AreEqual(IntentToWithdrawCodes.ElementAt(i).Code, IntentToWithdrawCodeDtos.ElementAt(i).Code);
                Assert.AreEqual(IntentToWithdrawCodes.ElementAt(i).Description, IntentToWithdrawCodeDtos.ElementAt(i).Description);
            }
        }

        [TestMethod]
        public async Task IntentToWithdrawCodesController_GetAsync_Bypass_Cache_returns_noncached_IntentToWithdrawCode_DTOs()
        {
            IntentToWithdrawCodesController = new IntentToWithdrawCodesController(adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
            IntentToWithdrawCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            IntentToWithdrawCodesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            var IntentToWithdrawCodeDtos = await IntentToWithdrawCodesController.GetIntentToWithdrawCodesAsync();
            Assert.AreEqual(IntentToWithdrawCodeDtos.Count(), IntentToWithdrawCodes2.Count());
            for (int i = 0; i < IntentToWithdrawCodes2.Count(); i++)
            {
                Assert.AreEqual(IntentToWithdrawCodes2.ElementAt(i).Code, IntentToWithdrawCodeDtos.ElementAt(i).Code);
                Assert.AreEqual(IntentToWithdrawCodes2.ElementAt(i).Description, IntentToWithdrawCodeDtos.ElementAt(i).Description);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task IntentToWithdrawCodesController_GetAsync_throws_exception_when_exception_caught()
        {
            referenceDataRepositoryMock.Setup(repo => repo.GetIntentToWithdrawCodesAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            var IntentToWithdrawCodes = await IntentToWithdrawCodesController.GetIntentToWithdrawCodesAsync();
        }


        private IEnumerable<Domain.Student.Entities.IntentToWithdrawCode> BuildIntentToWithdrawCodes()
        {
            var IntentToWithdrawCodes = new List<Domain.Student.Entities.IntentToWithdrawCode>()
            {
                new Domain.Student.Entities.IntentToWithdrawCode("1", "NEWSAD", "I saw it in a newspaper ad"),
                new Domain.Student.Entities.IntentToWithdrawCode("2", "ONLINE", "I found it online")
            };

            return IntentToWithdrawCodes;
        }
        private IEnumerable<Domain.Student.Entities.IntentToWithdrawCode> BuildNonCachedIntentToWithdrawCodes()
        {
            var IntentToWithdrawCodes = new List<Domain.Student.Entities.IntentToWithdrawCode>()
            {
                new Domain.Student.Entities.IntentToWithdrawCode("1", "NEWSAD", "I saw it in a newspaper ad"),
                new Domain.Student.Entities.IntentToWithdrawCode("2", "ONLINE", "I found it online"),
                new Domain.Student.Entities.IntentToWithdrawCode("3", "OTHER", "Other"),
            };

            return IntentToWithdrawCodes;
        }

    }
}
