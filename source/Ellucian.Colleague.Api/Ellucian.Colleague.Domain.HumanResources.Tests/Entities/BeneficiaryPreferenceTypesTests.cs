//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
     [TestClass]
    public class BeneficiaryTypesTests
    {
        [TestClass]
        public class BeneficiaryTypesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private BeneficiaryTypes beneficiaryPreferenceTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                beneficiaryPreferenceTypes = new BeneficiaryTypes(guid, code, desc);
            }

            [TestMethod]
            public void BeneficiaryTypes_Code()
            {
                Assert.AreEqual(code, beneficiaryPreferenceTypes.Code);
            }

            [TestMethod]
            public void BeneficiaryTypes_Description()
            {
                Assert.AreEqual(desc, beneficiaryPreferenceTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BeneficiaryTypes_GuidNullException()
            {
                new BeneficiaryTypes(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BeneficiaryTypes_CodeNullException()
            {
                new BeneficiaryTypes(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BeneficiaryTypes_DescNullException()
            {
                new BeneficiaryTypes(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BeneficiaryTypesGuidEmptyException()
            {
                new BeneficiaryTypes(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BeneficiaryTypesCodeEmptyException()
            {
                new BeneficiaryTypes(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BeneficiaryTypesDescEmptyException()
            {
                new BeneficiaryTypes(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class BeneficiaryTypes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private BeneficiaryTypes beneficiaryPreferenceTypes1;
            private BeneficiaryTypes beneficiaryPreferenceTypes2;
            private BeneficiaryTypes beneficiaryPreferenceTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                beneficiaryPreferenceTypes1 = new BeneficiaryTypes(guid, code, desc);
                beneficiaryPreferenceTypes2 = new BeneficiaryTypes(guid, code, "Second Year");
                beneficiaryPreferenceTypes3 = new BeneficiaryTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void BeneficiaryTypesSameCodesEqual()
            {
                Assert.IsTrue(beneficiaryPreferenceTypes1.Equals(beneficiaryPreferenceTypes2));
            }

            [TestMethod]
            public void BeneficiaryTypesDifferentCodeNotEqual()
            {
                Assert.IsFalse(beneficiaryPreferenceTypes1.Equals(beneficiaryPreferenceTypes3));
            }
        }

        [TestClass]
        public class BeneficiaryTypes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private BeneficiaryTypes beneficiaryPreferenceTypes1;
            private BeneficiaryTypes beneficiaryPreferenceTypes2;
            private BeneficiaryTypes beneficiaryPreferenceTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                beneficiaryPreferenceTypes1 = new BeneficiaryTypes(guid, code, desc);
                beneficiaryPreferenceTypes2 = new BeneficiaryTypes(guid, code, "Second Year");
                beneficiaryPreferenceTypes3 = new BeneficiaryTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void BeneficiaryTypesSameCodeHashEqual()
            {
                Assert.AreEqual(beneficiaryPreferenceTypes1.GetHashCode(), beneficiaryPreferenceTypes2.GetHashCode());
            }

            [TestMethod]
            public void BeneficiaryTypesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(beneficiaryPreferenceTypes1.GetHashCode(), beneficiaryPreferenceTypes3.GetHashCode());
            }
        }
    }
}
