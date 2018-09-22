using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class RegistrationPaymentControlTests
    {
        string id = "1234";
        string studentId = "1234567";
        string termId = "2013/FA";
        RegistrationPaymentStatus paymentStatus = RegistrationPaymentStatus.New;

        string sectionId = "143145";
        string invoiceId = "9877";
        string acadCredId = "23456";
        string paymentId = "36925";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_Constructor_NullId()
        {
            var result = new RegistrationPaymentControl(null, studentId, termId, paymentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_Constructor_EmptyId()
        {
            var result = new RegistrationPaymentControl(string.Empty, studentId, termId, paymentStatus);
        }

        [TestMethod]
        public void RegistrationPaymentControl_Constructor_ValidId()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_Constructor_NullStudentId()
        {
            var result = new RegistrationPaymentControl(id, null, termId, paymentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_Constructor_EmptyStudentId()
        {
            var result = new RegistrationPaymentControl(id, string.Empty, termId, paymentStatus);
        }

        [TestMethod]
        public void RegistrationPaymentControl_Constructor_ValidStudentId()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);

            Assert.AreEqual(studentId, result.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_Constructor_NullTermId()
        {
            var result = new RegistrationPaymentControl(id, studentId, null, paymentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_Constructor_EmptyTermId()
        {
            var result = new RegistrationPaymentControl(id, studentId, string.Empty, paymentStatus);
        }

        [TestMethod]
        public void RegistrationPaymentControl_Constructor_ValidTermId()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);

            Assert.AreEqual(termId, result.TermId);
        }

        [TestMethod]
        public void RegistrationPaymentControl_Constructor_ValidPaymentStatus()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            Assert.AreEqual(paymentStatus, result.PaymentStatus);
        }

        [TestMethod]
        public void RegistrationPaymentControl_Constructor_ChangePaymentStatus()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.PaymentStatus = RegistrationPaymentStatus.Error;
            Assert.AreEqual(RegistrationPaymentStatus.Error, result.PaymentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_AddRegisteredSection_NullSection()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddRegisteredSection(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_AddRegisteredSection_EmptySection()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddRegisteredSection(string.Empty);
        }

        [TestMethod]
        public void RegistrationPaymentControl_AddRegisteredSection_Valid()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddRegisteredSection(sectionId);

            Assert.IsNotNull(result.RegisteredSectionIds);
            Assert.AreEqual(sectionId, result.RegisteredSectionIds.ElementAt(0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_AddInvoice_NullInvoice()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddInvoice(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_AddInvoice_EmptyInvoice()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddInvoice(string.Empty);
        }

        [TestMethod]
        public void RegistrationPaymentControl_AddInvoice_ValidInvoice()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddInvoice(invoiceId);

            Assert.IsNotNull(result.InvoiceIds);
            Assert.AreEqual(invoiceId, result.InvoiceIds.ElementAt(0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_AddAcademicCredit_NullAcadCred()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddAcademicCredit(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_AddAcademicCredit_EmptyAcadCred()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddAcademicCredit(string.Empty);
        }

        [TestMethod]
        public void RegistrationPaymentControl_AddAcademicCredit_ValidAcadCred()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddAcademicCredit(acadCredId);

            Assert.IsNotNull(result.AcademicCredits);
            Assert.AreEqual(1, result.AcademicCredits.Count);
            Assert.AreEqual(acadCredId, result.AcademicCredits[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_AddPayment_NullPayment()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddPayment(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationPaymentControl_AddPayment_EmptyPayment()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddPayment(string.Empty);
        }

        [TestMethod]
        public void RegistrationPaymentControl_AddPayment_ValidPayment()
        {
            var result = new RegistrationPaymentControl(id, studentId, termId, paymentStatus);
            result.AddPayment(paymentId);

            Assert.IsNotNull(result.Payments);
            Assert.AreEqual(paymentId, result.Payments.ElementAt(0));
        }
    }
}
