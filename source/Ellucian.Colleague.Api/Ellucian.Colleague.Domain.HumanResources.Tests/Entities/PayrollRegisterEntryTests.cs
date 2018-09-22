/* Copyright 2017 Ellucian Company L.P. and affiliates */
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollRegisterEntryTests
    {
        public string id;
        public string employeeId;
        public DateTime payPeriodStartDate;
        public DateTime payPeriodEndDate;
        public string payCycleId;
        public int sequenceNumber;
        public string paycheckReferenceId;
        public string payStatementReferenceId;
        public PayrollRegisterEntry pre
        {
            get
            {
                return new PayrollRegisterEntry(id, employeeId, payPeriodStartDate, payPeriodEndDate, payCycleId, sequenceNumber, paycheckReferenceId, payStatementReferenceId);
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            id = "tgc";
            employeeId = "0003914";
            payPeriodStartDate = new DateTime(2017, 1, 1);
            payPeriodEndDate = new DateTime(2017, 1, 31);
            payCycleId = "BW";
            sequenceNumber = 4;
            paycheckReferenceId = "91023";
            payStatementReferenceId = "32019";
        }
        [TestMethod]
        public void PropertiesAreSet()
        {
            //pre = new PayrollRegisterEntry(id, sequenceNumber, paycheckReferenceId, payStatementReferenceId);
            Assert.AreEqual(id, pre.Id);
            Assert.AreEqual(employeeId, pre.EmployeeId);
            Assert.AreEqual(payPeriodStartDate, pre.PayPeriodStartDate);
            Assert.AreEqual(payPeriodEndDate, pre.PayPeriodEndDate);
            Assert.AreEqual(payCycleId, pre.PayCycleId);
            Assert.AreEqual(sequenceNumber, pre.SequenceNumber);
            Assert.AreEqual(paycheckReferenceId, pre.PaycheckReferenceId);
            Assert.AreEqual(payStatementReferenceId, pre.PayStatementReferenceId);
            Assert.IsNotNull(pre.EarningsEntries);
            Assert.IsNotNull(pre.TaxEntries);
            Assert.IsNotNull(pre.LeaveEntries);
            Assert.IsNotNull(pre.BenefitDeductionEntries);
            Assert.IsNotNull(pre.TaxableBenefitEntries);
        }
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullIdIsHandled()
        {
            id = null;
            var error = pre;
        }
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void NullReferenceIdsAreHandled()
        {
            paycheckReferenceId = null;
            payStatementReferenceId = null;
            var error = pre; 
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayCycleRequiredTest()
        {
            payCycleId = null;
            var error = pre;
        }
        [TestMethod]
        public void ReferenceKeyIsSet()
        {
            //pre = new PayrollRegisterEntry(id, sequenceNumber, paycheckReferenceId, payStatementReferenceId);
            var key = string.Format("{0}*{1}", payStatementReferenceId, paycheckReferenceId);
            Assert.AreEqual(key, pre.ReferenceKey);

            paycheckReferenceId = null;
            //pre = new PayrollRegisterEntry(id, sequenceNumber, null, payStatementReferenceId);
            key = string.Format("{0}*{1}", payStatementReferenceId, string.Empty);
            Assert.AreEqual(key, pre.ReferenceKey);

            paycheckReferenceId = "foo";
            payStatementReferenceId = null;
            //pre = new PayrollRegisterEntry(id, sequenceNumber, paycheckReferenceId, null);
            key = string.Format("{0}*{1}", string.Empty, paycheckReferenceId);
            Assert.AreEqual(key, pre.ReferenceKey);
        }
        [TestMethod]
        public void EqualsOverride()
        {
            var pre1 = pre;
            var pre2 = pre;
            Assert.IsTrue(pre1.Equals(pre2));
            Assert.IsFalse(pre1.Equals(null));
        }
        [TestMethod]
        public void HashCodeOverride()
        {
            //pre = new PayrollRegisterEntry(id, sequenceNumber, paycheckReferenceId, payStatementReferenceId);
            Assert.AreEqual(id.GetHashCode(), pre.GetHashCode());
        }
    }
}
