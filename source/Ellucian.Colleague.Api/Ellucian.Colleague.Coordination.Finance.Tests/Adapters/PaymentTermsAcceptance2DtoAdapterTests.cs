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
    public class PaymentTermsAcceptance2DtoAdapterTests
    {
        PaymentTermsAcceptance2 paymentTermsAcceptance2Dto;
        Ellucian.Colleague.Domain.Finance.Entities.PaymentTermsAcceptance paymentTermsAcceptanceEntity;
        PaymentTermsAcceptance2DtoAdapter paymentTermsAcceptance2DtoAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            paymentTermsAcceptance2DtoAdapter = new PaymentTermsAcceptance2DtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            paymentTermsAcceptance2Dto = new PaymentTermsAcceptance2()
            {
                AcknowledgementDateTime = DateTimeOffset.UtcNow,
                AcknowledgementText = new List<string>() { "I hereby acknowledge the described payment plan.", "Something else." },
                ApprovalReceived = DateTimeOffset.UtcNow.AddMinutes(-3).AddMinutes(-3),
                ApprovalUserId = "jsmith",
                InvoiceIds = new List<string>() { "00001234", "00001235", "000001236"},
                PaymentControlId = "123",
                SectionIds = new List<string>() { "135", "217", "304" },
                StudentId = "0001234",
                TermsText = new List<string>() { "These are the terms and conditions.", "Something else." }
            };

            paymentTermsAcceptanceEntity = paymentTermsAcceptance2DtoAdapter.MapToType(paymentTermsAcceptance2Dto);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_AcknowledgementDateTime()
        {
            Assert.AreEqual(paymentTermsAcceptance2Dto.AcknowledgementDateTime, paymentTermsAcceptanceEntity.AcknowledgementDateTime);
        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_AcknowledgementText()
        {
            var dtoAcknowledgementTextList = new List<string>(paymentTermsAcceptance2Dto.AcknowledgementText);
            CollectionAssert.AreEqual(dtoAcknowledgementTextList, paymentTermsAcceptanceEntity.AcknowledgementText);
        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_ApprovalReceived()
        {
            Assert.AreEqual(paymentTermsAcceptance2Dto.ApprovalReceived, paymentTermsAcceptanceEntity.ApprovalReceived);
        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_ApprovalUserId()
        {
            Assert.AreEqual(paymentTermsAcceptance2Dto.ApprovalUserId, paymentTermsAcceptanceEntity.ApprovalUserId);
        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_InvoiceIds()
        {
            var dtoInvoiceIdList = new List<string>(paymentTermsAcceptance2Dto.InvoiceIds);
            CollectionAssert.AreEqual(dtoInvoiceIdList, paymentTermsAcceptanceEntity.InvoiceIds);
        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_PaymentControlId()
        {
            Assert.AreEqual(paymentTermsAcceptance2Dto.PaymentControlId, paymentTermsAcceptanceEntity.PaymentControlId);
        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_SectionIds()
        {
            var dtoSectionIdList = new List<string>(paymentTermsAcceptance2Dto.SectionIds);
            CollectionAssert.AreEqual(dtoSectionIdList, paymentTermsAcceptanceEntity.SectionIds);
        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_StudentId()
        {
            Assert.AreEqual(paymentTermsAcceptance2Dto.StudentId, paymentTermsAcceptanceEntity.StudentId);
        }

        [TestMethod]
        public void PaymentTermsAcceptance2DtoAdapter_TermsText()
        {
            var dtoTermsText = new List<string>(paymentTermsAcceptance2Dto.TermsText);
            CollectionAssert.AreEqual(dtoTermsText, paymentTermsAcceptanceEntity.TermsText);
        }
    }
}