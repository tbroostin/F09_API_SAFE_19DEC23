// Copyright 2015 Ellucian Company L.P. and its affiliates.
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
    public class PersonNameTypeServiceTests
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
        public class PersonNameTypeService_Get : CurrentUserSetup
        {

            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.PersonNameTypeItem> allPersonNameTypes;


            private PersonNameTypeService personNameTypeService;
            private string personNameTypeGuid = "69d3987d-a1da-4c32-a7ce-edb9b6c9c8b5";
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

                allPersonNameTypes = new TestPersonNameTypeRepository().Get();

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                personNameTypeService = new PersonNameTypeService(adapterRegistry, refRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                allPersonNameTypes = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                personNameTypeService = null;
            }

            [TestMethod]
            public async Task GetPersonNameTypeItemByGuid_ValidGuid()
            {
                Ellucian.Colleague.Domain.Base.Entities.PersonNameTypeItem thisPersonNameType = allPersonNameTypes.Where(m => m.Guid == personNameTypeGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(true)).ReturnsAsync(allPersonNameTypes.Where(m => m.Guid == personNameTypeGuid));
                Dtos.PersonNameTypeItem personNameTypeItem = await personNameTypeService.GetPersonNameTypeByIdAsync(personNameTypeGuid);
                Assert.AreEqual(thisPersonNameType.Guid, personNameTypeItem.Id);
                Assert.AreEqual(thisPersonNameType.Code, personNameTypeItem.Code);
                Assert.AreEqual(thisPersonNameType.Description, personNameTypeItem.Title);
             }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonNameTypeItemByGuid_InvalidGuid()
            {
                refRepoMock.Setup<Task<IEnumerable<Domain.Base.Entities.PersonNameTypeItem>>>(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonNameTypes);
                await personNameTypeService.GetPersonNameTypeByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetPersonNameTypeItemByGuid_InvalidPersonNameType()
            {
                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>())).Throws<Exception>();
                await personNameTypeService.GetPersonNameTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            public async Task GetPersonNameTypeItems_CountPersonNameTypeItems()
            {
                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(false)).ReturnsAsync(allPersonNameTypes);
                IEnumerable<Dtos.PersonNameTypeItem> personNameTypeItem = await personNameTypeService.GetPersonNameTypesAsync();
                Assert.AreEqual(allPersonNameTypes.Count(), personNameTypeItem.Count());
            }

            [TestMethod]
            public async Task GetPersonNameTypeItems_GetPersonNameTypesAsync()
            {
                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(false)).ReturnsAsync(allPersonNameTypes);
                IEnumerable<Dtos.PersonNameTypeItem> personNameTypeItem = await personNameTypeService.GetPersonNameTypesAsync();
                Assert.AreEqual(allPersonNameTypes.ElementAt(0).Guid, personNameTypeItem.ElementAt(0).Id);
                Assert.AreEqual(allPersonNameTypes.ElementAt(0).Code, personNameTypeItem.ElementAt(0).Code);
                Assert.AreEqual(allPersonNameTypes.ElementAt(0).Description, personNameTypeItem.ElementAt(0).Title);
             }

            [TestMethod]
            public async Task GetPersonNameTypeItemByGuid_Birth()
            {
                Ellucian.Colleague.Domain.Base.Entities.PersonNameTypeItem thisPersonNameType 
                    = allPersonNameTypes.Where(m => m.Type == Ellucian.Colleague.Domain.Base.Entities.PersonNameType.Birth).FirstOrDefault();
                var personNameTypeCollection = new List<Ellucian.Colleague.Domain.Base.Entities.PersonNameTypeItem>() { thisPersonNameType };
                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(true)).ReturnsAsync(personNameTypeCollection);
                Dtos.PersonNameTypeItem personNameTypeItem = await personNameTypeService.GetPersonNameTypeByIdAsync(thisPersonNameType.Guid);
                Assert.AreEqual(thisPersonNameType.Guid, personNameTypeItem.Id);
                Assert.AreEqual(thisPersonNameType.Code, personNameTypeItem.Code);
                Assert.AreEqual(thisPersonNameType.Description, personNameTypeItem.Title);
                Assert.AreEqual(Dtos.EnumProperties.PersonNameType2.Birth, personNameTypeItem.Type);
            }

            [TestMethod]
            public async Task GetPersonNameTypeItemByGuid_Legal()
            {
                Ellucian.Colleague.Domain.Base.Entities.PersonNameTypeItem thisPersonNameType 
                    = allPersonNameTypes.Where(m => m.Type == Ellucian.Colleague.Domain.Base.Entities.PersonNameType.Legal).FirstOrDefault();
                var personNameTypeCollection = new List<Ellucian.Colleague.Domain.Base.Entities.PersonNameTypeItem>() { thisPersonNameType };
                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(true)).ReturnsAsync(personNameTypeCollection);
                Dtos.PersonNameTypeItem personNameTypeItem = await personNameTypeService.GetPersonNameTypeByIdAsync(thisPersonNameType.Guid);
                Assert.AreEqual(thisPersonNameType.Guid, personNameTypeItem.Id);
                Assert.AreEqual(thisPersonNameType.Code, personNameTypeItem.Code);
                Assert.AreEqual(thisPersonNameType.Description, personNameTypeItem.Title);
                Assert.AreEqual(Dtos.EnumProperties.PersonNameType2.Legal, personNameTypeItem.Type);
            }

            [TestMethod]
            public async Task GetPersonNameTypeItemByGuid_Personal()
            {
                Ellucian.Colleague.Domain.Base.Entities.PersonNameTypeItem thisPersonNameType
                    = allPersonNameTypes.Where(m => m.Type == Ellucian.Colleague.Domain.Base.Entities.PersonNameType.Personal).FirstOrDefault();
                var personNameTypeCollection = new List<Ellucian.Colleague.Domain.Base.Entities.PersonNameTypeItem>() { thisPersonNameType };
                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(true)).ReturnsAsync(personNameTypeCollection);
                Dtos.PersonNameTypeItem personNameTypeItem = await personNameTypeService.GetPersonNameTypeByIdAsync(thisPersonNameType.Guid);
                Assert.AreEqual(thisPersonNameType.Guid, personNameTypeItem.Id);
                Assert.AreEqual(thisPersonNameType.Code, personNameTypeItem.Code);
                Assert.AreEqual(thisPersonNameType.Description, personNameTypeItem.Title);
                Assert.AreEqual(Dtos.EnumProperties.PersonNameType2.Personal, personNameTypeItem.Type);
            }
        }
    }
}