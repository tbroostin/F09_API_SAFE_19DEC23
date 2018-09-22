// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Services
{
    [TestClass]
    public class ReceiptProcessorTests
    {
        // payer data
        static string goodPayer = "goodPayer";
        static string goodPayerName = "Good PayerName";

        // holder data
        static string goodHolder = "goodHolder";

        // deposit type data
        static string depositType1 = "DTYPE1";
        static string depositType2 = "DTYPE2";
        static List<DepositType> depositTypes = new List<DepositType>(){
            new DepositType(depositType1, "Deposit Type 1"),
            new DepositType(depositType2, "Deposit Type 2")
        };

        // payment data
        static List<ReceiptPayment> zeroPayments = new List<ReceiptPayment>();

        // deposit data
        static Deposit goodDeposit1 = new Deposit(null, goodHolder, DateTime.Now, depositType1, 10000);
        static Deposit goodDeposit2 = new Deposit(null, goodHolder, DateTime.Now, depositType2, 10000);
        static List<Deposit> zeroDeposits = new List<Deposit>();
        static List<Deposit> oneDeposit = new List<Deposit>() {
            goodDeposit1
        };
        static List<Deposit> twoDeposits = new List<Deposit>() {
            goodDeposit1,
            goodDeposit2
        };

        // cashier data
        static Cashier nonECommCashier = new Cashier("nonecomm", "GOOD", false);
        static Cashier eCommCashier = new Cashier("ecomm", "BAD", true);

        // distribution data
        static string dist1 = "DIST1";
        static string dist2 = "DIST2";
        static Distribution goodDist1 = new Distribution(dist1, "Distribution 1");
        static Distribution goodDist2 = new Distribution(dist2, "Distribution 2");
        static List<Distribution> distributions = new List<Distribution>
        {
            goodDist1,
            goodDist2
        };

        // external system data
        static string sys1 = "SYS1";
        static string sys2 = "SYS2";
        static string ExternalId = "External Identifier 1";
        static List<ExternalSystem> systems = new List<ExternalSystem>(){
            new ExternalSystem(sys1, "External System 1"),
            new ExternalSystem(sys2, "External System 2")
        };

        // payment method data
        static string payMeth1 = "payMeth1";
        static string payMeth2 = "payMeth2";
        static string invalidPayMeth = "badPayMeth";
        static List<PaymentMethod> payMethods = new List<PaymentMethod>(){
            new PaymentMethod(payMeth1, "Payment Method 1", PaymentMethodCategory.CreditCard, false, false),
            new PaymentMethod(payMeth2, "Payment Method 2", PaymentMethodCategory.Other, false, false),
            new PaymentMethod(invalidPayMeth, "Invalid Payment Method", PaymentMethodCategory.Cash, false, false)
        };

        // non-cash payment data
        static List<NonCashPayment> zeroNonCashPayments = new List<NonCashPayment>();
        static List<NonCashPayment> oneNonCashPayments = new List<NonCashPayment>(){
            new NonCashPayment(payMeth1, 10000)
        };
        static List<NonCashPayment> twoNonCashPayments = new List<NonCashPayment>(){
            new NonCashPayment(payMeth1, 10000),
            new NonCashPayment(payMeth2, 10000)
        };

        static List<NonCashPayment> badNonCashPayment = new List<NonCashPayment>(){
            new NonCashPayment(invalidPayMeth, 10000)
        };

        static List<NonCashPayment> oneGoodOneBadNonCashPayment = new List<NonCashPayment>(){
            new NonCashPayment(payMeth1, 5000),
            new NonCashPayment(invalidPayMeth, 5000)
        };

        // receipt data
        Receipt goodReceipt;
        Receipt goodRLReceipt;

        [TestInitialize]
        public void Initialize()
        {
            goodReceipt = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, oneNonCashPayments);
            goodReceipt.CashierId = nonECommCashier.Id;

            goodRLReceipt = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, oneNonCashPayments)
                {
                    CashierId = nonECommCashier.Id,
                    PayerName = "Joe Payer"
                };
            goodRLReceipt.AddExternalSystemAndId(sys1, "External System Id");
        }

        #region validate receipts
        // Valid receipt passes validation
        [TestMethod]
        public void ValidateReceipt_ValidReceipt()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, oneDeposit, distributions,
                systems, payMethods);
        }

        // Valid external system passes validation
        [TestMethod]
        public void ValidateReceipt_ValidExternalSystem()
        {
            goodReceipt.AddExternalSystemAndId(sys1, ExternalId);
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, oneDeposit, distributions,
                systems, payMethods);
        }

        // Receipt must be provided
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateReceipt_NullReceipt()
        {
            ReceiptProcessor.ValidateReceipt(null, null, oneDeposit, distributions,
                systems, payMethods);
        }

        // payments and deposits cannot both be null
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateReceipt_NullPaymentAndNullDeposit()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, null, distributions,
                systems, payMethods);
        }

        // payments and deposits cannot both be empty
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateReceipt_EmptyPaymentAndEmptyDeposit()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, zeroPayments, zeroDeposits, distributions,
                systems, payMethods);
        }

        // Distributions cannot be null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateReceipt_NullDistributions()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, oneDeposit, null,
                systems, payMethods);
        }

        // Distributions cannot be empty
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateReceipt_EmptyDistributions()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, oneDeposit, new List<Distribution>(),
                systems, payMethods);
        }

        // external Systems cannot be null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateReceipt_NullExternalSystems()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, oneDeposit, distributions,
                null, payMethods);
        }

        // external Systems cannot be empty
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateReceipt_EmptyExternalSystems()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, oneDeposit, distributions,
                new List<ExternalSystem>(), payMethods);
        }

        // pay methods cannot be null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateReceipt_NullPaymentMethods()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, oneDeposit, distributions,
                systems, null);
        }

        // pay methods cannot be empty
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateReceipt_EmptyPaymentMethods()
        {
            ReceiptProcessor.ValidateReceipt(goodReceipt, null, oneDeposit, distributions,
                systems, new List<PaymentMethod>());
        }

        // Invalid receipt distribution
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateReceipt_InvalidDistribution()
        {
            Receipt invalidDistribution = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, "Bad Distribution",
                null, oneNonCashPayments);
            invalidDistribution.CashierId = nonECommCashier.Id;
            ReceiptProcessor.ValidateReceipt(invalidDistribution, null, oneDeposit, distributions,
                systems, payMethods);
        }

        // Invalid external system
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateReceipt_InvalidExternalSystem()
        {
            Receipt invalidSystem = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, oneNonCashPayments);
            invalidSystem.CashierId = nonECommCashier.Id;
            invalidSystem.AddExternalSystemAndId("Bad System", ExternalId);
            ReceiptProcessor.ValidateReceipt(invalidSystem, null, oneDeposit, distributions,
                systems, payMethods);
        }

        // require non-cash payments
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateReceipt_RequireNonCashPayment()
        {
            Receipt noNonCashPayment = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, zeroNonCashPayments);
            noNonCashPayment.CashierId = nonECommCashier.Id;
            ReceiptProcessor.ValidateReceipt(noNonCashPayment, null, oneDeposit, distributions,
                systems, payMethods);
        }

        // allow multiple non-cash payments
        [TestMethod]
        public void ValidateReceipt_AllowMultipleNonCashPayments()
        {
            Receipt multipleNonCashPayments = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, twoNonCashPayments);
            multipleNonCashPayments.CashierId = nonECommCashier.Id;
            ReceiptProcessor.ValidateReceipt(multipleNonCashPayments, null, twoDeposits, distributions,
                systems, payMethods);
        }

        // invalid non-cash payment method
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateReceipt_InvalidNonCashPayment()
        {
            Receipt invalidNonCashPayment = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, badNonCashPayment);
            invalidNonCashPayment.CashierId = nonECommCashier.Id;
            ReceiptProcessor.ValidateReceipt(invalidNonCashPayment, null, oneDeposit, distributions,
                systems, payMethods);
        }

        // one valid, one invalid non-cash payment
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateReceipt_OneValidAndOneInvalidNonCashPayment()
        {
            Receipt invalidNonCashPayment = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, oneGoodOneBadNonCashPayment);
            invalidNonCashPayment.CashierId = nonECommCashier.Id;
            ReceiptProcessor.ValidateReceipt(invalidNonCashPayment, null, oneDeposit, distributions,
                systems, payMethods);
        }


        // received amounts not equal to applied amounts
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateReceipt_ReceivedAmountsNotEqualToAppliedAmounts()
        {
            Receipt mismatchedAmounts = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, twoNonCashPayments);
            mismatchedAmounts.CashierId = nonECommCashier.Id;
            ReceiptProcessor.ValidateReceipt(mismatchedAmounts, null, oneDeposit, distributions,
                systems, payMethods);
        }
        #endregion

        #region Residence Life Validations

        // Valid receipt passes validation
        [TestMethod]
        public void ValidateResidenceLifeReceipt_ValidReceipt()
        {
            ReceiptProcessor.ValidateResidenceLifeReceipt(goodRLReceipt, oneDeposit, nonECommCashier);
        }

        // null receipt
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateResidentialLifeReceipt_NullReceipt()
        {
            ReceiptProcessor.ValidateResidenceLifeReceipt(null, oneDeposit, nonECommCashier);
        }

        // null deposit
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateResidenceLifeReceipt_NullDeposit()
        {
            ReceiptProcessor.ValidateResidenceLifeReceipt(goodRLReceipt, null, nonECommCashier);
        }

        // null cashier
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateResidenceLifeReceipt_NullCashier()
        {
            ReceiptProcessor.ValidateResidenceLifeReceipt(goodRLReceipt, oneDeposit, null);
        }

        // zero deposits is invalid
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateResidenceLifeReceipt_ZeroDeposits()
        {
            ReceiptProcessor.ValidateResidenceLifeReceipt(goodRLReceipt, zeroDeposits, nonECommCashier);
        }

        // multiple deposits is invalid
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateResidenceLifeReceipt_MultipleDeposits()
        {
            ReceiptProcessor.ValidateResidenceLifeReceipt(goodRLReceipt, twoDeposits, nonECommCashier);
        }

        // null payer name
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateResidenceLifeReceipt_NullPayerName()
        {
            Receipt nullPayerName = new Receipt(null, null, DateTime.Now.Date, goodPayer, dist1,
                null, oneNonCashPayments);
            nullPayerName.CashierId = nonECommCashier.Id;
            nullPayerName.AddExternalSystemAndId(sys1, ExternalId);
            ReceiptProcessor.ValidateResidenceLifeReceipt(nullPayerName, oneDeposit, nonECommCashier);
        }

        // empty payer name
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateResidenceLifeReceipt_EmptyPayerName()
        {
            Receipt nullPayerName = new Receipt(null, null, DateTime.Now.Date, goodPayer, dist1,
                null, oneNonCashPayments);
            nullPayerName.CashierId = nonECommCashier.Id;
            nullPayerName.PayerName = string.Empty;
            nullPayerName.AddExternalSystemAndId(sys1, ExternalId);
            ReceiptProcessor.ValidateResidenceLifeReceipt(nullPayerName, oneDeposit, nonECommCashier);
        }

        // Invalid eComm cashier
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateResidenceLifeReceipt_InvalidECommCashier()
        {
            Receipt badCashier = new Receipt(null, null, DateTime.Now.AddDays(-1), goodPayer, dist1,
                null, oneNonCashPayments);
            badCashier.CashierId = eCommCashier.Id;
            badCashier.PayerName = goodPayerName;
            badCashier.AddExternalSystemAndId(sys1, ExternalId);
            ReceiptProcessor.ValidateResidenceLifeReceipt(badCashier, oneDeposit, eCommCashier);
        }

        // Receipt date in the future
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateResidenceLifeReceipt_NoFutureDate(){
            Receipt futureDate = new Receipt(null, null, DateTime.Now.Date.AddDays(1), goodPayer, dist1,
                null, oneNonCashPayments);
            futureDate.CashierId = nonECommCashier.Id;
            futureDate.PayerName = goodPayerName;
            futureDate.AddExternalSystemAndId(sys1, ExternalId);
            ReceiptProcessor.ValidateResidenceLifeReceipt(futureDate, oneDeposit, nonECommCashier);
        }

        // multiple nonCash payments
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateResidenceLifeReceipt_MultipleNonCashPayments()
        {
            Receipt multNcPmts = new Receipt(null, null, DateTime.Now.Date, goodPayer, dist1,
                null, twoNonCashPayments);
            multNcPmts.CashierId = nonECommCashier.Id;
            multNcPmts.PayerName = goodPayerName;
            multNcPmts.AddExternalSystemAndId(sys1, ExternalId);
            ReceiptProcessor.ValidateResidenceLifeReceipt(multNcPmts, oneDeposit, nonECommCashier);
        }

        // external system and identifier are specified
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateResidenceLifeReceipt_NullExternalSystemAndIdentifier()
        {
            Receipt nullExternalInfo = new Receipt(null, null, DateTime.Now.Date, goodPayer, dist1,
                null, oneNonCashPayments);
            nullExternalInfo.CashierId = nonECommCashier.Id;
            nullExternalInfo.PayerName = goodPayerName;
            ReceiptProcessor.ValidateResidenceLifeReceipt(nullExternalInfo, oneDeposit, nonECommCashier);
        }
        #endregion
    }
}
