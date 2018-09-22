// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Repositories;
using Moq;
using Ellucian.Colleague.Api.Controllers.Base;
using System.Collections.Generic;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.Base;
using System.Threading.Tasks;
using System.Linq;
using System.Web.Http;


namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CommencementSitesControllerTests
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
        private IReferenceDataRepository referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private CommencementSitesController commencementSitesController;
        private IEnumerable<Domain.Base.Entities.CommencementSite> commencementSites;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.CommencementSite, CommencementSite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CommencementSite, CommencementSite>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            commencementSites = BuildCommencementSites();
            commencementSitesController = new CommencementSitesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            commencementSitesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task CommencementSitesController_ReturnsCommencementSiteDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetCommencementSitesAsync()).Returns(Task.FromResult(commencementSites));
            var commencementSiteDtos = await commencementSitesController.GetAsync();
            Assert.IsTrue(commencementSiteDtos is IEnumerable<Dtos.Base.CommencementSite>);
            Assert.AreEqual(2, commencementSiteDtos.Count());
        }

        [TestMethod]
        public async Task CommencementSitesController_NullRepositoryResponse_ReturnsEmptyCommencementSiteDtos()
        {
            IEnumerable<Domain.Base.Entities.CommencementSite> nullCommencementSiteEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetCommencementSitesAsync()).Returns(Task.FromResult(nullCommencementSiteEntities));
            var CommencementSiteDtos = await commencementSitesController.GetAsync();
            Assert.IsTrue(CommencementSiteDtos is IEnumerable<Dtos.Base.CommencementSite>);
            Assert.AreEqual(0, CommencementSiteDtos.Count());
        }

        [TestMethod]
        public async Task CommencementSitesController_EmptyRepositoryResponse_ReturnsEmptyCommencementSiteDtos()
        {
            IEnumerable<Domain.Base.Entities.CommencementSite> emptyCommencementSiteEntities = new List<Domain.Base.Entities.CommencementSite>();
            referenceDataRepositoryMock.Setup(x => x.GetCommencementSitesAsync()).Returns(Task.FromResult(emptyCommencementSiteEntities));
            var commencementSiteDtos = await commencementSitesController.GetAsync();
            Assert.IsTrue(commencementSiteDtos is IEnumerable<Dtos.Base.CommencementSite>);
            Assert.AreEqual(0, commencementSiteDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommencementSitesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetCommencementSitesAsync()).Throws(new ApplicationException());
                var commencementSites = await commencementSitesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        private IEnumerable<Domain.Base.Entities.CommencementSite> BuildCommencementSites()
        {
            var commencementSites = new List<Domain.Base.Entities.CommencementSite>()
                {
                    new Domain.Base.Entities.CommencementSite("PATC", "Patriot Center"),
                    new Domain.Base.Entities.CommencementSite("OTHER", "Other spot")
                };

            return commencementSites;
        }

    }
}
