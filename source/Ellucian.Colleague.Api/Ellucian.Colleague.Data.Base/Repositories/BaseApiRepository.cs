// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Cache;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Data.Colleague.Repositories
{
    public abstract class BaseApiRepository : BaseColleagueRepository
    {
        public BaseApiRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Asynchronously reads a batch of records and logs exceptions and invalid key/record information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="physicalFileName">Name of the physical Colleague entity. Required if reading non-physical entities such as logical files, application-hierarchy files, and templates.</param>
        /// <param name="keys">String array containing the keys of the records to read.</param>
        /// <param name="replaceTextVms">If set to true, text/comment fields will replace double value marks with a newline character, and will replace single value marks with a space</param>
        /// <param name="rethrowExceptions">If set to true, any exceptions encountered during the bulk read will be logged and then re-thrown</param>
        /// <returns>Batch of records of type <typeparamref name="T"/>/>
        public async Task<List<T>> BulkReadRecordWithLoggingAsync<T>(string physicalFileName, string[] keys, int readSize = 5000, bool replaceTextVms = true, bool rethrowExceptions = true)
            where T : class, IColleagueEntity
        {
            List<T> records = new List<T>();
            BulkReadOutput<T> bulkData;
            logger.Debug(string.Format("Reading {0} file for records: {1}...", physicalFileName, string.Join(",", keys)));
            try
            {
                for (int i = 0; i < keys.Count(); i += readSize)
                {
                    var subsetOfKeys = keys.Skip(i).Take(readSize).ToArray();
                    bulkData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<T>(physicalFileName, keys, replaceTextVms);
                    if (bulkData.BulkRecordsRead == null || !bulkData.BulkRecordsRead.Any())
                    {
                        logger.Debug(string.Format("Bulk data retrieval for {0} file did not return any valid records for keys: {1}.", physicalFileName, string.Join(",", keys)));
                    }
                    if (bulkData.InvalidKeys != null && bulkData.InvalidKeys.Any())
                    {
                        logger.Debug(string.Format("Bulk data retrieval for {0} file returned one or more invalid keys: {1}", physicalFileName, string.Join(",", bulkData.InvalidKeys)));
                    }
                    if (bulkData.InvalidRecords != null && bulkData.InvalidRecords.Any())
                    {
                        string invalidRecordMessages = string.Format("Bulk data retrieval for {0} file returned one or more invalid records:", physicalFileName);
                        foreach (var invalidRecord in bulkData.InvalidRecords)
                        {
                            invalidRecordMessages += Environment.NewLine + string.Format("Record: {0}, Message: '{1}'", invalidRecord.Key, invalidRecord.Value);
                        }
                        logger.Debug(invalidRecordMessages);
                    }
                    if (bulkData.BulkRecordsRead != null && bulkData.BulkRecordsRead.Any())
                    {
                        records.AddRange(bulkData.BulkRecordsRead);
                    }
                }
            }
            catch (Exceptions.ColleagueSessionExpiredException)
            {
                throw;
            }

            catch (Exception ex)
            {
                logger.Error(ex, "An exception occurred while reading the {0} file.");
                if (rethrowExceptions)
                {
                    throw ex;
                }
            }

            return records;
        }

        /// <summary>
        /// Get the GUID for a specified entity, primary key, and optional secondary field and key
        /// </summary>
        /// <param name="entity">The name of the entity</param>
        /// <param name="primaryKey">The value of the primary key</param>
        /// <param name="secondaryField">The CDD name of the secondary field</param>
        /// <param name="secondaryKey">The value of the secondary key</param>
        /// <returns>The corresponding GUID</returns>
        protected string GetGuidFromEntityId(string entity, string primaryKey, string secondaryField = "", string secondaryKey = "")
        {
            if (string.IsNullOrEmpty(entity))
            {
                throw new ArgumentNullException("entity", "Entity is a required argument.");
            }
            if (string.IsNullOrEmpty(primaryKey))
            {
                throw new ArgumentNullException("primaryKey", "The primary key is a required argument.");
            }

            var lookup = new RecordKeyLookup(entity, primaryKey, secondaryField, secondaryKey, false);
            var result = DataReader.Select(new RecordKeyLookup[] { lookup });
            var lookupResult = result[lookup.ResultKey];

            return lookupResult.Guid;
        }

        /// <summary>
        /// Get a domain entity using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The domain entity</returns>
        public TDomain GetDomainEntityByGuid<TContract, TDomain>(string guid, Func<TContract, TDomain> buildMethod)
            where TContract : class, IColleagueEntity
            where TDomain : class
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID must be provided.");
            }

            TContract contract = DataReader.ReadRecord<TContract>(new GuidLookup(guid, null));

            if (contract == null)
            {
                throw new KeyNotFoundException("Record not found, invalid GUID provided: " + guid);
            }

            // Now we have a record, so we can pass of the rest of the work to another routine
            TDomain entity = buildMethod.Invoke(contract);

            return entity;
        }

        public static string QuoteDelimit(IEnumerable<string> stringList)
        {
            if (stringList == null || stringList.Select(i => (!string.IsNullOrEmpty(i))).Count() == 0)
            {
                return null;
            }

            return "'" + (string.Join(" ", stringList.ToArray())).Replace(" ", "' '") + "'";
        }

    }
}
