using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class ReceiptEntity2ReceiptDtoTests
    {
        Domain.Finance.Entities.Receipt _entReceipt1;
        Dtos.Finance.Receipt _dtoReceipt1;
        Domain.Finance.Entities.Receipt _entReceipt2;
        Dtos.Finance.Receipt _dtoReceipt2;
        static Mock<ILogger> loggerMock = new Mock<ILogger>();
        static Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
        static ReceiptEntityAdapter adapter = new ReceiptEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

        // scalars
        static string _id1 = "RECEIPT1";
        static string _rcptNo1 = "000001234";
        static DateTime _today = DateTime.Now.Date;
        static string _payerId = "Payer1_Id";
        static string _distr = "Distribution";
        static string _payerName = "Payer Name";
        static string _cashier = "Cashier";
        static string _extSystem = "ACME";
        static string _extIdentifier = "External Id 1";
        // deposits
        static string _deposit1 = "Deposit1";
        static string _deposit2 = "Deposit2";
        static List<string> _oneDeposit = new List<string>() { _deposit1 };
        static List<string> _twoDeposits = new List<string>() { _deposit1, _deposit2 };
        // payment methods
        static string _payMethod1 = "PM1";
        static string _payMethod2 = "PM2";
        static decimal _amount1 = 10000;
        static decimal _amount2 = 20000;
        static List<Domain.Finance.Entities.NonCashPayment> _onePayment = new List<Domain.Finance.Entities.NonCashPayment>(){
            new Domain.Finance.Entities.NonCashPayment(_payMethod1, _amount1),
        };
        static List<Domain.Finance.Entities.NonCashPayment> _twoPayments = new List<Domain.Finance.Entities.NonCashPayment>(){
            new Domain.Finance.Entities.NonCashPayment(_payMethod1, _amount1),
            new Domain.Finance.Entities.NonCashPayment(_payMethod2, _amount2),
        };

        [TestInitialize]
        public void Initialize()
        {
            _entReceipt1 = new Domain.Finance.Entities.Receipt(_id1, _rcptNo1, _today, _payerId,
                _distr, _oneDeposit, _onePayment)
                {
                    PayerName = _payerName,
                    CashierId = _cashier,
                };
            _entReceipt1.AddExternalSystemAndId(_extSystem, _extIdentifier);

            _entReceipt2 = new Domain.Finance.Entities.Receipt(_id1, _rcptNo1, _today, _payerId,
                _distr, _twoDeposits, _twoPayments)
            {
                PayerName = _payerName,
                CashierId = _cashier,
            };

        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Id()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.AreEqual(_entReceipt1.Id, _dtoReceipt1.Id);
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Date()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.AreEqual(_entReceipt1.Date, _dtoReceipt1.Date);
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Payer()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.AreEqual(_entReceipt1.PayerId, _dtoReceipt1.PayerId);
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Distribution()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.AreEqual(_entReceipt1.DistributionCode, _dtoReceipt1.DistributionCode);
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_PayerName()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.AreEqual(_entReceipt1.PayerName, _dtoReceipt1.PayerName);
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_ExternalSystem()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.AreEqual(_entReceipt1.ExternalSystem, _dtoReceipt1.ExternalSystem);
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_ExternalIdentifier()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.AreEqual(_entReceipt1.ExternalIdentifier, _dtoReceipt1.ExternalIdentifier);
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_TotalAmount()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.AreEqual(_entReceipt1.TotalNonCashPaymentAmount, _dtoReceipt1.TotalNonCashPaymentAmount);
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Deposit()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            CollectionAssert.AreEqual(_entReceipt1.DepositIds.ToList(), _dtoReceipt1.DepositIds.ToList());
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Payment()
        {
            _dtoReceipt1 = adapter.MapToType(_entReceipt1);
            Assert.IsTrue(ArePaymentsEqual(_entReceipt1.NonCashPayments, _dtoReceipt1.NonCashPayments));
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_MultipleNonPaymentMethodMultipleDeposit_Deposit()
        {
            _dtoReceipt2 = adapter.MapToType(_entReceipt2);
            CollectionAssert.AreEqual(_entReceipt2.DepositIds.ToList(), _dtoReceipt2.DepositIds.ToList());
        }

        [TestMethod]
        public void ReceiptEntity2ReceiptDto_MultipleNonPaymentMethodMultipleDeposit_Payment()
        {
            _dtoReceipt2 = adapter.MapToType(_entReceipt2);
            Assert.IsTrue(ArePaymentsEqual(_entReceipt2.NonCashPayments, _dtoReceipt2.NonCashPayments));
        }

        private bool ArePaymentsEqual(IEnumerable<Domain.Finance.Entities.NonCashPayment> genEntPayments,
            IEnumerable<Dtos.Finance.NonCashPayment> genDtoPayments)
        {
            if (genEntPayments == null || genDtoPayments == null) return false;

            List<Domain.Finance.Entities.NonCashPayment> entPayments = genEntPayments.ToList();
            List<Dtos.Finance.NonCashPayment> dtoPayments = genDtoPayments.ToList();

            // check counts
            int numEnts = entPayments.Count;
            int numDtos = dtoPayments.Count;
            if (numEnts != numDtos) return false;
            // check values
            for (int i=0; i< numEnts; i++)
            {
                if (!entPayments[i].PaymentMethodCode.Equals(dtoPayments[i].PaymentMethodCode) ||
                    entPayments[i].Amount != dtoPayments[i].Amount)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
