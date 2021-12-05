// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
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
    /// This class implements the IPaymentTransactionsRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PaymentTransactionsRepository : BaseColleagueRepository, IPaymentTransactionsRepository
    {

        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        protected const string AllPaymentTransactionsFilterCache = "AllPaymentTransactionsFilter";
        protected const int AllPaymentTransactionsCacheTimeout = 20; // Clear from cache every 20 minutes
        RepositoryException exception = null;

        /// <summary>
        /// This constructor allows us to instantiate a PaymentTransactions repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public PaymentTransactionsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            exception = new RepositoryException();
        }

        /// <summary>
        /// Get PaymentTransactions Domain Entity
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <param name="documentId">Id for VOUCHER</param>
        /// <param name="documentTypeValue">invoice or refund</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<PaymentTransaction>, int>> GetPaymentTransactionsAsync(
            int offset, int limit, string documentId, InvoiceOrRefund invoiceOrRefund, string docNumber, List<string> refPoDoc, List<string> refBpoDoc, List<string> refRecDoc)
        {
            try
            {
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
                    return new Tuple<IEnumerable<PaymentTransaction>, int>(new List<PaymentTransaction>(), 0);
                }

                int totalCount = 0;
                string[] subList = null;
                string paymentCacheKey = CacheSupport.BuildCacheKey(AllPaymentTransactionsFilterCache, documentId, invoiceOrRefund.ToString(), docNumber, refPoDoc, refBpoDoc, refRecDoc);
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       paymentCacheKey,
                       string.Empty,
                       offset,
                       limit,
                       AllPaymentTransactionsCacheTimeout,
                       async () =>
                       {
                           var checkCriteria = string.Concat("WITH CHK.AP.TYPE = ", apTypesToInclude);
                          // var voucherCriteria = "WITH VOU.PMT.TXN.INTG.IDX NE ''";
                           var voucherCriteria = string.Concat("WITH VOU.PMT.TXN.INTG.IDX NE '' AND WITH VOU.AP.TYPE EQ ", apTypesToInclude);
                          
                           var checkIds = new List<string>();
                           var voucherIds = new List<string>();
                           var checksLimitingKeys = new List<string>();
                           var voucherLimitingKeys = new List<string>();
                           var checkId = string.Empty;
                           //documentId and type filter
                           if ((!string.IsNullOrEmpty(documentId)) && (invoiceOrRefund != InvoiceOrRefund.NotSet))
                           {
                               if (invoiceOrRefund == InvoiceOrRefund.Invoice)
                               {
                                   checkCriteria = string.Format("{0} AND WITH CHK.VOUCHERS.IDS = '{1}'", checkCriteria, documentId);

                               }
                               else if (invoiceOrRefund == InvoiceOrRefund.Refund)
                               {
                                   voucherCriteria = string.Format("{0} AND WITH VOUCHERS.ID = '{1}'", voucherCriteria, documentId);

                               }
                           }
                           //document Number Filter
                           if (!string.IsNullOrEmpty(docNumber))
                           {
                               checkId = docNumber;
                               checksLimitingKeys.Add(docNumber);
                               voucherLimitingKeys.Add(docNumber);
                           }
                           // PO Id filter
                           if ((refPoDoc != null && refPoDoc.Any()))
                           {
                               voucherLimitingKeys = new List<string>() { "x" };
                               //check criteria
                               var posCri = string.Format("WITH VOU.PO.NO EQ {0}", string.Join(" ", refPoDoc.Select(a => string.Format("'{0}'", a))));
                               //var posCri = string.Concat("WITH VOU.PO.NO EQ ", ConvertArrayToString(refPoDoc));
                               var poVouchers = (await DataReader.SelectAsync("VOUCHERS", posCri));
                               var poCheckIds = new List<string>();
                               if (!string.IsNullOrEmpty(checkId))
                                   poCheckIds = (await DataReader.SelectAsync("CHECKS", string.Concat("WITH CHECKS.ID EQ '", checkId, "' AND WITH CHK.VOUCHERS.IDS EQ '?'"), poVouchers)).ToList();
                               else
                                   poCheckIds = (await DataReader.SelectAsync("CHECKS", "WITH CHK.VOUCHERS.IDS EQ '?'", poVouchers)).ToList();
                               if (poCheckIds != null && poCheckIds.Any())
                               {
                                   checksLimitingKeys.AddRange(poCheckIds);
                               }
                               else
                               {
                                   //we need make the limit keys invalid. we cannot clear it as it will select all. 
                                   checksLimitingKeys = new List<string>() { "x" };                 
                               }
                           }

                           // BPO Id filter
                           if ((refBpoDoc != null && refBpoDoc.Any()))
                           {
                               voucherLimitingKeys = new List<string>() { "x" };
                               //check criteria
                               var bposCri = string.Format("WITH VOU.BPO.ID EQ {0}", string.Join(" ", refBpoDoc.Select(a => string.Format("'{0}'", a))));
                               //var bposCri = string.Concat("WITH VOU.BPO.ID EQ ", ConvertArrayToString(refBpoDoc));
                               var bpoVouchers = (await DataReader.SelectAsync("VOUCHERS", bposCri));

                               var bpoCheckIds = new List<string>();
                               if (!string.IsNullOrEmpty(checkId))
                                   bpoCheckIds = (await DataReader.SelectAsync("CHECKS", string.Concat("WITH CHECKS.ID EQ '", checkId, "' AND WITH CHK.VOUCHERS.IDS EQ '?'"), bpoVouchers)).ToList();
                               else
                                   bpoCheckIds = (await DataReader.SelectAsync("CHECKS", "WITH CHK.VOUCHERS.IDS EQ '?'", bpoVouchers)).ToList();                            
                               if (bpoCheckIds != null && bpoCheckIds.Any())
                               {
                                   checksLimitingKeys.AddRange(bpoCheckIds);
                               }
                               else
                               {
                                   //we need make the limit keys invalid. we cannot clear it as it will select all. 
                                   checksLimitingKeys = new List<string>() { "x" };
                               }
                           }

                           // recurring voucher filter
                           if ((refRecDoc != null && refRecDoc.Any()))
                           {
                               voucherLimitingKeys = new List<string>() { "x" };
                               //check criteria
                               var recVouCri = string.Format("WITH VOU.RCVS.ID EQ {0}", string.Join(" ", refRecDoc.Select(a => string.Format("'{0}'", a))));
                               //var recVouCri = string.Concat("WITH VOU.RCVS.ID EQ ", ConvertArrayToString(refRecDoc));
                               var recVouVouchers = (await DataReader.SelectAsync("VOUCHERS", recVouCri));
                               var recVouCheckIds = new List<string>();
                               if (!string.IsNullOrEmpty(checkId))
                                   recVouCheckIds = (await DataReader.SelectAsync("CHECKS", string.Concat("WITH CHECKS.ID EQ '", checkId, "' AND WITH CHK.VOUCHERS.IDS EQ '?'"), recVouVouchers)).ToList();
                               else
                                   recVouCheckIds = (await DataReader.SelectAsync("CHECKS", "WITH CHK.VOUCHERS.IDS EQ '?'", recVouVouchers)).ToList();
                               if (recVouCheckIds != null && recVouCheckIds.Any())
                               {
                                   checksLimitingKeys.AddRange(recVouCheckIds);
                               }
                               else
                               {
                                   //we need make the limit keys invalid. we cannot clear it as it will select all. 
                                   checksLimitingKeys = new List<string>() { "x" };
                               }
                           }
                           checkIds = (await DataReader.SelectAsync("CHECKS", checksLimitingKeys.ToArray(), checkCriteria)).ToList();
                           voucherIds = (await DataReader.SelectAsync("VOUCHERS", voucherLimitingKeys.ToArray(), voucherCriteria)).ToList();

                           var mergedChecksVouchers = checkIds.Union(voucherIds).ToArray();
                           CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                           {
                               limitingKeys = mergedChecksVouchers.ToList()
                           };
                           return requirements;

                       }
                   );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<PaymentTransaction>, int>(new List<PaymentTransaction>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                totalCount = keyCache.TotalCount.Value;
                var voucherData = await DataReader.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", subList);
                var checkData = await DataReader.BulkReadRecordAsync<DataContracts.Checks>("CHECKS", subList);
                if (voucherData == null && checkData == null)
                {
                    return new Tuple<IEnumerable<PaymentTransaction>, int>(new List<PaymentTransaction>(), 0);
                }
                else
                {

                    var voucherPersonSubList = new List<string>();
                    var checkPersonSubList = new List<string>();

                    if (voucherData != null && voucherData.Any())
                    {
                        voucherPersonSubList = (voucherData.Where(v => !string.IsNullOrEmpty(v.VouVendor)).Select(v => v.VouVendor)).ToList();
                    }
                    if (checkData != null && checkData.Any())
                    {
                        checkPersonSubList = (checkData.Where(c => !string.IsNullOrEmpty(c.ChkVendor)).Select(c => c.ChkVendor)).ToList();
                    }
                    var personSubList = voucherPersonSubList.Union(checkPersonSubList).ToArray();
                    var personsData = await DataReader.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", personSubList);
                    var chkVouchersSubList = checkData.Where(cv => cv.ChkVouchersIds != null && cv.ChkVouchersIds.Any())
                        .SelectMany(cv => cv.ChkVouchersIds).Distinct();
                    var chkVouchers = (await DataReader.BulkReadRecordAsync<Vouchers>(chkVouchersSubList.ToArray())).ToList();
                    //read all the items records in bulk rather than reading one at a time
                    var itemsCheckSubList = chkVouchers.Where(cv => cv.VouItemsId != null && cv.VouItemsId.Any())
                        .SelectMany(cv => cv.VouItemsId).Distinct();
                    var itemsVouSubList = voucherData.Where(cv => cv.VouItemsId != null && cv.VouItemsId.Any())
                        .SelectMany(cv => cv.VouItemsId).Distinct();
                    var itemsSubList = itemsCheckSubList.Union(itemsVouSubList);
                    var vouItems = (await DataReader.BulkReadRecordAsync<Items>(itemsSubList.ToArray())).ToList();
                    var paymentTransactions = await BuildPaymentTransactions(checkData, voucherData, personsData, chkVouchers, vouItems);
                    return new Tuple<IEnumerable<PaymentTransaction>, int>(paymentTransactions, totalCount);
                }

            }
            catch (RepositoryException e)
            {
                throw e;
            }
        }


        /// <summary>
        /// returns a string of valid AP types
        /// </summary>
        /// <returns>list of valid AP Types for vouchers</returns>
        private async Task<List<string>> GetValidApTypes()
        {
            var apTypes = new List<string>();
            apTypes = await GetOrAddToCacheAsync<List<string>>("AllPaymentTypesValidAPTypes",
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
        /// <returns>The PaymentTransactions domain entity</returns>
        public async Task<PaymentTransaction> GetPaymentTransactionsByGuidAsync(string guid)
        {
            try
            {
                Tuple<string, string> voucherID = await GetVoucherIdFromGuidAsync(guid);                
                var response = await GetPaymentTransactionsAsync(voucherID.Item1, voucherID.Item2);
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
                return response;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw new KeyNotFoundException(string.Concat("No payment transactions was found for guid ", guid));
            }
        }

        /// <summary>
        /// Get PaymentTransactions id from Guid
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>id</returns>
        public async Task<string> GetPaymentTransactionsIdFromGuidAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<Tuple<string, string>> GetVoucherIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new RepositoryException("guid is required.");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(string.Concat("No payment transactions was found for guid ", guid));
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(string.Concat("No payment transactions was found for guid ", guid));

            }
            if (foundEntry.Value.Entity != "VOUCHERS" && foundEntry.Value.Entity != "CHECKS")
            {
                throw new RepositoryException(string.Concat("The GUID specified: ", guid, " is used by a different resource: ", foundEntry.Value.Entity, " than expected: VOUCHERS or CHECKS"));
            }
            if (string.IsNullOrEmpty(foundEntry.Value.PrimaryKey))
            {
                throw new RepositoryException(string.Concat("The GUID specified: ", guid, " is not valid for payment-transactions."));
            }
            if ((foundEntry.Value.Entity == "VOUCHERS") && (string.IsNullOrEmpty(foundEntry.Value.SecondaryKey)))
            {
                throw new RepositoryException(string.Concat("The GUID specified: ", guid, " for record key ", foundEntry.Value.PrimaryKey, " from file: VOUCHERS is not valid for payment-transactions. "));
            }
            
            if (foundEntry.Value.Entity.Equals("VOUCHERS", StringComparison.OrdinalIgnoreCase))
            {
                var voucher = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vouchers>(foundEntry.Value.PrimaryKey);
                if (voucher == null)
                {
                    throw new ArgumentException("The VOUCHERS specified for record key " + voucher.Recordkey + " is not found. ");
                }
                
                var apTypesToInclude = await GetValidApTypes();

                if ((apTypesToInclude != null) && (!(apTypesToInclude.ToList().Contains(voucher.VouApType))))
                {
                    throw new RepositoryException("The guid specified " + guid + " for record key " + voucher.Recordkey + " from file VOUCHERS is not valid for payment-transactions.");
                }
            }

            return new Tuple<string, string>(foundEntry.Value.PrimaryKey, foundEntry.Value.Entity);
        }

        /// <summary>
        /// Get a single PaymentTransaction using an ID
        /// </summary>
        /// <param name="id">The PaymentTransaction ID</param>
        /// <returns>The PaymentTransactions</returns>
        private async Task<PaymentTransaction> GetPaymentTransactionsAsync(string id, string entity)
        {
            PaymentTransaction paymentTransaction = null;
            Ellucian.Colleague.Data.Base.DataContracts.Person person = null;
            //System.Collections.ObjectModel.Collection<Vouchers> vouchers = null;
            List<Vouchers> vouchers = null;
            IEnumerable<Items> itemsData = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new RepositoryException("ID is required to get a voucher.");
            }

            var apTypes = await GetValidApTypes();
            if (entity == "VOUCHERS")
            {
                // Now we have an ID, so we can read the record
                var voucher = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vouchers>(id);
                if (voucher == null)
                {
                    throw new KeyNotFoundException(string.Concat("Record not found for voucher with ID ", id, "invalid."));
                }
                // Make sure the voucher has the proper AP type defined
                if (string.IsNullOrEmpty(voucher.VouApType) || !apTypes.Contains(voucher.VouApType))
                {
                    throw new KeyNotFoundException(string.Concat("Record not found for voucher with ID ", id, "invalid."));
                }

                if (!string.IsNullOrEmpty(voucher.VouVendor))
                {
                    person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", voucher.VouVendor);                   
                }
                Dictionary<string, string> voucherGuidCollection = null;
                try
                {
                    voucherGuidCollection = await GetPaymentTransactionsGuidsCollectionAsync(new List<string> { voucher.Recordkey }, "VOUCHERS");
                }
                catch // the error will be thrown when the guid cannot be found.
                { }
                //get the items record
                if (voucher.VouItemsId != null && voucher.VouItemsId.Any())
                {
                    itemsData = await DataReader.BulkReadRecordAsync<Items>(voucher.VouItemsId.ToArray());
                }
                paymentTransaction = await BuildPaymentTransactionFromVoucher(voucher, person, voucherGuidCollection, itemsData);
            }
            else if (entity == "CHECKS")
            {
                // Now we have an ID, so we can read the record
                var check = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Checks>(id);
                if (check == null)
                {
                    throw new KeyNotFoundException(string.Concat("Record not found for check with ID ", id, "invalid."));
                }

                if (!string.IsNullOrEmpty(check.ChkVendor))
                {
                    person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", check.ChkVendor);                  
                }

                if (check.ChkVouchersIds != null && check.ChkVouchersIds.Any())
                {
                    vouchers = (await DataReader.BulkReadRecordAsync<Vouchers>(check.ChkVouchersIds.ToArray())).ToList();
                    if (vouchers != null && vouchers.Any())
                    {
                        // Make sure the voucher has the proper AP type defined
                        foreach (var voucher in vouchers)
                        {
                            if (string.IsNullOrEmpty(voucher.VouApType) || !apTypes.Contains(voucher.VouApType))
                            {
                                throw new KeyNotFoundException(string.Concat("Record not found for check with ID ", id, "invalid."));
                            }
                        }
                        var itemsVouSubList = vouchers.Where(cv => cv.VouItemsId != null && cv.VouItemsId.Any())
                       .SelectMany(cv => cv.VouItemsId).Distinct();
                        itemsData = await DataReader.BulkReadRecordAsync<Items>(itemsVouSubList.ToArray());
                    }
                }

                paymentTransaction = await BuildPaymentTransactionFromCheck(check, person, vouchers, itemsData);
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return paymentTransaction;
        }

        /// <summary>
        ///  Build collection of PaymentTransactions domain entities from collections of associated 
        ///  vouchers and persons
        /// </summary>
        /// <param name="vouchers">Collection of Vouchers data contracts</param>
        /// <param name="persons">Collection of Persons data contracts</param>
        /// <returns>PaymentTransactions domain entity</returns>
        private async Task<IEnumerable<PaymentTransaction>> BuildPaymentTransactions(
            IEnumerable<Checks> checks,
            IEnumerable<Vouchers> vouchers,
            IEnumerable<Base.DataContracts.Person> persons, IEnumerable<Vouchers> checkVouchers, IEnumerable<Items> vouItems
            )
        {
            var paymentTransactionsCollection = new List<PaymentTransaction>();

            //process vouchers
            if (vouchers != null && vouchers.Any())
            {
                var vouIds = vouchers.Select(x => x.Recordkey);
                Dictionary<string, string> voucherGuidCollection = null;
                IEnumerable<Items> itemsData = null;
                try
                {
                    voucherGuidCollection = await GetPaymentTransactionsGuidsCollectionAsync(vouIds, "VOUCHERS");
                }
                catch // the error will be thrown when the guid cannot be found.
                {  }
                foreach (var voucher in vouchers)
                {
                    Base.DataContracts.Person person = null;
                    List<Items> items = null;
                    if (!string.IsNullOrWhiteSpace(voucher.VouVendor))
                    {
                        if (persons != null && persons.Any())
                        {
                            person = persons.FirstOrDefault(p => p.Recordkey == voucher.VouVendor);
                        }
                    }
                    else
                    {
                        exception.AddError(
                    new RepositoryError("Bad.Data", "Missing person record for Id '" + voucher.VouVendor + "'.")
                    {
                         SourceId = voucher.Recordkey
                    });
                    }
                    if (voucher.VouItemsId != null && voucher.VouItemsId.Any())
                    {
                        itemsData = vouItems.Where(itm => voucher.VouItemsId.Contains(itm.Recordkey));

                    }
                    paymentTransactionsCollection.Add(await BuildPaymentTransactionFromVoucher(voucher, person, voucherGuidCollection, itemsData));
                }
            }
            //process checks
            if (checks != null && checks.Any())
            {
                foreach (var check in checks)
                {
                    Base.DataContracts.Person person = null;
                    List<Vouchers> chkVouchers = null;
                    List<Items> itemsData = null;
                    if (!string.IsNullOrWhiteSpace(check.ChkVendor))
                    {
                        if (persons != null && persons.Any())
                        {
                            person = persons.FirstOrDefault(p => p.Recordkey == check.ChkVendor);
                        }
                        else
                        {
                            exception.AddError(
                     new RepositoryError("Bad.Data", "Missing person record for Id '" + check.ChkVendor + "'.")
                     {
                         Id = check.RecordGuid,
                         SourceId = check.Recordkey
                     });
                        }
                    }
                    if (check.ChkVouchersIds != null && check.ChkVouchersIds.Any())
                    {
                        if (checkVouchers != null && checkVouchers.Any())
                        {
                            chkVouchers = checkVouchers.Where(r => check.ChkVouchersIds.Contains(r.Recordkey)).ToList();
                            var itemsVouSubList = chkVouchers.Where(cv => cv.VouItemsId != null && cv.VouItemsId.Any())
                       .SelectMany(cv => cv.VouItemsId).Distinct();
                            itemsData = vouItems.Where(itm => itemsVouSubList.Contains(itm.Recordkey)).ToList();
                        }
                    }
                    paymentTransactionsCollection.Add(await BuildPaymentTransactionFromCheck(check, person, chkVouchers, itemsData));

                }
            }
            if (exception.Errors.Any())
            {
                throw exception;
            }
            return paymentTransactionsCollection.AsEnumerable();
        }

        /// <summary>
        /// Build PaymentTransactions domain entity from associated voucher and person data contracts
        /// </summary>
        /// <param name="voucher">Voucher data contract</param>
        /// <param name="person">Person data contract</param>
        /// <returns>PaymentTransactions domain entity</returns>
        private async Task<PaymentTransaction> BuildPaymentTransactionFromVoucher(Vouchers voucher,
            Ellucian.Colleague.Data.Base.DataContracts.Person person,
            Dictionary<string, string> voucherGuidCollection,
            IEnumerable<Items> itemsData)
        {
            PaymentTransaction paymentTransactionEntity = null;
            var guid = string.Empty;
            if (voucherGuidCollection == null)
            {
                exception.AddError(
                     new RepositoryError("GUID.Not.Found", "Could not find a GUID for payment transactions in VOUCHERS entity.")
                     {
                         SourceId = voucher.Recordkey
                     });
            }
            else
            {
                voucherGuidCollection.TryGetValue(voucher.Recordkey, out guid);
                if (string.IsNullOrEmpty(guid))
                {
                    exception.AddError(
                          new RepositoryError("GUID.Not.Found", "Could not find a GUID for payment transactions in VOUCHERS entity.")
                          {
                              SourceId = voucher.Recordkey
                          });
                }
            }
            // Translate the status code into a VoucherStatus enumeration value
            DateTime? voucherStatusDate = default(DateTime);
            DateTime? voidDate = default(DateTime);
            VoucherStatus? voucherStatus = null;
            if (voucher.VoucherStatusEntityAssociation != null && voucher.VoucherStatusEntityAssociation.Any())
            {
                var voucherStatusEntity =
                    voucher.VoucherStatusEntityAssociation.FirstOrDefault();
                // Get the first status in the list of voucher statuses, and check that it has a value
                if (voucherStatusEntity == null)
                {
                    exception.AddError(
                     new RepositoryError("Bad.Data", "Missing status.")
                     {
                         Id = guid,
                         SourceId = voucher.Recordkey
                     });
                }
                else
                {
                    var voidPaymentTransaction = voucher.VoucherStatusEntityAssociation.FirstOrDefault(x => x.VouStatusAssocMember.ToUpper() == "V");
                    var paidPaymentTransaction = voucher.VoucherStatusEntityAssociation.FirstOrDefault(x => x.VouStatusAssocMember.ToUpper() == "P");

                    if (paidPaymentTransaction != null)
                    {
                        if (paidPaymentTransaction.VouStatusDateAssocMember.HasValue)
                        {
                            voucherStatusDate = paidPaymentTransaction.VouStatusDateAssocMember;
                        }
                        else
                        {
                            //this is a required data field so throwing an exception here
                            exception.AddError(
                         new RepositoryError("Bad.Data", "Missing status date.")
                         {
                             Id = guid,
                             SourceId = voucher.Recordkey
                         });

                        }
                    }
                    if (voidPaymentTransaction != null)
                    {
                        voidDate = voidPaymentTransaction.VouStatusDateAssocMember;
                    }

                    var vouStatus = voucherStatusEntity.VouStatusAssocMember;
                    if (vouStatus != null)
                    {
                        voucherStatus = GetVoucherStatusVoucher(vouStatus);
                    }
                }
            }
            else
            {
                exception.AddError(
                     new RepositoryError("Bad.Data", "Missing status.")
                     {
                         Id = guid,
                         SourceId = voucher.Recordkey
                     });
            }
            try
            {
                paymentTransactionEntity = new PaymentTransaction(voucher.Recordkey, guid, voucherStatusDate.Value);

                paymentTransactionEntity.Check = false;
                paymentTransactionEntity.VoucherStatus = voucherStatus;
                paymentTransactionEntity.VoidDate = voidDate;
                //add data from Items
                var voucherInfo =await GetPaymentTransactionVouchers( voucher, itemsData);
                if (voucherInfo != null)
                {
                    paymentTransactionEntity.AddVoucher(voucherInfo);                       
                }
                var paymentMethod = PaymentMethod.NotSet;
                if ((string.IsNullOrEmpty(voucher.VouArPayment)) && (voucher.VouArDepositItems == null))
                {
                    paymentMethod = PaymentMethod.Creditcard;
                }
                else if (!string.IsNullOrEmpty(voucher.VouArPayment))
                {
                    var arPayments = await DataReader.ReadRecordAsync<ArPayments>(voucher.VouArPayment);
                    if (arPayments != null && !string.IsNullOrEmpty(arPayments.ArpOrigPayMethod))
                    {
                        var paymentMethods = await DataReader.ReadRecordAsync<Base.DataContracts.PaymentMethods>(arPayments.ArpOrigPayMethod);
                        paymentMethod =
                            ((paymentMethods != null) && (paymentMethods.PmthCategory == "CC"))
                                ? PaymentMethod.Creditcard : PaymentMethod.Debitcard;
                    }
                }
                else if ((voucher.VouArDepositItems != null) && (voucher.VouArDepositItems.Any()))
                {
                    var depositItem = voucher.VouArDepositItems.Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(depositItem))
                    {
                        var arDepositItem = await DataReader.ReadRecordAsync<ArDepositItems>(depositItem);
                        if (arDepositItem != null && !string.IsNullOrEmpty(arDepositItem.ArdiOrigPayMethod))
                        {
                            var paymentMethods = await DataReader.ReadRecordAsync<Base.DataContracts.PaymentMethods>(arDepositItem.ArdiOrigPayMethod);
                            paymentMethod =
                                ((paymentMethods != null) && (paymentMethods.PmthCategory == "CC"))
                                    ? PaymentMethod.Creditcard : PaymentMethod.Debitcard;
                        }
                    }
                }
                paymentTransactionEntity.PaymentMethod = (paymentMethod == PaymentMethod.NotSet)
                    ? PaymentMethod.Creditcard : paymentMethod;

                string[] eCommerceStrings = new string[] { voucher.VouEcommerceSession, voucher.VouEcommerceTransNo };
                paymentTransactionEntity.ReferenceNumber = string.Join("-", eCommerceStrings
                    .Where(str => !string.IsNullOrEmpty(str)));

                paymentTransactionEntity.HostCountry = await GetHostCountryAsync();

                paymentTransactionEntity.Vendor = voucher.VouVendor;
                if (person != null)
                {
                    paymentTransactionEntity.IsOrganization = (person.PersonCorpIndicator == "Y");
                }

                paymentTransactionEntity.CurrencyCode = voucher.VouCurrencyCode;
                paymentTransactionEntity.PaymentAmount = voucher.VouTotalAmt;

                var miscName = new List<string>();

                if ((voucher.VouAltFlag == "Y") || (string.IsNullOrEmpty(voucher.VouVendor)))
                {
                    miscName.AddRange(voucher.VouMiscName);
                    paymentTransactionEntity.Address = voucher.VouMiscAddress;
                    paymentTransactionEntity.State = voucher.VouMiscState;
                    paymentTransactionEntity.City = voucher.VouMiscCity;
                    paymentTransactionEntity.Zip = voucher.VouMiscZip;
                    paymentTransactionEntity.Country = voucher.VouMiscCountry;
                }
                else if (!string.IsNullOrEmpty(voucher.VouVendor))
                    {
                    if (person != null)
                    {
                        if (person.PersonCorpIndicator == "Y")
                        {
                            miscName.Add(person.PreferredName);
                        }
                        else
                            miscName.Add(string.Concat(person.FirstName, " ", person.LastName));

                        if (!(string.IsNullOrEmpty(person.PreferredAddress)))
                        {
                            var prefAddress = await DataReader.ReadRecordAsync<Base.DataContracts.Address>(person.PreferredAddress);
                            if (prefAddress != null)
                            {
                                paymentTransactionEntity.Address = prefAddress.AddressLines;
                                paymentTransactionEntity.State = prefAddress.State;
                                paymentTransactionEntity.City = prefAddress.City;
                                paymentTransactionEntity.Zip = prefAddress.Zip;

                            }
                        }
                    }
                    else
                    {
                        exception.AddError(
                     new RepositoryError("Bad.Data", "Missing person record for Id '" + voucher.VouVendor + "'.")
                     {
                         Id = guid,
                         SourceId = voucher.Recordkey
                     });
                    }
                }
                paymentTransactionEntity.MiscName = miscName;

               
            }
            catch (Exception ex)
            {
                exception.AddError(
                     new RepositoryError("Bad.Data", ex.Message)
                     {
                         Id = guid,
                         SourceId = voucher.Recordkey
                     });
            }
            return paymentTransactionEntity;
        }   

        private VoucherStatus? GetVoucherStatusCheck(string vouStatus)
        {
            VoucherStatus? voucherStatus;
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
                    voucherStatus = null;
                    break;
            }
            return voucherStatus;
        }

        private VoucherStatus? GetVoucherStatusVoucher(string vouStatus)
        {
            VoucherStatus? voucherStatus;
            switch (vouStatus.ToUpper())
            {
        
                case "P":
                    voucherStatus = VoucherStatus.Paid;
                    break;
                case "R":
                    voucherStatus = VoucherStatus.Reconciled;
                    break;
                case "V":
                    voucherStatus = VoucherStatus.Voided;
                    break;
                default:
                    voucherStatus = VoucherStatus.Voided;
                    break;
            }
            return voucherStatus;
        }

        private async Task<PaymentTransaction> BuildPaymentTransactionFromCheck(Checks check,
            Ellucian.Colleague.Data.Base.DataContracts.Person person, List<Vouchers> vouchers, IEnumerable<Items> itemsData)
        {
            PaymentTransaction paymentTransactionEntity = null;
            // Translate the status code into a VoucherStatus enumeration value
            VoucherStatus? voucherStatus = null;
            DateTime? voucherStatusDate = default(DateTime);
            DateTime? voidDate = default(DateTime);
            //status is required so this is a bad data situation
            if (check.ChkStatEntityAssociation != null && check.ChkStatEntityAssociation.Any())
            {
                var voucherStatusEntity = check.ChkStatEntityAssociation.FirstOrDefault();

                // Get the first status in the list of voucher statuses, and check that it has a value
                if (voucherStatusEntity != null)
                {
                    var vouStatus = voucherStatusEntity.ChkStatusAssocMember;
                    if (vouStatus != null)
                    {
                        voucherStatus = GetVoucherStatusCheck(vouStatus);
                    }
                    if (voucherStatus == null)                    
                    {
                        exception.AddError(
                    new RepositoryError("Bad.Data", "Invalid voucher status '" + vouStatus + "'")
                    {
                        Id = check.RecordGuid,
                        SourceId = check.Recordkey
                    });
                    }
                }

                var voidPaymentTransaction = check.ChkStatEntityAssociation.FirstOrDefault(x => x.ChkStatusAssocMember.ToUpper() == "V");
                var paidPaymentTransaction = check.ChkStatEntityAssociation.FirstOrDefault(x => x.ChkStatusAssocMember.ToUpper() == "O");

                if (paidPaymentTransaction != null && paidPaymentTransaction.ChkStatusDateAssocMember.HasValue)
                {
                    voucherStatusDate = paidPaymentTransaction.ChkStatusDateAssocMember;
                }

                if (voidPaymentTransaction != null)
                {
                    voidDate = voidPaymentTransaction.ChkStatusDateAssocMember;
                }
            }
            else
            {
                exception.AddError(
                    new RepositoryError("Bad.Data", "Missing status.")
                    {
                        Id = check.RecordGuid,
                        SourceId = check.Recordkey
                    });
            }
            try
            {
                //chkDate is required by the model and it is required in Colleague so if it missing then it is a bad data issue. 
                ///we can contiue creating the entity because it is not required by the entity.
                
                if (!check.ChkDate.HasValue)
                {
                    exception.AddError(
                    new RepositoryError("Bad.Data", "Missing check Date.")
                    {
                        Id = check.RecordGuid,
                        SourceId = check.Recordkey
                    });
                }

                paymentTransactionEntity = new PaymentTransaction(check.Recordkey, check.RecordGuid,
                   check.ChkDate.HasValue ? Convert.ToDateTime(check.ChkDate) : default(DateTime));
                paymentTransactionEntity.VoucherStatus = voucherStatus;
                paymentTransactionEntity.VoidDate = voidDate;
                paymentTransactionEntity.HostCountry = await GetHostCountryAsync();
                paymentTransactionEntity.Check = true;
                paymentTransactionEntity.PaymentMethod = check.ChkEcheckFlag == "Y"
                    ? PaymentMethod.Directdeposit : PaymentMethod.Check;
                paymentTransactionEntity.PaymentAmount = check.ChkAmount;
                paymentTransactionEntity.Vendor = check.ChkVendor;

                if (check.ChkVouchersIds != null && check.ChkVouchersIds.Any())
                {
                    var voucherAmountAndCurrency = new Dictionary<string, AmountAndCurrency>();
                    if (vouchers != null && vouchers.Any())
                    {
                        foreach (var voucher in vouchers)
                        {                            
                            try
                            {
                                var voucherInfo = await GetPaymentTransactionVouchers(voucher, itemsData);
                                if (voucherInfo != null)
                                {
                                    paymentTransactionEntity.AddVoucher(voucherInfo);
                                }
                            }
                            catch (Exception ex)
                            {
                                exception.AddError(
                            new RepositoryError("Bad.Data", ex.Message + " for voucher with Id '" + voucher.Recordkey + "'.")
                            {
                                Id = check.RecordGuid,
                                SourceId = check.Recordkey
                            });
                            }

                        }
                    }
                    else
                    {
                        exception.AddError(
                            new RepositoryError("Bad.Data", "Unable to extract voucher records '" + check.ChkVouchersIds.FirstOrDefault() + "' for check vouchers.")
                            {
                                Id = check.RecordGuid,
                                SourceId = check.Recordkey
                            });
                    }
                }

                if (person != null)
                {
                    paymentTransactionEntity.IsOrganization = (person.PersonCorpIndicator == "Y");
                }
                else
                {
                    if (!string.IsNullOrEmpty(check.ChkVendor))
                    {
                        exception.AddError(
                            new RepositoryError("Bad.Data", "Person Id " + check.ChkVendor + " is not returning any data. Person may be corrupted.")
                            {
                                Id = check.RecordGuid,
                                SourceId = check.Recordkey
                            });
                    }
                }

                paymentTransactionEntity.CurrencyCode = check.ChkCurrencyCode;

                paymentTransactionEntity.MiscName = check.ChkMiscName;
                paymentTransactionEntity.Address = check.ChkAddress;
                paymentTransactionEntity.City = check.ChkCity;
                paymentTransactionEntity.State = check.ChkState;
                paymentTransactionEntity.Zip = check.ChkZip;
                paymentTransactionEntity.Country = check.ChkCountry;
            }
            catch (Exception ex)
            {
                exception.AddError(
                     new RepositoryError("Bad.Data", ex.Message)
                     {
                         Id = check.RecordGuid,
                         SourceId = check.Recordkey
                     });
            }


            return paymentTransactionEntity;
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

        private async Task<PaymentTransactionVoucher> GetPaymentTransactionVouchers(Vouchers voucher, IEnumerable<Items> lineItemRecords)
        {
            var voucherEntity = new PaymentTransactionVoucher(voucher.RecordGuid, voucher.Recordkey);

            if (voucher != null)
            {
                CurrencyCodes currencyCode = CurrencyCodes.USD;
                switch (voucher.VouCurrencyCode)
                {
                    case "CAD":
                        currencyCode = CurrencyCodes.CAD; break;
                    case "EUR":
                        currencyCode = CurrencyCodes.EUR; break;
                    default:
                        currencyCode = CurrencyCodes.USD; break;
                }
                if (voucher.VouTotalAmt != 0)
                { 
                    var amountAndCurrency = new AmountAndCurrency(voucher.VouTotalAmt, currencyCode);
                    voucherEntity.VoucherInvoiceAmt = amountAndCurrency;                    
                }
                
                //save PO Id
                if (!string.IsNullOrEmpty(voucher.VouPoNo))
                {
                    voucherEntity.PurchaseOrderId = voucher.VouPoNo;
                }
                //save recurring voucher Id
                if (!string.IsNullOrEmpty(voucher.VouRcvsId))
                {
                    voucherEntity.RecurringVoucherId = voucher.VouRcvsId;
                }
                //save BPO Id
                if (!string.IsNullOrEmpty(voucher.VouBpoId))
                {
                    voucherEntity.BlanketPurchaseOrderId = voucher.VouBpoId;
                }
                if (voucher.VouItemsId != null && voucher.VouItemsId.Any())
                {
                    foreach( var item in voucher.VouItemsId)
                    {
                        var itemData = lineItemRecords.FirstOrDefault(line => line.Recordkey == item);
                        if( itemData != null)
                        {
                            var itemEntity = GetLineItem(itemData);
                            if (itemEntity != null)
                            {
                                voucherEntity.AddAccountsPayableInvoicesLineItem(itemEntity);

                            }
                        }
                        else
                        {
                            throw new RepositoryException(string.Format("Error occured while getting items record for id '{0}'", item)); ;
                        }
                    }
                }                
            }

            return voucherEntity;

        }

        /// <summary>
        /// Get AccountsPayableInvoicesLineItem
        /// </summary>
        /// <param name="lineItem">Items data contract</param>
        /// <param name="currencyCode">line amount subtotal including tax</param>
        /// <returns>Tuple containing an AccountsPayableInvoicesLineItem and amout subtotal</returns>
        private AccountsPayableInvoicesLineItem GetLineItem(Items lineItem)
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
            if (string.IsNullOrEmpty(itemDescription))
            {
                itemDescription = "Unknown";
            }

            decimal itemQuantity = lineItem.ItmVouQty ?? 0;
            decimal itemPrice = lineItem.ItmVouPrice ?? 0;
            decimal itemExtendedPrice = lineItem.ItmVouExtPrice ?? 0;
            var lineItemDomainEntity = new AccountsPayableInvoicesLineItem(lineItem.Recordkey, itemDescription, itemQuantity, itemPrice, itemExtendedPrice);     
            return lineItemDomainEntity;
        }

        /// <summary>
        /// Using a collection of voucher ids, get a dictionary collection of associated secondary guids on VOU.PMT.TXN.INTG.IDX
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetPaymentTransactionsGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();

            try
            {
                var guidLookup = ids
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Distinct().ToList()
                   .ConvertAll(p => new RecordKeyLookup(filename, p, "VOU.PMT.TXN.INTG.IDX", p, false)).ToArray();

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
                throw new Exception(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
        }

        /// <summary>
        /// Get a collection of guids for parameter fileName 
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();

            try
            {
                var guidLookup = ids
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Distinct().ToList()
                   .ConvertAll(p => new RecordKeyLookup(filename, p,false)).ToArray();

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
                throw new Exception(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
        }


        /// <summary>
        /// Get id from Guid
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>id</returns>
        public async Task<string> GetIdFromGuidAsync(string guid, string entity)
        {
            string id = string.Empty;
            try
            {
                var guidRec = await GetRecordInfoFromGuidAsync(guid);
                if (guidRec != null && guidRec.Entity == entity && string.IsNullOrEmpty(guidRec.SecondaryKey))
                {
                    id = guidRec.PrimaryKey;
                }
            }
            catch
            { }
            return id;
            
        }
    }
}