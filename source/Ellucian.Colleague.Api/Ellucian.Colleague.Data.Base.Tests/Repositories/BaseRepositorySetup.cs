// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Colleague.Configuration;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    public abstract class BaseRepositorySetup
    {
        protected Mock<IColleagueTransactionFactory> transFactoryMock;
        protected IColleagueTransactionFactory transFactory;
        protected Mock<ObjectCache> localCacheMock;
        protected ObjectCache localCache;
        protected Mock<ICacheProvider> cacheProviderMock;
        protected ICacheProvider cacheProvider;
        protected Mock<IColleagueDataReader> dataReaderMock;
        protected IColleagueDataReader dataReader;
        protected Mock<ILogger> loggerMock;
        protected ILogger logger;
        protected Mock<IColleagueTransactionInvoker> transManagerMock;
        protected IColleagueTransactionInvoker transManager;
        protected ApiSettings apiSettings;
        protected ColleagueSettings colleagueSettings;
        protected string colleagueTimeZone;

        protected void MockInitialize()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            transFactory = transFactoryMock.Object;
            // Cache Mock
            localCacheMock = new Mock<ObjectCache>();
            localCache = localCacheMock.Object;
            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();
            cacheProvider = cacheProviderMock.Object;
            // Set up data reader for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();
            dataReader = dataReaderMock.Object;
            // Logger mock
            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;
            // Set up transaction manager for mocking 
            transManagerMock = new Mock<IColleagueTransactionInvoker>();
            transManager = transManagerMock.Object;
            // Set up API settings object
            apiSettings = new ApiSettings("TEST");
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
            colleagueSettings = new ColleagueSettings();

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            // Setup localCacheMock as the object for the CacheProvider
            //cacheProviderMock.Setup(provider => provider.Get(It.IsAny<string>(), null)).Returns(localCache);
            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReader);
            // Set up transManagerMock as the object for the transaction manager
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
        }

        protected void MockCleanup()
        {
            transFactory = null;
            transFactoryMock = null;
            localCache = null;
            localCacheMock = null;
            cacheProvider = null;
            cacheProviderMock = null;
            dataReader = null;
            dataReaderMock = null;
            logger = null;
            loggerMock = null;
            transManager = null;
            transManagerMock = null;
            apiSettings = null;
        }

        /// <summary>
        /// Mock the read or selection of a single record from a file
        /// </summary>
        /// <typeparam name="T">Data contract type being read</typeparam>
        /// <param name="fileName">Name of Colleague file being read</param>
        /// <param name="record">The record to be returned</param>
        /// <param name="guid">Optional GUID of the file/record being read</param>
        protected void MockRecord<T>(string fileName, T record, string guid = null)
            where T : class, IColleagueEntity
        {
            if (dataReaderMock == null) MockInitialize();
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            // Allow a null record for exception processing
            //if (record == null) throw new ArgumentNullException("record");

            if (record == null)
            {
                // Read record using any input string
                dataReaderMock.Setup(r => r.ReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<T>(null);
                dataReaderMock.Setup(r => r.ReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<T>(null);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<T>(null);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<T>(null);

                // Bulk read using list of record IDs
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T>());

                // Bulk read using any criteria
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T>());

                return;
            }

            if (record.GetType() != typeof(T)) throw new ArgumentException("Record not of specified type", "record");

            if (string.IsNullOrEmpty(record.Recordkey))
            {
                // Read record using any input string
                dataReaderMock.Setup(r => r.ReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns(record);
                dataReaderMock.Setup(r => r.ReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns(record);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(record);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(record);

                // Bulk read using list of record IDs
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });

                // Bulk read using any criteria
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });
            }
            else
            {
                // Read record using record ID
                dataReaderMock.Setup(r => r.ReadRecord<T>(record.Recordkey, It.IsAny<bool>())).Returns(record);
                dataReaderMock.Setup(r => r.ReadRecord<T>(fileName, record.Recordkey, It.IsAny<bool>())).Returns(record);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(record.Recordkey, It.IsAny<bool>())).ReturnsAsync(record);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, record.Recordkey, It.IsAny<bool>())).ReturnsAsync(record);

                // Select using criteria or record IDs and criteria
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string>())).Returns(new [] { record.Recordkey });
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns(new [] { record.Recordkey });
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).ReturnsAsync(new[] { record.Recordkey });
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new[] { record.Recordkey });

                // Bulk read using individual record ID
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(new string[] { record.Recordkey }, It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, new string[] { record.Recordkey }, It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(new string[] { record.Recordkey }, It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, new string[] { record.Recordkey }, It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });

                // Bulk read with criteria
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });
            }

            if (!string.IsNullOrEmpty(guid) || typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
            {
                if (string.IsNullOrEmpty(guid))
                {
                    guid = (record as IColleagueGuidEntity) == null ? null : (record as IColleagueGuidEntity).RecordGuid;
                    if (string.IsNullOrEmpty(guid)) guid = GenerateGuid();
                }

                var guidLookup = new GuidLookup(guid);
                var lookupResult = new RecordKeyLookupResult { Guid = guid };

                // Read record using GUID
                dataReaderMock.Setup(r => r.ReadRecord<T>(guidLookup, It.IsAny<bool>())).Returns(record);
                dataReaderMock.Setup(r => r.ReadRecord<T>(fileName, guidLookup, It.IsAny<bool>())).Returns(record);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(guidLookup, It.IsAny<bool>())).ReturnsAsync(record);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, guidLookup, It.IsAny<bool>())).ReturnsAsync(record);

                // Bulk read record using GUID
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(new[] { guidLookup }, It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, new[] { guidLookup }, It.IsAny<bool>())).Returns(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(new[] { guidLookup }, It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, new[] { guidLookup }, It.IsAny<bool>())).ReturnsAsync(new Collection<T> { record });

                if (!string.IsNullOrEmpty(record.Recordkey))
                {
                    // Select using GUID to get record ID/Filename via GuidLookupResult
                    var guidLookupResult = new GuidLookupResult { Entity = fileName, PrimaryKey = record.Recordkey };
                    var guidSelectResult = new Dictionary<string, GuidLookupResult>();
                    guidSelectResult.Add(guid, guidLookupResult);
                    dataReaderMock.Setup(r => r.Select(new [] { guidLookup })).Returns(guidSelectResult);
                    dataReaderMock.Setup(r => r.SelectAsync(new[] { guidLookup })).ReturnsAsync(guidSelectResult);

                    // Select using filename/record ID to get GUID via RecordKeyLookupResult
                    var recLookup = new RecordKeyLookup(fileName, record.Recordkey, false);
                    var recordKeySelectResult = new Dictionary<string, RecordKeyLookupResult>();
                    recordKeySelectResult.Add(recLookup.ResultKey, lookupResult);
                    dataReaderMock.Setup(r => r.Select(new [] { recLookup })).Returns(recordKeySelectResult);
                    dataReaderMock.Setup(r => r.SelectAsync(new[] { recLookup })).ReturnsAsync(recordKeySelectResult);
                }
            }
        }

        /// <summary>
        /// Mock the read or selection of multiple records from a file using selection criteria
        /// </summary>
        /// <typeparam name="T">Data contract type being read</typeparam>
        /// <param name="fileName">Name of Colleague file being read</param>
        /// <param name="records">The records to be returned; required unless an outputMethod is provided</param>
        /// <param name="outputMethod">An optional function to produce the output records</param>
        protected void MockRecords<T>(string fileName, IEnumerable<T> records, Func<string, IEnumerable<T>, IList<T>> outputMethod = null)
            where T : class, IColleagueEntity
        {
            if (dataReaderMock == null) MockInitialize();
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            // Allow null for exception processing
            //if (records == null) throw new ArgumentNullException("records");
            var recordList = (records == null) ? new List<T>() : new List<T>(records);
            var recordTypeList = recordList.OfType<T>().ToList();
            if (recordList.Any() && (recordTypeList == null || recordTypeList.Count != recordList.Count()))
                throw new ArgumentException("All records not of specified type", "records");
            var keys = recordList.Select(r => r.Recordkey).ToArray();

            // Handle the case with no records passed in - everything returns null or an empty list
            if (keys.Length == 0)
            {
                dataReaderMock.Setup(r => r.ReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<T>(null);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                dataReaderMock.Setup(r => r.ReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<T>(null);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);

                // Read or select records using any input criteria
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string>())).Returns<string[]>(null);
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns<string[]>(null);
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T>());

                // Read or select records using key values - only return the corresponding records
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string[]>(null);
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(null);
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T>()));
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns(new Collection<T>());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T>()));


                // If this entity has a GUID, include mocking for the GUID lookups
                if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
                {
                    dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns(new Collection<T>());
                    dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns(new Collection<T>());
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T>());
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<T>());

                    dataReaderMock.Setup(r => r.Select(It.IsAny<RecordKeyLookup[]>())).Returns(new Dictionary<string, RecordKeyLookupResult>());
                    dataReaderMock.Setup(r => r.Select(It.IsAny<GuidLookup[]>())).Returns(new Dictionary<string, GuidLookupResult>());
                    dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(new Dictionary<string, RecordKeyLookupResult>());
                    dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>());

                }

                return;
            }

            // With no output method, this will effectively return all the data, since we can't do anything with the criteria
            if (outputMethod == null)
            {
                dataReaderMock.Setup(r => r.ReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((key, flag) =>
                    recordList.FirstOrDefault(r => r.Recordkey == key));
                dataReaderMock.Setup(r => r.ReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, key, flag) =>
                    recordList.FirstOrDefault(r => r.Recordkey == key));
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((key, flag) =>
                    Task.FromResult(recordList.FirstOrDefault(r => r.Recordkey == key)));
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, key, flag) =>
                    Task.FromResult(recordList.FirstOrDefault(r => r.Recordkey == key)));


                // Read or select records using any input criteria
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string>())).Returns(keys);
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                    new Collection<T>(keys.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()));
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                    new Collection<T>(keys.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()));
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).ReturnsAsync(keys);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                    Task.FromResult(new Collection<T>(keys.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                    Task.FromResult(new Collection<T>(keys.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));


                // Read or select records using key values - only return the corresponding records
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string, string[], string>((file, ids, criteria) => ids);
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string, string[], string>((file, ids, criteria) => Task.FromResult(ids));
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                    new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult(new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                    new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                    Task.FromResult(new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));

                // If this entity has a GUID, include mocking for the GUID lookups
                if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
                {
                    dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList()));
                    dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<string, GuidLookup[], bool>((file, guidLookups, flag) =>
                        new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList()));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList())));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<string, GuidLookup[], bool>((file, guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList())));

                    dataReaderMock.Setup(r => r.Select(recordList.Select(rl => new RecordKeyLookup(fileName, rl.Recordkey, string.Empty, string.Empty, false)).ToArray()))
                        .Returns(recordList.ToDictionary(r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult { Guid = (r as IColleagueGuidEntity).RecordGuid }));
                    dataReaderMock.Setup(r => r.Select(recordList.Select(rl => new GuidLookup((rl as IColleagueGuidEntity).RecordGuid)).ToArray()))
                        .Returns(recordList.ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid, r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey }));
                    dataReaderMock.Setup(r => r.SelectAsync(recordList.Select(rl => new RecordKeyLookup(fileName, rl.Recordkey, string.Empty, string.Empty, false)).ToArray()))
                        .ReturnsAsync(recordList.ToDictionary(r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult { Guid = (r as IColleagueGuidEntity).RecordGuid }));
                    dataReaderMock.Setup(r => r.SelectAsync(recordList.Select(rl => new GuidLookup((rl as IColleagueGuidEntity).RecordGuid)).ToArray()))
                        .ReturnsAsync(recordList.ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid, r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey }));
                }
                return;
            }

            dataReaderMock.Setup(r => r.ReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                outputMethod.Invoke(criteria, recordList).FirstOrDefault());
            dataReaderMock.Setup(r => r.ReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                outputMethod.Invoke(criteria, recordList).FirstOrDefault());
            dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                Task.FromResult(outputMethod.Invoke(criteria, recordList).FirstOrDefault()));
            dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                Task.FromResult(outputMethod.Invoke(criteria, recordList).FirstOrDefault()));


            // Read or select records using any input criteria
            dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string>())).Returns<string, string>((file, criteria) =>
                outputMethod.Invoke(criteria, recordList).Select(r => r.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                new Collection<T>(outputMethod.Invoke(criteria, recordList)));
            dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                new Collection<T>(outputMethod.Invoke(criteria, recordList)));
            dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns<string, string>((file, criteria) =>
                Task.FromResult(outputMethod.Invoke(criteria, recordList).Select(r => r.Recordkey).ToArray()));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                Task.FromResult(new Collection<T>(outputMethod.Invoke(criteria, recordList))));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                Task.FromResult(new Collection<T>(outputMethod.Invoke(criteria, recordList))));


            // Read or select records using key values - only return the corresponding records
            dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string, string[], string>((file, ids, criteria) => 
                ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).Select(x => x.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()));
            dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()));
            dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string, string[], string>((file, ids, criteria) =>
                Task.FromResult(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).Select(x => x.Recordkey).ToArray()));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                Task.FromResult(new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                Task.FromResult(new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));


            if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
            {
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList()));
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<string, GuidLookup[], bool>((file, guidLookups, flag) =>
                        new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList()));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<string, GuidLookup[], bool>((file, guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList())));

                dataReaderMock.Setup(r => r.Select(recordList.Select(rl => new RecordKeyLookup(fileName, rl.Recordkey, string.Empty, string.Empty, false)).ToArray()))
                    .Returns<string>(criteria => outputMethod.Invoke(criteria, recordList).Where(r => r != null)
                        .ToDictionary(r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult { Guid = (r as IColleagueGuidEntity).RecordGuid }));
                dataReaderMock.Setup(r => r.Select(recordList.Select(rl => new GuidLookup((rl as IColleagueGuidEntity).RecordGuid)).ToArray()))
                    .Returns<string>(criteria => outputMethod.Invoke(criteria, recordList).Where(r => r != null)
                        .ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid, r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey }));
                dataReaderMock.Setup(r => r.SelectAsync(recordList.Select(rl => new RecordKeyLookup(fileName, rl.Recordkey, string.Empty, string.Empty, false)).ToArray()))
                    .Returns<string>(criteria => Task.FromResult(outputMethod.Invoke(criteria, recordList).Where(r => r != null)
                        .ToDictionary(r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult { Guid = (r as IColleagueGuidEntity).RecordGuid })));
                dataReaderMock.Setup(r => r.SelectAsync(recordList.Select(rl => new GuidLookup((rl as IColleagueGuidEntity).RecordGuid)).ToArray()))
                    .Returns<string>(criteria => Task.FromResult(outputMethod.Invoke(criteria, recordList).Where(r => r != null)
                        .ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid, r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey })));

            }
        }

        /// <summary>
        /// Mock records using a dictionary to provide the keys and "records"
        /// </summary>
        /// <typeparam name="T">Data contract type being read</typeparam>
        /// <param name="fileName">Name of Colleague file being read</param>
        /// <param name="keyedRecords">The dictionary of keys and records to be returned</param>
        /// <param name="outputMethod">An optional function to produce the output records</param>
        protected void MockRecords<T>(string fileName, IDictionary<string, T> keyedRecords, Func<IDictionary<string, T>, string[], string, IList<T>> outputMethod = null)
            where T : class, IColleagueEntity
        {
            if (dataReaderMock == null) MockInitialize();
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (keyedRecords == null) throw new ArgumentNullException("keyedRecords");
            var recordList = new List<T>(keyedRecords.Values);
            var recordTypeList = recordList.OfType<T>().ToList();
            if (recordList.Any() && (recordTypeList == null || recordTypeList.Count != recordList.Count()))
                throw new ArgumentException("All records not of specified type", "keyedRecords");
            var keys = keyedRecords.Keys.ToArray();

            // Read individual records
            dataReaderMock.Setup(r => r.ReadRecord<T>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                { T record = null; keyedRecords.TryGetValue(id, out record); return record; });
            dataReaderMock.Setup(r => r.ReadRecord<T>(fileName, It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                { T record = null; keyedRecords.TryGetValue(id, out record); return record; });
            dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                { T record = null; keyedRecords.TryGetValue(id, out record); return Task.FromResult(record); });
            dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
                { T record = null; keyedRecords.TryGetValue(id, out record); return Task.FromResult(record); });

            if (outputMethod == null)
            {
                // Select and bulk read with no output function to process
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string>())).Returns<string>(criteria => keys.ToArray());
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string[], string>((ids, criteria) => ids.ToArray());
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns<string>(criteria => Task.FromResult(keys.ToArray()));
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string[], string>((ids, criteria) => Task.FromResult(ids.ToArray()));

                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string[]>(), true)).Returns<string[], bool>((ids, flag) => 
                    new Collection<T>(keyedRecords.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList()));
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, keys, true)).Returns<string[], bool>((ids, flag) =>
                    new Collection<T>(keyedRecords.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList()));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), true)).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult(new Collection<T>(keyedRecords.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, keys, true)).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult(new Collection<T>(keyedRecords.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList())));

            }
            else
            {
                // Select and bulk read with an output function to process
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string>())).Returns<string>(criteria =>
                    outputMethod.Invoke(keyedRecords, keys, criteria).Select(r => r.Recordkey).ToArray());
                dataReaderMock.Setup(r => r.Select(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string[], string>((ids, criteria) =>
                    outputMethod.Invoke(keyedRecords, ids, criteria).Select(r => r.Recordkey).ToArray());
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns<string>(criteria =>
                    Task.FromResult(outputMethod.Invoke(keyedRecords, keys, criteria).Select(r => r.Recordkey).ToArray()));
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string[], string>((ids, criteria) =>
                    Task.FromResult(outputMethod.Invoke(keyedRecords, ids, criteria).Select(r => r.Recordkey).ToArray()));

                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<string[]>(), true)).Returns<string[], bool>((ids, flag) => 
                    new Collection<T>(outputMethod.Invoke(keyedRecords, ids, null)));
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, keys, true)).Returns<string[], bool>((ids, flag) => 
                    new Collection<T>(outputMethod.Invoke(keyedRecords, ids, null)));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), true)).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult(new Collection<T>(outputMethod.Invoke(keyedRecords, ids, null))));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, keys, true)).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult(new Collection<T>(outputMethod.Invoke(keyedRecords, ids, null))));
            }

            if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
            {
                if (outputMethod == null)
                {
                    // GUID lookup processing with no output function to process
                    dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<GuidLookup[]>(), true)).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        new Collection<T>(guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToList()));
                    dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<GuidLookup[]>(), true)).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        new Collection<T>(guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToList()));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), true)).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToList())));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), true)).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToList())));
                }
                else
                {
                    // GUID lookup processing with an output function to process
                    dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<GuidLookup[]>(), true)).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        new Collection<T>(outputMethod.Invoke(
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToDictionary(k => k.Recordkey, v => v),
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).Select(x => x.Recordkey).ToArray(),
                            null)));
                    dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<GuidLookup[]>(), true)).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        new Collection<T>(outputMethod.Invoke(
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToDictionary(k => k.Recordkey, v => v),
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).Select(x => x.Recordkey).ToArray(),
                            null)));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), true)).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(outputMethod.Invoke(
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToDictionary(k => k.Recordkey, v => v),
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).Select(x => x.Recordkey).ToArray(),
                            null))));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), true)).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(outputMethod.Invoke(
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToDictionary(k => k.Recordkey, v => v),
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).Select(x => x.Recordkey).ToArray(),
                            null))));

                }

                var recordLookups = recordList.Select(r => new RecordKeyLookup(fileName, r.Recordkey, string.Empty, string.Empty, false)).ToArray();
                var lookupResults = recordList.ToDictionary
                    (r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult() { Guid = (r as IColleagueGuidEntity).RecordGuid });
                dataReaderMock.Setup(r => r.Select(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(rkls =>
                    rkls.Select(x => lookupResults.FirstOrDefault(y => y.Key == x.ResultKey)).ToDictionary(k => k.Key, v => v.Value));
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(rkls =>
                    Task.FromResult(rkls.Select(x => lookupResults.FirstOrDefault(y => y.Key == x.ResultKey)).ToDictionary(k => k.Key, v => v.Value)));

                var guidLookup = recordList.Select(r => new GuidLookup((r as IColleagueGuidEntity).RecordGuid)).ToArray();
                var guidLookupResult = recordList.ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid,
                    r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey });
                dataReaderMock.Setup(r => r.Select(It.IsAny<GuidLookup[]>())).Returns <GuidLookup[]>(glus =>
                    glus.Select(glu => guidLookupResult.FirstOrDefault(glr => glr.Key == glu.Guid)).ToDictionary(k => k.Key, v => v.Value));
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(glus =>
                   Task.FromResult(glus.Select(glu => guidLookupResult.FirstOrDefault(glr => glr.Key == glu.Guid)).ToDictionary(k => k.Key, v => v.Value)));
            }
        }

        /// <summary>
        /// Mock the read or selection of multiple records from a file using specific record keys
        /// </summary>
        /// <typeparam name="T">Data contract type being read</typeparam>
        /// <param name="fileName">Name of Colleague file being read</param>
        /// <param name="keys">The array of record keys</param>
        /// <param name="records">The records to be returned; required unless an outputMethod is provided</param>
        /// <param name="outputMethod">An optional function to produce the output records</param>
        protected void MockRecords<T>(string fileName, string[] keys, IEnumerable<T> records, Func<string[], bool, Collection<T>> outputMethod)
            where T : class, IColleagueEntity
        {
            if (dataReaderMock == null) MockInitialize();
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (keys == null || !keys.Any()) throw new ArgumentNullException("keys");
            if (records == null && outputMethod == null) throw new ArgumentNullException("records");
            var recordList = records == null ? new List<T>() : new List<T>(records);
            if (keys.Length != recordList.Count) throw new ArgumentException("Counts of keys and records don't match.");
            var recordTypeList = recordList.OfType<T>().ToList();
            if (recordList.Any() && (recordTypeList == null || recordTypeList.Count != recordList.Count()))
                throw new ArgumentException("All records not of specified type", "records");
            var keyList = keys.ToList();

            // Form a dictionary from the keys and records
            var keyedRecords = new Dictionary<string, T>();
            for (int i = 0; i < keys.Length; i++)
            {
                keyedRecords.Add(keys[i], recordList[i]);
            }

            if (outputMethod == null)
            {
                MockRecords(fileName, keyedRecords, null);
                return;
            }

            dataReaderMock.Setup(r => r.Select(fileName, keys, It.IsAny<string>())).Returns(outputMethod.Invoke(keys, true).Select(r => r.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.BulkReadRecord<T>(keys, true)).Returns(outputMethod.Invoke(keys, true));
            dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, keys, true)).Returns(new Collection<T>(outputMethod.Invoke(keys, true)));
            dataReaderMock.Setup(r => r.SelectAsync(fileName, keys, It.IsAny<string>())).ReturnsAsync(outputMethod.Invoke(keys, true).Select(r => r.Recordkey).ToArray());
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(keys, true)).ReturnsAsync(outputMethod.Invoke(keys, true));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, keys, true)).ReturnsAsync(new Collection<T>(outputMethod.Invoke(keys, true)));


            if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
            {
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(It.IsAny<GuidLookup[]>(), true)).Returns(new Collection<T>(outputMethod.Invoke(keys, true)));
                dataReaderMock.Setup(r => r.BulkReadRecord<T>(fileName, It.IsAny<GuidLookup[]>(), true)).Returns(new Collection<T>(outputMethod.Invoke(keys, true)));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), true)).ReturnsAsync(new Collection<T>(outputMethod.Invoke(keys, true)));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), true)).ReturnsAsync(new Collection<T>(outputMethod.Invoke(keys, true)));

            }
        }

        /// <summary>
        /// Setup the mocking structure for cache verification
        /// </summary>
        /// <typeparam name="T">Type of data being stored in the cache</typeparam>
        /// <param name="cacheKey">The full cache key used</param>
        /// <param name="cachedObject">The data being stored in the cache, of type T</param>
        /// <param name="existsInCache">Is the data already in the cache?</param>
        /// <param name="lockHandle">An object used for locking the cache; if null, no locking is done.</param>
        protected void MockCacheSetup<T>(string cacheKey, T cachedObject, bool existsInCache = false, object lockHandle = null)
            where T : class
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");

            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(existsInCache);
            if (existsInCache)
            {
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(cachedObject).Verifiable();
            }
            else
            {
                cacheProviderMock.Setup(x => x.GetAndLock(cacheKey, out lockHandle, null)).Returns(null);
                if (lockHandle == null)
                {
                    cacheProviderMock.Setup(x => x.Add(cacheKey, cachedObject, It.IsAny<CacheItemPolicy>(), null)).Verifiable();
                }
                else
                {
                    cacheProviderMock.Setup(x => x.AddAndUnlock(cacheKey, cachedObject, lockHandle, It.IsAny<CacheItemPolicy>(), null)).Verifiable();
                }
                cacheProviderMock.Setup(x => x.Unlock(cacheKey, lockHandle, null));
                cacheProviderMock.SetupGet(x => x[cacheKey]).Returns(cachedObject);
            }
        }

        /// <summary>
        /// Verify that the caching functioned as expected
        /// </summary>
        /// <typeparam name="T">Type of data being cached</typeparam>
        /// <param name="cacheKey">The full cache key used</param>
        /// <param name="cachedObject">The data being stored in the cache, of type T</param>
        /// <param name="existsInCache">Was the data already in the cache?</param>
        /// <param name="lockHandle">An object used for locking the cache; if null, no locking is done.</param>
        protected void VerifyCache<T>(string cacheKey, T cachedObject, bool existsInCache = false, object lockHandle = null)
            where T : class
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");

            if (existsInCache)
            {
                cacheProviderMock.Verify(m => m.Get(cacheKey, null));
            }
            else
            {
                if (lockHandle == null)
                {
                    cacheProviderMock.Verify(x => x.Add(cacheKey, cachedObject, It.IsAny<CacheItemPolicy>(), null));
                }
                else
                {
                    cacheProviderMock.Verify(x => x.AddAndUnlock(cacheKey, cachedObject, lockHandle, It.IsAny<CacheItemPolicy>(), null));
                }
            }
        }

        /// <summary>
        /// Generate a GUID
        /// </summary>
        /// <returns>The new GUID</returns>
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString().ToLowerInvariant();
        }

        #region Async Additions

        /// <summary>
        /// Mock the read or selection of a single record from a file
        /// </summary>
        /// <typeparam name="T">Data contract type being read</typeparam>
        /// <param name="fileName">Name of Colleague file being read</param>
        /// <param name="record">The record to be returned</param>
        /// <param name="guid">Optional GUID of the file/record being read</param>
        protected void MockRecordAsync<T>(string fileName, T record, string guid = null)
            where T : class, IColleagueEntity
        {
           
            if (dataReaderMock == null) MockInitialize();
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            // Allow a null here for exception processing
            //if (record == null) throw new ArgumentNullException("record");
            if (record != null && record.GetType() != typeof(T)) throw new ArgumentException("Record not of specified type", "record");
            
            // Set up mocking for a null record
            if (record == null)
            {
                // Read record using any input string
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<Task<T>>(null);
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<Task<T>>(null);

                // Bulk read using list of record IDs
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<Task<T>>(null);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<Task<T>>(null);
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<Task<T>>(null);
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<Task<T>>(null);

                // Bulk read using any criteria
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<Task<T>>(null);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<Task<T>>(null);

                return;
            }

            if (string.IsNullOrEmpty(record.Recordkey))
            {
                // Read record using any input string
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(record));
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(record));

                // Bulk read using list of record IDs
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T> { record } }));
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T> { record } }));

                // Bulk read using any criteria
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
            }
            else
            {
                // Read record using record ID
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(record.Recordkey, It.IsAny<bool>())).Returns(Task.FromResult(record));
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, record.Recordkey, It.IsAny<bool>())).Returns(Task.FromResult(record));

                // Select using criteria or record IDs and criteria
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns(Task.FromResult(new[] { record.Recordkey }));
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns(Task.FromResult(new[] { record.Recordkey }));

                // Bulk read using individual record ID
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(new string[] { record.Recordkey }, It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, new string[] { record.Recordkey }, It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(new string[] { record.Recordkey }, It.IsAny<bool>())).Returns(Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T> { record } }));
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(fileName, new string[] { record.Recordkey }, It.IsAny<bool>())).Returns(Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T> { record } }));

                // Bulk read with criteria
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
            }

            if (!string.IsNullOrEmpty(guid) || typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
            {
                if (string.IsNullOrEmpty(guid))
                {
                    guid = (record as IColleagueGuidEntity) == null ? null : (record as IColleagueGuidEntity).RecordGuid;
                    if (string.IsNullOrEmpty(guid)) guid = GenerateGuid();
                }

                var guidLookup = new GuidLookup(guid);
                var lookupResult = new RecordKeyLookupResult { Guid = guid };

                // Read record using GUID
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(guidLookup, It.IsAny<bool>())).Returns(Task.FromResult(record));
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, guidLookup, It.IsAny<bool>())).Returns(Task.FromResult(record));

                // Bulk read record using GUID
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(new[] { guidLookup }, It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, new[] { guidLookup }, It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T> { record }));
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(fileName, new[] { guidLookup }, It.IsAny<bool>())).Returns(Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T> { record } }));

                if (!string.IsNullOrEmpty(record.Recordkey))
                {
                    // Select using GUID to get record ID/Filename via GuidLookupResult
                    var guidLookupResult = new GuidLookupResult { Entity = fileName, PrimaryKey = record.Recordkey };
                    var guidSelectResult = new Dictionary<string, GuidLookupResult>();
                    guidSelectResult.Add(guid, guidLookupResult);
                    dataReaderMock.Setup(r => r.SelectAsync(new[] { guidLookup })).Returns(Task.FromResult(guidSelectResult));

                    // Select using filename/record ID to get GUID via RecordKeyLookupResult
                    var recLookup = new RecordKeyLookup(fileName, record.Recordkey, false);
                    var recordKeySelectResult = new Dictionary<string, RecordKeyLookupResult>();
                    recordKeySelectResult.Add(recLookup.ResultKey, lookupResult);
                    dataReaderMock.Setup(r => r.SelectAsync(new[] { recLookup })).Returns(Task.FromResult(recordKeySelectResult));
                }
            }
        }

        /// <summary>
        /// Mock the read or selection of multiple records from a file using selection criteria
        /// </summary>
        /// <typeparam name="T">Data contract type being read</typeparam>
        /// <param name="fileName">Name of Colleague file being read</param>
        /// <param name="records">The records to be returned; required unless an outputMethod is provided</param>
        /// <param name="outputMethod">An optional function to produce the output records</param>
        protected void MockRecordsAsync<T>(string fileName, IEnumerable<T> records, Func<string, IEnumerable<T>, IList<T>> outputMethod = null)
            where T : class, IColleagueEntity
        {
            if (dataReaderMock == null) MockInitialize();
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            // Allow null records list for exception testing
            //if (records == null) throw new ArgumentNullException("records");
            List<T> recordList = (records == null) ? new List<T>() : new List<T>(records);
            var recordTypeList = recordList.OfType<T>().ToList();
            if (recordList.Any() && (recordTypeList == null || recordTypeList.Count != recordList.Count()))
                throw new ArgumentException("All records not of specified type", "records");
            var keys = recordList.Select(r => r.Recordkey).ToArray();

            // With no output method, this will effectively return all the data, since we can't do anything with the criteria
            if (outputMethod == null)
            {
                // If there are no record keys, then always return the entire (empty) record list for exception testing
                if (keys.Length == 0)
                {
                    dataReaderMock.Setup<Task<T>>(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>()))
                        .ReturnsAsync(null);
                    dataReaderMock.Setup<Task<T>>(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                        .ReturnsAsync(null);
                    dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(null);
                    dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>()))
                        .ReturnsAsync(null);
                    dataReaderMock.Setup<Task<Collection<T>>>(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>()))
                        .ReturnsAsync(null);
                    dataReaderMock.Setup<Task<Collection<T>>>(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                        .ReturnsAsync(null);
                    dataReaderMock.Setup<Task<Collection<T>>>(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>()))
                        .ReturnsAsync(null);
                    dataReaderMock.Setup<Task<Collection<T>>>(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                        .ReturnsAsync(null);
                    dataReaderMock.Setup<Task<BulkReadOutput<T>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>()))
                        .ReturnsAsync(new BulkReadOutput<T>() { BulkRecordsRead = null, InvalidKeys = null, InvalidRecords = null });
                    dataReaderMock.Setup<Task<BulkReadOutput<T>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                        .ReturnsAsync(new BulkReadOutput<T>() { BulkRecordsRead = null, InvalidKeys = null, InvalidRecords = null });


                    // If this entity has a GUID, include mocking for the GUID lookups
                    if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
                    {
                        dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>()))
                            .ReturnsAsync(null);
                        dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<GuidLookup[]>(), It.IsAny<bool>()))
                            .ReturnsAsync(null);
                        dataReaderMock.Setup<Task<BulkReadOutput<T>>>(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(It.IsAny<string>(), It.IsAny<GuidLookup[]>(), It.IsAny<bool>()))
                            .ReturnsAsync(new BulkReadOutput<T>() { BulkRecordsRead = null, InvalidKeys = null, InvalidRecords = null });
                        dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);
                        dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                    }
                    // done with mock setup for empty list
                    return;
                }

                // Set up the mocks for a non-empty list of records
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((key, flag) =>
                    Task.FromResult( recordList.FirstOrDefault(r => r.Recordkey == key)));
                dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, key, flag) =>
                    Task.FromResult( recordList.FirstOrDefault(r => r.Recordkey == key)));

                // Read or select records using any input criteria
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns(
                    Task.FromResult(keys));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                    Task.FromResult( new Collection<T>(keys.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                    Task.FromResult( new Collection<T>(keys.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));

                // Read or select records using key values - only return the corresponding records
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string, string[], string>((file, ids, criteria) => 
                    Task.FromResult(ids));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult( new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                    Task.FromResult( new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()) }));
                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                    Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()) }));

                // If this entity has a GUID, include mocking for the GUID lookups
                if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
                {
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList())));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<string, GuidLookup[], bool>((file, guidLookups, flag) =>
                        Task.FromResult( new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList())));

                    dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<string, GuidLookup[], bool>((file, guidLookups, flag) =>
                        Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList()) }));

                    dataReaderMock.Setup(r => r.SelectAsync(recordList.Select(rl => new RecordKeyLookup(fileName, rl.Recordkey, string.Empty, string.Empty, false)).ToArray()))
                        .Returns(Task.FromResult(recordList.ToDictionary(r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult { Guid = (r as IColleagueGuidEntity).RecordGuid })));
                    dataReaderMock.Setup(r => r.SelectAsync(recordList.Select(rl => new GuidLookup((rl as IColleagueGuidEntity).RecordGuid)).ToArray()))
                        .Returns(Task.FromResult(recordList.ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid, r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey })));
                }
                return;
            }

            dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                Task.FromResult(outputMethod.Invoke(criteria, recordList).FirstOrDefault()));
            dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                Task.FromResult(outputMethod.Invoke(criteria, recordList).FirstOrDefault()));

            // Read or select records using any input criteria
            dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns<string, string>((file, criteria) =>
                Task.FromResult(outputMethod.Invoke(criteria, recordList).Select(r => r.Recordkey).ToArray()));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((criteria, flag) =>
                Task.FromResult( new Collection<T>(outputMethod.Invoke(criteria, recordList))));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, string, bool>((file, criteria, flag) =>
                Task.FromResult( new Collection<T>(outputMethod.Invoke(criteria, recordList))));

            // Read or select records using key values - only return the corresponding records
            dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string, string[], string>((file, ids, criteria) =>
                Task.FromResult(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).Select(x => x.Recordkey).ToArray()));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                Task.FromResult(new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                Task.FromResult(new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList())));
            dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()) }));
            dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T>(ids.Select(id => recordList.FirstOrDefault(r => r.Recordkey == id)).Where(r => r != null).ToList()) }));

            if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                       Task.FromResult( new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<string, GuidLookup[], bool>((file, guidLookups, flag) =>
                       Task.FromResult(  new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList())));

                dataReaderMock.Setup(r => r.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<string, GuidLookup[], bool>((file, guidLookups, flag) =>
                    Task.FromResult(new BulkReadOutput<T>() { BulkRecordsRead = new Collection<T>(guidLookups.Select(g => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == g.Guid)).ToList()) }));

                dataReaderMock.Setup(r => r.SelectAsync(recordList.Select(rl => new RecordKeyLookup(fileName, rl.Recordkey, string.Empty, string.Empty, false)).ToArray()))
                    .Returns<string>(criteria => Task.FromResult(outputMethod.Invoke(criteria, recordList).Where(r => r != null)
                        .ToDictionary(r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult { Guid = (r as IColleagueGuidEntity).RecordGuid })));
                dataReaderMock.Setup(r => r.SelectAsync(recordList.Select(rl => new GuidLookup((rl as IColleagueGuidEntity).RecordGuid)).ToArray()))
                    .Returns<string>(criteria => Task.FromResult(outputMethod.Invoke(criteria, recordList).Where(r => r != null)
                        .ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid, r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey })));
            }
        }

        /// <summary>
        /// Mock records using a dictionary to provide the keys and "records"
        /// </summary>
        /// <typeparam name="T">Data contract type being read</typeparam>
        /// <param name="fileName">Name of Colleague file being read</param>
        /// <param name="keyedRecords">The dictionary of keys and records to be returned</param>
        /// <param name="outputMethod">An optional function to produce the output records</param>
        protected void MockRecordsAsync<T>(string fileName, IDictionary<string, T> keyedRecords, Func<IDictionary<string, T>, string[], string, IList<T>> outputMethod = null)
            where T : class, IColleagueEntity
        {
            if (dataReaderMock == null) MockInitialize();
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (keyedRecords == null) throw new ArgumentNullException("keyedRecords");
            var recordList = new List<T>(keyedRecords.Values);
            var recordTypeList = recordList.OfType<T>().ToList();
            if (recordList.Any() && (recordTypeList == null || recordTypeList.Count != recordList.Count()))
                throw new ArgumentException("All records not of specified type", "keyedRecords");
            var keys = keyedRecords.Keys.ToArray();

            // Read individual records
            dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((id, flag) =>
            { T record = null; keyedRecords.TryGetValue(id, out record); return Task.FromResult(record); });
            dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>((id, flag) =>
            { T record = null; keyedRecords.TryGetValue(id, out record); return Task.FromResult(record); });

            if (outputMethod == null)
            {
                // Select and bulk read with no output function to process
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns<string, string>((file, criteria) => Task.FromResult(keys.ToArray()));
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string, string[], string>((file, ids, criteria) =>Task.FromResult( ids.ToArray()));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                   Task.FromResult( new Collection<T>(keyedRecords.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList())));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                   Task.FromResult( new Collection<T>(keyedRecords.Where(x => ids.Contains(x.Key)).Select(x => x.Value).ToList())));
            }
            else
            {
                // Select and bulk read with an output function to process
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string>())).Returns<string, string>((file, criteria) =>
                  Task.FromResult(  outputMethod.Invoke(keyedRecords, keys, criteria).Select(r => r.Recordkey).ToArray()));
                dataReaderMock.Setup(r => r.SelectAsync(fileName, It.IsAny<string[]>(), It.IsAny<string>())).Returns<string,string[], string>((file, ids, criteria) =>
                    Task.FromResult(outputMethod.Invoke(keyedRecords, ids, criteria).Select(r => r.Recordkey).ToArray()));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                  Task.FromResult(  new Collection<T>(outputMethod.Invoke(keyedRecords, ids, null))));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string, string[], bool>((file, ids, flag) =>
                   Task.FromResult( new Collection<T>(outputMethod.Invoke(keyedRecords, ids, null))));
            }

            if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
            {
                if (outputMethod == null)
                {
                    // GUID lookup processing with no output function to process
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                       Task.FromResult( new Collection<T>(guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToList())));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                        Task.FromResult(new Collection<T>(guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToList())));
                }
                else
                {
                    // GUID lookup processing with an output function to process
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                       Task.FromResult( new Collection<T>(outputMethod.Invoke(
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToDictionary(k => k.Recordkey, v => v),
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).Select(x => x.Recordkey).ToArray(),
                            null))));
                    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns<GuidLookup[], bool>((guidLookups, flag) =>
                      Task.FromResult(  new Collection<T>(outputMethod.Invoke(
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).ToDictionary(k => k.Recordkey, v => v),
                            guidLookups.Select(x => recordList.FirstOrDefault(r => (r as IColleagueGuidEntity).RecordGuid == x.Guid)).Where(x => x != null).Select(x => x.Recordkey).ToArray(),
                            null))));
                }

                var recordLookups = recordList.Select(r => new RecordKeyLookup(fileName, r.Recordkey, string.Empty, string.Empty, false)).ToArray();
                var lookupResults = recordList.ToDictionary
                    (r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult() { Guid = (r as IColleagueGuidEntity).RecordGuid });
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(rkls =>
                   Task.FromResult( rkls.Select(x => lookupResults.FirstOrDefault(y => y.Key == x.ResultKey)).ToDictionary(k => k.Key, v => v.Value)));

                var guidLookup = recordList.Select(r => new GuidLookup((r as IColleagueGuidEntity).RecordGuid)).ToArray();
                var guidLookupResult = recordList.ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid,
                    r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey });
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(glus =>
                 Task.FromResult(   glus.Select(glu => guidLookupResult.FirstOrDefault(glr => glr.Key == glu.Guid)).ToDictionary(k => k.Key, v => v.Value)));
            }
        }

        /// <summary>
        /// Mock the read or selection of multiple records from a file using specific record keys
        /// </summary>
        /// <typeparam name="T">Data contract type being read</typeparam>
        /// <param name="fileName">Name of Colleague file being read</param>
        /// <param name="keys">The array of record keys</param>
        /// <param name="records">The records to be returned; required unless an outputMethod is provided</param>
        /// <param name="outputMethod">An optional function to produce the output records</param>
        protected void MockRecordsAsync<T>(string fileName, string[] keys, IEnumerable<T> records, Func<string[], bool, Collection<T>> outputMethod)
            where T : class, IColleagueEntity
        {
            if (dataReaderMock == null) MockInitialize();
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (keys == null || !keys.Any()) throw new ArgumentNullException("keys");
            if (records == null && outputMethod == null) throw new ArgumentNullException("records");
            var recordList = records == null ? new List<T>() : new List<T>(records);
            if (keys.Length != recordList.Count) throw new ArgumentException("Counts of keys and records don't match.");
            var recordTypeList = recordList.OfType<T>().ToList();
            if (recordList.Any() && (recordTypeList == null || recordTypeList.Count != recordList.Count()))
                throw new ArgumentException("All records not of specified type", "records");
            var keyList = keys.ToList();

            // Form a dictionary from the keys and records
            var keyedRecords = new Dictionary<string, T>();
            for (int i = 0; i < keys.Length; i++)
            {
                keyedRecords.Add(keys[i], recordList[i]);
            }

            // Read individual records
            //dataReaderMock.Setup(r => r.ReadRecordAsync<T>(It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
            //    {
            //        int pos = keyList.FindIndex(x => x == id);
            //        if (pos >= 0)
            //        {
            //            return recordList[pos];
            //        }
            //        return null;
            //    });
            //dataReaderMock.Setup(r => r.ReadRecordAsync<T>(fileName, It.IsAny<string>(), true)).Returns<string, bool>((id, flag) =>
            //{
            //    int pos = keyList.FindIndex(x => x == id);
            //    if (pos >= 0)
            //    {
            //        return recordList[pos];
            //    }
            //    return null;
            //});

            if (outputMethod == null)
            {
                MockRecords(fileName, keyedRecords, null);
                return;
            }

            //if (outputMethod == null)
            //{ 
            //    dataReaderMock.Setup(r => r.Select(fileName, keys, It.IsAny<string>())).Returns(recordList.Select(r => r.Recordkey).ToArray());
            //    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(keys, true)).Returns(new Collection<T>(recordList));
            //    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, keys, true)).Returns(new Collection<T>(recordList));
            //}
            //else
            //{
            dataReaderMock.Setup(r => r.SelectAsync(fileName, keys, It.IsAny<string>())).Returns(Task.FromResult(outputMethod.Invoke(keys, true).Select(r => r.Recordkey).ToArray()));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(keys, It.IsAny<bool>())).Returns(Task.FromResult(outputMethod.Invoke(keys, true)));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, keys, It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T>(outputMethod.Invoke(keys, true))));
            //}

            if (typeof(T).GetInterfaces().Contains(typeof(IColleagueGuidEntity)))
            {
                //if (outputMethod == null)
                //{
                //    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), true)).Returns(new Collection<T>(recordList));
                //    dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), true)).Returns(new Collection<T>(recordList));
                //}
                //else
                //{
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T>(outputMethod.Invoke(keys, true))));
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<T>(fileName, It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).Returns(Task.FromResult(new Collection<T>(outputMethod.Invoke(keys, true))));
                //}

                //var recordLookup = recordList.Select(r => new RecordKeyLookup(fileName, r.Recordkey, string.Empty, string.Empty, false)).ToArray();
                //var lookupResult = recordList.ToDictionary
                //    (r => fileName + "+" + r.Recordkey, r => new RecordKeyLookupResult() { Guid = (r as IColleagueGuidEntity).RecordGuid });
                //dataReaderMock.Setup(reader => reader.Select(recordLookup)).Returns(lookupResult);

                //var guidLookup = recordList.Select(r => new GuidLookup((r as IColleagueGuidEntity).RecordGuid)).ToArray();
                //var guidLookupResult = recordList.ToDictionary(r => (r as IColleagueGuidEntity).RecordGuid,
                //    r => new GuidLookupResult() { Entity = fileName, PrimaryKey = r.Recordkey });
                //dataReaderMock.Setup(r => r.Select(guidLookup)).Returns(guidLookupResult);
            }
        }

        /// <summary>
        /// Setup the mocking structure for cache verification
        /// </summary>
        /// <typeparam name="T">Type of data being stored in the cache</typeparam>
        /// <param name="cacheKey">The full cache key used</param>
        /// <param name="cachedObject">The data being stored in the cache, of type T</param>
        /// <param name="existsInCache">Is the data already in the cache?</param>
        /// <param name="lockHandle">An object used for locking the cache; if null, no locking is done.</param>
        protected void MockCacheSetupAsync<T>(string cacheKey, T cachedObject, bool existsInCache = false, object lockHandle = null)
            where T : class
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");

            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(existsInCache);
            if (existsInCache)
            {
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(Task.FromResult(cachedObject)).Verifiable();
            }
            else
            {
                cacheProviderMock.Setup(x => x.GetAndLock(cacheKey, out lockHandle, null)).Returns(null);
                if (lockHandle == null)
                {
                    cacheProviderMock.Setup(x => x.Add(cacheKey, Task.FromResult(cachedObject), It.IsAny<CacheItemPolicy>(), null)).Verifiable();
                }
                else
                {
                    cacheProviderMock.Setup(x => x.AddAndUnlock(cacheKey, Task.FromResult(cachedObject), lockHandle, It.IsAny<CacheItemPolicy>(), null)).Verifiable();
                }
                cacheProviderMock.Setup(x => x.Unlock(cacheKey, lockHandle, null));
                cacheProviderMock.SetupGet(x => x[cacheKey]).Returns(cachedObject);
            }
        }

        /// <summary>
        /// Verify that the caching functioned as expected
        /// </summary>
        /// <typeparam name="T">Type of data being cached</typeparam>
        /// <param name="cacheKey">The full cache key used</param>
        /// <param name="cachedObject">The data being stored in the cache, of type T</param>
        /// <param name="existsInCache">Was the data already in the cache?</param>
        /// <param name="lockHandle">An object used for locking the cache; if null, no locking is done.</param>
        protected void VerifyCacheAsync<T>(string cacheKey, T cachedObject, bool existsInCache = false, object lockHandle = null)
            where T : class
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");

            if (existsInCache)
            {
                cacheProviderMock.Verify(m => m.Get(cacheKey, null));
            }
            else
            {
                if (lockHandle == null)
                {
                    cacheProviderMock.Verify(x => x.Add(cacheKey, Task.FromResult(cachedObject), It.IsAny<CacheItemPolicy>(), null));
                }
                else
                {
                    cacheProviderMock.Verify(x => x.AddAndUnlock(cacheKey, Task.FromResult(cachedObject), lockHandle, It.IsAny<CacheItemPolicy>(), null));
                }
            }
        }

        #endregion
    }
}
