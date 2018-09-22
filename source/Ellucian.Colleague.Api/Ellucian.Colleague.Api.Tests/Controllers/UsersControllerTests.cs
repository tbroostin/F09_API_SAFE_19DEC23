// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers
{
    [TestClass]
    public class UsersControllerTests
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

        #region Initialization and Cleanup

        private UsersController userController;
        private IAdapterRegistry AdapterRegistry;
        ILogger logger = new Mock<ILogger>().Object;
        private Mock<IProxyService> proxyServiceMock;
        private IProxyService proxyService;
        private Mock<ISelfservicePreferencesService> preferencesServiceMock;
        private ISelfservicePreferencesService preferencesService;
        private Mock<IUserRepository> userRepoMock;
        private IUserRepository userRepository;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);

            proxyServiceMock = new Mock<IProxyService>();
            proxyService = proxyServiceMock.Object;

            preferencesServiceMock = new Mock<ISelfservicePreferencesService>();
            preferencesService = preferencesServiceMock.Object;

            userRepoMock = new Mock<IUserRepository>();
            userRepository = userRepoMock.Object;


            userController = new UsersController(userRepository, AdapterRegistry, logger, proxyService, preferencesService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            userController = null;
            userRepository = null;
            userRepository = null;
            proxyService = null;
            AdapterRegistry = null;
        }

        #endregion

        #region Self Service Preference Tests

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeleteSelfServicePreferenceAsync_NullPersonId()
        {
            await userController.DeleteSelfServicePreferenceAsync(null, "prefKey");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeleteSelfServicePreferenceAsync_NullPreferenceType()
        {
            await userController.DeleteSelfServicePreferenceAsync("0000015", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeleteSelfServicePreferenceAsync_EmptyPersonId()
        {
            await userController.DeleteSelfServicePreferenceAsync("", "prefKey");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DeleteSelfServicePreferenceAsync_EmptyPreferenceType()
        {
            await userController.DeleteSelfServicePreferenceAsync("0000015", "");
        }

        [TestMethod]
        public async Task DeleteSelfServicePreferenceAsync_Success()
        {
            await userController.DeleteSelfServicePreferenceAsync("0000015", "prefKey");
        }

        #endregion

    }
}
