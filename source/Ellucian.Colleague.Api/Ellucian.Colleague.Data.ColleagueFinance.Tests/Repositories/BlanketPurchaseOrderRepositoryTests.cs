// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
    public class BlanketPurchaseOrderRepositoryTests
    {
        #region Initialize and Cleanup

        private Mock<IColleagueDataReader> dataReader = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private BlanketPurchaseOrderRepository blanketPurchaseOrderRepository;
        private TestBlanketPurchaseOrderRepository testBlanketPurchaseOrderRepository;
        private BlanketPurchaseOrder blanketPurchaseOrderDomainEntity;

        // Data contract objects
        private Bpo bpoDataContract;
        private Collection<Opers> opersDataContracts;
        private Collection<Opers> opersResponse;
        private Collection<Projects> projectDataContracts;
        private Collection<ProjectsLineItems> projectLineItemDataContracts;
        private GetHierarchyNamesForIdsResponse hierarchyNamesForIdsResponse;
        private TxCheckUserGlAccessResponse checkUserGlAccessResponse;
        private GetGlAccountDescriptionResponse getGlAccountDescriptionResponse;
        private Collection<Vouchers> vouchersDataContracts;
        private Collection<Items> itemsDataContracts;

        private string personId = "1";
        private string bpoIdForTransaction;

        [TestInitialize]
        public void Initialize()
        {
            // Set up a mock data reader
            dataReader = new Mock<IColleagueDataReader>();

            // Set up a mock transaction invoker
            transactionInvoker = new Mock<IColleagueTransactionInvoker>();

            // Initialize the data contract object.
            bpoDataContract = new Bpo();

            // Initialize the blanket purchase order repository
            testBlanketPurchaseOrderRepository = new TestBlanketPurchaseOrderRepository();
            bpoIdForTransaction = "";
            this.blanketPurchaseOrderRepository = BuildBlanketPurchaseOrderRepository();
            this.vouchersDataContracts = new Collection<Vouchers>();
            this.itemsDataContracts = new Collection<Items>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            dataReader = null;
            transactionInvoker = null;
            bpoDataContract = null;
            opersDataContracts = null;
            projectDataContracts = null;
            projectLineItemDataContracts = null;
            testBlanketPurchaseOrderRepository = null;
            blanketPurchaseOrderDomainEntity = null;
            hierarchyNamesForIdsResponse = null;
            checkUserGlAccessResponse = null;
            getGlAccountDescriptionResponse = null;
            opersResponse = null;
            bpoIdForTransaction = null;
            this.vouchersDataContracts = null;
            this.itemsDataContracts = null;
        }
        #endregion

        #region Base purchase order test
        [TestMethod]
        public async Task GetPurchaseOrder_Base()
        {
            string bpoId = "1";
            
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // Confirm that the SV properties for the blanket purchase order are the same
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Id, blanketPurchaseOrder.Id);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Number, blanketPurchaseOrder.Number);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.VendorId, blanketPurchaseOrder.VendorId);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.VendorName, blanketPurchaseOrder.VendorName);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.InitiatorName, blanketPurchaseOrder.InitiatorName);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Status, blanketPurchaseOrder.Status);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.StatusDate, blanketPurchaseOrder.StatusDate);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Amount, blanketPurchaseOrder.Amount);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.ApType, blanketPurchaseOrder.ApType);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Date, blanketPurchaseOrder.Date);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Description, blanketPurchaseOrder.Description);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.MaintenanceDate, blanketPurchaseOrder.MaintenanceDate);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.ExpirationDate, blanketPurchaseOrder.ExpirationDate);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Comments, blanketPurchaseOrder.Comments);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.InternalComments, blanketPurchaseOrder.InternalComments);
            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.CurrencyCode, blanketPurchaseOrder.CurrencyCode);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersData()
        {
            string bpoId = "1";

            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = this.vouchersDataContracts.Select(x => x.Recordkey).ToList();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                var selectedVoucherDistribution = this.itemsDataContracts.SelectMany(x => x.VouchGlEntityAssociation)
                    .FirstOrDefault(x => x.ItmVouGlNoAssocMember == glDistribution.GlAccountNumber
                        && x.ItmVouProjectCfIdAssocMember == glDistribution.ProjectId);
                var selectedTaxDistribution = this.itemsDataContracts.SelectMany(x => x.VouGlTaxesEntityAssociation)
                    .FirstOrDefault(x => x.ItmVouTaxGlNoAssocMember == glDistribution.GlAccountNumber
                        && x.ItmVouTaxProjectCfIdAssocMember == glDistribution.ProjectId);

                Assert.AreEqual(selectedVoucherDistribution.ItmVouGlAmtAssocMember.Value + selectedTaxDistribution.ItmVouGlTaxAmtAssocMember.Value, glDistribution.ExpensedAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersAmountsAreNull()
        {
            string bpoId = "1";

            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = this.vouchersDataContracts.Select(x => x.Recordkey).ToList();
            foreach (var glDist in this.itemsDataContracts.SelectMany(x => x.VouchGlEntityAssociation).ToList())
            {
                glDist.ItmVouGlAmtAssocMember = null;
            }

            foreach (var glDist in this.itemsDataContracts.SelectMany(x => x.VouGlTaxesEntityAssociation).ToList())
            {
                glDist.ItmVouGlTaxAmtAssocMember = null;
            }
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.AreEqual(0, glDistribution.ExpensedAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VendorId_NoVendorName()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            
            this.bpoDataContract.BpoVenName = new List<string>();
            this.bpoDataContract.BpoVendor = "0001234";
            this.hierarchyNamesForIdsResponse.IoPersonIds[0] = this.bpoDataContract.BpoVendor;
            this.hierarchyNamesForIdsResponse.OutPersonNames[1] = "Susty Corporation";
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            var index = this.hierarchyNamesForIdsResponse.IoPersonIds.IndexOf(this.bpoDataContract.BpoVendor);
            var expectedVendorName = this.hierarchyNamesForIdsResponse.OutPersonNames[index];
            Assert.AreEqual(expectedVendorName, blanketPurchaseOrder.VendorName);

            index = this.hierarchyNamesForIdsResponse.IoPersonIds.IndexOf(this.bpoDataContract.BpoInitiator);
            var expectedInitatorName = this.hierarchyNamesForIdsResponse.OutPersonNames[index];
            Assert.AreEqual(expectedInitatorName, blanketPurchaseOrder.InitiatorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HasMaintenanceDate()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var maintenanceDate = DateTime.Now.Date;
            this.bpoDataContract.BpoMaintGlTranDate = maintenanceDate;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(maintenanceDate, blanketPurchaseOrder.MaintenanceDate);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HasExpirationDate()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var expirationDate = DateTime.Now.Date;
            this.bpoDataContract.BpoExpireDate = expirationDate;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(expirationDate, blanketPurchaseOrder.ExpirationDate);
        }
        #endregion

        #region Invalid data tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrder_NullId()
        {
            var blanketPurchaseOrder = await this.blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(null, personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBlanketPurchaseOrder_NullBlanketPurchaseOrder()
        {
            // Mock ReadRecord to return a pre-defined, null blanket purchase orders data contract
            var nullBlanketPurchaseOrderObject = new Bpo();
            nullBlanketPurchaseOrderObject = null;
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(nullBlanketPurchaseOrderObject));
            var blanketPurchaseOrder = await this.blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBlanketPurchaseOrder_NullStatus()
        {
            // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
            var blanketPurchaseOrderObject = new Bpo()
            {
                Recordkey = "1",
                BpoStatus = null
            };
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
            var blanketPurchaseOrder = await this.blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBlanketPurchaseOrder_StatusListHasNullValue()
        {
            // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
            var blanketPurchaseOrderObject = new Bpo()
            {
                Recordkey = "1",
                BpoStatus = new List<string>() { null }
            };
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
            var blanketPurchaseOrder = await this.blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBlanketPurchaseOrder_StatusListHasEmptyValue()
        {
            // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
            var blanketPurchaseOrderObject = new Bpo()
            {
                Recordkey = "1",
                BpoStatus = new List<string>() { "" }
            };
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
            var blanketPurchaseOrder = await this.blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBlanketPurchaseOrder_InvalidStatus()
        {
            // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
            var blanketPurchaseOrderObject = new Bpo()
            {
                Recordkey = "1",
                BpoStatus = new List<string>() { "Z" }
            };
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
            var blanketPurchaseOrder = await this.blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBlanketPurchaseOrder_NullStatusDate()
        {
            // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
            var blanketPurchaseOrderObject = new Bpo()
            {
                Recordkey = "1",
                BpoStatus = new List<string>() { "O" },
                BpoStatusDate = null
            };
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
            var blanketPurchaseOrder = await this.blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBlanketPurchaseOrder_StatusDateListHasNullValue()
        {
            // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
            var blanketPurchaseOrderObject = new Bpo()
            {
                Recordkey = "1",
                BpoStatus = new List<string>() { "O" },
                BpoStatusDate = new List<DateTime?>() { null }
            };
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
            var blanketPurchaseOrder = await this.blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        //[TestMethod]
        //public async Task GetPurchaseOrder_VendorNameAndIdAreNull()
        //{
        //    var expectedMessage = "";
        //    var actualMessage = "";
        //    try
        //    {
        //        string bpoId = "1";
        //        this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
        //        var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
        //        ConvertDomainEntitiesIntoDataContracts();
        //        InitializeMockMethods();

        //        this.bpoDataContract.BpoVenName = null;
        //        this.bpoDataContract.BpoVendor = null;
        //        expectedMessage = "Missing vendor ID and vendor name for blanket purchase order: " + this.bpoDataContract.Recordkey;
        //        var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
        //    }
        //    catch (ApplicationException aex)
        //    {
        //        actualMessage = aex.Message;
        //    }

        //    Assert.AreEqual(expectedMessage, actualMessage);
        //}

        [TestMethod]
        public async Task GetPurchaseOrder_VendorNameAndIdAreEmpty()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVenName = new List<string>();
            this.bpoDataContract.BpoVendor = "";
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.IsTrue(string.IsNullOrEmpty(blanketPurchaseOrder.VendorName));
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HierarchyNamesCtxReturnsNullNamesList()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVenName = new List<string>() { "Susty Corporation" };
            this.hierarchyNamesForIdsResponse.OutPersonNames = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(this.bpoDataContract.BpoVenName.FirstOrDefault(), blanketPurchaseOrder.VendorName);
            Assert.IsNull(blanketPurchaseOrder.InitiatorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HierarchyNamesCtxReturnsEmptyNamesList()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVenName = new List<string>() { "Susty Corporation" };
            this.hierarchyNamesForIdsResponse.OutPersonNames = new List<string>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(this.bpoDataContract.BpoVenName.FirstOrDefault(), blanketPurchaseOrder.VendorName);
            Assert.IsNull(blanketPurchaseOrder.InitiatorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_BpoDateIsNull()
        {
            var expectedMessage = "";
            var actualMessage = "";
            try
            {
                string bpoId = "1";
                this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                expectedMessage = "Missing date for blanket purchase order: " + this.bpoDataContract.Recordkey;
                this.bpoDataContract.BpoDate = null;
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_TotalAmountIsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoTotalAmt = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Amount);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_RequisitionsListIsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoReqIds = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Requisitions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_RequisitionsListIsEmpty()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoReqIds = new List<string>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Requisitions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersListIsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Vouchers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersListIsEmpty()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = new List<string>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Vouchers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_OpersBulkReadReturnsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersResponse = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Approvers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_OpersBulkReadReturnsEmptyCollection()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersResponse = new Collection<Opers>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Approvers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ApprovalsAssociationIsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoAuthEntityAssociation = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(this.opersResponse.Count, blanketPurchaseOrder.Approvers.Count);

            foreach (var approver in blanketPurchaseOrder.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ApprovalsAssociationIsEmpty()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoAuthEntityAssociation = new List<BpoBpoAuth>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(this.opersResponse.Count, blanketPurchaseOrder.Approvers.Count);

            foreach (var approver in blanketPurchaseOrder.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ApprovalsAssociationHasNoDates()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var approval in this.bpoDataContract.BpoAuthEntityAssociation)
            {
                approval.BpoAuthorizationDatesAssocMember = null;
            }
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(this.opersResponse.Count, blanketPurchaseOrder.Approvers.Count);

            foreach (var approver in blanketPurchaseOrder.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_GlDistributionsListIsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoGlEntityAssociation = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.GlDistributions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_GlDistributionsListIsEmpty()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoGlEntityAssociation = new List<BpoBpoGl>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.GlDistributions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_GlDistributionsAmountIsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDistribution in this.bpoDataContract.BpoGlEntityAssociation)
            {
                glDistribution.BpoGlAmtAssocMember = null;
            }
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.AreEqual(0, glDistribution.EncumberedAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ProjectsBulkReadReturnsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectDataContracts = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ProjectsBulkReadReturnsEmpty()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectDataContracts = new Collection<Projects>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ProjectsLineItemsBulkReadReturnsNull()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectLineItemDataContracts = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ProjectsLineItemsBulkReadReturnsEmpty()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectLineItemDataContracts = new Collection<ProjectsLineItems>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersBulkReadReturnsNull()
        {
            string bpoId = "1";

            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = this.vouchersDataContracts.Select(x => x.Recordkey).ToList();
            this.vouchersDataContracts = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.AreEqual(0, glDistribution.ExpensedAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersBulkReadReturnsEmpty()
        {
            string bpoId = "1";

            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = this.vouchersDataContracts.Select(x => x.Recordkey).ToList();
            this.vouchersDataContracts = new Collection<Vouchers>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.AreEqual(0, glDistribution.ExpensedAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VoucherStatusIsU()
        {
            string bpoId = "1";

            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = this.vouchersDataContracts.Select(x => x.Recordkey).ToList();
            foreach (var dataContract in this.vouchersDataContracts)
            {
                dataContract.VouStatus = new List<string>() { "U" };
            }
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.AreEqual(0, glDistribution.ExpensedAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ItemsBulkReadReturnsNull()
        {
            string bpoId = "1";

            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = this.vouchersDataContracts.Select(x => x.Recordkey).ToList();
            this.itemsDataContracts = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.AreEqual(0, glDistribution.ExpensedAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ItemsBulkReadReturnsEmpty()
        {
            string bpoId = "1";

            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.bpoDataContract.BpoVouIds = this.vouchersDataContracts.Select(x => x.Recordkey).ToList();
            this.itemsDataContracts = new Collection<Items>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                Assert.AreEqual(0, glDistribution.ExpensedAmount);
            }
        }
        #endregion

        #region Status tests
        [TestMethod]
        public async Task GetBlanketPurchaseOrder_CStatus()
        {
            string bpoId = "5";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(BlanketPurchaseOrderStatus.Closed, blanketPurchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_NStatus()
        {
            string bpoId = "6";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(BlanketPurchaseOrderStatus.NotApproved, blanketPurchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_OStatus()
        {
            string bpoId = "4";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(BlanketPurchaseOrderStatus.Outstanding, blanketPurchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_UStatus()
        {
            string bpoId = "3";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(BlanketPurchaseOrderStatus.InProgress, blanketPurchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_VStatus()
        {
            string bpoId = "7";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(BlanketPurchaseOrderStatus.Voided, blanketPurchaseOrder.Status);
        }
        #endregion

        #region REQ, VOU tests

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_OriginatedFromReq()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Requisitions.Count(), blanketPurchaseOrder.Requisitions.Count());
            for (int x = 0; x < this.blanketPurchaseOrderDomainEntity.Requisitions.Count(); x++)
            {
                Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Requisitions[x], blanketPurchaseOrder.Requisitions[x]);
            }
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_IntoVouchers()
        {
            string bpoId = "4";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Vouchers.Count(), blanketPurchaseOrder.Vouchers.Count());
            for (int x = 0; x < this.blanketPurchaseOrderDomainEntity.Vouchers.Count(); x++)
            {
                Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Vouchers[x], blanketPurchaseOrder.Vouchers[x]);
            }
        }
        #endregion

        #region Approvers and Next Approvers tests

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_HasApproversAndNextApprovers()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.Approvers.Count(), blanketPurchaseOrder.Approvers.Count());
            foreach (var approver in this.blanketPurchaseOrderDomainEntity.Approvers)
            {
                Assert.IsTrue(blanketPurchaseOrder.Approvers.Any(x =>
                    x.ApproverId == approver.ApproverId
                    && x.ApprovalName == approver.ApprovalName
                    && x.ApprovalDate == approver.ApprovalDate));
            }
        }
        #endregion

        #region GL Distribution tests
        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_NullExpenseAccounts()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, null);
            Assert.AreEqual(0, blanketPurchaseOrder.GlDistributions.Count);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_GLDistributionHasNullGlNumber()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();

            // Set the first GL distribution GL number to null
            this.bpoDataContract.BpoGlEntityAssociation[0].BpoGlNoAssocMember = null;

            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // The BPO should have one less GL distribution than the data contract.
            Assert.AreEqual(this.bpoDataContract.BpoGlEntityAssociation.Count - 1, blanketPurchaseOrder.GlDistributions.Count);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_GLDistributionHasEmptyGlNumber()
        {
            string bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();

            // Set the first GL distribution GL number to null
            this.bpoDataContract.BpoGlEntityAssociation[0].BpoGlNoAssocMember = "";

            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // The BPO should have one less GL distribution than the data contract.
            Assert.AreEqual(this.bpoDataContract.BpoGlEntityAssociation.Count - 1, blanketPurchaseOrder.GlDistributions.Count);
        }
        #endregion

        #region GL Security tests

        [TestMethod]
        public async Task UserHasFullAccess()
        {
            var bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.GlDistributions.Count(), blanketPurchaseOrder.GlDistributions.Count(), "We should be able to see all of the blanket purchase order GL distributions.");
            foreach (var glDist in this.blanketPurchaseOrderDomainEntity.GlDistributions)
            {
                Assert.IsTrue(blanketPurchaseOrder.GlDistributions.Any(x =>
                    x.EncumberedAmount == glDist.EncumberedAmount
                    && x.GlAccountDescription == glDist.GlAccountDescription
                    && x.GlAccountNumber == glDist.GlAccountNumber
                    && x.ProjectId == glDist.ProjectId
                    && x.ProjectNumber == glDist.ProjectNumber
                    && x.ProjectLineItemCode == glDist.ProjectLineItemCode
                    && x.ProjectLineItemId == glDist.ProjectLineItemId));
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_PartialGLDistributionsAvailable()
        {
            var bpoId = "31";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.blanketPurchaseOrderDomainEntity.GlDistributions.Count()-1, blanketPurchaseOrder.GlDistributions.Count(), "The blanket purchase order should only have one GL distribution.");
            Assert.IsTrue(blanketPurchaseOrder.GlDistributions.Count() == 1, "The blanket purchase order should only have one GL distribution.");
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_NoGLDistributionsAvailable()
        {
            var bpoId = "32";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.IsTrue(blanketPurchaseOrder.GlDistributions.Count() == 0, "The blanket purchase order should have no GL distributions.");
        }

        [TestMethod]
        public async Task UserHasNoAccess()
        {
            var bpoId = "1";
            this.blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.No_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.No_Access, expenseAccounts);

            Assert.IsTrue(blanketPurchaseOrder.GlDistributions.Count() == 0, "The purchase order should have no GL distributions.");
        }

        #endregion

        #region Private methods

        private BlanketPurchaseOrderRepository BuildBlanketPurchaseOrderRepository()
        {
            // Instantiate all objects necessary to mock data reader and CTX calls
            var cacheProviderObject = new Mock<ICacheProvider>().Object;
            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            // The transaction factory has a method to get its data reader
            // Make sure that method returns our mock data reader
            transactionFactory.Setup(transFac => transFac.GetDataReader()).Returns(dataReader.Object);
            transactionFactory.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

            return new BlanketPurchaseOrderRepository(cacheProviderObject, transactionFactoryObject, loggerObject);
        }

        private void InitializeMockMethods()
        {
            // Mock ReadRecord to return a pre-defined Blanket Purchase Orders data contract
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.bpoDataContract);
                });

            // Mock ReadRecord to return a pre-defined Opers data contract.
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

            // Mock BulkReadRecord to return a list of Projects data contracts
            dataReader.Setup<Task<Collection<Projects>>>(acc => acc.BulkReadRecordAsync<Projects>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.projectDataContracts);
                });

            // Mock BulkReadRecord to return a list of ProjectsLineItems data contracts
            dataReader.Setup<Task<Collection<ProjectsLineItems>>>(acc => acc.BulkReadRecordAsync<ProjectsLineItems>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.projectLineItemDataContracts);
                });

            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transactionInvoker.Setup(tio => tio.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(this.hierarchyNamesForIdsResponse);

            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transactionInvoker.Setup(tio => tio.Execute<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(It.IsAny<GetGlAccountDescriptionRequest>())).Returns(this.getGlAccountDescriptionResponse);

            this.vouchersDataContracts = new Collection<Vouchers>()
            {
                new Vouchers()
                {
                    Recordkey = "V0000001",
                    VouStatus = new List<string>() { "O" },
                    VouItemsId = new List<string>() { "1" },
                },
            };

            this.itemsDataContracts = new Collection<Items>();
            foreach (var voucherDataContract in this.vouchersDataContracts)
            {
                var itemsDataContract = new Items()
                {
                    Recordkey = voucherDataContract.Recordkey,
                    ItmVouGlNo = new List<string>() { "11_10_00_01_20601_51000", "11_10_00_01_20601_51001", "11_10_00_01_20601_52001" },
                    ItmVouGlAmt = new List<decimal?>() { 100m, 200m, 300m },
                    ItmVouLineGlNo = new List<string>() { "11_10_00_01_20601_51000", "11_10_00_01_20601_51001", "11_10_00_01_20601_52001" },
                    ItmVouTaxGlNo = new List<string>() { "11_10_00_01_20601_51000", "11_10_00_01_20601_51001", "11_10_00_01_20601_52001" },
                    ItmVouGlTaxAmt = new List<decimal?>() { 10m, 20m, 30m },
                    ItmVouProjectCfId = new List<string>() { "100", null, "200" },
                    ItmVouPrjItemIds = new List<string>() { "50", null, "60" },
                    ItmVouTaxProjectCfId = new List<string>() { "100", null, "200" },
                    ItmVouTaxPrjItemIds = new List<string>() { "50", null, "60" },
                };
                itemsDataContract.buildAssociations();

                itemsDataContracts.Add(itemsDataContract);
            }

            dataReader.Setup(dr => dr.BulkReadRecordAsync<Vouchers>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.vouchersDataContracts);
                });

            dataReader.Setup(dr => dr.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.itemsDataContracts);
            });
        }

        private List<string> CalculateExpenseAccountsForUser(string blanketPurchaseOrderId)
        {
            // Return a specific set of GL numbers depending on which purchase order we are looking at
            var glNumbersToReturn = new List<string>();
            switch (blanketPurchaseOrderId)
            {
                case "31":
                    glNumbersToReturn = new List<string>() { "11_10_00_01_20601_51001" };
                    break;
                case "32":
                    // Do nothing; we want to return an empty list
                    break;
                default:
                    if (this.blanketPurchaseOrderDomainEntity.GlDistributions != null)
                    {
                        foreach (var lineItem in this.blanketPurchaseOrderDomainEntity.GlDistributions)
                        {
                            if ((blanketPurchaseOrderDomainEntity.GlDistributions != null) && (blanketPurchaseOrderDomainEntity.GlDistributions.Count > 0))
                            {
                                foreach (var glDistribution in blanketPurchaseOrderDomainEntity.GlDistributions)
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
            // Convert the Blanket Purchase Order object
            this.bpoDataContract.Recordkey = this.blanketPurchaseOrderDomainEntity.Id;
            this.bpoDataContract.BpoVendor = this.blanketPurchaseOrderDomainEntity.VendorId;

            if (this.blanketPurchaseOrderDomainEntity.VendorName == "null")
            {
                this.bpoDataContract.BpoVenName = new List<string>() { null };
            }
            else
            {
                this.bpoDataContract.BpoVenName = new List<string>() { this.blanketPurchaseOrderDomainEntity.VendorName };
            }

            // vendor name and initiator name come from CTX

            this.bpoDataContract.BpoInitiator = "0001687";

            // For the unit tests that use blanket purchase order 2, there is no vendor id, so we do not need to call the CTX
            if ((blanketPurchaseOrderDomainEntity.Id != "5") && (blanketPurchaseOrderDomainEntity.Id != "6"))
            {
                string ctxVendorName = "Ellucian Consulting, Inc.";
                this.hierarchyNamesForIdsResponse = new GetHierarchyNamesForIdsResponse()
                {
                    IoPersonIds = new List<string>() { this.blanketPurchaseOrderDomainEntity.VendorId, this.bpoDataContract.BpoInitiator },
                    IoHierarchies = new List<string>() { "PO", "PREFERRED" },
                    OutPersonNames = new List<string>() { ctxVendorName, this.blanketPurchaseOrderDomainEntity.InitiatorName }
                };
            }

            this.bpoDataContract.BpoNo = this.blanketPurchaseOrderDomainEntity.Number;
            this.bpoDataContract.BpoTotalAmt = this.blanketPurchaseOrderDomainEntity.Amount;
            this.bpoDataContract.BpoApType = this.blanketPurchaseOrderDomainEntity.ApType;
            this.bpoDataContract.BpoDate = this.blanketPurchaseOrderDomainEntity.Date;
            this.bpoDataContract.BpoDesc = this.blanketPurchaseOrderDomainEntity.Description;
            this.bpoDataContract.BpoMaintGlTranDate = this.blanketPurchaseOrderDomainEntity.MaintenanceDate;
            this.bpoDataContract.BpoExpireDate = this.blanketPurchaseOrderDomainEntity.ExpirationDate;
            this.bpoDataContract.BpoComments = this.blanketPurchaseOrderDomainEntity.InternalComments;
            this.bpoDataContract.BpoPrintedComments = this.blanketPurchaseOrderDomainEntity.Comments;
            this.bpoDataContract.BpoCurrencyCode = this.blanketPurchaseOrderDomainEntity.CurrencyCode;

            this.bpoDataContract.BpoStatus = new List<string>();
            switch (this.blanketPurchaseOrderDomainEntity.Status)
            {
                case BlanketPurchaseOrderStatus.Closed:
                    this.bpoDataContract.BpoStatus.Add("C");
                    break;
                case BlanketPurchaseOrderStatus.InProgress:
                    this.bpoDataContract.BpoStatus.Add("U");
                    break;
                case BlanketPurchaseOrderStatus.NotApproved:
                    this.bpoDataContract.BpoStatus.Add("N");
                    break;
                case BlanketPurchaseOrderStatus.Outstanding:
                    this.bpoDataContract.BpoStatus.Add("O");
                    break;
                case BlanketPurchaseOrderStatus.Voided:
                    this.bpoDataContract.BpoStatus.Add("V");
                    break;
                default:
                    throw new Exception("Invalid status specified in BlanketPurchaseOrderRepositoryTests");
            }

            // Build the blanket purchase order status date
            this.bpoDataContract.BpoStatusDate = new List<DateTime?>();
            this.bpoDataContract.BpoStatusDate.Add(this.blanketPurchaseOrderDomainEntity.StatusDate);

            // Build a list of requisitions related to the blanket purchase order
            this.bpoDataContract.BpoReqIds = new List<string>();
            foreach (var req in this.blanketPurchaseOrderDomainEntity.Requisitions)
            {
                if (!String.IsNullOrEmpty(req))
                {
                    this.bpoDataContract.BpoReqIds.Add(req);
                }
            }

            // Build a list of vouchers related to the blanket purchase order
            this.bpoDataContract.BpoVouIds = new List<string>();
            foreach (var vou in this.blanketPurchaseOrderDomainEntity.Vouchers)
            {
                if (!String.IsNullOrEmpty(vou))
                {
                    this.bpoDataContract.BpoVouIds.Add(vou);
                }
            }

            this.bpoDataContract.BpoGlEntityAssociation = new List<BpoBpoGl>();
            this.bpoDataContract.BpoGlNo = new List<string>();
            this.projectDataContracts = new Collection<Projects>();
            this.projectLineItemDataContracts = new Collection<ProjectsLineItems>();

            foreach (var glDist in this.blanketPurchaseOrderDomainEntity.GlDistributions)
            {
                var dataContract = new BpoBpoGl()
                {
                    BpoGlNoAssocMember = glDist.GlAccountNumber,
                    BpoGlAmtAssocMember = glDist.EncumberedAmount,
                    BpoGlForeignAmtAssocMember = null,
                    BpoGlPctAssocMember = null,
                    BpoGlBalanceAssocMember = null,
                    BpoProjectCfIdAssocMember = glDist.ProjectId,
                    BpoPrjItemIdsAssocMember = glDist.ProjectLineItemId,
                };
                this.bpoDataContract.BpoGlEntityAssociation.Add(dataContract);
                this.bpoDataContract.BpoGlNo.Add(glDist.GlAccountNumber);

                this.projectDataContracts.Add(new Projects()
                {
                    Recordkey = glDist.ProjectId,
                    PrjRefNo = glDist.ProjectNumber
                });

                this.projectLineItemDataContracts.Add(new ProjectsLineItems()
                {
                    Recordkey = glDist.ProjectLineItemId,
                    PrjlnProjectItemCode = glDist.ProjectLineItemCode
                });
            }

            this.getGlAccountDescriptionResponse = new GetGlAccountDescriptionResponse();
            var GlAccountIds = new List<string>();
            var GlAccountDescriptions = new List<string>();
            foreach (var glDist in this.blanketPurchaseOrderDomainEntity.GlDistributions)
            {
                GlAccountIds.Add(glDist.GlAccountNumber);
                GlAccountDescriptions.Add(glDist.GlAccountDescription);
            }
            getGlAccountDescriptionResponse.GlAccountIds = GlAccountIds;
            getGlAccountDescriptionResponse.GlDescriptions = GlAccountDescriptions;

            // Build a list of Approver data contracts
            ConvertApproversIntoDataContracts();
        }

        private void ConvertApproversIntoDataContracts()
        {
            // Initialize the associations for approvers and next approvers.
            this.bpoDataContract.BpoAuthEntityAssociation = new List<BpoBpoAuth>();
            this.bpoDataContract.BpoApprEntityAssociation = new List<BpoBpoAppr>();
            this.opersDataContracts = new Collection<Opers>();
            this.bpoDataContract.BpoAuthorizations = new List<string>();
            this.bpoDataContract.BpoNextApprovalIds = new List<string>();
            foreach (var approver in this.blanketPurchaseOrderDomainEntity.Approvers)
            {
                if (approver.ApprovalDate != null)
                {
                    // Populate approvers
                    var dataContract = new BpoBpoAuth()
                    {
                        BpoAuthorizationsAssocMember = approver.ApproverId,
                        BpoAuthorizationDatesAssocMember = approver.ApprovalDate
                    };

                    this.bpoDataContract.BpoAuthEntityAssociation.Add(dataContract);
                    this.bpoDataContract.BpoAuthorizations.Add(approver.ApproverId);
                }
                else
                {
                    // Populate next approvers
                    var nextApproverDataContract = new BpoBpoAppr()
                    {
                        BpoNextApprovalIdsAssocMember = approver.ApproverId
                    };
                    this.bpoDataContract.BpoApprEntityAssociation.Add(nextApproverDataContract);
                    this.bpoDataContract.BpoNextApprovalIds.Add(approver.ApproverId);
                }

                // Populate the Opers data contract
                this.opersDataContracts.Add(new Opers()
                {
                    Recordkey = approver.ApproverId,
                    SysUserName = approver.ApprovalName
                });
            }
        }

        #endregion
    }
}