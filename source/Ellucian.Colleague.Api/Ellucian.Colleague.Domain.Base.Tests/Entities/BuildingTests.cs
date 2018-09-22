using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    
    [TestClass]
    public class BuildingTests {

        [TestClass]
        public class BuildingConstructor 
        {
            private string guid;
            private string code;
            private string desc;
            private string locId;
            private string bldgType;
            private string longDesc;
            private List<string> addressLines;
            private string city;
            private string state;
            private string postalCode;
            private string country;
            private decimal? lati;
            private decimal? longi;
            private string imageUrl;
            private string addnlSvcs;
            private string vfm;
            private Building bldg;

            [TestInitialize]
            public void Initialize() 
            {
                guid = Guid.NewGuid().ToString();
                code = "BLDG";
                desc = "Building A";
                locId = "LOCN";
                bldgType = "ADMIN";
                longDesc = "This is the admin building.";
                addressLines = new List<string>(); addressLines.Add("Addr Line 1"); addressLines.Add("Addr Line 2");
                city = "Fairfax";
                state = "VA";
                postalCode = "22033";
                country = "USA";
                lati = new decimal?(77.234132M);
                longi = new decimal?(-114.123987M);
                imageUrl = "http://www.somewhere.com/123.png";
                addnlSvcs = "Other services available in the building.";
                vfm = "Y";
                bldg = new Building(guid, code, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, vfm);
            }

            [TestMethod]
            public void BuildingGuid()
            {
                Assert.AreEqual(guid, bldg.Guid);
            }

            [TestMethod]
            public void BuildingCode() {
                Assert.AreEqual(code, bldg.Code);
            }

            [TestMethod]
            public void BuildingDescription() {
                Assert.AreEqual(desc, bldg.Description);
            }

            [TestMethod]
            public void BuildingLocationCode() {
                Assert.AreEqual(locId,bldg.LocationId);
            }

            [TestMethod]
            public void BuildingBuildingType() {
                Assert.AreEqual(bldgType, bldg.BuildingType);
            }

            [TestMethod]
            public void BuildingLongDescription() {
                Assert.AreEqual(longDesc, bldg.LongDescription);
            }

            [TestMethod]
            public void BuildingAddressLines() {
                for (int i = 0; i < addressLines.Count; i++) {
                    Assert.AreEqual(addressLines[i], bldg.AddressLines[i]);
                }
            }

            [TestMethod]
            public void BuildingCity() {
                Assert.AreEqual(city, bldg.City);
            }

            [TestMethod]
            public void BuildingState() {
                Assert.AreEqual(state, bldg.State);
            }

            [TestMethod]
            public void BuildingPostalCode() {
                Assert.AreEqual(postalCode, bldg.PostalCode);
            }

            [TestMethod]
            public void BuildingCountry() {
                Assert.AreEqual(country, bldg.Country);
            }

            [TestMethod]
            public void BuildingLatitude() {
                Assert.AreEqual(lati, bldg.BuildingCoordinate.Latitude);
            }

            [TestMethod]
            public void BuildingLongitude() {
                Assert.AreEqual(longi, bldg.BuildingCoordinate.Longitude);
            }

            [TestMethod]
            public void BuildingImageUrl() {
                Assert.AreEqual(imageUrl, bldg.ImageUrl);
            }

            [TestMethod]
            public void BuildingAdditionalServices() {
                Assert.AreEqual(addnlSvcs, bldg.AdditionalServices);
            }

            [TestMethod]
            public void BuildingOfficeUseOnly() {
                bool officeUseOnly = true;
                if (!string.IsNullOrEmpty(vfm) && vfm.Equals("Y")) { officeUseOnly = false; }
                Assert.AreEqual(officeUseOnly, bldg.BuildingCoordinate.OfficeUseOnly);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingGuidNullException()
            {
                new Building(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingGuidEmptyException()
            {
                new Building(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingCodeNullException() {
                new Building(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingCodeEmptyException()
            {
                new Building(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingDescriptionNullException() {
                new Building(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingDescriptionEmptyException()
            {
                new Building(guid, code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingGuidNullException2()
            {
                new Building(null, code, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, vfm);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingGuidEmptyException2()
            {
                new Building(string.Empty, code, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, vfm);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingCodeNullException2() {
                new Building(guid, null, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, vfm);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingCodeEmptyException2()
            {
                new Building(guid, string.Empty, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, vfm);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingDescriptionNullException2()
            {
                new Building(guid, code, null, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, vfm);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BuildingDescriptionEmptyException2() {
                new Building(guid, code, string.Empty, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, vfm);
            }

            [TestMethod]
            public void Building_NullLatitude_VerifyBuildingCoordinate()
            {
                bldg = new Building(guid, code, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, null, longi, imageUrl, addnlSvcs, vfm);
                Assert.IsNull(bldg.BuildingCoordinate);
            }

            [TestMethod]
            public void Building_NullLongitude_VerifyOfficeUseOnlyIsFalse()
            {
                bldg = new Building(guid, code, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, null, imageUrl, addnlSvcs, vfm);
                Assert.IsNull(bldg.BuildingCoordinate);
            }

            [TestMethod]
            public void Building_VfmNotY_VerifyOfficeUseOnlyIsFalse()
            {
                bldg = new Building(guid, code, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, "N");
                Assert.IsTrue(bldg.BuildingCoordinate.OfficeUseOnly);
            }

            [TestMethod]
            public void Building_VfmNull_VerifyOfficeUseOnlyIsFalse()
            {
                bldg = new Building(guid, code, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, null);
                Assert.IsTrue(bldg.BuildingCoordinate.OfficeUseOnly);
            }

            [TestMethod]
            public void Building_VfmEmpty_VerifyOfficeUseOnlyIsFalse()
            {
                bldg = new Building(guid, code, desc, locId, bldgType, longDesc, addressLines, city, state, postalCode, country, lati, longi, imageUrl, addnlSvcs, string.Empty);
                Assert.IsTrue(bldg.BuildingCoordinate.OfficeUseOnly);
            }
        }

        [TestClass]
        public class BuildingEquals 
        {
            private string guid;
            private string code;
            private string desc;
            private Building bldg1;
            private Building bldg2;
            private Building bldg3;

            [TestInitialize]
            public void Initialize() 
            {
                guid = Guid.NewGuid().ToString();
                code = "BLDG1";
                desc = "Building 1";
                bldg1 = new Building(guid, code, desc);
                bldg2 = new Building(guid, code, "Other Building");
                bldg3 = new Building(guid, "BLDG2", desc);
            }

            [TestMethod]
            public void BuildingSameCodesEqual() {
                Assert.IsTrue(bldg1.Equals(bldg2));
            }

            [TestMethod]
            public void BuildingDifferentCodeNotEqual() {
                Assert.IsFalse(bldg1.Equals(bldg3));
            }

        }

        [TestClass]
        public class BuildingGetHashCode 
        {
            private string guid;
            private string code;
            private string desc;
            private Building bldg1;
            private Building bldg2;
            private Building bldg3;

            [TestInitialize]
            public void Initialize() 
            {
                guid = Guid.NewGuid().ToString();
                code = "BLDG1";
                desc = "Building 1";
                bldg1 = new Building(guid, code, desc);
                bldg2 = new Building(guid, code, "Other Building");
                bldg3 = new Building(guid, "BLDG2", desc);
            }

            [TestMethod]
            public void BuildingSameCodeHashEqual() {
                Assert.AreEqual(bldg1.GetHashCode(), bldg2.GetHashCode());
            }

            [TestMethod]
            public void BuildingDifferentCodeHashNotEqual() {
                Assert.AreNotEqual(bldg1.GetHashCode(), bldg3.GetHashCode());
            }
        }
    }
}
