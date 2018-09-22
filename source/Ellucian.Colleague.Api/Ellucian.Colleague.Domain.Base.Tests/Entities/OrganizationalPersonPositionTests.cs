// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class OrganizationalPersonPositionTests
    {
        protected string orgEntityRoleId;
        protected string positionId;
        protected string positionTitle;
        protected string category;
        protected OrganizationalPersonPosition orgPersonPosition;
        protected string relId;
        protected string personId;
        protected string relatedOerId;
        protected string relatedPersonId;
        protected string relatedPositionId;
        protected string relatedPositionTitle;

        [TestInitialize]
        public void Initialize()
        {
            orgEntityRoleId = "OER1";
            positionId = "POS1";
            positionTitle = "title1";
            category = "manager";
            relId = "R1";
            personId = "PER1";
            relatedOerId = "OER2";
            relatedPersonId = "PER2";
            relatedPositionId = "POS2";
            relatedPositionTitle = "title2";
        }
    }

    [TestClass]
    public class OrganizationalPersonPosition_Constructor : OrganizationalPersonPositionTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalPersonPosition_IdNull_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(null, personId, positionId, positionTitle, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalPersonPosition_IdEmpty_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(string.Empty, personId, positionId, positionTitle, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalPersonPosition_PersonIdNull_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleId, null, positionId, positionTitle, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalPersonPosition_PersonIdEmpty_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleId, string.Empty, positionId, positionTitle, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalPersonPosition_positionIdNull_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleId, personId, null, positionTitle, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalPersonPosition_positionTitleNull_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleId, personId, positionId, null, null, null);
        }

        [TestMethod]
        public void OrganizationalPersonPosition_Success()
        {
            orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleId, personId, positionId, positionTitle, null, null);
            Assert.AreEqual(orgEntityRoleId, orgPersonPosition.Id);
            Assert.AreEqual(positionId, orgPersonPosition.PositionId);
            Assert.AreEqual(positionTitle, orgPersonPosition.PositionTitle);
            Assert.AreEqual(0, orgPersonPosition.Relationships.Count());
        }
    }

    [TestClass]
    public class OrganizationalPersonPosition_AddRelationship : OrganizationalPersonPositionTests
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddRelationship_Relationship_Null_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleId, personId, positionId, positionTitle, null, null);
            orgPersonPosition.AddRelationship(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddRelationship_Relationship_IrrelevantPerson_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleId, personId, positionId, positionTitle, null, null);
            orgPersonPosition.AddRelationship(new OrganizationalRelationship("9", "OER8", "P8", "POS8", "Irrelevant1", null, null, "OER9", "P9", "POS9", "Irrelevant2", null, null, "manager"));
        }

        [TestMethod]
        public void AddRelationship_Multiple_Successful()
        {
            orgPersonPosition = new OrganizationalPersonPosition("OER1", "PER1", "POS1", "title1", null, null);
            var rel1 = new OrganizationalRelationship("1", "OER1", "PER1", "POS1", "title1", null, null, "OER2", "PER2", "POS2", "title2", null, null, "manager");
            orgPersonPosition.AddRelationship(rel1);
            // Same related position, different category
            var rel2 = new OrganizationalRelationship("2", "OER1", "PER1", "POS1", "title1", null, null, "OER2", "PER2", "POS2", "title2", null, null, "approver");
            orgPersonPosition.AddRelationship(rel2);
            // different related position, different category
            var rel3 = new OrganizationalRelationship("3", "OER1", "PER1", "POS1", "title1", null, null, "OER3", "PER3", "POS3", "title3", null, null, "budget");
            orgPersonPosition.AddRelationship(rel3);
            Assert.AreEqual(3, orgPersonPosition.Relationships.Count);
            // Verify that the data carried into the correct relationship fields
            Assert.AreEqual("2", orgPersonPosition.Relationships.ElementAt(1).Id);
            Assert.AreEqual("OER1", orgPersonPosition.Relationships.ElementAt(1).OrganizationalPersonPositionId);
            Assert.AreEqual("OER2", orgPersonPosition.Relationships.ElementAt(1).RelatedOrganizationalPersonPositionId);
            Assert.AreEqual("PER2", orgPersonPosition.Relationships.ElementAt(1).RelatedPersonId);
            Assert.AreEqual("POS2", orgPersonPosition.Relationships.ElementAt(1).RelatedPositionId);
            Assert.AreEqual("title2", orgPersonPosition.Relationships.ElementAt(1).RelatedPositionTitle);
            Assert.AreEqual("approver", orgPersonPosition.Relationships.ElementAt(1).Category);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddRelationship_DuplicateCategory_ThrowsException()
        {
            orgPersonPosition = new OrganizationalPersonPosition(orgEntityRoleId, personId, positionId, positionTitle, null, null);
            var rel1 = new OrganizationalRelationship("R1", orgEntityRoleId, personId, positionId, positionTitle, null, null, "SL2", "a", "b", "c", null, null, "Manager");
            var rel2 = new OrganizationalRelationship("R2", orgEntityRoleId, personId, positionId, positionTitle, null, null, "SL3", "x", "Y", "z", null, null, "Manager");
            orgPersonPosition.AddRelationship(rel1);
            orgPersonPosition.AddRelationship(rel2);
        }

    }
}
