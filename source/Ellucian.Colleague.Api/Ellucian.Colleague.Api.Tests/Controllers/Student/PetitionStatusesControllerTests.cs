// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class PetitionStatusesControllerTests
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
        private PetitionStatusesController petitionStatusesController;
        private List<Domain.Student.Entities.PetitionStatus> petitionStatuses;
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
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PetitionStatus, PetitionStatus>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PetitionStatus, PetitionStatus>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            petitionStatuses = BuildPetitionStatuses();
            petitionStatusesController = new PetitionStatusesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            petitionStatusesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task PetitionStatusesController_ReturnsPetitionStatusDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetPetitionStatusesAsync()).Returns(Task.FromResult(petitionStatuses.AsEnumerable()));
            var petitionStatusDtos = await petitionStatusesController.GetAsync();
            Assert.IsTrue(petitionStatusDtos is IEnumerable<Dtos.Student.PetitionStatus>);
            Assert.AreEqual(2, petitionStatusDtos.Count());
        }

        [TestMethod]
        public async Task PetitionStatusesController_NullRepositoryResponse_ReturnsEmptyPetitionStatusDtos()
        {
            List<Domain.Student.Entities.PetitionStatus> nullPetitionStatusEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetPetitionStatusesAsync()).Returns(Task.FromResult(nullPetitionStatusEntities.AsEnumerable()));
            var petitionStatusDtos = await petitionStatusesController.GetAsync();
            Assert.IsTrue(petitionStatusDtos is IEnumerable<Dtos.Student.PetitionStatus>);
            Assert.AreEqual(0, petitionStatusDtos.Count());
        }

        [TestMethod]
        public async Task PetitionStatusesController_EmptyRepositoryResponse_ReturnsEmptyPetitionStatusDtos()
        {
            List<Domain.Student.Entities.PetitionStatus> emptyPetitionStatusEntities = new List<Domain.Student.Entities.PetitionStatus>();
            referenceDataRepositoryMock.Setup(x => x.GetPetitionStatusesAsync()).Returns(Task.FromResult(emptyPetitionStatusEntities.AsEnumerable()));
            var petitionStatusDtos =await petitionStatusesController.GetAsync();
            Assert.IsTrue(petitionStatusDtos is IEnumerable<Dtos.Student.PetitionStatus>);
            Assert.AreEqual(0, petitionStatusDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PetitionStatusesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetPetitionStatusesAsync()).Throws(new ApplicationException());
                var petitionStatuses = await petitionStatusesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        private List<Domain.Student.Entities.PetitionStatus> BuildPetitionStatuses()
        {
            var petitionStatuses = new List<Domain.Student.Entities.PetitionStatus>()
                {
                    new Domain.Student.Entities.PetitionStatus("ACC", "Accepted", true),
                    new Domain.Student.Entities.PetitionStatus("DEN", "Denied")
                };

            return petitionStatuses;
        }
    }
}
