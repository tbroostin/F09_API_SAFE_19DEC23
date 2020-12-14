// Copyright 2015-2020 Ellucian Company L.P. and its affiliates

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
using System.Threading;
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class PurchaseOrderRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup
      
        private PurchaseOrderRepository purchaseOrderRepository;
        private TestPurchaseOrderRepository testPurchaseOrderRepository;
        private PurchaseOrder purchaseOrderDomainEntity;

        // Data contract objects
        private PurchaseOrders purchaseOrderDataContract;
        private Collection<Opers> opersDataContracts;
        private ShipToCodes shipToCodesDataContract;
        private Collection<Items> itemsDataContracts;
        private Collection<Projects> projectDataContracts;
        private Collection<PurchaseOrders> purchaseOrdersCollection;
        private Collection<ProjectsLineItems> projectLineItemDataContracts;
        private GetHierarchyNamesForIdsResponse hierarchyNamesForIdsResponse;
        private TxCheckUserGlAccessResponse checkUserGlAccessResponse;
        private Collection<Opers> opersResponse;

        private string personId = "1";
        private string purchaseOrderIdForTransaction;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();


            // Initialize the data contract object.
            purchaseOrderDataContract = new PurchaseOrders();

            // Initialize the purchase order repository.
            testPurchaseOrderRepository = new TestPurchaseOrderRepository();

            purchaseOrderIdForTransaction = "";

            this.purchaseOrderRepository = BuildPurchaseOrderRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
            purchaseOrderDataContract = null;
            shipToCodesDataContract = null;
            opersDataContracts = null;
            itemsDataContracts = null;
            projectDataContracts = null;
            projectLineItemDataContracts = null;
            testPurchaseOrderRepository = null;
            purchaseOrderDomainEntity = null;
            hierarchyNamesForIdsResponse = null;
            checkUserGlAccessResponse = null;
            opersResponse = null;
            purchaseOrderIdForTransaction = null;
        }
        #endregion

        #region Base purchase order test
        [TestMethod]
        public async Task GetPurchaseOrder_Base()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);

            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // Confirm that the SV properties for the purchase order are the same
            Assert.AreEqual(this.purchaseOrderDomainEntity.Id, purchaseOrder.Id);
            Assert.AreEqual(this.purchaseOrderDomainEntity.VendorId, purchaseOrder.VendorId);
            Assert.AreEqual(this.purchaseOrderDomainEntity.VendorName, purchaseOrder.VendorName);
            Assert.AreEqual(this.purchaseOrderDomainEntity.InitiatorName, purchaseOrder.InitiatorName);
            Assert.AreEqual(this.purchaseOrderDomainEntity.RequestorName, purchaseOrder.RequestorName);
            Assert.AreEqual(this.purchaseOrderDomainEntity.Status, purchaseOrder.Status);
            Assert.AreEqual(this.purchaseOrderDomainEntity.StatusDate, purchaseOrder.StatusDate);
            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount);
            Assert.AreEqual(this.purchaseOrderDomainEntity.ApType, purchaseOrder.ApType);
            Assert.AreEqual(this.purchaseOrderDomainEntity.Date, purchaseOrder.Date);
            Assert.AreEqual(this.purchaseOrderDomainEntity.MaintenanceDate, purchaseOrder.MaintenanceDate);
            Assert.AreEqual(this.purchaseOrderDomainEntity.DeliveryDate, purchaseOrder.DeliveryDate);
            Assert.AreEqual(this.purchaseOrderDomainEntity.ShipToCodeName + " " + this.shipToCodesDataContract.ShptName, purchaseOrder.ShipToCodeName);
            Assert.AreEqual(this.purchaseOrderDomainEntity.Comments, purchaseOrder.Comments);
            Assert.AreEqual(this.purchaseOrderDomainEntity.InternalComments, purchaseOrder.InternalComments);
            Assert.AreEqual(this.purchaseOrderDomainEntity.CurrencyCode, purchaseOrder.CurrencyCode);
        }

        [TestMethod]
        public async Task PurchaseOrderRepositoryTests_GetPurchaseOrdersAsync()
        {

            string purchaseOrderId = "1";
            var purchaseOrderGuid = Guid.NewGuid().ToString();
            var testPurchaseOrderRepository = new TestPurchaseOrderRepository(purchaseOrderGuid);

            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderGuid, purchaseOrderId, personId, GlAccessLevel.Full_Access, null);

            purchaseOrderDataContract.RecordGuid = purchaseOrderGuid;


            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);

            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var purchaseOrders = await purchaseOrderRepository.GetPurchaseOrdersAsync(0, 1, string.Empty);

            var purchaseOrder = purchaseOrders.Item1.FirstOrDefault(x => x.Id == this.purchaseOrderDomainEntity.Id);

            // Confirm that the SV properties for the purchase order are the same
            Assert.AreEqual(this.purchaseOrderDomainEntity.Id, purchaseOrder.Id);
            Assert.AreEqual(this.purchaseOrderDomainEntity.VendorId, purchaseOrder.VendorId);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Status, purchaseOrder.Status);
            Assert.AreEqual(this.purchaseOrderDomainEntity.StatusDate, purchaseOrder.StatusDate);
            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount);
            Assert.AreEqual(this.purchaseOrderDomainEntity.ApType, purchaseOrder.ApType);
            Assert.AreEqual(this.purchaseOrderDomainEntity.Date, purchaseOrder.Date);
            Assert.AreEqual(this.purchaseOrderDomainEntity.MaintenanceDate, purchaseOrder.MaintenanceDate);
            Assert.AreEqual(this.purchaseOrderDomainEntity.DeliveryDate, purchaseOrder.DeliveryDate);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Comments, purchaseOrder.Comments);
            Assert.AreEqual(this.purchaseOrderDomainEntity.InternalComments, purchaseOrder.InternalComments);
            Assert.AreEqual(this.purchaseOrderDomainEntity.CurrencyCode, purchaseOrder.CurrencyCode);
        }

        [TestMethod]
        public async Task PurchaseOrderRepositoryTests_GetPurchaseOrdersAsync_With_OrderNumber()
        {

            string purchaseOrderId = "1";
            var purchaseOrderGuid = Guid.NewGuid().ToString();
            var testPurchaseOrderRepository = new TestPurchaseOrderRepository( purchaseOrderGuid );

            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync( purchaseOrderGuid, purchaseOrderId, personId, GlAccessLevel.Full_Access, null );

            purchaseOrderDataContract.RecordGuid = purchaseOrderGuid;


            var expenseAccounts = CalculateExpenseAccountsForUser( purchaseOrderId );

            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var purchaseOrders = await purchaseOrderRepository.GetPurchaseOrdersAsync( 0, 1, "1" );

            var purchaseOrder = purchaseOrders.Item1.FirstOrDefault( x => x.Id == this.purchaseOrderDomainEntity.Id );

            // Confirm that the SV properties for the purchase order are the same
            Assert.AreEqual( this.purchaseOrderDomainEntity.Id, purchaseOrder.Id );
            Assert.AreEqual( this.purchaseOrderDomainEntity.VendorId, purchaseOrder.VendorId );

            Assert.AreEqual( this.purchaseOrderDomainEntity.Status, purchaseOrder.Status );
            Assert.AreEqual( this.purchaseOrderDomainEntity.StatusDate, purchaseOrder.StatusDate );
            Assert.AreEqual( this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount );
            Assert.AreEqual( this.purchaseOrderDomainEntity.ApType, purchaseOrder.ApType );
            Assert.AreEqual( this.purchaseOrderDomainEntity.Date, purchaseOrder.Date );
            Assert.AreEqual( this.purchaseOrderDomainEntity.MaintenanceDate, purchaseOrder.MaintenanceDate );
            Assert.AreEqual( this.purchaseOrderDomainEntity.DeliveryDate, purchaseOrder.DeliveryDate );

            Assert.AreEqual( this.purchaseOrderDomainEntity.Comments, purchaseOrder.Comments );
            Assert.AreEqual( this.purchaseOrderDomainEntity.InternalComments, purchaseOrder.InternalComments );
            Assert.AreEqual( this.purchaseOrderDomainEntity.CurrencyCode, purchaseOrder.CurrencyCode );
        }

        [TestMethod]
        public async Task PurchaseOrderRepositoryTests_GetPurchaseOrdersByGuidAsync()
        {
            string purchaseOrderId = "1";
            var purchaseOrderGuid = "d43c02c1-ce35-4090-a30b-4e4735670ccd";
            var testPurchaseOrderRepository = new TestPurchaseOrderRepository(purchaseOrderGuid);

            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderGuid, purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            purchaseOrderDataContract.RecordGuid = purchaseOrderGuid;

            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);

            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            GuidLookupResult result = new GuidLookupResult() { Entity = "PURCHASE.ORDERS", PrimaryKey = purchaseOrderId, SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(purchaseOrderGuid, result);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);


            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrdersByGuidAsync(purchaseOrderGuid);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Id, purchaseOrder.Id);
            Assert.AreEqual(this.purchaseOrderDomainEntity.VendorId, purchaseOrder.VendorId);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Status, purchaseOrder.Status);
            Assert.AreEqual(this.purchaseOrderDomainEntity.StatusDate, purchaseOrder.StatusDate);
            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount);
            Assert.AreEqual(this.purchaseOrderDomainEntity.ApType, purchaseOrder.ApType);
            Assert.AreEqual(this.purchaseOrderDomainEntity.Date, purchaseOrder.Date);
            Assert.AreEqual(this.purchaseOrderDomainEntity.MaintenanceDate, purchaseOrder.MaintenanceDate);
            Assert.AreEqual(this.purchaseOrderDomainEntity.DeliveryDate, purchaseOrder.DeliveryDate);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Comments, purchaseOrder.Comments);
            Assert.AreEqual(this.purchaseOrderDomainEntity.InternalComments, purchaseOrder.InternalComments);
            Assert.AreEqual(this.purchaseOrderDomainEntity.CurrencyCode, purchaseOrder.CurrencyCode);
        }
        #endregion

        #region Invalid data tests        

        [TestMethod]
        [ExpectedException( typeof( RepositoryException ) )]
        public async Task PurchaseOrderRepositoryTests_GetPurchaseOrdersAsync_EntityBuild_RepositoryException()
        {

            string purchaseOrderId = "1";
            var purchaseOrderGuid = Guid.NewGuid().ToString();
            var testPurchaseOrderRepository = new TestPurchaseOrderRepository( purchaseOrderGuid );
            dataReaderMock.Setup( repo => repo.ReadRecordAsync<Base.DataContracts.Person>( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>() ) ).ThrowsAsync( new Exception() );
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync( purchaseOrderGuid, purchaseOrderId, personId, GlAccessLevel.Full_Access, null );

            purchaseOrderDataContract.RecordGuid = purchaseOrderGuid;


            var expenseAccounts = CalculateExpenseAccountsForUser( purchaseOrderId );

            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            var purchaseOrders = await purchaseOrderRepository.GetPurchaseOrdersAsync( 0, 1, string.Empty );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPurchaseOrder_NullId()
        {
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync(null, personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetPurchaseOrdersByGuidAsync_ApplicationException_When_Status_Inprogress()
        {
            string purchaseOrderId = "1";
            var purchaseOrderGuid = "d43c02c1-ce35-4090-a30b-4e4735670ccd";
            var testPurchaseOrderRepository = new TestPurchaseOrderRepository(purchaseOrderGuid);

            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderGuid, purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            purchaseOrderDataContract.RecordGuid = purchaseOrderGuid;
            
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);

            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            GuidLookupResult result = new GuidLookupResult() { Entity = "PURCHASE.ORDERS", PrimaryKey = purchaseOrderId, SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(purchaseOrderGuid, result);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            purchaseOrderDataContract.PoStatus = new List<string> { "U" };
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(this.purchaseOrderDataContract);
            await purchaseOrderRepository.GetPurchaseOrdersByGuidAsync(purchaseOrderGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetPurchaseOrder_NullPurchaseOrder()
        {
            // Mock ReadRecord to return a pre-defined, null purchase orders data contract
            var nullPurchaseOrderObject = new PurchaseOrders();
            nullPurchaseOrderObject = null;
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(nullPurchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_NullStatus()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = null
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_StatusListHasNullValue()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { null }
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_StatusListHasBlankValue()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { "" }
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_InvalidStatus()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { "Z" }
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_NullVendorName_NullVendorId()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { "O" },
                PoMiscName = null,
                PoVendor = null
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_VendorNameListHasNullValue_NullVendorId()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { "O" },
                PoMiscName = new List<string>() { null },
                PoVendor = null
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_VendorNameListHasBlankValue_NullVendorId()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { "O" },
                PoMiscName = new List<string>() { "" },
                PoVendor = null
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_NullStatusDate()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { "O" },
                PoMiscName = new List<string>() { "Susty Corp" },
                PoStatusDate = null
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrder_StatusDateListHasNullValue()
        {
            // Mock ReadRecord to return a pre-defined, null Purchase Order data contract
            var purchaseOrderObject = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { "O" },
                PoStatusDate = new List<DateTime?>() { null }
            };
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(purchaseOrderObject));
            var purchaseOrder = await this.purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HierarchyNamesCtxReturnsNullNames()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.hierarchyNamesForIdsResponse.OutPersonNames = null;
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.IsTrue(string.IsNullOrEmpty(purchaseOrder.InitiatorName));
            Assert.IsTrue(string.IsNullOrEmpty(purchaseOrder.RequestorName));
        }

        [TestMethod]
        public async Task GetPurchaseOrder_PoDateIsNull()
        {
            var expectedMessage = "";
            var actualMessage = "";
            try
            {
                string purchaseOrderId = "1";
                this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
                var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
                ConvertDomainEntitiesIntoDataContracts();
                InitializeMockMethods();

                expectedMessage = "Missing date for purchase order: " + this.purchaseOrderDataContract.Recordkey;
                this.purchaseOrderDataContract.PoDate = null;
                var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_PoReqIdsIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoReqIds = null;
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.Requisitions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_PoReqIdsIsEmpty()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoReqIds = new List<string>();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.Requisitions.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_PoVouIdsIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoVouIds = null;
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.Vouchers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_PoVouIdsIsEmpty()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoVouIds = new List<string>();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.Vouchers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_OpersBulkReadReturnsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersResponse = null;
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.Approvers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_OpersBulkReadReturnsEmptyList()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersResponse = new Collection<Opers>();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.Approvers.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ApproversAssociationIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoAuthEntityAssociation = null;
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.opersResponse.Count, purchaseOrder.Approvers.Count);
            foreach (var approver in purchaseOrder.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ApproversAssociationIsEmpty()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoAuthEntityAssociation = new List<PurchaseOrdersPoAuth>();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.opersResponse.Count, purchaseOrder.Approvers.Count);
            foreach (var approver in purchaseOrder.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_PoItemsIdIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoItemsId = null;
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.LineItems.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_PoItemsIdIsEmpty()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoItemsId = new List<string>();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.LineItems.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ItemsBulkReadReturnsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.itemsDataContracts = null;
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.LineItems.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ItemsBulkReadReturnsEmpty()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.itemsDataContracts = new Collection<Items>();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.LineItems.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_LineItemAssociationIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItemPoEntityAssociation = null;
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.LineItems.Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_LineItemAssociationIsEmpty()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItemPoEntityAssociation = new List<ItemsItemPo>();
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, purchaseOrder.LineItems.Count);
        }

        //[TestMethod]
        //public async Task GetPurchaseOrder_LineItemDescriptionIsNull()
        //{
        //    string purchaseOrderId = "1";
        //    this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
        //    var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
        //    ConvertDomainEntitiesIntoDataContracts();
        //    InitializeMockMethods();

        //    foreach (var lineItem in this.itemsDataContracts)
        //    {
        //        lineItem.ItmDesc = null;
        //    }
        //    var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

        //    Assert.AreEqual(0, purchaseOrder.LineItems.Count);
        //}

        [TestMethod]
        public async Task GetPurchaseOrder_LineItemDescriptionIsMultipleLines()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItmDesc = new List<string>() { "Line 1", "Line 2" };
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var lineItem in this.itemsDataContracts)
            {
                var expectedDescription = string.Join(" ", lineItem.ItmDesc) + " ";
                var actualLineItem = purchaseOrder.LineItems.FirstOrDefault(x => x.Description == expectedDescription);

                Assert.IsNotNull(actualLineItem);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ItemQuantityPriceAndExtendedPriceAreNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.ItmPoQty = null;
                lineItem.ItmPoPrice = null;
                lineItem.ItmPoExtPrice = null;
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);
            foreach (var lineItem in purchaseOrder.LineItems)
            {
                Assert.AreEqual(0, lineItem.Quantity);
                Assert.AreEqual(0, lineItem.Price);
                Assert.AreEqual(0, lineItem.ExtendedPrice);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_LineItemStatusIs_Accepted_Invoiced()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
           
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(LineItemStatus.Accepted, purchaseOrder.LineItems[0].LineItemStatus);
            Assert.AreEqual(LineItemStatus.Invoiced, purchaseOrder.LineItems[1].LineItemStatus);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_GlDistributionQuantityAndAmountAreNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.ItemPoEntityAssociation).ToList())
            {
                glDistribution.ItmPoGlQtyAssocMember = null;
                glDistribution.ItmPoGlAmtAssocMember = null;
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in purchaseOrder.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.AreEqual(0, glDistribution.Quantity);
                Assert.AreEqual(0, glDistribution.Amount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_LocalCurrency_AmountIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoCurrencyCode = "";
            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.ItemPoEntityAssociation).ToList())
            {
                glDistribution.ItmPoGlAmtAssocMember = null;
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in purchaseOrder.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.AreEqual(0, glDistribution.Amount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ForeignCurrency_AmountIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoCurrencyCode = "CA";
            foreach (var glDistribution in this.itemsDataContracts.SelectMany(x => x.ItemPoEntityAssociation).ToList())
            {
                glDistribution.ItmPoGlAmtAssocMember = null;
                glDistribution.ItmPoGlForeignAmtAssocMember = null;
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in purchaseOrder.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.AreEqual(0, glDistribution.Amount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_TaxAssociationIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoCurrencyCode = "CA";
            foreach (var lineItem in this.itemsDataContracts)
            {
                lineItem.PoGlTaxesEntityAssociation = null;
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var lineItem in purchaseOrder.LineItems)
            {
                Assert.AreEqual(0, lineItem.LineItemTaxes.Count);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ForeignTaxAmountIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoCurrencyCode = "CA";
            foreach (var taxDistribution in this.itemsDataContracts.SelectMany(x => x.PoGlTaxesEntityAssociation).ToList())
            {
                taxDistribution.ItmPoGlForeignTaxAmtAssocMember = 10m;
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var taxDistribution in purchaseOrder.LineItems.SelectMany(x => x.LineItemTaxes).ToList())
            {
                Assert.AreEqual(10m, taxDistribution.TaxAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_LocalTaxAmountIsNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.purchaseOrderDataContract.PoCurrencyCode = "";
            foreach (var taxDistribution in this.itemsDataContracts.SelectMany(x => x.PoGlTaxesEntityAssociation).ToList())
            {
                taxDistribution.ItmPoGlForeignTaxAmtAssocMember = null;
                taxDistribution.ItmPoGlTaxAmtAssocMember = null;
            }
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var taxDistribution in purchaseOrder.LineItems.SelectMany(x => x.LineItemTaxes).ToList())
            {
                Assert.AreEqual(0, taxDistribution.TaxAmount);
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ProjectsAndProjectsLineItemsBulkReadsReturnNull()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectDataContracts = null;
            this.projectLineItemDataContracts = null;
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in purchaseOrder.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }

        [TestMethod]
        public async Task GetPurchaseOrder_ProjectsAndProjectsLineItemsBulkReadsReturnEmpty()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectDataContracts = new Collection<Projects>();
            this.projectLineItemDataContracts = new Collection<ProjectsLineItems>();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            foreach (var glDistribution in purchaseOrder.LineItems.SelectMany(x => x.GlDistributions).ToList())
            {
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(glDistribution.ProjectLineItemCode));
            }
        }
        #endregion

        #region Status tests
        [TestMethod]
        public async Task GetPurchaseOrder_AStatus()
        {
            string purchaseOrderId = "7";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.Accepted, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_BStatus()
        {
            string purchaseOrderId = "6";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.Backordered, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_CStatus()
        {
            string purchaseOrderId = "10";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.Closed, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_UStatus()
        {
            string purchaseOrderId = "4";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.InProgress, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_IStatus()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.Invoiced, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_NStatus()
        {
            string purchaseOrderId = "2";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.NotApproved, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_OStatus()
        {
            string purchaseOrderId = "3";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.Outstanding, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_PStatus()
        {
            string purchaseOrderId = "11";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.Paid, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_RStatus()
        {
            string purchaseOrderId = "9";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.Reconciled, purchaseOrder.Status);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VStatus()
        {
            string purchaseOrderId = "12";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(PurchaseOrderStatus.Voided, purchaseOrder.Status);
        }

        #endregion

        #region Vendor tests
        [TestMethod]
        public async Task GetPurchaseOrder_VendorNameOnly_ShortVendorName()
        {
            string purchaseOrderId = "5";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.VendorName, purchaseOrder.VendorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VendorNameOnly_LongVendorName()
        {
            string purchaseOrderId = "6";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.VendorName, purchaseOrder.VendorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VendorIdOnly_NameListHasNullValue_CTXShortName()
        {
            string purchaseOrderId = "7";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.hierarchyNamesForIdsResponse.OutPersonNames.First(), purchaseOrder.VendorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_VendorIdOnly_NameIsNull_CTXLongName()
        {
            string purchaseOrderId = "8";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts(true);
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.hierarchyNamesForIdsResponse.OutPersonNames.First(), purchaseOrder.VendorName);
        }

        [TestMethod]
        public async Task GetPurchaseOrder_HasVendorIdAndName()
        {
            string purchaseOrderId = "4";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.VendorName, purchaseOrder.VendorName);
        }

        #endregion

        #region REQ, VOU tests
        [TestMethod]
        public async Task GetPurchaseOrder_OriginatedFromReq()
        {
            string purchaseOrderId = "3";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Requisitions.Count(), purchaseOrder.Requisitions.Count());
            for (int x = 0; x < this.purchaseOrderDomainEntity.Requisitions.Count(); x++)
            {
                Assert.AreEqual(this.purchaseOrderDomainEntity.Requisitions[x], purchaseOrder.Requisitions[x]);
            }
        }
        #endregion

        #region Approvers and Next Approvers tests
        [TestMethod]
        public async Task GetPurchaseOrder_HasApproversAndNextApprovers()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Approvers.Count(), purchaseOrder.Approvers.Count());
            foreach (var approver in this.purchaseOrderDomainEntity.Approvers)
            {
                Assert.IsTrue(purchaseOrder.Approvers.Any(x =>
                    x.ApproverId == approver.ApproverId
                    && x.ApprovalName == approver.ApprovalName
                    && x.ApprovalDate == approver.ApprovalDate));
            }
        }
        #endregion

        #region LineItems tests
        [TestMethod]
        public async Task GetPurchaseOrder_LineItems_Base()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.LineItems.Count(), purchaseOrder.LineItems.Count(), "Purchase Orders should have the same number of line items.");

            foreach (var lineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                Assert.IsTrue(purchaseOrder.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExpectedDeliveryDate == lineItem.ExpectedDeliveryDate
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
        #endregion

        #region GL Distribution tests
        [TestMethod]
        public async Task GetPurchaseOrderAsync_NullExpenseAccounts()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);

            Assert.AreEqual(0, purchaseOrder.LineItems.Count);
            Assert.AreEqual(0, purchaseOrder.LineItems.SelectMany(x => x.GlDistributions).Count());
        }

        [TestMethod]
        public async Task GetPurchaseOrder_GlDistributions_AllLocalAmounts()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount);
            foreach (var domainLineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                foreach (var domainGlDistribution in domainLineItem.GlDistributions)
                {
                    foreach (var lineItem in purchaseOrder.LineItems)
                    {
                        // Since we're comparing two purchase order objects that SHOULD be the same, we only
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
        public async Task GetPurchaseOrder_GlDistributions_AllForeignAmounts()
        {
            string purchaseOrderId = "2";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount);
            foreach (var domainLineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                foreach (var domainGlDistribution in domainLineItem.GlDistributions)
                {
                    foreach (var lineItem in purchaseOrder.LineItems)
                    {
                        // Since we're comparing two purchase order objects that SHOULD be the same, we only
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
        public async Task GetPurchaseOrder_LineItemTaxes_AllLocalAmounts()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount);
            foreach (var domainLineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                foreach (var domainTax in domainLineItem.LineItemTaxes)
                {
                    foreach (var lineItem in purchaseOrder.LineItems)
                    {
                        // Since we're comparing two purchase order objects that SHOULD be the same, we only
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
        public async Task GetPurchaseOrder_LineItemTaxes_AllForeignAmounts()
        {
            string purchaseOrderId = "2";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount);
            foreach (var domainLineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                foreach (var domainTax in domainLineItem.LineItemTaxes)
                {
                    foreach (var lineItem in purchaseOrder.LineItems)
                    {
                        // Since we're comparing two purchase order objects that SHOULD be the same, we only
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
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount, "The purchase order amounts should be the same.");
            Assert.AreEqual(this.purchaseOrderDomainEntity.LineItems.Count(), purchaseOrder.LineItems.Count(), "We should be able to see all of the purchase order line items.");
            foreach (var lineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                Assert.IsTrue(purchaseOrder.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.ExpectedDeliveryDate == lineItem.ExpectedDeliveryDate
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
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount, "The purchase order amounts should be the same.");
            Assert.AreEqual(this.purchaseOrderDomainEntity.LineItems.Count(), purchaseOrder.LineItems.Count(), "We should be able to see all of the purchase order line items.");
            foreach (var lineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                Assert.IsTrue(purchaseOrder.LineItems.Any(x =>
                    x.Comments == lineItem.Comments
                    && x.Description == lineItem.Description
                    && x.ExtendedPrice == lineItem.ExtendedPrice
                    && x.Id == lineItem.Id
                    && x.ExpectedDeliveryDate == lineItem.ExpectedDeliveryDate
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
            string purchaseOrderId = "31";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount, "The purchase order amount should show as if we have full access.");
            Assert.AreEqual(this.purchaseOrderDomainEntity.LineItems.Count(), purchaseOrder.LineItems.Count(), "The purchase order should have all of its line items.");

            decimal glDistributionTotal = 0.00m;
            decimal? taxDistributionTotal = 0.00m;
            foreach (var lineItem in purchaseOrder.LineItems)
            {
                glDistributionTotal += lineItem.GlDistributions.Sum(x => x.Amount);
                taxDistributionTotal += lineItem.LineItemTaxes.Sum(x => x.TaxAmount);
            }

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, glDistributionTotal + taxDistributionTotal, "The purchase order amount should be the same as the sum of the GL and tax distributions for all line items");

            foreach (var lineItem in purchaseOrder.LineItems)
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
            string purchaseOrderId = "32";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount, "The purchase order amount should show as if we have full access.");

            var excludedLineItems = new List<LineItem>();
            foreach (var lineItem in purchaseOrder.LineItems)
            {
                excludedLineItems.AddRange(this.purchaseOrderDomainEntity.LineItems.Where(x => x.Id != lineItem.Id));
            }
            Assert.AreEqual(this.purchaseOrderDomainEntity.LineItems.Sum(x => x.ExtendedPrice),
                purchaseOrder.LineItems.Sum(x => x.ExtendedPrice) + excludedLineItems.Sum(x => x.ExtendedPrice), "The extended price should reflect which line items are included or excluded.");
            Assert.IsTrue(purchaseOrder.LineItems.Count() == 1, "The purchase order should only have one line item.");

            foreach (var lineItem in purchaseOrder.LineItems)
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
            string purchaseOrderId = "33";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount, "The purchase order amount should show as if we have full access.");
            Assert.IsTrue(purchaseOrder.LineItems.Count() == 0, "The purchase order should have no line items.");
        }

        [TestMethod]
        public async Task UserHasNoAccess()
        {
            string purchaseOrderId = "1";
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.No_Access, null);
            var expenseAccounts = CalculateExpenseAccountsForUser(purchaseOrderId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.No_Access, expenseAccounts);

            Assert.AreEqual(this.purchaseOrderDomainEntity.Amount, purchaseOrder.Amount, "The purchase order amount should show as if we have full access.");
            Assert.IsTrue(purchaseOrder.LineItems.Count() == 0, "The purchase order should have no line items.");
        }
        #endregion

        #region Purchase Order Summary Test
        [TestMethod]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_Base()
        {
            string purchaseOrderId = "1";
            InitDataForPurchaseOrderSummary();
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();
            purchaseOrderDataContractList.Add(this.purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<Requisitions>());
            var expectedPurchaseOrderSummaryList = await testPurchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
            var actual = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.ToList().Count == 1);

            var expectedPurchaseOrderSummary = expectedPurchaseOrderSummaryList.Where(x => x.Id == purchaseOrderId).FirstOrDefault();
            var actualPurchaseOrderSummary = actual.FirstOrDefault();

            //assert on entity properties
            Assert.AreEqual(expectedPurchaseOrderSummary.Id, actualPurchaseOrderSummary.Id);
            Assert.AreEqual(expectedPurchaseOrderSummary.Number, actualPurchaseOrderSummary.Number);
            Assert.AreEqual(expectedPurchaseOrderSummary.Date, actualPurchaseOrderSummary.Date);
            Assert.AreEqual(expectedPurchaseOrderSummary.Status, actualPurchaseOrderSummary.Status);
            Assert.AreEqual(expectedPurchaseOrderSummary.InitiatorName, actualPurchaseOrderSummary.InitiatorName);
            Assert.AreEqual(expectedPurchaseOrderSummary.RequestorName, actualPurchaseOrderSummary.RequestorName);
            Assert.AreEqual(expectedPurchaseOrderSummary.VendorId, actualPurchaseOrderSummary.VendorId);
            Assert.AreEqual(expectedPurchaseOrderSummary.VendorName, actualPurchaseOrderSummary.VendorName);
            Assert.AreEqual(expectedPurchaseOrderSummary.Amount, actualPurchaseOrderSummary.Amount);

            Assert.AreEqual(expectedPurchaseOrderSummary.Approvers.Count, actualPurchaseOrderSummary.Approvers.Count);
            Assert.AreEqual(expectedPurchaseOrderSummary.Approvers.Where(a=>a.ApprovalDate == null).ToList().Count, actualPurchaseOrderSummary.Approvers.Where(a => a.ApprovalDate == null).ToList().Count);
            Assert.AreEqual(expectedPurchaseOrderSummary.Approvers.Where(a => a.ApprovalDate != null).ToList().Count, actualPurchaseOrderSummary.Approvers.Where(a => a.ApprovalDate != null).ToList().Count);
        }

        [TestMethod]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_With_CfwebDefaults()
        {
            string purchaseOrderId = "1";

            InitDataForPurchaseOrderSummary();
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            dataReaderMock.Setup<Task<CfwebDefaults>>(d => d.ReadRecordAsync<CfwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new CfwebDefaults() { CfwebPoStatuses = new List<string> { "0" } });
            });

            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();
            purchaseOrderDataContractList.Add(this.purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);

            var purchaseOrderSummaryList = await testPurchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
            var actual = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.ToList().Count == 1);
            dataReaderMock.Verify(x => x.SelectAsync("PURCHASE.ORDERS", It.IsAny<string[]>(), It.IsAny<string>()), Times.Once);
            dataReaderMock.Verify(x => x.SelectAsync("PURCHASE.ORDERS", It.IsAny<string>()), Times.Once);

            var actualPurchaseOrderSummary = purchaseOrderSummaryList.Where(x => x.Id == purchaseOrderId).FirstOrDefault();
            var purchaseOrderSummary = actual.FirstOrDefault();

            //assert on entity properties
            Assert.AreEqual(actualPurchaseOrderSummary.Id, purchaseOrderSummary.Id);
            Assert.AreEqual(actualPurchaseOrderSummary.Number, purchaseOrderSummary.Number);
            Assert.AreEqual(actualPurchaseOrderSummary.Date, purchaseOrderSummary.Date);
            Assert.AreEqual(actualPurchaseOrderSummary.Status, purchaseOrderSummary.Status);
            //Assert.AreEqual(actualRequisitionSummary.StatusDate, requisitionSummary.StatusDate);
            Assert.AreEqual(actualPurchaseOrderSummary.InitiatorName, purchaseOrderSummary.InitiatorName);
            Assert.AreEqual(actualPurchaseOrderSummary.RequestorName, purchaseOrderSummary.RequestorName);
            Assert.AreEqual(actualPurchaseOrderSummary.VendorId, purchaseOrderSummary.VendorId);
            Assert.AreEqual(actualPurchaseOrderSummary.VendorName, purchaseOrderSummary.VendorName);
            Assert.AreEqual(actualPurchaseOrderSummary.Amount, purchaseOrderSummary.Amount);
        }

        [TestMethod]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_EmptyList()
        {
            var purchaseOrderSummaryList = await this.purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
            Assert.IsNull(purchaseOrderSummaryList);
        }

        [TestMethod]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_CfwebDefaults_EmptyList()
        {
            string[] emptyArray = new string[0];
            //mock SelectAsync to return empty array of string
            dataReaderMock.Setup(dr => dr.SelectAsync("PURCHASE.ORDERS", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(emptyArray);
            });
            //mock SelectAsync to return empty array of string
            dataReaderMock.Setup(dr => dr.SelectAsync("PURCHASE.ORDERS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(emptyArray);
            });
            dataReaderMock.Setup<Task<CfwebDefaults>>(d => d.ReadRecordAsync<CfwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new CfwebDefaults() { CfwebReqStatuses = new List<string> { "0" } });
            });
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<PurchaseOrders>());
            var purchaseOrderSummaryList = await this.purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
            Assert.IsNull(purchaseOrderSummaryList);
            dataReaderMock.Verify(x => x.SelectAsync("PURCHASE.ORDERS", It.IsAny<string>()), Times.Exactly(1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_NullPersonId()
        {
            var purchaseOrderSummaryList = await this.purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_NullStatus()
        {
            InitDataForPurchaseOrderSummary();

            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();

            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "10",
                PoStatus= null,
                PoReqIds = new List<string>{ "1" },
                PoDefaultInitiator="ABC",
                PoRequestor="MlOwks",
                PoVendor="ABC Company",
                PoMiscName=new List<string> { "ABC" },
                PoAuthorizations = new List<string> { "0000001", "0000002" },
                PoNextApprovalIds = new List<string> { "0000003" }
            };

            purchaseOrderDataContractList.Add(purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);
             dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Requisitions>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<Requisitions>());
            var requisition = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_StatusListHasBlankValue()
        {
            InitDataForPurchaseOrderSummary();
            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();

            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "10",
                PoStatus = new List<string>() { "" },
                PoReqIds = new List<string> { "1" },
                PoDefaultInitiator = "ABC",
                PoRequestor = "MlOwks",
                PoVendor = "ABC Company",
                PoMiscName = new List<string> { "ABC" },
                PoAuthorizations = new List<string> { "0000001", "0000002" },
                PoNextApprovalIds = new List<string> { "0000003" }
            };

            purchaseOrderDataContractList.Add(purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);
            var requisition = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);

        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_StatusDateHasNullValue()
        {
            InitDataForPurchaseOrderSummary();
            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();

            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "10",
                PoStatus = new List<string>() { "P" },
                PoStatusDate = new List<DateTime?>() { null },
                PoReqIds = new List<string> { "1" },
                PoDefaultInitiator = "ABC",
                PoRequestor = "MlOwks",
                PoVendor = "ABC Company",
                PoMiscName = new List<string> { "ABC" },
                PoAuthorizations = new List<string> { "0000001", "0000002" },
                PoNextApprovalIds = new List<string> { "0000003" }
            };

            purchaseOrderDataContractList.Add(purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);
            var purchaseOrderSummaryList = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_InvalidPurchaseOrderStatus()
        {
            string PurchaseOrderId = "1";

            InitDataForPurchaseOrderSummary();
            this.purchaseOrderDomainEntity = await testPurchaseOrderRepository.GetPurchaseOrderAsync(PurchaseOrderId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();

            // Mock ReadRecord to return a pre-defined, null requisition data contract
            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "1",
                PoStatus = new List<string>() { "Z" },
                PoReqIds = new List<string> { "1" },
                PoDefaultInitiator = "ABC",
                PoRequestor = "MlOwks",
                PoVendor = "ABC Company",
                PoMiscName = new List<string> { "ABC" },
                PoAuthorizations = new List<string> { "0000001", "0000002" },
                PoNextApprovalIds = new List<string> { "0000003" }
            };
            purchaseOrderDataContractList.Add(purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);
            var purchaseOrderSummaryList = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_NullStatusDate()
        {
            InitDataForPurchaseOrderSummary();
            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();

            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "10",
                PoStatus = new List<string>() { "P" },
                PoStatusDate = null,
                PoReqIds = new List<string> { "1" },
                PoDefaultInitiator = "ABC",
                PoRequestor = "MlOwks",
                PoVendor = "ABC Company",
                PoMiscName = new List<string> { "ABC" },
                PoAuthorizations = new List<string> { "0000001", "0000002" },
                PoNextApprovalIds = new List<string> { "0000003" }
            };

            purchaseOrderDataContractList.Add(purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);
            var purchaseOrderSummaryList = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_StatusDateListHasNullValue()
        {
            InitDataForPurchaseOrderSummary();
            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();

            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "10",
                PoStatus = new List<string>() { "P" },
                PoStatusDate = new List<DateTime?>() { null },
                PoReqIds = new List<string> { "1" },
                PoDefaultInitiator = "ABC",
                PoRequestor = "MlOwks",
                PoVendor = "ABC Company",
                PoMiscName = new List<string> { "ABC" },
                PoAuthorizations = new List<string> { "0000001", "0000002" },
                PoNextApprovalIds = new List<string> { "0000003" }
            };


            purchaseOrderDataContractList.Add(purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);
            var purchaseOrderSummaryList = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetPurchaseOrderSummaryByPersonIdAsync_NullPODate()
        {
            InitDataForPurchaseOrderSummary();
            Collection<DataContracts.PurchaseOrders> purchaseOrderDataContractList = new Collection<DataContracts.PurchaseOrders>();

            var purchaseOrderDataContract = new PurchaseOrders()
            {
                Recordkey = "10",
                PoStatus = new List<string>() { "P" },
                PoStatusDate = new List<DateTime?>() { new DateTime(2015, 1, 1) },
                PoDate = null,
                PoReqIds = new List<string> { "1" },
                PoDefaultInitiator = "ABC",
                PoRequestor = "MlOwks",
                PoVendor = "ABC Company",
                PoMiscName = new List<string> { "ABC" },
                PoAuthorizations = new List<string> { "0000001", "0000002" },
                PoNextApprovalIds = new List<string> { "0000003" }
            };


            purchaseOrderDataContractList.Add(purchaseOrderDataContract);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PurchaseOrders>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrderDataContractList);
            var purchaseOrderSummaryList = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);
        }
        #endregion

        #region Purchase Order Create Test
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatePurchaseOrderAsync_NullcreateUpdateRequest()
        {
            var purchaseOrderCreate = await this.purchaseOrderRepository.CreatePurchaseOrderAsync(null);
        }

        [TestMethod]
        public async Task CreatePurchaseOrderAsync_TransactionError()
        {
            PurchaseOrderCreateUpdateRequest createUpdateRequestEntity = new PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                InitiatorInitials = "ANA",
                IsPersonVendor = false,
                PurchaseOrder = new PurchaseOrder("1", "P000001", "VendorName", PurchaseOrderStatus.InProgress, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            };
            // Mock Execute within the transaction invoker to return a TxCreateWebPOResponse object
            TxCreateWebPOResponse createPurchaseOrderResponse = new TxCreateWebPOResponse();
            createPurchaseOrderResponse.APurchaseOrderId = "1";
            createPurchaseOrderResponse.APurchaseOrderNo = "P0001111";
            createPurchaseOrderResponse.APoDate = new DateTime(2020, 02, 01);
            createPurchaseOrderResponse.AError = "true";
            createPurchaseOrderResponse.AlErrorMessages = new List<string>() { "Purchase Order locked" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxCreateWebPORequest, TxCreateWebPOResponse>(It.IsAny<TxCreateWebPORequest>())).ReturnsAsync(createPurchaseOrderResponse);

            var purchaseOrderCreateUpdate = await this.purchaseOrderRepository.CreatePurchaseOrderAsync(createUpdateRequestEntity);
            Assert.IsNotNull(purchaseOrderCreateUpdate);
            Assert.AreEqual(createPurchaseOrderResponse.APurchaseOrderId, createUpdateRequestEntity.PurchaseOrder.Id);
            Assert.IsTrue(purchaseOrderCreateUpdate.ErrorOccured);
        }

        [TestMethod]
        [ExpectedException (typeof (Exception))]
        public async Task CreatePurchaseOrderAsync_Transaction_ThrowException()
        {
            PurchaseOrderCreateUpdateRequest createUpdateRequestEntity = new PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                InitiatorInitials = "ANA",
                IsPersonVendor = false,
                PurchaseOrder = new PurchaseOrder("1", "P000001", "VendorName", PurchaseOrderStatus.InProgress, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            };
            // Mock Execute within the transaction invoker to return a TxCreateWebPOResponse object
            TxCreateWebPOResponse createPurchaseOrderResponse = new TxCreateWebPOResponse();
            createPurchaseOrderResponse.APurchaseOrderId = "1";
            createPurchaseOrderResponse.APurchaseOrderNo = "P0001111";
            createPurchaseOrderResponse.APoDate = new DateTime(2020, 02, 01);
            createPurchaseOrderResponse.AError = "true";
            createPurchaseOrderResponse.AlErrorMessages = new List<string>() { "Purchase Order locked" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxCreateWebPORequest, TxCreateWebPOResponse>(It.IsAny<TxCreateWebPORequest>())).Throws(new Exception());

            await this.purchaseOrderRepository.CreatePurchaseOrderAsync(createUpdateRequestEntity);
            
        }

        [TestMethod]
        public async Task CreatePurchaseOrderAsync_ValidCreateUpdateRequest()
        {
            PurchaseOrderCreateUpdateRequest createUpdateRequestEntity = new PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                InitiatorInitials = "ANA",
                IsPersonVendor = false,
                PurchaseOrder = new PurchaseOrder("1", "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            };
            // Mock Execute within the transaction invoker to return a TxCreateWebPOResponse object
            TxCreateWebPOResponse createPurchaseOrderResponse = new TxCreateWebPOResponse();
            createPurchaseOrderResponse.APurchaseOrderId = "123";
            createPurchaseOrderResponse.APurchaseOrderNo = "P0000123";
            createPurchaseOrderResponse.APoDate = new DateTime(2020, 02, 01);
            createPurchaseOrderResponse.AError = "";
            createPurchaseOrderResponse.AlErrorMessages = new List<string>() { };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxCreateWebPORequest, TxCreateWebPOResponse>(It.IsAny<TxCreateWebPORequest>())).ReturnsAsync(createPurchaseOrderResponse);

            var purchaseOrderCreateUpdate = await this.purchaseOrderRepository.CreatePurchaseOrderAsync(createUpdateRequestEntity);
            Assert.IsNotNull(purchaseOrderCreateUpdate);
        }

        #endregion

        #region Purchase Order Update Test
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatePurchaseOrderAsync_NullcreateUpdateRequest()
        {
            var purchaseOrderCreate = await this.purchaseOrderRepository.UpdatePurchaseOrderAsync(null,null);
        }

        [TestMethod]
        public async Task UpdatePurchaseOrderAsync_TransactionError()
        {
            PurchaseOrderCreateUpdateRequest createUpdateRequestEntity = new PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                InitiatorInitials = "ANA",
                IsPersonVendor = false,
                PurchaseOrder = new PurchaseOrder("1", "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            };
            PurchaseOrder originalPo = new PurchaseOrder("1", "P0001111", "Ellucian Consulting, Inc.", PurchaseOrderStatus.InProgress, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            {
                DeliveryDate = new DateTime(2020, 05, 01),
                Amount = 117,
                CurrencyCode = "",
                MaintenanceDate = null,
                VendorId = "0009876",
                InitiatorName = "Abc",
                RequestorName = "Abc",
                ApType = "AP",
                ShipToCode = "MC",
                ShipToCodeName = "MC Datatel - Main Campus",
                DefaultCommodityCode = "",
                Comments = "It is a PO for Pen",
                InternalComments = "Pen is ordered",
                CommodityCode = "00375",

            };
            // Mock Execute within the transaction invoker to return a TxUpdateWebPurchaseOrderResponse object

            TxUpdateWebPurchaseOrderResponse updatePurchaseOrderResponse = new TxUpdateWebPurchaseOrderResponse();
            updatePurchaseOrderResponse.APoId = "1";
            updatePurchaseOrderResponse.AError = "true";
            updatePurchaseOrderResponse.AlErrorMessages = new List<string>() { "Purchase Order locked" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxUpdateWebPurchaseOrderRequest, TxUpdateWebPurchaseOrderResponse>(It.IsAny<TxUpdateWebPurchaseOrderRequest>())).ReturnsAsync(updatePurchaseOrderResponse);

            var purchaseOrderCreateUpdate = await this.purchaseOrderRepository.UpdatePurchaseOrderAsync(createUpdateRequestEntity, originalPo);
            Assert.IsNotNull(purchaseOrderCreateUpdate);
            Assert.AreEqual(updatePurchaseOrderResponse.APoId, createUpdateRequestEntity.PurchaseOrder.Id);
            Assert.IsTrue(purchaseOrderCreateUpdate.ErrorOccured);
        }

        [TestMethod]
        public async Task UpdatePurchaseOrderAsync_validCreateUpdateRequest_And_validOriginalPO()
        {
            PurchaseOrderCreateUpdateRequest createUpdateRequestEntity = new PurchaseOrderCreateUpdateRequest()
            {
                PersonId = "966",
                ConfEmailAddresses = new List<string>() { "abc@ellucian.com" },
                InitiatorInitials = "ANA",
                IsPersonVendor = false,
                PurchaseOrder = new PurchaseOrder("1", "P000001", "VendorName", PurchaseOrderStatus.Closed, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            };
            PurchaseOrder originalPo = new PurchaseOrder("1", "P0001111", "Ellucian Consulting, Inc.", PurchaseOrderStatus.InProgress, new DateTime(2017, 01, 01), new DateTime(2017, 01, 01))
            {
                DeliveryDate = new DateTime(2020, 05, 01),
                Amount = 117,
                CurrencyCode = "",
                MaintenanceDate = null,
                VendorId = "0009876",
                InitiatorName = "Abc",
                RequestorName = "Abc",
                ApType = "AP",
                ShipToCode = "MC",
                ShipToCodeName = "MC Datatel - Main Campus",
                DefaultCommodityCode = "",
                Comments = "It is a PO for Pen",
                InternalComments = "Pen is ordered",
                CommodityCode = "00375"
            };
            // Mock Execute within the transaction invoker to return a TxUpdateWebPurchaseOrderResponse object

            TxUpdateWebPurchaseOrderResponse updatePurchaseOrderResponse = new TxUpdateWebPurchaseOrderResponse();
            updatePurchaseOrderResponse.APoId = "123";
            //voidPurchaseOrderResponse.A = "P0000123";
            updatePurchaseOrderResponse.AError = "false";
            updatePurchaseOrderResponse.AlErrorMessages = new List<string>() { "Purchase Order locked" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxUpdateWebPurchaseOrderRequest, TxUpdateWebPurchaseOrderResponse>(It.IsAny<TxUpdateWebPurchaseOrderRequest>())).ReturnsAsync(updatePurchaseOrderResponse);

            var purchaseOrderCreateUpdate = await this.purchaseOrderRepository.UpdatePurchaseOrderAsync(createUpdateRequestEntity, originalPo);
            Assert.IsNotNull(purchaseOrderCreateUpdate);
        }

        #endregion

        #region Void PurchaseOrder

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PurchaseOrders_VoidPurchaseOrderAsync_ArgumentNullException_PurchaseOrder_Null()
        {
            await purchaseOrderRepository.VoidPurchaseOrderAsync(null);
        }

        [TestMethod]
        public async Task PurchaseOrders_VoidPurchaseOrderAsync_Transaction_Error()
        {
            // Mock Execute within the transaction invoker to return a TxVoidPurchaseOrderResponse object

            TxVoidPurchaseOrderResponse voidPurchaseOrderResponse = new TxVoidPurchaseOrderResponse();

            voidPurchaseOrderResponse.APurchaseOrderId = "123";
            voidPurchaseOrderResponse.APurchaseOrderNumber = "P0000123";
            voidPurchaseOrderResponse.AErrorOccurred = true;
            voidPurchaseOrderResponse.AlErrorMessages = new List<string>() { "Purchase Order locked" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxVoidPurchaseOrderRequest, TxVoidPurchaseOrderResponse>(It.IsAny<TxVoidPurchaseOrderRequest>())).ReturnsAsync(voidPurchaseOrderResponse);
            
            PurchaseOrderVoidRequest voidRequest = new PurchaseOrderVoidRequest();
            voidRequest.PersonId = "0001234";
            voidRequest.PurchaseOrderId = "123";
            var result = await purchaseOrderRepository.VoidPurchaseOrderAsync(voidRequest);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.PurchaseOrderId, voidRequest.PurchaseOrderId);
            Assert.IsTrue(result.ErrorOccured);
        }

        [TestMethod]
        [ExpectedException (typeof(Exception))]
        public async Task PurchaseOrders_VoidPurchaseOrderAsync_Transaction_ThrowsException()
        {
            // Mock Execute within the transaction invoker to return a TxVoidPurchaseOrderResponse object

            TxVoidPurchaseOrderResponse voidPurchaseOrderResponse = new TxVoidPurchaseOrderResponse();

            voidPurchaseOrderResponse.APurchaseOrderId = "123";
            voidPurchaseOrderResponse.APurchaseOrderNumber = "P0000123";
            voidPurchaseOrderResponse.AErrorOccurred = true;
            voidPurchaseOrderResponse.AlErrorMessages = new List<string>() { "Purchase Order locked" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxVoidPurchaseOrderRequest, TxVoidPurchaseOrderResponse>(It.IsAny<TxVoidPurchaseOrderRequest>())).Throws(new Exception());

            PurchaseOrderVoidRequest voidRequest = new PurchaseOrderVoidRequest();
            voidRequest.PersonId = "0001234";
            voidRequest.PurchaseOrderId = "123";
            await purchaseOrderRepository.VoidPurchaseOrderAsync(voidRequest);
            
        }

        [TestMethod]
        public async Task PurchaseOrders_VoidPurchaseOrderAsync()
        {
            // Mock Execute within the transaction invoker to return a TxVoidPurchaseOrderResponse object

            TxVoidPurchaseOrderResponse voidPurchaseOrderResponse = new TxVoidPurchaseOrderResponse();

            voidPurchaseOrderResponse.APurchaseOrderId = "123";
            voidPurchaseOrderResponse.APurchaseOrderNumber = "P0000123";
            voidPurchaseOrderResponse.AErrorOccurred = false;
            voidPurchaseOrderResponse.AlErrorMessages = null;
            voidPurchaseOrderResponse.AWarningOccurred = true;
            voidPurchaseOrderResponse.AlWarningMessages = new List<string>() { "Warning Occurred" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxVoidPurchaseOrderRequest, TxVoidPurchaseOrderResponse>(It.IsAny<TxVoidPurchaseOrderRequest>())).ReturnsAsync(voidPurchaseOrderResponse);

            PurchaseOrderVoidRequest voidRequest = new PurchaseOrderVoidRequest();
            voidRequest.PersonId = "0001234";
            voidRequest.PurchaseOrderId = "123";
            var result = await purchaseOrderRepository.VoidPurchaseOrderAsync(voidRequest);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.PurchaseOrderId, voidRequest.PurchaseOrderId);
            Assert.IsFalse(result.ErrorOccured);
        }

        [TestMethod]
        public async Task PurchaseOrders_VoidPurchaseOrderAsync_EmptyErrorMsg()
        {
            // Mock Execute within the transaction invoker to return a TxVoidPurchaseOrderResponse object

            TxVoidPurchaseOrderResponse voidPurchaseOrderResponse = new TxVoidPurchaseOrderResponse();

            voidPurchaseOrderResponse.APurchaseOrderId = "123";
            voidPurchaseOrderResponse.APurchaseOrderNumber = "P0000123";
            voidPurchaseOrderResponse.AErrorOccurred = false;
            voidPurchaseOrderResponse.AlErrorMessages = new List<string>() { };
            voidPurchaseOrderResponse.AWarningOccurred = true;
            voidPurchaseOrderResponse.AlWarningMessages = new List<string>() { "Warning Occurred" };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxVoidPurchaseOrderRequest, TxVoidPurchaseOrderResponse>(It.IsAny<TxVoidPurchaseOrderRequest>())).ReturnsAsync(voidPurchaseOrderResponse);

            PurchaseOrderVoidRequest voidRequest = new PurchaseOrderVoidRequest();
            voidRequest.PersonId = "0001234";
            voidRequest.PurchaseOrderId = "123";
            var result = await purchaseOrderRepository.VoidPurchaseOrderAsync(voidRequest);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.PurchaseOrderId, voidRequest.PurchaseOrderId);
            Assert.IsFalse(result.ErrorOccured);
        }

        #endregion

        #region Private methods
        private PurchaseOrderRepository BuildPurchaseOrderRepository()
        {
            // Instantiate all objects necessary to mock data reader and CTX calls.
            //var cacheProviderObject = new Mock<ICacheProvider>().Object;
            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            // The transaction factory has a method to get its data reader
            // Make sure that method returns our mock data reader
            transactionFactory.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transactionFactory.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);

            return new PurchaseOrderRepository(cacheProviderMock.Object, transactionFactoryObject, loggerObject);
        }

        private async void InitDataForPurchaseOrderSummary()
        {
            string purchaseOrderId = "1";
            var purchaseOrdersFilename = "PURCHASE.ORDERS";
            var purchaseOrderIds = new List<string>()
            {
                purchaseOrderId
            };
            dataReaderMock.Setup(dr => dr.SelectAsync(purchaseOrdersFilename, It.IsAny<string[]>(), It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(purchaseOrderIds.ToArray());
            });
            dataReaderMock.Setup(dr => dr.SelectAsync(purchaseOrdersFilename, It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(purchaseOrderIds.ToArray());
            });

            dataReaderMock.Setup<Task<CfwebDefaults>>(d => d.ReadRecordAsync<CfwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(new CfwebDefaults());
            });
        }

        private List<string> CalculateExpenseAccountsForUser(string purchaseOrderId)
        {
            var expenseAccounts = new List<string>();
            switch (purchaseOrderId)
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
                    if (this.purchaseOrderDomainEntity.LineItems != null)
                    {
                        foreach (var lineItem in this.purchaseOrderDomainEntity.LineItems)
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

        private void InitializeMockMethods()
        {
          

            var intlParams = new Base.DataContracts.IntlParams()
            {
                HostCountry = "USA",
                HostDateDelimiter = "/",
                HostShortDateFormat = "MDY"
            };
            dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", It.IsAny<bool>())).ReturnsAsync(intlParams);

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { this.purchaseOrderDataContract.Recordkey });

            dataReaderMock.Setup(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(this.purchaseOrderDataContract);

            purchaseOrdersCollection = new Collection<PurchaseOrders>();

            purchaseOrdersCollection.Add(purchaseOrderDataContract);

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), It.IsAny<bool>()))
               .ReturnsAsync(purchaseOrdersCollection);
        

            var purchaseOrdersCollectionIds = purchaseOrdersCollection.Select(x => x.Recordkey).ToList();

            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 100,
                CacheName = "AllPurchaseOrders",
                Entity = "PURCHASE.ORDERS",
                Sublist = purchaseOrdersCollectionIds,
                TotalCount = purchaseOrdersCollection.Count(),
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
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .ReturnsAsync(resp);

            // Mock ReadRecord to return a pre-defined Purchase Orders data contract.
            dataReaderMock.Setup<Task<PurchaseOrders>>(acc => acc.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).Returns(Task.FromResult(this.purchaseOrderDataContract));

            // Mock ReadRecord to return a pre-defined ShipTo data contract.
            dataReaderMock.Setup<Task<ShipToCodes>>(acc => acc.ReadRecordAsync<ShipToCodes>(It.IsAny<string>(), true)).Returns(Task.FromResult(this.shipToCodesDataContract));

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
            dataReaderMock.Setup<Task<Collection<Opers>>>(acc => acc.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(opersResponse);
            });


            // Mock BulkReadRecord to return a list of Projects data contracts

            dataReaderMock.Setup<Task<Collection<Projects>>>(acc => acc.BulkReadRecordAsync<Projects>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.projectDataContracts);
            });

            // Mock BulkReadRecord to return a list of ProjectsLineItems data contracts

            dataReaderMock.Setup<Task<Collection<ProjectsLineItems>>>(acc => acc.BulkReadRecordAsync<ProjectsLineItems>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.projectLineItemDataContracts);
            });

            // Mock BulkReadRecord to return a list of Items data contracts.
            dataReaderMock.Setup<Task<Collection<Items>>>(acc => acc.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(this.itemsDataContracts);
            });

            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transManagerMock.Setup(tio => tio.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(() =>
            {
                return this.hierarchyNamesForIdsResponse;
            });


            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transManagerMock.Setup(tio => tio.ExecuteAsync<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(Task.FromResult(this.hierarchyNamesForIdsResponse));
        }

        private void ConvertDomainEntitiesIntoDataContracts(bool ctxLongName = false)
        {
            // Convert the Purchase Order object
            this.purchaseOrderDataContract.Recordkey = this.purchaseOrderDomainEntity.Id;
            this.purchaseOrderDataContract.RecordGuid = this.purchaseOrderDomainEntity.Guid;
            this.purchaseOrderDataContract.PoVendor = this.purchaseOrderDomainEntity.VendorId;

            if (this.purchaseOrderDomainEntity.VendorName == "null")
            {
                if (this.purchaseOrderDomainEntity.Id == "8")
                {
                    this.purchaseOrderDataContract.PoMiscName = null;
                }
                else
                {
                    this.purchaseOrderDataContract.PoMiscName = new List<string>() { null };
                }
            }
            else
            {
                this.purchaseOrderDataContract.PoMiscName = new List<string>() { this.purchaseOrderDomainEntity.VendorName };
            }

            // vendor name, initiator name and requestor name come from CTX

            this.purchaseOrderDataContract.PoDefaultInitiator = "0001687";
            this.purchaseOrderDataContract.PoRequestor = "0004437";

            // For the unit tests that use purchase orders 5 and 6, there is no vendor id, so we do not need to call the CTX
            if ((purchaseOrderDomainEntity.Id != "5") && (purchaseOrderDomainEntity.Id != "6"))
            {
                string ctxVendorName = "Ellucian Consulting, Inc.";
                if (ctxLongName)
                {
                    ctxVendorName = "Very long vendor name for use in a colleague transaction";
                }

                this.hierarchyNamesForIdsResponse = new GetHierarchyNamesForIdsResponse()
                {
                    IoPersonIds = new List<string>() { this.purchaseOrderDomainEntity.VendorId, this.purchaseOrderDataContract.PoDefaultInitiator, this.purchaseOrderDataContract.PoRequestor },
                    IoHierarchies = new List<string>() { "PO", "PREFERRED", "PREFERRED" },
                    OutPersonNames = new List<string>() { ctxVendorName, this.purchaseOrderDomainEntity.InitiatorName, this.purchaseOrderDomainEntity.RequestorName }
                };
            }

            this.purchaseOrderDataContract.PoNo = this.purchaseOrderDomainEntity.Number;
            this.purchaseOrderDataContract.PoTotalAmt = this.purchaseOrderDomainEntity.Amount;
            this.purchaseOrderDataContract.PoApType = this.purchaseOrderDomainEntity.ApType;
            this.purchaseOrderDataContract.PoDate = this.purchaseOrderDomainEntity.Date;
            this.purchaseOrderDataContract.PoMaintGlTranDate = this.purchaseOrderDomainEntity.MaintenanceDate;
            this.purchaseOrderDataContract.PoExpectedDeliveryDate = this.purchaseOrderDomainEntity.DeliveryDate;
            this.purchaseOrderDataContract.PoShipTo = this.purchaseOrderDomainEntity.ShipToCodeName;
            this.purchaseOrderDataContract.PoComments = this.purchaseOrderDomainEntity.InternalComments;
            this.purchaseOrderDataContract.PoPrintedComments = this.purchaseOrderDomainEntity.Comments;
            this.purchaseOrderDataContract.PoCurrencyCode = this.purchaseOrderDomainEntity.CurrencyCode;

            this.purchaseOrderDataContract.PoStatus = new List<string>();
            switch (this.purchaseOrderDomainEntity.Status)
            {
                case PurchaseOrderStatus.Accepted:
                    this.purchaseOrderDataContract.PoStatus.Add("A");
                    break;
                case PurchaseOrderStatus.Backordered:
                    this.purchaseOrderDataContract.PoStatus.Add("B");
                    break;
                case PurchaseOrderStatus.Closed:
                    this.purchaseOrderDataContract.PoStatus.Add("C");
                    break;
                case PurchaseOrderStatus.InProgress:
                    this.purchaseOrderDataContract.PoStatus.Add("U");
                    break;
                case PurchaseOrderStatus.Invoiced:
                    this.purchaseOrderDataContract.PoStatus.Add("I");
                    break;
                case PurchaseOrderStatus.NotApproved:
                    this.purchaseOrderDataContract.PoStatus.Add("N");
                    break;
                case PurchaseOrderStatus.Outstanding:
                    this.purchaseOrderDataContract.PoStatus.Add("O");
                    break;
                case PurchaseOrderStatus.Paid:
                    this.purchaseOrderDataContract.PoStatus.Add("P");
                    break;
                case PurchaseOrderStatus.Reconciled:
                    this.purchaseOrderDataContract.PoStatus.Add("R");
                    break;
                case PurchaseOrderStatus.Voided:
                    this.purchaseOrderDataContract.PoStatus.Add("V");
                    break;
                default:
                    throw new Exception("Invalid status specified in PurchaseOrderRepositoryTests");
            }

            // Build the purchase order status date
            this.purchaseOrderDataContract.PoStatusDate = new List<DateTime?>();
            this.purchaseOrderDataContract.PoStatusDate.Add(this.purchaseOrderDomainEntity.StatusDate);

            // Build the Ship To Code contract
            this.shipToCodesDataContract = new ShipToCodes()
            {
                Recordkey = this.purchaseOrderDomainEntity.ShipToCodeName,
                ShptName = "Main Campus Delivery"
            };

            // Build a list of requisitions related to the purchase order
            this.purchaseOrderDataContract.PoReqIds = new List<string>();
            foreach (var req in this.purchaseOrderDomainEntity.Requisitions)
            {
                if (!String.IsNullOrEmpty(req))
                {
                    this.purchaseOrderDataContract.PoReqIds.Add(req);
                }
            }

            // Build a list of vouchers related to the purchase order
            this.purchaseOrderDataContract.PoVouIds = new List<string>();
            foreach (var vou in this.purchaseOrderDomainEntity.Vouchers)
            {
                if (!String.IsNullOrEmpty(vou))
                {
                    this.purchaseOrderDataContract.PoVouIds.Add(vou);
                }
            }

            // Build a list of line item IDs
            this.purchaseOrderDataContract.PoItemsId = new List<string>();
            foreach (var lineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                if (lineItem.Id != "null")
                {
                    this.purchaseOrderDataContract.PoItemsId.Add(lineItem.Id);
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
            this.purchaseOrderDataContract.PoAuthEntityAssociation = new List<PurchaseOrdersPoAuth>();
            this.purchaseOrderDataContract.PoApprEntityAssociation = new List<PurchaseOrdersPoAppr>();
            this.opersDataContracts = new Collection<Opers>();
            this.purchaseOrderDataContract.PoAuthorizations = new List<string>();
            this.purchaseOrderDataContract.PoNextApprovalIds = new List<string>();
            foreach (var approver in this.purchaseOrderDomainEntity.Approvers)
            {
                if (approver.ApprovalDate != null)
                {
                    // Populate approvers
                    var dataContract = new PurchaseOrdersPoAuth()
                    {
                        PoAuthorizationsAssocMember = approver.ApproverId,
                        PoAuthorizationDatesAssocMember = approver.ApprovalDate
                    };

                    this.purchaseOrderDataContract.PoAuthEntityAssociation.Add(dataContract);
                    this.purchaseOrderDataContract.PoAuthorizations.Add(approver.ApproverId);
                }
                else
                {
                    // Populate next approvers
                    var nextApproverDataContract = new PurchaseOrdersPoAppr()
                    {
                        PoNextApprovalIdsAssocMember = approver.ApproverId
                    };
                    this.purchaseOrderDataContract.PoApprEntityAssociation.Add(nextApproverDataContract);
                    this.purchaseOrderDataContract.PoNextApprovalIds.Add(approver.ApproverId);
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

            foreach (var lineItem in this.purchaseOrderDomainEntity.LineItems)
            {
                // Populate the line items directly
                var itemsDataContract = new Items()
                {
                    Recordkey = lineItem.Id,
                    ItmDesc = new List<string>() { lineItem.Description },
                    ItmPoQty = lineItem.Quantity,
                    ItmPoPrice = lineItem.Price,
                    ItmPoExtPrice = lineItem.ExtendedPrice,
                    ItmPoIssue = lineItem.UnitOfIssue,
                    ItmTaxForm = lineItem.TaxForm,
                    ItmTaxFormCode = lineItem.TaxFormCode,
                    ItmTaxFormLoc = lineItem.TaxFormLocation,
                    ItmComments = lineItem.Comments,
                    ItmExpectedDeliveryDate = lineItem.ExpectedDeliveryDate,
                    ItmVendorPart = lineItem.VendorPart,
                    ItmFixedAssetsFlag = lineItem.FixedAssetsFlag,                  
                    ItemPoEntityAssociation = new List<ItemsItemPo>(),
                    PoGlTaxesEntityAssociation = new List<ItemsPoGlTaxes>(),
                    ItemPoStatusEntityAssociation = new List<ItemsItemPoStatus>()
                };

                itemsDataContract.ItmPoStatus = new List<string>();
                switch(lineItem.LineItemStatus)
                {
                    case LineItemStatus.Accepted:
                        itemsDataContract.ItmPoStatus.Add("A");
                        break;
                    case LineItemStatus.Backordered:
                        itemsDataContract.ItmPoStatus.Add("B");
                        break;
                    case LineItemStatus.Closed:
                        itemsDataContract.ItmPoStatus.Add("C");
                        break;
                    case LineItemStatus.Hold:
                        itemsDataContract.ItmPoStatus.Add("H");
                        break;
                    case LineItemStatus.Invoiced:
                        itemsDataContract.ItmPoStatus.Add("I");
                        break;
                    case LineItemStatus.Outstanding:
                        itemsDataContract.ItmPoStatus.Add("O");
                        break;
                    case LineItemStatus.Paid:
                        itemsDataContract.ItmPoStatus.Add("P");
                        break;
                    case LineItemStatus.Reconciled:
                        itemsDataContract.ItmPoStatus.Add("R");
                        break;
                    case LineItemStatus.Voided:
                        itemsDataContract.ItmPoStatus.Add("V");
                        break;                   
                    default:
                        itemsDataContract.ItmPoStatus.Add("None");
                        break;
                }

                itemsDataContract.ItemPoStatusEntityAssociation.Add(new ItemsItemPoStatus(itemsDataContract.ItmPoStatus[0], null));

                // Populate the GL Distributions
                int counter = 0;
                foreach (var glDistr in lineItem.GlDistributions)
                {
                    counter++;
                    decimal localGlAmount = 0,
                        foreignGlAmount = 0;

                    // The amount from the LineItemGlDistribution domain entity is always going to be a local amount.
                    // If the purchase order is in foreign currency, we need to manually set the test foreign amounts
                    // since they cannot be gotten from the domain entity. Currently, there is only one foreign
                    // currency purchase order in the test data.
                    localGlAmount = glDistr.Amount;
                    if (!string.IsNullOrEmpty(this.purchaseOrderDomainEntity.CurrencyCode))
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

                    itemsDataContract.ItemPoEntityAssociation.Add(new ItemsItemPo()
                    {
                        ItmPoGlNoAssocMember = glDistr.GlAccountNumber,
                        ItmPoGlQtyAssocMember = glDistr.Quantity,
                        ItmPoProjectCfIdAssocMember = glDistr.ProjectId,
                        ItmPoPrjItemIdsAssocMember = glDistr.ProjectLineItemId,
                        ItmPoGlAmtAssocMember = localGlAmount,
                        ItmPoGlForeignAmtAssocMember = foreignGlAmount
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

                    // The amount from the LineItemTax domain entity is going to be in local currency
                    //  unless there is a currency code on the purchase order.
                    //
                    // If the purchase order does not have a currency code, the tax amount in the domain entity
                    // will be in local currency, and the foreign tax amount on the data contract will be null. 
                    //
                    // If the purchase order does have a currency code, the tax amount in the domain entity will be in foreign
                    // currency, and we need to manually set the test local tax amounts since they cannot be gotten from
                    // the domain entity. Currently, there is only one foreign currency purchase order in the test data.

                    if (string.IsNullOrEmpty(this.purchaseOrderDomainEntity.CurrencyCode))
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

                    itemsDataContract.PoGlTaxesEntityAssociation.Add(new ItemsPoGlTaxes()
                    {
                        ItmPoGlTaxCodeAssocMember = taxDistr.TaxCode,
                        ItmPoGlTaxAmtAssocMember = localTaxAmount,
                        ItmPoGlForeignTaxAmtAssocMember = foreignTaxAmount
                    });
                }

                this.itemsDataContracts.Add(itemsDataContract);
            }
        }
        #endregion
    }

    [TestClass]
    public class PurchaseOrderRepositoryTests_V10 : BaseRepositorySetup
    {
        [TestClass]
        public class PurchaseOrderRepositoryTests_POST : BaseRepositorySetup
        {
            #region DECLARATIONS

            private PurchaseOrderRepository purchaseOrderRepository;

            private PurchaseOrder purchaseOrder;
            private DataContracts.PurchaseOrders purchaseOrders;
            private List<LineItem> lineItems;
            private UpdateCreatePoResponse response;
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

                purchaseOrderRepository = new PurchaseOrderRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
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

                response = new UpdateCreatePoResponse() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" };

                purchaseOrder = new PurchaseOrder("1", "number", "VendorName", PurchaseOrderStatus.InProgress, DateTime.Today, DateTime.Today)
                {
                    CurrencyCode = "USD",
                    Type = "Travel",
                    SubmittedBy = "1",
                    MaintenanceDate = DateTime.Today,
                    DeliveryDate = DateTime.Today,
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

                purchaseOrders = new PurchaseOrders()
                {
                    Recordkey = "1",
                    RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    PoStatus = new List<string>() { "A" },
                    PoStatusDate = new List<DateTime?>() { DateTime.Today },
                    PoDate = DateTime.Today,
                    PoNo = "1",
                    PoIntgType = "A",
                    PoIntgSubmittedBy = "1",
                    PoVendor = "1",
                    PoDefaultInitiator = "1",
                    PoCurrencyCode = "USD",
                    PoMaintGlTranDate = DateTime.Today,
                    PoExpectedDeliveryDate = DateTime.Today,
                    PoReqIds = new List<string>() { "1" },
                    PoVouIds = new List<string>() { "1" },
                    PoItemsId = new List<string>() { "1" },
                };
            }

            private void InitializeTestMock()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateCreatePoRequest, UpdateCreatePoResponse>(It.IsAny<UpdateCreatePoRequest>())).ReturnsAsync(response);
                dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrders);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(person);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", It.IsAny<bool>())).ReturnsAsync(intlParams);
                dataReaderMock.Setup(d => d.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS", true)).ReturnsAsync(purDefaults);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_ArgumentNullException_PurchaseOrder_Null()
            {
                await purchaseOrderRepository.CreatePurchaseOrdersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_RepositoryException()
            {
                response.UpdatePOErrors = new List<UpdatePOErrors>() { new UpdatePOErrors() { ErrorMessages = "Error", ErrorCodes = "PurchaseOrders" } };

                await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_Get_ArgumentNullException_Guid_Null()
            {
                response.Guid = null;

                await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(null);

                await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_RecordKey_Null()
            {
                purchaseOrders.Recordkey = null;

                await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_RecordGuid_Null()
            {
                purchaseOrders.RecordGuid = null;

                await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_PoStatus_Null()
            {
                purchaseOrders.PoStatus = new List<string>();

                await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_PoStatusDate_Null()
            {
                purchaseOrders.PoStatusDate = null;

                await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_PoDate_Null()
            {
                purchaseOrders.PoDate = null;

                await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            public async Task PurchaseOrdersService_CreatePurchaseOrdersAsync()
            {
                var result = await purchaseOrderRepository.CreatePurchaseOrdersAsync(purchaseOrder);

                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class PurchaseOrderRepositoryTests_PUT : BaseRepositorySetup
        {
            #region DECLARATIONS

            private PurchaseOrderRepository purchaseOrderRepository;

            private PurchaseOrder purchaseOrder;
            private DataContracts.PurchaseOrders purchaseOrders;
            private UpdateCreatePoResponse response;
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

                purchaseOrderRepository = new PurchaseOrderRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                lineItem = new LineItem("1", "desc", 10, 100, 110) { LineItemStatus = LineItemStatus.Accepted };

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

                response = new UpdateCreatePoResponse() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" };

                purchaseOrder = new PurchaseOrder("1", "1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "vendor", PurchaseOrderStatus.InProgress, DateTime.Today, DateTime.Today)
                {
                    CurrencyCode = "USD",
                    Type = "Travel",
                    SubmittedBy = "1",
                    MaintenanceDate = DateTime.Today,
                    DeliveryDate = DateTime.Today,
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

                purchaseOrder.AddLineItem(lineItem);

                purchaseOrders = new PurchaseOrders()
                {
                    Recordkey = "1",
                    RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    PoStatus = new List<string>() { "A" },
                    PoStatusDate = new List<DateTime?>() { DateTime.Today },
                    PoDate = DateTime.Today,
                    PoNo = "1",
                    PoIntgType = "A",
                    PoIntgSubmittedBy = "1",
                    PoVendor = "1",
                    PoDefaultInitiator = "1",
                    PoCurrencyCode = "USD",
                    PoMaintGlTranDate = DateTime.Today,
                    PoExpectedDeliveryDate = DateTime.Today,
                    PoReqIds = new List<string>() { "1" },
                    PoVouIds = new List<string>() { "1" },
                    PoItemsId = new List<string>() { "1" },
                    
                };
            }

            private void InitializeTestMock()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateCreatePoRequest, UpdateCreatePoResponse>(It.IsAny<UpdateCreatePoRequest>())).ReturnsAsync(response);
                dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(purchaseOrders);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(person);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", It.IsAny<bool>())).ReturnsAsync(intlParams);
                dataReaderMock.Setup(d => d.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS", true)).ReturnsAsync(purDefaults);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync_ArgumentNullException_PurchaseOrder_Null()
            {
                await purchaseOrderRepository.UpdatePurchaseOrdersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync_RepositoryException()
            {
                response.UpdatePOErrors = new List<UpdatePOErrors>() { new UpdatePOErrors() { ErrorMessages = "Error", ErrorCodes = "PurchaseOrders" } };

                await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<PurchaseOrders>(It.IsAny<string>(), true)).ReturnsAsync(null);

                await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_RecordKey_Null()
            {
                purchaseOrders.Recordkey = null;

                await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_RecordGuid_Null()
            {
                purchaseOrders.RecordGuid = null;

                await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_PoStatus_Null()
            {
                purchaseOrders.PoStatus = new List<string>();

                await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_PoStatusDate_Null()
            {
                purchaseOrders.PoStatusDate = null;

                await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync_Get_BuildPurchaseOrder_Exception_PurchaseOrder_PoDate_Null()
            {
                purchaseOrders.PoDate = null;

                await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrder);
            }

            [TestMethod]
            public async Task PurchaseOrdersService_UpdatePurchaseOrdersAsync()
            {
                var result = await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrder);

                Assert.IsNotNull(result);
            }

        }
    }
}