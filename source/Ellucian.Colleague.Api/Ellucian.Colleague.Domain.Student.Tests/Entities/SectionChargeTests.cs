// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionChargeTests
    {
        private string id;
        private string chargeCode;
        private decimal baseAmount;
        private bool isFlatFee;
        private bool isRuleBased;
        private SectionCharge entity;

        [TestInitialize]
        public void Initialize()
        {
            id = "123";
            chargeCode = "ABC";
            baseAmount = 50m;
            isFlatFee = true;
            isRuleBased = true;
        }

        [TestClass]
        public class SectionCharge_Constructor : SectionChargeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionCharge_Constructor_Null_Id()
            {
                entity = new SectionCharge(null, chargeCode, baseAmount, isFlatFee, isRuleBased);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionCharge_Constructor_Null_ChargeCode()
            {
                entity = new SectionCharge(id, null, baseAmount, isFlatFee, isRuleBased);
            }

            [TestMethod]
            public void SectionCharge_Constructor_Valid()
            {
                entity = new SectionCharge(id, chargeCode, baseAmount, isFlatFee, isRuleBased);
                Assert.AreEqual(id, entity.Id);
                Assert.AreEqual(chargeCode, entity.ChargeCode);
                Assert.AreEqual(baseAmount, entity.BaseAmount);
                Assert.AreEqual(isFlatFee, entity.IsFlatFee);
                Assert.AreEqual(isRuleBased, entity.IsRuleBased);
            }
        }

        [TestClass]
        public class SectionCharge_Equals : SectionChargeTests
        {
            [TestMethod]
            public void SectionCharge_Equals_False_Null_Object_to_Compare()
            {
                entity = new SectionCharge(id, chargeCode, baseAmount, isFlatFee, isRuleBased);
                SectionCharge compare = null;
                Assert.IsFalse(entity.Equals(compare));
            }

            [TestMethod]
            public void SectionCharge_Equals_False_Non_SectionCharge_Object_to_Compare()
            {
                entity = new SectionCharge(id, chargeCode, baseAmount, isFlatFee, isRuleBased);
                string compare = null;
                Assert.IsFalse(entity.Equals(compare));
            }

            [TestMethod]
            public void SectionCharge_Equals_False_SectionCharge_Object_to_Compare_has_different_Id()
            {
                entity = new SectionCharge(id, chargeCode, baseAmount, isFlatFee, isRuleBased);
                SectionCharge compare = new SectionCharge(id + "2", chargeCode, baseAmount, isFlatFee, isRuleBased);
                Assert.IsFalse(entity.Equals(compare));
            }

            [TestMethod]
            public void SectionCharge_Equals_True_SectionCharge_Object_to_Compare_has_same_Id()
            {
                entity = new SectionCharge(id, chargeCode, baseAmount, isFlatFee, isRuleBased);
                SectionCharge compare = new SectionCharge(id, chargeCode, baseAmount, isFlatFee, isRuleBased);
                Assert.IsTrue(entity.Equals(compare));
            }
        }

        [TestClass]
        public class SectionCharge_GetHashCode : SectionChargeTests
        {
            [TestMethod]
            public void SectionCharge_GetHashCode_returns_Id_HashCode()
            {
                entity = new SectionCharge(id, chargeCode, baseAmount, isFlatFee, isRuleBased);
                int expectedHashCode = entity.Id.GetHashCode();
                Assert.AreEqual(expectedHashCode, entity.GetHashCode());
            }
        }
    }
}
