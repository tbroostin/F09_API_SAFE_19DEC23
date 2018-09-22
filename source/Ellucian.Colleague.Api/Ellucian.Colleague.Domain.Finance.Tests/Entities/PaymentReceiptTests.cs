// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentReceiptTests
    {
        [TestMethod]
        public void PaymentReceipt_MerchantNameAddress()
        {
            var pmtRcpt = new PaymentReceipt();
            CollectionAssert.AreEqual(new List<string>(), pmtRcpt.MerchantNameAddress);
        }

        [TestMethod]
        public void PaymentReceipt_ReceiptAcknowledgeText()
        {
            var pmtRcpt = new PaymentReceipt();
            CollectionAssert.AreEqual(new List<string>(), pmtRcpt.ReceiptAcknowledgeText);
        }

        [TestMethod]
        public void PaymentReceipt_Payments()
        {
            var pmtRcpt = new PaymentReceipt();
            CollectionAssert.AreEqual(new List<AccountsReceivablePayment>(), pmtRcpt.Payments);
        }

        [TestMethod]
        public void PaymentReceipt_Deposits()
        {
            var pmtRcpt = new PaymentReceipt();
            CollectionAssert.AreEqual(new List<AccountsReceivableDeposit>(), pmtRcpt.Deposits);
        }

        [TestMethod]
        public void PaymentReceipt_OtherItems()
        {
            var pmtRcpt = new PaymentReceipt();
            CollectionAssert.AreEqual(new List<GeneralPayment>(), pmtRcpt.OtherItems);
        }

        [TestMethod]
        public void PaymentReceipt_ConvenienceFees()
        {
            var pmtRcpt = new PaymentReceipt();
            CollectionAssert.AreEqual(new List<ConvenienceFee>(), pmtRcpt.ConvenienceFees);
        }

        [TestMethod]
        public void PaymentReceipt_PaymentMethods()
        {
            var pmtRcpt = new PaymentReceipt();
            CollectionAssert.AreEqual(new List<PaymentMethod>(), pmtRcpt.PaymentMethods);
        }
    }
}
