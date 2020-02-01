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
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class RequisitionRepositoryTests
    {
        #region Initialize and Cleanup

        private Mock<IColleagueDataReader> dataReader = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private RequisitionRepository requisitionRepository;
        private TestRequisitionRepository testRequisitionRepository;
        private Requisition requisitionDomainEntity;

        // Data contract objects
        private Requisitions requisitionDataContract;
        private Collection<DataContracts.Opers> opersDataContracts;
        private ShipToCodes shipToCodesDataContract;
        private Collection<Items> itemsDataContracts;
        private Collection<DataContracts.Projects> projectDataContracts;
        private Collection<DataContracts.ProjectsLineItems> projectLineItemDataContracts;
        private GetHierarchyNamesForIdsResponse hierarchyNamesForIdsResponse;
        private TxCheckUserGlAccessResponse checkUserGlAccessResponse;
        private Collection<DataContracts.Opers> opersResponse;

        private string personId = "1";
        private string requisitionIdForTransaction;

        [TestInitialize]
        public void Initialize()
        {            
            // Set up a mock data reader
            dataReader = new Mock<IColleagueDataReader>();

            // Set up a mock transaction invoker
            transactionInvoker = new Mock<IColleagueTransactionInvoker>();

            // Initialize the data contract object
            requisitionDataContract = new Requisitions();

            // Initialize the requisition repository
            testRequisitionRepository = new TestRequisitionRepository();

            requisitionIdForTransaction = "";

            this.requisitionRepository = BuildRequisitionRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            dataReader = null;
            transactionInvoker = null;
            requisitionDataContract = null;
            shipToCodesDataContract = null;
            opersDataContracts = null;
            itemsDataContracts = null;
            projectDataContracts = null;
            projectLineItemDataContracts = null;
            testRequisitionRepository = null;
            requisitionDomainEntity = null;
            hierarchyNamesForIdsResponse = null;
            checkUserGlAccessResponse = null;
            opersResponse = null;
            requisitionIdForTransaction = null;
        }

        #endregion

        #region Base Requisition Test
        [TestMethod]
        public async Task GetRequisition_Base()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // Confirm that the SV properties for the requisition are the same
            Assert.AreEqual(this.requisitionDomainEntity.Id, requisition.Id);
            Assert.AreEqual(this.requisitionDomainEntity.VendorId, requisition.VendorId);
            Assert.AreEqual(this.requisitionDomainEntity.VendorName, requisition.VendorName);
            Assert.AreEqual(this.requisitionDomainEntity.InitiatorName, requisition.InitiatorName);
            Assert.AreEqual(this.requisitionDomainEntity.RequestorName, requisition.RequestorName);
            Assert.AreEqual(this.requisitionDomainEntity.Status, requisition.Status);
            Assert.AreEqual(this.requisitionDomainEntity.StatusDate, requisition.StatusDate);
            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount);
            Assert.AreEqual(this.requisitionDomainEntity.ApType, requisition.ApType);
            Assert.AreEqual(this.requisitionDomainEntity.Date, requisition.Date);
            Assert.AreEqual(this.requisitionDomainEntity.MaintenanceDate, requisition.MaintenanceDate);
            Assert.AreEqual(this.requisitionDomainEntity.DesiredDate, requisition.DesiredDate);
            Assert.AreEqual(this.requisitionDomainEntity.ShipToCode, requisition.ShipToCode);
            Assert.AreEqual(this.requisitionDomainEntity.Comments, requisition.Comments);
            Assert.AreEqual(this.requisitionDomainEntity.InternalComments, requisition.InternalComments);
            Assert.AreEqual(this.requisitionDomainEntity.CurrencyCode, requisition.CurrencyCode);
            Assert.AreEqual(this.requisitionDomainEntity.BlanketPurchaseOrder, requisition.BlanketPurchaseOrder);
        }
        #endregion

        #region Invalid data tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRequisition_NullId()
        {
            var requisition = await this.requisitionRepository.GetRequisitionAsync(null, personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetRequisition_NullRequisition()
        {
            // Mock ReadRecord to return a pre-defined, null requisition data contract
            var nullRequisitionObject = new Requisitions();
            nullRequisitionObject = null;
            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(nullRequisitionObject));
            var requisition = await this.requisitionRepository.GetRequisitionAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisition_NullStatus()
        {
            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = null
            };

            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisitionDataContract));
            var requisition = await requisitionRepository.GetRequisitionAsync("10", "1", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisition_StatusListHasBlankValue()
        {
            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "" }
            };

            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisitionDataContract));
            var requisition = await requisitionRepository.GetRequisitionAsync("10", "1", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisition_StatusDateHasNullValue()
        {
            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "P" },
                ReqStatusDate = new List<DateTime?>() { null }
            };

            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisitionDataContract));
            var requisition = await requisitionRepository.GetRequisitionAsync("10", "1", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisition_InvalidRequisitionStatus()
        {
            // Mock ReadRecord to return a pre-defined, null requisition data contract
            var requisitionObject = new Requisitions()
            {
                Recordkey = "1",
                ReqStatus = new List<string>() { "Z" }
            };
            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisitionObject));
            var requisition = await this.requisitionRepository.GetRequisitionAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisition_NullStatusDate()
        {
            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "P" },
                ReqStatusDate = null
            };

            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisitionDataContract));
            var requisition = await requisitionRepository.GetRequisitionAsync("10", "1", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisition_StatusDateListHasNullValue()
        {
            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "P" },
                ReqStatusDate = new List<DateTime?>() { null }
            };

            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisitionDataContract));
            var requisition = await requisitionRepository.GetRequisitionAsync("10", "1", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisition_NullReqDate()
        {
            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "P" },
                ReqStatusDate = new List<DateTime?>() { new DateTime(2015, 1, 1) },
                ReqDate = null
            };

            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisitionDataContract));
            var requisition = await requisitionRepository.GetRequisitionAsync("10", "1", GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisition_MultipleBpos()
        {
            string requisitionId = "4";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            this.requisitionDataContract.ReqBpoNo = new List<string>() { "B0001", "B0002" };
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_InitiatorAndRequstorAreNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqRequestor = null;
            this.requisitionDataContract.ReqDefaultInitiator = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDataContract.ReqMiscName.FirstOrDefault(), requisition.VendorName);
            Assert.IsTrue(string.IsNullOrEmpty(requisition.InitiatorName));
            Assert.IsTrue(string.IsNullOrEmpty(requisition.RequestorName));
        }

        [TestMethod]
        public async Task GetRequisitionAsync_HierarchyNamesCtxReturnsNullNamesList()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.hierarchyNamesForIdsResponse.OutPersonNames = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDataContract.ReqMiscName.FirstOrDefault(), requisition.VendorName);
            Assert.IsTrue(string.IsNullOrEmpty(requisition.InitiatorName));
            Assert.IsTrue(string.IsNullOrEmpty(requisition.RequestorName));
        }

        [TestMethod]
        public async Task GetRequisitionAsync_HierarchyNamesCtxReturnsEmptyNamesList()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.hierarchyNamesForIdsResponse.OutPersonNames = new List<string>();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDataContract.ReqMiscName.FirstOrDefault(), requisition.VendorName);
            Assert.IsTrue(string.IsNullOrEmpty(requisition.InitiatorName));
            Assert.IsTrue(string.IsNullOrEmpty(requisition.RequestorName));
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ReqPoNoIsNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqPoNo = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.PurchaseOrders.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ReqPoNoIsEmpty()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqPoNo = new List<string>();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.PurchaseOrders.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ReqBpoNoIsNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqBpoNo = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.IsTrue(string.IsNullOrEmpty(requisition.BlanketPurchaseOrder));
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ReqBpoNoIsEmpty()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqBpoNo = new List<string>();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.IsTrue(string.IsNullOrEmpty(requisition.BlanketPurchaseOrder));
        }

        [TestMethod]
        public async Task GetRequisitionAsync_OpersBulkReadReturnsNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersResponse = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.Approvers.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_OpersBulkReadReturnsEmptyList()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersResponse = new Collection<DataContracts.Opers>();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.Approvers.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ApproversAssociationIsNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqAuthEntityAssociation = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.opersResponse.Count, requisition.Approvers.Count);
            foreach (var approver in requisition.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ApproversAssociationIsEmpty()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqAuthEntityAssociation = new List<RequisitionsReqAuth>();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.opersResponse.Count, requisition.Approvers.Count);
            foreach (var approver in requisition.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ReqItemIdIsNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqItemsId = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.LineItems.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ReqItemIdIsEmpty()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqItemsId = new List<string>();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.LineItems.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ItemsBulkReadReturnsNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.itemsDataContracts = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.LineItems.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ItemsBulkReadReturnsEmpty()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.itemsDataContracts = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.LineItems.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_LineItemAssociationIsNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItemReqEntityAssociation = null;
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.LineItems.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_LineItemAssociationIsEmpty()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItemReqEntityAssociation = new List<ItemsItemReq>();
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, requisition.LineItems.Count);
        }

        [TestMethod]
        public async Task GetRequisitionAsync_LineItem_QuantityPriceAndExtendedPriceAreNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItmReqQty = null;
                lineItem.ItmReqPrice = null;
                lineItem.ItmReqExtPrice = null;
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var lineItem in requisition.LineItems)
            {
                Assert.AreEqual(0, lineItem.Quantity);
                Assert.AreEqual(0, lineItem.Price);
                Assert.AreEqual(0, lineItem.ExtendedPrice);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_GlDistribution_QuantityAndAmountAreNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.ItemReqEntityAssociation).ToList())
            {
                glDistribution.ItmReqGlQtyAssocMember = null;
                glDistribution.ItmReqGlAmtAssocMember = null;
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in requisition.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.AreEqual(0, glDistribution.Quantity);
                Assert.AreEqual(0, glDistribution.Amount);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_GlDistribution_ForeignAmountIsNull()
        {

            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqCurrencyCode = "CA";
            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.ItemReqEntityAssociation).ToList())
            {
                glDistribution.ItmReqGlForeignAmtAssocMember = null;
                glDistribution.ItmReqGlAmtAssocMember = null;
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in requisition.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.AreEqual(0, glDistribution.Amount);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_TaxAssociationIsNull()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqCurrencyCode = "CA";
            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ReqGlTaxesEntityAssociation = null;
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var lineItem in requisition.LineItems)
            {
                Assert.AreEqual(0, lineItem.LineItemTaxes.Count);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_TaxAssociationIsEmpty()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqCurrencyCode = "CA";
            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ReqGlTaxesEntityAssociation = new List<ItemsReqGlTaxes>();
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var lineItem in requisition.LineItems)
            {
                Assert.AreEqual(0, lineItem.LineItemTaxes.Count);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_LocalCurrency_AmountIsNull()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqCurrencyCode = "";
            foreach (var taxAssociation in this.itemsDataContracts.SelectMany(x => x.ReqGlTaxesEntityAssociation.ToList()))
            {
                taxAssociation.ItmReqGlTaxAmtAssocMember = null;
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            var expectedAmount = this.itemsDataContracts.SelectMany(x => x.ItemReqEntityAssociation).ToList().Sum(x => x.ItmReqGlAmtAssocMember);
            Assert.AreEqual(expectedAmount, requisition.Amount);
            foreach (var tax in requisition.LineItems.SelectMany(x => x.LineItemTaxes).ToList())
            {
                Assert.AreEqual(0, tax.TaxAmount);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ForeignCurrency_AmountIsNull()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.requisitionDataContract.ReqCurrencyCode = "CA";
            foreach (var taxAssociation in this.itemsDataContracts.SelectMany(x => x.ReqGlTaxesEntityAssociation.ToList()))
            {
                taxAssociation.ItmReqGlTaxAmtAssocMember = null;
            }
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            var expectedAmount = this.itemsDataContracts.SelectMany(x => x.ItemReqEntityAssociation).ToList().Sum(x => x.ItmReqGlForeignAmtAssocMember);
            Assert.AreEqual(expectedAmount, requisition.Amount);
            foreach (var tax in requisition.LineItems.SelectMany(x => x.LineItemTaxes).ToList())
            {
                Assert.AreEqual(0, tax.TaxAmount);
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ProjectsBulkReadReturnsNull()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectDataContracts = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in requisition.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ProjectsBulkReadReturnsEmptyCollection()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectDataContracts = new Collection<DataContracts.Projects>();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in requisition.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ProjectsLineItemsBulkReadReturnsNull()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectLineItemDataContracts = null;
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in requisition.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.IsFalse(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_ProjectsLineItemsBulkReadReturnsEmptyCollection()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectLineItemDataContracts = new Collection<DataContracts.ProjectsLineItems>();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in requisition.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.IsFalse(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }
        #endregion

        #region Status tests
        [TestMethod]
        public async Task GetRequisition_UStatus()
        {
            string requisitionId = "4";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.InProgress, requisition.Status);
        }

        [TestMethod]
        public async Task GetRequisition_NStatus()
        {
            string requisitionId = "2";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.NotApproved, requisition.Status);
        }

        [TestMethod]
        public async Task GetRequisition_OStatus()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.Outstanding, requisition.Status);
        }

        [TestMethod]
        public async Task GetRequisition_PStatus()
        {
            string requisitionId = "6";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.PoCreated, requisition.Status);
        }        
        #endregion

        #region Vendor tests
        [TestMethod]
        public async Task GetRequisition_VendorNameOnly_ShortVendorName()
        {
            string requisitionId = "5";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.VendorName, requisition.VendorName);
        }

        [TestMethod]
        public async Task GetRequisition_VendorNameOnly_LongVendorName()
        {
            string requisitionId = "6";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.VendorName, requisition.VendorName);
        }

        [TestMethod]
        public async Task GetRequisition_VendorIdOnly_CTXShortName()
        {
            string requisitionId = "7";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.hierarchyNamesForIdsResponse.OutPersonNames.First(), requisition.VendorName);
        }

        [TestMethod]
        public async Task GetRequisition_VendorIdOnly_CTXLongName()
        {
            string requisitionId = "8";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts(true);
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.hierarchyNamesForIdsResponse.OutPersonNames.First(), requisition.VendorName);
        }
        [TestMethod]
        public async Task GetRequisition_HasVendorIdAndName()
        {
            string requisitionId = "4";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.VendorName, requisition.VendorName);
        }

        [TestMethod]
        public async Task GetRequisition_NoVendorIdOrName()
        {
            string requisitionId = "10";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            Assert.IsTrue(string.IsNullOrEmpty(requisition.VendorName));
        }
        #endregion

        #region PO tests
        [TestMethod]
        public async Task GetRequisition_ConvertedToPurchaseOrder()
        {
            string requisitionId = "3";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.PurchaseOrders.Count(), requisition.PurchaseOrders.Count());

            for (int i = 0; i < this.requisitionDomainEntity.PurchaseOrders.Count(); i++)
            {
                Assert.AreEqual(this.requisitionDomainEntity.PurchaseOrders[i], requisition.PurchaseOrders[i]);
            }
        }
        #endregion

        #region Approvers and Next Approvers tests
        [TestMethod]
        public async Task GetRequisition_HasApproversAndNextApprovers()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Approvers.Count(), requisition.Approvers.Count());
            foreach (var approver in this.requisitionDomainEntity.Approvers)
            {
                Assert.IsTrue(requisition.Approvers.Any(x =>
                    x.ApproverId == approver.ApproverId
                    && x.ApprovalName == approver.ApprovalName
                    && x.ApprovalDate == approver.ApprovalDate));
            }
        }
        #endregion

        #region LineItems tests
        [TestMethod]
        public async Task GetRequisition_LineItems_Base()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Count(), requisition.LineItems.Count(), "Requisitions should have the same number of line items.");

            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                Assert.IsTrue(requisition.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.DesiredDate == lineItem.DesiredDate
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.InvoiceNumber == lineItem.InvoiceNumber
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue
                    && x.VendorPart == lineItem.VendorPart));
            }
        }

        [TestMethod]
        public async Task GetRequisition_LineItems_LongDescription()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();

            this.itemsDataContracts[0].ItmDesc.Add("more training");
            string lineItemDescription = string.Empty;
            foreach (var desc in this.itemsDataContracts[0].ItmDesc)
            {
                lineItemDescription += desc + ' ';
            }

            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            var lineItem = requisition.LineItems.FirstOrDefault();
            Assert.AreEqual(lineItemDescription, lineItem.Description, "The line item descriptions should be the same and concatenated using blank spaces.");
        }
        #endregion

        #region GL Distribution tests
        [TestMethod]
        public async Task GetRequisition_GlDistributions_AllLocalAmounts()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount);
            foreach (var domainLineItem in this.requisitionDomainEntity.LineItems)
            {
                foreach (var domainGlDistribution in domainLineItem.GlDistributions)
                {
                    foreach (var lineItem in requisition.LineItems)
                    {
                        // Since we're comparing two requisition objects that SHOULD be the same, we only
                        // want to execute the assertion if we know we are comparing the same line items.
                        if (domainLineItem.Id == lineItem.Id)
                        {
                            Assert.IsTrue(lineItem.GlDistributions.Any(x =>
                                x.Amount == domainGlDistribution.Amount
                                && x.GlAccountNumber == domainGlDistribution.GlAccountNumber
                                && x.ProjectId == domainGlDistribution.ProjectId
                                && x.ProjectNumber == domainGlDistribution.ProjectNumber
                                && x.ProjectLineItemCode == domainGlDistribution.ProjectLineItemCode
                                && x.ProjectLineItemId == domainGlDistribution.ProjectLineItemId
                                && x.Quantity == domainGlDistribution.Quantity));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetRequisition_GlDistributions_AllForeignAmounts()
        {
            string requisitionId = "2";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount);
            foreach (var domainLineItem in this.requisitionDomainEntity.LineItems)
            {
                foreach (var domainGlDistribution in domainLineItem.GlDistributions)
                {
                    foreach (var lineItem in requisition.LineItems)
                    {
                        // Since we're comparing two requisition objects that SHOULD be the same, we only
                        // want to execute the assertion if we know we are comparing the same line items.
                        if (domainLineItem.Id == lineItem.Id)
                        {
                            Assert.IsTrue(lineItem.GlDistributions.Any(x =>
                                x.Amount == domainGlDistribution.Amount));
                        }
                    }
                }
            }
        }
        #endregion

        #region Line Item Tax tests
        [TestMethod]
        public async Task GetGetRequisition_LineItemTaxes_AllLocalAmounts()
        {
            string requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount);
            foreach (var domainLineItem in this.requisitionDomainEntity.LineItems)
            {
                foreach (var domainTax in domainLineItem.LineItemTaxes)
                {
                    foreach (var lineItem in requisition.LineItems)
                    {
                        // Since we're comparing two requisition objects that SHOULD be the same, we only
                        // want to execute the assertion if we know we are comparing the same line items taxes.
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
        public async Task GetGetRequisition_LineItemTaxes_AllForeignAmounts()
        {
            string requisitionId = "2";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount);
            foreach (var domainLineItem in this.requisitionDomainEntity.LineItems)
            {
                foreach (var domainTax in domainLineItem.LineItemTaxes)
                {
                    foreach (var lineItem in requisition.LineItems)
                    {
                        // Since we're comparing two requisition objects that SHOULD be the same, we only
                        // want to execute the assertion if we know we are comparing the same line items taxes.
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
        #endregion

        #region GL Security tests
        [TestMethod]
        public async Task UserHasFullAccess()
        {
            var requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amounts should be the same.");
            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Count(), requisition.LineItems.Count(), "We should be able to see all of the requisition line items.");
            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                Assert.IsTrue(requisition.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.DesiredDate == lineItem.DesiredDate
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue
                    && x.VendorPart == lineItem.VendorPart));
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_AllLineItemsAvailable()
        {
            var requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amounts should be the same.");
            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Count(), requisition.LineItems.Count(), "We should be able to see all of the requisition line items.");
            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                Assert.IsTrue(requisition.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.DesiredDate == lineItem.DesiredDate
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue
                    && x.VendorPart == lineItem.VendorPart));
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_PartialLineItemsAvailable()
        {
            var requisitionId = "31";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amount should show as if we have full access.");
            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Count(), requisition.LineItems.Count(), "The requisition should have all of it's line items.");

            decimal glDistributionTotal = 0.00m;
            decimal taxDistributionTotal = 0.00m;
            foreach (var lineItem in requisition.LineItems)
            {
                glDistributionTotal += lineItem.GlDistributions.Sum(x => x.Amount);
                taxDistributionTotal += lineItem.LineItemTaxes.Sum(x => x.TaxAmount);
            }

            Assert.AreEqual(this.requisitionDomainEntity.Amount, glDistributionTotal + taxDistributionTotal, "The requisition amount should be the same as the sum of the GL and tax distributions for all line items");

            foreach (var lineItem in requisition.LineItems)
            {
                foreach (var glDistribution in lineItem.GlDistributions)
                {
                    if (glDistribution.GlAccountNumber == "11_10_00_01_20601_51000")
                    {
                        Assert.IsFalse(glDistribution.Masked, "GL account should NOT be masked.");
                    }
                    else
                    {
                        Assert.IsTrue(glDistribution.Masked, "GL account SHOULD be masked.");
                    }
                }
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_SomeLineItemsExcluded()
        {
            var requisitionId = "32";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amount should show as if we have full access.");

            var excludedLineItems = new List<LineItem>();
            foreach (var lineItem in requisition.LineItems)
            {
                excludedLineItems.AddRange(this.requisitionDomainEntity.LineItems.Where(x => x.Id != lineItem.Id));
            }
            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Sum(x => x.ExtendedPrice),
                requisition.LineItems.Sum(x => x.ExtendedPrice) + excludedLineItems.Sum(x => x.ExtendedPrice), "The extended price should reflect which line items are included or excluded.");
            Assert.IsTrue(requisition.LineItems.Count() == 1, "The requisition should only have one line item.");

            foreach (var lineItem in requisition.LineItems)
            {
                foreach (var glDistribution in lineItem.GlDistributions)
                {
                    if (glDistribution.GlAccountNumber == "11_10_00_01_20601_52001")
                    {
                        Assert.IsFalse(glDistribution.Masked, "GL account should NOT be masked.");
                    }
                    else
                    {
                        Assert.IsTrue(glDistribution.Masked, "GL account SHOULD be masked.");
                    }
                }
            }
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_NoLineItemsAvailable()
        {
            var requisitionId = "33";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amount should show as if we have full access.");
            Assert.IsTrue(requisition.LineItems.Count() == 0, "The requisition should have no line items.");
        }

        [TestMethod]
        public async Task UserHasNoAccess()
        {
            var requisitionId = "1";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.No_Access, expenseAccounts);

            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amount should show as if we have full access.");
            Assert.IsTrue(requisition.LineItems.Count() == 0, "The requisition should have no line items.");
        }

        [TestMethod]
        public async Task GetRequisitionAsync_NullExpenseAccounts()
        {
            var requisitionId = "33";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);

            Assert.AreEqual(0, requisition.LineItems.Count());
            Assert.AreEqual(0, requisition.LineItems.SelectMany(x => x.GlDistributions).Count());
        }

        [TestMethod]
        public async Task GetRequisitionAsync_UStatus_PersonIdIsRequestor_AllLineItemsAvailable()
        {
            //this.requisitionDataContract.ReqDefaultInitiator = "0001687";
            //this.requisitionDataContract.ReqRequestor = "0004437";
            personId = "0004437";
            string requisitionId = "4";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);            
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            Assert.AreEqual(this.requisitionDataContract.ReqRequestor, personId);
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.InProgress, requisition.Status);
            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amounts should be the same.");
            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Count(), requisition.LineItems.Count(), "We should be able to see all of the requisition line items.");
            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                Assert.IsTrue(requisition.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.DesiredDate == lineItem.DesiredDate
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue
                    && x.VendorPart == lineItem.VendorPart));
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_UStatus_PersonIdIsInitiator_AllLineItemsAvailable()
        {
            //this.requisitionDataContract.ReqDefaultInitiator = "0001687";
            //this.requisitionDataContract.ReqRequestor = "0004437";
            personId = "0001687";
            string requisitionId = "4";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);            
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            Assert.AreEqual(this.requisitionDataContract.ReqDefaultInitiator, personId);
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.InProgress, requisition.Status);
            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amounts should be the same.");
            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Count(), requisition.LineItems.Count(), "We should be able to see all of the requisition line items.");
            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                Assert.IsTrue(requisition.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.DesiredDate == lineItem.DesiredDate
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue
                    && x.VendorPart == lineItem.VendorPart));
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_UStatus__LineItemWithOnlyDescQty_PersonIdIsRequestor_AllLineItemsAvailable()
        {
            //this.requisitionDataContract.ReqDefaultInitiator = "0001687";
            //this.requisitionDataContract.ReqRequestor = "0004437";
            personId = "0004437";
            string requisitionId = "34";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.InProgress, requisition.Status);
            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Count(), requisition.LineItems.Count(), "We should be able to see all of the requisition line items.");
            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                Assert.IsTrue(requisition.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.DesiredDate == lineItem.DesiredDate
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue
                    && x.VendorPart == lineItem.VendorPart));
            }
        }

        [TestMethod]
        public async Task GetRequisitionAsync_UStatus_LineItemWithOnlyDescQty_PersonIdIdIsNeitherRequestorNorInitiator()
        {
            //this.requisitionDataContract.ReqDefaultInitiator = "0001687";
            //this.requisitionDataContract.ReqRequestor = "0004437";            
            string requisitionId = "34";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.InProgress, requisition.Status);
            Assert.AreEqual(0, requisition.LineItems.Count(), "We should not be able to see the requisition line items.");            
        }        

        [TestMethod]
        public async Task GetRequisitionAsync_UStatus_PersonIdIdIsNeitherRequestorNorInitiator_AllLineItemsAvailable()
        {
            //this.requisitionDataContract.ReqDefaultInitiator = "0001687";
            //this.requisitionDataContract.ReqRequestor = "0004437";            
            string requisitionId = "4";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(requisitionId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.InProgress, requisition.Status);
            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amounts should be the same.");
            Assert.AreEqual(this.requisitionDomainEntity.LineItems.Count(), requisition.LineItems.Count(), "We should be able to see all of the requisition line items.");
            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                Assert.IsTrue(requisition.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.DesiredDate == lineItem.DesiredDate
                    && x.Price == lineItem.Price
                    && x.Quantity == lineItem.Quantity
                    && x.TaxForm == lineItem.TaxForm
                    && x.TaxFormCode == lineItem.TaxFormCode
                    && x.TaxFormLocation == lineItem.TaxFormLocation
                    && x.UnitOfIssue == lineItem.UnitOfIssue
                    && x.VendorPart == lineItem.VendorPart));
            }
        }
        
        [TestMethod]
        public async Task GetRequisition_UStatus_PersonIdIdIsNeitherRequestorNorInitiator_UserHasNoAccess()
        {
            string requisitionId = "4";
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = new List<string>();
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var requisition = await requisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Possible_Access, expenseAccounts);
            Assert.AreEqual(RequisitionStatus.InProgress, requisition.Status);
            Assert.AreEqual(this.requisitionDomainEntity.Amount, requisition.Amount, "The requisition amount should show as if we have full access.");
            Assert.IsTrue(requisition.LineItems.Count() == 0, "The requisition should have no line items.");

        }
        #endregion

        #region Requisitions Summary Test
        [TestMethod]
        public async Task GetRequisitionsSummaryByPersonIdAsync_Base()
        {
            string requisitionId = "1";
            InitDataForRequisitionSummary();
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();
            requisitionDataContractList.Add(this.requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);
            var requisitionSummaryList = await testRequisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
            var actual = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.ToList().Count == 1);

            var actualRequisitionSummary = requisitionSummaryList.Where(x => x.Id == requisitionId).FirstOrDefault();
            var requisitionSummary = actual.FirstOrDefault();

            //assert on entity properties
            Assert.AreEqual(actualRequisitionSummary.Id, requisitionSummary.Id);
            Assert.AreEqual(actualRequisitionSummary.Number, requisitionSummary.Number);
            Assert.AreEqual(actualRequisitionSummary.Date, requisitionSummary.Date);
            Assert.AreEqual(actualRequisitionSummary.Status, requisitionSummary.Status);
            Assert.AreEqual(actualRequisitionSummary.StatusDate, requisitionSummary.StatusDate);
            Assert.AreEqual(actualRequisitionSummary.InitiatorName, requisitionSummary.InitiatorName);
            Assert.AreEqual(actualRequisitionSummary.RequestorName, requisitionSummary.RequestorName);
            Assert.AreEqual(actualRequisitionSummary.VendorId, requisitionSummary.VendorId);
            Assert.AreEqual(actualRequisitionSummary.VendorName, requisitionSummary.VendorName);
            Assert.AreEqual(actualRequisitionSummary.Amount, requisitionSummary.Amount);
        }

        [TestMethod]
        public async Task GetRequisitionsSummaryByPersonIdAsync_With_CfwebDefaults()
        {
            string requisitionId = "1";

            InitDataForRequisitionSummary();
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            dataReader.Setup<Task<CfwebDefaults>>(d => d.ReadRecordAsync<CfwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new CfwebDefaults() { CfwebReqStatuses = new List<string> { "0" } });
            });
            
            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();
            requisitionDataContractList.Add(this.requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);

            var requisitionSummaryList = await testRequisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
            var actual = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.ToList().Count == 1);
            dataReader.Verify(x => x.SelectAsync("REQUISITIONS", It.IsAny<string[]>(), It.IsAny<string>()), Times.Once);
            dataReader.Verify(x => x.SelectAsync("REQUISITIONS", It.IsAny<string>()), Times.Once);

            var actualRequisitionSummary = requisitionSummaryList.Where(x => x.Id == requisitionId).FirstOrDefault();
            var requisitionSummary = actual.FirstOrDefault();

            //assert on entity properties
            Assert.AreEqual(actualRequisitionSummary.Id, requisitionSummary.Id);
            Assert.AreEqual(actualRequisitionSummary.Number, requisitionSummary.Number);
            Assert.AreEqual(actualRequisitionSummary.Date, requisitionSummary.Date);
            Assert.AreEqual(actualRequisitionSummary.Status, requisitionSummary.Status);
            Assert.AreEqual(actualRequisitionSummary.StatusDate, requisitionSummary.StatusDate);
            Assert.AreEqual(actualRequisitionSummary.InitiatorName, requisitionSummary.InitiatorName);
            Assert.AreEqual(actualRequisitionSummary.RequestorName, requisitionSummary.RequestorName);
            Assert.AreEqual(actualRequisitionSummary.VendorId, requisitionSummary.VendorId);
            Assert.AreEqual(actualRequisitionSummary.VendorName, requisitionSummary.VendorName);
            Assert.AreEqual(actualRequisitionSummary.Amount, requisitionSummary.Amount);
        }

        [TestMethod]
        public async Task GetRequisitionsSummaryByPersonIdAsync_EmptyList()
        {
            var requisitionSummaryList = await this.requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
            Assert.IsNull(requisitionSummaryList);
        }

        [TestMethod]
        public async Task GetRequisitionsSummaryByPersonIdAsync_CfwebDefaults_EmptyList()
        {
            string[] emptyArray = new string[0];
            //mock SelectAsync to return empty array of string
            dataReader.Setup(dr => dr.SelectAsync("REQUISITIONS", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(emptyArray);
            });
            //mock SelectAsync to return empty array of string
            dataReader.Setup(dr => dr.SelectAsync("REQUISITIONS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(emptyArray);
            });
            dataReader.Setup<Task<CfwebDefaults>>(d => d.ReadRecordAsync<CfwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new CfwebDefaults() { CfwebReqStatuses = new List<string> { "0" } });
            });
            var requisitionSummaryList = await this.requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
            Assert.IsNull(requisitionSummaryList);            
            dataReader.Verify(x => x.SelectAsync("REQUISITIONS", It.IsAny<string>()), Times.Exactly(2));
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_NullPersonId()
        {
            var requisitionSummaryList = await this.requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(null);
        }
        

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_NullStatus()
        {
            InitDataForRequisitionSummary();
            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();
            
            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = null
            };
            
            requisitionDataContractList.Add(requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);            
            var requisition = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_StatusListHasBlankValue()
        {
            InitDataForRequisitionSummary();
            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();

            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "" }
            };

            requisitionDataContractList.Add(requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);
            var requisition = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);

        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_StatusDateHasNullValue()
        {
            InitDataForRequisitionSummary();
            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();

            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "P" },
                ReqStatusDate = new List<DateTime?>() { null }
            };

            requisitionDataContractList.Add(requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);
            var requisition = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_InvalidRequisitionStatus()
        {
            string requisitionId = "1";

            InitDataForRequisitionSummary();
            this.requisitionDomainEntity = await testRequisitionRepository.GetRequisitionAsync(requisitionId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();

            // Mock ReadRecord to return a pre-defined, null requisition data contract
            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "1",
                ReqStatus = new List<string>() { "Z" }
            };
            requisitionDataContractList.Add(requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);
            var requisition = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_NullStatusDate()
        {
            InitDataForRequisitionSummary();
            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();

            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "P" },
                ReqStatusDate = null
            };

            requisitionDataContractList.Add(requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);
            var requisition = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_StatusDateListHasNullValue()
        {
            InitDataForRequisitionSummary();
            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();

            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "P" },
                ReqStatusDate = new List<DateTime?>() { null }
            };


            requisitionDataContractList.Add(requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);
            var requisition = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetRequisitionsSummaryByPersonIdAsync_NullReqDate()
        {
            InitDataForRequisitionSummary();
            Collection<DataContracts.Requisitions> requisitionDataContractList = new Collection<DataContracts.Requisitions>();

            var requisitionDataContract = new Requisitions()
            {
                Recordkey = "10",
                ReqStatus = new List<string>() { "P" },
                ReqStatusDate = new List<DateTime?>() { new DateTime(2015, 1, 1) },
                ReqDate = null
            };


            requisitionDataContractList.Add(requisitionDataContract);
            dataReader.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitionDataContractList);
            var requisition = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);
        }
        #endregion

        #region Private methods

        private RequisitionRepository BuildRequisitionRepository()
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

            return new RequisitionRepository(cacheProviderObject, transactionFactoryObject, loggerObject);
        }

        private void InitializeMockMethods()
        {
            // Mock ReadRecord to return a pre-defined requisition data contract
            dataReader.Setup<Task<Requisitions>>(acc => acc.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.requisitionDataContract);
                });

            // Mock ReadRecord to return a pre-defined ShipTo data contract.
            dataReader.Setup<Task<ShipToCodes>>(acc => acc.ReadRecordAsync<ShipToCodes>(It.IsAny<string>(), true)).Returns(Task.FromResult(this.shipToCodesDataContract));

            // Mock ReadRecord to return a pre-defined Opers data contract.
            // Mock bulk read UT.OPERS bulk read
            opersResponse = new Collection<DataContracts.Opers>()
                {
                    new DataContracts.Opers()
                    {
                        // "0000001"
                        Recordkey = "0000001", SysUserName = "Andy Kleehammer"
                    },
                    new DataContracts.Opers()
                    {
                        // ""
                        Recordkey = "0000002", SysUserName = "Gary Thorne"
                    },
                    new DataContracts.Opers()
                    {
                        // "0000003"
                        Recordkey = "0000003", SysUserName = "Teresa Longerbeam"
                    }
                };
            dataReader.Setup<Task<Collection<DataContracts.Opers>>>(acc => acc.BulkReadRecordAsync<DataContracts.Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(opersResponse);
                });


            // Mock BulkReadRecord to return a list of Projects data contracts

            dataReader.Setup<Task<Collection<DataContracts.Projects>>>(acc => acc.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.projectDataContracts);
                });

            // Mock BulkReadRecord to return a list of ProjectsLineItems data contracts

            dataReader.Setup<Task<Collection<DataContracts.ProjectsLineItems>>>(acc => acc.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.projectLineItemDataContracts);
                });

            // Mock BulkReadRecord to return a list of Items data contracts.
            dataReader.Setup<Task<Collection<Items>>>(acc => acc.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.itemsDataContracts);
                });

            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transactionInvoker.Setup(tio => tio.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(() =>
                {
                    return this.hierarchyNamesForIdsResponse;
                });

            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transactionInvoker.Setup(tio => tio.ExecuteAsync<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(Task.FromResult(this.hierarchyNamesForIdsResponse));
        }

        private List<string> CalculateExpenseAccountsForUser(string requisitionId)
        {
            var expenseAccounts = new List<string>();
            switch (requisitionId)
            {
                case "31":
                    expenseAccounts = new List<string>() { "11_10_00_01_20601_51000" };
                    break;
                case "32":
                    expenseAccounts = new List<string>() { "11_10_00_01_20601_52001" };
                    break;
                case "33":
                    // Do nothing; we want to return an empty list
                    break;
                default:
                    if (this.requisitionDomainEntity.LineItems != null)
                    {
                        foreach (var lineItem in this.requisitionDomainEntity.LineItems)
                        {
                            if ((lineItem.GlDistributions != null) && (lineItem.GlDistributions.Count > 0))
                            {
                                foreach (var glDistribution in lineItem.GlDistributions)
                                {
                                    if (!expenseAccounts.Contains(glDistribution.GlAccountNumber))
                                    {
                                        expenseAccounts.Add(glDistribution.GlAccountNumber);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return expenseAccounts;
        }

        private void ConvertDomainEntitiesIntoDataContracts(bool ctxLongName = false)
        {
            // Convert the Requisition object
            this.requisitionDataContract.Recordkey = this.requisitionDomainEntity.Id;
            this.requisitionDataContract.ReqVendor = this.requisitionDomainEntity.VendorId;

            if (this.requisitionDomainEntity.VendorName == "null")
            {
                this.requisitionDataContract.ReqMiscName = new List<string>() { null };
            }
            else
            {
                this.requisitionDataContract.ReqMiscName = new List<string>() { this.requisitionDomainEntity.VendorName };
            }

            // vendor name, initiator name and requestor name come from CTX

            this.requisitionDataContract.ReqDefaultInitiator = "0001687";
            this.requisitionDataContract.ReqRequestor = "0004437";

            // For the unit tests that use requisitions 5 and 6, there is no vendor id, so we do not need to call the CTX
            if ((requisitionDomainEntity.Id != "5") && (requisitionDomainEntity.Id != "6") && (requisitionDomainEntity.Id != "10"))
            {
                string ctxVendorName = "Ellucian Consulting, Inc.";
                if (ctxLongName)
                {
                    ctxVendorName = "Very long vendor name for use in a colleague transaction";
                }

                this.hierarchyNamesForIdsResponse = new GetHierarchyNamesForIdsResponse()
                {
                    IoPersonIds = new List<string>() { this.requisitionDomainEntity.VendorId, this.requisitionDataContract.ReqDefaultInitiator, this.requisitionDataContract.ReqRequestor },
                    IoHierarchies = new List<string>() { "PO", "PREFERRED", "PREFERRED" },
                    OutPersonNames = new List<string>() { ctxVendorName, this.requisitionDomainEntity.InitiatorName, this.requisitionDomainEntity.RequestorName }
                };
            }

            this.requisitionDataContract.ReqNo = this.requisitionDomainEntity.Number;
            this.requisitionDataContract.ReqTotalAmt = this.requisitionDomainEntity.Amount;
            this.requisitionDataContract.ReqApType = this.requisitionDomainEntity.ApType;
            this.requisitionDataContract.ReqDate = this.requisitionDomainEntity.Date;
            this.requisitionDataContract.ReqMaintGlTranDate = this.requisitionDomainEntity.MaintenanceDate;
            this.requisitionDataContract.ReqDesiredDeliveryDate = this.requisitionDomainEntity.DesiredDate;
            this.requisitionDataContract.ReqShipTo = this.requisitionDomainEntity.ShipToCode;
            this.requisitionDataContract.ReqComments = this.requisitionDomainEntity.InternalComments;
            this.requisitionDataContract.ReqPrintedComments = this.requisitionDomainEntity.Comments;
            this.requisitionDataContract.ReqCurrencyCode = this.requisitionDomainEntity.CurrencyCode;

            // A requisition can only be associated to one bpo even though the CDD is defined as a list
            this.requisitionDataContract.ReqBpoNo = new List<string>();
            this.requisitionDataContract.ReqBpoNo.Add(this.requisitionDomainEntity.BlanketPurchaseOrder);

            this.requisitionDataContract.ReqStatus = new List<string>();
            switch (this.requisitionDomainEntity.Status)
            {
                case RequisitionStatus.InProgress:
                    this.requisitionDataContract.ReqStatus.Add("U");
                    break;
                case RequisitionStatus.NotApproved:
                    this.requisitionDataContract.ReqStatus.Add("N");
                    break;
                case RequisitionStatus.Outstanding:
                    this.requisitionDataContract.ReqStatus.Add("O");
                    break;
                case RequisitionStatus.PoCreated:
                    this.requisitionDataContract.ReqStatus.Add("P");
                    break;
                default:
                    throw new Exception("Invalid status specified in PurchaseOrderRepositoryTests");
            }

            // Build the requisition status date
            this.requisitionDataContract.ReqStatusDate = new List<DateTime?>();
            this.requisitionDataContract.ReqStatusDate.Add(this.requisitionDomainEntity.StatusDate);

            // Build the Ship To Code contract
            this.shipToCodesDataContract = new ShipToCodes()
            {
                Recordkey = this.requisitionDomainEntity.ShipToCode,
                ShptName = "Main Campus Delivery"
            };

            // Build a list of purchase orders related to the requisition
            this.requisitionDataContract.ReqPoNo = new List<string>();
            foreach (var po in this.requisitionDomainEntity.PurchaseOrders)
            {
                if (!String.IsNullOrEmpty(po))
                {
                    this.requisitionDataContract.ReqPoNo.Add(po);
                }
            }

            // Build a list of line item IDs
            this.requisitionDataContract.ReqItemsId = new List<string>();
            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                if (lineItem.Id != "null")
                {
                    this.requisitionDataContract.ReqItemsId.Add(lineItem.Id);
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
            this.requisitionDataContract.ReqAuthEntityAssociation = new List<RequisitionsReqAuth>();
            this.requisitionDataContract.ReqApprEntityAssociation = new List<RequisitionsReqAppr>();
            this.opersDataContracts = new Collection<DataContracts.Opers>();
            this.requisitionDataContract.ReqAuthorizations = new List<string>();
            this.requisitionDataContract.ReqNextApprovalIds = new List<string>();
            foreach (var approver in this.requisitionDomainEntity.Approvers)
            {
                if (approver.ApprovalDate != null)
                {
                    // Populate approvers
                    var dataContract = new RequisitionsReqAuth()
                    {
                        ReqAuthorizationsAssocMember = approver.ApproverId,
                        ReqAuthorizationDatesAssocMember = approver.ApprovalDate
                    };

                    this.requisitionDataContract.ReqAuthEntityAssociation.Add(dataContract);
                    this.requisitionDataContract.ReqAuthorizations.Add(approver.ApproverId);
                }
                else
                {
                    // Populate next approvers
                    var nextApproverDataContract = new RequisitionsReqAppr()
                    {
                        ReqNextApprovalIdsAssocMember = approver.ApproverId
                    };
                    this.requisitionDataContract.ReqApprEntityAssociation.Add(nextApproverDataContract);
                    this.requisitionDataContract.ReqNextApprovalIds.Add(approver.ApproverId);
                }

                // Populate the Opers data contract
                this.opersDataContracts.Add(new DataContracts.Opers()
                {
                    Recordkey = approver.ApproverId,
                    SysUserName = approver.ApprovalName
                });
            }
        }

        private void ConvertLineItemsIntoDataContracts()
        {
            this.itemsDataContracts = new Collection<Items>();
            this.projectDataContracts = new Collection<DataContracts.Projects>();
            this.projectLineItemDataContracts = new Collection<DataContracts.ProjectsLineItems>();

            foreach (var lineItem in this.requisitionDomainEntity.LineItems)
            {
                // Populate the line items directly
                var itemsDataContract = new Items()
                {
                    Recordkey = lineItem.Id,
                    ItmDesc = new List<string>() { lineItem.Description },
                    ItmReqQty = lineItem.Quantity,
                    ItmReqPrice = lineItem.Price,
                    ItmReqExtPrice = lineItem.ExtendedPrice,
                    ItmReqIssue = lineItem.UnitOfIssue,
                    ItmTaxForm = lineItem.TaxForm,
                    ItmTaxFormCode = lineItem.TaxFormCode,
                    ItmTaxFormLoc = lineItem.TaxFormLocation,
                    ItmComments = lineItem.Comments,
                    ItmDesiredDeliveryDate = lineItem.DesiredDate,
                    ItmVendorPart = lineItem.VendorPart,
                    ItemReqEntityAssociation = new List<ItemsItemReq>(),
                    ReqGlTaxesEntityAssociation = new List<ItemsReqGlTaxes>()
                };

                // Populate the GL Distributions
                int counter = 0;
                foreach (var glDistr in lineItem.GlDistributions)
                {
                    counter++;
                    decimal localGlAmount = 0,
                        foreignGlAmount = 0;

                    // The amount from the LineItemGlDistribution domain entity is always going to be a local amount.
                    // If the requisition is in foreign currency, we need to manually set the test foreign amounts
                    // since they cannot be gotten from the domain entity. Currently, there is only one foreign
                    // currency requisition in the test data.
                    localGlAmount = glDistr.Amount;
                    if (!string.IsNullOrEmpty(this.requisitionDomainEntity.CurrencyCode))
                    {
                        if (counter == 1)
                        {
                            foreignGlAmount = 22.22m;
                        }
                        else if (counter == 2)
                        {
                            foreignGlAmount = 110.00m;
                        }
                        else
                        {
                            foreignGlAmount = 60.00m;
                        }
                    }

                    itemsDataContract.ItemReqEntityAssociation.Add(new ItemsItemReq()
                    {
                        ItmReqGlNoAssocMember = glDistr.GlAccountNumber,
                        ItmReqGlQtyAssocMember = glDistr.Quantity,
                        ItmReqProjectCfIdAssocMember = glDistr.ProjectId,
                        ItmReqPrjItemIdsAssocMember = glDistr.ProjectLineItemId,
                        ItmReqGlAmtAssocMember = localGlAmount,
                        ItmReqGlForeignAmtAssocMember = foreignGlAmount
                    });

                    this.projectDataContracts.Add(new DataContracts.Projects()
                    {
                        Recordkey = glDistr.ProjectId,
                        PrjRefNo = glDistr.ProjectNumber
                    });

                    this.projectLineItemDataContracts.Add(new DataContracts.ProjectsLineItems()
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

                    // The amount from the LineItemTax domain entity is going to be in local currency
                    //  unless there is a currency code on the requisition.
                    //
                    // If the requisition does not have a currency code, the tax amount in the domain entity
                    // will be in local currency, and the foreign tax amount on the data contract will be null. 
                    //
                    // If the requisition does have a currency code, the tax amount in the domain entity will be in foreign
                    // currency, and we need to manually set the test local tax amounts since they cannot be gotten from
                    // the domain entity. Currently, there is only one foreign currency requisition in the test data.

                    if (string.IsNullOrEmpty(this.requisitionDomainEntity.CurrencyCode))
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
                            localTaxAmount = 5.00m;
                        }
                        else
                        {
                            localTaxAmount = 10.00m;
                        }
                    }

                    itemsDataContract.ReqGlTaxesEntityAssociation.Add(new ItemsReqGlTaxes()
                    {
                        ItmReqGlTaxCodeAssocMember = taxDistr.TaxCode,
                        ItmReqGlTaxAmtAssocMember = localTaxAmount,
                        ItmReqGlForeignTaxAmtAssocMember = foreignTaxAmount
                    });
                }

                this.itemsDataContracts.Add(itemsDataContract);
            }
        }

        private async void InitDataForRequisitionSummary()
        {
            string requisitionId = "1";
            var requisitionsFilename = "REQUISITIONS";
            var requisitionIds = new List<string>()
            {
                requisitionId
            };
            dataReader.Setup(dr => dr.SelectAsync(requisitionsFilename, It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(requisitionIds.ToArray());
            });
            dataReader.Setup(dr => dr.SelectAsync(requisitionsFilename, It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(requisitionIds.ToArray());
            });

            dataReader.Setup<Task<CfwebDefaults>>(d => d.ReadRecordAsync<CfwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new CfwebDefaults());
            });
        }
        #endregion

        #region Delete Test

        [TestMethod]
        public async Task RequisitionRepository_Delete()
        {
            string guid = "ce1f6d92-ed33-4723-afde-4139e53d6c47";

            var _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "REQUISITIONS",
                    new GuidLookupResult() {Entity = "REQUISITIONS", PrimaryKey = "100", SecondaryKey = ""}
                }
            };
            var deleteResponse = new DeleteRequisitionResponse()
            {
                ErrorCode = new List<string>(),
                ErrorMessages = new List<string>(),
                RequisitionsId = "100"
            };
            dataReader.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);
            transactionInvoker.Setup(t => t.ExecuteAsync<DeleteRequisitionRequest, DeleteRequisitionResponse>(It.IsAny<DeleteRequisitionRequest>())).ReturnsAsync(deleteResponse);

            var result = await requisitionRepository.DeleteRequisitionAsync(guid);
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RequisitionRepository_Delete_Entity_Null_KeyNotFoundException()
        {
            string guid = "ce1f6d92-ed33-4723-afde-4139e53d6c47";

            var _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "REQUISITIONS",
                    new GuidLookupResult() {Entity = "", PrimaryKey = "100", SecondaryKey = ""}
                }
            };
            dataReader.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);

            var result = await requisitionRepository.DeleteRequisitionAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RequisitionRepository_Delete_PrimaryKey_Null_KeyNotFoundException()
        {
            string guid = "ce1f6d92-ed33-4723-afde-4139e53d6c47";

            var _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "REQUISITIONS",
                    new GuidLookupResult() {Entity = "REQUISITIONS", PrimaryKey = "", SecondaryKey = ""}
                }
            };
            dataReader.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);

            var result = await requisitionRepository.DeleteRequisitionAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RequisitionRepository_Delete_WrongEntity_KeyNotFoundException()
        {
            string guid = "ce1f6d92-ed33-4723-afde-4139e53d6c47";

            var _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "REQUISITIONS",
                    new GuidLookupResult() {Entity = "PERSONS", PrimaryKey = "100", SecondaryKey = ""}
                }
            };
            dataReader.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);

            var result = await requisitionRepository.DeleteRequisitionAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RequisitionRepository_Delete_Response_Error_KeyNotFoundException()
        {
            string guid = "ce1f6d92-ed33-4723-afde-4139e53d6c47";

            var _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "REQUISITIONS",
                    new GuidLookupResult() {Entity = "REQUISITIONS", PrimaryKey = "100", SecondaryKey = ""}
                }
            };
            var deleteResponse = new DeleteRequisitionResponse()
            {
                ErrorCode = new List<string>() { "Requisition.MissingRecord" },
                ErrorMessages = new List<string>() { "No Id Found" },
                RequisitionsId = ""
            };
            dataReader.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);
            transactionInvoker.Setup(t => t.ExecuteAsync<DeleteRequisitionRequest, DeleteRequisitionResponse>(It.IsAny<DeleteRequisitionRequest>())).ReturnsAsync(deleteResponse);

            var result = await requisitionRepository.DeleteRequisitionAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task RequisitionRepository_Delete_Response_Error_ApplicationException()
        {
            string guid = "ce1f6d92-ed33-4723-afde-4139e53d6c47";

            var _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "REQUISITIONS",
                    new GuidLookupResult() {Entity = "REQUISITIONS", PrimaryKey = "100", SecondaryKey = ""}
                }
            };
            var deleteResponse = new DeleteRequisitionResponse()
            {
                ErrorCode = new List<string>() { "SomeCode" },
                ErrorMessages = new List<string>() { "No Id Found" },
                RequisitionsId = ""
            };
            dataReader.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);
            transactionInvoker.Setup(t => t.ExecuteAsync<DeleteRequisitionRequest, DeleteRequisitionResponse>(It.IsAny<DeleteRequisitionRequest>())).ReturnsAsync(deleteResponse);

            var result = await requisitionRepository.DeleteRequisitionAsync(guid);
        }
        #endregion
    }

    [TestClass]
    public class RequisitionRepositoryTests_V11
    {
        [TestClass]
        public class RequisitionRepositoryTests_GET_DELETE : BaseRepositorySetup
        {
            #region DECLARATIONS

            private RequisitionRepository requisitionRepository;

            private Dictionary<string, GuidLookupResult> dicResult;

            private Requisitions requisition;
            private Person person;
            private Collection<Items> items;
            private List<ItemsItemReq> itemReqEntityAssociation;
            private List<ItemsReqGlTaxes> reqGlTaxesEntityAssociation;
            private Collection<DataContracts.Projects> projects;
            private Collection<DataContracts.ProjectsLineItems> projectLineItems;

            private Collection<Requisitions> requisitions;

            private DeleteRequisitionResponse response;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                requisitionRepository = new RequisitionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "REQUISITIONS", PrimaryKey = "1" } } };

                itemReqEntityAssociation = new List<ItemsItemReq>()
                {
                    new ItemsItemReq(100, "1", 10, 100, 1, "1", 1000, 100, "1", "1"){},
                    new ItemsItemReq(100, "2", null, null, null, "1", 1000, 100, "1", "1")
                };

                reqGlTaxesEntityAssociation = new List<ItemsReqGlTaxes>()
                {
                    new ItemsReqGlTaxes("1", "1", "1", 1000, "1", "1", 500, "1", "1"),
                    new ItemsReqGlTaxes("1", "1", "1", 1000, "1", "1", null, "1", "1")
                };

                items = new Collection<Items>()
                {
                    new Items()
                    {
                        Recordkey = "1",
                        ItmDesc = new List<string>() { "Description" },
                        ItmReqQty = 10,
                        ItmReqPrice = 100,
                        ItmReqExtPrice = 10,
                        ItmDesiredDeliveryDate = DateTime.Now.AddDays(10),
                        ItemReqEntityAssociation = itemReqEntityAssociation,
                        ReqGlTaxesEntityAssociation = reqGlTaxesEntityAssociation
                    },
                    new Items()
                    {
                        Recordkey = "2",
                        ItmDesc = new List<string>() { "Description1", "Description2" }
                    }
                };

                projectLineItems = new Collection<DataContracts.ProjectsLineItems>()
                {
                    new DataContracts.ProjectsLineItems(){ Recordkey = "1", }
                };

                projects = new Collection<DataContracts.Projects>()
                {
                    new DataContracts.Projects() {RecordGuid = guid, Recordkey = "1"}
                };

                person = new Person()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    FirstName = "First",
                    LastName = "Last",
                    PreferredAddress = "1"
                };

                requisition = new Requisitions()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    ReqStatus = new List<string>() { "O" },
                    ReqStatusDate = new List<DateTime?>() { DateTime.Now },
                    ReqDate = DateTime.Now,
                    ReqNo = "1",
                    ReqVendor = "1",
                    ReqDefaultInitiator = "1",
                    ReqMaintGlTranDate = DateTime.Now,
                    ReqDesiredDeliveryDate = DateTime.Now.AddDays(10),
                    ReqShipTo = "1",
                    ReqPoNo = new List<string>() { "1" },
                    ReqBpoNo = new List<string>() { "1" },
                    ReqItemsId = new List<string>() { "1", "2" }
                };

                requisitions = new Collection<Requisitions>()
                {
                    requisition,
                    new Requisitions()
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "2",
                        ReqStatus = new List<string>() { "P" },
                        ReqStatusDate = new List<DateTime?>() { DateTime.Now },
                        ReqDate = DateTime.Now,
                        ReqNo = "1",
                        ReqCurrencyCode = "USD",
                        ReqVendor = "1",
                        ReqDefaultInitiator = "1",
                        ReqMaintGlTranDate = DateTime.Now,
                        ReqDesiredDeliveryDate = DateTime.Now.AddDays(10),
                        ReqItemsId = new List<string>() { "1", "2" }
                    }
                };

                response = new DeleteRequisitionResponse() { RequisitionsId = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(person);
                dataReaderMock.SetupSequence(d => d.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisition)).Returns(Task.FromResult(requisition));
                dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });
                dataReaderMock.Setup(d => d.ReadRecordAsync<PurDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new PurDefaults() { PurShipToCode = "1" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), true)).ReturnsAsync(items);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), true)).ReturnsAsync(projects);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(It.IsAny<string[]>(), true)).ReturnsAsync(projectLineItems);

                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitions);

                transManagerMock.Setup(t => t.ExecuteAsync<DeleteRequisitionRequest, DeleteRequisitionResponse>(It.IsAny<DeleteRequisitionRequest>())).ReturnsAsync(response);
            }

            #endregion

            #region GETBYID

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRequisitionsByGuidAsync_ArgumentNullException_When_Guid_Null()
            {
                await requisitionRepository.GetRequisitionsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetRequisitionsByGuidAsync_KeyNotFoundException_On_Invalid_Guid()
            {
                dicResult.FirstOrDefault().Value.PrimaryKey = null;

                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetRequisitionsByGuidAsync_KeyNotFoundException_When_Requistion_Null_For_Guid()
            {
                dataReaderMock.Setup<Task<Requisitions>>(d => d.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).ReturnsAsync(null);
                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRequisitionsByGuidAsync_ArgumentNullException_When_Requistion_RecordKey_Null()
            {
                requisition.Recordkey = null;
                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }


            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetRequisitionsByGuidAsync_ApplicationException_When_Requistion_Status_Empty()
            {
                requisition.ReqStatus = null;
                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetRequisitionsByGuidAsync_ApplicationException_When_Requistion_Status_Inprogress()
            {
                requisition.ReqStatus = new List<string> {"U" };
                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetRequisitionsByGuidAsync_ApplicationException_When_Requistion_Status_Is_Invalid()
            {
                requisition.ReqStatus = new List<string>() { "A" };
                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetRequisitionsByGuidAsync_ApplicationException_When_Requistion_StatusDate_Is_Empty()
            {
                requisition.ReqStatusDate = null;
                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetRequisitionsByGuidAsync_ApplicationException_When_Requistion_Date_Is_Empty()
            {
                requisition.ReqDate = null;
                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetRequisitionsByGuidAsync_ApplicationException_When_Requistion_BlanketPurchaseOrder_Count_MoreThan_One()
            {
                requisition.ReqStatus = new List<string>() { "N" };
                requisition.ReqBpoNo = new List<string>() { "1", "2" };
                await requisitionRepository.GetRequisitionsByGuidAsync(guid);
            }

            [TestMethod]
            public async Task GetRequisitionsByGuidAsync()
            {
                requisition.ReqStatus = new List<string>() { "O" };
                var result = await requisitionRepository.GetRequisitionsByGuidAsync(guid);

                Assert.IsNotNull(result);
            }

            #endregion

            #region GETALL

            [TestMethod]
            public async Task GetRequisitionsAsync_KeyNotFoundException_When_Requisitions_NotFound_For_Given_Ids()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] {});
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(null);
                var result = await requisitionRepository.GetRequisitionsAsync(0, 2);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Item2 == 0);
            }

            [TestMethod]
            public async Task GetRequisitionsAsync()
            {
                dataReaderMock.SetupSequence(d => d.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true))
                              .Returns(Task.FromResult(requisitions.FirstOrDefault()))
                              .Returns(Task.FromResult(requisitions.LastOrDefault()));

                var result = await requisitionRepository.GetRequisitionsAsync(0, 2);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Item2 == 2);
            }

            #endregion
        }

        [TestClass]
        public class RequisitionRepositoryTests_POST_PUT : BaseRepositorySetup
        {
            #region DECLARATIONS

            private RequisitionRepository requisitionRepository;

            private Dictionary<string, GuidLookupResult> dicResult;

            private Requisitions requisition;
            private Requisition requisitionEntity;
            private Person person;
            private Collection<Items> items;
            private List<ItemsItemReq> itemReqEntityAssociation;
            private List<ItemsReqGlTaxes> reqGlTaxesEntityAssociation;
            private Collection<DataContracts.Projects> projects;
            private Collection<DataContracts.ProjectsLineItems> projectLineItems;

            private Collection<Requisitions> requisitions;

            private UpdateCreateRequisitionResponse response;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                requisitionRepository = new RequisitionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "REQUISITIONS", PrimaryKey = "1" } } };

                itemReqEntityAssociation = new List<ItemsItemReq>()
                {
                    new ItemsItemReq(100, "1", 10, 100, 1, "1", 1000, 100, "1", "1"){},
                    new ItemsItemReq(100, "2", null, null, null, "1", 1000, 100, "1", "1")
                };

                reqGlTaxesEntityAssociation = new List<ItemsReqGlTaxes>()
                {
                    new ItemsReqGlTaxes("1", "1", "1", 1000, "1", "1", 500, "1", "1"),
                    new ItemsReqGlTaxes("1", "1", "1", 1000, "1", "1", null, "1", "1")
                };

                items = new Collection<Items>()
                {
                    new Items()
                    {
                        Recordkey = "1",
                        ItmDesc = new List<string>() { "Description" },
                        ItmReqQty = 10,
                        ItmReqPrice = 100,
                        ItmReqExtPrice = 10,
                        ItmDesiredDeliveryDate = DateTime.Now.AddDays(10),
                        ItemReqEntityAssociation = itemReqEntityAssociation,
                        ReqGlTaxesEntityAssociation = reqGlTaxesEntityAssociation
                    },
                    new Items()
                    {
                        Recordkey = "2",
                        ItmDesc = new List<string>() { "Description1", "Description2" }
                    }
                };

                projectLineItems = new Collection<DataContracts.ProjectsLineItems>()
                {
                    new DataContracts.ProjectsLineItems(){ Recordkey = "1", }
                };

                projects = new Collection<DataContracts.Projects>()
                {
                    new DataContracts.Projects() {RecordGuid = guid, Recordkey = "1"}
                };

                person = new Person()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    FirstName = "First",
                    LastName = "Last",
                    PreferredAddress = "1"
                };

                requisition = new Requisitions()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    ReqStatus = new List<string>() { "O" },
                    ReqStatusDate = new List<DateTime?>() { DateTime.Now },
                    ReqDate = DateTime.Now,
                    ReqNo = "1",
                    ReqVendor = "1",
                    ReqDefaultInitiator = "1",
                    ReqMaintGlTranDate = DateTime.Now,
                    ReqDesiredDeliveryDate = DateTime.Now.AddDays(10),
                    ReqShipTo = "1",
                    ReqPoNo = new List<string>() { "1" },
                    ReqBpoNo = new List<string>() { "1" },
                    ReqItemsId = new List<string>() { "1", "2" }
                };

                requisitions = new Collection<Requisitions>()
                {
                    requisition,
                    new Requisitions()
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "2",
                        ReqStatus = new List<string>() { "P" },
                        ReqStatusDate = new List<DateTime?>() { DateTime.Now },
                        ReqDate = DateTime.Now,
                        ReqNo = "1",
                        ReqCurrencyCode = "USD",
                        ReqVendor = "1",
                        ReqDefaultInitiator = "1",
                        ReqMaintGlTranDate = DateTime.Now,
                        ReqDesiredDeliveryDate = DateTime.Now.AddDays(10),
                        ReqItemsId = new List<string>() { "1", "2" }
                    }
                };

                response = new UpdateCreateRequisitionResponse() { Guid = guid };

                requisitionEntity = new Requisition("1", guid, "1", "name", RequisitionStatus.InProgress, DateTime.Today, DateTime.Today)
                {
                    MaintenanceDate = DateTime.Today,
                    DeliveryDate = DateTime.Today,
                    CurrencyCode = "USD",
                    Buyer = guid,
                    DefaultInitiator = guid,
                    ShipToCode = guid,
                    Fob = guid,
                    AltShippingName = "name",
                    AltShippingAddress = new List<string>() { "address1" },
                    IntgAltShipCountry = "USA",
                    AltShippingState = "SA",
                    AltShippingCity = "City",
                    AltShippingZip = "512321",
                    AltShippingPhoneExt = "1234",
                    AltShippingPhone = "1234-5678",
                    VendorId = guid,
                    MiscName = new List<string>() { "name" },
                    MiscAddress = new List<string>() { "address1" },
                    MiscCountry = "USA",
                    MiscState = "SA",
                    MiscCity = "City",
                    MiscZip = "512321",
                    IntgCorpPerIndicator = "Y",
                    VendorTerms = guid,
                    ApType = guid,
                    InternalComments = "comments",
                    Comments = "comments"

                };

                LineItem lineItem = new LineItem("1", "desc", 10, 1000, 100)
                {
                    DesiredDate = DateTime.Today,
                    TradeDiscountPercentage = 2
                };

                lineItem.AddTax(new LineItemTax("1", 100) { });

                lineItem.AddGlDistribution(new LineItemGlDistribution("1", 10, 1000, 1) { });

                requisitionEntity.AddLineItem(lineItem);
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(person);
                dataReaderMock.SetupSequence(d => d.ReadRecordAsync<Requisitions>(It.IsAny<string>(), true)).Returns(Task.FromResult(requisition)).Returns(Task.FromResult(requisition));
                dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });
                dataReaderMock.Setup(d => d.ReadRecordAsync<PurDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new PurDefaults() { PurShipToCode = "1" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), true)).ReturnsAsync(items);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), true)).ReturnsAsync(projects);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(It.IsAny<string[]>(), true)).ReturnsAsync(projectLineItems);

                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(requisitions);

                transManagerMock.Setup(t => t.ExecuteAsync<UpdateCreateRequisitionRequest, UpdateCreateRequisitionResponse>(It.IsAny<UpdateCreateRequisitionRequest>())).ReturnsAsync(response);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateRequisitionAsync_ArgumentNullException_When_Entity_Null()
            {
                await requisitionRepository.CreateRequisitionAsync((Requisition)null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CreateRequisitionAsync_RepositoryException()
            {
                response.ReqErrors = new List<ReqErrors>() { new ReqErrors() { ErrorCodes = "ERROR", ErrorMessages = "message" } };
                await requisitionRepository.CreateRequisitionAsync(requisitionEntity);
            }

            [TestMethod]
            public async Task RequisitionRepository_CreateRequisitionAsync()
            {
                var result = await requisitionRepository.CreateRequisitionAsync(requisitionEntity);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequisitionRepository_UpdateRequisitionAsync_Entity_Null()
            {
                await requisitionRepository.UpdateRequisitionAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RequisitionRepository_UpdateRequisitionAsync_Entity_Guid_Null()
            {
                requisitionEntity = new Requisition("1", "1", "name", RequisitionStatus.InProgress, DateTime.Today, DateTime.Today) { };

                await requisitionRepository.UpdateRequisitionAsync(requisitionEntity);
            }

            [TestMethod]
            public async Task RequisitionRepository_UpdateRequisitionAsync_Create_Requisition()
            {
                var firstResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "REQUISITIONS", PrimaryKey = null } } };
                dataReaderMock.SetupSequence(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).Returns(Task.FromResult(firstResult)).Returns(Task.FromResult(dicResult));
                var result = await requisitionRepository.UpdateRequisitionAsync(requisitionEntity);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdateRequisitionAsync_RepositoryException()
            {
                response.ReqErrors = new List<ReqErrors>() { new ReqErrors() { ErrorCodes = "ERROR", ErrorMessages = "message" } };
                await requisitionRepository.UpdateRequisitionAsync(requisitionEntity);
            }

            [TestMethod]
            public async Task RequisitionRepository_UpdateRequisitionAsync()
            {
                var result = await requisitionRepository.UpdateRequisitionAsync(requisitionEntity);

                Assert.IsNotNull(result);
            }
        }
    }
}
