// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class PersonBaseRepository : BaseColleagueRepository, IPersonBaseRepository
    {
        private const string _preferredHierarchyCode = "PREFERRED";
        private ApplValcodes PersonEthnics;
        private ApplValcodes MaritalStatuses;
        private ApplValcodes PersonRaces;
        private ApplValcodes PersonalStatuses;
        private ApplValcodes Languages;
        protected string Quote = '"'.ToString();
        protected const int PersonCacheTimeout = 120;
        protected const int AddressCacheTimeout = 120;
        protected const int AllFilteredPersonsCacheTimeout = 20;
        protected const string PersonContractCachePrefix = "PersonContract";
        protected const string AddressContractCachePrefix = "AddressContract";
        protected const string AllFilteredPersonsCache = "AllFilteredPersons";
        protected const int AllOrganizationsCacheTimeout = 20;
        protected const string AllOrganizationsCache = "AllOrganizations";
        private readonly string colleagueTimeZone;
        private int bulkReadSize;
        private RepositoryException exception = new RepositoryException();

        public static char SubValueMark { get; set; } = DmiString._SM;


        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public PersonBaseRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
             : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
            bulkReadSize = settings.BulkReadSize;
        }

        #region Valcodes
        private async Task<ApplValcodes> GetEthnicitiesAsync()
        {
            if (PersonEthnics != null)
            {
                return PersonEthnics;
            }

            PersonEthnics = await GetOrAddToCacheAsync<ApplValcodes>("PersonEthnics",
                async () =>
                {
                    ApplValcodes ethnicsTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ETHNICS");
                    if (ethnicsTable == null)
                    {
                        var errorMessage = "Unable to access PERSON.ETHNICS valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return ethnicsTable;
                }, Level1CacheTimeoutValue);
            return PersonEthnics;
        }
        private async Task<ApplValcodes> GetMaritalStatusesAsync()
        {
            if (MaritalStatuses != null)
            {
                return MaritalStatuses;
            }

            MaritalStatuses = await GetOrAddToCacheAsync<ApplValcodes>("MaritalStatuses",
                async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MARITAL.STATUSES");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access MARITAL.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return MaritalStatuses;
        }
        private async Task<ApplValcodes> GetRacesAsync()
        {
            if (PersonRaces != null)
            {
                return PersonRaces;
            }

            PersonRaces = await GetOrAddToCacheAsync<ApplValcodes>("PersonRaces",
                async () =>
                {
                    ApplValcodes racesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.RACES");
                    if (racesTable == null)
                    {
                        var errorMessage = "Unable to access PERSON.RACES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return racesTable;
                }, Level1CacheTimeoutValue);
            return PersonRaces;
        }

        public async Task<ApplValcodes> GetLanguagesAsync()
        {
            if (Languages != null)
            {
                return Languages;
            }

            Languages = await GetOrAddToCacheAsync<ApplValcodes>("PersonLanguages",
                async () =>
                {
                    ApplValcodes languagesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "LANGUAGES");
                    if (languagesTable == null)
                    {
                        var errorMessage = "Unable to access LANGUAGES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return languagesTable;
                }, Level1CacheTimeoutValue);
            return Languages;
        }

        private async Task<ApplValcodes> GetPersonalStatusesAsync()
        {
            if (PersonalStatuses != null)
            {
                return PersonalStatuses;
            }

            PersonalStatuses = await GetOrAddToCacheAsync<ApplValcodes>("PersonalStatuses",
                async () =>
                {
                    ApplValcodes statusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSONAL.STATUSES");
                    if (statusesTable == null)
                    {
                        var errorMessage = "Unable to access PERSONAL.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return statusesTable;
                }, Level1CacheTimeoutValue);
            return PersonalStatuses;
        }

        /// <summary>
        /// Returns the NameAddrHierarchy record.
        /// </summary>
        /// <returns></returns>
        public async Task<NameAddressHierarchy> GetCachedNameAddressHierarchyAsync(string nameAddressHierarchyCode = _preferredHierarchyCode)
        {
            NameAddressHierarchy requestedNameAddressHierarchy = await GetOrAddToCacheAsync<NameAddressHierarchy>(nameAddressHierarchyCode + "_NameAddressHierarchy",
               async () =>
               {
                   NameAddrHierarchy nameAddrHierarchy = await DataReader.ReadRecordAsync<NameAddrHierarchy>("NAME.ADDR.HIERARCHY", nameAddressHierarchyCode);
                   if (nameAddrHierarchy == null)
                   {
                       if (nameAddressHierarchyCode != _preferredHierarchyCode)
                       {
                           // for all hierarchies except PREFERRED throw an error - this means nothing will be cached 
                           var errorMessage = "Unable to find NAME.ADDR.HIERARCHY record with Id " + nameAddressHierarchyCode + ". Cache not built.";
                           logger.Info(errorMessage);
                           throw new KeyNotFoundException(errorMessage);
                       }
                       else
                       {
                           // All clients are expected to have a PREFERRED name address hierarchy. If they don't, report it but also default one so that it is cached.
                           var errorMessage = "Unable to find NAME.ADDR.HIERARCHY record with Id " + nameAddressHierarchyCode + ". Creating a basic preferred hierarchy with PF name type.";
                           logger.Info(errorMessage);
                           // Construct a default one with the desired name type values.
                           nameAddrHierarchy = new NameAddrHierarchy();
                           nameAddrHierarchy.Recordkey = _preferredHierarchyCode;
                           nameAddrHierarchy.NahNameHierarchy = new List<string>() { "PF" };
                       }

                   }
                   // Build the NameAddressHierarchy Entity and cache that.
                   NameAddressHierarchy newHierarchy = new NameAddressHierarchy(nameAddrHierarchy.Recordkey);
                   if (nameAddrHierarchy.NahNameHierarchy != null && nameAddrHierarchy.NahNameHierarchy.Any())
                   {
                       foreach (var nameType in nameAddrHierarchy.NahNameHierarchy)
                       {
                           newHierarchy.AddNameTypeHierarchy(nameType);
                       }
                   }

                   return newHierarchy;
               }, Level1CacheTimeoutValue);
            return requestedNameAddressHierarchy;
        }
        #endregion

        #region PersonBase Get methods

        /// <summary>
        /// Gets PersonBase entity (Same as person but without Preferred Address)
        /// </summary>
        /// <param name="personId">Id of base person to retrieve</param>
        /// <param name="useCache">Indicates whether to retrieve from cache if available</param>
        /// <returns></returns>
        public async Task<PersonBase> GetPersonBaseAsync(string personId, bool useCache = true)
        {
            Domain.Base.Entities.PersonBase personBaseEntity = await GetBaseAsync<Domain.Base.Entities.PersonBase>(personId,
                 person =>
                 {
                     Domain.Base.Entities.PersonBase entity = new Domain.Base.Entities.Person(person.Recordkey, person.LastName, person.PrivacyFlag);
                     return entity;
                 });
            return personBaseEntity;
        }

        /// <summary>
        /// Gets PersonBase entities (Same as person but without Preferred Address)
        /// </summary>
        /// <param name="personIds">Ids of base persons to retrieve</param>
        /// <param name="useCache">Indicates whether to retrieve from cache if available</param>
        /// <returns>Collection of <see cref="PersonBase"/>PersonBase entities</returns>
        public async Task<IEnumerable<PersonBase>> GetPersonsBaseAsync(IEnumerable<string> personIds, bool hasLastName = false)
        {
            List<PersonBase> personEntities = new List<PersonBase>();
            try
            {
                personEntities = await GetBaseAsync<Domain.Base.Entities.PersonBase>(personIds, person =>
                {
                    List<Domain.Base.Entities.PersonBase> entities = new List<Domain.Base.Entities.PersonBase>();
                    foreach (var p in person)
                    {
                        try
                        {
                            entities.Add(new PersonBase(p.Recordkey, p.LastName, p.PrivacyFlag));
                        }
                        catch (ArgumentNullException ane)
                        {
                            var message = string.Format("Last name for Person record {0} is null or empty.", p.Recordkey);
                            logger.Error(ane, message);
                            if (!hasLastName)
                            {
                                throw new ApplicationException(message, ane);
                            }
                        }

                    }
                    return entities;
                });
            }
            catch (ArgumentOutOfRangeException aoore)
            {
                var message = "One or more IDs did not return any data. Person records may be corrupt.";
                logger.Info(message);
                throw new ApplicationException(message, aoore);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (ArgumentNullException ane)
            {
                var message = "One or more IDs was null.";
                logger.Info(message);
                throw new ApplicationException(message, ane);
            }

            return personEntities;
        }

        /// <summary>
        /// Builds a base person entity.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="id"></param>
        /// <param name="objectBuilder"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public async Task<TDomain> GetBaseAsync<TDomain>(string id, Func<DataContracts.Person, TDomain> objectBuilder, bool useCache = true)
            where TDomain : Domain.Base.Entities.PersonBase
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide an ID to get a record.");
            }
            DataContracts.Person record = await GetPersonContractAsync(id, useCache);
            if (record == null)
            {
                throw new ArgumentOutOfRangeException("id", "No person record for ID " + id);
            }
            // Create the specified domain object
            TDomain person = objectBuilder.Invoke(record);
            var personBaseEntities = await BuildBasePersonsAsync<TDomain>(new List<string>() { id },
                new Collection<DataContracts.Person>() { record }, new Collection<TDomain>() { person });
            return personBaseEntities.FirstOrDefault();
        }

        /// <summary>
        /// Builds base person entities.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="id"></param>
        /// <param name="objectBuilder"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public async Task<List<TDomain>> GetBaseAsync<TDomain>(IEnumerable<string> ids, Func<IEnumerable<DataContracts.Person>, IEnumerable<TDomain>> objectBuilder, bool useCache = true)
            where TDomain : Domain.Base.Entities.PersonBase
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids", "Must provide IDs to get records.");
            }
            IEnumerable<DataContracts.Person> records = await GetPersonContractsAsync(ids);
            if (records == null || !records.Any())
            {
                throw new ArgumentOutOfRangeException("ids", "No person records for IDs " + string.Join(",", ids));
            }
            // Create the specified domain object
            IEnumerable<TDomain> persons = objectBuilder.Invoke(records);
            var personBaseEntities = await BuildBasePersonsAsync<TDomain>(ids, records, persons);
            return personBaseEntities.ToList();
        }

        public async Task<Data.Base.DataContracts.Person> GetPersonContractAsync(string personId, bool useCache = true)
        {
            Data.Base.DataContracts.Person personData = null;
            if (useCache)
            {
                personData = await GetOrAddToCacheAsync<Data.Base.DataContracts.Person>(PersonContractCachePrefix + personId,
                    async () =>
                    {
                        var personRecord = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Person>("PERSON", personId);
                        if (personRecord == null)
                        {
                            throw new ArgumentOutOfRangeException("Person Id " + personId + " is not returning any data. Person may be corrupted.");
                        }
                        return personRecord;
                    }, PersonCacheTimeout);
            }
            else
            {
                personData = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Person>("PERSON", personId);
                await AddOrUpdateCacheAsync<Data.Base.DataContracts.Person>(PersonContractCachePrefix + personId, personData, PersonCacheTimeout);
            }
            return personData;
        }

        public async Task<IEnumerable<Data.Base.DataContracts.Person>> GetPersonContractsAsync(IEnumerable<string> personIds)
        {
            var personData = new List<Data.Base.DataContracts.Person>();

            for (int i = 0; i < personIds.Count(); i += bulkReadSize)
            {
                var subList = personIds.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Data.Base.DataContracts.Person>("PERSON", subList.ToArray());
                if (records != null)
                {
                    personData.AddRange(records);
                    var message = string.Format("{0} records added in PersonBaseRepository", records.Count());
                    logger.Info(message);
                }
            }

            //personData = await DataReader.BulkReadRecordAsync<Data.Base.DataContracts.Person>("PERSON", personIds.ToArray());
            if (personData == null)
            {
                throw new ArgumentOutOfRangeException("Person Ids " + string.Join(",", personIds) + " are not returning any data. Person records may be corrupted.");
            }
            return personData;
        }

        public async Task<Data.Base.DataContracts.Address> GetPersonAddressContractAsync(string addressId, bool useCache = true)
        {
            Data.Base.DataContracts.Address addressData = null;
            if (useCache)
            {
                addressData = await GetOrAddToCacheAsync<Data.Base.DataContracts.Address>(AddressContractCachePrefix + addressId,
                    async () =>
                    {
                        var addressRecord = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Address>("ADDRESS", addressId);
                        if (addressRecord == null)
                        {
                            throw new ArgumentOutOfRangeException("Address Id " + addressId + " is not returning any data. Address may be corrupted.");
                        }
                        return addressRecord;
                    }, AddressCacheTimeout);
            }
            else
            {
                addressData = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Address>("ADDRESS", addressId);
                await AddOrUpdateCacheAsync<Data.Base.DataContracts.Address>(AddressContractCachePrefix + addressId, addressData, AddressCacheTimeout);
            }
            return addressData;
        }

        public async Task<IEnumerable<Data.Base.DataContracts.Address>> GetPersonAddressContractsAsync(IEnumerable<string> addressIds)
        {
            IEnumerable<Data.Base.DataContracts.Address> addressData = null;
            addressData = await DataReader.BulkReadRecordAsync<Data.Base.DataContracts.Address>("ADDRESS", addressIds.ToArray());
            if (addressData == null)
            {
                throw new ArgumentOutOfRangeException("Address Ids " + string.Join(",", addressIds) + " are not returning any data. Address records may be corrupted.");
            }
            return addressData;
        }

        #endregion

        #region Build Person Methods

        /// <summary>
        /// Builds PersonBase objects 
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="personIds"></param>
        /// <param name="records"></param>
        /// <param name="personBasedObjects"></param>
        /// <returns></returns>
        private async Task<IEnumerable<TDomain>> BuildBasePersonsAsync<TDomain>(IEnumerable<string> personIds, IEnumerable<DataContracts.Person> records, IEnumerable<TDomain> personBasedObjects)
        where TDomain : Domain.Base.Entities.PersonBase
        {
            var personResults = new List<TDomain>();
            List<string> personIdsNotFound = new List<string>();

            foreach (string personId in personIds)
            {
                DataContracts.Person record = records.Where(p => p.Recordkey == personId).FirstOrDefault();
                TDomain tDomainObject = personBasedObjects.Where(p => p.Id == personId).FirstOrDefault();
                if (record != null && tDomainObject != null)
                {
                    TDomain personBasedObject = await BuildBasePersonAsync<TDomain>(personId, record, tDomainObject);

                    personResults.Add(personBasedObject);
                }
                else
                {
                    personIdsNotFound.Add(personId);
                }
            }
            if (personIdsNotFound.Count() > 0)
            {
                // log any ids that were not found.
                var errorMessage = "The following person Ids were requested but not found: " + string.Join(",", personIdsNotFound.ToArray());
                logger.Debug(errorMessage);
            }
            return personResults;
        }

        /// <summary>
        /// Build PersonBase object, which is the base class of the Person Object.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="personId"></param>
        /// <param name="record"></param>
        /// <param name="tDomainObject"></param>
        /// <returns></returns>
        protected async Task<TDomain> BuildBasePersonAsync<TDomain>(string personId, Base.DataContracts.Person record, TDomain tDomainObject)
        where TDomain : Domain.Base.Entities.PersonBase
        {
            TDomain personBasedObject = tDomainObject;

            if (record != null && personBasedObject != null)
            {
                // Populate additional Person fields
                personBasedObject.Guid = record.RecordGuid;
                personBasedObject.FirstName = record.FirstName;
                personBasedObject.MiddleName = record.MiddleName;
                personBasedObject.BirthNameFirst = record.BirthNameFirst;
                personBasedObject.BirthNameMiddle = record.BirthNameMiddle;
                personBasedObject.BirthNameLast = record.BirthNameLast;
                personBasedObject.Prefix = record.Prefix;
                personBasedObject.Suffix = record.Suffix;
                personBasedObject.Nickname = record.Nickname;
                personBasedObject.GovernmentId = record.Ssn;
                personBasedObject.BirthDate = record.BirthDate;
                personBasedObject.MaritalStatusCode = record.MaritalStatus;
                personBasedObject.RaceCodes = record.PerRaces;
                personBasedObject.EthnicCodes = record.PerEthnics;
                personBasedObject.DeceasedDate = record.DeceasedDate;
                personBasedObject.IsDeceased = await IsDeceasedAsync(record.PersonStatus);
                personBasedObject.Gender = record.Gender;
                personBasedObject.GenderIdentityCode = record.GenderIdentity;
                personBasedObject.PersonalPronounCode = record.PersonalPronoun;
                personBasedObject.PersonCorpIndicator = record.PersonCorpIndicator;
                personBasedObject.ChosenLastName = record.PersonChosenLastName;
                personBasedObject.ChosenFirstName = record.PersonChosenFirstName;
                personBasedObject.ChosenMiddleName = record.PersonChosenMiddleName;
                personBasedObject.PersonalPronounCode = record.PersonalPronoun;
                personBasedObject.GenderIdentityCode = record.GenderIdentity;
                // Take the mail label name or preferred name override values from the data contract (which could be either a name or a coded entry) and 
                // convert it into simply a name override - where the coded entries are convered into their actual results.
                // In case of mail label name, it defaults to the preferred name override information unless it has its own.
                string mailLabelOverride = record.PersonMailLabel != null && record.PersonMailLabel.Any() ? string.Join(" ", record.PersonMailLabel.ToArray()) : record.PreferredName;
                personBasedObject.MailLabelNameOverride = FormalNameFormat(mailLabelOverride, record.Prefix, record.FirstName, record.MiddleName, record.LastName, record.Suffix);
                personBasedObject.PreferredNameOverride = FormalNameFormat(record.PreferredName, record.Prefix, record.FirstName, record.MiddleName, record.LastName, record.Suffix);
                // Add any special formatted names provided for this person.
                if (record.PFormatEntityAssociation != null && record.PFormatEntityAssociation.Any())
                {
                    foreach (var pFormat in record.PFormatEntityAssociation)
                    {
                        try
                        {
                            personBasedObject.AddFormattedName(pFormat.PersonFormattedNameTypesAssocMember, pFormat.PersonFormattedNamesAssocMember);
                        }
                        catch (Exception)
                        {
                            logger.Error("Unable to add formatted name to person " + record.Recordkey + " with type " + pFormat.PersonFormattedNameTypesAssocMember);
                        }
                    }
                }
                // If the change time is null, use the change date for the time (which will be zero). A null date causes
                // the resulting datetimeoffset to be 1/1/0001 00.00.000.
                if (record.PersonChgtime == null)
                {
                    personBasedObject.LastChangedDateTime = record.PersonChangeDate.ToPointInTimeDateTimeOffset(
                                record.PersonChangeDate, colleagueTimeZone) ?? new DateTimeOffset();
                }
                else
                {
                    personBasedObject.LastChangedDateTime = record.PersonChgtime.ToPointInTimeDateTimeOffset(
                                    record.PersonChangeDate, colleagueTimeZone) ?? new DateTimeOffset();
                }

                // Build Ethnicities Data element from Ethnic Codes and Race Codes
                var ethnicOrigins = new List<EthnicOrigin>();
                if (record.PerEthnics != null && record.PerEthnics.Count > 0)
                {
                    foreach (var ethnicCode in record.PerEthnics)
                    {
                        var ethnicOrigin = EthnicOrigin.Unknown;
                        var codeAssoc = (await GetEthnicitiesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == ethnicCode).FirstOrDefault();
                        if (codeAssoc != null && codeAssoc.ValActionCode1AssocMember == "H")
                        {
                            ethnicOrigin = EthnicOrigin.HispanicOrLatino;
                            ethnicOrigins.Add(ethnicOrigin);
                        }
                    }
                    foreach (var raceCode in record.PerRaces)
                    {
                        var ethnicOrigin = EthnicOrigin.Unknown;
                        var codeAssoc = (await GetRacesAsync()).ValsEntityAssociation.Where(v => v != null && v.ValInternalCodeAssocMember == raceCode).FirstOrDefault();
                        if (codeAssoc != null)
                        {
                            switch (codeAssoc.ValActionCode1AssocMember)
                            {
                                case ("1"): { ethnicOrigin = EthnicOrigin.AmericanIndianOrAlaskanNative; break; }
                                case ("2"): { ethnicOrigin = EthnicOrigin.Asian; break; }
                                case ("3"): { ethnicOrigin = EthnicOrigin.BlackOrAfricanAmerican; break; }
                                case ("4"): { ethnicOrigin = EthnicOrigin.NativeHawaiianOrOtherPacificIslander; break; }
                                case ("5"): { ethnicOrigin = EthnicOrigin.White; break; }
                            }
                            if (ethnicOrigin != EthnicOrigin.Unknown)
                            {
                                ethnicOrigins.Add(ethnicOrigin);
                            }
                        }
                    }
                }
                personBasedObject.Ethnicities = ethnicOrigins;

                // Build Marital Status field
                personBasedObject.MaritalStatus = new MaritalState();
                if (!string.IsNullOrEmpty(record.MaritalStatus))
                {
                    var maritalStatusCode = (await GetMaritalStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == record.MaritalStatus).FirstOrDefault();
                    if (maritalStatusCode != null)
                    {
                        switch (maritalStatusCode.ValActionCode1AssocMember)
                        {
                            case ("1"): { personBasedObject.MaritalStatus = MaritalState.Single; break; }
                            case ("2"): { personBasedObject.MaritalStatus = MaritalState.Married; break; }
                            case ("3"): { personBasedObject.MaritalStatus = MaritalState.Divorced; break; }
                            case ("4"): { personBasedObject.MaritalStatus = MaritalState.Widowed; break; }
                        }
                    }
                }

                // Get preferred name using name hierarchy PREFERRED.
                NameAddressHierarchy hierarchy = await GetCachedNameAddressHierarchyAsync(_preferredHierarchyCode);
                if (hierarchy != null)
                {
                    var personNameHierarchy = PersonNameService.GetHierarchyName(personBasedObject, hierarchy);
                    personBasedObject.PreferredName = personNameHierarchy != null ? personNameHierarchy.FullName : string.Empty;
                }

                // person email addresses
                if (record.PeopleEmailEntityAssociation != null && record.PeopleEmailEntityAssociation.Count > 0)
                {
                    foreach (var emailData in record.PeopleEmailEntityAssociation)
                    {
                        try
                        {
                            EmailAddress eAddress = new EmailAddress(emailData.PersonEmailAddressesAssocMember, emailData.PersonEmailTypesAssocMember);
                            eAddress.IsPreferred = emailData.PersonPreferredEmailAssocMember == "Y";
                            personBasedObject.AddEmailAddress(eAddress);
                        }
                        catch (Exception ex)
                        {
                            if (emailData != null && string.IsNullOrEmpty(emailData.PersonEmailTypesAssocMember))
                            {
                                logger.Error(ex, "Person {0} has email with no type code that could not be added", record.Recordkey);
                            }
                            else
                            {
                                logger.Error(ex, "Person {0} has email with type {1} that could not be added", record.Recordkey, emailData.PersonEmailTypesAssocMember);
                            }
                        }
                    }
                }

                // person alternate Ids
                if (record.PersonAltEntityAssociation != null && record.PersonAltEntityAssociation.Count > 0)
                {
                    foreach (var personAltData in record.PersonAltEntityAssociation)
                    {
                        try
                        {
                            var personAlt = new PersonAlt(personAltData.PersonAltIdsAssocMember, personAltData.PersonAltIdTypesAssocMember);
                            personBasedObject.AddPersonAlt(personAlt);
                        }
                        catch (Exception ex)
                        {
                            if (personAltData != null && string.IsNullOrEmpty(personAltData.PersonAltIdTypesAssocMember))
                            {
                                logger.Error(ex, "Person {0} has alternate ID with no type", record.Recordkey);
                            }
                            else
                            {
                                logger.Error(ex, "Person {0} has alternate ID of type {1} that could not be added", record.Recordkey, personAltData.PersonAltIdTypesAssocMember);
                            }
                        }
                    }
                }

                // person languages
                if (record.PersonPrimaryLanguage != null)
                {
                    var codeAssoc = (await GetLanguagesAsync()).ValsEntityAssociation.Where(v => v != null && v.ValInternalCodeAssocMember == record.PersonPrimaryLanguage).FirstOrDefault();
                    if (codeAssoc != null)
                    {
                        personBasedObject.PrimaryLanguage = codeAssoc.ValInternalCodeAssocMember;
                    }
                }

                if (record.PersonSecondaryLanguage != null && record.PersonSecondaryLanguage.Count > 0)
                {
                    var secondaryLanguages = new List<string>();
                    foreach (var secondaryLanguage in record.PersonSecondaryLanguage)
                    {
                        var codeAssoc = (await GetLanguagesAsync()).ValsEntityAssociation.Where(v => v != null && v.ValInternalCodeAssocMember == secondaryLanguage).FirstOrDefault();
                        if (codeAssoc != null)
                        {
                            secondaryLanguages.Add(codeAssoc.ValInternalCodeAssocMember);
                        }
                    }
                    if (secondaryLanguages.Any())
                    {
                        personBasedObject.SecondaryLanguages = secondaryLanguages;
                    }
                }
            }
            return personBasedObject;
        }

        /// <summary>
        /// Build PersonBase object, which is the base class of the Person Object, for persons-credentials
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="personId"></param>
        /// <param name="record"></param>
        /// <param name="tDomainObject"></param>
        /// <returns></returns>
        protected async Task<TDomain> BuildBasePersonCredentialAsync<TDomain>(string personId, Base.DataContracts.Person record, TDomain tDomainObject)
        //protected Task<TDomain> BuildBasePersonCredentialAsync<TDomain>(string personId, Base.DataContracts.Person record, TDomain tDomainObject)
        where TDomain : Domain.Base.Entities.PersonBase
        {
            TDomain personBasedObject = tDomainObject;

            if (record != null && personBasedObject != null)
            {
                // Populate additional Person fields
                personBasedObject.Guid = record.RecordGuid;
                personBasedObject.GovernmentId = record.Ssn;

                // person alternate Ids
                if (record.PersonAltEntityAssociation != null && record.PersonAltEntityAssociation.Count > 0)
                {
                    foreach (var personAltData in record.PersonAltEntityAssociation)
                    {
                        try
                        {
                            var personAlt = new PersonAlt(personAltData.PersonAltIdsAssocMember, personAltData.PersonAltIdTypesAssocMember);
                            personBasedObject.AddPersonAlt(personAlt);
                        }
                        catch (Exception ex)
                        {
                            if (personAltData != null && string.IsNullOrEmpty(personAltData.PersonAltIdTypesAssocMember))
                            {
                                logger.Error(ex, "Person {0} has alternate ID with no type", record.Recordkey);
                            }
                            else
                            {
                                logger.Error(ex, "Person {0} has alternate ID of type {1} that could not be added", record.Recordkey, personAltData.PersonAltIdTypesAssocMember);
                            }
                        }
                    }
                }

            }
            return personBasedObject;
        }

        #endregion

        #region Name methods

        /// <summary>
        /// Builds a formal mail name override or preferred name override based on the specific coded name override in data contract (could be a code) and all the person's current name parts.
        /// </summary>
        /// <param name="nameValue">Either the mail label or preferred name override - which can include special codes</param>
        /// <param name="prefix">Name prefix</param>
        /// <param name="firstName">First Name</param>
        /// <param name="middleName">Middle Name</param>
        /// <param name="lastName">Last Name</param>
        /// <param name="suffix">Suffix</param>
        /// <returns></returns>
        public static string FormalNameFormat(string nameValue, string prefix, string firstName, string middleName, string lastName, string suffix)
        {
            string firstInitial = !string.IsNullOrWhiteSpace(firstName) ? firstName.Trim().Substring(0, 1) + "." : string.Empty;
            string middleInitial = !string.IsNullOrWhiteSpace(middleName) ? middleName.Trim().Substring(0, 1) + "." : string.Empty;
            if (!string.IsNullOrEmpty(nameValue))
            {
                string shortCutCode = nameValue.ToUpper();
                switch (shortCutCode)
                {
                    case "IM":
                        return PersonNameService.FormatName(prefix, firstInitial, middleName, lastName, suffix);
                    case "II":
                        return PersonNameService.FormatName(prefix, firstInitial, middleInitial, lastName, suffix);
                    case "FM":
                        return PersonNameService.FormatName(prefix, firstName, middleName, lastName, suffix);
                    case "FI":
                        return PersonNameService.FormatName(prefix, firstName, middleInitial, lastName, suffix);
                    default:
                        return nameValue;
                }
            }
            // If no name value supplied default to prefix first middle initial last suffix.
            return PersonNameService.FormatName(prefix, firstName, middleInitial, lastName, suffix);
        }

        #endregion

        #region Get Persona Methods

        /// <summary>
        /// Determine if the identifier represents a valid person
        /// </summary>
        /// <param name="personId">Identifier to test</param>
        /// <returns>True if the identifier represents a person in the system</returns>
        public async Task<bool> IsPersonAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID is required for determining existence.");

            var result = (await DataReader.SelectAsync("PERSON", new string[] { personId }, string.Empty)).FirstOrDefault();
            return result != null && result.Any() ? true : false;
        }

        /// <summary>
        /// Determine if the identifier represents a valid corporation
        /// </summary>
        /// <param name="personId">Identifier to test</param>
        /// <returns>True if the identifier represents a person in the system</returns>
        public async Task<bool> IsCorpAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID is required for determining existence.");

            var result = await DataReader.ReadRecordAsync<DataContracts.Person>("PERSON", personId);
            return result != null && result.PersonCorpIndicator == "Y" ? true : false;
        }

        /// <summary>
        /// Determine if the GUID represents a valid person, rather than an institution or corporation
        /// </summary>
        /// <param name="personId">potential person GUID</param>
        /// <returns>Person Record Key if CorpIndicator is false, otherwise returns empty string</returns>
        public async Task<string> GetPersonIdForNonCorpOnly(string personGuid)
        {
            if (string.IsNullOrEmpty(personGuid))
                return string.Empty;
            var result = await DataReader.ReadRecordAsync<DataContracts.Person>(new GuidLookup(personGuid));
            if (result == null)
                return string.Empty;
            return result.PersonCorpIndicator == "Y" ? string.Empty : result.Recordkey;
        }


        /// <summary>
        /// Determine if the person is a faculty member
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>True if the person is a faculty member, false otherwise</returns>
        public async Task<bool> IsFacultyAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID is required for determining faculty role");

            var result = (await DataReader.SelectAsync("FACULTY", new string[] { personId }, string.Empty)).FirstOrDefault();
            return result != null && result.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Determine if the person is a faculty advisor
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>True if the person is a faculty advisor, false otherwise</returns>
        public async Task<bool> IsAdvisorAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID is required for determining faculty role");

            var result = (await DataReader.SelectAsync("FACULTY", new string[] { personId }, "WITH FAC.ADVISE.FLAG EQ 'Y'")).FirstOrDefault();
            return result != null && result.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Determine if the person is a student
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>True if the person is a student, false otherwise</returns>
        public async Task<bool> IsStudentAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID is required for determining student role");

            var result = (await DataReader.SelectAsync("STUDENTS", new string[] { personId }, string.Empty)).FirstOrDefault();
            return result != null && result.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Determines if the person with the specified id is an applicant 
        /// </summary>
        /// <param name="personId">person id</param>
        /// <returns>boolean value indicating whether the person is an applicant</returns>
        public async Task<bool> IsApplicantAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID is required for determining applicant role");
            }

            var result = (await DataReader.SelectAsync("APPLICANTS", new string[] { personId }, string.Empty)).FirstOrDefault();
            return result != null && result.Any() ? true : false;
        }

        #endregion

        #region Get Person/Org Guids

        /// <summary>
        /// Returns collection of opers ids with person id & guid.
        /// </summary>
        /// <param name="operKeys"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, KeyValuePair<string, string>>> GetPersonGuidsFromOperKeysAsync(IEnumerable<string> operKeys)
        {
            Dictionary<string, KeyValuePair<string, string>> operIdsWithGuidsDict = new Dictionary<string, KeyValuePair<string, string>>();

            if (operKeys == null || !operKeys.Any())
            {
                return null;
            }
            try
            {
                var operRecords = await DataReader.BulkReadRecordAsync<Opers>("UT.OPERS", operKeys.ToArray());
                var staffIds = await DataReader.SelectAsync("STAFF", "WITH STAFF.INITIALS EQ '?'", operKeys.ToArray());
                var staffRecords = await DataReader.BulkReadRecordAsync<DataContracts.Staff>(staffIds);

                if (operRecords == null || !operRecords.Any())
                {
                    return null;
                }

                var sysPersonIds = operRecords.Where(rec => !string.IsNullOrWhiteSpace(rec.SysPersonId)).Select(i => i.SysPersonId).ToList();
                var tempStaffIds = staffRecords.Where(rec => !string.IsNullOrWhiteSpace(rec.Recordkey)).Select(i => i.Recordkey).ToList();
                var personIds = sysPersonIds.Union(tempStaffIds).Distinct();

                if (personIds == null || !personIds.Any())
                {
                    return null;
                }

                string criteria = "WITH PERSON.CORP.INDICATOR NE 'Y'";
                var ids = await DataReader.SelectAsync("PERSON", personIds.ToArray(), criteria);

                var personGuidLookup = ids
                                        .Where(s => !string.IsNullOrWhiteSpace(s))
                                        .Distinct().ToList()
                                        .ConvertAll(p => new RecordKeyLookup("PERSON", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
                if (recordKeyLookupResults != null && recordKeyLookupResults.Any())
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                        var splitKey = splitKeys[1];

                        var operRecord = operRecords.FirstOrDefault(i => i.SysPersonId.Equals(splitKey, StringComparison.OrdinalIgnoreCase));
                        var staffRecord = staffRecords.FirstOrDefault(i => i.Recordkey.Equals(splitKey, StringComparison.OrdinalIgnoreCase));
                        if (operRecord != null)
                        {
                            if (recordKeyLookupResult.Value != null && !string.IsNullOrWhiteSpace(recordKeyLookupResult.Value.Guid))
                            {
                                if (!operIdsWithGuidsDict.ContainsKey(operRecord.Recordkey))
                                {
                                    KeyValuePair<string, string> tuple = new KeyValuePair<string, string>(splitKey, recordKeyLookupResult.Value.Guid);
                                    operIdsWithGuidsDict.Add(operRecord.Recordkey, tuple);
                                }
                            }
                        }
                        else if (staffRecord != null && operRecord == null)
                        {
                            if (recordKeyLookupResult.Value != null && !string.IsNullOrWhiteSpace(recordKeyLookupResult.Value.Guid))
                            {
                                if (!string.IsNullOrEmpty(staffRecord.StaffInitials) && !operIdsWithGuidsDict.ContainsKey(staffRecord.StaffInitials))
                                {
                                    KeyValuePair<string, string> tuple = new KeyValuePair<string, string>(splitKey, recordKeyLookupResult.Value.Guid);
                                    operIdsWithGuidsDict.Add(staffRecord.StaffInitials, tuple);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
            return operIdsWithGuidsDict;
        }

        /// <summary>
        /// Gets person guid from opers
        /// </summary>
        /// <param name="opersKey">opersKey</param>
        /// <returns></returns>
        public async Task<string> GetPersonGuidFromOpersAsync(string opersKey)
        {
            if (string.IsNullOrEmpty(opersKey))
                throw new ArgumentNullException("opersKey", "Opers ID is a required argument.");

            string personId = await GetPersonIdFromOpersAsync(opersKey);
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("opersKey", "No ID found for person ID " + opersKey);

            string personGuid = await GetPersonGuidFromIdAsync(personId);
            if (!string.IsNullOrEmpty(personGuid))
                return personGuid;

            return string.Empty;
        }

        /// <summary>
        /// Gets person id from opers
        /// </summary>
        /// <param name="opersKey">opersKey</param>
        /// <returns></returns>
        public async Task<string> GetPersonIdFromOpersAsync(string opersKey)
        {
            if (string.IsNullOrEmpty(opersKey))
                throw new ArgumentNullException("opersKey", "Opers ID is a required argument.");

            int id;
            bool isNumber = int.TryParse(opersKey, out id);
            if (isNumber && opersKey.Length == 7)
                return opersKey;
            //else get the recordId and return
            DataContracts.Opers record = await DataReader.ReadRecordAsync<DataContracts.Opers>("UT.OPERS", opersKey);

            if (record != null)
            {
                return record.SysPersonId;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get the GUID from a person ID
        /// </summary>
        /// <param name="personId">The person ID</param>
        /// <returns>The GUID</returns>
        public async Task<string> GetPersonGuidFromIdAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID is a required argument.");

            var lookup = new RecordKeyLookup("PERSON", personId, false);
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

            throw new ArgumentOutOfRangeException("personId", "No GUID found for person ID " + personId);
        }

        /// <summary>
        /// Gets all the person guids
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<string>, int>> GetPersonGuidsAsync(int offset, int limit, bool bypassCache)
        {
            //Dont return Orgs & Institutions
            string criteria = "WITH PERSON.CORP.INDICATOR NE 'Y'";

            var personIds = await DataReader.SelectAsync("PERSON", criteria);
            if ((personIds != null && !personIds.Any()))
            {
                return new Tuple<IEnumerable<string>, int>(null, 0);
            }

            var totalCount = personIds.Count();

            if (totalCount <= offset)
            {
                return new Tuple<IEnumerable<string>, int>(null, 0);
            }

            Array.Sort(personIds);

            var sublist = personIds.Skip(offset).Take(limit);

            var personGuids = new List<string>();

            if (sublist != null && sublist.Any())
            {
                try
                {
                    // convert the person keys to person guids
                    var personGuidLookup = sublist.ToList().ConvertAll(p => new RecordKeyLookup("PERSON", p, false)).ToArray();
                    personGuids = (await DataReader.SelectAsync(personGuidLookup)).ToList().ConvertAll(p => p.Value.Guid);
                }
                catch (Exception ex)
                {
                    throw new ColleagueWebApiException("Error occured while getting person guids.", ex); ;
                }
            }

            return (personGuids != null && personGuids.Any()) ? new Tuple<IEnumerable<string>, int>(personGuids, totalCount) : null;
        }

        /// <summary>
        /// Using a collection of person ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="personIds">collection of person ids</param>
        /// <returns>Dictionary consisting of a personId (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetPersonGuidsCollectionAsync(IEnumerable<string> personIds)
        {
            if ((personIds == null) || (personIds != null && !personIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var personGuidCollection = new Dictionary<string, string>();

            var personGuidLookup = personIds
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList()
                .ConvertAll(p => new RecordKeyLookup("PERSON", p, false)).ToArray();
            var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!personGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        personGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
                catch (Exception ex)
                {
                    // Do not throw error.
                    logger.Error(ex.Message, "Cannot add person ID to guid collection.");
                }
            }

            return personGuidCollection;
        }

        /// <summary>
        /// Using a collection of person ids, get a dictionary collection of associated guids for person who are not orgs or institutions.
        /// </summary>
        /// <param name="personIds">collection of person ids</param>
        /// <returns>Dictionary consisting of a personId (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetPersonGuidsWithNoCorpCollectionAsync(IEnumerable<string> personIds)
        {
            if ((personIds == null) || (personIds != null && !personIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var personGuidCollection = new Dictionary<string, string>();
            try
            {
                //Dont return Orgs & Institutions
                string criteria = "WITH PERSON.CORP.INDICATOR NE 'Y'";
                var ids = await DataReader.SelectAsync("PERSON", personIds.ToArray(), criteria);

                var personGuidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup("PERSON", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!personGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        personGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException("Error occured while getting person guids.", ex); ;
            }

            return personGuidCollection;
        }

        /// <summary>
        /// Gets all the person guids
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<string>, int>> GetPersonGuidsFilteredAsync(int offset, int limit, Dictionary<string, string> credentials, bool bypassCache)
        {
            //Dont return Orgs & Institutions
            string criteria = "WITH PERSON.CORP.INDICATOR NE 'Y'";

            var personPinIds = new List<string>();
            var elevateIds = new List<string>();

            if (credentials != null && credentials.Any())
            {
                foreach (KeyValuePair<string, string> credential in credentials)
                {

                    if ((!string.IsNullOrWhiteSpace(credential.Value)) && (!string.IsNullOrWhiteSpace(credential.Key)))
                    {
                        var credentialValue = credential.Value.Contains("'") ?
                            credential.Value.Replace("'", "") : credential.Value;

                        var credentialKey = credential.Key.ToLowerInvariant();

                        // Colleague will only use the "ssn", "sin", "colleaguePersonId", "elevateId", and
                        //  "colleagueUserName" options..
                        if (credentialKey == "colleaguepersonid")
                        {
                            criteria = string.Concat(criteria, " AND WITH ID = '", credentialValue, "'");
                        }
                        else if ((credentialKey == "ssn") || (credentialKey == "sin"))
                        {
                            criteria = string.Concat(criteria, " AND WITH SSN = '", credentialValue, "'");
                        }
                        else if (credentialKey == "colleagueusername")
                        {
                            personPinIds.Add(credentialValue);
                        }
                        else if (credentialKey == "elevateid")
                        {
                            //criteria = string.Concat(criteria, " AND WITH PERSON.ALT.ID.TYPES EQ 'ELEV' AND PERSON.ALT.IDS EQ '", credentialValue, "'");
                            elevateIds.Add(credentialValue);
                        }
                    }
                }
            }
            var personIds = await DataReader.SelectAsync("PERSON", criteria);

            if (elevateIds.Any())
            {
                var elevatePersonList = await DataReader.SelectAsync("PERSON",
                    " WITH PERSON.CORP.INDICATOR NE 'Y' AND WITH PERSON.ALT.ID.TYPES EQ 'ELEV' AND PERSON.ALT.IDS EQ '?'",
                    elevateIds.ToArray());

                // if the filter criteria contains person.alt.id, and no records match, then return
                if (elevatePersonList == null || !elevatePersonList.Any())
                {
                    return new Tuple<IEnumerable<string>, int>(null, 0);
                }

                var elevatePersonIds = new List<string>();
                var elevatePersons = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", elevatePersonList.ToArray());

                if ((elevatePersons != null) && (elevatePersons.Any()))
                {
                    foreach (var elevatePerson in elevatePersons)
                    {
                        var personAltEntityAssociations = elevatePerson.PersonAltEntityAssociation;
                        if ((personAltEntityAssociations != null) && (personAltEntityAssociations.Any())
                            && (personAltEntityAssociations.Any(x => x.PersonAltIdTypesAssocMember == "ELEV")))
                        {
                            elevatePersonIds.Add(elevatePerson.Recordkey);
                        }
                    }
                }
                if (!elevatePersonIds.Any())
                {
                    return new Tuple<IEnumerable<string>, int>(null, 0);
                }
                personIds = personIds.Intersect(elevatePersonIds).ToArray();
            }

            if (personPinIds.Any())
            {
                var credentialPersonList = await DataReader.SelectAsync("PERSON.PIN", " WITH PERSON.PIN.USER.ID EQ '?'", personPinIds.ToArray());
                // if the filter criteria contains person.pins, and no records match, then return
                if (credentialPersonList == null || !credentialPersonList.Any())
                {
                    return new Tuple<IEnumerable<string>, int>(null, 0);
                }
                personIds = personIds.Intersect(credentialPersonList).ToArray();
            }
            var totalCount = personIds.Count();

            Array.Sort(personIds);

            var sublist = personIds.Skip(offset).Take(limit);

            var personGuids = new List<string>();

            if (sublist != null && sublist.Any())
            {
                try
                {
                    // convert the person keys to person guids
                    var personGuidLookup = sublist.ToList().ConvertAll(p => new RecordKeyLookup("PERSON", p, false)).ToArray();
                    personGuids = (await DataReader.SelectAsync(personGuidLookup)).ToList().ConvertAll(p => p.Value.Guid);
                }
                catch (Exception ex)
                {
                    throw new ColleagueWebApiException("Error occured while getting person guids.", ex); ;
                }
            }

            return (personGuids != null && personGuids.Any()) ? new Tuple<IEnumerable<string>, int>(personGuids, totalCount) : null;
        }
        /// <summary>
        /// Get a list of guids associated with faculty
        /// </summary>
        /// <returns>List of person guids associated with faculty</returns>
        public async Task<Tuple<IEnumerable<string>, int>> GetFacultyPersonGuidsAsync(int offset, int limit)
        {
            var facultyPersonGuids = new List<string>();
            // get the faculty keys
            var facultyIds = await DataReader.SelectAsync("FACULTY", "");
            // check HR database to see if we have faculty setup there.
            // the best we can do is check for termination date.  If we have no ID's
            // returned, then they aren't using HR to track faculty contracts.
            var hrperIds = (await DataReader.SelectAsync("HRPER", facultyIds.ToArray(), "WITH HRP.EFFECT.TERM.DATE = ''"));
            if (hrperIds != null && hrperIds.Any())
            {
                facultyIds = hrperIds;
            }
            var totalCount = facultyIds.Count();
            Array.Sort(facultyIds);
            var sublist = facultyIds.Skip(offset).Take(limit);
            var newFacultyIds = sublist.ToList();
            if (newFacultyIds != null && newFacultyIds.Any())
            {
                // convert the faculty keys to person guids
                var facultyPersonGuidLookup = newFacultyIds.ConvertAll(f => new RecordKeyLookup("PERSON", f, false)).ToArray();
                facultyPersonGuids = (await DataReader.SelectAsync(facultyPersonGuidLookup)).ToList().ConvertAll(p => p.Value.Guid);
            }
            return new Tuple<IEnumerable<string>, int>(facultyPersonGuids, totalCount);
        }

        /// <summary>
        /// Get a list of guids associated with faculty
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="title">Specific title</param>
        /// <param name="firstName">Specific first name</param>
        /// <param name="middleName">Specific middle name</param>
        /// <param name="lastNamePrefix">Last name beings with</param>
        /// <param name="lastName">Specific last name</param>
        /// <param name="pedigree">Specific suffix</param>
        /// <param name="preferredName">Specific preferred name</param>
        /// <param name="role">Specific role of a person</param>
        /// <param name="credentialType">Credential type of either colleagueId or ssn</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns>List of person guids </returns>
        public async Task<Tuple<IEnumerable<string>, int>> GetFilteredPerson2GuidsAsync(int offset, int limit, bool bypassCache, PersonFilterCriteria personFilterCriteria, string personFilter = "")
        {
            try
            {
                string criteria = "WITH PERSON.CORP.INDICATOR NE 'Y'";
                int totalCount = 0;
                var personGuids = new List<string>();
                string personFilterKey = string.Empty;
                List<string> outIds = new List<string>(), colleaguePersonIds = null, elevateIds = null, colleagueusernameIds = null;
                Collection<DataContracts.Person> people = null;

                //If there are no filter criteria passed then search the PERSON file/table. This is for Get All. 
                if (personFilterCriteria == null && string.IsNullOrWhiteSpace(personFilter))
                {
                    outIds = (await DataReader.SelectAsync("PERSON", criteria)).ToList();

                    totalCount = outIds.Count();
                    var filteredPersonIds = outIds.ToArray();

                    if (totalCount <= offset)
                    {
                        return new Tuple<IEnumerable<string>, int>(null, 0);
                    }

                    Array.Sort(filteredPersonIds);

                    var subList = filteredPersonIds.Skip(offset).Take(limit).ToArray();

                    var idLookUpList = new List<RecordKeyLookup>();
                    subList.ForEach(s => idLookUpList.Add(new RecordKeyLookup("PERSON", s, false)));

                    var personGuidLookupList = await DataReader.SelectAsync(idLookUpList.ToArray());
                    foreach (var o in personGuidLookupList)
                    {
                        if (o.Value != null && !string.IsNullOrEmpty(o.Value.Guid))
                        {
                            personGuids.Add(o.Value.Guid);
                        }
                        else
                        {
                            var ex = new RepositoryException();
                            ex.AddError(new Domain.Entities.RepositoryError(string.Format("PERSON record '{0}' is missing a guid.", o.Key.Split('+')[1])));
                            throw ex;
                        }
                    }
                    return personGuids != null && personGuids.Any() ? new Tuple<IEnumerable<string>, int>(personGuids, totalCount) : new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                }

                //if we have some filters

                /*
                    Here check the validity of the personFilter first, if a bad guid is passed then nothing needs to get processed
                    since we are using "AND" clause between all the params passed between personFilterCriteria and personFilter
                */
                if (!string.IsNullOrEmpty(personFilter))
                {
                    var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(personFilter) });
                    if (idDict == null || idDict.Count == 0)
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    var foundEntry = idDict.FirstOrDefault();
                    if (foundEntry.Value == null)
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    if (foundEntry.Value.Entity != "SAVE.LIST.PARMS")
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }
                    personFilterKey = foundEntry.Value.PrimaryKey;
                }

                #region Start with credentials and get first record out and then compare with others

                #region Credentials
                if (personFilterCriteria != null && personFilterCriteria.Credentials != null && personFilterCriteria.Credentials.Any())
                {
                    foreach (var cred in personFilterCriteria.Credentials)
                    {
                        if (cred.Item1.ToLowerInvariant() == "colleaguepersonid")
                        {
                            colleaguePersonIds = new List<string>();
                            // to address issue if there is single quote around ColleagueId
                            var personId = cred.Item2;
                            if (personId.Contains("'"))
                            {
                                personId = personId.Replace("'", string.Empty);
                            }
                            var ids = await DataReader.SelectAsync("PERSON", new string[] { personId }, string.Empty);
                            if (ids == null || !ids.Any())
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                            else
                            {
                                colleaguePersonIds.AddRange(ids);
                                if (ids != null && ids.Any() && elevateIds != null && elevateIds.Any())
                                {
                                    var intersectIds = ids.Intersect(elevateIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                }
                                else if (ids != null && ids.Any() && colleagueusernameIds != null && colleagueusernameIds.Any())
                                {
                                    var intersectIds = ids.Intersect(colleagueusernameIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                }
                                else
                                {
                                    outIds.AddRange(ids);
                                }
                            }
                        }
                        else if (cred.Item1.ToLowerInvariant() == "elevateid")
                        {
                            elevateIds = new List<string>();
                            //there is index in alternate Ids so we can use first and then filter the rest. 
                            var altIds = await DataReader.SelectAsync("PERSON", outIds != null && outIds.Any() ? outIds.ToArray() : null,
                                                     string.Format("WITH PERSON.ALT.IDS EQ '{0}'", cred.Item2));
                            if (altIds == null || !altIds.Any())
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                            else
                            {
                                //apply the check for alternate Id type of Elevate here
                                var elevatePersonIds = new List<string>();
                                var elevatePersons = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", altIds.ToArray());

                                if ((elevatePersons != null) && (elevatePersons.Any()))
                                {
                                    foreach (var elevatePerson in elevatePersons)
                                    {
                                        var personAltEntityAssociations = elevatePerson.PersonAltEntityAssociation;
                                        if ((personAltEntityAssociations != null) && (personAltEntityAssociations.Any())
                                            && (personAltEntityAssociations.Any(x => x.PersonAltIdTypesAssocMember == "ELEV" && x.PersonAltIdsAssocMember == cred.Item2)))
                                        {
                                            elevatePersonIds.Add(elevatePerson.Recordkey);
                                        }
                                    }
                                }

                                //apply the check for alternate Id type of Elevate here

                                if (elevatePersonIds == null || !elevatePersonIds.Any())
                                {
                                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                }
                                elevateIds.AddRange(elevatePersonIds);
                                if (elevatePersonIds != null && elevatePersonIds.Any() && colleaguePersonIds != null && colleaguePersonIds.Any())
                                {
                                    var intersectIds = elevatePersonIds.Intersect(colleaguePersonIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                }
                                else if (elevatePersonIds != null && elevatePersonIds.Any() && colleagueusernameIds != null && colleagueusernameIds.Any())
                                {
                                    var intersectIds = elevatePersonIds.Intersect(colleagueusernameIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                }
                                else
                                {
                                    outIds.AddRange(elevatePersonIds);
                                }
                            }
                        }
                        else if (cred.Item1.ToLowerInvariant() == "colleagueusername")
                        {
                            colleagueusernameIds = new List<string>();
                            var ids = (await DataReader.SelectAsync("ORG.ENTITY.ENV", outIds != null && outIds.Any() ? outIds.ToArray() : null,
                                            string.Format("WITH OEE.USERNAME = '{0}' SAVING OEE.RESOURCE", cred.Item2))).ToList();
                            if (ids == null || !ids.Any())
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                            else
                            {
                                ids = (await DataReader.SelectAsync("ORG.ENTITY", ids.ToArray(), null)).ToList();
                                colleagueusernameIds.AddRange(ids);
                                if (ids != null && ids.Any() && elevateIds != null && elevateIds.Any())
                                {
                                    var intersectIds = ids.Intersect(elevateIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                }
                                else if (ids != null && ids.Any() && colleaguePersonIds != null && colleaguePersonIds.Any())
                                {
                                    var intersectIds = ids.Intersect(colleaguePersonIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                }
                                else
                                {
                                    outIds.AddRange(ids);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region AlternativeCredentials
                if (personFilterCriteria != null && personFilterCriteria.AlternativeCredentials != null && personFilterCriteria.AlternativeCredentials.Any())
                {
                    foreach (var altCred in personFilterCriteria.AlternativeCredentials)
                    {
                        //there is index in alternate Ids so we can use first and then filter the rest. 
                        var altIds = await DataReader.SelectAsync("PERSON", outIds != null && outIds.Any() ? outIds.ToArray() : null,
                                                 string.Format("WITH PERSON.ALT.IDS EQ '{0}'", altCred.Item2));
                        if (altIds == null || !altIds.Any())
                        {
                            return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(altCred.Item1))
                            {
                                outIds.AddRange(altIds);
                            }
                            else
                            {
                                //apply the check for alternate Id type here
                                var alternatePersonIds = new List<string>();
                                var alternatePersons = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", altIds.ToArray());

                                if ((alternatePersons != null) && (alternatePersons.Any()))
                                {
                                    foreach (var alternatePerson in alternatePersons)
                                    {
                                        var personAltEntityAssociations = alternatePerson.PersonAltEntityAssociation;
                                        if ((personAltEntityAssociations != null) && (personAltEntityAssociations.Any())
                                            && (personAltEntityAssociations.Any(x => x.PersonAltIdTypesAssocMember == altCred.Item1.ToUpperInvariant() && x.PersonAltIdsAssocMember == altCred.Item2)))
                                        {
                                            alternatePersonIds.Add(alternatePerson.Recordkey);
                                        }
                                    }
                                }

                                if (alternatePersonIds == null || !alternatePersonIds.Any())
                                {
                                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                }
                                outIds.AddRange(alternatePersonIds);
                            }
                        }
                    }
                }
                #endregion

                #region Emails
                if (personFilterCriteria != null && personFilterCriteria.Emails != null && personFilterCriteria.Emails.Any())
                {
                    var emailCriteria = string.Empty;
                    foreach (var email in personFilterCriteria.Emails)
                    {
                        if (!string.IsNullOrEmpty(email))
                        {
                            if (string.IsNullOrEmpty(emailCriteria))
                                emailCriteria = string.Format("WITH PERSON.EMAIL.ADDRESSES.IDX EQ '{0}'", email);
                            else
                                emailCriteria += string.Format(" AND WITH PERSON.EMAIL.ADDRESSES.IDX EQ '{0}'", email);
                        }
                    }

                    //we need to pass in the outIds here
                    var emailIds = await DataReader.SelectAsync("PERSON", outIds != null && outIds.Any() ? outIds.ToArray() : null, emailCriteria);
                    if (emailIds == null || !emailIds.Any())
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    if (emailIds != null && emailIds.Any())
                    {
                        if (outIds != null && outIds.Any())
                        {
                            var intersectIds = outIds.Distinct().Intersect(emailIds.ToList()).ToList();
                            if (intersectIds == null || !intersectIds.Any())
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                            else
                            {
                                outIds.AddRange(intersectIds);
                            }
                        }
                        else
                        {
                            outIds.AddRange(emailIds);
                        }
                    }
                }
                #endregion

                #region Roles
                if (personFilterCriteria != null && personFilterCriteria.Roles != null && personFilterCriteria.Roles.Any())
                {
                    var rolesIds = await BuildRoleIdsAsync(personFilterCriteria.Roles, outIds);

                    if (rolesIds == null || !rolesIds.Any())
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    if (rolesIds != null && rolesIds.Any())
                    {
                        if (outIds != null && outIds.Any())
                        {
                            var intersectIds = outIds.Distinct().Intersect(rolesIds.ToList()).ToList();
                            if (intersectIds == null || !intersectIds.Any())
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                            else
                            {
                                outIds.AddRange(intersectIds);
                            }
                        }
                        else
                        {
                            outIds.AddRange(rolesIds);
                        }
                    }
                }
                #endregion

                #region Names
                /*
                    Before proceeding with names, I think here we should first examine if we have collected any record keys in "outIds" 
                    and if there are any then get those records and compare the names provided in the filter as long as personFilter named 
                    query is also not provided. If personFilter named query parameter is provided then we have to call the CTX to 
                    perform further processing.If preferred name is there then call the CTX
                */
                //check if there is preferred name
                bool ispreferred = false;
                bool useCTX = outIds.Count() >= bulkReadSize;
                if (personFilterCriteria != null && personFilterCriteria.Names != null && personFilterCriteria.Names.Any() && personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.PreferredName)).Any())
                    ispreferred = true;

                if (outIds != null && outIds.Any() && !useCTX && string.IsNullOrEmpty(personFilter) && !ispreferred)
                {
                    if (personFilterCriteria != null && personFilterCriteria.Names != null && personFilterCriteria.Names.Any())
                    {
                        people = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", outIds.Distinct().ToArray());

                        if (people == null || !people.Any())
                        {
                            return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                        }

                        Data.Base.DataContracts.Dflts defaults = await DataReader.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS");
                        var lookupType = defaults.DfltsLookupType ?? "S";

                        List<string> nameIds = new List<string>();
                        List<string> titleIds = new List<string>();
                        List<string> pedigreeIds = new List<string>();
                        List<string> fnameIds = new List<string>();
                        List<string> lnameIds = new List<string>();
                        List<string> mnameIds = new List<string>();
                        List<string> lNamePrefixIds = new List<string>();
                        List<string> pnameIds = new List<string>();

                        var titles = personFilterCriteria.Names.Where(t => !string.IsNullOrEmpty(t.Title)).Select(i => i.Title).ToList();
                        var pedigrees = personFilterCriteria.Names.Where(t => !string.IsNullOrEmpty(t.Pedigree)).Select(i => i.Pedigree).ToList();
                        var firstNames = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.FirstName)).Select(i => i.FirstName).ToList();
                        var lastNames = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.LastName)).Select(i => i.LastName).ToList();
                        var PrefNames = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.PreferredName)).Select(i => i.PreferredName).ToList();
                        var middleNames = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.MiddleName)).Select(i => i.MiddleName).ToList();
                        var prefix = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.LastNamePrefix)).Select(i => i.LastNamePrefix).ToList();

                        #region  Titles
                        if (titles != null && titles.Any())
                        {
                            List<DataContracts.Person> titleList = people.ToList();

                            foreach (var title in titles)
                            {
                                titleList = titleList.Where(p => p != null && (!string.IsNullOrEmpty(p.Prefix) && p.Prefix.ToUpperInvariant()
                                                     .Equals(title.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)))
                                                     .ToList();

                                if (titleList == null || !titleList.Any())
                                {
                                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                }
                            }

                            if (titleList != null || titleList.Any())
                            {
                                if (nameIds != null && nameIds.Any())
                                {
                                    var intersectIds = nameIds.Intersect(titleList.Select(p => p.Recordkey));
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                    nameIds = intersectIds.ToList();
                                }
                                else
                                {
                                    nameIds.AddRange(titleList.Select(p => p.Recordkey));
                                }
                            }
                            else
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                        }
                        #endregion

                        #region  Pedigree
                        if (pedigrees != null && pedigrees.Any())
                        {
                            List<DataContracts.Person> pedigreeList = people.ToList();

                            foreach (var pedigree in pedigrees)
                            {
                                pedigreeList = pedigreeList.Where(p => p != null && (!string.IsNullOrEmpty(p.Suffix) && p.Suffix.ToUpperInvariant()
                                                     .Equals(pedigree.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)))
                                                     .ToList();

                                if (pedigreeList == null || !pedigreeList.Any())
                                {
                                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                }
                            }

                            if (pedigreeList != null || pedigreeList.Any())
                            {
                                if (nameIds != null && nameIds.Any())
                                {
                                    var intersectIds = nameIds.Intersect(pedigreeList.Select(p => p.Recordkey));
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                    nameIds = intersectIds.ToList();
                                }
                                else
                                {
                                    nameIds.AddRange(pedigreeList.Select(p => p.Recordkey));
                                }
                            }
                            else
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                        }
                        #endregion

                        #region First Name
                        if (firstNames != null && firstNames.Any())
                        {
                            List<DataContracts.Person> fNameList = people.ToList();

                            foreach (var fn in firstNames)
                            {
                                if (lookupType.ToUpperInvariant().Equals("S", StringComparison.OrdinalIgnoreCase))
                                {
                                    fNameList = fNameList.Where(p => p != null && !string.IsNullOrEmpty(p.FirstName) && p.FirstName.ToUpperInvariant().Contains(fn.ToUpperInvariant()) ||
                                                                                  !string.IsNullOrEmpty(p.BirthNameFirst) && p.BirthNameFirst.ToUpperInvariant().Contains(fn.ToUpperInvariant()) ||
                                                                                  !string.IsNullOrEmpty(p.PersonChosenFirstName) && p.PersonChosenFirstName.ToUpperInvariant().Contains(fn.ToUpperInvariant()) ||
                                                                                  (p.NameHistoryFirstName != null && p.NameHistoryFirstName
                                                                                  .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().Contains(fn.ToUpperInvariant())))).ToList();
                                }
                                else
                                {
                                    fNameList = fNameList.Where(p => p != null && !string.IsNullOrEmpty(p.FirstName) && p.FirstName.ToUpperInvariant().StartsWith(fn.ToUpperInvariant()) ||
                                              !string.IsNullOrEmpty(p.BirthNameFirst) && p.BirthNameFirst.ToUpperInvariant().StartsWith(fn.ToUpperInvariant()) ||
                                              !string.IsNullOrEmpty(p.PersonChosenFirstName) && p.PersonChosenFirstName.ToUpperInvariant().StartsWith(fn.ToUpperInvariant()) ||
                                              (p.NameHistoryFirstName != null && p.NameHistoryFirstName
                                              .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().StartsWith(fn.ToUpperInvariant())))).ToList();

                                }

                                if (fNameList == null || !fNameList.Any())
                                {
                                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                }
                            }

                            if (fNameList != null || fNameList.Any())
                            {
                                if (nameIds != null && nameIds.Any())
                                {
                                    var intersectIds = nameIds.Intersect(fNameList.Select(p => p.Recordkey));
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                    nameIds = intersectIds.ToList();
                                }
                                else
                                {
                                    nameIds.AddRange(fNameList.Select(p => p.Recordkey));
                                }
                            }
                            else
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                        }

                        #endregion

                        #region Middle Name
                        if (middleNames != null && middleNames.Any())
                        {
                            List<DataContracts.Person> mNameList = people.ToList();

                            foreach (var mn in middleNames)
                            {
                                if (lookupType.ToUpperInvariant().Equals("S", StringComparison.OrdinalIgnoreCase))
                                {
                                    mNameList = mNameList.Where(p => p != null && !string.IsNullOrEmpty(p.MiddleName) &&
                                                                              !string.IsNullOrEmpty(p.MiddleName) && p.MiddleName.ToUpperInvariant().Contains(mn.ToUpperInvariant()) ||
                                                                              !string.IsNullOrEmpty(p.BirthNameMiddle) && p.BirthNameMiddle.ToUpperInvariant().Contains(mn.ToUpperInvariant()) ||
                                                                              !string.IsNullOrEmpty(p.PersonChosenMiddleName) && p.PersonChosenMiddleName.ToUpperInvariant().Contains(mn.ToUpperInvariant()) ||
                                                                              (p.NameHistoryMiddleName != null && p.NameHistoryMiddleName
                                                                              .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().Contains(mn.ToUpperInvariant())))).ToList();
                                }
                                else
                                {
                                    mNameList = mNameList.Where(p => p != null && !string.IsNullOrEmpty(p.MiddleName) &&
                                                                             !string.IsNullOrEmpty(p.MiddleName) && p.MiddleName.ToUpperInvariant().StartsWith(mn.ToUpperInvariant()) ||
                                                                             !string.IsNullOrEmpty(p.BirthNameMiddle) && p.BirthNameMiddle.ToUpperInvariant().StartsWith(mn.ToUpperInvariant()) ||
                                                                             !string.IsNullOrEmpty(p.PersonChosenMiddleName) && p.PersonChosenMiddleName.ToUpperInvariant().StartsWith(mn.ToUpperInvariant()) ||
                                                                             (p.NameHistoryMiddleName != null && p.NameHistoryMiddleName
                                                                             .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().StartsWith(mn.ToUpperInvariant())))).ToList();
                                }

                                if (mNameList == null || !mNameList.Any())
                                {
                                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                }
                            }

                            if (mNameList != null || mNameList.Any())
                            {
                                if (nameIds != null && nameIds.Any())
                                {
                                    var intersectIds = nameIds.Intersect(mNameList.Select(p => p.Recordkey));
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                    nameIds = intersectIds.ToList();
                                }
                                else
                                {
                                    nameIds.AddRange(mNameList.Select(p => p.Recordkey));
                                }
                            }
                            else
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                        }

                        #endregion

                        #region LastName
                        if (lastNames != null && lastNames.Any())
                        {
                            List<DataContracts.Person> lNameList = people.ToList();

                            foreach (var ln in lastNames)
                            {
                                if (lookupType.ToUpperInvariant().Equals("S", StringComparison.OrdinalIgnoreCase))
                                {
                                    lNameList = lNameList.Where(p => p != null && !string.IsNullOrEmpty(p.LastName) && p.LastName.ToUpperInvariant().Contains(ln.ToUpperInvariant()) ||
                                                                              !string.IsNullOrEmpty(p.BirthNameLast) && p.BirthNameLast.ToUpperInvariant().Contains(ln.ToUpperInvariant()) ||
                                                                              (p.NameHistoryLastName != null && p.NameHistoryLastName
                                                                              .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().Contains(ln.ToUpperInvariant())))).ToList();
                                }
                                else
                                {
                                    lNameList = lNameList.Where(p => p != null && !string.IsNullOrEmpty(p.LastName) && p.LastName.ToUpperInvariant().StartsWith(ln.ToUpperInvariant()) ||
                                                         !string.IsNullOrEmpty(p.BirthNameLast) && p.BirthNameLast.ToUpperInvariant().StartsWith(ln.ToUpperInvariant()) ||
                                                         (p.NameHistoryLastName != null && p.NameHistoryLastName
                                                         .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().StartsWith(ln.ToUpperInvariant())))).ToList();
                                }

                                if (lNameList == null || !lNameList.Any())
                                {
                                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                }
                            }

                            if (lNameList != null || lNameList.Any())
                            {
                                if (nameIds != null && nameIds.Any())
                                {
                                    var intersectIds = nameIds.Intersect(lNameList.Select(p => p.Recordkey));
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                    nameIds = intersectIds.ToList();
                                }
                                else
                                {
                                    nameIds.AddRange(lNameList.Select(p => p.Recordkey));
                                }
                            }
                            else
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                        }

                        #endregion                        

                        #region LastNamePrefix
                        if (prefix != null && prefix.Any())
                        {
                            List<DataContracts.Person> prefixesList = people.ToList();

                            foreach (var pfx in prefix)
                            {
                                if (lookupType.ToUpperInvariant().Equals("S", StringComparison.OrdinalIgnoreCase))
                                {
                                    prefixesList = prefixesList.Where(p => p != null && !string.IsNullOrEmpty(p.LastName) && p.LastName.ToUpperInvariant().Contains(pfx.ToUpperInvariant()) ||
                                                                                    !string.IsNullOrEmpty(p.BirthNameLast) && p.BirthNameLast.ToUpperInvariant().Contains(pfx.ToUpperInvariant()) ||
                                                                                    !string.IsNullOrEmpty(p.PersonChosenLastName) && p.PersonChosenLastName.ToUpperInvariant().Contains(pfx.ToUpperInvariant()) ||
                                                                                    (p.NameHistoryLastName != null && p.NameHistoryLastName
                                                                                    .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().Contains(pfx.ToUpperInvariant())))).ToList();
                                }
                                else
                                {
                                    prefixesList = prefixesList.Where(p => p != null && !string.IsNullOrEmpty(p.LastName) && p.LastName.ToUpperInvariant().StartsWith(pfx.ToUpperInvariant()) ||
                                                                                    !string.IsNullOrEmpty(p.BirthNameLast) && p.BirthNameLast.ToUpperInvariant().StartsWith(pfx.ToUpperInvariant()) ||
                                                                                    !string.IsNullOrEmpty(p.PersonChosenLastName) && p.PersonChosenLastName.ToUpperInvariant().StartsWith(pfx.ToUpperInvariant()) ||
                                                                                    (p.NameHistoryLastName != null && p.NameHistoryLastName
                                                                                    .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().StartsWith(pfx.ToUpperInvariant())))).ToList();
                                }
                                if (prefixesList == null || !prefixesList.Any())
                                {
                                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                }
                            }

                            if (prefixesList != null || prefixesList.Any())
                            {
                                if (nameIds != null && nameIds.Any())
                                {
                                    var intersectIds = nameIds.Intersect(prefixesList.Select(p => p.Recordkey));
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                                    }
                                    nameIds = intersectIds.ToList();
                                }
                                else
                                {
                                    nameIds.AddRange(prefixesList.Select(p => p.Recordkey));
                                }
                            }
                            else
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                        }

                        #endregion

                        if (nameIds == null || !nameIds.Any())
                        {
                            return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                        }
                        else
                        {
                            var intersectIds = outIds.Intersect(nameIds);
                            if (intersectIds == null || !intersectIds.Any())
                            {
                                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                            }
                            else
                            {
                                outIds = intersectIds.Distinct().ToList();
                            }
                        }
                    }

                    #endregion

                    if (outIds == null || !outIds.Any())
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    var sublist = (await DataReader.SelectAsync("PERSON", outIds.ToArray(), criteria)).ToList();

                    if (sublist == null || !sublist.Any())
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    totalCount = sublist.Distinct().Count();
                    personGuids = await GetGuidsAsync(offset, limit, sublist.Distinct().ToList());
                }
                //if(personFilter value is provided then call the ctx.
                else if ((personFilterCriteria != null && personFilterCriteria.Names != null) || !string.IsNullOrEmpty(personFilter) || useCTX)
                {
                    List<GetPersonFilterResultsV2Names> names = new List<GetPersonFilterResultsV2Names>();
                    if (personFilterCriteria != null && personFilterCriteria.Names != null && personFilterCriteria.Names.Any())
                    {
                        personFilterCriteria.Names.ForEach(i =>
                        {
                            GetPersonFilterResultsV2Names name = new GetPersonFilterResultsV2Names()
                            {
                                FirstName = i.FirstName,
                                LastName = i.LastName,
                                MiddleName = i.MiddleName,
                                LastNamePrefix = i.LastNamePrefix,
                                Title = i.Title,
                                Pedigree = i.Pedigree,
                                PreferredName = i.PreferredName

                            };
                            names.Add(name);
                        });

                    }
                    List<GetPersonFilterResultsV2Credentials> creds = new List<GetPersonFilterResultsV2Credentials>();
                    if (personFilterCriteria != null && personFilterCriteria.Credentials != null && personFilterCriteria.Credentials.Any())
                    {
                        personFilterCriteria.Credentials.ForEach(i =>
                        {
                            GetPersonFilterResultsV2Credentials cred = new GetPersonFilterResultsV2Credentials()
                            {
                                CredentialType = i.Item1,
                                CredentialValue = i.Item2
                            };
                            creds.Add(cred);
                        });
                    }
                    List<GetPersonFilterResultsV2AlternateCredentials> altCreds = new List<GetPersonFilterResultsV2AlternateCredentials>();
                    if (personFilterCriteria != null && personFilterCriteria.AlternativeCredentials != null && personFilterCriteria.AlternativeCredentials.Any())
                    {
                        personFilterCriteria.AlternativeCredentials.ForEach(i =>
                        {
                            GetPersonFilterResultsV2AlternateCredentials cred = new GetPersonFilterResultsV2AlternateCredentials()
                            {
                                AltCredentialType = i.Item1,
                                AltCredentialValue = i.Item2
                            };
                            altCreds.Add(cred);
                        });
                    }
                    var request = new GetPersonFillterResultsV2Request()
                    {
                        PersonIds = outIds,
                        EmailAddresses = personFilterCriteria != null && personFilterCriteria.Emails != null ? personFilterCriteria.Emails : null,
                        SaveListParmsId = personFilterKey,
                        Role = personFilterCriteria != null && personFilterCriteria.Roles != null ? personFilterCriteria.Roles : null,
                        GetPersonFilterResultsV2Names = names,
                        GetPersonFilterResultsV2Credentials = creds,
                        GetPersonFilterResultsV2AlternateCredentials = altCreds,
                        //we do not need to send the Guid again.
                        //Guid = personFilter,
                        Offset = offset,
                        Limit = limit
                    };

                    // Execute request
                    var response = await transactionInvoker.ExecuteAsync<GetPersonFillterResultsV2Request, GetPersonFillterResultsV2Response>(request);

                    if (response.ErrorMessages.Any())
                    {
                        var errorMessage = "Error(s) occurred retrieving person data: ";
                        var exception = new RepositoryException(errorMessage);
                        foreach (var errMsg in response.ErrorMessages)
                        {
                            response.LogStmt.ForEach(i =>
                            {
                                logger.Info(i);
                            });
                            exception.AddError(new Domain.Entities.RepositoryError("person.filter", errMsg));
                            errorMessage += string.Join(Environment.NewLine, errMsg);
                        }
                        throw exception;
                    }

                    if (response.PersonIds == null || !response.PersonIds.Any())
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    Array.Sort(response.PersonIds.ToArray());
                    var personIds = response.PersonIds;
                    var filteredPersonIds = personIds.ToArray();

                    var idLookUpList = new List<RecordKeyLookup>();
                    filteredPersonIds.ForEach(s => idLookUpList.Add(new RecordKeyLookup("PERSON", s, false)));

                    var personGuidLookupList = await DataReader.SelectAsync(idLookUpList.ToArray());
                    foreach (var o in personGuidLookupList)
                    {
                        if (o.Value != null && !string.IsNullOrEmpty(o.Value.Guid))
                        {
                            personGuids.Add(o.Value.Guid);
                        }
                        else
                        {
                            var ex = new RepositoryException();
                            ex.AddError(new Domain.Entities.RepositoryError(string.Format("PERSON record '{0}' is missing a guid.", o.Key.Split('+')[1])));
                            throw ex;
                        }
                    }

                    totalCount = response.TotalRecords.Value;
                }
                return new Tuple<IEnumerable<string>, int>(personGuids, totalCount);

                #endregion
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Get a list of guids associated with faculty
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="title">Specific title</param>
        /// <param name="firstName">Specific first name</param>
        /// <param name="middleName">Specific middle name</param>
        /// <param name="lastNamePrefix">Last name beings with</param>
        /// <param name="lastName">Specific last name</param>
        /// <param name="pedigree">Specific suffix</param>
        /// <param name="preferredName">Specific preferred name</param>
        /// <param name="role">Specific role of a person</param>
        /// <param name="credentialType">Credential type of either colleagueId or ssn</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns>List of person guids </returns>
        public async Task<Tuple<IEnumerable<string>, int>> GetFilteredPerson3GuidsAsync(int offset, int limit, bool bypassCache, PersonFilterCriteria personFilterCriteria, string personFilter = "")
        {

            try
            {
                string personNamesCriteriaString = null;
                if ((personFilterCriteria != null) && (personFilterCriteria.Names != null) && (personFilterCriteria.Names.Any()))
                {
                    personNamesCriteriaString = String.Join(";", personFilterCriteria.Names.Select(x => x.ToString()));
                }

                string filteredPersonsCacheKey = CacheSupport.BuildCacheKey(AllFilteredPersonsCache, personFilter,
                    personFilterCriteria != null ? personFilterCriteria.AlternativeCredentials : null,
                    personFilterCriteria != null ? personFilterCriteria.Credentials : null,
                    personFilterCriteria != null ? personFilterCriteria.Emails : null,
                    personNamesCriteriaString,
                    personFilterCriteria != null ? personFilterCriteria.Roles : null,
                    personFilter);
                int totalCount = 0;

                string[] subList = null;

                string criteria = "WITH PERSON.CORP.INDICATOR NE 'Y' AND WITH LAST.NAME NE ''";
                //int totalCount = 0;
                var personGuids = new List<string>();
                string personFilterKey = string.Empty;
                List<string> outIds = new List<string>(), colleaguePersonIds = null, elevateIds = null, colleagueusernameIds = null;
                Collection<DataContracts.Person> people = null;


                #region No Filters
                //If there are no filter criteria passed then search the PERSON file/table. This is for Get All. 
                if (personFilterCriteria == null && string.IsNullOrWhiteSpace(personFilter))
                {
                    var keyCacheNoFilters = await CacheSupport.GetOrAddKeyCacheToCache(

                        this,
                        ContainsKey,
                        GetOrAddToCacheAsync,
                        AddOrUpdateCacheAsync,
                        transactionInvoker,
                        filteredPersonsCacheKey,
                        "PERSON",
                        offset,
                        limit,
                        AllFilteredPersonsCacheTimeout,
                        async () =>
                        {

                            var keys = new List<string>();

                            //outIds = (await DataReader.SelectAsync("PERSON", criteria)).ToList();

                            var filteredPersonIds = outIds.ToArray();

                            CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                            {

                                limitingKeys = keys.Distinct().ToList(),
                                criteria = criteria.ToString(),
                            };

                            return requirements;
                        });

                    if (keyCacheNoFilters == null || keyCacheNoFilters.Sublist == null || !keyCacheNoFilters.Sublist.Any())
                    {
                        return new Tuple<IEnumerable<string>, int>(null, 0);
                    }

                    subList = keyCacheNoFilters.Sublist.ToArray();
                    totalCount = keyCacheNoFilters.TotalCount.Value;

                    if (totalCount <= offset)
                    {
                        return new Tuple<IEnumerable<string>, int>(null, 0);
                    }

                    var idLookUpList = new List<RecordKeyLookup>();
                    subList.ForEach(s => idLookUpList.Add(new RecordKeyLookup("PERSON", s, false)));

                    var personGuidLookupList = await DataReader.SelectAsync(idLookUpList.ToArray());
                    foreach (var o in personGuidLookupList)
                    {
                        if (o.Value != null && !string.IsNullOrEmpty(o.Value.Guid))
                        {
                            personGuids.Add(o.Value.Guid);
                        }
                        else
                        {
                            var ex = new RepositoryException();
                            ex.AddError(new Domain.Entities.RepositoryError(string.Format("PERSON record '{0}' is missing a guid.", o.Key.Split('+')[1])));
                            throw ex;
                        }
                    }
                    return personGuids != null && personGuids.Any() ? new Tuple<IEnumerable<string>, int>(personGuids, totalCount) : new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                }
                #endregion

                #region Filters                
                /*
                    Here check the validity of the personFilter first, if a bad guid is passed then nothing needs to get processed
                    since we are using "AND" clause between all the params passed between personFilterCriteria and personFilter
                */
                if (!string.IsNullOrEmpty(personFilter))
                {
                    var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(personFilter) });
                    if (idDict == null || idDict.Count == 0)
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    var foundEntry = idDict.FirstOrDefault();
                    if (foundEntry.Value == null)
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }

                    if (foundEntry.Value.Entity != "SAVE.LIST.PARMS")
                    {
                        return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                    }
                    personFilterKey = foundEntry.Value.PrimaryKey;
                }


                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(

                        this,
                        ContainsKey,
                        GetOrAddToCacheAsync,
                        AddOrUpdateCacheAsync,
                        transactionInvoker,
                        filteredPersonsCacheKey,
                        "",
                        offset,
                        limit,
                        AllFilteredPersonsCacheTimeout,
                        async () =>
                        {

                            var keys = new List<string>();

                            #region Start with credentials and get first record out and then compare with others

                            #region Credentials
                            if (personFilterCriteria != null && personFilterCriteria.Credentials != null && personFilterCriteria.Credentials.Any())
                            {
                                foreach (var cred in personFilterCriteria.Credentials)
                                {
                                    if (cred.Item1.ToLowerInvariant() == "colleaguepersonid")
                                    {
                                        colleaguePersonIds = new List<string>();
                                        // to address issue if there is single quote around ColleagueId
                                        var personId = cred.Item2;
                                        if (personId.Contains("'"))
                                        {
                                            personId = personId.Replace("'", string.Empty);
                                        }
                                        var ids = await DataReader.SelectAsync("PERSON", new string[] { personId }, string.Empty);
                                        if (ids == null || !ids.Any())
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                        else
                                        {
                                            colleaguePersonIds.AddRange(ids);
                                            if (ids != null && ids.Any() && elevateIds != null && elevateIds.Any())
                                            {
                                                var intersectIds = ids.Intersect(elevateIds);
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                            }
                                            else if (ids != null && ids.Any() && colleagueusernameIds != null && colleagueusernameIds.Any())
                                            {
                                                var intersectIds = ids.Intersect(colleagueusernameIds);
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                            }
                                            else
                                            {
                                                outIds.AddRange(ids);
                                            }
                                        }
                                    }
                                    else if (cred.Item1.ToLowerInvariant() == "elevateid")
                                    {
                                        elevateIds = new List<string>();
                                        //there is index in alternate Ids so we can use first and then filter the rest. 
                                        var altIds = await DataReader.SelectAsync("PERSON", outIds != null && outIds.Any() ? outIds.ToArray() : null,
                                                                 string.Format("WITH PERSON.ALT.IDS EQ '{0}'", cred.Item2));
                                        if (altIds == null || !altIds.Any())
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                        else
                                        {
                                            //apply the check for alternate Id type of Elevate here
                                            var elevatePersonIds = new List<string>();
                                            var elevatePersons = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", altIds.ToArray());

                                            if ((elevatePersons != null) && (elevatePersons.Any()))
                                            {
                                                foreach (var elevatePerson in elevatePersons)
                                                {
                                                    var personAltEntityAssociations = elevatePerson.PersonAltEntityAssociation;
                                                    if ((personAltEntityAssociations != null) && (personAltEntityAssociations.Any())
                                                        && (personAltEntityAssociations.Any(x => x.PersonAltIdTypesAssocMember == "ELEV" && x.PersonAltIdsAssocMember == cred.Item2)))
                                                    {
                                                        elevatePersonIds.Add(elevatePerson.Recordkey);
                                                    }
                                                }
                                            }

                                            //apply the check for alternate Id type of Elevate here

                                            if (elevatePersonIds == null || !elevatePersonIds.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                            }
                                            elevateIds.AddRange(elevatePersonIds);
                                            if (elevatePersonIds != null && elevatePersonIds.Any() && colleaguePersonIds != null && colleaguePersonIds.Any())
                                            {
                                                var intersectIds = elevatePersonIds.Intersect(colleaguePersonIds);
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                            }
                                            else if (elevatePersonIds != null && elevatePersonIds.Any() && colleagueusernameIds != null && colleagueusernameIds.Any())
                                            {
                                                var intersectIds = elevatePersonIds.Intersect(colleagueusernameIds);
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                            }
                                            else
                                            {
                                                outIds.AddRange(elevatePersonIds);
                                            }
                                        }
                                    }
                                    else if (cred.Item1.ToLowerInvariant() == "colleagueusername")
                                    {
                                        colleagueusernameIds = new List<string>();
                                        var ids = (await DataReader.SelectAsync("ORG.ENTITY.ENV", outIds != null && outIds.Any() ? outIds.ToArray() : null,
                                            string.Format("WITH OEE.USERNAME = '{0}' SAVING OEE.RESOURCE", cred.Item2))).ToList();
                                        if (ids == null || !ids.Any())
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                        else
                                        {
                                            ids = (await DataReader.SelectAsync("ORG.ENTITY", ids.ToArray(), null)).ToList();
                                            colleagueusernameIds.AddRange(ids);
                                            if (ids != null && ids.Any() && elevateIds != null && elevateIds.Any())
                                            {
                                                var intersectIds = ids.Intersect(elevateIds);
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                            }
                                            else if (ids != null && ids.Any() && colleaguePersonIds != null && colleaguePersonIds.Any())
                                            {
                                                var intersectIds = ids.Intersect(colleaguePersonIds);
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                            }
                                            else
                                            {
                                                outIds.AddRange(ids);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region AlternativeCredentials
                            if (personFilterCriteria != null && personFilterCriteria.AlternativeCredentials != null && personFilterCriteria.AlternativeCredentials.Any())
                            {
                                foreach (var cred in personFilterCriteria.AlternativeCredentials)
                                {
                                    //there is index in alternate Ids so we can use first and then filter the rest. 
                                    var altIds = await DataReader.SelectAsync("PERSON", outIds != null && outIds.Any() ? outIds.ToArray() : null,
                                                             string.Format("WITH PERSON.ALT.IDS EQ '{0}'", cred.Item2));
                                    if (altIds == null || !altIds.Any())
                                    {
                                        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(cred.Item1))
                                        {
                                            outIds.AddRange(altIds);
                                        }
                                        else
                                        {
                                            //apply the check for alternate Id type here
                                            var alternatePersonIds = new List<string>();
                                            var alternatePersons = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", altIds.ToArray());

                                            if ((alternatePersons != null) && (alternatePersons.Any()))
                                            {
                                                foreach (var alternatePerson in alternatePersons)
                                                {
                                                    var personAltEntityAssociations = alternatePerson.PersonAltEntityAssociation;
                                                    if ((personAltEntityAssociations != null) && (personAltEntityAssociations.Any())
                                                        && (personAltEntityAssociations.Any(x => x.PersonAltIdTypesAssocMember == cred.Item1.ToUpperInvariant() && x.PersonAltIdsAssocMember == cred.Item2)))
                                                    {
                                                        alternatePersonIds.Add(alternatePerson.Recordkey);
                                                    }
                                                }
                                            }

                                            if (alternatePersonIds == null || !alternatePersonIds.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                            }
                                            outIds.AddRange(alternatePersonIds);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Emails
                            if (personFilterCriteria != null && personFilterCriteria.Emails != null && personFilterCriteria.Emails.Any())
                            {
                                var emailCriteria = string.Empty;
                                foreach (var email in personFilterCriteria.Emails)
                                {
                                    if (!string.IsNullOrEmpty(email))
                                    {
                                        if (string.IsNullOrEmpty(emailCriteria))
                                            emailCriteria = string.Format("WITH PERSON.EMAIL.ADDRESSES.IDX EQ '{0}'", email);
                                        else
                                            emailCriteria += string.Format(" AND WITH PERSON.EMAIL.ADDRESSES.IDX EQ '{0}'", email);
                                    }
                                }

                                //we need to pass in the outIds here
                                var emailIds = await DataReader.SelectAsync("PERSON", outIds != null && outIds.Any() ? outIds.ToArray() : null, emailCriteria);
                                if (emailIds == null || !emailIds.Any())
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }

                                if (emailIds != null && emailIds.Any())
                                {
                                    if (outIds != null && outIds.Any())
                                    {
                                        var intersectIds = outIds.Distinct().Intersect(emailIds.ToList()).ToList();
                                        if (intersectIds == null || !intersectIds.Any())
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                        else
                                        {
                                            outIds.AddRange(intersectIds);
                                        }
                                    }
                                    else
                                    {
                                        outIds.AddRange(emailIds);
                                    }
                                }
                            }
                            #endregion

                            #region Roles
                            if (personFilterCriteria != null && personFilterCriteria.Roles != null && personFilterCriteria.Roles.Any())
                            {
                                var rolesIds = await BuildRoleIdsAsync(personFilterCriteria.Roles, outIds);

                                if (rolesIds == null || !rolesIds.Any())
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }

                                if (rolesIds != null && rolesIds.Any())
                                {
                                    if (outIds != null && outIds.Any())
                                    {
                                        var intersectIds = outIds.Distinct().Intersect(rolesIds.ToList()).ToList();
                                        if (intersectIds == null || !intersectIds.Any())
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                        else
                                        {
                                            outIds.AddRange(intersectIds);
                                        }
                                    }
                                    else
                                    {
                                        outIds.AddRange(rolesIds);
                                    }
                                }
                            }
                            #endregion

                            #region Names
                            /*
                                Before proceeding with names, I think here we should first examine if we have collected any record keys in "outIds" 
                                and if there are any then get those records and compare the names provided in the filter as long as personFilter named 
                                query is also not provided. If personFilter named query parameter is provided then we have to call the CTX to 
                                perform further processing.If preferred name is there then call the CTX
                            */
                            //check if there is preferred name
                            bool ispreferred = false;
                            bool useCTX = outIds.Count() >= bulkReadSize;
                            if (personFilterCriteria != null && personFilterCriteria.Names != null && personFilterCriteria.Names.Any() && personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.PreferredName)).Any())
                                ispreferred = true;

                            if (outIds != null && outIds.Any() && !useCTX && string.IsNullOrEmpty(personFilter) && !ispreferred)
                            {
                                if (personFilterCriteria != null && personFilterCriteria.Names != null && personFilterCriteria.Names.Any())
                                {
                                    people = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", outIds.Distinct().ToArray());

                                    if (people == null || !people.Any())
                                    {
                                        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                    }

                                    Data.Base.DataContracts.Dflts defaults = await DataReader.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS");
                                    var lookupType = defaults.DfltsLookupType ?? "S";

                                    List<string> nameIds = new List<string>();
                                    List<string> titleIds = new List<string>();
                                    List<string> pedigreeIds = new List<string>();
                                    List<string> fnameIds = new List<string>();
                                    List<string> lnameIds = new List<string>();
                                    List<string> mnameIds = new List<string>();
                                    List<string> lNamePrefixIds = new List<string>();
                                    List<string> pnameIds = new List<string>();

                                    var titles = personFilterCriteria.Names.Where(t => !string.IsNullOrEmpty(t.Title)).Select(i => i.Title).ToList();
                                    var pedigrees = personFilterCriteria.Names.Where(t => !string.IsNullOrEmpty(t.Pedigree)).Select(i => i.Pedigree).ToList();
                                    var firstNames = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.FirstName)).Select(i => i.FirstName).ToList();
                                    var lastNames = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.LastName)).Select(i => i.LastName).ToList();
                                    var PrefNames = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.PreferredName)).Select(i => i.PreferredName).ToList();
                                    var middleNames = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.MiddleName)).Select(i => i.MiddleName).ToList();
                                    var prefix = personFilterCriteria.Names.Where(p => !string.IsNullOrEmpty(p.LastNamePrefix)).Select(i => i.LastNamePrefix).ToList();

                                    #region  Titles
                                    if (titles != null && titles.Any())
                                    {
                                        List<DataContracts.Person> titleList = people.ToList();

                                        foreach (var title in titles)
                                        {
                                            titleList = titleList.Where(p => p != null && (!string.IsNullOrEmpty(p.Prefix) && p.Prefix.ToUpperInvariant()
                                                                 .Equals(title.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)))
                                                                 .ToList();

                                            if (titleList == null || !titleList.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                            }
                                        }

                                        if (titleList != null || titleList.Any())
                                        {
                                            if (nameIds != null && nameIds.Any())
                                            {
                                                var intersectIds = nameIds.Intersect(titleList.Select(p => p.Recordkey));
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                                nameIds = intersectIds.ToList();
                                            }
                                            else
                                            {
                                                nameIds.AddRange(titleList.Select(p => p.Recordkey));
                                            }
                                        }
                                        else
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                    }
                                    #endregion

                                    #region  Pedigree
                                    if (pedigrees != null && pedigrees.Any())
                                    {
                                        List<DataContracts.Person> pedigreeList = people.ToList();

                                        foreach (var pedigree in pedigrees)
                                        {
                                            pedigreeList = pedigreeList.Where(p => p != null && (!string.IsNullOrEmpty(p.Suffix) && p.Suffix.ToUpperInvariant()
                                                                 .Equals(pedigree.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)))
                                                                 .ToList();

                                            if (pedigreeList == null || !pedigreeList.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                            }
                                        }

                                        if (pedigreeList != null || pedigreeList.Any())
                                        {
                                            if (nameIds != null && nameIds.Any())
                                            {
                                                var intersectIds = nameIds.Intersect(pedigreeList.Select(p => p.Recordkey));
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                                nameIds = intersectIds.ToList();
                                            }
                                            else
                                            {
                                                nameIds.AddRange(pedigreeList.Select(p => p.Recordkey));
                                            }
                                        }
                                        else
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                    }
                                    #endregion

                                    #region First Name
                                    if (firstNames != null && firstNames.Any())
                                    {
                                        List<DataContracts.Person> fNameList = people.ToList();

                                        foreach (var fn in firstNames)
                                        {
                                            if (lookupType.ToUpperInvariant().Equals("S", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fNameList = fNameList.Where(p => p != null && !string.IsNullOrEmpty(p.FirstName) && p.FirstName.ToUpperInvariant().Contains(fn.ToUpperInvariant()) ||
                                                                                              !string.IsNullOrEmpty(p.BirthNameFirst) && p.BirthNameFirst.ToUpperInvariant().Contains(fn.ToUpperInvariant()) ||
                                                                                              !string.IsNullOrEmpty(p.PersonChosenFirstName) && p.PersonChosenFirstName.ToUpperInvariant().Contains(fn.ToUpperInvariant()) ||
                                                                                              (p.NameHistoryFirstName != null && p.NameHistoryFirstName
                                                                                              .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().Contains(fn.ToUpperInvariant())))).ToList();
                                            }
                                            else
                                            {
                                                fNameList = fNameList.Where(p => p != null && !string.IsNullOrEmpty(p.FirstName) && p.FirstName.ToUpperInvariant().StartsWith(fn.ToUpperInvariant()) ||
                                                          !string.IsNullOrEmpty(p.BirthNameFirst) && p.BirthNameFirst.ToUpperInvariant().StartsWith(fn.ToUpperInvariant()) ||
                                                          !string.IsNullOrEmpty(p.PersonChosenFirstName) && p.PersonChosenFirstName.ToUpperInvariant().StartsWith(fn.ToUpperInvariant()) ||
                                                          (p.NameHistoryFirstName != null && p.NameHistoryFirstName
                                                          .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().StartsWith(fn.ToUpperInvariant())))).ToList();

                                            }

                                            if (fNameList == null || !fNameList.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                            }
                                        }

                                        if (fNameList != null || fNameList.Any())
                                        {
                                            if (nameIds != null && nameIds.Any())
                                            {
                                                var intersectIds = nameIds.Intersect(fNameList.Select(p => p.Recordkey));
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                                nameIds = intersectIds.ToList();
                                            }
                                            else
                                            {
                                                nameIds.AddRange(fNameList.Select(p => p.Recordkey));
                                            }
                                        }
                                        else
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                    }

                                    #endregion

                                    #region Middle Name
                                    if (middleNames != null && middleNames.Any())
                                    {
                                        List<DataContracts.Person> mNameList = people.ToList();

                                        foreach (var mn in middleNames)
                                        {
                                            if (lookupType.ToUpperInvariant().Equals("S", StringComparison.OrdinalIgnoreCase))
                                            {
                                                mNameList = mNameList.Where(p => p != null && !string.IsNullOrEmpty(p.MiddleName) &&
                                                                                          !string.IsNullOrEmpty(p.MiddleName) && p.MiddleName.ToUpperInvariant().Contains(mn.ToUpperInvariant()) ||
                                                                                          !string.IsNullOrEmpty(p.BirthNameMiddle) && p.BirthNameMiddle.ToUpperInvariant().Contains(mn.ToUpperInvariant()) ||
                                                                                          !string.IsNullOrEmpty(p.PersonChosenMiddleName) && p.PersonChosenMiddleName.ToUpperInvariant().Contains(mn.ToUpperInvariant()) ||
                                                                                          (p.NameHistoryMiddleName != null && p.NameHistoryMiddleName
                                                                                          .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().Contains(mn.ToUpperInvariant())))).ToList();
                                            }
                                            else
                                            {
                                                mNameList = mNameList.Where(p => p != null && !string.IsNullOrEmpty(p.MiddleName) &&
                                                                                         !string.IsNullOrEmpty(p.MiddleName) && p.MiddleName.ToUpperInvariant().StartsWith(mn.ToUpperInvariant()) ||
                                                                                         !string.IsNullOrEmpty(p.BirthNameMiddle) && p.BirthNameMiddle.ToUpperInvariant().StartsWith(mn.ToUpperInvariant()) ||
                                                                                         !string.IsNullOrEmpty(p.PersonChosenMiddleName) && p.PersonChosenMiddleName.ToUpperInvariant().StartsWith(mn.ToUpperInvariant()) ||
                                                                                         (p.NameHistoryMiddleName != null && p.NameHistoryMiddleName
                                                                                         .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().StartsWith(mn.ToUpperInvariant())))).ToList();
                                            }

                                            if (mNameList == null || !mNameList.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                            }
                                        }

                                        if (mNameList != null || mNameList.Any())
                                        {
                                            if (nameIds != null && nameIds.Any())
                                            {
                                                var intersectIds = nameIds.Intersect(mNameList.Select(p => p.Recordkey));
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                                nameIds = intersectIds.ToList();
                                            }
                                            else
                                            {
                                                nameIds.AddRange(mNameList.Select(p => p.Recordkey));
                                            }
                                        }
                                        else
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                    }

                                    #endregion

                                    #region LastName
                                    if (lastNames != null && lastNames.Any())
                                    {
                                        List<DataContracts.Person> lNameList = people.ToList();

                                        foreach (var ln in lastNames)
                                        {
                                            if (lookupType.ToUpperInvariant().Equals("S", StringComparison.OrdinalIgnoreCase))
                                            {
                                                lNameList = lNameList.Where(p => p != null && !string.IsNullOrEmpty(p.LastName) && p.LastName.ToUpperInvariant().Contains(ln.ToUpperInvariant()) ||
                                                                                          !string.IsNullOrEmpty(p.BirthNameLast) && p.BirthNameLast.ToUpperInvariant().Contains(ln.ToUpperInvariant()) ||
                                                                                          (p.NameHistoryLastName != null && p.NameHistoryLastName
                                                                                          .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().Contains(ln.ToUpperInvariant())))).ToList();
                                            }
                                            else
                                            {
                                                lNameList = lNameList.Where(p => p != null && !string.IsNullOrEmpty(p.LastName) && p.LastName.ToUpperInvariant().StartsWith(ln.ToUpperInvariant()) ||
                                                                     !string.IsNullOrEmpty(p.BirthNameLast) && p.BirthNameLast.ToUpperInvariant().StartsWith(ln.ToUpperInvariant()) ||
                                                                     (p.NameHistoryLastName != null && p.NameHistoryLastName
                                                                     .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().StartsWith(ln.ToUpperInvariant())))).ToList();
                                            }

                                            if (lNameList == null || !lNameList.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                            }
                                        }
                                        if (lNameList != null || lNameList.Any())
                                        {
                                            if (nameIds != null && nameIds.Any())
                                            {
                                                var intersectIds = nameIds.Intersect(lNameList.Select(p => p.Recordkey));
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                                nameIds = intersectIds.ToList();
                                            }
                                            else
                                            {
                                                nameIds.AddRange(lNameList.Select(p => p.Recordkey));
                                            }
                                        }
                                        else
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                    }

                                    #endregion

                                    #region LastNamePrefix
                                    if (prefix != null && prefix.Any())
                                    {
                                        List<DataContracts.Person> prefixesList = people.ToList();

                                        foreach (var pfx in prefix)
                                        {
                                            if (lookupType.ToUpperInvariant().Equals("S", StringComparison.OrdinalIgnoreCase))
                                            {
                                                prefixesList = prefixesList.Where(p => p != null && !string.IsNullOrEmpty(p.LastName) && p.LastName.ToUpperInvariant().Contains(pfx.ToUpperInvariant()) ||
                                                                                                !string.IsNullOrEmpty(p.BirthNameLast) && p.BirthNameLast.ToUpperInvariant().Contains(pfx.ToUpperInvariant()) ||
                                                                                                !string.IsNullOrEmpty(p.PersonChosenLastName) && p.PersonChosenLastName.ToUpperInvariant().Contains(pfx.ToUpperInvariant()) ||
                                                                                                (p.NameHistoryLastName != null && p.NameHistoryLastName
                                                                                                .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().Contains(pfx.ToUpperInvariant())))).ToList();
                                            }
                                            else
                                            {
                                                prefixesList = prefixesList.Where(p => p != null && !string.IsNullOrEmpty(p.LastName) && p.LastName.ToUpperInvariant().StartsWith(pfx.ToUpperInvariant()) ||
                                                                                                !string.IsNullOrEmpty(p.BirthNameLast) && p.BirthNameLast.ToUpperInvariant().StartsWith(pfx.ToUpperInvariant()) ||
                                                                                                !string.IsNullOrEmpty(p.PersonChosenLastName) && p.PersonChosenLastName.ToUpperInvariant().StartsWith(pfx.ToUpperInvariant()) ||
                                                                                                (p.NameHistoryLastName != null && p.NameHistoryLastName
                                                                                                .Any(h => !string.IsNullOrEmpty(h) && h.ToUpperInvariant().StartsWith(pfx.ToUpperInvariant())))).ToList();
                                            }
                                            if (prefixesList == null || !prefixesList.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                            }
                                        }

                                        if (prefixesList != null || prefixesList.Any())
                                        {
                                            if (nameIds != null && nameIds.Any())
                                            {
                                                var intersectIds = nameIds.Intersect(prefixesList.Select(p => p.Recordkey));
                                                if (intersectIds == null || !intersectIds.Any())
                                                {
                                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                                }
                                                nameIds = intersectIds.ToList();
                                            }
                                            else
                                            {
                                                nameIds.AddRange(prefixesList.Select(p => p.Recordkey));
                                            }
                                        }
                                        else
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                    }

                                    #endregion

                                    if (nameIds == null || !nameIds.Any())
                                    {
                                        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                    }
                                    else
                                    {
                                        var intersectIds = outIds.Intersect(nameIds);
                                        if (intersectIds == null || !intersectIds.Any())
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                        else
                                        {
                                            outIds = intersectIds.Distinct().ToList();
                                        }
                                    }
                                }


                                #endregion

                                if (outIds == null || !outIds.Any())
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                keys = outIds;

                            }
                            #endregion

                            #region personFilter
                            //if(personFilter value is provided then call the ctx.
                            else if ((personFilterCriteria != null && personFilterCriteria.Names != null) || !string.IsNullOrEmpty(personFilter) || useCTX)
                            {
                                List<GetPersonFilterResultsV2Names> names = new List<GetPersonFilterResultsV2Names>();
                                if (personFilterCriteria != null && personFilterCriteria.Names != null && personFilterCriteria.Names.Any())
                                {
                                    personFilterCriteria.Names.ForEach(i =>
                                    {
                                        GetPersonFilterResultsV2Names name = new GetPersonFilterResultsV2Names()
                                        {
                                            FirstName = i.FirstName,
                                            LastName = i.LastName,
                                            MiddleName = i.MiddleName,
                                            LastNamePrefix = i.LastNamePrefix,
                                            Title = i.Title,
                                            Pedigree = i.Pedigree,
                                            PreferredName = i.PreferredName

                                        };
                                        names.Add(name);
                                    });

                                }
                                List<GetPersonFilterResultsV2Credentials> creds = new List<GetPersonFilterResultsV2Credentials>();
                                if (personFilterCriteria != null && personFilterCriteria.Credentials != null && personFilterCriteria.Credentials.Any())
                                {
                                    personFilterCriteria.Credentials.ForEach(i =>
                                    {
                                        GetPersonFilterResultsV2Credentials cred = new GetPersonFilterResultsV2Credentials()
                                        {
                                            CredentialType = i.Item1,
                                            CredentialValue = i.Item2
                                        };
                                        creds.Add(cred);
                                    });
                                }
                                List<GetPersonFilterResultsV2AlternateCredentials> altCreds = new List<GetPersonFilterResultsV2AlternateCredentials>();
                                if (personFilterCriteria != null && personFilterCriteria.AlternativeCredentials != null && personFilterCriteria.AlternativeCredentials.Any())
                                {
                                    personFilterCriteria.AlternativeCredentials.ForEach(i =>
                                    {
                                        GetPersonFilterResultsV2AlternateCredentials cred = new GetPersonFilterResultsV2AlternateCredentials()
                                        {
                                            AltCredentialType = i.Item1,
                                            AltCredentialValue = i.Item2
                                        };
                                        altCreds.Add(cred);
                                    });
                                }
                                var request = new GetPersonFillterResultsV2Request()
                                {
                                    PersonIds = outIds,
                                    EmailAddresses = personFilterCriteria != null && personFilterCriteria.Emails != null ? personFilterCriteria.Emails : null,
                                    SaveListParmsId = personFilterKey,
                                    Role = personFilterCriteria != null && personFilterCriteria.Roles != null ? personFilterCriteria.Roles : null,
                                    GetPersonFilterResultsV2Names = names,
                                    GetPersonFilterResultsV2Credentials = creds,
                                    GetPersonFilterResultsV2AlternateCredentials = altCreds,
                                    //we do not need to send the Guid again.
                                    //Guid = personFilter, Pass offset and Limit of zero to bypass paging by the CTX.
                                    Offset = 0,
                                    Limit = 0
                                };

                                // Execute request
                                var response = await transactionInvoker.ExecuteAsync<GetPersonFillterResultsV2Request, GetPersonFillterResultsV2Response>(request);

                                if (response.ErrorMessages.Any())
                                {
                                    var errorMessage = "Error(s) occurred retrieving person data: ";
                                    var exception = new RepositoryException(errorMessage);
                                    foreach (var errMsg in response.ErrorMessages)
                                    {
                                        response.LogStmt.ForEach(i =>
                                        {
                                            logger.Info(i);
                                        });
                                        exception.AddError(new Domain.Entities.RepositoryError("person.filter", errMsg));
                                        errorMessage += string.Join(Environment.NewLine, errMsg);
                                    }
                                    throw exception;
                                }

                                if (response.PersonIds == null || !response.PersonIds.Any())
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }

                                Array.Sort(response.PersonIds.ToArray());
                                keys = response.PersonIds;
                                totalCount = response.TotalRecords.Value;
                            }
                            #endregion

                            CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                            {
                                limitingKeys = keys.Distinct().ToList(),
                                criteria = criteria.ToString(),
                            };

                            return requirements;

                        });

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                if (totalCount == 0)
                    totalCount = keyCache.TotalCount.Value;

                var idLookUpList2 = new List<RecordKeyLookup>();
                subList.ForEach(s => idLookUpList2.Add(new RecordKeyLookup("PERSON", s, false)));

                var personGuidLookupList2 = await DataReader.SelectAsync(idLookUpList2.ToArray());
                foreach (var o in personGuidLookupList2)
                {
                    if (o.Value != null && !string.IsNullOrEmpty(o.Value.Guid))
                    {
                        personGuids.Add(o.Value.Guid);
                    }
                    else
                    {
                        var ex = new RepositoryException();
                        ex.AddError(new Domain.Entities.RepositoryError(string.Format("PERSON record '{0}' is missing a guid.", o.Key.Split('+')[1])));
                        throw ex;
                    }
                }
                return new Tuple<IEnumerable<string>, int>(personGuids, totalCount);
            }
            #endregion

            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets guids based on criteria and list of record keys.
        /// </summary>
        /// <param name="outIds"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private async Task<List<string>> GetGuidsAsync(int offset, int limit, List<string> outIds)
        {
            List<string> outGuids = new List<string>();
            if (outIds != null && outIds.Any())
            {
                var filteredPersonIds = outIds.ToArray();
                Array.Sort(filteredPersonIds);

                var subList = filteredPersonIds.Skip(offset).Take(limit).ToArray();

                var idLookUpList = new List<RecordKeyLookup>();

                subList.ToList().ForEach(s => idLookUpList.Add(new RecordKeyLookup("PERSON", s, false)));

                var personGuidLookupList = await DataReader.SelectAsync(idLookUpList.ToArray());

                foreach (var lookupResult in personGuidLookupList)
                {
                    if (lookupResult.Value != null && !string.IsNullOrEmpty(lookupResult.Value.Guid))
                    {
                        outGuids.Add(lookupResult.Value.Guid);
                    }
                    else
                    {
                        var ex = new RepositoryException();
                        Domain.Entities.RepositoryError repoError = null;
                        var message = string.Empty;
                        try
                        {
                            message = string.Format("PERSON record '{0}' is missing a guid.", lookupResult.Key.Split('+')[1]);
                            repoError = new Domain.Entities.RepositoryError("Bad.Data", message);
                            repoError.SourceId = lookupResult.Key.Split('+')[1];
                        }
                        catch (Exception)
                        {
                            message = string.Format("PERSON record '{0}' is missing a guid.", lookupResult.Key);
                            repoError = new Domain.Entities.RepositoryError("Bad.Data", message);
                            repoError.SourceId = lookupResult.Key;
                        }
                        ex.AddError(repoError);
                        throw ex;
                    }
                }
            }

            return outGuids;
        }

        /// <summary>
        /// Builds Role ids.
        /// </summary>
        /// <param name="roles"></param>
        /// <param name="inIds"></param>
        /// <returns></returns>
        private async Task<List<string>> BuildRoleIdsAsync(List<string> roles, List<string> inIds)
        {
            List<string> rolesIds = new List<string>();
            List<string> outIds = new List<string>();
            string roleCriteria = string.Empty;

            //build the orgId list for the role in the filter
            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        string[] hrperIds = null;

                        switch (role.ToLower())
                        {
                            case "student":
                                if (string.IsNullOrEmpty(roleCriteria))
                                {
                                    roleCriteria = "WITH PER.INTG.ROLE = 'Student'";
                                }
                                else
                                {
                                    roleCriteria = string.Concat(roleCriteria, " AND WITH PER.INTG.ROLE = 'Student'");
                                }
                                rolesIds = (await DataReader.SelectAsync("PERSON", "WITH WHERE.USED EQ 'STUDENTS'", outIds != null && outIds.Any() ? outIds.ToArray() : inIds != null && inIds.Any() ? inIds.ToArray() : null, "")).ToList();
                                if (outIds != null && outIds.Any())
                                {
                                    var intersectIds = outIds.Intersect(rolesIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return null;
                                    }
                                    outIds = intersectIds.ToList();
                                }
                                else
                                {
                                    outIds.AddRange(rolesIds);
                                }
                                break;
                            case "instructor":
                                if (string.IsNullOrEmpty(roleCriteria))
                                {
                                    roleCriteria = "WITH PER.INTG.ROLE = 'Instructor'";
                                }
                                else
                                {
                                    roleCriteria = string.Concat(roleCriteria, " AND WITH PER.INTG.ROLE = 'Instructor'");
                                }

                                rolesIds = (await DataReader.SelectAsync("PERSON", "WITH WHERE.USED EQ 'FACULTY'", outIds != null && outIds.Any() ? outIds.ToArray() : inIds != null && inIds.Any() ? inIds.ToArray() : null, "")).ToList();
                                if (outIds != null && outIds.Any())
                                {
                                    var intersectIds = outIds.Intersect(rolesIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return null;
                                    }
                                    outIds = intersectIds.ToList();
                                }
                                else
                                {
                                    outIds.AddRange(rolesIds);
                                }
                                break;

                            case "employee":
                                if (string.IsNullOrEmpty(roleCriteria))
                                {
                                    roleCriteria = "WITH PER.INTG.ROLE = 'Employee'";
                                }
                                else
                                {
                                    roleCriteria = string.Concat(roleCriteria, " AND WITH PER.INTG.ROLE = 'Employee'");
                                }

                                rolesIds = (await DataReader.SelectAsync("PERSON", "WITH WHERE.USED EQ 'EMPLOYES'", outIds != null && outIds.Any() ? outIds.ToArray() : inIds != null && inIds.Any() ? inIds.ToArray() : null, "")).ToList();

                                if (!rolesIds.Any())
                                {
                                    rolesIds = (await DataReader.SelectAsync("PERSON", "WITH WHERE.USED EQ 'HRPER'", outIds != null && outIds.Any() ? outIds.ToArray() : inIds != null && inIds.Any() ? inIds.ToArray() : null, "")).ToList();
                                }


                                if (outIds != null && outIds.Any())
                                {
                                    var intersectIds = outIds.Intersect(rolesIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return null;
                                    }
                                    outIds = intersectIds.ToList();
                                }
                                else
                                {
                                    outIds.AddRange(rolesIds);
                                }
                                break;
                            case "prospectivestudent":
                                if (string.IsNullOrEmpty(roleCriteria))
                                {
                                    roleCriteria = "WITH PER.INTG.ROLE = 'ProspectiveStudent'";
                                }
                                else
                                {
                                    roleCriteria = string.Concat(roleCriteria, " AND WITH PER.INTG.ROLE = 'ProspectiveStudent'");
                                }
                                rolesIds = (await DataReader.SelectAsync("PERSON", "WITH WHERE.USED EQ 'APPLICANTS'", outIds != null && outIds.Any() ? outIds.ToArray() : inIds != null && inIds.Any() ? inIds.ToArray() : null, String.Empty)).ToList();
                                if (outIds != null && outIds.Any())
                                {
                                    var intersectIds = outIds.Intersect(rolesIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return null;
                                    }
                                    outIds = intersectIds.ToList();
                                }
                                else
                                {
                                    outIds.AddRange(rolesIds);
                                }
                                break;
                            case "advisor":
                                if (string.IsNullOrEmpty(roleCriteria))
                                {
                                    roleCriteria = "WITH PER.INTG.ROLE = 'Advisor'";
                                }
                                else
                                {
                                    roleCriteria = string.Concat(roleCriteria, " AND WITH PER.INTG.ROLE = 'Advisor'");
                                }
                                // check HR database to see if we have faculty setup there.
                                // the best we can do is check for termination date.  If we have no ID's
                                // returned, then they aren't using HR to track faculty contracts.

                                var limitinglist = outIds != null && outIds.Any() ? outIds.ToArray() : inIds != null && inIds.Any() ? inIds.ToArray() : null;


                                rolesIds = (await DataReader.SelectAsync("PERSON", "WITH WHERE.USED EQ 'FACULTY'", limitinglist)).ToList();
                                if (!rolesIds.Any())
                                {
                                    rolesIds = (await DataReader.SelectAsync("PERSON", "WITH WHERE.USED EQ 'HRPER'", limitinglist)).ToList();
                                }

                                var limitinglist2 = rolesIds != null && rolesIds.Any() ? rolesIds.ToArray() : inIds != null && inIds.Any() ? inIds.ToArray() : null;

                                var advisorIds = (await DataReader.SelectAsync("FACULTY", "WITH FAC.ADVISE.FLAG = 'Y'", limitinglist2)).ToList();


                                if (outIds != null && outIds.Any())
                                {
                                    var intersectIds = outIds.Intersect(advisorIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return null;
                                    }
                                    outIds = intersectIds.ToList();
                                }
                                else
                                {
                                    outIds.AddRange(advisorIds);
                                }
                                break;
                            case "alumni":
                                if (string.IsNullOrEmpty(roleCriteria))
                                {
                                    roleCriteria = "WITH PER.INTG.ROLE = 'Alumni'";
                                }
                                else
                                {
                                    roleCriteria = string.Concat(roleCriteria, " AND WITH PER.INTG.ROLE = 'Alumni'");
                                }
                                break;
                            case "vendor":
                                if (string.IsNullOrEmpty(roleCriteria))
                                {
                                    roleCriteria = "WITH PER.INTG.ROLE = 'Vendor'";
                                }
                                else
                                {
                                    roleCriteria = string.Concat(roleCriteria, " AND WITH PER.INTG.ROLE = 'Vendor'");
                                }
                                rolesIds = (await DataReader.SelectAsync("PERSON", "WITH WHERE.USED EQ 'VENDORS'", outIds != null && outIds.Any() ? outIds.ToArray() : inIds != null && inIds.Any() ? inIds.ToArray() : null, "")).ToList();
                                if (outIds != null && outIds.Any())
                                {
                                    var intersectIds = outIds.Intersect(rolesIds);
                                    if (intersectIds == null || !intersectIds.Any())
                                    {
                                        return null;
                                    }
                                    outIds = intersectIds.ToList();
                                }
                                else
                                {
                                    outIds.AddRange(rolesIds);
                                }
                                break;
                            default:
                                return null;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(roleCriteria))
                {
                    var personIntgKeys = (await DataReader.SelectAsync("PERSON.INTG", inIds != null ? inIds.ToArray() : null, roleCriteria)).ToList();
                    if (personIntgKeys != null || personIntgKeys.Any())
                    {
                        outIds.AddRange(personIntgKeys);
                    }
                }
            }
            return outIds.Distinct().ToList();
        }

        /// <summary>
        /// Get a list of guids associated with organizations
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="role">Specific role of a organization</param>
        /// <param name="credentialType">Credential type of colleagueId</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <returns>List of organization guids associated with faculty</returns>
        public async Task<Tuple<IEnumerable<string>, int>> GetFilteredOrganizationGuidsAsync(int offset, int limit,
            string role, string credentialType, string credentialValue)
        {
            var orgGuids = new List<string>();
            int totalCount = 0;
            string[] subList = null;
            string criteria = "WITH PERSON.CORP.INDICATOR = 'Y' ";

            string organizationsCacheKey = CacheSupport.BuildCacheKey(AllOrganizationsCache, role, credentialType, credentialValue);


            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
               this,
               ContainsKey,
               GetOrAddToCacheAsync,
               AddOrUpdateCacheAsync,
               transactionInvoker,
               organizationsCacheKey,
               "",
               offset,
               limit,
               AllOrganizationsCacheTimeout,
               async () =>
               {

                   if (!string.IsNullOrEmpty(credentialValue))
                   {
                       if (credentialType.Equals("elevateId", StringComparison.OrdinalIgnoreCase))
                           criteria = string.Concat(criteria, " AND WITH PERSON.ALT.ID.TYPES EQ 'ELEV' AND WITH PERSON.ALT.IDS EQ '", credentialValue, "'");
                       else
                           criteria = string.Concat(criteria, " AND WITH ID = '", credentialValue, "'");
                   }

                   //reteieve all the ids from Person with PERSON.CORP.INDICATOR = 'Y'.  This will return both institutions and organizations.
                   var orgIds = (await DataReader.SelectAsync("PERSON", criteria)).ToList();

                   //list of institutions
                   var institutionIds = (await DataReader.SelectAsync("INSTITUTIONS", string.Empty)).ToList();

                   //remove the existing institutions
                   orgIds = orgIds.Except(institutionIds).ToList();

                   List<string> roleOrgList = new List<string>();

                   //time to check for other role filters
                   //build the orgId list for the role in the filter
                   if (!string.IsNullOrEmpty(role) &&
                       (role.Equals("partner", StringComparison.OrdinalIgnoreCase) ||
                       role.Equals("affiliate", StringComparison.OrdinalIgnoreCase) ||
                       role.Equals("constituent", StringComparison.OrdinalIgnoreCase) ||
                       role.Equals("vendor", StringComparison.OrdinalIgnoreCase)))
                   {
                       string roleCriteria = string.Empty;

                       switch (role.ToLower())
                       {
                           case "partner":
                               roleCriteria = "WITH PER.INTG.ROLE = 'Partner'";
                               break;
                           case "affiliate":
                               roleCriteria = "WITH PER.INTG.ROLE = 'Affiliate'";
                               break;
                           case "constituent":
                               roleCriteria = "WITH PER.INTG.ROLE = 'Constituent'";
                               break;
                           case "vendor":
                               roleCriteria = "WITH PER.INTG.ROLE = 'Vendor'";
                               break;
                       }

                       roleOrgList = (await DataReader.SelectAsync("PERSON.INTG", roleCriteria)).ToList();
                   }

                   //we have a list of orgs without institutions
                   //we have a list or per intg ids with vendor role
                   //we have to get a list or orgs without insitutions that has whre.used vendor
                   //take those three list and find the correct intersections and we now have a full list of orgs with vendor where.used or per.intg role
                   if (!string.IsNullOrEmpty(role) && role.Equals("vendor", StringComparison.OrdinalIgnoreCase))
                   {
                       criteria = string.Concat(criteria, " AND WITH WHERE.USED = 'VENDORS'");

                       //get orgs that have vendor where used and remove institutions
                       //original code
                       var orgIdsUsedVendor = (await DataReader.SelectAsync("PERSON", criteria)).ToList();
                       orgIdsUsedVendor = orgIdsUsedVendor.Except(institutionIds).ToList();

                       //orgs that are vendors from either per.intg or where.sed will get added in this list
                       var vendorOrgResultList = new List<string>();

                       if (roleOrgList.Any())
                       {
                           //intersect the list of orgs and perintg with role of vendor
                           var orgsWithVendorPerIntgRole = orgIds.Intersect(roleOrgList);

                           //add intersect result to result list
                           vendorOrgResultList.AddRange(orgsWithVendorPerIntgRole);
                       }

                       //add list of orgs that have where.used vendor
                       vendorOrgResultList.AddRange(orgIdsUsedVendor);
                       //select distinct from the combinded list, repeats could happen
                       orgIds = vendorOrgResultList.Distinct().ToList();
                   }
                   else if (roleOrgList.Any())
                   {
                       //this means it was something other than the vendor role so just remove anything from the filtered list 
                       //if it isnt in the list of roleOrgids
                       orgIds.RemoveAll(o => !roleOrgList.Exists(r => r.Equals(o, StringComparison.OrdinalIgnoreCase)));
                   }
                   else if (!roleOrgList.Any() && !string.IsNullOrEmpty(role) && (role.Equals("partner", StringComparison.OrdinalIgnoreCase) ||
                       role.Equals("affiliate", StringComparison.OrdinalIgnoreCase) ||
                       role.Equals("constituent", StringComparison.OrdinalIgnoreCase)))
                   {
                       //this means nothing matched the roles so return nothing
                       return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                   }

                   if (orgIds == null || !orgIds.Any())
                   {
                       return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                   }

                   return new CacheSupport.KeyCacheRequirements()
                   {
                       limitingKeys = orgIds.Distinct().ToList()
                   };
               }
            );

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<string>, int>(new List<string>(), 0);
            }

            subList = keyCache.Sublist.ToArray();

            totalCount = keyCache.TotalCount.Value;

            //return to array form the list now that insitutions have been removed
            //var filteredOrgIds = orgIds.ToArray();

            // totalCount = filteredOrgIds.Count();

            //Array.Sort(filteredOrgIds);

            //var subList = filteredOrgIds.Skip(offset).Take(limit).ToArray();

            var idLookUpList = new List<RecordKeyLookup>();
            subList.ForEach(s => idLookUpList.Add(new RecordKeyLookup("PERSON", s, false)));

            var orgGuidLookupList = await DataReader.SelectAsync(idLookUpList.ToArray());
            //IF there is no guid then we need throw an exception
            if (orgGuidLookupList != null && orgGuidLookupList.Any())
            {
                foreach (var org in orgGuidLookupList)
                {
                    if (org.Value != null && !string.IsNullOrEmpty(org.Value.Guid))
                    {
                        orgGuids.Add(org.Value.Guid);
                    }
                    else
                    {
                        exception.AddError(new RepositoryError("GUID.Not.Found", string.Format("Organization record '{0}' is missing a GUID.", org.Key.Split('+')[1]))
                        {
                            SourceId = org.Key.Split('+')[1]
                        });
                    }
                }
            }
            else
            {
                exception.AddError(new RepositoryError("GUID.Not.Found", "Organization GUIDs are missing."));

            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<string>, int>(orgGuids, totalCount);
        }

        #endregion

        #region Search by Name
        /// <summary>
        /// We are querying the SORT.NAME, a computed column on PERSON, an upper-cased field which appends last first middle name
        /// Second query against partial last name and nickname, in the format entered by the user.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <param name="lastName"></param>
        /// <returns>list of Ids</returns>
        public async Task<IEnumerable<string>> SearchByNameAsync(string lastName, string firstName = null, string middleName = null)
        {
            var watch = new Stopwatch();
            watch.Start();

            if (lastName == null || lastName.Trim().Count() < 2)
            {
                throw new ArgumentNullException("lastName", "Supplied last name must be at least two characters");
            }

            // Trim spaces from each name part
            lastName = lastName.Trim();
            firstName = (firstName != null) ? firstName = firstName.Trim() : null;
            middleName = (middleName != null) ? middleName = middleName.Trim() : null;

            // Call transaction that returns search string
            var lookupStringRequest = new GetPersonLookupStringRequest();
            lookupStringRequest.SearchString = lastName + "," + firstName + " " + middleName;
            var response = await transactionInvoker.ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(lookupStringRequest);

            var persons = new string[] { };
            string searchString = string.Empty;

            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Debug("Transaction GetPersonLookupStringRequest returns following response :" + response.IndexString);
                // Transaction returns something like ;PARTIAL.NAME.INDEX SMITH_BL. Parse out into valid query clause: WITH PARTIAL.NAME.INDEX EQ SMITH_BL
                var searchArray = (response.IndexString.Replace(";", string.Empty)).Split(' ');
                searchString = "WITH " + searchArray.ElementAt(0) + " EQ " + "\"" + searchArray.ElementAt(1) + "\"";
                logger.Debug("String passed for PERSON select :" + searchString);

                // Select on person
                persons = await DataReader.SelectAsync("PERSON", searchString);
            }

            watch.Stop();
            logger.Debug("    STEPX.1.1 Select PERSON " + searchString + " ... completed in " + watch.ElapsedMilliseconds.ToString());
            if (persons != null)
            {
                logger.Debug("    STEPX.1.1 Select found " + persons.Count() + " PERSONS with search string " + searchString);
            }

            return persons;
        }


        /// <summary>
        /// We are querying the SORT.NAME, a computed column on PERSON, an upper-cased field which appends last first middle name
        /// Second query against partial last name and nickname, in the format entered by the user.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <param name="lastName"></param>
        /// <returns>list of Ids</returns>
        public async Task<IEnumerable<string>> SearchByNameForExactMatchAsync(string lastName, string firstName = null, string middleName = null)
        {
            List<string> persons = new List<string>();
            string searchString = string.Empty;
            var watch = new Stopwatch();
            watch.Start();

            if (lastName == null || lastName.Trim().Count() < 2)
            {
                throw new ArgumentNullException("lastName", "Supplied last name must be at least two characters");
            }

            // Trim spaces from each name part
            lastName = lastName.Trim();
            firstName = (firstName != null) ? firstName = firstName.Trim() : null;
            middleName = (middleName != null) ? middleName = middleName.Trim() : null;

            // Call transaction that returns search string
            var lookupStringRequest = new GetPersonSearchKeyListRequest();
            lookupStringRequest.SearchString = lastName + "," + firstName + " " + middleName;
            var response = await transactionInvoker.ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(lookupStringRequest);
            watch.Stop();
            logger.Debug("Transaction GetPersonSearchKeyListRequest completed in " + watch.ElapsedMilliseconds.ToString());

            if (response == null)
            {
                logger.Error("Response from transaction GetPersonSearchKeyListRequest is null");
                return persons;
            }
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.Error("Response have error returned from  transaction GetPersonSearchKeyListRequest- " + response.ErrorMessage);
                return persons;
            }
            if (response.KeyList == null)
            {
                logger.Error("Response from transaction GetPersonSearchKeyListRequest Person Ids are null");
                return persons;

            }
            if (response.KeyList.Count == 0)
            {
                logger.Error("Response from transaction GetPersonSearchKeyListRequest Person Ids count is 0");
                return persons;

            }
            persons = response.KeyList;
            return persons;
        }
        /// <summary>
        /// Retrieves the information for PersonBase for ids provided,
        /// and the matching PersonBases if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following keyword input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="ids">Enumeration of ids for which to retrieve records</param>
        /// <param name="keyword">either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <param name="useCache">True if you want to use cached data</param>
        /// <returns>An enumeration of <see cref="PersonBase">PersonBase</see> information</returns>
        public async Task<IEnumerable<PersonBase>> SearchByIdsOrNamesAsync(IEnumerable<string> ids, string keyword, bool useCache = true)
        {
            if ((ids == null || !ids.Any()) && string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentNullException("keyword");
            }

            List<PersonBase> personBases = new List<PersonBase>();
            List<string> personIds = new List<string>();

            if (ids != null)
            {
                personIds.AddRange(ids);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                // If search string is a numeric ID, add only that ID to the list 
                int personId;
                bool isId = int.TryParse(keyword, out personId);
                if (isId)
                {
                    string id = await PadIdPerPid2ParamsAsync(keyword);
                    personIds.Add(id);
                }
                // If search string is alphanumeric, parse names from query and add matching persons to list
                else
                {
                    string lastName = "";
                    string firstName = "";
                    string middleName = "";
                    ParseNames(keyword, ref lastName, ref firstName, ref middleName);
                    if (string.IsNullOrEmpty(firstName))
                    {
                        throw new ArgumentException("Either an id or a first and last name must be supplied.");
                    }
                    personIds.AddRange(await SearchByNameAsync(lastName, firstName, middleName));
                }
            }
            // If there are no persons, return the empty set
            if (!personIds.Any()) return personBases;

            // Filter out corporations from the set of persons
            personIds = (await FilterOutCorporationsAsync(personIds)).ToList();

            // Build PersonBase objects with full names
            personBases = await GetBaseAsync<PersonBase>(personIds,
                persons =>
                {
                    var personBaseList = new List<PersonBase>();
                    foreach (var person in persons)
                    {

                        personBaseList.Add(new PersonBase(person.Recordkey, person.LastName, person.PrivacyFlag)
                        {
                            MiddleName = person.MiddleName,
                            FirstName = person.FirstName
                        });

                    }
                    return personBaseList;
                }, useCache);

            return personBases;
        }

        private void ParseNames(string criteria, ref string lastName, ref string firstName, ref string middleName)
        {
            // Regular expression for all punctuation and numbers to remove from name string
            Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
            Regex regexNotSpace = new Regex(@"\s");

            var nameStrings = criteria.Split(',');
            // If there was a comma, set the first item to last name
            if (nameStrings.Count() > 1)
            {
                lastName = nameStrings.ElementAt(0).Trim();
                if (nameStrings.Count() >= 2)
                {
                    // parse the two items after the comma using a space. Ignore anything else
                    var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                    if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                    if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                }
            }
            else
            {
                // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                // Blank values don't hurt anything.
                nameStrings = criteria.Split(' ');
                switch (nameStrings.Count())
                {
                    case 1:
                        lastName = nameStrings.ElementAt(0).Trim();
                        break;
                    case 2:
                        firstName = nameStrings.ElementAt(0).Trim();
                        lastName = nameStrings.ElementAt(1).Trim();
                        break;
                    default:
                        firstName = nameStrings.ElementAt(0).Trim();
                        middleName = nameStrings.ElementAt(1).Trim();
                        lastName = nameStrings.ElementAt(2).Trim();
                        break;
                }
            }
            // Remove characters that won't make sense for each name part, including all punctuation and numbers 
            if (lastName != null)
            {
                lastName = regexNotPunc.Replace(lastName, "");
                lastName = regexNotSpace.Replace(lastName, "");
            }
            if (firstName != null)
            {
                firstName = regexNotPunc.Replace(firstName, "");
                firstName = regexNotSpace.Replace(firstName, "");
            }
            if (middleName != null)
            {
                middleName = regexNotPunc.Replace(middleName, "");
                middleName = regexNotSpace.Replace(middleName, "");
            }

        }

        /// <summary>
        /// Takes a colleague entity name and a list of Ids and filters the list of Ids
        /// by selecting against the specified entity.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> FilterByEntityAsync(string entityName, IEnumerable<string> ids, string limitingCriteria = null)
        {
            // If none were found on person, return an empty list
            if (ids == null || ids.Count() == 0)
            {
                return new List<string>();
            }

            // If Ids WERE found on person, limit the list by those that exist on entityName
            var limitedIds = await DataReader.SelectAsync(entityName, ids.ToArray(), limitingCriteria);

            if (limitedIds == null) { return new List<string>(); }

            return limitedIds.ToList();
        }

        /// <summary>
        /// Filters corporations out of a list of person IDs
        /// </summary>
        /// <param name="personIds"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<string>> FilterOutCorporationsAsync(IEnumerable<string> personIds)
        {
            if (personIds == null || !personIds.Any()) return personIds;

            string criteria = "WITH PERSON.CORP.INDICATOR NE 'Y'";

            var nonCorpIds = await FilterByEntityAsync("PERSON", personIds, criteria);
            return nonCorpIds;
        }

        private string QuoteDelimitIds(IEnumerable<string> stringList)
        {
            if (stringList == null || stringList.Select(i => (!string.IsNullOrEmpty(i))).Count() == 0)
            {
                return null;
            }
            else
            {
                return Quote + (string.Join(" ", stringList.ToArray())).Replace(" ", Quote + " " + Quote) + Quote;
            }
        }

        #endregion



        #region Get Person Integration Data

        /// <summary>
        /// Get person addresses, email addresses and phones used for integration.
        /// </summary>
        /// <param name="personId">Person's Colleague ID</param>
        /// <param name="emailAddresses">List of <see cref="EmailAddress"> email addresses</see></param>
        /// <param name="phones">List of <see cref="Phone"> phones</see></param>
        /// <param name="addresses">List of <see cref="Address">addresses</see></param>
        /// <returns>Boolean where true is success and false otherwise</returns>
        public async Task<Tuple<List<EmailAddress>, List<Phone>, List<Domain.Base.Entities.Address>, bool>> GetPersonIntegrationDataAsync(string personId)
        {
            List<Domain.Base.Entities.Address> addresses = new List<Domain.Base.Entities.Address>();
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            List<Phone> phones = new List<Phone>();

            var response = await transactionInvoker.ExecuteAsync<GetPersonAddressesRequest, GetPersonAddressesResponse>(
                new GetPersonAddressesRequest() { PersonId = personId });

            if (response.ErrorMessages.Count() > 0)
            {
                var errorMessage = "Error(s) occurred retrieving person data:";
                errorMessage += string.Join(Environment.NewLine, response.ErrorMessages);
                logger.Error(errorMessage.ToString());
                throw new InvalidOperationException("Error occurred retrieving person data");
            }
            else
            {
                if (response.PersonPhones != null && response.PersonPhones.Count() > 0)
                {
                    // create the phone entities
                    foreach (var phone in response.PersonPhones)
                    {
                        phones.Add(new Phone(phone.PhoneNumber, phone.PhoneType, phone.PhoneExtension));
                    }
                }

                if (response.PersonEmailAddresses != null && response.PersonEmailAddresses.Count() > 0)
                {
                    // create the email address entities
                    foreach (var emailAddress in response.PersonEmailAddresses)
                    {
                        emailAddresses.Add(new EmailAddress(emailAddress.EmailAddressValue, emailAddress.EmailAddressType));
                    }
                }

                if (response.PersonAddresses != null && response.PersonAddresses.Count() > 0)
                {
                    // create the address entities
                    foreach (var address in response.PersonAddresses)
                    {
                        var addressEntity = new Domain.Base.Entities.Address();
                        addressEntity.Type = address.AddressType;
                        addressEntity.City = address.AddressCity;
                        addressEntity.State = address.AddressRegion;
                        addressEntity.PostalCode = address.AddressPostalCode;
                        addressEntity.Country = address.AddressCountry;
                        addressEntity.County = address.AddressCounty;
                        var addressLines = new List<string>();
                        addressLines.Add(address.AddressStreet1);
                        if (!string.IsNullOrEmpty(address.AddressStreet2)) addressLines.Add(address.AddressStreet2);
                        if (!string.IsNullOrEmpty(address.AddressStreet3)) addressLines.Add(address.AddressStreet3);
                        addressEntity.AddressLines = addressLines;
                        addresses.Add(addressEntity);
                    }
                }
            }

            return new Tuple<List<EmailAddress>, List<Phone>, List<Domain.Base.Entities.Address>, bool>(emailAddresses, phones, addresses, true);
        }

        /// <summary>
        /// Get person addresses, email addresses and phones used for integration.
        /// This was originally used by educational-institutions and called by the service.
        /// Since this uses a CTX, all code was put into the InstitutionRepository and the
        /// data is now built into the institution entity.  This therefore is obsolete and
        /// should not be used anywhere.
        /// </summary>
        /// <param name="personId">Person's Colleague ID</param>
        /// <param name="emailAddresses">List of <see cref="EmailAddress"> email addresses</see></param>
        /// <param name="phones">List of <see cref="Phone"> phones</see></param>
        /// <param name="addresses">List of <see cref="Address">addresses</see></param>
        /// <returns>Boolean where true is success and false otherwise</returns>
        public async Task<Tuple<List<EmailAddress>, List<Phone>, List<Domain.Base.Entities.Address>, List<Domain.Base.Entities.SocialMedia>, bool>> GetPersonIntegrationData2Async(string personId)
        {
            List<Domain.Base.Entities.Address> addresses = new List<Domain.Base.Entities.Address>();
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            List<Phone> phones = new List<Phone>();
            List<SocialMedia> socialMedias = new List<SocialMedia>();

            var response = await transactionInvoker.ExecuteAsync<GetPersonContactInfoRequest, GetPersonContactInfoResponse>(
                new GetPersonContactInfoRequest() { PersonId = personId });

            if (response.PersonErrorInfo != null && response.PersonErrorInfo.Any())
            {
                var errorMessage = "Error(s) occurred retrieving person data:";
                foreach (var message in response.PersonErrorInfo)
                {
                    errorMessage += string.Join(Environment.NewLine, message.ErrorMessages);
                }
                logger.Error(errorMessage.ToString());
                throw new InvalidOperationException("Error occurred retrieving person data");
            }
            else
            {
                if (response.PersonPhoneInfo != null && response.PersonPhoneInfo.Any())
                {
                    // create the phone entities
                    foreach (var phone in response.PersonPhoneInfo)
                    {
                        phones.Add(new Phone(phone.PhoneNumber, phone.PhoneType, phone.PhoneExtension)
                        {
                            CountryCallingCode = phone.PhoneCallingCode,
                            IsPreferred = (!string.IsNullOrEmpty(phone.PhonePref) ? phone.PhonePref.Equals("Y", StringComparison.OrdinalIgnoreCase) : false)
                        });
                    }
                }

                if (response.PersonSocialMediaInfo != null && response.PersonSocialMediaInfo.Any())
                {
                    // create the email address entities
                    foreach (var socialMedia in response.PersonSocialMediaInfo)
                    {
                        var preferred = false;
                        if (!string.IsNullOrEmpty(socialMedia.SocialMediaPref))
                        {
                            preferred = socialMedia.SocialMediaPref.Equals("y", StringComparison.OrdinalIgnoreCase);
                        }

                        socialMedias.Add(new SocialMedia(socialMedia.SocialMediaType, socialMedia.SocialMediaHandle, preferred));
                    }
                }

                var personDataContract = await GetPersonContractAsync(personId, false);

                if (personDataContract.PersonEmailAddresses != null && personDataContract.PersonEmailAddresses.Any())
                {
                    for (int i = 0; i < personDataContract.PersonEmailAddresses.Count; i++)
                    {
                        try
                        {
                            var emailAddress = personDataContract.PersonEmailAddresses[i];
                            var emailAddressType = personDataContract.PersonEmailTypes.Count > i
                                ? personDataContract.PersonEmailTypes[i]
                                : null;
                            var emailAddressPreferred = personDataContract.PersonPreferredEmail.Count > i
                                ? personDataContract.PersonPreferredEmail[i]
                                : string.Empty;

                            var emailToAdd = new EmailAddress(emailAddress, emailAddressType)
                            {
                                IsPreferred = emailAddressPreferred.Equals("y", StringComparison.OrdinalIgnoreCase)
                            };

                            emailAddresses.Add(emailToAdd);
                        }
                        catch (Exception ex)
                        {
                            // do not log error
                            //logger.Error(exception, string.Format("Could not load email address for person id {0} with GUID {1}", personDataContract.Recordkey, personDataContract.RecordGuid));
                            logger.Error(ex.Message, "Could not load email address .");
                        }
                    }
                }

                var addressIds = personDataContract.PersonAddresses;

                // Current Addresses First
                if (addressIds.Any())
                {
                    var addressDataContracts = await GetPersonAddressContractsAsync(addressIds);
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
                            addressEntity.Country = address.Country;
                            addressEntity.County = address.County;
                            addressEntity.AddressLines = address.AddressLines;
                            // Find Addrel Association in Person contract
                            var assocEntity = personDataContract.PseasonEntityAssociation.FirstOrDefault(pa => address.Recordkey == pa.PersonAddressesAssocMember);
                            if (assocEntity != null)
                            {
                                //addressEntity.TypeCode = assocEntity.AddrTypeAssocMember.Split(_SM).FirstOrDefault();
                                addressEntity.TypeCode = assocEntity.AddrTypeAssocMember;
                                addressEntity.EffectiveStartDate = assocEntity.AddrEffectiveStartAssocMember;
                                addressEntity.EffectiveEndDate = assocEntity.AddrEffectiveEndAssocMember;
                                addressEntity.SeasonalDates = new List<AddressSeasonalDates>();
                                if (!string.IsNullOrEmpty(assocEntity.AddrSeasonalStartAssocMember) && !string.IsNullOrEmpty(assocEntity.AddrSeasonalEndAssocMember))
                                {
                                    // This could be subvalued so need to split on subvalue mark ASCII 252.
                                    string[] startDate = assocEntity.AddrSeasonalStartAssocMember.Split(SubValueMark);
                                    string[] endDate = assocEntity.AddrSeasonalEndAssocMember.Split(SubValueMark);
                                    for (int i = 0; i < startDate.Length; i++)
                                    {
                                        try
                                        {
                                            // add in the address override phones into the person's list of phones
                                            AddressSeasonalDates seasonalDates = new AddressSeasonalDates(startDate[i], endDate[i]);
                                            addressEntity.SeasonalDates.Add(seasonalDates);
                                        }
                                        catch (Exception ex)
                                        {
                                            var error = "Person address seasonal start/end information is invalid. PersonId: " + personId;

                                            // Log the original exception
                                            logger.Error(ex, error);
                                        }
                                    }
                                }
                            }
                            addressEntity.IsPreferredAddress = (address.Recordkey == personDataContract.PreferredAddress);
                            addressEntity.IsPreferredResidence = (address.Recordkey == personDataContract.PreferredResidence);
                            addressEntity.Status = "Current";
                            addresses.Add(addressEntity);
                        }
                    }
                }
            }

            return new Tuple<List<EmailAddress>, List<Phone>, List<Domain.Base.Entities.Address>, List<Domain.Base.Entities.SocialMedia>, bool>(emailAddresses, phones, addresses, socialMedias, true);
        }

        #endregion

        /// <summary>
        /// In Colleague, returns true if the first special processing associated to the status code contains a "D", indicating 
        /// a deceased status. (Note: A ValActionCode2AssocMember of "V" indicates that the deceased has been verified, but for 
        /// the current need we care only if the person is purported to be deceased, not whether the deceased status has been 
        /// fully verified.)
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private async Task<bool> IsDeceasedAsync(string statusCode)
        {
            try
            {
                var assocEntry = (await GetPersonalStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == statusCode).First();
                return assocEntry.ValActionCode1AssocMember == "D";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Pads/removes padding from person ids depending on pid2 fixed length person setting
        /// </summary>
        /// <param name="keyword">The person Id being searched.</param>
        /// <returns>Returns the person Id that is stored in Colleague</returns>
        protected async Task<string> PadIdPerPid2ParamsAsync(string personId)
        {
            var configuration = await GetConfigurationAsync();
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration", "Error retrieving PID2 configuration");
            }

            //assume there is no fixed length before checking the configuration
            string id = personId;

            //fixed length is given
            if (!String.IsNullOrEmpty(configuration.DfltsFixedLenPerson))
            {
                var personIdMaxLength = Int32.Parse(configuration.DfltsFixedLenPerson);

                //fixed length is given, searched id length is less than or equal to fixed length, pad with 0s accordingly
                if (id.Length <= personIdMaxLength)
                {
                    id = personId.PadLeft(personIdMaxLength, '0');
                }
            }
            return id;
        }

        /// <summary>
        /// Gets Configuration
        /// </summary>
        /// <returns></returns>
        protected async Task<Data.Base.DataContracts.Dflts> GetConfigurationAsync()
        {
            var defaults = await GetOrAddToCacheAsync<Data.Base.DataContracts.Dflts>("Dflts",
                    async () =>
                    {
                        var dflts = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Dflts>("CORE.PARMS", "DEFAULTS");
                        if (dflts == null)
                        {
                            throw new ConfigurationException("Default configuration setup not complete.");
                        }
                        return dflts;
                    }
                );
            return defaults;
        }
    }
}
