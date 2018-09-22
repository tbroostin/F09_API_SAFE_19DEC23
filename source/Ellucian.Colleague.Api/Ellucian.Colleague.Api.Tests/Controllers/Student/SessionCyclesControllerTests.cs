// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Repositories;
using Moq;
using Ellucian.Colleague.Api.Controllers.Student;
using System.Collections.Generic;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SessionCyclesControllerTests
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
        private SessionCyclesController sessionCyclesController;
        private IEnumerable<Domain.Student.Entities.SessionCycle> sessionCycles;
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
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SessionCycle, SessionCycle>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SessionCycle, SessionCycle>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            sessionCycles = BuildSessionCycles();
            sessionCyclesController = new SessionCyclesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sessionCyclesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task SessionCyclesController_ReturnsSessionCycleDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetSessionCyclesAsync()).Returns(Task.FromResult(sessionCycles));
            var sessionCyclesDtos = await sessionCyclesController.GetAsync();
            Assert.IsTrue(sessionCyclesDtos is IEnumerable<Dtos.Student.SessionCycle>);
            Assert.AreEqual(2, sessionCyclesDtos.Count());
        }

        [TestMethod]
        public async Task SessionCyclesController_NullRepositoryResponse_ReturnsEmptySessionCyclesDtos()
        {
            IEnumerable<Domain.Student.Entities.SessionCycle> nullSessionCyclesEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetSessionCyclesAsync()).Returns(Task.FromResult(nullSessionCyclesEntities));
            var sessionCyclesDtos = await sessionCyclesController.GetAsync();
            Assert.IsTrue(sessionCyclesDtos is IEnumerable<Dtos.Student.SessionCycle>);
            Assert.AreEqual(0, sessionCyclesDtos.Count());
        }

        [TestMethod]
        public async Task SessionCyclesController_EmptyRepositoryResponse_ReturnsEmptySessionCyclesDtos()
        {
            IEnumerable<Domain.Student.Entities.SessionCycle> emptySessionCyclesEntities = new List<Domain.Student.Entities.SessionCycle>();
            referenceDataRepositoryMock.Setup(x => x.GetSessionCyclesAsync()).Returns(Task.FromResult(emptySessionCyclesEntities));
            var sessionCyclesDtos = await sessionCyclesController.GetAsync();
            Assert.IsTrue(sessionCyclesDtos is IEnumerable<Dtos.Student.SessionCycle>);
            Assert.AreEqual(0, sessionCyclesDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SessionCyclesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetSessionCyclesAsync()).Throws(new ApplicationException());
                var SessionCycles = await sessionCyclesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch(System.Exception e)
            {
                throw e;
            }
        }

        private IEnumerable<Domain.Student.Entities.SessionCycle> BuildSessionCycles()
        {
            var sessionCycles = new List<Domain.Student.Entities.SessionCycle>()
                {
                    new Domain.Student.Entities.SessionCycle("SO","Spring Only"),
                    new Domain.Student.Entities.SessionCycle("FO","Fall Only")
                };

            return sessionCycles;
        }
    }
}
