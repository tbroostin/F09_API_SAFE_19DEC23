// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Colleague.Domain.Exceptions;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for persons
    /// </summary>
    [RegisterType]
    public class PersonRepository : PersonBaseRepository, IPersonRepository
    {
        private Data.Base.DataContracts.IntlParams internationalParameters;

        /// Initializes a new instance of the <see cref="PersonRepository">PersonRepository</see>/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public PersonRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger, settings)
        {
        }

        #region Person Get methods

        /// <summary>
        /// Get a person entity by guid, without caching.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        public async Task<Domain.Base.Entities.Person> GetPersonByGuidNonCachedAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");

            Domain.Base.Entities.Person personEntity = await GetByGuidNonCachedAsync<Domain.Base.Entities.Person>(guid,
                 person =>
                 {
                     Domain.Base.Entities.Person entity = new Domain.Base.Entities.Person(person.Recordkey, person.LastName);
                     return entity;
                 });

            return personEntity;
        }

        /// <summary>
        /// Get a person entity by guid, without caching for persons-credentials
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        public async Task<Domain.Base.Entities.Person> GetPersonCredentialByGuidNonCachedAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");

            Domain.Base.Entities.Person personEntity = await GetCredentialByGuidNonCachedAsync<Domain.Base.Entities.Person>(guid,
                 person =>
                 {
                     Domain.Base.Entities.Person entity = new Domain.Base.Entities.Person(person.Recordkey, person.LastName);
                     return entity;
                 });

            return personEntity;
        }

        /// <summary>
        /// Get a list of person entities by guid, without caching.
        /// </summary>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <returns>List of <see cref="Person">person</see> entities</returns>
        public async Task<IEnumerable<Domain.Base.Entities.Person>> GetPersonByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get person records.");

            var personEntities = await GetByGuidNonCachedAsync<Domain.Base.Entities.Person>(guids,
                 person =>
                 {
                     Domain.Base.Entities.Person entity = new Domain.Base.Entities.Person(person.Recordkey, person.LastName);
                     return entity;
                 });

            return personEntities;
        }

        /// <summary>
        /// Used to get person entities. Caches information for 2 hours.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="id">Id of the person in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>The extended person entity, such as student</returns>
        public async Task<TDomain> GetAsync<TDomain>(string id, Func<DataContracts.Person, TDomain> objectBuilder, bool useCache = true)
            where TDomain : Domain.Base.Entities.Person
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide an ID to get a record.");
            }
            if (useCache)
            {
                // useCache == True: If item exists in cache, get it from cache. If not in cache, read from Colleague and write to cache.
                return await GetOrAddToCacheAsync<TDomain>(id,
                   async () =>
                   {
                       return await BuildPersonAsync(id, objectBuilder);
                   }, PersonCacheTimeout);
            }
            else
            {
                TDomain personEntity = await BuildPersonAsync(id, objectBuilder);
                await AddOrUpdateCacheAsync<TDomain>(id, personEntity, PersonCacheTimeout);
                return personEntity;
            }
        }

        // For a single person get, implements the common logic that reads from PERSON and invokes the
        // BuildPersons method to return an object of the specified type.
        private async Task<TDomain> BuildPersonAsync<TDomain>(string id, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.Person
        {
            DataContracts.Person record = await DataReader.ReadRecordAsync<DataContracts.Person>("PERSON", id);
            if (record == null)
            {
                throw new ArgumentOutOfRangeException("id", "No person record for ID " + id);
            }
            // Create the specified domain object
            TDomain person = objectBuilder.Invoke(record);
            var personBasedEntities = await BuildPersonsAsync<TDomain>(new List<string>() { id },
                new Collection<DataContracts.Person>() { record }, new Collection<TDomain>() { person });
            return personBasedEntities.FirstOrDefault();
        }

        /// <summary>
        /// Used to retrieve multiple persons. Caches information for 2 hours
        /// </summary>
        /// <typeparam name="TDomain">type of person</typeparam>
        /// <param name="ids">list of persons' ids</param>
        /// <param name="objectBuilder">function to build a single object</param>
        /// <param name="useCache">flag to indicate whether to use cache</param>
        /// <returns>List of objects of specified type</returns>
        public async Task<IEnumerable<TDomain>> GetPersonsAsync<TDomain>(IEnumerable<string> ids, Func<DataContracts.Person, TDomain> objectBuilder, bool useCache = true)
            where TDomain : Domain.Base.Entities.Person
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids", "Must provide at least one id to get record(s)");
            }
            var personEntities = new List<TDomain>();
            if (useCache)
            {
                List<string> notFoundRecordIds = new List<string>();

                ids = ids.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                foreach (var id in ids)
                {
                    string cacheKey = BuildFullCacheKey(id);
                    if (ContainsKey(cacheKey))
                    {
                        personEntities.Add((TDomain)_cacheProvider.Get(cacheKey));
                    }
                    else
                    {
                        notFoundRecordIds.Add(id);
                    }
                }
                if (notFoundRecordIds.Any())
                {
                    IEnumerable<TDomain> additionalRecords = await GetNonCachedPersonsAsync(notFoundRecordIds, objectBuilder);
                    personEntities.AddRange(additionalRecords);
                }
            }
            else
            {
                var records = await BuildRecordsAsync(ids, objectBuilder);
                foreach (var record in records)
                {

                    await AddOrUpdateCacheAsync<TDomain>(record.Id, record, PersonCacheTimeout);
                    personEntities.Add(record);

                }
            }
            return personEntities;
        }

        /// <summary>
        /// Method to read the requested records that are not in cache
        /// </summary>
        /// <param name="ids">ids of records to retrieve</param>
        /// <returns>List of specified objects</returns>
        private async Task<IEnumerable<TDomain>> GetNonCachedPersonsAsync<TDomain>(List<string> ids, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.Person
        {
            List<TDomain> nonCachedRecords = new List<TDomain>();
            if (ids != null && ids.Any())
            {
                nonCachedRecords = (List<TDomain>)await BuildRecordsAsync(ids, objectBuilder);
                if (nonCachedRecords != null)
                {
                    foreach (var record in nonCachedRecords)
                    {
                        await AddOrUpdateCacheAsync<TDomain>(record.Id, record, PersonCacheTimeout);
                    }
                }
            }
            return nonCachedRecords;
        }

        /// <summary>
        /// Private method to read and build multiple person-like objects of specified type
        /// </summary>
        /// <typeparam name="TDomain">object type</typeparam>
        /// <param name="ids">list of ids</param>
        /// <param name="objectBuilder">function to build a single object</param>
        /// <returns>List of objects of specified type</returns>
        private async Task<IEnumerable<TDomain>> BuildRecordsAsync<TDomain>(IEnumerable<string> ids, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.Person
        {
            var personsIds = ids.ToArray();

            var records = await DataReader.BulkReadRecordAsync<DataContracts.Person>(personsIds);
            if (records == null || !records.Any())
            {
                throw new ArgumentOutOfRangeException("ids", "No records were found for specified ids");
            }
            List<TDomain> persons = new List<TDomain>();
            foreach (var record in records)
            {
                try
                {
                    persons.Add(objectBuilder.Invoke(record));
                }
                catch (Exception e)
                {
                    LogDataError("Person", record.Recordkey, record, e);
                }

            }
            return await BuildPersonsAsync<TDomain>(personsIds, records, persons);
        }


        /// <summary>
        /// Used to get person entities by guid, without using the cache.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>The extended person entity, such as student</returns>
        public async Task<TDomain> GetByGuidNonCachedAsync<TDomain>(string guid, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.Person
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");

            DataContracts.Person record = await DataReader.ReadRecordAsync<DataContracts.Person>(new GuidLookup(guid));
            if (record == null)
                throw new ArgumentOutOfRangeException("guid", "No person record for guid " + guid);

            // Create the specified domain object
            TDomain person = objectBuilder.Invoke(record);
            var personBasedEntities = await BuildPersonsAsync<TDomain>(new List<string>() { record.Recordkey },
                new Collection<DataContracts.Person>() { record }, new Collection<TDomain>() { person });
            return personBasedEntities.FirstOrDefault();
        }

        /// <summary>
        /// Used to get person entities by guid, without using the cache for persons-credentials
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>The extended person entity, such as student</returns>
        public async Task<TDomain> GetCredentialByGuidNonCachedAsync<TDomain>(string guid, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.Person
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");

            DataContracts.Person record = await DataReader.ReadRecordAsync<DataContracts.Person>(new GuidLookup(guid));
            if (record == null)
                throw new ArgumentOutOfRangeException("guid", "No person record for guid " + guid);

            // Create the specified domain object
            TDomain person = objectBuilder.Invoke(record);
            var personBasedEntities = await BuildPersonCredentialsAsync<TDomain>(new List<string>() { record.Recordkey },
                new Collection<DataContracts.Person>() { record }, new Collection<TDomain>() { person });
            return personBasedEntities.FirstOrDefault();
        }

        /// <summary>
        /// Used to get person entities by guids, without using the cache.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>List of extended person entities, such as student</returns>
        public async Task<IEnumerable<TDomain>> GetByGuidNonCachedAsync<TDomain>(IEnumerable<string> guids, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.Person
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get person records");

            // get all the person records associated with the guids
            var personsGuidLookup = guids.ToList().ConvertAll(g => new GuidLookup(g)).ToArray();
            var personRecords = (await DataReader.BulkReadRecordAsync<DataContracts.Person>(personsGuidLookup)).ToList();

            if (personRecords == null)
                throw new ArgumentOutOfRangeException("guids", "No person records found for guids");

            // create the specified domain objects
            var personBasedObjects = new Collection<TDomain>();
            foreach (var personRecord in personRecords)
            {
                try
                {
                    personBasedObjects.Add(objectBuilder.Invoke(personRecord));
                }
                catch (Exception e)
                {
                    LogDataError("Person", personRecord.Recordkey, personRecord, e);
                }
            }

            // build the person entities
            return await BuildPersonsAsync<TDomain>(personRecords.Select(p => p.Recordkey).ToList(), personRecords, personBasedObjects);
        }

        #endregion

        #region Email Address Hierarchy
        /// <summary>
        /// Used to retrieve the email address from the GetHierarchyEmail transaction.
        /// </summary>
        /// <param name="personId">Person ID of the proxy user</param>
        /// <param name="emailHierarchy">Email address hierarchy used by proxy, specified on PREP</param>
        /// <returns>An email address for a proxy user based on the email address hierarchy from PREP</returns>
        public async Task<EmailAddress> GetEmailAddressFromHierarchyAsync(string personId, string emailHierarchy)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Must provide a person ID to get a record");
            }

            var hierarchyRequest = new TxGetHierarchyEmailRequest();
            hierarchyRequest.IoPersonId = personId;
            if (string.IsNullOrEmpty(emailHierarchy))
            {
                hierarchyRequest.InHierarchy = null;
            }
            else
            {
                hierarchyRequest.InHierarchy = emailHierarchy;
            }
            var hierarchyResponse = await transactionInvoker.ExecuteAsync<TxGetHierarchyEmailRequest, TxGetHierarchyEmailResponse>(hierarchyRequest);
            if (!string.IsNullOrEmpty(hierarchyResponse.OutEmailAddress))
            {
                var emailFromHierarchy = new EmailAddress(hierarchyResponse.OutEmailAddress, hierarchyResponse.OutEmailAddrType);
                return emailFromHierarchy;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Person Integration Get methods

        /// <summary>
        /// Get a person entity by guid, without caching.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        public async Task<Domain.Base.Entities.PersonIntegration> GetPersonIntegrationByGuidNonCachedAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");

            Domain.Base.Entities.PersonIntegration personEntity = await GetIntegrationByGuidNonCachedAsync<Domain.Base.Entities.PersonIntegration>(guid,
                 person =>
                 {
                     Domain.Base.Entities.PersonIntegration entity = new Domain.Base.Entities.PersonIntegration(person.Recordkey, person.LastName);
                     return entity;
                 });

            return personEntity;
        }

        /// <summary>
        /// Get a person entity by guid, with caching.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        public async Task<Domain.Base.Entities.PersonIntegration> GetPersonIntegrationByGuidAsync(string guid, bool bypassCache)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");
            try
            {
                Domain.Base.Entities.PersonIntegration personEntity = await GetIntegrationByGuidAsync<Domain.Base.Entities.PersonIntegration>(guid,
                                 person =>
                                 {
                                     Domain.Base.Entities.PersonIntegration entity = new Domain.Base.Entities.PersonIntegration(person.Recordkey, person.LastName);
                                     return entity;
                                 }, bypassCache);

                return personEntity;
            }
            catch (RepositoryException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Get a person entity by guid, with caching.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Person">person</see> entity</returns>
        public async Task<Domain.Base.Entities.PersonIntegration> GetPersonIntegration2ByGuidAsync(string guid, bool bypassCache)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");
            try
            {
                Domain.Base.Entities.PersonIntegration personEntity = await GetIntegration2ByGuidAsync<Domain.Base.Entities.PersonIntegration>(guid,
                                 person =>
                                 {
                                     Domain.Base.Entities.PersonIntegration entity = new Domain.Base.Entities.PersonIntegration(person.Recordkey, person.LastName);
                                     return entity;
                                 }, bypassCache);

                return personEntity;
            }
            catch (RepositoryException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Get a list of person entities by guid, without caching.
        /// </summary>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <returns>List of <see cref="Person">person</see> entities</returns>
        public async Task<IEnumerable<Domain.Base.Entities.PersonIntegration>> GetPersonIntegrationByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get person records.");

            var personEntities = await GetIntegrationByGuidNonCachedAsync<Domain.Base.Entities.PersonIntegration>(guids,
                 person =>
                 {
                     Domain.Base.Entities.PersonIntegration entity = new Domain.Base.Entities.PersonIntegration(person.Recordkey, person.LastName);
                     return entity;
                 });

            return personEntities;
        }

        /// <summary>
        /// Get a list of person entities by guid, without caching for persons-credentials
        /// </summary>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <returns>List of <see cref="Person">person</see> entities</returns>
        public async Task<IEnumerable<Domain.Base.Entities.PersonIntegration>> GetPersonCredentialsIntegrationByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get person records.");

            var personEntities = await GetCredentialsIntegrationByGuidNonCachedAsync<Domain.Base.Entities.PersonIntegration>(guids,
                person =>
                 {
                     Domain.Base.Entities.PersonIntegration entity = new Domain.Base.Entities.PersonIntegration(person.Recordkey, person.LastName);
                     return entity;
                 });

            return personEntities;
        }

        /// <summary>
        /// Get a list of person entities by guid, without caching.
        /// </summary>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <returns>List of <see cref="Person">person</see> entities</returns>
        public async Task<IEnumerable<Domain.Base.Entities.PersonIntegration>> GetPersonIntegration2ByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get person records.");

            var personEntities = await GetIntegration2ByGuidNonCachedAsync<Domain.Base.Entities.PersonIntegration>(guids,
                 person =>
                 {
                     Domain.Base.Entities.PersonIntegration entity = new Domain.Base.Entities.PersonIntegration(person.Recordkey, person.LastName);
                     return entity;
                 });

            return personEntities;
        }

        /// <summary>
        /// Used to get person entities by guid, without using the cache.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>The extended person entity, such as student</returns>
        private async Task<TDomain> GetIntegrationByGuidNonCachedAsync<TDomain>(string guid, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.PersonIntegration
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");

            DataContracts.Person record = await DataReader.ReadRecordAsync<DataContracts.Person>(new GuidLookup(guid));
            if (record == null)
                throw new ArgumentOutOfRangeException("guid", string.Format("No person record for guid '{0}'", guid));

            var foreignPerson = await DataReader.ReadRecordAsync<DataContracts.ForeignPerson>(record.Recordkey);
            var integrationPerson = await DataReader.ReadRecordAsync<DataContracts.PersonIntg>(record.Recordkey);
            var socialMediaHandles = await DataReader.BulkReadRecordAsync<DataContracts.SocialMediaHandles>("WITH SMH.PERSON.ID EQ '" + record.Recordkey + "'");
            var addressDataContracts = new List<Data.Base.DataContracts.Address>();
            if (record.PersonAddresses != null && record.PersonAddresses.Any())
            {
                addressDataContracts = (await GetPersonAddressContractsAsync(record.PersonAddresses)).ToList();
            }
            // Create the specified domain object
            TDomain person = objectBuilder.Invoke(record);
            var personBasedEntities = await BuildPersonsIntegrationAsync<TDomain>(new List<string>() { record.Recordkey },
                new Collection<DataContracts.Person>() { record },
                new Collection<DataContracts.ForeignPerson>() { foreignPerson },
                new Collection<DataContracts.PersonIntg>() { integrationPerson }, socialMediaHandles, new Collection<TDomain>() { person }, addressDataContracts);

            return personBasedEntities.FirstOrDefault();
        }

        /// <summary>
        /// Used to get person entities by guid, using cache.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>The extended person entity, such as student</returns>
        private async Task<TDomain> GetIntegrationByGuidAsync<TDomain>(string guid,
            Func<DataContracts.Person, TDomain> objectBuilder, bool bypassCache)
            where TDomain : Domain.Base.Entities.PersonIntegration
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");

            var cacheKey = BuildFullCacheKey("PersonIntegration" + guid);
            if ((!bypassCache) && (ContainsKey(cacheKey)))
            {
                return (TDomain)_cacheProvider.Get(cacheKey);

            }

            DataContracts.Person record =
                await DataReader.ReadRecordAsync<DataContracts.Person>(new GuidLookup(guid));
            if (record == null)
                throw new ArgumentOutOfRangeException("guid", "No person record for guid " + guid);

            var foreignPerson = await DataReader.ReadRecordAsync<DataContracts.ForeignPerson>(record.Recordkey);
            var integrationPerson = await DataReader.ReadRecordAsync<DataContracts.PersonIntg>(record.Recordkey);
            var socialMediaHandles =
                await
                    DataReader.BulkReadRecordAsync<DataContracts.SocialMediaHandles>("WITH SMH.PERSON.ID EQ '" +
                                                                                     record.Recordkey + "'");
            //get all the addresses once
            var addressDataContracts = new List<Data.Base.DataContracts.Address>();
            if (record.PersonAddresses != null && record.PersonAddresses.Any())
            {
                addressDataContracts = (await GetPersonAddressContractsAsync(record.PersonAddresses)).ToList();
            }
            // Create the specified domain object
            TDomain person = objectBuilder.Invoke(record);
            var personBasedEntities =
                await BuildPersonsIntegrationAsync<TDomain>(new List<string>() { record.Recordkey },
                    new Collection<DataContracts.Person>() { record },
                    new Collection<DataContracts.ForeignPerson>() { foreignPerson },
                    new Collection<DataContracts.PersonIntg>() { integrationPerson }, socialMediaHandles,
                    new Collection<TDomain>() { person }, addressDataContracts);
            var personRecord = personBasedEntities.FirstOrDefault();
            if (personRecord != null)
            {
                await
                    AddOrUpdateCacheAsync<TDomain>(("PersonIntegration" + personRecord.Guid), personRecord,
                        CacheTimeout);
            }
            return personRecord;
        }

        /// <summary>
        /// Used to get person entities by guid, using cache.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>The extended person entity, such as student</returns>
        private async Task<TDomain> GetIntegration2ByGuidAsync<TDomain>(string guid,
            Func<DataContracts.Person, TDomain> objectBuilder, bool bypassCache)
            where TDomain : Domain.Base.Entities.PersonIntegration
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a record.");

            var cacheKey = BuildFullCacheKey("PersonIntegration" + guid);
            if ((!bypassCache) && (ContainsKey(cacheKey)))
            {
                return (TDomain)_cacheProvider.Get(cacheKey);

            }

            DataContracts.Person record =
                await DataReader.ReadRecordAsync<DataContracts.Person>(new GuidLookup(guid));
            if (record == null)
                throw new ArgumentOutOfRangeException("guid", "No person record for guid " + guid);

            var foreignPerson = await DataReader.ReadRecordAsync<DataContracts.ForeignPerson>(record.Recordkey);
            var integrationPerson = await DataReader.ReadRecordAsync<DataContracts.PersonIntg>(record.Recordkey);
            var militaryPerson = await DataReader.ReadRecordAsync<DataContracts.PerMilitary>(record.Recordkey);
            var socialMediaHandles =
                await
                    DataReader.BulkReadRecordAsync<DataContracts.SocialMediaHandles>("WITH SMH.PERSON.ID EQ '" +
                                                                                     record.Recordkey + "'");
            var addressDataContracts = new List<Data.Base.DataContracts.Address>();
            if (record.PersonAddresses != null && record.PersonAddresses.Any())
            {
                addressDataContracts = (await GetPersonAddressContractsAsync(record.PersonAddresses)).ToList();
            }
            // Create the specified domain object
            TDomain person = objectBuilder.Invoke(record);
            var personBasedEntities =
                await BuildPersons2IntegrationAsync<TDomain>(new List<string>() { record.Recordkey },
                    new Collection<DataContracts.Person>() { record },
                    new Collection<DataContracts.ForeignPerson>() { foreignPerson },
                    new Collection<DataContracts.PersonIntg>() { integrationPerson }, socialMediaHandles,
                    militaryPerson == null ? null : new Collection<DataContracts.PerMilitary>() { militaryPerson },
                    new Collection<TDomain>() { person }, addressDataContracts);
            var personRecord = personBasedEntities.FirstOrDefault();
            if (personRecord != null)
            {
                await
                    AddOrUpdateCacheAsync<TDomain>(("PersonIntegration" + personRecord.Guid), personRecord,
                        CacheTimeout);
            }
            return personRecord;
        }

        /// <summary>
        /// Used to get person entities by guids, without using the cache.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>List of extended person entities, such as student</returns>
        private async Task<IEnumerable<TDomain>> GetIntegrationByGuidNonCachedAsync<TDomain>(IEnumerable<string> guids, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.PersonIntegration
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get person records");

            // get all the person records associated with the guids
            var personsGuidLookup = guids.ToList().ConvertAll(g => new GuidLookup(g)).ToArray();
            var personRecords = (await DataReader.BulkReadRecordAsync<DataContracts.Person>(personsGuidLookup)).ToList();

            if (personRecords == null)
                throw new ArgumentOutOfRangeException("guids", "No person records found for guids");

            var personIds = personRecords.Select(pr => pr.Recordkey).Distinct().ToArray();
            var foreignPersonRecords = await DataReader.BulkReadRecordAsync<DataContracts.ForeignPerson>(personIds);
            var integrationPersonRecords = await DataReader.BulkReadRecordAsync<DataContracts.PersonIntg>(personIds);
            var socialMediaHandlesRecords = new Collection<SocialMediaHandles>();

            var socialMediaHandlesIds = DataReader.Select("SOCIAL.MEDIA.HANDLES", "WITH SMH.PERSON.ID EQ '?'", personIds);
            if (socialMediaHandlesIds != null && socialMediaHandlesIds.Any())
            {
                socialMediaHandlesRecords = await DataReader.BulkReadRecordAsync<DataContracts.SocialMediaHandles>(socialMediaHandlesIds.ToArray());
            }

            //get all associated addresses
            var addressDataContracts = new List<Data.Base.DataContracts.Address>();
            var personAddresses = personRecords.SelectMany(pr => pr.PersonAddresses).Distinct().ToArray();
            if (personAddresses != null && personAddresses.Any())
            {
                addressDataContracts = (await GetPersonAddressContractsAsync(personAddresses)).ToList();
            }
            // create the specified domain objects
            var personBasedObjects = new Collection<TDomain>();
            var ex = new RepositoryException("Error retrieving person data");

            foreach (var personRecord in personRecords)
            {
                try
                {
                    personBasedObjects.Add(objectBuilder.Invoke(personRecord));
                }
                catch (Exception e)
                {
                    LogDataError("Person", personRecord.Recordkey, personRecord, e);
                    if (e.GetType() == typeof(ArgumentNullException) && e.Message.Contains("lastName"))
                    {
                        var msg = string.Format("Person ID '{0}' has no last name.", personRecord.Recordkey);
                        ex.AddError(new RepositoryError("lastName.null", msg));
                    }
                }
            }
            if (ex.Errors.Any())
            {
                throw ex;
            }

            // build the person entities
            return await BuildPersonsIntegrationAsync<TDomain>(personRecords.Select(p => p.Recordkey).ToList(), personRecords, foreignPersonRecords, integrationPersonRecords, socialMediaHandlesRecords, personBasedObjects, addressDataContracts);
        }

        /// <summary>
        /// Used to get person entities by guids, without using the cache for persons-credentials
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>List of extended person entities, such as student</returns>
        private async Task<IEnumerable<TDomain>> GetCredentialsIntegrationByGuidNonCachedAsync<TDomain>(IEnumerable<string> guids, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.PersonIntegration
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get person records");

            // get all the person records associated with the guids
            var personsGuidLookup = guids.ToList().ConvertAll(g => new GuidLookup(g)).ToArray();
            var personRecords = (await DataReader.BulkReadRecordAsync<DataContracts.Person>(personsGuidLookup)).ToList();

            if (personRecords == null)
                throw new ArgumentOutOfRangeException("guids", "No person records found for guids");

            var foreignPersonRecords = new List<ForeignPerson>();
            var integrationPersonRecords = new List<PersonIntg>();
            var socialMediaHandlesRecords = new Collection<SocialMediaHandles>();

            var addressDataContracts = new List<Data.Base.DataContracts.Address>();

            // create the specified domain objects
            var personBasedObjects = new Collection<TDomain>();
            var ex = new RepositoryException("Error retrieving person data");

            foreach (var personRecord in personRecords)
            {
                try
                {
                    personBasedObjects.Add(objectBuilder.Invoke(personRecord));
                }
                catch (Exception e)
                {
                    LogDataError("Person", personRecord.Recordkey, personRecord, e);
                    if (e.GetType() == typeof(ArgumentNullException) && e.Message.Contains("lastName"))
                    {
                        var msg = string.Format("Person ID '{0}' has no last name.", personRecord.Recordkey);
                        ex.AddError(new RepositoryError("lastName.null", msg));
                    }
                }
            }
            if (ex.Errors.Any())
            {
                throw ex;
            }

            // build the person entities
            return await BuildPersonsIntegrationAsync<TDomain>(personRecords.Select(p => p.Recordkey).ToList(), personRecords, foreignPersonRecords, integrationPersonRecords, socialMediaHandlesRecords, personBasedObjects, addressDataContracts);
        }

        /// <summary>
        /// Used to get person entities by guids, without using the cache.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="guids">Guids of the persons in Colleague.</param>
        /// <param name="objectBuilder"></param>
        /// <returns>List of extended person entities, such as student</returns>
        private async Task<IEnumerable<TDomain>> GetIntegration2ByGuidNonCachedAsync<TDomain>(IEnumerable<string> guids, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.PersonIntegration
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get person records");

            // get all the person records associated with the guids
            var personsGuidLookup = guids.ToList().ConvertAll(g => new GuidLookup(g)).ToArray();
            var personRecords = (await DataReader.BulkReadRecordAsync<DataContracts.Person>(personsGuidLookup)).ToList();

            if (personRecords == null)
                throw new ArgumentOutOfRangeException("guids", "No person records found for guids");

            var personIds = personRecords.Select(pr => pr.Recordkey).Distinct().ToArray();
            var foreignPersonRecords = await DataReader.BulkReadRecordAsync<DataContracts.ForeignPerson>(personIds);
            var integrationPersonRecords = await DataReader.BulkReadRecordAsync<DataContracts.PersonIntg>(personIds);
            var militaryPersonRecords = await DataReader.BulkReadRecordAsync<DataContracts.PerMilitary>(personIds);

            var socialMediaHandlesRecords = new Collection<SocialMediaHandles>();

            var socialMediaHandlesIds = DataReader.Select("SOCIAL.MEDIA.HANDLES", "WITH SMH.PERSON.ID EQ '?'", personIds);
            if (socialMediaHandlesIds != null && socialMediaHandlesIds.Any())
            {
                socialMediaHandlesRecords = await DataReader.BulkReadRecordAsync<DataContracts.SocialMediaHandles>(socialMediaHandlesIds.ToArray());
            }

            //get all associated addresses
            var addressDataContracts = new List<Data.Base.DataContracts.Address>();
            var personAddresses = personRecords.SelectMany(pr => pr.PersonAddresses).Distinct().ToArray();
            if (personAddresses != null && personAddresses.Any())
            {
                addressDataContracts = (await GetPersonAddressContractsAsync(personAddresses)).ToList();
            }

            // create the specified domain objects
            var personBasedObjects = new Collection<TDomain>();
            var ex = new RepositoryException("Error retrieving person data");

            foreach (var personRecord in personRecords)
            {
                try
                {
                    personBasedObjects.Add(objectBuilder.Invoke(personRecord));
                }
                catch (Exception e)
                {
                    LogDataError("Person", personRecord.Recordkey, personRecord, e);
                    if (e.GetType() == typeof(ArgumentNullException) && e.Message.Contains("lastName"))
                    {
                        var msg = string.Format("Person ID '{0}' has no last name.", personRecord.Recordkey);
                        ex.AddError(new RepositoryError("lastName.null", msg));
                    }
                }
            }
            if (ex.Errors.Any())
            {
                throw ex;
            }

            // build the person entities
            return await BuildPersons2IntegrationAsync<TDomain>(personRecords.Select(p => p.Recordkey).ToList(), personRecords, foreignPersonRecords, integrationPersonRecords, socialMediaHandlesRecords,
                militaryPersonRecords, personBasedObjects, addressDataContracts);
        }

        #endregion

        #region PERSON PIN Get Method

        /// <summary>
        /// Gets person pin entities
        /// </summary>
        /// <param name="personGuids"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonPin>> GetPersonPinsAsync(string[] personGuids)
        {
            List<Domain.Base.Entities.PersonPin> personPinEntities = new List<Domain.Base.Entities.PersonPin>();

            List<GuidLookup> guidLookUps = new List<GuidLookup>();

            personGuids.ToList().ForEach(guid => guidLookUps.Add(new GuidLookup(guid) { ModelName = "PERSON.PIN" }));

            var guidLookUpResults = await DataReader.SelectAsync(guidLookUps.ToArray());

            if (guidLookUpResults == null)
            {
                return null;
            }

            var distinctPersonIdArray = guidLookUpResults.Select(s => s.Value.PrimaryKey).Distinct().ToArray();
            if (distinctPersonIdArray.Count() != guidLookUpResults.Count())
            {
                logger.Error("Possible duplicate guids found in Person.");
            }
            var personPinDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.PersonPin>("PERSON.PIN", distinctPersonIdArray);
            if (personPinDataContracts != null)
            {
                foreach (var personPinDataContract in personPinDataContracts)
                {
                    personPinEntities.Add(new Domain.Base.Entities.PersonPin(personPinDataContract.Recordkey, personPinDataContract.PersonPinUserId));
                }
            }
            return personPinEntities.Any() ? personPinEntities : null;
        }

        #endregion

        #region Build Person Methods

        /// <summary>
        /// Builds Person objects 
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="personIds"></param>
        /// <param name="records"></param>
        /// <param name="personBasedObjects"></param>
        /// <returns></returns>
        private async Task<IEnumerable<TDomain>> BuildPersonsAsync<TDomain>(IEnumerable<string> personIds, IEnumerable<DataContracts.Person> records, IEnumerable<TDomain> personBasedObjects)
            where TDomain : Domain.Base.Entities.Person
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

                    // Build items not included in base person
                    List<string> preferredAddress = await GetPreferredAddressAsync(personBasedObject.Id);
                    if (preferredAddress != null)
                    {
                        personBasedObject.PreferredAddress = preferredAddress;
                    }

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
                logger.Info(errorMessage);
            }
            return personResults;
        }

        /// <summary>
        /// Builds Person objects for persons-credentials
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="personIds"></param>
        /// <param name="records"></param>
        /// <param name="personBasedObjects"></param>
        /// <returns></returns>
        private async Task<IEnumerable<TDomain>> BuildPersonCredentialsAsync<TDomain>(IEnumerable<string> personIds, IEnumerable<DataContracts.Person> records, IEnumerable<TDomain> personBasedObjects)
            where TDomain : Domain.Base.Entities.Person
        {
            var personResults = new List<TDomain>();
            List<string> personIdsNotFound = new List<string>();

            foreach (string personId in personIds)
            {
                DataContracts.Person record = records.Where(p => p.Recordkey == personId).FirstOrDefault();
                TDomain tDomainObject = personBasedObjects.Where(p => p.Id == personId).FirstOrDefault();
                if (record != null && tDomainObject != null)
                {
                    TDomain personBasedObject = await BuildBasePersonCredentialAsync<TDomain>(personId, record, tDomainObject);
                    //TDomain personBasedObject = BuildBasePersonCredentialAsync<TDomain>(personId, record, tDomainObject);

                    // Build items not included in base person
                    List<string> preferredAddress = await GetPreferredAddressAsync(personBasedObject.Id);
                    if (preferredAddress != null)
                    {
                        personBasedObject.PreferredAddress = preferredAddress;
                    }

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
                logger.Info(errorMessage);
            }
            return personResults;
        }

        /// <summary>
        /// Builds Person Integration objects 
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="personIds"></param>
        /// <param name="records"></param>
        /// <param name="personBasedObjects"></param>
        /// <returns></returns>
        private async Task<IEnumerable<TDomain>> BuildPersonsIntegrationAsync<TDomain>(IEnumerable<string> personIds, IEnumerable<DataContracts.Person> records, IEnumerable<DataContracts.ForeignPerson> foreignPersonRecords, IEnumerable<DataContracts.PersonIntg> integrationPersonRecords, IEnumerable<DataContracts.SocialMediaHandles> socialMediaHandlesRecords, IEnumerable<TDomain> personBasedObjects, IEnumerable<Data.Base.DataContracts.Address> allAddressDataContracts)
            where TDomain : Domain.Base.Entities.PersonIntegration
        {
            var personResults = new List<TDomain>();
            List<string> personIdsNotFound = new List<string>();
            string idPerson = string.Empty;

            try
            {
                foreach (string personId in personIds)
                {
                    idPerson = personId;
                    // Person Contract
                    DataContracts.Person personDataContract = records.Where(p => p.Recordkey == personId).FirstOrDefault();
                    // Foreign Person Contract
                    DataContracts.ForeignPerson foreignPerson = null;
                    try
                    {
                        foreignPerson = foreignPersonRecords.Where(fp => fp.Recordkey == personId).FirstOrDefault();
                    }
                    catch
                    {
                        // If Foreign Person is null, ignore
                    }
                    // Integration Person Contract
                    DataContracts.PersonIntg integrationPerson = null;
                    try
                    {
                        integrationPerson = integrationPersonRecords.Where(ip => ip.Recordkey == personId).FirstOrDefault();
                    }
                    catch
                    {
                        // If person integration does not exist, ignore
                    }
                    // Social Media Handles Contracts
                    List<DataContracts.SocialMediaHandles> socialMediaHandles = null;
                    try
                    {
                        socialMediaHandles = socialMediaHandlesRecords.Where(smh => smh.SmhPersonId == personId).ToList();
                    }
                    catch
                    {
                        // If social media handles do not exist, ignore
                    }
                    // Addresses Contracts
                    List<DataContracts.Address> addressDataContracts = null;
                    try
                    {
                        if (personDataContract.PersonAddresses != null && personDataContract.PersonAddresses.Any() && allAddressDataContracts != null && allAddressDataContracts.Any())
                        {
                            addressDataContracts = new List<DataContracts.Address>();
                            foreach (var addr in personDataContract.PersonAddresses)
                            {
                                var address = allAddressDataContracts.FirstOrDefault(adr => adr.Recordkey == addr);
                                if (address != null)
                                    addressDataContracts.Add(address);
                            }
                        }
                    }
                    catch
                    {
                        // If addresses do not exist, ignore
                    }
                    TDomain tDomainObject = personBasedObjects.Where(p => p.Id == personId).FirstOrDefault();
                    if (personDataContract != null && tDomainObject != null)
                    {
                        TDomain personBasedObject = await BuildBasePersonAsync<TDomain>(personId, personDataContract, tDomainObject);

                        // Build items not included in base person
                        // Preferred Address is no longer used in the PersonIntegration calls.
                        personBasedObject.PreferredAddress = new List<string>();
                        // Name History is in PersonProxyUser but not in base person.
                        List<PersonName> personNameHistory = new List<PersonName>();
                        if (personDataContract.NamehistEntityAssociation != null)
                        {
                            foreach (var names in personDataContract.NamehistEntityAssociation)
                            {
                                try
                                {
                                    if (names.NameHistoryLastNameAssocMember == null)
                                        names.NameHistoryLastNameAssocMember = "test";
                                    var nameHist = new PersonName(names.NameHistoryFirstNameAssocMember, names.NameHistoryMiddleNameAssocMember, names.NameHistoryLastNameAssocMember);
                                    personNameHistory.Add(nameHist);
                                }
                                catch (ArgumentNullException e)
                                {
                                    var ex = new RepositoryException();
                                    var errMsg = new RepositoryError("lastName.Missing", "The last name is missing when gathering names., Entity: 'PERSON’, Record ID: ‘" + personId + "’");
                                    ex.AddError(errMsg);
                                    throw ex;
                                }

                            }
                        }
                        if (personNameHistory != null)
                        {
                            personBasedObject.FormerNames = personNameHistory;
                        }
                        // deceased date
                        if (integrationPerson != null && !personBasedObject.DeceasedDate.HasValue && integrationPerson.PerIntgDeceasedDate.HasValue)
                        {
                            personBasedObject.DeceasedDate = integrationPerson.PerIntgDeceasedDate;
                        }
                        // religion
                        personBasedObject.Religion = personDataContract.Denomination;
                        // Privacy Status
                        personBasedObject.PrivacyStatus = !string.IsNullOrEmpty(personDataContract.PrivacyFlag) ? PrivacyStatusType.restricted : PrivacyStatusType.unrestricted;
                        personBasedObject.PrivacyStatusCode = personDataContract.PrivacyFlag;
                        // Citizenship
                        personBasedObject.Citizenship = personDataContract.Citizenship;
                        // Alien Status
                        personBasedObject.AlienStatus = (foreignPerson != null ? foreignPerson.FperAlienStatus : string.Empty);
                        // Birth Country
                        if (foreignPerson != null)
                        {
                            personBasedObject.BirthCountry = foreignPerson.FperBirthCountry;
                        }
                        else
                        {
                            personBasedObject.BirthCountry = (integrationPerson != null ? integrationPerson.PerIntgBirthCountry : string.Empty);
                        }
                        // Interests
                        personBasedObject.Interests = personDataContract.Interests;
                        // Professional Abreviations
                        personBasedObject.ProfessionalAbbreviations = personDataContract.ProfessionalSuffixes;
                        // Drivers License
                        if (!string.IsNullOrEmpty(personDataContract.DriverLicenseNo))
                        {
                            var license = new PersonDriverLicense(personDataContract.Recordkey, personDataContract.DriverLicenseNo) { IssuingState = personDataContract.DriverLicenseState };
                            personBasedObject.DriverLicense = license;
                        }
                        // Visa information
                        if (!string.IsNullOrEmpty(personDataContract.VisaType))
                        {
                            var visa = new PersonVisa(personDataContract.Recordkey, personDataContract.VisaType)
                            {
                                EntryDate = personDataContract.PersonCountryEntryDate,
                                ExpireDate = personDataContract.VisaExpDate,
                                IssueDate = personDataContract.VisaIssuedDate,
                                IssuingCountry = (foreignPerson != null ? foreignPerson.FperVisaIssueCountry : string.Empty),
                                Guid = (foreignPerson != null ? foreignPerson.RecordGuid : string.Empty),
                                PersonGuid = (foreignPerson != null ? personDataContract.RecordGuid : string.Empty),
                                RequestDate = (foreignPerson != null ? foreignPerson.FperVisaRequestDate : null),
                                VisaNumber = (foreignPerson != null ? foreignPerson.FperVisaNo : string.Empty)
                            };
                            personBasedObject.Visa = visa;
                        }
                        // Passport information
                        if (foreignPerson != null && !string.IsNullOrEmpty(foreignPerson.FperPassportNo))
                        {
                            var passport = new PersonPassport(personDataContract.Recordkey, foreignPerson.FperPassportNo)
                            {
                                PersonGuid = personDataContract.RecordGuid,
                                ExpireDate = foreignPerson.FperPassportExpireDate,
                                IssuingCountry = foreignPerson.FperPassportCountry
                            };
                            personBasedObject.Passport = passport;
                        }
                        // Drivers license
                        if (!string.IsNullOrEmpty(personDataContract.DriverLicenseNo))
                        {
                            personBasedObject.DriverLicense = new PersonDriverLicense(personDataContract.Recordkey, personDataContract.DriverLicenseNo, (integrationPerson != null ? integrationPerson.PerIntgDlExpireDate : null)) { IssuingState = personDataContract.DriverLicenseState };
                        }
                        // Other Identity Documents
                        if ((integrationPerson != null) && integrationPerson.PerIntgIdDocNo != null && integrationPerson.PerIntgIdDocNo.Any())
                        {
                            personBasedObject.IdentityDocuments = new List<PersonIdentityDocuments>();
                            foreach (var document in integrationPerson.PerIntgIdentityDocsEntityAssociation)
                            {
                                var personDocument = new PersonIdentityDocuments(personDataContract.Recordkey, document.PerIntgIdDocNoAssocMember, document.PerIntgIdDocExpDateAssocMember);
                                personDocument.PersonGuid = personDataContract.RecordGuid;
                                personDocument.Country = document.PerIntgIdDocCountryAssocMember;
                                personDocument.Region = document.PerIntgIdDocRegionAssocMember;
                                personBasedObject.IdentityDocuments.Add(personDocument);
                            }
                        }
                        // Languages
                        if ((integrationPerson != null) && (integrationPerson.PerIntgLanguagesEntityAssociation != null))
                        {
                            foreach (var languageAssociation in integrationPerson.PerIntgLanguagesEntityAssociation)
                            {
                                var language = languageAssociation.PerIntgLanguageAssocMember;
                                var preference = !string.IsNullOrEmpty(languageAssociation.PerIntgLanguagePrefAssocMember) ? (Domain.Base.Entities.LanguagePreference)Enum.Parse(typeof(Domain.Base.Entities.LanguagePreference), languageAssociation.PerIntgLanguagePrefAssocMember) : Domain.Base.Entities.LanguagePreference.Secondary;

                                personBasedObject.AddPersonLanguage(new PersonLanguage(personDataContract.Recordkey, language, preference));
                            }
                        }
                        // Phone numbers
                        List<Phone> phones = new List<Phone>();
                        if (integrationPerson != null && integrationPerson.PerIntgPhonesEntityAssociation != null && integrationPerson.PerIntgPhonesEntityAssociation.Any())
                        {
                            // create the phone entities
                            foreach (var phone in integrationPerson.PerIntgPhonesEntityAssociation)
                            {
                                if (!string.IsNullOrEmpty(phone.PerIntgPhoneNumberAssocMember))
                                {
                                    // Ignore phones if they are not also in the person record.  They may have been deleted from Colleague
                                    // but not the INTG table.

                                    bool found = personDataContract.PersonalPhoneNumber.Any(ppn => ppn.Contains(phone.PerIntgPhoneNumberAssocMember));

                                    if (found)
                                    {
                                        var phoneEntity = new Phone(phone.PerIntgPhoneNumberAssocMember, phone.PerIntgPhoneTypeAssocMember, phone.PerIntgPhoneExtensionAssocMember)
                                        {
                                            CountryCallingCode = phone.PerIntgCtryCallingCodeAssocMember,
                                            IsPreferred = (!string.IsNullOrEmpty(phone.PerIntgPhonePrefAssocMember) ? phone.PerIntgPhonePrefAssocMember.Equals("Y", StringComparison.OrdinalIgnoreCase) : false)
                                        };
                                        personBasedObject.AddPhone(phoneEntity);
                                    }
                                }
                            }
                        }
                        // Personal Phones for individuals
                        if (personDataContract.PersonCorpIndicator == null || !personDataContract.PersonCorpIndicator.Equals("y", StringComparison.OrdinalIgnoreCase))
                        {
                            if (personDataContract.PersonalPhoneNumber != null && personDataContract.PersonalPhoneNumber.Any())
                            {
                                for (int i = 0; i < personDataContract.PersonalPhoneNumber.Count; i++)
                                {
                                    try
                                    {
                                        var phoneNumber = personDataContract.PersonalPhoneNumber[i];
                                        var phoneType = personDataContract.PersonalPhoneType.Count > i
                                            ? personDataContract.PersonalPhoneType[i]
                                            : null;
                                        var phoneExtension = personDataContract.PersonalPhoneExtension.Count > i
                                            ? personDataContract.PersonalPhoneExtension[i]
                                            : string.Empty;

                                        if (!string.IsNullOrEmpty(phoneNumber))
                                            personBasedObject.AddPhone(new Phone(phoneNumber, phoneType, phoneExtension));
                                    }
                                    catch (Exception exception)
                                    {
                                        logger.Error(exception, string.Concat("Could not load phone numbers for person id", personDataContract.RecordGuid));
                                    }
                                }
                            }
                        }
                        // Addresses              
                        if (addressDataContracts != null && addressDataContracts.Any())
                        {
                            personBasedObject.Addresses = new List<Domain.Base.Entities.Address>();
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
                                        string[] startDate = assocEntity.AddrSeasonalStartAssocMember.Split(_SM);
                                        string[] endDate = assocEntity.AddrSeasonalEndAssocMember.Split(_SM);
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
                                                logger.Error(ex.ToString());
                                                logger.Info(error);
                                            }
                                        }
                                    }
                                }
                                addressEntity.IsPreferredAddress = (address.Recordkey == personDataContract.PreferredAddress);
                                addressEntity.IsPreferredResidence = (address.Recordkey == personDataContract.PreferredResidence);
                                addressEntity.Status = "Current";
                                personBasedObject.Addresses.Add(addressEntity);
                                // Address Phones for Corporations
                                if (personDataContract.PersonCorpIndicator != null && personDataContract.PersonCorpIndicator.Equals("y", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (address.AddressPhones != null && address.AddressPhones.Any())
                                    {
                                        for (int i = 0; i < address.AddressPhones.Count; i++)
                                        {
                                            try
                                            {
                                                var phoneNumber = address.AddressPhones[i];
                                                var phoneType = address.AddressPhoneType.Count > i
                                                    ? address.AddressPhoneType[i]
                                                    : null;
                                                var phoneExtension = address.AddressPhoneExtension.Count > i
                                                    ? address.AddressPhoneExtension[i]
                                                    : string.Empty;

                                                if (!string.IsNullOrEmpty(phoneNumber))
                                                    personBasedObject.AddPhone(new Phone(phoneNumber, phoneType, phoneExtension));
                                            }
                                            catch (Exception exception)
                                            {
                                                logger.Error(exception, string.Concat("Could not load phone numbers for person id", personDataContract.RecordGuid));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Social Media
                        if (socialMediaHandles != null)
                        {
                            foreach (var socialMedia in socialMediaHandles)
                            {
                                if (socialMedia != null && !string.IsNullOrEmpty(socialMedia.SmhHandle) && !string.IsNullOrEmpty(socialMedia.SmhNetwork))
                                {
                                    var preferred = false;
                                    if (!string.IsNullOrEmpty(socialMedia.SmhPreferred))
                                    {
                                        preferred = socialMedia.SmhPreferred.Equals("y", StringComparison.OrdinalIgnoreCase);
                                    }
                                    personBasedObject.AddSocialMedia(new SocialMedia(socialMedia.SmhNetwork, socialMedia.SmhHandle, preferred));
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(personDataContract.PersonWebsiteAddress))
                        {
                            personBasedObject.AddSocialMedia(new SocialMedia("website", personDataContract.PersonWebsiteAddress));
                        }
                        // Email Addresses
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

                                    personBasedObject.AddEmailAddress(emailToAdd);
                                }
                                catch (Exception exception)
                                {
                                    logger.Error(exception, string.Format("Could not load email address for person id {0} with GUID {1}", personDataContract.Recordkey, personDataContract.RecordGuid));
                                }
                            }
                        }
                        // Roles
                        if (integrationPerson != null && integrationPerson.PerIntgRolesEntityAssociation != null)
                        {
                            foreach (var role in integrationPerson.PerIntgRolesEntityAssociation)
                            {
                                var personRole = new PersonRole((PersonRoleType)Enum.Parse(typeof(PersonRoleType), role.PerIntgRoleAssocMember), role.PerIntgRoleStartDateAssocMember, role.PerIntgRoleEndDateAssocMember);
                                personBasedObject.AddRole(personRole);
                            }
                        }

                        //check if vendor role is assigned and if not check if VEN is associated and assign vendor role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.Vendor).Any())
                        {
                            if (personDataContract.WhereUsed != null && personDataContract.WhereUsed.Contains("VENDORS"))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Vendor, null, null));
                            }
                        }

                        //check if employee role is assigned and if not check if EMP is associated and assign employee role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.Employee).Any())
                        {
                            if (personDataContract.WhereUsed != null && (personDataContract.WhereUsed.Contains("EMPLOYES") || personDataContract.WhereUsed.Contains("HRPER")))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Employee, null, null));
                            }
                        }

                        //check if student role is assigned and if not check if STU is associated and assign student role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.Student).Any())
                        {
                            if (personDataContract.WhereUsed != null && personDataContract.WhereUsed.Contains("STUDENTS"))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Student, null, null));
                            }
                        }

                        //check if prospective student role is assigned and if not check if PRO is associated and assign prospective student role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.ProspectiveStudent).Any())
                        {
                            if (personDataContract.WhereUsed != null && personDataContract.WhereUsed.Contains("APPLICANTS"))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.ProspectiveStudent, null, null));
                            }
                        }

                        //check if instructor role is assigned and if not check if FAC is associated and assign instructor role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.Instructor).Any())
                        {
                            if (personDataContract.WhereUsed != null && personDataContract.WhereUsed.Contains("FACULTY"))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Instructor, null, null));
                                //since we know this is a faculty, we can check for advisor role now.
                                if (await IsAdvisorAsync(personId))
                                {
                                    personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Advisor, null, null));
                                }
                            }
                        }

                        // Single Ethnicity update
                        if (integrationPerson != null && !string.IsNullOrEmpty(integrationPerson.PerIntgEthnic))
                        {
                            personBasedObject.EthnicityCode = integrationPerson.PerIntgEthnic;
                            if (!personBasedObject.EthnicCodes.Contains(integrationPerson.PerIntgEthnic))
                            {
                                var allCodes = new List<string>();
                                allCodes.Add(integrationPerson.PerIntgEthnic);
                                foreach (var code in personDataContract.PerEthnics)
                                {
                                    allCodes.Add(code);
                                }
                                personBasedObject.EthnicCodes = allCodes;
                            }
                        }
                        // Person Integration Preferred Name Type
                        if (integrationPerson != null)
                        {
                            if (string.IsNullOrEmpty(integrationPerson.PerIntgPreferredNameType))
                            {
                                integrationPerson.PerIntgPreferredNameType = "LEGAL";
                            }
                            personBasedObject.PreferredNameType = integrationPerson.PerIntgPreferredNameType;
                        }
                        else
                        {
                            personBasedObject.PreferredNameType = "LEGAL";
                        }

                        personResults.Add(personBasedObject);
                    }
                    else
                    {
                        personIdsNotFound.Add(personId);
                    }
                }
            }
            catch (RepositoryException e)
            {
                Exception ex = new Exception(string.Concat(e.Message, " Entity: 'PERSON', Record ID: '", idPerson, "'"));
                logger.Error(string.Concat(e.Message, " Entity: 'PERSON', Record ID: '", idPerson, "'"));
                throw ex;
            }
            catch (Exception e)
            {
                Exception ex = new Exception(string.Concat(e.Message, " Entity: 'PERSON', Record ID: '", idPerson, "'"));
                logger.Error(string.Concat(e.Message, " Entity: 'PERSON', Record ID: '", idPerson, "'"));
                throw ex;
            }
            if (personIdsNotFound.Any())
            {
                // log any ids that were not found.
                var errorMessage = "The following person Ids were requested but not found: " + string.Join(",", personIdsNotFound.ToArray());
                logger.Info(errorMessage);
            }
            return personResults;
        }


        /// <summary>
        /// Builds Person Integration objects 
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="personIds"></param>
        /// <param name="records"></param>
        /// <param name="personBasedObjects"></param>
        /// <returns></returns>
        private async Task<IEnumerable<TDomain>> BuildPersons2IntegrationAsync<TDomain>(IEnumerable<string> personIds, IEnumerable<DataContracts.Person> records,
            IEnumerable<DataContracts.ForeignPerson> foreignPersonRecords, IEnumerable<DataContracts.PersonIntg> integrationPersonRecords,
            IEnumerable<DataContracts.SocialMediaHandles> socialMediaHandlesRecords, IEnumerable<PerMilitary> militaryPersonRecords, IEnumerable<TDomain> personBasedObjects, IEnumerable<Data.Base.DataContracts.Address> allAddressDataContracts)
            where TDomain : Domain.Base.Entities.PersonIntegration
        {
            var personResults = new List<TDomain>();
            List<string> personIdsNotFound = new List<string>();

            foreach (string personId in personIds)
            {
                try
                {
                    // Person Contract
                    DataContracts.Person personDataContract = records.Where(p => p.Recordkey == personId).FirstOrDefault();
                    // Foreign Person Contract
                    DataContracts.ForeignPerson foreignPerson = null;
                    try
                    {
                        foreignPerson = foreignPersonRecords.Where(fp => fp.Recordkey == personId).FirstOrDefault();
                    }
                    catch
                    {
                        // If Foreign Person is null, ignore
                    }
                    // Integration Person Contract
                    DataContracts.PersonIntg integrationPerson = null;
                    try
                    {
                        integrationPerson = integrationPersonRecords.Where(ip => ip.Recordkey == personId).FirstOrDefault();
                    }
                    catch
                    {
                        // If person integration does not exist, ignore
                    }
                    // Social Media Handles Contracts
                    List<DataContracts.SocialMediaHandles> socialMediaHandles = null;
                    try
                    {
                        socialMediaHandles = socialMediaHandlesRecords.Where(smh => smh.SmhPersonId == personId).ToList();
                    }
                    catch
                    {
                        // If social media handles do not exist, ignore
                    }
                    // Addresses Contracts
                    List<DataContracts.Address> addressDataContracts = null;
                    try
                    {
                        if (personDataContract.PersonAddresses != null && personDataContract.PersonAddresses.Any() && allAddressDataContracts != null && allAddressDataContracts.Any())
                        {
                            addressDataContracts = new List<DataContracts.Address>();
                            foreach (var addr in personDataContract.PersonAddresses)
                            {
                                var address = allAddressDataContracts.FirstOrDefault(adr => adr.Recordkey == addr);
                                if (address != null)
                                    addressDataContracts.Add(address);
                            }
                        }
                    }
                    catch
                    {
                        // If addresses do not exist, ignore
                    }

                    //military records
                    List<DataContracts.PerMilitary> militaryRecordsHandles = null;
                    if (militaryPersonRecords != null && militaryPersonRecords.Any())
                    {
                        militaryRecordsHandles = militaryPersonRecords.Where(dc => !string.IsNullOrEmpty(dc.Recordkey) && dc.Recordkey.Equals(personId, StringComparison.OrdinalIgnoreCase))
                                                                      .ToList();
                    }

                    TDomain tDomainObject = personBasedObjects.Where(p => p.Id == personId).FirstOrDefault();
                    if (personDataContract != null && tDomainObject != null)
                    {
                        TDomain personBasedObject = await BuildBasePersonAsync<TDomain>(personId, personDataContract, tDomainObject);

                        // Build items not included in base person
                        // Preferred Address is no longer used in the PersonIntegration calls.
                        personBasedObject.PreferredAddress = new List<string>();
                        // Name History is in PersonProxyUser but not in base person.
                        List<PersonName> personNameHistory = new List<PersonName>();
                        if (personDataContract.NamehistEntityAssociation != null)
                        {
                            foreach (var names in personDataContract.NamehistEntityAssociation)
                            {
                                try
                                {
                                    if (names.NameHistoryLastNameAssocMember == null)
                                        names.NameHistoryLastNameAssocMember = "test";
                                    var nameHist = new PersonName(names.NameHistoryFirstNameAssocMember, names.NameHistoryMiddleNameAssocMember, names.NameHistoryLastNameAssocMember);
                                    personNameHistory.Add(nameHist);
                                }
                                catch (ArgumentNullException e)
                                {
                                    var ex = new RepositoryException("The last name is missing when gathering person history names.");
                                    throw ex;
                                }
                            }
                        }
                        if (personNameHistory != null)
                        {
                            personBasedObject.FormerNames = personNameHistory;
                        }
                        // deceased date
                        if (integrationPerson != null && !personBasedObject.DeceasedDate.HasValue && integrationPerson.PerIntgDeceasedDate.HasValue)
                        {
                            personBasedObject.DeceasedDate = integrationPerson.PerIntgDeceasedDate;
                        }
                        // religion
                        personBasedObject.Religion = personDataContract.Denomination;
                        // Privacy Status
                        personBasedObject.PrivacyStatus = !string.IsNullOrEmpty(personDataContract.PrivacyFlag) ? PrivacyStatusType.restricted : PrivacyStatusType.unrestricted;
                        personBasedObject.PrivacyStatusCode = personDataContract.PrivacyFlag;
                        // Citizenship
                        personBasedObject.Citizenship = personDataContract.Citizenship;
                        // Alien Status
                        personBasedObject.AlienStatus = (foreignPerson != null ? foreignPerson.FperAlienStatus : string.Empty);
                        // Birth Country
                        if (foreignPerson != null)
                        {
                            personBasedObject.BirthCountry = foreignPerson.FperBirthCountry;
                        }
                        else
                        {
                            personBasedObject.BirthCountry = (integrationPerson != null ? integrationPerson.PerIntgBirthCountry : string.Empty);
                        }
                        // Interests
                        personBasedObject.Interests = personDataContract.Interests;
                        // Professional Abreviations
                        personBasedObject.ProfessionalAbbreviations = personDataContract.ProfessionalSuffixes;
                        // Drivers License
                        if (!string.IsNullOrEmpty(personDataContract.DriverLicenseNo))
                        {
                            var license = new PersonDriverLicense(personDataContract.Recordkey, personDataContract.DriverLicenseNo) { IssuingState = personDataContract.DriverLicenseState };
                            personBasedObject.DriverLicense = license;
                        }
                        // Visa information
                        if (!string.IsNullOrEmpty(personDataContract.VisaType))
                        {
                            var visa = new PersonVisa(personDataContract.Recordkey, personDataContract.VisaType)
                            {
                                EntryDate = personDataContract.PersonCountryEntryDate,
                                ExpireDate = personDataContract.VisaExpDate,
                                IssueDate = personDataContract.VisaIssuedDate,
                                IssuingCountry = (foreignPerson != null ? foreignPerson.FperVisaIssueCountry : string.Empty),
                                Guid = (foreignPerson != null ? foreignPerson.RecordGuid : string.Empty),
                                PersonGuid = (foreignPerson != null ? personDataContract.RecordGuid : string.Empty),
                                RequestDate = (foreignPerson != null ? foreignPerson.FperVisaRequestDate : null),
                                VisaNumber = (foreignPerson != null ? foreignPerson.FperVisaNo : string.Empty)
                            };
                            personBasedObject.Visa = visa;
                        }
                        // Passport information
                        if (foreignPerson != null && !string.IsNullOrEmpty(foreignPerson.FperPassportNo))
                        {
                            var passport = new PersonPassport(personDataContract.Recordkey, foreignPerson.FperPassportNo)
                            {
                                PersonGuid = personDataContract.RecordGuid,
                                ExpireDate = foreignPerson.FperPassportExpireDate,
                                IssuingCountry = foreignPerson.FperPassportCountry
                            };
                            personBasedObject.Passport = passport;
                        }
                        // Drivers license
                        if (!string.IsNullOrEmpty(personDataContract.DriverLicenseNo))
                        {
                            personBasedObject.DriverLicense = new PersonDriverLicense(personDataContract.Recordkey, personDataContract.DriverLicenseNo, (integrationPerson != null ? integrationPerson.PerIntgDlExpireDate : null)) { IssuingState = personDataContract.DriverLicenseState };
                        }
                        // Other Identity Documents
                        if ((integrationPerson != null) && integrationPerson.PerIntgIdDocNo != null && integrationPerson.PerIntgIdDocNo.Any())
                        {
                            personBasedObject.IdentityDocuments = new List<PersonIdentityDocuments>();
                            foreach (var document in integrationPerson.PerIntgIdentityDocsEntityAssociation)
                            {
                                var personDocument = new PersonIdentityDocuments(personDataContract.Recordkey, document.PerIntgIdDocNoAssocMember, document.PerIntgIdDocExpDateAssocMember);
                                personDocument.PersonGuid = personDataContract.RecordGuid;
                                personDocument.Country = document.PerIntgIdDocCountryAssocMember;
                                personDocument.Region = document.PerIntgIdDocRegionAssocMember;
                                personBasedObject.IdentityDocuments.Add(personDocument);
                            }
                        }
                        // Languages
                        if ((integrationPerson != null) && (integrationPerson.PerIntgLanguagesEntityAssociation != null))
                        {
                            foreach (var languageAssociation in integrationPerson.PerIntgLanguagesEntityAssociation)
                            {
                                var language = languageAssociation.PerIntgLanguageAssocMember;
                                var preference = !string.IsNullOrEmpty(languageAssociation.PerIntgLanguagePrefAssocMember) ? (Domain.Base.Entities.LanguagePreference)Enum.Parse(typeof(Domain.Base.Entities.LanguagePreference), languageAssociation.PerIntgLanguagePrefAssocMember) : Domain.Base.Entities.LanguagePreference.Secondary;

                                personBasedObject.AddPersonLanguage(new PersonLanguage(personDataContract.Recordkey, language, preference));
                            }
                        }
                        // Phone numbers
                        List<Phone> phones = new List<Phone>();
                        if (integrationPerson != null && integrationPerson.PerIntgPhonesEntityAssociation != null && integrationPerson.PerIntgPhonesEntityAssociation.Any())
                        {
                            // create the phone entities
                            foreach (var phone in integrationPerson.PerIntgPhonesEntityAssociation)
                            {
                                if (!string.IsNullOrEmpty(phone.PerIntgPhoneNumberAssocMember))
                                {
                                    // Ignore phones if they are not also in the person record.  They may have been deleted from Colleague
                                    // but not the INTG table.

                                    bool found = personDataContract.PersonalPhoneNumber.Any(ppn => ppn.Contains(phone.PerIntgPhoneNumberAssocMember));

                                    if (found)
                                    {
                                        var phoneEntity = new Phone(phone.PerIntgPhoneNumberAssocMember, phone.PerIntgPhoneTypeAssocMember, phone.PerIntgPhoneExtensionAssocMember)
                                        {
                                            CountryCallingCode = phone.PerIntgCtryCallingCodeAssocMember,
                                            IsPreferred = (!string.IsNullOrEmpty(phone.PerIntgPhonePrefAssocMember) ? phone.PerIntgPhonePrefAssocMember.Equals("Y", StringComparison.OrdinalIgnoreCase) : false)
                                        };
                                        personBasedObject.AddPhone(phoneEntity);
                                    }
                                }
                            }
                        }
                        // Personal Phones for individuals
                        if (personDataContract.PersonCorpIndicator == null || !personDataContract.PersonCorpIndicator.Equals("y", StringComparison.OrdinalIgnoreCase))
                        {
                            if (personDataContract.PersonalPhoneNumber != null && personDataContract.PersonalPhoneNumber.Any())
                            {
                                for (int i = 0; i < personDataContract.PersonalPhoneNumber.Count; i++)
                                {
                                    try
                                    {
                                        var phoneNumber = personDataContract.PersonalPhoneNumber[i];
                                        var phoneType = personDataContract.PersonalPhoneType.Count > i
                                            ? personDataContract.PersonalPhoneType[i]
                                            : null;
                                        var phoneExtension = personDataContract.PersonalPhoneExtension.Count > i
                                            ? personDataContract.PersonalPhoneExtension[i]
                                            : string.Empty;

                                        if (!string.IsNullOrEmpty(phoneNumber))
                                            personBasedObject.AddPhone(new Phone(phoneNumber, phoneType, phoneExtension));
                                    }
                                    catch (Exception exception)
                                    {
                                        logger.Error(exception, string.Concat("Could not load phone numbers for person id", personDataContract.RecordGuid));
                                    }
                                }
                            }
                        }
                        // Addresses
                        if (addressDataContracts != null && addressDataContracts.Any())
                        {
                            // create the address entities
                            personBasedObject.Addresses = new List<Domain.Base.Entities.Address>();
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
                                        string[] startDate = assocEntity.AddrSeasonalStartAssocMember.Split(_SM);
                                        string[] endDate = assocEntity.AddrSeasonalEndAssocMember.Split(_SM);
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
                                                logger.Error(ex.ToString());
                                                logger.Info(error);
                                            }
                                        }
                                    }
                                }
                                addressEntity.IsPreferredAddress = (address.Recordkey == personDataContract.PreferredAddress);
                                addressEntity.IsPreferredResidence = (address.Recordkey == personDataContract.PreferredResidence);
                                addressEntity.Status = "Current";
                                personBasedObject.Addresses.Add(addressEntity);
                                // Address Phones for Corporations
                                if (personDataContract.PersonCorpIndicator != null && personDataContract.PersonCorpIndicator.Equals("y", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (address.AddressPhones != null && address.AddressPhones.Any())
                                    {
                                        for (int i = 0; i < address.AddressPhones.Count; i++)
                                        {
                                            try
                                            {
                                                var phoneNumber = address.AddressPhones[i];
                                                var phoneType = address.AddressPhoneType.Count > i
                                                    ? address.AddressPhoneType[i]
                                                    : null;
                                                var phoneExtension = address.AddressPhoneExtension.Count > i
                                                    ? address.AddressPhoneExtension[i]
                                                    : string.Empty;

                                                if (!string.IsNullOrEmpty(phoneNumber))
                                                    personBasedObject.AddPhone(new Phone(phoneNumber, phoneType, phoneExtension));
                                            }
                                            catch (Exception exception)
                                            {
                                                logger.Error(exception, string.Concat("Could not load phone numbers for person id", personDataContract.RecordGuid));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Social Media
                        if (socialMediaHandles != null)
                        {
                            foreach (var socialMedia in socialMediaHandles)
                            {
                                if (socialMedia != null && !string.IsNullOrEmpty(socialMedia.SmhHandle) && !string.IsNullOrEmpty(socialMedia.SmhNetwork))
                                {
                                    var preferred = false;
                                    if (!string.IsNullOrEmpty(socialMedia.SmhPreferred))
                                    {
                                        preferred = socialMedia.SmhPreferred.Equals("y", StringComparison.OrdinalIgnoreCase);
                                    }
                                    personBasedObject.AddSocialMedia(new SocialMedia(socialMedia.SmhNetwork, socialMedia.SmhHandle, preferred));
                                }
                            }
                        }
                        //military record
                        if (militaryRecordsHandles != null && militaryRecordsHandles.Any())
                        {
                            var militaryRecordHandle = militaryRecordsHandles.SelectMany(dc => dc.PermilStatusesEntityAssociation)
                                                                             .FirstOrDefault(rec => rec.PermilPrimaryFlagAssocMember.Equals("Y", StringComparison.OrdinalIgnoreCase));
                            if (militaryRecordHandle != null)
                            {
                                personBasedObject.MilitaryStatus = !string.IsNullOrEmpty(militaryRecordHandle.PermilMilStatusAssocMember) ? militaryRecordHandle.PermilMilStatusAssocMember : null;
                            }
                        }

                        if (!string.IsNullOrEmpty(personDataContract.PersonWebsiteAddress))
                        {
                            personBasedObject.AddSocialMedia(new SocialMedia("website", personDataContract.PersonWebsiteAddress));
                        }
                        // Email Addresses
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

                                    personBasedObject.AddEmailAddress(emailToAdd);
                                }
                                catch (Exception exception)
                                {
                                    logger.Error(exception, string.Format("Could not load email address for person id {0} with GUID {1}", personDataContract.Recordkey, personDataContract.RecordGuid));
                                }
                            }
                        }
                        // Roles
                        if (integrationPerson != null && integrationPerson.PerIntgRolesEntityAssociation != null)
                        {
                            foreach (var role in integrationPerson.PerIntgRolesEntityAssociation)
                            {
                                var personRole = new PersonRole((PersonRoleType)Enum.Parse(typeof(PersonRoleType), role.PerIntgRoleAssocMember), role.PerIntgRoleStartDateAssocMember, role.PerIntgRoleEndDateAssocMember);
                                personBasedObject.AddRole(personRole);
                            }
                        }

                        //check if vendor role is assigned and if not check if VEN is associated and assign vendor role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.Vendor).Any())
                        {
                            if (personDataContract.WhereUsed != null && personDataContract.WhereUsed.Contains("VENDORS"))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Vendor, null, null));
                            }
                        }

                        //check if employee role is assigned and if not check if EMP is associated and assign employee role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.Employee).Any())
                        {
                            if (personDataContract.WhereUsed != null && (personDataContract.WhereUsed.Contains("EMPLOYES") || personDataContract.WhereUsed.Contains("HRPER")))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Employee, null, null));
                            }
                        }

                        //check if student role is assigned and if not check if STU is associated and assign student role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.Student).Any())
                        {
                            if (personDataContract.WhereUsed != null && personDataContract.WhereUsed.Contains("STUDENTS"))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Student, null, null));
                            }
                        }

                        //check if prospective student role is assigned and if not check if PRO is associated and assign prospective student role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.ProspectiveStudent).Any())
                        {
                            if (personDataContract.WhereUsed != null && personDataContract.WhereUsed.Contains("APPLICANTS"))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.ProspectiveStudent, null, null));
                            }
                        }

                        //check if instructor role is assigned and if not check if FAC is associated and assign instructor role
                        if (personBasedObject.Roles == null || !personBasedObject.Roles.Any() ||
                            !personBasedObject.Roles.Where(r => r.RoleType == PersonRoleType.Instructor).Any())
                        {
                            if (personDataContract.WhereUsed != null && personDataContract.WhereUsed.Contains("FACULTY"))
                            {
                                personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Instructor, null, null));
                                //since we know this is a faculty, we can check for advisor role now.
                                if (await IsAdvisorAsync(personId))
                                {
                                    personBasedObject.Roles.Add(new PersonRole(PersonRoleType.Advisor, null, null));
                                }
                            }
                        }

                        // Single Ethnicity update
                        if (integrationPerson != null && !string.IsNullOrEmpty(integrationPerson.PerIntgEthnic))
                        {
                            personBasedObject.EthnicityCode = integrationPerson.PerIntgEthnic;
                            if (!personBasedObject.EthnicCodes.Contains(integrationPerson.PerIntgEthnic))
                            {
                                var allCodes = new List<string>();
                                allCodes.Add(integrationPerson.PerIntgEthnic);
                                foreach (var code in personDataContract.PerEthnics)
                                {
                                    allCodes.Add(code);
                                }
                                personBasedObject.EthnicCodes = allCodes;
                            }
                        }
                        // Person Integration Preferred Name Type
                        if (integrationPerson != null)
                        {
                            if (string.IsNullOrEmpty(integrationPerson.PerIntgPreferredNameType))
                            {
                                integrationPerson.PerIntgPreferredNameType = "LEGAL";
                            }
                            personBasedObject.PreferredNameType = integrationPerson.PerIntgPreferredNameType;
                        }
                        else
                        {
                            personBasedObject.PreferredNameType = "LEGAL";
                        }

                        personResults.Add(personBasedObject);
                    }
                    else
                    {
                        personIdsNotFound.Add(personId);
                    }
                }
                catch (RepositoryException e)
                {
                    Exception ex = new Exception(string.Concat(e.Message, " Entity: 'PERSON', Record ID: '", personId, "'"));
                    logger.Error(string.Concat(e.Message, " Entity: 'PERSON', Record ID: '", personId, "'"));
                    throw ex;
                }
                catch (Exception e)
                {
                    Exception ex = new Exception(string.Concat(e.Message, " Entity: 'PERSON', Record ID: '", personId, "'"));
                    logger.Error(string.Concat(e.Message, " Entity: 'PERSON', Record ID: '", personId, "'"));
                    throw ex;
                }
            }

            if (personIdsNotFound.Any())
            {
                // log any ids that were not found.
                var errorMessage = "The following person Ids were requested but not found: " + string.Join(",", personIdsNotFound.ToArray());
                logger.Info(errorMessage);
            }
            return personResults;
        }


        #endregion

        #region Person Create and Update Methods

        /// <summary>
        /// Get the person ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        public async Task<string> GetPersonIdFromGuidAsync(string guid)
        {
            //return await GetRecordKeyFromGuidAsync(guid);
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Person GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Person GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "PERSON")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, PERSON", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Create a person in the database.
        /// </summary>
        /// <param name="person">The <see cref="Person">person</see> to create in the database.</param>
        /// <param name="addresses">List of <see cref="Address">addresses</see> to associate to the person in the database.</param>
        /// <param name="phones">List of <see cref="Phone">phones</see> to associate to the person in the database.</param>
        /// <returns>The newly created <see cref="PersonIntegration">person</see> entity</returns>
        public async Task<PersonIntegration> Create2Async(Domain.Base.Entities.PersonIntegration person, IEnumerable<Domain.Base.Entities.Address> addresses, IEnumerable<Phone> phones, int version = 1)
        {
            if (person == null)
                throw new ArgumentNullException("person", "Must provide a person to create.");

            var extendedDataTuple = GetEthosExtendedDataLists();

            var createRequest = await BuildPersonIntegrationUpdateRequest(person, addresses, phones);
            //send the version to the CTX
            createRequest.Version = version;
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // write the person data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdatePersonIntegrationRequest, UpdatePersonIntegrationResponse>(createRequest);

            if (createResponse.PersonIntgErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating person '{0}':", person.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.PersonIntgErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created person from the database
            return await GetPersonIntegration2ByGuidAsync(createResponse.PersonGuid, true);
        }

        /// <summary>
        /// Create a organization.
        /// </summary>
        /// <param name="personOrg">The <see cref="Dtos.Organization2">person</see> to create in the database.</param>
        /// <param name="addresses">List of <see cref="Address">addresses</see> to associate to the organization in the database.</param>
        /// <param name="phones">List of <see cref="Phone">phones</see> to associate to the organization in the database.</param>
        /// <returns>The newly created <see cref="PersonIntegration">organization</see> entity</returns>
        public async Task<PersonIntegration> CreateOrganizationAsync(PersonIntegration personOrg, IEnumerable<Domain.Base.Entities.Address> addresses, IEnumerable<Phone> phones)
        {
            if (personOrg == null)
                throw new ArgumentNullException("person", "Must provide a person to create.");

            var extendedDataTuple = GetEthosExtendedDataLists();

            var createRequest = await BuildOrganizationIntegrationUpdateRequest(personOrg, addresses, phones);


            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // write the person data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdateOrganizationIntegrationRequest, UpdateOrganizationIntegrationResponse>(createRequest);

            if (createResponse.OrgIntgErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating organization '{0}':", personOrg.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.OrgIntgErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created person from the database
            return await GetPersonIntegrationByGuidNonCachedAsync(createResponse.OrgGuid);
        }

        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="person">The person entity to update in the database.</param>
        /// <param name="addresses">List of <see cref="Address">addresses</see> to associate to the person in the database.</param>
        /// <param name="phones">List of <see cref="Phone">phones</see> to associate to the person in the database.</param>
        /// <returns>The newly updated <see cref="PersonIntegration">person</see> entity</returns>
        public async Task<PersonIntegration> Update2Async(Domain.Base.Entities.PersonIntegration person, IEnumerable<Domain.Base.Entities.Address> addresses, IEnumerable<Phone> phones, int version = 1)
        {
            if (person == null)
                throw new ArgumentNullException("person", "Must provide a person to update.");
            if (string.IsNullOrEmpty(person.Guid))
                throw new ArgumentNullException("person", "Must provide the guid of the person to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var personId = await GetPersonIdFromGuidAsync(person.Guid);
            if (!string.IsNullOrEmpty(personId))
            {
                // set the person ID to make it available to the update request
                if (string.IsNullOrEmpty(person.Id)) person.Id = personId;

                var extendedDataTuple = GetEthosExtendedDataLists();

                var updateRequest = await BuildPersonIntegrationUpdateRequest(person, addresses, phones);
                //send version to the CTX
                updateRequest.Version = version;
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the person data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdatePersonIntegrationRequest, UpdatePersonIntegrationResponse>(updateRequest);

                if (updateResponse.PersonIntgErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating person '{0}':", person.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.PersonIntgErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated person from the database
                return await GetPersonIntegration2ByGuidAsync(updateResponse.PersonGuid, true);
            }
            else
            {
                // perform a create instead
                return await Create2Async(person, addresses, phones);
            }
        }

        /// <summary>
        /// Update a organization.
        /// </summary>
        /// <param name="personOrg">The  organization entity to update in the database.</param>
        /// <param name="addresses">List of <see cref="Address">addresses</see> to associate to the person in the database.</param>
        /// <param name="phones">List of <see cref="Phone">phones</see> to associate to the person in the database.</param>
        /// <returns>The newly updated <see cref="PersonIntegration">organization</see> entity</returns>
        public async Task<PersonIntegration> UpdateOrganizationAsync(Domain.Base.Entities.PersonIntegration personOrg, IEnumerable<Domain.Base.Entities.Address> addresses, IEnumerable<Phone> phones)
        {
            if (personOrg == null)
                throw new ArgumentNullException("person", "Must provide a person to update.");
            if (string.IsNullOrEmpty(personOrg.Guid))
                throw new ArgumentNullException("person", "Must provide the guid of the person to update.");

            var extendedDataTuple = GetEthosExtendedDataLists();

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var personId = await GetPersonIdFromGuidAsync(personOrg.Guid);
            if (!string.IsNullOrEmpty(personId))
            {
                // set the person ID to make it available to the update request
                if (string.IsNullOrEmpty(personOrg.Id)) personOrg.Id = personId;

                var updateRequest = await BuildOrganizationIntegrationUpdateRequest(personOrg, addresses, phones);

                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the person data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateOrganizationIntegrationRequest, UpdateOrganizationIntegrationResponse>(updateRequest);

                if (updateResponse.OrgIntgErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating organization '{0}':", personOrg.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.OrgIntgErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated person from the database
                return await GetPersonIntegrationByGuidNonCachedAsync(updateResponse.OrgGuid);
            }
            else
            {
                // perform a create instead
                return await CreateOrganizationAsync(personOrg, addresses, phones);
            }
        }

        private async Task<UpdateOrganizationIntegrationRequest> BuildOrganizationIntegrationUpdateRequest(Domain.Base.Entities.PersonIntegration person, IEnumerable<Domain.Base.Entities.Address> addresses, IEnumerable<Phone> phones)
        {
            var request = new UpdateOrganizationIntegrationRequest
            {
                OrgId = person.Id,
                OrgGuid = person.Guid,
                OrgTitle = person.PreferredName
            };

            //roles
            if (person.Roles.Any())
            {
                foreach (var role in person.Roles)
                {
                    OrgIntgRoles orgIntgRoles = new OrgIntgRoles();
                    orgIntgRoles.OrgRoles = role.RoleType.ToString();
                    orgIntgRoles.OrgRoleStartDate = role.StartDate.HasValue ? role.StartDate : default(DateTime?);
                    orgIntgRoles.OrgRoleEndDate = role.EndDate.HasValue ? role.EndDate : default(DateTime?);
                    request.OrgIntgRoles.Add(orgIntgRoles);
                }
            }

            //credentials
            if (person.PersonAltIds != null && person.PersonAltIds.Any())
            {
                foreach (var doc in person.PersonAltIds)
                {
                    OrgIntgAlternate identityDoc = new OrgIntgAlternate();
                    identityDoc.OrgAlternateIds = doc.Id;
                    identityDoc.OrgAlternateIdTypes = doc.Type;
                    request.OrgIntgAlternate.Add(identityDoc);
                }
            }

            // emails
            if (person.EmailAddresses != null && person.EmailAddresses.Any())
            {
                foreach (var emailAddress in person.EmailAddresses)
                {
                    request.OrgIntgEmailAddresses.Add(new OrgIntgEmailAddresses()
                    {
                        OrgEmailAddresses = emailAddress.Value,
                        OrgEmailAddressTypes = emailAddress.TypeCode,
                        OrgEmailPref = emailAddress.IsPreferred ? "Y" : string.Empty
                    });
                }
            }

            // addresses
            if (addresses != null && addresses.Any())
            {
                foreach (var address in addresses)
                {
                    var addressRequest = new OrgIntgAddresses();
                    addressRequest.OrgAddrIds = !string.IsNullOrEmpty(address.Guid) ? await GetAddressIdFromGuidAsync(address.Guid) : null;
                    addressRequest.OrgAddrTypes = address.TypeCode;
                    addressRequest.OrgAddrCities = !string.IsNullOrEmpty(address.City) ? address.City : address.IntlLocality;
                    addressRequest.OrgAddrCountries = !string.IsNullOrEmpty(address.CountryCode) ? address.CountryCode : address.Country;
                    addressRequest.OrgAddrSubregions = !string.IsNullOrEmpty(address.County) ? address.County : address.IntlSubRegion;
                    addressRequest.OrgAddrCodes = !string.IsNullOrEmpty(address.PostalCode) ? address.PostalCode : address.IntlPostalCode;
                    addressRequest.OrgAddrRegions = !string.IsNullOrEmpty(address.State) ? address.State : address.IntlRegion;
                    addressRequest.OrgAddrLongitude = address.Longitude.ToString();
                    addressRequest.OrgAddrLatitude = address.Latitude.ToString();
                    addressRequest.OrgAddrDeliveryPoint = address.DeliveryPoint;
                    addressRequest.OrgAddrCorrectionDigit = address.CorrectionDigit;
                    if (address.AddressChapter != null)
                    {
                        var newChapters = new StringBuilder();
                        foreach (var chapters in address.AddressChapter)
                        {
                            if (newChapters.Length > 0) newChapters.Append(_SM);
                            newChapters.Append(chapters);
                        }
                        addressRequest.OrgAddrChapters = newChapters.ToString();
                    }
                    addressRequest.OrgAddrPreference = address.IsPreferredResidence == true ? "Y" : null;
                    if (address.EffectiveStartDate.HasValue)
                    {
                        addressRequest.OrgAddrStartDate = address.EffectiveStartDate;
                    }
                    addressRequest.OrgAddrCarrierRoute = address.CarrierRoute;
                    if (address.EffectiveEndDate.HasValue)
                    {
                        addressRequest.OrgAddrEndDate = address.EffectiveEndDate;
                    }

                    if (address.AddressLines != null && address.AddressLines.Any())
                    {
                        var newAddressLines = new StringBuilder();
                        foreach (var addressLine in address.AddressLines)
                        {
                            if (newAddressLines.Length > 0) newAddressLines.Append(_SM);
                            newAddressLines.Append(addressLine);
                        }
                        addressRequest.OrgAddrLines = newAddressLines.ToString();
                    }
                    request.OrgIntgAddresses.Add(addressRequest);
                }
            }
            // Phone numbers
            if (phones != null && phones.Any())
            {
                foreach (var phone in phones)
                {
                    var phoneRequest = new OrgIntgPhones();
                    phoneRequest.OrgPhoneTypes = phone.TypeCode;
                    phoneRequest.OrgPhoneNumbers = phone.Number;
                    phoneRequest.OrgPhoneExtensions = phone.Extension;
                    phoneRequest.OrgPhoneCallingCode = phone.CountryCallingCode;
                    phoneRequest.OrgPhonePref = phone.IsPreferred ? "Y" : string.Empty;
                    request.OrgIntgPhones.Add(phoneRequest);
                }
            }
            // Social media
            if (person.SocialMedia != null && person.SocialMedia.Any())
            {
                foreach (var socialMedia in person.SocialMedia)
                {
                    var socialMediaRequest = new OrgIntgSocialMedia();
                    socialMediaRequest.OrgSocialMediaTypes = socialMedia.TypeCode;
                    socialMediaRequest.OrgSocialMediaHandles = socialMedia.Handle;
                    socialMediaRequest.OrgSocialMediaPref = socialMedia.IsPreferred ? "Y" : null;
                    request.OrgIntgSocialMedia.Add(socialMediaRequest);
                }
            }
            return request;
        }

        private async Task<UpdatePersonIntegrationRequest> BuildPersonIntegrationUpdateRequest(Domain.Base.Entities.PersonIntegration person, IEnumerable<Domain.Base.Entities.Address> addresses, IEnumerable<Phone> phones)
        {
            var personNameType = new List<string>();
            var request = new UpdatePersonIntegrationRequest
            {
                PersonId = person.Id,
                PersonGuid = person.Guid
            };

            var personIntgNames = new List<PersonIntgNames>();
            var personIntgNameLegal = new PersonIntgNames
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                MiddleName = person.MiddleName,
                Prefix = person.Prefix,
                Suffix = person.Suffix,
                PersonNameType = "LEGAL",
                PersonFullName = PersonNameService.FormatName(person.Prefix, person.FirstName, person.MiddleName, person.LastName, person.Suffix)
            };
            if (person.PreferredNameType == "LEGAL")
            {
                personIntgNameLegal.PersonNamePreferred = "preferred";
                if (!string.IsNullOrEmpty(person.PreferredName)) personIntgNameLegal.PersonFullName = person.PreferredName;
            }
            personIntgNames.Add(personIntgNameLegal);
            personNameType.Add("LEGAL");

            if (!string.IsNullOrEmpty(person.ChosenLastName) || !string.IsNullOrEmpty(person.ChosenFirstName) || !string.IsNullOrEmpty(person.ChosenMiddleName))
            {
                var personIntgNameChosen = new PersonIntgNames
                {
                    FirstName = person.ChosenFirstName,
                    LastName = person.ChosenLastName,
                    MiddleName = person.ChosenMiddleName,
                    PersonNameType = "CHOSEN",
                    PersonFullName = PersonNameService.FormatName("", person.ChosenFirstName, person.ChosenMiddleName, person.ChosenLastName, "")
                };
                if (person.PreferredNameType == "CHOSEN")
                {
                    personIntgNameChosen.PersonNamePreferred = "preferred";
                    if (!string.IsNullOrEmpty(person.PreferredName)) personIntgNameChosen.PersonFullName = person.PreferredName;
                }
                personIntgNames.Add(personIntgNameChosen);
                personNameType.Add("CHOSEN");
            }

            if (!string.IsNullOrEmpty(person.BirthNameLast))
            {
                var personIntgNameBirth = new PersonIntgNames
                {
                    FirstName = person.BirthNameFirst,
                    LastName = person.BirthNameLast,
                    MiddleName = person.BirthNameMiddle,
                    PersonNameType = "BIRTH",
                    PersonFullName = PersonNameService.FormatName("", person.BirthNameFirst, person.BirthNameMiddle, person.BirthNameLast, "")
                };
                if (person.PreferredNameType == "BIRTH")
                {
                    personIntgNameBirth.PersonNamePreferred = "preferred";
                    if (!string.IsNullOrEmpty(person.PreferredName)) personIntgNameBirth.PersonFullName = person.PreferredName;
                }
                personIntgNames.Add(personIntgNameBirth);
                personNameType.Add("BIRTH");
            }

            if (!string.IsNullOrEmpty(person.Nickname))
            {
                var personIntgNameNickname = new PersonIntgNames
                {
                    PersonFullName = person.Nickname,
                    PersonNameType = "NICKNAME"
                };
                if (person.PreferredNameType == "NICKNAME")
                {
                    personIntgNameNickname.PersonNamePreferred = "preferred";
                }
                personIntgNames.Add(personIntgNameNickname);
                personNameType.Add("NICKNAME");
            }


            var formerNames = person.FormerNames;
            if (formerNames != null)
            {
                foreach (var formerName in formerNames)
                {
                    var personIntgNameHistory = new PersonIntgNames
                    {
                        FirstName = formerName.GivenName,
                        LastName = formerName.FamilyName,
                        MiddleName = formerName.MiddleName,
                        PersonFullName = PersonNameService.FormatName("", formerName.GivenName, formerName.MiddleName, formerName.FamilyName, "")
                        ,
                        PersonNameType = "HISTORY"
                    };
                    personIntgNames.Add(personIntgNameHistory);
                    personNameType.Add("HISTORY");
                }
            }

            if (personNameType.Any())
                request.PersonIntgNames = personIntgNames;

            //birth & deceased date
            request.BirthDate = person.BirthDate;
            request.DeceasedDate = person.DeceasedDate;

            //privacy status
            if (!string.IsNullOrEmpty(person.PrivacyStatusCode))
            {
                request.PrivacyCode = person.PrivacyStatusCode;
            }
            else
            {
                request.PrivacyCode = (person.PrivacyStatus == PrivacyStatusType.unrestricted) ? null : person.PrivacyStatus.ToString();
            }

            //Gender
            request.Gender = person.Gender;

            //Gender Identity
            request.PersonGenderIdentity = person.GenderIdentityCode;

            //personal pronoun
            request.PersonPronoun = person.PersonalPronounCode;

            //religion
            request.Religion = person.Religion;

            //ethnicity code
            request.PersonEthnicCode = person.EthnicCodes;

            //races
            request.RaceCodes = person.RaceCodes;

            //language
            if (person.Languages != null && person.Languages.Any())
            {
                foreach (var language in person.Languages)
                {
                    PersonIntgLanguage personLanguage = new PersonIntgLanguage()
                    {
                        Language = language.Code,
                        LanguagePref = language.Preference.ToString()
                    };
                    request.PersonIntgLanguage.Add(personLanguage);
                }
            }

            //marital status
            request.MaritalStatus = person.MaritalStatus.HasValue ? person.MaritalStatus.Value.ToString() : string.Empty;

            //citizenshipStatus
            request.CitizenshipStatus = person.AlienStatus;

            //countryOfBirth
            request.BirthCountry = person.BirthCountry;

            //countryOfCitizenship
            request.PersonCitizenship = person.Citizenship;

            //roles
            if (person.Roles.Any())
            {
                foreach (var role in person.Roles)
                {
                    PersonIntgRoles personIntgRole = new PersonIntgRoles();
                    personIntgRole.PersonRoles = role.RoleType.ToString();
                    personIntgRole.PersonRoleStartDate = role.StartDate.HasValue ? role.StartDate : default(DateTime?);
                    personIntgRole.PersonRoleEndDate = role.EndDate.HasValue ? role.EndDate : default(DateTime?);
                    request.PersonIntgRoles.Add(personIntgRole);
                }
            }

            //identityDocuments
            if (person.DriverLicense != null)
            {
                PersonIntgIdentityDocuments driversLicense = new PersonIntgIdentityDocuments()
                {
                    PersonIdDocId = person.DriverLicense.LicenseNumber,
                    PersonIdDocType = person.DriverLicense.DocType,
                    PersonIdDocCtry = person.DriverLicense.IssuingState,
                    PersonIdDocExpDate =
                        (person.DriverLicense.ExpireDate.HasValue)
                            ? person.DriverLicense.ExpireDate
                            : default(DateTime?)
                };
                request.PersonIntgIdentityDocuments.Add(driversLicense);
            }
            if (person.Passport != null)
            {
                PersonIntgIdentityDocuments passport = new PersonIntgIdentityDocuments()
                {
                    PersonIdDocCtry = person.Passport.IssuingCountry,
                    PersonIdDocExpDate =
                        (person.Passport.ExpireDate.HasValue) ? person.Passport.ExpireDate : default(DateTime?),
                    PersonIdDocId = person.Passport.PassportNumber,
                    PersonIdDocType = person.Passport.DocType
                };
                request.PersonIntgIdentityDocuments.Add(passport);
            }
            if (person.IdentityDocuments != null && person.IdentityDocuments.Any())
            {
                foreach (var document in person.IdentityDocuments)
                {
                    PersonIntgIdentityDocuments identityDocument = new PersonIntgIdentityDocuments()
                    {
                        PersonIdDocCtry = document.Country,
                        PersonIdDocRegion = document.Region,
                        PersonIdDocExpDate =
                            (document.ExpireDate.HasValue) ? document.ExpireDate : default(DateTime?),
                        PersonIdDocId = document.Number,
                        PersonIdDocType = "OTHER"
                    };
                    request.PersonIntgIdentityDocuments.Add(identityDocument);
                }
            }
            //credentials
            if (person.PersonAltIds != null && person.PersonAltIds.Any())
            {
                foreach (var doc in person.PersonAltIds)
                {
                    PersonIntgAlternate identityDoc = new PersonIntgAlternate();
                    identityDoc.PersonAltId = doc.Id;
                    identityDoc.PersonAltIdType = doc.Type;
                    request.PersonIntgAlternate.Add(identityDoc);
                }
            }

            // interests
            if (person.Interests != null && person.Interests.Any())
            {
                request.PersonInterests = person.Interests;
            }
            // emails
            if (person.EmailAddresses != null && person.EmailAddresses.Any())
            {
                foreach (var emailAddress in person.EmailAddresses)
                {
                    request.PersonIntgEmailAddresses.Add(new PersonIntgEmailAddresses()
                    {
                        EmailAddressValue = emailAddress.Value,
                        EmailAddressType = emailAddress.TypeCode,
                        EmailPref = emailAddress.IsPreferred ? "Y" : string.Empty
                    });
                }
            }
            // addresses
            if (addresses != null && addresses.Any())
            {
                foreach (var address in addresses)
                {
                    var addressRequest = new PersonIntgAddresses();
                    addressRequest.PersonAddrIds = !string.IsNullOrEmpty(address.Guid) ? await GetAddressIdFromGuidAsync(address.Guid) : null;
                    addressRequest.PersonAddrTypes = address.TypeCode;
                    addressRequest.PersonAddrCities = !string.IsNullOrEmpty(address.City) ? address.City : address.IntlLocality;
                    addressRequest.PersonAddrCountries = !string.IsNullOrEmpty(address.CountryCode) ? address.CountryCode : address.Country;
                    addressRequest.PersonAddrSubregions = !string.IsNullOrEmpty(address.County) ? address.County : address.IntlSubRegion;
                    addressRequest.PersonAddrCodes = !string.IsNullOrEmpty(address.PostalCode) ? address.PostalCode : address.IntlPostalCode;
                    addressRequest.PersonAddrRegions = !string.IsNullOrEmpty(address.State) ? address.State : address.IntlRegion;
                    addressRequest.PersonAddrLongitude = address.Longitude.ToString();
                    addressRequest.PersonAddrLatitude = address.Latitude.ToString();
                    addressRequest.PersonAddrDeliveryPoint = address.DeliveryPoint;
                    addressRequest.PersonAddrCorrectionDigit = address.CorrectionDigit;
                    addressRequest.PersonAddrStartDate = address.EffectiveStartDate;
                    addressRequest.PersonAddrCarrierRoute = address.CarrierRoute;
                    addressRequest.PersonAddrEndDate = address.EffectiveEndDate;
                    if (address.AddressChapter != null)
                    {
                        var newChapters = new StringBuilder();
                        foreach (var chapters in address.AddressChapter)
                        {
                            if (newChapters.Length > 0) newChapters.Append(_SM);
                            newChapters.Append(chapters);
                        }
                        addressRequest.PersonAddrChapters = newChapters.ToString();
                    }
                    addressRequest.PersonAddrPreference = address.IsPreferredResidence == true ? "Y" : null;
                    if (address.SeasonalDates != null)
                    {
                        var newSeasonalStart = new StringBuilder();
                        var newSeasonalEnd = new StringBuilder();
                        foreach (var seasonalDates in address.SeasonalDates)
                        {
                            if (newSeasonalStart.Length > 0) newSeasonalStart.Append(_SM);
                            if (newSeasonalEnd.Length > 0) newSeasonalEnd.Append(_SM);
                            newSeasonalStart.Append(seasonalDates.StartOn);
                            newSeasonalEnd.Append(seasonalDates.EndOn);
                        }
                        addressRequest.PersonSeaAddrStartDate = newSeasonalStart.ToString();
                        addressRequest.PersonSeaAddrEndDate = newSeasonalEnd.ToString();
                    }
                    if (address.AddressLines != null && address.AddressLines.Any())
                    {
                        var newAddressLines = new StringBuilder();
                        foreach (var addressLine in address.AddressLines)
                        {
                            if (newAddressLines.Length > 0) newAddressLines.Append(_SM);
                            newAddressLines.Append(addressLine);
                        }
                        addressRequest.PersonAddrLines = newAddressLines.ToString();
                    }
                    request.PersonIntgAddresses.Add(addressRequest);
                }
            }
            // Phone numbers
            if (phones != null && phones.Any())
            {
                foreach (var phone in phones)
                {
                    var phoneRequest = new PersonIntgPhones();
                    phoneRequest.PhoneType = phone.TypeCode;
                    phoneRequest.PhoneNumber = phone.Number;
                    phoneRequest.PhoneExtension = phone.Extension;
                    phoneRequest.PhoneCallingCode = phone.CountryCallingCode;
                    phoneRequest.PhonePref = phone.IsPreferred ? "Y" : string.Empty;
                    request.PersonIntgPhones.Add(phoneRequest);
                }
            }
            // Social media
            if (person.SocialMedia != null && person.SocialMedia.Any())
            {
                foreach (var socialMedia in person.SocialMedia)
                {
                    var socialMediaRequest = new PersonIntgSocialMedia();
                    socialMediaRequest.SocialMediaTypes = socialMedia.TypeCode;
                    socialMediaRequest.SocialMediaHandles = socialMedia.Handle;
                    socialMediaRequest.SocialMediaPref = socialMedia.IsPreferred ? "Y" : null;
                    request.PersonIntgSocialMedia.Add(socialMediaRequest);
                }
            }
            return request;
        }

        #endregion

        #region Address Methods
        public async Task<List<string>> Get1098HierarchyAddressAsync(string id)
        {
            return await GetHierarchyAddressAsync(id, "TN98", DateTime.Today);
        }

        private async Task<List<string>> GetPreferredAddressAsync(string id)
        {
            return await GetHierarchyAddressAsync(id);
        }

        private async Task<List<string>> GetHierarchyAddressAsync(string id)
        {
            return await GetHierarchyAddressAsync(id, "PREFERRED");
        }

        private async Task<List<string>> GetHierarchyAddressAsync(string id, string hierarchy)
        {
            return await GetHierarchyAddressAsync(id, hierarchy, DateTime.Today);
        }

        private async Task<List<string>> GetHierarchyAddressAsync(string id, string hierarchy, DateTime? date)
        {
            TxGetHierarchyAddressResponse response = await transactionInvoker.ExecuteAsync<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                new TxGetHierarchyAddressRequest()
                {
                    IoPersonId = id,
                    InHierarchy = hierarchy,
                    InDate = date.GetValueOrDefault(DateTime.Today)
                });

            if (response == null || (String.IsNullOrEmpty(response.OutAddressId)))
            {
                return null;
            }

            return GetAddressLabel(response);
        }

        /// <summary>
        /// Get an address label from an Address entity
        /// </summary>
        /// <param name="address">Address domain entity</param>
        /// <returns>IEnumerable of type string</returns>
        private List<string> GetAddressLabel(TxGetHierarchyAddressResponse address)
        {
            List<string> label = new List<string>();

            if (address.OutAddressLabel.Count > 0)
            {
                label.AddRange(address.OutAddressLabel);
            }
            else
            {
                // Build address label
                if (!String.IsNullOrEmpty(address.OutAddressModifier))
                {
                    label.Add(address.OutAddressModifier);
                }
                if (address.OutAddressLines.Count > 0)
                {
                    label.AddRange(address.OutAddressLines);
                }
                string cityStatePostalCode = AddressProcessor.GetCityStatePostalCode(address.OutAddressCity, address.OutAddressState, address.OutAddressZip);
                if (!String.IsNullOrEmpty(cityStatePostalCode))
                {
                    label.Add(cityStatePostalCode);
                }
                if (!String.IsNullOrEmpty(address.OutAddressCountryDesc))
                {
                    // Country name gets included in all caps
                    label.Add(address.OutAddressCountryDesc.ToUpper());
                }
            }
            return label;
        }

        /// <summary>
        /// Get the address ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        public async Task<string> GetAddressIdFromGuidAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
        }

        #endregion

        #region InternationalParameters

        /// <summary>
        /// Read the international parameters records to extract date format used
        /// locally and setup in the INTL parameters.
        /// </summary>
        /// <returns>International Parameters with date properties</returns>
        private async Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> GetInternationalParametersAsync()
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
                        // throw new Exception(errorMessage);
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


        #region Search for Matching Persons

        /// <summary>
        /// Search for matching person records.
        /// </summary>
        /// <param name="person"><see cref="Person">Person</see> to use for matching</param>
        /// <returns>List of person guids</returns>
        public async Task<IEnumerable<string>> GetMatchingPersonsAsync(Domain.Base.Entities.Person person)
        {
            if (person == null || string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
                throw new ArgumentNullException("person", "Person first and last name are required for matching");

            var matchRequest = new GetPersonMatchRequest();
            matchRequest.PersonId = person.Id;
            matchRequest.FirstName = person.FirstName;
            matchRequest.MiddleName = person.MiddleName;
            matchRequest.LastName = person.LastName;
            matchRequest.BirthNameFirst = person.BirthNameFirst;
            matchRequest.BirthNameMiddle = person.BirthNameMiddle;
            matchRequest.BirthNameLast = person.BirthNameLast;
            matchRequest.BirthDate = person.BirthDate;
            matchRequest.Gender = person.Gender;
            matchRequest.Ssn = person.GovernmentId;
            if (person.EmailAddresses != null && person.EmailAddresses.Count() > 0)
            {
                var emailAddress = person.EmailAddresses.FirstOrDefault();
                if (emailAddress != null)
                {
                    matchRequest.EmailAddress = emailAddress.Value;
                }
            }

            var matchResponse = await transactionInvoker.ExecuteAsync<GetPersonMatchRequest, GetPersonMatchResponse>(matchRequest);
            if (matchResponse.ErrorMessages.Count() > 0)
            {
                var errorMessage = "Error(s) occurred during person matching:";
                errorMessage += string.Join(Environment.NewLine, matchResponse.ErrorMessages);
                logger.Error(errorMessage);
                throw new InvalidOperationException("Error occurred during person matching");
            }
            return matchResponse.PersonMatchingGuids;
        }

        /// <summary>
        /// Return the results from the matching algorithm when searching for a person based on supplied criteria
        /// </summary>
        /// <param name="criteria">The criteria for identifying a person</param>
        /// <returns>The actual results from the matching algorithm</returns>
        public async Task<IEnumerable<PersonMatchResult>> GetMatchingPersonResultsAsync(PersonMatchCriteria criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria", "Criteria required to query");

            var matchRequest = new GetPersonMatchResultsRequest();
            matchRequest.MatchCriteriaRecordId = criteria.MatchCriteriaIdentifier;
            matchRequest.MatchPersonNames = criteria.MatchNames.Select(x => new MatchPersonNames() { FirstNames = x.GivenName, MiddleNames = x.MiddleName, LastNames = x.FamilyName }).ToList();
            matchRequest.BirthDate = criteria.BirthDate;
            matchRequest.Gender = criteria.Gender;
            matchRequest.Ssn = criteria.GovernmentId;
            matchRequest.Phone = criteria.Phone;
            matchRequest.PhoneExtension = criteria.PhoneExtension;
            matchRequest.EmailAddress = criteria.EmailAddress;
            matchRequest.Prefix = criteria.Prefix;
            matchRequest.Suffix = criteria.Suffix;

            var matchResponse = await transactionInvoker.ExecuteAsync<GetPersonMatchResultsRequest, GetPersonMatchResultsResponse>(matchRequest);
            if (matchResponse == null)
            {
                throw new InvalidOperationException("An error occurred during person matching");
            }
            if (matchResponse.ErrorMessages.Count() > 0)
            {
                var errorMessage = "Error(s) occurred during person matching:";
                errorMessage += string.Join(Environment.NewLine, matchResponse.ErrorMessages);
                logger.Error(errorMessage);
                throw new InvalidOperationException("An error occurred during person matching");
            }
            var result = new List<PersonMatchResult>();
            if (matchResponse.MatchResults != null && matchResponse.MatchResults.Any())
            {
                result = matchResponse.MatchResults.Select(x => new PersonMatchResult(x.MatchPersonIds, x.MatchScores, x.MatchCategories)).ToList();
            }

            return result;
        }

        #endregion

        #region sync call used by other branches such as Finance
        public bool IsPerson(string personId)
        {
            var x = Task.Run(async () =>
            {
                return await IsPersonAsync(personId);
            }).GetAwaiter().GetResult();
            return x;
        }
        public TDomain Get<TDomain>(string id, Func<DataContracts.Person, TDomain> objectBuilder)
            where TDomain : Domain.Base.Entities.Person
        {
            var x = Task.Run(async () =>
            {
                return await GetAsync(id, objectBuilder);
            }).GetAwaiter().GetResult();
            return x;
        }
        #endregion
    }
}
