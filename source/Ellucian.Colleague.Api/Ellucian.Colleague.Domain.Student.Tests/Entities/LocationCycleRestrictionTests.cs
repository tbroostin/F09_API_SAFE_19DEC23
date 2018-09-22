// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class LocationCycleRestrictionTests
    {
        [TestClass]
        public class LocationCycleRestriction_Constructor
        {
            private string location;
            private string sessionCycle;
            private string yearlyCycle;

            [TestInitialize]
            public void Initialize()
            {
                location = "LOCATION";
                sessionCycle = "SESSION1";
                yearlyCycle = "YEARLY1";
            }

            [TestMethod]
            public void LocationCycleRestriction_AllProperties()
            {
                var lcr = new LocationCycleRestriction(location, sessionCycle, yearlyCycle);
                Assert.AreEqual(location, lcr.Location);
                Assert.AreEqual(sessionCycle, lcr.SessionCycle);
                Assert.AreEqual(yearlyCycle, lcr.YearlyCycle);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullLocation_ThrowsException()
            {
                var lcr = new LocationCycleRestriction(null, sessionCycle, yearlyCycle);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmptyLocation_ThrowsException()
            {
                var lcr = new LocationCycleRestriction(string.Empty, sessionCycle, yearlyCycle);
            }

            [TestMethod]
            public void LocationCycleRestriction_NullCyclesOk()
            {
                var lcr = new LocationCycleRestriction(location, null, null);
                Assert.IsNull(lcr.SessionCycle);
                Assert.IsNull(lcr.YearlyCycle);
            }
        }

        [TestClass]
        public class LocationCycleRestrictionEquals
        {
            LocationCycleRestriction lcr1;
            LocationCycleRestriction lcr2;
            LocationCycleRestriction lcr3;
            LocationCycleRestriction lcr4;
            LocationCycleRestriction lcr5;
            LocationCycleRestriction lcr6;
            LocationCycleRestriction lcr7;
            LocationCycleRestriction lcr8;
            LocationCycleRestriction lcr9;
            LocationCycleRestriction lcr10;
            LocationCycleRestriction lcr11;

            [TestInitialize]
            public void Initialize()
            {
                lcr1 = new LocationCycleRestriction("LOC1", string.Empty, string.Empty);
                lcr2 = new LocationCycleRestriction("LOC1", null, null);
                lcr3 = new LocationCycleRestriction("LOC1", null, null);
                lcr4 = new LocationCycleRestriction("LOC2", null, null);
                lcr5 = new LocationCycleRestriction("LOC2", "F", string.Empty);
                lcr6 = new LocationCycleRestriction("LOC2", "F", null);
                lcr7 = new LocationCycleRestriction("LOC2", "F", "E");
                lcr8 = new LocationCycleRestriction("LOC2", "S", "E");
                lcr9 = new LocationCycleRestriction("LOC2", string.Empty, "E");
                lcr10 = new LocationCycleRestriction("LOC2", null, "E");
                lcr11 = new LocationCycleRestriction("LOC1", "F", "E");
                
            }

            [TestMethod]
            public void EqualsWithNoSessionOrYearlyCycle_NullVsEmptyString()
            {
                // Test to assure that items no cycle info (regardless of empty or null) are the same to prevent duplication.
                Assert.IsTrue(lcr1.Equals(lcr2));
            }

            [TestMethod]
            public void EqualsWithNoSessionOrYearlyCycle_Nulls()
            {
                Assert.IsTrue(lcr2.Equals(lcr3));
            }

            [TestMethod]
            public void NotEqualsWithNoSessionOrYearlyCycle_Nulls()
            {
                Assert.IsFalse(lcr3.Equals(lcr4));
            }

            [TestMethod]
            public void EqualsWithSessionCycle_NullVsEmpty()
            {
                Assert.IsTrue(lcr5.Equals(lcr6));
            }

            [TestMethod]
            public void NotEqualsWithDifferentYearlyCycle()
            {
                Assert.IsFalse(lcr6.Equals(lcr7));
            }

            [TestMethod]
            public void NotEqualsWithDifferentYearlyCycle2()
            {
                Assert.IsFalse(lcr5.Equals(lcr7));
            }

            [TestMethod]
            public void NotEqualsWithDifferentSessionCycles()
            {
                Assert.IsFalse(lcr8.Equals(lcr9));
            }

            [TestMethod]
            public void EqualsWithSameYearlyCycles_NullVsEmpty()
            {
                Assert.IsTrue(lcr9.Equals(lcr10));
            }

            [TestMethod]
            public void NotEqualsWithDifferentLocations()
            {
                Assert.IsFalse(lcr11.Equals(lcr7));
            }
        }
    }
}
