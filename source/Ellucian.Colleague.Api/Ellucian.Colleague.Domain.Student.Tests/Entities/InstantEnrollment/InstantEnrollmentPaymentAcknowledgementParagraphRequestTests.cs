// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentPaymentAcknowledgementParagraphRequestTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentPaymentAcknowledgementParagraphRequest_null_person_Id()
        {
            var entity = new InstantEnrollmentPaymentAcknowledgementParagraphRequest(null, "0004567");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentPaymentAcknowledgementParagraphRequest_empty_person_Id()
        {
            var entity = new InstantEnrollmentPaymentAcknowledgementParagraphRequest(string.Empty, "0004567");
        }

        [TestMethod]
        public void InstantEnrollmentPaymentAcknowledgementParagraphRequest_valid()
        {
            var entity = new InstantEnrollmentPaymentAcknowledgementParagraphRequest("0001234", "0004567");
            Assert.AreEqual("0001234", entity.PersonId);
            Assert.AreEqual("0004567", entity.CashReceiptId);
        }
    }
}
