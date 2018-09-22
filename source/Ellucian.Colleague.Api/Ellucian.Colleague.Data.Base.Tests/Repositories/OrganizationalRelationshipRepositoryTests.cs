using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class OrganizationalRelationshipRepositoryTests : BaseRepositorySetup
    {
        List<OrgEntityRoleRel> roleRels;
        List<OrgEntityRoleRel> allOrgEntityRoleRels;
        ApplValcodes relationshipCategories;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            // Mock read of RELATIONSHIP.CATEGORIES valcode
            relationshipCategories = new ApplValcodes()
            {
                ValInternalCode = new List<string>() { "MGR", "OTH" },
                ValExternalRepresentation = new List<string>() { "Manager", "Other" },
                ValActionCode1 = new List<string>() { "ORG", "" },
                ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "MGR",
                            ValExternalRepresentationAssocMember = "Manager",
                            ValActionCode1AssocMember = "ORG"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "OTH",
                            ValExternalRepresentationAssocMember = "Other",
                            ValActionCode1AssocMember = ""
                        },
                    }
            };
            dataReaderMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "RELATIONSHIP.CATEGORIES", true)).ReturnsAsync(relationshipCategories);

            // Mock read of ORG.ENTITY.ROLE.REL. Overridden in some tests.
            roleRels = new List<OrgEntityRoleRel>();
            roleRels.Add(new OrgEntityRoleRel { Recordkey = "RR1", OerrelOerId = "RS1", OerrelRelatedOerId = "RS2", OerrelRelationshipCategory = "MGR" });
            roleRels.Add(new OrgEntityRoleRel { Recordkey = "RR2", OerrelOerId = "RS2", OerrelRelatedOerId = "RS3", OerrelRelationshipCategory = "MGR" });
            roleRels.Add(new OrgEntityRoleRel { Recordkey = "RR3", OerrelOerId = "RS3", OerrelRelatedOerId = "RS4", OerrelRelationshipCategory = "MGR" });
            MockRecordsAsync<OrgEntityRoleRel>("ORG.ENTITY.ROLE.REL", roleRels);
        }


        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        [TestMethod]
        public async Task GetOrgRelationshipAsync_WithRelationships()
        {
            var repository = BuildOrganizationalRelationshipRepository();

            var actualRelationships = await repository.GetAsync(new List<string> { "RR1", "RR2", "RR3" });

            Assert.AreEqual(3, actualRelationships.Count());
            foreach (var relationship in actualRelationships)
            {
                var roleRelRecord = roleRels.Where(rr => rr.Recordkey == relationship.Id).FirstOrDefault();
                Assert.AreEqual(roleRelRecord.Recordkey, relationship.Id);
                Assert.AreEqual(roleRelRecord.OerrelOerId, relationship.OrganizationalPersonPositionId);
                Assert.AreEqual(roleRelRecord.OerrelRelatedOerId, relationship.RelatedOrganizationalPersonPositionId);
                Assert.AreEqual(roleRelRecord.OerrelRelationshipCategory, relationship.Category);
            }

        }

        [TestMethod]
        public async Task UpdateOrganizationalRelationshipAsync_Succeeds()
        {
            var relationshipToUpdate = new OrganizationalRelationship("RR123", "RS45", "0005678", "P2", "OtherTitle", null, null, "RS67", "0001234", "P1", "Title", null, null, "MGR");
            var transactionResponse = new UpdateOrgEntityRoleRelMultiResponse()
            {
                ErrorOccurred = "0",
                Message = ""
            };
            transManagerMock.Setup(ti => ti.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(It.IsAny<UpdateOrgEntityRoleRelMultiRequest>()))
                .ReturnsAsync(transactionResponse)
                .Callback(() =>
                {
                    var recordUpdate = new OrgEntityRoleRel
                    {
                        Recordkey = "RR123",
                        OerrelOerId = "RS45",
                        OerrelRelatedOerId = "RS67",
                        OerrelRelationshipCategory = "MGR"
                    };
                    MockRecordsAsync<OrgEntityRoleRel>("ORG.ENTITY.ROLE.REL", new List<OrgEntityRoleRel> { recordUpdate });
                });
            var repository = BuildOrganizationalRelationshipRepository();

            var updatedRelationship = await repository.UpdateAsync(relationshipToUpdate);
            Assert.AreEqual(relationshipToUpdate.Id, updatedRelationship.Id);
            Assert.AreEqual(relationshipToUpdate.OrganizationalPersonPositionId, updatedRelationship.OrganizationalPersonPositionId);
            Assert.AreEqual(relationshipToUpdate.RelatedOrganizationalPersonPositionId, updatedRelationship.RelatedOrganizationalPersonPositionId);
            Assert.AreEqual(relationshipToUpdate.Category, updatedRelationship.Category);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UpdateOrganizationalRelationshipAsync_Fails()
        {
            var relationshipToUpdate = new OrganizationalRelationship("RR123", "RS45", "0005678", "P2", "OtherTitle", null, null, "RS67", "0001234", "P1", "Title", null, null, "MGR");
            var transactionResponse = new UpdateOrgEntityRoleRelMultiResponse()
            {
                ErrorOccurred = "1",
                Message = "Failure"
            };
            transManagerMock.Setup(ti => ti.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(It.IsAny<UpdateOrgEntityRoleRelMultiRequest>()))
                .ReturnsAsync(transactionResponse);
            var repository = BuildOrganizationalRelationshipRepository();

            var updatedRelationship = await repository.UpdateAsync(relationshipToUpdate);
        }

        [TestMethod]
        public async Task UpdateOrganizationalRelationshipAsync_HasExistingSupervisor_SameOERRId_Succeeds()
        {
            // Does NOT throw a exception if trying to update a supervisor of an existing record
            var relationshipToUpdate = new OrganizationalRelationship("RR123", "OER1", "OER2", "MGR");
            BuildOrganizationalRelationshipChain();
            SetSuccessfulUpdateTransactionMock();
            var repository = BuildOrganizationalRelationshipRepository();

            var updatedRelationship = await repository.UpdateAsync(relationshipToUpdate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UpdateOrganizationalRelationshipAsync_CircularSuperior_Fails()
        {
            // Throws an exception if trying to add a superior who exists further down the chain
            var relationshipToUpdate = new OrganizationalRelationship("RR3", "OER2", "OER1", "MGR");
            BuildOrganizationalRelationshipChain();
            var repository = BuildOrganizationalRelationshipRepository();

            var updatedRelationship = await repository.UpdateAsync(relationshipToUpdate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateOrganizationalRelationshipAsync_HasExistingSupervisor_Fails()
        {
            // Throws an exception if trying to add a supervisor record for a person who already has a
            // supervisor of the given category
            var relationshipToUpdate = new OrganizationalRelationship("", "OER1", "OER2", "MGR");
            BuildOrganizationalRelationshipChain();
            var repository = BuildOrganizationalRelationshipRepository();

            var updatedRelationship = await repository.AddAsync(relationshipToUpdate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateOrganizationalRelationshipAsync_CircularSuperior_Fails()
        {
            // Throws an exception if trying to add a superior who exists further down the chain
            var relationshipToUpdate = new OrganizationalRelationship("", "OER5", "OER0", "MGR");
            BuildOrganizationalRelationshipChain();
            var repository = BuildOrganizationalRelationshipRepository();

            var updatedRelationship = await repository.AddAsync(relationshipToUpdate);
        }


        [TestMethod]
        public async Task CreateOrganizationalRelationshipAsync_Succeeds()
        {
            var expectedIdAfterAddition = "RR999";
            var expectedCategoryAfterAddition = "MGR";
            var relationshipToAdd = new OrganizationalRelationship("", "RS88", "RS99", "");
            var transactionResponse = new UpdateOrgEntityRoleRelMultiResponse()
            {
                ErrorOccurred = "0",
                Message = "",
                OrgEntityRoleRelId = expectedIdAfterAddition
            };
            transManagerMock.Setup(ti => ti.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(It.IsAny<UpdateOrgEntityRoleRelMultiRequest>()))
                .ReturnsAsync(transactionResponse)
                .Callback(() =>
                {
                    var recordUpdate = new OrgEntityRoleRel
                    {
                        Recordkey = "RR999",
                        OerrelOerId = "RS88",
                        OerrelRelatedOerId = "RS99",
                        OerrelRelationshipCategory = "MGR"
                    };
                    MockRecordsAsync<OrgEntityRoleRel>("ORG.ENTITY.ROLE.REL", new List<OrgEntityRoleRel> { recordUpdate });

                });
            var repository = BuildOrganizationalRelationshipRepository();

            var addedRelationship = await repository.AddAsync(relationshipToAdd);

            Assert.AreEqual(expectedIdAfterAddition, addedRelationship.Id);
            Assert.AreEqual(relationshipToAdd.OrganizationalPersonPositionId, addedRelationship.OrganizationalPersonPositionId);
            Assert.AreEqual(relationshipToAdd.RelatedOrganizationalPersonPositionId, addedRelationship.RelatedOrganizationalPersonPositionId);
            Assert.AreEqual(expectedCategoryAfterAddition, addedRelationship.Category);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateOrganizationalRelationshipAsync_Fail()
        {
            var relationshipToAdd = new OrganizationalRelationship("RR999", "RS88", "0006789", "P11", "OtherTitle", null, null, "RS99", "0008888", "P9", "Title", null, null, "MGR");
            var transactionResponse = new UpdateOrgEntityRoleRelMultiResponse()
            {
                ErrorOccurred = "1",
                Message = "Failure"
            };
            transManagerMock.Setup(ti => ti.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(It.IsAny<UpdateOrgEntityRoleRelMultiRequest>()))
                .ReturnsAsync(transactionResponse);
            var repository = BuildOrganizationalRelationshipRepository();

            var addedRelationship = await repository.AddAsync(relationshipToAdd);
        }


        [TestMethod]
        public async Task DeleteOrganizationalRelationshipAsync_Succeeds()
        {
            var deletedId = "RR100";
            var transactionResponse = new UpdateOrgEntityRoleRelMultiResponse()
            {
                ErrorOccurred = "0",
                Message = ""
            };
            transManagerMock.Setup(ti => ti.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(It.IsAny<UpdateOrgEntityRoleRelMultiRequest>()))
                .ReturnsAsync(transactionResponse).Verifiable();
            var repository = BuildOrganizationalRelationshipRepository();

            await repository.DeleteAsync(deletedId);

            transManagerMock.Verify(
                ti => ti.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(It.IsAny<UpdateOrgEntityRoleRelMultiRequest>()),
                Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteOrganizationalRelationshipAsync_Fails()
        {
            var deletedId = "RR100";
            var transactionResponse = new UpdateOrgEntityRoleRelMultiResponse()
            {
                ErrorOccurred = "1",
                Message = "Failure"
            };
            transManagerMock.Setup(ti => ti.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(It.IsAny<UpdateOrgEntityRoleRelMultiRequest>()))
                .ReturnsAsync(transactionResponse);
            var repository = BuildOrganizationalRelationshipRepository();

            await repository.DeleteAsync(deletedId);
        }

        private OrganizationalRelationshipRepository BuildOrganizationalRelationshipRepository()
        {
            return new OrganizationalRelationshipRepository(cacheProvider, transFactory, logger, apiSettings);
        }

        private void BuildOrganizationalRelationshipChain()
        {
            // Form a multi-role and multi-category hierarchy chain that can be searched in both directions
            allOrgEntityRoleRels = new List<OrgEntityRoleRel>();
            allOrgEntityRoleRels.Add(new OrgEntityRoleRel() { Recordkey = "RR1", OerrelOerId = "OER3", OerrelRelatedOerId = "OER5", OerrelRelationshipCategory = "MGR" });
            allOrgEntityRoleRels.Add(new OrgEntityRoleRel() { Recordkey = "RR2", OerrelOerId = "OER3", OerrelRelatedOerId = "OER4", OerrelRelationshipCategory = "OTH" });
            allOrgEntityRoleRels.Add(new OrgEntityRoleRel() { Recordkey = "RR3", OerrelOerId = "OER2", OerrelRelatedOerId = "OER3", OerrelRelationshipCategory = "MGR" });
            allOrgEntityRoleRels.Add(new OrgEntityRoleRel() { Recordkey = "RR123", OerrelOerId = "OER1", OerrelRelatedOerId = "OER2", OerrelRelationshipCategory = "MGR" });
            allOrgEntityRoleRels.Add(new OrgEntityRoleRel() { Recordkey = "RR4", OerrelOerId = "OER0", OerrelRelatedOerId = "OER1", OerrelRelationshipCategory = "MGR" });
            MockRecordsAsync<OrgEntityRoleRel>("ORG.ENTITY.ROLE.REL", allOrgEntityRoleRels);
        }

        private void SetSuccessfulUpdateTransactionMock()
        {
            var transactionResponse = new UpdateOrgEntityRoleRelMultiResponse()
            {
                ErrorOccurred = "0",
                Message = ""
            };
            transManagerMock.Setup(ti => ti.ExecuteAsync<UpdateOrgEntityRoleRelMultiRequest, UpdateOrgEntityRoleRelMultiResponse>(It.IsAny<UpdateOrgEntityRoleRelMultiRequest>()))
                .ReturnsAsync(transactionResponse)
                .Callback(() =>
                {
                    var recordUpdate = new OrgEntityRoleRel
                    {
                        Recordkey = "RR123",
                        OerrelOerId = "RS45",
                        OerrelRelatedOerId = "RS67",
                        OerrelRelationshipCategory = "MGR"
                    };
                    MockRecordsAsync<OrgEntityRoleRel>("ORG.ENTITY.ROLE.REL", new List<OrgEntityRoleRel> { recordUpdate });
                });
        }
    }
}
