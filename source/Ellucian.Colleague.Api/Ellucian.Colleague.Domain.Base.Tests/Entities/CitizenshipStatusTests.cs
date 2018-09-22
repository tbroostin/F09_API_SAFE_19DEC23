// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class CitizenshipStatusTests
    {
        private string guid;
        private string code;
        private string description;
        private CitizenshipStatusType type;
        private CitizenshipStatus citizenshipStatus;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "R";
            description = "Resident Alien";
            type = CitizenshipStatusType.Citizen;
        }

        [TestClass]
        public class CitizenshipStatusConstructor : CitizenshipStatusTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenshipStatusConstructorNullGuid()
            {
                citizenshipStatus = new CitizenshipStatus(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenshipStatusConstructorEmptyGuid()
            {
                citizenshipStatus = new CitizenshipStatus(string.Empty, code, description, type);
            }

            [TestMethod]
            public void CitizenshipStatusConstructorValidGuid()
            {
                citizenshipStatus = new CitizenshipStatus(guid, code, description, type);
                Assert.AreEqual(guid, citizenshipStatus.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenshipStatusConstructorNullCode()
            {
                citizenshipStatus = new CitizenshipStatus(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenshipStatusConstructorEmptyCode()
            {
                citizenshipStatus = new CitizenshipStatus(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void CitizenshipStatusConstructorValidCode()
            {
                citizenshipStatus = new CitizenshipStatus(guid, code, description, type);
                Assert.AreEqual(code, citizenshipStatus.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenshipStatusConstructorNullDescription()
            {
                citizenshipStatus = new CitizenshipStatus(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenshipStatusConstructorEmptyDescription()
            {
                citizenshipStatus = new CitizenshipStatus(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void CitizenshipStatusConstructorValidDescription()
            {
                citizenshipStatus = new CitizenshipStatus(guid, code, description, type);
                Assert.AreEqual(description, citizenshipStatus.Description);
            }

            [TestMethod]
            public void CitizenshipStatusConstructorValidType()
            {
                citizenshipStatus = new CitizenshipStatus(guid, code, description, type);
                Assert.AreEqual(type, citizenshipStatus.CitizenshipStatusType);
            }
        }
    }
}
