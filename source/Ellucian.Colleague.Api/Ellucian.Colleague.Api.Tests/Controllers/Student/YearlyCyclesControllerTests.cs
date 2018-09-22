// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class YearlyCyclesControllerTests
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
        private YearlyCyclesController yearlyCyclesController;
        private IEnumerable<Domain.Student.Entities.YearlyCycle> yearlyCycles;
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
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.YearlyCycle, YearlyCycle>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.YearlyCycle, YearlyCycle>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            yearlyCycles = BuildYearlyCycles();
            yearlyCyclesController = new YearlyCyclesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            yearlyCyclesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task YearlyCyclesController_ReturnsYearlyCycleDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetYearlyCyclesAsync()).Returns(Task.FromResult(yearlyCycles));
            var yearlyCyclesDtos = await yearlyCyclesController.GetAsync();
            Assert.IsTrue(yearlyCyclesDtos is IEnumerable<Dtos.Student.YearlyCycle>);
            Assert.AreEqual(2, yearlyCyclesDtos.Count());
        }

        [TestMethod]
        public async Task YearlyCyclesController_NullRepositoryResponse_ReturnsEmptyYearlyCyclesDtos()
        {
            IEnumerable<Domain.Student.Entities.YearlyCycle> nullYearlyCyclesEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetYearlyCyclesAsync()).Returns(Task.FromResult(nullYearlyCyclesEntities));
            var yearlyCyclesDtos = await yearlyCyclesController.GetAsync();
            Assert.IsTrue(yearlyCyclesDtos is IEnumerable<Dtos.Student.YearlyCycle>);
            Assert.AreEqual(0, yearlyCyclesDtos.Count());
        }

        [TestMethod]
        public async Task YearlyCyclesController_EmptyRepositoryResponse_ReturnsEmptyYearlyCyclesDtos()
        {
            IEnumerable<Domain.Student.Entities.YearlyCycle> emptyYearlyCyclesEntities = new List<Domain.Student.Entities.YearlyCycle>();
            referenceDataRepositoryMock.Setup(x => x.GetYearlyCyclesAsync()).Returns(Task.FromResult(emptyYearlyCyclesEntities));
            var yearlyCyclesDtos = await yearlyCyclesController.GetAsync();
            Assert.IsTrue(yearlyCyclesDtos is IEnumerable<Dtos.Student.YearlyCycle>);
            Assert.AreEqual(0, yearlyCyclesDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task YearlyCyclesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetYearlyCyclesAsync()).Throws(new ApplicationException());
                var yearlyCycles = await yearlyCyclesController.GetAsync();
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

        private IEnumerable<Domain.Student.Entities.YearlyCycle> BuildYearlyCycles()
        {
            var yearlyCycles = new List<Domain.Student.Entities.YearlyCycle>()
                {
                    new Domain.Student.Entities.YearlyCycle("OY","Odd Years Only"),
                    new Domain.Student.Entities.YearlyCycle("EY","Even Years Only")
                };

            return yearlyCycles;
        }
    }
}
