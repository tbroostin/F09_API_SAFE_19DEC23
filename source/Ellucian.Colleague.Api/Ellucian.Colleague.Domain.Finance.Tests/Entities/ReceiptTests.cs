using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ReceiptTests
    {
        private string id, rcptNo, payerId, distribution;
        private readonly List<string> deposits = new List<string>();
        private readonly List<NonCashPayment> nonCashPayments = new List<NonCashPayment>();
        private NonCashPayment payment1;
        private NonCashPayment payment2;
        private string payMethod1, payMethod2;
        private decimal payAmount1, payAmount2;
        private string payerName, externalSystem, externalId;
        private string depositId1, depositId2;
        private DateTime today;

        [TestInitialize]
        public void Initialize()
        {
            payMethod1 = "VISA";
            payAmount1 = 200.00m;
            payment1 = new NonCashPayment(payMethod1, payAmount1);
            payMethod2 = "CHK";
            payAmount2 = 50.00m;
            payment2 = new NonCashPayment(payMethod2, payAmount2);
            nonCashPayments.Add(payment1);
            nonCashPayments.Add(payment2);

            depositId1 = "12345";
            depositId2 = "23456";
            deposits.Add(depositId1);
            deposits.Add(depositId2);

            id = "0001234567";
            rcptNo = "9876543210";
            today = DateTime.Now;
            payerId = "1234567";
            distribution = "BANK";

            payerName = "Fred Flintstone";
            externalSystem = "AHD";
            externalId = "ABC123";
        }

        [TestMethod]
        public void Receipt_Constructor_ValidId()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void Receipt_Constructor_ValidNullId()
        {
            var result = new Receipt(null, rcptNo, today, payerId, distribution, null, nonCashPayments);
            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        public void Receipt_Constructor_ValidEmptyId()
        {
            var result = new Receipt(string.Empty, rcptNo, today, payerId, distribution, null, nonCashPayments);
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        public void Receipt_Constructor_ValidReferenceNumber()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(rcptNo, result.ReferenceNumber);
        }

        [TestMethod]
        public void Receipt_Constructor_ValidNullReferenceNumber()
        {
            var result = new Receipt(id, null, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(null, result.ReferenceNumber);
        }

        [TestMethod]
        public void Receipt_Constructor_ValidEmptyReferenceNumber()
        {
            var result = new Receipt(id, string.Empty, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(string.Empty, result.ReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Receipt_Constructor_DateIsDefaultDateTime()
        {
            var result = new Receipt(id, rcptNo, default(DateTime), payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void Receipt_Constructor_ValidDate()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(today, result.Date);
        }

        [TestMethod]
        public void Receipt_Constructor_ValidPayerId()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(payerId, result.PayerId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_Constructor_NullPayerId()
        {
            var result = new Receipt(id, rcptNo, today, null, distribution, deposits, nonCashPayments);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_Constructor_EmptyPayerId()
        {
            var result = new Receipt(id, rcptNo, today, string.Empty, distribution, deposits, nonCashPayments);
        }

        [TestMethod]
        public void Receipt_Constructor_ValidDistribution()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(distribution, result.DistributionCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_Constructor_NullDistribution()
        {
            var result = new Receipt(id, rcptNo, today, payerId, null, deposits, nonCashPayments);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_Constructor_EmptyDistribution()
        {
            var result = new Receipt(id, rcptNo, today, payerId, string.Empty, deposits, nonCashPayments);
        }

        [TestMethod]
        public void Receipt_Constructor_ExistingDepositsExistingReceipt()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(deposits.Count, result.DepositIds.Count);
            CollectionAssert.AllItemsAreInstancesOfType(result.DepositIds, typeof(string));
            CollectionAssert.AreEqual(deposits, result.DepositIds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_Constructor_NullDepositsExistingReceipt()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, null, nonCashPayments);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_Constructor_EmptyDepositsExistingReceipt()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, new List<string>(), nonCashPayments);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Receipt_Constructor_ExistingDepositsNewReceipt()
        {
            var result = new Receipt(null, null, today, payerId, distribution, deposits, nonCashPayments);
        }

        [TestMethod]
        public void Receipt_Constructor_NullDepositsNewReceipt()
        {
            var result = new Receipt(null, null, today, payerId, distribution, null, nonCashPayments);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Receipt));
        }

        [TestMethod]
        public void Receipt_Constructor_EmptyDepositsNewReceipt()
        {
            var result = new Receipt(null, null, today, payerId, distribution, new List<string>(), nonCashPayments);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Receipt));
        }

        [TestMethod]
        public void Receipt_Constructor_ValidNonCashPayments()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(nonCashPayments.Count, result.NonCashPayments.Count);
            CollectionAssert.AllItemsAreInstancesOfType(result.NonCashPayments, typeof(NonCashPayment));
            CollectionAssert.AreEqual(nonCashPayments, result.NonCashPayments);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_Constructor_NullNonCashPayments()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_Constructor_EmptyNonCashPayments()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, new List<NonCashPayment>());
        }

        [TestMethod]
        public void Receipt_Id_ValidUpdate()
        {
            var result = new Receipt(null, rcptNo, today, payerId, distribution, null, nonCashPayments);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Receipt_Id_InvalidUpdate()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.Id = "1234567890";
        }

        [TestMethod]
        public void Receipt_ReferenceNumber_ValidUpdate()
        {
            var result = new Receipt(id, null, today, payerId, distribution, deposits, nonCashPayments);
            result.ReferenceNumber = rcptNo;
            Assert.AreEqual(rcptNo, result.ReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Receipt_ReferenceNumber_InvalidUpdate()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.ReferenceNumber = "1234567890";
        }

        [TestMethod]
        public void Receipt_PayerName_ValidUpdate()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.PayerName = payerName;
            Assert.AreEqual(payerName, result.PayerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Receipt_CashierId_InvalidUpdate()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments) { CashierId = payerId };
            result.CashierId = "1234567";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Receipt_PayerName_InvalidUpdate()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.PayerName = payerName;
            result.PayerName = "John Smith";
        }

        [TestMethod]
        public void Receipt_TotalNonCashPaymentAmount()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(result.NonCashPayments.Select(x => x.Amount).Sum(), result.TotalNonCashPaymentAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_AddNonCashPayment_NullNonCashPayment()
        {
            nonCashPayments.Add(null);
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
        }

        [TestMethod]
        public void Receipt_AddNonCashPayment_TakeDuplicateNonCashPayment()
        {
            nonCashPayments.Add(payment1);
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            Assert.AreEqual(3, result.NonCashPayments.Count);
            Assert.AreEqual(2, result.NonCashPayments.Count(ncp => ncp.PaymentMethodCode == "VISA"));
            Assert.AreEqual(450m, result.NonCashPayments.Sum(ncp => ncp.Amount));
            CollectionAssert.Contains(result.NonCashPayments, payment1);
            CollectionAssert.Contains(result.NonCashPayments, payment2);
            CollectionAssert.AreNotEqual(result.NonCashPayments.Distinct().ToList(), result.NonCashPayments);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Receipt_AddExternalSystemAndId_SystemAlreadyPopulated()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.AddExternalSystemAndId(externalSystem, externalId);
            result.AddExternalSystemAndId(externalSystem, externalId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Receipt_AddExternalSystemAndId_IdAlreadyPopulated()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.AddExternalSystemAndId(externalSystem, externalId);
            result.AddExternalSystemAndId(externalSystem, externalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_AddExternalSystemAndId_NullSystem()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.AddExternalSystemAndId(null, externalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_AddExternalSystemAndId_EmptySystem()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.AddExternalSystemAndId(string.Empty, externalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_AddExternalSystemAndId_NullId()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.AddExternalSystemAndId(externalSystem, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Receipt_AddExternalSystemAndId_EmptyId()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.AddExternalSystemAndId(externalSystem, string.Empty);
        }

        [TestMethod]
        public void Receipt_AddExternalSystemAndId_ValidSystem()
        {
            var result = new Receipt(id, rcptNo, today, payerId, distribution, deposits, nonCashPayments);
            result.AddExternalSystemAndId(externalSystem, externalId);
            Assert.AreEqual(externalSystem, result.ExternalSystem);
        }
    }
}
