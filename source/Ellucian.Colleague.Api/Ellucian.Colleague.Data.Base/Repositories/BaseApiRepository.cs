// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using slf4net;

namespace Ellucian.Data.Colleague.Repositories
{
    public abstract class BaseApiRepository : BaseColleagueRepository
    {
        public BaseApiRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        ///// <summary>
        ///// Get the record ID from a GUID
        ///// </summary>
        ///// <param name="guid">The GUID</param>
        ///// <param name="entity">The entity the GUID should be in</param>
        ///// <returns>The record ID</returns>
        //protected string GetIdFromGuid(string guid, string entity)
        //{
        //    if (string.IsNullOrEmpty(guid))
        //    {
        //        throw new ArgumentNullException("guid", "GUID is required to get an ID.");
        //    }

        //    Dictionary<string, GuidLookupResult> idLookup = DataReader.Select(new GuidLookup[] { new GuidLookup(guid) });
        //    if (idLookup == null || idLookup.Count == 0)
        //    {
        //        return null;
        //    }

        //    var result = idLookup[guid];
        //    if (result.Entity != entity)
        //    {
        //        throw new KeyNotFoundException("GUID " + guid + " not valid for " + entity);
        //    }

        //    return result.PrimaryKey;
        //}

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
            where TContract: class, IColleagueEntity
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
