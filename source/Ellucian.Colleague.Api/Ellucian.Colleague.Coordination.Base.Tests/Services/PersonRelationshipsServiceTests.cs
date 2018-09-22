//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonRelationshipsServiceTests : CurrentUserSetup
    {
        private const string personRelationshipsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string personRelationshipsCode = "AT";
        private ICollection<Domain.Base.Entities.Relationship> _personRelationshipsCollection;
        private ICollection<Domain.Base.Entities.RelationType> _relationTypesCollection;
        private ICollection<Domain.Base.Entities.RelationshipStatus> _relationStatusesCollection;
        private PersonRelationshipsService _personRelationshipsService;

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


            _personRelationshipsCollection = new List<Domain.Base.Entities.Relationship>()
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

            _relationStatusesCollection = new List<Domain.Base.Entities.RelationshipStatus>()
                {
                    new Domain.Base.Entities.RelationshipStatus("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.RelationshipStatus("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.RelationshipStatus("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            var PersonRelationshipsTuple =
                new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(_personRelationshipsCollection.Take(4), _personRelationshipsCollection.Count());

            _relationshipRepositoryMock.Setup(repo => repo.GetRelationships2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(PersonRelationshipsTuple);

            _referenceRepositoryMock.Setup(repo => repo.GetRelationTypes3Async(It.IsAny<bool>()))
                .ReturnsAsync(_relationTypesCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetRelationshipStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_relationStatusesCollection);

            // Set up current user
            _currentUserFactory = new GenericUserFactory.PersonRelationshipUserFactory();

            Ellucian.Colleague.Domain.Entities.Role viewAnyPersonRelationshipRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.PERSON.RELATIONSHIPS");
            viewAnyPersonRelationshipRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewAnyPersonRelationship));
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { viewAnyPersonRelationshipRole });

            _personRelationshipsService = new PersonRelationshipsService(_adapterRegistryMock.Object, _referenceRepositoryMock.Object,
                _relationshipRepositoryMock.Object, _personRepositoryMock.Object, _configurationRepoMock.Object,
                _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _personRelationshipsService = null;
            _personRelationshipsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PersonRelationshipsService_GetPersonRelationshipsAsync()
        {
            var results = await _personRelationshipsService.GetPersonRelationshipsAsync(0, 4);
            Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonRelationships>, int>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PersonRelationshipsService_GetPersonRelationshipsAsync_Count()
        {
            var results = await _personRelationshipsService.GetPersonRelationshipsAsync(0, 3);
            Assert.AreEqual(3, results.Item1.Count());
        }

        [TestMethod]
        public async Task PersonRelationshipsService_GetPersonRelationshipsAsync_Properties()
        {
            var result =
                (await _personRelationshipsService.GetPersonRelationshipsAsync(0, 4)).Item1.FirstOrDefault(x => x.Id == personRelationshipsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.SubjectPerson);
            Assert.IsNotNull(result.Related);

        }

        [TestMethod]
        public async Task PersonRelationshipsService_GetPersonRelationshipsAsync_Expected()
        {
            var expectedResults = _personRelationshipsCollection.FirstOrDefault(c => c.Guid == personRelationshipsGuid);
            var actualResult =
                (await _personRelationshipsService.GetPersonRelationshipsAsync(0, 4)).Item1.FirstOrDefault(x => x.Id == personRelationshipsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.SubjectPersonGuid, actualResult.SubjectPerson.Id);
            Assert.AreEqual(expectedResults.RelationPersonGuid, actualResult.Related.person.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PersonRelationshipsService_GetPersonRelationshipsByGuidAsync_Empty()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonRelationshipById2Async("")).Throws<ArgumentNullException>();

            await _personRelationshipsService.GetPersonRelationshipsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PersonRelationshipsService_GetPersonRelationshipsByGuidAsync_Null()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonRelationshipById2Async(null)).Throws<ArgumentNullException>();

            await _personRelationshipsService.GetPersonRelationshipsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonRelationshipsService_GetPersonRelationshipsByGuidAsync_InvalidId()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonRelationshipById2Async(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _personRelationshipsService.GetPersonRelationshipsByGuidAsync("99");
        }

        [TestMethod]
        public async Task PersonRelationshipsService_GetPersonRelationshipsByGuidAsync_Expected()
        {
            var expectedResults =
                _personRelationshipsCollection.First(c => c.Guid == personRelationshipsGuid);
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonRelationshipById2Async(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);
            var actualResult =
                await _personRelationshipsService.GetPersonRelationshipsByGuidAsync(personRelationshipsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.SubjectPersonGuid, actualResult.SubjectPerson.Id);
            Assert.AreEqual(expectedResults.RelationPersonGuid, actualResult.Related.person.Id);

        }

        [TestMethod]
        public async Task PersonRelationshipsService_GetPersonRelationshipsByGuidAsync_Properties()
        {
            var expectedResults =
                _personRelationshipsCollection.First(c => c.Guid == personRelationshipsGuid);
            _relationshipRepositoryMock.Setup(repo => repo.GetPersonRelationshipById2Async(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);
            var result =
                await _personRelationshipsService.GetPersonRelationshipsByGuidAsync(personRelationshipsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.SubjectPerson);
            Assert.IsNotNull(result.Related);

        }
    }

    [TestClass]
    public class PersonRelationshipsServiceTests_POST_V13 : GenericUserFactory
    {
        #region DECLARATION

        protected Domain.Entities.Role createPersonRelationShip = new Domain.Entities.Role(1, "UPDATE.PERSON.RELATIONSHIPS");

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IRelationshipRepository> relationshipRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;

        private PersonRelationshipUserFactory currentUserFactory;

        private PersonRelationshipsService personRelationshipsService;

        private Dtos.PersonRelationships personRelationships;
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

            currentUserFactory = new PersonRelationshipUserFactory();

            personRelationshipsService = new PersonRelationshipsService(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, relationshipRepositoryMock.Object,
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

            personRelationships = new Dtos.PersonRelationships()
            {
                Id = guid,
                SubjectPerson = new Dtos.GuidObject2(guid),
                DirectRelationshipType = new Dtos.GuidObject2(guid),
                ReciprocalRelationshipType = new Dtos.GuidObject2("5a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                StartOn = DateTime.Today.AddDays(-10),
                Status = new Dtos.GuidObject2(guid),
                Comment = "comments",
                Related = new Dtos.PersonRelationshipsRelatedPerson()
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
            createPersonRelationShip.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdatePersonRelationship));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRelationShip });
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>("2"));
            referenceDataRepositoryMock.Setup(r => r.GetRelationTypes3Async(false)).ReturnsAsync(relationTypes);
            referenceDataRepositoryMock.Setup(r => r.GetRelationshipStatusesAsync(false)).ReturnsAsync(relationshipStatuses);
            relationshipRepositoryMock.Setup(r => r.CreatePersonRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ReturnsAsync(relationship);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PerRelshpService_CreatePersonRelationshipsAsync_Dto_Null()
        {
            await personRelationshipsService.CreatePersonRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PerRelshpService_CreatePersonRelationshipsAsync_Dto_Id_Null()
        {
            await personRelationshipsService.CreatePersonRelationshipsAsync(new Dtos.PersonRelationships() { });
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PerRelshpService_CreatePersonRelationshipsAsync_PermissionsException()
        {
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { });
            await personRelationshipsService.CreatePersonRelationshipsAsync(new Dtos.PersonRelationships() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_SubjectPerson_Empty()
        {
            personRelationships.SubjectPerson = null;
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_SubjectPerson_Id_Empty()
        {
            personRelationships.SubjectPerson.Id = null;
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_SubjectPerson_Empty()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Related_Empty()
        {
            personRelationships.Related = null;
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Related_Person_Empty()
        {
            personRelationships.Related.person = null;
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Related_PersonId_Empty()
        {
            personRelationships.Related.person.Id = null;
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_Related_PersonId()
        {
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_SubjectPerson_And_RelatedPerson_AreSame()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1"));
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_DirectRelationshipType_Empty()
        {
            personRelationships.DirectRelationshipType = null;
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_DirectRelationshipTypeId_Empty()
        {
            personRelationships.DirectRelationshipType.Id = null;
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_DirectRelationshipType()
        {
            personRelationships.DirectRelationshipType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_DirectRelationshipType_OrgIndicator()
        {
            personRelationships.DirectRelationshipType.Id = "3a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_ReciprocalRelationshipTypeId_Empty()
        {
            personRelationships.ReciprocalRelationshipType.Id = null;
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_ReciprocalRelationshipTypeId()
        {
            personRelationships.ReciprocalRelationshipType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_DirectRelation_Without_InverseRelation()
        {
            personRelationships.DirectRelationshipType.Id = "4a59eed8-5fe7-4120-b1cf-f23266b9e874";
            personRelationships.ReciprocalRelationshipType.Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_InverseRelation_DoesNotMatch_ReciprocalRelation()
        {
            personRelationships.DirectRelationshipType.Id = "6a59eed8-5fe7-4120-b1cf-f23266b9e874";
            personRelationships.ReciprocalRelationshipType.Id = "5a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_StartOn_As_FutureDate()
        {
            personRelationships.ReciprocalRelationshipType = null;
            personRelationships.StartOn = DateTime.Today.AddDays(10);
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_EndOn_As_FutureDate()
        {
            personRelationships.EndOn = DateTime.Today.AddDays(10);
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_StartOn_GreaterThan_EndOn()
        {
            personRelationships.EndOn = DateTime.Today.AddDays(-15);
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_StatusId_Empty()
        {
            personRelationships.Status = new Dtos.GuidObject2(null);
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_RelationStatuses_Are_Empty()
        {
            referenceDataRepositoryMock.Setup(r => r.GetRelationshipStatusesAsync(false)).ReturnsAsync(null);
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Create_DtoToEntity_Invalid_StatusId()
        {
            personRelationships.Status.Id = "5a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PerRelshpService_Create_RepositoryException()
        {
            relationshipRepositoryMock.Setup(r => r.CreatePersonRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new RepositoryException());
            await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        public async Task PerRelshpService_CreatePersonRelationshipsAsync()
        {
            var result = await personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, guid);
        }
    }

    [TestClass]
    public class PersonRelationshipsServiceTests_PUT_V13 : GenericUserFactory
    {
        #region DECLARATION

        protected Domain.Entities.Role createPersonRelationShip = new Domain.Entities.Role(1, "UPDATE.PERSON.RELATIONSHIPS");

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IRelationshipRepository> relationshipRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;

        private PersonRelationshipUserFactory currentUserFactory;

        private PersonRelationshipsService personRelationshipsService;

        private Dtos.PersonRelationships personRelationships;
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

            currentUserFactory = new PersonRelationshipUserFactory();

            personRelationshipsService = new PersonRelationshipsService(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, relationshipRepositoryMock.Object,
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

            personRelationships = new Dtos.PersonRelationships()
            {
                Id = guid,
                SubjectPerson = new Dtos.GuidObject2(guid),
                DirectRelationshipType = new Dtos.GuidObject2(guid),
                ReciprocalRelationshipType = new Dtos.GuidObject2("5a59eed8-5fe7-4120-b1cf-f23266b9e874"),
                StartOn = DateTime.Today.AddDays(-10),
                Status = new Dtos.GuidObject2(guid),
                Comment = "comments",
                Related = new Dtos.PersonRelationshipsRelatedPerson()
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
            createPersonRelationShip.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdatePersonRelationship));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRelationShip });
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>("2"));
            referenceDataRepositoryMock.Setup(r => r.GetRelationTypes3Async(false)).ReturnsAsync(relationTypes);
            referenceDataRepositoryMock.Setup(r => r.GetRelationshipStatusesAsync(false)).ReturnsAsync(relationshipStatuses);
            relationshipRepositoryMock.Setup(r => r.CreatePersonRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ReturnsAsync(relationship);
            relationshipRepositoryMock.Setup(r => r.UpdatePersonRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ReturnsAsync(relationship);
            relationshipRepositoryMock.Setup(r => r.GetPersonRelationshipsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PerRelshpService_UpdatePersonRelationshipsAsync_Dto_Null()
        {
            await personRelationshipsService.UpdatePersonRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PerRelshpService_UpdatePersonRelationshipsAsync_Dto_Id_Null()
        {
            await personRelationshipsService.UpdatePersonRelationshipsAsync(new Dtos.PersonRelationships() { });
        }

        [TestMethod]
        public async Task PerRelshpService_Create_With_UpdatePersonRelationshipsAsync()
        {
            relationshipRepositoryMock.Setup(r => r.GetPersonRelationshipsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            var result = await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PerRelshpService_UpdatePersonRelationshipsAsync_PermissionsException()
        {
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { });
            await personRelationshipsService.UpdatePersonRelationshipsAsync(new Dtos.PersonRelationships() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_SubjectPerson_Empty()
        {
            personRelationships.SubjectPerson = null;
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_SubjectPerson_Id_Empty()
        {
            personRelationships.SubjectPerson.Id = null;
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_SubjectPerson_Empty()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Related_Empty()
        {
            personRelationships.Related = null;
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Related_Person_Empty()
        {
            personRelationships.Related.person = null;
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Related_PersonId_Empty()
        {
            personRelationships.Related.person.Id = null;
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_Related_PersonId()
        {
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_SubjectPerson_And_RelatedPerson_AreSame()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1"));
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_DirectRelationshipType_Empty()
        {
            personRelationships.DirectRelationshipType = null;
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_DirectRelationshipTypeId_Empty()
        {
            personRelationships.DirectRelationshipType.Id = null;
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_DirectRelationshipType()
        {
            personRelationships.DirectRelationshipType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_DirectRelationshipType_OrgIndicator()
        {
            personRelationships.DirectRelationshipType.Id = "3a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_ReciprocalRelationshipTypeId_Empty()
        {
            personRelationships.ReciprocalRelationshipType.Id = null;
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_ReciprocalRelationshipTypeId()
        {
            personRelationships.ReciprocalRelationshipType.Id = "2a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_DirectRelation_Without_InverseRelation()
        {
            personRelationships.DirectRelationshipType.Id = "4a59eed8-5fe7-4120-b1cf-f23266b9e874";
            personRelationships.ReciprocalRelationshipType.Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_InverseRelation_DoesNotMatch_ReciprocalRelation()
        {
            personRelationships.DirectRelationshipType.Id = "6a59eed8-5fe7-4120-b1cf-f23266b9e874";
            personRelationships.ReciprocalRelationshipType.Id = "5a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_StartOn_As_FutureDate()
        {
            personRelationships.ReciprocalRelationshipType = null;
            personRelationships.StartOn = DateTime.Today.AddDays(10);
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_EndOn_As_FutureDate()
        {
            personRelationships.EndOn = DateTime.Today.AddDays(10);
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_StartOn_GreaterThan_EndOn()
        {
            personRelationships.EndOn = DateTime.Today.AddDays(-15);
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_StatusId_Empty()
        {
            personRelationships.Status = new Dtos.GuidObject2(null);
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_RelationStatuses_Are_Empty()
        {
            referenceDataRepositoryMock.Setup(r => r.GetRelationshipStatusesAsync(false)).ReturnsAsync(null);
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_Update_DtoToEntity_Invalid_StatusId()
        {
            personRelationships.Status.Id = "5a59eed8-5fe7-4120-b1cf-f23266b9e874";
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        public async Task PerRelshpService_UpdatePersonRelationshipsAsync()
        {
            var result = await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PerRelshpService_UpdatePersonRelationshipsAsync_RepositoryException()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new RepositoryException());
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PerRelshpService_UpdatePersonRelationshipsAsync_KeyNotFoundException()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new KeyNotFoundException());
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task PerRelshpService_UpdatePersonRelationshipsAsync_ArgumentException()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new ArgumentException());
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_UpdatePersonRelationshipsAsync_Exception()
        {
            relationshipRepositoryMock.Setup(r => r.UpdatePersonRelationshipsAsync(It.IsAny<Domain.Base.Entities.Relationship>())).ThrowsAsync(new Exception());
            await personRelationshipsService.UpdatePersonRelationshipsAsync(personRelationships);
        }
    }

    [TestClass]
    public class PersonRelationshipsServiceTests_DELETE_V13 : GenericUserFactory
    {
        #region DECLARATION

        protected Domain.Entities.Role deletePersonRelationShip = new Domain.Entities.Role(1, "DELETE.PERSON.RELATIONSHIPS");

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IRelationshipRepository> relationshipRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;

        private PersonRelationshipUserFactory currentUserFactory;

        private PersonRelationshipsService personRelationshipsService;

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

            currentUserFactory = new PersonRelationshipUserFactory();

            personRelationshipsService = new PersonRelationshipsService(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, relationshipRepositoryMock.Object,
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
            deletePersonRelationShip.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.DeletePersonRelationship));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { deletePersonRelationShip });
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PerRelshpService_DeletePersonRelationshipsAsync_Guid_Null()
        {
            await personRelationshipsService.DeletePersonRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PerRelshpService_DeletePersonRelationshipsAsync_PermissionsException()
        {
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { });
            await personRelationshipsService.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PerRelshpService_DeletePersonRelationshipsAsync_Guid_NotFound()
        {
            relationshipRepositoryMock.Setup(r => r.GetPersonRelationshipById2Async(It.IsAny<string>())).ReturnsAsync(null);
            await personRelationshipsService.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PerRelshpService_DeletePersonRelationshipsAsync_RepositoryException()
        {
            relationship = new Domain.Base.Entities.Relationship(guid, "1", "1", "1", true, null, null) { };
            relationshipRepositoryMock.Setup(r => r.GetPersonRelationshipById2Async(It.IsAny<string>())).ReturnsAsync(relationship);
            relationshipRepositoryMock.Setup(r => r.DeletePersonRelationshipAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            await personRelationshipsService.DeletePersonRelationshipsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PerRelshpService_DeletePersonRelationshipsAsync_Exception()
        {
            relationship = new Domain.Base.Entities.Relationship(guid, "1", "1", "1", true, null, null) { };
            relationshipRepositoryMock.Setup(r => r.GetPersonRelationshipById2Async(It.IsAny<string>())).ReturnsAsync(relationship);
            relationshipRepositoryMock.Setup(r => r.DeletePersonRelationshipAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await personRelationshipsService.DeletePersonRelationshipsAsync(guid);
        }
    }
}