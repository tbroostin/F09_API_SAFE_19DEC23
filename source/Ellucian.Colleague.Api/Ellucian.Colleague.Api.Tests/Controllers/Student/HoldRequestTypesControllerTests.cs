// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class HoldRequestTypesControllerTests
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
        private HoldRequestTypesController holdRequestTypesController;
        private IEnumerable<Domain.Student.Entities.HoldRequestType> holdRequestTypes;
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
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.HoldRequestType, HoldRequestType>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.HoldRequestType, HoldRequestType>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            holdRequestTypes = BuildHoldRequestTypes();
            holdRequestTypesController = new HoldRequestTypesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            holdRequestTypesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task HoldRequestTypesController_ReturnsHoldRequestTypeDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetHoldRequestTypesAsync()).Returns(Task.FromResult(holdRequestTypes));
            var holdRequestTypesDtos = await holdRequestTypesController.GetHoldRequestTypesAsync();
            Assert.IsTrue(holdRequestTypesDtos is IEnumerable<Dtos.Student.HoldRequestType>);
            Assert.AreEqual(2, holdRequestTypesDtos.Count());
        }

        [TestMethod]
        public async Task HoldRequestTypesController_NullRepositoryResponse_ReturnsEmptyHoldRequestTypesDtos()
        {
            IEnumerable<Domain.Student.Entities.HoldRequestType> nullHoldRequestTypesEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetHoldRequestTypesAsync()).Returns(Task.FromResult(nullHoldRequestTypesEntities));
            var holdRequestTypesDtos = await holdRequestTypesController.GetHoldRequestTypesAsync();
            Assert.IsTrue(holdRequestTypesDtos is IEnumerable<Dtos.Student.HoldRequestType>);
            Assert.AreEqual(0, holdRequestTypesDtos.Count());
        }

        [TestMethod]
        public async Task HoldRequestTypesController_EmptyRepositoryResponse_ReturnsEmptyHoldRequestTypesDtos()
        {
            IEnumerable<Domain.Student.Entities.HoldRequestType> emptyHoldRequestTypeEntities = new List<Domain.Student.Entities.HoldRequestType>();
            referenceDataRepositoryMock.Setup(x => x.GetHoldRequestTypesAsync()).Returns(Task.FromResult(emptyHoldRequestTypeEntities));
            var holdRequestTypeDtos = await holdRequestTypesController.GetHoldRequestTypesAsync();
            Assert.IsTrue(holdRequestTypeDtos is IEnumerable<Dtos.Student.HoldRequestType>);
            Assert.AreEqual(0, holdRequestTypeDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task HoldRequestTypesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetHoldRequestTypesAsync()).Throws(new ApplicationException());
                var holdRequestTypes = await holdRequestTypesController.GetHoldRequestTypesAsync();
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

        private IEnumerable<Domain.Student.Entities.HoldRequestType> BuildHoldRequestTypes()
        {
            var holdRequestType = new List<Domain.Student.Entities.HoldRequestType>()
                {
                    new Domain.Student.Entities.HoldRequestType("GRADE","All grades posted"),
                    new Domain.Student.Entities.HoldRequestType("SEM","End of semester")
                };

            return holdRequestType;
        }
    }
}
