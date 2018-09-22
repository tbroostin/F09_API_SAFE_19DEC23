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
    public class EthnicityTests
    {
        private string guid;
        private string code;
        private string description;
        private EthnicityType type;
        private Ethnicity ethnicity;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "HIS";
            description = "Hispanic/Latino";
            type = EthnicityType.Hispanic;
        }

        [TestClass]
        public class EthnicityConstructor : EthnicityTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EthnicityConstructorNullGuid()
            {
                ethnicity = new Ethnicity(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EthnicityConstructorEmptyGuid()
            {
                ethnicity = new Ethnicity(string.Empty, code, description, type);
            }

            [TestMethod]
            public void EthnicityConstructorValidGuid()
            {
                ethnicity = new Ethnicity(guid, code, description, type);
                Assert.AreEqual(guid, ethnicity.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EthnicityConstructorNullCode()
            {
                ethnicity = new Ethnicity(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EthnicityConstructorEmptyCode()
            {
                ethnicity = new Ethnicity(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void EthnicityConstructorValidCode()
            {
                ethnicity = new Ethnicity(guid, code, description, type);
                Assert.AreEqual(code, ethnicity.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EthnicityConstructorNullDescription()
            {
                ethnicity = new Ethnicity(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EthnicityConstructorEmptyDescription()
            {
                ethnicity = new Ethnicity(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void EthnicityConstructorValidDescription()
            {
                ethnicity = new Ethnicity(guid, code, description, type);
                Assert.AreEqual(description, ethnicity.Description);
            }

            [TestMethod]
            public void EthnicityConstructorValidType()
            {
                ethnicity = new Ethnicity(guid, code, description, type);
                Assert.AreEqual(type, ethnicity.Type);
            }
        }
    }
}
