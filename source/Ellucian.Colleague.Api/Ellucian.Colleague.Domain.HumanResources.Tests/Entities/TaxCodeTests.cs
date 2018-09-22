/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    [TestClass]
    public class TaxCodeTests
    {
        string code;
        string description;
        TaxCodeType type;
        TaxCodeFilingStatus filingStatus;
        TaxCode taxcode
        {
            get
            {
                return new TaxCode(code, description, type)
                {
                    FilingStatus = filingStatus
                };
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            code = "AU";
            description = "latansyap";
            type = TaxCodeType.CityWithholding;
            filingStatus = new TaxCodeFilingStatus("HH", "Head of Household");            
        }

        [TestMethod]
        public void PropertiesAreSet()
        {
            //taxcode = new TaxCode(code, description);
            Assert.AreEqual(code, taxcode.Code);
            Assert.AreEqual(description, taxcode.Description);
            Assert.AreEqual(type, taxcode.Type);
            Assert.AreEqual(filingStatus, taxcode.FilingStatus);
        }

        [TestMethod]
        public void Equals()
        {
            var code1 = taxcode;
            var code2 = taxcode;
            Assert.IsTrue(code1.Equals(code2));
        }

        [TestMethod]
        public void HashCode()
        {
           
            Assert.AreEqual(code.GetHashCode(), taxcode.GetHashCode());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CodeIsNull()
        {
            code = null;
            var error = taxcode;
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void DescriptionIsNull()
        {
            description = null;
            var error = taxcode;
        }
    }
}
