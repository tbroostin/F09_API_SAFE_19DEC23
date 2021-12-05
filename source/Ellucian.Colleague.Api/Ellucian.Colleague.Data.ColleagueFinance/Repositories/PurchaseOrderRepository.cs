// Copyright 2015-2021 Ellucian Company L.P. and its affiliates

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
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.Base.Services;
using System.Collections.ObjectModel;
using System.Text;
using Ellucian.Dmi.Runtime;
using GlAccts = Ellucian.Colleague.Data.ColleagueFinance.DataContracts.GlAccts;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    ///  This class implements the IPurchaseOrderRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PurchaseOrderRepository : BaseColleagueRepository, IPurchaseOrderRepository
    {
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        private const int PurchaseOrdersCacheTimeout = 20;
        private const string AllPurchaseOrdersCache = "AllPurchaseOrders";
        RepositoryException exception = null;
        public static char _SM = Convert.ToChar(DynamicArray.SM);

        /// <summary>
        /// The constructor to instantiate a purchase order repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public PurchaseOrderRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        public async Task<PurchaseOrder> CreatePurchaseOrdersAsync(PurchaseOrder purchaseOrdersEntity)
        {
            if (purchaseOrdersEntity == null)
                throw new ArgumentNullException("purchaseOrdersEntity", "Must provide a purchaseOrderEntity to create.");

            var extendedDataTuple = GetEthosExtendedDataLists();

            var createRequest = BuildPurchaseOrderUpdateRequest(purchaseOrdersEntity);
            createRequest.PoId = null;
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdateCreatePoRequest, UpdateCreatePoResponse>(createRequest);

            if (createResponse.UpdatePOErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred creating purchaseOrder '{0}':", purchaseOrdersEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.UpdatePOErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created  from the database
            return await GetPurchaseOrdersByGuidAsync(createResponse.Guid);
        }

        public async Task<PurchaseOrder> UpdatePurchaseOrdersAsync(PurchaseOrder purchaseOrdersEntity)
        {
            if (purchaseOrdersEntity == null)
                throw new ArgumentNullException("purchaseOrdersEntity", "Must provide a purchaseOrdersEntity to update.");
            if (string.IsNullOrEmpty(purchaseOrdersEntity.Guid))
                throw new ArgumentNullException("purchaseOrdersEntity", "Must provide the guid of the purchaseOrdersEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var purchaseOrderId = string.Empty;
            if (purchaseOrdersEntity.Guid != Guid.Empty.ToString())
            {
                try
                {
                    purchaseOrderId = await this.GetPurchaseOrdersIdFromGuidAsync(purchaseOrdersEntity.Guid);
                }
                catch (KeyNotFoundException)
                { }
            }

            if (!string.IsNullOrEmpty(purchaseOrderId))
            {
                var extendedDataTuple = GetEthosExtendedDataLists();
                var updateRequest = BuildPurchaseOrderUpdateRequest(purchaseOrdersEntity);
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }
                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateCreatePoRequest, UpdateCreatePoResponse>(updateRequest);

                if ((!string.IsNullOrEmpty(updateResponse.Error) && updateResponse.Error != "0") || updateResponse.UpdatePOErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating purchaseOrder '{0}':", purchaseOrdersEntity.Guid);
                    var exception = new RepositoryException();
                    updateResponse.UpdatePOErrors.ForEach(e => exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(!string.IsNullOrEmpty(e.ErrorCodes) ? e.ErrorCodes + ": " : "", e.ErrorMessages))
                    {
                        Id = purchaseOrdersEntity.Guid,
                        SourceId = purchaseOrderId
                    }));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated entity from the database
                return await GetPurchaseOrdersByGuidAsync(purchaseOrdersEntity.Guid);
            }

            // perform a create instead
            return await CreatePurchaseOrdersAsync(purchaseOrdersEntity);
        }

        /// <summary>
        /// Get the purchase order requested
        /// </summary>
        /// <param name="id">Purchase Order ID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>A purchase order domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<PurchaseOrder> GetPurchaseOrderAsync(string id, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            logger.Debug(string.Format("purchase order {0} ", id));
            logger.Debug(string.Format("gl access level {0}", glAccessLevel));

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (expenseAccounts == null)
            {
                expenseAccounts = new List<string>();
                logger.Debug(string.Format("no GL accounts for purchase order {0} ", id));
            }

            var purchaseOrder = await DataReader.ReadRecordAsync<PurchaseOrders>(id);
            if (purchaseOrder == null)
            {
                throw new KeyNotFoundException(string.Format("Purchase Order record {0} does not exist.", id));
            }

            // Translate the status code into a PurchaseOrderStatus enumeration value
            PurchaseOrderStatus purchaseOrderStatus = new PurchaseOrderStatus();

            // Get the first status in the list of purchase order statuses and check it has a value.
            if (purchaseOrder.PoStatus != null && !string.IsNullOrEmpty(purchaseOrder.PoStatus.FirstOrDefault()))
            {
                switch (purchaseOrder.PoStatus.FirstOrDefault().ToUpper())
                {
                    case "A":
                        purchaseOrderStatus = PurchaseOrderStatus.Accepted;
                        break;
                    case "B":
                        purchaseOrderStatus = PurchaseOrderStatus.Backordered;
                        break;
                    case "C":
                        purchaseOrderStatus = PurchaseOrderStatus.Closed;
                        break;
                    case "U":
                        purchaseOrderStatus = PurchaseOrderStatus.InProgress;
                        break;
                    case "I":
                        purchaseOrderStatus = PurchaseOrderStatus.Invoiced;
                        break;
                    case "N":
                        purchaseOrderStatus = PurchaseOrderStatus.NotApproved;
                        break;
                    case "O":
                        purchaseOrderStatus = PurchaseOrderStatus.Outstanding;
                        break;
                    case "P":
                        purchaseOrderStatus = PurchaseOrderStatus.Paid;
                        break;
                    case "R":
                        purchaseOrderStatus = PurchaseOrderStatus.Reconciled;
                        break;
                    case "V":
                        purchaseOrderStatus = PurchaseOrderStatus.Voided;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid purchase order status for purchase order: " + purchaseOrder.Recordkey);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for purchase order: " + purchaseOrder.Recordkey);
            }

            #region Get Hierarchy Names
            // Determine the vendor name for the purchase order. If there is a miscellaneous name, use it.
            // Otherwise, we will get the name for the id further down.
            var purchaseOrderVendorName = string.Empty;

            if (purchaseOrder.PoMiscName != null)
            {
                purchaseOrderVendorName = purchaseOrder.PoMiscName.FirstOrDefault();
            }

            // Use a colleague transaction to get all names at once. 
            List<string> personIds = new List<string>();
            List<string> hierarchies = new List<string>();
            List<string> personNames = new List<string>();
            string initiatorName = null;
            string requestorName = null;

            // If there is no vendor name and there is a vendor id, use the PO hierarchy to get the vendor name.
            if ((string.IsNullOrEmpty(purchaseOrderVendorName)) && (!string.IsNullOrEmpty(purchaseOrder.PoVendor)))
            {
                personIds.Add(purchaseOrder.PoVendor);
                hierarchies.Add("PO");
            }

            // Use the PREFERRED hierarchy for the initiator and the requestor.
            if (!string.IsNullOrEmpty(purchaseOrder.PoDefaultInitiator))
            {
                personIds.Add(purchaseOrder.PoDefaultInitiator);
                hierarchies.Add("PREFERRED");
            }

            // Sometimes the requestor is the same person as the initiator. f they are the same,
            // there is no need to add it to the list because the hierarchy is the same.
            if ((!string.IsNullOrEmpty(purchaseOrder.PoRequestor)) && (purchaseOrder.PoRequestor != purchaseOrder.PoDefaultInitiator))
            {
                personIds.Add(purchaseOrder.PoRequestor);
                hierarchies.Add("PREFERRED");
            }

            // Call a colleague transaction to get the person names based on their hierarchies, if necessary
            if ((personIds != null) && (personIds.Count > 0))
            {
                GetHierarchyNamesForIdsRequest request = new GetHierarchyNamesForIdsRequest()
                {
                    IoPersonIds = personIds,
                    IoHierarchies = hierarchies
                };
                GetHierarchyNamesForIdsResponse response = transactionInvoker.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(request);

                // The transaction returns the hierarchy names. If the name is multivalued, 
                // the transaction only returns the first value of the name.
                if (response != null)
                {
                    if (!((response.OutPersonNames == null) || (response.OutPersonNames.Count < 1)))
                    {
                        for (int x = 0; x < response.IoPersonIds.Count(); x++)
                        {
                            var ioPersonId = response.IoPersonIds[x];
                            var hierarchy = response.IoHierarchies[x];
                            var name = response.OutPersonNames[x];
                            if (!string.IsNullOrEmpty(name))
                            {
                                if ((ioPersonId == purchaseOrder.PoVendor) && (hierarchy == "PO"))
                                {
                                    purchaseOrderVendorName = name;
                                }
                                if ((ioPersonId == purchaseOrder.PoDefaultInitiator) && (hierarchy == "PREFERRED"))
                                {
                                    initiatorName = name;
                                }
                                if ((ioPersonId == purchaseOrder.PoRequestor) && (hierarchy == "PREFERRED"))
                                {
                                    requestorName = name;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            if (purchaseOrder.PoStatusDate == null || !purchaseOrder.PoStatusDate.First().HasValue)
            {
                throw new ApplicationException("Missing status date for purchase order: " + purchaseOrder.Recordkey);
            }

            if (!purchaseOrder.PoDate.HasValue)
            {
                throw new ApplicationException("Missing date for purchase order: " + purchaseOrder.Recordkey);
            }

            // The purchase order status date contains one to many dates
            var purchaseOrderStatusDate = purchaseOrder.PoStatusDate.First().Value;

            var purchaseOrderDomainEntity = new PurchaseOrder(purchaseOrder.Recordkey, purchaseOrder.PoNo, purchaseOrderVendorName, purchaseOrderStatus, purchaseOrderStatusDate, purchaseOrder.PoDate.Value.Date);

            purchaseOrderDomainEntity.VendorId = purchaseOrder.PoVendor;

            // Assign Vendor Address
            await GetVendorAddress(purchaseOrder.PoVendor, purchaseOrderDomainEntity);

            if (!string.IsNullOrEmpty(initiatorName))
            {
                purchaseOrderDomainEntity.InitiatorName = initiatorName;
            }
            if (!string.IsNullOrEmpty(requestorName))
            {
                purchaseOrderDomainEntity.RequestorName = requestorName;
            }

            purchaseOrderDomainEntity.Amount = 0;
            purchaseOrderDomainEntity.CurrencyCode = purchaseOrder.PoCurrencyCode;
            if (purchaseOrder.PoMaintGlTranDate.HasValue)
            {
                purchaseOrderDomainEntity.MaintenanceDate = purchaseOrder.PoMaintGlTranDate.Value.Date;
            }

            if (purchaseOrder.PoExpectedDeliveryDate.HasValue)
            {
                purchaseOrderDomainEntity.DeliveryDate = purchaseOrder.PoExpectedDeliveryDate.Value.Date;
            }
            purchaseOrderDomainEntity.ApType = purchaseOrder.PoApType;
            purchaseOrderDomainEntity.DefaultCommodityCode = purchaseOrder.PoDefaultCommodity;
            purchaseOrderDomainEntity.Comments = purchaseOrder.PoPrintedComments;
            purchaseOrderDomainEntity.InternalComments = purchaseOrder.PoComments;
            purchaseOrderDomainEntity.ConfirmationEmailAddresses = purchaseOrder.PoConfEmailAddresses;

            // Add any associated requisitions to the purchase order domain entity
            if ((purchaseOrder.PoReqIds != null) && (purchaseOrder.PoReqIds.Count > 0))
            {
                foreach (var requisitionId in purchaseOrder.PoReqIds)
                {
                    if (!string.IsNullOrEmpty(requisitionId))
                    {
                        purchaseOrderDomainEntity.AddRequisition(requisitionId);
                    }
                }
            }

            // Add any associated vouchers to the purchase order domain entity
            if ((purchaseOrder.PoVouIds != null) && (purchaseOrder.PoVouIds.Count > 0))
            {
                foreach (var voucherNumber in purchaseOrder.PoVouIds)
                {
                    if (!string.IsNullOrEmpty(voucherNumber))
                    {
                        purchaseOrderDomainEntity.AddVoucher(voucherNumber);
                    }
                }
            }

            if (!string.IsNullOrEmpty(purchaseOrder.PoDefaultCommodity))
            {
                purchaseOrderDomainEntity.CommodityCode = purchaseOrder.PoDefaultCommodity;
            }

            // Read the OPERS records associated with the approval signatures and 
            // next approvers on the purchase order, and build approver objects.

            var operators = new List<string>();
            if (purchaseOrder.PoAuthorizations != null)
            {
                operators.AddRange(purchaseOrder.PoAuthorizations);
            }
            if (purchaseOrder.PoNextApprovalIds != null)
            {
                operators.AddRange(purchaseOrder.PoNextApprovalIds);
            }
            var uniqueOperators = operators.Distinct().ToList();
            if (uniqueOperators.Count > 0)
            {
                var Approvers = await DataReader.BulkReadRecordAsync<DataContracts.Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                if ((Approvers != null) && (Approvers.Count > 0))
                {
                    // loop through the opers, create Approver objects, add the name, and if they
                    // are one of the approvers of the purchase order, add the approval date.
                    foreach (var appr in Approvers)
                    {
                        Approver approver = new Approver(appr.Recordkey);
                        var approverName = appr.SysUserName;
                        approver.SetApprovalName(approverName);
                        if ((purchaseOrder.PoAuthEntityAssociation != null) && (purchaseOrder.PoAuthEntityAssociation.Count > 0))
                        {
                            foreach (var approval in purchaseOrder.PoAuthEntityAssociation)
                            {
                                if (approval.PoAuthorizationsAssocMember == appr.Recordkey)
                                {
                                    approver.ApprovalDate = approval.PoAuthorizationDatesAssocMember.Value;
                                }
                            }
                        }

                        // Add any approvals to the purchase order domain entity
                        purchaseOrderDomainEntity.AddApprover(approver);
                    }
                }
            }

            purchaseOrderDomainEntity.ShipToCode = purchaseOrder.PoShipTo;
            if (string.IsNullOrEmpty(purchaseOrder.PoShipTo))
            {
                var poDefaults = await DataReader.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS");
                if (poDefaults != null)
                {
                    purchaseOrderDomainEntity.ShipToCode = poDefaults.PurShipToCode;
                }
            }


            //Read the SHIP.TO code to get the name and populate the property
            var shiptoCode = purchaseOrder.PoShipTo;
            if (!string.IsNullOrEmpty(shiptoCode))
            {
                var ShipTo = await DataReader.ReadRecordAsync<ShipToCodes>(shiptoCode);
                if (ShipTo != null)
                {
                    var shipToName = ShipTo.ShptName;
                    purchaseOrderDomainEntity.ShipToCodeName = purchaseOrder.PoShipTo + " " + shipToName;
                }
            }

            purchaseOrderDomainEntity.AcceptedItemsId = purchaseOrder.PoAcceptedItemsId;
            // Add any accepted itemsto purchase order domain entity
            if ((purchaseOrder.PoAcceptedItemsId != null) && (purchaseOrder.PoAcceptedItemsId.Count > 0))
            {
                purchaseOrderDomainEntity.AcceptedItemsId = new List<string>();
                foreach (var acceptedItemId in purchaseOrder.PoAcceptedItemsId)
                {
                    if (!string.IsNullOrEmpty(acceptedItemId))
                    {
                        purchaseOrderDomainEntity.AcceptedItemsId.Add(acceptedItemId);
                    }
                }
            }

            //pre-paid voucher id (required to validate if PO can be enabled modification)
            purchaseOrderDomainEntity.PrepayVoucherId = purchaseOrder.PoPrepayVouId;


            // Populate the line item domain entities and add them to the purchase order domain entity
            var lineItemIds = purchaseOrder.PoItemsId;
            int documentFiscalYear = 0;
            if (lineItemIds != null && lineItemIds.Count() > 0)
            {
                // determine the fiscal year from the requisition date or maintenance date
                DateTime? transactionDate = purchaseOrder.PoDate;
                if (purchaseOrder.PoMaintGlTranDate != null)
                {
                    transactionDate = purchaseOrder.PoMaintGlTranDate;
                }
                logger.Debug(string.Format("transaction date for funds avail {0} ", transactionDate));
                if (transactionDate.HasValue)
                {
                    var transactionDateMonth = transactionDate.Value.Month;
                    documentFiscalYear = transactionDate.Value.Year;

                    var fiscalYearDataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);
                    if (fiscalYearDataContract != null && fiscalYearDataContract.FiscalStartMonth.HasValue)
                    {
                        if (fiscalYearDataContract.FiscalStartMonth > 1)
                        {
                            if (transactionDateMonth >= fiscalYearDataContract.FiscalStartMonth)
                            {
                                documentFiscalYear += 1;
                            }
                        }
                    }
                }
                logger.Debug(string.Format("purchase order fiscal year {0} ", documentFiscalYear));

                // Read the item records for the list of IDs in the purchase order record
                var lineItemRecords = await DataReader.BulkReadRecordAsync<Items>(lineItemIds.ToArray());
                if ((lineItemRecords != null) && (lineItemRecords.Count > 0))
                {
                    // If the user has the full access GL role, they have access to all GL accounts.
                    // There is no need to check for GL account access security. If they have partial 
                    // access, we need to check if the GL accounts on the PO are in the list of
                    // GL accounts to which the user has access.

                    bool hasGlAccess = false;
                    List<string> glAccountsAllowed = new List<string>();
                    if (glAccessLevel == GlAccessLevel.Full_Access)
                    {
                        hasGlAccess = true;
                    }
                    else if (glAccessLevel == GlAccessLevel.Possible_Access)
                    {
                        if (CanUserByPassGlAccessCheck(personId, purchaseOrder, purchaseOrderDomainEntity))
                        {
                            hasGlAccess = true;
                        }
                        else
                        {
                            // Put together a list of unique GL accounts for all the items
                            foreach (var lineItem in lineItemRecords)
                            {
                                if ((lineItem.ItemPoEntityAssociation != null) && (lineItem.ItemPoEntityAssociation.Count > 0))
                                {
                                    foreach (var glDist in lineItem.ItemPoEntityAssociation)
                                    {
                                        if (expenseAccounts.Contains(glDist.ItmPoGlNoAssocMember))
                                        {
                                            hasGlAccess = true;
                                            glAccountsAllowed.Add(glDist.ItmPoGlNoAssocMember);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // define a unique list of GL numbers to use for funds availability when the user has full GL access.
                    List<string> availableFundsGlAccounts = new List<string>();

                    List<string> itemProjectIds = new List<string>();
                    List<string> itemProjectLineIds = new List<string>();

                    // If this purchase order has a currency code, the purchase order amount has to be in foreign currency.
                    // We can only obtain that amount by adding up the foreign amounts from its items, so we still have to
                    // process the item information, regardless of GL account security.

                    foreach (var lineItem in lineItemRecords)
                    {
                        // The item description is a list of strings                     
                        string itemDescription = string.Empty;
                        foreach (var desc in lineItem.ItmDesc)
                        {
                            if (lineItem.ItmDesc.Count() > 1)
                            {
                                // If it is not a blank line, added it to the string.
                                // We are going to display all description as it if were one paragraph
                                // even if the user entered it in different paragraphs.
                                if (desc.Length > 0)
                                {
                                    itemDescription += desc + ' ';
                                }
                            }
                            else
                            {
                                // If the line item description is just one line, don't add a space at the end of it.
                                itemDescription = desc;
                            }
                        }

                        decimal itemQuantity = lineItem.ItmPoQty.HasValue ? lineItem.ItmPoQty.Value : 0;
                        decimal itemPrice = lineItem.ItmPoPrice.HasValue ? lineItem.ItmPoPrice.Value : 0;
                        decimal extendedPrice = lineItem.ItmPoExtPrice.HasValue ? lineItem.ItmPoExtPrice.Value : 0;

                        LineItem lineItemDomainEntity = new LineItem(lineItem.Recordkey, itemDescription, itemQuantity, itemPrice, extendedPrice);

                        if (lineItem.ItmExpectedDeliveryDate != null)
                        {
                            lineItemDomainEntity.ExpectedDeliveryDate = lineItem.ItmExpectedDeliveryDate.Value.Date;
                        }
                        lineItemDomainEntity.UnitOfIssue = lineItem.ItmPoIssue;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;
                        lineItemDomainEntity.TaxForm = lineItem.ItmTaxForm;
                        lineItemDomainEntity.TaxFormCode = lineItem.ItmTaxFormCode;
                        lineItemDomainEntity.TaxFormLocation = lineItem.ItmTaxFormLoc;
                        lineItemDomainEntity.Comments = lineItem.ItmComments;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart; var comments = string.Empty;
                        if (!string.IsNullOrEmpty(lineItem.ItmComments))
                        {
                            comments = CommentsUtility.ConvertCommentsToParagraphs(lineItem.ItmComments);
                        }
                        lineItemDomainEntity.Comments = comments;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;
                        lineItemDomainEntity.CommodityCode = lineItem.ItmCommodityCode;
                        lineItemDomainEntity.FixedAssetsFlag = lineItem.ItmFixedAssetsFlag;
                        lineItemDomainEntity.TradeDiscountAmount = lineItem.ItmPoTradeDiscAmt;
                        lineItemDomainEntity.TradeDiscountPercentage = lineItem.ItmPoTradeDiscPct;

                        if (lineItem.ItemPoStatusEntityAssociation != null && lineItem.ItemPoStatusEntityAssociation.Any())
                        {
                            // Current status is always in the first position in the association.
                            var poStatus = lineItem.ItemPoStatusEntityAssociation.FirstOrDefault();
                            if (poStatus != null)
                            {
                                if (!string.IsNullOrEmpty(poStatus.ItmPoStatusAssocMember))
                                {
                                    lineItemDomainEntity.LineItemStatus = GetLineItemStatus(poStatus.ItmPoStatusAssocMember, lineItem.Recordkey);
                                }
                                if (poStatus.ItmPoStatusDateAssocMember != null && poStatus.ItmPoStatusDateAssocMember.HasValue)
                                {
                                    lineItemDomainEntity.StatusDate = poStatus.ItmPoStatusDateAssocMember;
                                }
                            }
                        }

                        //Populate the Line Item tax codes for PO
                        if (lineItem.ItmTaxCodes != null)
                        {
                            List<LineItemReqTax> itemTaxes = new List<LineItemReqTax>();
                            foreach (var tax in lineItem.ItmTaxCodes)
                            {
                                var lineitemReqTaxCodes = tax;
                                LineItemReqTax itemTax = new LineItemReqTax(lineitemReqTaxCodes);
                                itemTaxes.Add(itemTax);
                            }
                            //sort the itemtaxes before adding to entities
                            itemTaxes = itemTaxes.OrderBy(tax => tax.TaxReqTaxCode).ToList();
                            //add the sorted taxes in the entities
                            foreach (var itemTax in itemTaxes)
                            {
                                lineItemDomainEntity.AddReqTax(itemTax);
                            }

                        }

                        // Populate the GL distribution domain entities and add them to the line items
                        if ((lineItem.ItemPoEntityAssociation != null) && (lineItem.ItemPoEntityAssociation.Count > 0))
                        {
                            foreach (var glDist in lineItem.ItemPoEntityAssociation)
                            {
                                // build a list of unique list of GL numbers on the line item GL distributions
                                // if the user has full GL access.
                                if (glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, purchaseOrder, purchaseOrderDomainEntity))
                                {
                                    if (!availableFundsGlAccounts.Contains(glDist.ItmPoGlNoAssocMember))
                                    {
                                        availableFundsGlAccounts.Add(glDist.ItmPoGlNoAssocMember);
                                    }
                                }

                                // The GL Distribution always uses the local currency amount.
                                decimal gldistGlQty = glDist.ItmPoGlQtyAssocMember.HasValue ? glDist.ItmPoGlQtyAssocMember.Value : 0;
                                decimal gldistGlAmount = glDist.ItmPoGlAmtAssocMember.HasValue ? glDist.ItmPoGlAmtAssocMember.Value : 0;
                                LineItemGlDistribution glDistribution = new LineItemGlDistribution(glDist.ItmPoGlNoAssocMember, gldistGlQty, gldistGlAmount);

                                if (!(string.IsNullOrEmpty(glDist.ItmPoProjectCfIdAssocMember)))
                                {
                                    glDistribution.ProjectId = glDist.ItmPoProjectCfIdAssocMember;
                                    if (!itemProjectIds.Contains(glDist.ItmPoProjectCfIdAssocMember))
                                    {
                                        itemProjectIds.Add(glDist.ItmPoProjectCfIdAssocMember);
                                    }
                                }

                                if (!(string.IsNullOrEmpty(glDist.ItmPoPrjItemIdsAssocMember)))
                                {
                                    glDistribution.ProjectLineItemId = glDist.ItmPoPrjItemIdsAssocMember;
                                    if (!itemProjectLineIds.Contains(glDist.ItmPoPrjItemIdsAssocMember))
                                    {
                                        itemProjectLineIds.Add(glDist.ItmPoPrjItemIdsAssocMember);
                                    }
                                }

                                lineItemDomainEntity.AddGlDistribution(glDistribution);

                                // Check the currency code to see if we need the local or foreign amount
                                if (string.IsNullOrEmpty(purchaseOrder.PoCurrencyCode))
                                {
                                    purchaseOrderDomainEntity.Amount += glDist.ItmPoGlAmtAssocMember.HasValue ? glDist.ItmPoGlAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    purchaseOrderDomainEntity.Amount += glDist.ItmPoGlForeignAmtAssocMember.HasValue ? glDist.ItmPoGlForeignAmtAssocMember.Value : 0;
                                }
                            }
                        }

                        // Add taxes to the line item
                        if ((lineItem.PoGlTaxesEntityAssociation != null) && (lineItem.PoGlTaxesEntityAssociation.Count > 0))
                        {
                            foreach (var taxGlDist in lineItem.PoGlTaxesEntityAssociation)
                            {
                                decimal itemTaxAmount = 0;
                                string lineItemTaxCode = taxGlDist.ItmPoGlTaxCodeAssocMember;

                                if (taxGlDist.ItmPoGlForeignTaxAmtAssocMember.HasValue)
                                {
                                    itemTaxAmount = taxGlDist.ItmPoGlForeignTaxAmtAssocMember.HasValue ? taxGlDist.ItmPoGlForeignTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    itemTaxAmount = taxGlDist.ItmPoGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmPoGlTaxAmtAssocMember.Value : 0;
                                }

                                LineItemTax itemTax = new LineItemTax(lineItemTaxCode, itemTaxAmount);

                                lineItemDomainEntity.AddTax(itemTax);

                                if (string.IsNullOrEmpty(purchaseOrder.PoCurrencyCode))
                                {
                                    purchaseOrderDomainEntity.Amount += taxGlDist.ItmPoGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmPoGlTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    purchaseOrderDomainEntity.Amount += taxGlDist.ItmPoGlForeignTaxAmtAssocMember.HasValue ? taxGlDist.ItmPoGlForeignTaxAmtAssocMember.Value : 0;
                                }
                            }
                        }

                        // Now apply GL account security to the line items.
                        // If hasGlAccess is true, it indicates the user has full access or has some
                        // access to the GL accounts in this purchase order. If hasGlAccess if false, 
                        // no line items will be added to the PO domain entity.

                        if (hasGlAccess == true)
                        {
                            // Now apply GL account access security when creating the line items.
                            // Check to see if the user has access to the GL accounts for each line item:
                            // - if they do not have access to any of them, we will not add the line item to the PO domain entity.
                            // - if the user has access to some of the GL accounts, the ones they do not have access to will be masked.

                            bool addItem = false;
                            if (glAccessLevel == GlAccessLevel.Full_Access)
                            {
                                // The user has full access and there is no need to check further
                                addItem = true;
                            }
                            else
                            {
                                if (CanUserByPassGlAccessCheck(personId, purchaseOrder, purchaseOrderDomainEntity))
                                {
                                    addItem = true;
                                }
                                else
                                {
                                    // We have the list of GL accounts the user can access in the argument fron the CTX.
                                    // Check if the user has GL access to at least one GL account in the list of GL accounts for this item
                                    // If the user has access to at least one GL account in the line item, add it to the domain

                                    if ((lineItemDomainEntity.GlDistributions != null) && (lineItemDomainEntity.GlDistributions.Count > 0))
                                    {
                                        foreach (var glDististribution in lineItemDomainEntity.GlDistributions)
                                        {
                                            if (glAccountsAllowed.Contains(glDististribution.GlAccountNumber))
                                            {
                                                addItem = true;
                                            }
                                            else
                                            {
                                                glDististribution.Masked = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (addItem)
                            {
                                purchaseOrderDomainEntity.AddLineItem(lineItemDomainEntity);
                            }
                        }
                    }

                    // If there are project IDs, we need to get the project number,
                    // and also the project line item code for each project line item ID 
                    if ((itemProjectIds != null) && (itemProjectIds.Count > 0))
                    {
                        // For each project ID, get the project number
                        var projectRecords = await DataReader.BulkReadRecordAsync<DataContracts.Projects>(itemProjectIds.ToArray());

                        // If there are project IDs, there should be project line item IDs
                        if ((itemProjectLineIds != null) && (itemProjectLineIds.Count > 0))
                        {
                            // For each project line item ID, get the project line item code
                            var projectLineItemRecords = await DataReader.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(itemProjectLineIds.ToArray());

                            if ((projectRecords != null) && (projectRecords.Count > 0))
                            {
                                for (int i = 0; i < purchaseOrderDomainEntity.LineItems.Count(); i++)
                                {
                                    foreach (var glDist in purchaseOrderDomainEntity.LineItems[i].GlDistributions)
                                    {
                                        // Only populate project information if the GL account is not masked.

                                        if (glDist.Masked == false)
                                        {
                                            foreach (var project in projectRecords)
                                            {
                                                if (project.Recordkey == glDist.ProjectId)
                                                {
                                                    glDist.ProjectNumber = project.PrjRefNo;
                                                }
                                            }

                                            if ((projectLineItemRecords != null) && (projectLineItemRecords.Count > 0))
                                            {
                                                foreach (var projectItem in projectLineItemRecords)
                                                {
                                                    if (projectItem.Recordkey == glDist.ProjectLineItemId)
                                                    {
                                                        glDist.ProjectLineItemCode = projectItem.PrjlnProjectItemCode;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // For each GL number that the user for which the user has access, get the funds availability 
                    // information on disk and add the amounts that are on the purchase order if the purchase order
                    // status is In Progress or Not Approved.
                    if (glAccountsAllowed.Any() || ((glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, purchaseOrder, purchaseOrderDomainEntity)) && availableFundsGlAccounts.Any()))
                    {
                        logger.Debug(string.Format("Calculating funds availability"));
                        string[] fundsAvailabilityArray;
                        if ((glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, purchaseOrder, purchaseOrderDomainEntity)) && availableFundsGlAccounts.Any())
                        {
                            fundsAvailabilityArray = availableFundsGlAccounts.ToArray();
                        }
                        else
                        {
                            fundsAvailabilityArray = glAccountsAllowed.ToArray();
                        }

                        //Read GL Account records.
                        var glAccountContracts = await DataReader.BulkReadRecordAsync<GlAccts>(fundsAvailabilityArray);

                        GlAcctsMemos glAccountMemosForFiscalYear;
                        string documentFiscalYr = documentFiscalYear.ToString();
                        decimal accountBudgetAmount = 0m;
                        decimal accountActualAmount = 0m;
                        decimal accountEncumbranceAmount = 0m;
                        decimal accountRequisitionAmount = 0m;

                        // find out if split requisition is on or off
                        var purchaseDefaults = await DataReader.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS");
                        if (purchaseDefaults == null)
                        {
                            logger.Debug(string.Format("Cannot get split requisition parameter"));
                        }
                        else
                        {
                            logger.Debug(string.Format("Allow split requisition flag {0} ", purchaseDefaults.PurAllowSplitReqFlag));
                        }

                        // Get funds availability information for all GL numbers for which the user has access.
                        string glpId = null;
                        foreach (var glAccount in fundsAvailabilityArray)
                        {
                            // get GL account available funds.
                            var glAccountContract = glAccountContracts.FirstOrDefault(x => x.Recordkey == glAccount);
                            if (glAccountContract != null)
                            {
                                if (glAccountContract.MemosEntityAssociation != null)
                                {
                                    // Check that the GL account is available for the fiscal year.
                                    glAccountMemosForFiscalYear = glAccountContract.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == documentFiscalYr);
                                    if (glAccountMemosForFiscalYear != null)
                                    {
                                        glpId = null;
                                        if (string.IsNullOrEmpty(glAccountMemosForFiscalYear.GlPooledTypeAssocMember))
                                        {
                                            accountBudgetAmount = glAccountMemosForFiscalYear.GlBudgetPostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlBudgetPostedAssocMember.Value : 0m;
                                            accountBudgetAmount += glAccountMemosForFiscalYear.GlBudgetMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlBudgetMemosAssocMember.Value : 0m;
                                            accountEncumbranceAmount = glAccountMemosForFiscalYear.GlEncumbrancePostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlEncumbrancePostedAssocMember.Value : 0m;
                                            accountEncumbranceAmount += glAccountMemosForFiscalYear.GlEncumbranceMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlEncumbranceMemosAssocMember.Value : 0m;
                                            accountRequisitionAmount = glAccountMemosForFiscalYear.GlRequisitionMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlRequisitionMemosAssocMember.Value : 0m;
                                            accountActualAmount = glAccountMemosForFiscalYear.GlActualPostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlActualPostedAssocMember.Value : 0m;
                                            accountActualAmount += glAccountMemosForFiscalYear.GlActualMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlActualMemosAssocMember.Value : 0m;
                                        }
                                        //poolee gl accounts should be assigned with corresponding umbrella gl account amounts
                                        else
                                        {
                                            if (glAccountMemosForFiscalYear.GlPooledTypeAssocMember.ToUpperInvariant() == "U")
                                            {
                                                glpId = glAccount;
                                                PopulateGlAmountFromFaFields(glAccountMemosForFiscalYear, out accountBudgetAmount, out accountActualAmount, out accountEncumbranceAmount, out accountRequisitionAmount);
                                            }
                                            else if (glAccountMemosForFiscalYear.GlPooledTypeAssocMember.ToUpperInvariant() == "P")
                                            {
                                                // get umbrella for this poolee
                                                var umbrellaGlAccount = glAccountMemosForFiscalYear.GlBudgetLinkageAssocMember;

                                                // Read the GL.ACCTS record for the umbrella, and get the amounts for fiscal year.
                                                var umbrellaAccount = await DataReader.ReadRecordAsync<GlAccts>(umbrellaGlAccount);
                                                if (umbrellaAccount != null)
                                                {
                                                    var umbrellaGlAccountAmounts = umbrellaAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == documentFiscalYr);
                                                    if (umbrellaGlAccountAmounts != null)
                                                    {
                                                        glpId = umbrellaGlAccount;
                                                        PopulateGlAmountFromFaFields(umbrellaGlAccountAmounts, out accountBudgetAmount, out accountActualAmount, out accountEncumbranceAmount, out accountRequisitionAmount);
                                                    }
                                                    else
                                                    {
                                                        logger.Debug(string.Format("Cannot get budget pool account for funds availability"));
                                                    }
                                                }
                                                else
                                                {
                                                    logger.Debug(string.Format("Cannot get budget pool account for fiscal year"));
                                                }
                                            }
                                        }
                                    }

                                    // if the purchase order status is In Progress or Not Approved, add the "unposted" amounts
                                    // on the purchase order to the funds availability information that has already been posted. This
                                    // logic will depend on whether the PO originated from one or more requisitions, and if it did,
                                    // whether requisition split is allowed.
                                    if (purchaseOrder.PoStatus.First().ToUpper() == "U" || purchaseOrder.PoStatus.First().ToUpper() == "N")
                                    {
                                        // If the GL account that the user has access to is a part of a budget pool, read the GLP record
                                        // to get all of the GL numbers associated with the budget pool so that the funds availability can 
                                        // updated for all of the GL numbers in the budget pool that are on the line items.
                                        List<string> budgetPoolAccounts = new List<string>();
                                        if (!string.IsNullOrEmpty(glpId))
                                        {
                                            string glpFyrFilename = "GLP." + documentFiscalYr;
                                            var glpFyrDataContract = await DataReader.ReadRecordAsync<GlpFyr>(glpFyrFilename, glpId);
                                            budgetPoolAccounts.Add(glpId);
                                            if (glpFyrDataContract != null && glpFyrDataContract.GlpPooleeAcctsList != null && glpFyrDataContract.GlpPooleeAcctsList.Any())
                                            {
                                                foreach (var poolee in glpFyrDataContract.GlpPooleeAcctsList)
                                                {
                                                    budgetPoolAccounts.Add(poolee);
                                                }
                                            }
                                        }

                                        // list of line items to update funds availability.
                                        List<string> itemsList = purchaseOrder.PoItemsId;
                                        
                                        if ((purchaseOrder.PoReqIds != null) && (purchaseOrder.PoReqIds.Any()))
                                        {
                                            // if the purchase order originated from one or more requisitions, then if requisition split is
                                            // not allowed, add line items from the requisitions that are not on the purchase order so that
                                            // memo requisition can be subtracted from the requisition funds availability amount.
                                            if (purchaseDefaults != null)
                                            {
                                                if (purchaseDefaults.PurAllowSplitReqFlag != "Y")
                                                {
                                                    var requisitionIds = purchaseOrder.PoReqIds;
                                                    var requisitions = await DataReader.BulkReadRecordAsync<DataContracts.Requisitions>("REQUISITIONS", requisitionIds.ToArray());
                                                    if (requisitions != null)
                                                    {
                                                        foreach (var req in requisitions)
                                                        {
                                                            if (req.ReqItemsId != null && req.ReqItemsId.Any())
                                                            {
                                                                foreach (var item in req.ReqItemsId)
                                                                {
                                                                    if (!itemsList.Contains(item))
                                                                    {
                                                                        itemsList.Add(item);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        // Read the item records needed to update funds availability. Use the PO line item GL distribution
                                                        // amounts to update the encumbrance funds availability amount. Use the requisition line item GL
                                                        // distribution amounts to update the requisition funds availability amount.
                                                        var faLineItemRecords = await DataReader.BulkReadRecordAsync<Items>(itemsList.ToArray());
                                                        if ((faLineItemRecords != null) && (faLineItemRecords.Any()))
                                                        {
                                                            foreach (var lineItem in faLineItemRecords)
                                                            {
                                                                if ((lineItem.ItemPoEntityAssociation != null) && (lineItem.ItemPoEntityAssociation.Any()))
                                                                {
                                                                    foreach (var glDist in lineItem.ItemPoEntityAssociation)
                                                                    {
                                                                        if (glDist != null)
                                                                        {
                                                                            if (glDist.ItmPoGlNoAssocMember == glAccount || budgetPoolAccounts.Contains(glDist.ItmPoGlNoAssocMember))
                                                                            {
                                                                                if (glDist.ItmPoGlAmtAssocMember.HasValue)
                                                                                {
                                                                                    accountEncumbranceAmount += glDist.ItmPoGlAmtAssocMember.Value;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                if ((lineItem.ItemReqEntityAssociation != null) && (lineItem.ItemReqEntityAssociation.Any()))
                                                                {
                                                                    foreach (var glDist in lineItem.ItemReqEntityAssociation)
                                                                    {
                                                                        if (glDist != null)
                                                                        {
                                                                            if (glDist.ItmReqGlNoAssocMember == glAccount || budgetPoolAccounts.Contains(glDist.ItmReqGlNoAssocMember))
                                                                            {
                                                                                if (glDist.ItmReqGlAmtAssocMember.HasValue)
                                                                                {
                                                                                    accountRequisitionAmount -= glDist.ItmReqGlAmtAssocMember.Value;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // if the purchase order originated from one or more requisitions and requisition split is allowed,
                                                    // then use the data contracts for the line items on the purchase order to update the encumbrance funds
                                                    // availability amount from the PO line item GL distributions, and update the requisition funds
                                                    // availability amount from the requisition line item GL distributions.
                                                    foreach (var lineItem in lineItemRecords)
                                                    {
                                                        if ((lineItem.ItemPoEntityAssociation != null) && (lineItem.ItemPoEntityAssociation.Any()))
                                                        {
                                                            foreach (var glDist in lineItem.ItemPoEntityAssociation)
                                                            {
                                                                if (glDist != null)
                                                                {
                                                                    if (glDist.ItmPoGlNoAssocMember == glAccount || budgetPoolAccounts.Contains(glDist.ItmPoGlNoAssocMember))
                                                                    {
                                                                        if (glDist.ItmPoGlAmtAssocMember.HasValue)
                                                                        {
                                                                            accountEncumbranceAmount += glDist.ItmPoGlAmtAssocMember.Value;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        if ((lineItem.ItemReqEntityAssociation != null) && (lineItem.ItemReqEntityAssociation.Any()))
                                                        {
                                                            foreach (var glDist in lineItem.ItemReqEntityAssociation)
                                                            {
                                                                if (glDist != null)
                                                                {
                                                                    if (glDist.ItmReqGlNoAssocMember == glAccount || budgetPoolAccounts.Contains(glDist.ItmReqGlNoAssocMember))
                                                                    {
                                                                        if (glDist.ItmReqGlAmtAssocMember.HasValue)
                                                                        {
                                                                            accountRequisitionAmount -= glDist.ItmReqGlAmtAssocMember.Value;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // if the purchase order did not originate from requisitions, use the line item GL distribution
                                            // amounts from the domain entity to update the encumbrance funds availability amount.
                                            foreach (var lineItem in purchaseOrderDomainEntity.LineItems)
                                            {
                                                if (lineItem != null)
                                                {
                                                    foreach (var glDistribution in lineItem.GlDistributions)
                                                    {
                                                        if (glDistribution != null)
                                                        {
                                                            if (glDistribution.GlAccountNumber == glAccount || budgetPoolAccounts.Contains(glDistribution.GlAccountNumber))
                                                            {
                                                                accountEncumbranceAmount += glDistribution.Amount;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // Update the funds availability information on the line item GL distributions, and
                                    // update the line item to indicate that there is GL number on the line item that is
                                    // overbudget.
                                    foreach (var lineItem in purchaseOrderDomainEntity.LineItems)
                                    {
                                        foreach (var glDistribution in lineItem.GlDistributions)
                                        {
                                            if (glDistribution.GlAccountNumber == glAccount)
                                            {
                                                glDistribution.BudgetAmount = accountBudgetAmount;
                                                glDistribution.ActualAmount = accountActualAmount;
                                                glDistribution.EncumbranceAmount = accountEncumbranceAmount;
                                                glDistribution.RequisitionAmount = accountRequisitionAmount;

                                                if ((accountBudgetAmount - accountActualAmount - accountEncumbranceAmount - accountRequisitionAmount) < 0)
                                                {
                                                    lineItem.OverBudget = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        logger.Debug(string.Format("Completed calculating funds availability"));
                    }

                }
            }

            return purchaseOrderDomainEntity;
        }

        private static void PopulateGlAmountFromFaFields(GlAcctsMemos glAccountMemos, out decimal budgetAmount, out decimal actualAmount, out decimal encumbranceAmount, out decimal requisitionAmount)
        {
            budgetAmount = glAccountMemos.FaBudgetPostedAssocMember.HasValue ? glAccountMemos.FaBudgetPostedAssocMember.Value : 0m;
            budgetAmount += glAccountMemos.FaBudgetMemoAssocMember.HasValue ? glAccountMemos.FaBudgetMemoAssocMember.Value : 0m;
            encumbranceAmount = glAccountMemos.FaEncumbrancePostedAssocMember.HasValue ? glAccountMemos.FaEncumbrancePostedAssocMember.Value : 0m;
            encumbranceAmount += glAccountMemos.FaEncumbranceMemoAssocMember.HasValue ? glAccountMemos.FaEncumbranceMemoAssocMember.Value : 0m;
            requisitionAmount = glAccountMemos.FaRequisitionMemoAssocMember.HasValue ? glAccountMemos.FaRequisitionMemoAssocMember.Value : 0m;
            actualAmount = glAccountMemos.FaActualPostedAssocMember.HasValue ? glAccountMemos.FaActualPostedAssocMember.Value : 0m;
            actualAmount += glAccountMemos.FaActualMemoAssocMember.HasValue ? glAccountMemos.FaActualMemoAssocMember.Value : 0m;
        }

        /// <summary>
        /// Determine whether gl access check can be by passed, based on two conditions Requisition status should be "In-Progress" & person id should be either requestor or initiator
        /// </summary>
        /// <param name="personId">PersonId</param>
        /// <param name="purchaseOrders">Purchase Order data contract</param>
        /// <param name="purchaseOrderDomainEntity">Purchase Order domain entity</param>
        /// <returns></returns>
        private static bool CanUserByPassGlAccessCheck(string personId, PurchaseOrders purchaseOrders, PurchaseOrder purchaseOrderDomainEntity)
        {
            if (purchaseOrderDomainEntity.Requisitions == null || (purchaseOrderDomainEntity.Requisitions.Count == 0))
                return ((purchaseOrderDomainEntity.Status == PurchaseOrderStatus.InProgress ) || (purchaseOrderDomainEntity.Status == PurchaseOrderStatus.Voided) || (purchaseOrderDomainEntity.Status == PurchaseOrderStatus.Closed)) && (purchaseOrders.PoRequestor == personId || purchaseOrders.PoDefaultInitiator == personId);
            else
                return false;
        }


        /// <summary>
        /// Get the purchase order requested 
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <returns>Tuple of PurchaseOrder entity objects <see cref="PurchaseOrder"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<PurchaseOrder>, int>> GetPurchaseOrdersAsync(int offset, int limit, string orderNumber)
        {

            int totalCount = 0;

            string purchaseOrdersCacheKey = CacheSupport.BuildCacheKey(AllPurchaseOrdersCache, orderNumber);

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                purchaseOrdersCacheKey,
                "PURCHASE.ORDERS",
                offset,
                limit,
                PurchaseOrdersCacheTimeout,
                async () =>
                {
                    var criteria = "WITH PO.CURRENT.STATUS NE 'U' AND WITH PO.ITEMS.ID NE ''";
                    if (!string.IsNullOrEmpty(orderNumber))
                    {
                        criteria += string.Format(" AND WITH PO.NO = '{0}'", orderNumber);
                    }
                    var requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = criteria
                    };

                    return requirements;
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<PurchaseOrder>, int>(new List<PurchaseOrder>(), 0);
            }

            var subList = keyCacheObject.Sublist.ToArray();
            totalCount = keyCacheObject.TotalCount.Value;

            IEnumerable<PurchaseOrder> purchaseOrders = null;


            var purchaseOrderData = await DataReader.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", subList);

            purchaseOrders = await BuildPurchaseOrdersAsync(purchaseOrderData);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<PurchaseOrder>, int>(purchaseOrders, totalCount);

        }

        /// <summary>
        /// Get PurchaseOrder by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>PurchaseOrder entity object <see cref="PurchaseOrder"/></returns>
        public async Task<PurchaseOrder> GetPurchaseOrdersByGuidAsync(string guid)
        {
            exception = new RepositoryException();
            string id = await GetPurchaseOrdersIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("No purchase-orders was found for guid " + guid);
            }
            var purchaseOrderDataContract = await DataReader.ReadRecordAsync<PurchaseOrders>(id);
            // exclude those PO that are inprogress

            if (purchaseOrderDataContract == null)
            {
                throw new KeyNotFoundException("No purchase-orders was found for GUID " + guid);
            }

            if (purchaseOrderDataContract.PoStatus != null && purchaseOrderDataContract.PoStatus.Any() && purchaseOrderDataContract.PoStatus.FirstOrDefault().Equals("U", StringComparison.OrdinalIgnoreCase))
            {
                exception.AddError(new RepositoryError("Validation.Exception", string.Format("The GUID specified {0} for record key {1} from file PURCHASE.ORDERS is not valid for purchase-orders.", guid, purchaseOrderDataContract.Recordkey)));
                throw exception;
                //throw new KeyNotFoundException(string.Format("The GUID specified " + guid + " for record key " + purchaseOrderDataContract.Recordkey + " from file PURCHASE.ORDERS is not valid for purchase-orders.");
            }

            if ((purchaseOrderDataContract.PoItemsId == null) || (!purchaseOrderDataContract.PoItemsId.Any()))
            {
                exception.AddError(new RepositoryError("Validation.Exception", string.Format("The GUID specified {0} for record key {1} from file PURCHASE.ORDERS is not valid for purchase-orders.", guid, purchaseOrderDataContract.Recordkey)));
                throw exception;
                //throw new KeyNotFoundException("The GUID specified " + guid + " for record key " + purchaseOrderDataContract.Recordkey + " from file PURCHASE.ORDERS is not valid for purchase-orders.");
            }

            PurchaseOrder purchaseOrderDomainEntity = null;

            purchaseOrderDomainEntity = await BuildPurchaseOrderAsync(purchaseOrderDataContract);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return purchaseOrderDomainEntity;

        }

        public async Task<string> GetPurchaseOrdersIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("No purchase-orders was found for guid " + guid);
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("No purchase-orders was found for guid " + guid);
            }

            if (foundEntry.Value.Entity != "PURCHASE.ORDERS")
            {
                exception.AddError(new RepositoryError("GUID.Wrong.Type", string.Format("GUID {0} has different entity, {1}, than expected, PURCHASE.ORDERS", guid, foundEntry.Value.Entity))
                {
                    Id = guid
                });
                throw exception;

            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Gets project ref no's.
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, string>> GetProjectIdsFromReferenceNo(string[] projectRefNo)
        {
            var dict = new Dictionary<string, string>();
            if (projectRefNo == null || !projectRefNo.Any())
            {
                return dict;
            }
            var criteria = "WITH PRJ.REF.NO EQ '" + (string.Join(" ", projectRefNo)).Replace(" ", "' '") + "'";

            var projects = await DataReader.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", criteria);
            if (projects != null && projects.Any())
            {
                foreach (var project in projects)
                {
                    if (!dict.ContainsKey(project.Recordkey))
                    {
                        dict.Add(project.Recordkey, project.PrjRefNo);
                    }
                }
            }
            return dict;
        }

        public async Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();

            try
            {
                var projects = await DataReader.BulkReadRecordAsync<DataContracts.Projects>(projectIds);
                if (projects != null && projects.Any())
                {
                    foreach (var project in projects)
                    {
                        if (!dict.ContainsKey(project.Recordkey))
                        {
                            dict.Add(project.Recordkey, project.PrjRefNo);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do not throw exception.
            }
            return dict;
        }

        /// <summary>
        /// Create / Update a purchase Order.
        /// </summary>       
        /// <param name="PurchaseOrderCreateUpdateRequest">The purchase order create update request domain entity.</param>        
        /// <returns>The purchase order create update response entity</returns>
        public async Task<PurchaseOrderCreateUpdateResponse> CreatePurchaseOrderAsync(PurchaseOrderCreateUpdateRequest createUpdateRequest)
        {
            if (createUpdateRequest == null)
                throw new ArgumentNullException("purchaseOrderEntity", "Must provide a createUpdateRequest to create.");

            PurchaseOrderCreateUpdateResponse response = new PurchaseOrderCreateUpdateResponse();
            var createRequest = BuildPurchaseOrderCreateRequest(createUpdateRequest);

            try
            {
                // write the  data
                var createResponse = await transactionInvoker.ExecuteAsync<TxCreateWebPORequest, TxCreateWebPOResponse>(createRequest);
                if (string.IsNullOrEmpty(createResponse.AError) && (createResponse.AlErrorMessages == null || !createResponse.AlErrorMessages.Any()))
                {
                    response.ErrorOccured = false;
                    response.ErrorMessages = new List<string>();
                    response.PurchaseOrderId = createResponse.APurchaseOrderId;
                    response.PurchaseOrderNumber = createResponse.APurchaseOrderNo;
                    response.PurchaseOrderDate = createResponse.APoDate.Value;
                }
                else
                {
                    response.ErrorOccured = true;
                    response.ErrorMessages = createResponse.AlErrorMessages;
                    response.ErrorMessages.RemoveAll(message => string.IsNullOrEmpty(message));
                }
                response.WarningOccured = (!string.IsNullOrEmpty(createResponse.AWarning) && createResponse.AWarning == "1") ? true : false;
                response.WarningMessages = (createResponse.AlWarningMessages != null || createResponse.AlWarningMessages.Any()) ? createResponse.AlWarningMessages : new List<string>();

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw e;
            }

            return response;
        }

        /// <summary>
        /// Void a purchase Order.
        /// </summary>       
        /// <param name="PurchaseOrderVoidRequest">The purchase order void request domain entity.</param>        
        /// <returns>The purchase order void response entity</returns>
        public async Task<PurchaseOrderVoidResponse> VoidPurchaseOrderAsync(PurchaseOrderVoidRequest voidRequest)
        {
            if (voidRequest == null)
                throw new ArgumentNullException("voidRequestEntity", "Must provide a voidRequest to void a purchases order.");

            PurchaseOrderVoidResponse response = new PurchaseOrderVoidResponse();
            var createRequest = BuildPurchaseOrderVoidRequest(voidRequest);

            try
            {
                // write the  data
                var voidResponse = await transactionInvoker.ExecuteAsync<TxVoidPurchaseOrderRequest, TxVoidPurchaseOrderResponse>(createRequest);
                if (!(voidResponse.AErrorOccurred) && (voidResponse.AlErrorMessages == null || !voidResponse.AlErrorMessages.Any()))
                {
                    response.ErrorOccured = false;
                    response.ErrorMessages = new List<string>();
                }
                else
                {
                    response.ErrorOccured = true;
                    response.ErrorMessages = voidResponse.AlErrorMessages;
                    response.ErrorMessages.RemoveAll(message => string.IsNullOrEmpty(message));
                }

                response.PurchaseOrderId = voidResponse.APurchaseOrderId;
                response.PurchaseOrderNumber = voidResponse.APurchaseOrderNumber;
                response.WarningOccured = voidResponse.AWarningOccurred;
                response.WarningMessages = (voidResponse.AlWarningMessages != null || voidResponse.AlWarningMessages.Any()) ? voidResponse.AlWarningMessages : new List<string>();

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw e;
            }

            return response;
        }

        /// <summary>
        /// Update a Purchase Order.
        /// </summary>       
        /// <param name="purchaseOrderCreateUpdateRequest">The Purchase Order create update request domain entity.</param>        
        /// <returns>The Purchase Order create update response entity</returns>
        public async Task<PurchaseOrderCreateUpdateResponse> UpdatePurchaseOrderAsync(PurchaseOrderCreateUpdateRequest createUpdateRequest, PurchaseOrder originalPurchaseOrder)
        {
            if (createUpdateRequest == null)
                throw new ArgumentNullException("createUpdateRequest", "Must provide a createUpdateRequest entity to update.");

            PurchaseOrderCreateUpdateResponse response = new PurchaseOrderCreateUpdateResponse();
            var updateRequest = BuildPurchaseOrderUpdateRequest(createUpdateRequest, originalPurchaseOrder);

            try
            {
                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<TxUpdateWebPurchaseOrderRequest, TxUpdateWebPurchaseOrderResponse>(updateRequest);
                if (string.IsNullOrEmpty(updateResponse.AError) && (updateResponse.AlErrorMessages == null || !updateResponse.AlErrorMessages.Any()))
                {
                    response.ErrorOccured = false;
                    response.ErrorMessages = new List<string>();
                    response.PurchaseOrderId = updateResponse.APoId;
                    response.PurchaseOrderNumber = createUpdateRequest.PurchaseOrder.Number;
                    response.PurchaseOrderDate = createUpdateRequest.PurchaseOrder.Date;
                }
                else
                {
                    response.ErrorOccured = true;
                    response.ErrorMessages = updateResponse.AlErrorMessages;
                    response.ErrorMessages.RemoveAll(message => string.IsNullOrEmpty(message));
                }
                response.WarningOccured = (!string.IsNullOrEmpty(updateResponse.AWarning) && updateResponse.AWarning == "1") ? true : false;
                response.WarningMessages = (updateResponse.AlWarningMessages != null || updateResponse.AlWarningMessages.Any()) ? updateResponse.AlWarningMessages : new List<string>();

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw e;
            }

            return response;
        }

        /// <summary>
        /// Get a list of purchase order summary domain entity objects
        /// </summary>
        /// <param name="criteria">procurement filter criteria</param>
        /// <returns>list of purchase order summary domain entity objects</returns>
        public async Task<IEnumerable<PurchaseOrderSummary>> QueryPurchaseOrderSummariesAsync(ProcurementDocumentFilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("filterCriteria", "filter criteria must be specified.");
            }

            var personId = criteria.PersonId;
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            var cfWebDefaults = await DataReader.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS");
            string queryCriteria = await BuildFilterCriteria(criteria, cfWebDefaults);
            if (string.IsNullOrEmpty(queryCriteria))
            {
                throw new ApplicationException("Invalid query string.");
            }
            var filteredPurchaseOrderIds = await DataReader.SelectAsync("PURCHASE.ORDERS", queryCriteria);

            if (filteredPurchaseOrderIds == null || !filteredPurchaseOrderIds.Any())
            {
                logger.Debug(string.Format("Purchase orders not found for query string: '{0}'.", queryCriteria));
                return null;
            }
            logger.Debug(string.Format("Purchase orders count {0} found.", filteredPurchaseOrderIds.ToList().Count()));
            return await BuildPurchaseOrderSummaryList(personId, filteredPurchaseOrderIds.ToList());
        }

        /// <summary>
        /// Create an TxUpdateWebPurchaseOrderRequest from a PurchaseOrder domain entity
        /// </summary>
        /// <param name="createUpdateRequest">create/update purchase order request entity</param>
        /// <param name="originalPurchaseOrder">PurchaseOrder domain entity</param>
        /// <returns> TxUpdateWebPurchaseOrderRequest transaction object</returns>
        private TxUpdateWebPurchaseOrderRequest BuildPurchaseOrderUpdateRequest(PurchaseOrderCreateUpdateRequest createUpdateRequest, PurchaseOrder originalPurchaseOrder)
        {
            var request = new TxUpdateWebPurchaseOrderRequest();
            var personId = createUpdateRequest.PersonId;
            var confirmationEmailAddresses = createUpdateRequest.ConfEmailAddresses;
            var purchaseOrderEntity = createUpdateRequest.PurchaseOrder;
            bool isPersonVendor = createUpdateRequest.IsPersonVendor;

            if (!string.IsNullOrEmpty(createUpdateRequest.PersonId))
            {
                request.APersonId = personId;
            }

            if (confirmationEmailAddresses != null && confirmationEmailAddresses.Any())
            {
                request.AlConfEmailAddresses = confirmationEmailAddresses;
            }

            if (!string.IsNullOrEmpty(purchaseOrderEntity.Id) && !purchaseOrderEntity.Id.Equals("NEW"))
            {
                request.APoId = purchaseOrderEntity.Id;
            }
            if (!string.IsNullOrEmpty(purchaseOrderEntity.ShipToCode))
            {
                request.AShipTo = purchaseOrderEntity.ShipToCode;
            }

            if (!string.IsNullOrEmpty(purchaseOrderEntity.ApType))
            {
                request.AApType = purchaseOrderEntity.ApType;
            }

            request.AlPrintedComments = CommentsUtility.ConvertMultiLineTextToList(purchaseOrderEntity.Comments);
            request.AlInternalComments = new List<string>() { purchaseOrderEntity.InternalComments };

            if (originalPurchaseOrder != null && originalPurchaseOrder.DefaultCommodityCode != null)
            {
                request.APoCommodityCode = originalPurchaseOrder.DefaultCommodityCode;
            }

            var lineItems = new List<Data.ColleagueFinance.Transactions.AlUpdatedPoLineItems>();

            if (purchaseOrderEntity.LineItems != null && purchaseOrderEntity.LineItems.Any())
            {
                foreach (var apLineItem in purchaseOrderEntity.LineItems)
                {
                    var lineItem = new Data.ColleagueFinance.Transactions.AlUpdatedPoLineItems();

                    bool addLineItemToModify = true;
                    if (string.IsNullOrEmpty(apLineItem.Id) || apLineItem.Id.Equals("NEW"))
                    {
                        //New line item added
                        lineItem.AlLineItemIds = "";
                    }
                    else
                    {
                        lineItem.AlLineItemIds = apLineItem.Id;
                        var originalLineItem = originalPurchaseOrder.LineItems.FirstOrDefault(x => x.Id == apLineItem.Id);
                        if (originalLineItem != null)
                        {
                            //if user doesnt have access to any of the gl account/s, send only lineItemId to CTX.
                            if (originalLineItem.GlDistributions != null && originalLineItem.GlDistributions.Any())
                            {
                                addLineItemToModify = originalLineItem.GlDistributions.Any(x => x.Masked) ? false : true;
                            }
                            //SS - cannot maintain more than 3 tax codes, so if line item has more than 3  tax codes send the original taxcodes back to CTX.
                            if (originalLineItem.ReqLineItemTaxCodes != null && (originalLineItem.ReqLineItemTaxCodes.Any() && originalLineItem.ReqLineItemTaxCodes.Count > 3))
                            {
                                addLineItemToModify = false;
                            }
                        }
                    }

                    if (addLineItemToModify)
                    {
                        var descriptionList = CommentsUtility.ConvertMultiLineTextToList(apLineItem.Description);
                        lineItem.AlLineItemDescs = string.Join(_SM.ToString(), descriptionList);
                        lineItem.AlLineItemQtys = apLineItem.Quantity.ToString();
                        lineItem.AlItemPrices = apLineItem.Price.ToString();
                        lineItem.AlItemUnitIssues = apLineItem.UnitOfIssue;
                        lineItem.AlItemVendorParts = apLineItem.VendorPart;
                        lineItem.AlItemComments = apLineItem.Comments;
                        lineItem.AlItemTrdDscPcts = apLineItem.TradeDiscountPercentage.HasValue ? apLineItem.TradeDiscountPercentage.Value.ToString() : null;
                        lineItem.AlItemTrdDscAmts = apLineItem.TradeDiscountAmount.HasValue ? apLineItem.TradeDiscountAmount.Value.ToString() : null;
                        lineItem.AlItemFxaFlags = apLineItem.FixedAssetsFlag;

                        lineItem.AlItemCommodityCode = !string.IsNullOrEmpty(apLineItem.CommodityCode) ? apLineItem.CommodityCode : "";

                        if (apLineItem.ExpectedDeliveryDate.HasValue)
                        {
                            lineItem.AlItemDeliveryDate = DateTime.SpecifyKind(apLineItem.ExpectedDeliveryDate.Value.Date, DateTimeKind.Unspecified);
                        }
                        else
                        {
                            lineItem.AlItemDeliveryDate = null;
                        }
                        lineItem.AlItemTaxForm = !string.IsNullOrEmpty(apLineItem.TaxForm) ? apLineItem.TaxForm : "";
                        lineItem.AlItemTaxFormCode = !string.IsNullOrEmpty(apLineItem.TaxFormCode) ? apLineItem.TaxFormCode : "";
                        lineItem.AlItemTaxFormLoc = !string.IsNullOrEmpty(apLineItem.TaxFormLocation) ? apLineItem.TaxFormLocation : "";

                        var taxCodes = new List<string>();
                        foreach (var taxCode in apLineItem.ReqLineItemTaxCodes)
                        {
                            if (!string.IsNullOrEmpty(taxCode.TaxReqTaxCode))
                                taxCodes.Add(taxCode.TaxReqTaxCode);
                        }
                        lineItem.AlItemTaxCodes = string.Join("|", taxCodes);

                        var glAccts = new List<string>();
                        var glDistributionAmounts = new List<decimal?>();
                        var projectNos = new List<string>();
                        foreach (var item in apLineItem.GlDistributions)
                        {
                            glAccts.Add(!string.IsNullOrEmpty(item.GlAccountNumber) ? item.GlAccountNumber : string.Empty);
                            glDistributionAmounts.Add(item.Amount);
                            projectNos.Add(!string.IsNullOrEmpty(item.ProjectNumber) ? item.ProjectNumber : string.Empty);
                        }
                        lineItem.AlItemGlAccts = string.Join("|", glAccts);
                        lineItem.AlItemGlAcctAmts = string.Join("|", glDistributionAmounts);
                        lineItem.AlItemProjectNos = string.Join("|", projectNos);
                        lineItems.Add(lineItem);
                    }
                }
            }
            if (originalPurchaseOrder.LineItems != null && originalPurchaseOrder.LineItems.Any())
            {
                var deletedLineItems = originalPurchaseOrder.LineItems.Where(x => !purchaseOrderEntity.LineItems.Any(u => !string.IsNullOrEmpty(u.Id) && u.Id == x.Id));
                if (deletedLineItems != null && deletedLineItems.Any())
                {
                    foreach (var deletedItem in deletedLineItems)
                    {
                        var deletedLineItem = new Transactions.AlUpdatedPoLineItems();
                        //when user has no access to any of the gl account, nullify the values sent for update.
                        //CTX will validate the GL access against lineItem Id and skips the update.
                        deletedLineItem.AlLineItemIds = deletedItem.Id;
                        deletedLineItem.AlLineItemDescs = string.Empty;
                        deletedLineItem.AlLineItemQtys = string.Empty;
                        deletedLineItem.AlItemPrices = string.Empty;
                        deletedLineItem.AlItemUnitIssues = string.Empty;
                        deletedLineItem.AlItemVendorParts = string.Empty;
                        deletedLineItem.AlItemGlAccts = string.Empty;
                        deletedLineItem.AlItemGlAcctAmts = null;
                        deletedLineItem.AlItemProjectNos = string.Empty;
                        lineItems.Add(deletedLineItem);
                    }

                }
            }
            request.AlUpdatedPoLineItems = lineItems;


            if (!string.IsNullOrEmpty(purchaseOrderEntity.VendorId))
            {
                request.AVendor = purchaseOrderEntity.VendorId;
            }
            else if (!string.IsNullOrEmpty(purchaseOrderEntity.VendorName))
            {
                request.AVendor = purchaseOrderEntity.VendorName;
            }
            request.AVendorIsPersonFlag = isPersonVendor;

            if (purchaseOrderEntity.Approvers != null && purchaseOrderEntity.Approvers.Any())
            {
                request.AlNextApprovers = new List<string>();
                foreach (var nextApprover in purchaseOrderEntity.Approvers)
                {
                    request.AlNextApprovers.Add(nextApprover.ApproverId);
                }
            }

            return request;
        }

        /// <summary>
        ///  Build collection of PurchaseOrder domain entities 
        /// </summary>
        /// <param name="purchaseOrders">Collection of PurchaseOrder data contracts</param>
        /// <returns>PurchaseOrder domain entity</returns>
        private async Task<IEnumerable<PurchaseOrder>> BuildPurchaseOrdersAsync(IEnumerable<DataContracts.PurchaseOrders> purchaseOrders)
        {
            var upperLevelException = new RepositoryException();

            var purchaseOrderCollection = new List<PurchaseOrder>();

            if (purchaseOrders == null || !purchaseOrders.Any())
            {
                return purchaseOrderCollection;
            }

            foreach (var purchaseOrder in purchaseOrders)
            {
                try
                {
                    exception = null;
                    purchaseOrderCollection.Add(await BuildPurchaseOrderAsync(purchaseOrder));
                }
                catch (RepositoryException ex)
                {
                    exception = null;
                    upperLevelException.AddErrors(ex.Errors);
                }
                catch (Exception ex)
                {
                    upperLevelException.AddError(new RepositoryError("Bad.Data", ex.Message)
                    {
                        SourceId = purchaseOrder.Recordkey,
                        Id = purchaseOrder.RecordGuid
                    });
                }
            }

            exception = upperLevelException;

            return purchaseOrderCollection.AsEnumerable();
        }

        /// <summary>
        /// Build Purchase Order domain entity.  This will either be called in a loop to build a collection, or directly for an individual PO      
        /// </summary>
        /// <param name="purchaseOrder">data contact</param>
        /// <param name="personId">personId</param>
        /// <param name="expenseAccounts">expenseAccounts</param>
        /// <returns>A single PurchaseOrder domain entity.</returns>
        private async Task<PurchaseOrder> BuildPurchaseOrderAsync(PurchaseOrders purchaseOrder,
            string personId = "", IEnumerable<string> expenseAccounts = null)
        {
            if (purchaseOrder == null)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Unable to build purchaseOrder. Missing datacontract."));
            }

            string id = purchaseOrder.Recordkey;

            if (string.IsNullOrEmpty(id))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Unable to build purchase order. Missing record id.") { Id = purchaseOrder.RecordGuid } );
            }

            string guid = purchaseOrder.RecordGuid;

            if (string.IsNullOrEmpty(guid))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Unable to build purchase order. Missing record guid.") { SourceId = purchaseOrder.Recordkey } );
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }


            if (expenseAccounts == null)
            {
                expenseAccounts = new List<string>();
            }
            // Translate the status code into a PurchaseOrderStatus enumeration value
            PurchaseOrderStatus? purchaseOrderStatus = null;

            // Get the first status in the list of purchase order statuses and check it has a value.
            if (purchaseOrder.PoStatus != null && !string.IsNullOrEmpty(purchaseOrder.PoStatus.FirstOrDefault()))
            {
                purchaseOrderStatus = GetPurchaseOrderStatus(purchaseOrder.PoStatus.FirstOrDefault(), purchaseOrder.Recordkey);
                // Both PO status and Line Item status use the same method to get a status.
                // Do not allow a status of "H" on the PO even though a Line item status can have this status.
                if (purchaseOrderStatus == null)
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", string.Concat("Invalid purchase order status for purchase order '", purchaseOrder.PoNo, "'.  Status: '", purchaseOrder.PoStatus.FirstOrDefault(), "'," +
                        " Entity: 'PURCHASE.ORDERS'")) { Id = purchaseOrder.RecordGuid, SourceId = purchaseOrder.Recordkey } );
                }
            }
            else
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Missing status for purchase order '" + purchaseOrder.PoNo + "', Entity: 'PURCHASE.ORDERS'") { Id = purchaseOrder.RecordGuid, SourceId = purchaseOrder.Recordkey } );
            }

            string purchaseOrderVendorName = "";

            if (purchaseOrder.PoStatusDate == null || !purchaseOrder.PoStatusDate.Any() || !purchaseOrder.PoStatusDate.First().HasValue)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Missing status date for purchase order '" + purchaseOrder.PoNo + "', Entity: 'PURCHASE.ORDERS'") { Id = purchaseOrder.RecordGuid, SourceId = purchaseOrder.Recordkey } );
            }

            if (!purchaseOrder.PoDate.HasValue)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Missing date for purchase order '" + purchaseOrder.PoNo + "', Entity: 'PURCHASE.ORDERS'") { Id = purchaseOrder.RecordGuid, SourceId = purchaseOrder.Recordkey } );
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            // The purchase order status date contains one to many dates
            var purchaseOrderStatusDate = purchaseOrder.PoStatusDate.First().Value;

            PurchaseOrder purchaseOrderDomainEntity = null;
            try
            {
                purchaseOrderDomainEntity = new PurchaseOrder(purchaseOrder.Recordkey, purchaseOrder.RecordGuid, purchaseOrder.PoNo, purchaseOrderVendorName, purchaseOrderStatus, purchaseOrderStatusDate, purchaseOrder.PoDate.Value.Date);
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                var errorMessage = string.Format("Invalid Purchase Order, Entity: 'PURCHASE.ORDERS', Record ID: '{0}'. {1}", purchaseOrder.Recordkey, ex.Message);
                exception.AddError(new RepositoryError("Bad.Data", errorMessage) { Id = purchaseOrder.RecordGuid, SourceId = purchaseOrder.Recordkey });
                throw exception;
            }
            purchaseOrderDomainEntity.Type = purchaseOrder.PoIntgType;
            purchaseOrderDomainEntity.SubmittedBy = purchaseOrder.PoIntgSubmittedBy;

            // Existing Vendor info
            purchaseOrderDomainEntity.VendorId = purchaseOrder.PoVendor;
            purchaseOrderDomainEntity.VendorAddressId = purchaseOrder.PoIntgAddressId;

            if (!(string.IsNullOrEmpty(purchaseOrder.PoDefaultInitiator)))
            {
                var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>("PERSON", purchaseOrder.PoDefaultInitiator);
                if (personRecord != null)
                {
                    var initiatorName = string.Concat(personRecord.FirstName, " ", personRecord.LastName);
                    if (!string.IsNullOrWhiteSpace(initiatorName))
                    {
                        purchaseOrderDomainEntity.InitiatorName = initiatorName;
                    }
                }
            }


            purchaseOrderDomainEntity.Amount = 0;
            purchaseOrderDomainEntity.CurrencyCode = purchaseOrder.PoCurrencyCode;
            if (purchaseOrder.PoMaintGlTranDate.HasValue)
            {
                purchaseOrderDomainEntity.MaintenanceDate = purchaseOrder.PoMaintGlTranDate.Value.Date;
            }

            if (purchaseOrder.PoExpectedDeliveryDate.HasValue)
            {
                purchaseOrderDomainEntity.DeliveryDate = purchaseOrder.PoExpectedDeliveryDate.Value.Date;
            }
            purchaseOrderDomainEntity.ApType = purchaseOrder.PoApType;
            purchaseOrderDomainEntity.Comments = purchaseOrder.PoPrintedComments;
            purchaseOrderDomainEntity.InternalComments = purchaseOrder.PoComments;
            purchaseOrderDomainEntity.Buyer = purchaseOrder.PoBuyer;
            purchaseOrderDomainEntity.HostCountry = await GetHostCountryAsync();
            purchaseOrderDomainEntity.VendorTerms = purchaseOrder.PoVendorTerms;

            purchaseOrderDomainEntity.ReferenceNo = purchaseOrder.PoReferenceNo;

            // Get the ship to code or the default ship to code.
            purchaseOrderDomainEntity.ShipToCode = purchaseOrder.PoShipTo;
            if (string.IsNullOrEmpty(purchaseOrder.PoShipTo))
            {
                var purchasingDefaultsDataContract = await DataReader.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS");
                if (purchasingDefaultsDataContract != null)
                {
                    purchaseOrderDomainEntity.ShipToCode = purchasingDefaultsDataContract.PurShipToCode;
                }
            }

            // Ship Via (Shipping Method)
            purchaseOrderDomainEntity.ShipViaCode = purchaseOrder.PoShipVia;

            //Alternative Shipping 
            purchaseOrderDomainEntity.AltShippingName = purchaseOrder.PoAltShipName;
            purchaseOrderDomainEntity.AltShippingAddress = purchaseOrder.PoAltShipAddress;
            purchaseOrderDomainEntity.AltShippingCity = purchaseOrder.PoAltShipCity;
            purchaseOrderDomainEntity.AltShippingState = purchaseOrder.PoAltShipState;
            purchaseOrderDomainEntity.AltShippingZip = purchaseOrder.PoAltShipZip;
            purchaseOrderDomainEntity.AltShippingCountry = purchaseOrder.PoIntgAltShipCountry;
            purchaseOrderDomainEntity.AltShippingPhone = purchaseOrder.PoAltShipPhone;
            purchaseOrderDomainEntity.AltShippingPhoneExt = purchaseOrder.PoAltShipExt;


            //Misc vendor information
            purchaseOrderDomainEntity.AltAddressFlag = purchaseOrder.PoAltFlag == "Y" ? true : false;
            purchaseOrderDomainEntity.MiscIntgCorpPersonFlag = purchaseOrder.PoIntgCorpPersonIndicato;
            purchaseOrderDomainEntity.MiscCountry = purchaseOrder.PoMiscCountry;
            purchaseOrderDomainEntity.MiscName = purchaseOrder.PoMiscName;
            purchaseOrderDomainEntity.MiscAddress = purchaseOrder.PoMiscAddress;
            purchaseOrderDomainEntity.MiscCity = purchaseOrder.PoMiscCity;
            purchaseOrderDomainEntity.MiscState = purchaseOrder.PoMiscState;
            purchaseOrderDomainEntity.MiscZip = purchaseOrder.PoMiscZip;

            //Required for Procurement Receipts
            purchaseOrderDomainEntity.OutstandingItemsId = purchaseOrder.PoOutstandingItemsId;
            purchaseOrderDomainEntity.AcceptedItemsId = purchaseOrder.PoAcceptedItemsId;

            if (!string.IsNullOrWhiteSpace(purchaseOrderDomainEntity.VendorAddressId) && purchaseOrderDomainEntity.AltAddressFlag == true)
            {
                var address = await DataReader.ReadRecordAsync<Address>(purchaseOrderDomainEntity.VendorAddressId);
                if (address != null)
                {
                    int counter = 0;
                    foreach(var addLine in address.AddressLines)
                    {
                        if (purchaseOrderDomainEntity.MiscAddress.Count >= counter)
                        {
                            if (addLine != purchaseOrderDomainEntity.MiscAddress[counter])
                            {
                                purchaseOrderDomainEntity.VendorAddressId = "";
                                break;
                            }
                        } else
                        {
                            purchaseOrderDomainEntity.VendorAddressId = "";
                            break;
                        }
                        counter++;
                    }
                    if (counter < purchaseOrderDomainEntity.MiscAddress.Count)
                        purchaseOrderDomainEntity.VendorAddressId = "";


                    if (address.Country != purchaseOrderDomainEntity.MiscCountry)
                        purchaseOrderDomainEntity.VendorAddressId = "";

                    if (address.City != purchaseOrderDomainEntity.MiscCity)
                        purchaseOrderDomainEntity.VendorAddressId = "";

                    if (address.State != purchaseOrderDomainEntity.MiscState)
                        purchaseOrderDomainEntity.VendorAddressId = "";

                    if (address.Zip != purchaseOrderDomainEntity.MiscZip)
                        purchaseOrderDomainEntity.VendorAddressId = "";
                }
            }


            purchaseOrderDomainEntity.DefaultInitiator = purchaseOrder.PoDefaultInitiator;

            purchaseOrderDomainEntity.VoidGlTranDate = purchaseOrder.PoVoidGlTranDate;

            // Add any associated requisitions to the purchase order domain entity
            if ((purchaseOrder.PoReqIds != null) && (purchaseOrder.PoReqIds.Count > 0))
            {
                foreach (var requisitionId in purchaseOrder.PoReqIds)
                {
                    if (!string.IsNullOrEmpty(requisitionId))
                    {
                        purchaseOrderDomainEntity.AddRequisition(requisitionId);
                    }
                }
            }

            // Add any associated vouchers to the purchase order domain entity
            if ((purchaseOrder.PoVouIds != null) && (purchaseOrder.PoVouIds.Count > 0))
            {
                foreach (var voucherNumber in purchaseOrder.PoVouIds)
                {
                    if (!string.IsNullOrEmpty(voucherNumber))
                    {
                        purchaseOrderDomainEntity.AddVoucher(voucherNumber);
                    }
                }
            }

            purchaseOrderDomainEntity.Fob = purchaseOrder.PoFob;

            // Populate the line item domain entities and add them to the purchase order domain entity
            var lineItemIds = purchaseOrder.PoItemsId;
            if (lineItemIds == null || !lineItemIds.Any())
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", string.Format("Line Items are missing for Purchase Order '{0}', Entity: 'PURCHASE.ORDERS'.", purchaseOrder.PoNo))
                { Id = purchaseOrder.RecordGuid, SourceId = purchaseOrder.Recordkey });
            }
            else
            {

                await GetLineItems(expenseAccounts, purchaseOrder, purchaseOrderDomainEntity, lineItemIds);
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }


            return purchaseOrderDomainEntity;
        }

        private async Task GetLineItems(IEnumerable<string> expenseAccounts, PurchaseOrders purchaseOrder, PurchaseOrder purchaseOrderDomainEntity, List<string> lineItemIds)
        {

            // Read the item records for the list of IDs in the purchase order record
            var lineItemRecords = await DataReader.BulkReadRecordAsync<Items>(lineItemIds.ToArray());
            if ((lineItemRecords != null) && (lineItemRecords.Any()))
            {

                List<string> glAccountsAllowed = new List<string>();

                List<string> itemProjectIds = new List<string>();
                List<string> itemProjectLineIds = new List<string>();

                // If this purchase order has a currency code, the purchase order amount has to be in foreign currency.
                // We can only obtain that amount by adding up the foreign amounts from its items, so we still have to
                // process the item information, regardless of GL account security.

                foreach (var lineItem in lineItemRecords)
                {
                    // The item description is a list of strings                     
                    string itemDescription = string.Empty;
                    bool firstDescPos = true;
                    foreach (var desc in lineItem.ItmDesc)
                    {
                        if (lineItem.ItmDesc.Count() > 1)
                        {
                            // If it is not a blank line, added it to the string.
                            // We are going to display all description as it if were one paragraph
                            // even if the user entered it in different paragraphs.
                            if (desc.Length > 0)
                            {
                                if (firstDescPos == true)
                                {
                                    firstDescPos = false;
                                    itemDescription = desc;
                                }
                                else
                                {
                                    itemDescription += ' ' + desc;
                                }
                            }
                        }
                        else
                        {
                            // If the line item description is just one line, don't add a space at the end of it.
                            itemDescription = desc;
                        }
                    }

                    decimal itemQuantity = lineItem.ItmPoQty.HasValue ? lineItem.ItmPoQty.Value : 0;
                    // Accepted line items will have an accepted quantity and accepted price so
                    // if we have accepted values, then use them (regardless of status).
                    if (lineItem.ItmAcceptedQty.HasValue)
                    {
                        itemQuantity = lineItem.ItmAcceptedQty.Value;
                    }
                    decimal itemPrice = lineItem.ItmPoPrice.HasValue ? lineItem.ItmPoPrice.Value : 0;
                    if (lineItem.ItmAcceptedPrice.HasValue)
                    {
                        itemPrice = lineItem.ItmAcceptedPrice.Value;
                    }
                    decimal extendedPrice = lineItem.ItmPoExtPrice.HasValue ? lineItem.ItmPoExtPrice.Value : 0;
                    // If the integration type is "travel" then make sure unit price is $1.00 and quantity
                    // represents the actual dollar amount of the purchase order.
                    if (!string.IsNullOrEmpty(purchaseOrder.PoIntgType) && purchaseOrder.PoIntgType.ToLower() == "travel" && itemPrice != 1)
                    {
                        var errorMessage = string.Format("Invalid Price/Quantity on Travel Purchase Order: '{0}', Entity: 'PURCHASE.ORDERS', Record ID: {1}", purchaseOrder.PoNo, purchaseOrder.Recordkey);
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", errorMessage) { Id = purchaseOrder.RecordGuid, SourceId = purchaseOrder.Recordkey });
                    }

                    LineItem lineItemDomainEntity = null;
                    try
                    {
                        lineItemDomainEntity = new LineItem(lineItem.Recordkey, itemDescription, itemQuantity, itemPrice, extendedPrice);
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Format("Invalid line item on Purchase Order, Entity: 'ITEMS', Record ID: '{0}'. {1}", lineItem.Recordkey, ex.Message);
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", errorMessage) { Id = purchaseOrder.RecordGuid, SourceId = purchaseOrder.Recordkey });
                    }

                    if (lineItemDomainEntity != null)
                    {
                        if (lineItem.ItmExpectedDeliveryDate != null)
                        {
                            lineItemDomainEntity.ExpectedDeliveryDate = lineItem.ItmExpectedDeliveryDate.Value.Date;
                        }
                        if (lineItem.ItmDesiredDeliveryDate != null)
                        {
                            lineItemDomainEntity.DesiredDate = lineItem.ItmDesiredDeliveryDate.Value.Date;
                        }
                        lineItemDomainEntity.UnitOfIssue = lineItem.ItmPoIssue;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;
                        lineItemDomainEntity.TaxForm = lineItem.ItmTaxForm;
                        lineItemDomainEntity.TaxFormCode = lineItem.ItmTaxFormCode;
                        lineItemDomainEntity.TaxFormLocation = lineItem.ItmTaxFormLoc;
                        lineItemDomainEntity.Comments = lineItem.ItmComments;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;

                        lineItemDomainEntity.CommodityCode = lineItem.ItmCommodityCode;
                        lineItemDomainEntity.FixedAssetsFlag = lineItem.ItmFixedAssetsFlag;
                        lineItemDomainEntity.TradeDiscountAmount = lineItem.ItmPoTradeDiscAmt;
                        lineItemDomainEntity.TradeDiscountPercentage = lineItem.ItmPoTradeDiscPct;
                        lineItemDomainEntity.RequisitionId = lineItem.ItmReqId;

                        if (lineItem.ItemPoStatusEntityAssociation != null && lineItem.ItemPoStatusEntityAssociation.Any())
                        {
                            // Current status is always in the first position in the association.
                            var poStatus = lineItem.ItemPoStatusEntityAssociation.FirstOrDefault();
                            if (poStatus != null)
                            {
                                if (!string.IsNullOrEmpty(poStatus.ItmPoStatusAssocMember))
                                {
                                    lineItemDomainEntity.LineItemStatus = GetLineItemStatus(poStatus.ItmPoStatusAssocMember, purchaseOrder.Recordkey);
                                }
                                if (poStatus.ItmPoStatusDateAssocMember != null && poStatus.ItmPoStatusDateAssocMember.HasValue)
                                {
                                    lineItemDomainEntity.StatusDate = poStatus.ItmPoStatusDateAssocMember;
                                }
                            }
                        }
                        // Populate the GL distribution domain entities and add them to the line items
                        if ((lineItem.ItemPoEntityAssociation != null) && (lineItem.ItemPoEntityAssociation.Count > 0))
                        {
                            foreach (var glDist in lineItem.ItemPoEntityAssociation)
                            {
                                // The GL Distribution always uses the local currency amount.
                                decimal gldistGlQty = glDist.ItmPoGlQtyAssocMember.HasValue ? glDist.ItmPoGlQtyAssocMember.Value : 0;
                                //decimal gldistGlAmount = glDist.ItmPoGlAmtAssocMember.HasValue ? glDist.ItmPoGlAmtAssocMember.Value : 0;
                                decimal gldistGlAmount = 0;

                                if (string.IsNullOrEmpty(purchaseOrder.PoCurrencyCode))
                                {
                                    gldistGlAmount = glDist.ItmPoGlAmtAssocMember.HasValue ? glDist.ItmPoGlAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    gldistGlAmount = glDist.ItmPoGlForeignAmtAssocMember.HasValue ? glDist.ItmPoGlForeignAmtAssocMember.Value : 0;
                                }

                                decimal gldistGlPercent = glDist.ItmPoGlPctAssocMember.HasValue ? glDist.ItmPoGlPctAssocMember.Value : 0;
                                LineItemGlDistribution glDistribution = new LineItemGlDistribution(glDist.ItmPoGlNoAssocMember, gldistGlQty, gldistGlAmount, gldistGlPercent);

                                if (!(string.IsNullOrEmpty(glDist.ItmPoProjectCfIdAssocMember)))
                                {
                                    glDistribution.ProjectId = glDist.ItmPoProjectCfIdAssocMember;
                                    if (!itemProjectIds.Contains(glDist.ItmPoProjectCfIdAssocMember))
                                    {
                                        itemProjectIds.Add(glDist.ItmPoProjectCfIdAssocMember);
                                    }
                                }

                                if (!(string.IsNullOrEmpty(glDist.ItmPoPrjItemIdsAssocMember)))
                                {
                                    glDistribution.ProjectLineItemId = glDist.ItmPoPrjItemIdsAssocMember;
                                    if (!itemProjectLineIds.Contains(glDist.ItmPoPrjItemIdsAssocMember))
                                    {
                                        itemProjectLineIds.Add(glDist.ItmPoPrjItemIdsAssocMember);
                                    }
                                }

                                lineItemDomainEntity.AddGlDistribution(glDistribution);

                                // Check the currency code to see if we need the local or foreign amount
                                if (string.IsNullOrEmpty(purchaseOrder.PoCurrencyCode))
                                {
                                    purchaseOrderDomainEntity.Amount += glDist.ItmPoGlAmtAssocMember.HasValue ? glDist.ItmPoGlAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    purchaseOrderDomainEntity.Amount += glDist.ItmPoGlForeignAmtAssocMember.HasValue ? glDist.ItmPoGlForeignAmtAssocMember.Value : 0;
                                }
                            }
                        }

                        // Add taxes to the line item
                        if ((lineItem.PoGlTaxesEntityAssociation != null) && (lineItem.PoGlTaxesEntityAssociation.Count > 0))
                        {
                            foreach (var taxGlDist in lineItem.PoGlTaxesEntityAssociation)
                            {
                                decimal itemTaxAmount = 0;
                                string lineItemTaxCode = taxGlDist.ItmPoGlTaxCodeAssocMember;

                                if (taxGlDist.ItmPoGlForeignTaxAmtAssocMember.HasValue)
                                {
                                    itemTaxAmount = taxGlDist.ItmPoGlForeignTaxAmtAssocMember.HasValue ? taxGlDist.ItmPoGlForeignTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    itemTaxAmount = taxGlDist.ItmPoGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmPoGlTaxAmtAssocMember.Value : 0;
                                }

                                //LineItemTax itemTax = new LineItemTax(lineItemTaxCode, itemTaxAmount);
                                //taxGlDist.ItmVouGlTaxCodeAssocMember
                                LineItemTax itemTax = new LineItemTax(taxGlDist.ItmPoGlTaxCodeAssocMember,
                                itemTaxAmount)
                                {
                                    TaxGlNumber = taxGlDist.ItmPoTaxGlNoAssocMember,
                                    LineGlNumber = taxGlDist.ItmPoLineGlNoAssocMember
                                };

                                lineItemDomainEntity.AddTaxByGL(itemTax);

                                if (string.IsNullOrEmpty(purchaseOrder.PoCurrencyCode))
                                {
                                    purchaseOrderDomainEntity.Amount += taxGlDist.ItmPoGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmPoGlTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    purchaseOrderDomainEntity.Amount += taxGlDist.ItmPoGlForeignTaxAmtAssocMember.HasValue ? taxGlDist.ItmPoGlForeignTaxAmtAssocMember.Value : 0;
                                }
                            }
                        }

                        purchaseOrderDomainEntity.AddLineItem(lineItemDomainEntity);
                    }
                }

                // If there are project IDs, we need to get the project number,
                // and also the project line item code for each project line item ID 
                if ((itemProjectIds != null) && (itemProjectIds.Count > 0))
                {
                    // For each project ID, get the project number
                    var projectRecords = await DataReader.BulkReadRecordAsync<DataContracts.Projects>(itemProjectIds.ToArray());

                    // If there are project IDs, there should be project line item IDs
                    if ((itemProjectLineIds != null) && (itemProjectLineIds.Count > 0))
                    {
                        // For each project line item ID, get the project line item code
                        var projectLineItemRecords = await DataReader.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(itemProjectLineIds.ToArray());

                        if ((projectRecords != null) && (projectRecords.Count > 0))
                        {
                            for (int i = 0; i < purchaseOrderDomainEntity.LineItems.Count(); i++)
                            {
                                foreach (var glDist in purchaseOrderDomainEntity.LineItems[i].GlDistributions)
                                {
                                    // Only populate project information if the GL account is not masked.

                                    if (glDist.Masked == false)
                                    {
                                        foreach (var project in projectRecords)
                                        {
                                            if (project.Recordkey == glDist.ProjectId)
                                            {
                                                glDist.ProjectNumber = project.PrjRefNo;
                                            }
                                        }

                                        if ((projectLineItemRecords != null) && (projectLineItemRecords.Count > 0))
                                        {
                                            foreach (var projectItem in projectLineItemRecords)
                                            {
                                                if (projectItem.Recordkey == glDist.ProjectLineItemId)
                                                {
                                                    glDist.ProjectLineItemCode = projectItem.PrjlnProjectItemCode;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private LineItemStatus? GetLineItemStatus(string status, string recordId)
        {
            LineItemStatus? purchaseOrderLineItemStatus = null;
            if (string.IsNullOrEmpty(status))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", string.Concat("Purchase Order Line item Status is required. Entity: 'ITEMS', Record ID: '", recordId, "'")));
            }
            switch (status.ToUpper())
            {
                case "A":
                    purchaseOrderLineItemStatus = LineItemStatus.Accepted;
                    break;
                case "B":
                    purchaseOrderLineItemStatus = LineItemStatus.Backordered;
                    break;
                case "C":
                    purchaseOrderLineItemStatus = LineItemStatus.Closed;
                    break;
                case "I":
                    purchaseOrderLineItemStatus = LineItemStatus.Invoiced;
                    break;
                case "O":
                    purchaseOrderLineItemStatus = LineItemStatus.Outstanding;
                    break;
                case "P":
                    purchaseOrderLineItemStatus = LineItemStatus.Paid;
                    break;
                case "R":
                    purchaseOrderLineItemStatus = LineItemStatus.Reconciled;
                    break;
                case "V":
                    purchaseOrderLineItemStatus = LineItemStatus.Voided;
                    break;
                case "H":
                    purchaseOrderLineItemStatus = LineItemStatus.Hold;
                    break;
                default:
                    {
                        // if we get here, we have corrupt data.
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat("Invalid line item status for purchase order.  Status: '", status, "', Entity: 'ITEMS', Record ID: '", recordId, "'")));
                        break;
                    }
            }

            return purchaseOrderLineItemStatus;
        }

        private  PurchaseOrderStatus? GetPurchaseOrderStatus(string status, string recordId)
        {
            PurchaseOrderStatus? purchaseOrderStatus = null;
            if (string.IsNullOrEmpty(status))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", string.Concat("Purchase Order Status is required. Entity: 'PURCHASE.ORDERS', Record ID: '", recordId , "'")));
            }
            switch (status.ToUpper())
            {
                case "A":
                    purchaseOrderStatus = PurchaseOrderStatus.Accepted;
                    break;
                case "B":
                    purchaseOrderStatus = PurchaseOrderStatus.Backordered;
                    break;
                case "C":
                    purchaseOrderStatus = PurchaseOrderStatus.Closed;
                    break;
                case "U":
                    purchaseOrderStatus = PurchaseOrderStatus.InProgress;
                    break;
                case "I":
                    purchaseOrderStatus = PurchaseOrderStatus.Invoiced;
                    break;
                case "N":
                    purchaseOrderStatus = PurchaseOrderStatus.NotApproved;
                    break;
                case "O":
                    purchaseOrderStatus = PurchaseOrderStatus.Outstanding;
                    break;
                case "P":
                    purchaseOrderStatus = PurchaseOrderStatus.Paid;
                    break;
                case "R":
                    purchaseOrderStatus = PurchaseOrderStatus.Reconciled;
                    break;
                case "V":
                    purchaseOrderStatus = PurchaseOrderStatus.Voided;
                    break;
                default:
                    {
                        // if we get here, we have corrupt data.
                        if (exception == null)
                            exception = new RepositoryException();
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat("Invalid purchase order status for purchase order.  Status: '", status, "', Entity: 'PURCHASE.ORDERS', Record ID: '", recordId, "'")));
                        break;
                    }
            }

            return purchaseOrderStatus;
        }

        /// <summary>
        /// Get Host Country from international parameters
        /// </summary>
        /// <returns>HOST.COUNTRY</returns>
        private async Task<string> GetHostCountryAsync()
        {
            if (_internationalParameters == null)
                _internationalParameters = await GetInternationalParametersAsync();
            return _internationalParameters.HostCountry;
        }

        /// <summary>
        /// Create an UpdateCreatePoRequest from a PurchaseOrder domain entity
        /// </summary>
        /// <param name="PurchaseOrderEntity">PurchaseOrder domain entity</param>
        /// <returns> UpdateCreatePoRequest transaction object</returns>
        private UpdateCreatePoRequest BuildPurchaseOrderUpdateRequest(PurchaseOrder PurchaseOrderEntity)
        {
            string poId = PurchaseOrderEntity.Id;
            if (PurchaseOrderEntity.Id == "NEW") { poId = null; }
            var request = new UpdateCreatePoRequest
            {
                PoId = poId,
                Guid = PurchaseOrderEntity.Guid,
                PopulateTaxForm = PurchaseOrderEntity.bypassTaxForms,
                BypassApprovalFlag = PurchaseOrderEntity.bypassApprovals
            };

            if (PurchaseOrderEntity.Date != null)
            {
                request.OrderOn = PurchaseOrderEntity.Date;
            }
            if (PurchaseOrderEntity.MaintenanceDate.HasValue)
                request.TransactionDate = PurchaseOrderEntity.MaintenanceDate;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.Type))
                request.Type = PurchaseOrderEntity.Type;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.Number))
                request.OrderNumber = PurchaseOrderEntity.Number;

            if (request.OrderNumber == "new") { request.OrderNumber = ""; }

            if (PurchaseOrderEntity.ReferenceNo != null && PurchaseOrderEntity.ReferenceNo.Any())
                request.ReferenceNumbers = PurchaseOrderEntity.ReferenceNo;

            if (PurchaseOrderEntity.DeliveryDate.HasValue)
                request.DeliveredBy = PurchaseOrderEntity.DeliveryDate;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.CurrencyCode))
                request.Currency = PurchaseOrderEntity.CurrencyCode;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.Buyer))
                request.BuyerId = PurchaseOrderEntity.Buyer;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.DefaultInitiator))
                request.InitiatorId = PurchaseOrderEntity.DefaultInitiator;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.ShipToCode))
                request.ShipToId = PurchaseOrderEntity.ShipToCode;

            request.Status = PurchaseOrderEntity.Status.ToString();

            if(PurchaseOrderEntity.StatusDate != default(DateTime))
                request.StatusDate = PurchaseOrderEntity.StatusDate;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.Fob))
                request.FreeOnBoardId = PurchaseOrderEntity.Fob;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.AltShippingName))
                request.OverrideDesc = PurchaseOrderEntity.AltShippingName;

            if (PurchaseOrderEntity.AltShippingAddress != null && PurchaseOrderEntity.AltShippingAddress.Any())
                request.OverrideAddressLines = PurchaseOrderEntity.AltShippingAddress;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.AltShippingCountry))
                request.OverridePlaceCountry = PurchaseOrderEntity.AltShippingCountry;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.AltShippingState))
                request.OverrideRegionCode = PurchaseOrderEntity.AltShippingState;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.AltShippingCity))
                request.OverrideLocality = PurchaseOrderEntity.AltShippingCity;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.AltShippingZip))
                request.OverridePostalCode = PurchaseOrderEntity.AltShippingZip;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.AltShippingPhoneExt))
                request.OverrideContactExt = PurchaseOrderEntity.AltShippingPhoneExt;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.AltShippingPhone))
                request.OverrideContactNumber = PurchaseOrderEntity.AltShippingPhone;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.ShipViaCode))
                request.ShippingMethodId = PurchaseOrderEntity.ShipViaCode;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.VendorId))
                request.VendorId = PurchaseOrderEntity.VendorId;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.VendorAddressId))
                request.VendorAddressId = PurchaseOrderEntity.VendorAddressId;

            if (PurchaseOrderEntity.MiscName != null && PurchaseOrderEntity.MiscName.Any())
                request.ManualVendor = PurchaseOrderEntity.MiscName[0];

            if (PurchaseOrderEntity.MiscAddress != null && PurchaseOrderEntity.MiscAddress.Any())
                request.MiscAddress = PurchaseOrderEntity.MiscAddress;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.MiscCountry))
                request.MiscCountry = PurchaseOrderEntity.MiscCountry;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.MiscState))
                request.MiscState = PurchaseOrderEntity.MiscState;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.MiscCity))
                request.MiscCity = PurchaseOrderEntity.MiscCity;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.MiscZip) && PurchaseOrderEntity.MiscZip != "0")
                request.MiscPostalCode = PurchaseOrderEntity.MiscZip;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.MiscIntgCorpPersonFlag))
                request.MiscCoprIndicator = PurchaseOrderEntity.MiscIntgCorpPersonFlag;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.VendorTerms))
                request.PaymentTermsId = PurchaseOrderEntity.VendorTerms;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.ApType))
                request.PaymentSourceId = PurchaseOrderEntity.ApType;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.SubmittedBy))
                request.SubmittedBy = PurchaseOrderEntity.SubmittedBy;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.InternalComments))
                request.Comments = PurchaseOrderEntity.InternalComments;

            if (!string.IsNullOrEmpty(PurchaseOrderEntity.Comments))
                request.PrintedComments = PurchaseOrderEntity.Comments;

            var lineItems = new List<Transactions.PoLineItems>();

            if (PurchaseOrderEntity.LineItems != null && PurchaseOrderEntity.LineItems.Any())
            {
                foreach (var apLineItem in PurchaseOrderEntity.LineItems)
                {

                    var lineItem = new Transactions.PoLineItems()
                    {
                        ItemsDesc= apLineItem.Description,
                        ItemsCommodityCode = apLineItem.CommodityCode,
                        ItemsFixedAssetsFlag = apLineItem.FixedAssetsFlag,
                        ItemsPartNumber = apLineItem.VendorPart,
                        // Line items have both expected date and desired date.  We want to use expected date.
                        // ItemsDesiredDate = apLineItem.DesiredDate,
                        ItemsDesiredDate = apLineItem.ExpectedDeliveryDate,
                        ItemsQuantity = apLineItem.Quantity.ToString(),
                        ItemsUnitsOfMeasuredId = apLineItem.UnitOfIssue,
                        ItemsPrice = apLineItem.Price,
                        ItemsTradeDiscAmt = apLineItem.TradeDiscountAmount,
                        ItemsTradeDiscountPercent = apLineItem.TradeDiscountPercentage.ToString(),
                        ItemsComments = apLineItem.Comments
                    };

                    if (!string.IsNullOrWhiteSpace(apLineItem.Id))
                    {
                        lineItem.ItemsNo = apLineItem.Id;
                    } else { lineItem.ItemsNo = "NEW"; }

                    lineItem.ItemsStatus = apLineItem.LineItemStatus.ToString();
                    switch (apLineItem.LineItemStatus)
                    {
                        case LineItemStatus.Accepted:
                            lineItem.ItemsStatus = "A";
                            break;
                        case LineItemStatus.Closed:
                            lineItem.ItemsStatus = "C";
                            break;
                        case LineItemStatus.Outstanding:
                            lineItem.ItemsStatus = "O";
                            break;
                        case LineItemStatus.Voided:
                            lineItem.ItemsStatus = "V";
                            break;
                    }
                    lineItem.ItemsStatusDate = apLineItem.StatusDate;
                    lineItem.ItemsRequisitionId = apLineItem.RequisitionId;

                    if (apLineItem.LineItemTaxes != null && apLineItem.LineItemTaxes.Any())
                    {
                        var taxCodes = new List<string>();
                        foreach (var lineItemTaxes in apLineItem.LineItemTaxes)
                        {
                            if (!string.IsNullOrEmpty(lineItemTaxes.TaxCode))
                                taxCodes.Add(lineItemTaxes.TaxCode);
                        }
                        lineItem.ItemsTaxCodes = string.Join("|", taxCodes);
                    }

                    lineItem.ItemsTaxForm = apLineItem.TaxForm;
                    lineItem.ItemsTaxFormBox = apLineItem.TaxFormCode;
                    lineItem.ItemsTaxFormLoc = apLineItem.TaxFormLocation;

                    if (apLineItem.GlDistributions != null && apLineItem.GlDistributions.Any())
                    {
                        var gl = new List<string>();
                        var adAmts = new List<decimal?>();
                        var adPct = new List<decimal?>();
                        var adQty = new List<decimal?>();
                        var adSub = new List<string>();
                        foreach (var glDistribution in apLineItem.GlDistributions)
                        {
                            gl.Add(glDistribution.GlAccountNumber);
                            adAmts.Add(glDistribution.Amount);
                            adPct.Add(glDistribution.Percent);
                            adQty.Add(glDistribution.Quantity);
                            adSub.Add(glDistribution.SubmittedBy);
                        }
                        lineItem.ItemsAccountingString = string.Join("|", gl);
                        lineItem.ItemsAdAmount = string.Join("|", adAmts);
                        lineItem.ItemsAdPercent = string.Join("|", adPct);
                        lineItem.ItemsAdQuantity = string.Join("|", adQty);
                        lineItem.ItemsAdSubmittedBy = string.Join("|", adSub);
                    }

                    lineItems.Add(lineItem);
                }

                if (lineItems != null && lineItems.Any())
                {
                    request.PoLineItems = lineItems;
                }

            }

            return request;
        }

        /// <summary>
        /// Get the GUID for a buyer using its ID
        /// </summary>
        /// <param name="id">The Entity ID we are looking for</param>
        /// <param name="entity">Any entity found in Colleague</param>
        /// <returns>Buyer GUID</returns>
        public async Task<string> GetGuidFromIdAsync(string id, string entity)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync(entity, id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Bad.Data", "Guid.Not.Found", "GUID not found for " + entity + " " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Get a collection of Purchase Order summary domain entity objects
        /// </summary>
        /// <param name="id">Person ID</param>        
        /// <returns>collection of purchase order summary domain entity objects</returns>
        public async Task<IEnumerable<PurchaseOrderSummary>> GetPurchaseOrderSummaryByPersonIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            var cWebDefaults = await DataReader.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS");
            var filteredPurchaseOrder = await ApplyFilterCriteria(personId, cWebDefaults);

            if (!filteredPurchaseOrder.Any())
                return null;


            return await BuildPurchaseOrderSummaryList(personId, filteredPurchaseOrder);
        }


        /// <summary>
        ///  BUild Purchase order for create Request
        /// </summary>
        /// <param name="createUpdateRequest"></param>
        /// <returns></returns>
        private TxCreateWebPORequest BuildPurchaseOrderCreateRequest(PurchaseOrderCreateUpdateRequest createUpdateRequest)
        {
            var request = new TxCreateWebPORequest();
            var personId = createUpdateRequest.PersonId;
            var initiatorInitials = createUpdateRequest.InitiatorInitials;
            var confirmationEmailAddresses = createUpdateRequest.ConfEmailAddresses;
            var purchaseOrderEntity = createUpdateRequest.PurchaseOrder;
            bool isPersonVendor = createUpdateRequest.IsPersonVendor;
            if (!string.IsNullOrEmpty(personId))
            {
                request.APersonId = personId;
            }
            if (purchaseOrderEntity.Date != null)
            {
                request.APoDate = DateTime.SpecifyKind(purchaseOrderEntity.Date, DateTimeKind.Unspecified);
            }
            if (!string.IsNullOrEmpty(initiatorInitials))
            {
                request.APoInitiatorInitials = initiatorInitials;
            }
            if (confirmationEmailAddresses != null && confirmationEmailAddresses.Any())
            {
                request.AlConfEmailAddresses = confirmationEmailAddresses;
            }
            if (!string.IsNullOrEmpty(purchaseOrderEntity.ShipToCode))
            {
                request.APoShipToAddress = purchaseOrderEntity.ShipToCode;
            }

            if (!string.IsNullOrEmpty(purchaseOrderEntity.ApType))
            {
                request.AApType = purchaseOrderEntity.ApType;
            }

            request.AlPrintedComments = CommentsUtility.ConvertMultiLineTextToList(purchaseOrderEntity.Comments);
            request.AlInternalComments = new List<string>() { purchaseOrderEntity.InternalComments };

            if (purchaseOrderEntity.Approvers != null && purchaseOrderEntity.Approvers.Any())
            {
                request.AlNextApprovers = new List<string>();
                foreach (var nextApprover in purchaseOrderEntity.Approvers)
                {
                    request.AlNextApprovers.Add(nextApprover.ApproverId);
                }
            }

            // TaxCodes - In create purchase order, take taxcodes from first lineitem and assign to Tax code parameter 
            if (purchaseOrderEntity.LineItems != null && purchaseOrderEntity.LineItems.Any())
            {
                //Check if first lineitem has taxcode, as in create request it is auto applied to all lineitems
                var firstLineItem = purchaseOrderEntity.LineItems.FirstOrDefault();
                if (firstLineItem != null && firstLineItem.ReqLineItemTaxCodes != null && firstLineItem.ReqLineItemTaxCodes.Any())
                {
                    var taxCodeList = new List<string>();
                    foreach (var item in firstLineItem.ReqLineItemTaxCodes)
                    {
                        taxCodeList.Add(!string.IsNullOrEmpty(item.TaxReqTaxCode) ? item.TaxReqTaxCode : string.Empty);
                    }
                    request.AlTaxCodes = taxCodeList;
                }
            }
            // Commodity Codes - In create purchase order, take Commodity Codes from first lineitem 
            if (purchaseOrderEntity.LineItems != null && purchaseOrderEntity.LineItems.Any())
            {
                //Check if first lineitem has Commodity Codes, as in create request it is auto applied to all lineitems
                var firstLineItem = purchaseOrderEntity.LineItems.FirstOrDefault();
                if (firstLineItem != null && firstLineItem.CommodityCode != null && firstLineItem.CommodityCode.Any())
                {
                    request.ACommodityCode = firstLineItem.CommodityCode;
                }
            }
            if (purchaseOrderEntity.LineItems != null && purchaseOrderEntity.LineItems.Any())
            {
                var lineItems = new List<Transactions.AlCreatePoLineItems>();

                foreach (var apLineItem in purchaseOrderEntity.LineItems)
                {
                    var descriptionList = CommentsUtility.ConvertMultiLineTextToList(apLineItem.Description);
                    var lineItem = new Transactions.AlCreatePoLineItems()
                    {
                        AlItemDescs = string.Join(_SM.ToString(), descriptionList),
                        AlItemQtys = apLineItem.Quantity.ToString(),
                        AlItemPrices = apLineItem.Price.ToString(),
                        AlItemUnitIssues = apLineItem.UnitOfIssue,
                        AlItemVendorParts = apLineItem.VendorPart
                    };
                    var glAccts = new List<string>();
                    var glDistributionAmounts = new List<decimal?>();
                    var projectNos = new List<string>();
                    foreach (var item in apLineItem.GlDistributions)
                    {
                        glAccts.Add(!string.IsNullOrEmpty(item.GlAccountNumber) ? item.GlAccountNumber : string.Empty);
                        glDistributionAmounts.Add(item.Amount);
                        projectNos.Add(!string.IsNullOrEmpty(item.ProjectNumber) ? item.ProjectNumber : string.Empty);
                    }
                    lineItem.AlItemGlAccts = string.Join("|", glAccts);
                    lineItem.AlItemGlAcctAmts = string.Join("|", glDistributionAmounts);
                    lineItem.AlItemProjectNos = string.Join("|", projectNos);
                    lineItems.Add(lineItem);
                }
                request.AlCreatePoLineItems = lineItems;
            }

            if (!string.IsNullOrEmpty(purchaseOrderEntity.VendorId))
            {
                request.AVendorId = purchaseOrderEntity.VendorId;
            }
            else if (!string.IsNullOrEmpty(purchaseOrderEntity.VendorName))
            {
                request.AVendorId = purchaseOrderEntity.VendorName;
            }
            request.AVendorIsPersonFlag = isPersonVendor;
            return request;
        }

        /// <summary>
        ///  Build Purchase order for void Request
        /// </summary>
        /// <param name="createUpdateRequest"></param>
        /// <returns></returns>
        private TxVoidPurchaseOrderRequest BuildPurchaseOrderVoidRequest(PurchaseOrderVoidRequest voidRequest)
        {
            var request = new TxVoidPurchaseOrderRequest();
            var personId = voidRequest.PersonId;
            var purchaseOrderId = voidRequest.PurchaseOrderId;
            var confirmationEmailAddresses = voidRequest.ConfirmationEmailAddresses;
            var internalComments = voidRequest.InternalComments;

            if (!string.IsNullOrEmpty(personId))
            {
                request.AStaffUserId = personId;
            }
            if (!string.IsNullOrEmpty(purchaseOrderId))
            {
                request.APurchaseOrderId = purchaseOrderId;
            }
            if (!string.IsNullOrEmpty(confirmationEmailAddresses))
            {
                request.AConfirmationEmailAddress = confirmationEmailAddresses;
            }
            if (!string.IsNullOrEmpty(internalComments))
            {
                request.AInternalComments = internalComments;
            }

            return request;

        }


        /// <summary>
        /// Get a collection of hierarchy names for purchase order.
        /// </summary>
        /// <param name="purchaseOrderData">Purchase Order collection</param>        
        /// <returns>collection of hierarchy names for purchase order</returns>
        private Dictionary<string, string> GetPersonHierarchyNamesDictionary(System.Collections.ObjectModel.Collection<PurchaseOrders> purchaseOrderData)
        {
            #region Get Hierarchy Names

            // Use a colleague transaction to get all names at once. 
            List<string> personIds = new List<string>();
            List<string> hierarchies = new List<string>();
            List<string> personNames = new List<string>();
            Dictionary<string, string> hierarchyNameDictionary = new Dictionary<string, string>();

            GetHierarchyNamesForIdsResponse response = null;

            //Get all unique requestor & initiator personIds
            personIds = purchaseOrderData.Where(x => !string.IsNullOrEmpty(x.PoRequestor)).Select(s => s.PoRequestor)
                .Union(purchaseOrderData.Where(x => !string.IsNullOrEmpty(x.PoDefaultInitiator)).Select(s => s.PoDefaultInitiator)).Distinct().ToList();

            // Call a colleague transaction to get the person names based on their hierarchies, if necessary
            if ((personIds != null) && (personIds.Count > 0))
            {
                hierarchies = Enumerable.Repeat("PREFERRED", personIds.Count).ToList();
                GetHierarchyNamesForIdsRequest request = new GetHierarchyNamesForIdsRequest()
                {
                    IoPersonIds = personIds,
                    IoHierarchies = hierarchies
                };
                response = transactionInvoker.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(request);

                // The transaction returns the hierarchy names. If the name is multivalued, 
                // the transaction only returns the first value of the name.
                if (response != null)
                {
                    for (int i = 0; i < response.IoPersonIds.Count; i++)
                    {
                        string key = response.IoPersonIds[i];
                        string value = response.OutPersonNames[i];
                        if (!hierarchyNameDictionary.ContainsKey(key))
                        {
                            hierarchyNameDictionary.Add(key, value);
                        }
                    }

                }
            }

            //Get all unique ReqVendor Ids where ReqMiscName is missing
            var vendorIds = purchaseOrderData.Where(x => !string.IsNullOrEmpty(x.PoVendor) && !x.PoMiscName.Any()).Select(s => s.PoVendor).Distinct().ToList();

            if ((vendorIds != null) && (vendorIds.Count > 0))
            {
                hierarchies = Enumerable.Repeat("PO", vendorIds.Count).ToList();
                GetHierarchyNamesForIdsRequest request = new GetHierarchyNamesForIdsRequest()
                {
                    IoPersonIds = vendorIds,
                    IoHierarchies = hierarchies
                };
                response = transactionInvoker.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(request);

                // The transaction returns the hierarchy names. If the name is multivalued, 
                // the transaction only returns the first value of the name.
                if (response != null)
                {
                    for (int i = 0; i < response.IoPersonIds.Count; i++)
                    {
                        string key = response.IoPersonIds[i];
                        string value = response.OutPersonNames[i];
                        if (!hierarchyNameDictionary.ContainsKey(key))
                        {
                            hierarchyNameDictionary.Add(key, value);
                        }
                    }

                }
            }
            #endregion
            return hierarchyNameDictionary;
        }

        private PurchaseOrderSummary BuildPurchaseOrderSummary(PurchaseOrders purchaseOrderDataContract, Dictionary<string, Requisitions> requistionDictionary, string vendorName, string initiatorName, string requestorName, Collection<DataContracts.Opers> opersCollection)
        {
            if (purchaseOrderDataContract == null)
            {
                throw new ArgumentNullException("purchaseOrderDataContract");
            }

            if (string.IsNullOrEmpty(purchaseOrderDataContract.Recordkey))
            {
                throw new ArgumentNullException("id");
            }

            if (!purchaseOrderDataContract.PoDate.HasValue)
            {
                throw new ApplicationException("Missing date for purchase order id: " + purchaseOrderDataContract.Recordkey);
            }


            var requisitionStatus = GetPurchaseOrderStatus(purchaseOrderDataContract.PoStatus.FirstOrDefault(), purchaseOrderDataContract.Recordkey);

            var purchaseOrderSummaryEntity = new PurchaseOrderSummary(purchaseOrderDataContract.Recordkey, purchaseOrderDataContract.PoNo, vendorName, purchaseOrderDataContract.PoDate.Value.Date)
            {
                Status = requisitionStatus,
                VendorId = purchaseOrderDataContract.PoVendor,
                InitiatorName = initiatorName,
                RequestorName = requestorName,
                Amount = purchaseOrderDataContract.PoTotalAmt.HasValue ? purchaseOrderDataContract.PoTotalAmt.Value : 0
            };
            // build approvers and add to entity
            if ((purchaseOrderDataContract.PoAuthEntityAssociation != null) && (purchaseOrderDataContract.PoAuthEntityAssociation.Any()))
            {
                // Approver object is declared once
                Approver approver;
                foreach (var approval in purchaseOrderDataContract.PoAuthEntityAssociation)
                {
                    //get opersId for the requisition
                    var oper = opersCollection.FirstOrDefault(x => x.Recordkey == approval.PoAuthorizationsAssocMember);
                    if (oper != null)
                    {
                        approver = new Approver(oper.Recordkey);
                        approver.SetApprovalName(oper.SysUserName);
                        approver.ApprovalDate = approval.PoAuthorizationDatesAssocMember.Value;
                        purchaseOrderSummaryEntity.AddApprover(approver);
                    }
                }
            }
            // build next approvers and add to entity
            if ((purchaseOrderDataContract.PoApprEntityAssociation != null) && (purchaseOrderDataContract.PoApprEntityAssociation.Any()))
            {
                // Approver object is declared once
                Approver approver;
                foreach (var approval in purchaseOrderDataContract.PoApprEntityAssociation)
                {
                    //get opersId for the requisition
                    var oper = opersCollection.FirstOrDefault(x => x.Recordkey == approval.PoNextApprovalIdsAssocMember);
                    if (oper != null)
                    {
                        approver = new Approver(oper.Recordkey);
                        approver.SetApprovalName(oper.SysUserName);
                        purchaseOrderSummaryEntity.AddApprover(approver);
                    }
                }
            }

            //  Add any associated vouchers to the purchase order summary domain entity
            if ((purchaseOrderDataContract.PoVouIds != null) && (purchaseOrderDataContract.PoVouIds.Any()))
            {
                foreach (var voucherId in purchaseOrderDataContract.PoVouIds)
                {
                    if (!string.IsNullOrEmpty(voucherId))
                    {
                        purchaseOrderSummaryEntity.AddVoucherId(voucherId);
                    }
                }
            }

            //  Add any associated requisitions to the purchase order summary domain entity
            if ((purchaseOrderDataContract.PoReqIds != null) && (purchaseOrderDataContract.PoReqIds.Any()))
            {
                foreach (var requisitionId in purchaseOrderDataContract.PoReqIds)
                {
                    if (!string.IsNullOrEmpty(requisitionId))
                    {
                        Requisitions requisition = null;
                        if (requistionDictionary.TryGetValue(requisitionId, out requisition))
                        {
                            var requisitionSummaryEntity = new RequisitionSummary(requisition.Recordkey, requisition.ReqNo, vendorName, requisition.ReqDate.Value.Date);
                            purchaseOrderSummaryEntity.AddRequisition(requisitionSummaryEntity);
                        }
                    }
                }
            }

            return purchaseOrderSummaryEntity;
        }

        private async Task<List<string>> ApplyFilterCriteria(string personId, CfwebDefaults cfWebDefaults)
        {
            List<string> filteredPurchaseOrder = new List<string>();
            string poStartEndTransDateQuery = string.Empty;
            //where personId is Initiator OR requestor
            string PurchaseOrderPersonIdQuery = string.Format("WITH PO.DEFAULT.INITIATOR EQ '{0}' OR WITH PO.REQUESTOR EQ '{0}' BY.DSND PO.NO", personId);
            filteredPurchaseOrder = await ExecuteQueryStatementAsync(filteredPurchaseOrder, PurchaseOrderPersonIdQuery);

            if (filteredPurchaseOrder != null && filteredPurchaseOrder.Any())
            {
                if (cfWebDefaults != null)
                {
                    //Filter by CfwebPoStartDate, CfwebPoEndDate values configured in CFWP form
                    //when CfwebPoStartDate & CfwebPoEndDate has a value
                    if (cfWebDefaults.CfwebPoStartDate.HasValue && cfWebDefaults.CfwebPoEndDate.HasValue)
                    {
                        var startDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebPoStartDate.Value);
                        var endDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebPoEndDate.Value);
                        poStartEndTransDateQuery = string.Format("WITH (PO.MAINT.GL.TRAN.DATE GE '{0}' AND PO.MAINT.GL.TRAN.DATE LE '{1}') OR WITH (PO.DATE GE '{0}' AND PO.DATE LE '{1}') BY.DSND PO.NO", startDate, endDate);
                    }
                    //when CfwebPoStartDate has value but CfwebReqEndDate is null
                    else if (cfWebDefaults.CfwebPoStartDate.HasValue && !cfWebDefaults.CfwebPoEndDate.HasValue)
                    {
                        var startDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebPoStartDate.Value);
                        poStartEndTransDateQuery = string.Format("PO.MAINT.GL.TRAN.DATE GE '{0}' OR WITH PO.DATE GE '{0}' BY.DSND PO.NO", startDate);
                    }
                    //when CfwebPoStartDate is null but CfwebPoEndDate has value
                    else if (!cfWebDefaults.CfwebPoStartDate.HasValue && cfWebDefaults.CfwebPoEndDate.HasValue)
                    {
                        var endDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebPoEndDate.Value);
                        poStartEndTransDateQuery = string.Format("WITH ((PO.MAINT.GL.TRAN.DATE NE '') AND (PO.MAINT.GL.TRAN.DATE LE '{0}')) OR WITH ((PO.DATE NE '') AND (PO.DATE LE '{0}')) BY.DSND PO.NO", endDate);
                    }

                    if (!string.IsNullOrEmpty(poStartEndTransDateQuery))
                    {
                        filteredPurchaseOrder = await ExecuteQueryStatementAsync(filteredPurchaseOrder, poStartEndTransDateQuery);
                    }

                    //query by CfwebPoStatuses if statuses are configured in CFWP form.
                    if (cfWebDefaults.CfwebPoStatuses != null && cfWebDefaults.CfwebPoStatuses.Any())
                    {
                        var purchaseOrderStatusesCriteria = string.Join(" ", cfWebDefaults.CfwebPoStatuses.Select(x => string.Format("'{0}'", x.ToUpper())));
                        purchaseOrderStatusesCriteria = "WITH PO.CURRENT.STATUS EQ " + purchaseOrderStatusesCriteria;
                        filteredPurchaseOrder = await ExecuteQueryStatementAsync(filteredPurchaseOrder, purchaseOrderStatusesCriteria);
                    }
                }
            }

            return filteredPurchaseOrder;
        }

        private async Task<List<string>> ExecuteQueryStatementAsync(List<string> filteredPurchaseOrders, string queryCriteria)
        {
            string[] filteredByQueryCriteria = null;
            if (string.IsNullOrEmpty(queryCriteria))
                return null;
            if (filteredPurchaseOrders != null && filteredPurchaseOrders.Any())
            {
                filteredByQueryCriteria = await DataReader.SelectAsync("PURCHASE.ORDERS", filteredPurchaseOrders.ToArray(), queryCriteria);
            }
            else
            {
                filteredByQueryCriteria = await DataReader.SelectAsync("PURCHASE.ORDERS", queryCriteria);
            }
            return filteredByQueryCriteria.ToList();
        }

        private async Task<string> BuildFilterCriteria(ProcurementDocumentFilterCriteria criteria, CfwebDefaults cfWebDefaults)
        {
            bool skipCFWPDateRangeFilter = false;
            bool skipCFWPStatusFilter = false;

            StringBuilder queryCriteria = new StringBuilder();
            //personID criteria
            queryCriteria.Append(string.Format("WITH PO.DEFAULT.INITIATOR EQ '{0}' OR WITH PO.REQUESTOR EQ '{0}' ", criteria.PersonId));

            //if SS criteria contains dateFrom or dateTo, CFWP date range filter should be skipped.
            if (criteria.DateFrom.HasValue || criteria.DateTo.HasValue)
            {
                skipCFWPDateRangeFilter = true;
            }
            List<string> procurementFilterStatuses = new List<string>();
            if (criteria.Statuses != null && criteria.Statuses.Any())
            {
                foreach (var item in criteria.Statuses)
                {
                    var status = ConvertPurchaseOrderStatus(item);
                    if (!string.IsNullOrEmpty(status))
                    {
                        procurementFilterStatuses.Add(status);
                    }
                }
            }
            //if SS criteria contains statuses, CFWP status filter should be skipped
            if (procurementFilterStatuses.Any())
            {
                skipCFWPStatusFilter = true;
            }
            if (cfWebDefaults != null)
            {
                if (!skipCFWPDateRangeFilter)
                {
                    //criteria set in cfWebDefaults - CfwebPoStartDate, CfwebPoEndDate
                    var cfwpDateRangeCriteria = await BuildDateRangeQueryAsync(cfWebDefaults.CfwebPoStartDate, cfWebDefaults.CfwebPoEndDate);
                    if (!string.IsNullOrEmpty(cfwpDateRangeCriteria))
                    {
                        logger.Debug(string.Format("QueryPurchaseOrderSummaries - CFWP date range - data reader - query string: '{0}'.", cfwpDateRangeCriteria));
                        queryCriteria.Append(cfwpDateRangeCriteria);
                    }
                }
                if (!skipCFWPStatusFilter)
                {
                    //criteria set in cfwebdefaults - CfwebPoStatuses (PO.CURRENT.STATUS).
                    var statusesQuery = ProcurementFilterUtility.BuildListQuery(cfWebDefaults.CfwebPoStatuses, "PO.CURRENT.STATUS");
                    if (!string.IsNullOrEmpty(statusesQuery))
                    {
                        logger.Debug(string.Format("QueryPurchaseOrderSummaries - CFWP statuses - data reader - query string: '{0}'.", statusesQuery));
                        queryCriteria.Append(statusesQuery);
                    }
                }
            }

            //criteria sent from SS - VendorID's (PO.VENDOR).
            var vendorIdCriteria = ProcurementFilterUtility.BuildListQuery(criteria.VendorIds, "PO.VENDOR");
            if (!string.IsNullOrEmpty(vendorIdCriteria))
            {
                logger.Debug(string.Format("QueryPurchaseOrderSummaries - VendorId's - data reader - query string: '{0}'.", vendorIdCriteria));
                queryCriteria.Append(!string.IsNullOrEmpty(vendorIdCriteria) ? vendorIdCriteria : string.Empty);
            }
            //criteria sent from SS - Min - Max Amount (PO.TOTAL.AMT).
            var amountCriteria = ProcurementFilterUtility.BuildAmountRangeQuery(criteria, "PO.TOTAL.AMT");
            if (!string.IsNullOrEmpty(amountCriteria))
            {
                logger.Debug(string.Format("QueryPurchaseOrderSummaries - Amount range - data reader - query string: '{0}'.", amountCriteria));
                queryCriteria.Append(amountCriteria);
            }
            //criteria sent from SS - From - To date range
            if (skipCFWPDateRangeFilter)
            {
                var procurementDateRangeCriteria = await BuildDateRangeQueryAsync(criteria.DateFrom, criteria.DateTo);
                if (!string.IsNullOrEmpty(procurementDateRangeCriteria))
                {
                    logger.Debug(string.Format("QueryPurchaseOrderSummaries - SS procurement filter date range - data reader - query string: '{0}'.", procurementDateRangeCriteria));
                    queryCriteria.Append(procurementDateRangeCriteria);
                }
            }

            if (skipCFWPStatusFilter)
            {
                //criteria sent from SS - Statuses
                var procurementStatusesCriteria = ProcurementFilterUtility.BuildListQuery(procurementFilterStatuses, "PO.CURRENT.STATUS");
                if (!string.IsNullOrEmpty(procurementStatusesCriteria))
                {
                    logger.Debug(string.Format("QueryPurchaseOrderSummaries - SS procurement filter statuses - data reader - query string: '{0}'.", procurementStatusesCriteria));
                    queryCriteria.Append(procurementStatusesCriteria);
                }
            }

            queryCriteria.Append("BY.DSND PO.NO");
            logger.Debug(string.Format("QueryPurchaseOrderSummaries - data reader - query string: '{0}'.", queryCriteria));
            return queryCriteria.ToString();
        }

        private async Task<string> BuildDateRangeQueryAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            string startEndTransDateQuery = string.Empty;
            if (dateFrom != null || dateTo != null)
            {
                //when dateFrom & dateTo has a value
                if (dateFrom.HasValue && dateTo.HasValue)
                {
                    var startDate = await GetUnidataFormatDateAsync(dateFrom.Value);
                    var endDate = await GetUnidataFormatDateAsync(dateTo.Value);

                    startEndTransDateQuery = string.Format("WITH (PO.MAINT.GL.TRAN.DATE GE '{0}' AND PO.MAINT.GL.TRAN.DATE LE '{1}') OR (PO.DATE GE '{0}' AND PO.DATE LE '{1}') ", startDate, endDate);
                }
                //when dateFrom has value but dateTo is null
                else if (dateFrom.HasValue && !dateTo.HasValue)
                {
                    var startDate = await GetUnidataFormatDateAsync(dateFrom.Value);
                    startEndTransDateQuery = string.Format("AND WITH (PO.MAINT.GL.TRAN.DATE GE '{0}' OR PO.DATE GE '{0}') ", startDate);
                }
                //when dateFrom is null but dateTo has value
                else if (!dateFrom.HasValue && dateTo.HasValue)
                {
                    var endDate = await GetUnidataFormatDateAsync(dateTo.Value);
                    startEndTransDateQuery = string.Format("AND WITH ((PO.MAINT.GL.TRAN.DATE NE '') AND (PO.MAINT.GL.TRAN.DATE LE '{0}')) OR ((PO.DATE NE '') AND (PO.DATE LE '{0}')) ", endDate);
                }
            }

            return startEndTransDateQuery;
        }

        private async Task<IEnumerable<PurchaseOrderSummary>> BuildPurchaseOrderSummaryList(string personId, List<string> filteredPurchaseOrderIds)
        {
            var purchaseOrderList = new List<PurchaseOrderSummary>();

            if (!filteredPurchaseOrderIds.Any())
                return purchaseOrderList;

            var purchaseOrderData = await DataReader.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", filteredPurchaseOrderIds.ToArray());


            if (purchaseOrderData != null && purchaseOrderData.Any())
            {
                Dictionary<string, string> hierarchyNameDictionary = GetPersonHierarchyNamesDictionary(purchaseOrderData);
                var RequisitionNumbers = purchaseOrderData.Where(x => x.PoReqIds.Any()).Select(s => s.PoReqIds).ToList();
                var requisitionIds = RequisitionNumbers.SelectMany(x => x).Distinct().ToList();
                var requisitions = await DataReader.BulkReadRecordAsync<DataContracts.Requisitions>("REQUISITIONS", requisitionIds.ToArray());
                var requistionDictionary = (requisitions != null && requisitions.Any()) ? requisitions.ToDictionary(x => x.Recordkey) : new Dictionary<string, Requisitions>();

                // Read the OPERS records associated with the approval signatures and 
                // next approvers on the purchase orders, and build approver objects.
                var operators = new List<string>();
                Collection<DataContracts.Opers> opersCollection = new Collection<DataContracts.Opers>();
                // get list of Approvers and next approvers from the entire po records

                var allPoDataApprovers = purchaseOrderData.SelectMany(poContract => poContract.PoAuthorizations).Distinct().ToList();
                if (allPoDataApprovers != null && allPoDataApprovers.Any(x => x != null))
                {
                    operators.AddRange(allPoDataApprovers);
                }

                var allPoDataNextApprovers = purchaseOrderData.SelectMany(poContract => poContract.PoNextApprovalIds).Distinct().ToList();
                if (allPoDataNextApprovers != null && allPoDataNextApprovers.Any(x => x != null))
                {
                    operators.AddRange(allPoDataNextApprovers);
                }

                var uniqueOperators = operators.Distinct().ToList();
                if (uniqueOperators.Count > 0)
                {
                    opersCollection = await DataReader.BulkReadRecordAsync<DataContracts.Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                }


                foreach (PurchaseOrders purchaseOrder in purchaseOrderData)
                {
                    try
                    {
                        string initiatorName = string.Empty;
                        hierarchyNameDictionary.TryGetValue(purchaseOrder.PoDefaultInitiator, out initiatorName);

                        string requestorName = string.Empty;
                        hierarchyNameDictionary.TryGetValue(purchaseOrder.PoRequestor, out requestorName);

                        // If there is no vendor name and there is a vendor id, use the PO hierarchy to get the vendor name.
                        var VendorName = purchaseOrder.PoMiscName.FirstOrDefault();
                        if ((string.IsNullOrEmpty(VendorName)) && (!string.IsNullOrEmpty(purchaseOrder.PoVendor)))
                        {
                            hierarchyNameDictionary.TryGetValue(purchaseOrder.PoVendor, out VendorName);
                        }
                        purchaseOrderList.Add(BuildPurchaseOrderSummary(purchaseOrder, requistionDictionary, VendorName, initiatorName, requestorName, opersCollection));
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            return purchaseOrderList;
        }

        private static string ConvertPurchaseOrderStatus(string status)
        {
            string purchaseOrderStatus = null;
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToUpper())
                {
                    case "ACCEPTED":
                        purchaseOrderStatus = "A";
                        break;
                    case "BACKORDERED":
                        purchaseOrderStatus = "B";
                        break;
                    case "CLOSED":
                        purchaseOrderStatus = "C";
                        break;
                    case "INPROGRESS":
                        purchaseOrderStatus = "U";
                        break;
                    case "INVOICED":
                        purchaseOrderStatus = "I";
                        break;
                    case "NOTAPPROVED":
                        purchaseOrderStatus = "N";
                        break;
                    case "OUTSTANDING":
                        purchaseOrderStatus = "O";
                        break;
                    case "PAID":
                        purchaseOrderStatus = "P";
                        break;
                    case "RECONCILED":
                        purchaseOrderStatus = "R";
                        break;
                    case "VOIDED":
                        purchaseOrderStatus = "V";
                        break;
                }
            }
            return purchaseOrderStatus;
        }

        #region Get VendorAddress
        private async Task GetVendorAddress(string vendorID, PurchaseOrder purchaseOrderDomainEntity)
        {
            if (!string.IsNullOrEmpty(vendorID))
            {
                GetActiveVendorResultsRequest searchRequest = new GetActiveVendorResultsRequest();
                searchRequest.ASearchCriteria = vendorID;
                searchRequest.AApType = string.Empty;
                try
                {
                    GetActiveVendorResultsResponse searchResponse = await transactionInvoker.ExecuteAsync<GetActiveVendorResultsRequest, GetActiveVendorResultsResponse>(searchRequest);

                    if (searchResponse != null && searchResponse.VendorSearchResults != null && searchResponse.VendorSearchResults.Any())
                    {
                        purchaseOrderDomainEntity.VendorAddress = searchResponse.VendorSearchResults.FirstOrDefault().AlVendorAddresses;
                        purchaseOrderDomainEntity.VendorAddressTypeCode = searchResponse.VendorSearchResults.FirstOrDefault().AlVendAddrTypeCodes;
                        purchaseOrderDomainEntity.VendorAddressTypeDesc = searchResponse.VendorSearchResults.FirstOrDefault().AlVendAddrTypeDesc;
                    }
                }
                catch (Exception)
                {
                    var message = string.Format("{0} Unable to get Vendor address.", vendorID);
                    logger.Error(message);
                }

            }
        }
        #endregion

    }

}
