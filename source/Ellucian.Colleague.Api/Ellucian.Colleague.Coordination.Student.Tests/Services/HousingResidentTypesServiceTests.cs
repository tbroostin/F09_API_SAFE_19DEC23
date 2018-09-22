//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    // sets up a current user
    public abstract class CurrentUserSetup
    {
        protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

        public class PersonUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "George",
                        PersonId = "0000015",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Faculty",
                        Roles = new List<string>() { "Faculty" },
                        SessionFixationId = "abc123",
                    });
                }
            }
        }
    }

    [TestClass]
    public class HousingResidentTypesServiceTests : CurrentUserSetup
    {
        private const string housingResidentTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string housingResidentTypesCode = "AT";
        private ICollection<HousingResidentType> _housingResidentTypesCollection;
        private HousingResidentTypesService _housingResidentTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private Domain.Entities.Permission permissionViewAnyAdvisee;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            // Mock permissions
            permissionViewAnyAdvisee = new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee);
            personRole.AddPermission(permissionViewAnyAdvisee);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            _housingResidentTypesCollection = new List<HousingResidentType>()
                {
                    new HousingResidentType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new HousingResidentType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new HousingResidentType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetHousingResidentTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_housingResidentTypesCollection);

            _housingResidentTypesService = new HousingResidentTypesService(_referenceRepositoryMock.Object, adapterRegistry, currentUserFactory, roleRepoMock.Object, _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _housingResidentTypesService = null;
            _housingResidentTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task HousingResidentTypesService_GetHousingResidentTypesAsync()
        {
            var results = await _housingResidentTypesService.GetHousingResidentTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<HousingResidentTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task HousingResidentTypesService_GetHousingResidentTypesAsync_Count()
        {
            var results = await _housingResidentTypesService.GetHousingResidentTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task HousingResidentTypesService_GetHousingResidentTypesAsync_Properties()
        {
            var result =
                (await _housingResidentTypesService.GetHousingResidentTypesAsync(true)).FirstOrDefault(x => x.Code == housingResidentTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task HousingResidentTypesService_GetHousingResidentTypesAsync_Expected()
        {
            var expectedResults = _housingResidentTypesCollection.FirstOrDefault(c => c.Guid == housingResidentTypesGuid);
            var actualResult =
                (await _housingResidentTypesService.GetHousingResidentTypesAsync(true)).FirstOrDefault(x => x.Id == housingResidentTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task HousingResidentTypesService_GetHousingResidentTypesByGuidAsync_Empty()
        {
            await _housingResidentTypesService.GetHousingResidentTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task HousingResidentTypesService_GetHousingResidentTypesByGuidAsync_Null()
        {
            await _housingResidentTypesService.GetHousingResidentTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task HousingResidentTypesService_GetHousingResidentTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetHousingResidentTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _housingResidentTypesService.GetHousingResidentTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task HousingResidentTypesService_GetHousingResidentTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _housingResidentTypesCollection.First(c => c.Guid == housingResidentTypesGuid);
            var actualResult =
                await _housingResidentTypesService.GetHousingResidentTypesByGuidAsync(housingResidentTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task HousingResidentTypesService_GetHousingResidentTypesByGuidAsync_Properties()
        {
            var result =
                await _housingResidentTypesService.GetHousingResidentTypesByGuidAsync(housingResidentTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}