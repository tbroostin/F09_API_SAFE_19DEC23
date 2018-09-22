// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class InvoicePaymentTests
    {
        string id = "123456789";
        string personId = "1234567";
        string accountTypeCode = "01";
        string term = "2013/FA";
        string referenceNumber = "000067890";
        decimal amountPaid = 1000;
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
        public void InvoicePayment_Constructor_NullId()
        {
            var result = new InvoicePayment(null, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_EmptyId()
        {
            var result = new InvoicePayment(String.Empty, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_IdValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, billingStart, dueDate, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_NullPersonId()
        {
            var result = new InvoicePayment(id, null, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_EmptyPersonId()
        {
            var result = new InvoicePayment(id, String.Empty, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_PersonIdValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_NullAccountTypeCode()
        {
            var result = new InvoicePayment(id, personId, null, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_EmptyAccountTypeCode()
        {
            var result = new InvoicePayment(id, personId, String.Empty, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_AccountTypeCodeValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(accountTypeCode, result.ReceivableTypeCode);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_NullTermValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, null, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(null, result.TermId);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_EmptyTermValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, String.Empty, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(String.Empty, result.TermId);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_TermValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(term, result.TermId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_NullReferenceNumber()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, null, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_EmptyReferenceNumber()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, string.Empty, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_ReferenceNumberValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_DateValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(date, result.Date);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_DueDateValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(dueDate, result.DueDate);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_BillingStartValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(billingStart, result.BillingStart);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_BillingEndValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(billingEnd, result.BillingEnd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvoicePayment_Constructor_BadBillingDates()
        {
            billingStart = billingEnd.AddDays(1);
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_NullDescription()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, null, charges, amountPaid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_EmptyDescription()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, String.Empty, charges, amountPaid);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_DescriptionValid()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(description, result.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_NullCharges()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, null, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_Constructor_NoCharges()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, new List<Charge>(), 0);
        }

        [TestMethod]
        public void InvoicePayment_Constructor_ChargesValid()
        {
            var charges = new List<Charge>() { charge1, charge2 };
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.IsNotNull(result.Charges);
            CollectionAssert.AreEqual(charges, result.Charges.ToList());
        }

        [TestMethod]
        public void InvoicePayment_BaseAmount_Valid()
        {
            var charges = new List<Charge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(baseAmount, result.BaseAmount);
        }

        [TestMethod]
        public void InvoicePayment_TaxAmount_Valid()
        {
            charge1.TaxAmount = 5;
            var charges = new List<Charge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var taxAmount = charge1.TaxAmount + charge2.TaxAmount;
            var amount = baseAmount + taxAmount;
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(taxAmount, result.TaxAmount);
        }

        [TestMethod]
        public void InvoicePayment_Amount_Valid()
        {
            charge1.TaxAmount = 5;
            var charges = new List<Charge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var taxAmount = charge1.TaxAmount + charge2.TaxAmount;
            var amount = baseAmount + taxAmount;
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        public void InvoicePayment_AmountPaid_Valid()
        {
            charge1.TaxAmount = 5;
            var charges = new List<Charge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var taxAmount = charge1.TaxAmount + charge2.TaxAmount;
            var amount = baseAmount + taxAmount;
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);

            Assert.AreEqual(amountPaid, result.AmountPaid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_AddAdjustingInvoicePayment_NullValue()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
            result.AddAdjustingInvoice(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvoicePayment_AddAdjustingInvoicePayment_EmptyValue()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
            result.AddAdjustingInvoice(String.Empty);
        }

        [TestMethod]
        public void InvoicePayment_AddAdjustingInvoicePayment_ValidValue()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
            foreach (var adj in adjustmentIds)
            {
                result.AddAdjustingInvoice(adj);
            }

            CollectionAssert.AreEqual(adjustmentIds, result.AdjustedByInvoices.ToList());
        }

        [TestMethod]
        public void InvoicePayment_AddAdjustingInvoicePayment_ValidNoDups()
        {
            var result = new InvoicePayment(id, personId, accountTypeCode, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges, amountPaid);
            foreach (var adj in adjustmentIdsWithDup)
            {
                result.AddAdjustingInvoice(adj);
            }

            CollectionAssert.AreEqual(adjustmentIds, result.AdjustedByInvoices.ToList());
        }
    }
}
