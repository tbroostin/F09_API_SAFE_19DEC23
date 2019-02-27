// Copyright 2015-2018 Ellucian Company L.P. and its affiliates

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
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    ///  This class implements the IPurchaseOrderRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PurchaseOrderRepository : BaseColleagueRepository, IPurchaseOrderRepository
    {
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;

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
                createResponse.UpdatePOErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
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
            var purchaseOrderId = await this.GetPurchaseOrdersIdFromGuidAsync(purchaseOrdersEntity.Guid);

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
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.UpdatePOErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
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
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (expenseAccounts == null)
            {
                expenseAccounts = new List<string>();
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
            purchaseOrderDomainEntity.Comments = purchaseOrder.PoPrintedComments;
            purchaseOrderDomainEntity.InternalComments = purchaseOrder.PoComments;

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

            // Populate the line item domain entities and add them to the purchase order domain entity
            var lineItemIds = purchaseOrder.PoItemsId;
            if (lineItemIds != null && lineItemIds.Count() > 0)
            {
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
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;


                        // Populate the GL distribution domain entities and add them to the line items
                        if ((lineItem.ItemPoEntityAssociation != null) && (lineItem.ItemPoEntityAssociation.Count > 0))
                        {
                            foreach (var glDist in lineItem.ItemPoEntityAssociation)
                            {
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
                }
            }

            return purchaseOrderDomainEntity;
        }

        /// <summary>
        /// Get the purchase order requested
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <returns>Tuple of PurchaseOrder entity objects <see cref="PurchaseOrder"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<PurchaseOrder>, int>> GetPurchaseOrdersAsync(int offset, int limit)
        {
            var purchaseOrderIds = await DataReader.SelectAsync("PURCHASE.ORDERS", "WITH PO.ITEMS.ID NE ''");

            var totalCount = purchaseOrderIds.Count();
            Array.Sort(purchaseOrderIds);
            var subList = purchaseOrderIds.Skip(offset).Take(limit).ToArray();

            var purchaseOrderData = await DataReader.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", subList);

            if (purchaseOrderData == null)
            {
                throw new KeyNotFoundException("No records selected from PURCHASE.ORDERS file in Colleague.");
            }

            var purchaseOrders = await BuildPurchaseOrdersAsync(purchaseOrderData);

            return new Tuple<IEnumerable<PurchaseOrder>, int>(purchaseOrders, totalCount);

        }

        /// <summary>
        /// Get PurchaseOrder by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>PurchaseOrder entity object <see cref="PurchaseOrder"/></returns>
        public async Task<PurchaseOrder> GetPurchaseOrdersByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var id = await GetPurchaseOrdersIdFromGuidAsync(guid);

            if (id == null || string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("Purchase Order not found for GUID " + guid);
            }

            var purchaseOrder = await DataReader.ReadRecordAsync<PurchaseOrders>(id);

            if ( purchaseOrder != null && purchaseOrder.RecordGuid != null && purchaseOrder.RecordGuid != guid)
            {
                throw new KeyNotFoundException("No Purchase Orders was found for guid " +guid);
            }

            return await BuildPurchaseOrderAsync(purchaseOrder);

        }

        public async Task<string> GetPurchaseOrdersIdFromGuidAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
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
            return dict;
        }

        /// <summary>
        ///  Build collection of PurchaseOrder domain entities 
        /// </summary>
        /// <param name="purchaseOrders">Collection of PurchaseOrder data contracts</param>
        /// <returns>PurchaseOrder domain entity</returns>
        private async Task<IEnumerable<PurchaseOrder>> BuildPurchaseOrdersAsync(IEnumerable<DataContracts.PurchaseOrders> purchaseOrders)
        {
            var purchaseOrderCollection = new List<PurchaseOrder>();

            foreach (var purchaseOrder in purchaseOrders)
            {
                try
                {
                    purchaseOrderCollection.Add(await BuildPurchaseOrderAsync(purchaseOrder));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(string.Concat(ex.Message, "  Purchase Order: '", purchaseOrder.PoNo, "', Entity: 'PURCHASE.ORDERS', Record ID: '", purchaseOrder.Recordkey, "'"));
                }
            }

            return purchaseOrderCollection.AsEnumerable();
        }

        private async Task<PurchaseOrder> BuildPurchaseOrderAsync(PurchaseOrders purchaseOrder,
            string personId = "", IEnumerable<string> expenseAccounts = null)
        {
            if (purchaseOrder == null)
            {
                throw new ArgumentNullException("purchaseOrder");
            }

            string id = purchaseOrder.Recordkey;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            string guid = purchaseOrder.RecordGuid;

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
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
                    throw new ApplicationException(string.Concat("Invalid purchase order status for purchase order '", purchaseOrder.PoNo, "'.  Status: '", purchaseOrder.PoStatus.FirstOrDefault(), "', Entity: 'PURCHASE.ORDERS', Record ID: '", purchaseOrder.Recordkey, "'"));
                }
            }
            else
            {
                throw new ApplicationException("Missing status for purchase order '" + purchaseOrder.PoNo + "', Entity: 'PURCHASE.ORDERS', Record ID: '" + purchaseOrder.Recordkey + "'");
            }

            string purchaseOrderVendorName = "";

            if (purchaseOrder.PoStatusDate == null || !purchaseOrder.PoStatusDate.Any() || !purchaseOrder.PoStatusDate.First().HasValue)
            {
                throw new ApplicationException("Missing status date for purchase order '" + purchaseOrder.PoNo + "', Entity: 'PURCHASE.ORDERS', Record ID: '" + purchaseOrder.Recordkey + "'");
            }

            if (!purchaseOrder.PoDate.HasValue)
            {
                throw new ApplicationException("Missing date for purchase order '" + purchaseOrder.PoNo + "', Entity: 'PURCHASE.ORDERS', Record ID: '" + purchaseOrder.Recordkey + "'");
            }

            // The purchase order status date contains one to many dates
            var purchaseOrderStatusDate = purchaseOrder.PoStatusDate.First().Value;

            var purchaseOrderDomainEntity = new PurchaseOrder(purchaseOrder.Recordkey, purchaseOrder.RecordGuid, purchaseOrder.PoNo, purchaseOrderVendorName, purchaseOrderStatus, purchaseOrderStatusDate, purchaseOrder.PoDate.Value.Date);

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
                throw new ApplicationException(string.Format("Line Items are missing for Purchase Order '{0}', Entity: 'PURCHASE.ORDERS', Record ID: '{1}'.", purchaseOrder.PoNo, purchaseOrder.Recordkey));
            }
            await GetLineItems(expenseAccounts, purchaseOrder, purchaseOrderDomainEntity, lineItemIds);

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
                                } else
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
                        throw new ApplicationException(errorMessage);
                    }

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
                    lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;

                    lineItemDomainEntity.CommodityCode = lineItem.ItmCommodityCode;
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
                                lineItemDomainEntity.Status = GetPurchaseOrderStatus(poStatus.ItmPoStatusAssocMember, purchaseOrder.Recordkey);
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

        private static PurchaseOrderStatus? GetPurchaseOrderStatus(string status, string recordId)
        {
            PurchaseOrderStatus? purchaseOrderStatus = null;
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException(string.Concat("Purchase Order Status is required. Entity: 'PURCHASE.ORDERS', Record ID: '", recordId , "'"));
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
                case "H":
                    // Leave null for now, until we have a valid enumeration
                    break;
                default:
                    // if we get here, we have corrupt data.
                    throw new ApplicationException(string.Concat("Invalid purchase order status for purchase order.  Status: '", status, "', Entity: 'PURCHASE.ORDERS', Record ID: '", recordId, "'"));
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
                        ItemsPartNumber = apLineItem.VendorPart,
                        ItemsDesiredDate = apLineItem.DesiredDate,
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

                    lineItem.ItemsStatus = apLineItem.Status.ToString();
                    switch (apLineItem.Status)
                    {
                        case PurchaseOrderStatus.Accepted:
                            lineItem.ItemsStatus = "A";
                            break;
                        case PurchaseOrderStatus.Closed:
                            lineItem.ItemsStatus = "C";
                            break;
                        case PurchaseOrderStatus.Outstanding:
                            lineItem.ItemsStatus = "O";
                            break;
                        case PurchaseOrderStatus.Voided:
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
                ex.AddError(new RepositoryError("Guid NotFound", "GUID not found for " + entity + " " + id));
                throw ex;
            }
        }
       
    }
}
