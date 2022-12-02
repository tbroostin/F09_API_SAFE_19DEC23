/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Student.Services;
//using Ellucian.Colleague.Domain.Base.;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
//using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AdmissionPopulationsServiceTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory CurrentUserFactory;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public Mock<IStudentReferenceDataRepository> referenceDataRepository;
        AdmissionPopulationService admissionPopulationService;
        List<Dtos.AdmissionPopulations> admissionPopulationDtoList = new List<Dtos.AdmissionPopulations>();
        List<Domain.Student.Entities.AdmissionPopulation> admissionPopulationEntityList = new List<Domain.Student.Entities.AdmissionPopulation>();
        string id = "03ef76f3-61be-4990-8a9d-9a80282fc420";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            BuildData();
            referenceDataRepository = new Mock<IStudentReferenceDataRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            admissionPopulationService = new AdmissionPopulationService(referenceDataRepository.Object,  adapterRegistryMock.Object,
                CurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            referenceDataRepository = null;
            admissionPopulationService = null;
            admissionPopulationDtoList = null;
            admissionPopulationEntityList = null;
        }

        [TestMethod]
        public async Task AdmissionPopulations_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationEntityList);

            var actuals = await admissionPopulationService.GetAdmissionPopulationsAsync(It.IsAny<bool>());

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = admissionPopulationDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task AdmissionPopulations_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetAdmissionPopulationsAsync(true)).ReturnsAsync(admissionPopulationEntityList);

            var actuals = await admissionPopulationService.GetAdmissionPopulationsAsync(true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = admissionPopulationDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task AdmissionPopulations_GetById()
        {
            var expected = admissionPopulationDtoList.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            referenceDataRepository.Setup(i => i.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationEntityList);

            var actual = await admissionPopulationService.GetAdmissionPopulationsByGuidAsync(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.IsNull(actual.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AdmissionPopulations_GetById_InvalidOperationException()
        {
            referenceDataRepository.Setup(i => i.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationEntityList);
            var actual = await admissionPopulationService.GetAdmissionPopulationsByGuidAsync("abc");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AdmissionPopulations_GetById_Exception()
        {
            referenceDataRepository.Setup(i => i.GetAdmissionPopulationsAsync(true)).ThrowsAsync(new Exception());
            var actual = await admissionPopulationService.GetAdmissionPopulationsByGuidAsync(It.IsAny<string>());
        }

        private void BuildData()
        {
            admissionPopulationEntityList = new List<Domain.Student.Entities.AdmissionPopulation>() 
            {
                new Domain.Student.Entities.AdmissionPopulation("03ef76f3-61be-4990-8a9d-9a80282fc420", "CR", "Certificate"),
                new Domain.Student.Entities.AdmissionPopulation("d2f4f0af-6714-48c7-88dd-1c40cb407b6c", "FH", "Freshman Honors"),
                new Domain.Student.Entities.AdmissionPopulation("c517d7a5-f06a-42c8-85ad-b6320e1c0c2a", "FR", "First Time Freshman"),
                new Domain.Student.Entities.AdmissionPopulation("6c591aaa-5d33-4b19-b5ed-f6cf8956ef0a", "GD", "Graduate"),
                new Domain.Student.Entities.AdmissionPopulation("81cd5b52-9705-4b1b-8eed-669c63db05e2", "ND", "Non-Degree"),
                new Domain.Student.Entities.AdmissionPopulation("164dc1ad-4d72-4dae-987d-52f761bb0132", "TR", "Transfer"),
            };
            foreach (var entity in admissionPopulationEntityList)
            {
                admissionPopulationDtoList.Add(new Dtos.AdmissionPopulations()
                {
                    Id = entity.Guid,
                    Code = entity.Code,
                    Title = entity.Description,
                    Description = null,
                });
            }
        }
        public void MockInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            CurrentUserFactory = new UserFactorySubset();
        }
        public class UserFactorySubset : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000001",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
