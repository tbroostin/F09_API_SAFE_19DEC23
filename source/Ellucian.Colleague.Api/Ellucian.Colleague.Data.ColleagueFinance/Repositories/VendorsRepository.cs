/*Copyright 2016-2021 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Exceptions;
using Vendors = Ellucian.Colleague.Domain.ColleagueFinance.Entities.Vendors;
using System.Text;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class VendorsRepository : BaseColleagueRepository, IVendorsRepository
    {
        public static char _SM = Convert.ToChar(DynamicArray.SM);

        private readonly int readSize;
        protected const int AllVendorsCacheTimeout = 20; // Clear from cache every 20 minutes
        protected const string AllVendorFilterCache = "AllVendorsFilter";


        public VendorsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {

            CacheTimeout = Level1CacheTimeoutValue;

            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;

        }

        /// <summary>
        /// Get the GUID for a vendor using its ID
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Section GUID</returns>
        public async Task<string> GetVendorGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("VENDORS", id);
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Bad.Data", "GUID not found for vendor id: " + id));
                throw ex;
            }
        }


        /// <summary>
        /// Get a list of vendors using criteria
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Vendors>, int>> GetVendorsAsync(int offset, int limit, string vendorDetail = "", List<string> classifications = null,
            List<string> statuses = null, List<string> relatedReference = null, List<string> types = null)
        {
            string criteria = "";

            var criteriaBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(vendorDetail))
            {
                criteriaBuilder.AppendFormat("WITH VENDORS.ID = '{0}'", vendorDetail);
            }
            if (classifications != null && classifications.Any())
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                int i = 0;
                foreach (var classification in classifications)
                {
                    if (i == 0)
                        criteriaBuilder.Append("WITH ");
                    else
                        criteriaBuilder.Append(" AND ");
                    criteriaBuilder.AppendFormat("VEN.TYPES = '{0}'", classification);
                    i++;
                }
            }
            if ((statuses != null) && (statuses.Any()))
            {
                foreach (var status in statuses)
                {
                    switch (status.ToLower())
                    {
                        case "active":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.ACTIVE.FLAG = 'Y'");
                            break;
                        case "holdpayment":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.STOP.PAYMENT.FLAG = 'Y'");
                            break;
                        case "approved":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.APPROVAL.FLAG = 'Y'");
                            break;
                    }
                }
            }

            string typesCriteria = string.Empty;
            if (types != null && types.Any())
            {
                foreach (var type in types.Distinct())
                {
                    if (!string.IsNullOrEmpty(type))
                    {
                        switch (type.ToLower())
                        {
                            case "eprocurement":
                                if (criteriaBuilder.Length > 0)
                                {
                                    typesCriteria = " AND WITH VEN.CATEGORIES = 'EP'";
                                }
                                else
                                {
                                    typesCriteria = "WITH VEN.CATEGORIES = 'EP'";
                                }
                                criteriaBuilder.Append(typesCriteria);
                                continue;
                            case "travel":
                                if (criteriaBuilder.Length > 0)
                                {
                                    typesCriteria = " AND WITH VEN.CATEGORIES = 'TR'";
                                }
                                else
                                {
                                    typesCriteria = "WITH VEN.CATEGORIES = 'TR'";
                                }
                                criteriaBuilder.Append(typesCriteria);
                                continue;
                        }
                    }
                }
            }

            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }

            string[] vendorsIds = null;

            if ((relatedReference != null) && (relatedReference.Any()))
            {
                //at the moment only perentVendor is available. So we'll look for only that type, even though
                // this is an array.

                string personCriteria = "WITH PERSON.CORP.INDICATOR = 'Y' AND WITH PARENTS NE ''";

                var corpIds = await DataReader.SelectAsync("PERSON", personCriteria);
                if (corpIds.Any())
                    vendorsIds = await DataReader.SelectAsync("VENDORS", corpIds, criteria);

            }
            else
            {
                vendorsIds = await DataReader.SelectAsync("VENDORS", criteria);
            }

            if ((vendorsIds == null) || (!vendorsIds.Any()))
            {
                return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
            }

            var totalCount = vendorsIds.Count();
            Array.Sort(vendorsIds);
            var subList = vendorsIds.Skip(offset).Take(limit).ToArray();

            var vendorsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", subList);
            {
                if (vendorsData == null)
                {
                    throw new KeyNotFoundException("No records selected from Vendors file in Colleague.");
                }
            }
            var personsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", subList);
            {
                if (personsData == null)
                {
                    throw new KeyNotFoundException("No records selected from Persons file in Colleague.");
                }
            }
            var corpContract = await DataReader.BulkReadRecordAsync<Corp>("PERSON", subList);

            var vendors = BuildVendors(vendorsData.ToList(), personsData.ToList(), corpContract.ToList());

            return new Tuple<IEnumerable<Vendors>, int>(vendors, totalCount);
        }
        /// <summary>
        /// Get a list of vendors using criteria
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Vendors>, int>> GetVendors2Async(int offset, int limit, string vendorDetail = "", List<string> classifications = null,
            List<string> statuses = null, List<string> relatedReference = null, List<string> types = null, string taxId = null)
        {
            try
            {
                int totalCount = 0;
                string[] subList = null;
                var repositoryException = new RepositoryException();
                string vendorCacheKey = CacheSupport.BuildCacheKey(AllVendorFilterCache, vendorDetail, classifications, statuses, relatedReference, types, taxId);
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       vendorCacheKey,
                       "VENDORS",
                       offset,
                       limit,
                       AllVendorsCacheTimeout,
                       async () =>
                       {
                           return await GetVendorsFilterCriteriaAsync(vendorDetail, classifications, statuses, types, relatedReference, taxId, null);
                       }
                   );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                totalCount = keyCache.TotalCount.Value;
                var vendorsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", subList);
                if (vendorsData != null && vendorsData.Any())
                {
                    var personsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", subList);
                    var corpContract = await DataReader.BulkReadRecordAsync<Corp>("PERSON", subList);
                    var corpFoundData = await DataReader.BulkReadRecordAsync<CorpFounds>(subList);
                    var vendors = BuildVendors2(vendorsData, personsData, corpContract, corpFoundData);
                    return new Tuple<IEnumerable<Vendors>, int>(vendors, totalCount);

                }
                else
                {
                    return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                }
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Get a list of vendors using criteria
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Vendors>, int>> GetVendorsMaximumAsync(int offset, int limit, string vendorDetail = "",
            List<string> statuses = null, List<string> types = null, string taxId = null, List<string> addresses = null)
        {
            try
            {
                int totalCount = 0;
                string[] subList = null;
                var repositoryException = new RepositoryException();
                string vendorCacheKey = CacheSupport.BuildCacheKey(AllVendorFilterCache, vendorDetail, statuses, types, taxId);
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       vendorCacheKey,
                       "VENDORS",
                       offset,
                       limit,
                       AllVendorsCacheTimeout,
                       async () =>
                       {
                           return await GetVendorsFilterCriteriaAsync(vendorDetail, null, statuses, types, null, taxId, addresses);
                       }
                   );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                totalCount = keyCache.TotalCount.Value;
                var vendorsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", subList);
                if (vendorsData != null && vendorsData.Any())
                {
                    var personsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", subList);
                    var corpContract = await DataReader.BulkReadRecordAsync<Corp>("PERSON", subList);
                    var corpFoundData = await DataReader.BulkReadRecordAsync<CorpFounds>(subList);
                    var personIntgData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.PersonIntg>("PERSON.INTG", subList);

                    var personAddresses = personsData.Where(p => p.PersonAddresses != null).SelectMany(p => p.PersonAddresses);
                    var addressesData = new List<Base.DataContracts.Address>();
                    if (personAddresses != null && personAddresses.Any())
                    {
                        var addressesDataContract = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", personAddresses.Distinct().ToArray());
                        if (addressesDataContract != null)
                        {
                            addressesData = addressesDataContract.ToList();
                        }
                    }
                    var vendors = BuildVendorsMaximum(vendorsData, personsData, corpContract, corpFoundData, personIntgData, addressesData);
                    return new Tuple<IEnumerable<Vendors>, int>(vendors, totalCount);

                }
                else
                {
                    return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                }
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }

        }



        /// <summary>
        /// Get vendor criteria and limiting list.
        /// </summary>
        /// <returns></returns>
        private async Task<CacheSupport.KeyCacheRequirements> GetVendorsFilterCriteriaAsync(string vendorDetail, List<string> classifications, List<string> statuses, List<string> types, List<string> relatedReference, string taxId, List<string> addresses)
        {
            string criteria = "";
            var criteriaBuilder = new StringBuilder();
            List<string> vendorsLimitingKeys = new List<string>();
            if (!string.IsNullOrEmpty(vendorDetail))
            {
                criteriaBuilder.AppendFormat("WITH VENDORS.ID = '{0}'", vendorDetail);
            }
            if (classifications != null && classifications.Any())
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                int i = 0;
                foreach (var classification in classifications)
                {
                    if (i == 0)
                        criteriaBuilder.Append("WITH ");
                    else
                        criteriaBuilder.Append(" AND ");
                    criteriaBuilder.AppendFormat("VEN.TYPES = '{0}'", classification);
                    i++;
                }
            }
            if ((statuses != null) && (statuses.Any()))
            {
                foreach (var status in statuses)
                {
                    switch (status.ToLower())
                    {
                        case "active":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.ACTIVE.FLAG = 'Y'");
                            break;
                        case "holdpayment":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.STOP.PAYMENT.FLAG = 'Y'");
                            break;
                        case "approved":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.APPROVAL.FLAG = 'Y'");
                            break;
                    }
                }
            }

            string typesCriteria = string.Empty;
            if (types != null && types.Any())
            {
                foreach (var type in types.Distinct())
                {
                    if (!string.IsNullOrEmpty(type))
                    {
                        switch (type.ToLower())
                        {
                            case "eprocurement":
                                if (criteriaBuilder.Length > 0)
                                {
                                    typesCriteria = " AND WITH VEN.CATEGORIES = 'EP'";
                                }
                                else
                                {
                                    typesCriteria = "WITH VEN.CATEGORIES = 'EP'";
                                }
                                criteriaBuilder.Append(typesCriteria);
                                continue;
                            case "travel":
                                if (criteriaBuilder.Length > 0)
                                {
                                    typesCriteria = " AND WITH VEN.CATEGORIES = 'TR'";
                                }
                                else
                                {
                                    typesCriteria = "WITH VEN.CATEGORIES = 'TR'";
                                }
                                criteriaBuilder.Append(typesCriteria);
                                continue;
                            case "procurement":
                                if (criteriaBuilder.Length > 0)
                                {
                                    typesCriteria = " AND WITH VEN.CATEGORIES = 'PR'";
                                }
                                else
                                {
                                    typesCriteria = "WITH VEN.CATEGORIES = 'PR'";
                                }
                                criteriaBuilder.Append(typesCriteria);
                                continue;
                        }
                    }
                }
            }

            if ((relatedReference != null) && (relatedReference.Any()))
            {
                //we will first select all the parents record for the corps.

                string parentCriteria = "WITH PERSON.CORP.INDICATOR = 'Y' AND WITH PARENTS NE '' BY.EXP PARENTS SAVING PARENTS";
                var parentCorpIds = await DataReader.SelectAsync("PERSON", parentCriteria);
                if (parentCorpIds != null && parentCorpIds.Any())
                {
                    var parentVendors = await DataReader.SelectAsync("VENDORS", parentCorpIds, criteria);
                    // if we have parent vendors then we need to select the child vendors. 
                    if (parentVendors != null && parentVendors.Any())
                    {
                        var children = await DataReader.SelectAsync("PERSON", "WITH PERSON.CORP.INDICATOR EQ 'Y' AND WITH PARENTS EQ '?'", parentVendors);
                        if (children != null && children.Any())
                        {
                            vendorsLimitingKeys = (await DataReader.SelectAsync("VENDORS", children, criteria)).ToList();
                            // if there is corp.founds but no vendor record.
                            if (vendorsLimitingKeys == null || !vendorsLimitingKeys.Any())
                            {
                                return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                            }
                        }
                        else
                        {
                            return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                        }

                    }
                    else
                    {
                        return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                    }
                }
                //if no parent corp is selected then we need to return null
                else
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
            }

            if ((addresses != null) && (addresses.Any()))
            {
                var addrCriteria = string.Empty;
                foreach (var addr in addresses)
                {
                    if (string.IsNullOrEmpty(addrCriteria))
                        addrCriteria = string.Format("WITH PERSON.ADDRESSES EQ '{0}'", addr);
                    else
                        addrCriteria = string.Format(" AND WITH PERSON.ADDRESSES EQ '{0}'", addr);
                }
                var persons = await DataReader.SelectAsync("PERSON", vendorsLimitingKeys.ToArray(), addrCriteria);
                if (persons != null && persons.Any())
                {
                    vendorsLimitingKeys = (await DataReader.SelectAsync("VENDORS", persons, criteria)).ToList();
                    if (vendorsLimitingKeys == null || !vendorsLimitingKeys.Any())
                    {
                        return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                    }
                }
                else
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
            }


            if (!string.IsNullOrEmpty(taxId))
            {

                //if the input is 9 characters then we know there is no - so send it with dash between 2 and 3 characters for US
                var intlParams = await GetInternationalParametersAsync();
                if (taxId.Length == 9 && intlParams.HostCountry == "USA")
                {
                    var forTaxId = string.Concat(taxId.Substring(0, 2), "-", taxId.Substring(2, 7));
                    taxId = forTaxId;
                }
                string taxCriteria = string.Format("WITH CORP.TAX.ID EQ '{0}'", taxId);

                var corpIds = await DataReader.SelectAsync("CORP.FOUNDS", vendorsLimitingKeys.ToArray(), taxCriteria);
                if (corpIds != null && corpIds.Any())
                {
                    vendorsLimitingKeys = (await DataReader.SelectAsync("VENDORS", corpIds, criteria)).ToList();
                    // if there is corp.founds but no vendor record.
                    if (vendorsLimitingKeys == null || !vendorsLimitingKeys.Any())
                    {
                        return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                    }
                }
                else
                {
                    // if there is no CORP.FOUNDS with no EIN
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
            }

            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }
            return new CacheSupport.KeyCacheRequirements()
            {
                limitingKeys = vendorsLimitingKeys,
                criteria = criteria
            };

        }

        /// Get a single vendor using a GUID
        /// </summary>
        /// <param name="guid">The Vendor guid</param>
        /// <returns>The vendor domain entity</returns>
        public async Task<Vendors> GetVendorsByGuidAsync(string guid)
        {
            try
            {
                string id = await GetVendorIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for vendor guid:", guid));
                }
                return await GetVendorsAsync(id);
            }
            catch (Exception e)
            {
                throw new KeyNotFoundException("Vendors GUID " + guid + " lookup failed.");
            }
        }

        /// <summary>
        /// Get a single vendor using a GUID
        /// </summary>
        /// <param name="guid">The Vendor guid</param>
        /// <returns>The vendor domain entity</returns>
        public async Task<Vendors> GetVendorsByGuid2Async(string guid)
        {
            string id = string.Empty;
            try
            {
                id = await GetVendorIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for vendor guid:", guid));
                }
                return await GetVendors2Async(id);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    Id = guid,
                    SourceId = id
                });
                throw exception;
            }
        }



        /// <summary>
        /// Get the list of vendors based on keyword search.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for vendor search.</param>
        /// <returns> The vendor search results</returns> 
        public async Task<IEnumerable<VendorSearchResult>> SearchByKeywordAsync(string searchCriteria, string apType)
        {
            List<VendorSearchResult> vendorSearchResults = new List<VendorSearchResult>();
            if (string.IsNullOrEmpty(searchCriteria))
                throw new ArgumentNullException("searchCriteria", "search criteria required to query");

            var searchRequest = new GetActiveVendorResultsRequest();
            searchRequest.ASearchCriteria = searchCriteria;
            searchRequest.AApType = apType;

            var searchResponse = await transactionInvoker.ExecuteAsync<GetActiveVendorResultsRequest, GetActiveVendorResultsResponse>(searchRequest);
            if (searchResponse == null)
            {
                throw new InvalidOperationException("An error occurred during person matching");
            }
            if (searchResponse.AlErrorMessages.Count() > 0)
            {
                var errorMessage = "Error(s) occurred during vendor search:";
                errorMessage += string.Join(Environment.NewLine, searchResponse.AlErrorMessages);
                logger.Error(errorMessage);
                throw new InvalidOperationException("An error occurred during vendor search");
            }

            if (searchResponse.VendorSearchResults != null && searchResponse.VendorSearchResults.Any())
            {
                vendorSearchResults = searchResponse.VendorSearchResults.Where(x => x != null && x.AlVendorIds != null).Select(x => new VendorSearchResult(x.AlVendorIds)
                {
                    VendorName = x.AlVendorNames,
                    VendorAddress = x.AlVendorAddresses,
                    TaxForm = x.AlVendorTaxForm,
                    TaxFormCode = x.AlVendorTaxFormCode,
                    TaxFormLocation = x.AlVendorTaxFormLoc,
                    TaxForm1099NecWithholding = x.AlVendorNecWthhld,
                    TaxForm1099MiscWithholding = x.AlVendor1099Wthhld,
                    AddressTypeCode = x.AlVendAddrTypeCodes,
                    AddressTypeDesc = x.AlVendAddrTypeDesc,
                    VendorApTypes = !string.IsNullOrEmpty(x.AlVenApTypes) ? x.AlVenApTypes.Split(_SM).ToList() : new List<string>()
                }).ToList();
            }

            return vendorSearchResults.AsEnumerable();
        }

        /// <summary>
        /// Get the list of vendors for vouchers based on keyword search.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for vendor search.</param>
        /// <returns> The vendor search results for the vouchers</returns> 
        public async Task<IEnumerable<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult>> VendorSearchForVoucherAsync(string searchCriteria, string apType)
        {
            List<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult> voucherVendorSearchResults = new List<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult>();
            if (string.IsNullOrEmpty(searchCriteria))
                throw new ArgumentNullException("searchCriteria", "search criteria required to query");

            var searchRequest = new TxGetVoucherVendorResultsRequest();
            searchRequest.ASearchCriteria = searchCriteria;
            searchRequest.AApType = apType;

            var searchResponse = await transactionInvoker.ExecuteAsync<TxGetVoucherVendorResultsRequest, TxGetVoucherVendorResultsResponse>(searchRequest);
            if (searchResponse == null)
            {
                throw new InvalidOperationException("An error occurred during person matching");
            }
            if (searchResponse.AlErrorMessages.Count() > 0 && searchResponse.AError)
            {
                var errorMessage = "Error(s) occurred during vendor search:";
                errorMessage += string.Join(Environment.NewLine, searchResponse.AlErrorMessages);
                logger.Error(errorMessage);
                throw new InvalidOperationException("An error occurred during vendor search");
            }

            if (searchResponse.VoucherVendorSearchResults != null && searchResponse.VoucherVendorSearchResults.Any())
            {

                foreach (var x in searchResponse.VoucherVendorSearchResults)
                {
                    var resultEntity = new Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult();
                    resultEntity.VendorId = x.AlVendorIds;

                    // there may be multiple (sub-valued) messages for each vendor names.
                    resultEntity.VendorNameLines = !string.IsNullOrEmpty(x.AlVendorNames) ? x.AlVendorNames.Split(_SM).ToList() : new List<string>();
                    resultEntity.AddressId = x.AlVendorAddrIds;
                    resultEntity.FormattedAddress = x.AlVendorFormattedAddresses;
                    // there may be multiple (sub-valued) messages for each vendor address.
                    resultEntity.AddressLines = !string.IsNullOrEmpty(x.AlVendorAddress) ? x.AlVendorAddress.Split(_SM).ToList() : new List<string>();
                    resultEntity.City = x.AlVendorCity;
                    resultEntity.Country = x.AlVendorCountry;
                    resultEntity.State = x.AlVendorState;
                    resultEntity.Zip = x.AlVendorZip;
                    resultEntity.TaxForm = x.AlVendorTaxForm;
                    resultEntity.TaxFormCode = x.AlVendorTaxFormCode;
                    resultEntity.TaxFormLocation = x.AlVendorTaxFormLoc;
                    resultEntity.TaxForm1099NecWithholding = x.AlVendor1099necWthhld;
                    resultEntity.TaxForm1099MiscWithholding = x.AlVendor1099miWthhld;
                    resultEntity.AddressTypeCode = x.AlVendAddrTypeCodes;
                    resultEntity.AddressTypeDesc = x.AlVendAddrTypeDesc;
                    resultEntity.VendorApTypes = !string.IsNullOrEmpty(x.AlVenApTypes) ? x.AlVenApTypes.Split(_SM).ToList() : new List<string>(); 
                    voucherVendorSearchResults.Add(resultEntity);
                }
               
            }

            return voucherVendorSearchResults.AsEnumerable();
        }

        /// <summary>
        /// Update an existing Vendors domain entity
        /// </summary>
        /// <param name="vendorsEntity">Vendors domain entity</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors> UpdateVendorsAsync(Vendors vendorsEntity)
        {
            if (vendorsEntity == null)
                throw new ArgumentNullException("vendorsEntity", "Must provide a vendorsEntity to update.");
            if (string.IsNullOrWhiteSpace(vendorsEntity.Guid))
                throw new ArgumentNullException("vendorsEntity", "Must provide the guid of the vendorsEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var vendorsId = await this.GetVendorIdFromGuidAsync(vendorsEntity.Guid);
            if (!string.IsNullOrEmpty(vendorsId))
            {

                var updateRequest = await BuildVendorsUpdateRequestAsync(vendorsEntity);

                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(updateRequest);

                if (updateResponse.Errors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating vendors '{0}':", vendorsEntity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.Errors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated entity from the database
                return await GetVendorsByGuidAsync(vendorsEntity.Guid);
            }

            // perform a create instead
            return await CreateVendorsAsync(vendorsEntity);

        }


        /// <summary>
        /// Update an existing Vendors domain entity
        /// </summary>
        /// <param name="vendorsEntity">Vendors domain entity</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors> UpdateVendors2Async(Vendors vendorsEntity)
        {
            if (vendorsEntity == null)
                throw new ArgumentNullException("vendorsEntity", "Must provide a vendorsEntity to update.");
            if (string.IsNullOrWhiteSpace(vendorsEntity.Guid))
                throw new ArgumentNullException("vendorsEntity", "Must provide the guid of the vendorsEntity to update.");
            var repositoryException = new RepositoryException();
            CreateUpdateVendorRequest updateRequest = null;
            Vendors updatedEntity = null;
            try
            {
                updateRequest = await BuildVendorsUpdateRequestAsync(vendorsEntity);
                updateRequest.Version = "11";
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = vendorsEntity.Guid,
                        SourceId = vendorsEntity.Id
                    });
                throw repositoryException;
            }
            if (updateRequest != null)
            {
                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(updateRequest);

                if (updateResponse != null && updateResponse.Errors.Any())
                {
                    foreach (var error in updateResponse.Errors)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCodes, " - ", error.ErrorMessages))
                        {
                            SourceId = updateResponse.VendorId,
                            Id = updateResponse.VendorGuid

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        updatedEntity = await GetVendorsByGuid2Async(updateResponse.VendorGuid);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = vendorsEntity.Guid,
                           SourceId = vendorsEntity.Id
                       });
                        throw repositoryException;
                    }
                }
            }
            // get the updated entity from the database
            return updatedEntity;
        }

        /// <summary>
        /// Create a new Vendors domain entity
        /// </summary>
        /// <param name="vendorsEntity">Vendors domain entity</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors> CreateVendorsAsync(Vendors vendorsEntity)
        {
            if (vendorsEntity == null)
                throw new ArgumentNullException("vendorsEntity", "Must provide a vendorsEntity to create.");

            var createRequest = await BuildVendorsUpdateRequestAsync(vendorsEntity);
            createRequest.VendorId = string.Empty;

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            var createResponse = await transactionInvoker.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(createRequest);

            if (createResponse.Errors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating vendors '{0}':", vendorsEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.Errors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created record from the database
            return await GetVendorsByGuidAsync(createResponse.VendorGuid);
        }

        /// <summary>
        /// Create a new Vendors domain entity
        /// </summary>
        /// <param name="vendorsEntity">Vendors domain entity</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors> CreateVendors2Async(Vendors vendorsEntity)
        {
            if (vendorsEntity == null)
                throw new ArgumentNullException("vendorsEntity", "Must provide a vendorsEntity to create.");
            CreateUpdateVendorRequest createRequest = null;
            var repositoryException = new RepositoryException();
            Vendors createdEntity = null;
            try
            {
                createRequest = await BuildVendorsUpdateRequestAsync(vendorsEntity);
                createRequest.Version = "11";
            }
            catch (Exception ex)
            {
                repositoryException.AddError(
                    new RepositoryError("Bad.Data", ex.Message)
                    {
                        Id = vendorsEntity.Guid,
                        SourceId = vendorsEntity.Id
                    });
                throw repositoryException;
            }
            if (createRequest != null)
            {
                createRequest.VendorId = string.Empty;

                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    createRequest.ExtendedNames = extendedDataTuple.Item1;
                    createRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                var createResponse = await transactionInvoker.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(createRequest);

                if (createResponse != null && createResponse.Errors.Any())
                {
                    foreach (var error in createResponse.Errors)
                    {

                        repositoryException.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCodes, " - ", error.ErrorMessages))
                        {
                            SourceId = createResponse.VendorId,
                            Id = createResponse.VendorGuid

                        });
                    }
                    throw repositoryException;
                }
                else
                {
                    try
                    {
                        createdEntity = await GetVendorsByGuid2Async(createResponse.VendorGuid);
                    }
                    catch (Exception ex)
                    {
                        repositoryException.AddError(
                       new RepositoryError("Bad.Data", ex.Message)
                       {
                           Id = vendorsEntity.Guid,
                           SourceId = vendorsEntity.Id
                       });
                        throw repositoryException;
                    }
                }
            }

            // get the newly created record from the database
            return createdEntity;
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetVendorIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Vendors GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Vendors GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "VENDORS")
            {
                throw new ArgumentException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, VENDORS");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single vendor using an ID
        /// </summary>
        /// <param name="id">The vendor GUID</param>
        /// <returns>The vendor</returns>
        public async Task<Vendors> GetVendorsAsync(string id)
        {
            Vendors vendor = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a vendor.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found for vendor with ID ", id));
            }

            var personRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(id);
            if (personRecord == null)
            {
                throw new ArgumentNullException(string.Concat("Record not found for person with ID ", id));
            }

            var corpRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", id);

            // Build the vendor data
            vendor = BuildVendor(record, personRecord, corpRecord);

            return vendor;
        }

        /// <summary>
        /// Get a single vendor using an ID
        /// </summary>
        /// <param name="id">The vendor GUID</param>
        /// <returns>The vendor</returns>
        public async Task<Vendors> GetVendors2Async(string id)
        {
            Vendors vendor = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a vendor.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found for vendor with ID ", id));
            }

            var personRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(id);
            if (personRecord == null)
            {
                throw new ArgumentNullException(string.Concat("Record not found for person with ID ", id));
            }

            var corpRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", id);
            var corpFoundRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.CorpFounds>(id);

            // Build the vendor data
            vendor = BuildVendor2(record, personRecord, corpRecord, corpFoundRecord);

            return vendor;
        }

        /// <summary>
        /// Get a single vendor using an ID
        /// </summary>
        /// <param name="id">The vendor GUID</param>
        /// <returns>The vendor</returns>
        public async Task<Vendors> GetVendorsMaximumByGuidAsync(string guid)
        {
            Vendors vendor = null;
            string id = string.Empty;
            try
            {
                id = await GetVendorIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for vendor guid:", guid));
                }
                else
                {
                    // Now we have an ID, so we can read the record
                    var record = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(id);
                    if (record == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Record not found for vendor with ID ", id));
                    }

                    var personRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(id);
                    if (personRecord == null)
                    {
                        throw new ArgumentNullException(string.Concat("Record not found for person with ID ", id));
                    }

                    var corpRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", id);
                    var corpFoundRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.CorpFounds>(id);
                    var personIntg = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.PersonIntg>(id);
                    //var addressRecords = (await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(personRecord.PersonAddresses.Distinct().ToArray())).ToList();
                    var personAddresses = personRecord.PersonAddresses;
                    var addressesData = new List<Base.DataContracts.Address>();
                    if (personAddresses != null && personAddresses.Any())
                    {
                        var addressesDataContract = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", personAddresses.Distinct().ToArray());
                        if (addressesDataContract != null)
                        {
                            addressesData = addressesDataContract.ToList();
                        }
                    }

                    // Build the vendor data
                    vendor = BuildVendorMaximum(record, personRecord, corpRecord, corpFoundRecord, personIntg, addressesData);

                    return vendor;
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    Id = guid,
                    SourceId = id
                });
                throw exception;
            }


        }

        /// <summary>
        /// Vendor default Tax form information
        /// </summary>
        /// <param name="vendorId">vendor id</param>
        /// <param name="apType">ap type</param>
        /// <returns>vendor default tax info entity</returns>
        public async Task<VendorDefaultTaxFormInfo> GetVendorDefaultTaxFormInfoAsync(string vendorId, string apType)
        {

            if (string.IsNullOrEmpty(vendorId))
                throw new ArgumentNullException("vendorId", "vendorId is required");

            VendorDefaultTaxFormInfo taxFormInfo = new VendorDefaultTaxFormInfo(vendorId);
            var request = new GetVendDefaultTaxInfoRequest();
            request.AVendorId = vendorId;
            request.AApType = apType;

            var response = await transactionInvoker.ExecuteAsync<GetVendDefaultTaxInfoRequest, GetVendDefaultTaxInfoResponse>(request);
            if (response == null)
            {
                throw new InvalidOperationException("An error occurred while getting vendor tax form");
            }
            taxFormInfo.TaxForm = response.ATaxForm;
            taxFormInfo.TaxFormBoxCode = response.ATaxFormCode;
            taxFormInfo.TaxFormState = response.ATaxFormLoc;
            taxFormInfo.TaxForm1099NecWithholding = response.AVen1099necWthhldFlag;
            taxFormInfo.TaxForm1099MiscWithholding = response.AVen1099miWthhldFlag;
            taxFormInfo.VendorApTypes = response.AlVenApTypes;
            return taxFormInfo;
        }

        /// <summary>
        /// Build a collection of vendor domain entities from data contracts
        /// </summary>
        /// <param name="sources">Vendors data contracts</param>
        /// <param name="persons">Persons data contracst</param>
        /// <param name="corp"></param>
        /// <returns>collection of vendor domain entities</returns>
        private IEnumerable<Vendors> BuildVendors(List<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors> sources,
            List<Ellucian.Colleague.Data.Base.DataContracts.Person> persons,
            List<Ellucian.Colleague.Data.Base.DataContracts.Corp> corp)
        {
            var vendorCollection = new List<Vendors>();
            var cor = new Corp();
            foreach (var source in sources)
            {
                var person = persons.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                if (corp != null)
                    cor = corp.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                vendorCollection.Add(BuildVendor(source, person, cor));
            }

            return vendorCollection.AsEnumerable();
        }

        /// <summary>
        /// Build a collection of vendor domain entities from data contracts
        /// </summary>
        /// <param name="sources">Vendors data contracts</param>
        /// <param name="persons">Persons data contracst</param>
        /// <param name="corp"></param>
        /// <returns>collection of vendor domain entities</returns>
        private IEnumerable<Vendors> BuildVendors2(IEnumerable<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors> sources,
            IEnumerable<Ellucian.Colleague.Data.Base.DataContracts.Person> persons,
            IEnumerable<Ellucian.Colleague.Data.Base.DataContracts.Corp> corp, IEnumerable<Ellucian.Colleague.Data.Base.DataContracts.CorpFounds> corpFounds)
        {
            var repositoryException = new RepositoryException();
            var vendorCollection = new List<Vendors>();
            var cor = new Corp();
            CorpFounds corpFound = null;
            foreach (var source in sources)
            {
                if (persons != null && persons.Any())
                {
                    var person = persons.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                    if (person == null)
                    {
                        repositoryException.AddError(
                      new RepositoryError("Bad.Data", "Person record cannot be found.")
                      {
                          Id = source.RecordGuid,
                          SourceId = source.Recordkey
                      });
                    }
                    else
                    {
                        if (corp != null && corp.Any())
                            cor = corp.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                        if (corpFounds != null && corpFounds.Any())
                            corpFound = corpFounds.FirstOrDefault(p => p.Recordkey == source.Recordkey);

                        try
                        {
                            vendorCollection.Add(BuildVendor2(source, person, cor, corpFound));
                        }
                        catch (Exception ex)
                        {
                            repositoryException.AddError(
                          new RepositoryError("Bad.Data", ex.Message)
                          {
                              Id = source.RecordGuid,
                              SourceId = source.Recordkey
                          });
                        }
                    }

                }
                else
                {
                    repositoryException.AddError(
                       new RepositoryError("Bad.Data", "Person record not found.")
                       {
                           Id = source.RecordGuid,
                           SourceId = source.Recordkey
                       });
                }
            }
            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return vendorCollection.AsEnumerable();
        }

        /// <summary>
        /// Build a vendor domain entity from data contracts
        /// </summary>
        /// <param name="source">Vendors data contract</param>
        /// <param name="person">Persons data contract</param>
        /// <param name="corp">Corp data contract</param>
        /// <returns>Vendors domain entity</returns>
        private Vendors BuildVendor(Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors source,
            Ellucian.Colleague.Data.Base.DataContracts.Person person,
            Ellucian.Colleague.Data.Base.DataContracts.Corp corp)
        {
            Vendors vendor = null;

            if (source == null)
            {
                throw new ArgumentNullException("source", "source required to build vendor.");
            }

            vendor = new Vendors(source.RecordGuid);
            vendor.Id = source.Recordkey;
            vendor.ActiveFlag = source.VenActiveFlag;
            if (source.VendorsAddDate.HasValue)
            {
                vendor.AddDate = source.VendorsAddDate;
            }
            vendor.ApTypes = source.VenApTypes;
            vendor.ApprovalFlag = source.VenApprovalFlag;
            vendor.CurrencyCode = source.VenCurrencyCode;
            vendor.Misc = source.VenMisc;
            vendor.StopPaymentFlag = source.VenStopPaymentFlag;
            vendor.Terms = source.VenTerms;
            vendor.Types = source.VenTypes;
            vendor.Categories = source.VenCategories;
            if (!string.IsNullOrWhiteSpace(source.VenComments))
            {
                vendor.Comments = source.VenComments;
            }


            if ((corp != null) && (corp.CorpParents != null && corp.CorpParents.Any()))
            {
                vendor.CorpParent = corp.CorpParents;
            }

            if (person != null)
            {
                vendor.IsOrganization = (person.PersonCorpIndicator == "Y");
            }

            if (source.VenIntgHoldReasons != null && source.VenIntgHoldReasons.Any())
            {
                vendor.IntgHoldReasons = source.VenIntgHoldReasons;
            }
            return vendor;
        }

        /// <summary>
        /// Build a vendor domain entity from data contracts
        /// </summary>
        /// <param name="source">Vendors data contract</param>
        /// <param name="person">Persons data contract</param>
        /// <param name="corp">Corp data contract</param>
        /// <returns>Vendors domain entity</returns>
        private Vendors BuildVendor2(Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors source,
            Ellucian.Colleague.Data.Base.DataContracts.Person person,
            Ellucian.Colleague.Data.Base.DataContracts.Corp corp, Ellucian.Colleague.Data.Base.DataContracts.CorpFounds corpFound)
        {
            Vendors vendor = null;

            if (source == null)
            {
                throw new ArgumentNullException("source", "source required to build vendor.");
            }

            vendor = new Vendors(source.RecordGuid);
            vendor.Id = source.Recordkey;
            vendor.ActiveFlag = source.VenActiveFlag;
            if (source.VendorsAddDate.HasValue)
            {
                vendor.AddDate = source.VendorsAddDate;
            }
            vendor.ApTypes = source.VenApTypes;
            vendor.ApprovalFlag = source.VenApprovalFlag;
            vendor.CurrencyCode = source.VenCurrencyCode;
            vendor.Misc = source.VenMisc;
            vendor.StopPaymentFlag = source.VenStopPaymentFlag;
            vendor.Terms = source.VenTerms;
            vendor.Types = source.VenTypes;
            vendor.Categories = source.VenCategories;
            vendor.TaxForm = source.VenTaxForm;
            if (!string.IsNullOrWhiteSpace(source.VenComments))
            {
                vendor.Comments = source.VenComments;
            }

            if ((corp != null) && (corp.CorpParents != null && corp.CorpParents.Any()))
            {
                vendor.CorpParent = corp.CorpParents;
            }

            if (corpFound != null)
            {
                vendor.TaxId = corpFound.CorpTaxId;
            }

            if (person != null)
            {
                vendor.IsOrganization = (person.PersonCorpIndicator == "Y");
            }

            if (source.VenIntgHoldReasons != null && source.VenIntgHoldReasons.Any())
            {
                vendor.IntgHoldReasons = source.VenIntgHoldReasons;
            }
            return vendor;
        }

        /// <summary>
        /// Build a collection of vendor domain entities from data contracts
        /// </summary>
        /// <param name="sources">Vendors data contracts</param>
        /// <param name="persons">Persons data contracst</param>
        /// <param name="corp"></param>
        /// <returns>collection of vendor domain entities</returns>
        private IEnumerable<Vendors> BuildVendorsMaximum(IEnumerable<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors> sources,
            IEnumerable<Ellucian.Colleague.Data.Base.DataContracts.Person> persons,
            IEnumerable<Ellucian.Colleague.Data.Base.DataContracts.Corp> corp, IEnumerable<Ellucian.Colleague.Data.Base.DataContracts.CorpFounds> corpFounds, IEnumerable<PersonIntg> personIntgData, IEnumerable<Base.DataContracts.Address> addressesData)
        {
            var repositoryException = new RepositoryException();
            var vendorCollection = new List<Vendors>();
            var cor = new Corp();
            CorpFounds corpFound = null;
            PersonIntg personIntg = null;
            List<Base.DataContracts.Address> addresses = null;
            foreach (var source in sources)
            {
                if (persons != null && persons.Any())
                {
                    var person = persons.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                    if (person == null)
                    {
                        repositoryException.AddError(
                      new RepositoryError("Bad.Data", "Person record cannot be found.")
                      {
                          Id = source.RecordGuid,
                          SourceId = source.Recordkey
                      });
                    }
                    else
                    {
                        if (corp != null && corp.Any())
                            cor = corp.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                        if (corpFounds != null && corpFounds.Any())
                            corpFound = corpFounds.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                        if (personIntgData != null && personIntgData.Any())
                            personIntg = personIntgData.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                        if (addressesData != null && addressesData.Any())
                            addresses = addressesData.Where(y => person.PersonAddresses.Contains(y.Recordkey)).ToList();

                        try
                        {
                            vendorCollection.Add(BuildVendorMaximum(source, person, cor, corpFound, personIntg, addresses));
                        }
                        catch (Exception ex)
                        {
                            repositoryException.AddError(
                          new RepositoryError("Bad.Data", ex.Message)
                          {
                              Id = source.RecordGuid,
                              SourceId = source.Recordkey
                          });
                        }
                    }

                }
                else
                {
                    repositoryException.AddError(
                       new RepositoryError("Bad.Data", "Person record not found.")
                       {
                           Id = source.RecordGuid,
                           SourceId = source.Recordkey
                       });
                }
            }
            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return vendorCollection.AsEnumerable();
        }


        /// <summary>
        /// Build a vendor domain entity from data contracts
        /// </summary>
        /// <param name="source">Vendors data contract</param>
        /// <param name="person">Persons data contract</param>
        /// <param name="corp">Corp data contract</param>
        /// <param name="corpFound">corpFound data contract</param>
        /// <param name="PersonIntg">PersonIntg data contract</param>
        /// <param name="addresses">list of addresses data contract</param>
        /// <returns>Vendors domain entity</returns>
        private Vendors BuildVendorMaximum(Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors source,
            Ellucian.Colleague.Data.Base.DataContracts.Person person,
            Ellucian.Colleague.Data.Base.DataContracts.Corp corp, Ellucian.Colleague.Data.Base.DataContracts.CorpFounds corpFound, PersonIntg personIntg, List<Base.DataContracts.Address> addresses)
        {
            Vendors vendor = null;
            vendor = new Vendors(source.RecordGuid);
            vendor.Id = source.Recordkey;
            vendor.ActiveFlag = source.VenActiveFlag;
            if (source.VendorsAddDate.HasValue)
            {
                vendor.AddDate = source.VendorsAddDate;
            }
            vendor.ApTypes = source.VenApTypes;
            vendor.ApprovalFlag = source.VenApprovalFlag;
            vendor.CurrencyCode = source.VenCurrencyCode;
            vendor.Misc = source.VenMisc;
            vendor.StopPaymentFlag = source.VenStopPaymentFlag;
            vendor.Terms = source.VenTerms;
            vendor.Types = source.VenTypes;
            vendor.Categories = source.VenCategories;
            vendor.TaxForm = source.VenTaxForm;
            if (!string.IsNullOrWhiteSpace(source.VenComments))
            {
                vendor.Comments = source.VenComments;
            }

            if ((corp != null) && (corp.CorpParents != null && corp.CorpParents.Any()))
            {
                vendor.CorpParent = corp.CorpParents;
            }

            if (corpFound != null)
            {
                vendor.TaxId = corpFound.CorpTaxId;
            }

            if (person != null)
            {
                vendor.IsOrganization = (person.PersonCorpIndicator == "Y");
                // Populate address and phone numbers
                var tuplePerson = GetPersonIntegrationDataAsync(source.Recordkey, person, personIntg, addresses);
                if (tuplePerson != null)
                {
                    vendor.Phones = tuplePerson.Item1;
                    vendor.Addresses = tuplePerson.Item2;
                }
            }

            if (source.VenIntgHoldReasons != null && source.VenIntgHoldReasons.Any())
            {
                vendor.IntgHoldReasons = source.VenIntgHoldReasons;
            }
            return vendor;
        }

        /// <summary>
        /// Get person addresses, email addresses and phones used for integration.
        /// </summary>
        /// <param name="personId">Person's Colleague ID</param>
        /// <param name="emailAddresses">List of <see cref="EmailAddress"> email addresses</see></param>
        /// <param name="phones">List of <see cref="Phone"> phones</see></param>
        /// <param name="addresses">List of <see cref="Address">addresses</see></param>
        /// <returns>Boolean where true is success and false otherwise</returns>
        private Tuple<List<Phone>, List<Domain.Base.Entities.Address>, bool> GetPersonIntegrationDataAsync(string personId,
            Base.DataContracts.Person personData, PersonIntg personIntgData, List<Base.DataContracts.Address> addressData)
        {
            List<Domain.Base.Entities.Address> addresses = new List<Domain.Base.Entities.Address>();
            List<Phone> phones = new List<Phone>();
            //get person phones.
            if (personData != null)
            {
                foreach (var phone in personData.PerphoneEntityAssociation)
                {
                    try
                    {
                        var phoneNumber = phone.PersonalPhoneNumberAssocMember;
                        var phoneType = phone.PersonalPhoneTypeAssocMember;
                        var phoneExt = phone.PersonalPhoneExtensionAssocMember;
                        bool isPreferred = false;
                        string countryCallingCode = null;
                        if (personIntgData != null && personIntgData.PerIntgPhonesEntityAssociation != null && personIntgData.PerIntgPhonesEntityAssociation.Any())
                        {
                            var matchingPhone = personIntgData.PerIntgPhonesEntityAssociation.FirstOrDefault(pi => pi.PerIntgPhoneNumberAssocMember == phoneNumber);
                            if (matchingPhone != null)
                            {
                                isPreferred = (!string.IsNullOrEmpty(matchingPhone.PerIntgPhonePrefAssocMember) ? matchingPhone.PerIntgPhonePrefAssocMember.Equals("Y", StringComparison.OrdinalIgnoreCase) : false);
                                countryCallingCode = matchingPhone.PerIntgCtryCallingCodeAssocMember;
                            }
                        }
                        phones.Add(new Phone(phoneNumber, phoneType, phoneExt)
                        {
                            CountryCallingCode = countryCallingCode,
                            IsPreferred = isPreferred
                        });
                    }
                    catch (Exception exception)
                    {
                        throw new RepositoryException(string.Format(exception.Message + ".  Could not load phone number for person id '{0}' with GUID '{1}'", personData.Recordkey, personData.RecordGuid));
                    }
                }
            }

            if (personData != null && addressData != null && personData.PersonAddresses != null && personData.PersonAddresses.Any())
            {
                // Current Addresses
                var addressIds = personData.PersonAddresses;
                var addressDataContracts = addressData.Where(ad => addressIds.Contains(ad.Recordkey));
                if (addressDataContracts.Any())
                {
                    // create the address entities
                    foreach (var address in addressDataContracts)
                    {
                        var addressEntity = new Domain.Base.Entities.Address();
                        addressEntity.Guid = address.RecordGuid;
                        addressEntity.City = address.City;
                        addressEntity.State = address.State;
                        addressEntity.PostalCode = address.Zip;
                        addressEntity.CountryCode = address.Country;
                        addressEntity.County = address.County;
                        addressEntity.AddressLines = address.AddressLines;
                        addressEntity.IntlLocality = address.IntlLocality;
                        addressEntity.IntlPostalCode = address.IntlPostalCode;
                        addressEntity.IntlRegion = address.IntlRegion;
                        addressEntity.IntlSubRegion = address.IntlSubRegion;
                        // Find Addrel Association in Person contract
                        var assocEntity = personData.PseasonEntityAssociation.FirstOrDefault(pa => address.Recordkey == pa.PersonAddressesAssocMember);
                        if (assocEntity != null)
                        {
                            //addressEntity.TypeCode = assocEntity.AddrTypeAssocMember.Split(_SM).FirstOrDefault();
                            addressEntity.TypeCode = assocEntity.AddrTypeAssocMember;
                            addressEntity.EffectiveStartDate = assocEntity.AddrEffectiveStartAssocMember;
                            addressEntity.EffectiveEndDate = assocEntity.AddrEffectiveEndAssocMember;

                        }
                        addressEntity.IsPreferredAddress = (address.Recordkey == personData.PreferredAddress);
                        addressEntity.IsPreferredResidence = (address.Recordkey == personData.PreferredResidence);
                        addressEntity.Status = "Current";
                        //add address phones if addresslines is there otherwise at it to the phone list. 

                        if (address.AdrPhonesEntityAssociation != null && address.AdrPhonesEntityAssociation.Any())
                        {
                            foreach (var phone in address.AdrPhonesEntityAssociation)
                            {
                                try
                                {
                                    bool isPreferred = false;
                                    string countryCallingCode = null;
                                    if (personIntgData != null && personIntgData.PerIntgPhonesEntityAssociation != null && personIntgData.PerIntgPhonesEntityAssociation.Any())
                                    {
                                        var matchingPhone = personIntgData.PerIntgPhonesEntityAssociation.FirstOrDefault(pi => pi.PerIntgPhoneNumberAssocMember == phone.AddressPhonesAssocMember);
                                        if (matchingPhone != null)
                                        {
                                            isPreferred = (!string.IsNullOrEmpty(matchingPhone.PerIntgPhonePrefAssocMember) ? matchingPhone.PerIntgPhonePrefAssocMember.Equals("Y", StringComparison.OrdinalIgnoreCase) : false);
                                            countryCallingCode = matchingPhone.PerIntgCtryCallingCodeAssocMember;
                                        }
                                    }
                                    if (addressEntity.AddressLines != null && addressEntity.AddressLines.Any())
                                    {
                                        addressEntity.AddPhone(new Phone(phone.AddressPhonesAssocMember, phone.AddressPhoneTypeAssocMember, phone.AddressPhoneExtensionAssocMember)

                                        {
                                            CountryCallingCode = countryCallingCode,
                                            IsPreferred = isPreferred
                                        });
                                    }
                                    else
                                    {
                                        phones.Add(new Phone(phone.AddressPhonesAssocMember, phone.AddressPhoneTypeAssocMember, phone.AddressPhoneExtensionAssocMember)
                                        {
                                            CountryCallingCode = countryCallingCode,
                                            IsPreferred = isPreferred
                                        });
                                    }
                                }
                                catch (Exception exception)
                                {
                                    throw new RepositoryException(string.Format(exception.Message + "Could not load address phone number for person id '{0}' with GUID '{1}'", personData.Recordkey, personData.RecordGuid));
                                }
                            }
                        }

                        addresses.Add(addressEntity);
                    }
                }
            }

            return new Tuple<List<Phone>, List<Domain.Base.Entities.Address>, bool>(phones, addresses, true);
        }

        /// <summary>
        /// Create an CreateUpdateVendorRequest from a Vendor domain entity
        /// </summary>
        /// <param name="vendorEntity">Vendor domain entity</param>
        /// <returns>CreateUpdateVendorRequest transaction object</returns>
        private async Task<CreateUpdateVendorRequest> BuildVendorsUpdateRequestAsync(Vendors vendorEntity)
        {
            var request = new CreateUpdateVendorRequest();

            if (vendorEntity.IsOrganization)
            {
                request.OrgId = vendorEntity.Id;
            }
            else
            {
                var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>(vendorEntity.Id);
                if (personRecord == null)
                {
                    throw new KeyNotFoundException(string.Concat("Record not found for person with ID ", vendorEntity.Id, "invalid."));
                }
                if (personRecord.PersonCorpIndicator == "Y")
                {
                    request.InstitutionId = vendorEntity.Id;
                }
                else
                {
                    request.PersonId = vendorEntity.Id;
                }
            }

            request.ClassificationsId = vendorEntity.Types;
            request.Comments = vendorEntity.Comments;
            request.DefaultCurrency = vendorEntity.CurrencyCode;

            request.PaymentSourcesId = vendorEntity.ApTypes;
            request.PaymentTermsId = vendorEntity.Terms;

            var vendorStatuses = new List<string>();
            if (vendorEntity.ActiveFlag == "Y")
                vendorStatuses.Add("active");
            if (vendorEntity.StopPaymentFlag == "Y")
                vendorStatuses.Add("holdPayment");
            if (vendorEntity.ApprovalFlag == "Y")
                vendorStatuses.Add("approved");

            request.Statuses = vendorStatuses;
            request.VendorGuid = vendorEntity.Guid;

            if (vendorEntity.IntgHoldReasons != null && vendorEntity.IntgHoldReasons.Any())
                request.VendorHoldReasonsId = vendorEntity.IntgHoldReasons;

            request.VendorTypes = vendorEntity.Categories;
            request.TaxForm = vendorEntity.TaxForm;
            request.TaxId = vendorEntity.TaxId;

            //request.VendorId = vendorEntity.Id;
            return request;
        }

        /// <summary>
        /// Using a collection of endor ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="vendorIds">collection of vendor ids</param>
        /// <returns>Dictionary consisting of a vendor (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetVendorGuidsCollectionAsync(IEnumerable<string> vendorIds)
        {
            if ((vendorIds == null) || (vendorIds != null && !vendorIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var vendorGuidCollection = new Dictionary<string, string>();

            var vendorGuidLookup = vendorIds
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList()
                .ConvertAll(p => new RecordKeyLookup("VENDORS", p, false)).ToArray();
            var recordKeyLookupResults = await DataReader.SelectAsync(vendorGuidLookup);
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!vendorGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        vendorGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
                catch (Exception) // Do not throw error.
                {
                }
            }

            return vendorGuidCollection;
        }

    }
}