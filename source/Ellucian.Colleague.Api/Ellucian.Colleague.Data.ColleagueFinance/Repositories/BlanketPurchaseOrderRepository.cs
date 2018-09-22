// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IBlanketPurchaseOrderRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BlanketPurchaseOrderRepository : BaseColleagueRepository, IBlanketPurchaseOrderRepository
    {
        /// <summary>
        /// This constructor to instantiate a blanket purchase order repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public BlanketPurchaseOrderRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            
        }

        /// <summary>
        /// Get a single blanket purchase order
        /// </summary>
        /// <param name="id">The Blanket Purchase Order ID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>A blanket purchase order domain entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string id, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (expenseAccounts == null)
            {
                expenseAccounts = new List<string>();
            }

            // Read the blanket purchase order record
            var bpo = await DataReader.ReadRecordAsync<Bpo>(id);
            if (bpo == null)
            {
                throw new KeyNotFoundException(string.Format("Blanket purchase order record {0} does not exist.", id));
            }

            // Translate the status code into a BlanketPurchaseOrderStatus enumeration value
            BlanketPurchaseOrderStatus bpoStatus = new BlanketPurchaseOrderStatus();

            // Get the first status in the list of blanket purchase order statuses and check it has a value
            if (bpo.BpoStatus != null && !string.IsNullOrEmpty(bpo.BpoStatus.FirstOrDefault()))
            {
                switch (bpo.BpoStatus.FirstOrDefault().ToUpper())
                {
                    case "C":
                        bpoStatus = BlanketPurchaseOrderStatus.Closed;
                        break;
                    case "U":
                        bpoStatus = BlanketPurchaseOrderStatus.InProgress;
                        break;
                    case "N":
                        bpoStatus = BlanketPurchaseOrderStatus.NotApproved;
                        break;
                    case "O":
                        bpoStatus = BlanketPurchaseOrderStatus.Outstanding;
                        break;
                    case "V":
                        bpoStatus = BlanketPurchaseOrderStatus.Voided;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid blanket purchase order status for blanket purchase order: " + bpo.Recordkey);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for blanket purchase order: " + bpo.Recordkey);
            }

            if (bpo.BpoStatusDate == null || !bpo.BpoStatusDate.First().HasValue)
            {
                throw new ApplicationException("Missing status date for blanket purchase order: " + bpo.Recordkey);
            }
            // The blanket purchase order status date contains one to many dates
            var bpoStatusDate = bpo.BpoStatusDate.First().Value.Date;

            #region Get Hierarchy Names

            // Determine the vendor name for the blanket purchase order. If there is a miscellaneous name, use it.
            // Otherwise, we will get the name for the vendor id further down.
            var bpoVendorName = bpo.BpoVenName.FirstOrDefault();

            // Use a colleague transaction to get all names at once. 
            List<string> personIds = new List<string>();
            List<string> hierarchies = new List<string>();
            List<string> personNames = new List<string>();
            string initiatorName = null;

            // If there is no vendor name and there is a vendor id, use the PO hierarchy to get the vendor name.
            if ((string.IsNullOrEmpty(bpoVendorName)) && (!string.IsNullOrEmpty(bpo.BpoVendor)))
            {
                personIds.Add(bpo.BpoVendor);
                hierarchies.Add("PO");
            }

            // Use the PREFERRED hierarchy for the initiator.
            if (!string.IsNullOrEmpty(bpo.BpoInitiator))
            {
                personIds.Add(bpo.BpoInitiator);
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
                                if ((ioPersonId == bpo.BpoVendor) && (hierarchy == "PO"))
                                {
                                    bpoVendorName = name;
                                }
                                if ((ioPersonId == bpo.BpoInitiator) && (hierarchy == "PREFERRED"))
                                {
                                    initiatorName = name;
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            if (!bpo.BpoDate.HasValue)
            {
                throw new ApplicationException("Missing date for blanket purchase order: " + bpo.Recordkey);
            }

            var bpoDomainEntity = new BlanketPurchaseOrder(bpo.Recordkey, bpo.BpoNo, bpoVendorName, bpoStatus, bpoStatusDate, bpo.BpoDate.Value.Date);

            bpoDomainEntity.VendorId = bpo.BpoVendor;
            if (!string.IsNullOrEmpty(initiatorName))
            {
                bpoDomainEntity.InitiatorName = initiatorName;
            }
            // The blanket purchase order amount is in local or foreign currency if a currency code is present
            bpoDomainEntity.Amount = bpo.BpoTotalAmt.HasValue ? bpo.BpoTotalAmt.Value : 0;
            bpoDomainEntity.CurrencyCode = bpo.BpoCurrencyCode;

            if (bpo.BpoMaintGlTranDate.HasValue)
            {
                bpoDomainEntity.MaintenanceDate = bpo.BpoMaintGlTranDate.Value.Date;
            }

            if (bpo.BpoExpireDate.HasValue)
            {
                bpoDomainEntity.ExpirationDate = bpo.BpoExpireDate.Value.Date;
            }

            bpoDomainEntity.ApType = bpo.BpoApType;
            bpoDomainEntity.Comments = bpo.BpoPrintedComments;
            bpoDomainEntity.InternalComments = bpo.BpoComments;
            bpoDomainEntity.Description = bpo.BpoDesc;

            // Add any associated requisitions to the blanket purchase order domain entity
            if ((bpo.BpoReqIds != null) && (bpo.BpoReqIds.Count > 0))
            {
                foreach (var requisitionId in bpo.BpoReqIds)
                {
                    if (!string.IsNullOrEmpty(requisitionId))
                    {
                        bpoDomainEntity.AddRequisition(requisitionId);
                    }
                }
            }

            // Add any associated vouchers to the blanket purchase order domain entity
            if ((bpo.BpoVouIds != null) && (bpo.BpoVouIds.Count > 0))
            {
                foreach (var voucherId in bpo.BpoVouIds)
                {
                    if (!string.IsNullOrEmpty(voucherId))
                    {
                        bpoDomainEntity.AddVoucher(voucherId);
                    }
                }
            }

            // Read the OPERS records associated with the approval signatures and 
            // next approvers on the blanket purchase order, and build approver objects.

            var operators = new List<string>();
            if ((bpo.BpoAuthorizations != null) && (bpo.BpoAuthorizations.Count > 0))
            {
                operators.AddRange(bpo.BpoAuthorizations);
            }
            if ((bpo.BpoNextApprovalIds != null) && (bpo.BpoNextApprovalIds.Count > 0))
            {
                operators.AddRange(bpo.BpoNextApprovalIds);
            }
            var uniqueOperators = operators.Distinct().ToList();
            if (uniqueOperators.Count > 0)
            {
                var Approvers = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
                if ((Approvers != null) && (Approvers.Count > 0))
                {
                    // loop through the opers, create Approver objects, add the name, and if they
                    // are one of the approvers of the blanket purchase order, add the approval date.
                    foreach (var appr in Approvers)
                    {
                        Approver approver = new Approver(appr.Recordkey);
                        var approverName = appr.SysUserName;
                        approver.SetApprovalName(approverName);
                        if ((bpo.BpoAuthEntityAssociation != null) && (bpo.BpoAuthEntityAssociation.Count > 0))
                        {
                            foreach (var approval in bpo.BpoAuthEntityAssociation)
                            {
                                if (approval.BpoAuthorizationsAssocMember == appr.Recordkey)
                                {
                                    if (approval.BpoAuthorizationDatesAssocMember.HasValue)
                                    {
                                        approver.ApprovalDate = approval.BpoAuthorizationDatesAssocMember.Value;
                                    }
                                    else
                                    {
                                        //throw exception
                                    }
                                }
                            }
                        }

                        // Add any approvals to the purchase order domain entity
                        bpoDomainEntity.AddApprover(approver);
                    }
                }
            }

            // If the blanket purchase order has GL distributions, populate the 
            // GL distribution domain entities and add them to the bpo domain entity
            if ((bpo.BpoGlEntityAssociation != null) && (bpo.BpoGlEntityAssociation.Any()))
            {
                if (glAccessLevel != GlAccessLevel.No_Access)
                {
                    foreach (var glDist in bpo.BpoGlEntityAssociation)
                    {
                        if (glAccessLevel == GlAccessLevel.Full_Access || expenseAccounts.Contains(glDist.BpoGlNoAssocMember))
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(glDist.BpoGlNoAssocMember))
                                {
                                    throw new ApplicationException("GL distribution account number cannot be null for BPO: " + bpo.Recordkey);
                                }

                                BlanketPurchaseOrderGlDistribution glDistribution = new BlanketPurchaseOrderGlDistribution(glDist.BpoGlNoAssocMember, glDist.BpoGlAmtAssocMember.HasValue ? glDist.BpoGlAmtAssocMember.Value : 0);
                                glDistribution.ExpensedAmount = 0;
                                glDistribution.ProjectId = glDist.BpoProjectCfIdAssocMember;
                                glDistribution.ProjectLineItemId = glDist.BpoPrjItemIdsAssocMember;
                                glDistribution.GlAccountDescription = "";

                                bpoDomainEntity.AddGlDistribution(glDistribution);
                            }
                            catch (ArgumentNullException anex)
                            {
                                LogDataError("BPO", bpo.Recordkey, glDist, anex, anex.Message);
                            }
                            catch (ApplicationException aex)
                            {
                                LogDataError("BPO", bpo.Recordkey, glDist, aex, aex.Message);
                            }
                            
                        }
                    }
                }

                // If the user can see at least one GL distribution, get any necessary GL account descriptions
                // and get any necessary project number and project item code
                if ((bpoDomainEntity.GlDistributions != null) && (bpoDomainEntity.GlDistributions.Count > 0))
                {
                    // Initialize master lists to get unique GL accounts, project IDs and project line item IDs,
                    // for all the items in the journal entry
                    List<string> itemGlAccounts = new List<string>();
                    List<string> itemProjectIds = new List<string>();
                    List<string> itemProjectLineIds = new List<string>();

                    List<string> glAccountDescriptions = new List<string>();

                    foreach (var glDist in bpoDomainEntity.GlDistributions)
                    {
                        if (!itemGlAccounts.Contains(glDist.GlAccountNumber))
                        {
                            itemGlAccounts.Add(glDist.GlAccountNumber);
                        }

                        if (!(string.IsNullOrEmpty(glDist.ProjectId)))
                        {
                            if (!itemProjectIds.Contains(glDist.ProjectId))
                            {
                                itemProjectIds.Add(glDist.ProjectId);
                            }
                        }

                        // If there are no project ids, there are no project item ids
                        if (!(string.IsNullOrEmpty(glDist.ProjectLineItemId)))
                        {
                            if (!itemProjectLineIds.Contains(glDist.ProjectLineItemId))
                            {
                                itemProjectLineIds.Add(glDist.ProjectLineItemId);
                            }
                        }
                    }

                    // Obtain the descriptions for all the GL accounts at once
                    GetGlAccountDescriptionRequest request = new GetGlAccountDescriptionRequest()
                    {
                        GlAccountIds = itemGlAccounts,
                        Module = "SS"
                    };

                    GetGlAccountDescriptionResponse response = transactionInvoker.Execute<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(request);

                    if (response != null)
                    {
                        for (int i = 0; i < response.GlDescriptions.Count(); i++)
                        {
                            foreach (var glDist in bpoDomainEntity.GlDistributions)
                            {
                                if (glDist.GlAccountNumber == response.GlAccountIds[i])
                                {
                                    glDist.GlAccountDescription = response.GlDescriptions[i];
                                }
                            }
                        }
                    }

                    // If there are project IDs, we need to get the project number,
                    // and also the project line item code for each project line item ID 
                    if ((itemProjectIds != null) && (itemProjectIds.Count > 0))
                    {
                        // Read the project records
                        var projectRecords = await DataReader.BulkReadRecordAsync<Projects>(itemProjectIds.ToArray());

                        // If there are project IDs, there should be project line item IDs
                        if ((itemProjectLineIds != null) && (itemProjectLineIds.Count > 0))
                        {
                            // Read the project line item records
                            var projectLineItemRecords = await DataReader.BulkReadRecordAsync<ProjectsLineItems>(itemProjectLineIds.ToArray());

                            if ((projectRecords != null) && (projectRecords.Count > 0))
                            {
                                // For each project ID, get the project number
                                foreach (var glDist in bpoDomainEntity.GlDistributions)
                                {
                                    foreach (var project in projectRecords)
                                    {
                                        if (project.Recordkey == glDist.ProjectId)
                                        {
                                            glDist.ProjectNumber = project.PrjRefNo;
                                        }
                                    }

                                    // For each project line item ID, get the project line item code
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

                // If the user can see at least one GL distribution, calculate the necessary expensed amounts
                if ((bpoDomainEntity.GlDistributions != null) && (bpoDomainEntity.GlDistributions.Count > 0))
                {
                    if ((bpoDomainEntity.Vouchers != null) && (bpoDomainEntity.Vouchers.Count > 0))
                    {
                        // Read all the voucher records associated with the bpo
                        var bpoVouchers = await DataReader.BulkReadRecordAsync<Vouchers>(bpoDomainEntity.Vouchers.ToArray());

                        // The expense amounts are calculated from items associated to vouchers whose
                        // status are not unfinished, not approved, voided or cancelled.

                        if ((bpoVouchers != null) && (bpoVouchers.Count > 0))
                        {
                            List<string> voucherItemIds = new List<string>();
                            foreach (var voucher in bpoVouchers)
                            {
                                if (!string.IsNullOrEmpty(voucher.VouStatus.FirstOrDefault().ToUpper()))
                                {
                                    var voucherStatus = voucher.VouStatus.FirstOrDefault().ToUpper();
                                    if ((voucherStatus != "U") && (voucherStatus != "N") && (voucherStatus != "V") && (voucherStatus != "X"))
                                    {
                                        foreach (var item in voucher.VouItemsId)
                                        {
                                            if (!voucherItemIds.Contains(item))
                                            {
                                                voucherItemIds.Add(item);
                                            }
                                        }
                                    }
                                }
                            }
                            if ((voucherItemIds != null) && (voucherItemIds.Count > 0))
                            {
                                // Read the item records
                                var items = await DataReader.BulkReadRecordAsync<Items>(voucherItemIds.ToArray());

                                if ((items != null) && (items.Count > 0))
                                {
                                    // Process each item to obtain the expense amounts
                                    foreach (var item in items)
                                    {
                                        foreach (var dist in item.VouchGlEntityAssociation)
                                        {
                                            foreach (var glDist in bpoDomainEntity.GlDistributions)
                                            {
                                                if ((dist.ItmVouGlNoAssocMember == glDist.GlAccountNumber) && (dist.ItmVouProjectCfIdAssocMember == glDist.ProjectId))
                                                {
                                                    glDist.ExpensedAmount += dist.ItmVouGlAmtAssocMember.HasValue ? dist.ItmVouGlAmtAssocMember.Value : 0;
                                                }
                                            }
                                        }
                                        // Also process the taxes distributions
                                        foreach (var taxGlDist in item.VouGlTaxesEntityAssociation)
                                        {
                                            foreach (var glDist in bpoDomainEntity.GlDistributions)
                                            {
                                                if ((taxGlDist.ItmVouTaxGlNoAssocMember == glDist.GlAccountNumber) && (taxGlDist.ItmVouTaxProjectCfIdAssocMember == glDist.ProjectId))
                                                {
                                                    glDist.ExpensedAmount += taxGlDist.ItmVouGlTaxAmtAssocMember.HasValue ? taxGlDist.ItmVouGlTaxAmtAssocMember.Value : 0;
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
            return bpoDomainEntity;
        }
    }
}
