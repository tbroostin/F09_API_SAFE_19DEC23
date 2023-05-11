// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PhoneTests
    {
        [TestClass]
        public class PhoneConstructorTests
        {
            string pNumber;
            string typeCode;
            string ext;
            Phone testPhone;

            [TestInitialize]
            public void Initialize()
            {
                pNumber = "111-222-3333";
                typeCode = "PER";
                ext = "444";
                testPhone = new Phone(pNumber, typeCode, ext);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Phone_Constructor_NullNumber()
            {
                testPhone = new Phone(null, typeCode, ext);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Phone_Constructor_EmptyNumber()
            {
                testPhone = new Phone(string.Empty, typeCode, ext);
            }

            [TestMethod]
            public void NewEmailAddress_Properties()
            {
                Assert.AreEqual(pNumber, testPhone.Number);
                Assert.AreEqual(typeCode, testPhone.TypeCode);
                Assert.AreEqual(ext, testPhone.Extension);
            }

            [TestMethod]
            public void NoOptionalParameters_Properties()
            {
                Phone ph = new Phone(pNumber);
                Assert.AreEqual(pNumber, ph.Number);
                Assert.IsNull(ph.TypeCode);
                Assert.IsNull(ph.Extension);
            }

            [TestMethod]
            public void NoExtension_Properties()
            {
                Phone ph = new Phone(pNumber, typeCode);
                Assert.AreEqual(pNumber, ph.Number);
                Assert.AreEqual(typeCode, ph.TypeCode);
                Assert.IsNull(ph.Extension);
            }

            [TestMethod]
            public void NoType_Properties()
            {
                Phone ph = new Phone(pNumber, extension: ext);
                Assert.AreEqual(pNumber, ph.Number);
                Assert.IsNull(ph.TypeCode);
                Assert.AreEqual(ext, ph.Extension);
            }

            [TestMethod]
            public void IsAuthorizedForText_DefaultsNull()
            {
                Phone ph = new Phone(pNumber, extension: ext);
                Assert.IsNull(ph.IsAuthorizedForText);
            }

            [TestMethod]
            public void IsAuthorizedForText_SetToTrue()
            {
                Phone ph = new Phone(pNumber, extension: ext, isAuthorizedForText: true);
                Assert.AreEqual(true, ph.IsAuthorizedForText);
            }
        }

        [TestClass]
        public class PhoneEqualsTests
        {
            string pNumber;
            string typeCode;
            string ext;
            Phone testPhone;

            [TestInitialize]
            public void Initialize()
            {
                pNumber = "111-222-3333";
                typeCode = "PER";
                ext = "444";
                testPhone = new Phone(pNumber, typeCode, ext);
            }

            [TestMethod]
            public void Phones_Equals_Null()
            {
                Assert.IsFalse(testPhone.Equals(null));
            }

            [TestMethod]
            public void Phones_Equals_NonPhone()
            {
                Assert.IsFalse(testPhone.Equals("abc"));
            }

            [TestMethod]
            public void Phones_Equal()
            {
                Phone phone2 = new Phone(pNumber, typeCode, ext);
                Assert.IsTrue(testPhone.Equals(phone2));

            }

            [TestMethod]
            public void Phones_DifferentExtensions_AreNotEqual()
            {
                Phone phone2 = new Phone(pNumber, "CP", "999");
                Assert.IsFalse(testPhone.Equals(phone2));
            }

            [TestMethod]
            public void Phone_NotEquals()
            {
                Phone phone2 = new Phone(pNumber, "PAR");
                Assert.IsFalse(testPhone.Equals(phone2));
            }

            [TestMethod]
            public void Phone_NotEquals2()
            {
                Phone phone2 = new Phone("112-222-3333", "PER");
                Assert.IsFalse(testPhone.Equals(phone2));
            }

            [TestMethod]
            public void Phone_NullTypeCodes_AreEqual()
            {
                Phone phone1 = new Phone(pNumber, null, ext);
                Phone phone2 = new Phone(pNumber, null, ext);
                Assert.IsTrue(phone1.Equals(phone2));
            }

            [TestMethod]
            public void Phone_NullExtensions_AreEqual()
            {
                Phone phone1 = new Phone(pNumber, typeCode, null);
                Phone phone2 = new Phone(pNumber, typeCode, null);
                Assert.IsTrue(phone1.Equals(phone2));
            }

            [TestMethod]
            public void Phone_CompareNullTypeCodeToPopulated_AreNotEqual()
            {
                Phone phoneWithNull = new Phone(pNumber, null, ext);
                Assert.IsFalse(phoneWithNull.Equals(testPhone));
            }

            [TestMethod]
            public void Phone_CompareNullExtensionToPopulated_AreNotEqual()
            {
                Phone phoneWithNull = new Phone(pNumber, typeCode, null);
                Assert.IsFalse(phoneWithNull.Equals(testPhone));
            }
        }
    }
}
