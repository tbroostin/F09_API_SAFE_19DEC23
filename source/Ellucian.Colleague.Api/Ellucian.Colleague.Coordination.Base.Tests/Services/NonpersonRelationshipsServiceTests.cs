//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
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
    public class NonpersonRelationshipsServiceTests: GenericUserFactory
    {
        private const string nonpersonRelationshipsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string nonpersonRelationshipsCode = "AT";
        private IEnumerable<Domain.Base.Entities.Relationship> _nonpersonRelationshipsCollection;
        private ICollection<Domain.Base.Entities.RelationshipStatus> _relationshipStatusCollection;
        private IEnumerable<Domain.Base.Entities.RelationType> _relationType;
        private Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int> _nonpersonRelationshipsTuple;
        private NonPersonRelationshipsService _nonpersonRelationshipsService;
        Domain.Entities.Role viewAnyPersonRelationshipRole;

        private Mock<IRelationshipRepository> _relationshipRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public void Initialize()
        {
            _relationshipRepositoryMock = new Mock<IRelationshipRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();

            _currentUserFactory = new GenericUserFactory.PersonalRelationshipUserFactory();

            viewAnyPersonRelationshipRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.NONPERSON.RELATIONSHIPS");
            viewAnyPersonRelationshipRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewAnyNonPersonRelationship));
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { viewAnyPersonRelationshipRole });

            BuildData();

            BuildMock();

            _nonpersonRelationshipsService = new NonPersonRelationshipsService(_adapterRegistryMock.Object, _referenceRepositoryMock.Object, _relationshipRepositoryMock.Object,
                _personRepositoryMock.Object, _configurationRepoMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
        }

        private void BuildMock()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_nonpersonRelationshipsTuple);
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipByIdAsync(It.IsAny<string>())).ReturnsAsync(_nonpersonRelationshipsCollection.First());
            _referenceRepositoryMock.Setup(repo => repo.GetRelationshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(_relationshipStatusCollection);
            _referenceRepositoryMock.Setup(repo => repo.GetRelationTypesAsync(It.IsAny<bool>())).ReturnsAsync(_relationType);
        }

        private void BuildData()
        {
            _nonpersonRelationshipsCollection = new List<Domain.Base.Entities.Relationship>()
            {
                new Domain.Base.Entities.Relationship("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc","1", "Emp", true, DateTime.Today, DateTime.Today.AddDays(30))
                {
                    Comment = "Coment 1",
                    RelationPersonGuid = "862feace-b876-4ba2-ada7-2a23764e0719",
                    RelationPersonInstFlag = true,
                    Status = "X",
                    SubjectPersonGuid = "781b14fb-cac8-4519-9e17-032c8c86a5b5"
                },
                new Domain.Base.Entities.Relationship("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc","2", "Boss", true, DateTime.Today, DateTime.Today.AddDays(30))
                {
                    Comment = "Coment 2",
                    RelationPersonGuid = "962feace-b876-4ba2-ada7-2a23764e0710",
                    RelationPersonOrgFlag = true,
                    Status = "Y",
                    SubjectPersonGuid = "881b14fb-cac8-4519-9e17-032c8c86a5b6"
                }
            };
            _nonpersonRelationshipsTuple = new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(_nonpersonRelationshipsCollection, _nonpersonRelationshipsCollection.Count());

            _relationshipStatusCollection = new List<Domain.Base.Entities.RelationshipStatus>()
            {
                new Domain.Base.Entities.RelationshipStatus("45b9a9e1-8bb3-4a83-bf94-b53df69a9f45", "X", "Desc 1"),
                new Domain.Base.Entities.RelationshipStatus("0352d647-9b19-41ba-b06c-92e310ce4733", "Y", "Desc 2")
            };

            _relationType = new List<Domain.Base.Entities.RelationType>()
            {
                new Domain.Base.Entities.RelationType("97b3c0c2-6779-43e7-ad48-8a92b2cb7d9c", "Emp", "Desc 1", "", "Boss"),
                new Domain.Base.Entities.RelationType("5a622f42-bb93-4e48-bc57-970f37e650aa", "Boss", "Desc 2", "", "Emp")
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _nonpersonRelationshipsService = null;
            _nonpersonRelationshipsCollection = null;
            _personRepositoryMock = null;
            _adapterRegistryMock = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync()
        {
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_PersonCode_Null()
        {
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), 
                It.IsAny<string>(), "ABC", It.IsAny<string>(), It.IsAny<bool>());
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_PersonCode_IsCorp()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            _personRepositoryMock.Setup(repo => repo.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(true);

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), "ABC", It.IsAny<string>(), It.IsAny<bool>());
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_OrganizationId_Null()
        {
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), "ABC",
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_OrganizationId_IsCorp()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            _personRepositoryMock.Setup(repo => repo.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), "ABC",
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_InstitutionId_Null()
        {
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                "ABC", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_InstitutionId_IsCorp()
        {
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            _personRepositoryMock.Setup(repo => repo.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                "ABC", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_RelationshipType()
        {
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), "97b3c0c2-6779-43e7-ad48-8a92b2cb7d9c", It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_EmptyCollection()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(new List<Domain.Base.Entities.Relationship>(), 0));

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_NullCollection()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(null);

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipByGuidAsync()
        {
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipByGuidAsync_PersonInstFlag_True()
        {
            var expected = new Domain.Base.Entities.Relationship("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Emp", true, DateTime.Today, DateTime.Today.AddDays(30))
            {
                Comment = "Coment 1",
                RelationPersonGuid = "862feace-b876-4ba2-ada7-2a23764e0719",
                RelationPersonInstFlag = true,
                SubjectPersonInstFlag = true,
                Status = "X",
                SubjectPersonGuid = "781b14fb-cac8-4519-9e17-032c8c86a5b5"
            };
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expected);

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipByGuidAsync_PersonInstFlag_False()
        {
            var expected = new Domain.Base.Entities.Relationship("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Emp", true, DateTime.Today, DateTime.Today.AddDays(30))
            {
                Comment = "Coment 1",
                RelationPersonGuid = "862feace-b876-4ba2-ada7-2a23764e0719",
                RelationPersonInstFlag = true,
                SubjectPersonInstFlag = false,
                SubjectPersonOrgFlag = true,
                Status = "X",
                SubjectPersonGuid = "781b14fb-cac8-4519-9e17-032c8c86a5b5"
            };
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expected);

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipByGuidAsync_PersonInst_Org_False()
        {
            var expected = new Domain.Base.Entities.Relationship("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Emp", true, DateTime.Today, DateTime.Today.AddDays(30))
            {
                Comment = "Coment 1",
                RelationPersonGuid = "862feace-b876-4ba2-ada7-2a23764e0719",
                RelationPersonInstFlag = false,
                SubjectPersonOrgFlag = false,
                Status = "X",
                SubjectPersonGuid = "781b14fb-cac8-4519-9e17-032c8c86a5b5"
            };
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expected);

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipByGuidAsync_KeyNotFoundException()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException());
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipByGuidAsync_InvalidOperationException()
        {
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipByIdAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException());
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipByGuidAsync_InvalidStatus_KeyNotFoundException()
        {
            _nonpersonRelationshipsCollection.First().Status = "ABCD";
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipByGuidAsync_InvalidRelationType_KeyNotFoundException()
        {
            var expected = new Domain.Base.Entities.Relationship("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Bad Type", true, DateTime.Today, DateTime.Today.AddDays(30))
            {
                Comment = "Coment 1",
                RelationPersonGuid = "862feace-b876-4ba2-ada7-2a23764e0719",
                RelationPersonInstFlag = true,
                Status = "X",
                SubjectPersonGuid = "781b14fb-cac8-4519-9e17-032c8c86a5b5"
            };
            _relationshipRepositoryMock.Setup(repo => repo.GetNonPersonRelationshipByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expected);

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_RelationshipType_ArgumentException()
        {
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), "77b3c0c2-6779-43e7-ad48-8a92b2cb7d9c", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_RelationshipType_PermissionsException()
        {
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { new Ellucian.Colleague.Domain.Entities.Role(1, "ABC") });

            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), "77b3c0c2-6779-43e7-ad48-8a92b2cb7d9c", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_NoRelationTypes_ArgumentNullException()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRelationTypesAsync(It.IsAny<bool>())).ReturnsAsync(null);
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_NoStatuses_ArgumentNullException()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRelationshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(null);
            var results = await _nonpersonRelationshipsService.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>());
        }

        //[TestMethod]
        //public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_Count()
        //{
        //    var results = await _nonpersonRelationshipsService.GetNonpersonRelationshipsAsync(true);
        //    Assert.AreEqual(3, results.Count());
        //}

        //[TestMethod]
        //public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_Properties()
        //{
        //    var result =
        //        (await _nonpersonRelationshipsService.GetNonpersonRelationshipsAsync(true)).FirstOrDefault(x => x.Code == nonpersonRelationshipsCode);
        //    Assert.IsNotNull(result.Id);
        //    Assert.IsNotNull(result.Code);
        //    Assert.IsNull(result.Description);

        //}

        //[TestMethod]
        //public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsAsync_Expected()
        //{
        //    var expectedResults = _nonpersonRelationshipsCollection.FirstOrDefault(c => c.Guid == nonpersonRelationshipsGuid);
        //    var actualResult =
        //        (await _nonpersonRelationshipsService.GetNonpersonRelationshipsAsync(true)).FirstOrDefault(x => x.Id == nonpersonRelationshipsGuid);
        //    Assert.AreEqual(expectedResults.Guid, actualResult.Id);
        //    Assert.AreEqual(expectedResults.Description, actualResult.Title);
        //    Assert.AreEqual(expectedResults.Code, actualResult.Code);

        //}

        //[TestMethod]
        //[ExpectedException(typeof (KeyNotFoundException))]
        //public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsByGuidAsync_Empty()
        //{
        //    await _nonpersonRelationshipsService.GetNonpersonRelationshipsByGuidAsync("");
        //}

        //[TestMethod]
        //[ExpectedException(typeof (KeyNotFoundException))]
        //public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsByGuidAsync_Null()
        //{
        //    await _nonpersonRelationshipsService.GetNonpersonRelationshipsByGuidAsync(null);
        //}

        //[TestMethod]
        //[ExpectedException(typeof (KeyNotFoundException))]
        //public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsByGuidAsync_InvalidId()
        //{
        //    _referenceRepositoryMock.Setup(repo => repo.GetRelationshipAsync(It.IsAny<bool>()))
        //        .Throws<KeyNotFoundException>();

        //    await _nonpersonRelationshipsService.GetNonpersonRelationshipsByGuidAsync("99");
        //}

        //[TestMethod]
        //public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsByGuidAsync_Expected()
        //{
        //    var expectedResults =
        //        _nonpersonRelationshipsCollection.First(c => c.Guid == nonpersonRelationshipsGuid);
        //    var actualResult =
        //        await _nonpersonRelationshipsService.GetNonpersonRelationshipsByGuidAsync(nonpersonRelationshipsGuid);
        //    Assert.AreEqual(expectedResults.Guid, actualResult.Id);
        //    Assert.AreEqual(expectedResults.Description, actualResult.Title);
        //    Assert.AreEqual(expectedResults.Code, actualResult.Code);

        //}

        //[TestMethod]
        //public async Task NonpersonRelationshipsService_GetNonpersonRelationshipsByGuidAsync_Properties()
        //{
        //    var result =
        //        await _nonpersonRelationshipsService.GetNonpersonRelationshipsByGuidAsync(nonpersonRelationshipsGuid);
        //    Assert.IsNotNull(result.Id);
        //    Assert.IsNotNull(result.Code);
        //    Assert.IsNull(result.Description);
        //    Assert.IsNotNull(result.Title);

        //}
    }
}