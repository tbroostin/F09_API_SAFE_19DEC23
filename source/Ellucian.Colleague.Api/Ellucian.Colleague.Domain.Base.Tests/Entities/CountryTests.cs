// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
     [TestClass]
     public class CountryTests
     {
          private string code;
          private string description;
          private string isoCode;
          private Country country;

          [TestInitialize]
          public void Initialize()
          {
               code = "USA";
               description = "United States of America";
               isoCode = "USA";
               country = new Country(code, description, isoCode);
          }

          [TestClass]
          public class CountryConstructor : CountryTests
          {
               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public void CountryConstructorNullCode()
               {
                    country = new Country(null, description, isoCode);
               }

               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public void CountryConstructorEmptyCode()
               {
                    country = new Country(string.Empty, description, isoCode);
               }

               [TestMethod]
               public void CountryConstructorValidCode()
               {
                    country = new Country(code, description, isoCode);
                    Assert.AreEqual(code, country.Code);
               }

               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public void CountryConstructorNullDescription()
               {
                    country = new Country(code, null, isoCode);
               }

               [TestMethod]
               [ExpectedException(typeof(ArgumentNullException))]
               public void CountryConstructorEmptyDescription()
               {
                    country = new Country(code, string.Empty, isoCode);
               }

               [TestMethod]
               public void CountryConstructorValidDescription()
               {
                    country = new Country(code, description, isoCode);
                    Assert.AreEqual(description, country.Description);
               }

               [TestMethod]
               public void CountryCounstructorNullIsoCode()
               {
                    country = new Country(code, description, null);
                    Assert.AreEqual(code.Substring(0, 2), country.IsoCode);
               }

               [TestMethod]
               public void CountryCounstructorEmptyIsoCode()
               {
                    country = new Country(code, description, string.Empty);
                    Assert.AreEqual(code.Substring(0, 2), country.IsoCode);
               }

               [TestMethod]
               public void CountryCounstructorValidIsoCode()
               {
                    country = new Country(code, description, isoCode);
                    Assert.AreEqual(isoCode.Substring(0, 2), country.IsoCode);
               }

               [TestMethod]
               public void CountryConstructorOneLetterCodeNullIsoCode()
               {
                    code = "X";
                    description = "Some Country";
                    isoCode = null;
                    country = new Country(code, description, isoCode);
                    Assert.AreEqual(code, country.IsoCode);
               }


               [TestMethod]
               public void CountryConstructorOneLetterCodeEmptyIsoCode()
               {
                    code = "X";
                    description = "Some Country";
                    isoCode = "";
                    country = new Country(code, description, isoCode);
                    Assert.AreEqual(code, country.IsoCode);
               }

               [TestMethod]
               public void CountryIsNotInUse_TrueWhenProvided()
               {
                   country = new Country(code, description, isoCode, true);
                   Assert.IsTrue(country.IsNotInUse);
               }

               [TestMethod]
               public void CountryIsNotInUse_FalseWhenOmitted()
               {
                   country = new Country(code, description, isoCode);
                   Assert.IsFalse(country.IsNotInUse);
               }

               [TestMethod]
               public void CountryIsNotInUse_FalseWhenProvided()
               {
                   country = new Country(code, description, isoCode, false);
                   Assert.IsFalse(country.IsNotInUse);
               }
          }
     }
}
