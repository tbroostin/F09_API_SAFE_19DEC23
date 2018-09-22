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
    public class PaymentRequirementEntityAdapterTests
    {
        PaymentRequirement paymentRequirementDto;
        Ellucian.Colleague.Domain.Finance.Entities.PaymentRequirement paymentRequirementEntity;
        PaymentRequirementEntityAdapter paymentRequirementEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            paymentRequirementEntityAdapter = new PaymentRequirementEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var paymentDeferralOptionEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PaymentDeferralOption, Dtos.Finance.PaymentDeferralOption>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.PaymentDeferralOption, Dtos.Finance.PaymentDeferralOption>()).Returns(paymentDeferralOptionEntityAdapter);

            var paymentPlanOptionEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PaymentPlanOption, Dtos.Finance.PaymentPlanOption>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.PaymentPlanOption, Dtos.Finance.PaymentPlanOption>()).Returns(paymentPlanOptionEntityAdapter);


            paymentRequirementEntity = new Domain.Finance.Entities.PaymentRequirement("PRID", "2014/FA", "ELIG", 1,
                new List<Domain.Finance.Entities.PaymentDeferralOption>()
                {
                    new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), 50m),
                    new Domain.Finance.Entities.PaymentDeferralOption(DateTime.Today.AddDays(31), null, 0m)
                },
                new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "DEFAULT", DateTime.Today.AddDays(35)),
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(31), DateTime.Today.AddDays(60), "TEMPLATE2", DateTime.Today.AddDays(65))
                });

            paymentRequirementDto = paymentRequirementEntityAdapter.MapToType(paymentRequirementEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentRequirementEntityAdapterTests_DeferralOptions()
        {
            var planOptions = new List<Dtos.Finance.PaymentDeferralOption>(paymentRequirementDto.DeferralOptions);
            Assert.AreEqual(paymentRequirementEntity.DeferralOptions.Count, planOptions.Count);
            for (int i = 0; i < planOptions.Count; i++)
            {
                Assert.AreEqual(paymentRequirementEntity.DeferralOptions[i].DeferralPercent, planOptions[i].DeferralPercent);
                Assert.AreEqual(paymentRequirementEntity.DeferralOptions[i].EffectiveEnd, planOptions[i].EffectiveEnd);
                Assert.AreEqual(paymentRequirementEntity.DeferralOptions[i].EffectiveStart, planOptions[i].EffectiveStart);
            }
        }

        [TestMethod]
        public void PaymentRequirementEntityAdapterTests_EligibilityRuleId()
        {
            Assert.AreEqual(paymentRequirementEntity.EligibilityRuleId, paymentRequirementDto.EligibilityRuleId);
        }

        [TestMethod]
        public void PaymentRequirementEntityAdapterTests_Id()
        {
            Assert.AreEqual(paymentRequirementEntity.Id, paymentRequirementDto.Id);
        }

        [TestMethod]
        public void PaymentRequirementEntityAdapterTests_PaymentPlanOptions()
        {
            var planOptions = new List<Dtos.Finance.PaymentPlanOption>(paymentRequirementDto.PaymentPlanOptions);
            Assert.AreEqual(paymentRequirementEntity.PaymentPlanOptions.Count, planOptions.Count);
            for (int i = 0; i < planOptions.Count; i++)
            {
                Assert.AreEqual(paymentRequirementEntity.PaymentPlanOptions[i].EffectiveEnd, planOptions[i].EffectiveEnd);
                Assert.AreEqual(paymentRequirementEntity.PaymentPlanOptions[i].EffectiveStart, planOptions[i].EffectiveStart);
                Assert.AreEqual(paymentRequirementEntity.PaymentPlanOptions[i].FirstPaymentDate, planOptions[i].FirstPaymentDate);
                Assert.AreEqual(paymentRequirementEntity.PaymentPlanOptions[i].TemplateId, planOptions[i].TemplateId);
            }
        }

        [TestMethod]
        public void PaymentRequirementEntityAdapterTests_TermId()
        {
            Assert.AreEqual(paymentRequirementEntity.TermId, paymentRequirementDto.TermId);
        }
    }
}