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
    public class ReceivableInvoiceEntityAdapterTests
    {
        ReceivableInvoice receivableInvoiceDto;
        Ellucian.Colleague.Domain.Finance.Entities.ReceivableInvoice receivableInvoiceEntity;
        ReceivableInvoiceEntityAdapter receivableInvoiceEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            receivableInvoiceEntityAdapter = new ReceivableInvoiceEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var receivableChargeEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.ReceivableCharge, ReceivableCharge>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ReceivableCharge, ReceivableCharge>()).Returns(receivableChargeEntityAdapter);

            receivableInvoiceEntity = new Domain.Finance.Entities.ReceivableInvoice("123", "0001234", "01", "2014/FA", "00012345", DateTime.Today,
                DateTime.Today.AddDays(3), DateTime.Today.AddMonths(-1), DateTime.Today.AddMonths(1), "ReceivableInvoice Description",
                new List<Domain.Finance.Entities.ReceivableCharge>() 
                {
                    new Domain.Finance.Entities.ReceivableCharge("234", "123", new List<string>() {"Materials Fee" }, "MATFE", -250m) { TaxAmount = -25m },
                    new Domain.Finance.Entities.ReceivableCharge("235", "123", new List<string>() {"Athletics Fee" }, "ATHFE", -375m) { TaxAmount = -37.5m}
                })
            {
                AdjustmentToInvoice = "122",
                IsArchived = false,
                InvoiceType = "InvType",
                Location = "MC",
            };
            receivableInvoiceEntity.Charges[0].AddAllocation("222");
            receivableInvoiceEntity.Charges[0].AddPaymentPlan("333");
            receivableInvoiceEntity.AddAdjustingInvoice("124");
            receivableInvoiceEntity.AddAdjustingInvoice("125");
            receivableInvoiceEntity.AddExternalSystemAndId("EXTSYS", "ABC123");
            receivableInvoiceDto = receivableInvoiceEntityAdapter.MapToType(receivableInvoiceEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_AdjustedByInvoices()
        {
            CollectionAssert.AreEqual(receivableInvoiceEntity.AdjustedByInvoices, receivableInvoiceDto.AdjustedByInvoices);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_AdjustmentToInvoice()
        {
            Assert.AreEqual(receivableInvoiceEntity.AdjustmentToInvoice, receivableInvoiceDto.AdjustmentToInvoice);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_Amount()
        {
            Assert.AreEqual(receivableInvoiceEntity.Amount, receivableInvoiceDto.Amount);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_BaseAmount()
        {
            Assert.AreEqual(receivableInvoiceEntity.BaseAmount, receivableInvoiceDto.BaseAmount);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_BillingEnd()
        {
            Assert.AreEqual(receivableInvoiceEntity.BillingEnd, receivableInvoiceDto.BillingEnd);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_BillingStart()
        {
            Assert.AreEqual(receivableInvoiceEntity.BillingStart, receivableInvoiceDto.BillingStart);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_Charges()
        {
            List<ReceivableCharge> actual = new List<ReceivableCharge>(receivableInvoiceDto.Charges);
            Assert.AreEqual(receivableInvoiceEntity.Charges.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                CollectionAssert.AreEqual(receivableInvoiceEntity.Charges[i].AllocationIds, actual[i].AllocationIds);
                Assert.AreEqual(receivableInvoiceEntity.Charges[i].BaseAmount, actual[i].BaseAmount);
                Assert.AreEqual(receivableInvoiceEntity.Charges[i].Code, actual[i].Code);
                CollectionAssert.AreEqual(receivableInvoiceEntity.Charges[i].Description, actual[i].Description);
                Assert.AreEqual(receivableInvoiceEntity.Charges[i].Id, actual[i].Id);
                Assert.AreEqual(receivableInvoiceEntity.Charges[i].InvoiceId, actual[i].InvoiceId);
                Assert.AreEqual(receivableInvoiceEntity.Charges[i].PaymentPlanIds.Count, actual[i].PaymentPlanIds.Count);
                CollectionAssert.AreEqual(receivableInvoiceEntity.Charges[i].PaymentPlanIds, actual[i].PaymentPlanIds);
                Assert.AreEqual(receivableInvoiceEntity.Charges[i].TaxAmount, actual[i].TaxAmount);
            }
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_Date()
        {
            Assert.AreEqual(receivableInvoiceEntity.Date, receivableInvoiceDto.Date);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_Description()
        {
            Assert.AreEqual(receivableInvoiceEntity.Description, receivableInvoiceDto.Description);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_DueDate()
        {
            Assert.AreEqual(receivableInvoiceEntity.DueDate, receivableInvoiceDto.DueDate);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_ExternalIdentifier()
        {
            Assert.AreEqual(receivableInvoiceEntity.ExternalIdentifier, receivableInvoiceDto.ExternalIdentifier);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_ExternalSystem()
        {
            Assert.AreEqual(receivableInvoiceEntity.ExternalSystem, receivableInvoiceDto.ExternalSystem);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_Id()
        {
            Assert.AreEqual(receivableInvoiceEntity.Id, receivableInvoiceDto.Id);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_InvoiceType()
        {
            Assert.AreEqual(receivableInvoiceEntity.InvoiceType, receivableInvoiceDto.InvoiceType);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_IsArchived()
        {
            Assert.AreEqual(receivableInvoiceEntity.IsArchived, receivableInvoiceDto.IsArchived);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_Location()
        {
            Assert.AreEqual(receivableInvoiceEntity.Location, receivableInvoiceDto.Location);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_PersonId()
        {
            Assert.AreEqual(receivableInvoiceEntity.PersonId, receivableInvoiceDto.PersonId);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_ReceivableType()
        {
            Assert.AreEqual(receivableInvoiceEntity.ReceivableType, receivableInvoiceDto.ReceivableType);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_ReferenceNumber()
        {
            Assert.AreEqual(receivableInvoiceEntity.ReferenceNumber, receivableInvoiceDto.ReferenceNumber);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_TaxAmount()
        {
            Assert.AreEqual(receivableInvoiceEntity.TaxAmount, receivableInvoiceDto.TaxAmount);
        }

        [TestMethod]
        public void ReceivableInvoiceEntityAdapterTests_TermId()
        {
            Assert.AreEqual(receivableInvoiceEntity.TermId, receivableInvoiceDto.TermId);
        }
    }
}