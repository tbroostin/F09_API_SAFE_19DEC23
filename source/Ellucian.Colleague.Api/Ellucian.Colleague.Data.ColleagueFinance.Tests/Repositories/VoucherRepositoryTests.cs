// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.

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
        private Collection<GlAccts> glAcctsDataContracts;
        private Collection<Items> itemsDataContracts;
        private Collection<ProjectsLineItems> projectLineItemDataContracts;
        private TxGetHierarchyNameResponse hierarchyNameResponse;
        private Collection<Opers> opersResponse;

        private string personId = "1";
        private int versionNumber;

        private TxGetReimbursePersonAddressResponse personAddressResponse;
        private ProcurementDocumentFilterCriteria filterCriteria;

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

            filterCriteria = new ProcurementDocumentFilterCriteria() { PersonId = personId };
            
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
            glAcctsDataContracts = null;
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
            decimal? taxDistributionTotal = 0.00m;
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

        #region Voucher Create Test
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateVoucherAsync_NullcreateUpdateRequest()
        {
            var purchaseOrderCreate = await this.voucherRepository.CreateVoucherAsync(null);
        }

        [TestMethod]
        public async Task CreateVoucherAsync_TransactionError()
        {
            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = new Voucher("1", new DateTime(2017, 01, 01), VoucherStatus.InProgress, "")
            };
            // Mock Execute within the transaction invoker to return a TxCreateWebVouResponse object
            TxCreateWebVouResponse createVoucherResponse = new TxCreateWebVouResponse();
            createVoucherResponse.AVoucherId = "1";
            createVoucherResponse.AInvoiceNo = "123";
            createVoucherResponse.ARequestDate = new DateTime(2020, 02, 01);
            createVoucherResponse.AError = "true";
            createVoucherResponse.AlErrorMessages = new List<string>() { "Voucher locked" };

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxCreateWebVouRequest, TxCreateWebVouResponse>(It.IsAny<TxCreateWebVouRequest>())).ReturnsAsync(createVoucherResponse);

            var voucherCreateUpdate = await this.voucherRepository.CreateVoucherAsync(createUpdateRequestEntity);
            Assert.IsNotNull(voucherCreateUpdate);
            Assert.AreEqual(createVoucherResponse.AVoucherId, createUpdateRequestEntity.Voucher.Id);
            Assert.IsTrue(voucherCreateUpdate.ErrorOccured);
        }

        [TestMethod]
        public async Task CreateVoucherAsync_ValidCreateUpdateRequest()
        {
            var ssVoucherId = "31";
            var expenseAccounts = CalculateGlAccountsForUser(ssVoucherId);
            //Get the predefined values from Test Voucher repository
            var voucherData = await testVoucherRepository.GetVoucherAsync(ssVoucherId, "0000001", GlAccessLevel.Full_Access, expenseAccounts, 1);

            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "0000001",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = voucherData
            };
            // Mock Execute within the transaction invoker to return a TxCreateWebVouResponse object
            TxCreateWebVouResponse createVoucherResponse = new TxCreateWebVouResponse();
            createVoucherResponse.AVoucherId = "123";
            createVoucherResponse.AInvoiceNo = "123";
            createVoucherResponse.ARequestDate = new DateTime(2020, 02, 01);
            
            createVoucherResponse.AlErrorMessages = new List<string>();

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxCreateWebVouRequest, TxCreateWebVouResponse>(It.IsAny<TxCreateWebVouRequest>())).ReturnsAsync(createVoucherResponse);

            var voucherCreateUpdate = await this.voucherRepository.CreateVoucherAsync(createUpdateRequestEntity);
            Assert.IsNotNull(voucherCreateUpdate);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CreateVoucherAsync_Transaction_ThrowsException()
        {
            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = new Voucher("1", new DateTime(2017, 01, 01), VoucherStatus.InProgress, "")
            };
            // Mock Execute within the transaction invoker to return a TxCreateWebVouResponse object
            TxCreateWebVouResponse createVoucherResponse = new TxCreateWebVouResponse();
            createVoucherResponse.AVoucherId = "1";
            createVoucherResponse.AInvoiceNo = "123";
            createVoucherResponse.ARequestDate = new DateTime(2020, 02, 01);
            createVoucherResponse.AError = "true";
            createVoucherResponse.AlErrorMessages = new List<string>() { "Voucher locked" };

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxCreateWebVouRequest, TxCreateWebVouResponse>(It.IsAny<TxCreateWebVouRequest>())).Throws(new Exception());

            var voucherCreateUpdate = await this.voucherRepository.CreateVoucherAsync(createUpdateRequestEntity);
        }

        #endregion

        #region Reimburse Myself

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoucherRepository_GetReimbursePersonAddressForVoucherAsync_ArgumentNullException()
        {
            var actual = await voucherRepository.GetReimbursePersonAddressForVoucherAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task VoucherRepository_GetReimbursePersonAddressForVoucherAsync_ResponseIsNull()
        {
            transactionInvoker.Setup(t => t.ExecuteAsync<TxGetReimbursePersonAddressRequest, TxGetReimbursePersonAddressResponse>(It.IsAny<TxGetReimbursePersonAddressRequest>())).ReturnsAsync(() => null);
            await voucherRepository.GetReimbursePersonAddressForVoucherAsync(personId);
        }

        [Ignore] // not required now
        [TestMethod]
        public async Task VoucherRepository_GetReimbursePersonAddressForVoucherAsync_ResponseNoSearchResultforVendors()
        {
            personAddressResponse.APersonAddrId = null;

            transactionInvoker.Setup(t => t.ExecuteAsync<TxGetReimbursePersonAddressRequest, TxGetReimbursePersonAddressResponse>(It.IsAny<TxGetReimbursePersonAddressRequest>())).ReturnsAsync(personAddressResponse);
            var results = await voucherRepository.GetReimbursePersonAddressForVoucherAsync(personId);
            Assert.IsTrue(results.VendorId == null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task VoucherRepository_GetReimbursePersonAddressForVoucherAsync_ResponseHasErrorMessages()
        {
            personAddressResponse.AlErrorMessages = new List<string>() { "ErrorMessage1", "ErrorMessage2" };
            personAddressResponse.AError = true;
            transactionInvoker.Setup(t => t.ExecuteAsync<TxGetReimbursePersonAddressRequest, TxGetReimbursePersonAddressResponse>(It.IsAny<TxGetReimbursePersonAddressRequest>())).ReturnsAsync(personAddressResponse);
            await voucherRepository.GetReimbursePersonAddressForVoucherAsync(personId);
        }

        [TestMethod]
        public async Task VoucherRepository_GetReimbursePersonAddressForVoucherAsync_Success()
        {
            transactionInvoker.Setup(t => t.ExecuteAsync<TxGetReimbursePersonAddressRequest, TxGetReimbursePersonAddressResponse>(It.IsAny<TxGetReimbursePersonAddressRequest>())).ReturnsAsync(personAddressResponse);
            var result = await voucherRepository.GetReimbursePersonAddressForVoucherAsync(personId);

            Assert.AreEqual(result.VendorId, personAddressResponse.APersonId);
            Assert.AreEqual(result.VendorNameLines.FirstOrDefault(), personAddressResponse.APersonName);
            Assert.AreEqual(result.Country, personAddressResponse.APersonCountry);
            Assert.AreEqual(result.FormattedAddress, personAddressResponse.APersonFormattedAddress);
            Assert.AreEqual(result.City, personAddressResponse.APersonCity);

        }

        #endregion

        #region Void Voucher

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Vouchers_VoidVoucherAsync_ArgumentNullException_Voucher_Null()
        {
            await voucherRepository.VoidVoucherAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task Vouchers_VoidVoucherAsync_Transaction_Exception()
        {
            // Mock Execute within the transaction invoker to return a TxVoidVoucherResponse object

            TxVoidVoucherResponse voidVoucherResponse = new TxVoidVoucherResponse();

            voidVoucherResponse.AVoucherId = "123";
            voidVoucherResponse.AErrorOccurred = true;
            voidVoucherResponse.AlErrorMessages = new List<string>() { "Voucher locked" };

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxVoidVoucherRequest, TxVoidVoucherResponse>(It.IsAny<TxVoidVoucherRequest>())).ThrowsAsync(new Exception());

            VoucherVoidRequest voidRequest = new VoucherVoidRequest();
            voidRequest.PersonId = "0001234";
            voidRequest.VoucherId = "123";
            await voucherRepository.VoidVoucherAsync(voidRequest);
        }

        [TestMethod]
        public async Task Vouchers_VoidVoucherAsync_Transaction_Error()
        {
            // Mock Execute within the transaction invoker to return a TxVoidVoucherResponse object

            TxVoidVoucherResponse voidVoucherResponse = new TxVoidVoucherResponse();

            voidVoucherResponse.AVoucherId = "123";
            voidVoucherResponse.AErrorOccurred = true;
            voidVoucherResponse.AlErrorMessages = new List<string>() { "Voucher locked" };

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxVoidVoucherRequest, TxVoidVoucherResponse>(It.IsAny<TxVoidVoucherRequest>())).ReturnsAsync(voidVoucherResponse);

            VoucherVoidRequest voidRequest = new VoucherVoidRequest();
            voidRequest.PersonId = "0001234";
            voidRequest.VoucherId = "123";
            var result = await voucherRepository.VoidVoucherAsync(voidRequest);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.VoucherId, voidRequest.VoucherId);
            Assert.IsTrue(result.ErrorOccured);
        }

        [TestMethod]
        public async Task Vouchers_VoidVoucherAsync()
        {
            // Mock Execute within the transaction invoker to return a TxVoidVoucherResponse object

            TxVoidVoucherResponse voidVoucherResponse = new TxVoidVoucherResponse();

            voidVoucherResponse.AVoucherId = "123";
            voidVoucherResponse.AErrorOccurred = false;
            voidVoucherResponse.AlErrorMessages = null;
            voidVoucherResponse.AWarningOccurred = true;
            voidVoucherResponse.AlWarningMessages = new List<string>() { "Warning Occurred" };

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxVoidVoucherRequest, TxVoidVoucherResponse>(It.IsAny<TxVoidVoucherRequest>())).ReturnsAsync(voidVoucherResponse);

            VoucherVoidRequest voidRequest = new VoucherVoidRequest();
            voidRequest.PersonId = "0001234";
            voidRequest.VoucherId = "123";
            var result = await voucherRepository.VoidVoucherAsync(voidRequest);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.VoucherId, voidRequest.VoucherId);
            Assert.IsFalse(result.ErrorOccured);
        }

        #endregion

        #region Voucher Update Test
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateVoucherAsync_NullcreateUpdateRequest()
        {
            var ssVoucherId = "31";
            var expenseAccounts = CalculateGlAccountsForUser(ssVoucherId);
            //Get the predefined values from Test Voucher repository
            var voucherData = await testVoucherRepository.GetVoucherAsync(ssVoucherId, "0000001", GlAccessLevel.Full_Access, expenseAccounts, 1);

            var voucher = await this.voucherRepository.UpdateVoucherAsync(null, voucherData);
        }

        [TestMethod]
        public async Task UpdateVoucherAsync_TransactionError()
        {
            var ssVoucherId = "31";
            var expenseAccounts = CalculateGlAccountsForUser(ssVoucherId);
            //Get the predefined values from Test Voucher repository
            var voucherData = await testVoucherRepository.GetVoucherAsync(ssVoucherId, "0000001", GlAccessLevel.Full_Access, expenseAccounts, 1);

            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = new Voucher(ssVoucherId, new DateTime(2017, 01, 01), VoucherStatus.InProgress, "")
            };
            // Mock Execute within the transaction invoker to return a TxUpdateWebVoucherResponse object
            TxUpdateWebVoucherResponse updateVoucherResponse = new TxUpdateWebVoucherResponse();
            updateVoucherResponse.AVoucherId = ssVoucherId;
            updateVoucherResponse.AError = "true";
            updateVoucherResponse.AlErrorMessages = new List<string>() { "Voucher locked" };

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxUpdateWebVoucherRequest, TxUpdateWebVoucherResponse>(It.IsAny<TxUpdateWebVoucherRequest>())).ReturnsAsync(updateVoucherResponse);

            var voucherCreateUpdate = await this.voucherRepository.UpdateVoucherAsync(createUpdateRequestEntity, voucherData);
            Assert.IsNotNull(voucherCreateUpdate);
            Assert.AreEqual(updateVoucherResponse.AVoucherId, createUpdateRequestEntity.Voucher.Id);
            Assert.IsTrue(voucherCreateUpdate.ErrorOccured);
        }

        [TestMethod]
        public async Task UpdateVoucherAsync_ValidUpdateRequest()
        {
            var ssVoucherId = "31";
            var expenseAccounts = CalculateGlAccountsForUser(ssVoucherId);
            //Get the predefined values from Test Voucher repository
            var voucherData = await testVoucherRepository.GetVoucherAsync(ssVoucherId, "0000001", GlAccessLevel.Full_Access, expenseAccounts, 1);

            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "0000001",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = voucherData
            };
            // Mock Execute within the transaction invoker to return a TxUpdateWebVoucherResponse object
            TxUpdateWebVoucherResponse updateVoucherResponse = new TxUpdateWebVoucherResponse();
            updateVoucherResponse.AVoucherId = ssVoucherId;
            //updateVoucherResponse.AError = "false";
            updateVoucherResponse.AlErrorMessages = new List<string>();

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxUpdateWebVoucherRequest, TxUpdateWebVoucherResponse>(It.IsAny<TxUpdateWebVoucherRequest>())).ReturnsAsync(updateVoucherResponse);

            var voucherCreateUpdate = await this.voucherRepository.UpdateVoucherAsync(createUpdateRequestEntity, voucherData);
            Assert.IsNotNull(voucherCreateUpdate);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdateVoucherAsync_TransactionError_ThrowsException()
        {
            var ssVoucherId = "31";
            var expenseAccounts = CalculateGlAccountsForUser(ssVoucherId);
            //Get the predefined values from Test Voucher repository
            var voucherData = await testVoucherRepository.GetVoucherAsync(ssVoucherId, "0000001", GlAccessLevel.Full_Access, expenseAccounts, 1);

            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "0000001",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = voucherData
            };
            // Mock Execute within the transaction invoker to return a TxUpdateWebVoucherResponse object
            TxUpdateWebVoucherResponse updateVoucherResponse = new TxUpdateWebVoucherResponse();
            updateVoucherResponse.AVoucherId = ssVoucherId;
            updateVoucherResponse.AlErrorMessages = new List<string>();

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxUpdateWebVoucherRequest, TxUpdateWebVoucherResponse>(It.IsAny<TxUpdateWebVoucherRequest>())).Throws(new Exception());

            var voucherCreateUpdate = await this.voucherRepository.UpdateVoucherAsync(createUpdateRequestEntity, voucherData);
        }

        [TestMethod]
        public async Task UpdateVoucherAsync_AddNewLineItem_GlAccountMaskedRequest()
        {
            //Get the predefined values from Test Voucher repository
            string ssVoucherId = "22";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(ssVoucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(ssVoucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucherData = await voucherRepository.GetVoucherAsync(ssVoucherId, personId, GlAccessLevel.Possible_Access, expenseAccounts, versionNumber);
            Assert.AreEqual(this.voucherDomainEntity.LineItems.Count(), voucherData.LineItems.Count(), "Vouchers should have the same number of line items.");

            var voucherRequest = voucherData;
            var lineItems = voucherData.LineItems.ToList();
            var lineItem = voucherData.LineItems.FirstOrDefault();
            Assert.IsNotNull(lineItem);
            var newLinetItem = new LineItem("NEW", lineItem.Description, lineItem.Quantity, lineItem.Price, lineItem.ExtendedPrice);
            voucherRequest.AddLineItem(newLinetItem);


            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "0000001",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = voucherRequest
            };
            // Mock Execute within the transaction invoker to return a TxUpdateWebVoucherResponse object
            TxUpdateWebVoucherResponse updateVoucherResponse = new TxUpdateWebVoucherResponse();
            updateVoucherResponse.AVoucherId = ssVoucherId;
            updateVoucherResponse.AError = "false";
            updateVoucherResponse.AlErrorMessages = new List<string>();

            transactionInvoker.Setup(tio => tio.ExecuteAsync<TxUpdateWebVoucherRequest, TxUpdateWebVoucherResponse>(It.IsAny<TxUpdateWebVoucherRequest>())).ReturnsAsync(updateVoucherResponse);

            PrivateObject obj = new PrivateObject(this.voucherRepository);
            var result = obj.Invoke("BuildVoucherUpdateRequest", new object[] { createUpdateRequestEntity, voucherData });
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(TxUpdateWebVoucherRequest));
            TxUpdateWebVoucherRequest requestObject = (TxUpdateWebVoucherRequest)result;
            Assert.IsNotNull(requestObject);
            Assert.AreNotEqual(requestObject.AlUpdatedVouLineItems.Count(), voucherData.LineItems.Count());
        }

        [TestMethod]
        public async Task UpdateVoucherAsync_AddLineItem_Test()
        {
            //Get the predefined values from Test Voucher repository
            string ssVoucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(ssVoucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(ssVoucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucherData = await voucherRepository.GetVoucherAsync(ssVoucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);
            Assert.AreEqual(this.voucherDomainEntity.LineItems.Count(), voucherData.LineItems.Count(), "Vouchers should have the same number of line items.");

            Voucher voucherRequest = new Voucher(voucherData.Id, voucherData.Date, voucherData.Status, voucherData.VendorName);
            foreach (var item in voucherData.LineItems)
            {
                var li = new LineItem(item.Id, item.Description, item.Quantity, item.Price, item.ExtendedPrice);

                foreach (var glDist in item.GlDistributions)
                {
                    var glDistribution = new LineItemGlDistribution(glDist.GlAccountNumber, glDist.Quantity, glDist.Amount);
                    glDistribution.GlAccountDescription = glDist.GlAccountDescription;
                    glDistribution.Masked = glDist.Masked;
                    li.AddGlDistribution(glDistribution);
                }
                voucherRequest.AddLineItem(li);
            }
            var lineItem = voucherData.LineItems.FirstOrDefault(x => x.Id == "1");
            var newLinetItem = new LineItem("NEW", lineItem.Description, lineItem.Quantity, lineItem.Price, lineItem.ExtendedPrice);
            foreach (var glDist in lineItem.GlDistributions)
            {
                var glDistribution = new LineItemGlDistribution(glDist.GlAccountNumber, glDist.Quantity, glDist.Amount);
                glDistribution.GlAccountDescription = glDist.GlAccountDescription;
                glDistribution.Masked = glDist.Masked;
                newLinetItem.AddGlDistribution(glDistribution);
            }
            voucherRequest.AddLineItem(newLinetItem);

            foreach (var approver in voucherData.Approvers)
            {
                voucherRequest.AddApprover(new Approver(approver.ApproverId));
            }

            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "0000001",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = voucherRequest
            };

            PrivateObject obj = new PrivateObject(this.voucherRepository);
            var result = obj.Invoke("BuildVoucherUpdateRequest", new object[] { createUpdateRequestEntity, voucherData });
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(TxUpdateWebVoucherRequest));
            TxUpdateWebVoucherRequest requestObject = (TxUpdateWebVoucherRequest)result;
            Assert.IsNotNull(requestObject);
            Assert.AreEqual(requestObject.AlUpdatedVouLineItems.Count(), voucherData.LineItems.Count() + 1);
        }

        [TestMethod]
        public async Task UpdateVoucherAsync_DeleteLineItem_Test()
        {
            //Get the predefined values from Test Voucher repository
            string ssVoucherId = "1";
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(ssVoucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);
            var expenseAccounts = CalculateGlAccountsForUser(ssVoucherId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var voucherData = await voucherRepository.GetVoucherAsync(ssVoucherId, personId, GlAccessLevel.Full_Access, expenseAccounts, versionNumber);
            Assert.AreEqual(this.voucherDomainEntity.LineItems.Count(), voucherData.LineItems.Count(), "Vouchers should have the same number of line items.");

            Voucher voucherRequest = new Voucher(voucherData.Id, voucherData.Date, voucherData.Status, voucherData.VendorName);
            var items = voucherData.LineItems.Skip(1);
            foreach (var item in items)
            {
                var li = new LineItem(item.Id, item.Description, item.Quantity, item.Price, item.ExtendedPrice);

                foreach (var glDist in item.GlDistributions)
                {
                    var glDistribution = new LineItemGlDistribution(glDist.GlAccountNumber, glDist.Quantity, glDist.Amount);
                    glDistribution.GlAccountDescription = glDist.GlAccountDescription;
                    glDistribution.Masked = glDist.Masked;
                    li.AddGlDistribution(glDistribution);
                }
                voucherRequest.AddLineItem(li);
            }

            foreach (var approver in voucherData.Approvers)
            {
                voucherRequest.AddApprover(new Approver(approver.ApproverId));
            }

            VoucherCreateUpdateRequest createUpdateRequestEntity = new VoucherCreateUpdateRequest()
            {
                PersonId = "0000001",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                Voucher = voucherRequest
            };

            PrivateObject obj = new PrivateObject(this.voucherRepository);
            var result = obj.Invoke("BuildVoucherUpdateRequest", new object[] { createUpdateRequestEntity, voucherData });
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(TxUpdateWebVoucherRequest));
            TxUpdateWebVoucherRequest requestObject = (TxUpdateWebVoucherRequest)result;
            Assert.IsNotNull(requestObject);
            Assert.AreEqual(requestObject.AlUpdatedVouLineItems.Count(), voucherData.LineItems.Count());
            foreach (var actual in requestObject.AlUpdatedVouLineItems)
            {
                var expected = voucherData.LineItems.FirstOrDefault(x => x.Id == actual.AlLineItemIds);
                if (expected.Id == "1")
                {
                    Assert.AreEqual(expected.Id, actual.AlLineItemIds);
                    Assert.IsTrue(string.IsNullOrEmpty(actual.AlLineItemDescs));
                    Assert.IsTrue(string.IsNullOrEmpty(actual.AlLineItemQtys));
                    Assert.IsTrue(string.IsNullOrEmpty(actual.AlItemPrices));
                    Assert.IsTrue(string.IsNullOrEmpty(actual.AlItemUnitIssues));
                    Assert.IsTrue(string.IsNullOrEmpty(actual.AlItemVendorParts));
                    Assert.IsTrue(string.IsNullOrEmpty(actual.AlItemGlAccts));
                    Assert.IsTrue(string.IsNullOrEmpty(actual.AlItemGlAcctAmts));
                    Assert.IsTrue(string.IsNullOrEmpty(actual.AlItemProjectNos));
                }
                else
                {
                    Assert.AreEqual(expected.Description, actual.AlLineItemDescs);
                    Assert.AreEqual(expected.Quantity.ToString(), actual.AlLineItemQtys);
                    Assert.AreEqual(expected.Price.ToString(), actual.AlItemPrices);
                    Assert.AreEqual("11_10_00_01_20601_51000|11_10_00_01_20601_51001", actual.AlItemGlAccts);
                    Assert.AreEqual("175.00|125.00", actual.AlItemGlAcctAmts);
                    Assert.AreEqual("|", actual.AlItemProjectNos);
                }
            }
        }

        #endregion

        #region Get Voucher Summary Test

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_PersonIdNull()
        {
            var actual = await voucherRepository.GetVoucherSummariesByPersonIdAsync(null);
        }

        [TestMethod]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_null()
        {
            // Mock ReadRecord to return a pre-defined, null purchase orders data contract
            string[] emptyArray = new string[0];
            //mock SelectAsync to return empty array of string
            dataReader.Setup(acc => acc.SelectAsync("VOUCHERS", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(emptyArray);
            });
            
            var result = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_Base()
        {
            string voucherId = "1";
            InitDataForVoucherSummary();
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null,2);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();
            voucherDataContractList.Add(this.voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(),true)).ReturnsAsync(voucherDataContractList);
            
            var expectedVoucherSummaryList = await testVoucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
            var actual = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.ToList().Count == 1);

            var expectedVoucherSummary = expectedVoucherSummaryList.Where(x => x.Id == voucherId).FirstOrDefault();
            var actualVoucherSummary = actual.FirstOrDefault();

            //assert on entity properties
            Assert.AreEqual(expectedVoucherSummary.Id, actualVoucherSummary.Id);
            Assert.AreEqual(expectedVoucherSummary.Date, actualVoucherSummary.Date);
            Assert.AreEqual(expectedVoucherSummary.Status, actualVoucherSummary.Status);
            Assert.AreEqual(expectedVoucherSummary.VendorId, actualVoucherSummary.VendorId);
            Assert.AreEqual(expectedVoucherSummary.VendorName, actualVoucherSummary.VendorName);
            Assert.AreEqual(expectedVoucherSummary.Amount, actualVoucherSummary.Amount);
            Assert.AreEqual(expectedVoucherSummary.Approvers.Count, actualVoucherSummary.Approvers.Count);
            Assert.AreEqual(expectedVoucherSummary.Approvers.Where(a => a.ApprovalDate == null).ToList().Count, actualVoucherSummary.Approvers.Where(a => a.ApprovalDate == null).ToList().Count);
            Assert.AreEqual(expectedVoucherSummary.Approvers.Where(a => a.ApprovalDate != null).ToList().Count, actualVoucherSummary.Approvers.Where(a => a.ApprovalDate != null).ToList().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_HeirarchyName()
        {
            string voucherId = "26";
            InitDataForVoucherSummary();
            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();
            Collection<DataContracts.PurchaseOrders> poDataContractList = new Collection<DataContracts.PurchaseOrders>();
            Collection<DataContracts.Bpo> bpoDataContractList = new Collection<DataContracts.Bpo>();
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, 2);
            var voucherDataContract = new Vouchers()
            {
                Recordkey = "26",
                VouStatus = null,
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string>() { },
                VouPoNo = "P000001",
                VouBpoId = "BPO0001",
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };
            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "10",
                PoStatus = new List<string>() { "O" },
                PoReqIds = new List<string> { "1" },
                PoDefaultInitiator = "ABC",
                PoRequestor = "MlOwks",
                PoVendor = "ABC Company",
                PoMiscName = new List<string> { "ABC" }
            };

            var bpoDataContract = new Bpo()
            {
                Recordkey = "10",
                BpoStatus = new List<string>() { "O" },
                BpoVendor = "ABC Company"
            };
            poDataContractList.Add(purchaseOrderDataContract);
            bpoDataContractList.Add(bpoDataContract);
            voucherDataContractList.Add(voucherDataContract);
            
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(poDataContractList);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Bpo>("BPO", It.IsAny<string[]>(), true)).ReturnsAsync(bpoDataContractList);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_NullStatus()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = null,
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_StatusHasBlankValue()
        {
            string voucherId = "26";
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_StatusDateHasNullValue()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = new List<DateTime?>() { null },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_InvalidStatus()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "Z" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_NullStatusDate()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = null,
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVoucherSummariesByPersonIdAsync_Vouchers_NullVoucherDate()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = new List<DateTime?>() { new DateTime(2015, 1, 1) },
                VouDate = null,
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVoucherSummariesByPersonIdAsync(personId);
        }

        #endregion

        #region GetVouchersByVendorAndInvoiceNoAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_CriteriaNull()
        {
            var actual = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_VendorIDNull()
        {
            var actual = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(null, "1234");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_InvoiceNoNull()
        {
            var actual = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync("0000111", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_CriteriaEmpty()
        {
            var actual = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(string.Empty, string.Empty);
        }

        [TestMethod]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_null()
        {
            var invoiceNumber = "1234567";
            var vendorId = "0000111";
            
            // Mock ReadRecord to return a pre-defined, null purchase orders data contract
            string[] emptyArray = new string[0];
            //mock SelectAsync to return empty array of string
            dataReader.Setup(acc => acc.SelectAsync("VOUCHERS", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(emptyArray);
            });

            var result = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ToList().Count==0);
        }

        [TestMethod]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_Base()
        {
            string voucherId = "1";
            InitDataForVoucherSummary();
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, 2);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();
            voucherDataContractList.Add(this.voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);

            
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";


            var expectedVoucherSummaryList = await testVoucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
            var actual = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.ToList().Count == 1);

            var expectedVoucherSummary = expectedVoucherSummaryList.Where(x => x.Id == voucherId).FirstOrDefault();
            var actualVoucherSummary = actual.FirstOrDefault();

            //assert on entity properties
            Assert.AreEqual(expectedVoucherSummary.Id, actualVoucherSummary.Id);
            Assert.AreEqual(expectedVoucherSummary.Date, actualVoucherSummary.Date);
            Assert.AreEqual(expectedVoucherSummary.Status, actualVoucherSummary.Status);
            Assert.AreEqual(expectedVoucherSummary.VendorName, actualVoucherSummary.VendorName);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_HeirarchyName()
        {
            string voucherId = "26";
            InitDataForVoucherSummary();
            
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, 2);
            var voucherDataContract = new Vouchers()
            {
                Recordkey = "26",
                VouStatus = null,
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string>() { },
                VouPoNo = "P000001",
                VouBpoId = "BPO0001",
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };
            
            voucherDataContractList.Add(voucherDataContract);

            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_NullStatus()
        {
            InitDataForVoucherSummary();
            
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = null,
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
        }


        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_StatusHasBlankValue()
        {
            string voucherId = "26";
            InitDataForVoucherSummary();
            
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_StatusDateHasNullValue()
        {
            InitDataForVoucherSummary();
            
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = new List<DateTime?>() { null },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_InvalidStatus()
        {
            InitDataForVoucherSummary();
            
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "Z" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_NullStatusDate()
        {
            InitDataForVoucherSummary();
            
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = null,
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_GetVouchersByVendorAndInvoiceNoAsync_Vouchers_NullVoucherDate()
        {
            InitDataForVoucherSummary();
            
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = new List<DateTime?>() { new DateTime(2015, 1, 1) },
                VouDate = null,
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
        }

        #endregion

        #region query voucher summary Test

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_NullCriteria_ExceptionTest()
        {
            var actual = await voucherRepository.QueryVoucherSummariesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_null()
        {
            // Mock ReadRecord to return a pre-defined, null purchase orders data contract
            string[] emptyArray = new string[0];
            //mock SelectAsync to return empty array of string
            dataReader.Setup(acc => acc.SelectAsync("VOUCHERS", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(emptyArray);
            });

            var result = await voucherRepository.QueryVoucherSummariesAsync(new ProcurementDocumentFilterCriteria());
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_Base()
        {
            string voucherId = "1";
            InitDataForVoucherSummary();
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, 2);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();
            voucherDataContractList.Add(this.voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);

            var expectedVoucherSummaryList = await testVoucherRepository.QueryVoucherSummariesAsync(filterCriteria);
            var actual = await voucherRepository.QueryVoucherSummariesAsync(filterCriteria);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.ToList().Count == 1);

            var expectedVoucherSummary = expectedVoucherSummaryList.Where(x => x.Id == voucherId).FirstOrDefault();
            var actualVoucherSummary = actual.FirstOrDefault();

            //assert on entity properties
            Assert.AreEqual(expectedVoucherSummary.Id, actualVoucherSummary.Id);
            Assert.AreEqual(expectedVoucherSummary.Date, actualVoucherSummary.Date);
            Assert.AreEqual(expectedVoucherSummary.Status, actualVoucherSummary.Status);
            Assert.AreEqual(expectedVoucherSummary.VendorId, actualVoucherSummary.VendorId);
            Assert.AreEqual(expectedVoucherSummary.VendorName, actualVoucherSummary.VendorName);
            Assert.AreEqual(expectedVoucherSummary.Amount, actualVoucherSummary.Amount);
            Assert.AreEqual(expectedVoucherSummary.Approvers.Count, actualVoucherSummary.Approvers.Count);
            Assert.AreEqual(expectedVoucherSummary.Approvers.Where(a => a.ApprovalDate == null).ToList().Count, actualVoucherSummary.Approvers.Where(a => a.ApprovalDate == null).ToList().Count);
            Assert.AreEqual(expectedVoucherSummary.Approvers.Where(a => a.ApprovalDate != null).ToList().Count, actualVoucherSummary.Approvers.Where(a => a.ApprovalDate != null).ToList().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_HeirarchyName()
        {
            string voucherId = "26";
            InitDataForVoucherSummary();
            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();
            Collection<DataContracts.PurchaseOrders> poDataContractList = new Collection<DataContracts.PurchaseOrders>();
            Collection<DataContracts.Bpo> bpoDataContractList = new Collection<DataContracts.Bpo>();
            this.voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, 2);
            var voucherDataContract = new Vouchers()
            {
                Recordkey = "26",
                VouStatus = null,
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string>() { },
                VouPoNo = "P000001",
                VouBpoId = "BPO0001",
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };
            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "10",
                PoStatus = new List<string>() { "O" },
                PoReqIds = new List<string> { "1" },
                PoDefaultInitiator = "ABC",
                PoRequestor = "MlOwks",
                PoVendor = "ABC Company",
                PoMiscName = new List<string> { "ABC" }
            };

            var bpoDataContract = new Bpo()
            {
                Recordkey = "10",
                BpoStatus = new List<string>() { "O" },
                BpoVendor = "ABC Company"
            };
            poDataContractList.Add(purchaseOrderDataContract);
            bpoDataContractList.Add(bpoDataContract);
            voucherDataContractList.Add(voucherDataContract);

            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(poDataContractList);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Bpo>("BPO", It.IsAny<string[]>(), true)).ReturnsAsync(bpoDataContractList);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_NullStatus()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = null,
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_StatusHasBlankValue()
        {
            string voucherId = "26";
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_StatusDateHasNullValue()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = new List<DateTime?>() { null },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_InvalidStatus()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "Z" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_NullStatusDate()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = null,
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }

            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task VoucherRepository_QueryVoucherSummariesAsync_Vouchers_NullVoucherDate()
        {
            InitDataForVoucherSummary();

            Collection<DataContracts.Vouchers> voucherDataContractList = new Collection<DataContracts.Vouchers>();

            var voucherDataContract = new Vouchers()
            {
                Recordkey = "10",
                VouStatus = new List<string>() { "O" },
                VouRequestor = "0000001",
                VouVendor = "ABC Company",
                VouMiscName = new List<string> { "ABC" },
                VouStatusDate = new List<DateTime?>() { new DateTime(2015, 1, 1) },
                VouDate = null,
                VouAuthorizations = new List<string> { "0000001", "0000002" },
                VouNextApprovalIds = new List<string> { "0000003" }
            };

            voucherDataContractList.Add(voucherDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>(It.IsAny<string[]>(), true)).ReturnsAsync(voucherDataContractList);
            var voucher = await voucherRepository.QueryVoucherSummariesAsync(filterCriteria);
        }

        #endregion

        #endregion

        #region Private methods

        private async void InitDataForVoucherSummary()
        {
            string voucherId = "1";
            var voucherFilename = "VOUCHERS";
            var voucherIds = new List<string>()
            {
                voucherId
            };
            dataReader.Setup(dr => dr.SelectAsync(voucherFilename, It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(voucherIds.ToArray());
            });
            dataReader.Setup(dr => dr.SelectAsync(voucherFilename, It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(voucherIds.ToArray());
            });

        }

        private VoucherRepository BuildVoucherRepository()
        {
            // Instantiate all objects necessary to mock data reader and CTX calls.
            var cacheProviderObject = new Mock<ICacheProvider>().Object;
            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            personAddressResponse = new TxGetReimbursePersonAddressResponse()
            {

                AError = false,
                AlErrorMessages = new List<string>(),
                APersonId = "124",
                APersonName = "Blue Cross Office Shield",
                APersonAddress = "PO Box 69845",
                APersonCity = "Minneapolis",
                APersonState = "MN",
                APersonZip = "55430",
                APersonCountry = "USA",
                APersonFormattedAddress = "PO Box 69845 Minneapolis MN 55430 USA",
                APersonAddrId = "143"
            };

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

            dataReader.Setup<Task<Collection<GlAccts>>>(acc => acc.BulkReadRecordAsync<GlAccts>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(glAcctsDataContracts);
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
                case "31":
                    glNumbersToReturn = new List<string>() { "11_10_00_01_20601_51000", "11_10_00_01_20601_51001" };
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
            this.voucherDataContract.VouTotalAmt = this.voucherDomainEntity.Amount;
            

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

            this.voucherDataContract.VouStatusDate = new List<DateTime?>();
            this.voucherDataContract.VouStatusDate.Add(DateTime.Today);

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
            this.glAcctsDataContracts = new Collection<GlAccts>();
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

                    this.glAcctsDataContracts.Add(new GlAccts()
                    {
                        Recordkey = glDistr.GlAccountNumber
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
