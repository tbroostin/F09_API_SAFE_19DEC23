// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
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
    /// <summary>
    /// Test class for proxy service
    /// </summary>
    [TestClass]
    public class ProxyServiceTests : GenericUserFactory
    {
        #region Initialize and Cleanup
        private ProxyService proxyService = null;
        private ICurrentUserFactory currentUserFactory;
        private Domain.Base.Entities.ProxyConfiguration configEntity;
        private List<Domain.Base.Entities.ProxyAccessPermission> permissionEntities;
        private List<Domain.Base.Entities.ProxyUser> userEntities;
        private List<Domain.Base.Entities.ProxySubject> fakeProxySubjects;
        private List<Domain.Base.Entities.ProxyCandidate> fakeProxyCandidates;
        private ProxyCandidate candidateDto;

        // Mock/fake objects to construct ProxyService
        private Mock<IProxyRepository> proxyRepoMock;
        private IProxyRepository proxyRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ICurrentUserFactory currentUserFactoryFake;
        private Mock<IProfileRepository> profileRepoMock;
        private IProfileRepository profileRepo;
        private Mock<IPersonProxyUserRepository> personProxyUserRepoMock;
        private IPersonProxyUserRepository personProxyUserRepo;

        [TestInitialize]
        public void Initialize()
        {
            // Instantiate mock and fake objects used to construct the service
            proxyRepoMock = new Mock<IProxyRepository>();
            proxyRepo = proxyRepoMock.Object;
            configEntity = new Domain.Base.Entities.ProxyConfiguration(true, "DISCLOSURE.ID", "EMAIL.ID", true, true) { DisclosureReleaseText = "Line 1" };
            var configEntityGroup = new Domain.Base.Entities.ProxyWorkflowGroup("SF", "Student Finance");
            configEntityGroup.AddWorkflow(new Domain.Base.Entities.ProxyWorkflow("SFAA", "Student Finance Account Activity", "SF", true));
            configEntityGroup.AddWorkflow(new Domain.Base.Entities.ProxyWorkflow("SFMAP", "Student Finance Make a Payment", "SF", true));
            configEntity.AddWorkflowGroup(configEntityGroup);
            configEntity.AddRelationshipTypeCode("PAR");
            configEntity.AddRelationshipTypeCode("SPO");
            configEntity.AddRelationshipTypeCode("EMP");
            configEntity.AddDemographicField(new Domain.Base.Entities.DemographicField("FIRST_NAME", "First Name", Domain.Base.Entities.DemographicFieldRequirement.Required));
            configEntity.AddDemographicField(new Domain.Base.Entities.DemographicField("MIDDLE_NAME", "Middle Name", Domain.Base.Entities.DemographicFieldRequirement.Optional));
            configEntity.AddDemographicField(new Domain.Base.Entities.DemographicField("LAST_NAME", "Last Name", Domain.Base.Entities.DemographicFieldRequirement.Required));
            configEntity.AddDemographicField(new Domain.Base.Entities.DemographicField("PHONE_EXTENSION", "Phone Extension", Domain.Base.Entities.DemographicFieldRequirement.Hidden));

            permissionEntities = new List<Domain.Base.Entities.ProxyAccessPermission>()
            {
                new Domain.Base.Entities.ProxyAccessPermission("32", "0001234", "0003316", "SFMAP", DateTime.Parse("08/21/2015")),
                new Domain.Base.Entities.ProxyAccessPermission("33", "0001234", "0003316", "SFAA", DateTime.Parse("08/21/2015")),
                new Domain.Base.Entities.ProxyAccessPermission("34", "0001234", "0004000", "SFMAP", DateTime.Parse("08/25/2015")),
                new Domain.Base.Entities.ProxyAccessPermission("35", "0001234", "0004000", "SFAA", DateTime.Parse("08/25/2015")),
                new Domain.Base.Entities.ProxyAccessPermission("35", "0003316", "0000001", "SFAA", DateTime.Parse("08/25/2015")),
                new Domain.Base.Entities.ProxyAccessPermission("35", "0004000", "0000001", "SFAA", DateTime.Parse("08/25/2015"))
            };

            userEntities = new List<Domain.Base.Entities.ProxyUser>()
            {
                new Domain.Base.Entities.ProxyUser("0003316"),
                new Domain.Base.Entities.ProxyUser("0004000")
            };
            userEntities[0].AddPermission(permissionEntities[0]);
            userEntities[0].AddPermission(permissionEntities[1]);
            userEntities[1].AddPermission(permissionEntities[2]);
            userEntities[1].AddPermission(permissionEntities[3]);

            proxyRepoMock.Setup<Task<Domain.Base.Entities.ProxyConfiguration>>(repo => repo.GetProxyConfigurationAsync()).ReturnsAsync(configEntity);
            proxyRepoMock.Setup<Task<IEnumerable<Domain.Base.Entities.ProxyUser>>>(repo => repo.GetUserProxyPermissionsAsync(It.IsAny<string>())).ReturnsAsync(userEntities);

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            currentUserFactoryFake = new Person001UserFactory();
            profileRepoMock = new Mock<IProfileRepository>();
            profileRepo = profileRepoMock.Object;
            personProxyUserRepoMock = new Mock<IPersonProxyUserRepository>();
            personProxyUserRepo = personProxyUserRepoMock.Object;

            candidateDto = new ProxyCandidate()
            {
                EmailAddress = "john@smith.com",
                FirstName = "John",
                LastName = "Smith",
                RelationType = "P",
                GrantedPermissions = new List<string>() { "SFAA" },
                ProxySubject = currentUserFactoryFake.CurrentUser.PersonId,
                ProxyMatchResults = new List<PersonMatchResult>()
                {
                    new PersonMatchResult() { MatchCategory = PersonMatchCategoryType.Potential, MatchScore = 50, PersonId = "0003317" },
                    new PersonMatchResult() { MatchCategory = PersonMatchCategoryType.Potential, MatchScore = 49, PersonId = "0003318" }
                }
            };

            fakeProxySubjects = new List<Domain.Base.Entities.ProxySubject>()
                {
                    new Domain.Base.Entities.ProxySubject("0003316"),
                    new Domain.Base.Entities.ProxySubject("0004000")
                };
            fakeProxySubjects[0].AddPermission(permissionEntities[4]);
            fakeProxySubjects[1].AddPermission(permissionEntities[5]);

            fakeProxyCandidates = new List<Domain.Base.Entities.ProxyCandidate>()
            {
                new Domain.Base.Entities.ProxyCandidate(currentUserFactoryFake.CurrentUser.PersonId, "P", new List<string>() { "SFAA" }, "John", "Smith", "john@smith.com", new List<Domain.Base.Entities.PersonMatchResult>() { new Domain.Base.Entities.PersonMatchResult("0003317", 50, "P") }),
                new Domain.Base.Entities.ProxyCandidate(currentUserFactoryFake.CurrentUser.PersonId, "P", new List<string>() { "SFaA" }, "Jon", "Smith", "jon@smith.com", new List<Domain.Base.Entities.PersonMatchResult>() { new Domain.Base.Entities.PersonMatchResult("0003318", 50, "P") })
            };

            proxyRepoMock.Setup<Task<IEnumerable<Domain.Base.Entities.ProxySubject>>>(
                repo => repo.GetUserProxySubjectsAsync(It.IsAny<string>())).ReturnsAsync(fakeProxySubjects);

            proxyRepoMock.Setup<Task<IEnumerable<Domain.Base.Entities.ProxyCandidate>>>(
                repo => repo.GetUserProxyCandidatesAsync(It.IsAny<string>())).ReturnsAsync(fakeProxyCandidates);

            proxyRepoMock.Setup<Task<Domain.Base.Entities.ProxyCandidate>>(
                repo => repo.PostProxyCandidateAsync(It.IsAny<Domain.Base.Entities.ProxyCandidate>())).ReturnsAsync(fakeProxyCandidates[0]);

            profileRepoMock.Setup<Task<Domain.Base.Entities.Profile>>(
                repo => repo.GetProfileAsync(It.Is<String>(s => s == "0003316"), It.IsAny<bool>()))
                .ReturnsAsync(new Domain.Base.Entities.Profile("0003316", "Doe"));

            profileRepoMock.Setup<Task<Domain.Base.Entities.Profile>>(
                repo => repo.GetProfileAsync(It.Is<String>(s => s == "0004000"), It.IsAny<bool>()))
                .ReturnsAsync(new Domain.Base.Entities.Profile("0004000", "Boe"));

            profileRepoMock.Setup<Task<Domain.Base.Entities.Profile>>(
                repo => repo.GetProfileAsync(It.Is<String>(s => s == "DECEASED"), It.IsAny<bool>()))
                .ReturnsAsync(new Domain.Base.Entities.Profile("DECEASED", "Rip") { IsDeceased = true });

            var configAdapter = new ProxyConfigurationEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.ProxyConfiguration, Dtos.Base.ProxyConfiguration>()).Returns(configAdapter);

            var groupAdapter = new ProxyWorkflowGroupEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.ProxyWorkflowGroup, Dtos.Base.ProxyWorkflowGroup>()).Returns(groupAdapter);

            var workflowAdapter = new AutoMapperAdapter<Domain.Base.Entities.ProxyWorkflow, Dtos.Base.ProxyWorkflow>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.ProxyWorkflow, Dtos.Base.ProxyWorkflow>()).Returns(workflowAdapter);

            var accessAdapter = new AutoMapperAdapter<Domain.Base.Entities.ProxyAccessPermission, ProxyAccessPermission>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.ProxyAccessPermission, Dtos.Base.ProxyAccessPermission>()).Returns(accessAdapter);

            var proxyUserAdapter = new AutoMapperAdapter<Domain.Base.Entities.ProxyUser, ProxyUser>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.ProxyUser, Dtos.Base.ProxyUser>()).Returns(proxyUserAdapter);

            var principalUserAdapter = new AutoMapperAdapter<Domain.Base.Entities.ProxySubject, Dtos.Base.ProxySubject>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.ProxySubject, Dtos.Base.ProxySubject>()).Returns(principalUserAdapter);

            var candidateEntityAdapter = new ProxyCandidateEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.ProxyCandidate, Dtos.Base.ProxyCandidate>()).Returns(candidateEntityAdapter);

            var candidateDtoAdapter = new ProxyCandidateDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.ProxyCandidate, Domain.Base.Entities.ProxyCandidate>()).Returns(candidateDtoAdapter);

            var matchResultAdapter = new PersonMatchResultEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult>()).Returns(matchResultAdapter);

            var matchResultDtoAdapter = new PersonMatchResultDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.PersonMatchResult, Domain.Base.Entities.PersonMatchResult>()).Returns(matchResultDtoAdapter);

            var proxyPermAsgmtAdapter = new ProxyPermissionAssignmentDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.ProxyPermissionAssignment, Domain.Base.Entities.ProxyPermissionAssignment>()).Returns(proxyPermAsgmtAdapter);

            // Mock the adapter registry to use the automappers between the EmergencyInformation domain entity and dto. 
            var emptyAdapterRegistryMock = new Mock<IAdapterRegistry>();

            // Instantiate the service
            // Set up current user
            currentUserFactory = currentUserFactoryFake;
            proxyService = new ProxyService(proxyRepo, profileRepo, personProxyUserRepo, adapterRegistry, currentUserFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset the services and repository variables.
            proxyRepo = null;
            adapterRegistry = null;
            logger = null;
            roleRepo = null;
            currentUserFactoryFake = null;
            proxyService = null;
            currentUserFactory = null;
        }
        #endregion

        [TestClass]
        public class ProxyService_GetProxyConfigurationAsync : ProxyServiceTests
        {
            [TestMethod]
            public async Task ProxyService_GetProxyConfigurationAsync_Valid()
            {
                var config = await proxyService.GetProxyConfigurationAsync();
                Assert.AreEqual(configEntity.ProxyIsEnabled, config.ProxyIsEnabled);
                Assert.AreEqual(configEntity.DisclosureReleaseDocumentId, config.DisclosureReleaseDocumentId);
                CollectionAssert.AreEqual(configEntity.DisclosureReleaseText.ToList(), config.DisclosureReleaseText.ToList());
                Assert.AreEqual(configEntity.ProxyEmailDocumentId, config.ProxyEmailDocumentId);
                Assert.AreEqual(configEntity.WorkflowGroups.Count(), config.WorkflowGroups.Count());
                Assert.AreEqual(configEntity.RelationshipTypeCodes.Count(), config.RelationshipTypeCodes.Count());
                Assert.AreEqual(configEntity.DemographicFields.Count(), config.DemographicFields.Count());
            }
        }

        [TestClass]
        public class ProxyService_GetUserProxyPermissionsAsync : ProxyServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyService_GetUserProxyPermissionsAsync_NullId()
            {
                var users = await proxyService.GetUserProxyPermissionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyService_GetUserProxyPermissionsAsync_EmptyId()
            {
                var users = await proxyService.GetUserProxyPermissionsAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyService_GetUserProxyPermissionsAsync_NoPermission()
            {
                var users = await proxyService.GetUserProxyPermissionsAsync("1234567");
            }

            [TestMethod]
            public async Task ProxyService_GetUserProxyPermissionsAsync_Valid()
            {
                var users = await proxyService.GetUserProxyPermissionsAsync(currentUserFactory.CurrentUser.PersonId);
                Assert.AreEqual(userEntities.Count, users.Count());
            }
        }

        [TestClass]
        public class ProxyService_GetUserProxySubjectsAsync : ProxyServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyService_GetUserProxySubjectsAsync_NullId()
            {
                var principals = await proxyService.GetUserProxySubjectsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyService_GetUserProxySubjectsAsync_EmptyId()
            {
                var principals = await proxyService.GetUserProxySubjectsAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyService_GetUserProxySubjectsAsync_NoPermission()
            {
                var principals = await proxyService.GetUserProxySubjectsAsync("1234567");
            }

            [TestMethod]
            public async Task ProxyService_GetUserProxySubjectsAsync_Valid()
            {
                var principals = await proxyService.GetUserProxySubjectsAsync(currentUserFactory.CurrentUser.PersonId);
                Assert.AreEqual(fakeProxySubjects.Count, principals.Count());
            }
        }

        [TestClass]
        public class ProxyService_PostProxyCandidateAsync : ProxyServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyService_PostProxyCandidateAsync_NullId()
            {
                var candidates = await proxyService.PostProxyCandidateAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyService_PostProxyCandidateAsync_NoPermission()
            {
                var badCandidate = candidateDto;
                badCandidate.ProxySubject = "1234567";
                var candidates = await proxyService.PostProxyCandidateAsync(badCandidate);
            }

            [TestMethod]
            public async Task ProxyService_PostProxyCandidateAsync_Valid()
            {
                var result = await proxyService.PostProxyCandidateAsync(candidateDto);
                Assert.AreEqual(fakeProxyCandidates[0].EmailAddress, result.EmailAddress);
                Assert.AreEqual(fakeProxyCandidates[0].FirstName, result.FirstName);
                Assert.AreEqual(fakeProxyCandidates[0].LastName, result.LastName);
                Assert.AreEqual(fakeProxyCandidates[0].RelationType, result.RelationType);
                Assert.AreEqual(fakeProxyCandidates[0].ProxyMatchResults.Count(), result.ProxyMatchResults.Count());
            }
        }

        [TestClass]
        public class ProxyService_GetUserProxyCandidatesAsync : ProxyServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyService_GetUserProxyCandidatesAsync_NullId()
            {
                var candidates = await proxyService.GetUserProxyCandidatesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyService_GetUserProxyCandidatesAsync_EmptyId()
            {
                var candidates = await proxyService.GetUserProxyCandidatesAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyService_GetUserProxyCandidatesAsync_NoPermission()
            {
                var candidates = await proxyService.GetUserProxyCandidatesAsync("1234567");
            }

            [TestMethod]
            public async Task ProxyService_GetUserProxyCandidatesAsync_Valid()
            {
                var candidates = await proxyService.GetUserProxyCandidatesAsync(currentUserFactory.CurrentUser.PersonId);
                Assert.AreEqual(fakeProxyCandidates.Count, candidates.Count());
            }
        }

        [TestClass]
        public class ProxyService_PostPersonProxyUserAsync : ProxyServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyService_PostPersonProxyUserAsync_NullId()
            {
                var result = await proxyService.PostPersonProxyUserAsync(null);
            }

            //[TestMethod]
            //public async Task ProxyService_PostPersonProxyUserAsync_Valid()
            //{
            //    Domain.Base.Entities.PersonProxyUser userEnt;
            //    Dtos.Base.PersonProxyUser userDto;
            //    string 
            //        mail1 = "mail1@mail.com",
            //        mailType1 = "PRI",
            //        mail2 = "mail2@mail.com",
            //        mailType2 = "BUS";

            //    string
            //        phone1 = "Phone1",
            //        phoneType1 = "HO",
            //        phoneExt1 = "Ext1",
            //        phone2 = "phone2",
            //        phoneType2 = "HO",
            //        phoneExt2 = "Ext2";

            //    string
            //        nameGiven1 = "Given1",
            //        nameMiddle1 = "Middle1",
            //        nameFamily1 = "Family1",
            //        nameGiven2 = "Given2",
            //        nameMiddle2 = "Middle2",
            //        nameFamily2 = "Family2";

            //    string
            //        given = "Given",
            //        middle = "Middle",
            //        family = "Family",
            //        gender = "M",
            //        ssn = "987654321",
            //        id = "0000001",
            //        prefix = "MR",
            //        suffix = "JR";

            //    List<EmailAddress> emails;
            //    List<Phone> phones;
            //    List<PersonName> names;
            //    DateTime birth = DateTime.Now.AddDays(-1);


            //    var result = await proxyService.PostPersonProxyUserAsync(userDto);
            //    Assert.AreEqual(fakeProxyCandidates[0].EmailAddress, result.EmailAddress);
            //    Assert.AreEqual(fakeProxyCandidates[0].FirstName, result.FirstName);
            //    Assert.AreEqual(fakeProxyCandidates[0].LastName, result.LastName);
            //    Assert.AreEqual(fakeProxyCandidates[0].RelationType, result.RelationType);
            //    Assert.AreEqual(fakeProxyCandidates[0].ProxyMatchResults.Count(), result.ProxyMatchResults.Count());
            //}
        }

        /// <summary>
        /// Fake an ICurrentUserFactory implementation to construct ProxyService
        /// </summary>
        public class Person001UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        // Only the PersonId is part of the test, whether it matches the ID of the person whose 
                        // emergency information is requested. The remaining fields are arbitrary.
                        ControlId = "123",
                        Name = "Fred",
                        PersonId = "0001234",   /* From the test data of the test class */
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "Student" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        [TestClass]
        public class ProxyService_PostUserProxyPermissionsAsync : ProxyServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task ProxyService_PostUserProxyPermissionAsync_GrantedForDeceased_ThrowsException()
            {
                currentUserFactoryFake = new Person001UserFactory();
                var accessPermissions = new List<ProxyAccessPermission>()
                {
                    // "isGranted" calculated based on start/end date
                    new ProxyAccessPermission() {IsGranted = true, ProxySubjectId = "0001234", ProxyUserId = "DECEASED", StartDate = new DateTime(2001, 01, 02), ProxyWorkflowCode = "SFBB" },
                    new ProxyAccessPermission() {IsGranted = true, ProxySubjectId = "0001234", ProxyUserId = "DECEASED", StartDate = new DateTime(2001, 01, 02), EndDate = new DateTime(2030, 01, 01), ProxyWorkflowCode = "SFAA" }
                };
                var assignment = new ProxyPermissionAssignment() { ProxySubjectId = "0001234", Permissions = accessPermissions, ProxySubjectApprovalDocumentText = new List<string>() };
                var result = await proxyService.PostUserProxyPermissionsAsync(assignment);
            }

            [TestMethod]
            public async Task ProxyService_PostUserProxyPermissionAsync_NotGrantedForDeceased_Processes()
            {
                currentUserFactoryFake = new Person001UserFactory();
                var accessPermissions = new List<ProxyAccessPermission>()
                {
                    // "isGranted" calculated based on start/end date
                    new ProxyAccessPermission() {IsGranted = false, ProxySubjectId = "0001234", ProxyUserId = "DECEASED", StartDate = new DateTime(2001, 01, 02), ProxyWorkflowCode = "SFBB" },
                    new ProxyAccessPermission() {IsGranted = false, ProxySubjectId = "0001234", ProxyUserId = "DECEASED", StartDate = new DateTime(2001, 01, 02), ProxyWorkflowCode = "SFAA" }
                };
                var assignment = new ProxyPermissionAssignment() { ProxySubjectId = "0001234", Permissions = accessPermissions, ProxySubjectApprovalDocumentText = new List<string>() };
                var result = await proxyService.PostUserProxyPermissionsAsync(assignment);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyService_PostUserProxyPermissionAsync_InvalidCurrentUser_ThrowsException()
            {
                currentUserFactoryFake = new Person001UserFactory();
                var accessPermissions = new List<ProxyAccessPermission>()
                {
                    new ProxyAccessPermission() {IsGranted = false, ProxySubjectId = "0000001", ProxyUserId = "00002", StartDate = new DateTime(2001, 01, 02), ProxyWorkflowCode = "SFBB" },
                    new ProxyAccessPermission() {IsGranted = false, ProxySubjectId = "0000001", ProxyUserId = "00002", StartDate = new DateTime(2001, 01, 02), ProxyWorkflowCode = "SFAA" }
                };
                var assignment = new ProxyPermissionAssignment() { ProxySubjectId = "0000001", Permissions = accessPermissions, ProxySubjectApprovalDocumentText = new List<string>() };
                var result = await proxyService.PostUserProxyPermissionsAsync(assignment);
            }
        }

        /// <summary>
        /// Builds a configuration service object.
        /// </summary>
        private void BuildProxyService()
        {

        }
    }
}
