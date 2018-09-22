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
    public class AllocationEntityAdapterTests
    {
        PaymentAllocation allocationDto;
        Ellucian.Colleague.Domain.Finance.Entities.PaymentAllocation allocationEntity;
        AllocationEntityAdapter allocationEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            allocationEntityAdapter = new AllocationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var allocationSourceEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PaymentAllocationSource, PaymentAllocationSource>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentAllocationSource, PaymentAllocationSource>()).Returns(allocationSourceEntityAdapter);

            allocationEntity = new Domain.Finance.Entities.PaymentAllocation("123", "234", Domain.Finance.Entities.PaymentAllocationSource.System, 500m)
            {
                ChargeId = "345",
                IsInvoiceAllocated = true,
                ScheduledPaymentId = "456",
            };

            allocationDto = allocationEntityAdapter.MapToType(allocationEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void AllocationEntityAdapterTests_Amount()
        {
            Assert.AreEqual(allocationEntity.Amount, allocationDto.Amount);
        }

        [TestMethod]
        public void AllocationEntityAdapterTests_ChargeId()
        {
            Assert.AreEqual(allocationEntity.ChargeId, allocationDto.ChargeId);
        }

        [TestMethod]
        public void AllocationEntityAdapterTests_Id()
        {
            Assert.AreEqual(allocationEntity.Id, allocationDto.Id);
        }

        [TestMethod]
        public void AllocationEntityAdapterTests_IsInvoiceAllocated()
        {
            Assert.AreEqual(allocationEntity.IsInvoiceAllocated, allocationDto.IsInvoiceAllocated);
        }

        [TestMethod]
        public void AllocationEntityAdapterTests_PaymentId()
        {
            Assert.AreEqual(allocationEntity.PaymentId, allocationDto.PaymentId);
        }

        [TestMethod]
        public void AllocationEntityAdapterTests_ScheduledPaymentId()
        {
            Assert.AreEqual(allocationEntity.ScheduledPaymentId, allocationDto.ScheduledPaymentId);
        }

        [TestMethod]
        public void AllocationEntityAdapterTests_Source()
        {
            Assert.AreEqual(Dtos.Finance.PaymentAllocationSource.System, allocationDto.Source);
        }

        [TestMethod]
        public void AllocationEntityAdapterTests_UnallocatedAmount()
        {
            Assert.AreEqual(allocationEntity.UnallocatedAmount, allocationDto.UnallocatedAmount);
        }
    }
}