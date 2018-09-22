// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
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
    public class OrganizationalPersonPositionServiceTests
    {
        // The service to be tested
        private OrganizationalPersonPositionService _orgPersonPositionService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<IOrganizationalPersonPositionRepository> _orgPersonPositionRepoMock;
        private IOrganizationalPersonPositionRepository _orgPersonPositionRepo;
        private Mock<IPersonBaseRepository> _personBaseRepoMock;
        private IPersonBaseRepository _personBaseRepo;
        private List<OrganizationalPersonPosition> orgPersonPositionsDomainObject;
        private List<string> criteriaIds;
        private string searchString;
        private List<PersonBase> personBaseObjects;


        [TestInitialize]
        public void Initialize()
        {
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _logger = new Mock<ILogger>().Object;
            _orgPersonPositionRepoMock = new Mock<IOrganizationalPersonPositionRepository>();
            _orgPersonPositionRepo = _orgPersonPositionRepoMock.Object;
            _personBaseRepoMock = new Mock<IPersonBaseRepository>();
            _personBaseRepo = _personBaseRepoMock.Object;

            _orgPersonPositionService = new OrganizationalPersonPositionService(_adapterRegistry, _orgPersonPositionRepo, _personBaseRepo, _logger);

            orgPersonPositionsDomainObject = BuildOrgPersonPositionRepoResponse();

            var orgPersonPositionAdapter = new AutoMapperAdapter<Domain.Base.Entities.OrganizationalPersonPosition, Dtos.Base.OrganizationalPersonPosition>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.OrganizationalPersonPosition, Dtos.Base.OrganizationalPersonPosition>()).Returns(orgPersonPositionAdapter);
            var orgRelAdapter = new AutoMapperAdapter<Domain.Base.Entities.OrganizationalRelationship, Dtos.Base.OrganizationalRelationship>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.OrganizationalRelationship, Dtos.Base.OrganizationalRelationship>()).Returns(orgRelAdapter);
            criteriaIds = new List<string> { "OPP1" };
            searchString = "P1";
            personBaseObjects = BuildPersonBaseRepoResponse();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _orgPersonPositionRepoMock = null;
            _orgPersonPositionRepo = null;
        }

        private List<OrganizationalPersonPosition> BuildOrgPersonPositionRepoResponse()
        {
            var orgPositions = new List<Domain.Base.Entities.OrganizationalPersonPosition>();
            var orgPosition1 = new OrganizationalPersonPosition("OPP1", "P1", "POS1", "Position1", null, null);
            var orgPosition2 = new OrganizationalPersonPosition("OPP2", "P2", "POS2", "Position2", null, null);
            var relationship = new OrganizationalRelationship("1", "OPP1", "P1", "POS1", "Position1", null, null, "OPP2", "P2", "POS2", "Position2", null, null, "Manager");
            orgPosition1.AddRelationship(relationship);
            orgPositions.Add(orgPosition1);
            orgPositions.Add(orgPosition2);
            return orgPositions;
        }

        private List<PersonBase> BuildPersonBaseRepoResponse()
        {
            var personBase = new List<PersonBase>();
            personBase.Add(new PersonBase("P1", "Smith") { PreferredName = "Smith" });
            personBase.Add(new PersonBase("P2", "Johnson") { PreferredName = "Johnson" });
            return personBase;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetOrganizationalPersonPositionsAsync_CriteriaNull_ThrowsException()
        {
            var orgPerPositionsDto = await _orgPersonPositionService.QueryOrganizationalPersonPositionAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetOrganizationalPersonPositionsAsync_NoCriteria_ThrowsException()
        {
            var criteria = new Dtos.Base.OrganizationalPersonPositionQueryCriteria() { Ids = new List<string>(), SearchString = string.Empty };
            var orgPerPositionsDto = await _orgPersonPositionService.QueryOrganizationalPersonPositionAsync(criteria);
        }

        [TestMethod]
        public async Task GetOrganizationalPersonPositionsAsync_OnlyCriteriaIds_ReturnsResults()
        {
            _orgPersonPositionRepoMock.Setup(repo => repo.GetOrganizationalPersonPositionsByIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(orgPersonPositionsDomainObject);
            _personBaseRepoMock.Setup(repo => repo.GetPersonsBaseAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ReturnsAsync(personBaseObjects);
            var criteria = new Dtos.Base.OrganizationalPersonPositionQueryCriteria() { Ids = criteriaIds, SearchString = null };
            var orgPerPositionsDto = await _orgPersonPositionService.QueryOrganizationalPersonPositionAsync(criteria);
            var personName = string.Empty;
            Assert.AreEqual(orgPersonPositionsDomainObject.Count(), orgPerPositionsDto.Count());
            for (int i = 0; i < orgPersonPositionsDomainObject.Count(); i++)
            {
                var orgPersonPositionDto = orgPerPositionsDto.Where(osdto => osdto.Id == orgPersonPositionsDomainObject.ElementAt(i).Id).First();
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Id, orgPersonPositionDto.Id);
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).PersonId, orgPersonPositionDto.PersonId);
                personName = personBaseObjects.Where(p => p.Id == orgPersonPositionDto.PersonId).First().PreferredName;
                Assert.AreEqual(personName, orgPersonPositionDto.PersonName);
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).PositionId, orgPersonPositionDto.PositionId);
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).PositionTitle, orgPersonPositionDto.PositionTitle);
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.Count(), orgPersonPositionDto.Relationships.Count());
                for (int j = 0; j < orgPersonPositionsDomainObject.ElementAt(i).Relationships.Count(); j++)
                {
                    var orgPositionRelDto = orgPersonPositionDto.Relationships.Where(osdtor => osdtor.Id == orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).Id).First();
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).OrganizationalPersonPositionId, orgPositionRelDto.OrganizationalPersonPositionId);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).RelatedOrganizationalPersonPositionId, orgPositionRelDto.RelatedOrganizationalPersonPositionId);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).Category, orgPositionRelDto.Category);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).PersonId, orgPositionRelDto.PersonId);
                    var relationshipPrimaryPersonName = personBaseObjects.Where(p => p.Id == orgPersonPositionDto.Relationships.ElementAt(j).PersonId).First().PreferredName;
                    Assert.AreEqual(relationshipPrimaryPersonName, orgPositionRelDto.PersonName);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).PositionId, orgPositionRelDto.PositionId);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).PositionTitle, orgPositionRelDto.PositionTitle);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).RelatedPersonId, orgPositionRelDto.RelatedPersonId);
                    var relationshipSecondaryPersonName = personBaseObjects.Where(p => p.Id == orgPersonPositionDto.Relationships.ElementAt(j).RelatedPersonId).First().PreferredName;
                    Assert.AreEqual(relationshipSecondaryPersonName, orgPositionRelDto.RelatedPersonName);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).RelatedPositionId, orgPositionRelDto.RelatedPositionId);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).RelatedPositionTitle, orgPositionRelDto.RelatedPositionTitle);
                }
            }
        }

        [TestMethod]
        public async Task GetOrganizationalPersonPositionsAsync_JustSearchString_ReturnsResults()
        {
            _orgPersonPositionRepoMock.Setup(repo => repo.GetOrganizationalPersonPositionAsync(It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(orgPersonPositionsDomainObject);
            _personBaseRepoMock.Setup(repo => repo.SearchByIdsOrNamesAsync(new List<string>(), searchString, true)).ReturnsAsync(personBaseObjects);
            _personBaseRepoMock.Setup(repo => repo.GetPersonsBaseAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ReturnsAsync(personBaseObjects);
            var criteria = new Dtos.Base.OrganizationalPersonPositionQueryCriteria() { Ids = new List<string>(), SearchString = searchString };
            var orgPerPositionsDto = await _orgPersonPositionService.QueryOrganizationalPersonPositionAsync(criteria);
            var personName = string.Empty;
            Assert.AreEqual(orgPersonPositionsDomainObject.Count(), orgPerPositionsDto.Count());
            for (int i = 0; i < orgPersonPositionsDomainObject.Count(); i++)
            {
                var orgPersonPositionDto = orgPerPositionsDto.Where(osdto => osdto.Id == orgPersonPositionsDomainObject.ElementAt(i).Id).First();
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Id, orgPersonPositionDto.Id);
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).PersonId, orgPersonPositionDto.PersonId);
                personName = personBaseObjects.Where(p => p.Id == orgPersonPositionDto.PersonId).First().PreferredName;
                Assert.AreEqual(personName, orgPersonPositionDto.PersonName);
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).PositionId, orgPersonPositionDto.PositionId);
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).PositionTitle, orgPersonPositionDto.PositionTitle);
                Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.Count(), orgPersonPositionDto.Relationships.Count());
                for (int j = 0; j < orgPersonPositionsDomainObject.ElementAt(i).Relationships.Count(); j++)
                {
                    var orgPositionRelDto = orgPersonPositionDto.Relationships.Where(osdtor => osdtor.Id == orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).Id).First();
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).OrganizationalPersonPositionId, orgPositionRelDto.OrganizationalPersonPositionId);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).RelatedOrganizationalPersonPositionId, orgPositionRelDto.RelatedOrganizationalPersonPositionId);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).Category, orgPositionRelDto.Category);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).PersonId, orgPositionRelDto.PersonId);
                    var relationshipPrimaryPersonName = personBaseObjects.Where(p => p.Id == orgPersonPositionDto.Relationships.ElementAt(j).PersonId).First().PreferredName;
                    Assert.AreEqual(relationshipPrimaryPersonName, orgPositionRelDto.PersonName);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).PositionId, orgPositionRelDto.PositionId);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).PositionTitle, orgPositionRelDto.PositionTitle);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).RelatedPersonId, orgPositionRelDto.RelatedPersonId);
                    var relationshipSecondaryPersonName = personBaseObjects.Where(p => p.Id == orgPersonPositionDto.Relationships.ElementAt(j).RelatedPersonId).First().PreferredName;
                    Assert.AreEqual(relationshipSecondaryPersonName, orgPositionRelDto.RelatedPersonName);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).RelatedPositionId, orgPositionRelDto.RelatedPositionId);
                    Assert.AreEqual(orgPersonPositionsDomainObject.ElementAt(i).Relationships.ElementAt(j).RelatedPositionTitle, orgPositionRelDto.RelatedPositionTitle);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetOrganizationalPersonPositionByIdAsync_IdIsNull_ThrowsException()
        {
            var orgPerPos = await _orgPersonPositionService.GetOrganizationalPersonPositionByIdAsync(null);
        }

        [TestMethod]
        public async Task GetOrganizationalPersonPositionByIdAsync_IdIsValid_ReturnsDto()
        {
            var orgPersonPosition1 = new Domain.Base.Entities.OrganizationalPersonPosition("OPP1", "P1", "POS1", "Position1", null, null);
            var relationship = new OrganizationalRelationship("1", "OPP1", "P1", "POS1", "Position1", null, null, "OPP2", "P2", "POS2", "Position2", null, null, "Manager");
            orgPersonPosition1.AddRelationship(relationship);
            var orgPersonPositionRepoResponse = new List<Domain.Base.Entities.OrganizationalPersonPosition> { orgPersonPosition1 };
            _orgPersonPositionRepoMock.Setup(repo => repo.GetOrganizationalPersonPositionsByIdsAsync(new List<string> { "OPP1" })).ReturnsAsync(orgPersonPositionRepoResponse);
            _personBaseRepoMock.Setup(repo => repo.GetPersonsBaseAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ReturnsAsync(personBaseObjects);

            var orgPerPositionDto = await _orgPersonPositionService.GetOrganizationalPersonPositionByIdAsync("OPP1");
            Assert.AreEqual(orgPersonPosition1.Id, orgPerPositionDto.Id);
            Assert.AreEqual(orgPersonPosition1.PersonId, orgPerPositionDto.PersonId);
            Assert.AreEqual(orgPersonPosition1.PositionTitle, orgPerPositionDto.PositionTitle);
            Assert.AreEqual(orgPersonPosition1.Relationships.Count, orgPerPositionDto.Relationships.Count());

        }
    }
}
