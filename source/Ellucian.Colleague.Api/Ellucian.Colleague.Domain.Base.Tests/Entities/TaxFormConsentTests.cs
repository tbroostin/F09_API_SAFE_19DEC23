// Copyright 2015 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class TaxFormConsentTests
    {
        [TestMethod]
        public void TaxFormConsent1095C_Success()
        {
            var person = "0001234";
            var taxForm = TaxForms.Form1095C;
            var status = true;
            var timeStamp = DateTime.Now;
            var taxFormConsent = new TaxFormConsent(person,taxForm,status,timeStamp);

            Assert.AreEqual(person, taxFormConsent.PersonId);
            Assert.AreEqual(taxForm, taxFormConsent.TaxForm);
            Assert.AreEqual(status, taxFormConsent.HasConsented);
            Assert.AreEqual(timeStamp, taxFormConsent.TimeStamp);
        }

        [TestMethod]
        public void TaxFormConsentW2_Success()
        {
            var person = "0001234";
            var taxForm = TaxForms.FormW2;
            var status = true;
            var timeStamp = DateTime.Now;
            var taxFormConsent = new TaxFormConsent(person, taxForm, status, timeStamp);

            Assert.AreEqual(person, taxFormConsent.PersonId);
            Assert.AreEqual(taxForm, taxFormConsent.TaxForm);
            Assert.AreEqual(status, taxFormConsent.HasConsented);
            Assert.AreEqual(timeStamp, taxFormConsent.TimeStamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TaxFormConsent_EmptyPersonId()
        {
            var taxFormConsent = new TaxFormConsent("", TaxForms.FormW2, true, DateTime.Now);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TaxFormConsent_NullPersonId()
        {
            var taxFormConsent = new TaxFormConsent(null, TaxForms.FormW2, true, DateTime.Now);
        }
    }
}
