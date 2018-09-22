// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class RecurringVoucherRepositoryTests
    {
        #region Initialize and Cleanup
        private Mock<IColleagueDataReader> dataReader = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private RecurringVoucherRepository recurringVoucherRepository;
        private TestRecurringVoucherRepository testRecurringVoucherRepository;

        // Data contract objects
        private TxGetHierarchyNameResponse hierarchyNameResponse;

        [TestInitialize]
        public void Initialize()
        {
            // Set up a mock data reader. All Colleague repositories have a local instance of an IColleagueDataReader.
            // We don't want the unit tests to rely on a real Colleague data reader 
            // (which would require a real Colleague environment).
            // Instead, we create a mock data reader which we can control locally.
            dataReader = new Mock<IColleagueDataReader>();

            // Set up a mock transaction invoker for the colleague transaction that gets
            // the GL accounts descriptions for the GL accounts in a project line item.
            transactionInvoker = new Mock<IColleagueTransactionInvoker>();

            // Initialize the Recurring Voucher repository
            testRecurringVoucherRepository = new TestRecurringVoucherRepository();

            this.recurringVoucherRepository = BuildRecurringVoucherRepository();

            InitializeMockMethods();
        }

        [TestCleanup]
        public void Cleanup()
        {
            dataReader = null;
            transactionInvoker = null;
            testRecurringVoucherRepository = null;
            hierarchyNameResponse = null;
        }
        #endregion

        #region Base RcVoucher test
        [TestMethod]
        public async Task GetRecurringVoucher_Base()
        {
            testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() { "O" };
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvApType, recurringVoucher.ApType);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvComments, recurringVoucher.Comments);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvDate, recurringVoucher.Date);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvDefaultInvoiceDate, recurringVoucher.InvoiceDate);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvDefaultInvoiceNo, recurringVoucher.InvoiceNumber);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvMaintGlTranDate, recurringVoucher.MaintenanceDate);
            Assert.AreEqual(RecurringVoucherStatus.Outstanding, recurringVoucher.Status);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvStatusDate[0], recurringVoucher.StatusDate);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvVendor, recurringVoucher.VendorId);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvVenName[0], recurringVoucher.VendorName);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvExchangeRate, recurringVoucher.ExchangeRate, "Exchange rates should match.");
        }

        [TestMethod]
        public async Task Constructor_HasCurrencyCode()
        {
            testRecurringVoucherRepository.RcVouchers.RcvCurrencyCode = "CAD";
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV000001", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvCurrencyCode, recurringVoucher.CurrencyCode, "Currency code should match.");
        }

        [TestMethod]
        public async Task HasPurgedVoucherId()
        {
            testRecurringVoucherRepository.RcVoucherSchedules[0].RcvsVouId = null;
            testRecurringVoucherRepository.RcVoucherSchedules[0].RcvsPurgedVouId = "V0000002";

            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(testRecurringVoucherRepository.RcVoucherSchedules[0].RcvsVouId, recurringVoucher.Schedules[0].VoucherId);
            Assert.AreEqual(testRecurringVoucherRepository.RcVoucherSchedules[0].RcvsPurgedVouId, recurringVoucher.Schedules[0].PurgedVoucherId);
        }

        [TestMethod]
        public async Task MissingBothExchangeRateAndDate()
        {
            testRecurringVoucherRepository.RcVouchers.RcvExchangeRate = null;
            testRecurringVoucherRepository.RcVouchers.RcvExchangeRateDate = null;

            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("R0000001", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(null, recurringVoucher.ExchangeRate, "Exchange rate should be null.");
        }
        #endregion

        #region Invalid data tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRecurringVoucher_NullRecurringVoucherId()
        {
            testRecurringVoucherRepository.RcVouchers.Recordkey = null;
            await recurringVoucherRepository.GetRecurringVoucherAsync("", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRecurringVoucher_EmptyRecurringVoucherId()
        {
            testRecurringVoucherRepository.RcVouchers.Recordkey = "";
            await recurringVoucherRepository.GetRecurringVoucherAsync("", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetRecurringVoucher_NullRecurringVoucher()
        {
            testRecurringVoucherRepository.RcVouchers = null;
            await recurringVoucherRepository.GetRecurringVoucherAsync("RV0000GTT", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRecurringVoucher_NullApTypeWithOutstandingStatus()
        {
            testRecurringVoucherRepository.RcVouchers.RcvApType = null;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        //[TestMethod]
        //public async Task GetRecurringVoucherAsync_NullStatus()
        //{
        //    var expectedMessage = "Missing status for recurring voucher: " + testRecurringVoucherRepository.RcVouchers.Recordkey;
        //    var actualMessage = "";
        //    try
        //    {
        //        testRecurringVoucherRepository.RcVouchers.RcvStatus = null;
        //        var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("1");
        //        Assert.AreEqual(RecurringVoucherStatus.Cancelled, recurringVoucher.Status);
        //    }
        //    catch (ApplicationException aex)
        //    {
        //        actualMessage = aex.Message;
        //    }
        //    Assert.AreEqual(expectedMessage, actualMessage);
        //}

        [TestMethod]
        public async Task GetRecurringVoucherAsync_FirstStatusIsNull()
        {
            var expectedMessage = "Missing status for recurring voucher: " + testRecurringVoucherRepository.RcVouchers.Recordkey;
            var actualMessage = "";
            try
            {
                testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() { null };
                var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("1", GlAccessLevel.Full_Access, null);
                Assert.AreEqual(RecurringVoucherStatus.Cancelled, recurringVoucher.Status);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_FirstStatusIsEmpty()
        {
            var expectedMessage = "Missing status for recurring voucher: " + testRecurringVoucherRepository.RcVouchers.Recordkey;
            var actualMessage = "";
            try
            {
                testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() { "" };
                var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("1", GlAccessLevel.Full_Access, null);
                Assert.AreEqual(RecurringVoucherStatus.Cancelled, recurringVoucher.Status);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRecurringVoucher_StatusListIsEmpty()
        {
            testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>();
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRecurringVoucher_InvalidStatus()
        {
            testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() {"Z"};
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        //[TestMethod]
        //public async Task GetRecurringVoucher_StatusDateIsNull()
        //{
        //    var expectedMessage = "Missing status date for recurring voucher: " + testRecurringVoucherRepository.RcVouchers.Recordkey;
        //    var actualMessage = "";
        //    try
        //    {
        //        testRecurringVoucherRepository.RcVouchers.RcvStatusDate = null;
        //        var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000");
        //    }
        //    catch (ApplicationException aex)
        //    {
        //        actualMessage = aex.Message;
        //    }
        //    Assert.AreEqual(expectedMessage, actualMessage);
        //}

        //[TestMethod]
        //public async Task GetRecurringVoucher_StatusDateListIsEmpty()
        //{
        //    var expectedMessage = "Missing status date for recurring voucher: " + testRecurringVoucherRepository.RcVouchers.Recordkey;
        //    var actualMessage = "";
        //    try
        //    {
        //        testRecurringVoucherRepository.RcVouchers.RcvStatusDate = new List<DateTime?>();
        //        var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000");
        //    }
        //    catch (ApplicationException aex)
        //    {
        //        actualMessage = aex.Message;
        //    }
        //    Assert.AreEqual(expectedMessage, actualMessage);
        //}

        [TestMethod]
        public async Task GetRecurringVoucher_FirstStatusDateIsNull()
        {
            var expectedMessage = "Missing status date for recurring voucher: " + testRecurringVoucherRepository.RcVouchers.Recordkey;
            var actualMessage = "";
            try
            {
                testRecurringVoucherRepository.RcVouchers.RcvStatusDate = new List<DateTime?>() { null, new DateTime(2015, 04, 01) };
                var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetRecurringVoucher_MissingVendorIdAndVendorName()
        {
            testRecurringVoucherRepository.RcVouchers.RcvVendor = null;
            testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() { };
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.IsTrue(string.IsNullOrEmpty(recurringVoucher.VendorName));
        }

        [TestMethod]
        public async Task GetRecurringVoucher_MissingVendorIdAndVendorName2()
        {
            testRecurringVoucherRepository.RcVouchers.RcvVendor = null;
            testRecurringVoucherRepository.RcVouchers.RcvVenName = null;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.IsTrue(string.IsNullOrEmpty(recurringVoucher.VendorName));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRecurringVoucher_MissingDate()
        {
            testRecurringVoucherRepository.RcVouchers.RcvDate = null;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRecurringVoucher_MissingDefaultInvoiceDate()
        {
            testRecurringVoucherRepository.RcVouchers.RcvDefaultInvoiceDate = null;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ScheduleDateIsNull()
        {
            testRecurringVoucherRepository.RcVoucherSchedules[0].RcvsScheduleDate = null;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task RcvsItemsTotalIsNull()
        {
            testRecurringVoucherRepository.RcVoucherSchedules[0].RcvsItemsTotal = null;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task HasVoucherIdAndPurgedVoucherId()
        {
            testRecurringVoucherRepository.RcVoucherSchedules[0].RcvsVouId = "V0000001";
            testRecurringVoucherRepository.RcVoucherSchedules[0].RcvsPurgedVouId = "V0000002";

            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ExchangeRateHasNoAssociatedDate()
        {
            testRecurringVoucherRepository.RcVouchers.RcvExchangeRate = 1.5m;
            testRecurringVoucherRepository.RcVouchers.RcvExchangeRateDate = null;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ExchangeRateIsNullButDateExists()
        {
            testRecurringVoucherRepository.RcVouchers.RcvExchangeRate = null;
            testRecurringVoucherRepository.RcVouchers.RcvExchangeRateDate = new DateTime(2015, 05, 18);
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ExchangeRateIsZero()
        {
            testRecurringVoucherRepository.RcVouchers.RcvExchangeRate = 0;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ExchangeRateIsNegative()
        {
            testRecurringVoucherRepository.RcVouchers.RcvExchangeRate = -1.4m;
            var recurringVoucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRecurringVoucherAsync_ItemsBulkReadReturnsNull()
        {
            this.testRecurringVoucherRepository.Items = null;
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.LineItems.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRecurringVoucherAsync_ItemsBulkReadReturnsEmptyList()
        {
            this.testRecurringVoucherRepository.Items = new Collection<Items>();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.LineItems.Count);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_ScheduleIdsIsNull()
        {
            this.testRecurringVoucherRepository.RcVouchers.RcvRcvsId = null;
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.Schedules.Count);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_ScheduleIdsIsEmpty()
        {
            this.testRecurringVoucherRepository.RcVouchers.RcvRcvsId = new List<string>();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.Schedules.Count);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_RcVoucherSchedulesBulkReadReturnsNull()
        {
            this.testRecurringVoucherRepository.RcVoucherSchedules = null;
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.Schedules.Count);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_RcVoucherSchedulesBulkReadReturnsEmptyList()
        {
            this.testRecurringVoucherRepository.RcVoucherSchedules = new Collection<RcVouSchedules>();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.Schedules.Count);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_ScheduleTaxAmountsAreNull()
        {
            foreach (var schedule in this.testRecurringVoucherRepository.RcVoucherSchedules)
            {
                schedule.RcvsItemsTaxTotal = null;
            }
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            foreach (var schedule in recurringVoucherDomainEntity.Schedules)
            {
                Assert.AreEqual(0, schedule.TaxAmount);
            }
        }
        #endregion

        #region Status tests
        [TestMethod]
        public async Task GetRecurringVoucher_CStatus()
        {
            testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() { "C" };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(RecurringVoucherStatus.Closed, voucher.Status);
        }
        
        [TestMethod]
        public async Task GetRecurringVoucher_NStatus()
        {
            testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() { "N" };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(RecurringVoucherStatus.NotApproved, voucher.Status);
        }
        
        [TestMethod]
        public async Task GetRecurringVoucher_OStatus()
        {
            testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() { "O" };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(RecurringVoucherStatus.Outstanding, voucher.Status);
        }

        [TestMethod]
        public async Task GetRecurringVoucher_VStatus()
        {
            testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() { "V" };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(RecurringVoucherStatus.Voided, voucher.Status);
        }

        [TestMethod]
        public async Task GetRecurringVoucher_XStatus()
        {
            testRecurringVoucherRepository.RcVouchers.RcvStatus = new List<string>() { "X" };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(RecurringVoucherStatus.Cancelled, voucher.Status);
        }
        #endregion

        #region Vendor tests
        [TestMethod]
        public async Task GetRecurringVoucher_VendorNameOnly()
        {
            testRecurringVoucherRepository.RcVouchers.RcvVendor = null;
            testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() {"Susty Corporation"};
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(String.Join(" ", testRecurringVoucherRepository.RcVouchers.RcvVenName.ToArray()), voucher.VendorName);
        }

        [TestMethod]
        public async Task GetRecurringVoucher_VendorNameOnly_MultiLineName()
        {
            testRecurringVoucherRepository.RcVouchers.RcvVendor = null;
            testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() { "Susty Corporation", "LLC." };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(String.Join(" ", testRecurringVoucherRepository.RcVouchers.RcvVenName.ToArray()), voucher.VendorName);
        }

        [TestMethod]
        public async Task GetRecurringVoucher_VendorIdOnly_NameIsNull()
        {
            testRecurringVoucherRepository.RcVouchers.RcvVendor = "0003949";
            testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() { };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(String.Join(" ", this.hierarchyNameResponse.OutPersonName.ToArray()), voucher.VendorName);
        }

        [TestMethod]
        public async Task GetRecurringVoucher_VendorIdOnly_CtxReturnsNullNameList()
        {
            testRecurringVoucherRepository.RcVouchers.RcvVendor = "0003949";
            testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() { };
            this.hierarchyNameResponse.OutPersonName = null;
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvVendor, voucher.VendorId);
            Assert.IsTrue(string.IsNullOrEmpty(voucher.VendorName));
        }

        [TestMethod]
        public async Task GetRecurringVoucher_VendorIdOnly_CtxReturnsEmptyNameList()
        {
            testRecurringVoucherRepository.RcVouchers.RcvVendor = "0003949";
            testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() { };
            this.hierarchyNameResponse.OutPersonName = new List<string>();
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(testRecurringVoucherRepository.RcVouchers.RcvVendor, voucher.VendorId);
            Assert.IsTrue(string.IsNullOrEmpty(voucher.VendorName));
        }

        [TestMethod]
        public async Task GetRecurringVoucher_VendorIdOnly_NameIsWhitespace()
        {

            testRecurringVoucherRepository.RcVouchers.RcvVendor = "0003949";
            testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() { " " };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(" ", voucher.VendorName);
        }

        //[TestMethod]
        //public async Task GetRecurringVoucher_VendorIdOnly_MultiLineName()
        //{
        //    testRecurringVoucherRepository.RcVouchers.RcvVendor = "0003949";
        //    testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() { };
        //    var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000");
        //    Assert.AreEqual(String.Join(" ", this.hierarchyNameResponse.OutPersonName.ToArray()), voucher.VendorName);
        //}

        [TestMethod]
        public async Task GetRecurringVoucher_HasVendorIdAndName()
        {
            testRecurringVoucherRepository.RcVouchers.RcvVendor = "0003949";
            testRecurringVoucherRepository.RcVouchers.RcvVenName = new List<string>() { "Susty Corporation", "LLC." };
            var voucher = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(String.Join(" ", testRecurringVoucherRepository.RcVouchers.RcvVenName.ToArray()), voucher.VendorName);
        }
        #endregion

        #region Approvers and Next Approvers tests
        [TestMethod]
        public async Task GetRecurringVoucher_HasApproversAndNextApprovers()
        {
            var recurringVoucherDataContract = testRecurringVoucherRepository.GetRcVouchersDataContract();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(recurringVoucherDataContract.RcvAuthorizations.Count() + testRecurringVoucherRepository.RcVouchers.RcvNextApprovalIds.Count(), recurringVoucherDomainEntity.Approvers.Count());
            foreach (var approver in recurringVoucherDomainEntity.Approvers)
           {
               if (approver.ApprovalDate != null)
               {
                   Assert.IsTrue(recurringVoucherDataContract.RcvAuthEntityAssociation.Any(x =>
                       x.RcvAuthorizationsAssocMember == approver.ApproverId
                       && x.RcvAuthorizationDatesAssocMember == approver.ApprovalDate));
               }
               else
               {
                   Assert.IsTrue(recurringVoucherDataContract.RcvApprEntityAssociation.Any(x =>
                       x.RcvNextApprovalIdsAssocMember == approver.ApproverId
                       && null == approver.ApprovalDate));
               }
           }
        }

        [TestMethod]
        public async Task GetRecurringVoucher_NoApproversAndNoNextApprovers()
        {
            testRecurringVoucherRepository.RcVouchers.RcvAuthorizations = new List<string>();
            testRecurringVoucherRepository.RcVouchers.RcvAuthorizationDates = new List<DateTime?>();
            testRecurringVoucherRepository.RcVouchers.RcvNextApprovalIds = new List<string>();
            var recurringVoucherDataContract = testRecurringVoucherRepository.GetRcVouchersDataContract();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(recurringVoucherDataContract.RcvAuthorizations.Count() + testRecurringVoucherRepository.RcVouchers.RcvNextApprovalIds.Count(), recurringVoucherDomainEntity.Approvers.Count());
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_OpersBulkReadReturnsNull()
        {
            this.testRecurringVoucherRepository.Opers = null;
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.Approvers.Count);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_OpersBulkReadReturnsEmptyList()
        {
            this.testRecurringVoucherRepository.Opers = new Collection<Opers>();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.Approvers.Count);
        }

        //[TestMethod]
        //public async Task GetRecurringVoucherAsync_NullApproverRecordKey()
        //{
        //    foreach (var opersContract in this.testRecurringVoucherRepository.Opers)
        //    {
        //        opersContract.Recordkey = null;
        //    }

        //    var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000");
        //    Assert.AreEqual(0, recurringVoucherDomainEntity.Approvers.Count);
        //}

        //[TestMethod]
        //public async Task GetRecurringVoucherAsync_EmptyApproverRecordKey()
        //{
        //    foreach (var opersContract in this.testRecurringVoucherRepository.Opers)
        //    {
        //        opersContract.Recordkey = "";
        //    }

        //    var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000");
        //    Assert.AreEqual(0, recurringVoucherDomainEntity.Approvers.Count);
        //}

        [TestMethod]
        public async Task GetRecurringVoucherAsync_NullApprovalsAssociation()
        {
            this.testRecurringVoucherRepository.RcVouchers.RcvAuthEntityAssociation = null;
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(this.testRecurringVoucherRepository.Opers.Count, recurringVoucherDomainEntity.Approvers.Count);
            foreach (var approver in recurringVoucherDomainEntity.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetRecurringVoucherAsync_EmptyApprovalsAssociation()
        {
            this.testRecurringVoucherRepository.RcVouchers.RcvAuthEntityAssociation = new List<RcVouchersRcvAuth>();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(this.testRecurringVoucherRepository.Opers.Count, recurringVoucherDomainEntity.Approvers.Count);
            foreach (var approver in recurringVoucherDomainEntity.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }
        #endregion

        #region Amount tests
        [TestMethod]
        public async Task GetRecurringVoucherAmount_LocalCurrency()
        {
            var recurringVoucherDataContract = testRecurringVoucherRepository.GetRcVouchersDataContract();
            var itemDataContracts = testRecurringVoucherRepository.GetItemsDataContract();
            decimal? total = 330m;
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(total,recurringVoucherDomainEntity.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRecurringVoucherAmount_NullLineItems()
        {
            testRecurringVoucherRepository.RcVouchers.RcvItemsId = null;
            var recurringVoucherDataContract = testRecurringVoucherRepository.GetRcVouchersDataContract();
            var itemDataContracts = testRecurringVoucherRepository.GetItemsDataContract();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.LineItems.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetRecurringVoucherAmount_NoLineItems()
        {
            testRecurringVoucherRepository.RcVouchers.RcvItemsId = new List<string>();
            var recurringVoucherDataContract = testRecurringVoucherRepository.GetRcVouchersDataContract();
            var itemDataContracts = testRecurringVoucherRepository.GetItemsDataContract();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(0, recurringVoucherDomainEntity.LineItems.Count);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAmount_MissingLineItems()
        {
            var recurringVoucherDataContract = testRecurringVoucherRepository.GetRcVouchersDataContract();
            //var itemDataContracts = testRecurringVoucherRepository.GetItemsDataContract();
            decimal? total = 0m;
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(total, recurringVoucherDomainEntity.Amount);
        }

        [TestMethod]
        public async Task GetRecurringVoucherAmount_ForeignCurrency()
        {
            testRecurringVoucherRepository.RcVouchers.RcvCurrencyCode = "CAD";
            testRecurringVoucherRepository.RcVouchers.RcvItemsId = new List<string>() {"4", "5", "6"};
            decimal? total = 660m;
            var recurringVoucherDataContract = testRecurringVoucherRepository.GetRcVouchersDataContract();
            var itemDataContracts = testRecurringVoucherRepository.GetItemsDataContract();
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync("RV0001000", GlAccessLevel.Full_Access, null);
            Assert.AreEqual(total, recurringVoucherDomainEntity.Amount);
        }
        #endregion

        #region Private methods
        private RecurringVoucherRepository BuildRecurringVoucherRepository()
        {
            // Instantiate all objects necessary to mock data reader and CTX calls.
            var cacheProviderObject = new Mock<ICacheProvider>().Object;
            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            // The transaction factory has a method to get its data reader
            // Make sure that method returns our mock data reader
            transactionFactory.Setup(transFac => transFac.GetDataReader()).Returns(dataReader.Object);
            transactionFactory.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

            return new RecurringVoucherRepository(cacheProviderObject, transactionFactoryObject, loggerObject);
        }

        private void InitializeMockMethods()
        {
            // Mock ReadRecord to return a pre-defined Vouchers data contract
            dataReader.Setup<Task<RcVouchers>>(d => d.ReadRecordAsync<RcVouchers>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.testRecurringVoucherRepository.RcVouchers);
            });

            // Mock bulk read UT.OPERS bulk read
            dataReader.Setup<Task<RcVouchers>>(d => d.ReadRecordAsync<RcVouchers>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(this.testRecurringVoucherRepository.RcVouchers);
            });

            dataReader.Setup<Task<Collection<RcVouSchedules>>>(acc => acc.BulkReadRecordAsync<RcVouSchedules>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.testRecurringVoucherRepository.RcVoucherSchedules);
            });

            dataReader.Setup<Task<Collection<Opers>>>(acc => acc.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.testRecurringVoucherRepository.Opers);
            });

            // Mock BulkReadRecord to return a list of Items data contracts
            dataReader.Setup<Task<Collection<Items>>>(acc => acc.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.testRecurringVoucherRepository.Items);
            }); 
            
            // vendor name comes from CTX
            string vendorName = "Vendor name for use in a colleague transaction";
            var ctxVendorName = new List<string>() { vendorName };
            ctxVendorName.Add(vendorName);
            ctxVendorName.Add(vendorName);

            this.hierarchyNameResponse = new TxGetHierarchyNameResponse()
            {
                IoPersonId = "0000001",
                OutPersonName = ctxVendorName
            };
            transactionInvoker.Setup(tio => tio.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(It.IsAny<TxGetHierarchyNameRequest>())).Returns(() =>
                {
                    return this.hierarchyNameResponse;
                });
        }

        #endregion
    }
}