// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Web.Http.Configuration.DataContracts;

namespace Ellucian.Web.Http.Configuration
{
    public class ExtendRepository : BaseColleagueRepository, IExtendRepository
    {
        public ExtendRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Get a collection of custom extended resource routes and versions.
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>Dictionary of 1) key: resource name  and 2) value: version number</returns>
        public Dictionary<string, string> GetEthosExtensibilityConfiguration(bool bypassCache = false)
        {
            const string ethosExtensiblityCacheKey = "AllEthosExtensibiltyRoutes";

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
            {
                ClearCache(new List<string> { ethosExtensiblityCacheKey });
            }

            Dictionary<string, string> dictValcodeItems = new Dictionary<string, string>();

            return GetOrAddToCache<Dictionary<string, string>>(ethosExtensiblityCacheKey,
                 () =>
                {
                    var edmExtVersions = DataReader.BulkReadRecord<EdmExtVersions>("EDM.EXT.VERSIONS", "");

                    if (edmExtVersions != null)
                    {
                        foreach (var row in edmExtVersions)
                        {
                            var dictKey = row.EdmvResourceName;
                            var dictValue = row.EdmvVersionNumber;
                            if (!string.IsNullOrEmpty(dictKey) && !string.IsNullOrEmpty(dictValue) && !dictValcodeItems.ContainsKey(dictKey))
                            {
                                dictValcodeItems.Add(dictKey, dictValue);
                            }
                        }
                    }
                    return dictValcodeItems;
                }, Level1CacheTimeoutValue);
        }
    }
}