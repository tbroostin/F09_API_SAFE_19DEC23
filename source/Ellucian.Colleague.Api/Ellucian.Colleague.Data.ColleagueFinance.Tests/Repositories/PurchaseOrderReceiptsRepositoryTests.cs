using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class PurchaseOrderReceiptRepositoryTests_V11
    {
        [TestClass]
        public class PurchaseOrderReceiptRepositoryTests_GET : BaseRepositorySetup
        {
            #region DECLARATIONS

            private PurchaseOrderReceiptRepository PurchaseOrderReceiptRepository;

            private Dictionary<string, RecordKeyLookupResult> lookupResult;
            private Dictionary<string, GuidLookupResult> dicResult;

            private Base.DataContracts.Defaults defaults;
            private Base.DataContracts.Corp corp;

            private Collection<DataContracts.PoReceiptIntg> poReceiptIntgs;
            private Collection<DataContracts.PoReceiptItemIntg> poReceiptItemIntgs;
            private CreateProcurementReceiptResponse response;
            private IntlParams intlParams;
            private PurchaseOrderReceipt purchaseOrderReceipt;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                PurchaseOrderReceiptRepository = new PurchaseOrderReceiptRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                response = new CreateProcurementReceiptResponse()
                {
                    PriGuid = guid,
                    PoReceiptIntgId = "1"
                };

                intlParams = new IntlParams()
                {
                    HostCountry = "USA",
                    HostShortDateFormat = "DMY",
                    HostDateDelimiter = "-"
                };

                poReceiptIntgs = new Collection<PoReceiptIntg>()
                {
                    new PoReceiptIntg() { PriPoId = "612",
                        Recordkey = "1", PriPackingSlip = "SLIP1",
                        PriReceivedDate = DateTime.Now,
                        PriReceivedBy = "0015600",
                        PriArrivedVia = "FX",
                        PriReceivingComments = "Hello World",
                        RecordGuid = guid, 
                        PriItems = new List<string>() { "100"}
                        
                    }
                };

                poReceiptItemIntgs = new Collection<PoReceiptItemIntg>()
                {
                    new PoReceiptItemIntg()
                    {
                        Recordkey = "100",
                        PriiItemsId = "11",
                        PriiReceivedQty = 3,
                        PriiReceivedAmt = 5,
                        PriiReceivedAmtCurrency = "USD",
                        PriiReceivingComments = "Hello Comment",
                        PriiRejectedAmt = 6,
                        PriiRejectedAmtCurrency = "USD",
                        PriiRejectedQty = 4
                    }
                };

                var poReceiptIntg = poReceiptIntgs.FirstOrDefault(x => x.RecordGuid == guid);
                var poReceiptIntgItem = poReceiptItemIntgs.FirstOrDefault(x=>x.Recordkey == "100");
                purchaseOrderReceipt = new PurchaseOrderReceipt(poReceiptIntg.RecordGuid)
                {
                    ArrivedVia = poReceiptIntg.PriArrivedVia,
                    PackingSlip = poReceiptIntg.PriPackingSlip,
                    PoId = poReceiptIntg.PriPoId,
                    ReceivingComments = poReceiptIntg.PriReceivingComments,
                    ReceivedBy = poReceiptIntg.PriReceivedBy,
                    ReceivedDate = poReceiptIntg.PriReceivedDate,
                    Recordkey = poReceiptIntg.Recordkey,
                    ReceiptItems = new List<PurchaseOrderReceiptItem>()
                    {
                        new PurchaseOrderReceiptItem(poReceiptIntgItem.Recordkey)
                        {
                               ReceivedAmt = poReceiptIntgItem.PriiReceivedAmt,
                               ReceivedAmtCurrency = poReceiptIntgItem.PriiReceivedAmtCurrency,
                               ReceivedQty = poReceiptIntgItem.PriiReceivedQty,
                               ReceivingComments = poReceiptIntgItem.PriiReceivingComments,
                               RejectedAmt = poReceiptIntgItem.PriiRejectedAmt,
                               RejectedAmtCurrency = poReceiptIntgItem.PriiRejectedAmtCurrency
                        }
                    }
                };

                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "PO.RECEIPT.INTG", PrimaryKey = "1" } }
                };

                lookupResult = new Dictionary<string, RecordKeyLookupResult>()
                {
                    { string.Join("+", new string[] { "PERSON", guid }), new RecordKeyLookupResult() { Guid = guid } }
                };

                defaults = new Base.DataContracts.Defaults() { DefaultHostCorpId = "1" };

                corp = new Base.DataContracts.Corp() { CorpName = new List<string>() { "Name" } };

            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2" });
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(lookupResult);
                dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(d => d.ReadRecordAsync<PoReceiptIntg>(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(poReceiptIntgs.FirstOrDefault(x => x.RecordGuid == guid));
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.PoReceiptItemIntg>("PO.RECEIPT.ITEM.INTG", It.IsAny<string[]>(), It.IsAny<bool>()))
                     .ReturnsAsync(poReceiptItemIntgs);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<PoReceiptIntg>("PO.RECEIPT.INTG", It.IsAny<string[]>(), It.IsAny<bool>()))
                     .ReturnsAsync(poReceiptIntgs);
                dataReaderMock.Setup(d => d.SelectAsync("PO.RECEIPT.INTG", "")).ReturnsAsync(new string[] { "1", "2" });
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PurchaseOrderReceiptRepository_Empty_PoReceiptIntg_From_Repository()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<PoReceiptIntg>("PO.RECEIPT.INTG", It.IsAny<string[]>(), true)).ReturnsAsync(null);

                await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptsAsync(0, 2);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PurchaseOrderReceiptRepositoryGetPurchaseOrderReceipts_PoReceiptIntg_Null_From_Repository()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<PoReceiptIntg>("PO.RECEIPT.INTG", It.IsAny<string[]>(), true)).ReturnsAsync(null);

                await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptsAsync(0, 2);
            }

            [TestMethod]
            public async Task PurchaseOrderReceiptRepository_GetPurchaseOrderReceiptsAsync()
            {
                var result = await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptsAsync(0, 2);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrderReceiptRepository_GetPurchaseOrderReceiptByGuidAsync_Empty_Guid()
            {
                await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrderReceiptRepository_GetPurchaseOrderReceiptByGuidAsync_RecordKey_NotFound()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PurchaseOrderReceiptRepository_GetPurchaseOrderReceiptByGuidAsync_Record_NotFound()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<PoReceiptIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                //dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);

                await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrderReceiptRepository_GetPurchaseOrderReceiptByGuidAsync_EmptyGuid_FromRepo()
            {
                var poReceiptIntg = poReceiptIntgs.FirstOrDefault(x => x.RecordGuid == guid);
                poReceiptIntg.RecordGuid = null;
                dataReaderMock.Setup(d => d.ReadRecordAsync<PoReceiptIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(poReceiptIntg);
                await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptByGuidAsync(guid);
            }

            [TestMethod]
            public async Task PurchaseOrderReceiptRepository_GetPurchaseOrderReceiptByGuidAsync()
            {

                var result = await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptByGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrderReceiptRepository_GetPurchaseOrderReceiptByGuidAsync_NullGuid()
            {
                var poReceiptIntg = poReceiptIntgs.FirstOrDefault();
                poReceiptIntg.RecordGuid = null;
                dataReaderMock.Setup(d => d.ReadRecordAsync<PoReceiptIntg>(It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync(poReceiptIntg);

                var result = await PurchaseOrderReceiptRepository.GetPurchaseOrderReceiptByGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }

            [TestMethod]
            public async Task PurchaseOrderReceiptRepository_CreatePurchaseOrderReceipt()
            {
                transManagerMock.Setup(repo => repo.ExecuteAsync<CreateProcurementReceiptRequest, CreateProcurementReceiptResponse>(It.IsAny<CreateProcurementReceiptRequest>()))
                    .ReturnsAsync(response);
                var actual = await PurchaseOrderReceiptRepository.CreatePurchaseOrderReceiptAsync(purchaseOrderReceipt);
                Assert.IsNotNull(actual);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrderReceiptRepository_CreatePurchaseOrderReceipt_NullArgument()
            {
                await PurchaseOrderReceiptRepository.CreatePurchaseOrderReceiptAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PurchaseOrderReceiptRepository_CreatePurchaseOrderReceipt_Empty_FromRepo()
            {
                response = new CreateProcurementReceiptResponse()
                {
                    PriGuid = null
                };
                transManagerMock.Setup(repo => repo.ExecuteAsync<CreateProcurementReceiptRequest, CreateProcurementReceiptResponse>(It.IsAny<CreateProcurementReceiptRequest>()))
                    .ReturnsAsync(response);
                var actual = await PurchaseOrderReceiptRepository.CreatePurchaseOrderReceiptAsync(purchaseOrderReceipt);
                Assert.IsNotNull(actual);
            }

            [TestMethod]
            public async Task PurchaseOrderReceiptRepository_CreatePurchaseOrderReceipt_Warning()
            {
                response = new CreateProcurementReceiptResponse()
                {
                    PriGuid = guid,
                    CreateProcurementReceiptWarnings = new List<CreateProcurementReceiptWarnings>()
                    {
                        new CreateProcurementReceiptWarnings() { WarningCodes = "1", WarningMessages = "Warning" }
                    }
                };
                transManagerMock.Setup(repo => repo.ExecuteAsync<CreateProcurementReceiptRequest, CreateProcurementReceiptResponse>(It.IsAny<CreateProcurementReceiptRequest>()))
                    .ReturnsAsync(response);
                var actual = await PurchaseOrderReceiptRepository.CreatePurchaseOrderReceiptAsync(purchaseOrderReceipt);
                Assert.IsNotNull(actual);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PurchaseOrderReceiptRepository_CreatePurchaseOrderReceipt_Error()
            {
                response = new CreateProcurementReceiptResponse()
                {
                    CreateProcurementReceiptErrors = new List<CreateProcurementReceiptErrors>()
                    {
                        new CreateProcurementReceiptErrors() { ErrorCodes = "1", ErrorMessages = "Error" }
                    }
                };
                transManagerMock.Setup(repo => repo.ExecuteAsync<CreateProcurementReceiptRequest, CreateProcurementReceiptResponse>(It.IsAny<CreateProcurementReceiptRequest>()))
                    .ReturnsAsync(response);
                var actual = await PurchaseOrderReceiptRepository.CreatePurchaseOrderReceiptAsync(purchaseOrderReceipt);
                Assert.IsNotNull(actual);
            }
        }
    }
}