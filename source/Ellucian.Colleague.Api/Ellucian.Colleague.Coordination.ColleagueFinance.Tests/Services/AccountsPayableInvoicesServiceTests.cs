// Copyright 2016-2018 Ellucian Company L.P. and its affiliates

using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Security;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using System;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Dtos;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    #region v11 AccountsPayableInvoices
    [TestClass]
    public class AccountsPayableInvoicesServiceTests_GET : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private AccountsPayableInvoicesService AccountsPayableInvoicesService;
        private TestVoucherRepository testVoucherRepository;
        private Mock<IAccountsPayableInvoicesRepository> mockAccountsPayableInvoices;
        private Mock<IColleagueFinanceReferenceDataRepository> mockcolleagueFinanceReferenceDataRepository;
        private Mock<IReferenceDataRepository> mockreferenceDataRepository;
        private Mock<IGeneralLedgerConfigurationRepository> mockGeneralLedgerConfigurationRepository;
        private Mock<IPersonRepository> mockPersonRepository;
        private Mock<IAddressRepository> mockaddressRepository;
        private Mock<IVendorsRepository> mockvendorsRepository;
        private Mock<IAccountFundsAvailableRepository> mockAccountFundsAvailable;
        private AccountFundsAvailableUser currentUserFactory = new GeneralLedgerCurrentUser.AccountFundsAvailableUser();
        private Mock<IRoleRepository> roleRepositoryMock;
        protected Ellucian.Colleague.Domain.Entities.Role viewAccountsPayableInvoicesRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.AP.INVOICES");
        protected Ellucian.Colleague.Domain.Entities.Role updateAccountsPayableInvoicesRole = new Domain.Entities.Role(1, "UPDATE.AP.INVOICES");

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices AccountsPayableInvoicesEntity;
        private Ellucian.Colleague.Dtos.AccountsPayableInvoices2 _accountsPayableInvoiceDto;
        private Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices> accountsPayableInvoicesEntities = new Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>();
        string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";

        string[] voucherIds = { "1" , "2", "3","4","11","13", "14", "15", "16", "17", "18"
                , "19", "20" ,"21", "22", "23", "24", "25", "26", "27", "29"};

        string[] guids = { "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", "guid2-f6a0-4a1c-8d55-9f2a6dd6be46", "guid3-f6a0-4a1c-8d55-9f2a6dd6be46"
            , "guid4-f6a0-4a1c-8d55-9f2a6dd6be46"};
        private int versionNumber;

        [TestInitialize]
        public void Initialize()
        {
            mockAccountsPayableInvoices = new Mock<IAccountsPayableInvoicesRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
            BuildValidVoucherService();
            versionNumber = 2;
            BuildDto();
        }



        [TestCleanup]
        public void Cleanup()
        {
            // Reset all of the services and repository variables.
            AccountsPayableInvoicesService = null;
            testVoucherRepository = null;
            mockAccountsPayableInvoices = null;
            mockcolleagueFinanceReferenceDataRepository = null;
            mockreferenceDataRepository = null;
            mockaddressRepository = null;
            mockvendorsRepository = null;
            currentUserFactory = null;
            mockGeneralLedgerConfigurationRepository = null;
            mockPersonRepository = null;
        }
        #endregion

        [TestMethod]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoices2ByGuidAsync()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            Collection<Ellucian.Colleague.Domain.Base.Entities.CommerceTaxCode> TaxCodInfo = new Collection<Ellucian.Colleague.Domain.Base.Entities.CommerceTaxCode>() { new Ellucian.Colleague.Domain.Base.Entities.CommerceTaxCode("TaxGuid", "ST", "TestGUIDdesc") };
            mockreferenceDataRepository.Setup(repo => repo.GetCommerceTaxCodesAsync(It.IsAny<bool>())).ReturnsAsync(TaxCodInfo);

            Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityCode> commodityCodes = new Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityCode>() { new Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityCode("CommodityGuid321", "00402", "Test Commodity") };
            mockcolleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityCodesAsync(It.IsAny<bool>())).ReturnsAsync(commodityCodes);

            Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableSources> apTypes = new Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableSources>() { new Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableSources("apTypeGuid321", "AP", "Account Payable") };
            mockcolleagueFinanceReferenceDataRepository.Setup(repo => repo.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(apTypes);

            Collection<VendorTerm> Terms = new Collection<VendorTerm>() { new VendorTerm("TermsGuid321", "02", "02-15 days") };
            mockcolleagueFinanceReferenceDataRepository.Setup(repo => repo.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(Terms);

            Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType> UnitTypes = new Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType>() {
                new Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType("unitGuid321", "rock", "Rocks"),
                new Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType("unitGuid123", "thing", "Things") };
            mockcolleagueFinanceReferenceDataRepository.Setup(repo => repo.GetCommodityUnitTypesAsync(It.IsAny<bool>())).ReturnsAsync(UnitTypes);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            AccountsPayableInvoicesEntity.PurchaseOrderId = "PO01";
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(AccountsPayableInvoicesEntity);
            mockvendorsRepository.Setup(repo => repo.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("VendorIDGuid");
            mockaddressRepository.Setup(repo => repo.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("AddressGuid");
            mockAccountsPayableInvoices.Setup(rep => rep.GetGuidFromID(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("POGuid");


            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);
            Assert.IsNotNull(actual);
            Assert.AreEqual(guid, actual.Id);
            Assert.AreEqual(AccountsPayableInvoicesEntity.Comments, actual.InvoiceComment);
            Assert.AreEqual(AccountsPayableInvoicesEntity.VoucherDiscAmt, actual.InvoiceDiscountAmount.Value);
            Assert.AreEqual(AccountsPayableInvoicesInvoiceType.Invoice, actual.InvoiceType);
            Assert.AreEqual(AccountsPayableInvoicesProcessState.Inprogress, actual.ProcessState);
            if (actual.ProcessState != AccountsPayableInvoicesProcessState.NotSet)
            {
                if (AccountsPayableInvoicesEntity.VoucherPayFlag == "Y")
                {
                    Assert.AreEqual(AccountsPayableInvoicesPaymentStatus.Nohold, actual.PaymentStatus);
                }
                else
                {
                    Assert.AreEqual(AccountsPayableInvoicesPaymentStatus.Hold, actual.PaymentStatus);
                }
            }
            Assert.AreEqual("RefNo1111", actual.ReferenceNumber);
            Assert.AreEqual("taxguid", actual.Taxes[0].TaxCode.Id);
            Assert.AreEqual(AccountsPayableInvoicesEntity.VoucherTaxes[0].TaxAmount, actual.Taxes[0].VendorAmount.Value);
            Assert.AreEqual(Dtos.EnumProperties.CurrencyIsoCode.USD, actual.Taxes[0].VendorAmount.Currency);

            Assert.AreEqual("VendorIDGuid", actual.Vendor.ExistingVendor.Vendor.Id);
            Assert.AreEqual("AddressGuid", actual.Vendor.ExistingVendor.AlternativeVendorAddress.Id);
            Assert.AreEqual(AccountsPayableInvoicesEntity.VoucherInvoiceAmt, actual.VendorBilledAmount.Value);
            Assert.AreEqual(Dtos.EnumProperties.CurrencyIsoCode.USD, actual.VendorBilledAmount.Currency);
            Assert.AreEqual(AccountsPayableInvoicesEntity.InvoiceDate, actual.VendorInvoiceDate);
            Assert.AreEqual(AccountsPayableInvoicesEntity.InvoiceNumber, actual.VendorInvoiceNumber);
            Assert.AreEqual(AccountsPayableInvoicesEntity.VoucherVoidGlTranDate, actual.VoidDate);
            Assert.AreEqual("aptypeguid321", actual.Payment.Source.Id);
            Assert.AreEqual(AccountsPayableInvoicesEntity.DueDate, actual.Payment.PaymentDueOn);
            Assert.AreEqual("termsguid321", actual.Payment.PaymentTerms.Id);

            Assert.AreEqual(AccountsPayableInvoicesEntity.LineItems.Count(), actual.LineItems.Count());
            for (int x = 0; x > actual.LineItems.Count(); x++)
            {
                var dtoLi = actual.LineItems[x];
                var entityLi = AccountsPayableInvoicesEntity.LineItems[x];

                if (dtoLi.ReferenceDocument != null)
                {
                    Assert.AreEqual("POGuid", dtoLi.ReferenceDocument.PurchaseOrder.Id);
                }

                Assert.AreEqual(entityLi.Description, dtoLi.Description);
                Assert.AreEqual("CommodityGuid321", dtoLi.CommodityCode.Id);
                Assert.AreEqual(entityLi.Quantity, dtoLi.Quantity);
                Assert.AreEqual(null, dtoLi.VendorBilledQuantity);
                if (entityLi.UnitOfIssue == "rocks")
                {
                    Assert.AreEqual("unitGuid321", dtoLi.Description);
                }
                else
                {
                    Assert.AreEqual("unitGuid123", dtoLi.Description);
                }
                Assert.AreEqual(entityLi.Price, dtoLi.UnitPrice.Value);
                Assert.AreEqual(Dtos.EnumProperties.CurrencyIsoCode.USD, dtoLi.UnitPrice.Currency);
                Assert.AreEqual(null, dtoLi.VendorBilledUnitPrice.Value);
                Assert.AreEqual(null, dtoLi.VendorBilledUnitPrice.Currency);
                Assert.AreEqual(null, dtoLi.AdditionalAmount.Value);
                Assert.AreEqual(null, dtoLi.AdditionalAmount.Currency);
                Assert.AreEqual(entityLi.LineItemTaxes.Count(), dtoLi.Taxes.Count());
                for (int i = 0; i < dtoLi.Taxes.Count(); i++)
                {
                    Assert.AreEqual(entityLi.LineItemTaxes[i].TaxCode, dtoLi.Taxes[i].TaxCode);
                    Assert.AreEqual(entityLi.LineItemTaxes[i].TaxAmount, dtoLi.Taxes[i].VendorAmount.Value);
                    Assert.AreEqual(Dtos.EnumProperties.CurrencyIsoCode.USD, dtoLi.Taxes[i].VendorAmount.Currency);
                }

                Assert.AreEqual(entityLi.CashDiscountAmount, dtoLi.Discount.Amount.Value);
                Assert.AreEqual(Dtos.EnumProperties.CurrencyIsoCode.USD, dtoLi.Discount.Amount.Currency);
                Assert.AreEqual(entityLi.TradeDiscountPercent, dtoLi.Discount.Percent);
                Assert.AreEqual(Dtos.EnumProperties.AccountsPayableInvoicesPaymentStatus.Nohold, dtoLi.PaymentStatus);
                Assert.AreEqual(entityLi.Comments, dtoLi.Comment);

                if (AccountsPayableInvoicesEntity.Status == VoucherStatus.InProgress
                    || AccountsPayableInvoicesEntity.Status == VoucherStatus.NotApproved
                    || AccountsPayableInvoicesEntity.Status == VoucherStatus.Outstanding)
                {
                    Assert.AreEqual(AccountsPayableInvoicesStatus.Open, dtoLi.Status);
                }
                else
                {
                    Assert.AreEqual(AccountsPayableInvoicesStatus.Closed, dtoLi.Status);
                }
                Assert.AreEqual(entityLi.GlDistributions.Count(), dtoLi.AccountDetails.Count());
                for (int j = 0; j < entityLi.GlDistributions.Count(); j++)
                {
                    Assert.AreEqual(j, dtoLi.AccountDetails[j].SequenceNumber);
                    Assert.AreEqual(entityLi.GlDistributions[j].GlAccountNumber, dtoLi.AccountDetails[j].AccountingString);
                    Assert.AreEqual(entityLi.GlDistributions[j].Amount, dtoLi.AccountDetails[j].Allocation.Allocated.Amount.Value);
                    Assert.AreEqual(Dtos.EnumProperties.CurrencyIsoCode.USD, dtoLi.AccountDetails[j].Allocation.Allocated.Amount.Currency);
                    Assert.AreEqual(entityLi.GlDistributions[j].Quantity, dtoLi.AccountDetails[j].Allocation.Allocated.Quantity);
                    Assert.AreEqual(entityLi.GlDistributions[j].Percent, dtoLi.AccountDetails[j].Allocation.Allocated.Percentage);
                    Assert.AreEqual(null, dtoLi.AccountDetails[j].Allocation.AdditionalAmount.Value);
                    Assert.AreEqual(null, dtoLi.AccountDetails[j].Allocation.AdditionalAmount.Currency);
                    Assert.AreEqual(null, dtoLi.AccountDetails[j].Allocation.DiscountAmount.Value);
                    Assert.AreEqual(null, dtoLi.AccountDetails[j].Allocation.DiscountAmount.Currency);
                    Assert.AreEqual("apTypeGuid321", dtoLi.AccountDetails[j].Source.Id);
                }
            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoices2Async()
        {
            for (int x = 0; x < 4; x++)
            {
                string voucherId = voucherIds[x];
                guid = guids[x];
                var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);
                AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);

                accountsPayableInvoicesEntities.Add(AccountsPayableInvoicesEntity);

            }

            Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>, int> GetAPIValues = new Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>, int>(accountsPayableInvoicesEntities, 4);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoices2Async(0, 100)).ReturnsAsync(GetAPIValues);
            mockvendorsRepository.Setup(repo => repo.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("VendorIDGuid");
            mockaddressRepository.Setup(repo => repo.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("AddressGuid");

            var actuals = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2Async(0, 100);
            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(4, actuals.Item2);
            Assert.AreEqual(4, actuals.Item1.Count());

            foreach (var actual in actuals.Item1)
            {
                var expected = accountsPayableInvoicesEntities.FirstOrDefault(x => x.Guid == actual.Id);

                Assert.IsNotNull(expected, actual.Id);
                Assert.AreEqual(expected.Comments, actual.InvoiceComment, actual.Id);
                Assert.AreEqual(expected.VoucherDiscAmt, actual.InvoiceDiscountAmount.Value, actual.Id);
                if (actual.ProcessState != AccountsPayableInvoicesProcessState.NotSet)
                {
                    if (AccountsPayableInvoicesEntity.VoucherPayFlag == "Y")
                    {
                        Assert.AreEqual(AccountsPayableInvoicesPaymentStatus.Nohold, actual.PaymentStatus, actual.Id);
                    }
                    else
                    {
                        Assert.AreEqual(AccountsPayableInvoicesPaymentStatus.Hold, actual.PaymentStatus, actual.Id);
                    }
                }
                Assert.AreEqual("RefNo1111", actual.ReferenceNumber, actual.Id);
                // Assert.AreEqual(AccountsPayableInvoicesEntity.VoucherTaxes, actual.Taxes);

                //Assert.AreEqual("VendorIDGuid", actual.Vendor.Id, actual.Id);
                //Assert.AreEqual("AddressGuid", actual.VendorAddress.Id, actual.Id);
                Assert.AreEqual(expected.VoucherInvoiceAmt, actual.VendorBilledAmount.Value, actual.Id);
                Dtos.EnumProperties.CurrencyIsoCode ThisCurrency = (expected.CurrencyCode == "CAD" ? Dtos.EnumProperties.CurrencyIsoCode.CAD : Dtos.EnumProperties.CurrencyIsoCode.USD);
                Assert.AreEqual(ThisCurrency, actual.VendorBilledAmount.Currency, actual.Id);
                Assert.AreEqual(expected.InvoiceDate, actual.VendorInvoiceDate, actual.Id);
                Assert.AreEqual(expected.InvoiceNumber, actual.VendorInvoiceNumber, actual.Id);
                Assert.AreEqual(expected.VoucherVoidGlTranDate, actual.VoidDate, actual.Id);

                if (expected.LineItems.Count() > 0)
                {
                    Assert.AreEqual(expected.LineItems.Count(), actual.LineItems.Count());
                    for (int x = 0; x > actual.LineItems.Count(); x++)
                    {
                        var dtoLi = actual.LineItems[x];
                        var entityLi = expected.LineItems[x];

                        Assert.AreEqual(entityLi.Description, dtoLi.Description);
                        Assert.AreEqual(entityLi.PurchaseOrderId, dtoLi.ReferenceDocument.PurchaseOrder.Id);
                        Assert.AreEqual(entityLi.Quantity, dtoLi.Quantity);
                        Assert.AreEqual(null, dtoLi.VendorBilledQuantity);
                        Assert.AreEqual(entityLi.Price, dtoLi.UnitPrice.Value);
                        Assert.AreEqual(Dtos.EnumProperties.CurrencyCodes.USD, dtoLi.UnitPrice.Currency);
                        Assert.AreEqual(null, dtoLi.VendorBilledUnitPrice.Value);
                        Assert.AreEqual(null, dtoLi.VendorBilledUnitPrice.Currency);
                        Assert.AreEqual(null, dtoLi.AdditionalAmount.Value);
                        Assert.AreEqual(null, dtoLi.AdditionalAmount.Currency);
                        Assert.AreEqual(entityLi.LineItemTaxes.Count(), dtoLi.Taxes.Count());
                        for (int i = 0; i < dtoLi.Taxes.Count(); i++)
                        {
                            Assert.AreEqual(entityLi.LineItemTaxes[i].TaxCode, dtoLi.Taxes[i].TaxCode);
                            Assert.AreEqual(entityLi.LineItemTaxes[i].TaxAmount, dtoLi.Taxes[i].VendorAmount.Value);
                            Assert.AreEqual(Dtos.EnumProperties.CurrencyCodes.USD, dtoLi.Taxes[i].VendorAmount.Currency);
                        }

                        Assert.AreEqual(entityLi.CashDiscountAmount, dtoLi.Discount.Amount.Value);
                        Assert.AreEqual(Dtos.EnumProperties.CurrencyCodes.USD, dtoLi.Discount.Amount.Currency);
                        Assert.AreEqual(entityLi.TradeDiscountPercent, dtoLi.Discount.Percent);
                        Assert.AreEqual(Dtos.EnumProperties.AccountsPayableInvoicesPaymentStatus.Nohold, dtoLi.PaymentStatus);
                        Assert.AreEqual(entityLi.Comments, dtoLi.Comment);

                        if (AccountsPayableInvoicesEntity.Status == VoucherStatus.InProgress
                            || AccountsPayableInvoicesEntity.Status == VoucherStatus.NotApproved
                            || AccountsPayableInvoicesEntity.Status == VoucherStatus.Outstanding)
                        {
                            Assert.AreEqual(AccountsPayableInvoicesStatus.Open, dtoLi.Status);
                        }
                        else
                        {
                            Assert.AreEqual(AccountsPayableInvoicesStatus.Closed, dtoLi.Status);
                        }
                        Assert.AreEqual(entityLi.GlDistributions.Count(), dtoLi.AccountDetails.Count());
                        for (int j = 0; j < entityLi.GlDistributions.Count(); j++)
                        {
                            Assert.AreEqual(j, dtoLi.AccountDetails[j].SequenceNumber);
                            Assert.AreEqual(entityLi.GlDistributions[j].GlAccountNumber, dtoLi.AccountDetails[j].AccountingString);
                            Assert.AreEqual(entityLi.GlDistributions[j].Amount, dtoLi.AccountDetails[j].Allocation.Allocated.Amount.Value);
                            Assert.AreEqual(Dtos.EnumProperties.CurrencyCodes.USD, dtoLi.AccountDetails[j].Allocation.Allocated.Amount.Currency);
                            Assert.AreEqual(entityLi.GlDistributions[j].Quantity, dtoLi.AccountDetails[j].Allocation.Allocated.Quantity);
                            Assert.AreEqual(entityLi.GlDistributions[j].Percent, dtoLi.AccountDetails[j].Allocation.Allocated.Percentage);
                            Assert.AreEqual(null, dtoLi.AccountDetails[j].Allocation.AdditionalAmount.Value);
                            Assert.AreEqual(null, dtoLi.AccountDetails[j].Allocation.AdditionalAmount.Currency);
                            Assert.AreEqual(null, dtoLi.AccountDetails[j].Allocation.DiscountAmount.Value);
                            Assert.AreEqual(null, dtoLi.AccountDetails[j].Allocation.DiscountAmount.Currency);
                        }
                    }
                }
            }

        }

        [TestMethod]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesAsync_MultipleReferenceDocNumbers()
        {

            string voucherId = voucherIds[0];
            guid = guids[0];
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);
            voucherDomainEntity.AddLineItem(new LineItem("10", "test", 1.22m, 1.23m, 1.24m));
            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI2(voucherDomainEntity);
            accountsPayableInvoicesEntities.Add(AccountsPayableInvoicesEntity);

            Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>, int> GetAPIValues = new Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>, int>(accountsPayableInvoicesEntities, 4);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoices2Async(0, 100)).ReturnsAsync(GetAPIValues);
            mockvendorsRepository.Setup(repo => repo.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("VendorIDGuid");
            mockaddressRepository.Setup(repo => repo.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("AddressGuid");

            var actuals = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2Async(0, 100);
            Assert.IsNotNull(actuals.Item1);


            foreach (var actual in actuals.Item1)
            {
                var expected = accountsPayableInvoicesEntities.FirstOrDefault(x => x.Guid == actual.Id);
                if (expected.LineItems.Count() > 0)
                {
                    Assert.AreEqual(expected.LineItems.Count(), actual.LineItems.Count());
                    for (int x = 0; x > actual.LineItems.Count(); x++)
                    {
                        var dtoLi = actual.LineItems[x];
                        var entityLi = expected.LineItems[x];

                        Assert.AreEqual(string.Format("{0}-{1}", entityLi.PurchaseOrderId, entityLi.Id),
                            string.Format("{0}-{1}", dtoLi.ReferenceDocument.PurchaseOrder.Id, dtoLi.LineItemNumber));
                    }
                }
            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesAsync_Offset()
        {
            for (int x = 1; x < 3; x++)
            {
                string voucherId = voucherIds[x];
                guid = guids[x];
                var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);
                AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);

                accountsPayableInvoicesEntities.Add(AccountsPayableInvoicesEntity);

            }

            Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>, int> GetAPIValues = new Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>, int>(accountsPayableInvoicesEntities, 2);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoices2Async(1, 100)).ReturnsAsync(GetAPIValues);
            mockvendorsRepository.Setup(repo => repo.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("VendorIDGuid");
            mockaddressRepository.Setup(repo => repo.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("AddressGuid");

            var actuals = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2Async(1, 100);
            int i = 1;
            foreach (var actual in actuals.Item1)
            {
                guid = guids[i];
                Assert.AreEqual(guid, actual.Id);
                i++;
            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesAsync_limit()
        {
            for (int x = 0; x < 2; x++)
            {
                string voucherId = voucherIds[x];
                guid = guids[x];
                var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);
                AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);

                accountsPayableInvoicesEntities.Add(AccountsPayableInvoicesEntity);

            }

            Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>, int> GetAPIValues = new Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>, int>(accountsPayableInvoicesEntities, 2);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoices2Async(0, 2)).ReturnsAsync(GetAPIValues);
            mockvendorsRepository.Setup(repo => repo.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("VendorIDGuid");
            mockaddressRepository.Setup(repo => repo.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("AddressGuid");

            var actuals = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2Async(0, 2);

            Assert.AreEqual(2, actuals.Item1.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_NoApPermission()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            UserFactoryAll TestcurrentUserFactory = new GeneralLedgerCurrentUser.UserFactoryAll();
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var loggerObject = new Mock<ILogger>().Object;

            AccountsPayableInvoicesService = new AccountsPayableInvoicesService(mockcolleagueFinanceReferenceDataRepository.Object,
                mockreferenceDataRepository.Object, mockAccountsPayableInvoices.Object, mockaddressRepository.Object, mockvendorsRepository.Object,
                mockGeneralLedgerConfigurationRepository.Object, mockPersonRepository.Object, baseConfigurationRepository,
                adapterRegistry.Object, TestcurrentUserFactory, roleRepositoryMock.Object, mockAccountFundsAvailable.Object, loggerObject);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(AccountsPayableInvoicesEntity);
            mockvendorsRepository.Setup(repo => repo.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("VendorIDGuid");
            mockaddressRepository.Setup(repo => repo.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("AddressGuid");

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_NoKeyException()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_NoRecordException()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_ArgumentExceptionn()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_ApplicationException()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ApplicationException());

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_NullGuid()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException()); ;

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(null);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_InvalidOperExcept()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException()); ;

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_RepoException()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException()); ;

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);

        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AccountsPayableInvoicesServiceTests_GetAccountsPayableInvoicesByGuidAsync_Exception()
        {
            string voucherId = "1";
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, "00000001", GlAccessLevel.Full_Access, null, versionNumber);

            AccountsPayableInvoicesEntity = ConvertVoucherEntityToAPI(voucherDomainEntity);
            mockAccountsPayableInvoices.Setup(repo => repo.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception()); ;

            var actual = await AccountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);

        }

        private async void BuildValidVoucherService()
        {
            var loggerObject = new Mock<ILogger>().Object;

            testVoucherRepository = new TestVoucherRepository();
            mockcolleagueFinanceReferenceDataRepository = new Mock<IColleagueFinanceReferenceDataRepository>();
            mockreferenceDataRepository = new Mock<IReferenceDataRepository>();
            mockAccountsPayableInvoices = new Mock<IAccountsPayableInvoicesRepository>();
            mockvendorsRepository = new Mock<IVendorsRepository>();
            mockaddressRepository = new Mock<IAddressRepository>();
            mockPersonRepository = new Mock<IPersonRepository>();
            mockGeneralLedgerConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            mockAccountFundsAvailable = new Mock<IAccountFundsAvailableRepository>();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();

            viewAccountsPayableInvoicesRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewApInvoices));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountsPayableInvoicesRole });

            var glAcctStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            glAcctStructure.CheckAvailableFunds = "Y";
            mockGeneralLedgerConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(glAcctStructure);

            var checkFund = new AccountFundsAvailable()
            {
                AccountingStringValue = "test",
                BalanceOn = new DateTime(2017, 01, 15),
                FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available
            };

            //mockAccountFundsAvailable.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(),
            //    It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(checkFund);

            mockAccountFundsAvailable.Setup(x => x.CheckAvailableFundsAsync(It.IsAny<List<Domain.ColleagueFinance.Entities.FundsAvailable>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);

            mockPersonRepository.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("ebf585ad-cc1f-478f-a7a3-aefae87f873a");

            AccountsPayableInvoicesService = new AccountsPayableInvoicesService(mockcolleagueFinanceReferenceDataRepository.Object,
                mockreferenceDataRepository.Object, mockAccountsPayableInvoices.Object, mockaddressRepository.Object, mockvendorsRepository.Object,
                mockGeneralLedgerConfigurationRepository.Object, mockPersonRepository.Object, baseConfigurationRepository,
                adapterRegistry.Object, currentUserFactory, roleRepositoryMock.Object, mockAccountFundsAvailable.Object, loggerObject);

        }
        private void BuildDto()
        {
            _accountsPayableInvoiceDto = new Ellucian.Colleague.Dtos.AccountsPayableInvoices2()
            {
                Id = guid,
                Vendor = new Dtos.DtoProperties.AccountsPayableInvoicesVendorDtoProperty()
                {
                    ExistingVendor = new Dtos.DtoProperties.AccountsPayableInvoicesExistingVendorDtoProperty()
                    {
                        Vendor = new GuidObject2("0123VendorGuid"),
                        AlternativeVendorAddress = new GuidObject2("02344AddressGuid")
                    }
                },
                ReferenceNumber = "refNo012",
                VendorInvoiceNumber = "VIN021",
                TransactionDate = new DateTime(2017, 1, 12),
                VendorInvoiceDate = new DateTime(2017, 1, 12),
                VoidDate = new DateTime(2017, 1, 25),
                ProcessState = Dtos.EnumProperties.AccountsPayableInvoicesProcessState.Inprogress,
                VendorBilledAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 40m, Currency = Dtos.EnumProperties.CurrencyIsoCode.USD },
                InvoiceDiscountAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 5m, Currency = Dtos.EnumProperties.CurrencyIsoCode.USD },
                Taxes = new List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty()
                    {
                        TaxCode = new GuidObject2("TaxCodeGuid"),
                        VendorAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 1m, Currency = Dtos.EnumProperties.CurrencyIsoCode.USD }
                    }
                },
                InvoiceType = Dtos.EnumProperties.AccountsPayableInvoicesInvoiceType.Invoice,
                Payment = new Dtos.DtoProperties.AccountsPayableInvoicesPaymentDtoProperty()
                {
                    Source = new GuidObject2("aptypeguid321"),
                    PaymentDueOn = new DateTime(2017, 1, 17),
                    PaymentTerms = new GuidObject2("termsguid321")
                },
                InvoiceComment = "This is a Comment 321",
                GovernmentReporting = new List<Dtos.DtoProperties.GovernmentReportingDtoProperty>()
                {
                    new Dtos.DtoProperties.GovernmentReportingDtoProperty()
                    {
                        Code = null,
                        TransactionType = null
                    }
                },
                LineItems = new List<Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2()
                    {
                         LineItemNumber = "10",
                         AccountDetails =new List<Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty>()
                         {
                             new Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty()
                             {
                                  AccountingString = "10-10-1000-400",
                                   Allocation = new Dtos.DtoProperties.AccountsPayableInvoicesAllocationDtoProperty()
                                   {
                                        Allocated = new Dtos.DtoProperties.AccountsPayableInvoicesAllocatedDtoProperty()
                                        {
                                             Amount = new Dtos.DtoProperties.Amount2DtoProperty() {
                                                 Value = 10m,
                                                 Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                                             },

                                              Quantity = 2m
                                        }
                                   },
                                   BudgetCheck = Dtos.EnumProperties.AccountsPayableInvoicesAccountBudgetCheck.NotRequired,
                                   SequenceNumber = 1,
                                   Source = new GuidObject2( "asbc-321"),
                                   SubmittedBy = new GuidObject2("submit-guid"),


                             }
                         },
                           Comment = "LineItem comment",
                           ReferenceDocument = new Dtos.DtoProperties.LineItemReferenceDocumentDtoProperty2
                           {
                               PurchaseOrder = new GuidObject2("PurchaseOrderGuid-10")
                           },
                         Description = "line item Description",
                         CommodityCode = new GuidObject2("commodity-guid"),
                         Quantity = 2m,
                         UnitofMeasure = new GuidObject2("unitguid321"),
                         UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() {
                            Value = 5m, Currency = Dtos.EnumProperties.CurrencyIsoCode.USD},
                         Taxes = new List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty>()
                         {
                             new Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty()
                             {
                                  TaxCode = new GuidObject2("taxCode-guid"),
                                  VendorAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                                  {
                                      Value = 1m,
                                      Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                                  }
                             }
                         },
                         Discount = new Dtos.DtoProperties.AccountsPayableInvoicesDiscountDtoProperty()
                         {
                             Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                             {
                                 Value = 1m,
                                 Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                             }
                         }
                        , PaymentStatus = Dtos.EnumProperties.AccountsPayableInvoicesPaymentStatus.Nohold,
                         Status = AccountsPayableInvoicesStatus.Open
                    }
                }
            };
            _accountsPayableInvoiceDto.PaymentStatus = AccountsPayableInvoicesPaymentStatus.Nohold;
        }

        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices ConvertVoucherEntityToAPI(Voucher voucher)
        {
            Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices NewApi = new Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices(voucher.Id, voucher.Date, voucher.Status, voucher.VendorName, voucher.InvoiceNumber, voucher.InvoiceDate);
            NewApi.Guid = guid;
            NewApi.Amount = voucher.Amount;

            foreach (var appr in voucher.Approvers)
            {
                Approver approver = new Approver(appr.ApproverId);
                approver.ApprovalDate = appr.ApprovalDate;
                approver.SetApprovalName(appr.ApprovalName);
                NewApi.AddApprover(approver);
            }
            NewApi.ApType = voucher.ApType;
            NewApi.CheckDate = voucher.CheckDate;
            NewApi.CheckNumber = (string.IsNullOrEmpty(voucher.CheckNumber) ? null : voucher.CheckNumber);
            NewApi.Comments = voucher.Comments;
            NewApi.CurrencyCode = voucher.CurrencyCode;
            NewApi.DueDate = voucher.DueDate;
            NewApi.MaintenanceDate = voucher.MaintenanceDate;
            NewApi.VendorAddressId = "00001";
            NewApi.VendorId = voucher.VendorId;
            NewApi.VoucherAddressId = "000002";
            NewApi.VoucherInvoiceAmt = 20m;
            NewApi.VoucherDiscAmt = 10m;
            NewApi.VoucherNet = 20m;
            NewApi.VoucherPayFlag = "Y";
            NewApi.VoucherReferenceNo = new List<string>() { "RefNo1111" };
            NewApi.VoucherStatusDate = new DateTime(2017, 1, 11);
            NewApi.VoucherVendorTerms = "testTerm";
            NewApi.VoucherVoidGlTranDate = new DateTime(2018, 1, 11);
            NewApi.VoucherTaxes = new List<LineItemTax>() { new LineItemTax("ST", 3m) };
            NewApi.VoucherVendorTerms = "02";
            NewApi.VoucherUseAltAddress = true;

            foreach (var lineItem in voucher.LineItems)
            {
                AccountsPayableInvoicesLineItem newLi = new AccountsPayableInvoicesLineItem(lineItem.Id, lineItem.Description, lineItem.Quantity, lineItem.Price, lineItem.ExtendedPrice);
                newLi.CommodityCode = "00402";
                newLi.UnitOfIssue = lineItem.UnitOfIssue;
                newLi.PurchaseOrderId = "PurchaseOrderGuid-123";
                foreach (var tax in lineItem.LineItemTaxes)
                {
                    newLi.AddTax(tax);
                }
                foreach (var gl in lineItem.GlDistributions)
                {
                    newLi.AddGlDistribution(gl);
                }
                newLi.CashDiscountAmount = 0m;
                newLi.TradeDiscountAmount = 10m;

                newLi.Comments = lineItem.Comments;
                NewApi.AddAccountsPayableInvoicesLineItem(newLi);
            }

            return NewApi;
        }

        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices ConvertVoucherEntityToAPI2(Voucher voucher)
        {
            Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices NewApi = new Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices(voucher.Id, voucher.Date, voucher.Status, voucher.VendorName, voucher.InvoiceNumber, voucher.InvoiceDate);
            NewApi.Guid = guid;
            NewApi.Amount = voucher.Amount;

            foreach (var appr in voucher.Approvers)
            {
                Approver approver = new Approver(appr.ApproverId);
                approver.ApprovalDate = appr.ApprovalDate;
                approver.SetApprovalName(appr.ApprovalName);
                NewApi.AddApprover(approver);
            }
            NewApi.ApType = voucher.ApType;
            NewApi.CheckDate = voucher.CheckDate;
            NewApi.CheckNumber = (string.IsNullOrEmpty(voucher.CheckNumber) ? null : voucher.CheckNumber);
            NewApi.Comments = voucher.Comments;
            NewApi.CurrencyCode = voucher.CurrencyCode;
            NewApi.DueDate = voucher.DueDate;
            NewApi.MaintenanceDate = voucher.MaintenanceDate;
            NewApi.VendorAddressId = "00001";
            NewApi.VendorId = voucher.VendorId;
            NewApi.VoucherAddressId = "000002";
            NewApi.VoucherInvoiceAmt = 20m;
            NewApi.VoucherDiscAmt = 10m;
            NewApi.VoucherNet = 20m;
            NewApi.VoucherPayFlag = "Y";
            NewApi.VoucherReferenceNo = new List<string>() { "RefNo1111" };
            NewApi.VoucherStatusDate = new DateTime(2017, 1, 11);
            NewApi.VoucherVendorTerms = "testTerm";
            NewApi.VoucherVoidGlTranDate = new DateTime(2018, 1, 11);
            NewApi.VoucherTaxes = new List<LineItemTax>() { new LineItemTax("ST", 3m) };
            NewApi.VoucherVendorTerms = "02";
            NewApi.VoucherUseAltAddress = true;

            foreach (var lineItem in voucher.LineItems)
            {
                AccountsPayableInvoicesLineItem newLi = new AccountsPayableInvoicesLineItem(lineItem.Id, lineItem.Description, lineItem.Quantity, lineItem.Price, lineItem.ExtendedPrice);
                newLi.CommodityCode = "00402";
                newLi.UnitOfIssue = lineItem.UnitOfIssue;
                newLi.PurchaseOrderId = string.Format("PurchaseOrderGuid-{0}", lineItem.Id);
                foreach (var tax in lineItem.LineItemTaxes)
                {
                    newLi.AddTax(tax);
                }
                foreach (var gl in lineItem.GlDistributions)
                {
                    newLi.AddGlDistribution(gl);
                }
                newLi.CashDiscountAmount = 0m;
                newLi.TradeDiscountAmount = 10m;
                newLi.Comments = lineItem.Comments;
                NewApi.AddAccountsPayableInvoicesLineItem(newLi);
            }

            return NewApi;
        }
    }


    [TestClass]
    public class AccountsPayableInvoicesServiceTests_POST : GeneralLedgerCurrentUser
    {
        #region DECLARATION

        protected Domain.Entities.Role viewAccountsPayableInvoicesRole = new Domain.Entities.Role(1, "VIEW.AP.INVOICES");
        protected Domain.Entities.Role updateAccountsPayableInvoicesRole = new Domain.Entities.Role(1, "UPDATE.AP.INVOICES");

        private AccountsPayableInvoicesService accountsPayableInvoicesService;

        private Mock<IAccountsPayableInvoicesRepository> accountsPayableInvoicesMock;
        private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IAddressRepository> addressRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IVendorsRepository> vendorsRepositoryMock;
        private Mock<IAccountFundsAvailableRepository> accountFundsAvailableMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
        private Mock<ILogger> loggerMock;

        private AccountFundsAvailableUser currentUserFactory;

        private Dtos.AccountsPayableInvoices2 accountsPayableInvoice;
        private Domain.ColleagueFinance.Entities.AccountsPayableInvoices accountsPayableInvoiceEntity;
        private Dtos.DtoProperties.AccountsPayableInvoicesVendorDtoProperty vendor;
        private Dtos.DtoProperties.AccountsPayableInvoicesPaymentDtoProperty payment;
        private List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty> taxes;
        private List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty> lineItemTaxes;
        private List<Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2> lineItems;
        private List<Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty> accountDetails;
        private List<Domain.ColleagueFinance.Entities.AccountsPayableSources> accountsPayableSources;
        private List<Domain.ColleagueFinance.Entities.VendorTerm> vendorTerms;
        private List<Domain.ColleagueFinance.Entities.CommodityCode> commodityCodes;
        private List<Domain.ColleagueFinance.Entities.CommodityUnitType> commodityUnitTypes;
        private List<Domain.Base.Entities.CommerceTaxCode> taxCodes;
        private List<Domain.Base.Entities.Country> countries;
        private List<Domain.Base.Entities.State> states;
        private Domain.Base.Entities.Address addressEntity;
        private AccountsPayableInvoicesLineItem lineItem;
        private List<Domain.ColleagueFinance.Entities.FundsAvailable> fundsAvailable;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private GeneralLedgerAccountStructure testGlAccountStructure;

        private Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices> accountsPayableInvoicesEntities = new Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>();

        string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            accountsPayableInvoicesMock = new Mock<IAccountsPayableInvoicesRepository>();
            colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            addressRepositoryMock = new Mock<IAddressRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            vendorsRepositoryMock = new Mock<IVendorsRepository>();
            accountFundsAvailableMock = new Mock<IAccountFundsAvailableRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            loggerMock = new Mock<ILogger>();
            currentUserFactory = new GeneralLedgerCurrentUser.AccountFundsAvailableUser();

            InitializeTestData();

            InitializeMock();

            accountsPayableInvoicesService = new AccountsPayableInvoicesService(colleagueFinanceReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object,
                accountsPayableInvoicesMock.Object, addressRepositoryMock.Object, vendorsRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object,
                personRepositoryMock.Object, baseConfigurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object,
                accountFundsAvailableMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            accountsPayableInvoicesMock = null;
            colleagueFinanceReferenceDataRepositoryMock = null;
            referenceDataRepositoryMock = null;
            generalLedgerConfigurationRepositoryMock = null;
            personRepositoryMock = null;
            addressRepositoryMock = null;
            adapterRegistryMock = null;
            vendorsRepositoryMock = null;
            accountFundsAvailableMock = null;
            roleRepositoryMock = null;
            baseConfigurationRepositoryMock = null;
            loggerMock = null;

            currentUserFactory = null;

            accountsPayableInvoicesService = null;
        }

        private async void InitializeTestData()
        {
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGlAccountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>()
                {
                    new Domain.ColleagueFinance.Entities.FundsAvailable("1")
                    {
                        AvailableStatus = FundsAvailableStatus.Override,
                        SubmittedBy = "1",
                        Sequence = "1.1.DS"
                    }
                };

            states = new List<Domain.Base.Entities.State>()
                {
                    new Domain.Base.Entities.State("SA", "sa", "USA"),
                    new Domain.Base.Entities.State("VA", "canada", "CAN"),
                    new Domain.Base.Entities.State("AU", "australia", "AUS")
                };

            countries = new List<Domain.Base.Entities.Country>()
                {
                    new Domain.Base.Entities.Country("USA", "United states", "USA", "USA"),
                    new Domain.Base.Entities.Country("CAN", "Canada", "CAN", "CAN"),
                    new Domain.Base.Entities.Country("AUS", "Australia", "AUS", "AUS"),
                    new Domain.Base.Entities.Country("BRA", "Brazil", "BRA", "BRA"),
                    new Domain.Base.Entities.Country("MEX", "Mexico", "MEX", "MEX"),
                    new Domain.Base.Entities.Country("NLD", "Netherlands", "NLD", "NLD"),
                    new Domain.Base.Entities.Country("GBR", "London", "GBR", "GBR"),
                    new Domain.Base.Entities.Country("JP", "Japan", "JPN", "JPN"),
                    new Domain.Base.Entities.Country("JP1", "Japan", "JPN", "JPN")
                };

            addressEntity = new Domain.Base.Entities.Address()
            {
                Guid = guid,
                AddressLines = new List<string>() { "12245 Spring Hill Blvd." },
                City = "Denver",
                State = "SA",
                PostalCode = "88495"
            };

            taxCodes = new List<Domain.Base.Entities.CommerceTaxCode>()
                {
                    new Domain.Base.Entities.CommerceTaxCode(guid, "1", "desc") { AppurEntryFlag = true }
                };

            commodityUnitTypes = new List<Domain.ColleagueFinance.Entities.CommodityUnitType>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityUnitType(guid, "1", "desc")
                };

            commodityCodes = new List<Domain.ColleagueFinance.Entities.CommodityCode>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityCode(guid, "1", "desc")
                };

            vendorTerms = new List<VendorTerm>()
                {
                    new VendorTerm(guid, "1", "desc")
                };

            accountsPayableSources = new List<Domain.ColleagueFinance.Entities.AccountsPayableSources>()
                {
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources(guid, "1", "desc")
                };

            vendor = new Dtos.DtoProperties.AccountsPayableInvoicesVendorDtoProperty()
            {
                ExistingVendor = new Dtos.DtoProperties.AccountsPayableInvoicesExistingVendorDtoProperty()
                {
                    Vendor = new GuidObject2(guid),
                    AlternativeVendorAddress = new GuidObject2(guid)
                },

            };

            payment = new Dtos.DtoProperties.AccountsPayableInvoicesPaymentDtoProperty()
            {
                Source = new GuidObject2(guid),
                PaymentDueOn = DateTime.Today,
                PaymentTerms = new GuidObject2(guid)
            };

            taxes = new List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty()
                    {
                        TaxCode = new GuidObject2(guid),
                        VendorAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD }
                    }
                };

            lineItemTaxes = new List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty()
                    {
                        TaxCode = new GuidObject2(guid),
                        VendorAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD }
                    }
                };

            accountDetails = new List<Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty()
                    {
                        Allocation = new Dtos.DtoProperties.AccountsPayableInvoicesAllocationDtoProperty()
                        {
                            Allocated = new Dtos.DtoProperties.AccountsPayableInvoicesAllocatedDtoProperty()
                            {
                                Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                                Quantity = 10
                            },
                            TaxAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                            DiscountAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                            AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD }
                        },
                        Source = new GuidObject2(guid),
                        SubmittedBy = new GuidObject2(guid),
                        AccountingString = "1",
                        BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override,
                        SequenceNumber = 1

                    }
                };

            lineItems = new List<Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2()
                    {
                        LineItemNumber = "1",
                        ReferenceDocumentLineItemNumber = "1",
                        CommodityCode = new GuidObject2(guid),
                        UnitofMeasure = new GuidObject2(guid),
                        UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                        VendorBilledUnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 105, Currency = CurrencyIsoCode.USD },
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 110, Currency = CurrencyIsoCode.USD },
                        Taxes = lineItemTaxes,
                        AccountDetails = accountDetails,
                        Description = "description",
                        Quantity = 2,
                        PaymentStatus = AccountsPayableInvoicesPaymentStatus.Nohold,
                        Status = AccountsPayableInvoicesStatus.Open,
                        Discount = new Dtos.DtoProperties.AccountsPayableInvoicesDiscountDtoProperty()
                        {
                            Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 120, Currency = CurrencyIsoCode.USD },

                        },
                        ReferenceDocument = new Dtos.DtoProperties.LineItemReferenceDocumentDtoProperty2()
                        {
                            PurchaseOrder = new GuidObject2(guid),
                        }
                    }
                };

            accountsPayableInvoice = new Dtos.AccountsPayableInvoices2()
            {
                Id = guid,
                InvoiceNumber = "1",
                TransactionDate = DateTime.Today,
                Vendor = vendor,
                VendorInvoiceNumber = "1",
                VendorInvoiceDate = DateTime.Today,
                ReferenceNumber = "1",
                InvoiceComment = "comment",
                SubmittedBy = new GuidObject2(guid),
                ProcessState = AccountsPayableInvoicesProcessState.Inprogress,
                Payment = payment,
                PaymentStatus = AccountsPayableInvoicesPaymentStatus.Hold,
                VendorBilledAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 1000, Currency = CurrencyIsoCode.USD },
                InvoiceDiscountAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                Taxes = taxes,
                LineItems = lineItems
            };

            accountsPayableInvoiceEntity = new Domain.ColleagueFinance.Entities.AccountsPayableInvoices(guid, "1", DateTime.Today, VoucherStatus.InProgress, "name", "1", DateTime.Today)
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
                ApType = "1",
                DueDate = DateTime.Today.AddDays(10),
                VoucherVendorTerms = "1",
                Comments = "comments",
                PurchaseOrderId = "1",
                VoucherTaxes = new List<LineItemTax>() { new LineItemTax("1", 50) }

            };

            lineItem = new AccountsPayableInvoicesLineItem("1", "desc", 10, 1000, 100)
            {
                CommodityCode = "1",
                UnitOfIssue = "1",
                CashDiscountAmount = 10,
                TradeDiscountAmount = 5,
                Comments = "comments",

                AccountsPayableLineItemTaxes = new List<LineItemTax>()
                    {
                        new LineItemTax("1", 10)
                        {
                            LineGlNumber = "1"
                        }
                    }
            };

            lineItem.AddGlDistribution(new LineItemGlDistribution("11-00-02-67-60000-54005", 10, 1000, 1)
            {
                ProjectId = "1",

            });

            accountsPayableInvoiceEntity.AddAccountsPayableInvoicesLineItem(lineItem);
        }

        private void InitializeMock()
        {
            updateAccountsPayableInvoicesRole.AddPermission(new Domain.Entities.Permission(Domain.ColleagueFinance.ColleagueFinancePermissionCodes.UpdateApInvoices));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { updateAccountsPayableInvoicesRole });

            accountFundsAvailableMock.Setup(p => p.CheckAvailableFundsAsync(It.IsAny<List<Domain.ColleagueFinance.Entities.FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(fundsAvailable);
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            vendorsRepositoryMock.Setup(p => p.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            vendorsRepositoryMock.Setup(p => p.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            addressRepositoryMock.Setup(p => p.GetAddressFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            addressRepositoryMock.Setup(p => p.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            addressRepositoryMock.Setup(p => p.GetAddressAsync(It.IsAny<string>())).ReturnsAsync(addressEntity);
            addressRepositoryMock.Setup(p => p.GetHostCountryAsync()).ReturnsAsync("USA");
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetAccountsPayableSourcesAsync(true)).ReturnsAsync(accountsPayableSources);
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetVendorTermsAsync(true)).ReturnsAsync(vendorTerms);
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityCodesAsync(true)).ReturnsAsync(commodityCodes);
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityUnitTypesAsync(true)).ReturnsAsync(commodityUnitTypes);
            accountsPayableInvoicesMock.Setup(p => p.GetAccountsPayableInvoicesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            referenceDataRepositoryMock.Setup(p => p.GetCommerceTaxCodesAsync(true)).ReturnsAsync(taxCodes);
            referenceDataRepositoryMock.Setup(p => p.GetCountryCodesAsync(false)).ReturnsAsync(countries);
            referenceDataRepositoryMock.Setup(p => p.GetStateCodesAsync(false)).ReturnsAsync(states);
            accountsPayableInvoicesMock.Setup(p => p.CreateAccountsPayableInvoicesAsync(It.IsAny<Domain.ColleagueFinance.Entities.AccountsPayableInvoices>())).ReturnsAsync(accountsPayableInvoiceEntity);
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            accountsPayableInvoicesMock.Setup(p => p.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { Entity = "PURCHASE.ORDERS", PrimaryKey = "1" });
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Is_Null()
        {
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Id_Is_Null()
        {
            accountsPayableInvoice.Id = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Vendor_Is_Null()
        {
            accountsPayableInvoice.Vendor = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_ExistingVendorId_Is_Null()
        {
            accountsPayableInvoice.Vendor.ExistingVendor.Vendor.Id = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_ManualVendorName_Is_Null()
        {
            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty() { Name = null };
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_ManualVendorCountry_Is_Null()
        {
            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty()
            { Name = "Acme Tools", Place = new AddressPlace() { } };
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_When_Dto_Vendor_Is_Not_Correct()
        {
            vendorsRepositoryMock.Setup(p => p.GetVendorIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Vendor_Id_Is_Null()
        {
            vendorsRepositoryMock.Setup(p => p.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Invalid_Address_Guid()
        {
            addressRepositoryMock.Setup(p => p.GetAddressFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_ManualVendorCounttryCode_Is_Null()
        {
            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty()
            { Name = "Acme Tools",
                Place = new AddressPlace() { Country = new AddressCountry { Title = "invalid" } } };
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_AllCountires_Is_Null()
        {
            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty()
            {
                Name = "Acme Tools",
                Place = new AddressPlace() { Country = new AddressCountry { Title = "invalid" } }
            };
            referenceDataRepositoryMock.Setup(p => p.GetCountryCodesAsync(false)).ReturnsAsync(null);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        public async Task ActPayInvService_POST_When_Dto_ManualVendorCounttryCode_Is_Mapped_two_Contries()
        {
            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty()
            {
                Name = "Acme Tools",
                Place = new AddressPlace() { Country = new AddressCountry { Code = IsoCode.JPN } }
            };
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_ProcessState_Is_NotSet()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.NotSet;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_PaymentStatus_Is_NotSet()
        {
            accountsPayableInvoice.PaymentStatus = AccountsPayableInvoicesPaymentStatus.NotSet;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Payment_Is_Null()
        {
            accountsPayableInvoice.Payment = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Payment_Source_Is_Null()
        {
            accountsPayableInvoice.Payment.Source = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Payment_PaymentDueOn_Is_Null()
        {
            accountsPayableInvoice.Payment.PaymentDueOn = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Payment_SourceId_Is_Null()
        {
            accountsPayableInvoice.Payment.Source.Id = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_Payment_PaymentTermsId_Is_Null()
        {
            accountsPayableInvoice.Payment.PaymentTerms.Id = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_VendorBilledAmount_Value_Is_Null()
        {
            accountsPayableInvoice.VendorBilledAmount.Value = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_VendorBilledAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.VendorBilledAmount.Currency = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_InvoiceDiscountAmount_Value_Is_Null()
        {
            accountsPayableInvoice.InvoiceDiscountAmount.Value = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_InvoiceDiscountAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.InvoiceDiscountAmount.Currency = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_When_Dto_VoidDate_LesserThan_TransactionDate()
        {
            accountsPayableInvoice.VoidDate = DateTime.Now.AddDays(-10);
            accountsPayableInvoice.TransactionDate = DateTime.Today;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_When_DefaultCurreny_NotMatching_With_Currency()
        {
            accountsPayableInvoice.InvoiceDiscountAmount.Currency = CurrencyIsoCode.AUD;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_TaxCodeId_Is_Null()
        {
            accountsPayableInvoice.Taxes.FirstOrDefault().TaxCode.Id = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_TaxVendorAmount_Is_Null()
        {
            accountsPayableInvoice.Taxes.FirstOrDefault().VendorAmount.Value = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_TaxVendorAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.Taxes.FirstOrDefault().VendorAmount.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_Are_Null()
        {
            accountsPayableInvoice.LineItems = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_CommodityCodeId_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().CommodityCode.Id = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_UnitofMeasureId_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitofMeasure.Id = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_UnitPriceValue_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitPrice.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_UnitPriceCurrency_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitPrice.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_VendorBilledUnitPriceValue_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().VendorBilledUnitPrice.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_VendorBilledUnitPriceCurrency_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().VendorBilledUnitPrice.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_AdditionalAmountValue_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AdditionalAmount.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItems_AdditionalAmountCurrency_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AdditionalAmount.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_TaxCode_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Taxes.FirstOrDefault().TaxCode = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_TaxCodeId_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Taxes.FirstOrDefault().TaxCode.Id = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_TaxVendorAmount_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Taxes.FirstOrDefault().VendorAmount.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_TaxVendorAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Taxes.FirstOrDefault().VendorAmount.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_DiscountAmount_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Amount.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_DiscountPercentageAmount_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Percent = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Amount.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_DiscountPercentageAmount_Not_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Percent = 1m;
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Amount.Value = 2m;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_DiscountAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Amount.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AllocatedAmountValue_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.Allocated.Amount.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AllocatedAmountCurrency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.Allocated.Amount.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AllocationTaxAmountValue_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.TaxAmount.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AllocationTaxAmountCurrency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.TaxAmount.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AllocationAdditionalAmountValue_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.AdditionalAmount.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AllocationAdditionalAmountCurrency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.AdditionalAmount.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AllocationDiscountAmountValue_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.DiscountAmount.Value = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AllocationDiscountAmountCurrency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.DiscountAmount.Currency = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_SourceId_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Source.Id = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_SubmittedById_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().SubmittedBy.Id = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_AccountingString_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().AccountingString = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_Allocation_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_BudgetOverride_NoBudgetCheckOverrideFlag()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.NotSet;
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().SubmittedBy = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_AccountDetails_BudgetCheckOverride_And_SubmittedById_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().SubmittedBy.Id = null;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_Description_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Description = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_Quantity_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Quantity = 0;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_UnitPrice_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitPrice = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_PaymentStatus_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().PaymentStatus = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_Status_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Status = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ActPayInvService_POST_PermissionException()
        {
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_POST_Multiple_AccountDetails_With_Same_AccountingString()
        {
            var accountDetail = accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault();
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.Add(accountDetail);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        //invalid process state is ignored
        //[TestMethod]
        //[ExpectedException(typeof(Exception))]
        //public async Task ActPayInvService_POST_DtoToEntity_Invalid_ProcessState()
        //{
        //    accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Awaitingreceipt;
        //    await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_DtoToEntity_From_VendorRepository()
        {
            vendorsRepositoryMock.Setup(p => p.GetVendorIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_DtoToEntity_VendorId_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Notapproved;
            vendorsRepositoryMock.Setup(p => p.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_DtoToEntity_Vendor_AlternateAddress_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Outstanding;
            addressRepositoryMock.Setup(p => p.GetAddressFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_POST_DtoToEntity_PaymentSource_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Paid;
            accountsPayableInvoice.Payment.Source.Id = Guid.NewGuid().ToString();
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_POST_DtoToEntity_PaymentTerms_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Reconciled;
            accountsPayableInvoice.Payment.PaymentTerms.Id = Guid.NewGuid().ToString();
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_DtoToEntity_SubmittedBy_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Voided;
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_BlanketPurchaseOrder_Is_NotNull()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.BlanketPurchaseOrder = new GuidObject2(guid);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_PurchasingArrangemen_Is_NotNull()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.PurchasingArrangement = new GuidObject2(guid);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_Dto_LineItem_RecurringVoucher_Is_NotNull()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.RecurringVoucher = guid;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_POST_DtoToEntity_Repository_Returns_CommodityCodes_As_Null()
        {
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityCodesAsync(true)).ReturnsAsync(null);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_POST_DtoToEntity_Repository_Returns_CommodityCodes_As_Empty()
        {
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityCodesAsync(true)).ReturnsAsync(new List<Domain.ColleagueFinance.Entities.CommodityCode>());
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_POST_DtoToEntity_Repository_Returns_CommodityUnitTypes_As_Empty()
        {
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityUnitTypesAsync(true)).ReturnsAsync(new List<Domain.ColleagueFinance.Entities.CommodityUnitType>());
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_POST_DtoToEntity_Repository_Returns_CommodityUnitTypes_As_Null()
        {
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityUnitTypesAsync(true)).ReturnsAsync(null);
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_DtoToEntity_LineItemNumber_NotSameAs_ReferenceLineItemNumber()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocumentLineItemNumber = "2";
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_DtoToEntity_LineItem_CommodityCode_NotFound()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.PurchaseOrder = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.BlanketPurchaseOrder = new GuidObject2(guid);
            accountsPayableInvoice.LineItems.FirstOrDefault().CommodityCode.Id = Guid.NewGuid().ToString();

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_POST_DtoToEntity_LineItem_UnitOfMeasure_NotFound()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocumentLineItemNumber = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.PurchaseOrder = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.RecurringVoucher = guid;
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitofMeasure.Id = Guid.NewGuid().ToString();

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_EntityToDto_Vendor_NotFound()
        {
            vendorsRepositoryMock.Setup(p => p.GetVendorGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            accountsPayableInvoiceEntity.VoucherMiscName = new List<string>();

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_EntityToDto_VendorId_As_Null()
        {
            accountsPayableInvoiceEntity.CurrencyCode = string.Empty;
            accountsPayableInvoiceEntity.HostCountry = "CAN";
            accountsPayableInvoiceEntity.VendorId = string.Empty;
            accountsPayableInvoiceEntity.VoucherMiscName = new List<string>();

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_EntityToDto_VoucherMiscCountry_NotFound()
        {
            var manualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty()
            {
                Name = "name",
                Type = AccountsPayableInvoicesVendorType.Organization,
                AddressLines = new List<string>() { "address" },
                Place = new AddressPlace()
                {
                    Country = new AddressCountry()
                    {
                        Code = IsoCode.NER,
                        Locality = "NER",
                        PostalCode = "12345",
                        Region = new AddressRegion()
                        {
                            Code = "NR-XX",
                        }
                    }
                }
            };

            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = manualVendorDetails;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_EntityToDto_StateCode_NotFound()
        {
            var manualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty()
            {
                Name = "name",
                Type = AccountsPayableInvoicesVendorType.Organization,
                AddressLines = new List<string>() { "address" },
                Place = new AddressPlace()
                {
                    Country = new AddressCountry()
                    {
                        Code = IsoCode.USA,
                        Locality = "USA",
                        PostalCode = "12345",
                        Region = new AddressRegion()
                        {
                            Code = "US-XX",
                        }
                    }
                }
            };

            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = manualVendorDetails;

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_Funds_NotAvailable()
        {
            accountsPayableInvoiceEntity.CurrencyCode = "A12";
            accountsPayableInvoiceEntity.HostCountry = "CAN";
            fundsAvailable.FirstOrDefault().AvailableStatus = FundsAvailableStatus.NotAvailable;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_POST_AvailableFunds_SubmittedId_NotFound()
        {
            accountsPayableInvoiceEntity.CurrencyCode = "A12";
            accountsPayableInvoiceEntity.HostCountry = "USA";
            accountsPayableInvoiceEntity.VoucherMiscCountry = String.Empty;
            accountsPayableInvoiceEntity.VoucherMiscState = "SA";

            personRepositoryMock.SetupSequence(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(guid)).Returns(Task.FromResult<string>(null));

            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_POST_RepositoryException()
        {

            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
            accountsPayableInvoiceEntity.CurrencyCode = String.Empty;
            accountsPayableInvoiceEntity.HostCountry = "USA";
            accountsPayableInvoiceEntity.VoucherMiscCountry = String.Empty;
            accountsPayableInvoiceEntity.VoucherMiscState = "SA";
            accountsPayableInvoice.LineItems[0].ReferenceDocument = null;
            accountsPayableInvoicesMock.Setup(p => p.CreateAccountsPayableInvoicesAsync(It.IsAny<Domain.ColleagueFinance.Entities.AccountsPayableInvoices>())).ThrowsAsync(new RepositoryException());
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }

        [TestMethod]
        public async Task ActPayInvService_POST()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            accountsPayableInvoice.LineItems[0].ReferenceDocument = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
            var result = await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);

            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_POST_InvalidTaxCode()
        {
            var taxCodesApPur = new List<Domain.Base.Entities.CommerceTaxCode>()
            {
                    new Domain.Base.Entities.CommerceTaxCode(guid, "1", "desc") { AppurEntryFlag = false }
            };
            referenceDataRepositoryMock.Setup(p => p.GetCommerceTaxCodesAsync(true)).ReturnsAsync(taxCodesApPur);

            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            accountsPayableInvoice.LineItems[0].ReferenceDocument = null;
            await accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoice);
        }
    }

    [TestClass]
    public class AccountsPayableInvoicesServiceTests_PUT : GeneralLedgerCurrentUser
    {
        #region DECLARATION

        protected Domain.Entities.Role viewAccountsPayableInvoicesRole = new Domain.Entities.Role(1, "VIEW.AP.INVOICES");
        protected Domain.Entities.Role updateAccountsPayableInvoicesRole = new Domain.Entities.Role(1, "UPDATE.AP.INVOICES");

        private AccountsPayableInvoicesService accountsPayableInvoicesService;

        private Mock<IAccountsPayableInvoicesRepository> accountsPayableInvoicesMock;
        private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IAddressRepository> addressRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IVendorsRepository> vendorsRepositoryMock;
        private Mock<IAccountFundsAvailableRepository> accountFundsAvailableMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
        private Mock<ILogger> loggerMock;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;

        private AccountFundsAvailableUser currentUserFactory;

        private Dtos.AccountsPayableInvoices2 accountsPayableInvoice;
        private Domain.ColleagueFinance.Entities.AccountsPayableInvoices accountsPayableInvoiceEntity;
        private Dtos.DtoProperties.AccountsPayableInvoicesVendorDtoProperty vendor;
        private Dtos.DtoProperties.AccountsPayableInvoicesPaymentDtoProperty payment;
        private List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty> taxes;
        private List<Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2> lineItems;
        private List<Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty> accountDetails;
        private List<Domain.ColleagueFinance.Entities.AccountsPayableSources> accountsPayableSources;
        private List<Domain.ColleagueFinance.Entities.VendorTerm> vendorTerms;
        private List<Domain.ColleagueFinance.Entities.CommodityCode> commodityCodes;
        private List<Domain.ColleagueFinance.Entities.CommodityUnitType> commodityUnitTypes;
        private List<Domain.Base.Entities.CommerceTaxCode> taxCodes;
        private List<Domain.Base.Entities.Country> countries;
        private List<Domain.Base.Entities.State> states;
        private AccountsPayableInvoicesLineItem lineItem;
        private List<Domain.ColleagueFinance.Entities.FundsAvailable> fundsAvailable;
        private Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty manualVendorDetails;
        private GeneralLedgerAccountStructure testGlAccountStructure;

        private Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices> accountsPayableInvoicesEntities = new Collection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices>();

        string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            accountsPayableInvoicesMock = new Mock<IAccountsPayableInvoicesRepository>();
            colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            addressRepositoryMock = new Mock<IAddressRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            vendorsRepositoryMock = new Mock<IVendorsRepository>();
            accountFundsAvailableMock = new Mock<IAccountFundsAvailableRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            loggerMock = new Mock<ILogger>();

            currentUserFactory = new GeneralLedgerCurrentUser.AccountFundsAvailableUser();

            InitializeTestData();

            InitializeMock();

            accountsPayableInvoicesService = new AccountsPayableInvoicesService(colleagueFinanceReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object,
                accountsPayableInvoicesMock.Object, addressRepositoryMock.Object, vendorsRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object,
                personRepositoryMock.Object, baseConfigurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object,
                accountFundsAvailableMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            accountsPayableInvoicesMock = null;
            colleagueFinanceReferenceDataRepositoryMock = null;
            referenceDataRepositoryMock = null;
            generalLedgerConfigurationRepositoryMock = null;
            personRepositoryMock = null;
            addressRepositoryMock = null;
            adapterRegistryMock = null;
            vendorsRepositoryMock = null;
            accountFundsAvailableMock = null;
            roleRepositoryMock = null;
            baseConfigurationRepositoryMock = null;
            loggerMock = null;

            currentUserFactory = null;

            accountsPayableInvoicesService = null;
        }

        private async void InitializeTestData()
        {
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGlAccountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>()
                {
                    new Domain.ColleagueFinance.Entities.FundsAvailable("1")
                    {
                        AvailableStatus = FundsAvailableStatus.Override,
                        SubmittedBy = "1",
                        Sequence = "1.1.DS"
                    }
                };

            states = new List<Domain.Base.Entities.State>()
                {
                    new Domain.Base.Entities.State("SA", "sa", "USA"),
                    new Domain.Base.Entities.State("VA", "canada", "CAN"),
                    new Domain.Base.Entities.State("AU", "australia", "AUS")
                };

            countries = new List<Domain.Base.Entities.Country>()
                {
                    new Domain.Base.Entities.Country("USA", "United states", "US", "USA"),
                    new Domain.Base.Entities.Country("CAN", "Canada", "CA", "CAN"),
                    new Domain.Base.Entities.Country("AUS", "Australia", "AU", "AUS"),
                    new Domain.Base.Entities.Country("BRA", "Brazil", "BR", "BRA"),
                    new Domain.Base.Entities.Country("MEX", "Mexico", "MX", "MEX"),
                    new Domain.Base.Entities.Country("NLD", "Netherlands", "ND", "NLD"),
                    new Domain.Base.Entities.Country("GBR", "London", "GB", "GBR"),
                    new Domain.Base.Entities.Country("JPN", "Japan", "JP", "JPN"),
                    new Domain.Base.Entities.Country("JPN1", "Japan", "JP", "JPN",true)
                };

            taxCodes = new List<Domain.Base.Entities.CommerceTaxCode>()
                {
                    new Domain.Base.Entities.CommerceTaxCode(guid, "1", "desc") { AppurEntryFlag = true }
                };

            commodityUnitTypes = new List<Domain.ColleagueFinance.Entities.CommodityUnitType>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityUnitType(guid, "1", "desc")
                };

            commodityCodes = new List<Domain.ColleagueFinance.Entities.CommodityCode>()
                {
                    new Domain.ColleagueFinance.Entities.CommodityCode(guid, "1", "desc")
                };

            vendorTerms = new List<VendorTerm>()
                {
                    new VendorTerm(guid, "1", "desc")
                };

            accountsPayableSources = new List<Domain.ColleagueFinance.Entities.AccountsPayableSources>()
                {
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources(guid, "1", "desc")
                };

            vendor = new Dtos.DtoProperties.AccountsPayableInvoicesVendorDtoProperty()
            {
                ExistingVendor = new Dtos.DtoProperties.AccountsPayableInvoicesExistingVendorDtoProperty()
                {
                    Vendor = new GuidObject2(guid),
                    AlternativeVendorAddress = new GuidObject2(guid)
                },

            };

            payment = new Dtos.DtoProperties.AccountsPayableInvoicesPaymentDtoProperty()
            {
                Source = new GuidObject2(guid),
                PaymentDueOn = DateTime.Today,
                PaymentTerms = new GuidObject2(guid)
            };

            taxes = new List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty()
                    {
                        TaxCode = new GuidObject2(guid),
                        VendorAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD }
                    }
                };

            accountDetails = new List<Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty()
                    {
                        Allocation = new Dtos.DtoProperties.AccountsPayableInvoicesAllocationDtoProperty()
                        {
                            Allocated = new Dtos.DtoProperties.AccountsPayableInvoicesAllocatedDtoProperty()
                            {
                                Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                                Quantity = 10

                            },
                            TaxAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                            DiscountAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                            AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD }
                        },
                        Source = new GuidObject2(guid),
                        SubmittedBy = new GuidObject2(guid),
                        AccountingString = "1",
                        BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override,
                        SequenceNumber = 1
                    }
                };

            lineItems = new List<Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2()
                    {
                        LineItemNumber = "1",
                        ReferenceDocumentLineItemNumber = "1",
                        CommodityCode = new GuidObject2(guid),
                        UnitofMeasure = new GuidObject2(guid),
                        UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                        VendorBilledUnitPrice = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 105, Currency = CurrencyIsoCode.USD },
                        AdditionalAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 110, Currency = CurrencyIsoCode.USD },
                        Taxes = taxes,
                        AccountDetails = accountDetails,
                        Description = "description",
                        Quantity = 2,
                        PaymentStatus = AccountsPayableInvoicesPaymentStatus.Nohold,
                        Status = AccountsPayableInvoicesStatus.Open,
                        Discount = new Dtos.DtoProperties.AccountsPayableInvoicesDiscountDtoProperty()
                        {
                            Amount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 120, Currency = CurrencyIsoCode.USD },

                        },
                        ReferenceDocument = new Dtos.DtoProperties.LineItemReferenceDocumentDtoProperty2()
                        {
                            PurchaseOrder = new GuidObject2(guid),
                        }
                    }
                };

            accountsPayableInvoice = new Dtos.AccountsPayableInvoices2()
            {
                Id = guid,
                TransactionDate = DateTime.Today,
                Vendor = vendor,
                VendorInvoiceNumber = "1",
                VendorInvoiceDate = DateTime.Today,
                ReferenceNumber = "1",
                InvoiceComment = "comment",
                SubmittedBy = new GuidObject2(guid),
                ProcessState = AccountsPayableInvoicesProcessState.Submitted,
                Payment = payment,
                PaymentStatus = AccountsPayableInvoicesPaymentStatus.Hold,
                VendorBilledAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 1000, Currency = CurrencyIsoCode.USD },
                InvoiceDiscountAmount = new Dtos.DtoProperties.Amount2DtoProperty() { Value = 100, Currency = CurrencyIsoCode.USD },
                Taxes = taxes,
                LineItems = lineItems
            };

            accountsPayableInvoiceEntity = new Domain.ColleagueFinance.Entities.AccountsPayableInvoices(guid, "1", DateTime.Today, VoucherStatus.InProgress, "name", "1", DateTime.Today)
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
                UnitOfIssue = "1",
                CashDiscountAmount = 10,
                TradeDiscountAmount = 5,

                Comments = "comments",

                AccountsPayableLineItemTaxes = new List<LineItemTax>()
                    {
                        new LineItemTax("1", 10)
                        {
                            LineGlNumber = "1"
                        }
                    }
            };

            lineItem.AddGlDistribution(new LineItemGlDistribution("11-00-02-67-60000-54005", 10, 1000, 1)
            {
                ProjectId = "1",

            });

            accountsPayableInvoiceEntity.AddAccountsPayableInvoicesLineItem(lineItem);

            manualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty()
            {
                Name = "name",
                Type = AccountsPayableInvoicesVendorType.Organization,
                AddressLines = new List<string>() { "address" },
                Place = new AddressPlace()
                {
                    Country = new AddressCountry()
                    {
                        Code = IsoCode.USA,
                        Locality = "USA",
                        PostalCode = "12345",
                        Region = new AddressRegion()
                        {
                            Code = "US-SA",
                        }
                    }
                }
            };
        }

        private void InitializeMock()
        {
            updateAccountsPayableInvoicesRole.AddPermission(new Domain.Entities.Permission(Domain.ColleagueFinance.ColleagueFinancePermissionCodes.UpdateApInvoices));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { updateAccountsPayableInvoicesRole });

            accountFundsAvailableMock.Setup(p => p.CheckAvailableFundsAsync(It.IsAny<List<Domain.ColleagueFinance.Entities.FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(fundsAvailable);
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            vendorsRepositoryMock.Setup(p => p.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            vendorsRepositoryMock.Setup(p => p.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            addressRepositoryMock.Setup(p => p.GetAddressFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            addressRepositoryMock.Setup(p => p.GetAddressGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            addressRepositoryMock.Setup(p => p.GetHostCountryAsync()).ReturnsAsync("USA");
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetAccountsPayableSourcesAsync(true)).ReturnsAsync(accountsPayableSources);
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetVendorTermsAsync(true)).ReturnsAsync(vendorTerms);
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityCodesAsync(true)).ReturnsAsync(commodityCodes);
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityUnitTypesAsync(true)).ReturnsAsync(commodityUnitTypes);
            accountsPayableInvoicesMock.Setup(p => p.GetAccountsPayableInvoicesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            referenceDataRepositoryMock.Setup(p => p.GetCommerceTaxCodesAsync(true)).ReturnsAsync(taxCodes);
            referenceDataRepositoryMock.Setup(p => p.GetCountryCodesAsync(false)).ReturnsAsync(countries);
            referenceDataRepositoryMock.Setup(p => p.GetStateCodesAsync(false)).ReturnsAsync(states);
            accountsPayableInvoicesMock.Setup(p => p.UpdateAccountsPayableInvoicesAsync(It.IsAny<Domain.ColleagueFinance.Entities.AccountsPayableInvoices>())).ReturnsAsync(accountsPayableInvoiceEntity);
            accountsPayableInvoicesMock.Setup(p => p.CreateAccountsPayableInvoicesAsync(It.IsAny<Domain.ColleagueFinance.Entities.AccountsPayableInvoices>())).ReturnsAsync(accountsPayableInvoiceEntity);
            accountsPayableInvoicesMock.Setup(p => p.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { Entity = "PURCHASE.ORDERS", PrimaryKey = "1" });
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_Is_Null()
        {
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_Id_Is_Null()
        {
            accountsPayableInvoice.Id = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_Vendor_Is_Null()
        {
            accountsPayableInvoice.Vendor = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_ExistingVendorId_Is_Null()
        {
            accountsPayableInvoice.Vendor.ExistingVendor.Vendor.Id = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_ManualVendorName_Is_Null()
        {
            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = new Dtos.DtoProperties.AccountsPayableInvoicesManualVendorDetailsDtoProperty() { Name = null };
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_VoidStatus_NoVoidDate()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Voided;
            accountsPayableInvoice.VoidDate = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_ProcessState_Is_NotSet()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.NotSet;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_PaymentStatus_Is_NotSet()
        {
            accountsPayableInvoice.PaymentStatus = AccountsPayableInvoicesPaymentStatus.NotSet;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_Payment_Is_Null()
        {
            accountsPayableInvoice.Payment = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_Payment_Source_Is_Null()
        {
            accountsPayableInvoice.Payment.Source = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_Payment_PaymentDueOn_Is_Null()
        {
            accountsPayableInvoice.Payment.PaymentDueOn = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_Payment_SourceId_Is_Null()
        {
            accountsPayableInvoice.Payment.Source.Id = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_Payment_PaymentTermsId_Is_Null()
        {
            accountsPayableInvoice.Payment.PaymentTerms.Id = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_VendorBilledAmount_Value_Is_Null()
        {
            accountsPayableInvoice.VendorBilledAmount.Value = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_VendorBilledAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.VendorBilledAmount.Currency = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_InvoiceDiscountAmount_Value_Is_Null()
        {
            accountsPayableInvoice.InvoiceDiscountAmount.Value = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_InvoiceDiscountAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.InvoiceDiscountAmount.Currency = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_When_Dto_VoidDate_LesserThan_TransactionDate()
        {
            accountsPayableInvoice.VoidDate = DateTime.Now.AddDays(-10);
            accountsPayableInvoice.TransactionDate = DateTime.Today;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_When_DefaultCurreny_NotMatching_With_Currency()
        {
            accountsPayableInvoice.InvoiceDiscountAmount.Currency = CurrencyIsoCode.AUD;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_TaxCodeId_Is_Null()
        {
            accountsPayableInvoice.Taxes.FirstOrDefault().TaxCode.Id = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_Dto_AllTaxCodeId_Is_Null()
        {
            accountsPayableInvoice.Taxes.FirstOrDefault().TaxCode.Id = "123";
            referenceDataRepositoryMock.Setup(p => p.GetCommerceTaxCodesAsync(true)).ReturnsAsync(null);
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_Dto_TaxCodeId_Is_Invalid()
        {
            accountsPayableInvoice.Taxes.FirstOrDefault().TaxCode.Id = "123";
            referenceDataRepositoryMock.Setup(p => p.GetCommerceTaxCodesAsync(true)).ReturnsAsync(taxCodes);
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_TaxVendorAmount_Is_Null()
        {
            accountsPayableInvoice.Taxes.FirstOrDefault().VendorAmount.Value = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_TaxVendorAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.Taxes.FirstOrDefault().VendorAmount.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_Are_Null()
        {
            accountsPayableInvoice.LineItems = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_CommodityCodeId_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().CommodityCode.Id = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_UnitofMeasureId_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitofMeasure.Id = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_UnitPriceValue_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitPrice.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_UnitPriceCurrency_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitPrice.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_VendorBilledUnitPriceValue_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().VendorBilledUnitPrice.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_VendorBilledUnitPriceCurrency_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().VendorBilledUnitPrice.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_AdditionalAmountValue_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AdditionalAmount.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItems_AdditionalAmountCurrency_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AdditionalAmount.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_TaxCode_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Taxes.FirstOrDefault().TaxCode = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_TaxCodeId_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Taxes.FirstOrDefault().TaxCode.Id = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_TaxVendorAmount_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Taxes.FirstOrDefault().VendorAmount.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_TaxVendorAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Taxes.FirstOrDefault().VendorAmount.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_DiscountAmount_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Amount.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_DiscountAmount_Percentage_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Amount = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Percent = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_DiscountAmount_Currency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Discount.Amount.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AllocatedAmountValue_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.Allocated.Amount.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AllocatedAmountCurrency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.Allocated.Amount.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AllocationTaxAmountValue_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.TaxAmount.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AllocationTaxAmountCurrency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.TaxAmount.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AllocationAdditionalAmountValue_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.AdditionalAmount.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AllocationAdditionalAmountCurrency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.AdditionalAmount.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AllocationDiscountAmountValue_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.DiscountAmount.Value = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AllocationDiscountAmountCurrency_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation.DiscountAmount.Currency = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_SourceId_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Source.Id = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_SubmittedById_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().SubmittedBy.Id = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_AccountingString_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().AccountingString = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_Allocation_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().Allocation = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_BudgetOverride_NoBudgetCheckOverride()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.NotSet;
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().SubmittedBy = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_AccountDetails_BudgetCheckOverride_And_SubmittedById_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().SubmittedBy.Id = null;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_Description_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Description = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_Quantity_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Quantity = 0;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_UnitPrice_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitPrice = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_Invalid_PO_GUID()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.PurchaseOrder.Id = "123";
            accountsPayableInvoicesMock.Setup(p => p.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1" });
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_Dto_LineItem__PO_No_RefLineNumber()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocumentLineItemNumber = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_Dto_LineItem__FinalPaymentFlag()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().EncumbranceStatus = AccountsPayableInvoicesEncumbranceStatus.FinalPayment;
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.PurchaseOrder.Id = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_PaymentStatus_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().PaymentStatus = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_Dto_LineItem_Status_Is_Null()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().Status = null;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_Funds_NotAvailable()
        {
            accountsPayableInvoiceEntity.CurrencyCode = "A12";
            accountsPayableInvoiceEntity.HostCountry = "CAN";
            fundsAvailable.FirstOrDefault().AvailableStatus = FundsAvailableStatus.NotAvailable;
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_PermissionException()
        {
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_Multiple_AccountDetails_With_Same_AccountingString()
        {
            var accountDetail = accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault();
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.Add(accountDetail);
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_DtoToEntity_Invalid_InvoiceNumber()
        {
            accountsPayableInvoice.InvoiceNumber = "Invalid";
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        //invalid processState is ignored.
        //[TestMethod]
        //[ExpectedException(typeof(Exception))]
        //public async Task ActPayInvService_PUT_DtoToEntity_Invalid_ProcessState()
        //{
        //    accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Awaitingreceipt;
        //    await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_DtoToEntity_From_VendorRepository()
        {
            vendorsRepositoryMock.Setup(p => p.GetVendorIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_DtoToEntity_VendorId_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Notapproved;
            vendorsRepositoryMock.Setup(p => p.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_DtoToEntity_Vendor_AlternateAddress_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Outstanding;
            addressRepositoryMock.Setup(p => p.GetAddressFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_DtoToEntity_PaymentSource_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Paid;
            accountsPayableInvoice.Payment.Source.Id = Guid.NewGuid().ToString();
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_DtoToEntity_PaymentTerms_NotFound()
        {
            accountsPayableInvoice.ProcessState = AccountsPayableInvoicesProcessState.Reconciled;
            accountsPayableInvoice.Payment.PaymentTerms.Id = Guid.NewGuid().ToString();
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_DtoToEntity_SubmittedBy_NotFound()
        {
            accountsPayableInvoice.SubmittedBy = new GuidObject2() {Id = "123"};
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_DtoToEntity_Repository_Returns_CommodityCodes_As_Null()
        {
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityCodesAsync(true)).ReturnsAsync(null);
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_DtoToEntity_Repository_Returns_CommodityCodes_As_Empty()
        {
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityCodesAsync(true)).ReturnsAsync(new List<Domain.ColleagueFinance.Entities.CommodityCode>());
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_DtoToEntity_Repository_Returns_CommodityUnitTypes_As_Empty()
        {
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityUnitTypesAsync(true)).ReturnsAsync(new List<Domain.ColleagueFinance.Entities.CommodityUnitType>());
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_DtoToEntity_Repository_Returns_CommodityUnitTypes_As_Null()
        {
            colleagueFinanceReferenceDataRepositoryMock.Setup(p => p.GetCommodityUnitTypesAsync(true)).ReturnsAsync(null);
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_DtoToEntity_LineItemNumber_NotSameAs_ReferenceLineItemNumber()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocumentLineItemNumber = "2";
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_DtoToEntity_LineItem_CommodityCode_NotFound()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.PurchaseOrder = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.BlanketPurchaseOrder = new GuidObject2(guid);
            accountsPayableInvoice.LineItems.FirstOrDefault().CommodityCode.Id = Guid.NewGuid().ToString();

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        //73
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ActPayInvService_PUT_DtoToEntity_LineItem_UnitOfMeasure_NotFound()
        {
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocumentLineItemNumber = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.PurchaseOrder = null;
            accountsPayableInvoice.LineItems.FirstOrDefault().ReferenceDocument.RecurringVoucher = guid;
            accountsPayableInvoice.LineItems.FirstOrDefault().UnitofMeasure.Id = Guid.NewGuid().ToString();

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_EntityToDto_Vendor_NotFound()
        {
            vendorsRepositoryMock.Setup(p => p.GetVendorGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            accountsPayableInvoiceEntity.VoucherMiscName = new List<string>();

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_EntityToDto_VendorId_As_Null()
        {
            accountsPayableInvoiceEntity.CurrencyCode = string.Empty;
            accountsPayableInvoiceEntity.HostCountry = "CAN";
            accountsPayableInvoiceEntity.VendorId = string.Empty;
            accountsPayableInvoiceEntity.VoucherMiscName = new List<string>();

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_EntityToDto_VoucherMiscCountry_NotFound()
        {
            manualVendorDetails.Place.Country.Code = IsoCode.AND;

            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = manualVendorDetails;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ActPayInvService_PUT_EntityToDto_StateCode_NotFound()
        {
            manualVendorDetails.Place.Country.Code = IsoCode.USA;
            manualVendorDetails.Place.Country.Region.Code = "US-XX";

            accountsPayableInvoice.Vendor.ExistingVendor = null;
            accountsPayableInvoice.Vendor.ManualVendorDetails = manualVendorDetails;

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ActPayInvService_PUT_AvailableFunds_SubmittedId_NotFound()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);

            accountsPayableInvoiceEntity.CurrencyCode = "A12";
            accountsPayableInvoiceEntity.HostCountry = "USA";
            accountsPayableInvoiceEntity.VoucherMiscCountry = String.Empty;
            accountsPayableInvoiceEntity.VoucherMiscState = "SA";

            personRepositoryMock.SetupSequence(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(guid)).Returns(Task.FromResult<string>(null));

            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ActPayInvService_PUT_RepositoryException()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            accountsPayableInvoice.LineItems[0].ReferenceDocument = null;
            accountsPayableInvoiceEntity.CurrencyCode = String.Empty;
            accountsPayableInvoiceEntity.HostCountry = "USA";
            accountsPayableInvoiceEntity.VoucherMiscCountry = String.Empty;
            accountsPayableInvoiceEntity.VoucherMiscState = "SA";
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
            accountsPayableInvoicesMock.Setup(p => p.UpdateAccountsPayableInvoicesAsync(It.IsAny<Domain.ColleagueFinance.Entities.AccountsPayableInvoices>())).ThrowsAsync(new RepositoryException());
            await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);
        }

        [TestMethod]
        public async Task ActPayInvService_PUT()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            accountsPayableInvoice.LineItems[0].ReferenceDocument = null;
            accountsPayableInvoiceEntity.PurchaseOrderId = string.Empty;
            accountsPayableInvoiceEntity.BlanketPurchaseOrderId = "1";

            manualVendorDetails.Place.Country.Code = IsoCode.CAN;

            accountsPayableInvoice.Vendor.ManualVendorDetails = manualVendorDetails;
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
            var result = await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);

            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
        }

        [TestMethod]
        public async Task ActPayInvService_PUT_valid_InvoiceNumber()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            accountsPayableInvoice.LineItems[0].ReferenceDocument = null;
            accountsPayableInvoiceEntity.PurchaseOrderId = string.Empty;
            accountsPayableInvoiceEntity.BlanketPurchaseOrderId = "1";

            manualVendorDetails.Place.Country.Code = IsoCode.CAN;

            accountsPayableInvoice.Vendor.ManualVendorDetails = manualVendorDetails;
            accountsPayableInvoice.InvoiceNumber = "1";
            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
            var result = await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);

            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
        }

        [TestMethod]
        public async Task ActPayInvService_PUT_Create_NewRecord()
        {
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            accountsPayableInvoice.LineItems[0].ReferenceDocument = null;
            accountsPayableInvoiceEntity.PurchaseOrderId = string.Empty;
            accountsPayableInvoiceEntity.RecurringVoucherId = "1";

            accountsPayableInvoice.Vendor.ExistingVendor = null;

            accountsPayableInvoice.Vendor.ManualVendorDetails = manualVendorDetails;

            accountsPayableInvoice.LineItems.FirstOrDefault().AccountDetails.FirstOrDefault().BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;

            accountsPayableInvoicesMock.Setup(p => p.GetAccountsPayableInvoicesIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

            accountsPayableInvoiceEntity.VendorId = null;
            accountsPayableInvoiceEntity.VoucherMiscName = new List<string>() { "name1", "name2" };
            accountsPayableInvoiceEntity.VoucherMiscAddress = new List<string>() { "address1" };

            var result = await accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid, accountsPayableInvoice);

            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
        }
    }

    #endregion v11 AccountsPayableInvoices
}
