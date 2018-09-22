using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonGuardianRelationshipServiceTests
    {
        [TestClass]
        public class PersonGuardianRelationshipServiceTests_GET
        {
            Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            Mock<IRelationshipRepository> _relationshipRepositoryMock;
            Mock<IRoleRepository> _roleRepoMock;
            ICurrentUserFactory _userFactory;
            Mock<ILogger> _loggerMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            PersonGuardianRelationshipService _personGuardianRelationshipService;
            Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int> actuals;
            Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Relationship>, int> personGuardianEntityTuple;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Relationship> relationshipEntities;
            IEnumerable<Domain.Base.Entities.RelationType> allRelationTypes;

            protected Ellucian.Colleague.Domain.Entities.Role viewPersonGuardian = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.PERSON.GUARDIAN");

            List<string> defaultGuardianRelationsips = new List<string>() { "Parent", "Child" };
            private int limit = 4;
            private int offset = 0;

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                _roleRepoMock = new Mock<IRoleRepository>();
                _userFactory = new GenericUserFactory.PersonGuardianRelationshipUserFactory();
                _loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                BuildMocks();

                _personGuardianRelationshipService = new PersonGuardianRelationshipService(_adapterRegistryMock.Object, _referenceDataRepositoryMock.Object, _relationshipRepositoryMock.Object,
                                                                               _personRepositoryMock.Object, baseConfigurationRepository, _userFactory, _roleRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _referenceDataRepositoryMock = null;
                _personRepositoryMock = null;
                _adapterRegistryMock = null;
                _relationshipRepositoryMock = null;
                _roleRepoMock = null;
                _userFactory = null;
                _loggerMock = null;
                _personGuardianRelationshipService = null;
                actuals = null;
                personGuardianEntityTuple = null;
                relationshipEntities = null;
                allRelationTypes = null;
                defaultGuardianRelationsips = null;
            }

            [TestMethod]
            public async Task PersonGuardianRelationshipService_GetAll_NoDefaultGuardianRels()
            {
                defaultGuardianRelationsips = null;
                _relationshipRepositoryMock.Setup(i => i.GetDefaultGuardianRelationshipTypesAsync(It.IsAny<bool>())).ReturnsAsync(defaultGuardianRelationsips);

                var expected = new Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int>(new List<Dtos.PersonGuardianRelationship>(), 0);

                actuals = await _personGuardianRelationshipService.GetPersonGuardianRelationshipsAllAndFilterAsync(offset, limit, "");

                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item1.Count());
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task PersonGuardianRelationshipService_GetAll()
            {
                actuals = await _personGuardianRelationshipService.GetPersonGuardianRelationshipsAllAndFilterAsync(offset, limit, "");

                Assert.IsNotNull(actuals);
                Assert.AreEqual(personGuardianEntityTuple.Item1.Count(), actuals.Item1.Count());
                Assert.AreEqual(4, actuals.Item2);

                foreach (var actual in actuals.Item1)
                {
                    var expected = relationshipEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.SubjectPersonGuid, actual.SubjectPerson.Id);
                    Assert.AreEqual(expected.RelationPersonGuid, actual.Guardians.First().Id);
                }
            }

            [TestMethod]
            public async Task PersonGuardianRelationshipService_GetAll_WithNonGuardianRelationship()
            {
                var relationEntity = new Relationship("9", "10", "Affiliated", true, new DateTime(2016, 04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "9d96d550-ba60-49fd-8401-e1a8094a4dc9",
                    Comment = "Comment 5"
                };
                var relationshipEntitiesList = new List<Ellucian.Colleague.Domain.Base.Entities.Relationship>();
                relationshipEntitiesList.Add(relationEntity);
                relationshipEntitiesList.AddRange(relationshipEntities.ToList());
                personGuardianEntityTuple = new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(relationshipEntitiesList, relationshipEntitiesList.Count());
                _relationshipRepositoryMock.Setup(i => i.GetAllGuardiansAsync(It.IsAny<int>(), It.IsAny<int>(), "", defaultGuardianRelationsips)).ReturnsAsync(personGuardianEntityTuple);


                actuals = await _personGuardianRelationshipService.GetPersonGuardianRelationshipsAllAndFilterAsync(offset, limit, "");

                Assert.IsNotNull(actuals);
                Assert.AreEqual(personGuardianEntityTuple.Item1.Count(), actuals.Item1.Count());
                Assert.AreEqual(5, actuals.Item2);

                foreach (var actual in actuals.Item1)
                {
                    var expected = relationshipEntitiesList.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.SubjectPersonGuid, actual.SubjectPerson.Id);
                    Assert.AreEqual(expected.RelationPersonGuid, actual.Guardians.First().Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PersonGuardianRelationshipService_GetAll_KeyNotFoundException()
            {
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await _personGuardianRelationshipService.GetPersonGuardianRelationshipsAllAndFilterAsync(offset, limit, "A");
            }

            [TestMethod]
            public async Task PersonGuardianRelationshipService_GetById()
            {
                string id = "be6304e5-409d-4345-b4ad-4d34e1730e14";
                var expected = relationshipEntities.FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
                _relationshipRepositoryMock.Setup(i => i.GetPersonGuardianRelationshipByIdAsync(id)).ReturnsAsync(expected);
                var actual = await _personGuardianRelationshipService.GetPersonGuardianRelationshipByIdAsync(id);

                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.SubjectPersonGuid, actual.SubjectPerson.Id);
                Assert.AreEqual(expected.RelationPersonGuid, actual.Guardians.First().Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonGuardianRelationshipService_GetById_KeyNotFoundException_NotGuardian()
            {
                var relationEntity = new Relationship("9", "10", "Affiliated", true, new DateTime(2016, 04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "9d96d550-ba60-49fd-8401-e1a8094a4dc9",
                    Comment = "Comment 5"
                };
                var relationshipEntitiesList = new List<Ellucian.Colleague.Domain.Base.Entities.Relationship>();
                relationshipEntitiesList.Add(relationEntity);
                relationshipEntitiesList.AddRange(relationshipEntities.ToList());
                personGuardianEntityTuple = new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(relationshipEntitiesList, relationshipEntitiesList.Count());
                _relationshipRepositoryMock.Setup(i => i.GetAllGuardiansAsync(It.IsAny<int>(), It.IsAny<int>(), "", defaultGuardianRelationsips)).ReturnsAsync(personGuardianEntityTuple);

                string id = "9d96d550-ba60-49fd-8401-e1a8094a4dc9";
                var expected = relationshipEntitiesList.FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
                _relationshipRepositoryMock.Setup(i => i.GetPersonGuardianRelationshipByIdAsync(id)).ReturnsAsync(expected);

                var actual = await _personGuardianRelationshipService.GetPersonGuardianRelationshipByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonGuardianRelationshipService_GetById_ArgumentNullException()
            {
                var actual = await _personGuardianRelationshipService.GetPersonGuardianRelationshipByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonGuardianRelationshipService_GetById_GuardianRelations_Null_KeyNotFoundException()
            {
                _relationshipRepositoryMock.Setup(i => i.GetDefaultGuardianRelationshipTypesAsync(It.IsAny<bool>())).ReturnsAsync(null);
                var actual = await _personGuardianRelationshipService.GetPersonGuardianRelationshipByIdAsync("1234");
            }

            private void BuildMocks()
            {
                viewPersonGuardian.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.ViewAnyPersonGuardian));
                _roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPersonGuardian });

                _referenceDataRepositoryMock.Setup(i => i.GetRelationTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRelationTypes);
                _relationshipRepositoryMock.Setup(i => i.GetDefaultGuardianRelationshipTypesAsync(It.IsAny<bool>())).ReturnsAsync(defaultGuardianRelationsips);
                _relationshipRepositoryMock.Setup(i => i.GetAllGuardiansAsync(It.IsAny<int>(), It.IsAny<int>(), "", defaultGuardianRelationsips)).ReturnsAsync(personGuardianEntityTuple);
            }

            private void BuildData()
            {
                allRelationTypes = new List<RelationType>()
                {
                    new RelationType("7989a936-f41d-4c08-9fda-dd41314a9e34", "Parent", "P", "", PersonalRelationshipType.Parent, PersonalRelationshipType.Father, PersonalRelationshipType.Mother, "Child"),
                    new RelationType("2c27b01e-fb4e-4884-aece-77dbfce45250", "Child", "C", "", PersonalRelationshipType.Child, PersonalRelationshipType.Son, PersonalRelationshipType.Daughter, "Parent"),
                    new RelationType("8c27b01e-fb4e-4884-aece-77dbfce45259", "Affiliated", "A", "", PersonalRelationshipType.Other, PersonalRelationshipType.Other, PersonalRelationshipType.Other, "Other")
                };
                relationshipEntities = new TestPersonalRelationshipsRepository().GetPersonalRelationshipsEnities();
                personGuardianEntityTuple = new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(relationshipEntities, relationshipEntities.Count());
            }
        }
    }
}
