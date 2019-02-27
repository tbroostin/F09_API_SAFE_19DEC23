//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
     [TestClass]
    public class AltIdTypesTests
    {
        [TestClass]
        public class AltIdTypesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AltIdTypes alternativeCredentialTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                alternativeCredentialTypes = new AltIdTypes(guid, code, desc);
            }

            [TestMethod]
            public void AltIdTypes_Code()
            {
                Assert.AreEqual(code, alternativeCredentialTypes.Code);
            }

            [TestMethod]
            public void AltIdTypes_Description()
            {
                Assert.AreEqual(desc, alternativeCredentialTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AltIdTypes_GuidNullException()
            {
                new AltIdTypes(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AltIdTypes_CodeNullException()
            {
                new AltIdTypes(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AltIdTypes_DescNullException()
            {
                new AltIdTypes(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AltIdTypesGuidEmptyException()
            {
                new AltIdTypes(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AltIdTypesCodeEmptyException()
            {
                new AltIdTypes(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AltIdTypesDescEmptyException()
            {
                new AltIdTypes(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class AltIdTypes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AltIdTypes alternativeCredentialTypes1;
            private AltIdTypes alternativeCredentialTypes2;
            private AltIdTypes alternativeCredentialTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                alternativeCredentialTypes1 = new AltIdTypes(guid, code, desc);
                alternativeCredentialTypes2 = new AltIdTypes(guid, code, "Second Year");
                alternativeCredentialTypes3 = new AltIdTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AltIdTypesSameCodesEqual()
            {
                Assert.IsTrue(alternativeCredentialTypes1.Equals(alternativeCredentialTypes2));
            }

            [TestMethod]
            public void AltIdTypesDifferentCodeNotEqual()
            {
                Assert.IsFalse(alternativeCredentialTypes1.Equals(alternativeCredentialTypes3));
            }
        }

        [TestClass]
        public class AltIdTypes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private AltIdTypes alternativeCredentialTypes1;
            private AltIdTypes alternativeCredentialTypes2;
            private AltIdTypes alternativeCredentialTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                alternativeCredentialTypes1 = new AltIdTypes(guid, code, desc);
                alternativeCredentialTypes2 = new AltIdTypes(guid, code, "Second Year");
                alternativeCredentialTypes3 = new AltIdTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AltIdTypesSameCodeHashEqual()
            {
                Assert.AreEqual(alternativeCredentialTypes1.GetHashCode(), alternativeCredentialTypes2.GetHashCode());
            }

            [TestMethod]
            public void AltIdTypesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(alternativeCredentialTypes1.GetHashCode(), alternativeCredentialTypes3.GetHashCode());
            }
        }
    }
}
