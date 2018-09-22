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
    public class PaymentTermsAcceptanceDtoAdapterTests
    {
        PaymentTermsAcceptance paymentTermsAcceptanceDto;
        Ellucian.Colleague.Domain.Finance.Entities.PaymentTermsAcceptance paymentTermsAcceptanceEntity;
        PaymentTermsAcceptanceDtoAdapter paymentTermsAcceptanceDtoAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            paymentTermsAcceptanceDtoAdapter = new PaymentTermsAcceptanceDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            paymentTermsAcceptanceDto = new PaymentTermsAcceptance()
            {
                AcknowledgementDateTime = DateTime.Now,
                AcknowledgementText = new List<string>() { "I hereby acknowledge the described payment plan.", "Something else." },
                ApprovalReceived = DateTime.Now.AddMinutes(-3).AddMinutes(-3),
                ApprovalUserId = "jsmith",
                InvoiceIds = new List<string>() { "00001234", "00001235", "000001236" },
                PaymentControlId = "123",
                SectionIds = new List<string>() { "135", "217", "304" },
                StudentId = "0001234",
                TermsText = new List<string>() { "These are the terms and conditions.", "Something else." }
            };

            paymentTermsAcceptanceEntity = paymentTermsAcceptanceDtoAdapter.MapToType(paymentTermsAcceptanceDto);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_AcknowledgementDateTime()
        {
            Assert.AreEqual(paymentTermsAcceptanceDto.AcknowledgementDateTime, paymentTermsAcceptanceEntity.AcknowledgementDateTime);
        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_AcknowledgementText()
        {
            var dtoAcknowledgementTextList = new List<string>(paymentTermsAcceptanceDto.AcknowledgementText);
            CollectionAssert.AreEqual(dtoAcknowledgementTextList, paymentTermsAcceptanceEntity.AcknowledgementText);
        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_ApprovalReceived()
        {
            Assert.AreEqual(paymentTermsAcceptanceDto.ApprovalReceived, paymentTermsAcceptanceEntity.ApprovalReceived);
        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_ApprovalUserId()
        {
            Assert.AreEqual(paymentTermsAcceptanceDto.ApprovalUserId, paymentTermsAcceptanceEntity.ApprovalUserId);
        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_InvoiceIds()
        {
            var dtoInvoiceIdList = new List<string>(paymentTermsAcceptanceDto.InvoiceIds);
            CollectionAssert.AreEqual(dtoInvoiceIdList, paymentTermsAcceptanceEntity.InvoiceIds);
        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_PaymentControlId()
        {
            Assert.AreEqual(paymentTermsAcceptanceDto.PaymentControlId, paymentTermsAcceptanceEntity.PaymentControlId);
        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_SectionIds()
        {
            var dtoSectionIdList = new List<string>(paymentTermsAcceptanceDto.SectionIds);
            CollectionAssert.AreEqual(dtoSectionIdList, paymentTermsAcceptanceEntity.SectionIds);
        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_StudentId()
        {
            Assert.AreEqual(paymentTermsAcceptanceDto.StudentId, paymentTermsAcceptanceEntity.StudentId);
        }

        [TestMethod]
        public void PaymentTermsAcceptanceDtoAdapter_TermsText()
        {
            var dtoTermsText = new List<string>(paymentTermsAcceptanceDto.TermsText);
            CollectionAssert.AreEqual(dtoTermsText, paymentTermsAcceptanceEntity.TermsText);
        }
    }
}