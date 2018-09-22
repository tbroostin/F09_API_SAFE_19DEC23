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
    public class PaymentDtoAdapterTests
    {
        Dtos.Finance.Payments.Payment paymentDto;
        Ellucian.Colleague.Domain.Finance.Entities.Payments.Payment paymentEntity;
        PaymentDtoAdapter paymentDtoAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            paymentDtoAdapter = new PaymentDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var checkPaymentDtoAdapter = new AutoMapperAdapter<Dtos.Finance.Payments.CheckPayment, Ellucian.Colleague.Domain.Finance.Entities.Payments.CheckPayment>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Finance.Payments.CheckPayment, Ellucian.Colleague.Domain.Finance.Entities.Payments.CheckPayment>()).Returns(checkPaymentDtoAdapter);

            var paymentItemDtoAdapter = new AutoMapperAdapter<Dtos.Finance.Payments.PaymentItem, Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentItem>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Finance.Payments.PaymentItem, Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentItem>()).Returns(paymentItemDtoAdapter);

            paymentDto = new Dtos.Finance.Payments.Payment()
            {
                AmountToPay = 500m,
                CheckDetails = new Dtos.Finance.Payments.CheckPayment()
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
                PaymentItems = new List<Dtos.Finance.Payments.PaymentItem>()
                {
                    new Dtos.Finance.Payments.PaymentItem()
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
                    new Dtos.Finance.Payments.PaymentItem()
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

            paymentEntity = paymentDtoAdapter.MapToType(paymentDto);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentDtoAdapter_AmountToPay()
        {
            Assert.AreEqual(paymentDto.AmountToPay, paymentEntity.AmountToPay);
        }

        [TestMethod]
        public void PaymentDtoAdapter_CheckDetails()
        {
            Assert.AreEqual(paymentDto.CheckDetails.AbaRoutingNumber, paymentEntity.CheckDetails.AbaRoutingNumber);
            Assert.AreEqual(paymentDto.CheckDetails.BankAccountNumber, paymentEntity.CheckDetails.BankAccountNumber);
            Assert.AreEqual(paymentDto.CheckDetails.BillingAddress1, paymentEntity.CheckDetails.BillingAddress1);
            Assert.AreEqual(paymentDto.CheckDetails.BillingAddress2, paymentEntity.CheckDetails.BillingAddress2);
            Assert.AreEqual(paymentDto.CheckDetails.CheckNumber, paymentEntity.CheckDetails.CheckNumber);
            Assert.AreEqual(paymentDto.CheckDetails.City, paymentEntity.CheckDetails.City);
            Assert.AreEqual(paymentDto.CheckDetails.DriversLicenseNumber, paymentEntity.CheckDetails.DriversLicenseNumber);
            Assert.AreEqual(paymentDto.CheckDetails.DriversLicenseState, paymentEntity.CheckDetails.DriversLicenseState);
            Assert.AreEqual(paymentDto.CheckDetails.EmailAddress, paymentEntity.CheckDetails.EmailAddress);
            Assert.AreEqual(paymentDto.CheckDetails.FirstName, paymentEntity.CheckDetails.FirstName);
            Assert.AreEqual(paymentDto.CheckDetails.LastName, paymentEntity.CheckDetails.LastName);
            Assert.AreEqual(paymentDto.CheckDetails.State, paymentEntity.CheckDetails.State);
            Assert.AreEqual(paymentDto.CheckDetails.ZipCode, paymentEntity.CheckDetails.ZipCode);
        }

        [TestMethod]
        public void PaymentDtoAdapter_ConvenienceFee()
        {
            Assert.AreEqual(paymentDto.ConvenienceFee, paymentEntity.ConvenienceFee);
        }

        [TestMethod]
        public void PaymentDtoAdapter_ConvenienceFeeAmount()
        {
            Assert.AreEqual(paymentDto.ConvenienceFeeAmount, paymentEntity.ConvenienceFeeAmount);
        }

        [TestMethod]
        public void PaymentDtoAdapter_ConvenienceFeeGeneralLedgerNumber()
        {
            Assert.AreEqual(paymentDto.ConvenienceFeeGeneralLedgerNumber, paymentEntity.ConvenienceFeeGeneralLedgerNumber);
        }

        [TestMethod]
        public void PaymentDtoAdapter_Distribution()
        {
            Assert.AreEqual(paymentDto.Distribution, paymentEntity.Distribution);
        }

        [TestMethod]
        public void PaymentDtoAdapter_PaymentItems()
        {
            var paymentItemsList = new List<Dtos.Finance.Payments.PaymentItem>(paymentDto.PaymentItems);
            Assert.AreEqual(paymentItemsList.Count, paymentEntity.PaymentItems.Count);
            for (int i = 0; i < paymentItemsList.Count; i++)
            {
                Assert.AreEqual(paymentItemsList[i].AccountType, paymentEntity.PaymentItems[i].AccountType);
                Assert.AreEqual(paymentItemsList[i].DepositDueId, paymentEntity.PaymentItems[i].DepositDueId);
                Assert.AreEqual(paymentItemsList[i].Description, paymentEntity.PaymentItems[i].Description);
                Assert.AreEqual(paymentItemsList[i].InvoiceId, paymentEntity.PaymentItems[i].InvoiceId);
                Assert.AreEqual(paymentItemsList[i].Overdue, paymentEntity.PaymentItems[i].Overdue);
                Assert.AreEqual(paymentItemsList[i].PaymentAmount, paymentEntity.PaymentItems[i].PaymentAmount);
                Assert.AreEqual(paymentItemsList[i].PaymentComplete, paymentEntity.PaymentItems[i].PaymentComplete);
                Assert.AreEqual(paymentItemsList[i].PaymentControlId, paymentEntity.PaymentItems[i].PaymentControlId);
                Assert.AreEqual(paymentItemsList[i].PaymentPlanId, paymentEntity.PaymentItems[i].PaymentPlanId);
                Assert.AreEqual(paymentItemsList[i].Term, paymentEntity.PaymentItems[i].Term);
            }
        }

        [TestMethod]
        public void PaymentDtoAdapter_PayMethod()
        {
            Assert.AreEqual(paymentDto.PayMethod, paymentEntity.PayMethod);
        }

        [TestMethod]
        public void PaymentDtoAdapter_PersonId()
        {
            Assert.AreEqual(paymentDto.PersonId, paymentEntity.PersonId);
        }

        [TestMethod]
        public void PaymentDtoAdapter_ProviderAccount()
        {
            Assert.AreEqual(paymentDto.ProviderAccount, paymentEntity.ProviderAccount);
        }

        [TestMethod]
        public void PaymentDtoAdapter_ReturnToOriginUrl()
        {
            Assert.AreEqual(paymentDto.ReturnToOriginUrl, paymentEntity.ReturnToOriginUrl);
        }

        [TestMethod]
        public void PaymentDtoAdapter_ReturnUrl()
        {
            Assert.AreEqual(paymentDto.ReturnUrl, paymentEntity.ReturnUrl);
        }        
    }
}