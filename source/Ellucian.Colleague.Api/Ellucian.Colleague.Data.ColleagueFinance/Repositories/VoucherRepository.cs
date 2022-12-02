// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

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
using System.Collections.ObjectModel;
using System.Text;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IVoucherRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class VoucherRepository : BaseColleagueRepository, IVoucherRepository
    {
        private static char _SM = Convert.ToChar(DynamicArray.SM);
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
            logger.Debug(string.Format("voucher {0} ", voucherId));
            logger.Debug(string.Format("gl access level {0}", glAccessLevel));

            if (string.IsNullOrEmpty(voucherId))
            {
                throw new ArgumentNullException("voucherId");
            }

            if (glAccessAccounts == null)
            {
                glAccessAccounts = new List<string>();
                logger.Debug(string.Format("no GL accounts for voucher {0} ", voucherId));
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

            // Use a colleague transaction to get requestor name. 
            List<string> personIds = new List<string>();
            List<string> hierarchies = new List<string>();
            List<string> personNames = new List<string>();
            string requestorName = null;

            if ((!string.IsNullOrEmpty(voucher.VouRequestor)))
            {
                personIds.Add(voucher.VouRequestor);
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
                                    requestorName = name;
                            }
                        }
                    }
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
            voucherDomainEntity.ApprovalReturnedIndicator = (!string.IsNullOrEmpty(voucher.VouReturnFlag) && voucher.VouReturnFlag.Equals("Y"));
            // The voucher status date contains one to many dates
            var voucherStatusDate = (voucher.VouStatusDate != null && voucher.VouStatusDate.Any()) ? voucher.VouStatusDate.First() : null;
            if (!voucherStatusDate.HasValue)
            {
                throw new ApplicationException("Voucher status date is a required field.");
            }

            voucherDomainEntity.StatusDate = voucherStatusDate.Value;

            voucherDomainEntity.Amount = 0;
            voucherDomainEntity.CurrencyCode = voucher.VouCurrencyCode;

            voucherDomainEntity.VendorId = voucher.VouVendor;

            if (!string.IsNullOrEmpty(requestorName))
            {
                voucherDomainEntity.RequestorName = requestorName;
            }

            if (string.IsNullOrEmpty(voucher.VouAddressId))
            {
                voucherDomainEntity.VendorAddressLines = voucher.VouMiscAddress;
                voucherDomainEntity.VendorCity = voucher.VouMiscCity;
                voucherDomainEntity.VendorState = voucher.VouMiscState;
                voucherDomainEntity.VendorZip = voucher.VouMiscZip;
                voucherDomainEntity.VendorCountry = voucher.VouMiscCountry;
            }
            else
            {
                if (!string.IsNullOrEmpty(voucher.VouAltFlag) && voucher.VouAltFlag.Equals("Y"))
                {
                    voucherDomainEntity.VendorAddressLines = voucher.VouMiscAddress;
                    voucherDomainEntity.VendorCity = voucher.VouMiscCity;
                    voucherDomainEntity.VendorState = voucher.VouMiscState;
                    voucherDomainEntity.VendorZip = voucher.VouMiscZip;
                    voucherDomainEntity.VendorCountry = voucher.VouMiscCountry;
                }
                else
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

                // Assign Vendor address
                await GetVendorAddress(voucher, voucherDomainEntity);
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

            voucherDomainEntity.ConfirmationEmailAddresses = voucher.VouConfEmailAddresses;

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

            // Populate the line item domain entities and add them to the voucher domain entity
            var lineItemIds = voucher.VouItemsId;
            int documentFiscalYear = 0;
            if (lineItemIds != null && lineItemIds.Count() > 0)
            {
                // determine the fiscal year from the voucher date or maintenance date
                DateTime? transactionDate = voucher.VouDate;
                if (voucher.VouMaintGlTranDate != null)
                {
                    transactionDate = voucher.VouMaintGlTranDate;
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
                logger.Debug(string.Format("voucher fiscal year {0} ", documentFiscalYear));

                // Read the item records for the list of IDs in the voucher record
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

                    // define a unique list of GL numbers to use for funds availability when the user has full GL access.
                    List<string> availableFundsGlAccounts = new List<string>();

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
                                // build a list of unique list of GL numbers on the line item GL distributions
                                // if the user has full GL access.
                                if (glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, voucher, voucherDomainEntity))
                                {
                                    if (!availableFundsGlAccounts.Contains(glDistr.ItmVouGlNoAssocMember))
                                    {
                                        availableFundsGlAccounts.Add(glDistr.ItmVouGlNoAssocMember);
                                    }
                                }

                                // The GL Distribution always uses the local currency amount.
                                LineItemGlDistribution glDistribution = new LineItemGlDistribution(glDistr.ItmVouGlNoAssocMember,
                                    glDistr.ItmVouGlQtyAssocMember.HasValue ? glDistr.ItmVouGlQtyAssocMember.Value : 0,
                                    glDistr.ItmVouGlAmtAssocMember.HasValue ? glDistr.ItmVouGlAmtAssocMember.Value : 0,
                                    glDistr.ItmVouGlPctAssocMember.HasValue ? glDistr.ItmVouGlPctAssocMember.Value : 0);

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

                        if (hasGlAccess == true)
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

                    // For each GL number that the user for which the user has access, get the funds availability 
                    // information on disk and add the amounts that are on the voucher if the voucher status is
                    // In Progress or Not Approved.
                    if (glNumbersAllowed.Any() || ((glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, voucher, voucherDomainEntity)) && availableFundsGlAccounts.Any()))
                    {
                        logger.Debug(string.Format("Calculating funds availability"));
                        string[] fundsAvailabilityArray;
                        if ((glAccessLevel == GlAccessLevel.Full_Access || CanUserByPassGlAccessCheck(personId, voucher, voucherDomainEntity)) && availableFundsGlAccounts.Any())
                        {
                            fundsAvailabilityArray = availableFundsGlAccounts.ToArray();
                        }
                        else
                        {
                            fundsAvailabilityArray = glNumbersAllowed.ToArray();
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

                                    // if the voucher status is In Progress or Not Approved, add the GL amounts from the
                                    // line item GL distributions to the actuals funds availability information. Reduce the
                                    // encumbrance funds availability amount if voucher originated from a PO or BPO.
                                    if (voucher.VouStatus.First().ToUpper() == "U" || voucher.VouStatus.First().ToUpper() == "N")
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

                                        if (!string.IsNullOrEmpty(voucher.VouPoNo) || !string.IsNullOrEmpty(voucher.VouBpoId))
                                        {
                                            if (!string.IsNullOrEmpty(voucher.VouPoNo))
                                            {
                                                // if the voucher originated from a purchase order, then use the data contracts for the line items
                                                // on the voucher to update the actuals funds availability amount from the VOU line item GL distributions,
                                                // and update the encumbrance funds availability amount from the purchase order line item GL distributions.
                                                foreach (var lineItem in lineItemRecords)
                                                {
                                                    if ((lineItem.VouchGlEntityAssociation != null) && (lineItem.VouchGlEntityAssociation.Any()))
                                                    {
                                                        foreach (var glDist in lineItem.VouchGlEntityAssociation)
                                                        {
                                                            if (glDist != null)
                                                            {
                                                                if (glDist.ItmVouGlNoAssocMember == glAccount || budgetPoolAccounts.Contains(glDist.ItmVouGlNoAssocMember))
                                                                {
                                                                    if (glDist.ItmVouGlAmtAssocMember.HasValue)
                                                                    {
                                                                        accountActualAmount += glDist.ItmVouGlAmtAssocMember.Value;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
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
                                                                        accountEncumbranceAmount -= glDist.ItmPoGlAmtAssocMember.Value;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // TODO                                                   
                                            }
                                        }
                                        else
                                        {
                                            foreach (var lineItem in voucherDomainEntity.LineItems)
                                            {
                                                if (lineItem != null)
                                                {
                                                    foreach (var glDistribution in lineItem.GlDistributions)
                                                    {
                                                        if (glDistribution != null)
                                                        {
                                                            if (glDistribution.GlAccountNumber == glAccount || budgetPoolAccounts.Contains(glDistribution.GlAccountNumber))
                                                            {
                                                                accountActualAmount += glDistribution.Amount;
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
                                    foreach (var lineItem in voucherDomainEntity.LineItems)
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

            return voucherDomainEntity;
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
        /// Get a collection of voucher summary domain entity objects
        /// </summary>
        /// <param name="id">Person ID</param>        
        /// <returns>collection of voucher summary domain entity objects</returns>
        public async Task<IEnumerable<VoucherSummary>> GetVoucherSummariesByPersonIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            var cfWebDefaults = await DataReader.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS");

            var filteredVouchers = await ApplyFilterCriteriaAsync(personId, cfWebDefaults);

            if (!filteredVouchers.Any())
                return null;

            return await BuildVoucherSummaryList(personId, filteredVouchers);

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
                // The warning flag can contain the number of warnings.
                response.WarningOccured = (!string.IsNullOrWhiteSpace(createResponse.AWarning) && createResponse.AWarning != "0") ? true : false;
                response.WarningMessages = (createResponse.AlWarningMessages != null || createResponse.AlWarningMessages.Any()) ? createResponse.AlWarningMessages : new List<string>();

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw;
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
                throw;
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
                // The warning flag can contain the number of warnings.
                response.WarningOccured = (!string.IsNullOrWhiteSpace(createResponse.AWarning) && createResponse.AWarning != "0") ? true : false;
                response.WarningMessages = (createResponse.AlWarningMessages != null || createResponse.AlWarningMessages.Any()) ? createResponse.AlWarningMessages : new List<string>();

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw;
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
                throw;
            }

            return response;
        }



        /// <summary>
        /// Get the list of voucher's by vendor id and invoice number.
        /// </summary>
        /// <param name="vendorId">Vendor Id</param>
        /// <param name="invoiceNo">Invoice number</param>
        /// <returns>List of <see cref="Voucher2">Vouchers</see></returns> 
        public async Task<IEnumerable<Voucher>> GetVouchersByVendorAndInvoiceNoAsync(string vendorId, string invoiceNo)
        {
            if (string.IsNullOrEmpty(vendorId))
                throw new ArgumentNullException("vendorId", "vendorId is required");

            if (string.IsNullOrEmpty(invoiceNo))
                throw new ArgumentNullException("invoiceNo", "invoice number is required");


            List<string> filteredVoucherIds = new List<string>();

            string queryCriteriaString = string.Format("WITH VOU.DEFAULT.INVOICE.NO EQ '{0}'  AND VOU.VENDOR EQ '{1}'", invoiceNo, vendorId);
            filteredVoucherIds = await ExecuteQueryStatementAsync(filteredVoucherIds, queryCriteriaString);


            List<Voucher> filteredVouchers = new List<Voucher>();
            if (filteredVoucherIds != null && filteredVoucherIds.Any())
            {
                var voucherDataContractList = await DataReader.BulkReadRecordAsync<DataContracts.Vouchers>(filteredVoucherIds.ToArray());
                if (voucherDataContractList != null && voucherDataContractList.Any())
                {
                    Dictionary<string, string> hierarchyNameDictionary = await GetPersonHierarchyNamesDictionaryAsync(voucherDataContractList);

                    foreach (var voucher in voucherDataContractList)
                    {
                        Voucher voucherDomainEntity = BuildVoucherEntity(voucher, hierarchyNameDictionary);
                        filteredVouchers.Add(voucherDomainEntity);
                    }
                }

            }

            return filteredVouchers;
        }

        /// <summary>
        /// Get Voucher summary list for the given user
        /// </summary>
        /// <param name="criteria">procurement filter criteria</param>      
        /// <returns>list of voucher summary domain entity objects</returns>
        public async Task<IEnumerable<VoucherSummary>> QueryVoucherSummariesAsync(ProcurementDocumentFilterCriteria criteria)
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
            var filteredVoucherIds = await DataReader.SelectAsync("VOUCHERS", queryCriteria);

            if (filteredVoucherIds == null || !filteredVoucherIds.Any())
            {
                logger.Debug(string.Format("Vouchers not found for query string: {0}.", queryCriteria));
                return null;
            }
            logger.Info(string.Format("Vouchers count {0} found.", filteredVoucherIds.ToList().Count()));
            return await BuildVoucherSummaryList(personId, filteredVoucherIds.ToList());
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
                    var glDistributionQuantities = new List<decimal?>();
                    var glDistributionPercents = new List<decimal?>();
                    var projectNos = new List<string>();
                    if (apLineItem.GlDistributions != null && apLineItem.GlDistributions.Any())
                    {
                        foreach (var item in apLineItem.GlDistributions)
                        {
                            glAccts.Add(!string.IsNullOrEmpty(item.GlAccountNumber) ? item.GlAccountNumber : string.Empty);
                            glDistributionAmounts.Add(item.Amount);
                            glDistributionQuantities.Add(item.Quantity);
                            glDistributionPercents.Add(item.Percent);
                            projectNos.Add(!string.IsNullOrEmpty(item.ProjectNumber) ? item.ProjectNumber : string.Empty);
                        }
                    }
                    lineItem.AlItemGlAccts = string.Join("|", glAccts);
                    lineItem.AlItemGlAcctAmts = string.Join("|", glDistributionAmounts);
                    lineItem.AlItemGlAcctQtys = string.Join("|", glDistributionQuantities);
                    lineItem.AlItemGlAcctPcts = string.Join("|", glDistributionPercents);
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
                request.AVendorCountry = createUpdateRequest.VendorsVoucherInfo.CountryCode;
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
                        var glDistributionQuantities = new List<decimal?>();
                        var glDistributionPercents = new List<decimal?>();
                        var projectNos = new List<string>();
                        if (apLineItem.GlDistributions != null && apLineItem.GlDistributions.Any())
                        {
                            foreach (var item in apLineItem.GlDistributions)
                            {
                                glAccts.Add(!string.IsNullOrEmpty(item.GlAccountNumber) ? item.GlAccountNumber : string.Empty);
                                glDistributionAmounts.Add(item.Amount);
                                glDistributionQuantities.Add(item.Quantity);
                                glDistributionPercents.Add(item.Percent);
                                projectNos.Add(!string.IsNullOrEmpty(item.ProjectNumber) ? item.ProjectNumber : string.Empty);
                            }
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

        private async Task<List<string>> ApplyFilterCriteriaAsync(string personId, CfwebDefaults cfWebDefaults)
        {
            List<string> filteredVouchers = new List<string>();
            //where personId is requestor
            string reqPersonIdQuery = string.Format("WITH VOU.REQUESTOR EQ '{0}'", personId);
            filteredVouchers = await ExecuteQueryStatementAsync(filteredVouchers, reqPersonIdQuery);
            if (filteredVouchers != null && filteredVouchers.Any())
            {
                if (cfWebDefaults != null)
                {
                    string voucherStartEndTransDateQuery = string.Empty;
                    //Filter by CfwebCkrStartDate, CfwebCkrEndDate values configured in CFWP form
                    //when CfwebCkrStartDate & CfwebCkrEndDate has a value
                    if (cfWebDefaults.CfwebCkrStartDate.HasValue && cfWebDefaults.CfwebCkrEndDate.HasValue)
                    {
                        var startDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebCkrStartDate.Value);
                        var endDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebCkrEndDate.Value);
                        voucherStartEndTransDateQuery = string.Format("WITH (VOU.MAINT.GL.TRAN.DATE GE '{0}' AND VOU.MAINT.GL.TRAN.DATE LE '{1}') OR WITH (VOU.DATE GE '{0}' AND VOU.DATE LE '{1}')", startDate, endDate);
                    }
                    //when CfwebCkrStartDate has value but CfwebCkrEndDate is null
                    else if (cfWebDefaults.CfwebCkrStartDate.HasValue && !cfWebDefaults.CfwebCkrEndDate.HasValue)
                    {
                        var startDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebCkrStartDate.Value);
                        voucherStartEndTransDateQuery = string.Format("VOU.MAINT.GL.TRAN.DATE GE '{0}' OR WITH VOU.DATE GE '{0}'", startDate);
                    }
                    //when CfwebCkrStartDate is null but CfwebCkrEndDate has value
                    else if (!cfWebDefaults.CfwebCkrStartDate.HasValue && cfWebDefaults.CfwebCkrEndDate.HasValue)
                    {
                        var endDate = await GetUnidataFormatDateAsync(cfWebDefaults.CfwebCkrEndDate.Value);
                        voucherStartEndTransDateQuery = string.Format("WITH ((VOU.MAINT.GL.TRAN.DATE NE '') AND (VOU.MAINT.GL.TRAN.DATE LE '{0}')) OR WITH ((VOU.DATE NE '') AND (VOU.DATE LE '{0}'))", endDate);
                    }

                    if (!string.IsNullOrEmpty(voucherStartEndTransDateQuery))
                    {
                        filteredVouchers = await ExecuteQueryStatementAsync(filteredVouchers, voucherStartEndTransDateQuery);
                    }

                    //query by CfwebCkrStatuses if statuses are configured in CFWP form.
                    if (cfWebDefaults.CfwebCkrStatuses != null && cfWebDefaults.CfwebCkrStatuses.Any())
                    {
                        var voucherStatusesCriteria = string.Join(" ", cfWebDefaults.CfwebCkrStatuses.Select(x => string.Format("'{0}'", x.ToUpper())));
                        voucherStatusesCriteria = "WITH VOU.CURRENT.STATUS EQ " + voucherStatusesCriteria;
                        filteredVouchers = await ExecuteQueryStatementAsync(filteredVouchers, voucherStatusesCriteria);
                    }
                }
            }
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
            if (voucherData != null && voucherData.Any())
            {
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
            }

            #endregion
            return hierarchyNameDictionary;
        }
        private VoucherSummary BuildVoucherSummary(Vouchers voucherDataContract, Dictionary<string, PurchaseOrders> poDictionary, Dictionary<string, Bpo> bpoDictionary, string vendorName, string requestorName, Collection<Opers> opersCollection)
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

            var voucherSummaryEntity = new VoucherSummary(voucherDataContract.Recordkey, vendorName, voucherDate)
            {
                Status = voucherStatus,
                MaintenanceDate = maintenanceDate,
                VendorId = voucherDataContract.VouVendor,
                RequestorName = requestorName,
                Amount = voucherDataContract.VouTotalAmt.HasValue ? voucherDataContract.VouTotalAmt.Value : 0,
                InvoiceDate = voucherDataContract.VouDefaultInvoiceDate.HasValue ? voucherDataContract.VouDefaultInvoiceDate.Value : default(DateTime?),
                InvoiceNumber = voucherDataContract.VouDefaultInvoiceNo,
                ApprovalReturnedIndicator = (!string.IsNullOrEmpty(voucherDataContract.VouReturnFlag) && voucherDataContract.VouReturnFlag.Equals("Y"))
            };
            // build approvers and add to entity
            if ((voucherDataContract.VouAuthEntityAssociation != null) && (voucherDataContract.VouAuthEntityAssociation.Any()))
            {
                // Approver object is declared once
                Approver approver;
                foreach (var approval in voucherDataContract.VouAuthEntityAssociation)
                {
                    //get opersId for the requisition
                    var oper = opersCollection.FirstOrDefault(x => x.Recordkey == approval.VouAuthorizationsAssocMember);
                    if (oper != null)
                    {
                        approver = new Approver(oper.Recordkey);
                        approver.SetApprovalName(oper.SysUserName);
                        approver.ApprovalDate = approval.VouAuthorizationDatesAssocMember.Value;
                        voucherSummaryEntity.AddApprover(approver);
                    }
                }
            }
            // build next approvers and add to entity
            if ((voucherDataContract.VouApprEntityAssociation != null) && (voucherDataContract.VouApprEntityAssociation.Any()))
            {
                // Approver object is declared once
                Approver approver;
                foreach (var approval in voucherDataContract.VouApprEntityAssociation)
                {
                    //get opersId for the requisition
                    var oper = opersCollection.FirstOrDefault(x => x.Recordkey == approval.VouNextApprovalIdsAssocMember);
                    if (oper != null)
                    {
                        approver = new Approver(oper.Recordkey);
                        approver.SetApprovalName(oper.SysUserName);
                        voucherSummaryEntity.AddApprover(approver);
                    }
                }
            }
            // Add any associated purchase orders to the voucher summary domain entity
            if (!string.IsNullOrEmpty(voucherDataContract.VouPoNo))
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

        private static Voucher BuildVoucherEntity(Vouchers voucher, Dictionary<string, string> hierarchyNameDictionary)
        {
            if (voucher == null)
            {
                throw new KeyNotFoundException(string.Format("Voucher record does not exist."));
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
            var voucherVendorName = "";
            var requestorName = "";
            if (!string.IsNullOrEmpty(voucher.VouRequestor))
                hierarchyNameDictionary.TryGetValue(voucher.VouRequestor, out requestorName);

            // If there is no vendor name and there is a vendor id, use the hierarchy to get the vendor name.
            voucherVendorName = voucher.VouMiscName != null && voucher.VouMiscName.Any() ? voucher.VouMiscName.FirstOrDefault() : string.Empty;
            if ((string.IsNullOrEmpty(voucherVendorName)) && (!string.IsNullOrEmpty(voucher.VouVendor)))
            {
                hierarchyNameDictionary.TryGetValue(voucher.VouVendor, out voucherVendorName);
            }

            if (!voucher.VouDate.HasValue)
            {
                throw new ApplicationException("Missing voucher date for voucher: " + voucher.Recordkey);
            }

            if ((string.IsNullOrEmpty(voucher.VouApType)) && (voucherStatus != VoucherStatus.Cancelled))
            {
                throw new ApplicationException("Missing AP type for voucher: " + voucher.Recordkey);
            }

            if (string.IsNullOrEmpty(voucher.VouDefaultInvoiceNo))
            {
                throw new ApplicationException("Invoice number is a required field.");
            }
            if (!voucher.VouDefaultInvoiceDate.HasValue)
            {
                throw new ApplicationException("Invoice date is a required field.");
            }


            var voucherDomainEntity = new Voucher(voucher.Recordkey, voucher.VouDate.Value, voucherStatus, voucherVendorName);
            voucherDomainEntity.VendorId = voucher.VouVendor;
            voucherDomainEntity.InvoiceNumber = voucher.VouDefaultInvoiceNo;
            voucherDomainEntity.InvoiceDate = voucher.VouDefaultInvoiceDate;

            return voucherDomainEntity;
        }

        private async Task<string> BuildFilterCriteria(ProcurementDocumentFilterCriteria criteria, CfwebDefaults cfWebDefaults)
        {
            bool skipCFWPDateRangeFilter = false;
            bool skipCFWPStatusFilter = false;

            StringBuilder queryCriteria = new StringBuilder();
            //personID criteria
            queryCriteria.Append(string.Format("WITH VOU.REQUESTOR EQ '{0}' ", criteria.PersonId));

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
                    var status = ConvertVoucherStatus(item);
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
                    //criteria set in cfwebdefaults - CfwebCkrStartDate, CfwebCkrEndDate
                    var cfwpDateRangeCriteria = await BuildDateRangeQueryAsync(cfWebDefaults.CfwebCkrStartDate, cfWebDefaults.CfwebCkrEndDate);
                    if (!string.IsNullOrEmpty(cfwpDateRangeCriteria))
                    {
                        logger.Debug(string.Format("QueryVoucherSummaries - CFWP date range - data reader - query string: '{0}'.", cfwpDateRangeCriteria));
                        queryCriteria.Append(cfwpDateRangeCriteria);
                    }
                }
                if (!skipCFWPStatusFilter)
                {
                    //criteria set in cfwebdefaults - CfwebCkrStatuses (VOU.CURRENT.STATUS).
                    var statusesQuery = ProcurementFilterUtility.BuildListQuery(cfWebDefaults.CfwebCkrStatuses, "VOU.CURRENT.STATUS");
                    if (!string.IsNullOrEmpty(statusesQuery))
                    {
                        logger.Debug(string.Format("QueryVoucherSummaries - CFWP statuses - data reader - query string: '{0}'.", statusesQuery));
                        queryCriteria.Append(statusesQuery);
                    }
                }
            }

            //criteria sent from SS - VendorID's (VOU.VENDOR).
            var vendorIdCriteria = ProcurementFilterUtility.BuildListQuery(criteria.VendorIds, "VOU.VENDOR");
            if (!string.IsNullOrEmpty(vendorIdCriteria))
            {
                logger.Debug(string.Format("QueryVoucherSummaries - VendorId's - data reader - query string: '{0}'.", vendorIdCriteria));
                queryCriteria.Append(!string.IsNullOrEmpty(vendorIdCriteria) ? vendorIdCriteria : string.Empty);
            }
            //criteria sent from SS - Min - Max Amount (VOU.TOTAL.AMT).
            var amountCriteria = ProcurementFilterUtility.BuildAmountRangeQuery(criteria, "VOU.TOTAL.AMT");
            if (!string.IsNullOrEmpty(amountCriteria))
            {
                logger.Debug(string.Format("QueryVoucherSummaries - Amount range - data reader - query string: '{0}'.", amountCriteria));
                queryCriteria.Append(amountCriteria);
            }
            //criteria sent from SS - From - To date range
            if (skipCFWPDateRangeFilter)
            {
                var procurementDateRangeCriteria = await BuildDateRangeQueryAsync(criteria.DateFrom, criteria.DateTo);
                if (!string.IsNullOrEmpty(procurementDateRangeCriteria))
                {
                    logger.Debug(string.Format("QueryVoucherSummaries - SS procurement filter date range - data reader - query string: '{0}'.", procurementDateRangeCriteria));
                    queryCriteria.Append(procurementDateRangeCriteria);
                }
            }

            if (skipCFWPStatusFilter)
            {
                //criteria sent from SS - Statuses
                var procurementStatusesCriteria = ProcurementFilterUtility.BuildListQuery(procurementFilterStatuses, "VOU.CURRENT.STATUS");
                if (!string.IsNullOrEmpty(procurementStatusesCriteria))
                {
                    logger.Debug(string.Format("QueryVoucherSummaries - SS procurement filter statuses - data reader - query string: '{0}'.", procurementStatusesCriteria));
                    queryCriteria.Append(procurementStatusesCriteria);
                }
            }
            queryCriteria.Append("BY.DSND VOUCHERS.ID");
            logger.Debug(string.Format("QueryVoucherSummaries - data reader - query string: '{0}'.", queryCriteria));
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
                    startEndTransDateQuery = string.Format("AND WITH (VOU.MAINT.GL.TRAN.DATE GE '{0}' AND VOU.MAINT.GL.TRAN.DATE LE '{1}') OR (VOU.DATE GE '{0}' AND VOU.DATE LE '{1}') ", startDate, endDate);
                }
                //when dateFrom has value but dateTo is null
                else if (dateFrom.HasValue && !dateTo.HasValue)
                {
                    var startDate = await GetUnidataFormatDateAsync(dateFrom.Value);
                    startEndTransDateQuery = string.Format("AND WITH (VOU.MAINT.GL.TRAN.DATE GE '{0}' OR VOU.DATE GE '{0}') ", startDate);
                }
                //when dateFrom is null but dateTo has value
                else if (!dateFrom.HasValue && dateTo.HasValue)
                {
                    var endDate = await GetUnidataFormatDateAsync(dateTo.Value);
                    startEndTransDateQuery = string.Format("AND WITH ((VOU.MAINT.GL.TRAN.DATE NE '') AND (VOU.MAINT.GL.TRAN.DATE LE '{0}')) OR ((VOU.DATE NE '') AND (VOU.DATE LE '{0}')) ", endDate);
                }
            }

            return startEndTransDateQuery;
        }

        private async Task<IEnumerable<VoucherSummary>> BuildVoucherSummaryList(string personId, List<string> filteredVoucherIds)
        {
            var voucherList = new List<VoucherSummary>();

            if (!filteredVoucherIds.Any())
                return voucherList;

            var voucherData = await DataReader.BulkReadRecordAsync<DataContracts.Vouchers>(filteredVoucherIds.ToArray());

            if (voucherData != null && voucherData.Any())
            {
                Dictionary<string, string> hierarchyNameDictionary = await GetPersonHierarchyNamesDictionaryAsync(voucherData);
                Dictionary<string, PurchaseOrders> poDictionary = new Dictionary<string, PurchaseOrders>();
                Dictionary<string, Bpo> bpoDictionary = new Dictionary<string, Bpo>();
                //Dictionary<string, RcVouchers> rcvDictionary = new Dictionary<string, RcVouchers>();

                poDictionary = await BuildPurchaseOrderDictionaryAsync(voucherData);
                bpoDictionary = await BuildBlanketPODictionaryAsync(voucherData);
                //rcvDictionary = await BuildBlanketRcvDictionaryAsync(VoucherData);

                // Read the OPERS records associated with the approval signatures and 
                // next approvers on the voucher, and build approver objects.
                var operators = new List<string>();
                Collection<Opers> opersCollection = new Collection<Opers>();
                // get list of Approvers and next approvers from the entire voucher records
                var allVoucherDataApprovers = voucherData.SelectMany(voucherContract => voucherContract.VouAuthorizations).Distinct().ToList();
                if (allVoucherDataApprovers != null && allVoucherDataApprovers.Any(x => x != null))
                {
                    operators.AddRange(allVoucherDataApprovers);
                }
                var allVoucherDataNextApprovers = voucherData.SelectMany(voucherContract => voucherContract.VouNextApprovalIds).Distinct().ToList();
                if (allVoucherDataNextApprovers != null && allVoucherDataNextApprovers.Any(x => x != null))
                {
                    operators.AddRange(allVoucherDataNextApprovers);
                }
                var uniqueOperators = operators.Distinct().ToList();
                if (uniqueOperators.Count > 0)
                {
                    opersCollection = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                }


                foreach (var voucher in voucherData)
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
                        voucherList.Add(BuildVoucherSummary(voucher, poDictionary, bpoDictionary, VoucherVendorName, requestorName, opersCollection));
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            return voucherList;
        }


        private static string ConvertVoucherStatus(string status)
        {
            string voucherStatus = null;

            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToUpper())
                {
                    case "INPROGRESS":
                        voucherStatus = "U";
                        break;
                    case "NOTAPPROVED":
                        voucherStatus = "N";
                        break;
                    case "OUTSTANDING":
                        voucherStatus = "O";
                        break;
                    case "PAID":
                        voucherStatus = "P";
                        break;
                    case "RECONCILED":
                        voucherStatus = "R";
                        break;
                    case "VOIDED":
                        voucherStatus = "V";
                        break;
                    case "CANCELLED":
                        voucherStatus = "X";
                        break;
                }
            }

            return voucherStatus;
        }

        #region Get VendorAddress
        private async Task GetVendorAddress(Vouchers voucher, Voucher voucherDomainEntity)
        {
            if (!string.IsNullOrEmpty(voucher.VouVendor) && !string.IsNullOrEmpty(voucher.VouAddressId))
            {
                TxGetVoucherVendorResultsRequest searchRequest = new TxGetVoucherVendorResultsRequest();
                searchRequest.ASearchCriteria = voucher.VouVendor;
                searchRequest.AApType = string.Empty;
                try
                {
                    TxGetVoucherVendorResultsResponse searchResponse = await transactionInvoker.ExecuteAsync<TxGetVoucherVendorResultsRequest, TxGetVoucherVendorResultsResponse>(searchRequest);

                    if (searchResponse != null && searchResponse.VoucherVendorSearchResults != null && searchResponse.VoucherVendorSearchResults.Any())
                    {
                        var voucherAddressResult = searchResponse.VoucherVendorSearchResults.FirstOrDefault(x => x.AlVendorAddrIds == voucher.VouAddressId);
                        if (voucherAddressResult != null)
                        {
                            voucherDomainEntity.VendorAddressTypeCode = voucherAddressResult.AlVendAddrTypeCodes;
                            voucherDomainEntity.VendorAddressTypeDesc = voucherAddressResult.AlVendAddrTypeDesc;
                        }
                    }
                }
                catch (Exception)
                {
                    var message = string.Format("{0} Unable to get Vendor address by lookup.", voucher.VouVendor);
                    logger.Error(message);
                }

            }
        }
        #endregion
    }
}