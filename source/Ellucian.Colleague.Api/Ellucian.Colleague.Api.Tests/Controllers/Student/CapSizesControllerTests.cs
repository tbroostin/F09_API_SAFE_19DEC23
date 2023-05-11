// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CapSizesControllerTests
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
        private CapSizesController capSizesController;
        private IEnumerable<Domain.Student.Entities.CapSize> capSizes;
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
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.CapSize, CapSize>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CapSize, CapSize>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            capSizes = BuildCapSizes();
            capSizesController = new CapSizesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            capSizesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task CapSizesController_ReturnsCapSizeDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetCapSizesAsync()).Returns(Task.FromResult(capSizes));
            var capSizesDtos = await capSizesController.GetAsync();
            Assert.IsTrue(capSizesDtos is IEnumerable<Dtos.Student.CapSize>);
            Assert.AreEqual(2, capSizesDtos.Count());
        }

        [TestMethod]
        public async Task CapSizesController_NullRepositoryResponse_ReturnsEmptyCapSizeDtos()
        {
            IEnumerable<Domain.Student.Entities.CapSize> nullcapSizeEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetCapSizesAsync()).Returns(Task.FromResult(nullcapSizeEntities));
            var capSizesDtos = await capSizesController.GetAsync();
            Assert.IsTrue(capSizesDtos is IEnumerable<Dtos.Student.CapSize>);
            Assert.AreEqual(0, capSizesDtos.Count());
        }

        [TestMethod]
        public async Task CapSizesController_EmptyRepositoryResponse_ReturnsEmptyCapSizeDtos()
        {
            IEnumerable<Domain.Student.Entities.CapSize> emptyCapSizeEntities = new List<Domain.Student.Entities.CapSize>();
            referenceDataRepositoryMock.Setup(x => x.GetCapSizesAsync()).Returns(Task.FromResult(emptyCapSizeEntities));
            var capSizeDtos = await capSizesController.GetAsync();
            Assert.IsTrue(capSizeDtos is IEnumerable<Dtos.Student.CapSize>);
            Assert.AreEqual(0, capSizeDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CapSizesController_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetCapSizesAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                await capSizesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CapSizesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetCapSizesAsync()).Throws(new ApplicationException());
                await capSizesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        private IEnumerable<Domain.Student.Entities.CapSize> BuildCapSizes()
        {
            var capSizes = new List<Domain.Student.Entities.CapSize>()
                {
                    new Domain.Student.Entities.CapSize("SMALL","Small"),
                    new Domain.Student.Entities.CapSize("MEDIUM","Medium")
                };

            return capSizes;
        }
    }
}
