// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IVoucherRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class VoucherRepository : BaseColleagueRepository, IVoucherRepository
    {
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

            voucherDomainEntity.Amount = 0;
            voucherDomainEntity.CurrencyCode = voucher.VouCurrencyCode;

            voucherDomainEntity.VendorId = voucher.VouVendor;

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
                    // Get all of the GL numbers for this voucher
                    var glNumbersAllowed = new List<string>();

                    // Only evaluate GL security if the GL access level is "Possible".
                    if (glAccessLevel == GlAccessLevel.Possible_Access)
                    {
                        foreach (var lineItem in lineItemRecords)
                        {
                            if ((lineItem.VouchGlEntityAssociation != null) && (lineItem.VouchGlEntityAssociation.Count > 0))
                            {
                                foreach (var glDistribution in lineItem.VouchGlEntityAssociation)
                                {
                                    if (glAccessAccounts.Contains(glDistribution.ItmVouGlNoAssocMember))
                                    {
                                        glNumbersAllowed.Add(glDistribution.ItmVouGlNoAssocMember);
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
                        lineItemDomainEntity.InvoiceNumber = lineItem.ItmInvoiceNo;
                        lineItemDomainEntity.TaxForm = lineItem.ItmTaxForm;
                        lineItemDomainEntity.TaxFormCode = lineItem.ItmTaxFormCode;
                        lineItemDomainEntity.TaxFormLocation = lineItem.ItmTaxFormLoc;
                        lineItemDomainEntity.Comments = lineItem.ItmComments;

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

                        // Only evaluate GL security if the GL access level is "Possible".
                        if (glAccessLevel == GlAccessLevel.Possible_Access)
                        {
                            // Figure out whether or not we should include this line item and if so, which GL numbers to display
                            bool includeLineItem = false;
                            if ((lineItemDomainEntity.GlDistributions != null) && (lineItemDomainEntity.GlDistributions.Count > 0))
                            {
                                foreach (var glDistribution in lineItemDomainEntity.GlDistributions)
                                {
                                    // If we have access to the GL number then mark the line item to be included.
                                    // Otherwise, mark the GL number as masked.
                                    if (glNumbersAllowed.Contains(glDistribution.GlAccountNumber))
                                    {
                                        includeLineItem = true;
                                    }
                                    else
                                    {
                                        glDistribution.Masked = true;
                                    }
                                }
                            }

                            if (includeLineItem)
                            {
                                voucherDomainEntity.AddLineItem(lineItemDomainEntity);
                            }
                        }
                        else
                        {
                            // If the GL access level is "Full Access" then add the line item
                            if (glAccessLevel == GlAccessLevel.Full_Access)
                            {
                                voucherDomainEntity.AddLineItem(lineItemDomainEntity);
                            }
                        }
                    }
                }
            }

            return voucherDomainEntity;
        }
    }
}