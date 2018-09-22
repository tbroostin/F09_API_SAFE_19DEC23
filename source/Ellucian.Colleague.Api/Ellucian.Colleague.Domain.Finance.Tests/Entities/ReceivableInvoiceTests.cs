// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ReceivableInvoiceTests
    {
        const string id = "123456789";
        const string referenceNumber = "000067890";
        const string personId = "1234567";
        const string receivableType = "01";
        const string term = "2013/FA";
        readonly DateTime date = DateTime.Parse("07/31/2013");
        readonly DateTime dueDate = DateTime.Parse("08/15/2013");
        DateTime billingStart = DateTime.Parse("08/01/2013");
        DateTime billingEnd = DateTime.Parse("12/31/2013");
        const string description = "Registration - 2013/FA";
        const string invoiceType = "BK";
        const string externalSystem = "ACME";
        const string externalId = "ACME #123";

        readonly List<string> adjustmentIds = new List<string> { "45678", "34567" };
        readonly List<string> adjustmentIdsWithNull = new List<string> { "45678", null, "34567" };
        readonly List<string> adjustmentIdsWithEmpty = new List<string> { "45678", "", "34567" };
        readonly List<string> adjustmentIdsWithDup = new List<string> { "45678", "34567", "45678" };

        readonly ReceivableCharge charge1 = new ReceivableCharge("24680", "123456789", new List<string> { "Full-Time Tuition", "BIOL-101-01" }, "TUIFT", 1875);
        readonly ReceivableCharge charge2 = new ReceivableCharge("13579", "123456789", new List<string> { "Activities Fee" }, "ACTFE", 175);
        readonly List<ReceivableCharge> charges = new List<ReceivableCharge> { new ReceivableCharge("24680", "123456789", new List<string> { "Full-Time Tuition" }, "TUIFT", 1875) };
        readonly List<ReceivableCharge> charges2 = new List<ReceivableCharge> { new ReceivableCharge("24680", "123456789", new List<string> { "Full-Time Tuition" }, "TUIFT", 1875), new ReceivableCharge("13579", "123456789", new List<string>() { "Activities Fee" }, "ACTFE", 175) };
        readonly List<ReceivableCharge> charges3 = new List<ReceivableCharge> { new ReceivableCharge("13579", "123456789", new List<string> { "Activities Fee" }, "ACTFE", 175) };
        readonly List<ReceivableCharge> charges4 = new List<ReceivableCharge> { new ReceivableCharge("13579", "123456789", new List<string> { "Activities Fee" }, "ACTFE", 175), new ReceivableCharge("13580", "123456789", new List<string>() { "Technology Fee" }, "TECFE", 225) };

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_ValidId()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                billingStart, dueDate, billingEnd, description, charges);

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceivableInvoice_IdAlreadySet()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.Id = "foo";
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_UpdateId()
        {
            var result = new ReceivableInvoice(null, referenceNumber, personId, receivableType, term, date,
                billingStart, dueDate, billingEnd, description, charges);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_ReferenceNumberValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceivableInvoice_ReferenceNumberAlreadySet()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.ReferenceNumber = "foo";
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_UpdateReferenceNumber()
        {
            var result = new ReceivableInvoice(id, null, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.ReferenceNumber = referenceNumber;
            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_NullPersonId()
        {
            var result = new ReceivableInvoice(id, referenceNumber, null, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_EmptyPersonId()
        {
            var result = new ReceivableInvoice(id, referenceNumber, string.Empty, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_PersonIdValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_NullAccountTypeCode()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, null, term, date,
                dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_EmptyAccountTypeCode()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, string.Empty, term, date,
                dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_AccountTypeCodeValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(receivableType, result.ReceivableType);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_NullTermValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, null, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.IsTrue(null == result.TermId);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_EmptyTermValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, String.Empty, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(String.Empty, result.TermId);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_TermValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(term, result.TermId);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_DateValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(date, result.Date);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReceivableInvoice_Constructor_DateInvalid()
        {
            var charges = new List<ReceivableCharge>() { charge1, charge2 };
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, DateTime.MinValue,
                billingStart, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_DueDateValid()
        {
            var charges = new List<ReceivableCharge>() { charge1, charge2 };
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(dueDate, result.DueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReceivableInvoice_Constructor_DueDateInvalid()
        {
            var charges = new List<ReceivableCharge>() { charge1, charge2 };
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                DateTime.MinValue, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_BillingStartValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            Assert.AreEqual(billingStart, result.BillingStart);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReceivableInvoice_Constructor_BillingStartInvalid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, DateTime.MinValue, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceivableInvoice_Constructor_BillingStartNull()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, null, billingEnd, description, charges);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_BillingEndValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(billingEnd, result.BillingEnd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReceivableInvoice_Constructor_BillingEndInvalid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, DateTime.MinValue, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceivableInvoice_Constructor_BillingEndNull()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, null, description, charges);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_BothBillingDatesNull()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, null, null, description, charges);
            Assert.IsNull(result.BillingStart);
            Assert.IsNull(result.BillingEnd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReceivableInvoice_Constructor_BadBillingDates()
        {
            billingStart = billingEnd.AddDays(1);
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_NullDescription()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, null, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_EmptyDescription()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, String.Empty, charges);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_DescriptionValid()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(description, result.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_NullCharges()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_NoCharges()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, new List<ReceivableCharge>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_Constructor_EmbeddedNullCharge()
        {
            var charges = new List<ReceivableCharge>() { charge1, null, charge2 };
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceivableInvoice_Constructor_NonMatchingInvoiceIds()
        {
            var charge3 = new ReceivableCharge("135246", "135792468", new List<string> { "Activities Fee" }, "ACTFE", 175);
            var charges = new List<ReceivableCharge> { charge1, charge2, charge3 };
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_DuplicateCharge()
        {
            var charges = new List<ReceivableCharge> { charge1, charge2, charge1 };
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            Assert.AreEqual(2, result.Charges.Count);
            Assert.AreEqual(charge1, result.Charges[0]);
            Assert.AreEqual(charge2, result.Charges[1]);
        }

        [TestMethod]
        public void ReceivableInvoice_Constructor_ChargesValid()
        {
            var charges = new List<ReceivableCharge>() { charge1, charge2 };
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.IsNotNull(result.Charges);
            CollectionAssert.AreEqual(charges, result.Charges.ToList());
        }

        [TestMethod]
        public void ReceivableInvoice_BaseAmount_Valid()
        {
            var charges = new List<ReceivableCharge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(baseAmount, result.BaseAmount);
        }

        [TestMethod]
        public void ReceivableInvoice_TaxAmount_Valid()
        {
            charge1.TaxAmount = 5;
            var charges = new List<ReceivableCharge>() { charge1, charge2 };
            var taxAmount = charge1.TaxAmount + charge2.TaxAmount;
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(taxAmount, result.TaxAmount);
        }

        [TestMethod]
        public void ReceivableInvoice_Amount_Valid()
        {
            charge1.TaxAmount = 5;
            var charges = new List<ReceivableCharge>() { charge1, charge2 };
            var baseAmount = charge1.BaseAmount + charge2.BaseAmount;
            var taxAmount = charge1.TaxAmount + charge2.TaxAmount;
            var amount = baseAmount + taxAmount;
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);

            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_AddAdjustingReceivableInvoice_NullValue()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.AddAdjustingInvoice(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_AddAdjustingReceivableInvoice_EmptyValue()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.AddAdjustingInvoice(String.Empty);
        }

        [TestMethod]
        public void ReceivableInvoice_AddAdjustingReceivableInvoice_ValidValue()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            foreach (var adj in adjustmentIds)
            {
                result.AddAdjustingInvoice(adj);
            }

            CollectionAssert.AreEqual(adjustmentIds, result.AdjustedByInvoices.ToList());
        }

        [TestMethod]
        public void ReceivableInvoice_AddAdjustingReceivableInvoice_ValidNoDups()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            foreach (var adj in adjustmentIdsWithDup)
            {
                result.AddAdjustingInvoice(adj);
            }

            CollectionAssert.AreEqual(adjustmentIds, result.AdjustedByInvoices.ToList());
        }

        [TestMethod]
        public void ReceivableInvoice_AddExternalInformation_ValidValues()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.AddExternalSystemAndId(externalSystem, externalId);

            Assert.AreEqual(externalSystem, result.ExternalSystem);
            Assert.AreEqual(externalId, result.ExternalIdentifier);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_AddExternalInformation_NullExternalSystem()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.AddExternalSystemAndId(null, externalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_AddExternalInformation_EmptyExternalSystem()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.AddExternalSystemAndId(string.Empty, externalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_AddExternalInformation_NullExternalIdentifier()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.AddExternalSystemAndId(externalSystem, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoice_AddExternalInformation_EmptyExternalIdentifier()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.AddExternalSystemAndId(externalSystem, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceivableInvoice_InvoiceTypeAlreadySet()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            result.InvoiceType = invoiceType;
            result.InvoiceType = "foo";
        }


        [TestMethod]
        public void ReceivableInvoice_TransactionType()
        {
            var result = new ReceivableInvoice(id, referenceNumber, personId, receivableType, term, date,
                dueDate, billingStart, billingEnd, description, charges);
            Assert.AreEqual(ReceivableTransactionType.Invoice, result.TransactionType);
        }
    }
}
