// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwitchableRoleUserFactory = Ellucian.Colleague.Coordination.Base.Tests.Services.OrganizationalRelationshipServiceTest.SwitchableRoleUserFactory;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class OrganizationalPositionServiceTests
    {
        // The service to be tested
        private OrganizationalPositionService _orgPositionService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<IOrganizationalPositionRepository> _orgPositionRepoMock;
        private IOrganizationalPositionRepository _orgPositionRepo;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private IConfigurationRepository _configurationRepo;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;
        private List<OrganizationalPosition> orgPositionsDomainObject;
        private SwitchableRoleUserFactory _currentUserFactory;


        [TestInitialize]
        public void Initialize()
        {
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _logger = new Mock<ILogger>().Object;
            _orgPositionRepoMock = new Mock<IOrganizationalPositionRepository>();
            _orgPositionRepo = _orgPositionRepoMock.Object;
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _configurationRepo = _configurationRepoMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _currentUserFactory = new SwitchableRoleUserFactory();
            _orgPositionService = new OrganizationalPositionService(_adapterRegistry, _orgPositionRepo, _logger, _configurationRepo, _roleRepo, _currentUserFactory);

            orgPositionsDomainObject = BuildOrgPositionRepoResponse();

            var orgPositionAdapter = new AutoMapperAdapter<Domain.Base.Entities.OrganizationalPosition, Dtos.Base.OrganizationalPosition>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.OrganizationalPosition, Dtos.Base.OrganizationalPosition>()).Returns(orgPositionAdapter);
            var orgPosRelAdapter = new AutoMapperAdapter<Domain.Base.Entities.OrganizationalPositionRelationship, Dtos.Base.OrganizationalPositionRelationship>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.OrganizationalPositionRelationship, Dtos.Base.OrganizationalPositionRelationship>()).Returns(orgPosRelAdapter);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _orgPositionRepoMock = null;
            _orgPositionRepo = null;
            _configurationRepoMock = null;
            _configurationRepo = null;
            _roleRepoMock = null;
            _roleRepo = null;
            _currentUserFactory = null;

        }

        private List<OrganizationalPosition> BuildOrgPositionRepoResponse()
        {
            var orgPositions = new List<Domain.Base.Entities.OrganizationalPosition>();
            var orgPosition1 = new OrganizationalPosition("POS1", "Position1");
            var orgPosition2 = new OrganizationalPosition("POS2", "Position2");
            var orgPosition3 = new OrganizationalPosition("POS3", "Positiox3");
            var relationship1 = new OrganizationalPositionRelationship("1", "POS1", "Position1", "POS2", "Position2", "Manager");
            var relationship2 = new OrganizationalPositionRelationship("2", "POS1", "Position1", "POS3", "Positiox3", "Other");
            orgPosition1.AddPositionRelationship(relationship1);
            orgPosition1.AddPositionRelationship(relationship2);
            orgPositions.Add(orgPosition1);
            orgPositions.Add(orgPosition2);
            orgPositions.Add(orgPosition3);
            return orgPositions;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetOrganizationalPositionByIdAsync_NullId_ThrowsException()
        {
            var orgPerPositionsDto = await _orgPositionService.GetOrganizationalPositionByIdAsync(null);
        }

        [TestMethod]
        public async Task GetOrganizationalPositionByIdAsync_Success()
        {
            var orgPosition = orgPositionsDomainObject.Where(p => p.Id == "POS1").FirstOrDefault();
            _orgPositionRepoMock.Setup(repo => repo.GetOrganizationalPositionsByIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<OrganizationalPosition>() { orgPosition });

            var orgPositionDto = await _orgPositionService.GetOrganizationalPositionByIdAsync("POS1");
            Assert.AreEqual(orgPosition.Id, orgPositionDto.Id);
            Assert.AreEqual(orgPosition.Relationships.Count(), orgPositionDto.Relationships.Count());
            for (int j = 0; j < orgPosition.Relationships.Count(); j++)
            {
                var orgPositionRel = orgPosition.Relationships.ElementAt(j);
                var orgPositionRelDto = orgPositionDto.Relationships.Where(opr => opr.Id == orgPositionRel.Id).First();
                Assert.AreEqual(orgPositionRel.OrganizationalPositionId, orgPositionRelDto.OrganizationalPositionId);
                Assert.AreEqual(orgPositionRel.OrganizationalPositionTitle, orgPositionRelDto.OrganizationalPositionTitle);
                Assert.AreEqual(orgPositionRel.RelatedOrganizationalPositionId, orgPositionRelDto.RelatedOrganizationalPositionId);
                Assert.AreEqual(orgPositionRel.RelatedOrganizationalPositionTitle, orgPositionRelDto.RelatedOrganizationalPositionTitle);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryOrganizationalPositionsAsync_NullCriteria_ThrowsException()
        {
            var orgPerPositionsDtos = await _orgPositionService.QueryOrganizationalPositionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryOrganizationalPositionsAsync_EmptyCriteria_ThrowsException()
        {
            var criteria = new Dtos.Base.OrganizationalPositionQueryCriteria() { Ids = new List<string>(), SearchString = string.Empty };
            var orgPerPositionsDtos = await _orgPositionService.QueryOrganizationalPositionsAsync(null);
        }

        [TestMethod]
        public async Task QueryOrganizationalPositionsAsync_Ids_ReturnsResults()
        {
            var positionIds = new List<string>() { "POS2", "POS3" };
            var criteria = new Dtos.Base.OrganizationalPositionQueryCriteria() { Ids = positionIds, SearchString = null };
            var orgPositionsEntities = orgPositionsDomainObject.Where(obj => positionIds.Contains(obj.Id));
            _orgPositionRepoMock.Setup(repo => repo.GetOrganizationalPositionsAsync(It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(orgPositionsEntities);
            var orgPositionsDtos = await _orgPositionService.QueryOrganizationalPositionsAsync(criteria);
            Assert.AreEqual(positionIds.Count(), orgPositionsDtos.Count());
            for (int i = 0; i < orgPositionsEntities.Count(); i++)
            {
                var orgPositionsEntity = orgPositionsEntities.ElementAt(i);
                var orgPositionsDto = orgPositionsDtos.Where(d=>d.Id == orgPositionsEntity.Id).First();
                Assert.AreEqual(orgPositionsEntity.Id, orgPositionsDto.Id);
                Assert.AreEqual(orgPositionsEntity.Relationships.Count(), orgPositionsDto.Relationships.Count());
                for (int j = 0; j < orgPositionsEntity.Relationships.Count(); j++)
                {
                    var orgPositionRel = orgPositionsEntity.Relationships.ElementAt(j);
                    var orgPositionRelDto = orgPositionsDto.Relationships.Where(opr => opr.Id == orgPositionRel.Id).First();
                    Assert.AreEqual(orgPositionRel.OrganizationalPositionId, orgPositionRelDto.OrganizationalPositionId);
                    Assert.AreEqual(orgPositionRel.OrganizationalPositionTitle, orgPositionRelDto.OrganizationalPositionTitle);
                    Assert.AreEqual(orgPositionRel.RelatedOrganizationalPositionId, orgPositionRelDto.RelatedOrganizationalPositionId);
                    Assert.AreEqual(orgPositionRel.RelatedOrganizationalPositionTitle, orgPositionRelDto.RelatedOrganizationalPositionTitle);
                }
            }
        }

        [TestMethod]
        public async Task QueryOrganizationalPositionsAsync_SearchString_ReturnsResults()
        {
            var searchString = "Position";
            var criteria = new Dtos.Base.OrganizationalPositionQueryCriteria() { Ids = null, SearchString = searchString };
            var orgPositionsEntities = orgPositionsDomainObject.Where(obj => obj.Title.IndexOf(searchString) >= 0);
            _orgPositionRepoMock.Setup(repo => repo.GetOrganizationalPositionsAsync(It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(orgPositionsEntities);
            var orgPositionsDtos = await _orgPositionService.QueryOrganizationalPositionsAsync(criteria);
            Assert.AreEqual(2, orgPositionsDtos.Count());
            for (int i = 0; i < orgPositionsEntities.Count(); i++)
            {
                var orgPositionsEntity = orgPositionsEntities.ElementAt(i);
                var orgPositionsDto = orgPositionsDtos.Where(d => d.Id == orgPositionsEntity.Id).First();
                Assert.AreEqual(orgPositionsEntity.Id, orgPositionsDto.Id);
                Assert.AreEqual(orgPositionsEntity.Relationships.Count(), orgPositionsDto.Relationships.Count());
                for (int j = 0; j < orgPositionsEntity.Relationships.Count(); j++)
                {
                    var orgPositionRel = orgPositionsEntity.Relationships.ElementAt(j);
                    var orgPositionRelDto = orgPositionsDto.Relationships.Where(opr => opr.Id == orgPositionRel.Id).First();
                    Assert.AreEqual(orgPositionRel.OrganizationalPositionId, orgPositionRelDto.OrganizationalPositionId);
                    Assert.AreEqual(orgPositionRel.OrganizationalPositionTitle, orgPositionRelDto.OrganizationalPositionTitle);
                    Assert.AreEqual(orgPositionRel.RelatedOrganizationalPositionId, orgPositionRelDto.RelatedOrganizationalPositionId);
                    Assert.AreEqual(orgPositionRel.RelatedOrganizationalPositionTitle, orgPositionRelDto.RelatedOrganizationalPositionTitle);
                }
            }
        }
    }
}
