using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class RegistrationApprovalTests
    {
        string id = "1234567890";
        string studentId = "0012345";
        DateTimeOffset timestamp = DateTimeOffset.Parse("10/01/2013 12:34:56 -4");
        string paymentControlId = "1234567890";
        List<String> courseSectionIds = new List<String>() { "13579", "24680", "14703", "25814" };
        List<String> invoiceIds = new List<String>() { "12345", "67890", "36925", "47036" };
        string termsResponseId = "34567";
        ApprovalDocument termsDocument = new ApprovalDocument("23456", new List<String>() { "These are your terms & conditions." });
        ApprovalResponse termsResponse = new ApprovalResponse("34567", "23456", "0006966", "tcarmen", DateTime.Parse("12/01/2013"), true);
        ApprovalDocument acknowledgementDocument = new ApprovalDocument("45678", new List<String>() { "I agree to pay the following charges by the due date provided." });

        [TestMethod]
        public void RegistrationApproval_Constructor_NullId()
        {
            var result = new RegistrationTermsApproval(null, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        public void RegistrationApproval_Constructor_EmptyId()
        {
            var result = new RegistrationTermsApproval(string.Empty, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        public void RegistrationApproval_Constructor_ValidId()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_NullStudentId()
        {
            var result = new RegistrationTermsApproval(id, null, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_EmptyStudentId()
        {
            var result = new RegistrationTermsApproval(id, string.Empty, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
        }

        [TestMethod]
        public void RegistrationApproval_Constructor_ValidStudentId()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.AreEqual(studentId, result.StudentId);
        }

        [TestMethod]
        public void RegistrationApproval_Constructor_ValidTimestamp()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.AreEqual(timestamp, result.AcknowledgementTimestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_NullPaymentControlId()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, null, courseSectionIds, invoiceIds, termsResponseId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_EmptyPaymentControlId()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, string.Empty, courseSectionIds, invoiceIds, termsResponseId);
        }

        [TestMethod]
        public void RegistrationApproval_Constructor_ValidPaymentControlId()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.AreEqual(paymentControlId, result.PaymentControlId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_NullCourseSectionIds()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, null, invoiceIds, termsResponseId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_NoCourseSectionIds()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, new List<String>(), invoiceIds, termsResponseId);
        }

        [TestMethod]
        public void RegistrationApproval_Constructor_ValidCourseSectionIds()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.IsNotNull(result.SectionIds);
            CollectionAssert.AreEqual(courseSectionIds, result.SectionIds.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_NullInvoiceIds()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, null, termsResponseId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_NoInvoiceIds()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, new List<String>(), termsResponseId);
        }

        [TestMethod]
        public void RegistrationApproval_Constructor_ValidInvoiceIds()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.IsNotNull(result.InvoiceIds);
            CollectionAssert.AreEqual(invoiceIds, result.InvoiceIds.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationApproval_Constructor_NullTermsResponse()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, null);
        }

        [TestMethod]
        public void RegistrationApproval_Constructor_ValidTermsResponse()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            Assert.AreEqual(termsResponseId, result.TermsResponseId);
        }

        [TestMethod]
        public void RegistrationApproval_SetIdOutsideConstructorNull()
        {
            var result = new RegistrationTermsApproval(null, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            result.Id = null;
            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        public void RegistrationApproval_SetIdOutsideConstructorEmpty()
        {
            var result = new RegistrationTermsApproval(null, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            result.Id = string.Empty;
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        public void RegistrationApproval_SetIdOutsideConstructorValid()
        {
            var result = new RegistrationTermsApproval(null, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegistrationApproval_ChangeIdValueSetInConstructor()
        {
            var result = new RegistrationTermsApproval(id, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            result.Id = "2345678901";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegistrationApproval_ChangeIdValueSetOutsideConstructor()
        {
            var result = new RegistrationTermsApproval(null, studentId, timestamp, paymentControlId, courseSectionIds, invoiceIds, termsResponseId);
            result.Id = "2345678901";
            result.Id = "3456789012";
        }
    }
}
