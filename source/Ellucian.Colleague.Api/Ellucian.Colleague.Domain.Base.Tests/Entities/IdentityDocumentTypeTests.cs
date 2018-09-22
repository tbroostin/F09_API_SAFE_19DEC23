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
    public class IdentityDocumentTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private IdentityDocumentTypeCategory type;
        private IdentityDocumentType identityDocumentType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "PASSPORT";
            description = "Passport Document";
            type = IdentityDocumentTypeCategory.Passport;
        }

        [TestClass]
        public class IdentityDocumentTypeConstructor : IdentityDocumentTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdentityDocumentTypeConstructorNullGuid()
            {
                identityDocumentType = new IdentityDocumentType(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdentityDocumentTypeConstructorEmptyGuid()
            {
                identityDocumentType = new IdentityDocumentType(string.Empty, code, description, type);
            }

            [TestMethod]
            public void IdentityDocumentTypeConstructorValidGuid()
            {
                identityDocumentType = new IdentityDocumentType(guid, code, description, type);
                Assert.AreEqual(guid, identityDocumentType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdentityDocumentTypeConstructorNullCode()
            {
                identityDocumentType = new IdentityDocumentType(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdentityDocumentTypeConstructorEmptyCode()
            {
                identityDocumentType = new IdentityDocumentType(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void IdentityDocumentTypeConstructorValidCode()
            {
                identityDocumentType = new IdentityDocumentType(guid, code, description, type);
                Assert.AreEqual(code, identityDocumentType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdentityDocumentTypeConstructorNullDescription()
            {
                identityDocumentType = new IdentityDocumentType(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdentityDocumentTypeConstructorEmptyDescription()
            {
                identityDocumentType = new IdentityDocumentType(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void IdentityDocumentTypeConstructorValidDescription()
            {
                identityDocumentType = new IdentityDocumentType(guid, code, description, type);
                Assert.AreEqual(description, identityDocumentType.Description);
            }

            [TestMethod]
            public void IdentityDocumentTypeConstructorValidType()
            {
                identityDocumentType = new IdentityDocumentType(guid, code, description, type);
                Assert.AreEqual(type, identityDocumentType.IdentityDocumentTypeCategory);
            }
        }
    }
}
