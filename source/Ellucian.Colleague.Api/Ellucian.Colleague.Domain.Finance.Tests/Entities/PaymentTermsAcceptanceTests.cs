using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentTermsAcceptanceTests
    {
        string studentId = "0012345";
        string payControlId = "12345";
        DateTime ackTimestamp = DateTime.Parse("10/01/2013 12:34:56");
        List<string> invoiceIds = new List<string>() { "12345", "67890", "36925", "47036" };
        List<string> sectionIds = new List<string>() { "13579", "24680", "14703", "25814" };
        List<string> termsText = new List<string>() { "These are your terms & conditions." };
        string approvalUserid = "tcarmen";
        DateTime approvalTimestamp = DateTime.Now;
        List<string> acknowledgementText = new List<string>() { "I agree to pay the following charges by the due date provided.", "I solemnly swear I am up to no good.", "Mischief managed." };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_NullStudentId()
        {
            var result = new PaymentTermsAcceptance(null, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_EmptyStudentId()
        {
            var result = new PaymentTermsAcceptance(String.Empty, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_Constructor_ValidStudentId()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            Assert.AreEqual(studentId, result.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_NullPayControlId()
        {
            var result = new PaymentTermsAcceptance(studentId, null, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_EmptyPayControlId()
        {
            var result = new PaymentTermsAcceptance(studentId, String.Empty, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_Constructor_ValidPayControlId()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            Assert.AreEqual(payControlId, result.PaymentControlId);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_Constructor_ValidAckTimestamp()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            Assert.AreEqual(ackTimestamp, result.AcknowledgementDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_NullInvoiceIds()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, null, sectionIds, termsText, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_EmptyInvoiceIds()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, new List<string>(), sectionIds, termsText, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_Constructor_ValidInvoiceIds()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            Assert.AreEqual(invoiceIds.Count, result.InvoiceIds.Count());
            CollectionAssert.AreEqual(invoiceIds, result.InvoiceIds.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_NullSectionIds()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, null, termsText, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_EmptySectionIds()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, new List<string>(), termsText, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_Constructor_ValidSectionIds()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            Assert.AreEqual(sectionIds.Count, result.SectionIds.Count());
            CollectionAssert.AreEqual(sectionIds, result.SectionIds.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_NullTermsText()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, null, approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_EmptyTermsText()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, new List<string>(), approvalUserid, approvalTimestamp);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_Constructor_ValidTermsText()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            Assert.AreEqual(termsText.Count, result.TermsText.Count());
            CollectionAssert.AreEqual(termsText, result.TermsText.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_NullApprovalUserId()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, null, approvalTimestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentTermsAcceptance_Constructor_EmptyApprovalUserId()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, String.Empty, approvalTimestamp);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_Constructor_ValidApprovalUserId()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            Assert.AreEqual(approvalUserid, result.ApprovalUserId);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_Constructor_ValidApprovalTimestamp()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            Assert.AreEqual(approvalTimestamp, result.ApprovalReceived);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_AcknowledgementText_ValidNull()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            result.AcknowledgementText = null;
            Assert.AreEqual(null, result.AcknowledgementText);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_AcknowledgementText_ValidEmpty()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            result.AcknowledgementText = new List<string>();
            Assert.AreEqual(0, result.AcknowledgementText.Count);
        }

        [TestMethod]
        public void PaymentTermsAcceptance_AcknowledgementText_ValidValue()
        {
            var result = new PaymentTermsAcceptance(studentId, payControlId, ackTimestamp, invoiceIds, sectionIds, termsText, approvalUserid, approvalTimestamp);
            result.AcknowledgementText = acknowledgementText;
            Assert.AreEqual(acknowledgementText.Count, result.AcknowledgementText.Count);
            CollectionAssert.AreEqual(acknowledgementText, result.AcknowledgementText);
        }
    }
}
