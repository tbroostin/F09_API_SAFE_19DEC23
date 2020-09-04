// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IVoucherRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class VoucherRepository : BaseColleagueRepository, IVoucherRepository
    {
        public static char _SM = Convert.ToChar(DynamicArray.SM);
        /// <summary>
        /// This constructor allows us to instantiate a voucher repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public VoucherRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get a specific voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher ID.</param>
        /// <param name="personId">Person ID of user.</param>
        /// <param name="glAccessLevel">GL Access level of user.</param>
        /// <param name="glAccessAccounts">Set of GL Accounts to which the user has access.</param>
        /// <param name="versionNumber">Voucher API version number.</param>
        /// <returns>Voucher domain entity.</returns>
        public async Task<Voucher> GetVoucherAsync(string voucherId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> glAccessAccounts, int versionNumber)
        {
            if (string.IsNullOrEmpty(voucherId))
            {
                throw new ArgumentNullException("voucherId");
            }

            if (glAccessAccounts == null)
            {
                glAccessAccounts = new List<string>();
            }

            var voucher = await DataReader.ReadRecordAsync<Vouchers>(voucherId);
            if (voucher == null)
            {
                throw new KeyNotFoundException(string.Format("Voucher record {0} does not exist.", voucherId));
            }

            // Translate the status code into a VoucherStatus enumeration value
            VoucherStatus voucherStatus = new VoucherStatus();

            // Get the first status in the list of voucher statuses, and check that it has a value
            if (voucher.VouStatus != null && !string.IsNullOrEmpty(voucher.VouStatus.FirstOrDefault()))
            {
                switch (voucher.VouStatus.FirstOrDefault().ToUpper())
                {
                    case "U":
                        voucherStatus = VoucherStatus.InProgress;
                        break;
                    case "N":
                        voucherStatus = VoucherStatus.NotApproved;
                        break;
                    case "O":
                        voucherStatus = VoucherStatus.Outstanding;
                        break;
                    case "P":
                        voucherStatus = VoucherStatus.Paid;
                        break;
                    case "R":
                        voucherStatus = VoucherStatus.Reconciled;
                        break;
                    case "V":
                        voucherStatus = VoucherStatus.Voided;
                        break;
                    case "X":
                        voucherStatus = VoucherStatus.Cancelled;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid voucher status for voucher: " + voucher.Recordkey);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for voucher: " + voucher.Recordkey);
            }

            // Determine the vendor name for the voucher. If there is a misc name, use it. Otherwise, get the 
            // AP.CHECK hierarchy name.
            string voucherVendorName = "";
            if ((voucher.VouMiscName != null) && (voucher.VouMiscName.Count() > 0))
            {
                voucherVendorName = String.Join(" ", voucher.VouMiscName.ToArray());
            }
            else if (!string.IsNullOrEmpty(voucher.VouVendor))
            {
                // Call a colleague transaction to get the AP.CHECK hierarchy.
                TxGetHierarchyNameRequest request = new TxGetHierarchyNameRequest()
                {
                    IoPersonId = voucher.VouVendor,
                    InHierarchy = "AP.CHECK"
                };

                TxGetHierarchyNameResponse response = transactionInvoker.Execute<TxGetHierarchyNameRequest, TxGetHierarchyNameResponse>(request);

                // The transaction returns the hierarchy name.
                if (!((response.OutPersonName == null) || (response.OutPersonName.Count < 1)))
                {
                    voucherVendorName = String.Join(" ", response.OutPersonName.ToArray());
                }
            }

            if (!voucher.VouDate.HasValue)
            {
                throw new ApplicationException("Missing voucher date for voucher: " + voucher.Recordkey);
            }

            if ((string.IsNullOrEmpty(voucher.VouApType)) && (voucherStatus != VoucherStatus.Cancelled))
            {
                throw new ApplicationException("Missing AP type for voucher: " + voucher.Recordkey);
            }

            // The first version of the voucher API required the voucher default invoice number and the voucher
            // default invoice date. It did not support AR refund vouchers that do not have this data.
            if (versionNumber < 2)
            {
                if (string.IsNullOrEmpty(voucher.VouDefaultInvoiceNo))
                {
                    throw new ApplicationException("Invoice number is a required field.");
                }
                if (!voucher.VouDefaultInvoiceDate.HasValue)
                {
                    throw new ApplicationException("Invoice date is a required field.");
                }
            }

            var voucherDomainEntity = new Voucher(voucher.Recordkey, voucher.VouDate.Value, voucherStatus, voucherVendorName);
            voucherDomainEntity.InvoiceNumber = voucher.VouDefaultInvoiceNo;
            voucherDomainEntity.InvoiceDate = voucher.VouDefaultInvoiceDate;
            // The voucher status date contains one to many dates
            var voucherStatusDate = (voucher.VouStatusDate!=null && voucher.VouStatusDate.Any())? voucher.VouStatusDate.First(): null;
            if(!voucherStatusDate.HasValue)
            {
                throw new ApplicationException("Voucher status date is a required field.");
            }

            voucherDomainEntity.StatusDate = voucherStatusDate.Value;

            voucherDomainEntity.Amount = 0;
            voucherDomainEntity.CurrencyCode = voucher.VouCurrencyCode;

            voucherDomainEntity.VendorId = voucher.VouVendor;
            if (string.IsNullOrEmpty(voucher.VouAddressId))
            {
                voucherDomainEntity.VendorAddressLines = voucher.VouMiscAddress;
                voucherDomainEntity.VendorCity = voucher.VouMiscCity;
                voucherDomainEntity.VendorState = voucher.VouMiscState;
                voucherDomainEntity.VendorZip = voucher.VouMiscZip;
                voucherDomainEntity.VendorCountry = voucher.VouMiscCountry;

            } else
            {
                if (!string.IsNullOrEmpty(voucher.VouAltFlag) && voucher.VouAltFlag.Equals("Y"))
                {
                    voucherDomainEntity.VendorAddressLines = voucher.VouMiscAddress;
                    voucherDomainEntity.VendorCity = voucher.VouMiscCity;
                    voucherDomainEntity.VendorState = voucher.VouMiscState;
                    voucherDomainEntity.VendorZip = voucher.VouMiscZip;
                    voucherDomainEntity.VendorCountry = voucher.VouMiscCountry;
                } else
                {
                    var addressRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(voucher.VouAddressId);
                    if (addressRecord != null)
                    {
                        voucherDomainEntity.VendorAddressLines = addressRecord.AddressLines;
                        voucherDomainEntity.VendorCity = addressRecord.City;
                        voucherDomainEntity.VendorState = addressRecord.State;
                        voucherDomainEntity.VendorZip = addressRecord.Zip;
                        voucherDomainEntity.VendorCountry = addressRecord.Country;
                    }
                }
                
            }
                            
            if (voucher.VouMaintGlTranDate.HasValue)
            {
                voucherDomainEntity.MaintenanceDate = voucher.VouMaintGlTranDate.Value.Date;
            }
            voucherDomainEntity.ApType = voucher.VouApType;

            if (voucher.VouDueDate.HasValue)
            {
                voucherDomainEntity.DueDate = voucher.VouDueDate;
            }

            // Get just the check number instead of the bank code and check number
            if (!string.IsNullOrEmpty(voucher.VouCheckNo))
            {
                // Parse the check number field, which contains the bank code, an asterisk, and the check number.
                var bankLength = voucher.VouCheckNo.IndexOf('*');
                var checkLength = voucher.VouCheckNo.Length;

                voucherDomainEntity.CheckNumber = voucher.VouCheckNo.Substring(bankLength + 1, checkLength - (bankLength + 1));
            }

            if (voucher.VouCheckDate.HasValue)
            {
                voucherDomainEntity.CheckDate = voucher.VouCheckDate;
            }

            if (!string.IsNullOrEmpty(voucher.VouPoNo))
            {
                voucherDomainEntity.PurchaseOrderId = voucher.VouPoNo;
            }
            if (!string.IsNullOrEmpty(voucher.VouBpoId))
            {
                voucherDomainEntity.BlanketPurchaseOrderId = voucher.VouBpoId;
            }
            if (!string.IsNullOrEmpty(voucher.VouRcvsId))
            {
                voucherDomainEntity.RecurringVoucherId = voucher.VouRcvsId;

                var rcVouSchedule = await DataReader.ReadRecordAsync<RcVouSchedules>(voucher.VouRcvsId);
                voucherDomainEntity.RecurringVoucherId = rcVouSchedule.RcvsRcVoucher;
            }

            voucherDomainEntity.Comments = voucher.VouComments;

            // Read the OPERS records associated with the approval signatures and next 
            // approvers on the voucher, and build approver objects.
            var operators = new List<string>();
            if (voucher.VouAuthorizations != null)
            {
                operators.AddRange(voucher.VouAuthorizations);
            }
            if (voucher.VouNextApprovalIds != null)
            {
                operators.AddRange(voucher.VouNextApprovalIds);
            }
            var uniqueOperators = operators.Distinct().ToList();

            if (uniqueOperators.Count > 0)
            {
                var Approvers = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                if ((Approvers != null) && (Approvers.Count > 0))
                {
                    // loop through the opers, create Approver objects, add the name, and if they
                    // are one of the approvers of the voucher, add the approval date.
                    foreach (var appr in Approvers)
                    {
                        Approver approver = new Approver(appr.Recordkey);
                        var approverName = appr.SysUserName;
                        approver.SetApprovalName(approverName);
                        if ((voucher.VouAuthEntityAssociation != null) && (voucher.VouAuthEntityAssociation.Count > 0))
                        {
                            foreach (var approval in voucher.VouAuthEntityAssociation)
                            {
                                if (approval.VouAuthorizationsAssocMember == appr.Recordkey)
                                {
                                    approver.ApprovalDate = approval.VouAuthorizationDatesAssocMember.Value;
                                }
                            }
                        }
                        voucherDomainEntity.AddApprover(approver);
                    }
                }
            }

            var lineItemIds = voucher.VouItemsId;
            if (lineItemIds != null && lineItemIds.Count() > 0)
            {
                var lineItemRecords = await DataReader.BulkReadRecordAsync<Items>(lineItemIds.ToArray());
                if ((lineItemRecords != null) && (lineItemRecords.Count > 0))
                {
                    // If the user has the full access GL role, they have access to all GL accounts.
                    // There is no need to check for GL account access security. If they have partial 
                    // access, we need to check if the GL accounts on the voucher are in the list of
                    // GL accounts to which the user has access.

                    bool hasGlAccess = false;
                    // Get all of the GL numbers for this voucher
                    var glNumbersAllowed = new List<string>();

                    if (glAccessLevel == GlAccessLevel.Full_Access)
                    {
                        hasGlAccess = true;
                    }
                    // Only evaluate GL security if the GL access level is "Possible".
                    else if (glAccessLevel == GlAccessLevel.Possible_Access)
                    {
                        if (CanUserByPassGlAccessCheck(personId, voucher, voucherDomainEntity))
                        {
                            hasGlAccess = true;
                        }
                        else
                        {
                            // Put together a list of unique GL accounts for all the items
                            foreach (var lineItem in lineItemRecords)
                            {
                                if ((lineItem.VouchGlEntityAssociation != null) && (lineItem.VouchGlEntityAssociation.Count > 0))
                                {
                                    foreach (var glDistribution in lineItem.VouchGlEntityAssociation)
                                    {
                                        if (glAccessAccounts.Contains(glDistribution.ItmVouGlNoAssocMember))
                                        {
                                            hasGlAccess = true;
                                            glNumbersAllowed.Add(glDistribution.ItmVouGlNoAssocMember);
                                        }
                                    }
                                }
                            }
                        }

                    }

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

                        decimal itemQuantity = lineItem.ItmVouQty.HasValue ? lineItem.ItmVouQty.Value : 0;
                        decimal itemPrice = lineItem.ItmVouPrice.HasValue ? lineItem.ItmVouPrice.Value : 0;
                        decimal itemExtendedPrice = lineItem.ItmVouExtPrice.HasValue ? lineItem.ItmVouExtPrice.Value : 0;
                        var lineItemDomainEntity = new LineItem(lineItem.Recordkey, itemDescription, itemQuantity, itemPrice, itemExtendedPrice);
                        lineItemDomainEntity.UnitOfIssue = lineItem.ItmVouIssue;
                        lineItemDomainEntity.VendorPart = lineItem.ItmVendorPart;
                        lineItemDomainEntity.InvoiceNumber = lineItem.ItmInvoiceNo;
                        lineItemDomainEntity.TaxForm = lineItem.ItmTaxForm;
                        lineItemDomainEntity.TaxFormCode = lineItem.ItmTaxFormCode;
                        lineItemDomainEntity.TaxFormLocation = lineItem.ItmTaxFormLoc;
                        lineItemDomainEntity.Comments = lineItem.ItmComments;
                        lineItemDomainEntity.CommodityCode = lineItem.ItmCommodityCode;
                        lineItemDomainEntity.FixedAssetsFlag = lineItem.ItmFixedAssetsFlag;
                        lineItemDomainEntity.TradeDiscountAmount = lineItem.ItmVouTradeDiscAmt;
                        lineItemDomainEntity.TradeDiscountPercentage = lineItem.ItmVouTradeDiscPct;

                        //Populate the Line Item tax codes for Voucher
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
                        if ((lineItem.VouchGlEntityAssociation != null) && (lineItem.VouchGlEntityAssociation.Count > 0))
                        {
                            var distrProjects = new List<string>();
                            var distrProjectLineItems = new List<string>();

                            foreach (var glDistr in lineItem.VouchGlEntityAssociation)
                            {
                                // The GL Distribution always uses the local currency amount.
                                LineItemGlDistribution glDistribution = new LineItemGlDistribution(glDistr.ItmVouGlNoAssocMember,
                                    glDistr.ItmVouGlQtyAssocMember.HasValue ? glDistr.ItmVouGlQtyAssocMember.Value : 0,
                                    glDistr.ItmVouGlAmtAssocMember.HasValue ? glDistr.ItmVouGlAmtAssocMember.Value : 0);

                                if (!(string.IsNullOrEmpty(glDistr.ItmVouProjectCfIdAssocMember)))
                                {
                                    glDistribution.ProjectId = glDistr.ItmVouProjectCfIdAssocMember;
                                    distrProjects.Add(glDistr.ItmVouProjectCfIdAssocMember);
                                }

                                if (!(string.IsNullOrEmpty(glDistr.ItmVouPrjItemIdsAssocMember)))
                                {
                                    distrProjectLineItems.Add(glDistr.ItmVouPrjItemIdsAssocMember);
                                    glDistribution.ProjectLineItemId = glDistr.ItmVouPrjItemIdsAssocMember;
                                }

                                lineItemDomainEntity.AddGlDistribution(glDistribution);

                                if (string.IsNullOrEmpty(voucher.VouCurrencyCode))
                                {
                                    voucherDomainEntity.Amount += glDistr.ItmVouGlAmtAssocMember.HasValue ? glDistr.ItmVouGlAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    voucherDomainEntity.Amount += glDistr.ItmVouGlForeignAmtAssocMember.HasValue ? glDistr.ItmVouGlForeignAmtAssocMember.Value : 0;
                                }
                            }

                            // bulk read the projects and project line items on the GL distributions
                            // and then update the project reference number and the project line item

                            if ((distrProjects != null) && (distrProjects.Count > 0))
                            {
                                var projectRecords = await DataReader.BulkReadRecordAsync<Projects>(distrProjects.ToArray());
                                if ((projectRecords != null) && (projectRecords.Count > 0))
                                {
                                    foreach (var project in projectRecords)
                                    {
                                        foreach (var distribution in lineItemDomainEntity.GlDistributions)
                                        {
                                            if (project.Recordkey == distribution.ProjectId)
                                            {
                                                distribution.ProjectNumber = project.PrjRefNo;
                                            }
                                        }
                                    }
                                }

                                if ((distrProjectLineItems != null) && (distrProjectLineItems.Count > 0))
                                {
                                    var projectLineItemRecords = await DataReader.BulkReadRecordAsync<ProjectsLineItems>(distrProjectLineItems.ToArray());
                                    if ((projectLineItemRecords != null) && (projectLineItemRecords.Count > 0))
                                    {
                                        foreach (var projectItem in projectLineItemRecords)
                                        {
                                            foreach (var distrib in lineItemDomainEntity.GlDistributions)
                                            {
                                                if (projectItem.Recordkey == distrib.ProjectLineItemId)
                                                {
                                                    distrib.ProjectLineItemCode = projectItem.PrjlnProjectItemCode;
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }

                        // Add taxes to the line item
                        if ((lineItem.VouGlTaxesEntityAssociation != null) && (lineItem.VouGlTaxesEntityAssociation.Count > 0))
                        {
                            foreach (var taxGlDistr in lineItem.VouGlTaxesEntityAssociation)
                            {
                                decimal? itemTaxAmount;
                                if (taxGlDistr.ItmVouGlForeignTaxAmtAssocMember.HasValue)
                                {
                                    itemTaxAmount = taxGlDistr.ItmVouGlForeignTaxAmtAssocMember;
                                }
                                else
                                {
                                    itemTaxAmount = taxGlDistr.ItmVouGlTaxAmtAssocMember;
                                }


                                LineItemTax itemTax = new LineItemTax(taxGlDistr.ItmVouGlTaxCodeAssocMember,
                                    itemTaxAmount.HasValue ? itemTaxAmount.Value : 0);

                                lineItemDomainEntity.AddTax(itemTax);

                                if (string.IsNullOrEmpty(voucher.VouCurrencyCode))
                                {
                                    voucherDomainEntity.Amount += taxGlDistr.ItmVouGlTaxAmtAssocMember.HasValue ? taxGlDistr.ItmVouGlTaxAmtAssocMember.Value : 0;
                                }
                                else
                                {
                                    voucherDomainEntity.Amount += taxGlDistr.ItmVouGlForeignTaxAmtAssocMember.HasValue ? taxGlDistr.ItmVouGlForeignTaxAmtAssocMember.Value : 0;
                                }
                            }
                        }

                        if (hasGlAccess == true )
                        {
                            // Now apply GL account access security when creating the line items.
                            // Check to see if the user has access to the GL accounts for each line item:
                            // - if they do not have access to any of them, we will not add the line item to the voucher domain entity.
                            // - if the user has access to some of the GL accounts, the ones they do not have access to will be masked.

                            // Figure out whether or not we should include this line item and if so, which GL numbers to display
                            bool addItem = false;

                            if (glAccessLevel == GlAccessLevel.Full_Access)
                            {
                                // The user has full access and there is no need to check further
                                addItem = true;
                            }
                            else
                            {
                                if (CanUserByPassGlAccessCheck(personId, voucher, voucherDomainEntity))
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
                                        foreach (var glDistribution in lineItemDomainEntity.GlDistributions)
                                        {
                                            // If we have access to the GL number then mark the line item to be included.                                            
                                            if (glNumbersAllowed.Contains(glDistribution.GlAccountNumber))
                                            {
                                                addItem = true;
                                            }
                                            // Otherwise, mark the GL number as masked.
                                            else
                                            {
                                                glDistribution.Masked = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (addItem)
                            {
                                voucherDomainEntity.AddLineItem(lineItemDomainEntity);
                            }
                        }
                    }
                }
            }

            return voucherDomainEntity;
        }

        /// <summary>
        /// Determine whether gl access check can be by passed, based on two conditions Voucher status should be "In-Progress", person id should be either requestor, No Purchase Order should be attached.
        /// </summary>
        /// <param name="personId">PersonId</param>
        /// <param name="voucher">Voucher data contract</param>
        /// <param name="voucherDomainEntity">Voucher domain entity</param>
        /// <returns></returns>
        private static bool CanUserByPassGlAccessCheck(string personId, Vouchers voucher, Voucher voucherDomainEntity)
        {
            if (string.IsNullOrEmpty(voucherDomainEntity.PurchaseOrderId))
                return ((voucherDomainEntity.Status == VoucherStatus.InProgress || voucherDomainEntity.Status == VoucherStatus.Voided) && (voucher.VouRequestor == personId));
            else
                return false;
        }

        /// <summary>
        /// Get a collection of voucher summary domain entity objects
        /// </summary>
        /// <param name="id">Person ID</param>        
        /// <returns>collection of voucher summary domain entity objects</returns>
        public async Task<IEnumerable<VoucherSummary>> GetVoucherSummariesByPersonIdAsync(string personId)
        {
            List<string> filteredVouchers = new List<string>();

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            filteredVouchers = await ApplyFilterCriteriaAsync(personId, filteredVouchers);

            if (!filteredVouchers.Any())
                return null;

            var VoucherData = await DataReader.BulkReadRecordAsync<DataContracts.Vouchers>(filteredVouchers.ToArray());

            var VoucherList = new List<VoucherSummary>();
            if (VoucherData != null && VoucherData.Any())
            {
                Dictionary<string, string> hierarchyNameDictionary = await GetPersonHierarchyNamesDictionaryAsync(VoucherData);
                Dictionary<string, PurchaseOrders> poDictionary = new Dictionary<string, PurchaseOrders>();
                Dictionary<string, Bpo> bpoDictionary = new Dictionary<string, Bpo>();
                //Dictionary<string, RcVouchers> rcvDictionary = new Dictionary<string, RcVouchers>();

                poDictionary = await BuildPurchaseOrderDictionaryAsync(VoucherData);
                bpoDictionary = await BuildBlanketPODictionaryAsync(VoucherData);
                //rcvDictionary = await BuildBlanketRcvDictionaryAsync(VoucherData);

                foreach (var voucher in VoucherData)
                {
                    try
                    {
                        string requestorName = string.Empty;
                        if (!string.IsNullOrEmpty(voucher.VouRequestor))
                            hierarchyNameDictionary.TryGetValue(voucher.VouRequestor, out requestorName);

                        // If there is no vendor name and there is a vendor id, use the hierarchy to get the vendor name.
                        var VoucherVendorName = voucher.VouMiscName != null && voucher.VouMiscName.Any() ? voucher.VouMiscName.FirstOrDefault() : string.Empty;
                        if ((string.IsNullOrEmpty(VoucherVendorName)) && (!string.IsNullOrEmpty(voucher.VouVendor)))
                        {
                            hierarchyNameDictionary.TryGetValue(voucher.VouVendor, out VoucherVendorName);
                        }
                        VoucherList.Add(BuildVoucherSummary(voucher, poDictionary, bpoDictionary, VoucherVendorName, requestorName));
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            return VoucherList.AsEnumerable();

        }
        private async Task<List<string>> ApplyFilterCriteriaAsync(string personId, List<string> filteredVouchers)
        {
            //where personId is requestor
            string reqPersonIdQuery = string.Format("WITH VOU.REQUESTOR EQ '{0}'", personId);
            filteredVouchers = await ExecuteQueryStatementAsync(filteredVouchers, reqPersonIdQuery);
            return filteredVouchers;
        }
        private async Task<Dictionary<string, string>> GetPersonHierarchyNamesDictionaryAsync(System.Collections.ObjectModel.Collection<Vouchers> voucherData)
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
            personIds = voucherData.Where(x => !string.IsNullOrEmpty(x.VouRequestor)).Select(s => s.VouRequestor).Distinct().ToList();

            if ((personIds != null) && (personIds.Count > 0))
            {
                hierarchies = Enumerable.Repeat("PREFERRED", personIds.Count).ToList();
                ioPersonIds.AddRange(personIds);
                ioHierarchies.AddRange(hierarchies);
            }

            //Get all unique ReqVendor Ids where ReqMiscName is missing
            var vendorIds = voucherData.Where(x => !string.IsNullOrEmpty(x.VouVendor) && !x.VouMiscName.Any()).Select(s => s.VouVendor).Distinct().ToList();
            if ((vendorIds != null) && (vendorIds.Count > 0))
            {
                hierarchies = new List<string>();
                hierarchies = Enumerable.Repeat("AP.CHECK", vendorIds.Count).ToList();
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
        private VoucherSummary BuildVoucherSummary(Vouchers voucherDataContract, Dictionary<string, PurchaseOrders> poDictionary, Dictionary<string, Bpo> bpoDictionary, string vendorName,  string requestorName)
        {
            if (voucherDataContract == null)
            {
                throw new ArgumentNullException("voucherDataContract");
            }

            if (string.IsNullOrEmpty(voucherDataContract.Recordkey))
            {
                throw new ArgumentNullException("id");
            }

            if (!voucherDataContract.VouDate.HasValue)
            {
                throw new ApplicationException("Missing date for voucher id: " + voucherDataContract.Recordkey);
            }

            if (voucherDataContract.VouStatusDate == null || !voucherDataContract.VouStatusDate.First().HasValue)
            {
                throw new ApplicationException("Missing status date for voucher id: " + voucherDataContract.Recordkey);
            }

            var voucherDate = voucherDataContract.VouDate.Value;
            if (!voucherDataContract.VouDate.HasValue)
            {
                throw new ApplicationException("Missing date for voucher: " + voucherDataContract.Recordkey);
            }

            DateTime? maintenanceDate = null;
            if (voucherDataContract.VouMaintGlTranDate.HasValue)
            {
                maintenanceDate = voucherDataContract.VouMaintGlTranDate.Value.Date;
            }

            var voucherStatus = ConvertVoucherStatus(voucherDataContract.VouStatus, voucherDataContract.Recordkey);

            var voucherSummaryEntity = new VoucherSummary(voucherDataContract.Recordkey, voucherDataContract.VouDefaultInvoiceNo, vendorName, voucherDate)
            {
                Status = voucherStatus,
                MaintenanceDate = maintenanceDate,
                VendorId = voucherDataContract.VouVendor,
                RequestorName = requestorName,
                Amount = voucherDataContract.VouTotalAmt.HasValue ? voucherDataContract.VouTotalAmt.Value : 0,
                InvoiceDate = voucherDataContract.VouDefaultInvoiceDate.HasValue ? voucherDataContract.VouDefaultInvoiceDate.Value : default(DateTime?),

            };

            // Add any associated purchase orders to the voucher summary domain entity
            if (!string.IsNullOrEmpty(voucherDataContract.VouPoNo ))
            {
                PurchaseOrders purchaseOrder = null;
                if (poDictionary.TryGetValue(voucherDataContract.VouPoNo, out purchaseOrder))
                {
                    var purchaseOrderSummaryEntity = new PurchaseOrderSummary(purchaseOrder.Recordkey, purchaseOrder.PoNo, vendorName, purchaseOrder.PoDate.Value.Date);
                    voucherSummaryEntity.AddPurchaseOrder(purchaseOrderSummaryEntity);
                }
            }

            // Add any associated blanket purchase orders to the voucher domain entity.
            if ((voucherDataContract.VouBpoId != null))
            {
                if (!string.IsNullOrEmpty(voucherDataContract.VouBpoId))
                {
                    Bpo bpo = null;
                    if (bpoDictionary.TryGetValue(voucherDataContract.VouBpoId, out bpo))
                    {
                        voucherSummaryEntity.BlanketPurchaseOrderId = voucherDataContract.VouBpoId;
                        voucherSummaryEntity.BlanketPurchaseOrderNumber = bpo.BpoNo;
                    }
                }
            }
            return voucherSummaryEntity;
        }

        private async Task<List<string>> ExecuteQueryStatementAsync(List<string> filteredVouchers, string queryCriteria)
        {
            string[] filteredByQueryCriteria = null;
            if (string.IsNullOrEmpty(queryCriteria))
                return null;
            if (filteredVouchers != null && filteredVouchers.Any())
            {
                filteredByQueryCriteria = await DataReader.SelectAsync("VOUCHERS", filteredVouchers.ToArray(), queryCriteria);
            }
            else
            {
                filteredByQueryCriteria = await DataReader.SelectAsync("VOUCHERS", queryCriteria);
            }
            return filteredByQueryCriteria.ToList();
        }
        private async Task<Dictionary<string, PurchaseOrders>> BuildPurchaseOrderDictionaryAsync(System.Collections.ObjectModel.Collection<Vouchers> voucherData)
        {
            Dictionary<string, PurchaseOrders> purchaseOrderDictionary = new Dictionary<string, PurchaseOrders>();
            //fetch purchase order no's from all requisitions
            var vouPoNos = voucherData.Where(x => x.VouPoNo != null && x.VouPoNo.Any()).Select(s => s.VouPoNo).ToList();
            if (vouPoNos != null && vouPoNos.Any())
            {
                List<string> purchaseOrderIds = vouPoNos.Select(x => x).Distinct().ToList();
                //fetch purchase order details and build dictionary
                var purchaseOrders = await DataReader.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", purchaseOrderIds.ToArray());

                if (purchaseOrders != null && purchaseOrders.Any())
                    purchaseOrderDictionary = purchaseOrders.ToDictionary(x => x.Recordkey);
            }

            return purchaseOrderDictionary;
        }

        private async Task<Dictionary<string, Bpo>> BuildBlanketPODictionaryAsync(System.Collections.ObjectModel.Collection<Vouchers> voucherData)
        {
            Dictionary<string, Bpo> bpoDictionary = new Dictionary<string, Bpo>();
            //fetch blanket purchase order no's from all requisitions
            var vouBpoIds = voucherData.Where(x => x.VouBpoId != null && x.VouBpoId.Any()).Select(s => s.VouBpoId).ToList();
            if (vouBpoIds != null && vouBpoIds.Any())
            {
                List<string> blanketPurchaseOrderIds = vouBpoIds.Select(x => x).Distinct().ToList();
                //fetch blanket purchase order details and build dictionary
                var Bpos = await DataReader.BulkReadRecordAsync<DataContracts.Bpo>("BPO", blanketPurchaseOrderIds.ToArray());


                if (Bpos != null && Bpos.Any())
                    bpoDictionary = Bpos.ToDictionary(x => x.Recordkey);
            }

            return bpoDictionary;
        }

        /// <summary>
        /// Take first value from a Requisition Status collection and convert to RequisitionStatus enumeration value
        /// </summary>
        /// <param name="reqStatus">requisition status</param>
        /// <param name="requisitionId">requisition id</param>
        /// <returns>RequisitionStatus enumeration value</returns>
        private static VoucherStatus ConvertVoucherStatus(List<string> vouStatus, string voucherId)
        {
            var voucherStatus = new VoucherStatus();

            // Get the first status in the list of requisition statuses and check it has a value
            if ((vouStatus) != null && (vouStatus.Any()))
            {
                switch (vouStatus.FirstOrDefault().ToUpper())
                {
                    case "U":
                        voucherStatus = VoucherStatus.InProgress;
                        break;
                    case "N":
                        voucherStatus = VoucherStatus.NotApproved;
                        break;
                    case "O":
                        voucherStatus = VoucherStatus.Outstanding;
                        break;
                    case "P":
                        voucherStatus = VoucherStatus.Paid;
                        break;
                    case "R":
                        voucherStatus = VoucherStatus.Reconciled;
                        break;
                    case "V":
                        voucherStatus = VoucherStatus.Voided;
                        break;
                    case "X":
                        voucherStatus = VoucherStatus.Cancelled;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid voucher status for voucher: " + voucherId);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for voucher: " + voucherId);
            }

            return voucherStatus;
        }

        public async Task<VoucherCreateUpdateResponse> CreateVoucherAsync(VoucherCreateUpdateRequest createUpdateRequest)
        {
            if (createUpdateRequest == null)
                throw new ArgumentNullException("voucherEntity", "Must provide a createUpdateRequest to create.");

            VoucherCreateUpdateResponse response = new VoucherCreateUpdateResponse();
            var createRequest = BuildVoucherCreateRequest(createUpdateRequest);

            try
            {
                // write the  data
                var createResponse = await transactionInvoker.ExecuteAsync<TxCreateWebVouRequest, TxCreateWebVouResponse>(createRequest);
                if (string.IsNullOrEmpty(createResponse.AError) && (createResponse.AlErrorMessages == null || !createResponse.AlErrorMessages.Any()))
                {
                    response.ErrorOccured = false;
                    response.ErrorMessages = new List<string>();
                    response.VoucherId = createResponse.AVoucherId;
                    response.VoucherDate = createResponse.ARequestDate.Value;
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
        /// Get the Reimburse person address details for voucher.
        /// </summary>
        /// <param name="personId"> The person id for which address details needs to fetch</param>
        /// <returns>The reimburse person address detail for voucher</returns> 
        public async Task<VendorsVoucherSearchResult> GetReimbursePersonAddressForVoucherAsync(string personId)
        {
            Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult address = new Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult();

            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId required");

            var request = new TxGetReimbursePersonAddressRequest();
            request.APersonId = personId;

            try
            {
                var response = await transactionInvoker.ExecuteAsync<TxGetReimbursePersonAddressRequest, TxGetReimbursePersonAddressResponse>(request);
                if (response == null)
                {
                    throw new InvalidOperationException("An error occurred during transaction");
                }

                if (response.AlErrorMessages.Count() > 0 && response.AError)
                {
                    var errorMessage = "Error(s) occurred during fetching Reimburse person address deatils:";
                    errorMessage += string.Join(Environment.NewLine, response.AlErrorMessages);
                    logger.Error(errorMessage);
                    throw new InvalidOperationException("An error occurred during transaction");
                }
                else
                {
                    address.VendorId = response.APersonId;
                    // there may be multiple (sub-valued) messages for name.
                    address.VendorNameLines = !string.IsNullOrEmpty(response.APersonName) ? response.APersonName.Split(_SM).ToList() : new List<string>();
                    address.AddressId = response.APersonAddrId;
                    address.FormattedAddress = response.APersonFormattedAddress;
                    // there may be multiple (sub-valued) messages for address.
                    address.AddressLines = !string.IsNullOrEmpty(response.APersonAddress) ? response.APersonAddress.Split(_SM).ToList() : new List<string>();
                    address.City = response.APersonCity;
                    address.Country = response.APersonCountry;
                    address.State = response.APersonState;
                    address.Zip = response.APersonZip;
                }

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw e;
            }

            return address;
        }

        public async Task<VoucherCreateUpdateResponse> UpdateVoucherAsync(VoucherCreateUpdateRequest createUpdateRequest, Voucher originalVoucher)
        {
            if (createUpdateRequest == null)
                throw new ArgumentNullException("voucherEntity", "Must provide a createUpdateRequest to create.");

            VoucherCreateUpdateResponse response = new VoucherCreateUpdateResponse();
            var updateRequest = BuildVoucherUpdateRequest(createUpdateRequest, originalVoucher);

            try
            {
                // write the  data
                var createResponse = await transactionInvoker.ExecuteAsync<TxUpdateWebVoucherRequest, TxUpdateWebVoucherResponse>(updateRequest);
                if (string.IsNullOrEmpty(createResponse.AError) && (createResponse.AlErrorMessages == null || !createResponse.AlErrorMessages.Any()))
                {
                    response.ErrorOccured = false;
                    response.ErrorMessages = new List<string>();
                    response.VoucherId = createResponse.AVoucherId;
                    response.VoucherDate = originalVoucher.Date;
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
        /// Void a voucher.
        /// </summary>       
        /// <param name="VoucherVoidRequest">The voucher void request domain entity.</param>        
        /// <returns>The voucher void response entity</returns>
        public async Task<VoucherVoidResponse> VoidVoucherAsync(VoucherVoidRequest voidRequest)
        {
            if (voidRequest == null)
                throw new ArgumentNullException("voidRequestEntity", "Must provide a voidRequest to void a voucher.");

            VoucherVoidResponse response = new VoucherVoidResponse();
            var createRequest = BuildVoucherVoidRequest(voidRequest);

            try
            {
                // write the  data
                var voidResponse = await transactionInvoker.ExecuteAsync<TxVoidVoucherRequest, TxVoidVoucherResponse>(createRequest);
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

                response.VoucherId = voidResponse.AVoucherId;
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
        ///  Build Voucher for void Request
        /// </summary>
        /// <param name="voucherVoidRequest"></param>
        /// <returns></returns>
        private TxVoidVoucherRequest BuildVoucherVoidRequest(VoucherVoidRequest voidRequest)
        {
            var request = new TxVoidVoucherRequest();
            var personId = voidRequest.PersonId;
            var voucherId = voidRequest.VoucherId;
            var confirmationEmailAddresses = voidRequest.ConfirmationEmailAddresses;
            var comments = voidRequest.Comments;

            if (!string.IsNullOrEmpty(personId))
            {
                request.AUserId = personId;
            }
            if (!string.IsNullOrEmpty(voucherId))
            {
                request.AVoucherId = voucherId;
            }
            if (!string.IsNullOrEmpty(confirmationEmailAddresses))
            {
                request.AConfirmationEmailAddress = confirmationEmailAddresses;
            }
            if (!string.IsNullOrEmpty(comments))
            {
                request.AComments = comments;
            }

            return request;

        }

        /// <summary>
        ///  Build Voucher for create Request
        /// </summary>
        /// <param name="createUpdateRequest"></param>
        /// <returns></returns>
        private TxCreateWebVouRequest BuildVoucherCreateRequest(VoucherCreateUpdateRequest createUpdateRequest)
        {
            var request = new TxCreateWebVouRequest();
            var personId = createUpdateRequest.PersonId;            
            var confirmationEmailAddresses = createUpdateRequest.ConfEmailAddresses;
            var voucherEntity = createUpdateRequest.Voucher;

            if (!string.IsNullOrEmpty(personId))
            {
                request.APersonId = personId;
            }
            if (voucherEntity.Date != null)
            {
                request.ARequestDate = DateTime.SpecifyKind(voucherEntity.Date, DateTimeKind.Unspecified);
            }
            //NeededByDate
            if (voucherEntity.DueDate.HasValue)
            {
                request.ANeededByDate = DateTime.SpecifyKind(voucherEntity.DueDate.Value, DateTimeKind.Unspecified);
            }

            request.AInvoiceNo = voucherEntity.InvoiceNumber;

            //Invoice Date
            if (voucherEntity.InvoiceDate.HasValue)
            {
                request.AInvoiceDate = DateTime.SpecifyKind(voucherEntity.InvoiceDate.Value, DateTimeKind.Unspecified);
            }

            if (confirmationEmailAddresses != null && confirmationEmailAddresses.Any())
            {
                request.AlConfEmailAddresses = confirmationEmailAddresses;
            }

            if (!string.IsNullOrEmpty(voucherEntity.ApType))
            {
                request.AApType = voucherEntity.ApType;
            }
            request.AlInternalComments = new List<string>() { voucherEntity.Comments };

            if (voucherEntity.Approvers != null && voucherEntity.Approvers.Any())
            {
                request.AlNextApprovers = new List<string>();
                foreach (var nextApprover in voucherEntity.Approvers)
                {
                    request.AlNextApprovers.Add(nextApprover.ApproverId);
                }
            }

            // TaxCodes - In create Voucher, take taxcodes from first lineitem and assign to Tax code parameter 
            if (voucherEntity.LineItems != null && voucherEntity.LineItems.Any())
            {
                //Check if first lineitem has taxcode, as in create request it is auto applied to all lineitems
                var firstLineItem = voucherEntity.LineItems.FirstOrDefault();
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
            if (voucherEntity.LineItems != null && voucherEntity.LineItems.Any())
            {
                var lineItems = new List<Transactions.AlCreateVouLineItems>();

                foreach (var apLineItem in voucherEntity.LineItems)
                {
                    var lineItem = new Transactions.AlCreateVouLineItems()
                    {
                        AlItemDescs = apLineItem.Description,
                        AlItemQtys = apLineItem.Quantity.ToString(),
                        AlItemPrices = apLineItem.Price.ToString()
                    };
                    var glAccts = new List<string>();
                    var glDistributionAmounts = new List<decimal?>();
                    var projectNos = new List<string>();
                    if (apLineItem.GlDistributions != null && apLineItem.GlDistributions.Any())
                    {
                        foreach (var item in apLineItem.GlDistributions)
                        {
                            glAccts.Add(!string.IsNullOrEmpty(item.GlAccountNumber) ? item.GlAccountNumber : string.Empty);
                            glDistributionAmounts.Add(item.Amount);
                            projectNos.Add(!string.IsNullOrEmpty(item.ProjectNumber) ? item.ProjectNumber : string.Empty);
                        }
                    }
                    lineItem.AlItemGlAccts = string.Join("|", glAccts);
                    lineItem.AlItemGlAcctAmts = string.Join("|", glDistributionAmounts);
                    lineItem.AlItemProjectNos = string.Join("|", projectNos);
                    lineItems.Add(lineItem);
                }
                request.AlCreateVouLineItems = lineItems;
            }

            if (!string.IsNullOrEmpty(voucherEntity.VendorId))
            {
                request.AVendorId = voucherEntity.VendorId;
            }

            //person address details here

            if (createUpdateRequest.VendorsVoucherInfo != null)
            {
                if (createUpdateRequest.VendorsVoucherInfo.MiscVendor)
                {
                    request.AlVendorName = new List<string>() { voucherEntity.VendorName };
                }
                else
                {
                    request.AlVendorName = createUpdateRequest.VendorsVoucherInfo.VendorNameLines;
                }
                request.AlVendorAddress = createUpdateRequest.VendorsVoucherInfo.AddressLines;
                request.AVendorCity = createUpdateRequest.VendorsVoucherInfo.City;
                request.AVendorState = createUpdateRequest.VendorsVoucherInfo.State;
                request.AVendorZip = createUpdateRequest.VendorsVoucherInfo.Zip;
                request.AVendorCountry = createUpdateRequest.VendorsVoucherInfo.Country;
                request.AVendorMiscVendor = createUpdateRequest.VendorsVoucherInfo.MiscVendor;
                request.AVendorAddrId = createUpdateRequest.VendorsVoucherInfo.AddressId;
                request.AVendorReImburseMyself = createUpdateRequest.VendorsVoucherInfo.ReImburseMyself;
                request.AInternationalAddress = createUpdateRequest.VendorsVoucherInfo.IsInternationalAddress;
            }
            return request;
        }

        /// <summary>
        ///  Build Voucher for update Request
        /// </summary>
        /// <param name="createUpdateRequest"></param>
        /// <returns>TxUpdateWebVoucherRequest</returns>
        protected TxUpdateWebVoucherRequest BuildVoucherUpdateRequest(VoucherCreateUpdateRequest createUpdateRequest, Voucher originalVoucher)
        {
            var request = new TxUpdateWebVoucherRequest();
            var personId = createUpdateRequest.PersonId;
            var confirmationEmailAddresses = createUpdateRequest.ConfEmailAddresses;
            var voucherEntity = createUpdateRequest.Voucher;

            if (!string.IsNullOrEmpty(personId))
            {
                request.APersonId = personId;
            }
            if (!string.IsNullOrEmpty(voucherEntity.Id) && !voucherEntity.Id.Equals("NEW"))
            {
                request.AVoucherId = voucherEntity.Id;
            }
            //NeededByDate
            if (voucherEntity.DueDate.HasValue)
            {
                request.ADateNeeded = DateTime.SpecifyKind(voucherEntity.DueDate.Value, DateTimeKind.Unspecified);
            }
            if (!string.IsNullOrEmpty(voucherEntity.ApType))
            {
                request.AApType = voucherEntity.ApType;
            }
            if (confirmationEmailAddresses != null && confirmationEmailAddresses.Any())
            {
                request.AlConfEmailAddresses = confirmationEmailAddresses;
            }

            request.AlInternalComments = new List<string>() { voucherEntity.Comments };

            if (voucherEntity.Approvers != null && voucherEntity.Approvers.Any())
            {
                request.AlNextApprovers = new List<string>();
                foreach (var nextApprover in voucherEntity.Approvers)
                {
                    request.AlNextApprovers.Add(nextApprover.ApproverId);
                }
            }

            List<AlUpdatedVouLineItems> lineItems = BuildLineItemsForUpdate(originalVoucher, voucherEntity);
            request.AlUpdatedVouLineItems = lineItems;

            return request;
        }

        private static List<AlUpdatedVouLineItems> BuildLineItemsForUpdate(Voucher originalVoucher, Voucher voucherEntity)
        {
            var lineItems = new List<Data.ColleagueFinance.Transactions.AlUpdatedVouLineItems>();

            if (voucherEntity.LineItems != null && voucherEntity.LineItems.Any())
            {
                foreach (var apLineItem in voucherEntity.LineItems)
                {
                    var lineItem = new Data.ColleagueFinance.Transactions.AlUpdatedVouLineItems();

                    bool addLineItemToModify = true;
                    if (string.IsNullOrEmpty(apLineItem.Id) || apLineItem.Id.Equals("NEW"))
                    {
                        //New line item added
                        lineItem.AlLineItemIds = "";
                    }
                    else
                    {
                        lineItem.AlLineItemIds = apLineItem.Id;
                        var originalLineItem = originalVoucher.LineItems.FirstOrDefault(x => x.Id == apLineItem.Id);
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
                        lineItem.AlLineItemDescs = apLineItem.Description;
                        lineItem.AlLineItemQtys = apLineItem.Quantity.ToString();
                        lineItem.AlItemPrices = apLineItem.Price.ToString();
                        lineItem.AlItemUnitIssues = apLineItem.UnitOfIssue;
                        lineItem.AlItemVendorParts = apLineItem.VendorPart;
                        lineItem.AlItemComments = apLineItem.Comments;
                        lineItem.AlItemTrdDscPcts = apLineItem.TradeDiscountPercentage.HasValue ? apLineItem.TradeDiscountPercentage.Value.ToString() : null;
                        lineItem.AlItemTrdDscAmts = apLineItem.TradeDiscountAmount.HasValue ? apLineItem.TradeDiscountAmount.Value.ToString() : null;
                        lineItem.AlItemFxaFlags = apLineItem.FixedAssetsFlag;

                        lineItem.AlItemCommodityCode = !string.IsNullOrEmpty(apLineItem.CommodityCode) ? apLineItem.CommodityCode : "";


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
                        if (apLineItem.GlDistributions != null && apLineItem.GlDistributions.Any())
                        {
                            foreach (var item in apLineItem.GlDistributions)
                            {
                                glAccts.Add(!string.IsNullOrEmpty(item.GlAccountNumber) ? item.GlAccountNumber : string.Empty);
                                glDistributionAmounts.Add(item.Amount);
                                projectNos.Add(!string.IsNullOrEmpty(item.ProjectNumber) ? item.ProjectNumber : string.Empty);
                            }
                        }
                        lineItem.AlItemGlAccts = string.Join("|", glAccts);
                        lineItem.AlItemGlAcctAmts = string.Join("|", glDistributionAmounts);
                        lineItem.AlItemProjectNos = string.Join("|", projectNos);
                        lineItems.Add(lineItem);
                    }
                }
            }
            if (originalVoucher.LineItems != null && originalVoucher.LineItems.Any())
            {
                var deletedLineItems = originalVoucher.LineItems.Where(x => !voucherEntity.LineItems.Any(u => !string.IsNullOrEmpty(u.Id) && u.Id == x.Id));
                if (deletedLineItems != null && deletedLineItems.Any())
                {
                    foreach (var deletedItem in deletedLineItems)
                    {
                        var deletedLineItem = new Transactions.AlUpdatedVouLineItems();
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

            return lineItems;
        }
    }
}