//Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{    
    [TestClass]
    public class ProcurementReceiptsServiceTests : GeneralLedgerCurrentUser.ProcurementReceiptsUser
    {
        private const string procurementReceiptsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string procurementReceiptsCode = "AT";
        private ICollection<PurchaseOrderReceipt> _procurementReceiptsCollection;
        IEnumerable<Domain.ColleagueFinance.Entities.ShippingMethod> _shippingMethods;
        IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> _currencyConv;
        Dtos.ProcurementReceipts procurementReciept;
        PurchaseOrder purchaseOrder;
        private ProcurementReceiptsService _procurementReceiptsService;

        private Mock<IPurchaseOrderReceiptRepository> _purchaseOrderReceiptRepositoryMock;
        private Mock<IColleagueFinanceReferenceDataRepository> _financeReferenceRepositoryMock;
        private Mock<IPurchaseOrderRepository> _purchaseOrderRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;

        protected Domain.Entities.Role viewProcurementReceiptsRole = new Domain.Entities.Role(1, "VIEW.PROCUREMENT.RECEIPTS");
        protected Domain.Entities.Role createProcurementReceiptsRole = new Domain.Entities.Role(2, "CREATE.PROCUREMENT.RECEIPT");

        string poGuid = "16f103c5-6565-49b3-9e34-8d97559ddc03";


        [TestInitialize]
        public void Initialize()
        {
            _purchaseOrderReceiptRepositoryMock = new Mock<IPurchaseOrderReceiptRepository>();
            _financeReferenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _purchaseOrderRepositoryMock = new Mock<IPurchaseOrderRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactory = new GeneralLedgerCurrentUser.ProcurementReceiptsUser();
            _roleRepositoryMock = new Mock<IRoleRepository>();

            BuildData();


            //_referenceRepositoryMock.Setup(repo => repo.GetPurchaseorderreceiptAsync(It.IsAny<bool>()))
            //    .ReturnsAsync(_procurementReceiptsCollection);

            _procurementReceiptsService = new ProcurementReceiptsService(_purchaseOrderReceiptRepositoryMock.Object, _financeReferenceRepositoryMock.Object, _purchaseOrderRepositoryMock.Object,
                _configurationRepoMock.Object, _adapterRegistryMock.Object, _currentUserFactory, _personRepositoryMock.Object, _roleRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _procurementReceiptsService = null;
            _procurementReceiptsCollection = null;
            _financeReferenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }


        #region GET GET ALL
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_PermissionsException()
        {
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>());
            await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>());
        }

        [TestMethod]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_EmptyTuple()
        {
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Tuple<IEnumerable<PurchaseOrderReceipt>, int>(new List<PurchaseOrderReceipt>(), 0));
            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>());

            Assert.IsNotNull(results);
            Assert.AreEqual(results.Item2, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_KeyNotFoundException()
        {
            _purchaseOrderRepositoryMock.Setup(i => i.GetGuidFromIdAsync(It.IsAny<string>(), "PURCHASE.ORDERS")).ReturnsAsync(null);

            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_ShippingMethods_Null()
        {
            _financeReferenceRepositoryMock.Setup(i => i.GetShippingMethodsAsync(It.IsAny<bool>())).ReturnsAsync(null);
            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_ShippingMethods_InvalidId()
        {
            _procurementReceiptsCollection.First().ArrivedVia = "WrongShippedVia";
            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_ReceivedByGuid()
        {
            _procurementReceiptsCollection.First().ReceivedBy = "WrongReceivedBy";
            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>());
        }

        [TestMethod]     
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync()
        {
            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Item2);
        }

        [TestMethod]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_ValidFilter()
        {
           var filter = new ProcurementReceipts()
            {
                PurchaseOrder = new GuidObject2("90c7d97e-8564-49c9-b020-a0fd635660f5")
            };
            
            _purchaseOrderRepositoryMock.Setup(x => x.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder.Id);

            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Item2);
        }


        [TestMethod]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_InvalidFilter()
        {
            var filter = new ProcurementReceipts()
            {
                PurchaseOrder = new GuidObject2("90c7d97e-8564-49c9-b020-a0fd635660f5")
            };
            
            _purchaseOrderRepositoryMock.Setup(x => x.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsAsync_FilterException()
        {
            var filter = new ProcurementReceipts()
            {
                PurchaseOrder = new GuidObject2("90c7d97e-8564-49c9-b020-a0fd635660f5")
            };

            _purchaseOrderRepositoryMock.Setup(x => x.GetPurchaseOrdersIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

            var results = await _procurementReceiptsService.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }


        [TestMethod]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsByGuidAsync()
        {
            var result = await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", It.IsAny<bool>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsByGuidAsync_ArgumentNullException()
        {
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptByGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(null);

            var results = await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
        }


        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsByGuidAsync_KeyNotFoundException()
        {
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            var result = await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsByGuidAsync_InvalidOperationException()
        {
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptByGuidAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
            var result = await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsByGuidAsync_RepositoryException()
        {
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            var result = await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsByGuidAsync_ArgumentException()
        {
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
            var result = await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_GetProcurementReceiptsByGuidAsync_Exception()
        {
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", It.IsAny<bool>());
        }

        #endregion

        #region CREATE

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Null_Input_Parameter()
        {
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Null_DotId()
        {
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(new ProcurementReceipts() { Id = string.Empty });
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_PermissionsException()
        {
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewProcurementReceiptsRole });
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(new ProcurementReceipts() { Id = "123" });
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Null_PO()
        {
            procurementReciept.PurchaseOrder = null;
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Null_POId()
        {
            procurementReciept.PurchaseOrder.Id = string.Empty;
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Null_LineItem()
        {
            procurementReciept.LineItems = null;
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Empty_LineItem()
        {
            procurementReciept.LineItems = new List<ProcurementReceiptsLineItems>();
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Null_ReceivedBy()
        {
            procurementReciept.ReceivedBy = null;
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Null_ReceivedBy_Id()
        {
            procurementReciept.ReceivedBy = new GuidObject2();
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_PO_Null()
        {
            _purchaseOrderRepositoryMock.Setup(i => i.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_POStatus_InProgress()
        {
            purchaseOrder = new PurchaseOrder("123", "123", "Vendor", PurchaseOrderStatus.InProgress, DateTime.Today, DateTime.Today)
            {
                HostCountry = "USA"
            };
            _purchaseOrderRepositoryMock.Setup(i => i.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_POStatus_NotApproved()
        {
            purchaseOrder = new PurchaseOrder("123", "123", "Vendor", PurchaseOrderStatus.NotApproved, DateTime.Today, DateTime.Today)
            {
                HostCountry = "USA"
            };
            _purchaseOrderRepositoryMock.Setup(i => i.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_POStatus_OutstandingItemsId_Null()
        {
            purchaseOrder = new PurchaseOrder("123", "123", "Vendor", PurchaseOrderStatus.Accepted, DateTime.Today, DateTime.Today)
            {
                HostCountry = "USA",
                OutstandingItemsId = null
            };
            _purchaseOrderRepositoryMock.Setup(i => i.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_POStatus_OutstandingItemsId_NotAny()
        {
            purchaseOrder = new PurchaseOrder("123", "123", "Vendor", PurchaseOrderStatus.Accepted, DateTime.Today, DateTime.Today)
            {
                HostCountry = "USA",
                OutstandingItemsId = new List<string>()
            };
            _purchaseOrderRepositoryMock.Setup(i => i.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_POStatus_AcceptedItemsId_Null()
        {
            purchaseOrder = new PurchaseOrder("123", "123", "Vendor", PurchaseOrderStatus.Accepted, DateTime.Today, DateTime.Today)
            {
                HostCountry = "USA",
                OutstandingItemsId = new List<string>() { "1"}
            };
            _purchaseOrderRepositoryMock.Setup(i => i.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_POStatus_AcceptedItemsId_NotAny()
        {
            purchaseOrder = new PurchaseOrder("123", "123", "Vendor", PurchaseOrderStatus.Accepted, DateTime.Today, DateTime.Today)
            {
                HostCountry = "USA",
                OutstandingItemsId = new List<string>() { "1" },
                AcceptedItemsId = new List<string>()
            };
            _purchaseOrderRepositoryMock.Setup(i => i.GetPurchaseOrdersByGuidAsync(It.IsAny<string>())).ReturnsAsync(purchaseOrder);
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_PO_LineItems_Empty()
        {
            procurementReciept.LineItems.First().LineItemNumber = string.Empty;
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_LineItemReceived_Empty()
        {
            procurementReciept.LineItems.First().Received = null;
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_LineItem_Noquantity()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty();
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_LineItem_NoCost()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty() { Quantity = 1 };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_LineItem_CostNotNull()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Quantity = 1,
                Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Value = 5
                }
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_LineItem_CostNull()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                   
                }
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_CurrencyMismatch()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Value = 5,
                    Currency = Dtos.EnumProperties.CurrencyIsoCode.CAD
                }
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Rejected_Null()
        {
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty() { };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Rejected_VS_Received()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10.00m },
                Quantity = 1
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10.00m },
                Quantity = -1
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Rejected_GT_Received()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10.00m },
                Quantity = 1
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10.00m },
                Quantity = 2
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_RejectedQT_VS_ReceivedCost()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = -1.00m },
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 10.00m },
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_RejectedQT_VS_ReceivedQT()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = null,
                Quantity = -1
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = null,
                Quantity = 1
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_RejectedQT_GT_ReceivedQT()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = null,
                Quantity = 1
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = null,
                Quantity = 2
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_RejectedQT_GT_ReceivedCost()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Value = -1,
                    Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                }
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Quantity = 1
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Rejected_QT_Rejected_Null()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = null,
                Quantity = 1
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = null
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Rejected_CostNull_QtHasValue()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = null,
                Quantity = 1
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty() { },
                Quantity = 1
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Rejected_CostNotNull_QtHasValue()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
               Quantity = -1
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                    Value = 1
                },
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_ReceivedCostNotNull()
        {
            procurementReciept.LineItems.First().Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                    Value = 1
                }
            };
            procurementReciept.LineItems.First().Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
            {
                Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                    Value = -1
                },
            };
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_ShippingMethod_Null()
        {
            _financeReferenceRepositoryMock.Setup(i => i.GetShippingMethodsAsync(It.IsAny<bool>())).ReturnsAsync(null);
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_ShippingMethod_BadGuid()
        {
            procurementReciept.ShippingMethod = new GuidObject2("BadGuid");
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_Person_BadId()
        {
            procurementReciept.ReceivedBy = new GuidObject2("BadGuid");
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);            
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync_RepositoryException()
        {
            _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.CreatePurchaseOrderReceiptAsync(It.IsAny<PurchaseOrderReceipt>())).ThrowsAsync(new RepositoryException());
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ProcurementReceiptsService_CreateProcurementReceiptsAsync()
        {
            _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            _purchaseOrderReceiptRepositoryMock.Setup(i => i.CreatePurchaseOrderReceiptAsync(It.IsAny<PurchaseOrderReceipt>())).ReturnsAsync(new PurchaseOrderReceipt("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"));
            var result = await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReciept);
            Assert.IsNotNull(result);
        }

        #endregion


        #region Build Data

        private void BuildData()
        {
            //Entity
            _procurementReceiptsCollection = new List<PurchaseOrderReceipt>()
                {
                    new PurchaseOrderReceipt("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                    {
                        ArrivedVia = "Fedex",
                        PackingSlip = "1234",
                        PoId = "123",
                        ReceiptItems = new List<PurchaseOrderReceiptItem>()
                        {
                            new PurchaseOrderReceiptItem("1")
                            {
                                ReceivedAmt = 100.00m,
                                ReceivedAmtCurrency = "USD",
                                ReceivedQty = 1,
                                ReceivingComments = "ReceivingComments 1",
                                RejectedAmt = 0,
                                RejectedAmtCurrency = "USD",
                                RejectedQty = 1
                            }
                        },
                        ReceivedBy = "1",
                        ReceivedDate = DateTime.Today,
                        ReceivingComments = "ReceivingComments 2"
                    }
                };
            //DTO
            procurementReciept = new Dtos.ProcurementReceipts()
            {
                Id = Guid.Empty.ToString(),
                Comment = "Comment 1",
                LineItems = new List<ProcurementReceiptsLineItems>()
                {
                    new ProcurementReceiptsLineItems()
                    {
                        Comment = "ProcurementReceiptsLineItems comment",
                        LineItemNumber = "1",
                        Received = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
                        {
                           Cost = null,
                            Quantity = 1
                        },
                        Rejected = new Dtos.DtoProperties.QuantityAmount2DtoProperty()
                        {
                            Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                            {
                               Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                               Value = 10.00m
                            }
                        }
                    }
                },
                PurchaseOrder = new GuidObject2("90c7d97e-8564-49c9-b020-a0fd635660f5"),
                PackingSlipNumber = "1234",
                ReceivedBy = new GuidObject2("3fb64216-c45e-4f6e-b615-b287d58e4fb6"),
                ReceivedOn = DateTime.Today,
                ShippingMethod = new GuidObject2("355c0cbc-8377-4b56-bbe2-26ad4ba6507e")
            };

            //DTO
            purchaseOrder = new PurchaseOrder("123", "123", "Vendor", PurchaseOrderStatus.Accepted, DateTime.Today, DateTime.Today)
            {
                CurrencyCode = "USD",
                HostCountry = "USA",
                AcceptedItemsId = new List<string>() { "1" },
                OutstandingItemsId = new List<string>() { "1" }                
            };
            purchaseOrder.AddLineItem(new LineItem("1", "Descr", 1, 100, 100));
            _purchaseOrderRepositoryMock.Setup(i => i.GetPurchaseOrdersByGuidAsync("90c7d97e-8564-49c9-b020-a0fd635660f5")).ReturnsAsync(purchaseOrder);

            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
               .ReturnsAsync(new Tuple<IEnumerable<PurchaseOrderReceipt>, int>(_procurementReceiptsCollection, _procurementReceiptsCollection.Count()));

            _purchaseOrderReceiptRepositoryMock.Setup(i => i.GetPurchaseOrderReceiptByGuidAsync(It.IsAny<string>())).ReturnsAsync(_procurementReceiptsCollection.First());


            viewProcurementReceiptsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewProcurementReceipts));
            createProcurementReceiptsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.CreateProcurementReceipts));
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewProcurementReceiptsRole, createProcurementReceiptsRole });

            Dictionary<string, string> personDict = new Dictionary<string, string>();
            personDict.Add("1", "3fb64216-c45e-4f6e-b615-b287d58e4fb6");
            _personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personDict);

            _purchaseOrderRepositoryMock.Setup(i => i.GetGuidFromIdAsync(It.IsAny<string>(), "PURCHASE.ORDERS")).ReturnsAsync(poGuid);

            _shippingMethods = new List<Domain.ColleagueFinance.Entities.ShippingMethod>()
            {
                new ShippingMethod("355c0cbc-8377-4b56-bbe2-26ad4ba6507e", "Fedex", "Descr 1"),
                new ShippingMethod("c97c91eb-cb42-4b01-b607-17d512327110", "Ups", "Descr 2")
            };
            _financeReferenceRepositoryMock.Setup(i => i.GetShippingMethodsAsync(It.IsAny<bool>())).ReturnsAsync(_shippingMethods);

            _currencyConv = new List<Domain.ColleagueFinance.Entities.CurrencyConversion>()
            {
                new CurrencyConversion("USD", "USD") { CurrencyCode = CurrencyCodes.USD },
                new CurrencyConversion("CAN", "CAN"){ CurrencyCode = CurrencyCodes.CAD }
            };
            _financeReferenceRepositoryMock.Setup(i => i.GetCurrencyConversionAsync()).ReturnsAsync(_currencyConv);

        }

        #endregion
    }
}