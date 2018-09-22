/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
    public class EarningsTypeTests
    {
        public string id;
        public string description;
        public bool isActive;
        public EarningsCategory category;
        public EarningsMethod method;
        public decimal? factor;

        public EarningsType earningsType;

        [TestClass]
        public class EarningsTypeConstructorTests : EarningsTypeTests
        {
            public EarningsType createEarningsType()
            {
                return new EarningsType(id, description, isActive, category, method, factor);
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "REG";
                description = "Regular Pay";
                isActive = true;
                category = EarningsCategory.Regular;
                factor = 1.5m;
            }

            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                earningsType = createEarningsType();
                Assert.AreEqual(id, earningsType.Id);
                Assert.AreEqual(description, earningsType.Description);
                Assert.AreEqual(isActive, earningsType.IsActive);
                Assert.AreEqual(EarningsCategory.Regular, earningsType.Category);
                Assert.AreEqual(factor.Value, earningsType.Factor);
            }

            [TestMethod]
            public void DefaultMethodTest()
            {
                earningsType = createEarningsType();
                Assert.AreEqual(EarningsMethod.None, earningsType.Method);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = "";
                createEarningsType();
            }
            
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DescriptionRequiredTest()
            {
                description = "";
                createEarningsType();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void FactorTooLowTest()
            {
                factor = 0.00009m;
                createEarningsType();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void FactorTooHighTest()
            {
                factor = 9.99991m;
                createEarningsType();
            }

            [TestMethod]
            public void FactorDefaultsToOneTest()
            {
                factor = null;
                earningsType = createEarningsType();
                Assert.AreEqual(1, earningsType.Factor);
            }
        }

        [TestClass]
        public class EarningsTypeAttributesTests : EarningsTypeTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "REG";
                description = "Regular Pay";
                isActive = true;
                category = EarningsCategory.Regular;
                factor = 1.5m;

                earningsType = new EarningsType(id, description, isActive, category, method, factor);
            }

            [TestMethod]
            public void DescriptionTest()
            {
                earningsType.Description = "foobar";
                Assert.AreEqual("foobar", earningsType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DescriptionRequiredTest()
            {
                earningsType.Description = "";
            }

            
        }

        [TestClass]
        public class EarningsTypeEqualsTest : EarningsTypeTests
        {
            public EarningsType createEarningsType()
            {
                return new EarningsType(id, description, isActive, category, method, factor);
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "REG";
                description = "Regular Pay";
                isActive = true;
                category = EarningsCategory.Regular;
            }

            [TestMethod]
            public void ObjectsEqualWhenIdsAreEqualTest()
            {
                var earningsType1 = createEarningsType();
                var earningsType2 = createEarningsType();

                Assert.AreEqual(earningsType1, earningsType2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenIdsAreNotEqualTest()
            {
                var earningsType1 = createEarningsType();
                id = "foobar";
                var earningsType2 = createEarningsType();

                Assert.AreNotEqual(earningsType1, earningsType2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                var earningsType1 = createEarningsType();
                Assert.AreNotEqual(earningsType1, null);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsDifferentTypeTest()
            {
                var earningsType1 = createEarningsType();
                var notEarningsType = new Base.Entities.Bank("bank", "name", "011000015");
                Assert.AreNotEqual(earningsType1, notEarningsType);
            }

            [TestMethod]
            public void HashCodeEqualWhenIdIsEqualTest()
            {
                var earningsType1 = createEarningsType();
                var earningsType2 = createEarningsType();
                Assert.AreEqual(earningsType1.GetHashCode(), earningsType2.GetHashCode());
            }

            [TestMethod]
            public void HashCodeNotEqualWhenIdsNotEqualTest()
            {
                var earningsType1 = createEarningsType();
                id = "foobar";
                var earningsType2 = createEarningsType();
                Assert.AreNotEqual(earningsType1.GetHashCode(), earningsType2.GetHashCode());
            }

            [TestMethod]
            public void ToStringTest()
            {
                var earningsType1 = createEarningsType();
                Assert.AreEqual(earningsType1.Description + "-" + earningsType1.Id, earningsType1.ToString());
            }
        }
    }
}
