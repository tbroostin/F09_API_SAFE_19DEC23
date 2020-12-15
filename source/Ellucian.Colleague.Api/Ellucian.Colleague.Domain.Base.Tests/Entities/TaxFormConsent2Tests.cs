// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class TaxFormConsent2Tests
    {
        [TestMethod]
        public void TaxFormConsent21095C_Success()
        {
            var person = "0001234";
            var taxForm = TaxFormTypes.Form1095C;
            var status = true;
            var timeStamp = DateTime.Now;
            var TaxFormConsent2 = new TaxFormConsent2(person, taxForm, status, timeStamp);

            Assert.AreEqual(person, TaxFormConsent2.PersonId);
            Assert.AreEqual(taxForm, TaxFormConsent2.TaxForm);
            Assert.AreEqual(status, TaxFormConsent2.HasConsented);
            Assert.AreEqual(timeStamp, TaxFormConsent2.TimeStamp);
        }

        [TestMethod]
        public void TaxFormConsent2W2_Success()
        {
            var person = "0001234";
            var taxForm = TaxFormTypes.FormW2;
            var status = true;
            var timeStamp = DateTime.Now;
            var TaxFormConsent2 = new TaxFormConsent2(person, taxForm, status, timeStamp);

            Assert.AreEqual(person, TaxFormConsent2.PersonId);
            Assert.AreEqual(taxForm, TaxFormConsent2.TaxForm);
            Assert.AreEqual(status, TaxFormConsent2.HasConsented);
            Assert.AreEqual(timeStamp, TaxFormConsent2.TimeStamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TaxFormConsent2_EmptyPersonId()
        {
            var TaxFormConsent2 = new TaxFormConsent2("", TaxFormTypes.FormW2, true, DateTime.Now);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TaxFormConsent2_NullPersonId()
        {
            var TaxFormConsent2 = new TaxFormConsent2(null, TaxFormTypes.FormW2, true, DateTime.Now);
        }
    }
}
