// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using OrgRole = Ellucian.Colleague.Data.Base.DataContracts.OrgRole;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class OrganizationalPositionRepositoryTests : BaseRepositorySetup
    {
        private OrganizationalPositionRepository organizationalPositionRepo;
        private List<OrgRole> orgRoleResponse;
        private List<RoleRelationships> roleRelationshipsResponse;
        private string primaryCriteria;
        private string secondaryCriteria;
        private string roleRelationshipsFile;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            organizationalPositionRepo = BuildOrganizationalPositionRepository();
        }

        private OrganizationalPositionRepository BuildOrganizationalPositionRepository()
        {
            // Mock org role types valcode response
            var orgRoleTypes = new ApplValcodes()
            {
                ValInternalCode = new List<string>() { "SS1", "ORG2", "ORG3", "POR4" },
                ValExternalRepresentation = new List<string>() { "SS Role Type 1", "Org Role Type 2", "Org Role Type3", "Portal Role Type 4" },
                ValActionCode1 = new List<string>() { "", "ORG", "ORG", "" }
            };
            orgRoleTypes.ValsEntityAssociation = new List<ApplValcodesVals>();
            for (int i = 0; i < orgRoleTypes.ValInternalCode.Count(); i++)
            {
                orgRoleTypes.ValsEntityAssociation.Add(new ApplValcodesVals(orgRoleTypes.ValInternalCode.ElementAt(i),
                    orgRoleTypes.ValExternalRepresentation.ElementAt(i),
                    orgRoleTypes.ValActionCode1.ElementAt(i),
                    orgRoleTypes.ValInternalCode.ElementAt(i),
                    "", "", ""));
            }
            MockRecordAsync<ApplValcodes>("UT.VALCODES", orgRoleTypes, null);


            // Mock OrgRole bulkread response
            orgRoleResponse = new List<OrgRole>()
            {
                new OrgRole() {Recordkey = "0000012", OroleTitle = "NOT A POSITION 2", OroleType = "SS1" },
                new OrgRole() {Recordkey = "0000013", OroleTitle = "NOT A POSITION 3", OroleType = "POR4" },
                new OrgRole() {Recordkey = "0000014", OroleTitle = "POSITION 4", OroleType = "ORG2" },
                new OrgRole() {Recordkey = "0000015", OroleTitle = "POSITION 5", OroleType = "ORG3" },
            };
            roleRelationshipsResponse = new List<RoleRelationships>()
            {
                new RoleRelationships() {
                    Recordkey = "RR1",
                    RrlsOrgRole = "0000014",
                    RrlsRelatedOrgRole = "0000015",
                    RrlsType = "S",
                    RrlsRelationshipCategory = "MGR",
                    RrlsStartDate = DateTime.MinValue,
                    RrlsEndDate = null
                },
            };
            roleRelationshipsFile = "ROLE.RELATIONSHIPS";
            primaryCriteria = "RRLS.TYPE EQ 'S' AND RRLS.ORG.ROLE EQ '?'";
            secondaryCriteria = "RRLS.TYPE EQ 'S' AND RRLS.RELATED.ORG.ROLE EQ '?'";
            var keys = orgRoleResponse.Select(orr => orr.Recordkey).ToArray();
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgRole>(keys, It.IsAny<bool>())).ReturnsAsync(new Collection<OrgRole>(orgRoleResponse));
            MockRecordsAsync<OrgRole>("ORG.ROLE", orgRoleResponse.ToDictionary(or => or.Recordkey));
            MockRecordsAsync<RoleRelationships>("ROLE.RELATIONSHIPS", roleRelationshipsResponse.ToDictionary(rr => rr.Recordkey));

            var repo = new OrganizationalPositionRepository(cacheProvider, transFactory, logger, apiSettings);
            return repo;
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetOrganizationalPositionsByIdsAsync_OnNullIds_ThrowsException()
        {
            var response = await organizationalPositionRepo.GetOrganizationalPositionsByIdsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetOrganizationalPositionsByIdsAsync_OnEmptyIds_ThrowsException()
        {
            var response = await organizationalPositionRepo.GetOrganizationalPositionsByIdsAsync(new List<string>());
        }

        [TestMethod]
        public async Task GetOrganizationalPositionsByIdsAsync_OnSingleIdThatIsAPosition_ReturnsPosition()
        {
            var ids = new List<string> { "0000014" };
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, primaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, secondaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(new string[0]);
            var response = await organizationalPositionRepo.GetOrganizationalPositionsByIdsAsync(ids);
            Assert.AreEqual(1, response.Count());
            foreach (var pos in response)
            {
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).OroleTitle, pos.Title);
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).Recordkey, pos.Id);
                Assert.AreEqual(1, pos.Relationships.Count());
            }
        }

        [TestMethod]
        public async Task GetOrganizationalPositionsByIdsAsync_OnSingleIdThatIsNotAPosition_ReturnsNothing()
        {
            var ids = new List<string> { "0000012" };
            var response = await organizationalPositionRepo.GetOrganizationalPositionsByIdsAsync(ids);
            Assert.AreEqual(0, response.Count());
        }

        [TestMethod]
        public async Task GetOrganizationalPositionsByIdsAsync_OnMultipleIds_ReturnsPositions()
        {
            var ids = new List<string> { "0000014", "0000015" };
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, primaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, secondaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            var response = await organizationalPositionRepo.GetOrganizationalPositionsByIdsAsync(ids);
            Assert.AreEqual(2, response.Count());
            foreach (var pos in response)
            {
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).OroleTitle, pos.Title);
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).Recordkey, pos.Id);
                Assert.AreEqual(1, pos.Relationships.Count());
            }
        }

        [TestMethod]
        public async Task GetOrganizationalPositionsByIdsAsync_OnMultipleIdsContainingSomeNonPositions_ReturnsPositions()
        {
            var ids = new List<string> { "0000013", "0000014" };
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, primaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, secondaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            var response = await organizationalPositionRepo.GetOrganizationalPositionsByIdsAsync(ids);
            Assert.AreEqual(1, response.Count());
            foreach (var pos in response)
            {
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).OroleTitle, pos.Title);
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).Recordkey, pos.Id);
                Assert.AreEqual(1, pos.Relationships.Count());
            }
        }

        [TestMethod]
        public async Task GetOrganizationalPositionsByIdsAsync_OnMultipleIdsContainingNoPositions_ReturnsNothing()
        {
            var ids = new List<string> { "0000012", "0000013" };
            var response = await organizationalPositionRepo.GetOrganizationalPositionsByIdsAsync(ids);
            Assert.AreEqual(0, response.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetOrganizationalPositionsAsync_WithNullArguments_ThrowsException()
        {
            var response = await organizationalPositionRepo.GetOrganizationalPositionsAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetOrganizationalPositionsAsync_WithEmptyArguments_ThrowsException()
        {
            var response = await organizationalPositionRepo.GetOrganizationalPositionsAsync(string.Empty, new List<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetOrganizationalPositionsAsync_WithEmptySearchAndNullIds_ThrowsException()
        {
            var response = await organizationalPositionRepo.GetOrganizationalPositionsAsync(string.Empty, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetOrganizationalPositionsAsync_WithNullSearchAndEmptyIds_ThrowsException()
        {
            var response = await organizationalPositionRepo.GetOrganizationalPositionsAsync(null, new List<string>());
        }

        [TestMethod]
        public async Task GetOrganizationalPositionsAsync_WithSearchCriteria_ReturnsMatchingPositions()
        {
            var searchCriteria = "POSITION";
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425))
                .ReturnsAsync(orgRoleResponse.Where(orr => orr.OroleTitle.Contains(searchCriteria)).Select(orr => orr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, primaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, secondaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            var response = await organizationalPositionRepo.GetOrganizationalPositionsAsync(searchCriteria, new List<string>());
            Assert.AreEqual(2, response.Count());
            foreach (var pos in response)
            {
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).OroleTitle, pos.Title);
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).Recordkey, pos.Id);
                Assert.AreEqual(1, pos.Relationships.Count());
            }
        }

        [TestMethod]
        public async Task GetOrganizationalPositionsAsync_WithSearchCriteriaForNonPositionRoles_ReturnsNothing()
        {
            var searchCriteria = "NOT A";
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425))
                .ReturnsAsync(orgRoleResponse.Where(orr => orr.OroleTitle.Contains(searchCriteria)).Select(orr => orr.Recordkey).ToArray());
            var response = await organizationalPositionRepo.GetOrganizationalPositionsAsync(searchCriteria, new List<string>());
            Assert.AreEqual(0, response.Count());
        }

        [TestMethod]
        public async Task GetOrganizationalPositionsAsync_WithSearchCriteriaAsId_ReturnsMatchingPositions()
        {
            var searchCriteria = "0000014";
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425))
                .ReturnsAsync(orgRoleResponse.Where(orr => orr.OroleTitle.Contains(searchCriteria)).Select(orr => orr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, primaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.SelectAsync(roleRelationshipsFile, secondaryCriteria, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(roleRelationshipsResponse.Select(rr => rr.Recordkey).ToArray());
            var response = await organizationalPositionRepo.GetOrganizationalPositionsAsync(searchCriteria, new List<string>());
            Assert.AreEqual(1, response.Count());
            foreach (var pos in response)
            {
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).OroleTitle, pos.Title);
                Assert.AreEqual(orgRoleResponse.First(orr => orr.Recordkey == pos.Id).Recordkey, pos.Id);
                Assert.AreEqual(1, pos.Relationships.Count());
            }
        }

    }
}
