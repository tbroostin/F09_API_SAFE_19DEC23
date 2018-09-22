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
            private IEnumerable<Domain.Base.Entities.PersonalRelationshipStatus> allPersonalRelationshipStatuses;
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
                Domain.Base.Entities.PersonalRelationshipStatus thisPersonalRelationshipStatus = allPersonalRelationshipStatuses.Where(m => m.Guid == personalRelationshipStatusGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetPersonalRelationshipStatusesAsync(true)).ReturnsAsync(allPersonalRelationshipStatuses.Where(m => m.Guid == personalRelationshipStatusGuid));
                Dtos.PersonalRelationshipStatus relationType = await personalRelationshipService.GetPersonalRelationshipStatusByGuidAsync(personalRelationshipStatusGuid);
                Assert.AreEqual(thisPersonalRelationshipStatus.Guid, relationType.Id);
                Assert.AreEqual(thisPersonalRelationshipStatus.Code, relationType.Code);
                Assert.AreEqual(null, relationType.Description);

            }


            [TestMethod]
            public async Task GetPersonalRelationshipStatuses_CountPersonalRelationshipStatuses()
            {
                refRepoMock.Setup(repo => repo.GetPersonalRelationshipStatusesAsync(false)).ReturnsAsync(allPersonalRelationshipStatuses);
                IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationshipStatus> personalRelationshipStatus = await personalRelationshipService.GetPersonalRelationshipStatusesAsync();
                Assert.AreEqual(4, personalRelationshipStatus.Count());
            }

            [TestMethod]
            public async Task GetPersonalRelationshipStatuses_ComparePersonalRelationshipStatuses()
            {
                refRepoMock.Setup(repo => repo.GetPersonalRelationshipStatusesAsync(false)).ReturnsAsync(allPersonalRelationshipStatuses);

                IEnumerable<Dtos.PersonalRelationshipStatus> personalRelationshipStatuses = await personalRelationshipService.GetPersonalRelationshipStatusesAsync();
                Assert.AreEqual(allPersonalRelationshipStatuses.ElementAt(0).Guid, personalRelationshipStatuses.ElementAt(0).Id);
                Assert.AreEqual(allPersonalRelationshipStatuses.ElementAt(0).Code, personalRelationshipStatuses.ElementAt(0).Code);
                Assert.AreEqual(null, personalRelationshipStatuses.ElementAt(0).Description);
            }
        }
    }
}
