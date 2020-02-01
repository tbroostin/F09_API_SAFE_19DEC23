// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IAccountsPayableInvoicesRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AccountsPayableInvoicesRepository : BaseColleagueRepository, IAccountsPayableInvoicesRepository
    {

        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;

        /// <summary>
        /// This constructor allows us to instantiate a AccountsPayableInvoices repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public AccountsPayableInvoicesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Get AccountsPayableInvoices Domain Entity
        /// Will not support vouchers that are paying student refunds or employee payroll related refunds, advances.
        /// Does now support invoices without VOU.VENDOR for V11 EEDM accounts-payable-invoices.
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<AccountsPayableInvoices>, int>> GetAccountsPayableInvoices2Async(int offset, int limit)
        {
            // We need to exclude any AP Types where the source does not equal Regular Accounts Payable
            // (ie - exclude Accounts Receivable and PayrollTax.Deductions)
            var apTypesToInclude = await DataReader.SelectAsync("AP.TYPES", "WITH APT.SOURCE EQ 'R'");

            var criteria = "WITH VOU.AP.TYPE EQ ?";

            var voucherIds = await DataReader.SelectAsync("VOUCHERS", criteria,
                apTypesToInclude.Select(id => string.Format("\"{0}\"", id)).ToArray());

            //Exclude any voucher with a status of 'X' 
            voucherIds = await DataReader.SelectAsync("VOUCHERS", voucherIds,
                "WITH VOU.CURRENT.STATUS NE 'X''U' "); // AND EVERY VOU.STATUS NE 'V'");

            var totalCount = voucherIds.Count();
            Array.Sort(voucherIds);
            var subList = voucherIds.Skip(offset).Take(limit).ToArray();

            var voucherData = await DataReader.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", subList);

            if (voucherData == null)
            {
                throw new KeyNotFoundException("No records selected from Vouchers file in Colleague.");
            }

            var personSubList = voucherData.Where(x => !string.IsNullOrEmpty(x.VouVendor)).Select(c => c.VouVendor).Distinct().ToArray();
            var personsData = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", personSubList);

            var accountsPayableInvoices = await BuildAccountsPayableInvoices(voucherData, personsData);

            return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(accountsPayableInvoices, totalCount);
        }

        /// <summary>
        /// Get a single voucher using a GUID
        /// </summary>
        /// <param name="guid">The voucher guid</param>
        /// <returns>The AccountsPayableInvoices domain entity</returns>
        public async Task<AccountsPayableInvoices> GetAccountsPayableInvoicesByGuidAsync(string guid, bool allowVoid)
        {
            try
            {
                string id = await GetVoucherIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for voucher guid:", guid));
                }
                AccountsPayableInvoices accountsPayableInvoice = null;

                // Now we have an ID, so we can read the record
                var voucher = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vouchers>(id);
                if (voucher == null)
                {
                    throw new KeyNotFoundException("No accounts-payable-invoices was found for guid " + guid);
                }

                if (voucher.VouStatus.Contains("X"))
                {
                    throw new KeyNotFoundException("The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS is not valid for accounts-payable-invoices.");
                }

                // exclude those Vouchers that are inprogress
                if (voucher != null && voucher.VouStatus != null && voucher.VouStatus.Any() && voucher.VouStatus.FirstOrDefault().Equals("U", StringComparison.OrdinalIgnoreCase))
                {
                    throw new KeyNotFoundException("The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS is not valid for accounts-payable-invoices.");
                }

                Ellucian.Colleague.Data.Base.DataContracts.Person person = null;
                if (!string.IsNullOrEmpty(voucher.VouVendor))
                {
                    person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", voucher.VouVendor);
                    if (person == null)
                    {
                        throw new ArgumentOutOfRangeException("Person Id " + voucher.VouVendor + " is not returning any data. Person may be corrupted.");
                    }
                }

                var apTypesToInclude = await DataReader.SelectAsync("AP.TYPES", "WITH APT.SOURCE EQ 'R'");

                if ((apTypesToInclude != null) && (!(apTypesToInclude.ToList().Contains(voucher.VouApType))))
                {
                    throw new KeyNotFoundException("The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS is not valid for accounts-payable-invoices.");
                }
               
                accountsPayableInvoice = await BuildAccountsPayableInvoice(voucher, person); //, vendor);

                return accountsPayableInvoice;
            }
            catch (KeyNotFoundException e)
            {
                throw new KeyNotFoundException(e.Message);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }
            catch (ApplicationException ae)
            {
                throw new ApplicationException(ae.Message);
            }
            catch (Exception e)
            {
                throw new KeyNotFoundException("No accounts-payable-invoices was found for guid " + guid);
            }
        }

        /// <summary>
        /// Update an AccountsPayableInvoices domain entity
        /// </summary>
        /// <param name="accountsPayableInvoicesEntity">The AccountsPayableInvoices domain entity to update</param>
        /// <returns>The updated AccountsPayableInvoices domain entity</returns>
        public async Task<AccountsPayableInvoices> UpdateAccountsPayableInvoicesAsync(AccountsPayableInvoices accountsPayableInvoicesEntity)
        {
            if (accountsPayableInvoicesEntity == null)
                throw new ArgumentNullException("accountsPayableInvoicesEntity", "Must provide a accountsPayableInvoicesEntity to update.");
            if (string.IsNullOrEmpty(accountsPayableInvoicesEntity.Guid))
                throw new ArgumentNullException("accountsPayableInvoicesEntity", "Must provide the guid of the accountsPayableInvoicesEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var  accountsPayableInvoicesId = await this.GetAccountsPayableInvoicesIdFromGuidAsync(accountsPayableInvoicesEntity.Guid);
           
            if (!string.IsNullOrEmpty(accountsPayableInvoicesId))
            {
                var extendedDataTuple = GetEthosExtendedDataLists();

                var updateRequest = BuildAccountsPayableInvoicesUpdateRequest(accountsPayableInvoicesEntity);

                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateVouchersIntegrationRequest, UpdateVouchersIntegrationResponse>(updateRequest);

                if (updateResponse.UpdateVouchersIntegrationErrors.Any())
                {
                    //var errorMessage = string.Format("Error(s) occurred updating accountsPayableInvoices '{0}':", accountsPayableInvoicesEntity.Guid);
                    var exception = new RepositoryException();
                    foreach (var err in updateResponse.UpdateVouchersIntegrationErrors)
                    {
                        exception.AddError(new RepositoryError(string.IsNullOrEmpty(err.ErrorCodes) ? "" : err.ErrorCodes, err.ErrorMessages));
                    }
                    throw exception;
                }

                // get the updated entity from the database
                return await GetAccountsPayableInvoicesByGuidAsync(accountsPayableInvoicesEntity.Guid, true);
            }

            // perform a create instead
            return await CreateAccountsPayableInvoicesAsync(accountsPayableInvoicesEntity);
        }

        /// <summary>
        /// Create an AccountsPayableInvoices domain entity
        /// </summary>
        /// <param name="accountsPayableInvoicesEntity">The AccountsPayableInvoices domain entity to create</param>
        /// <returns>The created AccountsPayableInvoices domain entity</returns>       
        public async Task<AccountsPayableInvoices> CreateAccountsPayableInvoicesAsync(AccountsPayableInvoices accountsPayableInvoicesEntity)
        {
            if (accountsPayableInvoicesEntity == null)
                throw new ArgumentNullException("accountsPayableInvoicesEntity", "Must provide a accountsPayableInvoicesEntity to create.");

            var extendedDataTuple = GetEthosExtendedDataLists();

            var createRequest = BuildAccountsPayableInvoicesUpdateRequest(accountsPayableInvoicesEntity);

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }
            createRequest.VouId = null;
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdateVouchersIntegrationRequest, UpdateVouchersIntegrationResponse>(createRequest);

            if (createResponse.UpdateVouchersIntegrationErrors.Any())
            {
                //var errorMessage = string.Format("Error(s) occurred creating accountsPayableInvoices '{0}':", accountsPayableInvoicesEntity.Guid);
                var exception = new RepositoryException();
                foreach (var err in createResponse.UpdateVouchersIntegrationErrors)
                {
                    exception.AddError(new RepositoryError(string.IsNullOrEmpty(err.ErrorCodes) ? "" : err.ErrorCodes, err.ErrorMessages));           
                }
                throw exception;
            }

            // get the newly created  from the database
            return await GetAccountsPayableInvoicesByGuidAsync(createResponse.VoucherGuid, true); 
        }

        /// <summary>
        /// Get AccountsPayableInvoices id from Guid
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>id</returns>
        public async Task<string> GetAccountsPayableInvoicesIdFromGuidAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
        }

        /// <summary>
        /// Get AccountsPayableInvoices ID from GUID
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>id</returns>
        public async Task<string> GetAccountsPayableInvoicesGuidFromIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("Id", "ID is a required argument.");

            var lookup = new RecordKeyLookup("VOUCHERS", id, false);
            var result = await DataReader.SelectAsync(new RecordKeyLookup[] { lookup });
            if (result != null && result.Count > 0)
            {
                RecordKeyLookupResult lookupResult = null;
                if (result.TryGetValue(lookup.ResultKey, out lookupResult))
                {
                    if (lookupResult != null)
                    {
                        return lookupResult.Guid;
                    }
                }
            }

            throw new ArgumentOutOfRangeException("voucherId", "No GUID found for ID " + id);
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<string> GetVoucherIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] {new GuidLookup(guid)});
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Vouchers GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Vouchers GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "VOUCHERS")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, VENDORS");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single AccountsPayableInvoice using an ID
        /// </summary>
        /// <param name="id">The AccountsPayableInvoice ID</param>
        /// <returns>The AccountsPayableInvoices</returns>
        private async Task<AccountsPayableInvoices> GetAccountsPayableInvoicesAsync(string id, bool allowVoid)
        {
            AccountsPayableInvoices accountsPayableInvoice = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a voucher.");
            }

            // Now we have an ID, so we can read the record
            var voucher = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vouchers>(id);
            if (voucher == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found for voucher with ID ", id, "invalid."));
            }
            Ellucian.Colleague.Data.Base.DataContracts.Person person = null;
            //Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors vendor = null;
            if (!string.IsNullOrEmpty(voucher.VouVendor))
            {
                person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", voucher.VouVendor);
                if (person == null)
                {
                    throw new ArgumentOutOfRangeException("Person Id " + voucher.VouVendor + " is not returning any data. Person may be corrupted.");
                }
            }

            var apTypesToInclude = await DataReader.SelectAsync("AP.TYPES", "WITH APT.SOURCE EQ 'R'");
            
            if ((apTypesToInclude != null) && (!(apTypesToInclude.ToList().Contains(voucher.VouApType))))
            {
                throw new ArgumentException("Does not support vouchers that are paying student refunds or employee payroll related refunds, advances");
            }
            if (voucher.VouStatus.Contains("X"))
            {
                throw new ArgumentException("Does not support vouchers that are cancelled.");
            }

            if (voucher.VouStatus.Contains("V") && (allowVoid == false))
            {
                throw new ArgumentException("Does not support vouchers that are cancelled or voided.");
            }
            accountsPayableInvoice = await BuildAccountsPayableInvoice(voucher, person); //, vendor);

            return accountsPayableInvoice;
        }

        /// <summary>
        /// Create an UpdateVouchersIntegrationRequest from an AccountsPayableInvoices domain entity
        /// </summary>
        /// <param name="accountsPayableInvoicesEntity">AccountsPayableInvoices domain entity</param>
        /// <returns>UpdateVouchersIntegrationRequest transaction object</returns>
        private UpdateVouchersIntegrationRequest BuildAccountsPayableInvoicesUpdateRequest(AccountsPayableInvoices accountsPayableInvoicesEntity)
        {
            var lineItemSequence = 0;

            var request = new UpdateVouchersIntegrationRequest
            {
                VouId = accountsPayableInvoicesEntity.Id,
                VoucherGuid = accountsPayableInvoicesEntity.Guid,
                TransactionDate = accountsPayableInvoicesEntity.Date,
                VendorInvoiceDate = accountsPayableInvoicesEntity.InvoiceDate

            };
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VendorId))
                request.VendorId = accountsPayableInvoicesEntity.VendorId;
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VendorAddressId))
                request.VendorAddressId = accountsPayableInvoicesEntity.VendorAddressId;
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherAddressId))
                request.VendorAddressId = accountsPayableInvoicesEntity.VoucherAddressId;
            // Manual Vendor Details
            if (accountsPayableInvoicesEntity.VoucherMiscName != null && accountsPayableInvoicesEntity.VoucherMiscName.Any())
                request.VendorName = accountsPayableInvoicesEntity.VoucherMiscName;
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherMiscType))
                request.VendorType = accountsPayableInvoicesEntity.VoucherMiscType;
            if (accountsPayableInvoicesEntity.VoucherMiscAddress != null && accountsPayableInvoicesEntity.VoucherMiscAddress.Any())
                request.VendorAddress = accountsPayableInvoicesEntity.VoucherMiscAddress;
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherMiscCity))
                request.VendorCity = accountsPayableInvoicesEntity.VoucherMiscCity;
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherMiscState))
                request.VendorState = accountsPayableInvoicesEntity.VoucherMiscState;
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherMiscZip))
                request.VendorZip = accountsPayableInvoicesEntity.VoucherMiscZip;
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherMiscCountry))
                request.VendorCountry = accountsPayableInvoicesEntity.VoucherMiscCountry;

            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.InvoiceNumber))
                request.VendorInvoiceNumber = accountsPayableInvoicesEntity.InvoiceNumber;

            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.ApType))
                request.PaymentSourceId = accountsPayableInvoicesEntity.ApType;
            if (accountsPayableInvoicesEntity.VoucherDiscAmt.HasValue)
                request.InvoiceDiscAmt = accountsPayableInvoicesEntity.VoucherDiscAmt;
            if (accountsPayableInvoicesEntity.DueDate.HasValue)
                request.PaymentDue = accountsPayableInvoicesEntity.DueDate;

            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherVendorTerms))
                request.PaymentTerms = accountsPayableInvoicesEntity.VoucherVendorTerms;

            request.InvoiceComment = accountsPayableInvoicesEntity.Comments;
            if (accountsPayableInvoicesEntity.VoucherReferenceNo != null && accountsPayableInvoicesEntity.VoucherReferenceNo.Any())
                request.ReferenceNo = accountsPayableInvoicesEntity.VoucherReferenceNo.FirstOrDefault();

            if (accountsPayableInvoicesEntity.VoucherVoidGlTranDate.HasValue)
                request.VoidDate = accountsPayableInvoicesEntity.VoucherVoidGlTranDate;

            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherRequestor))
                request.LiaSubmittedBy = new List<string>()
                {
                    accountsPayableInvoicesEntity.VoucherRequestor
                };


            request.VendorBilledAmtCurrency = accountsPayableInvoicesEntity.CurrencyCode;

            request.VendorBilledAmtValue = accountsPayableInvoicesEntity.VoucherInvoiceAmt;

            request.PaymentStatus = accountsPayableInvoicesEntity.VoucherPayFlag == "Y";

            request.ByPassApprovalFlag = accountsPayableInvoicesEntity.ByPassVoucherApproval;

            request.PopulateTaxForm = accountsPayableInvoicesEntity.ByPassTaxForms;

            request.SubmittedBy = accountsPayableInvoicesEntity.SubmittedBy;

            var lineItems = new List<LineItems>();
            var lineItemAccountDetails = new List<LineItemAccountDetails>();

            if (accountsPayableInvoicesEntity.LineItems != null && accountsPayableInvoicesEntity.LineItems.Any())
            {
                foreach (var apLineItem in accountsPayableInvoicesEntity.LineItems)
                {
                    lineItemSequence++;

                    var lineItem = new LineItems()
                    {
                        LineItemDescription = apLineItem.Description
                    };
                    if (!string.IsNullOrEmpty(apLineItem.CommodityCode))
                    {
                        lineItem.LineItemsCommodityCode = apLineItem.CommodityCode;
                    }

                    lineItem.LineItemsQuantity = apLineItem.Quantity.ToString();

                    if (!string.IsNullOrEmpty(apLineItem.UnitOfMeasure))
                    {
                        lineItem.LineItemsUnitMeasured = apLineItem.UnitOfMeasure;
                    }

                    lineItem.LineItemsUnitPrice = apLineItem.Price;
                    lineItem.LineItemsDiscAmt = apLineItem.TradeDiscountAmount;

                    if (apLineItem.TradeDiscountPercent.HasValue)
                    {
                        lineItem.LineItemsDiscPercent = apLineItem.TradeDiscountPercent.ToString();
                    }

                    if (!string.IsNullOrEmpty(apLineItem.Comments))
                    {
                        lineItem.LineItemsComment = apLineItem.Comments;
                    }

                    if (apLineItem.AccountsPayableLineItemTaxes != null && apLineItem.AccountsPayableLineItemTaxes.Any())
                    {
                        var taxCodes = new List<string>();
                        foreach (var lineItemTaxes in apLineItem.AccountsPayableLineItemTaxes)
                        {
                            if (!string.IsNullOrEmpty(lineItemTaxes.TaxCode))
                                taxCodes.Add(lineItemTaxes.TaxCode);
                        }
                        lineItem.LineItemsTaxCode = string.Join("|", taxCodes);
                    }

                    //lineItem.LineItemBpoId = accountsPayableInvoicesEntity.BlanketPurchaseOrderId;
                    //lineItem.LineItemPoId = accountsPayableInvoicesEntity.PurchaseOrderId;
                    //lineItem.LineItemRcvsId = accountsPayableInvoicesEntity.RecurringVoucherId;

                    lineItem.LineItemPoId = apLineItem.PurchaseOrderId;
                    lineItem.LineItemBpoId = apLineItem.BlanketPurchaseOrderId;

                    lineItem.LineItemFinalPaymentFlag = apLineItem.FinalPaymentFlag;

                    lineItem.LineItemDocId = apLineItem.DocLineItemId;
                    //for BPO, there is no document id so we are passing in item id as doc id to the CTX as CTX is looking for it since PO has it. 
                    if (!string.IsNullOrEmpty(lineItem.LineItemBpoId) && lineItem.LineItemDocId == new Guid().ToString())
                        lineItem.LineItemDocId = "NEW";
                    lineItem.LineItemItemId = apLineItem.Id;

                    lineItems.Add(lineItem);


                    foreach (var glDistribution in apLineItem.GlDistributions)
                    {
                        var lineItemAccountDetail = new LineItemAccountDetails();

                        if (!string.IsNullOrEmpty(glDistribution.GlAccountNumber))
                            lineItemAccountDetail.LiaAccountingString = glDistribution.GlAccountNumber;

                        lineItemAccountDetail.LiaAmount = glDistribution.Amount;
                        lineItemAccountDetail.LiaPercentage = glDistribution.Percent;
                        lineItemAccountDetail.LiaQuantity = glDistribution.Quantity.ToString();
                        lineItemAccountDetail.LiaItemSequence = lineItemSequence.ToString();
                        lineItemAccountDetails.Add(lineItemAccountDetail);
                    }
                }

                if (lineItems != null && lineItems.Any())
                {
                    request.LineItems = lineItems;
                }

                if (lineItemAccountDetails != null && lineItemAccountDetails.Any())
                {
                    request.LineItemAccountDetails = lineItemAccountDetails;
                }
            }

            var i = 1;
            var lineItemSequenceNumbers = new List<string>();
            while (i <= lineItemSequence)
            {
                lineItemSequenceNumbers.Add(i.ToString());
                i++;
            }
            request.LineItemSequenceNumber = lineItemSequenceNumbers;

            return request;
        }

        /// <summary>
        ///  Build collection of AccountsPayableInvoices domain entities from collections of associated 
        ///  vouchers and persons
        /// </summary>
        /// <param name="vouchers">Collection of Vouchers data contracts</param>
        /// <param name="persons">Collection of Persons data contracts</param>
        /// <returns>AccountsPayableInvoices domain entity</returns>
        private async Task<IEnumerable<AccountsPayableInvoices>> BuildAccountsPayableInvoices(IEnumerable<Vouchers> vouchers,
            IEnumerable<Base.DataContracts.Person> persons)
        {
            var accountsPayableInvoicesCollection = new List<AccountsPayableInvoices>();

            foreach (var voucher in vouchers)
            {
                Base.DataContracts.Person person = null;
                if (!(string.IsNullOrWhiteSpace(voucher.VouVendor)))
                {
                    if (persons == null)
                    {
                        throw new ArgumentNullException("Expected person record for: " + voucher.Recordkey);
                    }
                    person = persons.FirstOrDefault(p => p.Recordkey == voucher.VouVendor);
                }
                //var vendor = vendors.FirstOrDefault(p => p.Recordkey == voucher.VouVendor);

                accountsPayableInvoicesCollection.Add(await BuildAccountsPayableInvoice(voucher, person)); //, vendor));
            }

            return accountsPayableInvoicesCollection.AsEnumerable();
        }

        /// <summary>
        /// Build AccountsPayableInvoices domain entity from associated voucher and person data contracts
        /// </summary>
        /// <param name="voucher">Voucher data contract</param>
        /// <param name="person">Person data contract</param>
        /// <returns>AccountsPayableInvoices domain entity</returns>
        private async Task<AccountsPayableInvoices> BuildAccountsPayableInvoice(Vouchers voucher,
            Ellucian.Colleague.Data.Base.DataContracts.Person person)
        {
            if (voucher == null)
            {
                throw new KeyNotFoundException(string.Format("Voucher record does not exist."));
            }

            // Translate the status code into a VoucherStatus enumeration value
            var voucherStatus = new VoucherStatus();
            DateTime? voucherStatusDate;

            var voucherStatusEntity = voucher.VoucherStatusEntityAssociation.FirstOrDefault();

            // Get the first status in the list of voucher statuses, and check that it has a value
            if (voucherStatusEntity != null)
            {
                voucherStatusDate = voucherStatusEntity.VouStatusDateAssocMember;
                var vouStatus = voucherStatusEntity.VouStatusAssocMember;
                if (vouStatus != null)
                {
                    switch (vouStatus.ToUpper())
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
            }
            else
            {
                throw new ApplicationException("Missing status for voucher: " + voucher.Recordkey);
            }
          

            if (!voucher.VouDate.HasValue)
            {
                throw new ApplicationException("Missing voucher date for voucher: " + voucher.Recordkey);
            }

            if ((string.IsNullOrEmpty(voucher.VouApType)) && (voucherStatus != VoucherStatus.Cancelled))
            {
                throw new ApplicationException("Missing AP type for voucher: " + voucher.Recordkey);
            }
            
            // exclude any LDM.GUID entries that contain a secondary key and/or index
            string criteria = "WITH LDM.GUID.ENTITY EQ 'VOUCHERS' AND LDM.GUID.PRIMARY.KEY = '" + voucher.Recordkey + "' AND LDM.GUID.SECONDARY.KEY EQ '' ";

            //retrive string array of applicable guids
            var guid = voucher.RecordGuid;
            var ldmGuidVoucher = await DataReader.SelectAsync("LDM.GUID", criteria);
            if ((ldmGuidVoucher != null) && (ldmGuidVoucher.Any()))
                guid = ldmGuidVoucher[0];

                var voucherDomainEntity = new AccountsPayableInvoices(guid, voucher.Recordkey, voucher.VouDate.Value, voucherStatus, "", voucher.VouDefaultInvoiceNo, voucher.VouDefaultInvoiceDate);

            voucherDomainEntity.Amount =  0;

            try
            {
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
                    var rcVouSchedule = await DataReader.ReadRecordAsync<RcVouSchedules>(voucher.VouRcvsId);
                    if (rcVouSchedule == null)
                    {
                        throw new KeyNotFoundException(string.Format("Recurring Voucher record {0} does not exist.", voucher.VouRcvsId));
                    }
                    voucherDomainEntity.RecurringVoucherId = rcVouSchedule.RcvsRcVoucher;
                }
            }
            catch (ApplicationException ae)
            {
                throw new ApplicationException("Voucher " + voucher.Recordkey + " has a problem with parent documents. " + ae.Message);
            }
            

            voucherDomainEntity.CurrencyCode = voucher.VouCurrencyCode;
            if (!string.IsNullOrEmpty(voucher.VouRequestor))
                voucherDomainEntity.VoucherRequestor = voucher.VouRequestor;
            voucherDomainEntity.HostCountry = await GetHostCountryAsync();

            voucherDomainEntity.VoucherStatusDate = voucherStatusDate;
            if (!string.IsNullOrEmpty(voucher.VouVendor))
                voucherDomainEntity.VendorId = voucher.VouVendor;

            voucherDomainEntity.VoucherPayFlag = voucher.VouPayFlag;
            voucherDomainEntity.VoucherInvoiceAmt = voucher.VouInvoiceAmt;
            voucherDomainEntity.VoucherDiscAmt = voucher.VouDiscAmt;

            if (!string.IsNullOrEmpty(voucher.VouVendorTerms))
                voucherDomainEntity.VoucherVendorTerms = voucher.VouVendorTerms;

            voucherDomainEntity.VoucherNet = voucher.VouNet;
            voucherDomainEntity.VoucherReferenceNo = voucher.VouReferenceNo;
            voucherDomainEntity.VoucherVoidGlTranDate = voucher.VouVoidGlTranDate;

            voucherDomainEntity.VoucherAddressId = voucher.VouAddressId;
            
            if ((person != null) && (!string.IsNullOrEmpty(person.PreferredAddress)))
                voucherDomainEntity.VendorAddressId = person.PreferredAddress;

            var voucherTaxes = new List<LineItemTax>();
            foreach (var tax in voucher.VouTaxesEntityAssociation)
            {
                if (!string.IsNullOrEmpty(tax.VouTaxCodesAssocMember))
                {
                    var voucherTax = new LineItemTax(tax.VouTaxCodesAssocMember, Convert.ToDecimal(tax.VouTaxAmtsAssocMember));

                    voucherTaxes.Add(voucherTax);
                }
            }
            voucherDomainEntity.VoucherTaxes = voucherTaxes;

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

            // Load in all MISC data fields for vouchers without any Vendor in Colleague.
            voucherDomainEntity.VoucherUseAltAddress = (!string.IsNullOrEmpty(voucher.VouAltFlag) && voucher.VouAltFlag.ToUpper() == "Y" ? true : false);
            voucherDomainEntity.VoucherMiscName = voucher.VouMiscName;
            voucherDomainEntity.VoucherMiscType = voucher.VouIntgCorpPersonInd;
            voucherDomainEntity.VoucherMiscAddress = voucher.VouMiscAddress;
            voucherDomainEntity.VoucherMiscCity = voucher.VouMiscCity;
            voucherDomainEntity.VoucherMiscState = voucher.VouMiscState;
            voucherDomainEntity.VoucherMiscZip = voucher.VouMiscZip;
            voucherDomainEntity.VoucherMiscCountry = voucher.VouMiscCountry;

            voucherDomainEntity.Comments = voucher.VouComments;

            //get submitted by
            if (!string.IsNullOrEmpty(voucher.VouIntgSubmittedBy))
                voucherDomainEntity.SubmittedBy = voucher.VouIntgSubmittedBy;

            var lineItemIds = voucher.VouItemsId;
            if (lineItemIds != null && lineItemIds.Count() > 0)
            {
                var lineItemRecords = await DataReader.BulkReadRecordAsync<Items>(lineItemIds.ToArray());


                foreach (var lineItem in lineItemRecords)
                {

                    //voucherDomainEntity.AddLineItem(lineItemDomainEntity);
                    var item = await GetLineItem(lineItem, voucher.VouCurrencyCode);
                    if (item != null)
                    {
                        voucherDomainEntity.AddAccountsPayableInvoicesLineItem(item.Item1);
                        voucherDomainEntity.Amount += item.Item2;
                    }
                }
            }
            return voucherDomainEntity;
        }

        /// <summary>
        /// Get AccountsPayableInvoicesLineItem
        /// </summary>
        /// <param name="lineItem">Items data contract</param>
        /// <param name="currencyCode">line amount subtotal including tax</param>
        /// <returns>Tuple containing an AccountsPayableInvoicesLineItem and amout subtotal</returns>
        private async Task<Tuple<AccountsPayableInvoicesLineItem, decimal>> GetLineItem(Items lineItem, string currencyCode)
        {
            // The item description is a list of strings                     
            decimal amount = 0;
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
            if (string.IsNullOrEmpty(itemDescription))
            {
                itemDescription = "Unknown";
            }

            decimal itemQuantity = lineItem.ItmVouQty ?? 0;
            decimal itemPrice = lineItem.ItmVouPrice ?? 0;
            decimal itemExtendedPrice = lineItem.ItmVouExtPrice ?? 0;
            var lineItemDomainEntity = new AccountsPayableInvoicesLineItem(lineItem.Recordkey, itemDescription, itemQuantity, itemPrice, itemExtendedPrice);
            lineItemDomainEntity.UnitOfIssue = lineItem.ItmVouIssue;
            lineItemDomainEntity.InvoiceNumber = lineItem.ItmInvoiceNo;
            lineItemDomainEntity.TaxForm = lineItem.ItmTaxForm;
            lineItemDomainEntity.TaxFormCode = lineItem.ItmTaxFormCode;
            lineItemDomainEntity.TaxFormLocation = lineItem.ItmTaxFormLoc;
            lineItemDomainEntity.Comments = lineItem.ItmComments;
            lineItemDomainEntity.CommodityCode = lineItem.ItmCommodityCode;
            lineItemDomainEntity.PurchaseOrderId = lineItem.ItmPoId;

            lineItemDomainEntity.CashDiscountAmount = lineItem.ItmVouCashDiscAmt;
            lineItemDomainEntity.TradeDiscountAmount = lineItem.ItmVouTradeDiscAmt;
            lineItemDomainEntity.TradeDiscountPercent = lineItem.ItmVouTradeDiscPct;

             if ((lineItem.VouchGlEntityAssociation != null) && (lineItem.VouchGlEntityAssociation.Count > 0))
            {
                var distrProjects = new List<string>();
                var distrProjectLineItems = new List<string>();

                foreach (var glDistr in lineItem.VouchGlEntityAssociation)
                {
                    if (!string.IsNullOrEmpty(glDistr.ItmVouGlNoAssocMember))
                    {
                        decimal gldistGlAmount = 0;

                        if (string.IsNullOrEmpty(currencyCode)) 
                        {
                            gldistGlAmount = glDistr.ItmVouGlAmtAssocMember ?? 0;
                        }
                        else
                        {
                            gldistGlAmount = glDistr.ItmVouGlForeignAmtAssocMember ?? 0;
                        }

                        LineItemGlDistribution glDistribution = new LineItemGlDistribution(glDistr.ItmVouGlNoAssocMember,
                           glDistr.ItmVouGlQtyAssocMember ?? 0,
                           gldistGlAmount,
                           glDistr.ItmVouGlPctAssocMember ?? 0);


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

                        if (string.IsNullOrEmpty(currencyCode)) //voucher.VouCurrencyCode))
                        {
                            amount += glDistr.ItmVouGlAmtAssocMember ?? 0;
                        }
                        else
                        {
                            amount += glDistr.ItmVouGlForeignAmtAssocMember ?? 0;
                        }
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

                    if (!string.IsNullOrEmpty(taxGlDistr.ItmVouGlTaxCodeAssocMember))
                    {
                        LineItemTax itemTax = new LineItemTax(taxGlDistr.ItmVouGlTaxCodeAssocMember,
                            itemTaxAmount ?? 0)
                        {
                            TaxGlNumber = taxGlDistr.ItmVouTaxGlNoAssocMember,
                            LineGlNumber = taxGlDistr.ItmVouLineGlNoAssocMember
                        };

                        lineItemDomainEntity.AccountsPayableLineItemTaxes.Add(itemTax);
                    }
                   
                    if (string.IsNullOrEmpty(currencyCode)) 
                    {
                        amount += taxGlDistr.ItmVouGlTaxAmtAssocMember ?? 0;
                    }
                    else
                    {
                        amount += taxGlDistr.ItmVouGlForeignTaxAmtAssocMember ?? 0;
                    }
                }
            }

            return new Tuple<AccountsPayableInvoicesLineItem, decimal>(lineItemDomainEntity, amount);
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
        /// Get the Guid of an entity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> GetGuidFromID(string key, string entity)
        {
            try
            {   
                return await GetGuidFromRecordInfoAsync(entity, key);
            }
            catch (RepositoryException REX)
            {
                REX.AddError(new RepositoryError(entity + ".guid.NotFound", "GUID not found for " + entity + " id " + key));
                throw REX;
            }

        }

        /// <summary>
        /// Get ID from a guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<GuidLookupResult> GetIdFromGuidAsync(string guid)
        {
            try
            {
                return await GetRecordInfoFromGuidAsync(guid);               
            }
            catch (RepositoryException REX)
            {
                REX.AddError(new RepositoryError("Guid "+guid+" is not found"));
                throw REX;
            }
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
    }
}