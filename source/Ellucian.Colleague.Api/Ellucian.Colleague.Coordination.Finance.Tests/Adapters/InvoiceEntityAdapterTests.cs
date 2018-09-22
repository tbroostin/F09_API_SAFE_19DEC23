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
    public class InvoiceEntityAdapterTests
    {
        Invoice invoiceDto;
        Ellucian.Colleague.Domain.Finance.Entities.Invoice invoiceEntity;
        InvoiceEntityAdapter invoiceEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            invoiceEntityAdapter = new InvoiceEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var chargeEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Charge, Charge>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Charge, Charge>()).Returns(chargeEntityAdapter);

            invoiceEntity = new Domain.Finance.Entities.Invoice("123", "0001234", "01", "2014/FA", "00012345", DateTime.Today,
                DateTime.Today.AddDays(3), DateTime.Today.AddMonths(-1), DateTime.Today.AddMonths(1), "Invoice Description",
                new List<Domain.Finance.Entities.Charge>() 
                {
                    new Domain.Finance.Entities.Charge("234", "123", new List<string>() {"Materials Fee" }, "MATFE", -250m) { TaxAmount = -25m },
                    new Domain.Finance.Entities.Charge("235", "123", new List<string>() {"Athletics Fee" }, "ATHFE", -375m) { TaxAmount = -37.5m}
                })
            {
                AdjustmentToInvoice = "122",
                Archived = false,
            };
            invoiceEntity.Charges[0].AddAllocation("222");
            invoiceEntity.Charges[0].AddPaymentPlan("333");
            invoiceEntity.AddAdjustingInvoice("124");
            invoiceDto = invoiceEntityAdapter.MapToType(invoiceEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_Amount()
        {
            Assert.AreEqual(invoiceEntity.Amount, invoiceDto.Amount);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_BaseAmount()
        {
            Assert.AreEqual(invoiceEntity.BaseAmount, invoiceDto.BaseAmount);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_BillingEnd()
        {
            Assert.AreEqual(invoiceEntity.BillingEnd, invoiceDto.BillingEnd);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_BillingStart()
        {
            Assert.AreEqual(invoiceEntity.BillingStart, invoiceDto.BillingStart);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_Charges()
        {
            List<Charge> actual = new List<Charge>(invoiceDto.Charges);
            Assert.AreEqual(invoiceEntity.Charges.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(invoiceEntity.Charges[i].Amount, actual[i].Amount);
                Assert.AreEqual(invoiceEntity.Charges[i].BaseAmount, actual[i].BaseAmount);
                Assert.AreEqual(invoiceEntity.Charges[i].Code, actual[i].Code);
                CollectionAssert.AreEqual(invoiceEntity.Charges[i].Description, actual[i].Description);
                Assert.AreEqual(invoiceEntity.Charges[i].Id, actual[i].Id);
                Assert.AreEqual(invoiceEntity.Charges[i].InvoiceId, actual[i].InvoiceId);
                Assert.AreEqual(invoiceEntity.Charges[i].PaymentPlanIds.Count, actual[i].PaymentPlanIds.Count);
                CollectionAssert.AreEqual(invoiceEntity.Charges[i].PaymentPlanIds, actual[i].PaymentPlanIds);
                Assert.AreEqual(invoiceEntity.Charges[i].TaxAmount, actual[i].TaxAmount);
            }
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_Date()
        {
            Assert.AreEqual(invoiceEntity.Date, invoiceDto.Date);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_Description()
        {
            Assert.AreEqual(invoiceEntity.Description, invoiceDto.Description);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_DueDate()
        {
            Assert.AreEqual(invoiceEntity.DueDate, invoiceDto.DueDate);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_Id()
        {
            Assert.AreEqual(invoiceEntity.Id, invoiceDto.Id);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_PersonId()
        {
            Assert.AreEqual(invoiceEntity.PersonId, invoiceDto.PersonId);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_ReceivableTypeCode()
        {
            Assert.AreEqual(invoiceEntity.ReceivableTypeCode, invoiceDto.ReceivableTypeCode);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_ReferenceNumber()
        {
            Assert.AreEqual(invoiceEntity.ReferenceNumber, invoiceDto.ReferenceNumber);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_TaxAmount()
        {
            Assert.AreEqual(invoiceEntity.TaxAmount, invoiceDto.TaxAmount);
        }

        [TestMethod]
        public void InvoiceEntityAdapterTests_TermId()
        {
            Assert.AreEqual(invoiceEntity.TermId, invoiceDto.TermId);
        }
    }
}