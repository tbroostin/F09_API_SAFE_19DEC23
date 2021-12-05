// Copyright 2015-2021 Ellucian Company L.P. and its affiliates..

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
using System.Text;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IBlanketPurchaseOrderRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BlanketPurchaseOrderRepository : BaseColleagueRepository, IBlanketPurchaseOrderRepository
    {
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        private PurDefaults _purchasingDefaultsDataContract = null;
        private RepositoryException exception = new RepositoryException("");

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
                var Approvers = await DataReader.BulkReadRecordAsync<DataContracts.Opers>("UT.OPERS", uniqueOperators.ToArray(), true);
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
                                logger.Error(anex, "Could not create GL Distribution.");
                            }
                            catch (ApplicationException aex)
                            {
                                logger.Error(aex, "Could not create GL Distribution.");
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
                        var projectRecords = await DataReader.BulkReadRecordAsync<DataContracts.Projects>(itemProjectIds.ToArray());

                        // If there are project IDs, there should be project line item IDs
                        if ((itemProjectLineIds != null) && (itemProjectLineIds.Count > 0))
                        {
                            // Read the project line item records
                            var projectLineItemRecords = await DataReader.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(itemProjectLineIds.ToArray());

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

        /// <summary>
        /// Get the purchase order requested
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <param name="glAccessLevel">access level of current user.</param>
        /// <param name="expenseAccounts">List of expense accounts.</param>
        /// <returns>Tuple of PurchaseOrder entity objects <see cref="BlanketPurchaseOrder"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<BlanketPurchaseOrder>, int>> GetBlanketPurchaseOrdersAsync(int offset, int limit, string number)
        {
            var queryString = new StringBuilder();
            queryString.Append("WITH BPO.CURRENT.STATUS NE 'U' AND WITH BPO.ITEMS.ID = ''");
            if (!string.IsNullOrEmpty(number))
            {
                queryString.AppendFormat(" AND WITH BPO.NO = '{0}'", number);
            }
            var criteria = queryString.ToString();
            var blanketPurchaseOrderIds = await DataReader.SelectAsync("BPO", criteria);

            var totalCount = blanketPurchaseOrderIds.Count();
            Array.Sort(blanketPurchaseOrderIds);
            var subList = blanketPurchaseOrderIds.Skip(offset).Take(limit).ToArray();

            Collection<Person> personContracts = null;
            Collection<Address> addressContracts = null;

            var blanketPurchaseOrderData = await DataReader.BulkReadRecordAsync<DataContracts.Bpo>("BPO", subList);
            if (blanketPurchaseOrderData != null)
            {
                var personIds = blanketPurchaseOrderData.Select(bpo => bpo.BpoInitiator).Distinct().ToArray();
                if (personIds != null && personIds.Any())
                    personContracts = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", personIds);
                var addressIds = blanketPurchaseOrderData.Select(bpo => bpo.BpoIntgAddressId).Distinct().ToArray();
                if (addressIds != null && addressIds.Any())
                    addressContracts = await DataReader.BulkReadRecordAsync<Address>("ADDRESS", addressIds);
            }

            if (blanketPurchaseOrderData == null)
            {
                return new Tuple<IEnumerable<BlanketPurchaseOrder>, int>(new List<BlanketPurchaseOrder>(), 0);
            }

            var blanketPurchaseOrders = await BuildBlanketPurchaseOrdersAsync(blanketPurchaseOrderData, personContracts, addressContracts);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<BlanketPurchaseOrder>, int>(blanketPurchaseOrders, totalCount);

        }

        /// <summary>
        /// Get PurchaseOrder by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>PurchaseOrder entity object <see cref="BlanketPurchaseOrder"/></returns>
        public async Task<BlanketPurchaseOrder> GetBlanketPurchaseOrdersByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            string id = string.Empty;
            try
            {
                id = await GetBlanketPurchaseOrdersIdFromGuidAsync(guid);

                if (id == null || string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Format("No blanket-purchase-order was found for GUID '{0}'", guid));
                }
            }
            catch (Exception)
            {
                throw new KeyNotFoundException(string.Format("No blanket-purchase-orders was found for GUID '{0}'", guid));
            }

            Person personContract = null;
            Address addressContract = null;
            var blanketPurchaseOrder = await DataReader.ReadRecordAsync<Bpo>(id);
            if (blanketPurchaseOrder == null)
            {
                throw new KeyNotFoundException(string.Format("No blanket-purchase-orders was found for GUID '{0}'", guid));
            }
            if (!string.IsNullOrEmpty(blanketPurchaseOrder.BpoInitiator))
                personContract = await DataReader.ReadRecordAsync<Base.DataContracts.Person>("PERSON", blanketPurchaseOrder.BpoInitiator);
            if (!string.IsNullOrEmpty(blanketPurchaseOrder.BpoIntgAddressId))
                addressContract = await DataReader.ReadRecordAsync<Address>(blanketPurchaseOrder.BpoIntgAddressId);

            if (blanketPurchaseOrder != null && blanketPurchaseOrder.RecordGuid != null && blanketPurchaseOrder.RecordGuid != guid)
            {
                throw new KeyNotFoundException(string.Format("No blanket-purchase-orders was found for GUID '{0}'", guid));
            }

            if (blanketPurchaseOrder != null && blanketPurchaseOrder.BpoItemsId != null && blanketPurchaseOrder.BpoItemsId.Any())
            {
                throw new KeyNotFoundException(string.Format("No blanket-purchase-orders was found for GUID '{0}'", guid));
            }

            var blanketPurchaseOrderEntity = await BuildBlanketPurchaseOrderAsync(blanketPurchaseOrder, personContract, addressContract);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return blanketPurchaseOrderEntity;

        }

        public async Task<BlanketPurchaseOrder> CreateBlanketPurchaseOrdersAsync(BlanketPurchaseOrder blanketPurchaseOrdersEntity)
        {
            if (blanketPurchaseOrdersEntity == null)
                throw new ArgumentNullException("purchaseOrdersEntity", "Must provide a purchaseOrderEntity to create.");

            var extendedDataTuple = GetEthosExtendedDataLists();

            var createRequest = BuildBlanketPurchaseOrderUpdateRequest(blanketPurchaseOrdersEntity);
            createRequest.BpoId = null;
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<CreateUpdateBpoRequest, CreateUpdateBpoResponse>(createRequest);

            if (createResponse.UpdateBPOErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred creating purchaseOrder '{0}':", blanketPurchaseOrdersEntity.Guid);
                var exception = new RepositoryException("");
                createResponse.UpdateBPOErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created  from the database
            return await GetBlanketPurchaseOrdersByGuidAsync(createResponse.Guid);
        }

        public async Task<BlanketPurchaseOrder> UpdateBlanketPurchaseOrdersAsync(BlanketPurchaseOrder blanketPurchaseOrdersEntity)
        {
            if (blanketPurchaseOrdersEntity == null)
                throw new ArgumentNullException("blanketPurchaseOrdersEntity", "Must provide a blanketPurchaseOrdersEntity to update.");
            if (string.IsNullOrEmpty(blanketPurchaseOrdersEntity.Guid))
                throw new ArgumentNullException("blanketPurchaseOrdersEntity", "Must provide the guid of the blanketPurchaseOrdersEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var purchaseOrderId = await this.GetBlanketPurchaseOrdersIdFromGuidAsync(blanketPurchaseOrdersEntity.Guid);

            if (!string.IsNullOrEmpty(purchaseOrderId))
            {
                var extendedDataTuple = GetEthosExtendedDataLists();
                var updateRequest = BuildBlanketPurchaseOrderUpdateRequest(blanketPurchaseOrdersEntity);
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }
                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdateBpoRequest, CreateUpdateBpoResponse>(updateRequest);

                if ((!string.IsNullOrEmpty(updateResponse.Error) && updateResponse.Error != "0") || updateResponse.UpdateBPOErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating blanketPurchaseOrder '{0}':", blanketPurchaseOrdersEntity.Guid);
                    var exception = new RepositoryException("");
                    updateResponse.UpdateBPOErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)
                    {
                        Id = blanketPurchaseOrdersEntity.Guid,
                        SourceId = blanketPurchaseOrdersEntity.Id
                    }));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated entity from the database
                return await GetBlanketPurchaseOrdersByGuidAsync(blanketPurchaseOrdersEntity.Guid);
            }

            // perform a create instead
            return await CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrdersEntity);
        }

        /// <summary>
        /// Returns a BPO key from a GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Key to Blanket Purchase Order</returns>
        public async Task<string> GetBlanketPurchaseOrdersIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("No blanket-purchase-orders was found for guid " + guid);
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("No blanket-purchase-orders was found for guid " + guid);
            }

            if (foundEntry.Value.Entity != "BPO")
            {
                exception.AddError(new RepositoryError("GUID.Wrong.Type", string.Format("GUID {0} has different entity, {1}, than expected, BPO", guid, foundEntry.Value.Entity))
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

        /// <summary>
        /// Get a list of all project references within a blanket purchase order.
        /// </summary>
        /// <param name="projectIds">Project Keys.</param>
        /// <returns></returns>
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
        /// <param name="blanketPurchaseOrders">Collection of PurchaseOrder data contracts</param>
        /// <returns>PurchaseOrder domain entity</returns>
        private async Task<IEnumerable<BlanketPurchaseOrder>> BuildBlanketPurchaseOrdersAsync(IEnumerable<Bpo> blanketPurchaseOrders, IEnumerable<Person> personContracts, IEnumerable<Address> addressContracts)
        {
            var purchaseOrderCollection = new List<BlanketPurchaseOrder>();

            foreach (var blanketPurchaseOrder in blanketPurchaseOrders)
            {
                try
                {
                    Person personContract = null;
                    Address addressContract = null;
                    if (personContracts != null && personContracts.Any())
                    {
                        personContract = personContracts.FirstOrDefault(pc => pc.Recordkey == blanketPurchaseOrder.BpoInitiator);
                    }
                    if (addressContracts != null && addressContracts.Any())
                    {
                        addressContract = addressContracts.FirstOrDefault(ad => ad.Recordkey == blanketPurchaseOrder.BpoIntgAddressId);
                    }
                    purchaseOrderCollection.Add(await BuildBlanketPurchaseOrderAsync(blanketPurchaseOrder, personContract, addressContract));
                }
                catch (Exception ex)
                {
                    var repoError = new RepositoryError("blanketPurchaseOrders", ex.Message)
                    {
                        Id = blanketPurchaseOrder.RecordGuid,
                        SourceId = blanketPurchaseOrder.Recordkey
                    };
                    exception.AddError(repoError);
                }
            }

            return purchaseOrderCollection.AsEnumerable();
        }

        private async Task<BlanketPurchaseOrder> BuildBlanketPurchaseOrderAsync(Bpo blanketPurchaseOrder, Person personRecord, Address address)
        {
            if (blanketPurchaseOrder == null)
            {
                throw new ArgumentNullException("blanketPurchaseOrder");
            }

            string id = blanketPurchaseOrder.Recordkey;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            string guid = blanketPurchaseOrder.RecordGuid;

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            // todo srm make sure we don't get by id a record in BPO with line items

            // Translate the status code into a BlanketPurchaseOrderStatus enumeration value
            BlanketPurchaseOrderStatus? blanketPurchaseOrderStatus = null;

            // Get the first status in the list of purchase order statuses and check it has a value.
            if (blanketPurchaseOrder.BpoStatus != null && !string.IsNullOrEmpty(blanketPurchaseOrder.BpoStatus.FirstOrDefault()))
            {
                try
                {
                    blanketPurchaseOrderStatus = GetBlanketPurchaseOrderStatus(blanketPurchaseOrder.BpoStatus.FirstOrDefault(), blanketPurchaseOrder.BpoNo, blanketPurchaseOrder.RecordGuid, blanketPurchaseOrder.Recordkey);
                }
                catch
                {
                    exception.AddError(
                        new RepositoryError("blanketPurchaseOrder.status",
                        "Blanket Purchase Order Status is required. '" + blanketPurchaseOrder.BpoNo + "'")
                        {
                            Id = blanketPurchaseOrder.RecordGuid,
                            SourceId = blanketPurchaseOrder.Recordkey
                        }
                    );
                }
            }
            else
            {
                exception.AddError(
                    new RepositoryError("blanketPurchaseOrder.status",
                    "Missing status for blanket purchase order '" + blanketPurchaseOrder.BpoNo + "'")
                    {
                        Id = blanketPurchaseOrder.RecordGuid,
                        SourceId = blanketPurchaseOrder.Recordkey
                    }
                );
            }

            string purchaseOrderVendorName = "";
            DateTime purchaseOrderStatusDate = new DateTime();

            if (blanketPurchaseOrder.BpoStatusDate == null || !blanketPurchaseOrder.BpoStatusDate.Any() || !blanketPurchaseOrder.BpoStatusDate.First().HasValue)
            {
                exception.AddError(
                    new RepositoryError("blanketPurchaseOrder.transactionDate",
                    "Missing status date for blanket purchase order '" + blanketPurchaseOrder.BpoNo + "'")
                    {
                        Id = blanketPurchaseOrder.RecordGuid,
                        SourceId = blanketPurchaseOrder.Recordkey
                    }
                );
            }
            else
            {
                // The purchase order status date contains one to many dates
                purchaseOrderStatusDate = blanketPurchaseOrder.BpoStatusDate.First().Value;
            }

            if (blanketPurchaseOrder.BpoDate == null || !blanketPurchaseOrder.BpoDate.HasValue)
            {
                exception.AddError(
                    new RepositoryError("blanketPurchaseOrder.orderedOn",
                    "Missing date for blanket purchase order '" + blanketPurchaseOrder.BpoNo + "'")
                    {
                        Id = blanketPurchaseOrder.RecordGuid,
                        SourceId = blanketPurchaseOrder.Recordkey
                    }
                );
                return null;
            }

            var blanketPurchaseOrderDomainEntity = new BlanketPurchaseOrder(blanketPurchaseOrder.Recordkey, blanketPurchaseOrder.RecordGuid, blanketPurchaseOrder.BpoNo, purchaseOrderVendorName, blanketPurchaseOrderStatus, purchaseOrderStatusDate, blanketPurchaseOrder.BpoDate.Value.Date);
            
            blanketPurchaseOrderDomainEntity.SubmittedBy = blanketPurchaseOrder.BpoIntgSubmittedBy;

            // Existing Vendor info
            blanketPurchaseOrderDomainEntity.VendorId = blanketPurchaseOrder.BpoVendor;
            blanketPurchaseOrderDomainEntity.VendorAddressId = blanketPurchaseOrder.BpoIntgAddressId;

            if (!(string.IsNullOrEmpty(blanketPurchaseOrder.BpoInitiator)))
            {
                if (personRecord != null)
                {
                    var initiatorName = string.Concat(personRecord.FirstName, " ", personRecord.LastName);
                    if (!string.IsNullOrWhiteSpace(initiatorName))
                    {
                        blanketPurchaseOrderDomainEntity.InitiatorName = initiatorName;
                    }
                }
            }


            blanketPurchaseOrderDomainEntity.Amount = blanketPurchaseOrder.BpoTotalAmt.HasValue ? blanketPurchaseOrder.BpoTotalAmt.Value : 0;
            blanketPurchaseOrderDomainEntity.CurrencyCode = blanketPurchaseOrder.BpoCurrencyCode;
            if (blanketPurchaseOrder.BpoMaintGlTranDate.HasValue)
            {
                blanketPurchaseOrderDomainEntity.MaintenanceDate = blanketPurchaseOrder.BpoMaintGlTranDate.Value.Date;
            }

            blanketPurchaseOrderDomainEntity.Description = blanketPurchaseOrder.BpoDesc;
            blanketPurchaseOrderDomainEntity.ApType = blanketPurchaseOrder.BpoApType;
            blanketPurchaseOrderDomainEntity.Comments = blanketPurchaseOrder.BpoPrintedComments;
            blanketPurchaseOrderDomainEntity.InternalComments = blanketPurchaseOrder.BpoComments;
            blanketPurchaseOrderDomainEntity.Buyer = blanketPurchaseOrder.BpoBuyer;
            blanketPurchaseOrderDomainEntity.HostCountry = await GetHostCountryAsync();
            blanketPurchaseOrderDomainEntity.VendorTerms = blanketPurchaseOrder.BpoVendorTerms;
            blanketPurchaseOrderDomainEntity.CommodityCode = blanketPurchaseOrder.BpoDefaultCommodity;
            blanketPurchaseOrderDomainEntity.ExpirationDate = blanketPurchaseOrder.BpoExpireDate;
            blanketPurchaseOrderDomainEntity.ReferenceNo = blanketPurchaseOrder.BpoReferenceNo;

            // Get the ship to code or the default ship to code.
            blanketPurchaseOrderDomainEntity.ShipToCode = blanketPurchaseOrder.BpoShipTo;
            if (string.IsNullOrEmpty(blanketPurchaseOrder.BpoShipTo))
            {
                // Read only once per call instead of once per record.
                if(_purchasingDefaultsDataContract == null)
                    _purchasingDefaultsDataContract = await DataReader.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS");

                if (_purchasingDefaultsDataContract != null)
                {
                    blanketPurchaseOrderDomainEntity.ShipToCode = _purchasingDefaultsDataContract.PurShipToCode;
                }
            }

            //Alternative Shipping 
            blanketPurchaseOrderDomainEntity.AltShippingName = blanketPurchaseOrder.BpoAltShipName;
            blanketPurchaseOrderDomainEntity.AltShippingAddress = blanketPurchaseOrder.BpoAltShipAddress;
            blanketPurchaseOrderDomainEntity.AltShippingCity = blanketPurchaseOrder.BpoAltShipCity;
            blanketPurchaseOrderDomainEntity.AltShippingState = blanketPurchaseOrder.BpoAltShipState;
            blanketPurchaseOrderDomainEntity.AltShippingZip = blanketPurchaseOrder.BpoAltShipZip;
            blanketPurchaseOrderDomainEntity.AltShippingCountry = blanketPurchaseOrder.BpoIntgAltShipCountry;
            blanketPurchaseOrderDomainEntity.AltShippingPhone = blanketPurchaseOrder.BpoAltShipPhone;
            blanketPurchaseOrderDomainEntity.AltShippingPhoneExt = blanketPurchaseOrder.BpoAltShipExt;


            //Misc vendor information
            blanketPurchaseOrderDomainEntity.AltAddressFlag = blanketPurchaseOrder.BpoAltFlag == "Y" ? true : false;
            blanketPurchaseOrderDomainEntity.MiscIntgCorpPersonFlag = blanketPurchaseOrder.BpoIntgCorpPersonInd;
            blanketPurchaseOrderDomainEntity.MiscCountry = blanketPurchaseOrder.BpoVenCountry;
            blanketPurchaseOrderDomainEntity.MiscName = blanketPurchaseOrder.BpoVenName;
            blanketPurchaseOrderDomainEntity.MiscAddress = blanketPurchaseOrder.BpoVenAddress;
            blanketPurchaseOrderDomainEntity.MiscCity = blanketPurchaseOrder.BpoVenCity;
            blanketPurchaseOrderDomainEntity.MiscState = blanketPurchaseOrder.BpoVenState;
            blanketPurchaseOrderDomainEntity.MiscZip = blanketPurchaseOrder.BpoVenZip;

            if (!string.IsNullOrWhiteSpace(blanketPurchaseOrderDomainEntity.VendorAddressId) && blanketPurchaseOrderDomainEntity.AltAddressFlag == true)
            {
                if (address != null)
                {
                    int counter = 0;
                    foreach (var addLine in address.AddressLines)
                    {
                        if (blanketPurchaseOrderDomainEntity.MiscAddress.Count >= counter)
                        {
                            if (addLine != blanketPurchaseOrderDomainEntity.MiscAddress[counter])
                            {
                                blanketPurchaseOrderDomainEntity.VendorAddressId = "";
                                break;
                            }
                        }
                        else
                        {
                            blanketPurchaseOrderDomainEntity.VendorAddressId = "";
                            break;
                        }
                        counter++;
                    }
                    if (counter < blanketPurchaseOrderDomainEntity.MiscAddress.Count)
                        blanketPurchaseOrderDomainEntity.VendorAddressId = "";


                    if (address.Country != blanketPurchaseOrderDomainEntity.MiscCountry)
                        blanketPurchaseOrderDomainEntity.VendorAddressId = "";

                    if (address.City != blanketPurchaseOrderDomainEntity.MiscCity)
                        blanketPurchaseOrderDomainEntity.VendorAddressId = "";

                    if (address.State != blanketPurchaseOrderDomainEntity.MiscState)
                        blanketPurchaseOrderDomainEntity.VendorAddressId = "";

                    if (address.Zip != blanketPurchaseOrderDomainEntity.MiscZip)
                        blanketPurchaseOrderDomainEntity.VendorAddressId = "";
                }
            }


            blanketPurchaseOrderDomainEntity.DefaultInitiator = blanketPurchaseOrder.BpoInitiator;

            blanketPurchaseOrderDomainEntity.VoidGlTranDate = blanketPurchaseOrder.BpoVoidGlTranDate;

            // Add any associated requisitions to the purchase order domain entity
            if ((blanketPurchaseOrder.BpoReqIds != null) && (blanketPurchaseOrder.BpoReqIds.Count > 0))
            {
                foreach (var requisitionId in blanketPurchaseOrder.BpoReqIds)
                {
                    if (!string.IsNullOrEmpty(requisitionId))
                    {
                        blanketPurchaseOrderDomainEntity.AddRequisition(requisitionId);
                    }
                }
            }

            // Add any associated vouchers to the purchase order domain entity
            if ((blanketPurchaseOrder.BpoVouIds != null) && (blanketPurchaseOrder.BpoVouIds.Count > 0))
            {
                foreach (var voucherNumber in blanketPurchaseOrder.BpoVouIds)
                {
                    if (!string.IsNullOrEmpty(voucherNumber))
                    {
                        blanketPurchaseOrderDomainEntity.AddVoucher(voucherNumber);
                    }
                }
            }

            blanketPurchaseOrderDomainEntity.Fob = blanketPurchaseOrder.BpoFob;

            // Populate the GL Distribution domain entities and add them to the purchase order domain entity
            blanketPurchaseOrderDomainEntity = BuildGlDistribution(blanketPurchaseOrder, blanketPurchaseOrderDomainEntity);

            // Check for line items.  Currently, BPO's with line items are not supported.
            if (blanketPurchaseOrder.BpoItemsId != null && blanketPurchaseOrder.BpoItemsId.Any())
            {
                exception.AddError(
                    new RepositoryError("blanketPurchaseOrder",
                    "Blanket Purchase Orders with line item orders are not supported. '" + blanketPurchaseOrder.BpoNo + "'")
                    {
                        Id = blanketPurchaseOrder.RecordGuid,
                        SourceId = blanketPurchaseOrder.Recordkey
                    }
                );
            }

            return blanketPurchaseOrderDomainEntity;
        }

        private BlanketPurchaseOrderStatus? GetBlanketPurchaseOrderStatus(string status, string bpoNo, string recordGuid, string recordKey)
        {
            BlanketPurchaseOrderStatus? blanketPurchaseOrderStatus = null; 
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException(string.Concat("Blanket Purchase Order Status is required."));
            }
            switch (status.ToUpper())
            {
                case "C":
                    blanketPurchaseOrderStatus = BlanketPurchaseOrderStatus.Closed;
                    break;
                case "N":
                    blanketPurchaseOrderStatus = BlanketPurchaseOrderStatus.NotApproved;
                    break;
                case "O":
                    blanketPurchaseOrderStatus = BlanketPurchaseOrderStatus.Outstanding;
                    break;
                case "V":
                    blanketPurchaseOrderStatus = BlanketPurchaseOrderStatus.Voided;
                    break;
                default:
                    exception.AddError(
                    new RepositoryError("blanketPurchaseOrder.status",
                    "Blanket Purchase Orders in an unfinished state are not supported. '" + bpoNo + "'")
                        {
                            Id = recordGuid,
                            SourceId = recordKey
                        }
                    );
                    break;                    
            }

            return blanketPurchaseOrderStatus;
        }

        private BlanketPurchaseOrder BuildGlDistribution(Bpo bpo, BlanketPurchaseOrder bpoDomainEntity)
        {

            // If the blanket purchase order has GL distributions, populate the 
            // GL distribution domain entities and add them to the bpo domain entity
            if ((bpo.BpoGlEntityAssociation != null) && (bpo.BpoGlEntityAssociation.Any()))
            {
                foreach (var glDist in bpo.BpoGlEntityAssociation)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(glDist.BpoGlNoAssocMember))
                        {
                            exception.AddError(
                                new RepositoryError("blanketPurchaseOrder.orderDetails.accountDetails.accountingString",
                                "GL distribution account number cannot be null '" + bpo.BpoNo + "'")
                                {
                                    Id = bpo.RecordGuid,
                                    SourceId = bpo.Recordkey
                                }
                            );
                        }

                        decimal amount = glDist.BpoGlAmtAssocMember.HasValue ? glDist.BpoGlAmtAssocMember.Value : 0;
                        if (!string.IsNullOrEmpty(bpo.BpoCurrencyCode))
                        {
                            amount = glDist.BpoGlForeignAmtAssocMember.HasValue ? glDist.BpoGlForeignAmtAssocMember.Value : amount;
                        }

                        BlanketPurchaseOrderGlDistribution glDistribution = new BlanketPurchaseOrderGlDistribution(glDist.BpoGlNoAssocMember, amount);
                        glDistribution.ExpensedAmount = 0;
                        glDistribution.Percentage = glDist.BpoGlPctAssocMember.HasValue ? glDist.BpoGlPctAssocMember.Value : 0;
                        glDistribution.ProjectId = glDist.BpoProjectCfIdAssocMember;
                        glDistribution.ProjectLineItemId = glDist.BpoPrjItemIdsAssocMember;
                        glDistribution.GlAccountDescription = bpo.BpoDesc;

                        bpoDomainEntity.AddGlDistribution(glDistribution);
                    }
                    catch (ArgumentNullException anex)
                    {
                        logger.Error(anex, "Could not create GL Distribution.");
                        exception.AddError(
                            new RepositoryError("blanketPurchaseOrder.orderDetails.accountDetails",
                                anex.Message + " '" + bpo.BpoNo + "'")
                            {
                                Id = bpo.RecordGuid,
                                SourceId = bpo.Recordkey
                            }
                        );
                    }
                    catch (ApplicationException aex)
                    {
                        logger.Error(aex, "Could not create GL Distribution.");
                        exception.AddError(
                            new RepositoryError("blanketPurchaseOrder.orderDetails.accountDetails",
                                aex.Message + " '" + bpo.BpoNo + "'")
                            {
                                Id = bpo.RecordGuid,
                                SourceId = bpo.Recordkey
                            }
                        );
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

                }
            }
            return bpoDomainEntity;
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
        /// Get the GUID for an entity using its ID
        /// </summary>
        /// <param name="id">The Entity ID we are looking for</param>
        /// <param name="entity">Any entity found in Colleague</param>
        /// <returns>GUID</returns>
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

        /// <summary>
        /// Get Host Country from international parameters
        /// </summary>
        /// <returns>HOST.COUNTRY</returns>
        private async Task<string> GetHostCountryAsync()
        {
            try
            {
                if (_internationalParameters == null)
                    _internationalParameters = await GetInternationalParametersAsync();
            }
            catch
            {
                return "USA";
            }
            if (_internationalParameters != null)
                return _internationalParameters.HostCountry;
            return "USA";
        }

        /// <summary>
        /// Create an CreateUpdateBpoRequest from a PurchaseOrder domain entity
        /// </summary>
        /// <param name="blanketPurchaseOrderEntity">PurchaseOrder domain entity</param>
        /// <returns> CreateUpdateBpoRequest transaction object</returns>
        private CreateUpdateBpoRequest BuildBlanketPurchaseOrderUpdateRequest(BlanketPurchaseOrder blanketPurchaseOrderEntity)
        {
            // todo srm check NEW usage.???
            string bpoId = blanketPurchaseOrderEntity.Id;
            if (blanketPurchaseOrderEntity.Id == "NEW") { bpoId = null; }
            var request = new CreateUpdateBpoRequest
            {
                BpoId = bpoId,
                Guid = blanketPurchaseOrderEntity.Guid,
                BypassApprovalFlag = blanketPurchaseOrderEntity.bypassApprovals
            };
            request.TotalAmount = blanketPurchaseOrderEntity.Amount.ToString();
            request.Description = blanketPurchaseOrderEntity.Description;

            if (blanketPurchaseOrderEntity.Date != null)
            {
                request.OrderOn = blanketPurchaseOrderEntity.Date;
            }
            if (blanketPurchaseOrderEntity.MaintenanceDate != null && blanketPurchaseOrderEntity.MaintenanceDate.HasValue)
                request.TransactionDate = blanketPurchaseOrderEntity.MaintenanceDate;
            if (blanketPurchaseOrderEntity.ExpirationDate != null && blanketPurchaseOrderEntity.ExpirationDate.HasValue)
                request.ExpireDate = blanketPurchaseOrderEntity.ExpirationDate;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.Number))
                request.OrderNumber = blanketPurchaseOrderEntity.Number;
            //ctx is looking for 'B' for new order numbers
            if (request.OrderNumber == "new") { request.OrderNumber = "B"; }

            if (blanketPurchaseOrderEntity.ReferenceNo != null && blanketPurchaseOrderEntity.ReferenceNo.Any())
                request.ReferenceNumbers = blanketPurchaseOrderEntity.ReferenceNo;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.CurrencyCode))
                request.Currency = blanketPurchaseOrderEntity.CurrencyCode;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.Buyer))
                request.BuyerId = blanketPurchaseOrderEntity.Buyer;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.DefaultInitiator))
                request.InitiatorId = blanketPurchaseOrderEntity.DefaultInitiator;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.ShipToCode))
                request.ShipToId = blanketPurchaseOrderEntity.ShipToCode;

            request.Status = blanketPurchaseOrderEntity.Status.ToString();

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.Fob))
                request.FreeOnBoardId = blanketPurchaseOrderEntity.Fob;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.AltShippingName))
                request.OverrideDesc = blanketPurchaseOrderEntity.AltShippingName;

            if (blanketPurchaseOrderEntity.AltShippingAddress != null && blanketPurchaseOrderEntity.AltShippingAddress.Any())
                request.OverrideAddressLines = blanketPurchaseOrderEntity.AltShippingAddress;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.AltShippingCountry))
                request.OverridePlaceCountry = blanketPurchaseOrderEntity.AltShippingCountry;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.AltShippingState))
                request.OverrideRegionCode = blanketPurchaseOrderEntity.AltShippingState;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.AltShippingCity))
                request.OverrideLocality = blanketPurchaseOrderEntity.AltShippingCity;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.AltShippingZip))
                request.OverridePostalCode = blanketPurchaseOrderEntity.AltShippingZip;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.AltShippingPhoneExt))
                request.OverrideContactExt = blanketPurchaseOrderEntity.AltShippingPhoneExt;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.AltShippingPhone))
                request.OverrideContactNumber = blanketPurchaseOrderEntity.AltShippingPhone;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.VendorId))
                request.VendorId = blanketPurchaseOrderEntity.VendorId;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.VendorAddressId))
                request.VendorAddressId = blanketPurchaseOrderEntity.VendorAddressId;

            if (blanketPurchaseOrderEntity.MiscName != null && blanketPurchaseOrderEntity.MiscName.Any())
                request.ManualVendor = blanketPurchaseOrderEntity.MiscName[0];

            if (blanketPurchaseOrderEntity.MiscAddress != null && blanketPurchaseOrderEntity.MiscAddress.Any())
                request.MiscAddress = blanketPurchaseOrderEntity.MiscAddress;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.MiscCountry))
                request.MiscCountry = blanketPurchaseOrderEntity.MiscCountry;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.MiscState))
                request.MiscState = blanketPurchaseOrderEntity.MiscState;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.MiscCity))
                request.MiscCity = blanketPurchaseOrderEntity.MiscCity;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.MiscZip) && blanketPurchaseOrderEntity.MiscZip != "0")
                request.MiscPostalCode = blanketPurchaseOrderEntity.MiscZip;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.VendorTerms))
                request.PaymentTermsId = blanketPurchaseOrderEntity.VendorTerms;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.CommodityCode))
                request.Commodity = blanketPurchaseOrderEntity.CommodityCode;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.ApType))
                request.PaymentSourceId = blanketPurchaseOrderEntity.ApType;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.SubmittedBy))
                request.SubmittedBy = blanketPurchaseOrderEntity.SubmittedBy;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.InternalComments))
                request.Comments = blanketPurchaseOrderEntity.InternalComments;

            if (!string.IsNullOrEmpty(blanketPurchaseOrderEntity.Comments))
                request.PrintedComments = blanketPurchaseOrderEntity.Comments;

            if (blanketPurchaseOrderEntity.Requisitions != null && blanketPurchaseOrderEntity.Requisitions.Any())
                request.Requisitions = blanketPurchaseOrderEntity.Requisitions.ToList();

            var accountDetails = new List<Transactions.GlDistribution>();

            if (blanketPurchaseOrderEntity.GlDistributions != null && blanketPurchaseOrderEntity.GlDistributions.Any())
            {
                foreach (var distr in blanketPurchaseOrderEntity.GlDistributions)
                {

                    var lineItem = new Transactions.GlDistribution()
                    {
                        GlDistrGlNo = distr.GlAccountNumber,
                        GlDistrGlAmt = distr.EncumberedAmount.ToString(),
                        GlDistrGlPct = distr.Percentage.ToString()
                    };

                    accountDetails.Add(lineItem);
                }

                if (accountDetails != null && accountDetails.Any())
                {
                    request.GlDistribution = accountDetails;
                }

            }

            return request;
        }
    }
}
