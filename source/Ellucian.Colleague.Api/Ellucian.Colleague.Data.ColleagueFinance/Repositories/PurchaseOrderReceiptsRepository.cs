// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    ///  This class implements the IPurchaseOrderReceiptRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PurchaseOrderReceiptRepository : BaseColleagueRepository, IPurchaseOrderReceiptRepository
    {
        /// <summary>
        /// The constructor to instantiate a purchase order repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        /// 

        private RepositoryException exception = new RepositoryException();
        public PurchaseOrderReceiptRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Create a purchaseOrderReceipt
        /// </summary>
        /// <param name="purchaseOrderReceiptsEntity">PurchaseOrderReceipt domain entity</param>
        /// <returns>new PurchaseOrderReceipt domain entity</returns>
        public async Task<PurchaseOrderReceipt> CreatePurchaseOrderReceiptAsync(PurchaseOrderReceipt purchaseOrderReceiptsEntity)
        {
            if (purchaseOrderReceiptsEntity == null)
                throw new ArgumentNullException("purchaseOrderReceiptsEntity", "Must provide a purchaseOrderReceiptEntity to create.");

            var createRequest = BuildPurchaseOrderReceiptUpdateRequest(purchaseOrderReceiptsEntity);
            createRequest.PriGuid = null;
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<CreateProcurementReceiptRequest, CreateProcurementReceiptResponse>(createRequest);

            if (createResponse.CreateProcurementReceiptErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred creating purchaseOrderReceipt '{0}':", purchaseOrderReceiptsEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.CreateProcurementReceiptErrors.ForEach(e => exception.AddError(new RepositoryError("purchaseOrderReceipt", e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            if (createResponse.CreateProcurementReceiptWarnings.Any())
            {
                var message = string.Format("Warnings(s) occurred creating purchaseOrderReceipt '{0}': ", purchaseOrderReceiptsEntity.Guid);
                message += createResponse.CreateProcurementReceiptWarnings.Select(w => w.WarningMessages).Aggregate((a, b) => a + ", " + b);
                logger.Error(message);
            }

            // get the newly created  from the database
            return await GetPurchaseOrderReceiptByGuidAsync(createResponse.PriGuid);
        }

        /// <summary>
        /// Get the purchase order receipt requested
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <returns>Tuple of PurchaseOrderReceipt entity objects <see cref="PurchaseOrderReceipt"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<PurchaseOrderReceipt>, int>> GetPurchaseOrderReceiptsAsync(int offset, int limit, string purchaseOrderID ="")
        {
            var criteria = "WITH PRI.RECEIVED.BY NE ''";

            if (!string.IsNullOrEmpty(purchaseOrderID))
            {
                criteria = string.Format("WITH PRI.RECEIVED.BY NE '' AND WITH PRI.PO.ID EQ '{0}'", purchaseOrderID);
            }
            var purchaseOrderReceiptIds = await DataReader.SelectAsync("PO.RECEIPT.INTG", criteria);
            if (purchaseOrderReceiptIds == null || !purchaseOrderReceiptIds.Any())
            {
                return new Tuple<IEnumerable<PurchaseOrderReceipt>, int>(new List<PurchaseOrderReceipt>(), 0);
            }

            var totalCount = purchaseOrderReceiptIds.Count();
            Array.Sort(purchaseOrderReceiptIds);
            var subList = purchaseOrderReceiptIds.Skip(offset).Take(limit).ToArray();

            var purchaseOrderReceiptData = await DataReader.BulkReadRecordAsync<DataContracts.PoReceiptIntg>("PO.RECEIPT.INTG", subList);

            if (purchaseOrderReceiptData == null)
            {
                return new Tuple<IEnumerable<PurchaseOrderReceipt>, int>(new List<PurchaseOrderReceipt>(), 0);
            }

            var itemIds = purchaseOrderReceiptData.SelectMany(x => x.PriItems);
            var purchaseOrderReceiptItems = await DataReader.BulkReadRecordAsync<DataContracts.PoReceiptItemIntg>("PO.RECEIPT.ITEM.INTG", itemIds.ToArray());

            Dictionary<string, Dictionary<string, string>> purchaseOrderData = null;
            var purchaseOrderIds = purchaseOrderReceiptData.Select(p => p.PriPoId).ToArray();
            if (purchaseOrderIds.Any())
            {
               purchaseOrderData = await DataReader.BatchReadRecordColumnsAsync("PURCHASE.ORDERS", purchaseOrderIds.Distinct().ToArray(), new string[] { "PO.NO" });
            }
            var purchaseOrderReceipts = BuildPurchaseOrderReceipts(purchaseOrderReceiptData, purchaseOrderReceiptItems, purchaseOrderData);
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<PurchaseOrderReceipt>, int>(purchaseOrderReceipts, totalCount);

        }

        /// <summary>
        /// Get PurchaseOrderReceipt by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>PurchaseOrderReceipt entity object <see cref="PurchaseOrderReceipt"/></returns>
        public async Task<PurchaseOrderReceipt> GetPurchaseOrderReceiptByGuidAsync(string guid)
        {
            PurchaseOrderReceipt purchaseOrderReceiptEntity = null;
            if (string.IsNullOrEmpty(guid))
            {
                throw new KeyNotFoundException("No purchase-order-receipts was found for GUID " + guid);
            }
            var id = await GetPurchaseOrderReceiptIdFromGuidAsync(guid);

            if (id == null || string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("No purchase-order-receipts was found for GUID " + guid);
            }

            var purchaseOrderReceipt = await DataReader.ReadRecordAsync<PoReceiptIntg>(id);

            if (purchaseOrderReceipt == null)
            {
                throw new KeyNotFoundException("No purchase-order-receipts was found for GUID " + guid);
            }

            ICollection<DataContracts.PoReceiptItemIntg> poReceiptItems = null;
            if (purchaseOrderReceipt != null)
            {
                poReceiptItems = await DataReader.BulkReadRecordAsync<DataContracts.PoReceiptItemIntg>("PO.RECEIPT.ITEM.INTG", purchaseOrderReceipt.PriItems.ToArray());
            }

            var poNo = string.Empty;
            var purchaseOrderIds = new string[]{purchaseOrderReceipt.PriPoId};
            var purchaseOrderData = await DataReader.BatchReadRecordColumnsAsync("PURCHASE.ORDERS", purchaseOrderIds.ToArray(), new string[] { "PO.NO" });
            if (purchaseOrderData != null)
            {                
                var poNoDict = new Dictionary<string, string>();
                var found = purchaseOrderData.TryGetValue(purchaseOrderReceipt.PriPoId, out poNoDict);
                if (found)
                    poNoDict.TryGetValue("PO.NO", out poNo);
            }
            purchaseOrderReceiptEntity = BuildPurchaseOrderReceipt(purchaseOrderReceipt, poReceiptItems, poNo);
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return purchaseOrderReceiptEntity;
        }

        /// <summary>
        /// Get a PurchaseOrderReceipt id fom a guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>id</returns>
        public async Task<string> GetPurchaseOrderReceiptIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new KeyNotFoundException("No purchase-order-receipts was found for GUID " + guid);
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("No purchase-order-receipts was found for GUID " + guid);
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("No purchase-order-receipts was found for GUID " + guid);
            }

            if (foundEntry.Value.Entity != "PO.RECEIPT.INTG")
            {
                exception.AddError(new RepositoryError("GUID.Wrong.Type", string.Format("GUID {0} has different entity, {1}, than expected, PO.RECEIPT.INTG", guid, foundEntry.Value.Entity))
                {
                    Id = guid
                });
                throw exception;

            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        ///  Build collection of PurchaseOrderReceipt domain entities 
        /// </summary>
        /// <param name="purchaseOrderReceipts">Collection of PurchaseOrderReceipt data contracts</param>
        /// <returns>PurchaseOrderReceipt domain entity</returns>
        private IEnumerable<PurchaseOrderReceipt> BuildPurchaseOrderReceipts(IEnumerable<DataContracts.PoReceiptIntg> purchaseOrderReceipts, ICollection<DataContracts.PoReceiptItemIntg> poReceiptItems,
           Dictionary<string, Dictionary<string, string>> purchaseOrderData)
        {
            var purchaseOrderReceiptCollection = new List<PurchaseOrderReceipt>();

            foreach (var purchaseOrderReceipt in purchaseOrderReceipts)
            {
                try
                {
                    var poNo = string.Empty;
                    if (purchaseOrderData != null)
                    {
                        var poNoDict = new Dictionary<string, string>();
                        var found = purchaseOrderData.TryGetValue(purchaseOrderReceipt.PriPoId, out poNoDict);
                        if (found)
                            poNoDict.TryGetValue("PO.NO", out poNo);
                    }
                    var purchaseOrderReceiptEntity = BuildPurchaseOrderReceipt(purchaseOrderReceipt, poReceiptItems, poNo);
                    if (purchaseOrderReceiptEntity != null)
                        purchaseOrderReceiptCollection.Add(purchaseOrderReceiptEntity);
                }
                catch (Exception e)
                {
                    exception.AddError(new RepositoryError("Bad.Data", e.Message)
                    {
                        SourceId = purchaseOrderReceipt.Recordkey,
                        Id = purchaseOrderReceipt.RecordGuid
                    });
                }

            }
            return purchaseOrderReceiptCollection.AsEnumerable();
        }

        /// <summary>
        /// Build a PurchaseOrderReceipt
        /// </summary>
        /// <param name="purchaseOrderReceipt"></param>
        /// <param name="poReceiptItems"></param>
        /// <returns>PurchaseOrderReceipt domain entity</returns>
        private PurchaseOrderReceipt BuildPurchaseOrderReceipt(PoReceiptIntg purchaseOrderReceipt,
            ICollection<DataContracts.PoReceiptItemIntg> poReceiptItems, string poNo="")
        {
            if (purchaseOrderReceipt == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Procurement receipt is missing."));
                throw exception;
            }

            if (string.IsNullOrEmpty(purchaseOrderReceipt.RecordGuid))
            {
                exception.AddError(new RepositoryError("GUID.Not.Found", "Procurement receipt GUID is missing.")
                {
                    SourceId = purchaseOrderReceipt.Recordkey,
                    Id = purchaseOrderReceipt.RecordGuid                 
                }) ;
                throw exception;

            }

            var purchaseOrderReceiptDomainEntity = new PurchaseOrderReceipt(purchaseOrderReceipt.RecordGuid);

            if (!string.IsNullOrEmpty(poNo))
            {
                purchaseOrderReceiptDomainEntity.PoNo = poNo;
            }

            purchaseOrderReceiptDomainEntity.ArrivedVia = purchaseOrderReceipt.PriArrivedVia;
            purchaseOrderReceiptDomainEntity.PackingSlip = purchaseOrderReceipt.PriPackingSlip;
            purchaseOrderReceiptDomainEntity.PoId = purchaseOrderReceipt.PriPoId;

            purchaseOrderReceiptDomainEntity.ReceivedBy = purchaseOrderReceipt.PriReceivedBy;
            purchaseOrderReceiptDomainEntity.ReceivedDate = purchaseOrderReceipt.PriReceivedDate;
            purchaseOrderReceiptDomainEntity.ReceivingComments = purchaseOrderReceipt.PriReceivingComments;
            purchaseOrderReceiptDomainEntity.Recordkey = purchaseOrderReceipt.Recordkey;

            if (purchaseOrderReceipt.PriItems != null && purchaseOrderReceipt.PriItems.Any())
            {
                var receiptItems = new List<PurchaseOrderReceiptItem>();
                foreach (var item in purchaseOrderReceipt.PriItems)
                {
                    if (poReceiptItems != null)
                    {
                        var poReceiptItem = poReceiptItems.FirstOrDefault(x => x.Recordkey == item);
                        if ((poReceiptItem != null) && !(string.IsNullOrEmpty(poReceiptItem.PriiItemsId)))
                        {
                            var receiptItem = new PurchaseOrderReceiptItem(poReceiptItem.PriiItemsId);

                            receiptItem.ReceivedAmt = poReceiptItem.PriiReceivedAmt;
                            receiptItem.ReceivedAmtCurrency = poReceiptItem.PriiReceivedAmtCurrency;
                            receiptItem.ReceivedQty = poReceiptItem.PriiReceivedQty;

                            receiptItem.RejectedAmt = poReceiptItem.PriiRejectedAmt;
                            receiptItem.RejectedAmtCurrency = poReceiptItem.PriiRejectedAmtCurrency;
                            receiptItem.RejectedQty = poReceiptItem.PriiRejectedQty;

                            receiptItem.ReceivingComments = poReceiptItem.PriiReceivingComments;

                            receiptItems.Add(receiptItem);
                        }
                    }
                }
                purchaseOrderReceiptDomainEntity.ReceiptItems = receiptItems;
            }
            return purchaseOrderReceiptDomainEntity;
        }

        /// <summary>
        /// Create an CreateProcurementReceiptRequest from a PurchaseOrderReceipt domain entity
        /// </summary>
        /// <param name="PurchaseOrderReceiptEntity">PurchaseOrderReceipt domain entity</param>
        /// <returns> CreateProcurementReceiptRequest transaction object</returns>
        private CreateProcurementReceiptRequest BuildPurchaseOrderReceiptUpdateRequest(PurchaseOrderReceipt purchaseOrderReceiptEntity)
        {
            var request = new CreateProcurementReceiptRequest();
            request.PriGuid = purchaseOrderReceiptEntity.Guid;
            request.PoReceiptIntgId = purchaseOrderReceiptEntity.Recordkey;
            request.PriArrivedVia = purchaseOrderReceiptEntity.ArrivedVia;
            if (purchaseOrderReceiptEntity.ReceiptItems != null && purchaseOrderReceiptEntity.ReceiptItems.Any())
            {
                var bpvItems = new List<BpvItems>();
                foreach (var lineItem in purchaseOrderReceiptEntity.ReceiptItems)
                {
                    var bpvItem = new BpvItems();
                    bpvItem.PriiItemsId = lineItem.Id;
                    bpvItem.PriiReceivedAmt = lineItem.ReceivedAmt;
                    bpvItem.PriiReceivedAmtCurrency = lineItem.ReceivedAmtCurrency;
                    bpvItem.PriiReceivedQty = lineItem.ReceivedQty;
                    if (!string.IsNullOrEmpty(lineItem.ReceivingComments))
                    {
                        bpvItem.PriiReceivingComments = lineItem.ReceivingComments.Replace(DmiString._VM, '\n')
                                                       .Replace(DmiString._TM, ' ')
                                                       .Replace(DmiString._SM, ' ');
                    }
                    bpvItem.PriiRejectedAmt = lineItem.RejectedAmt;
                    bpvItem.PriiRejectedAmtCurrency = lineItem.RejectedAmtCurrency;
                    bpvItem.PriiRejectedQty = lineItem.RejectedQty;
                    bpvItems.Add(bpvItem);
                }
                request.BpvItems = bpvItems;
            }

            request.PriPackingSlip = purchaseOrderReceiptEntity.PackingSlip;
            request.PriPoId = purchaseOrderReceiptEntity.PoId;
            request.PriReceivedBy = purchaseOrderReceiptEntity.ReceivedBy;

            request.PriReceivedDate = purchaseOrderReceiptEntity.ReceivedDate;
            if (!string.IsNullOrEmpty(purchaseOrderReceiptEntity.ReceivingComments))
            {
                var comment = purchaseOrderReceiptEntity.ReceivingComments.Replace(DmiString._VM, '\n')
                                               .Replace(DmiString._TM, ' ')
                                               .Replace(DmiString._SM, ' ');
                request.PriReceivingComments = comment;
            }

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }
            return request;
        }
    }
}