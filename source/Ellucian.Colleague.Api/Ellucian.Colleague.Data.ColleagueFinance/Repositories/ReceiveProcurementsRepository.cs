// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    ///  This class implements the IReceiveProcurementsRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ReceiveProcurementsRepository : BaseColleagueRepository, IReceiveProcurementsRepository
    {
        /// <summary>
        /// The constructor to instantiate a receive procurement repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public ReceiveProcurementsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        public async Task<IEnumerable<ReceiveProcurementSummary>> GetReceiveProcurementsByPersonIdAsync(string personId)
        {
            List<ReceiveProcurementSummary> receiveProcurementList = new List<ReceiveProcurementSummary>();
            List<string> filteredPurchaseOrder = new List<string>();
            List<string> filteredPOFromRequisition = new List<string>();

            List<Items> lineItems = new List<Items>();

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            List<string> filteredPurchaseOrderByStatus = await ApplyFilterCriteriaForPOByStatus();

            
            var purchaseOrderData = await DataReader.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", filteredPurchaseOrderByStatus.ToArray());

            // if there is no PO data with matching criteria return empty list
            if (purchaseOrderData == null || !(purchaseOrderData.Any()))
            {
                return receiveProcurementList;
            }

            List<string> filteredRequisitionIds = null;

            //Changes TODO
            var filteredPurchaseOrderByPersonId = await ApplyFilterCriteriaForPOByPersonId(personId, filteredPurchaseOrderByStatus);
            if (filteredPurchaseOrderByPersonId != null && filteredPurchaseOrderByPersonId.Any())
            {
                var requisitionIds = purchaseOrderData.SelectMany(r => r.PoReqIds).ToList().Distinct();
                filteredRequisitionIds = await ApplyFilterCriteriaForRequisitionsByPersonId(personId, requisitionIds.ToList());
            }

            var requisitions = await DataReader.BulkReadRecordAsync<DataContracts.Requisitions>("REQUISITIONS", purchaseOrderData.SelectMany(r => r.PoReqIds).ToList().Distinct().ToArray());

            string[] filteredItemIdsByStatus = null;
            var poItemIds = purchaseOrderData.SelectMany(r => r.PoItemsId).ToList().Distinct();
            if (poItemIds != null && poItemIds.Any())
            {
                string poItemIdQueryByStatus = string.Format("WITH ITM.PO.CURRENT.STATUS EQ 'B''H''O'");
                filteredItemIdsByStatus = await DataReader.SelectAsync("ITEMS", poItemIds.ToArray(), poItemIdQueryByStatus);
            }

            if ((filteredPurchaseOrderByPersonId == null || !(filteredPurchaseOrderByPersonId.Any())) && (filteredRequisitionIds == null || !(filteredRequisitionIds.Any())) && (filteredItemIdsByStatus == null || !(filteredItemIdsByStatus.Any())))
            {
                return receiveProcurementList;
            }

            List<string> filteredItemIdByInitiator = new List<string>();
            if (filteredItemIdsByStatus != null && filteredItemIdsByStatus.Any())
            {
                string itemIdQueryByInitiator = string.Format("WITH ITM.INITIATOR EQ '{0}'", personId);
                filteredItemIdByInitiator = await ExecuteQueryStatement("ITEMS", filteredItemIdsByStatus.ToList(), itemIdQueryByInitiator);
            }


            #region Get all VendorInformation from transactions

            List<string> vendorIds = purchaseOrderData.Where(x => (x.PoVendor != null && x.PoVendor.Length > 0)).Select(s => s.PoVendor).Distinct().ToList();

            GetVendorInformationResponse vendorResponse = new GetVendorInformationResponse();

            if (vendorIds != null && vendorIds.Any())
            {
                vendorResponse = await GetVendorsInformation(vendorIds);
            }

            #endregion

            var cWebDefaults = await DataReader.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS");

            if (purchaseOrderData != null && purchaseOrderData.Any())
            {
                foreach (PurchaseOrders purchaseOrder in purchaseOrderData)
                {
                    List<Items> filteredLineItems = new List<Items>();
                    try
                    {
                        //  * Evaluate the PO Requestor and Initiator fields
                        //  * Include the items in the list
                        //  * No need to check the ITEMS initiator
                        if (filteredPurchaseOrderByPersonId.Contains(purchaseOrder.Recordkey))
                        {
                            var items = filteredItemIdsByStatus.Where(x => purchaseOrder.PoItemsId.Any(y => y.Equals(x)));
                            if (items != null && items.Any())
                            {
                                await GetPoItems(cWebDefaults, items.ToList(), filteredLineItems);
                            }
                        }
                        else
                        {
                            //* Since the user was not the initiator or requestor for the PO, see if
                            //* the PO was created from requisition(s).
                            if (filteredRequisitionIds != null && filteredRequisitionIds.Any())
                            {
                                bool requisitionIdMatchFound = false;
                                foreach (var requisitionId in purchaseOrder.PoReqIds)
                                {
                                    requisitionIdMatchFound = filteredRequisitionIds.Contains(requisitionId);
                                    bool initiatorCheck = !requisitionIdMatchFound;

                                    //* This covers the case where a PO was created from requisition(s), and the
                                    //* user is not the initiator or requestor on the PO, so we need to read the
                                    //* requisition(s) to further evaluate the match.
                                    //* if user is requestor or initiator of requisution(s) validate the ITEMS without checking Item Initiator
                                    //* if not,  a PO was created from requisition(s), and the
                                    //* user is not the initiator or requestor on the PO or REQ, so we
                                    //* need to check the items for Item inititor .

                                    var items = initiatorCheck ? filteredItemIdByInitiator.Where(x => purchaseOrder.PoItemsId.Any(y => y.Equals(x))) : filteredItemIdsByStatus.Where(x => purchaseOrder.PoItemsId.Any(y => y.Equals(x)));
                                    if (items != null && items.Any())
                                    {
                                        await GetPoItems(cWebDefaults, items.ToList(), filteredLineItems);
                                    }
                                }

                            }
                            else
                            {
                                //* This covers the case where the user is not the initiator or requestor of
                                //* the PO, and no requisitions are associated with the PO, so we need to
                                //* check the items for item initiator.
                                var items = filteredItemIdByInitiator.Where(x => purchaseOrder.PoItemsId.Any(y => y.Equals(x)));
                                if (items != null && items.Any())
                                {
                                    await GetPoItems(cWebDefaults, items.ToList(), filteredLineItems);
                                }
                            }
                        }

                        // Build ReceiveProcurementSummary object only if there any line items to receive
                        if (filteredLineItems != null && filteredLineItems.Any())
                        {
                            filteredLineItems = filteredLineItems.GroupBy(x => x.Recordkey).Select(x => x.First()).ToList();
                            // Filter vendor transaction response to get only current vendor inforation
                            var vendorinfo = vendorResponse.VendorInfo.Where(x => x.VendorId == purchaseOrder.PoVendor).SingleOrDefault();
                            List<Transactions.VendorContactSummary> contactSummary = new List<Transactions.VendorContactSummary>();
                            var commodityCodeData = await DataReader.BulkReadRecordAsync<DataContracts.CommodityCodes>("COMMODITY.CODES", filteredLineItems.Where(x => (x.ItmCommodityCode != null && x.ItmCommodityCode.Length > 0)).Select(s => s.ItmCommodityCode).Distinct().ToArray());
                            List<Requisitions> requisitionSummary = new List<Requisitions>();

                            if (purchaseOrder.PoReqIds != null && purchaseOrder.PoReqIds.Any() && requisitions != null && requisitions.Any())
                                requisitionSummary = requisitions.Where(x => purchaseOrder.PoReqIds.Any(y => y.Equals(x.Recordkey))).ToList();

                            if (!(string.IsNullOrEmpty(purchaseOrder.PoVendor)))
                                contactSummary = vendorResponse.VendorContactSummary.Where(x => x.ContactVendorId == purchaseOrder.PoVendor).ToList();
                           var commodityCodeList = commodityCodeData == null ? new List<CommodityCodes>() : commodityCodeData.ToList();
                            receiveProcurementList.Add(BuildReceiveProcurementSummary(purchaseOrder, filteredLineItems, vendorinfo, contactSummary, commodityCodeList, requisitionSummary));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            return receiveProcurementList.AsEnumerable();
        }

        public async Task<ProcurementAcceptReturnItemInformationResponse> AcceptOrReturnProcurementItemsAsync(ProcurementAcceptReturnItemInformationRequest acceptReturnRequest)
        {
            if (acceptReturnRequest == null)
                throw new ArgumentNullException("acceptReturnProcurementItemEntity", "Must provide a acceptReturnRequest for receiving.");

            ProcurementAcceptReturnItemInformationResponse response = new ProcurementAcceptReturnItemInformationResponse();
            var createRequest = BuildProcurementAcceptReturnRequest(acceptReturnRequest);

            try
            {
                // write the  data
                var createResponse = await transactionInvoker.ExecuteAsync<TxReceiveReturnProcurementItemsRequest, TxReceiveReturnProcurementItemsResponse>(createRequest);

                if (!(createResponse.ErrorOccurred) && (createResponse.ErrorMessages == null || !createResponse.ErrorMessages.Any()))
                {
                    response.ErrorOccurred = false;
                    response.ErrorMessages = new List<string>();
                    response.ProcurementItemsInformationResponse = ConvertTransactionDataToEntity(createResponse.ItemInformation);
                }
                else
                {
                    response.ErrorOccurred = true;
                    response.ErrorMessages = createResponse.ErrorMessages;
                    response.ErrorMessages.RemoveAll(message => string.IsNullOrEmpty(message));
                }
                response.WarningOccurred = createResponse.WarningOccurred  ? true : false;
                response.WarningMessages = (createResponse.WarningMessages != null || createResponse.WarningMessages.Any()) ? createResponse.WarningMessages : new List<string>();
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Debug(csee, "Session expired.");
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw;
            }

            return response;
        }

        #region Private Methods

        private async Task GetPoItems(CfwebDefaults cWebDefaults, List<string> poItemIds, List<Items> filteredLineItems)
        {
            if (poItemIds != null && poItemIds.Any())
            {
                List<Items> lineItems = await ApplyFilterCriteriaForItems(poItemIds, cWebDefaults);
                if (lineItems != null && lineItems.Any())
                {
                    filteredLineItems.AddRange(lineItems);
                }
            }

        }

        /// <summary>
        /// Used to fetch vendor information and contact information from vendor ids
        /// </summary>
        /// <param name="vendorIds">vendor ids to fetch information</param>
        /// <returns></returns>
        private async Task<GetVendorInformationResponse> GetVendorsInformation(List<string> vendorIds)
        {
            var vendorInfo = new GetVendorInformationRequest();

            vendorInfo.VendorIds = vendorIds;

            var vendorResponse = await transactionInvoker.ExecuteAsync<GetVendorInformationRequest, GetVendorInformationResponse>(vendorInfo);
            if (vendorResponse == null)
            {
                throw new InvalidOperationException("An error occurred during person matching");
            }
            if (vendorResponse.ErrorMessages.Count() > 0)
            {
                var errorMessage = "Error(s) occurred during vendor information fetch:";
                errorMessage += string.Join(Environment.NewLine, vendorResponse.ErrorMessages);
                logger.Error(errorMessage);
                throw new InvalidOperationException("An error occurred during vendor information fetch");
            }

            return vendorResponse;
        }


        private async Task<List<string>> ApplyFilterCriteriaForPOByStatus()
        {
            List<string> filteredPurchaseOrder = new List<string>();

            //status should be either 'BackOrdered' or 'Outstanding' and  personId is Initiator OR requestor
            string PurchaseOrderFilterQuery = string.Format("WITH PO.CURRENT.STATUS EQ 'B''O' BY.DSND PO.NO");
            filteredPurchaseOrder = await ExecuteQueryStatement("PURCHASE.ORDERS", filteredPurchaseOrder, PurchaseOrderFilterQuery);

            return filteredPurchaseOrder;
        }

        private async Task<List<string>> ApplyFilterCriteriaForPOByPersonId(string personId, List<string> filteredPurchaseOrder)
        {
            //personId is Initiator OR requestor for the purchase order
            string PurchaseOrderFilterQuery = string.Format("WITH PO.DEFAULT.INITIATOR EQ '{0}' OR WITH PO.REQUESTOR EQ '{0}' BY.DSND PO.NO", personId);

            filteredPurchaseOrder = await ExecuteQueryStatement("PURCHASE.ORDERS", filteredPurchaseOrder, PurchaseOrderFilterQuery);

            return filteredPurchaseOrder;
        }

        private async Task<List<string>> ApplyFilterCriteriaForRequisitionsByPersonId(string personId, List<string> requisitionsIds)
        {
            //personId is Initiator OR requestor for the requisitions
            string reqPersonIdFilterQuery = string.Format("WITH REQ.DEFAULT.INITIATOR EQ '{0}' OR WITH REQ.REQUESTOR EQ '{0}' BY.DSND REQ.NO", personId);
            requisitionsIds = await ExecuteQueryStatement("REQUISITIONS", requisitionsIds, reqPersonIdFilterQuery);

            return requisitionsIds;
        }

        /// <summary>
        /// Used to execute query criteria
        /// </summary>
        /// <param name="physicalFileName">Entity from which data needs to be fetch</param>
        /// <param name="filteredIds">Ids from which query needs to be filetered</param>
        /// <param name="queryCriteria"></param>
        /// <returns></returns>
        private async Task<List<string>> ExecuteQueryStatement(string physicalFileName, List<string> filteredIds, string queryCriteria)
        {
            string[] filteredByQueryCriteria = null;
            if (string.IsNullOrEmpty(queryCriteria))
                return null;
            if (filteredIds != null && filteredIds.Any())
            {
                filteredByQueryCriteria = await DataReader.SelectAsync(physicalFileName, filteredIds.ToArray(), queryCriteria);
            }
            else
            {
                filteredByQueryCriteria = await DataReader.SelectAsync(physicalFileName, queryCriteria);
            }
            return filteredByQueryCriteria.ToList();
        }

        /// <summary>
        /// Get the lineitems to receive
        /// </summary>
        /// <param name="purchaseOrderItems">Purchase order items</param>
        /// <param name="cfWebDefaults">This is used to do some item level validations</param>
        /// <param name="personId">This is parameter is used only when to check the current user is item level initiator</param>
        /// <returns></returns>
        private async Task<List<Items>> ApplyFilterCriteriaForItems(List<string> purchaseOrderItems, CfwebDefaults cfWebDefaults)
        {
            List<Items> itms = new List<Items>();
            bool eligibleItem = false;

            //null & empty check for purchaseOrderItems

            if (purchaseOrderItems != null && purchaseOrderItems.Any())
            {
                //var lineItems = await ApplyFilterCriteriaForPOItemIdsByStatus(purchaseOrderItems);
                var lineItemRecords = await DataReader.BulkReadRecordAsync<Items>(purchaseOrderItems.ToArray());

                if (lineItemRecords != null && lineItemRecords.Any())
                {
                    foreach (var item in lineItemRecords)
                    {
                        eligibleItem = false;

                        if (!string.IsNullOrEmpty(item.ItmFixedAssetsFlag))
                        {
                            if (item.ItmFixedAssetsFlag.Equals("S"))
                            {
                                if (item.ItmPoExtPrice <= cfWebDefaults.CfwebFixedAssetAmt || cfWebDefaults.CfwebFixedAssetAmt == null)
                                {
                                    eligibleItem = true;
                                }
                            }
                            if (item.ItmFixedAssetsFlag.Equals("M"))
                            {
                                if (item.ItmPoPrice <= cfWebDefaults.CfwebFixedAssetAmt || cfWebDefaults.CfwebFixedAssetAmt == null)
                                {
                                    eligibleItem = true;
                                }
                            }
                        }
                        else
                        {
                            eligibleItem = true;
                        }

                        //if (initiatorCheck)
                        //{
                        //    eligibleItem = item.ItmInitiator == personId;
                        //}

                        if (eligibleItem)
                        {
                            itms.Add(item);
                        }
                    }
                }
            }
            return itms;
        }



        /// <summary>
        /// Building ReceiveProcurementSummary entity
        /// </summary>
        /// <param name="purchaseOrderDataContract"></param>
        /// <param name="lineItemDataContract"></param>
        /// <param name="vendorInfo"></param>
        /// <param name="vendorContactInformation"></param>
        /// <returns></returns>
        private ReceiveProcurementSummary BuildReceiveProcurementSummary(PurchaseOrders purchaseOrderDataContract, List<Items> lineItemDataContract, Transactions.VendorInfo vendorInfo, List<Transactions.VendorContactSummary> vendorContactInformation, List<CommodityCodes> commodityCodes, List<Requisitions> requisitions)
        {

            if (purchaseOrderDataContract == null)
            {
                throw new ArgumentNullException("purchaseOrderDataContract");
            }

            if (string.IsNullOrEmpty(purchaseOrderDataContract.Recordkey))
            {
                throw new ArgumentNullException("id");
            }

            List<LineItemSummary> lineItemSummary = new List<LineItemSummary>();

            foreach (var itm in lineItemDataContract)
            {
                var commodityCode = commodityCodes.Where(x => (x.Recordkey == itm.ItmCommodityCode));
                bool msdsFlag = false;
                if (commodityCode != null && commodityCode.Any())
                {   
                    msdsFlag = !(string.IsNullOrEmpty(commodityCode.FirstOrDefault().CmdtyMsdsFlag)) ? commodityCode.FirstOrDefault().CmdtyMsdsFlag.Equals("Y") : false ;
                }

                lineItemSummary.Add(new LineItemSummary(itm.Recordkey, itm.ItmVendorPart, itm.ItmPoQty)
                {
                    ItemDescription = itm.ItmDesc.FirstOrDefault(),
                    ItemUnitOfIssue = itm.ItmPoIssue,
                    ItemMSDSFlag = msdsFlag

                });
            }

            List<RequisitionLinkSummary> reqs = new List<RequisitionLinkSummary>();

            foreach (var req in requisitions) {
                reqs.Add(new RequisitionLinkSummary(req.Recordkey, req.ReqNo));
            }

            #region Converting transaction vendor informtion to VendorInfo entity

            Domain.ColleagueFinance.Entities.VendorInfo vendorInformation = new Domain.ColleagueFinance.Entities.VendorInfo();
            if (vendorInfo != null)
            {
                vendorInformation.VendorId = vendorInfo.VendorId;
                vendorInformation.VendorName = vendorInfo.VendorName;
                vendorInformation.Address = vendorInfo.VendorAddress;
                vendorInformation.City = vendorInfo.VendorCity;
                vendorInformation.State = vendorInfo.VendorState;
                vendorInformation.Country = vendorInfo.VendorCountry;
                vendorInformation.Zip = vendorInformation.Zip;
            }

            if (purchaseOrderDataContract.PoMiscName != null && purchaseOrderDataContract.PoMiscName.Any())
            {
                vendorInformation.VendorMiscName = purchaseOrderDataContract.PoMiscName.FirstOrDefault();
            }

            List<Domain.ColleagueFinance.Entities.VendorContactSummary> vendorContactSummary = new List<Domain.ColleagueFinance.Entities.VendorContactSummary>();
            if (vendorContactInformation != null && vendorContactInformation.Count > 0)
            {
                foreach (var vendorContact in vendorContactInformation)
                {
                    vendorContactSummary.Add(new Domain.ColleagueFinance.Entities.VendorContactSummary()
                    {
                        Name = vendorContact.VendorContactName,
                        Title = vendorContact.VendorContactTitle,
                        Phone = vendorContact.VendorContactPhone,
                        Email = vendorContact.VendorContactEmail
                    });
                }
            }
            vendorInformation.VendorContacts = vendorContactSummary;
            #endregion

            
            var receiveProcurementSummary = new ReceiveProcurementSummary(purchaseOrderDataContract.Recordkey, purchaseOrderDataContract.PoNo, lineItemSummary, vendorInformation, reqs);

            return receiveProcurementSummary;
        }

        private TxReceiveReturnProcurementItemsRequest BuildProcurementAcceptReturnRequest(ProcurementAcceptReturnItemInformationRequest acceptReturnRequest)
        {
            var request = new TxReceiveReturnProcurementItemsRequest();

            if (!string.IsNullOrEmpty(acceptReturnRequest.StaffUserId))
            {
                request.StaffUserId = acceptReturnRequest.StaffUserId;
            }
            if (!string.IsNullOrEmpty(acceptReturnRequest.ArrivedVia))
            {
                request.ArrivedVia = acceptReturnRequest.ArrivedVia;
            }
            request.AcceptAll = acceptReturnRequest.AcceptAll;
            request.IsPoFilterApplied = acceptReturnRequest.IsPoFilterApplied;
            request.PackingSlip = acceptReturnRequest.PackingSlip;

            foreach( var item in acceptReturnRequest.ProcurementItemsInformation)
            {
                ItemInformation poItem = new ItemInformation();
                poItem.PoId = item.PurchaseOrderId;
                poItem.PoNumber = item.PurchaseOrderNumber;
                poItem.Vendor = item.Vendor;
                poItem.ItemId = item.ItemId;
                poItem.ItemDesc = item.ItemDescription;
                poItem.ItemMsdsFlag = item.ItemMsdsFlag;
                poItem.ItemMsdsReceived = item.ItemMsdsReceived;
                poItem.QuantityOrdered = item.QuantityOrdered.ToString();
                poItem.QuantityAccepted = item.QuantityAccepted.ToString();
                poItem.QuantityRejected = item.QuantityRejected.ToString();
                poItem.Reorder = item.ReOrder;
                poItem.ReturnAuthorizationNumber = item.ReturnAuthorizationNumber;
                poItem.ReturnComments = item.ReturnComments;
                poItem.ConfirmationEmail = item.ConfirmationEmail;

                if (!string.IsNullOrEmpty(item.ReturnVia))
                {
                    poItem.ReturnVia = item.ReturnVia;
                }

                if (item.ReturnDate.HasValue)
                {
                    poItem.ReturnDate = item.ReturnDate;
                }

                if (!string.IsNullOrEmpty(item.ReturnReason))
                {
                    poItem.ReturnReason = item.ReturnReason;
                }

                request.ItemInformation.Add(poItem);
            }
            
            return request;
        }

        private List<ProcurementItemInformationResponse> ConvertTransactionDataToEntity(List<ItemInformation> items)
        {
            List<ProcurementItemInformationResponse> response = new List<ProcurementItemInformationResponse>();

            foreach (var item in items)
            {
                ProcurementItemInformationResponse responseItem = new ProcurementItemInformationResponse();
                responseItem.PurchaseOrderId = item.PoId;
                responseItem.PurchaseOrderNumber = item.PoNumber;
                responseItem.ItemId = item.ItemId;
                responseItem.ItemDescription = item.ItemDesc;
                response.Add(responseItem);
            }
            return response;
        }

        #endregion
    }

}


