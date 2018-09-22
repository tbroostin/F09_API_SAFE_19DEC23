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
    public class PositionFundingSourceTests
    {
        public string fundingSourceId;
        public int fundingOrder;
        public string projectReferenceNumber;

        public PositionFundingSource positionFundingSource;

        public void PositionFundingSourceTestsInitialize()
        {
            fundingSourceId = "COMP";
            fundingOrder = 1;
            projectReferenceNumber = "123456789";
        }

        [TestClass]
        public class ConstructorTests : PositionFundingSourceTests
        {
            public new PositionFundingSource positionFundingSource
            {
                get
                {
                    return new PositionFundingSource(fundingSourceId, fundingOrder);
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                PositionFundingSourceTestsInitialize();
            }

            [TestMethod]
            public void FundingSourceIdTest()
            {
                Assert.AreEqual(fundingSourceId, positionFundingSource.FundingSourceId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FundingSourceIdRequiredTest()
            {
                fundingSourceId = "";
                var error = positionFundingSource;
            }

            [TestMethod]
            public void FundingOrderTest()
            {
                Assert.AreEqual(fundingOrder, positionFundingSource.FundingOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void FundingOrderNegativeTest()
            {
                fundingOrder = -1;
                var error = positionFundingSource;
            }
        }

        [TestClass]
        public class EqualsTests : PositionFundingSourceTests
        {
            public PositionFundingSource buildPositionFundingSource()
            {
                return new PositionFundingSource(fundingSourceId, fundingOrder);
            }
            [TestInitialize]
            public void Initialize()
            {
                PositionFundingSourceTestsInitialize();
            }

            [TestMethod]
            public void ObjectsEqualWhenIdAndOrderEqualTest()
            {
                var s1 = buildPositionFundingSource();
                var s2 = buildPositionFundingSource();

                Assert.IsTrue(s1.Equals(s2));
                Assert.IsTrue(s2.Equals(s1));
            }

            [TestMethod]
            public void HashCodesEqualWhenIdAndOrderEqualTest()
            {
                var s1 = buildPositionFundingSource();
                var s2 = buildPositionFundingSource();

                Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());
            }

            [TestMethod]
            public void ObjectsNotEqualWhenIdNotEqualTest()
            {
                var s1 = buildPositionFundingSource();
                fundingSourceId = "foobar";
                var s2 = buildPositionFundingSource();

                Assert.IsFalse(s1.Equals(s2));
                Assert.IsFalse(s2.Equals(s1));
            }

            [TestMethod]
            public void HashCodeNotEqualWhenIdNotEqualTest()
            {
                var s1 = buildPositionFundingSource();
                fundingSourceId = "foobar";
                var s2 = buildPositionFundingSource();

                Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode());
            }

            [TestMethod]
            public void ObjectsNotEqualWhenOrderNotEqualTest()
            {
                var s1 = buildPositionFundingSource();
                fundingOrder = ++fundingOrder;
                var s2 = buildPositionFundingSource();

                Assert.IsFalse(s1.Equals(s2));
                Assert.IsFalse(s2.Equals(s1));
            }

            [TestMethod]
            public void HashCodeNotEqualWhenOrderNotEqualTest()
            {
                var s1 = buildPositionFundingSource();
                fundingOrder = ++fundingOrder;
                var s2 = buildPositionFundingSource();

                Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode());
            }
        }
    }
}
