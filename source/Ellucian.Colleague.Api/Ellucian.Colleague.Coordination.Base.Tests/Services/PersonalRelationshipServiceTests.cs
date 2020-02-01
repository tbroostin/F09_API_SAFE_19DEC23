// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonalRelationshipServiceTests 
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
        public class PersonalRelationshipTypeService_Get : CurrentUserSetup
        {

            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.RelationType> allRelationTypes;
            private PersonalRelationshipTypeService personalRelationshipService;
            private string relationTypeGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";
           
            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize() {
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;


                allRelationTypes = new TestRelationTypesRepository().GetRelationTypes();
               

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                personalRelationshipService = new PersonalRelationshipTypeService(adapterRegistry, refRepo, currentUserFactory, roleRepo, logger, configRepo);
            }

            [TestCleanup]
            public void Cleanup() {
                refRepo = null;
                //personRepo = null;
                allRelationTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                personalRelationshipService = null;
            }

            [TestMethod]
            public async Task GetPersonalRelationTypeByGuid_ValidRelationTypeGuid()
            {
               Domain.Base.Entities.RelationType thisRelationType = allRelationTypes.Where(m => m.Guid == relationTypeGuid).FirstOrDefault();
               refRepoMock.Setup(repo => repo.GetRelationTypesAsync(true)).ReturnsAsync(allRelationTypes.Where(m => m.Guid == relationTypeGuid));
               Dtos.RelationType relationType = await personalRelationshipService.GetPersonalRelationTypeByGuidAsync(relationTypeGuid);
                Assert.AreEqual(thisRelationType.Guid, relationType.Id);
                Assert.AreEqual(thisRelationType.Code, relationType.Code);
                Assert.AreEqual(null, relationType.Description);
              
            }

            
            [TestMethod]
            public async Task GetPersonalRelationTypes_CountRelationTypes()
            {
                refRepoMock.Setup(repo => repo.GetRelationTypesAsync(false)).ReturnsAsync(allRelationTypes);
                IEnumerable<Ellucian.Colleague.Dtos.RelationType> relationType = await personalRelationshipService.GetPersonalRelationTypesAsync();
                Assert.AreEqual(2, relationType.Count());
            }

            [TestMethod]
            public async Task GetPersonalRelationTypes_CompareRelationTypes()
            {
                refRepoMock.Setup(repo => repo.GetRelationTypesAsync(false)).ReturnsAsync(allRelationTypes);

                IEnumerable<Dtos.RelationType> relationTypes = await personalRelationshipService.GetPersonalRelationTypesAsync();
                Assert.AreEqual(allRelationTypes.ElementAt(0).Guid, relationTypes.ElementAt(0).Id);
                Assert.AreEqual(allRelationTypes.ElementAt(0).Code, relationTypes.ElementAt(0).Code);
                Assert.AreEqual(null, relationTypes.ElementAt(0).Description);
            }
        }

        [TestClass]
        public class PersonalRelationshipStatuses_Get : CurrentUserSetup
        {
            //private Mock<IPersonRepository> personRepoMock;
            //private IPersonRepository personRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.RelationshipStatus> allPersonalRelationshipStatuses;
            private PersonalRelationshipTypeService personalRelationshipService;
            private string personalRelationshipStatusGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {

                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;

                allPersonalRelationshipStatuses = new TestPersonalRelationshipStatusRepository().GetPersonalRelationshipStatuses();


                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                personalRelationshipService = new PersonalRelationshipTypeService(adapterRegistry, refRepo, currentUserFactory, roleRepo, logger, configRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                allPersonalRelationshipStatuses = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                personalRelationshipService = null;
            }

            [TestMethod]
            public async Task GetPersonalRelationshipStatusByGuid_ValidPersonalRelationshipStatusGuid()
            {
                Domain.Base.Entities.RelationshipStatus thisPersonalRelationshipStatus = allPersonalRelationshipStatuses.Where(m => m.Guid == personalRelationshipStatusGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetRelationshipStatusesAsync(true)).ReturnsAsync(allPersonalRelationshipStatuses.Where(m => m.Guid == personalRelationshipStatusGuid));
                Dtos.PersonalRelationshipStatus relationType = await personalRelationshipService.GetPersonalRelationshipStatusByGuidAsync(personalRelationshipStatusGuid);
                Assert.AreEqual(thisPersonalRelationshipStatus.Guid, relationType.Id);
                Assert.AreEqual(thisPersonalRelationshipStatus.Code, relationType.Code);
                Assert.AreEqual(null, relationType.Description);

            }


            [TestMethod]
            public async Task GetPersonalRelationshipStatuses_CountPersonalRelationshipStatuses()
            {
                refRepoMock.Setup(repo => repo.GetRelationshipStatusesAsync(false)).ReturnsAsync(allPersonalRelationshipStatuses);
                IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationshipStatus> personalRelationshipStatus = await personalRelationshipService.GetPersonalRelationshipStatusesAsync();
                Assert.AreEqual(4, personalRelationshipStatus.Count());
            }

            [TestMethod]
            public async Task GetPersonalRelationshipStatuses_ComparePersonalRelationshipStatuses()
            {
                refRepoMock.Setup(repo => repo.GetRelationshipStatusesAsync(false)).ReturnsAsync(allPersonalRelationshipStatuses);

                IEnumerable<Dtos.PersonalRelationshipStatus> personalRelationshipStatuses = await personalRelationshipService.GetPersonalRelationshipStatusesAsync();
                Assert.AreEqual(allPersonalRelationshipStatuses.ElementAt(0).Guid, personalRelationshipStatuses.ElementAt(0).Id);
                Assert.AreEqual(allPersonalRelationshipStatuses.ElementAt(0).Code, personalRelationshipStatuses.ElementAt(0).Code);
                Assert.AreEqual(null, personalRelationshipStatuses.ElementAt(0).Description);
            }
        }
    }

    [TestClass]
    public class PersonalRelationshipsServiceTestsGetV16 : CurrentUserSetup
    {
        private const string personalRelationships2Guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string personalRelationships2Code = "AT";
        private ICollection<Domain.Base.Entities.Relationship> _personalRelationships2Collection;
        private ICollection<Domain.Base.Entities.RelationType> _relationTypesCollection;
        private ICollection<Domain.Base.Entities.RelationshipStatus> _relationStatusesCollection;
        private PersonalRelationshipsService _personalRelationships2Service;

        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IRelationshipRepository> _relationshipRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _relationshipRepositoryMock = new Mock<Domain.Base.Repositories.IRelationshipRepository>();
            _personRepositoryMock = new Mock<Domain.Base.Repositories.IPersonRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            //_currentUserFactory = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _personalRelationships2Collection = new List<Domain.Base.Entities.Relationship>()
                {
                    new Domain.Base.Entities.Relationship("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "d2253ac7-9931-4560-b42f-1fccd43c952e", "AT", false, DateTime.Now, DateTime.Now) { Guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", SubjectPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"},
                    new Domain.Base.Entities.Relationship("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AC", true, DateTime.Now, DateTime.Now) { Guid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", SubjectPersonGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"},
                    new Domain.Base.Entities.Relationship("d2253ac7-9931-4560-b42f-1fccd43c952e", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CU", true, DateTime.Now, DateTime.Now) { Guid = "d2253ac7-9931-4560-b42f-1fccd43c952e", SubjectPersonGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", RelationPersonGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e"}
                };

            _relationTypesCollection = new List<Domain.Base.Entities.RelationType>()
                {
                    new Domain.Base.Entities.RelationType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "1", "AC"),
                    new Domain.Base.Entities.RelationType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "1", "CU"),
                    new Domain.Base.Entities.RelationType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "1", "AT")
                };

            foreach (var type in _relationTypesCollection)
            {
                _referenceRepositoryMock.Setup(repo => repo.GetRelationTypes3GuidAsync(type.Code)).ReturnsAsync(new Tuple<string, string>(type.Guid, type.Guid));
            }
            _relationStatusesCollection = new List<Domain.Base.Entities.RelationshipStatus>()
                {
                    new Domain.Base.Entities.RelationshipStatus("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.RelationshipStatus("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.RelationshipStatus("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var stat in _relationStatusesCollection)
            {
                _referenceRepositoryMock.Setup(repo => repo.GetRelationshipStatusesGuidAsync(stat.Code)).ReturnsAsync(stat.Guid);
            }
            var PersonalRelationshipsTuple =
                new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(_personalRelationships2Collection.Take(4), _personalRelationships2Collection.Count());

            _relationshipRepositoryMock.Setup(repo => repo.GetRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(PersonalRelationshipsTuple);

            _referenceRepositoryMock.Setup(repo => repo.GetRelationTypes3Async(It.IsAny<bool>()))
                .ReturnsAsync(_relationTypesCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetRelationshipStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_relationStatusesCollection);

            // Set up current user
            _currentUserFactory = new GenericUserFactory.PersonalRelationshipUserFactory();

            Ellucian.Colleague.Domain.Entities.Role viewAnyPersonRelationshipRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.RELATIONSHIP");
            viewAnyPersonRelationshipRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewAnyRelationship));
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { viewAnyPersonRelationshipRole });

            _personalRelationships2Service = new PersonalRelationshipsService(_adapterRegistryMock.Object, _referenceRepositoryMock.Object,
                _relationshipRepositoryMock.Object, _personRepositoryMock.Object, _configurationRepoMock.Object,
                _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _personalRelationships2Service = null;
            _personalRelationships2Collection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async()
        {
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4);
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_Count()
        {
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 3);
            Assert.AreEqual(3, results.Item1.Count());
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_With_Filters()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 3, "1", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                "1");
            Assert.AreEqual(3, results.Item1.Count());
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_Properties()
        {
            var result =
                (await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4)).Item1.FirstOrDefault(x => x.Id == personalRelationships2Guid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.SubjectPerson);
            Assert.IsNotNull(result.Related);

        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_Expected()
        {
            var expectedResults = _personalRelationships2Collection.FirstOrDefault(c => c.Guid == personalRelationships2Guid);
            var actualResult =
                (await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4)).Item1.FirstOrDefault(x => x.Id == personalRelationships2Guid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.SubjectPersonGuid, actualResult.SubjectPerson.Id);
            Assert.AreEqual(expectedResults.RelationPersonGuid, actualResult.Related.person.Id);

        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_personFilterKeys_Null()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4, personFilterValue: "1");
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>);
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_personFilterKeys_Exception()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4, personFilterValue: "1");
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>);
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_person_Null()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4, person: "1");
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>);
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_person_Exception()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4, person: "1");
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>);
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_relationshipType_Null()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4, relationshipType: "1");
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>);
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_relationshipType_Exception()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRelationTypes3Async(It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4, relationshipType: "1");
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>);
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_relationshipType_Reference_Empty()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRelationTypes3Async(It.IsAny<bool>()))
                .ReturnsAsync(new List<Domain.Base.Entities.RelationType>()
                {
                    new Domain.Base.Entities.RelationType("guid1", "code1", "descr", "OrgInd", "rel")
                });
            var results = await _personalRelationships2Service.GetPersonalRelationships2Async(0, 4, relationshipType: "1");
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>);
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_PermissionsException()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(),
                It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());

            await _personalRelationships2Service.GetPersonalRelationships2Async(0, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_IntegrationApiException()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(),
                It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

            await _personalRelationships2Service.GetPersonalRelationships2Async(0, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2Async_Exception()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>(),
                It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

            await _personalRelationships2Service.GetPersonalRelationships2Async(0, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2ByGuidAsync_Empty()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonalRelationshipById2Async("")).Throws<ArgumentNullException>();

            await _personalRelationships2Service.GetPersonalRelationships2ByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2ByGuidAsync_Null()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonalRelationshipById2Async(null)).Throws<ArgumentNullException>();

            await _personalRelationships2Service.GetPersonalRelationships2ByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2ByGuidAsync_InvalidId()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonalRelationshipById2Async(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _personalRelationships2Service.GetPersonalRelationships2ByGuidAsync("99");
        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2ByGuidAsync_Expected()
        {
            var expectedResults =
                _personalRelationships2Collection.First(c => c.Guid == personalRelationships2Guid);
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonalRelationshipById2Async(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);
            var actualResult =
                await _personalRelationships2Service.GetPersonalRelationships2ByGuidAsync(personalRelationships2Guid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.SubjectPersonGuid, actualResult.SubjectPerson.Id);
            Assert.AreEqual(expectedResults.RelationPersonGuid, actualResult.Related.person.Id);

        }

        [TestMethod]
        public async Task PersonalRelationshipsService_GetPersonalRelationships2ByGuidAsync_Properties()
        {
            var expectedResults =
                _personalRelationships2Collection.First(c => c.Guid == personalRelationships2Guid);
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonalRelationshipById2Async(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);
            var result =
                await _personalRelationships2Service.GetPersonalRelationships2ByGuidAsync(personalRelationships2Guid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.SubjectPerson);
            Assert.IsNotNull(result.Related);

        }
    }

    [TestClass]
    public class PersonalRelationshipsServiceTests_POST_V16 : GenericUserFactory
    {
        #region DECLARATION

        protected Domain.Entities.Role createPersonRelationShip = new Domain.Entities.Role(1, "UPDATE.RELATIONSHIP");

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IRelationshipRepository> relationshipRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;

        private PersonalRelationshipUserFactory currentUserFactory;

        private PersonalRelationshipsService personalRelationships2Service;

        private Dtos.PersonalRelationships2 personalRelationships2;
        private IEnumerable<Domain.Base.Entities.RelationType> relationTypes;
        private IEnumerable<Domain.Base.Entities.RelationshipStatus> relationshipStatuses;

        private Domain.Base.Entities.Relationship relationship;

        private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            relationshipRepositoryMock = new Mock<IRelationshipRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            currentUserFactory = new PersonalRelationshipUserFactory();

            personalRelationships2Service = new PersonalRelationshipsService(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, relationshipRepositoryMock.Object,
                personRepositoryMock.Object, configurationRepositoryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

            InitializeTestData();

            InitializeMock();
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            referenceDataRepositoryMock = null;
            relationshipRepositoryMock = null;
            personRepositoryMock = null;
            configurationRepositoryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            currentUserFactory = null;
        }

        private void InitializeTestData()
        {
            relationTypes = new List<Domain.Base.Entities.RelationType>()
            {
                new Domain.Base.Entities.RelationType("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "desc", "N", "4", "S"),
                new Domain.Base.Entities.RelationType("3a59eed8-5fe7-4120-b1cf-f23266b9e874", "2", "desc", "Y", "1"),
                new Domain.Base.Entities.RelationType("4a59eed8-5fe7-4120-b1cf-f23266b9e874", "3", "desc", "N", ""),
                new Domain.Base.Entities.RelationType("5a59eed8-5fe7-4120-b1cf-f23266b9e874", "4", "desc", "N", "5"),
                new Domain.Base.Entities.RelationType("6a59eed8-5fe7-4120-b1cf-f23266b9e874", "5", "desc", "N", "3")
            };

            relationshipStatuses = new List<Domain.Base.Entities.RelationshipStatus>()
            {
                new Domain.Base.Entities.RelationshipStatus(guid, "1", "desc")
            };

            personalRelationships2 = new Dtos.PersonalRelationships2()
            {
                Id = guid,
                SubjectPerson = new Dtos.GuidObject2(guid),
                DirectRelationshipType = new Dtos.GuidObject2(guid),
                ReciprocalRelationshipType = new Dtos.GuidObject2("5a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                StartOn = DateTime.Today.AddDays(-10),
                Status = new Dtos.GuidObject2(guid),
                Comment = "comments",
                Related = new Dtos.PersonalRelationshipsRelatedPerson()
                {
                    person = new Dtos.GuidObject2(guid)
                }
            };

            relationship = new Domain.Base.Entities.Relationship(guid, "1", "2", "1", "1", true, DateTime.Today.AddDays(-100), DateTime.Today.AddDays(-1))
            {
                Guid = guid,
                SubjectPersonGuid = guid,
                RelationPersonGuid = guid,
                Comment = "comments",
                Status = "1"
            };
        }

        private void InitializeMock()
        {
            createPersonRelationShip.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdatePersonalRelationship));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRelationShip });
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>("2"));
            referenceDataRepositoryMock.Setup(r => r.GetRelationTypes3Async(false)).ReturnsAsync(relationTypes);
            referenceDataRepositoryMock.Setup(r => r.GetRelationshipStatusesAsync(false)).ReturnsAsync(relationshipStatuses);
            relationshipRepositoryMock.Setup(r => r.UpdatePersonalRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ReturnsAsync(relationship);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_CreatePersonalRelationships2Async_Dto_Null()
        {
            await personalRelationships2Service.CreatePersonalRelationships2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_CreatePersonalRelationships2Async_Dto_Id_Null()
        {
            await personalRelationships2Service.CreatePersonalRelationships2Async(new Dtos.PersonalRelationships2() { });
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PerRelshpService_CreatePersonalRelationships2Async_PermissionsException()
        {
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { });
            await personalRelationships2Service.CreatePersonalRelationships2Async(new Dtos.PersonalRelationships2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_SubjectPerson_Empty()
        {
            personalRelationships2.SubjectPerson = null;
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_SubjectPerson_Id_Empty()
        {
            personalRelationships2.SubjectPerson.Id = null;
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_SubjectPerson_Empty()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Related_Empty()
        {
            personalRelationships2.Related = null;
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Related_Person_Empty()
        {
            personalRelationships2.Related.person = null;
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Related_PersonId_Empty()
        {
            personalRelationships2.Related.person.Id = null;
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_Related_PersonId()
        {
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_SubjectPerson_And_RelatedPerson_AreSame()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1"));
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_DirectRelationshipType_Empty()
        {
            personalRelationships2.DirectRelationshipType = null;
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_DirectRelationshipTypeId_Empty()
        {
            personalRelationships2.DirectRelationshipType.Id = null;
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_DirectRelationshipType()
        {
            personalRelationships2.DirectRelationshipType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_DirectRelationshipType_OrgIndicator()
        {
            personalRelationships2.DirectRelationshipType.Id = "3a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_ReciprocalRelationshipTypeId_Empty()
        {
            personalRelationships2.ReciprocalRelationshipType.Id = null;
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_ReciprocalRelationshipTypeId()
        {
            personalRelationships2.ReciprocalRelationshipType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_DirectRelation_Without_InverseRelation()
        {
            personalRelationships2.DirectRelationshipType.Id = "4a59eed8-5fe7-4120-b1cf-f23266b9e874";
            personalRelationships2.ReciprocalRelationshipType.Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_InverseRelation_DoesNotMatch_ReciprocalRelation()
        {
            personalRelationships2.DirectRelationshipType.Id = "6a59eed8-5fe7-4120-b1cf-f23266b9e874";
            personalRelationships2.ReciprocalRelationshipType.Id = "5a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_StartOn_As_FutureDate()
        {
            personalRelationships2.ReciprocalRelationshipType = null;
            personalRelationships2.StartOn = DateTime.Today.AddDays(10);
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_EndOn_As_FutureDate()
        {
            personalRelationships2.EndOn = DateTime.Today.AddDays(10);
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_StartOn_GreaterThan_EndOn()
        {
            personalRelationships2.EndOn = DateTime.Today.AddDays(-15);
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_StatusId_Empty()
        {
            personalRelationships2.Status = new Dtos.GuidObject2(null);
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_RelationStatuses_Are_Empty()
        {
            referenceDataRepositoryMock.Setup(r => r.GetRelationshipStatusesAsync(false)).ReturnsAsync(null);
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_StatusId()
        {
            personalRelationships2.Status.Id = "5a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Create_RepositoryException()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonalRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new RepositoryException());
            await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        public async Task PerRelshpService_CreatePersonalRelationshipsAsync()
        {
            personalRelationships2.Id = Guid.Empty.ToString();
            var result = await personalRelationships2Service.CreatePersonalRelationships2Async(personalRelationships2);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, guid);
        }
    }

    [TestClass]
    public class PersonalRelationshipsServiceTests_PUT_V16 : GenericUserFactory
    {
        #region DECLARATION

        protected Domain.Entities.Role createPersonRelationShip = new Domain.Entities.Role(1, "UPDATE.RELATIONSHIP");

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IRelationshipRepository> relationshipRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;

        private PersonalRelationshipUserFactory currentUserFactory;

        private PersonalRelationshipsService personalRelationships2Service;

        private Dtos.PersonalRelationships2 personalRelationships2;
        private IEnumerable<Domain.Base.Entities.RelationType> relationTypes;
        private IEnumerable<Domain.Base.Entities.RelationshipStatus> relationshipStatuses;

        private Domain.Base.Entities.Relationship relationship;

        private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            relationshipRepositoryMock = new Mock<IRelationshipRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            currentUserFactory = new PersonalRelationshipUserFactory();

            personalRelationships2Service = new PersonalRelationshipsService(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, relationshipRepositoryMock.Object,
                personRepositoryMock.Object, configurationRepositoryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

            InitializeTestData();

            InitializeMock();
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            referenceDataRepositoryMock = null;
            relationshipRepositoryMock = null;
            personRepositoryMock = null;
            configurationRepositoryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            currentUserFactory = null;
        }

        private void InitializeTestData()
        {
            relationTypes = new List<Domain.Base.Entities.RelationType>()
            {
                new Domain.Base.Entities.RelationType("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "desc", "N", "4", "S"),
                new Domain.Base.Entities.RelationType("3a59eed8-5fe7-4120-b1cf-f23266b9e874", "2", "desc", "Y", "1"),
                new Domain.Base.Entities.RelationType("4a59eed8-5fe7-4120-b1cf-f23266b9e874", "3", "desc", "N", ""),
                new Domain.Base.Entities.RelationType("5a59eed8-5fe7-4120-b1cf-f23266b9e874", "4", "desc", "N", "5"),
                new Domain.Base.Entities.RelationType("6a59eed8-5fe7-4120-b1cf-f23266b9e874", "5", "desc", "N", "3")
            };

            relationshipStatuses = new List<Domain.Base.Entities.RelationshipStatus>()
            {
                new Domain.Base.Entities.RelationshipStatus(guid, "1", "desc")
            };

            personalRelationships2 = new Dtos.PersonalRelationships2()
            {
                Id = guid,
                SubjectPerson = new Dtos.GuidObject2(guid),
                DirectRelationshipType = new Dtos.GuidObject2(guid),
                ReciprocalRelationshipType = new Dtos.GuidObject2("5a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                StartOn = DateTime.Today.AddDays(-10),
                Status = new Dtos.GuidObject2(guid),
                Comment = "comments",
                Related = new Dtos.PersonalRelationshipsRelatedPerson()
                {
                    person = new Dtos.GuidObject2(guid)
                }
            };

            relationship = new Domain.Base.Entities.Relationship(guid, "1", "2", "1", "1", true, DateTime.Today.AddDays(-100), DateTime.Today.AddDays(-1))
            {
                Guid = guid,
                SubjectPersonGuid = guid,
                RelationPersonGuid = guid,
                Comment = "comments",
                Status = "1"
            };
        }

        private void InitializeMock()
        {
            createPersonRelationShip.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdatePersonalRelationship));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRelationShip });
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>("2"));
            referenceDataRepositoryMock.Setup(r => r.GetRelationTypes3Async(false)).ReturnsAsync(relationTypes);
            referenceDataRepositoryMock.Setup(r => r.GetRelationshipStatusesAsync(false)).ReturnsAsync(relationshipStatuses);
            relationshipRepositoryMock.Setup(r => r.UpdatePersonalRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ReturnsAsync(relationship);
            relationshipRepositoryMock.Setup(r => r.GetPersonalRelationshipsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_UpdatePersonalRelationships2Async_Dto_Null()
        {
            await personalRelationships2Service.UpdatePersonalRelationships2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_UpdatePersonalRelationships2Async_Dto_Id_Null()
        {
            await personalRelationships2Service.UpdatePersonalRelationships2Async(new Dtos.PersonalRelationships2() { });
        }

        [TestMethod]
        public async Task PerRelshpService_Create_With_UpdatePersonalRelationships2Async()
        {
            relationshipRepositoryMock.Setup(r => r.GetPersonalRelationshipsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            var result = await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PerRelshpService_UpdatePersonalRelationships2Async_PermissionsException()
        {
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { });
            await personalRelationships2Service.UpdatePersonalRelationships2Async(new Dtos.PersonalRelationships2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_SubjectPerson_Empty()
        {
            personalRelationships2.SubjectPerson = null;
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_SubjectPerson_Id_Empty()
        {
            personalRelationships2.SubjectPerson.Id = null;
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_SubjectPerson_Empty()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_Related_Empty()
        {
            personalRelationships2.Related = null;
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_Related_Person_Empty()
        {
            personalRelationships2.Related.person = null;
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_Related_PersonId_Empty()
        {
            personalRelationships2.Related.person.Id = null;
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_Related_PersonId()
        {
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_SubjectPerson_And_RelatedPerson_AreSame()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1"));
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_DirectRelationshipType_Empty()
        {
            personalRelationships2.DirectRelationshipType = null;
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_DirectRelationshipTypeId_Empty()
        {
            personalRelationships2.DirectRelationshipType.Id = null;
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_DirectRelationshipType()
        {
            personalRelationships2.DirectRelationshipType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_DirectRelationshipType_OrgIndicator()
        {
            personalRelationships2.DirectRelationshipType.Id = "3a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_ReciprocalRelationshipTypeId_Empty()
        {
            personalRelationships2.ReciprocalRelationshipType.Id = null;
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_ReciprocalRelationshipTypeId()
        {
            personalRelationships2.ReciprocalRelationshipType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_DirectRelation_Without_InverseRelation()
        {
            personalRelationships2.DirectRelationshipType.Id = "4a59eed8-5fe7-4120-b1cf-f23266b9e874";
            personalRelationships2.ReciprocalRelationshipType.Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_InverseRelation_DoesNotMatch_ReciprocalRelation()
        {
            personalRelationships2.DirectRelationshipType.Id = "6a59eed8-5fe7-4120-b1cf-f23266b9e874";
            personalRelationships2.ReciprocalRelationshipType.Id = "5a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_StartOn_As_FutureDate()
        {
            personalRelationships2.ReciprocalRelationshipType = null;
            personalRelationships2.StartOn = DateTime.Today.AddDays(10);
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_EndOn_As_FutureDate()
        {
            personalRelationships2.EndOn = DateTime.Today.AddDays(10);
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_StartOn_GreaterThan_EndOn()
        {
            personalRelationships2.EndOn = DateTime.Today.AddDays(-15);
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_RelationStatuses_Are_Empty()
        {
            referenceDataRepositoryMock.Setup(r => r.GetRelationshipStatusesAsync(false)).ReturnsAsync(null);
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_StatusId()
        {
            personalRelationships2.Status.Id = "5a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        public async Task PerRelshpService_UpdatePersonalRelationshipsAsync()
        {
            var result = await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PerRelshpService_UpdatePersonalRelationshipsAsync_RepositoryException()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonalRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new RepositoryException());
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PerRelshpService_UpdatePersonalRelationshipsAsync_KeyNotFoundException()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonalRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new KeyNotFoundException());
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task PerRelshpService_UpdatePersonalRelationshipsAsync_ArgumentException()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonalRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new ArgumentException());
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_UpdatePersonalRelationshipsAsync_Exception()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonalRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new Exception());
            await personalRelationships2Service.UpdatePersonalRelationships2Async(personalRelationships2);
        }
    }

    [TestClass]
    public class PersonalRelationshipsServiceTests_DELETE_V16 : GenericUserFactory
    {
        #region DECLARATION

        protected Domain.Entities.Role deletePersonRelationShip = new Domain.Entities.Role(1, "DELETE.RELATIONSHIP");

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IRelationshipRepository> relationshipRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;

        private PersonalRelationshipUserFactory currentUserFactory;

        private PersonalRelationshipsService personalRelationships2Service;

        private Domain.Base.Entities.Relationship relationship;

        private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            relationshipRepositoryMock = new Mock<IRelationshipRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            currentUserFactory = new PersonalRelationshipUserFactory();

            personalRelationships2Service = new PersonalRelationshipsService(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, relationshipRepositoryMock.Object,
                personRepositoryMock.Object, configurationRepositoryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

            InitializeTestData();

            InitializeMock();
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            referenceDataRepositoryMock = null;
            relationshipRepositoryMock = null;
            personRepositoryMock = null;
            configurationRepositoryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            currentUserFactory = null;
        }

        private void InitializeTestData()
        {

        }

        private void InitializeMock()
        {
            deletePersonRelationShip.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.DeletePersonalRelationship));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { deletePersonRelationShip });
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PerRelshpService_DeletePersonalRelationshipsAsync_Guid_Null()
        {
            await personalRelationships2Service.DeletePersonalRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PerRelshpService_DeletePersonalRelationshipsAsync_PermissionsException()
        {
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { });
            await personalRelationships2Service.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PerRelshpService_DeletePersonalRelationshipsAsync_Guid_NotFound()
        {
            relationshipRepositoryMock.Setup(r => r.GetPersonalRelationshipById2Async(It.IsAny<string>())).ReturnsAsync(null);
            await personalRelationships2Service.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PerRelshpService_DeletePersonalRelationshipsAsync_RepositoryException()
        {
            relationship = new Domain.Base.Entities.Relationship(guid, "1", "1", "1", true, null, null) { };
            relationshipRepositoryMock.Setup(r => r.GetPersonalRelationshipById2Async(It.IsAny<string>())).ReturnsAsync(relationship);
            relationshipRepositoryMock.Setup(r => r.DeletePersonRelationshipAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            await personalRelationships2Service.DeletePersonalRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_DeletePersonalRelationshipsAsync_Exception()
        {
            relationship = new Domain.Base.Entities.Relationship(guid, "1", "1", "1", true, null, null) { };
            relationshipRepositoryMock.Setup(r => r.GetPersonalRelationshipById2Async(It.IsAny<string>())).ReturnsAsync(relationship);
            relationshipRepositoryMock.Setup(r => r.DeletePersonRelationshipAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await personalRelationships2Service.DeletePersonalRelationshipsAsync(guid);
        }
    }
}
