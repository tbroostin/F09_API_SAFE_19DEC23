// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class AccountsPayableInvoicesTests : BaseRepositorySetup
    {
        // this unit tests are very similar in the data of the voucher repository tests
        // So using the already created Mock data that is used there and add to it for testing
        // this accounts payable invoices tests.
        #region Initialize and Cleanup
        ApiSettings apiSettings;

        private AccountsPayableInvoicesRepository accountsPayableInvoicesRepo;
        protected Mock<IColleagueTransactionFactory> transFactoryMock;        
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private TestAccountsPayableInvoicesRepository testVoucherRepository;
        private AccountsPayableInvoices accountsPayableInvoicesEntity;
        private Voucher voucherDomainEntity;
        UpdateVouchersIntegrationResponse response;

        private Vouchers voucherDataContract;
        private Ellucian.Colleague.Data.Base.DataContracts.Person personContract;
        private TxGetHierarchyNameResponse hierarchyNameResponse;
        private Collection<Opers> opersResponse;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
        System.Collections.ObjectModel.Collection<PurchaseOrders> purchaseOrders;
        System.Collections.ObjectModel.Collection<RcVouSchedules> recurringVouchers;
        private RcVouSchedules recurringVoucherDataContract;
        private Collection<Opers> opersDataContracts;
        private Collection<Projects> projectDataContracts;
        private Collection<Items> itemsDataContracts;
        private Collection<ProjectsLineItems> projectLineItemDataContracts;
        List<LineItemTax> Taxes;

        private string personId = "1";
        string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";
        string[] voucherIds = { "1" , "2", "3","4","11","13", "14", "15", "16", "17", "18"
                , "19", "20" ,"21", "22", "23", "24", "25", "26", "27", "29"};
        private int versionNumber;


        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            // Set up a mock transaction invoker for the colleague transaction that gets
            // the GL accounts descriptions for the GL accounts in a project line item.
            dataReaderMock = new Mock<IColleagueDataReader>();

            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            transactionInvoker = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            // Initialize the Voucher repository
            testVoucherRepository = new TestAccountsPayableInvoicesRepository();
            this.voucherDataContract = new Vouchers();
            personContract = new Base.DataContracts.Person();
            Taxes = new List<LineItemTax>();
            apiSettings = new ApiSettings("TEST");
                        
            BuildData();
            accountsPayableInvoicesRepo = new AccountsPayableInvoicesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            versionNumber = 2;
        }

        [TestCleanup]
        public void Cleanup()
        {
            cacheProviderMock = null;
            transFactoryMock = null;
            loggerMock = null;
            dataReaderMock = null;
            hierarchyNameResponse = null;
            transactionInvoker = null;
            accountsPayableInvoicesEntity = null;
            apiSettings = null;
        }
        #endregion

        #region Test Cases

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync()
        {
            {
                string voucherId = "1";
                this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                ConvertDomainEntitiesIntoDataContracts();

                //adding values that are missing in defaults of Vouchers
                this.voucherDataContract.VouDiscAmt = 1.5m;
                this.voucherDataContract.VouManualCashDisc = "Y";
                this.voucherDataContract.VouAddressId = "112233";
                this.voucherDataContract.VouNet = 2.5m;
                this.voucherDataContract.VouPayFlag = "Y";
                this.voucherDataContract.VouReferenceNo = new List<string>() { "Ref123" };
                this.voucherDataContract.VouStatus = new List<string>() { "O" };

                this.voucherDataContract.VouStatusDate = new List<DateTime?>() { new DateTime(2013, 1, 18) };
                this.voucherDataContract.VouVendorTerms = "SaTerm";
                this.voucherDataContract.VouVoidGlTranDate = new DateTime(2013, 1, 19);

                VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
                this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);

                // this.voucherDataContract.VouItemsId = null;

                dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
                string[] itemsId = { "1", "2" };
                string[] prjtsId = { "10", "11", "11" };
                string[] prjtsLineId = { "50", "60", "60" };

                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>(itemsId, true)).ReturnsAsync(this.itemsDataContracts);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Projects>(prjtsId, true)).ReturnsAsync(this.projectDataContracts);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(prjtsLineId, true)).ReturnsAsync(this.projectLineItemDataContracts);

                var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrder);

                var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);

                // Confirm that the SV properties for the voucher are the same
                Assert.IsNotNull(accountsPayable);
                Assert.AreEqual(this.voucherDomainEntity.Amount, accountsPayable.VoucherInvoiceAmt);
                //Assert.AreEqual(this.voucherDomainEntity.Approvers.Count(), accountsPayable.Approvers.Count());
                //for(int i= 0; i < accountsPayable.Approvers.Count(); i++)
                //{
                //    var apApprover = accountsPayable.Approvers[i];
                //    var vdeApprover = this.voucherDomainEntity.Approvers[i];
                //    Assert.AreEqual(apApprover.ApprovalDate, vdeApprover.ApprovalDate);
                //    Assert.AreEqual(apApprover.ApprovalName, vdeApprover.ApprovalName);
                //    Assert.AreEqual(apApprover.ApproverId, vdeApprover.ApproverId);
                //}
                Assert.AreEqual(this.voucherDomainEntity.ApType, accountsPayable.ApType);
                Assert.AreEqual(this.voucherDomainEntity.CheckDate, accountsPayable.CheckDate);
                Assert.AreEqual(this.voucherDomainEntity.CheckNumber, accountsPayable.CheckNumber);
                Assert.AreEqual(this.voucherDomainEntity.Comments, accountsPayable.Comments);
                Assert.AreEqual(this.voucherDomainEntity.CurrencyCode, accountsPayable.CurrencyCode);
                Assert.AreEqual(this.voucherDomainEntity.Date, accountsPayable.Date);
                Assert.AreEqual(this.voucherDomainEntity.DueDate, accountsPayable.DueDate);
                Assert.AreEqual(guid, accountsPayable.Guid);
                Assert.AreEqual(this.voucherDomainEntity.Id, accountsPayable.Id);
                Assert.AreEqual(this.voucherDomainEntity.InvoiceDate, accountsPayable.InvoiceDate);
                Assert.AreEqual(this.voucherDomainEntity.InvoiceNumber, accountsPayable.InvoiceNumber);
                //  Assert.AreEqual(this.voucherDomainEntity.LineItems.Count(), accountsPayable.LineItems.Count());
                Assert.AreEqual(this.voucherDomainEntity.MaintenanceDate, accountsPayable.MaintenanceDate);

                Assert.AreEqual(this.voucherDomainEntity.Status, accountsPayable.Status);
                Assert.AreEqual("0123", accountsPayable.VendorAddressId);
                Assert.AreEqual(this.voucherDomainEntity.VendorId, accountsPayable.VendorId);
                // Assert.AreEqual(this.voucherDomainEntity.VendorName, accountsPayable.VendorName);
                Assert.AreEqual(1.5m, accountsPayable.VoucherDiscAmt);
                Assert.AreEqual("112233", accountsPayable.VoucherAddressId);
                Assert.AreEqual(2.5m, accountsPayable.VoucherNet);
                Assert.AreEqual("Y", accountsPayable.VoucherPayFlag);
                Assert.AreEqual(this.voucherDataContract.VouReferenceNo.Count(), accountsPayable.VoucherReferenceNo.Count());
                for (int i = 0; i < accountsPayable.VoucherReferenceNo.Count(); i++)
                {
                    Assert.AreEqual(this.voucherDataContract.VouReferenceNo[i], accountsPayable.VoucherReferenceNo[i]);
                }
                Assert.AreEqual(this.voucherDataContract.VouStatusDate[0], accountsPayable.VoucherStatusDate);
                Assert.AreEqual(this.voucherDataContract.VouTaxesEntityAssociation.Count(), accountsPayable.VoucherTaxes.Count());
                for (int i = 0; i < accountsPayable.VoucherTaxes.Count(); i++)
                {
                    var vdcTaxes = this.voucherDataContract.VouTaxesEntityAssociation[i];
                    var apTaxes = accountsPayable.VoucherTaxes[i];
                    Assert.AreEqual(vdcTaxes.VouTaxAmtsAssocMember, apTaxes.TaxAmount);
                    Assert.AreEqual(vdcTaxes.VouTaxCodesAssocMember, apTaxes.TaxCode);
                }
                Assert.AreEqual(this.voucherDataContract.VouVendorTerms, accountsPayable.VoucherVendorTerms);
                Assert.AreEqual(this.voucherDataContract.VouVoidGlTranDate, accountsPayable.VoucherVoidGlTranDate);

                Assert.AreEqual(this.itemsDataContracts.Count(), accountsPayable.LineItems.Count());
                for (int x = 0; x < accountsPayable.LineItems.Count(); x++)
                {
                    var lineItem = accountsPayable.LineItems[x];
                    var dtoItem = this.itemsDataContracts[x];

                    Assert.AreEqual(dtoItem.ItmVouQty, lineItem.Quantity);
                    Assert.AreEqual(dtoItem.ItmVouPrice, lineItem.Price);
                    Assert.AreEqual(dtoItem.ItmVouExtPrice, lineItem.ExtendedPrice);
                    Assert.AreEqual(dtoItem.Recordkey, lineItem.Id);
                    Assert.AreEqual(dtoItem.ItmVouIssue, lineItem.UnitOfIssue);
                    Assert.AreEqual(dtoItem.ItmInvoiceNo, lineItem.InvoiceNumber);
                    Assert.AreEqual(dtoItem.ItmTaxForm, lineItem.TaxForm);
                    Assert.AreEqual(dtoItem.ItmTaxFormCode, lineItem.TaxFormCode);
                    Assert.AreEqual(dtoItem.ItmTaxFormLoc, lineItem.TaxFormLocation);
                    Assert.AreEqual(dtoItem.ItmCommodityCode, lineItem.CommodityCode);
                    Assert.AreEqual(dtoItem.ItmFixedAssetsFlag, lineItem.FixedAssetsFlag);
                    Assert.AreEqual(dtoItem.ItmVouCashDiscAmt, lineItem.CashDiscountAmount);
                    Assert.AreEqual(dtoItem.ItmVouTradeDiscAmt, lineItem.TradeDiscountAmount);
                    Assert.AreEqual(dtoItem.ItmVouTradeDiscPct, lineItem.TradeDiscountPercent);

                    Assert.AreEqual(dtoItem.VouchGlEntityAssociation.Count(), lineItem.GlDistributions.Count());
                    for (int i = 0; i < lineItem.GlDistributions.Count(); i++)
                    {
                        var glDist = lineItem.GlDistributions[i];
                        var dtoGlDist = dtoItem.VouchGlEntityAssociation[i];

                        Assert.AreEqual(dtoGlDist.ItmVouGlNoAssocMember, glDist.GlAccountNumber);
                        Assert.AreEqual(dtoGlDist.ItmVouGlQtyAssocMember, glDist.Quantity);
                        Assert.AreEqual(dtoGlDist.ItmVouGlAmtAssocMember, glDist.Amount);
                        if (dtoGlDist.ItmVouGlPctAssocMember != null)
                        {
                            Assert.AreEqual(dtoGlDist.ItmVouGlPctAssocMember, glDist.Percent);
                        }
                        Assert.AreEqual(dtoGlDist.ItmVouProjectCfIdAssocMember, glDist.ProjectId);
                        Assert.AreEqual(dtoGlDist.ItmVouPrjItemIdsAssocMember, glDist.ProjectLineItemId);
                    }

                    Assert.AreEqual(dtoItem.VouGlTaxesEntityAssociation.Count(), lineItem.AccountsPayableLineItemTaxes.Count());
                    for (int i = 0; i < lineItem.AccountsPayableLineItemTaxes.Count(); i++)
                    {
                        var entitytax = lineItem.AccountsPayableLineItemTaxes[i];
                        var dtoTax = dtoItem.VouGlTaxesEntityAssociation[i];

                        if (dtoTax.ItmVouGlForeignTaxAmtAssocMember.HasValue)
                        {
                            Assert.AreEqual(dtoTax.ItmVouGlForeignTaxAmtAssocMember, entitytax.TaxAmount);
                        }
                        else
                        {
                            Assert.AreEqual(dtoTax.ItmVouGlTaxAmtAssocMember, entitytax.TaxAmount);
                        }

                        Assert.AreEqual(dtoTax.ItmVouTaxGlNoAssocMember, entitytax.TaxGlNumber);
                        Assert.AreEqual(dtoTax.ItmVouGlTaxCodeAssocMember, entitytax.TaxCode);
                    }

                }
            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync()
        {
            {

                List<string> rcVouSchedulesIds = new List<string>();

                Collection<DataContracts.Vouchers> dataContractVouchers = new Collection<DataContracts.Vouchers>();
                RcVouSchedules rcVouSched = new RcVouSchedules();

                foreach (var voucherId in voucherIds)
                {
                    this.voucherDataContract = new DataContracts.Vouchers();
                    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                    ConvertDomainEntitiesIntoDataContracts();
                    VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
                    this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);
                    this.voucherDataContract.VouMiscName = new List<string>();
                    if (string.IsNullOrEmpty(this.voucherDataContract.VouVendor))
                    {
                        this.voucherDataContract.VouMiscName.Add("Test misc Name");
                    }

                    if (!string.IsNullOrEmpty(this.voucherDataContract.VouRcvsId))
                    {
                        string vouRcvsId = this.voucherDataContract.VouRcvsId;
                        rcVouSchedulesIds.Add(vouRcvsId);
                        rcVouSched = new RcVouSchedules() { Recordkey = vouRcvsId, RcvsRcVoucher = "RV000001" };
                    }
                    dataContractVouchers.Add(this.voucherDataContract);
                }
                dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).ReturnsAsync(rcVouSched);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(voucherIds);

                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", voucherIds, true)).ReturnsAsync(dataContractVouchers);
                string[] personIds = { "0001234", "0000002" };
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", personIds, true)).ReturnsAsync(people);

                var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrder);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>("ITEMS", It.IsAny<string[]>(), true)).ReturnsAsync(this.itemsDataContracts);

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 21,
                    CacheName = "AllAccountsPayableInvoices",
                    Entity = "VOUCHERS",
                    Sublist = voucherIds.ToList(),
                    TotalCount = 21,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, It.IsAny<string>());

                Assert.AreEqual(dataContractVouchers.Count(), accountsPayables.Item1.Count());

                foreach (var accountsPayable in accountsPayables.Item1)
                {

                    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(accountsPayable.Id, personId, GlAccessLevel.Full_Access, null, versionNumber);
                    var apDataContract = dataContractVouchers.FirstOrDefault(a => a.Recordkey == accountsPayable.Id);

                    Assert.IsNotNull(accountsPayable);
                    Assert.AreEqual(apDataContract.VouInvoiceAmt, accountsPayable.VoucherInvoiceAmt);
                    //Assert.AreEqual(this.voucherDomainEntity.Approvers.Count(), accountsPayable.Approvers.Count(), accountsPayable.Id);
                    //for (int i = 0; i < accountsPayable.Approvers.Count(); i++)
                    //{
                    //    var apApprover = accountsPayable.Approvers[i];
                    //    var vdeApprover = this.voucherDomainEntity.Approvers[i];
                    //    Assert.AreEqual(apApprover.ApprovalDate, vdeApprover.ApprovalDate);
                    //    Assert.AreEqual(apApprover.ApprovalName, vdeApprover.ApprovalName);
                    //    Assert.AreEqual(apApprover.ApproverId, vdeApprover.ApproverId);
                    //}
                    Assert.AreEqual(this.voucherDomainEntity.ApType, accountsPayable.ApType);

                    Assert.AreEqual(this.voucherDomainEntity.CheckDate, accountsPayable.CheckDate);

                    Assert.AreEqual(this.voucherDomainEntity.Comments, accountsPayable.Comments);
                    Assert.AreEqual(this.voucherDomainEntity.CurrencyCode, accountsPayable.CurrencyCode);
                    Assert.AreEqual(this.voucherDomainEntity.Date, accountsPayable.Date);
                    Assert.AreEqual(this.voucherDomainEntity.DueDate, accountsPayable.DueDate);
                    Assert.AreEqual(this.voucherDomainEntity.Id, accountsPayable.Id);
                    Assert.AreEqual(this.voucherDomainEntity.InvoiceDate, accountsPayable.InvoiceDate);
                    Assert.AreEqual(this.voucherDomainEntity.InvoiceNumber, accountsPayable.InvoiceNumber);
                    Assert.AreEqual(this.voucherDomainEntity.MaintenanceDate, accountsPayable.MaintenanceDate);

                    Assert.AreEqual(this.voucherDomainEntity.Status, accountsPayable.Status);
                    var vendorID = (string.IsNullOrEmpty(this.voucherDomainEntity.VendorId) ? null : this.voucherDomainEntity.VendorId);
                    Assert.AreEqual(vendorID, accountsPayable.VendorId);
                    //if (apDataContract.VouMiscName != null && apDataContract.VouMiscName.Count() > 0) {
                    //    Assert.AreEqual("Test misc Name", accountsPayable.VendorName);
                    //} else
                    //{
                    //    Assert.AreEqual("Vendor name for use in a colleague transaction", accountsPayable.VendorName);
                    //}

                }

            }
        }

        //[TestMethod]
        //public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_testOffset()
        //{
        //    {

        //        List<string> rcVouSchedulesIds = new List<string>();

        //        Collection<DataContracts.Vouchers> dataContractVouchers = new Collection<DataContracts.Vouchers>();
        //        RcVouSchedules rcVouSched = new RcVouSchedules();


        //        for (int i = 2; i < 3; i++)
        //        {
        //            var voucherId = voucherIds[i];
        //            this.voucherDataContract = new DataContracts.Vouchers();
        //            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
        //            ConvertDomainEntitiesIntoDataContracts();
        //            VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
        //            this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);
        //            this.voucherDataContract.VouMiscName = new List<string>();
        //            if (string.IsNullOrEmpty(this.voucherDataContract.VouVendor))
        //            {
        //                this.voucherDataContract.VouMiscName.Add("Test misc Name");
        //            }

        //            if (!string.IsNullOrEmpty(this.voucherDataContract.VouRcvsId))
        //            {
        //                string vouRcvsId = this.voucherDataContract.VouRcvsId;
        //                rcVouSchedulesIds.Add(vouRcvsId);
        //                rcVouSched = new RcVouSchedules() { Recordkey = vouRcvsId, RcvsRcVoucher = "RV000001" };
        //            }

        //            dataContractVouchers.Add(this.voucherDataContract);
        //        }

        //        dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

        //        dataReaderMock.Setup(repo => repo.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).ReturnsAsync(rcVouSched);
        //        dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
        //        dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(voucherIds);
        //        dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(voucherIds);
        //        dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), true)).ReturnsAsync(dataContractVouchers);
        //        people = new Collection<Base.DataContracts.Person>()
        //    {
        //        new Base.DataContracts.Person(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "0001234", PersonCorpIndicator = "Y"}
        //    };
        //        dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);
        //        var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
        //        dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrder);

        //        var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesAsync(2, 1);

        //        Assert.AreEqual("3", accountsPayables.Item1.FirstOrDefault().Id);

        //    }
        //}

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_testLimit()
        {
            {

                List<string> rcVouSchedulesIds = new List<string>();

                Collection<DataContracts.Vouchers> dataContractVouchers = new Collection<DataContracts.Vouchers>();
                RcVouSchedules rcVouSched = new RcVouSchedules();


                for (int i = 0; i < 3; i++)
                {
                    var voucherId = voucherIds[i];
                    this.voucherDataContract = new DataContracts.Vouchers();
                    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                    ConvertDomainEntitiesIntoDataContracts();
                    VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
                    this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);
                    this.voucherDataContract.VouMiscName = new List<string>();
                    if (string.IsNullOrEmpty(this.voucherDataContract.VouVendor))
                    {
                        this.voucherDataContract.VouMiscName.Add("Test misc Name");
                    }

                    if (!string.IsNullOrEmpty(this.voucherDataContract.VouRcvsId))
                    {
                        string vouRcvsId = this.voucherDataContract.VouRcvsId;
                        rcVouSchedulesIds.Add(vouRcvsId);
                        rcVouSched = new RcVouSchedules() { Recordkey = vouRcvsId, RcvsRcVoucher = "RV000001" };
                    }

                    dataContractVouchers.Add(this.voucherDataContract);
                }

                dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

                dataReaderMock.Setup(repo => repo.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).ReturnsAsync(rcVouSched);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), true)).ReturnsAsync(dataContractVouchers);

                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);
                var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrder);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>("ITEMS", It.IsAny<string[]>(), true)).ReturnsAsync(this.itemsDataContracts);

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 3,
                    CacheName = "AllAccountsPayableInvoices",
                    Entity = "VOUCHERS",
                    Sublist = voucherIds.ToList(),
                    TotalCount = 21,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 3, It.IsAny<string>());

                Assert.AreEqual(3, accountsPayables.Item1.Count());

            }
        }


        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_BulkReadNullRecord()
        {
            {
                dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });


                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), true)).ReturnsAsync(() => null);

                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);
                var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrder);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>("ITEMS", It.IsAny<string[]>(), true)).ReturnsAsync(this.itemsDataContracts);

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 3,
                    CacheName = "AllAccountsPayableInvoices",
                    Entity = "VOUCHERS",
                    Sublist = voucherIds.ToList(),
                    TotalCount = 21,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 3, It.IsAny<string>());

            }
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_BulkReadPONullRecord()
        {
            {
                dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
                Collection<DataContracts.Vouchers> dataContractVouchers = new Collection<DataContracts.Vouchers>();
                for (int i = 0; i < 3; i++)
                {
                    var voucherId = voucherIds[i];
                    this.voucherDataContract = new DataContracts.Vouchers();
                    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                    ConvertDomainEntitiesIntoDataContracts();
                    VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
                    this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);
                    this.voucherDataContract.VouMiscName = new List<string>();
                    if (string.IsNullOrEmpty(this.voucherDataContract.VouVendor))
                    {
                        this.voucherDataContract.VouMiscName.Add("Test misc Name");
                    }
                    this.voucherDataContract.VouPoNo = "123";
                    dataContractVouchers.Add(this.voucherDataContract);
                }


                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), true)).ReturnsAsync(dataContractVouchers);

                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);
                var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(() => null);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>("ITEMS", It.IsAny<string[]>(), true)).ReturnsAsync(this.itemsDataContracts);

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 3,
                    CacheName = "AllAccountsPayableInvoices",
                    Entity = "VOUCHERS",
                    Sublist = voucherIds.ToList(),
                    TotalCount = 21,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 3, It.IsAny<string>());

            }
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_BulkReadRecNullRecord()
        {
            {
                dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
                Collection<DataContracts.Vouchers> dataContractVouchers = new Collection<DataContracts.Vouchers>();
                for (int i = 0; i < 3; i++)
                {
                    var voucherId = voucherIds[i];
                    this.voucherDataContract = new DataContracts.Vouchers();
                    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                    ConvertDomainEntitiesIntoDataContracts();
                    VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
                    this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);
                    this.voucherDataContract.VouMiscName = new List<string>();
                    if (string.IsNullOrEmpty(this.voucherDataContract.VouVendor))
                    {
                        this.voucherDataContract.VouMiscName.Add("Test misc Name");
                    }
                    this.voucherDataContract.VouPoNo = string.Empty;
                    this.voucherDataContract.VouRcvsId = "123";
                    dataContractVouchers.Add(this.voucherDataContract);
                }


                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), true)).ReturnsAsync(dataContractVouchers);

                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);
                var purchaseOrder = new RcVouSchedules() { Recordkey = "0000002", RcvsRcVoucher = "R0002" };
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<RcVouSchedules>("RC.VOU.SCHEDULES", It.IsAny<string[]>(), true)).ReturnsAsync(() => null);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>("ITEMS", It.IsAny<string[]>(), true)).ReturnsAsync(this.itemsDataContracts);

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 3,
                    CacheName = "AllAccountsPayableInvoices",
                    Entity = "VOUCHERS",
                    Sublist = voucherIds.ToList(),
                    TotalCount = 21,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 3, It.IsAny<string>());

            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_InvoiceNumber_Filter()
        {
            {

                List<string> rcVouSchedulesIds = new List<string>();
                string[] itemsId = { "1", "2" };
                Collection<DataContracts.Vouchers> dataContractVouchers = new Collection<DataContracts.Vouchers>();
                RcVouSchedules rcVouSched = new RcVouSchedules();


                for (int i = 0; i < 3; i++)
                {
                    var voucherId = voucherIds[i];
                    this.voucherDataContract = new DataContracts.Vouchers();
                    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                    ConvertDomainEntitiesIntoDataContracts();
                    VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
                    this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);
                    this.voucherDataContract.VouMiscName = new List<string>();
                    if (string.IsNullOrEmpty(this.voucherDataContract.VouVendor))
                    {
                        this.voucherDataContract.VouMiscName.Add("Test misc Name");
                    }

                    if (!string.IsNullOrEmpty(this.voucherDataContract.VouRcvsId))
                    {
                        string vouRcvsId = this.voucherDataContract.VouRcvsId;
                        rcVouSchedulesIds.Add(vouRcvsId);
                        rcVouSched = new RcVouSchedules() { Recordkey = vouRcvsId, RcvsRcVoucher = "RV000001" };
                    }

                    dataContractVouchers.Add(this.voucherDataContract);
                }

                dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

                dataReaderMock.Setup(repo => repo.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).ReturnsAsync(rcVouSched);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(voucherIds);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), true)).ReturnsAsync(dataContractVouchers);
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>("1", true)).ReturnsAsync(dataContractVouchers.FirstOrDefault(x=>x.Recordkey == "1"));

                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);
                var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrder);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>("ITEMS", It.IsAny<string[]>(), true)).ReturnsAsync(this.itemsDataContracts);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>(itemsId, true)).ReturnsAsync(this.itemsDataContracts);

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 3,
                    CacheName = "AllAccountsPayableInvoices",
                    Entity = "VOUCHERS",
                    Sublist = voucherIds.ToList(),
                    TotalCount = 21,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 3, "1");

                Assert.AreEqual("1", accountsPayables.Item1.FirstOrDefault().Id);

            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Invoice_FilterVouchers_Null()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>("1", true)).ReturnsAsync(() => null);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, "1");
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Invoice_FilterVouchers_StatusX()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouStatus = new List<string> { "X" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, "1");
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Invoice_FilterVouchers_StatusU()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouStatus = new List<string> { "U" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, "1");
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }


        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Invoice_FilterVouchers_noLineItem()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouItemsId = null;
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, "1");
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Invoice_FilterVouchers_InvalidAPType()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouApType = "123";
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, "1");
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Invoice_FilterVouchers_Null_Person()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouStatus = new List<string> { "O" };
            voucherDataContract.VouVendor = "123";
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(() => null);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, "1");
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Invoice_FilterVouchers_InvalidPO()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouPoNo = "123";
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(() => null);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, "1");
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Invoice_FilterVouchers_InvalidRecVou()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouRcvsId = "123";
            voucherDataContract.VouPoNo = string.Empty;
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).ReturnsAsync(() => null);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, "1");
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_Vouchers_Null()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), true)).ReturnsAsync(() => null);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, It.IsAny<string>());
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_NoApType()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[0]);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), true)).ReturnsAsync(() => null);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(people);

            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(() => null);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, It.IsAny<string>());
            Assert.AreEqual(0, accountsPayables.Item1.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_Guid_Null()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(() => null);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(null, false);
        }
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_GUID_RecordMissing()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(() => null);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_GUID_RecordNotVoucher()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "PERSONS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_PersonsRec_Is_Null()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouStatus = new List<string> { "O" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(() => null);
            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_ReadRecord_Null()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouStatus = new List<string> { "O" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(() => null);
            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_Status_X()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouStatus = new List<string> { "X" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_Status_U()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            voucherDataContract.VouStatus = new List<string> { "U" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }


        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesAsync_PersonsRecs_Is_Null()
        {
            List<string> rcVouSchedulesIds = new List<string>();

            Collection<DataContracts.Vouchers> dataContractVouchers = new Collection<DataContracts.Vouchers>();
            RcVouSchedules rcVouSched = new RcVouSchedules();


            foreach (var voucherId in voucherIds)
            {
                this.voucherDataContract = new DataContracts.Vouchers();
                this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                ConvertDomainEntitiesIntoDataContracts();
                VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
                this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);
                this.voucherDataContract.VouMiscName = new List<string>();
                if (string.IsNullOrEmpty(this.voucherDataContract.VouVendor))
                {
                    this.voucherDataContract.VouMiscName.Add("Test misc Name");
                }

                if (!string.IsNullOrEmpty(this.voucherDataContract.VouRcvsId))
                {
                    string vouRcvsId = this.voucherDataContract.VouRcvsId;
                    rcVouSchedulesIds.Add(vouRcvsId);
                    rcVouSched = new RcVouSchedules() { Recordkey = vouRcvsId, RcvsRcVoucher = "RV000001" };
                }

                dataContractVouchers.Add(this.voucherDataContract);
            }

            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).ReturnsAsync(rcVouSched);
            dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(voucherIds);
            dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(voucherIds);
            dataReaderMock.Setup(repo => repo.SelectAsync("VOUCHERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(voucherIds);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", voucherIds, true)).ReturnsAsync(dataContractVouchers);
            string[] personIds = { "0001234", "0000002" };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", personIds, true)).ReturnsAsync(() => null);


            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 21,
                CacheName = "AllAccountsPayableInvoices",
                Entity = "VOUCHERS",
                Sublist = voucherIds.ToList(),
                TotalCount = 21,
                KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
            };
            transactionInvoker.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .ReturnsAsync(resp);

            var accountsPayables = await accountsPayableInvoicesRepo.GetAccountsPayableInvoices2Async(0, 100, It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_VoucherStatus_isNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();

            voucherDataContract.VoucherStatusEntityAssociation = null;
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_VoucherStatus_ContainsInvalidStatus()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();

            voucherDataContract.VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>()
            { new VouchersVoucherStatus() {VouStatusAssocMember = "C", VouStatusDateAssocMember = new DateTime(2017,1,11) } };

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_VoucherStatus_ContainsInProgressStatus()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();

            voucherDataContract.VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>()
            { new VouchersVoucherStatus() {VouStatusAssocMember = "U", VouStatusDateAssocMember = new DateTime(2017,1,11) } };

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_VoucherStatus_Contains_Cancelled()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();

            voucherDataContract.VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>()
            { new VouchersVoucherStatus() {VouStatusAssocMember = "X", VouStatusDateAssocMember = new DateTime(2017,1,11) } };

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }


        [TestMethod]
          public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_NameMissing()
        {
            string voucherId = "1";
            string[] itemsId = { "1", "2" };
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            purchaseOrders = new Collection<PurchaseOrders>()
            {
                new PurchaseOrders(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "12",PoNo = "0001", PoIntgType = "procurement"},
                new PurchaseOrders(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "0000002", PoNo = "0002"}
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrders);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrders.FirstOrDefault());
            voucherDataContract.VouMiscName = null;
            voucherDataContract.VouVendor = null;
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>(itemsId, true)).ReturnsAsync(this.itemsDataContracts);

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_MissingDate()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            purchaseOrders = new Collection<PurchaseOrders>()
            {
                new PurchaseOrders(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "12",PoNo = "0001", PoIntgType = "procurement"},
                new PurchaseOrders(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "0000002", PoNo = "0002"}
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrders);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(),true)).ReturnsAsync(purchaseOrders.FirstOrDefault());
            voucherDataContract.VouDate = null;
            voucherDataContract.VouStatus = new List<string> { "O" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_MissingInvoiceDate()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            purchaseOrders = new Collection<PurchaseOrders>()
            {
                new PurchaseOrders(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "12",PoNo = "0001", PoIntgType = "procurement"},
                new PurchaseOrders(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "0000002", PoNo = "0002"}
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrders);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrders.FirstOrDefault());
            voucherDataContract.VouDefaultInvoiceDate = null;
            voucherDataContract.VouDate = null;
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoices_GetAccountsPayableInvoicesByGuidAsync_MissingAPType()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();

            voucherDataContract.VouApType = null;
            voucherDataContract.VouStatus = new List<string> { "O" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });

            var accountsPayable = await accountsPayableInvoicesRepo.GetAccountsPayableInvoicesByGuidAsync(guid, false);
        }

        [TestMethod]
        public async Task AccountsPayableInvoices_CreateAccountsPayableInvoicesAsync()
        {
            string[] itemsId = { "1", "2" };
            string[] prjtsId = { "10", "11", "11" };
            string[] prjtsLineId = { "50", "60", "60" };

            accountsPayableInvoicesEntity = new AccountsPayableInvoices(new Guid().ToString(), "1", DateTime.Today, VoucherStatus.InProgress, "ABC", "1", DateTime.Today.Date.AddDays(-5));
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();

            //adding values that are missing in defaults of Vouchers
            this.voucherDataContract.VouDiscAmt = 1.5m;
            this.voucherDataContract.VouAddressId = "112233";
            this.voucherDataContract.VouNet = 2.5m;
            this.voucherDataContract.VouPayFlag = "Y";
            this.voucherDataContract.VouReferenceNo = new List<string>() { "Ref123" };
            this.voucherDataContract.VouStatus = new List<string>() { "O" };
            this.voucherDataContract.VouIntgSubmittedBy = "1";

            this.voucherDataContract.VouStatusDate = new List<DateTime?>() { new DateTime(2013, 1, 18) };
            this.voucherDataContract.VouVendorTerms = "SaTerm";
            this.voucherDataContract.VouVoidGlTranDate = new DateTime(2013, 1, 19);

            VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
            this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);

            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>(itemsId, true)).ReturnsAsync(this.itemsDataContracts);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Projects>(prjtsId, true)).ReturnsAsync(this.projectDataContracts);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(prjtsLineId, true)).ReturnsAsync(this.projectLineItemDataContracts);
            var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrder);

            transactionInvoker.Setup(i => i.ExecuteAsync<UpdateVouchersIntegrationRequest, UpdateVouchersIntegrationResponse>(It.IsAny<UpdateVouchersIntegrationRequest>()))
                .ReturnsAsync(response);

            var result = await accountsPayableInvoicesRepo.CreateAccountsPayableInvoicesAsync(accountsPayableInvoicesEntity);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AccountsPayableInvoices_UpdateAccountsPayableInvoicesAsync()
        {
            string[] itemsId = { "1", "2" };
            string[] prjtsId = { "10", "11", "11" };
            string[] prjtsLineId = { "50", "60", "60" };

            accountsPayableInvoicesEntity = new AccountsPayableInvoices(new Guid().ToString(), "1", DateTime.Today, VoucherStatus.InProgress, "ABC", "1", DateTime.Today.Date.AddDays(-5));
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();

            //adding values that are missing in defaults of Vouchers
            this.voucherDataContract.VouDiscAmt = 1.5m;
            this.voucherDataContract.VouAddressId = "112233";
            this.voucherDataContract.VouNet = 2.5m;
            this.voucherDataContract.VouPayFlag = "Y";
            this.voucherDataContract.VouReferenceNo = new List<string>() { "Ref123" };
            this.voucherDataContract.VouStatus = new List<string>() { "O" };

            this.voucherDataContract.VouStatusDate = new List<DateTime?>() { new DateTime(2013, 1, 18) };
            this.voucherDataContract.VouVendorTerms = "SaTerm";
            this.voucherDataContract.VouVoidGlTranDate = new DateTime(2013, 1, 19);

            VouchersVouTaxes tax = new VouchersVouTaxes() { VouTaxAmtsAssocMember = Taxes[0].TaxAmount, VouTaxCodesAssocMember = Taxes[0].TaxCode };
            this.voucherDataContract.VouTaxesEntityAssociation.Add(tax);

            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucherDataContract);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>(itemsId, true)).ReturnsAsync(this.itemsDataContracts);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Projects>(prjtsId, true)).ReturnsAsync(this.projectDataContracts);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(prjtsLineId, true)).ReturnsAsync(this.projectLineItemDataContracts);
            var purchaseOrder = new PurchaseOrders() { PoNo = "P0000001" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrder);

            transactionInvoker.Setup(i => i.ExecuteAsync<UpdateVouchersIntegrationRequest, UpdateVouchersIntegrationResponse>(It.IsAny<UpdateVouchersIntegrationRequest>()))
                .ReturnsAsync(response);

            var result = await accountsPayableInvoicesRepo.UpdateAccountsPayableInvoicesAsync(accountsPayableInvoicesEntity);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountsPayableInvoices_CreateAccountsPayableInvoicesAsync_ArgumentNullException()
        {
            await accountsPayableInvoicesRepo.CreateAccountsPayableInvoicesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountsPayableInvoices_UpdateAccountsPayableInvoicesAsync_ArgumentNullException()
        {
            await accountsPayableInvoicesRepo.UpdateAccountsPayableInvoicesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountsPayableInvoices_UpdateAccountsPayableInvoicesAsync_Guid_Null_ArgumentNullException()
        {
            await accountsPayableInvoicesRepo.UpdateAccountsPayableInvoicesAsync(new AccountsPayableInvoices("1", DateTime.Today, VoucherStatus.InProgress, "ABC", "1", DateTime.Today.Date.AddDays(-5)));
        }

        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task AccountsPayableInvoices_CreateAccountsPayableInvoicesAsync_RepositoryException()
        //{
        //    accountsPayableInvoicesEntity = new AccountsPayableInvoices(new Guid().ToString(), "1", DateTime.Today, VoucherStatus.InProgress, "ABC", "1", DateTime.Today.Date.AddDays(-5));

        //    response.ErrorMessages = new List<string>() { "Error occured in CTX" };
        //    transactionInvoker.Setup(i => i.ExecuteAsync<UpdateVouchersIntegrationRequest, UpdateVouchersIntegrationResponse>(It.IsAny<UpdateVouchersIntegrationRequest>()))
        //        .ReturnsAsync(response);

        //    var result = await accountsPayableInvoicesRepo.CreateAccountsPayableInvoicesAsync(accountsPayableInvoicesEntity);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task AccountsPayableInvoices_UpdateAccountsPayableInvoicesAsync_RepositoryException()
        //{
        //    accountsPayableInvoicesEntity = new AccountsPayableInvoices(guid, "1", DateTime.Today, VoucherStatus.InProgress, "ABC", "1", DateTime.Today.Date.AddDays(-5));

        //    response.ErrorMessages = new List<string>() { "Error occured in CTX" };
        //    transactionInvoker.Setup(i => i.ExecuteAsync<UpdateVouchersIntegrationRequest, UpdateVouchersIntegrationResponse>(It.IsAny<UpdateVouchersIntegrationRequest>()))
        //        .ReturnsAsync(response);

        //    var result = await accountsPayableInvoicesRepo.UpdateAccountsPayableInvoicesAsync(accountsPayableInvoicesEntity);
        //}
        #endregion

        #region Private methods

        private void BuildData()
        {
            // Mock ReadRecord to return a pre-defined Vouchers data contract
            dataReaderMock.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.voucherDataContract);
            });

            GuidLookupResult result = new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            people = new Collection<Base.DataContracts.Person>()
            {
                new Base.DataContracts.Person(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "0001234", PersonCorpIndicator = "Y"},
                new Base.DataContracts.Person(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "0000002", PersonCorpIndicator = "N"}
            };

            purchaseOrders = new Collection<PurchaseOrders>()
            {
                new PurchaseOrders(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "12",PoNo = "0001", PoIntgType = "procurement"},
                new PurchaseOrders(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "0000002", PoNo = "0002"}
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrders);
            recurringVouchers = new Collection<RcVouSchedules>()
            {
                new RcVouSchedules(){Recordkey = "65", RcvsRcVoucher = "R0001"},
                new RcVouSchedules(){ Recordkey = "0000002",  RcvsRcVoucher = "R0002"}
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<RcVouSchedules>("RC.VOU.SCHEDULES", It.IsAny<string[]>(), true)).ReturnsAsync(recurringVouchers);
            var expectedPerson = people.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            expectedPerson.PreferredAddress = "0123";

            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            // dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(It.IsAny<string>(), true)).ReturnsAsync(expectedPerson);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(expectedPerson);
            // Mock bulk read UT.OPERS bulk read
            opersResponse = new Collection<Opers>()
                {
                    new Opers()
                    {
                        // "0000001"
                        Recordkey = "0000001", SysUserName = "Andy Kleehammer"
                    },
                    new Opers()
                    {
                        // ""
                        Recordkey = "0000002", SysUserName = "Gary Thorne"
                    },
                    new Opers()
                    {
                        // "0000003"
                        Recordkey = "0000003", SysUserName = "Teresa Longerbeam"
                    }
                };
            dataReaderMock.Setup<Task<Collection<Opers>>>(acc => acc.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(opersResponse);
            });

            LineItemTax Tax = new LineItemTax("st", 3);
            Taxes.Add(Tax);
            Tax = new LineItemTax("st", 4);
            Taxes.Add(Tax);

            // vendor name comes from CTX
            string vendorName = "Test Name";
            var ctxVendorName = new List<string>() { vendorName };

            this.hierarchyNameResponse = new TxGetHierarchyNameResponse()
            {
                IoPersonId = "0001234",
                OutPersonName = ctxVendorName
            };

            // Mock Execute within the transaction invoker to return a GetGlAccountDescriptionResponse object
            transactionInvoker.Setup(tio => tio.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(It.IsAny<TxGetHierarchyNameRequest>())).Returns(() =>
            {
                return this.hierarchyNameResponse;
            });
            response = new UpdateVouchersIntegrationResponse() { VoucherGuid = guid };
        }

        private void ConvertDomainEntitiesIntoDataContracts()
        {
            // Convert the Voucher object
            this.voucherDataContract.RecordGuid = guid;
            this.voucherDataContract.Recordkey = this.voucherDomainEntity.Id;
            this.voucherDataContract.VouVendor = this.voucherDomainEntity.VendorId;

            if (this.voucherDomainEntity.VendorName == "null")
            {
                this.voucherDataContract.VouMiscName = null;
            }
            else if (this.voucherDomainEntity.VendorName == "whitespace")
            {
                this.voucherDataContract.VouMiscName = new List<string>() { " " };
            }
            else if (this.voucherDomainEntity.Id == "25")
            {
                this.voucherDataContract.VouMiscName = new List<string>()
                {
                    this.voucherDomainEntity.VendorName,
                    this.voucherDomainEntity.VendorName,
                    this.voucherDomainEntity.VendorName
                };
            }
            else
            {
                this.voucherDataContract.VouMiscName = new List<string>() { this.voucherDomainEntity.VendorName };
            }

            this.voucherDataContract.VouApType = this.voucherDomainEntity.ApType;
            this.voucherDataContract.VouDate = this.voucherDomainEntity.Date;
            this.voucherDataContract.VouDueDate = this.voucherDomainEntity.DueDate;
            this.voucherDataContract.VouMaintGlTranDate = this.voucherDomainEntity.MaintenanceDate;
            this.voucherDataContract.VouDefaultInvoiceNo = this.voucherDomainEntity.InvoiceNumber;
            this.voucherDataContract.VouDefaultInvoiceDate = this.voucherDomainEntity.InvoiceDate;
            this.voucherDataContract.VouCheckNo = (string.IsNullOrEmpty(this.voucherDomainEntity.CheckNumber) ? null : this.voucherDomainEntity.CheckNumber);
            this.voucherDataContract.VouCheckDate = this.voucherDomainEntity.CheckDate;

            this.voucherDataContract.VouComments = this.voucherDomainEntity.Comments;
            this.voucherDataContract.VouPoNo = (string.IsNullOrEmpty(this.voucherDomainEntity.PurchaseOrderId) ? null : this.voucherDomainEntity.PurchaseOrderId);
            this.voucherDataContract.VouBpoId = (string.IsNullOrEmpty(this.voucherDomainEntity.BlanketPurchaseOrderId) ? null : this.voucherDomainEntity.BlanketPurchaseOrderId);
            this.voucherDataContract.VouRcvsId = (string.IsNullOrEmpty(this.voucherDomainEntity.RecurringVoucherId) ? null : this.voucherDomainEntity.RecurringVoucherId);
            this.voucherDataContract.VouCurrencyCode = this.voucherDomainEntity.CurrencyCode;

            this.voucherDataContract.VouStatus = new List<string>();
            switch (this.voucherDomainEntity.Status)
            {
                case VoucherStatus.InProgress:
                    this.voucherDataContract.VouStatus.Add("U");
                    break;
                case VoucherStatus.NotApproved:
                    this.voucherDataContract.VouStatus.Add("N");
                    break;
                case VoucherStatus.Outstanding:
                    this.voucherDataContract.VouStatus.Add("O");
                    break;
                case VoucherStatus.Paid:
                    this.voucherDataContract.VouStatus.Add("P");
                    break;
                case VoucherStatus.Reconciled:
                    this.voucherDataContract.VouStatus.Add("R");
                    break;
                case VoucherStatus.Voided:
                    this.voucherDataContract.VouStatus.Add("V");
                    break;
                case VoucherStatus.Cancelled:
                    this.voucherDataContract.VouStatus.Add("X");
                    break;
                default:
                    throw new Exception("Invalid status specified in VoucherRepositoryTests");
            }
            this.voucherDataContract.VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>();
            var statusAssociation = new VouchersVoucherStatus();
            statusAssociation.VouStatusAssocMember = this.voucherDataContract.VouStatus[0];
            statusAssociation.VouStatusDateAssocMember = new DateTime(2013, 1, 18);
            this.voucherDataContract.VoucherStatusEntityAssociation.Add(statusAssociation);


            this.recurringVoucherDataContract = new RcVouSchedules()
            {
                Recordkey = "1",
                RcvsRcVoucher = this.voucherDomainEntity.RecurringVoucherId
            };

            // vendor name comes from CTX
            string vendorName = "Vendor name for use in a colleague transaction";
            var ctxVendorName = new List<string>() { vendorName };

            if (this.voucherDomainEntity.Id == "26")
            {
                ctxVendorName.Add(vendorName);
                ctxVendorName.Add(vendorName);
            }

            this.hierarchyNameResponse = new TxGetHierarchyNameResponse()
            {
                IoPersonId = this.voucherDomainEntity.VendorId,
                OutPersonName = ctxVendorName
            };

            this.voucherDataContract.VouTaxesEntityAssociation = new List<VouchersVouTaxes>();

            // Build a list of line item IDs
            this.voucherDataContract.VouItemsId = new List<string>();
            foreach (var lineItem in this.voucherDomainEntity.LineItems)
            {
                if (lineItem.Id != "null")
                {
                    this.voucherDataContract.VouItemsId.Add(lineItem.Id);
                }
            }

            this.voucherDataContract.VouInvoiceAmt = this.voucherDomainEntity.Amount;
            // Build a list of Approver data contracts
            ConvertApproversIntoDataContracts();
            ConvertLineItemsIntoDataContracts();

        }

        private void ConvertApproversIntoDataContracts()
        {
            // Initialize the associations for approvers and next approvers.
            this.voucherDataContract.VouAuthEntityAssociation = new List<VouchersVouAuth>();
            this.voucherDataContract.VouApprEntityAssociation = new List<VouchersVouAppr>();
            this.opersDataContracts = new Collection<Opers>();
            this.voucherDataContract.VouAuthorizations = new List<string>();
            this.voucherDataContract.VouNextApprovalIds = new List<string>();
            foreach (var approver in this.voucherDomainEntity.Approvers)
            {
                if (approver.ApprovalDate != null)
                {
                    // Populate approvers
                    var dataContract = new VouchersVouAuth()
                    {
                        VouAuthorizationsAssocMember = approver.ApproverId,
                        VouAuthorizationDatesAssocMember = approver.ApprovalDate
                    };

                    this.voucherDataContract.VouAuthEntityAssociation.Add(dataContract);
                    this.voucherDataContract.VouAuthorizations.Add(approver.ApproverId);
                }
                else
                {
                    // Populate next approvers
                    var nextApproverDataContract = new VouchersVouAppr()
                    {
                        VouNextApprovalIdsAssocMember = approver.ApproverId
                    };
                    this.voucherDataContract.VouApprEntityAssociation.Add(nextApproverDataContract);
                    this.voucherDataContract.VouNextApprovalIds.Add(approver.ApproverId);
                }

                // Populate the Opers data contract
                this.opersDataContracts.Add(new Opers()
                {
                    Recordkey = approver.ApproverId,
                    SysUserName = approver.ApprovalName
                });
            }
        }

        private void ConvertLineItemsIntoDataContracts()
        {
            this.itemsDataContracts = new Collection<Items>();
            this.projectDataContracts = new Collection<Projects>();
            this.projectLineItemDataContracts = new Collection<ProjectsLineItems>();

            foreach (var lineItem in this.voucherDomainEntity.LineItems)
            {
                // Populate the line items directly
                var itemsDataContract = new Items()
                {
                    Recordkey = lineItem.Id,
                    ItmDesc = new List<string>() { lineItem.Description },
                    ItmVouQty = lineItem.Quantity,
                    ItmVouPrice = lineItem.Price,
                    ItmVouExtPrice = lineItem.ExtendedPrice,
                    ItmVouIssue = lineItem.UnitOfIssue,
                    ItmInvoiceNo = lineItem.InvoiceNumber,
                    ItmTaxForm = lineItem.TaxForm,
                    ItmTaxFormCode = lineItem.TaxFormCode,
                    ItmTaxFormLoc = lineItem.TaxFormLocation,
                    ItmComments = lineItem.Comments,
                    VouchGlEntityAssociation = new List<ItemsVouchGl>(),
                    VouGlTaxesEntityAssociation = new List<ItemsVouGlTaxes>()
                };

                // Populate the GL Distributions

                int counter = 0;
                foreach (var glDistr in lineItem.GlDistributions)
                {
                    counter++;

                    decimal localGlAmount = 0,
                        foreignGlAmount = 0;

                    // The amount from the LineItemGlDistribution domain entity is always going to be a local amount.
                    // If the voucher is in foreign currency, we need to manually set the test foreign amounts since
                    // they cannot be gotten from the domain entity. Currently, there is only one foreign currency voucher
                    // in the test data.
                    localGlAmount = glDistr.Amount;
                    if (!string.IsNullOrEmpty(this.voucherDomainEntity.CurrencyCode))
                    {
                        if (counter == 1)
                        {
                            foreignGlAmount = 150.00m;
                        }
                        else if (counter == 2)
                        {
                            foreignGlAmount = 100.00m;
                        }
                        else
                        {
                            foreignGlAmount = 50.00m;
                        }
                    }

                    itemsDataContract.VouchGlEntityAssociation.Add(new ItemsVouchGl()
                    {
                        ItmVouGlNoAssocMember = glDistr.GlAccountNumber,
                        ItmVouGlQtyAssocMember = glDistr.Quantity,
                        ItmVouProjectCfIdAssocMember = glDistr.ProjectId,
                        ItmVouPrjItemIdsAssocMember = glDistr.ProjectLineItemId,
                        ItmVouGlAmtAssocMember = localGlAmount,
                        ItmVouGlForeignAmtAssocMember = foreignGlAmount
                    });

                    this.projectDataContracts.Add(new Projects()
                    {
                        Recordkey = glDistr.ProjectId,
                        PrjRefNo = glDistr.ProjectNumber
                    });

                    this.projectLineItemDataContracts.Add(new ProjectsLineItems()
                    {
                        Recordkey = glDistr.ProjectLineItemId,
                        PrjlnProjectItemCode = glDistr.ProjectLineItemCode
                    });
                }

                // Populate the taxes
                int taxCounter = 0;
                foreach (var taxDistr in lineItem.LineItemTaxes)
                {
                    taxCounter++;
                    decimal? localTaxAmount = null,
                        foreignTaxAmount = null;

                    // The amount from the LineItemTax domain entity is going to be in local currency unless there is a
                    // currency code on the voucher.
                    //
                    // If the voucher does not have a currency code, the tax amount in the domain entity will be in local
                    // currency, and the foreign tax amount on the data contract will be null. 
                    //
                    // If the voucher does have a currency code, the tax amount in the domain entity will be in foreign
                    // currency, and we need to manually set the test local tax amounts since they cannot be gotten from
                    // the domain entity. Currently, there is only one foreign currency voucher in the test data.

                    if (string.IsNullOrEmpty(this.voucherDomainEntity.CurrencyCode))
                    {
                        localTaxAmount = taxDistr.TaxAmount;
                    }
                    else
                    {
                        foreignTaxAmount = taxDistr.TaxAmount;
                        if (counter == 1)
                        {
                            localTaxAmount = 15.00m;
                        }
                        else if (counter == 2)
                        {
                            localTaxAmount = 25.00m;
                        }
                        else
                        {
                            localTaxAmount = 10.00m;
                        }
                    }

                    itemsDataContract.VouGlTaxesEntityAssociation.Add(new ItemsVouGlTaxes()
                    {
                        ItmVouGlTaxCodeAssocMember = taxDistr.TaxCode,
                        ItmVouGlTaxAmtAssocMember = localTaxAmount,
                        ItmVouGlForeignTaxAmtAssocMember = foreignTaxAmount
                    });
                }

                this.itemsDataContracts.Add(itemsDataContract);
            }
        }

        #endregion
    }

    [TestClass]
    public class AccountsPayableInvoicesTests_PUT_POST_V11 : BaseRepositorySetup
    {
        #region DECLARATIONS

        AccountsPayableInvoicesRepository accountsPayableInvoicesRepository;

        private AccountsPayableInvoices accountsPayableInvoiceEntity;
        private AccountsPayableInvoicesLineItem lineItem;
        private UpdateVouchersIntegrationResponse response;
        private Dictionary<string, GuidLookupResult> dicResult;
        private Vouchers voucher;
        private Base.DataContracts.Person person;
        private List<string> apTypes;

        private Collection<Projects> projectDataContracts;
        private Collection<Items> itemsDataContracts;
        private Collection<ProjectsLineItems> projectLineItemDataContracts;
        private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            InitializeTestData();

            InitializeTestMock();

            accountsPayableInvoicesRepository = new AccountsPayableInvoicesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        private void InitializeTestData()
        {
            voucher = new Vouchers()
            {
                RecordGuid = guid,
                Recordkey = "1",
                VouVendor = "1",
                VouApType = "1",
                VouDefaultInvoiceNo = "1",
                VouDefaultInvoiceDate = DateTime.Today,
                VouDate = DateTime.Today,
                VouStatus = new List<string>() { "O" },
                VouRequestor = "1",
                VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>()
                    {
                        new VouchersVoucherStatus("U", DateTime.Today)
                    },
                VouTaxesEntityAssociation = new List<VouchersVouTaxes>()
                {

                },
                VouItemsId = new List<string>() { "1"}
            };

           
            person = new Base.DataContracts.Person()
            {
                RecordGuid = guid,
                Recordkey = "1"
            };

            apTypes = new List<string>() { "1" };

            response = new UpdateVouchersIntegrationResponse() { VoucherGuid = guid };

            dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1" } }
                };

            accountsPayableInvoiceEntity = new Domain.ColleagueFinance.Entities.AccountsPayableInvoices(guid, "1", DateTime.Today, VoucherStatus.Outstanding, "name", "1", DateTime.Today)
            {
                CurrencyCode = "USD",
                HostCountry = "CAN",
                VendorId = "1",
                VoucherAddressId = "1",
                VoucherUseAltAddress = true,
                VoucherMiscName = new List<string>() { "name" },
                VoucherMiscType = "person",
                VoucherMiscAddress = new List<string>() { "address" },
                VoucherMiscCity = "city",
                VoucherMiscZip = "12345",
                VoucherReferenceNo = new List<string>() { "1" },
                VoucherPayFlag = "Y",
                VoucherInvoiceAmt = 1000,
                VoucherDiscAmt = 100,
                VoucherNet = 10,
                VoucherRequestor = "1",
                VendorAddressId = "1",
                VoucherMiscState = "SA",
                VoucherMiscCountry = "USA",
                VoucherVoidGlTranDate = DateTime.Today,
                ApType = "1",
                DueDate = DateTime.Today.AddDays(10),
                VoucherVendorTerms = "1",
                Comments = "comments",
                PurchaseOrderId = "1",
               
                VoucherTaxes = new List<LineItemTax>() { new LineItemTax("1", 50) },
            };

            lineItem = new AccountsPayableInvoicesLineItem("1", "desc", 10, 1000, 100)
            {
                CommodityCode = "1",
                FixedAssetsFlag = "S",
                UnitOfIssue = "1",
                CashDiscountAmount = 10,
                TradeDiscountAmount = 5,
                TradeDiscountPercent = 1,
                UnitOfMeasure = "1",
                Comments = "comments",

                AccountsPayableLineItemTaxes = new List<LineItemTax>()
                    {
                        new LineItemTax("1", 10)
                        {
                            LineGlNumber = "1"
                        }
                    }
            };

            lineItem.AddGlDistribution(new LineItemGlDistribution("1", 10, 1000, 1)
            {
                ProjectId = "1",

            });

            accountsPayableInvoiceEntity.AddAccountsPayableInvoicesLineItem(lineItem);
            ConvertLineItemsIntoDataContracts();

        }

        private void ConvertLineItemsIntoDataContracts()
        {
            this.itemsDataContracts = new Collection<Items>();
            this.projectDataContracts = new Collection<Projects>();
            this.projectLineItemDataContracts = new Collection<ProjectsLineItems>();

            foreach (var lineItem in this.accountsPayableInvoiceEntity.LineItems)
            {
                // Populate the line items directly
                var itemsDataContract = new Items()
                {
                    Recordkey = lineItem.Id,
                    ItmDesc = new List<string>() { lineItem.Description },
                    ItmVouQty = lineItem.Quantity,
                    ItmVouPrice = lineItem.Price,
                    ItmVouExtPrice = lineItem.ExtendedPrice,
                    ItmVouIssue = lineItem.UnitOfIssue,
                    ItmInvoiceNo = lineItem.InvoiceNumber,
                    ItmTaxForm = lineItem.TaxForm,
                    ItmFixedAssetsFlag = lineItem.FixedAssetsFlag,
                    ItmTaxFormCode = lineItem.TaxFormCode,
                    ItmTaxFormLoc = lineItem.TaxFormLocation,
                    ItmComments = lineItem.Comments,
                    VouchGlEntityAssociation = new List<ItemsVouchGl>(),
                    VouGlTaxesEntityAssociation = new List<ItemsVouGlTaxes>()
                };

                // Populate the GL Distributions

                int counter = 0;
                foreach (var glDistr in lineItem.GlDistributions)
                {
                    counter++;

                    decimal localGlAmount = 0,
                        foreignGlAmount = 0;

                    // The amount from the LineItemGlDistribution domain entity is always going to be a local amount.
                    // If the voucher is in foreign currency, we need to manually set the test foreign amounts since
                    // they cannot be gotten from the domain entity. Currently, there is only one foreign currency voucher
                    // in the test data.
                    localGlAmount = glDistr.Amount;
                    if (!string.IsNullOrEmpty(this.accountsPayableInvoiceEntity.CurrencyCode))
                    {
                        if (counter == 1)
                        {
                            foreignGlAmount = 150.00m;
                        }
                        else if (counter == 2)
                        {
                            foreignGlAmount = 100.00m;
                        }
                        else
                        {
                            foreignGlAmount = 50.00m;
                        }
                    }

                    itemsDataContract.VouchGlEntityAssociation.Add(new ItemsVouchGl()
                    {
                        ItmVouGlNoAssocMember = glDistr.GlAccountNumber,
                        ItmVouGlQtyAssocMember = glDistr.Quantity,
                        ItmVouProjectCfIdAssocMember = glDistr.ProjectId,
                        ItmVouPrjItemIdsAssocMember = glDistr.ProjectLineItemId,
                        ItmVouGlAmtAssocMember = localGlAmount,
                        ItmVouGlForeignAmtAssocMember = foreignGlAmount
                    });

                    this.projectDataContracts.Add(new Projects()
                    {
                        Recordkey = glDistr.ProjectId,
                        PrjRefNo = glDistr.ProjectNumber
                    });

                    this.projectLineItemDataContracts.Add(new ProjectsLineItems()
                    {
                        Recordkey = glDistr.ProjectLineItemId,
                        PrjlnProjectItemCode = glDistr.ProjectLineItemCode
                    });
                }

                // Populate the taxes
                int taxCounter = 0;
                foreach (var taxDistr in lineItem.LineItemTaxes)
                {
                    taxCounter++;
                    decimal? localTaxAmount = null,
                        foreignTaxAmount = null;

                    // The amount from the LineItemTax domain entity is going to be in local currency unless there is a
                    // currency code on the voucher.
                    //
                    // If the voucher does not have a currency code, the tax amount in the domain entity will be in local
                    // currency, and the foreign tax amount on the data contract will be null. 
                    //
                    // If the voucher does have a currency code, the tax amount in the domain entity will be in foreign
                    // currency, and we need to manually set the test local tax amounts since they cannot be gotten from
                    // the domain entity. Currently, there is only one foreign currency voucher in the test data.

                    if (string.IsNullOrEmpty(this.accountsPayableInvoiceEntity.CurrencyCode))
                    {
                        localTaxAmount = taxDistr.TaxAmount;
                    }
                    else
                    {
                        foreignTaxAmount = taxDistr.TaxAmount;
                        if (counter == 1)
                        {
                            localTaxAmount = 15.00m;
                        }
                        else if (counter == 2)
                        {
                            localTaxAmount = 25.00m;
                        }
                        else
                        {
                            localTaxAmount = 10.00m;
                        }
                    }

                    itemsDataContract.VouGlTaxesEntityAssociation.Add(new ItemsVouGlTaxes()
                    {
                        ItmVouGlTaxCodeAssocMember = taxDistr.TaxCode,
                        ItmVouGlTaxAmtAssocMember = localTaxAmount,
                        ItmVouGlForeignTaxAmtAssocMember = foreignTaxAmount
                    });
                }

                this.itemsDataContracts.Add(itemsDataContract);
            }
        }

        private void InitializeTestMock()
        {
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateVouchersIntegrationRequest, UpdateVouchersIntegrationResponse>(It.IsAny<UpdateVouchersIntegrationRequest>())).ReturnsAsync(response);
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
            dataReaderMock.Setup(r => r.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(voucher);
            dataReaderMock.Setup(r => r.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(person);
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(apTypes.ToArray());
            string[] itemsId = { "1"};
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Items>(itemsId, true)).ReturnsAsync(this.itemsDataContracts);

        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_ArgumentNullException_Entity_Null()
        {
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(null);
        }

        //[TestMethod]
        //[ExpectedException(typeof(RepositoryException))]
        //public async Task ActPayInvService_CreateAccountsPayableInvoices_RepositoryException()
        //{
        //    response.ErrorMessages = new List<string>() { "error1" };
        //    await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_ArgumentException()
        {
            response.VoucherGuid = null;
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_Result_Null()
        {
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(() => null);
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_KeyNotFoundException()
        {
            dicResult[guid] = null;
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_InvalidEntity_Name_Repositoryxception()
        {
            dicResult[guid].Entity = "VOUCHER";
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_PrimaryKey_Empty_KeyNotFoundException()
        {
            dicResult[guid].PrimaryKey = string.Empty;
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_Voucher_Null_KeyNotFoundException()
        {
            dataReaderMock.Setup(r => r.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).ReturnsAsync(() => null);
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_Vendor_Null_ArgumentException()
        {
            dataReaderMock.Setup(r => r.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(() => null);
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_VoucherType_Null_RepositorytException()
        {
            voucher.VouApType = "2";
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_VoucherStatus_Cancelled_OR_Voided_ArgumentException()
        {
            voucher.VouStatus = new List<string>() { "X", "V" };
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_Empty_VoucherStatus_Association()
        {
            voucher.VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>();
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_Invalid_VoucherStatus()
        {
            voucher.VoucherStatusEntityAssociation.FirstOrDefault().VouStatusAssocMember = "A";
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_Empty_VoucherDate()
        {
            voucher.VouDate = null;
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }
        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_ReccuringVoucher_NotFound()
        {
            voucher.VouRcvsId = "1";
            dataReaderMock.Setup(d => d.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).ReturnsAsync(() => null);
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ActPayInvService_CreateAccountsPayableInvoices_Get_Voucher_ParentDocuments_Exception()
        {
            voucher.VouRcvsId = "1";
            dataReaderMock.Setup(d => d.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).ThrowsAsync(new ApplicationException());
            await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);
        }


        [TestMethod]
        public async Task ActPayInvService_CreateAccountsPayableInvoices()
        {
            var result = await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ActPayInvService_VoucherDefaultInvoiceNumberIsNull()
        {
            var expectedMessage = "";
            var actualMessage = "";
            try
            {                
               
                expectedMessage = "Vendor Invoice Number is a required field. Entity: 'VOUCHERS', Record ID: '1'\r\nParameter name: vendorInvoiceNumber";
                accountsPayableInvoiceEntity = new Domain.ColleagueFinance.Entities.AccountsPayableInvoices(guid, "1", DateTime.Today, 
                    VoucherStatus.InProgress, "name", null, DateTime.Today)
                {
                    CurrencyCode = "USD",
                    HostCountry = "CAN",
                    VendorId = "1",
                    SubmittedBy = "1",
                    VoucherAddressId = "1",
                    VoucherUseAltAddress = true,
                    VoucherMiscName = new List<string>() { "name" },
                    VoucherMiscType = "person",
                    VoucherMiscAddress = new List<string>() { "address" },
                    VoucherMiscCity = "city",
                    VoucherMiscZip = "12345",
                    VoucherReferenceNo = new List<string>() { "1" },
                    VoucherPayFlag = "Y",
                    VoucherInvoiceAmt = 1000,
                    VoucherDiscAmt = 100,
                    VoucherNet = 10,
                    VoucherRequestor = "1",
                    VendorAddressId = "1",
                    VoucherMiscState = "SA",
                    VoucherMiscCountry = "USA",
                    VoucherVoidGlTranDate = DateTime.Today,
                    ApType = "1",
                    DueDate = DateTime.Today.AddDays(10),
                    VoucherVendorTerms = "1",
                    Comments = "comments",
                    PurchaseOrderId = "1",
                    VoucherTaxes = new List<LineItemTax>() { new LineItemTax("1", 50) },
                };

               await accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoiceEntity);

            }
            catch (ArgumentNullException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
    }
}
