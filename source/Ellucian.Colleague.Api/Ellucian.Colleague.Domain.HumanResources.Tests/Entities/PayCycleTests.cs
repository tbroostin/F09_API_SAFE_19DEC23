/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayCycleTests
    {
        public string id;
        public string description;
        public DayOfWeek startDay;
        public List<string> payClassIds;
        public List<PayPeriod> payPeriods;
        public bool displayInSelfService;

        public PayCycle payCycle;

        [TestClass]
        public class PaycycleConstructorTests : PayCycleTests
        {
            public PayCycle createPaycycle()
            {
                return new PayCycle(id, description, startDay);
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "foobar";
                description = "Lisa's Pay Cycle";
            }

            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                payCycle = createPaycycle();
                Assert.AreEqual(id, payCycle.Id);
                Assert.AreEqual(description, payCycle.Description);

            }

            [TestMethod]
            public void PayClassIdsInitializedTest()
            {
                payCycle = createPaycycle();
                Assert.IsNotNull(payCycle.PayClassIds);
            }

            [TestMethod]
            public void PayPeriodsInitialzedTest()
            {
                payCycle = createPaycycle();
                Assert.IsNotNull(payCycle.PayPeriods);
            }

            [TestMethod]
            public void DisplayInSelfService_InitialzedTest()
            {
                payCycle = createPaycycle();
                Assert.IsFalse(payCycle.DisplayInSelfService);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = "";
                createPaycycle();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DescriptionRequiredTest()
            {
                description = "";
                createPaycycle();
            }
        }

        [TestClass]
        public class PayCycleAttributesTests : PayCycleTests
        {
            [TestInitialize]
            public void Initialize()
                {
                    id = "foobar";
                    description = "Lisa's Test Paycycle";

                    payCycle = new PayCycle(id, description, startDay);
                }

            [TestMethod]
            public void DescriptionTest()
            {
                payCycle.Description = "foobar";
                Assert.AreEqual("foobar", payCycle.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DescriptionRequiredTest()
            {
                payCycle.Description = "";
            }

            [TestMethod]
            public void DisplayInSelfService_GetSetTest()
            {
                payCycle.DisplayInSelfService = true;
                Assert.IsTrue(payCycle.DisplayInSelfService);
            }
        }

        [TestClass]
        public class PayCycleEqualsTests : PayCycleTests
        {
            public PayCycle createPayCycle()
            {
                return new PayCycle(id, description, startDay);
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "LK";
                description = "Lisa's Test Paycycle";
            }

            [TestMethod]
            public void ObjectsEqualWhenIdsAreEqualTest()
            {
                var payCycle1 = createPayCycle();
                var payCycle2 = createPayCycle();

                Assert.AreEqual(payCycle1, payCycle2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenIdsAreNotEqualTest()
            {
                var payCycle1 = createPayCycle();
                id = "foobar";
                var payCycle2 = createPayCycle();

                Assert.AreNotEqual(payCycle1, payCycle2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                var payCycle1 = createPayCycle();
                Assert.AreNotEqual(payCycle1, null);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsDifferentTypeTest()
            {
                var payCycle1 = createPayCycle();
                var notPayCycle = new Base.Entities.Bank("011000015", "name", "011000015");
                Assert.AreNotEqual(payCycle1, notPayCycle);
            }

            [TestMethod]
            public void HashCodesEqualWhenIdsEqualTest()
            {
                var payCycle1 = createPayCycle();
                var payCycle2 = createPayCycle();
                Assert.AreEqual(payCycle1.GetHashCode(), payCycle2.GetHashCode());
            }

            [TestMethod]
            public void HashCodesNotEqualWhenIdsNotEqualTest()
            {
                var payCycle1 = createPayCycle();
                id = "foobar";
                var payCycle2 = createPayCycle();
                Assert.AreNotEqual(payCycle1.GetHashCode(), payCycle2.GetHashCode());
            }

            [TestMethod]
            public void ToStringTest()
            {
                var payCycle1 = createPayCycle();
                Assert.AreEqual(payCycle1.Description + "-" + payCycle1.Id, payCycle1.ToString());
            }
        }
    }
}
