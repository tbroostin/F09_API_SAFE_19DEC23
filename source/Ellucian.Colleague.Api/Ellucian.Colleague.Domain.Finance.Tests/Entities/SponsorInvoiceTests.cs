// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class SponsorInvoiceTests
    {
        const string id = "12345";
        const string personId = "0003315";
        const string receivableType = "01";
        const string termId = "2014/FA";
        const string referenceNumber = "23456";
        static readonly DateTime date = DateTime.Today.AddDays(-3);
        const decimal amount = 1500;
        const string invoiceId = "24680";
        const string sponsorId = "1";
        const string sponsorshipId = "101";

        readonly SponsorInvoice inv = new SponsorInvoice(id, referenceNumber, personId, receivableType, termId, date, amount, invoiceId, sponsorId, sponsorshipId);
        
        [TestMethod]
        public void SponsorInvoice_Constructor_ValidId()
        {
            Assert.AreEqual(id, inv.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SponsorInvoice_Constructor_NullPersonId()
        {
            var inv = new SponsorInvoice(id, referenceNumber, null, receivableType, termId, date, amount, invoiceId, sponsorId, sponsorshipId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SponsorInvoice_Constructor_EmptyPersonId()
        {
            var inv = new SponsorInvoice(id, referenceNumber, string.Empty, receivableType, termId, date, amount, invoiceId, sponsorId, sponsorshipId);
        }

        [TestMethod]
        public void SponsorInvoice_Constructor_ValidPersonId()
        {
            Assert.AreEqual(personId, inv.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SponsorInvoice_Constructor_NullReceivableType()
        {
            var inv = new SponsorInvoice(id, referenceNumber, personId, null, termId, date, amount, invoiceId, sponsorId, sponsorshipId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SponsorInvoice_Constructor_EmptyReceivableType()
        {
            var inv = new SponsorInvoice(id, referenceNumber, personId, string.Empty, termId, date, amount, invoiceId, sponsorId, sponsorshipId);
        }

        [TestMethod]
        public void SponsorInvoice_Constructor_ValidReceivableType()
        {
            Assert.AreEqual(receivableType, inv.ReceivableType);
        }

        [TestMethod]
        public void SponsorInvoice_Constructor_ValidReferenceNumber()
        {
            Assert.AreEqual(referenceNumber, inv.ReferenceNumber);
        }

        [TestMethod]
        public void SponsorInvoice_Constructor_ValidInvoiceId()
        {
            Assert.AreEqual(invoiceId, inv.InvoiceId);
        }

        [TestMethod]
        public void SponsorInvoice_Constructor_ValidSponsorId()
        {
            Assert.AreEqual(sponsorId, inv.SponsorId);
        }

        [TestMethod]
        public void SponsorInvoice_Constructor_ValidSponsorshipId()
        {
            Assert.AreEqual(sponsorshipId, inv.SponsorshipId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SponsorInvoice_AddTerm_Null()
        {
            inv.AddTerm(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SponsorInvoice_AddTerm_Empty()
        {
            inv.AddTerm(string.Empty);
        }

        [TestMethod]
        public void SponsorInvoice_AddTerm_Valid()
        {
            inv.AddTerm("2015/SP");
            Assert.AreEqual(1, inv.TermIds.Count);
            Assert.AreEqual("2015/SP", inv.TermIds[0]);
        }

        [TestMethod]
        public void SponsorInvoice_AddTerm_Duplicate()
        {
            inv.AddTerm("2015/SP");
            inv.AddTerm("2015/SP");
            Assert.AreEqual(1, inv.TermIds.Count);
            Assert.AreEqual("2015/SP", inv.TermIds[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SponsorInvoice_AddAllocations_Null()
        {
            inv.AddAllocation(null);
        }

        [TestMethod]
        public void SponsorInvoice_AddAllocations_Valid()
        {
            var alloc = new PaymentAllocation("111", "222", PaymentAllocationSource.System, 25m);
            inv.AddAllocation(alloc);
            Assert.AreEqual(1, inv.Allocations.Count);
            Assert.AreEqual(alloc, inv.Allocations[0]);
        }

        [TestMethod]
        public void SponsorInvoice_TransactionType()
        {
            Assert.AreEqual(ReceivableTransactionType.SponsoredBilling, inv.TransactionType);
        }
    }
}
