// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Base.Services;
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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class OrganizationalRelationshipServiceTest
    {
        private OrganizationalRelationshipService _orgRelationshipService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private SwitchableRoleUserFactory _currentUserFactory;
        private IRoleRepository _roleRepository;
        private ILogger _logger;
        private Mock<IOrganizationalRelationshipRepository> _orgRelationshipRepoMock;
        private IOrganizationalRelationshipRepository _orgRelationshipRepo;

        [TestInitialize]
        public void Initialize()
        {
            _currentUserFactory = new SwitchableRoleUserFactory();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();

            //Register adapters
            var orgRelationshipEntityToDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.OrganizationalRelationship, Dtos.Base.OrganizationalRelationship>(_adapterRegistry, _logger);
            var orgRelationshipDtoToEntityAdapter = new OrganizationalRelationshipDtoToEntityAdapter(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.OrganizationalRelationship, Dtos.Base.OrganizationalRelationship>()).Returns(orgRelationshipEntityToDtoAdapter);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.OrganizationalRelationship, Domain.Base.Entities.OrganizationalRelationship>()).Returns(orgRelationshipDtoToEntityAdapter);

            _adapterRegistry = _adapterRegistryMock.Object;
            var _roleRepositoryMock = new Mock<IRoleRepository>();
            var personnelDirectorRole = new Domain.Entities.Role(1, "Personnel Director");
            personnelDirectorRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdateOrganizationalRelationships));
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { personnelDirectorRole });
            _roleRepository = _roleRepositoryMock.Object;
            _logger = new Mock<ILogger>().Object;
            _orgRelationshipRepoMock = new Mock<IOrganizationalRelationshipRepository>();
            _orgRelationshipRepo = _orgRelationshipRepoMock.Object;

            _orgRelationshipService = new OrganizationalRelationshipService(_orgRelationshipRepo, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _orgRelationshipService = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _currentUserFactory = null;
            _roleRepository = null;
            _logger = null;
            _orgRelationshipRepoMock = null;
            _orgRelationshipRepo = null;
        }

        [TestMethod]
        public async Task CreateOrganizationalRelationshipAsync()
        {
            _currentUserFactory.UsePersonnelDirectorRole = true;
            var expectedDto = new OrganizationalRelationship
            {
                Id = "RR1",
                Category = "MGR",
                OrganizationalPersonPositionId = "RS1",
                RelatedOrganizationalPersonPositionId = "RS2",
                RelatedPersonId = "1",
                RelatedPersonName = "Walter White",
                RelatedPositionId = "P1",
                RelatedPositionTitle = "Chemistry Teacher"
            };

            var newEntity = new Domain.Base.Entities.OrganizationalRelationship
            (
                "",
                "RS1",
                "RS2",
                ""
            );

            _orgRelationshipRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Domain.Base.Entities.OrganizationalRelationship>()))
                .Returns<Domain.Base.Entities.OrganizationalRelationship>(or => Task.FromResult(newEntity));

            var serviceResult = await _orgRelationshipService.AddAsync(expectedDto);

            Assert.AreEqual(expectedDto.OrganizationalPersonPositionId, serviceResult.OrganizationalPersonPositionId);
            Assert.AreEqual(expectedDto.RelatedOrganizationalPersonPositionId, serviceResult.RelatedOrganizationalPersonPositionId);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task CreateOrganizationalRelationshipAsync_WithoutPermission_ThrowsException()
        {
            var expectedDto = new OrganizationalRelationship
            {
                Id = "RR1",
                Category = "MGR",
                OrganizationalPersonPositionId = "RS1",
                RelatedOrganizationalPersonPositionId = "RS2",
                RelatedPersonId = "1",
                RelatedPersonName = "Walter White",
                RelatedPositionId = "P1",
                RelatedPositionTitle = "Chemistry Teacher"
            };

            var newEntity = new Domain.Base.Entities.OrganizationalRelationship
            (
                "",
                "RS1",
                "RS2",
                ""
            );

            _orgRelationshipRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Domain.Base.Entities.OrganizationalRelationship>()))
                .Returns<Domain.Base.Entities.OrganizationalRelationship>(or => Task.FromResult(newEntity));

            var serviceResult = await _orgRelationshipService.AddAsync(expectedDto);
        }

        [TestMethod]
        public async Task DeleteOrganizationalRelationshipAsync()
        {
            _currentUserFactory.UsePersonnelDirectorRole = true;
            var deletedId = "RR1";

            _orgRelationshipRepoMock.Setup(repo => repo.DeleteAsync(It.IsAny<string>())).Returns(Task.FromResult(false)).Verifiable();

            await _orgRelationshipService.DeleteAsync(deletedId);
            _orgRelationshipRepoMock.Verify(m => m.DeleteAsync(deletedId), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task DeleteOrganizationalRelationshipAsync_WithoutPermission_ThrowsException()
        {
            var deletedId = "RR1";

            _orgRelationshipRepoMock.Setup(repo => repo.DeleteAsync(It.IsAny<string>())).Returns(Task.FromResult(false)).Verifiable();

            await _orgRelationshipService.DeleteAsync(deletedId);
        }

        [TestMethod]
        public async Task UpdateOrganizationalRelationshipAsync()
        {
            _currentUserFactory.UsePersonnelDirectorRole = true;
            // Give dto
            var expectedDto = new OrganizationalRelationship
            {
                Id = "RR1",
                Category = "MGR",
                OrganizationalPersonPositionId = "RS1",
                RelatedOrganizationalPersonPositionId = "RS2",
                RelatedPersonId = "1",
                RelatedPersonName = "Walter White",
                RelatedPositionId = "P1",
                RelatedPositionTitle = "Chemistry Teacher"
            };

            // Mock up entity
            var updatedEntity = new Domain.Base.Entities.OrganizationalRelationship
            (
                "RR1",
                "RS1",
                "7",
                "P2",
                "Chemistry Student",
                null, null, "RS2",
                "1",
                "P1",
                "Chemistry Teacher",
                null, null, "MGR"
            );

            // Mock up repo, return task with result of updated entity
            _orgRelationshipRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Base.Entities.OrganizationalRelationship>())).Returns<Domain.Base.Entities.OrganizationalRelationship>(or => Task.FromResult(updatedEntity));

            // Test dtoToBeUpdated (expect it to be returned from service)
            var serviceResult = await _orgRelationshipService.UpdateAsync(expectedDto);

            Assert.AreEqual(expectedDto.Id, serviceResult.Id);
            Assert.AreEqual(expectedDto.Category, serviceResult.Category);
            Assert.AreEqual(expectedDto.OrganizationalPersonPositionId, serviceResult.OrganizationalPersonPositionId);
            Assert.AreEqual(expectedDto.RelatedOrganizationalPersonPositionId, serviceResult.RelatedOrganizationalPersonPositionId);
            Assert.AreEqual(expectedDto.RelatedPositionId, serviceResult.RelatedPositionId);
            Assert.AreEqual(expectedDto.RelatedPositionTitle, serviceResult.RelatedPositionTitle);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UpdateOrganizationalRelationshipAsync_WithoutPermission_ThrowsException()
        {
            // Give dto
            var expectedDto = new OrganizationalRelationship
            {
                Id = "RR1",
                Category = "MGR",
                OrganizationalPersonPositionId = "RS1",
                RelatedOrganizationalPersonPositionId = "RS2",
                RelatedPersonId = "1",
                RelatedPersonName = "Walter White",
                RelatedPositionId = "P1",
                RelatedPositionTitle = "Chemistry Teacher"
            };

            // Mock up entity
            var updatedEntity = new Domain.Base.Entities.OrganizationalRelationship
            (
                "RR1",
                "RS1",
                "7",
                "P2",
                "Chemistry Student",
                null, null, "RS2",
                "1",
                "P1",
                "Chemistry Teacher",
                null, null, "MGR"
            );

            // Mock up repo, return task with result of updated entity
            _orgRelationshipRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Base.Entities.OrganizationalRelationship>())).Returns<Domain.Base.Entities.OrganizationalRelationship>(or => Task.FromResult(updatedEntity));

            // Test dtoToBeUpdated (expect it to be returned from service)
            var serviceResult = await _orgRelationshipService.UpdateAsync(expectedDto);

            Assert.AreEqual(expectedDto.Id, serviceResult.Id);
            Assert.AreEqual(expectedDto.Category, serviceResult.Category);
            Assert.AreEqual(expectedDto.OrganizationalPersonPositionId, serviceResult.OrganizationalPersonPositionId);
            Assert.AreEqual(expectedDto.RelatedOrganizationalPersonPositionId, serviceResult.RelatedOrganizationalPersonPositionId);
            Assert.AreEqual(expectedDto.RelatedPositionId, serviceResult.RelatedPositionId);
            Assert.AreEqual(expectedDto.RelatedPositionTitle, serviceResult.RelatedPositionTitle);
        }

        public class SwitchableRoleUserFactory : ICurrentUserFactory
        {
            public bool UsePersonnelDirectorRole = false;

            public ICurrentUser CurrentUser
            {
                get
                {
                    var roles = new List<string>();
                    if (UsePersonnelDirectorRole)
                    {
                        roles.Add("Personnel Director");
                    }
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "George",
                        PersonId = "0000015",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "george",
                        Roles = roles,
                        SessionFixationId = "abc123",
                    });
                }
            }
        }
    }

}
