// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
    public class ReceivableChargeDtoAdapterTests
    {
        ReceivableCharge chargeDto,
            chargeDto2;
        Ellucian.Colleague.Domain.Finance.Entities.ReceivableCharge chargeEntity,
            chargeEntity2;

        string id = "LINE1";
        string invoiceId = "1";
        List<string> description;
        string code = "AR1";
        decimal baseAmt = 1000;
        decimal taxAmt = 50;
        List<string> allocs;
        List<string> plans;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var chargeDtoAdapter = new ReceivableChargeDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            // Complex initializations
            description = new List<string>() { "Description line 1", "Description line 2" };
            allocs = new List<string>() { "Allocation 1", "Allocation 2" };
            plans = new List<string>() { "Payment plan 1", "Payment plan 2" };

            // Create the ReceivableCharge DTO
            chargeDto = 
                new Ellucian.Colleague.Dtos.Finance.ReceivableCharge(){
                    Id = id,
                    InvoiceId = invoiceId,
                    Description = description,
                    Code = code,
                    BaseAmount = baseAmt,
                    TaxAmount = taxAmt,
                    AllocationIds = allocs,
                    PaymentPlanIds = plans
                };

            chargeEntity = chargeDtoAdapter.MapToType(chargeDto);

            // Create a DTO without allocations or pay plan ids
            chargeDto2 =
                new Ellucian.Colleague.Dtos.Finance.ReceivableCharge()
                {
                    Id = id,
                    InvoiceId = invoiceId,
                    Description = description,
                    Code = code,
                    BaseAmount = baseAmt,
                    TaxAmount = taxAmt
                };

            chargeEntity2 = chargeDtoAdapter.MapToType(chargeDto2);

        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_Id()
        {
            Assert.AreEqual(id, chargeEntity.Id);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_InvoiceId()
        {
            Assert.AreEqual(invoiceId, chargeEntity.InvoiceId);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_Code()
        {
            Assert.AreEqual(code, chargeEntity.Code);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_BaseAmount()
        {
            Assert.AreEqual(baseAmt, chargeEntity.BaseAmount);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_TaxAmount()
        {
            Assert.AreEqual(taxAmt, chargeEntity.TaxAmount);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_DescriptionCount()
        {
            Assert.AreEqual(description.Count, chargeEntity.Description.Count);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_DescriptionContent()
        {
            for (int i = 0; i < description.Count; i++)
            {
                Assert.AreEqual(description[i], chargeEntity.Description[i]);
            }
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_AllocationCount()
        {
            Assert.AreEqual(allocs.Count, chargeEntity.AllocationIds.Count);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_AllocationContent()
        {
            for (int i = 0; i < allocs.Count; i++)
            {
                Assert.AreEqual(allocs[i], chargeEntity.AllocationIds[i]);
            }
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_PaymentPlanCount()
        {
            Assert.AreEqual(plans.Count, chargeEntity.PaymentPlanIds.Count);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_PaymentPlanContent()
        {
            for (int i = 0; i < allocs.Count; i++)
            {
                Assert.AreEqual(plans[i], chargeEntity.PaymentPlanIds[i]);
            }
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_NullAllocationOk()
        {
            Assert.IsTrue(chargeEntity2.AllocationIds != null && chargeEntity2.AllocationIds.Count == 0);
        }

        [TestMethod]
        public void ReceivableChargeDtoAdapter_NullPaymentPlanOk()
        {
            Assert.IsTrue(chargeEntity2.PaymentPlanIds != null && chargeEntity2.PaymentPlanIds.Count == 0);
        }
    }
}
