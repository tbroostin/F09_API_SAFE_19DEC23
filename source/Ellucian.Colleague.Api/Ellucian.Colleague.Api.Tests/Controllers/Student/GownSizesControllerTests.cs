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
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class GownSizesControllerTests
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
        private GownSizesController gownSizesController;
        private IEnumerable<Domain.Student.Entities.GownSize> gownSizes;
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
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GownSize, GownSize>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GownSize, GownSize>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            gownSizes = BuildGownSizes();
            gownSizesController = new GownSizesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            gownSizesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task GownSizesController_ReturnsGownSizeDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetGownSizesAsync()).Returns(Task.FromResult(gownSizes));
            var gownSizesDtos = await gownSizesController.GetAsync();
            Assert.IsTrue(gownSizesDtos is IEnumerable<Dtos.Student.GownSize>);
            Assert.AreEqual(2, gownSizesDtos.Count());
        }

        [TestMethod]
        public async Task GownSizesController_NullRepositoryResponse_ReturnsEmptyGownSizeDtos()
        {
            IEnumerable<Domain.Student.Entities.GownSize> nullGownSizeEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetGownSizesAsync()).Returns(Task.FromResult(nullGownSizeEntities));
            var gownSizesDtos = await gownSizesController.GetAsync();
            Assert.IsTrue(gownSizesDtos is IEnumerable<Dtos.Student.GownSize>);
            Assert.AreEqual(0, gownSizesDtos.Count());
        }

        [TestMethod]
        public async Task GownSizesController_EmptyRepositoryResponse_ReturnsEmptyGownSizeDtos()
        {
            IEnumerable<Domain.Student.Entities.GownSize> emptyGownSizeEntities = new List<Domain.Student.Entities.GownSize>();
            referenceDataRepositoryMock.Setup(x => x.GetGownSizesAsync()).Returns(Task.FromResult(emptyGownSizeEntities));
            var gownSizeDtos = await gownSizesController.GetAsync();
            Assert.IsTrue(gownSizeDtos is IEnumerable<Dtos.Student.GownSize>);
            Assert.AreEqual(0, gownSizeDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GownSizesController_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetGownSizesAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                await gownSizesController.GetAsync();
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
        public async Task GownSizesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetGownSizesAsync()).Throws(new ApplicationException());
                await gownSizesController.GetAsync();
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

        private IEnumerable<Domain.Student.Entities.GownSize> BuildGownSizes()
        {
            var gownSizes = new List<Domain.Student.Entities.GownSize>()
                {
                    new Domain.Student.Entities.GownSize("SMALL","Small"),
                    new Domain.Student.Entities.GownSize("MEDIUM","Medium")
                };

            return gownSizes;
        }
    }
}
