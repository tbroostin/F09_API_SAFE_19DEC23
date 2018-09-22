// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class OrganizationalRelationshipTests
    {
        protected OrganizationalRelationship relationship;
    }

    [TestClass]
    public class OrganizationalRelationship_Constructor : OrganizationalRelationshipTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_IdNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship(null, "PP1", "PER1", "POS1", "title1", null, null, "PP2", "PER2", "POS2", "title2", null, null, "Management");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_PersonPositionIdNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", null, "PER1", "POS1", "title1", null, null, "PP2", "PER2", "POS2", "title2", null, null, "Management");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_PersonIdNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", null, "POS1", "title1", null, null, "PP2", "PER2", "POS2", "title2", null, null, "Manager");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_PosIdNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", null, "title1", null, null, "PP2", "PER2", "POS2", "title2", null, null, "Manager");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_PosTitleNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", "POS1", null, null, null, "PP2", "PER2", "POS2", "title2", null, null, "Manager");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_RelatedPersonPositionIdNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", "POS1", "title1", null, null, null, "PER2", "POS2", "title2", null, null, "Management");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_RelatedPersonIdNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", "POS1", "title1", null, null, "PP2", null, "POS2", "title2", null, null, "Manager");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_RelatedPosIdNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", "POS1", "title1", null, null, "PP2", "PER2", null, "title2", null, null, "Manager");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_RelatedPosTitleNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", "POS1", "title1", null, null, "PP2", "PER2", "POS2", null, null, null, "Manager");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OrganizationalRelationship_CategoryNull_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", "POS1", "title1", null, null, "PP2", "PER2", "POS2", "title2", null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void OrganizationalRelationship_RelatedPersonPositionIsSame_ThrowsException()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", "POS1", "title1", null, null, "PP1", "PER1", "POS1", "title1", null, null, "Management");
        }

        [TestMethod]
        public void OrganizationalRelationship_Success()
        {
            relationship = new OrganizationalRelationship("REL1", "PP1", "PER1", "POS1", "title1", null, null, "PP2", "PER2", "POS2", "title2", null, null, "Management");
            Assert.AreEqual("REL1", relationship.Id);
            Assert.AreEqual("PP1", relationship.OrganizationalPersonPositionId);
            Assert.AreEqual("PER1", relationship.PersonId);
            Assert.AreEqual("POS1", relationship.PositionId);
            Assert.AreEqual("title1", relationship.PositionTitle);
            Assert.AreEqual("PP2", relationship.RelatedOrganizationalPersonPositionId);
            Assert.AreEqual("PER2", relationship.RelatedPersonId);
            Assert.AreEqual("POS2", relationship.RelatedPositionId);
            Assert.AreEqual("title2", relationship.RelatedPositionTitle);
            Assert.AreEqual("Management", relationship.Category);
        }
    }

    // Remove these tests until we have a need for a constructore with no ID... probably will need it for updates
    //[TestClass]
    //public class OrganizationalRelationship_NoId_Constructor : OrganizationalRelationshipTests
    //{
    //    [TestMethod]
    //    [ExpectedException(typeof(ArgumentNullException))]
    //    public void OrganizationalRelationship_PositionIdNull_ThrowsException()
    //    {
    //        relationship = new OrganizationalRelationship(null, "SL2", "Management");
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(ArgumentNullException))]
    //    public void OrganizationalRelationship_RelatedPositionIdNull_ThrowsException()
    //    {
    //        relationship = new OrganizationalRelationship("SL1", null, "Management");
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(ArgumentNullException))]
    //    public void OrganizationalRelationship_CategoryNull_ThrowsException()
    //    {
    //        relationship = new OrganizationalRelationship("SL1", "SL2", null);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(ArgumentException))]
    //    public void OrganizationalRelationship_RelatedPositionIsSame_ThrowsException()
    //    {
    //        relationship = new OrganizationalRelationship("SL1", "SL1", "Management");
    //    }

    //    [TestMethod]
    //    public void OrganizationalRelationship_Success()
    //    {
    //        relationship = new OrganizationalRelationship("SL1", "SL2", "Management");
    //        Assert.AreEqual("SL1", relationship.OrganizationalPersonPositionId);
    //        Assert.AreEqual("SL2", relationship.RelatedOrganizationalPersonPositionId);
    //        Assert.AreEqual("Management", relationship.Category);
    //    }
    //}
}