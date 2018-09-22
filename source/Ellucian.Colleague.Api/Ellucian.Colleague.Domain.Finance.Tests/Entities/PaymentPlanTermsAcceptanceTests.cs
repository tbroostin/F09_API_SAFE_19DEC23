using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentPlanTermsAcceptanceTests
    {
        static string studentId = "0003315";
        static string paymentControlId = "101";
        static DateTimeOffset acknowledgementDateTime = new DateTimeOffset(DateTime.Today.AddDays(-2));
        static string studentName = "Charles Lindmuller";
        static PaymentPlan proposedPlan = new PaymentPlan("", "DEFAULT", studentId, "01", "2014/FA", 5000m, DateTime.Today.AddDays(3), null, null, null);
        static decimal downPaymentAmount = 500m;
        static DateTime downPaymentDate = DateTime.Today.AddDays(2);
        static DateTimeOffset approvalReceived = new DateTimeOffset(DateTime.Now.AddMinutes(3).AddSeconds(15));
        static string approvalUserId = "0003315";
        static List<string> termsText = new List<string>() { "I hereby agree to the following terms and conditions regarding my payment plan with Ellucian University..." };
        static string registrationApprovalId = "2595";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_NullStudentId()
        {
            var result = new PaymentPlanTermsAcceptance(null, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_EmptyStudentId()
        {
            var result = new PaymentPlanTermsAcceptance(string.Empty, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_ValidStudentId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            Assert.AreEqual(studentId, result.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_NullProposedPlan()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, null, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_ValidProposedPlan()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            Assert.AreEqual(proposedPlan, result.ProposedPlan);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTermsAcceptance_Constructor_NegativeDownPaymentAmount()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, -500m, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_PositiveDownPaymentAmount()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            Assert.AreEqual(downPaymentAmount, result.DownPaymentAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTermsAcceptance_Constructor_NonNullDownPaymentDateWithNoDownPayment()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, 0m, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_NullDownPaymentDateWithNoDownPayment()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, 0m, null, approvalReceived, approvalUserId, termsText, registrationApprovalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTermsAcceptance_Constructor_NullDownPaymentDateWithDownPayment()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, 500m, null, approvalReceived, approvalUserId, termsText, registrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_FutureDownPaymentDate()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, 500m, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            Assert.AreEqual(downPaymentDate, result.DownPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_NullApprovalUserId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, null, termsText, registrationApprovalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_EmptyApprovalUserId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, string.Empty, termsText, registrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_ValidApprovalUserId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            Assert.AreEqual(approvalUserId, result.ApprovalUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_NullTermsText()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, null, registrationApprovalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_EmptyTermsText()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, new List<string>(), registrationApprovalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_NullRegistrationApprovalId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_EmptyRegistrationApprovalId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, string.Empty);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_ValidRegistrationApprovalId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            Assert.AreEqual(registrationApprovalId, result.RegistrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_ValidTermsText()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            CollectionAssert.AreEqual(termsText, result.TermsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_ValidAcknowledgementDateTime()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            Assert.AreEqual(acknowledgementDateTime, result.AcknowledgementDateTime);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_ValidApprovalReceived()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            Assert.AreEqual(approvalReceived, result.ApprovalReceived);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_PaymentControlId_Set()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            result.PaymentControlId = paymentControlId;

            Assert.AreEqual(paymentControlId, result.PaymentControlId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_PaymentControlId_NotSet()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);

            Assert.IsTrue(string.IsNullOrEmpty(result.PaymentControlId));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PaymentPlanTermsAcceptance_Constructor_ValidPaymentControlId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText, registrationApprovalId);
            result.PaymentControlId = paymentControlId;
            result.PaymentControlId = "ABC";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NullStudentId()
        {
            var result = new PaymentPlanTermsAcceptance(null, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_EmptyStudentId()
        {
            var result = new PaymentPlanTermsAcceptance(string.Empty, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_ValidStudentId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.AreEqual(studentId, result.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NullProposedPlan()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, null, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_ValidProposedPlan()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.AreEqual(proposedPlan, result.ProposedPlan);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NegativeDownPaymentAmount()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, -500m, downPaymentDate, approvalReceived, approvalUserId, termsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_PositiveDownPaymentAmount()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.AreEqual(downPaymentAmount, result.DownPaymentAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NonNullDownPaymentDateWithNoDownPayment()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, 0m, downPaymentDate, approvalReceived, approvalUserId, termsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NullDownPaymentDateWithNoDownPayment()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, 0m, null, approvalReceived, approvalUserId, termsText);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NullDownPaymentDateWithDownPayment()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, 500m, null, approvalReceived, approvalUserId, termsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_FutureDownPaymentDate()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, 500m, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.AreEqual(downPaymentDate, result.DownPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NullApprovalUserId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, null, termsText);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_EmptyApprovalUserId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, string.Empty, termsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_ValidApprovalUserId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.AreEqual(approvalUserId, result.ApprovalUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NullTermsText()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_EmptyTermsText()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, new List<string>());
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_NullRegistrationApprovalId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.IsNull(result.RegistrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_EmptyRegistrationApprovalId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.IsNull(result.RegistrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_ValidTermsText()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            CollectionAssert.AreEqual(termsText, result.TermsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_ValidAcknowledgementDateTime()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.AreEqual(acknowledgementDateTime, result.AcknowledgementDateTime);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_ValidApprovalReceived()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            Assert.AreEqual(approvalReceived, result.ApprovalReceived);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PaymentPlanTermsAcceptance_Constructor_Overload_ValidPaymentControlId()
        {
            var result = new PaymentPlanTermsAcceptance(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText);
            result.PaymentControlId = paymentControlId;
            result.PaymentControlId = "ABC";
        }
    }
}
