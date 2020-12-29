// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AddressTests
    {
        [TestClass]
        public class AddressTests_Constructor
        {
            private string addressId;

            // Non-required fields

            private string type;
            private List<string> addressLines;
            private string addressModifier;
            private string city;
            private string state;
            private string postalCode;
            private string country;
            private string routeCode;
            private List<string> addressLabel;
            private string personId;
            private DateTime? effectiveStartDate;
            private DateTime? effectiveEndDate;
            private Address address;
            private Phone phoneNumber;
            private string addressGuid;

            [TestInitialize]
            public void Initialize()
            {
                addressId = "123";
                personId = "0000304";
                type = "Home";
                addressLines = new List<string> { "4375 Fair Lakes Ct.", "Suite 400" };
                addressModifier = "Attn: Payroll Department";
                city = "Fairfax";
                state = "VA";
                postalCode = "22033";
                country = "USA";
                routeCode = "RR1";
                addressLabel = new List<string> { "4375 Fair Lakes Ct.", "Suite 400", "Fairfax, VA 22033" };
                personId = "0000304";
                effectiveStartDate = DateTime.Now;
                effectiveEndDate = DateTime.Now;
                phoneNumber = new Phone("703-815-4221", "HO", null);
                addressGuid = Guid.NewGuid().ToString();

                address = new Address(addressId, personId);

                address.Type = type;
                address.AddressLines = addressLines;
                address.AddressModifier = addressModifier;
                address.City = city;
                address.State = state;
                address.PostalCode = postalCode;
                address.Country = country;
                address.RouteCode = routeCode;
                address.AddressLabel = addressLabel;
                address.EffectiveStartDate = effectiveStartDate;
                address.EffectiveEndDate = effectiveEndDate;
                address.AddPhone(phoneNumber);
                address.Guid = addressGuid;
            }

            [TestMethod]
            public void AddressId()
            {
                Assert.AreEqual(addressId, address.AddressId);
            }

            [TestMethod]
            public void GuidAddress()
            {
                Assert.AreEqual(addressGuid, address.Guid);
            }

            [TestMethod]
            public void AddressType()
            {
                Assert.AreEqual(type, address.Type);
            }

            [TestMethod]
            public void AddressLines()
            {
                var addressLine1 = addressLines.ElementAt(0);
                var addressLine2 = addressLines.ElementAt(1);
                Assert.AreEqual(addressLine1, address.AddressLines.ElementAt(0));
                Assert.AreEqual(addressLine2, address.AddressLines.ElementAt(1));
            }

            [TestMethod]
            public void AddressModifier()
            {
                Assert.AreEqual(addressModifier, address.AddressModifier);
            }

            [TestMethod]
            public void AddressCity()
            {
                Assert.AreEqual(city, address.City);
            }

            [TestMethod]
            public void AddressState()
            {
                Assert.AreEqual(state, address.State);
            }

            [TestMethod]
            public void AddressPostalCode()
            {
                Assert.AreEqual(postalCode, address.PostalCode);
            }

            [TestMethod]
            public void AddressCountry()
            {
                Assert.AreEqual(country, address.Country);
            }

            [TestMethod]
            public void AddressRouteCode()
            {
                Assert.AreEqual(routeCode, address.RouteCode);
            }

            [TestMethod]
            public void AddressLabel()
            {
                var label1 = addressLabel.ElementAt(0);
                var label2 = addressLabel.ElementAt(1);
                var label3 = addressLabel.ElementAt(2);
                Assert.AreEqual(label1, address.AddressLabel.ElementAt(0));
                Assert.AreEqual(label2, address.AddressLabel.ElementAt(1));
                Assert.AreEqual(label3, address.AddressLabel.ElementAt(2));
            }

            [TestMethod]
            public void AddressPersonId()
            {
                Assert.AreEqual(personId, address.PersonId);
            }

            [TestMethod]
            public void AddressEffectiveStartDate()
            {
                Assert.AreEqual(effectiveStartDate, address.EffectiveStartDate);
            }

            [TestMethod]
            public void AddressEffectiveEndDate()
            {
                Assert.AreEqual(effectiveEndDate, address.EffectiveEndDate);
            }

            [TestMethod]
            public void PhoneNumber()
            {
                Assert.AreEqual(phoneNumber, address.PhoneNumbers.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddressIdCodeNullException()
            {
                new Address(null, personId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdCodeNullException()
            {
                new Address(addressId, null);
            }
        }

        [TestClass]
        public class AddressTests_Equals
        {
            private string addressId;
            private string type;
            private string typeCode;
            private List<string> addressLines;
            private string addressModifier;
            private string city;
            private string state;
            private string postalCode;
            private string routeCode;
            private string countryCode;
            private string county;
            private List<string> addressLabel;
            private string personId;
            private DateTime? effectiveStartDate;
            private DateTime? effectiveEndDate;
            private Phone phoneNumber;
            private Address address;
            private Address address2;
            private string addressGuid;

            [TestInitialize]
            public void Initialize()
            {
                addressId = "123";
                personId = "0000304";
                typeCode = "H";
                type = "Home";
                addressLines = new List<string> { "4375 Fair Lakes Ct.", "Suite 400" };
                addressModifier = "Attn: Payroll Department";
                city = "Fairfax";
                state = "VA";
                postalCode = "22033";
                routeCode = "RR1";
                countryCode = "USA";
                county = "FFX";
                addressLabel = new List<string> { "4375 Fair Lakes Ct.", "Suite 400", "Fairfax, VA 22033" };
                personId = "0000304";
                effectiveStartDate = DateTime.Now;
                effectiveEndDate = DateTime.Now;
                phoneNumber = new Phone("703-815-4221", "HO", null);
                addressGuid = Guid.NewGuid().ToString();

                address = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate,
                    Guid = addressGuid
                };
            }

            [TestMethod]
            public void AddressEquals_PersonDifferent_Equal()
            {
                address2 = new Address(addressId, "0000999")
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate,
                };
                Assert.IsTrue(address.Equals(address2));
            }

            [TestMethod]
            public void AddressEquals_TypeCodeDifferent_NotEqual()
            {
                address2 = new Address(addressId, personId)
                {
                    TypeCode = "X",
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
            }


            [TestMethod]
            public void AddressEquals_TypeCodeNullOrEmpty_Equal()
            {
                address.TypeCode = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = string.Empty,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsTrue(address.Equals(address2));
                Assert.IsTrue(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_NoExceptionWithNullTypeCode()
            {
                address.TypeCode = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
                Assert.IsFalse(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_AddressLinesDifferent_NotEqual()
            {
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = new List<string>() { "line1", "line2" },
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
            }

            [TestMethod]
            public void AddressEquals_AddressLinesNullOrEmpty_Equal()
            {
                address.AddressLines = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = new List<string>(),
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsTrue(address.Equals(address2));
                Assert.IsTrue(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_NoExceptionWithNullAddressLines()
            {
                address.AddressLines = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    County = county,
                    RouteCode = routeCode,
                    CountryCode = countryCode,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
                Assert.IsFalse(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_AddressModifierDifferent_NotEqual()
            {
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = "modifier line",
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
            }


            [TestMethod]
            public void AddressEquals_AddressModifierNullorEmpty_Equal()
            {
                address.AddressModifier = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = string.Empty,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsTrue(address.Equals(address2));
                Assert.IsTrue(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_NoExceptionWithNullAddressModifier()
            {
                address.AddressModifier = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
                Assert.IsFalse(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_CityDifferent_NotEqual()
            {
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = "differentCity",
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
            }

            [TestMethod]
            public void AddressEquals_CityNullOrEmpty_Equal()
            {
                address.City = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = string.Empty,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsTrue(address.Equals(address2));
                Assert.IsTrue(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_NoExceptionWithNullCity()
            {
                address.City = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
                Assert.IsFalse(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_StateDifferent_NotEqual()
            {
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = "differentState",
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
            }


            [TestMethod]
            public void AddressEquals_StateNullOrEmpty_Equal()
            {
                address.State = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = string.Empty,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsTrue(address.Equals(address2));
                Assert.IsTrue(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_NoExceptionWithNullState()
            {
                address.State = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
                Assert.IsFalse(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_PostalCodeDifferent_NotEqual()
            {
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = "99999",
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
            }

            [TestMethod]
            public void AddressEquals_PostalCodeNullOrEmpty_Equal()
            {
                address.PostalCode = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = string.Empty,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsTrue(address.Equals(address2));
                Assert.IsTrue(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_NoExceptionWithNullPostalCode()
            {
                address.PostalCode = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
                Assert.IsFalse(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_CountryCodeDifferent_NotEqual()
            {
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = "CANADA",
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
            }

            [TestMethod]
            public void AddressEquals_CountryCodeNullOrEmpty_Equal()
            {
                address.CountryCode = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = "",
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsTrue(address.Equals(address2));
                Assert.IsTrue(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_NoExceptionWithNullCountryCode()
            {
                address.CountryCode = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
                Assert.IsFalse(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_CountyDifferent_NotEqual()
            {
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = "XYZ",
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
            }


            [TestMethod]
            public void AddressEquals_CountyNullOrEmpty_Equal()
            {
                address.County = string.Empty;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = null,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsTrue(address.Equals(address2));
                Assert.IsTrue(address2.Equals(address));
            }

            [TestMethod]
            public void AddressEquals_NoExceptionWithNullCounty()
            {
                address.County = null;
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                Assert.IsFalse(address.Equals(address2));
                Assert.IsFalse(address2.Equals(address));
            }

            //[TestMethod]
            //public void AddressEquals_EffectiveStartDifferent_NotEqual()
            //{
            //    address2 = new Address(addressId, personId)
            //    {
            //        TypeCode = typeCode,
            //        AddressLines = addressLines,
            //        AddressModifier = addressModifier,
            //        City = city,
            //        State = state,
            //        PostalCode = postalCode,
            //        CountryCode = countryCode,
            //        RouteCode = routeCode,
            //        County = county,
            //        EffectiveStartDate = new DateTime(2012,1,1),
            //        EffectiveEndDate = effectiveEndDate
            //    };
            //    Assert.IsFalse(address.Equals(address2));
            //}

            //[TestMethod]
            //public void AddressEquals_EffectiveStartNull_Equal()
            //{
            //    address.EffectiveStartDate = null;
            //    address2 = new Address(addressId, personId)
            //    {
            //        TypeCode = typeCode,
            //        AddressLines = addressLines,
            //        AddressModifier = addressModifier,
            //        City = city,
            //        State = state,
            //        PostalCode = postalCode,
            //        CountryCode = countryCode,
            //        RouteCode = routeCode,
            //        County = county,
            //        EffectiveStartDate = null,
            //        EffectiveEndDate = effectiveEndDate
            //    };
            //    Assert.IsTrue(address.Equals(address2));
            //    Assert.IsTrue(address2.Equals(address));
            //}

            //[TestMethod]
            //public void AddressEquals_NoExceptionForEffectiveStart()
            //{
            //    address.EffectiveStartDate = null;
            //    address2 = new Address(addressId, personId)
            //    {
            //        TypeCode = typeCode,
            //        AddressLines = addressLines,
            //        AddressModifier = addressModifier,
            //        City = city,
            //        State = state,
            //        PostalCode = postalCode,
            //        CountryCode = countryCode,
            //        RouteCode = routeCode,
            //        County = county,
            //        EffectiveStartDate = effectiveStartDate,
            //        EffectiveEndDate = effectiveEndDate
            //    };
            //    Assert.IsFalse(address.Equals(address2));
            //    Assert.IsFalse(address2.Equals(address));
            //}

            //[TestMethod]
            //public void AddressEquals_EffectiveEndDifferent_NotEqual()
            //{
            //    address.EffectiveEndDate = null;
            //    address2 = new Address(addressId, personId)
            //    {
            //        TypeCode = typeCode,
            //        AddressLines = addressLines,
            //        AddressModifier = addressModifier,
            //        City = city,
            //        State = state,
            //        PostalCode = postalCode,
            //        CountryCode = countryCode,
            //        RouteCode = routeCode,
            //        County = county,
            //        EffectiveStartDate = effectiveStartDate,
            //        EffectiveEndDate = new DateTime(2001,1,1)
            //    };
            //    Assert.IsFalse(address2.Equals(address));
            //}

            //[TestMethod]
            //public void AddressEquals_EffectiveEndNull_Equal()
            //{
            //    address.EffectiveEndDate = null;
            //    address2 = new Address(addressId, personId)
            //    {
            //        TypeCode = typeCode,
            //        AddressLines = addressLines,
            //        AddressModifier = addressModifier,
            //        City = city,
            //        State = state,
            //        PostalCode = postalCode,
            //        CountryCode = countryCode,
            //        RouteCode = routeCode,
            //        County = county,
            //        EffectiveStartDate = effectiveStartDate,
            //        EffectiveEndDate = null
            //    };
            //    Assert.IsTrue(address.Equals(address2));
            //    Assert.IsTrue(address2.Equals(address));
            //}

            //[TestMethod]
            //public void AddressEquals_NoExceptionForEffectiveEnd()
            //{
            //    address.EffectiveEndDate = null;
            //    address2 = new Address(addressId, personId)
            //    {
            //        TypeCode = typeCode,
            //        AddressLines = addressLines,
            //        AddressModifier = addressModifier,
            //        City = city,
            //        State = state,
            //        PostalCode = postalCode,
            //        CountryCode = countryCode,
            //        RouteCode = routeCode,
            //        County = county,
            //        EffectiveStartDate = effectiveStartDate,
            //        EffectiveEndDate = effectiveEndDate
            //    };
            //    Assert.IsFalse(address.Equals(address2));
            //    Assert.IsFalse(address2.Equals(address));
            //}

            [TestMethod]
            public void AddressEquals_PhoneNumbersDifferent_NotEqual()
            {
                address.AddPhone(phoneNumber);
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                var phoneNumber2 = new Phone("703-815-4221", "HO", "123");
                address2.AddPhone(phoneNumber2);
                Assert.IsFalse(address.Equals(address2));
            }

            [TestMethod]
            public void AddressEquals_PhoneNumbersSame_Equal()
            {
                address.AddPhone(phoneNumber);
                address2 = new Address(addressId, personId)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = county,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate
                };
                address2.AddPhone(phoneNumber);
                Assert.IsTrue(address.Equals(address2));
            }

            [TestMethod]
            public void AddressId_Alternative_Constructor()
            {

                address2 = new Address(addressId, false)
                {
                    TypeCode = typeCode,
                    AddressLines = addressLines,
                    AddressModifier = addressModifier,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    CountryCode = countryCode,
                    RouteCode = routeCode,
                    County = null,
                    EffectiveStartDate = effectiveStartDate,
                    EffectiveEndDate = effectiveEndDate,
                    Guid = addressGuid
                };
                Assert.AreEqual(address.Guid, address2.Guid);
                Assert.AreEqual(address.AddressId, address2.AddressId);
                Assert.AreEqual(address.TypeCode, address2.TypeCode);
                Assert.AreEqual(address.AddressLines, address2.AddressLines);
                Assert.AreEqual(address.City, address2.City);
                Assert.AreEqual(address.State, address2.State);
                Assert.AreEqual(address.PostalCode, address2.PostalCode);
                Assert.AreEqual(address.Country, address2.Country);
                Assert.AreEqual(address.CountryCode, address2.CountryCode);
                Assert.AreEqual(address.RouteCode, address2.RouteCode);
                Assert.AreEqual(address.EffectiveEndDate, address2.EffectiveEndDate);
                Assert.AreEqual(address.EffectiveStartDate, address2.EffectiveStartDate);
                Assert.AreEqual(address.IsPreferredAddress, address2.IsPreferredAddress);

            }
        }
    }
}