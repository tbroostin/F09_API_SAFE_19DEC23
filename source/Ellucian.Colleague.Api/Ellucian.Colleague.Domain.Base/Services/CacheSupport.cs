// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using System.Threading.Tasks;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Services
{
    public static class CacheSupport
    {
        /// <summary>
        /// Build a unique cache key from a provided prefix/name and a list of criteria arguments
        /// </summary>
        /// <param name="prefix">Cache Key Prefix</param>
        /// <param name="arguments">List of arguments that effect the result set being cached</param>
        /// <returns>Cache Key string</returns>
        public static string BuildCacheKey(string prefix, params object[] arguments)
        {
            // Define the argument types and how to their values into a string
            var stringConversionFunctions = new Dictionary<Type, Func<object, string>> {
                { typeof(string), (object t) => { return t as string; } },
                { typeof(int), (object t) => { return t.ToString(); } },
                { typeof(float), (object t) => { return t.ToString(); } },
                { typeof(double), (object t) => { return t.ToString(); } },
                { typeof(DateTime), (object t) => { return t.ToString(); } },
                { typeof(bool), (object t) => { return t.ToString(); } },
                { typeof(List<string>), (object t) => { return string.Join(",", t as List<string>); } },
                { typeof(List<int>), (object t) => { return string.Join<int>(",", t as List<int>); } },
                { typeof(List<float>), (object t) => { return string.Join<float>(",", t as List<float>); } },
                { typeof(List<double>), (object t) => { return string.Join<double>(",", t as List<double>); } },
                { typeof(List<DateTime>), (object t) => { return string.Join<DateTime>(",", t as List<DateTime>); } },
                { typeof(List<bool>), (object t) => { return string.Join<bool>(",", t as List<bool>); } },
                { typeof(string[]), (object t) => { return string.Join(",", t as string[]); } },
                { typeof(int[]), (object t) => { return string.Join<int>(",", t as int[]); } },
                { typeof(float[]), (object t) => { return string.Join<float>(",", t as float[]); } },
                { typeof(double[]), (object t) => { return string.Join<double>(",", t as double[]); } },
                { typeof(DateTime[]), (object t) => { return string.Join<DateTime>(",", t as DateTime[]); } },
                { typeof(bool[]), (object t) => { return string.Join<bool>(",", t as bool[]); } },
                { typeof(List<Tuple<string, string>>), (object t) => {
                    List<Tuple<string,string>>list = t as List<Tuple<string, string>>;
                    return string.Join(",", list.Select(x => string.Format("{0}{1}", x.Item1, x.Item2)));
                } }
            };

            // Build a list of argument strings
            List<string> args = new List<string>();
            if (arguments != null && arguments.Any())
            {
                foreach (var arg in arguments)
                {
                    Func<object, string> convertArgValueToString;
                    string argValue = "";
                    if (arg != null)
                    {
                        // Each non-null argument needs to be converted into a string consisting of its data
                        // Start by getting a conversion function that matches the argument's data type
                        if (stringConversionFunctions.TryGetValue(arg.GetType(), out convertArgValueToString))
                        {
                            // If we have a conversion function, use it to convert the data value(s) into a single string
                            argValue = convertArgValueToString(arg);
                        }
                        else
                        {
                            // If we throw an Argument Exception, developers making use of this function will get an error if they use it
                            // with arguments made up of unsupported data types - which can then be added to stringConversionFunctions
                            throw new ArgumentException(string.Format("CacheSupport.BuildCacheKey must be updated to support argument type {0}", arg.GetType().ToString()));

                            // If the type is not supported, try to use the built-in ToString() function
                            //   argValue = arg.ToString();
                        }
                    }
                    args.Add(argValue);
                }
            }

            // Join all the arguments into a single string which will identify the criteria of a specific resultset
            string argumentString = string.Join(";", args);
            // If the criteria string isn't empty, we can now get a unique hash code for the criteria string
            string hashCode = (string.IsNullOrEmpty(argumentString)) ? "" : argumentString.GetHashCode().ToString();
            // We can now finalize the cache key using the provided cache prefix and the hash code of the criteria
            string cacheKey = string.Concat(prefix, ":", hashCode);

            return cacheKey;
        }

        public class KeyCacheRequirements
        {
            public List<string> limitingKeys { get; set; }
            public string criteria { get; set; }
            // When requesting the keys for an entity with no qualifying criteria or limiting keys, the CTX cant not differentiate between a request for all records,
            // or no qualifyingRecords.
            public bool NoQualifyingRecords { get; set; }
            public KeyCacheRequirements()
            {
                NoQualifyingRecords = false;
            }
        }

        const int maxKeyListSize = 100000; // max number of keys to prevent CTX serialization from blowing out memory

        public static async Task<GetCacheApiKeysResponse> GetOrAddKeyCacheToCache(
            BaseCachingRepository rep, 
            Func<string, bool> ContainsKeyFunc,
            Func<string, Func<Task<GetCacheApiKeysResponse>>, double?, Task<GetCacheApiKeysResponse>> GetOrAddToCacheAsyncFunc,
            Func<string, GetCacheApiKeysResponse, double?, Task<GetCacheApiKeysResponse>> AddOrUpdateCacheAsyncFunc,
            IColleagueTransactionInvoker transactionInvoker, 
            string cacheName, 
            string entity, 
            int offset, 
            int limit,
            double? cacheTimeout,
            Func<Task<KeyCacheRequirements>> getSelectionCriteriaFunc
            )
        {
            // if page 0 is being requested, clear any existing cache entry - allow new selection to occur
            if (offset == 0 && ContainsKeyFunc(rep.BuildFullCacheKey(cacheName)))
            {
                rep.ClearCache(new List<string> { cacheName });
            }
            // get an existing cache entry for cacheName, or call a CTX to build a new cache entry
            var remoteKeyCache = await GetOrAddToCacheAsyncFunc(
                cacheName,
                async () => {
                    GetCacheApiKeysResponse response = new GetCacheApiKeysResponse();
                    var selectionCriteria = await getSelectionCriteriaFunc();
                    // if the selectionCriteria.NoQualifyingRecords is true, then there are no results and no CTX should be called
                    if (!selectionCriteria.NoQualifyingRecords)
                    {                     

                        int requestSize = selectionCriteria.limitingKeys != null && selectionCriteria.limitingKeys.Any()? selectionCriteria.limitingKeys.Count : 0;
                        if (requestSize <= maxKeyListSize)
                        {
                            // make one call
                            var request = new GetCacheApiKeysRequest()
                            {
                                ForceCreate = true,
                                CacheMode = "",
                                CacheName = rep.BuildFullCacheKey(cacheName),
                                Entity = entity,
                                LimitList = selectionCriteria.limitingKeys,
                                Criteria = selectionCriteria.criteria,
                                Offset = offset,
                                Limit = limit
                            };
                            response = await transactionInvoker.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(request);
                        }
                        else
                        {
                            bool forceCreate = false;
                            string cacheMode = "";
                            string fullCacheName = rep.BuildFullCacheKey(cacheName);
                            int totalCount = 0;
                            List<KeyCacheInfo> keyCacheInfo = null;
                            // loop over list, building server-side key list
                            for (int skipPos = 0; skipPos < requestSize; skipPos += maxKeyListSize)
                            {
                                if (skipPos == 0)
                                {
                                    forceCreate = true;
                                    cacheMode = "CREATE";
                                } else
                                {
                                    forceCreate = false;
                                    if (skipPos + maxKeyListSize < requestSize)
                                    {
                                        cacheMode = "APPEND";
                                    } 
                                    else
                                    {
                                        cacheMode = "APPEND+EXECUTE";
                                    }
                                }
                                var request = new GetCacheApiKeysRequest()
                                {
                                    ForceCreate = forceCreate,
                                    CacheMode = cacheMode,
                                    CacheName = fullCacheName,
                                    Entity = entity,
                                    LimitList = selectionCriteria.limitingKeys.Skip(skipPos).Take(maxKeyListSize).ToList(),
                                    Criteria = selectionCriteria.criteria,
                                    Offset = offset,
                                    Limit = limit,
                                    KeyCacheInfo = keyCacheInfo,
                                    TotalCount = totalCount
                                };
                                response = await transactionInvoker.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(request);
                                if (response != null && response.KeyCacheInfo != null && response.TotalCount.HasValue)
                                {
                                    keyCacheInfo = response.KeyCacheInfo;
                                    totalCount = response.TotalCount.Value;
                                }
                            }
                        }
                    }
                    return response;
                },
                cacheTimeout);
            // if nothing was selected, return null
            if (remoteKeyCache == null || remoteKeyCache.KeyCacheInfo == null || !remoteKeyCache.KeyCacheInfo.Any() || !remoteKeyCache.TotalCount.HasValue || remoteKeyCache.TotalCount == 0)
            {
                return null;
            }
            // if the cache entry does not match the current page request, make a CTX call to get the current page of keys
            if ((remoteKeyCache.Offset != offset) || (remoteKeyCache.Limit != limit))
            {
                int firstKey = offset + 1;
                int lastKey = offset + limit;
                // determine which key partitions contain the requested keys, by position
                var keyParts = remoteKeyCache.KeyCacheInfo.Where(x => ((firstKey <= x.KeyCacheMax) && (lastKey >= x.KeyCacheMin)));
                var request = new GetCacheApiKeysRequest()
                {
                    ForceCreate = false,
                    CacheMode = "",
                    CacheName = remoteKeyCache.CacheName,
                    Entity = remoteKeyCache.Entity,
                    LimitList = remoteKeyCache.LimitList,
                    Criteria = remoteKeyCache.Criteria,
                    Offset = offset,
                    Limit = limit,
                    KeyCacheInfo = (keyParts != null) ? keyParts.ToList() : null,
                    TotalCount = remoteKeyCache.TotalCount
                };
                // call a CTX to get the requested keys
                remoteKeyCache = await transactionInvoker.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(request);
                // if the CTX rebuilt the key cache, update the cache with the new key info in case it has changed
                if (remoteKeyCache != null && remoteKeyCache.CacheBuilt)
                {
                    await AddOrUpdateCacheAsyncFunc(cacheName, remoteKeyCache, cacheTimeout);
                }
            }
            if (remoteKeyCache == null || remoteKeyCache.Sublist == null || !remoteKeyCache.Sublist.Any())
            {
                return null;
            }
            return remoteKeyCache;
        }
    }
}
