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
    public class MaritalStatusTests
    {
        private string guid;
        private string code;
        private string description;
        private MaritalStatusType type;
        private MaritalStatus maritalStatus;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "M";
            description = "Married";
            type = MaritalStatusType.Married;
        }

        [TestClass]
        public class MaritalStatusConstructor : MaritalStatusTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MaritalStatusConstructorNullGuid()
            {
                maritalStatus = new MaritalStatus(null, code, description) { Type = type};
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MaritalStatusConstructorEmptyGuid()
            {
                maritalStatus = new MaritalStatus(string.Empty, code, description) { Type = type };
            }

            [TestMethod]
            public void MaritalStatusConstructorValidGuid()
            {
                maritalStatus = new MaritalStatus(guid, code, description) { Type = type };
                Assert.AreEqual(guid, maritalStatus.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MaritalStatusConstructorNullCode()
            {
                maritalStatus = new MaritalStatus(guid, null, description) { Type = type};
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MaritalStatusConstructorEmptyCode()
            {
                maritalStatus = new MaritalStatus(guid, string.Empty, description) { Type = type};
            }

            [TestMethod]
            public void MaritalStatusConstructorValidCode()
            {
                maritalStatus = new MaritalStatus(guid, code, description) { Type = type };
                Assert.AreEqual(code, maritalStatus.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MaritalStatusConstructorNullDescription()
            {
                maritalStatus = new MaritalStatus(guid, code, null) { Type = type };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MaritalStatusConstructorEmptyDescription()
            {
                maritalStatus = new MaritalStatus(guid, code, string.Empty) { Type = type };
            }

            [TestMethod]
            public void MaritalStatusConstructorValidDescription()
            {
                maritalStatus = new MaritalStatus(guid, code, description) { Type = type };
                Assert.AreEqual(description, maritalStatus.Description);
            }

            [TestMethod]
            public void MaritalStatusConstructorValidType()
            {
                maritalStatus = new MaritalStatus(guid, code, description) { Type = type };
                Assert.AreEqual(type, maritalStatus.Type);
            }
        }
    }
}
