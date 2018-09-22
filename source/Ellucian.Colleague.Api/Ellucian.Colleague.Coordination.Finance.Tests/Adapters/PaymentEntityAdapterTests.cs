// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class PaymentEntityAdapterTests
    {
        Ellucian.Colleague.Domain.Finance.Entities.Payments.Payment paymentEntity;
        Dtos.Finance.Payments.Payment paymentDto;
        PaymentEntityAdapter paymentEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            paymentEntityAdapter = new PaymentEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var checkPaymentEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.CheckPayment, Dtos.Finance.Payments.CheckPayment>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.CheckPayment, Dtos.Finance.Payments.CheckPayment>()).Returns(checkPaymentEntityAdapter);

            var paymentItemEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentItem, Dtos.Finance.Payments.PaymentItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentItem, Dtos.Finance.Payments.PaymentItem>()).Returns(paymentItemEntityAdapter);

            paymentEntity = new Domain.Finance.Entities.Payments.Payment()
            {
                AmountToPay = 500m,
                CheckDetails = new Domain.Finance.Entities.Payments.CheckPayment()
                {
                    AbaRoutingNumber = "111111118",
                    BankAccountNumber = "1234567890",
                    BillingAddress1 = "123 Main Street",
                    BillingAddress2 = "Apartment 23",
                    CheckNumber = "123",
                    City = "Fairfax",
                    DriversLicenseNumber = "C77174369",
                    DriversLicenseState = "Virginia",
                    EmailAddress = "john.smith@ellucian.edu",
                    FirstName = "John",
                    LastName = "Smith",
                    State = "Virginia",
                    ZipCode = "22033"
                },
                ConvenienceFee = "CF1",
                ConvenienceFeeAmount = 5m,
                ConvenienceFeeGeneralLedgerNumber = "110101000000010001",
                Distribution = "BANK",
                PaymentItems = new List<Domain.Finance.Entities.Payments.PaymentItem>()
                {
                    new Domain.Finance.Entities.Payments.PaymentItem()
                    {
                        AccountType = "Student",
                        DepositDueId = null,
                        Description = "Student Payment",
                        InvoiceId = "000001234",
                        Overdue = false,
                        PaymentAmount = 300m,
                        PaymentComplete = true,
                        PaymentControlId = "234",
                        PaymentPlanId = "345",
                        Term = "2014/FA"
                    },
                    new Domain.Finance.Entities.Payments.PaymentItem()
                    {
                        AccountType = "Student",
                        DepositDueId = "456",
                        Description = "Student Payment",
                        InvoiceId = null,
                        Overdue = true,
                        PaymentAmount = 200m,
                        PaymentComplete = false,
                        PaymentControlId = null,
                        PaymentPlanId = null,
                        Term = "2014/FA"
                    }
                },
                PayMethod = "CC",
                PersonId = "0001357",
                ProviderAccount = "PAYPALCC",
                ReturnToOriginUrl = "http://localhost/Ellucian.Web.Student/ImmediatePayments/PaymentComplete",
                ReturnUrl = "http://localhost/Ellucian.Web.Student/Finance"
            };

            paymentDto = paymentEntityAdapter.MapToType(paymentEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentEntityAdapter_AmountToPay()
        {
            Assert.AreEqual(paymentEntity.AmountToPay, paymentDto.AmountToPay);
        }

        [TestMethod]
        public void PaymentEntityAdapter_CheckDetails()
        {
            Assert.AreEqual(paymentEntity.CheckDetails.AbaRoutingNumber, paymentDto.CheckDetails.AbaRoutingNumber);
            Assert.AreEqual(paymentEntity.CheckDetails.BankAccountNumber, paymentDto.CheckDetails.BankAccountNumber);
            Assert.AreEqual(paymentEntity.CheckDetails.BillingAddress1, paymentDto.CheckDetails.BillingAddress1);
            Assert.AreEqual(paymentEntity.CheckDetails.BillingAddress2, paymentDto.CheckDetails.BillingAddress2);
            Assert.AreEqual(paymentEntity.CheckDetails.CheckNumber, paymentDto.CheckDetails.CheckNumber);
            Assert.AreEqual(paymentEntity.CheckDetails.City, paymentDto.CheckDetails.City);
            Assert.AreEqual(paymentEntity.CheckDetails.DriversLicenseNumber, paymentDto.CheckDetails.DriversLicenseNumber);
            Assert.AreEqual(paymentEntity.CheckDetails.DriversLicenseState, paymentDto.CheckDetails.DriversLicenseState);
            Assert.AreEqual(paymentEntity.CheckDetails.EmailAddress, paymentDto.CheckDetails.EmailAddress);
            Assert.AreEqual(paymentEntity.CheckDetails.FirstName, paymentDto.CheckDetails.FirstName);
            Assert.AreEqual(paymentEntity.CheckDetails.LastName, paymentDto.CheckDetails.LastName);
            Assert.AreEqual(paymentEntity.CheckDetails.State, paymentDto.CheckDetails.State);
            Assert.AreEqual(paymentEntity.CheckDetails.ZipCode, paymentDto.CheckDetails.ZipCode);
        }

        [TestMethod]
        public void PaymentEntityAdapter_ConvenienceFee()
        {
            Assert.AreEqual(paymentEntity.ConvenienceFee, paymentDto.ConvenienceFee);
        }

        [TestMethod]
        public void PaymentEntityAdapter_ConvenienceFeeAmount()
        {
            Assert.AreEqual(paymentEntity.ConvenienceFeeAmount, paymentDto.ConvenienceFeeAmount);
        }

        [TestMethod]
        public void PaymentEntityAdapter_ConvenienceFeeGeneralLedgerNumber()
        {
            Assert.AreEqual(paymentEntity.ConvenienceFeeGeneralLedgerNumber, paymentDto.ConvenienceFeeGeneralLedgerNumber);
        }

        [TestMethod]
        public void PaymentEntityAdapter_Distribution()
        {
            Assert.AreEqual(paymentEntity.Distribution, paymentDto.Distribution);
        }

        [TestMethod]
        public void PaymentEntityAdapter_PaymentItems()
        {
            Assert.AreEqual(paymentEntity.PaymentItems.Count, paymentDto.PaymentItems.Count);
            for (int i = 0; i < paymentEntity.PaymentItems.Count; i++)
            {
                Assert.AreEqual(paymentEntity.PaymentItems[i].AccountType, paymentDto.PaymentItems[i].AccountType);
                Assert.AreEqual(paymentEntity.PaymentItems[i].DepositDueId, paymentDto.PaymentItems[i].DepositDueId);
                Assert.AreEqual(paymentEntity.PaymentItems[i].Description, paymentDto.PaymentItems[i].Description);
                Assert.AreEqual(paymentEntity.PaymentItems[i].InvoiceId, paymentDto.PaymentItems[i].InvoiceId);
                Assert.AreEqual(paymentEntity.PaymentItems[i].Overdue, paymentDto.PaymentItems[i].Overdue);
                Assert.AreEqual(paymentEntity.PaymentItems[i].PaymentAmount, paymentDto.PaymentItems[i].PaymentAmount);
                Assert.AreEqual(paymentEntity.PaymentItems[i].PaymentComplete, paymentDto.PaymentItems[i].PaymentComplete);
                Assert.AreEqual(paymentEntity.PaymentItems[i].PaymentControlId, paymentDto.PaymentItems[i].PaymentControlId);
                Assert.AreEqual(paymentEntity.PaymentItems[i].PaymentPlanId, paymentDto.PaymentItems[i].PaymentPlanId);
                Assert.AreEqual(paymentEntity.PaymentItems[i].Term, paymentDto.PaymentItems[i].Term);
            }
        }

        [TestMethod]
        public void PaymentEntityAdapter_PayMethod()
        {
            Assert.AreEqual(paymentEntity.PayMethod, paymentDto.PayMethod);
        }

        [TestMethod]
        public void PaymentEntityAdapter_PersonId()
        {
            Assert.AreEqual(paymentEntity.PersonId, paymentDto.PersonId);
        }

        [TestMethod]
        public void PaymentEntityAdapter_ProviderAccount()
        {
            Assert.AreEqual(paymentEntity.ProviderAccount, paymentDto.ProviderAccount);
        }

        [TestMethod]
        public void PaymentEntityAdapter_ReturnToOriginUrl()
        {
            Assert.AreEqual(paymentEntity.ReturnToOriginUrl, paymentDto.ReturnToOriginUrl);
        }

        [TestMethod]
        public void PaymentEntityAdapter_ReturnUrl()
        {
            Assert.AreEqual(paymentEntity.ReturnUrl, paymentDto.ReturnUrl);
        }
    }
}