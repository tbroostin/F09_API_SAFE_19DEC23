// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class GenderIdentityTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private GenderIdentityType genderIdentityType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "FEM";
            description = "Female";
        }

        [TestClass]
        public class GenderTypeConstructor : GenderIdentityTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GenderIdentityTypeConstructorNullGuid()
            {
                genderIdentityType = new GenderIdentityType(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GenderIdentityTypeConstructorEmptyGuid()
            {
                genderIdentityType = new GenderIdentityType(string.Empty, code, description);
            }

            [TestMethod]
            public void GenderIdentityTypeConstructorValidGuid()
            {
                genderIdentityType = new GenderIdentityType(guid, code, description);
                Assert.AreEqual(guid, genderIdentityType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GenderIdentityTypeConstructorNullCode()
            {
                genderIdentityType = new GenderIdentityType(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GenderIdentityTypeConstructorEmptyCode()
            {
                genderIdentityType = new GenderIdentityType(guid, string.Empty, description);
            }

            [TestMethod]
            public void GenderIdentityTypeConstructorValidCode()
            {
                genderIdentityType = new GenderIdentityType(guid, code, description);
                Assert.AreEqual(code, genderIdentityType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GenderIdentityTypeConstructorNullDescription()
            {
                genderIdentityType = new GenderIdentityType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GenderIdentityTypeConstructorEmptyDescription()
            {
                genderIdentityType = new GenderIdentityType(guid, code, string.Empty);
            }

            [TestMethod]
            public void EmailTypeConstructorValidDescription()
            {
                genderIdentityType = new GenderIdentityType(guid, code, description);
                Assert.AreEqual(description, genderIdentityType.Description);
            }

        }
    }
}
