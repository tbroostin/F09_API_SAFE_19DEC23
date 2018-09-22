// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class InvoiceTests
    {
        string id = "123456789";
        string personId = "1234567";
        string accountTypeCode = "01";
        string term = "2013/FA";
        string referenceNumber = "000067890";
        DateTime date = DateTime.Parse("07/31/2013");
        DateTime dueDate = DateTime.Parse("08/15/2013");
        DateTime billingEnd = DateTime.Parse("12/31/2013");
        DateTime billingStart = DateTime.Parse("08/01/2013");
        string description = "Registration - 2013/FA";
        List<string> adjustmentIds = new List<string>() { "45678", "34567" };
        List<string> adjustmentIdsWithNull = new List<string>() { "45678", null, "34567" };
        List<string> adjustmentIdsWithEmpty = new List<string>() { "45678", "", "34567" };
        List<string> adjustmentIdsWithDup = new List<string>() { "45678", "34567", "45678" };

        Charge charge1 = new Charge("24680", "123456789", new List<String>() {"Full-Time Tuition", "BIOL-101-01"}, "TUIFT", 1875);
        Charge charge2 = new Charge("13579", "123456789", new List<String>() {"Activities Fee"}, "ACTFE", 175);
        List<Charge> charges = new List<Charge>() { new Charge("24680", "123456789", new List<String>() { "Full-Time Tuition" }, "TUIFT", 1875) };
        List<Charge> charges2 = new List<Charge>() { new Charge("24680", "123456789", new List<String>() { "Full-Time Tuition" }, "TUIFT", 1875), new Charge("13579", "123456789", new List<String>() { "Activities Fee" }, "ACTFE", 175) };
        List<Charge> charges3 = new List<Charge>() { new Charge("13579", "123456789", new List<String>() { "Activities Fee" }, "ACTFE", 175) };
        List<Charge> charges4 = new List<Charge>() { new Charge("13579", "123456789", new List<String>() { "Activities Fee" }, "ACTFE", 175), new Charge("13580", "123456789", new List<String>() { "Technology Fee" }, "TECFE", 225) };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_NullId()
        {
            var result = new Invoice(null, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_EmptyId()
        {
            var result = new Invoice(String.Empty, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        public void Invoice_Constructor_IdValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, billingStart, dueDate, billingEnd, description, charges);

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_NullPersonId()
        {
            var result = new Invoice(id, null, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_EmptyPersonId()
        {
            var result = new Invoice(id, String.Empty, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        public void Invoice_Constructor_PersonIdValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_NullAccountTypeCode()
        {
            var result = new Invoice(id, personId, null, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_EmptyAccountTypeCode()
        {
            var result = new Invoice(id, personId, String.Empty, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        public void Invoice_Constructor_AccountTypeCodeValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(accountTypeCode, result.ReceivableTypeCode);
        }

        [TestMethod]
        public void Invoice_Constructor_NullTermValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, null, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(null, result.TermId);
        }

        [TestMethod]
        public void Invoice_Constructor_EmptyTermValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, String.Empty, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(String.Empty, result.TermId);
        }

        [TestMethod]
        public void Invoice_Constructor_TermValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(term, result.TermId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_NullReferenceNumber()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, null, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_EmptyReferenceNumber()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, string.Empty, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        public void Invoice_Constructor_ReferenceNumberValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        public void Invoice_Constructor_DateValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(date, result.Date);
        }

        [TestMethod]
        public void Invoice_Constructor_DueDateValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(dueDate, result.DueDate);
        }

        [TestMethod]
        public void Invoice_Constructor_BillingStartValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(billingStart, result.BillingStart);
        }

        [TestMethod]
        public void Invoice_Constructor_BillingEndValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(billingEnd, result.BillingEnd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Invoice_Constructor_BadBillingDates()
        {
            billingStart = billingEnd.AddDays(1);
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_NullDescription()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, null, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_EmptyDescription()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, String.Empty, charges);
        }

        [TestMethod]
        public void Invoice_Constructor_DescriptionValid()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(description, result.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_NullCharges()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_Constructor_NoCharges()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, new List<Charge>());
        }

        [TestMethod]
        public void Invoice_Constructor_ChargesValid()
        {
            var charges = new List<Charge>() { charge1, charge2 };
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.IsNotNull(result.Charges);
            CollectionAssert.AreEqual(charges, result.Charges.ToList());
        }

        [TestMethod]
        public void Invoice_BaseAmount_Valid()
        {
            var charges = new List<Charge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(baseAmount, result.BaseAmount);
        }

        [TestMethod]
        public void Invoice_TaxAmount_Valid()
        {
            charge1.TaxAmount = 5;
            var charges = new List<Charge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var taxAmount = charge1.TaxAmount + charge2.TaxAmount;
            var amount = baseAmount + taxAmount;
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(taxAmount, result.TaxAmount);
        }

        [TestMethod]
        public void Invoice_Amount_Valid()
        {
            charge1.TaxAmount = 5;
            var charges = new List<Charge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var taxAmount = charge1.TaxAmount + charge2.TaxAmount;
            var amount = baseAmount + taxAmount;
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_AddAdjustingInvoice_NullValue()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
            result.AddAdjustingInvoice(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoice_AddAdjustingInvoice_EmptyValue()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
            result.AddAdjustingInvoice(String.Empty);
        }

        [TestMethod]
        public void Invoice_AddAdjustingInvoice_ValidValue()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
            foreach (var adj in adjustmentIds)
            {
                result.AddAdjustingInvoice(adj);
            }

            CollectionAssert.AreEqual(adjustmentIds, result.AdjustedByInvoices.ToList());
        }

        [TestMethod]
        public void Invoice_AddAdjustingInvoice_ValidNoDups()
        {
            var result = new Invoice(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges);
            foreach (var adj in adjustmentIdsWithDup)
            {
                result.AddAdjustingInvoice(adj);
            }

            CollectionAssert.AreEqual(adjustmentIds, result.AdjustedByInvoices.ToList());
        }
    }
}
