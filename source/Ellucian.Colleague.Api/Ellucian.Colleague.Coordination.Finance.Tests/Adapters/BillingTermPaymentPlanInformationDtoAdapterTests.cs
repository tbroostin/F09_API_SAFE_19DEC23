// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class BillingTermPaymentPlanInformationDtoAdapterTests
    {
        private BillingTermPaymentPlanInformation entity;
        private Dtos.Finance.BillingTermPaymentPlanInformation dto;
        private BillingTermPaymentPlanInformationDtoAdapter adapter;

        [TestInitialize]
        public void Initialize()
        {
            dto = new Dtos.Finance.BillingTermPaymentPlanInformation()
            {
                PersonId = "0001234", 
                TermId = "2017/SP", 
                ReceivableTypeCode = "01", 
                PaymentPlanAmount = 500m, 
                PaymentPlanTemplateId = "DEFAULT",
                IneligibilityReason = Dtos.Finance.PaymentPlanIneligibilityReason.ChargesAreNotEligible
            };

            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapter = new BillingTermPaymentPlanInformationDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            entity = null;
            dto = null;
            adapter = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BillingTermPaymentPlanInformationDtoAdapter_MapToType_Source_Null()
        {
            entity = adapter.MapToType(null);
        }

        [TestMethod]
        public void BillingTermPaymentPlanInformationDtoAdapter_MapToType_Source_Valid()
        {
            entity = adapter.MapToType(dto);
            Assert.AreEqual(dto.PersonId, entity.PersonId);
            Assert.AreEqual(dto.TermId, entity.TermId);
            Assert.AreEqual(dto.ReceivableTypeCode, entity.ReceivableTypeCode);
            Assert.AreEqual(dto.PaymentPlanAmount, entity.PaymentPlanAmount);
            Assert.AreEqual(dto.PaymentPlanTemplateId, entity.PaymentPlanTemplateId);
            Assert.AreEqual(PaymentPlanIneligibilityReason.ChargesAreNotEligible, entity.IneligibilityReason);
        }
    }
}
