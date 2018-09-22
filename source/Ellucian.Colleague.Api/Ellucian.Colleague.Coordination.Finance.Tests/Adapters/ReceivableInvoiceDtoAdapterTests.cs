// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Data.Base.Tests;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class ReceivableInvoiceDtoAdapterTests
    {
        ReceivableInvoice invDto;
        DateTime rightNow = DateTime.Now;
        Ellucian.Colleague.Domain.Finance.Entities.ReceivableInvoice invEntity;

        string id = "1";
        string personId = "0004224";
        string rcvType = "ARTYPE";
        string term = "TERM";
        string refNum = "My Reference Number";
        DateTime invDate;
        bool isArchived = false;
        string location = "HERE";
        string extSystem = "ACME";
        string extId = "42";

        DateTime dueDate;
        DateTime billStartDate;
        DateTime billEndDate;
        string desc = "Description of invoice";
        string adjTo = "Adjustment to 2";
        List<string> adjBy;
        decimal baseAmt = 1500;
        decimal taxAmt = 150;
        decimal amt;
        string invType = "INVTYPE";

        List<ReceivableCharge> charges;
        ReceivableInvoiceDtoAdapter invDtoAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            invDtoAdapter = new ReceivableInvoiceDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            // Complex initializations
            invDate = DateTime.Now;
            dueDate = invDate.AddDays(10);
            billStartDate = invDate.AddDays(-10);
            billEndDate = billStartDate.AddDays(100);
            adjBy = new List<string>() { "Adjusted by 3", "Adjusted by 4" };
            amt = baseAmt + taxAmt;
            charges = new List<ReceivableCharge>() {
                new Ellucian.Colleague.Dtos.Finance.ReceivableCharge(){
                    Id = "LINE1",
                    InvoiceId = "1",
                    Description = new List<string>(){"LINE1 description line 1", "LINE1 Description line 2"},
                    Code = "AR1",
                    BaseAmount = 100,
                    TaxAmount = 10,
                    AllocationIds = new List<string>(){"LINE1 Allocation 1", "LINE1 Allocation 2"},
                    PaymentPlanIds = new List<string>(){"LINE1 Payment Plan 1", "LINE1 Payment Plan 2"}
                },
                new ReceivableCharge() {
                    Id = "LINE2",
                    InvoiceId = "1",
                    Description = new List<string>(){"LINE2 description line 1", "LINE2 Description line 2"},
                    Code = "AR2",
                    BaseAmount = 200,
                    TaxAmount = 20,
                    AllocationIds = new List<string>(){"LINE2 Allocation 1", "LINE2 Allocation 2"},
                    PaymentPlanIds = new List<string>(){"LINE2 Payment Plan 1", "LINE2 Payment Plan 2"}
                },
                new ReceivableCharge() {
                    Id = "LINE3",
                    InvoiceId = "1",
                    Description = new List<string>(){"LINE3 description line 1", "LINE3 Description line 2"},
                    Code = "AR3",
                    BaseAmount = 300,
                    TaxAmount = 30,
                    AllocationIds = new List<string>(){"LINE3 Allocation 1", "LINE3 Allocation 2"},
                    PaymentPlanIds = new List<string>(){"LINE3 Payment Plan 1", "LINE3 Payment Plan 2"}
                },
                    new ReceivableCharge() {
                    Id = "LINE4",
                    InvoiceId = "1",
                    Description = new List<string>(){"LINE3 description line 1", "LINE3 Description line 2"},
                    Code = "AR4",
                    BaseAmount = 400,
                    TaxAmount = 40,
                    AllocationIds = null,
                    PaymentPlanIds = null
                },
                    new ReceivableCharge() {
                    Id = "LINE5",
                    InvoiceId = "1",
                    Description = new List<string>(){"LINE3 description line 1", "LINE3 Description line 2"},
                    Code = "AR5",
                    BaseAmount = 500,
                    TaxAmount = 50,
                    AllocationIds = new List<string>(),
                    PaymentPlanIds = new List<string>()
                }
            };


            // Create the ReceivableInvoice DTO
            invDto = new ReceivableInvoice();
            invDto.Id = id;
            invDto.PersonId = personId;
            invDto.ReceivableType = rcvType;
            invDto.TermId = term;
            invDto.ReferenceNumber = refNum;
            invDto.Date = invDate;
            invDto.IsArchived = isArchived;
            invDto.Location = location;
            invDto.ExternalSystem = extSystem;
            invDto.ExternalIdentifier = extId;

            invDto.DueDate = dueDate;
            invDto.BillingStart = billStartDate;
            invDto.BillingEnd = billEndDate;
            invDto.Description = desc;
            invDto.AdjustmentToInvoice = adjTo;
            invDto.AdjustedByInvoices = adjBy;
            invDto.BaseAmount = baseAmt;
            invDto.TaxAmount = taxAmt;
            invDto.Amount = amt;
            invDto.InvoiceType = invType;
            invDto.Charges = charges;

            invEntity = invDtoAdapter.MapToType(invDto);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_Id()
        {
            Assert.AreEqual(id, invEntity.Id);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_PersonId()
        {
            Assert.AreEqual(personId, invEntity.PersonId);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_ReceivableType()
        {
            Assert.AreEqual(rcvType, invEntity.ReceivableType);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_Term()
        {
            Assert.AreEqual(term, invEntity.TermId);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_RefNum()
        {
            Assert.AreEqual(refNum, invEntity.ReferenceNumber);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_Date()
        {
            Assert.AreEqual(invDate, invEntity.Date);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_IsArchived()
        {
            Assert.AreEqual(isArchived, invEntity.IsArchived);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_Location()
        {
            Assert.AreEqual(location, invEntity.Location);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_ExtSystem()
        {
            Assert.AreEqual(extSystem, invEntity.ExternalSystem);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_ExtId()
        {
            Assert.AreEqual(extId, invEntity.ExternalIdentifier);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_DueDate()
        {
            Assert.AreEqual(dueDate, invEntity.DueDate);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_BillingStart()
        {
            Assert.AreEqual(billStartDate, invEntity.BillingStart);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_BillingEnd()
        {
            Assert.AreEqual(billEndDate, invEntity.BillingEnd);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_AdjToInvoice()
        {
            Assert.AreEqual(adjTo, invEntity.AdjustmentToInvoice);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_BaseAmount()
        {
            Assert.AreEqual(baseAmt, invEntity.BaseAmount);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_TaxAmount()
        {
            Assert.AreEqual(taxAmt, invEntity.TaxAmount);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_Amount()
        {
            Assert.AreEqual(amt, invEntity.Amount);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_InvoiceType()
        {
            Assert.AreEqual(invType, invEntity.InvoiceType);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_Desc()
        {
            Assert.AreEqual(desc, invEntity.Description);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_AdjustedByInvoicesCount()
        {
            Assert.AreEqual(adjBy.Count, invEntity.AdjustedByInvoices.Count);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_AdjustedByInvoicesContent()
        {
            for (int i = 0; i < adjBy.Count; i++)
            {
                Assert.AreEqual(adjBy[i], invEntity.AdjustedByInvoices[i]);
            }
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_ChargesCount()
        {
            Assert.AreEqual(charges.Count, invEntity.Charges.Count);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_ChargesContent()
        {
            for (int i = 0; i < charges.Count; i++)
            {
                var dtoCharge = charges[i];
                var entCharge = invEntity.Charges[i];

                // Scalar tests
                Assert.AreEqual(dtoCharge.Id, entCharge.Id);
                Assert.AreEqual(dtoCharge.InvoiceId, entCharge.InvoiceId);
                Assert.AreEqual(dtoCharge.Code, entCharge.Code);
                Assert.AreEqual(dtoCharge.BaseAmount, entCharge.BaseAmount);
                Assert.AreEqual(dtoCharge.TaxAmount, entCharge.TaxAmount);

                // Description tests
                Assert.AreEqual(dtoCharge.Description.Count, entCharge.Description.Count);
                for (int j = 0; j < dtoCharge.Description.Count; j++){
                    Assert.AreEqual(dtoCharge.Description[j], entCharge.Description[j]);
                }

                // Allocation tests
                if (entCharge.AllocationIds != null && entCharge.AllocationIds.Count > 0)
                {
                    Assert.AreEqual(dtoCharge.AllocationIds.Count, entCharge.AllocationIds.Count);
                    for (int j = 0; j < dtoCharge.AllocationIds.Count; j++)
                    {
                        Assert.AreEqual(dtoCharge.AllocationIds[j], entCharge.AllocationIds[j]);
                    }
                }

                //Payment plan tests
                if (entCharge.PaymentPlanIds != null && entCharge.PaymentPlanIds.Count > 0)
                {
                    Assert.AreEqual(dtoCharge.PaymentPlanIds.Count, entCharge.PaymentPlanIds.Count);
                    for (int j = 0; j < dtoCharge.PaymentPlanIds.Count; j++)
                    {
                        Assert.AreEqual(dtoCharge.PaymentPlanIds[j], entCharge.PaymentPlanIds[j]);
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoiceDto2EntityAdapter_NullCharges()
        {
            invDto = new ReceivableInvoice();
            invDto.Id = id;
            invDto.PersonId = personId;
            invDto.ReceivableType = rcvType;
            invDto.TermId = term;
            invDto.ReferenceNumber = refNum;
            invDto.Date = invDate;
            invDto.IsArchived = isArchived;
            invDto.Location = location;
            invDto.ExternalSystem = extSystem;
            invDto.ExternalIdentifier = extId;

            invDto.DueDate = dueDate;
            invDto.BillingStart = billStartDate;
            invDto.BillingEnd = billEndDate;
            invDto.Description = desc;
            invDto.AdjustmentToInvoice = adjTo;
            invDto.AdjustedByInvoices = adjBy;
            invDto.BaseAmount = baseAmt;
            invDto.TaxAmount = taxAmt;
            invDto.Amount = amt;
            invDto.InvoiceType = invType;
            invDto.Charges = null;

            invEntity = invDtoAdapter.MapToType(invDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableInvoiceDto2EntityAdapter_ZeroCharges()
        {
            invDto = new ReceivableInvoice();
            invDto.Id = id;
            invDto.PersonId = personId;
            invDto.ReceivableType = rcvType;
            invDto.TermId = term;
            invDto.ReferenceNumber = refNum;
            invDto.Date = invDate;
            invDto.IsArchived = isArchived;
            invDto.Location = location;
            invDto.ExternalSystem = extSystem;
            invDto.ExternalIdentifier = extId;

            invDto.DueDate = dueDate;
            invDto.BillingStart = billStartDate;
            invDto.BillingEnd = billEndDate;
            invDto.Description = desc;
            invDto.AdjustmentToInvoice = adjTo;
            invDto.AdjustedByInvoices = adjBy;
            invDto.BaseAmount = baseAmt;
            invDto.TaxAmount = taxAmt;
            invDto.Amount = amt;
            invDto.InvoiceType = invType;
            invDto.Charges = new List<ReceivableCharge>();

            invEntity = invDtoAdapter.MapToType(invDto);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_NullAdjustedByInvoices()
        {
            invDto = new ReceivableInvoice();
            invDto.Id = id;
            invDto.PersonId = personId;
            invDto.ReceivableType = rcvType;
            invDto.TermId = term;
            invDto.ReferenceNumber = refNum;
            invDto.Date = invDate;
            invDto.IsArchived = isArchived;
            invDto.Location = location;
            invDto.ExternalSystem = extSystem;
            invDto.ExternalIdentifier = extId;

            invDto.DueDate = dueDate;
            invDto.BillingStart = billStartDate;
            invDto.BillingEnd = billEndDate;
            invDto.Description = desc;
            invDto.AdjustmentToInvoice = adjTo;
            invDto.AdjustedByInvoices = null;
            invDto.BaseAmount = baseAmt;
            invDto.TaxAmount = taxAmt;
            invDto.Amount = amt;
            invDto.InvoiceType = invType;
            invDto.Charges = charges;

            invEntity = invDtoAdapter.MapToType(invDto);
            Assert.AreEqual(0, invEntity.AdjustedByInvoices.Count);
        }

        [TestMethod]
        public void ReceivableInvoiceDto2EntityAdapter_ZeroAdjustedByInvoices()
        {
            invDto = new ReceivableInvoice();
            invDto.Id = id;
            invDto.PersonId = personId;
            invDto.ReceivableType = rcvType;
            invDto.TermId = term;
            invDto.ReferenceNumber = refNum;
            invDto.Date = invDate;
            invDto.IsArchived = isArchived;
            invDto.Location = location;
            invDto.ExternalSystem = extSystem;
            invDto.ExternalIdentifier = extId;

            invDto.DueDate = dueDate;
            invDto.BillingStart = billStartDate;
            invDto.BillingEnd = billEndDate;
            invDto.Description = desc;
            invDto.AdjustmentToInvoice = adjTo;
            invDto.AdjustedByInvoices = new List<string>();
            invDto.BaseAmount = baseAmt;
            invDto.TaxAmount = taxAmt;
            invDto.Amount = amt;
            invDto.InvoiceType = invType;
            invDto.Charges = charges;

            invEntity = invDtoAdapter.MapToType(invDto);
            Assert.AreEqual(0, invEntity.AdjustedByInvoices.Count);
        }
    }
}
