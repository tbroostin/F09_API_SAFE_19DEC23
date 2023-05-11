// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
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
        // setting for Cache
        const string AllAccountsPayableInvoicesCache = "AllAccountsPayableInvoices";
        const int AllAccountsPayableInvoicesCacheTimeout = 20; // Clear from cache every 20 minutes
        private RepositoryException exception = new RepositoryException();

        /// <summary>
        /// This constructor allows us to instantiate a AccountsPayableInvoices repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public AccountsPayableInvoicesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get AccountsPayableInvoices Domain  Entity
        /// Will not support vouchers that are paying student refunds or employee payroll related refunds, advances.
        /// Does now support invoices without VOU.VENDOR for V11 EEDM accounts-payable-invoices.
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<AccountsPayableInvoices>, int>> GetAccountsPayableInvoices2Async(int offset, int limit, string invoiceNumber)
        {
            int totalCount = 0;
            string[] subList = null;
            // We need to exclude any AP Types where the source does not equal Regular Accounts Payable
            // (ie - exclude Accounts Receivable and PayrollTax.Deductions)
            var apTypesToInclude = string.Empty;
            var apTypes = await GetValidApTypes();
            if (apTypes != null || apTypes.Any())
            {
                foreach (var type in apTypes)
                {
                    apTypesToInclude = string.Concat(apTypesToInclude, "'", type, "'");
                }
            }        
            else
            {
                return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices>(), totalCount);
            }

            try
            {
                //if there is a filter then we read that record and check to make sure it is a good one. 
                if (!string.IsNullOrEmpty(invoiceNumber))
                {
                    var voucher = await DataReader.ReadRecordAsync<DataContracts.Vouchers>(invoiceNumber);
                    if (voucher == null)
                    {
                        return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices>(), totalCount);
                    }
                    else
                    {
                        if (voucher.VouStatus.Contains("X"))
                        {
                            return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices>(), totalCount);
                        }

                        // exclude those Vouchers that are inprogress
                        if (voucher != null && voucher.VouStatus != null && voucher.VouStatus.Any() && voucher.VouStatus.FirstOrDefault().Equals("U", StringComparison.OrdinalIgnoreCase))
                        {
                            return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices>(), totalCount);
                        }
                        // if voucher does not have line items, then it is not valid
                        IEnumerable<Items> itemsData = null;
                        if (voucher.VouItemsId != null && voucher.VouItemsId.Any())
                        {
                            itemsData = await DataReader.BulkReadRecordAsync<Items>(voucher.VouItemsId.ToArray());
                            if (itemsData == null)
                            {
                                exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Items record {0} does not exist.", string.Join(",", voucher.VouItemsId)), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                                {
                                    Id = voucher.RecordGuid,
                                    SourceId = voucher.Recordkey,
                                });
                            }
                        }
                        else
                        {
                            return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices>(), totalCount);
                        }

                        if ((apTypes != null) && (!(apTypes.ToList().Contains(voucher.VouApType))))
                        {
                            return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices>(), totalCount);
                        }
                        Ellucian.Colleague.Data.Base.DataContracts.Person person = null;
                        if (!string.IsNullOrEmpty(voucher.VouVendor))
                        {
                            person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", voucher.VouVendor);
                            if (person == null)
                            {
                                exception.AddError(new RepositoryError("Bad.Data", "Person Id " + voucher.VouVendor + " is not returning any data. Person may be corrupted.")
                                {
                                    Id = voucher.RecordGuid,
                                    SourceId = voucher.Recordkey,
                                });
                            }
                        }
                        PurchaseOrders purchaseOrder = null;
                        if (!string.IsNullOrEmpty(voucher.VouPoNo))
                        {
                            purchaseOrder = await DataReader.ReadRecordAsync<PurchaseOrders>(voucher.VouPoNo);
                            if (purchaseOrder == null)
                            {
                                exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Purchase Order record {0} does not exist.", voucher.VouPoNo), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                                {
                                    Id = voucher.RecordGuid,
                                    SourceId = voucher.Recordkey,
                                });
                            }
                        }
                        RcVouSchedules recurringVoucher = null;
                        if (!string.IsNullOrEmpty(voucher.VouRcvsId))
                        {
                            recurringVoucher = await DataReader.ReadRecordAsync<RcVouSchedules>(voucher.VouRcvsId);
                            if (recurringVoucher == null)
                            {
                                exception.AddError(new RepositoryError("Bad.Data", (string.Concat(string.Format("Recurring Voucher record {0} does not exist.", voucher.VouRcvsId), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'")))
                                {
                                    Id = voucher.RecordGuid,
                                    SourceId = voucher.Recordkey,
                                });
                            }
                        }
                        var accountsPayableInvoice = await BuildAccountsPayableInvoice(voucher, person, purchaseOrder, itemsData, recurringVoucher);
                        if (exception != null && exception.Errors != null && exception.Errors.Any())
                        {
                            throw exception;
                        }

                        if (accountsPayableInvoice != null)
                        {
                            return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices> { accountsPayableInvoice }, 1);
                        }
                        else
                        {
                            return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices>(), totalCount);
                        }
                    }
                }
                else
                {
                    string AccountsPayableInvoicesCacheKey = CacheSupport.BuildCacheKey(AllAccountsPayableInvoicesCache);
                    var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                        this,
                        ContainsKey,
                        GetOrAddToCacheAsync,
                        AddOrUpdateCacheAsync,
                        transactionInvoker,
                        AccountsPayableInvoicesCacheKey,
                        "VOUCHERS",
                        offset,
                        limit,
                        AllAccountsPayableInvoicesCacheTimeout,
                        async () =>
                        {
                            CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                            {
                                criteria = string.Concat("WITH VOU.ITEMS.ID NE '' AND WITH VOU.AP.TYPE EQ ", apTypesToInclude, " AND WITH VOU.CURRENT.STATUS NE 'X' AND WITH VOU.CURRENT.STATUS NE 'U'")
                            };
                            return requirements;
                        });
                    if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
                    {
                        return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(new List<AccountsPayableInvoices>(), totalCount);
                    }

                    totalCount = keyCacheObject.TotalCount.Value;
                    subList = keyCacheObject.Sublist.ToArray();
                    IEnumerable<AccountsPayableInvoices> accountsPayableInvoices = null;
                    try
                    {
                        var voucherData = await DataReader.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", subList);
                        var personSubList = voucherData.Where(x => !string.IsNullOrEmpty(x.VouVendor)).Select(c => c.VouVendor).Distinct().ToArray();
                        var personsData = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", personSubList);
                        var purchaseOrdersSubList = voucherData.Where(x => !string.IsNullOrEmpty(x.VouPoNo)).Select(c => c.VouPoNo).Distinct().ToArray();
                        IEnumerable<PurchaseOrders> purchaseOrdersData = null;
                        if (purchaseOrdersSubList != null && purchaseOrdersSubList.Any())
                            purchaseOrdersData = await DataReader.BulkReadRecordAsync<PurchaseOrders>("PURCHASE.ORDERS", purchaseOrdersSubList);
                        var recurringVoucherSubList = voucherData.Where(x => !string.IsNullOrEmpty(x.VouRcvsId)).Select(c => c.VouRcvsId).Distinct().ToArray();
                        IEnumerable<RcVouSchedules> recurringVoucherData = null;
                        if (recurringVoucherSubList != null && recurringVoucherSubList.Any())
                            recurringVoucherData = await DataReader.BulkReadRecordAsync<RcVouSchedules>("RC.VOU.SCHEDULES", recurringVoucherSubList);
                        var itemsSubList = voucherData.Where(x => x.VouItemsId != null && x.VouItemsId.Any()).SelectMany(c => c.VouItemsId).Distinct().ToArray();
                        IEnumerable<Items> itemsData = null;
                        if (itemsSubList != null && itemsSubList.Any())
                            itemsData = await DataReader.BulkReadRecordAsync<Items>("ITEMS", itemsSubList);
                        accountsPayableInvoices = await BuildAccountsPayableInvoices(voucherData, personsData, purchaseOrdersData, itemsData, recurringVoucherData);
                    }
                    catch (Exception ex)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                    }

                    if (exception != null && exception.Errors != null && exception.Errors.Any())
                    {
                        throw exception;
                    }
                    return new Tuple<IEnumerable<AccountsPayableInvoices>, int>(accountsPayableInvoices, totalCount);
                }

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw new RepositoryException(e.Message);  
            }            
        }


        /// <summary>
        /// returns a string of valid AP types
        /// </summary>
        /// <returns>list of valid AP Types for vouchers</returns>
        private async Task<List<string>> GetValidApTypes()
        {
            var apTypes = new List<string>();
            apTypes = await GetOrAddToCacheAsync<List<string>>("AllAccountsPayableSourcesValidAPTypes",
               async () =>
               {
                   apTypes = (await DataReader.SelectAsync("AP.TYPES", "WITH APT.SOURCE EQ 'R'")).ToList();                   
                   return apTypes;
                }, Level1CacheTimeoutValue);
            return apTypes;
        }


        /// <summary>
        /// Get a single voucher using a GUID
        /// </summary>
        /// <param name="guid">The voucher guid</param>
        /// <returns>The AccountsPayableInvoices domain entity</returns>
        public async Task<AccountsPayableInvoices> GetAccountsPayableInvoicesByGuidAsync(string guid, bool allowInProgress)
        {
            try
            {
                string id = await GetAccountsPayableInvoicesIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException("No accounts-payable-invoices was found for guid " + guid);
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
                    exception.AddError(new RepositoryError("Bad.Data", "The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS has been cancelled and is not valid for accounts-payable-invoices.")
                    {
                        Id = voucher.RecordGuid, 
                        SourceId = voucher.Recordkey,
                    });
                }
                IEnumerable<Items> itemsData = null;
                if (voucher.VouItemsId == null || !voucher.VouItemsId.Any())
                {
                    exception.AddError(new RepositoryError("Bad.Data", "The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS has no line items and is not valid for accounts-payable-invoices.")
                    {
                        Id = voucher.RecordGuid,
                        SourceId = voucher.Recordkey,
                    });
                }
                else
                {
                    itemsData = await DataReader.BulkReadRecordAsync<Items>(voucher.VouItemsId.ToArray());
                    if (itemsData == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Items record {0} does not exist.", string.Join(",", voucher.VouItemsId)), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
                    }
                    else
                    {
                        // exclude those Vouchers that are inprogress
                        if (!allowInProgress && voucher != null && voucher.VouStatus != null && voucher.VouStatus.Any() && voucher.VouStatus.FirstOrDefault().Equals("U", StringComparison.OrdinalIgnoreCase))
                        {
                            var awaitingReceipt = itemsData != null && itemsData.Any(iData => !string.IsNullOrEmpty(iData.ItmPoId) && iData.ItmPoStatus != null && iData.ItmPoStatus.Any() && iData.ItmPoStatus.FirstOrDefault().Equals("H", StringComparison.OrdinalIgnoreCase));
                            if (awaitingReceipt)
                            {
                                exception.AddError(new RepositoryError("Bad.Data", "The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS has one or more line items with an 'awaitingReceipt' status and is not valid for accounts-payable-invoices.")
                                {
                                    Id = voucher.RecordGuid,
                                    SourceId = voucher.Recordkey,
                                });
                            }
                            else
                            {
                                exception.AddError(new RepositoryError("Bad.Data", "The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS has an 'inProgress' status and is not valid for accounts-payable-invoices.")
                                {
                                    Id = voucher.RecordGuid,
                                    SourceId = voucher.Recordkey,
                                });
                            }
                        }
                    }

                }

                

                Ellucian.Colleague.Data.Base.DataContracts.Person person = null;
                if (!string.IsNullOrEmpty(voucher.VouVendor))
                {
                    person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", voucher.VouVendor);
                    if (person == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", "Person Id " + voucher.VouVendor + " is not returning any data. Person may be corrupted.")
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
                    }
                }
                PurchaseOrders purchaseOrder = null;
                if (!string.IsNullOrEmpty(voucher.VouPoNo))
                {
                    purchaseOrder = await DataReader.ReadRecordAsync<PurchaseOrders>(voucher.VouPoNo);
                    if (purchaseOrder == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Purchange Order record {0} does not exist.", voucher.VouPoNo), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
                    }
                }
                RcVouSchedules recurringVoucher = null;
                if (!string.IsNullOrEmpty(voucher.VouRcvsId))
                {
                    recurringVoucher = await DataReader.ReadRecordAsync<RcVouSchedules>(voucher.VouRcvsId);
                    if (recurringVoucher == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Recurring Voucher record {0} does not exist.", voucher.VouRcvsId), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
                    }
                }
                var apTypesToInclude = await GetValidApTypes();

                if ((apTypesToInclude != null) && (!(apTypesToInclude.ToList().Contains(voucher.VouApType))))
                {
                    exception.AddError(new RepositoryError("Bad.Data", "The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS is not valid for accounts-payable-invoices.")
                    {
                        Id = voucher.RecordGuid,
                        SourceId = voucher.Recordkey,
                    });
                }
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
                accountsPayableInvoice = await BuildAccountsPayableInvoice(voucher, person, purchaseOrder, itemsData, recurringVoucher);
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
                return accountsPayableInvoice;
            }
            catch (KeyNotFoundException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
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
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("No accounts-payable-invoices was found for guid " + guid);
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("No accounts-payable-invoices was found for guid " + guid);
            }

            if (foundEntry.Value.Entity != "VOUCHERS")
            {
                exception.AddError(new RepositoryError("GUID.Wrong.Type", string.Format("GUID {0} has different entity, {1}, than expected, VOUCHERS", guid, foundEntry.Value.Entity))
                {
                    Id = guid                   
                });
                throw exception;
                
            }

            return foundEntry.Value.PrimaryKey;
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
            if (!string.IsNullOrEmpty(accountsPayableInvoicesEntity.Type))
                request.VouType = accountsPayableInvoicesEntity.Type;
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
                    if (!string.IsNullOrEmpty(apLineItem.FixedAssetsFlag))
                    {
                        lineItem.LineItemsFixedAssetsFlag = apLineItem.FixedAssetsFlag;
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
                        var taxCodeAmts = new List<string>();
                        foreach (var lineItemTaxes in apLineItem.AccountsPayableLineItemTaxes)
                        {
                            if (!string.IsNullOrEmpty(lineItemTaxes.TaxCode))
                            {
                                taxCodes.Add(lineItemTaxes.TaxCode);
                                taxCodeAmts.Add(lineItemTaxes.TaxAmount.ToString());
                            }
                        }
                        lineItem.LineItemsTaxCode = string.Join("|", taxCodes);
                        lineItem.LineItemsTaxAmt = string.Join("|", taxCodeAmts);
                    }

                    //lineItem.LineItemBpoId = accountsPayableInvoicesEntity.BlanketPurchaseOrderId;
                    //lineItem.LineItemPoId = accountsPayableInvoicesEntity.PurchaseOrderId;
                    //lineItem.LineItemRcvsId = accountsPayableInvoicesEntity.RecurringVoucherId;

                    lineItem.LineItemPoId = apLineItem.PurchaseOrderId;
                    lineItem.LineItemBpoId = apLineItem.BlanketPurchaseOrderId;

                    lineItem.LineItemFinalPaymentFlag = apLineItem.FinalPaymentFlag;
                    lineItem.LineItemTaxForm = apLineItem.TaxForm;
                    lineItem.LineItemTaxFormBox = apLineItem.TaxFormCode;
                    lineItem.LineItemTaxFormLoc = apLineItem.TaxFormLocation;

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
            IEnumerable<Base.DataContracts.Person> persons, IEnumerable<PurchaseOrders> purchaseOrders, IEnumerable<Items> items, IEnumerable<RcVouSchedules> recurringVouchers)
        {
            var accountsPayableInvoicesCollection = new List<AccountsPayableInvoices>();

            foreach (var voucher in vouchers)
            {
                Base.DataContracts.Person person = null;
                if (!(string.IsNullOrWhiteSpace(voucher.VouVendor)))
                {
                    if (persons == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Person record { 0 } does not exist.", voucher.VouVendor), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
                    }
                    else
                    {
                        person = persons.FirstOrDefault(p => p.Recordkey == voucher.VouVendor);
                        if (person == null)
                        {
                            exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Person record { 0 } does not exist.", voucher.VouVendor), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                            {
                                Id = voucher.RecordGuid,
                                SourceId = voucher.Recordkey,
                            });
                        }
                    }
                }
                PurchaseOrders purchaseOrder = null;
                if (!string.IsNullOrEmpty(voucher.VouPoNo))
                {
                    if (purchaseOrders == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Purchase Order record {0} does not exist.", voucher.VouPoNo), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
                    }
                    else
                    {
                        purchaseOrder = purchaseOrders.FirstOrDefault(p => p.Recordkey == voucher.VouPoNo);
                        if (purchaseOrder == null)
                        {
                            exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Purchase Order record {0} does not exist.", voucher.VouPoNo), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                            {
                                Id = voucher.RecordGuid,
                                SourceId = voucher.Recordkey,
                            });
                        }
                    }
                }
                RcVouSchedules recurringVoucher = null;
                if (!string.IsNullOrEmpty(voucher.VouRcvsId))
                {
                    if (recurringVouchers == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Recurring voucher record {0} does not exist.", voucher.VouRcvsId), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
                    }
                    else
                    {
                        recurringVoucher = recurringVouchers.FirstOrDefault(p => p.Recordkey == voucher.VouRcvsId);
                        if (recurringVoucher == null)
                        {
                            exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Recurring voucher record {0} does not exist.", voucher.VouRcvsId), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                            {
                                Id = voucher.RecordGuid,
                                SourceId = voucher.Recordkey,
                            });
                        }
                    }
                }

                IEnumerable<Items> itemsData = null;
                if (voucher.VouItemsId != null && voucher.VouItemsId.Any())
                {
                    if (items == null)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Items records {0} does not exist.", string.Join(",", voucher.VouItemsId)), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
                    }
                    else
                    {
                        itemsData = items.Where(p => voucher.VouItemsId.Contains(p.Recordkey));
                        if (itemsData == null)
                        {
                            exception.AddError(new RepositoryError("Bad.Data", string.Concat(string.Format("Items record {0} does not exist.", string.Join(",", voucher.VouItemsId)), " Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                            {
                                Id = voucher.RecordGuid,
                                SourceId = voucher.Recordkey,
                            });
                        }
                    }
                }
                else
                {
                    exception.AddError(new RepositoryError("Bad.Data", string.Concat("Missing valid Items records.", "  Entity: 'VOUCHERS', Record ID: '", voucher.Recordkey, "'"))
                    {
                        Id = voucher.RecordGuid,
                        SourceId = voucher.Recordkey,
                    });
                }
                var voucherEntity = await BuildAccountsPayableInvoice(voucher, person, purchaseOrder, itemsData, recurringVoucher);
                if (voucherEntity != null)
                accountsPayableInvoicesCollection.Add(voucherEntity);
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
            Ellucian.Colleague.Data.Base.DataContracts.Person person,
            PurchaseOrders purchaseOrder,
            IEnumerable<Items> lineItemRecords,
            RcVouSchedules recurringVoucher)
        {
            // Translate the status code into a VoucherStatus enumeration value
            var voucherStatus = new VoucherStatus();
            DateTime? voucherStatusDate = new DateTime() ;

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
                            exception.AddError(new RepositoryError("Bad.Data", "Invalid voucher status for voucher: " + voucher.Recordkey)
                            {
                                Id = voucher.RecordGuid,
                                SourceId = voucher.Recordkey,
                            });
                            break;

                    }
                }
            }
            else
            {
                exception.AddError(new RepositoryError( "Bad.Data", "Missing status for voucher: " + voucher.Recordkey)
                {
                    Id = voucher.RecordGuid,
                    SourceId = voucher.Recordkey,
                });
            }


            if (!voucher.VouDate.HasValue)
            {
                exception.AddError(new RepositoryError( "Bad.Data", "Missing voucher date for voucher: " + voucher.Recordkey)
                {
                    Id = voucher.RecordGuid,
                    SourceId = voucher.Recordkey,
                });
            }

            if ((string.IsNullOrEmpty(voucher.VouApType)) && (voucherStatus != VoucherStatus.Cancelled))
            {
                exception.AddError(new RepositoryError( "Bad.Data", "Missing AP type for voucher: " + voucher.Recordkey)
                {
                    Id = voucher.RecordGuid,
                    SourceId = voucher.Recordkey,
                });
            }
            AccountsPayableInvoices voucherDomainEntity = null;
            //retrive string array of applicable guids
            if (string.IsNullOrEmpty(voucher.RecordGuid))
            {
                exception.AddError(new RepositoryError("GUID.Not.Found", "Unable to find the GUID for accounts-payable-invoices.")
                {
                    Id = voucher.RecordGuid,
                    SourceId = voucher.Recordkey,
                });
            }
            else
            {
                var guid = voucher.RecordGuid;                
                try
                {
                    voucherDomainEntity = new AccountsPayableInvoices(guid, voucher.Recordkey, voucher.VouDate.Value, voucherStatus, "", voucher.VouDefaultInvoiceNo, voucher.VouDefaultInvoiceDate);

                }
                catch (Exception ex)
                {
                    exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = voucher.RecordGuid,
                        SourceId = voucher.Recordkey,
                    });
                }
            }

            if (voucherDomainEntity != null)
            {
                voucherDomainEntity.Amount = 0;
                if (!string.IsNullOrEmpty(voucher.VouPoNo))
                {
                    voucherDomainEntity.PurchaseOrderId = voucher.VouPoNo;
                    // if there is a PO, then we need to get PO.INTG.TYPE to populate the type attribute
                    if (purchaseOrder != null)
                    {
                        voucherDomainEntity.Type = purchaseOrder.PoIntgType;
                    }
                }
                if (!string.IsNullOrEmpty(voucher.VouBpoId))
                {
                    voucherDomainEntity.BlanketPurchaseOrderId = voucher.VouBpoId;
                }

                if (!string.IsNullOrEmpty(voucher.VouRcvsId))
                {
                    if (recurringVoucher != null)
                        voucherDomainEntity.RecurringVoucherId = recurringVoucher.RcvsRcVoucher;
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
                //display the discount amount only if it is manualiis
                if (voucher.VouManualCashDisc == "Y")
                {
                    voucherDomainEntity.VoucherDiscAmt = voucher.VouDiscAmt;
                }

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

                if (lineItemRecords != null && lineItemRecords.Any())
                {
                    foreach (var lineItem in lineItemRecords)
                    {
                        try
                        {
                            var item = await GetLineItem(lineItem, voucher.VouCurrencyCode);
                            if (item != null)
                            {
                                voucherDomainEntity.AddAccountsPayableInvoicesLineItem(item.Item1);
                                voucherDomainEntity.Amount += item.Item2;
                            }
                        }
                        catch(Exception ex)
                        {
                            exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                            {
                                Id = voucher.RecordGuid,
                                SourceId = voucher.Recordkey,
                            });
                        }
                    }
                    //if there is no line items then we want to throw an exception
                    if (voucherDomainEntity.LineItems == null || !voucherDomainEntity.LineItems.Any())
                    {
                        exception.AddError(new RepositoryError("Bad.Data", string.Concat("Voucher contains invalid line items with id ", string.Join(",", voucher.VouItemsId), " .  Entity: 'VOUCHERS', Record ID: '", voucherDomainEntity.Id, "'"))
                        {
                            Id = voucher.RecordGuid,
                            SourceId = voucher.Recordkey,
                        });
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
            lineItemDomainEntity.FixedAssetsFlag = lineItem.ItmFixedAssetsFlag;
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
                            itemTaxAmount)
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
        /// Using a collection of purchase-order guids, 
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            if (string.IsNullOrEmpty(filename))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();

            try
            {
                var guidLookup = ids
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Distinct().ToList()
                   .ConvertAll(p => new RecordKeyLookup(filename, p, false)).ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
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