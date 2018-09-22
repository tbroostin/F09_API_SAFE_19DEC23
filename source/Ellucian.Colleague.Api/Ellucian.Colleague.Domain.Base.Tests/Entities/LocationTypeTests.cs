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
    public class LocationTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private EntityType entityType;
        private PersonLocationType personType;
        private OrganizationLocationType orgType;
        private LocationTypeItem locationType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "PER";
            description = "Personal";
            entityType = EntityType.Person;
            personType = PersonLocationType.Home;
            orgType = OrganizationLocationType.Other;
        }

        [TestClass]
        public class LocationTypeConstructor : LocationTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationTypeConstructorNullGuid()
            {
                locationType = new LocationTypeItem(null, code, description, entityType, personType, orgType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationTypeConstructorEmptyGuid()
            {
                locationType = new LocationTypeItem(string.Empty, code, description, entityType, personType, orgType);
            }

            [TestMethod]
            public void LocationTypeConstructorValidGuid()
            {
                locationType = new LocationTypeItem(guid, code, description, entityType, personType, orgType);
                Assert.AreEqual(guid, locationType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationTypeConstructorNullCode()
            {
                locationType = new LocationTypeItem(guid, null, description, entityType, personType, orgType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationTypeConstructorEmptyCode()
            {
                locationType = new LocationTypeItem(guid, string.Empty, description, entityType, personType, orgType);
            }

            [TestMethod]
            public void LocationTypeConstructorValidCode()
            {
                locationType = new LocationTypeItem(guid, code, description, entityType, personType, orgType);
                Assert.AreEqual(code, locationType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationTypeConstructorNullDescription()
            {
                locationType = new LocationTypeItem(guid, code, null, entityType, personType, orgType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationTypeConstructorEmptyDescription()
            {
                locationType = new LocationTypeItem(guid, code, string.Empty, entityType, personType, orgType);
            }

            [TestMethod]
            public void LocationTypeConstructorValidDescription()
            {
                locationType = new LocationTypeItem(guid, code, description, entityType, personType, orgType);
                Assert.AreEqual(description, locationType.Description);
            }

            [TestMethod]
            public void LocationTypeConstructorValidEntityType()
            {
                locationType = new LocationTypeItem(guid, code, description, entityType, personType, orgType);
                Assert.AreEqual(entityType, locationType.Type.EntityType);
            }

            [TestMethod]
            public void LocationTypeConstructorValidPersonLocationType()
            {
                locationType = new LocationTypeItem(guid, code, description, entityType, personType, orgType);
                Assert.AreEqual(personType, locationType.Type.PersonLocationType);
            }

            [TestMethod]
            public void LocationTypeConstructorValidOrganizationLocationType()
            {
                locationType = new LocationTypeItem(guid, code, description, entityType, personType, orgType);
                Assert.AreEqual(orgType, locationType.Type.OrganizationLocationType);
            }
        }
    }
}
