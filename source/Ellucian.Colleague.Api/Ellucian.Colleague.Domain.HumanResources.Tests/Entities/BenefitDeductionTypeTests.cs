/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class BenefitDeductionTypeTests
    {
        public string id;
        public string description;
        public string selfServiceDescription;
        public BenefitDeductionTypeCategory type;
        public BenefitDeductionType bendedType;

        [TestInitialize]
        public void Initialize()
        {
            id = "tma001";
            description = "something a little more verbose";
            selfServiceDescription = "something a liiiiiittle more verboooose";
            type = BenefitDeductionTypeCategory.Benefit;
        }

        [TestMethod]
        public void PropertiesAreSet()
        {
            bendedType = new BenefitDeductionType(id, description, selfServiceDescription, type);
            Assert.AreEqual(id, bendedType.Id);
            Assert.AreEqual(description, bendedType.Description);
            Assert.AreEqual(selfServiceDescription, bendedType.SelfServiceDescription);
            Assert.AreEqual(type, bendedType.Category);
        }

        [TestMethod]
        public void DescriptionSetterTest()
        {
            bendedType = new BenefitDeductionType(id, description, selfServiceDescription, type);
            bendedType.Description = "foo";
            Assert.AreEqual("foo", bendedType.Description);
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void NullIdInConstructor()
        {
            new BenefitDeductionType(null, description, selfServiceDescription, type);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullDescriptionInConstructor()
        {
            new BenefitDeductionType(id, null, selfServiceDescription, type);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void DescriptionCannotBeSetToNull()
        {
            bendedType = new BenefitDeductionType(id, description, selfServiceDescription, type);
            bendedType.Description = "";
        }

        [TestMethod]
        public void EqualsOverride()
        {
            var bendedTypeA = new BenefitDeductionType(id, description, selfServiceDescription, type);
            var bendedTypeB = new BenefitDeductionType(id, description, selfServiceDescription, type);
            Assert.IsTrue(bendedTypeA.Equals(bendedTypeB));
        }

        [TestMethod]
        public void EqualsIsFalseForNullObject()
        {
            var bendedTypeA = new BenefitDeductionType(id, description, selfServiceDescription, type);
            BenefitDeductionType bendedTypeB = null;
            Assert.IsFalse(bendedTypeA.Equals(bendedTypeB));
        }

        [TestMethod]
        public void EqualsIsFalseForDifferentObjectType()
        {
            var bendedTypeA = new BenefitDeductionType(id, description, selfServiceDescription, type);
            string bendedTypeB = "foo";
            Assert.IsFalse(bendedTypeA.Equals(bendedTypeB));
        }

        [TestMethod]
        public void HashCodeOverride()
        {
            bendedType = new BenefitDeductionType(id, description,selfServiceDescription, type);
            Assert.AreEqual(id.GetHashCode(), bendedType.GetHashCode());
        }

        [TestMethod]
        public void ToStringOverride()
        {
            bendedType = new BenefitDeductionType(id, description, selfServiceDescription, type);
            Assert.AreEqual(string.Format("{0}-{1}",description,id), bendedType.ToString());
        }
    }
}
