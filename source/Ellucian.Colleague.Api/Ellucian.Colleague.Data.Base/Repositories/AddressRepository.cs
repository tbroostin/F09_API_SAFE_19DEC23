// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class AddressRepository : BaseColleagueRepository, IAddressRepository
    {
        private Data.Base.DataContracts.IntlParams internationalParameters;
        private RepositoryException exception;

        // setting for AllAddresses Cache
        const string AllAddressesCache = "AllAddresses";
        const int AllAddressesCacheTimeout = 20; // Clear from cache every 20 minutes


        public AddressRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
        }

        #region ValidationTables
        private IEnumerable<Country> GetCountries()
        {
            return GetCodeItem<Ellucian.Colleague.Data.Base.DataContracts.Countries, Country>("AllCountries", "COUNTRIES",
                d => new Country(d.Recordkey, d.CtryDesc, d.CtryIsoCode, d.CtryIsoAlpha3Code));
        }

        private async Task<IEnumerable<Country>> GetCountriesAsync()
        {
            return await GetCodeItemAsync<Ellucian.Colleague.Data.Base.DataContracts.Countries, Country>("AllCountries", "COUNTRIES",
                d => new Country(d.Recordkey, d.CtryDesc, d.CtryIsoCode, d.CtryIsoAlpha3Code));
        }
        private ApplValcodes GetAddressRelationships()
        {

            return GetOrAddToCache<ApplValcodes>("AddressRelationships",
                () =>
                {
                    ApplValcodes relationshipTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES");
                    if (relationshipTable == null)
                    {
                        var errorMessage = "Unable to access ADREL.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return relationshipTable;
                }, Level1CacheTimeoutValue);
        }

        private async Task<ApplValcodes> GetAddressRelationshipsAsync()
        {

            return await GetOrAddToCacheAsync<ApplValcodes>("AddressRelationships",
                async () =>
                {
                    ApplValcodes relationshipTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES");
                    if (relationshipTable == null)
                    {
                        var errorMessage = "Unable to access ADREL.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return relationshipTable;
                }, Level1CacheTimeoutValue);
        }

        private ApplValcodes GetPhoneTypes()
        {

            return GetOrAddToCache<ApplValcodes>("PhoneTypes",
                () =>
                {
                    ApplValcodes phoneTypesTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES");
                    if (phoneTypesTable == null)
                    {
                        var errorMessage = "Unable to access PHONE.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return phoneTypesTable;
                }, Level1CacheTimeoutValue);
        }

        private async Task<ApplValcodes> GetPhoneTypesAsync()
        {

            return await GetOrAddToCacheAsync<ApplValcodes>("PhoneTypes",
                async () =>
                {
                    ApplValcodes phoneTypesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES");
                    if (phoneTypesTable == null)
                    {
                        var errorMessage = "Unable to access PHONE.TYPES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return phoneTypesTable;
                }, Level1CacheTimeoutValue);
        }


        /// <summary>
        /// Using a collection of zip code ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="zipCodeIds">collection of zipCode ids</param>
        /// <returns>Dictionary consisting of a zipCodeIds (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetZipCodeGuidsCollectionAsync(IEnumerable<string> zipCodeIds)
        {
            if ((zipCodeIds == null) || (zipCodeIds != null && !zipCodeIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var zipGuidCollection = new Dictionary<string, string>();
            try
            {
                // First build a lookup with 5 character US keys or Canadian keys with spaces removed.
                var zipGuidLookup = zipCodeIds
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup("ZIP.CODE.XLAT", p.Split('-')[0].ToString().Replace(" ", ""), false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(zipGuidLookup);

                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    if (recordKeyLookupResult.Value != null)
                    {
                        var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                        if (!zipGuidCollection.ContainsKey(splitKeys[1]))
                        {
                            zipGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                        }
                    }
                }
                // Next build a lookup using a 3 character foreign key (non-US, non-Canadian).
                zipGuidLookup = zipCodeIds
                    .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length >= 3)
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup("ZIP.CODE.XLAT", p.Length >= 3 ? p.Substring(0, 3) : p, false)).ToArray();
                recordKeyLookupResults = await DataReader.SelectAsync(zipGuidLookup);

                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    if (recordKeyLookupResult.Value != null)
                    {
                        var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                        if (!zipGuidCollection.ContainsKey(splitKeys[1]))
                        {
                            zipGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error occured while getting guids for zip codes.", ex); ;
            }

            return zipGuidCollection;
        }

        /// <summary>
        /// Read the international parameters records to extract date format used
        /// locally and setup in the INTL parameters.
        /// </summary>
        /// <returns>International Parameters with date properties</returns>
        private async new Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> GetInternationalParametersAsync()
        {
            if (internationalParameters != null)
            {
                return internationalParameters;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            internationalParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
                async () =>
                {
                    Data.Base.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                    if (intlParams == null)
                    {
                        var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                        logger.Info(errorMessage);
                        // If we cannot read the international parameters default to US with a / delimiter.
                        // throw new ColleagueWebApiException(errorMessage);
                        Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
                        newIntlParams.HostShortDateFormat = "MDY";
                        newIntlParams.HostDateDelimiter = "/";
                        newIntlParams.HostCountry = "USA";
                        intlParams = newIntlParams;
                    }
                    return intlParams;
                }, Level1CacheTimeoutValue);
            return internationalParameters;
        }

        public async Task<string> GetHostCountryAsync()
        {
            var intlParams = await GetInternationalParametersAsync();
            return intlParams.HostCountry;
        }

        #endregion

        #region ByPerson
        /// <summary>
        /// Get the Preferred Address and Preferred Residence for a single Person
        /// </summary>
        /// <param name="personId">Person Key</param>
        /// <returns>Address Objects for preferred address and preferred residence</returns>
        public IEnumerable<Address> GetPersonAddresses(string personId)
        {
            List<Address> addresses = new List<Address>();

            Ellucian.Colleague.Data.Base.DataContracts.Person person = DataReader.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", personId);
            if (person == null)
            {
                throw new ArgumentOutOfRangeException("Person Id " + personId + " is not returning any data. Person may be corrupted.");
            }
            string[] addressIds = person.PersonAddresses.ToArray();
            ICollection<Ellucian.Colleague.Data.Base.DataContracts.Address> addressesData = DataReader.BulkReadRecord<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", addressIds);

            if (addressesData == null)
            {
                throw new ArgumentOutOfRangeException("Person Id " + personId + " is not returning address data.  Person or Address may be corrupted.");
            }
            foreach (var addressData in addressesData)
            {
                var addressId = addressData.Recordkey;
                Address address = new Address(addressId, personId);

                try
                {
                    address = BuildAddress(addressData, person);
                    var phoneNumber = BuildPhones(addressData, person);
                    if (phoneNumber.PhoneNumbers.Count() > 0)
                    {
                        foreach (var phone in phoneNumber.PhoneNumbers)
                        {
                            address.AddPhone(phone);
                        }
                    }
                    addresses.Add(address);
                }
                catch (Exception ex)
                {
                    /// Don't do anything, just skip this address
                    logger.Error(ex.Message, "Cannot process address");
                }
            }
            return addresses;
        }

        /// <summary>
        /// Get the Preferred Address and Preferred Residence for a single Person
        /// </summary>
        /// <param name="personId">Person Key</param>
        /// <returns>Address Objects for preferred address and preferred residence</returns>
        public async Task<IEnumerable<Address>> GetPersonAddressesAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person Id is required to retrieve the person's addresses.");
            }
            List<Address> addresses = new List<Address>();
            Ellucian.Colleague.Data.Base.DataContracts.Person person = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", personId);
            if (person == null)
            {
                throw new ArgumentOutOfRangeException("Person Id " + personId + " is not returning any data. Person may be corrupted.");
            }
            string[] addressIds = person.PersonAddresses.ToArray();
            ICollection<Ellucian.Colleague.Data.Base.DataContracts.Address> addressesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>("ADDRESS", addressIds);

            if (addressesData == null)
            {
                throw new ArgumentOutOfRangeException("Person Id " + personId + " is not returning address data.  Person or Address may be corrupted.");
            }
            foreach (var addressData in addressesData)
            {
                var addressId = addressData.Recordkey;
                Address address = new Address(addressId, personId);

                try
                {
                    address = await BuildAddressAsync(addressData, person);
                    var phoneNumber = await BuildPhonesAsync(addressData, person);
                    if (phoneNumber.PhoneNumbers.Count() > 0)
                    {
                        foreach (var phone in phoneNumber.PhoneNumbers)
                        {
                            address.AddPhone(phone);
                        }
                    }
                    addresses.Add(address);
                }
                catch (Exception ex)
                {
                    /// Don't do anything, just skip this address
                    logger.Error(ex, "Cannot process address");
                }
            }
            return addresses;
        }

        /// <summary>
        /// Get a preferred address and preferred residence for a list of person keys
        /// </summary>
        /// <param name="personIds"></param>
        /// <returns>Address Objects for preferred address and preferred residence</returns>
        public IEnumerable<Address> GetPersonAddressesByIds(List<string> personIds)
        {
            List<Address> addresses = new List<Address>();
            ICollection<DataContracts.Address> addressesData = new Collection<DataContracts.Address>();
            ICollection<DataContracts.Person> personData = new Collection<DataContracts.Person>();
            bool error = false;

            personData = DataReader.BulkReadRecord<DataContracts.Person>("PERSON", personIds.ToArray());
            if (personData != null && personData.Count > 0)
            {
                addressesData = DataReader.BulkReadRecord<DataContracts.Address>("ADDRESS", personData.SelectMany(p => p.PersonAddresses).Distinct().ToArray());
            }
            else
            {
                logger.Error(string.Format("Selection of Person Data did not return any results from the list of input Ids: {0}", string.Join(",", personIds)));
            }

            if (addressesData == null || addressesData.Count <= 0)
            {
                // Simply return without exception when no addresses exist for the group of persons.
                logger.Error(string.Format("Selection of Address Data did not return any results from the list of input Ids: {0}", string.Join(",", personIds)));
            }
            else
            {
                foreach (var person in personData)
                {
                    var personId = person.Recordkey;
                    foreach (var addressId in person.PersonAddresses)
                    {
                        // Check for Address ID in case we have invalid/null address keys in the list.
                        // srm - 09/08/2014 (Found while testing with Contra Costa database).
                        if (!string.IsNullOrEmpty(addressId))
                        {
                            var addressData = addressesData.Where(a => a.Recordkey == addressId).FirstOrDefault();
                            Address address = new Address(addressId, personId);
                            try
                            {
                                address = BuildAddress(addressData, person);
                                var phoneNumber = BuildPhones(addressData, person);
                                if (phoneNumber.PhoneNumbers.Count() > 0)
                                {
                                    foreach (var phone in phoneNumber.PhoneNumbers)
                                    {
                                        address.AddPhone(phone);
                                    }
                                }
                                addresses.Add(address);
                            }
                            catch (Exception e)
                            {
                                /// Just skip this address and log it.
                                logger.Error("Failed to build address. PersonId: " + personId + " AddressId: " + addressId);
                                logger.Error(e.GetBaseException().Message);
                                logger.Error(e.GetBaseException().StackTrace);
                                error = true;
                            }
                        }
                    }
                }
            }
            if (error && addresses.Count() == 0)
                throw new ColleagueWebApiException("Unexpected errors occurred. No address records returned. Check API error log.");

            return addresses;
        }
        #endregion

        #region Places
        /// <summary>
        /// Get a Place 
        /// </summary>
        /// <returns>A collection of Place entities</returns>
        public async Task<IEnumerable<Place>> GetPlacesAsync()
        {
            var places = await GetOrAddToCacheAsync<List<Place>>("AllPlaces",
             async () =>
             {
                 Collection<DataContracts.Places> placeData = await DataReader.BulkReadRecordAsync<DataContracts.Places>("PLACES", "");
                 var placesList = BuildPlaces(placeData.ToList());
                 return placesList.ToList();
             }
          );
            return places;
        }

        /// <summary>
        /// Build a collection of Place domain entities from a Place datacontract collection
        /// </summary>
        /// <param name="placeData">place data contract</param>
        /// <returns>Collection of Place domain entities</returns>
        private IEnumerable<Place> BuildPlaces(List<DataContracts.Places> placeData)
        {
            List<Place> placeCollection = new List<Place>();
            if (placeData != null)
            {
                foreach (var place in placeData)
                {
                    try
                    {
                        var placeItem = new Place();
                        placeItem.PlacesCountry = place.PlacesCountry;
                        placeItem.PlacesDesc = place.PlacesDesc;
                        placeItem.PlacesRegion = place.PlacesRegion;
                        placeItem.PlacesSubRegion = place.PlacesSubRegion;
                        placeCollection.Add(placeItem);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Place", place.Recordkey, null, ex);
                    }
                }
            }
            return placeCollection;
        }
        #endregion

        #region Address GET Methods

        /// <summary>
        /// Get an Address entity from guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Address entity</returns>
        public async Task<Address> GetAddressAsync(string guid)
        {
            var addressId = await GetAddressFromGuidAsync(guid);
            if (string.IsNullOrEmpty(addressId))
            {
                var errorMessage = "Unable to locate address from guid.";
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }
            return await GetAddressbyIDAsync(addressId);
        }

        /// <summary>
        /// Get an Address entity from guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Address entity</returns>
        public async Task<Address> GetAddress2Async(string guid)
        {
            var addressId = string.Empty;
            Address address = null;
            try
            {
                addressId = await GetAddressFromGuidAsync(guid);
                if (string.IsNullOrEmpty(addressId))
                {
                    var errorMessage = string.Format("Unable to locate address from guid '{0}'.", guid);
                    logger.Error(errorMessage);
                    var exception = new RepositoryException();
                    exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                    throw exception;
                }

                address = await GetAddressbyID2Async(addressId);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }

            return address;
        }

        /// <summary>
        /// Get a single address using an ID
        /// </summary>
        /// <param name="id">The address ID</param>
        /// <returns>The address</returns>
        public async Task<Address> GetAddressbyIDAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get an address.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<DataContracts.Address>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or address with ID ", id, " invalid."));
            }

            // Build the address data
            return BuildAddress(record);
        }

        /// <summary>
        /// Get a single address using an ID
        /// </summary>
        /// <param name="id">The address ID</param>
        /// <returns>The address</returns>
        public async Task<Address> GetAddressbyID2Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get an address.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<DataContracts.Address>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or address with ID ", id, " invalid."));
            }

            // Build the address data
            return BuildAddress2(record);
        }


        /// <summary>
        /// Get the GUID for an Address using its ID
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <returns>Address GUID</returns>
        public async Task<string> GetAddressGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("ADDRESS", id);
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError(string.Concat("GUID not found for address ", id)));
                throw ex;
            }
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetAddressFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Address GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Address GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "ADDRESS")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, ADDRESS", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get all addresses with Paging options
        /// </summary>
        /// <param name="offset">The starting point to look at the list</param>
        /// <param name="limit">The amount of records to view</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Address>, int>> GetAddressesAsync(int offset, int limit)
        {
            string criteria = "WITH ADDRESS.LINES NE ''";

            string[] addressIds = await DataReader.SelectAsync("ADDRESS", criteria);
            var totalCount = addressIds.Count();

            Array.Sort(addressIds);

            var subList = addressIds.Skip(offset).Take(limit).ToArray();
            var Addresses = await DataReader.BulkReadRecordAsync<DataContracts.Address>("ADDRESS", subList);
            {
                if (Addresses == null)
                {
                    throw new KeyNotFoundException("No records selected from Address in Colleague.");
                }
            }

            var AddressEntities = new List<Address>();
            foreach (var addressEntity in Addresses)
            {
                AddressEntities.Add(BuildAddress(addressEntity));
            }
            return new Tuple<IEnumerable<Address>, int>(AddressEntities, totalCount);

        }


        /// <summary>
        /// Get all addresses with Paging options
        /// </summary>
        /// <param name="offset">The starting point to look at the list</param>
        /// <param name="limit">The amount of records to view</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Address>, int>> GetAddresses2Async(int offset, int limit, string[] filterPersonIds)
        {
            List<string> limitingKeys = null;
            int totalCount = 0;
            string[] subList = null;

            var addressEntities = new List<Address>();
            try
            {
                string addressesCacheKey = CacheSupport.BuildCacheKey(AllAddressesCache, filterPersonIds);
                var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    addressesCacheKey,
                    "ADDRESS",
                    offset,
                    limit,
                    AllAddressesCacheTimeout,
                    async () =>
                    {

                        if (filterPersonIds != null && filterPersonIds.ToList().Any())
                        {
                            // Set limiting keys to previously retrieved personIds from SAVE.LIST.PARMS
                            //limitingKeys = filterPersonIds;
                            var criteria = "WITH PERSON.ADDRESSES NE '' BY.EXP PERSON.ADDRESSES SAVING PERSON.ADDRESSES";
                            limitingKeys = (await DataReader.SelectAsync("PERSON", filterPersonIds, criteria)).ToList();
                        }

                        CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                        {
                            limitingKeys = limitingKeys != null && limitingKeys.Any() ? limitingKeys.Distinct().ToList() : null,
                            //criteria = "WITH ADDRESS.LINES NE ''"
                        };
                        return requirements;
                    }
                );

                if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
                {
                    return new Tuple<IEnumerable<Address>, int>(addressEntities, 0);
                }
                subList = keyCacheObject.Sublist.ToArray();
                totalCount = keyCacheObject.TotalCount.Value;

                var results = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.Address>("ADDRESS", subList);

                if (results.Equals(default(BulkReadOutput<DataContracts.Address>)))
                {
                    return new Tuple<IEnumerable<Address>, int>(addressEntities, totalCount);
                }
                if (results.InvalidKeys != null || results.InvalidRecords != null)
                {
                    if (results.InvalidKeys.Any() || results.InvalidRecords.Any())
                    {
                        if (results.InvalidKeys.Any())
                        {
                            exception.AddErrors(results.InvalidKeys
                                .Select(key => new RepositoryError("invalid.key",
                                string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                        }
                        if (results.InvalidRecords.Any())
                        {
                            exception.AddErrors(results.InvalidRecords
                               .Select(r => new RepositoryError("invalid.record",
                               string.Format("Error: '{0}' ", r.Value))
                               { SourceId = r.Key }));
                        }
                        throw exception;
                    }
                }

                var addresses = results.BulkRecordsRead;

                foreach (var address in addresses)
                {
                    try
                    {
                        var addressEntity = BuildAddress2(address);
                        if (addressEntity != null)
                        {
                            addressEntities.Add(addressEntity);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        exception.AddErrors(ex.Errors);
                    }
                    catch (Exception ex)
                    {
                        exception.AddError(new RepositoryError("data.access", ex.Message)
                        {
                            Id = address.RecordGuid,
                            SourceId = address.Recordkey
                        });
                    }
                }

                if (exception.Errors.Any())
                {
                    throw exception;
                }
                return new Tuple<IEnumerable<Address>, int>(addressEntities, totalCount);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                exception.AddError(new RepositoryError("Bad.Data", e.Message));
                throw exception;
            }
        }

        /// <summary>
        /// Build an Address entity from a Address datacontract
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns>A Address Entity</returns>
        private Address BuildAddress(DataContracts.Address addressData)
        {
            Address address = new Address();
            if (addressData != null)
            {
                if (!addressData.AddressLines.Any())
                {
                    var errorMessage = string.Format("Invalid Address Record '{0}'.  Missing address Lines.", addressData.RecordGuid);
                    logger.Error("Invalid Address Record '{0}'.  Missing address lines.", addressData.Recordkey);
                    logger.Error(errorMessage);
                    var exception = new RepositoryException(errorMessage);
                    exception.AddError(new RepositoryError("invalid.AddressLines", errorMessage));
                    throw exception;
                }

                if (string.IsNullOrEmpty(addressData.RecordGuid))
                {
                    var errorMessage = "Address record with ID '" + addressData.Recordkey + "' does not have a GUID.";
                    var exception = new RepositoryException(errorMessage);
                    exception.AddError(new RepositoryError("invalid.AddressGuid", errorMessage));
                    throw exception;
                }

                address.Guid = addressData.RecordGuid;

                if (!string.IsNullOrEmpty(address.Type))
                {
                    var codeAddressRelationship = GetAddressRelationships().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == address.Type).FirstOrDefault();
                    if (codeAddressRelationship != null)
                    {
                        address.Type = codeAddressRelationship.ValExternalRepresentationAssocMember;
                    }
                }

                // Set Preferred Flags
                address.IsPreferredAddress = false;
                address.IsPreferredResidence = false;

                // If there is no translation for country, the country description carries the country code.
                string countryDesc = null;
                if (!string.IsNullOrEmpty(addressData.Country))
                {
                    var codeCountry = GetCountries().Where(v => v.Code == addressData.Country).FirstOrDefault();
                    if (codeCountry != null)
                    {
                        countryDesc = codeCountry.Description;
                    }
                    else
                    {
                        countryDesc = addressData.Country;
                    }
                }

                // Build address label
                List<string> label = new List<string>();
                if (!string.IsNullOrEmpty(address.AddressModifier))
                {
                    label.Add(address.AddressModifier);
                }
                if (addressData.AddressLines.Count > 0)
                {
                    label.AddRange(addressData.AddressLines);
                }
                string cityStatePostalCode = GetCityStatePostalCode(addressData.City, addressData.State, addressData.Zip);
                if (!String.IsNullOrEmpty(cityStatePostalCode))
                {
                    label.Add(cityStatePostalCode);
                }
                if (!String.IsNullOrEmpty(countryDesc))
                {
                    // Country name gets included in all caps
                    label.Add(countryDesc.ToUpper());
                }

                address.AddressLabel = label;
                address.AddressLines = addressData.AddressLines;
                address.City = addressData.City;
                address.State = addressData.State;
                address.PostalCode = addressData.Zip;
                address.County = addressData.County;
                address.CountryCode = addressData.Country;
                address.Country = countryDesc;
                address.RouteCode = addressData.AddressRouteCode;
                address.CarrierRoute = addressData.CarrierRoute;

                address.IntlLocality = addressData.IntlLocality;
                address.IntlPostalCode = addressData.IntlPostalCode;
                address.IntlRegion = addressData.IntlRegion;
                address.IntlSubRegion = addressData.IntlSubRegion;

                address.CorrectionDigit = addressData.CorrectionDigit;
                address.DeliveryPoint = addressData.DeliveryPoint;
                address.Latitude = addressData.Latitude;
                address.Longitude = addressData.Longitude;

                address.AddressChapter = addressData.AddressChapter;
            }
            return address;
        }

        /// <summary>
        /// Build an Address entity from a Address datacontract
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns>An Address Entity</returns>
        private Address BuildAddress2(DataContracts.Address addressData)
        {

            if (addressData == null)
                return new Address();

            var repositoryException = new RepositoryException();

            /*
            if (!addressData.AddressLines.Any())
            {
                var errorMessage = string.Format("Invalid Address Record '{0}'.  Missing address Lines.", addressData.RecordGuid);
                logger.Error("Invalid Address Record '{0}'.  Missing address lines.", addressData.Recordkey);
                logger.Error(errorMessage);

                repositoryException.AddError(new RepositoryError("invalid.AddressLines", errorMessage)
                {
                    SourceId = addressData.Recordkey,
                    Id = addressData.RecordGuid
                });
            }
            */

            if (string.IsNullOrEmpty(addressData.RecordGuid))
            {
                var errorMessage = "Address record with ID '" + addressData.Recordkey + "' does not have a GUID.";
                repositoryException.AddError(new RepositoryError("invalid.AddressGuid", errorMessage)
                {
                    SourceId = addressData.Recordkey
                });
            }

            Address address = null;
            try
            {
                address = new Address(addressData.Recordkey, false);

                address.Guid = addressData.RecordGuid;

                if (!string.IsNullOrEmpty(address.Type))
                {
                    var codeAddressRelationship = GetAddressRelationships().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == address.Type).FirstOrDefault();
                    if (codeAddressRelationship != null)
                    {
                        address.Type = codeAddressRelationship.ValExternalRepresentationAssocMember;
                    }
                }

                // Set Preferred Flags              
                address.IsPreferredResidence = false;

                // If there is no translation for country, the country description carries the country code.
                string countryDesc = null;
                if (!string.IsNullOrEmpty(addressData.Country))
                {
                    var codeCountry = GetCountries().Where(v => v.Code == addressData.Country).FirstOrDefault();
                    if (codeCountry != null)
                    {
                        countryDesc = codeCountry.Description;
                    }
                    else
                    {
                        countryDesc = addressData.Country;
                    }
                }

                // Build address label
                var label = new List<string>();
                if (!string.IsNullOrEmpty(address.AddressModifier))
                {
                    label.Add(address.AddressModifier);
                }
                if (addressData.AddressLines != null && addressData.AddressLines.Count > 0)
                {
                    label.AddRange(addressData.AddressLines);
                }
                var cityStatePostalCode = GetCityStatePostalCode(addressData.City, addressData.State, addressData.Zip);
                if (!string.IsNullOrEmpty(cityStatePostalCode))
                {
                    label.Add(cityStatePostalCode);
                }
                if (!string.IsNullOrEmpty(countryDesc))
                {
                    // Country name gets included in all caps
                    label.Add(countryDesc.ToUpper());
                }

                address.AddressLabel = label;
                address.AddressLines = addressData.AddressLines;
                address.City = addressData.City;
                address.State = addressData.State;
                address.PostalCode = addressData.Zip;
                address.County = addressData.County;
                address.CountryCode = addressData.Country;
                address.Country = countryDesc;
                address.RouteCode = addressData.AddressRouteCode;
                address.CarrierRoute = addressData.CarrierRoute;

                address.IntlLocality = addressData.IntlLocality;
                address.IntlPostalCode = addressData.IntlPostalCode;
                address.IntlRegion = addressData.IntlRegion;
                address.IntlSubRegion = addressData.IntlSubRegion;

                address.CorrectionDigit = addressData.CorrectionDigit;
                address.DeliveryPoint = addressData.DeliveryPoint;
                address.Latitude = addressData.Latitude;
                address.Longitude = addressData.Longitude;

                address.AddressChapter = addressData.AddressChapter;

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                repositoryException.AddError(new RepositoryError("invalid.Address", ex.Message)
                {
                    SourceId = addressData.Recordkey,
                    Id = addressData.RecordGuid
                });
            }
            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return address;
        }


        private Address BuildAddress(Ellucian.Colleague.Data.Base.DataContracts.Address addressData, Ellucian.Colleague.Data.Base.DataContracts.Person person)
        {
            Address address = new Address("NEW", person.Recordkey);
            if (addressData != null)
            {
                var addressId = addressData.Recordkey;
                var personId = person.Recordkey;
                address = new Address(addressId, personId);

                // Update Person Data fields
                var assoc = person.PseasonEntityAssociation.Where(r => r.PersonAddressesAssocMember == addressId).FirstOrDefault();

                //if multiple address types are specified then 
                //deal with it separately
                if (!string.IsNullOrEmpty(assoc.AddrTypeAssocMember) && assoc.AddrTypeAssocMember.Contains(DmiString._SM))
                {
                    string[] addressTypeCodes = assoc.AddrTypeAssocMember.Split(DmiString._SM);
                    foreach (var typeCode in addressTypeCodes)
                    {
                        address.AddressTypeCodes.Add(typeCode);
                    }
                }

                address.TypeCode = assoc.AddrTypeAssocMember;
                if (!address.AddressTypeCodes.Any())
                {
                    address.AddressTypeCodes.Add(address.TypeCode);
                }
                address.Type = assoc.AddrTypeAssocMember;
                if (!string.IsNullOrEmpty(address.Type))
                {
                    var codeAddressRelationship = GetAddressRelationships().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == address.Type).FirstOrDefault();
                    if (codeAddressRelationship != null)
                    {
                        address.Type = codeAddressRelationship.ValExternalRepresentationAssocMember;
                    }
                }
                address.AddressModifier = assoc.AddrModifierLineAssocMember;
                address.EffectiveStartDate = assoc.AddrEffectiveStartAssocMember;
                address.EffectiveEndDate = assoc.AddrEffectiveEndAssocMember;

                // Set Preferred Flags
                address.IsPreferredAddress = false;
                address.IsPreferredResidence = false;
                if (addressId == person.PreferredAddress) { address.IsPreferredAddress = true; }
                if (addressId == person.PreferredResidence) { address.IsPreferredResidence = true; }

                // If there is no translation for country, the country description carries the country code.
                string countryDesc = null;
                if (!string.IsNullOrEmpty(addressData.Country))
                {
                    var codeCountry = GetCountries().Where(v => v.Code == addressData.Country).FirstOrDefault();
                    if (codeCountry != null)
                    {
                        countryDesc = codeCountry.Description;
                    }
                    else
                    {
                        countryDesc = addressData.Country;
                    }
                }

                // Build address label
                List<string> label = new List<string>();
                if (!string.IsNullOrEmpty(address.AddressModifier))
                {
                    label.Add(address.AddressModifier);
                }
                if (addressData.AddressLines.Count > 0)
                {
                    label.AddRange(addressData.AddressLines);
                }
                string cityStatePostalCode = GetCityStatePostalCode(addressData.City, addressData.State, addressData.Zip);
                if (!String.IsNullOrEmpty(cityStatePostalCode))
                {
                    label.Add(cityStatePostalCode);
                }
                if (!String.IsNullOrEmpty(countryDesc))
                {
                    // Country name gets included in all caps
                    label.Add(countryDesc.ToUpper());
                }

                address.AddressLabel = label;
                address.AddressLines = addressData.AddressLines;
                address.City = addressData.City;
                address.State = addressData.State;
                address.PostalCode = addressData.Zip;
                address.County = addressData.County;
                address.CountryCode = addressData.Country;
                address.Country = countryDesc;
                address.RouteCode = addressData.AddressRouteCode;
                address.CarrierRoute = addressData.CarrierRoute;

                address.IntlLocality = addressData.IntlLocality;
                address.IntlPostalCode = addressData.IntlPostalCode;
                address.IntlRegion = addressData.IntlRegion;
                address.IntlSubRegion = addressData.IntlSubRegion;

                address.CorrectionDigit = addressData.CorrectionDigit;
                address.DeliveryPoint = addressData.DeliveryPoint;
                address.Latitude = addressData.Latitude;
                address.Longitude = addressData.Longitude;

                address.AddressChapter = addressData.AddressChapter;
            }
            return address;
        }

        private async Task<Address> BuildAddressAsync(Ellucian.Colleague.Data.Base.DataContracts.Address addressData, Ellucian.Colleague.Data.Base.DataContracts.Person person)
        {
            Address address = new Address("NEW", person.Recordkey);
            if (addressData != null)
            {
                var addressId = addressData.Recordkey;
                var personId = person.Recordkey;
                address = new Address(addressId, personId);

                // Update Person Data fields
                var assoc = person.PseasonEntityAssociation.Where(r => r.PersonAddressesAssocMember == addressId).FirstOrDefault();

                //if multiple address types are specified then 
                //deal with it separately
                if (!string.IsNullOrEmpty(assoc.AddrTypeAssocMember) && assoc.AddrTypeAssocMember.Contains(DmiString._SM))
                {
                    string[] addressTypeCodes = assoc.AddrTypeAssocMember.Split(DmiString._SM);
                    foreach (var typeCode in addressTypeCodes)
                    {
                        address.AddressTypeCodes.Add(typeCode);
                    }
                }

                address.TypeCode = assoc.AddrTypeAssocMember;
                if (!address.AddressTypeCodes.Any())
                {
                    address.AddressTypeCodes.Add(address.TypeCode);
                }
                address.Type = assoc.AddrTypeAssocMember;
                if (!string.IsNullOrEmpty(address.Type))
                {
                    var codeAddressRelationship = (await GetAddressRelationshipsAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == address.Type).FirstOrDefault();
                    if (codeAddressRelationship != null)
                    {
                        address.Type = codeAddressRelationship.ValExternalRepresentationAssocMember;
                    }
                }
                address.AddressModifier = assoc.AddrModifierLineAssocMember;
                address.EffectiveStartDate = assoc.AddrEffectiveStartAssocMember;
                address.EffectiveEndDate = assoc.AddrEffectiveEndAssocMember;

                // Set Preferred Flags
                address.IsPreferredAddress = false;
                address.IsPreferredResidence = false;
                if (addressId == person.PreferredAddress) { address.IsPreferredAddress = true; }
                if (addressId == person.PreferredResidence) { address.IsPreferredResidence = true; }

                // If there is no translation for country, the country description carries the country code.
                string countryDesc = null;
                if (!string.IsNullOrEmpty(addressData.Country))
                {
                    var codeCountry = (await GetCountriesAsync()).Where(v => v.Code == addressData.Country).FirstOrDefault();
                    if (codeCountry != null)
                    {
                        countryDesc = codeCountry.Description;
                    }
                    else
                    {
                        countryDesc = addressData.Country;
                    }
                }

                // Build address label
                List<string> label = new List<string>();
                if (!string.IsNullOrEmpty(address.AddressModifier))
                {
                    label.Add(address.AddressModifier);
                }
                if (addressData.AddressLines.Count > 0)
                {
                    label.AddRange(addressData.AddressLines);
                }
                string cityStatePostalCode = GetCityStatePostalCode(addressData.City, addressData.State, addressData.Zip);
                if (!String.IsNullOrEmpty(cityStatePostalCode))
                {
                    label.Add(cityStatePostalCode);
                }
                if (!String.IsNullOrEmpty(countryDesc))
                {
                    // Country name gets included in all caps
                    label.Add(countryDesc.ToUpper());
                }

                address.AddressLabel = label;
                address.AddressLines = addressData.AddressLines;
                address.City = addressData.City;
                address.State = addressData.State;
                address.PostalCode = addressData.Zip;
                address.County = addressData.County;
                address.CountryCode = addressData.Country;
                address.Country = countryDesc;
                address.RouteCode = addressData.AddressRouteCode;
                address.CarrierRoute = addressData.CarrierRoute;

                address.IntlLocality = addressData.IntlLocality;
                address.IntlPostalCode = addressData.IntlPostalCode;
                address.IntlRegion = addressData.IntlRegion;
                address.IntlSubRegion = addressData.IntlSubRegion;

                address.CorrectionDigit = addressData.CorrectionDigit;
                address.DeliveryPoint = addressData.DeliveryPoint;
                address.Latitude = addressData.Latitude;
                address.Longitude = addressData.Longitude;

                address.AddressChapter = addressData.AddressChapter;
            }
            return address;
        }

        /// <summary>
        /// Returns a person's non-address related phone numbers as well as the phone numbers
        /// associated to an address entity.
        /// </summary>
        /// <param name="addressData">Address Data Contract object for the address being built</param>
        /// <param name="personData">Person Data Contract object for the person at this address</param>
        /// <returns>Returns a Phone Number entity which contains all phones for person and address</returns>
        private PhoneNumber BuildPhones(Ellucian.Colleague.Data.Base.DataContracts.Address addressData, Ellucian.Colleague.Data.Base.DataContracts.Person personData)
        {
            if (personData != null)
            {
                var personId = personData.Recordkey;
                PhoneNumber phoneNumber = new PhoneNumber(personId);

                // Update Address Phone Numbers
                if (addressData != null)
                {
                    var addressId = addressData.Recordkey;
                    if (!string.IsNullOrEmpty(addressId))
                    {
                        var assoc = personData.PseasonEntityAssociation.Where(r => r.PersonAddressesAssocMember == addressId).FirstOrDefault();

                        if (!string.IsNullOrEmpty(assoc.AddrLocalPhoneAssocMember))
                        {
                            // Address Local Phones in Person data
                            // This could be subvalued so need to split on subvalue mark ASCII 252.
                            string[] localPhones = assoc.AddrLocalPhoneAssocMember.Split(DmiString._SM);
                            string[] localPhoneExts = assoc.AddrLocalExtAssocMember.Split(DmiString._SM);
                            string[] localPhoneTypes = assoc.AddrLocalPhoneTypeAssocMember.Split(DmiString._SM);
                            for (int i = 0; i < localPhones.Length; i++)
                            {
                                // Only get Address Phone numbers of type "Home"
                                var phoneType = GetPhoneTypes().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == localPhoneTypes[i]).FirstOrDefault();
                                if (phoneType != null && phoneType.ValActionCode2AssocMember == "H")
                                {
                                    try
                                    {
                                        // add in the address override phones into the person's list of phones
                                        Phone personalPhone = new Phone(localPhones[i], localPhoneTypes[i], localPhoneExts[i]);
                                        phoneNumber.AddPhone(personalPhone);
                                    }
                                    catch (Exception ex)
                                    {
                                        var phoneError = "Person local phone information is invalid.";

                                        // Log the original exception
                                        logger.Error(ex.ToString());
                                        logger.Info(phoneError);
                                    }
                                }
                            }
                        }
                        // Update Address Phone
                        if (addressData.AdrPhonesEntityAssociation != null && addressData.AdrPhonesEntityAssociation.Count > 0)
                        {
                            foreach (var addrPhone in addressData.AdrPhonesEntityAssociation)
                            {
                                // Only get Address Phone numbers of type "Home"
                                if (addrPhone != null && !string.IsNullOrEmpty(addrPhone.AddressPhoneTypeAssocMember))
                                {
                                    var phoneType = GetPhoneTypes().ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == addrPhone.AddressPhoneTypeAssocMember).FirstOrDefault();
                                    if (phoneType != null && phoneType.ValActionCode2AssocMember == "H")
                                    {
                                        try
                                        {
                                            Phone addressPhone = new Phone(addrPhone.AddressPhonesAssocMember, addrPhone.AddressPhoneTypeAssocMember, addrPhone.AddressPhoneExtensionAssocMember);
                                            phoneNumber.AddPhone(addressPhone);
                                        }
                                        catch (Exception ex)
                                        {
                                            var phoneError = "Person address phone information is invalid.";
                                            // Log the original exception
                                            logger.Error(ex.ToString());
                                            logger.Info(phoneError);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Update Personal Phone
                if (personData.PerphoneEntityAssociation != null && personData.PerphoneEntityAssociation.Count > 0)
                {
                    foreach (var phoneData in personData.PerphoneEntityAssociation)
                    {
                        try
                        {
                            Phone personalPhone = new Phone(phoneData.PersonalPhoneNumberAssocMember, phoneData.PersonalPhoneTypeAssocMember, phoneData.PersonalPhoneExtensionAssocMember);
                            phoneNumber.AddPhone(personalPhone);
                        }
                        catch (Exception ex)
                        {
                            var phoneError = "Person personal phone information is invalid.";

                            // Log the original exception
                            logger.Error(ex.ToString());
                            logger.Info(phoneError);
                        }
                    }
                }
                return phoneNumber;
            }
            return null;
        }


        // <summary>
        /// Returns a person's non-address related phone numbers as well as the phone numbers
        /// associated to an address entity.
        /// </summary>
        /// <param name="addressData">Address Data Contract object for the address being built</param>
        /// <param name="personData">Person Data Contract object for the person at this address</param>
        /// <returns>Returns a Phone Number entity which contains all phones for person and address</returns>
        private async Task<PhoneNumber> BuildPhonesAsync(Ellucian.Colleague.Data.Base.DataContracts.Address addressData, Ellucian.Colleague.Data.Base.DataContracts.Person personData)
        {
            if (personData != null)
            {
                var personId = personData.Recordkey;
                PhoneNumber phoneNumber = new PhoneNumber(personId);

                // Update Address Phone Numbers
                if (addressData != null)
                {
                    var addressId = addressData.Recordkey;
                    if (!string.IsNullOrEmpty(addressId))
                    {
                        var assoc = personData.PseasonEntityAssociation.Where(r => r.PersonAddressesAssocMember == addressId).FirstOrDefault();

                        if (!string.IsNullOrEmpty(assoc.AddrLocalPhoneAssocMember))
                        {
                            // Address Local Phones in Person data
                            // This could be subvalued so need to split on subvalue mark ASCII 252.
                            string[] localPhones = assoc.AddrLocalPhoneAssocMember.Split(DmiString._SM);
                            string[] localPhoneExts = assoc.AddrLocalExtAssocMember.Split(DmiString._SM);
                            string[] localPhoneTypes = assoc.AddrLocalPhoneTypeAssocMember.Split(DmiString._SM);
                            for (int i = 0; i < localPhones.Length; i++)
                            {
                                // Only get Address Phone numbers of type "Home"
                                var phoneType = (await GetPhoneTypesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == localPhoneTypes[i]).FirstOrDefault();
                                if (phoneType != null && phoneType.ValActionCode2AssocMember == "H")
                                {
                                    try
                                    {
                                        // add in the address override phones into the person's list of phones
                                        Phone personalPhone = new Phone(localPhones[i], localPhoneTypes[i], localPhoneExts[i]);
                                        phoneNumber.AddPhone(personalPhone);
                                    }
                                    catch (Exception ex)
                                    {
                                        var phoneError = "Person local phone information is invalid.";

                                        // Log the original exception
                                        logger.Error(ex.ToString());
                                        logger.Info(phoneError);
                                    }
                                }
                            }
                        }
                        // Update Address Phone
                        if (addressData.AdrPhonesEntityAssociation != null && addressData.AdrPhonesEntityAssociation.Count > 0)
                        {
                            foreach (var addrPhone in addressData.AdrPhonesEntityAssociation)
                            {
                                // Only get Address Phone numbers of type "Home"
                                if (addrPhone != null && !string.IsNullOrEmpty(addrPhone.AddressPhoneTypeAssocMember))
                                {
                                    var phoneType = (await GetPhoneTypesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == addrPhone.AddressPhoneTypeAssocMember).FirstOrDefault();
                                    if (phoneType != null && phoneType.ValActionCode2AssocMember == "H")
                                    {
                                        try
                                        {
                                            Phone addressPhone = new Phone(addrPhone.AddressPhonesAssocMember, addrPhone.AddressPhoneTypeAssocMember, addrPhone.AddressPhoneExtensionAssocMember);
                                            phoneNumber.AddPhone(addressPhone);
                                        }
                                        catch (Exception ex)
                                        {
                                            var phoneError = "Person address phone information is invalid.";
                                            // Log the original exception
                                            logger.Error(ex.ToString());
                                            logger.Info(phoneError);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Update Personal Phone
                if (personData.PerphoneEntityAssociation != null && personData.PerphoneEntityAssociation.Count > 0)
                {
                    foreach (var phoneData in personData.PerphoneEntityAssociation)
                    {
                        try
                        {
                            Phone personalPhone = new Phone(phoneData.PersonalPhoneNumberAssocMember, phoneData.PersonalPhoneTypeAssocMember, phoneData.PersonalPhoneExtensionAssocMember);
                            phoneNumber.AddPhone(personalPhone);
                        }
                        catch (Exception ex)
                        {
                            var phoneError = "Person personal phone information is invalid.";

                            // Log the original exception
                            logger.Error(ex.ToString());
                            logger.Info(phoneError);
                        }
                    }
                }
                return phoneNumber;
            }
            return null;
        }
        /// <summary>
        /// Build a string containing the city, state/province, and postal code
        /// </summary>
        /// <param name="city">City</param>
        /// <param name="state">State or Province</param>
        /// <param name="postalCode">Postal Code</param>
        /// <returns>Formatted string with all 3 components</returns>
        private string GetCityStatePostalCode(string city, string state, string postalCode)
        {
            StringBuilder line = new StringBuilder();

            if (!String.IsNullOrEmpty(city))
            {
                line.Append(city);
            }
            if (!String.IsNullOrEmpty(state))
            {
                if (line.Length > 0)
                {
                    line.Append(", ");
                }
                line.Append(state);
            }
            if (!String.IsNullOrEmpty(postalCode))
            {
                if (line.Length > 0)
                {
                    line.Append(" ");
                }
                line.Append(postalCode);
            }
            return line.ToString();
        }

        string Quote = '"'.ToString();

        #endregion

        #region Address Update Methods

        /// <summary>
        /// Update an Address Record in Colleague
        /// </summary>
        /// <param name="addressEntity">A domain object for Address</param>
        /// <returns>Primary key</returns>
        public async Task<Address> UpdateAsync(string addressKey, Address addressEntity)
        {
            if (addressEntity == null)
            {
                throw new ArgumentNullException("addressEntity");
            }

            UpdateAddressRequest updateRequest = new UpdateAddressRequest()
            {
                AddressId = addressKey,
                AddressLines = addressEntity.AddressLines,
                Guid = addressEntity.Guid,
                CarrierRoute = addressEntity.CarrierRoute,
                CorrectionDigit = addressEntity.CorrectionDigit,
                Country = addressEntity.Country,
                DeliveryPoint = addressEntity.DeliveryPoint,
                GeographicArea = addressEntity.AddressChapter,
                IntlLocality = addressEntity.City,
                IntlRegion = string.IsNullOrEmpty(addressEntity.State) ? addressEntity.IntlRegion : addressEntity.State,
                IntlSubRegion = addressEntity.IntlSubRegion,
                IntlPostalCode = addressEntity.IntlPostalCode,
                Longitude = addressEntity.Longitude,
                Latitude = addressEntity.Latitude
            };

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAddressRequest, UpdateAddressResponse>(updateRequest);

            if (updateResponse.AddressErrors.Any())
            {
                var exception = new RepositoryException();
                updateResponse.AddressErrors.ForEach(e =>
                {
                    // It is worth noting here that AddressException.ErrorCodes is a single string, not a list or array
                    var errorCodes = string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes;
                    exception.AddError(new RepositoryError(errorCodes, e.ErrorMessages));
                }

                );
                updateResponse.AddressErrors.ForEach(error => logger.Error(error.ErrorMessages));

                throw exception;
            }

            // get the updated address from the database
            var addressId = await GetAddressFromGuidAsync(updateResponse.Guid);
            return await GetAddressbyIDAsync(addressId);
        }

        /// <summary>
        /// Update an Address Record in Colleague
        /// </summary>
        /// <param name="addressEntity">A domain object for Address</param>
        /// <returns>Primary key</returns>
        public async Task<Address> Update2Async(string addressKey, Address addressEntity)
        {
            if (addressEntity == null)
            {
                throw new ArgumentNullException("addressEntity");
            }

            UpdateAddressRequest updateRequest = new UpdateAddressRequest()
            {
                AddressId = addressKey,
                AddressLines = addressEntity.AddressLines,
                Guid = addressEntity.Guid,
                CarrierRoute = addressEntity.CarrierRoute,
                CorrectionDigit = addressEntity.CorrectionDigit,
                Country = addressEntity.Country,
                DeliveryPoint = addressEntity.DeliveryPoint,
                GeographicArea = addressEntity.AddressChapter,
                IntlLocality = addressEntity.City,
                IntlRegion = string.IsNullOrEmpty(addressEntity.State) ? addressEntity.IntlRegion : addressEntity.State,
                IntlSubRegion = addressEntity.IntlSubRegion,
                IntlPostalCode = addressEntity.IntlPostalCode,
                Longitude = addressEntity.Longitude,
                Latitude = addressEntity.Latitude
            };

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAddressRequest, UpdateAddressResponse>(updateRequest);

            if (updateResponse.AddressErrors.Any())
            {
                var exception = new RepositoryException();
                updateResponse.AddressErrors.ForEach(e =>
                {
                    // It is worth noting here that AddressException.ErrorCodes is a single string, not a list or array
                    var errorCodes = string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes;
                    exception.AddError(new RepositoryError(errorCodes, e.ErrorMessages));
                }

                );
                updateResponse.AddressErrors.ForEach(error => logger.Error(error.ErrorMessages));

                throw exception;
            }

            // get the updated address from the database
            var addressId = await GetAddressFromGuidAsync(updateResponse.Guid);
            return await GetAddressbyID2Async(addressId);
        }

        #endregion

        #region Delete Methods

        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            string addressKey = await GetAddressFromGuidAsync(id);

            var request = new DeleteAddressRequest()
            {
                AddressGuid = id,
                AddressId = addressKey
            };

            var response = await transactionInvoker.ExecuteAsync<DeleteAddressRequest, DeleteAddressResponse>(request);

            if (response.ErrorMessages != null && response.ErrorMessages.Any())
            {
                var errorMessage = string.Format("Error(s) occurred deleting address '{0}':", id);
                var exception = new RepositoryException(errorMessage);
                response.ErrorMessages.ForEach(e => exception.AddError(new RepositoryError("Addresses.Id", e)));
                logger.Error(errorMessage);
                throw exception;
            }
        }

        #endregion
    }
}