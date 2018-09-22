// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using People = System.Collections.Generic.IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Person>;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonalRelationshipsServiceTests
    {
        [TestClass]
        public class PersonalRelationshipsService_GET
        {
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRelationshipRepository> relationshipRepositoryMock;
            private Mock<IRoleRepository> roleRepoMock;
            private ICurrentUserFactory userFactoryMock;
            private Mock<ILogger> loggerMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            protected Ellucian.Colleague.Domain.Entities.Role viewPersonRelationship = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.RELATIONSHIP");

            private People people;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Relationship> relationshipEnities;
            private Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Relationship>, int> relationshipEnityTuple;
            private IEnumerable<Domain.Base.Entities.PersonalRelationshipStatus> allPersonalRelationshipStatuses;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RelationType> relationTypes;
            private IPersonalRelationshipsService personalRelationshipsService;
            private List<string> guardianRelationshipType;
            int offset = 0;
            int limit = 3;

            [TestInitialize]
            public void Initialize()
            {
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                userFactoryMock = new GenericUserFactory.RelationshipUserFactory();
                roleRepoMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                viewPersonRelationship.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.ViewAnyRelationship));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPersonRelationship });

                personalRelationshipsService = new PersonalRelationshipsService(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, relationshipRepositoryMock.Object,
                                                                                personRepositoryMock.Object, baseConfigurationRepository, userFactoryMock, roleRepoMock.Object, loggerMock.Object);
            }

            private void BuildData()
            {
                relationshipEnities = new TestPersonalRelationshipsRepository().GetPersonalRelationshipsEnities();
                relationshipEnityTuple = new Tuple<IEnumerable<Relationship>, int>(relationshipEnities, 3);
                allPersonalRelationshipStatuses = new TestPersonalRelationshipStatusRepository().GetPersonalRelationshipStatuses();
                relationTypes = new List<RelationType>()
                {
                    new RelationType("7989a936-f41d-4c08-9fda-dd41314a9e34", "Parent", "P", "", PersonalRelationshipType.Parent, PersonalRelationshipType.Father, PersonalRelationshipType.Mother, "Child"),
                    new RelationType("2c27b01e-fb4e-4884-aece-77dbfce45250", "Child", "C", "", PersonalRelationshipType.Child, PersonalRelationshipType.Son, PersonalRelationshipType.Daughter, "Parent"),
                    new RelationType("8c27b01e-fb4e-4884-aece-77dbfce45259", "Affiliated", "A", "", PersonalRelationshipType.Other, PersonalRelationshipType.Other, PersonalRelationshipType.Other, "Other")
                };

                personRepositoryMock.SetupSequence(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("f3836d0e-ca45-455a-a873-d0771b8f089e"))
                    .Returns(Task.FromResult("e963d192-00ba-4327-aa00-9630b86e0533"))
                    .Returns(Task.FromResult("f3836d0e-ca45-455a-a873-d0771b8f089e"))
                    .Returns(Task.FromResult("93fd7cc0-2f4a-4e04-a239-b8b59a7575b4"))
                    .Returns(Task.FromResult("f3836d0e-ca45-455a-a873-d0771b8f089e"))
                    .Returns(Task.FromResult("e9e8e973-3d65-4c14-8155-6efc91100ae3"))
                    .Returns(Task.FromResult("6d96d550-ba60-49fd-8401-e1a8094a4dc7"))
                    .Returns(Task.FromResult("f3836d0e-ca45-455a-a873-d0771b8f089e"));

                people = new List<Person>()
                {
                    new Person("1", "Bhole"){ Guid = "f3836d0e-ca45-455a-a873-d0771b8f089e", Gender = "F"},
                    new Person("2", "Bhole"){ Guid = "e963d192-00ba-4327-aa00-9630b86e0533", Gender = "F"},
                    new Person("3", "Bhole"){ Guid = "93fd7cc0-2f4a-4e04-a239-b8b59a7575b4", Gender = "M"},
                    new Person("4", "Bhole"){ Guid = "e9e8e973-3d65-4c14-8155-6efc91100ae3", Gender = "M"},
                    new Person("7", "Bhole"){ Gender = "M"}
                };

                personRepositoryMock.SetupSequence(x => x.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(people.FirstOrDefault(p => p.Id == "1")))
                    .Returns(Task.FromResult(people.FirstOrDefault(p => p.Id == "2")))
                    .Returns(Task.FromResult(people.FirstOrDefault(p => p.Id == "1")))
                    .Returns(Task.FromResult(people.FirstOrDefault(p => p.Id == "3")))
                    .Returns(Task.FromResult(people.FirstOrDefault(p => p.Id == "1")))
                    .Returns(Task.FromResult(people.FirstOrDefault(p => p.Id == "4")))
                    .Returns(Task.FromResult(people.FirstOrDefault(p => p.Id == "1")))
                    .Returns(Task.FromResult(people.FirstOrDefault(p => p.Id == "7")));

                guardianRelationshipType = new List<string>() { "Parent" };
                relationshipRepositoryMock.Setup(i => i.GetDefaultGuardianRelationshipTypesAsync(It.IsAny<bool>())).ReturnsAsync(guardianRelationshipType);
                referenceDataRepositoryMock.Setup(rt => rt.GetRelationTypesAsync(It.IsAny<bool>())).ReturnsAsync(relationTypes);
                referenceDataRepositoryMock.Setup(s => s.GetPersonalRelationshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonalRelationshipStatuses);
            }

            [TestCleanup]
            public void Cleanup()
            {
                referenceDataRepositoryMock = null;
                personRepositoryMock = null;
                adapterRegistryMock = null;
                relationshipRepositoryMock = null;
                userFactoryMock = null;
                roleRepoMock = null;
                loggerMock = null;
                personalRelationshipsService = null;
                relationshipEnities = null;
                relationshipEnityTuple = null;
                allPersonalRelationshipStatuses = null;
                relationTypes = null;
                people = null;
            }

            [TestMethod]
            public async Task PersonalRelationshipsService_GetAllPersonalRelationshipsAsync()
            {
                BuildData();
                relationshipRepositoryMock.Setup(x => x.GetAllAsync(offset, limit, It.IsAny<bool>(), It.IsAny<List<string>>())).ReturnsAsync(relationshipEnityTuple);
                var results = await personalRelationshipsService.GetAllPersonalRelationshipsAsync(offset, limit, It.IsAny<bool>());

                relationshipEnities.OrderBy(i => i.Guid);
                results.Item1.OrderBy(i => i.Id);

                var count = results.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = relationshipEnities.ToList()[i];
                    var actual = results.Item1.ToList()[i];

                    Assert.AreEqual(expected.Comment, actual.Comment);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                }
            }

            [TestMethod]
            public async Task PersonalRelationshipsService_GetPersonalRelationshipFilterByRelationTypeAsync()
            {
                BuildData();
                relationshipRepositoryMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(relationshipEnityTuple);

                var results = (await personalRelationshipsService.GetPersonalRelationshipsByFilterAsync(0, 1, null, null, "Parent"));

                relationshipEnities.OrderBy(i => i.Guid);
                results.Item1.OrderBy(i => i.Id);

                var count = results.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = relationshipEnities.ToList()[i];
                    var actual = results.Item1.ToList()[i];

                    Assert.AreEqual(expected.Comment, actual.Comment);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                }
            }

            [TestMethod]
            public async Task PersonalRelationshipsService_GetPersonalRelationshipFilterByRelationTypeIDAsync()
            {
                BuildData();
                relationshipRepositoryMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(relationshipEnityTuple);

                var results = (await personalRelationshipsService.GetPersonalRelationshipsByFilterAsync(0, 1, null, null, null, "7989a936-f41d-4c08-9fda-dd41314a9e34"));

                relationshipEnities.OrderBy(i => i.Guid);
                results.Item1.OrderBy(i => i.Id);

                var count = results.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = relationshipEnities.ToList()[i];
                    var actual = results.Item1.ToList()[i];

                    Assert.AreEqual(expected.Comment, actual.Comment);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                }
            }

            [TestMethod]
            public async Task PersonalRelationshipsService_GetPersonalRelationshipByIdAsync()
            {
                BuildData();
                var id = "9d96d550-ba60-49fd-8401-e1a8094a4dc9";
                Relationship expected = new Relationship("5", "6", "Affiliated", true, new DateTime(2016, 04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "9d96d550-ba60-49fd-8401-e1a8094a4dc9",
                    Comment = "Comment 4",
                    RelationPersonGender = "F",
                    RelationPersonGuid = "3d65dec8-f6f3-445d-ad7a-5f7574f0e624",
                    SubjectPersonGuid = "1d65dec8-f6f3-445d-ad7a-5f7574f0e622",
                    SubjectPersonGender = "M"
                };
                relationshipRepositoryMock.Setup(x => x.GetPersonRelationshipByIdAsync(id)).ReturnsAsync(expected);
                var actual = await personalRelationshipsService.GetPersonalRelationshipByIdAsync(id);

                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
                Assert.AreEqual(actual.DirectRelationship.RelationshipType, Dtos.PersonalRelationshipType.Other);
                Assert.AreEqual(actual.ReciprocalRelationship.RelationshipType, Dtos.PersonalRelationshipType.Other);
            }

            [TestMethod]
            public async Task PersonalRelationshipsService_GetPersonalRelationshipByFilterAsync()
            {
                BuildData();
                relationshipRepositoryMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(relationshipEnityTuple);

                var results = (await personalRelationshipsService.GetPersonalRelationshipsByFilterAsync(0, 1, "9d96d550-ba60-49fd-8401-e1a8094a4dc9"));
                Assert.AreEqual(0, results.Item2);
            }

            //KeyNotFoundException
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonalRelationshipsService_GetPersonalRelationshipByIdAsync_KeyNotFoundException()
            {
                BuildData();
                var id = "9d96d550-ba60-49fd-8401-e1a8094a4dc9";
                Relationship expected = new Relationship("5", "6", "Affiliate", true, new DateTime(2016, 04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "9d96d550-ba60-49fd-8401-e1a8094a4dc9",
                    Comment = "Comment 4"
                };
                relationshipRepositoryMock.Setup(x => x.GetPersonRelationshipByIdAsync(id)).ReturnsAsync(expected);
                var actual = await personalRelationshipsService.GetPersonalRelationshipByIdAsync(id);
            }

            [TestMethod]

            public async Task PersonalRelationshipsService_ConvertRelationshipTypeGuidToCode_Exception()
            {
                BuildData();
                relationshipRepositoryMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(relationshipEnityTuple);

                var results = (await personalRelationshipsService.GetPersonalRelationshipsByFilterAsync(0, 1, null, null, null, "7989a936-f41d-4c08-9fda-dd41314a9e35"));
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonalRelationshipsService_RelativeNull_KeyNotFoundException()
            {
                people = new List<Person>()
                {
                    new Person("1", "Bhole"){ Guid = "f3836d0e-ca45-455a-a873-d0771b8f089e", Gender = "F"},
                    new Person("2", "Bhole"){ Guid = "e963d192-00ba-4327-aa00-9630b86e0533", Gender = "F"},
                    new Person("3", "Bhole"){ Guid = "93fd7cc0-2f4a-4e04-a239-b8b59a7575b4", Gender = "M"},
                    new Person("4", "Bhole"){ Guid = "e9e8e973-3d65-4c14-8155-6efc91100ae3", Gender = "M"},
                    new Person("7", "Bhole"){ Gender = "M"}
                };
                var subjectId = "1d96d550-ba60-50fd-8401-e1a8094a4dc1";
                var relativeId = "2d96d550-ba60-50fd-8401-e1a8094a4dc2";
                personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("5")).ReturnsAsync(subjectId);
                personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync("7")).ReturnsAsync(relativeId);

                personRepositoryMock.Setup(x => x.GetPersonByGuidNonCachedAsync(subjectId)).ReturnsAsync(people.ToList()[4]);
                personRepositoryMock.Setup(x => x.GetPersonByGuidNonCachedAsync(relativeId)).ReturnsAsync(null);

                Relationship expected = new Relationship("5", "7", "Affiliated", true, new DateTime(2016, 04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "9d96d550-ba60-49fd-8401-e1a8094a4dc9",
                    Comment = "Comment 4"
                };
                relationshipRepositoryMock.Setup(x => x.GetPersonRelationshipByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
                var actual = await personalRelationshipsService.GetPersonalRelationshipByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonalRelationshipsService_InvalidStatus_KeyNotFoundException()
            {
                BuildData();
                var id = "9d96d550-ba60-49fd-8401-e1a8094a4dc9";
                Relationship expected = new Relationship("5", "6", "Affiliated", true, new DateTime(2016, 04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "9d96d550-ba60-49fd-8401-e1a8094a4dc9",
                    Comment = "Comment 4",
                    Status = "Z"
                };
                relationshipRepositoryMock.Setup(x => x.GetPersonRelationshipByIdAsync(id)).ReturnsAsync(expected);
                var actual = await personalRelationshipsService.GetPersonalRelationshipByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task PersonalRelationshipsService_InvalidStatus_InvalidOperationException()
            {
                BuildData();
                var id = "9d96d550-ba60-49fd-8401-e1a8094a4dc9";
                Relationship expected = new Relationship("5", "6", "Child", true, new DateTime(2016, 04, 14), new DateTime(2016, 05, 15))
                {
                    Guid = "9d96d550-ba60-49fd-8401-e1a8094a4dc9",
                    Comment = "Comment 4",
                    Status = "Z"
                };
                relationshipRepositoryMock.Setup(x => x.GetPersonRelationshipByIdAsync(id)).ReturnsAsync(expected);
                var actual = await personalRelationshipsService.GetPersonalRelationshipByIdAsync(id);
            }
        }
    }
}
