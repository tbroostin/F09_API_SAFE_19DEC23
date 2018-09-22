using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities {
    [TestClass]
    public class ImportantNumberTests {

        // do a test class for impB and one for impN
        [TestClass]
        public class ImportantNumberWithBuildingConstructor {
            private string id;
            private string name;
            private string city;
            private string state;
            private string postalCode;
            private string country;
            private string phone;
            private string extension;
            private string category;
            private string email;
            private string buildingCode;
            private string visibleToMobile;
            private string locationCode;
            private ImportantNumber imp;

            [TestInitialize]
            public void Initialize() {
                id = "1";
                name = "SAM";
                phone = "703-555-1212";
                extension = "x123";
                category = "ADMIN";
                email = "destin@some.edu";
                buildingCode = "AND";
                visibleToMobile = "Y";
                city = string.Empty;
                state = string.Empty; ;
                postalCode = string.Empty; ;
                country = string.Empty; ;
                locationCode = string.Empty; ;
                imp = new ImportantNumber(id, name, phone, extension, category, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            public void Name() {
                Assert.AreEqual(name, imp.Name);
            }
            [TestMethod]
            public void City() {
                Assert.IsNull(imp.City);
            }

            [TestMethod]
            public void State() {
                Assert.IsNull(imp.State);
            }

            [TestMethod]
            public void PostalCode() {
                Assert.IsNull(imp.PostalCode);
            }

            [TestMethod]
            public void Country() {
                Assert.IsNull(imp.Country);
            }

            [TestMethod]
            public void Get_Phone() {
                Assert.AreEqual(phone, imp.Phone);
            }

            [TestMethod]
            public void Get_Extension() {
                Assert.AreEqual(extension, imp.Extension);
            }

            [TestMethod]
            public void Get_Category() {
                Assert.AreEqual(category, imp.Category);
            }

            [TestMethod]
            public void Get_Email() {
                Assert.AreEqual(email, imp.Email);
            }

            [TestMethod]
            public void Get_AddressLines() {
                Assert.IsNull(imp.AddressLines);
            }

            [TestMethod]
            public void Get_BuildingCode() {
                Assert.AreEqual(buildingCode, imp.BuildingCode);
            }

            [TestMethod]
            public void Get_OfficeUseOnly_Y() 
            {
                imp = new ImportantNumber(id, name, phone, extension, category, email, buildingCode, "Y");
                Assert.AreEqual(false, imp.OfficeUseOnly);
            }

            [TestMethod]
            public void Get_OfficeUseOnly_N()
            {
                imp = new ImportantNumber(id, name, phone, extension, category, email, buildingCode, "N");
                Assert.AreEqual(true, imp.OfficeUseOnly);
            }

            [TestMethod]
            public void Get_Latitude() {
                Assert.IsNull(imp.Latitude);
            }

            [TestMethod]
            public void Get_Longitude() {
                Assert.IsNull(imp.Longitude);
            }

            [TestMethod]
            public void Get_LocationCode() {
                Assert.IsNull(imp.LocationCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdNullException() {
                new ImportantNumber(null, name, phone, extension, category, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdEmptyException()
            {
                new ImportantNumber(string.Empty, name, phone, extension, category, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameNullException() {
                new ImportantNumber(id, null, phone, extension, category, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameEmptyException()
            {
                new ImportantNumber(id, string.Empty, phone, extension, category, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CategoryNullException() {
                new ImportantNumber(id, name, phone, extension, null, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CategoryEmptyException()
            {
                new ImportantNumber(id, name, phone, extension, string.Empty, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PhoneNullException() {
                new ImportantNumber(id, name, null, extension, category, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PhoneEmptyException()
            {
                new ImportantNumber(id, name, string.Empty, extension, category, email, buildingCode, visibleToMobile);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void VisibleFlagNullException() {
                new ImportantNumber(id, name, phone, extension, category, email, buildingCode, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void VisibleFlagEmptyException()
            {
                new ImportantNumber(id, name, phone, extension, category, email, buildingCode, string.Empty);
            }
        }

        [TestClass]
        public class ImportantNumberNoBuildingConstructor {
            private string id;
            private string name;
            private string city;
            private string state;
            private string postalCode; 
            private string country;
            private string phone; 
            private string extension; 
            private string category;
            private string email; 
            private List<string> addressLines; 
            private string buildingCode;
            private string visibleToMobile;
            private decimal? latitude; 
            private decimal? longitude;
            private string locationCode;
            private ImportantNumber imp;

            [TestInitialize]
            public void Initialize() {
                id = "1";
                name = "SAM";
                city = "Fairfax";
                state = "VA";
                postalCode = "22033";
                country = "USA";
                phone = "703-555-1212";
                extension = "x123";
                category = "ADMIN";
                email = "destin@some.edu";
                addressLines = new List<string>(); addressLines.Add("Line 1"); addressLines.Add("Line 2");
                buildingCode = string.Empty;
                visibleToMobile = "Y";
                latitude = new decimal?(77.234132M);
                longitude = new decimal?(-114.123987M);
                locationCode = "MC";
                imp = new ImportantNumber(id, name, city, state, postalCode, country, phone, extension, category, email, addressLines, visibleToMobile, latitude, longitude, locationCode);
            }

            [TestMethod]
            public void Name() {
                Assert.AreEqual(name, imp.Name);
            }

            [TestMethod]
            public void City() {
                Assert.AreEqual(city, imp.City);
            }

            [TestMethod]
            public void State() {
                Assert.AreEqual(state, imp.State);
            }

            [TestMethod]
            public void PostalCode() {
                Assert.AreEqual(postalCode, imp.PostalCode);
            }

            [TestMethod]
            public void Country() {
                Assert.AreEqual(country, imp.Country);
            }

            [TestMethod]
            public void Get_Phone() {
                Assert.AreEqual(phone, imp.Phone);
            }

            [TestMethod]
            public void Get_Extension() {
                Assert.AreEqual(extension, imp.Extension);
            }

            [TestMethod]
            public void Get_Category() {
                Assert.AreEqual(category, imp.Category);
            }

            [TestMethod]
            public void Get_Email() {
                Assert.AreEqual(email, imp.Email);
            }

            [TestMethod]
            public void Get_AddressLines() {
                for (int i = 0; i < addressLines.Count; i++) {
                    Assert.AreEqual(addressLines[i], imp.AddressLines[i]);
                }
            }

            [TestMethod]
            public void Get_BuildingCode() {
                Assert.IsNull(imp.BuildingCode);
            }

            [TestMethod]
            public void Get_OfficeUseOnly_Y() {
                imp = new ImportantNumber(id, name, city, state, postalCode, country, phone, extension, category, email, addressLines, "Y", latitude, longitude, locationCode);
                Assert.AreEqual(false, imp.OfficeUseOnly);
            }

            [TestMethod]
            public void Get_OfficeUseOnly_N()
            {
                imp = new ImportantNumber(id, name, city, state, postalCode, country, phone, extension, category, email, addressLines, "N", latitude, longitude, locationCode);
                Assert.AreEqual(true, imp.OfficeUseOnly);
            }

            [TestMethod]
            public void Get_Latitude() {
                Assert.AreEqual(latitude, imp.Latitude);
            }

            [TestMethod]
            public void Get_Longitude() {
                Assert.AreEqual(longitude, imp.Longitude);
            }

            [TestMethod]
            public void Get_LocationCode() {
                Assert.AreEqual(locationCode, imp.LocationCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdNullException() {
                new ImportantNumber(null, name, city, state, postalCode, country, phone, extension, category, email, addressLines, visibleToMobile, latitude, longitude, locationCode); 
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdEmptyException()
            {
                new ImportantNumber(string.Empty, name, city, state, postalCode, country, phone, extension, category, email, addressLines, visibleToMobile, latitude, longitude, locationCode);
            }
 
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameNullException() {
                new ImportantNumber(id, null, city, state, postalCode, country, phone, extension, category, email, addressLines, visibleToMobile, latitude, longitude, locationCode); 
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameEmptyException()
            {
                new ImportantNumber(id, string.Empty, city, state, postalCode, country, phone, extension, category, email, addressLines, visibleToMobile, latitude, longitude, locationCode);
            }
 
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CategoryNullException() {
                new ImportantNumber(id, name, city, state, postalCode, country, phone, extension, null, email, addressLines, visibleToMobile, latitude, longitude, locationCode); 
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CategoryEmptyException()
            {
                new ImportantNumber(id, name, city, state, postalCode, country, phone, extension, string.Empty, email, addressLines, visibleToMobile, latitude, longitude, locationCode);
            }
 
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PhoneNullException() {
                new ImportantNumber(id, name, city, state, postalCode, country, null, extension, category, email, addressLines, visibleToMobile, latitude, longitude, locationCode); 
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PhoneEmptyException()
            {
                new ImportantNumber(id, name, city, state, postalCode, country, string.Empty, extension, category, email, addressLines, visibleToMobile, latitude, longitude, locationCode);
            }
 
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void VisibleFlagNullException() {
                new ImportantNumber(id, name, city, state, postalCode, country, phone, extension, category, email, addressLines, null, latitude, longitude, locationCode); 
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void VisibleFlagEmptyException()
            {
                new ImportantNumber(id, name, city, state, postalCode, country, phone, extension, category, email, addressLines, string.Empty, latitude, longitude, locationCode);
            }
 
        }

        [TestClass]
        public class ImportantNumberEquals {
            private string id;
            private string name;
            private string phone; 
            private string category;
            private string building;
            private string visibleToMobile;
            private ImportantNumber imp1;
            private ImportantNumber imp2;
            private ImportantNumber imp3;

            [TestInitialize]
            public void Initialize() {
                id = "1";
                name = "SAM";
                phone = "703-555-1212";
                building = "AND";
                category = "ADMIN";

                visibleToMobile = "Y";
                imp1 = new ImportantNumber(id, name, phone, null, category, null, building, visibleToMobile);
                imp2 = new ImportantNumber(id, "Other", phone, null, category, null, building, visibleToMobile);
                imp3 = new ImportantNumber("2", name, phone, null, category, null, building, visibleToMobile); 
            }

            [TestMethod]
            public void ImpNumSameIdEqual() {
                Assert.IsTrue(imp1.Equals(imp2));
            }

            [TestMethod]
            public void ImpNumDifferentIdNotEqual() {
                Assert.IsFalse(imp1.Equals(imp3));
            }

            [TestMethod]
            public void ImpNumNullNotEqual()
            {
                Assert.IsFalse(imp1.Equals(null));
            }

            [TestMethod]
            public void ImpNumNonImpNumNotEqual()
            {
                Assert.IsFalse(imp1.Equals("abc"));
            }
        }

        [TestClass]
        public class ImportantNumberGetHashCode {
            private string id;
            private string name;
            private string phone;
            private string building;
            private string category;
            private string visibleToMobile;
            private ImportantNumber imp1;
            private ImportantNumber imp2;
            private ImportantNumber imp3;

            [TestInitialize]
            public void Initialize() {
                id = "1";
                name = "SAM";
                phone = "703-555-1212";
                category = "ADMIN";
                building = "AND";
                visibleToMobile = "Y";
                imp1 = new ImportantNumber(id, name, phone, null, category, null, building, visibleToMobile);
                imp2 = new ImportantNumber(id, "Other", phone, null, category, null, building, visibleToMobile);
                imp3 = new ImportantNumber("2", name, phone, null, category, null, building, visibleToMobile); 
            }

            [TestMethod]
            public void ImpNumSameIdHashEqual() {
                Assert.AreEqual(imp1.GetHashCode(), imp2.GetHashCode());
            }

            [TestMethod]
            public void ImpNumDifferentIdHashNotEqual() {
                Assert.AreNotEqual(imp1.GetHashCode(), imp3.GetHashCode());
            }
        }
    }
}
