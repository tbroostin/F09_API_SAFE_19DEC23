// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RequisiteWaiverTests
    {

        [TestClass]
        public class RequisiteWaiver_Constructor
        {
            string requisiteId;
            WaiverStatus status;
            RequisiteWaiver reqWaiver;

            [TestInitialize]
            public void Initialize()
            {
                requisiteId = "012";
                status = WaiverStatus.Waived;
            }

            [TestCleanup]
            public void CleanUp()
            {
                reqWaiver = null;
            }

            [TestMethod]
            public void RequisiteWaiver_RequisiteId()
            {
                reqWaiver = new RequisiteWaiver(requisiteId, status);
                Assert.AreEqual(requisiteId, reqWaiver.RequisiteId);
            }

            [TestMethod]
            public void RequisiteWaiver_Status()
            {
                reqWaiver = new RequisiteWaiver(requisiteId, status);
                Assert.AreEqual(status, reqWaiver.Status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfRequisiteIdIsNull()
            {
                reqWaiver = new RequisiteWaiver(null, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfRequisiteIdIsEmpty()
            {
                reqWaiver = new RequisiteWaiver(string.Empty, status);
            }

        }

        [TestClass]
        public class RequisiteWaiver_Equals
        {
            [TestMethod]
            public void RequisiteWaiver_EqualIfRequisiteIdsSame()
            {
                var reqWaiver1 = new RequisiteWaiver("01", WaiverStatus.Denied);
                var reqWaiver2 = new RequisiteWaiver("01", WaiverStatus.NotSelected);
                Assert.IsTrue(reqWaiver1.Equals(reqWaiver2));
            }

            [TestMethod]
            public void RequisiteWaiver_NotEqualIfRequisiteIdsDifferent()
            {
                var reqWaiver1 = new RequisiteWaiver("01", WaiverStatus.Denied);
                var reqWaiver2 = new RequisiteWaiver("02", WaiverStatus.Denied);
                Assert.IsFalse(reqWaiver1.Equals(reqWaiver2));
            }

            [TestMethod]
            public void RequisiteWaiver_NotEqualIfObjectNull()
            {
                var reqWaiver1 = new RequisiteWaiver("01", WaiverStatus.Denied);
                RequisiteWaiver reqWaiver2 = null;
                Assert.IsFalse(reqWaiver1.Equals(reqWaiver2));
            }
        }
    }
}
