// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentCashReceiptAcknowledgementRequestTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentCashReceiptAcknowledgementRequest_null_transactionId_cashReciptsId()
        {
            new InstantEnrollmentCashReceiptAcknowledgementRequest(null, null, "0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentCashReceiptAcknowledgementRequest_empty_transactionId_cashReciptsId()
        {
            new InstantEnrollmentCashReceiptAcknowledgementRequest(string.Empty, string.Empty, "0001234");
        }

        [TestMethod]
        public void InstantEnrollmentCashReceiptAcknowledgementRequest_valid_TransactionId()
        {
            var entity = new InstantEnrollmentCashReceiptAcknowledgementRequest("0004567", null, null);
            Assert.AreEqual("0004567", entity.TransactionId);
            Assert.AreEqual(null, entity.CashReceiptId);
            Assert.AreEqual(null, entity.PersonId);
        }

        [TestMethod]
        public void InstantEnrollmentCashReceiptAcknowledgementRequest_valid_cashReceiptsId()
        {
            var entity = new InstantEnrollmentCashReceiptAcknowledgementRequest(null, "0004567", null);
            Assert.AreEqual(null, entity.TransactionId);
            Assert.AreEqual("0004567", entity.CashReceiptId);
            Assert.AreEqual(null, entity.PersonId);
        }
    }
}
