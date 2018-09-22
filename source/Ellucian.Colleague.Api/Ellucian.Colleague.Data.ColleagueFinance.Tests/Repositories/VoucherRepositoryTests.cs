// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class VoucherRepositoryTests
    {
        #region Initialize and Cleanup
        private Mock<IColleagueDataReader> dataReader = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private VoucherRepository voucherRepository;
        private TestVoucherRepository testVoucherRepository;
        private Voucher voucherDomainEntity;

        // Data contract objects
        private Vouchers voucherDataContract;
        private RcVouSchedules recurringVoucherDataContract;
        private Collection<Opers> opersDataContracts;
        private Collection<Projects> projectDataContracts;
        private Collection<Items> itemsDataContracts;
        private Collection<ProjectsLineItems> projectLineItemDataContracts;
        private TxGetHierarchyNameResponse hierarchyNameResponse;
        private Collection<Opers> opersResponse;

        private string personId = "1";
        private int versionNumber;

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

            // Initialize the data contract objects
            voucherDataContract = new Vouchers();

            // Initialize the Voucher repository
            testVoucherRepository = new TestVoucherRepository();

            this.voucherRepository = BuildVoucherRepository();
            versionNumber = 2;
        }

        [TestCleanup]
        public void Cleanup()
        {
            dataReader = null;
            transactionInvoker = null;
            voucherDataContract = null;
            recurringVoucherDataContract = null;
            opersDataContracts = null;
            projectDataContracts = null;
            projectLineItemDataContracts = null;
            itemsDataContracts = null;
            testVoucherRepository = null;
            voucherDomainEntity = null;
            hierarchyNameResponse = null;
            opersResponse = null;
        }
        #endregion

        #region Test methods
        #region Base voucher test
        [TestMethod]
        public async Task GetVoucher_Base()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            // Confirm that the SV properties for the voucher are the same
            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount);
            Assert.AreEqual(this.voucherDomainEntity.ApType, voucher.ApType);
            Assert.AreEqual(this.voucherDomainEntity.BlanketPurchaseOrderId, voucher.BlanketPurchaseOrderId);
            Assert.AreEqual(this.voucherDomainEntity.CheckDate, voucher.CheckDate);
            Assert.AreEqual(this.voucherDomainEntity.CheckNumber, voucher.CheckNumber);
            Assert.AreEqual(this.voucherDomainEntity.Comments, voucher.Comments);
            Assert.AreEqual(this.voucherDomainEntity.CurrencyCode, voucher.CurrencyCode);
            Assert.AreEqual(this.voucherDomainEntity.Date, voucher.Date);
            Assert.AreEqual(this.voucherDomainEntity.DueDate, voucher.DueDate);
            Assert.AreEqual(this.voucherDomainEntity.InvoiceDate, voucher.InvoiceDate);
            Assert.AreEqual(this.voucherDomainEntity.InvoiceNumber, voucher.InvoiceNumber);
            Assert.AreEqual(this.voucherDomainEntity.MaintenanceDate, voucher.MaintenanceDate);
            Assert.AreEqual(this.voucherDomainEntity.PurchaseOrderId, voucher.PurchaseOrderId);
            Assert.AreEqual(this.voucherDomainEntity.RecurringVoucherId, voucher.RecurringVoucherId);
            Assert.AreEqual(this.voucherDomainEntity.Status, voucher.Status);
            Assert.AreEqual(this.voucherDomainEntity.VendorId, voucher.VendorId);
            Assert.AreEqual(this.voucherDomainEntity.VendorName, voucher.VendorName);
            Assert.AreEqual(this.voucherDomainEntity.Id, voucher.Id);
        }
        #endregion

        #region Invalid data tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucher_NullId()
        {
            var voucher = await this.voucherRepository.GetVoucherAsync(null, personId, GlAccessLevel.Full_Access, null, versionNumber);
        }

        [TestMethod]
        public async Task GetVoucher_EmptyId()
        {
            var expectedParamName = "voucherId";
            var actualParamName = "";
            try
            {
                var voucher = await this.voucherRepository.GetVoucherAsync("", personId, GlAccessLevel.Full_Access, null, versionNumber);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName;
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetVoucher_NullVoucher()
        {
            // Mock ReadRecord to return a pre-defined, null Vouchers data contract
            var nullVouchersObject = new Vouchers();
            nullVouchersObject = null;
            dataReader.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(Task.FromResult(nullVouchersObject));
            var voucher = await this.voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetVoucher_NullApType()
        {
            // Mock ReadRecord to return a pre-defined, null Vouchers data contract
            // Set the required data except for the AP type
            var vouchersObject = new Vouchers()
            {
                Recordkey = "1",
                VouStatus = new List<string>() { "U" },
                VouDate = new DateTime(2015, 04, 01),
                VouMiscName = new List<string>() { "Susty Corporation" },
                VouDefaultInvoiceNo = "ABC123",
                VouDefaultInvoiceDate = new DateTime(2015, 04, 01),
                VouApType = string.Empty
            };
            dataReader.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(Task.FromResult(vouchersObject));
            var voucher = await this.voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetVoucher_NullStatus()
        {
            // Mock ReadRecord to return a pre-defined, null Vouchers data contract
            var vouchersObject = new Vouchers()
            {
                Recordkey = "1",
                VouStatus = null

            };
            dataReader.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(Task.FromResult(vouchersObject));
            var voucher = await this.voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetVoucher_StatusListHasNullValue()
        {
            // Mock ReadRecord to return a pre-defined, null Vouchers data contract
            var vouchersObject = new Vouchers()
            {
                Recordkey = "1",
                VouStatus = new List<string>() { null },

            };
            dataReader.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(Task.FromResult(vouchersObject));
            var voucher = await this.voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetVoucher_StatusListHasBlankValue()
        {
            // Mock ReadRecord to return a pre-defined, null Vouchers data contract
            var vouchersObject = new Vouchers()
            {
                Recordkey = "1",
                VouStatus = new List<string>() { "" },

            };
            dataReader.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(Task.FromResult(vouchersObject));
            var voucher = await this.voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetVoucher_InvalidStatus()
        {
            // Mock ReadRecord to return a pre-defined, null Vouchers data contract
            var vouchersObject = new Vouchers()
            {
                Recordkey = "1",
                VouStatus = new List<string>() { "Z" },

            };
            dataReader.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(Task.FromResult(vouchersObject));
            var voucher = await this.voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetVoucher_NullDate()
        {
            // Mock ReadRecord to return a pre-defined, null Vouchers data contract
            var vouchersObject = new Vouchers()
            {
                Recordkey = "1",
                VouStatus = new List<string>() { "O" },
                VouDate = null
            };
            dataReader.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(Task.FromResult(vouchersObject));
            var voucher = await this.voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
        }

        [TestMethod]
        public async Task GetVoucher_VoucherDateIsNull()
        {
            var expectedMessage = "";
            var actualMessage = "";
            try
            {
                var voucherId = "1";
                this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                var expenseAccounts = CalculateGlAccountsForUser(voucherId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                expectedMessage = "Missing voucher date for voucher: " + this.voucherDataContract.Recordkey;
                this.voucherDataContract.VouDate = null;
                var voucher = await this.voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetVoucher_VoucherDefaultInvoiceDateIsNull()
        {
            var expectedMessage = "";
            var actualMessage = "";
            try
            {
                var voucherId = "1";
                versionNumber = 1;
                this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                var expenseAccounts = CalculateGlAccountsForUser(voucherId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                expectedMessage = "Invoice date is a required field.";
                this.voucherDataContract.VouDefaultInvoiceDate = null;
                var voucher = await this.voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetVoucher_VoucherDefaultInvoiceNumberIsNull()
        {
            var expectedMessage = "";
            var actualMessage = "";
            try
            {
                var voucherId = "1";
                versionNumber = 1;
                this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
                var expenseAccounts = CalculateGlAccountsForUser(voucherId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                expectedMessage = "Invoice number is a required field.";
                this.voucherDataContract.VouDefaultInvoiceNo = null;
                var voucher = await this.voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region Status tests
        [TestMethod]
        public async Task GetVoucher_UStatus()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);
            Assert.AreEqual(VoucherStatus.InProgress, voucher.Status);
        }


        [TestMethod]
        public async Task GetVoucher_NStatus()
        {
            string voucherId = "17";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(VoucherStatus.NotApproved, voucher.Status);
        }

        [TestMethod]
        public async Task GetVoucher_OStatus()
        {
            string voucherId = "3";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(VoucherStatus.Outstanding, voucher.Status);
        }

        [TestMethod]
        public async Task GetVoucher_PStatus()
        {
            string voucherId = "4";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(VoucherStatus.Paid, voucher.Status);
        }

        [TestMethod]
        public async Task GetVoucher_RStatus()
        {
            string voucherId = "18";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(VoucherStatus.Reconciled, voucher.Status);
        }

        [TestMethod]
        public async Task GetVoucher_VStatus()
        {
            string voucherId = "19";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(VoucherStatus.Voided, voucher.Status);
        }

        [TestMethod]
        public async Task GetVoucher_XStatus()
        {
            string voucherId = "20";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(VoucherStatus.Cancelled, voucher.Status);
        }

        #endregion

        #region Vendor tests
        [TestMethod]
        public async Task GetVoucher_VendorNameOnly()
        {
            string voucherId = "11";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.VendorName, voucher.VendorName);
        }

        [TestMethod]
        public async Task GetVoucher_VendorNameOnly_MultiLineName()
        {
            string voucherId = "25";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(String.Join(" ", this.voucherDataContract.VouMiscName.ToArray()), voucher.VendorName);
        }
        
        [TestMethod]
        public async Task GetVoucher_VendorIdOnly_NameIsNull()
        {
            string voucherId = "13";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.hierarchyNameResponse.OutPersonName.First(), voucher.VendorName);
        }

        [TestMethod]
        public async Task GetVoucher_VendorIdOnly_NameIsWhitespace()
        {
            string voucherId = "27";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);
            Assert.AreEqual(" ", voucher.VendorName);
            Assert.AreEqual(String.Join(" ", this.voucherDataContract.VouMiscName.ToArray()), voucher.VendorName);
        }

        [TestMethod]
        public async Task GetVoucher_VendorIdOnly_MultiLineName()
        {
            string voucherId = "26";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(String.Join(" ", this.hierarchyNameResponse.OutPersonName.ToArray()), voucher.VendorName);
        }

        [TestMethod]
        public async Task GetVoucher_HasVendorIdAndName()
        {
            string voucherId = "14";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.VendorName, voucher.VendorName);
        }

        [TestMethod]
        public async Task GetVoucher_NoVendorIdOrName()
        {
            string voucherId = "15";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);
            Assert.IsTrue(string.IsNullOrEmpty(voucher.VendorName));
        }

        [TestMethod]
        public async Task GetVoucherAsync_CtxReturnsNull()
        {
            string voucherId = "26";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.hierarchyNameResponse.OutPersonName = null;
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);
            Assert.IsTrue(string.IsNullOrEmpty(voucher.VendorName));
        }

        [TestMethod]
        public async Task GetVoucherAsync_CtxReturnsEmptyList()
        {
            string voucherId = "26";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.hierarchyNameResponse.OutPersonName = new List<string>();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);
            Assert.IsTrue(string.IsNullOrEmpty(voucher.VendorName));
        }
        #endregion

        #region Check tests
        [TestMethod]
        public async Task GetVoucher_PaidVoucher()
        {
            string voucherId = "4";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            // Confirm that the check properties for the voucher are the same
            Assert.AreEqual(this.voucherDomainEntity.CheckDate, voucher.CheckDate);

            var bankLength = this.voucherDomainEntity.CheckNumber.IndexOf('*');
            var checkLength = this.voucherDomainEntity.CheckNumber.Length;

            this.voucherDomainEntity.CheckNumber = this.voucherDomainEntity.CheckNumber.Substring(bankLength + 1, checkLength - (bankLength + 1));
            Assert.AreEqual(this.voucherDomainEntity.CheckNumber, voucher.CheckNumber);
        }
        #endregion

        #region PO, BPO, RCV tests
        [TestMethod]
        public async Task GetVoucher_OriginatedFromPO()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.PurchaseOrderId, voucher.PurchaseOrderId);
        }

        [TestMethod]
        public async Task GetVoucher_OriginatedFromBPO()
        {
            string voucherId = "21";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.BlanketPurchaseOrderId, voucher.BlanketPurchaseOrderId);
        }

        [TestMethod]
        public async Task GetVoucher_OriginatedFromRCV()
        {
            string voucherId = "16";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.RecurringVoucherId, voucher.RecurringVoucherId);
        }
        #endregion

        #region Approvers and Next Approvers tests
        [TestMethod]
        public async Task GetVoucher_HasApproversAndNextApprovers()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Approvers.Count(), voucher.Approvers.Count());
            foreach (var approver in this.voucherDomainEntity.Approvers)
            {
                Assert.IsTrue(voucher.Approvers.Any(x =>
                    x.ApproverId == approver.ApproverId
                    && x.ApprovalName == approver.ApprovalName
                    && x.ApprovalDate == approver.ApprovalDate));
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_OpersBulkReadReturnsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersResponse = null;
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.Approvers.Count());
        }

        [TestMethod]
        public async Task GetVoucherAsync_OpersBulkReadReturnsEmptyList()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersResponse = new Collection<Opers>();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.Approvers.Count());
        }

        [TestMethod]
        public async Task GetVoucherAsync_ApproversAssociationIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.voucherDataContract.VouAuthEntityAssociation = null;
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.opersResponse.Count, voucher.Approvers.Count());
            foreach (var approver in voucher.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_ApproversAssociationIsEmpty()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.voucherDataContract.VouAuthEntityAssociation = new List<VouchersVouAuth>();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.opersResponse.Count, voucher.Approvers.Count());
            foreach (var approver in voucher.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        //[TestMethod]
        //public async Task GetVoucherAsync_ApproverRecordKeyIsNull()
        //{
        //    string voucherId = "1";
        //    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null);
        //    var expenseAccounts = CalculateGlAccountsForUser(voucherId);
        //    ConvertDomainEntitiesIntoDataContracts();
        //    InitializeMockMethods();

        //    foreach (var approverContract in this.opersResponse)
        //    {
        //        approverContract.Recordkey = null;
        //    }
        //    var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts);

        //    Assert.AreEqual(0, voucher.Approvers.Count());
        //}

        //[TestMethod]
        //public async Task GetVoucherAsync_ApproverRecordKeyIsEmpty()
        //{
        //    string voucherId = "1";
        //    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null);
        //    var expenseAccounts = CalculateGlAccountsForUser(voucherId);
        //    ConvertDomainEntitiesIntoDataContracts();
        //    InitializeMockMethods();

        //    foreach (var approverContract in this.opersResponse)
        //    {
        //        approverContract.Recordkey = "";
        //    }
        //    var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts);

        //    Assert.AreEqual(0, voucher.Approvers.Count());
        //}
        #endregion

        #region LineItems tests
        [TestMethod]
        public async Task GetVoucher_LineItems_Base()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.LineItems.Count(), voucher.LineItems.Count(), "Vouchers should have the same number of line items.");

            foreach (var lineItem in this.voucherDomainEntity.LineItems)
            {
                Assert.IsTrue(voucher.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.InvoiceNumber == lineItem.InvoiceNumber
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue));
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_LineItemsIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.voucherDataContract.VouItemsId = null;
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.LineItems.Count());
        }

        [TestMethod]
        public async Task GetVoucherAsync_LineItemsIsEmpty()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.voucherDataContract.VouItemsId = new List<string>();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.LineItems.Count());
        }

        [TestMethod]
        public async Task GetVoucherAsync_ItemsBulkReadReturnsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.itemsDataContracts = null;
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.LineItems.Count());
        }

        [TestMethod]
        public async Task GetVoucherAsync_ItemsBulkReadReturnsEmptyList()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.itemsDataContracts = null;
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.LineItems.Count());
        }

        [TestMethod]
        public async Task GetVoucherAsync_LineItemAssociationIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.VouchGlEntityAssociation = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.LineItems.Count());
        }

        [TestMethod]
        public async Task GetVoucherAsync_LineItemAssociationIsEmptyList()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.VouchGlEntityAssociation = new List<ItemsVouchGl>();
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.LineItems.Count());
        }

        //[TestMethod]
        //public async Task GetVoucherAsync_LineItemDescriptionIsNull()
        //{
        //    string voucherId = "1";
        //    this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null);
        //    var expenseAccounts = CalculateGlAccountsForUser(voucherId);
        //    ConvertDomainEntitiesIntoDataContracts();
        //    InitializeMockMethods();

        //    foreach (var lineItem in this.itemsDataContracts)
        //    {
        //        lineItem.ItmDesc = null;
        //    }
        //    var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

        //    Assert.Fail();
        //}

        [TestMethod]
        public async Task GetVoucherAsync_LineItemDescriptionHasMultipleLines()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItmDesc = new List<string>() { "Line 1", "Line 2" };
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var lineItem in this.itemsDataContracts)
            {
                var expectedLineItemDescription = String.Join(" ", lineItem.ItmDesc) + " ";
                var actualLineItem = voucher.LineItems.FirstOrDefault(x => x.Description == expectedLineItemDescription);

                Assert.IsNotNull(actualLineItem);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_ItemQuantityIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItmVouQty = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var lineItem in voucher.LineItems)
            {
                Assert.AreEqual(0, lineItem.Quantity);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_ItemPriceIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItmVouPrice = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var lineItem in voucher.LineItems)
            {
                Assert.AreEqual(0, lineItem.Price);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_ItemExtendedPriceIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItmVouExtPrice = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var lineItem in voucher.LineItems)
            {
                Assert.AreEqual(0, lineItem.ExtendedPrice);
            }
        }
        #endregion

        #region Gl Distribution tests
        [TestMethod]
        public async Task GetVoucherAsync_NullExpenseAccounts()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);

            Assert.AreEqual(0, voucher.LineItems.Count);
            Assert.AreEqual(0, voucher.LineItems.SelectMany(x => x.GlDistributions).Count());
        }

        [TestMethod]
        public async Task GetVoucher_GlDistributions_AllLocalAmounts()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount);
            foreach (var domainLineItem in this.voucherDomainEntity.LineItems)
            {
                foreach (var domainGlDistribution in domainLineItem.GlDistributions)
                {
                    foreach (var lineItem in voucher.LineItems)
                    {
                        // Since we're comparing two voucher objects that SHOULD be the same
                        // we only want to execute the assertion if we know we're comparing the
                        // same line items.
                        if (domainLineItem.Id == lineItem.Id)
                        {
                            Assert.IsTrue(lineItem.GlDistributions.Any(x =>
                                x.Amount == domainGlDistribution.Amount
                                && x.GlAccountNumber == domainGlDistribution.GlAccountNumber
                                && x.ProjectId == domainGlDistribution.ProjectId
                                & x.ProjectLineItemCode == domainGlDistribution.ProjectLineItemCode
                                & x.ProjectLineItemId == domainGlDistribution.ProjectLineItemId
                                && x.ProjectNumber == domainGlDistribution.ProjectNumber
                                && x.Quantity == domainGlDistribution.Quantity));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetVoucher_GlDistributions_AllForeignAmounts()
        {
            string voucherId = "2";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount);
            foreach (var domainLineItem in this.voucherDomainEntity.LineItems)
            {
                foreach (var domainGlDistribution in domainLineItem.GlDistributions)
                {
                    foreach (var lineItem in voucher.LineItems)
                    {
                        // Since we're comparing two voucher objects that SHOULD be the same
                        // we only want to execute the assertion if we know we're comparing the
                        // same line items.
                        if (domainLineItem.Id == lineItem.Id)
                        {
                            Assert.IsTrue(lineItem.GlDistributions.Any(x =>
                                x.Amount == domainGlDistribution.Amount));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_GlDistributionQuantityIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.VouchGlEntityAssociation).ToList())
            {
                glDistribution.ItmVouGlQtyAssocMember = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var glDistribution in voucher.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.AreEqual(0, glDistribution.Quantity);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_GlDistributionAmountIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.VouchGlEntityAssociation).ToList())
            {
                glDistribution.ItmVouGlAmtAssocMember = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var glDistribution in voucher.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.AreEqual(0, glDistribution.Amount);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_VoucherHasCurrencyCode_GlDistributionForeignAmountIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.voucherDataContract.VouCurrencyCode = "CA";
            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.VouchGlEntityAssociation).ToList())
            {
                glDistribution.ItmVouGlForeignAmtAssocMember = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(0, voucher.Amount);
        }

        [TestMethod]
        public async Task GetVoucherAsync_GlDistributionProjectsListIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.VouchGlEntityAssociation).ToList())
            {
                glDistribution.ItmVouProjectCfIdAssocMember = null;
                glDistribution.ItmVouPrjItemIdsAssocMember = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var glDistribution in voucher.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_ProjectsAndProjectsLineItemsBulkReadsReturnNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectDataContracts = null;
            this.projectLineItemDataContracts = null;
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var glDistribution in voucher.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }
        #endregion

        #region Line Item Tax tests
        [TestMethod]
        public async Task GetVoucher_LineItemTaxes_AllLocalAmounts()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount);
            foreach (var domainLineItem in this.voucherDomainEntity.LineItems)
            {
                foreach (var domainTax in domainLineItem.LineItemTaxes)
                {
                    foreach (var lineItem in voucher.LineItems)
                    {
                        // Since we're comparing two voucher objects that SHOULD be the same
                        // we only want to execute the assertion if we know we're comparing the
                        // same line items.
                        if (domainLineItem.Id == lineItem.Id)
                        {
                            Assert.IsTrue(lineItem.LineItemTaxes.Any(x =>
                                x.TaxCode == domainTax.TaxCode
                                && x.TaxAmount == domainTax.TaxAmount));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetVoucher_LineItemTaxes_AllForeignAmounts()
        {
            string voucherId = "2";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount);
            foreach (var domainLineItem in this.voucherDomainEntity.LineItems)
            {
                foreach (var domainTax in domainLineItem.LineItemTaxes)
                {
                    foreach (var lineItem in voucher.LineItems)
                    {
                        // Since we're comparing two voucher objects that SHOULD be the same
                        // we only want to execute the assertion if we know we're comparing the
                        // same line items.
                        if (domainLineItem.Id == lineItem.Id)
                        {
                            Assert.IsTrue(lineItem.LineItemTaxes.Any(x =>
                                x.TaxCode == domainTax.TaxCode
                                && x.TaxAmount == domainTax.TaxAmount));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_LineItemTaxAssociationIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.VouGlTaxesEntityAssociation = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var lineItem in voucher.LineItems)
            {
                Assert.AreEqual(0, lineItem.LineItemTaxes.Count);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_LineItemTaxAssociationIsEmpty()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.VouGlTaxesEntityAssociation = new List<ItemsVouGlTaxes>();
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var lineItem in voucher.LineItems)
            {
                Assert.AreEqual(0, lineItem.LineItemTaxes.Count);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_LineItemTaxAmountIsNull()
        {
            string voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var taxDistribution in this.itemsDataContracts.SelectMany(x => x.VouGlTaxesEntityAssociation))
            {
                taxDistribution.ItmVouGlForeignTaxAmtAssocMember = null;
                taxDistribution.ItmVouGlTaxAmtAssocMember = null;
            }
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            foreach (var taxDistribution in voucher.LineItems.SelectMany(x => x.LineItemTaxes).ToList())
            {
                Assert.AreEqual(0, taxDistribution.TaxAmount);
            }
        }
        #endregion

        #region GL Security tests
        [TestMethod]
        public async Task UserHasFullAccess()
        {
            var voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount, "The voucher amounts should be the same.");
            Assert.AreEqual(this.voucherDomainEntity.LineItems.Count(), voucher.LineItems.Count(), "We should be able to see all of the voucher line items.");
            foreach (var lineItem in this.voucherDomainEntity.LineItems)
            {
                Assert.IsTrue(voucher.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.InvoiceNumber == lineItem.InvoiceNumber
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue));
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_AllLineItemsAvailable()
        {
            var voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount, "The voucher amounts should be the same.");
            Assert.AreEqual(this.voucherDomainEntity.LineItems.Count(), voucher.LineItems.Count(), "We should be able to see all of the voucher line items.");
            foreach (var lineItem in this.voucherDomainEntity.LineItems)
            {
                Assert.IsTrue(voucher.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.InvoiceNumber == lineItem.InvoiceNumber
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue));
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_PartialLineItemsAvailable()
        {
            var voucherId = "22";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount, "The voucher amount should show as if we have full access.");
            Assert.AreEqual(this.voucherDomainEntity.LineItems.Count(), voucher.LineItems.Count(), "Voucher should have all of it's line items.");

            decimal glDistributionTotal = 0.00m;
            decimal taxDistributionTotal = 0.00m;
            foreach (var lineItem in voucher.LineItems)
            {
                glDistributionTotal += lineItem.GlDistributions.Sum(x => x.Amount);
                taxDistributionTotal += lineItem.LineItemTaxes.Sum(x => x.TaxAmount);
            }
            Assert.AreEqual(this.voucherDomainEntity.Amount, glDistributionTotal + taxDistributionTotal, "Voucher amount should be the same as the sum of the GL and tax distributions for all line items");
            foreach (var lineItem in voucher.LineItems)
            {
                foreach (var glDistribution in lineItem.GlDistributions)
                {
                    if (glDistribution.GlAccountNumber == "11_10_00_01_20601_51000")
                    {
                        Assert.IsFalse(glDistribution.Masked, "GL number should NOT be masked.");
                    }
                    else
                    {
                        Assert.IsTrue(glDistribution.Masked, "GL number SHOULD be masked.");
                    }
                }
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_SomeLineItemsExcluded()
        {
            var voucherId = "23";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount, "The voucher amount should show as if we have full access.");

            var excludedLineItems = new List<LineItem>();
            foreach (var lineItem in voucher.LineItems)
            {
                excludedLineItems.AddRange(this.voucherDomainEntity.LineItems.Where(x => x.Id != lineItem.Id));
            }
            Assert.AreEqual(this.voucherDomainEntity.LineItems.Sum(x => x.ExtendedPrice),
                voucher.LineItems.Sum(x => x.ExtendedPrice) + excludedLineItems.Sum(x => x.ExtendedPrice), "The extended price should reflect which line items are included or excluded.");
            Assert.IsTrue(voucher.LineItems.Count() == 1, "Voucher should only have one line item.");
            foreach (var lineItem in voucher.LineItems)
            {
                foreach (var glDistribution in lineItem.GlDistributions)
                {
                    if (glDistribution.GlAccountNumber == "11_10_00_01_20601_52001")
                    {
                        Assert.IsFalse(glDistribution.Masked, "GL number should NOT be masked.");
                    }
                    else
                    {
                        Assert.IsTrue(glDistribution.Masked, "GL number SHOULD be masked.");
                    }
                }
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_NoLineItemsAvailable()
        {
            var voucherId = "24";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount, "The voucher amount should show as if we have full access.");
            Assert.IsTrue(voucher.LineItems.Count() == 0, "Voucher should have no line items.");
        }

        [TestMethod]
        public async Task UserHasNoAccess()
        {
            var voucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(voucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucher = await voucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.No_Access, expenseAccounts, versionNumber);

            Assert.AreEqual(this.voucherDomainEntity.Amount, voucher.Amount, "The voucher amount should show as if we have full access.");
            Assert.IsTrue(voucher.LineItems.Count() == 0, "Voucher should have no line items.");
        }
        #endregion
        #endregion

        #region Private methods
        private VoucherRepository BuildVoucherRepository()
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

            return new VoucherRepository(cacheProviderObject, transactionFactoryObject, loggerObject);
        }

        private void InitializeMockMethods()
        {
            // Mock ReadRecord to return a pre-defined Vouchers data contract
            dataReader.Setup<Task<Vouchers>>(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.voucherDataContract);
                });

            // Mock ReadRecord to return a pre-defined RcVouSchedules data contract
            dataReader.Setup<Task<RcVouSchedules>>(acc => acc.ReadRecordAsync<RcVouSchedules>(It.IsAny<string>(), true)).Returns(Task.FromResult(this.recurringVoucherDataContract));

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
            dataReader.Setup<Task<Collection<Opers>>>(acc => acc.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(opersResponse);
                });

            dataReader.Setup<Task<Collection<Projects>>>(acc => acc.BulkReadRecordAsync<Projects>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(projectDataContracts);
                });

            dataReader.Setup<Task<Collection<ProjectsLineItems>>>(acc => acc.BulkReadRecordAsync<ProjectsLineItems>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(projectLineItemDataContracts);
                });

            // Mock BulkReadRecord to return a list of Items data contracts
            dataReader.Setup<Task<Collection<Items>>>(acc => acc.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.itemsDataContracts);
                });

            // Mock Execute within the transaction invoker to return a GetGlAccountDescriptionResponse object
            transactionInvoker.Setup(tio => tio.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(It.IsAny<TxGetHierarchyNameRequest>())).Returns(() =>
                {
                    return this.hierarchyNameResponse;
                });
        }

        private List<string> CalculateGlAccountsForUser(string voucherId)
        {
            // Return a specific set of GL numbers depending on which voucher we're looking at
            var glNumbersToReturn = new List<string>();
            switch (voucherId)
            {
                case "22":
                    glNumbersToReturn = new List<string>() { "11_10_00_01_20601_51000" };
                    break;
                case "23":
                    glNumbersToReturn = new List<string>() { "11_10_00_01_20601_52001" };
                    break;
                case "24":
                    // Do nothing; we want to return an emply list.
                    break;
                default:
                    if (this.voucherDomainEntity.LineItems != null)
                    {
                        foreach (var lineItem in this.voucherDomainEntity.LineItems)
                        {
                            if (lineItem.GlDistributions != null)
                            {
                                foreach (var glDistribution in lineItem.GlDistributions)
                                {
                                    if (!glNumbersToReturn.Contains(glDistribution.GlAccountNumber))
                                    {
                                        glNumbersToReturn.Add(glDistribution.GlAccountNumber);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return glNumbersToReturn;
        }

        private void ConvertDomainEntitiesIntoDataContracts()
        {
            // Convert the Voucher object
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
            this.voucherDataContract.VouCheckNo = this.voucherDomainEntity.CheckNumber;
            this.voucherDataContract.VouCheckDate = this.voucherDomainEntity.CheckDate;

            this.voucherDataContract.VouComments = this.voucherDomainEntity.Comments;
            this.voucherDataContract.VouPoNo = this.voucherDomainEntity.PurchaseOrderId;
            this.voucherDataContract.VouBpoId = this.voucherDomainEntity.BlanketPurchaseOrderId;
            this.voucherDataContract.VouRcvsId = this.voucherDomainEntity.RecurringVoucherId;
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

            // Build a list of line item IDs
            this.voucherDataContract.VouItemsId = new List<string>();
            foreach (var lineItem in this.voucherDomainEntity.LineItems)
            {
                if (lineItem.Id != "null")
                {
                    this.voucherDataContract.VouItemsId.Add(lineItem.Id);
                }
            }

            // amount is cumulated using various line item associations

            // Build a list of Approver data contracts
            ConvertApproversIntoDataContracts();

            // Build a list of line items
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
}
