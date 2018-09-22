// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class PlanChargeEntityAdapterTests
    {
        PlanCharge planChargeDto;
        Ellucian.Colleague.Domain.Finance.Entities.Charge chargeEntity;
        Ellucian.Colleague.Domain.Finance.Entities.PlanCharge planChargeEntity;
        PlanChargeEntityAdapter planChargeEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            planChargeEntityAdapter = new PlanChargeEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var chargeEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Charge, Charge>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Charge, Charge>()).Returns(chargeEntityAdapter);

            chargeEntity = new Domain.Finance.Entities.Charge("000000123", "000001234", 
                    new List<string>() { "Charge on a plan", "Some more text"}, "PPLAN", 500m)
                    {
                        TaxAmount = 5m
                    };
            chargeEntity.AddAllocation("135");
            chargeEntity.AddAllocation("136");
            chargeEntity.AddPaymentPlan("123");
            chargeEntity.AddPaymentPlan("124");

            planChargeEntity = new Domain.Finance.Entities.PlanCharge("123", chargeEntity, 305m, true, false);

            planChargeDto = planChargeEntityAdapter.MapToType(planChargeEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PlanChargeEntityAdapterTests_Amount()
        {
            Assert.AreEqual(planChargeEntity.Amount, planChargeDto.Amount);
        }

        [TestMethod]
        public void PlanChargeEntityAdapterTests_Charge()
        {
            Assert.AreEqual(planChargeEntity.Charge.Amount, planChargeDto.Charge.Amount);
            Assert.AreEqual(planChargeEntity.Charge.BaseAmount, planChargeDto.Charge.BaseAmount);
            Assert.AreEqual(planChargeEntity.Charge.Code, planChargeDto.Charge.Code);
            CollectionAssert.AreEqual(planChargeEntity.Charge.Description, planChargeDto.Charge.Description);
            Assert.AreEqual(planChargeEntity.Charge.Id, planChargeDto.Charge.Id);
            Assert.AreEqual(planChargeEntity.Charge.InvoiceId, planChargeDto.Charge.InvoiceId);
            CollectionAssert.AreEqual(planChargeEntity.Charge.PaymentPlanIds, planChargeDto.Charge.PaymentPlanIds);
            Assert.AreEqual(planChargeEntity.Charge.TaxAmount, planChargeDto.Charge.TaxAmount);
        }

        [TestMethod]
        public void PlanChargeEntityAdapterTests_Id()
        {
            Assert.AreEqual(planChargeEntity.Id, planChargeDto.Id);
        }

        [TestMethod]
        public void PlanChargeEntityAdapterTests_IsAutomaticallyModifiable()
        {
            Assert.AreEqual(planChargeEntity.IsAutomaticallyModifiable, planChargeDto.IsAutomaticallyModifiable);
        }

        [TestMethod]
        public void PlanChargeEntityAdapterTests_IsSetupCharge()
        {
            Assert.AreEqual(planChargeEntity.IsSetupCharge, planChargeDto.IsSetupCharge);
        }
        
        [TestMethod]
        public void PlanChargeEntityAdapterTests_PlanId()
        {
            Assert.AreEqual(planChargeEntity.PlanId, planChargeDto.PlanId);
        }
    }
}