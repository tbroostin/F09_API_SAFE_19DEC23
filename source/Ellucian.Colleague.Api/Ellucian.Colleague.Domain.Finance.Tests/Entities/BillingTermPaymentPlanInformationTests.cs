// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class BillingTermPaymentPlanInformationTests
    {
        public string personId;
        public string termId;
        public string receivableTypeCode;
        public decimal paymentPlanAmount;
        public string paymentPlanTemplate;
        public List<string> invoiceIds = new List<string>();
        public BillingTermPaymentPlanInformation entity;

        [TestInitialize]
        public void Initialize()
        {
            personId = "0001234";
            termId = "TERM1";
            receivableTypeCode = "01";
            paymentPlanAmount = 500;
            paymentPlanTemplate = "TEMPLATE";
            invoiceIds.AddRange(new List<string>() { "1", "2", "3" });
            entity = new BillingTermPaymentPlanInformation(personId, termId, receivableTypeCode, paymentPlanAmount, paymentPlanTemplate);
        }

        [TestCleanup]
        public void Cleanup()
        {
            personId = null;
            termId = null;
            receivableTypeCode = null;
            paymentPlanAmount = 0;
            paymentPlanTemplate = null;
            invoiceIds.Clear();
            entity = null;
        }

        [TestClass]
        public class BillingTermPaymentPlanInformation_Constructor : BillingTermPaymentPlanInformationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingTermPaymentPlanInformation_Constructor_PersonId_Null()
            {
                entity = new BillingTermPaymentPlanInformation(null, termId, receivableTypeCode, paymentPlanAmount, paymentPlanTemplate);
            }

            [TestMethod]
            public void BillingTermPaymentPlanInformation_Constructor_PersonId()
            {
                Assert.AreEqual(personId, entity.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingTermPaymentPlanInformation_Constructor_TermId_Null()
            {
                entity = new BillingTermPaymentPlanInformation(personId, null, receivableTypeCode, paymentPlanAmount, paymentPlanTemplate);
            }

            [TestMethod]
            public void BillingTermPaymentPlanInformation_Constructor_TermId()
            {
                Assert.AreEqual(termId, entity.TermId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BillingTermPaymentPlanInformation_Constructor_ReceivableTypeCode_Null()
            {
                entity = new BillingTermPaymentPlanInformation(personId, termId, null, paymentPlanAmount, paymentPlanTemplate);
            }

            [TestMethod]
            public void BillingTermPaymentPlanInformation_Constructor_ReceivableTypeCode()
            {
                Assert.AreEqual(receivableTypeCode, entity.ReceivableTypeCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void BillingTermPaymentPlanInformation_Constructor_PaymentPlanAmount_Negative()
            {
                entity = new BillingTermPaymentPlanInformation(personId, termId, receivableTypeCode, -1000m, paymentPlanTemplate);
            }

            [TestMethod]
            public void BillingTermPaymentPlanInformation_Constructor_PaymentPlanAmount()
            {
                Assert.AreEqual(paymentPlanAmount, entity.PaymentPlanAmount);
            }

            [TestMethod]
            public void BillingTermPaymentPlanInformation_Constructor_PaymentPlanTemplateId()
            {
                Assert.AreEqual(paymentPlanTemplate, entity.PaymentPlanTemplateId);
            }

            [TestMethod]
            public void BillingTermPaymentPlanInformation_Constructor_IneligibilityReason()
            {
                Assert.IsNull(entity.IneligibilityReason);
            }
        }
    }
}
