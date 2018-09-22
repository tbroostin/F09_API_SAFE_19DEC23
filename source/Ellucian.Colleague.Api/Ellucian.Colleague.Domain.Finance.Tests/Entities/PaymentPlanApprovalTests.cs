// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
    public class PaymentPlanApprovalTests
    {
        static string id = "201";
        static string studentId = "0003315";
        static string studentName = "Charles Lindmuller";
        static DateTimeOffset acknowledgementDateTime = new DateTimeOffset(DateTime.Today.AddDays(-2));
        static string templateId = "DEFAULT";
        static string paymentControlId = "101";
        static string proposedPlanId = "123";
        static PaymentPlan proposedPlan = new PaymentPlan(null, "DEFAULT", studentId, "01", "2014/FA", 5000m, DateTime.Today.AddDays(3), null, null, null);
        static DateTimeOffset approvalReceived = new DateTimeOffset(DateTime.Now.AddMinutes(3).AddSeconds(15));
        static string approvalUserId = "jsmith";
        static decimal planAmount = 6815m;
        static List<PlanSchedule> planSchedules = new List<PlanSchedule>() { new PlanSchedule(new DateTime(2021, 12, 1), 2288.34m), new PlanSchedule(new DateTime(2022, 1, 19), 2263.33m), new PlanSchedule(new DateTime(2022, 2, 19), 2263.33m) };
        static ApprovalResponse termsResponse = new ApprovalResponse("123", "PAYPLANTC", studentId, approvalUserId, approvalReceived.DateTime, true);

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_NullId()
        {
            var result = new PaymentPlanApproval(null, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_EmptyId()
        {
            var result = new PaymentPlanApproval(string.Empty, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        public void PaymentPlanApproval_Constructor_ValidId()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_NullStudentId()
        {
            var result = new PaymentPlanApproval(id, null, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_EmptyStudentId()
        {
            var result = new PaymentPlanApproval(id, string.Empty, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        public void PaymentPlanApproval_Constructor_ValidStudentId()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            Assert.AreEqual(studentId, result.StudentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_NullStudentName()
        {
            var result = new PaymentPlanApproval(id, studentId, null, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_EmptyStudentName()
        {
            var result = new PaymentPlanApproval(id, studentId, string.Empty, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        public void PaymentPlanApproval_Constructor_ValidStudentName()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            Assert.AreEqual(studentName, result.StudentName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_NullTemplateId()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, null, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_EmptyTemplateId()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, string.Empty, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        public void PaymentPlanApproval_Constructor_ValidTemplateId()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            Assert.AreEqual(templateId, result.TemplateId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_NullApprovedPlan()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, null, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_EmptyApprovedPlan()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, string.Empty, termsResponse.Id, planAmount, planSchedules);
        }

        [TestMethod]
        public void PaymentPlanApproval_Constructor_ValidApprovedPlan()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            Assert.AreEqual(proposedPlanId, result.PaymentPlanId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_NullTermsResponse()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, null, planAmount, planSchedules);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_EmptyTermsResponse()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, string.Empty, planAmount, planSchedules);
        }

        [TestMethod]
        public void PaymentPlanApproval_Constructor_ValidTermsResponse()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            Assert.AreEqual(termsResponse.Id, result.TermsResponseId);
        }

        [TestMethod]
        public void PaymentPlanApproval_Constructor_ValidTimestamp()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            Assert.AreEqual(acknowledgementDateTime, result.Timestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_NullPlanSchedules()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanApproval_Constructor_EmptyPlanSchedules()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, new List<PlanSchedule>());
        }

        [TestMethod]
        public void PaymentPlanApproval_Constructor_ValidPlanSchedules()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            for (int i = 0; i < result.PlanSchedules.Count; i++)
            {
                Assert.AreEqual(planSchedules[i], result.PlanSchedules[i]);
            }
        }

        [TestMethod]
        public void PaymentPlanApproval_ValidAcknowledgementText()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            result.AcknowledgementDocumentId = "123";
            
            Assert.AreEqual("123", result.AcknowledgementDocumentId);
        }

        [TestMethod]
        public void PaymentPlanApproval_PaymentControlId_NotSet()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            Assert.IsTrue(string.IsNullOrEmpty(result.PaymentControlId));
        }

        [TestMethod]
        public void PaymentPlanApproval_PaymentControlId_Set()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            result.PaymentControlId = paymentControlId;
            
            Assert.AreEqual(paymentControlId, result.PaymentControlId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PaymentPlanApproval_PaymentControlId_Change()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);
            result.PaymentControlId = paymentControlId;
            result.PaymentControlId = "ABC";
        }

        [TestMethod]
        public void PaymentPlanApproval_ValidPlanAmount()
        {
            var result = new PaymentPlanApproval(id, studentId, studentName, acknowledgementDateTime, templateId, proposedPlanId, termsResponse.Id, planAmount, planSchedules);

            Assert.AreEqual(planAmount, result.PlanAmount);
        }
    }
}
