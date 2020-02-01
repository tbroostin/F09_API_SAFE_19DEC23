// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.Exceptions;

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
            blanketPurchaseOrderRepository = BuildBlanketPurchaseOrderRepository();
            vouchersDataContracts = new Collection<Vouchers>();
            itemsDataContracts = new Collection<Items>();
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
            vouchersDataContracts = null;
            itemsDataContracts = null;
        }
        #endregion

        #region Base purchase order test
        [TestMethod]
        public async Task GetPurchaseOrder_Base()
        {
            string bpoId = "1";

            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // Confirm that the SV properties for the blanket purchase order are the same
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Id, blanketPurchaseOrder.Id);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Number, blanketPurchaseOrder.Number);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.VendorId, blanketPurchaseOrder.VendorId);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.VendorName, blanketPurchaseOrder.VendorName);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.InitiatorName, blanketPurchaseOrder.InitiatorName);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Status, blanketPurchaseOrder.Status);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.StatusDate, blanketPurchaseOrder.StatusDate);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Amount, blanketPurchaseOrder.Amount);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.ApType, blanketPurchaseOrder.ApType);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Date, blanketPurchaseOrder.Date);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Description, blanketPurchaseOrder.Description);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.MaintenanceDate, blanketPurchaseOrder.MaintenanceDate);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.ExpirationDate, blanketPurchaseOrder.ExpirationDate);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Comments, blanketPurchaseOrder.Comments);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.InternalComments, blanketPurchaseOrder.InternalComments);
            Assert.AreEqual(blanketPurchaseOrderDomainEntity.CurrencyCode, blanketPurchaseOrder.CurrencyCode);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersData()
        {
            string bpoId = "1";

            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = vouchersDataContracts.Select(x => x.Recordkey).ToList();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
            {
                var selectedVoucherDistribution = itemsDataContracts.SelectMany(x => x.VouchGlEntityAssociation)
                    .FirstOrDefault(x => x.ItmVouGlNoAssocMember == glDistribution.GlAccountNumber
                        && x.ItmVouProjectCfIdAssocMember == glDistribution.ProjectId);
                var selectedTaxDistribution = itemsDataContracts.SelectMany(x => x.VouGlTaxesEntityAssociation)
                    .FirstOrDefault(x => x.ItmVouTaxGlNoAssocMember == glDistribution.GlAccountNumber
                        && x.ItmVouTaxProjectCfIdAssocMember == glDistribution.ProjectId);

                Assert.AreEqual(selectedVoucherDistribution.ItmVouGlAmtAssocMember.Value + selectedTaxDistribution.ItmVouGlTaxAmtAssocMember.Value, glDistribution.ExpensedAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersAmountsAreNull()
        {
            string bpoId = "1";

            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = vouchersDataContracts.Select(x => x.Recordkey).ToList();
            foreach (var glDist in itemsDataContracts.SelectMany(x => x.VouchGlEntityAssociation).ToList())
            {
                glDist.ItmVouGlAmtAssocMember = null;
            }

            foreach (var glDist in itemsDataContracts.SelectMany(x => x.VouGlTaxesEntityAssociation).ToList())
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVenName = new List<string>();
            bpoDataContract.BpoVendor = "0001234";
            hierarchyNamesForIdsResponse.IoPersonIds[0] = bpoDataContract.BpoVendor;
            hierarchyNamesForIdsResponse.OutPersonNames[1] = "Susty Corporation";
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            var index = hierarchyNamesForIdsResponse.IoPersonIds.IndexOf(bpoDataContract.BpoVendor);
            var expectedVendorName = hierarchyNamesForIdsResponse.OutPersonNames[index];
            Assert.AreEqual(expectedVendorName, blanketPurchaseOrder.VendorName);

            index = hierarchyNamesForIdsResponse.IoPersonIds.IndexOf(bpoDataContract.BpoInitiator);
            var expectedInitatorName = hierarchyNamesForIdsResponse.OutPersonNames[index];
            Assert.AreEqual(expectedInitatorName, blanketPurchaseOrder.InitiatorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HasMaintenanceDate()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var maintenanceDate = DateTime.Now.Date;
            bpoDataContract.BpoMaintGlTranDate = maintenanceDate;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(maintenanceDate, blanketPurchaseOrder.MaintenanceDate);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HasExpirationDate()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var expirationDate = DateTime.Now.Date;
            bpoDataContract.BpoExpireDate = expirationDate;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(expirationDate, blanketPurchaseOrder.ExpirationDate);
        }
        #endregion

        #region Invalid data tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBlanketPurchaseOrder_NullId()
        {
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(null, personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBlanketPurchaseOrder_NullBlanketPurchaseOrder()
        {
            // Mock ReadRecord to return a pre-defined, null blanket purchase orders data contract
            var nullBlanketPurchaseOrderObject = new Bpo();
            nullBlanketPurchaseOrderObject = null;
            dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(nullBlanketPurchaseOrderObject));
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
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
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
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
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
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
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
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
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
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
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
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
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        //[TestMethod]
        //public async Task GetPurchaseOrder_VendorNameAndIdAreNull()
        //{
        //    var expectedMessage = "";
        //    var actualMessage = "";
        //    try
        //    {
        //        string bpoId = "1";
        //        blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
        //        var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
        //        ConvertDomainEntitiesIntoDataContracts();
        //        InitializeMockMethods();

        //        bpoDataContract.BpoVenName = null;
        //        bpoDataContract.BpoVendor = null;
        //        expectedMessage = "Missing vendor ID and vendor name for blanket purchase order: " + bpoDataContract.Recordkey;
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVenName = new List<string>();
            bpoDataContract.BpoVendor = "";
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.IsTrue(string.IsNullOrEmpty(blanketPurchaseOrder.VendorName));
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HierarchyNamesCtxReturnsNullNamesList()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVenName = new List<string>() { "Susty Corporation" };
            hierarchyNamesForIdsResponse.OutPersonNames = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(bpoDataContract.BpoVenName.FirstOrDefault(), blanketPurchaseOrder.VendorName);
            Assert.IsNull(blanketPurchaseOrder.InitiatorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HierarchyNamesCtxReturnsEmptyNamesList()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVenName = new List<string>() { "Susty Corporation" };
            hierarchyNamesForIdsResponse.OutPersonNames = new List<string>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(bpoDataContract.BpoVenName.FirstOrDefault(), blanketPurchaseOrder.VendorName);
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
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                expectedMessage = "Missing date for blanket purchase order: " + bpoDataContract.Recordkey;
                bpoDataContract.BpoDate = null;
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoTotalAmt = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Amount);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_RequisitionsListIsNull()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoReqIds = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Requisitions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_RequisitionsListIsEmpty()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoReqIds = new List<string>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Requisitions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersListIsNull()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Vouchers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VouchersListIsEmpty()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = new List<string>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Vouchers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_OpersBulkReadReturnsNull()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            opersResponse = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Approvers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_OpersBulkReadReturnsEmptyCollection()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            opersResponse = new Collection<Opers>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.Approvers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ApprovalsAssociationIsNull()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoAuthEntityAssociation = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(opersResponse.Count, blanketPurchaseOrder.Approvers.Count);

            foreach (var approver in blanketPurchaseOrder.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ApprovalsAssociationIsEmpty()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoAuthEntityAssociation = new List<BpoBpoAuth>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(opersResponse.Count, blanketPurchaseOrder.Approvers.Count);

            foreach (var approver in blanketPurchaseOrder.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ApprovalsAssociationHasNoDates()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var approval in bpoDataContract.BpoAuthEntityAssociation)
            {
                approval.BpoAuthorizationDatesAssocMember = null;
            }
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(opersResponse.Count, blanketPurchaseOrder.Approvers.Count);

            foreach (var approver in blanketPurchaseOrder.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_GlDistributionsListIsNull()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoGlEntityAssociation = null;
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.GlDistributions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_GlDistributionsListIsEmpty()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoGlEntityAssociation = new List<BpoBpoGl>();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(0, blanketPurchaseOrder.GlDistributions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_GlDistributionsAmountIsNull()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDistribution in bpoDataContract.BpoGlEntityAssociation)
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            projectDataContracts = null;
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            projectDataContracts = new Collection<Projects>();
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            projectLineItemDataContracts = null;
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            projectLineItemDataContracts = new Collection<ProjectsLineItems>();
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

            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = vouchersDataContracts.Select(x => x.Recordkey).ToList();
            vouchersDataContracts = null;
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

            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = vouchersDataContracts.Select(x => x.Recordkey).ToList();
            vouchersDataContracts = new Collection<Vouchers>();
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

            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = vouchersDataContracts.Select(x => x.Recordkey).ToList();
            foreach (var dataContract in vouchersDataContracts)
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

            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = vouchersDataContracts.Select(x => x.Recordkey).ToList();
            itemsDataContracts = null;
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

            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            bpoDataContract.BpoVouIds = vouchersDataContracts.Select(x => x.Recordkey).ToList();
            itemsDataContracts = new Collection<Items>();
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Requisitions.Count(), blanketPurchaseOrder.Requisitions.Count());
            for (int x = 0; x < blanketPurchaseOrderDomainEntity.Requisitions.Count(); x++)
            {
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Requisitions[x], blanketPurchaseOrder.Requisitions[x]);
            }
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_IntoVouchers()
        {
            string bpoId = "4";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Vouchers.Count(), blanketPurchaseOrder.Vouchers.Count());
            for (int x = 0; x < blanketPurchaseOrderDomainEntity.Vouchers.Count(); x++)
            {
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Vouchers[x], blanketPurchaseOrder.Vouchers[x]);
            }
        }
        #endregion

        #region Approvers and Next Approvers tests

        [TestMethod]
        public async Task GetBlanketPurchaseOrder_HasApproversAndNextApprovers()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(blanketPurchaseOrderDomainEntity.Approvers.Count(), blanketPurchaseOrder.Approvers.Count());
            foreach (var approver in blanketPurchaseOrderDomainEntity.Approvers)
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, null);
            Assert.AreEqual(0, blanketPurchaseOrder.GlDistributions.Count);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_GLDistributionHasNullGlNumber()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();

            // Set the first GL distribution GL number to null
            bpoDataContract.BpoGlEntityAssociation[0].BpoGlNoAssocMember = null;

            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // The BPO should have one less GL distribution than the data contract.
            Assert.AreEqual(bpoDataContract.BpoGlEntityAssociation.Count - 1, blanketPurchaseOrder.GlDistributions.Count);
        }

        [TestMethod]
        public async Task GetBlanketPurchaseOrderAsync_GLDistributionHasEmptyGlNumber()
        {
            string bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();

            // Set the first GL distribution GL number to null
            bpoDataContract.BpoGlEntityAssociation[0].BpoGlNoAssocMember = "";

            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // The BPO should have one less GL distribution than the data contract.
            Assert.AreEqual(bpoDataContract.BpoGlEntityAssociation.Count - 1, blanketPurchaseOrder.GlDistributions.Count);
        }
        #endregion

        #region GL Security tests

        [TestMethod]
        public async Task UserHasFullAccess()
        {
            var bpoId = "1";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(blanketPurchaseOrderDomainEntity.GlDistributions.Count(), blanketPurchaseOrder.GlDistributions.Count(), "We should be able to see all of the blanket purchase order GL distributions.");
            foreach (var glDist in blanketPurchaseOrderDomainEntity.GlDistributions)
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(blanketPurchaseOrderDomainEntity.GlDistributions.Count() - 1, blanketPurchaseOrder.GlDistributions.Count(), "The blanket purchase order should only have one GL distribution.");
            Assert.IsTrue(blanketPurchaseOrder.GlDistributions.Count() == 1, "The blanket purchase order should only have one GL distribution.");
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_NoGLDistributionsAvailable()
        {
            var bpoId = "32";
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.Possible_Access, null);
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
            blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.No_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(bpoId, personId, GlAccessLevel.No_Access, expenseAccounts);

            Assert.IsTrue(blanketPurchaseOrder.GlDistributions.Count() == 0, "The purchase order should have no GL distributions.");
        }

        #endregion

        #region GetGuidFromIdAsync tests

        [TestMethod]
        public async Task BPORepository_GetGuidFromIdAsync()
        {
            var id = "1";
            var guid = Guid.NewGuid().ToString();

            dataReader.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                   .ReturnsAsync(new Dictionary<string, GuidLookupResult>() {
                       { guid, new GuidLookupResult() { Entity = "BPO", PrimaryKey = id } } });
            dataReader.Setup(ep => ep.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() {
                    { "BPO+" + id , new RecordKeyLookupResult() { Guid = guid } } }));


            var result = await blanketPurchaseOrderRepository.GetGuidFromIdAsync(id, "BPO");

            Assert.IsNotNull(result);
            Assert.AreEqual(result, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task BPORepository_GetGuidFromIdAsync_ArgumentNullException()
        {
            var id = "1";
                      
            dataReader.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                   .ThrowsAsync(new ArgumentNullException());
            dataReader.Setup(ep => ep.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
               .ThrowsAsync(new ArgumentNullException());

           await blanketPurchaseOrderRepository.GetGuidFromIdAsync(id, "BPO"); 
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task BPORepository_GetGuidFromIdAsync_RepositoryException()
        {
            var id = "1";

            dataReader.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                   .ThrowsAsync(new RepositoryException());
            dataReader.Setup(ep => ep.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
               .ThrowsAsync(new RepositoryException());

            await blanketPurchaseOrderRepository.GetGuidFromIdAsync(id, "BPO");
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
                    return Task.FromResult(bpoDataContract);
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
                    return Task.FromResult(projectDataContracts);
                });

            // Mock BulkReadRecord to return a list of ProjectsLineItems data contracts
            dataReader.Setup<Task<Collection<ProjectsLineItems>>>(acc => acc.BulkReadRecordAsync<ProjectsLineItems>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(projectLineItemDataContracts);
                });

            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transactionInvoker.Setup(tio => tio.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(hierarchyNamesForIdsResponse);

            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transactionInvoker.Setup(tio => tio.Execute<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(It.IsAny<GetGlAccountDescriptionRequest>())).Returns(getGlAccountDescriptionResponse);

            vouchersDataContracts = new Collection<Vouchers>()
            {
                new Vouchers()
                {
                    Recordkey = "V0000001",
                    VouStatus = new List<string>() { "O" },
                    VouItemsId = new List<string>() { "1" },
                },
            };

            itemsDataContracts = new Collection<Items>();
            foreach (var voucherDataContract in vouchersDataContracts)
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
                    return Task.FromResult(vouchersDataContracts);
                });

            dataReader.Setup(dr => dr.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(itemsDataContracts);
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
                    if (blanketPurchaseOrderDomainEntity.GlDistributions != null)
                    {
                        foreach (var lineItem in blanketPurchaseOrderDomainEntity.GlDistributions)
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
            bpoDataContract.Recordkey = blanketPurchaseOrderDomainEntity.Id;
            bpoDataContract.BpoVendor = blanketPurchaseOrderDomainEntity.VendorId;

            if (blanketPurchaseOrderDomainEntity.VendorName == "null")
            {
                bpoDataContract.BpoVenName = new List<string>() { null };
            }
            else
            {
                bpoDataContract.BpoVenName = new List<string>() { blanketPurchaseOrderDomainEntity.VendorName };
            }

            // vendor name and initiator name come from CTX

            bpoDataContract.BpoInitiator = "0001687";

            // For the unit tests that use blanket purchase order 2, there is no vendor id, so we do not need to call the CTX
            if ((blanketPurchaseOrderDomainEntity.Id != "5") && (blanketPurchaseOrderDomainEntity.Id != "6"))
            {
                string ctxVendorName = "Ellucian Consulting, Inc.";
                hierarchyNamesForIdsResponse = new GetHierarchyNamesForIdsResponse()
                {
                    IoPersonIds = new List<string>() { blanketPurchaseOrderDomainEntity.VendorId, bpoDataContract.BpoInitiator },
                    IoHierarchies = new List<string>() { "PO", "PREFERRED" },
                    OutPersonNames = new List<string>() { ctxVendorName, blanketPurchaseOrderDomainEntity.InitiatorName }
                };
            }

            bpoDataContract.BpoNo = blanketPurchaseOrderDomainEntity.Number;
            bpoDataContract.BpoTotalAmt = blanketPurchaseOrderDomainEntity.Amount;
            bpoDataContract.BpoApType = blanketPurchaseOrderDomainEntity.ApType;
            bpoDataContract.BpoDate = blanketPurchaseOrderDomainEntity.Date;
            bpoDataContract.BpoDesc = blanketPurchaseOrderDomainEntity.Description;
            bpoDataContract.BpoMaintGlTranDate = blanketPurchaseOrderDomainEntity.MaintenanceDate;
            bpoDataContract.BpoExpireDate = blanketPurchaseOrderDomainEntity.ExpirationDate;
            bpoDataContract.BpoComments = blanketPurchaseOrderDomainEntity.InternalComments;
            bpoDataContract.BpoPrintedComments = blanketPurchaseOrderDomainEntity.Comments;
            bpoDataContract.BpoCurrencyCode = blanketPurchaseOrderDomainEntity.CurrencyCode;

            bpoDataContract.BpoStatus = new List<string>();
            switch (blanketPurchaseOrderDomainEntity.Status)
            {
                case BlanketPurchaseOrderStatus.Closed:
                    bpoDataContract.BpoStatus.Add("C");
                    break;
                case BlanketPurchaseOrderStatus.InProgress:
                    bpoDataContract.BpoStatus.Add("U");
                    break;
                case BlanketPurchaseOrderStatus.NotApproved:
                    bpoDataContract.BpoStatus.Add("N");
                    break;
                case BlanketPurchaseOrderStatus.Outstanding:
                    bpoDataContract.BpoStatus.Add("O");
                    break;
                case BlanketPurchaseOrderStatus.Voided:
                    bpoDataContract.BpoStatus.Add("V");
                    break;
                default:
                    throw new Exception("Invalid status specified in BlanketPurchaseOrderRepositoryTests");
            }

            // Build the blanket purchase order status date
            bpoDataContract.BpoStatusDate = new List<DateTime?>();
            bpoDataContract.BpoStatusDate.Add(blanketPurchaseOrderDomainEntity.StatusDate);

            // Build a list of requisitions related to the blanket purchase order
            bpoDataContract.BpoReqIds = new List<string>();
            foreach (var req in blanketPurchaseOrderDomainEntity.Requisitions)
            {
                if (!String.IsNullOrEmpty(req))
                {
                    bpoDataContract.BpoReqIds.Add(req);
                }
            }

            // Build a list of vouchers related to the blanket purchase order
            bpoDataContract.BpoVouIds = new List<string>();
            foreach (var vou in blanketPurchaseOrderDomainEntity.Vouchers)
            {
                if (!String.IsNullOrEmpty(vou))
                {
                    bpoDataContract.BpoVouIds.Add(vou);
                }
            }

            bpoDataContract.BpoGlEntityAssociation = new List<BpoBpoGl>();
            bpoDataContract.BpoGlNo = new List<string>();
            projectDataContracts = new Collection<Projects>();
            projectLineItemDataContracts = new Collection<ProjectsLineItems>();

            foreach (var glDist in blanketPurchaseOrderDomainEntity.GlDistributions)
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
                bpoDataContract.BpoGlEntityAssociation.Add(dataContract);
                bpoDataContract.BpoGlNo.Add(glDist.GlAccountNumber);

                projectDataContracts.Add(new Projects()
                {
                    Recordkey = glDist.ProjectId,
                    PrjRefNo = glDist.ProjectNumber
                });

                projectLineItemDataContracts.Add(new ProjectsLineItems()
                {
                    Recordkey = glDist.ProjectLineItemId,
                    PrjlnProjectItemCode = glDist.ProjectLineItemCode
                });
            }

            getGlAccountDescriptionResponse = new GetGlAccountDescriptionResponse();
            var GlAccountIds = new List<string>();
            var GlAccountDescriptions = new List<string>();
            foreach (var glDist in blanketPurchaseOrderDomainEntity.GlDistributions)
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
            bpoDataContract.BpoAuthEntityAssociation = new List<BpoBpoAuth>();
            bpoDataContract.BpoApprEntityAssociation = new List<BpoBpoAppr>();
            opersDataContracts = new Collection<Opers>();
            bpoDataContract.BpoAuthorizations = new List<string>();
            bpoDataContract.BpoNextApprovalIds = new List<string>();
            foreach (var approver in blanketPurchaseOrderDomainEntity.Approvers)
            {
                if (approver.ApprovalDate != null)
                {
                    // Populate approvers
                    var dataContract = new BpoBpoAuth()
                    {
                        BpoAuthorizationsAssocMember = approver.ApproverId,
                        BpoAuthorizationDatesAssocMember = approver.ApprovalDate
                    };

                    bpoDataContract.BpoAuthEntityAssociation.Add(dataContract);
                    bpoDataContract.BpoAuthorizations.Add(approver.ApproverId);
                }
                else
                {
                    // Populate next approvers
                    var nextApproverDataContract = new BpoBpoAppr()
                    {
                        BpoNextApprovalIdsAssocMember = approver.ApproverId
                    };
                    bpoDataContract.BpoApprEntityAssociation.Add(nextApproverDataContract);
                    bpoDataContract.BpoNextApprovalIds.Add(approver.ApproverId);
                }

                // Populate the Opers data contract
                opersDataContracts.Add(new Opers()
                {
                    Recordkey = approver.ApproverId,
                    SysUserName = approver.ApprovalName
                });
            }
        }

        #endregion
    }

    [TestClass]
    public class BlanketPurchaseOrderRepositoryTests_V16_0_0
    {
        [TestClass]
        public class BlanketPurchaseOrderRepositoryTests_GET
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
            private GetGlAccountDescriptionResponse getGlAccountDescriptionResponse;

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
                blanketPurchaseOrderRepository = BuildBlanketPurchaseOrderRepository();
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
                getGlAccountDescriptionResponse = null;
                opersResponse = null;
            }
            #endregion

            #region Base purchase order test
            [TestMethod]
            public async Task GetPurchaseOrder_Base()
            {
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);

                // Confirm that the SV properties for the blanket purchase order are the same
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Id, blanketPurchaseOrder.Id);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Number, blanketPurchaseOrder.Number);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.VendorId, blanketPurchaseOrder.VendorId);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.InitiatorName, blanketPurchaseOrder.InitiatorName);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Status, blanketPurchaseOrder.Status);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.StatusDate, blanketPurchaseOrder.StatusDate);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Amount, blanketPurchaseOrder.Amount);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.ApType, blanketPurchaseOrder.ApType);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Date, blanketPurchaseOrder.Date);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Description, blanketPurchaseOrder.Description);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.MaintenanceDate, blanketPurchaseOrder.MaintenanceDate);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.ExpirationDate, blanketPurchaseOrder.ExpirationDate);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Comments, blanketPurchaseOrder.Comments);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.InternalComments, blanketPurchaseOrder.InternalComments);
                Assert.AreEqual(blanketPurchaseOrderDomainEntity.CurrencyCode, blanketPurchaseOrder.CurrencyCode);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_HasMaintenanceDate()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                var maintenanceDate = DateTime.Now.Date;
                bpoDataContract.BpoMaintGlTranDate = maintenanceDate;
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                Assert.AreEqual(maintenanceDate, blanketPurchaseOrder.MaintenanceDate);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_HasExpirationDate()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                var expirationDate = DateTime.Now.Date;
                bpoDataContract.BpoExpireDate = expirationDate;
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                Assert.AreEqual(expirationDate, blanketPurchaseOrder.ExpirationDate);
            }
            #endregion

            #region Invalid data tests
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetBlanketPurchaseOrder_NullId()
            {
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetBlanketPurchaseOrder_NullBlanketPurchaseOrder()
            {
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase orders data contract
                var nullBlanketPurchaseOrderObject = new Bpo();
                nullBlanketPurchaseOrderObject = null;

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(nullBlanketPurchaseOrderObject));
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetBlanketPurchaseOrder_NullStatus()
            {
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Bpo()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    BpoStatus = null
                };

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetBlanketPurchaseOrder_StatusListHasNullValue()
            {
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Bpo()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    BpoStatus = new List<string>() { null }
                };

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetBlanketPurchaseOrder_StatusListHasEmptyValue()
            {
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Bpo()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    BpoStatus = new List<string>() { "" }
                };

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetBlanketPurchaseOrder_InvalidStatus()
            {
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Bpo()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    BpoStatus = new List<string>() { "Z" }
                };

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetBlanketPurchaseOrder_NullStatusDate()
            {
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Bpo()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    BpoStatus = new List<string>() { "O" },
                    BpoStatusDate = null
                };

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetBlanketPurchaseOrder_StatusDateListHasNullValue()
            {
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Bpo()
                {
                    Recordkey = "1",
                    RecordGuid = guid,
                    BpoStatus = new List<string>() { "O" },
                    BpoStatusDate = new List<DateTime?>() { null }
                };

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                dataReader.Setup<Task<Bpo>>(acc => acc.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).Returns(Task.FromResult(blanketPurchaseOrderObject));
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_BpoDateIsNull()
            {
                var expectedMessage = "";
                var actualMessage = "";
                try
                {
                    string bpoId = "1";
                    string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                    blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                    var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                    ConvertDomainEntitiesIntoDataContracts();
                    InitializeMockMethods();

                    expectedMessage = "Missing date for blanket purchase order '" + bpoDataContract.BpoNo + "'";
                    bpoDataContract.BpoDate = null;
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_TotalAmountIsNull()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                bpoDataContract.BpoTotalAmt = null;
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                Assert.AreEqual(0, blanketPurchaseOrder.Amount);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_RequisitionsListIsNull()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                bpoDataContract.BpoReqIds = null;
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                Assert.AreEqual(0, blanketPurchaseOrder.Requisitions.Count);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_RequisitionsListIsEmpty()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                bpoDataContract.BpoReqIds = new List<string>();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                Assert.AreEqual(0, blanketPurchaseOrder.Requisitions.Count);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_GlDistributionsListIsNull()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                bpoDataContract.BpoGlEntityAssociation = null;
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                Assert.AreEqual(0, blanketPurchaseOrder.GlDistributions.Count);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_GlDistributionsListIsEmpty()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                bpoDataContract.BpoGlEntityAssociation = new List<BpoBpoGl>();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                Assert.AreEqual(0, blanketPurchaseOrder.GlDistributions.Count);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_GlDistributionsAmountIsNull()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                foreach (var glDistribution in bpoDataContract.BpoGlEntityAssociation)
                {
                    glDistribution.BpoGlAmtAssocMember = null;
                }
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
                {
                    Assert.AreEqual(0, glDistribution.EncumberedAmount);
                }
            }

            [TestMethod]
            public async Task GetPurchaseOrder_ProjectsBulkReadReturnsNull()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                projectDataContracts = null;
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
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
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                projectDataContracts = new Collection<Projects>();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
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
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                projectLineItemDataContracts = null;
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
                {
                    Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
                }
            }

            [TestMethod]
            public async Task GetPurchaseOrder_ProjectsLineItemsBulkReadReturnsEmpty()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                projectLineItemDataContracts = new Collection<ProjectsLineItems>();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
                {
                    Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
                }
            }
            #endregion

            #region Status tests
            [TestMethod]
            public async Task GetBlanketPurchaseOrder_CStatus()
            {
                string bpoId = "5";
                string guid = "f49d6308-363c-4ca3-97e3-bb2753e2b726";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);

                Assert.AreEqual(BlanketPurchaseOrderStatus.Closed, blanketPurchaseOrder.Status);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_NStatus()
            {
                string bpoId = "6";
                string guid = "599bf3f5-6565-4063-823a-6fe5200c1555";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);

                Assert.AreEqual(BlanketPurchaseOrderStatus.NotApproved, blanketPurchaseOrder.Status);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_OStatus()
            {
                string bpoId = "4";
                string guid = "82d06e63-2fee-486a-8136-4faa01988752";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);

                Assert.AreEqual(BlanketPurchaseOrderStatus.Outstanding, blanketPurchaseOrder.Status);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_UStatus()
            {
                string bpoId = "3";
                string guid = "c03bc9df-fbbd-4716-ab16-f978c3816dcf";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                var expectedMessage = "";
                var actualMessage = "";
                try
                {
                    expectedMessage = "Blanket Purchase Orders in an unfinished state are not supported. '" + bpoDataContract.BpoNo + "'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_VStatus()
            {
                string bpoId = "7";
                string guid = "bf6dc54f-df34-4e25-a329-59a235a1197d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);

                Assert.AreEqual(BlanketPurchaseOrderStatus.Voided, blanketPurchaseOrder.Status);
            }
            #endregion

            #region REQ, VOU tests

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_OriginatedFromReq()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);

                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Requisitions.Count(), blanketPurchaseOrder.Requisitions.Count());
                for (int x = 0; x < blanketPurchaseOrderDomainEntity.Requisitions.Count(); x++)
                {
                    Assert.AreEqual(blanketPurchaseOrderDomainEntity.Requisitions[x], blanketPurchaseOrder.Requisitions[x]);
                }
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_IntoVouchers()
            {
                string bpoId = "4";
                string guid = "82d06e63-2fee-486a-8136-4faa01988752";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();
                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);

                Assert.AreEqual(blanketPurchaseOrderDomainEntity.Vouchers.Count(), blanketPurchaseOrder.Vouchers.Count());
                for (int x = 0; x < blanketPurchaseOrderDomainEntity.Vouchers.Count(); x++)
                {
                    Assert.AreEqual(blanketPurchaseOrderDomainEntity.Vouchers[x], blanketPurchaseOrder.Vouchers[x]);
                }
            }
            #endregion

            #region GL Distribution tests
            [TestMethod]
            public async Task GetBlanketPurchaseOrderAsync_GLDistributionHasNullGlNumber()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();

                // Set the first GL distribution GL number to null
                bpoDataContract.BpoGlEntityAssociation[0].BpoGlNoAssocMember = null;

                InitializeMockMethods();

                var expectedMessage = "";
                var actualMessage = "";
                try
                {
                    expectedMessage = "GL distribution account number cannot be null '" + bpoDataContract.BpoNo + "'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrderAsync_GLDistributionHasEmptyGlNumber()
            {
                string bpoId = "1";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";
                blanketPurchaseOrderDomainEntity = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                var expenseAccounts = CalculateExpenseAccountsForUser(bpoId);
                ConvertDomainEntitiesIntoDataContracts();

                // Set the first GL distribution GL number to null
                bpoDataContract.BpoGlEntityAssociation[0].BpoGlNoAssocMember = "";

                InitializeMockMethods();

                var expectedMessage = "";
                var actualMessage = "";
                try
                {
                    expectedMessage = "GL distribution account number cannot be null '" + bpoDataContract.BpoNo + "'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
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
                    return Task.FromResult(bpoDataContract);
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
                    return Task.FromResult(projectDataContracts);
                });

                // Mock BulkReadRecord to return a list of ProjectsLineItems data contracts
                dataReader.Setup<Task<Collection<ProjectsLineItems>>>(acc => acc.BulkReadRecordAsync<ProjectsLineItems>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(projectLineItemDataContracts);
                });

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                // Mock international parameters for host country setup.
                var intlParams = new Base.DataContracts.IntlParams()
                {
                    HostCountry = "USA",
                    HostDateDelimiter = "/",
                    HostShortDateFormat = "MDY"
                };
                dataReader.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(intlParams);
                
                // Mock Purchase Defaults
                dataReader.Setup(d => d.ReadRecordAsync<PurDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new PurDefaults() { PurShipToCode = "1" });

                // Mock Initiator Names
                var person = new Base.DataContracts.Person()
                 {
                     FirstName = !string.IsNullOrEmpty(blanketPurchaseOrderDomainEntity.InitiatorName) ? blanketPurchaseOrderDomainEntity.InitiatorName.Split(' ')[0] : string.Empty,
                     LastName = !string.IsNullOrEmpty(blanketPurchaseOrderDomainEntity.InitiatorName) ? blanketPurchaseOrderDomainEntity.InitiatorName.Split(' ')[1] : string.Empty
                };
                dataReader.Setup(d => d.ReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(person);
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
                        if (blanketPurchaseOrderDomainEntity.GlDistributions != null)
                        {
                            foreach (var lineItem in blanketPurchaseOrderDomainEntity.GlDistributions)
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
                bpoDataContract.Recordkey = blanketPurchaseOrderDomainEntity.Id;
                bpoDataContract.RecordGuid = blanketPurchaseOrderDomainEntity.Guid;
                bpoDataContract.BpoVendor = blanketPurchaseOrderDomainEntity.VendorId;

                if (blanketPurchaseOrderDomainEntity.VendorName == "null")
                {
                    bpoDataContract.BpoVenName = new List<string>() { null };
                }
                else
                {
                    bpoDataContract.BpoVenName = new List<string>() { blanketPurchaseOrderDomainEntity.VendorName };
                }

                // vendor name and initiator name come from CTX

                bpoDataContract.BpoInitiator = "0001687";

                bpoDataContract.BpoNo = blanketPurchaseOrderDomainEntity.Number;
                bpoDataContract.BpoTotalAmt = blanketPurchaseOrderDomainEntity.Amount;
                bpoDataContract.BpoApType = blanketPurchaseOrderDomainEntity.ApType;
                bpoDataContract.BpoDate = blanketPurchaseOrderDomainEntity.Date;
                bpoDataContract.BpoDesc = blanketPurchaseOrderDomainEntity.Description;
                bpoDataContract.BpoMaintGlTranDate = blanketPurchaseOrderDomainEntity.MaintenanceDate;
                bpoDataContract.BpoExpireDate = blanketPurchaseOrderDomainEntity.ExpirationDate;
                bpoDataContract.BpoComments = blanketPurchaseOrderDomainEntity.InternalComments;
                bpoDataContract.BpoPrintedComments = blanketPurchaseOrderDomainEntity.Comments;
                bpoDataContract.BpoCurrencyCode = blanketPurchaseOrderDomainEntity.CurrencyCode;
                bpoDataContract.BpoIntgAddressId = blanketPurchaseOrderDomainEntity.VendorAddressId;

                bpoDataContract.BpoStatus = new List<string>();
                switch (blanketPurchaseOrderDomainEntity.Status)
                {
                    case BlanketPurchaseOrderStatus.Closed:
                        bpoDataContract.BpoStatus.Add("C");
                        break;
                    case BlanketPurchaseOrderStatus.InProgress:
                        bpoDataContract.BpoStatus.Add("U");
                        break;
                    case BlanketPurchaseOrderStatus.NotApproved:
                        bpoDataContract.BpoStatus.Add("N");
                        break;
                    case BlanketPurchaseOrderStatus.Outstanding:
                        bpoDataContract.BpoStatus.Add("O");
                        break;
                    case BlanketPurchaseOrderStatus.Voided:
                        bpoDataContract.BpoStatus.Add("V");
                        break;
                    default:
                        throw new Exception("Invalid status specified in BlanketPurchaseOrderRepositoryTests");
                }

                // Build the blanket purchase order status date
                bpoDataContract.BpoStatusDate = new List<DateTime?>();
                bpoDataContract.BpoStatusDate.Add(blanketPurchaseOrderDomainEntity.StatusDate);

                // Build a list of requisitions related to the blanket purchase order
                bpoDataContract.BpoReqIds = new List<string>();
                foreach (var req in blanketPurchaseOrderDomainEntity.Requisitions)
                {
                    if (!String.IsNullOrEmpty(req))
                    {
                        bpoDataContract.BpoReqIds.Add(req);
                    }
                }

                // Build a list of vouchers related to the blanket purchase order
                bpoDataContract.BpoVouIds = new List<string>();
                foreach (var vou in blanketPurchaseOrderDomainEntity.Vouchers)
                {
                    if (!String.IsNullOrEmpty(vou))
                    {
                        bpoDataContract.BpoVouIds.Add(vou);
                    }
                }

                bpoDataContract.BpoGlEntityAssociation = new List<BpoBpoGl>();
                bpoDataContract.BpoGlNo = new List<string>();
                projectDataContracts = new Collection<Projects>();
                projectLineItemDataContracts = new Collection<ProjectsLineItems>();

                foreach (var glDist in blanketPurchaseOrderDomainEntity.GlDistributions)
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
                    bpoDataContract.BpoGlEntityAssociation.Add(dataContract);
                    bpoDataContract.BpoGlNo.Add(glDist.GlAccountNumber);

                    projectDataContracts.Add(new Projects()
                    {
                        Recordkey = glDist.ProjectId,
                        PrjRefNo = glDist.ProjectNumber
                    });

                    projectLineItemDataContracts.Add(new ProjectsLineItems()
                    {
                        Recordkey = glDist.ProjectLineItemId,
                        PrjlnProjectItemCode = glDist.ProjectLineItemCode
                    });
                }

                getGlAccountDescriptionResponse = new GetGlAccountDescriptionResponse();
                var GlAccountIds = new List<string>();
                var GlAccountDescriptions = new List<string>();
                foreach (var glDist in blanketPurchaseOrderDomainEntity.GlDistributions)
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
                bpoDataContract.BpoAuthEntityAssociation = new List<BpoBpoAuth>();
                bpoDataContract.BpoApprEntityAssociation = new List<BpoBpoAppr>();
                opersDataContracts = new Collection<Opers>();
                bpoDataContract.BpoAuthorizations = new List<string>();
                bpoDataContract.BpoNextApprovalIds = new List<string>();
                foreach (var approver in blanketPurchaseOrderDomainEntity.Approvers)
                {
                    if (approver.ApprovalDate != null)
                    {
                        // Populate approvers
                        var dataContract = new BpoBpoAuth()
                        {
                            BpoAuthorizationsAssocMember = approver.ApproverId,
                            BpoAuthorizationDatesAssocMember = approver.ApprovalDate
                        };

                        bpoDataContract.BpoAuthEntityAssociation.Add(dataContract);
                        bpoDataContract.BpoAuthorizations.Add(approver.ApproverId);
                    }
                    else
                    {
                        // Populate next approvers
                        var nextApproverDataContract = new BpoBpoAppr()
                        {
                            BpoNextApprovalIdsAssocMember = approver.ApproverId
                        };
                        bpoDataContract.BpoApprEntityAssociation.Add(nextApproverDataContract);
                        bpoDataContract.BpoNextApprovalIds.Add(approver.ApproverId);
                    }

                    // Populate the Opers data contract
                    opersDataContracts.Add(new Opers()
                    {
                        Recordkey = approver.ApproverId,
                        SysUserName = approver.ApprovalName
                    });
                }
            }

            #endregion
        }

        [TestClass]
        public class BlanketPurchaseOrderRepositoryTests_GETALL
        {
            #region Initialize and Cleanup

            private Mock<IColleagueDataReader> dataReader = null;
            private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
            private BlanketPurchaseOrderRepository blanketPurchaseOrderRepository;
            private TestBlanketPurchaseOrderRepository testBlanketPurchaseOrderRepository;
            private Tuple<IEnumerable<BlanketPurchaseOrder>, int> blanketPurchaseOrderDomainTuple;
            private List<BlanketPurchaseOrder> blanketPurchaseOrderDomainEntities;

            // Data contract objects
            private Bpo bpoDataContract;
            private Collection<Bpo> bpoDataContracts;
            private Collection<Opers> opersDataContracts;
            private Collection<Opers> opersResponse;
            private Collection<Projects> projectDataContracts;
            private Collection<ProjectsLineItems> projectLineItemDataContracts;
            private GetGlAccountDescriptionResponse getGlAccountDescriptionResponse;

            [TestInitialize]
            public void Initialize()
            {
                // Set up a mock data reader
                dataReader = new Mock<IColleagueDataReader>();

                // Set up a mock transaction invoker
                transactionInvoker = new Mock<IColleagueTransactionInvoker>();

                // Initialize the data contract object.
                bpoDataContract = new Bpo();
                bpoDataContracts = new Collection<Bpo>();

                // Initialize the blanket purchase order repository
                testBlanketPurchaseOrderRepository = new TestBlanketPurchaseOrderRepository();
                blanketPurchaseOrderRepository = BuildBlanketPurchaseOrderRepository();
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
                blanketPurchaseOrderDomainTuple = null;
                getGlAccountDescriptionResponse = null;
                opersResponse = null;
            }
            #endregion

            #region Base purchase order test
            [TestMethod]
            public async Task GetPurchaseOrder_Base()
            {
                int offset = 0;
                int limit = 100;
                string number = "";

                blanketPurchaseOrderDomainTuple = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                blanketPurchaseOrderDomainEntities = blanketPurchaseOrderDomainTuple.Item1.ToList();
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                var blanketPurchaseOrderTuple = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);

                var index = 0;
                foreach (var blanketPurchaseOrder in blanketPurchaseOrderTuple.Item1)
                {
                    // Confirm that the SV properties for the blanket purchase order are the same
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].Id, blanketPurchaseOrder.Id);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].Number, blanketPurchaseOrder.Number);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].VendorId, blanketPurchaseOrder.VendorId);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].InitiatorName, blanketPurchaseOrder.InitiatorName);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].Status, blanketPurchaseOrder.Status);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].StatusDate, blanketPurchaseOrder.StatusDate);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].Amount, blanketPurchaseOrder.Amount);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].ApType, blanketPurchaseOrder.ApType);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].Date, blanketPurchaseOrder.Date);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].Description, blanketPurchaseOrder.Description);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].MaintenanceDate, blanketPurchaseOrder.MaintenanceDate);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].ExpirationDate, blanketPurchaseOrder.ExpirationDate);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].Comments, blanketPurchaseOrder.Comments);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].InternalComments, blanketPurchaseOrder.InternalComments);
                    Assert.AreEqual(blanketPurchaseOrderDomainEntities[index].CurrencyCode, blanketPurchaseOrder.CurrencyCode);
                    index++;
                }
            }

            [TestMethod]
            public async Task GetPurchaseOrder_NumberFilter()
            {
                int offset = 0;
                int limit = 100;
                string number = "B0004441";

                blanketPurchaseOrderDomainTuple = await testBlanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                blanketPurchaseOrderDomainEntities = blanketPurchaseOrderDomainTuple.Item1.ToList();
                var total = blanketPurchaseOrderDomainTuple.Item2;
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();
                
                var blanketPurchaseOrderTuple = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                var blanketPurchaseOrder = blanketPurchaseOrderTuple.Item1.First();
                Assert.AreEqual(number, blanketPurchaseOrder.Number);
                Assert.AreEqual(1, blanketPurchaseOrderTuple.Item2);
            }
            #endregion

            #region Invalid data tests

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_NullStatus()
            {
                int offset = 0;
                int limit = 100;
                string number = "";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Collection<Bpo>()
                {
                    new Bpo()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        BpoStatus = null,
                        BpoNo = "B0000384"
                    }
                };

                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrderObject);

                string expectedMessage = "";
                string actualMessage = "";
                try
                {
                    expectedMessage = "Missing status for blanket purchase order 'B0000384'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_StatusListHasNullValue()
            {
                int offset = 0;
                int limit = 100;
                string number = "";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Collection<Bpo>()
                {
                    new Bpo()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        BpoStatus = new List<string>() { null },
                        BpoNo = "B0000384"
                    }
                };

                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrderObject);

                string expectedMessage = "";
                string actualMessage = "";
                try
                {
                    expectedMessage = "Missing status for blanket purchase order 'B0000384'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_StatusListHasEmptyValue()
            {
                int offset = 0;
                int limit = 100;
                string number = "";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Collection<Bpo>()
                {
                    new Bpo()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        BpoStatus = new List<string>() { "" },
                        BpoNo = "B0000384"
                    }
                };

                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrderObject);

                string expectedMessage = "";
                string actualMessage = "";
                try
                {
                    expectedMessage = "Missing status for blanket purchase order 'B0000384'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_InvalidStatus()
            {
                int offset = 0;
                int limit = 100;
                string number = "";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Collection<Bpo>()
                {
                    new Bpo()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        BpoStatus = new List<string>() { "Z" },
                        BpoNo = "B0000384"
                    }
                };

                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrderObject);

                string expectedMessage = "";
                string actualMessage = "";
                try
                {
                    expectedMessage = "Blanket Purchase Orders in an unfinished state are not supported. 'B0000384'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetBlanketPurchaseOrder_NullStatusDate()
            {
                int offset = 0;
                int limit = 100;
                string number = "";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Collection<Bpo>()
                {
                    new Bpo()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        BpoStatus = new List<string>() { "O" },
                        BpoStatusDate = null,
                        BpoNo = "B0000384"
                    }
                };

                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrderObject);

                string expectedMessage = "";
                string actualMessage = "";
                try
                {
                    expectedMessage = "Missing status date for blanket purchase order 'B0000384'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }
            
            [TestMethod]
            public async Task GetBlanketPurchaseOrder_StatusDateListHasNullValue()
            {
                int offset = 0;
                int limit = 100;
                string number = "";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Collection<Bpo>()
                {
                    new Bpo()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        BpoStatus = new List<string>() { "O" },
                        BpoStatusDate = new List<DateTime?>() { null },
                        BpoNo = "B0000384"
                    }
                };

                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrderObject);

                string expectedMessage = "";
                string actualMessage = "";
                try
                {
                    expectedMessage = "Missing status date for blanket purchase order 'B0000384'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_BpoDateIsNull()
            {
                int offset = 0;
                int limit = 100;
                string number = "";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Collection<Bpo>()
                {
                    new Bpo()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        BpoStatus = new List<string>() { "O" },
                        BpoStatusDate = new List<DateTime?>() { DateTime.Now },
                        BpoNo = "B0000384",
                        BpoDate = null
                    }
                };

                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrderObject);

                string expectedMessage = "";
                string actualMessage = "";
                try
                {
                    expectedMessage = "Missing date for blanket purchase order 'B0000384'";
                    var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                }
                catch (RepositoryException aex)
                {
                    actualMessage = aex.Errors[0].Message;
                }

                Assert.AreEqual(expectedMessage, actualMessage);
            }

            [TestMethod]
            public async Task GetPurchaseOrder_TotalAmountIsNull()
            {
                int offset = 0;
                int limit = 100;
                string number = "";
                string guid = "b18288c0-cca7-45b1-a310-39e376db0c3d";

                // Mock ReadRecord to return a pre-defined, null blanket purchase order data contract
                var blanketPurchaseOrderObject = new Collection<Bpo>()
                {
                    new Bpo()
                    {
                        Recordkey = "1",
                        RecordGuid = guid,
                        BpoStatus = new List<string>() { "O" },
                        BpoStatusDate = new List<DateTime?>() { DateTime.Now },
                        BpoNo = "B0000384",
                        BpoDate = DateTime.Now,
                        BpoTotalAmt = null
                    }
                };

                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(blanketPurchaseOrderObject);

                var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, number);
                
                Assert.AreEqual(0, blanketPurchaseOrder.Item1.FirstOrDefault().Amount);
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
                // Mock Select of BPO keys
                string[] bpoIds = bpoDataContracts.Select(bpo => bpo.Recordkey).ToArray();
                dataReader.Setup(dr => dr.SelectAsync("BPO", It.IsAny<string>())).ReturnsAsync(bpoIds);
                // Mock ReadRecord to return a pre-defined Blanket Purchase Orders data contract
                dataReader.Setup(acc => acc.BulkReadRecordAsync<Bpo>("BPO", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(bpoDataContracts);
    
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
                dataReader.Setup(acc => acc.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(opersResponse);

                // Mock BulkReadRecord to return a list of Projects data contracts
                dataReader.Setup(acc => acc.BulkReadRecordAsync<Projects>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(projectDataContracts);

                // Mock BulkReadRecord to return a list of ProjectsLineItems data contracts
                dataReader.Setup(acc => acc.BulkReadRecordAsync<ProjectsLineItems>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(projectLineItemDataContracts);

                // Mock Guid lookup
                GuidLookupResult result = new GuidLookupResult() { Entity = "BPO", PrimaryKey = "1", SecondaryKey = "" };
                Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
                resultDict.Add("1", result);
                dataReader.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

                // Mock international parameters for host country setup.
                var intlParams = new Base.DataContracts.IntlParams()
                {
                    HostCountry = "USA",
                    HostDateDelimiter = "/",
                    HostShortDateFormat = "MDY"
                };
                dataReader.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(intlParams);

                // Mock Purchase Defaults
                dataReader.Setup(d => d.ReadRecordAsync<PurDefaults>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new PurDefaults() { PurShipToCode = "1" });

                // Mock Initiator Names
                var personContracts = new Collection<Base.DataContracts.Person>();
                foreach (var bpo in blanketPurchaseOrderDomainTuple.Item1)
                {
                    if (!string.IsNullOrEmpty(bpo.DefaultInitiator))
                    {
                        var person = new Base.DataContracts.Person()
                        {
                            FirstName = !string.IsNullOrEmpty(bpo.InitiatorName) ? bpo.InitiatorName.Split(' ')[0] : string.Empty,
                            LastName = !string.IsNullOrEmpty(bpo.InitiatorName) ? bpo.InitiatorName.Split(' ')[1] : string.Empty,
                            Recordkey = bpo.DefaultInitiator,
                            RecordGuid = Guid.NewGuid().ToString()
                        };
                        personContracts.Add(person);
                    }
                }
                dataReader.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(personContracts);
            }

            private List<string> CalculateExpenseAccountsForUser(string blanketPurchaseOrderId)
            {
                var bpo = blanketPurchaseOrderDomainTuple.Item1.FirstOrDefault(bp => bp.Id == blanketPurchaseOrderId);
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
                        if (bpo.GlDistributions != null)
                        {
                            foreach (var lineItem in bpo.GlDistributions)
                            {
                                if ((bpo.GlDistributions != null) && (bpo.GlDistributions.Count > 0))
                                {
                                    foreach (var glDistribution in bpo.GlDistributions)
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
                bpoDataContracts = new Collection<Bpo>();
                foreach (var bpoDomainEntity in blanketPurchaseOrderDomainEntities)
                {
                    // Convert the Blanket Purchase Order object
                    bpoDataContract = new Bpo();
                    bpoDataContract.Recordkey = bpoDomainEntity.Id;
                    bpoDataContract.RecordGuid = bpoDomainEntity.Guid;
                    bpoDataContract.BpoVendor = bpoDomainEntity.VendorId;

                    if (bpoDomainEntity.VendorName == "null")
                    {
                        bpoDataContract.BpoVenName = new List<string>() { null };
                    }
                    else
                    {
                        bpoDataContract.BpoVenName = new List<string>() { bpoDomainEntity.VendorName };
                    }

                    bpoDataContract.BpoNo = bpoDomainEntity.Number;
                    bpoDataContract.BpoTotalAmt = bpoDomainEntity.Amount;
                    bpoDataContract.BpoApType = bpoDomainEntity.ApType;
                    bpoDataContract.BpoDate = bpoDomainEntity.Date;
                    bpoDataContract.BpoDesc = bpoDomainEntity.Description;
                    bpoDataContract.BpoMaintGlTranDate = bpoDomainEntity.MaintenanceDate;
                    bpoDataContract.BpoExpireDate = bpoDomainEntity.ExpirationDate;
                    bpoDataContract.BpoComments = bpoDomainEntity.InternalComments;
                    bpoDataContract.BpoPrintedComments = bpoDomainEntity.Comments;
                    bpoDataContract.BpoCurrencyCode = bpoDomainEntity.CurrencyCode;
                    bpoDataContract.BpoIntgAddressId = bpoDomainEntity.VendorAddressId;
                    bpoDataContract.BpoInitiator = bpoDomainEntity.DefaultInitiator;

                    bpoDataContract.BpoStatus = new List<string>();
                    switch (bpoDomainEntity.Status)
                    {
                        case BlanketPurchaseOrderStatus.Closed:
                            bpoDataContract.BpoStatus.Add("C");
                            break;
                        case BlanketPurchaseOrderStatus.InProgress:
                            bpoDataContract.BpoStatus.Add("U");
                            break;
                        case BlanketPurchaseOrderStatus.NotApproved:
                            bpoDataContract.BpoStatus.Add("N");
                            break;
                        case BlanketPurchaseOrderStatus.Outstanding:
                            bpoDataContract.BpoStatus.Add("O");
                            break;
                        case BlanketPurchaseOrderStatus.Voided:
                            bpoDataContract.BpoStatus.Add("V");
                            break;
                        default:
                            throw new Exception("Invalid status specified in BlanketPurchaseOrderRepositoryTests");
                    }

                    // Build the blanket purchase order status date
                    bpoDataContract.BpoStatusDate = new List<DateTime?>();
                    bpoDataContract.BpoStatusDate.Add(bpoDomainEntity.StatusDate);

                    // Build a list of requisitions related to the blanket purchase order
                    bpoDataContract.BpoReqIds = new List<string>();
                    foreach (var req in bpoDomainEntity.Requisitions)
                    {
                        if (!String.IsNullOrEmpty(req))
                        {
                            bpoDataContract.BpoReqIds.Add(req);
                        }
                    }

                    // Build a list of vouchers related to the blanket purchase order
                    bpoDataContract.BpoVouIds = new List<string>();
                    foreach (var vou in bpoDomainEntity.Vouchers)
                    {
                        if (!String.IsNullOrEmpty(vou))
                        {
                            bpoDataContract.BpoVouIds.Add(vou);
                        }
                    }

                    bpoDataContract.BpoGlEntityAssociation = new List<BpoBpoGl>();
                    bpoDataContract.BpoGlNo = new List<string>();
                    projectDataContracts = new Collection<Projects>();
                    projectLineItemDataContracts = new Collection<ProjectsLineItems>();

                    foreach (var glDist in bpoDomainEntity.GlDistributions)
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
                        bpoDataContract.BpoGlEntityAssociation.Add(dataContract);
                        bpoDataContract.BpoGlNo.Add(glDist.GlAccountNumber);

                        projectDataContracts.Add(new Projects()
                        {
                            Recordkey = glDist.ProjectId,
                            PrjRefNo = glDist.ProjectNumber
                        });

                        projectLineItemDataContracts.Add(new ProjectsLineItems()
                        {
                            Recordkey = glDist.ProjectLineItemId,
                            PrjlnProjectItemCode = glDist.ProjectLineItemCode
                        });
                    }

                    getGlAccountDescriptionResponse = new GetGlAccountDescriptionResponse();
                    var GlAccountIds = new List<string>();
                    var GlAccountDescriptions = new List<string>();
                    foreach (var glDist in bpoDomainEntity.GlDistributions)
                    {
                        GlAccountIds.Add(glDist.GlAccountNumber);
                        GlAccountDescriptions.Add(glDist.GlAccountDescription);
                    }
                    getGlAccountDescriptionResponse.GlAccountIds = GlAccountIds;
                    getGlAccountDescriptionResponse.GlDescriptions = GlAccountDescriptions;

                    // Build a list of Approver data contracts
                    ConvertApproversIntoDataContracts(bpoDomainEntity);
                    bpoDataContracts.Add(bpoDataContract);
                }
            }

            private void ConvertApproversIntoDataContracts(BlanketPurchaseOrder bpoDomainEntity)
            {
                // Initialize the associations for approvers and next approvers.
                bpoDataContract.BpoAuthEntityAssociation = new List<BpoBpoAuth>();
                bpoDataContract.BpoApprEntityAssociation = new List<BpoBpoAppr>();
                opersDataContracts = new Collection<Opers>();
                bpoDataContract.BpoAuthorizations = new List<string>();
                bpoDataContract.BpoNextApprovalIds = new List<string>();
                foreach (var approver in bpoDomainEntity.Approvers)
                {
                    if (approver.ApprovalDate != null)
                    {
                        // Populate approvers
                        var dataContract = new BpoBpoAuth()
                        {
                            BpoAuthorizationsAssocMember = approver.ApproverId,
                            BpoAuthorizationDatesAssocMember = approver.ApprovalDate
                        };

                        bpoDataContract.BpoAuthEntityAssociation.Add(dataContract);
                        bpoDataContract.BpoAuthorizations.Add(approver.ApproverId);
                    }
                    else
                    {
                        // Populate next approvers
                        var nextApproverDataContract = new BpoBpoAppr()
                        {
                            BpoNextApprovalIdsAssocMember = approver.ApproverId
                        };
                        bpoDataContract.BpoApprEntityAssociation.Add(nextApproverDataContract);
                        bpoDataContract.BpoNextApprovalIds.Add(approver.ApproverId);
                    }

                    // Populate the Opers data contract
                    opersDataContracts.Add(new Opers()
                    {
                        Recordkey = approver.ApproverId,
                        SysUserName = approver.ApprovalName
                    });
                }
            }

            #endregion
        }

        [TestClass]
        public class BlanketPurchaseOrderRepositoryTests_POST : BaseRepositorySetup
        {
            #region DECLARATIONS

            private BlanketPurchaseOrderRepository blanketPurchaseOrderRepository;
            private Collection<Projects> projects;
            private BlanketPurchaseOrder blanketPurchaseOrder;
            private DataContracts.Bpo blanketPurchaseOrders;
            private List<LineItem> lineItems;
            private CreateUpdateBpoResponse response;
            private Dictionary<string, GuidLookupResult> dicResult;
            private Base.DataContracts.Person person;
            private Base.DataContracts.IntlParams intlParams;
            private PurDefaults purDefaults;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                blanketPurchaseOrderRepository = new BlanketPurchaseOrderRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                purDefaults = new PurDefaults()
                {
                    PurShipToCode = "1"
                };

                intlParams = new Base.DataContracts.IntlParams()
                {
                    HostCountry = "USA",
                    HostDateDelimiter = "/",
                    HostShortDateFormat = "MDY"
                };

                person = new Base.DataContracts.Person()
                {
                    FirstName = "first",
                    LastName = "last"
                };

                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "PURCHASE.ORDERS", PrimaryKey = "1" } }
                };

                response = new CreateUpdateBpoResponse() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" };

                blanketPurchaseOrder = new BlanketPurchaseOrder("1", "number", "VendorName", BlanketPurchaseOrderStatus.InProgress, DateTime.Today, DateTime.Today)
                {
                    CurrencyCode = "USD",
                    SubmittedBy = "1",
                    MaintenanceDate = DateTime.Today,
                    VoidGlTranDate = DateTime.Today,
                    ReferenceNo = new List<string>() { "1" },
                    Buyer = "1",
                    InitiatorName = "Name",
                    DefaultInitiator = "1",
                    ShipToCode = "1",
                    Fob = "1",
                    AltShippingName = "A",
                    AltShippingAddress = new List<string>() { "A" },
                    MiscCountry = "USA",
                    AltShippingCity = "C",
                    AltShippingState = "FL",
                    AltShippingZip = "Z",
                    AltShippingCountry = "USA",
                    AltShippingPhoneExt = "1234",
                    AltShippingPhone = "1234",
                    MiscCity = "city",
                    MiscState = "state",
                    MiscZip = "12345",
                    VendorId = "1",
                    VendorAddressId = "001",
                    MiscName = new List<string>() { "Name" },
                    MiscAddress = new List<string>() { "Line1" },
                    VendorTerms = "1",
                    ApType = "1",
                    Comments = "comments",
                    InternalComments = "Internalcomments",
                };

                blanketPurchaseOrders = new Bpo()
                {
                    Recordkey = "1",
                    RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    BpoStatus = new List<string>() { "N" },
                    BpoStatusDate = new List<DateTime?>() { DateTime.Today },
                    BpoDate = DateTime.Today,
                    BpoNo = "1",
                    BpoIntgSubmittedBy = "1",
                    BpoVendor = "1",
                    BpoInitiator = "1",
                    BpoCurrencyCode = "USD",
                    BpoMaintGlTranDate = DateTime.Today,
                    BpoReqIds = new List<string>() { "1" },
                    BpoVouIds = new List<string>() { "1" },
                };

                projects = new Collection<Projects>() { new Projects() { Recordkey = "1", RecordGuid = "3a46eef8-5fe7-4120-b1cf-f23266b9e001" }, new Projects() { Recordkey = "2", RecordGuid = "3a46eef8-5fe7-4120-b1cf-f23266b9e002" } };
            }

            private void InitializeTestMock()
            {

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateBpoRequest, CreateUpdateBpoResponse>(It.IsAny<CreateUpdateBpoRequest>())).ReturnsAsync(response);

                dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).ReturnsAsync(blanketPurchaseOrders);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(person);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", It.IsAny<bool>())).ReturnsAsync(intlParams);
                dataReaderMock.Setup(d => d.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS", true)).ReturnsAsync(purDefaults);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_ArgumentNullException_Null()
            {
                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_RepositoryException()
            {
                response.UpdateBPOErrors = new List<UpdateBPOErrors>() { new UpdateBPOErrors() { ErrorMessages = "Error", ErrorCodes = "BlanketPurchaseOrders" } };

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_ArgumentNullException_Guid_Null()
            {
                response.Guid = null;

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_KeyNotFoundException_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).ReturnsAsync(null);

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_ArgumentNullException_RecordKey_Null()
            {
                blanketPurchaseOrders.Recordkey = null;

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_ArgumentNullException_RecordGuid_Null()
            {
                blanketPurchaseOrders.RecordGuid = null;

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_RepositoryException_PoStatus_Null()
            {
                blanketPurchaseOrders.BpoStatus = new List<string>();

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_RepositoryException_PoStatusDate_Null()
            {
                blanketPurchaseOrders.BpoStatusDate = null;

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_RepositoryException_PoDate_Null()
            {
                blanketPurchaseOrders.BpoDate = null;

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_RepositoryException_LineItems()
            {
                blanketPurchaseOrders.BpoItemsId = new List<string>() { "1" };

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);

            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_RepositoryException_PoStatus_Invalid()
            {
                blanketPurchaseOrders.BpoStatus = new List<string>() { "X" };

                await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync_GLDistributions()
            {
                blanketPurchaseOrder.AddGlDistribution(new BlanketPurchaseOrderGlDistribution("1", 25));

                var retVal = await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
                Assert.IsNotNull(retVal);
            }

            [TestMethod]
            public async Task BPORepository_CreateBlanketPurchaseOrdersAsync()
            {

                var retVal = await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
                Assert.IsNotNull(retVal);
            }

            [TestMethod]
            public async Task BPORepository_GetProjectReferenceIds()
            {

                string[] projs = new string[] { "1", "2" };
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(projects);
                var result = await blanketPurchaseOrderRepository.GetProjectReferenceIds(projs);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Count, 2);
            }

            [TestMethod]
            public async Task BPORepository_GetProjectReferenceIds_null()
            {

                string[] projs = new string[] { "1", "2" };
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(null);
                var result = await blanketPurchaseOrderRepository.GetProjectReferenceIds(projs);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Count, 0);
            }

            [TestMethod]
            public async Task BPORepository_GetProjectIdsFromReferenceNo()
            {
                projects = new Collection<Projects>() { new Projects() { Recordkey = "1", RecordGuid = "3a46eef8-5fe7-4120-b1cf-f23266b9e001", PrjRefNo = "001"},
                    new Projects() { Recordkey = "2", RecordGuid = "3a46eef8-5fe7-4120-b1cf-f23266b9e002" , PrjRefNo = "002"} };

                var projs = projects.Select(x => x.PrjRefNo).ToArray();
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(projects);

                var result = await blanketPurchaseOrderRepository.GetProjectIdsFromReferenceNo(projs);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Count, projects.Count);

                foreach (var i in projects.Select(x => x.Recordkey))
                {
                    string projRefNo = string.Empty;
                    result.TryGetValue(i, out projRefNo);
                    Assert.AreEqual(projects.FirstOrDefault(x => x.Recordkey == i).PrjRefNo, projRefNo);
                }
            }

            [TestMethod]
            public async Task BPORepository_GetProjectIdsFromReferenceNo_null()
            {
                projects = new Collection<Projects>() { new Projects() { Recordkey = "1", RecordGuid = "3a46eef8-5fe7-4120-b1cf-f23266b9e001", PrjRefNo = "001"},
                    new Projects() { Recordkey = "2", RecordGuid = "3a46eef8-5fe7-4120-b1cf-f23266b9e002" , PrjRefNo = "002"} };

                var projs = projects.Select(x => x.PrjRefNo).ToArray();
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);

                var result = await blanketPurchaseOrderRepository.GetProjectIdsFromReferenceNo(projs);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Count, 0);

            }
        }

        [TestClass]
        public class BlanketPurchaseOrderRepositoryTests_PUT : BaseRepositorySetup
        {
            #region DECLARATIONS

            private BlanketPurchaseOrderRepository blanketPurchaseOrderRepository;

            private BlanketPurchaseOrder blanketPurchaseOrder;
            private DataContracts.Bpo bpoData;
            private CreateUpdateBpoResponse response;
            private Dictionary<string, GuidLookupResult> dicResult;
            private Base.DataContracts.Person person;
            private Base.DataContracts.IntlParams intlParams;
            private PurDefaults purDefaults;
            private LineItem lineItem;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                blanketPurchaseOrderRepository = new BlanketPurchaseOrderRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                lineItem = new LineItem("1", "desc", 10, 100, 110) { Status = PurchaseOrderStatus.InProgress };

                lineItem.AddTax(new LineItemTax("1", 100));
                lineItem.AddGlDistribution(new LineItemGlDistribution("a", 10, 100) { });

                purDefaults = new PurDefaults()
                {
                    PurShipToCode = "1"
                };

                intlParams = new Base.DataContracts.IntlParams()
                {
                    HostCountry = "USA",
                    HostDateDelimiter = "/",
                    HostShortDateFormat = "MDY"
                };

                person = new Base.DataContracts.Person()
                {
                    FirstName = "first",
                    LastName = "last"
                };

                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "PURCHASE.ORDERS", PrimaryKey = "1" } }
                };

                response = new CreateUpdateBpoResponse() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" };

                blanketPurchaseOrder = new BlanketPurchaseOrder("1", "1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "vendor", BlanketPurchaseOrderStatus.InProgress, DateTime.Today, DateTime.Today)
                {
   
                    CurrencyCode = "USD",
                    SubmittedBy = "1",
                    MaintenanceDate = DateTime.Today,
                    VoidGlTranDate = DateTime.Today,
                    ReferenceNo = new List<string>() { "1" },
                    Buyer = "1",
                    InitiatorName = "Name",
                    DefaultInitiator = "1",
                    ShipToCode = "1",
                    Fob = "1",
                    AltShippingName = "A",
                    AltShippingAddress = new List<string>() { "A" },
                    MiscCountry = "USA",
                    AltShippingCity = "C",
                    AltShippingState = "FL",
                    AltShippingZip = "Z",
                    AltShippingCountry = "USA",
                    AltShippingPhoneExt = "1234",
                    AltShippingPhone = "1234",
                    MiscCity = "city",
                    MiscState = "state",
                    MiscZip = "12345",
                    VendorId = "1",
                    VendorAddressId = "001",
                    MiscName = new List<string>() { "Name" },
                    MiscAddress = new List<string>() { "Line1" },
                    VendorTerms = "1",
                    ApType = "1",
                    Comments = "comments",
                    InternalComments = "Internalcomments",
                };

                blanketPurchaseOrder.AddLineItem(lineItem);

                bpoData = new Bpo()
                {
                    Recordkey = "1",
                    RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    BpoStatus = new List<string>() { "N" },
                    BpoStatusDate = new List<DateTime?>() { DateTime.Today },
                    BpoDate = DateTime.Today,
                    BpoNo = "1",
                    BpoIntgSubmittedBy = "1",
                    BpoVendor = "1",
                    BpoInitiator = "1",
                    BpoCurrencyCode = "USD",
                    BpoMaintGlTranDate = DateTime.Today,
                    BpoReqIds = new List<string>() { "1" },
                    BpoVouIds = new List<string>() { "1" },
                };
            }

            private void InitializeTestMock()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateBpoRequest, CreateUpdateBpoResponse>(It.IsAny<CreateUpdateBpoRequest>())).ReturnsAsync(response);
                
                dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).ReturnsAsync(bpoData);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(person);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", It.IsAny<bool>())).ReturnsAsync(intlParams);
                dataReaderMock.Setup(d => d.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS", true)).ReturnsAsync(purDefaults);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync_ArgumentNullException_Null()
            {
                await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync_RepositoryException()
            {
                response.UpdateBPOErrors = new List<UpdateBPOErrors>() { new UpdateBPOErrors() { ErrorMessages = "Error", ErrorCodes = "BlanketPurchaseOrders" } };

                await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync_KeyNotFoundException_BlanketPurchaseOrder_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Bpo>(It.IsAny<string>(), true)).ReturnsAsync(null);

                await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync_ArgumentNullException_RecordKey_Null()
            {
                bpoData.Recordkey = null;

                await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync_ArgumentNullException_RecordGuid_Null()
            {
                bpoData.RecordGuid = null;

                await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync_RepositoryException_PoStatus_Null()
            {
                bpoData.BpoStatus = new List<string>();

                await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync_RepositoryException_PoStatusDate_Null()
            {
                bpoData.BpoStatusDate = null;

                await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync_RepositoryException_PoDate_Null()
            {
                bpoData.BpoDate = null;

                await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
            }

            [TestMethod]
            public async Task BPORepository_UpdateBlanketPurchaseOrdersAsync()
            {
                var result = await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrder);

                Assert.IsNotNull(result);
            }

        }
    }
}