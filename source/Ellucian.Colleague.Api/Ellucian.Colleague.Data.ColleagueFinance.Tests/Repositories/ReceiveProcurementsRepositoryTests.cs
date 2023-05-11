using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class ReceiveProcurementsRepositoryTests : BaseRepositorySetup
    {
        #region DECLARATIONS

        private CfwebDefaults cfwebDefaults;
        private Collection<PurchaseOrders> purchaseOrders;
        private Collection<Items> items;
        private Collection<CommodityCodes> commodityCode;
        private ReceiveProcurementsRepository receiveProcurementsRepository;

        private GetVendorInformationResponse getVendorInformationResponse;

        private ProcurementAcceptReturnItemInformationRequest procurementAcceptReturnItemInformationRequest;
        private TxReceiveReturnProcurementItemsResponse txReceiveReturnProcurementItemsResponse;

        private string personId = "0000143";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            InitializeTestData();

            InitializeTestMock();

            receiveProcurementsRepository = new ReceiveProcurementsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();

            receiveProcurementsRepository = null;

        }

        private void InitializeTestData()
        {

            cfwebDefaults = new CfwebDefaults() { Recordkey = "1", CfwebFixedAssetAmt = decimal.Parse("10.000") };

            purchaseOrders = new Collection<PurchaseOrders>() {
                new PurchaseOrders() { Recordkey="1", PoNo = "P001", PoMiscName = new List<string>() { "Ven_Misc_Name_1", "Ven_Misc_Name_2" }, PoRequestor ="0000143", PoDefaultInitiator ="0000143", PoReqIds = new List<string>() { "001","002", "003" }, PoItemsId = new List<string>() { "111","112","113" }  },
                new PurchaseOrders() { Recordkey="2", PoNo = "P002", PoVendor="0000123", PoRequestor ="0000143", PoDefaultInitiator ="0000143", PoReqIds = new List<string>() { "011","012", "013" }, PoItemsId = new List<string>() { "121","122","123" }, },
                new PurchaseOrders() { Recordkey="3", PoNo = "P003", PoVendor="0000123", PoReqIds = new List<string>() { "011","012", "013" }, PoItemsId = new List<string>() { "121","122","123" }, },
                new PurchaseOrders() { Recordkey="4", PoNo = "P004", PoMiscName = new List<string>() { "Ven_Misc_Name_1", "Ven_Misc_Name_2" }, PoReqIds = new List<string>() { "011","012", "013" }, PoItemsId = new List<string>() { "121","122","123" }, }
            };

            items = new Collection<Items>() { new Items() { Recordkey = "111", ItmDesc = new List<string>() { "Itm_Desc_1", "Itm_Desc_2" }, ItmFixedAssetsFlag = "S", ItemPoStatusEntityAssociation = new List<ItemsItemPoStatus>() { new ItemsItemPoStatus() { ItmPoStatusAssocMember = "B" }, new ItemsItemPoStatus() { ItmPoStatusAssocMember = "B" } }, ItmPoExtPrice = decimal.Parse("10.000"), ItmPoPrice = decimal.Parse("10.000"), ItmInitiator = "0000143", ItmVendorPart = "Itm_Name_1", ItmPoQty = decimal.Parse("10.000"), ItmCommodityCode = "100" },
                                              new Items() { Recordkey = "112", ItmDesc = new List<string>() { "Itm_Desc_1", "Itm_Desc_2" }, ItmFixedAssetsFlag = "S", ItemPoStatusEntityAssociation = new List<ItemsItemPoStatus>() { new ItemsItemPoStatus() { ItmPoStatusAssocMember = "H" }, new ItemsItemPoStatus() { ItmPoStatusAssocMember = "H" } }, ItmPoExtPrice = decimal.Parse("10.000"), ItmPoPrice = decimal.Parse("10.000"), ItmInitiator = "0000143", ItmVendorPart = "Itm_Name_1", ItmPoQty = decimal.Parse("10.000"), ItmCommodityCode = "101" },
                                              new Items() { Recordkey = "113", ItmDesc = new List<string>() { "Itm_Desc_1", "Itm_Desc_2" }, ItmFixedAssetsFlag = "M", ItemPoStatusEntityAssociation = new List<ItemsItemPoStatus>() { new ItemsItemPoStatus() { ItmPoStatusAssocMember = "O" }, new ItemsItemPoStatus() { ItmPoStatusAssocMember = "O" } }, ItmPoExtPrice = decimal.Parse("10.000"), ItmPoPrice = decimal.Parse("10.000"), ItmInitiator = "0000143", ItmVendorPart = "Itm_Name_1", ItmPoQty = decimal.Parse("10.000") },
                                              new Items() { Recordkey = "114", ItmDesc = new List<string>() { "Itm_Desc_1", "Itm_Desc_2" }, ItmFixedAssetsFlag = "", ItemPoStatusEntityAssociation = new List<ItemsItemPoStatus>() { new ItemsItemPoStatus() { ItmPoStatusAssocMember = "O" }, new ItemsItemPoStatus() { ItmPoStatusAssocMember = "O" } }, ItmPoExtPrice = decimal.Parse("10.000"), ItmPoPrice = decimal.Parse("10.000"), ItmInitiator = "0000143", ItmVendorPart = "Itm_Name_1", ItmPoQty = decimal.Parse("10.000") },
                                              new Items() { Recordkey = "1241", ItmDesc = new List<string>() { "Itm_Desc_1", "Itm_Desc_2" }, ItmFixedAssetsFlag = "", ItemPoStatusEntityAssociation = new List<ItemsItemPoStatus>() { new ItemsItemPoStatus() { ItmPoStatusAssocMember = "O" }, new ItemsItemPoStatus() { ItmPoStatusAssocMember = "O" } }, ItmPoExtPrice = decimal.Parse("10.000"), ItmPoPrice = decimal.Parse("10.000"), ItmInitiator = "0000143", ItmVendorPart = "Itm_Name_1", ItmPoQty = decimal.Parse("10.000") }
            };

            commodityCode = new Collection<CommodityCodes>() { new CommodityCodes() { Recordkey="100", CmdtyMsdsFlag = "Y" },
                new CommodityCodes() { Recordkey="101", CmdtyMsdsFlag = "N" }
            };

            getVendorInformationResponse = new GetVendorInformationResponse()
            {
                VendorIds = { "0000123", "0000124" },
                VendorContactSummary = new List<Transactions.VendorContactSummary>() {
                    new Transactions.VendorContactSummary() { ContactVendorId = "0000123", VendorContactEmail ="Abc@mail.com", VendorContactName = "joe", VendorContactPhone="897123", VendorContactTitle="Mr." } },
                Error = "",
                ErrorMessages = { },
                VendorInfo = new List<Transactions.VendorInfo>() {
                        new Transactions.VendorInfo() { VendorId="0000123", VendorAddress="address1", VendorCity="city", VendorCountry="country", VendorName="vendorName", VendorState="state", VendorZip="576231" },
                        new Transactions.VendorInfo() { VendorId="0000124", VendorAddress="address1", VendorCity="city", VendorCountry="country", VendorName="vendorName", VendorState="state", VendorZip="576231" } }
            };

            procurementAcceptReturnItemInformationRequest = new ProcurementAcceptReturnItemInformationRequest()
            {
                AcceptAll = false,
                ArrivedVia = "FX",
                IsPoFilterApplied = false,
                PackingSlip = "",
                StaffUserId = personId,
                ProcurementItemsInformation = new List<ProcurementItemInformation>() {
                new ProcurementItemInformation("001","P001","001",decimal.Parse("10.000"),decimal.Parse("10.000"),null, null,"") { ItemDescription="Item1", ItemMsdsFlag=false, ItemMsdsReceived=false, Vendor="Vendor1"  },
                new ProcurementItemInformation("002","P002","002",decimal.Parse("2.000"),decimal.Parse("2.000"),null, null,"") { ItemDescription="Item2", ItemMsdsFlag=false, ItemMsdsReceived=false, Vendor="Vendor2"  }
                }
            };

            txReceiveReturnProcurementItemsResponse = new TxReceiveReturnProcurementItemsResponse()
            {
                ErrorOccurred = false,
                ErrorMessages = new List<string>() { "" },
                WarningOccurred = false,
                WarningMessages = new List<string>() { "" },
                ItemInformation = new List<ItemInformation>() {
                 new ItemInformation() { PoId = "001", PoNumber="P001", ItemId ="001", ItemDesc = "Item1", ItemMsdsFlag=false, ItemMsdsReceived=false, QuantityOrdered= "10.000", QuantityAccepted="10.000"  },
                 new ItemInformation() { PoId = "002", PoNumber="P002", ItemId ="002", ItemDesc = "Item2", ItemMsdsFlag=false, ItemMsdsReceived=false, QuantityOrdered= "2.000", QuantityAccepted="2.000"  }
                }
            };
        }

        private void InitializeTestMock()
        {
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(purchaseOrders);
            dataReaderMock.Setup(d => d.ReadRecordAsync<CfwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(cfwebDefaults);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Items>(It.IsAny<string[]>(),true)).ReturnsAsync(items);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<CommodityCodes>(It.IsAny<string[]>(), true)).ReturnsAsync(commodityCode);
            dataReaderMock.Setup(d => d.SelectAsync("PURCHASE.ORDERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2" }.ToArray<string>());
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2" }.ToArray<string>());
            dataReaderMock.Setup(d => d.SelectAsync("REQUISITIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "011", "012" }.ToArray<string>());
            dataReaderMock.Setup(d => d.SelectAsync("ITEMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "121", "122","123" }.ToArray<string>());
            transManagerMock.Setup(t => t.ExecuteAsync<GetVendorInformationRequest, GetVendorInformationResponse>(It.IsAny<GetVendorInformationRequest>())).ReturnsAsync(getVendorInformationResponse);
            transManagerMock.Setup(t => t.ExecuteAsync<TxReceiveReturnProcurementItemsRequest, TxReceiveReturnProcurementItemsResponse>(It.IsAny<TxReceiveReturnProcurementItemsRequest>())).ReturnsAsync(txReceiveReturnProcurementItemsResponse);
        }

        #endregion

        #region TEST METHODS

        #region GET

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsRepository_GetReceiveProcurementsByPersonIdAsync_PersonId_As_Null()
        {
            await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync(null);
            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsRepository_GetReceiveProcurementsByPersonIdAsync_PersonId_As_Empty()
        {
            await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync("");

        }

        [TestMethod]
        public async Task ReceiveProcurementsRepository_GetReceiveProcurementsByPersonIdAsync_WithNoPO()
        {
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<PurchaseOrders>() { });
            var result = await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync(personId);
            Assert.AreEqual(result.Count(), 0);
        }

        [TestMethod]
        public async Task ReceiveProcurementsRepository_GetReceiveProcurementsByPersonIdAsync_WithNoPO_Requisitions_Items()
        {
            dataReaderMock.Setup(d => d.SelectAsync("PURCHASE.ORDERS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { }.ToArray<string>());
            dataReaderMock.Setup(d => d.SelectAsync("REQUISITIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() {  }.ToArray<string>());
            dataReaderMock.Setup(d => d.SelectAsync("ITEMS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() {  }.ToArray<string>());
            var result = await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync(personId);
            Assert.AreEqual(result.Count(), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ReceiveProcurementsRepository_GetReceiveProcurementsByPersonIdAsync_InvalidOperationfor_VendorInfo()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<GetVendorInformationRequest, GetVendorInformationResponse>(It.IsAny<GetVendorInformationRequest>())).ReturnsAsync(() => null);
            await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ReceiveProcurementsRepository_GetReceiveProcurementsByPersonIdAsync_VendorInfo_WithError()
        {
            getVendorInformationResponse.ErrorMessages = new List<string>() { "Error1", "Error2" };
            transManagerMock.Setup(t => t.ExecuteAsync<GetVendorInformationRequest, GetVendorInformationResponse>(It.IsAny<GetVendorInformationRequest>())).ReturnsAsync(getVendorInformationResponse);
            await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync(personId);
        }

        [TestMethod]
        public async Task ReceiveProcurementsRepository_GetReceiveProcurementsByPersonIdAsync()
        {
            var result = await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync(personId);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count(), 3);
        }

        [TestMethod]
        public async Task ReceiveProcurementsRepository_GetReceiveProcurementsByPersonIdAsync_WithNoRequisitions()
        {
            dataReaderMock.Setup(d => d.SelectAsync("REQUISITIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { }.ToArray<string>());
            var result = await receiveProcurementsRepository.GetReceiveProcurementsByPersonIdAsync(personId);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count(), 3);
        }

        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReceiveProcurementsRepository_AcceptOrReturnProcurementItemsAsync_AcceptReturnRequest_Null()
        {
            await receiveProcurementsRepository.AcceptOrReturnProcurementItemsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ReceiveProcurementsRepository_AcceptOrReturnProcurementItemsAsync_ThrowsException()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<TxReceiveReturnProcurementItemsRequest, TxReceiveReturnProcurementItemsResponse>(It.IsAny<TxReceiveReturnProcurementItemsRequest>())).ThrowsAsync(new Exception());

            await receiveProcurementsRepository.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequest);
        }

        [TestMethod]
        public async Task ReceiveProcurementsRepository_AcceptOrReturnProcurementItemsAsync_WithError()
        {
            txReceiveReturnProcurementItemsResponse.ErrorOccurred = true;
            txReceiveReturnProcurementItemsResponse.ErrorMessages = new List<string>() { "Error1" };

            var result = await receiveProcurementsRepository.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequest);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ReceiveProcurementsRepository_AcceptOrReturnProcurementItemsAsync_WithWarning()
        {
            txReceiveReturnProcurementItemsResponse.ErrorOccurred = false;
            txReceiveReturnProcurementItemsResponse.ErrorMessages = null;
            txReceiveReturnProcurementItemsResponse.WarningOccurred = true;
            txReceiveReturnProcurementItemsResponse.WarningMessages = new List<string>() { "Warning1" };

            var result = await receiveProcurementsRepository.AcceptOrReturnProcurementItemsAsync(procurementAcceptReturnItemInformationRequest);
            Assert.IsNotNull(result);
        }

        #endregion

        #endregion

    }
}
