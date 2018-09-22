// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
     public class StatesControllerTests
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
        private StatesController statesController;
        private IEnumerable<Domain.Base.Entities.State> states;
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
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.State, State>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.State, State>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            states = BuildStates();
            statesController = new StatesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            statesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task StatesController_ReturnsStatesReasonDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetStateCodesAsync()).Returns(Task.FromResult(states));
            var stateDtos = await statesController.GetAsync();
            Assert.IsTrue(stateDtos is IEnumerable<State>);
            Assert.AreEqual(states.Count(), stateDtos.Count());
        }

        [TestMethod]
        public async Task StatesController_NullRepositoryResponse_ReturnsEmptyStatesReasonDtos()
        {
            IEnumerable<Domain.Base.Entities.State> nullStateEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetStateCodesAsync()).Returns(Task.FromResult(nullStateEntities));
            var stateDtos = await statesController.GetAsync();
            Assert.IsTrue(stateDtos is IEnumerable<State>);
            Assert.AreEqual(0, stateDtos.Count());
        }

        [TestMethod]
        public async Task StatesController_EmptyRepositoryResponse_ReturnsEmptyStatesReasonDtos()
        {
            IEnumerable<Domain.Base.Entities.State> emptyStateEntities = new List<Domain.Base.Entities.State>();
            referenceDataRepositoryMock.Setup(x => x.GetStateCodesAsync()).Returns(Task.FromResult(emptyStateEntities));
            var stateDtos = await statesController.GetAsync();
            Assert.IsTrue(stateDtos is IEnumerable<State>);
            Assert.AreEqual(0, stateDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StatesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetStateCodesAsync()).Throws(new ApplicationException());
                var stateDtos = await statesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        private IEnumerable<Domain.Base.Entities.State> BuildStates()
        {
            var states = new List<Domain.Base.Entities.State>()
                {
                    new Domain.Base.Entities.State("VA", "Virginia"),
                    new Domain.Base.Entities.State("TX", "Texas"),
                    new Domain.Base.Entities.State("SC", "South Carolina")
                };

            return states;
        }
     }
}
