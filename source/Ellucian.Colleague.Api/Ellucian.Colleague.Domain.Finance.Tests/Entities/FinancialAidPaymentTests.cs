// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class FinancialAidPaymentTests
    {
        static string id = "12345";
        static string personId = "0003315";
        static string receivableType = "01";
        static string termId = "2014/FA";
        static string referenceNumber = "23456";
        static DateTime date = DateTime.Today.AddDays(-3);
        static decimal amount = 1500;
        static int year = date.Year;
        static string awardId = "AWDA";
        static int priority = 99;

        FinancialAidPayment pmt = new FinancialAidPayment(id, referenceNumber, personId, receivableType, termId, date, amount, year, awardId, priority);
        
        [TestMethod]
        public void FinancialAidPayment_Constructor_ValidId()
        {
            Assert.AreEqual(id, pmt.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialAidPayment_Constructor_NullPersonId()
        {
            FinancialAidPayment pmt = new FinancialAidPayment(id, referenceNumber, null, receivableType, termId, date, amount, year, awardId, priority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialAidPayment_Constructor_EmptyPersonId()
        {
            FinancialAidPayment pmt = new FinancialAidPayment(id, referenceNumber, string.Empty, receivableType, termId, date, amount, year, awardId, priority);
        }

        [TestMethod]
        public void FinancialAidPayment_Constructor_ValidPersonId()
        {
            Assert.AreEqual(personId, pmt.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialAidPayment_Constructor_NullReceivableType()
        {
            FinancialAidPayment pmt = new FinancialAidPayment(id, referenceNumber, personId, null, termId, date, amount, year, awardId, priority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialAidPayment_Constructor_EmptyReceivableType()
        {
            FinancialAidPayment pmt = new FinancialAidPayment(id, referenceNumber, personId, string.Empty, termId, date, amount, year, awardId, priority);
        }

        [TestMethod]
        public void FinancialAidPayment_Constructor_ValidReceivableType()
        {
            Assert.AreEqual(receivableType, pmt.ReceivableType);
        }
        
        [TestMethod]
        public void FinancialAidPayment_Constructor_ValidReferenceNumber()
        {
            Assert.AreEqual(referenceNumber, pmt.ReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialAidPayment_Constructor_NullAwardId()
        {
            FinancialAidPayment pmt = new FinancialAidPayment(id, referenceNumber, personId, receivableType, termId, date, amount, year, null, priority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialAidPayment_Constructor_EmptyAwardId()
        {
            FinancialAidPayment pmt = new FinancialAidPayment(id, referenceNumber, personId, receivableType, termId, date, amount, year, string.Empty, priority);
        }

        [TestMethod]
        public void FinancialAidPayment_Constructor_ValidAwardId()
        {
            Assert.AreEqual(awardId, pmt.AwardId);
        }

        [TestMethod]
        public void FinancialAidPayment_Constructor_ValidYear()
        {
            Assert.AreEqual(year, pmt.Year);
        }

        [TestMethod]
        public void FinancialAidPayment_Constructor_ValidPriority()
        {
            Assert.AreEqual(priority, pmt.Priority);
        }

        [TestMethod]
        public void FinancialAidPayment_TransactionType()
        {
            Assert.AreEqual(ReceivableTransactionType.FinancialAid, pmt.TransactionType);
        }
    }
}
