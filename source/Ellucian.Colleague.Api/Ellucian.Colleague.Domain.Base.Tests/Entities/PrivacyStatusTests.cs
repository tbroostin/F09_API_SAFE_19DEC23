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
    public class PrivacyStatusTests
    {
        private string guid;
        private string code;
        private string description;
        private PrivacyStatusType type;
        private PrivacyStatus privacyStatus;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "X";
            description = "Duplicate";
            type = PrivacyStatusType.restricted;
        }

        [TestClass]
        public class PrivacyStatusConstructor : PrivacyStatusTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrivacyStatusConstructorNullGuid()
            {
                privacyStatus = new PrivacyStatus(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrivacyStatusConstructorEmptyGuid()
            {
                privacyStatus = new PrivacyStatus(string.Empty, code, description, type);
            }

            [TestMethod]
            public void PrivacyStatusConstructorValidGuid()
            {
                privacyStatus = new PrivacyStatus(guid, code, description, type);
                Assert.AreEqual(guid, privacyStatus.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrivacyStatusConstructorNullCode()
            {
                privacyStatus = new PrivacyStatus(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrivacyStatusConstructorEmptyCode()
            {
                privacyStatus = new PrivacyStatus(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void PrivacyStatusConstructorValidCode()
            {
                privacyStatus = new PrivacyStatus(guid, code, description, type);
                Assert.AreEqual(code, privacyStatus.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrivacyStatusConstructorNullDescription()
            {
                privacyStatus = new PrivacyStatus(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PrivacyStatusConstructorEmptyDescription()
            {
                privacyStatus = new PrivacyStatus(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void PrivacyStatusConstructorValidDescription()
            {
                privacyStatus = new PrivacyStatus(guid, code, description, type);
                Assert.AreEqual(description, privacyStatus.Description);
            }

            [TestMethod]
            public void PrivacyStatusConstructorValidType()
            {
                privacyStatus = new PrivacyStatus(guid, code, description, type);
                Assert.AreEqual(type, privacyStatus.PrivacyStatusType);
            }
        }
    }
}
