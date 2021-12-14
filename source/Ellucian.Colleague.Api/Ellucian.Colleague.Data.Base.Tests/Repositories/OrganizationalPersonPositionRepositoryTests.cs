// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OrgRole = Ellucian.Data.Colleague.DataContracts.OrgRole;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass, System.Runtime.InteropServices.GuidAttribute("F3D52205-4914-41B6-83D2-7D6636D4105A")]
    public class OrganizationalPersonPositionRepositoryTests : BaseRepositorySetup
    {
        public OrganizationalPersonPositionRepository repository;
        List<OrgEntityRolePosition> orgEntityRoleResponse;
        List<OrgRole> orgRoleResponse;
        List<string> personIds;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            repository = BuildOrganizationalPersonPositionRepository();
            personIds = new List<string>() { "P1", "P2" };
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }


        private OrganizationalPersonPositionRepository BuildOrganizationalPersonPositionRepository()
        {
            // Mock org role types valcode response
            var orgRoleTypes = new ApplValcodes()
            {
                ValInternalCode = new List<string>() { "SS1", "ORG2", "ORG3", "POR4" },
                ValExternalRepresentation = new List<string>() { "SS Role Type 1", "Org Role Type 2", "Org Role Type3", "Portal Role Type 4" },
                ValActionCode1 = new List<string>() { "", "HR", "HR", "" }
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

                //new OrgRole() {Recordkey = "R1", OroleTitle = "SS Role 1"}, 
                new OrgRole() {Recordkey = "R2", OroleTitle = "Position 2"},
                new OrgRole() {Recordkey = "R3", OroleTitle = "Position 3"},
                new OrgRole() {Recordkey = "R4", OroleTitle = "Position 4"},
                new OrgRole() {Recordkey = "R5", OroleTitle = "Position 5"},
            };
            var keys = orgRoleResponse.Select(orr => orr.Recordkey).ToArray();
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(keys);
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgRole>(keys, It.IsAny<bool>())).ReturnsAsync(new Collection<OrgRole>(orgRoleResponse));

            // Mock OrgRolePosition bulkread response
            orgEntityRoleResponse = new List<OrgEntityRolePosition>()
            {
                new OrgEntityRolePosition() {Recordkey = "RS1", OerOrgRole = "R1", OerPerson = "P1"}, // ignored--excluded role
                new OrgEntityRolePosition() {Recordkey = "RS2", OerOrgRole = "R2", OerPerson = "P1"}, // included
                new OrgEntityRolePosition() {Recordkey = "RS3", OerOrgRole = "R3", OerPerson = "P2"}, // included
                new OrgEntityRolePosition() {Recordkey = "RS4", OerOrgRole = "R4", OerPerson = "P2"}, // included
                new OrgEntityRolePosition() {Recordkey = "RS5", OerOrgRole = "R5", OerPerson = "P2"}, // included
            };
            keys = orgEntityRoleResponse.Select(ors => ors.Recordkey).ToArray();
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(keys);
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRolePosition>(keys, It.IsAny<bool>())).ReturnsAsync(new Collection<OrgEntityRolePosition>(orgEntityRoleResponse));
            //Also mock for a simple id - based bulk read for the read of the related ORG.ENTITY.ROLE

            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(keys);
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRolePosition>(keys, It.IsAny<bool>())).ReturnsAsync(new Collection<OrgEntityRolePosition>(orgEntityRoleResponse));

            // Initialize the repository
            var orgStructureRepo = new OrganizationalPersonPositionRepository(cacheProvider, transFactory, logger, apiSettings);
            return orgStructureRepo;
        }


        [TestMethod]
        public async Task GetOrgPersonPositionAsync_WithoutRelationships()
        {
            // No relationships selected

            var orgPersonPositions = await repository.GetOrganizationalPersonPositionAsync(personIds, new List<string>());

            Assert.AreEqual(4, orgPersonPositions.Count());
            foreach (var role in orgRoleResponse)
            {
                var personPositions = orgPersonPositions.Where(sl => sl.PositionId == role.Recordkey);
                foreach (var item in personPositions)
                {
                    var rolePositionRecord = orgEntityRoleResponse.Where(orsr => orsr.Recordkey == item.Id).FirstOrDefault();
                    Assert.AreEqual(rolePositionRecord.Recordkey, item.Id);
                    Assert.AreEqual(rolePositionRecord.OerOrgRole, item.PositionId);
                    Assert.AreEqual(orgRoleResponse.Where(or => or.Recordkey == item.PositionId).FirstOrDefault().OroleTitle, item.PositionTitle);
                    Assert.AreEqual(0, item.Relationships.Count());
                }
            }
        }

        [TestMethod]
        public async Task GetOrgPersonPositionAsync_HasRelationship()
        {
            // One management relationship for RS2
            List<OrgEntityRoleRel> orgEntityRoleRelResponse = new List<OrgEntityRoleRel>()
            {
                new OrgEntityRoleRel() {Recordkey = "RSR1", OerrelOerId = "RS2", OerrelRelatedOerId = "RS3", OerrelRelationshipCategory = "Manager"}, // Added--RS2 REL #1
            };

            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE.REL", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(orgEntityRoleRelResponse.Select(rsrr => rsrr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRoleRel>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<OrgEntityRoleRel>(orgEntityRoleRelResponse));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRolePosition>(new string[] { "RS3", "RS2" }, true)).ReturnsAsync(new Collection<OrgEntityRolePosition>(orgEntityRoleResponse.Where(oer => oer.Recordkey == "RS2" || oer.Recordkey == "RS3").ToList()));

            var orgPersonPositions = await repository.GetOrganizationalPersonPositionAsync(personIds, new List<string>());

            var relationship = orgPersonPositions.Where(s => s.Id == "RS2").FirstOrDefault().Relationships;
            Assert.AreEqual(1, relationship.Count());
            Assert.AreEqual(orgEntityRoleRelResponse.ElementAt(0).Recordkey, relationship.ElementAt(0).Id);
            Assert.AreEqual(orgEntityRoleRelResponse.ElementAt(0).OerrelOerId, relationship.ElementAt(0).OrganizationalPersonPositionId);
            Assert.AreEqual(orgEntityRoleRelResponse.ElementAt(0).OerrelRelatedOerId, relationship.ElementAt(0).RelatedOrganizationalPersonPositionId);
            Assert.AreEqual(orgEntityRoleRelResponse.ElementAt(0).OerrelRelationshipCategory, relationship.ElementAt(0).Category);
        }

        [TestMethod]
        public async Task GetOrgPersonPositionAsync_OnlyOneRelationshipPerCategory()
        {
            List<OrgEntityRoleRel> OrgEntityRoleRelResponse = new List<OrgEntityRoleRel>()
            {
                new OrgEntityRoleRel() {Recordkey = "RSR1", OerrelOerId = "RS2", OerrelRelatedOerId = "RS3", OerrelRelationshipCategory = "Manager"}, // Added--RS2 REL #1
                new OrgEntityRoleRel() {Recordkey = "RSR2", OerrelOerId = "RS2", OerrelRelatedOerId = "RS4", OerrelRelationshipCategory = "Manager"}, // Ignored--dup category                                     // Ignored--not S type relationship
            };
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE.REL", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(OrgEntityRoleRelResponse.Select(rsrr => rsrr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRoleRel>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<OrgEntityRoleRel>(OrgEntityRoleRelResponse));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRolePosition>(new string[] { "RS3", "RS4", "RS2" }, true)).ReturnsAsync(new Collection<OrgEntityRolePosition>(orgEntityRoleResponse.Where(oer => oer.Recordkey == "RS2" || oer.Recordkey == "RS3" || oer.Recordkey == "RS4").ToList()));

            var orgPersonPositions = await repository.GetOrganizationalPersonPositionAsync(personIds, new List<string>());

            var oppsFound = orgPersonPositions.Where(s => s.Id == "RS2");

            Assert.AreEqual(1, oppsFound.Count());
            Assert.AreEqual(1, oppsFound.ElementAt(0).Relationships.Count());
        }

        [TestMethod]
        public async Task GetOrgPersonPositionAsync_ByPersonId_SupervisorAndSubordinatesReturned()
        {
            List<OrgEntityRoleRel> OrgEntityRoleRelResponse = new List<OrgEntityRoleRel>()
            {
                new OrgEntityRoleRel() {Recordkey = "RSR1", OerrelOerId = "RS2", OerrelRelatedOerId = "RS3", OerrelRelationshipCategory = "Manager"}, // Added--RS2 REL #1
                new OrgEntityRoleRel() {Recordkey = "RSR2", OerrelOerId = "RS2", OerrelRelatedOerId = "RS4", OerrelRelationshipCategory = "Approver"}, // Added RS2 REL #2 different category
                new OrgEntityRoleRel() {Recordkey = "RSR3", OerrelOerId = "RS5", OerrelRelatedOerId = "RS2", OerrelRelationshipCategory = "Manager"}, // Added RS5, subordinate of RS2

            };
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE.REL", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(OrgEntityRoleRelResponse.Select(rsrr => rsrr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRoleRel>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<OrgEntityRoleRel>(OrgEntityRoleRelResponse));

            var keys = new List<string>() { "RS2" }.ToArray();
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(keys);
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRolePosition>(keys, It.IsAny<bool>())).ReturnsAsync(new Collection<OrgEntityRolePosition>(orgEntityRoleResponse.Where(oer => oer.Recordkey == "RS2").ToList()));


            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRolePosition>(It.Is<string[]>(x => x.Contains("RS5")), true)).ReturnsAsync(new Collection<OrgEntityRolePosition>(orgEntityRoleResponse));


            var result = await repository.GetOrganizationalPersonPositionAsync(new List<string> { "P1" }, new List<string>());

            Assert.AreEqual(3, result.Where(s => s.Id == "RS2").FirstOrDefault().Relationships.Count());

        }

        [TestMethod]
        public async Task GetOrgPersonPositionAsync_ByOrgEntityRoleId_SupervisorAndSubordinatesReturned()
        {
            List<OrgEntityRoleRel> OrgEntityRoleRelResponse = new List<OrgEntityRoleRel>()
            {
                new OrgEntityRoleRel() {Recordkey = "RSR1", OerrelOerId = "RS2", OerrelRelatedOerId = "RS3", OerrelRelationshipCategory = "Manager"}, // Added--RS2 REL #1
                new OrgEntityRoleRel() {Recordkey = "RSR2", OerrelOerId = "RS2", OerrelRelatedOerId = "RS4", OerrelRelationshipCategory = "Approver"}, // Added RS2 REL #2 different category
                new OrgEntityRoleRel() {Recordkey = "RSR3", OerrelOerId = "RS5", OerrelRelatedOerId = "RS2", OerrelRelationshipCategory = "Manager"}, // Added RS5, subordinate of RS2

            };
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE.REL", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(OrgEntityRoleRelResponse.Select(rsrr => rsrr.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRoleRel>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<OrgEntityRoleRel>(OrgEntityRoleRelResponse));

            var keys = new List<string>() { "RS2" }.ToArray();
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(keys);
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRolePosition>(keys, It.IsAny<bool>())).ReturnsAsync(new Collection<OrgEntityRolePosition>(orgEntityRoleResponse.Where(oer => oer.Recordkey == "RS2").ToList()));


            dataReaderMock.Setup(r => r.BulkReadRecordAsync<OrgEntityRolePosition>(It.Is<string[]>(x => x.Contains("RS5")), true)).ReturnsAsync(new Collection<OrgEntityRolePosition>(orgEntityRoleResponse));


            var result = await repository.GetOrganizationalPersonPositionAsync(new List<string>(), new List<string> { "RS2" });

            Assert.AreEqual(3, result.Where(s => s.Id == "RS2").FirstOrDefault().Relationships.Count());

        }

        [TestMethod]
        public async Task GetOrgPersonPositionAsync_NullOrgRoleReponse_ReturnsEmptyList()
        {
            // Mock null return for ORG.ROLE select
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(() => null);

            var result = await repository.GetOrganizationalPersonPositionAsync(personIds, new List<string>());

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetOrgPersonPositionAsync_NullOrgEntityRoleReponse_ReturnsEmptyList()
        {
            // Mock null return for ORG.ENTITY.ROLE select
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, 425)).ReturnsAsync(() => null);
            dataReaderMock.Setup(r => r.SelectAsync("ORG.ENTITY.ROLE", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(() => null);

            var orgPerPos = await repository.GetOrganizationalPersonPositionAsync(personIds, new List<string>());

            Assert.AreEqual(0, orgPerPos.Count());
        }
    }
}
