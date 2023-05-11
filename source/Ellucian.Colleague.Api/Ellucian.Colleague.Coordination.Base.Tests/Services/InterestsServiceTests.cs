// Copyright 2016 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class InterestsServiceTests
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
        public class InterestsService_Get : CurrentUserSetup
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
            private IEnumerable<Domain.Base.Entities.Interest> allInterests;
            private IEnumerable<Domain.Base.Entities.InterestType> allInterestTypes;


            private InterestsService interestsService;
            private string interestGuid = "6ae3a175-1dfd-4937-b97b-3c9ad596e024"; //ART, Art
            private string interestTypeGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023"; //AR, Arts
            private Domain.Entities.Permission permissionViewAnyPerson;
            private string invalidGuid;

            [TestInitialize]
            public void Initialize()
            {
                invalidGuid = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz";

                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;


                allInterests = new TestInterestsRepository().GetInterests();
                allInterestTypes = new TestInterestTypesRepository().GetInterestTypes();

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                interestsService = new InterestsService(adapterRegistry, refRepo, currentUserFactory, roleRepo, logger, configRepo);

            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                allInterests = null;
                allInterestTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                interestsService = null;
            }

            [TestMethod]
            public async Task InterestsService_GetInterestsById_ValidGuid()
            {
                var thisInterest = allInterests.Where(m => m.Guid == interestGuid).FirstOrDefault();
                var thisInterestType = allInterestTypes.FirstOrDefault(x => x.Code == thisInterest.Type);
                refRepoMock.Setup(repo => repo.GetInterestsAsync(true)).ReturnsAsync(allInterests.Where(m => m.Guid == interestGuid));
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(It.IsAny<bool>())).ReturnsAsync(allInterestTypes);
                Dtos.Interest interestItem = await interestsService.GetHedmInterestByIdAsync(interestGuid);
                Assert.AreEqual(thisInterest.Guid, interestItem.Id);
                Assert.AreEqual(thisInterest.Code, interestItem.Code);
                Assert.AreEqual(thisInterest.Description, interestItem.Title);
                Assert.AreEqual(thisInterestType.Guid, interestItem.Area.Id);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InterestsService_GetInterestsById_InvalidGuid()
            {
                refRepoMock.Setup<Task<IEnumerable<Domain.Base.Entities.Interest>>>(repo => repo.GetInterestsAsync(It.IsAny<bool>())).ReturnsAsync(allInterests);
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(true)).ReturnsAsync(allInterestTypes.Where(m => m.Guid == interestTypeGuid));
                await interestsService.GetHedmInterestByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InterestsService_GetInterestsById_Invalid()
            {
                refRepoMock.Setup(repo => repo.GetInterestsAsync(It.IsAny<bool>())).Throws<Exception>();
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(true)).ReturnsAsync(allInterestTypes.Where(m => m.Guid == interestTypeGuid));
                await interestsService.GetHedmInterestByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            public async Task InterestsService_GetInterests_Count()
            {
                refRepoMock.Setup(repo => repo.GetInterestsAsync(false)).ReturnsAsync(allInterests);
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(false)).ReturnsAsync(allInterestTypes);

                IEnumerable<Dtos.Interest> interests = await interestsService.GetHedmInterestsAsync();
                Assert.AreEqual(allInterests.Count(), interests.Count());
            }

            [TestMethod]
            public async Task InterestsService_GetInterests_IgnoreCache()
            {
                refRepoMock.Setup(repo => repo.GetInterestsAsync(true)).ReturnsAsync(allInterests);
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(true)).ReturnsAsync(allInterestTypes);

                IEnumerable<Dtos.Interest> interests = await interestsService.GetHedmInterestsAsync(true);
                Assert.AreEqual(allInterests.ElementAt(0).Guid, interests.ElementAt(0).Id);
                Assert.AreEqual(allInterests.ElementAt(0).Code, interests.ElementAt(0).Code);
                Assert.AreEqual(allInterests.ElementAt(0).Description, interests.ElementAt(0).Title);
            }

            [TestMethod]
            public async Task InterestsService_GetInterests_Cache()
            {
                refRepoMock.Setup(repo => repo.GetInterestsAsync(false)).ReturnsAsync(allInterests);
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(false)).ReturnsAsync(allInterestTypes);

                IEnumerable<Dtos.Interest> interests = await interestsService.GetHedmInterestsAsync();
                Assert.AreEqual(allInterests.ElementAt(0).Guid, interests.ElementAt(0).Id);
                Assert.AreEqual(allInterests.ElementAt(0).Code, interests.ElementAt(0).Code);
                Assert.AreEqual(allInterests.ElementAt(0).Description, interests.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InterestsService_GetInterestsById_InvalidInterests()
            {
                refRepoMock.Setup(repo => repo.GetInterestsAsync(It.IsAny<bool>())).Throws<Exception>();
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(It.IsAny<bool>())).ReturnsAsync(allInterestTypes.Where(m => m.Guid == interestTypeGuid));
                await interestsService.GetHedmInterestsAsync();
            }
        }
    }

    [TestClass]
    public class InterestAreaServiceTests
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
        public class InterestsService_InterestAreas : CurrentUserSetup
        {

            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.InterestType> allInterestTypes;
            private Mock<IConfigurationRepository> configRepoMock;
            private IConfigurationRepository configRepo;

            private InterestsService interestsService;
            private string interestTypeGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023"; //AR, Arts
            private Domain.Entities.Permission permissionViewAnyPerson;
            private string invalidGuid;

            [TestInitialize]
            public void Initialize()
            {
                invalidGuid = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz";

                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                configRepoMock = new Mock<IConfigurationRepository>();
                configRepo = configRepoMock.Object;

                allInterestTypes = new TestInterestTypesRepository().GetInterestTypes();

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                interestsService = new InterestsService(adapterRegistry, refRepo, currentUserFactory, roleRepo, logger, configRepo);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                allInterestTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                interestsService = null;
            }

            [TestMethod]
            public async Task InterestsService_GetInterestAreasById_ValidGuid()
            {
                Ellucian.Colleague.Domain.Base.Entities.InterestType thisPersonNameType = allInterestTypes.Where(m => m.Guid == interestTypeGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(true)).ReturnsAsync(allInterestTypes.Where(m => m.Guid == interestTypeGuid));
                Dtos.InterestArea personNameTypeItem = await interestsService.GetInterestAreasByIdAsync(interestTypeGuid);
                Assert.AreEqual(thisPersonNameType.Guid, personNameTypeItem.Id);
                Assert.AreEqual(thisPersonNameType.Code, personNameTypeItem.Code);
                Assert.AreEqual(thisPersonNameType.Description, personNameTypeItem.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InterestsService_GetInterestAreasById_InvalidGuid()
            {
                refRepoMock.Setup<Task<IEnumerable<Domain.Base.Entities.InterestType>>>(repo => repo.GetInterestTypesAsync(It.IsAny<bool>())).ReturnsAsync(allInterestTypes);
                await interestsService.GetInterestAreasByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InterestsService_GetInterestAreasById_Invalid()
            {
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(It.IsAny<bool>())).Throws<Exception>();
                await interestsService.GetInterestAreasByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            public async Task InterestsService_GetInterestAreas_Count()
            {
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(false)).ReturnsAsync(allInterestTypes);
                IEnumerable<Dtos.InterestArea> interestAreas = await interestsService.GetInterestAreasAsync();
                Assert.AreEqual(allInterestTypes.Count(), interestAreas.Count());
            }

            [TestMethod]
            public async Task InterestsService_GetInterestAreas_IgnoreCache()
            {
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(true)).ReturnsAsync(allInterestTypes);
                IEnumerable<Dtos.InterestArea> interestAreas = await interestsService.GetInterestAreasAsync(true);
                Assert.AreEqual(allInterestTypes.ElementAt(0).Guid, interestAreas.ElementAt(0).Id);
                Assert.AreEqual(allInterestTypes.ElementAt(0).Code, interestAreas.ElementAt(0).Code);
                Assert.AreEqual(allInterestTypes.ElementAt(0).Description, interestAreas.ElementAt(0).Title);
            }

            [TestMethod]
            public async Task InterestsService_GetInterestAreas_Cache()
            {
                refRepoMock.Setup(repo => repo.GetInterestTypesAsync(false)).ReturnsAsync(allInterestTypes);
                IEnumerable<Dtos.InterestArea> interestAreas = await interestsService.GetInterestAreasAsync();
                Assert.AreEqual(allInterestTypes.ElementAt(0).Guid, interestAreas.ElementAt(0).Id);
                Assert.AreEqual(allInterestTypes.ElementAt(0).Code, interestAreas.ElementAt(0).Code);
                Assert.AreEqual(allInterestTypes.ElementAt(0).Description, interestAreas.ElementAt(0).Title);
            }
        }
    }

}