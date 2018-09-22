// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PayrollDeductionTests
    {
        const string id = "12345";
        const string personId = "0003315";
        const string receivableType = "01";
        const string termId = "2014/FA";
        const string referenceNumber = "23456";
        static readonly DateTime date = DateTime.Today.AddDays(-3);
        const decimal amount = 1500;
        const string glReferenceNumber = "OVCHG";

        readonly PayrollDeduction deduction = new PayrollDeduction(id, referenceNumber, personId, receivableType, termId, date, amount, glReferenceNumber);
        
        [TestMethod]
        public void PayrollDeduction_Constructor_ValidId()
        {
            Assert.AreEqual(id, deduction.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayrollDeduction_Constructor_NullPersonId()
        {
            var pmt = new PayrollDeduction(id, referenceNumber, null, receivableType, termId, date, amount, glReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayrollDeduction_Constructor_EmptyPersonId()
        {
            var pmt = new PayrollDeduction(id, referenceNumber, string.Empty, receivableType, termId, date, amount, glReferenceNumber);
        }

        [TestMethod]
        public void PayrollDeduction_Constructor_ValidPersonId()
        {
            Assert.AreEqual(personId, deduction.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayrollDeduction_Constructor_NullReceivableType()
        {
            var pmt = new PayrollDeduction(id, referenceNumber, personId, null, termId, date, amount, glReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayrollDeduction_Constructor_EmptyReceivableType()
        {
            var pmt = new PayrollDeduction(id, referenceNumber, personId, string.Empty, termId, date, amount, glReferenceNumber);
        }

        [TestMethod]
        public void PayrollDeduction_Constructor_ValidReceivableType()
        {
            Assert.AreEqual(receivableType, deduction.ReceivableType);
        }

        [TestMethod]
        public void PayrollDeduction_Constructor_ValidReferenceNumber()
        {
            Assert.AreEqual(referenceNumber, deduction.ReferenceNumber);
        }

        [TestMethod]
        public void PayrollDeduction_Constructor_ValidGlReferenceNumberReason()
        {
            Assert.AreEqual(glReferenceNumber, deduction.GLReferenceNumber);
        }

        [TestMethod]
        public void PayrollDeduction_TransactionType()
        {
            Assert.AreEqual(ReceivableTransactionType.PayrollDeduction, deduction.TransactionType);
        }
    }
}
