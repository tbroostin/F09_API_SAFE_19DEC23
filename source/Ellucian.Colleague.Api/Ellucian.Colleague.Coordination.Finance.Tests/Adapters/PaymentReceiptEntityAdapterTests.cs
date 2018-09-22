// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class PaymentReceiptEntityAdapterTests
    {
        PaymentReceipt paymentReceiptDto;
        Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentReceipt paymentReceiptEntity;
        PaymentReceiptEntityAdapter paymentReceiptEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            paymentReceiptEntityAdapter = new PaymentReceiptEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var accountsReceivablePaymentEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.AccountsReceivablePayment, Dtos.Finance.Payments.AccountsReceivablePayment>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.Payments.AccountsReceivablePayment, Dtos.Finance.Payments.AccountsReceivablePayment>()).Returns(accountsReceivablePaymentEntityAdapter);

            var accountsReceivableDepositEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.AccountsReceivableDeposit, Dtos.Finance.Payments.AccountsReceivableDeposit>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.Payments.AccountsReceivableDeposit, Dtos.Finance.Payments.AccountsReceivableDeposit>()).Returns(accountsReceivableDepositEntityAdapter);

            var generalPaymentEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.GeneralPayment, Dtos.Finance.Payments.GeneralPayment>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.Payments.GeneralPayment, Dtos.Finance.Payments.GeneralPayment>()).Returns(generalPaymentEntityAdapter);

            var convenienceFeeEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.ConvenienceFee, Dtos.Finance.Payments.ConvenienceFee>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.Payments.ConvenienceFee, Dtos.Finance.Payments.ConvenienceFee>()).Returns(convenienceFeeEntityAdapter);

            var paymentMethodEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.PaymentMethod, Dtos.Finance.Payments.PaymentMethod>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.Payments.PaymentMethod, Dtos.Finance.Payments.PaymentMethod>()).Returns(paymentMethodEntityAdapter);


            paymentReceiptEntity = new Domain.Finance.Entities.Payments.PaymentReceipt()
            {
                AcknowledgeFooterImageUrl = new Uri("http://ellucianuniversity.edu/footer.png"),
                AcknowledgeFooterText = new List<string>() { "This is some...", "footer text." },
                CashReceiptsId = "123",
                ConvenienceFees = new List<Domain.Finance.Entities.Payments.ConvenienceFee>()
                {
                    new Domain.Finance.Entities.Payments.ConvenienceFee()
                    {
                        Amount = 1m,
                        Code = "1FLAT",
                        Description = "One Dollar Flat Fee"
                    },
                    new Domain.Finance.Entities.Payments.ConvenienceFee()
                    {
                        Amount = 2.5m,
                        Code = "25PCT",
                        Description = "2.5 Percent Fee"
                    }
                },
                Deposits = new List<Domain.Finance.Entities.Payments.AccountsReceivableDeposit>()
                {
                    new Domain.Finance.Entities.Payments.AccountsReceivableDeposit()
                    {
                        Description = "Meals Deposit",
                        Location = "MC",
                        LocationDescription = "Main Campus",
                        NetAmount = 300m,
                        PersonId = "0003315",
                        PersonName = "John Smith",
                        Term = "2014/FA",
                        TermDescription = "2014 Fall Term",
                        Type = "MEALS"
                    },
                    new Domain.Finance.Entities.Payments.AccountsReceivableDeposit()
                    {
                        Description = "Rooms Deposit",
                        Location = "MC",
                        LocationDescription = "Main Campus",
                        NetAmount = 250m,
                        PersonId = "0003315",
                        PersonName = "John Smith",
                        Term = "2014/FA",
                        TermDescription = "2014 Fall Term",
                        Type = "RESHL"
                    }
                },
                ErrorMessage = null,
                MerchantEmail = "merchant@ellucian.edu",
                MerchantPhone = "123-456-7890",
                MerchantNameAddress = new List<string>() { "Ellucian University", "123 Main Street", "Fairfax, VA 22033" },
                OtherItems = new List<Domain.Finance.Entities.Payments.GeneralPayment>()
                {
                    new Domain.Finance.Entities.Payments.GeneralPayment()
                    {
                        Code = "CODE",
                        Description = "General Payment Code",
                        Location = "MC",
                        LocationDescription = "Main Campus",
                        NetAmount = 100m
                    },
                    new Domain.Finance.Entities.Payments.GeneralPayment()
                    {
                        Code = "CODE2",
                        Description = "General Payment Code 2",
                        Location = "SC",
                        LocationDescription = "South Campus",
                        NetAmount = 200m
                    }
                },
                PaymentMethods = new List<Domain.Finance.Entities.Payments.PaymentMethod>()
                {
                    new Domain.Finance.Entities.Payments.PaymentMethod()
                    {
                        ConfirmationNumber = "1234567890",
                        ControlNumber = "2345",
                        PayMethodCode = "CC",
                        PayMethodDescription = "Credit Card",
                        TransactionAmount = 401m,
                        TransactionDescription = "Transaction 1",
                        TransactionNumber = "2345678901"
                    },
                    new Domain.Finance.Entities.Payments.PaymentMethod()
                    {
                        ConfirmationNumber = "1357924680",
                        ControlNumber = "3456",
                        PayMethodCode = "ECHK",
                        PayMethodDescription = "Electronic Check",
                        TransactionAmount = 452.5m,
                        TransactionDescription = "Transaction 2",
                        TransactionNumber = "3456789012"
                    }
                },
                Payments = new List<Domain.Finance.Entities.Payments.AccountsReceivablePayment>()
                {
                    new Domain.Finance.Entities.Payments.AccountsReceivablePayment()
                    {
                        Description = "Payment 1",
                        Location = "MC",
                        LocationDescription = "Main Campus",
                        NetAmount = 401m,
                        PaymentControlId = null,
                        PaymentDescription = "Payment Description 1",
                        PersonId = "0003315",
                        PersonName = "John Smith",
                        Term = "2014/FA",
                        TermDescription = "2014 Fall Term",
                        Type = "01"
                    },
                    new Domain.Finance.Entities.Payments.AccountsReceivablePayment()
                    {
                        Description = "Payment 2",
                        Location = "SC",
                        LocationDescription = "South Campus",
                        NetAmount = 452.5m,
                        PaymentControlId = null,
                        PaymentDescription = "Payment Description 2",
                        PersonId = "0003315",
                        PersonName = "John Smith",
                        Term = "2014/FA",
                        TermDescription = "2014 Fall Term",
                        Type = "02"
                    }
                },
                ReceiptAcknowledgeText = new List<string>() { "This is some...", "acknowledgment text." },
                ReceiptDate = DateTime.Today,
                ReceiptNo = "0001234",
                ReceiptPayerId = "0003315",
                ReceiptPayerName = "John Smith",
                ReceiptTime = DateTime.Now.AddMinutes(-3),
                ReturnUrl = "http://www.ellucian.edu/payments"
            };

            paymentReceiptDto = paymentReceiptEntityAdapter.MapToType(paymentReceiptEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_AcknowledgeFooterImageUrl()
        {
            Assert.AreEqual(paymentReceiptEntity.AcknowledgeFooterImageUrl, paymentReceiptDto.AcknowledgeFooterImageUrl);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_AcknowledgeFooterText()
        {
            CollectionAssert.AreEqual(paymentReceiptEntity.AcknowledgeFooterText, paymentReceiptDto.AcknowledgeFooterText);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_CashReceiptsId()
        {
            Assert.AreEqual(paymentReceiptEntity.CashReceiptsId, paymentReceiptDto.CashReceiptsId);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ConvenienceFees()
        {
            Assert.AreEqual(paymentReceiptEntity.ConvenienceFees.Count, paymentReceiptDto.ConvenienceFees.Count);
            for (int i = 0; i < paymentReceiptDto.ConvenienceFees.Count; i++)
            {
                Assert.AreEqual(paymentReceiptEntity.ConvenienceFees[i].Amount, paymentReceiptDto.ConvenienceFees[i].Amount);
                Assert.AreEqual(paymentReceiptEntity.ConvenienceFees[i].Code, paymentReceiptDto.ConvenienceFees[i].Code);
                Assert.AreEqual(paymentReceiptEntity.ConvenienceFees[i].Description, paymentReceiptDto.ConvenienceFees[i].Description);

            }
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_Deposits()
        {
            Assert.AreEqual(paymentReceiptEntity.Deposits.Count, paymentReceiptDto.Deposits.Count);
            for (int i = 0; i < paymentReceiptDto.Deposits.Count; i++)
            {
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].Description, paymentReceiptDto.Deposits[i].Description);
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].Location, paymentReceiptDto.Deposits[i].Location);
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].LocationDescription, paymentReceiptDto.Deposits[i].LocationDescription);
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].NetAmount, paymentReceiptDto.Deposits[i].NetAmount);
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].PersonId, paymentReceiptDto.Deposits[i].PersonId);
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].PersonName, paymentReceiptDto.Deposits[i].PersonName);
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].Term, paymentReceiptDto.Deposits[i].Term);
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].TermDescription, paymentReceiptDto.Deposits[i].TermDescription);
                Assert.AreEqual(paymentReceiptEntity.Deposits[i].Type, paymentReceiptDto.Deposits[i].Type);
            }
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ErrorMessage()
        {
            Assert.AreEqual(paymentReceiptEntity.ErrorMessage, paymentReceiptDto.ErrorMessage);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_MerchantEmail()
        {
            Assert.AreEqual(paymentReceiptEntity.MerchantEmail, paymentReceiptDto.MerchantEmail);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_MerchantNameAddress()
        {
            CollectionAssert.AreEqual(paymentReceiptEntity.MerchantNameAddress, paymentReceiptDto.MerchantNameAddress);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_MerchantPhone()
        {
            Assert.AreEqual(paymentReceiptEntity.MerchantPhone, paymentReceiptDto.MerchantPhone);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_OtherItems()
        {
            Assert.AreEqual(paymentReceiptEntity.OtherItems.Count, paymentReceiptDto.OtherItems.Count);
            for (int i = 0; i < paymentReceiptDto.OtherItems.Count; i++)
            {
                Assert.AreEqual(paymentReceiptEntity.OtherItems[i].Code, paymentReceiptDto.OtherItems[i].Code);
                Assert.AreEqual(paymentReceiptEntity.OtherItems[i].Description, paymentReceiptDto.OtherItems[i].Description);
                Assert.AreEqual(paymentReceiptEntity.OtherItems[i].Location, paymentReceiptDto.OtherItems[i].Location);
                Assert.AreEqual(paymentReceiptEntity.OtherItems[i].LocationDescription, paymentReceiptDto.OtherItems[i].LocationDescription);
                Assert.AreEqual(paymentReceiptEntity.OtherItems[i].NetAmount, paymentReceiptDto.OtherItems[i].NetAmount);
            }
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_PaymentMethods()
        {
            Assert.AreEqual(paymentReceiptEntity.PaymentMethods.Count, paymentReceiptDto.PaymentMethods.Count);
            for (int i = 0; i < paymentReceiptDto.PaymentMethods.Count; i++)
            {
                Assert.AreEqual(paymentReceiptEntity.PaymentMethods[i].ConfirmationNumber, paymentReceiptDto.PaymentMethods[i].ConfirmationNumber);
                Assert.AreEqual(paymentReceiptEntity.PaymentMethods[i].ControlNumber, paymentReceiptDto.PaymentMethods[i].ControlNumber);
                Assert.AreEqual(paymentReceiptEntity.PaymentMethods[i].PayMethodCode, paymentReceiptDto.PaymentMethods[i].PayMethodCode);
                Assert.AreEqual(paymentReceiptEntity.PaymentMethods[i].PayMethodDescription, paymentReceiptDto.PaymentMethods[i].PayMethodDescription);
                Assert.AreEqual(paymentReceiptEntity.PaymentMethods[i].TransactionAmount, paymentReceiptDto.PaymentMethods[i].TransactionAmount);
                Assert.AreEqual(paymentReceiptEntity.PaymentMethods[i].TransactionDescription, paymentReceiptDto.PaymentMethods[i].TransactionDescription);
                Assert.AreEqual(paymentReceiptEntity.PaymentMethods[i].TransactionNumber, paymentReceiptDto.PaymentMethods[i].TransactionNumber);
            }
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_Payments()
        {
            Assert.AreEqual(paymentReceiptEntity.Payments.Count, paymentReceiptDto.Payments.Count);
            for (int i = 0; i < paymentReceiptDto.Payments.Count; i++)
            {
                Assert.AreEqual(paymentReceiptEntity.Payments[i].Description, paymentReceiptDto.Payments[i].Description);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].Location, paymentReceiptDto.Payments[i].Location);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].LocationDescription, paymentReceiptDto.Payments[i].LocationDescription);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].NetAmount, paymentReceiptDto.Payments[i].NetAmount);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].PaymentControlId, paymentReceiptDto.Payments[i].PaymentControlId);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].PaymentDescription, paymentReceiptDto.Payments[i].PaymentDescription);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].PersonId, paymentReceiptDto.Payments[i].PersonId);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].PersonName, paymentReceiptDto.Payments[i].PersonName);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].Term, paymentReceiptDto.Payments[i].Term);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].TermDescription, paymentReceiptDto.Payments[i].TermDescription);
                Assert.AreEqual(paymentReceiptEntity.Payments[i].Type, paymentReceiptDto.Payments[i].Type);
            }
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ReceiptAcknowledgeText()
        {
            CollectionAssert.AreEqual(paymentReceiptEntity.ReceiptAcknowledgeText, paymentReceiptDto.ReceiptAcknowledgeText);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ReceiptDate()
        {
            Assert.AreEqual(paymentReceiptEntity.ReceiptDate, paymentReceiptDto.ReceiptDate);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ReceiptNo()
        {
            Assert.AreEqual(paymentReceiptEntity.ReceiptNo, paymentReceiptDto.ReceiptNo);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ReceiptPayerId()
        {
            Assert.AreEqual(paymentReceiptEntity.ReceiptPayerId, paymentReceiptDto.ReceiptPayerId);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ReceiptPayerName()
        {
            Assert.AreEqual(paymentReceiptEntity.ReceiptPayerName, paymentReceiptDto.ReceiptPayerName);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ReceiptTime()
        {
            Assert.AreEqual(paymentReceiptEntity.ReceiptTime, paymentReceiptDto.ReceiptTime);
        }

        [TestMethod]
        public void PaymentReceiptEntityAdapterTests_ReturnUrl()
        {
            Assert.AreEqual(paymentReceiptEntity.ReturnUrl, paymentReceiptDto.ReturnUrl);
        }
    }
}