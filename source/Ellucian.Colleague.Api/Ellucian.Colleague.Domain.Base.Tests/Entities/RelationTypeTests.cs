// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RelationTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private string orgIndicator;
        PersonalRelationshipType personalRelationshipType;
        private RelationType relationType;


        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "C";
            description = "Child";
            orgIndicator = "N";
            personalRelationshipType = PersonalRelationshipType.Other;
        }

        [TestClass]
        public class RelationTypeConstructor : RelationTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationTypeConstructorNullGuid()
            {
                relationType = new RelationType(null, code, description, orgIndicator, personalRelationshipType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationTypeConstructorEmptyGuid()
            {
                relationType = new RelationType(string.Empty, code, description, orgIndicator, personalRelationshipType);
            }

            [TestMethod]
            public void RelationTypeConstructorValidGuid()
            {
                relationType = new RelationType(guid, code, description, orgIndicator, personalRelationshipType);
                Assert.AreEqual(guid, relationType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationTypeConstructorNullCode()
            {
                relationType = new RelationType(guid, null, description, orgIndicator, personalRelationshipType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationTypeConstructorEmptyCode()
            {
                relationType = new RelationType(guid, string.Empty, description, orgIndicator, personalRelationshipType);
            }

            [TestMethod]
            public void RelationTypeConstructorValidCode()
            {
                relationType = new RelationType(guid, code, description, orgIndicator, personalRelationshipType);
                Assert.AreEqual(code, relationType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationTypeConstructorNullDescription()
            {
                relationType = new RelationType(guid, code, null, orgIndicator, personalRelationshipType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationTypeConstructorEmptyDescription()
            {
                relationType = new RelationType(guid, code, string.Empty, orgIndicator, personalRelationshipType);
            }

            [TestMethod]
            public void RelationTypeConstructorValidDescription()
            {
                relationType = new RelationType(guid, code, description, orgIndicator, personalRelationshipType);
                Assert.AreEqual(description, relationType.Description);
            }


            [TestMethod]
            public void RelationTypeConstructorValidOrgIndicator()
            {
                relationType = new RelationType(guid, code, description, orgIndicator, personalRelationshipType);
                Assert.AreEqual(orgIndicator, relationType.OrgIndicator);
            }

            [TestMethod]
            public void RelationTypeConstructorValidPersonalRelationshipType()
            {
                relationType = new RelationType(guid, code, description, orgIndicator, personalRelationshipType);
                Assert.AreEqual(personalRelationshipType, relationType.PersonRelType);
            }


        }
    }
}
