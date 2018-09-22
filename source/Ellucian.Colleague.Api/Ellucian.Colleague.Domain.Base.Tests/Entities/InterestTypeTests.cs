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
    public class InterestTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private InterestType interestType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "AR";
            description = "Art";

        }

        [TestClass]
        public class InterestTypeConstructor : InterestTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestTypeConstructorNullGuid()
            {
                interestType = new InterestType(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestTypeConstructorEmptyGuid()
            {
                interestType = new InterestType(string.Empty, code, description);
            }

            [TestMethod]
            public void InterestTypeConstructorValidGuid()
            {
                interestType = new InterestType(guid, code, description);
                Assert.AreEqual(guid, interestType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestTypeConstructorNullCode()
            {
                interestType = new InterestType(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestTypeConstructorEmptyCode()
            {
                interestType = new InterestType(guid, string.Empty, description);
            }

            [TestMethod]
            public void InterestTypeConstructorValidCode()
            {
                interestType = new InterestType(guid, code, description);
                Assert.AreEqual(code, interestType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestTypeConstructorNullDescription()
            {
                interestType = new InterestType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestTypeConstructorEmptyDescription()
            {
                interestType = new InterestType(guid, code, string.Empty);
            }

            [TestMethod]
            public void InterestTypeConstructorValidDescription()
            {
                interestType = new InterestType(guid, code, description);
                Assert.AreEqual(description, interestType.Description);
            }
        }
    }
}