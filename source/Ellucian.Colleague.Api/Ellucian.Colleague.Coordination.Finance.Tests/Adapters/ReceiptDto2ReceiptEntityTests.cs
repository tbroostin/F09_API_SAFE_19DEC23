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
    public class ReceiptDto2ReceiptEntityTests
    {
        Domain.Finance.Entities.Receipt _entReceipt1;
        Dtos.Finance.Receipt _dtoReceipt1;
        Domain.Finance.Entities.Receipt _entReceipt2;
        Dtos.Finance.Receipt _dtoReceipt2;
        static Mock<ILogger> loggerMock = new Mock<ILogger>();
        static Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();
        static ReceiptDtoAdapter adapter = new ReceiptDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

        // scalars
        static string _id1 = "RECEIPT1";
        static DateTime _today = DateTime.Now.Date;
        static string _payerId = "Payer1_Id";
        static string _distr = "Distribution";
        static string _payerName = "Payer Name";
        static string _cashier = "Cashier";
        static string _extSystem = "ACME";
        static string _extIdentifier = "External Id 1";
        static decimal _badNonCashPaymentTotalAmount;
        static decimal _goodNonCashPaymentTotalAmount;
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
        static List<Dtos.Finance.NonCashPayment> _onePayment = new List<Dtos.Finance.NonCashPayment>(){
            new Dtos.Finance.NonCashPayment(){
                PaymentMethodCode = _payMethod1,
                Amount =_amount1},
        };
        static List<Dtos.Finance.NonCashPayment> _twoPayments = new List<Dtos.Finance.NonCashPayment>(){
            new Dtos.Finance.NonCashPayment(){
                PaymentMethodCode = _payMethod1,
                Amount =_amount1},
            new Dtos.Finance.NonCashPayment(){
                PaymentMethodCode = _payMethod2,
                Amount =_amount2},
        };

        [TestInitialize]
        public void Initialize()
        {
            _dtoReceipt1 = new Dtos.Finance.Receipt()
            {
                Id = _id1,
                Date = _today,
                PayerId = _payerId,
                DistributionCode = _distr,
                DepositIds = _oneDeposit,
                NonCashPayments = _onePayment,
                PayerName = _payerName,
                CashierId = _cashier,
                ExternalSystem = _extSystem,
                ExternalIdentifier = _extIdentifier,
            };
            _badNonCashPaymentTotalAmount = (_onePayment.Sum(x => x.Amount)) + 1;
            _dtoReceipt1.TotalNonCashPaymentAmount = _badNonCashPaymentTotalAmount;

            _dtoReceipt2 = new Dtos.Finance.Receipt()
            {
                Id = _id1,
                Date = _today,
                PayerId = _payerId,
                DistributionCode = _distr,
                DepositIds = _twoDeposits,
                NonCashPayments = _twoPayments,
                PayerName = _payerName,
                CashierId = _cashier,
            };
            _goodNonCashPaymentTotalAmount = _twoPayments.Sum(x => x.Amount);
            _dtoReceipt2.TotalNonCashPaymentAmount = _goodNonCashPaymentTotalAmount;

        }

        // Id transfers OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Id()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.AreEqual(_dtoReceipt1.Id, _entReceipt1.Id);
        }

        // Date transfers OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Date()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.AreEqual(_dtoReceipt1.Date, _entReceipt1.Date);
        }

        // Payer Id transfers OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Payer()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.AreEqual(_dtoReceipt1.PayerId, _entReceipt1.PayerId);
        }

        // Distribution transfers OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Distribution()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.AreEqual(_dtoReceipt1.DistributionCode, _entReceipt1.DistributionCode);
        }

        // Payer Name transfers OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_PayerName()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.AreEqual(_dtoReceipt1.PayerName, _entReceipt1.PayerName);
        }

        // External System transfers OK when it has a value
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_ExternalSystem()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.AreEqual(_dtoReceipt1.ExternalSystem, _entReceipt1.ExternalSystem);
        }

        // External Id transfers OK when it has a value
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_ExternalIdentifier()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.AreEqual(_dtoReceipt1.ExternalIdentifier, _entReceipt1.ExternalIdentifier);
        }

        // Total amount in entity is correct even if DTO value is bad
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_BadTotalAmount()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.AreNotEqual(_dtoReceipt1.TotalNonCashPaymentAmount, _entReceipt1.TotalNonCashPaymentAmount);
        }

        // A single deposit transfers OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Deposit()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            CollectionAssert.AreEqual(_dtoReceipt1.DepositIds.ToList(), _entReceipt1.DepositIds.ToList());
        }

        // A single non-cash payment transfers OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_SingleNonPaymentMethodSingleDeposit_Payment()
        {
            _entReceipt1 = adapter.MapToType(_dtoReceipt1);
            Assert.IsTrue(ArePaymentsEqual(_dtoReceipt1.NonCashPayments, _entReceipt1.NonCashPayments));
        }

        // Total amount in entity is good when DTO value is good
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_MultipleNonPaymentMethodMultipleDeposit_GoodTotalAmount()
        {
            _entReceipt2 = adapter.MapToType(_dtoReceipt2);
            Assert.AreEqual(_dtoReceipt2.TotalNonCashPaymentAmount, _entReceipt2.TotalNonCashPaymentAmount);
        }

        // Multiple deposits transfer OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_MultipleNonPaymentMethodMultipleDeposit_Deposit()
        {
            _entReceipt2 = adapter.MapToType(_dtoReceipt2);
            CollectionAssert.AreEqual(_dtoReceipt2.DepositIds.ToList(), _entReceipt2.DepositIds.ToList());
        }

        // Multiple non-cash payments transfer OK
        [TestMethod]
        public void ReceiptEntity2ReceiptDto_MultipleNonPaymentMethodMultipleDeposit_Payment()
        {
            _entReceipt2 = adapter.MapToType(_dtoReceipt2);
            Assert.IsTrue(ArePaymentsEqual(_dtoReceipt2.NonCashPayments, _entReceipt2.NonCashPayments));
        }

        // Failure when extern system but not external identifier
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptEntity2ReceiptDto_ExternalSystemAndNullExternalIdentifier()
        {
            Dtos.Finance.Receipt _dtoReceipt = new Dtos.Finance.Receipt()
            {
                Id = _id1,
                Date = _today,
                PayerId = _payerId,
                DistributionCode = _distr,
                DepositIds = _oneDeposit,
                NonCashPayments = _onePayment,
                PayerName = _payerName,
                CashierId = _cashier,
                ExternalSystem = _extSystem,
            };
            Domain.Finance.Entities.Receipt _entReceipt = adapter.MapToType(_dtoReceipt);
        }

        // Failure when extern identifier but not external system
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptEntity2ReceiptDto_ExternalIentifierAndNullExternalSystem()
        {
            Dtos.Finance.Receipt _dtoReceipt = new Dtos.Finance.Receipt()
            {
                Id = _id1,
                Date = _today,
                PayerId = _payerId,
                DistributionCode = _distr,
                DepositIds = _oneDeposit,
                NonCashPayments = _onePayment,
                PayerName = _payerName,
                CashierId = _cashier,
                ExternalIdentifier = _extIdentifier,
            };
            Domain.Finance.Entities.Receipt _entReceipt = adapter.MapToType(_dtoReceipt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptDto2ReceiptEntity_NullSourceNonCashPayments_EmptyResultList()
        {
            var rcptDto = _dtoReceipt1;
            rcptDto.NonCashPayments = null;
            Domain.Finance.Entities.Receipt result = adapter.MapToType(rcptDto);
            Assert.AreEqual(0, result.NonCashPayments.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptDto2ReceiptEntity_EmptySourceNonCashPayments_EmptyResultList()
        {
            var rcptDto = _dtoReceipt1;
            rcptDto.NonCashPayments = new List<Dtos.Finance.NonCashPayment>();
            Domain.Finance.Entities.Receipt result = adapter.MapToType(rcptDto);
            Assert.AreEqual(0, result.NonCashPayments.Count);
        }

        private bool ArePaymentsEqual(IEnumerable<Dtos.Finance.NonCashPayment> genDtoPayments,
            IEnumerable<Domain.Finance.Entities.NonCashPayment> genEntPayments)
        {
            if (genEntPayments == null || genDtoPayments == null) return false;

            List<Domain.Finance.Entities.NonCashPayment> entPayments = genEntPayments.ToList();
            List<Dtos.Finance.NonCashPayment> dtoPayments = genDtoPayments.ToList();

            // check counts
            int numEnts = entPayments.Count;
            int numDtos = dtoPayments.Count;
            if (numEnts != numDtos) return false;
            // check values
            for (int i = 0; i < numEnts; i++)
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
