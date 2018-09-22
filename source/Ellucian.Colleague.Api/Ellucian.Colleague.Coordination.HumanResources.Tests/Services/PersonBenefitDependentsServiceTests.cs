//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PersonBenefitDependentsServiceTests : CurrentUserSetup
    {
        private const string personBenefitDependentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string personBenefitDependentsPersonId = "PID";
        private ICollection<PersonBenefitDependent> _personBenefitDependentsCollection;
        private Tuple<IEnumerable<PersonBenefitDependent>, int> personBenefitDependentsTuple;
        private ICollection<PersonBenefitDependents> _personBenefitDependentsDtoCollection;
        private Tuple<IEnumerable<PersonBenefitDependents>, int> personBenefitDependentsDtoTuple;
        private PersonBenefitDependentsService _personBenefitDependentsService;

        private Mock<IPersonBenefitDependentsRepository> _personBenefitDependentRepositoryMock;
        private Mock<IPositionRepository> _positionRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _personBenefitDependentRepositoryMock = new Mock<IPersonBenefitDependentsRepository>();
            _positionRepositoryMock = new Mock<IPositionRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            //_currentUserFactory = new ICurrentUserFactory();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _personBenefitDependentsCollection = new List<PersonBenefitDependent>()
                {
                    new Domain.HumanResources.Entities.PersonBenefitDependent("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    new Domain.HumanResources.Entities.PersonBenefitDependent("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    new Domain.HumanResources.Entities.PersonBenefitDependent("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                };
            personBenefitDependentsTuple = new Tuple<IEnumerable<PersonBenefitDependent>, int>(_personBenefitDependentsCollection, _personBenefitDependentsCollection.Count);

            _personBenefitDependentsDtoCollection = new List<PersonBenefitDependents>()
                {
                    new PersonBenefitDependents() { Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", DeductionArrangement = new GuidObject2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"), Dependent = new Dtos.DtoProperties.PersonBenefitDependentsDependentDtoProperty() { Person = new GuidObject2("d2253ac7-9931-4560-b42f-1fccd43c952e") } },
                    new PersonBenefitDependents() { Id = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", DeductionArrangement = new GuidObject2("d2253ac7-9931-4560-b42f-1fccd43c952e"), Dependent = new Dtos.DtoProperties.PersonBenefitDependentsDependentDtoProperty() { Person = new GuidObject2("d2253ac7-9931-4560-b42f-1fccd43c952e") } },
                    new PersonBenefitDependents() { Id = "d2253ac7-9931-4560-b42f-1fccd43c952e", DeductionArrangement = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"), Dependent = new Dtos.DtoProperties.PersonBenefitDependentsDependentDtoProperty() { Person = new GuidObject2("d2253ac7-9931-4560-b42f-1fccd43c952e") } }
                };

            personBenefitDependentsDtoTuple = new Tuple<IEnumerable<PersonBenefitDependents>, int>(_personBenefitDependentsDtoCollection, _personBenefitDependentsDtoCollection.Count);

            // Set up current user
            _currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            var permissionView = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewDependents);
            personRole.AddPermission(permissionView);
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            _personBenefitDependentRepositoryMock.Setup(repo => repo.GetPersonBenefitDependentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(personBenefitDependentsTuple);

            _personBenefitDependentRepositoryMock.Setup(repo => repo.GetPersonBenefitDependentByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_personBenefitDependentsCollection.ElementAt(0));

            _personBenefitDependentsService = new PersonBenefitDependentsService(_personBenefitDependentRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _configurationRepoMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _personBenefitDependentsService = null;
            _personBenefitDependentsCollection = null;
            _personBenefitDependentRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PersonBenefitDependentsService_GetPersonBenefitDependentsAsync()
        {
            var results = await _personBenefitDependentsService.GetPersonBenefitDependentsAsync(0, 2, true);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Item1 is IEnumerable<PersonBenefitDependents>);
        }

        [TestMethod]
        public async Task PersonBenefitDependentsService_GetPersonBenefitDependentsAsync_Count()
        {
            var results = await _personBenefitDependentsService.GetPersonBenefitDependentsAsync(0, 2, true);
            Assert.AreEqual(3, results.Item2);
        }

        [TestMethod]
        public async Task PersonBenefitDependentsService_GetPersonBenefitDependentsAsync_Expected()
        {
            var expectedResults = _personBenefitDependentsCollection.FirstOrDefault(c => c.Guid == personBenefitDependentsGuid);
            
            _personBenefitDependentRepositoryMock.Setup(repo => repo.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(expectedResults.DeductionArrangement);

            var actualResult =
                (await _personBenefitDependentsService.GetPersonBenefitDependentsAsync(0, 2, true)).Item1.FirstOrDefault(x => x.Id == personBenefitDependentsGuid);
            
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.DeductionArrangement, actualResult.DeductionArrangement.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonBenefitDependentsService_GetPersonBenefitDependentsByGuidAsync_Empty()
        {
            _personBenefitDependentRepositoryMock.Setup(repo => repo.GetPersonBenefitDependentByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _personBenefitDependentsService.GetPersonBenefitDependentsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonBenefitDependentsService_GetPersonBenefitDependentsByGuidAsync_Null()
        {
            _personBenefitDependentRepositoryMock.Setup(repo => repo.GetPersonBenefitDependentByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _personBenefitDependentsService.GetPersonBenefitDependentsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonBenefitDependentsService_GetPersonBenefitDependentsByGuidAsync_InvalidId()
        {
            _personBenefitDependentRepositoryMock.Setup(repo => repo.GetPersonBenefitDependentByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _personBenefitDependentsService.GetPersonBenefitDependentsByGuidAsync("99");
        }

        [TestMethod]
        public async Task PersonBenefitDependentsService_GetPersonBenefitDependentsByGuidAsync_Expected()
        {
            var expectedResults =
                _personBenefitDependentsCollection.First(c => c.Guid == personBenefitDependentsGuid);

            _personBenefitDependentRepositoryMock.Setup(repo => repo.GetGuidFromIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(expectedResults.DeductionArrangement);

            var actualResult =
                await _personBenefitDependentsService.GetPersonBenefitDependentsByGuidAsync(personBenefitDependentsGuid);
            
            
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.DeductionArrangement, actualResult.DeductionArrangement.Id);

        }

        [TestMethod]
        public async Task PersonBenefitDependentsService_GetPersonBenefitDependentsByGuidAsync_Properties()
        {
            var result =
                await _personBenefitDependentsService.GetPersonBenefitDependentsByGuidAsync(personBenefitDependentsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.DeductionArrangement);

        }
    }
}