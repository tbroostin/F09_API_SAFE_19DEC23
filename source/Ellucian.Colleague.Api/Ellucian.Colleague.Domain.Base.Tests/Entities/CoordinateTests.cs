using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{

    [TestClass]
    public class CoordinateTests {

        [TestClass]
        public class CoordinateConstructor {
            private decimal? latitude;
            private decimal? longitude;
            private bool officeUseOnly;
            private Coordinate coord;

            [TestInitialize]
            public void Initialize() {
                latitude = new decimal?(77.234123M);
                longitude = new decimal?(-114.123789M);
                officeUseOnly = false;
                coord = new Coordinate(latitude, longitude, officeUseOnly);
            }
            [TestMethod]
            public void Latitude() {
                Assert.AreEqual(latitude, coord.Latitude);
            }
            [TestMethod]
            public void Longitude() {
                Assert.AreEqual(longitude, coord.Longitude);
            }
            [TestMethod]
            public void OfficeUseOnly() {
                Assert.AreEqual(officeUseOnly, coord.OfficeUseOnly);
            }
            [TestMethod]
            public void CoordinateMissingLatitudeForcesOfficeUseTrue() {
                Coordinate coord = new Coordinate(null, -114.123789M, false);
                Assert.IsTrue(coord.OfficeUseOnly);
            }
            [TestMethod]
            public void CoordinateMissingLongitudeForcesOfficeUseTrue() {
                Coordinate coord = new Coordinate(77.234123M, null, false);
                Assert.IsTrue(coord.OfficeUseOnly);
            }
            [TestMethod]
            public void CoordinateRespectOfficeUseTrue() {
                Coordinate coord = new Coordinate(77.234123M, -114.123789M, true);
                Assert.IsTrue(coord.OfficeUseOnly);
            }
        }

        [TestClass]
        public class CoordinateEquals {
            private Coordinate coord1;
            private Coordinate coord2;
            private Coordinate coord3;
            private Coordinate coord4;
            private Coordinate coord5;
            private Coordinate coord6;
            private Coordinate coord7;
            private Coordinate coord8;

            [TestInitialize]
            public void Initialize() {
                coord1 = new Coordinate(77.111111M, -114.111111M, true);
                coord2 = new Coordinate(77.111111M, -114.111111M, true);
                coord3 = new Coordinate(77.222222M, -114.111111M, true);
                coord4 = new Coordinate(77.111111M, -114.222222M, true);
                coord5 = new Coordinate(77.111111M, -114.111111M, false);
                coord6 = new Coordinate(77.222222M, -114.222222M, false);
                coord7 = new Coordinate(null, -114.111111M, true);
                coord8 = new Coordinate(77.111111M, null, true);
            }

            [TestMethod]
            public void CoordinateAllValuesEqual() {
                Assert.IsTrue(coord1.Equals(coord2));
            }
            [TestMethod]
            public void CoordinateDifferentLatitudeNotEqual() {
                Assert.IsFalse(coord1.Equals(coord3));
            }
            [TestMethod]
            public void CoordinateDifferentLongitudeNotEqual() {
                Assert.IsFalse(coord1.Equals(coord4));
            }
            [TestMethod]
            public void CoordinateDifferentOfficeUseNotEqual() {
                Assert.IsFalse(coord1.Equals(coord5));
            }
            [TestMethod]
            public void CoordinateAllDifferentNotEqual() {
                Assert.IsFalse(coord1.Equals(coord6));
            }
            [TestMethod]
            public void CoordinateNonCoordinateNotEqual()
            {
                Assert.IsFalse(coord1.Equals("abcd"));
            }
            [TestMethod]
            public void CoordinateNoLatitudeNotEqual()
            {
                Assert.IsFalse(coord1.Equals(coord7));
            }
            [TestMethod]
            public void CoordinateNoLatitude2NotEqual()
            {
                Assert.IsFalse(coord7.Equals(coord1));
            }
            [TestMethod]
            public void CoordinateNoLongitudeNotEqual()
            {
                Assert.IsFalse(coord1.Equals(coord8));
            }
            [TestMethod]
            public void CoordinateNoLongitude2NotEqual()
            {
                Assert.IsFalse(coord8.Equals(coord1));
            }
            [TestMethod]
            public void CoordinateNoLatitudeBothEqual()
            {
                Assert.IsFalse(coord8.Equals(coord7));
            }
            [TestMethod]
            public void CoordinateNullNotEqual()
            {
                Assert.IsFalse(coord8.Equals(null));
            }
        }

        [TestClass]
        public class CoordinateGetHashCode {
            private Coordinate coord1;
            private Coordinate coord2;
            private Coordinate coord3;
            private Coordinate coord4;
            private Coordinate coord5;
            private Coordinate coord6;

            [TestInitialize]
            public void Initialize() {
                coord1 = new Coordinate(77.111111M, -114.111111M, true);
                coord2 = new Coordinate(77.111111M, -114.111111M, true);
                coord3 = new Coordinate(77.222222M, -114.111111M, true);
                coord4 = new Coordinate(77.111111M, -114.222222M, true);
                coord5 = new Coordinate(77.111111M, -114.111111M, false);
                coord6 = new Coordinate(77.222222M, -114.222222M, false);
            }

            [TestMethod]
            public void CoordinateAllSameHashEqual() {
                Assert.AreEqual(coord1.GetHashCode(), coord2.GetHashCode());
            }
            [TestMethod]
            public void CoordinateDifferentLatitudeHashNotEqual() {
                Assert.AreNotEqual(coord1.GetHashCode(), coord3.GetHashCode());
            }
            [TestMethod]
            public void CoordinateDifferentLongitudeHashNotEqual() {
                Assert.AreNotEqual(coord1.GetHashCode(), coord4.GetHashCode());
            }
            [TestMethod]
            public void CoordinateDifferentOfficeUseHashNotEqual() {
                Assert.AreNotEqual(coord1.GetHashCode(), coord5.GetHashCode());
            }
            [TestMethod]
            public void CoordinateAllDifferentHashNotEqual() {
                Assert.AreNotEqual(coord1.GetHashCode(), coord6.GetHashCode());
            }
        }
    }
}
