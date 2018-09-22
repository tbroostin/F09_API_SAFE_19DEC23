// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonalRelationshipStatusTests
    {
        string guid;
        private string code;
        private string description;
        private PersonalRelationshipStatus personalRelationshipStatus;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "A";
            description = "Active";
            personalRelationshipStatus = new PersonalRelationshipStatus(guid, code, description);
        }

        [TestClass]
        public class PersonalRelationshipStatusConstructor : PersonalRelationshipStatusTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonalRelationshipStatusConstructorNullGuid()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonalRelationshipStatusConstructorEmptyGuid()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(string.Empty, code, description);
            }

            [TestMethod]
            public void PersonalRelationshipStatusConstructorValidGuid()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(guid, code, description);
                Assert.AreEqual(guid, personalRelationshipStatus.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonalRelationshipStatusConstructorNullCode()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonalRelationshipStatusConstructorEmptyCode()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(guid, string.Empty, description);
            }

            [TestMethod]
            public void PersonalRelationshipStatusConstructorValidCode()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(guid, code, description);
                Assert.AreEqual(code, personalRelationshipStatus.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonalRelationshipStatusConstructorNullDescription()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonalRelationshipStatusConstructorEmptyDescription()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(guid, code, string.Empty);
            }

            [TestMethod]
            public void PersonalRelationshipStatusConstructorValidDescription()
            {
                personalRelationshipStatus = new PersonalRelationshipStatus(guid, code, description);
                Assert.AreEqual(description, personalRelationshipStatus.Description);
            }
        }
    }
}
