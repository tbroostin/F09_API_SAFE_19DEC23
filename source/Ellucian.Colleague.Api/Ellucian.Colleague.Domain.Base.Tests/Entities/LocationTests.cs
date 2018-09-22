// Copyright 2013-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class LocationTests
    {
        private string guid;
        private string code;
        private string desc;
        private decimal? nwlat;
        private decimal? nwlong;
        private decimal? selat;
        private decimal? selong;
        private string vfm;
        private List<string> bldgCodes = new List<string>();
        private Location loc;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "WEST";
            desc = "West Campus";
            nwlat = new decimal?(77.234123M);
            nwlong = new decimal?(-114.123987M);
            selat = new decimal?(77.234120M);
            selong = new decimal?(-114.123989M);
            vfm = "Y";
            bldgCodes.Add("BLDG1");
            bldgCodes.Add("BLDG2");
            loc = new Location(guid, code, desc, nwlat, nwlong, selat, selong, vfm, bldgCodes);
        }

        [TestClass]
        public class LocationConstructor_Default : LocationTests
        {
            [TestMethod]
            public void LocationGuid()
            {
                Assert.AreEqual(guid, loc.Guid);
            }

            [TestMethod]
            public void LocationCode()
            {
                Assert.AreEqual(code, loc.Code);
            }

            [TestMethod]
            public void LocationDescription()
            {
                Assert.AreEqual(desc, loc.Description);
            }

            [TestMethod]
            public void LocationNorthWestLatitude() {
                Assert.AreEqual(nwlat, loc.NorthWestCoordinate.Latitude);
            }

            [TestMethod]
            public void LocationNorthWestLongitude() {
                Assert.AreEqual(nwlong, loc.NorthWestCoordinate.Longitude);
            }

            [TestMethod]
            public void LocationSouthEastLatitude() {
                Assert.AreEqual(selat, loc.SouthEastCoordinate.Latitude);
            }

            [TestMethod]
            public void LocationSouthEastLongitude() {
                Assert.AreEqual(selong, loc.SouthEastCoordinate.Longitude);
            }

            [TestMethod]
            public void LocationOfficeUseOnly() {
                bool officeUseOnly = true;
                if (!string.IsNullOrEmpty(vfm) && vfm.Equals("Y")) { officeUseOnly = false; }
                Assert.AreEqual(officeUseOnly, loc.NorthWestCoordinate.OfficeUseOnly);
                Assert.AreEqual(officeUseOnly, loc.SouthEastCoordinate.OfficeUseOnly);
            }

            [TestMethod]
            public void LocationBuildingCodes() {
                Assert.AreEqual(bldgCodes.Count, loc.BuildingCodes.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationGuidNullException()
            {
                new Location(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationGuidEmptyException()
            {
                new Location(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationCodeNullException()
            {
                new Location(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationCodeEmptyException()
            {
                new Location(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationDescNullException()
            {
                new Location(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationDescEmptyException()
            {
                new Location(guid, code, string.Empty);
            }

            [TestMethod]
            public void LocationConstructor_Default_HideInSelfServiceCourseSearch()
            {
                var location = new Location(guid, code, desc);
                Assert.IsFalse(location.HideInSelfServiceCourseSearch);
            }

            [TestMethod]
            public void LocationConstructor_HideInSelfServiceCourseSearch_False()
            {
                var location = new Location(guid, code, desc, false);
                Assert.IsFalse(location.HideInSelfServiceCourseSearch);
            }

            [TestMethod]
            public void LocationConstructor_HideInSelfServiceCourseSearch_True()
            {
                var location = new Location(guid, code, desc, true);
                Assert.IsTrue(location.HideInSelfServiceCourseSearch);
            }
        }

        [TestClass]
        public class LocationConstructor : LocationTests
        {
            [TestInitialize]
            public void LocationConstructor_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationConstructor_CodeNullException2()
            {
                new Location(guid, null, desc, nwlat, nwlong, selat, selong, vfm, bldgCodes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationConstructor_CodeEmptyException2()
            {
                new Location(guid, string.Empty, desc, nwlat, nwlong, selat, selong, vfm, bldgCodes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationConstructor_DescNullException2()
            {
                new Location(guid, code, null, nwlat, nwlong, selat, selong, vfm, bldgCodes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationConstructor_DescEmptyException2()
            {
                new Location(guid, code, string.Empty, nwlat, nwlong, selat, selong, vfm, bldgCodes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationConstructor_VisibleForMobileYesAndNWLatNullException()
            {
                new Location(guid, code, desc, null, nwlong, selat, selong, "Y", bldgCodes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationConstructor_VisibleForMobileYesAndNWLongNullException()
            {
                new Location(guid, code, desc, nwlat, null, selat, selong, "Y", bldgCodes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationConstructor_VisibleForMobileYesAndSELatNullException()
            {
                new Location(guid, code, desc, nwlat, nwlong, null, selong, "Y", bldgCodes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationConstructor_VisibleForMobileYesAndSELongNullException()
            {
                new Location(guid, code, desc, nwlat, nwlong, selat, null, "Y", bldgCodes);
            }

            [TestMethod]
            public void LocationConstructor_NoVisibility()
            {
                var location = new Location(guid, code, desc, nwlat, nwlong, selat, selong, "N", bldgCodes);
                Assert.IsTrue(location.NorthWestCoordinate.OfficeUseOnly);
                Assert.IsTrue(location.SouthEastCoordinate.OfficeUseOnly);
            }

            [TestMethod]
            public void LocationConstructor_NullVfm()
            {
                var location = new Location(guid, code, desc, nwlat, nwlong, selat, selong, null, bldgCodes);
                Assert.IsTrue(location.NorthWestCoordinate.OfficeUseOnly);
                Assert.IsTrue(location.SouthEastCoordinate.OfficeUseOnly);
            }

            [TestMethod]
            public void LocationConstructor_EmptyVfm()
            {
                var location = new Location(guid, code, desc, nwlat, nwlong, selat, selong, string.Empty, bldgCodes);
                Assert.IsTrue(location.NorthWestCoordinate.OfficeUseOnly);
                Assert.IsTrue(location.SouthEastCoordinate.OfficeUseOnly);
            }

            [TestMethod]
            public void LocationConstructor__NullBuildings()
            {
                var location = new Location(guid, code, desc, nwlat, nwlong, selat, selong, vfm, null);
                Assert.AreEqual(0, location.BuildingCodes.Count);
                CollectionAssert.AreEqual(new List<string>(), location.BuildingCodes);
            }

            [TestMethod]
            public void LocationConstructor__NoBuildings()
            {
                var location = new Location(guid, code, desc, nwlat, nwlong, selat, selong, vfm, new List<string>());
                Assert.AreEqual(0, location.BuildingCodes.Count);
                CollectionAssert.AreEqual(new List<string>(), location.BuildingCodes);
            }

            [TestMethod]
            public void LocationConstructor_Default_HideInSelfServiceCourseSearch()
            {
                var location = new Location(guid, code, desc, nwlat, nwlong, selat, selong, string.Empty, bldgCodes);
                Assert.IsFalse(location.HideInSelfServiceCourseSearch);
            }

            [TestMethod]
            public void LocationConstructor_HideInSelfServiceCourseSearch_False()
            {
                var location = new Location(guid, code, desc, nwlat, nwlong, selat, selong, string.Empty, bldgCodes, false);
                Assert.IsFalse(location.HideInSelfServiceCourseSearch);
            }

            [TestMethod]
            public void LocationConstructor_HideInSelfServiceCourseSearch_True()
            {
                var location = new Location(guid, code, desc, nwlat, nwlong, selat, selong, string.Empty, bldgCodes, true);
                Assert.IsTrue(location.HideInSelfServiceCourseSearch);
            }
        }

        [TestClass]
        public class LocationTests_AddBuilding
        {
            string guid, code, desc, building1, building2, building3;
            Location result;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString().ToLowerInvariant();
                code = "MAIN";
                desc = "Main Campus";
                building1 = "AAAA";
                building2 = "BBBB";
                building3 = "CCCC";

                result = new Location(guid, code, desc);
                result.AddBuilding(building1);
                result.AddBuilding(building2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationTests_AddBuilding_NullBuilding()
            {
                result.AddBuilding(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LocationTests_AddBuilding_EmptyBuilding()
            {
                result.AddBuilding(string.Empty);
            }

            [TestMethod]
            public void LocationTests_AddBuilding_EqualLists()
            {
                var buildings = new List<string>() { building1, building2 };
                CollectionAssert.AreEqual(buildings, result.BuildingCodes.ToList());
            }

            [TestMethod]
            public void LocationTests_AddBuilding_Count()
            {
                Assert.AreEqual(2, result.BuildingCodes.Count);
            }

            [TestMethod]
            public void LocationTests_AddBuilding_Code0()
            {
                Assert.AreEqual(building1, result.BuildingCodes[0]);
            }

            [TestMethod]
            public void LocationTests_AddBuilding_Code1()
            {
                Assert.AreEqual(building2, result.BuildingCodes[1]);
            }

            [TestMethod]
            public void LocationTests_AddBuilding_AddDuplicate()
            {
                result.AddBuilding(building1);
                Assert.AreEqual(2, result.BuildingCodes.Count);
            }

            [TestMethod]
            public void LocationTests_AddBuilding_AddBuilding()
            {
                result.AddBuilding(building3);
                Assert.AreEqual(3, result.BuildingCodes.Count);
                Assert.AreEqual(building3, result.BuildingCodes[2]);
            }
        }

        [TestClass]
        public class Location_SortOrder : LocationTests
        {
            [TestInitialize]
            public void Location_SortOrder_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void Location_SortOrder_Negative_InvalidOperation()
            {
                loc.SortOrder = -1;
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void Location_SortOrder_Zero_InvalidOperation()
            {
                loc.SortOrder = 0;
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void Location_SortOrder_Positive_InvalidOperation()
            {
                loc.SortOrder = 11111;
            }

            [TestMethod]
            public void Location_SortOrder_Get_Set_Null()
            {
                int sortOrder = 10;
                loc.SortOrder = sortOrder;
                loc.SortOrder = null;
                Assert.IsNull(loc.SortOrder);
            }

            [TestMethod]
            public void Location_SortOrder_Get_Set_NonNull()
            {
                int sortOrder = 10;
                loc.SortOrder = sortOrder;
                Assert.AreEqual(sortOrder, loc.SortOrder);
            }
        }

        [TestClass]
        public class LocationEquals
        {
            private string guid;
            private string code;
            private string desc;
            private Location loc1;
            private Location loc2;
            private Location loc3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "WEST";
                desc = "West Campus";
                loc1 = new Location(guid, code, desc);
                loc2 = new Location(guid, code, "Main campus");
                loc3 = new Location(guid, "NORTH", desc);
            }

            [TestMethod]
            public void LocationSameCodesEqual()
            {
                Assert.IsTrue(loc1.Equals(loc2));
            }

            [TestMethod]
            public void LocationDifferentCodeNotEqual()
            {
                Assert.IsFalse(loc1.Equals(loc3));
            }
        }

        [TestClass]
        public class LocationGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private Location loc1;
            private Location loc2;
            private Location loc3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "WEST";
                desc = "West Campus";
                loc1 = new Location(guid, code, desc);
                loc2 = new Location(guid, code, "Main Campus");
                loc3 = new Location(guid, "NORTH", desc);
            }

            [TestMethod]
            public void LocationSameCodeHashEqual()
            {
                Assert.AreEqual(loc1.GetHashCode(), loc2.GetHashCode());
            }

            [TestMethod]
            public void LocationDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(loc1.GetHashCode(), loc3.GetHashCode());
            }
        }
    }
}