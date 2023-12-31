﻿// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.Base.Services;
using System.Collections.ObjectModel;
using System.Text;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IRequisitionRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RequisitionRepository : BaseColleagueRepository, IRequisitionRepository
    {
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        protected const int AllRequisitionCacheTimeout = 20; // Clear from cache every 20 minutes
        protected const string AllRequisitionsCache = "AllEthosRequisitions";
        private static char _SM = Convert.ToChar(DynamicArray.SM);
        private RepositoryException exception = new RepositoryException();

        /// <summary>
        /// The constructor to instantiate a requisition repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public RequisitionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get a single requisition
        /// </summary>
        /// <param name="id">Requisition ID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>A requisition domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Requisition> GetRequisitionAsync(string id, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            logger.Debug(string.Format("requisition {0} ", id));
            logger.Debug(string.Format("gl access level {0}", glAccessLevel));

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (expenseAccounts == null)
            {
                expenseAccounts = new List<string>();
                logger.Debug(string.Format("no GL accounts for requisition {0} ", id));
            }

            var requisition = await DataReader.ReadRecordAsync<Requisitions>(id);
            if (requisition == null)
            {
                throw new KeyNotFoundException(string.Format("Requisition record {0} does not exist.", id));
            }

            // Translate the status code into a RequisitionStatus enumeration value
            RequisitionStatus requisitionStatus = new RequisitionStatus();

            // Get the first status in the list of requisition statuses and check it has a value
            if (requisition.ReqStatus != null && !string.IsNullOrEmpty(requisition.ReqStatus.FirstOrDefault()))
            {
                switch (requisition.ReqStatus.First().ToUpper())
                {
                    case "U":
                        requisitionStatus = RequisitionStatus.InProgress;
                        break;
                    case "N":
                        requisitionStatus = RequisitionStatus.NotApproved;
                        break;
                    case "O":
                        requisitionStatus = RequisitionStatus.Outstanding;
                        break;
                    case "P":
                        requisitionStatus = RequisitionStatus.PoCreated;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid requisition status for requisition: " + requisition.Recordkey);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for requisition: " + requisition.Recordkey);
            }

            // Check that the requisition status date has a value
            if (requisition.ReqStatusDate == null || !requisition.ReqStatusDate.First().HasValue)
            {
                throw new ApplicationException("Missing status date for requisition: " + requisition.Recordkey);
            }

            // The requisition status date contains one to many dates
            var requisitionStatusDate = requisition.ReqStatusDate.First().Value;

            if (!requisition.ReqDate.HasValue)
            {
                throw new ApplicationException("Missing date for requisition: " + requisition.Recordkey);
            }

            #region Get Hierarchy Names

            // Determine the vendor name for the requisition. If there is a miscellaneous name, use it.
            // Otherwise, we will get the name for the id further down.
            var requisitionVendorName = requisition.ReqMiscName.FirstOrDefault();

            // Use a colleague transaction to get all names at once. 
            List<string> personIds = new List<string>();
            List<string> hierarchies = new List<string>();
            List<string> personNames = new List<string>();
            string initiatorName = null;
            string requestorName = null;

            // If there is no vendor name and there is a vendor id, use the PO hierarchy to get the vendor name.
            if ((string.IsNullOrEmpty(requisitionVendorName)) && (!string.IsNullOrEmpty(requisition.ReqVendor)))
            {
                personIds.Add(requisition.ReqVendor);
                hierarchies.Add("PO");
            }

            // Use the PREFERRED hierarchy for the initiator and the requestor.
            if (!string.IsNullOrEmpty(requisition.ReqDefaultInitiator))
            {
                personIds.Add(requisition.ReqDefaultInitiator);
                hierarchies.Add("PREFERRED");
            }

            // Sometimes the requestor is the same person as the initiator. f they are the same,
            // there is no need to add it to the list because the hierarchy is the same.
            if ((!string.IsNullOrEmpty(requisition.ReqRequestor)) && (requisition.ReqRequestor != requisition.ReqDefaultInitiator))
            {
                personIds.Add(requisition.ReqRequestor);
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
                                if ((ioPersonId == requisition.ReqVendor) && (hierarchy == "PO"))
                                {
                                    requisitionVendorName = name;
                                }
                                if ((ioPersonId == requisition.ReqDefaultInitiator) && (hierarchy == "PREFERRED"))
                                {
                                    initiatorName = name;
                                }
                                if ((ioPersonId == requisition.ReqRequestor) && (hierarchy == "PREFERRED"))
                                {
                                    requestorName = name;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            var requisitionDomainEntity = new Requisition(requisition.Recordkey, requisition.ReqNo, requisitionVendorName, requisitionStatus, requisitionStatusDate, requisition.ReqDate.Value.Date);

            requisitionDomainEntity.VendorId = requisition.ReqVendor;

            // Assign Vendor Address
            await GetVendorAddress(requisition.ReqVendor, requisitionDomainEntity);

            if (!string.IsNullOrEmpty(initiatorName))
            {
                requisitionDomainEntity.InitiatorName = initiatorName;
            }
            if (!string.IsNullOrEmpty(requestorName))
            {
                requisitionDomainEntity.RequestorName = requestorName;
            }

            requisitionDomainEntity.Amount = 0;
            requisitionDomainEntity.CurrencyCode = requisition.ReqCurrencyCode;
            if (requisition.ReqMaintGlTranDate.HasValue)
            {
                requisitionDomainEntity.MaintenanceDate = requisition.ReqMaintGlTranDate.Value.Date;
            }

            if (requisition.ReqDesiredDeliveryDate.HasValue)
            {
                requisitionDomainEntity.DesiredDate = requisition.ReqDesiredDeliveryDate.Value.Date;
            }
            requisitionDomainEntity.ApType = requisition.ReqApType;
            requisitionDomainEntity.Comments = requisition.ReqPrintedComments;
            requisitionDomainEntity.InternalComments = requisition.ReqComments;
            requisitionDomainEntity.ShipToCode = requisition.ReqShipTo;
            requisitionDomainEntity.ApprovalReturnedIndicator = (!string.IsNullOrEmpty(requisition.ReqReturnFlag) && requisition.ReqReturnFlag.Equals("Y"));
            if (string.IsNullOrEmpty(requisition.ReqShipTo))
            {
                var requisitionDefaults = await DataReader.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS");
                if (requisitionDefaults != null)
                {
                    requisitionDomainEntity.ShipToCode = requisitionDefaults.PurShipToCode;
                }
            }
            requisitionDomainEntity.CommodityCode = requisition.ReqDefaultCommodity;
            requisitionDomainEntity.ConfirmationEmailAddresses = requisition.ReqConfEmailAddresses;

            // Add any associated purchase orders to the requisition domain entity
            if ((requisition.ReqPoNo != null) && (requisition.ReqPoNo.Count > 0))
            {
                foreach (var purchaseOrderId in requisition.ReqPoNo)
                {
                    if (!string.IsNullOrEmpty(purchaseOrderId))
                    {
                        requisitionDomainEntity.AddPurchaseOrder(purchaseOrderId);
                    }
                }
            }

            // Add any associated blanket purchase orders to the requisition domain entity.
            // Even though ReqBpoNo is a list of string, only one bpo can be associated to a requisition
            if ((requisition.ReqBpoNo != null) && (requisition.ReqBpoNo.Count > 0))
            {
                if (requisition.ReqBpoNo.Count > 1)
                {
                    throw new ApplicationException("Only one blanket purchase order can be associated with the requisition: " + requisition.Recordkey);
                }
                else
                {
                    requisitionDomainEntity.BlanketPurchaseOrder = requisition.ReqBpoNo.FirstOrDefault();
                }
            }

            // Read the OPERS records associated with the approval signatures and 
            // next approvers on the requisiton, and build approver objects.
            var operators = new List<string>();
            if (requisition.ReqAuthorizations != null)
            {
                operators.AddRange(requisition.ReqAuthorizations);
            }
            if (requisition.ReqNextApprovalIds != null)
            {
                operators.AddRange(requisition.ReqNextApprovalIds);
            }
            var uniqueOperators = operators.Distinct().ToList();
            if (uniqueOperators.Count > 0)
            {
                var Approvers = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                if ((Approvers != null) && (Approvers.Count > 0))
                {
                    // loop through the opers, create Approver objects, add the name, and if they
                    // are one of the approvers of the requisition, add the approval date.
                    foreach (var appr in Approvers)
                    {
                        Approver approver = new Approver(appr.Recordkey);
                        var approverName = appr.SysUserName;
                        approver.SetApprovalName(approverName);
                        if ((requisition.ReqAuthEntityAssociation != null) && (requisition.ReqAuthEntityAssociation.Count > 0))
                        {
                            foreach (var approval in requisition.ReqAuthEntityAssociation)
                            {
                                if (approval.ReqAuthorizationsAssocMember == appr.Recordkey)
                                {
                                    approver.ApprovalDate = approval.ReqAuthorizationDatesAssocMember.Value;
                                }
                            }
                        }

                        // Add any approvals to the requisition domain entity
                        requisitionDomainEntity.AddApprover(approver);
                    }
                }
            }

            // Populate the line item domain entities and add them to the requisition domain entity
            var lineItemIds = requisition.ReqItemsId;
            int documentFiscalYear = 0;
            if (lineItemIds != null && lineItemIds.Count() > 0)
            {
                // determine the fiscal year from the requisition date or maintenance date
                DateTime? transactionDate = requisition.ReqDate;
                if (requisition.ReqMaintGlTranDate != null)
                {
                    transactionDate = requisition.ReqMaintGlTranDate;
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
                logger.Debug(string.Format("requisition fiscal year {0} ", documentFiscalYear));

                // Read the item records for the list of IDs in the requisition record
                var lineItemRecords = await DataReader.BulkReadRecordAsync<Items>(lineItemIds.ToArray());
                if ((lineItemRecords != null) && (lineItemRecords.Count > 0))
                {
                    // If the user has the full access GL role, they have access to all GL accounts.
                    // There is no need to check for GL account access security. If they have partial 
                    // access, we need to call the CTX to check security.

                    bool hasGlAccess = false;
                    List<string> glAccountsAllowed = new List<string>();
                    if (glAccessLevel == GlAccessLevel.Full_Access)
                    {
                        hasGlAccess = true;
                    }
                    else if (glAccessLevel == GlAccessLevel.Possible_Access)
                    {
                        if (CanUserByPassGlAccessCheck(personId, requisition, requisitionDomainEntity))
                        {
                            hasGlAccess = true;
                        }
                        else
                        {
                            // Put together a list of unique GL accounts for all the items
                            foreach (var lineItem in lineItemRecords)
                            {
                                if ((lineItem.ItemReqEntityAssociation != null) && (lineItem.ItemReqEntityAssociation.Count > 0))
                                {
                                    foreach (var glDist in lineItem.ItemReqEntityAssociation)
                                    {
                                        if (expenseAccounts.Contains(glDist.ItmReqGlNoAssocMember))
                                        {
                                            hasGlAccess = true;
                                            glAccountsAllowed.Add(glDist.ItmReqGlNoAssocMember);
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

                    // If this requisition has a currency code, the requisition amount has to be in foreign currency.
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

                        decimal itemQuantity = lineItem.ItmReqQty.HasValue ? lineItem.ItmReqQty.Value : 0;
                        decimal itemPrice = lineItem.ItmReqPrice.HasValue ? lineItem.ItmReqPrice.Value : 0;
                        decimal extendedPrice = lineItem.ItmReqExtPrice.HasValue ? lineItem.ItmReqExtPrice.Value : 0;

                        LineItem lineItemDomainEntity = new LineItem(lineItem.Recordkey, itemDescription, itemQuantity, itemPrice, extendedPrice);

                        if (lineItem.ItmDesiredDeliveryDate != null)
                        {
                            lineItemDomainEntity.DesiredDate = lineItem.ItmDesiredDeliveryDate.Value.Date;
                        }
                        lineItemDomainEntity.UnitOfIssue = lineItem.ItmReqIssue;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;
                        lineItemDomainEntity.TaxForm = lineItem.ItmTaxForm;
                        lineItemDomainEntity.TaxFormCode = lineItem.ItmTaxFormCode;
                        lineItemDomainEntity.TaxFormLocation = lineItem.ItmTaxFormLoc;
                        var comments = string.Empty;
                        if (!string.IsNullOrEmpty(lineItem.ItmComments))
                        {
                            comments = CommentsUtility.ConvertCommentsToParagraphs(lineItem.ItmComments);
                        }
                        lineItemDomainEntity.Comments = comments;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;
                        lineItemDomainEntity.CommodityCode = lineItem.ItmCommodityCode;
                        lineItemDomainEntity.FixedAssetsFlag = lineItem.ItmFixedAssetsFlag;
                        lineItemDomainEntity.TradeDiscountAmount = lineItem.ItmReqTradeDiscAmt;
                        lineItemDomainEntity.TradeDiscountPercentage = lineItem.ItmReqTradeDiscPct;

                        //Populate the Line Item tax codes for Requisition
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
                        if ((lineItem.ItemReqEntityAssociation != null) && (lineItem.ItemReqEntityAssociation.Count > 0))
                        {
                            foreach (var glDist in lineItem.ItemReqEntityAssociation)
                            {
                                // build a list of unique list of GL numbers on the line item GL distributions
                                // if the user has full GL access.
                                if (glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, requisition, requisitionDomainEntity))
                                {
                                    if (!availableFundsGlAccounts.Contains(glDist.ItmReqGlNoAssocMember))
                                    {
                                        availableFundsGlAccounts.Add(glDist.ItmReqGlNoAssocMember);
                                    }
                                }

                                // The GL Distribution always uses the local currency amount.
                                decimal gldistGlQty = glDist.ItmReqGlQtyAssocMember.HasValue ? glDist.ItmReqGlQtyAssocMember.Value : 0;
                                decimal gldistGlAmount = glDist.ItmReqGlAmtAssocMember.HasValue ? glDist.ItmReqGlAmtAssocMember.Value : 0;
                                decimal gldistGLPct = glDist.ItmReqGlPctAssocMember.HasValue ? glDist.ItmReqGlPctAssocMember.Value : 0;
                                LineItemGlDistribution glDistribution = new LineItemGlDistribution(glDist.ItmReqGlNoAssocMember, gldistGlQty, gldistGlAmount, gldistGLPct);

                                if (!(string.IsNullOrEmpty(glDist.ItmReqProjectCfIdAssocMember)))
                                {
                                    glDistribution.ProjectId = glDist.ItmReqProjectCfIdAssocMember;
                                    if (!itemProjectIds.Contains(glDist.ItmReqProjectCfIdAssocMember))
                                    {
                                        itemProjectIds.Add(glDist.ItmReqProjectCfIdAssocMember);
                                    }
                                }

                                if (!(string.IsNullOrEmpty(glDist.ItmReqPrjItemIdsAssocMember)))
                                {
                                    glDistribution.ProjectLineItemId = glDist.ItmReqPrjItemIdsAssocMember;
                                    if (!itemProjectLineIds.Contains(glDist.ItmReqPrjItemIdsAssocMember))
                                    {
                                        itemProjectLineIds.Add(glDist.ItmReqPrjItemIdsAssocMember);
                                    }
                                }

                                lineItemDomainEntity.AddGlDistribution(glDistribution);

                                // Check the currency code to see if we need the local or foreign amount
                                if (string.IsNullOrEmpty(requisition.ReqCurrencyCode))
                                {
                                    requisitionDomainEntity.Amount += glDist.ItmReqGlAmtAssocMember.HasValue ? glDist.ItmReqGlAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    requisitionDomainEntity.Amount += glDist.ItmReqGlForeignAmtAssocMember.HasValue ? glDist.ItmReqGlForeignAmtAssocMember.Value : 0;
                                }
                            }
                        }

                        // Add taxes to the line item
                        if ((lineItem.ReqGlTaxesEntityAssociation != null) && (lineItem.ReqGlTaxesEntityAssociation.Count > 0))
                        {
                            var sortedGlTaxesAssociation = lineItem.ReqGlTaxesEntityAssociation.OrderBy(x => x.ItmReqLineGlNoAssocMember);
                            foreach (var taxGlDist in lineItem.ReqGlTaxesEntityAssociation)
                            {
                                decimal itemTaxAmount = 0;
                                string lineItemTaxCode = taxGlDist.ItmReqGlTaxCodeAssocMember;

                                if (taxGlDist.ItmReqGlForeignTaxAmtAssocMember.HasValue)
                                {
                                    itemTaxAmount = taxGlDist.ItmReqGlForeignTaxAmtAssocMember.HasValue ? taxGlDist.ItmReqGlForeignTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    itemTaxAmount = taxGlDist.ItmReqGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmReqGlTaxAmtAssocMember.Value : 0;
                                }

                                LineItemTax itemTax = new LineItemTax(lineItemTaxCode, itemTaxAmount);

                                lineItemDomainEntity.AddTax(itemTax);

                                if (string.IsNullOrEmpty(requisition.ReqCurrencyCode))
                                {
                                    requisitionDomainEntity.Amount += taxGlDist.ItmReqGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmReqGlTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    requisitionDomainEntity.Amount += taxGlDist.ItmReqGlForeignTaxAmtAssocMember.HasValue ? taxGlDist.ItmReqGlForeignTaxAmtAssocMember.Value : 0;
                                }
                            }
                        }

                        // Now apply GL account security to the line items.
                        // If hasGlAccess is true, it indicates the user has full access or has some
                        // access to the GL accounts in this requisition. If hasGlAccess if false, 
                        // no line items will be added to the requisition domain entity.


                        if (hasGlAccess == true)
                        {
                            // Now apply GL account access security when creating the line items.
                            // Check to see if the user has access to the GL accounts for each line item:
                            // - if they do not have access to any of them, we will not add the line item to the requisition domain entity.
                            // - if the user has access to some of the GL accounts, the ones they do not have access to will be masked.

                            bool addItem = false;
                            if (glAccessLevel == GlAccessLevel.Full_Access)
                            {
                                // The user has full access and there is no need to check further
                                addItem = true;
                            }
                            else
                            {
                                if (CanUserByPassGlAccessCheck(personId, requisition, requisitionDomainEntity))
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
                                requisitionDomainEntity.AddLineItem(lineItemDomainEntity);
                            }
                        }

                    }

                    // If there are project IDs, we need to get the project number,
                    // and also the project line item code for each project line item ID 
                    if ((itemProjectIds != null) && (itemProjectIds.Count > 0))
                    {
                        // For each project ID, get the project number
                        var projectRecords = await DataReader.BulkReadRecordAsync<Projects>(itemProjectIds.ToArray());

                        // If there are project IDs, there should be project line item IDs
                        if ((itemProjectLineIds != null) && (itemProjectLineIds.Count > 0))
                        {
                            // For each project line item ID, get the project line item code
                            var projectLineItemRecords = await DataReader.BulkReadRecordAsync<ProjectsLineItems>(itemProjectLineIds.ToArray());

                            if ((projectRecords != null) && (projectRecords.Count > 0))
                            {
                                for (int i = 0; i < requisitionDomainEntity.LineItems.Count(); i++)
                                {
                                    foreach (var glDist in requisitionDomainEntity.LineItems[i].GlDistributions)
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
                    // information on disk and add the amounts that are on the requisition if the requisition status is
                    // In Progress.
                    if (glAccountsAllowed.Any() || ((glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, requisition, requisitionDomainEntity)) && availableFundsGlAccounts.Any()))
                    {
                        logger.Debug(string.Format("Calculating funds availability"));
                        string[] fundsAvailabilityArray;
                        if ((glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, requisition, requisitionDomainEntity)) && availableFundsGlAccounts.Any())
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

                                    // if the requisition status is In Progress, add the GL amounts from the line item 
                                    // GL distributions to the requisition funds availability information.
                                    if (requisition.ReqStatus.First().ToUpper() == "U")
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

                                        foreach (var lineItem in requisitionDomainEntity.LineItems)
                                        {
                                            if (lineItem != null)
                                            {
                                                foreach (var glDistribution in lineItem.GlDistributions)
                                                {
                                                    if (glDistribution != null)
                                                    {
                                                        if (glDistribution.GlAccountNumber == glAccount || budgetPoolAccounts.Contains(glDistribution.GlAccountNumber))
                                                        {
                                                            accountRequisitionAmount += glDistribution.Amount;
                                                        }
                                                    }
                                                }

                                                // Add tax amounts that are distributed to the GL distribution account to funds availability.
                                                foreach (var item in lineItemRecords)
                                                {
                                                    if ((item.ReqGlTaxesEntityAssociation != null) && (item.ReqGlTaxesEntityAssociation.Any()))
                                                        {
                                                        foreach (var taxAssociation in item.ReqGlTaxesEntityAssociation)
                                                        {
                                                            if (taxAssociation != null)
                                                            {
                                                                if (taxAssociation.ItmReqLineGlNoAssocMember == glAccount || budgetPoolAccounts.Contains(taxAssociation.ItmReqLineGlNoAssocMember))
                                                                {
                                                                    if (taxAssociation.ItmReqGlTaxAmtAssocMember.HasValue)
                                                                    {
                                                                        accountRequisitionAmount += taxAssociation.ItmReqGlTaxAmtAssocMember.Value;
                                                                    }
                                                                }
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
                                    foreach (var lineItem in requisitionDomainEntity.LineItems)
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

            return requisitionDomainEntity;
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
        /// Get a collection of requisition summary domain entity objects
        /// </summary>
        /// <param name="id">Person ID</param>        
        /// <returns>collection of requisition summary domain entity objects</returns>
        public async Task<IEnumerable<RequisitionSummary>> GetRequisitionsSummaryByPersonIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            var cfWebDefaults = await DataReader.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS");

            var filteredRequisitions = await ApplyFilterCriteriaAsync(personId, cfWebDefaults);

            if (filteredRequisitions == null || !filteredRequisitions.Any())
            {
                return null;
            }
            return await BuildRequisitionSummaryList(personId, filteredRequisitions);
        }

        /// <summary>
        /// Get a collection of requisition domain entity objects
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <returns>collection of requisition domain entity objects</returns>
        public async Task<Tuple<IEnumerable<Requisition>, int>> GetRequisitionsAsync(int offset, int limit, string requisitionNumber, string referenceNumber)
        {
            int totalCount = 0;
            string[] subList = null;
            string reqCacheKey = CacheSupport.BuildCacheKey(AllRequisitionsCache);
            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                   this,
                   ContainsKey,
                   GetOrAddToCacheAsync,
                   AddOrUpdateCacheAsync,
                   transactionInvoker,
                   reqCacheKey,
                   "REQUISITIONS",
                   offset,
                   limit,
                   AllRequisitionCacheTimeout,
                   async () =>
                   {
                       var criteria = "WITH REQ.CURRENT.STATUS NE 'U'";
                       if (!string.IsNullOrEmpty(requisitionNumber))
                       {
                           criteria = string.Format("{0} AND WITH REQ.NO = '{1}'", criteria, requisitionNumber);
                       }
                       if (!string.IsNullOrEmpty(referenceNumber))
                       {
                           criteria = string.Format("{0} AND WITH REQ.REFERENCE.NO = '{1}'", criteria, referenceNumber);
                       }
                       var requirements = new CacheSupport.KeyCacheRequirements()
                       {
                           criteria = criteria
                       };
                       return requirements;
                   }
               );

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<Requisition>, int>(new List<Requisition>(), 0);
            }

            subList = keyCache.Sublist.ToArray();
            totalCount = keyCache.TotalCount.Value;
            IEnumerable<Requisition> requisitions = null;
            try
            {

                var requisitionData = await DataReader.BulkReadRecordAsync<DataContracts.Requisitions>("REQUISITIONS", subList);
                requisitions = await BuildRequisitionsAsync(requisitionData);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<Requisition>, int>(requisitions, totalCount);

        }

        /// <summary>
        /// Get a single requisition domain entity object by guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Requisition> GetRequisitionsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var id = await GetRequisitionsIdFromGuidAsync(guid);

            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("No requisitions was found for GUID " + guid);
            }
            var requisitionData = await DataReader.ReadRecordAsync<Requisitions>(id);
            if (requisitionData == null)
            {
                throw new KeyNotFoundException("No requisitions was found for GUID " + guid);
            }
            // exclude those PO that are inprogress
            if (requisitionData != null && requisitionData.ReqStatus != null && requisitionData.ReqStatus.Any() && requisitionData.ReqStatus.FirstOrDefault().Equals("U", StringComparison.OrdinalIgnoreCase))
            {
                //throw new RepositoryException("The guid specified " + guid + " for record key " + requisitionData.Recordkey + " from file REQUISITIONS is not valid for requisitions.");
                throw new RepositoryException("Requisitions at a current status of 'Unfinished' cannot be viewed or modified");
            }
            var retval = await BuildRequisitionAsync(requisitionData);
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return retval;

        }

        /// <summary>
        /// Get a requisition id by guid
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>id</returns>
        public async Task<string> GetRequisitionsIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("No requisitions was found for GUID " + guid);
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("No requisitions was found for GUID " + guid);
            }

            if (foundEntry.Value.Entity != "REQUISITIONS")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, REQUISITIONS");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Delete a single Requisition
        /// </summary>
        /// <param name="id">The requested Requisition guid</param>
        /// <returns></returns>
        public async Task<Requisition> DeleteRequisitionAsync(string guid)
        {
            var recordInfo = await GetRecordInfoFromGuidAsync(guid);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "REQUISITIONS")
            {
                throw new KeyNotFoundException(string.Format("Requisition record {0} does not exist.", guid));
            }
            var request = new DeleteRequisitionRequest
            {
                RequisitionsId = recordInfo.PrimaryKey,
                Guid = guid
            };

            var response = await transactionInvoker.ExecuteAsync<DeleteRequisitionRequest, DeleteRequisitionResponse>(request);

            if (response.ErrorMessages != null && response.ErrorMessages.Any())
            {
                var errorMessagesString = string.Join(Environment.NewLine, response.ErrorMessages);
                logger.Error(errorMessagesString);
                if (response.ErrorCode.Contains("Requisition.MissingRecord"))
                {
                    throw new KeyNotFoundException(errorMessagesString);
                }
                else
                {
                    throw new ApplicationException(errorMessagesString);
                }
            }
            return null;
        }

        /// <summary>
        ///  Build Requisition for delete Request
        /// </summary>
        /// <param name="deleteRequest"></param>
        /// <returns></returns>
        private TxDeleteRequisitionRequest BuildRequisitionDeleteRequest(RequisitionDeleteRequest deleteRequest)
        {
            var request = new TxDeleteRequisitionRequest();
            var personId = deleteRequest.PersonId;
            var requisitionId = deleteRequest.RequisitionId;
            var confirmationEmailAddresses = deleteRequest.ConfirmationEmailAddresses;

            if (!string.IsNullOrEmpty(personId))
            {
                request.AUserId = personId;
            }
            if (!string.IsNullOrEmpty(requisitionId))
            {
                request.ARequisitionId = requisitionId;
            }
            if (!string.IsNullOrEmpty(confirmationEmailAddresses))
            {
                request.AConfirmationEmailAddress = confirmationEmailAddresses;
            }

            return request;

        }

        /// <summary>
        /// Update a requisition 
        /// </summary>
        /// <param name="requisitionEntity">requisition domain entity to update</param>
        /// <returns>updated requisition domain entity</returns>
        public async Task<Requisition> UpdateRequisitionAsync(Requisition requisitionEntity)
        {
            if (requisitionEntity == null)
                throw new ArgumentNullException("requisitionEntity", "Must provide a requisitionEntity to update.");
            if (string.IsNullOrEmpty(requisitionEntity.Guid))
                throw new ArgumentNullException("requisitionEntity", "Must provide the guid of the requisitionEntity to update.");

            var updateRequest = BuildRequisitionUpdateRequest(requisitionEntity);

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<UpdateCreateRequisitionRequest, UpdateCreateRequisitionResponse>(updateRequest);

            if (updateResponse.ReqErrors.Any())
            {
                var exception = new RepositoryException();
                updateResponse.ReqErrors.ForEach(e => exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(!string.IsNullOrEmpty(e.ErrorCodes) ? e.ErrorCodes + ": " : "", e.ErrorMessages))));
                throw exception;
            }

            // get the updated entity from the database
            return await GetRequisitionsByGuidAsync(requisitionEntity.Guid);

        }

        /// <summary>
        /// Create a requisition
        /// </summary>
        /// <param name="requisitionEntity">requisition domain entity to create</param>
        /// <returns>created requisition domain entity</returns>
        public async Task<Requisition> CreateRequisitionAsync(Requisition requisitionEntity)
        {
            if (requisitionEntity == null)
                throw new ArgumentNullException("requisitionEntity", "Must provide a requisitionEntity to create.");

            var createRequest = BuildRequisitionUpdateRequest(requisitionEntity);
            createRequest.ReqId = null;

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdateCreateRequisitionRequest, UpdateCreateRequisitionResponse>(createRequest);

            if (createResponse.ReqErrors.Any())
            {
                var exception = new RepositoryException();
                createResponse.ReqErrors.ForEach(e => exception.AddError(new RepositoryError("requisition", e.ErrorMessages)));
                throw exception;
            }

            // get the newly created requisition from the database
            return await GetRequisitionsByGuidAsync(createResponse.Guid);
        }

        /// <summary>
        /// Create / Update a requisition.
        /// </summary>       
        /// <param name="requisitionCreateUpdateRequest">The requisition create update request domain entity.</param>        
        /// <returns>The requisition create update response entity</returns>
        public async Task<RequisitionCreateUpdateResponse> CreateRequisitionsAsync(RequisitionCreateUpdateRequest createUpdateRequest)
        {
            if (createUpdateRequest == null)
                throw new ArgumentNullException("requisitionEntity", "Must provide a createUpdateRequest to create.");

            RequisitionCreateUpdateResponse response = new RequisitionCreateUpdateResponse();
            var createRequest = BuildRequisitionCreateRequest(createUpdateRequest);

            try
            {
                // write the  data
                var createResponse = await transactionInvoker.ExecuteAsync<TxCreateWebRequisitionRequest, TxCreateWebRequisitionResponse>(createRequest);
                if (string.IsNullOrEmpty(createResponse.AError) && (createResponse.AlErrorMessages == null || !createResponse.AlErrorMessages.Any()))
                {
                    response.ErrorOccured = false;
                    response.ErrorMessages = new List<string>();
                    response.RequisitionId = createResponse.ARequisitionId;
                    response.RequisitionNumber = createResponse.ARequisitionNo;
                    response.RequisitionDate = createResponse.AReqDate.Value;
                }
                else
                {
                    response.ErrorOccured = true;
                    response.ErrorMessages = createResponse.AlErrorMessages;
                    response.ErrorMessages.RemoveAll(message => string.IsNullOrEmpty(message));
                }
                // The warning flag can contain the number of warnings.
                response.WarningOccured = (!string.IsNullOrWhiteSpace(createResponse.AWarning) && createResponse.AWarning != "0") ? true : false;
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
        /// Update a requisition.
        /// </summary>       
        /// <param name="requisitionCreateUpdateRequest">The requisition create update request domain entity.</param>        
        /// <returns>The requisition create update response entity</returns>
        public async Task<RequisitionCreateUpdateResponse> UpdateRequisitionsAsync(RequisitionCreateUpdateRequest createUpdateRequest, Requisition originalRequisition)
        {
            if (createUpdateRequest == null)
                throw new ArgumentNullException("createUpdateRequest", "Must provide a createUpdateRequest entity to update.");

            RequisitionCreateUpdateResponse response = new RequisitionCreateUpdateResponse();
            var updateRequest = BuildRequisitionUpdateRequest(createUpdateRequest, originalRequisition);

            try
            {
                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<TxUpdateWebRequisitionRequest, TxUpdateWebRequisitionResponse>(updateRequest);
                if (string.IsNullOrEmpty(updateResponse.AError) && (updateResponse.AlErrorMessages == null || !updateResponse.AlErrorMessages.Any()))
                {
                    response.ErrorOccured = false;
                    response.ErrorMessages = new List<string>();
                    response.RequisitionId = updateResponse.ARequisitionId;
                    response.RequisitionNumber = createUpdateRequest.Requisition.Number;
                    response.RequisitionDate = createUpdateRequest.Requisition.Date;
                }
                else
                {
                    response.ErrorOccured = true;
                    response.ErrorMessages = updateResponse.AlErrorMessages;
                    response.ErrorMessages.RemoveAll(message => string.IsNullOrEmpty(message));
                }
                // The warning flag can contain the number of warnings.
                response.WarningOccured = (!string.IsNullOrWhiteSpace(updateResponse.AWarning) && updateResponse.AWarning != "0") ? true : false;
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
        /// Delete a requisition.
        /// </summary>       
        /// <param name="RequisitionDeleteRequest">The requisition delete request domain entity.</param>        
        /// <returns>The requisition delete response entity</returns>
        public async Task<RequisitionDeleteResponse> DeleteRequisitionsAsync(RequisitionDeleteRequest deleteRequest)
        {
            if (deleteRequest == null)
                throw new ArgumentNullException("deleteRequestEntity", "Must provide a deleteRequestEntity to delete a requisition.");

            RequisitionDeleteResponse response = new RequisitionDeleteResponse();
            var request = BuildRequisitionDeleteRequest(deleteRequest);

            try
            {
                // write the  data
                var deleteResponse = await transactionInvoker.ExecuteAsync<TxDeleteRequisitionRequest, TxDeleteRequisitionResponse>(request);
                if (!(deleteResponse.AErrorOccurred) && (deleteResponse.AlErrorMessages == null || !deleteResponse.AlErrorMessages.Any()))
                {
                    response.ErrorOccured = false;
                    response.ErrorMessages = new List<string>();
                }
                else
                {
                    response.ErrorOccured = true;
                    response.ErrorMessages = deleteResponse.AlErrorMessages;
                    response.ErrorMessages.RemoveAll(message => string.IsNullOrEmpty(message));
                }

                response.RequisitionId = deleteResponse.ARequisitionId;
                response.RequisitionNumber = deleteResponse.ARequisitionNumber;

                response.WarningOccured = deleteResponse.AWarningOccurred;
                response.WarningMessages = (deleteResponse.AlWarningMessages != null || deleteResponse.AlWarningMessages.Any()) ? deleteResponse.AlWarningMessages : new List<string>();

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw;
            }

            return response;
        }

        /// <summary>
        /// Get a list of requisition summary domain entity objects
        /// </summary>
        /// <param name="criteria">procurement filter criteria</param>
        /// <returns>list of requisition summary domain entity objects</returns>
        public async Task<IEnumerable<RequisitionSummary>> QueryRequisitionSummariesAsync(ProcurementDocumentFilterCriteria criteria)
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
            var filteredRequisitionIds = await DataReader.SelectAsync("REQUISITIONS", queryCriteria);

            if (filteredRequisitionIds == null || !filteredRequisitionIds.Any())
            {
                logger.Debug(string.Format("Requisitions not found for query string: '{0}'.", queryCriteria));
                return null;
            }
            logger.Info(string.Format("Requisitions count {0} found.", filteredRequisitionIds.ToList().Count()));
            return await BuildRequisitionSummaryList(personId, filteredRequisitionIds.ToList());
        }

        /// <summary>
        /// Build requisition domain entity from a requisitions data contract
        /// </summary>
        /// <param name="requisitionDataContract">requisitions data contract</param>
        /// <returns>requisition domain entity</returns>
        private async Task<Requisition> BuildRequisitionAsync(Requisitions requisitionDataContract)
        {

            if (requisitionDataContract == null)
            {
                //throw new ArgumentNullException("requisition");
                exception.AddError(new RepositoryError("Bad.Data", "Requisition data contract is required."));
                return null;
            }

            if (string.IsNullOrEmpty(requisitionDataContract.Recordkey))
            {
                exception.AddError(new RepositoryError("Bad.Data", "Requisition record key is required."));
            }

            if (!requisitionDataContract.ReqDate.HasValue)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Missing date for requisition.")
                {
                    SourceId = requisitionDataContract.Recordkey,
                    Id = requisitionDataContract.RecordGuid
                });
            }

            if (requisitionDataContract.ReqStatusDate == null || !requisitionDataContract.ReqStatusDate.FirstOrDefault().HasValue)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Missing status date for requisition.")
                {
                    SourceId = requisitionDataContract.Recordkey,
                    Id = requisitionDataContract.RecordGuid
                });
            }

            RequisitionStatus? requisitionStatus = null;
            try
            {
                requisitionStatus = ConvertRequisitionStatus(requisitionDataContract.ReqStatus, requisitionDataContract.Recordkey);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    SourceId = requisitionDataContract.Recordkey,
                    Id = requisitionDataContract.RecordGuid
                });
            }
            // constructor expects record key and guid as required fields. exit prior to constructor errors being thrown.
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                return null;
            }

            DateTime requisitionStatusDate = requisitionDataContract.ReqStatusDate.First().Value;

            var requisitionDomainEntity = new Requisition(requisitionDataContract.Recordkey, requisitionDataContract.RecordGuid,
            requisitionDataContract.ReqNo, string.Empty, requisitionStatus.Value, requisitionStatusDate, requisitionDataContract.ReqDate.Value.Date);

            if (requisitionDataContract.ReqReferenceNo != null && requisitionDataContract.ReqReferenceNo.Any())
            {
                requisitionDomainEntity.ReferenceNumbers = requisitionDataContract.ReqReferenceNo;
            }

            var glAccessLevel = GlAccessLevel.Full_Access;
            var expenseAccounts = new List<string>();

            requisitionDomainEntity.VendorId = requisitionDataContract.ReqVendor;

            requisitionDomainEntity.DefaultInitiator = requisitionDataContract.ReqDefaultInitiator;
            if (!(string.IsNullOrEmpty(requisitionDataContract.ReqDefaultInitiator)))
            {
                var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>("PERSON", requisitionDataContract.ReqDefaultInitiator);
                if (personRecord != null)
                {
                    var initiatorName = string.Concat(personRecord.FirstName, " ", personRecord.LastName);
                    if (!string.IsNullOrWhiteSpace(initiatorName))
                    {
                        requisitionDomainEntity.InitiatorName = initiatorName;
                    }
                }
            }
            requisitionDomainEntity.Type = requisitionDataContract.ReqIntgType;
            requisitionDomainEntity.Amount = 0;
            requisitionDomainEntity.CurrencyCode = requisitionDataContract.ReqCurrencyCode;
            if (requisitionDataContract.ReqMaintGlTranDate.HasValue)
            {
                requisitionDomainEntity.MaintenanceDate = requisitionDataContract.ReqMaintGlTranDate.Value.Date;
            }

            if (requisitionDataContract.ReqDesiredDeliveryDate.HasValue)
            {
                requisitionDomainEntity.DesiredDate = requisitionDataContract.ReqDesiredDeliveryDate.Value.Date;
            }

            requisitionDomainEntity.Buyer = requisitionDataContract.ReqBuyer;
            requisitionDomainEntity.HostCountry = await GetHostCountryAsync();

            requisitionDomainEntity.ApType = requisitionDataContract.ReqApType;
            requisitionDomainEntity.Comments = requisitionDataContract.ReqPrintedComments;
            requisitionDomainEntity.InternalComments = requisitionDataContract.ReqComments;
            requisitionDomainEntity.ShipToCode = requisitionDataContract.ReqShipTo;
            if (string.IsNullOrEmpty(requisitionDataContract.ReqShipTo))
            {
                var requisitionDefaults = await DataReader.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS");
                if (requisitionDefaults != null)
                {
                    requisitionDomainEntity.ShipToCode = requisitionDefaults.PurShipToCode;
                }
            }
            requisitionDomainEntity.CommodityCode = requisitionDataContract.ReqDefaultCommodity;
            requisitionDomainEntity.Fob = requisitionDataContract.ReqFob;
            requisitionDomainEntity.VendorTerms = requisitionDataContract.ReqVendorTerms;

            requisitionDomainEntity.VendorAlternativeAddressId = requisitionDataContract.ReqIntgAddressId;
            requisitionDomainEntity.AltShippingAddress = requisitionDataContract.ReqAltShipAddress;
            requisitionDomainEntity.AltShippingCity = requisitionDataContract.ReqAltShipCity;

            requisitionDomainEntity.AltShippingName = requisitionDataContract.ReqAltShipName;
            requisitionDomainEntity.AltShippingPhone = requisitionDataContract.ReqAltShipPhone;
            requisitionDomainEntity.AltShippingPhoneExt = requisitionDataContract.ReqAltShipExt;
            requisitionDomainEntity.AltShippingState = requisitionDataContract.ReqAltShipState;
            requisitionDomainEntity.AltShippingZip = requisitionDataContract.ReqAltShipZip;

            requisitionDomainEntity.UseAltAddress = (!string.IsNullOrEmpty(requisitionDataContract.ReqAltFlag) && requisitionDataContract.ReqAltFlag.ToUpper() == "Y" ? true : false);

            requisitionDomainEntity.MiscName = requisitionDataContract.ReqMiscName;
            requisitionDomainEntity.MiscAddress = requisitionDataContract.ReqMiscAddress;
            requisitionDomainEntity.MiscCity = requisitionDataContract.ReqMiscCity;
            requisitionDomainEntity.MiscState = requisitionDataContract.ReqMiscState;
            requisitionDomainEntity.MiscZip = requisitionDataContract.ReqMiscZip;
            requisitionDomainEntity.MiscCountry = requisitionDataContract.ReqMiscCountry;

            requisitionDomainEntity.IntgAltShipCountry = requisitionDataContract.ReqIntgAltShipCountry;
            requisitionDomainEntity.IntgCorpPerIndicator = requisitionDataContract.ReqIntgCorpPerIndicator;
            requisitionDomainEntity.IntgSubmittedBy = requisitionDataContract.ReqIntgSubmittedBy;

            if (!(string.IsNullOrEmpty(requisitionDataContract.ReqVendor)))
            {
                var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>("PERSON", requisitionDataContract.ReqVendor);
                if (personRecord != null)
                {
                    if (!string.IsNullOrWhiteSpace(personRecord.PreferredAddress))
                    {
                        requisitionDomainEntity.VendorPreferredAddressId = personRecord.PreferredAddress;
                    }
                }
            }


            // Add any associated purchase orders to the requisition domain entity
            if ((requisitionDataContract.ReqPoNo != null) && (requisitionDataContract.ReqPoNo.Count > 0))
            {
                foreach (var purchaseOrderId in requisitionDataContract.ReqPoNo)
                {
                    if (!string.IsNullOrEmpty(purchaseOrderId))
                    {
                        try
                        {
                            requisitionDomainEntity.AddPurchaseOrder(purchaseOrderId);
                        }
                        catch (Exception ex)
                        {
                            exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                            {
                                SourceId = requisitionDataContract.Recordkey,
                                Id = requisitionDataContract.RecordGuid
                            });
                        }
                    }
                }
            }

            // Add any associated blanket purchase orders to the requisition domain entity.
            // Even though ReqBpoNo is a list of string, only one bpo can be associated to a requisition
            if ((requisitionDataContract.ReqBpoNo != null) && (requisitionDataContract.ReqBpoNo.Count > 0))
            {
                if (requisitionDataContract.ReqBpoNo.Count > 1)
                {
                    exception.AddError(new RepositoryError("Bad.Data", "Only one blanket purchase order can be associated with the requisition.")
                    {
                        SourceId = requisitionDataContract.Recordkey,
                        Id = requisitionDataContract.RecordGuid
                    });
                }
                else
                {
                    requisitionDomainEntity.BlanketPurchaseOrder = requisitionDataContract.ReqBpoNo.FirstOrDefault();
                }
            }

            // Populate the line item domain entities and add them to the requisition domain entity
            try
            {
                await GetLineItems(glAccessLevel, expenseAccounts, requisitionDataContract, requisitionDomainEntity, requisitionDataContract.ReqItemsId);
            }
            catch (Exception ex)
            {
                //throw new RepositoryException(ex.Message + ". Missing GL details for line items for requistion id : " + requisitionDataContract.Recordkey);
                exception.AddError(new RepositoryError("Bad.Data", "An exception occurred obtaining the GL details for line items for requistion. " + ex.Message)
                {
                    SourceId = requisitionDataContract.Recordkey,
                    Id = requisitionDataContract.RecordGuid
                });
            }

            return requisitionDomainEntity;
        }

        /// <summary>
        /// Take first value from a Requisition Status collection and convert to RequisitionStatus enumeration value
        /// </summary>
        /// <param name="reqStatus">requisition status</param>
        /// <param name="requisitionId">requisition id</param>
        /// <returns>RequisitionStatus enumeration value</returns>
        private static RequisitionStatus ConvertRequisitionStatus(List<string> reqStatus, string requisitionId)
        {
            var requisitionStatus = new RequisitionStatus();

            // Get the first status in the list of requisition statuses and check it has a value
            if ((reqStatus) != null && (reqStatus.Any()))
            {
                switch (reqStatus.FirstOrDefault().ToUpper())
                {
                    case "U":
                        requisitionStatus = RequisitionStatus.InProgress;
                        break;
                    case "N":
                        requisitionStatus = RequisitionStatus.NotApproved;
                        break;
                    case "O":
                        requisitionStatus = RequisitionStatus.Outstanding;
                        break;
                    case "P":
                        requisitionStatus = RequisitionStatus.PoCreated;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid requisition status for requisition: " + requisitionId);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for requisition: " + requisitionId);
            }

            return requisitionStatus;
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
        /// Get Line Item
        /// </summary>
        /// <param name="glAccessLevel"></param>
        /// <param name="expenseAccounts"></param>
        /// <param name="requisition"></param>
        /// <param name="requisitionDomainEntity"></param>
        /// <param name="lineItemIds"></param>
        /// <returns></returns>
        private async Task GetLineItems(GlAccessLevel glAccessLevel, List<string> expenseAccounts, Requisitions requisition, Requisition requisitionDomainEntity, List<string> lineItemIds)
        {
            if (lineItemIds != null && lineItemIds.Count() > 0)
            {
                // Read the item records for the list of IDs in the requisition record
                var lineItemRecords = await DataReader.BulkReadRecordAsync<Items>(lineItemIds.ToArray());
                if ((lineItemRecords != null) && (lineItemRecords.Count > 0))
                {
                    // If the user has the full access GL role, they have access to all GL accounts.
                    // There is no need to check for GL account access security. If they have partial 
                    // access, we need to call the CTX to check security.

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
                            if ((lineItem.ItemReqEntityAssociation != null) && (lineItem.ItemReqEntityAssociation.Count > 0))
                            {
                                foreach (var glDist in lineItem.ItemReqEntityAssociation)
                                {
                                    if (expenseAccounts.Contains(glDist.ItmReqGlNoAssocMember))
                                    {
                                        hasGlAccess = true;
                                        glAccountsAllowed.Add(glDist.ItmReqGlNoAssocMember);
                                    }
                                }
                            }
                        }
                    }

                    List<string> itemProjectIds = new List<string>();
                    List<string> itemProjectLineIds = new List<string>();

                    // If this requisition has a currency code, the requisition amount has to be in foreign currency.
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

                        decimal itemQuantity = lineItem.ItmReqQty.HasValue ? lineItem.ItmReqQty.Value : 0;
                        decimal itemPrice = lineItem.ItmReqPrice.HasValue ? lineItem.ItmReqPrice.Value : 0;
                        decimal extendedPrice = lineItem.ItmReqExtPrice.HasValue ? lineItem.ItmReqExtPrice.Value : 0;

                        LineItem lineItemDomainEntity = new LineItem(lineItem.Recordkey, itemDescription, itemQuantity, itemPrice, extendedPrice);

                        if (lineItem.ItmDesiredDeliveryDate != null)
                        {
                            lineItemDomainEntity.DesiredDate = lineItem.ItmDesiredDeliveryDate.Value.Date;
                        }
                        lineItemDomainEntity.UnitOfIssue = lineItem.ItmReqIssue;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;
                        lineItemDomainEntity.TaxForm = lineItem.ItmTaxForm;
                        lineItemDomainEntity.TaxFormCode = lineItem.ItmTaxFormCode;
                        lineItemDomainEntity.TaxFormLocation = lineItem.ItmTaxFormLoc;
                        lineItemDomainEntity.Comments = lineItem.ItmComments;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;

                        lineItemDomainEntity.CommodityCode = lineItem.ItmCommodityCode;
                        lineItemDomainEntity.FixedAssetsFlag = lineItem.ItmFixedAssetsFlag;
                        lineItemDomainEntity.TradeDiscountAmount = lineItem.ItmReqTradeDiscAmt;
                        lineItemDomainEntity.TradeDiscountPercentage = lineItem.ItmReqTradeDiscPct;
                        lineItemDomainEntity.FixedAssetsFlag = lineItem.ItmFixedAssetsFlag;

                        // Populate the GL distribution domain entities and add them to the line items
                        if ((lineItem.ItemReqEntityAssociation != null) && (lineItem.ItemReqEntityAssociation.Count > 0))
                        {
                            foreach (var glDist in lineItem.ItemReqEntityAssociation)
                            {
                                // The GL Distribution always uses the local currency amount.
                                decimal gldistGlQty = glDist.ItmReqGlQtyAssocMember.HasValue ? glDist.ItmReqGlQtyAssocMember.Value : 0;
                                // decimal gldistGlAmount = glDist.ItmReqGlAmtAssocMember.HasValue ? glDist.ItmReqGlAmtAssocMember.Value : 0;
                                decimal gldistGlAmount = 0;

                                if (string.IsNullOrEmpty(requisition.ReqCurrencyCode))
                                {
                                    gldistGlAmount = glDist.ItmReqGlAmtAssocMember.HasValue ? glDist.ItmReqGlAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    gldistGlAmount = glDist.ItmReqGlForeignAmtAssocMember.HasValue ? glDist.ItmReqGlForeignAmtAssocMember.Value : 0;
                                }

                                decimal gldistGlPercent = glDist.ItmReqGlPctAssocMember.HasValue ? glDist.ItmReqGlPctAssocMember.Value : 0;
                                LineItemGlDistribution glDistribution = new LineItemGlDistribution(glDist.ItmReqGlNoAssocMember, gldistGlQty, gldistGlAmount, gldistGlPercent);

                                if (!(string.IsNullOrEmpty(glDist.ItmReqProjectCfIdAssocMember)))
                                {
                                    glDistribution.ProjectId = glDist.ItmReqProjectCfIdAssocMember;
                                    if (!itemProjectIds.Contains(glDist.ItmReqProjectCfIdAssocMember))
                                    {
                                        itemProjectIds.Add(glDist.ItmReqProjectCfIdAssocMember);
                                    }
                                }

                                if (!(string.IsNullOrEmpty(glDist.ItmReqPrjItemIdsAssocMember)))
                                {
                                    glDistribution.ProjectLineItemId = glDist.ItmReqPrjItemIdsAssocMember;
                                    if (!itemProjectLineIds.Contains(glDist.ItmReqPrjItemIdsAssocMember))
                                    {
                                        itemProjectLineIds.Add(glDist.ItmReqPrjItemIdsAssocMember);
                                    }
                                }

                                lineItemDomainEntity.AddGlDistribution(glDistribution);

                                // Check the currency code to see if we need the local or foreign amount
                                if (string.IsNullOrEmpty(requisition.ReqCurrencyCode))
                                {
                                    requisitionDomainEntity.Amount += glDist.ItmReqGlAmtAssocMember.HasValue ? glDist.ItmReqGlAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    requisitionDomainEntity.Amount += glDist.ItmReqGlForeignAmtAssocMember.HasValue ? glDist.ItmReqGlForeignAmtAssocMember.Value : 0;
                                }
                            }
                        }

                        // Add taxes to the line item
                        if ((lineItem.ReqGlTaxesEntityAssociation != null) && (lineItem.ReqGlTaxesEntityAssociation.Count > 0))
                        {
                            foreach (var taxGlDist in lineItem.ReqGlTaxesEntityAssociation)
                            {
                                decimal itemTaxAmount = 0;
                                var lineItemTaxCode = taxGlDist.ItmReqGlTaxCodeAssocMember;

                                if (taxGlDist.ItmReqGlForeignTaxAmtAssocMember.HasValue)
                                {
                                    itemTaxAmount = taxGlDist.ItmReqGlForeignTaxAmtAssocMember.HasValue ? taxGlDist.ItmReqGlForeignTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    itemTaxAmount = taxGlDist.ItmReqGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmReqGlTaxAmtAssocMember.Value : 0;
                                }

                                if (!string.IsNullOrEmpty(taxGlDist.ItmReqGlTaxCodeAssocMember))
                                {
                                    var itemTax = new LineItemTax(taxGlDist.ItmReqGlTaxCodeAssocMember,
                                        itemTaxAmount)
                                    {
                                        TaxGlNumber = taxGlDist.ItmReqTaxGlNoAssocMember,
                                        LineGlNumber = taxGlDist.ItmReqLineGlNoAssocMember
                                    };
                                    //lineItemDomainEntity.AddTax(itemTax);
                                    lineItemDomainEntity.AddTaxByGL(itemTax);
                                }
                                if (string.IsNullOrEmpty(requisition.ReqCurrencyCode))
                                {
                                    requisitionDomainEntity.Amount += taxGlDist.ItmReqGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmReqGlTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    requisitionDomainEntity.Amount += taxGlDist.ItmReqGlForeignTaxAmtAssocMember.HasValue ? taxGlDist.ItmReqGlForeignTaxAmtAssocMember.Value : 0;
                                }
                            }
                        }
                        requisitionDomainEntity.AddLineItem(lineItemDomainEntity);

                    }

                    // If there are project IDs, we need to get the project number,
                    // and also the project line item code for each project line item ID 
                    if ((itemProjectIds != null) && (itemProjectIds.Count > 0))
                    {
                        // For each project ID, get the project number
                        var projectRecords = await DataReader.BulkReadRecordAsync<Projects>(itemProjectIds.ToArray());

                        // If there are project IDs, there should be project line item IDs
                        if ((itemProjectLineIds != null) && (itemProjectLineIds.Count > 0))
                        {
                            // For each project line item ID, get the project line item code
                            var projectLineItemRecords = await DataReader.BulkReadRecordAsync<ProjectsLineItems>(itemProjectLineIds.ToArray());

                            if ((projectRecords != null) && (projectRecords.Count > 0))
                            {
                                for (int i = 0; i < requisitionDomainEntity.LineItems.Count(); i++)
                                {
                                    foreach (var glDist in requisitionDomainEntity.LineItems[i].GlDistributions)
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
        }

        /// <summary>
        ///  Build collection of requisition domain entities from a collection of requisition data contracts
        /// </summary>
        /// <param name="requisitionDataContracts">Collection of requisition data contracts</param>
        /// <returns>Collection of requisition domain entity</returns>
        private async Task<IEnumerable<Requisition>> BuildRequisitionsAsync(IEnumerable<DataContracts.Requisitions> requisitionDataContracts)
        {
            var requisitionCollection = new List<Requisition>();

            if (requisitionDataContracts != null && requisitionDataContracts.Any())
            {
                foreach (var requisition in requisitionDataContracts)
                {
                    requisitionCollection.Add(await BuildRequisitionAsync(requisition));
                }
            }
            return requisitionCollection.AsEnumerable();
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
        /// Create an UpdateCreateRequisitionRequest from a requisition domain entity
        /// </summary>
        /// <param name="requisitionEntity">Requisition domain entity</param>
        /// <returns> UpdateCreateRequisitionRequest transaction object</returns>
        private UpdateCreateRequisitionRequest BuildRequisitionUpdateRequest(Requisition requisitionEntity)
        {
            var reqId = requisitionEntity.Id;
            if (requisitionEntity.Id == "NEW") { reqId = null; }
            var request = new UpdateCreateRequisitionRequest
            {
                ReqId = reqId,
                Guid = requisitionEntity.Guid
            };

            if (requisitionEntity.Date != null)
            {
                request.RequestedOn = requisitionEntity.Date;
            }

            if (requisitionEntity.MaintenanceDate.HasValue)
                request.TransactionDate = requisitionEntity.MaintenanceDate;

            if (!string.IsNullOrEmpty(requisitionEntity.Number))
                request.RequisitionNumber = requisitionEntity.Number;

            if (requisitionEntity.ReferenceNumbers != null && requisitionEntity.ReferenceNumbers.Any())
            {
                request.ReferenceNumber = requisitionEntity.ReferenceNumbers.FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(requisitionEntity.Type))
                request.Type = requisitionEntity.Type;

            if (requisitionEntity.DeliveryDate != null && requisitionEntity.DeliveryDate.HasValue)
                request.DeliveredBy = requisitionEntity.DeliveryDate;

            if (!string.IsNullOrEmpty(requisitionEntity.CurrencyCode))
                request.Currency = requisitionEntity.CurrencyCode;

            if (!string.IsNullOrEmpty(requisitionEntity.Buyer))
                request.BuyerId = requisitionEntity.Buyer;

            if (!string.IsNullOrEmpty(requisitionEntity.DefaultInitiator))
                request.Initiator = requisitionEntity.DefaultInitiator;

            if (!string.IsNullOrEmpty(requisitionEntity.ShipToCode))
                request.ShipToId = requisitionEntity.ShipToCode;

            request.Status = requisitionEntity.Status.ToString();

            if (!string.IsNullOrEmpty(requisitionEntity.Fob))
                request.FreeOnBoardId = requisitionEntity.Fob;

            if (!string.IsNullOrEmpty(requisitionEntity.AltShippingName))
                request.OverrideDescription = requisitionEntity.AltShippingName;

            if (requisitionEntity.AltShippingAddress != null && requisitionEntity.AltShippingAddress.Any())
                request.OverrideAddressLines = requisitionEntity.AltShippingAddress;

            if (!string.IsNullOrEmpty(requisitionEntity.IntgAltShipCountry))
                request.OverrideCountryCode = requisitionEntity.IntgAltShipCountry;

            if (!string.IsNullOrEmpty(requisitionEntity.AltShippingState))
                request.OverrideRegionCode = requisitionEntity.AltShippingState;

            if (!string.IsNullOrEmpty(requisitionEntity.AltShippingCity))
                request.OverrideCity = requisitionEntity.AltShippingCity;

            if (!string.IsNullOrEmpty(requisitionEntity.AltShippingZip))
                request.OverrideZip = requisitionEntity.AltShippingZip;

            if (!string.IsNullOrEmpty(requisitionEntity.AltShippingPhoneExt))
                request.OverridePhoneExt = requisitionEntity.AltShippingPhoneExt;

            if (!string.IsNullOrEmpty(requisitionEntity.AltShippingPhone))
                request.OverridePhone = requisitionEntity.AltShippingPhone;

            if (!string.IsNullOrEmpty(requisitionEntity.VendorId))
                request.VendorId = requisitionEntity.VendorId;

            if (!string.IsNullOrEmpty(requisitionEntity.VendorAlternativeAddressId))
                request.AlternativeVendorAddressId = requisitionEntity.VendorAlternativeAddressId;


            if (requisitionEntity.MiscName != null && requisitionEntity.MiscName.Any())
                request.ManualName = requisitionEntity.MiscName[0];

            if (requisitionEntity.MiscAddress != null && requisitionEntity.MiscAddress.Any())
                request.ManualAddressLines = requisitionEntity.MiscAddress;


            if (!string.IsNullOrEmpty(requisitionEntity.MiscCountry))
                request.ManualCountry = requisitionEntity.MiscCountry;

            if (!string.IsNullOrEmpty(requisitionEntity.MiscState))
                request.ManualState = requisitionEntity.MiscState;

            if (!string.IsNullOrEmpty(requisitionEntity.MiscCity))
                request.ManualCity = requisitionEntity.MiscCity;

            if (!string.IsNullOrEmpty(requisitionEntity.MiscZip))
                request.ManualZip = requisitionEntity.MiscZip;

            if (!string.IsNullOrEmpty(requisitionEntity.IntgCorpPerIndicator))
                request.ManualType = requisitionEntity.IntgCorpPerIndicator;

            if (!string.IsNullOrEmpty(requisitionEntity.VendorTerms))
                request.PaymentTermsId = requisitionEntity.VendorTerms;

            if (!string.IsNullOrEmpty(requisitionEntity.ApType))
                request.PaymentSourceId = requisitionEntity.ApType;

            if (!string.IsNullOrEmpty(requisitionEntity.IntgSubmittedBy))
                request.SubmittedById = requisitionEntity.IntgSubmittedBy;

            if (!string.IsNullOrEmpty(requisitionEntity.InternalComments))
                request.Comments = requisitionEntity.InternalComments;

            if (!string.IsNullOrEmpty(requisitionEntity.Comments))
                request.PrintedComment = requisitionEntity.Comments;

            request.BypassApprovalFlag = requisitionEntity.bypassApprovals;
            request.PopulateTaxForm = requisitionEntity.bypassTaxForms;

            if (requisitionEntity.LineItems != null && requisitionEntity.LineItems.Any())
            {
                var lineItems = new List<Transactions.ReqLineItems>();

                foreach (var apLineItem in requisitionEntity.LineItems)
                {
                    var lineItem = new Transactions.ReqLineItems()
                    {
                        LineDesc = apLineItem.Description,
                        LineCommodityCodeId = apLineItem.CommodityCode,
                        LineFixedAssetsFlag = apLineItem.FixedAssetsFlag,
                        LinePartNumber = apLineItem.VendorPart,
                        LineDesiredDate = apLineItem.DesiredDate,
                        LineQuantity = apLineItem.Quantity.ToString(),
                        LineUnitOfMeasureId = apLineItem.UnitOfIssue,
                        LineUnitPrice = apLineItem.Price,
                        LineTradeDiscountAmt = apLineItem.TradeDiscountAmount,
                        LineTradeDiscountPct = apLineItem.TradeDiscountPercentage.ToString(),
                        LineComments = apLineItem.Comments
                    };

                    if (!string.IsNullOrWhiteSpace(apLineItem.Id))
                    {
                        lineItem.LineItemId = apLineItem.Id;
                    }
                    else { lineItem.LineItemId = "NEW"; }


                    if (apLineItem.LineItemTaxes != null && apLineItem.LineItemTaxes.Any())
                    {
                        var taxCodes = new List<string>();
                        foreach (var lineItemTaxes in apLineItem.LineItemTaxes)
                        {
                            if (!string.IsNullOrEmpty(lineItemTaxes.TaxCode))
                                taxCodes.Add(lineItemTaxes.TaxCode);
                        }
                        lineItem.LineTaxCodes = string.Join("|", taxCodes);
                    }
                    if (apLineItem.GlDistributions != null && apLineItem.GlDistributions.Any())
                    {
                        var gl = new List<string>();
                        var adAmts = new List<decimal?>();
                        var adPct = new List<decimal?>();
                        var adQty = new List<decimal?>();
                        foreach (var glDistribution in apLineItem.GlDistributions)
                        {
                            gl.Add(glDistribution.GlAccountNumber);
                            adAmts.Add(glDistribution.Amount);
                            adPct.Add(glDistribution.Percent);
                            adQty.Add(glDistribution.Quantity);
                        }
                        lineItem.LineAccountingString = string.Join("|", gl);
                        lineItem.LineGlAmt = string.Join("|", adAmts);
                        lineItem.LineGlPct = string.Join("|", adPct);
                        lineItem.LineGlQuantity = string.Join("|", adQty);
                    }

                    lineItems.Add(lineItem);
                }

                if (lineItems != null && lineItems.Any())
                {
                    request.ReqLineItems = lineItems;
                }

            }
            return request;
        }

        private async Task<Dictionary<string, string>> GetPersonHierarchyNamesDictionaryAsync(System.Collections.ObjectModel.Collection<Requisitions> requisitionData)
        {
            #region Get Hierarchy Names

            // Use a colleague transaction to get all names at once. 
            List<string> personIds = new List<string>();
            List<string> hierarchies = new List<string>();
            List<string> personNames = new List<string>();
            List<string> ioPersonIds = new List<string>();
            List<string> ioHierarchies = new List<string>();

            Dictionary<string, string> hierarchyNameDictionary = new Dictionary<string, string>();

            GetHierarchyNamesForIdsResponse response = null;

            //Get all unique requestor & initiator personIds
            personIds = requisitionData.Where(x => !string.IsNullOrEmpty(x.ReqRequestor)).Select(s => s.ReqRequestor)
                .Union(requisitionData.Where(x => !string.IsNullOrEmpty(x.ReqDefaultInitiator)).Select(s => s.ReqDefaultInitiator)).Distinct().ToList();

            if ((personIds != null) && (personIds.Count > 0))
            {
                hierarchies = Enumerable.Repeat("PREFERRED", personIds.Count).ToList();
                ioPersonIds.AddRange(personIds);
                ioHierarchies.AddRange(hierarchies);
            }

            //Get all unique ReqVendor Ids where ReqMiscName is missing
            var vendorIds = requisitionData.Where(x => !string.IsNullOrEmpty(x.ReqVendor) && !x.ReqMiscName.Any()).Select(s => s.ReqVendor).Distinct().ToList();
            if ((vendorIds != null) && (vendorIds.Count > 0))
            {
                hierarchies = new List<string>();
                hierarchies = Enumerable.Repeat("PO", vendorIds.Count).ToList();
                ioPersonIds.AddRange(vendorIds);
                ioHierarchies.AddRange(hierarchies);
            }

            // Call a colleague transaction to get the person names based on their hierarchies.
            GetHierarchyNamesForIdsRequest request = new GetHierarchyNamesForIdsRequest()
            {
                IoPersonIds = ioPersonIds,
                IoHierarchies = ioHierarchies
            };

            response = await transactionInvoker.ExecuteAsync<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(request);

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
            #endregion
            return hierarchyNameDictionary;
        }

        private RequisitionSummary BuildRequisitionSummary(Requisitions requisitionDataContract, Dictionary<string, PurchaseOrders> poDictionary, Dictionary<string, Bpo> bpoDictionary, string vendorName, string initiatorName, string requestorName, Collection<Opers> opersCollection)
        {
            if (requisitionDataContract == null)
            {
                throw new ArgumentNullException("requisitionDataContract");
            }

            if (string.IsNullOrEmpty(requisitionDataContract.Recordkey))
            {
                throw new ArgumentNullException("id");
            }

            if (!requisitionDataContract.ReqDate.HasValue)
            {
                throw new ApplicationException("Missing date for requisition id: " + requisitionDataContract.Recordkey);
            }

            if (requisitionDataContract.ReqStatusDate == null || !requisitionDataContract.ReqStatusDate.First().HasValue)
            {
                throw new ApplicationException("Missing status date for requisition id: " + requisitionDataContract.Recordkey);
            }

            var requisitionStatusDate = requisitionDataContract.ReqStatusDate.First().Value;
            var requisitionStatus = ConvertRequisitionStatus(requisitionDataContract.ReqStatus, requisitionDataContract.Recordkey);

            var requisitionSummaryEntity = new RequisitionSummary(requisitionDataContract.Recordkey, requisitionDataContract.ReqNo, vendorName, requisitionDataContract.ReqDate.Value.Date)
            {
                Status = requisitionStatus,
                VendorId = requisitionDataContract.ReqVendor,
                InitiatorName = initiatorName,
                RequestorName = requestorName,
                Amount = requisitionDataContract.ReqTotalAmt.HasValue ? requisitionDataContract.ReqTotalAmt.Value : 0,
                ApprovalReturnedIndicator = (!string.IsNullOrEmpty(requisitionDataContract.ReqReturnFlag) && requisitionDataContract.ReqReturnFlag.Equals("Y"))
            };
            // build approvers and add to entity
            if ((requisitionDataContract.ReqAuthEntityAssociation != null) && (requisitionDataContract.ReqAuthEntityAssociation.Any()))
            {
                // Approver object is declared once
                Approver approver;
                foreach (var approval in requisitionDataContract.ReqAuthEntityAssociation)
                {
                    //get opersId for the requisition
                    var oper = opersCollection.FirstOrDefault(x => x.Recordkey == approval.ReqAuthorizationsAssocMember);
                    if (oper != null)
                    {
                        approver = new Approver(oper.Recordkey);
                        approver.SetApprovalName(oper.SysUserName);
                        approver.ApprovalDate = approval.ReqAuthorizationDatesAssocMember.Value;
                        requisitionSummaryEntity.AddApprover(approver);
                    }
                }
            }
            // build next approvers and add to entity
            if ((requisitionDataContract.ReqApprEntityAssociation != null) && (requisitionDataContract.ReqApprEntityAssociation.Any()))
            {
                // Approver object is declared once
                Approver approver;
                foreach (var approval in requisitionDataContract.ReqApprEntityAssociation)
                {
                    //get opersId for the requisition
                    var oper = opersCollection.FirstOrDefault(x => x.Recordkey == approval.ReqNextApprovalIdsAssocMember);
                    if (oper != null)
                    {
                        approver = new Approver(oper.Recordkey);
                        approver.SetApprovalName(oper.SysUserName);
                        requisitionSummaryEntity.AddApprover(approver);
                    }
                }
            }

            // Add any associated purchase orders to the requisition summary domain entity
            if ((requisitionDataContract.ReqPoNo != null) && (requisitionDataContract.ReqPoNo.Any()))
            {
                foreach (var purchaseOrderId in requisitionDataContract.ReqPoNo)
                {
                    if (!string.IsNullOrEmpty(purchaseOrderId))
                    {
                        PurchaseOrders purchaseOrder = null;
                        if (poDictionary.TryGetValue(purchaseOrderId, out purchaseOrder))
                        {
                            var poDate = purchaseOrder.PoDate.HasValue ? purchaseOrder.PoDate.Value.Date : new DateTime().Date;
                            var purchaseOrderSummaryEntity = new PurchaseOrderSummary(purchaseOrder.Recordkey, purchaseOrder.PoNo, vendorName, poDate);
                            requisitionSummaryEntity.AddPurchaseOrder(purchaseOrderSummaryEntity);
                        }
                    }
                }
            }

            // Add any associated blanket purchase orders to the requisition domain entity.
            // Even though ReqBpoNo is a list of string, only one bpo can be associated to a requisition
            if ((requisitionDataContract.ReqBpoNo != null) && (requisitionDataContract.ReqBpoNo.Any()))
            {
                if (requisitionDataContract.ReqBpoNo.Count > 1)
                {
                    throw new ApplicationException("Only one blanket purchase order can be associated with the requisition: " + requisitionDataContract.Recordkey);
                }
                else
                {
                    var blanketPONo = requisitionDataContract.ReqBpoNo.FirstOrDefault();
                    if (!string.IsNullOrEmpty(blanketPONo))
                    {
                        Bpo bpo = null;
                        if (bpoDictionary.TryGetValue(blanketPONo, out bpo))
                        {
                            requisitionSummaryEntity.BlanketPurchaseOrderId = blanketPONo;
                            requisitionSummaryEntity.BlanketPurchaseOrderNumber = bpo.BpoNo;
                        }
                    }

                }
            }

            return requisitionSummaryEntity;
        }

        private async Task<List<string>> ApplyFilterCriteriaAsync(string personId, CfwebDefaults cfWebDefaults)
        {
            List<string> filteredRequisitions = new List<string>();
            //where personId is Initiator OR requestor
            string reqPersonIdQuery = string.Format("WITH REQ.DEFAULT.INITIATOR EQ '{0}' OR WITH REQ.REQUESTOR EQ '{0}' BY.DSND REQ.NO", personId);
            filteredRequisitions = await ExecuteQueryStatementAsync(filteredRequisitions, reqPersonIdQuery);

            if (filteredRequisitions != null && filteredRequisitions.Any())
            {
                string reqStartEndTransDateQuery = string.Empty;
                if (cfWebDefaults != null)
                {
                    //Filter by CfwebReqStartDate, CfwebReqEndDate values configured in CFWP form
                    //when CfwebReqStartDate & CfwebReqEndDate has a value
                    if (cfWebDefaults.CfwebReqStartDate.HasValue && cfWebDefaults.CfwebReqEndDate.HasValue)
                    {
                        var startDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebReqStartDate.Value);
                        var endDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebReqEndDate.Value);
                        reqStartEndTransDateQuery = string.Format("WITH (REQ.MAINT.GL.TRAN.DATE GE '{0}' AND REQ.MAINT.GL.TRAN.DATE LE '{1}') OR WITH (REQ.DATE GE '{0}' AND REQ.DATE LE '{1}') BY.DSND REQ.NO", startDate, endDate);
                    }
                    //when CfwebReqStartDate has value but CfwebReqEndDate is null
                    else if (cfWebDefaults.CfwebReqStartDate.HasValue && !cfWebDefaults.CfwebReqEndDate.HasValue)
                    {
                        var startDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebReqStartDate.Value);
                        reqStartEndTransDateQuery = string.Format("REQ.MAINT.GL.TRAN.DATE GE '{0}' OR WITH REQ.DATE GE '{0}' BY.DSND REQ.NO", startDate);
                    }
                    //when CfwebReqStartDate is null but CfwebReqEndDate has value
                    else if (!cfWebDefaults.CfwebReqStartDate.HasValue && cfWebDefaults.CfwebReqEndDate.HasValue)
                    {
                        var endDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebReqEndDate.Value);
                        reqStartEndTransDateQuery = string.Format("WITH ((REQ.MAINT.GL.TRAN.DATE NE '') AND (REQ.MAINT.GL.TRAN.DATE LE '{0}')) OR WITH ((REQ.DATE NE '') AND (REQ.DATE LE '{0}')) BY.DSND REQ.NO", endDate);
                    }

                    if (!string.IsNullOrEmpty(reqStartEndTransDateQuery))
                    {
                        filteredRequisitions = await ExecuteQueryStatementAsync(filteredRequisitions, reqStartEndTransDateQuery);
                    }

                    //query by CfwebReqStatuses if statuses are configured in CFWP form.
                    if (cfWebDefaults.CfwebReqStatuses != null && cfWebDefaults.CfwebReqStatuses.Any() && cfWebDefaults.CfwebReqStatuses != null)
                    {
                        var reqStatusesCriteria = string.Join(" ", cfWebDefaults.CfwebReqStatuses.Select(x => string.Format("'{0}'", x.ToUpper())));
                        reqStatusesCriteria = "WITH REQ.CURRENT.STATUS EQ " + reqStatusesCriteria;
                        filteredRequisitions = await ExecuteQueryStatementAsync(filteredRequisitions, reqStatusesCriteria);
                    }
                }
            }
            return filteredRequisitions;
        }

        private async Task<string> BuildFilterCriteria(ProcurementDocumentFilterCriteria criteria, CfwebDefaults cfWebDefaults)
        {
            bool skipCFWPDateRangeFilter = false;
            bool skipCFWPStatusFilter = false;
            StringBuilder queryCriteria = new StringBuilder();
            //personID criteria
            queryCriteria.Append(string.Format("WITH REQ.DEFAULT.INITIATOR EQ '{0}' OR WITH REQ.REQUESTOR EQ '{0}' ", criteria.PersonId));

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
                    var status = ConvertRequisitionStatus(item);
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
                    //criteria set in cfwebdefaults - CfwebReqStartDate, CfwebReqEndDate
                    var cfwpDateRangeCriteria = await BuildDateRangeQueryAsync(cfWebDefaults.CfwebReqStartDate, cfWebDefaults.CfwebReqEndDate);
                    if (!string.IsNullOrEmpty(cfwpDateRangeCriteria))
                    {
                        logger.Debug(string.Format("QueryRequisitionSummaries - CFWP date range - data reader - query string: '{0}'.", cfwpDateRangeCriteria));
                        queryCriteria.Append(cfwpDateRangeCriteria);
                    }
                }
                if (!skipCFWPStatusFilter)
                {
                    //criteria set in cfwebdefaults - CfwebReqStatuses (REQ.CURRENT.STATUS).
                    var statusesQuery = ProcurementFilterUtility.BuildListQuery(cfWebDefaults.CfwebReqStatuses, "REQ.CURRENT.STATUS");
                    if (!string.IsNullOrEmpty(statusesQuery))
                    {
                        logger.Debug(string.Format("QueryRequisitionSummaries - CFWP statuses - data reader - query string: '{0}'.", statusesQuery));
                        queryCriteria.Append(statusesQuery);
                    }
                }
            }

            //criteria sent from SS - VendorID's (REQ.VENDOR).
            var vendorIdCriteria = ProcurementFilterUtility.BuildListQuery(criteria.VendorIds, "REQ.VENDOR");
            if (!string.IsNullOrEmpty(vendorIdCriteria))
            {
                logger.Debug(string.Format("QueryRequisitionSummaries - VendorId's - data reader - query string: '{0}'.", vendorIdCriteria));
                queryCriteria.Append(!string.IsNullOrEmpty(vendorIdCriteria) ? vendorIdCriteria : string.Empty);
            }
            //criteria sent from SS - Min - Max Amount (REQ.TOTAL.AMT).
            var amountCriteria = ProcurementFilterUtility.BuildAmountRangeQuery(criteria, "REQ.TOTAL.AMT");
            if (!string.IsNullOrEmpty(amountCriteria))
            {
                logger.Debug(string.Format("QueryRequisitionSummaries - Amount range - data reader - query string: '{0}'.", amountCriteria));
                queryCriteria.Append(amountCriteria);
            }
            //criteria sent from SS - From - To date range
            if (skipCFWPDateRangeFilter)
            {
                var procurementDateRangeCriteria = await BuildDateRangeQueryAsync(criteria.DateFrom, criteria.DateTo);
                if (!string.IsNullOrEmpty(procurementDateRangeCriteria))
                {
                    logger.Debug(string.Format("QueryRequisitionSummaries - SS procurement filter date range - data reader - query string: '{0}'.", procurementDateRangeCriteria));
                    queryCriteria.Append(procurementDateRangeCriteria);
                }
            }


            if (skipCFWPStatusFilter)
            {
                //criteria sent from SS - Statuses
                var procurementStatusesCriteria = ProcurementFilterUtility.BuildListQuery(procurementFilterStatuses, "REQ.CURRENT.STATUS");
                if (!string.IsNullOrEmpty(procurementStatusesCriteria))
                {
                    logger.Debug(string.Format("QueryRequisitionSummaries - SS procurement filter status - data reader - query string: '{0}'.", procurementStatusesCriteria));
                    queryCriteria.Append(procurementStatusesCriteria);
                }
            }

            queryCriteria.Append("BY.DSND REQ.NO");
            logger.Debug(string.Format("QueryRequisitionSummaries - data reader - query string: '{0}'.", queryCriteria));
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
                    startEndTransDateQuery = string.Format("AND WITH (REQ.MAINT.GL.TRAN.DATE GE '{0}' AND REQ.MAINT.GL.TRAN.DATE LE '{1}') OR (REQ.DATE GE '{0}' AND REQ.DATE LE '{1}') ", startDate, endDate);
                    logger.Debug(string.Format("date range From - {0}, To - {1}.", dateFrom.Value, dateTo.Value));
                }
                //when dateFrom has value but dateTo is null
                else if (dateFrom.HasValue && !dateTo.HasValue)
                {
                    var startDate = await GetUnidataFormatDateAsync(dateFrom.Value);
                    startEndTransDateQuery = string.Format("AND WITH (REQ.MAINT.GL.TRAN.DATE GE '{0}' OR REQ.DATE GE '{0}') ", startDate);
                    logger.Debug(string.Format("date range From - {0}.", dateFrom.Value));
                }
                //when dateFrom is null but dateTo has value
                else if (!dateFrom.HasValue && dateTo.HasValue)
                {
                    var endDate = await GetUnidataFormatDateAsync(dateTo.Value);
                    startEndTransDateQuery = string.Format("AND WITH ((REQ.MAINT.GL.TRAN.DATE NE '') AND (REQ.MAINT.GL.TRAN.DATE LE '{0}')) OR ((REQ.DATE NE '') AND (REQ.DATE LE '{0}')) ", endDate);
                    logger.Debug(string.Format("date range To - {0}.", dateTo.Value));
                }
            }

            return startEndTransDateQuery;
        }

        private async Task<List<string>> ExecuteQueryStatementAsync(List<string> filteredRequisitions, string queryCriteria)
        {
            if (string.IsNullOrEmpty(queryCriteria))
                return null;
            string[] filteredByQueryCriteria;
            if (filteredRequisitions != null && filteredRequisitions.Any())
            {
                filteredByQueryCriteria = await DataReader.SelectAsync("REQUISITIONS", filteredRequisitions.ToArray(), queryCriteria);
            }
            else
            {
                filteredByQueryCriteria = await DataReader.SelectAsync("REQUISITIONS", queryCriteria);
            }
            return filteredByQueryCriteria.ToList();
        }

        private TxCreateWebRequisitionRequest BuildRequisitionCreateRequest(RequisitionCreateUpdateRequest createUpdateRequest)
        {
            var request = new TxCreateWebRequisitionRequest();
            var personId = createUpdateRequest.PersonId;
            var initiatorInitials = createUpdateRequest.InitiatorInitials;
            var confirmationEmailAddresses = createUpdateRequest.ConfEmailAddresses;
            var requisitionEntity = createUpdateRequest.Requisition;
            bool isPersonVendor = createUpdateRequest.IsPersonVendor;
            if (!string.IsNullOrEmpty(personId))
            {
                request.APersonId = personId;
            }
            if (requisitionEntity.Date != null)
            {
                request.AReqDate = DateTime.SpecifyKind(requisitionEntity.Date, DateTimeKind.Unspecified);
            }
            if (requisitionEntity.DesiredDate != null)
            {
                request.AReqDesiredDate = requisitionEntity.DesiredDate;
            }
            if (!string.IsNullOrEmpty(initiatorInitials))
            {
                request.AReqInitiatorInitials = initiatorInitials;
            }
            if (confirmationEmailAddresses != null && confirmationEmailAddresses.Any())
            {
                request.AlConfEmailAddresses = confirmationEmailAddresses;
            }
            if (!string.IsNullOrEmpty(requisitionEntity.ShipToCode))
            {
                request.AReqShipToAddress = requisitionEntity.ShipToCode;
            }

            if (!string.IsNullOrEmpty(requisitionEntity.ApType))
            {
                request.AApType = requisitionEntity.ApType;
            }

            request.AlPrintedComments = CommentsUtility.ConvertMultiLineTextToList(requisitionEntity.Comments);
            request.AlInternalComments = new List<string>() { requisitionEntity.InternalComments };

            if (requisitionEntity.Approvers != null && requisitionEntity.Approvers.Any())
            {
                request.AlNextApprovers = new List<string>();
                foreach (var nextApprover in requisitionEntity.Approvers)
                {
                    request.AlNextApprovers.Add(nextApprover.ApproverId);
                }
            }

            // TaxCodes - In create requisition, take taxcodes from first lineitem and assign to Tax code parameter 
            if (requisitionEntity.LineItems != null && requisitionEntity.LineItems.Any())
            {
                //Check if first lineitem has taxcode, as in create request it is auto applied to all lineitems
                var firstLineItem = requisitionEntity.LineItems.FirstOrDefault();
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
            if (requisitionEntity.LineItems != null && requisitionEntity.LineItems.Any())
            {
                var lineItems = new List<Transactions.AlReqLineItems>();

                foreach (var apLineItem in requisitionEntity.LineItems)
                {
                    var descriptionList = CommentsUtility.ConvertMultiLineTextToList(apLineItem.Description);
                    var lineItem = new Transactions.AlReqLineItems()
                    {
                        AlLineItemDescs = string.Join(_SM.ToString(), descriptionList),
                        AlLineItemQtys = apLineItem.Quantity.ToString(),
                        AlItemPrices = apLineItem.Price.ToString(),
                        AlUnitOfIssues = apLineItem.UnitOfIssue,
                        AlVendorItems = apLineItem.VendorPart
                    };
                    var glAccts = new List<string>();
                    var glDistributionAmounts = new List<decimal?>();
                    var glDistributionQuantities = new List<decimal?>();
                    var glDistributionPercents = new List<decimal?>();
                    var projectNos = new List<string>();
                    foreach (var item in apLineItem.GlDistributions)
                    {
                        glAccts.Add(!string.IsNullOrEmpty(item.GlAccountNumber) ? item.GlAccountNumber : string.Empty);
                        glDistributionAmounts.Add(item.Amount);
                        glDistributionQuantities.Add(item.Quantity);
                        glDistributionPercents.Add(item.Percent);
                        projectNos.Add(!string.IsNullOrEmpty(item.ProjectNumber) ? item.ProjectNumber : string.Empty);
                    }
                    lineItem.AlGlAccts = string.Join("|", glAccts);
                    lineItem.AlGlAcctAmts = string.Join("|", glDistributionAmounts);
                    lineItem.AlGlAcctQtys = string.Join("|", glDistributionQuantities);
                    lineItem.AlGlAcctPcts = string.Join("|", glDistributionPercents);
                    lineItem.AlProjects = string.Join("|", projectNos);
                    lineItems.Add(lineItem);
                }
                request.AlReqLineItems = lineItems;
            }

            if (!string.IsNullOrEmpty(requisitionEntity.VendorId))
            {
                request.AVendorId = requisitionEntity.VendorId;
            }
            else if (!string.IsNullOrEmpty(requisitionEntity.VendorName))
            {
                request.AVendorId = requisitionEntity.VendorName;
            }
            request.AVendorIsPersonFlag = isPersonVendor;
            request.AReqDesiredDate = requisitionEntity.DesiredDate.HasValue ? requisitionEntity.DesiredDate.Value : (DateTime?)null;

            return request;
        }

        private TxUpdateWebRequisitionRequest BuildRequisitionUpdateRequest(RequisitionCreateUpdateRequest createUpdateRequest, Requisition originalRequisition)
        {
            var request = new TxUpdateWebRequisitionRequest();
            var personId = createUpdateRequest.PersonId;
            var confirmationEmailAddresses = createUpdateRequest.ConfEmailAddresses;
            var requisitionEntity = createUpdateRequest.Requisition;
            bool isPersonVendor = createUpdateRequest.IsPersonVendor;

            if (!string.IsNullOrEmpty(createUpdateRequest.PersonId))
            {
                request.APersonId = personId;
            }

            if (confirmationEmailAddresses != null && confirmationEmailAddresses.Any())
            {
                request.AlConfEmailAddresses = confirmationEmailAddresses;
            }

            if (!string.IsNullOrEmpty(requisitionEntity.Id) && !requisitionEntity.Id.Equals("NEW"))
            {
                request.ARequisitionId = requisitionEntity.Id;
            }
            if (!string.IsNullOrEmpty(requisitionEntity.ShipToCode))
            {
                request.AShipTo = requisitionEntity.ShipToCode;
            }

            if (!string.IsNullOrEmpty(requisitionEntity.ApType))
            {
                request.AApType = requisitionEntity.ApType;
            }

            request.AlPrintedComments = CommentsUtility.ConvertMultiLineTextToList(requisitionEntity.Comments);
            request.AlInternalComments = new List<string>() { requisitionEntity.InternalComments };

            var lineItems = new List<Transactions.AlUpdatedReqLineItems>();

            if (requisitionEntity.LineItems != null && requisitionEntity.LineItems.Any())
            {
                foreach (var apLineItem in requisitionEntity.LineItems)
                {
                    var lineItem = new Transactions.AlUpdatedReqLineItems();

                    bool addLineItemToModify = true;
                    if (string.IsNullOrEmpty(apLineItem.Id) || apLineItem.Id.Equals("NEW"))
                    {
                        //New line item added
                        lineItem.AlLineItemIds = "";
                    }
                    else
                    {
                        lineItem.AlLineItemIds = apLineItem.Id;
                        var originalLineItem = originalRequisition.LineItems.FirstOrDefault(x => x.Id == apLineItem.Id);
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

                        if (apLineItem.DesiredDate.HasValue)
                        {
                            lineItem.AlItemDesiredDate = DateTime.SpecifyKind(apLineItem.DesiredDate.Value.Date, DateTimeKind.Unspecified);
                        }
                        else
                        {
                            lineItem.AlItemDesiredDate = null;
                        }
                        lineItem.AlItemTaxForm = !string.IsNullOrEmpty(apLineItem.TaxForm) ? apLineItem.TaxForm : "";
                        lineItem.AlItemTaxFormCode = !string.IsNullOrEmpty(apLineItem.TaxFormCode) ? apLineItem.TaxFormCode : "";
                        lineItem.AlItemTaxFormLoc = !string.IsNullOrEmpty(apLineItem.TaxFormLocation) ? apLineItem.TaxFormLocation : ""; ;

                        var taxCodes = new List<string>();
                        foreach (var taxCode in apLineItem.ReqLineItemTaxCodes)
                        {
                            if (!string.IsNullOrEmpty(taxCode.TaxReqTaxCode))
                                taxCodes.Add(taxCode.TaxReqTaxCode);
                        }
                        lineItem.AlItemTaxCodes = string.Join("|", taxCodes);

                        var glAccts = new List<string>();
                        var glDistributionAmounts = new List<decimal?>();
                        var glDistributionQuantities = new List<decimal?>();
                        var glDistributionPercents = new List<decimal?>();
                        var projectNos = new List<string>();
                        foreach (var item in apLineItem.GlDistributions)
                        {
                            glAccts.Add(!string.IsNullOrEmpty(item.GlAccountNumber) ? item.GlAccountNumber : string.Empty);
                            glDistributionAmounts.Add(item.Amount);
                            glDistributionQuantities.Add(item.Quantity);
                            glDistributionPercents.Add(item.Percent);
                            projectNos.Add(!string.IsNullOrEmpty(item.ProjectNumber) ? item.ProjectNumber : string.Empty);
                        }
                        lineItem.AlItemGlAccts = string.Join("|", glAccts);
                        lineItem.AlItemGlAcctAmts = string.Join("|", glDistributionAmounts);
                        lineItem.AlItemGlAcctQtys = string.Join("|", glDistributionQuantities);
                        lineItem.AlItemGlAcctPcts = string.Join("|", glDistributionPercents);
                        lineItem.AlItemProjectNos = string.Join("|", projectNos);
                        lineItems.Add(lineItem);
                    }
                }
            }
            if (originalRequisition.LineItems != null && originalRequisition.LineItems.Any())
            {
                var deletedLineItems = originalRequisition.LineItems.Where(x => !requisitionEntity.LineItems.Any(u => !string.IsNullOrEmpty(u.Id) && u.Id == x.Id));
                if (deletedLineItems != null && deletedLineItems.Any())
                {
                    foreach (var deletedItem in deletedLineItems)
                    {
                        var deletedLineItem = new Transactions.AlUpdatedReqLineItems();
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
            request.AlUpdatedReqLineItems = lineItems;


            if (!string.IsNullOrEmpty(requisitionEntity.VendorId))
            {
                request.AVendor = requisitionEntity.VendorId;
            }
            else if (!string.IsNullOrEmpty(requisitionEntity.VendorName))
            {
                request.AVendor = requisitionEntity.VendorName;
            }
            request.AVendorIsPersonFlag = isPersonVendor;

            if (requisitionEntity.Approvers != null && requisitionEntity.Approvers.Any())
            {
                request.AlNextApprovers = new List<string>();
                foreach (var nextApprover in requisitionEntity.Approvers)
                {
                    request.AlNextApprovers.Add(nextApprover.ApproverId);
                }
            }

            return request;
        }

        /// <summary>
        /// Determine whether gl access check can be by passed, based on two conditions Requisition status should be "In-Progress" & person id should be either requestor or initiator
        /// </summary>
        /// <param name="personId">PersonId</param>
        /// <param name="requisition">Requisitions data contract</param>
        /// <param name="requisitionDomainEntity">Requisition domain entity</param>
        /// <returns></returns>
        private static bool CanUserByPassGlAccessCheck(string personId, Requisitions requisition, Requisition requisitionDomainEntity)
        {
            return requisitionDomainEntity.Status == RequisitionStatus.InProgress && (requisition.ReqRequestor == personId || requisition.ReqDefaultInitiator == personId);
        }

        private async Task<Dictionary<string, PurchaseOrders>> BuildPurchaseOrderDictionaryAsync(System.Collections.ObjectModel.Collection<Requisitions> requisitionData)
        {
            Dictionary<string, PurchaseOrders> poDictionary = new Dictionary<string, PurchaseOrders>();
            //fetch purchase order no's from all requisitions
            var reqPoNos = requisitionData.Where(x => x.ReqPoNo != null && x.ReqPoNo.Any()).Select(s => s.ReqPoNo).ToList();
            if (reqPoNos != null && reqPoNos.Any())
            {
                List<string> purchaseOrderIds = reqPoNos.SelectMany(x => x).Distinct().ToList();
                //fetch purchase order details and build dictionary
                var purchaseOrders = await DataReader.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", purchaseOrderIds.ToArray());


                if (purchaseOrders != null && purchaseOrders.Any())
                    poDictionary = purchaseOrders.ToDictionary(x => x.Recordkey);
            }

            return poDictionary;
        }

        private async Task<Dictionary<string, Bpo>> BuildBlanketPODictionaryAsync(System.Collections.ObjectModel.Collection<Requisitions> requisitionData)
        {
            Dictionary<string, Bpo> bpoDictionary = new Dictionary<string, Bpo>();
            //fetch blanket purchase order no's from all requisitions
            var reqBpoNos = requisitionData.Where(x => x.ReqBpoNo != null && x.ReqBpoNo.Any()).Select(s => s.ReqBpoNo).ToList();
            if (reqBpoNos != null && reqBpoNos.Any())
            {
                List<string> blanketPurchaseOrderIds = reqBpoNos.SelectMany(x => x).Distinct().ToList();
                //fetch blanket purchase order details and build dictionary
                var Bpos = await DataReader.BulkReadRecordAsync<DataContracts.Bpo>("BPO", blanketPurchaseOrderIds.ToArray());


                if (Bpos != null && Bpos.Any())
                    bpoDictionary = Bpos.ToDictionary(x => x.Recordkey);
            }

            return bpoDictionary;
        }

        private async Task<IEnumerable<RequisitionSummary>> BuildRequisitionSummaryList(string personId, List<string> filteredRequisitionIds)
        {
            var requisitionList = new List<RequisitionSummary>();

            if (!filteredRequisitionIds.Any())
                return requisitionList;

            var requisitionData = await DataReader.BulkReadRecordAsync<DataContracts.Requisitions>("REQUISITIONS", filteredRequisitionIds.ToArray());

            if (requisitionData != null && requisitionData.Any())
            {
                Dictionary<string, string> hierarchyNameDictionary = await GetPersonHierarchyNamesDictionaryAsync(requisitionData);
                Dictionary<string, PurchaseOrders> poDictionary = new Dictionary<string, PurchaseOrders>();
                Dictionary<string, Bpo> bpoDictionary = new Dictionary<string, Bpo>();

                poDictionary = await BuildPurchaseOrderDictionaryAsync(requisitionData);
                bpoDictionary = await BuildBlanketPODictionaryAsync(requisitionData);

                // Read the OPERS records associated with the approval signatures and 
                // next approvers on the requisiton, and build approver objects.
                var operators = new List<string>();
                Collection<Opers> opersCollection = new Collection<Opers>();
                // get list of Approvers and next approvers from the entire requisition records
                var allRequisitionDataApprovers = requisitionData.SelectMany(requisitionContract => requisitionContract.ReqAuthorizations).Distinct().ToList();
                if (allRequisitionDataApprovers != null && allRequisitionDataApprovers.Any(x => x != null))
                {
                    operators.AddRange(allRequisitionDataApprovers);
                }

                var allRequisitionDataNextApprovers = requisitionData.SelectMany(requisitionContract => requisitionContract.ReqNextApprovalIds).Distinct().ToList();
                if (allRequisitionDataNextApprovers != null && allRequisitionDataNextApprovers.Any(x => x != null))
                {
                    operators.AddRange(allRequisitionDataNextApprovers);
                }
                var uniqueOperators = operators.Distinct().ToList();
                if (uniqueOperators.Count > 0)
                {
                    opersCollection = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                }

                foreach (var requisition in requisitionData)
                {
                    try
                    {
                        //filter out any requisitions with Requisition Number whose values are empty/ whitespace
                        if (string.IsNullOrWhiteSpace(requisition.ReqNo))
                        {
                            logger.Debug(string.Format("The Requisition with the ID \"{0}\" has Requisition Number \"{1}\"; skipped this record for Requisition View on Procurement page for person {2}.", requisition.Recordkey, requisition.ReqNo, personId));
                        }
                        else
                        {
                            string initiatorName = string.Empty;
                            if (!string.IsNullOrEmpty(requisition.ReqDefaultInitiator))
                                hierarchyNameDictionary.TryGetValue(requisition.ReqDefaultInitiator, out initiatorName);

                            string requestorName = string.Empty;
                            if (!string.IsNullOrEmpty(requisition.ReqRequestor))
                                hierarchyNameDictionary.TryGetValue(requisition.ReqRequestor, out requestorName);

                            // If there is no vendor name and there is a vendor id, use the PO hierarchy to get the vendor name.
                            var requisitionVendorName = requisition.ReqMiscName != null && requisition.ReqMiscName.Any() ? requisition.ReqMiscName.FirstOrDefault() : string.Empty;
                            if ((string.IsNullOrEmpty(requisitionVendorName)) && (!string.IsNullOrEmpty(requisition.ReqVendor)))
                            {
                                hierarchyNameDictionary.TryGetValue(requisition.ReqVendor, out requisitionVendorName);
                            }
                            requisitionList.Add(BuildRequisitionSummary(requisition, poDictionary, bpoDictionary, requisitionVendorName, initiatorName, requestorName, opersCollection));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }

            return requisitionList;
        }

        private static string ConvertRequisitionStatus(string status)
        {
            string requisitionStatus = string.Empty;
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToUpper())
                {
                    case "INPROGRESS":
                        requisitionStatus = "U";
                        break;
                    case "NOTAPPROVED":
                        requisitionStatus = "N";
                        break;
                    case "OUTSTANDING":
                        requisitionStatus = "O";
                        break;
                    case "POCREATED":
                        requisitionStatus = "P";
                        break;
                }
            }
            return requisitionStatus;
        }

        #region Get VendorAddress
        private async Task GetVendorAddress(string vendorID, Requisition requisitionDomainEntity)
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
                        requisitionDomainEntity.VendorAddress = searchResponse.VendorSearchResults.FirstOrDefault().AlVendorAddresses;
                        requisitionDomainEntity.VendorAddressTypeCode = searchResponse.VendorSearchResults.FirstOrDefault().AlVendAddrTypeCodes;
                        requisitionDomainEntity.VendorAddressTypeDesc = searchResponse.VendorSearchResults.FirstOrDefault().AlVendAddrTypeDesc;
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
